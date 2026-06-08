using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик событий игрока (перемещение, здоровье)
    /// </summary>
    public class PlayerEventHandler
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly TreatmentManager _treatmentManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly DialogueManager _dialogueManager;
        private readonly HarveyReactionManager _harveyReactionManager;
        private readonly PrescriptionManager _prescriptionManager;
        private readonly ComplianceManager _complianceManager;
        private readonly CareTrustManager _careTrustManager;
        private readonly RehabManager _rehabManager;
        private readonly RecoveryPlanManager _recoveryPlanManager;
        private readonly ComplicationManager _complicationManager;
        private readonly MineEntryCoordinator _mineEntryCoordinator;

        private PassOutHandler? _passOutHandler;
        private HarveyHomeCareEventLauncher? _homeCareLauncher;

        /// <summary>Непрерывное использование тяжёлых инструментов при низкой stamina (секунды).</summary>
        private int _lightWorkLowStaminaToolSeconds;

        private const int KeepDryRainViolationSeconds = 120;
        private const int LightWorkViolationSeconds = 90;
        private const double PrescriptionDirtyWoundBaseChance = 0.10;
        private const double PrescriptionDirtyWoundBonusChance = 0.20;
        private const double PrescriptionWetBandageBonusChance = 0.20;
        private const double PrescriptionWetStitchesBonusChance = 0.15;

        /// <summary>Последняя обычная proximity-реакция (игровые минуты) — в InjuryState для debug.</summary>
        /// <summary>Одно облачко за визит в локацию (сброс при варпе в другую локацию).</summary>
        private bool _proximityReactionShown = false;
        private const int ProximityReactionCooldownMinutes = 120;
        private string _lastLocationName = "";
        /// <summary>День последнего мягкого HUD в шахте (не MineWarningDay — тот только для Severe).</summary>
        private int _lastMineSoftHudDay = -1;
        private bool _eventWasActive;
        private bool _stormComfortEventRunning;
        private bool _firstTreatmentEventRunning;
        private bool _firstTreatmentTopicClearedForRun;
        private bool _e5StormBesideEventRunning;
        private bool _rescueOperationEventRunning;

        public PlayerEventHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            TreatmentManager treatmentManager,
            HospitalizationManager hospitalizationManager,
            DialogueManager dialogueManager,
            HarveyReactionManager harveyReactionManager,
            PrescriptionManager prescriptionManager,
            ComplianceManager complianceManager,
            CareTrustManager careTrustManager,
            RehabManager rehabManager,
            RecoveryPlanManager recoveryPlanManager,
            ComplicationManager complicationManager,
            MineEntryCoordinator mineEntryCoordinator)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _treatmentManager = treatmentManager;
            _hospitalizationManager = hospitalizationManager;
            _dialogueManager = dialogueManager;
            _harveyReactionManager = harveyReactionManager;
            _prescriptionManager = prescriptionManager;
            _complianceManager = complianceManager;
            _careTrustManager = careTrustManager;
            _rehabManager = rehabManager;
            _recoveryPlanManager = recoveryPlanManager;
            _complicationManager = complicationManager;
            _mineEntryCoordinator = mineEntryCoordinator;
        }

        public void SetPassOutHandler(PassOutHandler passOutHandler)
        {
            _passOutHandler = passOutHandler;
        }

        public void SetHomeCareLauncher(HarveyHomeCareEventLauncher homeCareLauncher)
        {
            _homeCareLauncher = homeCareLauncher;
        }

        /// <summary>
        /// Игрок переместился в другую локацию
        /// </summary>
        public void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer) return;

            try
            {
                _dialogueManager.ClearHarveyNeedsFirstTreatmentTopicIfObsolete("HarveyMod_FirstTreatment завершён");

                // Одна proximity-реакция на локацию: сброс только «уже показано здесь» (кулдаун 2 ч сохраняется).
                if (e.NewLocation?.Name != _lastLocationName)
                {
                    _proximityReactionShown = false;
                    _lastLocationName = e.NewLocation?.Name ?? "";
                    _monitor.Log(
                        $"[Proximity] Локация «{_lastLocationName}»: сброс per-location, кулдаун с {_stateManager.State.LastProximityReactionMinute} игр. мин",
                        LogLevel.Debug);
                }

                if (Game1.CurrentEvent != null || Game1.eventUp)
                {
                    if (TryAbortMineInterceptionOnExternalWarp(e))
                        return;

                    _monitor.Log("[Warped] Пропуск location logic: активно событие.", LogLevel.Trace);
                    return;
                }

                // Проверка госпитализации
                bool hospitalExitBlocked = _hospitalizationManager.HandleWarpAttempt(e.NewLocation, e.OldLocation);
                _hospitalizationManager.NotifyPlayerWarped(e.NewLocation);

                if (hospitalExitBlocked)
                {
                    return; // Варп заблокирован — возврат в палату отложен или уже запущен
                }

                // Логика локаций
                HandleLocationLogic(e.NewLocation);

                _homeCareLauncher?.TryTriggerHarveyHomeCareEvent(e.NewLocation, "Warp");

                if (IsMineOrVolcano(e.OldLocation) && !IsMineOrVolcano(e.NewLocation))
                {
                    _stateManager.State.LastMineDirtyWoundRollMinute = -1;
                    _stateManager.State.MineDirtyRiskBoostUntilMinute = -1;
                    _monitor.Log(
                        "[Шахта] Игрок вышел из шахты/вулкана: сброшен roll timer и damage boost грязной раны",
                        LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка в OnWarped: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Обновление каждый тик
        /// </summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            try
            {
                bool eventActive = Game1.eventUp;
                if (eventActive)
                {
                    string? eventId = Game1.currentLocation?.currentEvent?.id;
                    _stormComfortEventRunning = StormComfortLauncher.IsStormComfortEventId(eventId);
                    _firstTreatmentEventRunning = string.Equals(
                        eventId,
                        EventIds.FirstTreatment,
                        StringComparison.OrdinalIgnoreCase);
                    if (_firstTreatmentEventRunning && !_firstTreatmentTopicClearedForRun)
                    {
                        _dialogueManager.ClearHarveyNeedsFirstTreatmentTopic(
                            "HarveyMod_FirstTreatment запущено — снят триггер повторного входа");
                        _firstTreatmentTopicClearedForRun = true;
                    }
                    _e5StormBesideEventRunning = string.Equals(
                        eventId,
                        RescueOperationIds.E5StormBesideEvent,
                        StringComparison.OrdinalIgnoreCase);
                    _rescueOperationEventRunning = RescueOperationLauncher.IsRescueOperationEventId(eventId);
                }
                else if (_eventWasActive)
                {
                    _firstTreatmentTopicClearedForRun = false;
                    if (_stormComfortEventRunning)
                    {
                        StormComfortLauncher.MarkStormComfortEventPlayed(_stateManager, _monitor);
                        RescueOperationLauncher.TryOfferRescueOperationTopic(
                            _dialogueManager,
                            _monitor,
                            "storm comfort");
                        _stormComfortEventRunning = false;
                    }

                    if (_e5StormBesideEventRunning)
                    {
                        RescueOperationLauncher.TryOfferRescueOperationTopic(
                            _dialogueManager,
                            _monitor,
                            "E5_StormBeside");
                        _e5StormBesideEventRunning = false;
                    }

                    if (_rescueOperationEventRunning)
                    {
                        RescueOperationLauncher.MarkRescueOperationPlayed(
                            _dialogueManager,
                            _stateManager,
                            _monitor);
                        _rescueOperationEventRunning = false;
                    }

                    if (_firstTreatmentEventRunning)
                    {
                        _dialogueManager.TryAddDiagnosisCompleteTopic(_injuryManager.GetActiveInjury());
                        _firstTreatmentEventRunning = false;
                    }

                    _dialogueManager.ClearHarveyNeedsFirstTreatmentTopicIfObsolete("игровое событие завершено");
                }

                _eventWasActive = eventActive;

                if (Game1.CurrentEvent != null || Game1.eventUp)
                    return;

                if (_hospitalizationManager.IsHospitalized)
                    _hospitalizationManager.UpdateHospitalizationLock();

                // Домашние care-события: retry, если игрок уже дома
                if (e.IsMultipleOf(30) && LocationEventLauncher.IsFarmHouseLocation(Game1.currentLocation))
                    _homeCareLauncher?.TryTriggerHarveyHomeCareEvent(source: "UpdateRetry");

                // Каждые 6 тиков (~100 мс) — накопление использований инструмента
                if (e.IsMultipleOf(6))
                {
                    TrackToolUsage();
                }

                // Каждые 0.5 секунды — проверка взрывов
                if (e.IsMultipleOf(30))
                {
                    CheckExplosionInjuries();
                }

                // Каждую секунду — близость Харви, фермерские травмы, дождь/простуда
                if (e.IsMultipleOf(60))
                {
                    CheckHarveyProximity();
                    CheckFarmingInjuries();
                    CheckRainExposure();
                    _rehabManager.CheckRehabViolationOnHeavyWork();
                    _recoveryPlanManager.CheckRecoveryStaminaViolations((int)e.Ticks);
                }

                if (e.IsMultipleOf(120))
                {
                    CheckHealthBasedInjuries();
                    _recoveryPlanManager.CheckLowHealthViolation((int)e.Ticks);
                }

                // Каждые 10 секунд — окружающая среда (холод, алкоголь при лечении)
                if (e.IsMultipleOf(600))
                {
                    CheckEnvironmentalConditions();
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка в OnUpdateTicked: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Изменилось игровое время (обычно шаг 10 минут) — накопление экспозиции в шахте.
        /// </summary>
        public void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            try
            {
                HandleMineDirtyExposureTimeChanged(e.OldTime, e.NewTime);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка в PlayerEventHandler.OnTimeChanged: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Обработать логику локации
        /// </summary>
        private void HandleLocationLogic(GameLocation location)
        {
            if (string.Equals(location?.Name, _config.HospitalLocationName, StringComparison.OrdinalIgnoreCase))
                HandleHospitalLogic();

            _recoveryPlanManager.CheckViolationOnLocationEntry(location);

            if (IsMineOrVolcanoLocation(location))
            {
                HandleMinesLogic();
            }

            if (string.Equals(location?.Name, "BathHouse_Pool", StringComparison.OrdinalIgnoreCase))
                HandleSpaLogic();
        }

        private void HandleHospitalLogic()
        {
            bool hasMineInjuryTopic = Helpers.GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue);
            bool hasSeriousMainInjury = _injuryManager.IsMainInjurySerious();

            if (!_config.ForceHospitalization || !hasMineInjuryTopic || !hasSeriousMainInjury)
                return;

            if (_hospitalizationManager.IsHospitalized)
                return;

            var state = _stateManager.State;
            if (state.DischargedToday)
            {
                _monitor.Log("[Hospital] Пропуск повторной госпитализации — игрок уже выписан сегодня", LogLevel.Debug);
                return;
            }

            string? injury = _injuryManager.GetActiveInjuryOrPhaseByPriority() ?? "buffBadlyHurt";
            string caseId = HospitalizationManager.BuildCaseId(injury, "mine_rescue", GameUtils.Today());
            if (string.Equals(state.HospitalizationCompletedCaseId, caseId, StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log($"[Hospital] Пропуск — кейс {caseId} уже завершён", LogLevel.Debug);
                _dialogueManager.RemoveTopic(ConversationTopics.MineInjuryRescue);
                return;
            }

            _monitor.Log("⚠️ Игрок в госпитале с ранами после шахты → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ", LogLevel.Warn);

            NPC? harvey = HarveyHelper.GetHarvey();
            _hospitalizationManager.StartForcedHospitalizationWithExplanation(injury, harvey, "mine_rescue");
            _dialogueManager.RemoveTopic(ConversationTopics.MineInjuryRescue);
        }

        /// <summary>
        /// Логика шахты/вулкана: MineForbidden (жёсткий) vs RecoveryPlan (мягкий) vs MineRestricted.
        /// </summary>
        private void HandleMinesLogic()
        {
            if (Game1.CurrentEvent != null || Game1.eventUp)
                return;

            int today = Helpers.GameUtils.Today();
            var state = _stateManager.State;
            GameLocation location = Game1.currentLocation;
            bool isVolcano = IsVolcanoLocation(location);

            MineForbiddenHelper.ResetDailyRestrictionViolations(state, today);
            MineForbiddenHelper.SyncMineForbiddenBuff(
                state, _config, _buffManager, _stateManager, _monitor, today, "MineEntry");
            MineForbiddenHelper.SyncMineRestrictedBuff(
                state, _config, _injuryManager, _buffManager, _stateManager, _monitor, today, "MineEntry");

            bool hasMineForbidden = MineForbiddenHelper.IsMineForbiddenActive(state, _config, today);
            bool hardBlocked = MineForbiddenHelper.IsMineHardBlocked(
                state, _config, _injuryManager, _buffManager, today, out string? hardBlockReason);
            bool hasSevereInjury = MineForbiddenHelper.HasSevereMineCondition(
                state, _injuryManager, _buffManager, out _);

            if (hasMineForbidden)
                _careTrustManager.PenalizeMineViolationOncePerDay(hasSevereInjury);
            else if (hardBlocked)
                _careTrustManager.PenalizeMineViolationOncePerDay(true);

            if (state.NeedsMineRescueEvent)
                return;

            CheckNoMinePrescriptionViolation(today);
            _rehabManager.CheckRehabViolationOnMine();

            if (hasMineForbidden)
            {
                _mineEntryCoordinator.HandleMineEntryDuringMineForbidden(
                    location,
                    today,
                    isVolcano,
                    WarpOutOfForbiddenDungeon,
                    () => !isVolcano && TryStartMineInterceptionEvent());
                return;
            }

            if (hardBlocked)
            {
                HandleAcuteHardBlockMineEntry(today, location, isVolcano, hardBlockReason);
                return;
            }

            if (_mineEntryCoordinator.ShouldWarnRecoveryPlanMineEntry())
            {
                _mineEntryCoordinator.HandleMineEntryDuringRecoveryPlan(location, today, isVolcano);
                if (!_config.RecoveryPlanMineRuleBlocksEntry)
                {
                    ShowSoftMineWarningIfNeeded(today);
                    return;
                }

                WarpOutOfForbiddenDungeon(location);
                return;
            }

            var mode = MineForbiddenHelper.GetMineAccessMode(state, _config, _injuryManager, _buffManager, today);

            switch (mode)
            {
                case MineAccessMode.Restricted:
                    HandleMineRestrictedEntry(today);
                    return;
                default:
                    ShowSoftMineWarningIfNeeded(today);
                    break;
            }
        }

        private void ShowSoftMineWarningIfNeeded(int today)
        {
            if (!MineForbiddenHelper.ShouldShowSoftMineWarning(
                    _stateManager.State, _config, _injuryManager, _buffManager, today)
                || _lastMineSoftHudDay == today)
                return;

            _lastMineSoftHudDay = today;

            string text = MineForbiddenHelper.GetSoftMineWarningText(
                _stateManager.State, _injuryManager, _buffManager);

            Game1.addHUDMessage(new HUDMessage(text, HUDMessage.health_type));
            _monitor.Log("ℹ️ [Шахта] Мягкое предупреждение (MineWarningDay не ставится)", LogLevel.Debug);
        }

        /// <summary>
        /// Острое окно hard block без активного MineForbidden: строгое предупреждение и MineWarningDay,
        /// повторный вход в тот же день — вынос без катсцены перехвата.
        /// </summary>
        private void HandleAcuteHardBlockMineEntry(
            int today,
            GameLocation location,
            bool isVolcano,
            string? hardBlockReason)
        {
            var state = _stateManager.State;

            if (state.LastMineSevereWarningDay != today)
            {
                Game1.addHUDMessage(new HUDMessage(
                    MineForbiddenHelper.GetStrictMineWarningText(),
                    HUDMessage.error_type));
                state.LastMineSevereWarningDay = today;
                state.MineWarningDay = today;
                _stateManager.Save();
                _monitor.Log(
                    $"[MineHardBlock] Строгое предупреждение ({hardBlockReason ?? "acute"}) — MineWarningDay={today}",
                    LogLevel.Warn);
                return;
            }

            Game1.addHUDMessage(new HUDMessage(
                "Харви: Я уже предупреждал. Сегодня в шахту нельзя.",
                HUDMessage.error_type));
            state.LastMineSevereForcedExitDay = today;
            _stateManager.Save();
            _monitor.Log(
                $"[MineHardBlock] Повторный вход — вынос ({hardBlockReason ?? "acute"})",
                LogLevel.Warn);

            Game1.playSound("cancel");
            WarpOutOfForbiddenDungeon(location);
        }

        private void HandleActiveMineForbiddenBlock(int today)
        {
            GameLocation location = Game1.currentLocation;
            bool isVolcano = IsVolcanoLocation(location);

            _mineEntryCoordinator.HandleMineEntryDuringMineForbidden(
                location,
                today,
                isVolcano,
                WarpOutOfForbiddenDungeon,
                () => !isVolcano && TryStartMineInterceptionEvent());
        }

        /// <summary>Мягкое ограничение: предупреждение и риски, без выноса.</summary>
        private void HandleMineRestrictedEntry(int today)
        {
            var state = _stateManager.State;
            MineForbiddenHelper.ResetDailyRestrictionViolations(state, today);

            if (state.MineRestrictionViolationsToday == 0)
            {
                Game1.addHUDMessage(new HUDMessage(
                    _careTrustManager.GetMineWarningHudLine(severe: true, forbidden: false),
                    HUDMessage.health_type));
                state.MineRestrictionViolationsToday = 1;
                state.MineRestrictionStrikes++;
                _stateManager.Save();
                _monitor.Log(
                    $"[MineRestricted] Первый вход за день — предупреждение (strikes={state.MineRestrictionStrikes})",
                    LogLevel.Info);
                return;
            }

            _monitor.Log("[MineRestricted] Повторный вход в шахту за день — риск осложнений", LogLevel.Warn);
            TryApplyMineRestrictionViolationRisks(today);
        }

        private void TryApplyMineRestrictionViolationRisks(int today)
        {
            var state = _stateManager.State;

            if (MineForbiddenHelper.TryEscalateRestrictionToHardBan(
                    state, _config, _buffManager, _stateManager, _monitor, today, "restriction_strikes"))
            {
                HandleActiveMineForbiddenBlock(today);
                return;
            }

            string? mainId = _injuryManager.GetActiveInjury();
            if (!string.IsNullOrEmpty(mainId)
                && InjurySets.IsPainFlareEligibleMain(mainId)
                && !_complicationManager.HasComplication(InjuryBuffs.PainFlare)
                && Game1.random.NextDouble() < MineForbiddenHelper.RestrictedPainFlareChance)
            {
                _complicationManager.TryApplyComplication(
                    InjuryBuffs.PainFlare,
                    InjurySets.StormPainSensitive,
                    topicDays: 3,
                    new HUDMessage("Обострение боли в шахте!", HUDMessage.error_type));
                _monitor.Log("[MineRestricted] PainFlare от нарушения режима", LogLevel.Warn);
            }
            else if (Game1.random.NextDouble() < MineForbiddenHelper.RestrictedRepeatNeglectChance)
            {
                _complicationManager.TryApplyNeglectComplication(
                    mainId,
                    new HUDMessage("Харви заметил, что ты игнорируешь режим восстановления.", HUDMessage.error_type));
                state.MineRestrictionStrikes++;
                _stateManager.Save();
                _monitor.Log("[MineRestricted] Neglect от нарушения режима", LogLevel.Warn);
            }
        }

        private void TryApplyMineRestrictionLongStayRisk(int today, int exposureMinutes)
        {
            if (!MineForbiddenHelper.ShouldMineRestricted(
                    _stateManager.State, _config, _injuryManager, _buffManager, today))
                return;

            if (exposureMinutes < MineForbiddenHelper.RestrictedLongStayMinutes)
                return;

            if (_stateManager.State.MineRestrictionViolationsToday > 1)
                TryApplyMineRestrictionViolationRisks(today);
        }

        private bool TryStartMineInterceptionEvent()
        {
            if (!_dialogueManager.HasTopic(ConversationTopics.HarveyMineIntercept))
                _dialogueManager.AddTopic(ConversationTopics.HarveyMineIntercept, 3);

            return TryStartEventByName(
                EventIds.MineInterception,
                "Mine",
                OnMineInterceptionEventFinished);
        }

        private void OnMineInterceptionEventFinished()
        {
            try
            {
                ClearEventFadeAndControl();

                if (!_dialogueManager.HasTopic(ConversationTopics.HarveyMineIntercept))
                    _dialogueManager.AddTopic(ConversationTopics.HarveyMineIntercept, 3);

                WarpOutOfMineIfStillInside();
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MineForbidden] onEventFinished interception: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Прерывает зависшее перехват-событие, если игрок варпнулся не в Mine/Mountain (debug warp и т.п.).
        /// </summary>
        private bool TryAbortMineInterceptionOnExternalWarp(WarpedEventArgs e)
        {
            if (Game1.CurrentEvent == null && !Game1.eventUp)
                return false;

            string eventId = Game1.CurrentEvent?.id ?? string.Empty;
            if (!eventId.StartsWith(EventIds.MineInterception, StringComparison.OrdinalIgnoreCase))
                return false;

            if (IsMineLocation(e.NewLocation)
                || string.Equals(e.NewLocation?.NameOrUniqueName, "Mountain", StringComparison.OrdinalIgnoreCase))
                return false;

            ForceStopMineInterceptionEvent($"warp_to_{e.NewLocation?.NameOrUniqueName ?? "unknown"}");
            return true;
        }

        private void ForceStopMineInterceptionEvent(string reason)
        {
            _monitor.Log(
                $"[MineForbidden] Принудительная остановка {EventIds.MineInterception} ({reason})",
                LogLevel.Warn);

            if (!_dialogueManager.HasTopic(ConversationTopics.HarveyMineIntercept))
                _dialogueManager.AddTopic(ConversationTopics.HarveyMineIntercept, 3);

            if (Game1.currentLocation?.currentEvent != null)
                Game1.currentLocation.currentEvent = null;

            ClearEventFadeAndControl();
            WarpOutOfMineIfStillInside();
        }

        private static void ClearEventFadeAndControl()
        {
            Game1.fadeToBlackAlpha = 0f;
            Game1.eventUp = false;
            Game1.player.CanMove = true;
            Game1.player.completelyStopAnimatingOrDoingAction();
            Game1.player.showNotCarrying();
        }

        private void WarpOutOfMineIfStillInside()
        {
            GameLocation location = Game1.currentLocation;
            if (!IsInsideMineOrVolcano(location))
                return;

            _monitor.Log(
                "[MineForbidden] Событие завершено, игрок всё ещё в подземелье — warp наружу (CP без changeLocation)",
                LogLevel.Info);
            WarpOutOfForbiddenDungeon(location);
        }

        private bool IsVolcanoLocation(GameLocation? location)
        {
            return location is VolcanoDungeon
                || string.Equals(location?.NameOrUniqueName, "VolcanoDungeon", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsMineLocation(GameLocation? location)
        {
            return location is MineShaft
                || string.Equals(location?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase)
                || string.Equals(location?.NameOrUniqueName, "UndergroundMine", StringComparison.OrdinalIgnoreCase);
        }

        private void WarpOutOfForbiddenDungeon(GameLocation location)
        {
            if (IsVolcanoLocation(location))
            {
                // TODO: уточнить тайл у входа в вулкан на IslandNorth при тесте на Ginger Island.
                Game1.warpFarmer("IslandNorth", 31, 17, 2);
                return;
            }

            Game1.warpFarmer("Mountain", 53, 8, 2);
        }

        private bool IsInsideMineOrVolcano(GameLocation? location)
        {
            return IsMineLocation(location) || IsVolcanoLocation(location);
        }

        private bool TryStartEventByName(string eventId, string locationName, Action? onFinished = null)
        {
            try
            {
                var location = Game1.getLocationFromName(locationName);
                if (location == null)
                {
                    _monitor.Log($"[MineForbidden] Локация '{locationName}' не найдена", LogLevel.Warn);
                    return false;
                }

                var eventData = Game1.content.Load<Dictionary<string, string>>($"Data/Events/{locationName}");
                if (eventData == null)
                {
                    _monitor.Log($"[MineForbidden] Data/Events/{locationName} не найден", LogLevel.Warn);
                    return false;
                }

                string? eventScript = null;
                foreach (var kvp in eventData)
                {
                    if (kvp.Key.StartsWith(eventId, StringComparison.OrdinalIgnoreCase))
                    {
                        eventScript = kvp.Value;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(eventScript))
                {
                    _monitor.Log($"[MineForbidden] Событие '{eventId}' не найдено в Data/Events/{locationName}", LogLevel.Warn);
                    return false;
                }

                var gameEvent = new Event(eventScript);
                if (onFinished != null)
                    gameEvent.onEventFinished += onFinished;

                GameLocation startLocation = Game1.currentLocation;
                bool onRequestedLocation =
                    string.Equals(locationName, "Mine", StringComparison.OrdinalIgnoreCase) && IsMineLocation(startLocation)
                    || string.Equals(startLocation?.NameOrUniqueName, locationName, StringComparison.OrdinalIgnoreCase);
                if (!onRequestedLocation)
                    startLocation = location;

                startLocation.startEvent(gameEvent);
                _monitor.Log($"[MineForbidden] Запущено событие '{eventId}'", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MineForbidden] Ошибка запуска события '{eventId}': {ex}", LogLevel.Error);
                return false;
            }
        }

        private static bool IsEventOrMenuBlocking()
        {
            return Game1.CurrentEvent != null
                || Game1.eventUp
                || Game1.activeClickableMenu != null;
        }

        private static bool IsMineOrVolcanoLocation(GameLocation? location)
        {
            if (location == null)
                return false;

            return location is MineShaft
                || location is VolcanoDungeon
                || string.Equals(location.Name, "Mine", StringComparison.OrdinalIgnoreCase);
        }

        private void HandleMineDirtyExposureTimeChanged(int oldTime, int newTime)
        {
            if (!IsMineOrVolcano(Game1.currentLocation))
            {
                ResetMineDirtyRollTimerIfNeeded();
                return;
            }

            int today = Helpers.GameUtils.Today();
            if (_stateManager.State.LastMineDirtyExposureDay != today)
            {
                _stateManager.State.LastMineDirtyExposureDay = today;
                _stateManager.State.MineDirtyExposureMinutesToday = 0;
                _stateManager.State.LastMineDirtyWoundRollMinute = -1;
                _stateManager.State.MineDirtyRiskBoostUntilMinute = -1;
            }

            if (!HasDirtyMineInjury())
                return;

            if (_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound))
                return;

            int oldMinutes = ToGameMinutes(oldTime);
            int newMinutes = ToGameMinutes(newTime);
            int delta = newMinutes - oldMinutes;

            if (delta <= 0)
                return;

            _stateManager.State.MineDirtyExposureMinutesToday += delta;

            int currentMinute = newMinutes;
            int interval = Math.Max(10, _config.DirtyWoundMineRollIntervalMinutes);

            if (_stateManager.State.LastMineDirtyWoundRollMinute >= 0 &&
                currentMinute - _stateManager.State.LastMineDirtyWoundRollMinute < interval)
            {
                return;
            }

            int exposure = _stateManager.State.MineDirtyExposureMinutesToday;
            double chance = CalculateDirtyWoundChance(exposure, currentMinute);

            if (chance <= 0)
                return;

            _stateManager.State.LastMineDirtyWoundRollMinute = currentMinute;
            TryApplyDirtyWoundFromMine(chance, $"exposure={exposure}m");
            TryApplyMineRestrictionLongStayRisk(today, exposure);
        }

        private void ResetMineDirtyRollTimerIfNeeded()
        {
            if (_stateManager.State.LastMineDirtyWoundRollMinute >= 0)
                _stateManager.State.LastMineDirtyWoundRollMinute = -1;
        }

        private static bool IsMineOrVolcano(GameLocation? location)
        {
            return location is MineShaft || location is VolcanoDungeon;
        }

        private bool HasDirtyMineInjury() => _complicationManager.CanReceiveMineDirtyWound();

        private static int ToGameMinutes(int timeOfDay)
        {
            return (timeOfDay / 100) * 60 + (timeOfDay % 100);
        }

        private double CalculateDirtyWoundChance(int exposureMinutes, int currentMinute)
        {
            int safeMinutes = Math.Max(0, _config.DirtyWoundSafeMineMinutes);
            int highMinutes = Math.Max(safeMinutes + 10, _config.DirtyWoundHighMineMinutes);

            double chance;
            if (exposureMinutes < safeMinutes)
                chance = 0.0;
            else if (exposureMinutes < highMinutes)
                chance = Math.Clamp(_config.DirtyWoundChanceMinesMedium, 0.0, 1.0);
            else
                chance = Math.Clamp(_config.DirtyWoundChanceMines, 0.0, 1.0);

            if (_stateManager.State.MineDirtyRiskBoostUntilMinute >= currentMinute)
                chance += Math.Clamp(_config.DirtyWoundMineDamageBonusChance, 0.0, 1.0);

            if (_buffManager.HasBuff(InjuryBuffs.MineRestricted))
                chance *= MineForbiddenHelper.GetRestrictedDirtyChanceMultiplier();

            return Math.Clamp(chance, 0.0, 0.95);
        }

        /// <summary>
        /// QA/MCP: simulate mine exposure minutes and roll dirty wound (ignores location if ignoreLocation=true).
        /// </summary>
        public string SimulateMineDirtyExposureForQa(int minutes, bool forceRoll = false, bool ignoreLocation = true)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (!ignoreLocation && !IsMineOrVolcano(Game1.currentLocation))
                return "SKIP: not in Mine/Volcano";

            if (!HasDirtyMineInjury())
                return "SKIP: hasDirtyInjury=false";

            if (_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound))
                return "SKIP: DirtyWound already active";

            int today = Helpers.GameUtils.Today();
            if (_stateManager.State.LastMineDirtyExposureDay != today)
            {
                _stateManager.State.LastMineDirtyExposureDay = today;
                _stateManager.State.MineDirtyExposureMinutesToday = 0;
                _stateManager.State.LastMineDirtyWoundRollMinute = -1;
                _stateManager.State.MineDirtyRiskBoostUntilMinute = -1;
            }

            int delta = Math.Max(1, minutes);
            _stateManager.State.MineDirtyExposureMinutesToday += delta;

            int currentMinute = ToGameMinutes(Game1.timeOfDay);
            double chance = forceRoll
                ? 1.0
                : CalculateDirtyWoundChance(
                    _stateManager.State.MineDirtyExposureMinutesToday,
                    currentMinute);

            if (chance <= 0)
            {
                return
                    $"exposure={_stateManager.State.MineDirtyExposureMinutesToday}m chance=0% applied=no (below safe threshold)";
            }

            _stateManager.State.LastMineDirtyWoundRollMinute = currentMinute;
            bool applied = _complicationManager.TryApplyDirtyWoundFromMine(
                chance,
                $"qa simulate exposure={_stateManager.State.MineDirtyExposureMinutesToday}m");

            _stateManager.Save();

            return
                $"exposure={_stateManager.State.MineDirtyExposureMinutesToday}m chance={chance:P0} applied={applied}";
        }

        /// <summary>
        /// QA/MCP: simulate severe mine entry warning (sets MineWarningDay without SMAPI warp).
        /// </summary>
        public string SimulateMineSevereWarningForQa(bool warningWasYesterday = false)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            int today = Helpers.GameUtils.Today();
            var state = _stateManager.State;

            if (!_injuryManager.IsMainInjurySerious()
                && !MineForbiddenHelper.IsMineHardBlocked(
                    state, _config, _injuryManager, _buffManager, today, out _))
                return "SKIP: no hard-block mine condition";

            if (state.LastMineSevereWarningDay == today && !warningWasYesterday)
            {
                return
                    $"SKIP: warning already set today " +
                    $"MineWarningDay={state.MineWarningDay} " +
                    $"LastMineSevereWarningDay={state.LastMineSevereWarningDay}";
            }

            state.LastMineSevereWarningDay = today;
            state.MineWarningDay = warningWasYesterday ? today - 1 : today;
            _stateManager.Save();

            return
                $"MineWarningDay={state.MineWarningDay} " +
                $"LastMineSevereWarningDay={state.LastMineSevereWarningDay} " +
                $"warningWasYesterday={warningWasYesterday}";
        }

        /// <summary>
        /// QA/MCP: run HandleLocationLogic for current location (hospital admission, mine warning, spa).
        /// </summary>
        public string RunLocationLogicForQa()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (Game1.CurrentEvent != null || Game1.eventUp)
                return "SKIP: event active";

            HandleLocationLogic(Game1.currentLocation);
            var state = _stateManager.State;
            return
                $"location={Game1.currentLocation?.Name ?? "(none)"} " +
                $"IsHospitalized={state.IsHospitalized} " +
                $"HospitalizedInjuryId={state.HospitalizedInjuryId ?? "(none)"} " +
                $"MineWarningDay={state.MineWarningDay} " +
                $"MineForbidden={_buffManager.HasBuff(InjuryBuffs.MineForbidden)}";
        }

        /// <summary>
        /// QA/MCP: apply WetBandage from rain exposure counters without waiting for UpdateTick.
        /// </summary>
        public string SimulateRainWetBandageForQa(bool force = true)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (!_complicationManager.CanReceiveWetBandageFromWater())
                return "SKIP: CanReceiveWetBandageFromWater=false";

            if (_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.WetBandage))
                return "SKIP: WetBandage already active";

            int today = Helpers.GameUtils.Today();
            _stateManager.State.LastRainDay = today;
            if (_stateManager.State.TimeUnderRainTicks <= 0)
                _stateManager.State.TimeUnderRainTicks = 60;

            if (!force)
            {
                double wetChance = CalculateWetChance(_stateManager.State.TimeUnderRainTicks);
                if (!Helpers.GameUtils.Roll(wetChance))
                {
                    return
                        $"ticks={_stateManager.State.TimeUnderRainTicks}s chance={wetChance:P0} applied=no";
                }
            }

            bool applied = _complicationManager.TryApplyWetBandageFromWater(
                topicDays: 4,
                new HUDMessage("Повязка промокла!", HUDMessage.error_type),
                $"qa rain wet simulate ticks={_stateManager.State.TimeUnderRainTicks}s force={force}");

            if (applied)
            {
                _complianceManager.AddCompliance(-1, "wet_bandage");
                _stateManager.State.TimeUnderRainTicks = 0;
            }

            _stateManager.Save();
            return $"ticks={_stateManager.State.TimeUnderRainTicks}s force={force} applied={applied}";
        }

        private void TryApplyDirtyWoundFromMine(double chance, string reason)
        {
            if (!_complicationManager.TryApplyDirtyWoundFromMine(chance, reason))
                return;

            var harvey = HarveyHelper.FindHarveyInLocation(Game1.currentLocation);
            if (harvey != null)
            {
                var injuries = _injuryManager.CollectAllInjuries();
                var plan = _harveyReactionManager.DetermineProximityReaction(harvey, injuries);
                if (plan != null)
                {
                    string text = _harveyReactionManager.ResolveReactionText(plan);
                    _dialogueManager.ShowEmoteWithText(harvey, plan.Emote, text);
                }
            }
        }

        private void CheckRainExposure()
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (Game1.timeOfDay >= 2600)
                return;

            var location = Game1.player?.currentLocation;
            if (location == null)
                return;

            bool isOutsideInRain = location.IsOutdoors && (Game1.isRaining || Game1.isLightning);

            if (isOutsideInRain)
            {
                HandleRainLogic();

                bool isOutsideInStorm = location.IsOutdoors && Game1.isLightning;
                _complicationManager.TryApplyStormPainFlareIfEligible(isOutsideInStorm);
            }
            else
            {
                ResetRainExposureIfNeeded();
            }
        }

        private void ResetRainExposureIfNeeded()
        {
            // Сбрасываем только непрерывное промокание повязки.
            // TotalTimeUnderRainToday НЕ сбрасывать здесь: это накопительный счётчик за день.
            if (_stateManager.State.TimeUnderRainTicks != 0)
                _stateManager.State.TimeUnderRainTicks = 0;
        }

        private void HandleRainLogic()
        {
            int currentDay = Helpers.GameUtils.Today();
            
            // Сбрасываем счётчики в новый день
            if (_stateManager.State.LastRainDay != currentDay)
            {
                _stateManager.State.TotalTimeUnderRainToday = 0;
                _stateManager.State.TimeUnderRainTicks = 0;
                _stateManager.State.LastRainDay = currentDay;
            }
            
            // ⭐ ОБЩИЙ СЧЁТЧИК ВРЕМЕНИ ПОД ДОЖДЁМ (для простуды), секунды за текущий день
            // HandleRainLogic вызывается раз в секунду из OnUpdateTicked, поэтому +1 = 1 секунда.
            _stateManager.State.TotalTimeUnderRainToday++;

            int totalSecondsToday = _stateManager.State.TotalTimeUnderRainToday;

            if (IsColdRiskThreshold(totalSecondsToday))
            {
                CheckColdRisk(totalSecondsToday);
            }
            
            bool keepDryActive = _prescriptionManager.HasActivePrescription(PrescriptionIds.KeepDry);
            bool bandageLogic = CanGetWetBandage();

            if (!keepDryActive && !bandageLogic)
            {
                _stateManager.State.TimeUnderRainTicks = 0;
                return;
            }

            // HandleRainLogic вызывается раз в секунду из OnUpdateTicked, поэтому +1 = 1 секунда.
            _stateManager.State.TimeUnderRainTicks++;

            int secondsUnderRain = _stateManager.State.TimeUnderRainTicks;

            if (keepDryActive)
                CheckKeepDryRainPrescriptionViolation(secondsUnderRain);

            if (keepDryActive || bandageLogic)
                _recoveryPlanManager.CheckRainBandageViolation(secondsUnderRain);

            // === ПРОМОКАНИЕ ПОВЯЗКИ ===
            if (!bandageLogic)
                return;

            // Проверка промокания раз в 10 секунд, чтобы повязка не роллилась слишком часто
            if (secondsUnderRain % 10 == 0)
            {
                double wetChance = CalculateWetChance(secondsUnderRain);

                if (Helpers.GameUtils.Roll(wetChance))
                {
                    ApplyWetBandageComplication(secondsUnderRain);
                }
            }
        }

        private void ApplyWetBandageComplication(int secondsUnderRain)
        {
            if (_complicationManager.TryApplyWetBandageFromWater(
                    topicDays: 4,
                    new HUDMessage("Повязка промокла!", HUDMessage.error_type),
                    $"Повязка промокла после {secondsUnderRain}с под дождём"))
            {
                _complianceManager.AddCompliance(-1, "wet_bandage");
                _stateManager.State.TimeUnderRainTicks = 0;
            }
        }
        
        private static bool IsColdRiskThreshold(int totalSecondsToday)
        {
            return totalSecondsToday == 300
                || totalSecondsToday == 600
                || totalSecondsToday == 900
                || totalSecondsToday == 1200;
        }

        /// <summary>
        /// Проверить риск простуды от долгого пребывания под дождём
        /// </summary>
        private void CheckColdRisk(int totalSecondsToday)
        {
            // Если уже простужен - не проверяем
            if (_buffManager.HasBuff(InjuryBuffs.Cold)) return;
            
            // Пороги (в секундах):
            // 5 минут (300с): 5% риск
            // 10 минут (600с): 20% риск
            // 15 минут (900с): 50% риск
            // 20+ минут: 80% риск
            
            double coldChance = totalSecondsToday switch
            {
                < 300 => 0.0,    // < 5 мин: безопасно
                < 600 => 0.05,   // 5-10 мин: 5%
                < 900 => 0.20,   // 10-15 мин: 20%
                < 1200 => 0.50,  // 15-20 мин: 50%
                _ => 0.80        // 20+ мин: 80%
            };
            
            if (coldChance > 0 && Helpers.GameUtils.Roll(coldChance))
            {
                _monitor.Log($"🤧 Игрок простудился после {totalSecondsToday / 60} минут под дождём (шанс {coldChance:P0})", LogLevel.Warn);
                
                _injuryManager.ApplyColdSafe();
                Game1.addHUDMessage(new HUDMessage("Ты простудился! Нужно к врачу...", HUDMessage.error_type));
            }
        }

        private bool HasActiveTreatmentBandage() =>
            _complicationManager.HasActiveBandageOrWoundDressing();

        private bool CanGetWetBandage() =>
            _complicationManager.CanReceiveWetBandageFromWater();

        /// <summary>
        /// Вычислить вероятность промокания повязки в зависимости от времени под дождем
        /// </summary>
        private double CalculateWetChance(int secondsUnderRain)
        {
            // Нелинейная функция: медленный рост в начале, быстрый к концу
            if (secondsUnderRain < 10) return 0.02;   // < 10с: 2%
            if (secondsUnderRain < 30) return 0.05;   // 10-30с: 5%
            if (secondsUnderRain < 60) return 0.15;   // 30-60с: 15%
            if (secondsUnderRain < 90) return 0.35;   // 60-90с: 35%
            if (secondsUnderRain < 120) return 0.60;  // 90-120с: 60%
            if (secondsUnderRain < 180) return 0.85;  // 120-180с: 85%
            return 0.98; // 180+с: почти гарантированно
        }

        private void CheckNoMinePrescriptionViolation(int today)
        {
            if (!_prescriptionManager.HasActivePrescription(PrescriptionIds.NoMine))
                return;

            if (!_prescriptionManager.TryMarkViolation(PrescriptionIds.NoMine, "mine", out int count))
                return;

            string hud = count switch
            {
                1 => "Харви просил тебя не ходить в шахту с этой травмой...",
                2 => "Харви уже предупреждал: шахта при такой травме — серьёзное нарушение.",
                _ => "Снова нарушение режима: шахта при такой травме недопустима."
            };
            Game1.addHUDMessage(new HUDMessage(hud, count >= 2 ? HUDMessage.error_type : HUDMessage.health_type));
            _monitor.Log($"[Prescription] NoMine violation #{count}", LogLevel.Info);

            if (count < 2)
                return;

            if (!_injuryManager.IsMainInjurySerious())
                return;

            if (!HasDirtyWoundEligibleInjury())
                return;

            if (_buffManager.HasBuff(InjuryBuffs.DirtyWound)
                || _stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound))
                return;

            double chance = Math.Clamp(PrescriptionDirtyWoundBaseChance + PrescriptionDirtyWoundBonusChance, 0.0, 0.95);
            TryApplyDirtyWoundFromMine(chance, $"prescription_no_mine viol={count}");
        }

        private void CheckKeepDryRainPrescriptionViolation(int secondsUnderRain)
        {
            if (secondsUnderRain < KeepDryRainViolationSeconds)
                return;

            if (!_prescriptionManager.TryMarkViolation(PrescriptionIds.KeepDry, "rain", out int count))
                return;

            string hud = count switch
            {
                1 => "Повязка намокает. Харви предупреждал держать раны сухими.",
                _ => "Харви предупреждал держать раны сухими. Ты снова под дождём."
            };
            Game1.addHUDMessage(new HUDMessage(hud, count >= 2 ? HUDMessage.error_type : HUDMessage.health_type));
            _monitor.Log($"[Prescription] KeepDry rain violation #{count} after {secondsUnderRain}s", LogLevel.Info);

            TryRollPrescriptionWetComplication(PrescriptionWetBandageBonusChance, PrescriptionWetStitchesBonusChance);
        }

        private void CheckLightWorkPrescriptionViolation()
        {
            if (!_prescriptionManager.HasActivePrescription(PrescriptionIds.LightWork))
            {
                _lightWorkLowStaminaToolSeconds = 0;
                return;
            }

            if (!IsLightWorkHeavyToolInUse())
            {
                _lightWorkLowStaminaToolSeconds = 0;
                return;
            }

            float staminaThreshold = AtLeastZero(_config.TornMusclesStaminaThreshold);
            if (Game1.player.Stamina > staminaThreshold)
            {
                _lightWorkLowStaminaToolSeconds = 0;
                return;
            }

            _lightWorkLowStaminaToolSeconds++;

            if (_lightWorkLowStaminaToolSeconds < LightWorkViolationSeconds)
                return;

            if (!_prescriptionManager.TryMarkViolation(PrescriptionIds.LightWork, "heavy_work", out int count))
            {
                _lightWorkLowStaminaToolSeconds = 0;
                return;
            }

            string hud = count switch
            {
                1 => "Спина/мышцы отзываются болью. Похоже, ты перегрузилась...",
                _ => "Харви просил не перегружаться. Ты снова работаешь через боль."
            };
            Game1.addHUDMessage(new HUDMessage(hud, count >= 2 ? HUDMessage.error_type : HUDMessage.health_type));
            _lightWorkLowStaminaToolSeconds = 0;
            _monitor.Log($"[Prescription] LightWork violation #{count}", LogLevel.Info);
        }

        private bool IsLightWorkHeavyToolInUse()
        {
            if (Game1.player?.UsingTool != true)
                return false;

            return GetCurrentToolKey() switch
            {
                "Axe" or "Pickaxe" or "Hoe" or "WateringCan" => true,
                _ => false,
            };
        }

        private bool HasDirtyWoundEligibleInjury() => _complicationManager.CanReceiveMineDirtyWound();

        private bool HasKeepDryWoundExposure()
        {
            if (HasActiveTreatmentBandage())
                return true;

            string? mainInjuryId = _complicationManager.GetActiveMainInjuryId();
            return !string.IsNullOrEmpty(mainInjuryId)
                && InjurySets.BandageSensitive.Contains(mainInjuryId)
                && _injuryManager.HasInjuryOrPhase(mainInjuryId);
        }

        private bool HasWetStitchesExposure()
        {
            string? mainInjuryId = _complicationManager.GetActiveMainInjuryId();
            if (string.IsNullOrEmpty(mainInjuryId))
                return false;

            if (string.Equals(mainInjuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
                return _injuryManager.HasInjuryOrPhase(mainInjuryId);

            if (!InjurySets.BandageSensitive.Contains(mainInjuryId))
                return false;

            return _injuryManager.HasInjuryOrPhase(mainInjuryId)
                && (mainInjuryId is "buffDeepCuts" or "buffBurnWounds"
                    || _buffManager.HasBuff(_injuryManager.GetPhaseBuffId(mainInjuryId, 1))
                    || _buffManager.HasBuff(_injuryManager.GetPhaseBuffId(mainInjuryId, 2)));
        }

        private void TryRollPrescriptionWetComplication(double wetBandageBonus, double wetStitchesBonus)
        {
            if (HasWetStitchesExposure()
                && Helpers.GameUtils.Roll(Math.Clamp(wetStitchesBonus, 0.0, 1.0)))
            {
                TryApplyWetStitchesComplication("[Prescription] WetStitches после нарушения KeepDry");
                return;
            }

            if (HasKeepDryWoundExposure()
                && !HasWetStitchesExposure()
                && Helpers.GameUtils.Roll(Math.Clamp(wetBandageBonus, 0.0, 1.0)))
            {
                _complicationManager.TryApplyWetBandageFromWater(
                    topicDays: 4,
                    new HUDMessage("Повязка промокла!", HUDMessage.error_type),
                    "[Prescription] WetBandage после нарушения KeepDry");
            }
        }

        private bool TryApplyWetStitchesComplication(string logContext)
        {
            if (_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.WetStitches))
                return false;

            int today = Helpers.GameUtils.Today();
            _buffManager.AddBuff(InjuryBuffs.WetStitches, -2);
            _stateManager.State.ActiveComplications[InjuryBuffs.WetStitches] = today;
            _stateManager.CreateComplicationState(InjuryBuffs.WetStitches, today);
            _dialogueManager.AddTopic(ConversationTopics.WetStitches, 4);
            Game1.addHUDMessage(new HUDMessage("Швы намокли! Нельзя было купаться со швами!", HUDMessage.error_type));
            _monitor.Log(logContext, LogLevel.Warn);
            return true;
        }

        private void HandleSpaLogic()
        {
            if (_prescriptionManager.HasActivePrescription(PrescriptionIds.KeepDry))
            {
                if (_prescriptionManager.TryMarkViolation(PrescriptionIds.KeepDry, "pool", out int keepDryCount))
                {
                    string hud = keepDryCount switch
                    {
                        1 => "Харви просил держать раны сухими — бассейн был плохой идеей.",
                        _ => "Ты снова намочила раны, хотя Харви запретил. Он будет строг."
                    };
                    Game1.addHUDMessage(new HUDMessage(hud, keepDryCount >= 2 ? HUDMessage.error_type : HUDMessage.health_type));
                    TryRollPrescriptionWetComplication(PrescriptionWetBandageBonusChance, PrescriptionWetStitchesBonusChance);
                    _monitor.Log($"[Prescription] KeepDry pool violation #{keepDryCount}", LogLevel.Info);
                }
            }

            if (HasWetStitchesExposure())
            {
                if (TryApplyWetStitchesComplication("Швы намокли при купании"))
                    _complianceManager.AddCompliance(-1, "wet_stitches_spa");
            }
            else if (CanGetWetBandage())
            {
                if (_complicationManager.TryApplyWetBandageFromWater(
                        topicDays: 4,
                        new HUDMessage("Повязка промокла! Нельзя было купаться с повязкой!", HUDMessage.error_type),
                        "Повязка промокла при купании"))
                {
                    _complianceManager.AddCompliance(-1, "wet_bandage_spa");
                }
            }
        }

        /// <summary>
        /// Близость Харви: принудительная госпитализация (mine rescue) или одно короткое облачко.
        /// Антиспам: не чаще 1× за локацию и не чаще 1× за 2 игровых часа (проверка ~1 раз/сек, показ — нет).
        /// Облачко: только ShowEmoteWithText / showTextAboveHead — без DialogueBox и без блокировки движения.
        /// </summary>
        private void CheckHarveyProximity()
        {
            if (!Context.IsPlayerFree) return;

            bool canForcedHosp = _config.ForceHospitalization
                && Helpers.GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue)
                && _injuryManager.IsMainInjurySerious();
            bool hasAnyInState = _stateManager.State.ActiveDebuffs.Count > 0
                || _stateManager.State.ActiveComplications.Count > 0;

            if (!canForcedHosp && !hasAnyInState)
                return;

            var harvey = HarveyHelper.FindHarveyInLocation(Game1.currentLocation);
            if (harvey == null) return;

            float distance = Microsoft.Xna.Framework.Vector2.Distance(
                Game1.player.Position,
                harvey.Position
            ) / Game1.tileSize;

            if (distance > _config.ProximityTiles)
                return;

            // topicMineInjuryRescue + Severe: предупреждение, затем госпитализация (не обычное облачко).
            if (canForcedHosp)
            {
                if (!_hospitalizationManager.IsHospitalized)
                {
                    int today = Helpers.GameUtils.Today();
                    var injuryState = _stateManager.State;
                    bool warningShownToday = injuryState.PendingForcedHospitalizationWarning
                        && injuryState.PendingForcedHospitalizationWarningDay == today;

                    if (!warningShownToday)
                    {
                        _monitor.Log("⚠️ Харви: proximity-предупреждение перед госпитализацией", LogLevel.Warn);
                        _dialogueManager.ShowEmoteWithText(
                            harvey,
                            HarveyEmotes.ForcedHospitalization,
                            "Стой. Я вижу, как ты держишься. В клинику. Сейчас.");
                        injuryState.PendingForcedHospitalizationWarning = true;
                        injuryState.PendingForcedHospitalizationWarningDay = today;
                        _stateManager.Save();
                        _proximityReactionShown = true;
                        return;
                    }

                    _monitor.Log($"⚠️ Харви обнаружил раны после обморока в шахте → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ", LogLevel.Warn);
                    string? injury = _injuryManager.GetActiveInjuryOrPhaseByPriority() ?? "buffBadlyHurt";
                    _hospitalizationManager.StartForcedHospitalizationWithExplanation(injury, harvey, "mine_rescue");
                    _dialogueManager.RemoveTopic(ConversationTopics.MineInjuryRescue);
                }
                _proximityReactionShown = true;
                return;
            }

            var injuries = _injuryManager.CollectAllInjuries();
            if (!injuries.HasAny)
                return;

            if (!CanShowNormalProximityReaction())
                return;

            _monitor.Log(
                $"[Proximity] Показ облачка: локация={_lastLocationName}, дистанция={distance:F1} клеток",
                LogLevel.Debug);
            ShowProximityDiscovery(harvey, injuries);
            _proximityReactionShown = true;
        }

        /// <summary>
        /// Можно ли показать обычное proximity-облачко (не госпитализация).
        /// </summary>
        private bool CanShowNormalProximityReaction()
        {
            if (_proximityReactionShown)
            {
                _monitor.Log("[Proximity] Пропуск: уже показано в этой локации", LogLevel.Debug);
                return false;
            }

            int elapsed = _harveyReactionManager.GetProximityCooldownElapsedMinutes();
            if (elapsed < ProximityReactionCooldownMinutes)
            {
                _monitor.Log(
                    $"[Proximity] Пропуск: кулдаун {elapsed}/{ProximityReactionCooldownMinutes} игр. мин",
                    LogLevel.Debug);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Короткое облачко при проходе мимо (эмоция + текст над головой, без диалогового окна).
        /// </summary>
        private void ShowProximityDiscovery(NPC harvey, Core.Models.InjuryCollection injuries)
        {
            var plan = _harveyReactionManager.DetermineProximityReaction(harvey, injuries);
            if (plan == null)
                return;

            if (plan.IsStrict && !_harveyReactionManager.CanShowStrictReactionToday())
            {
                _monitor.Log("[Proximity] Пропуск: строгая реакция уже была сегодня", LogLevel.Debug);
                return;
            }

            string textMessage = _harveyReactionManager.ResolveReactionText(plan);
            _dialogueManager.ShowEmoteWithText(harvey, plan.Emote, textMessage);

            if (!string.IsNullOrEmpty(plan.TopicId))
                _dialogueManager.AddTopic(plan.TopicId, plan.TopicDays);

            _harveyReactionManager.RecordReactionShown(plan);

            _monitor.Log(
                $"[Proximity] Облачко: emote={plan.Emote}, context={plan.Context}, tone={plan.Tone}, " +
                $"strict={plan.IsStrict}, reason={plan.Reason}, text='{textMessage}'",
                LogLevel.Debug);
        }

        /// <summary>
        /// Проверить травмы на основе здоровья
        /// </summary>
        private void CheckHealthBasedInjuries()
        {
            int currentHealth = Game1.player.health;
            int maxHealth = Game1.player.maxHealth;
            int lastHealth = _stateManager.State.LastHealth;

            // Проверяем только если здоровье уменьшилось
            if (currentHealth >= lastHealth)
            {
                _stateManager.State.LastHealth = currentHealth;
                return;
            }

            int damage = lastHealth - currentHealth;
            _stateManager.State.LastHealth = currentHealth;

            if (damage > 0)
            {
                if (IsMineOrVolcano(Game1.currentLocation) && HasDirtyMineInjury())
                {
                    int currentMinute = Helpers.GameUtils.CurrentTimeInMinutes();
                    int boostMinutes = Math.Max(10, _config.DirtyWoundMineDamageBoostMinutes);
                    _stateManager.State.MineDirtyRiskBoostUntilMinute = currentMinute + boostMinutes;

                    _monitor.Log(
                        $"[Шахта] Получен урон с открытой/опасной раной: риск загрязнения повышен до минуты {_stateManager.State.MineDirtyRiskBoostUntilMinute}",
                        LogLevel.Debug
                    );
                }

                _monitor.Log(
                    $"[DamageInjury] Health drop: old={lastHealth}, current={currentHealth}, damage={damage}, {FormatInjuryDiagnosticContext()}",
                    LogLevel.Debug);

                ProcessDamageBasedInjuries(damage, lastHealth, currentHealth);
            }
        }

        // Счётчики использования инструментов и cooldown roll-а фермерских травм
        private int _lastToolUseTick = -1;
        private string _lastToolUseKey = "";
        private Dictionary<string, int> _toolUseCounters = new(StringComparer.OrdinalIgnoreCase);
        private int _lastFarmingInjuryRollTime = -1;

        /// <summary>Секунды непрерывного пребывания на улице в холодных условиях (без защиты).</summary>
        private int _coldExposureSeconds = 0;

        /// <summary>Шаг накопления: CheckEnvironmentalConditions() вызывается раз в 600 SMAPI-тиков (~10 секунд).</summary>
        private const int ENVIRONMENT_CHECK_INTERVAL_SECONDS = 10;

        /// <summary>5 минут на улице в холоде → debuff buffTooCold.</summary>
        private const int COLD_EXPOSURE_THRESHOLD_SECONDS = 300;

        /// <summary>
        /// Обработать травмы на основе полученного урона
        /// ПРИОРИТЕТ: От серьёзных к лёгким!
        /// </summary>
        private void ProcessDamageBasedInjuries(int damage, int oldHealth, int newHealth)
        {
            bool fromMonster = IsNearHostileMonster();
            int combatSkill = Game1.player.CombatLevel;
            string ctx = FormatInjuryDiagnosticContext();

            _monitor.Log(
                $"[DamageInjury] Evaluate damage={damage}, health {oldHealth}->{newHealth}, fromMonster={fromMonster}, combatSkill={combatSkill}, {ctx}",
                LogLevel.Debug);

            // 1. КРИТИЧЕСКОЕ ЗДОРОВЬЕ (всегда приоритет!)
            if (Game1.player.health <= 10)
            {
                _monitor.Log(
                    $"[DamageInjury] APPLY buffBadlyHurt: health<=10 after damage, {ctx}",
                    LogLevel.Warn);
                _injuryManager.ApplyBadlyHurtSafe();
                return;
            }

            // 2. ТЯЖЁЛЫЕ ТРАВМЫ (большой урон)
            // Fractured Bone (30+ урона, 10% шанс)
            if (damage >= 30 && TryRollCombatInjury(0.10, combatSkill, fromMonster, damage, "FracturedBone"))
            {
                _monitor.Log(
                    $"[DamageInjury] APPLY buffFracturedBone: damage>={damage}>=30, roll ok, {ctx}",
                    LogLevel.Warn);
                _injuryManager.ApplyFracturedBoneSafe();
                return;
            }

            // Concussion (20+ урона, 25% шанс)
            if (damage >= 20 && TryRollCombatInjury(0.25, combatSkill, fromMonster, damage, "Concussion"))
            {
                _monitor.Log(
                    $"[DamageInjury] APPLY buffConcussion: damage>={damage}>=20, roll ok, {ctx}",
                    LogLevel.Warn);
                _injuryManager.ApplyConcussionSafe();
                return;
            }

            // 3. СРЕДНИЕ ТРАВМЫ
            // Bruised Ribs (15+ урона, 25% шанс)
            if (damage >= 15 && TryRollCombatInjury(0.25, combatSkill, fromMonster, damage, "BruisedRibs"))
            {
                _monitor.Log(
                    $"[DamageInjury] APPLY buffBruisedRibs: damage>={damage}>=15, roll ok, {ctx}",
                    LogLevel.Warn);
                _injuryManager.ApplyBruisedRibsSafe();
                return;
            }

            // Deep Cuts (10+ урона, 30% шанс)
            if (damage >= 10)
            {
                if (TryRollCombatInjury(0.30, combatSkill, fromMonster, damage, "DeepCuts"))
                {
                    _monitor.Log(
                        $"[DamageInjury] APPLY buffDeepCuts (combat): damage>={damage}>=10, roll ok, fromMonster={fromMonster}, {ctx}",
                        LogLevel.Warn);
                    _injuryManager.ApplyDeepCutsSafe("combat");
                    return;
                }

                _monitor.Log(
                    $"[DamageInjury] SKIP buffDeepCuts (combat): damage>={damage}>=10, roll failed, fromMonster={fromMonster}, {ctx}",
                    LogLevel.Debug);
            }

            // 4. ЛЁГКИЕ ТРАВМЫ (последние!)
            // Hurt (5+ урона, 35% шанс)
            if (damage >= 5 && TryRollCombatInjury(0.35, combatSkill, fromMonster, damage, "Hurt"))
            {
                _monitor.Log(
                    $"[DamageInjury] APPLY buffHurt: damage>={damage}>=5, roll ok, {ctx}",
                    LogLevel.Info);
                _injuryManager.ApplyHurtSafe();
                return;
            }

            _monitor.Log(
                $"[DamageInjury] No injury: damage={damage} (thresholds/rolls not met), fromMonster={fromMonster}, {ctx}",
                LogLevel.Trace);
        }

        /// <summary>
        /// Бросок шанса боевой травмы. При уроне от мобов шанс снижается с ростом навыка боя.
        /// </summary>
        private bool TryRollCombatInjury(
            double baseChance,
            int combatSkill,
            bool fromMonster,
            int damage,
            string injuryKey)
        {
            double chance = fromMonster
                ? GetSkillAdjustedChance(baseChance, combatSkill)
                : ClampChance(baseChance);

            if (!Helpers.GameUtils.Roll(chance))
            {
                _monitor.Log(
                    $"[CombatInjury] {injuryKey} roll failed: damage={damage}, combatSkill={combatSkill}, fromMonster={fromMonster}, chance={chance:P1} (base={ClampChance(baseChance):P0})",
                    LogLevel.Debug);

                return false;
            }

            _monitor.Log(
                $"[CombatInjury] {injuryKey} roll success: damage={damage}, combatSkill={combatSkill}, fromMonster={fromMonster}, chance={chance:P1} (base={ClampChance(baseChance):P0})",
                LogLevel.Debug);

            return true;
        }

        /// <summary>Есть ли живой враждебный моб рядом с игроком (урон, скорее всего, от боя).</summary>
        private static bool IsNearHostileMonster(int tileRadius = 3)
        {
            var location = Game1.currentLocation;
            if (location?.characters == null)
                return false;

            int reach = tileRadius * Game1.tileSize;
            var playerBox = Game1.player.GetBoundingBox();
            var area = new Microsoft.Xna.Framework.Rectangle(
                playerBox.X - reach,
                playerBox.Y - reach,
                playerBox.Width + reach * 2,
                playerBox.Height + reach * 2);

            foreach (var character in location.characters)
            {
                if (character is Monster monster && monster.Health > 0 && area.Intersects(monster.GetBoundingBox()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверить травмы от взрывов
        /// </summary>
        private void CheckExplosionInjuries()
        {
            var location = Game1.currentLocation;
            if (location == null) return;

            bool nearExplosion = false;
            int bombSpritesFound = 0;
            int bombSpritesIntersecting = 0;
            var playerBox = Game1.player.GetBoundingBox();
            var explosionArea = new Microsoft.Xna.Framework.Rectangle(
                playerBox.X - Game1.tileSize * 3,
                playerBox.Y - Game1.tileSize * 3,
                playerBox.Width + Game1.tileSize * 6,
                playerBox.Height + Game1.tileSize * 6
            );

            if (location.temporarySprites != null)
            {
                foreach (var sprite in location.temporarySprites)
                {
                    if (sprite.bombRadius <= 0) continue;

                    bombSpritesFound++;
                    var spriteBox = new Microsoft.Xna.Framework.Rectangle(
                        (int)sprite.position.X,
                        (int)sprite.position.Y,
                        Game1.tileSize * 2,
                        Game1.tileSize * 2
                    );
                    bool intersects = explosionArea.Intersects(spriteBox);
                    float dx = sprite.position.X - playerBox.Center.X;
                    float dy = sprite.position.Y - playerBox.Center.Y;
                    float distTiles = (float)Math.Sqrt(dx * dx + dy * dy) / Game1.tileSize;

                    _monitor.Log(
                        $"[ExplosionInjury] bomb sprite: radius={sprite.bombRadius}, distTiles={distTiles:F1}, " +
                        $"intersects={intersects}, playerBox=({playerBox.X},{playerBox.Y},{playerBox.Width}x{playerBox.Height}), " +
                        $"spritePos=({sprite.position.X:F0},{sprite.position.Y:F0})",
                        LogLevel.Trace);

                    if (intersects)
                    {
                        bombSpritesIntersecting++;
                        nearExplosion = true;
                    }
                }
            }

            if (bombSpritesFound > 0 || nearExplosion)
            {
                _monitor.Log(
                    $"[ExplosionInjury] Scan: bombsWithRadius={bombSpritesFound}, intersecting={bombSpritesIntersecting}, " +
                    $"nearExplosion={nearExplosion}, {FormatInjuryDiagnosticContext()}",
                    LogLevel.Debug);
            }

            if (!nearExplosion) return;

            if (!Helpers.GameUtils.Roll(0.50))
            {
                _monitor.Log(
                    $"[ExplosionInjury] Near explosion but 50% gate roll failed, {FormatInjuryDiagnosticContext()}",
                    LogLevel.Debug);
                return;
            }

            if (Helpers.GameUtils.Roll(0.60))
            {
                _monitor.Log(
                    $"[ExplosionInjury] APPLY buffShrapnelWounds: near explosion, 50%+60% rolls ok, {FormatInjuryDiagnosticContext()}",
                    LogLevel.Warn);
                _injuryManager.ApplyShrapnelWoundsSafe();
            }
            else
            {
                _monitor.Log(
                    $"[ExplosionInjury] APPLY buffBurnWounds: near explosion, 50% ok / 60% failed, {FormatInjuryDiagnosticContext()}",
                    LogLevel.Warn);
                _injuryManager.ApplyBurnWoundsSafe();
            }
        }

        private string GetCurrentToolKey()
        {
            var tool = Game1.player?.CurrentTool;
            if (tool == null) return "Other";

            string name = tool.BaseName ?? tool.Name ?? "";

            if (name.Contains("Watering Can", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("WateringCan", StringComparison.OrdinalIgnoreCase))
                return "WateringCan";

            if (name.Contains("Hoe", StringComparison.OrdinalIgnoreCase))
                return "Hoe";

            if (name.Contains("Pickaxe", StringComparison.OrdinalIgnoreCase))
                return "Pickaxe";

            if (name.Contains("Axe", StringComparison.OrdinalIgnoreCase))
                return "Axe";

            if (name.Contains("Scythe", StringComparison.OrdinalIgnoreCase))
                return "Scythe";

            return "Other";
        }

        private void IncrementToolUseCounter(string toolKey)
        {
            if (toolKey == "Other") return;

            if (!_toolUseCounters.ContainsKey(toolKey))
                _toolUseCounters[toolKey] = 0;

            _toolUseCounters[toolKey]++;
        }

        private int GetToolUseCounter(string toolKey)
        {
            return _toolUseCounters.TryGetValue(toolKey, out int count) ? count : 0;
        }

        public string GetFarmingToolCountersDebug()
        {
            return string.Join(", ", new[]
            {
                $"Hoe={GetToolUseCounter("Hoe")}",
                $"WateringCan={GetToolUseCounter("WateringCan")}",
                $"Axe={GetToolUseCounter("Axe")}",
                $"Pickaxe={GetToolUseCounter("Pickaxe")}",
                $"Scythe={GetToolUseCounter("Scythe")}",
            });
        }

        private void ResetToolUseCounter(string toolKey)
        {
            _toolUseCounters[toolKey] = 0;
        }

        private int GetRelevantSkillLevel(string toolKey)
        {
            return toolKey switch
            {
                "Hoe" => Game1.player.FarmingLevel,
                "WateringCan" => Game1.player.FarmingLevel,
                "Scythe" => Game1.player.FarmingLevel,
                "Axe" => Game1.player.ForagingLevel,
                "Pickaxe" => Game1.player.MiningLevel,
                _ => 0
            };
        }

        private double GetSkillAdjustedChance(double baseChance, string toolKey)
        {
            return GetSkillAdjustedChance(baseChance, GetRelevantSkillLevel(toolKey));
        }

        private double GetSkillAdjustedChance(double baseChance, int skillLevel)
        {
            double reduction = ClampChance(_config.SkillChanceReductionPerLevel);
            double minMultiplier = ClampChance(_config.MinSkillChanceMultiplier);
            double multiplier = Math.Max(minMultiplier, 1.0 - skillLevel * reduction);
            return ClampChance(baseChance) * multiplier;
        }

        private int GetSkillAdjustedUseThreshold(int baseThreshold, string toolKey)
        {
            return GetSkillAdjustedUseThreshold(baseThreshold, GetRelevantSkillLevel(toolKey));
        }

        private int GetSkillAdjustedUseThreshold(int baseThreshold, int skillLevel)
        {
            int bonusPerTwoLevels = AtLeastOne(_config.SkillThresholdBonusPerTwoLevels);
            int maxBonus = Math.Max(0, _config.MaxSkillThresholdBonus);
            int bonus = Math.Min(maxBonus, (skillLevel / 2) * bonusPerTwoLevels);
            return AtLeastOne(baseThreshold) + bonus;
        }

        private static double ClampChance(double value)
        {
            if (double.IsNaN(value)) return 0;
            return Math.Clamp(value, 0.0, 1.0);
        }

        private static int AtLeastOne(int value)
        {
            return Math.Max(1, value);
        }

        private static float AtLeastZero(float value)
        {
            if (float.IsNaN(value)) return 0f;
            return Math.Max(0f, value);
        }

        private void TrackToolUsage()
        {
            if (!_config.EnableFarmingToolUseInjuries) return;
            if (!Context.IsWorldReady || Game1.player == null) return;
            if (Game1.player.CurrentTool == null) return;
            if (!Game1.player.UsingTool) return;

            string toolKey = GetCurrentToolKey();
            if (toolKey == "Other") return;

            int tick = (int)Game1.ticks;

            if (_lastToolUseKey == toolKey && _lastToolUseTick >= 0 && tick - _lastToolUseTick < 20)
                return;

            IncrementToolUseCounter(toolKey);
            _lastToolUseTick = tick;
            _lastToolUseKey = toolKey;

            _monitor.Log(
                $"[FarmingInjury] Tool use: {toolKey}, count={GetToolUseCounter(toolKey)}, stamina={Game1.player.Stamina:F0}",
                LogLevel.Trace);
        }

        /// <summary>
        /// Проверить травмы от сельскохозяйственных инструментов по накопленным счётчикам использования.
        /// </summary>
        private void CheckFarmingInjuries()
        {
            if (!_config.EnableFarmingToolUseInjuries) return;

            int currentMinutes = Helpers.GameUtils.CurrentTimeInMinutes();
            int cooldownMinutes = Math.Max(0, _config.FarmingInjuryRollCooldownMinutes);
            if (_lastFarmingInjuryRollTime >= 0 && currentMinutes - _lastFarmingInjuryRollTime < cooldownMinutes)
                return;

            float stamina = Game1.player.Stamina;
            float deepCutsStaminaThreshold = AtLeastZero(_config.DeepCutsStaminaThreshold);
            float tornMusclesStaminaThreshold = AtLeastZero(_config.TornMusclesStaminaThreshold);
            float backStrainStaminaThreshold = AtLeastZero(_config.BackStrainStaminaThreshold);

            if (_complicationManager.IsMainInjuryIn(InjurySets.OverworkSensitive))
            {
                int totalUses = GetToolUseCounter("Scythe")
                    + GetToolUseCounter("Axe")
                    + GetToolUseCounter("Pickaxe")
                    + GetToolUseCounter("Hoe")
                    + GetToolUseCounter("WateringCan");
                float staminaThreshold = Math.Min(
                    deepCutsStaminaThreshold,
                    Math.Min(tornMusclesStaminaThreshold, backStrainStaminaThreshold));
                int usesThreshold = Math.Min(
                    _config.DeepCutsToolUsesThreshold,
                    Math.Min(_config.TornMusclesToolUsesThreshold, _config.BackStrainToolUsesThreshold));

                LogFarmingInjuryPotential("OverworkComplication", stamina);

                if (_complicationManager.TryRollOverworkComplication(
                        stamina,
                        totalUses,
                        staminaThreshold,
                        usesThreshold))
                {
                    _lastFarmingInjuryRollTime = currentMinutes;
                }

                return;
            }

            if (stamina <= deepCutsStaminaThreshold)
            {
                LogFarmingInjuryPotential("DeepCuts", stamina);
                if (TryRollDeepCuts(stamina))
                {
                    _lastFarmingInjuryRollTime = currentMinutes;
                    return;
                }
            }

            if (stamina <= tornMusclesStaminaThreshold)
            {
                LogFarmingInjuryPotential("TornMuscles", stamina);
                if (TryRollTornMuscles(stamina))
                {
                    _lastFarmingInjuryRollTime = currentMinutes;
                    return;
                }
            }

            if (TryRollBackStrain(stamina))
            {
                _lastFarmingInjuryRollTime = currentMinutes;
            }
        }

        private void LogFarmingInjuryPotential(string rollTarget, float stamina)
        {
            string toolName = GetCurrentToolKey();
            var currentTool = Game1.player?.CurrentTool;
            string toolDetail = currentTool != null
                ? (currentTool.BaseName ?? currentTool.Name ?? "?")
                : "none";
            int hoeCan = GetToolUseCounter("Hoe") + GetToolUseCounter("WateringCan");
            int axePick = GetToolUseCounter("Axe") + GetToolUseCounter("Pickaxe");
            int scytheAxe = GetToolUseCounter("Scythe") + GetToolUseCounter("Axe");

            _monitor.Log(
                $"[FarmingInjury] Potential {rollTarget}: tool={toolName}({toolDetail}), stamina={stamina:F0}, " +
                $"isHoeOrCan={hoeCan > 0} (uses={hoeCan}), isAxeOrPick={axePick > 0} (uses={axePick}), " +
                $"isScytheOrAxe={scytheAxe > 0} (uses={scytheAxe}), {FormatInjuryDiagnosticContext()}",
                LogLevel.Debug);
        }

        private static int? TryGetMineLevel(GameLocation? location)
        {
            if (location is MineShaft shaft)
                return shaft.mineLevel;
            return null;
        }

        private string FormatInjuryDiagnosticContext()
        {
            var loc = Game1.currentLocation;
            string locName = loc?.NameOrUniqueName ?? "?";
            int? mineLevel = TryGetMineLevel(loc);
            string minePart = mineLevel.HasValue ? $", mineLevel={mineLevel.Value}" : "";
            string tool = GetCurrentToolKey();
            var currentTool = Game1.player?.CurrentTool;
            string toolDetail = currentTool != null
                ? (currentTool.BaseName ?? currentTool.Name ?? "?")
                : "none";
            bool hasMenu = Game1.activeClickableMenu != null;
            bool eventUp = Game1.eventUp;
            bool usingTool = Game1.player?.UsingTool == true;

            return
                $"loc={locName}{minePart}, health={Game1.player?.health}/{Game1.player?.maxHealth}, " +
                $"tool={tool}({toolDetail}), stamina={Game1.player?.Stamina:F0}, " +
                $"menu={hasMenu}, eventUp={eventUp}, usingTool={usingTool}";
        }

        private bool TryRollDeepCuts(float stamina)
        {
            int scytheUses = GetToolUseCounter("Scythe");
            int axeUses = GetToolUseCounter("Axe");
            int uses = scytheUses + axeUses;
            int skill = scytheUses > axeUses ? Game1.player.FarmingLevel : Game1.player.ForagingLevel;
            int threshold = GetSkillAdjustedUseThreshold(_config.DeepCutsToolUsesThreshold, skill);
            double baseChance = ClampChance(_config.DeepCutsFarmingBaseChance);
            double chance = GetSkillAdjustedChance(baseChance, skill);
            float staminaThreshold = AtLeastZero(_config.DeepCutsStaminaThreshold);

            if (uses < threshold) return false;
            if (stamina > staminaThreshold) return false;

            ResetToolUseCounter("Scythe");
            ResetToolUseCounter("Axe");

            _monitor.Log(
                $"[FarmingInjury] DeepCuts roll: uses={uses}, threshold={threshold}, skill={skill}, stamina={stamina:F0}, chance={chance:P1} (base={baseChance:P0})",
                LogLevel.Debug);

            if (!Helpers.GameUtils.Roll(chance))
            {
                _monitor.Log(
                    $"[FarmingInjury] DeepCuts roll failed: uses={uses}, threshold={threshold}, skill={skill}, stamina={stamina:F0}, chance={chance:P1}",
                    LogLevel.Debug);
                return false;
            }

            _monitor.Log(
                $"[FarmingInjury] DeepCuts roll success → ApplyDeepCutsSafe(source=farming), {FormatInjuryDiagnosticContext()}",
                LogLevel.Warn);
            _injuryManager.ApplyDeepCutsSafe("farming");
            return true;
        }

        private bool TryRollTornMuscles(float stamina)
        {
            int axeUses = GetToolUseCounter("Axe");
            int pickUses = GetToolUseCounter("Pickaxe");
            int uses = axeUses + pickUses;
            int skill = pickUses > axeUses ? Game1.player.MiningLevel : Game1.player.ForagingLevel;
            int threshold = GetSkillAdjustedUseThreshold(_config.TornMusclesToolUsesThreshold, skill);
            double baseChance = ClampChance(_config.TornMusclesBaseChance);
            double chance = GetSkillAdjustedChance(baseChance, skill);
            float staminaThreshold = AtLeastZero(_config.TornMusclesStaminaThreshold);

            if (uses < threshold) return false;
            if (stamina > staminaThreshold) return false;

            ResetToolUseCounter("Axe");
            ResetToolUseCounter("Pickaxe");

            _monitor.Log(
                $"[FarmingInjury] TornMuscles roll: uses={uses}, threshold={threshold}, skill={skill}, stamina={stamina:F0}, chance={chance:P1} (base={baseChance:P0})",
                LogLevel.Debug);

            if (!Helpers.GameUtils.Roll(chance))
            {
                _monitor.Log(
                    $"[FarmingInjury] TornMuscles roll failed: uses={uses}, threshold={threshold}, skill={skill}, stamina={stamina:F0}, chance={chance:P1}",
                    LogLevel.Debug);
                return false;
            }

            _injuryManager.ApplyTornMusclesSafe();
            return true;
        }

        private bool TryRollBackStrain(float stamina)
        {
            int uses = GetToolUseCounter("Hoe") + GetToolUseCounter("WateringCan");
            int skill = Game1.player.FarmingLevel;
            int threshold = GetSkillAdjustedUseThreshold(_config.BackStrainToolUsesThreshold, skill);
            double baseChance = ClampChance(_config.BackStrainBaseChance);
            double chance = GetSkillAdjustedChance(baseChance, skill);
            float staminaThreshold = AtLeastZero(_config.BackStrainStaminaThreshold);

            if (uses < threshold) return false;
            if (stamina > staminaThreshold) return false;

            ResetToolUseCounter("Hoe");
            ResetToolUseCounter("WateringCan");

            _monitor.Log(
                $"[FarmingInjury] BackStrain roll: uses={uses}, threshold={threshold}, skill={skill}, stamina={stamina:F0}, chance={chance:P1} (base={baseChance:P0})",
                LogLevel.Debug);

            if (!Helpers.GameUtils.Roll(chance))
            {
                _monitor.Log(
                    $"[FarmingInjury] BackStrain roll failed: uses={uses}, threshold={threshold}, skill={skill}, stamina={stamina:F0}, chance={chance:P1}",
                    LogLevel.Debug);
                return false;
            }

            _injuryManager.ApplyBackStrainSafe();
            return true;
        }

        /// <summary>
        /// Проверить условия окружающей среды (холод на улице, алкоголь при лечении).
        /// </summary>
        private void CheckEnvironmentalConditions()
        {
            // Секунды на улице в холоде: зима, дождь или снег без buffHarveyProtection / buffWarmth.
            bool outdoors = Game1.player.currentLocation?.IsOutdoors == true;
            bool coldSeason = Game1.currentSeason == "winter" || Game1.isRaining || Game1.isSnowing;
            bool hasProtection = Game1.player.hasBuff("buffHarveyProtection") || Game1.player.hasBuff("buffWarmth");

            if (outdoors && coldSeason && !hasProtection)
            {
                _coldExposureSeconds += ENVIRONMENT_CHECK_INTERVAL_SECONDS;

                if (_coldExposureSeconds % 60 == 0)
                {
                    _monitor.Log($"[ColdExposure] На холоде {_coldExposureSeconds}/{COLD_EXPOSURE_THRESHOLD_SECONDS} сек.", LogLevel.Trace);
                }

                if (_coldExposureSeconds >= COLD_EXPOSURE_THRESHOLD_SECONDS
                    && !Game1.player.hasBuff("buffTooCold")
                    && !Helpers.GameUtils.HasConversationTopic(ConversationTopics.TooCold))
                {
                    _buffManager.AddBuff("buffTooCold", -2);
                    Game1.player.activeDialogueEvents.TryAdd(ConversationTopics.TooCold, 2);
                    _monitor.Log($"Игрок замерз после {_coldExposureSeconds} секунд на холоде", LogLevel.Debug);
                    _coldExposureSeconds = 0;
                }
            }
            else
            {
                // Ушли в помещение, сезон/погода или защита сняли холод — сброс накопленных секунд.
                _coldExposureSeconds = 0;
            }

            // Алкоголь при лечении
            if (Game1.player.hasBuff("Tipsy")
                && (Game1.player.hasBuff("buffAntibioticsTreatment") || Game1.player.hasBuff("buffTeracitin"))
                && _dialogueManager.GetFriendshipHearts("Harvey") >= 4
                && !Helpers.GameUtils.HasConversationTopic("situationReaction_Drunk")
                && !_stateManager.WasStoryTriggerApplied("{{ModId}}_triggerSituationReactionDrunk"))
            {
                Game1.player.activeDialogueEvents.TryAdd("situationReaction_Drunk", 3);
                _buffManager.AddBuff("buffAlcoholPoisoning", -2);
                _stateManager.MarkStoryTriggerApplied("{{ModId}}_triggerSituationReactionDrunk");
            }
        }
    }
}

