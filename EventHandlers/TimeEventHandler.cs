using System;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик событий времени (изменение времени, ночные визиты)
    /// </summary>
    public class TimeEventHandler
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly HospitalActivityManager _hospitalActivityManager;
        private readonly TreatmentManager _treatmentManager;

        public TimeEventHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            HospitalizationManager hospitalizationManager,
            HospitalActivityManager hospitalActivityManager,
            TreatmentManager treatmentManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _hospitalizationManager = hospitalizationManager;
            _hospitalActivityManager = hospitalActivityManager;
            _treatmentManager = treatmentManager;
        }

        /// <summary>
        /// Обработать изменение игрового времени
        /// </summary>
        public void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            try
            {
                // Проверка разгрузки из госпиталя
                CheckHospitalDischarge(e.NewTime);

                // Обновить активности во время госпитализации
                _hospitalActivityManager.UpdateHospitalActivities(_hospitalizationManager);

                // Ночные визиты Харви (22:00-26:00)
                CheckNightVisit(e.NewTime);

                // Напоминания о визите к врачу
                CheckDoctorVisitReminders(e.NewTime);

                // Storm comfort: один C#-бросок в день → buff/topic для CP cutscenes
                StormComfortLauncher.TryDailyStormComfortRoll(
                    _monitor,
                    _stateManager,
                    _buffManager,
                    _dialogueManager,
                    e.NewTime);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка в OnTimeChanged: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Проверить, можно ли выписать игрока из госпиталя
        /// </summary>
        private void CheckHospitalDischarge(int newTime)
        {
            _hospitalizationManager.NotifyDischargeReadyIfNeeded();
        }

        /// <summary>
        /// Проверить ночной визит Харви (только Dating/Married, FarmHouse, severe).
        /// </summary>
        private void CheckNightVisit(int newTime)
        {
            // Ночное время: 22:00-26:00
            bool isNight = newTime >= 2200 && newTime <= 2600;
            if (!isNight) return;

            // Должен быть дома
            bool atHome = Game1.player.currentLocation is FarmHouse;
            if (!atHome) return;

            // Должна быть серьёзная травма
            bool hasSevereInjury = InjurySets.Severe.Any(Game1.player.hasBuff);
            if (!hasSevereInjury) return;

            if (!_dialogueManager.IsDatingOrMarriedToHarvey())
                return;

            int today = Helpers.GameUtils.Today();

            // TimeChanged вызывается много раз за ночь — roll один раз за день/ночь.
            // LastNightRoundRollDay = попытка; LastNightRoundDay = только после показа визита.
            if (_stateManager.State.LastNightRoundRollDay == today)
                return;

            _stateManager.State.LastNightRoundRollDay = today;
            _stateManager.Save();

            // 35% шанс визита ЗА НОЧЬ, а не за каждый TimeChanged.
            if (!Helpers.GameUtils.Roll(Math.Clamp(_config.NightVisitChance, 0.0, 1.0)))
            {
                _monitor.Log("Ночной визит Харви: roll не сработал сегодня", LogLevel.Debug);
                return;
            }

            _monitor.Log("Ночной визит Харви", LogLevel.Info);

            var harvey = HarveyHelper.FindHarvey(Game1.currentLocation);
            string line = "Тихо постучал и заглянул — не спи на животе, ладно?$u#$b#" +
                            "Пульс ровный. Не геройствуй до утра — я присмотрю.$l";

            if (harvey != null)
                _dialogueManager.Speak(harvey, line);
            else
                Game1.drawObjectDialogue(line);

            _stateManager.State.LastNightRoundDay = today;
            _stateManager.Save();

            Game1.player.changeFriendship(10, Game1.getCharacterFromName("Harvey"));

            // Снять боль с 50% шансом
            if (Game1.player.hasBuff(InjuryBuffs.PainFlare) && Helpers.GameUtils.Roll(0.5))
            {
                _buffManager.RemoveBuff(InjuryBuffs.PainFlare);
                Game1.addHUDMessage(new HUDMessage("После ночного визита Харви боль утихла.", 2));
            }

            _dialogueManager.AddTopic("topicHarvey_NightRound", 2);
        }

        /// <summary>
        /// Проверить напоминания о визите к врачу
        /// </summary>
        private void CheckDoctorVisitReminders(int newTime)
        {
            // Не напоминать слишком часто (раз в 2 часа игрового времени)
            if (newTime % 200 != 0) return;

            // Проверяем наличие серьёзных травм без лечения
            var severeInjuries = InjurySets.Severe
                .Where(id => Game1.player.hasBuff(id))
                .ToList();

            if (severeInjuries.Count == 0) return;

            // Проверяем, есть ли лечение
            bool hasAnyCure = severeInjuries.Any(injury =>
            {
                if (TreatmentManager.CureByInjury.TryGetValue(injury, out var cure))
                {
                    return Game1.player.hasBuff(cure);
                }
                return false;
            });

            if (hasAnyCure) return; // Уже лечится

            // Показываем напоминание
            string injuryName = GetInjuryDisplayName(severeInjuries[0]);
            string message = $"У вас {injuryName}. Обратитесь к врачу!";
            ShowDoctorReminder(message, HUDMessage.error_type);
        }

        /// <summary>
        /// Показать напоминание о враче
        /// </summary>
        private void ShowDoctorReminder(string message, int messageType)
        {
            Game1.addHUDMessage(new HUDMessage(message, messageType));
            _monitor.Log($"Показано напоминание: {message}", LogLevel.Debug);
        }

        /// <summary>
        /// Получить отображаемое имя травмы
        /// </summary>
        private string GetInjuryDisplayName(string injuryId)
        {
            return injuryId switch
            {
                "buffConcussion" => "сотрясение мозга",
                "buffFracturedBone" => "перелом кости",
                "buffBadlyHurt" => "тяжёлая травма",
                "buffInfectedWound" => "инфицированная рана",
                "buffShrapnelWounds" => "осколочные раны",
                "buffBurnWounds" => "ожоги",
                "buffSurgicalWound" => "послеоперационная рана",
                _ => "серьёзная травма"
            };
        }
    }
}

