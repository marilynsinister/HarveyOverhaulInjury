using System;
using System.Collections.Generic;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление лечением травм
    /// </summary>
    public class TreatmentManager
    {
        private readonly IMonitor _monitor;
        private readonly BuffManager _buffManager;
        private readonly InjuryManager _injuryManager;
        private readonly DialogueManager _dialogueManager;
        private readonly StateManager _stateManager;

        // Маппинг травм к лечебным баффам (только для простых травм БЕЗ фаз)
        public static readonly Dictionary<string, string> CureByInjury = new()
        {
            { "buffHurt", CureBuffs.Treatment },
            { "buffBadlyHurt", CureBuffs.IntensiveCare },
            { "buffSurgicalWound", CureBuffs.PostSurgical }
        };

        /// <summary>
        /// Простое (нефазовое) лечение: buffHurt, buffBadlyHurt, buffSurgicalWound.
        /// Для них не используются injury_phase_ready / injury_phase_advance.
        /// </summary>
        public static bool IsSimpleTreatmentInjury(string injuryId) =>
            CureByInjury.ContainsKey(injuryId);
        
        // Травмы с фазовой системой (используют свои фазовые баффы травм)
        public static readonly HashSet<string> PhasedInjuries = new()
        {
            "buffConcussion",
            "buffFracturedBone",
            "buffTornMuscles",
            "buffSprainedAnkle",
            "buffBruisedRibs",
            "buffDeepCuts",
            "buffBurnWounds",
            "buffInfectedWound",
            "buffBackStrain",
            "buffShrapnelWounds",
            "buffCold" // Простуда (2 фазы: острая + восстановление)
        };

        public TreatmentManager(
            IMonitor monitor,
            BuffManager buffManager,
            InjuryManager injuryManager,
            DialogueManager dialogueManager,
            StateManager stateManager)
        {
            _monitor = monitor;
            _buffManager = buffManager;
            _injuryManager = injuryManager;
            _dialogueManager = dialogueManager;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Получить ID фазового баффа травмы (используется для восстановления)
        /// </summary>
        public string GetInjuryPhaseBuffId(string injuryId, int phase)
        {
            // Используем существующие методы InjuryManager
            return _injuryManager.GetPhaseBuffId(injuryId, phase);
        }
        
        /// <summary>
        /// Проверить готовность травмы к смене фазы
        /// </summary>
        public bool IsInjuryReadyForNextPhase(string injuryId, out int currentPhase, out int nextPhase)
        {
            currentPhase = 0;
            nextPhase = 0;

            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
                return false;

            if (debuffState.TotalPhases <= 0)
                return false;

            if (!debuffState.IsInTreatment)
                return false;

            if (debuffState.CurrentPhase <= 0)
                return false;

            if (debuffState.CurrentPhase >= debuffState.TotalPhases)
                return false;

            currentPhase = debuffState.CurrentPhase;
            nextPhase = currentPhase + 1;

            return debuffState.ReadyForNextPhase;
        }
        
        /// <summary>
        /// Проверить готовность травмы к полному выздоровлению
        /// </summary>
        public bool IsInjuryReadyForRecovery(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                return false;
            }
            
            // Проверяем флаг готовности из DebuffState
            return debuffState.ReadyForRecovery;
        }
        
        /// <summary>
        /// Сменить фазу травмы (вызывается при разговоре с Харви)
        /// </summary>
        public void AdvanceInjuryToNextPhase(string injuryId)
        {
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} не найдено", LogLevel.Warn);
                return;
            }

            if (debuffState.TotalPhases <= 0 || debuffState.CurrentPhase >= debuffState.TotalPhases)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: смена фазы невозможна (TotalPhases={debuffState.TotalPhases}, CurrentPhase={debuffState.CurrentPhase}). " +
                    "Для простого лечения используйте injury_phase_recovery или injury_phase_cure.",
                    LogLevel.Warn);
                return;
            }

            if (debuffState.CurrentPhase <= 0)
            {
                _monitor.Log($"⚠️ {injuryId}: лечение не начато (CurrentPhase={debuffState.CurrentPhase}), смена фазы пропущена", LogLevel.Warn);
                return;
            }

            if (!debuffState.ReadyForNextPhase)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: ReadyForNextPhase=false (фаза {debuffState.CurrentPhase}/{debuffState.TotalPhases}), смена фазы пропущена",
                    LogLevel.Warn);
                return;
            }

            int oldPhase = debuffState.CurrentPhase;

            _monitor.Log($"🔄 Смена фазы {injuryId}: {oldPhase} → {oldPhase + 1}", LogLevel.Info);

            string oldPhaseBuffId = _injuryManager.GetPhaseBuffId(injuryId, oldPhase);
            _buffManager.RemoveBuff(oldPhaseBuffId);
            _monitor.Log($"❌ Удалён бафф фазы {oldPhase}: {oldPhaseBuffId}", LogLevel.Debug);

            int currentDay = (int)StardewValley.Game1.stats.DaysPlayed;
            _stateManager.AdvancePhase(injuryId, currentDay);

            var updatedState = _stateManager.GetDebuffState(injuryId);
            if (updatedState == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} исчезло после AdvancePhase", LogLevel.Warn);
                return;
            }

            int actualNewPhase = updatedState.CurrentPhase;
            if (actualNewPhase <= oldPhase)
            {
                _monitor.Log(
                    $"⚠️ {injuryId}: AdvancePhase не изменил фазу ({oldPhase} → {actualNewPhase}), новый бафф не накладывается",
                    LogLevel.Warn);
                return;
            }

            if (updatedState.ReadyForNextPhase)
                _stateManager.SetReadyForNextPhase(injuryId, false);

            string newPhaseBuffId = _injuryManager.GetPhaseBuffId(injuryId, actualNewPhase);
            _buffManager.AddBuff(newPhaseBuffId, -2);
            _monitor.Log($"✅ Применён бафф фазы {actualNewPhase}: {newPhaseBuffId}", LogLevel.Info);

            string oldPhaseTopicId = _injuryManager.GetPhaseTopicId(injuryId, oldPhase);
            _dialogueManager.RemoveTopic(oldPhaseTopicId);
            int topicDays = Math.Max(1, updatedState.GetCurrentPhaseDuration());
            string newPhaseTopicId = _injuryManager.GetPhaseTopicId(injuryId, actualNewPhase);
            _dialogueManager.AddTopic(newPhaseTopicId, topicDays);
            _monitor.Log($"💬 Phase topic {oldPhaseTopicId} → {newPhaseTopicId} ({topicDays} дн.)", LogLevel.Debug);

            string phaseName = GetPhaseDisplayName(actualNewPhase);
            StardewValley.Game1.addHUDMessage(new StardewValley.HUDMessage(
                $"Переход к фазе: {phaseName}",
                StardewValley.HUDMessage.health_type));
        }
        
        /// <summary>
        /// Механическое завершение фазовой травмы без диалога (канон для игрового клика).
        /// </summary>
        public void ApplyMechanicalPhasedRecovery(string injuryId, int careDurationMs = 2880000)
        {
            _injuryManager.RemoveAllPhaseBuffs(injuryId);
            _stateManager.RemoveDebuffState(injuryId);
            _injuryManager.NotifyInjuryRecovered(injuryId);

            _dialogueManager.RemoveTopic(TopicIds.GetInjuryTopic(injuryId));
            _dialogueManager.RemoveTopic(TopicIds.GetTreatmentTopic(injuryId));
            for (int phase = 1; phase <= 3; phase++)
                _dialogueManager.RemoveTopic(_injuryManager.GetPhaseTopicId(injuryId, phase));

            _buffManager.AddBuff(CureBuffs.Care, careDurationMs);
            _monitor.Log($"Механическое выздоровление применено: {injuryId}, Care={careDurationMs}ms", LogLevel.Debug);
        }

        /// <summary>
        /// Завершить лечение травмы (debug-команда injury_phase_cure).
        /// </summary>
        public void CompleteInjuryRecovery(string injuryId)
        {
            if (_stateManager.GetDebuffState(injuryId) == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} не найдено", LogLevel.Warn);
                return;
            }

            _monitor.Log($"🎉 Debug-завершение лечения {injuryId}", LogLevel.Info);
            ApplyMechanicalPhasedRecovery(injuryId, careDurationMs: 28800000);
            _dialogueManager.AddTopic(ConversationTopics.TreatmentCompleted, 7);

            StardewValley.Game1.addHUDMessage(new StardewValley.HUDMessage(
                "🎉 Лечение завершено! Ты полностью здоров${^а}$!",
                StardewValley.HUDMessage.achievement_type));
        }
        
        private string GetPhaseDisplayName(int phase)
        {
            return phase switch
            {
                1 => "Острая фаза",
                2 => "Заживление",
                3 => "Восстановление",
                _ => $"Фаза {phase}"
            };
        }

        /// <summary>
        /// Применить лечение для конкретной травмы
        /// </summary>
        public void ApplyTreatmentForInjury(string injuryId)
        {
            _monitor.Log($"Применяем лечение для {injuryId}", LogLevel.Info);

            // Удаляем топик «нелеченной» травмы — он для диалогов, больше не нужен
            _dialogueManager.RemoveTopic(TopicIds.GetInjuryTopic(injuryId));

            if (PhasedInjuries.Contains(injuryId))
            {
                StartPhasedTreatment(injuryId);
            }
            else if (IsSimpleTreatmentInjury(injuryId))
            {
                _buffManager.RemoveBuff(injuryId);
                _buffManager.AddBuff(CureByInjury[injuryId], -2);

                // Записываем в DebuffState: лечение началось, PhaseStartDay = сегодня,
                // Phase1Duration = срок лечения (для CheckSimpleTreatmentCompletion)
                int today = (int)StardewValley.Game1.stats.DaysPlayed;
                int treatmentDays = CalculateTopicDuration(injuryId);

                var ds = _stateManager.GetDebuffState(injuryId);
                if (ds != null)
                {
                    ds.TreatmentStarted = true;
                    ds.PhaseStartDay    = today;
                    ds.Phase1Duration   = treatmentDays;
                    _stateManager.UpdateDebuffState(injuryId, ds);
                }

                _monitor.Log($"Нефазовое лечение начато: {CureByInjury[injuryId]}, срок={treatmentDays} дней", LogLevel.Info);

                if (string.Equals(injuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
                    _dialogueManager.TryAddDiagnosisCompleteTopic(injuryId);
            }
            else
            {
                _monitor.Log($"Лечебный бафф для {injuryId} не найден", LogLevel.Warn);
            }
        }
        
        /// <summary>
        /// Начать фазовое лечение (для травм с фазовыми баффами)
        /// </summary>
        private void StartPhasedTreatment(string injuryId)
        {
            _monitor.Log($"🏥 Начинаем фазовое лечение для {injuryId}", LogLevel.Info);
            
            // Получаем состояние дебаффа
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState == null)
            {
                _monitor.Log($"⚠️ Состояние дебаффа для {injuryId} не найдено!", LogLevel.Warn);
                return;
            }
            
            // ВАЖНО: Заменяем базовый бафф травмы на Фазу 1
            _buffManager.RemoveBuff(injuryId);  // Удаляем базовый бафф
            _monitor.Log($"❌ Удалён базовый бафф: {injuryId}", LogLevel.Debug);
            
            // Применяем Фазу 1
            string phase1BuffId = _injuryManager.GetPhaseBuffId(injuryId, 1);
            _buffManager.AddBuff(phase1BuffId, -2);
            _monitor.Log($"✅ Применена Фаза 1: {phase1BuffId}", LogLevel.Info);
            
            // Обновляем состояние - лечение началось
            int currentDay = (int)StardewValley.Game1.stats.DaysPlayed;
            _stateManager.StartTreatment(injuryId, currentDay);

            string phase1TopicId = _injuryManager.GetPhaseTopicId(injuryId, 1);
            int phase1TopicDays = Math.Max(1, debuffState.Phase1Duration);
            _dialogueManager.AddTopic(phase1TopicId, phase1TopicDays);
            _monitor.Log($"💬 Phase topic: {phase1TopicId} на {phase1TopicDays} дн.", LogLevel.Debug);

            // Создаём топик наблюдения
            string treatmentTopicId = TopicIds.GetTreatmentTopic(injuryId);
            int totalDuration = debuffState.GetTotalDuration();
            _dialogueManager.AddTopic(treatmentTopicId, totalDuration);
            _monitor.Log($"🏥 Создан топик фазового лечения: {treatmentTopicId} на {totalDuration} дней", LogLevel.Debug);

            _dialogueManager.TryAddDiagnosisCompleteTopic(injuryId);
        }

        /// <summary>
        /// Показать реакцию Харви и начать лечение (с защитой от зависаний)
        /// </summary>
        public void TreatWithReaction(NPC harvey, InjuryCollection injuries)
        {
            try
            {
                // Проверяем, что Харви существует и не занят
                if (harvey == null)
                {
                    _monitor.Log("⚠️ Харви не найден, пропускаем реакцию", LogLevel.Warn);
                    return;
                }

                // Определяем эмоцию на основе тяжести травм
                int emote = DetermineEmoteForInjuries(injuries);
                
                // Определяем текстовое сообщение
                string textMessage = DetermineTextForInjuries(injuries);
                
                // Показываем эмоцию с текстом
                _dialogueManager.ShowEmoteWithText(harvey, emote, textMessage);
                
                // Звук
                Game1.playSound(GetSoundForInjuries(injuries));
                
                _monitor.Log($"😊 Харви отреагировал эмоцией {emote} и текстом '{textMessage}'", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"❌ Ошибка при показе реакции Харви: {ex}", LogLevel.Error);
                // Показываем простую эмоцию в случае ошибки
                try
                {
                    _dialogueManager.ShowEmote(harvey, HarveyHelper.GetCaringEmote());
                }
                catch
                {
                    // Если и это не работает, просто логируем
                    _monitor.Log("❌ Не удалось показать даже простую эмоцию", LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Определить эмоцию Харви на основе травм
        /// </summary>
        public int DetermineEmoteForInjuries(InjuryCollection injuries)
        {
            // Критические травмы - тревога
            if (injuries.MainInjury != null && IsCriticalInjury(injuries.MainInjury))
            {
                return HarveyEmotes.CriticalInjury; // Восклицание
            }

            // Серьёзные травмы с осложнениями - беспокойство
            if (injuries.MainInjury != null && IsSeriousInjury(injuries.MainInjury) && injuries.Complications.Count > 0)
            {
                return HarveyEmotes.WorriedAboutPatient; // Грусть
            }

            // Множественные осложнения - вопрос
            if (injuries.Complications.Count >= 2)
            {
                return HarveyEmotes.FoundComplication; // Вопрос
            }

            // Одна травма — забота (♥ только при dating/married)
            if (injuries.MainInjury != null)
            {
                return HarveyHelper.GetCaringEmote();
            }

            // Только осложнения - восклицание
            if (injuries.Complications.Count > 0)
            {
                return HarveyEmotes.DirtyWound; // Восклицание
            }

            // По умолчанию - дружелюбие
            return HarveyEmotes.StartTreatment; // Улыбка
        }

        /// <summary>
        /// Получить звук для травм
        /// </summary>
        private string GetSoundForInjuries(InjuryCollection injuries)
        {
            if (injuries.MainInjury != null && IsCriticalInjury(injuries.MainInjury))
            {
                return "debuffHit"; // Критическая травма
            }

            if (injuries.Complications.Count > 0)
            {
                return "debuffSpell"; // Осложнения
            }

            return "healSound"; // Обычное лечение
        }

        private bool IsCriticalInjury(string injuryId)
        {
            return injuryId switch
            {
                "buffConcussion" => true,
                "buffFracturedBone" => true,
                "buffInfectedWound" => true,
                "buffBadlyHurt" => true,
                _ => false
            };
        }

        private bool IsSeriousInjury(string injuryId)
        {
            return injuryId switch
            {
                "buffShrapnelWounds" => true,
                "buffBurnWounds" => true,
                "buffSurgicalWound" => true,
                "buffDeepCuts" => true,
                "buffTornMuscles" => true,
                _ => IsCriticalInjury(injuryId)
            };
        }

        /// <summary>
        /// Определить текстовое сообщение для травм
        /// </summary>
        public string DetermineTextForInjuries(InjuryCollection injuries)
        {
            // Критические травмы
            if (injuries.MainInjury != null && IsCriticalInjury(injuries.MainInjury))
            {
                return TextMessageSelector.ForInjuryDiscovery(isCritical: true, isSerious: false);
            }

            // Множественные осложнения
            if (injuries.Complications.Count >= 3)
            {
                return HarveyTextMessages.MultipleInjuries;
            }

            // Серьёзная травма с осложнениями
            if (injuries.MainInjury != null && IsSeriousInjury(injuries.MainInjury) && injuries.Complications.Count > 0)
            {
                return TextMessageSelector.ForInjuryDiscovery(isCritical: false, isSerious: true);
            }

            // Специфичные осложнения
            if (injuries.Complications.Contains(InjuryBuffs.DirtyWound))
            {
                return HarveyTextMessages.DirtyWound;
            }

            if (injuries.Complications.Contains(InjuryBuffs.WetBandage))
            {
                return HarveyTextMessages.WetBandage;
            }

            if (injuries.Complications.Contains(InjuryBuffs.WetStitches))
            {
                return HarveyTextMessages.WetStitches;
            }

            if (injuries.Complications.Contains(InjuryBuffs.AllergicRash))
            {
                return HarveyTextMessages.AllergicReaction;
            }

            // Обычное лечение
            if (injuries.MainInjury != null)
            {
                return TextMessageSelector.ForTreatmentStart(injuries.Complications.Count > 0);
            }

            // По умолчанию
            return HarveyTextMessages.StartingTreatment;
        }

        /// <summary>
        /// Вылечить все осложнения — убираем баффы и удаляем из _state
        /// </summary>
        public void TreatAllComplications(List<string> complications)
        {
            foreach (var compId in complications)
            {
                _buffManager.RemoveBuff(compId);
                _stateManager.State.ActiveComplications.Remove(compId);
                _stateManager.RemoveDebuffState(compId);
                _stateManager.State.TopicMemory.Remove(compId);

                // Диалоговый топик убираем тоже
                string topicId = TopicIds.GetComplicationTopic(compId);
                _dialogueManager.RemoveTopic(topicId);

                _monitor.Log($"Осложнение вылечено и удалено из state: {compId}", LogLevel.Info);
            }
            _stateManager.Save();
        }

        /// <summary>
        /// Проверить наличие соответствующего лечения для травмы
        /// </summary>
        public bool HasMatchingTreatment(string? injuryId)
        {
            if (injuryId == null) return false;

            // Для фазовых травм лечение считается активным если TreatmentStarted = true
            if (PhasedInjuries.Contains(injuryId))
            {
                var debuffState = _stateManager.GetDebuffState(injuryId);
                return debuffState?.TreatmentStarted == true;
            }

            // Для нефазовых — проверяем наличие лечебного баффа
            if (!CureByInjury.TryGetValue(injuryId, out var cure)) return false;
            return _buffManager.HasBuff(cure);
        }

        /// <summary>
        /// Построить диалог лечения из топиков
        /// </summary>
        /// <param name="markTreatmentDiscussed">Если false — только выбор текста, без записи в state.</param>
        public string BuildCombinedDialogue(InjuryCollection injuries, bool markTreatmentDiscussed = true)
        {
            var parts = new List<string>();

            // Основная травма
            if (injuries.MainInjury != null)
            {
                string mainText = GetTreatmentDialogue(injuries.MainInjury, markTreatmentDiscussed);
                parts.Add(mainText);
            }

            // Осложнения
            AddComplicationDialogue(parts, injuries, InjuryBuffs.DirtyWound, "Proximity_DirtyWound", 
                "И рана загрязнилась — сейчас обработаю.$a");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.WetBandage, "Proximity_WetBandage", 
                "Повязка промокла. Меняю на сухую.$0");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.WetStitches, "Proximity_WetStitches", 
                "Швы намокли — наложу водонепроницаемую повязку.$a");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.AllergicRash, "Proximity_AllergicRash", 
                "Есть аллергическая реакция. Сменю препарат.$u");
            AddComplicationDialogue(parts, injuries, InjuryBuffs.PainFlare, "Proximity_PainFlare", 
                "Метеочувствительность даёт о себе знать. Дам обезболивающее.$0");

            return string.Join("$b", parts);
        }

        private void AddComplicationDialogue(
            List<string> parts, 
            InjuryCollection injuries, 
            string buffId, 
            string prefix, 
            string fallback)
        {
            if (injuries.Complications.Contains(buffId))
            {
                string line = _dialogueManager.PickRandomDialogueByPrefix(prefix, fallback);
                parts.Add(line);
            }
        }

        private string GetTreatmentDialogue(string injuryId, bool markTreatmentDiscussed = true)
        {
            // Убираем префикс "buff" для получения чистого названия травмы
            string cleanInjuryId = injuryId.Replace("buff", "");
            
            // Проверяем, был ли уже разговор о лечении этой травмы
            bool wasDiscussed = _stateManager.GetDebuffState(injuryId)?.HarveyConversationHappened == true;
            
            _monitor.Log($"Получаем диалог лечения для: {cleanInjuryId}, wasDiscussed={wasDiscussed}", LogLevel.Debug);
            
            // Получаем диалог с учётом состояния разговора
            string dialogue = _dialogueManager.PickTreatmentDialogue(cleanInjuryId, wasDiscussed, 
                $"Сейчас займусь твоей травмой.$u");
            
            _monitor.Log($"Получен диалог лечения: {dialogue}", LogLevel.Debug);
            
            if (markTreatmentDiscussed)
                _stateManager.MarkHarveyConversation(injuryId, true);
            
            return dialogue;
        }

        /// <summary>
        /// Рассчитать длительность топика на основе травмы
        /// </summary>
        public int CalculateTopicDuration(string injuryId)
        {
            return injuryId switch
            {
                "buffHurt" => 2,
                "buffBadlyHurt" => 4,
                "buffConcussion" => 9,
                "buffFracturedBone" => 18,
                "buffSurgicalWound" => 7,
                "buffInfectedWound" => 6,
                "buffShrapnelWounds" => 11,
                "buffBurnWounds" => 8,
                "buffDeepCuts" => 7,
                _ => 3
            };
        }
    }
}

