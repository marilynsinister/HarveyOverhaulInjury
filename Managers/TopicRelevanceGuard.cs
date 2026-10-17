using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Снимает медицинские топики, чья причина уже прошла: заноза вытащена самой фермершей, травма вылечена,
    /// осложнение снято. Иначе топик доживает свои дни, и Харви «лечит» то, чего уже нет
    /// («Секунду, пинцет…», «Я оставил тебе план лечения», «Режим соблюдается…»).
    /// Топики-последствия (topic*Cured, проверки после выздоровления) не трогает — они и должны звучать после.
    /// </summary>
    public sealed class TopicRelevanceGuard
    {
        private const int PollIntervalTicks = 60;

        /// <summary>После загрузки/начала дня баффы травм восстанавливаются с задержкой — не проверяем сразу.</summary>
        private const int GraceTicks = 180;

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;

        /// <summary>Топик → условие, при котором он ещё уместен.</summary>
        private readonly Dictionary<string, Func<bool>> _requirements = new(StringComparer.OrdinalIgnoreCase);

        private int _graceTicksLeft = -1;

        public TopicRelevanceGuard(
            IMonitor monitor,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            BuildRequirements();
        }

        public void OnSaveLoaded() => _graceTicksLeft = GraceTicks;

        public void OnDayStarted() => _graceTicksLeft = GraceTicks;

        public void OnReturnedToTitle() => _graceTicksLeft = -1;

        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (_graceTicksLeft < 0)
                return;

            if (_graceTicksLeft > 0)
            {
                _graceTicksLeft--;
                return;
            }

            if (e.Ticks % PollIntervalTicks != 0)
                return;

            if (!Context.IsWorldReady || Game1.CurrentEvent != null || Game1.activeClickableMenu != null)
                return;

            var active = Game1.player?.activeDialogueEvents;
            if (active == null)
                return;

            foreach (string topicId in active.Keys.ToList())
            {
                if (!_requirements.TryGetValue(topicId, out var isRelevant) || isRelevant())
                    continue;

                _dialogueManager.RemoveTopic(topicId);
                _monitor.Log($"[TopicGuard] снят {topicId}: причины больше нет", LogLevel.Debug);
            }
        }

        private void BuildRequirements()
        {
            foreach (string mishapId in MishapBuffs.All)
                Require(MishapManager.GetTopic(mishapId), () => _buffManager.HasBuff(mishapId));

            foreach (string buffId in InjurySets.HarveyTreatable)
            {
                Func<bool> injuryActive = () => IsInjuryActive(buffId);

                Require(TopicIds.GetInjuryTopic(buffId), injuryActive);
                Require(TopicIds.GetTreatmentTopic(buffId), injuryActive);
                Require(TopicIds.GetFestivalDeferTopic(buffId), injuryActive);
                Require(TreatmentPlanTopics.GetInjuryTopic(buffId), injuryActive);
                foreach (string topic in TopicIds.GetAllActionTopicsForInjury(buffId))
                    Require(topic, injuryActive);
                foreach (string topic in TopicIds.GetAllHiddenInjuryTopicsForInjury(buffId))
                    Require(topic, injuryActive);

                int totalPhases = InjurySets.InferDefaultTotalPhases(buffId);
                for (int phase = 1; phase <= totalPhases; phase++)
                    Require(TopicIds.GetPhaseTopicId(buffId, phase, totalPhases), injuryActive);
            }

            foreach (string compId in InjurySets.KnownComplicationBuffIds)
            {
                Func<bool> complicationActive = () => IsComplicationActive(compId);

                Require(TopicIds.GetComplicationTopic(compId), complicationActive);
                Require(TopicIds.GetTreatmentNeededComplicationTopic(compId), complicationActive);
                Require(TopicIds.GetTreatComplicationTopic(compId), complicationActive);
            }

            // Общие реплики о ходе лечения уместны, пока лечится хоть одна травма.
            Require(TreatmentPlanTopics.Given, IsAnyInjuryActive);

            // О режиме — пока лечение или реабилитация ещё идут.
            Require(ComplianceTopics.High, IsUnderMedicalCare);
            Require(ComplianceTopics.Neutral, IsUnderMedicalCare);
            Require(ComplianceTopics.Low, IsUnderMedicalCare);
            Require(ComplianceTopics.StrictMedicalMode, IsUnderMedicalCare);
            Require(PrescriptionTopics.Violation, IsUnderMedicalCare);
            Require(PrescriptionTopics.Followed, IsUnderMedicalCare);

            // Назначение — пока оно действует.
            Require(PrescriptionTopics.Rest, () => IsPrescriptionActive(PrescriptionIds.Rest));
            Require(PrescriptionTopics.NoMine, () => IsPrescriptionActive(PrescriptionIds.NoMine));
            Require(PrescriptionTopics.KeepDry, () => IsPrescriptionActive(PrescriptionIds.KeepDry));
            Require(PrescriptionTopics.LightWork, () => IsPrescriptionActive(PrescriptionIds.LightWork));
            Require(PrescriptionTopics.Checkup, () => IsPrescriptionActive(PrescriptionIds.Checkup));

            // Реабилитация — пока она идёт (RehabCompleted звучит после — его не трогаем).
            Require(ConversationTopics.Rehab, IsRehabActive);
            Require(ConversationTopics.RehabStrict, IsRehabActive);
        }

        /// <summary>Первое правило выигрывает: у разных травм могут совпасть legacy-имена.</summary>
        private void Require(string topicId, Func<bool> isRelevant) => _requirements.TryAdd(topicId, isRelevant);

        /// <summary>Травма жива, пока есть её DebuffState (во время лечения базовый бафф заменяется лечебным).</summary>
        private bool IsInjuryActive(string buffId) =>
            _stateManager.GetDebuffState(buffId) != null || _buffManager.HasBuff(buffId);

        private bool IsComplicationActive(string compId) =>
            _buffManager.HasBuff(compId)
            || _stateManager.State.ActiveComplications?.ContainsKey(compId) == true
            || _stateManager.GetDebuffState(compId) != null;

        private bool IsAnyInjuryActive() =>
            InjurySets.HarveyTreatable.Any(IsInjuryActive);

        private bool IsPrescriptionActive(string prescriptionId) =>
            _stateManager.State.ActivePrescriptions?.ContainsKey(prescriptionId) == true;

        private bool IsRehabActive() =>
            !string.IsNullOrEmpty(_stateManager.State.ActiveRehabInjuryId);

        private bool IsUnderMedicalCare() =>
            IsAnyInjuryActive() || IsRehabActive() || _stateManager.State.ActivePrescriptions?.Count > 0;
    }
}
