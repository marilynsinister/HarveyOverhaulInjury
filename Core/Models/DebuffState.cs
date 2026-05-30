namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Полное состояние дебаффа/травмы с флагами
    /// Хранится в сохранении игры
    /// </summary>
    public class DebuffState
    {
        /// <summary>
        /// ID баффа травмы (например, "buffDeepCuts")
        /// </summary>
        public string BuffId { get; set; } = "";
        
        /// <summary>
        /// День получения травмы
        /// </summary>
        public int InjuryStartDay { get; set; } = 0;
        
        // ============================================================================
        // ФЛАГИ СОСТОЯНИЯ ЛЕЧЕНИЯ
        // ============================================================================
        
        /// <summary>
        /// Было ли начато лечение (разговор с Харви и начало фазового лечения)
        /// </summary>
        public bool TreatmentStarted { get; set; } = false;
        
        /// <summary>
        /// Был ли разговор с Харви о лечении этой травмы
        /// </summary>
        public bool HarveyConversationHappened { get; set; } = false;
        
        // ============================================================================
        // ИНФОРМАЦИЯ О ФАЗАХ
        // ============================================================================
        
        /// <summary>
        /// Общее количество фаз лечения (2 или 3)
        /// </summary>
        public int TotalPhases { get; set; } = 0;
        
        /// <summary>
        /// Текущая фаза лечения (0 = нелеченная травма, 1-3 = фазы лечения)
        /// </summary>
        public int CurrentPhase { get; set; } = 0;
        
        /// <summary>
        /// День начала текущей фазы
        /// </summary>
        public int PhaseStartDay { get; set; } = 0;
        
        /// <summary>
        /// Длительность фазы 1 в днях
        /// </summary>
        public int Phase1Duration { get; set; } = 0;
        
        /// <summary>
        /// Длительность фазы 2 в днях
        /// </summary>
        public int Phase2Duration { get; set; } = 0;
        
        /// <summary>
        /// Длительность фазы 3 в днях (0 если фаз только 2)
        /// </summary>
        public int Phase3Duration { get; set; } = 0;
        
        /// <summary>
        /// Флаг готовности к смене фазы (устанавливается системой времени)
        /// </summary>
        public bool ReadyForNextPhase { get; set; } = false;
        
        /// <summary>
        /// Флаг готовности к полному выздоровлению
        /// </summary>
        public bool ReadyForRecovery { get; set; } = false;

        /// <summary>День, когда выставлен ReadyForNextPhase или ReadyForRecovery.</summary>
        public int ReadySinceDay { get; set; } = -1;

        /// <summary>Дней просрочки контрольного осмотра (today - ReadySinceDay).</summary>
        public int MissedCheckupDays { get; set; } = 0;

        /// <summary>Мягкое HUD-напоминание на 2-й день просрочки уже показано.</summary>
        public bool CheckupReminderSent { get; set; } = false;

        /// <summary>Письмо о просрочке (4-й день) уже запланировано.</summary>
        public bool CheckupLateLetterSent { get; set; } = false;

        /// <summary>Штраф TreatmentComplianceScore / Neglect за 5+ дней просрочки уже применён.</summary>
        public bool CheckupOverduePenaltyApplied { get; set; } = false;
        
        // ============================================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ============================================================================
        
        /// <summary>
        /// Проверить, является ли травма фазовой (имеет фазы лечения)
        /// </summary>
        public bool IsPhasedInjury => TotalPhases > 0;
        
        /// <summary>
        /// Проверить, находится ли травма в активной фазе лечения
        /// </summary>
        public bool IsInTreatment => TreatmentStarted && CurrentPhase > 0;
        
        /// <summary>
        /// Получить длительность текущей фазы
        /// </summary>
        public int GetCurrentPhaseDuration()
        {
            return CurrentPhase switch
            {
                1 => Phase1Duration,
                2 => Phase2Duration,
                3 => Phase3Duration,
                _ => 0
            };
        }
        
        /// <summary>
        /// Получить общую длительность лечения
        /// </summary>
        public int GetTotalDuration()
        {
            return Phase1Duration + Phase2Duration + Phase3Duration;
        }
        
        /// <summary>
        /// Проверить, прошло ли достаточно дней для смены фазы
        /// </summary>
        public bool HasPhaseTimeElapsed(int currentDay)
        {
            if (CurrentPhase <= 0 || CurrentPhase > TotalPhases)
                return false;
            
            int daysInPhase = currentDay - PhaseStartDay;
            int phaseDuration = GetCurrentPhaseDuration();
            
            return daysInPhase >= phaseDuration;
        }
        
        /// <summary>
        /// Проверить, является ли текущая фаза последней
        /// </summary>
        public bool IsLastPhase => CurrentPhase == TotalPhases;
        
        /// <summary>
        /// Начать лечение (переход с фазы 0 на фазу 1)
        /// </summary>
        public void StartTreatment(int currentDay)
        {
            TreatmentStarted = true;
            CurrentPhase = 1;
            PhaseStartDay = currentDay;
        }
        
        /// <summary>
        /// Перейти к следующей фазе
        /// </summary>
        public void AdvancePhase(int currentDay)
        {
            if (CurrentPhase < TotalPhases)
            {
                CurrentPhase++;
                PhaseStartDay = currentDay;
                ReadyForNextPhase = false;
                ReadyForRecovery = false;
            }
        }
        
        /// <summary>
        /// Создать копию состояния (для отладки)
        /// </summary>
        public DebuffState Clone()
        {
            return new DebuffState
            {
                BuffId = BuffId,
                InjuryStartDay = InjuryStartDay,
                TreatmentStarted = TreatmentStarted,
                HarveyConversationHappened = HarveyConversationHappened,
                TotalPhases = TotalPhases,
                CurrentPhase = CurrentPhase,
                PhaseStartDay = PhaseStartDay,
                Phase1Duration = Phase1Duration,
                Phase2Duration = Phase2Duration,
                Phase3Duration = Phase3Duration,
                ReadyForNextPhase = ReadyForNextPhase,
                ReadyForRecovery = ReadyForRecovery,
                ReadySinceDay = ReadySinceDay,
                MissedCheckupDays = MissedCheckupDays,
                CheckupReminderSent = CheckupReminderSent,
                CheckupLateLetterSent = CheckupLateLetterSent,
                CheckupOverduePenaltyApplied = CheckupOverduePenaltyApplied
            };
        }
    }
}

