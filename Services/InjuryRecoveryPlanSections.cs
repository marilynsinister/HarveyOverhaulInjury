using System.Text;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using CoreTask = HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanTask;

namespace HarveyOverhaul.InjuryCare.Services;

/// <summary>Форматирование RecoveryPlan для вкладки «План» через Core sections.</summary>
internal static class InjuryRecoveryPlanSections
{
    public static (bool HasPlan, HarveyPanelPlanFields? Fields, List<HarveyPanelSectionDto> Sections) Build(
        RecoveryPlanViewModel dto)
    {
        if (!dto.HasPlan)
        {
            return (false, null, []);
        }

        var sections = new List<HarveyPanelSectionDto>();
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(dto.InjuryDisplayName))
        {
            sections.Add(Section(dto.InjuryDisplayName, dto.PhaseLabel, 0, HarveyPanelSeverity.Normal));
            sb.AppendLine(dto.InjuryDisplayName);
        }

        if (!string.IsNullOrWhiteSpace(dto.DayProgressText))
        {
            sections.Add(Section("Прогресс", dto.DayProgressText, 10, HarveyPanelSeverity.Info));
            sb.AppendLine(dto.DayProgressText);
        }

        if (dto.HarveyTone.HasTone)
        {
            sections.Add(Section(dto.HarveyTone.Title, dto.HarveyTone.Description, 20, HarveyPanelSeverity.Warning));
            sb.AppendLine();
            sb.AppendLine(dto.HarveyTone.Title);
            sb.AppendLine(dto.HarveyTone.Description);
        }

        string tasksText = BuildTasksText(dto.Tasks);
        sections.Add(new HarveyPanelSectionDto
        {
            Title = "План на сегодня",
            Body = tasksText,
            Priority = 30,
            Severity = dto.TodayFailed ? HarveyPanelSeverity.Urgent : HarveyPanelSeverity.Normal,
            Status = dto.TodayFailed ? "День не засчитан" : dto.TodayCompleted ? "День засчитан" : "",
        });
        sb.AppendLine();
        sb.AppendLine("План на сегодня:");
        sb.AppendLine(tasksText);

        string todayFailed = RecoveryPlanViolationReasonTexts.BuildTodayFailedSection(
            dto.IsActive,
            dto.TodayFailed,
            dto.TodayViolationReasons);
        if (!string.IsNullOrWhiteSpace(todayFailed))
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Нарушения",
                Body = todayFailed,
                Priority = 40,
                Severity = HarveyPanelSeverity.Urgent,
            });
            sb.AppendLine();
            sb.AppendLine(todayFailed);
        }

        if (!string.IsNullOrWhiteSpace(dto.WhyImportant))
        {
            sections.Add(Section("Почему это важно", dto.WhyImportant, 50, HarveyPanelSeverity.Info));
            sb.AppendLine();
            sb.AppendLine("Почему это важно:");
            sb.AppendLine(dto.WhyImportant);
        }

        if (!string.IsNullOrWhiteSpace(dto.ComplicationSummary))
        {
            sections.Add(Section("Осложнения", dto.ComplicationSummary, 60, HarveyPanelSeverity.Warning));
            sb.AppendLine();
            sb.AppendLine(dto.ComplicationSummary);
        }

        if (dto.RequiresHarveyTalk)
        {
            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Нужен разговор с Харви",
                Body = "Харви ждёт контрольный осмотр, прежде чем продолжить план.",
                Priority = 5,
                Severity = HarveyPanelSeverity.Urgent,
            });
        }

        return (true, new HarveyPanelPlanFields
        {
            Title = "План восстановления",
            Body = sb.ToString().TrimEnd(),
        }, sections);
    }

    private static string BuildTasksText(IReadOnlyList<RecoveryPlanTask> tasks)
    {
        if (tasks.Count == 0)
            return "• Харви пока не добавил пунктов на сегодня";

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
