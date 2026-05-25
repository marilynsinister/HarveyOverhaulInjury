using System;
using System.Collections.Generic;

namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>
    /// Нефазовые травмы и соответствующие им лечебные баффы (назначаются Харви за один визит)
    /// </summary>
    public static class SimpleInjuryCures
    {
        public static readonly System.Collections.Generic.Dictionary<string, string> Map = new()
        {
            ["buffHurt"]          = CureBuffs.Treatment,
            ["buffBadlyHurt"]     = CureBuffs.IntensiveCare,
            ["buffSurgicalWound"] = CureBuffs.PostSurgical,
        };
    }

    /// <summary>
    /// Константы для ID баффов лечения
    /// </summary>
    public static class CureBuffs
    {
        // Для простых травм
        public const string Treatment = "buffHarveyTreatment";
        public const string IntensiveCare = "buffHarveyIntensiveCare";
        public const string BadlyHurtOutpatientCare = "HarveyMod_BadlyHurt_OutpatientCare";
        public const string Protection = "buffHarveyProtection";
        public const string Recovery = "buffHarveyRecovery";

        // Для специфических травм
        public const string Teracitin = "buffTeracitin";
        public const string Antibiotics = "buffAntibioticsTreatment";
        public const string ForcedSedation = "buffForcedSedation";
        public const string PostSurgical = "buffPostSurgicalCare";
        
        // Бафф заботы после завершения лечения
        public const string Care = "buffHarveyCare";

        /// <summary>Восстановительный режим после тяжёлой травмы.</summary>
        public const string Rehab = "buffHarveyRehab";
    }

    /// <summary>ID предписаний Харви (временные правила лечения).</summary>
    public static class PrescriptionIds
    {
        public const string Rest = "HarveyMod_Prescription_Rest";
        public const string NoMine = "HarveyMod_Prescription_NoMine";
        public const string KeepDry = "HarveyMod_Prescription_KeepDry";
        public const string LightWork = "HarveyMod_Prescription_LightWork";
        public const string Checkup = "HarveyMod_Prescription_Checkup";
    }

    /// <summary>Conversation topics для предписаний Харви.</summary>
    public static class PrescriptionTopics
    {
        public const string Rest = "topicHarvey_Prescription_Rest";
        public const string NoMine = "topicHarvey_Prescription_NoMine";
        public const string KeepDry = "topicHarvey_Prescription_KeepDry";
        public const string LightWork = "topicHarvey_Prescription_LightWork";
        public const string Checkup = "topicHarvey_Prescription_Checkup";
        public const string Violation = "topicHarvey_PrescriptionViolation";
        public const string Followed = "topicHarvey_PrescriptionFollowed";
    }

    /// <summary>Conversation topics для контрольных осмотров между фазами.</summary>
    public static class CheckupTopics
    {
        public const string CheckupDue = "topicHarvey_CheckupDue";
        public const string RecoveryCheckupDue = "topicHarvey_RecoveryCheckupDue";

        public static string GetCheckupDueInjury(string buffId) =>
            $"topicHarvey_CheckupDue_{buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase)}";

        public static string GetCheckupPhase(int nextPhase) =>
            $"topicHarvey_CheckupPhase{nextPhase}";

        public static string GetRecoveryCheckupDueInjury(string buffId) =>
            $"topicHarvey_RecoveryCheckupDue_{buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase)}";
    }

    /// <summary>Conversation topics для плана лечения от Харви.</summary>
    public static class TreatmentPlanTopics
    {
        public const string Given = "topicHarvey_TreatmentPlanGiven";

        public static string GetInjuryTopic(string buffId) =>
            $"topicHarvey_TreatmentPlan_{buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase)}";
    }

    /// <summary>Conversation topics по уровню соблюдения лечения (не Friendship).</summary>
    public static class ComplianceTopics
    {
        public const string High = "topicHarvey_ComplianceHigh";
        public const string Neutral = "topicHarvey_ComplianceNeutral";
        public const string Low = "topicHarvey_ComplianceLow";
        /// <summary>Усиленный медицинский контроль после повторных нарушений (не наказание за отношения).</summary>
        public const string StrictMedicalMode = "topicHarvey_StrictMedicalMode";
        /// <summary>Стабильное соблюдение лечения при выздоровлении (ID исторический; не романтическое доверие).</summary>
        public const string TrustedPatient = "topicHarvey_TrustedPatient";
    }

    /// <summary>
    /// Константы для ID новых баффов (осложнений)
    /// </summary>
    public static class InjuryBuffs
    {
        public const string WetBandage = "HarveyMod_WetBandage";
        public const string DirtyWound = "HarveyMod_DirtyWound";
        public const string Neglect = "HarveyMod_Neglect";
        public const string PainFlare = "HarveyMod_PainFlare";
        public const string AllergicRash = "HarveyMod_AllergicRash";
        public const string WetStitches = "HarveyMod_WetStitches";
        public const string Cold = "buffCold"; // Простуда (2 фазы: острая + восстановление)
        /// <summary>Дебафф «Харви запретил вход в шахту» — навешивается при входе в шахту/вулкан с серьёзной травмой.</summary>
        public const string MineForbidden = "HarveyMod_MineForbidden";

        /// <summary>Острая фаза простуды (фазовый бафф).</summary>
        public const string ColdAcute = "HarveyMod_Cold_Acute";
    }

    /// <summary>Баффы самопомощи (слабее лечения у Харви).</summary>
    public static class SelfCareBuffs
    {
        public const string SelfCare = "HarveyMod_SelfCare";
        public const string CleanBandage = "HarveyMod_CleanBandage";
        public const string WarmTea = "HarveyMod_WarmTea";
    }

    /// <summary>Типы временной защиты от осложнений после самопомощи.</summary>
    public static class SelfCareProtectionTypes
    {
        public const string CleanBandage = "CleanBandage";
        public const string WarmTea = "WarmTea";
    }

    /// <summary>
    /// Константы для разговорных топиков
    /// </summary>
    public static class ConversationTopics
    {
        // --- Осложнения (HarveyMod_* → topicHarvey_*) ---
        public const string WetBandage = "topicHarvey_WetBandage";
        public const string DirtyWound = "topicHarvey_DirtyWound";
        public const string AllergicRash = "topicHarvey_AllergicRash";
        public const string Neglect = "topicHarvey_Neglect";
        public const string WetStitches = "topicHarvey_WetStitches";
        public const string PainFlare = "topicHarvey_PainFlare";
        public const string ForcedHosp = "topicHarvey_ForcedHospitalization";

        // --- Базовые травмы (buff* → topic*) ---
        public const string Hurt = "topicHurt";
        public const string BadlyHurt = "topicBadlyHurt";
        public const string SprainedAnkle = "topicSprainedAnkle";
        public const string BruisedRibs = "topicBruisedRibs";
        public const string BackStrain = "topicBackStrain";
        public const string DeepCuts = "topicDeepCuts";
        public const string BurnWounds = "topicBurnWounds";
        public const string InfectedWound = "topicInfectedWound";
        public const string TornMuscles = "topicTornMuscles";
        public const string Concussion = "topicConcussion";
        public const string FracturedBone = "topicFracturedBone";
        public const string ShrapnelWounds = "topicShrapnelWounds";
        public const string SurgicalWound = "topicSurgicalWound";
        public const string Cold = "topicCold";

        // --- Сопутствующие / situational ---
        public const string HealthDamageCritical = "topicHealthDamageCritical";
        public const string HealthDamageSevere = "topicHealthDamageSevere";
        public const string PostOperativeCare = "topicPostOperativeCare";
        public const string FarmerExhausted = "topicFarmerExhausted";
        public const string PassedOutInTown = "topicPassedOutInTown";
        public const string TooCold = "topicTooCold";

        // --- Cured (финальный осмотр) ---
        public const string ColdCured = "topicColdCured";
        public const string SurgicalWoundCured = "topicSurgicalWoundCured";

        // --- Лечение / события ---
        public const string TreatmentCompleted = "topicTreatmentCompleted";

        // --- Реабилитация после тяжёлой травмы ---
        public const string Rehab = "topicHarvey_Rehab";
        public const string RehabStrict = "topicHarvey_RehabStrict";
        public const string RehabCompleted = "topicHarvey_RehabCompleted";

        // --- Самопомощь (домашний уход) ---
        public const string SelfCare = "topicHarvey_SelfCare";
        public const string CleanBandage = "topicHarvey_CleanBandage";
        public const string WarmTea = "topicHarvey_WarmTea";
        public const string SelfCarePraise = "topicHarvey_SelfCarePraise";

        // --- Proximity-реакции Харви ---
        public const string ProximityReaction = "topicHarvey_ProximityReaction";
        public const string ProximityStrict = "topicHarvey_ProximityStrict";
        public const string ProximityPraise = "topicHarvey_ProximityPraise";
        public const string MineInjuryRescue = "topicMineInjuryRescue";
        /// <summary>После eventHarveyMinorMineRescue — опасное состояние в шахте без Severe.</summary>
        public const string MinorMineRescue = "topicHarveyMinorMineRescue";
        /// <summary>Блокирует CP interception/warning, пока C# готовит cutscene спасения из шахты.</summary>
        public const string MineRescuePending = "topicMineRescuePending";
        /// <summary>Триггер CP-события HarveyMod_FirstTreatment (ставится C# при первой травме).</summary>
        public const string HarveyNeedsFirstTreatment = "topicHarveyNeedsFirstTreatment";
        /// <summary>Ставится событием HarveyMod_FirstTreatment после прохождения.</summary>
        public const string FirstTreatmentComplete = "topicFirstTreatmentComplete";
        /// <summary>Триггер CP-события HarveyMod_TreatmentPlanMeeting после начала серьёзного лечения.</summary>
        public const string DiagnosisComplete = "topicDiagnosisComplete";
    }

    /// <summary>
    /// Storm comfort: C# launcher → CP cutscenes.
    /// </summary>
    public static class StormComfortIds
    {
        public const string StressThunderBuff = "buffStressThunder";
        public const string StormStressTopic = "topicHarveyStormStress";
        public const string LegacyStressTopic = "topicStressThunder";
        public const string CooldownTopic = "HarveyMod_CD_StormComfort";
        public const string EventIdPrefix = "eventHarveyStormComfort";

        public const int MinFriendshipPoints = 750;
        public const int RollTimeStart = 1200;
        public const int RollTimeEnd = 2200;
        public const double DefaultRollChance = 0.35;
    }

    /// <summary>
    /// Единый источник правды для динамически генерируемых topic ID (см. id-naming-standard.md §7).
    /// </summary>
    public static class TopicIds
    {
        public static string GetInjuryTopic(string buffId) => buffId.Replace("buff", "topic");

        public static string GetTreatmentTopic(string buffId) => buffId.Replace("buff", "topicTreatment");

        public static string GetCuredTopic(string buffId) => $"topic{buffId.Replace("buff", "")}Cured";

        public static string GetComplicationTopic(string complicationBuffId) =>
            complicationBuffId.Replace("HarveyMod_", "topicHarvey_");

        public static string GetPhaseTopicId(string injuryId, int phase)
        {
            string injuryName = injuryId.Replace("buff", "");
            string stageName = phase switch
            {
                1 => "Acute",
                2 => "Healing",
                3 => "Recovery",
                _ => "Unknown"
            };
            return $"topic{injuryName}Phase{stageName}";
        }
    }

    /// <summary>
    /// Топики, принадлежащие InjuryCare — для безопасного RemoveTopic / debug-сброса без затрагивания чужих модов.
    /// </summary>
    public static class ModTopicRegistry
    {
        public static HashSet<string> GetAllOwnedTopicIds()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in typeof(ConversationTopics).GetFields(
                         System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.GetValue(null) is string topicId)
                    ids.Add(topicId);
            }

            foreach (string buffId in InjurySets.HarveyTreatable)
            {
                ids.Add(TopicIds.GetInjuryTopic(buffId));
                ids.Add(TopicIds.GetTreatmentTopic(buffId));
                ids.Add(TopicIds.GetCuredTopic(buffId));
                for (int phase = 1; phase <= 3; phase++)
                    ids.Add(TopicIds.GetPhaseTopicId(buffId, phase));
            }

            foreach (string compId in InjurySets.KnownComplicationBuffIds)
                ids.Add(TopicIds.GetComplicationTopic(compId));

            ids.Add(StormComfortIds.StormStressTopic);
            ids.Add(StormComfortIds.LegacyStressTopic);
            ids.Add(StormComfortIds.CooldownTopic);

            foreach (var field in typeof(PrescriptionTopics).GetFields(
                         System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.GetValue(null) is string topicId)
                    ids.Add(topicId);
            }

            foreach (var field in typeof(ComplianceTopics).GetFields(
                         System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.GetValue(null) is string topicId)
                    ids.Add(topicId);
            }

            ids.Add(CheckupTopics.CheckupDue);
            ids.Add(CheckupTopics.RecoveryCheckupDue);

            ids.Add(TreatmentPlanTopics.Given);
            foreach (string buffId in InjurySets.HarveyTreatable)
                ids.Add(TreatmentPlanTopics.GetInjuryTopic(buffId));

            return ids;
        }
    }

    /// <summary>ID событий Content Patcher / vanilla.</summary>
    public static class EventIds
    {
        public const string FirstTreatment = "HarveyMod_FirstTreatment";
        public const string TreatmentPlanMeeting = "HarveyMod_TreatmentPlanMeeting";
        public const string EmergencyCare = "eventHarveyEmergencyCare";
        public const string Exhaustion = "eventHarveyExhaustion";
        public const string MinorMineRescue = "eventHarveyMinorMineRescue";
        public const string MineRescueDating = "eventHarveyMineRescueDating";
        public const string MineRescue = "eventHarveyMineRescue";
        public const string RescueOperation = "eventRescueOperation";
    }

    /// <summary>
    /// eventRescueOperation: параллельная trauma-арка (не E1–E8), launcher ставит topicRescueOperation.
    /// </summary>
    public static class RescueOperationIds
    {
        public const string Topic = "topicRescueOperation";
        public const string CooldownTopic = "HarveyMod_CD_RescueOperation";
        public const string E5StormBesideEvent = "HarveyOverhaulStory.E5_StormBeside";
        public const int MinFriendshipPoints = 2000;
        public const int TopicDurationDays = 3;
        public const int CooldownDays = 14;
    }

    /// <summary>
    /// Константы для ID писем
    /// </summary>
    public static class MailIds
    {
        // --- Legacy (freeze): exact CP keys ---
        /// <summary>Рекомендации по сну после обморока/усталости (mailInjury.json).</summary>
        public const string SleepControl = "mailHarveySleepControl";
        /// <summary>Письмо о запрете шахты после предупреждения в шахте с серьёзными ранами.</summary>
        public const string MineForbidden = "mailHarveyMineForbidden";

        // --- HarveyMod_* (canonical для системных писем C#) ---
        public const string WetCare = "HarveyMod_WetCare";
        public const string WetStitchesCare = "HarveyMod_WetStitchesCare";
        public const string InfectionAlert = "HarveyMod_InfectionAlert";
        public const string NeglectWarning = "HarveyMod_NeglectWarning";
        public const string DirtyWoundInfection = "HarveyMod_DirtyWoundInfection";
        public const string WetBandageInfection = "HarveyMod_WetBandageInfection";
        public const string TreatmentUrgentReminder = "HarveyMod_TreatmentUrgentReminder";
        public const string TreatmentFinalWarning = "HarveyMod_TreatmentFinalWarning";
        /// <summary>Письмо при просрочке контрольного осмотра (4+ дня).</summary>
        public const string CheckupOverdue = "HarveyMod_CheckupOverdue";

        // --- План лечения (после начала лечения) ---
        public const string TreatmentPlanMinor = "mailHarveyTreatmentPlan_Minor";
        public const string TreatmentPlanSevere = "mailHarveyTreatmentPlan_Severe";
        public const string TreatmentPlanInfection = "mailHarveyTreatmentPlan_Infection";
        public const string TreatmentPlanConcussion = "mailHarveyTreatmentPlan_Concussion";
        public const string TreatmentPlanFracture = "mailHarveyTreatmentPlan_Fracture";
        public const string TreatmentPlanBurn = "mailHarveyTreatmentPlan_Burn";
        public const string TreatmentPlanCold = "mailHarveyTreatmentPlan_Cold";

        // --- Письма по тону отношений (суффикс _LowHearts | _MidHearts | _Dating | _Married) ---
        public const string PrescriptionViolation = "mailHarveyPrescriptionViolation";
        public const string CheckupReminder = "mailHarveyCheckupReminder";
        public const string RehabReminder = "mailHarveyRehabReminder";
        public const string RehabCompleted = "mailHarveyRehabCompleted";
        public const string NoMineViolation = "mailHarveyNoMineViolation";
        public const string KeepDryViolation = "mailHarveyKeepDryViolation";
        public const string RestViolation = "mailHarveyRestViolation";
    }

    /// <summary>
    /// Наборы травм для различных проверок
    /// </summary>
    public static class InjurySets
    {
        public static readonly HashSet<string> Severe = new()
        {
            "buffBadlyHurt",
            "buffShrapnelWounds",
            "buffFracturedBone",
            "buffConcussion",
            "buffSurgicalWound",
            "buffInfectedWound",
            "buffBurnWounds"
        };

        /// <summary>
        /// Ограничение активности: не Severe/Critical, без принудительной госпитализации;
        /// мягкое предупреждение в шахте и при перегрузке.
        /// </summary>
        public static readonly HashSet<string> LimitedActivity = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "buffTornMuscles",
            "buffBruisedRibs",
            "buffSprainedAnkle",
            "buffBackStrain",
        };

        public static readonly HashSet<string> DirtyInMines = new()
        {
            "buffDeepCuts",
            "buffBurnWounds",
            "buffShrapnelWounds"
        };

        public static readonly HashSet<string> PainFlareOnStorm = new()
        {
            "buffFracturedBone",
            "buffShrapnelWounds"
        };

        public static readonly HashSet<string> Simple = new()
        {
            "buffHurt",
            "buffBadlyHurt"
        };

        public static readonly HashSet<string> Critical = new()
        {
            "buffConcussion",
            "buffFracturedBone",
            "buffBadlyHurt",
            "buffInfectedWound"
        };

        /// <summary>Травмы, требующие лечения у Харви (не осложнения).</summary>
        public static readonly HashSet<string> HarveyTreatable = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "buffHurt",
            "buffBadlyHurt",
            "buffSprainedAnkle",
            "buffBruisedRibs",
            "buffBackStrain",
            "buffDeepCuts",
            "buffBurnWounds",
            "buffInfectedWound",
            "buffTornMuscles",
            "buffConcussion",
            "buffFracturedBone",
            "buffShrapnelWounds",
            "buffSurgicalWound",
            InjuryBuffs.Cold,
        };

        /// <summary>Осложнения InjuryCare (DebuffState + TreatComplications), не чужие баффы.</summary>
        public static readonly HashSet<string> KnownComplicationBuffIds = new(System.StringComparer.OrdinalIgnoreCase)
        {
            InjuryBuffs.WetBandage,
            InjuryBuffs.DirtyWound,
            InjuryBuffs.WetStitches,
            InjuryBuffs.AllergicRash,
            InjuryBuffs.PainFlare,
            InjuryBuffs.Neglect,
        };

        /// <summary>Порядок приоритета осложнений (выше в списке = важнее).</summary>
        public static readonly string[] ComplicationPriorityOrder =
        {
            InjuryBuffs.DirtyWound,
            InjuryBuffs.WetStitches,
            InjuryBuffs.WetBandage,
            InjuryBuffs.AllergicRash,
            InjuryBuffs.PainFlare,
            InjuryBuffs.Neglect,
        };

        /// <summary>Травмы, после начала лечения которых ставится topicDiagnosisComplete.</summary>
        public static readonly HashSet<string> TreatmentPlanEligible = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "buffConcussion",
            "buffFracturedBone",
            "buffDeepCuts",
            "buffBurnWounds",
            "buffInfectedWound",
            "buffShrapnelWounds",
            "buffTornMuscles",
            "buffSurgicalWound",
        };
    }

    /// <summary>
    /// Константы для триггеров травм (для системы AppliedTriggers)
    /// </summary>
    public static class Triggers
    {
        // Лёгкие травмы
        public const string Hurt = "{{ModId}}_triggerHurt";
        public const string BadlyHurt = "{{ModId}}_triggerBadlyHurt";

        // Средние травмы
        public const string SprainedAnkle = "{{ModId}}_triggerSprainedAnkle";
        public const string BruisedRibs = "{{ModId}}_triggerBruisedRibs";
        public const string BackStrain = "{{ModId}}_triggerBackStrain";
        public const string DeepCutsCombat = "{{ModId}}_triggerDeepCutsCombat";
        public const string DeepCutsFarming = "{{ModId}}_triggerDeepCutsFarming";
        public const string BurnWounds = "{{ModId}}_triggerBurnWounds";
        public const string InfectedWound = "{{ModId}}_triggerInfectedWound";

        // Тяжёлые травмы
        public const string TornMuscles = "{{ModId}}_triggerTornMuscles";
        public const string Concussion = "{{ModId}}_triggerConcussion";
        public const string FracturedBone = "{{ModId}}_triggerFracturedBone";
        public const string ShrapnelWounds = "{{ModId}}_triggerShrapnelWounds";

        // Специальные
        public const string SurgicalWound = "{{ModId}}_triggerSurgicalWound";
        public const string ExplosionInjury = "{{ModId}}_triggerExplosionInjury";
        public const string Cold = "{{ModId}}_triggerCold";
    }

    /// <summary>
    /// Политика one-shot story-триггеров vs повторяемых травм.
    /// </summary>
    public static class InjuryTriggerPolicy
    {
        public static readonly HashSet<string> StoryOneShotTriggers = new()
        {
            Triggers.SurgicalWound,
            Triggers.ExplosionInjury,
        };

        public static readonly HashSet<string> RepeatableInjuryBuffIds = new()
        {
            "buffHurt",
            "buffBadlyHurt",
            "buffSprainedAnkle",
            "buffBruisedRibs",
            "buffBackStrain",
            "buffDeepCuts",
            "buffBurnWounds",
            "buffInfectedWound",
            "buffTornMuscles",
            "buffConcussion",
            "buffFracturedBone",
            "buffShrapnelWounds",
            InjuryBuffs.Cold,
        };

        public static bool IsStoryOneShotTrigger(string triggerConst)
            => StoryOneShotTriggers.Contains(triggerConst);

        public static bool IsRepeatableInjuryBuff(string buffId)
            => RepeatableInjuryBuffIds.Contains(buffId);

        /// <summary>
        /// Маппинг legacy trigger id (в т.ч. с {{ModId}}) в buffId для миграции cooldown-словаря.
        /// </summary>
        public static string? MapTriggerKeyToBuffId(string triggerKey)
        {
            const string marker = "_trigger";
            int idx = triggerKey.LastIndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
                return null;

            return triggerKey[(idx + marker.Length)..] switch
            {
                "Hurt" => "buffHurt",
                "BadlyHurt" => "buffBadlyHurt",
                "SprainedAnkle" => "buffSprainedAnkle",
                "BruisedRibs" => "buffBruisedRibs",
                "BackStrain" => "buffBackStrain",
                "DeepCutsCombat" or "DeepCutsFarming" => "buffDeepCuts",
                "BurnWounds" => "buffBurnWounds",
                "InfectedWound" => "buffInfectedWound",
                "TornMuscles" => "buffTornMuscles",
                "Concussion" => "buffConcussion",
                "FracturedBone" => "buffFracturedBone",
                "ShrapnelWounds" => "buffShrapnelWounds",
                "Cold" => InjuryBuffs.Cold,
                _ => null,
            };
        }
    }
}

