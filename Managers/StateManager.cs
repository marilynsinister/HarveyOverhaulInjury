using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using StardewModdingAPI;
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

            // Миграция старых данных ActivePhases в новую систему ActiveDebuffs
            MigrateOldPhaseData();
            
            // Миграция осложнений из ActiveComplications в ActiveDebuffs (DebuffState)
            MigrateComplicationsToDebuffState();
            
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
            if (_state.ActiveDebuffs.Remove(buffId))
            {
                Save();
                _monitor.Log($"Удалено состояние дебаффа: {buffId}", LogLevel.Debug);
            }
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
                Save();
                
                _monitor.Log($"Начато лечение: {buffId} (фаза 1/{debuffState.TotalPhases})", LogLevel.Info);
            }
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
                Save();
            }
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

