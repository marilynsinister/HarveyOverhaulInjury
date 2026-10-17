using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Дозирует «информационные» реплики Харви о лечении: после начала лечения разом появляются фаза,
    /// «раны заживают», план, режим и по топику на каждое назначение — и Харви выдаёт их подряд на каждый клик.
    /// В день звучит не больше <see cref="DailyBudget"/> таких реплик, по приоритету; остальные
    /// временно убираются из activeDialogueEvents и возвращаются утром.
    /// Топики с $action (начать лечение, сменить фазу, обработать осложнение), реакции на травму и нарушения
    /// не дозируются — без них не работает механика или они срочные.
    /// </summary>
    public sealed class MedicalTopicPacer
    {
        private const int DailyBudget = 2;
        private const int PollIntervalTicks = 30;
        private const string ParkedKey = "marilynsinister.HarveyOverhaul.Injury/ParkedMedicalTopics";
        private const string NpcName = "Harvey";

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;

        /// <summary>Топик → приоритет (меньше — раньше).</summary>
        private readonly Dictionary<string, int> _priority = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Топик → фаза, к которой он относится (для фазовых топиков).</summary>
        private readonly Dictionary<string, (string InjuryId, int Phase)> _phaseTopics = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Информационные топики, уже сказанные к моменту проверки (флаг Harvey_&lt;topic&gt;).</summary>
        private readonly HashSet<string> _saidSeen = new(StringComparer.OrdinalIgnoreCase);

        private int _saidToday;
        private bool _ready;

        public MedicalTopicPacer(IMonitor monitor, StateManager stateManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            BuildPriorities();
        }

        public void OnDayStarted()
        {
            _ready = true;
            _saidToday = 0;
            RestoreParked();
            SnapshotSaid();
        }

        public void OnReturnedToTitle()
        {
            _ready = false;
            _saidSeen.Clear();
        }

        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!_ready || e.Ticks % PollIntervalTicks != 0)
                return;

            if (!Context.IsWorldReady || Game1.CurrentEvent != null || Game1.activeClickableMenu != null)
                return;

            var active = Game1.player?.activeDialogueEvents;
            if (active == null)
                return;

            var unsaid = new List<string>();
            foreach (string topicId in active.Keys)
            {
                if (!_priority.ContainsKey(topicId))
                    continue;

                if (WasSaid(topicId))
                {
                    if (_saidSeen.Add(topicId))
                        _saidToday++;
                    continue;
                }

                unsaid.Add(topicId);
            }

            int slots = Math.Max(0, DailyBudget - _saidToday);
            if (unsaid.Count <= slots)
                return;

            var parked = LoadParked();
            foreach (string topicId in unsaid.OrderBy(t => _priority[t]).Skip(slots))
            {
                parked[topicId] = Math.Max(1, active[topicId]);
                active.Remove(topicId);
                _monitor.Log($"[TopicPacer] {topicId} отложен на завтра (сегодня уже {_saidToday}/{DailyBudget})", LogLevel.Debug);
            }

            SaveParked(parked);
        }

        private void RestoreParked()
        {
            var parked = LoadParked();
            if (parked.Count == 0)
                return;

            var active = Game1.player.activeDialogueEvents;
            foreach (var (topicId, days) in parked)
            {
                if (active.ContainsKey(topicId) || IsStalePhaseTopic(topicId))
                    continue;

                active[topicId] = days;
                _monitor.Log($"[TopicPacer] {topicId} возвращён ({days} дн.)", LogLevel.Debug);
            }

            SaveParked(new Dictionary<string, int>());
        }

        /// <summary>Пока топик ждал, травма могла перейти в другую фазу — реплика о прошлой фазе уже не нужна.</summary>
        private bool IsStalePhaseTopic(string topicId)
        {
            if (!_phaseTopics.TryGetValue(topicId, out var phase))
                return false;

            var state = _stateManager.GetDebuffState(phase.InjuryId);
            return state == null || (state.CurrentPhase > 0 && state.CurrentPhase != phase.Phase);
        }

        private void SnapshotSaid()
        {
            _saidSeen.Clear();
            if (Game1.player?.activeDialogueEvents is not { } active)
                return;

            foreach (string topicId in active.Keys)
            {
                if (_priority.ContainsKey(topicId) && WasSaid(topicId))
                    _saidSeen.Add(topicId);
            }
        }

        private static bool WasSaid(string topicId) =>
            Game1.player.mailReceived.Contains(NpcName + "_" + topicId);

        private void BuildPriorities()
        {
            Add(TreatmentPlanTopics.Given, 0);

            foreach (string buffId in InjurySets.HarveyTreatable)
            {
                int totalPhases = InjurySets.InferDefaultTotalPhases(buffId);
                for (int phase = 1; phase <= totalPhases; phase++)
                {
                    string topicId = TopicIds.GetPhaseTopicId(buffId, phase, totalPhases);
                    Add(topicId, 1);
                    _phaseTopics.TryAdd(topicId, (buffId, phase));
                }

                Add(TopicIds.GetTreatmentTopic(buffId), 2);
            }

            Add(ComplianceTopics.High, 3);
            Add(ComplianceTopics.Neutral, 3);
            Add(ComplianceTopics.Low, 3);
            Add(PrescriptionTopics.Checkup, 4);
            Add(PrescriptionTopics.Rest, 5);
            Add(PrescriptionTopics.NoMine, 5);
            Add(PrescriptionTopics.KeepDry, 5);
            Add(PrescriptionTopics.LightWork, 5);
            Add(PrescriptionTopics.Followed, 6);
            Add(ComplianceTopics.TrustedPatient, 6);
            Add(ConversationTopics.SelfCare, 7);
            Add(ConversationTopics.CleanBandage, 7);
            Add(ConversationTopics.WarmTea, 7);
            Add(ConversationTopics.SelfCarePraise, 7);
        }

        private void Add(string topicId, int priority) => _priority.TryAdd(topicId, priority);

        private static Dictionary<string, int> LoadParked()
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (Game1.player?.modData.TryGetValue(ParkedKey, out string? raw) != true || string.IsNullOrEmpty(raw))
                return result;

            foreach (string entry in raw.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                int sep = entry.LastIndexOf(':');
                if (sep > 0 && int.TryParse(entry[(sep + 1)..], out int days))
                    result[entry[..sep]] = days;
            }

            return result;
        }

        private static void SaveParked(Dictionary<string, int> parked)
        {
            if (Game1.player?.modData is not { } modData)
                return;

            if (parked.Count == 0)
                modData.Remove(ParkedKey);
            else
                modData[ParkedKey] = string.Join('|', parked.Select(kv => $"{kv.Key}:{kv.Value}"));
        }
    }
}
