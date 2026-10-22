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
    /// <summary>MCP-инструменты для автотестов: очередь команд и выполнение на главном потоке.</summary>
    public sealed partial class ModEntry
    {
        private string ExecuteMcpTool(string toolName, JsonElement? arguments)
        {
            var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _mcpCommandQueue.Enqueue((toolName, arguments, completion));

            if (!completion.Task.Wait(TimeSpan.FromSeconds(30)))
                return "Error: timeout waiting for game thread (30s). Is the game running?";

            return completion.Task.GetAwaiter().GetResult();
        }

        private void OnProcessMcpCommandQueue(object? sender, UpdateTickedEventArgs e)
        {
            while (_mcpCommandQueue.TryDequeue(out var item))
            {
                try
                {
                    item.Completion.TrySetResult(ExecuteMcpToolOnMainThread(item.Tool, item.Args));
                }
                catch (Exception ex)
                {
                    item.Completion.TrySetResult($"Error: {ex.Message}");
                }
            }
        }

        private string ExecuteMcpToolOnMainThread(string toolName, JsonElement? arguments)
        {
            switch (toolName)
            {
                case "injury_reset":
                    FullReset();
                    return BuildPhaseListReport();

                case "injury_debuff_list":
                    return BuildDebuffListReport();

                case "injury_debuff_add":
                {
                    if (!McpTryGetString(arguments, "buff_id", out string? buffId) || string.IsNullOrWhiteSpace(buffId))
                        return "Error: buff_id is required.";

                    var cmdArgs = new List<string>();
                    if (McpTryGetBool(arguments, "force", out bool force) && force)
                        cmdArgs.Add("--force");
                    cmdArgs.Add(buffId.Trim());
                    if (McpTryGetInt(arguments, "minutes", out int minutes))
                        cmdArgs.Add(minutes.ToString());
                    CmdDebuffAdd(cmdArgs.ToArray());
                    return BuildPhaseListReport();
                }

                case "injury_phase_list":
                    return BuildPhaseListReport();

                case "injury_debug_dump":
                    if (!Context.IsWorldReady)
                        return "Error: load a save first.";
                    return BuildDebugReport(full: true);

                case "injury_state_dump":
                    return BuildStateDumpReport();

                case "injury_buff_dump":
                    return BuildBuffDumpReport();

                case "injury_topic_dump":
                    return BuildTopicDumpReport();

                case "injury_validate_buffs":
                {
                    string validateReport = BuildValidateBuffsReport();
                    if (validateReport.StartsWith("Error:", StringComparison.Ordinal))
                        return validateReport;
                    string summary = validateReport.StartsWith("result=OK", StringComparison.Ordinal)
                        ? "[QA] injury_validate_buffs: OK"
                        : "[QA] injury_validate_buffs: " + validateReport.Replace("result=MISSING ", "MISSING ");
                    return summary + "\n" + validateReport;
                }

                case "injury_main_clear":
                    CmdMainClear();
                    return BuildPhaseListReport();

                case "injury_main_set":
                    if (!McpTryGetString(arguments, "buff_id", out string? mainBuffId) || string.IsNullOrWhiteSpace(mainBuffId))
                        return "Error: buff_id is required.";
                    CmdMainSet(new[] { mainBuffId.Trim() });
                    return BuildPhaseListReport();

                case "injury_phase_ready":
                    if (!McpTryGetString(arguments, "buff_id", out string? readyBuffId) || string.IsNullOrWhiteSpace(readyBuffId))
                        return "Error: buff_id is required.";
                    bool readyForNext = !McpTryGetBool(arguments, "ready", out bool r1) || r1;
                    CmdPhaseReady(new[] { readyBuffId.Trim(), readyForNext ? "1" : "0" });
                    return BuildPhaseListReport();

                case "injury_phase_recovery":
                    if (!McpTryGetString(arguments, "buff_id", out string? recoveryBuffId) || string.IsNullOrWhiteSpace(recoveryBuffId))
                        return "Error: buff_id is required.";
                    bool readyForRecovery = !McpTryGetBool(arguments, "ready", out bool r2) || r2;
                    CmdPhaseRecovery(new[] { recoveryBuffId.Trim(), readyForRecovery ? "1" : "0" });
                    return BuildPhaseListReport();

                case "injury_phase_advance":
                    if (!McpTryGetString(arguments, "buff_id", out string? advanceBuffId) || string.IsNullOrWhiteSpace(advanceBuffId))
                        return "Error: buff_id is required.";
                    CmdPhaseAdvance(new[] { advanceBuffId.Trim() });
                    return BuildPhaseListReport();

                case "injury_phase_cure":
                    if (!McpTryGetString(arguments, "buff_id", out string? cureBuffId) || string.IsNullOrWhiteSpace(cureBuffId))
                        return "Error: buff_id is required.";
                    CmdPhaseCure(new[] { cureBuffId.Trim() });
                    return BuildPhaseListReport();

                case "injury_mine_dirty_debug":
                    return BuildMineDirtyDebugReport();

                case "injury_rain_debug":
                {
                    var rainArgs = new List<string>();
                    if (McpTryGetInt(arguments, "seconds_today", out int secondsToday))
                        rainArgs.Add(secondsToday.ToString());
                    if (McpTryGetInt(arguments, "continuous_seconds", out int continuousSeconds))
                        rainArgs.Add(continuousSeconds.ToString());
                    CmdRainDebug(rainArgs.ToArray());
                    if (!Context.IsWorldReady)
                        return "Error: load a save first.";
                    var state = _stateManager.State;
                    return
                        $"Rain debug: TotalTimeUnderRainToday={state.TotalTimeUnderRainToday}s, " +
                        $"TimeUnderRainTicks={state.TimeUnderRainTicks}s, LastRainDay={state.LastRainDay}";
                }

                case "injury_debug_mine_rescue":
                    CmdDebugMineRescue();
                    return BuildPhaseListReport();

                case "injury_cleanup":
                    CmdCleanupLingeringMedical();
                    return BuildPhaseListReport();

                case "injury_cleanup_invalid_complications":
                    CmdCleanupInvalidComplications();
                    return BuildPhaseListReport();

                case "injury_foreign_topic_add":
                    if (!McpTryGetString(arguments, "topic_id", out string? topicId) || string.IsNullOrWhiteSpace(topicId))
                        return "Error: topic_id is required.";
                    var topicArgs = new List<string> { topicId.Trim() };
                    if (McpTryGetInt(arguments, "days", out int days))
                        topicArgs.Add(days.ToString());
                    CmdForeignTopicAdd(topicArgs.ToArray());
                    return BuildPhaseListReport();

                case "injury_harvey_click":
                {
                    bool dryRun = McpTryGetBool(arguments, "dry_run", out bool dry) && dry;
                    bool ignoreHospital = McpTryGetBool(arguments, "ignore_hospital", out bool ignoreH) && ignoreH;
                    if (McpTryGetBool(arguments, "discharge_if_hospitalized", out bool discharge) && discharge
                        && _hospitalizationManager.IsHospitalized)
                    {
                        RunHospitalDischarge();
                    }

                    string clickResult = _interactionHandler.TryDebugApplyHarveyMedicalAction(dryRun, ignoreHospital);
                    return clickResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_run_daily_checks":
                    return _gameEventHandler.RunQaDailyChecks() + "\n\n" + BuildPhaseListReport();

                case "injury_mine_dirty_simulate":
                {
                    if (!McpTryGetInt(arguments, "minutes", out int simMinutes))
                        return "Error: minutes is required.";
                    bool forceRoll = McpTryGetBool(arguments, "force_roll", out bool fr) && fr;
                    bool ignoreLoc = !McpTryGetBool(arguments, "require_mine", out bool requireMine) || !requireMine;
                    string simResult = _playerEventHandler.SimulateMineDirtyExposureForQa(simMinutes, forceRoll, ignoreLoc);
                    return "[QA] " + simResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_main_migrate":
                {
                    string migrated = _stateManager.DebugMigrateMainInjuryId();
                    return $"[QA] injury_main_migrate MainInjuryId={migrated}\n\n" + BuildPhaseListReport();
                }

                case "injury_neglect_set":
                {
                    if (!McpTryGetString(arguments, "buff_id", out string? neglectBuff) || string.IsNullOrWhiteSpace(neglectBuff))
                        return "Error: buff_id is required.";
                    if (!McpTryGetInt(arguments, "strikes", out int neglectStrikes))
                        return "Error: strikes is required.";
                    string neglectResult = RunNeglectSet(neglectBuff.Trim(), neglectStrikes);
                    return "[QA] " + neglectResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_mine_warning_simulate":
                {
                    bool warningYesterday = McpTryGetBool(arguments, "warning_was_yesterday", out bool wy) && wy;
                    string warningResult = _playerEventHandler.SimulateMineSevereWarningForQa(warningYesterday);
                    return "[QA] injury_mine_warning_simulate " + warningResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_mine_forbidden_clear":
                    CmdMineForbiddenClear();
                    return BuildPhaseListReport();

                case "injury_mine_clear":
                    CmdMineClear();
                    return BuildPhaseListReport();

                case "injury_mine_forbid":
                {
                    if (!McpTryGetInt(arguments, "days", out int forbidDays))
                        forbidDays = _config.MineForbiddenDurationDays;
                    CmdMineForbid(new[] { forbidDays.ToString() });
                    return BuildPhaseListReport();
                }

                case "injury_mine_status":
                case "injury_mine_forbidden_status":
                {
                    if (!Context.IsWorldReady)
                        return "Error: load a save first.";
                    return MineForbiddenHelper.BuildStatusReport(
                        _stateManager.State,
                        _config,
                        _injuryManager,
                        _buffManager,
                        (int)Game1.stats.DaysPlayed);
                }

                case "injury_mine_restriction_clear":
                    CmdMineRestrictionClear();
                    return BuildPhaseListReport();

                case "injury_location_logic":
                {
                    string locationResult = _playerEventHandler.RunLocationLogicForQa();
                    return "[QA] injury_location_logic " + locationResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_rain_wet_simulate":
                {
                    bool wetForce = !McpTryGetBool(arguments, "noroll", out bool noroll) || !noroll;
                    string wetResult = _playerEventHandler.SimulateRainWetBandageForQa(wetForce);
                    return "[QA] injury_rain_wet_simulate " + wetResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_hospital_lock_enforce":
                {
                    string lockResult = _hospitalizationManager.EnforceLockForQa();
                    return "[QA] injury_hospital_lock_enforce " + lockResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_game_ui_status":
                    return QaGameUiCommands.BuildUiStatusReport();

                case "injury_game_ui_advance":
                {
                    int uiSteps = 1;
                    if (McpTryGetInt(arguments, "steps", out int parsedSteps))
                        uiSteps = parsedSteps;
                    return QaGameUiCommands.AdvanceUiMany(uiSteps);
                }

                case "injury_game_ui_end_event":
                    return QaGameUiCommands.EndActiveEvent();

                case "injury_game_ui_close_menu":
                    return QaGameUiCommands.CloseActiveMenu();

                case "injury_topic_add":
                {
                    var topicAddArgs = new List<string>();
                    if (McpTryGetString(arguments, "topic_id", out string? topicAddId) && !string.IsNullOrWhiteSpace(topicAddId))
                        topicAddArgs.Add(topicAddId.Trim());
                    if (McpTryGetInt(arguments, "days", out int topicDays))
                        topicAddArgs.Add(topicDays.ToString());
                    string topicAddResult = RunTopicAdd(topicAddArgs.ToArray());
                    if (topicAddResult.StartsWith("Usage:", StringComparison.Ordinal)
                        || topicAddResult.StartsWith("Error:", StringComparison.Ordinal))
                        return topicAddResult;
                    return "[QA] injury_topic_add " + topicAddResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_topic_remove":
                {
                    if (!McpTryGetString(arguments, "topic_id", out string? topicRemoveId) || string.IsNullOrWhiteSpace(topicRemoveId))
                        return "Error: topic_id is required.";
                    string topicRemoveResult = RunTopicRemove(new[] { topicRemoveId.Trim() });
                    return "[QA] injury_topic_remove " + topicRemoveResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_complication_add":
                {
                    var compAddArgs = new List<string>();
                    if (McpTryGetString(arguments, "complication_id", out string? compAddId) && !string.IsNullOrWhiteSpace(compAddId))
                        compAddArgs.Add(compAddId.Trim());
                    if (McpTryGetInt(arguments, "age_days", out int ageDays))
                        compAddArgs.Add(ageDays.ToString());
                    string compAddResult = RunComplicationAdd(compAddArgs.ToArray());
                    if (compAddResult.StartsWith("Usage:", StringComparison.Ordinal)
                        || compAddResult.StartsWith("Error:", StringComparison.Ordinal))
                        return compAddResult;
                    return "[QA] injury_complication_add " + compAddResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_complication_remove":
                {
                    if (!McpTryGetString(arguments, "complication_id", out string? compRemoveId) || string.IsNullOrWhiteSpace(compRemoveId))
                        return "Error: complication_id is required.";
                    string compRemoveResult = RunComplicationRemove(new[] { compRemoveId.Trim() });
                    return "[QA] injury_complication_remove " + compRemoveResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_test_age_injury":
                {
                    if (!McpTryGetString(arguments, "buff_id", out string? ageBuffId) || string.IsNullOrWhiteSpace(ageBuffId))
                        return "Error: buff_id is required.";
                    if (!McpTryGetInt(arguments, "days_back", out int injuryDaysBack))
                        return "Error: days_back is required.";
                    string ageInjuryResult = RunTestAgeInjury(new[] { ageBuffId.Trim(), injuryDaysBack.ToString() });
                    if (ageInjuryResult.StartsWith("Usage:", StringComparison.Ordinal)
                        || ageInjuryResult.StartsWith("Error:", StringComparison.Ordinal))
                        return ageInjuryResult;
                    return "[QA] injury_test_age_injury " + ageInjuryResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_test_age_complication":
                {
                    if (!McpTryGetString(arguments, "complication_id", out string? ageCompId) || string.IsNullOrWhiteSpace(ageCompId))
                        return "Error: complication_id is required.";
                    if (!McpTryGetInt(arguments, "days_back", out int compDaysBack))
                        return "Error: days_back is required.";
                    string ageCompResult = RunTestAgeComplication(new[] { ageCompId.Trim(), compDaysBack.ToString() });
                    if (ageCompResult.StartsWith("Usage:", StringComparison.Ordinal)
                        || ageCompResult.StartsWith("Error:", StringComparison.Ordinal))
                        return ageCompResult;
                    return "[QA] injury_test_age_complication " + ageCompResult + "\n\n" + BuildPhaseListReport();
                }

                case "injury_hospital_status":
                    return "[QA] injury_hospital_status\n" + BuildHospitalStatusReport();

                case "injury_hospital_progress":
                {
                    if (!McpTryGetInt(arguments, "minutes", out int hospMinutes))
                        return "Error: minutes is required.";
                    CmdHospitalProgress(new[] { hospMinutes.ToString() });
                    return "[QA] injury_hospital_progress\n" + BuildHospitalStatusReport();
                }

                case "injury_hospital_reset":
                    CmdHospitalReset();
                    return "[QA] injury_hospital_reset ok\n" + BuildHospitalStatusReport();

                case "injury_hospital_discharge":
                {
                    string dischargeResult = RunHospitalDischarge();
                    if (dischargeResult.StartsWith("Error:", StringComparison.Ordinal))
                        return dischargeResult;
                    return "[QA] injury_hospital_discharge " + dischargeResult + "\n\n" + BuildPhaseListReport();
                }

                default:
                    return $"Error: unknown tool '{toolName}'.";
            }
        }

        private static bool McpTryGetString(JsonElement? arguments, string name, out string? value)
        {
            value = null;
            if (arguments is not { ValueKind: JsonValueKind.Object } args)
                return false;
            if (!args.TryGetProperty(name, out JsonElement el) || el.ValueKind != JsonValueKind.String)
                return false;
            value = el.GetString();
            return true;
        }

        private static bool McpTryGetBool(JsonElement? arguments, string name, out bool value)
        {
            value = false;
            if (arguments is not { ValueKind: JsonValueKind.Object } args)
                return false;
            if (!args.TryGetProperty(name, out JsonElement el))
                return false;
            if (el.ValueKind == JsonValueKind.True) { value = true; return true; }
            if (el.ValueKind == JsonValueKind.False) { value = false; return true; }
            return false;
        }

        private static bool McpTryGetInt(JsonElement? arguments, string name, out int value)
        {
            value = 0;
            if (arguments is not { ValueKind: JsonValueKind.Object } args)
                return false;
            if (!args.TryGetProperty(name, out JsonElement el))
                return false;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out int n))
            {
                value = n;
                return true;
            }
            return false;
        }
    }
}
