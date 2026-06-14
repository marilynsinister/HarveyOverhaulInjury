using System;
using System.Linq;
using System.Text;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Домашняя married/engaged забота Харви на ферме и в FarmHouse.
    /// Не лечение, не стресс-квест, не recovery plan — только бытовая романтика супруга.
    /// </summary>
    public sealed class DomesticSpouseCareManager
    {
        private const string LogTag = "[DomesticCare]";
        private const string SaveKey = "domestic_spouse_state";

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;
        private readonly InjuryManager _injuryManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private IHarveyCoreApi? _coreApi;

        private DomesticSpouseState _state = new();

        public DomesticSpouseCareManager(
            IMonitor monitor,
            IModHelper helper,
            ModConfig config,
            StateManager stateManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager,
            HospitalizationManager hospitalizationManager)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
            _injuryManager = injuryManager;
            _hospitalizationManager = hospitalizationManager;
        }

        public void SetCoreApi(IHarveyCoreApi? coreApi)
            => _coreApi = coreApi;

        public void Load()
        {
            _state = _helper.Data.ReadSaveData<DomesticSpouseState>(SaveKey) ?? new DomesticSpouseState();
            _monitor.Log($"{LogTag} Loaded state", LogLevel.Debug);
        }

        private void Save()
        {
            _helper.Data.WriteSaveData(SaveKey, _state);
        }

        public void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            try
            {
                Load();
                _state.DomesticReactionsShownToday = 0;
                Save();
                _monitor.Log($"{LogTag} DayStarted reset", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"{LogTag} OnDayStarted error: {ex}", LogLevel.Error);
            }
        }

        public void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            try
            {
                var ctx = BuildContext();

                if (!CanShowDomesticLine(ctx))
                    return;

                if (!ctx.IsFarmHouse)
                    return;

                int today = GameUtils.Today();

                if (ctx.TimeBucket is DomesticTimeBucket.EarlyMorning or DomesticTimeBucket.Morning)
                {
                    if (_state.LastMorningLineDay == today)
                        return;

                    if (!GameUtils.Roll(Math.Clamp(_config.MorningSpouseLineChance, 0.0, 1.0)))
                        return;

                    string prefix = PickDomesticPrefix(ctx);
                    TryShowHomeLine(ctx, prefix);

                    _state.LastMorningLineDay = today;
                    Save();
                    return;
                }

                if (ctx.TimeBucket == DomesticTimeBucket.Evening)
                {
                    if (_state.LastEveningLineDay == today)
                        return;

                    if (!GameUtils.Roll(Math.Clamp(_config.EveningSpouseLineChance, 0.0, 1.0)))
                        return;

                    string prefix = PickDomesticPrefix(ctx);
                    TryShowHomeLine(ctx, prefix);

                    _state.LastEveningLineDay = today;
                    Save();
                    return;
                }

                if (ctx.TimeBucket == DomesticTimeBucket.LateNight)
                {
                    if (_state.LastLateNightLineDay == today)
                        return;

                    if (!GameUtils.Roll(Math.Clamp(_config.LateNightSpouseLineChance, 0.0, 1.0)))
                        return;

                    string prefix = PickDomesticPrefix(ctx);
                    TryShowHomeLine(ctx, prefix);

                    _state.LastLateNightLineDay = today;
                    Save();
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"{LogTag} OnTimeChanged error: {ex}", LogLevel.Error);
            }
        }

        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(600))
                return;

            try
            {
                var ctx = BuildContext();

                if (!CanShowDomesticLine(ctx))
                    return;

                if (!ctx.IsFarm)
                    return;

                if (ctx.TimeBucket != DomesticTimeBucket.Day
                    && ctx.TimeBucket != DomesticTimeBucket.Morning
                    && ctx.TimeBucket != DomesticTimeBucket.Evening)
                {
                    return;
                }

                string prefix = PickDomesticPrefix(ctx);
                TryShowFarmProximityLine(ctx, prefix);
            }
            catch (Exception ex)
            {
                _monitor.Log($"{LogTag} OnUpdateTicked error: {ex}", LogLevel.Error);
            }
        }

        public void ResetState()
        {
            _state = new DomesticSpouseState();
            Save();
            _monitor.Log($"{LogTag} state reset", LogLevel.Info);
        }

        public string BuildDebugStatusReport()
        {
            var ctx = BuildContext();
            string prefix = PickDomesticPrefix(ctx);
            var sb = new StringBuilder();
            sb.AppendLine($"{LogTag} status");
            sb.AppendLine($"location: {Game1.currentLocation?.NameOrUniqueName ?? "(null)"}");
            sb.AppendLine($"timeBucket: {ctx.TimeBucket}");
            sb.AppendLine($"isMarried: {ctx.IsMarriedToHarvey}");
            sb.AppendLine($"isEngagedOrDating: {ctx.IsEngagedOrDatingHarvey}");
            sb.AppendLine($"lowHealth: {ctx.LowHealth}");
            sb.AppendLine($"lowStamina: {ctx.LowStamina}");
            sb.AppendLine($"hasInjury: {ctx.HasAnyInjury} (severe={ctx.HasSevereInjury})");
            sb.AppendLine($"hasStress: {ctx.HasStress}");
            sb.AppendLine($"weather: rain={ctx.IsRaining} storm={ctx.IsStorm} snow={ctx.IsSnowing}");
            sb.AppendLine($"season: {ctx.Season} day: {ctx.DayOfWeek}");
            sb.AppendLine($"festivalToday: {ctx.IsFestivalToday} festivalTomorrow: {ctx.IsFestivalTomorrow}");
            sb.AppendLine($"pickedPrefix: {prefix}");
            sb.AppendLine($"reactionsToday: {_state.DomesticReactionsShownToday}/{_config.MaxDomesticReactionsPerDay}");
            sb.AppendLine($"lastPrefix: {_state.LastDomesticPrefix}");
            sb.AppendLine($"lastLine: {_state.LastDomesticLine}");
            sb.AppendLine($"canShow: {CanShowDomesticLine(ctx)}");
            return sb.ToString();
        }

        public void TestPrefix(string prefix, bool mutateState = false)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                _monitor.Log($"{LogTag} test: prefix required", LogLevel.Error);
                return;
            }

            var ctx = BuildContext();
            NPC? harvey = HarveyHelper.FindHarvey(Game1.currentLocation);
            if (harvey == null)
            {
                _monitor.Log($"{LogTag} test: Harvey not in current location", LogLevel.Warn);
                return;
            }

            string line = PickLineWithFallback(prefix, ctx);
            if (string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"{LogTag} test: no line for prefix {prefix}", LogLevel.Warn);
                return;
            }

            if (ctx.IsFarm && _config.EnableSpouseProximityLines)
            {
                _dialogueManager.ShowEmoteWithText(harvey, Emotes.Heart, line, 3500);
            }
            else
            {
                _dialogueManager.Speak(harvey, line);
            }

            _monitor.Log($"{LogTag} test shown: {prefix} -> {line}", LogLevel.Info);

            if (mutateState)
            {
                _state.LastDomesticPrefix = prefix;
                _state.LastDomesticLine = line;
                Save();
            }
        }

        private sealed class DomesticContext
        {
            public bool IsFarmHouse { get; set; }
            public bool IsFarm { get; set; }
            public DomesticTimeBucket TimeBucket { get; set; }
            public bool IsMarriedToHarvey { get; set; }
            public bool IsEngagedOrDatingHarvey { get; set; }
            public bool HasSevereInjury { get; set; }
            public bool HasAnyInjury { get; set; }
            public bool HasStress { get; set; }
            public bool LowHealth { get; set; }
            public bool LowStamina { get; set; }
            public bool IsRaining { get; set; }
            public bool IsStorm { get; set; }
            public bool IsSnowing { get; set; }
            public string Season { get; set; } = "";
            public string DayOfWeek { get; set; } = "";
            public bool IsFestivalToday { get; set; }
            public bool IsFestivalTomorrow { get; set; }
            public bool HasRecoveryPlanPerfectTopic { get; set; }
            public bool HasRecoveryPlanViolationTopic { get; set; }
            public bool HasAfterClinicTopic { get; set; }
            public bool HasAfterMineTopic { get; set; }
            public bool HasThunderFearTopic { get; set; }
            public bool HasDarknessFearTopic { get; set; }
            public bool HasSocialAnxietyTopic { get; set; }
        }

        private DomesticContext BuildContext()
        {
            var loc = Game1.currentLocation;
            var player = Game1.player;

            var ctx = new DomesticContext
            {
                IsFarmHouse = loc is StardewValley.Locations.FarmHouse,
                IsFarm = string.Equals(loc?.NameOrUniqueName, "Farm", StringComparison.OrdinalIgnoreCase),
                TimeBucket = GetTimeBucket(Game1.timeOfDay),
                IsMarriedToHarvey = IsMarriedToHarvey(),
                IsEngagedOrDatingHarvey = IsDatingEngagedOrMarriedToHarvey(),
                IsRaining = Game1.isRaining,
                IsStorm = Game1.isLightning,
                IsSnowing = Game1.isSnowing,
                Season = Game1.currentSeason ?? "",
                DayOfWeek = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth),
                LowHealth = player.health > 0 && player.health <= player.maxHealth * 0.35,
                LowStamina = player.Stamina <= player.MaxStamina * 0.25f,
                HasSevereInjury = HasSevereInjury(),
                HasAnyInjury = HasAnyInjury(),
                HasStress = HasStressState(),
                IsFestivalToday = Utility.isFestivalDay(Game1.dayOfMonth, Game1.season),
                IsFestivalTomorrow = IsFestivalTomorrow(),
                HasRecoveryPlanPerfectTopic = HasTopic(ConversationTopics.RecoveryPlanPerfect)
                    || HasTopic("topicHarvey_RecoveryPlanPerfect"),
                HasRecoveryPlanViolationTopic = HasTopic(ConversationTopics.RecoveryPlanViolated)
                    || HasTopic("topicHarvey_RecoveryPlanViolated")
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedMine)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedLowStamina)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedLowHealth)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedLateNight)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedRain)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedMild)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedMedium)
                    || HasTopic(ConversationTopics.RecoveryPlanViolatedSevere),
                HasAfterClinicTopic = HasTopic(ConversationTopics.TreatmentCompleted)
                    || HasTopic("topicHarvey_AfterClinic")
                    || HasTopic(ConversationTopics.NightRound)
                    || HasTopic(ConversationTopics.NightRoundFollowup),
                HasAfterMineTopic = HasTopic(ConversationTopics.MineInjuryRescue)
                    || HasTopic("topicHarvey_MineDeathRescue"),
                HasThunderFearTopic = HasTopic("HarveyStress_Thunder")
                    || HasTopic("topicHarvey_Thunder")
                    || HasTopic("topicThunder")
                    || HasTopic(StormComfortIds.LegacyStressTopic),
                HasDarknessFearTopic = HasTopic("HarveyStress_Darkness")
                    || HasTopic("topicHarvey_Darkness")
                    || HasTopic("topicDarkness"),
                HasSocialAnxietyTopic = HasTopic("HarveyStress_SocialAnxiety")
                    || HasTopic("topicHarvey_SocialAnxiety")
                    || HasTopic("topicSocialAnxiety"),
            };

            return ctx;
        }

        private bool CanShowDomesticLine(DomesticContext ctx)
        {
            if (!_config.EnableSpouseDomesticCare)
            {
                LogSkip("config disabled");
                return false;
            }

            if (!Context.IsWorldReady || !Context.IsPlayerFree)
            {
                LogSkip("world/player not ready");
                return false;
            }

            if (Game1.activeClickableMenu != null || Game1.eventUp || Game1.dialogueUp)
                return false;

            if (!ctx.IsFarmHouse && !ctx.IsFarm)
            {
                LogSkip("not FarmHouse/Farm");
                return false;
            }

            if (!HasEligibleSpouseRelationship(ctx))
            {
                LogSkip("not married/engaged");
                return false;
            }

            if (_state.DomesticReactionsShownToday >= Math.Max(0, _config.MaxDomesticReactionsPerDay))
            {
                LogSkip("daily cap");
                return false;
            }

            if (IsCriticalMedicalBlocking(ctx))
            {
                LogSkip("critical medical context");
                return false;
            }

            if (ctx.HasAfterMineTopic && ctx.HasSevereInjury)
            {
                LogSkip("mine rescue + severe injury");
                return false;
            }

            return true;
        }

        private bool HasEligibleSpouseRelationship(DomesticContext ctx)
        {
            if (ctx.IsMarriedToHarvey)
                return true;

            if (!_config.AllowDomesticCareWhenEngaged)
                return false;

            var friendship = Game1.player?.friendshipData;
            if (friendship == null || !friendship.TryGetValue("Harvey", out var data))
                return false;

            return data.IsEngaged();
        }

        private bool IsCriticalMedicalBlocking(DomesticContext ctx)
        {
            if (_coreApi?.HasPriorityHarveyInteraction() == true)
                return true;

            if (_hospitalizationManager.IsHospitalized)
                return true;

            if (Game1.CurrentEvent != null)
                return true;

            var injuryState = _stateManager.State;
            int today = GameUtils.Today();

            if (injuryState.NeedsHarveyAfterExternalRescueHomeEvent
                && injuryState.HarveyAfterExternalRescueShownDay != today)
            {
                return true;
            }

            if (injuryState.NeedsHarveyAfterHospitalDischargeHomeEvent
                && injuryState.HarveyAfterHospitalDischargeShownDay != today)
            {
                return true;
            }

            if (injuryState.NeedsSevereNightRoundEvent
                && injuryState.SevereNightRoundEventShownDay != today)
            {
                return true;
            }

            if (injuryState.NeedsHarveyMorningAfterExhaustionEvent
                && injuryState.HarveyMorningAfterExhaustionShownDay != today)
            {
                return true;
            }

            if (HasActiveMedicalReviewPriority(injuryState))
                return true;

            return false;
        }

        /// <summary>
        /// Блокирует бытовые реплики, когда важнее медицинский осмотр или recovery plan.
        /// Stress/Injury priority приходит через HarveyOverhaul.Core API.
        /// </summary>
        private bool HasActiveMedicalReviewPriority(InjuryState injuryState)
        {
            if (injuryState.RecoveryPlanNeedsHarveyVisit)
                return true;

            var dailyPlan = injuryState.RecoveryPlan;
            if (dailyPlan.IsActive && (dailyPlan.NeedsHarveyVisit || dailyPlan.TodayFailed))
                return true;

            if (injuryState.ActiveComplications.Count > 0)
                return true;

            foreach (var (buffId, debuff) in injuryState.ActiveDebuffs)
            {
                if (debuff == null)
                    continue;

                if (debuff.ReadyForNextPhase || debuff.ReadyForRecovery)
                    return true;

                if (!debuff.TreatmentStarted && InjurySets.HarveyTreatable.Contains(buffId))
                    return true;
            }

            return false;
        }

        private string PickDomesticPrefix(DomesticContext ctx)
        {
            string time = ctx.TimeBucket switch
            {
                DomesticTimeBucket.EarlyMorning => "EarlyMorning",
                DomesticTimeBucket.Morning => "Morning",
                DomesticTimeBucket.Day => "Day",
                DomesticTimeBucket.Evening => "Evening",
                DomesticTimeBucket.LateNight => "LateNight",
                _ => "",
            };

            if (string.IsNullOrEmpty(time))
                return "";

            string basePrefix = ctx.IsFarm
                ? $"HarveyMod_Spouse_Farm_{time}"
                : $"HarveyMod_Spouse_{time}";

            string prefix;

            if (ctx.HasSevereInjury || ctx.HasAnyInjury)
                prefix = $"{basePrefix}_Injured";
            else if (ctx.HasStress)
                prefix = $"{basePrefix}_Stress";
            else if (ctx.LowHealth)
                prefix = $"{basePrefix}_LowHealth";
            else if (ctx.LowStamina)
                prefix = $"{basePrefix}_LowStamina";
            else if (ctx.HasRecoveryPlanPerfectTopic)
                prefix = $"{basePrefix}_AfterPerfectPlan";
            else if (ctx.HasRecoveryPlanViolationTopic)
                prefix = $"{basePrefix}_AfterViolation";
            else if (ctx.HasAfterClinicTopic)
                prefix = $"{basePrefix}_AfterClinic";
            else if (ctx.IsStorm)
                prefix = $"{basePrefix}_Storm";
            else if (ctx.IsRaining)
                prefix = $"{basePrefix}_Rain";
            else if (ctx.IsSnowing)
                prefix = $"{basePrefix}_Snow";
            else if (ctx.IsFestivalToday)
                prefix = $"{basePrefix}_Festival";
            else if (ctx.IsFestivalTomorrow)
                prefix = $"{basePrefix}_FestivalEve";
            else if (!string.IsNullOrEmpty(ctx.Season) && ctx.IsFarm)
                prefix = $"{basePrefix}_{ToPascalSeason(ctx.Season)}";
            else
                prefix = $"{basePrefix}_Default";

            _monitor.Log($"{LogTag} Pick prefix: {prefix}", LogLevel.Debug);
            return prefix;
        }

        private string PickLineWithFallback(string prefix, DomesticContext ctx)
        {
            string line = _dialogueManager.PickRandomDialogueByPrefix(prefix, "");
            if (!string.IsNullOrWhiteSpace(line))
                return line;

            string time = ctx.TimeBucket switch
            {
                DomesticTimeBucket.EarlyMorning => "EarlyMorning",
                DomesticTimeBucket.Morning => "Morning",
                DomesticTimeBucket.Day => "Day",
                DomesticTimeBucket.Evening => "Evening",
                DomesticTimeBucket.LateNight => "LateNight",
                _ => "Default",
            };

            if (!ctx.IsFarm)
            {
                line = _dialogueManager.PickRandomDialogueByPrefix($"HarveyMod_Spouse_{time}_Default", "");
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _monitor.Log($"{LogTag} Fallback prefix used: HarveyMod_Spouse_{time}_Default", LogLevel.Debug);
                    return line;
                }
            }

            line = _dialogueManager.PickRandomDialogueByPrefix("HarveyMod_Spouse_Default", "");
            if (!string.IsNullOrWhiteSpace(line))
                _monitor.Log($"{LogTag} Fallback prefix used: HarveyMod_Spouse_Default", LogLevel.Debug);

            return line ?? "";
        }

        private void TryShowHomeLine(DomesticContext ctx, string prefix)
        {
            NPC? harvey = HarveyHelper.FindHarvey(Game1.currentLocation);
            if (harvey == null)
                return;

            string line = PickLineWithFallback(prefix, ctx);
            if (string.IsNullOrWhiteSpace(line))
                return;

            _dialogueManager.Speak(harvey, line);
            Game1.player.changeFriendship(5, harvey);

            _state.DomesticReactionsShownToday++;
            _state.LastDomesticPrefix = prefix;
            _state.LastDomesticLine = line;
            Save();

            _monitor.Log($"{LogTag} Show home line: {prefix}", LogLevel.Info);
        }

        private void TryShowFarmProximityLine(DomesticContext ctx, string prefix)
        {
            if (!_config.EnableSpouseProximityLines)
                return;

            NPC? harvey = HarveyHelper.FindHarvey(Game1.currentLocation);
            if (harvey == null)
                return;

            if (!HarveyHelper.IsNearPlayer(harvey, _config.DomesticProximityTiles))
                return;

            int today = GameUtils.Today();
            int nowMinutes = GameUtils.CurrentTimeInMinutes();

            if (_state.LastProximityLineDay == today
                && nowMinutes - _state.LastProximityGameMinutes < _config.SpouseProximityCooldownMinutes)
            {
                return;
            }

            if (!GameUtils.Roll(Math.Clamp(_config.FarmProximityLineChance, 0.0, 1.0)))
                return;

            string line = PickLineWithFallback(prefix, ctx);
            if (string.IsNullOrWhiteSpace(line))
                return;

            _dialogueManager.ShowEmoteWithText(harvey, Emotes.Heart, line, 3500);

            _state.LastProximityLineDay = today;
            _state.LastProximityGameMinutes = nowMinutes;
            _state.DomesticReactionsShownToday++;
            _state.LastDomesticPrefix = prefix;
            _state.LastDomesticLine = line;
            Save();

            _monitor.Log($"{LogTag} Show proximity line: {prefix}", LogLevel.Info);
        }

        private static DomesticTimeBucket GetTimeBucket(int time)
        {
            if (time >= 600 && time < 800)
                return DomesticTimeBucket.EarlyMorning;

            if (time >= 800 && time < 1100)
                return DomesticTimeBucket.Morning;

            if (time >= 1100 && time < 1700)
                return DomesticTimeBucket.Day;

            if (time >= 1700 && time < 2200)
                return DomesticTimeBucket.Evening;

            if (time >= 2200 && time <= 2600)
                return DomesticTimeBucket.LateNight;

            return DomesticTimeBucket.None;
        }

        private static string ToPascalSeason(string season) =>
            season.ToLowerInvariant() switch
            {
                "spring" => "Spring",
                "summer" => "Summer",
                "fall" => "Fall",
                "winter" => "Winter",
                _ => "Default",
            };

        private bool IsMarriedToHarvey()
        {
            if (Game1.player == null)
                return false;

            if (string.Equals(Game1.player.spouse, "Harvey", StringComparison.OrdinalIgnoreCase))
                return true;

            if (Game1.player.friendshipData != null
                && Game1.player.friendshipData.TryGetValue("Harvey", out var data))
            {
                return data.IsMarried();
            }

            return false;
        }

        private bool IsDatingEngagedOrMarriedToHarvey()
        {
            if (IsMarriedToHarvey())
                return true;

            if (Game1.player?.friendshipData != null
                && Game1.player.friendshipData.TryGetValue("Harvey", out var data))
            {
                return data.IsDating() || data.IsEngaged();
            }

            return false;
        }

        private bool HasTopic(string id) =>
            Game1.player?.activeDialogueEvents?.ContainsKey(id) == true;

        private bool HasAnyBuff(params string[] ids) =>
            ids.Any(id => Game1.player.hasBuff(id));

        private bool HasSevereInjury()
        {
            foreach (string injury in InjurySets.Severe)
            {
                if (Game1.player.hasBuff(injury))
                    return true;

                if (_injuryManager.HasInjuryOrPhase(injury))
                    return true;
            }

            return false;
        }

        private bool HasAnyInjury()
        {
            if (_stateManager.GetAllActiveDebuffStates().Count > 0)
                return true;

            if (_stateManager.State.ActiveComplications.Count > 0)
                return true;

            return false;
        }

        private bool HasStressState()
        {
            string[] stressBuffs =
            {
                "HarveyStress_NoSleep",
                "HarveyStress_Hunger",
                "HarveyStress_TooCold",
                "HarveyStress_Thunder",
                "HarveyStress_Darkness",
                "HarveyStress_Overwork",
                "HarveyStress_Tired",
                "HarveyStress_Lonely",
                "HarveyStress_SocialAnxiety",
                "buffStressNoSleep",
                "buffStressHunger",
                "buffStressTooCold",
                "buffStressThunder",
                "buffStressDarkness",
                "buffStressOverwork",
                "buffStressTired",
                "buffStressLonely",
                "buffStressSocialAnxiety",
                "buffNoSleep",
                "buffHunger",
                "buffTooCold",
                "buffThunder",
                "buffDarkness",
                "buffOverwork",
                "buffTired",
                "buffLonely",
                "buffSocialAnxiety",
            };

            if (HasAnyBuff(stressBuffs))
                return true;

            string[] stressTopics =
            {
                "topicHarvey_Thunder",
                "topicHarvey_Darkness",
                "topicHarvey_SocialAnxiety",
                "topicThunder",
                "topicDarkness",
                "topicSocialAnxiety",
            };

            foreach (string topic in stressTopics)
            {
                if (HasTopic(topic))
                    return true;
            }

            return false;
        }

        private static bool IsFestivalTomorrow()
        {
            int day = Game1.dayOfMonth + 1;
            Season season = Game1.season;
            if (day > 28)
            {
                day = 1;
                season = (Season)(((int)season + 1) % 4);
            }

            return Utility.isFestivalDay(day, season);
        }

        private void LogSkip(string reason) =>
            _monitor.Log($"{LogTag} Skip: {reason}", LogLevel.Trace);
    }

    public enum DomesticTimeBucket
    {
        None,
        EarlyMorning,
        Morning,
        Day,
        Evening,
        LateNight,
    }
}
