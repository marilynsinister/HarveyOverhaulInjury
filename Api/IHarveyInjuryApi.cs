using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Api
{
    /// <summary>Публичный read-only API для UI «План Харви» (HarveyStressMeter).</summary>
    public interface IHarveyInjuryApi
    {
        bool IsAvailable { get; }

        /// <summary>Текущее состояние травм для вкладки «Травмы» и блока обзора.</summary>
        InjuryPanelStateDto GetPanelState();

        /// <summary>Текущий RecoveryPlan для вкладки «План».</summary>
        RecoveryPlanPanelDto GetRecoveryPlanState();
    }

    public sealed class RecoveryPlanPanelDto
    {
        public bool HasPlan { get; set; }

        public string Title { get; set; } = "";

        public string BodyText { get; set; } = "";

        public string SummaryLine { get; set; } = "";
    }

    public sealed class InjuryPanelStateDto
    {
        public bool HasAnyInjury { get; set; }

        public List<InjuryPanelItemDto> Injuries { get; set; } = new();

        public List<InjuryPanelItemDto> Complications { get; set; } = new();

        public string SummaryText { get; set; } = "";
    }

    public sealed class InjuryPanelItemDto
    {
        public string BuffId { get; set; } = "";

        public string Title { get; set; } = "";

        public string StatusText { get; set; } = "";

        public string AdviceText { get; set; } = "";

        public int CurrentPhase { get; set; }

        public int TotalPhases { get; set; }

        public bool TreatmentStarted { get; set; }

        public bool ReadyForNextPhase { get; set; }

        public bool ReadyForRecovery { get; set; }

        public bool IsComplication { get; set; }
    }
}
