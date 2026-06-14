using System.Text;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.InjuryCare.Api;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;

namespace HarveyOverhaul.InjuryCare.Services;

/// <summary>Отдаёт данные травм и recovery plan в общее окно «План Харви» (HarveyOverhaul.Core).</summary>
public sealed class InjuryPanelProvider : IHarveyPanelProvider
{
    public const string ProviderId = HarveyOverhaul.Core.Services.HarveyProviderRegistry.InjuryProviderId;

    private readonly StateManager _stateManager;
    private readonly InjuryManager _injuryManager;
    private readonly RecoveryPlanManager _recoveryPlanManager;
    private readonly CareTrustManager _careTrustManager;

    public InjuryPanelProvider(
        StateManager stateManager,
        InjuryManager injuryManager,
        RecoveryPlanManager recoveryPlanManager,
        CareTrustManager careTrustManager)
    {
        _stateManager = stateManager;
        _injuryManager = injuryManager;
        _recoveryPlanManager = recoveryPlanManager;
        _careTrustManager = careTrustManager;
    }

    public string UniqueId => ProviderId;
    public string DisplayName => "Harvey Injury";
    public int Priority => 200;

    public HarveyPanelContribution GetPanelContribution()
    {
        var state = _stateManager.State;
        var injuries = CollectMainInjuries(state);
        var complications = CollectComplications(state);
        bool hasAnyInjury = injuries.Count > 0 || complications.Count > 0;
        bool pendingReview = injuries.Any(NeedsHarveyTalk) || state.RecoveryPlanNeedsHarveyVisit;

        var recoveryVm = _recoveryPlanManager.BuildViewModel();
        var (hasPlan, planFields, planSections) = InjuryRecoveryPlanSections.Build(recoveryVm);

        return new HarveyPanelContribution
        {
            ProviderId = ProviderId,
            HasActiveRecoveryPlan = hasPlan,
            HasPendingHarveyReview = pendingReview,
            OverviewFields = BuildOverviewFields(injuries, complications, hasAnyInjury, pendingReview),
            OverviewSections = BuildOverviewSections(injuries, complications, hasAnyInjury, pendingReview),
            InjurySections = BuildInjurySections(injuries, complications, hasAnyInjury),
            InjuriesBody = FormatInjuriesBody(injuries, complications, hasAnyInjury),
            PlanFields = planFields,
            PlanSections = planSections,
            TrustSections = BuildTrustSections(),
        };
    }

    private List<InjuryPanelEntry> CollectMainInjuries(InjuryState state)
    {
        var entries = new List<InjuryPanelEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? mainId = _injuryManager.GetCurrentMainInjuryId() ?? state.MainInjuryId;
        if (!string.IsNullOrEmpty(mainId)
            && !InjurySets.KnownComplicationBuffIds.Contains(mainId)
            && state.ActiveDebuffs.TryGetValue(mainId, out var mainDebuff)
            && mainDebuff != null
            && seen.Add(mainId))
        {
            entries.Add(MapMainInjury(mainId, mainDebuff));
        }

        foreach (string buffId in state.ActiveDebuffs.Keys.OrderBy(id => id, StringComparer.Ordinal))
        {
            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                continue;

            if (!InjurySets.HarveyTreatable.Contains(buffId) && !IsDisplayableInjuryBuff(buffId))
                continue;

            if (!seen.Add(buffId))
                continue;

            if (state.ActiveDebuffs.TryGetValue(buffId, out var debuff) && debuff != null)
                entries.Add(MapMainInjury(buffId, debuff));
        }

        return entries;
    }

    private List<InjuryPanelEntry> CollectComplications(InjuryState state)
    {
        var entries = new List<InjuryPanelEntry>();
        foreach (string id in InjurySets.ComplicationPriorityOrder)
        {
            if (!state.ActiveComplications.ContainsKey(id))
                continue;

            state.ActiveDebuffs.TryGetValue(id, out var debuff);
            entries.Add(MapComplication(id, debuff));
        }

        foreach (string id in state.ActiveComplications.Keys)
        {
            if (InjurySets.ComplicationPriorityOrder.Contains(id))
                continue;

            state.ActiveDebuffs.TryGetValue(id, out var debuff);
            entries.Add(MapComplication(id, debuff));
        }

        return entries;
    }

    private static bool IsDisplayableInjuryBuff(string buffId) =>
        buffId.StartsWith("buff", StringComparison.OrdinalIgnoreCase)
        || buffId.Equals(InjuryBuffs.Cold, StringComparison.OrdinalIgnoreCase);

    private InjuryPanelEntry MapMainInjury(string buffId, DebuffState debuff)
    {
        var (statusText, adviceText) = BuildMainInjuryTexts(buffId, debuff);
        return new InjuryPanelEntry
        {
            BuffId = buffId,
            Title = _injuryManager.GetInjuryName(buffId),
            StatusText = statusText,
            AdviceText = adviceText,
            ReadyForNextPhase = debuff.ReadyForNextPhase,
            ReadyForRecovery = debuff.ReadyForRecovery,
            IsComplication = false,
        };
    }

    private static InjuryPanelEntry MapComplication(string buffId, DebuffState? debuff)
    {
        var (title, status, advice) = GetComplicationCopy(buffId);
        return new InjuryPanelEntry
        {
            BuffId = buffId,
            Title = title,
            StatusText = status,
            AdviceText = advice,
            ReadyForNextPhase = debuff?.ReadyForNextPhase ?? false,
            ReadyForRecovery = debuff?.ReadyForRecovery ?? false,
            IsComplication = true,
        };
    }

    private static HarveyPanelOverviewFields? BuildOverviewFields(
        IReadOnlyList<InjuryPanelEntry> injuries,
        IReadOnlyList<InjuryPanelEntry> complications,
        bool hasAnyInjury,
        bool pendingReview)
    {
        if (!hasAnyInjury)
            return null;

        return new HarveyPanelOverviewFields
        {
            StateLine = pendingReview ? "Харви ждёт контрольный разговор" : "Травма требует внимания.",
            AssignmentLine = injuries.FirstOrDefault()?.AdviceText ?? "",
            InjuriesLine = complications.Count > 0 ? $"Осложнения: {complications.Count}" : "",
        };
    }

    private static List<HarveyPanelSectionDto> BuildOverviewSections(
        IReadOnlyList<InjuryPanelEntry> injuries,
        IReadOnlyList<InjuryPanelEntry> complications,
        bool hasAnyInjury,
        bool pendingReview)
    {
        if (!hasAnyInjury)
            return [];

        var sections = new List<HarveyPanelSectionDto>
        {
            new()
            {
                Title = pendingReview ? "Нужен осмотр Харви" : "Активная травма",
                Body = injuries.FirstOrDefault()?.Title ?? "Травма требует внимания",
                Priority = 0,
                Severity = pendingReview ? HarveyPanelSeverity.Urgent : HarveyPanelSeverity.Warning,
            },
        };

        if (complications.Count > 0)
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Осложнения",
                Body = string.Join("\n", complications.Select(c => c.Title)),
                Priority = 10,
                Severity = HarveyPanelSeverity.Urgent,
            });
        }

        if (!string.IsNullOrWhiteSpace(injuries.FirstOrDefault()?.AdviceText))
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Совет Харви",
                Body = injuries[0].AdviceText,
                Priority = 20,
                Severity = HarveyPanelSeverity.Info,
            });
        }

        return sections;
    }

    private static List<HarveyPanelSectionDto> BuildInjurySections(
        IReadOnlyList<InjuryPanelEntry> injuries,
        IReadOnlyList<InjuryPanelEntry> complications,
        bool hasAnyInjury)
    {
        if (!hasAnyInjury)
            return [];

        var sections = new List<HarveyPanelSectionDto>();

        if (injuries.Count > 0)
        {
            sections.Add(ToSection("Основная травма", injuries[0], 0));
            for (int i = 1; i < injuries.Count; i++)
                sections.Add(ToSection("Дополнительно", injuries[i], 10 + i));
        }

        foreach (var complication in complications)
            sections.Add(ToSection("Осложнение", complication, 50, HarveyPanelSeverity.Urgent));

        return sections;
    }

    private static HarveyPanelSectionDto ToSection(
        string label,
        InjuryPanelEntry entry,
        int priority,
        HarveyPanelSeverity severity = HarveyPanelSeverity.Normal)
    {
        var body = new StringBuilder();
        body.AppendLine(entry.Title);
        if (!string.IsNullOrWhiteSpace(entry.StatusText))
            body.AppendLine(entry.StatusText);
        if (!string.IsNullOrWhiteSpace(entry.AdviceText))
            body.AppendLine(entry.AdviceText);

        return new HarveyPanelSectionDto
        {
            Title = label,
            Body = body.ToString().TrimEnd(),
            Status = entry.ReadyForRecovery
                ? "Готово к выздоровлению"
                : entry.ReadyForNextPhase
                    ? "Готово к следующей фазе"
                    : "",
            Priority = priority,
            Severity = entry.ReadyForNextPhase || entry.ReadyForRecovery
                ? HarveyPanelSeverity.Urgent
                : severity,
        };
    }

    private static string FormatInjuriesBody(
        IReadOnlyList<InjuryPanelEntry> injuries,
        IReadOnlyList<InjuryPanelEntry> complications,
        bool hasAnyInjury)
    {
        if (!hasAnyInjury)
            return "";

        var sb = new StringBuilder();
        sb.AppendLine("Травма требует ухода");
        sb.AppendLine("Следуйте назначению Харви и не рискуйте восстановлением.");
        sb.AppendLine();

        if (injuries.Count > 0)
        {
            sb.AppendLine("Сейчас");
            AppendEntry(sb, injuries[0]);
            for (int i = 1; i < injuries.Count; i++)
            {
                sb.AppendLine();
                sb.AppendLine("Ещё");
                AppendEntry(sb, injuries[i]);
            }
        }

        if (complications.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Осложнения");
            foreach (var complication in complications)
            {
                sb.AppendLine();
                AppendEntry(sb, complication);
            }
        }

        return sb.ToString().TrimEnd();
    }

    private List<HarveyPanelSectionDto> BuildTrustSections()
    {
        int trust = _careTrustManager.GetTrust();
        var level = _careTrustManager.GetLevel();
        string levelName = level switch
        {
            CareTrustLevel.Low => "Харви насторожен",
            CareTrustLevel.High => "Харви доверяет режиму",
            _ => "Харви наблюдает",
        };

        return
        [
            new HarveyPanelSectionDto
            {
                Title = levelName,
                Body = $"Медицинское доверие: {trust} очков.",
                Priority = 10,
                Severity = level == CareTrustLevel.Low
                    ? HarveyPanelSeverity.Warning
                    : HarveyPanelSeverity.Info,
            },
        ];
    }

    private static bool NeedsHarveyTalk(InjuryPanelEntry entry) =>
        entry.ReadyForNextPhase || entry.ReadyForRecovery;

    private static void AppendEntry(StringBuilder sb, InjuryPanelEntry entry)
    {
        sb.AppendLine(entry.Title);
        if (!string.IsNullOrWhiteSpace(entry.StatusText))
            sb.AppendLine(entry.StatusText);
        if (!string.IsNullOrWhiteSpace(entry.AdviceText))
            sb.AppendLine(entry.AdviceText);
    }

    private static (string StatusText, string AdviceText) BuildMainInjuryTexts(string buffId, DebuffState debuff)
    {
        if (debuff.ReadyForRecovery)
            return (InjuryPanelTexts.Status.ReadyForRecovery, InjuryPanelTexts.Status.ReadyForRecoveryAdvice);

        if (debuff.ReadyForNextPhase)
            return (InjuryPanelTexts.Status.ReadyForNextPhase, InjuryPanelTexts.Status.ReadyForNextPhaseAdvice);

        if (debuff.IsInTreatment && debuff.TotalPhases > 0)
        {
            string phaseName = TreatmentManager.GetPhaseDisplayName(buffId, debuff.CurrentPhase, debuff.TotalPhases);
            return (InjuryPanelTexts.PhaseLine(NormalizePhaseName(phaseName)), BuildOngoingTreatmentAdvice(buffId));
        }

        if (!debuff.TreatmentStarted)
            return (InjuryPanelTexts.Status.AwaitingTreatment, InjuryPanelTexts.Status.AwaitingTreatmentAdvice);

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

    private sealed class InjuryPanelEntry
    {
        public string BuffId { get; init; } = "";
        public string Title { get; init; } = "";
        public string StatusText { get; init; } = "";
        public string AdviceText { get; init; } = "";
        public bool ReadyForNextPhase { get; init; }
        public bool ReadyForRecovery { get; init; }
        public bool IsComplication { get; init; }
    }
}
