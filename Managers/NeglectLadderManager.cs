using System;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Services;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;
using StardewValley.Triggers;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Лестница последствий для запущенной травмы, ступени 4–5. Ступени 1–3 (письма, грязная рана → инфекция,
    /// «Небрежность») ведёт ComplicationManager; раньше на «Небрежности» всё и заканчивалось.
    /// <para>
    /// Ступень 4 — «Харви приходит сам»: накануне письмо «загляну утром», утром на ферме (в браке — дома)
    /// сцена с выбором. «Хорошо, осмотри» — всё, что он сделал бы в клинике, плюс день лечения назад.
    /// «Не сейчас» — бинт и обезболивающее, ухудшение на паузе пару дней, потом он придёт ещё раз.
    /// </para>
    /// <para>
    /// Ступень 5 — после второго «не сейчас» и ещё одного дня: обморок за тяжёлой работой или без сил
    /// после полудня, день в клинике. Один раз на случай, без потери денег; на «Уютно» её нет.
    /// </para>
    /// Скрытые травмы (Харви о них не знает) лестница не трогает — у них своя система.
    /// </summary>
    public sealed class NeglectLadderManager
    {
        public const string ProviderId = "HarveyOverhaulInjury";

        private const string InfectedWoundId = "buffInfectedWound";
        private const int VisitTimeStart = 600;
        private const int VisitTimeEnd = 1100;
        private const int CollapseLowStaminaTimeStart = 1200;
        private const float CollapseLowStaminaShare = 0.25f;
        private const int CollapseDelayMs = 1500;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly ComplicationManager _complicationManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly TreatmentStartHandler _treatmentStartHandler;
        private readonly CareTrustManager _careTrustManager;
        private readonly MedicalLetterScheduler _medicalLetterScheduler;
        private IHarveyCoreApi? _coreApi;

        /// <summary>День, когда в сцене визита уже выбрали ответ (accept/defer). Без выбора — считаем «не сейчас».</summary>
        private int _choiceMadeDay = -1;
        private bool _collapseInProgress;
        private int _missingEventLoggedDay = -1;

        public NeglectLadderManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            ComplicationManager complicationManager,
            HospitalizationManager hospitalizationManager,
            TreatmentStartHandler treatmentStartHandler,
            CareTrustManager careTrustManager,
            MedicalLetterScheduler medicalLetterScheduler)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _complicationManager = complicationManager;
            _hospitalizationManager = hospitalizationManager;
            _treatmentStartHandler = treatmentStartHandler;
            _careTrustManager = careTrustManager;
            _medicalLetterScheduler = medicalLetterScheduler;
        }

        public void SetCoreApi(IHarveyCoreApi? coreApi) => _coreApi = coreApi;

        private NeglectLadderState Ladder => _stateManager.State.NeglectLadder;

        private bool IsCozy => _config.GetEffectiveDifficulty() == DifficultyPresets.Cozy;
        private bool IsStrict => _config.GetEffectiveDifficulty() == DifficultyPresets.Hardcore;

        private bool HouseCallsEnabled => _coreApi?.HouseCallsEnabled ?? true;
        private bool CollapseEnabled => (_coreApi?.NeglectCollapseEnabled ?? true) && !IsCozy;

        /// <summary>Сколько дней «Небрежности» до письма (визит — назавтра).</summary>
        private int NeglectDaysBeforeLetter => IsStrict ? 1 : 2;

        /// <summary>Сколько дней нелеченой инфекции до письма (визит — назавтра).</summary>
        private int InfectionDaysBeforeLetter => IsStrict ? 0 : 1;

        /// <summary>Отсрочка после первого «не сейчас».</summary>
        private int DeferDays => IsCozy ? 3 : 2;

        public void RegisterTriggerActions()
        {
            TriggerActionManager.RegisterAction(TreatmentStartActions.HouseCallAccept, OnHouseCallAcceptAction);
            TriggerActionManager.RegisterAction(TreatmentStartActions.HouseCallDefer, OnHouseCallDeferAction);
            _monitor.Log(
                $"[NeglectLadder] Registered trigger actions: {TreatmentStartActions.HouseCallAccept}, {TreatmentStartActions.HouseCallDefer}",
                LogLevel.Debug);
        }

        // ===== Утро: оценить ступень =====

        /// <summary>Вызывается в DayStarted после CheckTreatmentCompletion: осложнения на сегодня уже известны.</summary>
        public void OnDayStarted()
        {
            if (!Context.IsWorldReady)
                return;

            var ladder = Ladder;
            int today = GameUtils.Today();

            if (!TryGetCase(out string injuryId, out DebuffState debuffState))
            {
                if (ladder.HasCase)
                {
                    _monitor.Log($"[NeglectLadder] Случай {ladder.CaseInjuryId} закрыт: травмы больше нет", LogLevel.Info);
                    ladder.Clear();
                    _stateManager.Save();
                }
                return;
            }

            if (!string.Equals(ladder.CaseInjuryId, injuryId, StringComparison.OrdinalIgnoreCase)
                || ladder.CaseInjuryStartDay != debuffState.InjuryStartDay)
            {
                ladder.StartCase(injuryId, debuffState.InjuryStartDay);
                _monitor.Log($"[NeglectLadder] Новый случай: {injuryId} (день травмы {debuffState.InjuryStartDay})", LogLevel.Debug);
            }

            if (!NeedsHouseCall(injuryId, debuffState, today, out bool infection))
            {
                if (ladder.HouseCallDay >= 0 || ladder.Deferrals > 0 || ladder.CollapseFromDay >= 0)
                {
                    _monitor.Log($"[NeglectLadder] {injuryId}: лечение идёт — лестница сброшена", LogLevel.Info);
                    ladder.ResetProgress();
                    _stateManager.Save();
                }
                return;
            }

            if (!HouseCallsEnabled || _hospitalizationManager.IsHospitalized)
                return;

            // Первый раз: письмо сегодня, визит завтра. Повторный визит назначает «не сейчас».
            if (ladder.HouseCallDay < 0 && ladder.Deferrals == 0)
            {
                ladder.HouseCallDay = today + 1;
                ladder.LetterDay = today;
                SendHouseCallLetter();
                _stateManager.Save();
                _monitor.Log(
                    $"[NeglectLadder] {injuryId}: ступень 4 — письмо сегодня, визит с дня {ladder.HouseCallDay} " +
                    $"(инфекция={infection}, сложность={_config.GetEffectiveDifficulty()})",
                    LogLevel.Info);
            }
        }

        /// <summary>
        /// Есть случай для лестницы: main-травма, о которой Харви знает.
        /// </summary>
        private bool TryGetCase(out string injuryId, out DebuffState debuffState)
        {
            injuryId = _complicationManager.GetActiveMainInjuryId() ?? "";
            debuffState = null!;

            if (string.IsNullOrEmpty(injuryId) || _stateManager.GetDebuffState(injuryId) is not { } ds)
                return false;

            if (ds.HiddenFromHarvey && !ds.HarveyAware)
                return false;

            debuffState = ds;
            return true;
        }

        /// <summary>Лечение запущено: «Небрежность» держится, или инфекцию не показали Харви.</summary>
        private bool NeedsHouseCall(string injuryId, DebuffState debuffState, int today, out bool infection)
        {
            infection = string.Equals(injuryId, InfectedWoundId, StringComparison.OrdinalIgnoreCase)
                && !debuffState.TreatmentStarted;

            if (infection && today - debuffState.InjuryStartDay >= InfectionDaysBeforeLetter)
                return true;

            return _stateManager.State.ActiveComplications.TryGetValue(InjuryBuffs.Neglect, out int neglectStart)
                && today - neglectStart >= NeglectDaysBeforeLetter;
        }

        /// <summary>«Загляну утром» — сразу в ящик: визит уже завтра, письмо не может прийти на день позже.</summary>
        private void SendHouseCallLetter()
        {
            if (!_medicalLetterScheduler.ShouldQueueMedical(critical: false))
                return;

            string mailId = HarveyMailHelper.BuildRelationshipMailId(MailIds.HouseCallInjury);
            if (!HarveyMailHelper.MailEntryExists(mailId))
            {
                _monitor.Log($"[NeglectLadder] Письмо {mailId} не найдено в Data/mail — визит без письма", LogLevel.Warn);
                return;
            }

            var player = Game1.player;
            player.mailReceived.Remove(mailId);
            player.mailForTomorrow.Remove(mailId);
            if (!player.mailbox.Contains(mailId))
                player.mailbox.Add(mailId);
        }

        // ===== Визит на ферму =====

        /// <summary>Вход в локацию или периодический retry: если визит назначен — запустить сцену.</summary>
        public bool TryLaunchHouseCall(GameLocation? location, string source)
        {
            location ??= Game1.currentLocation;
            var ladder = Ladder;
            int today = GameUtils.Today();

            if (location == null || ladder.HouseCallDay < 0 || today < ladder.HouseCallDay
                || ladder.LastVisitDay == today || ladder.Deferrals >= 2)
                return false;

            if (Game1.timeOfDay < VisitTimeStart || Game1.timeOfDay > VisitTimeEnd)
                return false;

            if (!HouseCallsEnabled || _hospitalizationManager.IsHospitalized || IsFestival())
                return false;

            bool married = HarveyHelper.GetHarveyRelationshipTier() == HarveyRelationshipTier.Married;
            bool atVisitPlace = married
                ? LocationEventLauncher.IsFarmHouseLocation(location)
                : location is Farm;
            if (!atVisitPlace)
                return false;

            if (IsPlayerBlocked(out _))
                return false;

            if (!TryGetCase(out string injuryId, out DebuffState debuffState)
                || !NeedsHouseCall(injuryId, debuffState, today, out bool infection))
            {
                ladder.ResetProgress();
                _stateManager.Save();
                return false;
            }

            string eventId = ResolveEventId(married, infection, location.NameOrUniqueName);
            if (!LocationEventLauncher.LocationEventExists(eventId, location.NameOrUniqueName))
            {
                if (_missingEventLoggedDay != today)
                {
                    _missingEventLoggedDay = today;
                    _monitor.Log(
                        $"[NeglectLadder] Нет события {eventId} в Data/Events/{location.NameOrUniqueName} — визит отложен",
                        LogLevel.Warn);
                }
                return false;
            }

            if (_coreApi != null && !_coreApi.TryClaimHouseCall(ProviderId))
            {
                _monitor.Log("[NeglectLadder] Слот визита сегодня занят другим модом — завтра", LogLevel.Trace);
                return false;
            }

            _dialogueManager.AddTopic(ConversationTopics.HouseCallInjuryPending, 1);

            if (!LocationEventLauncher.TryStartEventByPrefix(
                    eventId,
                    location.NameOrUniqueName,
                    _monitor,
                    onFinished: resolvedId => OnHouseCallFinished(resolvedId),
                    scriptPrefix: "pause 200/",
                    addToEventsSeenOnFinished: false,
                    logTag: "NeglectLadder/HouseCall"))
            {
                _dialogueManager.RemoveTopic(ConversationTopics.HouseCallInjuryPending);
                _coreApi?.ReleaseHouseCall(ProviderId);
                return false;
            }

            ladder.LastVisitDay = today;
            _stateManager.Save();
            _monitor.Log(
                $"[NeglectLadder/{source}] Визит Харви: {eventId}, травма {injuryId}, отказов {ladder.Deferrals}",
                LogLevel.Info);
            return true;
        }

        private string ResolveEventId(bool married, bool infection, string locationName)
        {
            if (Ladder.Deferrals > 0)
                return EventIds.HouseCallInjuryRevisit;

            // «Строго»: инфекцию Харви не оставит «на потом». Нет такой сцены — обычная.
            if (IsStrict && infection && LocationEventLauncher.LocationEventExists(EventIds.HouseCallInjuryUrgent, locationName))
                return EventIds.HouseCallInjuryUrgent;

            return married ? EventIds.HouseCallInjuryMarried : EventIds.HouseCallInjury;
        }

        private void OnHouseCallFinished(string eventId)
        {
            _dialogueManager.RemoveTopic(ConversationTopics.HouseCallInjuryPending);

            if (_choiceMadeDay != GameUtils.Today())
            {
                // Сцену пропустили без setSkipActions или в ней нет выбора — мягкий вариант.
                _monitor.Log($"[NeglectLadder] {eventId}: выбор не сделан — считаем «не сейчас»", LogLevel.Warn);
                ApplyDefer();
            }

            _stateManager.Save();
        }

        private bool OnHouseCallAcceptAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;
            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            if (!TryGetCase(out string injuryId, out _))
            {
                error = "no injury case";
                return false;
            }

            _choiceMadeDay = GameUtils.Today();
            string summary = _treatmentStartHandler.PerformHouseCallExam(injuryId);

            if (_stateManager.GetDebuffState(injuryId) is { } ds && RefundNeglectDay(ds))
                _stateManager.UpdateDebuffState(injuryId, ds);

            _careTrustManager.AddTrust(1, "house_call_accepted");
            _dialogueManager.AddTopic(ConversationTopics.HouseCallAccepted, 3);
            Ladder.ResetProgress();
            _stateManager.Save();

            _monitor.Log($"[NeglectLadder] Визит: осмотр принят ({injuryId}: {summary})", LogLevel.Info);
            return true;
        }

        private bool OnHouseCallDeferAction(string[] args, TriggerActionContext context, out string error)
        {
            error = string.Empty;
            if (!Context.IsWorldReady)
            {
                error = "world not ready";
                return false;
            }

            _choiceMadeDay = GameUtils.Today();
            ApplyDefer();
            _stateManager.Save();
            return true;
        }

        private void ApplyDefer()
        {
            var ladder = Ladder;
            int today = GameUtils.Today();
            ladder.Deferrals++;
            ladder.LastVisitDay = today;

            if (ladder.Deferrals == 1)
            {
                ladder.PausedUntilDay = today + DeferDays;
                ladder.HouseCallDay = ladder.PausedUntilDay;
                _dialogueManager.AddTopic(ConversationTopics.HouseCallDeferred, DeferDays);
                _monitor.Log(
                    $"[NeglectLadder] «Не сейчас» #1: пауза до дня {ladder.PausedUntilDay}, повторный визит в тот же день",
                    LogLevel.Info);
                return;
            }

            ladder.PausedUntilDay = -1;
            ladder.HouseCallDay = -1;
            if (CollapseEnabled && !ladder.CollapseDone)
                ladder.CollapseFromDay = today + 2;

            _monitor.Log(
                $"[NeglectLadder] «Не сейчас» #2: визитов больше не будет, обморок возможен с дня {ladder.CollapseFromDay}",
                LogLevel.Info);
        }

        /// <summary>
        /// Награда за то, что позволила помочь: один день из удлинения «Небрежности» возвращается.
        /// Сокращаем текущий отрезок, но не короче 1 дня.
        /// </summary>
        private static bool RefundNeglectDay(DebuffState ds)
        {
            if (ds.NeglectExtensionDays <= 0)
                return false;

            switch (ds.IsPhasedInjury ? ds.CurrentPhase : 1)
            {
                case 1 when ds.Phase1Duration > 1: ds.Phase1Duration--; break;
                case 2 when ds.Phase2Duration > 1: ds.Phase2Duration--; break;
                case 3 when ds.Phase3Duration > 1: ds.Phase3Duration--; break;
                default: return false;
            }

            ds.NeglectExtensionDays--;
            return true;
        }

        // ===== Ступень 5: обморок =====

        /// <summary>Каждые 10 игровых минут: после второго отказа тело может не выдержать.</summary>
        public void OnTimeChanged()
        {
            var ladder = Ladder;
            if (_collapseInProgress || ladder.CollapseDone || ladder.CollapseFromDay < 0)
                return;

            int today = GameUtils.Today();
            if (today < ladder.CollapseFromDay || !CollapseEnabled)
                return;

            if (_hospitalizationManager.IsHospitalized || IsFestival() || IsPlayerBlocked(out _))
                return;

            var location = Game1.currentLocation;
            if (location == null || location is MineShaft || location is VolcanoDungeon
                || _hospitalizationManager.IsInClinic(location))
                return;

            if (!TryGetCase(out string injuryId, out DebuffState debuffState)
                || !NeedsHouseCall(injuryId, debuffState, today, out _))
            {
                ladder.ResetProgress();
                _stateManager.Save();
                return;
            }

            bool lowStamina = Game1.timeOfDay >= CollapseLowStaminaTimeStart
                && Game1.player.Stamina < Game1.player.MaxStamina * CollapseLowStaminaShare;
            if (!_complicationManager.IsDoingHeavyWorkRecently() && !lowStamina)
                return;

            StartCollapse(injuryId, lowStamina ? "low_stamina" : "heavy_work");
        }

        private void StartCollapse(string injuryId, string trigger)
        {
            var ladder = Ladder;
            ladder.CollapseDone = true;
            ladder.CollapseFromDay = -1;
            _stateManager.Save();
            _collapseInProgress = true;

            Game1.player.Halt();
            Game1.playSound("ow");
            Game1.addHUDMessage(new HUDMessage("В глазах темнеет… ноги не держат.", HUDMessage.error_type));
            _monitor.Log($"[NeglectLadder] Ступень 5: обморок ({injuryId}, {trigger})", LogLevel.Warn);

            Game1.delayedActions.Add(new DelayedAction(CollapseDelayMs, () =>
            {
                try
                {
                    _hospitalizationManager.StartForcedHospitalizationWithExplanation(
                        injuryId,
                        HarveyHelper.GetHarvey(),
                        HospitalizationManager.NeglectCollapseReason);

                    // Госпитализация уже начала лечение; осталось то, что Харви сделал бы на осмотре.
                    _treatmentStartHandler.PerformHouseCallExam(injuryId, startTreatment: false);
                    _dialogueManager.AddTopic(ConversationTopics.NeglectCollapse, 3);
                    Ladder.ResetProgress();
                    _stateManager.Save();
                }
                catch (Exception ex)
                {
                    _monitor.Log($"[NeglectLadder] Ошибка обморока: {ex}", LogLevel.Error);
                }
                finally
                {
                    _collapseInProgress = false;
                }
            }));
        }

        // ===== Общее =====

        private bool IsFestival() =>
            _coreApi?.IsFestivalContext() ?? (Game1.isFestival() || Utility.isFestivalDay());

        private static bool IsPlayerBlocked(out string reason)
        {
            reason = Game1.CurrentEvent != null || Game1.eventUp ? "event"
                : Game1.dialogueUp ? "dialogue"
                : Game1.activeClickableMenu != null ? "menu"
                : !Context.IsPlayerFree ? "player not free"
                : Game1.locationRequest != null ? "warp"
                : "";
            return reason.Length > 0;
        }

        /// <summary>QA: состояние лестницы одной строкой.</summary>
        public string Describe()
        {
            var l = Ladder;
            return $"case={l.CaseInjuryId}({l.CaseInjuryStartDay}) letter={l.LetterDay} visitFrom={l.HouseCallDay} " +
                   $"lastVisit={l.LastVisitDay} deferrals={l.Deferrals} pausedUntil={l.PausedUntilDay} " +
                   $"collapseFrom={l.CollapseFromDay} collapseDone={l.CollapseDone} today={GameUtils.Today()} " +
                   $"houseCalls={HouseCallsEnabled} collapse={CollapseEnabled} difficulty={_config.GetEffectiveDifficulty()}";
        }

        /// <summary>QA: перевести лестницу на ступень (4 — визит сегодня, 5 — обморок взведён), reset — сбросить.</summary>
        public string QaSetStage(string stage)
        {
            var ladder = Ladder;
            int today = GameUtils.Today();

            if (!TryGetCase(out string injuryId, out DebuffState debuffState))
                return "Нет main-травмы, о которой знает Харви.";

            if (!string.Equals(ladder.CaseInjuryId, injuryId, StringComparison.OrdinalIgnoreCase))
                ladder.StartCase(injuryId, debuffState.InjuryStartDay);

            switch (stage.ToLowerInvariant())
            {
                case "reset":
                    ladder.StartCase(injuryId, debuffState.InjuryStartDay);
                    break;
                case "4":
                    ladder.ResetProgress();
                    ladder.HouseCallDay = today;
                    ladder.LetterDay = today;
                    SendHouseCallLetter();
                    break;
                case "revisit":
                    ladder.ResetProgress();
                    ladder.Deferrals = 1;
                    ladder.HouseCallDay = today;
                    break;
                case "5":
                    ladder.ResetProgress();
                    ladder.Deferrals = 2;
                    ladder.CollapseDone = false;
                    ladder.CollapseFromDay = today;
                    break;
                default:
                    return "Ступень: 4 | revisit | 5 | reset";
            }

            _stateManager.Save();
            return Describe();
        }
    }
}
