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

        public bool IsVisitNeeded()
        {
            foreach (var debuffState in _stateManager.State.ActiveDebuffs.Values)
            {
                if (!debuffState.TreatmentStarted)
                    continue;

                if (debuffState.ReadyForNextPhase || debuffState.ReadyForRecovery)
                    return true;
            }

            if (_stateManager.State.ActiveComplications.Count > 0)
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
