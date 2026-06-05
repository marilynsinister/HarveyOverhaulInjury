using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Безопасный запуск CP-событий по префиксу ID в Data/Events/{locationName}.
    /// </summary>
    public static class LocationEventLauncher
    {
        public static string GetFarmHouseLocationName()
        {
            return Game1.player?.homeLocation.Value ?? "FarmHouse";
        }

        public static bool IsFarmHouseLocation(GameLocation? location)
        {
            if (location == null)
                return false;

            string name = location.NameOrUniqueName ?? "";
            if (string.Equals(name, GetFarmHouseLocationName(), StringComparison.OrdinalIgnoreCase))
                return true;

            return name.Contains("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Cabin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool EventKeyMatches(string key, string eventIdPrefix)
        {
            return key.Equals(eventIdPrefix, StringComparison.OrdinalIgnoreCase)
                || key.StartsWith(eventIdPrefix + "/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool LocationEventExists(string eventIdPrefix, string locationName)
        {
            return !string.IsNullOrWhiteSpace(LoadLocationEventScript(eventIdPrefix, locationName));
        }

        public static bool TryStartEventByPrefix(
            string eventIdPrefix,
            string locationName,
            IMonitor monitor,
            Action<string>? onFinished = null,
            string? scriptPrefix = null,
            bool addToEventsSeenOnFinished = false,
            string logTag = "LocationEvent")
        {
            try
            {
                var location = Game1.getLocationFromName(locationName);
                if (location == null)
                {
                    monitor.Log($"[{logTag}] Локация '{locationName}' не найдена", LogLevel.Warn);
                    return false;
                }

                string? eventScript = LoadLocationEventScript(eventIdPrefix, locationName);
                if (string.IsNullOrWhiteSpace(eventScript))
                {
                    monitor.Log(
                        $"[{logTag}] Событие '{eventIdPrefix}' не найдено в Data/Events/{locationName}",
                        LogLevel.Warn);
                    return false;
                }

                if (!string.IsNullOrEmpty(scriptPrefix))
                    eventScript = scriptPrefix + eventScript;

                if (!ReferenceEquals(Game1.currentLocation, location)
                    && !string.Equals(Game1.currentLocation?.NameOrUniqueName, locationName, StringComparison.OrdinalIgnoreCase))
                {
                    monitor.Log(
                        $"[{logTag}] Игрок не в '{locationName}' (текущая: {Game1.currentLocation?.NameOrUniqueName})",
                        LogLevel.Warn);
                    return false;
                }

                string resolvedEventId = ResolveEventId(eventIdPrefix, locationName) ?? eventIdPrefix;
                var gameEvent = new Event(eventScript);
                gameEvent.onEventFinished += () =>
                {
                    try
                    {
                        if (addToEventsSeenOnFinished
                            && !Game1.player.eventsSeen.Contains(resolvedEventId))
                        {
                            Game1.player.eventsSeen.Add(resolvedEventId);
                        }

                        onFinished?.Invoke(resolvedEventId);
                    }
                    catch (Exception ex)
                    {
                        monitor.Log($"[{logTag}] onEventFinished error for '{resolvedEventId}': {ex}", LogLevel.Error);
                    }
                };

                location.startEvent(gameEvent);
                monitor.Log($"[{logTag}] Запущено событие '{resolvedEventId}'", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log($"[{logTag}] Ошибка запуска '{eventIdPrefix}': {ex}", LogLevel.Error);
                return false;
            }
        }

        private static string? ResolveEventId(string eventIdPrefix, string locationName)
        {
            var eventData = Game1.content.Load<Dictionary<string, string>>($"Data/Events/{locationName}");
            if (eventData == null)
                return null;

            foreach (string key in eventData.Keys)
            {
                if (EventKeyMatches(key, eventIdPrefix))
                    return key;
            }

            return null;
        }

        private static string? LoadLocationEventScript(string eventIdPrefix, string locationName)
        {
            var eventData = Game1.content.Load<Dictionary<string, string>>($"Data/Events/{locationName}");
            if (eventData == null)
                return null;

            foreach (var kvp in eventData)
            {
                if (EventKeyMatches(kvp.Key, eventIdPrefix))
                    return kvp.Value;
            }

            return null;
        }
    }
}
