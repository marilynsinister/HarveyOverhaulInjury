using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Services
{
    /// <summary>
    /// Единый планировщик медицинских писем: очередь в save-state, валидация перед отправкой,
    /// отмена при лечении, очистка устаревших mailForTomorrow.
    /// </summary>
    public sealed class MedicalLetterScheduler
    {
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private DoctorVisitReminderManager? _doctorVisitReminderManager;
        private readonly IMonitor _monitor;

        private static readonly HashSet<string> ManagedMedicalMailIds = new(StringComparer.OrdinalIgnoreCase)
        {
            MailIds.SleepControl,
            MailIds.MineForbidden,
            MailIds.WetCare,
            MailIds.WetStitchesCare,
            MailIds.InfectionAlert,
            MailIds.NeglectWarning,
            MailIds.DirtyWoundInfection,
            MailIds.WetBandageInfection,
            MailIds.TreatmentUrgentReminder,
            MailIds.TreatmentFinalWarning,
            MailIds.CheckupOverdue,
            MailIds.TreatmentPlanMinor,
            MailIds.TreatmentPlanSevere,
            MailIds.PrescriptionViolation,
            MailIds.CheckupReminder,
            MailIds.RehabReminder,
            MailIds.RehabCompleted,
            MailIds.NoMineViolation,
            MailIds.KeepDryViolation,
            MailIds.RestViolation,
        };

        public MedicalLetterScheduler(
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            IMonitor monitor,
            DoctorVisitReminderManager? doctorVisitReminderManager = null)
        {
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _monitor = monitor;
            _doctorVisitReminderManager = doctorVisitReminderManager;
        }

        public void SetDoctorVisitReminderManager(DoctorVisitReminderManager manager) =>
            _doctorVisitReminderManager = manager;

        /// <summary>Критические письма разрешены в режиме CriticalOnly.</summary>
        public static bool IsCriticalReason(string reason) =>
            reason switch
            {
                MedicalLetterReasons.SleepControl => true,
                MedicalLetterReasons.MineForbidden => true,
                MedicalLetterReasons.HospitalDischarge => true,
                _ => false,
            };

        public bool ShouldQueueMedical(bool critical) =>
            _config.MedicalLetters switch
            {
                MedicalLetterMode.Off => false,
                MedicalLetterMode.CriticalOnly => critical,
                MedicalLetterMode.All => true,
                _ => false,
            };

        /// <summary>Tiered письмо в pending-очередь.</summary>
        public bool TryQueueTieredMail(
            string baseMailId,
            string reason,
            string stateId = "",
            bool? critical = null,
            string? dedupeKey = null)
        {
            bool isCritical = critical ?? IsCriticalReason(reason);
            if (!ShouldQueueMedical(isCritical))
            {
                _monitor.Log(
                    $"[MedicalLetters] blocked by mode={_config.MedicalLetters}, reason={reason}, critical={isCritical}",
                    LogLevel.Debug);
                return false;
            }

            if (IsBlockedByHiddenInjury(stateId, reason))
                return false;

            string mailId = HarveyMailHelper.BuildRelationshipMailId(baseMailId);
            return QueueMedicalLetter(mailId, reason, stateId, isCritical, dedupeKey ?? baseMailId);
        }

        public bool QueueMedicalLetter(
            string mailId,
            string reason,
            string stateId,
            bool critical,
            string? dedupeKey = null)
        {
            if (string.IsNullOrWhiteSpace(mailId) || string.IsNullOrWhiteSpace(reason))
                return false;

            if (!ShouldQueueMedical(critical))
            {
                _monitor.Log(
                    $"[MedicalLetters] blocked by mode={_config.MedicalLetters}, reason={reason}",
                    LogLevel.Debug);
                return false;
            }

            if (IsBlockedByHiddenInjury(stateId, reason))
                return false;

            int today = Today();
            string key = dedupeKey ?? mailId;
            var sent = _stateManager.State.SentMedicalMailDays;

            if (sent.TryGetValue(key, out int sentDay) && sentDay == today)
            {
                _monitor.Log(
                    $"[MedicalLetters] skip duplicate key={key} day={today}",
                    LogLevel.Debug);
                return false;
            }

            var pending = _stateManager.State.PendingMedicalLetters;
            if (pending.Any(p =>
                    string.Equals(p.DedupeKey, key, StringComparison.OrdinalIgnoreCase)
                    && p.DeliverAfterDay >= today))
            {
                _monitor.Log(
                    $"[MedicalLetters] skip already pending key={key}",
                    LogLevel.Debug);
                return false;
            }

            pending.Add(new PendingMedicalLetter
            {
                MailId = mailId,
                Reason = reason,
                StateId = stateId ?? "",
                CreatedDay = today,
                DeliverAfterDay = today + 1,
                Critical = critical,
                DedupeKey = key,
            });

            sent[key] = today;
            _stateManager.Save();

            _monitor.Log(
                $"[MedicalLetters] queued mail={mailId} reason={reason} state={stateId} critical={critical} deliverDay={today + 1}",
                LogLevel.Info);
            return true;
        }

        public void CancelLettersForState(string stateId)
        {
            if (string.IsNullOrWhiteSpace(stateId))
                return;

            int removed = RemovePending(p =>
                string.Equals(p.StateId, stateId, StringComparison.OrdinalIgnoreCase)
                || MailMatchesInjuryState(p, stateId));

            if (removed > 0)
            {
                _stateManager.Save();
                _monitor.Log(
                    $"[MedicalLetters] cancelled {removed} pending letter(s) for state={stateId}",
                    LogLevel.Info);
            }
        }

        public void CancelLettersForReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return;

            int removed = RemovePending(p =>
                string.Equals(p.Reason, reason, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
            {
                _stateManager.Save();
                _monitor.Log(
                    $"[MedicalLetters] cancelled {removed} pending letter(s) for reason={reason}",
                    LogLevel.Info);
            }
        }

        public void FlushValidLettersForTomorrow()
        {
            int tomorrow = Today() + 1;
            var due = _stateManager.State.PendingMedicalLetters
                .Where(p => p.DeliverAfterDay <= tomorrow)
                .ToList();

            foreach (PendingMedicalLetter letter in due)
            {
                _stateManager.State.PendingMedicalLetters.Remove(letter);

                if (!ValidatePendingLetter(letter, out string? skipReason))
                {
                    _monitor.Log(
                        $"[MedicalLetters] not sent (stale): mail={letter.MailId} reason={letter.Reason} state={letter.StateId} ({skipReason})",
                        LogLevel.Info);
                    continue;
                }

                Game1.addMailForTomorrow(letter.MailId);
                _monitor.Log(
                    $"[MedicalLetters] sent critical={letter.Critical} mail={letter.MailId} reason={letter.Reason}",
                    LogLevel.Info);
            }

            _stateManager.Save();
        }

        public void RemoveStalePendingLetters()
        {
            int today = Today();
            int removed = RemovePending(p => p.DeliverAfterDay < today - 1);

            if (removed > 0)
            {
                _stateManager.Save();
                _monitor.Log(
                    $"[MedicalLetters] removed {removed} expired pending letter(s)",
                    LogLevel.Debug);
            }
        }

        /// <summary>Удалить устаревшие medical mail IDs из mailForTomorrow (legacy direct queue).</summary>
        public void ScrubStaleMailForTomorrow()
        {
            if (Game1.player?.mailForTomorrow == null)
                return;

            var toRemove = new List<string>();
            foreach (string mailId in Game1.player.mailForTomorrow.ToList())
            {
                if (!IsManagedMedicalMail(mailId))
                    continue;

                string reason = InferReasonFromMailId(mailId);
                if (!IsLetterStillValid(new PendingMedicalLetter
                    {
                        MailId = mailId,
                        Reason = reason,
                        StateId = InferStateIdFromMailId(mailId, reason),
                    },
                    out _))
                {
                    toRemove.Add(mailId);
                }
            }

            foreach (string mailId in toRemove)
            {
                Game1.player.mailForTomorrow.Remove(mailId);
                _monitor.Log(
                    $"[MedicalLetters] removed stale mailForTomorrow: {mailId}, reason={InferReasonFromMailId(mailId)}",
                    LogLevel.Info);
            }
        }

        public bool ValidatePendingLetter(PendingMedicalLetter letter, out string? skipReason)
        {
            if (IsBlockedByHiddenInjury(letter.StateId, letter.Reason))
            {
                skipReason = "hidden injury, HarveyAware=false";
                return false;
            }

            if (!IsLetterStillValid(letter, out skipReason))
                return false;

            skipReason = null;
            return true;
        }

        public bool IsLetterStillValid(PendingMedicalLetter letter, out string? skipReason)
        {
            skipReason = null;

            return letter.Reason switch
            {
                MedicalLetterReasons.DirtyWound =>
                    HasComplication(InjuryBuffs.DirtyWound),

                MedicalLetterReasons.WetBandage or MedicalLetterReasons.WetCare =>
                    HasComplication(InjuryBuffs.WetBandage),

                MedicalLetterReasons.WetStitchesCare =>
                    HasComplication(InjuryBuffs.WetStitches),

                MedicalLetterReasons.InfectionDirty =>
                    HasComplication(InjuryBuffs.DirtyWound)
                    || _buffManager.HasBuff("buffInfectedWound"),

                MedicalLetterReasons.InfectionWet =>
                    HasComplication(InjuryBuffs.WetBandage)
                    || _buffManager.HasBuff("buffInfectedWound"),

                MedicalLetterReasons.SleepControl =>
                    IsSleepControlStillRelevant(),

                MedicalLetterReasons.MineForbidden =>
                    _buffManager.HasBuff(InjuryBuffs.MineForbidden)
                    || _stateManager.State.MineForbiddenAppliedDay >= 0
                    || _stateManager.State.MineWarningDay >= 0,

                MedicalLetterReasons.NeedAppointment =>
                    _doctorVisitReminderManager?.IsVisitNeeded() == true,

                MedicalLetterReasons.TreatmentPlan or MedicalLetterReasons.CheckupReminder
                    or MedicalLetterReasons.CheckupOverdue =>
                    IsInjuryTreatmentStillActive(letter.StateId),

                MedicalLetterReasons.TreatmentUrgent or MedicalLetterReasons.TreatmentFinal
                    or MedicalLetterReasons.NeglectWarning or MedicalLetterReasons.UntreatedInjury =>
                    IsInjuryTreatmentStillActive(letter.StateId)
                    || HasUntreatedInjuryNeedingVisit(letter.StateId),

                MedicalLetterReasons.PrescriptionViolation =>
                    _stateManager.State.ActivePrescriptions.Count > 0,

                MedicalLetterReasons.RehabReminder =>
                    !string.IsNullOrEmpty(_stateManager.State.ActiveRehabInjuryId),

                MedicalLetterReasons.RehabCompleted =>
                    false,

                MedicalLetterReasons.HospitalDischarge =>
                    _stateManager.State.LastHospitalDischargeDay == Today()
                    || _stateManager.State.NeedsHarveyAfterHospitalDischargeHomeEvent,

                _ => _config.MedicalLetters == MedicalLetterMode.All,
            };
        }

        private bool IsSleepControlStillRelevant()
        {
            if (Helpers.GameUtils.HasConversationTopic(ConversationTopics.PassedOutInTown))
                return true;

            if (_buffManager.HasBuff("buffSleepy"))
                return true;

            if (_stateManager.State.PassedOutInTownYesterday)
                return true;

            return false;
        }

        private bool IsInjuryTreatmentStillActive(string stateId)
        {
            if (string.IsNullOrWhiteSpace(stateId))
                return _stateManager.State.ActiveDebuffs.Count > 0;

            if (!_stateManager.State.ActiveDebuffs.ContainsKey(stateId))
                return false;

            var debuff = _stateManager.State.ActiveDebuffs[stateId];
            if (debuff.ReadyForRecovery && debuff.HarveyConversationHappened)
                return false;

            return _buffManager.HasBuff(stateId)
                || debuff.IsInTreatment
                || !debuff.TreatmentStarted;
        }

        private bool HasUntreatedInjuryNeedingVisit(string stateId)
        {
            if (string.IsNullOrWhiteSpace(stateId))
            {
                return _stateManager.State.ActiveDebuffs.Values.Any(d =>
                    !d.TreatmentStarted && _buffManager.HasBuff(d.BuffId));
            }

            var debuff = _stateManager.GetDebuffState(stateId);
            return debuff != null && !debuff.TreatmentStarted && _buffManager.HasBuff(stateId);
        }

        private bool HasComplication(string complicationId) =>
            _stateManager.State.ActiveComplications.ContainsKey(complicationId)
            || _buffManager.HasBuff(complicationId);

        private bool IsBlockedByHiddenInjury(string stateId, string reason)
        {
            if (string.IsNullOrWhiteSpace(stateId))
                return false;

            if (!stateId.StartsWith("buff", StringComparison.OrdinalIgnoreCase)
                && !InjurySets.KnownComplicationBuffIds.Contains(stateId))
                return false;

            DebuffState? debuff = _stateManager.GetDebuffState(stateId);
            if (debuff == null && InjurySets.KnownComplicationBuffIds.Contains(stateId))
                return false;

            if (debuff == null)
                return false;

            if (!debuff.HiddenFromHarvey || debuff.HarveyAware)
                return false;

            _monitor.Log(
                $"[MedicalLetters] blocked hidden injury mail: mailReason={reason} state={stateId} HarveyAware=false",
                LogLevel.Info);
            return true;
        }

        private static bool MailMatchesInjuryState(PendingMedicalLetter letter, string stateId)
        {
            if (string.Equals(letter.StateId, stateId, StringComparison.OrdinalIgnoreCase))
                return true;

            return letter.Reason switch
            {
                MedicalLetterReasons.TreatmentPlan => true,
                MedicalLetterReasons.CheckupReminder or MedicalLetterReasons.CheckupOverdue => true,
                MedicalLetterReasons.TreatmentUrgent or MedicalLetterReasons.TreatmentFinal
                    or MedicalLetterReasons.NeglectWarning => true,
                MedicalLetterReasons.InfectionDirty or MedicalLetterReasons.DirtyWound => true,
                MedicalLetterReasons.InfectionWet or MedicalLetterReasons.WetBandage => true,
                _ => false,
            };
        }

        private int RemovePending(Func<PendingMedicalLetter, bool> predicate)
        {
            var list = _stateManager.State.PendingMedicalLetters;
            int before = list.Count;
            list.RemoveAll(p => predicate(p));
            return before - list.Count;
        }

        private static int Today() => (int)Game1.stats.DaysPlayed;

        private static bool IsManagedMedicalMail(string mailId)
        {
            if (ManagedMedicalMailIds.Contains(mailId))
                return true;

            foreach (string baseId in ManagedMedicalMailIds)
            {
                if (mailId.StartsWith(baseId + "_", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return mailId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase)
                || mailId.StartsWith("mailHarvey", StringComparison.OrdinalIgnoreCase);
        }

        private static string InferReasonFromMailId(string mailId)
        {
            string baseId = StripTierSuffix(mailId);
            return baseId switch
            {
                MailIds.SleepControl => MedicalLetterReasons.SleepControl,
                MailIds.MineForbidden => MedicalLetterReasons.MineForbidden,
                MailIds.DirtyWoundInfection => MedicalLetterReasons.InfectionDirty,
                MailIds.WetBandageInfection => MedicalLetterReasons.InfectionWet,
                MailIds.TreatmentUrgentReminder => MedicalLetterReasons.TreatmentUrgent,
                MailIds.TreatmentFinalWarning => MedicalLetterReasons.TreatmentFinal,
                MailIds.NeglectWarning => MedicalLetterReasons.NeglectWarning,
                MailIds.CheckupReminder => MedicalLetterReasons.CheckupReminder,
                MailIds.CheckupOverdue => MedicalLetterReasons.CheckupOverdue,
                MailIds.WetCare => MedicalLetterReasons.WetCare,
                MailIds.WetStitchesCare => MedicalLetterReasons.WetStitchesCare,
                _ when baseId.StartsWith("mailHarveyTreatmentPlan", StringComparison.OrdinalIgnoreCase)
                    => MedicalLetterReasons.TreatmentPlan,
                _ when baseId.StartsWith("mailHarveyPrescriptionViolation", StringComparison.OrdinalIgnoreCase)
                    || baseId.StartsWith("mailHarveyNoMineViolation", StringComparison.OrdinalIgnoreCase)
                    || baseId.StartsWith("mailHarveyKeepDryViolation", StringComparison.OrdinalIgnoreCase)
                    || baseId.StartsWith("mailHarveyRestViolation", StringComparison.OrdinalIgnoreCase)
                    => MedicalLetterReasons.PrescriptionViolation,
                _ when baseId.StartsWith("mailHarveyRehab", StringComparison.OrdinalIgnoreCase)
                    => MedicalLetterReasons.RehabReminder,
                _ => "",
            };
        }

        private string InferStateIdFromMailId(string mailId, string reason) =>
            reason switch
            {
                MedicalLetterReasons.TreatmentPlan or MedicalLetterReasons.CheckupOverdue
                    or MedicalLetterReasons.CheckupReminder or MedicalLetterReasons.TreatmentUrgent
                    or MedicalLetterReasons.TreatmentFinal or MedicalLetterReasons.NeglectWarning
                    => _stateManager.State.MainInjuryId ?? "",
                _ => "",
            };

        private static string StripTierSuffix(string mailId)
        {
            string[] suffixes = { "_Married", "_Dating", "_MidHearts", "_LowHearts", "_Friend", "_Neutral" };
            foreach (string suffix in suffixes)
            {
                if (mailId.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return mailId[..^suffix.Length];
            }

            return mailId;
        }
    }
}
