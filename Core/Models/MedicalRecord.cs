namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>Запись медицинской карты: травма, от которой игрок полностью выздоровел.</summary>
    public class MedicalRecord
    {
        public string InjuryId { get; set; } = "";

        /// <summary>DaysPlayed, когда получена травма.</summary>
        public int StartDay { get; set; }

        /// <summary>DaysPlayed, когда Харви закрыл лечение.</summary>
        public int RecoveredDay { get; set; }

        /// <summary>Суммарный сдвиг сроков от соблюдения режима (&lt;0 — быстрее плана).</summary>
        public int RegimenAdjustmentDays { get; set; }
    }
}
