using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>
    /// Read-only диагностика соответствия C# topic/mail ID загруженным игровым asset'ам.
    /// </summary>
    internal static class ContentAuditRunner
    {
        private static readonly string[] HarveyDialogueAssetPaths =
        {
            "Characters/Dialogue/Harvey",
            "Data/Characters/Dialogue/Harvey",
        };

        public static void Run(
            IModHelper helper,
            IMonitor monitor,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas,
            (string BuffId, string TopicId)[] knownComplications)
        {
            monitor.Log("=== injury_audit_content: проверка topic/mail ID (только чтение) ===", LogLevel.Info);

            if (!TryLoadMail(helper, monitor, out var mailKeys))
                mailKeys = null;

            if (!TryLoadHarveyDialogues(helper, monitor, out var dialogueKeys, out var dialogueAssetPath))
                dialogueKeys = null;

            int missingMail = 0;
            int missingDialogue = 0;

            missingMail += AuditMailIds(monitor, mailKeys);
            missingDialogue += AuditConversationTopics(monitor, dialogueKeys, dialogueAssetPath);
            missingDialogue += AuditKnownTraumas(monitor, dialogueKeys, dialogueAssetPath, knownTraumas);
            missingDialogue += AuditKnownComplications(monitor, dialogueKeys, dialogueAssetPath, knownComplications);
            missingDialogue += AuditDynamicTopics(monitor, dialogueKeys, dialogueAssetPath, knownTraumas);

            if (mailKeys == null && dialogueKeys == null)
            {
                monitor.Log(
                    "injury_audit_content: не удалось загрузить ни Data/Mail, ни диалоги Харви — проверка ключей пропущена.",
                    LogLevel.Warn);
                return;
            }

            monitor.Log(
                $"=== injury_audit_content: итого отсутствует mail={missingMail}, dialogue/topic={missingDialogue} ===",
                missingMail + missingDialogue > 0 ? LogLevel.Warn : LogLevel.Info);
        }

        private static bool TryLoadMail(IModHelper helper, IMonitor monitor, out HashSet<string>? mailKeys)
        {
            mailKeys = null;
            try
            {
                var mail = helper.GameContent.Load<Dictionary<string, string>>("Data/Mail");
                mailKeys = new HashSet<string>(mail.Keys, StringComparer.Ordinal);
                monitor.Log($"Data/Mail: загружено {mailKeys.Count} ключей.", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log($"Data/Mail: не удалось загрузить — {ex.Message}", LogLevel.Warn);
                return false;
            }
        }

        private static bool TryLoadHarveyDialogues(
            IModHelper helper,
            IMonitor monitor,
            out HashSet<string>? dialogueKeys,
            out string? loadedAssetPath)
        {
            dialogueKeys = null;
            loadedAssetPath = null;

            foreach (string assetPath in HarveyDialogueAssetPaths)
            {
                try
                {
                    var dialogues = helper.GameContent.Load<Dictionary<string, string>>(assetPath);
                    dialogueKeys = new HashSet<string>(dialogues.Keys, StringComparer.Ordinal);
                    loadedAssetPath = assetPath;
                    monitor.Log(
                        $"{assetPath}: загружено {dialogueKeys.Count} ключей диалогов Харви.",
                        LogLevel.Info);
                    return true;
                }
                catch (Exception ex)
                {
                    monitor.Log($"{assetPath}: не удалось загрузить — {ex.Message}", LogLevel.Debug);
                }
            }

            monitor.Log(
                "Диалоги Харви: не удалось загрузить ни один asset (" +
                string.Join(", ", HarveyDialogueAssetPaths) +
                "). Проверка topic keys пропущена.",
                LogLevel.Warn);
            return false;
        }

        private static int AuditMailIds(IMonitor monitor, HashSet<string>? mailKeys)
        {
            monitor.Log("--- MailIds (C# константы → Data/Mail) ---", LogLevel.Info);
            if (mailKeys == null)
            {
                monitor.Log("  (Data/Mail недоступен)", LogLevel.Warn);
                return 0;
            }

            int missing = 0;
            foreach (var (name, id) in GetStringConstants(typeof(MailIds)))
            {
                if (mailKeys.Contains(id))
                    monitor.Log($"  OK  {name} = {id}", LogLevel.Trace);
                else
                {
                    monitor.Log($"  MISSING mail  {name} = {id}", LogLevel.Warn);
                    missing++;
                }
            }

            if (missing == 0)
                monitor.Log("  Все MailIds найдены в Data/Mail.", LogLevel.Info);

            return missing;
        }

        private static int AuditConversationTopics(
            IMonitor monitor,
            HashSet<string>? dialogueKeys,
            string? dialogueAssetPath)
        {
            monitor.Log("--- ConversationTopics (C# константы → Harvey dialogue) ---", LogLevel.Info);
            if (dialogueKeys == null)
            {
                monitor.Log("  (диалоги Харви недоступны)", LogLevel.Warn);
                return 0;
            }

            int missing = 0;
            foreach (var (name, id) in GetStringConstants(typeof(ConversationTopics)))
            {
                if (dialogueKeys.Contains(id))
                    monitor.Log($"  OK  {name} = {id}", LogLevel.Trace);
                else
                {
                    monitor.Log($"  MISSING dialogue  {name} = {id}  (asset: {dialogueAssetPath})", LogLevel.Warn);
                    missing++;
                }
            }

            if (missing == 0)
                monitor.Log("  Все ConversationTopics найдены в диалогах Харви.", LogLevel.Info);

            return missing;
        }

        private static int AuditKnownTraumas(
            IMonitor monitor,
            HashSet<string>? dialogueKeys,
            string? dialogueAssetPath,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas)
        {
            monitor.Log("--- KnownTraumas (базовый topic → Harvey dialogue) ---", LogLevel.Info);
            if (dialogueKeys == null)
            {
                monitor.Log("  (диалоги Харви недоступны)", LogLevel.Warn);
                return 0;
            }

            int missing = 0;
            foreach (var trauma in knownTraumas)
            {
                string expected = TopicIds.GetInjuryTopic(trauma.BuffId);
                if (!string.Equals(trauma.TopicId, expected, StringComparison.Ordinal))
                {
                    monitor.Log(
                        $"  MISMATCH  {trauma.BuffId}: KnownTraumas.TopicId={trauma.TopicId}, TopicIds.GetInjuryTopic={expected}",
                        LogLevel.Warn);
                }

                if (dialogueKeys.Contains(trauma.TopicId))
                    monitor.Log($"  OK  {trauma.BuffId} → {trauma.TopicId}", LogLevel.Trace);
                else
                {
                    monitor.Log(
                        $"  MISSING dialogue  {trauma.BuffId} → {trauma.TopicId}  (asset: {dialogueAssetPath})",
                        LogLevel.Warn);
                    missing++;
                }
            }

            if (missing == 0)
                monitor.Log("  Все базовые topic KnownTraumas найдены.", LogLevel.Info);

            return missing;
        }

        private static int AuditKnownComplications(
            IMonitor monitor,
            HashSet<string>? dialogueKeys,
            string? dialogueAssetPath,
            (string BuffId, string TopicId)[] knownComplications)
        {
            monitor.Log("--- KnownComplications (topic → Harvey dialogue) ---", LogLevel.Info);
            if (dialogueKeys == null)
            {
                monitor.Log("  (диалоги Харви недоступны)", LogLevel.Warn);
                return 0;
            }

            int missing = 0;
            foreach (var comp in knownComplications)
            {
                string expected = TopicIds.GetComplicationTopic(comp.BuffId);
                if (!string.Equals(comp.TopicId, expected, StringComparison.Ordinal))
                {
                    monitor.Log(
                        $"  MISMATCH  {comp.BuffId}: KnownComplications.TopicId={comp.TopicId}, TopicIds.GetComplicationTopic={expected}",
                        LogLevel.Warn);
                }

                if (dialogueKeys.Contains(comp.TopicId))
                    monitor.Log($"  OK  {comp.BuffId} → {comp.TopicId}", LogLevel.Trace);
                else
                {
                    monitor.Log(
                        $"  MISSING dialogue  {comp.BuffId} → {comp.TopicId}  (asset: {dialogueAssetPath})",
                        LogLevel.Warn);
                    missing++;
                }
            }

            if (missing == 0)
                monitor.Log("  Все topic KnownComplications найдены.", LogLevel.Info);

            return missing;
        }

        private static int AuditDynamicTopics(
            IMonitor monitor,
            HashSet<string>? dialogueKeys,
            string? dialogueAssetPath,
            (string BuffId, string TopicId, int P1, int P2, int P3)[] knownTraumas)
        {
            monitor.Log("--- Dynamic topic IDs (TopicIds → Harvey dialogue) ---", LogLevel.Info);
            if (dialogueKeys == null)
            {
                monitor.Log("  (диалоги Харви недоступны)", LogLevel.Warn);
                return 0;
            }

            int missing = 0;
            foreach (var trauma in knownTraumas)
            {
                string curedId = TopicIds.GetCuredTopic(trauma.BuffId);
                missing += LogDialoguePresence(monitor, dialogueKeys, dialogueAssetPath, trauma.BuffId, "Cured", curedId);

                if (!TreatmentManager.PhasedInjuries.Contains(trauma.BuffId))
                    continue;

                string treatmentId = TopicIds.GetTreatmentTopic(trauma.BuffId);
                missing += LogDialoguePresence(monitor, dialogueKeys, dialogueAssetPath, trauma.BuffId, "Treatment", treatmentId);

                missing += LogDialoguePresence(
                    monitor, dialogueKeys, dialogueAssetPath, trauma.BuffId, "PhaseAcute",
                    TopicIds.GetPhaseTopicId(trauma.BuffId, 1));
                missing += LogDialoguePresence(
                    monitor, dialogueKeys, dialogueAssetPath, trauma.BuffId, "PhaseHealing",
                    TopicIds.GetPhaseTopicId(trauma.BuffId, 2));
                missing += LogDialoguePresence(
                    monitor, dialogueKeys, dialogueAssetPath, trauma.BuffId, "PhaseRecovery",
                    TopicIds.GetPhaseTopicId(trauma.BuffId, 3));
            }

            if (missing == 0)
                monitor.Log("  Все dynamic topic IDs найдены.", LogLevel.Info);

            return missing;
        }

        private static int LogDialoguePresence(
            IMonitor monitor,
            HashSet<string> dialogueKeys,
            string? dialogueAssetPath,
            string buffId,
            string kind,
            string topicId)
        {
            if (dialogueKeys.Contains(topicId))
            {
                monitor.Log($"  OK  {buffId} [{kind}] → {topicId}", LogLevel.Trace);
                return 0;
            }

            monitor.Log(
                $"  MISSING dialogue  {buffId} [{kind}] → {topicId}  (asset: {dialogueAssetPath})",
                LogLevel.Warn);
            return 1;
        }

        private static IEnumerable<(string Name, string Value)> GetStringConstants(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f is { IsLiteral: true, FieldType: var ft } && ft == typeof(string))
                .Select(f => (f.Name, (string)f.GetRawConstantValue()!))
                .OrderBy(x => x.Name, StringComparer.Ordinal);
        }
    }
}
