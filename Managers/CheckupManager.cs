using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Контрольные осмотры Харви между фазами фазовых травм.
    ///
    /// Ожидаемые CP conversation topics (Data/Conversations):
    /// - topicHarvey_CheckupDue
    /// - topicHarvey_CheckupDue_&lt;InjuryName&gt;  (DeepCuts, Concussion, …)
    /// - topicHarvey_CheckupPhase1 | Phase2 | Phase3
    /// - topicHarvey_RecoveryCheckupDue
    /// - topicHarvey_RecoveryCheckupDue_&lt;InjuryName&gt;
    /// </summary>
    public class CheckupManager
    {
        private const int CheckupTopicDays = 3;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly ComplianceManager _complianceManager;

        public CheckupManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            ComplianceManager complianceManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _complianceManager = complianceManager;
        }

        /// <summary>Первичная готовность к смене фазы — topics и счётчики.</summary>
        public void OnPhaseCheckupDue(string injuryId, DebuffState debuffState, int nextPhase, int today)
        {
            InitCheckupTracking(debuffState, today);

            _dialogueManager.AddTopic(CheckupTopics.CheckupDue, CheckupTopicDays);
            _dialogueManager.AddTopic(CheckupTopics.GetCheckupDueInjury(injuryId), CheckupTopicDays);
            _dialogueManager.AddTopic(CheckupTopics.GetCheckupPhase(nextPhase), CheckupTopicDays);

            _stateManager.Save();
            _monitor.Log(
                $"[Checkup] Phase due {injuryId} → phase {nextPhase} (topics × {CheckupTopicDays}d)",
                LogLevel.Info);
        }

        /// <summary>Первичная готовность к финальному осмотру.</summary>
        public void OnRecoveryCheckupDue(string injuryId, DebuffState debuffState, int today)
        {
            InitCheckupTracking(debuffState, today);

            _dialogueManager.AddTopic(CheckupTopics.RecoveryCheckupDue, CheckupTopicDays);
            _dialogueManager.AddTopic(CheckupTopics.GetRecoveryCheckupDueInjury(injuryId), CheckupTopicDays);

            _stateManager.Save();
            _monitor.Log(
                $"[Checkup] Recovery due {injuryId} (topics × {CheckupTopicDays}d)",
                LogLevel.Info);
        }

        /// <summary>Ежедневный учёт просрочки контрольного осмотра.</summary>
        public void ProcessMissedCheckupsDaily(int today)
        {
            foreach (var (injuryId, debuffState) in _stateManager.State.ActiveDebuffs)
            {
                if (debuffState.TotalPhases <= 0 || !debuffState.IsInTreatment)
                    continue;

                if (!debuffState.ReadyForNextPhase && !debuffState.ReadyForRecovery)
                    continue;

                if (debuffState.ReadySinceDay <= 0)
                    continue;

                debuffState.MissedCheckupDays = today - debuffState.ReadySinceDay;

                if (debuffState.MissedCheckupDays == 2 && !debuffState.CheckupReminderSent)
                {
                    debuffState.CheckupReminderSent = true;
                    string injuryName = GetInjuryDisplayName(injuryId);
                    Game1.addHUDMessage(new HUDMessage(
                        $"Харви ждёт тебя на контрольный осмотр ({injuryName}). Не откладывай лечение.",
                        HUDMessage.health_type));
                    _monitor.Log($"[Checkup] Soft reminder day 2: {injuryId}", LogLevel.Info);
                    if (HarveyMailHelper.TryScheduleTieredMail(
                        _config,
                        _stateManager,
                        _monitor,
                        MailIds.CheckupReminder,
                        MailIds.CheckupReminder))
                    {
                        _monitor.Log($"[Checkup] Reminder mail scheduled ({injuryId})", LogLevel.Debug);
                    }
                    else if (_config.SendLetters
                        && HarveyMailHelper.WasSentToday(_stateManager, MailIds.CheckupReminder))
                    {
                        _monitor.Log(
                            $"[Checkup] Reminder mail уже отправлено сегодня — пропуск письма ({injuryId})",
                            LogLevel.Debug);
                    }
                }

                if (debuffState.MissedCheckupDays == 4 && !debuffState.CheckupLateLetterSent)
                {
                    debuffState.CheckupLateLetterSent = true;
                    HarveyMailHelper.TryScheduleTieredMail(
                        _config,
                        _stateManager,
                        _monitor,
                        MailIds.CheckupOverdue,
                        $"{MailIds.CheckupOverdue}:{injuryId}");
                    _monitor.Log($"[Checkup] Overdue letter scheduled: {injuryId}", LogLevel.Info);
                }

                if (debuffState.MissedCheckupDays >= 5 && !debuffState.CheckupOverduePenaltyApplied)
                {
                    debuffState.CheckupOverduePenaltyApplied = true;
                    _complianceManager.AddCompliance(-1, $"checkup_overdue:{injuryId}");
                    int strikes = _stateManager.IncrementNeglectStrikes(injuryId);
                    _monitor.Log(
                        $"[Checkup] Overdue penalty {injuryId}: TreatmentComplianceScore -1, NeglectStrikes={strikes}",
                        LogLevel.Warn);
                }
            }

            _stateManager.Save();
        }

        /// <summary>Игрок прошёл осмотр (смена фазы или выздоровление).</summary>
        public void CompleteCheckup(string injuryId, DebuffState debuffState, int today)
        {
            if (debuffState.ReadySinceDay > 0)
            {
                int missed = debuffState.MissedCheckupDays > 0
                    ? debuffState.MissedCheckupDays
                    : today - debuffState.ReadySinceDay;

                if (missed <= 1)
                    _complianceManager.AddCompliance(+1, $"checkup_on_time:{injuryId}");
                else if (missed >= 3)
                    _complianceManager.AddCompliance(-1, $"checkup_late:{injuryId}");
            }

            RemoveCheckupTopics(injuryId, debuffState);
            ClearCheckupTracking(debuffState);
            _stateManager.Save();
            _monitor.Log($"[Checkup] Completed for {injuryId}", LogLevel.Debug);
        }

        /// <summary>Debug: выставить Ready* и topics как при CheckInjuryPhases.</summary>
        public bool DebugForceCheckupDue(string buffId)
        {
            var debuffState = _stateManager.GetDebuffState(buffId);
            if (debuffState == null)
            {
                _monitor.Log($"[Checkup] debug: {buffId} не найден в ActiveDebuffs", LogLevel.Warn);
                return false;
            }

            if (debuffState.TotalPhases <= 0)
            {
                _monitor.Log($"[Checkup] debug: {buffId} не фазовая травма", LogLevel.Warn);
                return false;
            }

            if (!debuffState.IsInTreatment)
            {
                _monitor.Log($"[Checkup] debug: {buffId} — лечение не начато", LogLevel.Warn);
                return false;
            }

            int today = (int)Game1.stats.DaysPlayed;

            if (debuffState.IsLastPhase)
            {
                debuffState.ReadyForNextPhase = false;
                debuffState.ReadyForRecovery = true;
                OnRecoveryCheckupDue(buffId, debuffState, today);
                _monitor.Log($"[Checkup] debug: {buffId} → ReadyForRecovery", LogLevel.Info);
            }
            else
            {
                debuffState.ReadyForRecovery = false;
                debuffState.ReadyForNextPhase = true;
                OnPhaseCheckupDue(buffId, debuffState, debuffState.CurrentPhase + 1, today);
                _monitor.Log(
                    $"[Checkup] debug: {buffId} → ReadyForNextPhase (→ phase {debuffState.CurrentPhase + 1})",
                    LogLevel.Info);
            }

            _stateManager.UpdateDebuffState(buffId, debuffState);
            return true;
        }

        public static void ClearCheckupTracking(DebuffState debuffState)
        {
            debuffState.ReadySinceDay = -1;
            debuffState.MissedCheckupDays = 0;
            debuffState.CheckupReminderSent = false;
            debuffState.CheckupLateLetterSent = false;
            debuffState.CheckupOverduePenaltyApplied = false;
        }

        private static void InitCheckupTracking(DebuffState debuffState, int today)
        {
            debuffState.ReadySinceDay = today;
            debuffState.MissedCheckupDays = 0;
            debuffState.CheckupReminderSent = false;
            debuffState.CheckupLateLetterSent = false;
            debuffState.CheckupOverduePenaltyApplied = false;
        }

        public void RemoveAllCheckupTopicsForInjury(string injuryId, int totalPhases = 0)
        {
            if (totalPhases <= 0)
            {
                var debuffState = _stateManager.GetDebuffState(injuryId);
                totalPhases = debuffState?.TotalPhases ?? InjurySets.InferDefaultTotalPhases(injuryId);
            }

            _dialogueManager.RemoveTopic(CheckupTopics.CheckupDue);
            _dialogueManager.RemoveTopic(CheckupTopics.GetCheckupDueInjury(injuryId));
            _dialogueManager.RemoveTopic(CheckupTopics.RecoveryCheckupDue);
            _dialogueManager.RemoveTopic(CheckupTopics.GetRecoveryCheckupDueInjury(injuryId));

            for (int phase = 1; phase <= Math.Max(1, totalPhases); phase++)
                _dialogueManager.RemoveTopic(CheckupTopics.GetCheckupPhase(phase));
        }

        private void RemoveCheckupTopics(string injuryId, DebuffState debuffState) =>
            RemoveAllCheckupTopicsForInjury(injuryId, debuffState.TotalPhases);

        private static string GetInjuryDisplayName(string injuryId) =>
            injuryId.Replace("buff", "", StringComparison.OrdinalIgnoreCase);
    }
}
