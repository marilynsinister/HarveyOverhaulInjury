using System;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    public sealed class InjuryVisibilityProfile
    {
        public string BuffId { get; set; } = "";
        public InjuryVisibilityLevel BaseVisibility { get; set; } = InjuryVisibilityLevel.Hidden;
        public bool CanBeHiddenFromHarvey { get; set; } = true;
        public bool AutoRevealOnTalkToHarvey { get; set; } = false;
        public bool AutoRevealOnProximityToHarvey { get; set; } = false;
        public bool AutoRevealWhenComplicated { get; set; } = true;
        public int HarveyDetectionBonus { get; set; } = 0;
        public string[] VisibleSigns { get; set; } = Array.Empty<string>();
    }
}
