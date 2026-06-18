using System.Text;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using CoreTask = HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanTask;

namespace HarveyOverhaul.InjuryCare.Services;

/// <summary>Устарело: план рендерится в HarveyOverhaulCore через InjuryCareDirectiveProvider.</summary>
[Obsolete("Plan UI is built in HarveyOverhaulCore from IHarveyCareDirectiveProvider.")]
internal static class InjuryRecoveryPlanSections
{
    public static (bool HasPlan, HarveyPanelPlanFields? Fields, List<HarveyPanelSectionDto> Sections) Build(
        RecoveryPlanViewModel dto)
    {
        bool hasAssignments = dto.Assignments.Count > 0;
        bool hasInjuryContext = !string.IsNullOrWhiteSpace(dto.InjuryDisplayName);
        bool hasTasks = dto.Tasks.Count > 0;
        if (!dto.HasPlan && !hasAssignments && !hasInjuryContext && !hasTasks && dto.TodayWarnings.Count == 0)
        {
            return (false, null, []);
        }

        var sections = new List<HarveyPanelSectionDto>();
        var sb = new StringBuilder();

        sections.Add(Section(
            "План Харви",
            BuildToneBlock(dto),
            5,
            dto.HarveyToneKind == RecoveryPlanToneKind.Strict
                ? HarveyPanelSeverity.Urgent
                : dto.HarveyToneKind == RecoveryPlanToneKind.Worried
                    ? HarveyPanelSeverity.Warning
                    : HarveyPanelSeverity.Info));
        sb.AppendLine(BuildToneBlock(dto));

        if (!string.IsNullOrWhiteSpace(dto.InjuryDisplayName))
        {
            string injuryLine = dto.InjuryDisplayName;
            if (!string.IsNullOrWhiteSpace(dto.PhaseLabel))
                injuryLine += $" — {dto.PhaseLabel}";

            sections.Add(Section("Травма", injuryLine, 8, HarveyPanelSeverity.Warning));
            sb.AppendLine(injuryLine);
        }

        if (!string.IsNullOrWhiteSpace(dto.DayProgressText))
        {
            sections.Add(Section("Прогресс", dto.DayProgressText, 40, HarveyPanelSeverity.Info));
            sb.AppendLine(dto.DayProgressText);
        }

        foreach (var assignment in dto.Assignments)
        {
            string body = assignment.Description;
            if (!string.IsNullOrWhiteSpace(assignment.ProgressText))
                body = string.IsNullOrWhiteSpace(body)
                    ? assignment.ProgressText
                    : $"{body}\n{assignment.ProgressText}";

            sections.Add(new HarveyPanelSectionDto
            {
                Title = $"□ {assignment.Title}",
                Body = body,
                Status = assignment.ProgressText,
                Priority = 20,
                Severity = HarveyPanelSeverity.Normal,
            });

            sb.AppendLine($"□ {assignment.Title}");
            if (!string.IsNullOrWhiteSpace(body))
                sb.AppendLine(body);
        }

        string tasksText = BuildTasksText(dto.Tasks);
        if (!string.IsNullOrWhiteSpace(tasksText))
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Режим восстановления",
                Body = tasksText,
                Priority = 30,
                Severity = dto.TodayFailed ? HarveyPanelSeverity.Urgent : HarveyPanelSeverity.Normal,
                Status = dto.TodayFailed
                    ? "День не засчитан"
                    : dto.TodayCompleted
                        ? "День засчитан"
                        : dto.DayProgressText,
            });
            sb.AppendLine();
            sb.AppendLine("Режим на сегодня:");
            sb.AppendLine(tasksText);
        }

        if (dto.TodayWarnings.Count > 0)
        {
            string warnings = string.Join("\n", dto.TodayWarnings.Select(w => $"• {w}"));
            sections.Add(Section("Предупреждения", warnings, 18, HarveyPanelSeverity.Warning));
            sb.AppendLine();
            sb.AppendLine("Предупреждения:");
            sb.AppendLine(warnings);
        }

        string todayFailed = RecoveryPlanViolationReasonTexts.BuildTodayFailedSection(
            dto.IsActive,
            dto.TodayFailed,
            dto.TodayViolationReasons);
        if (!string.IsNullOrWhiteSpace(todayFailed))
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "День не засчитан",
                Body = todayFailed,
                Status = "Харви волнуется",
                Priority = 15,
                Severity = HarveyPanelSeverity.Urgent,
            });
            sb.AppendLine();
            sb.AppendLine(todayFailed);
        }

        if (!string.IsNullOrWhiteSpace(dto.ComplicationSummary))
        {
            sections.Add(Section("Осложнения плана", dto.ComplicationSummary, 25, HarveyPanelSeverity.Warning));
            sb.AppendLine();
            sb.AppendLine(dto.ComplicationSummary);
        }

        if (dto.RequiresHarveyTalk || dto.ReadyForNextPhase || dto.ReadyForRecovery)
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Нужен разговор с Харви",
                Body = "Харви ждёт контрольный осмотр.",
                Priority = 1,
                Severity = HarveyPanelSeverity.Urgent,
            });
        }

        return (true, new HarveyPanelPlanFields
        {
            Title = "План Харви",
            Body = sb.ToString().TrimEnd(),
        }, sections);
    }

    private static string BuildToneBlock(RecoveryPlanViewModel dto)
    {
        if (dto.HarveyTone.HasTone)
            return $"{dto.HarveyTone.Title}. {dto.HarveyTone.Description}";

        return dto.HarveyToneKind switch
        {
            RecoveryPlanToneKind.Strict =>
                $"{RecoveryPlanTexts.HarveyTone.StrictTitle}. {RecoveryPlanTexts.HarveyTone.StrictDescription}",
            RecoveryPlanToneKind.Worried =>
                $"{RecoveryPlanTexts.HarveyTone.WorriedTitle}. {RecoveryPlanTexts.HarveyTone.WorriedDescription}",
            _ =>
                $"{RecoveryPlanTexts.HarveyTone.CalmTitle}. {RecoveryPlanTexts.HarveyTone.CalmDescription}",
        };
    }

    private static string BuildTasksText(IReadOnlyList<RecoveryPlanTask> tasks)
    {
        if (tasks.Count == 0)
            return "";

        var sb = new StringBuilder();
        foreach (RecoveryPlanTask task in tasks)
        {
            string mark = task.IsFailed ? "✗" : task.IsCompleted ? "✓" : "•";
            sb.Append(mark);
            sb.Append(' ');
            sb.Append(task.Title);
            if (task.IsFailed)
                sb.Append(" — сегодня не засчитано");
            else if (task.Id == RecoveryPlanTaskIds.VisitHarveyIfReady)
                sb.Append(" — нужно поговорить с Харви");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static HarveyPanelSectionDto Section(
        string title,
        string body,
        int priority,
        HarveyPanelSeverity severity)
        => new()
        {
            Title = title,
            Body = body,
            Priority = priority,
            Severity = severity,
        };
}
