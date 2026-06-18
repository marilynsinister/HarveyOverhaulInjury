using HarveyOverhaul.Core.Api;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.Core.Services;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Services;

/// <summary>
/// Регистрирует медицинские интенты Injury в Core и синхронизирует CP conversation topics.
/// </summary>
public sealed class InjuryMedicalIntentProvider
{
    private const int SyncIntervalTicks = 60;

    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    private readonly StateManager _stateManager;
    private readonly InjuryManager _injuryManager;
    private readonly DialogueManager _dialogueManager;
    private readonly ComplicationManager _complicationManager;
    private readonly HospitalizationManager _hospitalizationManager;
    private RecoveryPlanManager? _recoveryPlanManager;
    private IHarveyCoreApi? _coreApi;
    private int _ticksSinceSync;
    private string? _lastTopicSyncSignature;
    private List<HarveyMedicalIntentRegistration> _lastCollectedIntents = new();

    public void RegisterWithCore(IHarveyCoreApi coreApi)
    {
        _coreApi = coreApi;
        coreApi.RegisterMedicalIntentPublisher(
            HarveyProviderRegistry.InjuryProviderId,
            PublishMedicalIntents);
        coreApi.RegisterMedicalTopicApplier(
            HarveyProviderRegistry.InjuryProviderId,
            ApplyTopicsFromResolution);
    }

    public void PublishMedicalIntents()
    {
        if (_coreApi == null)
            return;

        if (_hospitalizationManager.IsHospitalized)
        {
            _coreApi.ClearMedicalIntents(HarveyProviderRegistry.InjuryProviderId);
            _lastCollectedIntents.Clear();
            return;
        }

        _injuryManager.EnsureActiveTreatmentBuffs();
        _lastCollectedIntents = CollectIntents();
        _coreApi.RegisterMedicalIntents(HarveyProviderRegistry.InjuryProviderId, _lastCollectedIntents);
        _coreApi.SetHarveyClickInjurySummary(BuildInjuryDebugSummary());
    }

    public void ApplyTopicsFromResolution(HarveyMedicalIntentResolution resolution)
    {
        ApplyTopicSync(resolution);
    }

    public InjuryMedicalIntentProvider(
        IMonitor monitor,
        ModConfig config,
        StateManager stateManager,
        InjuryManager injuryManager,
        DialogueManager dialogueManager,
        ComplicationManager complicationManager,
        HospitalizationManager hospitalizationManager)
    {
        _monitor = monitor;
        _config = config;
        _stateManager = stateManager;
        _injuryManager = injuryManager;
        _dialogueManager = dialogueManager;
        _complicationManager = complicationManager;
        _hospitalizationManager = hospitalizationManager;
    }

    public void SetRecoveryPlanManager(RecoveryPlanManager recoveryPlanManager)
        => _recoveryPlanManager = recoveryPlanManager;

    public void SetCoreApi(IHarveyCoreApi? coreApi)
    {
        if (coreApi == null)
            return;

        RegisterWithCore(coreApi);
    }

    public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (++_ticksSinceSync < SyncIntervalTicks)
            return;

        _ticksSinceSync = 0;
        PublishMedicalIntents();
        if (_coreApi != null)
        {
            var resolution = _coreApi.ResolveHarveyMedicalIntent(logDetails: false);
            ApplyTopicsFromResolution(resolution);
        }
    }

    public HarveyMedicalIntentResolution? SyncOnHarveyClick(bool logDetails = true)
    {
        if (!Context.IsWorldReady || _coreApi == null)
            return null;

        var resolution = _coreApi.PrepareHarveyMedicalClick(logDetails);
        _coreApi.SetHarveyClickGate(false, "InjuryHandler: vanilla/CP dialogue allowed");
        _coreApi.LogHarveyClickDiagnostics("injury-click");
        return resolution;
    }

    private string BuildInjuryDebugSummary()
    {
        var parts = _stateManager.GetAllActiveDebuffStates()
            .Where(d => InjurySets.HarveyTreatable.Contains(d.BuffId))
            .Select(d =>
                $"{d.BuffId}(started={d.TreatmentStarted},phase={d.CurrentPhase},readyPhase={d.ReadyForNextPhase},readyRecovery={d.ReadyForRecovery})")
            .ToList();

        return parts.Count == 0 ? "(none)" : string.Join("; ", parts);
    }

    private List<HarveyMedicalIntentRegistration> CollectIntents()
    {
        var intents = new List<HarveyMedicalIntentRegistration>();
        int today = (int)Game1.stats.DaysPlayed;

        foreach (var debuff in _stateManager.GetAllActiveDebuffStates()
                     .Where(d => InjurySets.HarveyTreatable.Contains(d.BuffId))
                     .OrderByDescending(d => GetInjuryDangerRank(d.BuffId)))
        {
            if (debuff.ReadyForRecovery
                && (debuff.IsLastPhase || TreatmentManager.IsSimpleTreatmentInjury(debuff.BuffId)))
            {
                intents.Add(BuildRecoveryIntent(debuff, today));
                continue;
            }

            if (debuff.IsPhasedInjury
                && debuff.TreatmentStarted
                && debuff.CurrentPhase > 0
                && debuff.CurrentPhase < debuff.TotalPhases
                && debuff.ReadyForNextPhase)
            {
                intents.Add(BuildPhaseIntent(debuff, today));
                continue;
            }

            if (!debuff.TreatmentStarted)
                intents.Add(BuildStartIntent(debuff, today));
        }

        foreach (string complicationId in _complicationManager.GetActiveTreatableComplicationIds())
        {
            intents.Add(BuildComplicationIntent(complicationId, today));
        }

        return intents;
    }

    private HarveyMedicalIntentRegistration BuildStartIntent(DebuffState debuff, int today)
    {
        string buffId = debuff.BuffId;
        bool critical = IsCriticalInjury(buffId);
        bool severe = InjuryManager.IsSeriousMainInjuryId(buffId);

        return new HarveyMedicalIntentRegistration
        {
            ProviderId = HarveyProviderRegistry.InjuryProviderId,
            Kind = HarveyMedicalIntentKind.Injury,
            StateId = buffId,
            BasePriority = critical
                ? HarveyMedicalIntentPriorities.EmergencyInjury
                : HarveyMedicalIntentPriorities.UntreatedInjury,
            DangerRank = GetInjuryDangerRank(buffId),
            StateAgeTicks = Math.Max(0, today - debuff.InjuryStartDay),
            ActionKey = TreatmentStartActions.StartTreatment,
            TopicKey = TopicIds.GetStartTreatmentTopic(buffId),
            AlternativeTopicKeys = new[] { TopicIds.GetTreatmentNeededTopic(buffId) },
            AllowDuringFestival = critical,
            AllowOutsideClinic = _config.AllowBasicTreatmentOutsideClinic,
            RequiresClinic = severe && _config.RequireClinicForSevereInjuries && !critical && !IsRomanticHarveyRelationship(),
            IsEmergency = critical,
            FallbackLine = "Я вижу травму — сначала займусь этим.$a",
            FestivalDeferTopicKey = TopicIds.GetFestivalDeferTopic(buffId),
            FestivalDeferLine =
                "Я вижу рану. Сейчас не место для полноценного осмотра, но после фестиваля ты сразу идёшь ко мне. Я серьёзно.$a",
        };
    }

    private HarveyMedicalIntentRegistration BuildPhaseIntent(DebuffState debuff, int today)
    {
        string buffId = debuff.BuffId;
        int nextPhase = debuff.CurrentPhase + 1;

        return new HarveyMedicalIntentRegistration
        {
            ProviderId = HarveyProviderRegistry.InjuryProviderId,
            Kind = HarveyMedicalIntentKind.PhaseTransition,
            StateId = buffId,
            BasePriority = HarveyMedicalIntentPriorities.ReadyForNextPhase,
            DangerRank = GetInjuryDangerRank(buffId),
            StateAgeTicks = Math.Max(0, today - debuff.InjuryStartDay),
            IsPhaseReady = true,
            ActionKey = TreatmentStartActions.AdvancePhase,
            TopicKey = TopicIds.GetAdvancePhaseTopic(buffId, nextPhase),
            AllowDuringFestival = false,
            AllowOutsideClinic = _config.AllowPhaseTransitionOutsideClinic,
            RequiresClinic = InjuryManager.IsSeriousMainInjuryId(buffId)
                && _config.RequireClinicForSevereInjuries
                && !IsRomanticHarveyRelationship(),
            IsEmergency = false,
            FallbackLine = "Пора перевести тебя на следующий этап лечения.$u",
            FestivalDeferTopicKey = TopicIds.GetFestivalDeferTopic(buffId),
            FestivalDeferLine =
                "Я вижу рану. Сейчас не место для полноценного осмотра, но после фестиваля ты сразу идёшь ко мне. Я серьёзно.$a",
        };
    }

    private HarveyMedicalIntentRegistration BuildRecoveryIntent(DebuffState debuff, int today)
    {
        string buffId = debuff.BuffId;

        return new HarveyMedicalIntentRegistration
        {
            ProviderId = HarveyProviderRegistry.InjuryProviderId,
            Kind = HarveyMedicalIntentKind.Recovery,
            StateId = buffId,
            BasePriority = HarveyMedicalIntentPriorities.ReadyForRecovery,
            DangerRank = GetInjuryDangerRank(buffId),
            StateAgeTicks = Math.Max(0, today - debuff.InjuryStartDay),
            ActionKey = TreatmentStartActions.CompleteRecovery,
            TopicKey = TopicIds.GetCompleteRecoveryTopic(buffId),
            AllowDuringFestival = false,
            AllowOutsideClinic = _config.AllowRecoveryOutsideClinic,
            RequiresClinic = false,
            IsEmergency = false,
            FallbackLine = "Ты готова к выписке — давай завершим лечение.$h",
            FestivalDeferTopicKey = TopicIds.GetFestivalDeferTopic(buffId),
            FestivalDeferLine =
                "Я вижу рану. Сейчас не место для полноценного осмотра, но после фестиваля ты сразу идёшь ко мне. Я серьёзно.$a",
        };
    }

    private HarveyMedicalIntentRegistration BuildComplicationIntent(string complicationId, int today)
    {
        return new HarveyMedicalIntentRegistration
        {
            ProviderId = HarveyProviderRegistry.InjuryProviderId,
            Kind = HarveyMedicalIntentKind.Complication,
            StateId = complicationId,
            BasePriority = HarveyMedicalIntentPriorities.Complication,
            DangerRank = GetComplicationDangerRank(complicationId),
            StateAgeTicks = today,
            ActionKey = TreatmentStartActions.TreatComplication,
            TopicKey = TopicIds.GetTreatComplicationTopic(complicationId),
            AlternativeTopicKeys = new[] { TopicIds.GetTreatmentNeededComplicationTopic(complicationId) },
            AllowDuringFestival = true,
            AllowOutsideClinic = _config.AllowBasicTreatmentOutsideClinic,
            RequiresClinic = false,
            IsEmergency = true,
            FallbackLine = "Осложнение нельзя откладывать — сейчас обработаю.$a",
        };
    }

    private void ApplyTopicSync(HarveyMedicalIntentResolution resolution)
    {
        var selected = resolution.Selected;
        bool injurySelected = selected != null
            && string.Equals(selected.ProviderId, HarveyProviderRegistry.InjuryProviderId, StringComparison.Ordinal);

        if (!injurySelected)
        {
            int removed = RemoveStaleMedicalActionTopics(Array.Empty<string>());
            LogTopicSyncIfChanged(
                $"none:{removed}",
                removed > 0,
                () => _monitor.Log(
                    $"[MedicalIntent] topic sync: no injury intent selected — removed {removed} stale action topic(s)",
                    LogLevel.Debug));

            return;
        }

        string topicToAdd = selected!.TopicKey;

        if (resolution.FestivalBlockedLongTreatment
            && !selected.AllowDuringFestival
            && !string.IsNullOrWhiteSpace(selected.FestivalDeferTopicKey))
        {
            topicToAdd = selected.FestivalDeferTopicKey!;
        }

        var keepTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { topicToAdd };

        string? legacyAlias = null;
        if (TopicIds.IsMedicalActionTopic(topicToAdd))
        {
            legacyAlias = TopicIds.GetLegacyActionTopicAlias(selected.ActionKey, selected.StateId);
            if (!string.IsNullOrWhiteSpace(legacyAlias)
                && !string.Equals(legacyAlias, topicToAdd, StringComparison.OrdinalIgnoreCase))
            {
                keepTopics.Add(legacyAlias);
            }
        }

        int staleRemoved = RemoveStaleMedicalActionTopics(keepTopics);

        _dialogueManager.TryAddMedicalIntentTopic(topicToAdd, 7);

        if (!string.IsNullOrWhiteSpace(legacyAlias)
            && !string.Equals(legacyAlias, topicToAdd, StringComparison.OrdinalIgnoreCase)
            && string.Equals(topicToAdd, selected.TopicKey, StringComparison.OrdinalIgnoreCase))
        {
            _dialogueManager.TryAddMedicalIntentTopic(legacyAlias!, 7);
        }

        if (selected.Kind == HarveyMedicalIntentKind.Complication)
            _dialogueManager.MoveTopicToEnd(TopicIds.GetComplicationTopic(selected.StateId));

        string syncSignature =
            $"{selected.Kind}|{selected.StateId}|{selected.ActionKey}|{topicToAdd}|{legacyAlias}|{staleRemoved}";
        LogTopicSyncIfChanged(
            syncSignature,
            logWhenUnchanged: false,
            () =>
            {
                _monitor.Log(
                    $"[MedicalIntent] topic sync SELECTED kind={selected.Kind} state={selected.StateId} " +
                    $"action={selected.ActionKey} topic={topicToAdd}",
                    LogLevel.Debug);

                if (!string.IsNullOrWhiteSpace(legacyAlias)
                    && !string.Equals(legacyAlias, topicToAdd, StringComparison.OrdinalIgnoreCase))
                {
                    _monitor.Log(
                        $"[MedicalIntent] topic sync legacy alias kept: {legacyAlias}",
                        LogLevel.Debug);
                }

                if (staleRemoved > 0)
                {
                    _monitor.Log(
                        $"[MedicalIntent] topic sync removed {staleRemoved} stale non-selected action topic(s)",
                        LogLevel.Debug);
                }
            });

        _recoveryPlanManager?.SyncHarveyTalkTopic();
    }

    private void LogTopicSyncIfChanged(string signature, bool logWhenUnchanged, Action logAction)
    {
        if (!logWhenUnchanged && string.Equals(_lastTopicSyncSignature, signature, StringComparison.Ordinal))
            return;

        _lastTopicSyncSignature = signature;
        logAction();
    }

    private int RemoveStaleMedicalActionTopics(IReadOnlyCollection<string> keepTopics)
    {
        int removed = 0;

        foreach (string topic in TopicIds.GetAllKnownMedicalActionTopics())
        {
            if (keepTopics.Contains(topic))
                continue;

            if (!_dialogueManager.HasTopic(topic))
                continue;

            _dialogueManager.RemoveTopicIfOwned(topic, "intent sync: non-selected action topic");
            removed++;
        }

        return removed;
    }

    private static int GetComplicationDangerRank(string complicationId) =>
        complicationId switch
        {
            "HarveyMod_DirtyWound" => 95,
            "HarveyMod_WetBandage" => 90,
            "HarveyMod_WetStitches" => 88,
            "HarveyMod_Neglect" => 85,
            _ => 50,
        };

    private static bool IsRomanticHarveyRelationship()
    {
        if (!Context.IsWorldReady)
            return false;

        if (string.Equals(Game1.player.spouse, "Harvey", StringComparison.OrdinalIgnoreCase))
            return true;

        var friendship = Game1.player.getFriendshipHeartLevelForNPC("Harvey");
        return friendship >= 8 && Game1.player.friendshipData.TryGetValue("Harvey", out var data)
            && data.IsDating();
    }

    private static bool IsCriticalInjury(string buffId) =>
        buffId is "buffConcussion" or "buffInfectedWound" or "buffFracturedBone" or "buffBadlyHurt";

    public static int GetInjuryDangerRank(string buffId) =>
        buffId switch
        {
            "buffConcussion" => 130,
            "buffInfectedWound" => 120,
            "buffFracturedBone" => 110,
            "buffSurgicalWound" => 100,
            "buffShrapnelWounds" => 90,
            "buffBurnWounds" => 80,
            "buffDeepCuts" => 70,
            "buffTornMuscles" => 60,
            "buffBackStrain" => 50,
            "buffBruisedRibs" => 40,
            "buffSprainedAnkle" => 30,
            "buffBadlyHurt" => 20,
            "buffHurt" => 10,
            InjuryBuffs.Cold => 5,
            _ => 0,
        };
}
