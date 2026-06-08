using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Сохранённый режим восстановления после выписки из госпитализации (3 дня).
    /// Отдельно от ежедневного <see cref="RecoveryPlanState"/>.
    /// </summary>
    public class HospitalDischargePlanState
    {
        public bool IsActive { get; set; } = false;

        public string PlanId { get; set; } = "";

        public string Reason { get; set; } = "";

        public string? InjuryId { get; set; }

        public int StartDay { get; set; } = -1;

        public int RequiredDays { get; set; } = 0;

        public int CompletedDays { get; set; } = 0;

        public bool TodayFailed { get; set; } = false;

        public bool TodayCompleted { get; set; } = false;

        public int ViolationsToday { get; set; } = 0;

        public int TotalViolations { get; set; } = 0;

        public List<string> TodayViolationReasons { get; set; } = new();

        public bool RequiresHarveyTalk { get; set; } = false;

        public bool CompletionTalkPending { get; set; } = false;

        public int LastEvaluatedDay { get; set; } = -1;
    }
}
