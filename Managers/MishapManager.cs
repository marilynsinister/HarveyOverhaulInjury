using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Мелкие однодневные неприятности: заноза при сборе, укус пчелы, солнечный удар, обморожение.
    /// Без фаз и DebuffState: проходят за ночь или снимаются медицинскими предметами / питьём.
    /// </summary>
    public sealed class MishapManager
    {
        private const int PollIntervalTicks = 60;
        private const int CooldownDays = 3;
        private const double SplinterChance = 0.04;
        private const double BeeStingChancePer10Minutes = 0.015;
        private const int SunstrokeMinutes = 150;
        private const int FrostbiteMinutes = 120;
        private const double ExposureMishapChance = 0.6;
        private const int TopicDays = 2;

        private static readonly HashSet<string> BeeLocations = new(StringComparer.OrdinalIgnoreCase)
        {
            "Farm", "Forest", "Mountain", "Town", "BusStop", "Backwoods",
        };

        private static readonly HashSet<string> HotDrinkIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "(O)395", // кофе
            "(O)253", // тройной эспрессо
            "(O)614", // зелёный чай
            MedicalItems.HerbalTea,
        };

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;

        private uint _lastItemsForaged;

        public MishapManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
        }

        public void OnDayStarted()
        {
            _lastItemsForaged = Game1.stats.ItemsForaged;
        }

        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || e.Ticks % PollIntervalTicks != 0)
                return;

            uint foraged = Game1.stats.ItemsForaged;
            bool foragedNow = foraged > _lastItemsForaged;
            _lastItemsForaged = foraged;

            if (foragedNow && CanHaveMishap() && GameUtils.Roll(ScaleChance(SplinterChance)))
                Apply(MishapBuffs.Splinter, "Ай! В пальце заноза. Нужно вытащить и обработать.");
        }

        public void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !_config.EnableMinorMishaps)
                return;

            var state = _stateManager.State;
            int today = GameUtils.Today();
            if (state.MishapExposureDay != today)
            {
                state.MishapExposureDay = today;
                state.SunExposureMinutesToday = 0;
                state.ColdExposureMinutesToday = 0;
            }

            var location = Game1.currentLocation;
            if (location == null || !location.IsOutdoors || Game1.CurrentEvent != null)
                return;

            int minutes = Math.Max(0, ToMinutes(e.NewTime) - ToMinutes(e.OldTime));
            int time = e.NewTime;
            bool clearSky = !location.IsRainingHere() && !location.IsSnowingHere();

            bool desert = string.Equals(location.Name, "Desert", StringComparison.OrdinalIgnoreCase);
            bool hotSun = clearSky && (desert ? time is >= 1000 and <= 1700 : Game1.IsSummer && time is >= 1100 and <= 1600);
            if (hotSun)
            {
                state.SunExposureMinutesToday += minutes;
                if (state.SunExposureMinutesToday >= SunstrokeMinutes)
                    TryExposureMishap(MishapBuffs.Sunstroke, "Голова кружится от жары. Нужно попить и уйти в тень.", () => state.SunExposureMinutesToday = SunstrokeMinutes / 2);
            }

            bool bitterCold = Game1.IsWinter && (location.IsSnowingHere() || time >= 1800);
            if (bitterCold)
            {
                state.ColdExposureMinutesToday += minutes;
                if (state.ColdExposureMinutesToday >= FrostbiteMinutes)
                    TryExposureMishap(MishapBuffs.Frostbite, "Пальцы онемели от холода. Нужно согреться горячим.", () => state.ColdExposureMinutesToday = FrostbiteMinutes / 2);
            }

            bool beeWeather = (Game1.IsSpring || Game1.IsSummer) && clearSky && time is >= 900 and <= 1700;
            if (beeWeather && BeeLocations.Contains(location.Name) && CanHaveMishap()
                && GameUtils.Roll(ScaleChance(BeeStingChancePer10Minutes)))
            {
                Apply(MishapBuffs.BeeSting, "Ой! Пчела ужалила в руку. Место укуса опухает.");
            }
        }

        /// <summary>Выпито или съедено: вода/напиток снимает солнечный удар, горячее — обморожение.</summary>
        public void OnConsumed(StardewValley.Object item)
        {
            bool isDrink = Game1.objectData.TryGetValue(item.ItemId, out var data) && data.IsDrink;
            if (isDrink && Remove(MishapBuffs.Sunstroke))
                Game1.addHUDMessage(new HUDMessage("Стало прохладнее. Голова больше не кружится.", HUDMessage.health_type));

            bool hot = HotDrinkIds.Contains(item.QualifiedItemId) || item.HasContextTag("hot_drink_item");
            if (hot && Remove(MishapBuffs.Frostbite))
                Game1.addHUDMessage(new HUDMessage("Горячее согрело руки. Пальцы снова слушаются.", HUDMessage.health_type));
        }

        /// <summary>Снять первую из указанных неприятностей (медицинский предмет). Возвращает снятый бафф или null.</summary>
        public string? TryCure(params string[] buffIds)
        {
            foreach (string buffId in buffIds)
            {
                if (Remove(buffId))
                    return buffId;
            }

            return null;
        }

        public bool HasAny(params string[] buffIds) => _buffManager.HasBuff(buffIds);

        private void TryExposureMishap(string buffId, string message, Action resetExposure)
        {
            if (!CanHaveMishap())
                return;

            if (GameUtils.Roll(ScaleChance(ExposureMishapChance)))
                Apply(buffId, message);
            else
                resetExposure();
        }

        private bool CanHaveMishap()
        {
            if (!_config.EnableMinorMishaps || Game1.CurrentEvent != null)
                return false;

            if (_buffManager.HasBuff(StatusBuffs.Hospitalized))
                return false;

            int last = _stateManager.State.LastMishapDay;
            return last < 0 || GameUtils.Today() - last >= CooldownDays;
        }

        private void Apply(string buffId, string message)
        {
            if (!_buffManager.BuffExists(buffId) || _buffManager.HasBuff(buffId))
                return;

            _buffManager.AddBuff(buffId, -2);
            _stateManager.State.LastMishapDay = GameUtils.Today();
            _dialogueManager.AddTopic(GetTopic(buffId), TopicDays);
            _stateManager.Save();
            Game1.playSound("ow");
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
            _monitor.Log($"[Mishap] {buffId}", LogLevel.Info);
        }

        private bool Remove(string buffId)
        {
            if (!_buffManager.HasBuff(buffId))
                return false;

            _buffManager.RemoveBuff(buffId);
            _monitor.Log($"[Mishap] {buffId} снят", LogLevel.Debug);
            return true;
        }

        private double ScaleChance(double chance) =>
            Math.Clamp(chance * _config.GetInjuryChanceMultiplier(), 0.0, 1.0);

        private static string GetTopic(string buffId) => "topicHarvey_" + buffId.Replace("HarveyMod_", "");

        private static int ToMinutes(int time) => time / 100 * 60 + time % 100;
    }
}
