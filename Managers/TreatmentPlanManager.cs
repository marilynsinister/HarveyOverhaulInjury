using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Services;
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
        private readonly MedicalLetterScheduler? _medicalLetterScheduler;

        /// <summary>
        /// Базовые mailId для tiered-пакетов в CP (mailHarveyMedicalTiered.json).
        /// Injury-specific письма — задел; пока Severe/Minor.
        /// </summary>
        private static readonly Dictionary<string, string> InjuryMailMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["buffConcussion"] = MailIds.TreatmentPlanSevere,
                ["buffFracturedBone"] = MailIds.TreatmentPlanSevere,
                ["buffBurnWounds"] = MailIds.TreatmentPlanSevere,
                ["buffInfectedWound"] = MailIds.TreatmentPlanSevere,
                ["buffShrapnelWounds"] = MailIds.TreatmentPlanSevere,
                ["buffSurgicalWound"] = MailIds.TreatmentPlanSevere,
                ["buffBadlyHurt"] = MailIds.TreatmentPlanSevere,
                ["buffTornMuscles"] = MailIds.TreatmentPlanSevere,
                ["buffBruisedRibs"] = MailIds.TreatmentPlanSevere,
                [InjuryBuffs.Cold] = MailIds.TreatmentPlanMinor,
                ["buffHurt"] = MailIds.TreatmentPlanMinor,
                ["buffSprainedAnkle"] = MailIds.TreatmentPlanMinor,
                ["buffBackStrain"] = MailIds.TreatmentPlanMinor,
                ["buffDeepCuts"] = MailIds.TreatmentPlanMinor,
            };

        public TreatmentPlanManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            MedicalLetterScheduler? medicalLetterScheduler = null)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _medicalLetterScheduler = medicalLetterScheduler;
        }

        public void SendTreatmentPlanForInjury(string injuryId)
        {
            if (string.IsNullOrWhiteSpace(injuryId))
                return;

            string mailBaseId = ResolveMailId(injuryId);

            _dialogueManager.AddTopic(TreatmentPlanTopics.Given, TopicDays);
            _dialogueManager.AddTopic(TreatmentPlanTopics.GetInjuryTopic(injuryId), TopicDays);

            Game1.addHUDMessage(new HUDMessage(
                "Харви составил план лечения. Подробности — в Recovery Plan и при разговоре с ним.",
                HUDMessage.health_type));

            if (_config.MedicalLetters == MedicalLetterMode.Off)
            {
                _monitor.Log($"[TreatmentPlan] MedicalLetters=Off, письмо пропущено ({injuryId})", LogLevel.Debug);
            }
            else if (_medicalLetterScheduler == null)
            {
                _monitor.Log($"[TreatmentPlan] scheduler missing, письмо пропущено ({injuryId})", LogLevel.Debug);
            }
            else if (HarveyMailHelper.WasSentToday(_stateManager, mailBaseId))
            {
                _monitor.Log(
                    $"[TreatmentPlan] Письмо {mailBaseId} уже отправлено сегодня для этого типа плана — пропуск ({injuryId})",
                    LogLevel.Debug);
            }
            else if (HarveyMailHelper.TryScheduleTieredMail(
                _medicalLetterScheduler,
                mailBaseId,
                MedicalLetterReasons.TreatmentPlan,
                injuryId,
                critical: false,
                mailBaseId))
            {
                _monitor.Log(
                    $"[TreatmentPlan] Письмо {HarveyMailHelper.BuildRelationshipMailId(mailBaseId)} запланировано ({injuryId})",
                    LogLevel.Info);
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
