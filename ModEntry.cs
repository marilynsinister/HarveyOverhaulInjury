using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.EventHandlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buffs;

namespace HarveyOverhaul.InjuryCare
{
    /// <summary>
    /// Точка входа мода - координирует работу всех менеджеров
    /// </summary>
    public sealed class ModEntry : Mod
    {
        // Менеджеры
        private StateManager _stateManager = null!;
        private BuffManager _buffManager = null!;
        private DialogueManager _dialogueManager = null!;
        private InjuryManager _injuryManager = null!;
        private TreatmentManager _treatmentManager = null!;
        private ProximityReactionManager _proximityReactionManager = null!;
        private HospitalizationManager _hospitalizationManager = null!;
        private HospitalActivityManager _hospitalActivityManager = null!;
        private ComplicationManager _complicationManager = null!;

        // Обработчики событий
        private GameEventHandler _gameEventHandler = null!;
        private PlayerEventHandler _playerEventHandler = null!;
        private InteractionHandler _interactionHandler = null!;
        private TimeEventHandler _timeEventHandler = null!;
        private PassOutHandler _passOutHandler = null!;

        // Конфигурация
        private ModConfig _config = null!;

        /// <summary>Режим дебаг-HUD (F10): 0 скрыт, 1 compact, 2 full.</summary>
        private int _debugHudMode = 1;

        /// <summary>
        /// Точка входа - инициализация мода
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // Загрузить конфигурацию
            _config = helper.ReadConfig<ModConfig>();

            // Инициализировать менеджеры
            InitializeManagers();

            // Подписаться на события
            SubscribeToEvents(helper.Events);

            // Консольная команда для полного сброса данных мода (отладка)
            helper.ConsoleCommands.Add(
                "injury_reset",
                "Полный сброс всех данных мода: дебаффы, осложнения, топики, состояние.",
                (_, _) => FullReset());

            helper.ConsoleCommands.Add(
                "injury_debuff_list",
                "Список ID дебаффов мода (травмы и осложнения). Используйте с injury_debuff_add.",
                (_, _) => CmdDebuffList());

            helper.ConsoleCommands.Add(
                "injury_debuff_add",
                "Применить дебафф: injury_debuff_add <id> [минуты]. Минуты по умолчанию -2 (день). Пример: injury_debuff_add buffHurt",
                (_, args) => CmdDebuffAdd(args));

            helper.ConsoleCommands.Add(
                "injury_phase_list",
                "Список активных травм с фазой и флагами готовности к лечению.",
                (_, _) => CmdPhaseList());

            helper.ConsoleCommands.Add(
                "injury_phase_ready",
                "Готовность к смене фазы: injury_phase_ready <buffId> [1|0]. Пример: injury_phase_ready buffDeepCuts 1",
                (_, args) => CmdPhaseReady(args));

            helper.ConsoleCommands.Add(
                "injury_phase_recovery",
                "Готовность к выздоровлению: injury_phase_recovery <buffId> [1|0]. Пример: injury_phase_recovery buffDeepCuts 1",
                (_, args) => CmdPhaseRecovery(args));

            helper.ConsoleCommands.Add(
                "injury_phase_advance",
                "Переключить травму на следующую фазу (баффы и state). injury_phase_advance <buffId>. Пример: injury_phase_advance buffDeepCuts",
                (_, args) => CmdPhaseAdvance(args));

            helper.ConsoleCommands.Add(
                "injury_phase_cure",
                "Полное выздоровление от травмы (удалить состояние и баффы). injury_phase_cure <buffId>",
                (_, args) => CmdPhaseCure(args));

            helper.ConsoleCommands.Add(
                "injury_rain_debug",
                "Показать/изменить счётчики дождя: injury_rain_debug [secondsToday] [continuousSeconds]",
                (_, args) => CmdRainDebug(args));

            helper.ConsoleCommands.Add(
                "injury_mine_dirty_debug",
                "Показать состояние шахтного риска грязной раны (только чтение).",
                (_, _) => CmdMineDirtyDebug());

            helper.ConsoleCommands.Add(
                "injury_debug_mine_rescue",
                "Выставить флаги шахтного rescue для теста (сработает на следующий DayStarted).",
                (_, _) => CmdDebugMineRescue());

            helper.ConsoleCommands.Add(
                "injury_cooldowns",
                "Показать cooldown-память повторяемых травм.",
                (_, _) => CmdInjuryCooldowns());

            helper.ConsoleCommands.Add(
                "injury_farming_counters",
                "Показать счётчики использований инструментов для фермерских травм.",
                (_, _) => CmdFarmingCounters());

            helper.ConsoleCommands.Add(
                "injury_night_visit_reset",
                "Сбросить флаги ночного визита Харви за сегодня: LastNightRoundRollDay и LastNightRoundDay.",
                (_, _) => CmdNightVisitReset());

            helper.ConsoleCommands.Add(
                "injury_audit_content",
                "Диагностика: MailIds и topic keys против Data/Mail и диалогов Харви (только лог, без изменений).",
                (_, _) => CmdAuditContent());

            helper.ConsoleCommands.Add(
                "injury_debug_dump",
                "Полный диагностический отчёт в SMAPI log (то же, что full debug HUD, без обрезки).",
                (_, _) => CmdDebugDump());

            helper.ConsoleCommands.Add(
                "injury_medical_snapshot",
                "Снимок medical pipeline: decision/gate/pending, DebuffState, фазовые баффы (до/после клика по Харви).",
                (_, _) => CmdMedicalSnapshot());

            helper.ConsoleCommands.Add(
                "injury_foreign_topic_add",
                "Тест конфликта модов: добавить чужой conversation topic. injury_foreign_topic_add <topicId> [days]",
                (_, args) => CmdForeignTopicAdd(args));

            helper.ConsoleCommands.Add(
                "injury_proximity_test",
                "Отладка proximity-реплик из CP без изменения state. injury_proximity_test <situation> [tone]",
                (_, args) => CmdProximityTest(args));

            Monitor.Log("Harvey Overhaul: Injury & Care загружен", LogLevel.Info);
        }

        /// <summary>Травмы мода: buffId, топик, фазы (p1, p2, p3) дней.</summary>
        private static readonly (string BuffId, string TopicId, int P1, int P2, int P3)[] KnownTraumas =
        {
            ("buffHurt", "topicHurt", 3, 0, 0),
            ("buffBadlyHurt", "topicBadlyHurt", 3, 3, 2),
            ("buffSprainedAnkle", "topicSprainedAnkle", 7, 7, 0),
            ("buffBruisedRibs", "topicBruisedRibs", 10, 11, 0),
            ("buffBackStrain", "topicBackStrain", 5, 7, 0),
            ("buffDeepCuts", "topicDeepCuts", 3, 7, 4),
            ("buffBurnWounds", "topicBurnWounds", 7, 14, 0),
            ("buffInfectedWound", "topicInfectedWound", 3, 11, 0),
            ("buffTornMuscles", "topicTornMuscles", 7, 14, 7),
            ("buffConcussion", "topicConcussion", 3, 11, 7),
            ("buffFracturedBone", "topicFracturedBone", 7, 35, 14),
            ("buffShrapnelWounds", "topicShrapnelWounds", 5, 10, 7),
            ("buffSurgicalWound", "topicSurgicalWound", 14, 0, 0),
            (Core.InjuryBuffs.Cold, Core.ConversationTopics.Cold, 3, 4, 0),
        };

        /// <summary>Осложнения мода: buffId, топик.</summary>
        private static readonly (string BuffId, string TopicId)[] KnownComplications =
        {
            (Core.InjuryBuffs.WetBandage, Core.ConversationTopics.WetBandage),
            (Core.InjuryBuffs.DirtyWound, Core.ConversationTopics.DirtyWound),
            (Core.InjuryBuffs.WetStitches, Core.ConversationTopics.WetStitches),
            (Core.InjuryBuffs.Neglect, Core.ConversationTopics.Neglect),
            (Core.InjuryBuffs.AllergicRash, Core.ConversationTopics.AllergicRash),
            (Core.InjuryBuffs.PainFlare, Core.ConversationTopics.PainFlare),
        };

        private void CmdDebuffList()
        {
            Monitor.Log("=== Дебаффы мода (травмы) ===", LogLevel.Info);
            foreach (var t in KnownTraumas)
                Monitor.Log($"  {t.BuffId}  (топик: {t.TopicId}, фазы: {t.P1}/{t.P2}/{t.P3} д)", LogLevel.Info);
            Monitor.Log("=== Осложнения ===", LogLevel.Info);
            foreach (var c in KnownComplications)
                Monitor.Log($"  {c.BuffId}  (топик: {c.TopicId})", LogLevel.Info);
            Monitor.Log("Любой ID из Data/Buffs тоже можно применить через injury_debuff_add <id> [минуты].", LogLevel.Info);
        }

        private void CmdDebuffAdd(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }
            if (args.Length == 0)
            {
                Monitor.Log("Использование: injury_debuff_add <id> [минуты]. injury_debuff_list — список ID.", LogLevel.Info);
                return;
            }
            string id = args[0].Trim();
            int minutes = -2;
            if (args.Length >= 2 && int.TryParse(args[1], out int m))
                minutes = m;

            var trauma = KnownTraumas.FirstOrDefault(t => t.BuffId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (trauma.BuffId != null)
            {
                int today = (int)Game1.stats.DaysPlayed;
                _buffManager.AddBuff(trauma.BuffId, minutes);
                _stateManager.CreateDebuffState(trauma.BuffId, today, trauma.P1, trauma.P2, trauma.P3);
                int topicDays = trauma.P1 + trauma.P2 + trauma.P3;
                if (topicDays <= 0) topicDays = 7;
                _dialogueManager.AddTopic(trauma.TopicId, topicDays);
                _dialogueManager.TryAddHarveyNeedsFirstTreatmentTopic(trauma.BuffId);
                Monitor.Log($"Применена травма: {trauma.BuffId}, топик {trauma.TopicId} на {topicDays} д.", LogLevel.Info);
                Game1.addHUDMessage(new HUDMessage($"[ДЕБАГ] Дебафф: {trauma.BuffId}", HUDMessage.achievement_type));
                return;
            }

            var comp = KnownComplications.FirstOrDefault(c => c.BuffId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (comp.BuffId != null)
            {
                int today = (int)Game1.stats.DaysPlayed;
                _buffManager.AddBuff(comp.BuffId, minutes);
                _stateManager.State.ActiveComplications[comp.BuffId] = today;
                _stateManager.CreateComplicationState(comp.BuffId, today);
                _dialogueManager.AddTopic(comp.TopicId, 4);
                Monitor.Log($"Применено осложнение: {comp.BuffId}, топик {comp.TopicId}.", LogLevel.Info);
                Game1.addHUDMessage(new HUDMessage($"[ДЕБАГ] Осложнение: {comp.BuffId}", HUDMessage.achievement_type));
                return;
            }

            // Неизвестный ID — только бафф (должен быть в Data/Buffs)
            if (!_buffManager.BuffExists(id))
                Monitor.Log($"ID «{id}» не в списке мода и не найден в Data/Buffs. Всё равно применяю бафф.", LogLevel.Warn);
            _buffManager.AddBuff(id, minutes);
            Monitor.Log($"Применён бафф: {id} на {(minutes == -2 ? "день" : minutes + " мин")}.", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[ДЕБАГ] Бафф: {id}", HUDMessage.achievement_type));
        }

        private void CmdPhaseList()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }
            var all = _stateManager.GetAllActiveDebuffStates();
            if (all.Count == 0)
            {
                Monitor.Log("Нет активных дебаффов в состоянии мода. injury_debuff_add <id> — применить травму.", LogLevel.Info);
                return;
            }
            Monitor.Log("=== Активные травмы (фаза, готовность) ===", LogLevel.Info);
            foreach (var ds in all)
            {
                string phaseInfo = ds.TotalPhases == 0
                    ? "не фазовая"
                    : $"фаза {ds.CurrentPhase}/{ds.TotalPhases} (с дня {ds.PhaseStartDay})";
                string flags = "";
                if (ds.ReadyForNextPhase) flags += " [→след.фаза]";
                if (ds.ReadyForRecovery) flags += " [→выздоровление]";
                if (ds.TreatmentStarted) flags += " в лечении";
                Monitor.Log($"  {ds.BuffId}: {phaseInfo}{flags}", LogLevel.Info);
            }
            Monitor.Log("Команды: injury_phase_ready <id> [1|0], injury_phase_recovery <id> [1|0], injury_phase_advance <id>, injury_phase_cure <id>", LogLevel.Info);
        }

        private static bool ParseBoolArg(string[] args, int index, bool defaultVal)
        {
            if (args.Length <= index) return defaultVal;
            return args[index].Trim() switch { "1" => true, "0" => false, _ => defaultVal };
        }

        private void CmdPhaseReady(string[] args)
        {
            if (!Context.IsWorldReady) { Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn); return; }
            if (args.Length == 0) { Monitor.Log("Использование: injury_phase_ready <buffId> [1|0]. injury_phase_list — список.", LogLevel.Info); return; }
            string id = args[0].Trim();
            if (!_stateManager.State.ActiveDebuffs.TryGetValue(id, out _))
            {
                Monitor.Log($"Дебафф «{id}» не найден в состоянии. injury_phase_list — список активных.", LogLevel.Warn);
                return;
            }
            bool ready = ParseBoolArg(args, 1, true);
            _stateManager.SetReadyForNextPhase(id, ready);
            Monitor.Log($"ReadyForNextPhase({id}) = {ready}. Клик по Харви откроет смену фазы.", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[Фаза] {id}: готовность к смене фазы = {(ready ? "да" : "нет")}", HUDMessage.health_type));
        }

        private void CmdPhaseRecovery(string[] args)
        {
            if (!Context.IsWorldReady) { Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn); return; }
            if (args.Length == 0) { Monitor.Log("Использование: injury_phase_recovery <buffId> [1|0]. injury_phase_list — список.", LogLevel.Info); return; }
            string id = args[0].Trim();
            if (!_stateManager.State.ActiveDebuffs.TryGetValue(id, out _))
            {
                Monitor.Log($"Дебафф «{id}» не найден в состоянии. injury_phase_list — список активных.", LogLevel.Warn);
                return;
            }
            bool ready = ParseBoolArg(args, 1, true);
            _stateManager.SetReadyForRecovery(id, ready);
            Monitor.Log($"ReadyForRecovery({id}) = {ready}. Клик по Харви откроет финальный осмотр.", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[Фаза] {id}: готовность к выздоровлению = {(ready ? "да" : "нет")}", HUDMessage.health_type));
        }

        private void CmdPhaseAdvance(string[] args)
        {
            if (!Context.IsWorldReady) { Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn); return; }
            if (args.Length == 0) { Monitor.Log("Использование: injury_phase_advance <buffId>. Переключает на следующую фазу (баффы + state).", LogLevel.Info); return; }
            string id = args[0].Trim();
            var ds = _stateManager.GetDebuffState(id);
            if (ds == null)
            {
                Monitor.Log($"Дебафф «{id}» не найден. injury_phase_list — список.", LogLevel.Warn);
                return;
            }
            if (ds.TotalPhases == 0 || ds.CurrentPhase >= ds.TotalPhases)
            {
                Monitor.Log($"Травма «{id}» не фазовая или уже на последней фазе. Используйте injury_phase_cure для полного выздоровления.", LogLevel.Warn);
                return;
            }
            int oldPhase = ds.CurrentPhase;
            int nextPhase = oldPhase + 1;
            _treatmentManager.AdvanceInjuryToNextPhase(id);
            var updated = _stateManager.GetDebuffState(id);
            int actualPhase = updated?.CurrentPhase ?? nextPhase;
            Monitor.Log($"Фаза «{id}» переключена: {oldPhase} → {actualPhase}.", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[Фаза] {id}: переход на фазу {actualPhase}", HUDMessage.health_type));
        }

        private void CmdPhaseCure(string[] args)
        {
            if (!Context.IsWorldReady) { Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn); return; }
            if (args.Length == 0) { Monitor.Log("Использование: injury_phase_cure <buffId>. Полное выздоровление (удаление состояния и баффов).", LogLevel.Info); return; }
            string id = args[0].Trim();
            if (_stateManager.GetDebuffState(id) == null)
            {
                Monitor.Log($"Дебафф «{id}» не найден. injury_phase_list — список.", LogLevel.Warn);
                return;
            }
            _treatmentManager.CompleteInjuryRecovery(id);
            Monitor.Log($"Выздоровление от «{id}» применено.", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[Фаза] {id}: полное выздоровление", HUDMessage.achievement_type));
        }

        private void CmdRainDebug(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length >= 1 && int.TryParse(args[0], out int secondsToday))
                _stateManager.State.TotalTimeUnderRainToday = Math.Max(0, secondsToday);

            if (args.Length >= 2 && int.TryParse(args[1], out int continuousSeconds))
                _stateManager.State.TimeUnderRainTicks = Math.Max(0, continuousSeconds);

            _stateManager.State.LastRainDay = (int)Game1.stats.DaysPlayed;

            Monitor.Log(
                $"Rain debug: TotalTimeUnderRainToday={_stateManager.State.TotalTimeUnderRainToday}s, " +
                $"TimeUnderRainTicks={_stateManager.State.TimeUnderRainTicks}s, " +
                $"LastRainDay={_stateManager.State.LastRainDay}",
                LogLevel.Info);

            Game1.addHUDMessage(new HUDMessage(
                $"Rain: {_stateManager.State.TotalTimeUnderRainToday}s / bandage {_stateManager.State.TimeUnderRainTicks}s",
                HUDMessage.health_type));
        }

        private void CmdMineDirtyDebug()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            var state = _stateManager.State;
            string loc = Game1.currentLocation?.Name ?? "(null)";

            bool hasDirtyInjury = false;
            foreach (var injuryId in InjurySets.DirtyInMines)
            {
                if (_buffManager.HasBuff(injuryId))
                {
                    hasDirtyInjury = true;
                    break;
                }
            }

            bool hasDirtyWound = _buffManager.HasBuff(InjuryBuffs.DirtyWound)
                || state.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound);

            Monitor.Log(
                $"[MineDirtyDebug] loc={loc}, exposure={state.MineDirtyExposureMinutesToday}m, " +
                $"lastExposureDay={state.LastMineDirtyExposureDay}, lastRoll={state.LastMineDirtyWoundRollMinute}, " +
                $"boostUntil={state.MineDirtyRiskBoostUntilMinute}, hasDirtyInjury={hasDirtyInjury}, hasDirtyWound={hasDirtyWound}",
                LogLevel.Info);
        }

        private void CmdDebugMineRescue()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _stateManager.State.WasPassedOut = true;
            _stateManager.State.WasExhausted = false;
            _stateManager.State.WasUpTooLate = false;
            _stateManager.State.LastPassedOutHealth = 0;
            _stateManager.State.LastPassedOutLocation = "Mine";
            _stateManager.State.PassedOutInMineYesterday = true;
            _stateManager.State.NeedsMineRescueEvent = true;

            _injuryManager.ApplyBadlyHurtFromMinePassOut();
            _stateManager.Save();

            Monitor.Log("[MineRescue][Debug] Флаги rescue выставлены вручную", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("[ДЕБАГ] Mine rescue будет запущен утром", HUDMessage.achievement_type));
        }

        private void CmdInjuryCooldowns()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            int today = (int)Game1.stats.DaysPlayed;
            var map = _stateManager.State.InjuryCooldownUntilDay;

            if (map == null || map.Count == 0)
            {
                Monitor.Log("Injury cooldowns: записей нет.", LogLevel.Info);
                return;
            }

            Monitor.Log("=== Injury cooldowns (until day) ===", LogLevel.Info);
            foreach (var kv in map.OrderBy(k => k.Key))
            {
                string status = today < kv.Value ? $"активен ({kv.Value - today} д.)" : "истёк";
                Monitor.Log($"  {kv.Key}: до дня {kv.Value} ({status})", LogLevel.Info);
            }
        }

        private void CmdFarmingCounters()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            string debug = _playerEventHandler.GetFarmingToolCountersDebug();
            Monitor.Log($"[FarmingInjury] {debug}", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"Farming counters: {debug}", HUDMessage.newQuest_type));
        }

        private void CmdNightVisitReset()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _stateManager.State.LastNightRoundRollDay = -1;
            _stateManager.State.LastNightRoundDay = -1;
            _stateManager.Save();

            Monitor.Log("Флаги ночного визита Харви сброшены: LastNightRoundRollDay=-1, LastNightRoundDay=-1.", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("[ДЕБАГ] Ночной визит Харви сброшен.", HUDMessage.achievement_type));
        }

        private void CmdAuditContent()
        {
            ContentAuditRunner.Run(Helper, Monitor, KnownTraumas, KnownComplications);
        }

        private void CmdDebugDump()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log(BuildDebugReport(full: true), LogLevel.Info);
        }

        private void CmdMedicalSnapshot()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log("=== MEDICAL PIPELINE SNAPSHOT ===", LogLevel.Info);
            Monitor.Log($"Pending: {_interactionHandler.GetPendingMedicalActionSummary() ?? "none"}", LogLevel.Info);
            Monitor.Log($"Decision now: {_interactionHandler.BuildDebugTreatmentDecision()}", LogLevel.Info);
            Monitor.Log($"Standard dialogue: {_interactionHandler.GetStandardDialogueGateReason()}", LogLevel.Info);
            Monitor.Log($"Last click: {_interactionHandler.LastClickDebug ?? "-"}", LogLevel.Info);

            foreach (var ds in _stateManager.GetAllActiveDebuffStates()
                         .Where(d => InjurySets.HarveyTreatable.Contains(d.BuffId))
                         .OrderByDescending(d => d.BuffId))
            {
                Monitor.Log(
                    $"  DebuffState {ds.BuffId}: phase={ds.CurrentPhase}/{ds.TotalPhases} " +
                    $"TreatmentStarted={ds.TreatmentStarted} ReadyNext={ds.ReadyForNextPhase} ReadyRecovery={ds.ReadyForRecovery}",
                    LogLevel.Info);
                for (int p = 1; p <= 3; p++)
                {
                    string phaseBuff = _injuryManager.GetPhaseBuffId(ds.BuffId, p);
                    if (_buffManager.HasBuff(phaseBuff))
                        Monitor.Log($"    buff ACTIVE: {phaseBuff}", LogLevel.Info);
                }
                if (_buffManager.HasBuff(ds.BuffId))
                    Monitor.Log($"    buff ACTIVE: {ds.BuffId} (base)", LogLevel.Info);
            }

            foreach (string compId in InjurySets.KnownComplicationBuffIds)
            {
                if (_stateManager.GetDebuffState(compId) != null || _buffManager.HasBuff(compId))
                    Monitor.Log($"  Complication: {compId} buff={_buffManager.HasBuff(compId)}", LogLevel.Info);
            }

            if (_buffManager.HasBuff(CureBuffs.Care))
                Monitor.Log($"  Care buff: ACTIVE ({CureBuffs.Care})", LogLevel.Info);

            Monitor.Log("=== END SNAPSHOT ===", LogLevel.Info);
        }

        private void CmdForeignTopicAdd(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: injury_foreign_topic_add <topicId> [days]. Пример: injury_foreign_topic_add topic_joja_Certified 5", LogLevel.Info);
                return;
            }

            string topicId = args[0].Trim();
            int days = 5;
            if (args.Length >= 2 && int.TryParse(args[1], out int d))
                days = Math.Max(1, d);

            if (ModTopicRegistry.GetAllOwnedTopicIds().Contains(topicId))
            {
                Monitor.Log($"⚠️ {topicId} — топик InjuryCare. Для теста конфликта используйте чужой ID (не из ModTopicRegistry).", LogLevel.Warn);
            }

            Game1.player.activeDialogueEvents[topicId] = days;
            Monitor.Log($"Добавлен foreign topic: {topicId} на {days} д. (не удаляется injury_reset, только вручную или save reload)", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[TEST] foreign topic: {topicId}", HUDMessage.newQuest_type));
        }

        private void CmdProximityTest(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_proximity_test <situation> [tone]. " +
                    "tone: Low | Mid | High | Romantic (по умолчанию — текущие отношения с Харви).",
                    LogLevel.Info);
                Monitor.Log("situation: untreated | intreatment | readyphase | recovery | wetbandage | dirtywound | " +
                            "wetstitches | allergicrash | painflare | neglect | generic | multiple", LogLevel.Info);
                Monitor.Log("Примеры: injury_proximity_test untreated Romantic | injury_proximity_test wetbandage High", LogLevel.Info);
                return;
            }

            string situationKey = args[0].Trim();
            if (!TryMapProximityTestSituation(situationKey, out string primaryPrefixBase))
            {
                Monitor.Log(
                    $"Неизвестная situation «{situationKey}». " +
                    "untreated, intreatment, readyphase, recovery, wetbandage, dirtywound, wetstitches, " +
                    "allergicrash, painflare, neglect, generic, multiple.",
                    LogLevel.Warn);
                return;
            }

            string tone;
            if (args.Length >= 2)
            {
                tone = NormalizeProximityTestTone(args[1]) ?? string.Empty;
                if (string.IsNullOrEmpty(tone))
                {
                    Monitor.Log("tone должен быть Low, Mid, High или Romantic.", LogLevel.Warn);
                    return;
                }
            }
            else
            {
                tone = HarveyHelper.GetRelationshipToneWithHarvey();
            }

            string primaryPrefix = $"{primaryPrefixBase}_{tone}";
            var prefixes = ProximityReactionManager.BuildPrefixCandidates(primaryPrefix);
            string text = _dialogueManager.PickRandomProximityLineByPrefixes(prefixes);

            bool usedFallback = string.Equals(text, DialogueManager.ProximityDialogueFallback, StringComparison.Ordinal);

            Monitor.Log("=== PROXIMITY TEST (debug) ===", LogLevel.Info);
            Monitor.Log($"situation={situationKey} tone={tone}", LogLevel.Info);
            Monitor.Log($"primary: {primaryPrefix}", LogLevel.Info);
            Monitor.Log($"fallback chain: {string.Join(" → ", prefixes)}", LogLevel.Info);
            Monitor.Log($"result: {text}", LogLevel.Info);
            if (usedFallback)
                Monitor.Log("CP: совпадений нет, использован defaultText.", LogLevel.Warn);
            Monitor.Log("=== END PROXIMITY TEST ===", LogLevel.Info);

            string hudText = text.Length > 120 ? text[..117] + "…" : text;
            Game1.addHUDMessage(new HUDMessage($"[proximity] {hudText}", HUDMessage.achievement_type));

            NPC? harvey = Game1.currentLocation?.getCharacterFromName("Harvey");
            if (harvey != null)
            {
                int emote = GetProximityTestEmote(situationKey);
                _dialogueManager.ShowEmoteWithText(harvey, emote, text);
                Monitor.Log("Харви в текущей локации — показано ShowEmoteWithText.", LogLevel.Info);
            }
            else
            {
                Monitor.Log("Харви не в текущей локации — только log + HUD.", LogLevel.Info);
            }
        }

        private static bool TryMapProximityTestSituation(string situation, out string prefixBase)
        {
            prefixBase = situation.Trim().ToLowerInvariant() switch
            {
                "untreated" => "Proximity_Injury_Untreated",
                "intreatment" or "in_treatment" or "in-treatment" => "Proximity_Injury_InTreatment",
                "readyphase" or "ready_phase" or "ready-phase" or "phaseready" => "Proximity_Phase_ReadyNextPhase",
                "recovery" or "readyrecovery" or "ready_recovery" => "Proximity_Recovery_ReadyRecovery",
                "wetbandage" or "wet_bandage" => "Proximity_Complication_WetBandage",
                "dirtywound" or "dirty_wound" => "Proximity_Complication_DirtyWound",
                "wetstitches" or "wet_stitches" => "Proximity_Complication_WetStitches",
                "allergicrash" or "allergic_rash" => "Proximity_Complication_AllergicRash",
                "painflare" or "pain_flare" => "Proximity_Complication_PainFlare",
                "neglect" => "Proximity_Complication_Neglect",
                "generic" => "Proximity_Complication_Generic",
                "multiple" => "Proximity_Complication_Multiple",
                _ => string.Empty
            };

            return !string.IsNullOrEmpty(prefixBase);
        }

        private static string? NormalizeProximityTestTone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            string normalized = raw.Trim();
            if (normalized.Equals("low", StringComparison.OrdinalIgnoreCase)) return "Low";
            if (normalized.Equals("mid", StringComparison.OrdinalIgnoreCase)) return "Mid";
            if (normalized.Equals("high", StringComparison.OrdinalIgnoreCase)) return "High";
            if (normalized.Equals("romantic", StringComparison.OrdinalIgnoreCase)) return "Romantic";
            return null;
        }

        private static int GetProximityTestEmote(string situation)
        {
            return situation.Trim().ToLowerInvariant() switch
            {
                "wetbandage" or "wet_bandage" or "wetstitches" or "wet_stitches"
                    => HarveyEmotes.WorriedAboutPatient,
                "dirtywound" or "dirty_wound" => HarveyEmotes.DirtyWound,
                "recovery" or "readyrecovery" or "ready_recovery"
                    or "readyphase" or "ready_phase" or "ready-phase" or "phaseready"
                    => HarveyEmotes.Thinking,
                "intreatment" or "in_treatment" or "in-treatment" => HarveyHelper.GetCaringEmote(),
                "generic" or "multiple" or "neglect" or "allergicrash" or "allergic_rash"
                    or "painflare" or "pain_flare"
                    => HarveyEmotes.FoundComplication,
                _ => HarveyEmotes.FindInjury
            };
        }

        /// <summary>
        /// Полный сброс данных мода для отладки.
        /// Удаляет все баффы мода, осложнения, топики и очищает _state.
        /// </summary>
        private void FullReset()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Нельзя сбросить данные до загрузки сохранения.", LogLevel.Warn);
                return;
            }

            var state = _stateManager.State;

            // Удалить все баффы из ActiveDebuffs (травмы + фазовые + осложнения)
            foreach (var (buffId, ds) in state.ActiveDebuffs)
            {
                _buffManager.RemoveBuff(buffId);
                if (ds.TotalPhases > 0)
                {
                    for (int p = 1; p <= 3; p++)
                        _buffManager.RemoveBuff(_injuryManager.GetPhaseBuffId(buffId, p));
                }
            }

            // Удалить все осложнения
            foreach (var compId in state.ActiveComplications.Keys.ToList())
                _buffManager.RemoveBuff(compId);

            // Удалить все лечебные баффы
            foreach (var cureBuff in new[]
            {
                Core.CureBuffs.Treatment, Core.CureBuffs.IntensiveCare, Core.CureBuffs.Protection,
                Core.CureBuffs.Recovery,  Core.CureBuffs.Teracitin,    Core.CureBuffs.Antibiotics,
                Core.CureBuffs.ForcedSedation, Core.CureBuffs.PostSurgical, Core.CureBuffs.Care,
            })
            {
                _buffManager.RemoveBuff(cureBuff);
            }

            // Удалить только топики InjuryCare (не чужие topic* / situation* из других модов)
            if (Game1.player?.activeDialogueEvents != null)
            {
                var ownedTopics = Core.ModTopicRegistry.GetAllOwnedTopicIds();
                var modTopics = Game1.player.activeDialogueEvents.Keys
                    .Where(k => ownedTopics.Contains(k))
                    .ToList();
                foreach (var topic in modTopics)
                    Game1.player.activeDialogueEvents.Remove(topic);
                Monitor.Log($"Удалено {modTopics.Count} топиков InjuryCare.", LogLevel.Info);
            }

            // Очистить _state
            _stateManager.Clear();

            Monitor.Log("=== ПОЛНЫЙ СБРОС ВЫПОЛНЕН ===", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("[ДЕБАГ] Все данные мода сброшены.", HUDMessage.error_type));
        }

        /// <summary>
        /// Инициализировать все менеджеры
        /// </summary>
        private void InitializeManagers()
        {
            // Порядок важен - базовые менеджеры создаются первыми
            _stateManager = new StateManager(Helper.Data, Monitor);
            _buffManager = new BuffManager(Monitor, Helper);
            _dialogueManager = new DialogueManager(Monitor);

            // Загрузить данные баффов
            _buffManager.LoadBuffData();
            
            // HospitalizationManager (будет обновлён позже с TreatmentManager)
            _hospitalizationManager = new HospitalizationManager(Monitor, _config, _dialogueManager);
            
            // HospitalActivityManager - интерактивная госпитализация
            _hospitalActivityManager = new HospitalActivityManager(Monitor, _dialogueManager);
            
            // Связываем менеджеры
            _hospitalizationManager.SetActivityManager(_hospitalActivityManager);
            
            // InjuryManager с полным набором зависимостей
            _injuryManager = new InjuryManager(
                Monitor, 
                _stateManager, 
                _buffManager,
                _dialogueManager,
                _hospitalizationManager,
                _config
            );
            
            // TreatmentManager
            _treatmentManager = new TreatmentManager(Monitor, _buffManager, _injuryManager, _dialogueManager, _stateManager);

            _proximityReactionManager = new ProximityReactionManager(_buffManager, _stateManager, _injuryManager);

            // ComplicationManager - управление осложнениями
            _complicationManager = new ComplicationManager(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _injuryManager
            );

            Monitor.Log("Все менеджеры инициализированы", LogLevel.Debug);
        }

        /// <summary>
        /// Подписаться на игровые события
        /// </summary>
        private void SubscribeToEvents(IModEvents events)
        {
            // Создать обработчики событий
            _gameEventHandler = new GameEventHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _treatmentManager,
                _hospitalizationManager,
                _dialogueManager,
                _complicationManager
            );

            _playerEventHandler = new PlayerEventHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _treatmentManager,
                _hospitalizationManager,
                _dialogueManager,
                _proximityReactionManager
            );

            _interactionHandler = new InteractionHandler(
                Monitor,
                Helper,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _dialogueManager,
                _treatmentManager
            );

            _timeEventHandler = new TimeEventHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _hospitalizationManager,
                _hospitalActivityManager,
                _treatmentManager
            );

            _passOutHandler = new PassOutHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _injuryManager,
                _treatmentManager
            );

            // События сохранения
            events.GameLoop.SaveLoaded += OnSaveLoaded;
            events.GameLoop.Saving += OnSaving;

            // События дня
            events.GameLoop.DayStarted += _gameEventHandler.OnDayStarted;
            events.GameLoop.DayEnding += _gameEventHandler.OnDayEnding;

            // Связываем обработчики
            _gameEventHandler.SetInteractionHandler(_interactionHandler);
            _gameEventHandler.SetPassOutHandler(_passOutHandler);
            _playerEventHandler.SetPassOutHandler(_passOutHandler);

            // События игрока
            events.Player.Warped += _playerEventHandler.OnWarped;
            events.GameLoop.UpdateTicked += _playerEventHandler.OnUpdateTicked;
            events.GameLoop.TimeChanged += _playerEventHandler.OnTimeChanged;

            // События обморока
            events.Player.Warped += _passOutHandler.OnPlayerWarped;
            events.GameLoop.UpdateTicked += _passOutHandler.OnUpdateTicked;
            events.GameLoop.DayEnding += OnDayEndingPassOutCheck;

            // События взаимодействия
            events.Input.ButtonPressed += _interactionHandler.OnButtonPressed;
            events.GameLoop.UpdateTicked += _interactionHandler.OnUpdateTicked;

            // События времени
            events.GameLoop.TimeChanged += _timeEventHandler.OnTimeChanged;

            // Дебаг: вывод _state на экран (переключение по F10)
            events.Display.RenderedHud += OnRenderedHudDebugState;
            events.Input.ButtonPressed += OnDebugHudToggleKeyPressed;

            // Без цветного свечения персонажа от модовых баффов/дебаффов
            events.Content.AssetRequested += OnAssetRequested;

            Monitor.Log("Подписка на события завершена", LogLevel.Debug);
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Buffs"))
                return;

            e.Edit(asset =>
            {
                ModBuffGlow.StripGlowFromBuffData(asset.AsDictionary<string, BuffData>());
            });
        }

        private string YesNo(bool value) => value ? "YES" : "no";

        private string Warn(bool ok, string text) => ok ? text : $"⚠ {text}";

        private string GetBaseTopicForBuff(string buffId)
        {
            var trauma = KnownTraumas.FirstOrDefault(t => t.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));
            if (trauma.BuffId != null)
                return trauma.TopicId;

            var comp = KnownComplications.FirstOrDefault(c => c.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));
            if (comp.BuffId != null)
                return comp.TopicId;

            return buffId.Replace("buff", "topic");
        }

        private string GetPhaseName(int phase) => phase switch
        {
            1 => "Acute",
            2 => "Healing",
            3 => "Recovery",
            _ => "Unknown",
        };

        private string GetExpectedPhaseTopic(string buffId, int phase) =>
            _injuryManager.GetPhaseTopicId(buffId, phase);

        private string GetExpectedPhaseBuff(string buffId, int phase) =>
            phase > 0 ? _injuryManager.GetPhaseBuffId(buffId, phase) : "";

        private bool HasTopic(string topicId) =>
            Game1.player.activeDialogueEvents?.ContainsKey(topicId) == true;

        private string TopicLeft(string topicId)
        {
            if (!HasTopic(topicId))
                return "missing";

            Game1.player.activeDialogueEvents!.TryGetValue(topicId, out int days);
            return $"{days}d";
        }

        private string GetClickExpectationForDebuff(DebuffState d)
        {
            if (!d.TreatmentStarted && _buffManager.HasBuff(d.BuffId))
                return "CLICK: start treatment";
            if (d.TreatmentStarted && d.ReadyForRecovery)
                return "CLICK: complete recovery";
            if (d.TreatmentStarted && d.ReadyForNextPhase)
                return "CLICK: next phase";
            if (d.TreatmentStarted)
                return "WAIT";
            return "BROKEN";
        }

        private bool IsKnownComplication(string buffId) =>
            KnownComplications.Any(c => c.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));

        private string GetInjuryStateLabel(DebuffState d)
        {
            if (IsKnownComplication(d.BuffId))
                return "complication";
            if (!d.TreatmentStarted)
                return "untreated";
            if (d.IsPhasedInjury && d.CurrentPhase > 0)
                return $"treatment phase {d.CurrentPhase}/{d.TotalPhases} {GetPhaseName(d.CurrentPhase)}";
            return "simple-treatment";
        }

        private string FormatTopicStatus(string topicId) =>
            HasTopic(topicId) ? $"{topicId} {TopicLeft(topicId)}" : $"{topicId} missing";

        private List<string> CollectDebuffIssues(
            DebuffState d,
            bool hasBaseBuff,
            string phaseBuffExpected,
            bool hasPhaseBuff)
        {
            var issues = new List<string>();
            bool expectPhaseBuff = d.TreatmentStarted && d.CurrentPhase > 0 && d.IsPhasedInjury;
            string baseTopic = GetBaseTopicForBuff(d.BuffId);

            if (!hasBaseBuff && !hasPhaseBuff)
                issues.Add("STATE_WITHOUT_BUFF");

            if (!d.TreatmentStarted && !hasBaseBuff)
                issues.Add("UNTREATED_WITHOUT_BASE_BUFF");

            if (d.TreatmentStarted && d.CurrentPhase <= 0)
                issues.Add("TREATMENT_STARTED_PHASE_ZERO");

            if (d.TreatmentStarted && d.TotalPhases > 0 && d.CurrentPhase > d.TotalPhases)
                issues.Add("PHASE_OUT_OF_RANGE");

            if (expectPhaseBuff && !string.IsNullOrEmpty(phaseBuffExpected) && !hasPhaseBuff)
                issues.Add("PHASE_BUFF_MISSING");

            if (d.ReadyForNextPhase && d.ReadyForRecovery)
                issues.Add("BOTH_READY_FLAGS");

            if (d.ReadyForNextPhase && d.IsLastPhase)
                issues.Add("NEXT_READY_ON_LAST_PHASE");

            if (d.ReadyForRecovery && !d.IsLastPhase)
                issues.Add("RECOVERY_READY_BEFORE_LAST_PHASE");

            if (!d.TreatmentStarted && !HasTopic(baseTopic))
                issues.Add("BASE_TOPIC_MISSING");

            if (d.TreatmentStarted && d.IsPhasedInjury && d.CurrentPhase > 0)
            {
                string phaseTopic = GetExpectedPhaseTopic(d.BuffId, d.CurrentPhase);
                if (!HasTopic(phaseTopic))
                    issues.Add("PHASE_TOPIC_MISSING");
            }

            return issues;
        }

        private bool IsKnownTraumaActiveInMatrix(
            (string BuffId, string TopicId, int P1, int P2, int P3) trauma,
            InjuryState state)
        {
            if (state.ActiveDebuffs.ContainsKey(trauma.BuffId))
                return true;

            if (_buffManager.HasBuff(trauma.BuffId))
                return true;

            for (int phase = 1; phase <= 3; phase++)
            {
                string phaseBuff = GetExpectedPhaseBuff(trauma.BuffId, phase);
                if (!string.IsNullOrEmpty(phaseBuff) && _buffManager.HasBuff(phaseBuff))
                    return true;
            }

            if (HasTopic(trauma.TopicId))
                return true;

            for (int phase = 1; phase <= 3; phase++)
            {
                if (HasTopic(GetExpectedPhaseTopic(trauma.BuffId, phase)))
                    return true;
            }

            string curedTopic = $"topic{trauma.BuffId.Replace("buff", "")}Cured";
            return HasTopic(curedTopic);
        }

        private void AppendKnownTraumasMatrixBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== KNOWN TRAUMAS MATRIX ===");
            bool any = false;

            foreach (var trauma in KnownTraumas)
            {
                if (!IsKnownTraumaActiveInMatrix(trauma, state))
                    continue;

                any = true;
                bool hasState = state.ActiveDebuffs.ContainsKey(trauma.BuffId);
                bool hasBaseBuff = _buffManager.HasBuff(trauma.BuffId);
                string? triggerMatch = FindMatchingAppliedTrigger(trauma.BuffId, trauma.TopicId, state.AppliedTriggers);
                string triggerHint = triggerMatch != null ? $"trigger {triggerMatch} maybe" : "trigger no";

                sb.AppendLine(
                    $"{trauma.BuffId} | state {YesNo(hasState)} | baseBuff {YesNo(hasBaseBuff)} | topic {TopicLeft(trauma.TopicId)} | phases {trauma.P1}/{trauma.P2}/{trauma.P3} | {triggerHint}");

                if (trauma.P1 + trauma.P2 + trauma.P3 <= 0)
                    continue;

                int[] phaseDurations = { trauma.P1, trauma.P2, trauma.P3 };
                for (int phase = 1; phase <= 3; phase++)
                {
                    if (phaseDurations[phase - 1] <= 0)
                        continue;

                    string phaseBuff = GetExpectedPhaseBuff(trauma.BuffId, phase);
                    string phaseTopic = GetExpectedPhaseTopic(trauma.BuffId, phase);
                    sb.AppendLine(
                        $"  P{phase} {phaseBuff} buff {YesNo(_buffManager.HasBuff(phaseBuff))} topic {phaseTopic} {TopicLeft(phaseTopic)}");
                }
            }

            if (!any)
                sb.AppendLine("  (none active)");
        }

        private void AppendInjuriesDiagnosticBlock(StringBuilder sb, InjuryState state, int today)
        {
            sb.AppendLine("=== INJURIES ===");
            if (state.ActiveDebuffs.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (buffId, d) in state.ActiveDebuffs)
            {
                bool hasBaseBuff = _buffManager.HasBuff(buffId);
                string phaseBuffExpected = GetExpectedPhaseBuff(buffId, d.CurrentPhase);
                bool hasPhaseBuff = !string.IsNullOrEmpty(phaseBuffExpected) && _buffManager.HasBuff(phaseBuffExpected);
                var issues = CollectDebuffIssues(d, hasBaseBuff, phaseBuffExpected, hasPhaseBuff);
                string status = issues.Count == 0 ? "OK" : "WARN";

                sb.AppendLine($"[{status}] {buffId}");
                sb.AppendLine($"  state: {GetInjuryStateLabel(d)}");

                int injuryDays = today - d.InjuryStartDay;
                if (d.TreatmentStarted && d.CurrentPhase > 0)
                {
                    int phaseDays = today - d.PhaseStartDay;
                    int needDays = d.GetCurrentPhaseDuration();
                    sb.AppendLine($"  days: injury +{injuryDays}, phase +{phaseDays} / need {needDays}");
                }
                else
                {
                    sb.AppendLine($"  days: injury +{injuryDays}");
                }

                string phaseBuffLabel = string.IsNullOrEmpty(phaseBuffExpected) ? "n/a" : phaseBuffExpected;
                sb.AppendLine($"  buffs: base {YesNo(hasBaseBuff)}, phase {phaseBuffLabel} {YesNo(hasPhaseBuff)}");

                string baseTopic = GetBaseTopicForBuff(buffId);
                string phaseTopicExpected = d.CurrentPhase > 0
                    ? GetExpectedPhaseTopic(buffId, d.CurrentPhase)
                    : "";
                if (!string.IsNullOrEmpty(phaseTopicExpected))
                {
                    sb.AppendLine($"  topics: base {FormatTopicStatus(baseTopic)}, phase {FormatTopicStatus(phaseTopicExpected)}");
                }
                else
                {
                    sb.AppendLine($"  topics: base {FormatTopicStatus(baseTopic)}");
                }

                sb.AppendLine($"  flags: Next {YesNo(d.ReadyForNextPhase)}, Recovery {YesNo(d.ReadyForRecovery)}, HarveyTalk {YesNo(d.HarveyConversationHappened)}");
                sb.AppendLine($"  click: {GetClickExpectationForDebuff(d)}");
                sb.AppendLine(issues.Count == 0
                    ? "  issues: none"
                    : $"  issues: {string.Join(", ", issues)}");
            }
        }

        private static bool IsModConversationTopic(string topicId) =>
            topicId.StartsWith("topic", StringComparison.Ordinal)
            || topicId.StartsWith("situation", StringComparison.Ordinal)
            || topicId.StartsWith("HarveyMod_CD_", StringComparison.Ordinal);

        private List<string> CollectActiveModTopics()
        {
            var dict = Game1.player?.activeDialogueEvents;
            if (dict == null)
                return new List<string>();

            return dict.Keys
                .Where(IsModConversationTopic)
                .OrderBy(t => t, StringComparer.Ordinal)
                .ToList();
        }

        private void AppendTopicsWatchedBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== ACTIVE TOPICS ===");
            var active = CollectActiveModTopics();
            if (active.Count == 0)
            {
                sb.AppendLine("(none)");
                return;
            }

            sb.AppendLine($"count: {active.Count}");
            var dict = Game1.player!.activeDialogueEvents!;
            foreach (var topicId in active)
            {
                dict.TryGetValue(topicId, out int days);
                sb.AppendLine($"{topicId} ({days}d)");
            }
        }

        private static readonly string[] WatchedExtraBuffIds =
        {
            CureBuffs.Teracitin,
            CureBuffs.Antibiotics,
            CureBuffs.ForcedSedation,
            CureBuffs.PostSurgical,
            InjuryBuffs.Cold,
        };

        private bool IsKnownTraumaBuff(string buffId) =>
            buffId.StartsWith("buff", StringComparison.Ordinal)
            && KnownTraumas.Any(t => t.BuffId.Equals(buffId, StringComparison.OrdinalIgnoreCase));

        private bool IsModWatchedBuff(string buffId)
        {
            if (IsKnownTraumaBuff(buffId))
                return true;
            if (buffId.StartsWith("HarveyMod_", StringComparison.Ordinal))
                return true;
            if (buffId.StartsWith("buffHarvey", StringComparison.Ordinal))
                return true;
            return WatchedExtraBuffIds.Any(id => id.Equals(buffId, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> CollectActiveModWatchedBuffs() =>
            _buffManager.GetActiveBuffs()
                .Where(IsModWatchedBuff)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

        private void AppendBuffsWatchedBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== BUFFS WATCHED ===");

            foreach (var buffId in state.ActiveDebuffs.Keys)
            {
                bool hasBase = _buffManager.HasBuff(buffId);
                string phaseExpected = GetExpectedPhaseBuff(buffId, state.ActiveDebuffs[buffId].CurrentPhase);
                bool hasPhase = !string.IsNullOrEmpty(phaseExpected) && _buffManager.HasBuff(phaseExpected);

                sb.AppendLine($"base {buffId} = {YesNo(hasBase)}");
                if (!string.IsNullOrEmpty(phaseExpected))
                    sb.AppendLine($"phase {phaseExpected} = {YesNo(hasPhase)}");

                if (!hasBase && !hasPhase)
                    sb.AppendLine($"⚠ DESYNC: {buffId} exists in state but no matching active buff");
            }

            var active = CollectActiveModWatchedBuffs();
            sb.AppendLine(active.Count == 0
                ? "active mod buffs: (none)"
                : $"active mod buffs: {string.Join(", ", active)}");
        }

        private const int MaxAppliedTriggersShown = 20;

        private void AppendAppliedTriggersBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== APPLIED TRIGGERS ===");
            var sorted = state.AppliedTriggers.OrderBy(t => t, StringComparer.Ordinal).ToList();
            sb.AppendLine($"count: {sorted.Count}");
            if (sorted.Count == 0)
                return;

            foreach (var triggerId in sorted.Take(MaxAppliedTriggersShown))
                sb.AppendLine(triggerId);

            int remaining = sorted.Count - MaxAppliedTriggersShown;
            if (remaining > 0)
                sb.AppendLine($"... +{remaining} more");
        }

        private static string? FindMatchingAppliedTrigger(
            string buffId,
            string topicId,
            IEnumerable<string> appliedTriggers)
        {
            string buffKey = buffId.Replace("buff", "", StringComparison.Ordinal);
            string topicKey = topicId.Replace("topic", "", StringComparison.Ordinal);

            foreach (var trigger in appliedTriggers)
            {
                if (!string.IsNullOrEmpty(buffKey)
                    && trigger.Contains(buffKey, StringComparison.OrdinalIgnoreCase))
                    return trigger;

                if (!string.IsNullOrEmpty(topicKey)
                    && trigger.Contains(topicKey, StringComparison.OrdinalIgnoreCase))
                    return trigger;
            }

            return null;
        }

        private void AppendBlockedKnownTraumasBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== BLOCKED KNOWN TRAUMAS ===");
            bool any = false;

            foreach (var trauma in KnownTraumas)
            {
                string? match = FindMatchingAppliedTrigger(trauma.BuffId, trauma.TopicId, state.AppliedTriggers);
                if (match == null)
                    continue;

                any = true;
                sb.AppendLine($"{trauma.BuffId}: possibly blocked by {match}");
            }

            if (!any)
                sb.AppendLine("  (no obvious trigger matches)");
        }

        private int CountActiveWatchedTopics(InjuryState state) =>
            CollectActiveModTopics().Count;

        private void AppendCompactInjuriesSummary(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine($"Active injuries ({state.ActiveDebuffs.Count}):");
            if (state.ActiveDebuffs.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (buffId, d) in state.ActiveDebuffs)
            {
                bool hasBaseBuff = _buffManager.HasBuff(buffId);
                string phaseBuffExpected = GetExpectedPhaseBuff(buffId, d.CurrentPhase);
                bool hasPhaseBuff = !string.IsNullOrEmpty(phaseBuffExpected) && _buffManager.HasBuff(phaseBuffExpected);
                var issues = CollectDebuffIssues(d, hasBaseBuff, phaseBuffExpected, hasPhaseBuff);
                string status = issues.Count == 0 ? "OK" : "WARN";
                sb.AppendLine($"  [{status}] {buffId} | {GetInjuryStateLabel(d)} | {GetClickExpectationForDebuff(d)}");
            }
        }

        private void AppendCompactIssuesSummary(StringBuilder sb, InjuryState state)
        {
            var lines = new List<string>();
            foreach (var (buffId, d) in state.ActiveDebuffs)
            {
                bool hasBaseBuff = _buffManager.HasBuff(buffId);
                string phaseBuffExpected = GetExpectedPhaseBuff(buffId, d.CurrentPhase);
                bool hasPhaseBuff = !string.IsNullOrEmpty(phaseBuffExpected) && _buffManager.HasBuff(phaseBuffExpected);
                var issues = CollectDebuffIssues(d, hasBaseBuff, phaseBuffExpected, hasPhaseBuff);
                if (issues.Count > 0)
                    lines.Add($"{buffId}: {string.Join(", ", issues)}");
            }

            sb.AppendLine(lines.Count == 0 ? "Issues: none" : "Issues:");
            foreach (var line in lines)
                sb.AppendLine($"  {line}");
        }

        private void AppendHarveyClickLines(StringBuilder sb)
        {
            sb.AppendLine($"Pending: {_interactionHandler.GetPendingMedicalActionSummary() ?? "none"}");
            sb.AppendLine($"Last click: {_interactionHandler.LastClickDebug ?? "-"}");
            sb.AppendLine($"Decision now: {_interactionHandler.BuildDebugTreatmentDecision()}");
            sb.AppendLine($"Standard dialogue: {_interactionHandler.GetStandardDialogueGateReason()}");
        }

        private void AppendComplicationsBlock(StringBuilder sb, InjuryState state, int today)
        {
            sb.AppendLine("=== COMPLICATIONS ===");
            if (state.ActiveComplications.Count == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var (compId, startDay) in state.ActiveComplications)
                sb.AppendLine($"  {compId}  day {startDay} (+{today - startDay}d)");
        }

        private void AppendSystemFlagsBlock(StringBuilder sb, InjuryState state)
        {
            sb.AppendLine("=== SYSTEM FLAGS ===");
            sb.AppendLine($"DaysWithSevere: {state.DaysWithSevere}  LastNightRoundDay: {state.LastNightRoundDay}");
            sb.AppendLine($"NeglectStrikes: {state.NeglectStrikes}");
            sb.AppendLine($"PassedOutInTownYesterday: {YesNo(state.PassedOutInTownYesterday)}  PassedOutInMineYesterday: {YesNo(state.PassedOutInMineYesterday)}");
            sb.AppendLine($"NeedsMineRescueEvent: {YesNo(state.NeedsMineRescueEvent)}  WasPassedOut: {YesNo(state.WasPassedOut)}");
            sb.AppendLine($"WasExhausted: {YesNo(state.WasExhausted)}  WasUpTooLate: {YesNo(state.WasUpTooLate)}");
            sb.AppendLine($"LastPassedOutHealth: {state.LastPassedOutHealth}  LastPassedOutLocation: {(string.IsNullOrEmpty(state.LastPassedOutLocation) ? "-" : state.LastPassedOutLocation)}");
            sb.AppendLine($"MineWarningDay: {state.MineWarningDay}  MineForbiddenAppliedDay: {state.MineForbiddenAppliedDay}  LastMineForbiddenInterceptionDay: {state.LastMineForbiddenInterceptionDay}");
            sb.AppendLine($"LastHealth: {state.LastHealth}  Rain: {state.TimeUnderRainTicks}t/{state.TotalTimeUnderRainToday}t");

            var saved = state.SavedActiveBuffs ?? new List<string>();
            sb.AppendLine($"SavedActiveBuffs: {saved.Count}");
            if (saved.Count > 0)
            {
                sb.AppendLine($"  {string.Join(", ", saved.Take(15))}");
                int remaining = saved.Count - 15;
                if (remaining > 0)
                    sb.AppendLine($"  ... +{remaining} more");
            }

            sb.AppendLine($"location: {Game1.currentLocation?.NameOrUniqueName ?? "-"}");
            sb.AppendLine($"time: {Game1.timeOfDay}  health: {Game1.player.health}/{Game1.player.maxHealth}  stamina: {(int)Game1.player.Stamina}/{(int)Game1.player.MaxStamina}");
            sb.AppendLine($"has Severe: {YesNo(_buffManager.HasAnyBuff(InjurySets.Severe.ToArray()))}  has Critical: {YesNo(_buffManager.HasAnyBuff(InjurySets.Critical.ToArray()))}");
        }

        private void BuildCompactDebugHud(StringBuilder sb, InjuryState state)
        {
            AppendCompactInjuriesSummary(sb, state);
            AppendCompactIssuesSummary(sb, state);
            AppendHarveyClickLines(sb);

            int activeTopics = CountActiveWatchedTopics(state);
            sb.AppendLine($"Active mod topics: {activeTopics}");

            var modBuffs = CollectActiveModWatchedBuffs();
            sb.AppendLine(modBuffs.Count == 0
                ? "Active mod buffs: (none)"
                : $"Active mod buffs: {string.Join(", ", modBuffs)}");
        }

        private void BuildFullDebugHud(StringBuilder sb, InjuryState state, int today)
        {
            AppendInjuriesDiagnosticBlock(sb, state, today);
            AppendComplicationsBlock(sb, state, today);
            AppendTopicsWatchedBlock(sb, state);
            AppendBuffsWatchedBlock(sb, state);
            AppendAppliedTriggersBlock(sb, state);
            AppendKnownTraumasMatrixBlock(sb, state);
            AppendSystemFlagsBlock(sb, state);
        }

        private string BuildDebugReport(bool full)
        {
            var state = _stateManager.State;
            int today = (int)Game1.stats.DaysPlayed;
            var sb = new StringBuilder();

            sb.AppendLine($"InjuryState  [day {today}]  [{(full ? "full" : "compact")}]");

            if (full)
                BuildFullDebugHud(sb, state, today);
            else
                BuildCompactDebugHud(sb, state);

            return sb.ToString().TrimEnd();
        }

        private void RenderDebugHud(string report, RenderedHudEventArgs e)
        {
            var lines = report.Split('\n');
            if (lines.Length > MaxDebugHudLines)
            {
                lines = lines.Take(MaxDebugHudLines).Concat(new[] { "... truncated, use SMAPI console" }).ToArray();
            }

            var font = Game1.smallFont;
            float maxW = 0, totalH = 0;
            foreach (var line in lines)
            {
                float w = font.MeasureString(line).X;
                if (w > maxW) maxW = w;
                totalH += font.LineSpacing;
            }

            const int pad = 8, x = 10, y = 10;
            e.SpriteBatch.Draw(Game1.staminaRect,
                new Rectangle(x - pad, y - pad, (int)maxW + pad * 2, (int)totalH + pad * 2),
                new Color(0, 0, 0, 170));

            float drawY = y;
            foreach (var line in lines)
            {
                e.SpriteBatch.DrawString(font, line, new Vector2(x, drawY), Color.White);
                drawY += font.LineSpacing;
            }
        }

        private const int MaxDebugHudLines = 45;

        /// <summary>
        /// Переключение режима дебаг-HUD по F10: 0 → 1 → 2 → 0.
        /// </summary>
        private void OnDebugHudToggleKeyPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.F10) return;
            if (!Context.IsWorldReady) return;

            _debugHudMode = (_debugHudMode + 1) % 3;
            string message = _debugHudMode switch
            {
                0 => "Debug HUD: hidden",
                1 => "Debug HUD: compact",
                2 => "Debug HUD: full",
                _ => "Debug HUD: hidden",
            };
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.achievement_type));
            Helper.Input.Suppress(e.Button);
        }

        /// <summary>
        /// Дебаг: рисует текущий _state в левом верхнем углу экрана.
        /// F10: hidden / compact / full.
        /// </summary>
        private void OnRenderedHudDebugState(object? sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || _debugHudMode == 0) return;

            RenderDebugHud(BuildDebugReport(_debugHudMode == 2), e);
        }

        /// <summary>
        /// Загрузка сохранения
        /// </summary>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            _stateManager.Load();
            
            // Перезагрузить данные баффов после того, как Content Patcher загрузил все патчи
            _buffManager.LoadBuffData();

            // Resume mine rescue, если reload случился между warp и startEvent
            Game1.delayedActions.Add(new StardewValley.DelayedAction(100, () =>
            {
                try
                {
                    ClearModBuffGlowIfNeeded();
                    _passOutHandler.ResumePendingMineRescueIfNeeded();
                    _passOutHandler.ResumePendingHospitalPassOutIfNeeded();
                    _passOutHandler.ResumePendingMinorMineRescueIfNeeded();
                }
                catch (Exception ex)
                {
                    Monitor.Log($"[PassOut] Ошибка resume pending cutscenes после загрузки: {ex}", LogLevel.Error);
                }
            }));
            
            Monitor.Log("Состояние загружено из сохранения", LogLevel.Info);
        }

        /// <summary>
        /// Убрать свечение, если на игроке висят модовые баффы (в т.ч. после загрузки старого сейва).
        /// </summary>
        private void ClearModBuffGlowIfNeeded()
        {
            var applied = Game1.player?.buffs?.AppliedBuffs;
            if (applied == null)
                return;

            foreach (var buffId in applied.Keys)
            {
                if (!ModBuffGlow.IsModBuff(buffId))
                    continue;

                Game1.player.stopGlowing();
                return;
            }
        }

        /// <summary>
        /// Сохранение игры
        /// </summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            _stateManager.Save();
            Monitor.Log("Состояние сохранено", LogLevel.Debug);
        }

        /// <summary>
        /// Проверка обморока перед окончанием дня
        /// </summary>
        private void OnDayEndingPassOutCheck(object? sender, DayEndingEventArgs e)
        {
            _passOutHandler.TrackPassOut();
        }
    }
}

