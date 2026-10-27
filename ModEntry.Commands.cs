using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HarveyOverhaul.Core.Api;
using HarveyOverhaul.InjuryCare.Api;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Managers;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.EventHandlers;
using HarveyOverhaul.InjuryCare.Services;
using HarveyOverhaul.InjuryCare.Testing;
using HarveyOverhaul.InjuryCare.UI.RecoveryPlan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buffs;

namespace HarveyOverhaul.InjuryCare
{
    /// <summary>Консольные команды отладки (травмы, фазы, шахта, клиника, план, доверие).</summary>
    public sealed partial class ModEntry
    {
        /// <summary>Травмы мода: buffId, топик, фазы (p1, p2, p3) дней.</summary>
        private static readonly (string BuffId, string TopicId, int P1, int P2, int P3)[] KnownTraumas =
        {
            ("buffHurt", "topicHurt", 2, 0, 0),
            ("buffBadlyHurt", "topicBadlyHurt", 4, 0, 0),
            ("buffSprainedAnkle", "topicSprainedAnkle", 3, 4, 0),
            ("buffBruisedRibs", "topicBruisedRibs", 4, 5, 0),
            ("buffBackStrain", "topicBackStrain", 2, 4, 0),
            ("buffDeepCuts", "topicDeepCuts", 2, 3, 2),
            ("buffBurnWounds", "topicBurnWounds", 3, 5, 0),
            ("buffInfectedWound", "topicInfectedWound", 3, 11, 0),
            ("buffTornMuscles", "topicTornMuscles", 3, 5, 3),
            ("buffConcussion", "topicConcussion", 2, 4, 3),
            ("buffFracturedBone", "topicFracturedBone", 4, 10, 4),
            ("buffShrapnelWounds", "topicShrapnelWounds", 3, 5, 3),
            ("buffSurgicalWound", "topicSurgicalWound", 7, 0, 0),
            (Core.InjuryBuffs.Cold, Core.ConversationTopics.Cold, 2, 2, 0),
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
            string report = BuildDebuffListReport();
            foreach (string line in report.Split('\n'))
                Monitor.Log(line, LogLevel.Info);
        }

        private string BuildDebuffListReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Дебаффы мода (травмы) ===");
            foreach (var t in KnownTraumas)
                sb.AppendLine($"  {t.BuffId}  (топик: {t.TopicId}, фазы: {t.P1}/{t.P2}/{t.P3} д)");
            sb.AppendLine("=== Осложнения ===");
            foreach (var c in KnownComplications)
                sb.AppendLine($"  {c.BuffId}  (топик: {c.TopicId})");
            sb.AppendLine("Любой ID из Data/Buffs тоже можно применить через injury_debuff_add <id> [минуты].");
            return sb.ToString().TrimEnd();
        }

        private void CmdDebuffAdd(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            bool forceReplace = args.Any(a => string.Equals(a, "--force", StringComparison.OrdinalIgnoreCase));
            var positionalArgs = args
                .Where(a => !string.Equals(a, "--force", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (positionalArgs.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_debuff_add [--force] <id> [минуты]. injury_debuff_list — список ID.",
                    LogLevel.Info);
                return;
            }

            string id = positionalArgs[0].Trim();
            int minutes = -2;
            if (positionalArgs.Length >= 2 && int.TryParse(positionalArgs[1], out int m))
                minutes = m;

            var trauma = KnownTraumas.FirstOrDefault(t => t.BuffId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (trauma.BuffId != null)
            {
                CmdDebuffAddTrauma(trauma, minutes, forceReplace);
                return;
            }

            var comp = KnownComplications.FirstOrDefault(c => c.BuffId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (comp.BuffId != null)
            {
                int today = (int)Game1.stats.DaysPlayed;
                _buffManager.AddBuff(comp.BuffId, minutes);
                _stateManager.State.ActiveComplications[comp.BuffId] = today;
                _stateManager.CreateComplicationState(comp.BuffId, today);
                _dialogueManager.EnsureComplicationDialogueTopics(comp.BuffId, 4);
                Monitor.Log($"Применено осложнение: {comp.BuffId}, топики action+{comp.TopicId}.", LogLevel.Info);
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

        private void CmdDebuffAddTrauma(
            (string BuffId, string TopicId, int P1, int P2, int P3) trauma,
            int minutes,
            bool forceReplace)
        {
            string? currentMain = _injuryManager.GetCurrentMainInjuryId();

            if (!string.IsNullOrEmpty(currentMain)
                && string.Equals(currentMain, trauma.BuffId, StringComparison.OrdinalIgnoreCase))
            {
                ApplyDebugTraumaEffects(trauma, minutes);
                Monitor.Log(
                    $"MainInjury уже {trauma.BuffId} — обновлены бафф/состояние/топик.",
                    LogLevel.Info);
                Game1.addHUDMessage(new HUDMessage($"[ДЕБАГ] Main: {trauma.BuffId}", HUDMessage.achievement_type));
                return;
            }

            bool applied = _injuryManager.TryApplyMainInjury(
                trauma.BuffId,
                () => ApplyDebugTraumaEffects(trauma, minutes),
                allowUpgrade: !forceReplace,
                forceReplace: forceReplace);

            if (!applied)
            {
                Monitor.Log(
                    $"Отказ: MainInjury={currentMain ?? "(none)"}, попытка добавить {trauma.BuffId}. " +
                    "Используйте --force для замены основной травмы.",
                    LogLevel.Warn);
                return;
            }

            _injuryManager.TryMarkNeedsSevereNightRoundEvent(trauma.BuffId);
            Monitor.Log(
                $"Применена основная травма: {trauma.BuffId}, MainInjuryId={_stateManager.GetMainInjuryId()}",
                LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[ДЕБАГ] Main injury: {trauma.BuffId}", HUDMessage.achievement_type));
        }

        private void ApplyDebugTraumaEffects(
            (string BuffId, string TopicId, int P1, int P2, int P3) trauma,
            int minutes)
        {
            int today = (int)Game1.stats.DaysPlayed;
            _buffManager.AddBuff(trauma.BuffId, minutes);
            _stateManager.CreateDebuffState(trauma.BuffId, today, trauma.P1, trauma.P2, trauma.P3);
            int topicDays = trauma.P1 + trauma.P2 + trauma.P3;
            if (topicDays <= 0) topicDays = 7;
            _dialogueManager.AddTopic(trauma.TopicId, topicDays);
            if (string.Equals(trauma.BuffId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                _dialogueManager.AddTopic(ConversationTopics.HealthDamageCritical, 4);
            _dialogueManager.TryAddHarveyNeedsFirstTreatmentTopic(trauma.BuffId);
            _dialogueManager.TryAddTreatmentNeededTopic(trauma.BuffId, topicDays);

            if (string.Equals(trauma.BuffId, "buffConcussion", StringComparison.OrdinalIgnoreCase)
                && _config.ForceHospitalization)
            {
                _hospitalizationManager.StartForcedHospitalization(
                    "buffConcussion",
                    HarveyHelper.GetHarvey());
            }
        }

        private void CmdMainClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _stateManager.DebugClearMainInjuryId();
        }

        private void CmdCleanupLingeringMedical()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            int cleaned = _treatmentManager.CleanupLingeringMedicalState();
            Monitor.Log(
                $"[RecoveryCleanup] injury_cleanup завершён: изменений={cleaned}. Проверьте: injury_phase_list, injury_buff_dump",
                LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage(
                $"[RecoveryCleanup] Очищено элементов: {cleaned}",
                HUDMessage.health_type));
        }

        private void CmdCleanupInvalidComplications()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log(
                $"[ComplicationCleanup] before: ActiveComplications={FormatComplicationsList(_stateManager.State)}",
                LogLevel.Info);
            _complicationManager.CleanupInvalidComplications();
            Monitor.Log(
                "Готово. Проверьте: injury_phase_list, injury_debug_dump",
                LogLevel.Info);
        }

        private void CmdMainSet(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: injury_main_set <buffId>", LogLevel.Info);
                return;
            }

            string buffId = args[0].Trim();
            if (_stateManager.DebugSetMainInjuryId(buffId))
            {
                Monitor.Log(
                    $"MainInjuryId={_stateManager.GetMainInjuryId()}, valid={YesNo(IsMainInjuryStateValid(_stateManager.GetMainInjuryId()))}",
                    LogLevel.Info);
            }
        }

        private bool IsMainInjuryStateValid(string? mainInjuryId) =>
            _injuryManager.ValidateMainInjury(mainInjuryId).Valid;

        private static string FormatComplicationsList(InjuryState state)
        {
            if (state.ActiveComplications.Count == 0)
                return "(none)";

            return string.Join(", ", state.ActiveComplications.Keys);
        }

        private void AppendMainInjuryDebugBlock(StringBuilder sb, InjuryState state)
        {
            MainInjuryValidation mainInfo = _injuryManager.GetMainInjuryDebugInfo();
            string? mainId = mainInfo.MainInjuryId ?? _injuryManager.GetActiveInjury();

            sb.AppendLine($"Main injury: {(string.IsNullOrEmpty(mainId) ? "(none)" : mainId)}  valid: {YesNo(mainInfo.Valid)}");
            if (!mainInfo.Valid && !string.IsNullOrEmpty(mainInfo.Reason))
                sb.AppendLine($"Main injury invalid reason: {mainInfo.Reason}");

            var mainState = string.IsNullOrEmpty(mainId) ? null : _stateManager.GetDebuffState(mainId);
            if (mainState == null)
            {
                sb.AppendLine("Main injury phase: -");
                sb.AppendLine("Main injury treatment started: -");
                sb.AppendLine("ReadyForNextPhase / ReadyForRecovery: - / -");
            }
            else
            {
                sb.AppendLine($"Main injury phase: {GetInjuryStateLabel(mainState)}");
                sb.AppendLine($"Main injury treatment started: {YesNo(mainState.TreatmentStarted)}");
                sb.AppendLine(
                    $"ReadyForNextPhase / ReadyForRecovery: {YesNo(mainState.ReadyForNextPhase)} / {YesNo(mainState.ReadyForRecovery)}");
            }

            sb.AppendLine($"Complications: {FormatComplicationsList(state)}");
            sb.AppendLine($"SavedActiveBuffs count: {state.SavedActiveBuffs?.Count ?? 0}");
        }

        private void CmdPhaseList()
        {
            string report = BuildPhaseListReport();
            foreach (string line in report.Split('\n'))
                Monitor.Log(line, LogLevel.Info);
        }

        private string BuildPhaseListReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            var sb = new StringBuilder();
            var state = _stateManager.State;
            string? storedMainId = _stateManager.GetMainInjuryId();
            MainInjuryValidation mainInfo = _injuryManager.GetMainInjuryDebugInfo();
            string? resolvedMainId = mainInfo.MainInjuryId;

            sb.AppendLine($"MainInjuryId (stored): {(string.IsNullOrEmpty(storedMainId) ? "(none)" : storedMainId)}");
            sb.AppendLine($"MainInjuryId (resolved): {(string.IsNullOrEmpty(resolvedMainId) ? "(none)" : resolvedMainId)}");
            if (_stateManager.QaSuppressMainInjuryAutoSync)
                sb.AppendLine("MainInjury auto-sync: suppressed (QA)");

            string? mainId = resolvedMainId;
            sb.AppendLine($"Active main injury valid: {YesNo(mainInfo.Valid)}");
            if (!mainInfo.Valid && !string.IsNullOrEmpty(mainInfo.Reason))
                sb.AppendLine($"Active main injury invalid reason: {mainInfo.Reason}");

            if (!string.IsNullOrEmpty(mainId))
            {
                sb.AppendLine($"BaseBuff active: {YesNo(mainInfo.BaseBuffActive)}");
                sb.AppendLine(
                    $"CureBuff: {(string.IsNullOrEmpty(mainInfo.CureBuffId) ? "(n/a)" : mainInfo.CureBuffId)} active={YesNo(mainInfo.CureBuffActive)}");
                sb.AppendLine(
                    $"PhaseBuff: {(string.IsNullOrEmpty(mainInfo.PhaseBuffId) ? "(n/a)" : mainInfo.PhaseBuffId)} active={YesNo(mainInfo.PhaseBuffActive)}");
                sb.AppendLine($"TreatmentStarted: {YesNo(mainInfo.TreatmentStarted)}");
                sb.AppendLine($"Main injury serious: {YesNo(InjuryManager.IsSeriousMainInjuryId(mainId))}");
            }

            sb.AppendLine($"Complications: {FormatComplicationsList(state)}");

            var all = _stateManager.GetAllActiveDebuffStates();
            if (all.Count == 0)
            {
                sb.AppendLine("Нет активных дебаффов в состоянии мода.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("=== Активные травмы (фаза, готовность) ===");
            foreach (var ds in all)
            {
                string phaseInfo = ds.TotalPhases == 0
                    ? "не фазовая"
                    : $"фаза {ds.CurrentPhase}/{ds.TotalPhases} (с дня {ds.PhaseStartDay})";
                string flags = "";
                if (ds.ReadyForNextPhase) flags += " [→след.фаза]";
                if (ds.ReadyForRecovery) flags += " [→выздоровление]";
                if (ds.TreatmentStarted) flags += " в лечении";
                sb.AppendLine($"  {ds.BuffId}: {phaseInfo}{flags}");
            }

            sb.AppendLine("=== Buff sync (DebuffState → expected buff) ===");
            foreach (var ds in all.Where(d => InjurySets.HarveyTreatable.Contains(d.BuffId)))
            {
                InjuryTreatmentDebugInfo info = _injuryManager.BuildInjuryTreatmentDebugInfo(ds.BuffId, mainId);
                sb.AppendLine($"  [{ds.BuffId}] MainInjury={YesNo(info.IsMainInjury)} TreatmentStarted={YesNo(info.TreatmentStarted)}");
                sb.AppendLine(
                    $"    Phase={info.CurrentPhase}/{info.TotalPhases} ReadyNext={YesNo(info.ReadyForNextPhase)} ReadyRecovery={YesNo(info.ReadyForRecovery)}");
                sb.AppendLine(
                    $"    expectedBuff={info.ExpectedBuffId ?? "(none)"} active={YesNo(info.ExpectedBuffActive)}");
                sb.AppendLine(
                    $"    base={YesNo(info.BaseBuffActive)} cure={YesNo(info.CureBuffActive)} stalePhases=[{string.Join(", ", info.StalePhaseBuffs)}]");
                sb.AppendLine(
                    $"    topics=[{(info.ActiveTopics.Count == 0 ? "(none)" : string.Join(", ", info.ActiveTopics))}]");
            }

            return sb.ToString().TrimEnd();
        }

        private static bool ParseBoolArg(string[] args, int index, bool defaultVal)
        {
            if (args.Length <= index) return defaultVal;
            return args[index].Trim() switch { "1" => true, "0" => false, _ => defaultVal };
        }

        private void CmdPhaseReady(string[] args)
        {
            if (!Context.IsWorldReady) { Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn); return; }
            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_phase_ready <buffId> [1|0] — только фазовые травмы. " +
                    "Для buffHurt, buffBadlyHurt, buffSurgicalWound: injury_phase_recovery или injury_phase_cure.",
                    LogLevel.Info);
                return;
            }
            string id = args[0].Trim();
            if (!_stateManager.State.ActiveDebuffs.TryGetValue(id, out _))
            {
                Monitor.Log($"Дебафф «{id}» не найден в состоянии. injury_phase_list — список активных.", LogLevel.Warn);
                return;
            }

            if (TreatmentManager.IsSimpleTreatmentInjury(id))
            {
                Monitor.Log(
                    $"«{id}» — простое лечение (buffHurt, buffBadlyHurt, buffSurgicalWound). " +
                    "injury_phase_ready не применяется; используйте injury_phase_recovery или injury_phase_cure.",
                    LogLevel.Warn);
                return;
            }

            var ds = _stateManager.GetDebuffState(id)!;
            bool ready = ParseBoolArg(args, 1, true);

            if (ready)
            {
                if (ds.TotalPhases <= 0)
                {
                    Monitor.Log(
                        $"Дебафф «{id}» не является фазовой травмой; используйте injury_phase_recovery или injury_phase_cure.",
                        LogLevel.Warn);
                    return;
                }

                if (ds.CurrentPhase >= ds.TotalPhases)
                {
                    Monitor.Log(
                        $"Травма «{id}» уже на последней фазе ({ds.CurrentPhase}/{ds.TotalPhases}). Используйте injury_phase_recovery.",
                        LogLevel.Warn);
                    return;
                }
            }

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
            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_phase_advance <buffId> — только фазовые травмы. " +
                    "Для buffHurt, buffBadlyHurt, buffSurgicalWound: injury_phase_cure.",
                    LogLevel.Info);
                return;
            }
            string id = args[0].Trim();

            if (TreatmentManager.IsSimpleTreatmentInjury(id))
            {
                Monitor.Log(
                    $"«{id}» — простое лечение (buffHurt, buffBadlyHurt, buffSurgicalWound). " +
                    "injury_phase_advance не применяется; используйте injury_phase_cure.",
                    LogLevel.Warn);
                return;
            }

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
            string report = BuildMineDirtyDebugReport();
            Monitor.Log(report, LogLevel.Info);
        }

        private void CmdMineDirtySimulate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0 || !int.TryParse(args[0], out int minutes))
            {
                Monitor.Log("Использование: injury_mine_dirty_simulate <minutes> [force]", LogLevel.Info);
                return;
            }

            bool force = args.Any(a => string.Equals(a, "force", StringComparison.OrdinalIgnoreCase));
            string result = _playerEventHandler.SimulateMineDirtyExposureForQa(minutes, force);
            Monitor.Log($"[QA] {result}", LogLevel.Info);
        }

        private void CmdRunDailyChecks()
        {
            string result = _gameEventHandler.RunQaDailyChecks();
            Monitor.Log(result, LogLevel.Info);
        }

        private void CmdMainMigrate()
        {
            string main = _stateManager.DebugMigrateMainInjuryId();
            Monitor.Log($"[QA] injury_main_migrate → MainInjuryId={main}", LogLevel.Info);
        }

        private void CmdNeglectSet(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length < 2 || !int.TryParse(args[1], out int strikes))
            {
                Monitor.Log("Использование: injury_neglect_set <buffId> <strikes>", LogLevel.Info);
                return;
            }

            string result = RunNeglectSet(args[0].Trim(), strikes);
            Monitor.Log($"[QA] {result}", LogLevel.Info);
        }

        private string RunNeglectSet(string buffId, int strikes)
        {
            _stateManager.SetNeglectStrikesForQa(buffId, strikes);

            if (strikes >= _config.NeglectDaysThreshold)
                _gameEventHandler.RunQaCheckNeglect();

            return
                $"NeglectStrikesByInjury[{buffId}]={strikes} " +
                $"Neglect={_buffManager.HasBuff(InjuryBuffs.Neglect)} " +
                $"topic={_dialogueManager.HasTopic(ConversationTopics.Neglect)}";
        }

        private void CmdMineWarningSimulate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            bool yesterday = args.Any(a =>
                string.Equals(a, "yesterday", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "1", StringComparison.Ordinal));
            string result = _playerEventHandler.SimulateMineSevereWarningForQa(yesterday);
            Monitor.Log($"[QA] injury_mine_warning_simulate {result}", LogLevel.Info);
        }

        private void CmdLocationLogic()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            string result = _playerEventHandler.RunLocationLogicForQa();
            Monitor.Log($"[QA] injury_location_logic {result}", LogLevel.Info);
        }

        private void CmdRainWetSimulate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            bool force = args.Length == 0
                || !args.Any(a => string.Equals(a, "noroll", StringComparison.OrdinalIgnoreCase));
            string result = _playerEventHandler.SimulateRainWetBandageForQa(force);
            Monitor.Log($"[QA] injury_rain_wet_simulate {result}", LogLevel.Info);
        }

        private void CmdHospitalLockEnforce()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            string result = _hospitalizationManager.EnforceLockForQa();
            Monitor.Log($"[QA] injury_hospital_lock_enforce {result}", LogLevel.Info);
        }

        private void CmdGameUiAdvance(string[] args)
        {
            int steps = 1;
            if (args.Length > 0 && int.TryParse(args[0], out int parsed))
                steps = parsed;

            string result = QaGameUiCommands.AdvanceUiMany(steps);
            Monitor.Log(result, LogLevel.Info);
        }

        private string BuildMineDirtyDebugReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            var state = _stateManager.State;
            string loc = Game1.currentLocation?.Name ?? "(null)";
            bool hasDirtyInjury = _complicationManager.CanReceiveMineDirtyWound();
            bool hasDirtyWound = _buffManager.HasBuff(InjuryBuffs.DirtyWound)
                || state.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound);

            return
                $"[MineDirtyDebug] loc={loc}, exposure={state.MineDirtyExposureMinutesToday}m, " +
                $"lastExposureDay={state.LastMineDirtyExposureDay}, lastRoll={state.LastMineDirtyWoundRollMinute}, " +
                $"boostUntil={state.MineDirtyRiskBoostUntilMinute}, hasDirtyInjury={hasDirtyInjury}, hasDirtyWound={hasDirtyWound}";
        }

        private void CmdMineForbiddenClear()
        {
            CmdMineBanClear();
        }

        private void CmdMineBanClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            MineForbiddenHelper.ClearHardMineForbiddenState(_stateManager.State, _buffManager);
            _stateManager.State.LastMineSevereWarningDay = -1;
            _stateManager.State.LastMineSevereForcedExitDay = -1;
            _stateManager.Save();

            Monitor.Log("[MineForbidden] Жёсткий запрет шахты сброшен", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("Харви: Запрет на шахту снят.", HUDMessage.achievement_type));
        }

        private void CmdMineClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            CmdMineForbiddenClear();
            CmdMineRestrictionClear();
            Monitor.Log("[Mine] [ДЕБАГ] Полный сброс шахты (forbidden + restricted)", LogLevel.Info);
        }

        private void CmdMineForbid(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            int today = (int)Game1.stats.DaysPlayed;
            int duration = Math.Max(1, _config.MineForbiddenDurationDays);
            int daysLeft = duration;

            if (args.Length > 0 && int.TryParse(args[0], out int parsed))
                daysLeft = Math.Max(1, parsed);

            var state = _stateManager.State;
            state.MineForbiddenAppliedDay = today - Math.Max(0, duration - daysLeft);
            MineForbiddenHelper.ApplyHardMineForbidden(
                state,
                _config,
                _buffManager,
                _stateManager,
                Monitor,
                today,
                "qa_forbid",
                resetAppliedDay: false);

            Monitor.Log($"[QA] injury_mine_forbid daysLeft={daysLeft} appliedDay={state.MineForbiddenAppliedDay}", LogLevel.Info);
        }

        private void CmdHospitalProgress(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0 || !int.TryParse(args[0], out int minutes))
            {
                Monitor.Log("Использование: injury_hospital_progress <minutes>", LogLevel.Warn);
                return;
            }

            _hospitalizationManager.SetHospitalStayProgressMinutes(minutes);
            Monitor.Log(
                $"[QA] injury_hospital_progress minutes={minutes} CanDischarge={_hospitalizationManager.CanDischarge()}",
                LogLevel.Info);
        }

        private void CmdHospitalReset()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _hospitalizationManager.ResetHospitalizationForQa();
            Monitor.Log("[QA] injury_hospital_reset ok", LogLevel.Info);
        }

        private void CmdMineRestrictionClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            MineForbiddenHelper.ClearMineRestrictedState(_stateManager.State, _buffManager);
            _stateManager.Save();

            Monitor.Log("[MineRestricted] [ДЕБАГ] Ограничения шахты сброшены", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("[ДЕБАГ] Ограничения шахты сброшены", HUDMessage.achievement_type));
        }

        private void CmdMineStatus()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            int today = (int)Game1.stats.DaysPlayed;
            string report = MineForbiddenHelper.BuildStatusReport(
                _stateManager.State, _config, _injuryManager, _buffManager, today);

            report += Environment.NewLine +
                $"ShouldPhysicallyBlockMines={_mineEntryCoordinator.ShouldPhysicallyBlockMines()}" + Environment.NewLine +
                $"ShouldWarnRecoveryPlanMineEntry={_mineEntryCoordinator.ShouldWarnRecoveryPlanMineEntry()}";

            foreach (string line in report.Split('\n'))
                Monitor.Log(line, LogLevel.Info);
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
            _stateManager.State.NeedsSevereNightRoundEvent = false;
            _stateManager.State.SevereNightRoundEventShownDay = -1;
            _stateManager.State.SevereNightRoundInjuryId = "";
            _stateManager.Save();

            Monitor.Log(
                "Флаги ночного визита Харви сброшены: LastNightRoundRollDay=-1, LastNightRoundDay=-1, SevereNightRound*=сброс.",
                LogLevel.Info);
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

        private void CmdMailDump()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Medical Letters ===");
            sb.AppendLine($"MedicalLetters={_config.MedicalLetters} SendStoryLetters={_config.SendStoryLetters} SendRomanticCareLetters={_config.SendRomanticCareLetters}");
            sb.AppendLine($"Pending ({_stateManager.State.PendingMedicalLetters.Count}):");
            foreach (var letter in _stateManager.State.PendingMedicalLetters)
            {
                sb.AppendLine(
                    $"  mail={letter.MailId} reason={letter.Reason} state={letter.StateId} critical={letter.Critical} deliverDay={letter.DeliverAfterDay}");
            }

            sb.AppendLine($"mailForTomorrow ({Game1.player.mailForTomorrow.Count}):");
            foreach (string mailId in Game1.player.mailForTomorrow)
                sb.AppendLine($"  {mailId}");

            Monitor.Log(sb.ToString(), LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("Medical mail dump → SMAPI log", HUDMessage.newQuest_type));
        }

        private void CmdInjuryDebugState()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== Injury Harvey Interaction ===");
            sb.AppendLine($"click={_interactionHandler.LastClickDebug ?? "(none)"}");
            sb.AppendLine($"gate={_interactionHandler.GetStandardDialogueGateReason()}");
            sb.AppendLine($"pending={_interactionHandler.GetPendingMedicalActionSummary() ?? "none"}");
            sb.AppendLine($"harveyDecision={_interactionHandler.BuildDebugTreatmentDecision()}");
            Monitor.Log(sb.ToString().TrimEnd(), LogLevel.Info);
        }

        private void CmdInjuryForceDebuff(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: injury_force_debuff <buffId> (e.g. buffDeepCuts)", LogLevel.Info);
                return;
            }

            string id = args[0].Trim();
            CmdDebuffAdd(new[] { "--force", id });

            var state = _stateManager.GetDebuffState(id);
            if (state != null)
            {
                state.TreatmentStarted = false;
                state.TreatmentApplied = false;
                state.TreatmentIntroShown = false;
                state.CurrentPhase = 0;
                state.InjuryStartDay = Math.Max(1, (int)Game1.stats.DaysPlayed - 1);
                _stateManager.Save();
            }

            _injuryManager.EnsureActiveTreatmentBuffs();
            _dialogueManager.TryAddTreatmentNeededTopic(id, 7);
            Monitor.Log(
                $"[QA] injury_force_debuff: {id} untreated, InjuryStartDay=yesterday, TreatmentNeeded topic set",
                LogLevel.Info);
        }

        private void CmdInjuryClearTreatment(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: injury_clear_treatment <buffId>", LogLevel.Info);
                return;
            }

            string id = args[0].Trim();
            var state = _stateManager.GetDebuffState(id);
            if (state == null)
            {
                Monitor.Log($"DebuffState не найден: {id}", LogLevel.Warn);
                return;
            }

            state.TreatmentStarted = false;
            state.TreatmentApplied = false;
            state.TreatmentIntroShown = false;
            state.CurrentPhase = 0;
            state.ReadyForNextPhase = false;
            state.ReadyForRecovery = false;

            foreach (var staleId in _injuryManager.GetStalePhaseBuffIds(id))
                _buffManager.RemoveBuff(staleId);

            string? cureId = _injuryManager.GetExpectedCureBuffId(id);
            if (!string.IsNullOrEmpty(cureId))
                _buffManager.RemoveBuff(cureId);

            if (!_buffManager.HasBuff(id))
                _buffManager.AddBuff(id, -2);

            _dialogueManager.TryAddTreatmentNeededTopic(id, 7);
            _injuryManager.EnsureActiveTreatmentBuffs();
            _stateManager.Save();
            Monitor.Log($"[QA] injury_clear_treatment: {id} reset to untreated debuff", LogLevel.Info);
        }

        private void CmdStateDump() => LogQaReport("injury_state_dump", BuildStateDumpReport);

        private void CmdBuffDump() => LogQaReport("injury_buff_dump", BuildBuffDumpReport);

        private void CmdTopicDump() => LogQaReport("injury_topic_dump", BuildTopicDumpReport);

        private void CmdValidateBuffs() => LogQaValidateBuffs();

        private string BuildStateDumpReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";
            return QaDumpCommands.BuildStateDump(_stateManager.State);
        }

        private string BuildBuffDumpReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";
            return QaDumpCommands.BuildBuffDump(
                _buffManager,
                _injuryManager,
                _stateManager.State,
                KnownTraumas,
                KnownComplications);
        }

        private string BuildTopicDumpReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";
            return QaDumpCommands.BuildTopicDump();
        }

        private string BuildValidateBuffsReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";
            return QaDumpCommands.BuildValidateBuffsReport(
                _buffManager,
                _injuryManager,
                KnownTraumas,
                KnownComplications);
        }

        private void LogQaReport(string command, Func<string> buildReport)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log($"[QA] {command}: load a save first.", LogLevel.Warn);
                return;
            }

            Monitor.Log($"[QA] {command}", LogLevel.Info);
            foreach (string line in buildReport().Split('\n'))
            {
                if (!string.IsNullOrEmpty(line))
                    Monitor.Log(line, LogLevel.Info);
            }
        }

        private void LogQaValidateBuffs()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("[QA] injury_validate_buffs: load a save first.", LogLevel.Warn);
                return;
            }

            string body = BuildValidateBuffsReport();
            if (body.StartsWith("result=OK", StringComparison.Ordinal))
                Monitor.Log("[QA] injury_validate_buffs: OK", LogLevel.Info);
            else
            {
                string missingPart = body.Contains("ids=")
                    ? body[(body.IndexOf("ids=", StringComparison.Ordinal) + 4)..]
                    : body;
                int missingCount = 0;
                var countMatch = System.Text.RegularExpressions.Regex.Match(body, @"missing_count=(\d+)");
                if (countMatch.Success)
                    int.TryParse(countMatch.Groups[1].Value, out missingCount);
                Monitor.Log(
                    $"[QA] injury_validate_buffs: MISSING {missingCount}: {missingPart}",
                    LogLevel.Warn);
            }

            foreach (string line in body.Split('\n'))
            {
                if (!string.IsNullOrEmpty(line))
                    Monitor.Log(line, LogLevel.Info);
            }
        }

        private string RunTopicAdd(string[] args)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (args.Length == 0)
                return "Usage: injury_topic_add <topicId> [days]";

            string topicId = args[0].Trim();
            if (!QaSetupCommands.IsOwnedTopic(topicId))
                return $"SKIP: {topicId} is not an owned topic (use injury_foreign_topic_add for foreign)";

            int days = QaSetupCommands.ResolveOwnedTopicDefaultDays(topicId, KnownTraumas, KnownComplications);
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out int parsedDays))
                    return "Usage: injury_topic_add <topicId> [days]";
                days = Math.Max(1, parsedDays);
            }

            QaSetupCommands.TryAddOwnedTopic(_dialogueManager, topicId, days);
            return $"topicId={topicId} days={days}";
        }

        private void CmdTopicAdd(string[] args)
        {
            string result = RunTopicAdd(args);
            if (result.StartsWith("Usage:", StringComparison.Ordinal) || result.StartsWith("Error:", StringComparison.Ordinal))
            {
                Monitor.Log($"[QA] injury_topic_add: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_topic_add", LogLevel.Info);
            Monitor.Log(result.StartsWith("SKIP:") ? $"[QA] injury_topic_add {result}" : $"[QA] injury_topic_add {result}", LogLevel.Info);
        }

        private string RunTopicRemove(string[] args)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (args.Length == 0)
                return "Usage: injury_topic_remove <topicId>";

            string topicId = args[0].Trim();
            bool removed = QaSetupCommands.TryRemoveTopic(_dialogueManager, topicId);
            return $"topicId={topicId} removed={(removed ? "yes" : "no")}";
        }

        private void CmdTopicRemove(string[] args)
        {
            string result = RunTopicRemove(args);
            if (result.StartsWith("Usage:", StringComparison.Ordinal) || result.StartsWith("Error:", StringComparison.Ordinal))
            {
                Monitor.Log($"[QA] injury_topic_remove: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_topic_remove", LogLevel.Info);
            Monitor.Log($"[QA] injury_topic_remove {result}", LogLevel.Info);
        }

        private string RunComplicationAdd(string[] args)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (args.Length == 0)
                return "Usage: injury_complication_add <complicationBuffId> [ageDays]";

            string compId = args[0].Trim();
            if (!QaSetupCommands.IsKnownComplication(compId, KnownComplications))
                return $"SKIP: unknown complication id {compId}";

            int? ageDays = null;
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out int parsedAge))
                    return "Usage: injury_complication_add <complicationBuffId> [ageDays]";
                ageDays = Math.Max(0, parsedAge);
            }

            if (!_complicationManager.TryApplyComplicationForQa(compId, ageDays, out string skipReason))
                return $"SKIP: {skipReason}";

            string agePart = ageDays.HasValue ? $" ageDays={ageDays.Value}" : string.Empty;
            return $"complication={compId} ok=yes{agePart}";
        }

        private void CmdComplicationAdd(string[] args)
        {
            string result = RunComplicationAdd(args);
            if (result.StartsWith("Usage:", StringComparison.Ordinal) || result.StartsWith("Error:", StringComparison.Ordinal))
            {
                Monitor.Log($"[QA] injury_complication_add: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_complication_add", LogLevel.Info);
            Monitor.Log(
                result.StartsWith("SKIP:", StringComparison.Ordinal)
                    ? $"[QA] injury_complication_add {result}"
                    : $"[QA] injury_complication_add {result}",
                result.StartsWith("SKIP:", StringComparison.Ordinal) ? LogLevel.Warn : LogLevel.Info);
        }

        private string RunComplicationRemove(string[] args)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (args.Length == 0)
                return "Usage: injury_complication_remove <complicationBuffId>";

            string compId = args[0].Trim();
            bool ok = _complicationManager.RemoveComplicationForQa(compId);
            return $"complication={compId} ok={(ok ? "yes" : "no")}";
        }

        private void CmdComplicationRemove(string[] args)
        {
            string result = RunComplicationRemove(args);
            if (result.StartsWith("Usage:", StringComparison.Ordinal) || result.StartsWith("Error:", StringComparison.Ordinal))
            {
                Monitor.Log($"[QA] injury_complication_remove: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_complication_remove", LogLevel.Info);
            Monitor.Log($"[QA] injury_complication_remove {result}", LogLevel.Info);
        }

        private string RunTestAgeInjury(string[] args)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (args.Length < 2)
                return "Usage: injury_test_age_injury <buffId> <daysBack>";

            string buffId = args[0].Trim();
            if (!int.TryParse(args[1], out int daysBack))
                return "Usage: injury_test_age_injury <buffId> <daysBack>";

            if (!QaSetupCommands.TryAgeInjury(_stateManager.State, _stateManager, buffId, daysBack, out string detail))
                return detail;

            return detail;
        }

        private void CmdTestAgeInjury(string[] args)
        {
            string result = RunTestAgeInjury(args);
            if (result.StartsWith("Usage:", StringComparison.Ordinal) || result.StartsWith("Error:", StringComparison.Ordinal)
                || result.Contains("not in ActiveDebuffs"))
            {
                Monitor.Log($"[QA] injury_test_age_injury: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_test_age_injury", LogLevel.Info);
            Monitor.Log($"[QA] injury_test_age_injury {result}", LogLevel.Info);
        }

        private string RunTestAgeComplication(string[] args)
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (args.Length < 2)
                return "Usage: injury_test_age_complication <complicationBuffId> <daysBack>";

            string compId = args[0].Trim();
            if (!int.TryParse(args[1], out int daysBack))
                return "Usage: injury_test_age_complication <complicationBuffId> <daysBack>";

            if (!QaSetupCommands.TryAgeComplication(_stateManager.State, _stateManager, compId, daysBack, out string detail))
                return detail;

            return detail;
        }

        private void CmdTestAgeComplication(string[] args)
        {
            string result = RunTestAgeComplication(args);
            if (result.StartsWith("Usage:", StringComparison.Ordinal) || result.StartsWith("Error:", StringComparison.Ordinal)
                || result.Contains("not in ActiveComplications"))
            {
                Monitor.Log($"[QA] injury_test_age_complication: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_test_age_complication", LogLevel.Info);
            Monitor.Log($"[QA] injury_test_age_complication {result}", LogLevel.Info);
        }

        private string BuildHospitalStatusReport()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";
            return QaSetupCommands.BuildHospitalStatusReport(_stateManager.State, _hospitalizationManager);
        }

        private void CmdHospitalStatus() => LogQaReport("injury_hospital_status", BuildHospitalStatusReport);

        private string RunHospitalDischarge()
        {
            if (!Context.IsWorldReady)
                return "Error: load a save first.";

            if (!_hospitalizationManager.IsHospitalized)
                return "injury=(none) ok=no reason=not hospitalized";

            string injuryId = _hospitalizationManager.CurrentInjury ?? "(none)";
            _hospitalizationManager.Discharge();
            return $"injury={injuryId} ok=yes";
        }

        private void CmdHospitalDischarge()
        {
            string result = RunHospitalDischarge();
            if (result.StartsWith("Error:", StringComparison.Ordinal))
            {
                Monitor.Log($"[QA] injury_hospital_discharge: {result}", LogLevel.Warn);
                return;
            }

            Monitor.Log("[QA] injury_hospital_discharge", LogLevel.Info);
            Monitor.Log($"[QA] injury_hospital_discharge {result}", LogLevel.Info);
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

        private void CmdHarveyClick(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            bool dryRun = args.Any(a => string.Equals(a, "dry", StringComparison.OrdinalIgnoreCase));
            bool ignoreHospital = args.Any(a =>
                string.Equals(a, "ignore_hospital", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "--ignore-hospital", StringComparison.OrdinalIgnoreCase));
            bool dischargeFirst = args.Any(a =>
                string.Equals(a, "discharge", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "--discharge", StringComparison.OrdinalIgnoreCase));

            if (dischargeFirst && _hospitalizationManager.IsHospitalized)
                RunHospitalDischarge();

            string result = _interactionHandler.TryDebugApplyHarveyMedicalAction(dryRun, ignoreHospital);
            Monitor.Log($"[HarveyClick] {result}", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"[Harvey] {result}", HUDMessage.health_type));
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
            var prefixes = HarveyReactionManager.BuildPrefixCandidates(primaryPrefix);
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

        private void CmdPrescriptionList()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            var state = _stateManager.State;
            Monitor.Log("=== Активные предписания Харви ===", LogLevel.Info);
            Monitor.Log($"TreatmentComplianceScore: {state.TreatmentComplianceScore} ({ComplianceManager.GetLevelDisplayName(_complianceManager.GetComplianceLevel())})", LogLevel.Info);

            var lines = _prescriptionManager.GetActivePrescriptionSummary().ToList();
            if (lines.Count == 0 || (lines.Count == 1 && lines[0] == "(none)"))
                Monitor.Log("  (нет активных предписаний)", LogLevel.Info);
            else
                foreach (string line in lines)
                    Monitor.Log($"  {line}", LogLevel.Info);
        }

        private void CmdPrescriptionAdd(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length < 2)
            {
                Monitor.Log(
                    "Использование: injury_prescription_add <id> <injuryId> [days]. " +
                    "id: HarveyMod_Prescription_Rest | NoMine | KeepDry | LightWork | Checkup",
                    LogLevel.Info);
                return;
            }

            string id = args[0].Trim();
            string injuryId = args[1].Trim();
            int days = 3;
            if (args.Length >= 3 && !int.TryParse(args[2], out days))
            {
                Monitor.Log("days должно быть целым числом.", LogLevel.Warn);
                return;
            }

            _prescriptionManager.AddPrescription(id, injuryId, days);
            Monitor.Log($"Предписание {id} добавлено для {injuryId} на {days} дн.", LogLevel.Info);
        }

        private void CmdPrescriptionClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _prescriptionManager.ClearAllPrescriptions();
            Monitor.Log("Все предписания сняты, TreatmentComplianceScore сброшен (0).", LogLevel.Info);
        }

        private void CmdComplianceSet(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0 || !int.TryParse(args[0], out int score))
            {
                Monitor.Log("Использование: injury_compliance_set <number>", LogLevel.Info);
                return;
            }

            _complianceManager.SetScore(score);
            _complianceManager.ApplyTreatmentComplianceTopics();
            Monitor.Log(
                $"TreatmentComplianceScore = {score} ({ComplianceManager.GetLevelDisplayName(_complianceManager.GetComplianceLevel())})",
                LogLevel.Info);
        }

        private void CmdTrustGet()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log($"CareTrust: {_careTrustManager.GetTrust()}", LogLevel.Info);
        }

        private void CmdTrustSet(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0 || !int.TryParse(args[0], out int value))
            {
                Monitor.Log("Использование: injury_trust_set <value>", LogLevel.Info);
                return;
            }

            _careTrustManager.SetTrust(value, "debug_console_set");
            Monitor.Log(
                $"CareTrust = {_careTrustManager.GetTrust()} ({_careTrustManager.GetLevel()})",
                LogLevel.Info);
        }

        private void CmdTrustAdd(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0 || !int.TryParse(args[0], out int delta))
            {
                Monitor.Log("Использование: injury_trust_add <value>", LogLevel.Info);
                return;
            }

            if (delta > 0)
                _careTrustManager.AddTrust(delta, "debug_console_add");
            else if (delta < 0)
                _careTrustManager.PenalizeTrust(-delta, "debug_console_add");

            Monitor.Log(
                $"CareTrust = {_careTrustManager.GetTrust()} ({_careTrustManager.GetLevel()})",
                LogLevel.Info);
        }

        private void CmdTrustLevel()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log(
                $"CareTrust level: {_careTrustManager.GetLevel()} (value={_careTrustManager.GetTrust()})",
                LogLevel.Info);
        }

        private void CmdTestPrescriptionViolation(string[] args)
        {
            string kind = args.Length > 0 ? args[0] : "NoMine";
            PrescriptionViolationTest.Run(
                Monitor,
                _prescriptionManager,
                _complianceManager,
                _dialogueManager,
                _stateManager,
                kind);
        }

        private void CmdCheckupDue(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_checkup_due <buffId>. Пример: injury_checkup_due buffDeepCuts",
                    LogLevel.Info);
                return;
            }

            string buffId = args[0].Trim();
            if (!_checkupManager.DebugForceCheckupDue(buffId))
            {
                Monitor.Log(
                    $"Не удалось выставить осмотр для «{buffId}». Нужна фазовая травма с начатым лечением.",
                    LogLevel.Warn);
                return;
            }
            var ds = _stateManager.GetDebuffState(buffId)!;
            Monitor.Log(
                $"[Checkup] debug: {buffId} ReadySinceDay={ds.ReadySinceDay}, " +
                $"Next={ds.ReadyForNextPhase}, Recovery={ds.ReadyForRecovery}",
                LogLevel.Info);
        }

        private void CmdRehabStart(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_rehab_start <buffId> [days]. Пример: injury_rehab_start buffConcussion 3",
                    LogLevel.Info);
                return;
            }

            string buffId = args[0].Trim();
            int? days = null;
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out int parsed) || parsed <= 0)
                {
                    Monitor.Log("days должно быть положительным целым числом.", LogLevel.Warn);
                    return;
                }

                days = parsed;
            }

            if (_rehabManager.IsRehabActive())
                _rehabManager.ClearRehab();

            _rehabManager.StartRehab(buffId, days);
            Monitor.Log($"Реабилитация запущена для {buffId}.", LogLevel.Info);
        }

        private void CmdRehabStatus()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log("=== REHAB ===", LogLevel.Info);
            foreach (string line in _rehabManager.GetStatusLines())
                Monitor.Log($"  {line}", LogLevel.Info);
        }

        private void CmdRehabClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _rehabManager.ClearRehab();
            Monitor.Log("Реабилитация снята.", LogLevel.Info);
        }

        private void CmdRecoveryPlanStart(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            string? injuryId = args.Length > 0 ? args[0].Trim() : null;
            bool started = _recoveryPlanManager.StartHospitalDischargePlan(injuryId);
            Monitor.Log(
                started
                    ? "[RecoveryPlan] План запущен."
                    : "[RecoveryPlan] План не запущен (уже активен или ошибка).",
                LogLevel.Info);
        }

        private void CmdRecoveryPlanFail(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: recovery_plan_fail <reason>", LogLevel.Info);
                return;
            }

            string reason = string.Join(" ", args).Trim();
            _recoveryPlanManager.RegisterHospitalViolation(reason);
            Monitor.Log($"[RecoveryPlan] Нарушение зарегистрировано: {reason}", LogLevel.Info);
        }

        private void CmdRecoveryPlanStatus()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log("=== RECOVERY PLAN ===", LogLevel.Info);
            foreach (string line in _recoveryPlanManager.GetStatusLines())
                Monitor.Log($"  {line}", LogLevel.Info);
        }

        private void CmdRecoveryPlanClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _recoveryPlanManager.ClearRecoveryPlan();
            Monitor.Log("[RecoveryPlan] Hospital-план снят.", LogLevel.Info);
        }

        private void CmdRecoveryViolationTest(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length < 2)
            {
                Monitor.Log(
                    "Использование: recovery_violation_test <type> <severity 1|2|3>\n"
                    + "Типы: low_stamina_farm, overwork, late_night, rain_with_bandage, "
                    + "mine_or_volcano, critical_health, pass_out",
                    LogLevel.Info);
                return;
            }

            if (!TryParseRecoveryViolationSeverity(args[^1], out int severity))
            {
                Monitor.Log("Severity: 1 (mild), 2 (medium) или 3 (severe).", LogLevel.Warn);
                return;
            }

            string type = string.Join(" ", args, 0, args.Length - 1).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(type))
            {
                Monitor.Log("Укажите тип нарушения.", LogLevel.Warn);
                return;
            }

            bool registered = _stateManager.DebugRegisterRecoveryViolation(type, severity);
            Monitor.Log(
                registered
                    ? $"[RecoveryViolation][QA] Зарегистрировано: type={type}, severity={severity}"
                    : $"[RecoveryViolation][QA] Не зарегистрировано (type={type}, severity={severity})",
                LogLevel.Info);
        }

        private void CmdRecoveryViolationStatus()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log("=== RECOVERY VIOLATION STATUS ===", LogLevel.Info);
            foreach (string line in _recoveryPlanManager.GetRecoveryViolationStatusLines())
                Monitor.Log($"  {line}", LogLevel.Info);
        }

        private void CmdRecoveryViolationClear(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            bool clearAll = args.Length > 0
                && string.Equals(args[0].Trim(), "all", StringComparison.OrdinalIgnoreCase);

            _stateManager.ClearRecoveryViolationState(clearAll);
            Monitor.Log(
                clearAll
                    ? "[RecoveryViolation][QA] Все поля и счётчики нарушений сброшены."
                    : "[RecoveryViolation][QA] Поля текущего нарушения сброшены (счётчики сохранены).",
                LogLevel.Info);
        }

        private static bool TryParseRecoveryViolationSeverity(string raw, out int severity)
        {
            severity = RecoveryViolationSeverity.None;
            string normalized = raw.Trim().ToLowerInvariant();

            switch (normalized)
            {
                case "1":
                case "mild":
                    severity = RecoveryViolationSeverity.Mild;
                    return true;
                case "2":
                case "medium":
                    severity = RecoveryViolationSeverity.Medium;
                    return true;
                case "3":
                case "severe":
                    severity = RecoveryViolationSeverity.Severe;
                    return true;
                default:
                    return int.TryParse(normalized, out severity)
                        && severity is >= RecoveryViolationSeverity.Mild and <= RecoveryViolationSeverity.Severe;
            }
        }

        private void CmdInjuryPlanShow()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            var plan = _recoveryPlanManager.GetDailyPlan();
            Monitor.Log("=== RECOVERY PLAN (daily) ===", LogLevel.Info);
            Monitor.Log($"  IsActive: {plan.IsActive}  Source: {plan.Source}  PlanId: {ValueOrEmpty(plan.PlanId)}", LogLevel.Info);
            Monitor.Log($"  HarveyTone: {plan.HarveyTone}", LogLevel.Info);
            Monitor.Log($"  active injury: {plan.ActiveInjuryId ?? "(none)"}", LogLevel.Info);
            Monitor.Log($"  day: {plan.CurrentDay}/{plan.TotalDays}  phase: {plan.CurrentPhase}/{plan.TotalPhases}", LogLevel.Info);
            Monitor.Log($"  status: {plan.Status}  concern: {plan.ConcernScore}", LogLevel.Info);
            Monitor.Log($"  needsHarvey: {plan.NeedsHarveyVisit}", LogLevel.Info);
            Monitor.Log($"  ActiveAssignments: [{string.Join(", ", plan.ActiveAssignments)}]", LogLevel.Info);
            Monitor.Log($"  CompletedToday: [{string.Join(", ", plan.CompletedAssignmentsToday)}]", LogLevel.Info);
            foreach (var id in plan.Progress.Keys.Union(plan.Goals.Keys))
            {
                plan.Progress.TryGetValue(id, out int progress);
                plan.Goals.TryGetValue(id, out int goal);
                Monitor.Log($"  progress {id}: {progress}/{goal}", LogLevel.Info);
            }
            foreach (string warning in plan.TodayWarnings)
                Monitor.Log($"  warning: {warning}", LogLevel.Info);
            foreach (var task in plan.Tasks)
                Monitor.Log($"  task {task.Id}: {task.Title} failed={task.IsFailed}", LogLevel.Info);
            foreach (var v in plan.TodayViolations)
                Monitor.Log($"  violation {v.Id} day={v.Day} time={v.TimeOfDay}", LogLevel.Info);
            foreach (string reason in plan.TodayViolationReasons)
                Monitor.Log($"  reason: {reason}", LogLevel.Info);
        }

        private static string ValueOrEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "(none)" : value;

        private void RegisterHarveyPlanConsoleCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add(
                "harvey_ladder",
                "Лестница последствий травмы: harvey_ladder [4|revisit|5|reset]. Без аргумента — состояние. "
                + "Нужна main-травма, о которой знает Харви, и «Небрежность» (или нелеченая инфекция), иначе лестница сбросится.",
                (_, args) =>
                {
                    if (!Context.IsWorldReady)
                    {
                        Monitor.Log("Мир не загружен.", LogLevel.Warn);
                        return;
                    }

                    Monitor.Log(
                        args.Length == 0 ? _neglectLadderManager.Describe() : _neglectLadderManager.QaSetStage(args[0]),
                        LogLevel.Info);
                });
            helper.ConsoleCommands.Add("harvey_plan_show", "Показать RecoveryPlan (save-state).", (_, _) => CmdInjuryPlanShow());
            helper.ConsoleCommands.Add("harvey_plan_reset", "Сбросить RecoveryPlan.", (_, _) => CmdHarveyPlanReset());
            helper.ConsoleCommands.Add(
                "harvey_plan_add",
                "Добавить назначение: harvey_plan_add <assignmentId> [goal]",
                (_, args) => CmdHarveyPlanAdd(args));
            helper.ConsoleCommands.Add(
                "harvey_plan_complete",
                "Завершить назначение: harvey_plan_complete <assignmentId>",
                (_, args) => CmdHarveyPlanComplete(args));
            helper.ConsoleCommands.Add(
                "harvey_plan_violate",
                "Нарушение плана: harvey_plan_violate <type>",
                (_, args) => CmdHarveyPlanViolate(args));
            helper.ConsoleCommands.Add("injury_plan_debug", "Debug HUD блок RecoveryPlan (Injury). Снапшот плана Core: harvey_plan_debug.", (_, _) => CmdHarveyPlanDebug());
        }

        private void CmdInjuryPanelDump()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _recoveryPlanManager.EnsurePlanFreshForDisplay();
            var contribution = _injuryPanelProvider.GetPanelContribution();
            var directives = _injuryCareDirectiveProvider.GetCareDirectives();
            Monitor.Log("=== Injury → Harvey Panel (flags) + Care Directives ===", LogLevel.Info);
            Monitor.Log($"HasActiveRecoveryPlan={contribution.HasActiveRecoveryPlan}", LogLevel.Info);
            Monitor.Log($"CareDirectives={directives.Count} (Plan tab renders in Core)", LogLevel.Info);
            foreach (var directive in directives)
            {
                Monitor.Log(
                    $"  id={directive.Id} type={directive.Type} pri={directive.Priority} title={directive.Title}",
                    LogLevel.Info);
            }

            string? activeId = _injuryManager.GetActiveInTreatmentInjuryId();
            Monitor.Log($"GetActiveInTreatmentInjuryId={ValueOrEmpty(activeId)}", LogLevel.Info);
            foreach (var (injuryId, debuff) in _injuryManager.GetInjuriesForHarveyPanel())
            {
                Monitor.Log(
                    $"  injury={injuryId} phase={debuff.CurrentPhase}/{debuff.TotalPhases} "
                    + $"TreatmentStarted={debuff.TreatmentStarted} IsInTreatment={debuff.IsInTreatment}",
                    LogLevel.Info);
            }
        }

        private void CmdHarveyPlanReset()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _recoveryPlanManager.ClearDailyPlan();
            Monitor.Log("[RecoveryPlan] Plan reset.", LogLevel.Info);
        }

        private void CmdHarveyPlanAdd(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: harvey_plan_add <assignmentId> [goal]", LogLevel.Info);
                return;
            }

            int goal = args.Length > 1 && int.TryParse(args[1], out int parsed) ? parsed : 0;
            _recoveryPlanManager.AddAssignment(args[0], goal);
            Monitor.Log($"[RecoveryPlan] Added assignment {args[0]} goal={goal}", LogLevel.Info);
        }

        private void CmdHarveyPlanComplete(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: harvey_plan_complete <assignmentId>", LogLevel.Info);
                return;
            }

            bool ok = _recoveryPlanManager.CompleteAssignment(args[0]);
            Monitor.Log($"[RecoveryPlan] CompleteAssignment {args[0]} => {ok}", LogLevel.Info);
        }

        private void CmdHarveyPlanViolate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: harvey_plan_violate mine|stamina|health|sleep|rain|...", LogLevel.Info);
                return;
            }

            _recoveryPlanManager.RegisterViolation(args[0], $"Тестовое нарушение: {args[0]}");
            Monitor.Log($"[RecoveryPlan] Violation registered: {args[0]}", LogLevel.Info);
        }

        private void CmdHarveyPlanDebug()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            foreach (string line in _recoveryPlanManager.GetDebugHudBlock().Split('\n'))
                Monitor.Log(line, LogLevel.Info);
        }

        private void CmdInjuryPlanRefresh()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _recoveryPlanManager.RefreshPlanForToday(notifyUpdated: true);
            Monitor.Log("[RecoveryPlan] План пересобран.", LogLevel.Info);
        }

        private void CmdInjuryPlanViolate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: injury_plan_violate mines|stamina|health|sleep|rain|missed_checkup", LogLevel.Info);
                return;
            }

            _recoveryPlanManager.AddTestViolation(args[0]);
            Monitor.Log($"[RecoveryPlan] Тестовое нарушение: {args[0]}", LogLevel.Info);
        }

        private void CmdInjuryPlanClear()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _recoveryPlanManager.ClearDailyPlan();
            Monitor.Log("[RecoveryPlan] Daily plan cleared.", LogLevel.Info);
        }

        private void CmdRecoveryViolate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: recovery_violate mine|stamina|health|night|rain",
                    LogLevel.Info);
                return;
            }

            bool registered = _recoveryPlanManager.DebugViolateRecoveryPlan(args[0]);
            if (!registered)
            {
                Monitor.Log(
                    $"[RecoveryPlan] Не удалось зарегистрировать нарушение: {args[0]}. "
                    + "Проверьте аргумент и активен ли план восстановления.",
                    LogLevel.Warn);
                return;
            }

            var plan = _recoveryPlanManager.GetDailyPlan();
            Monitor.Log(
                $"[RecoveryPlan] Нарушение: {args[0]} → type={plan.LastViolationType}, "
                + $"severity={plan.LastViolationSeverity}, topic={plan.TodayViolationDialogueType}",
                LogLevel.Info);
        }

        private void CmdRecoveryComplete(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("Использование: recovery_complete perfect|warnings|violated", LogLevel.Info);
                return;
            }

            if (!_recoveryPlanManager.DebugCompleteRecoveryPlan(args[0]))
            {
                Monitor.Log(
                    $"[RecoveryPlan] Неизвестный аргумент: {args[0]}. Используйте perfect, warnings или violated.",
                    LogLevel.Warn);
                return;
            }

            Monitor.Log($"[RecoveryPlan] Completion применён: {args[0]}", LogLevel.Info);
        }

        private void CmdInjuryRecoveryStatus()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log("=== RECOVERY PLAN STATUS ===", LogLevel.Info);
            foreach (string line in _recoveryPlanManager.GetRecoveryStatusLines())
                Monitor.Log($"  {line}", LogLevel.Info);
        }

        private void CmdInjuryRecoveryCompleteDebug(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log(
                    "Использование: injury_recovery_complete_debug perfect|warnings|violated",
                    LogLevel.Info);
                return;
            }

            if (!_recoveryPlanManager.DebugCompleteRecoveryPlan(args[0]))
            {
                Monitor.Log(
                    $"[RecoveryPlan] Неизвестный аргумент: {args[0]}. Используйте perfect, warnings или violated.",
                    LogLevel.Warn);
                return;
            }

            Monitor.Log($"[RecoveryPlan] Debug completion: {args[0]}", LogLevel.Info);
        }

        private void CmdInjuryRecoveryReset()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            _recoveryPlanManager.ResetRecoveryPlanOnly();
            Monitor.Log("[RecoveryPlan] Сброшен (травмы не затронуты).", LogLevel.Info);
        }

        private void CmdInjuryRecoveryViolate(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            if (args.Length < 2)
            {
                Monitor.Log(
                    "Использование: injury_recovery_violate <type> <severity>\n"
                    + "  type: Mine|LowStamina|LowHealth|LateNight|Rain|IgnoredCheckup|PassedOut\n"
                    + "  severity: 1 (mild) | 2 (medium) | 3 (severe)\n"
                    + "Примеры: injury_recovery_violate Mine 3 | injury_recovery_violate LowStamina 1",
                    LogLevel.Info);
                return;
            }

            if (!int.TryParse(args[1], out int severity) || severity < 1 || severity > 3)
            {
                Monitor.Log("severity должен быть 1, 2 или 3.", LogLevel.Warn);
                return;
            }

            bool registered = _recoveryPlanManager.DebugHandleRecoveryPlanViolation(args[0], severity);
            if (!registered)
            {
                Monitor.Log(
                    $"[RecoveryPlan] Не удалось: {args[0]} severity={severity}. "
                    + "Проверьте тип и активен ли план.",
                    LogLevel.Warn);
                return;
            }

            var plan = _recoveryPlanManager.GetDailyPlan();
            Monitor.Log(
                $"[RecoveryPlan] Violation: {args[0]} sev={severity} → "
                + $"type={plan.LastViolationType}, "
                + $"extensions={plan.ExtensionCount}/{_recoveryPlanManager.GetMaxExtensions()}, "
                + $"maxReached={plan.MaxExtensionsReached}, "
                + $"strictFollowUp={plan.NeedsStrictFollowUp}, "
                + $"reasons=[{string.Join(", ", plan.TodayViolationReasons)}]",
                LogLevel.Info);
        }

        private void CmdRecoveryDebug()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log("=== RECOVERY PLAN DEBUG ===", LogLevel.Info);
            foreach (string line in _recoveryPlanManager.GetStatusLines())
                Monitor.Log($"  {line}", LogLevel.Info);
        }

        private void CmdSelfCareBandage()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log(
                _selfCareManager.ApplyCleanBandage(force: true)
                    ? "[SelfCare] CleanBandage применён."
                    : "[SelfCare] CleanBandage не применён (нет условий).",
                LogLevel.Info);
        }

        private void CmdSelfCareTea()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log(
                _selfCareManager.ApplyWarmTea(force: true)
                    ? "[SelfCare] WarmTea применён."
                    : "[SelfCare] WarmTea не применён (нет простуды).",
                LogLevel.Info);
        }

        private void CmdSelfCareRest()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Сначала загрузите сохранение.", LogLevel.Warn);
                return;
            }

            Monitor.Log(
                _selfCareManager.ApplyRestCare(force: true)
                    ? "[SelfCare] RestCare применён."
                    : "[SelfCare] RestCare не применён.",
                LogLevel.Info);
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
    }
}
