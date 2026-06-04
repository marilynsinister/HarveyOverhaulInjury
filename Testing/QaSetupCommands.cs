using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Testing
{
    /// <summary>
    /// QA setup-команды: точечные мутации topics, complications, age-travel, hospital.
    /// </summary>
    internal static class QaSetupCommands
    {
        private const int DefaultOwnedTopicDays = 7;

        public static bool IsOwnedTopic(string topicId) =>
            ModTopicRegistry.GetAllOwnedTopicIds().Contains(topicId);

        public static int ResolveOwnedTopicDefaultDays(
            string topicId,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas,
            (string BuffId, string TopicId)[] knownComplications)
        {
            foreach (var trauma in knownTraumas)
            {
                if (string.Equals(trauma.TopicId, topicId, StringComparison.OrdinalIgnoreCase))
                {
                    int total = trauma.P1 + trauma.P2 + trauma.P3;
                    return total > 0 ? total : DefaultOwnedTopicDays;
                }
            }

            foreach (var comp in knownComplications)
            {
                if (string.Equals(comp.TopicId, topicId, StringComparison.OrdinalIgnoreCase))
                    return comp.BuffId == InjuryBuffs.Neglect ? 7 : 4;
            }

            return DefaultOwnedTopicDays;
        }

        public static bool TryAddOwnedTopic(
            DialogueManager dialogueManager,
            string topicId,
            int days)
        {
            dialogueManager.AddTopic(topicId, days);
            return dialogueManager.HasTopic(topicId);
        }

        public static bool TryRemoveTopic(DialogueManager dialogueManager, string topicId)
        {
            bool hadTopic = dialogueManager.HasTopic(topicId);
            if (hadTopic)
                dialogueManager.RemoveTopic(topicId);
            return hadTopic;
        }

        public static bool TryAgeInjury(InjuryState state, StateManager stateManager, string buffId, int daysBack, out string detail)
        {
            detail = string.Empty;
            if (!state.ActiveDebuffs.TryGetValue(buffId, out DebuffState? ds))
            {
                detail = $"buffId={buffId} not in ActiveDebuffs";
                return false;
            }

            int today = GameUtils.Today();
            int startDay = today - Math.Max(0, daysBack);
            ds.InjuryStartDay = startDay;
            ds.PhaseStartDay = startDay;
            stateManager.Save();
            detail = $"buffId={buffId} injuryStart={startDay} phaseStart={startDay} today={today}";
            return true;
        }

        public static bool TryAgeComplication(InjuryState state, StateManager stateManager, string compId, int daysBack, out string detail)
        {
            detail = string.Empty;
            if (!state.ActiveComplications.ContainsKey(compId))
            {
                detail = $"comp={compId} not in ActiveComplications";
                return false;
            }

            int today = GameUtils.Today();
            int startDay = today - Math.Max(0, daysBack);
            state.ActiveComplications[compId] = startDay;
            stateManager.Save();
            detail = $"comp={compId} startDay={startDay} today={today}";
            return true;
        }

        public static string BuildHospitalStatusReport(InjuryState state, HospitalizationManager hospitalizationManager)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"IsHospitalized={state.IsHospitalized}");
            sb.AppendLine($"HospitalizedInjuryId={Fmt(state.HospitalizedInjuryId)}");
            sb.AppendLine($"HospitalizationReason={Fmt(state.HospitalizationReason)}");
            sb.AppendLine($"HospitalAdmissionDay={state.HospitalAdmissionDay}");
            sb.AppendLine($"HospitalAdmissionTime={state.HospitalAdmissionTime}");
            sb.AppendLine($"HospitalAdmissionMinutes={state.HospitalAdmissionMinutes}");
            sb.AppendLine($"HospitalMinStayMinutes={state.HospitalMinStayMinutes}");
            sb.AppendLine($"HospitalDischargeReadyShown={state.HospitalDischargeReadyShown}");
            sb.AppendLine($"PendingForcedHospitalizationWarning={state.PendingForcedHospitalizationWarning}");
            sb.AppendLine($"PendingForcedHospitalizationWarningDay={state.PendingForcedHospitalizationWarningDay}");
            sb.AppendLine($"DaysWithSevere={state.DaysWithSevere}");
            sb.AppendLine($"PendingHospitalPassOutEventId={Fmt(state.PendingHospitalPassOutEventId)}");
            sb.AppendLine($"CanDischarge={hospitalizationManager.CanDischarge()}");
            sb.AppendLine($"HospitalElapsedMinutes={hospitalizationManager.HospitalElapsedMinutes}");
            sb.AppendLine($"DischargeAllowed={hospitalizationManager.DischargeAllowed}");
            sb.AppendLine($"LastHospitalClockMinutes={hospitalizationManager.LastHospitalClockMinutes}");
            return sb.ToString().TrimEnd();
        }

        public static bool IsKnownComplication(
            string complicationId,
            (string BuffId, string TopicId)[] knownComplications) =>
            knownComplications.Any(c => c.BuffId.Equals(complicationId, StringComparison.OrdinalIgnoreCase));

        private static string Fmt(string? value) =>
            string.IsNullOrEmpty(value) ? "(none)" : value;
    }
}
