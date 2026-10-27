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
    /// <summary>
    /// Точка входа мода - координирует работу всех менеджеров
    /// </summary>
    public sealed partial class ModEntry : Mod
    {
        // Менеджеры
        private StateManager _stateManager = null!;
        private BuffManager _buffManager = null!;
        private DialogueManager _dialogueManager = null!;
        private InjuryManager _injuryManager = null!;
        private TreatmentManager _treatmentManager = null!;
        private TreatmentStartHandler _treatmentStartHandler = null!;
        private NeglectLadderManager _neglectLadderManager = null!;
        private HiddenInjuryDialogueFlow _hiddenInjuryDialogueFlow = null!;
        private InjuryMedicalIntentProvider _injuryMedicalIntentProvider = null!;
        private HarveyReactionManager _harveyReactionManager = null!;
        private HospitalizationManager _hospitalizationManager = null!;
        private HospitalActivityManager _hospitalActivityManager = null!;
        private ComplicationManager _complicationManager = null!;
        private PrescriptionManager _prescriptionManager = null!;
        private ComplianceManager _complianceManager = null!;
        private CareTrustManager _careTrustManager = null!;
        private CheckupManager _checkupManager = null!;
        private RehabManager _rehabManager = null!;
        private RecoveryPlanManager _recoveryPlanManager = null!;
        private HarveyRecoveryPlanApi _harveyRecoveryPlanApi = null!;
        private MineEntryCoordinator _mineEntryCoordinator = null!;
        private RecoveryPlanMenu _recoveryPlanMenu = null!;
        private InjuryPanelProvider _injuryPanelProvider = null!;
        private InjuryCareDirectiveProvider _injuryCareDirectiveProvider = null!;
        private TreatmentPlanManager _treatmentPlanManager = null!;
        private SelfCareManager _selfCareManager = null!;
        private MedicalItemManager _medicalItemManager = null!;
        private MedicalCardManager _medicalCardManager = null!;
        private MishapManager _mishapManager = null!;
        private DoctorVisitReminderManager _doctorVisitReminderManager = null!;
        private MedicalLetterScheduler _medicalLetterScheduler = null!;

        // Обработчики событий
        private GameEventHandler _gameEventHandler = null!;
        private PlayerEventHandler _playerEventHandler = null!;
        private InteractionHandler _interactionHandler = null!;
        private TimeEventHandler _timeEventHandler = null!;
        private PassOutHandler _passOutHandler = null!;
        private HarveyHomeCareEventLauncher _homeCareEventLauncher = null!;
        private DomesticSpouseCareManager _domesticSpouseCareManager = null!;
        private DomesticHiddenInjuryManager _domesticHiddenInjuryManager = null!;

        // Конфигурация
        private ModConfig _config = null!;

        private InjuryMcpServer? _injuryMcpServer;

        private readonly ConcurrentQueue<(string Tool, JsonElement? Args, TaskCompletionSource<string> Completion)> _mcpCommandQueue = new();

        /// <summary>Режим дебаг-HUD (F10): 0 скрыт, 1 compact, 2 full.</summary>
        private int _debugHudMode = 0;

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
                "Применить дебафф: injury_debuff_add [--force] <id> [минуты]. Основная травма учитывает MainInjury; --force заменяет текущую main.",
                (_, args) => CmdDebuffAdd(args));

            helper.ConsoleCommands.Add(
                "injury_main_clear",
                "[DEBUG] Очистить MainInjuryId без удаления баффов и DebuffState. Только для ремонта состояния.",
                (_, _) => CmdMainClear());

            helper.ConsoleCommands.Add(
                "injury_main_set",
                "[DEBUG] Установить MainInjuryId: injury_main_set <buffId> (нужен DebuffState). Только для ремонта состояния.",
                (_, args) => CmdMainSet(args));

            helper.ConsoleCommands.Add(
                "injury_phase_list",
                "Список активных травм с фазой и флагами готовности к лечению.",
                (_, _) => CmdPhaseList());

            helper.ConsoleCommands.Add(
                "injury_phase_ready",
                "Только фазовые травмы: injury_phase_ready <buffId> [1|0]. Для buffHurt/buffBadlyHurt/buffSurgicalWound — injury_phase_recovery или injury_phase_cure.",
                (_, args) => CmdPhaseReady(args));

            helper.ConsoleCommands.Add(
                "injury_phase_recovery",
                "Готовность к выздоровлению: injury_phase_recovery <buffId> [1|0]. Для простого лечения (buffHurt, buffBadlyHurt, buffSurgicalWound) и последней фазы фазовых травм.",
                (_, args) => CmdPhaseRecovery(args));

            helper.ConsoleCommands.Add(
                "injury_phase_advance",
                "Только фазовые травмы: переключить на следующую фазу. injury_phase_advance <buffId>. Для простого лечения — injury_phase_cure.",
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
                "injury_mine_forbid",
                "[QA] Наложить MineForbidden: injury_mine_forbid [daysLeft]",
                (_, args) => CmdMineForbid(args));

            helper.ConsoleCommands.Add(
                "injury_mine_clear",
                "[QA] Снять MineForbidden и MineRestricted (полный сброс шахты).",
                (_, _) => CmdMineClear());

            helper.ConsoleCommands.Add(
                "injury_mine_forbidden_clear",
                "Снять жёсткий запрет HarveyMod_MineForbidden и связанный state.",
                (_, _) => CmdMineForbiddenClear());

            helper.ConsoleCommands.Add(
                "injury_mine_ban_clear",
                "Снять HarveyMod_MineForbidden, MineWarningDay и MineForbiddenAppliedDay.",
                (_, _) => CmdMineBanClear());

            helper.ConsoleCommands.Add(
                "injury_mine_restriction_clear",
                "Снять мягкое ограничение HarveyMod_MineRestricted и счётчики нарушений.",
                (_, _) => CmdMineRestrictionClear());

            helper.ConsoleCommands.Add(
                "injury_mine_status",
                "Статус шахты: forbidden, restricted, strikes, severe, режим входа.",
                (_, _) => CmdMineStatus());

            helper.ConsoleCommands.Add(
                "injury_mine_forbidden_status",
                "Алиас injury_mine_status.",
                (_, _) => CmdMineStatus());

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
                "injury_mail_dump",
                "[QA] Pending medical letters, mailForTomorrow, MedicalLetters config.",
                (_, _) => CmdMailDump());

            helper.ConsoleCommands.Add(
                "injury_debug_state",
                "[QA] Краткий снимок injury + Harvey click gate + stress interaction guard.",
                (_, _) => CmdInjuryDebugState());

            helper.ConsoleCommands.Add(
                "injury_force_debuff",
                "[QA] Нелеченая травма вчера: injury_force_debuff <buffId> (e.g. buffDeepCuts).",
                (_, args) => CmdInjuryForceDebuff(args));

            helper.ConsoleCommands.Add(
                "injury_clear_treatment",
                "[QA] Сбросить лечение травмы на debuff-only: injury_clear_treatment <buffId>.",
                (_, args) => CmdInjuryClearTreatment(args));

            helper.ConsoleCommands.Add(
                "injury_state_dump",
                "[QA] Машиночитаемый снимок InjuryState (read-only).",
                (_, _) => CmdStateDump());

            helper.ConsoleCommands.Add(
                "injury_buff_dump",
                "[QA] Все баффы игрока с тегами mod/trauma/phase/cure/complication/orphan (read-only).",
                (_, _) => CmdBuffDump());

            helper.ConsoleCommands.Add(
                "injury_topic_dump",
                "[QA] Все conversation topics игрока с днями до истечения (read-only).",
                (_, _) => CmdTopicDump());

            helper.ConsoleCommands.Add(
                "injury_validate_buffs",
                "[QA] Сверка C# buff ID с Data/Buffs; вывод missing ids (read-only).",
                (_, _) => CmdValidateBuffs());

            helper.ConsoleCommands.Add(
                "injury_topic_add",
                "[QA] Добавить owned conversation topic: injury_topic_add <topicId> [days]",
                (_, args) => CmdTopicAdd(args));

            helper.ConsoleCommands.Add(
                "injury_topic_remove",
                "[QA] Снять conversation topic: injury_topic_remove <topicId>",
                (_, args) => CmdTopicRemove(args));

            helper.ConsoleCommands.Add(
                "injury_complication_add",
                "[QA] Добавить осложнение через ComplicationManager: injury_complication_add <id> [ageDays]",
                (_, args) => CmdComplicationAdd(args));

            helper.ConsoleCommands.Add(
                "injury_complication_remove",
                "[QA] Снять осложнение: injury_complication_remove <complicationBuffId>",
                (_, args) => CmdComplicationRemove(args));

            helper.ConsoleCommands.Add(
                "injury_test_age_injury",
                "[QA] Сдвинуть InjuryStartDay/PhaseStartDay: injury_test_age_injury <buffId> <daysBack>",
                (_, args) => CmdTestAgeInjury(args));

            helper.ConsoleCommands.Add(
                "injury_test_age_complication",
                "[QA] Сдвинуть ActiveComplications start day: injury_test_age_complication <compId> <daysBack>",
                (_, args) => CmdTestAgeComplication(args));

            helper.ConsoleCommands.Add(
                "injury_hospital_status",
                "[QA] Read-only снимок госпитализации.",
                (_, _) => CmdHospitalStatus());

            helper.ConsoleCommands.Add(
                "injury_hospital_progress",
                "[QA] Установить прогресс госпитализации: injury_hospital_progress <minutes>",
                (_, args) => CmdHospitalProgress(args));

            helper.ConsoleCommands.Add(
                "injury_hospital_reset",
                "[QA] Полный сброс госпитализации (state + buff).",
                (_, _) => CmdHospitalReset());

            helper.ConsoleCommands.Add(
                "injury_hospital_discharge",
                "[QA] Принудительная выписка (HospitalizationManager.Discharge).",
                (_, _) => CmdHospitalDischarge());

            helper.ConsoleCommands.Add(
                "injury_cleanup",
                "Безопасная очистка зависших медицинских хвостов (лечебные баффы, DoctorVisit, WetBandage) без injury_reset.",
                (_, _) => CmdCleanupLingeringMedical());

            helper.ConsoleCommands.Add(
                "injury_cleanup_invalid_complications",
                "Удалить невалидные/stale осложнения из текущего сейва (WetBandage без лечения, orphan DebuffState/SavedActiveBuffs).",
                (_, _) => CmdCleanupInvalidComplications());

            helper.ConsoleCommands.Add(
                "injury_medical_snapshot",
                "Снимок medical pipeline: decision/gate/pending, DebuffState, фазовые баффы (до/после клика по Харви).",
                (_, _) => CmdMedicalSnapshot());

            helper.ConsoleCommands.Add(
                "injury_harvey_click",
                "DEBUG: мед. действие как после клика по Харви (без диалога). injury_harvey_click [dry] [ignore_hospital] [discharge]",
                (_, args) => CmdHarveyClick(args));

            helper.ConsoleCommands.Add(
                "injury_run_daily_checks",
                "[QA] Утренние проверки мода (infection roll, buff restore) без ожидания DayStarted.",
                (_, _) => CmdRunDailyChecks());

            helper.ConsoleCommands.Add(
                "injury_mine_dirty_simulate",
                "[QA] Симуляция exposure в шахте: injury_mine_dirty_simulate <minutes> [force]",
                (_, args) => CmdMineDirtySimulate(args));

            helper.ConsoleCommands.Add(
                "injury_mine_warning_simulate",
                "[QA] Severe mine warning: injury_mine_warning_simulate [yesterday]",
                (_, args) => CmdMineWarningSimulate(args));

            helper.ConsoleCommands.Add(
                "injury_location_logic",
                "[QA] HandleLocationLogic для текущей локации (госпиталь, шахта, spa).",
                (_, _) => CmdLocationLogic());

            helper.ConsoleCommands.Add(
                "injury_rain_wet_simulate",
                "[QA] WetBandage от дождя без UpdateTick: injury_rain_wet_simulate [force]",
                (_, args) => CmdRainWetSimulate(args));

            helper.ConsoleCommands.Add(
                "injury_hospital_lock_enforce",
                "[QA] Вернуть игрока в палату при госпитализации (обход StardewMCP warp).",
                (_, _) => CmdHospitalLockEnforce());

            helper.ConsoleCommands.Add(
                "injury_main_migrate",
                "[QA] Запустить MigrateMainInjuryId после injury_main_clear.",
                (_, _) => CmdMainMigrate());

            helper.ConsoleCommands.Add(
                "injury_neglect_set",
                "[QA] NeglectStrikesByInjury: injury_neglect_set <buffId> <strikes>",
                (_, args) => CmdNeglectSet(args));

            helper.ConsoleCommands.Add(
                "injury_game_ui_status",
                "[QA] Статус event/dialogue/menu.",
                (_, _) => Monitor.Log(QaGameUiCommands.BuildUiStatusReport(), LogLevel.Info));

            helper.ConsoleCommands.Add(
                "injury_game_ui_advance",
                "[QA] Шаг UI: injury_game_ui_advance [steps]",
                (_, args) => CmdGameUiAdvance(args));

            helper.ConsoleCommands.Add(
                "injury_game_ui_end_event",
                "[QA] Принудительно завершить активный cutscene event.",
                (_, _) => Monitor.Log(QaGameUiCommands.EndActiveEvent(), LogLevel.Info));

            helper.ConsoleCommands.Add(
                "injury_game_ui_close_menu",
                "[QA] Закрыть dialogue box или верхнее меню.",
                (_, _) => Monitor.Log(QaGameUiCommands.CloseActiveMenu(), LogLevel.Info));

            helper.ConsoleCommands.Add(
                "injury_foreign_topic_add",
                "Тест конфликта модов: добавить чужой conversation topic. injury_foreign_topic_add <topicId> [days]",
                (_, args) => CmdForeignTopicAdd(args));

            helper.ConsoleCommands.Add(
                "injury_proximity_test",
                "Отладка proximity-реплик из CP без изменения state. injury_proximity_test <situation> [tone]",
                (_, args) => CmdProximityTest(args));

            helper.ConsoleCommands.Add(
                "injury_prescription_list",
                "Список активных предписаний и TreatmentComplianceScore (соблюдение лечения, не Friendship).",
                (_, _) => CmdPrescriptionList());

            helper.ConsoleCommands.Add(
                "injury_prescription_add",
                "Добавить предписание: injury_prescription_add <id> <injuryId> [days]",
                (_, args) => CmdPrescriptionAdd(args));

            helper.ConsoleCommands.Add(
                "injury_prescription_clear",
                "Снять все активные предписания и сбросить TreatmentComplianceScore.",
                (_, _) => CmdPrescriptionClear());

            helper.ConsoleCommands.Add(
                "injury_compliance_set",
                "TreatmentComplianceScore (−10…10): насколько стабильно соблюдается лечение. Не влияет на Friendship.",
                (_, args) => CmdComplianceSet(args));

            helper.ConsoleCommands.Add(
                "injury_trust_get",
                "Текущее значение CareTrust (скрытое медицинское доверие Харви).",
                (_, _) => CmdTrustGet());

            helper.ConsoleCommands.Add(
                "injury_trust_set",
                "Установить CareTrust: injury_trust_set <value>",
                (_, args) => CmdTrustSet(args));

            helper.ConsoleCommands.Add(
                "injury_trust_add",
                "Изменить CareTrust: injury_trust_add <value> (отрицательное — штраф).",
                (_, args) => CmdTrustAdd(args));

            helper.ConsoleCommands.Add(
                "injury_trust_level",
                "Уровень CareTrust: Low / Medium / High.",
                (_, _) => CmdTrustLevel());

            helper.ConsoleCommands.Add(
                "injury_test_prescription_violation",
                "Тест нарушения предписания: Friendship не падает, topic violation + compliance −1. [NoMine|KeepDry]",
                (_, args) => CmdTestPrescriptionViolation(args));

            helper.ConsoleCommands.Add(
                "injury_checkup_due",
                "Тест контрольного осмотра: injury_checkup_due <buffId> — ReadyForNextPhase/Recovery + topics.",
                (_, args) => CmdCheckupDue(args));

            helper.ConsoleCommands.Add(
                "injury_rehab_start",
                "Старт реабилитации: injury_rehab_start <buffId> [days]",
                (_, args) => CmdRehabStart(args));

            helper.ConsoleCommands.Add(
                "injury_rehab_status",
                "Статус активной реабилитации.",
                (_, _) => CmdRehabStatus());

            helper.ConsoleCommands.Add(
                "injury_rehab_clear",
                "Снять реабилитацию и связанные topics/buff.",
                (_, _) => CmdRehabClear());

            helper.ConsoleCommands.Add(
                "recovery_plan_start",
                "[QA] Старт плана восстановления после выписки: recovery_plan_start [injuryId]",
                (_, args) => CmdRecoveryPlanStart(args));

            helper.ConsoleCommands.Add(
                "recovery_plan_fail",
                "[QA] Зарегистрировать нарушение плана: recovery_plan_fail <reason>",
                (_, args) => CmdRecoveryPlanFail(args));

            helper.ConsoleCommands.Add(
                "recovery_plan_status",
                "[QA] Статус плана восстановления.",
                (_, _) => CmdRecoveryPlanStatus());

            helper.ConsoleCommands.Add(
                "domestic_spouse_status",
                "[QA] Контекст Domestic Spouse Care: локация, погода, prefix, cooldown.",
                (_, _) => Monitor.Log(_domesticSpouseCareManager.BuildDebugStatusReport(), LogLevel.Info));

            helper.ConsoleCommands.Add(
                "domestic_spouse_test",
                "[QA] Показать реплику по prefix без изменения state: domestic_spouse_test <prefix>",
                (_, args) =>
                {
                    if (args.Length < 1)
                    {
                        Monitor.Log("Usage: domestic_spouse_test <prefix>", LogLevel.Error);
                        return;
                    }

                    _domesticSpouseCareManager.TestPrefix(string.Join(" ", args));
                });

            helper.ConsoleCommands.Add(
                "domestic_spouse_reset",
                "[QA] Сбросить DomesticSpouseState.",
                (_, _) => _domesticSpouseCareManager.ResetState());

            helper.ConsoleCommands.Add(
                "recovery_plan_clear",
                "[QA] Снять hospital-план восстановления.",
                (_, _) => CmdRecoveryPlanClear());

            helper.ConsoleCommands.Add(
                "recovery_violation_test",
                "[QA] Тест severity-нарушения: recovery_violation_test <type> <severity 1|2|3>",
                (_, args) => CmdRecoveryViolationTest(args));

            helper.ConsoleCommands.Add(
                "recovery_violation_status",
                "[QA] Статус severity-нарушений режима восстановления.",
                (_, _) => CmdRecoveryViolationStatus());

            helper.ConsoleCommands.Add(
                "recovery_violation_clear",
                "[QA] Сброс полей нарушения: recovery_violation_clear [all]",
                (_, args) => CmdRecoveryViolationClear(args));

            helper.ConsoleCommands.Add(
                "injury_plan_show",
                "Текущий RecoveryPlanState (daily plan).",
                (_, _) => CmdInjuryPlanShow());

            helper.ConsoleCommands.Add(
                "injury_plan_refresh",
                "Пересобрать план восстановления на сегодня.",
                (_, _) => CmdInjuryPlanRefresh());

            helper.ConsoleCommands.Add(
                "injury_plan_violate",
                "Тестовое нарушение: mines|stamina|health|sleep|rain|missed_checkup",
                (_, args) => CmdInjuryPlanViolate(args));

            helper.ConsoleCommands.Add(
                "injury_plan_clear",
                "Очистить daily RecoveryPlan и нарушения.",
                (_, _) => CmdInjuryPlanClear());

            helper.ConsoleCommands.Add(
                "injury_panel_dump",
                "Что Injury-провайдер отдаёт во вкладку «План» Harvey Panel (H).",
                (_, _) => CmdInjuryPanelDump());

            RegisterHarveyPlanConsoleCommands(helper);

            helper.ConsoleCommands.Add(
                "recovery_violate",
                "[QA] Типизированное нарушение: recovery_violate mine|stamina|health|night|rain",
                (_, args) => CmdRecoveryViolate(args));

            helper.ConsoleCommands.Add(
                "recovery_complete",
                "[QA] Completion topic: recovery_complete perfect|warnings|violated",
                (_, args) => CmdRecoveryComplete(args));

            helper.ConsoleCommands.Add(
                "injury_recovery_status",
                "Статус RecoveryPlan: дни, нарушения, perfect flag, награда.",
                (_, _) => CmdInjuryRecoveryStatus());

            helper.ConsoleCommands.Add(
                "injury_recovery_complete_debug",
                "[QA] Завершить план: injury_recovery_complete_debug perfect|warnings|violated",
                (_, args) => CmdInjuryRecoveryCompleteDebug(args));

            helper.ConsoleCommands.Add(
                "injury_recovery_reset",
                "Сбросить только RecoveryPlan (травмы не трогать).",
                (_, _) => CmdInjuryRecoveryReset());

            helper.ConsoleCommands.Add(
                "injury_recovery_violate",
                "[QA] Нарушение с тяжестью: injury_recovery_violate <type> <severity>",
                (_, args) => CmdInjuryRecoveryViolate(args));

            helper.ConsoleCommands.Add(
                "recovery_debug",
                "[QA] Debug Recovery Plan: типы, severity, completion topic.",
                (_, _) => CmdRecoveryDebug());

            helper.ConsoleCommands.Add(
                "injury_selfcare_bandage",
                "Самопомощь: сменить повязку дома (force без проверки локации).",
                (_, _) => CmdSelfCareBandage());

            helper.ConsoleCommands.Add(
                "injury_selfcare_tea",
                "Самопомощь: тёплый чай при простуде.",
                (_, _) => CmdSelfCareTea());

            helper.ConsoleCommands.Add(
                "injury_selfcare_rest",
                "Самопомощь: ранний отдых (force).",
                (_, _) => CmdSelfCareRest());

            if (_config.EnableInjuryMcp)
            {
                helper.Events.GameLoop.UpdateTicked += OnProcessMcpCommandQueue;
                _injuryMcpServer = new InjuryMcpServer(Monitor, ExecuteMcpTool);
                _injuryMcpServer.Start(_config.InjuryMcpPort);
            }

            Monitor.Log("Harvey Overhaul: Injury & Care загружен", LogLevel.Info);
        }

        public override object GetApi()
            => _harveyRecoveryPlanApi;

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

            foreach (string compId in InjurySets.KnownComplicationBuffIds)
                _buffManager.RemoveBuff(compId);

            // MineForbidden — не в KnownComplicationBuffIds, но может остаться orphan на игроке
            _buffManager.RemoveBuff(InjuryBuffs.MineForbidden);
            _buffManager.RemoveBuff(StatusBuffs.Hospitalized);

            // Удалить все лечебные баффы
            foreach (var cureBuff in new[]
            {
                Core.CureBuffs.Treatment, Core.CureBuffs.IntensiveCare, Core.CureBuffs.BadlyHurtOutpatientCare,
                Core.CureBuffs.Protection, Core.CureBuffs.Recovery, Core.CureBuffs.Teracitin,
                Core.CureBuffs.Antibiotics, Core.CureBuffs.ForcedSedation,                 Core.CureBuffs.PostSurgical,
                Core.CureBuffs.Care,
                Core.CureBuffs.Rehab,
                SelfCareBuffs.SelfCare,
                SelfCareBuffs.CleanBandage,
                SelfCareBuffs.WarmTea,
            })
            {
                _buffManager.RemoveBuff(cureBuff);
            }

            _prescriptionManager.ClearAllPrescriptions();
            _rehabManager.ClearRehab();
            foreach (var prescriptionId in new[]
            {
                PrescriptionIds.Rest,
                PrescriptionIds.NoMine,
                PrescriptionIds.KeepDry,
                PrescriptionIds.LightWork,
                PrescriptionIds.Checkup,
            })
            {
                _buffManager.RemoveBuff(prescriptionId);
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
            _careTrustManager = new CareTrustManager(Monitor, _config, _stateManager, _dialogueManager);

            // Загрузить данные баффов
            _buffManager.LoadBuffData();
            
            // HospitalizationManager (будет обновлён позже с TreatmentManager)
            _hospitalizationManager = new HospitalizationManager(Monitor, _config, _dialogueManager, _stateManager, _buffManager);
            
            // HospitalActivityManager - интерактивная госпитализация
            _hospitalActivityManager = new HospitalActivityManager(Monitor, _config, _dialogueManager, _stateManager);
            
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
            _complianceManager = new ComplianceManager(Monitor, _stateManager, _dialogueManager, _buffManager);

            _doctorVisitReminderManager = new DoctorVisitReminderManager(
                Monitor,
                _buffManager,
                _stateManager,
                _hospitalizationManager);
            _hospitalizationManager.SetDoctorVisitReminderManager(_doctorVisitReminderManager);

            _medicalLetterScheduler = new MedicalLetterScheduler(
                _config,
                _stateManager,
                _buffManager,
                Monitor,
                _doctorVisitReminderManager);

            _checkupManager = new CheckupManager(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _complianceManager,
                _medicalLetterScheduler);
            _prescriptionManager = new PrescriptionManager(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _buffManager,
                _complianceManager,
                _medicalLetterScheduler);
            _rehabManager = new RehabManager(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _buffManager,
                _complianceManager,
                _medicalLetterScheduler);
            _recoveryPlanManager = new RecoveryPlanManager(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _dialogueManager);
            _harveyRecoveryPlanApi = new HarveyRecoveryPlanApi(_recoveryPlanManager);
            _mineEntryCoordinator = new MineEntryCoordinator(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _recoveryPlanManager,
                _careTrustManager,
                _complianceManager);
            _recoveryPlanMenu = new RecoveryPlanMenu(Monitor);
            _treatmentPlanManager = new TreatmentPlanManager(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _medicalLetterScheduler);
            _selfCareManager = new SelfCareManager(
                Monitor,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _complianceManager,
                _prescriptionManager);
            _treatmentManager = new TreatmentManager(
                Monitor,
                _config,
                _buffManager,
                _injuryManager,
                _dialogueManager,
                _stateManager,
                _prescriptionManager,
                _complianceManager,
                _checkupManager,
                _treatmentPlanManager);

            _medicalCardManager = new MedicalCardManager(
                Monitor,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _injuryManager);
            _treatmentManager.InjuryRecovered += _medicalCardManager.OnInjuryRecovered;
            _injuryManager.MainInjuryApplied += _medicalCardManager.OnMainInjuryApplied;
            _injuryManager.InjuryEpisodeStarted += _complianceManager.StartNewEpisode;
            _mishapManager = new MishapManager(Monitor, _config, _stateManager, _buffManager, _dialogueManager);

            _injuryPanelProvider = new InjuryPanelProvider(
                _stateManager,
                _injuryManager,
                _recoveryPlanManager,
                _careTrustManager,
                _medicalCardManager);
            _injuryCareDirectiveProvider = new InjuryCareDirectiveProvider(
                _config,
                _stateManager,
                _injuryManager,
                _recoveryPlanManager);
            _hospitalizationManager.SetTreatmentManager(_treatmentManager);
            _hospitalizationManager.SetRecoveryPlanManager(_recoveryPlanManager);

            _harveyReactionManager = new HarveyReactionManager(
                Monitor,
                _stateManager,
                _buffManager,
                _injuryManager,
                _complianceManager,
                _prescriptionManager,
                _dialogueManager,
                _rehabManager);

            _domesticSpouseCareManager = new DomesticSpouseCareManager(
                Monitor,
                Helper,
                _config,
                _stateManager,
                _dialogueManager,
                _injuryManager,
                _hospitalizationManager);

            // ComplicationManager - управление осложнениями
            _complicationManager = new ComplicationManager(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _injuryManager,
                _complianceManager,
                _selfCareManager,
                _medicalLetterScheduler);
            _injuryManager.SetComplicationManager(_complicationManager);
            _treatmentManager.SetComplicationManager(_complicationManager);
            _treatmentManager.SetDoctorVisitReminderManager(_doctorVisitReminderManager);
            _doctorVisitReminderManager.SetComplicationManager(_complicationManager);
            _doctorVisitReminderManager.SetRecoveryPlanManager(_recoveryPlanManager);
            _complicationManager.ComplicationsChanged += _doctorVisitReminderManager.SyncReminderBuff;
            _selfCareManager.ComplicationsChanged += _doctorVisitReminderManager.SyncReminderBuff;
            _medicalItemManager = new MedicalItemManager(
                Monitor,
                Helper.Input,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _complianceManager,
                _complicationManager,
                _selfCareManager,
                _treatmentManager,
                _mishapManager,
                _medicalCardManager);

            _treatmentStartHandler = new TreatmentStartHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _dialogueManager,
                _treatmentManager,
                _hospitalizationManager,
                _complicationManager,
                _complianceManager,
                _doctorVisitReminderManager,
                _recoveryPlanManager,
                _checkupManager,
                _rehabManager,
                _selfCareManager);

            _treatmentStartHandler.SetMedicalLetterScheduler(_medicalLetterScheduler);

            _neglectLadderManager = new NeglectLadderManager(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _complicationManager,
                _hospitalizationManager,
                _treatmentStartHandler,
                _careTrustManager,
                _medicalLetterScheduler);

            _injuryMedicalIntentProvider = new InjuryMedicalIntentProvider(
                Monitor,
                _config,
                _stateManager,
                _injuryManager,
                _dialogueManager,
                _complicationManager,
                _hospitalizationManager);
            _injuryMedicalIntentProvider.SetRecoveryPlanManager(_recoveryPlanManager);

            _hiddenInjuryDialogueFlow = new HiddenInjuryDialogueFlow(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _treatmentStartHandler,
                _injuryManager,
                _buffManager,
                _complicationManager);

            _domesticHiddenInjuryManager = new DomesticHiddenInjuryManager(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _hiddenInjuryDialogueFlow);

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
                _complicationManager,
                _prescriptionManager,
                _complianceManager,
                _careTrustManager,
                _checkupManager,
                _rehabManager,
                _recoveryPlanManager,
                _selfCareManager,
                _doctorVisitReminderManager,
                _medicalLetterScheduler
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
                _harveyReactionManager,
                _prescriptionManager,
                _complianceManager,
                _careTrustManager,
                _rehabManager,
                _recoveryPlanManager,
                _complicationManager,
                _mineEntryCoordinator,
                _hiddenInjuryDialogueFlow
            );

            _interactionHandler = new InteractionHandler(
                Monitor,
                Helper,
                _config,
                _stateManager,
                _buffManager,
                _injuryManager,
                _dialogueManager,
                _treatmentManager,
                _hospitalizationManager,
                _complianceManager,
                _careTrustManager,
                _prescriptionManager,
                _checkupManager,
                _rehabManager,
                _selfCareManager,
                _complicationManager,
                _doctorVisitReminderManager,
                _recoveryPlanManager,
                _treatmentStartHandler,
                _injuryMedicalIntentProvider,
                _hiddenInjuryDialogueFlow);
            _interactionHandler.SetHospitalActivityManager(_hospitalActivityManager);

            _timeEventHandler = new TimeEventHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _hospitalizationManager,
                _hospitalActivityManager,
                _treatmentManager,
                _injuryManager,
                _complicationManager,
                _recoveryPlanManager);

            _passOutHandler = new PassOutHandler(
                Monitor,
                _config,
                _stateManager,
                _buffManager,
                _dialogueManager,
                _injuryManager,
                _treatmentManager
            );
            _passOutHandler.SetRecoveryPlanManager(_recoveryPlanManager);
            _passOutHandler.SetTreatmentStartHandler(_treatmentStartHandler);
            _passOutHandler.SetMedicalLetterScheduler(_medicalLetterScheduler);
            _passOutHandler.SetHospitalizationManager(_hospitalizationManager);
            _passOutHandler.RegisterQaCommands(Helper.ConsoleCommands);

            _homeCareEventLauncher = new HarveyHomeCareEventLauncher(
                Monitor,
                _config,
                _stateManager,
                _dialogueManager,
                _hospitalizationManager,
                _injuryManager,
                _complicationManager,
                _careTrustManager
            );

            // События сохранения
            events.GameLoop.GameLaunched += OnGameLaunched;
            events.GameLoop.SaveLoaded += OnSaveLoaded;
            events.GameLoop.Saving += OnSaving;

            // События дня
            events.GameLoop.DayStarted += _gameEventHandler.OnDayStarted;
            events.GameLoop.DayEnding += _gameEventHandler.OnDayEnding;

            // Связываем обработчики
            _gameEventHandler.SetInteractionHandler(_interactionHandler);
            _gameEventHandler.SetPassOutHandler(_passOutHandler);
            _gameEventHandler.SetHomeCareLauncher(_homeCareEventLauncher);
            _playerEventHandler.SetPassOutHandler(_passOutHandler);
            _playerEventHandler.SetHomeCareLauncher(_homeCareEventLauncher);
            _timeEventHandler.SetHomeCareLauncher(_homeCareEventLauncher);

            // Лестница последствий: утренняя оценка — из GameEventHandler (после осложнений),
            // визит — при входе на ферму (в браке — дома), обморок — по игровому времени.
            _gameEventHandler.SetNeglectLadderManager(_neglectLadderManager);
            events.Player.Warped += (_, e) =>
            {
                if (e.IsLocalPlayer)
                    _neglectLadderManager.TryLaunchHouseCall(e.NewLocation, "Warp");
            };
            events.GameLoop.UpdateTicked += (_, e) =>
            {
                // Раз в 5 секунд: игрок уже на месте, а визит не стартовал (меню, диалог, некуда поставить Харви).
                if (Context.IsWorldReady && e.IsMultipleOf(300))
                    _neglectLadderManager.TryLaunchHouseCall(null, "UpdateRetry");
            };
            events.GameLoop.TimeChanged += (_, _) =>
            {
                if (Context.IsWorldReady)
                    _neglectLadderManager.OnTimeChanged();
            };

            events.GameLoop.DayStarted += _domesticSpouseCareManager.OnDayStarted;
            events.GameLoop.TimeChanged += _domesticSpouseCareManager.OnTimeChanged;
            events.GameLoop.DayStarted += _domesticHiddenInjuryManager.OnDayStarted;
            events.GameLoop.TimeChanged += _domesticHiddenInjuryManager.OnTimeChanged;
            events.GameLoop.UpdateTicked += _domesticSpouseCareManager.OnUpdateTicked;

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
            events.Input.ButtonPressed += _medicalItemManager.OnButtonPressed;

            // Рубцы на холоде (TraumaBackstory): проверка при выходе на улицу и раз в 10 минут.
            var scarCareManager = new ScarCareManager(Monitor, _config, _dialogueManager);
            _medicalItemManager.SetScarCareManager(scarCareManager);
            events.GameLoop.DayStarted += (_, _) => scarCareManager.OnDayStarted();
            events.Player.Warped += (_, e) => { if (e.IsLocalPlayer) scarCareManager.Check(); };
            events.GameLoop.TimeChanged += (_, _) => scarCareManager.Check();

            // Харви тоже устаёт и болеет: забота в обе стороны.
            var wellbeingManager = new HarveyWellbeingManager(Monitor, _careTrustManager, _dialogueManager, _stateManager);
            _injuryPanelProvider.SetWellbeingManager(wellbeingManager);
            events.GameLoop.DayStarted += (_, _) => wellbeingManager.OnDayStarted();
            events.Input.ButtonPressed += (_, e) => wellbeingManager.OnButtonPressed(e);
            events.GameLoop.UpdateTicked += (_, _) => wellbeingManager.OnUpdateTicked();

            // Жена на больничном — Харви до работы поливает грядки, кормит животных и питомца.
            var spouseChoresManager = new SpouseChoresManager(Monitor, _config, _stateManager, _injuryManager, _dialogueManager);
            events.GameLoop.DayStarted += (_, _) => spouseChoresManager.OnDayStarted();

            // Сумка Харви (доверие «Опора»): раз в день спасает при четверти здоровья.
            var harveyBagManager = new HarveyBagManager(Monitor);
            events.GameLoop.UpdateTicked += (_, e) => harveyBagManager.OnUpdateTicked(e.Ticks);

            // Записка Харви перед шахтой: выбор при первом спуске за день.
            var mineNoteManager = new MineNoteManager(Monitor, Helper, _config, _careTrustManager, _dialogueManager,
                () => _mineEntryCoordinator.ShouldPhysicallyBlockMines());
            events.Player.Warped += (_, e) => mineNoteManager.OnWarped(e);
            events.GameLoop.UpdateTicked += _medicalItemManager.OnUpdateTicked;
            events.GameLoop.UpdateTicked += _mishapManager.OnUpdateTicked;
            var topicRelevanceGuard = new TopicRelevanceGuard(Monitor, _stateManager, _buffManager, _dialogueManager);
            events.GameLoop.SaveLoaded += (_, _) => topicRelevanceGuard.OnSaveLoaded();
            events.GameLoop.DayStarted += (_, _) => topicRelevanceGuard.OnDayStarted();
            events.GameLoop.ReturnedToTitle += (_, _) => topicRelevanceGuard.OnReturnedToTitle();
            events.GameLoop.UpdateTicked += topicRelevanceGuard.OnUpdateTicked;
            var medicalTopicPacer = new MedicalTopicPacer(Monitor, _stateManager);
            events.GameLoop.DayStarted += (_, _) => medicalTopicPacer.OnDayStarted();
            events.GameLoop.ReturnedToTitle += (_, _) => medicalTopicPacer.OnReturnedToTitle();
            events.GameLoop.UpdateTicked += medicalTopicPacer.OnUpdateTicked;
            events.GameLoop.TimeChanged += _mishapManager.OnTimeChanged;
            var rainHouseCallManager = new RainHouseCallManager(Monitor, _stateManager, _dialogueManager, _hospitalizationManager);
            events.GameLoop.OneSecondUpdateTicked += rainHouseCallManager.OnOneSecondUpdateTicked;
            events.GameLoop.DayStarted += (_, _) =>
            {
                _mishapManager.OnDayStarted();
                _medicalCardManager.OnDayStarted();
                rainHouseCallManager.OnDayStarted();
            };
            events.GameLoop.UpdateTicked += _interactionHandler.OnUpdateTicked;
            events.GameLoop.UpdateTicked += _injuryMedicalIntentProvider.OnUpdateTicked;
            events.GameLoop.UpdateTicked += _hiddenInjuryDialogueFlow.OnUpdateTicked;

            // События времени
            events.GameLoop.TimeChanged += _timeEventHandler.OnTimeChanged;

            // Дебаг: вывод _state на экран (переключение по F10)
            events.Display.RenderedHud += OnRenderedHudDebugState;
            events.Input.ButtonPressed += OnDebugHudToggleKeyPressed;

            if (_config.EnableStandaloneRecoveryPlanWindow)
            {
                events.Input.ButtonPressed += OnRecoveryPlanKeyPressed;
                Monitor.Log(
                    "[HarveyOverhaulInjury] Standalone RecoveryPlan UI enabled (StandaloneRecoveryPlanKey).",
                    LogLevel.Info);
            }
            else
            {
                Monitor.Log(
                    "[HarveyOverhaulInjury] Standalone RecoveryPlan UI disabled. Shared Harvey panel is opened by HarveyOverhaul.Core (H).",
                    LogLevel.Info);
            }

            // Без цветного свечения персонажа от модовых баффов/дебаффов
            events.Content.AssetRequested += OnAssetRequested;
            events.Content.AssetsInvalidated += OnAssetsInvalidated;

            Monitor.Log("Подписка на события завершена", LogLevel.Debug);
        }

        /// <summary>CP пересобрал ассеты (сменилась стадия, сезон, беременность…) — сбросить свои кэши.</summary>
        private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            var names = e.NamesWithoutLocale.Select(n => n.BaseName).ToList();
            _dialogueManager.InvalidateCaches(names);
            if (names.Any(n => n.Equals("Data/Mail", StringComparison.OrdinalIgnoreCase)))
                HarveyMailHelper.InvalidateMailCache();
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

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (_config.EnableStandaloneRecoveryPlanWindow)
                _recoveryPlanMenu.TryInitialize(Helper);

            RegisterCoreProviders("GameLaunched");
            GenericModConfigMenuIntegration.Register(Helper, ModManifest, _config);

            _treatmentStartHandler.RegisterTriggerActions();
            _neglectLadderManager.RegisterTriggerActions();
            _mishapManager.RegisterTriggerActions();
            _recoveryPlanManager.RegisterTriggerActions();
        }

        private void RegisterCoreProviders(string phase)
        {
            var coreApi = Helper.ModRegistry.GetApi<IHarveyCoreApi>("marilynsinister.HarveyOverhaul.Core");
            if (coreApi == null)
            {
                Monitor.Log(
                    $"[HarveyOverhaulInjury] ({phase}) HarveyOverhaul.Core API not found — panel/care providers not registered.",
                    LogLevel.Error);
                return;
            }

            coreApi.RegisterPanelProvider(_injuryPanelProvider);
            coreApi.RegisterCareDirectiveProvider(_injuryCareDirectiveProvider);
            coreApi.RegisterRecoveryPlanApi(_harveyRecoveryPlanApi);
            _domesticSpouseCareManager.SetCoreApi(coreApi);
            _timeEventHandler.SetCoreApi(coreApi);
            _treatmentStartHandler.SetCoreApi(coreApi);
            _neglectLadderManager.SetCoreApi(coreApi);
            _injuryMedicalIntentProvider.SetCoreApi(coreApi);
            _careTrustManager.SetCoreApi(coreApi);
            _config.SetCoreDifficulty(() => coreApi.DifficultyOverride);

            int directiveCount = _injuryCareDirectiveProvider.GetCareDirectives().Count;
            Monitor.Log(
                $"[HarveyOverhaulInjury] ({phase}) Injury providers registered with Core. CareDirectives={directiveCount}",
                LogLevel.Info);
        }

        private void OnRecoveryPlanKeyPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!_config.EnableStandaloneRecoveryPlanWindow)
                return;

            if (!Context.IsWorldReady)
                return;

            if (!MatchesStandaloneRecoveryPlanKey(e.Button))
                return;

            Helper.Input.Suppress(e.Button);

            // «План Харви» — единое окно Core; старое окно Injury только если Core недоступен.
            var coreApi = Helper.ModRegistry.GetApi<IHarveyCoreApi>("marilynsinister.HarveyOverhaul.Core");
            if (coreApi != null)
            {
                coreApi.OpenPanel(HarveyOverhaul.Core.Models.HarveyPanelTab.Plan);
                return;
            }

            Monitor.Log(
                "[HarveyPlan/Injury] Core API unavailable — opening legacy RecoveryPlan window.",
                LogLevel.Warn);
            _recoveryPlanMenu.TryOpen(_recoveryPlanManager);
        }

        private bool MatchesStandaloneRecoveryPlanKey(SButton button)
        {
            string configured = _config.StandaloneRecoveryPlanKey?.Trim() ?? "";
            if (string.IsNullOrEmpty(configured))
                return false;

            return Enum.TryParse(configured, true, out SButton parsed) && button == parsed;
        }

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
            // Другое сохранение — другая стадия отношений: кэши прошлой игры не годятся.
            _dialogueManager.InvalidateCaches();
            HarveyMailHelper.InvalidateMailCache();
            _stateManager.Load();
            _domesticSpouseCareManager.Load();
            
            // Перезагрузить данные баффов после того, как Content Patcher загрузил все патчи
            _buffManager.LoadBuffData();

            // Resume mine rescue, если reload случился между warp и startEvent
            Game1.delayedActions.Add(new StardewValley.DelayedAction(100, () =>
            {
                try
                {
                    ClearModBuffGlowIfNeeded();

                    int restored = _injuryManager.EnsureActiveTreatmentBuffs();
                    if (restored > 0)
                    {
                        Monitor.Log(
                            $"[BuffSync] После загрузки сейва восстановлено {restored} лечебных бафф(ов)",
                            LogLevel.Info);
                    }

                    _passOutHandler.ResumePendingMineRescueIfNeeded();
                    _passOutHandler.ResumePendingHospitalPassOutIfNeeded();
                    _passOutHandler.ResumePendingMinorMineRescueIfNeeded();

                    _hospitalizationManager.SanitizeOrphanHospitalizedBuff();
                    _hospitalizationManager.RestoreHospitalizedBuffIfActive();
                    _recoveryPlanManager.SyncHarveyTalkTopic();
                }
                catch (Exception ex)
                {
                    Monitor.Log($"[PassOut] Ошибка resume pending cutscenes после загрузки: {ex}", LogLevel.Error);
                }
            }));
            
            RegisterCoreProviders("SaveLoaded");
            Monitor.Log("Состояние загружено из сохранения", LogLevel.Info);
        }

        /// <summary>
        /// Убрать свечение, если на игроке висят модовые баффы (в т.ч. после загрузки старого сейва).
        /// </summary>
        private void ClearModBuffGlowIfNeeded()
        {
            var player = Game1.player;
            var applied = player?.buffs?.AppliedBuffs;
            if (player == null || applied == null)
                return;

            foreach (var buffId in applied.Keys)
            {
                if (!ModBuffGlow.IsModBuff(buffId))
                    continue;

                player.stopGlowing();
                return;
            }
        }

        /// <summary>
        /// Сохранение игры
        /// </summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            _stateManager.Save();
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

