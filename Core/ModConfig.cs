namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>
    /// Конфигурация мода
    /// </summary>
    public sealed class ModConfig
    {
        /// <summary>MCP HTTP server for Cursor/agents (injury_* debug tools). localhost only.</summary>
        public bool EnableInjuryMcp { get; set; } = true;

        /// <summary>Port for Injury MCP (default 24843; StardewMCP uses 24842).</summary>
        public int InjuryMcpPort { get; set; } = 24843;

        public bool OnlyAtClinic { get; set; } = true;

        public bool AllowBasicTreatmentOutsideClinic { get; set; } = true;
        public bool AllowPhaseTransitionOutsideClinic { get; set; } = true;
        public bool AllowRecoveryOutsideClinic { get; set; } = true;
        public bool RequireClinicForSevereInjuries { get; set; } = true;
        public bool BlockLongTreatmentDuringFestivals { get; set; } = true;
        /// <summary>Устарело: используйте <see cref="MedicalLetters"/>.</summary>
        public bool SendLetters
        {
            get => MedicalLetters != MedicalLetterMode.Off;
            set => MedicalLetters = value ? MedicalLetterMode.All : MedicalLetterMode.Off;
        }

        /// <summary>Сюжетные письма (CP events / story chain). C# не блокирует CP-триггеры.</summary>
        public bool SendStoryLetters { get; set; } = true;

        /// <summary>Романтические care-письма без медицинских требований (CP triggersCare).</summary>
        public bool SendRomanticCareLetters { get; set; } = true;

        /// <summary>Политика медицинских писем из C#. По умолчанию — только критические.</summary>
        public MedicalLetterMode MedicalLetters { get; set; } = MedicalLetterMode.CriticalOnly;

        // Принудительная госпитализация
        public bool ForceHospitalization { get; set; } = true;
        public int MinHospitalStayMinutes { get; set; } = 120;
        public int ProximityTiles { get; set; } = 3;
        public string HospitalLocationName { get; set; } = "Hospital";
        public int HospitalBedX { get; set; } = 20;
        public int HospitalBedY { get; set; } = 4;
        public int HospitalActivityIntervalMinutes { get; set; } = 40;
        public int MaxHospitalActivitiesPerStay { get; set; } = 3;

        // Настройки механик
        /// <summary>Сколько игровых дней действует жёсткий запрет «Харви запретил шахту» (не весь срок лечения).</summary>
        public int MineForbiddenDurationDays { get; set; } = 2;

        /// <summary>Если true — нарушение RecoveryPlan «не ходить в шахту» физически выносит игрока.</summary>
        public bool RecoveryPlanMineRuleBlocksEntry { get; set; } = false;

        /// <summary>Жёсткий MineForbidden только для тяжёлых травм из медицинского списка.</summary>
        public bool MineForbiddenOnlyForSevereInjuries { get; set; } = true;

        /// <summary>Дней жёсткого запрета шахты в острой фазе 1 (инфекция, ожог, шrapnel и т.п.).</summary>
        public int MineHardBlockAcuteDays { get; set; } = 2;

        /// <summary>Дней жёсткого запрета после крупной травмы (перелом, badly hurt, хирургия).</summary>
        public int MineHardBlockAfterMajorInjuryDays { get; set; } = 2;

        /// <summary>Разрешить обычную шахту на фазах лечения после острого окна (фаза 2+).</summary>
        public bool AllowMinesDuringHealingPhase { get; set; } = true;

        /// <summary>Показывать мягкие HUD-предупреждения Харви при входе в шахту во время лечения.</summary>
        public bool ShowMineWarningsDuringHealing { get; set; } = true;
        /// <summary>Максимальный (высокий) шанс грязной раны в шахте после DirtyWoundHighMineMinutes.</summary>
        public double DirtyWoundChanceMines { get; set; } = 0.35;

        /// <summary>До скольких игровых минут в шахте грязная рана невозможна.</summary>
        public int DirtyWoundSafeMineMinutes { get; set; } = 20;

        /// <summary>После SafeMineMinutes и до HighMineMinutes шанс грязной раны.</summary>
        public double DirtyWoundChanceMinesMedium { get; set; } = 0.10;

        /// <summary>После скольких игровых минут включается высокий риск.</summary>
        public int DirtyWoundHighMineMinutes { get; set; } = 60;

        /// <summary>Дополнительный шанс после получения урона в шахте.</summary>
        public double DirtyWoundMineDamageBonusChance { get; set; } = 0.15;

        /// <summary>На сколько игровых минут после урона в шахте повышается риск загрязнения.</summary>
        public int DirtyWoundMineDamageBoostMinutes { get; set; } = 30;

        /// <summary>Как часто делать бросок грязной раны в шахте, в игровых минутах.</summary>
        public int DirtyWoundMineRollIntervalMinutes { get; set; } = 10;

        /// <summary>
        /// Legacy: старый единый шанс инфекции от мокрой повязки.
        /// Новая логика использует дневную шкалу в ComplicationManager.CalculateWetBandageInfectionChance.
        /// Оставлено для совместимости существующих config.json.
        /// </summary>
        public double WetBandageToInfectionChance { get; set; } = 0.25;
        public double DirtyWoundToInfectionChance { get; set; } = 0.25;
        public double SpringRashChance { get; set; } = 0.35;
        public int NeglectDaysThreshold { get; set; } = 3;

        // Скрытое медицинское доверие Харви (CareTrust)
        public bool EnableCareTrust { get; set; } = true;
        public int CareTrustMin { get; set; } = -5;
        public int CareTrustMax { get; set; } = 8;
        public int CareTrustLowThreshold { get; set; } = -2;
        public int CareTrustHighThreshold { get; set; } = 4;

        /// <summary>
        /// Шанс ночного визита Харви за одну ночь при серьёзной травме.
        /// Значение от 0.0 до 1.0. По умолчанию 0.35 = 35%.
        /// </summary>
        public double NightVisitChance { get; set; } = 0.35;

        /// <summary>
        /// Через сколько игровых дней та же repeatable-травма может выпасть снова.
        /// AppliedTriggers для story-флагов этим не управляет.
        /// </summary>
        public int RepeatableInjuryCooldownDays { get; set; } = 7;

        /// <summary>
        /// Разрешать ли повторно накладывать ту же травму, пока она уже активна или лечится.
        /// Обычно false, чтобы не перезаписывать DebuffState.
        /// </summary>
        public bool AllowSameInjuryWhileActive { get; set; } = false;

        // Farming injuries by tool-use counters
        public bool EnableFarmingToolUseInjuries { get; set; } = true;
        public int BackStrainToolUsesThreshold { get; set; } = 30;
        public int TornMusclesToolUsesThreshold { get; set; } = 20;
        public int DeepCutsToolUsesThreshold { get; set; } = 25;

        public double BackStrainBaseChance { get; set; } = 0.15;
        public double TornMusclesBaseChance { get; set; } = 0.12;
        public double DeepCutsFarmingBaseChance { get; set; } = 0.15;

        public float BackStrainStaminaThreshold { get; set; } = 30f;
        public float TornMusclesStaminaThreshold { get; set; } = 20f;
        public float DeepCutsStaminaThreshold { get; set; } = 15f;

        public int FarmingInjuryRollCooldownMinutes { get; set; } = 10;
        public double SkillChanceReductionPerLevel { get; set; } = 0.04;
        public double MinSkillChanceMultiplier { get; set; } = 0.55;
        public int SkillThresholdBonusPerTwoLevels { get; set; } = 1;
        public int MaxSkillThresholdBonus { get; set; } = 5;

        /// <summary>
        /// Устарело: hotkey H теперь у HarveyStressMeter (общее окно «План Харви»).
        /// Используйте EnableStandaloneRecoveryPlanWindow + StandaloneRecoveryPlanKey для fallback UI.
        /// </summary>
        public string OpenRecoveryPlanKey { get; set; } = "";

        /// <summary>Устарело: используйте EnableStandaloneRecoveryPlanWindow + StandaloneRecoveryPlanKey.</summary>
        public string RecoveryPlanKey
        {
            get => StandaloneRecoveryPlanKey;
            set => StandaloneRecoveryPlanKey = value;
        }

        /// <summary>
        /// Отдельное StardewUI-окно RecoveryPlan в Injury (fallback для тестов).
        /// По умолчанию выключено — окно открывает HarveyStressMeter по H.
        /// </summary>
        public bool EnableStandaloneRecoveryPlanWindow { get; set; } = false;

        /// <summary>
        /// Клавиша fallback-окна RecoveryPlan (только если EnableStandaloneRecoveryPlanWindow=true).
        /// Пустая строка = hotkey не назначен.
        /// </summary>
        public string StandaloneRecoveryPlanKey { get; set; } = "";

        /// <summary>Авто-показ окна плана утром после начала лечения (требует StardewUI).</summary>
        public bool AutoShowRecoveryPlanMorning { get; set; } = false;

        /// <summary>Максимум продлений плана из-за тяжёлых нарушений (severe).</summary>
        public int MaxRecoveryPlanExtensions { get; set; } = 2;

        /// <summary>Разрешить продление плана при severe-нарушениях.</summary>
        public bool EnableRecoveryPlanExtensions { get; set; } = true;

        /// <summary>Severe-нарушение может добавить +1 день к плану (в пределах MaxRecoveryPlanExtensions).</summary>
        public bool SevereViolationExtendsRecoveryPlan { get; set; } = true;

        // Домашняя забота супруга (married romance, не лечение)
        public bool EnableSpouseDomesticCare { get; set; } = true;
        public bool EnableSpouseProximityLines { get; set; } = true;
        public bool AllowDomesticCareWhenEngaged { get; set; } = true;
        public bool RequireMarriedForIntimateLines { get; set; } = true;
        public double MorningSpouseLineChance { get; set; } = 0.35;
        public double EveningSpouseLineChance { get; set; } = 0.35;
        public double LateNightSpouseLineChance { get; set; } = 0.40;
        public double FarmProximityLineChance { get; set; } = 0.20;
        public int SpouseProximityCooldownMinutes { get; set; } = 120;
        public int MaxDomesticReactionsPerDay { get; set; } = 2;
        public int MorningDomesticStartTime { get; set; } = 600;
        public int MorningDomesticEndTime { get; set; } = 1000;
        public int EveningDomesticStartTime { get; set; } = 1800;
        public int EveningDomesticEndTime { get; set; } = 2200;
        public int LateNightDomesticStartTime { get; set; } = 2200;
        public int LateNightDomesticEndTime { get; set; } = 2600;
        public int FarmDomesticStartTime { get; set; } = 900;
        public int FarmDomesticEndTime { get; set; } = 1800;
        public int DomesticProximityTiles { get; set; } = 4;
        /// <summary>Устарело: используйте AllowDomesticCareWhenEngaged.</summary>
        public bool AllowDomesticCareWhenDating { get; set; } = false;

        // Domestic hidden-injury checks (morning/evening at home)
        public bool EnableDomesticHiddenInjuryChecks { get; set; } = true;
        public bool EnableMorningHiddenInjuryCheck { get; set; } = true;
        public bool EnableEveningHiddenInjuryCheck { get; set; } = true;
        public int MorningHiddenInjuryStartTime { get; set; } = 600;
        public int MorningHiddenInjuryEndTime { get; set; } = 1000;
        public int EveningHiddenInjuryStartTime { get; set; } = 1900;
        public int EveningHiddenInjuryEndTime { get; set; } = 2400;
        public int DomesticHiddenInjuryProximityTiles { get; set; } = 6;
        public double DomesticSubtleDetectionChance { get; set; } = 0.35;
        public double DomesticSuspiciousDetectionChance { get; set; } = 0.65;
        public double DomesticObviousDetectionChance { get; set; } = 0.95;
    }
}

