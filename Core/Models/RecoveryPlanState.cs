using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Save-state «Плана восстановления Харви» — отслеживание правил дня, не бафф.
    /// </summary>
    public class RecoveryPlanState
    {
        /// <summary>Уникальный id текущего плана (например RecoveryPlan_HospitalDischarge).</summary>
        public string PlanId { get; set; } = "";

        /// <summary>Источник плана: травма, стресс или смешанный.</summary>
        public RecoveryPlanSource Source { get; set; } = RecoveryPlanSource.None;

        /// <summary>Основная травма, к которой привязан план.</summary>
        public string? ActiveInjuryId { get; set; }

        /// <summary>Игровой день начала текущего плана.</summary>
        public int PlanStartDay { get; set; } = -1;

        /// <summary>Alias для PlanStartDay.</summary>
        public int StartedDay
        {
            get => PlanStartDay;
            set => PlanStartDay = value;
        }

        /// <summary>Текущий день плана (1-based внутри TotalDays или фазы).</summary>
        public int CurrentDay { get; set; } = 0;

        /// <summary>Общая длительность текущего этапа плана в днях.</summary>
        public int TotalDays { get; set; } = 0;

        /// <summary>Текущая фаза лечения (0 — без фаз).</summary>
        public int CurrentPhase { get; set; } = 0;

        /// <summary>Общее число фаз лечения.</summary>
        public int TotalPhases { get; set; } = 0;

        /// <summary>План активен и правила дня действуют.</summary>
        public bool IsActive { get; set; } = false;

        /// <summary>Нужен визит или разговор с Харви.</summary>
        public bool NeedsHarveyVisit { get; set; } = false;

        /// <summary>Текущий настроенческий статус для UI.</summary>
        public RecoveryPlanMoodStatus Status { get; set; } = RecoveryPlanMoodStatus.None;

        /// <summary>Актуальные пункты плана на сегодня.</summary>
        public List<RecoveryPlanTask> Tasks { get; set; } = new();

        /// <summary>Нарушения режима за текущий игровой день.</summary>
        public List<RecoveryPlanViolation> TodayViolations { get; set; } = new();

        /// <summary>Последний день, когда план пересчитывался.</summary>
        public int LastUpdatedDay { get; set; } = -1;

        /// <summary>HUD/UI плана уже показывался сегодня.</summary>
        public bool WasShownToday { get; set; } = false;

        /// <summary>Скрытый счётчик настороженности Харви (0+).</summary>
        public int ConcernScore { get; set; } = 0;

        /// <summary>Засчитанные дни плана восстановления (ежедневный план).</summary>
        public int CreditedDays { get; set; } = 0;

        /// <summary>Дни плана, не засчитанные из-за нарушений.</summary>
        public int FailedDays { get; set; } = 0;

        /// <summary>План завершён без нарушений и предупреждений.</summary>
        public bool PerfectPlan { get; set; } = false;

        /// <summary>Число использованных продлений плана (alias ExtensionCount).</summary>
        public int MaxExtensionsUsed
        {
            get => ExtensionCount;
            set => ExtensionCount = value;
        }

        /// <summary>Тон Харви для UI плана.</summary>
        public RecoveryPlanToneKind HarveyTone { get; set; } = RecoveryPlanToneKind.Calm;

        /// <summary>Активные назначения (machine ids).</summary>
        public List<string> ActiveAssignments { get; set; } = new();

        /// <summary>Завершённые сегодня назначения.</summary>
        public List<string> CompletedAssignmentsToday { get; set; } = new();

        /// <summary>Мягкие предупреждения за сегодня (читаемый текст).</summary>
        public List<string> TodayWarnings { get; set; } = new();

        /// <summary>Текущий прогресс по назначениям (id → значение).</summary>
        public Dictionary<string, int> Progress { get; set; } = new();

        /// <summary>Целевые значения назначений (id → цель).</summary>
        public Dictionary<string, int> Goals { get; set; } = new();

        /// <summary>Дополнительные дни плана из-за тяжёлых нарушений.</summary>
        public int PlanExtensionDays { get; set; } = 0;

        /// <summary>Число продлений плана из-за severe-нарушений.</summary>
        public int ExtensionCount { get; set; } = 0;

        /// <summary>Лимит продлений достигнут — план больше не удлиняется.</summary>
        public bool MaxExtensionsReached { get; set; } = false;

        /// <summary>Обязателен контрольный визит к Харви (после лимита продлений).</summary>
        public bool NeedsStrictFollowUp { get; set; } = false;

        /// <summary>Игровой день, к которому нужен контрольный осмотр (-1 = нет).</summary>
        public int RequiredFollowUpDay { get; set; } = -1;

        /// <summary>Последний день продления плана (не более одного за день).</summary>
        public int LastExtensionDay { get; set; } = -1;

        /// <summary>Текущий день не засчитан.</summary>
        public bool TodayFailed { get; set; } = false;

        /// <summary>Текущий день засчитан.</summary>
        public bool TodayCompleted { get; set; } = false;

        /// <summary>Последний день оценки прогресса плана.</summary>
        public int LastEvaluatedDay { get; set; } = -1;

        /// <summary>Последний день HUD-предупреждения о низкой stamina (мягкая страховка).</summary>
        public int LastStaminaWarningDay { get; set; } = -1;

        /// <summary>Последний день HUD-предупреждения о низком здоровье (мягкая страховка).</summary>
        public int LastHealthWarningDay { get; set; } = -1;

        /// <summary>Причины нарушений режима за текущий игровой день (машинные коды).</summary>
        public List<string> TodayViolationReasons { get; set; } = new();

        /// <summary>Последний день утреннего HUD-напоминания о плане.</summary>
        public int LastMorningHudDay { get; set; } = -1;

        /// <summary>Последний день HUD «причина записана в плане» после нарушения.</summary>
        public int LastViolationHudDay { get; set; } = -1;

        /// <summary>Тип последнего нарушения (<see cref="RecoveryPlanViolationType"/>).</summary>
        public string LastViolationType { get; set; } = "";

        /// <summary>Тяжесть последнего нарушения (1 mild, 2 medium, 3 severe).</summary>
        public int LastViolationSeverity { get; set; } = 0;

        /// <summary>Типы нарушений за текущий игровой день.</summary>
        public List<string> TodayViolationTypes { get; set; } = new();

        /// <summary>Сегодня было мягкое предупреждение без провала дня.</summary>
        public bool HadWarningsToday { get; set; } = false;

        /// <summary>Засчитанные дни без предупреждений и нарушений.</summary>
        public int PerfectDays { get; set; } = 0;

        /// <summary>Засчитанные дни с предупреждениями, но без провала.</summary>
        public int WarningDays { get; set; } = 0;

        /// <summary>За весь план были предупреждения (кумулятивно).</summary>
        public bool HadPlanWarnings { get; set; } = false;

        /// <summary>За весь план были настоящие нарушения, ломавшие прогресс (кумулятивно).</summary>
        public bool HadPlanViolations { get; set; } = false;

        /// <summary>Главный тип нарушения для CP-диалога сегодня.</summary>
        public string TodayViolationDialogueType { get; set; } = "";

        /// <summary>Тяжесть главного нарушения для CP-диалога сегодня.</summary>
        public int TodayViolationDialogueSeverity { get; set; } = 0;

        /// <summary>Общее число нарушений за текущий план (синхронизируется с InjuryState).</summary>
        public int TotalViolations { get; set; } = 0;

        /// <summary>Подряд засчитанных дней без предупреждений и провалов.</summary>
        public int ConsecutivePerfectDays { get; set; } = 0;

        /// <summary>Текущий план завершён идеально (TotalViolations == 0, без предупреждений).</summary>
        public bool WasPerfectOnCompletion { get; set; } = false;

        /// <summary>Последнее завершение плана было идеальным.</summary>
        public bool LastCompletionWasPerfect { get; set; } = false;

        /// <summary>Последнее завершение плана было с предупреждениями.</summary>
        public bool LastCompletionHadWarnings { get; set; } = false;

        /// <summary>До какого дня Харви говорит мягче после идеального плана (-1 = нет).</summary>
        public int SoftToneUntilDay { get; set; } = -1;

        /// <summary>Награда за завершение текущего плана уже выдана.</summary>
        public bool CompletionRewardApplied { get; set; } = false;

        /// <summary>Ждёт обязательного разговора с Харви после завершения плана.</summary>
        public bool CompletionTalkPending { get; set; } = false;

        /// <summary>Исход завершения, ожидающий разговора (<see cref="RecoveryPlanCompletionResult"/>).</summary>
        public string PendingCompletionResult { get; set; } = "";

        /// <summary>День показа финальной реплики Харви (не спамить каждый день).</summary>
        public int LastCompletionDialogueDay { get; set; } = -1;

        /// <summary>Засчитанные дни плана (alias <see cref="CreditedDays"/>).</summary>
        public int CompletedDays
        {
            get => CreditedDays;
            set => CreditedDays = value;
        }

        /// <summary>Требуемые дни плана (alias <see cref="TotalDays"/>).</summary>
        public int RequiredDays
        {
            get => TotalDays;
            set => TotalDays = value;
        }
    }
}
