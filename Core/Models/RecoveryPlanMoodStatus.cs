namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Эмоциональный тон плана восстановления для UI и save-state (не бафф).
    /// </summary>
    public enum RecoveryPlanMoodStatus
    {
        /// <summary>План не активен или контекста лечения нет.</summary>
        None = 0,

        /// <summary>Режим соблюдается, без срочных действий.</summary>
        Calm = 1,

        /// <summary>Есть риски или нарушения — Харви насторожен.</summary>
        HarveyConcerned = 2,

        /// <summary>Нужен визит или разговор с Харви.</summary>
        NeedsHarveyTalk = 3,

        /// <summary>Критическая ситуация — срочные действия.</summary>
        Urgent = 4,
    }
}
