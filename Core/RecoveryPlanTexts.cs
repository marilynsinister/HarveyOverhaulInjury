namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>ID задач плана восстановления (save-state, не баффы).</summary>
    public static class RecoveryPlanTaskIds
    {
        public const string SleepBeforeMidnight = "SleepBeforeMidnight";
        public const string AvoidMines = "AvoidMines";
        public const string KeepStaminaAbove15 = "KeepStaminaAbove15";
        public const string ReturnIfLowHealth = "ReturnIfLowHealth";
        public const string VisitHarveyIfReady = "VisitHarveyIfReady";
        public const string KeepBandageDry = "KeepBandageDry";
        public const string TreatComplications = "TreatComplications";
    }

    /// <summary>ID нарушений режима восстановления.</summary>
    public static class RecoveryPlanViolationIds
    {
        public const string EnteredMinesDuringRecovery = "EnteredMinesDuringRecovery";
        public const string LowStaminaDuringRecovery = "LowStaminaDuringRecovery";
        public const string LowHealthDuringRecovery = "LowHealthDuringRecovery";
        public const string LateSleepDuringRecovery = "LateSleepDuringRecovery";
        public const string RainDuringRecovery = "RainDuringRecovery";
        public const string MissedCheckup = "MissedCheckup";
    }

    /// <summary>Тексты UI, HUD и тона Харви для плана восстановления.</summary>
    public static class RecoveryPlanTexts
    {
        public static class Status
        {
            public const string Calm = "Спокойно. План соблюдается.";
            public const string CalmLong = "Всё спокойно. Ты соблюдаешь режим, и Харви будет доволен.";
            public const string HarveyConcerned = "Харви волнуется. Лучше сбавить темп.";
            public const string HarveyConcernedLong = "Харви волнуется. Организм уже на пределе — лучше сбавить темп.";
            public const string NeedsHarveyTalk = "Нужен разговор с Харви.";
            public const string NeedsHarveyTalkLong = "Нужен разговор с Харви. Он должен проверить, как идёт восстановление.";
            public const string Urgent = "Состояние тревожное. Вернись домой или в клинику.";
            public const string UrgentLong = "Состояние тревожное. Лучше вернуться домой или в клинику.";
        }

        public static class Tasks
        {
            public const string SleepTitle = "Лечь до 00:00";
            public const string SleepDescription = "По плану восстановления нужно лечь до полуночи.";
            public const string MinesTitle = "Не ходить в шахту и вулкан";
            public const string MinesDescription = "Не ходить в шахту и вулкан до разрешения Харви.";
            public const string MinesTitleRecommended = "Шахта не рекомендована";
            public const string MinesDescriptionRecommended = "Харви не рекомендует шахту и вулкан — риск осложнений.";
            public const string MinesTitleForbidden = "Шахта запрещена Харви";
            public const string MinesDescriptionForbidden = "Харви запретил шахту и вулкан ещё на {0}.";
            public const string StaminaTitle = "Не опускать выносливость ниже 15%";
            public const string StaminaDescription = "Не опускать stamina ниже 15% от максимума.";
            public const string HealthTitle = "Вернуться домой при низком HP";
            public const string HealthDescription = "Если здоровье ниже 35%, вернуться домой или в клинику.";
            public const string VisitTitle = "Поговорить с Харви";
            public const string VisitPhaseDescription = "Поговорить с Харви для следующего этапа лечения.";
            public const string VisitRecoveryDescription = "Финальный осмотр у Харви.";
            public const string VisitStartDescription = "Начать лечение — поговорить с Харви.";
            public const string BandageTitle = "Держать повязку сухой";
            public const string BandageDescription = "Держать повязку сухой — избегать дождя и бассейна.";
            public const string ComplicationTitle = "Показать осложнение Харви";
            public const string ComplicationDescription = "Поговорить с Харви об осложнении.";
        }

        public static class Hud
        {
            public const string PlanUpdated = "План восстановления обновлён. Открой окно лечения.";
            public const string PlanCreated = "Харви составил план восстановления. Открой окно лечения, чтобы посмотреть режим.";
            public const string PlanCompleted = "План восстановления завершён.";
            public const string MinesViolation = "План восстановления нарушен: шахта/вулкан запрещены. Подробности — в окне лечения.";
            public const string MinesSoftWarning = "Харви: Шахта сейчас не рекомендована. Режим восстановления зафиксирован.";
            public const string LateNightReminder = "Харви: Уже поздно. По плану восстановления тебе нужно лечь до полуночи.";
            public const string NoActivePlan = "Сейчас активного плана восстановления нет. Если почувствуешь себя плохо — зайди к Харви.";
            public const string MildViolation =
                "Харви: Сегодня ты переутомляешься. Остановись — день не засчитается, но режим я пока не удлиняю.";
            public const string PlanExtended =
                "Харви продлил режим на один день. Контрольный осмотр обязателен.";
            public const string MaxExtensionsReached =
                "Харви больше не продлевает режим, но требует контрольный визит.";
        }

        public static class Harvey
        {
            public const string Mines = "Ты серьёзно пошла туда в таком состоянии? Нет. Сегодня без шахты. Я не спорю о вещах, которые могут стоить тебе здоровья.";
            public const string LateNight = "Я ждал, вернёшься ли ты. По плану ты должна была спать уже час назад.";
            public const string Stamina = "Руки дрожат. Инструменты отложи. Хотя бы на сегодня — послушай меня.";
            public const string Health = "Домой. Или в клинику. Сейчас не время быть храброй.";
            public const string Checkup = "Я не злюсь. Но мне больно видеть, что ты тянешь до последнего. Дай мне проверить тебя.";
            public const string MaxExtensionsFallback =
                "Я мог бы продлить режим ещё раз. И ещё. И ещё.$s#$b#"
                + "Но это уже перестанет быть лечением и станет клеткой. Я не хочу так.$u#$b#"
                + "Поэтому так: режим заканчивается по сроку, но контрольный осмотр обязателен. "
                + "И на этот раз я буду очень внимателен.$a";
        }

        public static class HarveyTone
        {
            public const string SectionLabel = "Сейчас";
            public const string CalmTitle = "Харви спокоен";
            public const string CalmDescription = "Ты соблюдаешь режим, он доверяет тебе.";
            public const string WorriedTitle = "Харви тревожится";
            public const string WorriedDescription = "Сегодня было предупреждение.";
            public const string StrictTitle = "Харви строг";
            public const string StrictDescription = "Сегодня режим был нарушен.";
            public const string NoActivePlan = "Сейчас нет строгого режима. Просто берегите себя.";
        }

        public static class RegimeStatus
        {
            public const string Calm = "режим соблюдается";
            public const string Concerned = "были тревожные моменты";
            public const string NeedsHarveyTalk = "нужен осмотр";
            public const string Urgent = "состояние тревожное";
        }

        public static class Completion
        {
            public const string Perfect =
                "Идеальное соблюдение. Харви хотел бы меньше волноваться. Получается не очень, но он гордится тобой.";

            public const string WithWarnings =
                "План почти выполнен. Были тревожные моменты, но день ещё можно спасти.";

            public const string Normal =
                "День не засчитан. Организму нужен отдых, а не подвиги через силу.";
        }

        public static class WhyImportant
        {
            public const string Default = "Сейчас организм восстанавливается. Шахта, усталость и игнорирование режима повышают риск осложнений.";
            public const string Wound = "Сейчас рана легко воспаляется. Шахта, дождь и усталость повышают риск осложнений.";
            public const string Severe = "Тяжёлая травма требует строгого режима. Харви следит, чтобы ты не перегружала себя.";
            public const string Complication = "Есть осложнение — его нельзя игнорировать. Харви должен осмотреть тебя.";
        }

        public static class Complications
        {
            public const string WetBandage = "Есть осложнение: мокрая повязка";
            public const string DirtyWound = "Есть осложнение: грязная рана";
            public const string WetStitches = "Есть осложнение: мокрые швы";
            public const string Neglect = "Есть осложнение: запущенное лечение";
            public const string PainFlare = "Есть осложнение: обострение боли";
            public const string AllergicRash = "Есть осложнение: аллергическая сыпь";
        }

        public static string GetComplicationLine(string compId) => compId switch
        {
            InjuryBuffs.WetBandage => Complications.WetBandage,
            InjuryBuffs.DirtyWound => Complications.DirtyWound,
            InjuryBuffs.WetStitches => Complications.WetStitches,
            InjuryBuffs.Neglect => Complications.Neglect,
            InjuryBuffs.PainFlare => Complications.PainFlare,
            InjuryBuffs.AllergicRash => Complications.AllergicRash,
            _ => $"Есть осложнение: {compId}",
        };

        public static string GetWhyImportant(string? injuryId, bool hasComplication, bool isSevere)
        {
            if (hasComplication)
                return WhyImportant.Complication;
            if (isSevere || (injuryId != null && InjurySets.Severe.Contains(injuryId)))
                return WhyImportant.Severe;
            if (injuryId != null && InjurySets.BandageSensitive.Contains(injuryId))
                return WhyImportant.Wound;
            return WhyImportant.Default;
        }
    }
}
