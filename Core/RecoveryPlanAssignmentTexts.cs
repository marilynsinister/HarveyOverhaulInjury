using HarveyOverhaul.InjuryCare.Core.Models;

namespace HarveyOverhaul.InjuryCare.Core
{
    internal static class RecoveryPlanAssignmentTexts
    {
        public static string GetTitle(string assignmentId)
        {
            if (assignmentId == RecoveryPlanAssignmentIds.FindSafePlace)
                return "Найти безопасное место";

            if (assignmentId == RecoveryPlanAssignmentIds.DontStayAlone)
                return "Не оставаться одной";

            if (RecoveryPlanAssignmentIds.IsHarveyTalkAssignment(assignmentId))
            {
                if (assignmentId.Contains("_Recovery_", StringComparison.Ordinal))
                    return "Финальный осмотр у Харви: подтвердить выздоровление";

                return "Контрольный осмотр у Харви: следующая фаза лечения";
            }

            return assignmentId;
        }

        public static string GetDescription(string assignmentId)
        {
            return assignmentId switch
            {
                RecoveryPlanAssignmentIds.FindSafePlace =>
                    "Проведите время в тихом безопасном месте: дом, клиника, лес.",
                RecoveryPlanAssignmentIds.DontStayAlone =>
                    "Мягкий контакт сегодня — рядом с Харви или доверенными людьми.",
                _ when RecoveryPlanAssignmentIds.IsHarveyTalkAssignment(assignmentId) =>
                    "Поговорите с Харви, когда будете готовы.",
                _ => "",
            };
        }

        public static string FormatProgress(int progress, int goal)
        {
            if (goal <= 0)
                return "";

            return $"{Math.Min(progress, goal)}/{goal} сек";
        }
    }
}
