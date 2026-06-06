using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CoreDto = HarveyOverhaul.InjuryCare.Core.Models.RecoveryPlanViewModel;

namespace HarveyOverhaul.InjuryCare.UI.RecoveryPlan
{
    /// <summary>
    /// View model для окна StardewUI «План восстановления».
    /// </summary>
    public sealed class RecoveryPlanViewModel : INotifyPropertyChanged
    {
        private string _title = "План восстановления Харви";
        private string _planTypeLabel = "";
        private string _progressText = "";
        private string _todayStatusText = "";
        private string _rulesText = "";
        private string _todayViolationsText = "";
        private string _hintText = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public string PlanTypeLabel
        {
            get => _planTypeLabel;
            set => SetField(ref _planTypeLabel, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => SetField(ref _progressText, value);
        }

        public string TodayStatusText
        {
            get => _todayStatusText;
            set => SetField(ref _todayStatusText, value);
        }

        public string RulesText
        {
            get => _rulesText;
            set => SetField(ref _rulesText, value);
        }

        public string TodayViolationsText
        {
            get => _todayViolationsText;
            set => SetField(ref _todayViolationsText, value);
        }

        public string HintText
        {
            get => _hintText;
            set => SetField(ref _hintText, value);
        }

        public static RecoveryPlanViewModel FromDto(CoreDto dto)
        {
            var vm = new RecoveryPlanViewModel
            {
                Title = "План восстановления Харви",
                PlanTypeLabel = $"Тип плана: {ResolvePlanTypeLabel(dto.Reason, dto.PlanId)}",
                ProgressText = $"Прогресс: {dto.CompletedDays} / {dto.RequiredDays}",
                TodayStatusText = ResolveTodayStatus(dto),
                RulesText = BuildRulesText(),
                TodayViolationsText = BuildTodayViolationsText(dto.TodayViolationReasons),
                HintText = ResolveHint(dto),
            };

            return vm;
        }

        private static string ResolvePlanTypeLabel(string reason, string planId)
        {
            if (string.Equals(reason, "hospital", StringComparison.OrdinalIgnoreCase)
                || string.Equals(planId, Managers.RecoveryPlanManager.HospitalDischargePlanId, StringComparison.OrdinalIgnoreCase))
            {
                return "После выписки";
            }

            return string.IsNullOrWhiteSpace(reason) ? "Восстановление" : reason;
        }

        private static string ResolveTodayStatus(CoreDto dto)
        {
            if (dto.CompletionTalkPending)
                return "План завершён — поговори с Харви";

            if (dto.TodayFailed)
                return "Сегодня режим сорван";

            return "Сегодня режим соблюдается";
        }

        private static string ResolveHint(CoreDto dto)
        {
            if (dto.CompletionTalkPending)
                return "Поговори с Харви, чтобы снять режим.";

            if (dto.TodayFailed)
                return "Харви захочет обсудить это лично.";

            return "Харви будет доволен, если день закончится спокойно.";
        }

        private static string BuildRulesText()
        {
            return string.Join(
                Environment.NewLine,
                "• Не ходить в шахту",
                "• Не доводить себя до истощения",
                "• Не падать в обморок",
                "• Не получать новые тяжёлые травмы");
        }

        private static string BuildTodayViolationsText(IReadOnlyList<string> reasons)
        {
            if (reasons.Count == 0)
                return "";

            var sb = new StringBuilder("Нарушения за сегодня: ");
            for (int i = 0; i < reasons.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(reasons[i]);
            }

            return sb.ToString();
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
