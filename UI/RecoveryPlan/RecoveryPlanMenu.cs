using System;
using System.IO;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewUI.Framework;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.UI.RecoveryPlan
{
    /// <summary>
    /// Минимальная интеграция StardewUI для окна плана восстановления.
    /// </summary>
    public sealed class RecoveryPlanMenu
    {
        public const string ModUniqueId = "marilynsinister.HarveyOverhaul.Injury";
        public const string ViewAssetName = "Mods/marilynsinister.HarveyOverhaul.Injury/Views/RecoveryPlan";
        private const string HudNoActivePlan = "Сейчас нет активного плана восстановления.";

        private readonly IMonitor _monitor;
        private IViewEngine? _viewEngine;
        private bool _assetsRegistered;

        public RecoveryPlanMenu(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public bool IsAvailable => _viewEngine != null;

        public void TryInitialize(IModHelper helper)
        {
            if (_viewEngine != null)
                return;

            _viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
            if (_viewEngine == null)
            {
                _monitor.Log("[RecoveryPlanUI] StardewUI API недоступен — окно плана не откроется.", LogLevel.Warn);
                return;
            }

            string viewsDirectory = Path.Combine(helper.DirectoryPath, "assets", "views");
            _viewEngine.RegisterViews($"Mods/{ModUniqueId}/Views", viewsDirectory);
            _viewEngine.PreloadModels(typeof(RecoveryPlanViewModel));
            _viewEngine.PreloadAssets();
            _assetsRegistered = true;

            _monitor.Log("[RecoveryPlanUI] StardewUI views зарегистрированы.", LogLevel.Debug);
        }

        public void TryOpen(RecoveryPlanManager recoveryPlanManager)
        {
            if (!Context.IsWorldReady)
                return;

            if (!HasPlanToDisplay(recoveryPlanManager))
            {
                Game1.addHUDMessage(new HUDMessage(HudNoActivePlan, HUDMessage.health_type));
                return;
            }

            if (_viewEngine == null)
            {
                _monitor.Log("[RecoveryPlanUI] StardewUI не инициализирован.", LogLevel.Warn);
                Game1.addHUDMessage(new HUDMessage(HudNoActivePlan, HUDMessage.health_type));
                return;
            }

            if (!_assetsRegistered)
            {
                _monitor.Log("[RecoveryPlanUI] Views не зарегистрированы.", LogLevel.Warn);
                return;
            }

            if (!Context.IsPlayerFree)
            {
                _monitor.Log("[RecoveryPlanUI] Игрок занят — окно не открыто.", LogLevel.Debug);
                return;
            }

            if (Game1.activeClickableMenu != null)
            {
                _monitor.Log("[RecoveryPlanUI] Уже открыто меню — окно не открыто.", LogLevel.Debug);
                return;
            }

            RecoveryPlanViewModel viewModel = RecoveryPlanViewModel.FromDto(recoveryPlanManager.BuildViewModel());
            Game1.activeClickableMenu = _viewEngine.CreateMenuFromAsset(ViewAssetName, viewModel);
            _monitor.Log("[RecoveryPlanUI] Окно плана восстановления открыто.", LogLevel.Debug);
        }

        public static bool HasPlanToDisplay(RecoveryPlanManager recoveryPlanManager)
        {
            RecoveryPlanState? plan = recoveryPlanManager.GetActivePlan();
            return plan != null && (plan.IsActive || plan.CompletionTalkPending);
        }
    }
}
