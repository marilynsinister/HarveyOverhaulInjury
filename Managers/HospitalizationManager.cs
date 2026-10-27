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
        private DoctorVisitReminderManager? _doctorVisitReminderManager;
        private RecoveryPlanManager? _recoveryPlanManager;
        private bool _pendingReturnToHospital;
        private bool _pendingBlockedExitReaction;
        private bool _returnWarpInFlight;
        private int _returnWarpStartedTick = -1;
        private const int ReturnWarpTimeoutTicks = 180;
        private const int HospitalizedBuffMinDurationMinutes = 1;
        private const int MaxHospitalClockDeltaMinutes = 120;

        private bool _hospitalHoldActive;

        public const string StayModeDay = "day";
        public const string StayModeOvernight = "overnight";

        /// <summary>Объяснение Харви ждёт, пока пациент окажется в палате и будет свободен.</summary>
        private string? _admissionIntroReason;
        private bool _admissionPromptPending;
        private string? _currentHospitalCaseInjury;
        private bool _dischargeAllowed;

        /// <summary>
        /// Ожидается отложенный возврат в палату после заблокированного выхода.
        /// </summary>
        public bool HasPendingReturnToHospital => _pendingReturnToHospital;

        public int HospitalStayProgressMinutes => _stateManager.State.HospitalStayProgressMinutes;
        public int HospitalElapsedMinutes => HospitalStayProgressMinutes;
        public bool DischargeAllowed => _dischargeAllowed;
        public int LastHospitalTimeOfDay => _stateManager.State.LastHospitalTimeOfDay;
        public int AdmissionDay => _stateManager.State.HospitalAdmissionDay;
        public int MinHospitalStayMinutes => GetMinStayMinutes();

        /// <summary>Причина госпитализации после обморока от запущенной травмы (NeglectLadderManager).</summary>
        public const string NeglectCollapseReason = "neglect_collapse";

        public static string BuildCaseId(string injuryId, string reason, int day) =>
            $"{injuryId}|{reason}|{day}";

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

        public void SetDoctorVisitReminderManager(DoctorVisitReminderManager doctorVisitReminderManager)
        {
            _doctorVisitReminderManager = doctorVisitReminderManager;
        }

        public void SetRecoveryPlanManager(RecoveryPlanManager recoveryPlanManager)
        {
            _recoveryPlanManager = recoveryPlanManager;
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
            var state = _stateManager.State;
            int today = GameUtils.Today();
            string caseId = BuildCaseId(injuryId, reason, today);

            if (state.DischargedToday
                && string.Equals(state.LastHospitalDischargeInjuryId, injuryId, StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log(
                    $"[Hospital] Пропуск повторной госпитализации {injuryId} — уже выписан сегодня",
                    LogLevel.Debug);
                return;
            }

            if (string.Equals(state.HospitalizationCompletedCaseId, caseId, StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log($"[Hospital] Пропуск — кейс {caseId} уже завершён", LogLevel.Debug);
                return;
            }

            EnsureTreatmentBeforeForcedHospitalization(injuryId);

            if (!_dialogueManager.HasTopic(ConversationTopics.ForcedHosp))
                _dialogueManager.AddTopic(ConversationTopics.ForcedHosp, 2);

            _monitor.Log($"🏥 Начинаем принудительную госпитализацию: {injuryId}, причина: {reason}", LogLevel.Info);

            state.IsHospitalized = true;
            state.HospitalizedInjuryId = injuryId;
            state.HospitalizationReason = reason;
            state.HospitalAdmissionDay = today;
            state.HospitalAdmissionTime = Game1.timeOfDay;
            state.HospitalAdmissionMinutes = ToClockMinutes(Game1.timeOfDay);
            state.HospitalMinStayMinutes = CalculateHospitalStayMinutes(injuryId, reason);
            state.HospitalDischargeReadyShown = false;
            state.PendingForcedHospitalizationWarning = false;
            state.HospitalLastStatusHudMinute = -1;
            state.HospitalStayProgressMinutes = 0;
            state.LastHospitalTimeOfDay = Game1.timeOfDay;
            state.HospitalizationCaseId = caseId;
            state.DischargedToday = false;
            state.HospitalOvernightCreditDay = -1;
            state.HospitalStayMode = IsOvernightAdmission(state.HospitalMinStayMinutes)
                ? StayModeOvernight
                : StayModeDay;
            _stateManager.Save();

            _hospitalHoldActive = true;
            _currentHospitalCaseInjury = injuryId;
            _dischargeAllowed = false;
            _activityManager?.Reset();

            _monitor.Log(
                $"[Hospital] режим пребывания: {state.HospitalStayMode} (поступление {Game1.timeOfDay}, срок {state.HospitalMinStayMinutes} мин)",
                LogLevel.Info);

            ApplyHospitalizedBuff(Game1.timeOfDay);

            if (reason.Contains("mine", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("critical", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("rescue", StringComparison.OrdinalIgnoreCase))
            {
                MineForbiddenHelper.ApplyHardMineForbidden(
                    state,
                    _config,
                    _buffManager,
                    _stateManager,
                    _monitor,
                    GameUtils.Today(),
                    $"hospital_{reason}");
            }

            // Объяснение Харви и вопрос «чем займёмся» — когда пациент уже в палате и свободен.
            _admissionIntroReason = reason;
            _admissionPromptPending = false;

            if (IsInEventOrWarp())
            {
                // Событие/фестиваль: не ломаем катсцену — блокировка вернёт в палату, когда оно закончится.
                _pendingReturnToHospital = true;
                _monitor.Log("[Hospital] поступление во время события — варп в палату отложен", LogLevel.Info);
            }
            else
            {
                WarpToHospitalBed();
            }

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
                NeglectCollapseReason => GetNeglectCollapseExplanation(),
                "general" => GetGeneralExplanation(),
                _ => GetGeneralExplanation()
            };

            _dialogueManager.SpeakHarvey(explanation, harvey);
        }

        /// <summary>Фраза о сроке: дневной постельный режим или ночь в палате.</summary>
        private string GetStayLine()
        {
            var state = _stateManager.State;
            if (string.Equals(state.HospitalStayMode, StayModeOvernight, StringComparison.OrdinalIgnoreCase))
                return "Уже поздно — останешься на ночь. Утром посмотрим, как ты.$u#$b#";

            int minutes = state.HospitalMinStayMinutes;
            string duration = minutes == 90
                ? "полтора часа"
                : $"{minutes / 60} {GetHoursWord(minutes / 60)}";
            return $"Побудь здесь, под присмотром, хотя бы {duration}.$u#$b#";
        }

        /// <summary>
        /// Получить объяснение для случая спасения из шахты
        /// </summary>
        private string GetMineRescueExplanation()
        {
            return $"Твои раны после вчерашнего в шахте серьёзнее, чем кажется.$s#$b#" +
                   $"Я не хочу отпускать тебя в таком состоянии.$s#$b#" +
                   GetStayLine() +
                   $"Я буду рядом — обработаю раны и прослежу, чтобы всё заживало как надо.$l#$b#" +
                   $"Если станет хуже — сразу зови меня.$l";
        }

        /// <summary>Обморок от запущенной травмы (лестница последствий, ступень 5): тревога, а не упрёк.</summary>
        private string GetNeglectCollapseExplanation()
        {
            return $"Ты упала прямо посреди работы. Тело просто сказало «хватит» — травма зашла слишком далеко.$s#$b#" +
                   $"Я тебя не ругаю, правда. Я испугался, вот и всё.$s#$b#" +
                   GetStayLine() +
                   $"Рану я уже обработал. Остальное сделаем вместе, по шагу в день.$l";
        }

        /// <summary>
        /// Получить общее объяснение
        /// </summary>
        private string GetGeneralExplanation()
        {
            return $"Тише, тише... Я здесь. Сейчас — в палату, ладно?$s#$b#" +
                   GetStayLine() +
                   $"Я буду рядом.$l#$b#" +
                   $"Мне так спокойнее — правда.$s";
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
        /// Синхронизировать UI-бафф госпитализации при смене игрового времени / после загрузки сейва.
        /// </summary>
        public void SyncHospitalizedBuffOnTimeChanged(int timeOfDay, bool showHudReminder = true)
        {
            if (!IsHospitalized)
            {
                ResetHospitalStayCounters();
                SanitizeOrphanHospitalizedBuff();
                return;
            }

            int remaining = GetRemainingMinutes();
            RefreshHospitalizedBuffDuration(remaining);

            if (showHudReminder
                && HospitalizationHelper.ShouldShowStatusHud(timeOfDay, _stateManager.State.HospitalLastStatusHudMinute))
            {
                ShowHospitalizedStatusHud(timeOfDay, remaining);
            }
        }

        /// <summary>Убрать orphan HarveyMod_Hospitalized, если госпитализация не активна.</summary>
        public void SanitizeOrphanHospitalizedBuff()
        {
            if (IsHospitalized)
                return;

            RemoveHospitalizedBuffFromPlayerAndSaved();
        }

        /// <summary>Восстановить UI-бафф после сна / перезагрузки по флагу IsHospitalized.</summary>
        public void RestoreHospitalizedBuffIfActive()
        {
            if (!IsHospitalized)
                return;

            RestoreHospitalStayCounters();
            SyncHospitalizedBuffOnTimeChanged(Game1.timeOfDay, showHudReminder: false);
        }

        /// <summary>
        /// Накопить игровые минуты госпитализации по сменам timeOfDay (HHMM), устойчиво к откату времени.
        /// </summary>
        public void UpdateHospitalStayProgress(int oldTimeOfDay, int newTimeOfDay)
        {
            if (!_hospitalHoldActive || _dischargeAllowed || !IsHospitalized)
                return;

            var state = _stateManager.State;
            int oldMinutes = ToClockMinutes(oldTimeOfDay);
            int newMinutes = ToClockMinutes(newTimeOfDay);

            if (state.LastHospitalTimeOfDay >= 0 && oldTimeOfDay != state.LastHospitalTimeOfDay)
                oldMinutes = ToClockMinutes(state.LastHospitalTimeOfDay);

            if (newMinutes > oldMinutes)
            {
                int delta = newMinutes - oldMinutes;
                if (delta <= MaxHospitalClockDeltaMinutes)
                {
                    state.HospitalStayProgressMinutes += delta;
                    _stateManager.Save();
                }
                else
                {
                    _monitor.Log($"Hospital time jump ignored (delta={delta} min)", LogLevel.Debug);
                }
            }
            else if (newMinutes <= oldMinutes)
            {
                _monitor.Log(
                    "time moved backwards or wrapped, ignoring negative delta",
                    LogLevel.Debug);
            }

            state.LastHospitalTimeOfDay = newTimeOfDay;

            if (state.HospitalStayProgressMinutes < GetMinStayMinutes())
                return;

            _dischargeAllowed = true;
            _stateManager.Save();
            _monitor.Log("Минимальный срок госпитализации прошёл, можно выписаться", LogLevel.Debug);
        }

        /// <summary>
        /// Игрок поспал в палате: время уже прокручено, засчитываем его в срок пребывания.
        /// LastHospitalTimeOfDay = текущее время, чтобы TimeChanged не посчитал скачок второй раз.
        /// </summary>
        public void ApplyRestedMinutes(int minutes)
        {
            if (!IsHospitalized || minutes <= 0)
                return;

            var state = _stateManager.State;
            state.HospitalStayProgressMinutes += minutes;
            state.LastHospitalTimeOfDay = Game1.timeOfDay;
            _dischargeAllowed = state.HospitalStayProgressMinutes >= GetMinStayMinutes();
            _stateManager.Save();
            NotifyDischargeReadyIfNeeded();
        }

        // ============================================================================
        // НОЧЬ В ПАЛАТЕ
        // ============================================================================

        /// <summary>С этого времени пациент в палате засыпает сам (ванильный обморок — в 26:00).</summary>
        public const int WardAutoSleepTime = 2500;

        /// <summary>С этого времени в меню палаты есть «Спать до утра».</summary>
        public const int WardNightSleepFromTime = 1800;

        private bool _wardSleepStarted;

        public bool IsWardSleepStarted => _wardSleepStarted;

        /// <summary>Можно ли сейчас лечь спать в палате (пациент, в клинике, сон ещё не начат).</summary>
        public bool CanSleepInWard() =>
            IsHospitalized && !_wardSleepStarted && IsInClinic(Game1.currentLocation);

        /// <summary>
        /// Уложить пациента спать в палате до утра: ванильный сон в текущей локации.
        /// doSleep запоминает lastSleepLocation/lastSleepPoint, а sleptInTemporaryBed позволяет
        /// BedFurniture.ApplyWakeUpPosition разбудить игрока в клинике, а не дома.
        /// </summary>
        public bool TryStartWardSleep(string source)
        {
            if (!CanSleepInWard())
                return false;

            var startSleep = typeof(GameLocation).GetMethod(
                "startSleep",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (startSleep == null)
            {
                _monitor.Log("[Hospital] GameLocation.startSleep не найден — сон в палате недоступен", LogLevel.Error);
                return false;
            }

            _wardSleepStarted = true;
            WarpToHospitalBed();
            Game1.player.faceDirection(2);
            Game1.player.sleptInTemporaryBed.Value = true;
            Game1.dialogueUp = false;

            _monitor.Log($"🏥 Сон в палате до утра ({source}) в {Game1.timeOfDay}", LogLevel.Info);
            startSleep.Invoke(Game1.currentLocation, null);
            return true;
        }

        /// <summary>
        /// Каждый тик: в позднее время пациент в палате засыпает сам, чтобы не упасть в 02:00
        /// (ванильный обморок = штраф деньгами и пробуждение дома).
        /// </summary>
        public void UpdateWardAutoSleep()
        {
            if (Game1.timeOfDay < WardAutoSleepTime || !CanSleepInWard())
                return;

            if (_admissionIntroReason != null || _admissionPromptPending)
                return;

            if (!Context.IsWorldReady || IsGameBusy() || Game1.fadeToBlack || Game1.newDay)
                return;

            Game1.addHUDMessage(new HUDMessage(
                "Глаза слипаются... Тиканье часов в палате убаюкивает.",
                HUDMessage.health_type));
            TryStartWardSleep("auto");
        }

        /// <summary>
        /// Каждый тик: когда пациент в палате и свободен — объяснение Харви, затем вопрос
        /// «поспать / побыть в сознании». Работает и после отложенного (из события) поступления.
        /// </summary>
        public void UpdateAdmissionIntro()
        {
            if (!IsHospitalized)
            {
                _admissionIntroReason = null;
                _admissionPromptPending = false;
                return;
            }

            if (_admissionIntroReason == null && !_admissionPromptPending)
                return;

            if (_wardSleepStarted || !IsInClinic(Game1.currentLocation))
                return;

            if (!Context.IsWorldReady || IsGameBusy() || Game1.fadeToBlack || Game1.globalFade)
                return;

            if (_admissionIntroReason != null)
            {
                string reason = _admissionIntroReason;
                _admissionIntroReason = null;
                _admissionPromptPending = true;
                ShowHospitalizationExplanation(HarveyHelper.FindHarveyInLocation(Game1.currentLocation), reason);
                return;
            }

            _admissionPromptPending = false;
            _activityManager?.ShowAdmissionPrompt(this);
        }

        private static bool IsInEventOrWarp() =>
            Game1.eventUp || Game1.CurrentEvent != null || Game1.locationRequest != null;

        /// <summary>
        /// DayStarted (синхронно, до отложенной инициализации): снять временную кровать,
        /// засчитать ночь в срок и решить — выписка по записке или «проснулась в палате».
        /// </summary>
        public void OnDayStartedHospital()
        {
            _wardSleepStarted = false;

            bool wokeInWard = Game1.player.sleptInTemporaryBed.Value
                && string.Equals(Game1.player.lastSleepLocation.Value, _config.HospitalLocationName,
                    StringComparison.OrdinalIgnoreCase);
            if (wokeInWard)
                Game1.player.sleptInTemporaryBed.Value = false;

            if (!IsHospitalized)
                return;

            ApplyOvernightCreditIfNeeded();
            if (!IsOvernightCreditedToday())
                return;

            if (!IsInClinic(Game1.currentLocation))
            {
                DischargeAfterNight();
                return;
            }

            ApplyHospitalNightRest();
            string woke = Game1.player.IsMale ? "проснулся" : "проснулась";
            Game1.delayedActions.Add(new DelayedAction(1200, () =>
                Game1.addHUDMessage(new HUDMessage(
                    $"Ты {woke} в палате. Ночь прошла спокойно — Харви разрешил выписку.",
                    HUDMessage.health_type))));
        }

        /// <summary>
        /// Засчитать ночь в стационаре: если поступление было в прошлые дни, срок считается выполненным.
        /// Возвращает true, только если засчитали сейчас (идемпотентно в пределах дня).
        /// </summary>
        private bool ApplyOvernightCreditIfNeeded()
        {
            if (!IsHospitalized)
                return false;

            var state = _stateManager.State;
            int today = GameUtils.Today();
            if (state.HospitalAdmissionDay < 0 || state.HospitalAdmissionDay >= today)
                return false;
            if (state.HospitalOvernightCreditDay == today)
                return false;

            RestoreHospitalStayCounters();
            int before = state.HospitalStayProgressMinutes;
            state.HospitalStayProgressMinutes = Math.Max(before, GetMinStayMinutes());
            state.LastHospitalTimeOfDay = Game1.timeOfDay;
            state.HospitalDischargeReadyShown = true;
            state.HospitalOvernightCreditDay = today;
            _dischargeAllowed = true;
            _pendingReturnToHospital = false;
            _pendingBlockedExitReaction = false;
            ResetReturnWarpState();
            _stateManager.Save();

            _monitor.Log(
                $"🏥 Ночь в стационаре засчитана: progress {before} → {state.HospitalStayProgressMinutes} мин " +
                $"(поступление: день {state.HospitalAdmissionDay}, сегодня {today})",
                LogLevel.Info);
            return true;
        }

        private bool IsOvernightCreditedToday() =>
            IsHospitalized && _stateManager.State.HospitalOvernightCreditDay == GameUtils.Today();

        /// <summary>Ночь под присмотром: силы и здоровье полностью восстановлены.</summary>
        private static void ApplyHospitalNightRest()
        {
            Game1.player.health = Game1.player.maxHealth;
            Game1.player.Stamina = Game1.player.MaxStamina;
        }

        /// <summary>Выписка утром после ночи в стационаре — без сцены, записка от Харви.</summary>
        private void DischargeAfterNight()
        {
            bool wokeInWard = IsInClinic(Game1.currentLocation);
            Discharge();
            ApplyHospitalNightRest();

            bool male = Game1.player.IsMale;
            string note = wokeInWard
                ? "Записка от Харви:^«Ночь прошла спокойно, показатели в норме. Можешь идти домой.^" +
                  "Сегодня — без шахты и тяжёлой работы. Если что-то заболит — сразу ко мне.»"
                : $"Записка от Харви:^«Ночь ты {(male ? "провёл" : "провела")} дома — ничего, главное, что " +
                  $"{(male ? "выспался" : "выспалась")}. Вчера показатели уже стабилизировались.^" +
                  "Сегодня отдыхай: без шахты и тяжёлой работы. Загляни ко мне, если станет хуже.»";

            Game1.delayedActions.Add(new DelayedAction(1500, () =>
            {
                if (Context.IsPlayerFree && Game1.activeClickableMenu == null)
                    Game1.drawObjectDialogue(note);
                else
                    Game1.addHUDMessage(new HUDMessage("Харви разрешил выписку после ночи под присмотром.", HUDMessage.health_type));
            }));
        }

        /// <summary>Сколько минут ещё осталось до разрешения на выписку.</summary>
        public int RemainingStayMinutes => GetRemainingMinutes();

        /// <summary>Ночной режим: выписка только утром, после засчитанной ночи.</summary>
        public bool IsOvernightStay =>
            IsHospitalized
            && string.Equals(_stateManager.State.HospitalStayMode, StayModeOvernight, StringComparison.OrdinalIgnoreCase)
            && !IsOvernightCreditedToday();

        /// <summary>Ночь в палате, если поступили после 20:00 или срок заканчивается позже 22:00.</summary>
        private static bool IsOvernightAdmission(int minStayMinutes) =>
            Game1.timeOfDay >= 2000
            || ToClockMinutes(Game1.timeOfDay) + minStayMinutes > 22 * 60;

        /// <summary>QA: установить накопленный прогресс госпитализации в минутах.</summary>
        public void SetHospitalStayProgressMinutes(int minutes)
        {
            var state = _stateManager.State;
            state.HospitalStayProgressMinutes = Math.Max(0, minutes);
            if (state.HospitalStayProgressMinutes >= GetMinStayMinutes())
                state.HospitalStayMode = StayModeDay;
            _dischargeAllowed = state.HospitalStayProgressMinutes >= GetMinStayMinutes();
            _stateManager.Save();
        }

        /// <summary>QA: полный сброс госпитализации.</summary>
        public void ResetHospitalizationForQa()
        {
            ClearHospitalizationState(markDischarged: false);
            _stateManager.Save();
        }

        private void ApplyHospitalizedBuff(int timeOfDay)
        {
            int remaining = GetRemainingMinutes();
            RefreshHospitalizedBuffDuration(remaining);
            ShowHospitalizedStatusHud(timeOfDay, remaining);
            _monitor.Log(
                $"[Hospital] applied {StatusBuffs.Hospitalized}, remaining={remaining} min",
                LogLevel.Debug);
        }

        private void RefreshHospitalizedBuffDuration(int remainingMinutes)
        {
            int durationArg = remainingMinutes <= 0
                ? -2
                : Math.Max(HospitalizedBuffMinDurationMinutes, remainingMinutes);

            if (_buffManager.HasBuff(StatusBuffs.Hospitalized))
                _buffManager.RemoveBuff(StatusBuffs.Hospitalized);

            _buffManager.AddBuff(StatusBuffs.Hospitalized, durationArg);
        }

        private void ShowHospitalizedStatusHud(int timeOfDay, int remainingMinutes)
        {
            Game1.addHUDMessage(new HUDMessage(
                IsOvernightStay
                    ? "Госпитализация: ночь в палате, выписка утром."
                    : HospitalizationHelper.FormatRemainingHud(remainingMinutes),
                HUDMessage.health_type));

            _stateManager.State.HospitalLastStatusHudMinute =
                HospitalizationHelper.ToClockMinutes(timeOfDay);
        }

        private void RemoveHospitalizedBuffFromPlayerAndSaved()
        {
            _buffManager.RemoveBuff(StatusBuffs.Hospitalized);
            _stateManager.State.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, StatusBuffs.Hospitalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Проверить, можно ли выписать пациента
        /// </summary>
        public bool CanDischarge()
        {
            if (!IsHospitalized)
                return true;

            if (IsOvernightStay)
                return false;

            if (!_hospitalHoldActive)
                RestoreHospitalStayCounters();

            return _dischargeAllowed
                || _stateManager.State.HospitalStayProgressMinutes >= GetMinStayMinutes();
        }

        /// <summary>
        /// Однократно уведомить игрока, что минимальный срок госпитализации прошёл.
        /// </summary>
        public void NotifyDischargeReadyIfNeeded()
        {
            if (!_hospitalHoldActive) return;
            if (!_dischargeAllowed) return;
            if (IsOvernightStay) return;

            var state = _stateManager.State;
            if (state.HospitalDischargeReadyShown) return;

            state.HospitalDischargeReadyShown = true;
            _stateManager.Save();

            _monitor.Log("✅ Минимальный срок госпитализации прошёл, игрок может выписаться", LogLevel.Debug);

            Game1.addHUDMessage(new HUDMessage(
                "Харви разрешил выписку. Но сегодня без риска.",
                HUDMessage.health_type));

            NPC? harvey = HarveyHelper.GetHarvey();
            if (harvey != null)
            {
                _dialogueManager.ShowEmoteWithText(
                    harvey,
                    HarveyEmotes.Recovery,
                    "Показатели стабильны. Можешь идти, но без шахты сегодня.");
            }

            _doctorVisitReminderManager?.SyncReminderBuff();
        }

        /// <summary>
        /// Выписать пациента
        /// </summary>
        public void Discharge()
        {
            _monitor.Log("🏥 Выписка пациента", LogLevel.Info);

            string? injuryId = CurrentInjury;
            var state = _stateManager.State;
            string completedCase = state.HospitalizationCaseId;

            ClearHospitalizationState(markDischarged: true, completedCaseId: completedCase);

            if (!string.IsNullOrEmpty(completedCase))
            {
                state.HospitalizationCompletedCaseId = completedCase;
                state.LastHospitalDischargeDay = GameUtils.Today();
                state.LastHospitalDischargeInjuryId = injuryId ?? "";
                state.DischargedToday = true;
            }

            if (!_dialogueManager.HasTopic(ConversationTopics.HospitalDischarged))
                _dialogueManager.AddTopic(ConversationTopics.HospitalDischarged, 2);

            MarkAfterHospitalDischargeHomeEvent(injuryId);
            _stateManager.Save();

            if (string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                ReplaceIntensiveCareWithOutpatientRecovery();

            if (_recoveryPlanManager?.StartHospitalDischargePlan(injuryId) == true)
            {
                _monitor.Log(
                    $"[RecoveryPlan] План восстановления после выписки: injury={injuryId ?? "(none)"}",
                    LogLevel.Info);
            }

            // Сбрасываем активности госпитализации
            _activityManager?.Reset();

            _doctorVisitReminderManager?.SyncReminderBuff();
        }

        private void MarkAfterHospitalDischargeHomeEvent(string? injuryId)
        {
            if (string.IsNullOrEmpty(injuryId))
                return;

            if (!_dialogueManager.IsDatingEngagedOrMarriedToHarvey())
                return;

            var state = _stateManager.State;
            int today = GameUtils.Today();
            state.NeedsHarveyAfterHospitalDischargeHomeEvent = true;
            state.LastHospitalDischargeDay = today;
            state.LastHospitalDischargeInjuryId = injuryId;

            if (!GameUtils.HasConversationTopic(ConversationTopics.AfterHospitalDischargeHome))
                _dialogueManager.AddTopic(ConversationTopics.AfterHospitalDischargeHome, 2);

            _monitor.Log(
                $"[HospitalDischargeHome] Помечено для домашнего события: injury={injuryId}, day={today}",
                LogLevel.Info);
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

            if (string.Equals(Game1.currentLocation?.NameOrUniqueName, loc, StringComparison.OrdinalIgnoreCase))
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
            return string.Equals(location?.NameOrUniqueName, _config.HospitalLocationName,
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
                || !Context.IsPlayerFree
                || Game1.locationRequest != null;
        }

        /// <summary>
        /// Вызывается из Player.Warped после смены локации — завершает отложенный возврат в палату.
        /// </summary>
        public void NotifyPlayerWarped(GameLocation? newLocation)
        {
            if (!IsHospitalized)
            {
                ResetReturnWarpState();
                return;
            }

            if (CanDischarge())
            {
                ResetReturnWarpState();
                return;
            }

            if (!IsInClinic(newLocation))
                return;

            bool showReaction = _pendingBlockedExitReaction;
            _pendingReturnToHospital = false;
            ResetReturnWarpState();
            _pendingBlockedExitReaction = false;

            if (showReaction)
                ShowBlockedExitReaction();
        }

        /// <summary>
        /// Отложенный возврат в палату, когда игрок свободен от событий и диалогов.
        /// </summary>
        public void UpdateHospitalizationLock()
        {
            if (!IsHospitalized)
            {
                ResetReturnWarpState();
                _pendingReturnToHospital = false;
                _pendingBlockedExitReaction = false;
                return;
            }

            // Утро после ночи: проснулся вне клиники (обморок/ванильный будильник дома) — не тащим обратно.
            if (ApplyOvernightCreditIfNeeded() || IsOvernightCreditedToday())
            {
                if (!IsInClinic(Game1.currentLocation) && Context.IsWorldReady && !IsGameBusy())
                    DischargeAfterNight();
                return;
            }

            if (CanDischarge())
                return;

            // Игрок вне клиники не по своей воле (обморок, утро нового дня, внешний варп) —
            // возвращаем в палату без упрёков. Реакция Харви — только на попытку уйти (HandleWarpAttempt).
            bool outsideHospital = !IsInClinic(Game1.currentLocation);
            if (outsideHospital)
                _pendingReturnToHospital = true;

            if (!_pendingReturnToHospital)
                return;

            if (!Context.IsWorldReady)
                return;

            ClearReturnWarpIfTimedOut();

            if (_returnWarpInFlight)
                return;

            if (IsGameBusy())
                return;

            TryStartReturnToHospital();
        }

        /// <summary>
        /// QA/MCP: enforce hospital lock when player is outside clinic (StardewMCP warp bypasses HandleWarpAttempt).
        /// </summary>
        public string EnforceLockForQa()
        {
            if (!IsHospitalized)
                return "SKIP: not hospitalized";

            if (CanDischarge())
                return "SKIP: CanDischarge=true, lock not enforced";

            if (!IsInClinic(Game1.currentLocation))
            {
                _pendingReturnToHospital = true;
                ResetReturnWarpState();
                WarpToHospitalBed();
            }

            return
                $"location={Game1.currentLocation?.Name ?? "(none)"} " +
                $"inClinic={IsInClinic(Game1.currentLocation)} " +
                $"CanDischarge={CanDischarge()}";
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

            // Утро после ночи в стационаре: выписка «по записке», без сцены у двери.
            if (ApplyOvernightCreditIfNeeded() || IsOvernightCreditedToday())
            {
                _monitor.Log("✅ Выход из клиники утром после ночи в стационаре — выписка по записке", LogLevel.Info);
                DischargeAfterNight();
                return false;
            }

            // Игрок пытается уйти из больницы
            if (CanDischarge())
            {
                _monitor.Log("✅ Игрок покидает больницу после окончания срока", LogLevel.Info);
                Discharge();

                NPC? harvey = HarveyHelper.GetHarvey();
                if (harvey != null)
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
            Game1.addHUDMessage(new HUDMessage("Харви просит ещё немного полежать в палате.", HUDMessage.error_type));

            if (Context.IsWorldReady && !IsGameBusy())
                TryStartReturnToHospital();

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

            if (reason == NeglectCollapseReason)
                return Math.Max(baseMinutes, 120);

            return baseMinutes;
        }

        private bool TryStartReturnToHospital()
        {
            if (_returnWarpInFlight)
                return false;

            if (!IsHospitalized || CanDischarge())
                return false;

            if (IsInClinic(Game1.currentLocation))
            {
                _pendingReturnToHospital = false;
                ResetReturnWarpState();
                return false;
            }

            if (Game1.locationRequest != null)
                return false;

            _returnWarpInFlight = true;
            _returnWarpStartedTick = (int)Game1.ticks;
            _monitor.Log("[Hospital] return warp → clinic bed", LogLevel.Debug);
            WarpToHospitalBed();
            return true;
        }

        private void ClearReturnWarpIfTimedOut()
        {
            if (!_returnWarpInFlight || _returnWarpStartedTick < 0)
                return;

            if ((int)Game1.ticks - _returnWarpStartedTick <= ReturnWarpTimeoutTicks)
                return;

            _monitor.Log("[Hospital] return warp timed out — retry allowed", LogLevel.Debug);
            ResetReturnWarpState();
        }

        private void ResetReturnWarpState()
        {
            _returnWarpInFlight = false;
            _returnWarpStartedTick = -1;
        }

        private void ShowBlockedExitReaction()
        {
            NPC? harvey = HarveyHelper.GetHarvey();
            if (harvey != null)
            {
                _dialogueManager.ShowFullReaction(
                    harvey,
                    HarveyEmotes.StayInBed,
                    HarveyTextMessages.DontMove,
                    $"Подожди... Мне правда нужно, чтобы ты ещё {(Game1.player.IsMale ? "полежал" : "полежала")}.$s#$b#" +
                    "Ещё немного — и отпущу, обещаю.$l");
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("Харви просил ещё немного побыть в палате.", HUDMessage.error_type));
            }
        }

        private void RestoreHospitalStayCounters()
        {
            if (!IsHospitalized)
            {
                ResetHospitalStayCounters();
                return;
            }

            _hospitalHoldActive = true;
            _currentHospitalCaseInjury = CurrentInjury;
            var state = _stateManager.State;

            if (state.LastHospitalTimeOfDay < 0)
                state.LastHospitalTimeOfDay = Game1.timeOfDay;

            _dischargeAllowed = state.HospitalStayProgressMinutes >= GetMinStayMinutes();
        }

        private void ResetHospitalStayCounters()
        {
            _hospitalHoldActive = false;
            _currentHospitalCaseInjury = null;
            _dischargeAllowed = false;
        }

        private int GetMinStayMinutes()
        {
            var state = _stateManager.State;
            return state.HospitalMinStayMinutes > 0
                ? state.HospitalMinStayMinutes
                : _config.MinHospitalStayMinutes;
        }

        private int GetRemainingMinutes()
        {
            // Ночной режим: до выписки — до утра (для баффа считаем до 02:00, дальше сон).
            if (IsOvernightStay)
                return Math.Max(10, ToClockMinutes(2600) - ToClockMinutes(Game1.timeOfDay));

            return Math.Max(0, GetMinStayMinutes() - _stateManager.State.HospitalStayProgressMinutes);
        }

        private void ClearHospitalizationState(bool markDischarged = false, string? completedCaseId = null)
        {
            _pendingReturnToHospital = false;
            _pendingBlockedExitReaction = false;
            ResetReturnWarpState();
            ResetHospitalStayCounters();

            var state = _stateManager.State;
            state.IsHospitalized = false;
            state.HospitalizedInjuryId = "";
            state.HospitalizationReason = "";
            state.HospitalAdmissionDay = -1;
            state.HospitalAdmissionTime = -1;
            state.HospitalAdmissionMinutes = -1;
            state.HospitalMinStayMinutes = 120;
            state.HospitalDischargeReadyShown = false;
            state.HospitalLastStatusHudMinute = -1;
            state.HospitalStayProgressMinutes = 0;
            state.LastHospitalTimeOfDay = -1;
            state.HospitalizationCaseId = "";
            state.HospitalStayMode = "";
            _admissionIntroReason = null;
            _admissionPromptPending = false;

            RemoveHospitalizedBuffFromPlayerAndSaved();
        }
    }
}
