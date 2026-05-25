using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Testing
{
    /// <summary>
    /// Проверка: нарушение предписания не снижает Friendship с Harvey.
    /// TreatmentComplianceScore is a medical adherence metric. It must never reduce Harvey friendship.
    /// </summary>
    public static class PrescriptionViolationTest
    {
        private const int HighFriendshipPoints = 2000; // 8 hearts

        public static bool Run(
            IMonitor monitor,
            PrescriptionManager prescriptionManager,
            ComplianceManager complianceManager,
            DialogueManager dialogueManager,
            StateManager stateManager,
            string prescriptionKind)
        {
            if (!Context.IsWorldReady)
            {
                monitor.Log("Тест: сначала загрузите сохранение.", LogLevel.Warn);
                return false;
            }

            var (prescriptionId, reason) = ResolvePrescription(prescriptionKind);
            if (prescriptionId == null)
            {
                monitor.Log(
                    "Использование: injury_test_prescription_violation [NoMine|KeepDry]",
                    LogLevel.Info);
                return false;
            }

            EnsureHighFriendshipScenario(monitor);

            int friendshipBefore = GetHarveyFriendshipPoints();
            int complianceBefore = stateManager.State.TreatmentComplianceScore;

            prescriptionManager.AddPrescription(prescriptionId, "buffDeepCuts", 7);

            if (!prescriptionManager.TryMarkViolation(prescriptionId, reason, out int violationCount))
            {
                monitor.Log("Тест: TryMarkViolation не сработал (предписание или день).", LogLevel.Error);
                return false;
            }

            int friendshipAfter = GetHarveyFriendshipPoints();
            int complianceAfter = stateManager.State.TreatmentComplianceScore;
            bool hasViolationTopic = dialogueManager.HasTopic(PrescriptionTopics.Violation);

            bool friendshipOk = friendshipAfter == friendshipBefore;
            bool topicOk = hasViolationTopic;
            bool complianceOk = complianceAfter == complianceBefore - 1;
            bool passed = friendshipOk && topicOk && complianceOk;

            monitor.Log("=== injury_test_prescription_violation ===", LogLevel.Info);
            monitor.Log(
                $"Сценарий: {prescriptionKind} ({prescriptionId}), tier={HarveyHelper.GetHarveyRelationshipTier()}",
                LogLevel.Info);
            monitor.Log(
                $"Friendship: {friendshipBefore} → {friendshipAfter} ({(friendshipOk ? "OK" : "FAIL")})",
                friendshipOk ? LogLevel.Info : LogLevel.Error);
            monitor.Log(
                $"topicHarvey_PrescriptionViolation: {(topicOk ? "есть" : "нет")} ({(topicOk ? "OK" : "FAIL")})",
                topicOk ? LogLevel.Info : LogLevel.Error);
            monitor.Log(
                $"TreatmentComplianceScore: {complianceBefore} → {complianceAfter}, violations={violationCount} ({(complianceOk ? "OK" : "FAIL")})",
                complianceOk ? LogLevel.Info : LogLevel.Error);
            monitor.Log(
                passed ? "Результат: PASS" : "Результат: FAIL",
                passed ? LogLevel.Info : LogLevel.Error);

            return passed;
        }

        private static (string? Id, string Reason) ResolvePrescription(string kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
                kind = "NoMine";

            return kind.Trim().ToLowerInvariant() switch
            {
                "nomine" or "mine" => (PrescriptionIds.NoMine, "mine"),
                "keepdry" or "rain" => (PrescriptionIds.KeepDry, "rain"),
                _ => (null, ""),
            };
        }

        private static void EnsureHighFriendshipScenario(IMonitor monitor)
        {
            int points = GetHarveyFriendshipPoints();
            var tier = HarveyHelper.GetHarveyRelationshipTier();

            if (tier is HarveyRelationshipTier.Dating or HarveyRelationshipTier.Married)
            {
                monitor.Log($"Тест: отношения с Harvey — {tier}.", LogLevel.Debug);
                return;
            }

            if (points >= HighFriendshipPoints)
            {
                monitor.Log($"Тест: Friendship с Harvey = {points} (≥8♥).", LogLevel.Debug);
                return;
            }

            if (!Game1.player.friendshipData.TryGetValue("Harvey", out var friendship))
            {
                monitor.Log("Тест: нет данных дружбы с Harvey.", LogLevel.Warn);
                return;
            }

            friendship.Points = HighFriendshipPoints;
            monitor.Log(
                $"Тест: Friendship поднят до {HighFriendshipPoints} (8♥) для сценария высоких отношений.",
                LogLevel.Debug);
        }

        private static int GetHarveyFriendshipPoints()
        {
            return Game1.player.friendshipData.TryGetValue("Harvey", out var friendship)
                ? friendship.Points
                : 0;
        }
    }
}
