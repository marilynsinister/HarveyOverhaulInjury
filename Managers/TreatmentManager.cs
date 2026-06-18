using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление лечением травм
    /// </summary>
    public class TreatmentManager
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;
        private readonly StateManager _stateManager;
        private readonly PrescriptionManager _prescriptionManager;
        private readonly ComplianceManager _complianceManager;
        private readonly CheckupManager _checkupManager;
        private readonly TreatmentPlanManager _treatmentPlanManager;
        private ComplicationManager? _complicationManager;
        private DoctorVisitReminderManager? _doctorVisitReminderManager;

        /// <summary>Лечебные/служебные баффы, снимаемые при полном выздоровлении (не buffHarveyCare).</summary>
        private static readonly string[] RecoveryCleanupBuffIds =
        {
            CureBuffs.Treatment,
            CureBuffs.IntensiveCare,
            CureBuffs.BadlyHurtOutpatientCare,
            CureBuffs.PostSurgical,
            CureBuffs.Protection,
            CureBuffs.Recovery,
            CureBuffs.Teracitin,
            CureBuffs.Antibiotics,
            CureBuffs.ForcedSedation,
        };

        // Маппинг травм к лечебным баффам (только для простых травм БЕЗ фаз)
        public static readonly Dictionary<string, string> CureByInjury = new()
        {
            { "buffHurt", CureBuffs.Treatment },
            { "buffBadlyHurt", CureBuffs.IntensiveCare },
            { "buffSurgicalWound", CureBuffs.PostSurgical }
        };

        /// <summary>
        /// Простое (нефазовое) лечение: buffHurt, buffBadlyHurt, buffSurgicalWound.
        /// Для них не используются injury_phase_ready / injury_phase_advance.
        /// </summary>
        public static bool IsSimpleTreatmentInjury(string injuryId) =>
            CureByInjury.ContainsKey(injuryId);
        
        // Травмы с фазовой системой (используют свои фазовые баффы травм)
        public static readonly HashSet<string> PhasedInjuries = new()
        {
            "buffConcussion",
            "buffFracturedBone",
            "buffTornMuscles",
            "buffSprainedAnkle",
            "buffBruisedRibs",
            "buffDeepCuts",
            "buffBurnWounds",
            "buffInfectedWound",
            "buffBackStrain",
            "buffShrapnelWounds",
            "buffCold" // Простуда (2 фазы: острая + восстановление)
        };

        public TreatmentManager(
            IMonitor monitor,
            ModConfig config,
            BuffManager buffManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager,
            StateManager stateManager,
            PrescriptionManager prescriptionManager,
            ComplianceManager complianceManager,
            CheckupManager checkupManager,
            TreatmentPlanManager treatmentPlanManager)
        {
            _monitor = monitor;
            _config = config;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
            _stateManager = stateManager;
            _prescriptionManager = prescriptionManager;
            _complianceManager = complianceManager;
            _checkupManager = checkupManager;
            _treatmentPlanManager = treatmentPlanManager;
        }

        public void SetComplicationManager(ComplicationManager complicationManager) =>
            _complicationManager = complicationManager;

        public void SetDoctorVisitReminderManager(DoctorVisitReminderManager doctorVisitReminderManager) =>
            _doctorVisitReminderManager = doctorVisitReminderManager;

        /// <summary>
        /// Получить ID фазового баффа травмы (используется для восстановления)
        /// </summary>
        public string GetInjuryPhaseBuffId(string injuryId, int phase)
        {
            // Используем существующие методы InjuryManager
            return _injuryManager.GetPhaseBuffId(injuryId, phase);
        }
        
        /// <summary>
        /// Проверить готовность травмы к смене фазы
        /// </summary>
        public bool IsInjuryReadyForNextPhase(string injuryId, out int currentPhase, out int nextPhase)
        {
            currentPhase = 0;
            nextPhase = 0;

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
                return false;

            if (debuffState.TotalPhases <= 0)
                return false;

            if (!debuffState.IsInTreatment)
                return false;

            if (debuffState.CurrentPhase <= 0)
                return false;

            if (debuffState.CurrentPhase >= debuffState.TotalPhases)
                return false;

            currentPhase = debuffState.CurrentPhase;
            nextPhase = currentPhase + 1;

            return debuffState.ReadyForNextPhase;
        }
        
        /// <summary>
        /// Проверить готовность травмы к полному выздоровлению
        /// </summary>
        public bool IsInjuryReadyForRecovery(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                return false;
            }
            
            // Проверяем флаг готовности из DebuffState
            return debuffState.ReadyForRecovery;
        }
        
        /// <summary>
        /// Сменить фазу травмы (вызывается при разговоре с Харви)
        /// </summary>
        public void AdvanceInjuryToNextPhase(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} не найдено", LogLevel.Warn);
                return;
            }

            if (debuffState.TotalPhases <= 0 || debuffState.CurrentPhase >= debuffState.TotalPhases)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: смена фазы невозможна (TotalPhases={debuffState.TotalPhases}, CurrentPhase={debuffState.CurrentPhase}). " +
                    "Для простого лечения используйте injury_phase_recovery или injury_phase_cure.",
                    LogLevel.Warn);
                return;
            }

            if (debuffState.CurrentPhase <= 0)
            {
                _monitor.Log($"⚠️ {injuryId}: лечение не начато (CurrentPhase={debuffState.CurrentPhase}), смена фазы пропущена", LogLevel.Warn);
                return;
            }

            if (!debuffState.ReadyForNextPhase)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: ReadyForNextPhase=false (фаза {debuffState.CurrentPhase}/{debuffState.TotalPhases}), смена фазы пропущена",
                    LogLevel.Warn);
                return;
            }

            int currentDay = (int)StardewValley.Game1.stats.DaysPlayed;
            _checkupManager.CompleteCheckup(injuryId, debuffState, currentDay);

            int oldPhase = debuffState.CurrentPhase;
            int newPhase = oldPhase + 1;

            if (newPhase > debuffState.TotalPhases)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: смена фазы {oldPhase}→{newPhase} выходит за TotalPhases={debuffState.TotalPhases}",
                    LogLevel.Warn);
                return;
            }

            _monitor.Log($"🔄 Смена фазы {injuryId}: {oldPhase} → {newPhase}", LogLevel.Info);

            string oldPhaseBuffId = _injuryManager.GetPhaseBuffId(injuryId, oldPhase);
            string newPhaseBuffId = _injuryManager.GetPhaseBuffId(injuryId, newPhase);
            _monitor.Log($"🔄 Баффы фазы {injuryId}: {oldPhaseBuffId} → {newPhaseBuffId}", LogLevel.Info);

            if (!_buffManager.BuffExists(newPhaseBuffId))
            {
                _monitor.Log(
                    $"[PhaseBuffMissing] injury={injuryId} phase={newPhase} expected={newPhaseBuffId} BuffExists=false",
                    LogLevel.Error);
                return;
            }

            _buffManager.AddBuff(newPhaseBuffId, -2);
            if (!_buffManager.HasBuff(newPhaseBuffId))
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: не удалось наложить {newPhaseBuffId} — фаза {oldPhase} сохранена",
                    LogLevel.Error);
                return;
            }

            _monitor.Log($"✅ Применён бафф фазы {newPhase}: {newPhaseBuffId}", LogLevel.Info);

            if (_buffManager.HasBuff(oldPhaseBuffId))
            {
                _buffManager.RemoveBuff(oldPhaseBuffId);
                _monitor.Log($"❌ Удалён бафф фазы {oldPhase}: {oldPhaseBuffId}", LogLevel.Debug);
            }

            _stateManager.AdvancePhase(injuryId, currentDay);

            var updatedState = _stateManager.GetDebuffState(injuryId);
            if (updatedState == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} исчезло после AdvancePhase", LogLevel.Warn);
                _injuryManager.EnsureTreatmentBuffForInjury(injuryId);
                return;
            }

            int actualNewPhase = updatedState.CurrentPhase;
            if (actualNewPhase <= oldPhase)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: AdvancePhase не изменил фазу ({oldPhase} → {actualNewPhase})",
                    LogLevel.Warn);
                _injuryManager.EnsureTreatmentBuffForInjury(injuryId);
                return;
            }

            if (updatedState.ReadyForNextPhase)
                _stateManager.SetReadyForNextPhase(injuryId, false);

            if (updatedState.ReadyForRecovery)
                _stateManager.SetReadyForRecovery(injuryId, false);

            string oldPhaseTopicId = _injuryManager.GetPhaseTopicId(injuryId, oldPhase);
            _dialogueManager.RemoveTopic(oldPhaseTopicId);
            int topicDays = Math.Max(1, updatedState.GetCurrentPhaseDuration());
            string newPhaseTopicId = _injuryManager.GetPhaseTopicId(injuryId, actualNewPhase);
            _dialogueManager.AddTopic(newPhaseTopicId, topicDays);
            _monitor.Log($"💬 Phase topic {oldPhaseTopicId} → {newPhaseTopicId} ({topicDays} дн.)", LogLevel.Debug);

            _injuryManager.EnsureTreatmentBuffForInjury(injuryId);
            if (_injuryManager.HasExpectedTreatmentBuff(injuryId))
            {
                string phaseName = TopicIds.GetPhaseStageName(
                    actualNewPhase,
                    updatedState.TotalPhases) switch
                {
                    "Acute" => "Острая фаза",
                    "Healing" => "Заживление",
                    "Recovery" => "Восстановление",
                    _ => $"Фаза {actualNewPhase}"
                };
                StardewValley.Game1.addHUDMessage(new StardewValley.HUDMessage(
                    $"Переход к фазе: {phaseName}",
                    StardewValley.HUDMessage.health_type));
            }
            else
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: фаза {actualNewPhase} в состоянии, но ожидаемый бафф отсутствует после sync",
                    LogLevel.Error);
            }

            _complianceManager.ApplyTreatmentComplianceTopics();
        }
        
        /// <summary>
        /// Механическое завершение травмы без диалога (канон для игрового клика и debug-cure).
        /// </summary>
        public void ApplyMechanicalPhasedRecovery(string injuryId, int careDurationMs = 2880000)
        {
            ApplyFullRecoveryCleanup(injuryId);

            _buffManager.AddBuff(CureBuffs.Care, careDurationMs);
            _complianceManager.ApplyHighComplianceRecoveryBonuses();
            _doctorVisitReminderManager?.SyncReminderBuff();
            _monitor.Log($"Механическое выздоровление применено: {injuryId}, Care={careDurationMs}ms", LogLevel.Debug);
        }

        /// <summary>
        /// Централизованная финальная очистка после полного выздоровления от травмы.
        /// </summary>
        public void ApplyFullRecoveryCleanup(string injuryId)
        {
            _injuryManager.RemoveAllPhaseBuffs(injuryId);
            RemoveRecoveryTreatmentBuffs(injuryId);

            _stateManager.CompleteMainInjury(injuryId);
            _stateManager.RemoveDebuffState(injuryId);
            _injuryManager.NotifyInjuryRecovered(injuryId);

            RemoveRecoveryTopics(injuryId);
            _complicationManager?.CleanupWetBandageIfNoBandagedInjuries();

            if (_stateManager.State.TimeUnderRainTicks != 0)
                _stateManager.State.TimeUnderRainTicks = 0;

            _stateManager.Save();
        }

        /// <summary>
        /// Безопасная очистка зависших медицинских хвостов без полного injury_reset.
        /// </summary>
        public int CleanupLingeringMedicalState()
        {
            int cleaned = 0;

            foreach (string buffId in RecoveryCleanupBuffIds)
            {
                if (!_buffManager.HasBuff(buffId))
                    continue;

                if (HasActiveTreatmentDebuffForBuff(buffId))
                    continue;

                if (RemoveLingeringBuff(buffId))
                    cleaned++;
            }

            if (_buffManager.HasBuff(ReminderBuffs.DoctorVisitNeeded)
                && _doctorVisitReminderManager?.IsVisitNeeded() == false)
            {
                if (RemoveLingeringBuff(ReminderBuffs.DoctorVisitNeeded))
                    cleaned++;
            }

            _complicationManager?.CleanupWetBandageIfNoBandagedInjuries();

            foreach (string buffId in InjurySets.HarveyTreatable)
            {
                if (_stateManager.HasDebuffState(buffId))
                    continue;

                cleaned += RemoveOrphanRecoveryTopics(buffId);
            }

            if (_stateManager.State.TimeUnderRainTicks != 0
                && _complicationManager?.HasAnyActiveBandagedInjuryInTreatment() == false)
            {
                _stateManager.State.TimeUnderRainTicks = 0;
                cleaned++;
            }

            if (cleaned > 0)
                _stateManager.Save();

            _doctorVisitReminderManager?.SyncReminderBuff();
            return cleaned;
        }

        private void RemoveRecoveryTreatmentBuffs(string injuryId)
        {
            foreach (string buffId in RecoveryCleanupBuffIds)
                RemoveLingeringBuff(buffId);

            if (!IsSimpleTreatmentInjury(injuryId))
                return;

            string? cureBuffId = CureByInjury.GetValueOrDefault(injuryId);
            if (cureBuffId != null)
                RemoveLingeringBuff(cureBuffId);
        }

        private void RemoveRecoveryTopics(string injuryId)
        {
            _dialogueManager.ClearUntreatedInjuryTopic(injuryId, "механическое выздоровление");

            RemoveLingeringTopic(TopicIds.GetInjuryTopic(injuryId));
            RemoveLingeringTopic(TopicIds.GetTreatmentTopic(injuryId));
            RemoveLingeringTopic(TopicIds.GetCuredTopic(injuryId));
            RemoveLingeringTopic(TreatmentPlanTopics.GetInjuryTopic(injuryId));

            int totalPhases = InjurySets.InferDefaultTotalPhases(injuryId);
            for (int phase = 1; phase <= 3; phase++)
                RemoveLingeringTopic(_injuryManager.GetPhaseTopicId(injuryId, phase));

            if (totalPhases == 2)
            {
                string injuryName = injuryId.Replace("buff", "", StringComparison.OrdinalIgnoreCase);
                RemoveLingeringTopic($"topic{injuryName}PhaseHealing");
            }

            _checkupManager.RemoveAllCheckupTopicsForInjury(injuryId, totalPhases);
        }

        private int RemoveOrphanRecoveryTopics(string injuryId)
        {
            int removed = 0;
            string[] orphanCandidates =
            {
                TopicIds.GetInjuryTopic(injuryId),
                TopicIds.GetTreatmentTopic(injuryId),
                TopicIds.GetCuredTopic(injuryId),
                TreatmentPlanTopics.GetInjuryTopic(injuryId),
            };

            foreach (string topicId in orphanCandidates)
            {
                if (RemoveLingeringTopic(topicId))
                    removed++;
            }

            for (int phase = 1; phase <= 3; phase++)
            {
                if (RemoveLingeringTopic(_injuryManager.GetPhaseTopicId(injuryId, phase)))
                    removed++;
            }

            return removed;
        }

        private bool HasActiveTreatmentDebuffForBuff(string buffId)
        {
            foreach (var (injuryId, debuffState) in _stateManager.State.ActiveDebuffs)
            {
                if (!debuffState.TreatmentStarted)
                    continue;

                if (string.Equals(buffId, CureBuffs.Treatment, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(injuryId, "buffHurt", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (string.Equals(buffId, CureBuffs.IntensiveCare, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (string.Equals(buffId, CureBuffs.PostSurgical, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(injuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (string.Equals(buffId, CureBuffs.BadlyHurtOutpatientCare, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private bool RemoveLingeringBuff(string buffId)
        {
            if (!_buffManager.HasBuff(buffId))
                return false;

            _buffManager.RemoveBuff(buffId);
            _stateManager.State.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, buffId, StringComparison.OrdinalIgnoreCase));
            _monitor.Log($"[RecoveryCleanup] Removed lingering buff {buffId}", LogLevel.Info);
            return true;
        }

        private bool RemoveLingeringTopic(string topicId)
        {
            if (!_dialogueManager.HasTopic(topicId))
                return false;

            _dialogueManager.RemoveTopic(topicId);
            _monitor.Log($"[RecoveryCleanup] Removed lingering topic {topicId}", LogLevel.Info);
            return true;
        }

        /// <summary>
        /// Завершить лечение травмы (debug-команда injury_phase_cure).
        /// </summary>
        public void CompleteInjuryRecovery(string injuryId)
        {
            if (_stateManager.GetDebuffState(injuryId) == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} не найдено", LogLevel.Warn);
                return;
            }

            _monitor.Log($"🎉 Debug-завершение лечения {injuryId}", LogLevel.Info);
            ApplyMechanicalPhasedRecovery(injuryId, careDurationMs: 28800000);
            _dialogueManager.AddTopic(ConversationTopics.TreatmentCompleted, 7);

            StardewValley.Game1.addHUDMessage(new StardewValley.HUDMessage(
                "🎉 Лечение завершено! Ты полностью здорова!",
                StardewValley.HUDMessage.achievement_type));
        }
        
        public static string GetPhaseDisplayName(string injuryId, int phase, int totalPhases = -1)
        {
            if (totalPhases < 0)
                totalPhases = InjurySets.InferDefaultTotalPhases(injuryId);

            return TopicIds.GetPhaseStageName(phase, totalPhases) switch
            {
                "Acute" => "Острая фаза",
                "Healing" => "Заживление",
                "Recovery" => "Восстановление",
                _ => $"Фаза {phase}"
            };
        }

        /// <summary>
        /// Применить лечение для конкретной травмы
        /// </summary>
        public bool ApplyTreatmentForInjury(string injuryId)
        {
            _monitor.Log($"Применяем лечение для {injuryId}", LogLevel.Info);

            bool treatmentStarted = false;

            if (PhasedInjuries.Contains(injuryId))
            {
                treatmentStarted = StartPhasedTreatment(injuryId);
            }
            else if (IsSimpleTreatmentInjury(injuryId))
            {
                treatmentStarted = StartSimpleTreatment(injuryId);
            }
            else
            {
                _monitor.Log($"Лечебный бафф для {injuryId} не найден", LogLevel.Warn);
            }

            if (treatmentStarted)
            {
                _dialogueManager.ClearUntreatedInjuryTopic(injuryId, "лечение начато");
                _dialogueManager.ClearTreatmentNeededTopic(injuryId, "лечение начато");
                var dsAfter = _stateManager.GetDebuffState(injuryId);
                if (dsAfter != null)
                    dsAfter.TreatmentApplied = true;
                _injuryManager.EnsureTreatmentBuffForInjury(injuryId);
                _prescriptionManager.AssignPrescriptionsForInjury(injuryId);
                _complianceManager.ApplyTreatmentComplianceTopics();
                _treatmentPlanManager.SendTreatmentPlanForInjury(injuryId);

                int today = (int)Game1.stats.DaysPlayed;
                var ds = _stateManager.GetDebuffState(injuryId);
                int phase = ds?.CurrentPhase ?? 1;
                if (MineForbiddenHelper.ShouldHardBanOnTreatmentStart(injuryId, phase))
                {
                    MineForbiddenHelper.ApplyHardMineForbidden(
                        _stateManager.State,
                        _config,
                        _buffManager,
                        _stateManager,
                        _monitor,
                        today,
                        "treatment_acute_start");
                }
            }

            return treatmentStarted;
        }

        /// <summary>Снять topic нелеченной травмы для всех HarveyTreatable (фазовые и простые).</summary>
        public void ClearUntreatedInjuryTopic(string injuryId, string reason) =>
            _dialogueManager.ClearUntreatedInjuryTopic(injuryId, reason);

        private bool StartSimpleTreatment(string injuryId)
        {
            string cureBuffId = CureByInjury[injuryId];
            _buffManager.RemoveBuff(injuryId);
            _buffManager.AddBuff(cureBuffId, -2);
            if (!_buffManager.HasBuff(cureBuffId))
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: не удалось наложить {cureBuffId} — simple treatment отменено",
                    LogLevel.Error);
                _buffManager.AddBuff(injuryId, -2);
                return false;
            }

            // DebuffState: PhaseStartDay + Phase1Duration — для CheckSimpleTreatmentCompletion
            int today = (int)Game1.stats.DaysPlayed;
            int treatmentDays = CalculateTopicDuration(injuryId);

            var ds = _stateManager.GetDebuffState(injuryId);
            if (ds == null)
            {
                _monitor.Log(
                    $"[Treatment] DebuffState для {injuryId} отсутствует — создаём для simple treatment",
                    LogLevel.Warn);
                ds = _stateManager.CreateDebuffState(injuryId, today, treatmentDays, 0, 0);
            }

            if (ds == null)
            {
                _monitor.Log($"⚠️ Не удалось создать DebuffState для {injuryId}, simple treatment отменено", LogLevel.Error);
                _buffManager.RemoveBuff(cureBuffId);
                _buffManager.AddBuff(injuryId, -2);
                return false;
            }

            ds.TreatmentStarted = true;
            ds.PhaseStartDay = today;
            ds.Phase1Duration = treatmentDays;
            _stateManager.UpdateDebuffState(injuryId, ds);

            _monitor.Log(
                $"Нефазовое лечение начато: {CureByInjury[injuryId]}, срок={treatmentDays} дней (PhaseStartDay={today})",
                LogLevel.Info);

            if (string.Equals(injuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
                _dialogueManager.TryAddDiagnosisCompleteTopic(injuryId);

            return true;
        }
        
        /// <summary>
        /// Начать фазовое лечение (для травм с фазовыми баффами)
        /// </summary>
        private bool StartPhasedTreatment(string injuryId)
        {
            _monitor.Log($"🏥 Начинаем фазовое лечение для {injuryId}", LogLevel.Info);
            
            // Получаем состояние дебаффа
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} не найдено!", LogLevel.Warn);
                return false;
            }
            
            // ВАЖНО: Заменяем базовый бафф травмы на Фазу 1
            _buffManager.RemoveBuff(injuryId);
            _monitor.Log($"❌ Удалён базовый бафф: {injuryId}", LogLevel.Debug);

            string phase1BuffId = _injuryManager.GetPhaseBuffId(injuryId, 1);
            if (!_buffManager.BuffExists(phase1BuffId))
            {
                _monitor.Log(
                    $"[PhaseBuffMissing] injury={injuryId} phase=1 expected={phase1BuffId} BuffExists=false",
                    LogLevel.Error);
                return false;
            }

            _buffManager.AddBuff(phase1BuffId, -2);
            if (!_buffManager.HasBuff(phase1BuffId))
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: не удалось наложить {phase1BuffId} — фазовое лечение отменено",
                    LogLevel.Error);
                _buffManager.AddBuff(injuryId, -2);
                return false;
            }

            _monitor.Log($"✅ Применена Фаза 1: {phase1BuffId}", LogLevel.Info);

            int currentDay = (int)StardewValley.Game1.stats.DaysPlayed;
            _stateManager.StartTreatment(injuryId, currentDay);

            string phase1TopicId = _injuryManager.GetPhaseTopicId(injuryId, 1);
            int phase1TopicDays = Math.Max(1, debuffState.Phase1Duration);
            _dialogueManager.AddTopic(phase1TopicId, phase1TopicDays);
            _monitor.Log($"💬 Phase topic: {phase1TopicId} на {phase1TopicDays} дн.", LogLevel.Debug);

            // Создаём топик наблюдения
            string treatmentTopicId = TopicIds.GetTreatmentTopic(injuryId);
            int totalDuration = debuffState.GetTotalDuration();
            _dialogueManager.AddTopic(treatmentTopicId, totalDuration);
            _monitor.Log($"🏥 Создан топик фазового лечения: {treatmentTopicId} на {totalDuration} дней", LogLevel.Debug);

            _dialogueManager.TryAddDiagnosisCompleteTopic(injuryId);
            return true;
        }

        /// <summary>
        /// Показать реакцию Харви и начать лечение (с защитой от зависаний)
        /// </summary>
        public void TreatWithReaction(NPC harvey, InjuryCollection injuries)
        {
            try
            {
                // Проверяем, что Харви существует и не занят
                if (harvey == null)
                {
                    _monitor.Log("⚠️ Харви не найден, пропускаем реакцию", LogLevel.Warn);
                    return;
                }

                // Определяем эмоцию на основе тяжести травм
                int emote = DetermineEmoteForInjuries(injuries);
                
                // Определяем текстовое сообщение
                string textMessage = DetermineTextForInjuries(injuries);
                
                // Показываем эмоцию с текстом
                _dialogueManager.ShowEmoteWithText(harvey, emote, textMessage);
                
                // Звук
                Game1.playSound(GetSoundForInjuries(injuries));
                
                _monitor.Log($"😊 Харви отреагировал эмоцией {emote} и текстом '{textMessage}'", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"❌ Ошибка при показе реакции Харви: {ex}", LogLevel.Error);
                // Показываем простую эмоцию в случае ошибки
                try
                {
                    _dialogueManager.ShowEmote(harvey, HarveyHelper.GetCaringEmote());
                }
                catch
                {
                    // Если и это не работает, просто логируем
                    _monitor.Log("❌ Не удалось показать даже простую эмоцию", LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Определить эмоцию Харви на основе травм
        /// </summary>
        public int DetermineEmoteForInjuries(InjuryCollection injuries)
        {
            // Критические травмы - тревога
            if (injuries.MainInjury != null && IsCriticalInjury(injuries.MainInjury))
            {
                return HarveyEmotes.CriticalInjury; // Восклицание
            }

            // Серьёзные травмы с осложнениями - беспокойство
            if (injuries.MainInjury != null && IsSeriousInjury(injuries.MainInjury) && injuries.Complications.Count > 0)
            {
                return HarveyEmotes.WorriedAboutPatient; // Грусть
            }

            // Множественные осложнения - вопрос
            if (injuries.Complications.Count >= 2)
            {
                return HarveyEmotes.FoundComplication; // Вопрос
            }

            // Одна травма — забота (♥ только при dating/married)
            if (injuries.MainInjury != null)
            {
                return HarveyHelper.GetCaringEmote();
            }

            // Только осложнения - восклицание
            if (injuries.Complications.Count > 0)
            {
                return HarveyEmotes.DirtyWound; // Восклицание
            }

            // По умолчанию - дружелюбие
            return HarveyEmotes.StartTreatment; // Улыбка
        }

        /// <summary>
        /// Получить звук для травм
        /// </summary>
        private string GetSoundForInjuries(InjuryCollection injuries)
        {
            if (injuries.MainInjury != null && IsCriticalInjury(injuries.MainInjury))
            {
                return "debuffHit"; // Критическая травма
            }

            if (injuries.Complications.Count > 0)
            {
                return "debuffSpell"; // Осложнения
            }

            return "healSound"; // Обычное лечение
        }

        private bool IsCriticalInjury(string injuryId)
        {
            return injuryId switch
            {
                "buffConcussion" => true,
                "buffFracturedBone" => true,
                "buffInfectedWound" => true,
                "buffBadlyHurt" => true,
                _ => false
            };
        }

        private bool IsSeriousInjury(string injuryId)
        {
            return injuryId switch
            {
                "buffShrapnelWounds" => true,
                "buffBurnWounds" => true,
                "buffSurgicalWound" => true,
                "buffDeepCuts" => true,
                "buffTornMuscles" => true,
                _ => IsCriticalInjury(injuryId)
            };
        }

        /// <summary>
        /// Определить текстовое сообщение для травм
        /// </summary>
        public string DetermineTextForInjuries(InjuryCollection injuries)
        {
            // Критические травмы
            if (injuries.MainInjury != null && IsCriticalInjury(injuries.MainInjury))
            {
                return TextMessageSelector.ForInjuryDiscovery(isCritical: true, isSerious: false);
            }

            // Множественные осложнения
            if (injuries.Complications.Count >= 3)
            {
                return HarveyTextMessages.MultipleInjuries;
            }

            // Серьёзная травма с осложнениями
            if (injuries.MainInjury != null && IsSeriousInjury(injuries.MainInjury) && injuries.Complications.Count > 0)
            {
                return TextMessageSelector.ForInjuryDiscovery(isCritical: false, isSerious: true);
            }

            // Специфичные осложнения
            if (injuries.Complications.Contains(InjuryBuffs.DirtyWound))
            {
                return HarveyTextMessages.DirtyWound;
            }

            if (injuries.Complications.Contains(InjuryBuffs.WetBandage))
            {
                return HarveyTextMessages.WetBandage;
            }

            if (injuries.Complications.Contains(InjuryBuffs.WetStitches))
            {
                return HarveyTextMessages.WetStitches;
            }

            if (injuries.Complications.Contains(InjuryBuffs.AllergicRash))
            {
                return HarveyTextMessages.AllergicReaction;
            }

            // Обычное лечение
            if (injuries.MainInjury != null)
            {
                return TextMessageSelector.ForTreatmentStart(injuries.Complications.Count > 0);
            }

            // По умолчанию
            return HarveyTextMessages.StartingTreatment;
        }

        /// <summary>
        /// Вылечить все осложнения — убираем баффы и удаляем из _state
        /// </summary>
        public void TreatAllComplications(List<string> complications)
        {
            foreach (var compId in complications)
            {
                _buffManager.RemoveBuff(compId);
                _stateManager.State.ActiveComplications.Remove(compId);
                _stateManager.RemoveDebuffState(compId);
                _stateManager.State.TopicMemory.Remove(compId);

                // Диалоговые топики убираем тоже
                _dialogueManager.RemoveTopic(TopicIds.GetComplicationTopic(compId));
                _dialogueManager.RemoveTopicIfOwned(TopicIds.GetTreatComplicationTopic(compId), "осложнение снято");
                _dialogueManager.ClearTreatmentNeededComplicationTopic(compId, "осложнение снято");

                _monitor.Log($"Осложнение вылечено и удалено из state: {compId}", LogLevel.Info);
            }
            _stateManager.Save();
        }

        /// <summary>
        /// Проверить наличие соответствующего лечения для травмы
        /// </summary>
        public bool HasMatchingTreatment(string? injuryId)
        {
            if (injuryId == null) return false;

            // Для фазовых травм лечение считается активным если TreatmentStarted = true
            if (PhasedInjuries.Contains(injuryId))
            {
                var debuffState = _stateManager.GetDebuffState(injuryId);
                return debuffState?.TreatmentStarted == true;
            }

            // Для нефазовых — проверяем наличие лечебного баффа
            if (!CureByInjury.TryGetValue(injuryId, out var cure)) return false;

            if (string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
            {
                return _buffManager.HasBuff(cure)
                    || _buffManager.HasBuff(CureBuffs.BadlyHurtOutpatientCare);
            }

            return _buffManager.HasBuff(cure);
        }

        /// <summary>
        /// Один DialogueBox при первом клике «начать лечение»: TreatmentStart_{Injury}_* (+ осложнения через $b).
        /// </summary>
        public string BuildFirstStartTreatmentDialogue(InjuryCollection injuries, string? overrideMainInjuryText = null) =>
            BuildCombinedDialogue(
                injuries,
                markTreatmentDiscussed: false,
                firstTreatmentStart: true,
                overrideMainInjuryText: overrideMainInjuryText);

        /// <summary>
        /// Клик TreatComplications при уже идущем лечении: только ComplicationTreatment_* (без диагноза основной травмы).
        /// </summary>
        public string BuildComplicationTreatmentDialogue(IReadOnlyList<string> complicationBuffIds)
        {
            var parts = new List<string>();
            foreach (string compId in complicationBuffIds)
            {
                if (string.IsNullOrWhiteSpace(compId))
                    continue;
                parts.Add(_dialogueManager.PickComplicationTreatmentDialogue(compId));
            }

            if (parts.Count == 0)
                return DialogueManager.ComplicationTreatmentFallback;

            return string.Join("$b", parts);
        }

        /// <summary>
        /// Построить диалог лечения из топиков
        /// </summary>
        /// <param name="markTreatmentDiscussed">Если false — только выбор текста, без записи в state.</param>
        /// <param name="firstTreatmentStart">Префикс TreatmentStart_{InjuryName}_* (первый старт по клику).</param>
        /// <param name="overrideMainInjuryText">Готовая реплика основной травмы (например HarveyCareTrust_*).</param>
        public string BuildCombinedDialogue(
            InjuryCollection injuries,
            bool markTreatmentDiscussed = true,
            bool firstTreatmentStart = false,
            string? overrideMainInjuryText = null)
        {
            var parts = new List<string>();

            // Основная травма
            if (injuries.MainInjury != null)
            {
                string mainText = overrideMainInjuryText
                    ?? GetTreatmentDialogue(injuries.MainInjury, markTreatmentDiscussed, firstTreatmentStart);
                parts.Add(mainText);
            }

            // Осложнения
            AddComplicationDialogue(parts, injuries, InjuryBuffs.DirtyWound, "Proximity_DirtyWound", 
                "И рана загрязнилась — сейчас обработаю.$a");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.WetBandage, "Proximity_WetBandage", 
                "Повязка промокла. Меняю на сухую.$0");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.WetStitches, "Proximity_WetStitches", 
                "Швы намокли — наложу водонепроницаемую повязку.$a");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.AllergicRash, "Proximity_AllergicRash", 
                "Есть аллергическая реакция. Сменю препарат.$u");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.PainFlare, "Proximity_PainFlare", 
                "Боль обострилась — дам обезболивающее.$0");

            return string.Join("$b", parts);
        }

        private void AddComplicationDialogue(
            List<string> parts, 
            InjuryCollection injuries, 
            string buffId, 
            string prefix, 
            string fallback)
        {
            if (injuries.Complications.Contains(buffId))
            {
                string line = _dialogueManager.PickRandomDialogueByPrefix(prefix, fallback);
                parts.Add(line);
            }
        }

        private string GetTreatmentDialogue(
            string injuryId,
            bool markTreatmentDiscussed = true,
            bool firstTreatmentStart = false)
        {
            // Убираем префикс "buff" для получения чистого названия травмы
            string cleanInjuryId = injuryId.Replace("buff", "");
            
            if (firstTreatmentStart)
            {
                string firstStartLine = _dialogueManager.PickFirstTreatmentStartDialogue(injuryId);
                _monitor.Log($"Получен диалог первого лечения: {firstStartLine}", LogLevel.Debug);
                if (markTreatmentDiscussed)
                    _stateManager.MarkHarveyConversation(injuryId, true);
                return firstStartLine;
            }

            // Проверяем, был ли уже разговор о лечении этой травмы
            bool wasDiscussed = _stateManager.GetDebuffState(injuryId)?.HarveyConversationHappened == true;
            
            _monitor.Log($"Получаем диалог лечения для: {cleanInjuryId}, wasDiscussed={wasDiscussed}", LogLevel.Debug);
            
            // Получаем диалог с учётом состояния разговора
            string dialogue = _dialogueManager.PickTreatmentDialogue(cleanInjuryId, wasDiscussed, 
                $"Сейчас займусь твоей травмой.$u");
            
            _monitor.Log($"Получен диалог лечения: {dialogue}", LogLevel.Debug);
            
            if (markTreatmentDiscussed)
                _stateManager.MarkHarveyConversation(injuryId, true);
            
            return dialogue;
        }

        /// <summary>
        /// Рассчитать длительность топика на основе травмы
        /// </summary>
        public int CalculateTopicDuration(string injuryId)
        {
            return injuryId switch
            {
                "buffHurt" => 2,
                "buffBadlyHurt" => 4,
                "buffConcussion" => 9,
                "buffFracturedBone" => 18,
                "buffSurgicalWound" => 7,
                "buffInfectedWound" => 6,
                "buffShrapnelWounds" => 11,
                "buffBurnWounds" => 8,
                "buffDeepCuts" => 7,
                _ => 3
            };
        }
    }
}

