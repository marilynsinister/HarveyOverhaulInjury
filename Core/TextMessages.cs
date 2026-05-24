namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>
    /// Текстовые сообщения над головой Харви (короткие реплики)
    /// </summary>
    public static class HarveyTextMessages
    {
        // Обнаружение травм
        public static readonly string FindInjury = "Я уже здесь. Сейчас всё осмотрю — слушай мои указания.";
        public static readonly string SeriousInjury = "Ситуация серьёзная. Не спорь, я всё возьму под контроль.";
        public static readonly string CriticalInjury = "Это критично. Я займусь этим лично — делай, что я говорю.";
        public static readonly string MultipleInjuries = "Много травм? Теперь слушай меня внимательно и не мешай лечить.";

        // Начало лечения
        public static readonly string StartingTreatment = "Начинаю лечение. Расслабься и слушай мои советы.";
        public static readonly string Examining = "Осматриваю тебя. Не отвлекайся, мне важно всё заметить.";
        public static readonly string Processing = "Обрабатываю рану. Потерпи, это необходимо.";
        public static readonly string ApplyingBandage = "Накладываю повязку. Не двигайся.";
        public static readonly string GivingMedicine = "Вот лекарства. Принимай строго по инструкции.";

        // Осложнения
        public static readonly string FoundComplication = "Есть осложнения. Дальше только по моим правилам.";
        public static readonly string DirtyWound = "Рану надо очистить. Не спорь, я знаю, что делаю.";
        public static readonly string WetBandage = "Повязка промокла — меняю. В следующий раз будь внимательнее.";
        public static readonly string WetStitches = "Швы намокли. Исправляю, но впредь слушай мои советы.";
        public static readonly string Infection = "Есть инфекция. Теперь ты под моим контролем.";
        public static readonly string AllergicReaction = "Аллергия? Я разберусь, но впредь сообщай о реакциях сразу.";

        // Госпитализация
        public static readonly string NeedHospitalization = "В больницу — и точка. Это не обсуждается.";
        public static readonly string StayInBed = "Оставайся в постели. Без моего разрешения не вставать.";
        public static readonly string DontMove = "Не двигайся, пока я не разрешу.";
        public static readonly string RestRequired = "Тебе нужен покой. Я прослежу, чтобы никто не мешал.";

        // Пренебрежение лечением
        public static readonly string NotTreating = "Опять игнорируешь лечение? Я не позволю тебе рисковать.";
        public static readonly string Worried = "Я за тебя отвечаю. Позволь мне позаботиться о тебе.";
        public static readonly string Disappointed = "Я ожидал большего. Будь ответственнее — слушайся меня.";
        public static readonly string DangerousIgnoring = "Игнорировать лечение опасно. Я не дам тебе навредить себе.";

        // Выздоровление
        public static readonly string GoodProgress = "Прогресс хороший, но не забывай мои советы.";
        public static readonly string AlmostHealed = "Почти восстановилась, но не спеши — я слежу.";
        public static readonly string FullRecovery = "Выздоровела! Но если что — сразу ко мне.";
        public static readonly string Congratulations = "Молодец! Но не забывай заботиться о себе.";

        // Поддержка и забота
        public static readonly string BeCareful = "Будь осторожнее. Я всегда помогу, но лучше не рисковать.";
        public static readonly string TakeCare = "Береги себя. Даже если кажется, что всё в порядке.";
        public static readonly string ImHere = "Я рядом, если понадобится помощь — обращайся.";
        public static readonly string DontWorry = "Не переживай, я всё контролирую.";
        public static readonly string EveryThingWillBeOk = "Всё будет хорошо, если будешь слушаться меня.";

        // Срочные ситуации
        public static readonly string Emergency = "Срочно действую. Доверься мне — я справлюсь.";
        public static readonly string Dangerous = "Это опасно. Не пытайся сама, я разберусь.";
        public static readonly string CallAmbulance = "Вызываю скорую. Всё под контролем.";

        // Инструкции
        public static readonly string SitDown = "Сядь. Так безопаснее — слушай мои инструкции.";
        public static readonly string LieDown = "Ложись. Это важно для твоего состояния.";
        public static readonly string DontMoveArm = "Рукой не двигать, пока не разрешу.";
        public static readonly string DontMakeLeg = "Не наступай на ногу. Я помогу, если нужно.";
        public static readonly string Breathe = "Дыши ровно. Я рядом.";

        // Проверки
        public static readonly string CheckingPulse = "Проверяю пульс. Всё под контролем.";
        public static readonly string CheckingTemperature = "Измеряю температуру. Следи за самочувствием.";
        public static readonly string CheckingPressure = "Проверяю давление. Если что — сразу приму меры.";
        public static readonly string Examining_Detail = "Осматриваю внимательно. Всё замечу — не волнуйся.";

        // Позитивные
        public static readonly string ProudOfYou = "Горжусь тобой! Но помни — я всегда рядом.";
        public static readonly string GoodJob = "Отлично! Но не забывай мои рекомендации.";
        public static readonly string YoureStrong = "Ты сильная, но даже сильным нужна забота.";
        public static readonly string Brave = "Ты смелая, но врачу доверять нужно.";
    }

    /// <summary>
    /// Вспомогательный класс для выбора текстовых сообщений
    /// </summary>
    public static class TextMessageSelector
    {
        /// <summary>
        /// Получить сообщение для обнаруженной травмы
        /// </summary>
        public static string ForInjuryDiscovery(bool isCritical, bool isSerious)
        {
            if (isCritical)
                return HarveyTextMessages.CriticalInjury;
            if (isSerious)
                return HarveyTextMessages.SeriousInjury;
            return HarveyTextMessages.FindInjury;
        }

        /// <summary>
        /// Получить сообщение для начала лечения
        /// </summary>
        public static string ForTreatmentStart(bool hasComplications)
        {
            if (hasComplications)
                return HarveyTextMessages.Examining;
            return HarveyTextMessages.StartingTreatment;
        }

        /// <summary>
        /// Получить сообщение для выздоровления
        /// </summary>
        public static string ForRecovery(bool fullRecovery)
        {
            return fullRecovery 
                ? HarveyTextMessages.FullRecovery 
                : HarveyTextMessages.GoodProgress;
        }

        /// <summary>
        /// Получить случайное сообщение поддержки
        /// </summary>
        public static string ForSupport()
        {
            var random = StardewValley.Game1.random.Next(4);
            return random switch
            {
                0 => HarveyTextMessages.DontWorry,
                1 => HarveyTextMessages.ImHere,
                2 => HarveyTextMessages.TakeCare,
                _ => HarveyTextMessages.EveryThingWillBeOk
            };
        }
    }
}

