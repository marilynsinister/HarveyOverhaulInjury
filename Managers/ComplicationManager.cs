using System;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление осложнениями и проверками лечения
    /// </summary>
    public class ComplicationManager
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly InjuryManager _injuryManager;

        public ComplicationManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _injuryManager = injuryManager;
        }

        /// <summary>
        /// Проверить завершение/осложнения лечения (вызывается каждый день)
        /// </summary>
        public void CheckTreatmentCompletion()
        {
            int today = GameUtils.Today();

            // Проверка осложнений от грязной раны
            CheckDirtyWoundComplication(today);

            // Проверка осложнений от мокрой повязки
            CheckWetBandageComplication(today);

            // Проверка фазовых травм на небрежность
            CheckPhaseNeglect(today);
        }

        /// <summary>
        /// Проверить осложнение: Грязная рана → Инфекция
        /// </summary>
        private void CheckDirtyWoundComplication(int today)
        {
            if (!_stateManager.State.ActiveComplications.TryGetValue(InjuryBuffs.DirtyWound, out int startDay))
                return;

            int days = today - startDay;
            double infectionChance = days switch
            {
                0 => 0.0,
                1 => 0.15,
                2 => 0.40,
                _ => 1.0
            };

            if (!GameUtils.Roll(infectionChance)) return;

            _monitor.Log($"ОСЛОЖНЕНИЕ: Грязная рана → инфекция (день {days})", LogLevel.Warn);

            _buffManager.RemoveBuff(InjuryBuffs.DirtyWound);
            _stateManager.State.ActiveComplications.Remove(InjuryBuffs.DirtyWound);
            _stateManager.RemoveDebuffState(InjuryBuffs.DirtyWound);
            _dialogueManager.RemoveTopic(ConversationTopics.DirtyWound);

            _injuryManager.ApplyInfectedWoundSafe();

            if (_config.SendLetters)
                Game1.addMailForTomorrow(MailIds.DirtyWoundInfection);

            Game1.addHUDMessage(new HUDMessage("Грязная рана инфицирована! Срочно к врачу!", HUDMessage.error_type));
        }

        /// <summary>
        /// Проверить осложнение: Мокрая повязка → Инфекция
        /// </summary>
        private void CheckWetBandageComplication(int today)
        {
            if (!_stateManager.State.ActiveComplications.TryGetValue(InjuryBuffs.WetBandage, out int startDay))
                return;

            int days = today - startDay;
            double infectionChance = CalculateWetBandageInfectionChance(days);

            if (infectionChance <= 0)
            {
                _monitor.Log($"[WetBandage] Инфекция не проверяется: days={days}, startDay={startDay}, today={today}", LogLevel.Debug);
                return;
            }

            _monitor.Log(
                $"[WetBandage] Проверка инфекции: startDay={startDay}, today={today}, days={days}, chance={infectionChance:P0}",
                LogLevel.Debug);

            if (!GameUtils.Roll(infectionChance)) return;

            _monitor.Log($"ОСЛОЖНЕНИЕ: Мокрая повязка → инфекция (день {days})", LogLevel.Warn);

            _buffManager.RemoveBuff(InjuryBuffs.WetBandage);
            _stateManager.State.ActiveComplications.Remove(InjuryBuffs.WetBandage);
            _stateManager.RemoveDebuffState(InjuryBuffs.WetBandage);
            _dialogueManager.RemoveTopic(ConversationTopics.WetBandage);

            _injuryManager.ApplyInfectedWoundSafe();

            if (_config.SendLetters)
                Game1.addMailForTomorrow(MailIds.WetBandageInfection);

            Game1.addHUDMessage(new HUDMessage("Мокрая повязка привела к инфекции!", HUDMessage.error_type));
        }

        // Баланс мокрой повязки:
        // days = today - startDay
        // 0: нет инфекции в тот же день (0%)
        // 1: мягкий риск (15%)
        // 2: заметный риск (35%)
        // 3+: высокий, но не гарантированный риск (65%; не legacy 25/60)
        private static double CalculateWetBandageInfectionChance(int days)
        {
            return days switch
            {
                <= 0 => 0.0,
                1 => 0.15,
                2 => 0.35,
                _ => 0.65
            };
        }

        /// <summary>
        /// Проверить фазовые травмы на небрежность.
        /// Каждый тип письма/уведомления отправляется не более одного раза за вызов,
        /// даже если несколько травм одновременно достигли порога.
        /// </summary>
        private void CheckPhaseNeglect(int today)
        {
            // Флаги: отправлено ли уже данное письмо в текущем вызове
            bool sentUrgentReminder = false;
            bool sentFinalWarning   = false;
            bool sentNeglect        = false;

            foreach (var kv in _stateManager.State.ActiveDebuffs.ToList())
            {
                string injuryId = kv.Key;
                var debuffState = kv.Value;

                // Небрежность — только фазовые травмы в активной фазе (не buffHurt и не осложнения)
                if (!debuffState.IsPhasedInjury || !debuffState.IsInTreatment)
                    continue;

                int daysSincePhaseStart = today - debuffState.PhaseStartDay;
                int currentPhaseDuration = debuffState.GetCurrentPhaseDuration();

                int gracePeriod     = 7; // дней отсрочки после окончания фазы
                int totalAllowedDays = currentPhaseDuration + gracePeriod;

                // Первое предупреждение за 4 дня до осложнения
                if (daysSincePhaseStart == currentPhaseDuration + 3)
                {
                    _monitor.Log($"⚠️ Предупреждение: {injuryId} требует осмотра через 4 дня", LogLevel.Warn);

                    if (!sentUrgentReminder)
                    {
                        sentUrgentReminder = true;
                        if (_config.SendLetters)
                            Game1.addMailForTomorrow(MailIds.TreatmentUrgentReminder);
                        Game1.addHUDMessage(new HUDMessage("Харви настаивает на осмотре!", HUDMessage.health_type));
                    }
                }
                // Финальное предупреждение за 1 день до осложнения
                else if (daysSincePhaseStart == totalAllowedDays - 1)
                {
                    _monitor.Log($"🚨 ФИНАЛЬНОЕ предупреждение: {injuryId} требует осмотра завтра!", LogLevel.Error);

                    if (!sentFinalWarning)
                    {
                        sentFinalWarning = true;
                        if (_config.SendLetters)
                            Game1.addMailForTomorrow(MailIds.TreatmentFinalWarning);
                        Game1.addHUDMessage(new HUDMessage("СРОЧНО! Необходим осмотр!", HUDMessage.error_type));
                    }
                }
                // Применение небрежности при превышении срока
                else if (daysSincePhaseStart >= totalAllowedDays)
                {
                    _monitor.Log($"🚫 ОСЛОЖНЕНИЕ: {injuryId} → небрежность (превышен срок на {daysSincePhaseStart - totalAllowedDays} дней)",
                        LogLevel.Error);

                    if (!Game1.player.hasBuff(InjuryBuffs.Neglect))
                    {
                        _buffManager.AddBuff(InjuryBuffs.Neglect, -2);
                        _dialogueManager.AddTopic(ConversationTopics.Neglect, 7);
                        _stateManager.State.NeglectStrikes++;
                        _monitor.Log($"📊 Счетчик небрежности: {_stateManager.State.NeglectStrikes}", LogLevel.Warn);
                    }

                    if (!sentNeglect)
                    {
                        sentNeglect = true;
                        if (_config.SendLetters)
                            Game1.addMailForTomorrow(MailIds.NeglectWarning);
                        Game1.addHUDMessage(new HUDMessage("Травма ухудшилась из-за отсутствия лечения!", HUDMessage.error_type));
                    }
                }
            }
        }
    }
}

