using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CoreDto = HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanViewModel;
using CoreTask = HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanTask;
using HarveyOverhaul.InjuryCare.Core;

namespace HarveyOverhaul.InjuryCare.UI.RecoveryPlan
{
    /// <summary>
    /// View model для окна StardewUI «План восстановления».
    /// </summary>
    public sealed class RecoveryPlanViewModel : INotifyPropertyChanged
    {
        private string _title = "План восстановления";
        private string _injuryLine = "";
        private string _phaseLine = "";
        private string _progressLine = "";
        private string _regimeStatusLine = "";
        private string _harveyToneSectionLabel = "";
        private string _harveyToneTitle = "";
        private string _harveyToneDescription = "";
        private string _harveyToneAccentColor = "#7f6139";
        private string _tasksText = "";
        private string _whyImportant = "";
        private string _complicationLine = "";
        private string _hintText = "";
        private string _todayFailedSectionText = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title { get => _title; set => SetField(ref _title, value); }
        public string InjuryLine { get => _injuryLine; set => SetField(ref _injuryLine, value); }
        public string PhaseLine { get => _phaseLine; set => SetField(ref _phaseLine, value); }
        public string ProgressLine { get => _progressLine; set => SetField(ref _progressLine, value); }
        public string RegimeStatusLine { get => _regimeStatusLine; set => SetField(ref _regimeStatusLine, value); }
        public string HarveyToneSectionLabel { get => _harveyToneSectionLabel; set => SetField(ref _harveyToneSectionLabel, value); }
        public string HarveyToneTitle { get => _harveyToneTitle; set => SetField(ref _harveyToneTitle, value); }
        public string HarveyToneDescription { get => _harveyToneDescription; set => SetField(ref _harveyToneDescription, value); }
        public string HarveyToneAccentColor { get => _harveyToneAccentColor; set => SetField(ref _harveyToneAccentColor, value); }
        public string TasksText { get => _tasksText; set => SetField(ref _tasksText, value); }
        public string WhyImportant { get => _whyImportant; set => SetField(ref _whyImportant, value); }
        public string ComplicationLine { get => _complicationLine; set => SetField(ref _complicationLine, value); }
        public string HintText { get => _hintText; set => SetField(ref _hintText, value); }
        public string TodayFailedSectionText { get => _todayFailedSectionText; set => SetField(ref _todayFailedSectionText, value); }

        public static RecoveryPlanViewModel FromDto(CoreDto dto)
        {
            if (!dto.HasPlan)
            {
                return new RecoveryPlanViewModel
                {
                    Title = "План восстановления",
                    HarveyToneDescription = RecoveryPlanTexts.HarveyTone.NoActivePlan,
                    HintText = "Закрыть — ESC или клик вне окна.",
                };
            }

            var tone = dto.HarveyTone;

            return new RecoveryPlanViewModel
            {
                Title = "План восстановления",
                InjuryLine = string.IsNullOrWhiteSpace(dto.InjuryDisplayName)
                    ? ""
                    : $"Травма: {dto.InjuryDisplayName}",
                PhaseLine = string.IsNullOrWhiteSpace(dto.PhaseLabel) ? "" : $"Фаза: {dto.PhaseLabel}",
                ProgressLine = dto.DayProgressText,
                RegimeStatusLine = string.IsNullOrWhiteSpace(dto.RegimeStatusText)
                    ? ""
                    : $"Статус: {dto.RegimeStatusText}",
                HarveyToneSectionLabel = tone.HasTone ? RecoveryPlanTexts.HarveyTone.SectionLabel : "",
                HarveyToneTitle = tone.Title,
                HarveyToneDescription = tone.Description,
                HarveyToneAccentColor = tone.AccentColor,
                TasksText = BuildTasksText(dto.Tasks, dto.Assignments),
                WhyImportant = dto.WhyImportant,
                ComplicationLine = dto.ComplicationSummary,
                TodayFailedSectionText = RecoveryPlanViolationReasonTexts.BuildTodayFailedSection(
                    dto.IsActive,
                    dto.TodayFailed,
                    dto.TodayViolationReasons),
                HintText = BuildHint(dto),
            };
        }

        private static string BuildTasksText(
            IReadOnlyList<CoreTask> tasks,
            IReadOnlyList<HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanAssignmentViewModel> assignments)
        {
            var sb = new StringBuilder();

            foreach (var assignment in assignments)
            {
                sb.Append("○ ");
                sb.Append(assignment.Title);
                if (!string.IsNullOrWhiteSpace(assignment.ProgressText))
                {
                    sb.Append(" — ");
                    sb.Append(assignment.ProgressText);
                }
                sb.AppendLine();
            }

            if (tasks.Count == 0 && sb.Length == 0)
                return "• Нет активных задач на сегодня";

            foreach (CoreTask task in tasks)
            {
                string mark = task.IsFailed ? "✗" : task.IsCompleted ? "✓" : "○";
                sb.Append(mark);
                sb.Append(' ');
                sb.Append(task.Title);
                if (task.IsFailed)
                    sb.Append(" — нарушено");
                else if (task.Id == Core.RecoveryPlanTaskIds.VisitHarveyIfReady)
                    sb.Append(" — нужно поговорить с Харви");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildHint(CoreDto dto)
        {
            if (dto.RequiresHarveyTalk || dto.ReadyForNextPhase || dto.ReadyForRecovery)
                return "Подсказка: поговори с Харви, когда будешь готова. Закрыть — ESC.";

            return "Подсказка: соблюдай режим — окно помогает спланировать день. Закрыть — ESC.";
        }

        private void SetField(ref string field, string value, [CallerMemberName] string? propertyName = null)
        {
            if (string.Equals(field, value, StringComparison.Ordinal))
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
