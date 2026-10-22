using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Харви берёт хозяйство на себя, пока жена на больничном. Утром, если она замужем за Харви и вчера
    /// попала в клинику / потеряла сознание или у неё тяжёлая травма в лечении, он до работы поливает
    /// грядки (ферма и теплица), кормит животных сеном из силоса и наливает воду питомцу.
    ///
    /// Так травма — не только потерянный день: с ней отдыхают, а ферма не стоит.
    /// Для CP: тема <see cref="ChoresTopic"/> на сегодня (реплики — assets/Code/dialogues/harvey_chores.json).
    /// </summary>
    public sealed class SpouseChoresManager
    {
        public const string ChoresTopic = "HarveyChores_Done";
        private const string LastDayKey = "marilynsinister.HarveyOverhaul/ChoresLastDay";

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;

        public SpouseChoresManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
        }

        public void OnDayStarted()
        {
            try
            {
                if (!_config.EnableSpouseChoresWhileInjured || !Context.IsMainPlayer)
                    return;

                var player = Game1.player;
                if (player?.spouse != "Harvey" || !player.isMarriedOrRoommates() || Utility.isFestivalDay())
                    return;

                string? reason = GetReason();
                if (reason == null)
                    return;

                var done = new List<string>();
                int watered = Game1.IsRainingHere(Game1.getFarm()) ? 0 : WaterCrops(Game1.getFarm());
                if (Game1.getLocationFromName("Greenhouse") is { } greenhouse)
                    watered += WaterCrops(greenhouse);
                if (watered > 0)
                    done.Add("полил грядки");

                if (FeedAnimals() > 0)
                    done.Add("накормил животных");

                if (FillPetBowls() > 0)
                    done.Add("налил воды питомцу");

                if (done.Count == 0)
                    return;

                player.modData[LastDayKey] = Game1.stats.DaysPlayed.ToString();
                _dialogueManager.AddTopic(ChoresTopic, 1);

                string list = done.Count == 1 ? done[0] : string.Join(", ", done.GetRange(0, done.Count - 1)) + " и " + done[^1];
                Game1.addHUDMessage(new HUDMessage($"Харви встал раньше: {list}. Тебе сегодня — отдыхать.", HUDMessage.newQuest_type));
                _monitor.Log($"[Chores] {reason}: {list} (грядок {watered})", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[Chores] OnDayStarted error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Почему сегодня он берёт хозяйство на себя; null — не нужно.</summary>
        private string? GetReason()
        {
            int today = (int)Game1.stats.DaysPlayed;
            var state = _stateManager.State;

            if (state.LastHospitalDischargeDay >= today - 1 && state.LastHospitalDischargeDay >= 0)
                return "после клиники";

            if (state.RecentCollapseDays.Contains(today - 1))
                return "после обморока";

            if (_injuryManager.IsMainInjurySerious() || _injuryManager.HasAnySevereInjuryOrPhase())
                return "тяжёлая травма";

            return null;
        }

        private static int WaterCrops(GameLocation location)
        {
            int count = 0;
            foreach (var feature in location.terrainFeatures.Values)
            {
                if (feature is HoeDirt dirt && Water(dirt))
                    count++;
            }

            foreach (var obj in location.objects.Values)
            {
                if (obj is IndoorPot pot && pot.hoeDirt.Value is { } potDirt && Water(potDirt))
                    count++;
            }

            return count;
        }

        private static bool Water(HoeDirt dirt)
        {
            if (dirt.crop == null || dirt.crop.dead.Value || dirt.state.Value == HoeDirt.watered)
                return false;

            dirt.state.Value = HoeDirt.watered;
            return true;
        }

        /// <summary>Сено из силоса в кормушки — как делает супруг в игре. Без сена в силосе ничего не выйдет.</summary>
        private static int FeedAnimals()
        {
            int houses = 0;
            foreach (var building in Game1.getFarm().buildings)
            {
                if (building.GetIndoors() is AnimalHouse house && house.animalsThatLiveHere.Count > 0)
                {
                    house.feedAllAnimals();
                    houses++;
                }
            }

            return houses;
        }

        private static int FillPetBowls()
        {
            int count = 0;
            foreach (var building in Game1.getFarm().buildings)
            {
                if (building is PetBowl bowl && !bowl.watered.Value)
                {
                    bowl.watered.Value = true;
                    count++;
                }
            }

            return count;
        }
    }
}
