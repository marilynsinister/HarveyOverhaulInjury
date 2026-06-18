using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Утренние/вечерние domestic-триггеры обнаружения скрытых травм дома (dating/engaged/married).
    /// </summary>
    public sealed class DomesticHiddenInjuryManager
    {
        private const string LogTag = "[DomesticHiddenInjury]";

        private static readonly HashSet<string> DomesticPriorityInjuries = new(StringComparer.OrdinalIgnoreCase)
        {
            "buffDeepCuts",
            "buffConcussion",
            "buffFracturedBone",
            "buffInfectedWound",
        };

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly HiddenInjuryDialogueFlow _hiddenInjuryDialogueFlow;

        public DomesticHiddenInjuryManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            HiddenInjuryDialogueFlow hiddenInjuryDialogueFlow)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _hiddenInjuryDialogueFlow = hiddenInjuryDialogueFlow;
        }

        public void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!_config.EnableDomesticHiddenInjuryChecks || !_config.EnableMorningHiddenInjuryCheck)
                return;

            Game1.delayedActions.Add(new DelayedAction(2800, () => { TryMorningDomesticHiddenInjuryCheck(); }));
        }

        public void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!_config.EnableDomesticHiddenInjuryChecks || !_config.EnableEveningHiddenInjuryCheck)
                return;

            if (!DidEnterEveningWindow(e.OldTime, e.NewTime))
                return;

            TryEveningDomesticHiddenInjuryCheck(e.NewTime);
        }

        /// <summary>QA / ручной вызов утреннего check.</summary>
        public bool TryMorningDomesticHiddenInjuryCheck()
        {
            if (!_config.EnableDomesticHiddenInjuryChecks || !_config.EnableMorningHiddenInjuryCheck)
            {
                LogSkip("morning", "disabled in config");
                return false;
            }

            int today = GameUtils.Today();
            if (_stateManager.State.LastMorningHiddenInjuryCheckDay == today)
            {
                LogSkip("morning", "already checked today");
                return false;
            }

            int time = Game1.timeOfDay;
            if (time < _config.MorningHiddenInjuryStartTime || time > _config.MorningHiddenInjuryEndTime)
            {
                LogSkip("morning", $"outside time window ({time})");
                return false;
            }

            if (!IsValidDomesticHiddenInjuryContext("morning", out string? rejectReason))
            {
                LogSkip("morning", rejectReason ?? "invalid context");
                return false;
            }

            _stateManager.State.LastMorningHiddenInjuryCheckDay = today;
            _stateManager.State.LastDomesticHiddenInjuryCheckTime = time;
            _stateManager.Save();

            return TryRunDomesticFlow("morning_home", isEvening: false);
        }

        private void TryEveningDomesticHiddenInjuryCheck(int newTime)
        {
            int today = GameUtils.Today();
            if (_stateManager.State.LastEveningHiddenInjuryCheckDay == today)
            {
                LogSkip("evening", "already checked today");
                return;
            }

            if (newTime < _config.EveningHiddenInjuryStartTime || newTime > _config.EveningHiddenInjuryEndTime)
            {
                LogSkip("evening", $"outside time window ({newTime})");
                return;
            }

            if (!IsValidDomesticHiddenInjuryContext("evening", out string? rejectReason))
            {
                LogSkip("evening", rejectReason ?? "invalid context");
                return;
            }

            _stateManager.State.LastEveningHiddenInjuryCheckDay = today;
            _stateManager.State.LastDomesticHiddenInjuryCheckTime = newTime;
            _stateManager.Save();

            TryRunDomesticFlow("evening_home", isEvening: true);
        }

        private bool TryRunDomesticFlow(string reason, bool isEvening)
        {
            NPC? harvey = HarveyHelper.FindHarveyInLocation(Game1.currentLocation);
            if (harvey == null)
            {
                LogSkip(reason, "Harvey missing after context validation");
                return false;
            }

            DebuffState? chosen = PickMostImportantDomesticHiddenInjury();
            if (chosen == null)
            {
                LogSkip(reason, "no eligible hidden injury");
                return false;
            }

            if (!PassDomesticDetectionRoll(chosen))
            {
                LogSkip(
                    reason,
                    $"detection roll failed visibility={InjuryVisibilityHelper.GetVisibilityLevel(chosen)} "
                    + $"hiddenDays={chosen.HiddenDays} suspicion={chosen.SuspicionLevel}");
                return false;
            }

            bool started = _hiddenInjuryDialogueFlow.TryStartDomesticHiddenInjuryFlow(harvey, reason, chosen);
            if (!started)
            {
                LogSkip(reason, "flow not started (anti-spam or pending question)");
                return false;
            }

            if (isEvening)
                _stateManager.State.LastEveningHiddenInjuryBuffId = chosen.BuffId;
            else
                _stateManager.State.LastMorningHiddenInjuryBuffId = chosen.BuffId;

            _stateManager.Save();

            _monitor.Log(
                $"{LogTag} reason={reason}, loc={Game1.currentLocation?.NameOrUniqueName}, time={Game1.timeOfDay}, "
                + $"hasHidden=true, chosen={chosen.BuffId}, visibility={InjuryVisibilityHelper.GetVisibilityLevel(chosen)}, "
                + $"hiddenDays={chosen.HiddenDays}, suspicion={chosen.SuspicionLevel}",
                LogLevel.Debug);
            return true;
        }

        private bool PassDomesticDetectionRoll(DebuffState state)
        {
            var level = InjuryVisibilityHelper.GetVisibilityLevel(state);

            if (level == InjuryVisibilityLevel.Unhideable)
                return true;

            if (level == InjuryVisibilityLevel.Hidden)
                return false;

            double chance = level switch
            {
                InjuryVisibilityLevel.Subtle => _config.DomesticSubtleDetectionChance,
                InjuryVisibilityLevel.Suspicious => _config.DomesticSuspiciousDetectionChance,
                InjuryVisibilityLevel.Obvious => _config.DomesticObviousDetectionChance,
                InjuryVisibilityLevel.Unhideable => 1.0,
                _ => 0.0,
            };

            chance += Math.Min(0.25, state.HiddenDays * 0.05);
            chance += Math.Min(0.25, state.SuspicionLevel * 0.08);

            if (HasComplicationFor(state.BuffId))
                chance += 0.25;

            if (Game1.player != null && Game1.player.health < Game1.player.maxHealth * 0.35f)
                chance += 0.2;

            return Game1.random.NextDouble() < Math.Clamp(chance, 0.0, 1.0);
        }

        private bool HasComplicationFor(string buffId)
        {
            var injuryState = _stateManager.State;
            return injuryState.ActiveComplications.Count > 0
                && string.Equals(injuryState.MainInjuryId, buffId, StringComparison.OrdinalIgnoreCase);
        }

        private bool DidEnterEveningWindow(int oldTime, int newTime) =>
            oldTime < _config.EveningHiddenInjuryStartTime
            && newTime >= _config.EveningHiddenInjuryStartTime
            && newTime <= _config.EveningHiddenInjuryEndTime;

        private bool IsValidDomesticHiddenInjuryContext(string reason, out string? rejectReason)
        {
            rejectReason = null;

            if (!Context.IsWorldReady || !Context.IsPlayerFree)
            {
                rejectReason = "player not free / world not ready";
                return false;
            }

            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                rejectReason = "event active";
                return false;
            }

            if (Game1.activeClickableMenu != null)
            {
                rejectReason = "active menu";
                return false;
            }

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.season) || Game1.isFestival())
            {
                rejectReason = "festival day";
                return false;
            }

            var location = Game1.currentLocation;
            if (location == null)
            {
                rejectReason = "no location";
                return false;
            }

            string locName = location.NameOrUniqueName ?? "";
            bool isHome = locName.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || locName.Equals("Cabin", StringComparison.OrdinalIgnoreCase);

            if (!isHome)
            {
                rejectReason = $"not home ({locName})";
                return false;
            }

            if (!HarveyHelper.IsRomanticPartnerWithHarvey())
            {
                rejectReason = "not dating/engaged/married to Harvey";
                return false;
            }

            NPC? harvey = HarveyHelper.FindHarveyInLocation(location);
            if (harvey == null)
            {
                rejectReason = "Harvey not in location";
                return false;
            }

            if (!location.characters.Contains(harvey))
            {
                rejectReason = "Harvey not in character list";
                return false;
            }

            if (!IsHarveyCloseEnough(harvey, _config.DomesticHiddenInjuryProximityTiles))
            {
                rejectReason = "Harvey too far";
                return false;
            }

            if (!HasAnyHiddenInjuryWorthDomesticCheck())
            {
                rejectReason = "no hidden injuries worth domestic check";
                return false;
            }

            return true;
        }

        private bool IsHarveyCloseEnough(NPC harvey, int maxTiles)
        {
            if (Game1.player == null)
                return false;

            float distance = Vector2.Distance(Game1.player.Tile, harvey.Tile);
            return distance <= maxTiles;
        }

        private bool HasAnyHiddenInjuryWorthDomesticCheck() =>
            _stateManager.GetAllActiveDebuffStates()
                .Any(s => s.HiddenFromHarvey && !s.HarveyAware && IsWorthDomesticCheck(s));

        internal static bool IsWorthDomesticCheck(DebuffState state)
        {
            var level = InjuryVisibilityHelper.GetVisibilityLevel(state);
            if (level >= InjuryVisibilityLevel.Obvious)
                return true;

            if (state.HiddenDays >= 2)
                return true;

            if (state.SuspicionLevel >= 2)
                return true;

            if (level >= InjuryVisibilityLevel.Suspicious)
                return true;

            return false;
        }

        internal DebuffState? PickMostImportantDomesticHiddenInjury()
        {
            return _stateManager.GetAllActiveDebuffStates()
                .Where(s => s.HiddenFromHarvey && !s.HarveyAware)
                .Where(s => _buffManager.HasBuff(s.BuffId) || _injuryManager.HasInjuryOrPhase(s.BuffId))
                .Where(IsWorthDomesticCheck)
                .OrderByDescending(GetDomesticPickScore)
                .FirstOrDefault();
        }

        internal static int GetDomesticPickScore(DebuffState state)
        {
            var level = InjuryVisibilityHelper.GetVisibilityLevel(state);
            int score = (int)level * 100;
            score += state.HiddenDays * 5;
            score += state.SuspicionLevel * 3;

            if (DomesticPriorityInjuries.Contains(state.BuffId))
                score += 40;

            if (level >= InjuryVisibilityLevel.Unhideable)
                score += 200;

            return score;
        }

        private void LogSkip(string reason, string detail) =>
            _monitor.Log($"{LogTag} skip reason={reason}: {detail}", LogLevel.Debug);
    }
}
