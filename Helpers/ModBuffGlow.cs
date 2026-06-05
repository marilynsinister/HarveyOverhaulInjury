using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewValley.GameData.Buffs;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Отключение цветного свечения персонажа от баффов/дебаффов Harvey Overhaul.
    /// </summary>
    internal static class ModBuffGlow
    {
        private static readonly HashSet<string> ExtraModBuffIds = new(StringComparer.OrdinalIgnoreCase)
        {
            CureBuffs.Teracitin,
            CureBuffs.Antibiotics,
            CureBuffs.ForcedSedation,
            CureBuffs.PostSurgical,
            CureBuffs.Treatment,
            CureBuffs.IntensiveCare,
            CureBuffs.Protection,
            CureBuffs.Recovery,
            CureBuffs.Care,
            InjuryBuffs.WetBandage,
            InjuryBuffs.DirtyWound,
            InjuryBuffs.Neglect,
            InjuryBuffs.PainFlare,
            InjuryBuffs.AllergicRash,
            InjuryBuffs.WetStitches,
            InjuryBuffs.MineForbidden,
            InjuryBuffs.MineRestricted,
            "buffSleepy",
            "buffFarmerExhausted",
            "buffTooCold",
            "buffAlcoholPoisoning",
        };

        public static bool IsModBuff(string buffId)
        {
            if (string.IsNullOrEmpty(buffId))
                return false;

            if (buffId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
                return true;
            if (buffId.StartsWith("buffHarvey", StringComparison.OrdinalIgnoreCase))
                return true;
            if (buffId.StartsWith("buffStress", StringComparison.OrdinalIgnoreCase))
                return true;
            if (InjurySets.HarveyTreatable.Contains(buffId))
                return true;
            if (ExtraModBuffIds.Contains(buffId))
                return true;

            return false;
        }

        public static void StripGlowFromBuffData(IAssetDataForDictionary<string, BuffData> asset)
        {
            foreach (var (id, data) in asset.Data)
            {
                if (IsModBuff(id))
                    data.GlowColor = null;
            }
        }
    }
}
