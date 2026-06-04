using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>Стадия отношений с Харви для выбора реплик лечения (CP: Prefix_Stage_*).</summary>
    public enum HarveyRelationshipStage
    {
        Stranger,
        Acquaintance,
        Friend,
        Close,
        Dating,
        Married,
    }

    /// <summary>
    /// Управление диалогами и разговорными топиками
    /// </summary>
    public class DialogueManager
    {
        private readonly IMonitor _monitor;
        private Dictionary<string, string>? _cachedDialogues;
        private bool _isLoadingDialogues = false;
        private Dictionary<string, string>? _cachedProximityDialogues;
        private bool _isLoadingProximityDialogues = false;

        private const string ProximityDialogueAssetPath = "Data/HarveyOverhaul/HarveyProximityInjuryDialogue";
        public const string ProximityDialogueFallback = "Покажись мне в клинике.";

        /// <summary>CP: TreatmentStart_{InjuryName}_* (первый старт лечения по клику).</summary>
        public const string FirstTreatmentStartFallback =
            "Стой. Дай посмотреть... Я обработаю травму и назначу лечение. Сегодня без геройства, хорошо?$u";

        /// <summary>CP: PhaseTransition_{InjuryName}_{NextPhase}* (смена фазы, не первичный диагноз).</summary>
        public const string PhaseTransitionFallback =
            "Хорошо, заживление идёт как нужно. Переведём лечение на следующий этап.$u";

        /// <summary>CP: RecoveryComplete_{InjuryName}_* (полное выздоровление, не старт лечения).</summary>
        public const string RecoveryCompleteFallback =
            "Вот теперь я доволен. Лечение завершено, но пару дней всё равно береги себя.$h";

        /// <summary>CP: ComplicationTreatment_{Name}_* (клик — лечение осложнения без диагноза основной травмы).</summary>
        public const string ComplicationTreatmentFallback =
            "Так, это осложнение. Я обработаю всё заново, но дальше ты строго соблюдаешь уход.$a";

        /// <summary>Префикс ключей диалога первого лечения: buffDeepCuts → TreatmentStart_DeepCuts_</summary>
        public static string GetTreatmentStartDialoguePrefix(string buffId)
        {
            string injuryName = buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase);
            return $"TreatmentStart_{injuryName}_";
        }

        /// <summary>Префикс смены фазы: buffDeepCuts, 2 → PhaseTransition_DeepCuts_2</summary>
        public static string GetPhaseTransitionDialoguePrefix(string buffId, int nextPhase)
        {
            string injuryName = buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase);
            return $"PhaseTransition_{injuryName}_{nextPhase}";
        }

        /// <summary>
        /// Клик при ReadyForNextPhase: только PhaseTransition_* (не Treat_* / TreatmentStart_*).
        /// </summary>
        public string PickPhaseTransitionDialogue(string buffId, int nextPhase)
        {
            string prefix = GetPhaseTransitionDialoguePrefix(buffId, nextPhase);
            string? line = TryPickHarveyDialogueByPrefix(prefix);
            if (!string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"[PhaseTransition] {buffId} → фаза {nextPhase}, prefix={prefix}", LogLevel.Debug);
                return line;
            }

            _monitor.Log(
                $"[PhaseTransition] реплики не найдены ({prefix}), fallback",
                LogLevel.Warn);
            return PhaseTransitionFallback;
        }

        /// <summary>Префикс финального выздоровления: buffDeepCuts → RecoveryComplete_DeepCuts_</summary>
        public static string GetRecoveryCompleteDialoguePrefix(string buffId)
        {
            string injuryName = buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase);
            return $"RecoveryComplete_{injuryName}_";
        }

        /// <summary>Legacy CP: Recovery_Complete_{InjuryName}</summary>
        public static string GetLegacyRecoveryCompleteDialoguePrefix(string buffId)
        {
            string injuryName = buffId.Replace("buff", "", StringComparison.OrdinalIgnoreCase);
            return $"Recovery_Complete_{injuryName}";
        }

        /// <summary>
        /// Клик при ReadyForRecovery: только RecoveryComplete_* / Recovery_Complete_* (не Treat_* / topic*).
        /// </summary>
        public string PickRecoveryCompleteDialogue(string buffId)
        {
            string canonical = GetRecoveryCompleteDialoguePrefix(buffId);
            string? line = TryPickHarveyDialogueByPrefix(canonical);
            if (!string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"[RecoveryComplete] {buffId}, prefix={canonical}", LogLevel.Debug);
                return line;
            }

            string legacy = GetLegacyRecoveryCompleteDialoguePrefix(buffId);
            line = TryPickHarveyDialogueByPrefix(legacy);
            if (!string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"[RecoveryComplete] {buffId}, legacy prefix={legacy}", LogLevel.Debug);
                return line;
            }

            _monitor.Log(
                $"[RecoveryComplete] реплики не найдены ({canonical} / {legacy}), fallback",
                LogLevel.Warn);
            return RecoveryCompleteFallback;
        }

        /// <summary>HarveyMod_WetBandage → ComplicationTreatment_WetBandage_</summary>
        public static string GetComplicationTreatmentDialoguePrefix(string complicationBuffId)
        {
            string name = complicationBuffId.Replace("HarveyMod_", "", StringComparison.OrdinalIgnoreCase);
            return $"ComplicationTreatment_{name}_";
        }

        /// <summary>Клик TreatComplications: только осложнение (не Treat_* / TreatmentStart_*).</summary>
        public string PickComplicationTreatmentDialogue(string complicationBuffId)
        {
            string prefix = GetComplicationTreatmentDialoguePrefix(complicationBuffId);
            string? line = TryPickHarveyDialogueByPrefix(prefix);
            if (!string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"[ComplicationTreatment] {complicationBuffId}, prefix={prefix}", LogLevel.Debug);
                return line;
            }

            string legacy = $"Proximity_{complicationBuffId.Replace("HarveyMod_", "", StringComparison.OrdinalIgnoreCase)}";
            line = TryPickHarveyDialogueByPrefix(legacy);
            if (!string.IsNullOrWhiteSpace(line))
            {
                _monitor.Log($"[ComplicationTreatment] {complicationBuffId}, legacy prefix={legacy}", LogLevel.Debug);
                return line;
            }

            _monitor.Log(
                $"[ComplicationTreatment] реплики не найдены ({prefix}), fallback",
                LogLevel.Warn);
            return ComplicationTreatmentFallback;
        }

        private static readonly string[] FormalAddressMarkers =
        {
            "Вы", "Вам", "Вас", "Ваш", "держите", "садитесь", "приходите", "не забывайте",
        };

        private static readonly HarveyRelationshipStage[] AllRelationshipStages =
            (HarveyRelationshipStage[])Enum.GetValues(typeof(HarveyRelationshipStage));

        /// <summary>
        /// Стадия отношений: Married/Dating → иначе по сердцам (8+/4+/2+).
        /// </summary>
        public HarveyRelationshipStage GetHarveyRelationshipStage()
        {
            var friendship = Game1.player?.friendshipData;
            if (friendship == null || !friendship.TryGetValue("Harvey", out var data))
                return HarveyRelationshipStage.Stranger;

            if (data.IsMarried())
                return HarveyRelationshipStage.Married;
            if (data.IsDating())
                return HarveyRelationshipStage.Dating;

            int hearts = data.Points / 250;
            if (hearts >= 8)
                return HarveyRelationshipStage.Close;
            if (hearts >= 4)
                return HarveyRelationshipStage.Friend;
            if (hearts >= 2)
                return HarveyRelationshipStage.Acquaintance;
            return HarveyRelationshipStage.Stranger;
        }

        private static string BuildStagedPrefix(string prefix, HarveyRelationshipStage stage)
        {
            string stageToken = $"{stage}_";
            return prefix.EndsWith("_", StringComparison.Ordinal)
                ? prefix + stageToken
                : prefix + "_" + stageToken;
        }

        private static bool IsStagedDialogueKey(string key, string prefix, HarveyRelationshipStage stage) =>
            key.StartsWith(BuildStagedPrefix(prefix, stage), StringComparison.OrdinalIgnoreCase);

        private static bool IsUnstagedDialogueKey(string key, string prefix)
        {
            if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (var stage in AllRelationshipStages)
            {
                if (IsStagedDialogueKey(key, prefix, stage))
                    return false;
            }

            return true;
        }

        private static bool ContainsFormalAddress(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (string marker in FormalAddressMarkers)
            {
                if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool RequiresInformalTone(HarveyRelationshipStage stage) =>
            stage is HarveyRelationshipStage.Dating or HarveyRelationshipStage.Married;

        private static HarveyRelationshipStage[] GetRelationshipFallbackChain(HarveyRelationshipStage stage) =>
            stage switch
            {
                HarveyRelationshipStage.Married =>
                    new[]
                    {
                        HarveyRelationshipStage.Married,
                        HarveyRelationshipStage.Dating,
                        HarveyRelationshipStage.Close,
                        HarveyRelationshipStage.Friend,
                    },
                HarveyRelationshipStage.Dating =>
                    new[]
                    {
                        HarveyRelationshipStage.Dating,
                        HarveyRelationshipStage.Close,
                        HarveyRelationshipStage.Friend,
                    },
                HarveyRelationshipStage.Close =>
                    new[]
                    {
                        HarveyRelationshipStage.Close,
                        HarveyRelationshipStage.Friend,
                        HarveyRelationshipStage.Acquaintance,
                        HarveyRelationshipStage.Stranger,
                    },
                HarveyRelationshipStage.Friend =>
                    new[]
                    {
                        HarveyRelationshipStage.Friend,
                        HarveyRelationshipStage.Acquaintance,
                        HarveyRelationshipStage.Stranger,
                    },
                HarveyRelationshipStage.Acquaintance =>
                    new[]
                    {
                        HarveyRelationshipStage.Acquaintance,
                        HarveyRelationshipStage.Stranger,
                    },
                _ => new[] { HarveyRelationshipStage.Stranger },
            };

        private string? PickRandomLineByPrefixWithRelationship(
            Dictionary<string, string> dialogues,
            string prefix,
            HarveyRelationshipStage stage)
        {
            foreach (var fallbackStage in GetRelationshipFallbackChain(stage))
            {
                var staged = dialogues
                    .Where(kvp => IsStagedDialogueKey(kvp.Key, prefix, fallbackStage)
                        && !string.IsNullOrWhiteSpace(kvp.Value))
                    .Select(kvp => kvp.Value)
                    .ToList();

                if (staged.Count > 0)
                    return staged[Game1.random.Next(staged.Count)];
            }

            var unstaged = dialogues
                .Where(kvp => IsUnstagedDialogueKey(kvp.Key, prefix) && !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => kvp.Value)
                .ToList();

            if (RequiresInformalTone(stage))
                unstaged = unstaged.Where(line => !ContainsFormalAddress(line)).ToList();

            if (unstaged.Count == 0)
                return null;

            return unstaged[Game1.random.Next(unstaged.Count)];
        }

        /// <summary>Случайная строка из полного Characters/Dialogue/Harvey по префиксу ключа.</summary>
        private string? TryPickHarveyDialogueByPrefix(string prefix)
        {
            try
            {
                var dict = Game1.content.Load<Dictionary<string, string>>("Characters/Dialogue/Harvey");
                if (dict == null || dict.Count == 0)
                    return null;

                var stage = GetHarveyRelationshipStage();
                return PickRandomLineByPrefixWithRelationship(dict, prefix, stage);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка подбора диалога по префиксу '{prefix}': {ex}", LogLevel.Warn);
                return null;
            }
        }

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
            }
        }

        /// <summary>
        /// Реплика Харви с портретом и разбором $h/$a/#$b#/@. NPC не обязан быть на текущей карте.
        /// </summary>
        public void SpeakHarvey(string text, NPC? preferred = null)
        {
            NPC? harvey = preferred ?? HarveyHelper.GetHarvey();
            if (harvey == null)
            {
                _monitor.Log($"Harvey dialogue skipped (NPC missing): {text}", LogLevel.Warn);
                return;
            }

            Speak(harvey, text);
        }

        /// <summary>
        /// Отложенная реплика Харви (после warp/меню).
        /// </summary>
        public void SpeakHarveyDelayed(string text, int delayMs = 500, NPC? preferred = null)
        {
            Game1.delayedActions.Add(new DelayedAction(delayMs, () => SpeakHarvey(text, preferred)));
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
        /// Снять topic нелеченной травмы (topicDeepCuts, topicCold, …). Фазовые, topicTreatment* и осложнения не трогает.
        /// </summary>
        public void ClearUntreatedInjuryTopic(string buffId, string reason)
        {
            if (string.IsNullOrEmpty(buffId))
                return;

            string topicId = TopicIds.GetInjuryTopic(buffId);
            if (!HasTopic(topicId))
                return;

            RemoveTopic(topicId);
            _monitor.Log($"[Topic] снят нелеченный топик {topicId} ({buffId}): {reason}", LogLevel.Info);
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
                if (dialogues == null || dialogues.Count == 0)
                    return defaultText;

                var stage = GetHarveyRelationshipStage();
                string? line = PickRandomLineByPrefixWithRelationship(dialogues, prefix, stage);
                if (!string.IsNullOrWhiteSpace(line))
                    return line;

                _monitor.Log(
                    $"Диалоги с префиксом '{prefix}' не найдены (стадия {stage}), fallback",
                    LogLevel.Warn);
                return defaultText;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка при загрузке диалога: {ex}", LogLevel.Error);
                return defaultText;
            }
        }

        /// <summary>
        /// Выбрать случайную proximity-реплику (облачко) по префиксу из CP-ассета.
        /// </summary>
        public string PickRandomProximityLineByPrefix(string prefix, string defaultText = ProximityDialogueFallback) =>
            PickRandomProximityLineByPrefixes(new[] { prefix }, defaultText);

        /// <summary>
        /// Выбрать proximity-реплику, перебирая префиксы от точного к запасным.
        /// </summary>
        public string PickRandomProximityLineByPrefixes(IEnumerable<string> prefixes, string defaultText = ProximityDialogueFallback)
        {
            try
            {
                var dialogues = LoadProximityDialoguesFromAsset();
                if (dialogues == null || dialogues.Count == 0)
                    return defaultText;

                var tried = new List<string>();

                foreach (var prefix in prefixes)
                {
                    if (string.IsNullOrWhiteSpace(prefix))
                        continue;

                    if (tried.Exists(p => string.Equals(p, prefix, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    tried.Add(prefix);

                    var matching = dialogues
                        .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .Select(kvp => kvp.Value)
                        .ToList();

                    if (matching.Count > 0)
                        return matching[Game1.random.Next(matching.Count)];
                }

                if (tried.Count > 0)
                {
                    _monitor.Log(
                        $"Proximity-реплики не найдены для префиксов: {string.Join(" → ", tried)}",
                        LogLevel.Warn);
                }

                return defaultText;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка при загрузке proximity-реплики: {ex}", LogLevel.Error);
                return defaultText;
            }
        }

        /// <summary>
        /// Загрузить proximity-реплики из CP (с кэшированием).
        /// </summary>
        private Dictionary<string, string>? LoadProximityDialoguesFromAsset()
        {
            if (_cachedProximityDialogues != null)
                return _cachedProximityDialogues;

            if (_isLoadingProximityDialogues)
            {
                _monitor.Log("Proximity-реплики уже загружаются, пропускаем повторную загрузку", LogLevel.Warn);
                return new Dictionary<string, string>();
            }

            try
            {
                _isLoadingProximityDialogues = true;

                var dialogues = Game1.content.Load<Dictionary<string, string>>(ProximityDialogueAssetPath);
                _cachedProximityDialogues = dialogues ?? new Dictionary<string, string>();
                _monitor.Log(
                    $"Загружено {_cachedProximityDialogues.Count} proximity-реплик из {ProximityDialogueAssetPath}",
                    LogLevel.Debug);
                return _cachedProximityDialogues;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка загрузки proximity-реплик: {ex.Message}", LogLevel.Error);
                return new Dictionary<string, string>();
            }
            finally
            {
                _isLoadingProximityDialogues = false;
            }
        }

        /// <summary>
        /// Первый старт лечения по клику: TreatmentStart_{InjuryName}_* из Characters/Dialogue/Harvey.
        /// </summary>
        public string PickFirstTreatmentStartDialogue(string buffId)
        {
            string prefix = GetTreatmentStartDialoguePrefix(buffId);
            string line = PickRandomDialogueByPrefix(prefix, FirstTreatmentStartFallback);
            _monitor.Log(
                $"[TreatmentStart] buff={buffId} prefix={prefix} selected={(line == FirstTreatmentStartFallback ? "fallback" : "cp")}",
                LogLevel.Debug);
            return line;
        }

        /// <summary>
        /// Выбрать диалог лечения с учётом того, был ли уже разговор
        /// </summary>
        public string PickTreatmentDialogue(string injuryId, bool wasDiscussed, string defaultText = "…")
        {
            try
            {
                var dialogues = LoadDialoguesFromAsset();
                if (dialogues == null || dialogues.Count == 0)
                {
                    _monitor.Log($"Диалоги не загружены, возвращаем дефолт: {defaultText}", LogLevel.Warn);
                    return defaultText;
                }

                string prefix = wasDiscussed ? $"Treat_{injuryId}_After" : $"Treat_{injuryId}_Before";
                var stage = GetHarveyRelationshipStage();

                _monitor.Log($"Ищем диалоги лечения: prefix={prefix}, stage={stage}", LogLevel.Debug);

                string? selected = PickRandomLineByPrefixWithRelationship(dialogues, prefix, stage);
                if (!string.IsNullOrWhiteSpace(selected))
                {
                    _monitor.Log(
                        $"Выбран диалог лечения ({stage}): {selected.Substring(0, Math.Min(50, selected.Length))}...",
                        LogLevel.Debug);
                    return selected;
                }

                _monitor.Log(
                    $"Диалоги лечения с префиксом '{prefix}' не найдены (стадия {stage}), fallback",
                    LogLevel.Warn);
                return defaultText;
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
                
                // Фильтруем только диалоги лечения (Treat_, TreatmentStart_, RecoveryComplete_, Recovery_Complete_, …)
                var treatmentDialogues = dialogues?
                    .Where(kvp => kvp.Key.StartsWith("Treat_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("TreatmentStart_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("Support_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("RecoveryComplete_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("Recovery_Complete_", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.StartsWith("ComplicationTreatment_", StringComparison.OrdinalIgnoreCase) ||
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

