using System;
using HarveyOverhaul.InjuryCare.Core;
using Microsoft.Xna.Framework;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Уровень отношений с Харви для tiered-писем и тона (не TreatmentComplianceScore).</summary>
    public enum HarveyRelationshipTier
    {
        LowHearts,
        MidHearts,
        HighHearts,
        Dating,
        Married,
    }

    /// <summary>
    /// Вспомогательные методы для работы с NPC Харви
    /// </summary>
    public static class HarveyHelper
    {
        /// <summary>
        /// Получить Харви
        /// </summary>
        public static NPC? GetHarvey()
        {
            return Game1.getCharacterFromName("Harvey", mustBeVillager: true);
        }

        /// <summary>
        /// Проверить, встречается ли игрок с Харви или женат на нём
        /// </summary>
        public static bool IsDatingOrMarriedToHarvey()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship == null) return false;

            if (friendship.TryGetValue("Harvey", out var data))
            {
                return data.IsMarried() || data.IsDating();
            }

            return false;
        }

        /// <summary>Dating, Engaged или Married с Харви (для domestic hidden-injury).</summary>
        public static bool IsRomanticPartnerWithHarvey()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship == null)
                return false;

            return friendship.TryGetValue("Harvey", out var data)
                && (data.IsMarried() || data.IsDating() || data.IsEngaged());
        }

        /// <summary>
        /// Уровень отношений для писем: LowHearts (0–3), MidHearts (4–7), HighHearts (8+), Dating, Married.
        /// </summary>
        public static HarveyRelationshipTier GetHarveyRelationshipTier()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship == null || !friendship.TryGetValue("Harvey", out var data))
                return HarveyRelationshipTier.LowHearts;

            if (data.IsMarried())
                return HarveyRelationshipTier.Married;
            if (data.IsDating())
                return HarveyRelationshipTier.Dating;

            int hearts = data.Points / 250;
            if (hearts >= 8)
                return HarveyRelationshipTier.HighHearts;
            if (hearts >= 4)
                return HarveyRelationshipTier.MidHearts;
            return HarveyRelationshipTier.LowHearts;
        }

        /// <summary>Суффикс tier для mailId: LowHearts, MidHearts, Dating, Married.</summary>
        public static string GetHarveyRelationshipTierSuffix() =>
            GetHarveyRelationshipTier().ToString();

        /// <summary>
        /// Тон proximity-реплик по отношениям с Харви: Low / Mid / High / Romantic.
        /// Romantic — только dating или married; 8+ сердец без романтики = High.
        /// </summary>
        public static string GetRelationshipToneWithHarvey()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship == null || !friendship.TryGetValue("Harvey", out var data))
                return "Low";

            if (data.IsMarried() || data.IsDating())
                return "Romantic";

            int hearts = data.Points / 250;
            if (hearts >= 8)
                return "High";
            if (hearts >= 4)
                return "Mid";
            return "Low";
        }

        /// <summary>
        /// Получить количество сердец с Харви
        /// </summary>
        public static int GetHeartsWithHarvey()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship != null && friendship.TryGetValue("Harvey", out var data))
            {
                return data.Points / 250; // 250 очков = 1 сердце
            }
            return 0;
        }

        /// <summary>
        /// Найти Харви в локации
        /// </summary>
        public static NPC? FindHarveyInLocation(GameLocation location)
        {
            foreach (var npc in location.characters)
            {
                if (npc.Name.Equals("Harvey", StringComparison.OrdinalIgnoreCase))
                    return npc;
            }
            return null;
        }

        /// <summary>
        /// Найти Харви в текущей локации (alias)
        /// </summary>
        public static NPC? FindHarvey(GameLocation location)
        {
            return FindHarveyInLocation(location);
        }

        /// <summary>
        /// Есть ли Харви на тайле (GrabTile — тайл клика по мануалу SMAPI).
        /// </summary>
        public static NPC? GetHarveyAtTile(GameLocation location, Vector2 tile)
        {
            var npc = location.isCharacterAtTile(tile);
            return npc?.Name?.Equals("Harvey", StringComparison.OrdinalIgnoreCase) == true ? npc : null;
        }

        /// <summary>
        /// Надёжный поиск Харви при action-клике: курсор → grab игрока → соседние тайлы.
        /// </summary>
        public static NPC? TryGetInteractedHarvey(
            GameLocation location,
            Vector2 cursorGrabTile,
            bool lenientDistance = false)
        {
            var harvey = GetHarveyAtTile(location, cursorGrabTile);
            if (harvey != null)
                return harvey;

            harvey = GetHarveyAtTile(location, Game1.player.GetGrabTile());
            if (harvey != null)
                return harvey;

            float maxDistance = lenientDistance ? 3f : 2f;
            NPC? nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var character in location.characters)
            {
                if (!character.Name.Equals("Harvey", StringComparison.OrdinalIgnoreCase))
                    continue;

                float distance = Vector2.Distance(character.Tile, Game1.player.Tile);
                if (distance <= maxDistance && distance < nearestDist)
                {
                    nearest = character;
                    nearestDist = distance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Проверить расстояние между Харви и игроком (в клетках)
        /// </summary>
        public static float GetDistanceToPlayer(NPC harvey)
        {
            if (harvey == null || Game1.player == null) return float.MaxValue;
            return Vector2.Distance(harvey.Position, Game1.player.Position) / Game1.tileSize;
        }

        /// <summary>
        /// Эмоция заботы о пациенте: ♥ только при dating/married, иначе профессиональное «!».
        /// </summary>
        public static int GetCaringEmote() =>
            IsDatingOrMarriedToHarvey() ? Emotes.Heart : Emotes.Exclamation;

        /// <summary>
        /// Эмоция завершения лечения: ♥ только при dating/married, иначе улыбка.
        /// </summary>
        public static int GetRecoveryEmote() =>
            IsDatingOrMarriedToHarvey() ? Emotes.Heart : Emotes.Happy;

        /// <summary>
        /// Проверить, находится ли Харви рядом с игроком (3-5 клеток)
        /// </summary>
        public static bool IsNearPlayer(NPC harvey, float maxDistance = 5f)
        {
            if (harvey == null) return false;
            float distance = GetDistanceToPlayer(harvey);
            return distance <= maxDistance;
        }
    }
}

