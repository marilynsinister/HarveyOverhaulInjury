using HarveyOverhaul.InjuryCare.Core.Models;

namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>Conversation topics для CP-реплик Харви о нарушениях режима восстановления.</summary>
    public static class RecoveryViolationTopics
    {
        public const string Mild = "topicHarveyRecoveryViolationMild";
        public const string Medium = "topicHarveyRecoveryViolationMedium";
        public const string Severe = "topicHarveyRecoveryViolationSevere";

        public const int MildTopicDays = 1;
        public const int MediumTopicDays = 2;
        public const int SevereTopicDays = 3;

        /// <summary>Topic для CP по уровню нарушения; 0 — пустая строка.</summary>
        public static string GetRecoveryViolationTopic(int severity) => severity switch
        {
            RecoveryViolationSeverity.Mild => Mild,
            RecoveryViolationSeverity.Medium => Medium,
            RecoveryViolationSeverity.Severe => Severe,
            _ => "",
        };

        public static int GetTopicDays(int severity) => severity switch
        {
            RecoveryViolationSeverity.Mild => MildTopicDays,
            RecoveryViolationSeverity.Medium => MediumTopicDays,
            RecoveryViolationSeverity.Severe => SevereTopicDays,
            _ => 0,
        };
    }
}
