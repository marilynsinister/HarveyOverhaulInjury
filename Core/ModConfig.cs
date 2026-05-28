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
        public bool SendLetters { get; set; } = true;

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
        /// <summary>Сколько игровых дней действует дебафф «Харви запретил шахту» после письма (по умолчанию 2).</summary>
        public int MineForbiddenDurationDays { get; set; } = 2;
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
    }
}

