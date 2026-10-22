using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.InjuryCare.Api;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.EventHandlers;
using HarveyOverhaul.InjuryCare.Services;
using HarveyOverhaul.InjuryCare.Testing;
using HarveyOverhaul.InjuryCare.UI.RecoveryPlan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buffs;

namespace HarveyOverhaul.InjuryCare
{
    /// <summary>Отчёты состояния и отладочный HUD.</summary>
    public sealed partial class ModEntry
    {
        private string YesNo(bool value) => value ? "YES" : "no";

        private static string FormatNeglectStrikes(InjuryState state)
        {
            if (state.NeglectStrikesByInjury == null || state.NeglectStrikesByInjury.Count == 0)
                return state.NeglectStrikes > 0 ? $"legacy={state.NeglectStrikes}" : "(none)";

            return string.Join(
                ", ",
                state.NeglectStrikesByInjury.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        private string Warn(bool ok, string text) => ok ? text : $"⚠ {text}";

        private string GetBaseTopicForBuff(string buffId)
        {
            var trauma = KnownTraumas.FirstOrDefault(t => t.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));
            if (trauma.BuffId != null)
                return trauma.TopicId;

            var comp = KnownComplications.FirstOrDefault(c => c.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));
            if (comp.BuffId != null)
                return comp.TopicId;

            return buffId.Replace("buff", "topic");
        }

        private string GetPhaseName(string buffId, int phase, int totalPhases) =>
            _injuryManager.GetPhaseStageName(buffId, phase, totalPhases);

        private string GetExpectedPhaseTopic(string buffId, int phase, int totalPhases = 0) =>
            TopicIds.GetPhaseTopicId(buffId, phase, totalPhases > 0
                ? totalPhases
                : (_stateManager.GetDebuffState(buffId)?.TotalPhases ?? InjurySets.InferDefaultTotalPhases(buffId)));

        private string GetExpectedPhaseBuff(string buffId, int phase) =>
            phase > 0 ? _injuryManager.GetPhaseBuffId(buffId, phase) : "";

        private bool HasTopic(string topicId) =>
            Game1.player.activeDialogueEvents?.ContainsKey(topicId) == true;

        private string TopicLeft(string topicId)
        {
            if (!HasTopic(topicId))
                return "missing";

            Game1.player.activeDialogueEvents!.TryGetValue(topicId, out int days);
            return $"{days}d";
        }

        private string GetClickExpectationForDebuff(DebuffState d)
        {
            if (!d.TreatmentStarted && _buffManager.HasBuff(d.BuffId))
                return $"TALK: {TopicIds.GetTreatmentNeededTopic(d.BuffId)}";
            if (d.TreatmentStarted && d.ReadyForRecovery)
                return "CLICK: complete recovery";
            if (d.TreatmentStarted && d.IsPhasedInjury && d.ReadyForNextPhase
                && d.CurrentPhase > 0 && d.CurrentPhase < d.TotalPhases)
                return "CLICK: next phase";
            if (d.TreatmentStarted)
                return "WAIT";
            return "BROKEN";
        }

        private bool IsKnownComplication(string buffId) =>
            KnownComplications.Any(c => c.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));

        private string GetInjuryStateLabel(DebuffState d)
        {
            if (IsKnownComplication(d.BuffId))
                return "complication";
            if (!d.TreatmentStarted)
                return "untreated";
            if (d.IsPhasedInjury && d.CurrentPhase > 0)
                return $"treatment phase {d.CurrentPhase}/{d.TotalPhases} {GetPhaseName(d.BuffId, d.CurrentPhase, d.TotalPhases)}";
            return "simple-treatment";
        }

        private string FormatTopicStatus(string topicId) =>
            HasTopic(topicId) ? $"{topicId} {TopicLeft(topicId)}" : $"{topicId} missing";

        private List<string> CollectDebuffIssues(
            DebuffState d,
            bool hasBaseBuff,
            string phaseBuffExpected,
            bool hasPhaseBuff)
        {
            var issues = new List<string>();
            bool expectPhaseBuff = d.TreatmentStarted && d.CurrentPhase > 0 && d.IsPhasedInjury;
            string baseTopic = GetBaseTopicForBuff(d.BuffId);

            if (!hasBaseBuff && !hasPhaseBuff)
                issues.Add("STATE_WITHOUT_BUFF");

            if (!d.TreatmentStarted && !hasBaseBuff)
                issues.Add("UNTREATED_WITHOUT_BASE_BUFF");

            if (d.TreatmentStarted && d.CurrentPhase <= 0)
                issues.Add("TREATMENT_STARTED_PHASE_ZERO");

            if (d.TreatmentStarted && d.TotalPhases > 0 && d.CurrentPhase > d.TotalPhases)
                issues.Add("PHASE_OUT_OF_RANGE");

            if (expectPhaseBuff && !string.IsNullOrEmpty(phaseBuffExpected) && !hasPhaseBuff)
                issues.Add("PHASE_BUFF_MISSING");

            if (d.ReadyForNextPhase && (d.TotalPhases == 0 || TreatmentManager.IsSimpleTreatmentInjury(d.BuffId)))
                issues.Add("INVALID_READY_FOR_SIMPLE_TREATMENT");

            if (d.ReadyForNextPhase && d.ReadyForRecovery)
                issues.Add("BOTH_READY_FLAGS");

            if (d.ReadyForNextPhase && d.IsLastPhase)
                issues.Add("NEXT_READY_ON_LAST_PHASE");

            if (d.ReadyForRecovery && !d.IsLastPhase)
                issues.Add("RECOVERY_READY_BEFORE_LAST_PHASE");

            if (!d.TreatmentStarted && !HasTopic(baseTopic))
                issues.Add("BASE_TOPIC_MISSING");

            if (d.TreatmentStarted && d.IsPhasedInjury && d.CurrentPhase > 0)
            {
                string phaseTopic = GetExpectedPhaseTopic(d.BuffId, d.CurrentPhase, d.TotalPhases);
                if (!HasTopic(phaseTopic))
                    issues.Add("PHASE_TOPIC_MISSING");
            }

            return issues;
        }

        private bool IsKnownTraumaActiveInMatrix(
            (string BuffId, string TopicId, int P1, int P2, int P3) trauma,
            InjuryState state)
        {
            if (state.ActiveDebuffs.ContainsKey(trauma.BuffId))
                return true;

            if (_buffManager.HasBuff(trauma.BuffId))
                return true;

            for (int phase = 1; phase <= 3; phase++)
            {
                string phaseBuff = GetExpectedPhaseBuff(trauma.BuffId, phase);
                if (!string.IsNullOrEmpty(phaseBuff) && _buffManager.HasBuff(phaseBuff))
                    return true;
            }

            if (HasTopic(trauma.TopicId))
                return true;

            int matrixTotalPhases = trauma.P3 > 0 ? 3 : (trauma.P2 > 0 ? 2 : 0);
            for (int phase = 1; phase <= 3; phase++)
            {
                if (HasTopic(GetExpectedPhaseTopic(trauma.BuffId, phase, matrixTotalPhases)))
                    return true;
            }

            string curedTopic = $"topic{trauma.BuffId.Replace("buff", "")}Cured";
            return HasTopic(curedTopic);
        }

        private void AppendKnownTraumasMatrixBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== KNOWN TRAUMAS MATRIX ===");
            bool any = false;

            foreach (var trauma in KnownTraumas)
            {
                if (!IsKnownTraumaActiveInMatrix(trauma, state))
                    continue;

                any = true;
                bool hasState = state.ActiveDebuffs.ContainsKey(trauma.BuffId);
                bool hasBaseBuff = _buffManager.HasBuff(trauma.BuffId);
                string? triggerMatch = FindMatchingAppliedTrigger(trauma.BuffId, trauma.TopicId, state.AppliedTriggers);
                string triggerHint = triggerMatch != null ? $"trigger {triggerMatch} maybe" : "trigger no";

                sb.AppendLine(
                    $"{trauma.BuffId} | state {YesNo(hasState)} | baseBuff {YesNo(hasBaseBuff)} | topic {TopicLeft(trauma.TopicId)} | phases {trauma.P1}/{trauma.P2}/{trauma.P3} | {triggerHint}");

                if (trauma.P1 + trauma.P2 + trauma.P3 <= 0)
                    continue;

                int[] phaseDurations = { trauma.P1, trauma.P2, trauma.P3 };
                int totalPhases = phaseDurations[2] > 0 ? 3 : (phaseDurations[1] > 0 ? 2 : 0);
                for (int phase = 1; phase <= 3; phase++)
                {
                    if (phaseDurations[phase - 1] <= 0)
                        continue;

                    string phaseBuff = GetExpectedPhaseBuff(trauma.BuffId, phase);
                    string phaseTopic = GetExpectedPhaseTopic(trauma.BuffId, phase, totalPhases);
                    sb.AppendLine(
                        $"  P{phase} {phaseBuff} buff {YesNo(_buffManager.HasBuff(phaseBuff))} topic {phaseTopic} {TopicLeft(phaseTopic)}");
                }
            }

            if (!any)
                sb.AppendLine("  (none active)");
        }

        private void AppendInjuriesDiagnosticBlock(StringBuilder sb, InjuryState state, int today)
        {
            sb.AppendLine("=== MAIN INJURY ===");
            AppendMainInjuryDebugBlock(sb, state);

            sb.AppendLine("=== INJURIES ===");
            if (state.ActiveDebuffs.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (buffId, d) in state.ActiveDebuffs)
            {
                bool hasBaseBuff = _buffManager.HasBuff(buffId);
                string phaseBuffExpected = GetExpectedPhaseBuff(buffId, d.CurrentPhase);
                bool hasPhaseBuff = !string.IsNullOrEmpty(phaseBuffExpected) && _buffManager.HasBuff(phaseBuffExpected);
                var issues = CollectDebuffIssues(d, hasBaseBuff, phaseBuffExpected, hasPhaseBuff);
                string status = issues.Count == 0 ? "OK" : "WARN";

                sb.AppendLine($"[{status}] {buffId}");
                sb.AppendLine($"  state: {GetInjuryStateLabel(d)}");

                int injuryDays = today - d.InjuryStartDay;
                if (d.TreatmentStarted && d.CurrentPhase > 0)
                {
                    int phaseDays = today - d.PhaseStartDay;
                    int needDays = d.GetCurrentPhaseDuration();
                    sb.AppendLine($"  days: injury +{injuryDays}, phase +{phaseDays} / need {needDays}");
                }
                else
                {
                    sb.AppendLine($"  days: injury +{injuryDays}");
                }

                string phaseBuffLabel = string.IsNullOrEmpty(phaseBuffExpected) ? "n/a" : phaseBuffExpected;
                sb.AppendLine($"  buffs: base {YesNo(hasBaseBuff)}, phase {phaseBuffLabel} {YesNo(hasPhaseBuff)}");

                string baseTopic = GetBaseTopicForBuff(buffId);
                string phaseTopicExpected = d.CurrentPhase > 0
                    ? GetExpectedPhaseTopic(buffId, d.CurrentPhase, d.TotalPhases)
                    : "";
                if (!string.IsNullOrEmpty(phaseTopicExpected))
                {
                    sb.AppendLine($"  topics: base {FormatTopicStatus(baseTopic)}, phase {FormatTopicStatus(phaseTopicExpected)}");
                }
                else
                {
                    sb.AppendLine($"  topics: base {FormatTopicStatus(baseTopic)}");
                }

                sb.AppendLine($"  flags: Next {YesNo(d.ReadyForNextPhase)}, Recovery {YesNo(d.ReadyForRecovery)}, HarveyTalk {YesNo(d.HarveyConversationHappened)}");
                sb.AppendLine(
                    $"  checkup: since={d.ReadySinceDay} missed={d.MissedCheckupDays} reminder={YesNo(d.CheckupReminderSent)}");
                sb.AppendLine($"  click: {GetClickExpectationForDebuff(d)}");
                sb.AppendLine(issues.Count == 0
                    ? "  issues: none"
                    : $"  issues: {string.Join(", ", issues)}");
            }
        }

        private static bool IsModConversationTopic(string topicId) =>
            topicId.StartsWith("topic", StringComparison.Ordinal)
            || topicId.StartsWith("situation", StringComparison.Ordinal)
            || topicId.StartsWith("HarveyMod_CD_", StringComparison.Ordinal);

        private List<string> CollectActiveModTopics()
        {
            var dict = Game1.player?.activeDialogueEvents;
            if (dict == null)
                return new List<string>();

            return dict.Keys
                .Where(IsModConversationTopic)
                .OrderBy(t => t, StringComparer.Ordinal)
                .ToList();
        }

        private void AppendTopicsWatchedBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== ACTIVE TOPICS ===");
            var active = CollectActiveModTopics();
            if (active.Count == 0)
            {
                sb.AppendLine("(none)");
                return;
            }

            sb.AppendLine($"count: {active.Count}");
            var dict = Game1.player!.activeDialogueEvents!;
            foreach (var topicId in active)
            {
                dict.TryGetValue(topicId, out int days);
                sb.AppendLine($"{topicId} ({days}d)");
            }
        }

        private static readonly string[] WatchedExtraBuffIds =
        {
            CureBuffs.Teracitin,
            CureBuffs.Antibiotics,
            CureBuffs.ForcedSedation,
            CureBuffs.PostSurgical,
            InjuryBuffs.Cold,
        };

        private bool IsKnownTraumaBuff(string buffId) =>
            buffId.StartsWith("buff", StringComparison.Ordinal)
            && KnownTraumas.Any(t => t.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));

        private bool IsModWatchedBuff(string buffId)
        {
            if (IsKnownTraumaBuff(buffId))
                return true;
            if (buffId.StartsWith("HarveyMod_", StringComparison.Ordinal))
                return true;
            if (buffId.StartsWith("buffHarvey", StringComparison.Ordinal))
                return true;
            return WatchedExtraBuffIds.Any(id => id.Equals(buffId, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> CollectActiveModWatchedBuffs() =>
            _buffManager.GetActiveBuffs()
                .Where(IsModWatchedBuff)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

        private void AppendBuffsWatchedBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== BUFFS WATCHED ===");

            foreach (var buffId in state.ActiveDebuffs.Keys)
            {
                bool hasBase = _buffManager.HasBuff(buffId);
                string phaseExpected = GetExpectedPhaseBuff(buffId, state.ActiveDebuffs[buffId].CurrentPhase);
                bool hasPhase = !string.IsNullOrEmpty(phaseExpected) && _buffManager.HasBuff(phaseExpected);

                sb.AppendLine($"base {buffId} = {YesNo(hasBase)}");
                if (!string.IsNullOrEmpty(phaseExpected))
                    sb.AppendLine($"phase {phaseExpected} = {YesNo(hasPhase)}");

                if (!hasBase && !hasPhase)
                    sb.AppendLine($"⚠ DESYNC: {buffId} exists in state but no matching active buff");
            }

            var active = CollectActiveModWatchedBuffs();
            sb.AppendLine(active.Count == 0
                ? "active mod buffs: (none)"
                : $"active mod buffs: {string.Join(", ", active)}");
        }

        private const int MaxAppliedTriggersShown = 20;

        private void AppendAppliedTriggersBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== APPLIED TRIGGERS ===");
            var sorted = state.AppliedTriggers.OrderBy(t => t, StringComparer.Ordinal).ToList();
            sb.AppendLine($"count: {sorted.Count}");
            if (sorted.Count == 0)
                return;

            foreach (var triggerId in sorted.Take(MaxAppliedTriggersShown))
                sb.AppendLine(triggerId);

            int remaining = sorted.Count - MaxAppliedTriggersShown;
            if (remaining > 0)
                sb.AppendLine($"... +{remaining} more");
        }

        private static string? FindMatchingAppliedTrigger(
            string buffId,
            string topicId,
            IEnumerable<string> appliedTriggers)
        {
            string buffKey = buffId.Replace("buff", "", StringComparison.Ordinal);
            string topicKey = topicId.Replace("topic", "", StringComparison.Ordinal);

            foreach (var trigger in appliedTriggers)
            {
                if (!string.IsNullOrEmpty(buffKey)
                    && trigger.Contains(buffKey, StringComparison.OrdinalIgnoreCase))
                    return trigger;

                if (!string.IsNullOrEmpty(topicKey)
                    && trigger.Contains(topicKey, StringComparison.OrdinalIgnoreCase))
                    return trigger;
            }

            return null;
        }

        private void AppendBlockedKnownTraumasBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== BLOCKED KNOWN TRAUMAS ===");
            bool any = false;

            foreach (var trauma in KnownTraumas)
            {
                string? match = FindMatchingAppliedTrigger(trauma.BuffId, trauma.TopicId, state.AppliedTriggers);
                if (match == null)
                    continue;

                any = true;
                sb.AppendLine($"{trauma.BuffId}: possibly blocked by {match}");
            }

            if (!any)
                sb.AppendLine("  (no obvious trigger matches)");
        }

        private int CountActiveWatchedTopics(InjuryState state) =>
            CollectActiveModTopics().Count;

        private void AppendCompactInjuriesSummary(StringBuilder sb, InjuryState state)
        {
            AppendMainInjuryDebugBlock(sb, state);
            sb.AppendLine($"Active injuries ({state.ActiveDebuffs.Count}):");
            if (state.ActiveDebuffs.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (buffId, d) in state.ActiveDebuffs)
            {
                bool hasBaseBuff = _buffManager.HasBuff(buffId);
                string phaseBuffExpected = GetExpectedPhaseBuff(buffId, d.CurrentPhase);
                bool hasPhaseBuff = !string.IsNullOrEmpty(phaseBuffExpected) && _buffManager.HasBuff(phaseBuffExpected);
                var issues = CollectDebuffIssues(d, hasBaseBuff, phaseBuffExpected, hasPhaseBuff);
                string status = issues.Count == 0 ? "OK" : "WARN";
                sb.AppendLine($"  [{status}] {buffId} | {GetInjuryStateLabel(d)} | {InjuryVisibilityHelper.FormatVisibilityDebugLine(d)} | {GetClickExpectationForDebuff(d)}");
            }
        }

        private void AppendCompactIssuesSummary(StringBuilder sb, InjuryState state)
        {
            var lines = new List<string>();
            foreach (var (buffId, d) in state.ActiveDebuffs)
            {
                bool hasBaseBuff = _buffManager.HasBuff(buffId);
                string phaseBuffExpected = GetExpectedPhaseBuff(buffId, d.CurrentPhase);
                bool hasPhaseBuff = !string.IsNullOrEmpty(phaseBuffExpected) && _buffManager.HasBuff(phaseBuffExpected);
                var issues = CollectDebuffIssues(d, hasBaseBuff, phaseBuffExpected, hasPhaseBuff);
                if (issues.Count > 0)
                    lines.Add($"{buffId}: {string.Join(", ", issues)}");
            }

            sb.AppendLine(lines.Count == 0 ? "Issues: none" : "Issues:");
            foreach (var line in lines)
                sb.AppendLine($"  {line}");
        }

        private void AppendProximityReactionDebugLines(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== PROXIMITY ===");
            sb.AppendLine($"LastReactionReason: {(string.IsNullOrEmpty(state.LastProximityReactionReason) ? "-" : state.LastProximityReactionReason)}");
            sb.AppendLine($"LastProximityReactionMinute: {state.LastProximityReactionMinute}");
            sb.AppendLine($"LastStrictReactionDay: {state.LastStrictReactionDay}");
            sb.AppendLine($"Cooldown elapsed: {_harveyReactionManager.GetProximityCooldownElapsedMinutes()} min");
            sb.AppendLine($"Relationship tone: {_harveyReactionManager.GetRelationshipTone()}");
        }

        private void AppendHarveyClickLines(StringBuilder sb)
        {
            sb.AppendLine($"Pending: {_interactionHandler.GetPendingMedicalActionSummary() ?? "none"}");
            sb.AppendLine($"Last click: {_interactionHandler.LastClickDebug ?? "-"}");
            sb.AppendLine("(Decision/gate: use SMAPI `debug medical` — HUD is read-only)");
        }

        private void AppendComplicationsBlock(StringBuilder sb, InjuryState state, int today)
        {
            sb.AppendLine("=== COMPLICATIONS ===");
            if (state.ActiveComplications.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (compId, startDay) in state.ActiveComplications)
                sb.AppendLine($"  {compId}  day {startDay} (+{today - startDay}d)");
        }

        private void AppendRehabBlock(StringBuilder sb, InjuryState state, int today)
        {
            sb.AppendLine("=== REHAB ===");
            if (string.IsNullOrEmpty(state.ActiveRehabInjuryId))
            {
                sb.AppendLine("  (none)");
                return;
            }

            int left = _rehabManager.GetRehabDaysLeft();
            sb.AppendLine(
                $"  injury={state.ActiveRehabInjuryId}  start={state.RehabStartDay}  duration={state.RehabDurationDays}d  left={left}d");
            sb.AppendLine(
                $"  violated={YesNo(state.RehabViolated)}  violCount={state.RehabViolationCount}  lastViolDay={state.LastRehabViolationDay}");
            sb.AppendLine(
                $"  buff={YesNo(_buffManager.HasBuff(CureBuffs.Rehab))}  topicRehab={YesNo(_dialogueManager.HasTopic(ConversationTopics.Rehab))}");
        }

        private void AppendPrescriptionsBlock(StringBuilder sb, InjuryState state, int today)
        {
            sb.AppendLine("=== PRESCRIPTIONS ===");
            sb.AppendLine(
                $"TreatmentComplianceScore: {state.TreatmentComplianceScore} ({ComplianceManager.GetLevelDisplayName(_complianceManager.GetComplianceLevel())})");

            var prescriptions = state.ActivePrescriptions ?? new Dictionary<string, PrescriptionState>();
            if (prescriptions.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (id, prescription) in prescriptions.OrderBy(kvp => kvp.Key))
            {
                if (prescription.IsExpired(today))
                    continue;

                sb.AppendLine(
                    $"  {id}  src={prescription.SourceInjuryId}  left={prescription.GetDaysRemaining(today)}d  viol={prescription.ViolationCount}");
            }
        }

        private void AppendSystemFlagsBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== SYSTEM FLAGS ===");
            sb.AppendLine($"DaysWithSevere: {state.DaysWithSevere}  LastNightRoundDay: {state.LastNightRoundDay}");
            sb.AppendLine(
                $"SevereNightRoundEventShownDay: {state.SevereNightRoundEventShownDay}  " +
                $"SevereNightRoundInjuryId: {(string.IsNullOrEmpty(state.SevereNightRoundInjuryId) ? "-" : state.SevereNightRoundInjuryId)}  " +
                $"NeedsSevereNightRoundEvent: {YesNo(state.NeedsSevereNightRoundEvent)}");
            sb.AppendLine($"NeglectStrikesByInjury: {FormatNeglectStrikes(state)}");
            sb.AppendLine($"PassedOutInTownYesterday: {YesNo(state.PassedOutInTownYesterday)}  PassedOutInMineYesterday: {YesNo(state.PassedOutInMineYesterday)}");
            sb.AppendLine($"NeedsMineRescueEvent: {YesNo(state.NeedsMineRescueEvent)}  WasPassedOut: {YesNo(state.WasPassedOut)}");
            sb.AppendLine($"WasExhausted: {YesNo(state.WasExhausted)}  WasUpTooLate: {YesNo(state.WasUpTooLate)}");
            sb.AppendLine($"LastPassedOutHealth: {state.LastPassedOutHealth}  LastPassedOutLocation: {(string.IsNullOrEmpty(state.LastPassedOutLocation) ? "-" : state.LastPassedOutLocation)}");
            sb.AppendLine($"NeedsExternalRescueHomeEvent: {YesNo(state.NeedsHarveyAfterExternalRescueHomeEvent)}  LastExternalRescueLocation: {(string.IsNullOrEmpty(state.LastExternalRescueLocation) ? "-" : state.LastExternalRescueLocation)}");
            sb.AppendLine($"LastExternalRescueDay: {state.LastExternalRescueDay}  HarveyAfterExternalRescueShownDay: {state.HarveyAfterExternalRescueShownDay}");
            sb.AppendLine($"NeedsHarveyAfterHospitalDischargeHomeEvent: {YesNo(state.NeedsHarveyAfterHospitalDischargeHomeEvent)}  LastHospitalDischargeDay: {state.LastHospitalDischargeDay}  LastHospitalDischargeInjuryId: {(string.IsNullOrEmpty(state.LastHospitalDischargeInjuryId) ? "-" : state.LastHospitalDischargeInjuryId)}  HarveyAfterHospitalDischargeShownDay: {state.HarveyAfterHospitalDischargeShownDay}");
            sb.AppendLine($"NeedsMorningAfterExhaustion: {YesNo(state.NeedsHarveyMorningAfterExhaustionEvent)}  LastExhaustionCollapseDay: {state.LastExhaustionCollapseDay}  MorningAfterExhaustionShownDay: {state.HarveyMorningAfterExhaustionShownDay}");
            int today = (int)Game1.stats.DaysPlayed;
            int mineForbiddenLeft = MineForbiddenHelper.GetMineForbiddenDaysLeft(state, _config, today);
            bool mineForbiddenActive = _buffManager.HasBuff(InjuryBuffs.MineForbidden);
            sb.AppendLine($"MineWarningDay: {state.MineWarningDay}  LastMineSevereWarningDay: {state.LastMineSevereWarningDay}  LastMineSevereForcedExitDay: {state.LastMineSevereForcedExitDay}");
            sb.AppendLine($"MineForbiddenAppliedDay: {state.MineForbiddenAppliedDay}  MineForbiddenDaysLeft: {mineForbiddenLeft}  HarveyMod_MineForbidden: {YesNo(mineForbiddenActive)}  LastMineForbiddenInterceptionDay: {state.LastMineForbiddenInterceptionDay}");
            sb.AppendLine($"LastHealth: {state.LastHealth}  Rain: {state.TimeUnderRainTicks}t/{state.TotalTimeUnderRainToday}t");

            var saved = state.SavedActiveBuffs ?? new List<string>();
            sb.AppendLine($"SavedActiveBuffs: {saved.Count}");
            if (saved.Count > 0)
            {
                sb.AppendLine($"  {string.Join(", ", saved.Take(15))}");
                int remaining = saved.Count - 15;
                if (remaining > 0)
                    sb.AppendLine($"  ... +{remaining} more");
            }

            sb.AppendLine($"location: {Game1.currentLocation?.NameOrUniqueName ?? "-"}");
            sb.AppendLine($"time: {Game1.timeOfDay}  health: {Game1.player.health}/{Game1.player.maxHealth}  stamina: {(int)Game1.player.Stamina}/{(int)Game1.player.MaxStamina}");
            sb.AppendLine($"MainInjury serious: {YesNo(_injuryManager.IsMainInjurySerious())}  dirty+serious: {YesNo(_injuryManager.HasSeriousMainInjuryWithDirtyWound())}");
            sb.AppendLine(
                $"Hospital: IsHospitalized={YesNo(_hospitalizationManager.IsHospitalized)}  " +
                $"injury={(_hospitalizationManager.CurrentInjury ?? "-")}  " +
                $"progress={_hospitalizationManager.HospitalStayProgressMinutes}/{_hospitalizationManager.MinHospitalStayMinutes}m  " +
                $"lastTime={state.LastHospitalTimeOfDay}  " +
                $"admissionDay={_hospitalizationManager.AdmissionDay}  " +
                $"dischargeDay={state.LastHospitalDischargeDay}  " +
                $"CanDischarge={YesNo(_hospitalizationManager.CanDischarge())}");
        }

        private void BuildCompactDebugHud(StringBuilder sb, InjuryState state)
        {
            AppendCompactInjuriesSummary(sb, state);
            AppendCompactIssuesSummary(sb, state);
            AppendHarveyClickLines(sb);
            AppendProximityReactionDebugLines(sb, state);

            int activeTopics = CountActiveWatchedTopics(state);
            sb.AppendLine($"Active mod topics: {activeTopics}");

            var modBuffs = CollectActiveModWatchedBuffs();
            sb.AppendLine(modBuffs.Count == 0
                ? "Active mod buffs: (none)"
                : $"Active mod buffs: {string.Join(", ", modBuffs)}");

            int prescriptionCount = (state.ActivePrescriptions ?? new Dictionary<string, PrescriptionState>())
                .Count(kvp => !kvp.Value.IsExpired((int)Game1.stats.DaysPlayed));
            sb.AppendLine($"Prescriptions: {prescriptionCount}  соблюдение: {state.TreatmentComplianceScore} ({ComplianceManager.GetLevelDisplayName(_complianceManager.GetComplianceLevel())})");
            sb.AppendLine($"CareTrust: {_careTrustManager.GetTrust()} / {_careTrustManager.GetLevel()}");

            if (!string.IsNullOrEmpty(state.ActiveRehabInjuryId))
            {
                sb.AppendLine(
                    $"Rehab: {state.ActiveRehabInjuryId}  left={_rehabManager.GetRehabDaysLeft()}d  viol={state.RehabViolationCount}");
            }
            else
            {
                sb.AppendLine("Rehab: (none)");
            }

            sb.AppendLine(_recoveryPlanManager.GetDebugHudBlock());

            int today = (int)Game1.stats.DaysPlayed;
            sb.AppendLine(
                $"Mine: warningDay={state.MineWarningDay}  forbiddenApplied={state.MineForbiddenAppliedDay}  daysLeft={MineForbiddenHelper.GetMineForbiddenDaysLeft(state, _config, today)}  buff={YesNo(_buffManager.HasBuff(InjuryBuffs.MineForbidden))}");
            sb.AppendLine(
                $"NightRound: LastDay={state.LastNightRoundDay}  severeShownDay={state.SevereNightRoundEventShownDay}  severeInjury={(string.IsNullOrEmpty(state.SevereNightRoundInjuryId) ? "-" : state.SevereNightRoundInjuryId)}  needsFirst={YesNo(state.NeedsSevereNightRoundEvent)}");
            sb.AppendLine(
                $"MorningAfterExhaustion: needs={YesNo(state.NeedsHarveyMorningAfterExhaustionEvent)}  collapseDay={state.LastExhaustionCollapseDay}  shownDay={state.HarveyMorningAfterExhaustionShownDay}");
        }

        private void BuildFullDebugHud(StringBuilder sb, InjuryState state, int today)
        {
            AppendInjuriesDiagnosticBlock(sb, state, today);
            AppendComplicationsBlock(sb, state, today);
            AppendPrescriptionsBlock(sb, state, today);
            AppendRehabBlock(sb, state, today);
            AppendTopicsWatchedBlock(sb, state);
            AppendBuffsWatchedBlock(sb, state);
            AppendAppliedTriggersBlock(sb, state);
            AppendKnownTraumasMatrixBlock(sb, state);
            AppendSystemFlagsBlock(sb, state);
        }

        private string BuildDebugReport(bool full)
        {
            var state = _stateManager.State;
            int today = (int)Game1.stats.DaysPlayed;
            var sb = new StringBuilder();

            sb.AppendLine($"InjuryState  [day {today}]  [{(full ? "full" : "compact")}]");

            if (full)
                BuildFullDebugHud(sb, state, today);
            else
                BuildCompactDebugHud(sb, state);

            return sb.ToString().TrimEnd();
        }

        private void RenderDebugHud(string report, RenderedHudEventArgs e)
        {
            var lines = report.Split('\n');
            if (lines.Length > MaxDebugHudLines)
            {
                lines = lines.Take(MaxDebugHudLines).Concat(new[] { "... truncated, use SMAPI console" }).ToArray();
            }

            var font = Game1.smallFont;
            float maxW = 0, totalH = 0;
            foreach (var line in lines)
            {
                float w = font.MeasureString(line).X;
                if (w > maxW) maxW = w;
                totalH += font.LineSpacing;
            }

            const int pad = 8, x = 10, y = 10;
            e.SpriteBatch.Draw(Game1.staminaRect,
                new Rectangle(x - pad, y - pad, (int)maxW + pad * 2, (int)totalH + pad * 2),
                new Color(0, 0, 0, 170));

            float drawY = y;
            foreach (var line in lines)
            {
                e.SpriteBatch.DrawString(font, line, new Vector2(x, drawY), Color.White);
                drawY += font.LineSpacing;
            }
        }

        private const int MaxDebugHudLines = 45;
    }
}
