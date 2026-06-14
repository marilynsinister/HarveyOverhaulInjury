namespace HarveyOverhaul.InjuryCare.Api
{
    /// <summary>UI-тексты вкладки «Травмы» для окна «План Харви». Редактировать здесь.</summary>
    internal static class InjuryPanelTexts
    {
        public static string PhaseLine(string phaseName) => $"Фаза: {phaseName.ToLowerInvariant()}";

        public static class Status
        {
            public const string ReadyForRecovery = "Нужен финальный осмотр";
            public const string ReadyForRecoveryAdvice =
                "Харви должен убедиться, что всё зажило правильно.";

            public const string ReadyForNextPhase = "Пора на контрольный осмотр";
            public const string ReadyForNextPhaseAdvice =
                "Поговорите с Харви, чтобы перейти к следующему этапу лечения.";

            public const string AwaitingTreatment = "Нужен осмотр";
            public const string AwaitingTreatmentAdvice =
                "Поговорите с Харви, чтобы начать лечение.";

            public const string Ongoing = "Следуйте назначению Харви";
        }

        public static class Advice
        {
            public const string BandageAndMines = "Следите за повязкой и не рискуйте в шахте.";
            public const string WarmAndRest = "Согрейтесь и не перегружайте себя на улице.";
            public const string GeneralCare = "Соблюдайте режим и не пропускайте осмотры у Харви.";
            public const string MinesForbidden = "Шахта сейчас опасна. Харви не запрещает приключения навсегда — он запрещает осложнения.";
        }

        public static class Complications
        {
            public const string WetBandageTitle = "Повязка промокла";
            public const string WetBandageAdvice =
                "Нужно заменить её у Харви. Так инфекция не получит шанса.";

            public const string DirtyWoundTitle = "Рана загрязнилась";
            public const string DirtyWoundAdvice =
                "Это не страшилка. Это правда риск. Покажите её Харви.";

            public const string WetStitchesTitle = "Мокрые швы";
            public const string WetStitchesAdvice =
                "Швы намокли — срочно обратитесь к Харви.";

            public const string InfectionTitle = "Есть признаки инфекции";
            public const string InfectionAdvice =
                "Харви должен осмотреть рану как можно скорее.";

            public const string AllergicRashTitle = "Аллергическая сыпь";
            public const string AllergicRashAdvice =
                "Сообщите Харви о реакции — возможно, нужно скорректировать лечение.";

            public const string PainFlareTitle = "Обострение боли";
            public const string PainFlareAdvice =
                "Поговорите с Харви — возможно, нужна коррекция режима.";

            public const string NeglectTitle = "Пропущенный осмотр";
            public const string NeglectAdvice =
                "Лечение запущено — нужен осмотр у Харви.";

            public const string GenericTitle = "Осложнение";
            public const string GenericAdvice = "Покажите это Харви на осмотре.";
        }
    }
}
