using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// UI-бафф-напоминание о визите к Харви. Источник истины — DebuffState, ActiveComplications, госпитализация.
    /// </summary>
    public class DoctorVisitReminderManager
    {
        private readonly IMonitor _monitor;
        private readonly BuffManager _buffManager;
        private readonly StateManager _stateManager;
        private readonly HospitalizationManager _hospitalizationManager;

        public DoctorVisitReminderManager(
            IMonitor monitor,
            BuffManager buffManager,
            StateManager stateManager,
            HospitalizationManager hospitalizationManager)
        {
            _monitor = monitor;
            _buffManager = buffManager;
            _stateManager = stateManager;
            _hospitalizationManager = hospitalizationManager;
        }

        private ComplicationManager? _complicationManager;

        public void SetComplicationManager(ComplicationManager complicationManager) =>
            _complicationManager = complicationManager;

        /// <summary>
        /// Напоминание висит только когда Харви на приёме действительно что-то сделает:
        /// осмотр/смена фазы главной травмы, выписка, лечение осложнения, выписка из больницы.
        /// Раньше учитывались любые осложнения (в т.ч. «Обострение боли», которое Харви процедурой не лечит)
        /// и флаги готовности не-главных травм — бафф висел, а Харви отвечал обычным диалогом.
        /// </summary>
        public bool IsVisitNeeded()
        {
            string? mainInjuryId = _stateManager.GetMainInjuryId();
            foreach (var (injuryId, debuffState) in _stateManager.State.ActiveDebuffs)
            {
                if (!debuffState.TreatmentStarted)
                    continue;

                if (InjurySets.KnownComplicationBuffIds.Contains(injuryId))
                    continue;

                if (!string.IsNullOrEmpty(mainInjuryId)
                    && !string.Equals(injuryId, mainInjuryId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (debuffState.ReadyForNextPhase || debuffState.ReadyForRecovery)
                    return true;
            }

            if (_complicationManager != null
                ? _complicationManager.GetActiveTreatableComplicationIds().Count > 0
                : _stateManager.State.ActiveComplications.Keys.Any(id =>
                    !string.Equals(id, InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase)))
                return true;

            if (_hospitalizationManager.IsHospitalized && _hospitalizationManager.CanDischarge())
                return true;

            return false;
        }

        public void SyncReminderBuff()
        {
            bool needed = IsVisitNeeded();
            bool hasBuff = _buffManager.HasBuff(ReminderBuffs.DoctorVisitNeeded);

            if (needed && !hasBuff)
            {
                _buffManager.AddBuff(ReminderBuffs.DoctorVisitNeeded, -2);
                _monitor.Log("[DoctorVisit] reminder buff applied", LogLevel.Trace);
            }
            else if (!needed && hasBuff)
            {
                _buffManager.RemoveBuff(ReminderBuffs.DoctorVisitNeeded);
                _stateManager.State.SavedActiveBuffs.RemoveAll(id =>
                    string.Equals(id, ReminderBuffs.DoctorVisitNeeded, StringComparison.OrdinalIgnoreCase));
                _monitor.Log("[DoctorVisit] reminder buff removed", LogLevel.Trace);
            }
        }
    }
}
