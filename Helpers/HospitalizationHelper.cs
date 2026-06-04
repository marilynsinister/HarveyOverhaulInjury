using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Расчёт срока и тексты HUD для принудительной госпитализации (HarveyMod_Hospitalized).</summary>
    public static class HospitalizationHelper
    {
        public const int StatusHudIntervalMinutes = 15;

        public static int ToClockMinutes(int timeOfDay)
        {
            int hours = timeOfDay / 100;
            int minutes = timeOfDay % 100;
            return hours * 60 + minutes;
        }

        public static int GetElapsedMinutes(InjuryState state, int timeOfDay)
        {
            int admissionMinutes = state.HospitalAdmissionMinutes;
            if (admissionMinutes < 0 && state.HospitalAdmissionTime >= 0)
                admissionMinutes = ToClockMinutes(state.HospitalAdmissionTime);

            if (admissionMinutes < 0)
                return 0;

            int now = ToClockMinutes(timeOfDay);
            int elapsed = now - admissionMinutes;
            if (elapsed < 0)
                elapsed += 24 * 60;

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
                delta += 24 * 60;

            return delta >= StatusHudIntervalMinutes;
        }
    }
}
