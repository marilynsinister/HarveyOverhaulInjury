using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик игровых событий (начало/конец дня)
    /// </summary>
    public class GameEventHandler
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly TreatmentManager _treatmentManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly DialogueManager _dialogueManager;
        private readonly ComplicationManager _complicationManager;
        private readonly PrescriptionManager _prescriptionManager;
        private readonly ComplianceManager _complianceManager;
        private readonly CheckupManager _checkupManager;
        private readonly RehabManager _rehabManager;
        private readonly SelfCareManager _selfCareManager;
        private InteractionHandler? _interactionHandler;
        private PassOutHandler? _passOutHandler;

        public GameEventHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            TreatmentManager treatmentManager,
            HospitalizationManager hospitalizationManager,
            DialogueManager dialogueManager,
            ComplicationManager complicationManager,
            PrescriptionManager prescriptionManager,
            ComplianceManager complianceManager,
            CheckupManager checkupManager,
            RehabManager rehabManager,
            SelfCareManager selfCareManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _treatmentManager = treatmentManager;
            _hospitalizationManager = hospitalizationManager;
            _dialogueManager = dialogueManager;
            _complicationManager = complicationManager;
            _prescriptionManager = prescriptionManager;
            _complianceManager = complianceManager;
            _checkupManager = checkupManager;
            _rehabManager = rehabManager;
            _selfCareManager = selfCareManager;
        }

        /// <summary>
        /// Установить ссылку на InteractionHandler для сброса флагов
        /// </summary>
        public void SetInteractionHandler(InteractionHandler interactionHandler)
        {
            _interactionHandler = interactionHandler;
        }

        /// <summary>
        /// Установить ссылку на PassOutHandler для событий спасения
        /// </summary>
        public void SetPassOutHandler(PassOutHandler passOutHandler)
        {
            _passOutHandler = passOutHandler;
        }

        /// <summary>
        /// Начало нового дня (оптимизированная версия)
        /// </summary>
        public void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            try
            {
                _monitor.Log("=== Начало дня ===", LogLevel.Debug);

                // Все операции выполняем с задержкой, чтобы не блокировать начало дня
                Game1.delayedActions.Add(new StardewValley.DelayedAction(100, () =>
                {
                    try
                    {
                        // 1. Восстанавливаем баффы из снапшота конца прошлого дня
                        RestoreBuffsFromSnapshot();

                        // 1b. Сброс некорректного ReadyForNextPhase у простого лечения (TotalPhases == 0)
                        _stateManager.SanitizeNonPhasedReadyFlags();

                        // 2. Письмо и дебафф «Харви запретил шахту» — на следующий день после предупреждения в шахте
                        ApplyMineForbiddenIfWarningWasYesterday();

                        // 3. Снятие дебаффа «Харви запретил шахту» после N дней
                        ExpireMineForbiddenIfDue();

                        // 4. Запуск события спасения из шахты — ПОСЛЕ восстановления баффов,
                        //    чтобы HasAnyBuff(Severe) корректно видел все активные травмы
                        _passOutHandler?.TriggerMineRescueEvents();

                        // 5. Проверяем осложнения (перерастание в инфекцию и т.д.)
                        bool infectionEscalated = _complicationManager.CheckTreatmentCompletion();

                        // 6–7. После эскалации в buffInfectedWound не проверяем завершение/фазы —
                        // старые DebuffState уже сняты, но защита от ложных HUD обязательна.
                        if (!infectionEscalated)
                        {
                            // 6. Проверяем завершение нефазового лечения (buffHurt, buffBadlyHurt, buffSurgicalWound)
                            CheckSimpleTreatmentCompletion();

                            // 7. Проверяем прогресс фаз и устанавливаем флаги готовности
                            CheckInjuryPhases();
                        }
                        else
                        {
                            _monitor.Log(
                                "[DayStarted] Пропуск CheckSimpleTreatmentCompletion/CheckInjuryPhases после эскалации инфекции",
                                LogLevel.Debug);
                        }

                        _checkupManager.ProcessMissedCheckupsDaily(GetToday());
                        _complianceManager.TryShowLowComplianceReminder();

                        _rehabManager.CompleteRehabIfDue(GetToday());

                        // 8. Предписания: снять истёкшие, начислить TreatmentComplianceScore за вчера
                        _prescriptionManager.RemoveExpiredPrescriptions();
                        _prescriptionManager.RewardComplianceDaily();

                        _monitor.Log("Инициализация дня завершена", LogLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        _monitor.Log($"Ошибка в отложенной инициализации дня: {ex}", LogLevel.Error);
                    }
                }));
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка в OnDayStarted: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Конец дня (перед сном)
        /// </summary>
        public void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            try
            {
                _monitor.Log("=== Конец дня ===", LogLevel.Debug);

                // Проверка заброшенности лечения
                CheckNeglect();

                _selfCareManager.TryApplyRestCareOnDayEnding();
                CheckRestPrescriptionViolation();
                _rehabManager.CheckRehabViolationLateSleep();

                // Письмо о запрете шахты — на следующий день после предупреждения в шахте
                int todayEnd = GetToday();
                if (_stateManager.State.MineWarningDay == todayEnd && _config.SendLetters)
                {
                    HarveyMailHelper.TryScheduleTieredMail(_config, _stateManager, _monitor, MailIds.MineForbidden);
                    _monitor.Log($"[Шахта] Письмо о запрете шахты запланировано на завтра (день предупреждения: {todayEnd})", LogLevel.Debug);
                }

                // Сохраняем снапшот активных баффов мода на момент конца дня
                _stateManager.State.SavedActiveBuffs = _buffManager.GetActiveModBuffs();
                _monitor.Log($"Снапшот баффов ({_stateManager.State.SavedActiveBuffs.Count}): {string.Join(", ", _stateManager.State.SavedActiveBuffs)}", LogLevel.Debug);

                // Сохранение состояния
                _stateManager.Save();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка в OnDayEnding: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Проверить фазы активных травм
        /// </summary>
        private void CheckInjuryPhases()
        {
            int today = GetToday();
            string? mainInjuryId = _stateManager.GetMainInjuryId();
            
            foreach (var kvp in _stateManager.State.ActiveDebuffs)
            {
                string injuryId = kvp.Key;
                var debuffState = kvp.Value;

                if (_stateManager.State.ActiveComplications.ContainsKey(injuryId)
                    || InjurySets.KnownComplicationBuffIds.Contains(injuryId))
                    continue;

                if (injuryId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Пропускаем осложнения (TotalPhases == 0) — ими управляет ComplicationManager
                if (debuffState.TotalPhases == 0) continue;

                if (!string.IsNullOrEmpty(mainInjuryId)
                    && !string.Equals(injuryId, mainInjuryId, StringComparison.OrdinalIgnoreCase))
                    continue;
                
                // Пропускаем нелеченные травмы (CurrentPhase = 0)
                if (!debuffState.IsInTreatment)
                    continue;

                // Проверяем, прошло ли достаточно времени для смены фазы
                if (debuffState.HasPhaseTimeElapsed(today))
                {
                    // Если это не последняя фаза - готовность к смене фазы
                    if (!debuffState.IsLastPhase)
                    {
                        // Устанавливаем флаг готовности к смене фазы (если ещё не установлен)
                        if (!debuffState.ReadyForNextPhase)
                        {
                            int nextPhase = debuffState.CurrentPhase + 1;
                            _checkupManager.OnPhaseCheckupDue(injuryId, debuffState, nextPhase, today);
                            _stateManager.SetReadyForNextPhase(injuryId, true);
                            _monitor.Log($"📍 Установлен флаг готовности к смене фазы: {injuryId} (фаза {debuffState.CurrentPhase} → {nextPhase})", LogLevel.Info);
                            
                            ShowPhaseTransitionReminder(injuryId, nextPhase);
                        }
                    }
                    // Если последняя фаза завершена - готовность к выздоровлению
                    else
                    {
                        // Устанавливаем флаг готовности к выздоровлению (если ещё не установлен)
                        if (!debuffState.ReadyForRecovery)
                        {
                            _checkupManager.OnRecoveryCheckupDue(injuryId, debuffState, today);
                            _stateManager.SetReadyForRecovery(injuryId, true);
                            _monitor.Log($"🎉 Установлен флаг готовности к выздоровлению: {injuryId}", LogLevel.Info);
                            
                            ShowRecoveryReminder(injuryId);
                        }
                    }
                }
            }
        }

        private void ShowPhaseTransitionReminder(string injuryId, int nextPhase)
        {
            string injuryName = _injuryManager.GetInjuryName(injuryId);
            string phaseName = nextPhase switch
            {
                2 => "заживления",
                3 => "восстановления",
                _ => "следующей стадии"
            };
            
            Game1.addHUDMessage(new HUDMessage(
                $"Твоя травма готова к стадии {phaseName}. Посети Харви для продолжения лечения!",
                HUDMessage.health_type));
            
            _monitor.Log($"💡 Напоминание: {injuryName} готова к фазе {nextPhase}", LogLevel.Info);
        }

        private void ShowRecoveryReminder(string injuryId)
        {
            string injuryName = _injuryManager.GetInjuryName(injuryId);
            
            Game1.addHUDMessage(new HUDMessage(
                $"Твоя травма полностью зажила! Посети Харви для финального осмотра!",
                HUDMessage.achievement_type));
            
            _monitor.Log($"🎉 Напоминание: {injuryName} готова к полному выздоровлению", LogLevel.Info);
        }


        // private void AdvanceToNextPhase(string injuryId, Core.Models.InjuryPhaseTracker tracker)
        // {
        //     _monitor.Log($"Переход к фазе {tracker.CurrentPhase + 1} для {injuryId}", LogLevel.Info);
            
        //     // Удалить старый фазовый бафф
        //     string oldPhase = _injuryManager.GetPhaseBuffId(injuryId, tracker.CurrentPhase);
        //     _buffManager.RemoveBuff(oldPhase);

        //     // Перейти к следующей фазе
        //     tracker.CurrentPhase++;
        //     tracker.PhaseStartDay = GetToday();

        //     // Добавить новый фазовый бафф
        //     string newPhase = _injuryManager.GetPhaseBuffId(injuryId, tracker.CurrentPhase);
        //     _buffManager.AddBuff(newPhase, -2);

        //     // Создать топик фазового перехода (формат topic<InjuryName>Phase<StageName>)
        //     string phaseTopicId = _injuryManager.GetPhaseTopicId(injuryId, tracker.CurrentPhase);
        //     _dialogueManager.AddTopic(phaseTopicId, 7);
        //     _monitor.Log($"💬 Создан топик фазового перехода: {phaseTopicId}", LogLevel.Debug);

        //     _stateManager.Save();
        // }
        /// <summary>
        /// Проверить заброшенность лечения
        /// </summary>
        private void CheckRestPrescriptionViolation()
        {
            if (Game1.timeOfDay < 2400)
                return;

            if (!_prescriptionManager.HasActivePrescription(PrescriptionIds.Rest))
                return;

            if (!_prescriptionManager.TryMarkViolation(PrescriptionIds.Rest, "late_sleep", out int count))
                return;

            string hud = count switch
            {
                1 => "Харви просил отдыхать и ложиться раньше...",
                _ => "Ты снова легла спать слишком поздно. Харви это заметит."
            };
            Game1.addHUDMessage(new HUDMessage(hud, count >= 2 ? HUDMessage.error_type : HUDMessage.health_type));
            _monitor.Log($"[Prescription] Rest late_sleep violation #{count} at {Game1.timeOfDay}", LogLevel.Info);
        }

        private void CheckNeglect()
        {
            int today = GetToday();

            if (_stateManager.State.LastInfectionEscalationDay == today)
            {
                _stateManager.ResetNeglectStrikes();
                _monitor.Log(
                    "[Neglect] Пропуск: в этот день DirtyWound/WetBandage эскалировали в buffInfectedWound",
                    LogLevel.Debug);
                return;
            }

            string? mainInjuryId = _stateManager.GetMainInjuryId() ?? _injuryManager.GetActiveInjury();
            string? untreatedInjury = mainInjuryId;

            // Нелеченная main-травма: базовый бафф мог быть снят, DebuffState остался
            if (!string.IsNullOrEmpty(mainInjuryId)
                && !_injuryManager.HasInjuryOrPhase(mainInjuryId))
            {
                var mainDebuff = _stateManager.GetDebuffState(mainInjuryId);
                if (mainDebuff is not { IsPhasedInjury: true, TreatmentStarted: false })
                    untreatedInjury = null;
            }

            if (string.IsNullOrEmpty(untreatedInjury))
            {
                _stateManager.ResetNeglectStrikes();
                return;
            }

            if (!_treatmentManager.HasMatchingTreatment(untreatedInjury))
            {
                int strikes = _stateManager.IncrementNeglectStrikes(untreatedInjury);
                _monitor.Log(
                    $"Заброшенность лечения ({untreatedInjury}): {strikes} дней",
                    LogLevel.Debug);

                if (strikes >= _config.NeglectDaysThreshold)
                {
                    ApplyNeglectPenalty();
                }
            }
            else
            {
                _stateManager.ResetNeglectStrikes(untreatedInjury);
            }
        }

        private void ApplyNeglectPenalty()
        {
            _monitor.Log("Применение штрафа за заброшенность", LogLevel.Warn);
            _buffManager.AddBuff(InjuryBuffs.Neglect, -2);
            _dialogueManager.AddTopic(ConversationTopics.Neglect, 7);

            // Если Харви рядом — короткая реплика о пропуске лечения (без снижения Friendship)
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null && Game1.currentLocation?.characters.Contains(harvey) == true)
            {
                _dialogueManager.ShowEmoteWithText(harvey, 
                    HarveyEmotes.NeglectedCare,
                    HarveyTextMessages.NotTreating);
            }

            Game1.addHUDMessage(new HUDMessage("Ты запустил${^а}$ лечение...", HUDMessage.error_type));
        }

        private int GetToday() => (int)Game1.stats.DaysPlayed;

        /// <summary>
        /// На следующий день после предупреждения в шахте: наложить дебафф «Харви запретил шахту» и сбросить флаг.
        /// </summary>
        private void ApplyMineForbiddenIfWarningWasYesterday()
        {
            int today = GetToday();
            int yesterday = today - 1;
            if (_stateManager.State.MineWarningDay != yesterday) return;

            _stateManager.State.MineWarningDay = -1;
            _stateManager.State.MineForbiddenAppliedDay = today;
            _buffManager.AddBuff(InjuryBuffs.MineForbidden, -2);
            _monitor.Log($"[Шахта] Наложен дебафф «Харви запретил шахту» на {_config.MineForbiddenDurationDays} дн. (день {today})", LogLevel.Info);
        }

        /// <summary>
        /// Снять дебафф «Харви запретил шахту» после истечения срока (MineForbiddenDurationDays дней).
        /// </summary>
        private void ExpireMineForbiddenIfDue()
        {
            int today = GetToday();
            int appliedDay = _stateManager.State.MineForbiddenAppliedDay;
            if (appliedDay < 0) return;

            int durationDays = Math.Max(1, _config.MineForbiddenDurationDays);
            if (today >= appliedDay + durationDays)
            {
                _buffManager.RemoveBuff(InjuryBuffs.MineForbidden);
                _stateManager.State.MineForbiddenAppliedDay = -1;
                _monitor.Log($"[Шахта] Снят дебафф «Харви запретил шахту» (истёк срок: {durationDays} дн.)", LogLevel.Debug);
            }
        }

        /// <summary>
        /// Восстанавливает баффы мода из снапшота, сделанного в конце прошлого дня.
        /// Complication/main buffs восстанавливаются только при валидном состоянии InjuryState.
        /// </summary>
        private void RestoreBuffsFromSnapshot()
        {
            var saved = _stateManager.State.SavedActiveBuffs;
            if (saved.Count == 0)
            {
                _monitor.Log("Снапшот баффов пуст — нечего восстанавливать", LogLevel.Debug);
                return;
            }

            int restored = 0;
            foreach (string buffId in saved.ToList())
            {
                if (ShouldRestoreSavedBuff(buffId, out string? skipReason))
                {
                    if (!_buffManager.HasBuff(buffId))
                    {
                        _buffManager.AddBuff(buffId, -2);
                        restored++;
                    }

                    continue;
                }

                LogBuffRestoreSkip(buffId, skipReason);

                if (RemoveStaleSavedBuff(buffId))
                {
                    _monitor.Log(
                        $"[BuffRestore] removed stale saved buff: {buffId}",
                        LogLevel.Debug);
                }
            }

            _stateManager.Save();
            _monitor.Log(
                $"[BuffRestore] restored {restored} buff(s), snapshot now has {_stateManager.State.SavedActiveBuffs.Count} entries",
                LogLevel.Debug);
        }

        private bool ShouldRestoreSavedBuff(string buffId, out string? skipReason)
        {
            skipReason = null;

            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                return ShouldRestoreComplicationBuff(buffId, out skipReason);

            if (IsMainInjuryBaseBuff(buffId))
                return ShouldRestoreMainInjuryBuff(buffId, out skipReason);

            return true;
        }

        private static bool IsMainInjuryBaseBuff(string buffId) =>
            InjurySets.HarveyTreatable.Contains(buffId)
            && !InjurySets.KnownComplicationBuffIds.Contains(buffId);

        private bool ShouldRestoreMainInjuryBuff(string buffId, out string? skipReason)
        {
            skipReason = null;
            string? mainInjuryId = _stateManager.GetMainInjuryId();

            if (string.IsNullOrEmpty(mainInjuryId)
                || !string.Equals(buffId, mainInjuryId, StringComparison.OrdinalIgnoreCase)
                || !_stateManager.HasDebuffState(buffId))
            {
                skipReason = "main injury mismatch or missing DebuffState";
                return false;
            }

            return true;
        }

        private bool ShouldRestoreComplicationBuff(string buffId, out string? skipReason)
        {
            skipReason = null;

            if (!_stateManager.State.ActiveComplications.ContainsKey(buffId))
            {
                skipReason = "not in ActiveComplications";
                return false;
            }

            if (string.Equals(buffId, InjuryBuffs.WetBandage, StringComparison.OrdinalIgnoreCase)
                && !_complicationManager.IsWetBandageComplicationValid())
            {
                skipReason = "no active bandage/treatment";
                return false;
            }

            return true;
        }

        private void LogBuffRestoreSkip(string buffId, string? skipReason)
        {
            if (string.IsNullOrEmpty(skipReason))
                return;

            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
            {
                _monitor.Log(
                    $"[BuffRestore] skip invalid complication buff: {buffId}, reason={skipReason}",
                    LogLevel.Debug);
                return;
            }

            if (IsMainInjuryBaseBuff(buffId))
            {
                _monitor.Log(
                    $"[BuffRestore] skip invalid main injury buff: {buffId}, reason={skipReason}",
                    LogLevel.Debug);
            }
        }

        private bool RemoveStaleSavedBuff(string buffId)
        {
            bool removedFromSaved = _stateManager.State.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, buffId, StringComparison.OrdinalIgnoreCase)) > 0;

            if (!removedFromSaved)
                return false;

            _buffManager.RemoveBuff(buffId);

            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                RemoveStaleComplicationState(buffId);

            return true;
        }

        private void RemoveStaleComplicationState(string complicationId)
        {
            _stateManager.State.ActiveComplications.Remove(complicationId);

            if (_stateManager.HasDebuffState(complicationId))
                _stateManager.RemoveDebuffState(complicationId);

            _dialogueManager.RemoveTopic(TopicIds.GetComplicationTopic(complicationId));
        }

        /// <summary>
        /// Проверяет, истёк ли срок нефазового лечения (buffHurt, buffBadlyHurt, buffSurgicalWound).
        /// Срок хранится в DebuffState.PhaseStartDay + Phase1Duration.
        /// </summary>
        private void CheckSimpleTreatmentCompletion()
        {
            int today = GetToday();
            string? mainInjuryId = _stateManager.GetMainInjuryId();

            foreach (var (buffId, ds) in _stateManager.State.ActiveDebuffs.ToList())
            {
                if (WouldHaveLegacySimpleCompletion(ds, today))
                {
                    bool isComplication = _stateManager.State.ActiveComplications.ContainsKey(buffId)
                        || InjurySets.KnownComplicationBuffIds.Contains(buffId);
                    bool isSimpleMain = SimpleInjuryCures.Map.ContainsKey(buffId)
                        && string.Equals(buffId, mainInjuryId, StringComparison.OrdinalIgnoreCase);

                    if (isComplication || !isSimpleMain)
                    {
                        _monitor.Log(
                            $"[SimpleTreatment] skip non-main/non-simple: {buffId}",
                            LogLevel.Debug);
                    }
                }

                if (!SimpleInjuryCures.Map.ContainsKey(buffId))
                    continue;

                if (!string.Equals(buffId, mainInjuryId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (_stateManager.State.ActiveComplications.ContainsKey(buffId)
                    || InjurySets.KnownComplicationBuffIds.Contains(buffId))
                    continue;

                if (ds.IsPhasedInjury || !ds.TreatmentStarted)
                    continue;

                if (!SimpleInjuryCures.Map.TryGetValue(buffId, out var cureBuff)
                    || !_buffManager.HasBuff(cureBuff))
                    continue;

                int daysInTreatment = today - ds.PhaseStartDay;
                if (daysInTreatment < ds.Phase1Duration)
                    continue;

                _monitor.Log($"Нефазовое лечение завершено: {buffId} (прошло {daysInTreatment} из {ds.Phase1Duration} дней)", LogLevel.Info);

                _buffManager.RemoveBuff(cureBuff);

                if (string.Equals(buffId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                    _buffManager.RemoveBuff(CureBuffs.BadlyHurtOutpatientCare);

                _stateManager.CompleteMainInjury(buffId);

                // Удаляем состояние травмы
                _stateManager.RemoveDebuffState(buffId);
                _injuryManager.NotifyInjuryRecovered(buffId);

                // Добавляем топик завершения для диалога
                string curedTopic = TopicIds.GetCuredTopic(buffId);
                _dialogueManager.AddTopic(curedTopic, 7);

                Game1.addHUDMessage(new HUDMessage("Лечение завершено! Обратись к Харви для финального осмотра.", HUDMessage.health_type));
            }
        }

        private static bool WouldHaveLegacySimpleCompletion(DebuffState ds, int today) =>
            !ds.IsPhasedInjury
            && ds.TreatmentStarted
            && today - ds.PhaseStartDay >= ds.Phase1Duration;
    }
}

