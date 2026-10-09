using System;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.Core.Services;
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
    /// Страх грозы принадлежит Stress mod: Injury только просит реакцию через Core и сам баффы стресса не трогает.
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

        public static bool MeetsRollConditions(int timeOfDay, DialogueManager dialogueManager, IHarveyStressStateApi? stressApi)
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

            // Gate уже открыт: свой topic или Stress уже ведёт страх грозы (CP-сцена сработает по его баффу).
            if (dialogueManager.HasTopic(StormComfortIds.StormStressTopic))
                return false;

            if (stressApi?.HasCondition(HarveyStressConditions.Thunder) == true)
                return false;

            return true;
        }

        public static void TryDailyStormComfortRoll(
            IMonitor monitor,
            StateManager stateManager,
            DialogueManager dialogueManager,
            IHarveyStressStateApi? stressApi,
            int timeOfDay,
            double rollChance = StormComfortIds.DefaultRollChance)
        {
            if (!Context.IsWorldReady)
                return;

            int today = GameUtils.Today();
            var state = stateManager.State;

            if (!CanRollToday(state, today, dialogueManager))
                return;

            if (!MeetsRollConditions(timeOfDay, dialogueManager, stressApi))
                return;

            state.LastStormComfortRollDay = today;

            if (Game1.random.NextDouble() >= rollChance)
            {
                stateManager.Save();
                monitor.Log("[StormComfort] Daily roll failed.", LogLevel.Trace);
                return;
            }

            bool applied = ApplyStormStressGate(stressApi, dialogueManager, monitor);
            stateManager.Save();
            monitor.Log(
                applied
                    ? "[StormComfort] Roll success: storm stress gate applied."
                    : "[StormComfort] Roll success, but Stress declined the thunder reaction (immunity/cooldown).",
                LogLevel.Info);
        }

        /// <returns>true — gate для CP-сцены открыт.</returns>
        public static bool ApplyStormStressGate(IHarveyStressStateApi? stressApi, DialogueManager dialogueManager, IMonitor monitor)
        {
            if (stressApi != null)
            {
                bool active = stressApi.RequestReaction(HarveyStressConditions.Thunder, HarveyProviderRegistry.InjuryProviderId);
                monitor.Log($"[StormComfort] Thunder reaction requested from Stress via Core: active={active}.", LogLevel.Debug);
                return active;
            }

            dialogueManager.AddTopic(StormComfortIds.StormStressTopic, 1);
            monitor.Log("[StormComfort] Stress mod not registered in Core; applied topicHarveyStormStress.", LogLevel.Debug);
            return true;
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
