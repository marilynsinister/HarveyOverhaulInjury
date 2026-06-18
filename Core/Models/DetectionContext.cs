namespace HarveyOverhaul.InjuryCare.Core.Models
{
    public sealed class DetectionContext
    {
        public bool HarveyIsPresent { get; set; }
        public bool IsDirectTalk { get; set; }
        public bool IsProximityCheck { get; set; }
        public bool IsMarriedToHarvey { get; set; }
        public bool IsDatingOrEngagedToHarvey { get; set; }
        public bool IsFarmHouseMorningOrEvening { get; set; }
        public bool PlayerHealthLow { get; set; }
        public bool PlayerStaminaLow { get; set; }
        public bool HasComplication { get; set; }
        public bool IsFestivalContext { get; set; }
    }
}
