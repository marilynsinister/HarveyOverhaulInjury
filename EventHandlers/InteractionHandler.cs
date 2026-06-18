using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.Core.Services;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Services;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик взаимодействий с Харви (клики). Старт лечения/осложнений — только CP $action;
    /// фазовые переходы и выздоровление — programmatic диалог → закрытие DialogueBox.
    /// </summary>
    public class InteractionHandler
    {
        private const int PendingMedicalTimeoutTicks = 1800;

        private enum MedicalActionType
        {
            None,
            StartTreatment,
            TreatComplications,
            AdvancePhase,
            CompleteRecovery,
            SimpleCompletionTopic
        }

        private sealed class PendingMedicalAction
        {
            public MedicalActionType Type { get; set; }
            public string? InjuryId { get; set; }
            public List<string> Complications { get; set; } = new();
            public string? TopicId { get; set; }
            public int StartedTick { get; set; }
            public bool DialogueWasShown { get; set; }
            public bool Applied { get; set; }
        }

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;
        private readonly TreatmentManager _treatmentManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly ComplianceManager _complianceManager;
        private readonly CareTrustManager _careTrustManager;
        private readonly PrescriptionManager _prescriptionManager;
        private readonly CheckupManager _checkupManager;
        private readonly RehabManager _rehabManager;
        private readonly SelfCareManager _selfCareManager;
        private readonly ComplicationManager _complicationManager;
        private readonly DoctorVisitReminderManager _doctorVisitReminderManager;
        private readonly RecoveryPlanManager _recoveryPlanManager;
        private readonly TreatmentStartHandler _treatmentStartHandler;
        private readonly InjuryMedicalIntentProvider _medicalIntentProvider;
        private readonly HiddenInjuryDialogueFlow _hiddenInjuryDialogueFlow;

        private PendingMedicalAction? _pendingMedicalAction;
        private bool _pendingSawDialogueBox;

        /// <summary>Последний результат проверок при клике (для дебаг-HUD).</summary>
        public string? LastClickDebug { get; private set; }

        /// <summary>Краткое описание текущего pending medical action для HUD.</summary>
        public string? GetPendingMedicalActionSummary()
        {
            if (_pendingMedicalAction is not { Applied: false } pending)
                return null;

            return FormatMedicalActionLabel(pending);
        }

        /// <summary>
        /// Почему vanilla-диалог с Харви разрешён или заблокирован (без клика — текущее состояние).
        /// </summary>
        public string GetStandardDialogueGateReason()
        {
            if (_pendingMedicalAction is { Applied: false })
                return "BLOCKED: pending medical action in progress";

            if (_hospitalizationManager.IsHospitalized)
                return "ALLOWED: hospitalized — medical pipeline idle";

            _injuryManager.EnsureActiveTreatmentBuffs();
            var resolved = TryResolveMedicalAction(_injuryManager.CollectAllInjuries());
            if (resolved != null)
                return $"BLOCKED: InjuryCare medical action ({FormatMedicalActionLabel(resolved)})";

            return "ALLOWED: no InjuryCare medical action for HarveyTreatable DebuffState";
        }

        public InteractionHandler(
            IMonitor monitor,
            IModHelper helper,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager,
            TreatmentManager treatmentManager,
            HospitalizationManager hospitalizationManager,
            ComplianceManager complianceManager,
            CareTrustManager careTrustManager,
            PrescriptionManager prescriptionManager,
            CheckupManager checkupManager,
            RehabManager rehabManager,
            SelfCareManager selfCareManager,
            ComplicationManager complicationManager,
            DoctorVisitReminderManager doctorVisitReminderManager,
            RecoveryPlanManager recoveryPlanManager,
            TreatmentStartHandler treatmentStartHandler,
            InjuryMedicalIntentProvider medicalIntentProvider,
            HiddenInjuryDialogueFlow hiddenInjuryDialogueFlow)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
            _treatmentManager = treatmentManager;
            _hospitalizationManager = hospitalizationManager;
            _complianceManager = complianceManager;
            _careTrustManager = careTrustManager;
            _prescriptionManager = prescriptionManager;
            _checkupManager = checkupManager;
            _rehabManager = rehabManager;
            _selfCareManager = selfCareManager;
            _complicationManager = complicationManager;
            _doctorVisitReminderManager = doctorVisitReminderManager;
            _recoveryPlanManager = recoveryPlanManager;
            _treatmentStartHandler = treatmentStartHandler;
            _medicalIntentProvider = medicalIntentProvider;
            _hiddenInjuryDialogueFlow = hiddenInjuryDialogueFlow;
        }

        /// <summary>
        /// Применить отложенное медицинское действие после закрытия DialogueBox.
        /// </summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            _recoveryPlanManager.OnCompletionTalkDialogueUpdate();

            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                if (_pendingMedicalAction != null)
                {
                    _monitor.Log(
                        "[MedicalAction] сброс pending: активно игровое событие",
                        LogLevel.Debug);
                    ClearPendingMedicalAction();
                }

                return;
            }

            if (_pendingMedicalAction == null)
                return;

            var pending = _pendingMedicalAction;

            if (pending.Type is MedicalActionType.StartTreatment or MedicalActionType.TreatComplications)
            {
                HandleActionOnlyPendingCleanup(pending);
                return;
            }

            if (pending.Applied)
            {
                ClearPendingMedicalAction();
                return;
            }

            int elapsed = Game1.ticks - pending.StartedTick;
            if (elapsed > PendingMedicalTimeoutTicks)
            {
                _monitor.Log(
                    $"⚠️ Медицинское действие {pending.Type} ({pending.InjuryId ?? pending.TopicId}) " +
                    $"не завершилось за {PendingMedicalTimeoutTicks} тиков — финальная попытка apply.",
                    LogLevel.Warn);

                if (pending.DialogueWasShown
                    && _pendingSawDialogueBox
                    && CanApplyPendingMedicalAfterDialogue())
                {
                    TryApplyAndClearPendingMedicalAction(pending);
                }
                else
                {
                    ClearPendingMedicalAction();
                }

                return;
            }

            if (Game1.activeClickableMenu is DialogueBox)
            {
                _pendingSawDialogueBox = true;
                return;
            }

            if (!pending.DialogueWasShown)
                return;

            // Диалог мог закрыться до первого тика или без DialogueBox — не блокируем механику.
            if (!_pendingSawDialogueBox && elapsed >= 60)
                _pendingSawDialogueBox = true;

            if (!_pendingSawDialogueBox)
                return;

            if (!CanApplyPendingMedicalAfterDialogue())
                return;

            TryApplyAndClearPendingMedicalAction(pending);
        }

        /// <summary>
        /// После закрытия DialogueBox не требуем полный IsPlayerFree — игрок часто кратко «занят» анимацией.
        /// </summary>
        private static bool CanApplyPendingMedicalAfterDialogue()
        {
            if (!Context.IsWorldReady)
                return false;

            if (Game1.eventUp || Game1.CurrentEvent != null)
                return false;

            if (Game1.activeClickableMenu is DialogueBox)
                return false;

            return true;
        }

        private void TryApplyAndClearPendingMedicalAction(PendingMedicalAction pending)
        {
            try
            {
                bool applied = ApplyPendingMedicalAction(pending);
                pending.Applied = applied;
                if (!applied)
                {
                    _monitor.Log(
                        $"[MedicalAction] ⚠️ pending {pending.Type} ({pending.InjuryId ?? pending.TopicId}) " +
                        "не применён — состояние изменилось после диалога",
                        LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка ApplyPendingMedicalAction ({pending.Type}): {ex}", LogLevel.Error);
            }
            finally
            {
                ClearPendingMedicalAction();
            }
        }

        /// <summary>
        /// Обработать нажатие кнопки
        /// </summary>
        public void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return;
            if (Game1.eventUp || Game1.CurrentEvent != null) return;
            if (!e.Button.IsActionButton()) return;
            if (Game1.activeClickableMenu is DialogueBox) return;

            var loc = Game1.currentLocation;
            if (loc == null)
            {
                LastClickDebug = "Клик: нет currentLocation";
                return;
            }

            var tile = _helper.Input.GetCursorPosition().GrabTile;
            var harvey = HarveyHelper.TryGetInteractedHarvey(loc, tile, lenientDistance: true);

            if (harvey == null)
            {
                LastClickDebug = $"Клик: Action GrabTile({tile.X:F0},{tile.Y:F0}) — не Харви";
                return;
            }

            _stateManager.SanitizeNonPhasedReadyFlags();
            _injuryManager.EnsureActiveTreatmentBuffs();
            var clickResolution = _medicalIntentProvider.SyncOnHarveyClick(logDetails: true);

            bool deferHiddenForMedicalIntent = ShouldDeferHiddenInjuryForMedicalIntent(clickResolution);
            bool flowStarted = !deferHiddenForMedicalIntent
                && _hiddenInjuryDialogueFlow.TryStartDetection(
                    harvey,
                    isDirectTalk: true,
                    isProximityCheck: false,
                    triggerReason: "talk");

            if (deferHiddenForMedicalIntent)
            {
                _monitor.Log(
                    "[HiddenInjuryFlow] skipped: selected medical intent or treatable complication has priority",
                    LogLevel.Info);
            }

            if (flowStarted)
            {
                _helper.Input.Suppress(e.Button);
                _helper.Input.Suppress(SButton.MouseLeft);
                _helper.Input.Suppress(SButton.MouseRight);
                LastClickDebug = "HIDDEN INJURY FLOW: question dialogue shown — treatment only after player choice";
                return;
            }

            if (TryBeginSelectedInjuryMedicalIntentDialogue(harvey, clickResolution, e))
                return;

            LastClickDebug = BuildClickDebugSnapshot(
                null,
                null,
                "ALLOWED: vanilla/CP dialogue — treatment only via $action or hidden-injury choice");
        }

        /// <summary>
        /// Фазовый переход и выписка — programmatic диалог (важно дома у супруга: MarriageDialogueHarvey не содержит PhaseTransition_*).
        /// </summary>
        private bool TryBeginSelectedInjuryMedicalIntentDialogue(
            NPC harvey,
            HarveyMedicalIntentResolution? clickResolution,
            ButtonPressedEventArgs e)
        {
            var selected = clickResolution?.Selected;
            if (selected == null
                || !string.Equals(selected.ProviderId, HarveyProviderRegistry.InjuryProviderId, StringComparison.Ordinal))
            {
                return false;
            }

            bool started = selected.ActionKey switch
            {
                TreatmentStartActions.AdvancePhase => TryBeginAdvancePhase(harvey, selected.StateId),
                TreatmentStartActions.CompleteRecovery => TryBeginCompleteRecovery(harvey, selected.StateId),
                _ => false,
            };

            if (!started)
                return false;

            SuppressHarveyClickButtons(e);
            LastClickDebug = BuildClickDebugSnapshot(
                _pendingMedicalAction,
                _pendingMedicalAction,
                $"BLOCKED: programmatic {selected.ActionKey} {selected.StateId}");
            _monitor.Log(
                $"[MedicalAction] programmatic dialogue started action={selected.ActionKey} injury={selected.StateId} " +
                $"topic={selected.TopicKey}",
                LogLevel.Info);
            return true;
        }

        private bool ShouldDeferHiddenInjuryForMedicalIntent(HarveyMedicalIntentResolution? clickResolution)
        {
            if (_complicationManager.GetActiveTreatableComplicationIds().Count > 0)
                return true;

            return clickResolution?.Selected?.Kind == HarveyMedicalIntentKind.Complication;
        }

        private void SuppressHarveyClickButtons(ButtonPressedEventArgs e)
        {
            _helper.Input.Suppress(e.Button);
            _helper.Input.Suppress(SButton.MouseLeft);
            _helper.Input.Suppress(SButton.MouseRight);
        }

        /// <summary>
        /// Диагностика: какой шаг лечения сработал бы при клике по Харви (без побочных эффектов).
        /// </summary>
        public string BuildDebugTreatmentDecision()
        {
            if (!Context.IsWorldReady)
                return "not world ready";

            if (_pendingMedicalAction is { Applied: false } pending)
                return $"BLOCKED pending={FormatMedicalActionLabel(pending)}";

            if (_hospitalizationManager.IsHospitalized)
                return "none (hospitalized — pipeline idle)";

            _injuryManager.EnsureActiveTreatmentBuffs();
            var resolved = TryResolveMedicalAction(_injuryManager.CollectAllInjuries());
            if (resolved == null)
            {
                var untreated = GetHarveyTreatableInjuryStates()
                    .Where(d => !d.TreatmentStarted)
                    .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                    .FirstOrDefault();
                if (untreated != null)
                    return $"none (untreated {untreated.BuffId} — CP topic {TopicIds.GetTreatmentNeededTopic(untreated.BuffId)})";

                return "none (standard dialogue allowed)";
            }

            return $"SELECT {FormatMedicalActionLabel(resolved)}";
        }

        /// <summary>
        /// DEBUG: применить медицинское действие как после клика по Харви и закрытия диалога (без UI).
        /// </summary>
        public string TryDebugApplyHarveyMedicalAction(bool dryRun = false, bool ignoreHospital = false)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (_pendingMedicalAction is { Applied: false } pending)
                return $"BLOCKED: pending {FormatMedicalActionLabel(pending)}";

            if (_hospitalizationManager.IsHospitalized && !ignoreHospital)
                return "BLOCKED: player hospitalized (pipeline idle); use ignore_hospital or injury_hospital_discharge";

            _stateManager.SanitizeNonPhasedReadyFlags();
            _injuryManager.EnsureActiveTreatmentBuffs();

            var resolved = TryResolveMedicalAction(_injuryManager.CollectAllInjuries());
            if (resolved == null)
            {
                var untreated = GetHarveyTreatableInjuryStates()
                    .Where(d => !d.TreatmentStarted)
                    .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                    .FirstOrDefault();

                if (untreated != null)
                {
                    string label = $"StartTreatment:{untreated.BuffId}";
                    if (dryRun)
                    {
                        LastClickDebug = BuildClickDebugSnapshot(null, null, $"DRY_RUN: {label} (via TreatmentStartHandler)");
                        return $"DRY_RUN: {label}";
                    }

                    bool applied = _treatmentStartHandler.TryStartTreatment(
                        untreated.BuffId,
                        fromDialogueAction: false,
                        out string? skipReason);
                    LastClickDebug = BuildClickDebugSnapshot(
                        null,
                        null,
                        applied ? $"DEBUG: applied {label}" : $"DEBUG: skipped {label} ({skipReason})");

                    return applied
                        ? $"APPLIED: {label}"
                        : $"SKIPPED: {label} ({skipReason ?? "unknown"})";
                }

                var activeComplications = GetActiveComplicationIds();
                if (activeComplications.Count > 0)
                {
                    string complicationId = activeComplications[0];
                    string label = $"TreatComplication:{complicationId}";
                    if (dryRun)
                        return $"DRY_RUN: {label}";

                    bool applied = _treatmentStartHandler.TryTreatComplication(
                        complicationId,
                        fromDialogueAction: false,
                        out string? skipReason);
                    return applied
                        ? $"APPLIED: {label}"
                        : $"SKIPPED: {label} ({skipReason ?? "unknown"})";
                }

                LastClickDebug = "DEBUG: no InjuryCare medical action";
                return "NO_ACTION: standard Harvey dialogue would apply";
            }

            LogResolvedMedicalAction(resolved);

            if (dryRun)
            {
                LastClickDebug = BuildClickDebugSnapshot(null, resolved, "DRY_RUN: would apply");
                return $"DRY_RUN: {FormatMedicalActionLabel(resolved)}";
            }

            try
            {
                bool applied = ApplyPendingMedicalAction(resolved);
                LastClickDebug = BuildClickDebugSnapshot(
                    null,
                    resolved,
                    applied ? "DEBUG: applied without dialogue" : "DEBUG: apply skipped (stale state)");

                return applied
                    ? $"APPLIED: {FormatMedicalActionLabel(resolved)}"
                    : $"SKIPPED: {FormatMedicalActionLabel(resolved)} (state changed or already done)";
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MedicalAction] DEBUG apply error ({resolved.Type}): {ex}", LogLevel.Error);
                return $"Error: {ex.Message}";
            }
        }

        private string BuildClickDebugSnapshot(
            PendingMedicalAction? pending,
            PendingMedicalAction? selected,
            string standardDialogueGate)
        {
            string pendingLine = pending != null
                ? FormatMedicalActionLabel(pending)
                : GetPendingMedicalActionSummary() ?? "none";
            string selectedLine = selected != null
                ? FormatMedicalActionLabel(selected)
                : "none";
            return $"pending={pendingLine} | selected={selectedLine} | standard={standardDialogueGate}";
        }

        private static string FormatMedicalActionLabel(PendingMedicalAction action)
        {
            string target = action.InjuryId ?? action.TopicId
                ?? (action.Complications.Count > 0 ? string.Join("+", action.Complications) : "-");
            return $"{action.Type}:{target}";
        }

        private string BuildMedicalClickDebug(PendingMedicalAction pending)
        {
            return BuildClickDebugSnapshot(pending, pending, "BLOCKED: InjuryCare medical dialogue");
        }

        private PendingMedicalAction? TryResolveMedicalAction(InjuryCollection injuries)
        {
            // Лечение, фазы и выздоровление — только через CP topic + $action (Core arbitration).
            return null;
        }

        /// <summary>Лог только при реальном клике по Харви (не при опросе HUD/debug).</summary>
        private void LogResolvedMedicalAction(PendingMedicalAction resolved)
        {
            string message = resolved.Type switch
            {
                MedicalActionType.CompleteRecovery =>
                    $"[MedicalAction] клик → CompleteRecovery {resolved.InjuryId}",
                MedicalActionType.AdvancePhase => FormatPhaseResolveLog(resolved.InjuryId!),
                MedicalActionType.StartTreatment =>
                    $"[MedicalAction] клик → StartTreatment {resolved.InjuryId}",
                MedicalActionType.TreatComplications =>
                    $"[MedicalAction] клик → TreatComplications {string.Join(", ", resolved.Complications)}",
                MedicalActionType.SimpleCompletionTopic =>
                    $"[MedicalAction] клик → SimpleCompletionTopic {resolved.TopicId}",
                _ => $"[MedicalAction] клик → {resolved.Type}"
            };

            _monitor.Log(message, LogLevel.Info);
        }

        private string FormatPhaseResolveLog(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            int currentPhase = debuffState?.CurrentPhase ?? 0;
            return $"[MedicalAction] клик → AdvancePhase {injuryId} {currentPhase}→{currentPhase + 1}";
        }

        private IEnumerable<DebuffState> GetHarveyTreatableInjuryStates()
        {
            return _stateManager.GetAllActiveDebuffStates()
                .Where(d => InjurySets.HarveyTreatable.Contains(d.BuffId));
        }

        private List<string> GetActiveComplicationIds() =>
            _complicationManager.GetActiveTreatableComplicationIds().ToList();

        /// <summary>
        /// Запустить медицинский диалог по resolved action. Механика — только после закрытия DialogueBox.
        /// </summary>
        private bool BeginMedicalActionDialogue(NPC harvey, PendingMedicalAction pending)
        {
            return pending.Type switch
            {
                MedicalActionType.AdvancePhase => TryBeginAdvancePhase(harvey, pending.InjuryId!),
                MedicalActionType.CompleteRecovery => TryBeginCompleteRecovery(harvey, pending.InjuryId!),
                MedicalActionType.TreatComplications
                    or MedicalActionType.SimpleCompletionTopic => BeginMedicalDialogue(
                        harvey,
                        pending.Type,
                        pending.InjuryId,
                        pending.Complications,
                        pending.TopicId),
                _ => false
            };
        }

        /// <summary>
        /// Legacy wrapper: только pending + PhaseTransition-диалог. Смена фазы — в ApplyPendingMedicalAction.
        /// </summary>
        private void AdvanceToNextPhase(NPC harvey, string injuryId, DebuffState debuffState)
        {
            if (_pendingMedicalAction is { Applied: false })
            {
                _monitor.Log("[MedicalAction] AdvanceToNextPhase пропущен: уже есть pending", LogLevel.Warn);
                return;
            }

            if (!debuffState.IsPhasedInjury || debuffState.CurrentPhase >= debuffState.TotalPhases)
            {
                _monitor.Log(
                    $"[MedicalAction] AdvanceToNextPhase пропущен: {injuryId} не фазовая или уже на последней фазе",
                    LogLevel.Warn);
                return;
            }

            if (!debuffState.ReadyForNextPhase)
            {
                _monitor.Log(
                    $"[MedicalAction] AdvanceToNextPhase пропущен: {injuryId} ReadyForNextPhase=false",
                    LogLevel.Warn);
                return;
            }

            BeginMedicalDialogue(harvey, MedicalActionType.AdvancePhase, injuryId);
        }

        /// <summary>
        /// Legacy wrapper: только pending + диалог выздоровления. Баффы/state/Care — в ApplyPendingMedicalAction.
        /// topic*Cured для фазовой травмы не создаётся.
        /// </summary>
        private void CompleteRecovery(NPC harvey, string injuryId)
        {
            if (_pendingMedicalAction is { Applied: false })
            {
                _monitor.Log("[MedicalAction] CompleteRecovery пропущен: уже есть pending", LogLevel.Warn);
                return;
            }

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                _monitor.Log(
                    $"[MedicalAction] CompleteRecovery пропущен: состояние {injuryId} не найдено",
                    LogLevel.Warn);
                return;
            }

            if (!debuffState.ReadyForRecovery)
            {
                _monitor.Log(
                    $"[MedicalAction] CompleteRecovery пропущен: {injuryId} ReadyForRecovery=false",
                    LogLevel.Warn);
                return;
            }

            BeginMedicalDialogue(harvey, MedicalActionType.CompleteRecovery, injuryId);
        }

        private bool TryBeginAdvancePhase(NPC harvey, string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null
                || !debuffState.IsPhasedInjury
                || !debuffState.ReadyForNextPhase
                || debuffState.CurrentPhase <= 0
                || debuffState.CurrentPhase >= debuffState.TotalPhases)
                return false;

            AdvanceToNextPhase(harvey, injuryId, debuffState);
            return _pendingMedicalAction != null;
        }

        private bool TryBeginCompleteRecovery(NPC harvey, string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null || !debuffState.ReadyForRecovery)
                return false;

            CompleteRecovery(harvey, injuryId);
            return _pendingMedicalAction != null;
        }

        /// <summary>
        /// Legacy wrapper для проверки готовности фазы/выздоровления и запуска диалога без механики.
        /// </summary>
        private bool CheckAndHandlePhaseTransition(NPC harvey, string injuryId, DebuffState debuffState)
        {
            if (debuffState.TotalPhases <= 0)
                return false;

            if (debuffState.IsLastPhase && debuffState.ReadyForRecovery)
            {
                CompleteRecovery(harvey, injuryId);
                return _pendingMedicalAction != null;
            }

            if (debuffState.ReadyForNextPhase
                && debuffState.CurrentPhase > 0
                && debuffState.CurrentPhase < debuffState.TotalPhases)
            {
                AdvanceToNextPhase(harvey, injuryId, debuffState);
                return _pendingMedicalAction != null;
            }

            return false;
        }

        private bool BeginMedicalDialogue(
            NPC harvey,
            MedicalActionType type,
            string? injuryId,
            List<string>? complications = null,
            string? topicId = null)
        {
            if (_pendingMedicalAction is { Applied: false })
            {
                _monitor.Log($"[MedicalAction] BeginMedicalDialogue пропущен: уже есть pending ({type})", LogLevel.Warn);
                return false;
            }

            var pending = new PendingMedicalAction
            {
                Type = type,
                InjuryId = injuryId,
                Complications = complications != null ? new List<string>(complications) : new List<string>(),
                TopicId = topicId,
                StartedTick = Game1.ticks,
                DialogueWasShown = false,
                Applied = false
            };

            _pendingMedicalAction = pending;
            LogMedicalQueued(pending);

            if (type is MedicalActionType.AdvancePhase
                or MedicalActionType.CompleteRecovery)
            {
                _complianceManager.ApplyTreatmentComplianceTopics();
            }

            string dialogueKey;
            string dialogueText = type switch
            {
                MedicalActionType.StartTreatment
                    => BuildTreatmentDialogueText(pending, out dialogueKey),
                MedicalActionType.TreatComplications
                    => BuildComplicationTreatmentDialogueText(pending, out dialogueKey),
                MedicalActionType.AdvancePhase
                    => BuildAdvancePhaseDialogue(injuryId!, out dialogueKey),
                MedicalActionType.CompleteRecovery
                    => BuildCompleteRecoveryDialogue(injuryId!, out dialogueKey),
                MedicalActionType.SimpleCompletionTopic
                    => BuildSimpleCompletionDialogueText(topicId!, out dialogueKey),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            if (type is MedicalActionType.AdvancePhase or MedicalActionType.CompleteRecovery
                or MedicalActionType.SimpleCompletionTopic)
            {
                ShowMedicalEmote(harvey, type);
            }

            _dialogueManager.Speak(harvey, dialogueText);

            pending.DialogueWasShown = true;
            _pendingSawDialogueBox = Game1.activeClickableMenu is DialogueBox;
            if (!_pendingSawDialogueBox)
            {
                _monitor.Log(
                    "[MedicalAction] DialogueBox не открыт после Speak — механика применится после закрытия/таймаута",
                    LogLevel.Debug);
            }

            _monitor.Log($"[MedicalAction] dialogue shown key/prefix={dialogueKey}", LogLevel.Info);
            return true;
        }

        private InjuryCollection BuildInjuryContextForDialogue(PendingMedicalAction pending)
        {
            var injuries = _injuryManager.CollectAllInjuries();
            if (pending.Type == MedicalActionType.TreatComplications)
            {
                injuries.MainInjury = null;
                injuries.Complications = new List<string>(pending.Complications);
            }
            else
            {
                injuries.MainInjury = pending.InjuryId;
            }

            return injuries;
        }

        private string BuildTreatmentDialogueText(PendingMedicalAction pending, out string dialogueKey)
        {
            var injuries = BuildInjuryContextForDialogue(pending);
            string dialogue;

            if (pending.Type == MedicalActionType.StartTreatment
                && TryPickCareTrustDialogue("TreatmentStart", out string careTrustLine, out string careTrustPrefix))
            {
                dialogue = _treatmentManager.BuildFirstStartTreatmentDialogue(injuries, careTrustLine);
                dialogueKey = pending.Complications.Count > 0
                    ? $"{careTrustPrefix}+Proximity_*"
                    : $"{careTrustPrefix}*";
                return dialogue;
            }

            dialogue = pending.Type == MedicalActionType.StartTreatment
                ? _treatmentManager.BuildFirstStartTreatmentDialogue(injuries)
                : _treatmentManager.BuildCombinedDialogue(injuries, markTreatmentDiscussed: false);

            if (string.IsNullOrWhiteSpace(dialogue))
            {
                dialogue = pending.Type switch
                {
                    MedicalActionType.TreatComplications =>
                        "Я вижу осложнение — сейчас осмотрю рану и обработаю повязку.$a",
                    MedicalActionType.StartTreatment => DialogueManager.FirstTreatmentStartFallback,
                    _ => "Сейчас займусь лечением.$u"
                };
            }

            if (pending.InjuryId != null)
            {
                string injuryName = pending.InjuryId.Replace("buff", "");
                if (pending.Type == MedicalActionType.StartTreatment)
                {
                    string startPrefix = DialogueManager.GetTreatmentStartDialoguePrefix(pending.InjuryId);
                    dialogueKey = pending.Complications.Count > 0
                        ? $"{startPrefix}+Proximity_*"
                        : $"{startPrefix}*";
                }
                else
                {
                    bool useBefore = !(_stateManager.GetDebuffState(pending.InjuryId)?.HarveyConversationHappened ?? false);
                    string treatPrefix = useBefore ? $"Treat_{injuryName}_Before" : $"Treat_{injuryName}_After";
                    dialogueKey = pending.Complications.Count > 0
                        ? $"{treatPrefix}+Proximity_*"
                        : treatPrefix;
                }
            }
            else
            {
                dialogueKey = pending.Complications.Count > 0
                    ? "Proximity_*"
                    : "Treat_*";
            }

            return dialogue;
        }

        private string BuildComplicationTreatmentDialogueText(PendingMedicalAction pending, out string dialogueKey)
        {
            var complications = pending.Complications;
            string dialogue = _treatmentManager.BuildComplicationTreatmentDialogue(complications);

            if (string.IsNullOrWhiteSpace(dialogue))
                dialogue = DialogueManager.ComplicationTreatmentFallback;

            var prefixes = complications
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(DialogueManager.GetComplicationTreatmentDialoguePrefix)
                .ToList();
            dialogueKey = prefixes.Count > 0
                ? string.Join("+", prefixes) + "*"
                : "ComplicationTreatment_*";

            return dialogue;
        }

        private string BuildAdvancePhaseDialogue(string injuryId, out string dialogueKey)
        {
            if (TryPickCareTrustDialogue("PhaseAdvance", out string careTrustLine, out string careTrustPrefix))
            {
                dialogueKey = $"{careTrustPrefix}*";
                return careTrustLine;
            }

            var debuffState = _stateManager.GetDebuffState(injuryId);
            int currentPhase = debuffState?.CurrentPhase ?? 0;
            int nextPhase = currentPhase + 1;
            dialogueKey = $"{DialogueManager.GetPhaseTransitionDialoguePrefix(injuryId, nextPhase)}*";

            return _dialogueManager.PickPhaseTransitionDialogue(injuryId, nextPhase);
        }

        private string BuildCompleteRecoveryDialogue(string injuryId, out string dialogueKey)
        {
            if (TryPickCareTrustDialogue("Recovery", out string careTrustLine, out string careTrustPrefix))
            {
                dialogueKey = $"{careTrustPrefix}*";
                return careTrustLine;
            }

            dialogueKey = $"{DialogueManager.GetRecoveryCompleteDialoguePrefix(injuryId)}*";
            return _dialogueManager.PickRecoveryCompleteDialogue(injuryId);
        }

        private bool TryPickCareTrustDialogue(string scenario, out string line, out string prefix)
        {
            if (!_dialogueManager.TryPickCareTrustDialogue(
                    scenario,
                    _careTrustManager.GetLevelSuffix(),
                    out line,
                    out prefix))
            {
                line = string.Empty;
                return false;
            }

            _monitor.Log(
                $"[CareTrust] {scenario} trust={_careTrustManager.GetLevelSuffix()} " +
                $"relationship={_dialogueManager.GetCareTrustRelationshipLevel()} prefix={prefix}",
                LogLevel.Debug);
            return true;
        }

        private string BuildSimpleCompletionDialogueText(string topicId, out string dialogueKey)
        {
            dialogueKey = topicId;
            return _dialogueManager.PickRandomDialogueByPrefix(
                topicId,
                "Отлично! Ты полностью выздоровела. Я горжусь тобой за то, что ты следовала всем моим рекомендациям. Береги себя!$h");
        }

        private void ShowMedicalEmote(NPC harvey, MedicalActionType type)
        {
            int emote = type switch
            {
                MedicalActionType.AdvancePhase or MedicalActionType.CompleteRecovery
                    or MedicalActionType.SimpleCompletionTopic
                    => HarveyHelper.GetRecoveryEmote(),
                _ => HarveyHelper.GetCaringEmote()
            };

            _dialogueManager.ShowEmote(harvey, emote);
        }

        private void LogMedicalQueued(PendingMedicalAction pending)
        {
            string message = pending.Type switch
            {
                MedicalActionType.AdvancePhase => FormatPhaseQueuedLog(pending),
                MedicalActionType.CompleteRecovery =>
                    $"[MedicalAction] queued type=CompleteRecovery injury={pending.InjuryId}",
                MedicalActionType.StartTreatment =>
                    $"[MedicalAction] queued type=StartTreatment injury={pending.InjuryId}",
                MedicalActionType.TreatComplications =>
                    $"[MedicalAction] queued type=TreatComplications complications={string.Join(", ", pending.Complications)}",
                MedicalActionType.SimpleCompletionTopic =>
                    $"[MedicalAction] queued type=SimpleCompletionTopic topic={pending.TopicId}",
                _ => $"[MedicalAction] queued type={pending.Type}"
            };

            _monitor.Log(message, LogLevel.Info);
        }

        private string FormatPhaseQueuedLog(PendingMedicalAction pending)
        {
            var debuffState = _stateManager.GetDebuffState(pending.InjuryId!);
            int currentPhase = debuffState?.CurrentPhase ?? 0;
            int totalPhases = debuffState?.TotalPhases ?? InjurySets.InferDefaultTotalPhases(pending.InjuryId!);
            int nextPhase = currentPhase + 1;
            string stage = TopicIds.GetPhaseStageName(nextPhase, totalPhases);
            return $"[MedicalAction] queued type=AdvancePhase injury={pending.InjuryId} phase={currentPhase}->{nextPhase} stage={stage}";
        }

        private bool ApplyPendingMedicalAction(PendingMedicalAction action)
        {
            return action.Type switch
            {
                MedicalActionType.StartTreatment or MedicalActionType.TreatComplications
                    => LogActionOnlyPendingStale(action),
                MedicalActionType.AdvancePhase => ApplyPendingAdvancePhase(action),
                MedicalActionType.CompleteRecovery => ApplyPendingCompleteRecovery(action),
                MedicalActionType.SimpleCompletionTopic => ApplyPendingSimpleCompletionTopic(action),
                _ => false
            };
        }

        private bool ApplyPendingStartTreatment(PendingMedicalAction action)
        {
            if (action.InjuryId == null)
            {
                LogStaleApply(action, "InjuryId отсутствует");
                return false;
            }

            return _treatmentStartHandler.TryStartTreatment(
                action.InjuryId,
                fromDialogueAction: false,
                out string? skipReason)
                || string.Equals(skipReason, "already started", StringComparison.OrdinalIgnoreCase);
        }

        private bool ApplyPendingTreatComplications(PendingMedicalAction action)
        {
            var stillActive = GetActiveComplicationIds();
            if (stillActive.Count == 0)
            {
                LogStaleApply(action, "осложнения уже сняты");
                return false;
            }

            string complicationId = stillActive[0];
            bool applied = _treatmentStartHandler.TryTreatComplication(
                complicationId,
                fromDialogueAction: false,
                out _);

            if (applied)
            {
                _monitor.Log(
                    $"[MedicalAction] applied type=TreatComplications complication={complicationId} (debug/legacy pending)",
                    LogLevel.Info);
            }

            return applied;
        }

        private bool ApplyPendingAdvancePhase(PendingMedicalAction action)
        {
            string injuryId = action.InjuryId!;
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                LogStaleApply(action, $"состояние {injuryId} не найдено");
                return false;
            }

            if (!debuffState.IsPhasedInjury || debuffState.CurrentPhase >= debuffState.TotalPhases)
            {
                LogStaleApply(action, $"{injuryId} не фазовая или уже на последней фазе");
                _stateManager.SanitizeNonPhasedReadyFlags();
                return false;
            }

            if (!debuffState.ReadyForNextPhase)
            {
                LogStaleApply(action, $"{injuryId} ReadyForNextPhase=false");
                return false;
            }

            if (!_treatmentStartHandler.TryAdvancePhase(injuryId, fromDialogueAction: false, out _))
                return false;

            _careTrustManager.RewardTimelyCheckupOncePerDay();
            return true;
        }

        private bool ApplyPendingCompleteRecovery(PendingMedicalAction action)
        {
            string injuryId = action.InjuryId!;
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                LogStaleApply(action, $"состояние {injuryId} не найдено");
                return false;
            }

            if (!debuffState.ReadyForRecovery)
            {
                LogStaleApply(action, $"{injuryId} ReadyForRecovery=false");
                return false;
            }

            if (!_treatmentStartHandler.TryCompleteRecovery(injuryId, fromDialogueAction: false, out _))
                return false;

            _careTrustManager.RewardTimelyCheckupOncePerDay();
            return true;
        }

        private bool ApplyPendingSimpleCompletionTopic(PendingMedicalAction action)
        {
            string topicId = action.TopicId!;
            if (!_dialogueManager.HasTopic(topicId))
            {
                LogStaleApply(action, $"topic {topicId} уже снят");
                return false;
            }

            string injuryName = topicId.Replace("topic", "").Replace("Cured", "");
            string buffId = "buff" + injuryName;

            _dialogueManager.RemoveTopic(topicId);

            var debuffState = _stateManager.GetDebuffState(buffId);
            if (debuffState != null)
            {
                if (!debuffState.ReadyForRecovery)
                    _stateManager.SetReadyForRecovery(buffId, true);

                _monitor.Log(
                    $"[MedicalAction] SimpleCompletionTopic → CompleteRecovery (DebuffState для {buffId})",
                    LogLevel.Info);

                return ApplyPendingCompleteRecovery(new PendingMedicalAction
                {
                    Type = MedicalActionType.CompleteRecovery,
                    InjuryId = buffId,
                });
            }

            _treatmentManager.ApplyMechanicalPhasedRecovery(buffId);
            _rehabManager.TryStartRehabAfterRecovery(buffId);
            GrantMedicalFriendship(10);
            _stateManager.Save();

            _monitor.Log($"[MedicalAction] applied type=SimpleCompletionTopic topic={topicId} (legacy)", LogLevel.Info);
            _doctorVisitReminderManager.SyncReminderBuff();
            return true;
        }

        private void LogStaleApply(PendingMedicalAction action, string reason)
        {
            _monitor.Log(
                $"[MedicalAction] ⚠️ apply skipped type={action.Type} target={action.InjuryId ?? action.TopicId}: {reason}",
                LogLevel.Warn);
        }

        private void ShowHarveyEmote(int emoteId)
        {
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null)
                _dialogueManager.ShowEmote(harvey, emoteId);
        }

        /// <summary>Бонус Friendship за завершение медицинского действия (только положительный; нарушения режима не штрафуют Friendship).</summary>
        private void GrantMedicalFriendship(int points)
        {
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null)
                Game1.player.changeFriendship(points, harvey);
        }

        private void ProcessMedicalInteractionCompliance(
            Dictionary<string, int>? complicationStartDays = null,
            int? today = null)
        {
            int currentDay = today ?? (int)Game1.stats.DaysPlayed;

            if (_prescriptionManager.HasActivePrescription(PrescriptionIds.Checkup))
                _complianceManager.OnCheckupVisit(currentDay);

            _selfCareManager.OnHarveyMedicalVisit();

            if (complicationStartDays == null)
                return;

            foreach (var (compId, startDay) in complicationStartDays)
                _complianceManager.OnComplicationTreatedSameDay(startDay, currentDay);
        }

        private string? FindActiveCompletionTopic()
        {
            var completionTopics = new[]
            {
                TopicIds.GetCuredTopic("buffHurt"),
                TopicIds.GetCuredTopic("buffBadlyHurt"),
                TopicIds.GetCuredTopic("buffBruisedRibs"),
                TopicIds.GetCuredTopic("buffSprainedAnkle"),
                TopicIds.GetCuredTopic("buffBackStrain"),
                TopicIds.GetCuredTopic("buffDeepCuts"),
                TopicIds.GetCuredTopic("buffBurnWounds"),
                TopicIds.GetCuredTopic("buffTornMuscles"),
                TopicIds.GetCuredTopic("buffConcussion"),
                TopicIds.GetCuredTopic("buffFracturedBone"),
                TopicIds.GetCuredTopic("buffShrapnelWounds"),
                TopicIds.GetCuredTopic("buffInfectedWound"),
                ConversationTopics.ColdCured,
                ConversationTopics.SurgicalWoundCured,
            };

            return completionTopics
                .Where(topicId =>
                {
                    if (!_dialogueManager.HasTopic(topicId))
                        return false;

                    string? buffId = DeriveBuffIdFromCuredTopic(topicId);
                    if (buffId != null && _stateManager.HasDebuffState(buffId))
                        return false;

                    return true;
                })
                .OrderByDescending(GetCompletionTopicPriority)
                .FirstOrDefault();
        }

        private static string? DeriveBuffIdFromCuredTopic(string topicId)
        {
            if (topicId == ConversationTopics.ColdCured)
                return InjuryBuffs.Cold;
            if (topicId == ConversationTopics.SurgicalWoundCured)
                return "buffSurgicalWound";

            if (topicId.StartsWith("topic", StringComparison.OrdinalIgnoreCase)
                && topicId.EndsWith("Cured", StringComparison.OrdinalIgnoreCase))
                return "buff" + topicId.Substring(5, topicId.Length - 5 - 5);

            return null;
        }

        private int GetCompletionTopicPriority(string topicId)
        {
            if (topicId == ConversationTopics.ColdCured)
                return GetInjuryPriority(InjuryBuffs.Cold);
            if (topicId == ConversationTopics.SurgicalWoundCured)
                return GetInjuryPriority("buffSurgicalWound");

            if (topicId.EndsWith("Cured", StringComparison.OrdinalIgnoreCase))
            {
                string buffId = "buff" + topicId.Replace("topic", "").Replace("Cured", "");
                return GetInjuryPriority(buffId);
            }

            return 0;
        }

        private void ClearPendingMedicalAction()
        {
            _pendingMedicalAction = null;
            _pendingSawDialogueBox = false;
        }

        /// <summary>
        /// StartTreatment / TreatComplications — только через CP $action, не после закрытия DialogueBox.
        /// </summary>
        private void HandleActionOnlyPendingCleanup(PendingMedicalAction pending)
        {
            if (Game1.activeClickableMenu is DialogueBox)
            {
                _pendingSawDialogueBox = true;
                return;
            }

            if (!pending.DialogueWasShown)
                return;

            int elapsed = Game1.ticks - pending.StartedTick;
            if (!_pendingSawDialogueBox && elapsed < 60)
                return;

            string expectedAction = pending.Type == MedicalActionType.StartTreatment
                ? TreatmentStartActions.StartTreatment
                : TreatmentStartActions.TreatComplication;
            string target = pending.InjuryId
                ?? pending.Complications.FirstOrDefault()
                ?? "(none)";

            _monitor.Log(
                $"[MedicalAction] ⚠️ {pending.Type} dialogue closed without $action {expectedAction} " +
                $"(target={target}) — pending cleared, treatment unchanged",
                LogLevel.Warn);
            ClearPendingMedicalAction();
        }

        private bool LogActionOnlyPendingStale(PendingMedicalAction action)
        {
            string expectedAction = action.Type == MedicalActionType.StartTreatment
                ? TreatmentStartActions.StartTreatment
                : TreatmentStartActions.TreatComplication;
            _monitor.Log(
                $"[MedicalAction] ⚠️ stale pending {action.Type} ignored — use $action {expectedAction}",
                LogLevel.Warn);
            return false;
        }

        /// <summary>
        /// Получить приоритет травмы (выше = важнее)
        /// </summary>
        private int GetInjuryPriority(string buffId)
        {
            return buffId switch
            {
                "buffConcussion" => 100,
                "buffInfectedWound" => 90,
                "buffFracturedBone" => 85,
                "buffSurgicalWound" => 80,
                "buffShrapnelWounds" => 75,
                "buffBurnWounds" => 70,
                "buffDeepCuts" => 65,
                "buffTornMuscles" => 60,
                "buffBackStrain" => 55,
                "buffBruisedRibs" => 50,
                "buffSprainedAnkle" => 45,
                "buffBadlyHurt" => 40,
                "buffHurt" => 30,
                InjuryBuffs.Cold => 25,
                _ => 0
            };
        }
    }
}
