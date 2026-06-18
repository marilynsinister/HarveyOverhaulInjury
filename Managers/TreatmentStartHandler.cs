using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.Core.Services;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Services;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Старт лечения через CP conversation topic + dialogue $action (не через клик InteractionHandler).
    /// </summary>
    public sealed class TreatmentStartHandler
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;
        private readonly TreatmentManager _treatmentManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly ComplicationManager _complicationManager;
        private readonly ComplianceManager _complianceManager;
        private readonly DoctorVisitReminderManager _doctorVisitReminderManager;
        private readonly RecoveryPlanManager _recoveryPlanManager;
        private readonly CheckupManager _checkupManager;
        private readonly RehabManager _rehabManager;
        private readonly SelfCareManager _selfCareManager;
        private MedicalLetterScheduler? _medicalLetterScheduler;
        private IHarveyCoreApi? _coreApi;

        public TreatmentStartHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager,
            TreatmentManager treatmentManager,
            HospitalizationManager hospitalizationManager,
            ComplicationManager complicationManager,
            ComplianceManager complianceManager,
            DoctorVisitReminderManager doctorVisitReminderManager,
            RecoveryPlanManager recoveryPlanManager,
            CheckupManager checkupManager,
            RehabManager rehabManager,
            SelfCareManager selfCareManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
            _treatmentManager = treatmentManager;
            _hospitalizationManager = hospitalizationManager;
            _complicationManager = complicationManager;
            _complianceManager = complianceManager;
            _doctorVisitReminderManager = doctorVisitReminderManager;
            _recoveryPlanManager = recoveryPlanManager;
            _checkupManager = checkupManager;
            _rehabManager = rehabManager;
            _selfCareManager = selfCareManager;
        }

        public void SetCoreApi(IHarveyCoreApi? coreApi) => _coreApi = coreApi;

        public void SetMedicalLetterScheduler(MedicalLetterScheduler scheduler) =>
            _medicalLetterScheduler = scheduler;

        private void CancelMedicalLettersForInjury(string injuryId)
        {
            if (_medicalLetterScheduler == null || string.IsNullOrWhiteSpace(injuryId))
                return;

            _medicalLetterScheduler.CancelLettersForState(injuryId);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.TreatmentPlan);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.TreatmentUrgent);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.TreatmentFinal);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.NeglectWarning);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.UntreatedInjury);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.CheckupReminder);
            _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.CheckupOverdue);
        }

        private void CancelMedicalLettersForComplication(string complicationId)
        {
            if (_medicalLetterScheduler == null || string.IsNullOrWhiteSpace(complicationId))
                return;

            _medicalLetterScheduler.CancelLettersForState(complicationId);
            if (string.Equals(complicationId, InjuryBuffs.DirtyWound, StringComparison.OrdinalIgnoreCase))
                _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.InfectionDirty);
            if (string.Equals(complicationId, InjuryBuffs.WetBandage, StringComparison.OrdinalIgnoreCase))
                _medicalLetterScheduler.CancelLettersForReason(MedicalLetterReasons.InfectionWet);
        }

        public void RegisterTriggerActions()
        {
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.StartTreatment,
                OnDialogueStartTreatmentAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.TreatComplication,
                OnDialogueTreatComplicationAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.AdvancePhase,
                OnDialogueAdvancePhaseAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.CompleteRecovery,
                OnDialogueCompleteRecoveryAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.RevealAndStartTreatment,
                OnDialogueRevealAndStartTreatmentAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.DenyHiddenInjury,
                OnDialogueDenyHiddenInjuryAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.PostponeHiddenInjury,
                OnDialoguePostponeHiddenInjuryAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.MarkFestivalNotice,
                OnDialogueMarkFestivalNoticeAction);
            _monitor.Log(
                $"[TreatmentStart] Registered trigger actions: {TreatmentStartActions.StartTreatment}, " +
                $"{TreatmentStartActions.TreatComplication}, {TreatmentStartActions.AdvancePhase}, " +
                $"{TreatmentStartActions.CompleteRecovery}, {TreatmentStartActions.RevealAndStartTreatment}, " +
                $"{TreatmentStartActions.DenyHiddenInjury}, {TreatmentStartActions.PostponeHiddenInjury}, " +
                $"{TreatmentStartActions.MarkFestivalNotice}",
                LogLevel.Debug);
        }

        /// <summary>Обработчик $action HarveyOverhaulInjury_StartTreatment &lt;injuryId&gt; из диалога Харви.</summary>
        public bool OnDialogueStartTreatmentAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                _monitor.Log("[TreatmentStart] $action пропущен: world not ready", LogLevel.Warn);
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            string topicKey = TopicIds.GetStartTreatmentTopic(injuryId);
            if (!ValidateDialogueAction(injuryId, topicKey, TreatmentStartActions.StartTreatment, out error))
                return false;

            _monitor.Log(
                $"[TreatmentStart] StartTreatment action вызван из dialogue injury={injuryId}",
                LogLevel.Info);

            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.StartTreatment} {injuryId}");
            _coreApi?.LogHarveyClickDiagnostics("action");

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState != null)
                debuffState.TreatmentIntroShown = true;

            return TryStartTreatment(injuryId, fromDialogueAction: true, out error);
        }

        public bool OnDialogueAdvancePhaseAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                error = $"state {injuryId} not found";
                return false;
            }

            int nextPhase = debuffState.CurrentPhase + 1;
            string topicKey = TopicIds.GetAdvancePhaseTopic(injuryId, nextPhase);
            if (!ValidateDialogueAction(injuryId, topicKey, TreatmentStartActions.AdvancePhase, out error))
                return false;

            _monitor.Log(
                $"[MedicalAction] AdvancePhase action injury={injuryId} phase={debuffState.CurrentPhase}->{nextPhase}",
                LogLevel.Info);

            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.AdvancePhase} {injuryId}");
            _coreApi?.LogHarveyClickDiagnostics("action");

            return TryAdvancePhase(injuryId, fromDialogueAction: true, out error);
        }

        public bool OnDialogueCompleteRecoveryAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            string topicKey = TopicIds.GetCompleteRecoveryTopic(injuryId);
            if (!ValidateDialogueAction(injuryId, topicKey, TreatmentStartActions.CompleteRecovery, out error))
                return false;

            _monitor.Log($"[MedicalAction] CompleteRecovery action injury={injuryId}", LogLevel.Info);
            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.CompleteRecovery} {injuryId}");
            _coreApi?.LogHarveyClickDiagnostics("action");
            return TryCompleteRecovery(injuryId, fromDialogueAction: true, out error);
        }

        public bool OnDialogueRevealAndStartTreatmentAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            if (!ValidateHiddenInjuryDialogueAction(injuryId, TreatmentStartActions.RevealAndStartTreatment, out error))
                return false;

            _monitor.Log(
                $"[HiddenInjury] RevealAndStartTreatment action injury={injuryId}",
                LogLevel.Info);

            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.RevealAndStartTreatment} {injuryId}");
            _coreApi?.LogHarveyClickDiagnostics("action");

            return TryRevealAndStartTreatment(injuryId, fromDialogueAction: true, out error);
        }

        public bool OnDialogueDenyHiddenInjuryAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            injuryId = ResolveHiddenInjuryIdForAction(injuryId);

            if (!ValidateHiddenInjuryDialogueAction(injuryId, TreatmentStartActions.DenyHiddenInjury, out error))
                return false;

            _monitor.Log($"[HiddenInjury] DenyHiddenInjury action injury={injuryId}", LogLevel.Info);
            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.DenyHiddenInjury} {injuryId}");
            return TryDenyHiddenInjury(injuryId, out error);
        }

        public bool OnDialoguePostponeHiddenInjuryAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            injuryId = ResolveHiddenInjuryIdForAction(injuryId);

            if (!ValidateHiddenInjuryDialogueAction(injuryId, TreatmentStartActions.PostponeHiddenInjury, out error))
                return false;

            _monitor.Log($"[HiddenInjury] PostponeHiddenInjury action injury={injuryId}", LogLevel.Info);
            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.PostponeHiddenInjury} {injuryId}");
            return TryPostponeHiddenInjury(injuryId, out error);
        }

        public bool OnDialogueMarkFestivalNoticeAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            string? injuryId = ParseInjuryIdFromActionArgs(args, out error);
            if (injuryId == null)
                return false;

            injuryId = ResolveHiddenInjuryIdForAction(injuryId);

            if (!ValidateHiddenInjuryDialogueAction(injuryId, TreatmentStartActions.MarkFestivalNotice, out error))
                return false;

            _monitor.Log($"[HiddenInjury] MarkFestivalNotice action injury={injuryId}", LogLevel.Info);
            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.MarkFestivalNotice} {injuryId}");
            return TryMarkFestivalHiddenInjuryNotice(injuryId, out error);
        }

        private bool ValidateDialogueAction(string stateId, string topicKey, string actionKey, out string error)
        {
            error = string.Empty;

            string? activeTopic = ResolveActiveMedicalActionTopic(stateId, topicKey, actionKey);
            if (activeTopic == null)
            {
                error = $"topic {topicKey} not on player (foreign click?)";
                _monitor.Log($"[MedicalAction] $action {actionKey} rejected: {error}", LogLevel.Warn);
                return false;
            }

            if (_coreApi != null
                && !_coreApi.IsMedicalIntentActive(HarveyProviderRegistry.InjuryProviderId, activeTopic, stateId))
            {
                error = $"intent not active for topic={activeTopic} state={stateId}";
                _monitor.Log($"[MedicalAction] $action {actionKey} rejected: {error}", LogLevel.Warn);
                return false;
            }

            _monitor.Log(
                $"[MedicalAction] $action {actionKey} allowed topic={activeTopic} state={stateId}",
                LogLevel.Info);
            return true;
        }

        private string? ResolveActiveMedicalActionTopic(string stateId, string canonicalTopicKey, string actionKey)
        {
            if (_dialogueManager.HasTopic(canonicalTopicKey))
                return canonicalTopicKey;

            string? legacyAlias = TopicIds.GetLegacyActionTopicAlias(actionKey, stateId);
            if (!string.IsNullOrWhiteSpace(legacyAlias) && _dialogueManager.HasTopic(legacyAlias))
                return legacyAlias;

            if (stateId.StartsWith("buff", StringComparison.OrdinalIgnoreCase))
            {
                return TopicIds.GetAllActionTopicsForInjury(stateId)
                    .FirstOrDefault(_dialogueManager.HasTopic);
            }

            if (stateId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
            {
                string treatTopic = TopicIds.GetTreatComplicationTopic(stateId);
                if (_dialogueManager.HasTopic(treatTopic))
                    return treatTopic;

                string legacyTopic = TopicIds.GetTreatmentNeededComplicationTopic(stateId);
                if (_dialogueManager.HasTopic(legacyTopic))
                    return legacyTopic;
            }

            return null;
        }

        private bool HasAnyOwnedActionTopic(string stateId, string topicKey)
        {
            if (stateId.StartsWith("buff", StringComparison.OrdinalIgnoreCase))
            {
                return _dialogueManager.HasTopic(TopicIds.GetTreatmentNeededTopic(stateId))
                    || _dialogueManager.HasTopic(TopicIds.GetStartTreatmentTopic(stateId))
                    || _dialogueManager.HasTopic(TopicIds.GetCompleteRecoveryTopic(stateId))
                    || TopicIds.GetAllActionTopicsForInjury(stateId).Any(_dialogueManager.HasTopic)
                    || TopicIds.GetAllHiddenInjuryTopicsForInjury(stateId).Any(_dialogueManager.HasTopic);
            }

            if (stateId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
            {
                return _dialogueManager.HasTopic(TopicIds.GetTreatmentNeededComplicationTopic(stateId))
                    || _dialogueManager.HasTopic(TopicIds.GetTreatComplicationTopic(stateId));
            }

            return _dialogueManager.HasTopic(topicKey);
        }

        /// <summary>Обработчик $action HarveyOverhaulInjury_TreatComplication &lt;complicationBuffId&gt;.</summary>
        public bool OnDialogueTreatComplicationAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;

            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                _monitor.Log("[ComplicationTreatment] $action пропущен: world not ready", LogLevel.Warn);
                return false;
            }

            string? complicationId = ParseComplicationIdFromActionArgs(args, out error);
            if (complicationId == null)
                return false;

            string topicKey = TopicIds.GetTreatComplicationTopic(complicationId);
            if (!ValidateDialogueAction(complicationId, topicKey, TreatmentStartActions.TreatComplication, out error))
                return false;

            _monitor.Log(
                $"[ComplicationTreatment] TreatComplication action вызван из dialogue complication={complicationId}",
                LogLevel.Info);

            _coreApi?.SetHarveyClickActionExecuted($"{TreatmentStartActions.TreatComplication} {complicationId}");
            _coreApi?.LogHarveyClickDiagnostics("action");

            return TryTreatComplication(complicationId, fromDialogueAction: true, out error);
        }

        /// <summary>Снять одно осложнение. fromDialogueAction=false — debug fallback.</summary>
        public bool TryTreatComplication(string complicationId, bool fromDialogueAction, out string? skipReason)
        {
            skipReason = null;

            if (!Context.IsWorldReady)
            {
                skipReason = "world not ready";
                return false;
            }

            if (string.IsNullOrWhiteSpace(complicationId)
                || !InjurySets.KnownComplicationBuffIds.Contains(complicationId))
            {
                skipReason = $"unknown complication {complicationId}";
                _monitor.Log($"[ComplicationTreatment] ⚠️ {skipReason}", LogLevel.Warn);
                return false;
            }

            bool isActive = _stateManager.State.ActiveComplications.ContainsKey(complicationId)
                || _buffManager.HasBuff(complicationId);
            if (!isActive)
            {
                _monitor.Log(
                    $"[ComplicationTreatment] лечение пропущено: осложнение {complicationId} больше не активно",
                    LogLevel.Warn);
                skipReason = "complication no longer active";
                return false;
            }

            int today = (int)Game1.stats.DaysPlayed;
            int? startDay = _stateManager.State.ActiveComplications.GetValueOrDefault(complicationId);

            _treatmentManager.TreatAllComplications(new List<string> { complicationId });

            CancelMedicalLettersForComplication(complicationId);

            if (startDay.HasValue)
                _complianceManager.OnComplicationTreatedSameDay(startDay.Value, today);

            GrantMedicalFriendship(10);
            ShowHarveyEmote(HarveyHelper.GetCaringEmote());
            _stateManager.Save();

            _monitor.Log(
                $"[ComplicationTreatment] applied complication={complicationId} fromDialogue={fromDialogueAction}",
                LogLevel.Info);
            _doctorVisitReminderManager.SyncReminderBuff();
            _recoveryPlanManager.RefreshPlanForToday();
            return true;
        }

        /// <summary>
        /// Начать лечение (механика). fromDialogueAction=false — debug / аварийные пути без CP-диалога.
        /// </summary>
        public bool TryStartTreatment(string injuryId, bool fromDialogueAction, out string? skipReason)
        {
            skipReason = null;

            if (!Context.IsWorldReady)
            {
                skipReason = "world not ready";
                return false;
            }

            if (string.IsNullOrWhiteSpace(injuryId))
            {
                skipReason = "injuryId missing";
                return false;
            }

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                skipReason = $"DebuffState {injuryId} not found";
                _monitor.Log($"[TreatmentStart] ⚠️ {skipReason}", LogLevel.Warn);
                return false;
            }

            if (debuffState.TreatmentStarted || debuffState.TreatmentApplied)
            {
                _monitor.Log(
                    $"[TreatmentStart] лечение пропущено: TreatmentStarted уже true ({injuryId})",
                    LogLevel.Debug);
                skipReason = "already started";
                return true;
            }

            if (!HasBaseInjuryBuff(injuryId))
            {
                _monitor.Log(
                    $"[TreatmentStart] лечение пропущено: injury buff больше не активен ({injuryId})",
                    LogLevel.Warn);
                skipReason = "base injury buff missing";
                return false;
            }

            bool needsHospitalization = ShouldRequireHospitalization(injuryId, debuffState);

            if (!_treatmentManager.ApplyTreatmentForInjury(injuryId))
            {
                skipReason = $"ApplyTreatmentForInjury failed for {injuryId}";
                _monitor.Log($"[TreatmentStart] ⚠️ {skipReason}", LogLevel.Warn);
                return false;
            }

            CancelMedicalLettersForInjury(injuryId);

            debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState != null)
            {
                debuffState.TreatmentIntroShown = fromDialogueAction || debuffState.TreatmentIntroShown;
                debuffState.TreatmentApplied = true;
            }

            _dialogueManager.ClearHarveyNeedsFirstTreatmentTopic(
                fromDialogueAction
                    ? "лечение начато через dialogue $action"
                    : "лечение начато без CP-диалога");
            _stateManager.MarkHarveyConversation(injuryId, true);
            HarveyInjuryAwarenessHelper.TryAddKnownSevereInjuryTopic(_dialogueManager, injuryId);
            _dialogueManager.ClearTreatmentNeededTopic(injuryId, "лечение успешно начато");

            _injuryManager.EnsureTreatmentNeededComplicationTopics();
            int activeComplicationCount = _complicationManager.GetActiveTreatableComplicationIds().Count;

            GrantMedicalFriendship(10);
            ShowHarveyEmote(HarveyHelper.GetCaringEmote());
            _stateManager.Save();

            if (needsHospitalization)
            {
                string reason = GetTreatmentHospitalizationReason(injuryId, debuffState!);
                _monitor.Log(
                    $"[TreatmentStart] StartTreatment → forced hospitalization {injuryId} reason={reason}",
                    LogLevel.Info);

                if (reason == "mine_rescue")
                    _dialogueManager.RemoveTopic(ConversationTopics.MineInjuryRescue);

                NPC? harvey = HarveyHelper.GetHarvey();
                _hospitalizationManager.StartForcedHospitalizationWithExplanation(
                    injuryId,
                    harvey,
                    reason);
            }

            _monitor.Log(
                $"[TreatmentStart] applied injury={injuryId} fromDialogue={fromDialogueAction} " +
                $"activeComplications={activeComplicationCount} (осложнения не лечатся автоматически)",
                LogLevel.Info);
            _doctorVisitReminderManager.SyncReminderBuff();
            _recoveryPlanManager.RefreshPlanForToday(notifyCreated: true);
            return true;
        }

        public bool TryRevealAndStartTreatment(string injuryId, bool fromDialogueAction, out string? skipReason)
        {
            skipReason = null;

            DebuffState? debuffState = _stateManager.GetDebuffState(injuryId);

            if (debuffState != null)
            {
                InjuryVisibilityHelper.RevealHiddenInjury(
                    _stateManager,
                    _dialogueManager,
                    debuffState,
                    injuryId,
                    "reveal_and_treat",
                    _stateManager.State,
                    _monitor);
                debuffState = _stateManager.GetDebuffState(injuryId);
            }
            else
            {
                HarveyInjuryAwarenessHelper.MarkHarveyAware(
                    _stateManager,
                    _dialogueManager,
                    injuryId,
                    "reveal_and_treat",
                    _monitor);
                debuffState = _stateManager.GetDebuffState(injuryId);
            }

            _monitor.Log(
                $"[HiddenInjury] revealed injury={injuryId} fromDialogue={fromDialogueAction}",
                LogLevel.Info);

            ClearHiddenInjuryDetectionTopics(injuryId);

            bool anyComplicationTreated = false;
            foreach (string compId in _complicationManager.GetActiveTreatableComplicationIds().ToList())
            {
                if (!TryTreatComplication(compId, fromDialogueAction, out _))
                    continue;

                anyComplicationTreated = true;
                _monitor.Log(
                    $"[HiddenInjury] complication treated after reveal comp={compId} injury={injuryId}",
                    LogLevel.Info);
            }

            if (anyComplicationTreated)
                return true;

            debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState != null && (debuffState.TreatmentStarted || debuffState.TreatmentApplied))
            {
                _injuryManager.EnsureActiveTreatmentBuffs();
                _stateManager.Save();
                _doctorVisitReminderManager.SyncReminderBuff();
                _recoveryPlanManager.RefreshPlanForToday(notifyUpdated: true);

                _monitor.Log(
                    $"[HiddenInjury] revealed already-treated injury={injuryId}; no StartTreatment needed",
                    LogLevel.Info);
                _monitor.Log(
                    $"[HiddenInjury] StartTreatment skipped: injury={injuryId} already in treatment",
                    LogLevel.Debug);
                skipReason = "already in treatment";
                return true;
            }

            return TryStartTreatment(injuryId, fromDialogueAction, out skipReason);
        }

        public bool TryDenyHiddenInjury(string injuryId, out string? skipReason)
        {
            skipReason = null;

            if (_stateManager.GetDebuffState(injuryId) is not { } debuffState)
            {
                skipReason = $"DebuffState {injuryId} not found";
                return false;
            }

            debuffState.PlayerDeniedInjuryToday = true;
            debuffState.SuspicionLevel++;
            _stateManager.UpdateDebuffState(injuryId, debuffState);
            ClearHiddenInjuryDetectionTopics(injuryId);
            _stateManager.Save();

            _monitor.Log(
                $"[HiddenInjury] Denied injury={injuryId} suspicion={debuffState.SuspicionLevel}",
                LogLevel.Info);
            return true;
        }

        public bool TryPostponeHiddenInjury(string injuryId, out string? skipReason)
        {
            skipReason = null;

            if (_stateManager.GetDebuffState(injuryId) is not { } debuffState)
            {
                skipReason = $"DebuffState {injuryId} not found";
                return false;
            }

            debuffState.SuspicionLevel++;
            _stateManager.UpdateDebuffState(injuryId, debuffState);
            ClearHiddenInjuryDetectionTopics(injuryId);
            _stateManager.Save();

            _monitor.Log(
                $"[HiddenInjury] Postponed injury={injuryId} suspicion={debuffState.SuspicionLevel}",
                LogLevel.Info);
            return true;
        }

        public bool TryMarkFestivalHiddenInjuryNotice(string injuryId, out string? skipReason)
        {
            skipReason = null;

            if (_stateManager.GetDebuffState(injuryId) is { } debuffState)
            {
                InjuryVisibilityHelper.RevealHiddenInjury(
                    _stateManager,
                    _dialogueManager,
                    debuffState,
                    injuryId,
                    "festival_notice",
                    _stateManager.State,
                    _monitor);
            }

            _dialogueManager.AddTopic(TopicIds.GetFestivalDeferTopic(injuryId), 1);
            ClearHiddenInjuryDetectionTopics(injuryId);
            _stateManager.Save();

            _monitor.Log($"[HiddenInjury] Festival notice injury={injuryId}", LogLevel.Info);
            return true;
        }

        private bool ValidateHiddenInjuryDialogueAction(
            string injuryId,
            string actionKey,
            out string error)
        {
            error = string.Empty;

            if (_complicationManager.GetActiveTreatableComplicationIds().Count > 0)
            {
                error = "active treatable complication has priority over hidden injury flow";
                _monitor.Log($"[HiddenInjury] $action {actionKey} rejected: {error}", LogLevel.Warn);
                return false;
            }

            string? activeTopic = TopicIds.GetAllHiddenInjuryTopicsForInjury(injuryId)
                .FirstOrDefault(_dialogueManager.HasTopic);

            if (activeTopic == null)
            {
                error = $"no hidden-injury topic active for {injuryId}";
                _monitor.Log($"[HiddenInjury] $action {actionKey} rejected: {error}", LogLevel.Warn);
                return false;
            }

            if (_coreApi != null
                && !_coreApi.IsMedicalIntentActive(HarveyProviderRegistry.InjuryProviderId, activeTopic, injuryId))
            {
                error = $"intent not active for topic={activeTopic} injury={injuryId}";
                _monitor.Log($"[HiddenInjury] $action {actionKey} rejected: {error}", LogLevel.Warn);
                return false;
            }

            _monitor.Log(
                $"[HiddenInjury] $action {actionKey} allowed topic={activeTopic} injury={injuryId}",
                LogLevel.Info);
            return true;
        }

        private void ClearHiddenInjuryDetectionTopics(string injuryId)
        {
            foreach (string topic in TopicIds.GetAllHiddenInjuryTopicsForInjury(injuryId))
                _dialogueManager.RemoveTopic(topic);
        }

        private string ResolveHiddenInjuryIdForAction(string injuryId)
        {
            if (!string.Equals(injuryId, "buffHurt", StringComparison.OrdinalIgnoreCase))
                return injuryId;

            string? mainId = _injuryManager.GetActiveInjury();
            if (!string.IsNullOrEmpty(mainId)
                && TopicIds.GetAllHiddenInjuryTopicsForInjury(mainId).Any(_dialogueManager.HasTopic))
            {
                return mainId;
            }

            foreach (string buffId in InjurySets.HarveyTreatable)
            {
                if (TopicIds.GetAllHiddenInjuryTopicsForInjury(buffId).Any(_dialogueManager.HasTopic))
                    return buffId;
            }

            return injuryId;
        }

        private static string? ParseComplicationIdFromActionArgs(string[] args, out string error)
        {
            error = string.Empty;

            if (ArgUtility.TryGet(args, 1, out string complicationId, out error, allowBlank: false)
                && complicationId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
            {
                return complicationId;
            }

            if (ArgUtility.TryGet(args, 0, out complicationId, out error, allowBlank: false)
                && complicationId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
            {
                return complicationId;
            }

            error = string.IsNullOrWhiteSpace(error)
                ? "expected complicationBuffId argument (HarveyMod_*)"
                : error;
            return null;
        }

        private static string? ParseInjuryIdFromActionArgs(string[] args, out string error)
        {
            error = string.Empty;

            if (ArgUtility.TryGet(args, 1, out string injuryId, out error, allowBlank: false)
                && injuryId.StartsWith("buff", StringComparison.OrdinalIgnoreCase))
            {
                return injuryId;
            }

            if (ArgUtility.TryGet(args, 0, out injuryId, out error, allowBlank: false)
                && injuryId.StartsWith("buff", StringComparison.OrdinalIgnoreCase))
            {
                return injuryId;
            }

            error = string.IsNullOrWhiteSpace(error)
                ? "expected injuryId argument (buff*)"
                : error;
            return null;
        }

        public bool TryAdvancePhase(string injuryId, bool fromDialogueAction, out string? skipReason)
        {
            skipReason = null;
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                skipReason = $"state {injuryId} not found";
                return false;
            }

            if (!debuffState.IsPhasedInjury || debuffState.CurrentPhase >= debuffState.TotalPhases)
            {
                skipReason = "not phased or already at last phase";
                return false;
            }

            if (!debuffState.ReadyForNextPhase)
            {
                skipReason = "ReadyForNextPhase=false";
                return false;
            }

            int oldPhase = debuffState.CurrentPhase;
            string oldBuff = _injuryManager.GetPhaseBuffId(injuryId, oldPhase);

            _treatmentManager.AdvanceInjuryToNextPhase(injuryId);
            _injuryManager.EnsureTreatmentBuffForInjury(injuryId);

            var updatedState = _stateManager.GetDebuffState(injuryId);
            int newPhase = updatedState?.CurrentPhase ?? oldPhase;
            string newBuff = updatedState != null
                ? _injuryManager.GetPhaseBuffId(injuryId, newPhase)
                : "(unknown)";

            ClearInjuryActionTopics(injuryId, TopicIds.GetAdvancePhaseTopic(injuryId, oldPhase + 1));
            GrantMedicalFriendship(10);
            ShowHarveyEmote(HarveyHelper.GetRecoveryEmote());
            _complianceManager.ApplyTreatmentComplianceTopics();
            _selfCareManager.OnHarveyMedicalVisit();
            _stateManager.Save();

            _monitor.Log(
                $"[MedicalAction] applied AdvancePhase injury={injuryId} oldBuff={oldBuff} newBuff={newBuff} " +
                $"phase={oldPhase}->{newPhase} fromDialogue={fromDialogueAction}",
                LogLevel.Info);
            _doctorVisitReminderManager.SyncReminderBuff();
            _recoveryPlanManager.RefreshPlanForToday(notifyUpdated: true);
            return true;
        }

        public bool TryCompleteRecovery(string injuryId, bool fromDialogueAction, out string? skipReason)
        {
            skipReason = null;
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                skipReason = $"state {injuryId} not found";
                return false;
            }

            if (!debuffState.ReadyForRecovery)
            {
                skipReason = "ReadyForRecovery=false";
                return false;
            }

            int today = (int)Game1.stats.DaysPlayed;
            _checkupManager.CompleteCheckup(injuryId, debuffState, today);

            _treatmentManager.ApplyMechanicalPhasedRecovery(injuryId);
            _rehabManager.TryStartRehabAfterRecovery(injuryId);
            CancelMedicalLettersForInjury(injuryId);
            Game1.addHUDMessage(new HUDMessage(
                "Выздоровление завершено! Харви гордится тобой!",
                HUDMessage.achievement_type));

            ClearInjuryActionTopics(injuryId, TopicIds.GetCompleteRecoveryTopic(injuryId));
            GrantMedicalFriendship(15);
            _complianceManager.ApplyTreatmentComplianceTopics();
            _selfCareManager.OnHarveyMedicalVisit();
            _stateManager.Save();

            _monitor.Log(
                $"[MedicalAction] applied CompleteRecovery injury={injuryId} fromDialogue={fromDialogueAction}",
                LogLevel.Info);
            _doctorVisitReminderManager.SyncReminderBuff();
            _recoveryPlanManager.NotifyTreatmentCompleted();
            _recoveryPlanManager.RefreshPlanForToday();
            return true;
        }

        private void ClearInjuryActionTopics(string injuryId, string appliedTopic)
        {
            foreach (string topic in TopicIds.GetAllActionTopicsForInjury(injuryId))
            {
                if (!string.Equals(topic, appliedTopic, StringComparison.OrdinalIgnoreCase))
                    _dialogueManager.RemoveTopicIfOwned(topic, "action applied");
            }

            _dialogueManager.RemoveTopicIfOwned(TopicIds.GetFestivalDeferTopic(injuryId), "action applied");
        }

        private bool HasBaseInjuryBuff(string injuryId) => _buffManager.HasBuff(injuryId);

        private void GrantMedicalFriendship(int points)
        {
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null)
                Game1.player.changeFriendship(points, harvey);
        }

        private void ShowHarveyEmote(int emoteId)
        {
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null)
                _dialogueManager.ShowEmote(harvey, emoteId);
        }

        private bool ShouldRequireHospitalization(string injuryId, DebuffState state)
        {
            if (!_config.ForceHospitalization)
                return false;
            if (_hospitalizationManager.IsHospitalized)
                return false;

            string? mainInjuryId = _injuryManager.GetActiveInjury();
            if (string.IsNullOrEmpty(mainInjuryId))
                return false;

            if (!string.Equals(injuryId, mainInjuryId, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!_injuryManager.HasInjuryOrPhase(mainInjuryId))
                return false;

            if (!InjuryManager.IsSeriousMainInjuryId(mainInjuryId))
                return false;

            int today = GameUtils.Today();

            if (string.Equals(mainInjuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase))
                return true;

            if (_injuryManager.HasSeriousMainInjuryWithDirtyWound())
            {
                if (mainInjuryId is "buffBurnWounds" or "buffShrapnelWounds" or "buffSurgicalWound")
                    return true;
            }

            return mainInjuryId switch
            {
                "buffConcussion" => true,
                "buffFracturedBone" => true,
                "buffBadlyHurt" => today - state.InjuryStartDay <= 1,
                "buffShrapnelWounds" => IsShrapnelMineOrExplosionRelated(state),
                _ => false
            };
        }

        private bool IsShrapnelMineOrExplosionRelated(DebuffState state)
        {
            if (GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue))
                return true;
            if (GameUtils.HasConversationTopic(ConversationTopics.ShrapnelWounds))
                return true;
            if (_stateManager.State.PassedOutInMineYesterday || _stateManager.State.NeedsMineRescueEvent)
                return true;
            if (!string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId))
                return true;
            if (_stateManager.WasStoryTriggerApplied(Triggers.ShrapnelWounds))
                return true;

            return false;
        }

        private static string GetTreatmentHospitalizationReason(string injuryId, DebuffState state)
        {
            if (injuryId == "buffShrapnelWounds"
                && GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue))
            {
                return "mine_rescue";
            }

            return injuryId switch
            {
                "buffConcussion" => "concussion_observation",
                "buffInfectedWound" => "infection_fever",
                "buffFracturedBone" => "fracture_stabilization",
                _ => "general"
            };
        }
    }
}
