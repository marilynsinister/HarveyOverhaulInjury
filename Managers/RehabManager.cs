using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Восстановительный режим после полного выздоровления от тяжёлых травм.
    /// </summary>
    public class RehabManager
    {
        private const int StrictTopicDays = 2;
        private const int CompletedTopicDays = 3;
        private const int HeavyWorkViolationSeconds = 90;
        private const float LowStaminaFraction = 0.25f;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly BuffManager _buffManager;
        private readonly ComplianceManager _complianceManager;

        private int _heavyWorkLowStaminaSeconds;

        private static readonly Dictionary<string, int> RehabDurations =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["buffConcussion"] = 3,
                ["buffFracturedBone"] = 5,
                ["buffShrapnelWounds"] = 5,
                ["buffBurnWounds"] = 4,
                ["buffInfectedWound"] = 4,
                ["buffTornMuscles"] = 3,
                ["buffSurgicalWound"] = 4,
                ["buffBadlyHurt"] = 2,
            };

        public RehabManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            BuffManager buffManager,
            ComplianceManager complianceManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _buffManager = buffManager;
            _complianceManager = complianceManager;
        }

        /// <summary>
        /// Назначить реабилитацию после выздоровления, если травма входит в список тяжёлых.
        /// </summary>
        public bool TryStartRehabAfterRecovery(string injuryId)
        {
            if (string.IsNullOrWhiteSpace(injuryId))
                return false;

            if (!RehabDurations.TryGetValue(injuryId, out int days))
                return false;

            if (IsRehabActive())
            {
                _monitor.Log(
                    $"[Rehab] Пропуск старта для {injuryId}: уже активна реабилитация {_stateManager.State.ActiveRehabInjuryId}",
                    LogLevel.Debug);
                return false;
            }

            StartRehab(injuryId, days);
            return true;
        }

        /// <summary>Назначить реабилитацию (консоль / принудительный старт).</summary>
        public void StartRehab(string injuryId, int? durationDaysOverride = null)
        {
            if (string.IsNullOrWhiteSpace(injuryId))
                throw new ArgumentException("injuryId не может быть пустым", nameof(injuryId));

            int days = durationDaysOverride
                ?? (RehabDurations.TryGetValue(injuryId, out int mapped) ? mapped : 3);

            int today = GameUtils.Today();
            var state = _stateManager.State;

            state.ActiveRehabInjuryId = injuryId;
            state.RehabStartDay = today;
            state.RehabDurationDays = Math.Max(1, days);
            state.RehabViolated = false;
            state.RehabViolationCount = 0;
            state.LastRehabViolationDay = -1;
            _heavyWorkLowStaminaSeconds = 0;

            if (_buffManager.BuffExists(CureBuffs.Rehab))
                _buffManager.AddBuff(CureBuffs.Rehab, state.RehabDurationDays * 1440);

            _dialogueManager.AddTopic(ConversationTopics.Rehab, state.RehabDurationDays);

            Game1.addHUDMessage(new HUDMessage(
                "Харви назначил восстановительный режим на несколько дней.",
                HUDMessage.health_type));

            HarveyMailHelper.TryScheduleTieredMail(_config, _stateManager, _monitor, MailIds.RehabReminder);

            _stateManager.Save();
            _monitor.Log(
                $"[Rehab] Старт: {injuryId}, {state.RehabDurationDays} дн. (день {today})",
                LogLevel.Info);
        }

        public void CheckRehabViolationOnMine()
        {
            if (!IsRehabActive())
                return;

            if (!IsInsideMineOrVolcano(Game1.currentLocation))
                return;

            MarkViolation("mine");
        }

        /// <summary>Вызывать раз в секунду из PlayerEventHandler.</summary>
        public void CheckRehabViolationOnHeavyWork()
        {
            if (!IsRehabActive())
            {
                _heavyWorkLowStaminaSeconds = 0;
                return;
            }

            if (!IsHeavyToolInUse())
            {
                _heavyWorkLowStaminaSeconds = 0;
                return;
            }

            float maxStamina = Math.Max(1f, Game1.player.MaxStamina);
            if (Game1.player.Stamina > maxStamina * LowStaminaFraction)
            {
                _heavyWorkLowStaminaSeconds = 0;
                return;
            }

            _heavyWorkLowStaminaSeconds++;
            if (_heavyWorkLowStaminaSeconds < HeavyWorkViolationSeconds)
                return;

            if (MarkViolation("heavy_work"))
                _heavyWorkLowStaminaSeconds = 0;
        }

        public void CheckRehabViolationLateSleep()
        {
            if (Game1.timeOfDay < 2400)
                return;

            if (!IsRehabActive())
                return;

            MarkViolation("late_sleep");
        }

        public void CompleteRehabIfDue(int today)
        {
            var state = _stateManager.State;
            if (string.IsNullOrEmpty(state.ActiveRehabInjuryId))
                return;

            if (today < state.RehabStartDay + state.RehabDurationDays)
                return;

            string injuryId = state.ActiveRehabInjuryId;
            bool hadViolations = state.RehabViolationCount > 0 || state.RehabViolated;

            _buffManager.RemoveBuff(CureBuffs.Rehab);
            _dialogueManager.AddTopic(ConversationTopics.RehabCompleted, CompletedTopicDays);
            HarveyMailHelper.TryScheduleTieredMail(_config, _stateManager, _monitor, MailIds.RehabCompleted);

            if (!hadViolations)
            {
                _complianceManager.AddCompliance(+1, "rehab_perfect");
                int careMinutes = Game1.random.Next(1, 3) * 1440;
                if (_buffManager.BuffExists(CureBuffs.Care))
                    _buffManager.AddBuff(CureBuffs.Care, careMinutes);
            }

            state.ActiveRehabInjuryId = null;
            state.RehabStartDay = -1;
            state.RehabDurationDays = 0;
            state.RehabViolated = false;
            state.RehabViolationCount = 0;
            state.LastRehabViolationDay = -1;
            _heavyWorkLowStaminaSeconds = 0;

            _stateManager.Save();
            _monitor.Log(
                $"[Rehab] Завершена реабилитация после {injuryId} (violations={hadViolations})",
                LogLevel.Info);
        }

        public bool IsRehabActive()
        {
            var state = _stateManager.State;
            if (string.IsNullOrEmpty(state.ActiveRehabInjuryId))
                return false;

            return GetRehabDaysLeft() > 0;
        }

        public int GetRehabDaysLeft()
        {
            var state = _stateManager.State;
            if (string.IsNullOrEmpty(state.ActiveRehabInjuryId) || state.RehabStartDay < 0)
                return 0;

            int today = GameUtils.Today();
            return Math.Max(0, state.RehabStartDay + state.RehabDurationDays - today);
        }

        public void ClearRehab()
        {
            _buffManager.RemoveBuff(CureBuffs.Rehab);

            var state = _stateManager.State;
            state.ActiveRehabInjuryId = null;
            state.RehabStartDay = -1;
            state.RehabDurationDays = 0;
            state.RehabViolated = false;
            state.RehabViolationCount = 0;
            state.LastRehabViolationDay = -1;
            _heavyWorkLowStaminaSeconds = 0;

            _dialogueManager.RemoveTopic(ConversationTopics.Rehab);
            _dialogueManager.RemoveTopic(ConversationTopics.RehabStrict);
            _dialogueManager.RemoveTopic(ConversationTopics.RehabCompleted);

            _stateManager.Save();
            _monitor.Log("[Rehab] Сброшена", LogLevel.Info);
        }

        public IEnumerable<string> GetStatusLines()
        {
            var state = _stateManager.State;
            if (string.IsNullOrEmpty(state.ActiveRehabInjuryId))
            {
                yield return "(none)";
                yield break;
            }

            int today = GameUtils.Today();
            yield return
                $"injury={state.ActiveRehabInjuryId}  start={state.RehabStartDay}  duration={state.RehabDurationDays}d  left={GetRehabDaysLeft()}d  viol={state.RehabViolationCount}  violated={state.RehabViolated}  lastViolDay={state.LastRehabViolationDay}";
        }

        private bool MarkViolation(string reason)
        {
            int today = GameUtils.Today();
            var state = _stateManager.State;

            if (state.LastRehabViolationDay == today)
                return false;

            state.LastRehabViolationDay = today;
            state.RehabViolated = true;
            state.RehabViolationCount++;

            _complianceManager.AddCompliance(-1, $"rehab_{reason}");
            _dialogueManager.AddTopic(ConversationTopics.RehabStrict, StrictTopicDays);

            Game1.addHUDMessage(new HUDMessage(
                "Ты слишком рано вернулась к нагрузкам. Тело отзывается болью.",
                HUDMessage.error_type));

            _stateManager.Save();
            _monitor.Log(
                $"[Rehab] Нарушение #{state.RehabViolationCount} ({reason})",
                LogLevel.Info);
            return true;
        }

        private static bool IsHeavyToolInUse()
        {
            if (Game1.player?.UsingTool != true)
                return false;

            return Game1.player.CurrentTool?.GetType().Name switch
            {
                "Axe" or "Pickaxe" or "Hoe" or "WateringCan" => true,
                _ => false,
            };
        }

        private static bool IsInsideMineOrVolcano(GameLocation? location)
        {
            return location is MineShaft or VolcanoDungeon;
        }
    }
}
