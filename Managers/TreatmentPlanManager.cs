using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// План лечения от Харви: письмо на следующий день, topics и HUD после начала лечения.
    /// </summary>
    public class TreatmentPlanManager
    {
        private const int TopicDays = 3;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;

        private static readonly Dictionary<string, string> InjuryMailMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["buffConcussion"] = MailIds.TreatmentPlanConcussion,
                ["buffFracturedBone"] = MailIds.TreatmentPlanFracture,
                ["buffBurnWounds"] = MailIds.TreatmentPlanBurn,
                ["buffInfectedWound"] = MailIds.TreatmentPlanInfection,
                [InjuryBuffs.Cold] = MailIds.TreatmentPlanCold,
                ["buffBadlyHurt"] = MailIds.TreatmentPlanSevere,
                ["buffShrapnelWounds"] = MailIds.TreatmentPlanSevere,
                ["buffTornMuscles"] = MailIds.TreatmentPlanSevere,
                ["buffBruisedRibs"] = MailIds.TreatmentPlanSevere,
                ["buffSurgicalWound"] = MailIds.TreatmentPlanSevere,
                ["buffHurt"] = MailIds.TreatmentPlanMinor,
                ["buffSprainedAnkle"] = MailIds.TreatmentPlanMinor,
                ["buffBackStrain"] = MailIds.TreatmentPlanMinor,
                ["buffDeepCuts"] = MailIds.TreatmentPlanMinor,
            };

        public TreatmentPlanManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
        }

        public void SendTreatmentPlanForInjury(string injuryId)
        {
            if (string.IsNullOrWhiteSpace(injuryId))
                return;

            string mailBaseId = ResolveMailId(injuryId);

            _dialogueManager.AddTopic(TreatmentPlanTopics.Given, TopicDays);
            _dialogueManager.AddTopic(TreatmentPlanTopics.GetInjuryTopic(injuryId), TopicDays);

            Game1.addHUDMessage(new HUDMessage(
                "Харви составил план лечения. Завтра он пришлёт записку с рекомендациями.",
                HUDMessage.health_type));

            string dedupeKey = $"{mailBaseId}:{injuryId}";
            if (HarveyMailHelper.TryScheduleTieredMail(_config, _stateManager, _monitor, mailBaseId, dedupeKey))
            {
                _monitor.Log(
                    $"[TreatmentPlan] Письмо {HarveyMailHelper.BuildRelationshipMailId(mailBaseId)} запланировано ({injuryId})",
                    LogLevel.Info);
            }
            else
            {
                _monitor.Log($"[TreatmentPlan] SendLetters=false, письмо пропущено ({injuryId})", LogLevel.Debug);
            }

            _monitor.Log(
                $"[TreatmentPlan] topics: {TreatmentPlanTopics.Given}, {TreatmentPlanTopics.GetInjuryTopic(injuryId)}",
                LogLevel.Debug);
        }

        public static string ResolveMailId(string injuryId)
        {
            if (InjuryMailMap.TryGetValue(injuryId, out string? mailId))
                return mailId;

            if (InjurySets.Severe.Contains(injuryId))
                return MailIds.TreatmentPlanSevere;

            return MailIds.TreatmentPlanMinor;
        }
    }
}
