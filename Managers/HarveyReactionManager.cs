using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    public enum ProximityReactionContext
    {
        PrescriptionViolation,
        LowCompliance,
        HighCompliance,
        SevereInjury,
        LightInjury,
        Complication,
        Untreated,
        InTreatment,
        ReadyForNextPhase,
        ReadyForRecovery,
        RehabViolation,
    }

    /// <summary>
    /// Результат выбора proximity-реакции Харви (без DialogueBox).
    /// </summary>
    public sealed class ProximityReactionPlan
    {
        public int Emote { get; init; }
        public ProximityReactionContext Context { get; init; }
        public string Tone { get; init; } = "Low";
        public string Reason { get; init; } = "";
        public bool IsStrict { get; init; }
        public string? InjuryId { get; init; }
        public string? ComplicationId { get; init; }
        public string? TopicId { get; init; }
        public int TopicDays { get; init; } = 2;
        public IReadOnlyList<string> CpPrefixCandidates { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Контекстные proximity-реакции Харви: эмоция, текст, topic, strict/normal.
    /// </summary>
    public class HarveyReactionManager
    {
        private const int TopicDaysNormal = 1;
        private const int TopicDaysStrict = 2;

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly ComplianceManager _complianceManager;
        private readonly PrescriptionManager _prescriptionManager;
        private readonly DialogueManager _dialogueManager;
        private readonly RehabManager? _rehabManager;

        private static readonly string[] ComplicationPriority =
        {
            InjuryBuffs.DirtyWound,
            InjuryBuffs.WetStitches,
            InjuryBuffs.WetBandage,
            InjuryBuffs.AllergicRash,
            InjuryBuffs.PainFlare,
            InjuryBuffs.Neglect,
        };

        public HarveyReactionManager(
            IMonitor monitor,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            ComplianceManager complianceManager,
            PrescriptionManager prescriptionManager,
            DialogueManager dialogueManager,
            RehabManager? rehabManager = null)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _complianceManager = complianceManager;
            _prescriptionManager = prescriptionManager;
            _dialogueManager = dialogueManager;
            _rehabManager = rehabManager;
        }

        public ProximityReactionPlan? DetermineProximityReaction(NPC harvey, InjuryCollection injuries)
        {
            if (harvey == null || !injuries.HasAny)
                return null;

            string tone = GetRelationshipTone();
            var resolved = ResolveReactionContext(injuries, tone);
            if (resolved == null)
                return null;

            return new ProximityReactionPlan
            {
                Emote = DetermineEmote(resolved),
                Context = resolved.Context,
                Tone = tone,
                Reason = resolved.Reason,
                IsStrict = resolved.IsStrict,
                InjuryId = resolved.InjuryId,
                ComplicationId = resolved.ComplicationId,
                TopicId = resolved.TopicId,
                TopicDays = resolved.TopicDays,
                CpPrefixCandidates = BuildCpPrefixCandidates(resolved),
            };
        }

        public string GetRelationshipTone()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship == null || !friendship.TryGetValue("Harvey", out var data))
                return "Low";

            if (data.IsMarried())
                return "Married";

            if (data.IsDating())
                return "Romantic";

            int hearts = data.Points / 250;
            return hearts >= 4 ? "Mid" : "Low";
        }

        public string PickReactionTextByContext(
            ProximityReactionContext context,
            string tone,
            string? injuryId = null,
            string? complicationId = null)
        {
            bool severe = injuryId != null && IsSevereInjury(injuryId);

            return (context, tone) switch
            {
                (ProximityReactionContext.PrescriptionViolation, "Low") =>
                    "Мы договорились о режиме. Пожалуйста, соблюдайте его.",
                (ProximityReactionContext.PrescriptionViolation, _) =>
                    "Мы ведь договаривались. Без геройства.",

                (ProximityReactionContext.LowCompliance, "Low") =>
                    "Мне придётся быть строже, если вы продолжите игнорировать лечение.",
                (ProximityReactionContext.LowCompliance, "Romantic") =>
                    "Режим срывается. Я не сержусь — но мне придётся быть строже.",
                (ProximityReactionContext.LowCompliance, _) =>
                    "Мне придётся быть строже, если ты продолжаешь игнорировать лечение.",

                (ProximityReactionContext.HighCompliance, "Low") =>
                    "Спасибо, что слушаете рекомендации. Так вы быстрее восстановитесь.",
                (ProximityReactionContext.HighCompliance, _) =>
                    "Спасибо, что слушаешь рекомендации. Так ты быстрее восстановишься.",

                (ProximityReactionContext.SevereInjury, "Low") =>
                    "Стой. В таком состоянии нужен осмотр.",
                (ProximityReactionContext.SevereInjury, "Romantic") =>
                    "Стой. Я не отпущу тебя дальше в таком виде.",
                (ProximityReactionContext.SevereInjury, _) =>
                    "Стой. Ты не должна ходить в таком состоянии.",

                (ProximityReactionContext.LightInjury, "Low") =>
                    "Ты хромаешь. Лучше показаться врачу.",
                (ProximityReactionContext.LightInjury, "Mid") =>
                    "Ты бережёшь ногу. Покажись мне, когда сможешь.",
                (ProximityReactionContext.LightInjury, "Romantic") =>
                    "Эй... я вижу, как ты бережёшь ногу. Ко мне, пожалуйста.",
                (ProximityReactionContext.LightInjury, "Married") =>
                    "Я вижу, что тебе больно. Дай мне посмотреть.",
                (ProximityReactionContext.LightInjury, _) =>
                    "Ты хромаешь. Лучше показаться врачу.",

                (ProximityReactionContext.Complication, _) when complicationId == InjuryBuffs.Neglect =>
                    "Ты слишком долго откладываешь лечение. Сегодня — осмотр.",
                (ProximityReactionContext.Complication, _) when complicationId == InjuryBuffs.DirtyWound =>
                    "Рана загрязнена. Немедленно в клинику.",
                (ProximityReactionContext.Complication, _) =>
                    "Это осложнение нельзя игнорировать.",

                (ProximityReactionContext.Untreated, _) when severe =>
                    "Стой. Ты не должна ходить в таком состоянии.",
                (ProximityReactionContext.Untreated, "Romantic") =>
                    "Пожалуйста, не тяни. Мне нужно тебя осмотреть.",
                (ProximityReactionContext.Untreated, _) =>
                    "Тебе нужен осмотр. Не откладывай.",

                (ProximityReactionContext.ReadyForRecovery, _) =>
                    "Похоже, пора завершить лечение. Зайди на осмотр.",
                (ProximityReactionContext.ReadyForNextPhase, _) =>
                    "Следующий этап лечения готов. Приходи на осмотр.",

                (ProximityReactionContext.RehabViolation, _) =>
                    "Тело ещё не готово к прежней нагрузке.",

                (ProximityReactionContext.InTreatment, "Romantic") =>
                    "Я слежу за твоим восстановлением. Не перегружайся.",
                (ProximityReactionContext.InTreatment, "Married") =>
                    "Береги себя. Я рядом, если станет хуже.",
                (ProximityReactionContext.InTreatment, _) =>
                    "Продолжай соблюдать режим лечения.",

                _ => "Тебе нужен медицинский осмотр.",
            };
        }

        public string ResolveReactionText(ProximityReactionPlan plan)
        {
            string fallback = PickReactionTextByContext(
                plan.Context,
                plan.Tone,
                plan.InjuryId,
                plan.ComplicationId);

            if (plan.CpPrefixCandidates.Count == 0)
                return fallback;

            return _dialogueManager.PickRandomProximityLineByPrefixes(
                plan.CpPrefixCandidates,
                fallback);
        }

        public void RecordReactionShown(ProximityReactionPlan plan)
        {
            int today = GameUtils.Today();
            var state = _stateManager.State;
            state.LastProximityReactionMinute = GameUtils.CurrentTimeInMinutes();
            state.LastProximityReactionReason = plan.Reason;

            if (plan.IsStrict)
                state.LastStrictReactionDay = today;

            _stateManager.Save();
        }

        public bool CanShowStrictReactionToday() =>
            _stateManager.State.LastStrictReactionDay != GameUtils.Today();

        public int GetProximityCooldownElapsedMinutes()
        {
            int last = _stateManager.State.LastProximityReactionMinute;
            if (last < 0)
                return int.MaxValue;

            int now = GameUtils.CurrentTimeInMinutes();
            int elapsed = now - last;
            if (elapsed < 0)
                elapsed += 24 * 60;

            return elapsed;
        }

        public static IReadOnlyList<string> BuildPrefixCandidates(string primaryPrefix)
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

                if (!string.Equals(situation, "Multiple", StringComparison.OrdinalIgnoreCase))
                    AddUnique(candidates, $"Proximity_Complication_Multiple_{tone}");

                if (!string.Equals(tone, "High", StringComparison.OrdinalIgnoreCase))
                    AddUnique(candidates, "Proximity_Complication_Multiple_High");
            }

            AddUnique(candidates, $"Proximity_Injury_Untreated_{tone}");
            return candidates;
        }

        private sealed class ResolvedContext
        {
            public ProximityReactionContext Context { get; init; }
            public string Reason { get; init; } = "";
            public bool IsStrict { get; init; }
            public string? InjuryId { get; init; }
            public string? ComplicationId { get; init; }
            public string? TopicId { get; init; }
            public int TopicDays { get; init; } = TopicDaysNormal;
            public string Tone { get; init; } = "Low";
        }

        private ResolvedContext? ResolveReactionContext(InjuryCollection injuries, string tone)
        {
            if (TryGetViolatedPrescription(out string? violatedPrescription))
            {
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.PrescriptionViolation,
                    Reason = $"prescription_violation:{violatedPrescription}",
                    IsStrict = true,
                    TopicId = ConversationTopics.ProximityStrict,
                    TopicDays = TopicDaysStrict,
                    Tone = tone,
                };
            }

            if (_complianceManager.IsLowCompliance && HasActiveTreatment())
            {
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.LowCompliance,
                    Reason = "low_compliance",
                    IsStrict = true,
                    TopicId = ConversationTopics.ProximityStrict,
                    TopicDays = TopicDaysStrict,
                    Tone = tone,
                };
            }

            if (_rehabManager?.IsRehabActive() == true && _stateManager.State.RehabViolated)
            {
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.RehabViolation,
                    Reason = "rehab_violation",
                    IsStrict = true,
                    TopicId = ConversationTopics.ProximityStrict,
                    TopicDays = TopicDaysStrict,
                    Tone = tone,
                };
            }

            if (TryGetPriorityComplication(injuries, out string? complicationId))
            {
                bool strict = complicationId is InjuryBuffs.Neglect or InjuryBuffs.DirtyWound;
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.Complication,
                    Reason = $"complication:{complicationId}",
                    IsStrict = strict,
                    ComplicationId = complicationId,
                    TopicId = strict ? ConversationTopics.ProximityStrict : ConversationTopics.ProximityReaction,
                    TopicDays = strict ? TopicDaysStrict : TopicDaysNormal,
                    Tone = tone,
                };
            }

            string? injuryId = ResolvePrimaryInjury(injuries);
            var debuffState = injuryId != null ? _stateManager.GetDebuffState(injuryId) : null;

            if (debuffState?.ReadyForRecovery == true)
            {
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.ReadyForRecovery,
                    Reason = $"ready_recovery:{injuryId}",
                    InjuryId = injuryId,
                    TopicId = ConversationTopics.ProximityReaction,
                    TopicDays = TopicDaysNormal,
                    Tone = tone,
                };
            }

            if (debuffState?.ReadyForNextPhase == true)
            {
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.ReadyForNextPhase,
                    Reason = $"ready_phase:{injuryId}",
                    InjuryId = injuryId,
                    TopicId = ConversationTopics.ProximityReaction,
                    TopicDays = TopicDaysNormal,
                    Tone = tone,
                };
            }

            if (_complianceManager.IsHighCompliance && debuffState?.TreatmentStarted == true)
            {
                return new ResolvedContext
                {
                    Context = ProximityReactionContext.HighCompliance,
                    Reason = "high_compliance",
                    InjuryId = injuryId,
                    TopicId = ConversationTopics.ProximityPraise,
                    TopicDays = TopicDaysNormal,
                    Tone = tone,
                };
            }

            if (debuffState?.TreatmentStarted != true && injuryId != null)
            {
                var ctx = IsSevereInjury(injuryId)
                    ? ProximityReactionContext.SevereInjury
                    : ProximityReactionContext.Untreated;

                return new ResolvedContext
                {
                    Context = ctx,
                    Reason = $"untreated:{injuryId}",
                    IsStrict = ctx == ProximityReactionContext.SevereInjury,
                    InjuryId = injuryId,
                    TopicId = ctx == ProximityReactionContext.SevereInjury
                        ? ConversationTopics.ProximityStrict
                        : ConversationTopics.ProximityReaction,
                    TopicDays = ctx == ProximityReactionContext.SevereInjury ? TopicDaysStrict : TopicDaysNormal,
                    Tone = tone,
                };
            }

            if (injuryId != null)
            {
                if (IsSevereInjury(injuryId))
                {
                    return new ResolvedContext
                    {
                        Context = ProximityReactionContext.SevereInjury,
                        Reason = $"severe:{injuryId}",
                        IsStrict = true,
                        InjuryId = injuryId,
                        TopicId = ConversationTopics.ProximityStrict,
                        TopicDays = TopicDaysStrict,
                        Tone = tone,
                    };
                }

                if (!IsCriticalInjury(injuryId))
                {
                    return new ResolvedContext
                    {
                        Context = ProximityReactionContext.LightInjury,
                        Reason = $"light:{injuryId}",
                        InjuryId = injuryId,
                        TopicId = ConversationTopics.ProximityReaction,
                        TopicDays = TopicDaysNormal,
                        Tone = tone,
                    };
                }

                return new ResolvedContext
                {
                    Context = ProximityReactionContext.InTreatment,
                    Reason = $"in_treatment:{injuryId}",
                    InjuryId = injuryId,
                    TopicId = ConversationTopics.ProximityReaction,
                    TopicDays = TopicDaysNormal,
                    Tone = tone,
                };
            }

            return null;
        }

        private IReadOnlyList<string> BuildCpPrefixCandidates(ResolvedContext ctx)
        {
            string cpTone = MapCpTone(ctx.Tone);

            string primary = ctx.Context switch
            {
                ProximityReactionContext.PrescriptionViolation =>
                    $"Proximity_Prescription_Violated_{cpTone}",
                ProximityReactionContext.LowCompliance =>
                    $"Proximity_Compliance_Low_{cpTone}",
                ProximityReactionContext.HighCompliance =>
                    $"Proximity_Compliance_High_{cpTone}",
                ProximityReactionContext.RehabViolation =>
                    $"Proximity_Rehab_Violated_{cpTone}",
                ProximityReactionContext.Complication =>
                    $"Proximity_Complication_{MapComplicationKey(ctx.ComplicationId)}_{cpTone}",
                ProximityReactionContext.ReadyForRecovery =>
                    $"Proximity_Recovery_ReadyRecovery_{cpTone}",
                ProximityReactionContext.ReadyForNextPhase =>
                    $"Proximity_Phase_ReadyNextPhase_{cpTone}",
                ProximityReactionContext.SevereInjury =>
                    $"Proximity_Injury_Severe_{cpTone}",
                ProximityReactionContext.LightInjury =>
                    $"Proximity_Injury_Light_{cpTone}",
                ProximityReactionContext.Untreated =>
                    $"Proximity_Injury_Untreated_{cpTone}",
                ProximityReactionContext.InTreatment =>
                    $"Proximity_Injury_InTreatment_{cpTone}",
                _ => $"Proximity_Injury_Untreated_{cpTone}",
            };

            return BuildPrefixCandidates(primary);
        }

        private static string MapCpTone(string tone) =>
            tone switch
            {
                "Married" or "Romantic" => "High",
                "Mid" => "Mid",
                _ => "Low",
            };

        private int DetermineEmote(ResolvedContext ctx) =>
            ctx.Context switch
            {
                ProximityReactionContext.PrescriptionViolation
                    or ProximityReactionContext.LowCompliance
                    or ProximityReactionContext.RehabViolation => HarveyEmotes.ForcedHospitalization,
                ProximityReactionContext.Complication => ctx.ComplicationId switch
                {
                    InjuryBuffs.DirtyWound => HarveyEmotes.DirtyWound,
                    InjuryBuffs.WetBandage or InjuryBuffs.WetStitches => HarveyEmotes.WorriedAboutPatient,
                    InjuryBuffs.Neglect => HarveyEmotes.CriticalInjury,
                    _ => HarveyEmotes.DirtyWound,
                },
                ProximityReactionContext.HighCompliance => HarveyHelper.GetCaringEmote(),
                ProximityReactionContext.SevereInjury => HarveyEmotes.CriticalInjury,
                ProximityReactionContext.ReadyForRecovery
                    or ProximityReactionContext.ReadyForNextPhase => HarveyEmotes.Thinking,
                ProximityReactionContext.InTreatment => HarveyHelper.GetCaringEmote(),
                ProximityReactionContext.LightInjury => HarveyEmotes.FindInjury,
                ProximityReactionContext.Untreated when ctx.InjuryId != null && IsCriticalInjury(ctx.InjuryId)
                    => HarveyEmotes.CriticalInjury,
                ProximityReactionContext.Untreated => HarveyEmotes.FindInjury,
                _ => HarveyEmotes.FindInjury,
            };

        private bool HasActiveTreatment() =>
            _stateManager.State.ActiveDebuffs.Values.Any(d => d.TreatmentStarted);

        private bool TryGetViolatedPrescription(out string? prescriptionId)
        {
            prescriptionId = null;
            var state = _stateManager.State;
            if (state.ActivePrescriptions == null || state.ActivePrescriptions.Count == 0)
                return false;

            int today = GameUtils.Today();
            foreach (var (id, prescription) in state.ActivePrescriptions)
            {
                if (prescription.IsExpired(today))
                    continue;

                if (prescription.ViolationCount > 0 || prescription.IsViolated)
                {
                    prescriptionId = id;
                    return true;
                }
            }

            return false;
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

        private static string MapComplicationKey(string? complicationId) =>
            complicationId switch
            {
                InjuryBuffs.WetBandage => "WetBandage",
                InjuryBuffs.DirtyWound => "DirtyWound",
                InjuryBuffs.WetStitches => "WetStitches",
                InjuryBuffs.Neglect => "Neglect",
                InjuryBuffs.AllergicRash => "AllergicRash",
                InjuryBuffs.PainFlare => "PainFlare",
                _ => "Generic",
            };

        private static bool IsCriticalInjury(string injuryId) =>
            injuryId switch
            {
                "buffConcussion" => true,
                "buffFracturedBone" => true,
                "buffInfectedWound" => true,
                "buffBadlyHurt" => true,
                _ => false,
            };

        private static bool IsSevereInjury(string injuryId) =>
            InjurySets.Severe.Contains(injuryId)
            || injuryId switch
            {
                "buffShrapnelWounds" => true,
                "buffBurnWounds" => true,
                "buffSurgicalWound" => true,
                "buffDeepCuts" => true,
                "buffTornMuscles" => true,
                _ => IsCriticalInjury(injuryId),
            };

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
    }
}
