using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers;

/// <summary>Явное знание Харви о травме — без телепатии по баффу в шахте.</summary>
public static class HarveyInjuryAwarenessHelper
{
    public const string FreshInjuryMineNeutralHud =
        "Рана саднит. В шахте легко занести грязь.";

    private static readonly string[] GlobalMineAwarenessTopics =
    {
        ConversationTopics.MineInjuryRescue,
        ConversationTopics.KnownSevereInjury,
        ConversationTopics.RecoveryPlanStarted,
        "HarveyMod_RecoveryPlanActive",
    };

    public static bool HasGlobalHarveyMineAwareness(InjuryState state, BuffManager buffManager)
    {
        if (buffManager.HasBuff(InjuryBuffs.MineForbidden))
            return true;

        if (state.MineForbiddenAppliedDay >= 0)
            return true;

        foreach (string topic in GlobalMineAwarenessTopics)
        {
            if (GameUtils.HasConversationTopic(topic))
                return true;
        }

        return false;
    }

    public static bool IsInjuryHarveyAware(DebuffState? debuffState) =>
        debuffState != null
        && (debuffState.HarveyAware
            || debuffState.HarveyConversationHappened
            || debuffState.TreatmentStarted);

    public static bool IsHarveyAwareOfInjury(
        string injuryId,
        InjuryState state,
        StateManager stateManager,
        InjuryManager injuryManager,
        BuffManager buffManager)
    {
        if (HasGlobalHarveyMineAwareness(state, buffManager))
            return true;

        return IsInjuryHarveyAware(stateManager.GetDebuffState(injuryId));
    }

    public static bool HasKnownSevereInjury(
        InjuryState state,
        StateManager stateManager,
        InjuryManager injuryManager,
        BuffManager buffManager)
    {
        if (HasGlobalHarveyMineAwareness(state, buffManager))
            return true;

        foreach (string injuryId in InjurySets.Severe)
        {
            DebuffState? debuffState = stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
                continue;

            if (!injuryManager.HasInjuryOrPhase(injuryId) && !buffManager.HasBuff(injuryId))
                continue;

            if (IsInjuryHarveyAware(debuffState))
                return true;
        }

        return false;
    }

    public static bool IsHarveyAwareForMineReaction(
        DebuffState? debuffState,
        InjuryState state,
        BuffManager buffManager)
    {
        if (HasGlobalHarveyMineAwareness(state, buffManager))
            return true;

        return IsInjuryHarveyAware(debuffState);
    }

    public static bool HasActiveUnawareMineInjury(
        InjuryState state,
        InjuryManager injuryManager,
        BuffManager buffManager)
    {
        foreach (string injuryId in CollectMineRelevantInjuryIds(state, injuryManager))
        {
            if (!IsMineRelevantBuff(injuryId, injuryManager, buffManager))
                continue;

            DebuffState? debuffState = state.ActiveDebuffs.GetValueOrDefault(injuryId);
            if (!IsHarveyAwareForMineReaction(debuffState, state, buffManager))
                return true;
        }

        return false;
    }

    public static void MarkHarveyAware(
        StateManager stateManager,
        DialogueManager? dialogueManager,
        string buffId,
        string reason,
        IMonitor? monitor = null)
    {
        if (string.IsNullOrWhiteSpace(buffId))
            return;

        int today = GameUtils.Today();

        if (stateManager.GetDebuffState(buffId) is { } debuffState)
        {
            if (!debuffState.HarveyAware)
            {
                debuffState.HarveyAware = true;
                debuffState.HiddenFromHarvey = false;
                debuffState.AwarenessReason = reason;
                if (string.IsNullOrEmpty(debuffState.DiscoveryReason))
                    debuffState.DiscoveryReason = reason;
                debuffState.HarveyAwareDay = today;
                stateManager.UpdateDebuffState(buffId, debuffState);
                monitor?.Log(
                    $"[HarveyAware] {buffId} reason={reason} day={today}",
                    LogLevel.Debug);
            }
        }

        TryAddKnownSevereInjuryTopic(dialogueManager, buffId);
    }

    public static void MarkAllActiveInjuriesHarveyAware(
        StateManager stateManager,
        DialogueManager? dialogueManager,
        InjuryManager injuryManager,
        BuffManager buffManager,
        string reason,
        IMonitor? monitor = null)
    {
        foreach (string injuryId in CollectMineRelevantInjuryIds(stateManager.State, injuryManager))
        {
            if (!IsMineRelevantBuff(injuryId, injuryManager, buffManager))
                continue;

            MarkHarveyAware(stateManager, dialogueManager, injuryId, reason, monitor);
        }

        if (dialogueManager != null
            && !GameUtils.HasConversationTopic(ConversationTopics.KnownSevereInjury))
        {
            dialogueManager.AddTopic(ConversationTopics.KnownSevereInjury, 7);
        }
    }

    public static void TryAddKnownSevereInjuryTopic(DialogueManager? dialogueManager, string? buffId)
    {
        if (dialogueManager == null)
            return;

        if (buffId != null && !IsMineAwarenessRelevantInjury(buffId))
            return;

        if (!GameUtils.HasConversationTopic(ConversationTopics.KnownSevereInjury))
            dialogueManager.AddTopic(ConversationTopics.KnownSevereInjury, 7);
    }

    public static bool IsMineAwarenessRelevantInjury(string injuryId) =>
        InjurySets.Severe.Contains(injuryId)
        || InjurySets.Critical.Contains(injuryId)
        || string.Equals(injuryId, "buffDeepCuts", StringComparison.OrdinalIgnoreCase)
        || InjurySets.LimitedActivity.Contains(injuryId);

    private static IEnumerable<string> CollectMineRelevantInjuryIds(InjuryState state, InjuryManager injuryManager)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? mainId = injuryManager.GetActiveInjury();
        if (!string.IsNullOrEmpty(mainId))
            seen.Add(mainId);

        foreach (string injuryId in state.ActiveDebuffs.Keys)
            seen.Add(injuryId);

        return seen;
    }

    private static bool IsMineRelevantBuff(
        string injuryId,
        InjuryManager injuryManager,
        BuffManager buffManager) =>
        injuryManager.HasInjuryOrPhase(injuryId) || buffManager.HasBuff(injuryId);
}
