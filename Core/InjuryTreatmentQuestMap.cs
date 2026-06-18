using System;
using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core;

/// <summary>Квесты лечения травм (CP questsCure) ↔ buffId для вкладки «План».</summary>
public static class InjuryTreatmentQuestMap
{
    private static readonly Dictionary<string, string> QuestToInjury =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["HarveyMod_DeepCutsTreatment"] = "buffDeepCuts",
            ["HarveyMod_BurnTreatment"] = "buffBurnWounds",
            ["HarveyMod_MuscleTreatment"] = "buffTornMuscles",
            ["HarveyMod_ConcussionTreatment"] = "buffConcussion",
            ["HarveyMod_FractureTreatment"] = "buffFracturedBone",
            ["HarveyMod_InfectionTreatment"] = "buffInfectedWound",
            ["HarveyMod_RibsTreatment"] = "buffBruisedRibs",
            ["HarveyMod_AnkleTreatment"] = "buffSprainedAnkle",
            ["HarveyMod_BackTreatment"] = "buffBackStrain",
            ["HarveyMod_ShrapnelTreatment"] = "buffShrapnelWounds",
            ["HarveyMod_SurgicalTreatment"] = "buffSurgicalWound",
        };

    public static bool IsInjuryTreatmentQuest(string questId)
        => QuestToInjury.ContainsKey(questId);

    public static string? TryGetInjuryId(string questId)
        => QuestToInjury.GetValueOrDefault(questId);
}
