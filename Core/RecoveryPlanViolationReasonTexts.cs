using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>Человекочитаемые причины нарушений плана восстановления для UI.</summary>
    public static class RecoveryPlanViolationReasonTexts
    {
        private static readonly string[] DisplayOrder =
        {
            "entered_mine",
            "entered_volcano",
            "stamina_too_low",
            "health_too_low",
            "too_late",
            "heavy_work",
            "rain_bandage",
            "missed_harvey_checkup",
            "passed_out",
            "unknown",
        };

        private static readonly Dictionary<string, string> DisplayTexts =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["entered_mine"] = "ты вошла в шахту с активным режимом",
                ["entered_volcano"] = "ты вошла в вулкан с активным режимом",
                ["stamina_too_low"] = "stamina упала слишком низко",
                ["health_too_low"] = "здоровье упало слишком низко",
                ["too_late"] = "ты легла слишком поздно",
                ["heavy_work"] = "ты слишком сильно перегрузила организм",
                ["rain_bandage"] = "повязка промокла под дождём",
                ["missed_harvey_checkup"] = "Харви ждёт контрольный разговор",
                ["passed_out"] = "обморок от истощения",
                ["unknown"] = "нарушение режима",
            };

        /// <summary>
        /// Собрать секцию «Сегодня не засчитано» для окна плана.
        /// Пустая строка — секцию не показывать.
        /// </summary>
        public static string BuildTodayFailedSection(
            bool isActive,
            bool todayFailed,
            IReadOnlyList<string>? reasons)
        {
            if (!isActive)
                return "";

            bool hasReasons = reasons != null && reasons.Count > 0;
            if (!todayFailed && !hasReasons)
                return "";

            IReadOnlyList<string> formatted = FormatReasons(reasons);
            if (formatted.Count == 0)
                formatted = new[] { DisplayTexts["unknown"] };

            var sb = new StringBuilder();
            sb.AppendLine("Сегодня не засчитано:");
            for (int i = 0; i < formatted.Count; i++)
            {
                sb.Append("— ");
                sb.Append(formatted[i]);
                sb.Append(i < formatted.Count - 1 ? ';' : '.');
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>Преобразовать технические ID в уникальный упорядоченный список для UI.</summary>
        public static IReadOnlyList<string> FormatReasons(IEnumerable<string>? rawReasons)
        {
            if (rawReasons == null)
                return Array.Empty<string>();

            var canonicalSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in rawReasons)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                canonicalSeen.Add(CanonicalizeReasonId(raw.Trim()));
            }

            if (canonicalSeen.Count == 0)
                return Array.Empty<string>();

            var result = new List<string>();
            foreach (string key in DisplayOrder)
            {
                if (!canonicalSeen.Contains(key))
                    continue;

                result.Add(DisplayTexts[key]);
            }

            return result;
        }

        /// <summary>Привести legacy/alias ID к каноническому коду для TodayViolationReasons.</summary>
        public static string CanonicalizeReasonId(string raw)
        {
            string key = raw.ToLowerInvariant();
            return key switch
            {
                "mine"
                    or "skull_cave"
                    or RecoveryPlanReasonIds.EnteredMine
                    or "enteredminesduringrecovery" => RecoveryPlanReasonIds.EnteredMine,

                "volcano"
                    or "volcanodungeon"
                    or RecoveryPlanReasonIds.EnteredVolcano => RecoveryPlanReasonIds.EnteredVolcano,

                RecoveryPlanReasonIds.StaminaTooLow
                    or "low_stamina"
                    or "lowstamina"
                    or "lowstaminaduringrecovery"
                    or "low_stamina_farm" => RecoveryPlanReasonIds.StaminaTooLow,

                "overwork" => RecoveryPlanReasonIds.HeavyWork,

                RecoveryPlanReasonIds.HealthTooLow
                    or "low_health"
                    or "lowhealth"
                    or "lowhealthduringrecovery"
                    or "critical_health" => RecoveryPlanReasonIds.HealthTooLow,

                RecoveryPlanReasonIds.TooLate
                    or "late_night"
                    or "latenight"
                    or "latesleepduringrecovery" => RecoveryPlanReasonIds.TooLate,

                RecoveryPlanReasonIds.HeavyWork => RecoveryPlanReasonIds.HeavyWork,

                RecoveryPlanReasonIds.RainBandage
                    or "rain"
                    or "rain_with_bandage"
                    or "rainduringrecovery" => RecoveryPlanReasonIds.RainBandage,

                RecoveryPlanReasonIds.MissedHarveyCheckup
                    or "missed_checkup"
                    or "missedcheckup" => RecoveryPlanReasonIds.MissedHarveyCheckup,

                "mine_or_volcano" => RecoveryPlanReasonIds.EnteredMine,

                RecoveryPlanReasonIds.PassedOut
                    or "passout"
                    or "passedout"
                    or "pass_out" => RecoveryPlanReasonIds.PassedOut,

                _ when DisplayTexts.ContainsKey(key) => key,
                _ => RecoveryPlanReasonIds.Unknown,
            };
        }
    }
}
