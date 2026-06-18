using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    public sealed partial class RecoveryPlanManager
    {
        public bool IsUnifiedPlanActive()
        {
            var plan = _stateManager.GetRecoveryPlan();
            return plan.IsActive || plan.ActiveAssignments.Count > 0;
        }

        /// <summary>Перед открытием панели — пересобрать save-state план по текущим травмам.</summary>
        public void EnsurePlanFreshForDisplay()
        {
            if (!Context.IsWorldReady)
                return;

            _injuryManager.EnsureActiveTreatmentBuffs();

            var plan = _stateManager.GetRecoveryPlan();
            if (HasActiveInjuryTreatmentContext() || plan.ActiveAssignments.Count > 0 || plan.CompletionTalkPending)
                RefreshPlanForToday();
        }

        public bool HasActiveInjuryTreatmentContext() => TryGetActiveTreatmentDebuff(out _, out _);

        private bool TryGetActiveTreatmentDebuff(out string injuryId, out DebuffState debuffState)
        {
            injuryId = _injuryManager.GetActiveInTreatmentInjuryId() ?? "";
            if (string.IsNullOrEmpty(injuryId))
            {
                debuffState = null!;
                return false;
            }

            debuffState = _stateManager.GetDebuffState(injuryId)
                ?? _injuryManager.BuildPanelDebuffFallback(injuryId)
                ?? null!;
            if (debuffState == null)
            {
                injuryId = "";
                return false;
            }

            debuffState = _injuryManager.EnrichDebuffForPanel(injuryId, debuffState);

            if (!HarveyInjuryAwarenessHelper.IsInjuryHarveyAware(debuffState)
                && !debuffState.IsInTreatment
                && !debuffState.TreatmentStarted
                && debuffState.CurrentPhase <= 0)
            {
                injuryId = "";
                debuffState = null!;
                return false;
            }

            return true;
        }

        public void StartPlan(
            RecoveryPlanSource source,
            IReadOnlyList<string> assignmentIds,
            string? planId = null)
        {
            var plan = _stateManager.GetRecoveryPlan();
            int today = Context.IsWorldReady ? GameUtils.Today() : plan.PlanStartDay;

            if (plan.PlanStartDay < 0 && today >= 0)
                plan.PlanStartDay = today;

            plan.IsActive = true;
            plan.Source = source == RecoveryPlanSource.None ? RecoveryPlanSource.Mixed : source;

            if (!string.IsNullOrWhiteSpace(planId))
                plan.PlanId = planId.Trim();

            foreach (string id in assignmentIds)
                AddAssignmentInternal(plan, id, goal: 0, replaceGoal: false);

            SyncHarveyTone(plan);
            _stateManager.Save();

            _monitor.Log(
                $"[RecoveryPlan] StartPlan source={plan.Source}, assignments=[{string.Join(", ", plan.ActiveAssignments)}]",
                LogLevel.Info);
        }

        public void AddAssignment(string assignmentId, int goal = 0)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return;

            var plan = _stateManager.GetRecoveryPlan();
            AddAssignmentInternal(plan, assignmentId.Trim(), goal, replaceGoal: goal > 0);
            plan.IsActive = true;
            SyncHarveyTone(plan);
            _stateManager.Save();
        }

        public void AddProgress(string assignmentId, int amount)
        {
            if (string.IsNullOrWhiteSpace(assignmentId) || amount == 0)
                return;

            var plan = _stateManager.GetRecoveryPlan();
            string id = assignmentId.Trim();

            if (!plan.ActiveAssignments.Contains(id, StringComparer.OrdinalIgnoreCase))
                return;

            plan.Progress.TryGetValue(id, out int current);
            int next = Math.Max(0, current + amount);
            plan.Progress[id] = next;

            if (plan.Goals.TryGetValue(id, out int goal) && goal > 0 && next >= goal && !IsStressOwnedAssignment(id))
                CompleteAssignment(id);
            else
                _stateManager.Save();
        }

        public void SetAssignmentProgress(string assignmentId, int current, int goal)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return;

            var plan = _stateManager.GetRecoveryPlan();
            string id = assignmentId.Trim();

            if (!plan.ActiveAssignments.Contains(id, StringComparer.OrdinalIgnoreCase))
                AddAssignmentInternal(plan, id, goal, replaceGoal: true);

            plan.Progress[id] = Math.Max(0, current);
            if (goal > 0)
                plan.Goals[id] = goal;

            if (goal > 0 && current >= goal && !IsStressOwnedAssignment(id))
                CompleteAssignment(id);
            else
                _stateManager.Save();
        }

        private static bool IsStressOwnedAssignment(string assignmentId) =>
            string.Equals(assignmentId, RecoveryPlanAssignmentIds.FindSafePlace, StringComparison.OrdinalIgnoreCase)
            || string.Equals(assignmentId, RecoveryPlanAssignmentIds.DontStayAlone, StringComparison.OrdinalIgnoreCase);

        public bool CompleteAssignment(string assignmentId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return false;

            var plan = _stateManager.GetRecoveryPlan();
            string id = assignmentId.Trim();

            int removed = plan.ActiveAssignments.RemoveAll(a => string.Equals(a, id, StringComparison.OrdinalIgnoreCase));
            if (removed == 0
                && !plan.CompletedAssignmentsToday.Contains(id, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!plan.CompletedAssignmentsToday.Contains(id, StringComparer.OrdinalIgnoreCase))
                plan.CompletedAssignmentsToday.Add(id);

            plan.Progress.Remove(id);
            plan.Goals.Remove(id);

            SyncHarveyTone(plan);
            _stateManager.Save();

            _monitor.Log($"[RecoveryPlan] CompleteAssignment: {id}", LogLevel.Info);
            return true;
        }

        public void FailAssignment(string assignmentId, string reason)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return;

            RegisterViolation(
                RecoveryPlanViolationType.MissedHarveyTalk,
                string.IsNullOrWhiteSpace(reason)
                    ? $"Назначение не выполнено: {assignmentId}"
                    : reason);
        }

        public void RemoveAssignment(string assignmentId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return;

            var plan = _stateManager.GetRecoveryPlan();
            string id = assignmentId.Trim();
            plan.ActiveAssignments.RemoveAll(a => string.Equals(a, id, StringComparison.OrdinalIgnoreCase));
            plan.Progress.Remove(id);
            plan.Goals.Remove(id);
            _stateManager.Save();
        }

        public void RegisterWarning(string type, string text)
        {
            if (!IsUnifiedPlanActive() || string.IsNullOrWhiteSpace(text))
                return;

            var plan = _stateManager.GetRecoveryPlan();
            plan.HadWarningsToday = true;
            plan.HadPlanWarnings = true;

            if (!plan.TodayWarnings.Contains(text, StringComparer.Ordinal))
                plan.TodayWarnings.Add(text.Trim());

            plan.HarveyTone = RecoveryPlanToneKind.Worried;
            _stateManager.Save();

            _monitor.Log($"[RecoveryPlan] Warning: type={type}, text={text}", LogLevel.Debug);
        }

        public void RegisterViolation(string type, string text)
        {
            int severity = ResolveWarningOrViolationSeverity(type);
            if (severity == RecoveryViolationSeverity.Mild)
            {
                RegisterWarning(type, text);
                return;
            }

            HandleRecoveryPlanViolation(type, severity, text);
            var plan = _stateManager.GetRecoveryPlan();
            plan.HarveyTone = RecoveryPlanToneKind.Strict;
            _stateManager.Save();
        }

        public bool CheckDayResult()
        {
            var plan = _stateManager.GetRecoveryPlan();
            if (!plan.IsActive)
                return false;

            bool failed = plan.TodayFailed
                || _stateManager.State.RecoveryPlanDayFailed
                || plan.TodayViolationReasons.Count > 0;

            if (failed)
            {
                plan.FailedDays++;
                return false;
            }

            if (plan.TodayCompleted)
                return true;

            return !plan.HadWarningsToday && plan.TodayViolations.Count == 0;
        }

        public IReadOnlyList<RecoveryPlanAssignmentViewModel> BuildAssignmentViewModels()
        {
            var plan = _stateManager.GetRecoveryPlan();
            var result = new List<RecoveryPlanAssignmentViewModel>();

            foreach (string id in plan.ActiveAssignments.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                plan.Progress.TryGetValue(id, out int progress);
                plan.Goals.TryGetValue(id, out int goal);

                result.Add(new RecoveryPlanAssignmentViewModel
                {
                    Id = id,
                    Title = RecoveryPlanAssignmentTexts.GetTitle(id),
                    Description = RecoveryPlanAssignmentTexts.GetDescription(id),
                    Progress = progress,
                    Goal = goal,
                    ProgressText = goal > 0
                        ? RecoveryPlanAssignmentTexts.FormatProgress(progress, goal)
                        : "",
                    IsCompleted = false,
                    IsFailed = plan.TodayFailed,
                });
            }

            return result;
        }

        public void SyncInjuryTalkAssignments(string injuryId, DebuffState debuffState)
        {
            if (debuffState.ReadyForRecovery)
            {
                string id = RecoveryPlanAssignmentIds.TalkHarveyRecovery(injuryId);
                AddAssignment(id);
            }
            else if (debuffState.ReadyForNextPhase)
            {
                string id = RecoveryPlanAssignmentIds.TalkHarveyNextPhase(injuryId);
                AddAssignment(id);
            }
            else
            {
                RemoveAssignment(RecoveryPlanAssignmentIds.TalkHarveyRecovery(injuryId));
                RemoveAssignment(RecoveryPlanAssignmentIds.TalkHarveyNextPhase(injuryId));
            }
        }

        private static void AddAssignmentInternal(
            RecoveryPlanState plan,
            string assignmentId,
            int goal,
            bool replaceGoal)
        {
            if (!plan.ActiveAssignments.Any(a => string.Equals(a, assignmentId, StringComparison.OrdinalIgnoreCase)))
                plan.ActiveAssignments.Add(assignmentId);

            if (goal > 0 || replaceGoal)
                plan.Goals[assignmentId] = goal;

            plan.Progress.TryAdd(assignmentId, 0);
        }

        private void SyncHarveyTone(RecoveryPlanState plan)
        {
            if (plan.TodayFailed || plan.TodayViolationReasons.Count > 0 || plan.HadPlanViolations)
            {
                plan.HarveyTone = RecoveryPlanToneKind.Strict;
                return;
            }

            if (plan.HadWarningsToday || plan.TodayWarnings.Count > 0)
            {
                plan.HarveyTone = RecoveryPlanToneKind.Worried;
                return;
            }

            plan.HarveyTone = RecoveryPlanToneKind.Calm;
        }

        private int ResolveWarningOrViolationSeverity(string type)
        {
            string normalized = RecoveryPlanViolationTopicMap.ResolveViolationType(type);
            if (string.IsNullOrEmpty(normalized))
                normalized = type?.Trim() ?? "";

            return normalized switch
            {
                RecoveryPlanViolationType.LowStaminaWarning => RecoveryViolationSeverity.Mild,
                RecoveryPlanViolationType.LowHealthWarning => RecoveryViolationSeverity.Mild,
                RecoveryPlanViolationType.LowStaminaFail => RecoveryViolationSeverity.Medium,
                RecoveryPlanViolationType.LowHealthFail => RecoveryViolationSeverity.Severe,
                RecoveryPlanViolationType.LowStamina => ResolveDefaultViolationSeverity(RecoveryPlanViolationType.LowStamina),
                RecoveryPlanViolationType.LowHealth => ResolveDefaultViolationSeverity(RecoveryPlanViolationType.LowHealth),
                _ => RecoveryViolationSeverity.Medium,
            };
        }

        private static string FormatProgressGoals(RecoveryPlanState plan)
        {
            if (plan.Progress.Count == 0 && plan.Goals.Count == 0)
                return "-";

            var parts = plan.Progress.Keys
                .Union(plan.Goals.Keys, StringComparer.OrdinalIgnoreCase)
                .Select(id =>
                {
                    plan.Progress.TryGetValue(id, out int p);
                    plan.Goals.TryGetValue(id, out int g);
                    return g > 0 ? $"{id}:{p}/{g}" : $"{id}:{p}";
                });

            return string.Join(", ", parts);
        }
    }
}
