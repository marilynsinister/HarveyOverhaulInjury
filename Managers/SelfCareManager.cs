using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Домашняя самопомощь: слабее лечения у Харви, снижает риски осложнений.
    /// </summary>
    public class SelfCareManager
    {
        private const int TopicDays = 3;
        private const int SelfCareBuffMinutes = -2;
        private const double WetBandageRemoveChance = 0.50;
        private const double InfectionRiskMultiplier = 0.50;

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly ComplianceManager _complianceManager;
        private readonly PrescriptionManager _prescriptionManager;

        public SelfCareManager(
            IMonitor monitor,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            ComplianceManager complianceManager,
            PrescriptionManager prescriptionManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _complianceManager = complianceManager;
            _prescriptionManager = prescriptionManager;
        }

        public bool ApplyCleanBandage(bool force = false)
        {
            int today = GameUtils.Today();
            var state = _stateManager.State;

            if (!force && state.LastSelfCareBandageDay == today)
            {
                _monitor.Log("[SelfCare] CleanBandage: уже сегодня", LogLevel.Debug);
                return false;
            }

            if (!force && !IsAtHome())
            {
                _monitor.Log("[SelfCare] CleanBandage: не дома", LogLevel.Debug);
                return false;
            }

            if (!CanApplyCleanBandage())
            {
                _monitor.Log("[SelfCare] CleanBandage: нет условий", LogLevel.Debug);
                return false;
            }

            state.LastSelfCareBandageDay = today;
            bool hadWetBandage = HasWetBandage();
            bool removedWetBandage = false;

            if (hadWetBandage && Game1.random.NextDouble() < WetBandageRemoveChance)
            {
                RemoveWetBandageComplication();
                removedWetBandage = true;
                _monitor.Log("[SelfCare] CleanBandage: WetBandage снята (50%)", LogLevel.Info);
            }
            else
            {
                ScheduleProtection(SelfCareProtectionTypes.CleanBandage, today + 1);
                _monitor.Log("[SelfCare] CleanBandage: защита от инфекции на завтра", LogLevel.Info);
            }

            state.PendingSelfCareBandageCompliance = true;
            ApplySelfCareMarkers(SelfCareBuffs.CleanBandage);
            _dialogueManager.AddTopic(ConversationTopics.CleanBandage, TopicDays);

            Game1.addHUDMessage(new HUDMessage(
                removedWetBandage
                    ? "Ты аккуратно сменила повязку. Сухо и чище — так лучше."
                    : "Ты перевязала рану дома. Это не заменяет осмотр, но помогает.",
                HUDMessage.health_type));

            _stateManager.Save();
            return true;
        }

        public bool ApplyWarmTea(bool force = false)
        {
            int today = GameUtils.Today();
            var state = _stateManager.State;

            if (!force && state.LastSelfCareTeaDay == today)
                return false;

            if (!force && !IsAtHome())
                return false;

            if (!HasColdCondition())
                return false;

            state.LastSelfCareTeaDay = today;
            ScheduleProtection(SelfCareProtectionTypes.WarmTea, today);

            ApplySelfCareMarkers(SelfCareBuffs.WarmTea);
            _dialogueManager.AddTopic(ConversationTopics.WarmTea, TopicDays);

            Game1.addHUDMessage(new HUDMessage(
                "Тёплый чай согрел изнутри. Простуда не прошла, но день будет легче.",
                HUDMessage.health_type));

            _stateManager.Save();
            _monitor.Log("[SelfCare] WarmTea применён", LogLevel.Info);
            return true;
        }

        public bool ApplyRestCare(bool force = false)
        {
            int today = GameUtils.Today();

            if (!force && _stateManager.State.LastRestSelfCareDay == today)
                return false;

            if (!force)
            {
                if (!_prescriptionManager.HasActivePrescription(PrescriptionIds.Rest))
                    return false;

                if (Game1.timeOfDay >= 2200)
                    return false;
            }

            _stateManager.State.LastRestSelfCareDay = today;
            _complianceManager.AddCompliance(+1, "selfcare_rest");
            ApplySelfCareMarkers(SelfCareBuffs.SelfCare);
            _dialogueManager.AddTopic(ConversationTopics.SelfCarePraise, TopicDays);

            Game1.addHUDMessage(new HUDMessage(
                "Ты легла пораньше. Харви, наверное, одобрил бы.",
                HUDMessage.health_type));

            _stateManager.Save();
            _monitor.Log("[SelfCare] RestCare применён", LogLevel.Info);
            return true;
        }

        /// <summary>Вызывать в OnDayEnding до проверки позднего сна.</summary>
        public void TryApplyRestCareOnDayEnding()
        {
            if (Game1.timeOfDay >= 2200)
                return;

            if (!_prescriptionManager.HasActivePrescription(PrescriptionIds.Rest))
                return;

            ApplyRestCare();
        }

        /// <summary>Начислить отложенный +1 к TreatmentComplianceScore за повязку при визите к Харви.</summary>
        public void OnHarveyMedicalVisit()
        {
            var state = _stateManager.State;
            if (!state.PendingSelfCareBandageCompliance)
                return;

            state.PendingSelfCareBandageCompliance = false;
            _complianceManager.AddCompliance(+1, "selfcare_bandage_harvey_visit");
            _dialogueManager.AddTopic(ConversationTopics.SelfCare, TopicDays);
            _stateManager.Save();
            _monitor.Log("[SelfCare] +1 TreatmentComplianceScore за домашнюю повязку (визит к Харви)", LogLevel.Info);
        }

        public bool HasSelfCareProtection(string type)
        {
            int today = GameUtils.Today();
            return _stateManager.State.SelfCareProtections.TryGetValue(type, out int activeDay)
                && activeDay == today;
        }

        public bool ConsumeSelfCareProtection(string type)
        {
            if (!HasSelfCareProtection(type))
                return false;

            _stateManager.State.SelfCareProtections.Remove(type);
            _stateManager.Save();
            _monitor.Log($"[SelfCare] Защита {type} использована", LogLevel.Debug);
            return true;
        }

        /// <summary>Множитель шанса инфекции от мокрой повязки (1.0 = без изменений).</summary>
        public double GetWetBandageInfectionChanceMultiplier()
        {
            if (!HasSelfCareProtection(SelfCareProtectionTypes.CleanBandage))
                return 1.0;

            return InfectionRiskMultiplier;
        }

        /// <summary>+1 день отсрочки небрежности для простуды при тёплом чае.</summary>
        public int GetColdNeglectGraceBonus(string injuryId)
        {
            if (!string.Equals(injuryId, InjuryBuffs.Cold, StringComparison.OrdinalIgnoreCase))
                return 0;

            return HasSelfCareProtection(SelfCareProtectionTypes.WarmTea) ? 1 : 0;
        }

        public string GetStatusSummary()
        {
            var state = _stateManager.State;
            int today = GameUtils.Today();
            return
                $"bandageDay={state.LastSelfCareBandageDay} teaDay={state.LastSelfCareTeaDay} restDay={state.LastRestSelfCareDay} " +
                $"pendingCompliance={state.PendingSelfCareBandageCompliance} protections={state.SelfCareProtections.Count} today={today}";
        }

        private bool CanApplyCleanBandage() =>
            HasWetBandage() || HasBandageEligibleTreatment();

        private bool HasWetBandage() =>
            _buffManager.HasBuff(InjuryBuffs.WetBandage)
            || _stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.WetBandage);

        private bool HasBandageEligibleTreatment()
        {
            foreach (string injuryId in new[] { "buffDeepCuts", "buffBurnWounds", "buffSurgicalWound" })
            {
                var ds = _stateManager.GetDebuffState(injuryId);
                if (ds?.TreatmentStarted == true)
                    return true;
            }

            return _buffManager.HasBuff(CureBuffs.PostSurgical);
        }

        private bool HasColdCondition() =>
            _buffManager.HasBuff(InjuryBuffs.Cold)
            || _buffManager.HasBuff(InjuryBuffs.ColdAcute);

        private void RemoveWetBandageComplication()
        {
            _buffManager.RemoveBuff(InjuryBuffs.WetBandage);
            _stateManager.State.ActiveComplications.Remove(InjuryBuffs.WetBandage);
            _stateManager.RemoveDebuffState(InjuryBuffs.WetBandage);
            _dialogueManager.RemoveTopic(ConversationTopics.WetBandage);
        }

        private void ScheduleProtection(string type, int activeDay)
        {
            _stateManager.State.SelfCareProtections[type] = activeDay;
        }

        private void ApplySelfCareMarkers(string specificBuffId)
        {
            if (_buffManager.BuffExists(SelfCareBuffs.SelfCare))
                _buffManager.AddBuff(SelfCareBuffs.SelfCare, SelfCareBuffMinutes);

            if (_buffManager.BuffExists(specificBuffId))
                _buffManager.AddBuff(specificBuffId, SelfCareBuffMinutes);

            _dialogueManager.AddTopic(ConversationTopics.SelfCare, TopicDays);
        }

        private static bool IsAtHome()
        {
            return Game1.player.currentLocation is FarmHouse or Cabin;
        }
    }
}
