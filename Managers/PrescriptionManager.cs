using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Медицинские предписания Харви после начала лечения травмы.
    /// </summary>
    public class PrescriptionManager
    {
        private const int ViolationTopicDays = 2;
        private const int RepeatedViolationTopicDays = 3;
        private const int StrictMedicalModeTopicDays = 3;
        private const int StrictModeViolationThreshold = 2;
        private const int NeglectStrikeViolationThreshold = 3;
        private const int FollowedTopicDays = 1;
        private const int DefaultCheckupDays = 7;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly BuffManager _buffManager;
        private readonly ComplianceManager _complianceManager;

        private static readonly Dictionary<string, (string Id, int Days)[]> InjuryPrescriptionRules =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["buffConcussion"] = new[]
                {
                    (PrescriptionIds.Rest, 3),
                    (PrescriptionIds.Checkup, 0),
                },
                ["buffFracturedBone"] = new[]
                {
                    (PrescriptionIds.Rest, 5),
                    (PrescriptionIds.NoMine, 7),
                    (PrescriptionIds.Checkup, 0),
                },
                ["buffDeepCuts"] = new[]
                {
                    (PrescriptionIds.KeepDry, 3),
                    (PrescriptionIds.NoMine, 2),
                    (PrescriptionIds.Checkup, 0),
                },
                ["buffBurnWounds"] = new[]
                {
                    (PrescriptionIds.KeepDry, 5),
                    (PrescriptionIds.Checkup, 0),
                },
                ["buffInfectedWound"] = new[]
                {
                    (PrescriptionIds.Rest, 4),
                    (PrescriptionIds.KeepDry, 4),
                    (PrescriptionIds.Checkup, 0),
                },
                ["buffShrapnelWounds"] = new[]
                {
                    (PrescriptionIds.Rest, 5),
                    (PrescriptionIds.NoMine, 5),
                    (PrescriptionIds.Checkup, 0),
                },
                ["buffTornMuscles"] = new[]
                {
                    (PrescriptionIds.LightWork, 3),
                    (PrescriptionIds.Rest, 2),
                },
                ["buffBackStrain"] = new[]
                {
                    (PrescriptionIds.LightWork, 3),
                },
                ["buffBruisedRibs"] = new[]
                {
                    (PrescriptionIds.Rest, 3),
                    (PrescriptionIds.LightWork, 2),
                },
                ["buffSprainedAnkle"] = new[]
                {
                    (PrescriptionIds.LightWork, 2),
                },
                [InjuryBuffs.Cold] = new[]
                {
                    (PrescriptionIds.Rest, 2),
                    (PrescriptionIds.KeepDry, 2),
                },
                ["buffBadlyHurt"] = new[]
                {
                    (PrescriptionIds.Rest, 3),
                    (PrescriptionIds.NoMine, 3),
                },
                ["buffHurt"] = new[]
                {
                    (PrescriptionIds.LightWork, 1),
                },
            };

        public PrescriptionManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            BuffManager buffManager,
            ComplianceManager complianceManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _buffManager = buffManager;
            _complianceManager = complianceManager;
        }

        /// <summary>Назначить предписания по правилам для травмы (после начала лечения).</summary>
        public void AssignPrescriptionsForInjury(string injuryId)
        {
            if (!InjuryPrescriptionRules.TryGetValue(injuryId, out var rules))
            {
                _monitor.Log($"Предписания для {injuryId} не определены", LogLevel.Debug);
                return;
            }

            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            var existing = state.ActivePrescriptions
                .Where(kvp => string.Equals(kvp.Value.SourceInjuryId, injuryId, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string prescriptionId in existing)
                RemovePrescriptionInternal(prescriptionId, save: false, awardCompletionCompliance: false);

            foreach (var (id, days) in rules)
            {
                int durationDays = id == PrescriptionIds.Checkup
                    ? GetCheckupDuration(injuryId)
                    : days;
                AddPrescription(id, injuryId, durationDays);
            }

            _monitor.Log(
                $"Назначены предписания для {injuryId}: {string.Join(", ", rules.Select(r => r.Id))}",
                LogLevel.Info);
        }

        /// <summary>Добавить или обновить предписание.</summary>
        public void AddPrescription(string id, string sourceInjuryId, int durationDays)
        {
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            int today = (int)Game1.stats.DaysPlayed;
            durationDays = Math.Max(1, durationDays);

            state.ActivePrescriptions[id] = new PrescriptionState
            {
                Id = id,
                SourceInjuryId = sourceInjuryId,
                StartDay = today,
                DurationDays = durationDays,
                IsViolated = false,
                ViolationCount = 0,
                LastViolationDay = -1,
                LastViolationReason = null,
            };

            string topicId = GetTopicForPrescription(id);
            _dialogueManager.AddTopic(topicId, durationDays);

            if (_buffManager.BuffExists(id))
                _buffManager.AddBuff(id, -2);

            _stateManager.Save();
            _monitor.Log($"Предписание {id} ({durationDays} дн.) ← {sourceInjuryId}", LogLevel.Debug);
        }

        /// <summary>Удалить истёкшие предписания.</summary>
        public void RemoveExpiredPrescriptions()
        {
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            int today = (int)Game1.stats.DaysPlayed;
            var expired = state.ActivePrescriptions
                .Where(kvp => kvp.Value.IsExpired(today))
                .Select(kvp => kvp.Key)
                .ToList();

            if (expired.Count == 0)
                return;

            foreach (string prescriptionId in expired)
                RemovePrescriptionInternal(prescriptionId, save: false, awardCompletionCompliance: true);

            _stateManager.Save();
            _monitor.Log($"Сняты истёкшие предписания: {string.Join(", ", expired)}", LogLevel.Debug);
        }

        public bool HasActivePrescription(string id)
        {
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            if (!state.ActivePrescriptions.TryGetValue(id, out var prescription))
                return false;

            int today = (int)Game1.stats.DaysPlayed;
            return !prescription.IsExpired(today);
        }

        /// <summary>
        /// Зафиксировать нарушение предписания (не чаще 1 раза в день на предписание).
        /// TreatmentComplianceScore is a medical adherence metric. It must never reduce Harvey friendship.
        /// </summary>
        public bool TryMarkViolation(string prescriptionId, string reason, out int violationCount)
        {
            violationCount = 0;
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            if (!state.ActivePrescriptions.TryGetValue(prescriptionId, out var prescription))
            {
                _monitor.Log($"TryMarkViolation: предписание {prescriptionId} не найдено", LogLevel.Debug);
                return false;
            }

            int today = (int)Game1.stats.DaysPlayed;
            if (prescription.IsExpired(today))
                return false;

            if (prescription.LastViolationDay == today)
                return false;

            prescription.IsViolated = true;
            prescription.ViolationCount++;
            prescription.LastViolationDay = today;
            prescription.LastViolationReason = reason;
            violationCount = prescription.ViolationCount;

            // Медицинский score и topics — без changeFriendship и без «relationship penalty».
            _complianceManager.AddCompliance(-1, $"prescription_violation:{reason}");

            int violationTopicDays = violationCount >= StrictModeViolationThreshold
                ? RepeatedViolationTopicDays
                : ViolationTopicDays;
            _dialogueManager.AddTopic(PrescriptionTopics.Violation, violationTopicDays);

            HarveyMailHelper.TryScheduleTieredMail(
                _config,
                _stateManager,
                _monitor,
                HarveyMailHelper.GetPrescriptionViolationMailBase(reason),
                $"{PrescriptionTopics.Violation}:{prescriptionId}:{reason}");

            ApplyViolationEscalation(violationCount);

            _stateManager.Save();
            _monitor.Log(
                $"Нарушение предписания {prescriptionId}: {reason} (всего {violationCount}, compliance={state.TreatmentComplianceScore})",
                LogLevel.Info);
            return true;
        }

        /// <summary>
        /// Повторные нарушения: строже медицинский контроль, не холоднее отношения.
        /// Госпитализация — только из PlayerEventHandler при серьёзной угрозе здоровью.
        /// </summary>
        private void ApplyViolationEscalation(int violationCount)
        {
            _complianceManager.ApplyTreatmentComplianceTopics();

            if (violationCount >= StrictModeViolationThreshold)
            {
                _dialogueManager.AddTopic(ComplianceTopics.StrictMedicalMode, StrictMedicalModeTopicDays);

                if (_complianceManager.IsLowCompliance)
                    _dialogueManager.AddTopic(ComplianceTopics.Low, StrictMedicalModeTopicDays);

                Game1.addHUDMessage(new HUDMessage(
                    "Харви усиливает медицинский контроль. Режим лечения нужно соблюдать.",
                    HUDMessage.error_type));
            }

            if (violationCount >= NeglectStrikeViolationThreshold)
            {
                _stateManager.State.NeglectStrikes++;
                _monitor.Log(
                    $"Предписание: {NeglectStrikeViolationThreshold}-е нарушение → NeglectStrikes={_stateManager.State.NeglectStrikes}",
                    LogLevel.Warn);
            }
        }

        /// <summary>Зафиксировать нарушение предписания.</summary>
        public void MarkViolation(string prescriptionId, string reason)
        {
            TryMarkViolation(prescriptionId, reason, out _);
        }

        /// <summary>Ежедневная проверка предписаний (TreatmentComplianceScore за завершение — при снятии).</summary>
        public void RewardComplianceDaily()
        {
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            int today = (int)Game1.stats.DaysPlayed;
            int yesterday = today - 1;

            var active = state.ActivePrescriptions.Values
                .Where(p => !p.IsExpired(today))
                .ToList();

            if (active.Count == 0)
                return;

            int followedCount = active.Count(p => p.LastViolationDay != yesterday);
            if (followedCount == active.Count)
                _dialogueManager.AddTopic(PrescriptionTopics.Followed, FollowedTopicDays);
        }

        /// <summary>Строки для консоли и debug HUD.</summary>
        public IEnumerable<string> GetActivePrescriptionSummary()
        {
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            int today = (int)Game1.stats.DaysPlayed;

            if (state.ActivePrescriptions.Count == 0)
            {
                yield return "(none)";
                yield break;
            }

            foreach (var (id, prescription) in state.ActivePrescriptions.OrderBy(kvp => kvp.Key))
            {
                if (prescription.IsExpired(today))
                    continue;

                yield return
                    $"{id}  src={prescription.SourceInjuryId}  left={prescription.GetDaysRemaining(today)}d  viol={prescription.ViolationCount}";
            }
        }

        /// <summary>Удалить все предписания (debug reset).</summary>
        public void ClearAllPrescriptions()
        {
            var state = _stateManager.State;
            EnsurePrescriptionsInitialized(state);

            foreach (string prescriptionId in state.ActivePrescriptions.Keys.ToList())
                RemovePrescriptionInternal(prescriptionId, save: false, awardCompletionCompliance: false);

            _complianceManager.SetScore(0);
            state.LastPrescriptionReminderDay = -1;
            _stateManager.Save();
        }

        private void RemovePrescriptionInternal(string prescriptionId, bool save, bool awardCompletionCompliance = false)
        {
            var state = _stateManager.State;
            if (!state.ActivePrescriptions.TryGetValue(prescriptionId, out var prescription))
                return;

            if (awardCompletionCompliance && prescription.ViolationCount == 0)
                _complianceManager.OnPrescriptionCompletedWithoutViolations();

            if (!state.ActivePrescriptions.Remove(prescriptionId))
                return;

            _dialogueManager.RemoveTopic(GetTopicForPrescription(prescriptionId));
            _buffManager.RemoveBuff(prescriptionId);

            if (save)
                _stateManager.Save();
        }

        private int GetCheckupDuration(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
                return DefaultCheckupDays;

            if (debuffState.TotalPhases > 0)
                return Math.Max(1, debuffState.GetTotalDuration());

            if (debuffState.Phase1Duration > 0)
                return debuffState.Phase1Duration;

            return DefaultCheckupDays;
        }

        private static string GetTopicForPrescription(string prescriptionId) =>
            prescriptionId switch
            {
                PrescriptionIds.Rest => PrescriptionTopics.Rest,
                PrescriptionIds.NoMine => PrescriptionTopics.NoMine,
                PrescriptionIds.KeepDry => PrescriptionTopics.KeepDry,
                PrescriptionIds.LightWork => PrescriptionTopics.LightWork,
                PrescriptionIds.Checkup => PrescriptionTopics.Checkup,
                _ => prescriptionId.Replace("HarveyMod_Prescription_", "topicHarvey_Prescription_"),
            };

        private static void EnsurePrescriptionsInitialized(InjuryState state)
        {
            state.ActivePrescriptions ??= new Dictionary<string, PrescriptionState>();
        }
    }
}
