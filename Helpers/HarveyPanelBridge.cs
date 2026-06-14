using HarveyOverhaul.InjuryCare.Api;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Опциональный вызов общего окна «План Харви» в HarveyStressMeter.</summary>
    internal static class HarveyPanelBridge
    {
        public const string StressMeterModId = "marilynsinister.HarveyStressMeter";

        public static void TryOpenSharedPanel(IModHelper helper, string tabId, string hudFallbackMessage)
        {
            if (!Context.IsWorldReady)
                return;

            if (!helper.ModRegistry.IsLoaded(StressMeterModId))
            {
                Game1.addHUDMessage(new HUDMessage(hudFallbackMessage, HUDMessage.newQuest_type));
                return;
            }

            var panelApi = helper.ModRegistry.GetApi<IHarveyPanelHostApi>(StressMeterModId);
            if (panelApi == null)
            {
                Game1.addHUDMessage(new HUDMessage(hudFallbackMessage, HUDMessage.newQuest_type));
                return;
            }

            panelApi.OpenPanel(tabId);
        }
    }
}
