namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Зафиксированное нарушение режима восстановления за день (save-state).
    /// </summary>
    public class RecoveryPlanViolation
    {
        /// <summary>Уникальный идентификатор записи нарушения.</summary>
        public string Id { get; set; } = "";

        /// <summary>Машинный код причины (например, <c>mine</c>).</summary>
        public string Reason { get; set; } = "";

        /// <summary>Игровой день нарушения.</summary>
        public int Day { get; set; } = -1;

        /// <summary>Игровое время (формат Stardew, например 1430).</summary>
        public int TimeOfDay { get; set; } = -1;

        /// <summary>Локация игрока в момент нарушения.</summary>
        public string LocationName { get; set; } = "";

        /// <summary>Серьёзность нарушения.</summary>
        public RecoveryPlanTaskSeverity Severity { get; set; } = RecoveryPlanTaskSeverity.Warning;
    }
}
