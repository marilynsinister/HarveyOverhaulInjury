using System;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Медицинское предписание Харви — временное правило лечения, привязанное к травме.
    /// </summary>
    public class PrescriptionState
    {
        public string Id { get; set; } = "";

        /// <summary>BuffId травмы, из-за которой назначено предписание.</summary>
        public string SourceInjuryId { get; set; } = "";

        public int StartDay { get; set; } = 0;

        public int DurationDays { get; set; } = 0;

        public bool IsViolated { get; set; } = false;

        public int ViolationCount { get; set; } = 0;

        public int LastViolationDay { get; set; } = -1;

        public string? LastViolationReason { get; set; }

        /// <summary>Оставшиеся дни предписания (0 = истекло сегодня или раньше).</summary>
        public int GetDaysRemaining(int currentDay) =>
            Math.Max(0, DurationDays - (currentDay - StartDay));

        public bool IsExpired(int currentDay) => GetDaysRemaining(currentDay) <= 0;
    }
}
