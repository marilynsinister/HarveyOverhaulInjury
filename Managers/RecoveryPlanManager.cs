using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// План восстановления: save-state + объясняющий UI. Без новых баффов.
    /// </summary>
    public class RecoveryPlanManager
    {
        public const string HospitalDischargePlanId = "RecoveryPlan_HospitalDischarge";
        public const string HospitalDischargeReason = "hospital";
        public const int HospitalDischargeRequiredDays = 3;
        public const int MaxTasksPerDay = 6;

        private const string HudHospitalStart = "Харви назначил план восстановления на 3 дня.";
        private const string HudHospitalDayFailed = "План восстановления: режим сорван.";
        private const string HudHospitalDayCredited = "План восстановления: день зачтён.";
        private const string HudHospitalCompleted = "План восстановления завершён. Поговори с Харви.";

        private const int CriticalHealthFractionPercent = 15;
        private const float WarnHealthFraction = 0.35f;
        private const float CriticalStaminaFraction = 0.15f;
        private const float StaminaWarningFraction = 0.25f;
        private const float StaminaViolationFraction = 0.15f;
        private const float HealthWarningFraction = 0.45f;
        private const float HealthViolationFraction = 0.35f;
        private const int RestBeforeTime = 2400;
        private const int LateNightReminderTime = 2300;
        private const int CompletedTopicDays = RecoveryPlanViolationTopicMap.CompletedTopicDays;
        private const int PerfectTopicDaysMin = 3;
        private const int PerfectTopicDaysMax = 7;
        private const int SoftToneDays = 3;
        private const int PerfectFriendshipPoints = 20;
        private const int PerfectRomanticFriendshipPoints = 40;
        private const int NormalCompletionFriendshipPoints = 10;
        private const float HarveyCompletionDialogueDistance = 5f;
        private const int CompletionTalkDialogueTimeoutTicks = 1800;
        private const int StaminaCheckIntervalTicks = 60 * 10;
        private const int HealthCheckIntervalTicks = 60 * 20;
        private const int RainBandageViolationSeconds = 120;
        private const int OverworkViolationSeconds = 90;

        private const string HudMildStamina =
            "Харви: Ты переутомляешься. Остановись до того, как станет хуже.";
        private const string HudStaminaSoftWarning =
            "Харви: Ты уже дрожишь от усталости. Ещё немного — и я засчитаю это как нарушение режима.";
        private const string HudHealthSoftWarning =
            "Харви: Здоровье уже на пределе. Ещё немного — и я засчитаю это как нарушение режима.";
        private const string HudMediumDayFailed =
            "Харви: Сегодня режим сорван. Завтра зайди ко мне, пожалуйста.";
        private const string HudSevereViolation =
            "Харви: Нет. Это уже опасно. Ты идёшь ко мне на осмотр.";
        private const string HudMildEveningReminder =
            "Харви: Сегодня ты переутомлялась. Завтра — помягче.";
        private const string HudMildMorningReminder =
            "Харви: Вчера ты переутомлялась. Сегодня береги себя.";
        private const string HudDailyDayCredited = "План восстановления: день зачтён.";
        private const string HudDailyDayFailed = "План восстановления: день не засчитан.";
        private const string HudViolationRecorded =
            "План восстановления: день не засчитан. Причина записана в плане.";

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;

        private bool _awaitingCompletionTalkAck;
        private bool _completionTalkSawDialogue;
        private int _completionTalkStartedTick = -1;
        private int _lastHudDay = -1;
        private readonly HashSet<string> _hudShownKeys = new(StringComparer.OrdinalIgnoreCase);
        private int _lastStaminaCheckTick;
        private int _lastHealthCheckTick;
        private int _overworkLowStaminaToolSeconds;
        private int _morningHudRetryCount;

        public RecoveryPlanManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
        }

        // ============================================================================
        // Ежедневный план восстановления
        // ============================================================================

        /// <summary>Пересобрать план на сегодня и сохранить в InjuryState.RecoveryPlan.</summary>
        public void RefreshPlanForToday(bool notifyUpdated = false, bool notifyCreated = false)
        {
            if (!Context.IsWorldReady)
                return;

            ClearPlanIfRecovered();

            var plan = _stateManager.GetRecoveryPlan();
            int today = GameUtils.Today();

            if (previousDayReset(plan, today))
            {
                plan.TodayViolations.Clear();
                EnsureTodayViolationReasons(plan);
                plan.TodayViolationReasons.Clear();
                plan.TodayViolationTypes.Clear();
                plan.HadWarningsToday = false;
                plan.TodayViolationDialogueType = "";
                plan.TodayViolationDialogueSeverity = 0;
            }

            string? injuryId = ResolveMainInjuryId();
            if (string.IsNullOrEmpty(injuryId))
            {
                plan.IsActive = false;
                plan.Status = RecoveryPlanMoodStatus.None;
                _stateManager.Save();
                return;
            }

            if (!_stateManager.State.ActiveDebuffs.TryGetValue(injuryId, out DebuffState? debuffState)
                || debuffState == null)
            {
                plan.IsActive = false;
                plan.Status = RecoveryPlanMoodStatus.None;
                _stateManager.Save();
                return;
            }

            List<RecoveryPlanTask> tasks = BuildTasks(injuryId, debuffState);
            ApplyViolationFlagsToTasks(tasks, plan.TodayViolations);

            plan.ActiveInjuryId = injuryId;
            plan.PlanStartDay = debuffState.PhaseStartDay > 0 ? debuffState.PhaseStartDay : debuffState.InjuryStartDay;
            plan.CurrentDay = GetCurrentPlanDay(debuffState);
            plan.TotalDays = GetTotalPlanDays(debuffState) + Math.Max(0, plan.PlanExtensionDays);
            plan.CurrentPhase = debuffState.CurrentPhase;
            plan.TotalPhases = debuffState.TotalPhases;
            plan.IsActive = true;
            plan.NeedsHarveyVisit = debuffState.ReadyForNextPhase
                || debuffState.ReadyForRecovery
                || _stateManager.State.ActiveComplications.Count > 0
                || !debuffState.HarveyConversationHappened;
            plan.Tasks = tasks;
            plan.Status = CalculateStatus(plan, debuffState);
            plan.LastUpdatedDay = today;

            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] Refresh: injury={injuryId}, day={plan.CurrentDay}/{plan.TotalDays}, "
                + $"phase={plan.CurrentPhase}/{plan.TotalPhases}, status={plan.Status}, "
                + $"tasks={tasks.Count}, violations={plan.TodayViolations.Count}, concern={plan.ConcernScore}",
                LogLevel.Info);

            if (notifyCreated)
                TryShowHudOnce("plan_created", RecoveryPlanTexts.Hud.PlanCreated);
            else if (notifyUpdated)
                TryShowHudOnce("plan_updated", RecoveryPlanTexts.Hud.PlanUpdated);

            if (_config.AutoShowRecoveryPlanMorning && !plan.WasShownToday && notifyCreated)
            {
                plan.WasShownToday = true;
                _stateManager.Save();
            }
        }

        public List<RecoveryPlanTask> BuildTasks(string injuryId, DebuffState debuffState)
        {
            var tasks = new List<RecoveryPlanTask>();
            var state = _stateManager.State;

            if (debuffState.ReadyForRecovery)
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.VisitHarveyIfReady,
                    RecoveryPlanTexts.Tasks.VisitTitle,
                    RecoveryPlanTexts.Tasks.VisitRecoveryDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Danger));
            }
            else if (debuffState.ReadyForNextPhase)
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.VisitHarveyIfReady,
                    RecoveryPlanTexts.Tasks.VisitTitle,
                    RecoveryPlanTexts.Tasks.VisitPhaseDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Danger));
            }
            else if (!debuffState.HarveyConversationHappened && debuffState.TreatmentStarted == false)
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.VisitHarveyIfReady,
                    RecoveryPlanTexts.Tasks.VisitTitle,
                    RecoveryPlanTexts.Tasks.VisitStartDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Warning));
            }

            if (state.ActiveComplications.Count > 0)
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.TreatComplications,
                    RecoveryPlanTexts.Tasks.ComplicationTitle,
                    RecoveryPlanTexts.Tasks.ComplicationDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Danger));
            }

            if (ShouldAvoidMinesForPlan(injuryId, state))
            {
                (string minesTitle, string minesDesc) = MineEntryCoordinator.GetMineTaskLabels(
                    state, _config, _buffManager, GameUtils.Today());
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.AvoidMines,
                    minesTitle,
                    minesDesc,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Danger));
            }

            if (NeedsSleepRule(injuryId, state))
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.SleepBeforeMidnight,
                    RecoveryPlanTexts.Tasks.SleepTitle,
                    RecoveryPlanTexts.Tasks.SleepDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Danger));
            }

            if (NeedsBandageDryRule(injuryId, state))
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.KeepBandageDry,
                    RecoveryPlanTexts.Tasks.BandageTitle,
                    RecoveryPlanTexts.Tasks.BandageDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Warning));
            }

            tasks.Add(MakeTask(
                RecoveryPlanTaskIds.KeepStaminaAbove15,
                RecoveryPlanTexts.Tasks.StaminaTitle,
                RecoveryPlanTexts.Tasks.StaminaDescription,
                required: true,
                severity: RecoveryPlanTaskSeverity.Warning));

            if (InjurySets.Severe.Contains(injuryId) || InjurySets.Critical.Contains(injuryId))
            {
                tasks.Add(MakeTask(
                    RecoveryPlanTaskIds.ReturnIfLowHealth,
                    RecoveryPlanTexts.Tasks.HealthTitle,
                    RecoveryPlanTexts.Tasks.HealthDescription,
                    required: true,
                    severity: RecoveryPlanTaskSeverity.Danger));
            }

            return tasks.Take(MaxTasksPerDay).ToList();
        }

        /// <summary>День внутри текущей фазы (или простого лечения).</summary>
        public int GetCurrentPlanDay(DebuffState debuffState)
        {
            int today = GameUtils.Today();
            int start = debuffState.PhaseStartDay > 0 ? debuffState.PhaseStartDay : debuffState.InjuryStartDay;
            int daysInPhase = Math.Max(1, today - start + 1);
            int duration = GetTotalPlanDays(debuffState);
            return Math.Min(daysInPhase, Math.Max(1, duration));
        }

        /// <summary>Длительность текущей фазы (для подписи «день X из Y»).</summary>
        public int GetTotalPlanDays(DebuffState debuffState)
        {
            if (debuffState.TotalPhases > 0 && debuffState.CurrentPhase > 0)
                return Math.Max(1, debuffState.GetCurrentPhaseDuration());

            if (debuffState.Phase1Duration > 0)
                return debuffState.Phase1Duration;

            return Math.Max(1, debuffState.GetTotalDuration());
        }

        public RecoveryPlanMoodStatus CalculateStatus(RecoveryPlanState plan, DebuffState debuffState)
        {
            if (!plan.IsActive)
                return RecoveryPlanMoodStatus.None;

            bool severe = !string.IsNullOrEmpty(plan.ActiveInjuryId)
                && InjurySets.Severe.Contains(plan.ActiveInjuryId);
            bool inMine = Context.IsWorldReady && IsMineOrVolcanoLocation(Game1.currentLocation);
            bool hasMineViolation = plan.TodayViolations.Any(v =>
                v.Reason == RecoveryPlanViolationIds.EnteredMinesDuringRecovery);

            if (Context.IsWorldReady)
            {
                int criticalHp = (int)Math.Ceiling(Game1.player.maxHealth * (CriticalHealthFractionPercent / 100f));
                if (Game1.player.health <= criticalHp)
                    return RecoveryPlanMoodStatus.Urgent;

                if ((severe || hasMineViolation) && inMine)
                    return RecoveryPlanMoodStatus.Urgent;

                if (plan.TodayViolations.Count(v => v.Severity == RecoveryPlanTaskSeverity.Danger) >= 2)
                    return RecoveryPlanMoodStatus.Urgent;
            }

            if (debuffState.ReadyForNextPhase
                || debuffState.ReadyForRecovery
                || _stateManager.State.ActiveComplications.Count > 0
                || plan.NeedsHarveyVisit)
                return RecoveryPlanMoodStatus.NeedsHarveyTalk;

            if (plan.TodayViolations.Count == 1 || plan.ConcernScore >= 2)
                return RecoveryPlanMoodStatus.HarveyConcerned;

            if (plan.TodayViolations.Count > 1)
                return RecoveryPlanMoodStatus.HarveyConcerned;

            return RecoveryPlanMoodStatus.Calm;
        }

        /// <returns>true если нарушение добавлено впервые сегодня.</returns>
        public bool AddViolation(
            string violationId,
            RecoveryPlanTaskSeverity severity = RecoveryPlanTaskSeverity.Warning,
            string? hudMessage = null,
            string? harveyLine = null)
        {
            var plan = _stateManager.GetRecoveryPlan();
            if (!plan.IsActive)
                return false;

            if (plan.TodayViolations.Any(v =>
                    string.Equals(v.Id, violationId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(v.Reason, violationId, StringComparison.OrdinalIgnoreCase)))
                return false;

            int today = GameUtils.Today();
            plan.TodayViolations.Add(new RecoveryPlanViolation
            {
                Id = violationId,
                Reason = violationId,
                Day = today,
                TimeOfDay = Game1.timeOfDay,
                LocationName = Game1.currentLocation?.NameOrUniqueName ?? Game1.currentLocation?.Name ?? "",
                Severity = severity,
            });

            plan.ConcernScore++;
            ApplyViolationFlagsToTasks(plan.Tasks, plan.TodayViolations);

            if (!string.IsNullOrEmpty(plan.ActiveInjuryId)
                && _stateManager.State.ActiveDebuffs.TryGetValue(plan.ActiveInjuryId, out DebuffState? ds)
                && ds != null)
                plan.Status = CalculateStatus(plan, ds);

            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] Violation: {violationId}, severity={severity}, concern={plan.ConcernScore}, status={plan.Status}",
                LogLevel.Info);

            if (!string.IsNullOrWhiteSpace(hudMessage))
                TryShowHudOnce($"violation_{violationId}", hudMessage);

            if (!string.IsNullOrWhiteSpace(harveyLine))
                _monitor.Log($"[RecoveryPlan] Harvey: {harveyLine}", LogLevel.Debug);

            return true;
        }

        /// <summary>
        /// Единая запись нарушения плана: тип, severity, причина, topic для CP (один главный диалог в день).
        /// </summary>
        /// <returns>true, если нарушение зарегистрировано впервые сегодня для этого типа.</returns>
        public bool RegisterRecoveryPlanViolation(
            string type,
            int severity = RecoveryViolationSeverity.Medium,
            string? readableReason = null,
            string? hudMessage = null,
            string? harveyLine = null) =>
            HandleRecoveryPlanViolation(type, severity, readableReason, hudMessage, harveyLine);

        /// <summary>
        /// Центральная обработка нарушения режима: день, продление, topics, HUD.
        /// Не продлевает план бесконечно; один тип — один раз за день; не более одного продления за день.
        /// </summary>
        public bool HandleRecoveryPlanViolation(
            string violationType,
            int severity,
            string? readableReason = null,
            string? hudMessage = null,
            string? harveyLine = null)
        {
            if (!HasActiveRecoveryContext() || string.IsNullOrWhiteSpace(violationType))
                return false;

            if (!IsDailyPlanActive())
                return false;

            string normalizedType = RecoveryPlanViolationTopicMap.ResolveViolationType(violationType);
            if (string.IsNullOrEmpty(normalizedType))
                normalizedType = violationType.Trim();

            severity = Math.Clamp(severity, RecoveryViolationSeverity.Mild, RecoveryViolationSeverity.Severe);

            string canonical = RecoveryPlanViolationReasonTexts.CanonicalizeReasonId(
                violationType.Trim().ToLowerInvariant());
            if (string.IsNullOrEmpty(canonical) || canonical == RecoveryPlanReasonIds.Unknown)
                canonical = MapViolationTypeToReasonId(normalizedType);

            var plan = _stateManager.GetRecoveryPlan();
            EnsureTodayViolationReasons(plan);

            if (plan.TodayViolationTypes.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
                return false;

            string reasonText = RecoveryPlanViolationTopicMap.GetReadableReason(normalizedType, readableReason);

            plan.LastViolationType = normalizedType;
            plan.LastViolationSeverity = severity;
            plan.TodayViolationTypes.Add(normalizedType);

            if (!plan.TodayViolationReasons.Contains(canonical, StringComparer.OrdinalIgnoreCase))
                plan.TodayViolationReasons.Add(canonical);

            plan.TodayFailed = true;
            _stateManager.State.RecoveryPlanDayFailed = true;

            if (severity == RecoveryViolationSeverity.Mild)
            {
                plan.HadWarningsToday = true;
                plan.HadPlanWarnings = true;
            }
            else
            {
                plan.HadPlanViolations = true;
            }

            if (severity >= RecoveryViolationSeverity.Medium)
            {
                plan.NeedsHarveyVisit = true;
                _stateManager.State.RecoveryPlanNeedsHarveyVisit = true;
            }

            bool extended = false;
            bool maxExtensionsHit = false;

            if (severity == RecoveryViolationSeverity.Severe)
            {
                if (TryExtendRecoveryPlanForViolation(plan))
                {
                    extended = true;
                }
                else if (plan.ExtensionCount >= _config.MaxRecoveryPlanExtensions
                         || plan.MaxExtensionsReached)
                {
                    plan.MaxExtensionsReached = true;
                    plan.NeedsStrictFollowUp = true;
                    plan.RequiredFollowUpDay = GameUtils.Today();
                    maxExtensionsHit = true;
                }
            }

            if (TryMapReasonToViolationId(canonical, severity, out string violationId, out RecoveryPlanTaskSeverity taskSeverity))
                AddViolation(violationId, taskSeverity, harveyLine: harveyLine);

            ApplyViolationConversationTopics(plan, normalizedType, severity, extended, maxExtensionsHit);
            SyncPlanTotalViolations(plan);
            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] HandleViolation: type={normalizedType}, reason={canonical}, "
                + $"severity={severity}, extended={extended}, maxHit={maxExtensionsHit}, "
                + $"extensionCount={plan.ExtensionCount}/{_config.MaxRecoveryPlanExtensions}, "
                + $"readable={reasonText}",
                LogLevel.Info);

            ShowViolationHud(severity, extended, maxExtensionsHit, hudMessage, normalizedType);

            return true;
        }

        /// <summary>Дефолтная тяжесть нарушения по типу и текущему состоянию игрока.</summary>
        public int ResolveDefaultViolationSeverity(string violationType)
        {
            string normalized = RecoveryPlanViolationTopicMap.ResolveViolationType(violationType);
            if (string.IsNullOrEmpty(normalized))
                normalized = violationType?.Trim() ?? "";

            string? injuryId = ResolveMainInjuryId();
            bool isHeavyInjury = injuryId != null && InjurySets.Severe.Contains(injuryId);
            bool hasBandage = HasBandageExposure();

            return normalized switch
            {
                RecoveryPlanViolationType.Mine => isHeavyInjury
                    ? RecoveryViolationSeverity.Severe
                    : RecoveryViolationSeverity.Medium,
                RecoveryPlanViolationType.LowStamina => Game1.player.Stamina
                    <= Game1.player.MaxStamina * StaminaViolationFraction
                        ? RecoveryViolationSeverity.Medium
                        : RecoveryViolationSeverity.Mild,
                RecoveryPlanViolationType.LowHealth => Game1.player.health
                    <= Game1.player.maxHealth * HealthViolationFraction
                        ? RecoveryViolationSeverity.Severe
                        : RecoveryViolationSeverity.Medium,
                RecoveryPlanViolationType.LateNight => RecoveryViolationSeverity.Medium,
                RecoveryPlanViolationType.Rain => hasBandage
                    ? RecoveryViolationSeverity.Medium
                    : RecoveryViolationSeverity.Mild,
                RecoveryPlanViolationType.IgnoredCheckup => RecoveryViolationSeverity.Medium,
                RecoveryPlanViolationType.PassedOut => RecoveryViolationSeverity.Severe,
                _ => RecoveryViolationSeverity.Medium,
            };
        }

        private bool TryExtendRecoveryPlanForViolation(RecoveryPlanState plan)
        {
            if (!_config.EnableRecoveryPlanExtensions || !_config.SevereViolationExtendsRecoveryPlan)
                return false;

            if (plan.MaxExtensionsReached)
                return false;

            if (plan.ExtensionCount >= _config.MaxRecoveryPlanExtensions)
                return false;

            int today = GameUtils.Today();
            if (plan.LastExtensionDay == today || _stateManager.State.RecoveryPlanExtendedToday)
                return false;

            if (!ExtendRecoveryPlanByOneDay())
                return false;

            plan.ExtensionCount++;
            plan.LastExtensionDay = today;
            _stateManager.State.RecoveryPlanExtendedToday = true;
            RefreshPlanForToday();
            return true;
        }

        private void ShowViolationHud(
            int severity,
            bool extended,
            bool maxExtensionsHit,
            string? customHud,
            string violationType)
        {
            if (!string.IsNullOrWhiteSpace(customHud))
            {
                TryShowHudOnce($"reason_{violationType}", customHud);
                return;
            }

            if (maxExtensionsHit)
            {
                TryShowHudOnce("max_extensions_reached", RecoveryPlanTexts.Hud.MaxExtensionsReached);
                return;
            }

            if (extended)
            {
                TryShowHudOnce("plan_extended", RecoveryPlanTexts.Hud.PlanExtended);
                return;
            }

            switch (severity)
            {
                case RecoveryViolationSeverity.Mild:
                    TryShowHudOnce("mild_violation", RecoveryPlanTexts.Hud.MildViolation);
                    break;
                default:
                    TryShowViolationRecordedHud();
                    break;
            }
        }

        private static string MapViolationTypeToReasonId(string violationType) => violationType switch
        {
            RecoveryPlanViolationType.Mine => RecoveryPlanReasonIds.EnteredMine,
            RecoveryPlanViolationType.LowStamina => RecoveryPlanReasonIds.StaminaTooLow,
            RecoveryPlanViolationType.LowHealth => RecoveryPlanReasonIds.HealthTooLow,
            RecoveryPlanViolationType.LateNight => RecoveryPlanReasonIds.TooLate,
            RecoveryPlanViolationType.Rain => RecoveryPlanReasonIds.RainBandage,
            RecoveryPlanViolationType.IgnoredCheckup => RecoveryPlanReasonIds.MissedHarveyCheckup,
            RecoveryPlanViolationType.PassedOut => RecoveryPlanReasonIds.PassedOut,
            _ => RecoveryPlanReasonIds.Unknown,
        };

        /// <summary>HUD-предупреждение без записи нарушения (мягкая страховка и т.п.).</summary>
        public void ShowRecoveryPlanWarning(string hudKey, string message, Action<RecoveryPlanState, int>? markWarningDay = null)
        {
            if (!IsDailyPlanActive())
                return;

            int today = GameUtils.Today();
            var plan = _stateManager.GetRecoveryPlan();

            if (markWarningDay != null)
            {
                if (hudKey == "stamina_soft_warning" && plan.LastStaminaWarningDay == today)
                    return;
                if (hudKey == "health_soft_warning" && plan.LastHealthWarningDay == today)
                    return;
            }

            markWarningDay?.Invoke(plan, today);
            plan.HadWarningsToday = true;
            plan.HadPlanWarnings = true;
            _stateManager.Save();

            _monitor.Log($"[RecoveryPlan] Warning shown: {hudKey}, day={today}", LogLevel.Debug);
            TryShowHudOnce(hudKey, message);
        }

        public void ClearPlanIfRecovered()
        {
            if (HasRecoveryContext())
                return;

            var plan = _stateManager.GetRecoveryPlan();
            if (!plan.IsActive && string.IsNullOrEmpty(plan.ActiveInjuryId))
                return;

            ResetDailyPlanState(plan);
            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] План очищен — травм и осложнений нет", LogLevel.Info);
        }

        public void ClearDailyPlan()
        {
            ResetDailyPlanState(_stateManager.GetRecoveryPlan());
            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] Daily plan cleared (debug)", LogLevel.Info);
        }

        public bool IsDailyPlanActive() => _stateManager.GetRecoveryPlan().IsActive;

        public int GetMaxExtensions() => _config.MaxRecoveryPlanExtensions;

        public RecoveryPlanState GetDailyPlan() => _stateManager.GetRecoveryPlan();

        // ============================================================================
        // Контекст восстановления и нарушения по тяжести
        // ============================================================================

        /// <summary>
        /// Активен план восстановления или идёт лечение (DebuffState / лечебные баффы мода).
        /// </summary>
        public bool HasActiveRecoveryContext()
        {
            if (IsDailyPlanActive() || HasActivePlan())
                return true;

            if (_stateManager.State.ActiveDebuffs.Values.Any(d => d.TreatmentStarted))
                return true;

            return HasActiveTreatmentOrPhaseBuff();
        }

        public void OnRecoveryContextDayStarted()
        {
            TryShowMildViolationMorningReminder();
            _overworkLowStaminaToolSeconds = 0;
            _morningHudRetryCount = 0;

            var daily = _stateManager.GetRecoveryPlan();
            if (daily.IsActive)
            {
                daily.TodayFailed = false;
                daily.TodayCompleted = false;
                EnsureTodayViolationReasons(daily);
                daily.TodayViolationReasons.Clear();
                daily.TodayViolationTypes.Clear();
                daily.HadWarningsToday = false;
                daily.TodayViolationDialogueType = "";
                daily.TodayViolationDialogueSeverity = 0;
            }

            _stateManager.ResetRecoveryViolationDailyState();
        }

        /// <summary>Запланировать утренний HUD о плане (после buff restore / RefreshPlanForToday).</summary>
        public void ScheduleMorningPlanHud()
        {
            Game1.delayedActions.Add(new DelayedAction(1500, TryShowMorningPlanHud));
        }

        /// <summary>Утреннее напоминание: план активен и клавиша открытия окна.</summary>
        public void TryShowMorningPlanHud()
        {
            if (!HasActiveRecoveryPlanForHud())
                return;

            var plan = _stateManager.GetRecoveryPlan();
            int today = GameUtils.Today();
            if (plan.LastMorningHudDay == today)
                return;

            if (!CanShowRecoveryPlanHudNow())
            {
                if (_morningHudRetryCount < 2)
                {
                    _morningHudRetryCount++;
                    Game1.delayedActions.Add(new DelayedAction(1000, TryShowMorningPlanHud));
                }

                _monitor.Log("[RecoveryPlan] Morning HUD отложен — событие или игрок занят", LogLevel.Debug);
                return;
            }

            if (!TryGetMorningHudDayProgress(out int currentDay, out int totalDays))
                return;

            plan.LastMorningHudDay = today;
            _stateManager.Save();

            string message = string.Format(
                "План восстановления активен: день {0}/{1}. Нажми {2}, чтобы посмотреть предписания Харви.",
                currentDay,
                totalDays,
                FormatRecoveryPlanKey());

            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.health_type));
            _monitor.Log(
                $"[RecoveryPlan] Morning HUD shown: day {currentDay}/{totalDays}, key={FormatRecoveryPlanKey()}",
                LogLevel.Info);
        }

        /// <summary>Проверки stamina: лёгкое на ферме и среднее перегрузка.</summary>
        public void CheckRecoveryStaminaViolations(int currentTick)
        {
            if (!HasActiveRecoveryContext() || _stateManager.State.WasPassedOut)
            {
                _overworkLowStaminaToolSeconds = 0;
                return;
            }

            if (currentTick - _lastStaminaCheckTick < StaminaCheckIntervalTicks)
                return;

            _lastStaminaCheckTick = currentTick;

            if (IsDailyPlanActive())
                CheckDailyPlanStaminaSoftInsurance();
            else
            {
                CheckMildFarmStaminaViolation();
                CheckOverworkStaminaViolation();
            }
        }

        public void CheckMildFarmStaminaViolation()
        {
            if (!HasActiveRecoveryContext() || _stateManager.State.WasPassedOut)
                return;

            if (!IsFarmArea(Game1.currentLocation) || IsHeavyToolInUse())
                return;

            float max = Math.Max(1f, Game1.player.MaxStamina);
            if (Game1.player.Stamina > max * CriticalStaminaFraction)
                return;

            if (!RegisterSeverityViolation(
                    RecoveryViolationTypes.LowStaminaFarm,
                    RecoveryViolationSeverity.Mild,
                    failDay: false,
                    needsHarveyVisit: false,
                    HudMildStamina))
                return;

        }

        public void CheckOverworkStaminaViolation()
        {
            if (!HasActiveRecoveryContext() || _stateManager.State.WasPassedOut)
            {
                _overworkLowStaminaToolSeconds = 0;
                return;
            }

            if (!IsHeavyToolInUse())
            {
                _overworkLowStaminaToolSeconds = 0;
                return;
            }

            float max = Math.Max(1f, Game1.player.MaxStamina);
            if (Game1.player.Stamina > max * CriticalStaminaFraction)
            {
                _overworkLowStaminaToolSeconds = 0;
                return;
            }

            _overworkLowStaminaToolSeconds++;
            if (_overworkLowStaminaToolSeconds < OverworkViolationSeconds)
                return;

            _overworkLowStaminaToolSeconds = 0;

            if (!RegisterSeverityViolation(
                    RecoveryViolationTypes.Overwork,
                    RecoveryViolationSeverity.Medium,
                    failDay: true,
                    needsHarveyVisit: true,
                    HudMediumDayFailed))
                return;

            if (IsDailyPlanActive())
            {
                RegisterRecoveryPlanViolation(
                    RecoveryPlanViolationType.LowStamina,
                    RecoveryViolationSeverity.Medium,
                    harveyLine: RecoveryPlanTexts.Harvey.Stamina);
                RefreshPlanForToday();
            }
        }

        public void CheckRainBandageViolation(int secondsUnderRain)
        {
            if (!HasActiveRecoveryContext())
                return;

            if (secondsUnderRain < RainBandageViolationSeconds || !HasBandageExposure())
                return;

            string rainHud = IsDailyPlanActive() ? "" : HudMediumDayFailed;
            if (!RegisterSeverityViolation(
                    RecoveryViolationTypes.RainWithBandage,
                    RecoveryViolationSeverity.Medium,
                    failDay: true,
                    needsHarveyVisit: true,
                    rainHud))
                return;

            if (IsDailyPlanActive())
            {
                RegisterRecoveryPlanViolation(
                    RecoveryPlanViolationType.Rain,
                    ResolveDefaultViolationSeverity(RecoveryPlanViolationType.Rain));
                RefreshPlanForToday();
            }
        }

        // ============================================================================
        // Проверки правил дня (промпт 4)
        // ============================================================================

        /// <summary>Устарело: вход обрабатывает <see cref="MineEntryCoordinator"/> в PlayerEventHandler.</summary>
        public void CheckMineEntryOnWarp(GameLocation? location)
        {
            // Логика перенесена в MineEntryCoordinator (PlayerEventHandler.HandleMinesLogic).
        }

        /// <summary>Зарегистрировать нарушение шахты в RecoveryPlan (мягкое или severe).</summary>
        public void RegisterRecoveryPlanMineViolation(int severity, bool blockPhysicalExit)
        {
            if (!HasActiveRecoveryContext())
                return;

            bool failDay = severity >= RecoveryViolationSeverity.Medium;
            bool needsVisit = severity >= RecoveryViolationSeverity.Severe;

            if (!IsDailyPlanActive())
            {
                if (severity >= RecoveryViolationSeverity.Severe)
                {
                    RegisterSeverityViolation(
                        RecoveryViolationTypes.MineOrVolcano,
                        RecoveryViolationSeverity.Severe,
                        failDay: true,
                        needsHarveyVisit: true,
                        severity >= RecoveryViolationSeverity.Severe ? HudSevereViolation : "");
                }

                return;
            }

            RegisterRecoveryPlanViolation(
                RecoveryPlanViolationType.Mine,
                severity,
                harveyLine: RecoveryPlanTexts.Harvey.Mines);

            if (failDay)
            {
                var plan = _stateManager.GetRecoveryPlan();
                plan.TodayFailed = true;
                _stateManager.State.RecoveryPlanDayFailed = true;
            }

            if (needsVisit)
                _stateManager.State.RecoveryPlanNeedsHarveyVisit = true;

            RefreshPlanForToday();
            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] Mine violation: severity={severity}, blockExit={blockPhysicalExit}",
                LogLevel.Info);
        }

        public void CheckLowHealthViolation(int currentTick)
        {
            if (!HasActiveRecoveryContext() || currentTick - _lastHealthCheckTick < HealthCheckIntervalTicks)
                return;

            _lastHealthCheckTick = currentTick;

            if (IsDailyPlanActive())
                CheckDailyPlanHealthSoftInsurance();
            else
                CheckLegacyLowHealthViolation();
        }

        /// <summary>Мягкая страховка stamina для ежедневного плана: предупреждение 25%, нарушение 15%.</summary>
        private void CheckDailyPlanStaminaSoftInsurance()
        {
            if (!IsDailyPlanActive() || _stateManager.State.WasPassedOut)
                return;

            var plan = _stateManager.GetRecoveryPlan();
            int today = GameUtils.Today();
            float max = Math.Max(1f, Game1.player.MaxStamina);

            if (Game1.player.Stamina > max * StaminaWarningFraction)
                return;

            if (plan.LastStaminaWarningDay != today)
            {
                ShowStaminaSoftWarning(plan, today);
                return;
            }

            if (Game1.player.Stamina > max * StaminaViolationFraction)
                return;

            RegisterDailyStaminaViolation();
        }

        /// <summary>Мягкая страховка health для ежедневного плана: предупреждение 45%, нарушение 35%.</summary>
        private void CheckDailyPlanHealthSoftInsurance()
        {
            if (!IsDailyPlanActive())
                return;

            var plan = _stateManager.GetRecoveryPlan();
            int today = GameUtils.Today();
            float max = Math.Max(1f, Game1.player.maxHealth);

            if (Game1.player.health > max * HealthWarningFraction)
                return;

            if (plan.LastHealthWarningDay != today)
            {
                ShowHealthSoftWarning(plan, today);
                return;
            }

            if (Game1.player.health > max * HealthViolationFraction)
                return;

            RegisterDailyHealthViolation();
        }

        private void CheckLegacyLowHealthViolation()
        {
            int warnHp = (int)Math.Ceiling(Game1.player.maxHealth * WarnHealthFraction);
            int criticalHp = (int)Math.Ceiling(Game1.player.maxHealth * (CriticalHealthFractionPercent / 100f));

            if (Game1.player.health > warnHp)
                return;

            if (Game1.player.health <= criticalHp)
            {
                RegisterSeverityViolation(
                    RecoveryViolationTypes.CriticalHealth,
                    RecoveryViolationSeverity.Severe,
                    failDay: true,
                    needsHarveyVisit: true,
                    HudSevereViolation);
            }
        }

        private void ShowStaminaSoftWarning(RecoveryPlanState plan, int today)
        {
            _monitor.Log(
                $"[RecoveryPlan] Stamina soft warning shown (stamina={(int)Game1.player.Stamina}/{(int)Game1.player.MaxStamina}, day={today})",
                LogLevel.Info);

            ShowRecoveryPlanWarning(
                "stamina_soft_warning",
                HudStaminaSoftWarning,
                (p, d) => p.LastStaminaWarningDay = d);
        }

        private void ShowHealthSoftWarning(RecoveryPlanState plan, int today)
        {
            _monitor.Log(
                $"[RecoveryPlan] Health soft warning shown (health={Game1.player.health}/{Game1.player.maxHealth}, day={today})",
                LogLevel.Info);

            ShowRecoveryPlanWarning(
                "health_soft_warning",
                HudHealthSoftWarning,
                (p, d) => p.LastHealthWarningDay = d);
        }

        private void RegisterDailyStaminaViolation()
        {
            _monitor.Log(
                $"[RecoveryPlan] Stamina violation registered (stamina={(int)Game1.player.Stamina}/{(int)Game1.player.MaxStamina})",
                LogLevel.Info);

            if (!RegisterSeverityViolation(
                    RecoveryViolationTypes.StaminaTooLow,
                    RecoveryViolationSeverity.Medium,
                    failDay: true,
                    needsHarveyVisit: true,
                    hudMessage: ""))
                return;

            RegisterRecoveryPlanViolation(
                RecoveryPlanViolationType.LowStamina,
                ResolveDefaultViolationSeverity(RecoveryPlanViolationType.LowStamina),
                harveyLine: RecoveryPlanTexts.Harvey.Stamina);
            RefreshPlanForToday();
        }

        private void RegisterDailyHealthViolation()
        {
            _monitor.Log(
                $"[RecoveryPlan] Health violation registered (health={Game1.player.health}/{Game1.player.maxHealth})",
                LogLevel.Info);

            if (!RegisterSeverityViolation(
                    RecoveryViolationTypes.HealthTooLow,
                    RecoveryViolationSeverity.Severe,
                    failDay: true,
                    needsHarveyVisit: true,
                    hudMessage: ""))
                return;

            RegisterRecoveryPlanViolation(
                RecoveryPlanViolationType.LowHealth,
                ResolveDefaultViolationSeverity(RecoveryPlanViolationType.LowHealth),
                harveyLine: RecoveryPlanTexts.Harvey.Health);
            RefreshPlanForToday();
        }

        public void ProcessDayEndingDaily()
        {
            if (!HasActiveRecoveryContext())
                return;

            bool needsSleepRule = false;
            if (IsDailyPlanActive())
            {
                needsSleepRule = _stateManager.GetRecoveryPlan()
                    .Tasks.Any(t => t.Id == RecoveryPlanTaskIds.SleepBeforeMidnight);
            }
            else
            {
                string? injuryId = ResolveMainInjuryId();
                needsSleepRule = !string.IsNullOrEmpty(injuryId)
                    && NeedsSleepRule(injuryId, _stateManager.State);
            }

            if (Game1.timeOfDay >= RestBeforeTime && needsSleepRule)
            {
                string lateHud = IsDailyPlanActive() ? "" : HudMediumDayFailed;
                RegisterSeverityViolation(
                    RecoveryViolationTypes.LateNight,
                    RecoveryViolationSeverity.Medium,
                    failDay: true,
                    needsHarveyVisit: true,
                    lateHud);

                if (IsDailyPlanActive())
                {
                    RegisterRecoveryPlanViolation(
                        RecoveryPlanViolationType.LateNight,
                        RecoveryViolationSeverity.Medium,
                        harveyLine: RecoveryPlanTexts.Harvey.LateNight);
                }
            }

            var plan = _stateManager.GetRecoveryPlan();
            if (plan.IsActive)
            {
                if (plan.TodayViolations.Count == 0 && plan.ConcernScore > 0)
                    plan.ConcernScore = Math.Max(0, plan.ConcernScore - 1);

                _stateManager.Save();
                _monitor.Log(
                    $"[RecoveryPlan] DayEnding: violations={plan.TodayViolations.Count}, concern={plan.ConcernScore}",
                    LogLevel.Debug);
            }

            int todaySeverity = GetTodayMaxViolationSeverity();
            if (todaySeverity == RecoveryViolationSeverity.Mild)
                TryShowHudOnce("mild_evening_reminder", HudMildEveningReminder);

            OnRecoveryPlanDayEnding();
        }

        public void CheckLateNightReminder(int newTime)
        {
            if (!IsDailyPlanActive() || newTime < LateNightReminderTime || newTime >= RestBeforeTime)
                return;

            var plan = _stateManager.GetRecoveryPlan();
            if (!plan.Tasks.Any(t => t.Id == RecoveryPlanTaskIds.SleepBeforeMidnight))
                return;

            TryShowHudOnce("late_night_reminder", RecoveryPlanTexts.Hud.LateNightReminder);
        }

        public void NotifyTreatmentCompleted()
        {
            ClearPlanIfRecovered();
            TryShowHudOnce("plan_completed", RecoveryPlanTexts.Hud.PlanCompleted);
        }

        public void AddTestViolation(string id)
        {
            string normalized = id.Trim().ToLowerInvariant();
            (string violationType, int severity) = normalized switch
            {
                "mines" or "mine" or "volcano" => (RecoveryPlanViolationType.Mine, RecoveryViolationSeverity.Severe),
                "stamina" => (RecoveryPlanViolationType.LowStamina, RecoveryViolationSeverity.Medium),
                "health" => (RecoveryPlanViolationType.LowHealth, RecoveryViolationSeverity.Severe),
                "sleep" or "late" or "night" => (RecoveryPlanViolationType.LateNight, RecoveryViolationSeverity.Medium),
                "rain" => (RecoveryPlanViolationType.Rain, RecoveryViolationSeverity.Medium),
                "missed_checkup" or "checkup" => (RecoveryPlanReasonIds.MissedHarveyCheckup, RecoveryViolationSeverity.Medium),
                "overwork" or "heavy_work" => (RecoveryPlanViolationType.LowStamina, RecoveryViolationSeverity.Medium),
                _ => (RecoveryPlanViolationTopicMap.ResolveViolationType(id), RecoveryViolationSeverity.Medium),
            };

            RegisterRecoveryPlanViolation(violationType, severity);
            RefreshPlanForToday();
        }

        /// <summary>QA: симулировать типизированное нарушение (recovery_violate).</summary>
        public bool DebugViolateRecoveryPlan(string arg) =>
            DebugHandleRecoveryPlanViolation(arg, severity: null);

        /// <summary>QA: нарушение с явной тяжестью (injury_recovery_violate).</summary>
        public bool DebugHandleRecoveryPlanViolation(string typeArg, int? severity)
        {
            if (string.IsNullOrWhiteSpace(typeArg))
                return false;

            string? violationType = ResolveDebugViolationType(typeArg.Trim());
            if (violationType == null)
                return false;

            int resolvedSeverity = severity ?? ResolveDefaultViolationSeverity(violationType);

            if (!IsDailyPlanActive())
            {
                if (!HasActiveRecoveryContext())
                    RefreshPlanForToday();
                if (!IsDailyPlanActive())
                    return false;
            }

            bool registered = HandleRecoveryPlanViolation(violationType, resolvedSeverity);
            if (registered)
                RefreshPlanForToday();

            return registered;
        }

        private static string? ResolveDebugViolationType(string normalized) => normalized.ToLowerInvariant() switch
        {
            "mine" or "mines" => RecoveryPlanViolationType.Mine,
            "stamina" or "lowstamina" => RecoveryPlanViolationType.LowStamina,
            "health" or "lowhealth" => RecoveryPlanViolationType.LowHealth,
            "night" or "late" or "latenight" => RecoveryPlanViolationType.LateNight,
            "rain" => RecoveryPlanViolationType.Rain,
            "checkup" or "ignoredcheckup" or "missed_checkup" => RecoveryPlanViolationType.IgnoredCheckup,
            "passout" or "passedout" => RecoveryPlanViolationType.PassedOut,
            _ when RecoveryPlanViolationTopicMap.ResolveViolationType(normalized) is { Length: > 0 } resolved
                => resolved,
            _ => null,
        };

        /// <summary>QA: завершить план с заданным исходом (recovery_complete / injury_recovery_complete_debug).</summary>
        public bool DebugCompleteRecoveryPlan(string arg)
        {
            string normalized = arg.Trim().ToLowerInvariant();
            string result = normalized switch
            {
                "perfect" => RecoveryPlanCompletionResult.Perfect,
                "warnings" or "warning" => RecoveryPlanCompletionResult.WithWarnings,
                "violated" or "normal" => RecoveryPlanCompletionResult.Normal,
                _ => "",
            };

            if (string.IsNullOrEmpty(result))
                return false;

            var plan = _stateManager.GetRecoveryPlan();
            var state = _stateManager.State;
            plan.CompletionRewardApplied = false;

            switch (result)
            {
                case RecoveryPlanCompletionResult.Perfect:
                    plan.HadPlanWarnings = false;
                    plan.HadPlanViolations = false;
                    plan.WarningDays = 0;
                    plan.PlanExtensionDays = 0;
                    plan.PerfectDays = Math.Max(plan.PerfectDays, Math.Max(1, plan.CreditedDays));
                    state.RecoveryPlanTotalViolations = 0;
                    state.RecoveryPlanMildViolations = 0;
                    state.RecoveryPlanMediumViolations = 0;
                    state.RecoveryPlanSevereViolations = 0;
                    break;
                case RecoveryPlanCompletionResult.WithWarnings:
                    plan.HadPlanWarnings = true;
                    plan.WarningDays = Math.Max(1, plan.WarningDays);
                    state.RecoveryPlanTotalViolations = 0;
                    state.RecoveryPlanMediumViolations = 0;
                    state.RecoveryPlanSevereViolations = 0;
                    break;
                case RecoveryPlanCompletionResult.Normal:
                    plan.HadPlanWarnings = false;
                    plan.HadPlanViolations = true;
                    state.RecoveryPlanTotalViolations = 1;
                    state.RecoveryPlanMildViolations = 1;
                    state.RecoveryPlanMediumViolations = 0;
                    state.RecoveryPlanSevereViolations = 0;
                    break;
            }

            SyncPlanTotalViolations(plan);
            _stateManager.Save();

            var hospital = GetActivePlan();
            if (hospital != null)
            {
                hospital.IsActive = false;
                hospital.CompletionTalkPending = true;
                hospital.RequiresHarveyTalk = true;
            }

            CompleteRecoveryPlan(result);
            return true;
        }

        /// <summary>Завершить план восстановления и выдать награду (один раз за план).</summary>
        public void CompleteRecoveryPlan(string? forcedResult = null)
        {
            var plan = _stateManager.GetRecoveryPlan();
            SyncPlanTotalViolations(plan);

            if (plan.CompletionRewardApplied)
            {
                _monitor.Log(
                    "[RecoveryPlan] Completion reward skipped — already applied for this plan",
                    LogLevel.Info);
                return;
            }

            string result = forcedResult ?? ResolveCompletionResult(plan);
            bool perfectPlan = plan.TotalViolations == 0;

            plan.WasPerfectOnCompletion = result == RecoveryPlanCompletionResult.Perfect;
            plan.LastCompletionWasPerfect = plan.WasPerfectOnCompletion;
            plan.LastCompletionHadWarnings = result == RecoveryPlanCompletionResult.WithWarnings;

            _monitor.Log(
                $"[RecoveryPlan] Plan completed: result={result}, totalViolations={plan.TotalViolations}, "
                + $"perfectPlan={perfectPlan}, warnings={plan.HadPlanWarnings}",
                LogLevel.Info);

            switch (result)
            {
                case RecoveryPlanCompletionResult.Perfect:
                    ApplyPerfectRecoveryReward();
                    break;
                case RecoveryPlanCompletionResult.WithWarnings:
                    ApplyRecoveryCompletedWithWarnings();
                    break;
                default:
                    ApplyRecoveryCompletedNormal();
                    break;
            }

            plan.CompletionRewardApplied = true;
            _stateManager.Save();
        }

        /// <summary>Идеальное завершение: friendship, buffHarveyCare, topics, мягкий тон.</summary>
        public void ApplyPerfectRecoveryReward()
        {
            int friendship = HarveyHelper.IsDatingOrMarriedToHarvey()
                ? PerfectRomanticFriendshipPoints
                : PerfectFriendshipPoints;
            AddHarveyFriendshipSafe(friendship);

            int careDays = Game1.random.Next(1, 3);
            _buffManager.AddBuff(CureBuffs.Care, careDays * 1440);

            int topicDays = Game1.random.Next(PerfectTopicDaysMin, PerfectTopicDaysMax + 1);
            AddRecoveryCompletionTopic(ConversationTopics.RecoveryPlanPerfect, topicDays);

            int today = GameUtils.Today();
            var plan = _stateManager.GetRecoveryPlan();
            plan.SoftToneUntilDay = today + SoftToneDays;
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanSoftTone);
            _dialogueManager.AddTopic(ConversationTopics.RecoveryPlanSoftTone, SoftToneDays);

            ShowCompletionDialogue(RecoveryPlanCompletionResult.Perfect);
            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] Perfect reward applied: friendship={friendship}, careDays={careDays}, "
                + $"topicDays={topicDays}, softToneUntil={plan.SoftToneUntilDay}",
                LogLevel.Info);
        }

        /// <summary>Завершение с предупреждениями: topic, без perfect-награды.</summary>
        public void ApplyRecoveryCompletedWithWarnings()
        {
            AddRecoveryCompletionTopic(
                ConversationTopics.RecoveryPlanCompletedWithWarnings,
                CompletedTopicDays);
            ShowCompletionDialogue(RecoveryPlanCompletionResult.WithWarnings);
            _monitor.Log("[RecoveryPlan] Completion with warnings — no perfect reward", LogLevel.Info);
        }

        /// <summary>Завершение без тяжёлых нарушений: topic + небольшой friendship.</summary>
        public void ApplyRecoveryCompletedNormal()
        {
            AddRecoveryCompletionTopic(
                ConversationTopics.RecoveryPlanCompletedNormal,
                CompletedTopicDays);
            AddHarveyFriendshipSafe(NormalCompletionFriendshipPoints);
            ShowCompletionDialogue(RecoveryPlanCompletionResult.Normal);
            _monitor.Log(
                $"[RecoveryPlan] Normal completion: friendship +{NormalCompletionFriendshipPoints}",
                LogLevel.Info);
        }

        /// <summary>Безопасное начисление friendship Харви (только положительные очки).</summary>
        public void AddHarveyFriendshipSafe(int points)
        {
            if (points <= 0)
                return;

            NPC? harvey = HarveyHelper.GetHarvey();
            if (harvey == null)
            {
                _monitor.Log($"[RecoveryPlan] Friendship +{points} skipped — Harvey NPC missing", LogLevel.Warn);
                return;
            }

            Game1.player.changeFriendship(points, harvey);
            _monitor.Log($"[RecoveryPlan] Friendship +{points} (Harvey)", LogLevel.Info);
        }

        /// <summary>Добавить topic завершения плана (снимает старые completion-topics).</summary>
        public void AddRecoveryCompletionTopic(string topicId, int days)
        {
            RemoveCompletionTopics();
            _dialogueManager.AddTopic(topicId, days);
            _monitor.Log($"[RecoveryPlan] Completion topic added: {topicId} ({days}d)", LogLevel.Debug);
        }

        /// <summary>Сбросить только RecoveryPlan, не трогая травмы и injury debuffs.</summary>
        public void ResetRecoveryPlanOnly()
        {
            ClearRecoveryPlan();
            ResetDailyPlanState(_stateManager.GetRecoveryPlan());
            _stateManager.ClearRecoveryViolationState(includeCounters: true);
            RemoveCompletionTopics();
            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] Full recovery plan reset (injuries untouched)", LogLevel.Info);
        }

        /// <summary>Статус плана для injury_recovery_status.</summary>
        public IEnumerable<string> GetRecoveryStatusLines()
        {
            var plan = _stateManager.GetRecoveryPlan();
            var hospital = GetActivePlan();
            var state = _stateManager.State;
            SyncPlanTotalViolations(plan);

            yield return $"IsActive={plan.IsActive || (hospital?.IsActive ?? false)}";
            yield return $"CompletedDays={plan.CompletedDays}  RequiredDays={plan.RequiredDays}";
            if (hospital != null)
                yield return $"hospital: {hospital.CompletedDays}/{hospital.RequiredDays}  violations={hospital.TotalViolations}";

            yield return $"TotalViolations={plan.TotalViolations}  (mild={state.RecoveryPlanMildViolations} "
                + $"medium={state.RecoveryPlanMediumViolations} severe={state.RecoveryPlanSevereViolations})";
            yield return $"PerfectDays={plan.PerfectDays}  WarningDays={plan.WarningDays}  "
                + $"ConsecutivePerfectDays={plan.ConsecutivePerfectDays}";

            EnsureTodayViolationReasons(plan);
            string reasons = plan.TodayViolationReasons.Count > 0
                ? string.Join(", ", plan.TodayViolationReasons)
                : "(none)";
            yield return $"TodayViolationReasons=[{reasons}]";

            yield return $"WasPerfectOnCompletion={plan.WasPerfectOnCompletion}  "
                + $"LastCompletionWasPerfect={plan.LastCompletionWasPerfect}  "
                + $"LastCompletionHadWarnings={plan.LastCompletionHadWarnings}";
            yield return $"CompletionRewardApplied={plan.CompletionRewardApplied}  "
                + $"SoftToneUntilDay={plan.SoftToneUntilDay}";
            yield return $"NextCompletionTopic={RecoveryPlanViolationTopicMap.GetCompletionTopic(ResolveCompletionResult(plan))}";
            yield return $"ExtensionCount={plan.ExtensionCount}/{_config.MaxRecoveryPlanExtensions}  "
                + $"MaxExtensionsReached={plan.MaxExtensionsReached}  "
                + $"NeedsStrictFollowUp={plan.NeedsStrictFollowUp}  "
                + $"RequiredFollowUpDay={plan.RequiredFollowUpDay}  "
                + $"LastExtensionDay={plan.LastExtensionDay}";
            yield return $"LastViolationType={ValueOrDash(plan.LastViolationType)}  "
                + $"LastViolationSeverity={plan.LastViolationSeverity} ({FormatViolationSeverityLabel(plan.LastViolationSeverity)})";
            if (plan.IsActive)
            {
                HarveyCareTone tone = HarveyCareToneCalculator.Calculate(
                    plan,
                    state.CareTrust,
                    state.RecoveryPlanNeedsHarveyVisit,
                    BuildHarveyToneWorldContext());
                yield return $"HarveyTone={tone}  TodayViolations={plan.TodayViolations.Count}  "
                    + $"NeedsHarveyVisit={YesNo(plan.NeedsHarveyVisit || state.RecoveryPlanNeedsHarveyVisit)}";
            }
        }

        // ============================================================================
        // UI / debug
        // ============================================================================

        public bool HasDisplayablePlan()
        {
            var hospital = GetActivePlan();
            if (hospital is { IsActive: true } or { CompletionTalkPending: true })
                return true;

            return _stateManager.GetRecoveryPlan().IsActive;
        }

        public RecoveryPlanViewModel BuildViewModel()
        {
            var daily = _stateManager.GetRecoveryPlan();
            var hospital = GetActivePlan();
            bool hasHospital = hospital is { IsActive: true } or { CompletionTalkPending: true };

            if (!daily.IsActive && !hasHospital)
                return RecoveryPlanViewModel.Empty;

            string? injuryId = daily.ActiveInjuryId ?? hospital?.InjuryId;
            DebuffState? debuff = injuryId != null ? _stateManager.GetDebuffState(injuryId) : null;

            string phaseLabel = debuff != null && debuff.TotalPhases > 0 && debuff.CurrentPhase > 0
                ? TreatmentManager.GetPhaseDisplayName(injuryId!, debuff.CurrentPhase, debuff.TotalPhases)
                : debuff?.TreatmentStarted == true ? "Лечение" : "Ожидает лечения";

            string dayProgress = daily.IsActive
                ? BuildDayProgressLabel(daily, debuff)
                : "";

            bool hasComplication = _stateManager.State.ActiveComplications.Count > 0;
            bool isSevere = injuryId != null && InjurySets.Severe.Contains(injuryId);
            HarveyToneViewModel harveyTone = GetHarveyToneViewModel();

            return new RecoveryPlanViewModel
            {
                HasPlan = true,
                IsActive = daily.IsActive || (hospital?.IsActive ?? false),
                PlanId = hospital?.PlanId ?? "",
                Reason = hospital?.Reason ?? "treatment",
                InjuryId = injuryId,
                InjuryDisplayName = injuryId != null ? _injuryManager.GetInjuryName(injuryId) : "",
                PhaseLabel = phaseLabel,
                StartDay = hospital?.StartDay ?? daily.PlanStartDay,
                RequiredDays = hospital?.RequiredDays ?? daily.TotalDays,
                CompletedDays = hospital?.CompletedDays ?? 0,
                DaysRemaining = hospital != null
                    ? Math.Max(0, hospital.RequiredDays - hospital.CompletedDays)
                    : Math.Max(0, daily.TotalDays - daily.CurrentDay),
                TodayFailed = hasHospital
                    ? hospital!.TodayFailed
                    : daily.TodayFailed || _stateManager.State.RecoveryPlanDayFailed,
                TodayCompleted = hasHospital
                    ? hospital!.TodayCompleted
                    : daily.TodayCompleted,
                ViolationsToday = daily.TodayViolations.Count,
                TotalViolations = hospital?.TotalViolations ?? daily.TotalViolations,
                TodayViolationReasons = hasHospital
                    ? (hospital!.TodayViolationReasons ?? new List<string>()).ToList()
                    : daily.TodayViolationReasons.Count > 0
                        ? daily.TodayViolationReasons.ToList()
                        : daily.TodayViolations.Select(v => v.Reason).ToList(),
                RequiresHarveyTalk = _stateManager.State.RecoveryPlanNeedsHarveyVisit
                    || daily.NeedsHarveyVisit
                    || (hospital?.RequiresHarveyTalk ?? false),
                CompletionTalkPending = hospital?.CompletionTalkPending ?? false,
                LastEvaluatedDay = hospital?.LastEvaluatedDay ?? daily.LastUpdatedDay,
                Status = daily.IsActive ? daily.Status : RecoveryPlanMoodStatus.None,
                StatusText = FormatStatusText(daily.Status),
                StatusDescription = FormatStatusDescription(daily.Status),
                RegimeStatusText = FormatRegimeStatusShort(daily.Status),
                DayProgressText = dayProgress,
                WhyImportant = RecoveryPlanTexts.GetWhyImportant(injuryId, hasComplication, isSevere),
                ComplicationSummary = BuildComplicationSummaryFromState(),
                MainInjuryId = injuryId,
                CurrentPhase = daily.CurrentPhase,
                TotalPhases = daily.TotalPhases,
                ReadyForNextPhase = debuff?.ReadyForNextPhase ?? false,
                ReadyForRecovery = debuff?.ReadyForRecovery ?? false,
                ConcernScore = daily.ConcernScore,
                Tasks = daily.Tasks,
                Violations = daily.TodayViolations,
                HarveyTone = harveyTone,
            };
        }

        /// <summary>Вычисляет тон Харви для UI (не сохраняется в save-state).</summary>
        public HarveyToneViewModel GetHarveyToneViewModel()
        {
            var plan = _stateManager.GetRecoveryPlan();
            var hospital = GetActivePlan();
            bool hasDisplayablePlan = plan.IsActive || hospital is { IsActive: true } or { CompletionTalkPending: true };

            if (!hasDisplayablePlan)
            {
                return HarveyCareToneCalculator.BuildViewModel(HarveyCareTone.Calm, hasActivePlan: false);
            }

            if (!plan.IsActive)
            {
                return HarveyCareToneCalculator.BuildViewModel(HarveyCareTone.Calm, hasActivePlan: true);
            }

            SyncPlanTotalViolations(plan);
            var world = BuildHarveyToneWorldContext();
            bool needsHarveyVisitFromViolation = _stateManager.State.RecoveryPlanNeedsHarveyVisit;

            HarveyCareTone tone = HarveyCareToneCalculator.Calculate(
                plan,
                _stateManager.State.CareTrust,
                needsHarveyVisitFromViolation,
                world);

            return HarveyCareToneCalculator.BuildViewModel(tone, hasActivePlan: true);
        }

        private HarveyCareToneCalculator.WorldContext BuildHarveyToneWorldContext()
        {
            if (!Context.IsWorldReady)
                return HarveyCareToneCalculator.WorldContext.Unavailable;

            return new HarveyCareToneCalculator.WorldContext
            {
                IsAvailable = true,
                PlayerHealth = Game1.player.health,
                PlayerMaxHealth = Game1.player.maxHealth,
                InMineOrVolcano = IsMineOrVolcanoLocation(Game1.currentLocation),
            };
        }

        public string GetDebugHudBlock()
        {
            var plan = _stateManager.GetRecoveryPlan();
            var state = _stateManager.State;
            string active = plan.IsActive ? "active" : "inactive";
            string lastType = string.IsNullOrEmpty(plan.LastViolationType)
                ? "-"
                : plan.LastViolationType;
            string lastSeverity = FormatViolationSeverityLabel(plan.LastViolationSeverity);
            string todayTypes = plan.TodayViolationTypes.Count > 0
                ? string.Join(",", plan.TodayViolationTypes)
                : "-";
            string todayReasons = plan.TodayViolationReasons.Count > 0
                ? string.Join("; ", RecoveryPlanViolationReasonTexts.FormatReasons(plan.TodayViolationReasons))
                : "-";
            string completionTopic = ResolveCompletionResult(plan);
            HarveyCareTone harveyTone = plan.IsActive
                ? HarveyCareToneCalculator.Calculate(
                    plan,
                    state.CareTrust,
                    state.RecoveryPlanNeedsHarveyVisit,
                    BuildHarveyToneWorldContext())
                : HarveyCareTone.Calm;

            return
                $"RecoveryPlan: {active}  Status={plan.Status}  "
                + $"Day {plan.CurrentDay}/{plan.TotalDays}  "
                + $"Tasks={plan.Tasks.Count}  Violations={plan.TodayViolations.Count}  "
                + $"Concern={plan.ConcernScore}\n"
                + $"TodayViolations={plan.TodayViolations.Count}  "
                + $"NeedsHarveyVisit={YesNo(plan.NeedsHarveyVisit || state.RecoveryPlanNeedsHarveyVisit)}  "
                + $"HarveyTone={harveyTone}\n"
                + $"LastViolation: {lastType}/{lastSeverity}  "
                + $"TodayTypes=[{todayTypes}]  TodayReasons=[{todayReasons}]\n"
                + $"WarningToday={YesNo(plan.HadWarningsToday)}  "
                + $"PerfectDays={plan.PerfectDays}  WarningDays={plan.WarningDays}  "
                + $"CompletionTopic={RecoveryPlanViolationTopicMap.GetCompletionTopic(completionTopic)}\n"
                + $"Recovery violation: {ValueOrDash(state.LastRecoveryViolationType)}/{FormatViolationSeverityLabel(state.LastRecoveryViolationSeverity)}  "
                + $"DayFailed={YesNo(state.RecoveryPlanDayFailed)}  "
                + $"NeedsHarveyVisit={YesNo(state.RecoveryPlanNeedsHarveyVisit)}  "
                + $"ExtendedToday={YesNo(state.RecoveryPlanExtendedToday)}\n"
                + $"Extensions={plan.ExtensionCount}/{_config.MaxRecoveryPlanExtensions}  "
                + $"MaxReached={YesNo(plan.MaxExtensionsReached)}  "
                + $"StrictFollowUp={YesNo(plan.NeedsStrictFollowUp)}  "
                + $"FollowUpDay={plan.RequiredFollowUpDay}";
        }

        public IEnumerable<string> GetRecoveryViolationStatusLines()
        {
            var state = _stateManager.State;
            yield return $"TotalViolations={state.RecoveryPlanTotalViolations}";
            yield return $"MildViolations={state.RecoveryPlanMildViolations}";
            yield return $"MediumViolations={state.RecoveryPlanMediumViolations}";
            yield return $"SevereViolations={state.RecoveryPlanSevereViolations}";
            yield return $"LastViolationType={ValueOrDash(state.LastRecoveryViolationType)}";
            yield return $"LastViolationSeverity={state.LastRecoveryViolationSeverity} ({FormatViolationSeverityLabel(state.LastRecoveryViolationSeverity)})";
            yield return $"LastViolationDay={state.LastRecoveryViolationDay}";
            yield return $"LastViolationTime={state.LastRecoveryViolationTime}";
            yield return $"RecoveryPlanDayFailed={state.RecoveryPlanDayFailed}";
            yield return $"RecoveryPlanNeedsHarveyVisit={state.RecoveryPlanNeedsHarveyVisit}";
            yield return $"RecoveryPlanExtendedToday={state.RecoveryPlanExtendedToday}";
        }

        private static string FormatViolationSeverityLabel(int severity) => severity switch
        {
            RecoveryViolationSeverity.Mild => "mild",
            RecoveryViolationSeverity.Medium => "medium",
            RecoveryViolationSeverity.Severe => "severe",
            _ => "none",
        };

        private static string ValueOrDash(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "-" : value;

        private static string YesNo(bool value) => value ? "true" : "false";

        public IEnumerable<string> GetStatusLines()
        {
            var plan = _stateManager.GetRecoveryPlan();
            var hospital = GetActivePlan();
            int today = GameUtils.Today();

            if (!plan.IsActive && hospital is not { IsActive: true })
            {
                yield return "(none)";
                yield break;
            }

            yield return $"daily_active={plan.IsActive}  hospital_active={hospital?.IsActive ?? false}";
            yield return $"day={plan.CurrentDay}/{plan.TotalDays}";
            yield return
                $"today_failed={YesNo(plan.TodayFailed || _stateManager.State.RecoveryPlanDayFailed)}  "
                + $"today_completed={YesNo(plan.TodayCompleted)}";

            EnsureTodayViolationReasons(plan);
            string reasons = plan.TodayViolationReasons.Count > 0
                ? string.Join(", ", plan.TodayViolationReasons)
                : "(none)";
            yield return $"TodayViolationReasons=[{reasons}]";

            string types = plan.TodayViolationTypes.Count > 0
                ? string.Join(", ", plan.TodayViolationTypes)
                : "(none)";
            yield return $"TodayViolationTypes=[{types}]";
            yield return
                $"LastViolationType={ValueOrDash(plan.LastViolationType)}  "
                + $"LastViolationSeverity={plan.LastViolationSeverity} ({FormatViolationSeverityLabel(plan.LastViolationSeverity)})";
            yield return
                $"HadWarningsToday={YesNo(plan.HadWarningsToday)}  "
                + $"PerfectDays={plan.PerfectDays}  WarningDays={plan.WarningDays}";
            yield return
                $"CompletionTopic={RecoveryPlanViolationTopicMap.GetCompletionTopic(ResolveCompletionResult(plan))}";
            yield return
                $"warning_flags: stamina_day={plan.LastStaminaWarningDay} health_day={plan.LastHealthWarningDay} "
                + $"morning_hud_day={plan.LastMorningHudDay} violation_hud_day={plan.LastViolationHudDay} "
                + $"(today={today})";

            yield return GetDebugHudBlock();
            yield return
                $"injury={plan.ActiveInjuryId ?? "(none)"}  phase={plan.CurrentPhase}/{plan.TotalPhases}  "
                + $"needsHarvey={plan.NeedsHarveyVisit}";

            foreach (RecoveryPlanTask task in plan.Tasks)
                yield return $"  task {task.Id}: {task.Title} failed={task.IsFailed}";

            foreach (RecoveryPlanViolation v in plan.TodayViolations)
                yield return $"  violation {v.Id} @ {v.TimeOfDay} {v.LocationName}";
        }

        // ============================================================================
        // Post-hospital plan (legacy, отдельный save-state)
        // ============================================================================

        public bool StartHospitalDischargePlan(string? injuryId = null)
        {
            if (HasActivePlan())
            {
                _monitor.Log("[RecoveryPlan] Пропуск старта hospital: уже активен", LogLevel.Debug);
                return false;
            }

            string? resolved = ResolveInjuryIdForHospital(injuryId);
            int today = GameUtils.Today();

            var plan = new HospitalDischargePlanState
            {
                IsActive = true,
                PlanId = HospitalDischargePlanId,
                Reason = HospitalDischargeReason,
                InjuryId = resolved,
                StartDay = today,
                RequiredDays = HospitalDischargeRequiredDays,
                CompletedDays = 0,
                TodayViolationReasons = new List<string>(),
                LastEvaluatedDay = -1,
            };

            _stateManager.SetActiveRecoveryPlan(plan);
            _dialogueManager.AddTopic(ConversationTopics.RecoveryPlanStarted, plan.RequiredDays);
            Game1.addHUDMessage(new HUDMessage(HudHospitalStart, HUDMessage.health_type));
            RefreshPlanForToday(notifyCreated: true);

            var daily = _stateManager.GetRecoveryPlan();
            daily.CompletionRewardApplied = false;
            daily.WasPerfectOnCompletion = false;
            daily.LastCompletionDialogueDay = -1;
            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] Plan started: injury={resolved ?? "(none)"}, "
                + $"requiredDays={plan.RequiredDays}, startDay={today}",
                LogLevel.Info);
            return true;
        }

        public bool HasActivePlan() => GetActivePlan() is { IsActive: true };

        public HospitalDischargePlanState? GetActivePlan() => _stateManager.GetActiveRecoveryPlan();

        public bool RegisterHospitalViolation(string reason, bool serious = true)
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.IsActive)
                return false;

            plan.TodayViolationReasons ??= new List<string>();

            string normalized = string.IsNullOrWhiteSpace(reason)
                ? RecoveryPlanReasonIds.Unknown
                : RecoveryPlanViolationReasonTexts.CanonicalizeReasonId(reason.Trim());
            if (plan.TodayViolationReasons.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                return false;

            bool first = !plan.TodayFailed;
            plan.TodayFailed = true;
            plan.ViolationsToday++;
            plan.TotalViolations++;
            plan.TodayViolationReasons.Add(normalized);
            var daily = _stateManager.GetRecoveryPlan();
            if (daily.IsActive)
                SyncPlanTotalViolations(daily);
            _stateManager.Save();

            if (first)
            {
                string violationType = RecoveryPlanViolationTopicMap.ResolveViolationType(normalized);
                if (string.IsNullOrEmpty(violationType))
                    violationType = RecoveryPlanViolationType.LowStamina;

                SyncViolationDialogueTopic(daily, violationType, RecoveryViolationSeverity.Medium, warningOnly: false);
                _stateManager.Save();
                Game1.addHUDMessage(new HUDMessage(HudHospitalDayFailed, HUDMessage.error_type));
            }

            return true;
        }

        public void CheckViolationOnLocationEntry(GameLocation? location)
        {
            if (!HasActivePlan() || location == null)
                return;

            if (TryResolveMineViolationReason(location, out string? reason))
                RegisterHospitalViolation(reason);
        }

        public void CheckViolationOnLowHealth()
        {
            if (!HasActivePlan() || Game1.player.health > 10)
                return;

            RegisterHospitalViolation("low_health");
        }

        public void CheckViolationOnLowStamina()
        {
            if (!HasActivePlan())
                return;

            float max = Math.Max(1f, Game1.player.MaxStamina);
            if (Game1.player.Stamina > max * CriticalStaminaFraction)
                return;

            RegisterHospitalViolation("low_stamina");
        }

        public void CheckViolationOnPassOut()
        {
            if (!_stateManager.State.WasPassedOut)
                return;

            if (HasActiveRecoveryContext())
            {
                RegisterSeverityViolation(
                    RecoveryViolationTypes.PassOut,
                    RecoveryViolationSeverity.Severe,
                    failDay: true,
                    needsHarveyVisit: true,
                    HudSevereViolation);
            }

            if (IsDailyPlanActive())
            {
                HandleRecoveryPlanViolation(
                    RecoveryPlanViolationType.PassedOut,
                    RecoveryViolationSeverity.Severe);
                RefreshPlanForToday();
            }

            if (!HasActivePlan())
                return;

            RegisterHospitalViolation("passout");
        }

        public void OnHospitalPlanDayStarted()
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.IsActive)
                return;

            plan.TodayFailed = false;
            plan.TodayCompleted = false;
            plan.ViolationsToday = 0;
            plan.TodayViolationReasons.Clear();
            _stateManager.Save();
        }

        /// <summary>Оценка прогресса плана в конце дня (hospital + daily).</summary>
        public void OnRecoveryPlanDayEnding()
        {
            if (!HasActiveRecoveryContext())
                return;

            int today = GameUtils.Today();
            var hospital = GetActivePlan();
            var daily = _stateManager.GetRecoveryPlan();

            bool hospitalNeedsEval = hospital is { IsActive: true } && hospital.LastEvaluatedDay != today;
            bool dailyNeedsEval = daily.IsActive && daily.LastEvaluatedDay != today;

            if (!hospitalNeedsEval && !dailyNeedsEval)
                return;

            int severity = GetTodayMaxViolationSeverity();
            bool creditDay = ShouldCreditRecoveryDay(severity);
            bool dayFailed = !creditDay
                || _stateManager.State.RecoveryPlanDayFailed
                || (hospital?.TodayFailed ?? false);

            _monitor.Log(
                $"[RecoveryPlan] DayEnding evaluation: severity={severity}, creditDay={creditDay}, failed={dayFailed}",
                LogLevel.Info);

            if (dayFailed)
            {
                MarkRecoveryPlanDayFailed();
                if (dailyNeedsEval && hospital is not { IsActive: true })
                    TryShowHudOnce("daily_day_failed", HudDailyDayFailed);
            }
            else
            {
                MarkRecoveryPlanDayCompleted();
            }

            if (hospitalNeedsEval && hospital != null)
            {
                hospital.LastEvaluatedDay = today;

                if (hospital.CompletedDays >= hospital.RequiredDays)
                {
                    hospital.IsActive = false;
                    hospital.CompletionTalkPending = true;
                    hospital.RequiresHarveyTalk = true;
                    Game1.addHUDMessage(new HUDMessage(HudHospitalCompleted, HUDMessage.achievement_type));
                    SyncPlanTotalViolations(daily);
                    CompleteRecoveryPlan();
                }
            }

            if (dailyNeedsEval)
                daily.LastEvaluatedDay = today;

            _stateManager.Save();
        }

        /// <summary>Устаревший вызов — делегирует в <see cref="OnRecoveryPlanDayEnding"/>.</summary>
        public void OnHospitalPlanDayEnding() => OnRecoveryPlanDayEnding();

        public void NotifyHarveyRecoveryViolationTalkAcknowledged()
        {
            if (!_stateManager.State.RecoveryPlanNeedsHarveyVisit)
                return;

            _stateManager.State.RecoveryPlanNeedsHarveyVisit = false;
            _stateManager.GetRecoveryPlan().NeedsHarveyVisit = false;
            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] Harvey acknowledged recovery violation talk", LogLevel.Debug);
        }

        // ============================================================================
        // Прогресс плана по тяжести нарушений
        // ============================================================================

        public static string GetRecoveryViolationTopic(int severity) =>
            RecoveryViolationTopics.GetRecoveryViolationTopic(severity);

        /// <summary>Продлить план на 1 день (hospital RequiredDays и/или daily PlanExtensionDays).</summary>
        public bool ExtendRecoveryPlanByOneDay()
        {
            bool extended = false;

            var hospital = GetActivePlan();
            if (hospital is { IsActive: true })
            {
                hospital.RequiredDays++;
                extended = true;
                _monitor.Log(
                    $"[RecoveryPlan] Hospital plan extended: RequiredDays={hospital.RequiredDays}",
                    LogLevel.Info);
            }

            var daily = _stateManager.GetRecoveryPlan();
            if (daily.IsActive)
            {
                daily.PlanExtensionDays++;
                extended = true;
                _monitor.Log(
                    $"[RecoveryPlan] Daily plan extended: PlanExtensionDays={daily.PlanExtensionDays}",
                    LogLevel.Info);
            }

            if (extended)
                _stateManager.Save();

            return extended;
        }

        public void MarkRecoveryPlanDayFailed()
        {
            var state = _stateManager.State;
            state.RecoveryPlanDayFailed = true;

            var hospital = GetActivePlan();
            if (hospital is { IsActive: true })
                hospital.TodayFailed = true;

            var daily = _stateManager.GetRecoveryPlan();
            if (daily.IsActive)
                daily.TodayFailed = true;

            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] Day marked as failed", LogLevel.Debug);
        }

        public void MarkRecoveryPlanDayCompleted()
        {
            var hospital = GetActivePlan();
            if (hospital is { IsActive: true })
            {
                hospital.TodayCompleted = true;
                hospital.CompletedDays++;
                Game1.addHUDMessage(new HUDMessage(HudHospitalDayCredited, HUDMessage.health_type));
            }

            var daily = _stateManager.GetRecoveryPlan();
            if (daily.IsActive)
            {
                daily.TodayCompleted = true;
                daily.CreditedDays++;

                if (daily.HadWarningsToday || daily.TodayFailed)
                    daily.ConsecutivePerfectDays = 0;
                else
                    daily.ConsecutivePerfectDays++;

                if (daily.HadWarningsToday)
                    daily.WarningDays++;
                else if (daily.TodayViolationTypes.Count == 0)
                    daily.PerfectDays++;

                TryShowHudOnce("daily_day_credited", HudDailyDayCredited);
                _monitor.Log(
                    $"[RecoveryPlan] Day credited: {daily.CreditedDays}/{daily.TotalDays}, "
                    + $"perfectDays={daily.PerfectDays}, consecutive={daily.ConsecutivePerfectDays}",
                    LogLevel.Info);
            }

            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] Day marked as completed", LogLevel.Debug);
        }

        private static bool ShouldCreditRecoveryDay(int severity) =>
            severity <= RecoveryViolationSeverity.None;

        private int GetTodayMaxViolationSeverity()
        {
            var severities = _stateManager.State.RecoveryPlanTodayViolationSeverities;
            if (severities == null || severities.Count == 0)
                return RecoveryViolationSeverity.None;

            return severities.Values.Max();
        }

        private void ApplyViolationSeverityConsequences(int severity)
        {
            if (!IsDailyPlanActive())
            {
                string topic = GetRecoveryViolationTopic(severity);
                if (!string.IsNullOrEmpty(topic))
                {
                    _dialogueManager.AddTopic(topic, RecoveryViolationTopics.GetTopicDays(severity));
                    _monitor.Log($"[RecoveryPlan] Topic added: {topic} (severity={severity})", LogLevel.Debug);
                }
            }

            switch (severity)
            {
                case RecoveryViolationSeverity.Mild:
                    break;
                case RecoveryViolationSeverity.Medium:
                    MarkRecoveryPlanDayFailed();
                    _stateManager.State.RecoveryPlanNeedsHarveyVisit = true;
                    _stateManager.Save();
                    break;
                case RecoveryViolationSeverity.Severe:
                    MarkRecoveryPlanDayFailed();
                    _stateManager.State.RecoveryPlanNeedsHarveyVisit = true;
                    if (!IsDailyPlanActive()
                        && !_stateManager.State.RecoveryPlanExtendedToday)
                    {
                        ExtendRecoveryPlanByOneDay();
                        _stateManager.State.RecoveryPlanExtendedToday = true;
                        _stateManager.Save();
                    }
                    break;
            }
        }

        private void TryShowMildViolationMorningReminder()
        {
            var state = _stateManager.State;
            int today = GameUtils.Today();
            if (state.LastRecoveryViolationDay != today - 1)
                return;

            if (state.LastRecoveryViolationSeverity != RecoveryViolationSeverity.Mild)
                return;

            if (!HasActiveRecoveryContext())
                return;

            TryShowHudOnce("mild_morning_reminder", HudMildMorningReminder);
        }

        public bool IsCompletionTalkPending() => GetActivePlan()?.CompletionTalkPending == true;

        public void NotifyHarveyClickedForCompletionTalk()
        {
            if (!IsCompletionTalkPending())
                return;

            _awaitingCompletionTalkAck = true;
            _completionTalkSawDialogue = Game1.activeClickableMenu is StardewValley.Menus.DialogueBox;
            _completionTalkStartedTick = Game1.ticks;
        }

        public void OnCompletionTalkDialogueUpdate()
        {
            if (!_awaitingCompletionTalkAck || !IsCompletionTalkPending())
            {
                ResetCompletionTalkTracking();
                return;
            }

            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                ResetCompletionTalkTracking();
                return;
            }

            int elapsed = Game1.ticks - _completionTalkStartedTick;
            if (elapsed > CompletionTalkDialogueTimeoutTicks)
            {
                ResetCompletionTalkTracking();
                return;
            }

            if (Game1.activeClickableMenu is StardewValley.Menus.DialogueBox)
            {
                _completionTalkSawDialogue = true;
                return;
            }

            if (!_completionTalkSawDialogue && elapsed < 60)
                return;

            if (Game1.activeClickableMenu is StardewValley.Menus.DialogueBox)
                return;

            AcknowledgeCompletionTalk();
            ResetCompletionTalkTracking();
        }

        public void AcknowledgeCompletionTalk()
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.CompletionTalkPending)
                return;

            RemoveCompletionTopics();
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanStarted);
            RemoveTypedViolationTopics();
            ClearRecoveryPlan();
        }

        public void ClearRecoveryPlan()
        {
            if (GetActivePlan() == null)
                return;

            ResetCompletionTalkTracking();
            _stateManager.ClearActiveRecoveryPlan();
        }

        // ============================================================================
        // Helpers
        // ============================================================================

        private static bool previousDayReset(RecoveryPlanState plan, int today)
        {
            if (plan.LastUpdatedDay >= 0 && plan.LastUpdatedDay != today)
            {
                plan.WasShownToday = false;
                return true;
            }

            return false;
        }

        private static void ResetDailyPlanState(RecoveryPlanState plan)
        {
            plan.ActiveInjuryId = null;
            plan.PlanStartDay = -1;
            plan.CurrentDay = 0;
            plan.TotalDays = 0;
            plan.CurrentPhase = 0;
            plan.TotalPhases = 0;
            plan.IsActive = false;
            plan.NeedsHarveyVisit = false;
            plan.Status = RecoveryPlanMoodStatus.None;
            plan.Tasks.Clear();
            plan.TodayViolations.Clear();
            plan.TodayViolationReasons.Clear();
            plan.ConcernScore = 0;
            plan.LastUpdatedDay = -1;
            plan.WasShownToday = false;
            plan.CreditedDays = 0;
            plan.PlanExtensionDays = 0;
            plan.ExtensionCount = 0;
            plan.MaxExtensionsReached = false;
            plan.NeedsStrictFollowUp = false;
            plan.RequiredFollowUpDay = -1;
            plan.LastExtensionDay = -1;
            plan.TodayFailed = false;
            plan.TodayCompleted = false;
            plan.LastEvaluatedDay = -1;
            plan.LastStaminaWarningDay = -1;
            plan.LastHealthWarningDay = -1;
            plan.LastMorningHudDay = -1;
            plan.LastViolationHudDay = -1;
            plan.LastViolationType = "";
            plan.LastViolationSeverity = 0;
            plan.TodayViolationTypes.Clear();
            plan.HadWarningsToday = false;
            plan.PerfectDays = 0;
            plan.WarningDays = 0;
            plan.HadPlanWarnings = false;
            plan.HadPlanViolations = false;
            plan.TodayViolationDialogueType = "";
            plan.TodayViolationDialogueSeverity = 0;
            plan.TotalViolations = 0;
            plan.ConsecutivePerfectDays = 0;
            plan.WasPerfectOnCompletion = false;
            plan.CompletionRewardApplied = false;
            plan.LastCompletionDialogueDay = -1;
            plan.SoftToneUntilDay = -1;
        }

        private bool HasActiveRecoveryPlanForHud()
        {
            var hospital = GetActivePlan();
            if (hospital is { IsActive: true })
                return true;

            return IsDailyPlanActive();
        }

        private bool TryGetMorningHudDayProgress(out int currentDay, out int totalDays)
        {
            var hospital = GetActivePlan();
            if (hospital is { IsActive: true })
            {
                currentDay = Math.Min(hospital.RequiredDays, hospital.CompletedDays + 1);
                totalDays = Math.Max(1, hospital.RequiredDays);
                return true;
            }

            var daily = _stateManager.GetRecoveryPlan();
            if (!daily.IsActive)
            {
                currentDay = 0;
                totalDays = 0;
                return false;
            }

            currentDay = Math.Max(1, daily.CurrentDay);
            totalDays = Math.Max(1, daily.TotalDays);
            return true;
        }

        private static bool CanShowRecoveryPlanHudNow()
        {
            if (!Context.IsWorldReady)
                return false;

            if (Game1.eventUp || Game1.CurrentEvent != null)
                return false;

            if (Game1.isFestival())
                return false;

            if (!Context.IsPlayerFree)
                return false;

            return true;
        }

        private string FormatRecoveryPlanKey()
        {
            string key = _config.OpenRecoveryPlanKey?.Trim() ?? "";
            if (string.IsNullOrEmpty(key))
                return "H";

            return key.ToUpperInvariant();
        }

        private void TryShowViolationRecordedHud()
        {
            if (!HasActiveRecoveryPlanForHud())
                return;

            var plan = _stateManager.GetRecoveryPlan();
            int today = GameUtils.Today();
            if (plan.LastViolationHudDay == today)
                return;

            plan.LastViolationHudDay = today;
            _stateManager.Save();

            Game1.addHUDMessage(new HUDMessage(HudViolationRecorded, HUDMessage.error_type));
            _monitor.Log("[RecoveryPlan] Violation recorded HUD shown", LogLevel.Info);
        }

        private string? ResolveMainInjuryId()
        {
            string? main = _stateManager.GetMainInjuryId();
            if (!string.IsNullOrEmpty(main) && _stateManager.State.ActiveDebuffs.ContainsKey(main))
                return main;

            return InjurySets.SelectMainInjuryByPriority(_stateManager.State.ActiveDebuffs.Keys);
        }

        private bool HasRecoveryContext()
        {
            var state = _stateManager.State;
            if (state.IsHospitalized)
                return true;

            if (state.ActiveComplications.Count > 0)
                return true;

            return ResolveMainInjuryId() != null;
        }

        private bool RegisterSeverityViolation(
            string type,
            int severity,
            bool failDay,
            bool needsHarveyVisit,
            string hudMessage)
        {
            if (!_stateManager.TryRegisterRecoveryViolation(type, severity, failDay, needsHarveyVisit))
                return false;

            ApplyViolationSeverityConsequences(severity);
            TryShowViolationHudOnce(type, hudMessage, severity);
            return true;
        }

        private bool HasActiveTreatmentOrPhaseBuff()
        {
            foreach (string buffId in _buffManager.GetActiveModBuffs())
            {
                if (IsTreatmentOrPhaseBuff(buffId))
                    return true;
            }

            return false;
        }

        private static bool IsTreatmentOrPhaseBuff(string buffId)
        {
            if (InjurySets.SeverePhaseBuffIds.Contains(buffId))
                return true;

            if (string.Equals(buffId, InjuryBuffs.ColdAcute, StringComparison.OrdinalIgnoreCase)
                || string.Equals(buffId, InjuryBuffs.ColdRecovery, StringComparison.OrdinalIgnoreCase))
                return true;

            return buffId switch
            {
                CureBuffs.Treatment or CureBuffs.IntensiveCare or CureBuffs.BadlyHurtOutpatientCare
                    or CureBuffs.Protection or CureBuffs.Recovery or CureBuffs.Teracitin
                    or CureBuffs.Antibiotics or CureBuffs.ForcedSedation or CureBuffs.PostSurgical
                    or CureBuffs.Care or CureBuffs.Rehab => true,
                _ => false,
            };
        }

        private bool HasBandageExposure()
        {
            string? injuryId = ResolveMainInjuryId();
            if (string.IsNullOrEmpty(injuryId))
                return false;

            return NeedsBandageDryRule(injuryId, _stateManager.State)
                || _stateManager.State.ActivePrescriptions.ContainsKey(PrescriptionIds.KeepDry);
        }

        private static bool IsFarmArea(GameLocation? location)
        {
            if (location == null)
                return false;

            string name = location.NameOrUniqueName ?? location.Name ?? "";
            return location is Farm
                || LocationEventLauncher.IsFarmHouseLocation(location)
                || string.Equals(name, "Farm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Greenhouse", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "FarmCave", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHeavyToolInUse()
        {
            if (Game1.player?.UsingTool != true)
                return false;

            return Game1.player.CurrentTool?.GetType().Name switch
            {
                "Axe" or "Pickaxe" or "Hoe" or "WateringCan" => true,
                _ => false,
            };
        }

        private void TryShowViolationHudOnce(string type, string message, int severity)
        {
            int today = GameUtils.Today();
            if (_lastHudDay != today)
            {
                _hudShownKeys.Clear();
                _lastHudDay = today;
            }

            if (string.IsNullOrWhiteSpace(message))
                return;

            string key = $"severity_violation_{type}";
            if (!_hudShownKeys.Add(key))
                return;

            int hudType = severity >= RecoveryViolationSeverity.Severe
                ? HUDMessage.error_type
                : severity >= RecoveryViolationSeverity.Medium
                    ? HUDMessage.error_type
                    : HUDMessage.health_type;

            Game1.addHUDMessage(new HUDMessage(message, hudType));
        }

        private static RecoveryPlanTask MakeTask(
            string id,
            string title,
            string description,
            bool required,
            RecoveryPlanTaskSeverity severity)
        {
            return new RecoveryPlanTask
            {
                Id = id,
                Title = title,
                Description = description,
                IsRequired = required,
                Severity = severity,
            };
        }

        private static void ApplyViolationFlagsToTasks(
            List<RecoveryPlanTask> tasks,
            List<RecoveryPlanViolation> violations)
        {
            var failedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (RecoveryPlanViolation v in violations)
            {
                failedIds.Add(v.Id);
                failedIds.Add(v.Reason);
            }

            foreach (RecoveryPlanTask task in tasks)
            {
                task.IsFailed = task.Id switch
                {
                    RecoveryPlanTaskIds.AvoidMines =>
                        failedIds.Contains(RecoveryPlanViolationIds.EnteredMinesDuringRecovery),
                    RecoveryPlanTaskIds.KeepStaminaAbove15 =>
                        failedIds.Contains(RecoveryPlanViolationIds.LowStaminaDuringRecovery),
                    RecoveryPlanTaskIds.ReturnIfLowHealth =>
                        failedIds.Contains(RecoveryPlanViolationIds.LowHealthDuringRecovery),
                    RecoveryPlanTaskIds.SleepBeforeMidnight =>
                        failedIds.Contains(RecoveryPlanViolationIds.LateSleepDuringRecovery),
                    RecoveryPlanTaskIds.KeepBandageDry =>
                        failedIds.Contains(RecoveryPlanViolationIds.RainDuringRecovery),
                    RecoveryPlanTaskIds.VisitHarveyIfReady =>
                        failedIds.Contains(RecoveryPlanViolationIds.MissedCheckup),
                    _ => task.IsFailed,
                };
            }
        }

        public static bool ShouldAvoidMinesForPlan(string injuryId, InjuryState state)
        {
            if (InjurySets.Severe.Contains(injuryId) || InjurySets.Critical.Contains(injuryId))
                return true;

            if (state.ActivePrescriptions.ContainsKey(PrescriptionIds.NoMine))
                return true;

            return InjurySets.SeverePhaseBuffIds.Any(state.ActiveDebuffs.ContainsKey);
        }

        private static bool ShouldAvoidMines(string injuryId, InjuryState state) =>
            ShouldAvoidMinesForPlan(injuryId, state)
            || state.MineForbiddenAppliedDay >= 0
            || state.ActiveDebuffs.ContainsKey(InjuryBuffs.MineForbidden);

        private static bool NeedsSleepRule(string injuryId, InjuryState state)
        {
            if (InjurySets.Severe.Contains(injuryId) || InjurySets.Critical.Contains(injuryId))
                return true;

            return state.ActivePrescriptions.ContainsKey(PrescriptionIds.Rest);
        }

        private static bool NeedsBandageDryRule(string injuryId, InjuryState state)
        {
            return InjurySets.BandageSensitive.Contains(injuryId)
                || InjurySets.WetBandageSensitive.Contains(injuryId)
                || state.ActivePrescriptions.ContainsKey(PrescriptionIds.KeepDry);
        }

        private static string BuildDayProgressLabel(RecoveryPlanState plan, DebuffState? debuff)
        {
            if (debuff != null && debuff.TotalPhases > 0 && debuff.CurrentPhase > 0)
            {
                return
                    $"Фаза {debuff.CurrentPhase}/{debuff.TotalPhases}, день {plan.CurrentDay} из {plan.TotalDays}";
            }

            return $"Сегодня: день {plan.CurrentDay} из {plan.TotalDays}";
        }

        private string BuildComplicationSummaryFromState()
        {
            if (_stateManager.State.ActiveComplications.Count == 0)
                return "";

            return string.Join(
                " · ",
                _stateManager.State.ActiveComplications.Keys
                    .Select(RecoveryPlanTexts.GetComplicationLine));
        }

        private static string FormatStatusText(RecoveryPlanMoodStatus status) => status switch
        {
            RecoveryPlanMoodStatus.Calm => RecoveryPlanTexts.Status.Calm,
            RecoveryPlanMoodStatus.HarveyConcerned => RecoveryPlanTexts.Status.HarveyConcerned,
            RecoveryPlanMoodStatus.NeedsHarveyTalk => RecoveryPlanTexts.Status.NeedsHarveyTalk,
            RecoveryPlanMoodStatus.Urgent => RecoveryPlanTexts.Status.Urgent,
            _ => "—",
        };

        private static string FormatStatusDescription(RecoveryPlanMoodStatus status) => status switch
        {
            RecoveryPlanMoodStatus.Calm => RecoveryPlanTexts.Status.CalmLong,
            RecoveryPlanMoodStatus.HarveyConcerned => RecoveryPlanTexts.Status.HarveyConcernedLong,
            RecoveryPlanMoodStatus.NeedsHarveyTalk => RecoveryPlanTexts.Status.NeedsHarveyTalkLong,
            RecoveryPlanMoodStatus.Urgent => RecoveryPlanTexts.Status.UrgentLong,
            _ => RecoveryPlanTexts.Hud.NoActivePlan,
        };

        private static string FormatRegimeStatusShort(RecoveryPlanMoodStatus status) => status switch
        {
            RecoveryPlanMoodStatus.Calm => RecoveryPlanTexts.RegimeStatus.Calm,
            RecoveryPlanMoodStatus.HarveyConcerned => RecoveryPlanTexts.RegimeStatus.Concerned,
            RecoveryPlanMoodStatus.NeedsHarveyTalk => RecoveryPlanTexts.RegimeStatus.NeedsHarveyTalk,
            RecoveryPlanMoodStatus.Urgent => RecoveryPlanTexts.RegimeStatus.Urgent,
            _ => "",
        };

        private void TryShowHudOnce(string key, string message)
        {
            int today = GameUtils.Today();
            if (_lastHudDay != today)
            {
                _hudShownKeys.Clear();
                _lastHudDay = today;
            }

            if (!_hudShownKeys.Add(key))
                return;

            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.health_type));
        }

        private void ResetCompletionTalkTracking()
        {
            _awaitingCompletionTalkAck = false;
            _completionTalkSawDialogue = false;
            _completionTalkStartedTick = -1;
        }

        private static bool IsMineOrVolcanoLocation(GameLocation? location)
        {
            if (location == null)
                return false;

            string name = location.NameOrUniqueName ?? location.Name ?? "";
            return location is MineShaft
                || location is VolcanoDungeon
                || string.Equals(name, "Mine", StringComparison.OrdinalIgnoreCase)
                || name.Contains("SkullCave", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveMineReasonId(GameLocation location)
        {
            if (location is VolcanoDungeon)
                return RecoveryPlanReasonIds.EnteredVolcano;

            return RecoveryPlanReasonIds.EnteredMine;
        }

        private static bool TryResolveMineViolationReason(GameLocation location, out string reason)
        {
            if (!IsMineOrVolcanoLocation(location))
            {
                reason = "";
                return false;
            }

            reason = ResolveMineReasonId(location);
            return true;
        }

        private static void EnsureTodayViolationReasons(RecoveryPlanState plan)
        {
            plan.TodayViolationReasons ??= new List<string>();
        }

        private static bool TryMapReasonToViolationId(
            string canonicalReason,
            int severity,
            out string violationId,
            out RecoveryPlanTaskSeverity taskSeverity)
        {
            violationId = "";
            taskSeverity = RecoveryPlanTaskSeverity.Warning;

            switch (canonicalReason)
            {
                case RecoveryPlanReasonIds.EnteredMine:
                case RecoveryPlanReasonIds.EnteredVolcano:
                    violationId = RecoveryPlanViolationIds.EnteredMinesDuringRecovery;
                    taskSeverity = severity >= RecoveryViolationSeverity.Severe
                        ? RecoveryPlanTaskSeverity.Danger
                        : RecoveryPlanTaskSeverity.Warning;
                    return true;

                case RecoveryPlanReasonIds.StaminaTooLow:
                case RecoveryPlanReasonIds.HeavyWork:
                    violationId = RecoveryPlanViolationIds.LowStaminaDuringRecovery;
                    return true;

                case RecoveryPlanReasonIds.HealthTooLow:
                    violationId = RecoveryPlanViolationIds.LowHealthDuringRecovery;
                    return true;

                case RecoveryPlanReasonIds.TooLate:
                    violationId = RecoveryPlanViolationIds.LateSleepDuringRecovery;
                    return true;

                case RecoveryPlanReasonIds.RainBandage:
                    violationId = RecoveryPlanViolationIds.RainDuringRecovery;
                    return true;

                case RecoveryPlanReasonIds.MissedHarveyCheckup:
                    violationId = RecoveryPlanViolationIds.MissedCheckup;
                    return true;

                default:
                    return false;
            }
        }

        private string? ResolveInjuryIdForHospital(string? injuryId)
        {
            if (!string.IsNullOrWhiteSpace(injuryId))
                return injuryId.Trim();

            return _stateManager.GetMainInjuryId()
                ?? NullIfEmpty(_stateManager.State.LastHospitalDischargeInjuryId);
        }

        private static string? NullIfEmpty(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private void ApplyViolationConversationTopics(
            RecoveryPlanState plan,
            string violationType,
            int severity,
            bool extended,
            bool maxExtensionsHit)
        {
            if (severity < RecoveryViolationSeverity.Mild)
                return;

            if (!RecoveryPlanViolationTopicMap.ShouldReplaceDialogue(
                    plan.TodayViolationDialogueType,
                    plan.TodayViolationDialogueSeverity,
                    violationType,
                    severity))
            {
                return;
            }

            RemoveViolationConversationTopics();

            if (maxExtensionsHit)
            {
                _dialogueManager.AddTopic(
                    ConversationTopics.RecoveryPlanStrictFollowUpRequired,
                    RecoveryPlanViolationTopicMap.ViolatedTopicDays);
                _dialogueManager.AddTopic(
                    ConversationTopics.RecoveryPlanMaxExtensionsReached,
                    RecoveryPlanViolationTopicMap.ViolatedTopicDays);
            }
            else if (extended)
            {
                string typeTopic = RecoveryPlanViolationTopicMap.GetViolationTopic(violationType);
                _dialogueManager.AddTopic(typeTopic, RecoveryPlanViolationTopicMap.ViolatedTopicDays);
                _dialogueManager.AddTopic(
                    ConversationTopics.RecoveryPlanExtended,
                    RecoveryPlanViolationTopicMap.ViolatedTopicDays);
            }
            else
            {
                string severityTopic = RecoveryPlanViolationTopicMap.GetSeverityTopic(severity);
                if (!string.IsNullOrEmpty(severityTopic))
                {
                    _dialogueManager.AddTopic(severityTopic, RecoveryPlanViolationTopicMap.ViolatedTopicDays);
                }

                if (severity == RecoveryViolationSeverity.Severe)
                {
                    string typeTopic = RecoveryPlanViolationTopicMap.GetViolationTopic(violationType);
                    _dialogueManager.AddTopic(typeTopic, RecoveryPlanViolationTopicMap.ViolatedTopicDays);
                }
            }

            plan.TodayViolationDialogueType = violationType;
            plan.TodayViolationDialogueSeverity = severity;

            _monitor.Log(
                $"[RecoveryPlan] Violation topics: type={violationType}, severity={severity}, "
                + $"extended={extended}, maxHit={maxExtensionsHit}",
                LogLevel.Info);
        }

        private void SyncViolationDialogueTopic(
            RecoveryPlanState plan,
            string violationType,
            int severity,
            bool warningOnly)
        {
            if (warningOnly || severity < RecoveryViolationSeverity.Mild)
                return;

            ApplyViolationConversationTopics(plan, violationType, severity, extended: false, maxExtensionsHit: false);
        }

        private void RemoveViolationConversationTopics()
        {
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolated);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedMine);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedLowStamina);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedLowHealth);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedLateNight);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedRain);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedMild);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedMedium);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolatedSevere);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanExtended);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanMaxExtensionsReached);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanStrictFollowUpRequired);
        }

        private void RemoveTypedViolationTopics() => RemoveViolationConversationTopics();

        private void RemoveCompletionTopics()
        {
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanCompleted);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanPerfect);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanCompletedPerfect);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanCompletedWithWarnings);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanCompletedNormal);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanSoftTone);
        }

        private void SyncPlanTotalViolations(RecoveryPlanState plan)
        {
            var state = _stateManager.State;
            int total = state.RecoveryPlanTotalViolations;
            var hospital = GetActivePlan();
            if (hospital != null)
                total = Math.Max(total, hospital.TotalViolations);

            plan.TotalViolations = total;
        }

        private void ShowCompletionDialogue(string completionResult)
        {
            if (!Context.IsWorldReady)
                return;

            int today = GameUtils.Today();
            var plan = _stateManager.GetRecoveryPlan();
            if (plan.LastCompletionDialogueDay == today)
                return;

            string text = GetCompletionFallbackDialogue(completionResult);
            NPC? harvey = HarveyHelper.FindHarveyInLocation(Game1.currentLocation);
            if (harvey != null && HarveyHelper.IsNearPlayer(harvey, HarveyCompletionDialogueDistance))
            {
                _dialogueManager.Speak(harvey, text);
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage(
                    StripDialoguePortraits(text),
                    HUDMessage.achievement_type));
            }

            plan.LastCompletionDialogueDay = today;
            _stateManager.Save();
        }

        private static string GetCompletionFallbackDialogue(string completionResult) =>
            completionResult switch
            {
                RecoveryPlanCompletionResult.Perfect => RecoveryPlanTexts.Completion.Perfect,
                RecoveryPlanCompletionResult.WithWarnings => RecoveryPlanTexts.Completion.WithWarnings,
                RecoveryPlanCompletionResult.Normal => RecoveryPlanTexts.Completion.Normal,
                _ => RecoveryPlanTexts.Completion.Normal,
            };

        private static string StripDialoguePortraits(string text) =>
            text.Replace("#$b#", " ", StringComparison.Ordinal)
                .Replace("$l", "", StringComparison.Ordinal)
                .Replace("$h", "", StringComparison.Ordinal)
                .Replace("$s", "", StringComparison.Ordinal)
                .Replace("$a", "", StringComparison.Ordinal)
                .Replace("$u", "", StringComparison.Ordinal)
                .Trim();

        private string ResolveCompletionResult(RecoveryPlanState plan)
        {
            SyncPlanTotalViolations(plan);
            var state = _stateManager.State;
            bool hasWarnings = plan.HadPlanWarnings || plan.WarningDays > 0;
            bool hasHeavy = state.RecoveryPlanMediumViolations > 0
                || state.RecoveryPlanSevereViolations > 0
                || plan.HadPlanViolations
                || plan.PlanExtensionDays > 0;

            if (plan.TotalViolations == 0 && !hasHeavy)
            {
                if (hasWarnings)
                    return RecoveryPlanCompletionResult.WithWarnings;

                return RecoveryPlanCompletionResult.Perfect;
            }

            if (!hasHeavy && state.RecoveryPlanMildViolations > 0)
                return RecoveryPlanCompletionResult.Normal;

            return RecoveryPlanCompletionResult.WithWarnings;
        }
    }
}
