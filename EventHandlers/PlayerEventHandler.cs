using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

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
        private readonly ProximityReactionManager _proximityReactionManager;

        private PassOutHandler? _passOutHandler;

        /// <summary>Последняя обычная proximity-реакция (игровые минуты с полуночи). Кулдаун 2 ч.</summary>
        private int _lastProximityReactionMinute = -1;
        /// <summary>Одно облачко за визит в локацию (сброс при варпе в другую локацию).</summary>
        private bool _proximityReactionShown = false;
        private const int ProximityReactionCooldownMinutes = 120;
        private string _lastLocationName = "";
        private int _lastMineWarningDay = -1;
        private bool _eventWasActive;
        private bool _stormComfortEventRunning;
        private bool _firstTreatmentEventRunning;
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
            ProximityReactionManager proximityReactionManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _treatmentManager = treatmentManager;
            _hospitalizationManager = hospitalizationManager;
            _dialogueManager = dialogueManager;
            _proximityReactionManager = proximityReactionManager;
        }

        public void SetPassOutHandler(PassOutHandler passOutHandler)
        {
            _passOutHandler = passOutHandler;
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
                        $"[Proximity] Локация «{_lastLocationName}»: сброс per-location, кулдаун с {_lastProximityReactionMinute} игр. мин",
                        LogLevel.Debug);
                }

                // Проверка госпитализации
                if (_hospitalizationManager.HandleWarpAttempt(e.NewLocation, e.OldLocation))
                {
                    return; // Варп заблокирован
                }

                // Логика локаций
                HandleLocationLogic(e.NewLocation);

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
                    _e5StormBesideEventRunning = string.Equals(
                        eventId,
                        RescueOperationIds.E5StormBesideEvent,
                        StringComparison.OrdinalIgnoreCase);
                    _rescueOperationEventRunning = RescueOperationLauncher.IsRescueOperationEventId(eventId);
                }
                else if (_eventWasActive)
                {
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
                }

                // Проверка здоровья
                if (e.IsMultipleOf(120))
                {
                    CheckHealthBasedInjuries();
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

            if (IsMineOrVolcanoLocation(location))
                HandleMinesLogic();

            if (string.Equals(location?.Name, "BathHouse_Pool", StringComparison.OrdinalIgnoreCase))
                HandleSpaLogic();
        }

        private void HandleHospitalLogic()
        {
            // Проверка: игрок в госпитале после спасения из шахты
            bool hasMineInjuryTopic = Helpers.GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue);
            bool hasCriticalInjuries = _buffManager.HasAnyBuff(InjurySets.Severe.ToArray());
            
            if (_config.ForceHospitalization && hasMineInjuryTopic && hasCriticalInjuries)
            {
                // Если уже госпитализирован, не запускаем снова
                if (_hospitalizationManager.IsHospitalized)
                    return;

                _monitor.Log("⚠️ Игрок в госпитале с ранами после шахты → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ", LogLevel.Warn);
                
                NPC? harvey = HarveyHelper.GetHarvey();
                string? injury = _injuryManager.GetActiveInjury() ?? "buffBadlyHurt";
                
                _hospitalizationManager.StartForcedHospitalizationWithExplanation(injury, harvey, "mine_rescue");
                
                // Удаляем топик после срабатывания
                _dialogueManager.RemoveTopic(ConversationTopics.MineInjuryRescue);
            }
        }

        private void HandleMinesLogic()
        {
            int today = Helpers.GameUtils.Today();

            if (_buffManager.HasBuff(InjuryBuffs.MineForbidden))
            {
                HandleMineForbiddenEntry(today);
                return;
            }

            // Штатный rescue-warp утром — не показываем «не ходи в шахту» перед cutscene
            if (_stateManager.State.NeedsMineRescueEvent)
                return;

            bool hasSevereInjury = _buffManager.HasAnyBuff(InjurySets.Severe.ToArray());
            bool hasLimitedActivity = _buffManager.HasAnyBuff(InjurySets.LimitedActivity.ToArray());
            bool hasAnyInjury    = hasSevereInjury
                || hasLimitedActivity
                || _stateManager.GetAllActiveDebuffStates().Count > 0
                || _stateManager.State.ActiveComplications.Count > 0;

            if (hasSevereInjury)
            {
                if (!TryHandleSevereMineEntry(today))
                    return;
            }
            else if (hasAnyInjury && _lastMineWarningDay != today)
            {
                _lastMineWarningDay = today;

                if (hasLimitedActivity)
                {
                    Game1.addHUDMessage(new HUDMessage(
                        "Харви: С такой травмой шахта — плохая идея. Хотя бы не перегружайся.",
                        HUDMessage.health_type));
                    _monitor.Log("ℹ️ [Шахта] Вход с ограничением активности — предупреждение Харви", LogLevel.Debug);
                }
                else
                {
                    // Прочие травмы (напр. открытые раны DirtyInMines) — напоминание о загрязнении
                    Game1.addHUDMessage(new HUDMessage(
                        "Харви: Будь осторожна в шахте — твои раны могут загрязниться.",
                        HUDMessage.health_type));
                    _monitor.Log("ℹ️ [Шахта] Вход с ранами — напоминание Харви", LogLevel.Debug);
                }
            }

            if (!hasSevereInjury && hasAnyInjury && _passOutHandler != null)
            {
                if (_passOutHandler.TryTriggerMinorMineRescue())
                    _monitor.Log("[MinorMineRescue] Cutscene запущена — опасное состояние без Severe", LogLevel.Info);
            }
        }

        /// <summary>
        /// Severe без MineForbidden: первый вход за день — предупреждение; повторный — выход наружу.
        /// </summary>
        /// <returns>true, если игрок может остаться в подземелье; false, если уже выгнан.</returns>
        private bool TryHandleSevereMineEntry(int today)
        {
            var state = _stateManager.State;

            if (state.LastMineSevereWarningDay != today)
            {
                state.LastMineSevereWarningDay = today;
                state.MineWarningDay = today;
                _stateManager.Save();

                Game1.addHUDMessage(new HUDMessage(
                    "Харви: У тебя серьёзные раны — ты не должна идти в шахту! Возможны осложнения.",
                    HUDMessage.error_type));
                _monitor.Log("⚠️ [Шахта] Вход с серьёзными ранами — предупреждение Харви, письмо и дебафф завтра", LogLevel.Warn);
                return true;
            }

            GameLocation location = Game1.currentLocation;
            bool repeatAttempt = state.LastMineSevereForcedExitDay == today;

            if (!repeatAttempt)
            {
                state.LastMineSevereForcedExitDay = today;
                _stateManager.Save();
                _monitor.Log("⚠️ [Шахта] Повторный вход с Severe после предупреждения — принудительный выход", LogLevel.Warn);
            }
            else
            {
                _monitor.Log("⚠️ [Шахта] Повторная попытка входа с Severe после принудительного выхода", LogLevel.Debug);
            }

            Game1.addHUDMessage(new HUDMessage(
                repeatAttempt
                    ? "Сегодня шахта закончена."
                    : "Харви уже предупреждал тебя. Сегодня шахта закончена.",
                HUDMessage.error_type));
            Game1.playSound("cancel");
            WarpOutOfForbiddenDungeon(location);
            return false;
        }

        private void HandleMineForbiddenEntry(int today)
        {
            GameLocation location = Game1.currentLocation;

            if (IsVolcanoLocation(location))
            {
                _monitor.Log("[MineForbidden] Игрок вошёл в вулкан при активном запрете Харви", LogLevel.Warn);
                ShowMineForbiddenHudAndWarpOut(today, location);
                return;
            }

            _monitor.Log("[MineForbidden] Игрок вошёл в шахту при активном запрете Харви", LogLevel.Warn);

            if (_stateManager.State.LastMineForbiddenInterceptionDay != today)
            {
                _stateManager.State.LastMineForbiddenInterceptionDay = today;
                _stateManager.Save();

                if (TryStartEventByName("eventHarveyMineInterception", "Mine", WarpOutOfMineIfStillInside))
                    return;

                _monitor.Log("[MineForbidden] Событие не запустилось — fallback HUD + warp", LogLevel.Warn);
            }

            ShowMineForbiddenHudAndWarpOut(today, location);
        }

        private int GetMineForbiddenDaysLeft(int today)
        {
            int appliedDay = _stateManager.State.MineForbiddenAppliedDay;
            int duration = Math.Max(1, _config.MineForbiddenDurationDays);

            if (appliedDay < 0)
                return duration;

            return Math.Max(1, appliedDay + duration - today);
        }

        private string GetMineForbiddenMessage(int today)
        {
            int daysLeft = GetMineForbiddenDaysLeft(today);

            if (daysLeft == 1)
                return "Харви запретил шахту до окончания лечения. Остался 1 день.";

            return $"Харви запретил шахту до окончания лечения. Осталось: {daysLeft} дн.";
        }

        private string GetVolcanoForbiddenMessage(int today)
        {
            int daysLeft = GetMineForbiddenDaysLeft(today);

            if (daysLeft == 1)
                return "Харви запретил тебе ходить в опасные подземелья до окончания лечения. Остался 1 день.";

            return $"Харви запретил тебе ходить в опасные подземелья до окончания лечения. Осталось: {daysLeft} дн.";
        }

        private void ShowMineForbiddenHudAndWarpOut(int today, GameLocation location)
        {
            bool isVolcano = IsVolcanoLocation(location);

            Game1.addHUDMessage(new HUDMessage(
                isVolcano ? GetVolcanoForbiddenMessage(today) : GetMineForbiddenMessage(today),
                HUDMessage.error_type));

            Game1.playSound(isVolcano ? "debuffHit" : "cancel");
            WarpOutOfForbiddenDungeon(location);
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

                location.startEvent(gameEvent);
                _monitor.Log($"[MineForbidden] Запущено событие '{eventId}'", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MineForbidden] Ошибка запуска события '{eventId}': {ex}", LogLevel.Error);
                return false;
            }
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

            double chance = CalculateDirtyWoundChance(
                _stateManager.State.MineDirtyExposureMinutesToday,
                currentMinute
            );

            if (chance <= 0)
                return;

            _stateManager.State.LastMineDirtyWoundRollMinute = currentMinute;
            TryApplyDirtyWoundFromMine(chance, $"exposure={_stateManager.State.MineDirtyExposureMinutesToday}m");
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

        private bool HasDirtyMineInjury()
        {
            foreach (var injuryId in InjurySets.DirtyInMines)
            {
                if (_buffManager.HasBuff(injuryId))
                    return true;
            }

            return false;
        }

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
            {
                chance += Math.Clamp(_config.DirtyWoundMineDamageBonusChance, 0.0, 1.0);
            }

            return Math.Clamp(chance, 0.0, 0.95);
        }

        private void TryApplyDirtyWoundFromMine(double chance, string reason)
        {
            if (!Helpers.GameUtils.Roll(chance))
            {
                _monitor.Log($"[Шахта] Грязная рана не сработала: chance={chance:P0}, {reason}", LogLevel.Debug);
                return;
            }

            int today = Helpers.GameUtils.Today();

            _buffManager.AddBuff(InjuryBuffs.DirtyWound, -2);
            _stateManager.State.ActiveComplications[InjuryBuffs.DirtyWound] = today;
            _stateManager.CreateComplicationState(InjuryBuffs.DirtyWound, today);
            _dialogueManager.AddTopic(ConversationTopics.DirtyWound, 4);

            var harvey = HarveyHelper.FindHarveyInLocation(Game1.currentLocation);
            if (harvey != null)
            {
                var injuries = _injuryManager.CollectAllInjuries();
                int emote = _proximityReactionManager.DetermineEmoteForProximity(injuries);
                var prefixes = _proximityReactionManager.DetermineProximityPrefixCandidates(injuries);
                string text = _dialogueManager.PickRandomProximityLineByPrefixes(
                    prefixes,
                    DialogueManager.ProximityDialogueFallback);
                _dialogueManager.ShowEmoteWithText(harvey, emote, text);
            }

            Game1.addHUDMessage(new HUDMessage("Рана загрязнилась! Риск инфекции!", HUDMessage.error_type));
            _monitor.Log($"[Шахта] Рана загрязнилась: chance={chance:P0}, {reason}", LogLevel.Warn);
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
            
            // === ПРОМОКАНИЕ ПОВЯЗКИ ===
            bool hasBandage = _buffManager.HasBuff(CureBuffs.Treatment) || _buffManager.HasBuff(CureBuffs.IntensiveCare);
            bool alreadyWet = _buffManager.HasBuff(InjuryBuffs.WetBandage);

            if (!hasBandage || alreadyWet)
            {
                // Сбрасываем счетчик если повязки нет или уже промокла
                _stateManager.State.TimeUnderRainTicks = 0;
                return;
            }

            // HandleRainLogic вызывается раз в секунду из OnUpdateTicked, поэтому +1 = 1 секунда.
            _stateManager.State.TimeUnderRainTicks++;

            int secondsUnderRain = _stateManager.State.TimeUnderRainTicks;

            // Проверка промокания раз в 10 секунд, чтобы повязка не роллилась слишком часто
            if (secondsUnderRain % 10 == 0)
            {
                double wetChance = CalculateWetChance(secondsUnderRain);

                if (Helpers.GameUtils.Roll(wetChance))
                {
                    int today = Helpers.GameUtils.Today();
                    _buffManager.AddBuff(InjuryBuffs.WetBandage, -2);
                    _stateManager.State.ActiveComplications[InjuryBuffs.WetBandage] = today;
                    _stateManager.CreateComplicationState(InjuryBuffs.WetBandage, today);
                    _dialogueManager.AddTopic(ConversationTopics.WetBandage, 4);
                    Game1.addHUDMessage(new HUDMessage("Повязка промокла!", HUDMessage.error_type));
                    _stateManager.State.TimeUnderRainTicks = 0;
                    _monitor.Log($"Повязка промокла после {secondsUnderRain}с под дождём", LogLevel.Info);
                }
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

        private void HandleSpaLogic()
        {
            int today = Helpers.GameUtils.Today();

            bool hasBandage = _buffManager.HasBuff(CureBuffs.Treatment) || _buffManager.HasBuff(CureBuffs.IntensiveCare);
            if (hasBandage && !_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.WetBandage))
            {
                _buffManager.AddBuff(InjuryBuffs.WetBandage, -2);
                _stateManager.State.ActiveComplications[InjuryBuffs.WetBandage] = today;
                _stateManager.CreateComplicationState(InjuryBuffs.WetBandage, today);
                _dialogueManager.AddTopic(ConversationTopics.WetBandage, 4);
                Game1.addHUDMessage(new HUDMessage("Повязка промокла! Нельзя было купаться с повязкой!", HUDMessage.error_type));
                _monitor.Log("Повязка промокла при купании", LogLevel.Warn);
            }

            if (_buffManager.HasBuff("buffSurgicalWound") && !_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.WetStitches))
            {
                _buffManager.AddBuff(InjuryBuffs.WetStitches, -2);
                _stateManager.State.ActiveComplications[InjuryBuffs.WetStitches] = today;
                _stateManager.CreateComplicationState(InjuryBuffs.WetStitches, today);
                _dialogueManager.AddTopic(ConversationTopics.WetStitches, 4);
                Game1.addHUDMessage(new HUDMessage("Швы намокли! Нельзя было купаться со швами!", HUDMessage.error_type));
                _monitor.Log("Швы намокли при купании", LogLevel.Warn);
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
                && _buffManager.HasAnyBuff(InjurySets.Severe.ToArray());
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
                    string? injury = _injuryManager.GetActiveInjury() ?? "buffBadlyHurt";
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
            _lastProximityReactionMinute = Helpers.GameUtils.CurrentTimeInMinutes();
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

            if (_lastProximityReactionMinute >= 0)
            {
                int now = Helpers.GameUtils.CurrentTimeInMinutes();
                int elapsed = now - _lastProximityReactionMinute;
                if (elapsed < 0)
                    elapsed += 24 * 60;

                if (elapsed < ProximityReactionCooldownMinutes)
                {
                    _monitor.Log(
                        $"[Proximity] Пропуск: кулдаун {elapsed}/{ProximityReactionCooldownMinutes} игр. мин",
                        LogLevel.Debug);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Короткое облачко при проходе мимо (эмоция + текст над головой, без диалогового окна).
        /// </summary>
        private void ShowProximityDiscovery(NPC harvey, Core.Models.InjuryCollection injuries)
        {
            int emote = _proximityReactionManager.DetermineEmoteForProximity(injuries);
            var prefixes = _proximityReactionManager.DetermineProximityPrefixCandidates(injuries);
            string textMessage = _dialogueManager.PickRandomProximityLineByPrefixes(
                prefixes,
                DialogueManager.ProximityDialogueFallback);

            _dialogueManager.ShowEmoteWithText(harvey, emote, textMessage);

            _monitor.Log(
                $"[Proximity] Облачко: emote={emote}, prefixes=[{string.Join(", ", prefixes)}], text='{textMessage}'",
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

                ProcessDamageBasedInjuries(damage);
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
        private void ProcessDamageBasedInjuries(int damage)
        {
            // 1. КРИТИЧЕСКОЕ ЗДОРОВЬЕ (всегда приоритет!)
            if (Game1.player.health <= 10)
            {
                _injuryManager.ApplyBadlyHurtSafe();
                return;
            }

            // 2. ТЯЖЁЛЫЕ ТРАВМЫ (большой урон)
            // Fractured Bone (30+ урона, 10% шанс)
            if (damage >= 30 && Helpers.GameUtils.Roll(0.10))
            {
                _injuryManager.ApplyFracturedBoneSafe();
                return;
            }

            // Concussion (20+ урона, 25% шанс)
            if (damage >= 20 && Helpers.GameUtils.Roll(0.25))
            {
                _injuryManager.ApplyConcussionSafe();
                return;
            }

            // 3. СРЕДНИЕ ТРАВМЫ
            // Bruised Ribs (15+ урона, 25% шанс)
            if (damage >= 15 && Helpers.GameUtils.Roll(0.25))
            {
                _injuryManager.ApplyBruisedRibsSafe();
                return;
            }

            // Deep Cuts (10+ урона, 30% шанс)
            if (damage >= 10 && Helpers.GameUtils.Roll(0.30))
            {
                _injuryManager.ApplyDeepCutsSafe("combat");
                return;
            }

            // 4. ЛЁГКИЕ ТРАВМЫ (последние!)
            // Hurt (5+ урона, 35% шанс)
            if (damage >= 5 && Helpers.GameUtils.Roll(0.35))
            {
                _injuryManager.ApplyHurtSafe();
                return;
            }

            // Малый урон (< 5) - никакой травмы
        }

        /// <summary>
        /// Проверить травмы от взрывов
        /// </summary>
        private void CheckExplosionInjuries()
        {
            var location = Game1.currentLocation;
            if (location == null) return;

            bool nearExplosion = false;
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
                    if (sprite.bombRadius > 0)
                    {
                        var spriteBox = new Microsoft.Xna.Framework.Rectangle(
                            (int)sprite.position.X,
                            (int)sprite.position.Y,
                            Game1.tileSize * 2,
                            Game1.tileSize * 2
                        );

                        if (explosionArea.Intersects(spriteBox))
                        {
                            nearExplosion = true;
                            break;
                        }
                    }
                }
            }

            if (nearExplosion && Helpers.GameUtils.Roll(0.50))
            {
                _monitor.Log("Игрок рядом со взрывом - применяем травму", LogLevel.Warn);
                
                if (Helpers.GameUtils.Roll(0.60))
                {
                    _injuryManager.ApplyShrapnelWoundsSafe();
                }
                else
                {
                    _injuryManager.ApplyBurnWoundsSafe();
                }
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

            if (stamina <= deepCutsStaminaThreshold && TryRollDeepCuts(stamina))
            {
                _lastFarmingInjuryRollTime = currentMinutes;
                return;
            }

            if (stamina <= tornMusclesStaminaThreshold && TryRollTornMuscles(stamina))
            {
                _lastFarmingInjuryRollTime = currentMinutes;
                return;
            }

            if (TryRollBackStrain(stamina))
            {
                _lastFarmingInjuryRollTime = currentMinutes;
            }
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

