using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Логика «Плана восстановления Харви» (без UI).
    /// </summary>
    public class RecoveryPlanManager
    {
        public const string HospitalDischargePlanId = "RecoveryPlan_HospitalDischarge";
        public const string HospitalDischargeReason = "hospital";
        public const int HospitalDischargeRequiredDays = 3;

        private const string HudStart = "Харви назначил план восстановления на 3 дня.";
        private const string HudDayFailed = "План восстановления: режим сорван.";
        private const string HudDayCredited = "План восстановления: день зачтён.";
        private const string HudCompleted = "План восстановления завершён. Поговори с Харви.";

        private const int CriticalHealthThreshold = 10;
        private const float CriticalStaminaFraction = 0.15f;
        private const int ViolatedTopicDays = 2;
        private const int CompletedTopicDays = 3;

        private const int CompletionTalkDialogueTimeoutTicks = 1800;

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly DialogueManager _dialogueManager;

        private bool _awaitingCompletionTalkAck;
        private bool _completionTalkSawDialogue;
        private int _completionTalkStartedTick = -1;

        public RecoveryPlanManager(
            IMonitor monitor,
            StateManager stateManager,
            DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _dialogueManager = dialogueManager;
        }

        /// <summary>
        /// Старт плана после выписки из госпитализации. Не перезаписывает уже активный план.
        /// </summary>
        public bool StartHospitalDischargePlan(string? injuryId = null)
        {
            if (HasActivePlan())
            {
                _monitor.Log(
                    "[RecoveryPlan] Пропуск старта: уже активен план "
                    + $"{GetActivePlan()?.PlanId}",
                    LogLevel.Debug);
                return false;
            }

            string? resolvedInjuryId = ResolveInjuryId(injuryId);
            int today = GameUtils.Today();

            var plan = new RecoveryPlanState
            {
                IsActive = true,
                PlanId = HospitalDischargePlanId,
                Reason = HospitalDischargeReason,
                InjuryId = resolvedInjuryId,
                StartDay = today,
                RequiredDays = HospitalDischargeRequiredDays,
                CompletedDays = 0,
                TodayFailed = false,
                TodayCompleted = false,
                ViolationsToday = 0,
                TotalViolations = 0,
                TodayViolationReasons = new List<string>(),
                RequiresHarveyTalk = false,
                CompletionTalkPending = false,
                LastEvaluatedDay = -1,
            };

            _stateManager.SetActiveRecoveryPlan(plan);
            _dialogueManager.AddTopic(ConversationTopics.RecoveryPlanStarted, plan.RequiredDays);
            Game1.addHUDMessage(new HUDMessage(HudStart, HUDMessage.health_type));
            _monitor.Log(
                $"[RecoveryPlan] Старт {plan.PlanId}: injury={resolvedInjuryId ?? "(none)"}, "
                + $"required={plan.RequiredDays}d, day={today}",
                LogLevel.Info);
            return true;
        }

        public bool HasActivePlan()
        {
            return GetActivePlan() is { IsActive: true };
        }

        public RecoveryPlanState? GetActivePlan() => _stateManager.GetActiveRecoveryPlan();

        /// <returns>true, если нарушение зарегистрировано (новая причина за сегодня).</returns>
        public bool RegisterViolation(string reason, bool serious = true)
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.IsActive)
                return false;

            string normalizedReason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason.Trim();

            if (ContainsReason(plan.TodayViolationReasons, normalizedReason))
                return false;

            bool firstFailureToday = !plan.TodayFailed;

            plan.TodayFailed = true;
            plan.ViolationsToday++;
            plan.TotalViolations++;
            plan.TodayViolationReasons.Add(normalizedReason);

            _stateManager.Save();

            if (firstFailureToday)
            {
                _dialogueManager.AddTopic(ConversationTopics.RecoveryPlanViolated, ViolatedTopicDays);
                Game1.addHUDMessage(new HUDMessage(HudDayFailed, HUDMessage.error_type));
            }

            _monitor.Log(
                $"[RecoveryPlan] Нарушение ({normalizedReason}, serious={serious}): "
                + $"today={plan.ViolationsToday}, total={plan.TotalViolations}",
                LogLevel.Info);
            return true;
        }

        /// <summary>Вход в шахту или Skull Cave во время активного плана.</summary>
        public void CheckViolationOnLocationEntry(GameLocation? location)
        {
            if (!HasActivePlan() || location == null)
                return;

            if (TryResolveMineViolationReason(location, out string? reason))
                RegisterViolation(reason);
        }

        /// <summary>Критическое здоровье (throttled в PlayerEventHandler).</summary>
        public void CheckViolationOnLowHealth()
        {
            if (!HasActivePlan())
                return;

            if (Game1.player.health > CriticalHealthThreshold)
                return;

            RegisterViolation("low_health");
        }

        /// <summary>Критическая энергия (throttled в PlayerEventHandler).</summary>
        public void CheckViolationOnLowStamina()
        {
            if (!HasActivePlan())
                return;

            float maxStamina = Math.Max(1f, Game1.player.MaxStamina);
            if (Game1.player.Stamina > maxStamina * CriticalStaminaFraction)
                return;

            RegisterViolation("low_stamina");
        }

        /// <summary>Обморок перед сном.</summary>
        public void CheckViolationOnPassOut()
        {
            if (!HasActivePlan())
                return;

            if (!_stateManager.State.WasPassedOut)
                return;

            RegisterViolation("passout");
        }

        /// <summary>Сброс дневных счётчиков в начале нового игрового дня.</summary>
        public void OnDayStarted()
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.IsActive)
                return;

            plan.TodayFailed = false;
            plan.TodayCompleted = false;
            plan.ViolationsToday = 0;
            plan.TodayViolationReasons.Clear();

            _stateManager.Save();
            _monitor.Log("[RecoveryPlan] Новый день: сброс дневных флагов", LogLevel.Trace);
        }

        /// <summary>Зачёт или срыв текущего дня перед сном.</summary>
        public void OnDayEnding()
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.IsActive)
                return;

            int today = GameUtils.Today();
            if (plan.LastEvaluatedDay == today)
            {
                _monitor.Log("[RecoveryPlan] День уже оценён — пропуск", LogLevel.Trace);
                return;
            }

            bool completedPlan = false;

            if (!plan.TodayFailed)
            {
                plan.CompletedDays++;
                plan.TodayCompleted = true;
                Game1.addHUDMessage(new HUDMessage(HudDayCredited, HUDMessage.health_type));
                _monitor.Log(
                    $"[RecoveryPlan] День зачтён: {plan.CompletedDays}/{plan.RequiredDays}",
                    LogLevel.Info);
            }
            else
            {
                plan.TodayCompleted = false;
                _monitor.Log(
                    $"[RecoveryPlan] День не зачтён (нарушения: {string.Join(", ", plan.TodayViolationReasons)})",
                    LogLevel.Info);
            }

            plan.LastEvaluatedDay = today;

            if (plan.CompletedDays >= plan.RequiredDays)
            {
                plan.IsActive = false;
                plan.CompletionTalkPending = true;
                plan.RequiresHarveyTalk = true;
                completedPlan = true;
                Game1.addHUDMessage(new HUDMessage(HudCompleted, HUDMessage.achievement_type));
                _monitor.Log("[RecoveryPlan] План завершён — ожидается разговор с Харви", LogLevel.Info);
            }

            _stateManager.Save();

            if (completedPlan)
            {
                _dialogueManager.AddTopic(ConversationTopics.RecoveryPlanCompleted, CompletedTopicDays);
                _monitor.Log($"[RecoveryPlan] Итог: completed={plan.CompletedDays}, violations={plan.TotalViolations}", LogLevel.Info);
            }
        }

        public RecoveryPlanViewModel BuildViewModel()
        {
            var plan = GetActivePlan();
            if (plan == null)
                return RecoveryPlanViewModel.Empty;

            int daysRemaining = Math.Max(0, plan.RequiredDays - plan.CompletedDays);

            return new RecoveryPlanViewModel
            {
                HasPlan = true,
                IsActive = plan.IsActive,
                PlanId = plan.PlanId,
                Reason = plan.Reason,
                InjuryId = plan.InjuryId,
                StartDay = plan.StartDay,
                RequiredDays = plan.RequiredDays,
                CompletedDays = plan.CompletedDays,
                DaysRemaining = daysRemaining,
                TodayFailed = plan.TodayFailed,
                TodayCompleted = plan.TodayCompleted,
                ViolationsToday = plan.ViolationsToday,
                TotalViolations = plan.TotalViolations,
                TodayViolationReasons = plan.TodayViolationReasons.AsReadOnly(),
                RequiresHarveyTalk = plan.RequiresHarveyTalk,
                CompletionTalkPending = plan.CompletionTalkPending,
                LastEvaluatedDay = plan.LastEvaluatedDay,
            };
        }

        public bool IsCompletionTalkPending() =>
            GetActivePlan()?.CompletionTalkPending == true;

        /// <summary>
        /// Игрок кликнул по Харви при ожидании завершающего разговора (vanilla DialogueBox).
        /// </summary>
        public void NotifyHarveyClickedForCompletionTalk()
        {
            if (!IsCompletionTalkPending())
                return;

            _awaitingCompletionTalkAck = true;
            _completionTalkSawDialogue = Game1.activeClickableMenu is StardewValley.Menus.DialogueBox;
            _completionTalkStartedTick = Game1.ticks;

            _monitor.Log("[RecoveryPlan] Ожидаем завершающий диалог с Харви", LogLevel.Debug);
        }

        /// <summary>
        /// Отслеживает закрытие DialogueBox и снимает план после разговора.
        /// </summary>
        public void OnCompletionTalkDialogueUpdate()
        {
            if (!_awaitingCompletionTalkAck)
                return;

            if (!IsCompletionTalkPending())
            {
                ResetCompletionTalkTracking();
                return;
            }

            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                ResetCompletionTalkTracking();
                return;
            }

            int elapsed = Game1.ticks - _completionTalkStartedTick;
            if (elapsed > CompletionTalkDialogueTimeoutTicks)
            {
                _monitor.Log("[RecoveryPlan] Таймаут ожидания завершающего диалога — сброс трекинга", LogLevel.Debug);
                ResetCompletionTalkTracking();
                return;
            }

            if (Game1.activeClickableMenu is StardewValley.Menus.DialogueBox)
            {
                _completionTalkSawDialogue = true;
                return;
            }

            if (!_completionTalkSawDialogue)
            {
                if (elapsed < 60)
                    return;

                _completionTalkSawDialogue = true;
            }

            if (!Context.IsWorldReady)
                return;

            if (Game1.activeClickableMenu is StardewValley.Menus.DialogueBox)
                return;

            AcknowledgeCompletionTalk();
            ResetCompletionTalkTracking();
        }

        /// <summary>
        /// Завершающий разговор с Харви состоялся — снять режим и очистить state.
        /// </summary>
        public void AcknowledgeCompletionTalk()
        {
            var plan = GetActivePlan();
            if (plan == null || !plan.CompletionTalkPending)
                return;

            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanCompleted);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanStarted);
            _dialogueManager.RemoveTopic(ConversationTopics.RecoveryPlanViolated);

            ClearRecoveryPlan();

            _monitor.Log("[RecoveryPlan] Завершающий разговор с Харви учтён — план снят", LogLevel.Info);
        }

        public void ClearRecoveryPlan()
        {
            if (GetActivePlan() == null)
                return;

            ResetCompletionTalkTracking();
            _stateManager.ClearActiveRecoveryPlan();
            _monitor.Log("[RecoveryPlan] План снят", LogLevel.Info);
        }

        private void ResetCompletionTalkTracking()
        {
            _awaitingCompletionTalkAck = false;
            _completionTalkSawDialogue = false;
            _completionTalkStartedTick = -1;
        }

        public IEnumerable<string> GetStatusLines()
        {
            var plan = GetActivePlan();
            if (plan == null)
            {
                yield return "(none)";
                yield break;
            }

            int today = GameUtils.Today();
            yield return
                $"plan={plan.PlanId}  active={plan.IsActive}  reason={plan.Reason}  injury={plan.InjuryId ?? "(none)"}";
            yield return
                $"start={plan.StartDay}  today={today}  completed={plan.CompletedDays}/{plan.RequiredDays}  "
                + $"todayFailed={plan.TodayFailed}  todayCompleted={plan.TodayCompleted}";
            yield return
                $"violationsToday={plan.ViolationsToday}  totalViolations={plan.TotalViolations}  "
                + $"reasons=[{string.Join(", ", plan.TodayViolationReasons)}]";
            yield return
                $"requiresHarveyTalk={plan.RequiresHarveyTalk}  completionTalkPending={plan.CompletionTalkPending}  "
                + $"lastEvaluatedDay={plan.LastEvaluatedDay}";
        }

        private static bool TryResolveMineViolationReason(GameLocation location, out string reason)
        {
            string name = location.NameOrUniqueName ?? location.Name ?? "";

            if (name.Contains("SkullCave", StringComparison.OrdinalIgnoreCase))
            {
                reason = "skull_cave";
                return true;
            }

            if (location is MineShaft
                || string.Equals(name, "Mine", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "UndergroundMine", StringComparison.OrdinalIgnoreCase))
            {
                reason = "mine";
                return true;
            }

            reason = "";
            return false;
        }

        private string? ResolveInjuryId(string? injuryId)
        {
            if (!string.IsNullOrWhiteSpace(injuryId))
                return injuryId.Trim();

            return _stateManager.GetMainInjuryId()
                ?? NullIfEmpty(_stateManager.State.LastHospitalDischargeInjuryId);
        }

        private static string? NullIfEmpty(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private static bool ContainsReason(IList<string> reasons, string reason)
        {
            foreach (string existing in reasons)
            {
                if (string.Equals(existing, reason, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
