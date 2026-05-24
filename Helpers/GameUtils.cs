using System;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Утилиты для игровой логики
    /// </summary>
    public static class GameUtils
    {
        /// <summary>
        /// Проверить, находится ли значение в диапазоне
        /// </summary>
        public static bool Between(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Проверить случайность (бросок кубика)
        /// </summary>
        /// <param name="probability">Вероятность от 0.0 до 1.0</param>
        public static bool Roll(double probability)
        {
            return Game1.random.NextDouble() < probability;
        }

        /// <summary>
        /// Проверить, нет ли активного баффа и топика
        /// </summary>
        public static bool NoActive(string buffId, string topicId)
        {
            return !Game1.player.hasBuff(buffId) && !HasConversationTopic(topicId);
        }

        /// <summary>
        /// Проверить наличие conversation topic у игрока
        /// </summary>
        public static bool HasConversationTopic(string topicId)
        {
            if (Game1.player.activeDialogueEvents == null) return false;
            return Game1.player.activeDialogueEvents.ContainsKey(topicId);
        }

        /// <summary>
        /// Получить текущий день
        /// </summary>
        public static int Today()
        {
            return (int)Game1.stats.DaysPlayed;
        }

        /// <summary>
        /// Получить текущее игровое время в минутах
        /// </summary>
        public static int CurrentTimeInMinutes()
        {
            int timeOfDay = Game1.timeOfDay;
            return (timeOfDay / 100) * 60 + (timeOfDay % 100);
        }
    }
}

