using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>Уровень скрытого медицинского доверия Харви.</summary>
    public enum CareTrustLevel
    {
        Low,
        Medium,
        High,
    }

    /// <summary>
    /// Скрытое медицинское доверие Харви (CareTrust).
    /// Не Friendship: отдельный показатель заботы и соблюдения медицинских рекомендаций.
    /// </summary>
    public class CareTrustManager
    {
        private const int CareTrustTopicDays = 3;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;

        public CareTrustManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
        }

        public int GetTrust() => _stateManager.State.CareTrust;

        public CareTrustLevel GetLevel()
        {
            int trust = GetTrust();

            if (trust <= _config.CareTrustLowThreshold)
                return CareTrustLevel.Low;

            if (trust >= _config.CareTrustHighThreshold)
                return CareTrustLevel.High;

            return CareTrustLevel.Medium;
        }

        /// <summary>Суффикс уровня для CP-ключей HarveyCareTrust_*_{Level}_XX.</summary>
        public string GetLevelSuffix() => GetLevel() switch
        {
            CareTrustLevel.Low => "Low",
            CareTrustLevel.High => "High",
            _ => "Medium",
        };

        /// <summary>Синхронизирует CP-топик уровня CareTrust (PLAYER_HAS_CONVERSATION_TOPIC).</summary>
        public void SyncCareTrustTopic()
        {
            _dialogueManager.RemoveTopic(CareTrustTopics.Low);
            _dialogueManager.RemoveTopic(CareTrustTopics.Medium);
            _dialogueManager.RemoveTopic(CareTrustTopics.High);

            if (!_config.EnableCareTrust)
                return;

            string topicId = GetLevel() switch
            {
                CareTrustLevel.Low => CareTrustTopics.Low,
                CareTrustLevel.High => CareTrustTopics.High,
                _ => CareTrustTopics.Medium,
            };

            _dialogueManager.AddTopic(topicId, CareTrustTopicDays);
        }

        public string GetMineWarningHudLine(bool severe, bool forbidden)
        {
            if (forbidden)
            {
                return GetLevel() switch
                {
                    CareTrustLevel.Low => "Харви: Нет. Сегодня шахта закрыта. Даже не спорь.",
                    CareTrustLevel.High => "Харви: Я доверяю тебе. Но если станет хуже — сразу возвращайся ко мне.",
                    _ => "Харви: Я очень прошу тебя не идти. Дай мне хотя бы один день спокойствия.",
                };
            }

            return GetLevel() switch
            {
                CareTrustLevel.Low => "Харви: У тебя серьёзные раны. Шахта исключена.",
                CareTrustLevel.High => "Харви: Я знаю, ты осторожна. Но с такой травмой — только без геройства.",
                _ => "Харви: Пожалуйста, не рискуй сегодня. Твоему телу нужен отдых.",
            };
        }

        public void AddTrust(int amount, string reason)
        {
            if (!_config.EnableCareTrust || amount == 0)
                return;

            ApplyTrust(GetTrust() + amount, reason);
        }

        public void PenalizeTrust(int amount, string reason)
        {
            if (!_config.EnableCareTrust || amount == 0)
                return;

            ApplyTrust(GetTrust() - amount, reason);
        }

        public void SetTrust(int value, string reason)
        {
            ApplyTrust(value, reason);
        }

        /// <summary>+1 CareTrust за своевременный осмотр у Харви, не чаще 1× в день.</summary>
        public void RewardTimelyCheckupOncePerDay()
        {
            int today = (int)Game1.stats.DaysPlayed;
            var state = _stateManager.State;

            if (state.LastCareTrustCheckupRewardDay == today)
                return;

            state.LastCareTrustCheckupRewardDay = today;
            AddTrust(1, "timely_checkup");
            _stateManager.Save();
        }

        /// <summary>Штраф CareTrust за нарушение шахтного режима, не чаще 1× в день.</summary>
        public void PenalizeMineViolationOncePerDay(bool severe)
        {
            int today = (int)Game1.stats.DaysPlayed;
            var state = _stateManager.State;

            if (state.LastCareTrustMineViolationDay == today)
                return;

            state.LastCareTrustMineViolationDay = today;

            if (severe)
                PenalizeTrust(2, "mine_with_severe_injury");
            else
                PenalizeTrust(1, "mine_ban_violation");

            _stateManager.Save();
        }

        /// <summary>+1 CareTrust за день соблюдения запрета Харви на шахту.</summary>
        public void RewardMineBanObeyedIfEligible()
        {
            int today = (int)Game1.stats.DaysPlayed;
            var state = _stateManager.State;

            if (state.MineForbiddenAppliedDay < 0)
                return;

            if (!MineForbiddenHelper.IsMineForbiddenActive(state, _config, today))
                return;

            if (state.LastCareTrustMineViolationDay == today)
                return;

            if (state.LastCareTrustMineBanRewardDay == today)
                return;

            state.LastCareTrustMineBanRewardDay = today;
            AddTrust(1, "mine_ban_obeyed");
            _stateManager.Save();
        }

        /// <summary>+1 CareTrust за ранний сон при активной тяжёлой травме, не чаще 1× в день.</summary>
        public void RewardEarlySleepIfEligible(bool hasSevereInjury)
        {
            if (!hasSevereInjury)
                return;

            if (Game1.timeOfDay >= 2400)
                return;

            int today = (int)Game1.stats.DaysPlayed;
            var state = _stateManager.State;

            if (state.LastCareTrustEarlySleepRewardDay == today)
                return;

            state.LastCareTrustEarlySleepRewardDay = today;
            AddTrust(1, "early_sleep_after_severe_injury");
            _stateManager.Save();
        }

        private void ApplyTrust(int newValue, string reason)
        {
            var state = _stateManager.State;
            int oldValue = state.CareTrust;
            int clamped = Math.Clamp(newValue, _config.CareTrustMin, _config.CareTrustMax);

            if (clamped == oldValue)
                return;

            state.CareTrust = clamped;
            _stateManager.Save();
            _monitor.Log($"CareTrust {oldValue} → {clamped} ({reason})", LogLevel.Debug);
            SyncCareTrustTopic();
        }
    }
}
