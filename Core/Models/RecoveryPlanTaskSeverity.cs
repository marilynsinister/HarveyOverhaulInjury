namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>Важность пункта плана или нарушения.</summary>
    public enum RecoveryPlanTaskSeverity
    {
        /// <summary>Информационная рекомендация.</summary>
        Info = 0,

        /// <summary>Предупреждение — лучше соблюдать.</summary>
        Warning = 1,

        /// <summary>Опасное нарушение или критичное правило.</summary>
        Danger = 2,
    }
}
