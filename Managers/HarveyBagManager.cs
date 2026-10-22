using System;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// «Сумка Харви» — награда доверия «Опора» (письмо в trustRewards.json). Пока сумка в рюкзаке, раз в день,
    /// когда здоровье падает до четверти, бинт и вдох возвращают половину здоровья. Не лечит травмы —
    /// только помогает продержаться и выбраться.
    /// </summary>
    public sealed class HarveyBagManager
    {
        public const string BagItemId = "(O)marilynsinister.HarveyOverhaul_HarveyBag";
        private const string LastUsedDayKey = "marilynsinister.HarveyOverhaul/HarveyBagLastDay";

        private readonly IMonitor _monitor;

        public HarveyBagManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public void OnUpdateTicked(uint ticks)
        {
            if (ticks % 15 != 0 || !Context.IsWorldReady || Game1.eventUp)
                return;

            var player = Game1.player;
            if (player == null || player.health <= 0 || player.health > player.maxHealth / 4)
                return;

            int today = (int)Game1.stats.DaysPlayed;
            if (player.modData.TryGetValue(LastUsedDayKey, out string? raw) && raw == today.ToString())
                return;

            if (!player.Items.ContainsId(BagItemId))
                return;

            int heal = Math.Max(1, player.maxHealth / 2);
            player.health = Math.Min(player.maxHealth, player.health + heal);
            player.modData[LastUsedDayKey] = today.ToString();

            Game1.playSound("powerup");
            Game1.addHUDMessage(new HUDMessage("Сумка Харви: «Сначала дыши. Потом — домой». Бинт, вдох — держишься.", HUDMessage.health_type));
            _monitor.Log($"[HarveyBag] +{heal} здоровья ({player.currentLocation?.Name})", LogLevel.Debug);
        }
    }
}
