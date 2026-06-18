namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>Идентификаторы назначений единого «Плана Харви» (save-state Injury).</summary>
    public static class RecoveryPlanAssignmentIds
    {
        public const string FindSafePlace = HarveyOverhaul.Core.Models.HarveyRecoveryPlanAssignmentIds.FindSafePlace;
        public const string DontStayAlone = HarveyOverhaul.Core.Models.HarveyRecoveryPlanAssignmentIds.DontStayAlone;

        public static string TalkHarveyNextPhase(string injuryId)
            => HarveyOverhaul.Core.Models.HarveyRecoveryPlanAssignmentIds.TalkHarveyNextPhase(injuryId);

        public static string TalkHarveyRecovery(string injuryId)
            => HarveyOverhaul.Core.Models.HarveyRecoveryPlanAssignmentIds.TalkHarveyRecovery(injuryId);

        public static bool IsHarveyTalkAssignment(string assignmentId)
            => HarveyOverhaul.Core.Models.HarveyRecoveryPlanAssignmentIds.IsHarveyTalkAssignment(assignmentId);
    }
}
