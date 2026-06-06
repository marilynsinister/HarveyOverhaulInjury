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
        private readonly CareTrustManager _careTrustManager;
        private readonly CheckupManager _checkupManager;
        private readonly RehabManager _rehabManager;
        private readonly RecoveryPlanManager _recoveryPlanManager;
        private readonly SelfCareManager _selfCareManager;
        private readonly DoctorVisitReminderManager _doctorVisitReminderManager;
        private InteractionHandler? _interactionHandler;
        private PassOutHandler? _passOutHandler;
        private HarveyHomeCareEventLauncher? _homeCareLauncher;

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
            CareTrustManager careTrustManager,
            CheckupManager checkupManager,
            RehabManager rehabManager,
            RecoveryPlanManager recoveryPlanManager,
            SelfCareManager selfCareManager,
            DoctorVisitReminderManager doctorVisitReminderManager)
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
            _careTrustManager = careTrustManager;
            _checkupManager = checkupManager;
            _rehabManager = rehabManager;
            _recoveryPlanManager = recoveryPlanManager;
            _selfCareManager = selfCareManager;
            _doctorVisitReminderManager = doctorVisitReminderManager;
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

        public void SetHomeCareLauncher(HarveyHomeCareEventLauncher homeCareLauncher)
        {
            _homeCareLauncher = homeCareLauncher;
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
                        // 0. Orphan MineForbidden / Hospitalized не должны восстанавливаться из снапшота
                        SanitizeOrphanMineForbiddenBuff();
                        _hospitalizationManager.SanitizeOrphanHospitalizedBuff();

                        // 1. Восстанавливаем баффы из снапшота конца прошлого дня
                        RestoreBuffsFromSnapshot();

                        // 1a. Синхронизация фазовых/лечебных баффов по DebuffState (Stardew снимает баффы на ночь)
                        int resynced = _injuryManager.EnsureActiveTreatmentBuffs();
                        if (resynced > 0)
                        {
                            _monitor.Log(
                                $"[BuffSync] Восстановлено {resynced} лечебных бафф(ов) по ActiveDebuffs",
                                LogLevel.Info);
                        }

                        _hospitalizationManager.RestoreHospitalizedBuffIfActive();

                        _complicationManager.CleanupInvalidComplications();

                        // 1b. Сброс некорректного ReadyForNextPhase у простого лечения (TotalPhases == 0)
                        _stateManager.SanitizeNonPhasedReadyFlags();

                        // 2. Письмо и жёсткий запрет — на следующий день после предупреждения в шахте
                        ApplyMineForbiddenIfWarningWasYesterday();

                        // 3. Истечение hard-ban, переход на MineRestricted, синхронизация баффов
                        ProcessMineAccessDayStarted();

                        // 4. Запуск события спасения из шахты — ПОСЛЕ восстановления баффов,
                        //    чтобы HasAnyBuff(Severe) корректно видел все активные травмы
                        _passOutHandler?.TriggerMineRescueEvents();

                        // 4b. Утреннее мини-событие после истощения — после пробуждения, когда мир готов
                        Game1.delayedActions.Add(new StardewValley.DelayedAction(1000, () =>
                        {
                            _homeCareLauncher?.TryTriggerHarveyHomeCareEvent(source: "DayStartedDelayed");
                        }));

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

                        _doctorVisitReminderManager.SyncReminderBuff();
                        _careTrustManager.SyncCareTrustTopic();

                        _checkupManager.ProcessMissedCheckupsDaily(GetToday());
                        _complianceManager.TryShowLowComplianceReminder();

                        _rehabManager.CompleteRehabIfDue(GetToday());
                        _recoveryPlanManager.OnDayStarted();

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
        /// QA/MCP: run morning checks synchronously (infection roll, buff restore) without waiting for DayStarted delay.
        /// Use after StardewMCP advance_day when DayEnding snapshot may be missing.
        /// </summary>
        public string RunQaDailyChecks(bool runPhaseChecks = true)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            SanitizeOrphanMineForbiddenBuff();
            _hospitalizationManager.SanitizeOrphanHospitalizedBuff();

            if (_stateManager.State.SavedActiveBuffs.Count == 0)
            {
                _stateManager.State.SavedActiveBuffs = BuildSavedActiveBuffSnapshot();
                _monitor.Log(
                    $"[QA] SavedActiveBuffs refreshed ({_stateManager.State.SavedActiveBuffs.Count} entries)",
                    LogLevel.Debug);
            }

            RestoreBuffsFromSnapshot();
            _injuryManager.EnsureActiveTreatmentBuffs();
            _hospitalizationManager.RestoreHospitalizedBuffIfActive();
            _stateManager.SanitizeNonPhasedReadyFlags();

            ApplyMineForbiddenIfWarningWasYesterday();
            ProcessMineAccessDayStarted();
            _passOutHandler?.TriggerMineRescueEvents();

            bool infectionEscalated = _complicationManager.CheckTreatmentCompletion();

            if (!infectionEscalated && runPhaseChecks)
            {
                CheckSimpleTreatmentCompletion();
                CheckInjuryPhases();
            }

            RunQaCheckNeglect();

            _stateManager.State.SavedActiveBuffs = BuildSavedActiveBuffSnapshot();
            _stateManager.Save();

            return
                $"[QA] RunQaDailyChecks infectionEscalated={infectionEscalated} " +
                $"MainInjuryId={_stateManager.GetMainInjuryId() ?? "(none)"} " +
                $"Complications={string.Join(", ", _stateManager.State.ActiveComplications.Keys)} " +
                $"Neglect={_buffManager.HasBuff(InjuryBuffs.Neglect)} " +
                $"MineWarningDay={_stateManager.State.MineWarningDay} " +
                $"MineForbidden={_buffManager.HasBuff(InjuryBuffs.MineForbidden)}";
        }

        /// <summary>QA/MCP: DayEnding neglect strikes without sleeping.</summary>
        public void RunQaCheckNeglect() => CheckNeglect();

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
                _recoveryPlanManager.OnDayEnding();

                // Письмо о запрете шахты — на следующий день после предупреждения в шахте
                int todayEnd = GetToday();
                if (_stateManager.State.MineWarningDay == todayEnd && _config.SendLetters)
                {
                    HarveyMailHelper.TryScheduleTieredMail(_config, _stateManager, _monitor, MailIds.MineForbidden);
                    _monitor.Log($"[Шахта] Письмо о запрете шахты запланировано на завтра (день предупреждения: {todayEnd})", LogLevel.Debug);
                }

                // Сохраняем снапшот активных баффов мода на момент конца дня
                _stateManager.State.SavedActiveBuffs = BuildSavedActiveBuffSnapshot();
                _monitor.Log($"Снапшот баффов ({_stateManager.State.SavedActiveBuffs.Count}): {string.Join(", ", _stateManager.State.SavedActiveBuffs)}", LogLevel.Debug);

                _careTrustManager.RewardMineBanObeyedIfEligible();

                bool hasSevereInjury = InjurySets.Severe.Any(id => _injuryManager.HasInjuryOrPhase(id));
                _careTrustManager.RewardEarlySleepIfEligible(hasSevereInjury);

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

            _injuryManager.EnsureActiveTreatmentBuffs();
            
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

                            if (_injuryManager.HasExpectedTreatmentBuff(injuryId))
                                ShowPhaseTransitionReminder(injuryId, nextPhase);
                            else
                                _monitor.Log(
                                    $"⚠️ {injuryId}: ReadyForNextPhase выставлен, но бафф фазы {debuffState.CurrentPhase} отсутствует",
                                    LogLevel.Warn);
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
                            _dialogueManager.RemoveTopic(TopicIds.GetCuredTopic(injuryId));
                            _monitor.Log($"🎉 Установлен флаг готовности к выздоровлению: {injuryId}", LogLevel.Info);

                            if (_injuryManager.HasExpectedTreatmentBuff(injuryId))
                                ShowRecoveryReminder(injuryId);
                            else
                                _monitor.Log(
                                    $"⚠️ {injuryId}: ReadyForRecovery выставлен, но ожидаемый бафф отсутствует",
                                    LogLevel.Warn);
                        }
                    }
                }
            }
        }

        private void ShowPhaseTransitionReminder(string injuryId, int nextPhase)
        {
            string injuryName = _injuryManager.GetInjuryName(injuryId);
            var debuffState = _stateManager.GetDebuffState(injuryId);
            int totalPhases = debuffState?.TotalPhases ?? InjurySets.InferDefaultTotalPhases(injuryId);
            string phaseName = TopicIds.GetPhaseStageName(nextPhase, totalPhases) switch
            {
                "Recovery" => "восстановления",
                "Healing" => "заживления",
                "Acute" => "острой фазы",
                _ => "следующей стадии"
            };
            
            Game1.addHUDMessage(new HUDMessage(
                $"Лечение готово к стадии {phaseName}. Посети Харви для продолжения.",
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
            _complicationManager.TryApplyNeglectComplication(
                hudMessage: new HUDMessage("Ты запустила лечение...", HUDMessage.error_type));
            _careTrustManager.PenalizeTrust(1, "neglect_treatment");

            // Если Харви рядом — короткая реплика о пропуске лечения (без снижения Friendship)
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey != null && Game1.currentLocation?.characters.Contains(harvey) == true)
            {
                _dialogueManager.ShowEmoteWithText(harvey, 
                    HarveyEmotes.NeglectedCare,
                    HarveyTextMessages.NotTreating);
            }
        }

        private int GetToday() => (int)Game1.stats.DaysPlayed;

        /// <summary>На следующий день после предупреждения: жёсткий запрет на MineForbiddenDurationDays.</summary>
        private void ApplyMineForbiddenIfWarningWasYesterday()
        {
            int today = GetToday();
            if (_stateManager.State.MineWarningDay != today - 1)
                return;

            _stateManager.State.MineWarningDay = -1;
            MineForbiddenHelper.ApplyHardMineForbidden(
                _stateManager.State,
                _config,
                _buffManager,
                _stateManager,
                _monitor,
                today,
                "warning_yesterday");

            int daysLeft = MineForbiddenHelper.GetMineForbiddenDaysLeft(_stateManager.State, _config, today);
            Game1.addHUDMessage(new HUDMessage(
                MineForbiddenHelper.FormatAppliedHud(_config, today, _stateManager.State),
                HUDMessage.health_type));
            _monitor.Log(
                $"[Шахта] Жёсткий запрет после предупреждения: осталось {daysLeft} дн.",
                LogLevel.Info);
        }

        private void ProcessMineAccessDayStarted()
        {
            MineForbiddenHelper.ProcessDayStarted(
                _stateManager.State,
                _config,
                _injuryManager,
                _buffManager,
                _stateManager,
                _monitor,
                GetToday());
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

            if (string.Equals(buffId, StatusBuffs.Hospitalized, StringComparison.OrdinalIgnoreCase))
            {
                skipReason = "hospitalized buff synced from IsHospitalized state";
                return false;
            }

            if (_injuryManager.ShouldSkipSnapshotRestoreForBuff(buffId, out string? treatmentReason))
            {
                skipReason = treatmentReason;
                return false;
            }

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

            if (string.Equals(buffId, InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase)
                && !_complicationManager.IsPainFlareComplicationValid())
            {
                skipReason = "main not pain-sensitive";
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

        /// <summary>
        /// Снапшот только валидных mod-бaffов (без orphan MineForbidden и осложнений вне ActiveComplications).
        /// </summary>
        private List<string> BuildSavedActiveBuffSnapshot()
        {
            SanitizeOrphanMineForbiddenBuff();

            var snapshot = new List<string>();
            foreach (string buffId in _buffManager.GetActiveModBuffs())
            {
                if (ShouldIncludeInSavedSnapshot(buffId))
                    snapshot.Add(buffId);
            }

            return snapshot;
        }

        private bool ShouldIncludeInSavedSnapshot(string buffId)
        {
            if (string.Equals(buffId, InjuryBuffs.MineForbidden, StringComparison.OrdinalIgnoreCase))
                return MineForbiddenHelper.IsMineForbiddenActive(_stateManager.State, _config, GetToday());

            if (string.Equals(buffId, InjuryBuffs.MineRestricted, StringComparison.OrdinalIgnoreCase))
                return MineForbiddenHelper.ShouldMineRestricted(
                    _stateManager.State, _config, _injuryManager, _buffManager, GetToday());

            if (string.Equals(buffId, StatusBuffs.Hospitalized, StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.Equals(buffId, ReminderBuffs.DoctorVisitNeeded, StringComparison.OrdinalIgnoreCase))
                return false;

            if (InjurySets.KnownComplicationBuffIds.Contains(buffId)
                && !_stateManager.State.ActiveComplications.ContainsKey(buffId))
                return false;

            return true;
        }

        private void SanitizeOrphanMineForbiddenBuff()
        {
            int today = GetToday();
            MineForbiddenHelper.SyncMineForbiddenBuff(
                _stateManager.State, _config, _buffManager, _stateManager, _monitor, today, "SanitizeOrphan");
            MineForbiddenHelper.SyncMineRestrictedBuff(
                _stateManager.State, _config, _injuryManager, _buffManager, _stateManager, _monitor, today, "SanitizeOrphan");
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

                int daysInTreatment = today - ds.PhaseStartDay;
                if (daysInTreatment < ds.Phase1Duration)
                    continue;

                if (ds.ReadyForRecovery)
                    continue;

                _monitor.Log(
                    $"Нефазовое лечение готово к финальному осмотру: {buffId} (прошло {daysInTreatment} из {ds.Phase1Duration} дней)",
                    LogLevel.Info);

                _checkupManager.OnRecoveryCheckupDue(buffId, ds, today);
                _stateManager.SetReadyForRecovery(buffId, true);

                // Старый автозавершение вешал topic*Cured — убираем, чтобы не дублировать финальный диалог
                _dialogueManager.RemoveTopic(TopicIds.GetCuredTopic(buffId));

                _injuryManager.EnsureTreatmentBuffForInjury(buffId);
                if (_injuryManager.HasExpectedTreatmentBuff(buffId))
                    ShowRecoveryReminder(buffId);
                else
                    _monitor.Log(
                        $"⚠️ {buffId}: ReadyForRecovery выставлен, но лечебный бафф отсутствует",
                        LogLevel.Warn);
            }
        }

        private static bool WouldHaveLegacySimpleCompletion(DebuffState ds, int today) =>
            !ds.IsPhasedInjury
            && ds.TreatmentStarted
            && today - ds.PhaseStartDay >= ds.Phase1Duration;
    }
}

