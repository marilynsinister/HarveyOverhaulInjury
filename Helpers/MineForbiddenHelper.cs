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

    /// <summary>Запрет шахты: короткий hard-ban по фазе/дням + мягкие предупреждения при длительном лечении.</summary>
    public static class MineForbiddenHelper
    {
        public const int RestrictionStrikesForHardBan = 3;
        public const int RestrictedLongStayMinutes = 45;
        public const double RestrictedDirtyChanceBonus = 0.12;
        public const double RestrictedPainFlareChance = 0.18;
        public const double RestrictedRepeatNeglectChance = 0.22;

        private const int ConcussionAcuteHardBlockDays = 3;

        private static readonly string[] EmergencyMineBlockTopics =
        {
            "topicOverprotectiveMode",
            ConversationTopics.HealthDamageCritical,
            ConversationTopics.HealthDamageSevere,
            ConversationTopics.PostOperativeCare,
            ConversationTopics.MineInjuryRescue,
        };

        /// <summary>Фазовое лечение: острая фаза 1 → короткий жёсткий запрет при старте лечения.</summary>
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

        /// <summary>
        /// Жёсткий физический запрет шахты: активный MineForbidden, emergency-топики или острое окно травмы.
        /// Не использует InjurySets.Severe напрямую.
        /// </summary>
        public static bool IsMineHardBlocked(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today,
            out string? reason)
        {
            reason = null;

            if (IsMineForbiddenActive(state, config, today))
            {
                reason = "HarveyMod_MineForbidden active";
                return true;
            }

            if (buffManager.HasBuff(InjuryBuffs.MineForbidden))
            {
                reason = "HarveyMod_MineForbidden buff";
                return true;
            }

            if (HasEmergencyMineBlock(buffManager))
            {
                reason = "emergency topic or supervision";
                return true;
            }

            foreach (string injuryId in CollectActiveMineInjuryIds(state, injuryManager))
            {
                if (!ShouldHardBlockInjury(injuryId, state, config, injuryManager, buffManager, today, out string? injuryReason))
                    continue;

                reason = injuryReason;
                return true;
            }

            return false;
        }

        /// <summary>Alias для читаемости в обработчиках.</summary>
        public static bool ShouldHardBlockMines(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today,
            out string? reason) =>
            IsMineHardBlocked(state, config, injuryManager, buffManager, today, out reason);

        /// <summary>Есть активная травма/фаза, при которой стоит мягко предупредить (без MineWarningDay).</summary>
        public static bool ShouldShowSoftMineWarning(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today)
        {
            if (!config.ShowMineWarningsDuringHealing)
                return false;

            if (IsMineHardBlocked(state, config, injuryManager, buffManager, today, out _))
                return false;

            if (state.ActiveComplications.Count > 0)
                return true;

            foreach (string injuryId in CollectActiveMineInjuryIds(state, injuryManager))
            {
                if (injuryManager.HasInjuryOrPhase(injuryId) || buffManager.HasBuff(injuryId))
                    return true;
            }

            if (InjurySets.LimitedActivity.Any(id => buffManager.HasBuff(id)))
                return true;

            return !string.IsNullOrEmpty(injuryManager.GetActiveInjury());
        }

        public static string GetStrictMineWarningText()
        {
            return "Харви: Нет. Сегодня шахта закрыта. У тебя острая фаза, и я не позволю тебе сорвать лечение.";
        }

        public static string GetSoftMineWarningText(
            InjuryState state,
            InjuryManager injuryManager,
            BuffManager buffManager)
        {
            string? mainId = injuryManager.GetActiveInjury();

            if (string.Equals(mainId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase)
                && buffManager.HasBuff("HarveyMod_InfectedWound_Treatment"))
            {
                return "Харви: Я всё ещё считаю шахту плохой идеей. Но если тебе действительно нужно железо — не спускайся глубоко и уходи при первых признаках слабости.";
            }

            if (mainId != null && InjurySets.LimitedActivity.Contains(mainId))
            {
                return "Харви: С такой травмой шахта — плохая идея. Хотя бы не перегружайся.";
            }

            if (mainId != null && (InjurySets.Severe.Contains(mainId) || InjurySets.Critical.Contains(mainId)))
            {
                return "Харви: Я всё ещё переживаю за твоё лечение. Будь осторожна и не задерживайся глубоко.";
            }

            return "Харви: Будь осторожна в шахте — твои раны могут загрязниться.";
        }

        /// <summary>Legacy: любая тяжёлая травма/фаза (отчёты, trust — не для блокировки).</summary>
        public static bool HasSevereMineCondition(
            InjuryState state,
            InjuryManager injuryManager,
            BuffManager buffManager,
            out List<string> sources)
        {
            sources = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string injuryId in CollectActiveMineInjuryIds(state, injuryManager))
            {
                if (!InjurySets.Severe.Contains(injuryId) && !InjurySets.Critical.Contains(injuryId))
                    continue;

                DebuffState? ds = state.ActiveDebuffs.GetValueOrDefault(injuryId);
                if (ds?.TreatmentStarted == true)
                    TryAddSource(seen, sources, $"main:{injuryId} treatment phase {ds.CurrentPhase}/{ds.TotalPhases}");
                else if (injuryManager.HasInjuryOrPhase(injuryId))
                    TryAddSource(seen, sources, injuryId);
                else if (buffManager.HasBuff(injuryId))
                    TryAddSource(seen, sources, $"{injuryId} base");
            }

            return sources.Count > 0;
        }

        public static bool ShouldMineRestricted(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today)
        {
            if (config.AllowMinesDuringHealingPhase)
                return false;

            return !IsMineForbiddenActive(state, config, today)
                   && HasSevereMineCondition(state, injuryManager, buffManager, out _);
        }

        public static MineAccessMode GetMineAccessMode(
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today)
        {
            if (IsMineForbiddenActive(state, config, today)
                || IsMineHardBlocked(state, config, injuryManager, buffManager, today, out _))
                return MineAccessMode.Forbidden;

            if (ShouldMineRestricted(state, config, injuryManager, buffManager, today))
                return MineAccessMode.Restricted;

            return MineAccessMode.Allowed;
        }

        /// <summary>Наложить жёсткий запрет на MineForbiddenDurationDays.</summary>
        public static void ApplyHardMineForbidden(
            InjuryState state,
            ModConfig config,
            BuffManager buffManager,
            StateManager stateManager,
            IMonitor monitor,
            int today,
            string trigger,
            bool resetAppliedDay = true)
        {
            bool alreadyActive = IsMineForbiddenActive(state, config, today);

            if (resetAppliedDay || state.MineForbiddenAppliedDay < 0)
            {
                if (!alreadyActive || resetAppliedDay)
                    state.MineForbiddenAppliedDay = today;
            }

            buffManager.RemoveBuff(InjuryBuffs.MineRestricted);
            state.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, InjuryBuffs.MineRestricted, StringComparison.OrdinalIgnoreCase));

            if (!buffManager.HasBuff(InjuryBuffs.MineForbidden))
                buffManager.AddBuff(InjuryBuffs.MineForbidden, -2);

            stateManager.Save();
            monitor.Log(
                $"[MineForbidden] Жёсткий запрет на {GetMineForbiddenDurationDays(config)} дн. "
                + $"(trigger={trigger}, appliedDay={state.MineForbiddenAppliedDay}, reset={resetAppliedDay})",
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
            IsMineHardBlocked(state, config, injuryManager, buffManager, today, out string? hardReason);

            var sb = new StringBuilder();
            sb.AppendLine("=== Mine access status ===");
            sb.AppendLine($"Mode: {mode} (entry blocked only when Forbidden/hard block)");
            sb.AppendLine($"IsMineHardBlocked: {!string.IsNullOrEmpty(hardReason)} ({hardReason ?? "none"})");
            sb.AppendLine($"HarveyMod_MineForbidden: {buffManager.HasBuff(InjuryBuffs.MineForbidden)}");
            sb.AppendLine($"HarveyMod_MineRestricted: {buffManager.HasBuff(InjuryBuffs.MineRestricted)}");
            sb.AppendLine($"AllowMinesDuringHealingPhase: {config.AllowMinesDuringHealingPhase}");
            sb.AppendLine($"MineHardBlockAcuteDays: {config.MineHardBlockAcuteDays}");
            sb.AppendLine($"MineHardBlockAfterMajorInjuryDays: {config.MineHardBlockAfterMajorInjuryDays}");
            sb.AppendLine($"RecoveryPlanMineRuleBlocksEntry: {config.RecoveryPlanMineRuleBlocksEntry}");
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

        private static bool ShouldHardBlockInjury(
            string injuryId,
            InjuryState state,
            ModConfig config,
            InjuryManager injuryManager,
            BuffManager buffManager,
            int today,
            out string? reason)
        {
            reason = null;

            if (!IsMineRelevantInjury(injuryId, injuryManager, buffManager))
                return false;

            DebuffState? ds = state.ActiveDebuffs.GetValueOrDefault(injuryId);
            int hardBlockDays = GetHardBlockDayLimit(injuryId, ds, config);
            if (hardBlockDays <= 0)
                return false;

            int daysSinceStart = GetHardBlockDaysElapsed(injuryId, ds, today);
            if (daysSinceStart >= hardBlockDays)
                return false;

            int phase = ds?.CurrentPhase ?? 0;
            reason = $"{injuryId} phase={phase} day={daysSinceStart + 1}/{hardBlockDays}";
            return true;
        }

        private static int GetHardBlockDayLimit(string injuryId, DebuffState? ds, ModConfig config)
        {
            int phase = ds?.CurrentPhase ?? 0;
            bool inTreatment = ds?.TreatmentStarted == true && phase > 0;

            if (inTreatment && phase >= 2 && config.AllowMinesDuringHealingPhase)
            {
                if (string.Equals(injuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(injuryId, "buffBurnWounds", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(injuryId, "buffConcussion", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(injuryId, "buffShrapnelWounds", StringComparison.OrdinalIgnoreCase))
                    return 0;
            }

            if (string.Equals(injuryId, "buffDeepCuts", StringComparison.OrdinalIgnoreCase))
            {
                if (inTreatment && phase > 1)
                    return 0;

                return phase <= 1 ? Math.Max(1, config.MineHardBlockAcuteDays) : 0;
            }

            if (string.Equals(injuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffBurnWounds", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffShrapnelWounds", StringComparison.OrdinalIgnoreCase))
            {
                if (!inTreatment || phase <= 1)
                    return Math.Max(1, config.MineHardBlockAcuteDays);

                return 0;
            }

            if (string.Equals(injuryId, "buffConcussion", StringComparison.OrdinalIgnoreCase))
            {
                if (!inTreatment || phase <= 1)
                    return ConcussionAcuteHardBlockDays;

                return 0;
            }

            if (string.Equals(injuryId, "buffFracturedBone", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Max(1, config.MineHardBlockAfterMajorInjuryDays);
            }

            if (InjurySets.Severe.Contains(injuryId) || InjurySets.Critical.Contains(injuryId))
                return Math.Max(1, config.MineHardBlockAfterMajorInjuryDays);

            return 0;
        }

        private static int GetHardBlockDaysElapsed(string injuryId, DebuffState? ds, int today)
        {
            if (ds == null)
                return 0;

            bool useInjuryStart = string.Equals(injuryId, "buffFracturedBone", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase);

            if (!useInjuryStart && ds.TreatmentStarted && ds.CurrentPhase > 0 && ds.PhaseStartDay > 0)
                return Math.Max(0, today - ds.PhaseStartDay);

            if (ds.InjuryStartDay > 0)
                return Math.Max(0, today - ds.InjuryStartDay);

            return 0;
        }

        private static bool IsMineRelevantInjury(
            string injuryId,
            InjuryManager injuryManager,
            BuffManager buffManager) =>
            injuryManager.HasInjuryOrPhase(injuryId) || buffManager.HasBuff(injuryId);

        private static IEnumerable<string> CollectActiveMineInjuryIds(InjuryState state, InjuryManager injuryManager)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string? mainId = injuryManager.GetActiveInjury();
            if (!string.IsNullOrEmpty(mainId))
                seen.Add(mainId);

            foreach (string injuryId in state.ActiveDebuffs.Keys)
                seen.Add(injuryId);

            return seen;
        }

        private static bool HasEmergencyMineBlock(BuffManager buffManager)
        {
            if (buffManager.HasBuff("buffEmergencySupervision"))
                return true;

            foreach (string topic in EmergencyMineBlockTopics)
            {
                if (GameUtils.HasConversationTopic(topic))
                    return true;
            }

            return false;
        }

        private static void TryAddSource(HashSet<string> seen, List<string> sources, string entry)
        {
            if (seen.Add(entry))
                sources.Add(entry);
        }
    }
}
