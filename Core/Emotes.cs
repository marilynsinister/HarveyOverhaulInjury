namespace HarveyOverhaul.InjuryCare.Core
{
    /// <summary>
    /// Константы эмоций (облачка над головой NPC)
    /// </summary>
    public static class Emotes
    {
        /// <summary>Пустое облачко</summary>
        public const int Empty = 0;
        
        /// <summary>Вопросительный знак (?)</summary>
        public const int Question = 8;
        
        /// <summary>Злость (!!! красный)</summary>
        public const int Anger = 12;
        
        /// <summary>Восклицательный знак (!)</summary>
        public const int Exclamation = 16;
        
        /// <summary>Сердечко (♥)</summary>
        public const int Heart = 20;
        
        /// <summary>Сон (zzz)</summary>
        public const int Sleep = 24;
        
        /// <summary>Грусть (слеза)</summary>
        public const int Sad = 28;
        
        /// <summary>Радость (улыбка)</summary>
        public const int Happy = 32;
        
        /// <summary>Отказ (X)</summary>
        public const int Reject = 36;
        
        /// <summary>Пауза (...)</summary>
        public const int Pause = 40;
        
        /// <summary>Видеоигра</summary>
        public const int Videogame = 52;
        
        /// <summary>Музыка (нота)</summary>
        public const int Music = 56;
        
        /// <summary>Покраснение (смущение)</summary>
        public const int Blush = 60;
    }

    /// <summary>
    /// Эмоции Харви для различных ситуаций
    /// </summary>
    public static class HarveyEmotes
    {
        // Обнаружение травм
        public const int FindInjury = Emotes.Exclamation;          // ! - Обнаружил травму
        public const int SeriousInjury = Emotes.Anger;             // !!! - Серьёзная травма!
        public const int CriticalInjury = Emotes.Anger;            // !!! - Критическое состояние!
        
        // Лечение
        public const int StartTreatment = Emotes.Happy;            // Улыбка - Начинаем лечение
        public const int TreatmentSuccess = Emotes.Happy;          // Улыбка - Лечение успешно
        public const int Caring = Emotes.Heart;                    // ♥ - Забота о пациенте
        
        // Осложнения
        public const int FoundComplication = Emotes.Question;      // ? - Обнаружил осложнение
        public const int WorriedAboutPatient = Emotes.Sad;         // Слеза - Беспокойство
        public const int DirtyWound = Emotes.Exclamation;          // ! - Рана загрязнена
        
        // Госпитализация
        public const int ForcedHospitalization = Emotes.Anger;     // !!! - Немедленно в больницу!
        public const int StayInBed = Emotes.Reject;                // X - Назад в постель!
        
        // Пренебрежение лечением
        public const int NeglectedCare = Emotes.Sad;               // Слеза - Не лечишься?
        public const int Disappointed = Emotes.Sad;                // Слеза - Разочарован
        
        // Выздоровление
        public const int Recovery = Emotes.Happy;                  // Улыбка - Выздоравливаешь!
        public const int FullRecovery = Emotes.Heart;              // ♥ - Полное выздоровление!
        
        // Взаимодействие
        public const int Greeting = Emotes.Happy;                  // Улыбка - Приветствие
        public const int Thinking = Emotes.Pause;                  // ... - Размышляет
        public const int Confused = Emotes.Question;               // ? - Непонятная ситуация
    }
}

