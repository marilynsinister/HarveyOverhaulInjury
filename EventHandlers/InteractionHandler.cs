using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик взаимодействий с Харви (клики): диалог → закрытие DialogueBox → применение лечения.
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
            TreatmentManager treatmentManager)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
            _treatmentManager = treatmentManager;
        }

        /// <summary>
        /// Применить отложенное медицинское действие после закрытия DialogueBox.
        /// </summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (_pendingMedicalAction == null)
                return;

            var pending = _pendingMedicalAction;

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
                    $"не завершилось за {PendingMedicalTimeoutTicks} тиков — сброс pending.",
                    LogLevel.Warn);
                ClearPendingMedicalAction();
                return;
            }

            if (Game1.activeClickableMenu is DialogueBox)
            {
                _pendingSawDialogueBox = true;
                return;
            }

            if (!pending.DialogueWasShown)
                return;

            if (!_pendingSawDialogueBox)
                return;

            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

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
            if (!e.Button.IsActionButton()) return;
            if (Game1.activeClickableMenu is DialogueBox) return;

            var loc = Game1.currentLocation;
            if (loc == null)
            {
                LastClickDebug = "Клик: нет currentLocation";
                return;
            }

            var tile = _helper.Input.GetCursorPosition().GrabTile;
            var harvey = HarveyHelper.GetHarveyAtTile(loc, tile);

            if (harvey == null)
            {
                LastClickDebug = $"Клик: Action GrabTile({tile.X:F0},{tile.Y:F0}) — не Харви";
                return;
            }

            if (_pendingMedicalAction is { Applied: false } activePending)
            {
                SuppressHarveyClickButtons(e);
                _monitor.Log(
                    $"[MedicalAction] клик по Харви подавлен: pending {FormatMedicalActionLabel(activePending)} " +
                    $"(ticks={Game1.ticks - activePending.StartedTick}, dialogueShown={activePending.DialogueWasShown})",
                    LogLevel.Warn);
                LastClickDebug = BuildClickDebugSnapshot(
                    activePending,
                    null,
                    "BLOCKED: pending medical action in progress");
                return;
            }

            var injuries = _injuryManager.CollectAllInjuries();
            var resolved = TryResolveMedicalAction(injuries);
            if (resolved == null)
            {
                LastClickDebug = BuildClickDebugSnapshot(
                    null,
                    null,
                    "ALLOWED: no InjuryCare medical action");
                return;
            }

            LogResolvedMedicalAction(resolved);
            SuppressHarveyClickButtons(e);

            try
            {
                if (!BeginMedicalActionDialogue(harvey, resolved))
                {
                    ClearPendingMedicalAction();
                    _monitor.Log(
                        $"[MedicalAction] ⚠️ диалог не запущен — состояние изменилось до показа " +
                        $"({FormatMedicalActionLabel(resolved)})",
                        LogLevel.Warn);
                    LastClickDebug = BuildClickDebugSnapshot(
                        null,
                        resolved,
                        "BLOCKED: resolved action stale before dialogue (suppress, no vanilla)");
                    return;
                }

                LastClickDebug = BuildClickDebugSnapshot(
                    _pendingMedicalAction,
                    resolved,
                    "BLOCKED: InjuryCare medical dialogue started");
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка BeginMedicalDialogue ({resolved.Type}): {ex}", LogLevel.Error);
                ClearPendingMedicalAction();
                LastClickDebug = BuildClickDebugSnapshot(
                    null,
                    resolved,
                    "BLOCKED: medical dialogue error (suppress, no vanilla)");
            }
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

            var resolved = TryResolveMedicalAction(_injuryManager.CollectAllInjuries());
            if (resolved == null)
                return "none (standard dialogue allowed)";

            return $"SELECT {FormatMedicalActionLabel(resolved)}";
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
            var treatableStates = GetHarveyTreatableInjuryStates().ToList();

            // A. CompleteRecovery
            var readyRecovery = treatableStates
                .Where(d => d.TreatmentStarted && d.IsLastPhase && d.ReadyForRecovery)
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault();

            if (readyRecovery != null)
            {
                return new PendingMedicalAction
                {
                    Type = MedicalActionType.CompleteRecovery,
                    InjuryId = readyRecovery.BuffId
                };
            }

            // B. AdvancePhase
            var readyPhase = treatableStates
                .Where(d => d.TreatmentStarted && !d.IsLastPhase && d.ReadyForNextPhase)
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault();

            if (readyPhase != null)
            {
                return new PendingMedicalAction
                {
                    Type = MedicalActionType.AdvancePhase,
                    InjuryId = readyPhase.BuffId
                };
            }

            // C. StartTreatment — только DebuffState InjuryCare, без проверки чужих/фазовых баффов на игроке
            var nextToTreat = treatableStates
                .Where(d => !d.TreatmentStarted)
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault();

            if (nextToTreat != null)
            {
                injuries.MainInjury = nextToTreat.BuffId;
                var complications = GetActiveComplicationIds();
                return new PendingMedicalAction
                {
                    Type = MedicalActionType.StartTreatment,
                    InjuryId = nextToTreat.BuffId,
                    Complications = complications
                };
            }

            // D. TreatComplications — по DebuffState осложнений InjuryCare
            var activeComplications = GetActiveComplicationIds();
            if (activeComplications.Count > 0)
            {
                return new PendingMedicalAction
                {
                    Type = MedicalActionType.TreatComplications,
                    Complications = activeComplications
                };
            }

            // E. SimpleCompletionTopic
            string? completionTopic = FindActiveCompletionTopic();
            if (completionTopic != null)
            {
                return new PendingMedicalAction
                {
                    Type = MedicalActionType.SimpleCompletionTopic,
                    TopicId = completionTopic
                };
            }

            var inTreatmentNotReady = treatableStates.Where(d => d.TreatmentStarted).ToList();
            if (inTreatmentNotReady.Count > 0)
            {
                _monitor.Log(
                    $"Травмы в лечении ({inTreatmentNotReady.Count}), но ни одна не готова к переходу: {string.Join(", ", inTreatmentNotReady.Select(d => d.BuffId))}",
                    LogLevel.Debug);
            }

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

        private List<string> GetActiveComplicationIds()
        {
            var active = _stateManager.GetAllActiveDebuffStates()
                .Select(d => d.BuffId)
                .Where(id => InjurySets.KnownComplicationBuffIds.Contains(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var ordered = new List<string>();
            foreach (string compId in InjurySets.ComplicationPriorityOrder)
            {
                if (active.Contains(compId))
                    ordered.Add(compId);
            }

            foreach (string compId in active.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                if (!ordered.Contains(compId, StringComparer.OrdinalIgnoreCase))
                    ordered.Add(compId);
            }

            return ordered;
        }

        /// <summary>
        /// Запустить медицинский диалог по resolved action. Механика — только после закрытия DialogueBox.
        /// </summary>
        private bool BeginMedicalActionDialogue(NPC harvey, PendingMedicalAction pending)
        {
            return pending.Type switch
            {
                MedicalActionType.AdvancePhase => TryBeginAdvancePhase(harvey, pending.InjuryId!),
                MedicalActionType.CompleteRecovery => TryBeginCompleteRecovery(harvey, pending.InjuryId!),
                MedicalActionType.StartTreatment or MedicalActionType.TreatComplications
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
            if (debuffState == null || !debuffState.ReadyForNextPhase)
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
            if (debuffState.IsLastPhase && debuffState.ReadyForRecovery)
            {
                CompleteRecovery(harvey, injuryId);
                return _pendingMedicalAction != null;
            }

            if (debuffState.ReadyForNextPhase)
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

            string dialogueKey;
            string dialogueText = type switch
            {
                MedicalActionType.StartTreatment or MedicalActionType.TreatComplications
                    => BuildTreatmentDialogueText(pending, out dialogueKey),
                MedicalActionType.AdvancePhase
                    => BuildAdvancePhaseDialogue(injuryId!, out dialogueKey),
                MedicalActionType.CompleteRecovery
                    => BuildCompleteRecoveryDialogue(injuryId!, out dialogueKey),
                MedicalActionType.SimpleCompletionTopic
                    => BuildSimpleCompletionDialogueText(topicId!, out dialogueKey),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            if (type is MedicalActionType.StartTreatment or MedicalActionType.TreatComplications)
            {
                var injuries = BuildInjuryContextForDialogue(pending);
                _treatmentManager.TreatWithReaction(harvey, injuries);
            }
            else
            {
                ShowMedicalEmote(harvey, type);
            }

            _dialogueManager.Speak(harvey, dialogueText);

            pending.DialogueWasShown = true;
            _pendingSawDialogueBox = Game1.activeClickableMenu is DialogueBox;

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
            string dialogue = _treatmentManager.BuildCombinedDialogue(injuries, markTreatmentDiscussed: false);

            if (string.IsNullOrWhiteSpace(dialogue))
            {
                dialogue = pending.Type == MedicalActionType.TreatComplications
                    ? "Я вижу осложнение — сейчас осмотрю рану и обработаю повязку.$a"
                    : "Сейчас займусь лечением.$u";
            }

            if (pending.InjuryId != null)
            {
                string injuryName = pending.InjuryId.Replace("buff", "");
                bool wasDiscussed = _stateManager.GetDebuffState(pending.InjuryId)?.HarveyConversationHappened == true;
                string treatPrefix = wasDiscussed ? $"Treat_{injuryName}_After" : $"Treat_{injuryName}_Before";
                dialogueKey = pending.Complications.Count > 0
                    ? $"{treatPrefix}+Proximity_*"
                    : treatPrefix;
            }
            else
            {
                dialogueKey = pending.Complications.Count > 0
                    ? "Proximity_*"
                    : "Treat_*";
            }

            return dialogue;
        }

        private string BuildAdvancePhaseDialogue(string injuryId, out string dialogueKey)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            int currentPhase = debuffState?.CurrentPhase ?? 0;
            int nextPhase = currentPhase + 1;
            string injuryName = injuryId.Replace("buff", "");
            dialogueKey = $"PhaseTransition_{injuryName}_{nextPhase}";

            return _dialogueManager.PickRandomDialogueByPrefix(
                dialogueKey,
                "Я осмотрел тебя. Восстановление идёт достаточно хорошо, чтобы перейти к следующему этапу лечения.$u");
        }

        private string BuildCompleteRecoveryDialogue(string injuryId, out string dialogueKey)
        {
            string injuryName = injuryId.Replace("buff", "");
            string recoveryPrefix = $"Recovery_Complete_{injuryName}";

            string? exactRecovery = _dialogueManager.TryLoadHarveyDialogue(recoveryPrefix);
            if (!string.IsNullOrWhiteSpace(exactRecovery))
            {
                dialogueKey = recoveryPrefix;
                return exactRecovery;
            }

            string fromRecoveryPrefix = _dialogueManager.PickRandomDialogueByPrefix(
                recoveryPrefix,
                string.Empty);
            if (!string.IsNullOrWhiteSpace(fromRecoveryPrefix))
            {
                dialogueKey = recoveryPrefix;
                return fromRecoveryPrefix;
            }

            dialogueKey = TopicIds.GetCuredTopic(injuryId);
            return _dialogueManager.PickRandomDialogueByPrefix(
                dialogueKey,
                "Осмотр окончен. Ты ${выздоровел^выздоровела}$, но я всё равно хочу, чтобы ты берегла себя.$h");
        }

        private string BuildSimpleCompletionDialogueText(string topicId, out string dialogueKey)
        {
            dialogueKey = topicId;
            return _dialogueManager.PickRandomDialogueByPrefix(
                topicId,
                "Отлично! Ты полностью ${выздоровел^выздоровела}$. Я горжусь тобой за то, что ты следовал${^а}$ всем моим рекомендациям. Береги себя!$h");
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
            return $"[MedicalAction] queued type=AdvancePhase injury={pending.InjuryId} phase={currentPhase}->{currentPhase + 1}";
        }

        private bool ApplyPendingMedicalAction(PendingMedicalAction action)
        {
            return action.Type switch
            {
                MedicalActionType.StartTreatment => ApplyPendingStartTreatment(action),
                MedicalActionType.TreatComplications => ApplyPendingTreatComplications(action),
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

            var debuffState = _stateManager.GetDebuffState(action.InjuryId);
            if (debuffState == null)
            {
                LogStaleApply(action, $"DebuffState {action.InjuryId} не найден");
                return false;
            }

            if (debuffState.TreatmentStarted)
            {
                LogStaleApply(action, $"{action.InjuryId} уже TreatmentStarted");
                return false;
            }

            _treatmentManager.ApplyTreatmentForInjury(action.InjuryId);
            _dialogueManager.ClearHarveyNeedsFirstTreatmentTopic("лечение начато после медицинского диалога");
            _stateManager.MarkHarveyConversation(action.InjuryId, true);

            var stillActiveComplications = GetActiveComplicationIds();
            if (stillActiveComplications.Count > 0)
                _treatmentManager.TreatAllComplications(stillActiveComplications);

            GrantMedicalFriendship(10);
            ShowHarveyEmote(HarveyHelper.GetCaringEmote());
            _stateManager.Save();

            _monitor.Log(
                $"[MedicalAction] applied type=StartTreatment injury={action.InjuryId} complications={stillActiveComplications.Count}",
                LogLevel.Info);
            return true;
        }

        private bool ApplyPendingTreatComplications(PendingMedicalAction action)
        {
            var stillActive = GetActiveComplicationIds();
            if (stillActive.Count == 0)
            {
                LogStaleApply(action, "осложнения уже сняты");
                return false;
            }

            _treatmentManager.TreatAllComplications(stillActive);
            GrantMedicalFriendship(10);
            _stateManager.Save();

            _monitor.Log(
                $"[MedicalAction] applied type=TreatComplications complications={string.Join(", ", stillActive)}",
                LogLevel.Info);
            return true;
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

            if (!debuffState.ReadyForNextPhase)
            {
                LogStaleApply(action, $"{injuryId} ReadyForNextPhase=false");
                return false;
            }

            _treatmentManager.AdvanceInjuryToNextPhase(injuryId);
            GrantMedicalFriendship(10);
            ShowHarveyEmote(HarveyHelper.GetRecoveryEmote());
            _stateManager.Save();

            _monitor.Log($"[MedicalAction] applied type=AdvancePhase injury={injuryId}", LogLevel.Info);
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

            _treatmentManager.ApplyMechanicalPhasedRecovery(injuryId);
            Game1.addHUDMessage(new HUDMessage(
                "Выздоровление завершено! Харви гордится тобой!",
                HUDMessage.achievement_type));
            GrantMedicalFriendship(15);
            _stateManager.Save();

            _monitor.Log($"[MedicalAction] applied type=CompleteRecovery injury={injuryId}", LogLevel.Info);
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

            if (SimpleInjuryCures.Map.TryGetValue(buffId, out var cureBuff))
            {
                _buffManager.RemoveBuff(cureBuff);
                _monitor.Log($"Снят лечебный бафф: {cureBuff} (завершение {buffId})", LogLevel.Info);
            }

            _dialogueManager.RemoveTopic(topicId);
            _buffManager.AddBuff(CureBuffs.Care, 480);
            GrantMedicalFriendship(10);
            _stateManager.Save();

            _monitor.Log($"[MedicalAction] applied type=SimpleCompletionTopic topic={topicId}", LogLevel.Info);
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

        private void GrantMedicalFriendship(int points)
        {
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null)
                Game1.player.changeFriendship(points, harvey);
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
                .Where(_dialogueManager.HasTopic)
                .OrderByDescending(GetCompletionTopicPriority)
                .FirstOrDefault();
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
