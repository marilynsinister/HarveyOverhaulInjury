using System.Text;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HarveyOverhaul.InjuryCare.Testing
{
    /// <summary>
    /// QA: advance cutscenes, dialogue boxes, and generic menus (MCP / console testing).
    /// </summary>
    internal static class QaGameUiCommands
    {
        public static string BuildUiStatusReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            var sb = new StringBuilder();
            sb.AppendLine($"eventUp={Game1.eventUp}");
            sb.AppendLine($"dialogueUp={Game1.dialogueUp}");
            sb.AppendLine($"CurrentEvent={(Game1.CurrentEvent?.id ?? "(none)")}");
            sb.AppendLine($"activeClickableMenu={(Game1.activeClickableMenu?.GetType().Name ?? "(none)")}");
            sb.AppendLine($"IsPlayerFree={Context.IsPlayerFree}");
            sb.AppendLine($"location={Game1.currentLocation?.Name ?? "(null)"}");
            sb.AppendLine($"time={Game1.timeOfDay}");
            return sb.ToString().TrimEnd();
        }

        /// <summary>One UI step: event click, dialogue line, or menu click.</summary>
        public static string AdvanceUiOnce()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (Game1.CurrentEvent != null)
            {
                string eventId = Game1.CurrentEvent.id;
                Game1.CurrentEvent.receiveMouseClick(0, 0);
                bool stillRunning = Game1.CurrentEvent != null;
                return stillRunning
                    ? $"event advanced: {eventId} (still running)"
                    : $"event finished: {eventId}";
            }

            if (Game1.dialogueUp && Game1.activeClickableMenu is DialogueBox)
            {
                Game1.activeClickableMenu.receiveLeftClick(0, 0);
                return Game1.dialogueUp
                    ? "dialogue advanced (more lines)"
                    : "dialogue closed";
            }

            if (Game1.activeClickableMenu != null)
            {
                string menuName = Game1.activeClickableMenu.GetType().Name;
                Game1.activeClickableMenu.receiveLeftClick(0, 0);
                return $"menu click: {menuName}";
            }

            return "nothing to advance";
        }

        public static string AdvanceUiMany(int steps)
        {
            int count = Math.Max(1, Math.Min(steps, 200));
            var lines = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string result = AdvanceUiOnce();
                lines.Add($"[{i + 1}] {result}");
                if (result.StartsWith("nothing to advance", StringComparison.Ordinal)
                    || result.StartsWith("Error:", StringComparison.Ordinal))
                    break;
            }

            return string.Join("\n", lines);
        }

        /// <summary>Force-end the active farm event (use when advance is stuck).</summary>
        public static string EndActiveEvent()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (Game1.CurrentEvent == null)
                return "no active event";

            string eventId = Game1.CurrentEvent.id;
            int steps = 0;
            while (Game1.CurrentEvent != null && steps < 200)
            {
                Game1.CurrentEvent.receiveMouseClick(0, 0);
                steps++;
            }

            if (Game1.CurrentEvent != null)
            {
                Game1.eventUp = false;
                Game1.player.CanMove = true;
                Game1.player.completelyStopAnimatingOrDoingAction();
                Game1.player.showNotCarrying();
                return $"event {eventId}: force-unblocked after {steps} steps (event object may linger)";
            }

            Game1.eventUp = false;
            Game1.player.CanMove = true;
            return $"event ended: {eventId} ({steps} steps)";
        }

        /// <summary>Close dialogue box or dismiss the top clickable menu.</summary>
        public static string CloseActiveMenu()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (Game1.dialogueUp && Game1.activeClickableMenu is DialogueBox)
            {
                Game1.activeClickableMenu.exitThisMenu();
                return "dialogue closed";
            }

            if (Game1.activeClickableMenu != null)
            {
                string name = Game1.activeClickableMenu.GetType().Name;
                Game1.activeClickableMenu.exitThisMenu();
                return $"menu closed: {name}";
            }

            return "no menu to close";
        }
    }
}
