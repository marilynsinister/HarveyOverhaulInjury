using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using Microsoft.Xna.Framework;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Расчёт срока и тексты HUD для принудительной госпитализации (HarveyMod_Hospitalized).</summary>
    public static class HospitalizationHelper
    {
        public const int StatusHudIntervalMinutes = 15;

        /// <summary>
        /// Свободная клетка у койки HospitalBedX/Y. Сама койка — Buildings (на Alchemistry и ванили),
        /// поставленный на неё фермер после сцены не может сойти. Ищем ближайшую проходимую: сначала
        /// рядом с койкой, потом в радиусе 3. Не нашли (чужая карта) — возвращаем саму койку, как раньше.
        /// </summary>
        public static Point GetBedsideTile(GameLocation? location, ModConfig config)
        {
            var bed = new Point(Math.Max(0, config.HospitalBedX), Math.Max(0, config.HospitalBedY));
            if (location == null)
                return bed;

            Point[] beside = { new(0, 1), new(-1, 0), new(1, 0), new(0, -1), new(-1, 1), new(1, 1) };
            foreach (var d in beside)
            {
                var tile = new Point(bed.X + d.X, bed.Y + d.Y);
                if (IsStandable(location, tile))
                    return tile;
            }

            for (int distance = 2; distance <= 3; distance++)
            for (int dy = -distance; dy <= distance; dy++)
            for (int dx = -distance; dx <= distance; dx++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != distance)
                    continue;

                var tile = new Point(bed.X + dx, bed.Y + dy);
                if (IsStandable(location, tile))
                    return tile;
            }

            return bed;
        }

        private static bool IsStandable(GameLocation location, Point tile)
        {
            var vector = new Vector2(tile.X, tile.Y);
            return location.isTileOnMap(vector) && location.isTilePassable(vector);
        }

        public static int ToClockMinutes(int timeOfDay)
        {
            int hours = timeOfDay / 100;
            int minutes = timeOfDay % 100;
            return hours * 60 + minutes;
        }

        /// <summary>Накопленный прогресс госпитализации (устойчив к LongerDays).</summary>
        public static int GetElapsedMinutes(InjuryState state, int timeOfDay)
        {
            if (state.HospitalStayProgressMinutes > 0)
                return state.HospitalStayProgressMinutes;

            int admissionMinutes = state.HospitalAdmissionMinutes;
            if (admissionMinutes < 0 && state.HospitalAdmissionTime >= 0)
                admissionMinutes = ToClockMinutes(state.HospitalAdmissionTime);

            if (admissionMinutes < 0)
                return 0;

            int now = ToClockMinutes(timeOfDay);
            int elapsed = now - admissionMinutes;
            if (elapsed < 0)
                return 0;

            return elapsed;
        }

        public static int GetMinStayMinutes(InjuryState state, ModConfig config)
        {
            return state.HospitalMinStayMinutes > 0
                ? state.HospitalMinStayMinutes
                : config.MinHospitalStayMinutes;
        }

        /// <summary>Оставшиеся игровые минуты до выписки (0 — минимальный срок прошёл).</summary>
        public static int GetRemainingMinutes(InjuryState state, ModConfig config, int timeOfDay)
        {
            int minStay = GetMinStayMinutes(state, config);
            int elapsed = GetElapsedMinutes(state, timeOfDay);
            return Math.Max(0, minStay - elapsed);
        }

        public static string FormatRemainingHud(int remainingMinutes)
        {
            if (remainingMinutes <= 0)
                return "Госпитализация: Харви разрешил выписку.";

            return $"Госпитализация: осталось примерно {remainingMinutes} мин.";
        }

        public static bool ShouldShowStatusHud(int timeOfDay, int lastHudMinute)
        {
            if (lastHudMinute < 0)
                return true;

            int now = ToClockMinutes(timeOfDay);
            int delta = now - lastHudMinute;
            if (delta < 0)
                return false;

            return delta >= StatusHudIntervalMinutes;
        }
    }
}
