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
        /// ID основной травмы (не осложнения). Одновременно может быть только одна.
        /// </summary>
        public string? MainInjuryId { get; set; }

        /// <summary>
        /// Активные медицинские предписания Харви (ключ — PrescriptionIds.*).
        /// </summary>
        public Dictionary<string, PrescriptionState> ActivePrescriptions { get; set; } = new();

        /// <summary>
        /// Медицинский показатель соблюдения лечения (−10…10).
        /// Не Friendship: только тон Харви, topics, предписания, риски и мягкие бонусы.
        /// </summary>
        public int TreatmentComplianceScore { get; set; } = 0;

        /// <summary>День последнего напоминания о предписании. -1 = ещё не напоминали.</summary>
        public int LastPrescriptionReminderDay { get; set; } = -1;

        /// <summary>День последнего +1 к TreatmentComplianceScore за визит Checkup.</summary>
        public int LastCheckupComplianceDay { get; set; } = -1;

        /// <summary>День последнего HUD-предупреждения при нарушении режима лечения.</summary>
        public int LastLowComplianceHudDay { get; set; } = -1;

        /// <summary>Защита самопомощи: тип → день, когда она активна (SelfCareProtectionTypes.*).</summary>
        public Dictionary<string, int> SelfCareProtections { get; set; } = new();

        /// <summary>+1 к TreatmentComplianceScore за смену повязки — начислить при следующем визите к Харви.</summary>
        public bool PendingSelfCareBandageCompliance { get; set; } = false;

        /// <summary>День последней самопомощи: смена повязки.</summary>
        public int LastSelfCareBandageDay { get; set; } = -1;

        /// <summary>День последней самопомощи: тёплый чай.</summary>
        public int LastSelfCareTeaDay { get; set; } = -1;

        /// <summary>День последней самопомощи: ранний отдых.</summary>
        public int LastRestSelfCareDay { get; set; } = -1;

        /// <summary>День отправки медицинского письма: dedupeKey → DaysPlayed (не чаще 1×/день на повод).</summary>
        public Dictionary<string, int> SentMedicalMailDays { get; set; } = new();

        /// <summary>Последняя proximity-реакция Харви (игровые минуты с полуночи).</summary>
        public int LastProximityReactionMinute { get; set; } = -1;

        /// <summary>День последней строгой proximity-реакции.</summary>
        public int LastStrictReactionDay { get; set; } = -1;

        /// <summary>Причина последней proximity-реакции (debug).</summary>
        public string LastProximityReactionReason { get; set; } = "";

        /// <summary>BuffId травмы, после которой назначена реабилитация; null — реабилитации нет.</summary>
        public string? ActiveRehabInjuryId { get; set; }

        /// <summary>День начала реабилитации.</summary>
        public int RehabStartDay { get; set; } = -1;

        /// <summary>Длительность реабилитации в игровых днях.</summary>
        public int RehabDurationDays { get; set; } = 0;

        /// <summary>Было ли хотя бы одно нарушение режима реабилитации.</summary>
        public bool RehabViolated { get; set; } = false;

        /// <summary>Число зафиксированных нарушений реабилитации.</summary>
        public int RehabViolationCount { get; set; } = 0;

        /// <summary>День последнего нарушения реабилитации (не чаще 1×/день).</summary>
        public int LastRehabViolationDay { get; set; } = -1;

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
        /// День первого строгого предупреждения при входе в шахту/вулкан с Severe (до MineForbidden).
        /// -1 = сегодня ещё не предупреждали.
        /// </summary>
        public int LastMineSevereWarningDay { get; set; } = -1;

        /// <summary>
        /// День принудительного выхода из шахты после повторного нарушения Severe-предупреждения.
        /// -1 = сегодня принудительно не выгоняли.
        /// </summary>
        public int LastMineSevereForcedExitDay { get; set; } = -1;

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

