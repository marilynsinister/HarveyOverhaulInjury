using System;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using HarveyOverhaul.InjuryCare.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик обморока и истощения игрока
    /// </summary>
    public class PassOutHandler
    {
        private const string FallbackCritical = "critical";
        private const string FallbackExhaustion = "exhaustion";

        private const int CollapseWindowDays = 14;
        private const int FriendCollapseMinPoints = 1500; // 6 сердец
        private const float LowStaminaNudgeFraction = 0.1f;
        private const int LateNightNudgeTime = 2500;
        private const string MailFriendCollapse = "mailHarveyFriendCollapse";
        private const string MailCollapsePattern = "mailHarveyCollapsePattern";

        private int _lowStaminaNudgeDay = -1;
        private int _lateNightNudgeDay = -1;

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly InjuryManager _injuryManager;
        private readonly TreatmentManager _treatmentManager;
        private TreatmentStartHandler? _treatmentStartHandler;
        private RecoveryPlanManager? _recoveryPlanManager;
        private MedicalLetterScheduler? _medicalLetterScheduler;
        private HospitalizationManager? _hospitalizationManager;

        public PassOutHandler(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager,
            TreatmentManager treatmentManager)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _injuryManager = injuryManager;
            _treatmentManager = treatmentManager;
        }

        public void SetRecoveryPlanManager(RecoveryPlanManager recoveryPlanManager)
        {
            _recoveryPlanManager = recoveryPlanManager;
        }

        public void SetTreatmentStartHandler(TreatmentStartHandler treatmentStartHandler)
        {
            _treatmentStartHandler = treatmentStartHandler;
        }

        public void SetHospitalizationManager(HospitalizationManager hospitalizationManager)
        {
            _hospitalizationManager = hospitalizationManager;
        }

        public void SetMedicalLetterScheduler(MedicalLetterScheduler medicalLetterScheduler)
        {
            _medicalLetterScheduler = medicalLetterScheduler;
        }

        /// <summary>
        /// Обработать телепортацию после обморока
        /// </summary>
        public void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            // === Minor mine rescue (опасное состояние без Severe) ===
            string pendingMinorId = _stateManager.State.PendingMinorMineRescueEventId;
            if (!string.IsNullOrEmpty(pendingMinorId) &&
                string.Equals(e.NewLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log($"[MinorMineRescue] Игрок в Mine — запускаем: {pendingMinorId}", LogLevel.Info);
                _stateManager.State.PendingMinorMineRescueEventId = "";

                if (!TryStartLocationEvent(pendingMinorId, "Mine", OnMinorMineRescueEventFinished))
                    RunMinorMineRescueFallback();

                return;
            }

            // === Запуск отложенного утреннего rescue у кровати (FarmHouse) ===
            string pendingMineId = _stateManager.State.PendingMineRescueEventId;
            if (!string.IsNullOrEmpty(pendingMineId) &&
                IsFarmHouseLocation(e.NewLocation))
            {
                string farmHouseName = e.NewLocation!.NameOrUniqueName;
                _monitor.Log($"[MineRescue] Игрок дома — запускаем утреннее событие: {pendingMineId}", LogLevel.Info);
                if (TryStartFarmhouseMineRescueEvent(pendingMineId, farmHouseName))
                    return;

                RunMineRescueFallback(pendingMineId);
                return;
            }

            // === Запуск отложенного hospital pass-out cutscene ===
            string pendingHospitalId = _stateManager.State.PendingHospitalPassOutEventId;
            if (!string.IsNullOrEmpty(pendingHospitalId) &&
                IsHospitalLocation(e.NewLocation))
            {
                _monitor.Log($"[PassOutEvent] Игрок в Hospital — запускаем: {pendingHospitalId}", LogLevel.Info);
                if (TryStartLocationEvent(pendingHospitalId, GetHospitalLocationName(), OnHospitalPassOutEventFinished))
                    return;

                RunHospitalPassOutFallback(_stateManager.State.PendingHospitalPassOutFallbackKind);
                ClearHospitalPassOutPending();
                ClearPassOutFlags();
            }
        }

        /// <summary>
        /// Утро после ночного обморока (истощение / 2:00). Вызывается из DayStarted, а не из Warped:
        /// игра ставит фермера в кровать прямым присваиванием локации, и если он отключился дома,
        /// Warped не приходит вовсе — флаги висели до первого выхода из дома.
        /// Боевая гибель сюда не попадает: её обрабатывает <see cref="ProcessDeathRevive"/> в тот же день.
        /// </summary>
        public void ProcessOvernightPassOut()
        {
            var state = _stateManager.State;
            if (!Context.IsWorldReady || !state.WasPassedOut) return;

            if (!state.WasExhausted && !state.WasUpTooLate)
            {
                // Флаг остался от боевой гибели (нужен был только для нарушения плана в DayEnding).
                ClearPassOutFlags();
                return;
            }

            // Госпитализированного (или только что выписанного) пациента ведёт HospitalizationManager.
            if (_hospitalizationManager?.IsHospitalized == true || state.DischargedToday)
            {
                _monitor.Log("[PassOut] Утро после обморока: пациент госпитализирован/выписан — cutscene пропущен", LogLevel.Info);
                ClearPassOutFlags();
                return;
            }

            // Обморок случился вчера (DayEnding), обрабатываем утром.
            RegisterCollapse(Helpers.GameUtils.Today() - 1, state.WasExhausted ? "обморок от истощения" : "обморок в 2:00");

            _monitor.Log(
                $"🏥 Утро после ночного обморока: loc={state.LastPassedOutLocation}, health={state.LastPassedOutHealth}, " +
                $"exhausted={state.WasExhausted}, upTooLate={state.WasUpTooLate}",
                LogLevel.Info);

            bool isDatingOrMarried = _dialogueManager.IsDatingOrMarriedToHarvey();
            bool isMinePassOut = IsMineRelatedPassOut();

            // === 1. Критический обморок (почти без здоровья) вне шахты → eventHarveyEmergencyCare ===
            if (isDatingOrMarried &&
                !isMinePassOut &&
                state.LastPassedOutHealth >= 0 &&
                state.LastPassedOutHealth <= 10)
            {
                _monitor.Log($"⚠️ Критический pass-out вне шахты (здоровье было {state.LastPassedOutHealth})", LogLevel.Warn);
                _injuryManager.ApplyBadlyHurtSafe();
                MineForbiddenHelper.ApplyHardMineForbidden(
                    state, _config, _buffManager, _stateManager, _monitor,
                    (int)Game1.stats.DaysPlayed, "critical_passout");

                if (QueueHospitalEvent(EventIds.EmergencyCare, FallbackCritical))
                {
                    ClearPassOutFlags();
                    return;
                }

                RunCriticalPassOutFallback();
            }
            // === 2. Истощение: Dating+ → eventHarveyExhaustion в клинике; остальные — бафф + топик ===
            else if (state.WasExhausted &&
                     !Helpers.GameUtils.HasConversationTopic(ConversationTopics.FarmerExhausted) &&
                     !Game1.player.hasBuff("buffFarmerExhausted"))
            {
                if (isDatingOrMarried && !isMinePassOut)
                {
                    _monitor.Log("💤 Pass-out от истощения — пробуем hospital cutscene", LogLevel.Info);

                    if (QueueHospitalEvent(EventIds.Exhaustion, FallbackExhaustion))
                    {
                        MarkHarveyMorningAfterExhaustionIfEligible();
                        ClearPassOutFlags();
                        return;
                    }
                }
                else
                {
                    _monitor.Log(
                        isMinePassOut
                            ? "💤 Истощение в шахте — hospital cutscene пропущен"
                            : "💤 Истощение без романа с Харви — бафф и топик без cutscene",
                        LogLevel.Info);
                }

                RunExhaustionPassOutFallback();
            }
            // === 3. Обморок в городе из-за позднего времени ===
            else if (state.WasUpTooLate &&
                     _stateManager.State.LastPassedOutLocation.Contains("Town", StringComparison.OrdinalIgnoreCase) &&
                     !Helpers.GameUtils.HasConversationTopic(ConversationTopics.PassedOutInTown) &&
                     !Game1.player.hasBuff("buffSleepy"))
            {
                _monitor.Log("🌙 Триггер: Обморок в городе из-за позднего времени", LogLevel.Info);

                _buffManager.AddBuff("buffSleepy", -2);
                _dialogueManager.AddTopic(ConversationTopics.PassedOutInTown, 2);

                if (_medicalLetterScheduler != null)
                {
                    _medicalLetterScheduler.TryQueueTieredMail(
                        MailIds.SleepControl,
                        MedicalLetterReasons.SleepControl,
                        critical: true);
                }

                Game1.playSound("debuffHit");
                Game1.addHUDMessage(new HUDMessage("Ты упала без сил посреди города...", HUDMessage.health_type));
            }
            // === 4. Обморок в 2:00 на улице вне города (лес, пляж, гора, ферма, локации модов) ===
            // Только под открытым небом: сарай, теплица, салун, дома NPC сюда не относятся.
            else if (state.WasUpTooLate &&
                     !isMinePassOut &&
                     !IsHomeLocationName(state.LastPassedOutLocation) &&
                     IsOutdoorLocationName(state.LastPassedOutLocation) &&
                     !state.LastPassedOutLocation.Contains("Town", StringComparison.OrdinalIgnoreCase) &&
                     !Helpers.GameUtils.HasConversationTopic(ConversationTopics.PassedOutOutside) &&
                     !Game1.player.hasBuff("buffSleepy"))
            {
                _monitor.Log($"🌙 Триггер: обморок в 2:00 вне дома ({state.LastPassedOutLocation})", LogLevel.Info);

                _buffManager.AddBuff("buffSleepy", -2);
                _dialogueManager.AddTopic(ConversationTopics.PassedOutOutside, 2);

                Game1.playSound("debuffHit");
                Game1.addHUDMessage(new HUDMessage("Ты уснула прямо под открытым небом...", HUDMessage.health_type));
            }

            ClearPassOutFlags();
        }

        /// <summary>
        /// Каждый тик: фиксирует боевую гибель (health &lt;= 0) в ЛЮБОЙ локации, ждёт ванильного возрождения
        /// (Data/LocationContexts → ReviveLocations: шахты 1–120 → Mine, Пещера Черепа/город/ферма → Hospital,
        /// вулкан → IslandSouth, моды → свои клиники) и после сцены PlayerKilled запускает реакцию Харви.
        /// Ванильная гибель НЕ заканчивает день и восстанавливает 10 HP, поэтому проверка в DayEnding её не видит.
        /// </summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            var state = _stateManager.State;

            // Нельзя throttling-ить: health <= 0 держится лишь до ванильного warp.
            if (Game1.player.health <= 0)
            {
                if (!state.PendingDeathRevive)
                    RecordCombatDeath(Game1.currentLocation);
                return;
            }

            if (state.PendingDeathRevive)
            {
                if (Game1.killScreen)
                    return;

                // killScreen снят и HP восстановлено → фермер уже в локации возрождения.
                if (string.IsNullOrEmpty(state.PendingDeathReviveLocation))
                {
                    state.PendingDeathReviveLocation = Game1.currentLocation?.NameOrUniqueName ?? "";
                    _stateManager.Save();
                    _monitor.Log(
                        $"[Death] Возрождение: {state.PendingDeathLocation} → {state.PendingDeathReviveLocation}",
                        LogLevel.Info);
                }

                // Ждём окончания ванильной сцены PlayerKilled.
                if (Game1.eventUp || !Context.IsPlayerFree)
                    return;

                ProcessDeathRevive();
                return;
            }

            if (!e.IsMultipleOf(60) || !Context.IsPlayerFree)
                return;

            if (CanTriggerMinorMineRescue())
                TryTriggerMinorMineRescue();

            TryPreventionNudges();
        }

        /// <summary>
        /// Профилактика (Dating+): одно мягкое напоминание за день при почти пустой энергии и одно — в час ночи вне дома.
        /// </summary>
        private void TryPreventionNudges()
        {
            if (Game1.eventUp || !_dialogueManager.IsDatingEngagedOrMarriedToHarvey())
                return;

            if (_hospitalizationManager?.IsHospitalized == true)
                return;

            int today = Helpers.GameUtils.Today();
            var player = Game1.player;

            if (_lowStaminaNudgeDay != today && player.stamina <= player.MaxStamina * LowStaminaNudgeFraction)
            {
                _lowStaminaNudgeDay = today;
                Game1.addHUDMessage(new HUDMessage(
                    "В голове звучит голос Харви: «Остановись. Вода, еда, присесть — потом всё остальное».",
                    HUDMessage.stamina_type));
            }

            if (_lateNightNudgeDay != today
                && Game1.timeOfDay >= LateNightNudgeTime
                && Game1.timeOfDay < 2600
                && !IsHomeLocationName(Game1.currentLocation?.NameOrUniqueName)
                // Харви рядом — «он ждёт дома» звучит нелепо; напомним, когда она уйдёт.
                && (Game1.currentLocation == null || HarveyHelper.FindHarveyInLocation(Game1.currentLocation) == null))
            {
                _lateNightNudgeDay = today;
                Game1.addHUDMessage(new HUDMessage(
                    "Уже час ночи. Харви наверняка не спит и ждёт: «Домой, солнышко. Пожалуйста».",
                    HUDMessage.newQuest_type));
            }
        }

        /// <summary>
        /// Учёт гибелей и обмороков за 14 дней: второй раз — отдельная реплика, третий — серьёзный разговор и письмо.
        /// Друзьям (6+ сердец без романа) — короткое письмо после каждого случая.
        /// </summary>
        private void RegisterCollapse(int day, string reason)
        {
            var state = _stateManager.State;
            state.RecentCollapseDays.RemoveAll(d => day - d >= CollapseWindowDays || d > day);
            if (!state.RecentCollapseDays.Contains(day))
                state.RecentCollapseDays.Add(day);

            int count = state.RecentCollapseDays.Count;
            bool close = _dialogueManager.IsDatingEngagedOrMarriedToHarvey();

            if (!close && _dialogueManager.GetHarveyFriendship() >= FriendCollapseMinPoints)
                SendRepeatableMail(MailFriendCollapse);

            if (count >= 3)
            {
                _dialogueManager.RemoveTopic(ConversationTopics.RepeatCollapse);
                _dialogueManager.AddTopic(ConversationTopics.CollapsePattern, 3);

                if (close && (state.LastCollapsePatternMailDay < 0 || day - state.LastCollapsePatternMailDay >= CollapseWindowDays))
                {
                    state.LastCollapsePatternMailDay = day;
                    SendRepeatableMail(MailCollapsePattern);
                }
            }
            else if (count == 2)
            {
                _dialogueManager.AddTopic(ConversationTopics.RepeatCollapse, 2);
            }

            _stateManager.Save();
            _monitor.Log($"[Collapse] {reason}: день {day}, за {CollapseWindowDays} дней — {count}", LogLevel.Info);
        }

        /// <summary>
        /// Письмо сразу в почтовый ящик, даже если такое уже приходило: реплика Харви о письме
        /// (CollapsePattern) включается в тот же момент, и письмо не должно приходить на день позже.
        /// </summary>
        private static void SendRepeatableMail(string mailId)
        {
            var player = Game1.player;
            if (player.mailbox.Contains(mailId))
                return;

            player.mailForTomorrow.Remove(mailId);
            player.mailReceived.Remove(mailId);
            player.mailbox.Add(mailId);
        }

        private static bool IsOutdoorLocationName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return Game1.getLocationFromName(name)?.IsOutdoors == true;
        }

        private static bool IsHomeLocationName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return string.Equals(name, GetFarmHouseLocationName(), StringComparison.OrdinalIgnoreCase)
                || name.Contains("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Cabin", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Cellar", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Шахты долины 1–120 и вход (куда ведёт сцена eventHarveyMinorMineRescue).</summary>
        private static bool IsValleyMinesLocation(GameLocation? location)
        {
            if (location is MineShaft shaft)
                return shaft.mineLevel <= MineShaft.bottomOfMineLevel;

            return string.Equals(location?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>QA: проверка цепочек обморока без реальной гибели (без потери вещей и денег).</summary>
        public void RegisterQaCommands(ICommandHelper commands)
        {
            commands.Add(
                "injury_qa_death",
                "Симуляция гибели без потери вещей: injury_qa_death <локация_гибели> [локация_возрождения]. " +
                "По умолчанию возрождение как в ванили: UndergroundMine1–120 → Mine, VolcanoDungeon* → IslandSouth, иначе Hospital.",
                (_, args) => QaDeath(args));
            commands.Add(
                "injury_qa_passout",
                "Симуляция ночного обморока и утренней обработки: injury_qa_passout <exhaust|late> [локация] [здоровье].",
                (_, args) => QaPassOut(args));
            commands.Add(
                "injury_qa_collapse_status",
                "Состояние обмороков: pending-флаги, счётчик за 14 дней, ожидающие сцены.",
                (_, _) => _monitor.Log(BuildCollapseStatus(), LogLevel.Info));
        }

        private void QaDeath(string[] args)
        {
            if (!Context.IsWorldReady || args.Length == 0)
            {
                _monitor.Log("Использование: injury_qa_death <локация_гибели> [локация_возрождения]", LogLevel.Warn);
                return;
            }

            string deathLoc = args[0];
            string reviveLoc = args.Length > 1 ? args[1] : GuessVanillaReviveLocation(deathLoc);

            RecordCombatDeath(Game1.getLocationFromName(deathLoc) ?? Game1.currentLocation);
            _stateManager.State.PendingDeathLocation = deathLoc;
            _stateManager.State.PendingDeathReviveLocation = reviveLoc;
            Game1.player.health = Math.Max(Game1.player.health, 10);
            ProcessDeathRevive();

            _monitor.Log($"[QA] Гибель {deathLoc} → {reviveLoc} обработана.\n{BuildCollapseStatus()}", LogLevel.Info);
        }

        private string GuessVanillaReviveLocation(string deathLoc)
        {
            if (deathLoc.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(deathLoc.Substring("UndergroundMine".Length), out int level)
                && level <= MineShaft.bottomOfMineLevel)
            {
                return "Mine";
            }

            if (deathLoc.StartsWith("VolcanoDungeon", StringComparison.OrdinalIgnoreCase)
                || deathLoc.StartsWith("Island", StringComparison.OrdinalIgnoreCase))
            {
                return "IslandSouth";
            }

            return GetHospitalLocationName();
        }

        private void QaPassOut(string[] args)
        {
            if (!Context.IsWorldReady || args.Length == 0
                || (args[0] != "exhaust" && args[0] != "late"))
            {
                _monitor.Log("Использование: injury_qa_passout <exhaust|late> [локация] [здоровье]", LogLevel.Warn);
                return;
            }

            var state = _stateManager.State;
            state.WasPassedOut = true;
            state.WasExhausted = args[0] == "exhaust";
            state.WasUpTooLate = args[0] == "late";
            state.LastPassedOutLocation = args.Length > 1 ? args[1] : Game1.currentLocation?.NameOrUniqueName ?? "";
            state.LastPassedOutHealth = args.Length > 2 && int.TryParse(args[2], out int hp) ? hp : Game1.player.health;
            _stateManager.Save();

            ProcessOvernightPassOut();
            _monitor.Log($"[QA] Обморок ({args[0]}) обработан.\n{BuildCollapseStatus()}", LogLevel.Info);
        }

        private string BuildCollapseStatus()
        {
            var s = _stateManager.State;
            return string.Join("\n",
                $"PendingDeathRevive={s.PendingDeathRevive} death={s.PendingDeathLocation} revive={s.PendingDeathReviveLocation}",
                $"WasPassedOut={s.WasPassedOut} exhausted={s.WasExhausted} upTooLate={s.WasUpTooLate} loc={s.LastPassedOutLocation} hp={s.LastPassedOutHealth}",
                $"RecentCollapseDays=[{string.Join(", ", s.RecentCollapseDays)}] today={Helpers.GameUtils.Today()} patternMailDay={s.LastCollapsePatternMailDay}",
                $"NeedsMineRescueEvent={s.NeedsMineRescueEvent} PassedOutInMineYesterday={s.PassedOutInMineYesterday} PendingMineRescue={s.PendingMineRescueEventId}",
                $"PendingHospitalPassOut={s.PendingHospitalPassOutEventId} ExternalRescueHome={s.NeedsHarveyAfterExternalRescueHomeEvent} DischargeHome={s.NeedsHarveyAfterHospitalDischargeHomeEvent}",
                $"MorningAfterExhaustion={s.NeedsHarveyMorningAfterExhaustionEvent} mailForTomorrow=[{string.Join(", ", Game1.player.mailForTomorrow)}]");
        }

        private void RecordCombatDeath(GameLocation? location)
        {
            var state = _stateManager.State;
            string loc = location?.NameOrUniqueName ?? "";

            state.PendingDeathRevive = true;
            state.PendingDeathLocation = loc;
            state.PendingDeathReviveLocation = "";

            // WasPassedOut нужен RecoveryPlanManager.CheckViolationOnPassOut в DayEnding.
            state.WasPassedOut = true;
            state.WasExhausted = false;
            state.WasUpTooLate = false;
            state.LastPassedOutHealth = 0;
            state.LastPassedOutLocation = loc;

            _injuryManager.ApplyBadlyHurtFromMinePassOut();
            _stateManager.Save();

            _monitor.Log($"[Death] Боевая гибель зафиксирована в реальном времени: {loc}", LogLevel.Info);
        }

        /// <summary>
        /// Реакция на возрождение после гибели — по тому, КУДА игра перенесла фермера.
        /// </summary>
        private void ProcessDeathRevive()
        {
            var state = _stateManager.State;
            string deathLoc = state.PendingDeathLocation ?? "";
            string reviveLoc = state.PendingDeathReviveLocation;
            if (string.IsNullOrEmpty(reviveLoc))
                reviveLoc = Game1.currentLocation?.NameOrUniqueName ?? "";

            state.PendingDeathRevive = false;
            state.PendingDeathLocation = "";
            state.PendingDeathReviveLocation = "";

            bool dungeon = IsDungeonLocationName(deathLoc);

            if (string.Equals(reviveLoc, "Mine", StringComparison.OrdinalIgnoreCase))
                HandleMineDeath(deathLoc);
            else if (string.Equals(reviveLoc, GetHospitalLocationName(), StringComparison.OrdinalIgnoreCase))
                HandleHospitalRevive(deathLoc, dungeon);
            else
                HandleExternalRevive(deathLoc, reviveLoc, dungeon);

            RegisterCollapse(Helpers.GameUtils.Today(), $"гибель в {deathLoc}");
            _stateManager.Save();
        }

        /// <summary>Шахты 1–120: спасли Линус/Робин/Клинт/Мару → утренний rescue Харви у кровати.</summary>
        private void HandleMineDeath(string deathLoc)
        {
            var state = _stateManager.State;
            state.PassedOutInMineYesterday = true;

            if (!CanHarveyMineRescue())
            {
                _monitor.Log(
                    $"[MineRescue] Гибель в шахте ({deathLoc}): buffBadlyHurt применён, rescue Харви недоступен (мало дружбы / нет лечения)",
                    LogLevel.Debug);
                return;
            }

            state.NeedsMineRescueEvent = true;
            if (_buffManager.HasBuff(InjuryBuffs.MineRestricted))
            {
                MineForbiddenHelper.ApplyHardMineForbidden(
                    state, _config, _buffManager, _stateManager, _monitor, (int)Game1.stats.DaysPlayed,
                    "passout_in_mine_restricted");
            }

            _monitor.Log($"[MineRescue] Гибель в шахте ({deathLoc}) — утренний rescue запланирован", LogLevel.Info);
        }

        /// <summary>
        /// Пещера Черепа, город, ферма и т.п.: фермер очнулся в клинике после ванильной сцены с Харви.
        /// Харви сам осматривал — травма известна ему, лечение начато; Dating+ → вечером Харви приходит домой.
        /// </summary>
        private void HandleHospitalRevive(string deathLoc, bool dungeon)
        {
            var state = _stateManager.State;
            bool close = _dialogueManager.IsDatingEngagedOrMarriedToHarvey();

            HarveyInjuryAwarenessHelper.MarkAllActiveInjuriesHarveyAware(
                _stateManager, _dialogueManager, _injuryManager, _buffManager, "hospital_revive", _monitor);
            StartMineRescueTreatmentIfNeeded();

            if (close || (dungeon && CanHarveyMineRescue()))
            {
                MineForbiddenHelper.ApplyHardMineForbidden(
                    state, _config, _buffManager, _stateManager, _monitor, (int)Game1.stats.DaysPlayed,
                    dungeon ? "death_in_dungeon" : "critical_passout");
            }

            // addMailForTomorrow не шлёт уже полученное письмо — при повторной гибели рекомендаций не было бы.
            SendRepeatableMail("mailHarveyPostTrauma");

            if (close)
            {
                int today = Helpers.GameUtils.Today();
                state.NeedsHarveyAfterHospitalDischargeHomeEvent = true;
                state.LastHospitalDischargeDay = today;
                state.LastHospitalDischargeInjuryId = _injuryManager.GetActiveInjury() ?? "buffBadlyHurt";

                if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.AfterHospitalDischargeHome))
                    _dialogueManager.AddTopic(ConversationTopics.AfterHospitalDischargeHome, 2);
            }

            _monitor.Log(
                $"[Death] Возрождение в клинике после гибели в {deathLoc}: лечение начато, close={close}, dungeon={dungeon}",
                LogLevel.Info);
        }

        /// <summary>Вулкан (Вилли/Лео), клиники других модов: Харви не было рядом → Dating+ ночная забота дома.</summary>
        private void HandleExternalRevive(string deathLoc, string reviveLoc, bool dungeon)
        {
            if (dungeon && CanHarveyMineRescue())
            {
                MineForbiddenHelper.ApplyHardMineForbidden(
                    _stateManager.State, _config, _buffManager, _stateManager, _monitor, (int)Game1.stats.DaysPlayed,
                    "death_in_dungeon");
            }

            MarkExternalRescueIfEligible(deathLoc);

            _monitor.Log(
                $"[Death] Внешнее спасение: гибель в {deathLoc}, очнулась в {reviveLoc}",
                LogLevel.Info);
        }

        private static bool IsDungeonLocationName(string location)
        {
            return location.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)
                || location.StartsWith("VolcanoDungeon", StringComparison.OrdinalIgnoreCase)
                || location.Contains("SkullCave", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Отследить обморок игрока (вызывается перед DayEnding)
        /// </summary>
        public void TrackPassOut()
        {
            var currentLocation = Game1.currentLocation?.NameOrUniqueName ?? "";

            // Гибель, после которой день кончился раньше, чем игрок стал свободен (например, в 1:50).
            if (_stateManager.State.PendingDeathRevive)
                ProcessDeathRevive();

            // Пациент в палате под присмотром Харви: поздний отбой/усталость там — не обморок
            // и не нарушение плана (обычно сюда не доходит — в 25:00 срабатывает сон в палате).
            if (_hospitalizationManager?.IsHospitalized == true
                && _hospitalizationManager.IsInClinic(Game1.currentLocation))
            {
                _monitor.Log(
                    $"[PassOut] Пропуск: пациент в палате (time={Game1.timeOfDay}, stamina={Game1.player.stamina})",
                    LogLevel.Info);
                return;
            }

            if (Game1.player.stamina <= -15f)
            {
                _stateManager.State.WasPassedOut = true;
                _stateManager.State.WasExhausted = true;
                _stateManager.State.LastPassedOutHealth = Game1.player.health;
                _stateManager.State.LastPassedOutLocation = currentLocation;

                _monitor.Log($"Обнаружен обморок от истощения (stamina={Game1.player.stamina})", LogLevel.Info);
            }
            else if (Game1.timeOfDay >= 2600)
            {
                _stateManager.State.WasPassedOut = true;
                _stateManager.State.WasUpTooLate = true;
                _stateManager.State.LastPassedOutHealth = Game1.player.health;
                _stateManager.State.LastPassedOutLocation = currentLocation;

                _monitor.Log($"Обнаружен обморок от позднего времени (time={Game1.timeOfDay})", LogLevel.Info);
            }

            // День обморока нужен уже в DayStarted (HarveyWellbeingManager: «Харви тревожится»),
            // эскалация и реплики — утром в ProcessOvernightPassOut → RegisterCollapse.
            int collapseDay = Helpers.GameUtils.Today();
            if (_stateManager.State.WasPassedOut
                && (_stateManager.State.WasExhausted || _stateManager.State.WasUpTooLate)
                && !_stateManager.State.RecentCollapseDays.Contains(collapseDay))
            {
                _stateManager.State.RecentCollapseDays.Add(collapseDay);
            }

            // Боевая гибель обрабатывается в реальном времени (OnUpdateTicked → ProcessDeathRevive):
            // к DayEnding игра уже вернула 10 HP, здесь её не увидеть.
            _recoveryPlanManager?.CheckViolationOnPassOut();
        }

        /// <summary>
        /// Внешнее спасение (Вилли/Лео на острове, клиники модов): Dating+ → ночная реакция Харви дома.
        /// </summary>
        private void MarkExternalRescueIfEligible(string deathLocation)
        {
            if (!_dialogueManager.IsDatingEngagedOrMarriedToHarvey())
                return;

            var state = _stateManager.State;
            int today = Helpers.GameUtils.Today();
            state.NeedsHarveyAfterExternalRescueHomeEvent = true;
            state.LastExternalRescueLocation = deathLocation;
            state.LastExternalRescueDay = today;

            if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.ExternalRescueConcern))
                _dialogueManager.AddTopic(ConversationTopics.ExternalRescueConcern, 2);

            _stateManager.Save();
            _monitor.Log(
                $"[ExternalRescue] Помечено для ночного события дома: loc={deathLocation}, day={today}",
                LogLevel.Info);
        }

        /// <summary>
        /// Инициирует утреннее событие после боевой смерти в шахте.
        /// Вызывается из DayStarted, когда игрок уже проснулся дома (ванильная механика).
        /// Cutscene у кровати в FarmHouse; событие добавляет topicMineInjuryRescue.
        /// </summary>
        public void TriggerMineRescueEvents()
        {
            if (!_stateManager.State.NeedsMineRescueEvent) return;

            int today = (int)Game1.stats.DaysPlayed;
            MineForbiddenHelper.ApplyHardMineForbidden(
                _stateManager.State, _config, _buffManager, _stateManager, _monitor, today, "mine_rescue");

            _monitor.Log("[MineRescue] Подготовка события спасения из шахты", LogLevel.Info);

            if (!CanHarveyMineRescue())
            {
                _monitor.Log("[MineRescue] Харви не участвует в rescue — пропускаем cutscene", LogLevel.Debug);
                ClearMineRescueState();
                return;
            }

            // Resume после reload: pending ID уже в save
            if (!string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId))
            {
                _monitor.Log(
                    $"[MineRescue] Resume pending: {_stateManager.State.PendingMineRescueEventId}",
                    LogLevel.Info);
                BeginMineRescueMorningEvent(_stateManager.State.PendingMineRescueEventId);
                return;
            }

            // Боевая смерть в шахте → только major rescue (minor — отдельный триггер в OnUpdateTicked)
            bool hasSevere = _injuryManager.IsMainInjurySerious();
            string eventId = ResolveSevereMineRescueEventId();

            _monitor.Log($"[MineRescue] Выбран eventId: {eventId} (серьёзные травмы: {hasSevere})", LogLevel.Info);

            // Если событие уже было — показываем только топик, без повтора кинематики
            if (IsMineRescueEventAlreadySeen(eventId))
            {
                _monitor.Log($"[MineRescue] Событие {eventId} уже просматривалось — добавляем topicMineInjuryRescue", LogLevel.Info);
                EnsureMineRescueTopic();

                // Повторная гибель: вместо повтора сцены у кровати — осмотр на ферме (eventHarveyCheckHealthFarmer).
                if (_dialogueManager.IsDatingEngagedOrMarriedToHarvey()
                    && !Helpers.GameUtils.HasConversationTopic(ConversationTopics.CheckHealthAfterCollapse))
                {
                    _dialogueManager.AddTopic(ConversationTopics.CheckHealthAfterCollapse, 1);
                }

                ClearMineRescueState();
                return;
            }

            BeginMineRescueMorningEvent(eventId);
        }

        /// <summary>
        /// После загрузки сохранения: retry, если reload случился между warp домой и startEvent.
        /// </summary>
        public void ResumePendingMineRescueIfNeeded()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (!_stateManager.State.NeedsMineRescueEvent)
                return;

            if (string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId))
                return;

            _monitor.Log("[MineRescue] SaveLoaded: resume pending mine rescue", LogLevel.Info);
            BeginMineRescueMorningEvent(_stateManager.State.PendingMineRescueEventId);
        }

        /// <summary>
        /// После загрузки сохранения: retry hospital pass-out cutscene между warp и startEvent.
        /// </summary>
        public void ResumePendingHospitalPassOutIfNeeded()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (string.IsNullOrEmpty(_stateManager.State.PendingHospitalPassOutEventId))
                return;

            _monitor.Log("[PassOutEvent] SaveLoaded: resume pending hospital pass-out cutscene", LogLevel.Info);
            BeginHospitalPassOutWarp(
                _stateManager.State.PendingHospitalPassOutEventId,
                _stateManager.State.PendingHospitalPassOutFallbackKind);
        }

        /// <summary>
        /// Опасное состояние в шахте без Severe: eventHarveyMinorMineRescue (не боевой death-rescue).
        /// </summary>
        public bool TryTriggerMinorMineRescue()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return false;

            if (!CanTriggerMinorMineRescue())
                return false;

            const string eventId = EventIds.MinorMineRescue;
            int today = Helpers.GameUtils.Today();
            _stateManager.State.LastMinorMineRescueDay = today;

            if (Game1.player.eventsSeen.Contains(eventId))
            {
                _monitor.Log("[MinorMineRescue] Событие уже seen — пропуск cutscene", LogLevel.Debug);
                _stateManager.Save();
                return false;
            }

            if (!LocationEventExists(eventId, "Mine"))
            {
                _monitor.Log("[MinorMineRescue] Событие не найдено в Data/Events/Mine", LogLevel.Warn);
                return false;
            }

            _dialogueManager.AddTopic(ConversationTopics.MineRescuePending, 1);
            _stateManager.Save();

            if (string.Equals(Game1.currentLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
            {
                if (TryStartLocationEvent(eventId, "Mine", OnMinorMineRescueEventFinished))
                    return true;

                RunMinorMineRescueFallback();
                return false;
            }

            var mineLocation = Game1.getLocationFromName("Mine");
            if (mineLocation == null)
            {
                _monitor.Log("[MinorMineRescue] Локация Mine не найдена", LogLevel.Error);
                RunMinorMineRescueFallback();
                return false;
            }

            // Сцена начинается у входа в шахты долины. Из Пещеры Черепа и вулкана туда не переносим —
            // только реплика и HUD (LastMinorMineRescueDay уже не даст повторить сегодня).
            if (!IsValleyMinesLocation(Game1.currentLocation))
            {
                _monitor.Log(
                    $"[MinorMineRescue] {Game1.currentLocation?.NameOrUniqueName} — не шахты долины, только HUD",
                    LogLevel.Debug);
                RunMinorMineRescueFallback();
                return false;
            }

            _stateManager.State.PendingMinorMineRescueEventId = eventId;
            _stateManager.Save();
            _monitor.Log("[MinorMineRescue] Warp в Mine для cutscene", LogLevel.Info);
            Game1.warpFarmer(new LocationRequest("Mine", false, mineLocation), 17, 7, 2);
            return true;
        }

        public void ResumePendingMinorMineRescueIfNeeded()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            if (string.IsNullOrEmpty(_stateManager.State.PendingMinorMineRescueEventId))
                return;

            _monitor.Log("[MinorMineRescue] SaveLoaded: resume pending minor mine rescue", LogLevel.Info);

            var mineLocation = Game1.getLocationFromName("Mine");
            if (mineLocation == null)
            {
                RunMinorMineRescueFallback();
                return;
            }

            if (!string.Equals(Game1.currentLocation?.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase))
                Game1.warpFarmer(new LocationRequest("Mine", false, mineLocation), 17, 7, 2);
            else if (!TryStartLocationEvent(
                         _stateManager.State.PendingMinorMineRescueEventId,
                         "Mine",
                         OnMinorMineRescueEventFinished))
                RunMinorMineRescueFallback();
        }

        public bool CanTriggerMinorMineRescue()
        {
            if (!_dialogueManager.IsDatingOrMarriedToHarvey())
                return false;

            if (_stateManager.State.NeedsMineRescueEvent)
                return false;

            if (!string.IsNullOrEmpty(_stateManager.State.PendingMineRescueEventId)
                || !string.IsNullOrEmpty(_stateManager.State.PendingMinorMineRescueEventId))
                return false;

            if (_dialogueManager.HasTopic(ConversationTopics.MineRescuePending))
                return false;

            if (Game1.player.eventsSeen.Contains(EventIds.MinorMineRescue))
                return false;

            int today = Helpers.GameUtils.Today();
            if (_stateManager.State.LastMinorMineRescueDay == today)
                return false;

            if (Game1.eventUp || Game1.activeClickableMenu != null)
                return false;

            if (!IsPlayerInMineOrVolcano(Game1.currentLocation))
                return false;

            if (_injuryManager.IsMainInjurySerious())
                return false;

            bool hasAnyInjury = _stateManager.GetAllActiveDebuffStates().Count > 0
                || _stateManager.State.ActiveComplications.Count > 0
                || _injuryManager.GetActiveInjury() != null;

            if (!hasAnyInjury)
                return false;

            int healthThreshold = (int)Math.Ceiling(Game1.player.maxHealth * 0.35f);
            bool lowHealth = Game1.player.health <= healthThreshold;
            bool lowStamina = Game1.player.stamina <= Game1.player.maxStamina.Value * 0.15f;

            return lowHealth || lowStamina;
        }

        private static bool IsPlayerInMineOrVolcano(GameLocation? location)
        {
            if (location == null)
                return false;

            string name = location.NameOrUniqueName ?? "";
            return location is MineShaft
                || location is VolcanoDungeon
                || string.Equals(name, "Mine", StringComparison.OrdinalIgnoreCase)
                || name.Contains("SkullCave", StringComparison.OrdinalIgnoreCase);
        }

        private void OnMinorMineRescueEventFinished(string eventId)
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains(eventId))
                    Game1.player.eventsSeen.Add(eventId);

                _dialogueManager.RemoveTopic(ConversationTopics.MineRescuePending);
                _stateManager.State.PendingMinorMineRescueEventId = "";
                _stateManager.State.LastMinorMineRescueDay = Helpers.GameUtils.Today();

                if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.MinorMineRescue))
                    _dialogueManager.AddTopic(ConversationTopics.MinorMineRescue, 2);

                _stateManager.Save();
                _monitor.Log($"[MinorMineRescue] ✅ Событие '{eventId}' завершено", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MinorMineRescue] ❌ onEventFinished: {ex}", LogLevel.Error);
            }
        }

        private void RunMinorMineRescueFallback()
        {
            _dialogueManager.RemoveTopic(ConversationTopics.MineRescuePending);
            _stateManager.State.PendingMinorMineRescueEventId = "";

            if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.MinorMineRescue))
                _dialogueManager.AddTopic(ConversationTopics.MinorMineRescue, 2);

            Game1.addHUDMessage(new HUDMessage(
                "Харви настаивает: в таком состоянии шахта слишком опасна. Отдохни и зайди в клинику.",
                HUDMessage.health_type));

            _stateManager.Save();
        }

        /// <summary>
        /// Утренний cutscene у кровати: без warp в шахту (ваниль — игрок уже дома).
        /// </summary>
        private void BeginMineRescueMorningEvent(string eventId)
        {
            _dialogueManager.AddTopic(ConversationTopics.MineRescuePending, 1);

            string farmHouseName = GetFarmHouseLocationName();
            if (!LocationEventExists(eventId, farmHouseName))
            {
                _monitor.Log(
                    $"[MineRescue] ❌ {eventId} не найден в Data/Events/{farmHouseName}, fallback",
                    LogLevel.Error);
                RunMineRescueFallback(eventId);
                return;
            }

            if (IsFarmHouseLocation(Game1.currentLocation))
            {
                if (TryStartFarmhouseMineRescueEvent(eventId, farmHouseName))
                    return;

                RunMineRescueFallback(eventId);
                return;
            }

            var farmHouse = Game1.getLocationFromName(farmHouseName);
            if (farmHouse == null)
            {
                _monitor.Log($"[MineRescue] ❌ Локация '{farmHouseName}' не найдена", LogLevel.Error);
                RunMineRescueFallback(eventId);
                return;
            }

            _stateManager.State.PendingMineRescueEventId = eventId;
            _stateManager.Save();

            var bed = Game1.player.mostRecentBed;
            int bedX = bed.X >= 0 ? (int)bed.X : 9;
            int bedY = bed.Y >= 0 ? (int)bed.Y : 9;
            _monitor.Log($"[MineRescue] Warp домой ({bedX},{bedY}) для утреннего cutscene: {eventId}", LogLevel.Info);
            Game1.warpFarmer(farmHouseName, bedX, bedY, 2);
        }

        private bool TryStartFarmhouseMineRescueEvent(string eventId, string locationName)
        {
            int fx = (int)Game1.player.Tile.X;
            int fy = (int)Game1.player.Tile.Y;
            int hx = fx + 1;
            int hy = fy;

            string setup =
                $"viewport {fx} {fy} true/skippable/pause 200/faceDirection farmer 2/warp Harvey {hx} {hy} 3/faceDirection Harvey 3/";

            return TryStartLocationEvent(eventId, locationName, OnMineRescueEventFinished, setup);
        }

        private static string GetFarmHouseLocationName()
        {
            return Game1.player?.homeLocation.Value ?? "FarmHouse";
        }

        private static bool IsFarmHouseLocation(GameLocation? location)
        {
            if (location == null)
                return false;

            string name = location.NameOrUniqueName ?? "";
            if (string.Equals(name, GetFarmHouseLocationName(), StringComparison.OrdinalIgnoreCase))
                return true;

            return name.Contains("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Cabin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Ставит в очередь hospital cutscene: warp в Hospital → startEvent в OnPlayerWarped.
        /// Если событие уже seen — только fallback (topic/HUD), без повторной сцены.
        /// </summary>
        private bool QueueHospitalEvent(string eventId, string fallbackKind)
        {
            if (Game1.player.eventsSeen.Contains(eventId))
            {
                _monitor.Log($"[PassOutEvent] {eventId} уже просмотрено — fallback без cutscene", LogLevel.Info);
                RunHospitalPassOutFallback(fallbackKind);
                return true;
            }

            if (!LocationEventExists(eventId, GetHospitalLocationName()))
            {
                _monitor.Log($"[PassOutEvent] {eventId} не найден в Data/Events/{GetHospitalLocationName()}", LogLevel.Warn);
                return false;
            }

            BeginHospitalPassOutWarp(eventId, fallbackKind);
            return true;
        }

        private void BeginHospitalPassOutWarp(string eventId, string fallbackKind)
        {
            string hospitalName = GetHospitalLocationName();
            var hospitalLocation = Game1.getLocationFromName(hospitalName);
            if (hospitalLocation == null)
            {
                _monitor.Log($"[PassOutEvent] ❌ Локация '{hospitalName}' не найдена — fallback", LogLevel.Error);
                RunHospitalPassOutFallback(fallbackKind);
                return;
            }

            if (IsHospitalLocation(Game1.currentLocation))
            {
                _stateManager.State.PendingHospitalPassOutEventId = eventId;
                _stateManager.State.PendingHospitalPassOutFallbackKind = fallbackKind;
                _stateManager.Save();

                if (!TryStartLocationEvent(eventId, hospitalName, OnHospitalPassOutEventFinished))
                    RunHospitalPassOutFallback(fallbackKind);

                return;
            }

            _stateManager.State.PendingHospitalPassOutEventId = eventId;
            _stateManager.State.PendingHospitalPassOutFallbackKind = fallbackKind;
            _stateManager.Save();

            int bedX = Math.Max(0, _config.HospitalBedX);
            int bedY = Math.Max(0, _config.HospitalBedY);
            _monitor.Log($"[PassOutEvent] Warp в {hospitalName} ({bedX},{bedY}) для {eventId}", LogLevel.Info);
            Game1.warpFarmer(hospitalName, bedX, bedY, 0);
        }

        private void RunHospitalPassOutFallback(string fallbackKind)
        {
            switch (fallbackKind)
            {
                case FallbackCritical:
                    RunCriticalPassOutFallback();
                    break;
                case FallbackExhaustion:
                    RunExhaustionPassOutFallback();
                    break;
                default:
                    _monitor.Log($"[PassOutEvent] Неизвестный fallback '{fallbackKind}'", LogLevel.Warn);
                    break;
            }

            ClearHospitalPassOutPending();
        }

        private void RunCriticalPassOutFallback()
        {
            Game1.addHUDMessage(new HUDMessage(
                "Ты чувствуешь себя очень плохо после потери сознания...",
                HUDMessage.error_type));
        }

        private void RunExhaustionPassOutFallback()
        {
            MarkHarveyMorningAfterExhaustionIfEligible();
            _buffManager.AddBuff("buffFarmerExhausted", -2);
            _dialogueManager.AddTopic(ConversationTopics.FarmerExhausted, 3);
            Game1.playSound("debuffHit");
            Game1.addHUDMessage(new HUDMessage("Ты полностью измотана...", HUDMessage.health_type));
        }

        /// <summary>
        /// Помечает утреннее мини-событие после обморока от истощения (Dating / Engaged / Married).
        /// Запуск — через <see cref="HarveyHomeCareEventLauncher.TryTriggerHarveyHomeCareEvent"/>.
        /// </summary>
        private void MarkHarveyMorningAfterExhaustionIfEligible()
        {
            if (!_dialogueManager.IsDatingEngagedOrMarriedToHarvey())
                return;

            int today = Helpers.GameUtils.Today();
            var state = _stateManager.State;
            state.NeedsHarveyMorningAfterExhaustionEvent = true;
            state.LastExhaustionCollapseDay = today;

            if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.HarveyMorningAfterExhaustion))
                _dialogueManager.AddTopic(ConversationTopics.HarveyMorningAfterExhaustion, 2);

            _stateManager.Save();
            _monitor.Log(
                $"[MorningAfterExhaustion] Помечено утреннее событие: collapseDay={today}",
                LogLevel.Info);
        }

        private void ClearHospitalPassOutPending()
        {
            _stateManager.State.PendingHospitalPassOutEventId = "";
            _stateManager.State.PendingHospitalPassOutFallbackKind = "";
            _stateManager.Save();
        }

        private void ClearPassOutFlags()
        {
            _stateManager.State.WasPassedOut = false;
            _stateManager.State.WasExhausted = false;
            _stateManager.State.WasUpTooLate = false;
            _stateManager.State.LastPassedOutHealth = -1;
            _stateManager.State.LastPassedOutLocation = "";
            _stateManager.Save();
        }

        /// <summary>
        /// Харви приезжает в шахту: dating/married, 8+ сердец, или игрок уже на лечении у Харви.
        /// </summary>
        private bool CanHarveyMineRescue()
        {
            if (_dialogueManager.IsDatingOrMarriedToHarvey())
                return true;

            if (_dialogueManager.GetHarveyFriendship() >= RescueOperationIds.MinFriendshipPoints)
                return true;

            foreach (var debuffState in _stateManager.GetAllActiveDebuffStates())
            {
                if (debuffState.TreatmentStarted)
                    return true;
            }

            if (_buffManager.HasBuff(CureBuffs.Treatment))
                return true;

            return Helpers.GameUtils.HasConversationTopic(ConversationTopics.HarveyNeedsFirstTreatment);
        }

        private bool IsMineRelatedPassOut()
        {
            if (_stateManager.State.PassedOutInMineYesterday || _stateManager.State.NeedsMineRescueEvent)
                return true;

            string location = _stateManager.State.LastPassedOutLocation ?? "";
            return location.Contains("Mine", StringComparison.OrdinalIgnoreCase)
                || location.Contains("SkullCave", StringComparison.OrdinalIgnoreCase)
                || string.Equals(location, "UndergroundMine", StringComparison.OrdinalIgnoreCase);
        }

        private string GetHospitalLocationName() => _config.HospitalLocationName;

        private bool IsHospitalLocation(GameLocation? location)
        {
            return string.Equals(location?.NameOrUniqueName, GetHospitalLocationName(), StringComparison.OrdinalIgnoreCase);
        }

        private void OnHospitalPassOutEventFinished(string eventId)
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains(eventId))
                    Game1.player.eventsSeen.Add(eventId);

                ClearHospitalPassOutPending();
                ClearPassOutFlags();

                _monitor.Log($"[PassOutEvent] ✅ Событие '{eventId}' завершено", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[PassOutEvent] ❌ Ошибка в onEventFinished для '{eventId}': {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Аварийный сценарий, если CP-событие спасения не найдено или не стартовало.
        /// </summary>
        private void RunMineRescueFallback(string eventId)
        {
            _monitor.Log($"[MineRescue] Fallback для {eventId}: topic + перенос в госпиталь", LogLevel.Warn);

            EnsureMineRescueTopic();
            Game1.addHUDMessage(new HUDMessage("Харви нашёл тебя после обморока в шахте. Нужно срочно в клинику.", HUDMessage.error_type));

            string hospital = _config.HospitalLocationName;
            if (Game1.getLocationFromName(hospital) == null)
            {
                _monitor.Log(
                    $"[MineRescue] ❌ Локация '{hospital}' не найдена — только topic, NeedsMineRescueEvent остаётся для retry",
                    LogLevel.Error);
                _stateManager.State.PendingMineRescueEventId = "";
                _stateManager.Save();
                return;
            }

            ClearMineRescueState();
            WarpFarmerToHospitalBed();
            StartMineRescueTreatmentIfNeeded();
        }

        private void WarpFarmerToHospitalBed()
        {
            string hospital = _config.HospitalLocationName;
            if (Game1.getLocationFromName(hospital) == null)
            {
                _monitor.Log($"[MineRescue] Локация '{hospital}' не найдена — warp в госпиталь пропущен", LogLevel.Warn);
                return;
            }

            Game1.warpFarmer(hospital, Math.Max(0, _config.HospitalBedX), Math.Max(0, _config.HospitalBedY), 0);
        }

        private void EnsureMineRescueTopic()
        {
            if (!Helpers.GameUtils.HasConversationTopic(ConversationTopics.MineInjuryRescue))
                _dialogueManager.AddTopic(ConversationTopics.MineInjuryRescue, 2);

            HarveyInjuryAwarenessHelper.MarkAllActiveInjuriesHarveyAware(
                _stateManager,
                _dialogueManager,
                _injuryManager,
                _buffManager,
                "mine_rescue",
                _monitor);
        }

        private void ClearMineRescueState()
        {
            _dialogueManager.RemoveTopic(ConversationTopics.MineRescuePending);
            _stateManager.State.PendingMineRescueEventId = "";
            _stateManager.State.NeedsMineRescueEvent = false;
            _stateManager.State.PassedOutInMineYesterday = false;
            _stateManager.State.WasPassedOut = false;
            _stateManager.State.WasExhausted = false;
            _stateManager.State.WasUpTooLate = false;
            _stateManager.State.LastPassedOutHealth = -1;
            _stateManager.State.LastPassedOutLocation = "";
            _stateManager.Save();
        }

        /// <summary>
        /// После cutscene спасения в шахте Харви уже начал осмотр — переводим buffBadlyHurt в фазу 1 лечения.
        /// </summary>
        private void StartMineRescueTreatmentIfNeeded()
        {
            const string injuryId = "buffBadlyHurt";

            if (!_injuryManager.HasInjuryOrPhase(injuryId))
            {
                _monitor.Log("[MineRescue] Авто-лечение пропущено: buffBadlyHurt не активен", LogLevel.Debug);
                return;
            }

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState?.TreatmentStarted == true)
            {
                _monitor.Log("[MineRescue] Авто-лечение пропущено: TreatmentStarted уже true", LogLevel.Debug);
                return;
            }

            if (_treatmentStartHandler != null)
            {
                _treatmentStartHandler.TryStartTreatment(injuryId, fromDialogueAction: false, out _);
                return;
            }

            _treatmentManager.ApplyTreatmentForInjury(injuryId);
            _dialogueManager.ClearHarveyNeedsFirstTreatmentTopic("лечение начато после спасения в шахте");
            _stateManager.MarkHarveyConversation(injuryId, true);
            _stateManager.Save();

            _monitor.Log("[MineRescue] Автоматически начата фаза 1 лечения buffBadlyHurt", LogLevel.Info);
        }

        private void OnMineRescueEventFinished(string eventId)
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains(eventId))
                    Game1.player.eventsSeen.Add(eventId);

                EnsureMineRescueTopic();
                ClearMineRescueState();

                if (!IsMorningMineRescueEvent(eventId))
                    WarpFarmerToHospitalBed();

                StartMineRescueTreatmentIfNeeded();

                _monitor.Log(
                    $"[MineRescue] ✅ Событие '{eventId}' завершено — eventsSeen, флаги и лечение обновлены",
                    LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MineRescue] ❌ Ошибка в onEventFinished для '{eventId}': {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Утренний cutscene у кровати: dating-тон или нейтральный.
        /// </summary>
        private string ResolveSevereMineRescueEventId()
        {
            string farmHouseName = GetFarmHouseLocationName();

            if (_dialogueManager.IsDatingOrMarriedToHarvey()
                && LocationEventExists(EventIds.MineRescueMorningDating, farmHouseName))
            {
                return EventIds.MineRescueMorningDating;
            }

            if (LocationEventExists(EventIds.MineRescueMorning, farmHouseName))
                return EventIds.MineRescueMorning;

            _monitor.Log(
                $"[MineRescue] Утренние события не найдены в Data/Events/{farmHouseName} — будет fallback",
                LogLevel.Warn);
            return EventIds.MineRescueMorning;
        }

        private static bool IsMorningMineRescueEvent(string eventId)
        {
            return string.Equals(eventId, EventIds.MineRescueMorning, StringComparison.OrdinalIgnoreCase)
                || string.Equals(eventId, EventIds.MineRescueMorningDating, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Проверяет наличие entry в Data/Events/{locationName}.
        /// </summary>
        private static bool LocationEventExists(string eventId, string locationName)
        {
            var eventData = Game1.content.Load<System.Collections.Generic.Dictionary<string, string>>($"Data/Events/{locationName}");
            if (eventData == null)
                return false;

            foreach (string key in eventData.Keys)
            {
                if (EventKeyMatches(key, eventId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Уже просмотрено это событие или legacy major (для dating-ветки до патча).
        /// </summary>
        private static bool IsMineRescueEventAlreadySeen(string eventId)
        {
            var seen = Game1.player.eventsSeen;
            if (seen.Contains(eventId))
                return true;

            // Утренние и legacy mine-варианты — одна история, один просмотр.
            string[] majorRescueIds =
            {
                EventIds.MineRescueMorningDating,
                EventIds.MineRescueMorning,
                EventIds.MineRescueDating,
                EventIds.MineRescue,
            };

            foreach (string id in majorRescueIds)
            {
                if (seen.Contains(id))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Запускает событие по его ID в указанной локации.
        /// Предполагает, что игрок уже находится в этой локации.
        /// </summary>
        private static bool EventKeyMatches(string key, string eventId)
        {
            return key.Equals(eventId, StringComparison.OrdinalIgnoreCase)
                || key.StartsWith(eventId + "/", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryStartLocationEvent(
            string eventId,
            string locationName,
            Action<string> onFinished,
            string? scriptPrefix = null)
        {
            try
            {
                var location = Game1.getLocationFromName(locationName);
                if (location == null)
                {
                    _monitor.Log($"[PassOutEvent] ❌ Локация '{locationName}' не найдена", LogLevel.Error);
                    return false;
                }

                string? eventScript = LoadLocationEventScript(eventId, locationName);
                if (string.IsNullOrWhiteSpace(eventScript))
                    return false;

                if (!string.IsNullOrEmpty(scriptPrefix))
                    eventScript = scriptPrefix + eventScript;

                if (!ReferenceEquals(Game1.currentLocation, location)
                    && !string.Equals(Game1.currentLocation?.NameOrUniqueName, locationName, StringComparison.OrdinalIgnoreCase))
                {
                    _monitor.Log(
                        $"[PassOutEvent] ❌ Игрок не в '{locationName}' (текущая: {Game1.currentLocation?.NameOrUniqueName})",
                        LogLevel.Error);
                    return false;
                }

                try
                {
                    var gameEvent = new Event(eventScript);
                    string capturedId = eventId;
                    gameEvent.onEventFinished += () => onFinished(capturedId);
                    location.startEvent(gameEvent);
                    _monitor.Log(
                        $"[PassOutEvent] ✅ Событие '{eventId}' запущено; eventsSeen — по onEventFinished",
                        LogLevel.Info);
                    return true;
                }
                catch (Exception ex)
                {
                    _monitor.Log($"[PassOutEvent] ❌ Ошибка запуска события '{eventId}': {ex}", LogLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"[PassOutEvent] ❌ Ошибка при подготовке '{eventId}': {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private string? LoadLocationEventScript(string eventId, string locationName)
        {
            var eventData = Game1.content.Load<System.Collections.Generic.Dictionary<string, string>>(
                $"Data/Events/{locationName}");
            if (eventData == null)
            {
                _monitor.Log($"[PassOutEvent] ❌ Data/Events/{locationName} не найдены", LogLevel.Error);
                return null;
            }

            foreach (var kvp in eventData)
            {
                if (EventKeyMatches(kvp.Key, eventId))
                    return kvp.Value;
            }

            _monitor.Log(
                $"[PassOutEvent] ❌ Событие '{eventId}' не найдено в Data/Events/{locationName}",
                LogLevel.Error);
            return null;
        }
    }
}
