using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Жёсткий временной запрет (HarveyMod_MineForbidden) vs мягкое ограничение (HarveyMod_MineRestricted).</summary>
    public enum MineAccessMode
    {
        Allowed,
        Restricted,
        Forbidden,
    }

    /// <summary>Запрет шахты: короткий hard-ban + мягкий restricted при длительном лечении Severe.</summary>
    public static class MineForbiddenHelper
    {
        public const int RestrictionStrikesForHardBan = 3;
        public const int RestrictedLongStayMinutes = 45;
        public const double RestrictedDirtyChanceBonus = 0.12;
        public const double RestrictedPainFlareChance = 0.18;
        public const double RestrictedRepeatNeglectChance = 0.22;

        /// <summary>Фазовое лечение: острая фаза 1 → короткий жёсткий запрет.</summary>
        public static readonly HashSet<string> SevereAcutePhase1Treatment = new(StringComparer.OrdinalIgnoreCase)
        {
            "buffInfectedWound",
            "buffConcussion",
            "buffFracturedBone",
            "buffBurnWounds",
            "buffShrapnelWounds",
        };

        public static int GetMineForbiddenDurationDays(ModConfig config) =>
            Math.Max(1, config.MineForbiddenDurationDays);

        public static int GetMineForbiddenDaysLeft(InjuryState state, ModConfig config, int today)
        {
            if (state.MineForbiddenAppliedDay < 0)
                return 0;

            return Math.Max(0, state.MineForbiddenAppliedDay + GetMineForbiddenDurationDays(config) - today);
        }

        /// <summary>Жёсткий запрет: только пока не истёк срок по MineForbiddenAppliedDay.</summary>
        public static bool IsMineForbiddenActive(InjuryState state, ModConfig config, int today) =>
            state.MineForbiddenAppliedDay >= 0 && GetMineForbiddenDaysLeft(state, config, today) > 0;

        public static bool HasSevereMineCondition(
            InjuryState state,
            InjuryManager injuryManager,
            BuffManager buffManager,
            out List<string> sources)
        {
            sources = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string? mainId = injuryManager.GetActiveInjury();
            if (!string.IsNullOrEmpty(mainId) && InjurySets.Severe.Contains(mainId))
                CollectSevereEntry(mainId, state, injuryManager, buffManager, seen, sources);

            foreach (var kv in state.ActiveDebuffs)
            {
                if (InjurySets.Severe.Contains(kv.Key))
                    CollectSevereEntry(kv.Key, state, injuryManager, buffManager, seen, sources);
            }

            foreach (string phaseBuffId in InjurySets.SeverePhaseBuffIds)
            {
                if (buffManager.HasBuff(phaseBuffId))
                    TryAddSource(seen, sources, $"phaseBuff:{phaseBuffId}");
            }

            return sources.Count > 0;
        }

        public static bool ShouldMineRestricted(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today) =>
            !IsMineForbiddenActive(state, config, today)
            && HasSevereMineCondition(state, injuryManager, buffManager, out _);

        public static MineAccessMode GetMineAccessMode(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today)
        {
            if (IsMineForbiddenActive(state, config, today))
                return MineAccessMode.Forbidden;

            if (ShouldMineRestricted(state, config, injuryManager, buffManager, today))
                return MineAccessMode.Restricted;

            return MineAccessMode.Allowed;
        }

        /// <summary>Наложить жёсткий запрет на MineForbiddenDurationDays (сбрасывает срок с сегодня).</summary>
        public static void ApplyHardMineForbidden(
            InjuryState state,
            ModConfig config,
            BuffManager buffManager,
            StateManager stateManager,
            IMonitor monitor,
            int today,
            string trigger)
        {
            state.MineForbiddenAppliedDay = today;
            buffManager.RemoveBuff(InjuryBuffs.MineRestricted);
            state.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, InjuryBuffs.MineRestricted, StringComparison.OrdinalIgnoreCase));

            if (!buffManager.HasBuff(InjuryBuffs.MineForbidden))
                buffManager.AddBuff(InjuryBuffs.MineForbidden, -2);

            stateManager.Save();
            monitor.Log(
                $"[MineForbidden] Жёсткий запрет на {GetMineForbiddenDurationDays(config)} дн. (trigger={trigger}, appliedDay={today})",
                LogLevel.Info);
        }

        public static bool IsUntreatedSevereForMineEntry(
            InjuryState state,
            InjuryManager injuryManager,
            BuffManager buffManager)
        {
            string? mainId = injuryManager.GetActiveInjury();
            if (string.IsNullOrEmpty(mainId) || !InjurySets.Severe.Contains(mainId))
                return false;

            DebuffState? ds = state.ActiveDebuffs.GetValueOrDefault(mainId);
            if (ds?.TreatmentStarted == true)
                return false;

            return injuryManager.HasInjuryOrPhase(mainId) || buffManager.HasBuff(mainId);
        }

        public static bool ShouldHardBanOnTreatmentStart(string injuryId, int phase) =>
            string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase)
            || (phase == 1 && SevereAcutePhase1Treatment.Contains(injuryId));

        public static void ResetDailyRestrictionViolations(InjuryState state, int today)
        {
            if (state.LastMineRestrictionViolationDay == today)
                return;

            state.MineRestrictionViolationsToday = 0;
            state.LastMineRestrictionViolationDay = today;
        }

        /// <summary>Утро: сброс дневных нарушений, истечение hard-ban, переход на restricted.</summary>
        public static bool ProcessDayStarted(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            StateManager stateManager,
            IMonitor monitor,
            int today)
        {
            ResetDailyRestrictionViolations(state, today);
            bool changed = false;

            if (state.MineForbiddenAppliedDay >= 0 && !IsMineForbiddenActive(state, config, today))
            {
                state.MineForbiddenAppliedDay = -1;
                buffManager.RemoveBuff(InjuryBuffs.MineForbidden);
                state.SavedActiveBuffs.RemoveAll(id =>
                    string.Equals(id, InjuryBuffs.MineForbidden, StringComparison.OrdinalIgnoreCase));
                monitor.Log("[MineForbidden] Жёсткий запрет истёк", LogLevel.Info);
                changed = true;

                if (HasSevereMineCondition(state, injuryManager, buffManager, out _))
                {
                    monitor.Log(
                        "[MineRestricted] После истечения запрета — мягкое ограничение (тяжёлая травма ещё активна)",
                        LogLevel.Info);
                }
            }

            changed |= SyncMineForbiddenBuff(state, config, buffManager, stateManager, monitor, today, "DayStarted");
            changed |= SyncMineRestrictedBuff(state, config, injuryManager, buffManager, stateManager, monitor, today, "DayStarted");
            return changed;
        }

        public static bool SyncMineForbiddenBuff(
            InjuryState state,
            ModConfig config,
            BuffManager buffManager,
            StateManager stateManager,
            IMonitor monitor,
            int today,
            string trigger)
        {
            bool should = IsMineForbiddenActive(state, config, today);
            bool has = buffManager.HasBuff(InjuryBuffs.MineForbidden);

            if (should && !has)
            {
                buffManager.AddBuff(InjuryBuffs.MineForbidden, -2);
                stateManager.Save();
                monitor.Log($"[MineForbidden] Восстановлен дебафф (trigger={trigger})", LogLevel.Debug);
                return true;
            }

            if (!should && has)
            {
                buffManager.RemoveBuff(InjuryBuffs.MineForbidden);
                state.SavedActiveBuffs.RemoveAll(id =>
                    string.Equals(id, InjuryBuffs.MineForbidden, StringComparison.OrdinalIgnoreCase));
                stateManager.Save();
                monitor.Log($"[MineForbidden] Снят дебафф (trigger={trigger})", LogLevel.Debug);
                return true;
            }

            return false;
        }

        public static bool SyncMineRestrictedBuff(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            StateManager stateManager,
            IMonitor monitor,
            int today,
            string trigger)
        {
            bool should = ShouldMineRestricted(state, config, injuryManager, buffManager, today);
            bool has = buffManager.HasBuff(InjuryBuffs.MineRestricted);

            if (should && !has)
            {
                buffManager.AddBuff(InjuryBuffs.MineRestricted, -2);
                stateManager.Save();
                monitor.Log($"[MineRestricted] Включено мягкое ограничение (trigger={trigger})", LogLevel.Info);
                return true;
            }

            if (!should && has)
            {
                buffManager.RemoveBuff(InjuryBuffs.MineRestricted);
                state.SavedActiveBuffs.RemoveAll(id =>
                    string.Equals(id, InjuryBuffs.MineRestricted, StringComparison.OrdinalIgnoreCase));
                stateManager.Save();
                monitor.Log($"[MineRestricted] Снято ограничение (trigger={trigger})", LogLevel.Debug);
                return true;
            }

            return false;
        }

        public static void ClearMineRestrictedState(InjuryState state, BuffManager buffManager)
        {
            state.MineRestrictionViolationsToday = 0;
            state.LastMineRestrictionViolationDay = -1;
            state.MineRestrictionStrikes = 0;
            buffManager.RemoveBuff(InjuryBuffs.MineRestricted);
            state.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, InjuryBuffs.MineRestricted, StringComparison.OrdinalIgnoreCase));
        }

        public static void ClearHardMineForbiddenState(InjuryState state, BuffManager buffManager)
        {
            state.MineWarningDay = -1;
            state.MineForbiddenAppliedDay = -1;
            state.LastMineForbiddenInterceptionDay = -1;
            buffManager.RemoveBuff(InjuryBuffs.MineForbidden);
            state.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, InjuryBuffs.MineForbidden, StringComparison.OrdinalIgnoreCase));
        }

        public static bool TryEscalateRestrictionToHardBan(
            InjuryState state,
            ModConfig config,
            BuffManager buffManager,
            StateManager stateManager,
            IMonitor monitor,
            int today,
            string trigger)
        {
            if (state.MineRestrictionStrikes < RestrictionStrikesForHardBan)
                return false;

            ApplyHardMineForbidden(state, config, buffManager, stateManager, monitor, today, trigger);
            state.MineRestrictionStrikes = Math.Max(0, state.MineRestrictionStrikes - 1);
            state.MineRestrictionViolationsToday = 0;
            stateManager.Save();
            monitor.Log(
                $"[MineRestricted] Повторные нарушения → жёсткий запрет (strikes>={RestrictionStrikesForHardBan}, trigger={trigger})",
                LogLevel.Warn);
            return true;
        }

        public static string FormatHardBanEntryHud(ModConfig config, InjuryState state, int today, bool isVolcano)
        {
            int daysLeft = GetMineForbiddenDaysLeft(state, config, today);
            string daysText = daysLeft == 1 ? "1 день" : $"{daysLeft} дн.";

            return isVolcano
                ? $"Харви запретил опасные подземелья ещё на {daysText}. Сначала контрольный осмотр."
                : $"Харви запретил шахту ещё на {daysText}. Сначала контрольный осмотр.";
        }

        public static string FormatRestrictedEntryHud() =>
            "Харви разрешил шахту только с ограничениями. Риск осложнений повышен.";

        public static string FormatAppliedHud(ModConfig config, int today, InjuryState state)
        {
            int duration = GetMineForbiddenDurationDays(config);
            int left = GetMineForbiddenDaysLeft(state, config, today);
            if (left <= 0)
                left = duration;

            return $"Харви запретил шахту на {duration} дн. Осталось: {left} дн.";
        }

        public static double GetRestrictedDirtyChanceMultiplier() => 1.0 + RestrictedDirtyChanceBonus;

        public static string BuildStatusReport(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today)
        {
            var mode = GetMineAccessMode(state, config, injuryManager, buffManager, today);
            HasSevereMineCondition(state, injuryManager, buffManager, out List<string> severeSources);

            var sb = new StringBuilder();
            sb.AppendLine("=== Mine access status ===");
            sb.AppendLine($"Mode: {mode} (entry blocked only when Forbidden)");
            sb.AppendLine($"HarveyMod_MineForbidden: {buffManager.HasBuff(InjuryBuffs.MineForbidden)}");
            sb.AppendLine($"HarveyMod_MineRestricted: {buffManager.HasBuff(InjuryBuffs.MineRestricted)}");
            sb.AppendLine($"MineWarningDay: {state.MineWarningDay}");
            sb.AppendLine($"MineForbiddenAppliedDay: {state.MineForbiddenAppliedDay}");
            sb.AppendLine($"MineForbiddenDurationDays: {GetMineForbiddenDurationDays(config)}");
            sb.AppendLine($"MineForbiddenDaysLeft: {GetMineForbiddenDaysLeft(state, config, today)}");
            sb.AppendLine($"MineRestrictionViolationsToday: {state.MineRestrictionViolationsToday}");
            sb.AppendLine($"LastMineRestrictionViolationDay: {state.LastMineRestrictionViolationDay}");
            sb.AppendLine($"MineRestrictionStrikes: {state.MineRestrictionStrikes} (hard ban at {RestrictionStrikesForHardBan})");
            sb.AppendLine($"LastMineSevereWarningDay: {state.LastMineSevereWarningDay}");
            sb.AppendLine($"LastMineForbiddenInterceptionDay: {state.LastMineForbiddenInterceptionDay}");

            if (severeSources.Count == 0)
                sb.AppendLine("Severe sources: (none)");
            else
            {
                sb.AppendLine("Severe sources:");
                foreach (string source in severeSources)
                    sb.AppendLine($"  - {source}");
            }

            return sb.ToString().TrimEnd();
        }

        private static void CollectSevereEntry(
            string injuryId,
            InjuryState state,
            InjuryManager injuryManager,
            BuffManager buffManager,
            HashSet<string> seen,
            List<string> sources)
        {
            DebuffState? ds = state.ActiveDebuffs.GetValueOrDefault(injuryId);
            if (ds?.TreatmentStarted == true)
                TryAddSource(seen, sources, $"main:{injuryId} treatment phase {ds.CurrentPhase}/{ds.TotalPhases}");
            else if (injuryManager.HasInjuryOrPhase(injuryId))
                TryAddSource(seen, sources, $"{injuryId}");
            else if (buffManager.HasBuff(injuryId))
                TryAddSource(seen, sources, $"{injuryId} base");
        }

        private static void TryAddSource(HashSet<string> seen, List<string> sources, string entry)
        {
            if (seen.Add(entry))
                sources.Add(entry);
        }
    }
}
