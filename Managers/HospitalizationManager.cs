using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление госпитализацией игрока
    /// </summary>
    public class HospitalizationManager
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly DialogueManager _dialogueManager;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private HospitalActivityManager? _activityManager;
        private TreatmentManager? _treatmentManager;
        private bool _pendingReturnToHospital;
        private bool _pendingBlockedExitReaction;

        /// <summary>
        /// Ожидается отложенный возврат в палату после заблокированного выхода.
        /// </summary>
        public bool HasPendingReturnToHospital => _pendingReturnToHospital;

        public HospitalizationManager(
            IMonitor monitor,
            ModConfig config,
            DialogueManager dialogueManager,
            StateManager stateManager,
            BuffManager buffManager)
        {
            _monitor = monitor;
            _config = config;
            _dialogueManager = dialogueManager;
            _stateManager = stateManager;
            _buffManager = buffManager;
        }

        /// <summary>
        /// Установить менеджер активностей (вызывается после инициализации)
        /// </summary>
        public void SetActivityManager(HospitalActivityManager activityManager)
        {
            _activityManager = activityManager;
        }

        /// <summary>
        /// Связать TreatmentManager (вызывается после инициализации).
        /// </summary>
        public void SetTreatmentManager(TreatmentManager treatmentManager)
        {
            _treatmentManager = treatmentManager;
        }

        /// <summary>
        /// Госпитализация активна
        /// </summary>
        public bool IsHospitalized => _stateManager.State.IsHospitalized;

        /// <summary>
        /// Текущая травма, по которой госпитализирован
        /// </summary>
        public string? CurrentInjury => string.IsNullOrEmpty(_stateManager.State.HospitalizedInjuryId)
            ? null
            : _stateManager.State.HospitalizedInjuryId;

        /// <summary>
        /// Начать принудительную госпитализацию
        /// </summary>
        public void StartForcedHospitalization(string injuryId, NPC? harvey = null)
        {
            StartForcedHospitalizationWithExplanation(injuryId, harvey, "general");
        }

        /// <summary>
        /// Перед госпитализацией: начать лечение, если первичный осмотр ещё не проведён.
        /// </summary>
        public void EnsureTreatmentBeforeForcedHospitalization(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                _monitor.Log(
                    $"[Hospital] pre-hospital treatment skipped: no DebuffState for {injuryId}",
                    LogLevel.Debug);
                return;
            }

            if (debuffState.TreatmentStarted)
            {
                _monitor.Log(
                    $"[Hospital] treatment already started, skip pre-hospital treatment: {injuryId}",
                    LogLevel.Info);
                return;
            }

            if (_treatmentManager == null)
            {
                _monitor.Log(
                    $"[Hospital] pre-hospital treatment skipped: TreatmentManager not wired for {injuryId}",
                    LogLevel.Error);
                return;
            }

            _monitor.Log(
                $"[Hospital] starting treatment before forced hospitalization: {injuryId}",
                LogLevel.Info);

            string topicId = TopicIds.GetInjuryTopic(injuryId);
            bool hadUntreatedTopic = _dialogueManager.HasTopic(topicId);

            _treatmentManager.ApplyTreatmentForInjury(injuryId);

            if (hadUntreatedTopic)
            {
                _monitor.Log($"[Hospital] removed untreated topic: {topicId}", LogLevel.Info);
            }

            _stateManager.MarkHarveyConversation(injuryId, true);
            _dialogueManager.ClearHarveyNeedsFirstTreatmentTopic(
                "лечение начато перед принудительной госпитализацией");
            _stateManager.Save();

            _dialogueManager.AddTopic(ConversationTopics.ForcedHosp, 2);
        }

        /// <summary>
        /// Начать принудительную госпитализацию с объяснением причины
        /// </summary>
        public void StartForcedHospitalizationWithExplanation(string injuryId, NPC? harvey, string reason)
        {
            EnsureTreatmentBeforeForcedHospitalization(injuryId);

            if (!_dialogueManager.HasTopic(ConversationTopics.ForcedHosp))
                _dialogueManager.AddTopic(ConversationTopics.ForcedHosp, 2);

            _monitor.Log($"🏥 Начинаем принудительную госпитализацию: {injuryId}, причина: {reason}", LogLevel.Info);

            var state = _stateManager.State;
            state.IsHospitalized = true;
            state.HospitalizedInjuryId = injuryId;
            state.HospitalizationReason = reason;
            state.HospitalAdmissionDay = GameUtils.Today();
            state.HospitalAdmissionTime = Game1.timeOfDay;
            state.HospitalAdmissionMinutes = ToClockMinutes(Game1.timeOfDay);
            state.HospitalMinStayMinutes = CalculateHospitalStayMinutes(injuryId, reason);
            state.HospitalDischargeReadyShown = false;
            state.PendingForcedHospitalizationWarning = false;
            _stateManager.Save();

            // Телепортируем в больницу
            WarpToHospitalBed();

            // Показываем объяснение в зависимости от причины
            ShowHospitalizationExplanation(harvey, reason);

            Game1.playSound("debuffHit");
        }

        /// <summary>
        /// Показать объяснение причины госпитализации
        /// </summary>
        private void ShowHospitalizationExplanation(NPC? harvey, string reason)
        {
            string explanation = reason switch
            {
                "mine_rescue" => GetMineRescueExplanation(),
                "general" => GetGeneralExplanation(),
                _ => GetGeneralExplanation()
            };

            if (harvey != null && Game1.currentLocation.characters.Contains(harvey))
            {
                harvey.CurrentDialogue.Clear();
                harvey.CurrentDialogue.Push(new Dialogue(harvey, null, explanation));
                Game1.drawDialogue(harvey);
            }
            else
            {
                Game1.drawObjectDialogue(explanation);
            }
        }

        /// <summary>
        /// Получить объяснение для случая спасения из шахты
        /// </summary>
        private string GetMineRescueExplanation()
        {
            int hours = _stateManager.State.HospitalMinStayMinutes / 60;
            
            return $"@! Твои раны после вчерашнего инцидента в шахте очень серьёзны.$a#$b#" +
                   $"Я не могу отпустить тебя в таком состоянии!$a#$b#" +
                   $"Ты остаёшься здесь под моим наблюдением минимум на {hours} {GetHoursWord(hours)}.$u#$b#" +
                   $"Я буду следить за твоим состоянием и обрабатывать раны.$l#$b#" +
                   $"Это не обсуждается.$a";
        }

        /// <summary>
        /// Получить общее объяснение
        /// </summary>
        private string GetGeneralExplanation()
        {
            int hours = _stateManager.State.HospitalMinStayMinutes / 60;
            
            return $"@! Твоё состояние критическое. Немедленно в палату!$a#$b#" +
                   $"Тебе нужен постельный режим минимум на {hours} {GetHoursWord(hours)}.$u#$b#" +
                   $"Я буду рядом.$l#$b#" +
                   $"Это не обсуждается.$a";
        }

        /// <summary>
        /// Получить правильное склонение слова "час"
        /// </summary>
        private string GetHoursWord(int hours)
        {
            if (hours == 1) return "час";
            if (hours >= 2 && hours <= 4) return "часа";
            return "часов";
        }

        /// <summary>
        /// Проверить, можно ли выписать пациента
        /// </summary>
        public bool CanDischarge()
        {
            if (!IsHospitalized) return true;

            var state = _stateManager.State;
            int admissionMinutes = state.HospitalAdmissionMinutes;
            if (admissionMinutes < 0 && state.HospitalAdmissionTime >= 0)
                admissionMinutes = ToClockMinutes(state.HospitalAdmissionTime);

            if (admissionMinutes < 0)
                return false;

            int now = ToClockMinutes(Game1.timeOfDay);
            int elapsed = now - admissionMinutes;
            if (elapsed < 0)
                elapsed += 24 * 60;

            int minStay = state.HospitalMinStayMinutes > 0
                ? state.HospitalMinStayMinutes
                : _config.MinHospitalStayMinutes;

            return elapsed >= minStay;
        }

        /// <summary>
        /// Однократно уведомить игрока, что минимальный срок госпитализации прошёл.
        /// </summary>
        public void NotifyDischargeReadyIfNeeded()
        {
            if (!IsHospitalized) return;
            if (!CanDischarge()) return;

            var state = _stateManager.State;
            if (state.HospitalDischargeReadyShown) return;

            state.HospitalDischargeReadyShown = true;
            _stateManager.Save();

            _monitor.Log("✅ Минимальный срок госпитализации прошёл, игрок может выписаться", LogLevel.Debug);

            Game1.addHUDMessage(new HUDMessage(
                "Харви разрешил выписку. Но сегодня без риска.",
                HUDMessage.health_type));

            NPC? harvey = HarveyHelper.FindHarvey(Game1.currentLocation);
            if (harvey != null)
            {
                _dialogueManager.ShowEmoteWithText(
                    harvey,
                    HarveyEmotes.Recovery,
                    "Показатели стабильны. Можешь идти, но без шахты сегодня.");
            }
        }

        /// <summary>
        /// Выписать пациента
        /// </summary>
        public void Discharge()
        {
            _monitor.Log("🏥 Выписка пациента", LogLevel.Info);

            string? injuryId = CurrentInjury;
            ClearHospitalizationState();
            _stateManager.Save();

            if (string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                ReplaceIntensiveCareWithOutpatientRecovery();

            // Сбрасываем активности госпитализации
            _activityManager?.Reset();
        }

        private void ReplaceIntensiveCareWithOutpatientRecovery()
        {
            _buffManager.RemoveBuff(CureBuffs.IntensiveCare);
            _buffManager.AddBuff(CureBuffs.BadlyHurtOutpatientCare, -2);
            _monitor.Log(
                "[Hospital] buffBadlyHurt: intensive care replaced with outpatient recovery after discharge",
                LogLevel.Info);
        }

        /// <summary>
        /// Телепортировать игрока к больничной кровати
        /// </summary>
        public void WarpToHospitalBed()
        {
            string loc = _config.HospitalLocationName;
            int x = Math.Max(0, _config.HospitalBedX);
            int y = Math.Max(0, _config.HospitalBedY);

            if (string.Equals(Game1.currentLocation?.Name, loc, StringComparison.OrdinalIgnoreCase))
            {
                Game1.player.setTileLocation(new Vector2(x, y));
                Game1.player.faceDirection(0);
                return;
            }

            Game1.warpFarmer(loc, x, y, 0);
        }

        /// <summary>
        /// Проверить, находится ли игрок в клинике
        /// </summary>
        public bool IsInClinic(GameLocation? location = null)
        {
            location ??= Game1.currentLocation;
            return string.Equals(location?.Name, _config.HospitalLocationName, 
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Проверить, занята ли игра событием, диалогом или меню.
        /// </summary>
        private bool IsGameBusy()
        {
            return Game1.eventUp
                || Game1.CurrentEvent != null
                || Game1.activeClickableMenu != null
                || !Context.IsPlayerFree;
        }

        /// <summary>
        /// Отложенный возврат в палату, когда игрок свободен от событий и диалогов.
        /// </summary>
        public void UpdateHospitalizationLock()
        {
            if (!IsHospitalized)
            {
                _pendingReturnToHospital = false;
                _pendingBlockedExitReaction = false;
                return;
            }

            if (CanDischarge())
                return;

            bool outsideHospital = !IsInClinic(Game1.currentLocation);
            if (outsideHospital)
            {
                _pendingReturnToHospital = true;
                _pendingBlockedExitReaction = true;
            }

            if (!_pendingReturnToHospital)
                return;

            if (!Context.IsWorldReady)
                return;

            if (IsGameBusy())
                return;

            WarpToHospitalBed();

            if (!IsInClinic(Game1.currentLocation))
                return;

            _pendingReturnToHospital = false;

            if (!_pendingBlockedExitReaction)
                return;

            _pendingBlockedExitReaction = false;

            NPC? harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null && Game1.currentLocation.characters.Contains(harvey))
            {
                _dialogueManager.ShowFullReaction(
                    harvey,
                    HarveyEmotes.StayInBed,
                    HarveyTextMessages.DontMove,
                    "Назад в палату. Я не обсуждаю это.$a");
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("Харви не разрешает тебе покидать палату.", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// Обработать попытку покинуть больницу во время госпитализации.
        /// Блокируется только ВЫХОД из клиники — переходы между другими локациями
        /// (например, выход из шахты) не блокируются.
        /// </summary>
        public bool HandleWarpAttempt(GameLocation newLocation, GameLocation oldLocation)
        {
            if (!IsHospitalized) return false;

            // Блокируем только когда игрок уходит ИМЕННО из клиники
            bool wasInClinic  = IsInClinic(oldLocation);
            bool goingOutside = !IsInClinic(newLocation);

            if (!wasInClinic || !goingOutside)
                return false; // Варп не из клиники — не блокируем

            // Игрок пытается уйти из больницы
            if (CanDischarge())
            {
                _monitor.Log("✅ Игрок покидает больницу после окончания срока", LogLevel.Info);
                Discharge();

                NPC? harvey = Game1.getCharacterFromName("Harvey");
                if (harvey != null && Game1.currentLocation.characters.Contains(harvey))
                {
                    _dialogueManager.ShowFullReaction(harvey,
                        HarveyEmotes.FullRecovery,
                        HarveyTextMessages.GoodProgress,
                        "Ты молодец. Показатели в норме.$h#$b#Но будь осторожнее в следующий раз!$s");
                }

                return false; // Разрешаем варп
            }

            _monitor.Log("🏥 Попытка покинуть больницу заблокирована — отложенный возврат", LogLevel.Debug);

            _pendingReturnToHospital = true;
            _pendingBlockedExitReaction = true;
            Game1.addHUDMessage(new HUDMessage("Тебе пока нельзя покидать больницу.", HUDMessage.error_type));
            return true;
        }

        /// <summary>
        /// Показать реакцию выздоровления
        /// </summary>
        public void ShowRecoveryReaction(NPC harvey, bool fullRecovery = false)
        {
            int emote = fullRecovery ? HarveyEmotes.FullRecovery : HarveyEmotes.Recovery;
            string textMessage = TextMessageSelector.ForRecovery(fullRecovery);
            
            _dialogueManager.ShowEmoteWithText(harvey, emote, textMessage);
            
            string sound = fullRecovery ? "yoba" : "healSound";
            Game1.playSound(sound);
            
            _monitor.Log($"😊 Показана реакция выздоровления (полное={fullRecovery})", LogLevel.Debug);
        }

        private static int ToClockMinutes(int timeOfDay)
        {
            int hours = timeOfDay / 100;
            int minutes = timeOfDay % 100;
            return hours * 60 + minutes;
        }

        private int CalculateHospitalStayMinutes(string injuryId, string reason)
        {
            int baseMinutes = injuryId switch
            {
                "buffBadlyHurt" => 90,
                "buffBurnWounds" => 120,
                "buffShrapnelWounds" => 120,
                "buffSurgicalWound" => 120,
                "buffConcussion" => 180,
                "buffInfectedWound" => 180,
                "buffFracturedBone" => 180,
                _ => _config.MinHospitalStayMinutes
            };

            if (reason == "mine_rescue")
                return Math.Max(baseMinutes, 120);

            if (reason == "infection_fever")
                return Math.Max(baseMinutes, 180);

            return baseMinutes;
        }

        private void ClearHospitalizationState()
        {
            _pendingReturnToHospital = false;
            _pendingBlockedExitReaction = false;

            var state = _stateManager.State;
            state.IsHospitalized = false;
            state.HospitalizedInjuryId = "";
            state.HospitalizationReason = "";
            state.HospitalAdmissionDay = -1;
            state.HospitalAdmissionTime = -1;
            state.HospitalAdmissionMinutes = -1;
            state.HospitalMinStayMinutes = 120;
            state.HospitalDischargeReadyShown = false;
        }
    }
}
