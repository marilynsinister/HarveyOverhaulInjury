using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Managers;
using HarveyOverhaul.InjuryCare.Services;
using StardewModdingAPI;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Письма Харви с суффиксом тона отношений и fallback, если tier-вариант отсутствует в Data/Mail.
    /// Отправка — только через <see cref="MedicalLetterScheduler"/>.
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

        /// <summary>Поставить tiered-письмо в pending-очередь.</summary>
        public static bool TryScheduleTieredMail(
            MedicalLetterScheduler scheduler,
            string baseMailId,
            string reason,
            string stateId = "",
            bool? critical = null,
            string? dedupeKey = null)
        {
            if (scheduler == null || string.IsNullOrWhiteSpace(baseMailId))
                return false;

            return scheduler.TryQueueTieredMail(baseMailId, reason, stateId, critical, dedupeKey);
        }

        /// <summary>Было ли письмо с этим dedupeKey уже запланировано сегодня.</summary>
        public static bool WasSentToday(StateManager stateManager, string dedupeKey)
        {
            if (string.IsNullOrWhiteSpace(dedupeKey))
                return false;

            int today = (int)StardewValley.Game1.stats.DaysPlayed;
            return stateManager.State.SentMedicalMailDays.TryGetValue(dedupeKey, out int sentDay)
                && sentDay == today;
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
                _mailCache ??= StardewValley.Game1.content.Load<Dictionary<string, string>>("Data/Mail");
                return _mailCache.ContainsKey(mailId);
            }
            catch
            {
                return true;
            }
        }

        /// <summary>Сброс кэша Data/Mail (после reload контента).</summary>
        public static void InvalidateMailCache() => _mailCache = null;
    }
}
