using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление сохранением и загрузкой состояния мода
    /// </summary>
    public class StateManager
    {
        private const string SaveKey = "injury_state";
        private readonly IDataHelper _dataHelper;
        private readonly IMonitor _monitor;
        private InjuryState _state;

        public StateManager(IDataHelper dataHelper, IMonitor monitor)
        {
            _dataHelper = dataHelper;
            _monitor = monitor;
            _state = new InjuryState();
        }

        /// <summary>
        /// Текущее состояние
        /// </summary>
        public InjuryState State => _state;

        /// <summary>
        /// Загрузить состояние из сохранения
        /// </summary>
        public void Load()
        {
            _state = _dataHelper.ReadSaveData<InjuryState>(SaveKey) ?? new InjuryState();

            EnsureInjuryCooldownState();
            EnsurePrescriptionState();
            EnsureComplianceState();
            EnsureRehabState();
            EnsureRecoveryPlanState();
            EnsureRecoveryPlan();
            EnsureRecoveryViolationDailyState();
            EnsureSelfCareState();
            EnsureMedicalMailState();
            EnsureNeglectStrikesState();

            // Миграция старых данных ActivePhases в новую систему ActiveDebuffs
            MigrateOldPhaseData();
            
            // Миграция осложнений из ActiveComplications в ActiveDebuffs (DebuffState)
            MigrateComplicationsToDebuffState();

            MigrateMainInjuryId();
            
            _monitor.Log($"Состояние загружено: {_state.ActiveDebuffs.Count} активных дебаффов", LogLevel.Debug);
        }

        /// <summary>
        /// Добавить в ActiveDebuffs состояния для осложнений, которые есть только в ActiveComplications.
        /// </summary>
        private void MigrateComplicationsToDebuffState()
        {
            if (_state.ActiveComplications == null) return;
            foreach (var (compId, startDay) in _state.ActiveComplications)
            {
                if (_state.ActiveDebuffs.ContainsKey(compId)) continue;
                var ds = new DebuffState
                {
                    BuffId = compId,
                    InjuryStartDay = startDay,
                    TreatmentStarted = true,
                    HarveyConversationHappened = false,
                    TotalPhases = 0,
                    CurrentPhase = 1,
                    PhaseStartDay = startDay,
                    Phase1Duration = 0,
                    Phase2Duration = 0,
                    Phase3Duration = 0,
                    ReadyForNextPhase = false,
                    ReadyForRecovery = false
                };
                _state.ActiveDebuffs[compId] = ds;
                _monitor.Log($"Миграция: добавлено DebuffState для осложнения {compId}", LogLevel.Debug);
            }
            if (_state.ActiveComplications.Count > 0)
                Save();
        }

        /// <summary>QA: run MigrateMainInjuryId after injury_main_clear (clears QaSuppressMainInjuryAutoSync).</summary>
        public string DebugMigrateMainInjuryId()
        {
            QaSuppressMainInjuryAutoSync = false;
            MigrateMainInjuryId();
            Save();
            return _state.MainInjuryId ?? "(none)";
        }

        /// <summary>
        /// Миграция MainInjuryId для старых сохранений и восстановление при рассинхроне.
        /// </summary>
        private void MigrateMainInjuryId()
        {
            if (!string.IsNullOrEmpty(_state.MainInjuryId))
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(_state.MainInjuryId)
                    || !_state.ActiveDebuffs.ContainsKey(_state.MainInjuryId))
                {
                    _state.MainInjuryId = null;
                }
                else
                {
                    return;
                }
            }

            var candidates = _state.ActiveDebuffs.Keys
                .Where(id => !InjurySets.KnownComplicationBuffIds.Contains(id))
                .ToList();

            if (candidates.Count == 0)
                return;

            string? selected = InjurySets.SelectMainInjuryByPriority(candidates);
            if (selected == null)
                return;

            _state.MainInjuryId = selected;
            Save();
            _monitor.Log($"[MainInjury] Миграция: основная травма = {selected}", LogLevel.Info);
        }

        /// <summary>
        /// Миграция старых данных InjuryPhaseTracker в новую систему DebuffState
        /// </summary>
        private void MigrateOldPhaseData()
        {
            if (_state.ActivePhases != null && _state.ActivePhases.Count > 0)
            {
                _monitor.Log($"Обнаружены старые данные фаз ({_state.ActivePhases.Count}), выполняем миграцию...", LogLevel.Info);
                
                foreach (var kvp in _state.ActivePhases)
                {
                    string buffId = kvp.Key;
                    var oldTracker = kvp.Value;
                    
                    // Если уже есть в новой системе - пропускаем
                    if (_state.ActiveDebuffs.ContainsKey(buffId))
                        continue;
                    
                    // Создаем новое состояние дебаффа из старого трекера
                    var debuffState = new DebuffState
                    {
                        BuffId = buffId,
                        InjuryStartDay = oldTracker.PhaseStartDay,
                        TreatmentStarted = oldTracker.CurrentPhase > 0,
                        HarveyConversationHappened = _state.TreatmentConversations?.ContainsKey(buffId) ?? false,
                        TotalPhases = DeterminePhaseCount(oldTracker),
                        CurrentPhase = oldTracker.CurrentPhase,
                        PhaseStartDay = oldTracker.PhaseStartDay,
                        Phase1Duration = oldTracker.Phase1Duration,
                        Phase2Duration = oldTracker.Phase2Duration,
                        Phase3Duration = oldTracker.Phase3Duration,
                        ReadyForNextPhase = oldTracker.ReadyForNextPhase,
                        ReadyForRecovery = false
                    };
                    
                    _state.ActiveDebuffs[buffId] = debuffState;
                    _monitor.Log($"Мигрирован дебафф: {buffId} (фаза {debuffState.CurrentPhase}/{debuffState.TotalPhases})", LogLevel.Debug);
                }
                
                // Очищаем старые данные после миграции
                _state.ActivePhases.Clear();
                Save();
                
                _monitor.Log($"Миграция завершена, {_state.ActiveDebuffs.Count} дебаффов перенесено", LogLevel.Info);
            }
        }
        
        /// <summary>
        /// Определить количество фаз травмы по длительности
        /// </summary>
        private int DeterminePhaseCount(dynamic tracker)
        {
            if (tracker.Phase3Duration > 0)
                return 3;
            if (tracker.Phase2Duration > 0)
                return 2;
            return 1;
        }

        /// <summary>
        /// Сохранить текущее состояние
        /// </summary>
        public void Save()
        {
            _dataHelper.WriteSaveData(SaveKey, _state);
            _monitor.Log($"Состояние сохранено: {_state.ActiveDebuffs.Count} активных дебаффов", LogLevel.Trace);
        }

        /// <summary>
        /// Проверить, был ли применен триггер
        /// </summary>
        public bool WasApplied(string id) => _state.AppliedTriggers.Contains(id);

        /// <summary>
        /// Пометить триггер как применённый
        /// </summary>
        public void MarkApplied(string id, bool value = true)
        {
            if (value)
                _state.AppliedTriggers.Add(id);
            else
                _state.AppliedTriggers.Remove(id);
            
            Save();
        }

        /// <summary>
        /// Проверить, был ли применён one-shot story trigger (сцена, письмо, уникальное событие CP).
        /// </summary>
        public bool WasStoryTriggerApplied(string id) => WasApplied(id);

        /// <summary>
        /// Пометить one-shot story trigger как применённый или снять отметку.
        /// </summary>
        public void MarkStoryTriggerApplied(string id, bool value = true)
        {
            MarkApplied(id, value);
        }

        private void EnsureInjuryCooldownState()
        {
            if (_state.InjuryCooldownUntilDay == null)
            {
                _state.InjuryCooldownUntilDay = new Dictionary<string, int>();
                _monitor.Log("InjuryCooldownUntilDay инициализирован (старый сейв без cooldown-словаря)", LogLevel.Debug);
            }

            if (_state.LastInjuryAppliedDayByTrigger == null)
                _state.LastInjuryAppliedDayByTrigger = new Dictionary<string, int>();

            MigrateLegacyInjuryCooldowns();
        }

        private void EnsurePrescriptionState()
        {
            if (_state.ActivePrescriptions == null)
            {
                _state.ActivePrescriptions = new Dictionary<string, PrescriptionState>();
                _monitor.Log("ActivePrescriptions инициализирован (старый сейв без предписаний)", LogLevel.Debug);
            }
        }

        private void EnsureComplianceState()
        {
            int clamped = Math.Clamp(_state.TreatmentComplianceScore, ComplianceManager.MinScore, ComplianceManager.MaxScore);
            if (clamped != _state.TreatmentComplianceScore)
            {
                _state.TreatmentComplianceScore = clamped;
                _monitor.Log($"TreatmentComplianceScore clamped to {clamped}", LogLevel.Debug);
            }
        }

        private void EnsureRehabState()
        {
            if (_state.ActiveRehabInjuryId == null && _state.RehabStartDay < 0)
                return;

            if (_state.RehabStartDay < 0)
                _state.RehabStartDay = (int)Game1.stats.DaysPlayed;

            if (_state.RehabDurationDays <= 0)
                _state.RehabDurationDays = 3;
        }

        private void EnsureRecoveryPlanState()
        {
            if (_state.ActiveRecoveryPlan == null)
                return;

            _state.ActiveRecoveryPlan.PlanId ??= "";
            _state.ActiveRecoveryPlan.Reason ??= "";
            _state.ActiveRecoveryPlan.TodayViolationReasons ??= new List<string>();
        }

        /// <summary>Миграция: старые сейвы без RecoveryPlan получают пустой экземпляр.</summary>
        private void EnsureRecoveryPlan()
        {
            _state.RecoveryPlan ??= new RecoveryPlanState();
            _state.RecoveryPlan.Tasks ??= new List<RecoveryPlanTask>();
            _state.RecoveryPlan.TodayViolations ??= new List<RecoveryPlanViolation>();
            _state.RecoveryPlan.TodayViolationReasons ??= new List<string>();
        }

        private void EnsureSelfCareState()
        {
            _state.SelfCareProtections ??= new Dictionary<string, int>();
        }

        private void EnsureMedicalMailState()
        {
            _state.SentMedicalMailDays ??= new Dictionary<string, int>();
        }

        private void MigrateLegacyInjuryCooldowns()
        {
            if (_state.LastInjuryAppliedDayByTrigger.Count == 0)
                return;

            const int legacyCooldownDays = 7;
            bool migrated = false;

            foreach (var (legacyKey, lastAppliedDay) in _state.LastInjuryAppliedDayByTrigger)
            {
                string cooldownKey = InjuryTriggerPolicy.MapTriggerKeyToBuffId(legacyKey) ?? legacyKey;
                int untilDay = lastAppliedDay + legacyCooldownDays;

                if (!_state.InjuryCooldownUntilDay.TryGetValue(cooldownKey, out int existingUntil)
                    || untilDay > existingUntil)
                {
                    _state.InjuryCooldownUntilDay[cooldownKey] = untilDay;
                    migrated = true;
                }
            }

            if (migrated)
            {
                _state.LastInjuryAppliedDayByTrigger.Clear();
                Save();
                _monitor.Log("Мигрированы legacy injury cooldowns → InjuryCooldownUntilDay", LogLevel.Debug);
            }
        }

        public bool IsInjuryOnCooldown(string key, int today)
        {
            EnsureInjuryCooldownState();

            return _state.InjuryCooldownUntilDay.TryGetValue(key, out int untilDay)
                && today < untilDay;
        }

        public int? GetInjuryCooldownUntilDay(string key)
        {
            EnsureInjuryCooldownState();

            return _state.InjuryCooldownUntilDay.TryGetValue(key, out int untilDay)
                ? untilDay
                : null;
        }

        public void SetInjuryCooldown(string key, int untilDay)
        {
            EnsureInjuryCooldownState();

            _state.InjuryCooldownUntilDay[key] = untilDay;
            Save();
        }

        public void ClearInjuryCooldown(string key)
        {
            EnsureInjuryCooldownState();

            if (_state.InjuryCooldownUntilDay.Remove(key))
                Save();
        }

        /// <summary>
        /// После полного выздоровления: короткий остаточный cooldown, если текущий уже истёк.
        /// </summary>
        public void ApplyResidualInjuryCooldownAfterRecovery(string key, int today, int residualDays = 2)
        {
            if (!InjuryTriggerPolicy.IsRepeatableInjuryBuff(key))
                return;

            EnsureInjuryCooldownState();

            int residualUntilDay = today + Math.Max(1, residualDays);
            if (!_state.InjuryCooldownUntilDay.TryGetValue(key, out int existingUntil)
                || existingUntil < residualUntilDay)
            {
                _state.InjuryCooldownUntilDay[key] = residualUntilDay;
                Save();
            }
        }

        // ============================================================================
        // ОСНОВНАЯ ТРАВМА
        // ============================================================================

        public string? GetMainInjuryId() => _state.MainInjuryId;

        public bool HasMainInjury() => !string.IsNullOrEmpty(_state.MainInjuryId);

        public bool IsMainInjury(string buffId) =>
            !string.IsNullOrEmpty(_state.MainInjuryId)
            && string.Equals(_state.MainInjuryId, buffId, StringComparison.OrdinalIgnoreCase);

        public void SetMainInjury(string buffId, bool force = false)
        {
            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                return;

            string? previousMain = _state.MainInjuryId;

            if (!force
                && !string.IsNullOrEmpty(previousMain)
                && !string.Equals(previousMain, buffId, StringComparison.OrdinalIgnoreCase))
            {
                string? preferred = InjurySets.SelectMainInjuryByPriority(
                    new[] { previousMain, buffId });

                if (preferred == null
                    || string.Equals(preferred, previousMain, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(previousMain)
                && !string.Equals(previousMain, buffId, StringComparison.OrdinalIgnoreCase))
            {
                ResetNeglectStrikes(previousMain);
                _monitor.Log(
                    $"[Neglect] Сброс счётчика при смене MainInjuryId: {previousMain} -> {buffId}",
                    LogLevel.Debug);
            }

            _state.MainInjuryId = buffId;
            QaSuppressMainInjuryAutoSync = false;
            Save();
            _monitor.Log($"[MainInjury] Установлена основная травма: {buffId}", LogLevel.Debug);
        }

        public void ClearMainInjury(string buffId)
        {
            if (string.IsNullOrEmpty(_state.MainInjuryId))
                return;

            if (!string.Equals(_state.MainInjuryId, buffId, StringComparison.OrdinalIgnoreCase))
                return;

            _state.MainInjuryId = null;
            Save();
            _monitor.Log($"[MainInjury] Снята основная травма: {buffId}", LogLevel.Debug);
        }

        /// <summary>
        /// Полное выздоровление от основной травмы: очистить MainInjuryId с логом завершения.
        /// </summary>
        public void CompleteMainInjury(string injuryId)
        {
            if (!IsMainInjury(injuryId))
                return;

            ResetNeglectStrikes(injuryId);
            ClearMainInjury(injuryId);
            _monitor.Log($"[MainInjury] Основная травма завершена: {injuryId}", LogLevel.Info);
        }

        /// <summary>
        /// DEBUG ONLY: очистить MainInjuryId без удаления баффов и DebuffState.
        /// </summary>
        /// <summary>When true, GetCurrentMainInjuryId does not auto-fill MainInjuryId from ActiveDebuffs (QA migration tests).</summary>
        public bool QaSuppressMainInjuryAutoSync { get; set; }

        public void DebugClearMainInjuryId()
        {
            if (string.IsNullOrEmpty(_state.MainInjuryId))
            {
                _monitor.Log("[MainInjury][Debug] MainInjuryId уже пуст", LogLevel.Info);
                QaSuppressMainInjuryAutoSync = true;
                Save();
                return;
            }

            string previous = _state.MainInjuryId;
            _state.MainInjuryId = null;
            QaSuppressMainInjuryAutoSync = true;
            Save();
            _monitor.Log(
                $"[MainInjury][Debug] MainInjuryId очищен (было: {previous}). Баффы не удалены. Auto-sync suppressed.",
                LogLevel.Info);
        }

        /// <summary>
        /// DEBUG ONLY: установить MainInjuryId, если есть DebuffState для buffId.
        /// </summary>
        public bool DebugSetMainInjuryId(string buffId)
        {
            if (string.IsNullOrWhiteSpace(buffId))
                return false;

            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
            {
                _monitor.Log(
                    $"[MainInjury][Debug] Отказ: {buffId} — осложнение, не основная травма",
                    LogLevel.Warn);
                return false;
            }

            if (!_state.ActiveDebuffs.ContainsKey(buffId))
            {
                _monitor.Log(
                    $"[MainInjury][Debug] Отказ: DebuffState для {buffId} не найден",
                    LogLevel.Warn);
                return false;
            }

            _state.MainInjuryId = buffId;
            QaSuppressMainInjuryAutoSync = false;
            Save();
            _monitor.Log($"[MainInjury][Debug] MainInjuryId установлен: {buffId}", LogLevel.Info);
            return true;
        }

        // ============================================================================
        // ПЛАН ВОССТАНОВЛЕНИЯ ХАРВИ
        // ============================================================================

        public HospitalDischargePlanState? GetActiveRecoveryPlan() => _state.ActiveRecoveryPlan;

        public void SetActiveRecoveryPlan(HospitalDischargePlanState plan)
        {
            _state.ActiveRecoveryPlan = plan;
            EnsureRecoveryPlanState();
            Save();
        }

        public RecoveryPlanState GetRecoveryPlan()
        {
            EnsureRecoveryPlan();
            return _state.RecoveryPlan;
        }

        public void ClearActiveRecoveryPlan()
        {
            if (_state.ActiveRecoveryPlan == null)
                return;

            _state.ActiveRecoveryPlan = null;
            Save();
        }

        /// <summary>
        /// Зафиксировать нарушение режима восстановления с учётом тяжести.
        /// Не понижает severity за день; при повышении тяжести того же типа обновляет счётчики.
        /// </summary>
        public bool TryRegisterRecoveryViolation(string type, int severity, bool failDay, bool needsHarveyVisit)
        {
            type = type ?? "";
            EnsureRecoveryViolationDailyState();

            int today = GameUtils.Today();

            if (_state.RecoveryPlanTodayViolationSeverities.TryGetValue(type, out int existingSeverity))
            {
                if (existingSeverity >= severity)
                    return false;

                AdjustRecoverySeverityCounter(existingSeverity, increment: false);
                AdjustRecoverySeverityCounter(severity, increment: true);
                _state.RecoveryPlanTodayViolationSeverities[type] = severity;

                _state.LastRecoveryViolationType = type;
                _state.LastRecoveryViolationSeverity = severity;
                _state.LastRecoveryViolationDay = today;
                _state.LastRecoveryViolationTime = Game1.timeOfDay;

                if (failDay)
                    _state.RecoveryPlanDayFailed = true;

                if (needsHarveyVisit)
                    _state.RecoveryPlanNeedsHarveyVisit = true;

                Save();
                LogRecoveryViolation(type, severity, today, failDay, needsHarveyVisit, upgraded: true);
                return true;
            }

            _state.RecoveryPlanTodayViolationSeverities[type] = severity;
            ApplyRecoveryViolation(type, severity, failDay, needsHarveyVisit, today);
            return true;
        }

        /// <summary>Сбросить дневные флаги нарушений (начало нового игрового дня).</summary>
        public void ResetRecoveryViolationDailyState()
        {
            EnsureRecoveryViolationDailyState();

            bool changed = _state.RecoveryPlanDayFailed
                || _state.RecoveryPlanExtendedToday
                || _state.RecoveryPlanTodayViolationSeverities.Count > 0;

            _state.RecoveryPlanDayFailed = false;
            _state.RecoveryPlanExtendedToday = false;
            _state.RecoveryPlanTodayViolationSeverities.Clear();

            if (changed)
                Save();
        }

        private void EnsureRecoveryViolationDailyState()
        {
            _state.RecoveryPlanTodayViolationSeverities ??=
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        private void ApplyRecoveryViolation(string type, int severity, bool failDay, bool needsHarveyVisit, int today)
        {
            _state.LastRecoveryViolationType = type;
            _state.LastRecoveryViolationSeverity = severity;
            _state.LastRecoveryViolationDay = today;
            _state.LastRecoveryViolationTime = Game1.timeOfDay;
            _state.RecoveryPlanTotalViolations++;
            AdjustRecoverySeverityCounter(severity, increment: true);

            if (failDay)
                _state.RecoveryPlanDayFailed = true;

            if (needsHarveyVisit)
                _state.RecoveryPlanNeedsHarveyVisit = true;

            Save();
            LogRecoveryViolation(type, severity, today, failDay, needsHarveyVisit, upgraded: false);
        }

        private void AdjustRecoverySeverityCounter(int severity, bool increment)
        {
            int delta = increment ? 1 : -1;

            switch (severity)
            {
                case RecoveryViolationSeverity.Mild:
                    _state.RecoveryPlanMildViolations = Math.Max(0, _state.RecoveryPlanMildViolations + delta);
                    break;
                case RecoveryViolationSeverity.Medium:
                    _state.RecoveryPlanMediumViolations = Math.Max(0, _state.RecoveryPlanMediumViolations + delta);
                    break;
                case RecoveryViolationSeverity.Severe:
                    _state.RecoveryPlanSevereViolations = Math.Max(0, _state.RecoveryPlanSevereViolations + delta);
                    break;
            }
        }

        private void LogRecoveryViolation(
            string type,
            int severity,
            int today,
            bool failDay,
            bool needsHarveyVisit,
            bool upgraded)
        {
            string severityLabel = severity switch
            {
                RecoveryViolationSeverity.Mild => "mild",
                RecoveryViolationSeverity.Medium => "medium",
                RecoveryViolationSeverity.Severe => "severe",
                _ => "none",
            };

            string action = upgraded ? "эскалация" : "нарушение";
            _monitor.Log(
                $"[RecoveryPlan] {action} #{_state.RecoveryPlanTotalViolations}: type={type}, severity={severityLabel} ({severity}), day={today}, time={Game1.timeOfDay}, failDay={failDay}, needsHarveyVisit={needsHarveyVisit}",
                LogLevel.Info);
        }

        /// <summary>QA: зарегистрировать нарушение (снимает dedup по типу за сегодня, без HUD/topics).</summary>
        public bool DebugRegisterRecoveryViolation(string type, int severity)
        {
            type = type ?? "";
            EnsureRecoveryViolationDailyState();

            if (_state.RecoveryPlanTodayViolationSeverities.TryGetValue(type, out int existing))
            {
                AdjustRecoverySeverityCounter(existing, increment: false);
                _state.RecoveryPlanTodayViolationSeverities.Remove(type);
                if (_state.RecoveryPlanTotalViolations > 0)
                    _state.RecoveryPlanTotalViolations--;
            }

            bool failDay = severity >= RecoveryViolationSeverity.Medium;
            bool needsHarveyVisit = severity >= RecoveryViolationSeverity.Medium;
            return TryRegisterRecoveryViolation(type, severity, failDay, needsHarveyVisit);
        }

        /// <summary>QA: сбросить поля нарушения режима; includeCounters — также Total/Mild/Medium/Severe.</summary>
        public void ClearRecoveryViolationState(bool includeCounters = false)
        {
            EnsureRecoveryViolationDailyState();

            _state.LastRecoveryViolationType = "";
            _state.LastRecoveryViolationSeverity = RecoveryViolationSeverity.None;
            _state.RecoveryPlanDayFailed = false;
            _state.RecoveryPlanNeedsHarveyVisit = false;
            _state.RecoveryPlanExtendedToday = false;
            _state.RecoveryPlanTodayViolationSeverities.Clear();

            if (includeCounters)
            {
                _state.RecoveryPlanTotalViolations = 0;
                _state.RecoveryPlanMildViolations = 0;
                _state.RecoveryPlanMediumViolations = 0;
                _state.RecoveryPlanSevereViolations = 0;
            }

            Save();
        }

        // ============================================================================
        // МЕТОДЫ РАБОТЫ С DEBUFFSTATE
        // ============================================================================

        /// <summary>
        /// Получить состояние дебаффа
        /// </summary>
        public DebuffState? GetDebuffState(string buffId)
        {
            return _state.ActiveDebuffs.TryGetValue(buffId, out var state) ? state : null;
        }

        /// <summary>
        /// Создать новое состояние дебаффа
        /// </summary>
        public DebuffState CreateDebuffState(string buffId, int currentDay, int phase1Duration, int phase2Duration, int phase3Duration)
        {
            if (_state.ActiveDebuffs.TryGetValue(buffId, out var existing))
            {
                _monitor.Log($"DebuffState для {buffId} уже существует, не перезаписываем активное лечение", LogLevel.Warn);
                return existing;
            }

            var debuffState = new DebuffState
            {
                BuffId = buffId,
                InjuryStartDay = currentDay,
                TreatmentStarted = false,
                HarveyConversationHappened = false,
                TotalPhases = phase3Duration > 0 ? 3 : (phase2Duration > 0 ? 2 : 0),
                CurrentPhase = 0,
                PhaseStartDay = currentDay,
                Phase1Duration = phase1Duration,
                Phase2Duration = phase2Duration,
                Phase3Duration = phase3Duration,
                ReadyForNextPhase = false,
                ReadyForRecovery = false
            };
            
            _state.ActiveDebuffs[buffId] = debuffState;
            Save();
            
            _monitor.Log($"Создано состояние дебаффа: {buffId} (фазы: {debuffState.TotalPhases})", LogLevel.Debug);
            return debuffState;
        }

        /// <summary>
        /// Создать состояние для осложнения (WetBandage, DirtyWound, WetStitches и т.д.).
        /// TotalPhases = 0 — признак осложнения для восстановления баффа утром.
        /// </summary>
        public void CreateComplicationState(string compId, int startDay)
        {
            var debuffState = new DebuffState
            {
                BuffId = compId,
                InjuryStartDay = startDay,
                TreatmentStarted = true,
                HarveyConversationHappened = false,
                TotalPhases = 0,
                CurrentPhase = 1,
                PhaseStartDay = startDay,
                Phase1Duration = 0,
                Phase2Duration = 0,
                Phase3Duration = 0,
                ReadyForNextPhase = false,
                ReadyForRecovery = false
            };
            _state.ActiveDebuffs[compId] = debuffState;
            Save();
            _monitor.Log($"Создано состояние осложнения: {compId}", LogLevel.Debug);
        }

        /// <summary>
        /// Обновить состояние дебаффа
        /// </summary>
        public void UpdateDebuffState(string buffId, DebuffState debuffState)
        {
            _state.ActiveDebuffs[buffId] = debuffState;
            Save();
        }

        /// <summary>
        /// Удалить состояние дебаффа
        /// </summary>
        public void RemoveDebuffState(string buffId)
        {
            if (!_state.ActiveDebuffs.Remove(buffId))
                return;

            if (!InjurySets.KnownComplicationBuffIds.Contains(buffId))
                ClearMainInjury(buffId);

            Save();
            _monitor.Log($"Удалено состояние дебаффа: {buffId}", LogLevel.Debug);
        }

        /// <summary>
        /// Проверить, существует ли состояние дебаффа
        /// </summary>
        public bool HasDebuffState(string buffId)
        {
            return _state.ActiveDebuffs.ContainsKey(buffId);
        }

        /// <summary>
        /// Получить все активные состояния дебаффов
        /// </summary>
        public System.Collections.Generic.List<DebuffState> GetAllActiveDebuffStates()
        {
            return _state.ActiveDebuffs.Values.ToList();
        }

        /// <summary>
        /// Начать лечение дебаффа
        /// </summary>
        public void StartTreatment(string buffId, int currentDay)
        {
            if (_state.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
            {
                debuffState.StartTreatment(currentDay);
                debuffState.HarveyConversationHappened = true;
                ResetNeglectStrikes(buffId);
                Save();
                
                _monitor.Log($"Начато лечение: {buffId} (фаза 1/{debuffState.TotalPhases})", LogLevel.Info);
            }
        }

        private void EnsureNeglectStrikesState()
        {
            _state.NeglectStrikesByInjury ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (_state.NeglectStrikes <= 0 || _state.NeglectStrikesByInjury.Count > 0)
                return;

            if (!string.IsNullOrEmpty(_state.MainInjuryId))
            {
                _state.NeglectStrikesByInjury[_state.MainInjuryId] = _state.NeglectStrikes;
                _monitor.Log(
                    $"[Neglect] Миграция NeglectStrikes={_state.NeglectStrikes} -> {_state.MainInjuryId}",
                    LogLevel.Debug);
            }

            _state.NeglectStrikes = 0;
        }

        public int GetNeglectStrikes(string injuryId)
        {
            if (string.IsNullOrEmpty(injuryId))
                return 0;

            EnsureNeglectStrikesState();
            return _state.NeglectStrikesByInjury.TryGetValue(injuryId, out int strikes) ? strikes : 0;
        }

        public int IncrementNeglectStrikes(string injuryId)
        {
            EnsureNeglectStrikesState();
            int strikes = GetNeglectStrikes(injuryId) + 1;
            _state.NeglectStrikesByInjury[injuryId] = strikes;
            return strikes;
        }

        public void ResetNeglectStrikes(string? injuryId = null)
        {
            EnsureNeglectStrikesState();

            if (string.IsNullOrEmpty(injuryId))
            {
                if (_state.NeglectStrikesByInjury.Count == 0)
                    return;

                _state.NeglectStrikesByInjury.Clear();
                _monitor.Log("[Neglect] Сброс всех NeglectStrikesByInjury", LogLevel.Debug);
                return;
            }

            if (_state.NeglectStrikesByInjury.Remove(injuryId))
            {
                _monitor.Log($"[Neglect] Сброс NeglectStrikesByInjury для {injuryId}", LogLevel.Debug);
            }
        }

        /// <summary>DEBUG: set neglect strike counter for one injury (QA scenario 11).</summary>
        public void SetNeglectStrikesForQa(string injuryId, int strikes)
        {
            if (string.IsNullOrWhiteSpace(injuryId))
                return;

            EnsureNeglectStrikesState();
            int value = Math.Max(0, strikes);
            if (value == 0)
            {
                ResetNeglectStrikes(injuryId);
                return;
            }

            _state.NeglectStrikesByInjury[injuryId] = value;
            Save();
            _monitor.Log($"[Neglect][QA] NeglectStrikesByInjury[{injuryId}]={value}", LogLevel.Info);
        }

        /// <summary>
        /// Перейти к следующей фазе дебаффа
        /// </summary>
        public void AdvancePhase(string buffId, int currentDay)
        {
            if (_state.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
            {
                int oldPhase = debuffState.CurrentPhase;
                debuffState.AdvancePhase(currentDay);
                ClearPhaseReadyTracking(debuffState);
                Save();
                
                _monitor.Log($"Смена фазы {buffId}: {oldPhase} → {debuffState.CurrentPhase}", LogLevel.Info);
            }
        }

        /// <summary>
        /// Отметить разговор с Харви о дебаффе
        /// </summary>
        public void MarkHarveyConversation(string buffId, bool happened = true)
        {
            if (_state.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
            {
                debuffState.HarveyConversationHappened = happened;
                Save();
            }
        }

        /// <summary>
        /// Сбросить ReadyForNextPhase у нефазовых травм (TotalPhases == 0).
        /// ReadyForRecovery не трогаем — он может использоваться для завершения простого лечения.
        /// </summary>
        public int SanitizeNonPhasedReadyFlags()
        {
            int fixedCount = 0;

            foreach (var (buffId, debuffState) in _state.ActiveDebuffs)
            {
                if (!debuffState.ReadyForNextPhase)
                    continue;

                if (debuffState.TotalPhases != 0 && !TreatmentManager.IsSimpleTreatmentInjury(buffId))
                    continue;

                debuffState.ReadyForNextPhase = false;
                fixedCount++;
                _monitor.Log(
                    $"🔧 Санитарная очистка: {buffId} ReadyForNextPhase сброшен (TotalPhases=0, не фазовая травма)",
                    LogLevel.Warn);
            }

            if (fixedCount > 0)
                Save();

            return fixedCount;
        }

        /// <summary>
        /// Установить флаг готовности к следующей фазе
        /// </summary>
        public void SetReadyForNextPhase(string buffId, bool ready = true)
        {
            if (_state.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
            {
                debuffState.ReadyForNextPhase = ready;
                UpdatePhaseReadyTracking(debuffState, ready);
                Save();
            }
        }

        /// <summary>
        /// Установить флаг готовности к выздоровлению
        /// </summary>
        public void SetReadyForRecovery(string buffId, bool ready = true)
        {
            if (_state.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
            {
                debuffState.ReadyForRecovery = ready;
                UpdatePhaseReadyTracking(debuffState, ready);
                Save();
            }
        }

        private static void UpdatePhaseReadyTracking(DebuffState debuffState, bool ready)
        {
            if (!ready)
                CheckupManager.ClearCheckupTracking(debuffState);
        }

        private static void ClearPhaseReadyTracking(DebuffState debuffState)
        {
            CheckupManager.ClearCheckupTracking(debuffState);
        }

        /// <summary>
        /// Получить все активные дебаффы в лечении
        /// </summary>
        public System.Collections.Generic.List<DebuffState> GetActiveDebuffsInTreatment()
        {
            return _state.ActiveDebuffs.Values
                .Where(d => d.IsInTreatment)
                .ToList();
        }

        /// <summary>
        /// Получить все дебаффы, готовые к смене фазы
        /// </summary>
        public System.Collections.Generic.List<DebuffState> GetDebuffsReadyForNextPhase()
        {
            return _state.ActiveDebuffs.Values
                .Where(d => d.ReadyForNextPhase)
                .ToList();
        }

        // ============================================================================
        // УСТАРЕВШИЕ МЕТОДЫ (для совместимости)
        // ============================================================================

        /// <summary>
        /// Проверить, был ли разговор о лечении конкретной травмы
        /// </summary>
        [System.Obsolete("Используйте GetDebuffState(buffId)?.HarveyConversationHappened")]
        public bool WasTreatmentDiscussed(string injuryId)
        {
            return GetDebuffState(injuryId)?.HarveyConversationHappened ?? false;
        }

        /// <summary>
        /// Пометить разговор о лечении как состоявшийся
        /// </summary>
        [System.Obsolete("Используйте MarkHarveyConversation(buffId, true)")]
        public void MarkTreatmentDiscussed(string injuryId, bool discussed = true)
        {
            MarkHarveyConversation(injuryId, discussed);
        }

        /// <summary>
        /// Очистить все данные (новая игра)
        /// </summary>
        public void Clear()
        {
            _state = new InjuryState();
            Save();
            _monitor.Log("Состояние очищено", LogLevel.Debug);
        }
    }
}

