namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>Тяжесть нарушения режима восстановления (числовые уровни для save-state).</summary>
    public static class RecoveryViolationSeverity
    {
        public const int None = 0;
        public const int Mild = 1;
        public const int Medium = 2;
        public const int Severe = 3;
    }
}
