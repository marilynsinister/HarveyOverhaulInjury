namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Один пункт «Плана восстановления» — объясняющая рекомендация, сохраняется в save-state.
    /// </summary>
    public class RecoveryPlanTask
    {
        /// <summary>Уникальный ключ задачи (например, <c>no_mine</c>).</summary>
        public string Id { get; set; } = "";

        /// <summary>Краткий заголовок для UI.</summary>
        public string Title { get; set; } = "";

        /// <summary>Развёрнутое описание правила.</summary>
        public string Description { get; set; } = "";

        /// <summary>Обязательное правило дня.</summary>
        public bool IsRequired { get; set; }

        /// <summary>Правило выполнено сегодня.</summary>
        public bool IsCompleted { get; set; }

        /// <summary>Правило нарушено сегодня.</summary>
        public bool IsFailed { get; set; }

        /// <summary>Важность пункта.</summary>
        public RecoveryPlanTaskSeverity Severity { get; set; } = RecoveryPlanTaskSeverity.Info;
    }
}
