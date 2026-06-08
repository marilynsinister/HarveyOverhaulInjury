namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Эмоциональный тон Харви по отношению к соблюдению режима восстановления (UI, не шкала).
    /// </summary>
    public enum HarveyCareTone
    {
        /// <summary>Режим соблюдается, доверие спокойное.</summary>
        Calm = 0,

        /// <summary>Есть нарушения или нужен осмотр — Харви тревожится.</summary>
        Worried = 1,

        /// <summary>Реальный риск здоровью — Харви настаивает.</summary>
        Strict = 2,
    }
}
