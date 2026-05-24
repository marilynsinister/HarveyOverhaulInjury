using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
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
        private readonly DialogueManager _dialogueManager;
        
        private int _lastActivityTime = -1;
        private int _activityCounter = 0;
        private readonly List<string> _availableActivities = new();

        public HospitalActivityManager(IMonitor monitor, DialogueManager dialogueManager)
        {
            _monitor = monitor;
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
            if (!Context.IsPlayerFree) return;

            int currentTime = Game1.timeOfDay;
            
            // Активность каждые 20 минут игрового времени
            if (_lastActivityTime == -1 || currentTime - _lastActivityTime >= 20)
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

            NPC? harvey = Game1.getCharacterFromName("Harvey");
            if (harvey == null) return;

            // Выбираем случайную активность
            string activity = _availableActivities[Game1.random.Next(_availableActivities.Count)];
            _activityCounter++;

            _monitor.Log($"🏥 Активность #{_activityCounter}: {activity}", LogLevel.Debug);

            switch (activity)
            {
                case "checkVitals":
                    ShowActivity(harvey, "Харви проверяет твой пульс...",
                        "*прикладывает стетоскоп* Сердцебиение нормализовалось. Хороший знак.$h");
                    break;

                case "bringWater":
                    ShowActivity(harvey, "Харви приносит стакан воды...",
                        "*протягивает воду* Пей медленно. Тебе нужно восстановить водный баланс.$l");
                    Game1.player.Stamina = Math.Min(Game1.player.MaxStamina, Game1.player.Stamina + 15f);
                    break;

                case "adjustPillow":
                    ShowActivity(harvey, "Харви поправляет подушку...",
                        "*заботливо* Так удобнее? Тебе нужно лежать спокойно.$l");
                    break;

                case "readChart":
                    ShowActivity(harvey, "Харви изучает медицинскую карту...",
                        "*задумчиво* Показатели улучшаются... Ты молодец.$h");
                    break;

                case "conversation":
                    ShowConversation(harvey);
                    break;

                case "holdHand":
                    if (_dialogueManager.IsDatingOrMarriedToHarvey())
                    {
                        ShowActivity(harvey, "Харви берёт твою руку в свои...",
                            "*тихо* Я здесь. Ты не одна.$l#$b#Я не отойду, пока ты не поправишься.$l");
                        Game1.player.health = Math.Min(Game1.player.maxHealth, Game1.player.health + 5);
                    }
                    else
                    {
                        ShowActivity(harvey, "Харви проверяет твоё самочувствие...",
                            "Как ты себя чувствуешь? Боль стихла?$s");
                    }
                    break;

                case "checkBandage":
                    ShowActivity(harvey, "Харви осматривает повязку...",
                        "*осторожно* Заживает хорошо. Без признаков инфекции.$h");
                    break;

                case "bringMedicine":
                    ShowActivity(harvey, "Харви даёт обезболивающее...",
                        "Это поможет снять боль. *протягивает таблетку*$u");
                    Game1.player.health = Math.Min(Game1.player.maxHealth, Game1.player.health + 10);
                    break;

                case "comfort":
                    ShowActivity(harvey, "Харви сидит рядом с кроватью...",
                        "*мягко* Не волнуйся. Худшее позади.$l#$b#Ты в надёжных руках.$h");
                    break;

                case "checkTemperature":
                    ShowActivity(harvey, "Харви измеряет температуру...",
                        "*проверяет термометр* 36.8. Отлично, никакой лихорадки.$h");
                    break;
            }
        }

        /// <summary>
        /// Показать активность с диалогом
        /// </summary>
        private void ShowActivity(NPC harvey, string message, string dialogue)
        {
            // Показываем диалог Харви с сообщением
            if (harvey != null)
            {
                harvey.CurrentDialogue.Clear();
                harvey.CurrentDialogue.Push(new Dialogue(harvey, null, dialogue));
                Game1.drawDialogue(harvey);
            }
            else
            {
                // Если Харви нет, показываем просто сообщение
                Game1.drawObjectDialogue(message);
            }

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
            ShowActivity(harvey, "Харви садится рядом...", dialogue);
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
    }
}

