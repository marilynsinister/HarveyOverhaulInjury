namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>Политика медицинских писем Харви (не story/romantic).</summary>
    public enum MedicalLetterMode
    {
        /// <summary>Не отправлять медицинские письма из C#.</summary>
        Off,

        /// <summary>Только критические fallback-письма с проверкой актуальности.</summary>
        CriticalOnly,

        /// <summary>Все медицинские письма, но с проверкой актуальности перед отправкой.</summary>
        All,
    }
}
