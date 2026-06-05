using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Testing
{
    /// <summary>
    /// Read-only QA dump-команды: структурированные отчёты для MCP/assert.
    /// </summary>
    internal static class QaDumpCommands
    {
        public static string BuildStateDump(InjuryState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"MainInjuryId={Fmt(state.MainInjuryId)}");
            sb.AppendLine($"TreatmentComplianceScore={state.TreatmentComplianceScore}");
            sb.AppendLine($"DaysWithSevere={state.DaysWithSevere}");

            AppendDebuffsBlock(sb, state);
            AppendComplicationsBlock(sb, state);
            AppendPrescriptionsBlock(sb, state);
            AppendRehabBlock(sb, state);
            AppendMineBlock(sb, state);
            AppendPassOutBlock(sb, state);
            AppendHospitalBlock(sb, state);
            AppendRainBlock(sb, state);
            AppendNeglectBlock(sb, state);
            AppendMiscBlock(sb, state);

            return sb.ToString().TrimEnd();
        }

        public static string BuildBuffDump(
            BuffManager buffManager,
            InjuryManager injuryManager,
            InjuryState state,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas,
            (string BuffId, string TopicId)[] knownComplications)
        {
            var traumaIds = new HashSet<string>(
                knownTraumas.Select(t => t.BuffId),
                StringComparer.OrdinalIgnoreCase);
            var complicationIds = new HashSet<string>(
                knownComplications.Select(c => c.BuffId),
                StringComparer.OrdinalIgnoreCase);
            var cureIds = new HashSet<string>(CollectCureBuffIds(), StringComparer.OrdinalIgnoreCase);
            var prescriptionIds = new HashSet<string>(CollectPrescriptionBuffIds(), StringComparer.OrdinalIgnoreCase);
            var phaseIds = BuildPhaseBuffIdSet(injuryManager, knownTraumas);
            var debuffStateIds = new HashSet<string>(
                state.ActiveDebuffs.Keys,
                StringComparer.OrdinalIgnoreCase);

            var activeBuffs = buffManager.GetActiveBuffs()
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"count={activeBuffs.Count}");
            sb.AppendLine($"SavedActiveBuffs={FmtList(state.SavedActiveBuffs)}");

            foreach (string buffId in activeBuffs)
            {
                var tags = new List<string>();
                if (buffManager.BuffExists(buffId))
                    tags.Add("mod");
                if (traumaIds.Contains(buffId))
                    tags.Add("trauma");
                if (complicationIds.Contains(buffId))
                    tags.Add("complication");
                if (phaseIds.Contains(buffId))
                    tags.Add("phase");
                if (cureIds.Contains(buffId))
                    tags.Add("cure");
                if (prescriptionIds.Contains(buffId))
                    tags.Add("prescription");
                if (!debuffStateIds.Contains(buffId)
                    && !traumaIds.Contains(buffId)
                    && !complicationIds.Contains(buffId))
                    tags.Add("orphan");

                string tagStr = tags.Count > 0 ? string.Join(",", tags) : "vanilla";
                sb.AppendLine($"buff={buffId} tags={tagStr}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string BuildTopicDump()
        {
            var dict = Game1.player?.activeDialogueEvents;
            if (dict == null)
                return "count=0";

            var owned = ModTopicRegistry.GetAllOwnedTopicIds();
            var allTopics = dict.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"count={allTopics.Count}");
            sb.AppendLine("--- all ---");
            foreach (string topicId in allTopics)
            {
                dict.TryGetValue(topicId, out int days);
                string ownedFlag = owned.Contains(topicId) ? "owned" : "foreign";
                sb.AppendLine($"topic={topicId} days={days} {ownedFlag}");
            }

            AppendTopicSection(sb, allTopics, "topic", id => id.StartsWith("topic", StringComparison.Ordinal));
            AppendTopicSection(sb, allTopics, "HarveyMod", id => id.StartsWith("HarveyMod", StringComparison.Ordinal));
            AppendTopicSection(sb, allTopics, "owned", id => owned.Contains(id));

            return sb.ToString().TrimEnd();
        }

        public static string BuildValidateBuffsReport(
            BuffManager buffManager,
            InjuryManager injuryManager,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas,
            (string BuffId, string TopicId)[] knownComplications)
        {
            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var trauma in knownTraumas)
                expected.Add(trauma.BuffId);

            foreach (var comp in knownComplications)
                expected.Add(comp.BuffId);

            foreach (string cureId in CollectCureBuffIds())
                expected.Add(cureId);

            foreach (var trauma in knownTraumas)
            {
                for (int phase = 1; phase <= 3; phase++)
                {
                    string phaseBuffId = injuryManager.GetPhaseBuffId(trauma.BuffId, phase);
                    if (!string.IsNullOrEmpty(phaseBuffId))
                        expected.Add(phaseBuffId);
                }
            }

            var missing = expected
                .Where(id => !buffManager.BuffExists(id))
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            if (missing.Count == 0)
                return "result=OK checked=" + expected.Count;

            return $"result=MISSING missing_count={missing.Count} ids={string.Join(", ", missing)} checked={expected.Count}";
        }

        private static void AppendDebuffsBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"ActiveDebuffs.count={state.ActiveDebuffs.Count}");
            foreach (var (buffId, ds) in state.ActiveDebuffs.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            {
                string p = $"ActiveDebuffs.{buffId}";
                sb.AppendLine($"{p}.BuffId={ds.BuffId}");
                sb.AppendLine($"{p}.InjuryStartDay={ds.InjuryStartDay}");
                sb.AppendLine($"{p}.TreatmentStarted={ds.TreatmentStarted}");
                sb.AppendLine($"{p}.HarveyConversationHappened={ds.HarveyConversationHappened}");
                sb.AppendLine($"{p}.TotalPhases={ds.TotalPhases}");
                sb.AppendLine($"{p}.CurrentPhase={ds.CurrentPhase}");
                sb.AppendLine($"{p}.PhaseStartDay={ds.PhaseStartDay}");
                sb.AppendLine($"{p}.Phase1Duration={ds.Phase1Duration}");
                sb.AppendLine($"{p}.Phase2Duration={ds.Phase2Duration}");
                sb.AppendLine($"{p}.Phase3Duration={ds.Phase3Duration}");
                sb.AppendLine($"{p}.ReadyForNextPhase={ds.ReadyForNextPhase}");
                sb.AppendLine($"{p}.ReadyForRecovery={ds.ReadyForRecovery}");
                sb.AppendLine($"{p}.ReadySinceDay={ds.ReadySinceDay}");
                sb.AppendLine($"{p}.MissedCheckupDays={ds.MissedCheckupDays}");
                sb.AppendLine($"{p}.CheckupReminderSent={ds.CheckupReminderSent}");
                sb.AppendLine($"{p}.CheckupLateLetterSent={ds.CheckupLateLetterSent}");
                sb.AppendLine($"{p}.CheckupOverduePenaltyApplied={ds.CheckupOverduePenaltyApplied}");
            }
        }

        private static void AppendComplicationsBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"ActiveComplications.count={state.ActiveComplications.Count}");
            foreach (var (compId, startDay) in state.ActiveComplications.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"ActiveComplications.{compId}={startDay}");
            sb.AppendLine($"LastInfectionEscalationDay={state.LastInfectionEscalationDay}");
        }

        private static void AppendPrescriptionsBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"ActivePrescriptions.count={state.ActivePrescriptions.Count}");
            foreach (var (id, ps) in state.ActivePrescriptions.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            {
                string p = $"ActivePrescriptions.{id}";
                sb.AppendLine($"{p}.SourceInjuryId={Fmt(ps.SourceInjuryId)}");
                sb.AppendLine($"{p}.StartDay={ps.StartDay}");
                sb.AppendLine($"{p}.DurationDays={ps.DurationDays}");
                sb.AppendLine($"{p}.IsViolated={ps.IsViolated}");
                sb.AppendLine($"{p}.ViolationCount={ps.ViolationCount}");
                sb.AppendLine($"{p}.LastViolationDay={ps.LastViolationDay}");
            }
            sb.AppendLine($"LastPrescriptionReminderDay={state.LastPrescriptionReminderDay}");
            sb.AppendLine($"LastCheckupComplianceDay={state.LastCheckupComplianceDay}");
            sb.AppendLine($"LastLowComplianceHudDay={state.LastLowComplianceHudDay}");
        }

        private static void AppendRehabBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"ActiveRehabInjuryId={Fmt(state.ActiveRehabInjuryId)}");
            sb.AppendLine($"RehabStartDay={state.RehabStartDay}");
            sb.AppendLine($"RehabDurationDays={state.RehabDurationDays}");
            sb.AppendLine($"RehabViolated={state.RehabViolated}");
            sb.AppendLine($"RehabViolationCount={state.RehabViolationCount}");
            sb.AppendLine($"LastRehabViolationDay={state.LastRehabViolationDay}");
        }

        private static void AppendMineBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"MineWarningDay={state.MineWarningDay}");
            sb.AppendLine($"LastMineSevereWarningDay={state.LastMineSevereWarningDay}");
            sb.AppendLine($"LastMineSevereForcedExitDay={state.LastMineSevereForcedExitDay}");
            sb.AppendLine($"MineForbiddenAppliedDay={state.MineForbiddenAppliedDay}");
            sb.AppendLine($"LastMineForbiddenInterceptionDay={state.LastMineForbiddenInterceptionDay}");
            sb.AppendLine($"MineRestrictionViolationsToday={state.MineRestrictionViolationsToday}");
            sb.AppendLine($"MineRestrictionStrikes={state.MineRestrictionStrikes}");
            sb.AppendLine($"MineDirtyExposureMinutesToday={state.MineDirtyExposureMinutesToday}");
            sb.AppendLine($"LastMineDirtyExposureDay={state.LastMineDirtyExposureDay}");
            sb.AppendLine($"LastMineDirtyWoundRollMinute={state.LastMineDirtyWoundRollMinute}");
            sb.AppendLine($"MineDirtyRiskBoostUntilMinute={state.MineDirtyRiskBoostUntilMinute}");
            sb.AppendLine($"PassedOutInMineYesterday={state.PassedOutInMineYesterday}");
            sb.AppendLine($"NeedsMineRescueEvent={state.NeedsMineRescueEvent}");
            sb.AppendLine($"PendingMineRescueEventId={Fmt(state.PendingMineRescueEventId)}");
            sb.AppendLine($"PendingMinorMineRescueEventId={Fmt(state.PendingMinorMineRescueEventId)}");
            sb.AppendLine($"LastMinorMineRescueDay={state.LastMinorMineRescueDay}");
        }

        private static void AppendPassOutBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"WasPassedOut={state.WasPassedOut}");
            sb.AppendLine($"WasExhausted={state.WasExhausted}");
            sb.AppendLine($"WasUpTooLate={state.WasUpTooLate}");
            sb.AppendLine($"LastPassedOutHealth={state.LastPassedOutHealth}");
            sb.AppendLine($"LastPassedOutLocation={Fmt(state.LastPassedOutLocation)}");
            sb.AppendLine($"PassedOutInTownYesterday={state.PassedOutInTownYesterday}");
            sb.AppendLine($"PendingHospitalPassOutEventId={Fmt(state.PendingHospitalPassOutEventId)}");
            sb.AppendLine($"PendingHospitalPassOutFallbackKind={Fmt(state.PendingHospitalPassOutFallbackKind)}");
        }

        private static void AppendHospitalBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"IsHospitalized={state.IsHospitalized}");
            sb.AppendLine($"HospitalizedInjuryId={Fmt(state.HospitalizedInjuryId)}");
            sb.AppendLine($"HospitalizationReason={Fmt(state.HospitalizationReason)}");
            sb.AppendLine($"HospitalAdmissionDay={state.HospitalAdmissionDay}");
            sb.AppendLine($"HospitalAdmissionTime={state.HospitalAdmissionTime}");
            sb.AppendLine($"HospitalAdmissionMinutes={state.HospitalAdmissionMinutes}");
            sb.AppendLine($"HospitalMinStayMinutes={state.HospitalMinStayMinutes}");
            sb.AppendLine($"HospitalDischargeReadyShown={state.HospitalDischargeReadyShown}");
            sb.AppendLine($"PendingForcedHospitalizationWarning={state.PendingForcedHospitalizationWarning}");
            sb.AppendLine($"PendingForcedHospitalizationWarningDay={state.PendingForcedHospitalizationWarningDay}");
            sb.AppendLine($"NeedsHarveyAfterHospitalDischargeHomeEvent={state.NeedsHarveyAfterHospitalDischargeHomeEvent}");
            sb.AppendLine($"LastHospitalDischargeDay={state.LastHospitalDischargeDay}");
            sb.AppendLine($"LastHospitalDischargeInjuryId={Fmt(state.LastHospitalDischargeInjuryId)}");
            sb.AppendLine($"HarveyAfterHospitalDischargeShownDay={state.HarveyAfterHospitalDischargeShownDay}");
        }

        private static void AppendRainBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"TimeUnderRainTicks={state.TimeUnderRainTicks}");
            sb.AppendLine($"LastRainCheckTime={state.LastRainCheckTime}");
            sb.AppendLine($"TotalTimeUnderRainToday={state.TotalTimeUnderRainToday}");
            sb.AppendLine($"LastRainDay={state.LastRainDay}");
        }

        private static void AppendNeglectBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"NeglectStrikes={state.NeglectStrikes}");
            sb.AppendLine($"NeglectStrikesByInjury.count={state.NeglectStrikesByInjury.Count}");
            foreach (var (injuryId, strikes) in state.NeglectStrikesByInjury.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"NeglectStrikesByInjury.{injuryId}={strikes}");
        }

        private static void AppendMiscBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"AppliedTriggers={FmtList(state.AppliedTriggers)}");
            sb.AppendLine($"InjuryCooldownUntilDay.count={state.InjuryCooldownUntilDay.Count}");
            foreach (var (key, day) in state.InjuryCooldownUntilDay.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"InjuryCooldownUntilDay.{key}={day}");
            sb.AppendLine($"SavedActiveBuffs={FmtList(state.SavedActiveBuffs)}");
            sb.AppendLine($"SelfCareProtections.count={state.SelfCareProtections.Count}");
            foreach (var (key, day) in state.SelfCareProtections.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"SelfCareProtections.{key}={day}");
            sb.AppendLine($"PendingSelfCareBandageCompliance={state.PendingSelfCareBandageCompliance}");
            sb.AppendLine($"LastSelfCareBandageDay={state.LastSelfCareBandageDay}");
            sb.AppendLine($"LastSelfCareTeaDay={state.LastSelfCareTeaDay}");
            sb.AppendLine($"LastRestSelfCareDay={state.LastRestSelfCareDay}");
            sb.AppendLine($"SentMedicalMailDays.count={state.SentMedicalMailDays.Count}");
            foreach (var (key, day) in state.SentMedicalMailDays.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"SentMedicalMailDays.{key}={day}");
            sb.AppendLine($"LastNightRoundDay={state.LastNightRoundDay}");
            sb.AppendLine($"LastNightRoundRollDay={state.LastNightRoundRollDay}");
            sb.AppendLine($"NeedsSevereNightRoundEvent={state.NeedsSevereNightRoundEvent}");
            sb.AppendLine($"SevereNightRoundEventShownDay={state.SevereNightRoundEventShownDay}");
            sb.AppendLine($"SevereNightRoundInjuryId={Fmt(state.SevereNightRoundInjuryId)}");
            sb.AppendLine($"LastStormComfortRollDay={state.LastStormComfortRollDay}");
            sb.AppendLine($"LastStormComfortEventDay={state.LastStormComfortEventDay}");
            sb.AppendLine($"LastProximityCheckDay={state.LastProximityCheckDay}");
            sb.AppendLine($"LastSupportDay={state.LastSupportDay}");
            sb.AppendLine($"LastProximityReactionMinute={state.LastProximityReactionMinute}");
            sb.AppendLine($"LastStrictReactionDay={state.LastStrictReactionDay}");
            sb.AppendLine($"LastProximityReactionReason={Fmt(state.LastProximityReactionReason)}");
            sb.AppendLine($"LastHealth={state.LastHealth}");
            sb.AppendLine($"WetBandageMailDay={state.WetBandageMailDay}");
            sb.AppendLine($"WetStitchesMailDay={state.WetStitchesMailDay}");
        }

        private static void AppendTopicSection(
            StringBuilder sb,
            List<string> allTopics,
            string sectionName,
            Func<string, bool> filter)
        {
            var dict = Game1.player!.activeDialogueEvents!;
            var keys = allTopics.Where(filter).ToList();
            sb.AppendLine($"--- {sectionName} count={keys.Count} ---");
            foreach (string topicId in keys)
            {
                dict.TryGetValue(topicId, out int days);
                sb.AppendLine($"topic={topicId} days={days}");
            }
        }

        private static HashSet<string> BuildPhaseBuffIdSet(
            InjuryManager injuryManager,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var trauma in knownTraumas)
            {
                for (int phase = 1; phase <= 3; phase++)
                {
                    string phaseBuffId = injuryManager.GetPhaseBuffId(trauma.BuffId, phase);
                    if (!string.IsNullOrEmpty(phaseBuffId))
                        set.Add(phaseBuffId);
                }
            }
            return set;
        }

        private static IEnumerable<string> CollectCureBuffIds()
        {
            foreach (var field in typeof(CureBuffs).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.GetValue(null) is string id)
                    yield return id;
            }

            foreach (string id in SimpleInjuryCures.Map.Values)
                yield return id;
        }

        private static IEnumerable<string> CollectPrescriptionBuffIds()
        {
            foreach (var field in typeof(PrescriptionIds).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.GetValue(null) is string id)
                    yield return id;
            }
        }

        private static string Fmt(string? value) =>
            string.IsNullOrEmpty(value) ? "(none)" : value;

        private static string FmtList(IEnumerable<string> items)
        {
            var list = items?.ToList() ?? new List<string>();
            return list.Count == 0 ? "(none)" : string.Join(",", list.OrderBy(x => x, StringComparer.Ordinal));
        }
    }
}
