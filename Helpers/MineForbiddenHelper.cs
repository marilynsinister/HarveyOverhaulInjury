using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Расчёт срока и тексты HUD для запрета шахты (HarveyMod_MineForbidden).</summary>
    public static class MineForbiddenHelper
    {
        public static int GetMineForbiddenDurationDays(ModConfig config)
        {
            return Math.Max(1, config.MineForbiddenDurationDays);
        }

        /// <summary>Оставшихся игровых дней запрета (0 — срок истёк или не наложен).</summary>
        public static int GetMineForbiddenDaysLeft(InjuryState state, ModConfig config, int today)
        {
            int appliedDay = state.MineForbiddenAppliedDay;
            if (appliedDay < 0)
                return 0;

            int duration = GetMineForbiddenDurationDays(config);
            return Math.Max(0, appliedDay + duration - today);
        }

        public static string FormatAppliedHud(ModConfig config, int today, InjuryState state)
        {
            int duration = GetMineForbiddenDurationDays(config);
            int left = GetMineForbiddenDaysLeft(state, config, today);
            if (left <= 0)
                left = duration;

            return $"Харви запретил шахту на {duration} дн. Осталось: {left} дн.";
        }

        public static string FormatActiveMineHud(int daysLeft)
        {
            if (daysLeft <= 0)
                return "Запрет Харви на шахту ещё действует.";

            return daysLeft == 1
                ? "Запрет Харви на шахту ещё действует. Осталось: 1 день."
                : $"Запрет Харви на шахту ещё действует. Осталось: {daysLeft} дн.";
        }

        public static string FormatActiveVolcanoHud(int daysLeft)
        {
            if (daysLeft <= 0)
                return "Запрет Харви на опасные подземелья ещё действует.";

            return daysLeft == 1
                ? "Запрет Харви на опасные подземелья ещё действует. Осталось: 1 день."
                : $"Запрет Харви на опасные подземелья ещё действует. Осталось: {daysLeft} дн.";
        }
    }
}
