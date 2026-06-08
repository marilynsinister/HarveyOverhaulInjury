using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// DTO для UI «Плана восстановления» (без StardewUI-привязок).
    /// Объединяет сохранённый post-hospital план и вычисленный контекст лечения.
    /// </summary>
    public sealed class RecoveryPlanViewModel
    {
        public static RecoveryPlanViewModel Empty { get; } = new();

        public bool HasPlan { get; init; }

        public bool IsActive { get; init; }

        public string PlanId { get; init; } = "";

        public string Reason { get; init; } = "";

        public string? InjuryId { get; init; }

        public int StartDay { get; init; } = -1;

        public int RequiredDays { get; init; }

        public int CompletedDays { get; init; }

        public int DaysRemaining { get; init; }

        public bool TodayFailed { get; init; }

        public bool TodayCompleted { get; init; }

        public int ViolationsToday { get; init; }

        public int TotalViolations { get; init; }

        public IReadOnlyList<string> TodayViolationReasons { get; init; } = new List<string>();

        public bool RequiresHarveyTalk { get; init; }

        public bool CompletionTalkPending { get; init; }

        public int LastEvaluatedDay { get; init; } = -1;

        // --- Вычисленный контекст лечения ---

        public RecoveryPlanMoodStatus Status { get; init; } = RecoveryPlanMoodStatus.None;

        public string StatusText { get; init; } = "";

        public string StatusDescription { get; init; } = "";

        public string DayProgressText { get; init; } = "";

        public string InjuryDisplayName { get; init; } = "";

        public string PhaseLabel { get; init; } = "";

        public string WhyImportant { get; init; } = "";

        public string ComplicationSummary { get; init; } = "";

        public int ConcernScore { get; init; }

        public string? MainInjuryId { get; init; }

        public int CurrentPhase { get; init; }

        public int TotalPhases { get; init; }

        public bool ReadyForNextPhase { get; init; }

        public bool ReadyForRecovery { get; init; }

        public IReadOnlyList<RecoveryPlanTask> Tasks { get; init; } = new List<RecoveryPlanTask>();

        public IReadOnlyList<RecoveryPlanViolation> Violations { get; init; } = new List<RecoveryPlanViolation>();

        /// <summary>Короткий статус режима («режим соблюдается», «нужен осмотр»).</summary>
        public string RegimeStatusText { get; init; } = "";

        /// <summary>Блок «Тон Харви» для UI.</summary>
        public HarveyToneViewModel HarveyTone { get; init; } = HarveyToneViewModel.Empty;
    }
}
