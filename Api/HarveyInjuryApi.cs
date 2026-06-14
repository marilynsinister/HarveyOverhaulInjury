using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;

namespace HarveyOverhaul.InjuryCare.Api
{
    /// <summary>Read-only снимок InjuryState для внешнего UI.</summary>
    public sealed class HarveyInjuryApi : IHarveyInjuryApi
    {
        private readonly StateManager _stateManager;
        private readonly InjuryManager _injuryManager;
        private readonly RecoveryPlanManager _recoveryPlanManager;

        public HarveyInjuryApi(
            StateManager stateManager,
            InjuryManager injuryManager,
            RecoveryPlanManager recoveryPlanManager)
        {
            _stateManager = stateManager;
            _injuryManager = injuryManager;
            _recoveryPlanManager = recoveryPlanManager;
        }

        public bool IsAvailable => true;

        public RecoveryPlanPanelDto GetRecoveryPlanState()
        {
            RecoveryPlanViewModel vm = _recoveryPlanManager.BuildViewModel();
            return RecoveryPlanPanelFormatter.Format(vm);
        }

        public InjuryPanelStateDto GetPanelState()
        {
            var state = _stateManager.State;
            var dto = new InjuryPanelStateDto();

            foreach (string injuryId in CollectMainInjuryIds(state))
            {
                if (!state.ActiveDebuffs.TryGetValue(injuryId, out DebuffState? debuff) || debuff == null)
                    continue;

                dto.Injuries.Add(MapMainInjury(injuryId, debuff));
            }

            foreach (string complicationId in CollectComplicationIds(state))
            {
                state.ActiveDebuffs.TryGetValue(complicationId, out DebuffState? debuff);
                dto.Complications.Add(MapComplication(complicationId, debuff));
            }

            dto.HasAnyInjury = dto.Injuries.Count > 0 || dto.Complications.Count > 0;
            dto.SummaryText = BuildSummaryText(dto);
            return dto;
        }

        private IEnumerable<string> CollectMainInjuryIds(InjuryState state)
        {
            var ordered = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string? mainId = _injuryManager.GetCurrentMainInjuryId() ?? state.MainInjuryId;
            if (!string.IsNullOrEmpty(mainId)
                && !InjurySets.KnownComplicationBuffIds.Contains(mainId)
                && seen.Add(mainId))
            {
                ordered.Add(mainId);
            }

            foreach (string buffId in state.ActiveDebuffs.Keys.OrderBy(id => id, StringComparer.Ordinal))
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                    continue;

                if (!InjurySets.HarveyTreatable.Contains(buffId) && !IsDisplayableInjuryBuff(buffId))
                    continue;

                if (seen.Add(buffId))
                    ordered.Add(buffId);
            }

            return ordered;
        }

        private static IEnumerable<string> CollectComplicationIds(InjuryState state)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in InjurySets.ComplicationPriorityOrder)
            {
                if (state.ActiveComplications.ContainsKey(id))
                    ids.Add(id);
            }

            foreach (string id in state.ActiveComplications.Keys)
                ids.Add(id);

            return InjurySets.ComplicationPriorityOrder
                .Where(ids.Contains)
                .Concat(ids.Where(id => !InjurySets.ComplicationPriorityOrder.Contains(id))
                    .OrderBy(id => id, StringComparer.Ordinal));
        }

        private static bool IsDisplayableInjuryBuff(string buffId) =>
            buffId.StartsWith("buff", StringComparison.OrdinalIgnoreCase)
            || buffId.Equals(InjuryBuffs.Cold, StringComparison.OrdinalIgnoreCase);

        private InjuryPanelItemDto MapMainInjury(string buffId, DebuffState debuff)
        {
            var (statusText, adviceText) = BuildMainInjuryTexts(buffId, debuff);
            return new InjuryPanelItemDto
            {
                BuffId = buffId,
                Title = _injuryManager.GetInjuryName(buffId),
                StatusText = statusText,
                AdviceText = adviceText,
                CurrentPhase = debuff.CurrentPhase,
                TotalPhases = debuff.TotalPhases,
                TreatmentStarted = debuff.TreatmentStarted,
                ReadyForNextPhase = debuff.ReadyForNextPhase,
                ReadyForRecovery = debuff.ReadyForRecovery,
                IsComplication = false,
            };
        }

        private InjuryPanelItemDto MapComplication(string buffId, DebuffState? debuff)
        {
            var (title, status, advice) = GetComplicationCopy(buffId);
            return new InjuryPanelItemDto
            {
                BuffId = buffId,
                Title = title,
                StatusText = status,
                AdviceText = advice,
                CurrentPhase = debuff?.CurrentPhase ?? 0,
                TotalPhases = debuff?.TotalPhases ?? 0,
                TreatmentStarted = debuff?.TreatmentStarted ?? false,
                ReadyForNextPhase = debuff?.ReadyForNextPhase ?? false,
                ReadyForRecovery = debuff?.ReadyForRecovery ?? false,
                IsComplication = true,
            };
        }

        private static (string StatusText, string AdviceText) BuildMainInjuryTexts(string buffId, DebuffState debuff)
        {
            if (debuff.ReadyForRecovery)
            {
                return (
                    InjuryPanelTexts.Status.ReadyForRecovery,
                    InjuryPanelTexts.Status.ReadyForRecoveryAdvice);
            }

            if (debuff.ReadyForNextPhase)
            {
                return (
                    InjuryPanelTexts.Status.ReadyForNextPhase,
                    InjuryPanelTexts.Status.ReadyForNextPhaseAdvice);
            }

            if (debuff.IsInTreatment && debuff.TotalPhases > 0)
            {
                string phaseName = TreatmentManager.GetPhaseDisplayName(
                    buffId,
                    debuff.CurrentPhase,
                    debuff.TotalPhases);
                return (
                    InjuryPanelTexts.PhaseLine(NormalizePhaseName(phaseName)),
                    BuildOngoingTreatmentAdvice(buffId));
            }

            if (!debuff.TreatmentStarted)
            {
                return (
                    InjuryPanelTexts.Status.AwaitingTreatment,
                    InjuryPanelTexts.Status.AwaitingTreatmentAdvice);
            }

            return (InjuryPanelTexts.Status.Ongoing, BuildOngoingTreatmentAdvice(buffId));
        }

        private static string NormalizePhaseName(string phaseName) => phaseName switch
        {
            "Острая фаза" => "острая",
            "Заживление" => "заживление",
            "Восстановление" => "восстановление",
            _ when phaseName.Contains("гипс", StringComparison.OrdinalIgnoreCase) => "гипс",
            _ when phaseName.Contains("инфек", StringComparison.OrdinalIgnoreCase) => "лечение инфекции",
            _ => phaseName.ToLowerInvariant(),
        };

        private static string BuildOngoingTreatmentAdvice(string buffId)
        {
            if (InjurySets.WetBandageSensitive.Contains(buffId)
                || InjurySets.DirtyInMines.Contains(buffId)
                || InjurySets.InfectionSensitive.Contains(buffId))
            {
                return InjuryPanelTexts.Advice.BandageAndMines;
            }

            if (string.Equals(buffId, InjuryBuffs.Cold, StringComparison.OrdinalIgnoreCase))
                return InjuryPanelTexts.Advice.WarmAndRest;

            return InjuryPanelTexts.Advice.GeneralCare;
        }

        private static (string Title, string Status, string Advice) GetComplicationCopy(string buffId) => buffId switch
        {
            var id when id.Equals(InjuryBuffs.WetBandage, StringComparison.OrdinalIgnoreCase) =>
                (InjuryPanelTexts.Complications.WetBandageTitle,
                    InjuryPanelTexts.Complications.WetBandageTitle,
                    InjuryPanelTexts.Complications.WetBandageAdvice),

            var id when id.Equals(InjuryBuffs.DirtyWound, StringComparison.OrdinalIgnoreCase) =>
                (InjuryPanelTexts.Complications.DirtyWoundTitle,
                    InjuryPanelTexts.Complications.DirtyWoundTitle,
                    InjuryPanelTexts.Complications.DirtyWoundAdvice),

            var id when id.Equals(InjuryBuffs.WetStitches, StringComparison.OrdinalIgnoreCase) =>
                (InjuryPanelTexts.Complications.WetStitchesTitle,
                    InjuryPanelTexts.Complications.WetStitchesTitle,
                    InjuryPanelTexts.Complications.WetStitchesAdvice),

            var id when id.Equals(InjuryBuffs.AllergicRash, StringComparison.OrdinalIgnoreCase) =>
                (InjuryPanelTexts.Complications.AllergicRashTitle,
                    InjuryPanelTexts.Complications.AllergicRashTitle,
                    InjuryPanelTexts.Complications.AllergicRashAdvice),

            var id when id.Equals(InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase) =>
                (InjuryPanelTexts.Complications.PainFlareTitle,
                    InjuryPanelTexts.Complications.PainFlareTitle,
                    InjuryPanelTexts.Complications.PainFlareAdvice),

            var id when id.Equals(InjuryBuffs.Neglect, StringComparison.OrdinalIgnoreCase) =>
                (InjuryPanelTexts.Complications.NeglectTitle,
                    InjuryPanelTexts.Complications.NeglectTitle,
                    InjuryPanelTexts.Complications.NeglectAdvice),

            _ => (InjuryPanelTexts.Complications.GenericTitle,
                InjuryPanelTexts.Complications.GenericTitle,
                InjuryPanelTexts.Complications.GenericAdvice),
        };

        private static string BuildSummaryText(InjuryPanelStateDto dto)
        {
            if (!dto.HasAnyInjury)
                return "";

            if (dto.Injuries.Count > 0)
                return dto.Injuries[0].Title;

            return dto.Complications[0].Title;
        }
    }
}
