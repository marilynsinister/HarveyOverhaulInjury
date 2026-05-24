using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// C#-launcher для CP storm comfort cutscenes (замена отключённого triggersStress.json).
    /// </summary>
    public static class StormComfortLauncher
    {
        public static bool IsStormWeather() => Game1.isLightning;

        public static bool CanRollToday(InjuryState state, int today, DialogueManager dialogueManager)
        {
            if (state.LastStormComfortRollDay == today)
                return false;

            if (state.LastStormComfortEventDay == today)
                return false;

            if (dialogueManager.HasTopic(StormComfortIds.CooldownTopic))
                return false;

            return true;
        }

        public static bool MeetsRollConditions(int timeOfDay, DialogueManager dialogueManager)
        {
            if (timeOfDay < StormComfortIds.RollTimeStart || timeOfDay > StormComfortIds.RollTimeEnd)
                return false;

            if (!IsStormWeather())
                return false;

            if (Utility.isFestivalDay())
                return false;

            if (Game1.eventUp || Game1.activeClickableMenu != null)
                return false;

            if (Game1.player?.friendshipData == null
                || !Game1.player.friendshipData.TryGetValue("Harvey", out var friendship)
                || friendship.Points < StormComfortIds.MinFriendshipPoints)
            {
                return false;
            }

            if (dialogueManager.HasTopic(StormComfortIds.StormStressTopic)
                || dialogueManager.HasTopic(StormComfortIds.LegacyStressTopic))
            {
                return false;
            }

            return true;
        }

        public static void TryDailyStormComfortRoll(
            IMonitor monitor,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            int timeOfDay,
            double rollChance = StormComfortIds.DefaultRollChance)
        {
            if (!Context.IsWorldReady)
                return;

            int today = GameUtils.Today();
            var state = stateManager.State;

            if (!CanRollToday(state, today, dialogueManager))
                return;

            if (!MeetsRollConditions(timeOfDay, dialogueManager))
                return;

            state.LastStormComfortRollDay = today;

            if (Game1.random.NextDouble() >= rollChance)
            {
                stateManager.Save();
                monitor.Log("[StormComfort] Daily roll failed.", LogLevel.Trace);
                return;
            }

            ApplyStormStressGate(buffManager, dialogueManager, monitor);
            stateManager.Save();
            monitor.Log("[StormComfort] Roll success: storm stress gate applied.", LogLevel.Info);
        }

        public static void ApplyStormStressGate(BuffManager buffManager, DialogueManager dialogueManager, IMonitor monitor)
        {
            if (buffManager.BuffExists(StormComfortIds.StressThunderBuff))
            {
                buffManager.AddBuff(StormComfortIds.StressThunderBuff, -2);
                monitor.Log("[StormComfort] Applied buffStressThunder.", LogLevel.Debug);
                return;
            }

            dialogueManager.AddTopic(StormComfortIds.StormStressTopic, 1);
            monitor.Log("[StormComfort] buffStressThunder missing in Data/Buffs; applied topicHarveyStormStress.", LogLevel.Debug);
        }

        public static bool IsStormComfortEventId(string? eventId)
        {
            return !string.IsNullOrEmpty(eventId)
                && eventId.StartsWith(StormComfortIds.EventIdPrefix, StringComparison.Ordinal);
        }

        public static void MarkStormComfortEventPlayed(StateManager stateManager, IMonitor monitor)
        {
            int today = GameUtils.Today();
            stateManager.State.LastStormComfortEventDay = today;
            stateManager.Save();
            monitor.Log($"[StormComfort] Event completed on day {today}.", LogLevel.Debug);
        }
    }
}
