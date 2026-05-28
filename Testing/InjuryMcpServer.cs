using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StardewModdingAPI;

namespace HarveyOverhaul.InjuryCare.Testing
{
    /// <summary>
    /// JSON-RPC MCP over HTTP for Harvey Overhaul Injury debug commands (Cursor / agents).
    /// Protocol compatible with StardewMCP (initialize, tools/list, tools/call).
    /// </summary>
    public sealed class InjuryMcpServer : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly IMonitor _monitor;
        private readonly Func<string, JsonElement?, string> _executeTool;
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _loopTask;

        public InjuryMcpServer(IMonitor monitor, Func<string, JsonElement?, string> executeTool)
        {
            _monitor = monitor;
            _executeTool = executeTool;
        }

        public void Start(int port)
        {
            if (_listener != null)
                return;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            _listener.Prefixes.Add($"http://localhost:{port}/");

            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                _monitor.Log(
                    $"[InjuryMCP] Не удалось запустить HTTP на порту {port}: {ex.Message}. " +
                    "Проверьте, что порт свободен, или измените InjuryMcpPort в config.json.",
                    LogLevel.Error);
                _listener = null;
                return;
            }

            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => ListenLoopAsync(_cts.Token));
            _monitor.Log($"[InjuryMCP] listening on http://localhost:{port}", LogLevel.Info);
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
                _listener?.Close();
            }
            catch
            {
                // ignore shutdown races
            }
            finally
            {
                _listener = null;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async Task ListenLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener != null && _listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (HttpListenerException)
                {
                    break;
                }

                _ = Task.Run(() => HandleContextAsync(context), cancellationToken);
            }
        }

        private async Task HandleContextAsync(HttpListenerContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = await reader.ReadToEndAsync();

                string responseJson = string.IsNullOrWhiteSpace(body)
                    ? BuildError(null, -32700, "Parse error")
                    : HandleRequest(body);

                byte[] bytes = Encoding.UTF8.GetBytes(responseJson);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                _monitor.Log($"[InjuryMCP] Request error: {ex}", LogLevel.Warn);
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch
                {
                    // ignore
                }
            }
        }

        private string HandleRequest(string body)
        {
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (JsonException)
            {
                return BuildError(null, -32700, "Parse error");
            }

            using (doc)
            {
                JsonElement root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return BuildError(null, -32600, "Invalid Request");

                string? method = root.TryGetProperty("method", out JsonElement methodEl) && methodEl.ValueKind == JsonValueKind.String
                    ? methodEl.GetString()
                    : null;

                if (string.IsNullOrEmpty(method))
                    return BuildError(GetId(root), -32600, "Invalid Request");

                if (method == "notifications/initialized")
                    return string.Empty;

                JsonElement? id = root.TryGetProperty("id", out JsonElement idEl) ? idEl : null;

                return method switch
                {
                    "initialize" => BuildResult(id, new
                    {
                        protocolVersion = "2024-11-05",
                        capabilities = new { tools = new { } },
                        serverInfo = new { name = "HarveyOverhaulInjury", version = "1.0.0" },
                    }),
                    "tools/list" => BuildResult(id, new { tools = InjuryMcpTools.All }),
                    "tools/call" => HandleToolsCall(root, id),
                    _ => BuildError(id, -32601, $"Method not found: {method}"),
                };
            }
        }

        private string HandleToolsCall(JsonElement root, JsonElement? id)
        {
            if (!root.TryGetProperty("params", out JsonElement parameters) || parameters.ValueKind != JsonValueKind.Object)
                return BuildError(id, -32602, "Invalid params");

            if (!parameters.TryGetProperty("name", out JsonElement nameEl) || nameEl.ValueKind != JsonValueKind.String)
                return BuildError(id, -32602, "Tool name required");

            string toolName = nameEl.GetString() ?? string.Empty;
            JsonElement? arguments = parameters.TryGetProperty("arguments", out JsonElement argsEl)
                ? argsEl
                : null;

            try
            {
                string text = _executeTool(toolName, arguments);
                return BuildResult(id, new
                {
                    content = new[]
                    {
                        new { type = "text", text },
                    },
                });
            }
            catch (Exception ex)
            {
                _monitor.Log($"[InjuryMCP] Tool {toolName} failed: {ex}", LogLevel.Error);
                return BuildResult(id, new
                {
                    content = new[]
                    {
                        new { type = "text", text = $"Error: {ex.Message}" },
                    },
                    isError = true,
                });
            }
        }

        private static JsonElement? GetId(JsonElement root) =>
            root.TryGetProperty("id", out JsonElement idEl) ? idEl : null;

        private static string BuildResult(JsonElement? id, object result)
        {
            var payload = new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0",
                ["result"] = result,
            };
            if (id.HasValue)
                payload["id"] = id.Value;

            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        private static string BuildError(JsonElement? id, int code, string message)
        {
            var payload = new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0",
                ["error"] = new { code, message },
            };
            if (id.HasValue)
                payload["id"] = id.Value;

            return JsonSerializer.Serialize(payload, JsonOptions);
        }
    }

    internal static class InjuryMcpTools
    {
        public static readonly object[] All =
        {
            Tool("injury_reset", "Full mod reset: buffs, complications, topics, state."),
            Tool("injury_debuff_list", "List all mod injury and complication buff IDs."),
            Tool("injury_debuff_add", "Apply injury/complication buff. Respects MainInjury unless force=true.",
                Prop("buff_id", "string", "Buff ID, e.g. buffDeepCuts, buffFracturedBone, HarveyMod_DirtyWound"),
                Prop("force", "boolean", "Replace current main injury (default false)"),
                Prop("minutes", "integer", "Buff duration in minutes; omit for full day (-2)")),
            Tool("injury_phase_list", "MainInjuryId, active injuries, phases, complications (read-only)."),
            Tool("injury_debug_dump", "Full debug report (same as F10 full HUD / injury_debug_dump)."),
            Tool("injury_state_dump", "Machine-readable InjuryState snapshot (read-only QA)."),
            Tool("injury_buff_dump", "All player buffs with mod/trauma/phase/cure tags (read-only QA)."),
            Tool("injury_topic_dump", "All conversation topics with days remaining (read-only QA)."),
            Tool("injury_validate_buffs", "Validate C# buff IDs against Data/Buffs; report missing (read-only QA)."),
            Tool("injury_main_clear", "Clear MainInjuryId without removing buffs; suppresses auto-sync until migrate/set."),
            Tool("injury_main_migrate", "Run MainInjuryId migration from ActiveDebuffs (after main_clear)."),
            Tool("injury_main_set", "Set MainInjuryId (requires existing DebuffState).",
                Prop("buff_id", "string", "Main injury buff ID")),
            Tool("injury_phase_ready", "Mark phase ready for advance (phased injuries only).",
                Prop("buff_id", "string", "Injury buff ID"),
                Prop("ready", "boolean", "true/false (default true)")),
            Tool("injury_phase_recovery", "Mark ready for recovery completion.",
                Prop("buff_id", "string", "Injury buff ID"),
                Prop("ready", "boolean", "true/false (default true)")),
            Tool("injury_phase_advance", "Force advance to next treatment phase.",
                Prop("buff_id", "string", "Injury buff ID")),
            Tool("injury_phase_cure", "Complete recovery without Harvey click.",
                Prop("buff_id", "string", "Injury buff ID")),
            Tool("injury_harvey_click",
                "Apply Harvey medical action without dialogue (StartTreatment / AdvancePhase / CompleteRecovery / TreatComplications).",
                Prop("dry_run", "boolean", "If true, only report what would happen"),
                Prop("ignore_hospital", "boolean", "Apply even when hospitalized"),
                Prop("discharge_if_hospitalized", "boolean", "Discharge before apply (default false)")),
            Tool("injury_run_daily_checks",
                "Run DayStarted checks now: buff restore + infection roll + phase flags (after advance_day)."),
            Tool("injury_mine_dirty_debug", "Mine dirty-wound risk state (read-only)."),
            Tool("injury_mine_dirty_simulate",
                "Simulate mine dirty exposure minutes and roll DirtyWound.",
                Prop("minutes", "integer", "Game minutes to add"),
                Prop("force_roll", "boolean", "Use 100% roll chance"),
                Prop("require_mine", "boolean", "If true, player must be in Mine/Volcano")),
            Tool("injury_mine_warning_simulate",
                "Simulate severe mine entry warning (sets MineWarningDay without warp).",
                Prop("warning_was_yesterday", "boolean", "If true, MineWarningDay=today-1 for next daily_checks")),
            Tool("injury_mine_forbidden_clear", "Clear Harvey mine forbidden debuff and warning state."),
            Tool("injury_location_logic",
                "Run HandleLocationLogic for current location (hospital admission, mine warning)."),
            Tool("injury_rain_wet_simulate",
                "Apply WetBandage from rain counters without UpdateTick.",
                Prop("noroll", "boolean", "If true, roll chance instead of force apply")),
            Tool("injury_hospital_lock_enforce",
                "Warp hospitalized player back to clinic bed (StardewMCP warp bypass fix)."),
            Tool("injury_rain_debug", "Show/set rain counters for wet bandage tests.",
                Prop("seconds_today", "integer", "TotalTimeUnderRainToday"),
                Prop("continuous_seconds", "integer", "TimeUnderRainTicks")),
            Tool("injury_debug_mine_rescue", "Arm mine rescue flags for next DayStarted."),
            Tool("injury_cleanup_invalid_complications", "Remove stale/invalid complications from save."),
            Tool("injury_foreign_topic_add", "Add foreign conversation topic for conflict tests.",
                Prop("topic_id", "string", "Topic key"),
                Prop("days", "integer", "Duration in days (default 7)")),
            Tool("injury_topic_add", "Add owned conversation topic (ModTopicRegistry).",
                Prop("topic_id", "string", "Owned topic key"),
                Prop("days", "integer", "Duration in days (default by topic type or 7)")),
            Tool("injury_topic_remove", "Remove one conversation topic.",
                Prop("topic_id", "string", "Topic key")),
            Tool("injury_complication_add", "Apply complication via ComplicationManager (eligibility checks).",
                Prop("complication_id", "string", "Complication buff ID, e.g. HarveyMod_DirtyWound"),
                Prop("age_days", "integer", "Optional: set ActiveComplications start day to today - ageDays")),
            Tool("injury_complication_remove", "Remove one complication (buff + state + topic).",
                Prop("complication_id", "string", "Complication buff ID")),
            Tool("injury_test_age_injury", "Shift DebuffState InjuryStartDay/PhaseStartDay back N days.",
                Prop("buff_id", "string", "Injury buff ID in ActiveDebuffs"),
                Prop("days_back", "integer", "Days to subtract from today for start day")),
            Tool("injury_test_age_complication", "Shift ActiveComplications start day back N days.",
                Prop("complication_id", "string", "Complication buff ID"),
                Prop("days_back", "integer", "Days to subtract from today")),
            Tool("injury_hospital_status", "Read-only hospitalization state snapshot."),
            Tool("injury_hospital_discharge", "Force hospital discharge (HospitalizationManager.Discharge)."),
            Tool("injury_neglect_set", "Set NeglectStrikesByInjury for one buff (QA).",
                Prop("buff_id", "string", "Injury buff ID"),
                Prop("strikes", "integer", "Strike count")),
            Tool("injury_game_ui_status", "Active event/dialogue/menu status (read-only)."),
            Tool("injury_game_ui_advance", "Advance cutscene/dialogue/menu by steps.",
                Prop("steps", "integer", "Steps 1-200 (default 1)")),
            Tool("injury_game_ui_end_event", "Force-end active farm event."),
            Tool("injury_game_ui_close_menu", "Close dialogue box or top clickable menu."),
        };

        private static object Tool(string name, string description, params object[] extraProperties)
        {
            var properties = new Dictionary<string, object>();
            foreach (object prop in extraProperties)
            {
                if (prop is ValueTuple<string, string, string> tuple)
                    properties[tuple.Item1] = new { type = tuple.Item2, description = tuple.Item3 };
            }

            return new
            {
                name,
                description,
                inputSchema = new
                {
                    type = "object",
                    properties,
                },
            };
        }

        private static (string, string, string) Prop(string name, string type, string description) =>
            (name, type, description);
    }
}
