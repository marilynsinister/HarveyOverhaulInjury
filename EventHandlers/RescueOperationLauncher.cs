using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// C#-launcher для CP eventRescueOperation (Woods, гроза).
    /// Topic ставится после E5_StormBeside или storm comfort cutscene.
    /// </summary>
    public static class RescueOperationLauncher
    {
        public static bool IsRescueOperationEventId(string? eventId)
        {
            return string.Equals(eventId, EventIds.RescueOperation, StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanOfferTopic(DialogueManager dialogueManager)
        {
            if (Game1.player?.eventsSeen?.Contains(EventIds.RescueOperation) == true)
                return false;

            if (dialogueManager.HasTopic(RescueOperationIds.Topic))
                return false;

            if (dialogueManager.HasTopic(RescueOperationIds.CooldownTopic))
                return false;

            if (!dialogueManager.IsDatingOrMarriedToHarvey()
                && dialogueManager.GetHarveyFriendship() < RescueOperationIds.MinFriendshipPoints)
            {
                return false;
            }

            return true;
        }

        public static void TryOfferRescueOperationTopic(DialogueManager dialogueManager, IMonitor monitor, string triggerSource)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (!CanOfferTopic(dialogueManager))
                return;

            dialogueManager.AddTopic(RescueOperationIds.Topic, RescueOperationIds.TopicDurationDays);
            monitor.Log($"[RescueOperation] topicRescueOperation added (trigger: {triggerSource})", LogLevel.Info);
        }

        public static void MarkRescueOperationPlayed(
            DialogueManager dialogueManager,
            StateManager stateManager,
            IMonitor monitor)
        {
            dialogueManager.RemoveTopic(RescueOperationIds.Topic);
            dialogueManager.AddTopic(RescueOperationIds.CooldownTopic, RescueOperationIds.CooldownDays);
            stateManager.Save();
            monitor.Log("[RescueOperation] Event completed; cooldown applied.", LogLevel.Info);
        }
    }
}
