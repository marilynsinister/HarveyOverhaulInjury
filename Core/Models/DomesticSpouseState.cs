namespace HarveyOverhaul.InjuryCare.Core.Models
{
    public sealed class DomesticSpouseState
    {
        public int LastMorningLineDay { get; set; } = -1;
        public int LastEveningLineDay { get; set; } = -1;
        public int LastLateNightLineDay { get; set; } = -1;

        public int LastProximityLineDay { get; set; } = -1;
        public int LastProximityGameMinutes { get; set; } = -9999;

        public int DomesticReactionsShownToday { get; set; } = 0;
        public string LastDomesticPrefix { get; set; } = "";
        public string LastDomesticLine { get; set; } = "";

        public int LastFestivalSupportDay { get; set; } = -1;
        public int LastStormComfortDay { get; set; } = -1;
        public int LastAfterViolationDay { get; set; } = -1;
        public int LastAfterPerfectPlanDay { get; set; } = -1;
    }
}
