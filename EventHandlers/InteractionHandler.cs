using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HarveyOverhaul.InjuryCare.EventHandlers
{
    /// <summary>
    /// Обработчик взаимодействий с Харви (клики)
    /// </summary>
    public class InteractionHandler
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;
        private readonly TreatmentManager _treatmentManager;

        /// <summary>Последний результат проверок при клике (для дебаг-HUD).</summary>
        public string? LastClickDebug { get; private set; }

        public InteractionHandler(
            IMonitor monitor,
            IModHelper helper,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager,
            TreatmentManager treatmentManager)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
            _treatmentManager = treatmentManager;
        }

        /// <summary>
        /// Обработать нажатие кнопки
        /// </summary>
        public void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return;
            if (!e.Button.IsActionButton()) return;
            if (Game1.activeClickableMenu is DialogueBox) return;

            var loc = Game1.currentLocation;
            if (loc == null)
            {
                LastClickDebug = "Клик: нет currentLocation";
                return;
            }

            // GrabTile — тайл, который игра считает целью клика (SMAPI Input API)
            var tile = _helper.Input.GetCursorPosition().GrabTile;
            var harvey = HarveyHelper.GetHarveyAtTile(loc, tile);

            if (harvey == null)
            {
                LastClickDebug = $"Клик: Action GrabTile({tile.X:F0},{tile.Y:F0}) — не Харви";
                return;
            }

            // 1. Топики завершения лечения
            if (CheckAndHandleCompletionTopic(harvey))
            {
                LastClickDebug = "Клик: Харви + топик завершения → Suppress(e)";
                _helper.Input.Suppress(e.Button);
                return;
            }

            // Собираем все травмы + осложнения один раз
            var modDebuffs = _stateManager.GetAllActiveDebuffStates()
                .Where(d => d.BuffId.StartsWith("buff"))
                .ToList();

            var injuries = _injuryManager.CollectAllInjuries();

            // 2. Следующий нелеченный дебафф мода (по приоритету)
            var nextToTreat = modDebuffs
                .Where(d => !d.TreatmentStarted && _buffManager.HasBuff(d.BuffId))
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault();

            if (nextToTreat != null)
            {
                LastClickDebug = $"Клик: Харви + травма={nextToTreat.BuffId} → StartTreatment";
                _monitor.Log($"Начало лечения: {nextToTreat.BuffId}", LogLevel.Info);
                _helper.Input.Suppress(e.Button);
                injuries.MainInjury = nextToTreat.BuffId;
                StartTreatment(harvey, injuries, nextToTreat.BuffId);
                return;
            }

            // 3. Нет нелеченных травм, но есть осложнения — лечим осложнения
            if (injuries.Complications.Count > 0)
            {
                LastClickDebug = $"Клик: Харви + осложнений={injuries.Complications.Count} → TreatComplications";
                _monitor.Log($"Лечение осложнений: {string.Join(", ", injuries.Complications)}", LogLevel.Info);
                _helper.Input.Suppress(e.Button);
                StartTreatment(harvey, injuries, null);
                return;
            }

            // 4. Все дебаффы уже в лечении — ищем первую готовую к смене фазы / выздоровлению (по приоритету)
            var inTreatmentReady = modDebuffs
                .Where(d => d.TreatmentStarted)
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault(d =>
                    d.IsLastPhase
                        ? _treatmentManager.IsInjuryReadyForRecovery(d.BuffId)
                        : _treatmentManager.IsInjuryReadyForNextPhase(d.BuffId, out _, out _));

            if (inTreatmentReady != null && CheckAndHandlePhaseTransition(harvey, inTreatmentReady.BuffId, inTreatmentReady))
            {
                LastClickDebug = $"Клик: Харви + готовая фаза/выздоровление {inTreatmentReady.BuffId} → Suppress(e)";
                _helper.Input.Suppress(e.Button);
                return;
            }

            var inTreatmentNotReady = modDebuffs.Where(d => d.TreatmentStarted).ToList();
            if (inTreatmentNotReady.Count > 0)
            {
                _monitor.Log(
                    $"Травмы в лечении ({inTreatmentNotReady.Count}), но ни одна не готова к переходу: {string.Join(", ", inTreatmentNotReady.Select(d => d.BuffId))}",
                    LogLevel.Debug);
            }

            // 5. Нечего обрабатывать — стандартный диалог игры (не подавляем)
            LastClickDebug = inTreatmentNotReady.Count > 0
                ? "Клик: Харви, травмы в лечении но не готовы → стандартный диалог"
                : "Клик: Харви, нечего обрабатывать → стандартный диалог";
        }

        /// <summary>
        /// Диагностика: какой шаг лечения сработал бы при клике по Харви (без побочных эффектов).
        /// </summary>
        public string BuildDebugTreatmentDecision()
        {
            if (!Context.IsWorldReady)
                return "not world ready";

            var modDebuffs = _stateManager.GetAllActiveDebuffStates()
                .Where(d => d.BuffId.StartsWith("buff"))
                .ToList();

            var injuries = _injuryManager.CollectAllInjuries();

            var nextToTreat = modDebuffs
                .Where(d => !d.TreatmentStarted && _buffManager.HasBuff(d.BuffId))
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault();

            if (nextToTreat != null)
                return $"WOULD: start treatment {nextToTreat.BuffId}";

            if (injuries.Complications.Count > 0)
                return $"WOULD: treat complications: {string.Join(", ", injuries.Complications)}";

            var inTreatment = modDebuffs
                .Where(d => d.TreatmentStarted)
                .OrderByDescending(d => GetInjuryPriority(d.BuffId))
                .FirstOrDefault();

            if (inTreatment != null)
            {
                string id = inTreatment.BuffId;
                if (inTreatment.IsLastPhase && _treatmentManager.IsInjuryReadyForRecovery(id))
                    return $"WOULD: complete recovery {id}";

                if (_treatmentManager.IsInjuryReadyForNextPhase(id, out int current, out int next))
                    return $"WOULD: advance phase {id} {current}->{next}";

                return $"WOULD: standard dialogue; in treatment but not ready: {id}";
            }

            return "WOULD: standard dialogue; no injuries";
        }
        
        /// <summary>
        /// Получить приоритет травмы (выше = важнее)
        /// </summary>
        private int GetInjuryPriority(string buffId)
        {
            return buffId switch
            {
                "buffConcussion" => 100,
                "buffInfectedWound" => 90,
                "buffFracturedBone" => 85,
                "buffSurgicalWound" => 80,
                "buffShrapnelWounds" => 75,
                "buffBurnWounds" => 70,
                "buffDeepCuts" => 65,
                "buffTornMuscles" => 60,
                "buffBackStrain" => 55,
                "buffBruisedRibs" => 50,
                "buffSprainedAnkle" => 45,
                "buffBadlyHurt" => 40,
                "buffHurt" => 30,
                _ => 0
            };
        }


        /// <summary>
        /// Проверить и обработать переход фазы (при разговоре с Харви)
        /// </summary>
        private bool CheckAndHandlePhaseTransition(NPC harvey, string injuryId, Core.Models.DebuffState debuffState)
        {
            // На последней фазе приоритет у выздоровления, иначе сработает «смена фазы» и тот же бафф останется
            if (debuffState.IsLastPhase && _treatmentManager.IsInjuryReadyForRecovery(injuryId))
            {
                _monitor.Log($"🎉 Готовность к выздоровлению: {injuryId} (последняя фаза)", LogLevel.Info);
                CompleteRecovery(harvey, injuryId);
                return true;
            }

            // Готовность к смене фазы (переход на следующую)
            if (_treatmentManager.IsInjuryReadyForNextPhase(injuryId, out int currentPhase, out int nextPhase))
            {
                _monitor.Log($"🔄 Готовность к смене фазы: {injuryId} {currentPhase} → {nextPhase}", LogLevel.Info);
                AdvanceToNextPhase(harvey, injuryId, debuffState);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Переход к следующей фазе (при разговоре с Харви)
        /// </summary>
        private void AdvanceToNextPhase(NPC harvey, string injuryId, Core.Models.DebuffState debuffState)
        {
            int oldPhase = debuffState.CurrentPhase;
            int nextPhase = oldPhase + 1;

            _monitor.Log($"🔄 Переход к фазе {oldPhase} → {nextPhase} для {injuryId}", LogLevel.Info);

            _treatmentManager.AdvanceInjuryToNextPhase(injuryId);

            string injuryName = injuryId.Replace("buff", "");
            string phaseDialogue = _dialogueManager.PickRandomDialogueByPrefix(
                $"PhaseTransition_{injuryName}_{nextPhase}",
                "Отлично! Рана хорошо заживает. Переходим к следующему этапу лечения.$u");

            _dialogueManager.Speak(harvey, phaseDialogue);
            Game1.player.changeFriendship(10, harvey);

            _dialogueManager.ShowEmote(harvey, HarveyHelper.GetRecoveryEmote());
        }

        /// <summary>
        /// Полное выздоровление
        /// </summary>
        private void CompleteRecovery(NPC harvey, string injuryId)
        {
            _monitor.Log($"Полное выздоровление от {injuryId}", LogLevel.Info);
            
            _injuryManager.RemoveAllPhaseBuffs(injuryId);
            _stateManager.RemoveDebuffState(injuryId);
            _injuryManager.NotifyInjuryRecovered(injuryId);

            // Удаляем все топики, связанные с этой травмой (начало лечения, фазы), чтобы не висели и не путали алгоритм
            RemoveInjuryRelatedTopics(injuryId);
            
            // Применяем бафф заботы Харви на 2 дня
            _buffManager.AddBuff(Core.CureBuffs.Care, 2880000); // 2 дня = 2880000 мс
            _monitor.Log($"💖 Применён бафф заботы Харви на 2 дня", LogLevel.Info);
            
            // Создаём топик завершения
            string completionTopic = TopicIds.GetCuredTopic(injuryId);
            _dialogueManager.AddTopic(completionTopic, 7);
            _monitor.Log($"✉️ Создан топик завершения: {completionTopic}", LogLevel.Debug);
            
            _stateManager.Save();

            // Показать диалог завершения
            string completionDialogue = _dialogueManager.PickRandomDialogueByPrefix(
                completionTopic,
                "Поздравляю! Ты полностью ${выздоровел^выздоровела}$!$0");
            
            _dialogueManager.Speak(harvey, completionDialogue);
            Game1.player.changeFriendship(15, harvey);

            // Показать эмоцию
            _dialogueManager.ShowEmote(harvey, HarveyHelper.GetRecoveryEmote());

            // Уведомление игроку
            Game1.addHUDMessage(new HUDMessage("Выздоровление завершено! Харви гордится тобой!", HUDMessage.achievement_type));
        }

        /// <summary>
        /// Удалить все топики, относящиеся к травме: базовый топик, топик лечения, фазовые топики (1–3).
        /// Вызывается при полном выздоровлении, чтобы они не висели в списке и не влияли на логику.
        /// </summary>
        private void RemoveInjuryRelatedTopics(string injuryId)
        {
            _dialogueManager.RemoveTopic(TopicIds.GetInjuryTopic(injuryId));
            _dialogueManager.RemoveTopic(TopicIds.GetTreatmentTopic(injuryId));
            for (int phase = 1; phase <= 3; phase++)
                _dialogueManager.RemoveTopic(_injuryManager.GetPhaseTopicId(injuryId, phase));
            _monitor.Log($"Удалены топики травмы: {injuryId} (базовый, лечение, фазы 1–3)", LogLevel.Debug);
        }

        /// <summary>
        /// Начать лечение травмы (с защитой от конфликтов диалогов)
        /// </summary>
        private void StartTreatment(NPC harvey, InjuryCollection injuries, string? mainInjuryId)
        {
            try
            {
                if (Game1.activeClickableMenu is DialogueBox)
                    return;

                _treatmentManager.TreatWithReaction(harvey, injuries);

                if (mainInjuryId != null)
                {
                    _treatmentManager.ApplyTreatmentForInjury(mainInjuryId);
                    _dialogueManager.ClearHarveyNeedsFirstTreatmentTopic("лечение начато (клик по Харви)");
                }

                if (injuries.Complications.Count > 0)
                    _treatmentManager.TreatAllComplications(injuries.Complications);

                string combinedDialogue = _treatmentManager.BuildCombinedDialogue(injuries);

                Game1.delayedActions.Add(new StardewValley.DelayedAction(1000, () =>
                {
                    if (Game1.activeClickableMenu is not DialogueBox)
                    {
                        _dialogueManager.Speak(harvey, combinedDialogue);
                        Game1.player.changeFriendship(10, harvey);
                        _dialogueManager.ShowEmote(harvey, HarveyHelper.GetCaringEmote());
                    }
                }));

                _monitor.Log($"Лечение начато: травма={mainInjuryId ?? "нет"}, осложнений={injuries.Complications.Count}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка StartTreatment: {ex}", LogLevel.Error);
                _dialogueManager.Speak(harvey, "Что-то пошло не так с лечением. Попробуй ещё раз.$0");
            }
        }

        /// <summary>
        /// Проверить, находится ли игрок в клинике
        /// </summary>
        private bool IsClinic(GameLocation location)
        {
            return string.Equals(location?.Name, _config.HospitalLocationName, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Проверить и обработать топик завершения лечения
        /// </summary>
        private bool CheckAndHandleCompletionTopic(NPC harvey)
        {
            // Список возможных топиков завершения
            var completionTopics = new[]
            {
                TopicIds.GetCuredTopic("buffHurt"),
                TopicIds.GetCuredTopic("buffBadlyHurt"),
                TopicIds.GetCuredTopic("buffBruisedRibs"),
                TopicIds.GetCuredTopic("buffSprainedAnkle"),
                TopicIds.GetCuredTopic("buffBackStrain"),
                TopicIds.GetCuredTopic("buffDeepCuts"),
                TopicIds.GetCuredTopic("buffBurnWounds"),
                TopicIds.GetCuredTopic("buffTornMuscles"),
                TopicIds.GetCuredTopic("buffConcussion"),
                TopicIds.GetCuredTopic("buffFracturedBone"),
                TopicIds.GetCuredTopic("buffShrapnelWounds"),
                TopicIds.GetCuredTopic("buffInfectedWound"),
                ConversationTopics.ColdCured,
                ConversationTopics.SurgicalWoundCured,
            };

            foreach (var topicId in completionTopics)
            {
                if (_dialogueManager.HasTopic(topicId))
                {
                    _monitor.Log($"🎉 Обнаружен топик завершения: {topicId}", LogLevel.Info);
                    _helper.Input.Suppress(StardewModdingAPI.SButton.MouseLeft);
                    _helper.Input.Suppress(StardewModdingAPI.SButton.MouseRight);
                    
                    ShowCompletionDialogue(harvey, topicId);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Показать диалог завершения лечения
        /// </summary>
        private void ShowCompletionDialogue(NPC harvey, string topicId)
        {
            // Извлекаем название травмы из топика (формат topic<injuryName>Cured)
            string injuryName = topicId.Replace("topic", "").Replace("Cured", "");
            string buffId = "buff" + injuryName;

            // Снимаем лечебный бафф простой травмы (интенсивная терапия, повязка и т.д.), если есть
            if (Core.SimpleInjuryCures.Map.TryGetValue(buffId, out var cureBuff))
            {
                _buffManager.RemoveBuff(cureBuff);
                _monitor.Log($"Снят лечебный бафф: {cureBuff} (завершение {buffId})", LogLevel.Info);
            }

            _monitor.Log($"💚 Завершение лечения: {injuryName}", LogLevel.Info);

            _dialogueManager.ShowEmote(harvey, HarveyHelper.GetRecoveryEmote());

            // Загружаем диалог завершения из JSON (используем сам topicId)
            string completionText = _dialogueManager.PickRandomDialogueByPrefix(
                topicId,
                "Отлично! Ты полностью ${выздоровел^выздоровела}$. Я горжусь тобой за то, что ты следовал${^а}$ всем моим рекомендациям. Береги себя!$h");

            _dialogueManager.Speak(harvey, completionText);

            // Применяем бафф заботы
            _buffManager.AddBuff(Core.CureBuffs.Care, 480); // 8 игровых часов
            _monitor.Log($"💖 Применён бафф заботы на 8 часов", LogLevel.Debug);

            // Удаляем топик завершения
            _dialogueManager.RemoveTopic(topicId);

            // Дружба +10
            Game1.player.changeFriendship(10, harvey);
            _monitor.Log($"Завершение лечения обработано, дружба +10", LogLevel.Info);
        }
    }
}

