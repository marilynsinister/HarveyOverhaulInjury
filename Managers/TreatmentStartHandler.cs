using System;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
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
            RecoveryPlanManager recoveryPlanManager)
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
        }

        public void RegisterTriggerActions()
        {
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.StartTreatment,
                OnDialogueStartTreatmentAction);
            TriggerActionManager.RegisterAction(
                TreatmentStartActions.TreatComplication,
                OnDialogueTreatComplicationAction);
            _monitor.Log(
                $"[TreatmentStart] Registered trigger actions: {TreatmentStartActions.StartTreatment}, " +
                $"{TreatmentStartActions.TreatComplication}",
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

            _monitor.Log(
                $"[TreatmentStart] StartTreatment action вызван из dialogue injury={injuryId}",
                LogLevel.Info);

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState != null)
                debuffState.TreatmentIntroShown = true;

            return TryStartTreatment(injuryId, fromDialogueAction: true, out error);
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

            _monitor.Log(
                $"[ComplicationTreatment] TreatComplication action вызван из dialogue complication={complicationId}",
                LogLevel.Info);

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
            _dialogueManager.ClearTreatmentNeededTopic(injuryId, "лечение успешно начато");

            var stillActiveComplications = _complicationManager.GetActiveTreatableComplicationIds().ToList();
            if (stillActiveComplications.Count > 0)
                _treatmentManager.TreatAllComplications(stillActiveComplications);

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
                $"[TreatmentStart] applied injury={injuryId} fromDialogue={fromDialogueAction} complications={stillActiveComplications.Count}",
                LogLevel.Info);
            _doctorVisitReminderManager.SyncReminderBuff();
            _recoveryPlanManager.RefreshPlanForToday(notifyCreated: true);
            return true;
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
