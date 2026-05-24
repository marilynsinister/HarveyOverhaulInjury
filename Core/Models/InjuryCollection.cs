using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Коллекция травм и осложнений игрока
    /// </summary>
    public class InjuryCollection
    {
        public string? MainInjury { get; set; }
        public List<string> Complications { get; set; } = new();

        public int Count => (MainInjury != null ? 1 : 0) + Complications.Count;
        public bool HasAny => MainInjury != null || Complications.Count > 0;
    }
}

