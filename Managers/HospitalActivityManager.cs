using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
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

        private int _lastActivityAtProgressMinutes = -1;
        private int _activityCounter = 0;

        // Разовые за одно пребывание: разговор, чтение, визит друга.
        private bool _talkedThisStay;
        private bool _readThisStay;
        private bool _visitorThisStay;
        private const int VisitorAfterProgressMinutes = 30;
        private readonly System.Collections.Generic.List<string> _availableActivities = new();

        public HospitalActivityManager(IMonitor monitor, ModConfig config, DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _dialogueManager = dialogueManager;
            InitializeActivities();
        }

        private void InitializeActivities()
        {
            _availableActivities.AddRange(new[]
            {
                "checkVitals",
                "bringWater",
                "adjustPillow",
                "readChart",
                "conversation",
                "holdHand",
                "checkBandage",
                "bringMedicine",
                "comfort",
                "checkTemperature"
            });
        }

        /// <summary>
        /// Обновить активности во время госпитализации (по накопленным минутам прогресса).
        /// </summary>
        public void UpdateHospitalActivities(HospitalizationManager hospitalization, int newTimeOfDay)
        {
            if (!hospitalization.IsHospitalized) return;
            if (hospitalization.HasPendingReturnToHospital) return;
            if (Game1.eventUp || Game1.CurrentEvent != null || Game1.activeClickableMenu != null) return;
            if (!Context.IsPlayerFree) return;
            if (_activityCounter >= _config.MaxHospitalActivitiesPerStay) return;

            int intervalMinutes = Math.Max(1, _config.HospitalActivityIntervalMinutes);
            int progressMinutes = hospitalization.HospitalStayProgressMinutes;

            if (_lastActivityAtProgressMinutes < 0)
            {
                _lastActivityAtProgressMinutes = progressMinutes;
                return;
            }

            if (!_visitorThisStay && progressMinutes >= VisitorAfterProgressMinutes)
                TryFriendVisit();

            while (_lastActivityAtProgressMinutes + intervalMinutes <= progressMinutes
                && _activityCounter < _config.MaxHospitalActivitiesPerStay)
            {
                _lastActivityAtProgressMinutes += intervalMinutes;
                TriggerRandomActivity();
            }
        }

        private void TriggerRandomActivity()
        {
            if (_availableActivities.Count == 0) return;

            NPC? harvey = HarveyHelper.GetHarvey();
            if (harvey == null) return;

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

        private void ShowActivity(NPC harvey, string dialogue)
        {
            _dialogueManager.Speak(harvey, dialogue);
            Game1.playSound("healSound");
        }

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

        // ============================================================================
        // РАЗГОВОР С ХАРВИ В ПАЛАТЕ — выбор, чем занять время
        // ============================================================================

        /// <summary>
        /// Клик по Харви во время госпитализации: вместо обычного диалога — выбор занятия.
        /// Поспать — прокрутить время до выписки; поговорить; попросить почитать.
        /// </summary>
        public bool TryShowHospitalTalkMenu(NPC harvey, HospitalizationManager hospitalization)
        {
            if (!hospitalization.IsHospitalized || hospitalization.HasPendingReturnToHospital)
                return false;

            var choices = new List<Response>();
            int remaining = hospitalization.RemainingStayMinutes;
            if (remaining > 0)
                choices.Add(new Response("rest", $"Поспать (до выписки {FormatMinutes(remaining)})"));
            if (!_talkedThisStay)
                choices.Add(new Response("talk", "Поговорить с Харви"));
            if (!_readThisStay)
                choices.Add(new Response("read", "Попросить что-нибудь почитать"));
            if (remaining <= 0)
                choices.Add(new Response("discharge", "Спросить про выписку"));
            choices.Add(new Response("nothing", "Ничего, просто лежу"));

            harvey.facePlayer(Game1.player);
            Game1.currentLocation.createQuestionDialogue(
                remaining > 0
                    ? "Харви: Как ты? Тебе ещё нужно полежать. Чем займёмся?"
                    : $"Харви: Показатели хорошие. Можешь идти, если {(Game1.player.IsMale ? "готов" : "готова")}.",
                choices.ToArray(),
                (_, answer) => OnHospitalTalkAnswer(harvey, hospitalization, answer));
            return true;
        }

        private void OnHospitalTalkAnswer(NPC harvey, HospitalizationManager hospitalization, string answer)
        {
            switch (answer)
            {
                case "rest":
                    RestUntilDischarge(hospitalization);
                    break;
                case "talk":
                    _talkedThisStay = true;
                    ShowConversation(harvey);
                    Game1.player.changeFriendship(15, harvey);
                    break;
                case "read":
                    _readThisStay = true;
                    ReadSomething(harvey);
                    break;
                case "discharge":
                    _dialogueManager.Speak(harvey, "Можешь идти. Но сегодня — без шахты и тяжёлой работы, ладно?$u");
                    break;
            }
        }

        /// <summary>Сон в палате: затемнение, прокрутка часов до конца срока, полное восстановление сил.</summary>
        private void RestUntilDischarge(HospitalizationManager hospitalization)
        {
            int remaining = hospitalization.RemainingStayMinutes;
            int steps = (remaining + 9) / 10;

            Game1.globalFadeToBlack(() =>
            {
                int done = 0;
                // Не спим за полночь — дальше начинается усталость и обморок.
                while (done < steps && Game1.timeOfDay < 2400)
                {
                    Game1.performTenMinuteClockUpdate();
                    done++;
                }

                hospitalization.ApplyRestedMinutes(done * 10);
                Game1.player.health = Game1.player.maxHealth;
                Game1.player.Stamina = Game1.player.MaxStamina;
                Game1.globalFadeToClear();
                Game1.addHUDMessage(new HUDMessage(
                    $"Ты {(Game1.player.IsMale ? "проспал" : "проспала")} {FormatMinutes(done * 10)}. Силы восстановлены.",
                    HUDMessage.health_type));
                _monitor.Log($"🏥 Сон в палате: +{done * 10} мин к сроку", LogLevel.Info);
            });
        }

        private static readonly (string Text, int Skill)[] ReadingOptions =
        {
            ("Харви приносит старый справочник по травам. Ты узнаёшь пару новых растений.$h", Farmer.foragingSkill),
            ("Харви даёт журнал о садоводстве из приёмной. Пара советов про почву — очень кстати.$h", Farmer.farmingSkill),
            ("Харви находит книжку про рыб Долины. «Не спрашивай, откуда она у меня».$h", Farmer.fishingSkill),
            ("Харви приносит брошюру по технике безопасности в шахтах. Подозрительно уместно.$u", Farmer.miningSkill),
        };

        private void ReadSomething(NPC harvey)
        {
            var (text, skill) = ReadingOptions[Game1.random.Next(ReadingOptions.Length)];
            _dialogueManager.Speak(harvey, text);
            Game1.player.gainExperience(skill, 40);
        }

        // ============================================================================
        // ВИЗИТ ДРУГА
        // ============================================================================

        private static readonly string[] VisitorGiftItems = { "(O)196", "(O)223", "(O)614", "(O)613", "(O)216" };

        /// <summary>Самый близкий друг (от 2 сердечек) заходит проведать и оставляет гостинец.</summary>
        private void TryFriendVisit()
        {
            _visitorThisStay = true;

            string? bestName = null;
            int bestPoints = 499;
            foreach (var (name, friendship) in Game1.player.friendshipData.Pairs)
            {
                if (string.Equals(name, "Harvey", StringComparison.OrdinalIgnoreCase) || friendship.Points <= bestPoints)
                    continue;
                bestPoints = friendship.Points;
                bestName = name;
            }

            NPC? friend = bestName != null ? Game1.getCharacterFromName(bestName) : null;
            if (friend == null)
                return;

            var gift = ItemRegistry.Create(VisitorGiftItems[Game1.random.Next(VisitorGiftItems.Length)]);
            Game1.player.addItemByMenuIfNecessary(gift);
            Game1.player.changeFriendship(10, friend);
            Game1.addHUDMessage(new HUDMessage(
                $"{friend.displayName} заглядывает проведать и оставляет гостинец: {gift.DisplayName}.",
                HUDMessage.newQuest_type));
            Game1.playSound("give_gift");
            _monitor.Log($"🏥 Визит друга: {bestName}, подарок {gift.QualifiedItemId}", LogLevel.Info);
        }

        private static string FormatMinutes(int minutes)
        {
            int h = minutes / 60, m = minutes % 60;
            if (h == 0) return $"{m} мин";
            return m == 0 ? $"{h} ч" : $"{h} ч {m} мин";
        }

        public void Reset()
        {
            _lastActivityAtProgressMinutes = -1;
            _activityCounter = 0;
            _talkedThisStay = false;
            _readThisStay = false;
            _visitorThisStay = false;
            _monitor.Log("🏥 Сброс активностей госпитализации", LogLevel.Debug);
        }
    }
}
