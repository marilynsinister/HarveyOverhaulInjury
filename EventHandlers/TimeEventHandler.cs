using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

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
        private readonly InjuryManager _injuryManager;
        private readonly ComplicationManager _complicationManager;
        private HarveyHomeCareEventLauncher? _homeCareLauncher;

        public TimeEventHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            HospitalizationManager hospitalizationManager,
            HospitalActivityManager hospitalActivityManager,
            TreatmentManager treatmentManager,
            InjuryManager injuryManager,
            ComplicationManager complicationManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _hospitalizationManager = hospitalizationManager;
            _hospitalActivityManager = hospitalActivityManager;
            _treatmentManager = treatmentManager;
            _injuryManager = injuryManager;
            _complicationManager = complicationManager;
        }

        public void SetHomeCareLauncher(HarveyHomeCareEventLauncher homeCareLauncher)
        {
            _homeCareLauncher = homeCareLauncher;
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

                _hospitalizationManager.SyncHospitalizedBuffOnTimeChanged(e.NewTime);

                // Обновить активности во время госпитализации
                _hospitalActivityManager.UpdateHospitalActivities(_hospitalizationManager, e.NewTime);

                // Домашние care-события Харви (единый приоритетный запуск)
                if (_homeCareLauncher?.TryTriggerHarveyHomeCareEvent(source: "TimeChanged") == true)
                    return;

                // Короткий ночной визит (22:00–26:00), если нет pending событий выше по приоритету
                _homeCareLauncher?.TryTriggerShortNightRoundVisit(e.NewTime);

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
            _hospitalizationManager.UpdateHospitalStayProgress(newTime);
            _hospitalizationManager.NotifyDischargeReadyIfNeeded();
        }

        /// <summary>
        /// Проверить напоминания о визите к врачу
        /// </summary>
        private void CheckDoctorVisitReminders(int newTime)
        {
            // Не напоминать слишком часто (раз в 2 часа игрового времени)
            if (newTime % 200 != 0) return;

            string? mainInjuryId = _injuryManager.GetActiveInjury();
            if (string.IsNullOrEmpty(mainInjuryId)
                || !_injuryManager.HasInjuryOrPhase(mainInjuryId)
                || !_injuryManager.IsMainInjurySerious())
            {
                return;
            }

            var debuffState = _stateManager.GetDebuffState(mainInjuryId);
            if (debuffState?.TreatmentStarted == true)
                return;

            if (TreatmentManager.CureByInjury.TryGetValue(mainInjuryId, out var cure)
                && (_buffManager.HasBuff(cure) || _buffManager.HasBuff(CureBuffs.BadlyHurtOutpatientCare)))
            {
                return;
            }

            string injuryName = GetInjuryDisplayName(mainInjuryId);
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
