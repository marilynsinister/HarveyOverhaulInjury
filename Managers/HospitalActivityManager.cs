using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление интерактивными активностями во время госпитализации
    /// </summary>
    public class HospitalActivityManager
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly DialogueManager _dialogueManager;
        
        private int _lastActivityTime = -1;
        private int _activityCounter = 0;
        private readonly List<string> _availableActivities = new();

        public HospitalActivityManager(IMonitor monitor, ModConfig config, DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _dialogueManager = dialogueManager;
            InitializeActivities();
        }

        /// <summary>
        /// Инициализировать доступные активности
        /// </summary>
        private void InitializeActivities()
        {
            _availableActivities.AddRange(new[]
            {
                "checkVitals",      // Харви проверяет показатели
                "bringWater",       // Харви приносит воду
                "adjustPillow",     // Харви поправляет подушку
                "readChart",        // Харви изучает карту
                "conversation",     // Лёгкая беседа
                "holdHand",         // Харви держит за руку
                "checkBandage",     // Харви проверяет повязку
                "bringMedicine",    // Харви даёт лекарство
                "comfort",          // Харви успокаивает
                "checkTemperature"  // Харви измеряет температуру
            });
        }

        /// <summary>
        /// Обновить активности во время госпитализации
        /// </summary>
        public void UpdateHospitalActivities(HospitalizationManager hospitalization)
        {
            if (!hospitalization.IsHospitalized) return;
            if (hospitalization.HasPendingReturnToHospital) return;
            if (Game1.eventUp || Game1.CurrentEvent != null || Game1.activeClickableMenu != null) return;
            if (!Context.IsPlayerFree) return;
            if (_activityCounter >= _config.MaxHospitalActivitiesPerStay) return;

            int intervalMinutes = Math.Max(1, _config.HospitalActivityIntervalMinutes);
            int currentTime = Game1.timeOfDay;

            if (_lastActivityTime == -1 || GetElapsedMinutesSince(currentTime, _lastActivityTime) >= intervalMinutes)
            {
                _lastActivityTime = currentTime;
                TriggerRandomActivity();
            }
        }

        /// <summary>
        /// Запустить случайную активность
        /// </summary>
        private void TriggerRandomActivity()
        {
            if (_availableActivities.Count == 0) return;

            NPC? harvey = HarveyHelper.GetHarvey();
            if (harvey == null) return;

            // Выбираем случайную активность
            string activity = _availableActivities[Game1.random.Next(_availableActivities.Count)];
            _activityCounter++;

            _monitor.Log($"🏥 Активность #{_activityCounter}: {activity}", LogLevel.Debug);

            switch (activity)
            {
                case "checkVitals":
                    ShowActivity(harvey,
                        "*прикладывает стетоскоп* Сердцебиение нормализовалось. Хороший знак.$h");
                    break;

                case "bringWater":
                    ShowActivity(harvey,
                        "*протягивает воду* Пей медленно. Тебе нужно восстановить водный баланс.$l");
                    Game1.player.Stamina = Math.Min(Game1.player.MaxStamina, Game1.player.Stamina + 15f);
                    break;

                case "adjustPillow":
                    ShowActivity(harvey,
                        "*заботливо* Так удобнее? Тебе нужно лежать спокойно.$l");
                    break;

                case "readChart":
                    ShowActivity(harvey,
                        "*задумчиво* Показатели улучшаются... Ты молодец.$h");
                    break;

                case "conversation":
                    ShowConversation(harvey);
                    break;

                case "holdHand":
                    if (_dialogueManager.IsDatingOrMarriedToHarvey())
                    {
                        ShowActivity(harvey,
                            "*тихо* Я здесь. Ты не одна.$l#$b#Я не отойду, пока ты не поправишься.$l");
                        Game1.player.health = Math.Min(Game1.player.maxHealth, Game1.player.health + 5);
                    }
                    else
                    {
                        ShowActivity(harvey,
                            "Как ты себя чувствуешь? Боль стихла?$s");
                    }
                    break;

                case "checkBandage":
                    ShowActivity(harvey,
                        "*осторожно* Заживает хорошо. Без признаков инфекции.$h");
                    break;

                case "bringMedicine":
                    ShowActivity(harvey,
                        "Это поможет снять боль. *протягивает таблетку*$u");
                    Game1.player.health = Math.Min(Game1.player.maxHealth, Game1.player.health + 10);
                    break;

                case "comfort":
                    ShowActivity(harvey,
                        "*мягко* Не волнуйся. Худшее позади.$l#$b#Ты в надёжных руках.$h");
                    break;

                case "checkTemperature":
                    ShowActivity(harvey,
                        "*проверяет термометр* 36.8. Отлично, никакой лихорадки.$h");
                    break;
            }
        }

        /// <summary>
        /// Показать активность с диалогом
        /// </summary>
        private void ShowActivity(NPC harvey, string dialogue)
        {
            _dialogueManager.Speak(harvey, dialogue);
            Game1.playSound("healSound");
        }

        /// <summary>
        /// Показать беседу с Харви
        /// </summary>
        private void ShowConversation(NPC harvey)
        {
            var conversations = new[]
            {
                "Знаешь, я очень волновался...$s#$b#Когда увидел тебя в таком состоянии...$s",
                "Ты должна быть осторожнее.$u#$b#Я не хочу снова видеть тебя в больничной койке.$s",
                "*улыбается* Хорошие новости - скоро ты поправишься.$h#$b#Но нужно ещё немного терпения.$l",
                "Мару спрашивала о тебе.$h#$b#Все переживают. Ты важна для долины.$l",
                "После выписки я дам тебе витамины.$u#$b#И строгие инструкции по восстановлению.$a"
            };

            string dialogue = conversations[Game1.random.Next(conversations.Length)];
            ShowActivity(harvey, dialogue);
        }

        /// <summary>
        /// Сбросить счётчики при выписке
        /// </summary>
        public void Reset()
        {
            _lastActivityTime = -1;
            _activityCounter = 0;
            _monitor.Log("🏥 Сброс активностей госпитализации", LogLevel.Debug);
        }

        private static int ToClockMinutes(int timeOfDay)
        {
            int hours = timeOfDay / 100;
            int minutes = timeOfDay % 100;
            return hours * 60 + minutes;
        }

        private static int GetElapsedMinutesSince(int currentTimeOfDay, int previousTimeOfDay)
        {
            int elapsed = ToClockMinutes(currentTimeOfDay) - ToClockMinutes(previousTimeOfDay);
            if (elapsed < 0)
                elapsed += 24 * 60;
            return elapsed;
        }
    }
}

