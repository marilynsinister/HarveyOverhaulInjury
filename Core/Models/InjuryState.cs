using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Состояние системы травм игрока
    /// </summary>
    public class InjuryState
    {
        public int DaysWithSevere { get; set; } = 0;
        public int LastNightRoundDay { get; set; } = -1;

        /// <summary>
        /// День, когда уже был сделан ночной бросок на визит Харви.
        /// Отдельно от LastNightRoundDay: roll может не сработать, но повторять его в эту же ночь нельзя.
        /// </summary>
        public int LastNightRoundRollDay { get; set; } = -1;

        public int WetBandageMailDay { get; set; } = -1;
        public int WetStitchesMailDay { get; set; } = -1;
        public int NeglectStrikes { get; set; } = 0;
        public bool PassedOutInTownYesterday { get; set; } = false;
        
        // ⭐ Счетчик времени под дождем для постепенного промокания повязки
        public int TimeUnderRainTicks { get; set; } = 0;
        public int LastRainCheckTime { get; set; } = -1;
        
        // ⭐ Счётчик общего времени под дождём для простуды (за день)
        public int TotalTimeUnderRainToday { get; set; } = 0;
        public int LastRainDay { get; set; } = -1;

        /// <summary>День последнего C#-броска storm comfort (успешного).</summary>
        public int LastStormComfortRollDay { get; set; } = -1;

        /// <summary>День последней проигранной storm comfort сцены.</summary>
        public int LastStormComfortEventDay { get; set; } = -1;
        
        public HashSet<string> AppliedTriggers { get; set; } = new();

        /// <summary>
        /// Cooldown повторяемых травм: ключ buffId/triggerId → первый день, когда травму снова можно применить.
        /// </summary>
        public Dictionary<string, int> InjuryCooldownUntilDay { get; set; } = new();

        /// <summary>
        /// Устарело: мигрируется в <see cref="InjuryCooldownUntilDay"/> при загрузке.
        /// </summary>
        public Dictionary<string, int> LastInjuryAppliedDayByTrigger { get; set; } = new();

        /// <summary>
        /// Снапшот активных баффов мода на конец дня.
        /// Восстанавливается каждое утро.
        /// </summary>
        public List<string> SavedActiveBuffs { get; set; } = new();
        
        /// <summary>
        /// Активные дебаффы/травмы с полным состоянием
        /// Ключ - BuffId травмы, Значение - полное состояние с флагами
        /// </summary>
        public Dictionary<string, DebuffState> ActiveDebuffs { get; set; } = new();

        /// <summary>
        /// Активные осложнения (WetBandage, DirtyWound, WetStitches и т.д.)
        /// Ключ — ID баффа осложнения. Значение — день появления (для таймера заражения).
        /// </summary>
        public Dictionary<string, int> ActiveComplications { get; set; } = new();
        
        public int LastProximityCheckDay { get; set; } = -1;
        public int LastSupportDay { get; set; } = -1;

        // Данные об обмороке
        public bool WasPassedOut { get; set; } = false;
        public bool WasExhausted { get; set; } = false;
        public bool WasUpTooLate { get; set; } = false;
        public int LastPassedOutHealth { get; set; } = -1;
        public string LastPassedOutLocation { get; set; } = "";

        // ⭐ НОВОЕ: События спасения в шахте
        public bool PassedOutInMineYesterday { get; set; } = false;
        public bool NeedsMineRescueEvent { get; set; } = false;

        /// <summary>
        /// ID CP-события, ожидающего запуска в Mine (переживает reload между warp и startEvent).
        /// </summary>
        public string PendingMineRescueEventId { get; set; } = "";

        /// <summary>
        /// ID minor mine rescue cutscene (отдельно от боевой смерти в шахте).
        /// </summary>
        public string PendingMinorMineRescueEventId { get; set; } = "";

        /// <summary>День последней попытки/проигрыша eventHarveyMinorMineRescue.</summary>
        public int LastMinorMineRescueDay { get; set; } = -1;

        /// <summary>
        /// ID CP-события pass-out, ожидающего запуска в Hospital (переживает reload между warp и startEvent).
        /// </summary>
        public string PendingHospitalPassOutEventId { get; set; } = "";

        /// <summary>
        /// Fallback-ветка для pending hospital pass-out: "critical" | "exhaustion".
        /// </summary>
        public string PendingHospitalPassOutFallbackKind { get; set; } = "";

        /// <summary>
        /// День, когда было показано строгое предупреждение «не ходи в шахту» (Severe).
        /// На следующий день приходит письмо от Харви и накладывается дебафф MineForbidden.
        /// -1 = не было предупреждения.
        /// </summary>
        public int MineWarningDay { get; set; } = -1;

        /// <summary>
        /// День, когда был наложен дебафф «Харви запретил шахту».
        /// Снимается через MineForbiddenDurationDays дней. -1 = не наложен.
        /// </summary>
        public int MineForbiddenAppliedDay { get; set; } = -1;

        /// <summary>
        /// День последнего показа катсцены eventHarveyMineInterception при активном MineForbidden.
        /// -1 = ещё не показывали. Не пишется в eventsSeen, чтобы событие не стало one-shot на сейв.
        /// </summary>
        public int LastMineForbiddenInterceptionDay { get; set; } = -1;

        /// <summary>
        /// Накопленные игровые минуты в шахте/вулкане за текущий день (только при травме DirtyInMines).
        /// </summary>
        public int MineDirtyExposureMinutesToday { get; set; } = 0;

        /// <summary>
        /// День последнего учёта экспозиции в шахте; для сброса счётчика в новый день.
        /// </summary>
        public int LastMineDirtyExposureDay { get; set; } = -1;

        /// <summary>
        /// Последняя игровая минута, когда уже делали бросок на грязную рану.
        /// </summary>
        public int LastMineDirtyWoundRollMinute { get; set; } = -1;

        /// <summary>
        /// Игровая минута, до которой действует временный буст риска после урона в шахте.
        /// </summary>
        public int MineDirtyRiskBoostUntilMinute { get; set; } = -1;

        // Отслеживание здоровья для травм
        public int LastHealth { get; set; } = 100;

        // Госпитализация (переживает перезагрузку сейва)
        public bool IsHospitalized { get; set; } = false;
        public string HospitalizedInjuryId { get; set; } = "";
        public string HospitalizationReason { get; set; } = "";
        public int HospitalAdmissionDay { get; set; } = -1;
        public int HospitalAdmissionTime { get; set; } = -1;
        public int HospitalAdmissionMinutes { get; set; } = -1;
        public int HospitalMinStayMinutes { get; set; } = 120;
        public bool HospitalDischargeReadyShown { get; set; } = false;

        /// <summary>
        /// Proximity-предупреждение перед принудительной госпитализацией (уже показано).
        /// </summary>
        public bool PendingForcedHospitalizationWarning { get; set; } = false;

        /// <summary>
        /// День показа proximity-предупреждения о госпитализации. -1 = не показывали.
        /// </summary>
        public int PendingForcedHospitalizationWarningDay { get; set; } = -1;

        // Память о топиках (для старой логики)
        public Dictionary<string, int> TopicMemory { get; set; } = new();
        
        // УСТАРЕЛО: Используйте ActiveDebuffs вместо этих полей
        // Оставлено для совместимости при загрузке старых сохранений
        [System.Obsolete("Используйте ActiveDebuffs[buffId].HarveyConversationHappened")]
        public Dictionary<string, bool> TreatmentConversations { get; set; } = new();
        
        // Свойство для миграции старых сохранений (тип определяется динамически)
        [System.Obsolete("Используйте ActiveDebuffs вместо ActivePhases")]
        public Dictionary<string, dynamic> ActivePhases { get; set; } = new();
    }
}

