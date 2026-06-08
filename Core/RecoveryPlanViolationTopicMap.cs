using System;
using HarveyOverhaul.InjuryCare.Core.Models;

namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>Маппинг типов нарушений плана восстановления в conversation topics для Content Patcher.</summary>
    public static class RecoveryPlanViolationTopicMap
    {
        public const int ViolatedTopicDays = 2;
        public const int CompletedTopicDays = 3;

        /// <summary>Приоритет типа при выборе главного диалога за день (выше — важнее).</summary>
        public static int GetTypePriority(string violationType) => violationType switch
        {
            RecoveryPlanViolationType.Mine => 30,
            RecoveryPlanViolationType.LowHealth => 30,
            RecoveryPlanViolationType.LateNight => 20,
            RecoveryPlanViolationType.LowStamina => 10,
            RecoveryPlanViolationType.Rain => 10,
            RecoveryPlanViolationType.IgnoredCheckup => 25,
            RecoveryPlanViolationType.PassedOut => 35,
            _ => 0,
        };

        /// <summary>Topic для CP; неизвестный тип → общий fallback.</summary>
        public static string GetViolationTopic(string violationType) => violationType switch
        {
            RecoveryPlanViolationType.Mine => ConversationTopics.RecoveryPlanViolatedMine,
            RecoveryPlanViolationType.LowStamina => ConversationTopics.RecoveryPlanViolatedLowStamina,
            RecoveryPlanViolationType.LowHealth => ConversationTopics.RecoveryPlanViolatedLowHealth,
            RecoveryPlanViolationType.LateNight => ConversationTopics.RecoveryPlanViolatedLateNight,
            RecoveryPlanViolationType.Rain => ConversationTopics.RecoveryPlanViolatedRain,
            RecoveryPlanViolationType.IgnoredCheckup => ConversationTopics.RecoveryPlanViolated,
            RecoveryPlanViolationType.PassedOut => ConversationTopics.RecoveryPlanViolated,
            _ => ConversationTopics.RecoveryPlanViolated,
        };

        /// <summary>Topic по тяжести нарушения (mild / medium / severe).</summary>
        public static string GetSeverityTopic(int severity) => severity switch
        {
            RecoveryViolationSeverity.Mild => ConversationTopics.RecoveryPlanViolatedMild,
            RecoveryViolationSeverity.Medium => ConversationTopics.RecoveryPlanViolatedMedium,
            RecoveryViolationSeverity.Severe => ConversationTopics.RecoveryPlanViolatedSevere,
            _ => "",
        };

        public static string GetCompletionTopic(string result) => result switch
        {
            RecoveryPlanCompletionResult.Perfect => ConversationTopics.RecoveryPlanPerfect,
            RecoveryPlanCompletionResult.WithWarnings => ConversationTopics.RecoveryPlanCompletedWithWarnings,
            RecoveryPlanCompletionResult.Normal => ConversationTopics.RecoveryPlanCompletedNormal,
            _ => ConversationTopics.RecoveryPlanCompleted,
        };

        /// <summary>Привести reason ID или alias к типу нарушения.</summary>
        public static string ResolveViolationType(string typeOrReasonId)
        {
            if (string.IsNullOrWhiteSpace(typeOrReasonId))
                return "";

            string trimmed = typeOrReasonId.Trim();
            if (IsKnownViolationType(trimmed))
                return trimmed;

            string canonical = RecoveryPlanViolationReasonTexts.CanonicalizeReasonId(trimmed.ToLowerInvariant());
            return canonical switch
            {
                RecoveryPlanReasonIds.EnteredMine
                    or RecoveryPlanReasonIds.EnteredVolcano => RecoveryPlanViolationType.Mine,

                RecoveryPlanReasonIds.StaminaTooLow
                    or RecoveryPlanReasonIds.HeavyWork => RecoveryPlanViolationType.LowStamina,

                RecoveryPlanReasonIds.HealthTooLow => RecoveryPlanViolationType.LowHealth,

                RecoveryPlanReasonIds.TooLate => RecoveryPlanViolationType.LateNight,

                RecoveryPlanReasonIds.RainBandage => RecoveryPlanViolationType.Rain,

                RecoveryPlanReasonIds.MissedHarveyCheckup => RecoveryPlanViolationType.IgnoredCheckup,

                "passout" or "passed_out" or "pass_out" => RecoveryPlanViolationType.PassedOut,

                _ => "",
            };
        }

        /// <summary>Человекочитаемая причина для TodayViolationReasons / лога.</summary>
        public static string GetReadableReason(string violationType, string? overrideText = null)
        {
            if (!string.IsNullOrWhiteSpace(overrideText))
                return overrideText.Trim();

            return violationType switch
            {
                RecoveryPlanViolationType.Mine => "ты вошла в шахту с активным режимом",
                RecoveryPlanViolationType.LowStamina => "stamina упала слишком низко",
                RecoveryPlanViolationType.LowHealth => "здоровье упало слишком низко",
                RecoveryPlanViolationType.LateNight => "ты легла слишком поздно",
                RecoveryPlanViolationType.Rain => "повязка промокла под дождём",
                RecoveryPlanViolationType.IgnoredCheckup => "пропущен контрольный осмотр у Харви",
                RecoveryPlanViolationType.PassedOut => "обморок от истощения",
                _ => "нарушение режима",
            };
        }

        /// <summary>true, если новое нарушение должно заменить текущий главный диалог дня.</summary>
        public static bool ShouldReplaceDialogue(
            string currentType,
            int currentSeverity,
            string newType,
            int newSeverity)
        {
            if (string.IsNullOrEmpty(currentType))
                return true;

            if (newSeverity > currentSeverity)
                return true;

            if (newSeverity < currentSeverity)
                return false;

            return GetTypePriority(newType) > GetTypePriority(currentType);
        }

        /// <summary>Тяжёлое нарушение сразу ломает день; лёгкое — только предупреждение.</summary>
        public static bool IsWarningOnly(string violationType, int severity)
        {
            if (severity >= RecoveryViolationSeverity.Medium)
                return false;

            return violationType is RecoveryPlanViolationType.LowStamina
                or RecoveryPlanViolationType.Rain;
        }

        public static bool IsAlwaysSerious(string violationType) =>
            violationType is RecoveryPlanViolationType.Mine
                or RecoveryPlanViolationType.LowHealth;

        private static bool IsKnownViolationType(string value) =>
            string.Equals(value, RecoveryPlanViolationType.Mine, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, RecoveryPlanViolationType.LowStamina, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, RecoveryPlanViolationType.LowHealth, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, RecoveryPlanViolationType.LateNight, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, RecoveryPlanViolationType.Rain, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, RecoveryPlanViolationType.IgnoredCheckup, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, RecoveryPlanViolationType.PassedOut, StringComparison.OrdinalIgnoreCase);
    }
}
