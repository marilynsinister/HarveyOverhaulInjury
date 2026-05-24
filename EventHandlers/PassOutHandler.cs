using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик обморока и истощения игрока
    /// </summary>
    public class PassOutHandler
    {
        private const string FallbackCritical = "critical";
        private const string FallbackExhaustion = "exhaustion";

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly InjuryManager _injuryManager;

        public PassOutHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _injuryManager = injuryManager;
        }

        /// <summary>
        /// Обработать телепортацию после обморока
        /// </summary>
        public void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            // === Minor mine rescue (опасное состояние без Severe) ===
            string pendingMinorId = _stateManager.State.PendingMinorMineRescueEventId;
            if (!string.IsNullOrEmpty(pendingMinorId) &&
                string.Equals(e.NewLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log($"[MinorMineRescue] Игрок в Mine — запускаем: {pendingMinorId}", LogLevel.Info);
                _stateManager.State.PendingMinorMineRescueEventId = "";

                if (!TryStartLocationEvent(pendingMinorId, "Mine", OnMinorMineRescueEventFinished))
                    RunMinorMineRescueFallback();

                return;
            }

            // === Запуск отложенного события спасения после боевой смерти ===
            string pendingMineId = _stateManager.State.PendingMineRescueEventId;
            if (!string.IsNullOrEmpty(pendingMineId) &&
                e.NewLocation?.NameOrUniqueName == "Mine")
            {
                _monitor.Log($"[MineRescue] Игрок в шахте — запускаем событие: {pendingMineId}", LogLevel.Info);
                if (!TryStartLocationEvent(pendingMineId, "Mine", OnMineRescueEventFinished))
                    RunMineRescueFallback(pendingMineId);

                return;
            }

            // === Запуск отложенного hospital pass-out cutscene ===
            string pendingHospitalId = _stateManager.State.PendingHospitalPassOutEventId;
            if (!string.IsNullOrEmpty(pendingHospitalId) &&
                IsHospitalLocation(e.NewLocation))
            {
                _monitor.Log($"[PassOutEvent] Игрок в Hospital — запускаем: {pendingHospitalId}", LogLevel.Info);
                if (TryStartLocationEvent(pendingHospitalId, GetHospitalLocationName(), OnHospitalPassOutEventFinished))
                    return;

                RunHospitalPassOutFallback(_stateManager.State.PendingHospitalPassOutFallbackKind);
                ClearHospitalPassOutPending();
                ClearPassOutFlags();
                return;
            }

            if (!Context.IsWorldReady || !_stateManager.State.WasPassedOut) return;

            _monitor.Log($"🏥 Обнаружена телепортация после обморока: {e.OldLocation?.NameOrUniqueName} → {e.NewLocation?.NameOrUniqueName}", LogLevel.Info);

            bool isDatingOrMarried = _dialogueManager.IsDatingOrMarriedToHarvey();
            bool isMinePassOut = IsMineRelatedPassOut();

            // === 1. Критический pass-out вне шахты → eventHarveyEmergencyCare ===
            if (isDatingOrMarried &&
                !isMinePassOut &&
                _stateManager.State.LastPassedOutHealth >= 0 &&
                _stateManager.State.LastPassedOutHealth <= 10)
            {
                _monitor.Log($"⚠️ Критический pass-out вне шахты (здоровье было {_stateManager.State.LastPassedOutHealth})", LogLevel.Warn);
                _injuryManager.ApplyBadlyHurtSafe();

                if (QueueHospitalEvent(EventIds.EmergencyCare, FallbackCritical))
                {
                    ClearPassOutFlags();
                    return;
                }

                RunCriticalPassOutFallback();
            }
            // === 2. Истощение → eventHarveyExhaustion (вне шахты) ===
            else if (isDatingOrMarried &&
                     _stateManager.State.WasExhausted &&
                     !Helpers.GameUtils.HasConversationTopic(ConversationTopics.FarmerExhausted) &&
                     !Game1.player.hasBuff("buffFarmerExhausted"))
            {
                if (!isMinePassOut)
                {
                    _monitor.Log("💤 Pass-out от истощения — пробуем hospital cutscene", LogLevel.Info);

                    if (QueueHospitalEvent(EventIds.Exhaustion, FallbackExhaustion))
                    {
                        ClearPassOutFlags();
                        return;
                    }

                    RunExhaustionPassOutFallback();
                }
                else
                {
                    _monitor.Log("💤 Истощение в шахте — hospital cutscene пропущен", LogLevel.Info);
                    RunExhaustionPassOutFallback();
                }
            }
            // === 3. Шахтная гибель / rescue pipeline (без hospital cutscene) ===
            else if (isDatingOrMarried &&
                     isMinePassOut &&
                     _stateManager.State.LastPassedOutHealth >= 0 &&
                     _stateManager.State.LastPassedOutHealth <= 10)
            {
                _monitor.Log($"⚠️ Критический pass-out в шахте — mine rescue, не hospital cutscene", LogLevel.Warn);
                _injuryManager.ApplyBadlyHurtFromMinePassOut();
            }
            // === 4. Обморок в городе из-за позднего времени ===
            else if (_stateManager.State.WasUpTooLate &&
                     _stateManager.State.LastPassedOutLocation.Contains("Town", StringComparison.OrdinalIgnoreCase) &&
                     !Helpers.GameUtils.HasConversationTopic(ConversationTopics.PassedOutInTown) &&
                     !Game1.player.hasBuff("buffSleepy"))
            {
                _monitor.Log("🌙 Триггер: Обморок в городе из-за позднего времени", LogLevel.Info);

                _buffManager.AddBuff("buffSleepy", -2);
                _dialogueManager.AddTopic(ConversationTopics.PassedOutInTown, 2);

                if (_config.SendLetters)
                    Game1.addMailForTomorrow(MailIds.SleepControl);

                Game1.playSound("debuffHit");
                Game1.addHUDMessage(new HUDMessage("Ты упала без сил посреди города...", HUDMessage.health_type));
            }

            ClearPassOutFlags();
        }

        /// <summary>
        /// Вызывается каждый тик. Фиксирует боевую смерть в шахте в момент достижения health &lt;= 0,
        /// чтобы при варпе Робин/Марлон и восстановлении здоровья к DayEnding флаги уже были установлены.
        /// </summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            // Нельзя throttling-ить: health <= 0 может длиться один тик до ванильного rescue/warp.
            var loc = Game1.currentLocation?.NameOrUniqueName ?? "";
            bool isMine = loc.Contains("Mine", StringComparison.OrdinalIgnoreCase) || loc == "UndergroundMine";
            if (!isMine) return;

            if (Game1.player.health > 0) return;
            if (_stateManager.State.NeedsMineRescueEvent) return;
            if (!_dialogueManager.IsDatingOrMarriedToHarvey()) return;

            _stateManager.State.WasPassedOut = true;
            _stateManager.State.WasExhausted = false;
            _stateManager.State.WasUpTooLate = false;
            _stateManager.State.LastPassedOutHealth = 0;
            _stateManager.State.LastPassedOutLocation = loc;
            _stateManager.State.PassedOutInMineYesterday = true;
            _stateManager.State.NeedsMineRescueEvent = true;

            _injuryManager.ApplyBadlyHurtFromMinePassOut();
            _stateManager.Save();

            _monitor.Log($"[MineRescue] Зафиксирована боевая смерть в шахте в реальном времени: {loc}", LogLevel.Info);
        }

        /// <summary>
        /// Отследить обморок игрока (вызывается перед DayEnding)
        /// </summary>
        public void TrackPassOut()
        {
            var currentLocation = Game1.currentLocation?.NameOrUniqueName ?? "";

            if (Game1.player.stamina <= -15f)
            {
                _stateManager.State.WasPassedOut = true;
                _stateManager.State.WasExhausted = true;
                _stateManager.State.LastPassedOutHealth = Game1.player.health;
                _stateManager.State.LastPassedOutLocation = currentLocation;

                _monitor.Log($"Обнаружен обморок от истощения (stamina={Game1.player.stamina})", LogLevel.Info);
            }
            else if (Game1.player.health <= 0)
            {
                _stateManager.State.WasPassedOut = true;
                _stateManager.State.LastPassedOutHealth = 0;
                _stateManager.State.LastPassedOutLocation = currentLocation;

                _monitor.Log($"Обнаружен обморок от урона (health=0)", LogLevel.Info);
            }
            else if (Game1.timeOfDay >= 2600)
            {
                _stateManager.State.WasPassedOut = true;
                _stateManager.State.WasUpTooLate = true;
                _stateManager.State.LastPassedOutHealth = Game1.player.health;
                _stateManager.State.LastPassedOutLocation = currentLocation;

                _monitor.Log($"Обнаружен обморок от позднего времени (time={Game1.timeOfDay})", LogLevel.Info);
            }

            // Событие спасения нужно ТОЛЬКО при боевой гибели в шахте (health=0).
            // Обмороки от усталости или позднего времени — другие триггеры, не спасение.
            bool isMineLocation = currentLocation.Contains("Mine", StringComparison.OrdinalIgnoreCase)
                || currentLocation == "UndergroundMine";
            bool isCombatDeath = _stateManager.State.WasPassedOut
                && !_stateManager.State.WasExhausted
                && !_stateManager.State.WasUpTooLate;

            if (isMineLocation && isCombatDeath && _dialogueManager.IsDatingOrMarriedToHarvey())
            {
                _stateManager.State.PassedOutInMineYesterday = true;
                _stateManager.State.NeedsMineRescueEvent = true;

                _injuryManager.ApplyBadlyHurtFromMinePassOut();
                _stateManager.State.SavedActiveBuffs = _buffManager.GetActiveModBuffs();
                _stateManager.Save();

                _monitor.Log($"[MineRescue] Snapshot баффов обновлён после TrackPassOut fallback: {string.Join(", ", _stateManager.State.SavedActiveBuffs)}", LogLevel.Debug);
                _monitor.Log("[MineRescue] Шахтная смерть зафиксирована в TrackPassOut fallback", LogLevel.Warn);
            }
            else if (isMineLocation && isCombatDeath)
            {
                _monitor.Log("[MineRescue] Боевая смерть в шахте зафиксирована, но rescue Харви не запускается: нет отношений dating/married", LogLevel.Debug);
            }
        }

        /// <summary>
        /// Инициирует событие спасения из шахты на следующее утро.
        /// Вызывается из DayStarted. Телепортирует игрока в шахту, событие запускается
        /// в OnPlayerWarped после прибытия. Событие само добавляет topicMineInjuryRescue.
        /// </summary>
        public void TriggerMineRescueEvents()
        {
            if (!_stateManager.State.NeedsMineRescueEvent) return;

            _monitor.Log("[MineRescue] Подготовка события спасения из шахты", LogLevel.Info);

            if (!_dialogueManager.IsDatingOrMarriedToHarvey())
            {
                _monitor.Log("[MineRescue] Нет отношений с Харви — пропускаем", LogLevel.Debug);
                ClearMineRescueState();
                return;
            }

            // Resume после reload: pending ID уже в save
            if (!string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId))
            {
                _monitor.Log(
                    $"[MineRescue] Resume pending: {_stateManager.State.PendingMineRescueEventId}",
                    LogLevel.Info);
                BeginMineRescueWarp(_stateManager.State.PendingMineRescueEventId);
                return;
            }

            // Боевая смерть в шахте → только major rescue (minor — отдельный триггер в HandleMineEntryWarning)
            bool hasSevere = _buffManager.HasAnyBuff(InjurySets.Severe.ToArray());
            string eventId = ResolveSevereMineRescueEventId();

            _monitor.Log($"[MineRescue] Выбран eventId: {eventId} (серьёзные травмы: {hasSevere})", LogLevel.Info);

            // Если событие уже было — показываем только топик, без повтора кинематики
            if (IsMineRescueEventAlreadySeen(eventId))
            {
                _monitor.Log($"[MineRescue] Событие {eventId} уже просматривалось — добавляем topicMineInjuryRescue", LogLevel.Info);
                EnsureMineRescueTopic();
                ClearMineRescueState();
                return;
            }

            BeginMineRescueWarp(eventId);
        }

        /// <summary>
        /// После загрузки сохранения: retry, если reload случился между warp Mine и startEvent.
        /// </summary>
        public void ResumePendingMineRescueIfNeeded()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (!_stateManager.State.NeedsMineRescueEvent)
                return;

            if (string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId))
                return;

            _monitor.Log("[MineRescue] SaveLoaded: resume pending mine rescue", LogLevel.Info);
            BeginMineRescueWarp(_stateManager.State.PendingMineRescueEventId);
        }

        /// <summary>
        /// После загрузки сохранения: retry hospital pass-out cutscene между warp и startEvent.
        /// </summary>
        public void ResumePendingHospitalPassOutIfNeeded()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (string.IsNullOrEmpty(_stateManager.State.PendingHospitalPassOutEventId))
                return;

            _monitor.Log("[PassOutEvent] SaveLoaded: resume pending hospital pass-out cutscene", LogLevel.Info);
            BeginHospitalPassOutWarp(
                _stateManager.State.PendingHospitalPassOutEventId,
                _stateManager.State.PendingHospitalPassOutFallbackKind);
        }

        /// <summary>
        /// Опасное состояние в шахте без Severe: eventHarveyMinorMineRescue (не боевой death-rescue).
        /// </summary>
        public bool TryTriggerMinorMineRescue()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return false;

            if (!CanTriggerMinorMineRescue())
                return false;

            const string eventId = EventIds.MinorMineRescue;
            int today = Helpers.GameUtils.Today();
            _stateManager.State.LastMinorMineRescueDay = today;

            if (Game1.player.eventsSeen.Contains(eventId))
            {
                _monitor.Log("[MinorMineRescue] Событие уже seen — пропуск cutscene", LogLevel.Debug);
                _stateManager.Save();
                return false;
            }

            if (!LocationEventExists(eventId, "Mine"))
            {
                _monitor.Log("[MinorMineRescue] Событие не найдено в Data/Events/Mine", LogLevel.Warn);
                return false;
            }

            _dialogueManager.AddTopic(ConversationTopics.MineRescuePending, 1);
            _stateManager.Save();

            if (string.Equals(Game1.currentLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
            {
                if (TryStartLocationEvent(eventId, "Mine", OnMinorMineRescueEventFinished))
                    return true;

                RunMinorMineRescueFallback();
                return false;
            }

            var mineLocation = Game1.getLocationFromName("Mine");
            if (mineLocation == null)
            {
                _monitor.Log("[MinorMineRescue] Локация Mine не найдена", LogLevel.Error);
                RunMinorMineRescueFallback();
                return false;
            }

            _stateManager.State.PendingMinorMineRescueEventId = eventId;
            _stateManager.Save();
            _monitor.Log("[MinorMineRescue] Warp в Mine для cutscene", LogLevel.Info);
            Game1.warpFarmer(new LocationRequest("Mine", false, mineLocation), 17, 7, 2);
            return true;
        }

        public void ResumePendingMinorMineRescueIfNeeded()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (string.IsNullOrEmpty(_stateManager.State.PendingMinorMineRescueEventId))
                return;

            _monitor.Log("[MinorMineRescue] SaveLoaded: resume pending minor mine rescue", LogLevel.Info);

            var mineLocation = Game1.getLocationFromName("Mine");
            if (mineLocation == null)
            {
                RunMinorMineRescueFallback();
                return;
            }

            if (!string.Equals(Game1.currentLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
                Game1.warpFarmer(new LocationRequest("Mine", false, mineLocation), 17, 7, 2);
            else if (!TryStartLocationEvent(
                         _stateManager.State.PendingMinorMineRescueEventId,
                         "Mine",
                         OnMinorMineRescueEventFinished))
                RunMinorMineRescueFallback();
        }

        public bool CanTriggerMinorMineRescue()
        {
            if (!_dialogueManager.IsDatingOrMarriedToHarvey())
                return false;

            if (_stateManager.State.NeedsMineRescueEvent)
                return false;

            if (!string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId)
                || !string.IsNullOrEmpty(_stateManager.State.PendingMinorMineRescueEventId))
                return false;

            if (_dialogueManager.HasTopic(ConversationTopics.MineRescuePending))
                return false;

            if (Game1.player.eventsSeen.Contains(EventIds.MinorMineRescue))
                return false;

            int today = Helpers.GameUtils.Today();
            if (_stateManager.State.LastMinorMineRescueDay == today)
                return false;

            if (Game1.eventUp || Game1.activeClickableMenu != null)
                return false;

            if (!IsPlayerInMineOrVolcano(Game1.currentLocation))
                return false;

            if (_buffManager.HasAnyBuff(InjurySets.Severe.ToArray()))
                return false;

            bool hasAnyInjury = _stateManager.GetAllActiveDebuffStates().Count > 0
                || _stateManager.State.ActiveComplications.Count > 0
                || _injuryManager.GetActiveInjury() != null;

            if (!hasAnyInjury)
                return false;

            int healthThreshold = (int)Math.Ceiling(Game1.player.maxHealth * 0.35f);
            bool lowHealth = Game1.player.health <= healthThreshold;
            bool lowStamina = Game1.player.stamina <= Game1.player.maxStamina.Value * 0.15f;

            return lowHealth || lowStamina;
        }

        private static bool IsPlayerInMineOrVolcano(GameLocation? location)
        {
            if (location == null)
                return false;

            string name = location.NameOrUniqueName ?? "";
            return location is MineShaft
                || location is VolcanoDungeon
                || string.Equals(name, "Mine", StringComparison.OrdinalIgnoreCase)
                || name.Contains("SkullCave", StringComparison.OrdinalIgnoreCase);
        }

        private void OnMinorMineRescueEventFinished(string eventId)
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains(eventId))
                    Game1.player.eventsSeen.Add(eventId);

                _dialogueManager.RemoveTopic(ConversationTopics.MineRescuePending);
                _stateManager.State.PendingMinorMineRescueEventId = "";
                _stateManager.State.LastMinorMineRescueDay = Helpers.GameUtils.Today();

                if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.MinorMineRescue))
                    _dialogueManager.AddTopic(ConversationTopics.MinorMineRescue, 2);

                _stateManager.Save();
                _monitor.Log($"[MinorMineRescue] ✅ Событие '{eventId}' завершено", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MinorMineRescue] ❌ onEventFinished: {ex}", LogLevel.Error);
            }
        }

        private void RunMinorMineRescueFallback()
        {
            _dialogueManager.RemoveTopic(ConversationTopics.MineRescuePending);
            _stateManager.State.PendingMinorMineRescueEventId = "";

            if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.MinorMineRescue))
                _dialogueManager.AddTopic(ConversationTopics.MinorMineRescue, 2);

            Game1.addHUDMessage(new HUDMessage(
                "Харви настаивает: в таком состоянии шахта слишком опасна. Отдохни и зайди в клинику.",
                HUDMessage.health_type));

            _stateManager.Save();
        }

        private void BeginMineRescueWarp(string eventId)
        {
            _dialogueManager.AddTopic(ConversationTopics.MineRescuePending, 1);

            var mineLocation = Game1.getLocationFromName("Mine");
            if (mineLocation == null)
            {
                _monitor.Log("[MineRescue] ❌ Локация Mine не найдена, запускаем fallback без кинематики", LogLevel.Error);
                RunMineRescueFallback(eventId);
                return;
            }

            if (string.Equals(Game1.currentLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryStartLocationEvent(eventId, "Mine", OnMineRescueEventFinished))
                    RunMineRescueFallback(eventId);
                return;
            }

            _stateManager.State.PendingMineRescueEventId = eventId;
            _stateManager.Save();
            _monitor.Log($"[MineRescue] Телепортация в шахту для запуска: {eventId}", LogLevel.Info);
            Game1.warpFarmer(new LocationRequest("Mine", false, mineLocation), 17, 7, 2);
        }

        /// <summary>
        /// Ставит в очередь hospital cutscene: warp в Hospital → startEvent в OnPlayerWarped.
        /// Если событие уже seen — только fallback (topic/HUD), без повторной сцены.
        /// </summary>
        private bool QueueHospitalEvent(string eventId, string fallbackKind)
        {
            if (Game1.player.eventsSeen.Contains(eventId))
            {
                _monitor.Log($"[PassOutEvent] {eventId} уже просмотрено — fallback без cutscene", LogLevel.Info);
                RunHospitalPassOutFallback(fallbackKind);
                return true;
            }

            if (!LocationEventExists(eventId, GetHospitalLocationName()))
            {
                _monitor.Log($"[PassOutEvent] {eventId} не найден в Data/Events/{GetHospitalLocationName()}", LogLevel.Warn);
                return false;
            }

            BeginHospitalPassOutWarp(eventId, fallbackKind);
            return true;
        }

        private void BeginHospitalPassOutWarp(string eventId, string fallbackKind)
        {
            string hospitalName = GetHospitalLocationName();
            var hospitalLocation = Game1.getLocationFromName(hospitalName);
            if (hospitalLocation == null)
            {
                _monitor.Log($"[PassOutEvent] ❌ Локация '{hospitalName}' не найдена — fallback", LogLevel.Error);
                RunHospitalPassOutFallback(fallbackKind);
                return;
            }

            if (IsHospitalLocation(Game1.currentLocation))
            {
                _stateManager.State.PendingHospitalPassOutEventId = eventId;
                _stateManager.State.PendingHospitalPassOutFallbackKind = fallbackKind;
                _stateManager.Save();

                if (!TryStartLocationEvent(eventId, hospitalName, OnHospitalPassOutEventFinished))
                    RunHospitalPassOutFallback(fallbackKind);

                return;
            }

            _stateManager.State.PendingHospitalPassOutEventId = eventId;
            _stateManager.State.PendingHospitalPassOutFallbackKind = fallbackKind;
            _stateManager.Save();

            int bedX = Math.Max(0, _config.HospitalBedX);
            int bedY = Math.Max(0, _config.HospitalBedY);
            _monitor.Log($"[PassOutEvent] Warp в {hospitalName} ({bedX},{bedY}) для {eventId}", LogLevel.Info);
            Game1.warpFarmer(hospitalName, bedX, bedY, 0);
        }

        private void RunHospitalPassOutFallback(string fallbackKind)
        {
            switch (fallbackKind)
            {
                case FallbackCritical:
                    RunCriticalPassOutFallback();
                    break;
                case FallbackExhaustion:
                    RunExhaustionPassOutFallback();
                    break;
                default:
                    _monitor.Log($"[PassOutEvent] Неизвестный fallback '{fallbackKind}'", LogLevel.Warn);
                    break;
            }

            ClearHospitalPassOutPending();
        }

        private void RunCriticalPassOutFallback()
        {
            Game1.addHUDMessage(new HUDMessage(
                "Ты чувствуешь себя очень плохо после потери сознания...",
                HUDMessage.error_type));
        }

        private void RunExhaustionPassOutFallback()
        {
            _buffManager.AddBuff("buffFarmerExhausted", -2);
            _dialogueManager.AddTopic(ConversationTopics.FarmerExhausted, 3);
            Game1.playSound("debuffHit");
            Game1.addHUDMessage(new HUDMessage("Ты полностью измотана...", HUDMessage.health_type));
        }

        private void ClearHospitalPassOutPending()
        {
            _stateManager.State.PendingHospitalPassOutEventId = "";
            _stateManager.State.PendingHospitalPassOutFallbackKind = "";
            _stateManager.Save();
        }

        private void ClearPassOutFlags()
        {
            _stateManager.State.WasPassedOut = false;
            _stateManager.State.WasExhausted = false;
            _stateManager.State.WasUpTooLate = false;
            _stateManager.State.LastPassedOutHealth = -1;
            _stateManager.State.LastPassedOutLocation = "";
            _stateManager.Save();
        }

        private bool IsMineRelatedPassOut()
        {
            if (_stateManager.State.PassedOutInMineYesterday || _stateManager.State.NeedsMineRescueEvent)
                return true;

            string location = _stateManager.State.LastPassedOutLocation ?? "";
            return location.Contains("Mine", StringComparison.OrdinalIgnoreCase)
                || location.Contains("SkullCave", StringComparison.OrdinalIgnoreCase)
                || string.Equals(location, "UndergroundMine", StringComparison.OrdinalIgnoreCase);
        }

        private string GetHospitalLocationName() => _config.HospitalLocationName;

        private bool IsHospitalLocation(GameLocation? location)
        {
            return string.Equals(location?.NameOrUniqueName, GetHospitalLocationName(), StringComparison.OrdinalIgnoreCase);
        }

        private void OnHospitalPassOutEventFinished(string eventId)
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains(eventId))
                    Game1.player.eventsSeen.Add(eventId);

                ClearHospitalPassOutPending();
                ClearPassOutFlags();

                _monitor.Log($"[PassOutEvent] ✅ Событие '{eventId}' завершено", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[PassOutEvent] ❌ Ошибка в onEventFinished для '{eventId}': {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Аварийный сценарий, если CP-событие спасения не найдено или не стартовало.
        /// </summary>
        private void RunMineRescueFallback(string eventId)
        {
            _monitor.Log($"[MineRescue] Fallback для {eventId}: topic + перенос в госпиталь", LogLevel.Warn);

            EnsureMineRescueTopic();
            Game1.addHUDMessage(new HUDMessage("Харви нашёл тебя после обморока в шахте. Нужно срочно в клинику.", HUDMessage.error_type));

            string hospital = _config.HospitalLocationName;
            if (Game1.getLocationFromName(hospital) == null)
            {
                _monitor.Log(
                    $"[MineRescue] ❌ Локация '{hospital}' не найдена — только topic, NeedsMineRescueEvent остаётся для retry",
                    LogLevel.Error);
                _stateManager.State.PendingMineRescueEventId = "";
                _stateManager.Save();
                return;
            }

            ClearMineRescueState();
            Game1.warpFarmer(hospital, Math.Max(0, _config.HospitalBedX), Math.Max(0, _config.HospitalBedY), 0);
        }

        private void EnsureMineRescueTopic()
        {
            if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue))
                _dialogueManager.AddTopic(ConversationTopics.MineInjuryRescue, 2);
        }

        private void ClearMineRescueState()
        {
            _dialogueManager.RemoveTopic(ConversationTopics.MineRescuePending);
            _stateManager.State.PendingMineRescueEventId = "";
            _stateManager.State.NeedsMineRescueEvent = false;
            _stateManager.State.PassedOutInMineYesterday = false;
            _stateManager.State.WasPassedOut = false;
            _stateManager.State.WasExhausted = false;
            _stateManager.State.WasUpTooLate = false;
            _stateManager.State.LastPassedOutHealth = -1;
            _stateManager.State.LastPassedOutLocation = "";
            _stateManager.Save();
        }

        private void OnMineRescueEventFinished(string eventId)
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains(eventId))
                    Game1.player.eventsSeen.Add(eventId);

                EnsureMineRescueTopic();
                ClearMineRescueState();

                _monitor.Log($"[MineRescue] ✅ Событие '{eventId}' завершено — eventsSeen и флаги обновлены", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MineRescue] ❌ Ошибка в onEventFinished для '{eventId}': {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Severe + dating/married: dating-сцена, если есть в CP; иначе legacy major.
        /// </summary>
        private string ResolveSevereMineRescueEventId()
        {
            const string datingEvent = "eventHarveyMineRescueDating";
            const string legacyEvent = "eventHarveyMineRescue";

            if (MineRescueEventExists(datingEvent))
                return datingEvent;

            _monitor.Log(
                $"[MineRescue] {datingEvent} не найден в Data/Events/Mine — fallback на {legacyEvent}",
                LogLevel.Warn);
            return legacyEvent;
        }

        private static bool MineRescueEventExists(string eventId)
            => LocationEventExists(eventId, "Mine");

        /// <summary>
        /// Проверяет наличие entry в Data/Events/{locationName}.
        /// </summary>
        private static bool LocationEventExists(string eventId, string locationName)
        {
            var eventData = Game1.content.Load<System.Collections.Generic.Dictionary<string, string>>($"Data/Events/{locationName}");
            if (eventData == null)
                return false;

            foreach (string key in eventData.Keys)
            {
                if (EventKeyMatches(key, eventId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Уже просмотрено это событие или legacy major (для dating-ветки до патча).
        /// </summary>
        private static bool IsMineRescueEventAlreadySeen(string eventId)
        {
            if (Game1.player.eventsSeen.Contains(eventId))
                return true;

            if (eventId == "eventHarveyMineRescueDating"
                && Game1.player.eventsSeen.Contains("eventHarveyMineRescue"))
                return true;

            return false;
        }

        /// <summary>
        /// Запускает событие по его ID в указанной локации.
        /// Предполагает, что игрок уже находится в этой локации.
        /// </summary>
        private static bool EventKeyMatches(string key, string eventId)
        {
            return key.Equals(eventId, StringComparison.OrdinalIgnoreCase)
                || key.StartsWith(eventId + "/", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryStartLocationEvent(string eventId, string locationName, Action<string> onFinished)
        {
            try
            {
                var location = Game1.getLocationFromName(locationName);
                if (location == null)
                {
                    _monitor.Log($"[PassOutEvent] ❌ Локация '{locationName}' не найдена", LogLevel.Error);
                    return false;
                }

                var eventData = Game1.content.Load<System.Collections.Generic.Dictionary<string, string>>($"Data/Events/{locationName}");
                if (eventData == null)
                {
                    _monitor.Log($"[PassOutEvent] ❌ Data/Events/{locationName} не найдены", LogLevel.Error);
                    return false;
                }

                string? eventScript = null;
                foreach (var kvp in eventData)
                {
                    if (EventKeyMatches(kvp.Key, eventId))
                    {
                        eventScript = kvp.Value;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(eventScript))
                {
                    _monitor.Log($"[PassOutEvent] ❌ Событие '{eventId}' не найдено или script пуст в Data/Events/{locationName}", LogLevel.Error);
                    return false;
                }

                if (!ReferenceEquals(Game1.currentLocation, location)
                    && !string.Equals(Game1.currentLocation?.NameOrUniqueName, locationName, StringComparison.OrdinalIgnoreCase))
                {
                    _monitor.Log(
                        $"[PassOutEvent] ❌ Игрок не в '{locationName}' (текущая: {Game1.currentLocation?.NameOrUniqueName})",
                        LogLevel.Error);
                    return false;
                }

                try
                {
                    var gameEvent = new Event(eventScript);
                    string capturedId = eventId;
                    gameEvent.onEventFinished += () => onFinished(capturedId);
                    location.startEvent(gameEvent);
                    _monitor.Log(
                        $"[PassOutEvent] ✅ Событие '{eventId}' запущено; eventsSeen — по onEventFinished",
                        LogLevel.Info);
                    return true;
                }
                catch (Exception ex)
                {
                    _monitor.Log($"[PassOutEvent] ❌ Ошибка запуска события '{eventId}': {ex}", LogLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"[PassOutEvent] ❌ Ошибка при подготовке '{eventId}': {ex.Message}", LogLevel.Error);
                return false;
            }
        }
    }
}
