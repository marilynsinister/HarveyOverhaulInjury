using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Единая точка запуска домашних care-событий Харви (FarmHouse).
    /// Максимум одно событие за вызов; приоритет — от внешнего спасения к утреннему истощению.
    /// </summary>
    public sealed class HarveyHomeCareEventLauncher
    {
        public const int HospitalDischargeTimeStart = 1800;
        public const int HospitalDischargeTimeEnd = 2600;
        public const int NightCareTimeStart = 2200;
        public const int NightCareTimeEnd = 2600;
        public const int MorningExhaustionTimeStart = 600;
        public const int MorningExhaustionTimeEnd = 1100;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly InjuryManager _injuryManager;
        private readonly ComplicationManager _complicationManager;

        public HarveyHomeCareEventLauncher(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            HospitalizationManager hospitalizationManager,
            InjuryManager injuryManager,
            ComplicationManager complicationManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _hospitalizationManager = hospitalizationManager;
            _injuryManager = injuryManager;
            _complicationManager = complicationManager;
        }

        /// <summary>
        /// Попытка запустить одно домашнее care-событие по приоритету.
        /// </summary>
        /// <returns>true, если событие (или fallback-диалог ночного обхода) запущено.</returns>
        public bool TryTriggerHarveyHomeCareEvent(GameLocation? location = null, string source = "unknown")
        {
            location ??= Game1.currentLocation;

            if (!LocationEventLauncher.IsFarmHouseLocation(location))
            {
                _monitor.Log(
                    $"[HomeCare/{source}] Пропуск: игрок не в FarmHouse ({location?.NameOrUniqueName})",
                    LogLevel.Trace);
                return false;
            }

            var state = _stateManager.State;
            int today = GameUtils.Today();
            int time = Game1.timeOfDay;

            _monitor.Log(
                $"[HomeCare/{source}] Проверка pending: externalRescue={state.NeedsHarveyAfterExternalRescueHomeEvent}, " +
                $"hospitalDischarge={state.NeedsHarveyAfterHospitalDischargeHomeEvent}, " +
                $"nightRoundFirst={state.NeedsSevereNightRoundEvent}, " +
                $"morningExhaustion={state.NeedsHarveyMorningAfterExhaustionEvent}, " +
                $"time={time}, day={today}",
                LogLevel.Trace);

            foreach (var candidate in GetPriorityOrder())
            {
                if (!IsEligible(candidate, state, today, time, location))
                    continue;

                _monitor.Log(
                    $"[HomeCare/{source}] Кандидат: {candidate} (topic/flag={DescribeCandidateFlags(candidate, state)})",
                    LogLevel.Debug);

                if (IsPlayerBlocked(out string blockReason))
                {
                    _monitor.Log(
                        $"[HomeCare/{source}] Отложено ({candidate}): {blockReason}",
                        LogLevel.Trace);
                    return false;
                }

                var launchResult = TryLaunch(candidate, state, today, location, source);
                switch (launchResult)
                {
                    case HomeCareLaunchResult.Launched:
                        return true;
                    case HomeCareLaunchResult.DeferredBlocked:
                        return false;
                    case HomeCareLaunchResult.EventNotFound:
                        if (candidate == HomeCareEventKind.NightRoundSevereFirst)
                        {
                            _monitor.Log(
                                $"[HomeCare/{source}] CP-событие '{EventIds.NightRoundSevereFirst}' не найдено — fallback на короткий диалог",
                                LogLevel.Warn);
                            return TryNightRoundSevereFirstFallbackDialogue(state, today, source);
                        }

                        _monitor.Log(
                            $"[HomeCare/{source}] CP-событие для {candidate} не найдено — флаг/topic сохранены, повтор позже",
                            LogLevel.Warn);
                        return false;
                    case HomeCareLaunchResult.Skipped:
                        continue;
                }
            }

            _monitor.Log($"[HomeCare/{source}] Нет подходящих pending-событий", LogLevel.Trace);
            return false;
        }

        /// <summary>
        /// Короткий ночной визит (35% за ночь) — только если нет pending событий с более высоким приоритетом.
        /// </summary>
        public void TryTriggerShortNightRoundVisit(int newTime)
        {
            if (newTime < NightCareTimeStart || newTime > NightCareTimeEnd)
                return;

            if (!LocationEventLauncher.IsFarmHouseLocation(Game1.player.currentLocation))
                return;

            if (!_injuryManager.IsMainInjurySerious())
                return;

            if (!_dialogueManager.IsDatingOrMarriedToHarvey())
                return;

            var state = _stateManager.State;
            int today = GameUtils.Today();

            if (HasPendingHigherPriorityNightCare(state, today))
            {
                _monitor.Log(
                    "[HomeCare/ShortNightRound] Пропуск: есть pending событие с более высоким приоритетом",
                    LogLevel.Trace);
                return;
            }

            if (state.LastNightRoundDay == today)
                return;

            if (state.LastNightRoundRollDay == today)
                return;

            state.LastNightRoundRollDay = today;
            _stateManager.Save();

            if (!GameUtils.Roll(Math.Clamp(_config.NightVisitChance, 0.0, 1.0)))
            {
                _monitor.Log("[HomeCare/ShortNightRound] Roll не сработал сегодня", LogLevel.Debug);
                return;
            }

            if (IsPlayerBlocked(out string blockReason))
            {
                _monitor.Log(
                    $"[HomeCare/ShortNightRound] Отложено: {blockReason}",
                    LogLevel.Trace);
                return;
            }

            _monitor.Log("[HomeCare/ShortNightRound] Короткий ночной визит", LogLevel.Info);

            string line = "Тихо постучал и заглянул — не спи на животе, ладно?$u#$b#" +
                          "Пульс ровный. Не геройствуй до утра — я присмотрю.$l";

            var harvey = HarveyHelper.GetHarvey();
            if (harvey != null)
                _dialogueManager.Speak(harvey, line);

            state.LastNightRoundDay = today;

            string farmHouseName = LocationEventLauncher.GetFarmHouseLocationName();
            if (state.NeedsSevereNightRoundEvent
                && !LocationEventLauncher.LocationEventExists(EventIds.NightRoundSevereFirst, farmHouseName))
            {
                _monitor.Log(
                    "[HomeCare/ShortNightRound] CP первого обхода отсутствует — сброс NeedsSevereNightRoundEvent после fallback",
                    LogLevel.Debug);
                state.NeedsSevereNightRoundEvent = false;
                string? mainInjuryId = _injuryManager.GetActiveInjury();
                if (!string.IsNullOrEmpty(mainInjuryId))
                    state.SevereNightRoundInjuryId = mainInjuryId;
            }

            _stateManager.Save();
            ApplyNightRoundVisitBonuses();
            _dialogueManager.AddTopic(ConversationTopics.NightRound, 2);
        }

        public static bool IsHospitalDischargePending(InjuryState state, int today)
        {
            return state.NeedsHarveyAfterHospitalDischargeHomeEvent
                && state.HarveyAfterHospitalDischargeShownDay != today;
        }

        private static HomeCareEventKind[] GetPriorityOrder()
        {
            return new[]
            {
                HomeCareEventKind.ExternalRescue,
                HomeCareEventKind.HospitalDischarge,
                HomeCareEventKind.NightRoundSevereFirst,
                HomeCareEventKind.MorningAfterExhaustion,
            };
        }

        private static bool HasPendingHigherPriorityNightCare(InjuryState state, int today)
        {
            if (state.NeedsHarveyAfterExternalRescueHomeEvent
                && state.HarveyAfterExternalRescueShownDay != today)
            {
                return true;
            }

            if (IsHospitalDischargePending(state, today))
                return true;

            if (state.NeedsSevereNightRoundEvent
                && state.SevereNightRoundEventShownDay != today)
            {
                return true;
            }

            return false;
        }

        private bool IsEligible(
            HomeCareEventKind kind,
            InjuryState state,
            int today,
            int time,
            GameLocation location)
        {
            switch (kind)
            {
                case HomeCareEventKind.ExternalRescue:
                    if (!state.NeedsHarveyAfterExternalRescueHomeEvent)
                        return false;
                    if (state.HarveyAfterExternalRescueShownDay == today)
                        return false;
                    if (time < NightCareTimeStart || time > NightCareTimeEnd)
                        return false;
                    return PassesCommonNightCareEligibility(state, location);

                case HomeCareEventKind.HospitalDischarge:
                    if (!IsHospitalDischargePending(state, today))
                        return false;
                    if (time < HospitalDischargeTimeStart || time > HospitalDischargeTimeEnd)
                        return false;
                    return PassesCommonNightCareEligibility(state, location);

                case HomeCareEventKind.NightRoundSevereFirst:
                    if (!state.NeedsSevereNightRoundEvent)
                        return false;
                    if (state.SevereNightRoundEventShownDay == today)
                        return false;
                    if (time < NightCareTimeStart || time > NightCareTimeEnd)
                        return false;
                    if (!_injuryManager.IsMainInjurySerious())
                        return false;
                    if (!_dialogueManager.IsDatingOrMarriedToHarvey())
                        return false;
                    if (string.IsNullOrEmpty(_injuryManager.GetActiveInjury()))
                        return false;
                    return LocationEventLauncher.IsFarmHouseLocation(location);

                case HomeCareEventKind.MorningAfterExhaustion:
                    if (!state.NeedsHarveyMorningAfterExhaustionEvent)
                        return false;
                    if (!GameUtils.HasConversationTopic(ConversationTopics.HarveyMorningAfterExhaustion))
                    {
                        state.NeedsHarveyMorningAfterExhaustionEvent = false;
                        _stateManager.Save();
                        _monitor.Log(
                            "[HomeCare/MorningAfterExhaustion] Topic истёк — сброс NeedsHarveyMorningAfterExhaustionEvent",
                            LogLevel.Debug);
                        return false;
                    }
                    if (state.HarveyMorningAfterExhaustionShownDay == today)
                        return false;
                    if (time < MorningExhaustionTimeStart || time > MorningExhaustionTimeEnd)
                        return false;
                    if (!_dialogueManager.IsDatingEngagedOrMarriedToHarvey())
                        return false;
                    return LocationEventLauncher.IsFarmHouseLocation(location);

                default:
                    return false;
            }
        }

        private bool PassesCommonNightCareEligibility(InjuryState state, GameLocation location)
        {
            if (_hospitalizationManager.IsHospitalized || _hospitalizationManager.IsInClinic(location))
                return false;

            if (state.NeedsMineRescueEvent
                || !string.IsNullOrEmpty(state.PendingMineRescueEventId)
                || state.PassedOutInMineYesterday)
            {
                return false;
            }

            if (!_dialogueManager.IsDatingEngagedOrMarriedToHarvey())
                return false;

            return LocationEventLauncher.IsFarmHouseLocation(location);
        }

        private HomeCareLaunchResult TryLaunch(
            HomeCareEventKind kind,
            InjuryState state,
            int today,
            GameLocation location,
            string source)
        {
            string farmHouseName = LocationEventLauncher.GetFarmHouseLocationName();
            int fx = (int)Game1.player.Tile.X;
            int fy = (int)Game1.player.Tile.Y;
            string setup =
                $"viewport {fx} {fy} true/skippable/pause 200/faceDirection farmer 2/warp Harvey {fx + 1} {fy} 3/faceDirection Harvey 3/";

            switch (kind)
            {
                case HomeCareEventKind.ExternalRescue:
                {
                    string eventId = ResolveAfterExternalRescueHomeEventId();
                    if (!LocationEventLauncher.LocationEventExists(eventId, farmHouseName))
                        return HomeCareLaunchResult.EventNotFound;

                    if (!LocationEventLauncher.TryStartEventByPrefix(
                            eventId,
                            farmHouseName,
                            _monitor,
                            onFinished: resolvedId => OnExternalRescueFinished(resolvedId, source),
                            scriptPrefix: setup,
                            addToEventsSeenOnFinished: false,
                            logTag: "HomeCare/ExternalRescue"))
                    {
                        return HomeCareLaunchResult.EventNotFound;
                    }

                    state.HarveyAfterExternalRescueShownDay = today;
                    state.NeedsHarveyAfterExternalRescueHomeEvent = false;
                    _stateManager.Save();
                    _monitor.Log(
                        $"[HomeCare/{source}] Запущено ExternalRescue '{eventId}' " +
                        $"(flag NeedsHarveyAfterExternalRescueHomeEvent сброшен, shownDay={today}, " +
                        $"topic={ConversationTopics.ExternalRescueConcern}, loc={state.LastExternalRescueLocation})",
                        LogLevel.Info);
                    return HomeCareLaunchResult.Launched;
                }

                case HomeCareEventKind.HospitalDischarge:
                {
                    if (!LocationEventLauncher.LocationEventExists(EventIds.AfterHospitalDischargeHome, farmHouseName))
                        return HomeCareLaunchResult.EventNotFound;

                    if (!LocationEventLauncher.TryStartEventByPrefix(
                            EventIds.AfterHospitalDischargeHome,
                            farmHouseName,
                            _monitor,
                            onFinished: resolvedId => OnHospitalDischargeFinished(resolvedId, source),
                            scriptPrefix: setup,
                            addToEventsSeenOnFinished: false,
                            logTag: "HomeCare/HospitalDischarge"))
                    {
                        return HomeCareLaunchResult.EventNotFound;
                    }

                    state.HarveyAfterHospitalDischargeShownDay = today;
                    state.NeedsHarveyAfterHospitalDischargeHomeEvent = false;
                    _stateManager.Save();
                    _monitor.Log(
                        $"[HomeCare/{source}] Запущено HospitalDischarge " +
                        $"(flag NeedsHarveyAfterHospitalDischargeHomeEvent сброшен, shownDay={today}, " +
                        $"topic={ConversationTopics.AfterHospitalDischargeHome}, injury={state.LastHospitalDischargeInjuryId})",
                        LogLevel.Info);
                    return HomeCareLaunchResult.Launched;
                }

                case HomeCareEventKind.NightRoundSevereFirst:
                {
                    string? mainInjuryId = _injuryManager.GetActiveInjury();
                    if (string.IsNullOrEmpty(mainInjuryId))
                        return HomeCareLaunchResult.Skipped;

                    string nightSetup = setup + "ambientLight 70 75 110/";

                    if (!LocationEventLauncher.LocationEventExists(EventIds.NightRoundSevereFirst, farmHouseName))
                    {
                        _dialogueManager.RemoveTopic(ConversationTopics.NightRoundSevereFirst);
                        return HomeCareLaunchResult.EventNotFound;
                    }

                    _dialogueManager.AddTopic(ConversationTopics.NightRoundSevereFirst, 2);

                    if (!LocationEventLauncher.TryStartEventByPrefix(
                            EventIds.NightRoundSevereFirst,
                            farmHouseName,
                            _monitor,
                            onFinished: resolvedId => OnNightRoundSevereFirstFinished(resolvedId, source),
                            scriptPrefix: nightSetup,
                            addToEventsSeenOnFinished: false,
                            logTag: "HomeCare/NightRoundSevereFirst"))
                    {
                        _dialogueManager.RemoveTopic(ConversationTopics.NightRoundSevereFirst);
                        return HomeCareLaunchResult.EventNotFound;
                    }

                    state.SevereNightRoundEventShownDay = today;
                    state.LastNightRoundDay = today;
                    state.SevereNightRoundInjuryId = mainInjuryId;
                    state.NeedsSevereNightRoundEvent = false;
                    _stateManager.Save();
                    _monitor.Log(
                        $"[HomeCare/{source}] Запущено NightRoundSevereFirst " +
                        $"(flag NeedsSevereNightRoundEvent сброшен, shownDay={today}, " +
                        $"topic={ConversationTopics.NightRoundSevereFirst}, injury={mainInjuryId})",
                        LogLevel.Info);
                    return HomeCareLaunchResult.Launched;
                }

                case HomeCareEventKind.MorningAfterExhaustion:
                {
                    if (!LocationEventLauncher.LocationEventExists(EventIds.MorningAfterExhaustion, farmHouseName))
                        return HomeCareLaunchResult.EventNotFound;

                    if (!LocationEventLauncher.TryStartEventByPrefix(
                            EventIds.MorningAfterExhaustion,
                            farmHouseName,
                            _monitor,
                            onFinished: resolvedId => OnMorningAfterExhaustionFinished(resolvedId, source),
                            scriptPrefix: setup,
                            addToEventsSeenOnFinished: false,
                            logTag: "HomeCare/MorningAfterExhaustion"))
                    {
                        return HomeCareLaunchResult.EventNotFound;
                    }

                    state.NeedsHarveyMorningAfterExhaustionEvent = false;
                    state.HarveyMorningAfterExhaustionShownDay = today;
                    _stateManager.Save();
                    _monitor.Log(
                        $"[HomeCare/{source}] Запущено MorningAfterExhaustion " +
                        $"(flag NeedsHarveyMorningAfterExhaustionEvent сброшен, shownDay={today}, " +
                        $"topic={ConversationTopics.HarveyMorningAfterExhaustion}, collapseDay={state.LastExhaustionCollapseDay})",
                        LogLevel.Info);
                    return HomeCareLaunchResult.Launched;
                }

                default:
                    return HomeCareLaunchResult.Skipped;
            }
        }

        private bool TryNightRoundSevereFirstFallbackDialogue(InjuryState state, int today, string source)
        {
            if (IsPlayerBlocked(out string blockReason))
            {
                _monitor.Log(
                    $"[HomeCare/{source}] Fallback-диалог ночного обхода отложен: {blockReason}",
                    LogLevel.Trace);
                return false;
            }

            string? mainInjuryId = _injuryManager.GetActiveInjury();
            if (string.IsNullOrEmpty(mainInjuryId))
                return false;

            _dialogueManager.RemoveTopic(ConversationTopics.NightRoundSevereFirst);

            string line = "Тихо постучал и заглянул — не спи на животе, ладно?$u#$b#" +
                          "Пульс ровный. Не геройствуй до утра — я присмотрю.$l";

            var harvey = HarveyHelper.GetHarvey();
            if (harvey != null)
                _dialogueManager.Speak(harvey, line);

            state.SevereNightRoundEventShownDay = today;
            state.LastNightRoundDay = today;
            state.SevereNightRoundInjuryId = mainInjuryId;
            state.NeedsSevereNightRoundEvent = false;
            _stateManager.Save();

            ApplyNightRoundVisitBonuses();
            _dialogueManager.AddTopic(ConversationTopics.NightRound, 2);

            _monitor.Log(
                $"[HomeCare/{source}] Fallback-диалог NightRoundSevereFirst " +
                $"(flag NeedsSevereNightRoundEvent сброшен, shownDay={today}, injury={mainInjuryId})",
                LogLevel.Info);
            return true;
        }

        private void OnExternalRescueFinished(string eventId, string source)
        {
            _monitor.Log(
                $"[HomeCare/{source}] ExternalRescue '{eventId}' завершено (eventsSeen не добавлялся)",
                LogLevel.Info);
            _stateManager.Save();
        }

        private void OnHospitalDischargeFinished(string eventId, string source)
        {
            _monitor.Log(
                $"[HomeCare/{source}] HospitalDischarge '{eventId}' завершено (eventsSeen не добавлялся)",
                LogLevel.Info);
            _stateManager.Save();
        }

        private void OnNightRoundSevereFirstFinished(string eventId, string source)
        {
            _monitor.Log(
                $"[HomeCare/{source}] NightRoundSevereFirst '{eventId}' завершено (eventsSeen не добавлялся)",
                LogLevel.Info);
            ApplyNightRoundVisitBonuses();
            _stateManager.Save();
        }

        private void OnMorningAfterExhaustionFinished(string eventId, string source)
        {
            _dialogueManager.RemoveTopic(ConversationTopics.HarveyMorningAfterExhaustion);
            if (!GameUtils.HasConversationTopic(ConversationTopics.ExhaustionFollowup))
                _dialogueManager.AddTopic(ConversationTopics.ExhaustionFollowup, 2);
            _stateManager.Save();
            _monitor.Log(
                $"[HomeCare/{source}] MorningAfterExhaustion '{eventId}' завершено; " +
                $"topic {ConversationTopics.HarveyMorningAfterExhaustion} снят, followup добавлен",
                LogLevel.Info);
        }

        private void ApplyNightRoundVisitBonuses()
        {
            Game1.player.changeFriendship(10, Game1.getCharacterFromName("Harvey"));

            if (_complicationManager.HasComplication(InjuryBuffs.PainFlare) && GameUtils.Roll(0.5))
            {
                _complicationManager.RemoveComplicationForQa(InjuryBuffs.PainFlare);
                Game1.addHUDMessage(new HUDMessage("После ночного визита Харви боль утихла.", 2));
            }
        }

        private static bool IsPlayerBlocked(out string reason)
        {
            if (Game1.CurrentEvent != null)
            {
                reason = "активно Game1.CurrentEvent";
                return true;
            }

            if (Game1.eventUp)
            {
                reason = "Game1.eventUp";
                return true;
            }

            if (Game1.dialogueUp)
            {
                reason = "Game1.dialogueUp (DialogueBox)";
                return true;
            }

            if (Game1.activeClickableMenu != null)
            {
                reason = $"activeClickableMenu={Game1.activeClickableMenu.GetType().Name}";
                return true;
            }

            if (!Context.IsPlayerFree)
            {
                reason = "!Context.IsPlayerFree";
                return true;
            }

            if (Game1.locationRequest != null)
            {
                reason = "ожидается варп (locationRequest)";
                return true;
            }

            reason = "";
            return false;
        }

        private static string DescribeCandidateFlags(HomeCareEventKind kind, InjuryState state)
        {
            return kind switch
            {
                HomeCareEventKind.ExternalRescue =>
                    $"NeedsHarveyAfterExternalRescueHomeEvent, topic={ConversationTopics.ExternalRescueConcern}, rescueDay={state.LastExternalRescueDay}",
                HomeCareEventKind.HospitalDischarge =>
                    $"NeedsHarveyAfterHospitalDischargeHomeEvent, topic={ConversationTopics.AfterHospitalDischargeHome}, dischargeDay={state.LastHospitalDischargeDay}",
                HomeCareEventKind.NightRoundSevereFirst =>
                    $"NeedsSevereNightRoundEvent, topic={ConversationTopics.NightRoundSevereFirst}, injury={state.SevereNightRoundInjuryId}",
                HomeCareEventKind.MorningAfterExhaustion =>
                    $"NeedsHarveyMorningAfterExhaustionEvent, topic={ConversationTopics.HarveyMorningAfterExhaustion}, collapseDay={state.LastExhaustionCollapseDay}",
                _ => kind.ToString(),
            };
        }

        private static string ResolveAfterExternalRescueHomeEventId()
        {
            if (Game1.player?.friendshipData == null
                || !Game1.player.friendshipData.TryGetValue("Harvey", out var friendship))
            {
                return EventIds.AfterExternalRescueHome;
            }

            if (friendship.IsMarried())
                return EventIds.AfterExternalRescueHomeMarried;
            if (friendship.IsEngaged())
                return EventIds.AfterExternalRescueHomeEngaged;
            if (friendship.IsDating())
                return EventIds.AfterExternalRescueHome;

            return EventIds.AfterExternalRescueHome;
        }

        private enum HomeCareEventKind
        {
            ExternalRescue,
            HospitalDischarge,
            NightRoundSevereFirst,
            MorningAfterExhaustion,
        }

        private enum HomeCareLaunchResult
        {
            Launched,
            DeferredBlocked,
            EventNotFound,
            Skipped,
        }
    }
}
