using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;

namespace HarveyOverhaul.InjuryCare.Api
{
    public sealed class HarveyRecoveryPlanApi : IHarveyRecoveryPlanApi
    {
        private readonly RecoveryPlanManager _manager;

        public HarveyRecoveryPlanApi(RecoveryPlanManager manager)
        {
            _manager = manager;
        }

        public bool IsPlanActive() => _manager.IsUnifiedPlanActive();

        public void StartPlan(string source, IReadOnlyList<string> assignmentIds, string? planId = null)
            => _manager.StartPlan(ParseSource(source), assignmentIds, planId);

        public void AddAssignment(string assignmentId, int goal = 0)
            => _manager.AddAssignment(assignmentId, goal);

        public void AddProgress(string assignmentId, int amount)
            => _manager.AddProgress(assignmentId, amount);

        public void SetProgress(string assignmentId, int current, int goal)
            => _manager.SetAssignmentProgress(assignmentId, current, goal);

        public bool CompleteAssignment(string assignmentId)
            => _manager.CompleteAssignment(assignmentId);

        public void FailAssignment(string assignmentId, string reason)
            => _manager.FailAssignment(assignmentId, reason);

        public void RegisterWarning(string type, string text)
            => _manager.RegisterWarning(type, text);

        public void RegisterViolation(string type, string text)
            => _manager.RegisterViolation(type, text);

        public void RemoveAssignment(string assignmentId)
            => _manager.RemoveAssignment(assignmentId);

        private static RecoveryPlanSource ParseSource(string source) => source?.Trim().ToLowerInvariant() switch
        {
            "injury" => RecoveryPlanSource.Injury,
            "stress" => RecoveryPlanSource.Stress,
            "mixed" => RecoveryPlanSource.Mixed,
            _ => RecoveryPlanSource.Mixed,
        };
    }
}
