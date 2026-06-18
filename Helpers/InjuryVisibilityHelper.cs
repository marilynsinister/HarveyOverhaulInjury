using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Скрытые травмы: физическое наличие vs знание Харви vs обнаружение при контакте.</summary>
    public static class InjuryVisibilityHelper
    {
        private static readonly string[] NeutralMineHudMessages =
        {
            "Рана саднит. В шахте легко занести грязь.",
            "Кровь снова проступила через ткань.",
            "Лучше бы показать это врачу.",
        };

        private static readonly Dictionary<string, InjuryVisibilityProfile> VisibilityProfiles = new(StringComparer.OrdinalIgnoreCase)
        {
            ["buffHurt"] = new()
            {
                BuffId = "buffHurt",
                BaseVisibility = InjuryVisibilityLevel.Subtle,
                CanBeHiddenFromHarvey = true,
                HarveyDetectionBonus = 1,
                VisibleSigns = new[] { "tired", "minor_pain" }
            },
            ["buffBadlyHurt"] = new()
            {
                BuffId = "buffBadlyHurt",
                BaseVisibility = InjuryVisibilityLevel.Obvious,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 3,
                VisibleSigns = new[] { "weakness", "pale", "pain_movement" }
            },
            ["buffDeepCuts"] = new()
            {
                BuffId = "buffDeepCuts",
                BaseVisibility = InjuryVisibilityLevel.Obvious,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 4,
                VisibleSigns = new[] { "blood", "guarding_wound", "pain_movement" }
            },
            ["buffBurnWounds"] = new()
            {
                BuffId = "buffBurnWounds",
                BaseVisibility = InjuryVisibilityLevel.Obvious,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 4,
                VisibleSigns = new[] { "burns", "pain_touch", "stiff_movement" }
            },
            ["buffSprainedAnkle"] = new()
            {
                BuffId = "buffSprainedAnkle",
                BaseVisibility = InjuryVisibilityLevel.Suspicious,
                CanBeHiddenFromHarvey = true,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 3,
                VisibleSigns = new[] { "limping", "careful_steps" }
            },
            ["buffBruisedRibs"] = new()
            {
                BuffId = "buffBruisedRibs",
                BaseVisibility = InjuryVisibilityLevel.Subtle,
                CanBeHiddenFromHarvey = true,
                HarveyDetectionBonus = 2,
                VisibleSigns = new[] { "shallow_breathing", "guarding_side" }
            },
            ["buffBackStrain"] = new()
            {
                BuffId = "buffBackStrain",
                BaseVisibility = InjuryVisibilityLevel.Subtle,
                CanBeHiddenFromHarvey = true,
                HarveyDetectionBonus = 2,
                VisibleSigns = new[] { "stiff_back", "careful_bending" }
            },
            ["buffTornMuscles"] = new()
            {
                BuffId = "buffTornMuscles",
                BaseVisibility = InjuryVisibilityLevel.Suspicious,
                CanBeHiddenFromHarvey = true,
                AutoRevealOnTalkToHarvey = true,
                HarveyDetectionBonus = 3,
                VisibleSigns = new[] { "weak_limb", "pain_movement" }
            },
            ["buffConcussion"] = new()
            {
                BuffId = "buffConcussion",
                BaseVisibility = InjuryVisibilityLevel.Suspicious,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 5,
                VisibleSigns = new[] { "dizziness", "confusion", "pale", "slow_reaction" }
            },
            ["buffFracturedBone"] = new()
            {
                BuffId = "buffFracturedBone",
                BaseVisibility = InjuryVisibilityLevel.Unhideable,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 6,
                VisibleSigns = new[] { "limping", "unable_to_use_limb", "severe_pain" }
            },
            ["buffShrapnelWounds"] = new()
            {
                BuffId = "buffShrapnelWounds",
                BaseVisibility = InjuryVisibilityLevel.Obvious,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                HarveyDetectionBonus = 5,
                VisibleSigns = new[] { "blood", "embedded_fragments", "severe_pain" }
            },
            ["buffInfectedWound"] = new()
            {
                BuffId = "buffInfectedWound",
                BaseVisibility = InjuryVisibilityLevel.Obvious,
                CanBeHiddenFromHarvey = false,
                AutoRevealOnTalkToHarvey = true,
                AutoRevealOnProximityToHarvey = true,
                AutoRevealWhenComplicated = true,
                HarveyDetectionBonus = 5,
                VisibleSigns = new[] { "fever", "redness", "weakness", "infection" }
            },
        };

        public static InjuryVisibilityProfile GetVisibilityProfile(string buffId)
        {
            if (VisibilityProfiles.TryGetValue(buffId, out var profile))
                return profile;

            return new InjuryVisibilityProfile
            {
                BuffId = buffId,
                BaseVisibility = InjuryVisibilityLevel.Subtle,
                CanBeHiddenFromHarvey = true,
                HarveyDetectionBonus = 1
            };
        }

        public static InjuryVisibilityLevel GetVisibilityLevel(DebuffState state) =>
            (InjuryVisibilityLevel)Math.Clamp(state.VisibilityLevel, 0, (int)InjuryVisibilityLevel.Unhideable);

        /// <summary>buffDeepCuts → DeepCuts; HarveyMod_WetBandage без изменений.</summary>
        public static string CleanBuffId(string buffId)
        {
            if (string.IsNullOrWhiteSpace(buffId))
                return "";

            return buffId.StartsWith("buff", StringComparison.OrdinalIgnoreCase)
                ? buffId["buff".Length..]
                : buffId;
        }

        public static void InitializeVisibility(DebuffState state, string buffId, bool harveySawIt, string reason)
        {
            var profile = GetVisibilityProfile(buffId);
            state.VisibilityLevel = (int)profile.BaseVisibility;

            if (harveySawIt)
            {
                int today = GameUtils.Today();
                state.HarveyAware = true;
                state.HiddenFromHarvey = false;
                state.DiscoveryReason = reason;
                state.AwarenessReason = reason;
                state.HarveyAwareDay = today;
            }
            else
            {
                state.HarveyAware = false;
                state.HiddenFromHarvey = true;
                state.DiscoveryReason = "";
                state.AwarenessReason = "";
                state.HarveyAwareDay = -1;
            }
        }

        public static bool IsHarveyPresent(ModConfig config)
        {
            var harvey = HarveyHelper.FindHarveyInLocation(Game1.currentLocation);
            if (harvey == null)
                return false;

            return HarveyHelper.GetDistanceToPlayer(harvey) <= config.ProximityTiles;
        }

        public static DetectionContext BuildDetectionContext(
            ModConfig config,
            InjuryState state,
            bool isDirectTalk,
            bool isProximityCheck)
        {
            var friendship = Game1.player?.friendshipData;
            Friendship? harveyFriendship = null;
            friendship?.TryGetValue("Harvey", out harveyFriendship);

            bool isFarmHouse = string.Equals(
                Game1.currentLocation?.NameOrUniqueName,
                "FarmHouse",
                StringComparison.OrdinalIgnoreCase);
            int time = Game1.timeOfDay;
            bool morningOrEvening = isFarmHouse && (time < 1000 || time >= 1900);

            return new DetectionContext
            {
                HarveyIsPresent = IsHarveyPresent(config),
                IsDirectTalk = isDirectTalk,
                IsProximityCheck = isProximityCheck,
                IsMarriedToHarvey = harveyFriendship?.IsMarried() == true,
                IsDatingOrEngagedToHarvey = harveyFriendship?.IsDating() == true
                    || harveyFriendship?.IsEngaged() == true,
                IsFarmHouseMorningOrEvening = morningOrEvening,
                PlayerHealthLow = Game1.player != null
                    && Game1.player.health < Game1.player.maxHealth * 0.4f,
                PlayerStaminaLow = Game1.player != null
                    && Game1.player.Stamina < Game1.player.MaxStamina * 0.25f,
                HasComplication = state.ActiveComplications.Count > 0,
                IsFestivalContext = Game1.isFestival() || Game1.CurrentEvent != null,
            };
        }

        public static bool ShouldHarveyDetectHiddenInjury(
            DebuffState state,
            InjuryVisibilityProfile profile,
            DetectionContext context)
        {
            if (state.HarveyAware)
                return false;

            if (!context.HarveyIsPresent)
                return false;

            if (profile.BaseVisibility == InjuryVisibilityLevel.Unhideable)
                return true;

            if (context.IsDirectTalk && profile.AutoRevealOnTalkToHarvey)
                return true;

            if (context.IsProximityCheck && profile.AutoRevealOnProximityToHarvey)
            {
                if (profile.BaseVisibility >= InjuryVisibilityLevel.Obvious)
                    return true;
            }

            int score = 0;
            score += (int)profile.BaseVisibility * 2;
            score += profile.HarveyDetectionBonus;
            score += state.HiddenDays;
            score += state.SuspicionLevel;

            if (context.IsDirectTalk)
                score += 2;

            if (context.IsMarriedToHarvey)
                score += 2;

            if (context.IsDatingOrEngagedToHarvey)
                score += 1;

            if (context.IsFarmHouseMorningOrEvening)
                score += 2;

            if (context.PlayerHealthLow)
                score += 2;

            if (context.PlayerStaminaLow)
                score += 1;

            if (context.HasComplication)
                score += 3;

            if (state.PlayerDeniedInjuryToday)
                score -= 1;

            if (context.IsFestivalContext)
                score -= 2;

            return score >= 6;
        }

        public static string ResolveDetectionTopic(
            string buffId,
            InjuryVisibilityProfile profile,
            InjuryState state,
            bool hasComplicationForInjury)
        {
            if (profile.BaseVisibility == InjuryVisibilityLevel.Unhideable)
                return TopicIds.GetHiddenInjuryUnhideableTopic(buffId);

            if (!profile.CanBeHiddenFromHarvey)
                return TopicIds.GetHiddenInjuryObviousTopic(buffId);

            if (hasComplicationForInjury && profile.AutoRevealWhenComplicated)
                return TopicIds.GetHiddenInjuryComplicatedTopic(buffId);

            return TopicIds.GetHiddenInjuryDetectedTopic(buffId);
        }

        public static bool RevealHiddenInjury(
            StateManager stateManager,
            DialogueManager dialogueManager,
            DebuffState state,
            string buffId,
            string reason,
            InjuryState injuryState,
            IMonitor? monitor = null,
            bool addDetectionTopic = true)
        {
            if (state.HarveyAware)
                return false;

            var profile = GetVisibilityProfile(buffId);
            int today = GameUtils.Today();

            state.HarveyAware = true;
            state.HiddenFromHarvey = false;
            state.DiscoveryReason = reason;
            state.AwarenessReason = reason;
            state.HarveyAwareDay = today;
            stateManager.UpdateDebuffState(buffId, state);

            string? topic = null;
            if (addDetectionTopic)
            {
                bool hasComplication = injuryState.ActiveComplications.Count > 0;
                topic = ResolveDetectionTopic(buffId, profile, injuryState, hasComplication);
                dialogueManager.AddTopic(topic, 3);
            }

            HarveyInjuryAwarenessHelper.TryAddKnownSevereInjuryTopic(dialogueManager, buffId);

            monitor?.Log(
                $"[InjuryVisibility] Revealed {buffId} reason={reason} topic={topic ?? "(none)"} "
                + $"visibility={(InjuryVisibilityLevel)state.VisibilityLevel}",
                LogLevel.Info);

            return true;
        }

        /// <summary>Попытка обнаружить скрытые травмы в контексте разговора или близости.</summary>
        public static int TryDetectHiddenInjuries(
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager,
            BuffManager buffManager,
            bool isDirectTalk,
            bool isProximityCheck,
            string reason,
            IMonitor? monitor = null)
        {
            var injuryState = stateManager.State;
            var context = BuildDetectionContext(config, injuryState, isDirectTalk, isProximityCheck);
            if (!context.HarveyIsPresent)
                return 0;

            int revealed = 0;
            foreach (var (buffId, debuffState) in injuryState.ActiveDebuffs.ToList())
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                    continue;

                if (!debuffState.HiddenFromHarvey && debuffState.HarveyAware)
                    continue;

                if (!injuryManager.HasInjuryOrPhase(buffId) && !buffManager.HasBuff(buffId))
                    continue;

                var profile = GetVisibilityProfile(buffId);
                if (!ShouldHarveyDetectHiddenInjury(debuffState, profile, context))
                    continue;

                if (RevealHiddenInjury(
                        stateManager,
                        dialogueManager,
                        debuffState,
                        buffId,
                        reason,
                        injuryState,
                        monitor))
                {
                    revealed++;
                }
            }

            return revealed;
        }

        public static void ProcessHiddenInjuryDaily(
            InjuryState state,
            ComplicationManager? complicationManager,
            IMonitor? monitor = null)
        {
            foreach (var (_, debuffState) in state.ActiveDebuffs)
            {
                if (!debuffState.HiddenFromHarvey)
                    continue;

                debuffState.HiddenDays++;

                var profile = GetVisibilityProfile(debuffState.BuffId);
                if ((int)profile.BaseVisibility >= (int)InjuryVisibilityLevel.Suspicious
                    || InjurySets.Severe.Contains(debuffState.BuffId)
                    || InjurySets.Critical.Contains(debuffState.BuffId))
                {
                    debuffState.SuspicionLevel++;
                }

                if (debuffState.HiddenDays >= 2 && debuffState.SuspicionLevel >= 2)
                    complicationManager?.TryRollHiddenInjuryComplicationRisk(debuffState.BuffId);
            }

            foreach (var debuffState in state.ActiveDebuffs.Values)
                debuffState.PlayerDeniedInjuryToday = false;

            state.HiddenInjuryQuestionCountToday = 0;
            state.PendingHiddenInjuryBuffId = "";

            monitor?.Log("[InjuryVisibility] Daily hidden-injury progress applied", LogLevel.Debug);
        }

        public static void MigrateInjuryVisibility(InjuryState state, IMonitor? monitor = null)
        {
            int migrated = 0;
            foreach (var (buffId, debuffState) in state.ActiveDebuffs)
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                    continue;

                bool shouldBeVisible = debuffState.HarveyAware
                    || debuffState.TreatmentStarted
                    || debuffState.HarveyConversationHappened;

                if (shouldBeVisible && debuffState.HiddenFromHarvey)
                {
                    debuffState.HiddenFromHarvey = false;
                    migrated++;
                }
                else if (!shouldBeVisible && !debuffState.HarveyAware && debuffState.VisibilityLevel == 0)
                {
                    InitializeVisibility(debuffState, buffId, harveySawIt: false, reason: "");
                    migrated++;
                }
                else if (debuffState.VisibilityLevel == 0 && !shouldBeVisible)
                {
                    debuffState.VisibilityLevel = (int)GetVisibilityProfile(buffId).BaseVisibility;
                    debuffState.HiddenFromHarvey = true;
                    migrated++;
                }
            }

            if (migrated > 0)
                monitor?.Log($"[InjuryVisibility] Migrated {migrated} debuff visibility record(s)", LogLevel.Debug);
        }

        public static string GetNeutralMineHudMessage(string? mainInjuryId)
        {
            if (string.IsNullOrEmpty(mainInjuryId))
                return NeutralMineHudMessages[0];

            int index = Math.Abs(mainInjuryId.GetHashCode(StringComparison.OrdinalIgnoreCase))
                % NeutralMineHudMessages.Length;
            return NeutralMineHudMessages[index];
        }

        public static string FormatVisibilityDebugLine(DebuffState ds)
        {
            string topic = ds.HarveyAware || !ds.HiddenFromHarvey
                ? "-"
                : TopicIds.GetHiddenInjuryDetectedTopic(ds.BuffId);

            return $"vis={GetVisibilityLevel(ds)} aware={ds.HarveyAware} hidden={ds.HiddenFromHarvey} "
                + $"hiddenDays={ds.HiddenDays} suspicion={ds.SuspicionLevel} "
                + $"discovery={ds.DiscoveryReason ?? ""} topic={topic}";
        }
    }
}
