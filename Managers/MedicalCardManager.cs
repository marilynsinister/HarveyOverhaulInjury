using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Медицинская карта: история выздоровлений, шрамы, достижения и «Закалка» после тяжёлых травм.
    /// Показывается во вкладке «Травмы» панели Харви.
    /// </summary>
    public sealed class MedicalCardManager
    {
        private const int ResilienceDays = 14;
        private const int MilestoneTopicDays = 3;
        private const int VeteranRecoveries = 5;
        private const int HistoryLinesShown = 5;

        private sealed record Achievement(string Id, string Title, string Hint);

        private static readonly Achievement[] Achievements =
        {
            new(MedicalAchievements.FirstRecovery, "Первое выздоровление", "полностью вылечить любую травму"),
            new(MedicalAchievements.ModelPatient, "Образцовый пациент", "выздороветь быстрее плана, соблюдая режим"),
            new(MedicalAchievements.ThroughThorns, "Через тернии", "пережить тяжёлую травму"),
            new(MedicalAchievements.Veteran, "Бывалый пациент", $"выздороветь {VeteranRecoveries} раз"),
            new(MedicalAchievements.SelfReliant, "Сама себе медсестра", "обработать лёгкую травму аптечкой"),
        };

        private static readonly Dictionary<string, string> Scars = new(StringComparer.OrdinalIgnoreCase)
        {
            ["buffFracturedBone"] = "Кость срослась, но ноет к перемене погоды.",
            ["buffShrapnelWounds"] = "Россыпь мелких шрамов от осколков.",
            ["buffBurnWounds"] = "Светлое пятно на месте ожога.",
            ["buffDeepCuts"] = "Тонкий белый шрам от пореза.",
            ["buffConcussion"] = "Иногда кружится голова, если резко встать.",
            ["buffSurgicalWound"] = "Аккуратный шов — работа Харви.",
            ["buffInfectedWound"] = "Неровный рубец на месте воспаления.",
            ["buffTornMuscles"] = "Мышца помнит разрыв, если перетрудиться.",
        };

        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly InjuryManager _injuryManager;

        public MedicalCardManager(
            IMonitor monitor,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _injuryManager = injuryManager;
        }

        private InjuryState State => _stateManager.State;

        /// <summary>Подписчик TreatmentManager.InjuryRecovered.</summary>
        public void OnInjuryRecovered(string injuryId, DebuffState? recoveredState)
        {
            // Осложнения (HarveyMod_*) не попадают в карту — только сами травмы.
            if (!injuryId.StartsWith("buff", StringComparison.OrdinalIgnoreCase))
                return;

            int today = GameUtils.Today();
            var record = new MedicalRecord
            {
                InjuryId = injuryId,
                StartDay = recoveredState?.InjuryStartDay ?? today,
                RecoveredDay = today,
                RegimenAdjustmentDays = recoveredState?.TotalRegimenAdjustment ?? 0,
            };
            State.MedicalHistory.Add(record);
            _monitor.Log($"[MedicalCard] Выздоровление записано: {injuryId} (дни {record.StartDay}→{today}, режим {record.RegimenAdjustmentDays:+0;-0;0})", LogLevel.Info);

            Unlock(MedicalAchievements.FirstRecovery);
            if (record.RegimenAdjustmentDays < 0)
                Unlock(MedicalAchievements.ModelPatient);
            if (State.MedicalHistory.Count >= VeteranRecoveries)
                Unlock(MedicalAchievements.Veteran);

            if (InjurySets.Severe.Contains(injuryId))
            {
                Unlock(MedicalAchievements.ThroughThorns);
                State.ResilienceUntilDay = today + ResilienceDays;
                ApplyResilienceBuff();
                Game1.addHUDMessage(new HUDMessage("Закалка: пережитое сделало тебя крепче.", HUDMessage.achievement_type));
            }

            _stateManager.Save();
        }

        public void OnDayStarted()
        {
            if (State.ResilienceUntilDay >= GameUtils.Today())
                ApplyResilienceBuff();
            else if (_buffManager.HasBuff(MedicalCardBuffs.Resilience))
                _buffManager.RemoveBuff(MedicalCardBuffs.Resilience);
        }

        public void Unlock(string achievementId)
        {
            if (State.MedicalAchievements.Contains(achievementId))
                return;

            var achievement = Achievements.FirstOrDefault(a => a.Id == achievementId);
            if (achievement == null)
                return;

            State.MedicalAchievements.Add(achievementId);
            _dialogueManager.AddTopic(ConversationTopics.MedicalMilestone, MilestoneTopicDays);
            Game1.playSound("achievement");
            Game1.addHUDMessage(new HUDMessage($"Медицинская карта: «{achievement.Title}»", HUDMessage.achievement_type));
            _stateManager.Save();
            _monitor.Log($"[MedicalCard] Достижение: {achievementId}", LogLevel.Info);
        }

        /// <summary>Разделы «Медицинская карта» для вкладки «Травмы».</summary>
        public List<HarveyPanelSectionDto> BuildPanelSections(int basePriority)
        {
            var sections = new List<HarveyPanelSectionDto>();
            var history = State.MedicalHistory;

            var historyBody = new StringBuilder();
            if (history.Count == 0)
            {
                historyBody.Append("Пока чисто. Харви надеется, что так и останется.");
            }
            else
            {
                historyBody.AppendLine($"Вылечено травм: {history.Count}");
                foreach (var record in history.AsEnumerable().Reverse().Take(HistoryLinesShown))
                {
                    int days = Math.Max(1, record.RecoveredDay - record.StartDay);
                    string pace = record.RegimenAdjustmentDays < 0 ? " · быстрее плана" : "";
                    historyBody.AppendLine($"{_injuryManager.GetInjuryName(record.InjuryId)}: {FormatDay(record.StartDay)} – {FormatDay(record.RecoveredDay)} ({days} дн.){pace}");
                }
            }

            if (State.ResilienceUntilDay >= GameUtils.Today())
                historyBody.AppendLine($"Закалка активна до {FormatDay(State.ResilienceUntilDay)}.");

            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Медицинская карта",
                Body = historyBody.ToString().TrimEnd(),
                Priority = basePriority,
                Severity = HarveyPanelSeverity.Info,
            });

            var scars = history
                .Where(r => Scars.ContainsKey(r.InjuryId))
                .GroupBy(r => r.InjuryId, StringComparer.OrdinalIgnoreCase)
                .Select(g => $"{_injuryManager.GetInjuryName(g.Key)}: {Scars[g.Key]}")
                .ToList();
            if (scars.Count > 0)
            {
                sections.Add(new HarveyPanelSectionDto
                {
                    Title = "Шрамы и память тела",
                    Body = string.Join(Environment.NewLine, scars),
                    Priority = basePriority + 1,
                    Severity = HarveyPanelSeverity.Info,
                });
            }

            var achievementsBody = new StringBuilder();
            foreach (var achievement in Achievements)
            {
                bool unlocked = State.MedicalAchievements.Contains(achievement.Id);
                achievementsBody.AppendLine(unlocked
                    ? $"[+] {achievement.Title}"
                    : $"[ ] {achievement.Title} — {achievement.Hint}");
            }

            sections.Add(new HarveyPanelSectionDto
            {
                Title = "Достижения",
                Body = achievementsBody.ToString().TrimEnd(),
                Status = $"{State.MedicalAchievements.Count}/{Achievements.Length}",
                Priority = basePriority + 2,
                Severity = State.MedicalAchievements.Count == Achievements.Length ? HarveyPanelSeverity.Success : HarveyPanelSeverity.Info,
            });

            return sections;
        }

        private void ApplyResilienceBuff()
        {
            if (!_buffManager.HasBuff(MedicalCardBuffs.Resilience) && _buffManager.BuffExists(MedicalCardBuffs.Resilience))
                _buffManager.AddBuff(MedicalCardBuffs.Resilience, -2);
        }

        private static string FormatDay(int daysPlayed) =>
            SDate.FromDaysSinceStart(Math.Max(1, daysPlayed)).ToLocaleString(withYear: true);
    }
}
