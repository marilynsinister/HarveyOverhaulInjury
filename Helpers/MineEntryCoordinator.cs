using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Разводит жёсткий MineForbidden и мягкий RecoveryPlan при входе в шахту/вулкан.
    /// </summary>
    public sealed class MineEntryCoordinator
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly RecoveryPlanManager _recoveryPlanManager;
        private readonly CareTrustManager _careTrustManager;
        private readonly ComplianceManager _complianceManager;

        public MineEntryCoordinator(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            RecoveryPlanManager recoveryPlanManager,
            CareTrustManager careTrustManager,
            ComplianceManager complianceManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _recoveryPlanManager = recoveryPlanManager;
            _careTrustManager = careTrustManager;
            _complianceManager = complianceManager;
        }

        /// <summary>Жёсткий физический запрет: активный MineForbidden или тяжёлая травма из списка.</summary>
        public bool ShouldPhysicallyBlockMines()
        {
            int today = GameUtils.Today();
            var state = _stateManager.State;

            if (MineForbiddenHelper.IsMineForbiddenActive(state, _config, today)
                || _buffManager.HasBuff(InjuryBuffs.MineForbidden))
                return true;

            if (!_config.MineForbiddenOnlyForSevereInjuries)
                return MineForbiddenHelper.HasSevereMineCondition(state, _injuryManager, _buffManager, out _);

            return HasAnyActiveSevereMineForbiddenInjury();
        }

        /// <summary>Мягкий режим RecoveryPlan: предупреждение без обязательного выноса.</summary>
        public bool ShouldWarnRecoveryPlanMineEntry()
        {
            if (ShouldPhysicallyBlockMines())
                return false;

            if (!_recoveryPlanManager.HasActiveRecoveryContext())
                return false;

            var plan = _stateManager.GetRecoveryPlan();
            if (plan.IsActive
                && plan.Tasks.Exists(t => t.Id == RecoveryPlanTaskIds.AvoidMines))
                return true;

            string? injuryId = _injuryManager.GetActiveInjury();
            return injuryId != null && RecoveryPlanManager.ShouldAvoidMinesForPlan(injuryId, _stateManager.State);
        }

        /// <summary>Травма из списка жёсткого медицинского запрета шахты.</summary>
        public static bool IsSevereMineForbiddenInjury(string injuryId)
        {
            if (string.IsNullOrEmpty(injuryId))
                return false;

            if (InjurySets.Severe.Contains(injuryId) || InjurySets.Critical.Contains(injuryId))
                return true;

            if (string.Equals(injuryId, "buffDeepCuts", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>DeepCuts — только острая фаза; остальные Severe — всегда.</summary>
        public bool IsActiveSevereMineForbiddenInjury(string injuryId)
        {
            if (!IsSevereMineForbiddenInjury(injuryId))
                return false;

            if (string.Equals(injuryId, "buffDeepCuts", StringComparison.OrdinalIgnoreCase))
            {
                if (_buffManager.HasBuff("HarveyMod_DeepCuts_Acute"))
                    return true;

                DebuffState? ds = _stateManager.GetDebuffState(injuryId);
                return ds != null && ds.CurrentPhase == 1;
            }

            return _injuryManager.HasInjuryOrPhase(injuryId) || _buffManager.HasBuff(injuryId);
        }

        /// <summary>Мягкий вход при активном RecoveryPlan (без MineForbidden).</summary>
        public void HandleMineEntryDuringRecoveryPlan(GameLocation location, int today, bool isVolcano)
        {
            _monitor.Log("[RecoveryPlan] Вход в шахту/вулкан — мягкое предупреждение", LogLevel.Info);

            Game1.addHUDMessage(new HUDMessage(
                RecoveryPlanTexts.Hud.MinesSoftWarning,
                HUDMessage.health_type));

            _recoveryPlanManager.RegisterRecoveryPlanMineViolation(
                RecoveryViolationSeverity.Medium,
                blockPhysicalExit: _config.RecoveryPlanMineRuleBlocksEntry);

            if (_config.RecoveryPlanMineRuleBlocksEntry)
            {
                _monitor.Log("[RecoveryPlan] RecoveryPlanMineRuleBlocksEntry=true — физический вынос", LogLevel.Warn);
                Game1.playSound("cancel");
            }
        }

        /// <summary>Жёсткий запрет: диалог, вынос, severe violation в RecoveryPlan при необходимости.</summary>
        public void HandleMineEntryDuringMineForbidden(
            GameLocation location,
            int today,
            bool isVolcano,
            Action<GameLocation> warpOut,
            Func<bool> tryStartInterceptionEvent)
        {
            var state = _stateManager.State;

            EnsureMineForbiddenBuffApplied(today, "mine_entry_block");

            bool hasSevereInjury = MineForbiddenHelper.HasSevereMineCondition(
                state, _injuryManager, _buffManager, out _);

            string hud = _careTrustManager.GetMineWarningHudLine(hasSevereInjury, forbidden: true);
            Game1.addHUDMessage(new HUDMessage(hud, HUDMessage.error_type));
            _monitor.Log($"[MineForbidden] Блокировка входа: {hud}", LogLevel.Warn);

            if (_recoveryPlanManager.HasActiveRecoveryContext())
            {
                _recoveryPlanManager.RegisterRecoveryPlanMineViolation(
                    RecoveryViolationSeverity.Severe,
                    blockPhysicalExit: true);
            }

            if (isVolcano)
            {
                Game1.playSound("debuffHit");
                warpOut(location);
                return;
            }

            if (state.LastMineForbiddenInterceptionDay == today)
                _complianceManager.AddCompliance(-2, "mine_forbidden_repeat");

            if (state.LastMineForbiddenInterceptionDay != today)
            {
                state.LastMineForbiddenInterceptionDay = today;
                _stateManager.Save();

                if (tryStartInterceptionEvent())
                    return;

                _monitor.Log("[MineForbidden] Событие не запустилось — fallback HUD + warp", LogLevel.Warn);
            }

            Game1.playSound("cancel");
            warpOut(location);
        }

        /// <summary>Тексты задачи «шахта» для UI плана восстановления.</summary>
        public static (string Title, string Description) GetMineTaskLabels(
            InjuryState state,
            ModConfig config,
            BuffManager buffManager,
            int today)
        {
            bool forbidden = MineForbiddenHelper.IsMineForbiddenActive(state, config, today)
                || buffManager.HasBuff(InjuryBuffs.MineForbidden);

            if (forbidden)
            {
                int daysLeft = MineForbiddenHelper.GetMineForbiddenDaysLeft(state, config, today);
                string daysText = daysLeft == 1 ? "1 день" : $"{daysLeft} дн.";
                return (
                    RecoveryPlanTexts.Tasks.MinesTitleForbidden,
                    string.Format(RecoveryPlanTexts.Tasks.MinesDescriptionForbidden, daysText));
            }

            return (
                RecoveryPlanTexts.Tasks.MinesTitleRecommended,
                RecoveryPlanTexts.Tasks.MinesDescriptionRecommended);
        }

        /// <summary>Тексты задачи «шахта» для UI (instance wrapper).</summary>
        public (string Title, string Description) GetMineTaskLabels(int today) =>
            GetMineTaskLabels(_stateManager.State, _config, _buffManager, today);

        public void EnsureMineForbiddenBuffApplied(int today, string trigger)
        {
            var state = _stateManager.State;

            if (!ShouldPhysicallyBlockMines() && !MineForbiddenHelper.IsMineForbiddenActive(state, _config, today))
                return;

            if (!HasAnyActiveSevereMineForbiddenInjury()
                && !MineForbiddenHelper.IsMineForbiddenActive(state, _config, today))
                return;

            MineForbiddenHelper.ApplyHardMineForbidden(
                state,
                _config,
                _buffManager,
                _stateManager,
                _monitor,
                today,
                trigger,
                resetAppliedDay: false);

            MineForbiddenHelper.SyncMineForbiddenBuff(
                state, _config, _buffManager, _stateManager, _monitor, today, trigger);
        }

        private bool HasAnyActiveSevereMineForbiddenInjury()
        {
            string? mainId = _injuryManager.GetActiveInjury();
            if (!string.IsNullOrEmpty(mainId) && IsActiveSevereMineForbiddenInjury(mainId))
                return true;

            foreach (string injuryId in _stateManager.State.ActiveDebuffs.Keys)
            {
                if (IsActiveSevereMineForbiddenInjury(injuryId))
                    return true;
            }

            return false;
        }
    }
}
