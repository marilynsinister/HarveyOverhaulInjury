using System.Collections.Generic;
using System.Text;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using CoreTask = HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanTask;

namespace HarveyOverhaul.InjuryCare.Api
{
    /// <summary>Форматирование RecoveryPlan DTO для текстовой вкладки «План» (без StardewUI).</summary>
    internal static class RecoveryPlanPanelFormatter
    {
        public static RecoveryPlanPanelDto Format(RecoveryPlanViewModel dto)
        {
            if (!dto.HasPlan)
            {
                return new RecoveryPlanPanelDto
                {
                    HasPlan = false,
                    Title = "Плана восстановления нет",
                    BodyText = "Сейчас нет строгого режима. Просто берегите себя.",
                    SummaryLine = "",
                };
            }

            var tone = dto.HarveyTone;
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(dto.InjuryDisplayName))
                sb.AppendLine(dto.InjuryDisplayName);

            if (!string.IsNullOrWhiteSpace(dto.PhaseLabel))
                sb.AppendLine(dto.PhaseLabel);

            if (!string.IsNullOrWhiteSpace(dto.DayProgressText))
                sb.AppendLine(dto.DayProgressText);

            if (tone.HasTone)
            {
                sb.AppendLine();
                sb.AppendLine(tone.Title);
                sb.AppendLine(tone.Description);
            }

            sb.AppendLine();
            sb.AppendLine("План на сегодня:");
            sb.AppendLine(BuildTasksText(dto.Tasks));

            string todayFailed = RecoveryPlanViolationReasonTexts.BuildTodayFailedSection(
                dto.IsActive,
                dto.TodayFailed,
                dto.TodayViolationReasons);
            if (!string.IsNullOrWhiteSpace(todayFailed))
            {
                sb.AppendLine();
                sb.AppendLine(todayFailed);
            }

            if (!string.IsNullOrWhiteSpace(dto.WhyImportant))
            {
                sb.AppendLine();
                sb.AppendLine("Почему это важно:");
                sb.AppendLine(dto.WhyImportant);
            }

            if (!string.IsNullOrWhiteSpace(dto.ComplicationSummary))
            {
                sb.AppendLine();
                sb.AppendLine(dto.ComplicationSummary);
            }

            return new RecoveryPlanPanelDto
            {
                HasPlan = true,
                Title = "План восстановления",
                BodyText = sb.ToString().TrimEnd(),
                SummaryLine = BuildSummaryLine(dto),
            };
        }

        private static string BuildSummaryLine(RecoveryPlanViewModel dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.DayProgressText))
                return dto.DayProgressText;

            if (!string.IsNullOrWhiteSpace(dto.InjuryDisplayName))
                return dto.InjuryDisplayName;

            return "План восстановления";
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
    }
}
