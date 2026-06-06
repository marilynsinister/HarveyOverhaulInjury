using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// DTO для будущего UI «Плана восстановления» (без StardewUI-привязок).
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
    }
}
