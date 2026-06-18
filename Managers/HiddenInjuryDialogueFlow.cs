using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// C# suspicion-flow для скрытых травм: вопрос через createQuestionDialogue, тексты из CP.
    /// Лечение стартует только после явного выбора игрока.
    /// </summary>
    public sealed class HiddenInjuryDialogueFlow
    {
        private enum FlowKind
        {
            Suspicion,
            Obvious,
            ForcedReveal,
        }

        private sealed class PendingFlow
        {
            public string BuffId { get; init; } = "";
            public NPC Harvey { get; init; } = null!;
            public DetectionContext Context { get; init; } = null!;
            public FlowKind Kind { get; init; }
        }

        private const int PostponeCooldownGameMinutes = 180;
        private const int MaxChoicesPerQuestion = 5;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly TreatmentStartHandler _treatmentStartHandler;
        private readonly InjuryManager _injuryManager;
        private readonly BuffManager _buffManager;
        private readonly ComplicationManager _complicationManager;

        private PendingFlow? _pendingFlow;
        private string? _domesticReason;
        private Action? _afterHiddenDialogueClosed;
        private bool _hiddenDialogueWasShown;
        private bool _hiddenDialogueOpenObserved;

        public bool IsQuestionPending => _pendingFlow != null;

        public HiddenInjuryDialogueFlow(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            TreatmentStartHandler treatmentStartHandler,
            InjuryManager injuryManager,
            BuffManager buffManager,
            ComplicationManager complicationManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _treatmentStartHandler = treatmentStartHandler;
            _injuryManager = injuryManager;
            _buffManager = buffManager;
            _complicationManager = complicationManager;
        }

        /// <summary>Точка входа для клика / proximity / domestic-check.</summary>
        public bool TryStartDetection(
            NPC harvey,
            bool isDirectTalk,
            bool isProximityCheck,
            string triggerReason)
        {
            if (harvey == null || Game1.player == null || Game1.currentLocation == null)
                return false;

            if (IsQuestionPending)
                return false;

            if (ShouldDeferForActiveTreatableComplication())
                return false;

            var injuryState = _stateManager.State;
            var context = InjuryVisibilityHelper.BuildDetectionContext(
                _config,
                injuryState,
                isDirectTalk,
                isProximityCheck);

            if (!context.HarveyIsPresent)
                return false;

            DebuffState? target = PickMostImportantHiddenInjury(context);
            if (target == null)
                return false;

            var profile = InjuryVisibilityHelper.GetVisibilityProfile(target.BuffId);
            if (!InjuryVisibilityHelper.ShouldHarveyDetectHiddenInjury(target, profile, context))
                return false;

            FlowKind kind = DetermineFlowKind(target, profile, context);

            if (context.IsFestivalContext && !IsEmergency(target.BuffId, target, context))
                return TryStartFestivalNotice(harvey, target.BuffId, context);

            if (kind == FlowKind.ForcedReveal)
                return TryStartForcedRevealFlow(harvey, target, context);

            if (kind == FlowKind.Obvious)
                return TryStartObviousFlow(harvey, target, context);

            return TryStartSuspicionFlow(harvey, target, context);
        }

        /// <summary>Domestic morning/evening check (pre-validated proximity and injury).</summary>
        public bool TryStartDomesticHiddenInjuryFlow(NPC harvey, string reason, DebuffState target)
        {
            if (harvey == null || target == null || Game1.player == null || Game1.currentLocation == null)
                return false;

            if (IsQuestionPending)
                return false;

            if (ShouldDeferForActiveTreatableComplication())
                return false;

            _domesticReason = reason;

            try
            {
                var injuryState = _stateManager.State;
                var context = InjuryVisibilityHelper.BuildDetectionContext(
                    _config,
                    injuryState,
                    isDirectTalk: true,
                    isProximityCheck: false);
                context.HarveyIsPresent = true;
                context.IsFarmHouseMorningOrEvening = true;

                var profile = InjuryVisibilityHelper.GetVisibilityProfile(target.BuffId);
                FlowKind kind = DetermineFlowKind(target, profile, context);

                if (context.IsFestivalContext && !IsEmergency(target.BuffId, target, context))
                    return TryStartFestivalNotice(harvey, target.BuffId, context);

                if (kind == FlowKind.ForcedReveal)
                    return TryStartForcedRevealFlow(harvey, target, context);

                if (kind == FlowKind.Obvious)
                    return TryStartObviousFlow(harvey, target, context);

                return TryStartSuspicionFlow(harvey, target, context);
            }
            finally
            {
                if (!IsQuestionPending)
                    _domesticReason = null;
            }
        }

        /// <summary>Точка входа для domestic morning/evening (legacy alias).</summary>
        public bool TryStartHiddenInjuryFlowFromDomesticCheck(NPC harvey, string reason)
        {
            DebuffState? target = _stateManager.GetAllActiveDebuffStates()
                .Where(s => s.HiddenFromHarvey && !s.HarveyAware)
                .Where(s => _buffManager.HasBuff(s.BuffId) || _injuryManager.HasInjuryOrPhase(s.BuffId))
                .OrderByDescending(s => (int)InjuryVisibilityHelper.GetVisibilityLevel(s))
                .FirstOrDefault();

            if (target == null)
                return false;

            return TryStartDomesticHiddenInjuryFlow(harvey, reason, target);
        }

        /// <summary>Выполняет отложенное раскрытие/лечение после закрытия DialogueBox игроком.</summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (_afterHiddenDialogueClosed == null || !_hiddenDialogueWasShown)
                return;

            if (!Context.IsWorldReady)
                return;

            if (Game1.activeClickableMenu is DialogueBox || Game1.dialogueUp)
            {
                _hiddenDialogueOpenObserved = true;
                return;
            }

            if (!_hiddenDialogueOpenObserved)
                return;

            Action action = _afterHiddenDialogueClosed;
            ClearAfterDialogueQueue();

            try
            {
                action();
            }
            catch (Exception ex)
            {
                _monitor.Log($"[HiddenInjuryFlow] after-dialogue action error: {ex}", LogLevel.Error);
            }
            finally
            {
                _monitor.Log("[HiddenInjuryFlow] after-dialogue action executed", LogLevel.Info);
            }
        }

        public bool TryStartSuspicionFlow(NPC harvey, DebuffState state, DetectionContext context)
        {
            if (!CanAskAboutHiddenInjury(state, context, FlowKind.Suspicion))
                return false;

            string questionKey = ResolveQuestionKeyForFlow(FlowKind.Suspicion, state.BuffId, context);
            return BeginQuestionFlow(harvey, state.BuffId, context, FlowKind.Suspicion, questionKey);
        }

        public bool TryStartObviousFlow(NPC harvey, DebuffState state, DetectionContext context)
        {
            if (!CanAskAboutHiddenInjury(state, context, FlowKind.Obvious))
                return false;

            string questionKey = ResolveQuestionKeyForFlow(FlowKind.Obvious, state.BuffId, context);
            return BeginQuestionFlow(harvey, state.BuffId, context, FlowKind.Obvious, questionKey);
        }

        public bool TryStartForcedRevealFlow(NPC harvey, DebuffState state, DetectionContext context)
        {
            if (!CanAskAboutHiddenInjury(state, context, FlowKind.ForcedReveal))
                return false;

            string questionKey = ResolveQuestionKeyForFlow(FlowKind.ForcedReveal, state.BuffId, context);
            return BeginQuestionFlow(harvey, state.BuffId, context, FlowKind.ForcedReveal, questionKey);
        }

        private bool TryStartFestivalNotice(NPC harvey, string buffId, DetectionContext context)
        {
            if (!CanAskAboutHiddenInjury(
                    _stateManager.GetDebuffState(buffId) ?? new DebuffState { BuffId = buffId },
                    context,
                    FlowKind.Suspicion))
            {
                return false;
            }

            string key = GetFestivalNoticeKey(buffId, context);
            string line = _dialogueManager.GetDialogueTextByPrefix(key);
            _dialogueManager.Speak(harvey, line);

            _treatmentStartHandler.TryMarkFestivalHiddenInjuryNotice(buffId, out _);
            MarkQuestionAsked(buffId);
            ClearPending();

            _monitor.Log($"[HiddenInjuryFlow] Festival notice buff={buffId} key={key}", LogLevel.Info);
            return true;
        }

        private bool BeginQuestionFlow(
            NPC harvey,
            string buffId,
            DetectionContext context,
            FlowKind kind,
            string questionKey)
        {
            string question = StripDialogueCommands(_dialogueManager.GetDialogueTextByPrefix(questionKey));
            if (string.IsNullOrWhiteSpace(question))
            {
                _monitor.Log($"[HiddenInjuryFlow] Пустой вопрос key={questionKey}", LogLevel.Warn);
                return false;
            }

            Response[] choices = BuildChoices(kind, context);
            if (choices.Length == 0)
                return false;

            _pendingFlow = new PendingFlow
            {
                BuffId = buffId,
                Harvey = harvey,
                Context = context,
                Kind = kind,
            };

            var injuryState = _stateManager.State;
            injuryState.PendingHiddenInjuryBuffId = buffId;
            MarkQuestionAsked(buffId);
            _stateManager.Save();

            harvey.facePlayer(Game1.player);
            Game1.currentLocation.createQuestionDialogue(
                question,
                choices,
                (farmer, answer) => HandleHiddenInjuryAnswer(harvey, buffId, answer));

            _monitor.Log(
                $"[HiddenInjuryFlow] Question kind={kind} domestic={_domesticReason ?? "none"} "
                + $"buff={buffId} key={questionKey} choices={choices.Length}",
                LogLevel.Info);
            return true;
        }

        private void HandleHiddenInjuryAnswer(NPC harvey, string buffId, string answer)
        {
            try
            {
                var pending = _pendingFlow;
                FlowKind kind = pending?.Kind ?? FlowKind.Suspicion;
                DetectionContext context = pending?.Context
                    ?? InjuryVisibilityHelper.BuildDetectionContext(_config, _stateManager.State, true, false);

                switch (answer)
                {
                    case "show":
                    case "examine":
                    case "nod":
                        OnHiddenInjuryChoice_Show(harvey, buffId, context, "player_confessed");
                        break;
                    case "scared":
                        OnHiddenInjuryChoice_Scared(harvey, buffId, context);
                        break;
                    case "ashamed":
                        OnHiddenInjuryChoice_Show(harvey, buffId, context, "player_confessed_ashamed");
                        break;
                    case "deny":
                        if (kind == FlowKind.Obvious)
                            OnHiddenInjuryChoice_ObviousDeny(harvey, buffId, context);
                        else
                            OnHiddenInjuryChoice_Deny(harvey, buffId);
                        break;
                    case "joke":
                        if (kind == FlowKind.Obvious || kind == FlowKind.ForcedReveal)
                            OnHiddenInjuryChoice_ObviousJoke(harvey, buffId, context);
                        else
                            OnHiddenInjuryChoice_Joke(harvey, buffId);
                        break;
                    case "not_now":
                        OnHiddenInjuryChoice_NotNow(harvey, buffId);
                        break;
                    default:
                        _monitor.Log($"[HiddenInjuryFlow] Неизвестный ответ '{answer}' buff={buffId}", LogLevel.Warn);
                        break;
                }
            }
            finally
            {
                ClearPending();
            }
        }

        private void OnHiddenInjuryChoice_Show(
            NPC harvey,
            string buffId,
            DetectionContext context,
            string revealReason)
        {
            string responseText = _dialogueManager.GetDialogueTextByPrefix(ResolveResponseKey("Show", context));
            string confessedText = _dialogueManager.GetDialogueTextByPrefix(GetConfessedKey(buffId, context));
            SpeakAndQueueRevealAfterClose(
                harvey,
                CombineDialogueParts(responseText, confessedText),
                buffId,
                revealReason,
                context);
        }

        private void OnHiddenInjuryChoice_Scared(NPC harvey, string buffId, DetectionContext context)
        {
            string responseText = _dialogueManager.GetDialogueTextByPrefix(ResolveResponseKey("Scared", context));
            SpeakAndQueueRevealAfterClose(
                harvey,
                responseText,
                buffId,
                "player_confessed_scared",
                context);
        }

        private void OnHiddenInjuryChoice_Deny(NPC harvey, string buffId)
        {
            if (_stateManager.GetDebuffState(buffId) is not { } state)
                return;

            state.PlayerDeniedInjuryToday = true;
            state.SuspicionLevel++;
            _stateManager.UpdateDebuffState(buffId, state);
            _stateManager.Save();

            string prefix = ResolveResponsePrefix("Deny", BuildDetectionContextForRelationship());
            _dialogueManager.Speak(harvey, _dialogueManager.GetDialogueTextByPrefix(prefix));

            _monitor.Log($"[HiddenInjuryFlow] Denied buff={buffId} suspicion={state.SuspicionLevel}", LogLevel.Info);
        }

        private void OnHiddenInjuryChoice_Joke(NPC harvey, string buffId)
        {
            if (_stateManager.GetDebuffState(buffId) is not { } state)
                return;

            state.PlayerDeniedInjuryToday = true;
            state.SuspicionLevel++;
            _stateManager.UpdateDebuffState(buffId, state);
            _stateManager.Save();

            string prefix = ResolveResponsePrefix("Joke", BuildDetectionContextForRelationship());
            _dialogueManager.Speak(harvey, _dialogueManager.GetDialogueTextByPrefix(prefix));

            _monitor.Log($"[HiddenInjuryFlow] Joke-deny buff={buffId} suspicion={state.SuspicionLevel}", LogLevel.Info);
        }

        private void OnHiddenInjuryChoice_NotNow(NPC harvey, string buffId)
        {
            if (_stateManager.GetDebuffState(buffId) is not { } state)
                return;

            state.PlayerDeniedInjuryToday = true;
            state.SuspicionLevel += 2;
            _stateManager.UpdateDebuffState(buffId, state);

            var injuryState = _stateManager.State;
            injuryState.LastHiddenInjuryPostponeDay = GameUtils.Today();
            injuryState.LastHiddenInjuryPostponeTime = Game1.timeOfDay;
            _stateManager.Save();

            string prefix = ResolveResponsePrefix("NotNow", BuildDetectionContextForRelationship());
            _dialogueManager.Speak(harvey, _dialogueManager.GetDialogueTextByPrefix(prefix));

            _monitor.Log($"[HiddenInjuryFlow] Postponed buff={buffId} suspicion={state.SuspicionLevel}", LogLevel.Info);
        }

        private void OnHiddenInjuryChoice_ObviousDeny(NPC harvey, string buffId, DetectionContext context)
        {
            string strictText = _dialogueManager.GetDialogueTextByPrefix(GetObviousQuestionKey(buffId, context));
            SpeakAndQueueRevealAfterClose(
                harvey,
                strictText,
                buffId,
                "harvey_forced_obvious",
                context);
        }

        private void OnHiddenInjuryChoice_ObviousJoke(NPC harvey, string buffId, DetectionContext context)
        {
            string responseText = _dialogueManager.GetDialogueTextByPrefix(ResolveResponsePrefix("Joke", context));
            SpeakAndQueueRevealAfterClose(
                harvey,
                responseText,
                buffId,
                "harvey_forced_obvious_joke",
                context);
        }

        private void SpeakAndQueueRevealAfterClose(
            NPC harvey,
            string dialogueText,
            string buffId,
            string revealReason,
            DetectionContext context)
        {
            if (string.IsNullOrWhiteSpace(dialogueText))
            {
                _monitor.Log(
                    "[HiddenInjuryFlow] empty reveal dialogue — running reveal/treatment immediately",
                    LogLevel.Warn);
                RevealAndStartTreatment(buffId, revealReason, context);
                return;
            }

            _hiddenDialogueWasShown = true;
            _hiddenDialogueOpenObserved = false;
            _afterHiddenDialogueClosed = () =>
                RevealAndStartTreatment(buffId, revealReason, context);

            _dialogueManager.Speak(harvey, dialogueText);
            _monitor.Log("[HiddenInjuryFlow] queued reveal/treatment after dialogue close", LogLevel.Info);
        }

        private void ClearAfterDialogueQueue()
        {
            _afterHiddenDialogueClosed = null;
            _hiddenDialogueWasShown = false;
            _hiddenDialogueOpenObserved = false;
        }

        private static string CombineDialogueParts(string first, string second)
        {
            first = TrimDialogueActionSuffix(first);
            second = TrimDialogueActionSuffix(second);

            if (string.IsNullOrWhiteSpace(first))
                return second;
            if (string.IsNullOrWhiteSpace(second))
                return first;

            return first + "#$b#" + second;
        }

        private static string TrimDialogueActionSuffix(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            int actionIndex = text.IndexOf("$action", StringComparison.OrdinalIgnoreCase);
            if (actionIndex >= 0)
                text = text[..actionIndex];

            return text.Trim();
        }

        private void RevealAndStartTreatment(string buffId, string revealReason, DetectionContext context)
        {
            if (_stateManager.GetDebuffState(buffId) is { } debuffState)
            {
                InjuryVisibilityHelper.RevealHiddenInjury(
                    _stateManager,
                    _dialogueManager,
                    debuffState,
                    buffId,
                    revealReason,
                    _stateManager.State,
                    _monitor,
                    addDetectionTopic: false);
            }

            TryAddAfterRevealFollowUpTopic(buffId, context);
            _treatmentStartHandler.TryRevealAndStartTreatment(buffId, fromDialogueAction: false, out _);
        }

        private void TryAddAfterRevealFollowUpTopic(string buffId, DetectionContext context)
        {
            if (_stateManager.GetDebuffState(buffId) is not { } state)
                return;

            bool strict = state.HiddenDays >= 3
                || state.SuspicionLevel >= 3
                || _stateManager.State.ActiveComplications.Count > 0;

            string topicKey;
            if (context.IsMarriedToHarvey)
                topicKey = strict
                    ? "HarveyMod_HiddenInjury_AfterReveal_Married_Strict"
                    : "HarveyMod_HiddenInjury_AfterReveal_Married_Soft";
            else if (IsEngagedToHarvey())
                topicKey = "HarveyMod_HiddenInjury_AfterReveal_Engaged";
            else if (context.IsDatingOrEngagedToHarvey)
                topicKey = "HarveyMod_HiddenInjury_AfterReveal_Dating";
            else
                topicKey = strict
                    ? "HarveyMod_HiddenInjury_AfterReveal_Strict"
                    : "HarveyMod_HiddenInjury_AfterReveal_Soft";

            if (_dialogueManager.HasDialogueKey(topicKey))
                _dialogueManager.AddTopic(topicKey, 2);
        }

        private bool ShouldDeferForActiveTreatableComplication()
        {
            if (_complicationManager.GetActiveTreatableComplicationIds().Count <= 0)
                return false;

            _monitor.Log(
                "[HiddenInjuryFlow] skipped: active treatable complication has priority",
                LogLevel.Info);
            return true;
        }

        private DebuffState? PickMostImportantHiddenInjury(DetectionContext context)
        {
            return _stateManager.GetAllActiveDebuffStates()
                .Where(s => s.HiddenFromHarvey && !s.HarveyAware)
                .Where(s => _buffManager.HasBuff(s.BuffId) || _injuryManager.HasInjuryOrPhase(s.BuffId))
                .Where(s =>
                {
                    var profile = InjuryVisibilityHelper.GetVisibilityProfile(s.BuffId);
                    return InjuryVisibilityHelper.ShouldHarveyDetectHiddenInjury(s, profile, context);
                })
                .OrderByDescending(s => GetPickScore(s, context))
                .FirstOrDefault();
        }

        private int GetPickScore(DebuffState state, DetectionContext context)
        {
            var level = InjuryVisibilityHelper.GetVisibilityLevel(state);
            int score = (int)level * 100;
            score += state.HiddenDays * 5;
            score += state.SuspicionLevel * 3;

            if (context.HasComplication
                && string.Equals(_stateManager.State.MainInjuryId, state.BuffId, StringComparison.OrdinalIgnoreCase))
            {
                score += 50;
            }

            if (InjurySets.Severe.Contains(state.BuffId) || InjurySets.Critical.Contains(state.BuffId))
                score += 30;

            return score;
        }

        private static FlowKind DetermineFlowKind(
            DebuffState state,
            InjuryVisibilityProfile profile,
            DetectionContext context)
        {
            var level = InjuryVisibilityHelper.GetVisibilityLevel(state);

            if (level == InjuryVisibilityLevel.Unhideable
                || IsEmergencyStatic(state.BuffId, state, context))
            {
                return FlowKind.ForcedReveal;
            }

            if (level >= InjuryVisibilityLevel.Obvious || !profile.CanBeHiddenFromHarvey)
                return FlowKind.Obvious;

            return FlowKind.Suspicion;
        }

        private bool IsEmergency(string buffId, DebuffState state, DetectionContext context) =>
            IsEmergencyStatic(buffId, state, context);

        private static bool IsEmergencyStatic(string buffId, DebuffState state, DetectionContext context)
        {
            var profile = InjuryVisibilityHelper.GetVisibilityProfile(buffId);
            if (profile.BaseVisibility == InjuryVisibilityLevel.Unhideable)
                return true;

            if (context.PlayerHealthLow && InjurySets.Critical.Contains(buffId))
                return true;

            if (context.HasComplication)
                return true;

            if (string.Equals(buffId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(buffId, "buffFracturedBone", StringComparison.OrdinalIgnoreCase))
                return true;

            return state.SuspicionLevel >= 4 && InjurySets.Severe.Contains(buffId);
        }

        private bool CanAskAboutHiddenInjury(DebuffState state, DetectionContext context, FlowKind kind)
        {
            var injuryState = _stateManager.State;

            if (!string.IsNullOrEmpty(injuryState.PendingHiddenInjuryBuffId))
                return false;

            if (IsEmergency(state.BuffId, state, context) || kind == FlowKind.ForcedReveal)
                return true;

            int today = GameUtils.Today();
            if (injuryState.LastHiddenInjuryPostponeDay == today
                && injuryState.LastHiddenInjuryPostponeTime >= 0)
            {
                int elapsed = Game1.timeOfDay - injuryState.LastHiddenInjuryPostponeTime;
                if (elapsed < 0)
                    elapsed += 2400;
                if (elapsed < PostponeCooldownGameMinutes)
                    return false;
            }

            if (injuryState.LastHiddenInjuryQuestionDay == today
                && string.Equals(injuryState.LastHiddenInjuryQuestionBuffId, state.BuffId, StringComparison.OrdinalIgnoreCase)
                && injuryState.HiddenInjuryQuestionCountToday >= 1)
            {
                if (_domesticReason == "evening_home"
                    && (state.PlayerDeniedInjuryToday
                        || state.SuspicionLevel >= 1
                        || state.HiddenDays >= 2
                        || IsEmergency(state.BuffId, state, context)
                        || kind == FlowKind.ForcedReveal))
                {
                    return true;
                }

                if (!IsEmergency(state.BuffId, state, context) && kind != FlowKind.ForcedReveal)
                    return false;
            }

            return true;
        }

        private void MarkQuestionAsked(string buffId)
        {
            int today = GameUtils.Today();
            var injuryState = _stateManager.State;
            injuryState.LastHiddenInjuryQuestionDay = today;
            injuryState.LastHiddenInjuryQuestionBuffId = buffId;
            injuryState.HiddenInjuryQuestionCountToday++;
            injuryState.PendingHiddenInjuryBuffId = buffId;
        }

        private void ClearPending()
        {
            _pendingFlow = null;
            _domesticReason = null;
            _stateManager.State.PendingHiddenInjuryBuffId = "";
            _stateManager.Save();
        }

        private string ResolveQuestionKeyForFlow(FlowKind kind, string buffId, DetectionContext context)
        {
            if (_domesticReason == "morning_home")
                return ResolveDomesticMorningQuestionKey(buffId);

            if (_domesticReason == "evening_home")
            {
                var state = _stateManager.GetDebuffState(buffId);
                return ResolveDomesticEveningQuestionKey(buffId, state, context);
            }

            return kind switch
            {
                FlowKind.ForcedReveal => GetUnhideableQuestionKey(buffId, context),
                FlowKind.Obvious => GetObviousQuestionKey(buffId, context),
                _ => GetSuspicionQuestionKey(buffId, context),
            };
        }

        private string ResolveDomesticMorningQuestionKey(string buffId)
        {
            string clean = InjuryVisibilityHelper.CleanBuffId(buffId);
            foreach (string suffix in new[] { clean, buffId })
            {
                string key = $"HarveyMod_HiddenInjury_MorningHome_{suffix}";
                if (_dialogueManager.HasDialogueKey(key))
                    return key;
            }

            return "HarveyMod_HiddenInjury_MorningHome_Suspected";
        }

        private string ResolveDomesticEveningQuestionKey(string buffId, DebuffState? state, DetectionContext context)
        {
            if (state != null
                && (state.PlayerDeniedInjuryToday
                    || state.HiddenDays >= 2
                    || state.SuspicionLevel >= 2
                    || _stateManager.State.ActiveComplications.Count > 0))
            {
                if (_dialogueManager.HasDialogueKey("HarveyMod_HiddenInjury_EveningHome_AfterDenied"))
                    return "HarveyMod_HiddenInjury_EveningHome_AfterDenied";

                return "HarveyMod_HiddenInjury_EveningHome_Strict";
            }

            string clean = InjuryVisibilityHelper.CleanBuffId(buffId);
            foreach (string suffix in new[] { clean, buffId })
            {
                string key = $"HarveyMod_HiddenInjury_EveningHome_{suffix}";
                if (_dialogueManager.HasDialogueKey(key))
                    return key;
            }

            if (state != null && state.HiddenDays >= 1)
                return "HarveyMod_HiddenInjury_EveningHome_Strict";

            if (_dialogueManager.HasDialogueKey("HarveyMod_HiddenInjury_EveningHome_Suspected"))
                return "HarveyMod_HiddenInjury_EveningHome_Suspected";

            return "HarveyMod_HiddenInjury_EveningHome_Gentle";
        }

        private Response[] BuildChoices(FlowKind kind, DetectionContext context)
        {
            var choices = new List<Response>();

            switch (kind)
            {
                case FlowKind.Suspicion:
                    choices.Add(MakeChoice("show", "Show"));
                    choices.Add(MakeChoice("deny", "Deny"));
                    choices.Add(MakeChoice("joke", "Joke"));
                    choices.Add(MakeChoice("not_now", "NotNow"));
                    if (context.IsMarriedToHarvey || context.IsDatingOrEngagedToHarvey)
                    {
                        choices.Add(MakeChoice("scared", "Scared"));
                        if (choices.Count < MaxChoicesPerQuestion)
                            choices.Add(MakeChoice("ashamed", "Ashamed"));
                    }
                    break;

                case FlowKind.Obvious:
                    choices.Add(MakeChoice("show", "Show"));
                    if (context.IsMarriedToHarvey || context.IsDatingOrEngagedToHarvey)
                        choices.Add(MakeChoice("scared", "Scared"));
                    choices.Add(MakeChoice("joke", "Joke"));
                    break;

                case FlowKind.ForcedReveal:
                    choices.Add(MakeChoice("examine", "Show"));
                    choices.Add(MakeChoice("scared", "Scared"));
                    choices.Add(new Response("nod", "...Молча кивнуть."));
                    break;
            }

            return choices.Take(MaxChoicesPerQuestion).ToArray();
        }

        private Response MakeChoice(string answerId, string choiceSuffix) =>
            new(answerId, StripDialogueCommands(
                _dialogueManager.GetDialogueText($"HarveyMod_HiddenInjury_Choice_{choiceSuffix}")));

        private string GetSuspicionQuestionKey(string buffId, DetectionContext context) =>
            ResolveInjuryDialogueKey("HarveyMod_HiddenInjury_Suspected", buffId, context);

        private string GetObviousQuestionKey(string buffId, DetectionContext context) =>
            ResolveInjuryDialogueKey("HarveyMod_HiddenInjury_Obvious", buffId, context);

        private string GetUnhideableQuestionKey(string buffId, DetectionContext context) =>
            ResolveInjuryDialogueKey("HarveyMod_HiddenInjury_Unhideable", buffId, context);

        private string GetConfessedKey(string buffId, DetectionContext context) =>
            ResolveInjuryDialogueKey("HarveyMod_HiddenInjury_Confessed", buffId, context);

        private string GetDeniedKey(string buffId, DetectionContext context) =>
            ResolveInjuryDialogueKey("HarveyMod_HiddenInjury_Denied", buffId, context, genericOnly: true);

        private string GetNotNowKey(DetectionContext context) =>
            ResolveRelationshipKey("HarveyMod_HiddenInjury_NotNow", context, "HarveyMod_HiddenInjury_NotNow_Generic");

        private string GetFestivalNoticeKey(string buffId, DetectionContext context)
        {
            string clean = InjuryVisibilityHelper.CleanBuffId(buffId);
            foreach (string suffix in new[] { clean, buffId })
            {
                string specific = $"HarveyMod_HiddenInjury_Festival_Notice_{suffix}";
                if (_dialogueManager.HasDialogueKey(specific))
                    return specific;
            }

            if (string.Equals(buffId, "buffFracturedBone", StringComparison.OrdinalIgnoreCase)
                && _dialogueManager.HasDialogueKey("HarveyMod_HiddenInjury_Festival_Notice_Fracture"))
            {
                return "HarveyMod_HiddenInjury_Festival_Notice_Fracture";
            }

            return "HarveyMod_HiddenInjury_Festival_Notice_Generic";
        }

        private string ResolveResponsePrefix(string suffix, DetectionContext context)
        {
            if (context.IsMarriedToHarvey
                && _dialogueManager.HasDialogueKey($"HarveyMod_HiddenInjury_Response_{suffix}_Married"))
            {
                return $"HarveyMod_HiddenInjury_Response_{suffix}_Married";
            }

            if (IsEngagedToHarvey()
                && _dialogueManager.HasDialogueKey($"HarveyMod_HiddenInjury_Response_{suffix}_Engaged"))
            {
                return $"HarveyMod_HiddenInjury_Response_{suffix}_Engaged";
            }

            if (context.IsDatingOrEngagedToHarvey
                && _dialogueManager.HasDialogueKey($"HarveyMod_HiddenInjury_Response_{suffix}_Dating"))
            {
                return $"HarveyMod_HiddenInjury_Response_{suffix}_Dating";
            }

            return $"HarveyMod_HiddenInjury_Response_{suffix}";
        }

        private string ResolveResponseKey(string suffix, DetectionContext context) =>
            ResolveResponsePrefix(suffix, context);

        private string ResolveInjuryDialogueKey(
            string prefix,
            string buffId,
            DetectionContext context,
            bool genericOnly = false)
        {
            if (!genericOnly)
            {
                string clean = InjuryVisibilityHelper.CleanBuffId(buffId);
                foreach (string injurySuffix in new[] { clean, buffId })
                {
                    if (context.IsMarriedToHarvey)
                    {
                        string marriedSpecific = $"{prefix}_{injurySuffix}_Married";
                        if (_dialogueManager.HasDialogueKey(marriedSpecific))
                            return marriedSpecific;
                    }

                    string specific = $"{prefix}_{injurySuffix}";
                    if (_dialogueManager.HasDialogueKey(specific))
                        return specific;

                    string buffSpecific = $"{prefix}_buff{injurySuffix}";
                    if (_dialogueManager.HasDialogueKey(buffSpecific))
                        return buffSpecific;
                }
            }

            return ResolveRelationshipKey(prefix, context, $"{prefix}_Generic");
        }

        private string ResolveRelationshipKey(string prefix, DetectionContext context, string genericFallback)
        {
            if (context.IsMarriedToHarvey)
            {
                string married = $"{prefix}_Married";
                if (_dialogueManager.HasDialogueKey(married))
                    return married;
                string marriedGeneric = $"{prefix}_Generic_Married";
                if (_dialogueManager.HasDialogueKey(marriedGeneric))
                    return marriedGeneric;
            }

            if (IsEngagedToHarvey())
            {
                string engaged = $"{prefix}_Engaged";
                if (_dialogueManager.HasDialogueKey(engaged))
                    return engaged;
                string engagedGeneric = $"{prefix}_Generic_Engaged";
                if (_dialogueManager.HasDialogueKey(engagedGeneric))
                    return engagedGeneric;
            }

            if (context.IsDatingOrEngagedToHarvey)
            {
                string dating = $"{prefix}_Dating";
                if (_dialogueManager.HasDialogueKey(dating))
                    return dating;
                string datingGeneric = $"{prefix}_Generic_Dating";
                if (_dialogueManager.HasDialogueKey(datingGeneric))
                    return datingGeneric;
            }

            if (_dialogueManager.HasDialogueKey(genericFallback))
                return genericFallback;

            return prefix;
        }

        private DetectionContext BuildDetectionContextForRelationship() =>
            InjuryVisibilityHelper.BuildDetectionContext(_config, _stateManager.State, true, false);

        private bool IsEngagedToHarvey() =>
            Game1.player?.friendshipData.TryGetValue("Harvey", out var f) == true && f.IsEngaged();

        private static string StripDialogueCommands(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            int cut = text.IndexOf('$');
            if (cut >= 0)
                text = text[..cut];

            return text.Replace("#$b#", "\n", StringComparison.Ordinal).Trim();
        }
    }
}
