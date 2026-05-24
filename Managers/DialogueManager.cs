using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление диалогами и разговорными топиками
    /// </summary>
    public class DialogueManager
    {
        private readonly IMonitor _monitor;
        private Dictionary<string, string>? _cachedDialogues;
        private bool _isLoadingDialogues = false;

        public DialogueManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Показать диалог от NPC
        /// </summary>
        public void Speak(NPC npc, string text)
        {
            try
            {
                npc.facePlayer(Game1.player);
                var dialogue = new Dialogue(npc, null, text);
                
                // Инициализируем CurrentDialogue, если он null
                if (npc.CurrentDialogue == null)
                {
                    npc.CurrentDialogue = new Stack<Dialogue>();
                }
                
                npc.CurrentDialogue.Push(dialogue);
                Game1.drawDialogue(npc);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка при показе диалога: {ex}", LogLevel.Error);
                Game1.drawObjectDialogue(text);
            }
        }

        /// <summary>
        /// Показать эмоцию (облачко над головой NPC)
        /// </summary>
        /// <param name="npc">NPC</param>
        /// <param name="emoteId">ID эмоции</param>
        public void ShowEmote(NPC npc, int emoteId)
        {
            try
            {
                npc.doEmote(emoteId);
                _monitor.Log($"Показана эмоция {emoteId} для {npc.Name}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка при показе эмоции: {ex}", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Показать диалог с эмоцией
        /// </summary>
        /// <param name="npc">NPC</param>
        /// <param name="text">Текст диалога</param>
        /// <param name="emoteId">ID эмоции (опционально)</param>
        public void SpeakWithEmote(NPC npc, string text, int? emoteId = null)
        {
            if (emoteId.HasValue)
            {
                ShowEmote(npc, emoteId.Value);
            }
            Speak(npc, text);
        }

        /// <summary>
        /// Показать текстовое сообщение над головой NPC
        /// </summary>
        /// <param name="npc">NPC</param>
        /// <param name="text">Текст сообщения (короткий)</param>
        /// <param name="durationMs">Длительность в миллисекундах (по умолчанию 3000)</param>
        public void ShowTextAboveHead(NPC npc, string text, int durationMs = 3000)
        {
            try
            {
                npc.showTextAboveHead(text, duration: durationMs);
                _monitor.Log($"Показан текст над {npc.Name}: '{text}'", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка при показе текста над головой: {ex}", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Показать эмоцию с текстом над головой
        /// </summary>
        /// <param name="npc">NPC</param>
        /// <param name="emoteId">ID эмоции</param>
        /// <param name="text">Текст сообщения</param>
        /// <param name="textDurationMs">Длительность текста в миллисекундах</param>
        public void ShowEmoteWithText(NPC npc, int emoteId, string text, int textDurationMs = 3000)
        {
            ShowEmote(npc, emoteId);
            
            // Задержка перед показом текста (чтобы эмоция была видна)
            Game1.delayedActions.Add(new StardewValley.DelayedAction(500, () =>
            {
                ShowTextAboveHead(npc, text, textDurationMs);
            }));
        }

        /// <summary>
        /// Полная реакция: эмоция + текст + диалог
        /// </summary>
        /// <param name="npc">NPC</param>
        /// <param name="emoteId">ID эмоции</param>
        /// <param name="textAboveHead">Текст над головой</param>
        /// <param name="dialogue">Полный диалог (опционально)</param>
        public void ShowFullReaction(NPC npc, int emoteId, string textAboveHead, string? dialogue = null)
        {
            ShowEmoteWithText(npc, emoteId, textAboveHead);
            
            if (!string.IsNullOrEmpty(dialogue))
            {
                // Задержка перед диалогом (чтобы текст был прочитан)
                Game1.delayedActions.Add(new StardewValley.DelayedAction(2000, () =>
                {
                    Speak(npc, dialogue);
                }));
            }
        }

        /// <summary>Дней активности topicHarveyNeedsFirstTreatment после первой травмы.</summary>
        private const int HarveyNeedsFirstTreatmentTopicDays = 7;

        /// <summary>
        /// Добавить topicHarveyNeedsFirstTreatment, если игрок ещё не проходил FirstTreatment.
        /// </summary>
        public void TryAddHarveyNeedsFirstTreatmentTopic(string injuryBuffId)
        {
            if (string.IsNullOrEmpty(injuryBuffId))
                return;

            if (!InjurySets.HarveyTreatable.Contains(injuryBuffId))
            {
                _monitor.Log(
                    $"topicHarveyNeedsFirstTreatment пропущен: {injuryBuffId} не требует сцены FirstTreatment",
                    LogLevel.Debug);
                return;
            }

            if (HasSeenEvent(EventIds.FirstTreatment))
            {
                _monitor.Log(
                    "topicHarveyNeedsFirstTreatment пропущен: HarveyMod_FirstTreatment уже просмотрено",
                    LogLevel.Debug);
                return;
            }

            if (HasTopic(ConversationTopics.FirstTreatmentComplete))
            {
                _monitor.Log(
                    "topicHarveyNeedsFirstTreatment пропущен: topicFirstTreatmentComplete уже активен",
                    LogLevel.Debug);
                return;
            }

            if (HasTopic(ConversationTopics.HarveyNeedsFirstTreatment))
            {
                _monitor.Log("topicHarveyNeedsFirstTreatment уже активен", LogLevel.Debug);
                return;
            }

            AddTopic(ConversationTopics.HarveyNeedsFirstTreatment, HarveyNeedsFirstTreatmentTopicDays);
            _monitor.Log(
                $"topicHarveyNeedsFirstTreatment добавлен на {HarveyNeedsFirstTreatmentTopicDays} д. (травма {injuryBuffId})",
                LogLevel.Info);
        }

        /// <summary>
        /// Удалить topicHarveyNeedsFirstTreatment, если FirstTreatment уже пройден.
        /// </summary>
        public void ClearHarveyNeedsFirstTreatmentTopicIfObsolete(string reason)
        {
            if (!HasTopic(ConversationTopics.HarveyNeedsFirstTreatment))
                return;

            if (HasSeenEvent(EventIds.FirstTreatment) || HasTopic(ConversationTopics.FirstTreatmentComplete))
                ClearHarveyNeedsFirstTreatmentTopic(reason);
        }

        /// <summary>
        /// Удалить topicHarveyNeedsFirstTreatment.
        /// </summary>
        public void ClearHarveyNeedsFirstTreatmentTopic(string reason)
        {
            if (!HasTopic(ConversationTopics.HarveyNeedsFirstTreatment))
                return;

            RemoveTopic(ConversationTopics.HarveyNeedsFirstTreatment);
            _monitor.Log($"topicHarveyNeedsFirstTreatment удалён: {reason}", LogLevel.Info);
        }

        private const int DiagnosisCompleteTopicDays = 3;
        private const int TreatmentPlanMinFriendship = 750;

        /// <summary>
        /// Триггер HarveyMod_TreatmentPlanMeeting: после первого начала серьёзного/фазового лечения.
        /// </summary>
        public void TryAddDiagnosisCompleteTopic(string? injuryBuffId)
        {
            if (string.IsNullOrEmpty(injuryBuffId))
                return;

            if (!InjurySets.TreatmentPlanEligible.Contains(injuryBuffId))
                return;

            if (HasSeenEvent(EventIds.TreatmentPlanMeeting))
                return;

            if (HasTopic(ConversationTopics.DiagnosisComplete))
                return;

            if (GetHarveyFriendship() < TreatmentPlanMinFriendship && !HasSeenEvent(EventIds.FirstTreatment))
                return;

            AddTopic(ConversationTopics.DiagnosisComplete, DiagnosisCompleteTopicDays);
            _monitor.Log("[TreatmentPlan] topicDiagnosisComplete added", LogLevel.Info);
        }

        private static bool HasSeenEvent(string eventId)
        {
            return Game1.player?.eventsSeen?.Contains(eventId) == true;
        }

        /// <summary>
        /// Добавить разговорный топик
        /// </summary>
        public void AddTopic(string id, int days)
        {
            var dict = Game1.player?.activeDialogueEvents;
            if (dict is null) return;
            
            dict[id] = Math.Max(1, days);
            _monitor.Log($"Добавлен топик {id} на {days} дней", LogLevel.Debug);
        }

        /// <summary>
        /// Удалить разговорный топик
        /// </summary>
        public void RemoveTopic(string id)
        {
            Game1.player?.activeDialogueEvents?.Remove(id);
            _monitor.Log($"Удалён топик {id}", LogLevel.Debug);
        }

        /// <summary>
        /// Проверить наличие топика
        /// </summary>
        public bool HasTopic(string topic)
        {
            return Game1.player?.activeDialogueEvents?.ContainsKey(topic) == true;
        }

        /// <summary>
        /// Выбрать случайный диалог по префиксу из файла
        /// </summary>
        public string PickRandomDialogueByPrefix(string prefix, string defaultText = "…")
        {
            try
            {
                var dialogues = LoadDialoguesFromAsset();
                if (dialogues == null) return defaultText;

                var matching = dialogues
                    .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Value)
                    .ToList();

                if (matching.Count == 0)
                {
                    _monitor.Log($"Диалоги с префиксом '{prefix}' не найдены", LogLevel.Warn);
                    return defaultText;
                }

                return matching[Game1.random.Next(matching.Count)];
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка при загрузке диалога: {ex}", LogLevel.Error);
                return defaultText;
            }
        }

        /// <summary>
        /// Выбрать диалог лечения с учётом того, был ли уже разговор
        /// </summary>
        public string PickTreatmentDialogue(string injuryId, bool wasDiscussed, string defaultText = "…")
        {
            try
            {
                var dialogues = LoadDialoguesFromAsset();
                if (dialogues == null)
                {
                    _monitor.Log($"Диалоги не загружены, возвращаем дефолт: {defaultText}", LogLevel.Warn);
                    return defaultText;
                }

                // Определяем префикс в зависимости от того, был ли разговор
                string prefix = wasDiscussed ? $"Treat_{injuryId}_After" : $"Treat_{injuryId}_Before";
                
                _monitor.Log($"Ищем диалоги лечения с префиксом: {prefix}", LogLevel.Debug);
                
                var matching = dialogues
                    .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Value)
                    .ToList();

                if (matching.Count == 0)
                {
                    _monitor.Log($"⚠️ Диалоги лечения с префиксом '{prefix}' не найдены! Доступные ключи: {string.Join(", ", dialogues.Keys.Take(10))}", LogLevel.Warn);
                    return defaultText;
                }

                _monitor.Log($"Найдено {matching.Count} диалогов с префиксом '{prefix}'", LogLevel.Debug);
                string selected = matching[Game1.random.Next(matching.Count)];
                _monitor.Log($"Выбран диалог: {selected.Substring(0, Math.Min(50, selected.Length))}...", LogLevel.Debug);
                
                return selected;
            }
            catch (Exception ex)
            {
                _monitor.Log($"❌ Ошибка при загрузке диалога лечения: {ex}", LogLevel.Error);
                return defaultText;
            }
        }

        /// <summary>
        /// Загрузить диалоги из assets (с кэшированием и защитой от блокировок)
        /// </summary>
        private Dictionary<string, string>? LoadDialoguesFromAsset()
        {
            // Возвращаем кэшированные диалоги, если они уже загружены
            if (_cachedDialogues != null)
            {
                return _cachedDialogues;
            }

            // Защита от повторной загрузки
            if (_isLoadingDialogues)
            {
                _monitor.Log("⚠️ Диалоги уже загружаются, пропускаем повторную загрузку", LogLevel.Warn);
                return new Dictionary<string, string>();
            }

            try
            {
                _isLoadingDialogues = true;
                
                // Загружаем диалоги через Content Patcher (они добавлены в Characters/Dialogue/Harvey)
                var dialogues = Game1.content.Load<Dictionary<string, string>>("Characters/Dialogue/Harvey");
                _monitor.Log($"✅ Загружено {dialogues?.Count ?? 0} диалогов Харви через Content Patcher", LogLevel.Debug);
                
                // Фильтруем только диалоги лечения (с префиксами Treat_, Support_, Recovery_Complete_, PhaseTransition_, topic)
                var treatmentDialogues = dialogues?
                    .Where(kvp => kvp.Key.StartsWith("Treat_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("Support_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("Recovery_Complete_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("PhaseTransition_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("topic", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>();
                
                _monitor.Log($"🔍 Найдено {treatmentDialogues.Count} диалогов лечения из {dialogues?.Count ?? 0} общих диалогов", LogLevel.Debug);
                
                // Кэшируем результат
                _cachedDialogues = treatmentDialogues;
                return treatmentDialogues;
            }
            catch (Exception ex)
            {
                _monitor.Log($"❌ Ошибка загрузки диалогов лечения: {ex.Message}", LogLevel.Error);
                _monitor.Log($"Stack trace: {ex.StackTrace}", LogLevel.Error);
                return new Dictionary<string, string>();
            }
            finally
            {
                _isLoadingDialogues = false;
            }
        }

        /// <summary>
        /// Загрузить конкретный диалог Харви по ключу
        /// </summary>
        public string? TryLoadHarveyDialogue(string key)
        {
            try
            {
                var dict = Game1.content.Load<Dictionary<string, string>>("Characters/Dialogue/Harvey");
                if (dict != null && dict.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text))
                    return text;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка загрузки диалога '{key}': {ex}", LogLevel.Warn);
            }
            return null;
        }

        /// <summary>
        /// Проверить, встречается ли игрок с Харви или женат на нём
        /// </summary>
        public bool IsDatingOrMarriedToHarvey()
        {
            var harvey = Game1.getCharacterFromName("Harvey");
            if (harvey == null) return false;

            return Game1.player.friendshipData.TryGetValue("Harvey", out var friendship) 
                && (friendship.IsDating() || friendship.IsMarried());
        }

        /// <summary>
        /// Получить уровень сердец с NPC
        /// </summary>
        public int GetFriendshipHearts(string npcName)
        {
            if (Game1.player.friendshipData.TryGetValue(npcName, out var friendship))
            {
                return friendship.Points / 250; // 250 очков = 1 сердце
            }
            return 0;
        }

        /// <summary>
        /// Получить уровень дружбы с Харви (в очках)
        /// </summary>
        public int GetHarveyFriendship()
        {
            if (Game1.player.friendshipData.TryGetValue("Harvey", out var friendship))
            {
                return friendship.Points;
            }
            return 0;
        }
    }
}

