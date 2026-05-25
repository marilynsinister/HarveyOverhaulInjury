using System;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>Уровень соблюдения режима лечения (не Friendship и не романтическое доверие).</summary>
    public enum ComplianceLevel
    {
        /// <summary>Нарушает режим лечения (TreatmentComplianceScore ≤ −5).</summary>
        Low,
        /// <summary>Нестабильное соблюдение лечения.</summary>
        Neutral,
        /// <summary>Хорошо соблюдает лечение (TreatmentComplianceScore ≥ 5).</summary>
        High,
    }

    /// <summary>
    /// Медицинский показатель: насколько стабильно игрок соблюдает лечение.
    /// Не влияет на Friendship с Harvey; только тон, topics, предписания, риски и мягкие бонусы.
    /// </summary>
    public class ComplianceManager
    {
        public const int MinScore = -10;
        public const int MaxScore = 10;
        public const int HighThreshold = 5;
        public const int LowThreshold = -5;

        private const int TreatmentTopicDays = 2;
        private const int TrustedPatientTopicDays = 3;

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly BuffManager _buffManager;

        public ComplianceManager(
            IMonitor monitor,
            StateManager stateManager,
            DialogueManager dialogueManager,
            BuffManager buffManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _buffManager = buffManager;
        }

        public int Score => _stateManager.State.TreatmentComplianceScore;

        public bool IsHighCompliance => Score >= HighThreshold;

        public bool IsLowCompliance => Score <= LowThreshold;

        public ComplianceLevel GetComplianceLevel()
        {
            if (IsHighCompliance) return ComplianceLevel.High;
            if (IsLowCompliance) return ComplianceLevel.Low;
            return ComplianceLevel.Neutral;
        }

        public void ClampScoreOnLoad()
        {
            var state = _stateManager.State;
            int clamped = Math.Clamp(state.TreatmentComplianceScore, MinScore, MaxScore);
            if (clamped != state.TreatmentComplianceScore)
            {
                state.TreatmentComplianceScore = clamped;
                _monitor.Log(
                    $"TreatmentComplianceScore clamped to {clamped} (legacy save)",
                    LogLevel.Debug);
            }
        }

        /// <summary>
        /// Медицинский показатель соблюдения лечения.
        /// TreatmentComplianceScore is a medical adherence metric. It must never reduce Harvey friendship.
        /// </summary>
        public void AddCompliance(int delta, string reason)
        {
            if (delta == 0)
                return;

            var state = _stateManager.State;
            int oldScore = state.TreatmentComplianceScore;
            state.TreatmentComplianceScore = Math.Clamp(oldScore + delta, MinScore, MaxScore);

            _stateManager.Save();
            _monitor.Log(
                $"TreatmentComplianceScore {oldScore} → {state.TreatmentComplianceScore} ({delta:+0;-0;0}, {reason})",
                LogLevel.Info);
        }

        public void SetScore(int score)
        {
            _stateManager.State.TreatmentComplianceScore = Math.Clamp(score, MinScore, MaxScore);
            _stateManager.Save();
        }

        /// <summary>Топики для CP-диалогов при лечении (не меняет PickTreatmentDialogue).</summary>
        public void ApplyTreatmentComplianceTopics()
        {
            _dialogueManager.RemoveTopic(ComplianceTopics.High);
            _dialogueManager.RemoveTopic(ComplianceTopics.Low);
            _dialogueManager.RemoveTopic(ComplianceTopics.Neutral);

            switch (GetComplianceLevel())
            {
                case ComplianceLevel.High:
                    _dialogueManager.AddTopic(ComplianceTopics.High, TreatmentTopicDays);
                    break;
                case ComplianceLevel.Low:
                    _dialogueManager.AddTopic(ComplianceTopics.Low, TreatmentTopicDays);
                    break;
                case ComplianceLevel.Neutral:
                    _dialogueManager.AddTopic(ComplianceTopics.Neutral, TreatmentTopicDays);
                    break;
            }
        }

        public void ApplyHighComplianceRecoveryBonuses()
        {
            if (!IsHighCompliance)
                return;

            if (!_buffManager.HasBuff(CureBuffs.Care))
                _buffManager.AddBuff(CureBuffs.Care, 2880000);

            _dialogueManager.AddTopic(ComplianceTopics.TrustedPatient, TrustedPatientTopicDays);
            _monitor.Log(
                "Хорошо соблюдает лечение: buffHarveyCare + topicHarvey_TrustedPatient",
                LogLevel.Info);
        }

        public void TryShowLowComplianceReminder()
        {
            if (!IsLowCompliance)
                return;

            if (!_stateManager.State.ActiveDebuffs.Values.Any(d => d.TreatmentStarted))
                return;

            int today = (int)Game1.stats.DaysPlayed;
            if (_stateManager.State.LastLowComplianceHudDay == today)
                return;

            _stateManager.State.LastLowComplianceHudDay = today;
            _stateManager.Save();

            Game1.addHUDMessage(new HUDMessage(
                "Харви всё чаще напоминает, что лечение нельзя бросать.",
                HUDMessage.error_type));
        }

        public void OnTimelyPhaseVisit(DebuffState debuffState, int today)
        {
            // Устарело: TreatmentComplianceScore за своевременный осмотр начисляет CheckupManager.CompleteCheckup.
        }

        public void CheckPhaseVisitDelays(int today)
        {
            // Устарело: просрочка осмотров обрабатывает CheckupManager.ProcessMissedCheckupsDaily.
        }

        public void OnPrescriptionCompletedWithoutViolations()
        {
            AddCompliance(+1, "prescription_completed");
        }

        public void OnCheckupVisit(int today)
        {
            if (_stateManager.State.LastCheckupComplianceDay == today)
                return;

            _stateManager.State.LastCheckupComplianceDay = today;
            AddCompliance(+1, "checkup_visit");
        }

        public void OnComplicationTreatedSameDay(int complicationStartDay, int today)
        {
            if (complicationStartDay != today)
                return;

            AddCompliance(+1, "complication_same_day");
        }

        public static string GetLevelDisplayName(ComplianceLevel level) =>
            level switch
            {
                ComplianceLevel.High => "хорошо соблюдает лечение",
                ComplianceLevel.Low => "нарушает режим лечения",
                _ => "нестабильное соблюдение лечения",
            };
    }
}
