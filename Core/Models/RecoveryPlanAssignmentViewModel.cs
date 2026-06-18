namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>Строка назначения для UI «Плана Харви».</summary>
    public sealed class RecoveryPlanAssignmentViewModel
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public int Progress { get; init; }
        public int Goal { get; init; }
        public bool IsCompleted { get; init; }
        public bool IsFailed { get; init; }
        public string ProgressText { get; init; } = "";
    }
}
