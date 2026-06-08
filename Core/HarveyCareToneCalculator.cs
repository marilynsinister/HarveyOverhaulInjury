using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;

namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>
    /// Вычисление тона Харви из состояния плана восстановления (без хранения в save-state).
    /// </summary>
    public static class HarveyCareToneCalculator
    {
        /// <summary>Порог TotalViolations, после которого тон становится строгим.</summary>
        public const int StrictTotalViolationsThreshold = 4;

        private static readonly string[] StrictViolationTypes =
        {
            RecoveryPlanViolationType.Mine,
            RecoveryPlanViolationType.PassedOut,
        };

        /// <summary>Контекст текущего мира для runtime-проверок (HP, локация).</summary>
        public sealed class WorldContext
        {
            public bool IsAvailable { get; init; }

            public int PlayerHealth { get; init; }

            public int PlayerMaxHealth { get; init; }

            public bool InMineOrVolcano { get; init; }

            public static WorldContext Unavailable { get; } = new() { IsAvailable = false };
        }

        public static HarveyCareTone Calculate(
            RecoveryPlanState plan,
            int careTrust,
            bool needsHarveyVisitFromViolation,
            WorldContext? world = null)
        {
            if (!plan.IsActive)
                return HarveyCareTone.Calm;

            if (IsStrictTone(plan, needsHarveyVisitFromViolation, world))
                return HarveyCareTone.Strict;

            if (IsWorriedTone(plan, careTrust, needsHarveyVisitFromViolation))
                return HarveyCareTone.Worried;

            return HarveyCareTone.Calm;
        }

        public static HarveyToneViewModel BuildViewModel(HarveyCareTone tone, bool hasActivePlan)
        {
            if (!hasActivePlan)
            {
                return new HarveyToneViewModel
                {
                    HasTone = false,
                    Tone = HarveyCareTone.Calm,
                    Title = "",
                    Description = RecoveryPlanTexts.HarveyTone.NoActivePlan,
                    IconKey = "",
                    AccentColor = "#7f6139",
                };
            }

            return tone switch
            {
                HarveyCareTone.Strict => new HarveyToneViewModel
                {
                    HasTone = true,
                    Tone = HarveyCareTone.Strict,
                    Title = RecoveryPlanTexts.HarveyTone.StrictTitle,
                    Description = RecoveryPlanTexts.HarveyTone.StrictDescription,
                    IconKey = "strict",
                    AccentColor = "#8b4513",
                },
                HarveyCareTone.Worried => new HarveyToneViewModel
                {
                    HasTone = true,
                    Tone = HarveyCareTone.Worried,
                    Title = RecoveryPlanTexts.HarveyTone.WorriedTitle,
                    Description = RecoveryPlanTexts.HarveyTone.WorriedDescription,
                    IconKey = "worried",
                    AccentColor = "#a67c52",
                },
                _ => new HarveyToneViewModel
                {
                    HasTone = true,
                    Tone = HarveyCareTone.Calm,
                    Title = RecoveryPlanTexts.HarveyTone.CalmTitle,
                    Description = RecoveryPlanTexts.HarveyTone.CalmDescription,
                    IconKey = "calm",
                    AccentColor = "#5c7a5c",
                },
            };
        }

        private static bool IsStrictTone(
            RecoveryPlanState plan,
            bool needsHarveyVisitFromViolation,
            WorldContext? world)
        {
            if (plan.NeedsStrictFollowUp || plan.MaxExtensionsReached)
                return true;

            if (plan.TotalViolations >= StrictTotalViolationsThreshold)
                return true;

            if (HasTodayViolationOfTypes(plan, StrictViolationTypes))
                return true;

            if (HasTodaySeverity(plan, RecoveryViolationSeverity.Severe))
                return true;

            if (plan.LastViolationSeverity >= RecoveryViolationSeverity.Severe
                && HasAnyTodayViolation(plan))
                return true;

            if (world is { IsAvailable: true })
            {
                int criticalHp = (int)Math.Ceiling(world.PlayerMaxHealth * 0.15f);
                if (world.PlayerHealth > 0 && world.PlayerHealth <= criticalHp)
                    return true;

                if (world.InMineOrVolcano && HasAnyTodayViolation(plan))
                    return true;
            }

            return false;
        }

        private static bool IsWorriedTone(
            RecoveryPlanState plan,
            int careTrust,
            bool needsHarveyVisitFromViolation)
        {
            if (needsHarveyVisitFromViolation || plan.NeedsHarveyVisit)
                return true;

            if (plan.TodayViolationReasons.Count > 0 || plan.TodayViolationTypes.Count > 0)
                return true;

            if (plan.TodayViolations.Count > 0)
                return true;

            if (plan.TodayFailed || plan.HadWarningsToday)
                return true;

            if (HasTodaySeverity(plan, RecoveryViolationSeverity.Mild)
                || HasTodaySeverity(plan, RecoveryViolationSeverity.Medium))
                return true;

            if (plan.LastViolationSeverity >= RecoveryViolationSeverity.Mild && HasAnyTodayViolation(plan))
                return true;

            if (careTrust < 0)
                return true;

            int today = plan.LastUpdatedDay;
            if (plan.SoftToneUntilDay >= today && plan.SoftToneUntilDay >= 0)
                return false;

            return false;
        }

        private static bool HasAnyTodayViolation(RecoveryPlanState plan) =>
            plan.TodayViolationReasons.Count > 0
            || plan.TodayViolationTypes.Count > 0
            || plan.TodayViolations.Count > 0
            || plan.TodayFailed;

        private static bool HasTodayViolationOfTypes(RecoveryPlanState plan, string[] types)
        {
            foreach (string type in types)
            {
                if (plan.TodayViolationTypes.Any(t =>
                        string.Equals(t, type, StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (string.Equals(plan.LastViolationType, type, StringComparison.OrdinalIgnoreCase)
                    && HasAnyTodayViolation(plan))
                    return true;
            }

            return false;
        }

        private static bool HasTodaySeverity(RecoveryPlanState plan, int severity)
        {
            if (!HasAnyTodayViolation(plan))
                return false;

            if (plan.LastViolationSeverity == severity)
                return true;

            if (plan.TodayViolationDialogueSeverity == severity)
                return true;

            foreach (RecoveryPlanViolation violation in plan.TodayViolations)
            {
                int mapped = MapTaskSeverityToViolationSeverity(violation.Severity);
                if (mapped == severity)
                    return true;
            }

            return false;
        }

        private static int MapTaskSeverityToViolationSeverity(RecoveryPlanTaskSeverity taskSeverity) =>
            taskSeverity switch
            {
                RecoveryPlanTaskSeverity.Danger => RecoveryViolationSeverity.Severe,
                RecoveryPlanTaskSeverity.Warning => RecoveryViolationSeverity.Medium,
                _ => RecoveryViolationSeverity.Mild,
            };
    }
}
