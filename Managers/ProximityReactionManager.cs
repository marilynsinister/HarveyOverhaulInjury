using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Выбор эмоции и CP-префикса для proximity-облачка (без лечения и без изменения state).
    /// </summary>
    public class ProximityReactionManager
    {
        private readonly BuffManager _buffManager;
        private readonly StateManager _stateManager;
        private readonly InjuryManager _injuryManager;

        private static readonly string[] ComplicationPriority =
        {
            InjuryBuffs.DirtyWound,
            InjuryBuffs.WetStitches,
            InjuryBuffs.WetBandage,
            InjuryBuffs.AllergicRash,
            InjuryBuffs.PainFlare,
            InjuryBuffs.Neglect
        };

        public ProximityReactionManager(
            BuffManager buffManager,
            StateManager stateManager,
            InjuryManager injuryManager)
        {
            _buffManager = buffManager;
            _stateManager = stateManager;
            _injuryManager = injuryManager;
        }

        /// <summary>
        /// Эмоция для proximity-облачка (не «начало лечения»).
        /// </summary>
        public int DetermineEmoteForProximity(InjuryCollection injuries)
        {
            var context = ResolveProximityContext(injuries);

            return context.Kind switch
            {
                ProximityKind.Complication => context.ComplicationId switch
                {
                    InjuryBuffs.DirtyWound => HarveyEmotes.DirtyWound,
                    InjuryBuffs.WetBandage or InjuryBuffs.WetStitches => HarveyEmotes.WorriedAboutPatient,
                    _ when GetActiveComplicationIds(injuries).Count >= 2 => HarveyEmotes.FoundComplication,
                    _ => HarveyEmotes.DirtyWound
                },
                ProximityKind.ReadyForRecovery => HarveyEmotes.Thinking,
                ProximityKind.ReadyForNextPhase => HarveyEmotes.Thinking,
                ProximityKind.InTreatment => HarveyHelper.GetCaringEmote(),
                ProximityKind.Untreated when context.InjuryId != null && IsCriticalInjury(context.InjuryId)
                    => HarveyEmotes.CriticalInjury,
                ProximityKind.Untreated when context.InjuryId != null && IsSeriousInjury(context.InjuryId)
                    => HarveyEmotes.FindInjury,
                ProximityKind.Untreated => HarveyEmotes.FindInjury,
                _ => HarveyEmotes.FindInjury
            };
        }

        /// <summary>
        /// CP-префикс proximity-реплики (без индекса варианта).
        /// </summary>
        public string DetermineProximityPrefix(InjuryCollection injuries) =>
            DetermineProximityPrefixCandidates(injuries)[0];

        /// <summary>
        /// Префиксы proximity-реплик от точного к запасным (для CP fallback).
        /// </summary>
        public IReadOnlyList<string> DetermineProximityPrefixCandidates(InjuryCollection injuries)
        {
            var context = ResolveProximityContext(injuries);
            string tone = context.Tone;

            string primary = context.Kind switch
            {
                ProximityKind.Complication => $"Proximity_Complication_{MapComplicationKey(context.ComplicationId)}_{tone}",
                ProximityKind.ReadyForRecovery => $"Proximity_Recovery_ReadyRecovery_{tone}",
                ProximityKind.ReadyForNextPhase => $"Proximity_Phase_ReadyNextPhase_{tone}",
                ProximityKind.InTreatment => $"Proximity_Injury_InTreatment_{tone}",
                ProximityKind.Untreated => $"Proximity_Injury_Untreated_{tone}",
                _ => $"Proximity_Injury_Untreated_{tone}"
            };

            return BuildPrefixCandidates(primary);
        }

        /// <summary>
        /// Запасные префиксы, если точный ключ отсутствует в CP.
        /// </summary>
        internal static IReadOnlyList<string> BuildPrefixCandidates(string primaryPrefix)
        {
            var candidates = new List<string>();
            AddUnique(candidates, primaryPrefix);

            if (!TryParseProximityPrefix(primaryPrefix, out string category, out string situation, out string tone))
                return candidates;

            if (!string.Equals(tone, "High", StringComparison.OrdinalIgnoreCase))
                AddUnique(candidates, $"Proximity_{category}_{situation}_High");

            if (string.Equals(category, "Complication", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(situation, "Generic", StringComparison.OrdinalIgnoreCase))
                    AddUnique(candidates, $"Proximity_Complication_Generic_{tone}");

                if (!string.Equals(tone, "High", StringComparison.OrdinalIgnoreCase))
                    AddUnique(candidates, "Proximity_Complication_Generic_High");

                // CP-ассет использует Multiple вместо Generic
                if (!string.Equals(situation, "Multiple", StringComparison.OrdinalIgnoreCase))
                    AddUnique(candidates, $"Proximity_Complication_Multiple_{tone}");

                if (!string.Equals(tone, "High", StringComparison.OrdinalIgnoreCase))
                    AddUnique(candidates, "Proximity_Complication_Multiple_High");
            }

            AddUnique(candidates, $"Proximity_Injury_Untreated_{tone}");
            return candidates;
        }

        private static bool TryParseProximityPrefix(
            string prefix,
            out string category,
            out string situation,
            out string tone)
        {
            category = situation = tone = string.Empty;

            if (string.IsNullOrEmpty(prefix))
                return false;

            string[] parts = prefix.Split('_');
            if (parts.Length < 4 || !string.Equals(parts[0], "Proximity", StringComparison.OrdinalIgnoreCase))
                return false;

            category = parts[1];
            tone = parts[^1];
            situation = string.Join('_', parts, 2, parts.Length - 3);
            return !string.IsNullOrEmpty(situation) && !string.IsNullOrEmpty(tone);
        }

        private static void AddUnique(List<string> list, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            foreach (var existing in list)
            {
                if (string.Equals(existing, value, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            list.Add(value);
        }

        private ProximityContext ResolveProximityContext(InjuryCollection injuries)
        {
            string tone = HarveyHelper.GetRelationshipToneWithHarvey();

            if (TryGetPriorityComplication(injuries, out string? complicationId))
            {
                return new ProximityContext
                {
                    Kind = ProximityKind.Complication,
                    ComplicationId = complicationId,
                    Tone = tone
                };
            }

            string? injuryId = ResolvePrimaryInjury(injuries);
            if (injuryId != null)
            {
                var debuffState = _stateManager.GetDebuffState(injuryId);

                if (debuffState?.ReadyForRecovery == true)
                {
                    return new ProximityContext
                    {
                        Kind = ProximityKind.ReadyForRecovery,
                        InjuryId = injuryId,
                        Tone = tone
                    };
                }

                if (debuffState?.ReadyForNextPhase == true)
                {
                    return new ProximityContext
                    {
                        Kind = ProximityKind.ReadyForNextPhase,
                        InjuryId = injuryId,
                        Tone = tone
                    };
                }

                if (debuffState?.TreatmentStarted == true)
                {
                    return new ProximityContext
                    {
                        Kind = ProximityKind.InTreatment,
                        InjuryId = injuryId,
                        Tone = tone
                    };
                }

                return new ProximityContext
                {
                    Kind = ProximityKind.Untreated,
                    InjuryId = injuryId,
                    Tone = tone
                };
            }

            return new ProximityContext
            {
                Kind = ProximityKind.Fallback,
                Tone = tone
            };
        }

        private string? ResolvePrimaryInjury(InjuryCollection injuries)
        {
            if (!string.IsNullOrEmpty(injuries.MainInjury))
                return injuries.MainInjury;

            string? active = _injuryManager.GetActiveInjury();
            if (active != null)
                return active;

            if (_buffManager.HasBuff(InjuryBuffs.Cold))
                return InjuryBuffs.Cold;

            return null;
        }

        private static string MapComplicationKey(string? complicationId) =>
            complicationId switch
            {
                InjuryBuffs.WetBandage => "WetBandage",
                InjuryBuffs.DirtyWound => "DirtyWound",
                InjuryBuffs.WetStitches => "WetStitches",
                InjuryBuffs.Neglect => "Neglect",
                InjuryBuffs.AllergicRash => "AllergicRash",
                InjuryBuffs.PainFlare => "PainFlare",
                _ => "Generic"
            };

        private List<string> GetActiveComplicationIds(InjuryCollection injuries)
        {
            var result = new List<string>();

            foreach (var compId in injuries.Complications)
            {
                if (_buffManager.HasBuff(compId) && !result.Contains(compId))
                    result.Add(compId);
            }

            foreach (var compId in ComplicationPriority)
            {
                if (_buffManager.HasBuff(compId) && !result.Contains(compId))
                    result.Add(compId);
            }

            foreach (var compId in _stateManager.State.ActiveComplications.Keys)
            {
                if (_buffManager.HasBuff(compId) && !result.Contains(compId))
                    result.Add(compId);
            }

            return result;
        }

        private bool TryGetPriorityComplication(InjuryCollection injuries, out string? complicationId)
        {
            var active = GetActiveComplicationIds(injuries);
            if (active.Count == 0)
            {
                complicationId = null;
                return false;
            }

            foreach (var compId in ComplicationPriority)
            {
                if (active.Contains(compId))
                {
                    complicationId = compId;
                    return true;
                }
            }

            complicationId = active[0];
            return true;
        }

        private static bool IsCriticalInjury(string injuryId) =>
            injuryId switch
            {
                "buffConcussion" => true,
                "buffFracturedBone" => true,
                "buffInfectedWound" => true,
                "buffBadlyHurt" => true,
                _ => false
            };

        private static bool IsSeriousInjury(string injuryId) =>
            injuryId switch
            {
                "buffShrapnelWounds" => true,
                "buffBurnWounds" => true,
                "buffSurgicalWound" => true,
                "buffDeepCuts" => true,
                "buffTornMuscles" => true,
                _ => IsCriticalInjury(injuryId)
            };

        private enum ProximityKind
        {
            Complication,
            ReadyForRecovery,
            ReadyForNextPhase,
            InTreatment,
            Untreated,
            Fallback
        }

        private readonly struct ProximityContext
        {
            public ProximityKind Kind { get; init; }
            public string? InjuryId { get; init; }
            public string? ComplicationId { get; init; }
            public string Tone { get; init; }
        }
    }
}
