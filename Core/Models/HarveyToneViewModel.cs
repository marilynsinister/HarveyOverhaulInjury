namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>Готовая модель блока «Тон Харви» для StardewUI.</summary>
    public sealed class HarveyToneViewModel
    {
        public static HarveyToneViewModel Empty { get; } = new();

        public HarveyCareTone Tone { get; init; } = HarveyCareTone.Calm;

        public bool HasTone { get; init; }

        public string Title { get; init; } = "";

        public string Description { get; init; } = "";

        public string IconKey { get; init; } = "";

        /// <summary>Цвет акцента блока (#RRGGBB), без агрессивной красной на лёгких нарушениях.</summary>
        public string AccentColor { get; init; } = "#7f6139";
    }
}
