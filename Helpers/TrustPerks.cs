using System;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Что меняет высокое доверие Харви (шкала «Доверие» в StressMeter, флаги HarveyCareTrust_L1–L4).
    /// L3 «Надёжный человек» — меньше проверок: жёсткий запрет шахты на день короче (не меньше дня),
    /// вечерней домашней проверки скрытых травм нет — «скажешь сама». Письмо: trustRewards.json.
    /// Без StressMeter флагов нет — всё как раньше.
    /// </summary>
    public static class TrustPerks
    {
        public const string FewerChecksFlag = "HarveyCareTrust_L3";

        public static bool FewerChecks => Game1.player?.mailReceived.Contains(FewerChecksFlag) == true;

        /// <summary>Дни жёсткого запрета шахты с учётом доверия (минимум один день).</summary>
        public static int MineBlockDays(int configuredDays)
            => Math.Max(1, FewerChecks ? configuredDays - 1 : configuredDays);
    }
}
