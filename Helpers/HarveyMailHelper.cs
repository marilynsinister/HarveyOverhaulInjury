using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Письма Харви с суффиксом тона отношений и fallback, если tier-вариант отсутствует в Data/Mail.
    /// </summary>
    public static class HarveyMailHelper
    {
        private static Dictionary<string, string>? _mailCache;

        /// <summary>
        /// Собрать финальный mailId: base + _Tier с цепочкой fallback.
        /// </summary>
        public static string BuildRelationshipMailId(string baseMailId, HarveyRelationshipTier? tier = null)
        {
            if (string.IsNullOrWhiteSpace(baseMailId))
                return baseMailId;

            HarveyRelationshipTier resolvedTier = tier ?? HarveyHelper.GetHarveyRelationshipTier();

            foreach (string? suffix in GetSuffixFallbackChain(resolvedTier))
            {
                string candidate = string.IsNullOrEmpty(suffix)
                    ? baseMailId
                    : $"{baseMailId}_{suffix}";

                if (MailEntryExists(candidate))
                    return candidate;
            }

            return baseMailId;
        }

        /// <summary>Письмо на завтра: SendLetters, dedupe по dedupeKey, tier-fallback.</summary>
        public static bool TryScheduleTieredMail(
            ModConfig config,
            StateManager stateManager,
            IMonitor? monitor,
            string baseMailId,
            string? dedupeKey = null)
        {
            if (!config.SendLetters)
                return false;

            if (string.IsNullOrWhiteSpace(baseMailId))
                return false;

            int today = (int)Game1.stats.DaysPlayed;
            string key = dedupeKey ?? baseMailId;
            var sent = stateManager.State.SentMedicalMailDays;

            if (sent.TryGetValue(key, out int sentDay) && sentDay == today)
            {
                monitor?.Log(
                    $"[Mail] Пропуск дубликата «{key}» в день {today}",
                    LogLevel.Debug);
                return false;
            }

            string mailId = BuildRelationshipMailId(baseMailId);
            Game1.addMailForTomorrow(mailId);

            sent[key] = today;
            stateManager.Save();

            monitor?.Log(
                $"[Mail] {mailId} → завтра (base={baseMailId}, tier={HarveyHelper.GetHarveyRelationshipTier()}, key={key})",
                LogLevel.Debug);
            return true;
        }

        /// <summary>Базовый ID письма о нарушении предписания по reason.</summary>
        public static string GetPrescriptionViolationMailBase(string reason) =>
            reason switch
            {
                "mine" => MailIds.NoMineViolation,
                "rain" or "pool" => MailIds.KeepDryViolation,
                "late_sleep" => MailIds.RestViolation,
                _ => MailIds.PrescriptionViolation,
            };

        private static IEnumerable<string?> GetSuffixFallbackChain(HarveyRelationshipTier tier) =>
            tier switch
            {
                HarveyRelationshipTier.Married => new[] { "Married", "Dating", "MidHearts", "LowHearts", null },
                HarveyRelationshipTier.Dating => new[] { "Dating", "MidHearts", "LowHearts", null },
                HarveyRelationshipTier.HighHearts => new[] { "MidHearts", "LowHearts", null },
                HarveyRelationshipTier.MidHearts => new[] { "MidHearts", "LowHearts", null },
                _ => new[] { "LowHearts", null },
            };

        private static bool MailEntryExists(string mailId)
        {
            try
            {
                _mailCache ??= Game1.content.Load<Dictionary<string, string>>("Data/Mail");
                return _mailCache.ContainsKey(mailId);
            }
            catch
            {
                // CP/Data/Mail недоступен — используем candidate как есть.
                return true;
            }
        }

        /// <summary>Сброс кэша Data/Mail (после reload контента).</summary>
        public static void InvalidateMailCache() => _mailCache = null;
    }
}
