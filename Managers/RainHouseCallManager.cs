using System;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Визит на дом в дождь: если ты болеешь или лечишь травму, на улице дождь, а с Харви 6+ сердец
    /// (и он ещё не супруг — у супруга своя домашняя забота), утром он заходит перед работой с чаем.
    /// Не чаще раза в неделю; только пока ты дома и свободна утром — не прерывает дела.
    /// </summary>
    public sealed class RainHouseCallManager
    {
        private const int MinHearts = 6;
        private const int CooldownDays = 7;
        private const int LatestTime = 1000;
        private const int FriendshipBonus = 20;

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly HospitalizationManager _hospitalizationManager;

        private bool _pendingToday;

        public RainHouseCallManager(
            IMonitor monitor,
            StateManager stateManager,
            DialogueManager dialogueManager,
            HospitalizationManager hospitalizationManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _hospitalizationManager = hospitalizationManager;
        }

        public void OnDayStarted()
        {
            _pendingToday = IsEligibleToday();
            if (_pendingToday)
                _monitor.Log("[RainHouseCall] сегодня Харви зайдёт утром", LogLevel.Debug);
        }

        public void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!_pendingToday || !Context.IsWorldReady)
                return;

            if (Game1.timeOfDay > LatestTime)
            {
                _pendingToday = false;
                return;
            }

            if (Game1.currentLocation is not FarmHouse || !Context.IsPlayerFree || Game1.CurrentEvent != null)
                return;

            _pendingToday = false;
            Visit();
        }

        private bool IsEligibleToday()
        {
            var player = Game1.player;
            if (player == null || !Game1.isRaining || Utility.isFestivalDay())
                return false;

            if (string.Equals(player.spouse, "Harvey", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player.getFriendshipHeartLevelForNPC("Harvey") < MinHearts || _hospitalizationManager.IsHospitalized)
                return false;

            int today = GameUtils.Today();
            if (today - _stateManager.State.LastRainHouseCallDay < CooldownDays)
                return false;

            // Сегодня Харви и так придёт осмотреть запущенную травму (лестница последствий) — один визит за утро.
            var ladder = _stateManager.State.NeglectLadder;
            if (ladder.HouseCallDay >= 0 && ladder.HouseCallDay <= today)
                return false;

            return _stateManager.GetAllActiveDebuffStates()
                .Any(d => InjurySets.HarveyTreatable.Contains(d.BuffId) && (d.HarveyAware || !d.HiddenFromHarvey));
        }

        private void Visit()
        {
            bool dating = Game1.player.friendshipData.TryGetValue("Harvey", out var friendship) && friendship.IsDating();
            bool cold = _stateManager.GetDebuffState(InjuryBuffs.Cold) != null;
            string key = $"HarveyMod_RainHouseCall_{(cold ? "Cold" : "Injury")}_{(dating ? "Dating" : "Friend")}";

            string? line = _dialogueManager.TryLoadHarveyDialogue(key);
            if (string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"[RainHouseCall] нет реплики {key} — визит пропущен", LogLevel.Warn);
                return;
            }

            _stateManager.State.LastRainHouseCallDay = GameUtils.Today();
            _stateManager.Save();

            Game1.playSound("doorClose");
            _dialogueManager.SpeakHarvey(line);
            // Без меню поверх диалога: не влезло в рюкзак — чай остаётся на полу рядом.
            var tea = ItemRegistry.Create(MedicalItems.HerbalTea, 2);
            if (!Game1.player.addItemToInventoryBool(tea))
                Game1.createItemDebris(tea, Game1.player.getStandingPosition(), Game1.player.FacingDirection);

            var harvey = HarveyHelper.GetHarvey();
            if (harvey != null)
                Game1.player.changeFriendship(FriendshipBonus, harvey);

            _monitor.Log($"[RainHouseCall] визит: {key}", LogLevel.Info);
        }
    }
}
