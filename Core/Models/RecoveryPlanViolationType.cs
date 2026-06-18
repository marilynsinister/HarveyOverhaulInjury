namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>Типы нарушений плана восстановления для CP-диалогов (один главный topic в день).</summary>
    public static class RecoveryPlanViolationType
    {
        public const string Mine = "Mine";
        public const string LowStamina = "LowStamina";
        public const string LowStaminaWarning = "LowStaminaWarning";
        public const string LowStaminaFail = "LowStaminaFail";
        public const string LowHealth = "LowHealth";
        public const string LowHealthWarning = "LowHealthWarning";
        public const string LowHealthFail = "LowHealthFail";
        public const string LateNight = "LateNight";
        public const string Rain = "Rain";
        public const string IgnoredCheckup = "IgnoredCheckup";
        public const string MissedHarveyTalk = "MissedHarveyTalk";
        public const string LeftSafePlace = "LeftSafePlace";
        public const string AloneTooLong = "AloneTooLong";
        public const string PassedOut = "PassedOut";
    }

    /// <summary>Итог завершения всего плана восстановления.</summary>
    public static class RecoveryPlanCompletionResult
    {
        public const string Perfect = "Perfect";
        public const string WithWarnings = "WithWarnings";
        public const string Normal = "Normal";
    }
}
