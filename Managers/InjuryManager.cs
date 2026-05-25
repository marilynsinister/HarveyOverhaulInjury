using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление травмами игрока
    /// </summary>
    public class InjuryManager
    {
        private readonly IMonitor _monitor;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly HospitalizationManager _hospitalizationManager;
        private readonly ModConfig _config;
        private int _lastInjuryGameTime = -999;

        // Приоритет травм (от серьёзных к лёгким)
        private static readonly string[] InjuryPriority = new[]
        {
            "buffConcussion",
            "buffInfectedWound",
            "buffFracturedBone",
            "buffSurgicalWound",
            "buffShrapnelWounds",
            "buffBurnWounds",
            "buffDeepCuts",
            "buffTornMuscles",
            "buffBackStrain",
            "buffBruisedRibs",
            "buffSprainedAnkle",
            "buffBadlyHurt",
            "buffHurt"
        };

        public InjuryManager(
            IMonitor monitor, 
            StateManager stateManager, 
            BuffManager buffManager, 
            DialogueManager dialogueManager, 
            HospitalizationManager hospitalizationManager, 
            ModConfig config)
        {
            _monitor = monitor;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _hospitalizationManager = hospitalizationManager;
            _config = config;
        }

        /// <summary>
        /// Получить активную травму по приоритету (только базовый buff* на игроке).
        /// </summary>
        public string? GetActiveInjury()
        {
            foreach (var injury in InjuryPriority)
            {
                if (_buffManager.HasBuff(injury))
                    return injury;
            }
            return null;
        }

        /// <summary>
        /// Получить активную травму или её фазу по приоритету (для госпитализации и мед. пайплайна).
        /// </summary>
        public string? GetActiveInjuryOrPhaseByPriority()
        {
            foreach (var injuryId in InjuryPriority)
            {
                if (HasInjuryOrPhase(injuryId))
                    return injuryId;
            }
            return null;
        }

        /// <summary>
        /// Есть ли хотя бы одна Severe-травма (базовый buff или фазовый бафф лечения).
        /// </summary>
        public bool HasAnySevereInjuryOrPhase()
        {
            foreach (var injuryId in InjurySets.Severe)
            {
                if (HasInjuryOrPhase(injuryId))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Проверить наличие травмы или её фазы
        /// </summary>
        public bool HasInjuryOrPhase(string injuryId)
        {
            // Проверяем основную травму
            if (_buffManager.HasBuff(injuryId))
                return true;

            // Проверяем фазовые баффы через новую систему DebuffState
            var debuffState = _stateManager.GetDebuffState(injuryId);
            if (debuffState != null && debuffState.IsInTreatment)
            {
                string phaseBuffId = GetPhaseBuffId(injuryId, debuffState.CurrentPhase);
                if (_buffManager.HasBuff(phaseBuffId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Получить ID баффа фазы (с учетом реальных ID из JSON)
        /// </summary>
        public string GetPhaseBuffId(string injuryId, int phase)
        {
            // Маппинг травм к фазовым баффам (обновлённые ID после переименования)
            var phaseMapping = new Dictionary<string, Dictionary<int, string>>
            {
                ["buffDeepCuts"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_DeepCuts_Acute",
                    [2] = "HarveyMod_DeepCuts_Healing",
                    [3] = "HarveyMod_DeepCuts_Recovery"
                },
                ["buffFracturedBone"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_FracturedBone_Acute",
                    [2] = "HarveyMod_FracturedBone_Cast",
                    [3] = "HarveyMod_FracturedBone_Recovery"
                },
                ["buffConcussion"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_Concussion_Acute",
                    [2] = "HarveyMod_Concussion_Rest",
                    [3] = "HarveyMod_Concussion_Limited"
                },
                ["buffShrapnelWounds"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_Shrapnel_Surgery",
                    [2] = "HarveyMod_Shrapnel_Healing",
                    [3] = "HarveyMod_Shrapnel_Recovery"
                },
                ["buffTornMuscles"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_TornMuscles_Acute",
                    [2] = "HarveyMod_TornMuscles_Healing",
                    [3] = "HarveyMod_TornMuscles_Rehab"
                },
                ["buffSprainedAnkle"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_SprainedAnkle_Acute",
                    [2] = "HarveyMod_SprainedAnkle_Recovery",
                    [3] = "HarveyMod_SprainedAnkle_Recovery" // 2 фазы
                },
                ["buffBruisedRibs"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_BruisedRibs_Acute",
                    [2] = "HarveyMod_BruisedRibs_Healing",
                    [3] = "HarveyMod_BruisedRibs_Healing" // 2 фазы
                },
                ["buffBurnWounds"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_BurnWounds_Acute",
                    [2] = "HarveyMod_BurnWounds_Healing",
                    [3] = "HarveyMod_BurnWounds_Healing" // 2 фазы
                },
                ["buffInfectedWound"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_InfectedWound_Acute",
                    [2] = "HarveyMod_InfectedWound_Treatment",
                    [3] = "HarveyMod_InfectedWound_Treatment" // 2 фазы
                },
                ["buffBackStrain"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_BackStrain_Acute",
                    [2] = "HarveyMod_BackStrain_Recovery",
                    [3] = "HarveyMod_BackStrain_Recovery" // 2 фазы
                },
                ["buffCold"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_Cold_Acute",        // Острая фаза: температура, слабость
                    [2] = "HarveyMod_Cold_Recovery",     // Восстановление: остаточный кашель
                    [3] = "HarveyMod_Cold_Recovery"      // 2 фазы
                },
                ["buffBadlyHurt"] = new Dictionary<int, string>
                {
                    [1] = "HarveyMod_BadlyHurt_Acute",
                    [2] = "HarveyMod_BadlyHurt_Healing",
                    [3] = "HarveyMod_BadlyHurt_Recovery"
                }
            };

            if (phaseMapping.TryGetValue(injuryId, out var phases))
            {
                if (phases.TryGetValue(phase, out var phaseBuffId))
                {
                    return phaseBuffId;
                }
            }

            // Fallback для травм без фазовой системы
            return injuryId;
        }

        /// <summary>
        /// Собрать все травмы и осложнения
        /// </summary>
        public InjuryCollection CollectAllInjuries()
        {
            var result = new InjuryCollection
            {
                MainInjury = GetActiveInjuryOrPhaseByPriority()
            };

            //_monitor.Log($"📊 Основная травма = {result.MainInjury ?? "нет"}", LogLevel.Debug);

            // Проверяем осложнения
            CheckAndAddComplication(result, InjuryBuffs.DirtyWound, "DirtyWound");
            CheckAndAddComplication(result, InjuryBuffs.WetBandage, "WetBandage");
            CheckAndAddComplication(result, InjuryBuffs.WetStitches, "WetStitches");
            CheckAndAddComplication(result, InjuryBuffs.AllergicRash, "AllergicRash");
            CheckAndAddComplication(result, InjuryBuffs.PainFlare, "PainFlare");

            //_monitor.Log($"📊 Итого: основная травма={result.MainInjury ?? "нет"}, осложнений={result.Complications.Count}", LogLevel.Info);
            return result;
        }

        private void CheckAndAddComplication(InjuryCollection collection, string buffId, string name)
        {
            if (_buffManager.HasBuff(buffId))
                collection.Complications.Add(buffId);
        }

        /// <summary>
        /// Получить имя травмы для отображения
        /// </summary>
        public string GetInjuryName(string injuryId)
        {
            return injuryId switch
            {
                "buffHurt" => "Лёгкие травмы",
                "buffBadlyHurt" => "Тяжёлые травмы",
                "buffSprainedAnkle" => "Растяжение связок",
                "buffBruisedRibs" => "Ушибленные рёбра",
                "buffBackStrain" => "Растяжение спины",
                "buffDeepCuts" => "Глубокие порезы",
                "buffBurnWounds" => "Ожоги",
                "buffInfectedWound" => "Инфицированная рана",
                "buffTornMuscles" => "Разрыв мышц",
                "buffConcussion" => "Сотрясение мозга",
                "buffFracturedBone" => "Перелом",
                "buffShrapnelWounds" => "Осколочные ранения",
                "buffSurgicalWound" => "Хирургическая рана",
                _ => "Травма"
            };
        }

        /// <summary>
        /// Проверить, можно ли применить новую травму
        /// </summary>
        public bool CanApplyNewInjury(int lastInjuryTime, int currentTime, int cooldownMinutes = 5)
        {
            int elapsed = currentTime - lastInjuryTime;
            return elapsed >= cooldownMinutes;
        }

        /// <summary>
        /// Получить название топика для фазы
        /// </summary>
        public string GetPhaseTopicId(string injuryId, int phase) => TopicIds.GetPhaseTopicId(injuryId, phase);

        /// <summary>
        /// Удалить все фазовые баффы травмы
        /// </summary>
        public void RemoveAllPhaseBuffs(string injuryId)
        {
            var buffsToRemove = new List<string>
            {
                GetPhaseBuffId(injuryId, 1),
                GetPhaseBuffId(injuryId, 2),
                GetPhaseBuffId(injuryId, 3),
                injuryId
            };

            _buffManager.RemoveAllBuffs(buffsToRemove);
            _monitor.Log($"Удалены все фазовые баффы для {injuryId}", LogLevel.Debug);
        }

        // ============================================================================
        // МЕТОДЫ ПРИМЕНЕНИЯ КОНКРЕТНЫХ ТРАВМ
        // ============================================================================

        /// <summary>
        /// Проверить, можно ли применить новую травму (с учетом кулдауна)
        /// </summary>
        private bool CanApplyInjury()
        {
            int currentTime = Helpers.GameUtils.CurrentTimeInMinutes();
            return CanApplyNewInjury(_lastInjuryGameTime, currentTime, 30); // 30 минут кулдаун
        }

        /// <summary>
        /// Обновить время последней травмы
        /// </summary>
        private void UpdateLastInjuryTime()
        {
            _lastInjuryGameTime = Helpers.GameUtils.CurrentTimeInMinutes();
        }

        private void ApplyInjurySafe(string injuryId, Action applyFunc, string triggerConst)
        {
            try
            {
                bool storyOneShot = InjuryTriggerPolicy.IsStoryOneShotTrigger(triggerConst);

                if (storyOneShot && _stateManager.WasStoryTriggerApplied(triggerConst))
                {
                    _monitor.Log(
                        $"Story trigger {triggerConst} уже применён (AppliedTriggers), пропускаем {injuryId}",
                        LogLevel.Debug);
                    return;
                }

                int today = Helpers.GameUtils.Today();

                if (!storyOneShot && _stateManager.IsInjuryOnCooldown(injuryId, today))
                {
                    int? untilDay = _stateManager.GetInjuryCooldownUntilDay(injuryId);
                    _monitor.Log(
                        $"⏳ Injury cooldown для {injuryId}: до дня {untilDay}, сегодня {today}",
                        LogLevel.Debug);
                    return;
                }

                if (!_config.AllowSameInjuryWhileActive && HasInjuryOrPhase(injuryId))
                {
                    _monitor.Log($"⏳ Травма {injuryId} уже активна или лечится, повторное наложение пропущено", LogLevel.Debug);
                    return;
                }

                if (!CanApplyInjury())
                {
                    _monitor.Log($"Кулдаун травм активен, пропускаем {injuryId}", LogLevel.Debug);
                    return;
                }

                _monitor.Log($"Применяем травму {injuryId}", LogLevel.Info);
                applyFunc();
                _dialogueManager.TryAddHarveyNeedsFirstTreatmentTopic(injuryId);

                if (storyOneShot)
                {
                    _stateManager.MarkStoryTriggerApplied(triggerConst);
                }
                else
                {
                    int untilDay = today + Math.Max(0, _config.RepeatableInjuryCooldownDays);
                    _stateManager.SetInjuryCooldown(injuryId, untilDay);
                }

                UpdateLastInjuryTime();
            }
            catch (Exception ex)
            {
                _monitor.Log($"❌ Ошибка при применении травмы {injuryId}: {ex}", LogLevel.Error);
                // Важно: не MarkStoryTriggerApplied и не SetInjuryCooldown при ошибке.
            }
        }

        /// <summary>
        /// Вызывается при полном выздоровлении от repeatable-травмы.
        /// </summary>
        public void NotifyInjuryRecovered(string injuryId)
        {
            int today = Helpers.GameUtils.Today();
            _stateManager.ApplyResidualInjuryCooldownAfterRecovery(injuryId, today, residualDays: 2);
        }

        // === ЛЁГКИЕ ТРАВМЫ ===

        public void ApplyHurt()
        {
            _buffManager.AddBuff("buffHurt", -2);
            _dialogueManager.AddTopic(ConversationTopics.Hurt, 2);
            Game1.playSound("debuffHit");
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffHurt", currentDay, 2, 0, 0);
        }

        public void ApplyHurtSafe()
        {
            ApplyInjurySafe("buffHurt", ApplyHurt, Triggers.Hurt);
        }

        public void ApplyBadlyHurt()
        {
            _buffManager.AddBuff("buffBadlyHurt", -2);
            _dialogueManager.AddTopic(ConversationTopics.BadlyHurt, 4);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageCritical, 4);
            Game1.playSound("debuffHit");
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBadlyHurt", currentDay, 4, 0, 0);

            // Примечание: Прямая госпитализация УБРАНА!
            // Харви заметит травму через proximity detection и запустит госпитализацию
            // с информативным сообщением через CheckHarveyProximity() или HandleHospitalLogic()
        }

        public void ApplyBadlyHurtSafe()
        {
            ApplyInjurySafe("buffBadlyHurt", ApplyBadlyHurt, Triggers.BadlyHurt);
        }

        public void ApplyBadlyHurtFromMinePassOut()
        {
            if (_buffManager.HasBuff("buffBadlyHurt"))
            {
                _monitor.Log("[MineRescue] buffBadlyHurt уже активен, повторно не накладываем", LogLevel.Debug);
                return;
            }

            _monitor.Log("[MineRescue] Принудительно применяем buffBadlyHurt после смерти в шахте", LogLevel.Warn);
            ApplyBadlyHurt();
            _dialogueManager.TryAddHarveyNeedsFirstTreatmentTopic("buffBadlyHurt");
        }

        // === СРЕДНИЕ ТРАВМЫ ===

        public void ApplySprainedAnkle()
        {
            _buffManager.AddBuff("buffSprainedAnkle", -2);
            _dialogueManager.AddTopic(ConversationTopics.SprainedAnkle, 7);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (2 фазы: 3 + 4 = 7 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffSprainedAnkle", currentDay, 3, 4, 0);
        }

        public void ApplySprainedAnkleSafe()
        {
            ApplyInjurySafe("buffSprainedAnkle", ApplySprainedAnkle, Triggers.SprainedAnkle);
        }

        public void ApplyBruisedRibs()
        {
            _buffManager.AddBuff("buffBruisedRibs", -2);
            _dialogueManager.AddTopic(ConversationTopics.BruisedRibs, 9);
            Game1.playSound("debuffHit");
            
            // Инициализируем состояние дебаффа (2 фазы: 4 + 5 = 9 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBruisedRibs", currentDay, 4, 5, 0);
        }

        public void ApplyBruisedRibsSafe()
        {
            ApplyInjurySafe("buffBruisedRibs", ApplyBruisedRibs, Triggers.BruisedRibs);
        }

        public void ApplyBackStrain()
        {
            _buffManager.AddBuff("buffBackStrain", -2);
            _dialogueManager.AddTopic(ConversationTopics.BackStrain, 6);
            Game1.playSound("debuffHit");
            
            // Инициализируем состояние дебаффа (2 фазы: 2 + 4 = 6 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBackStrain", currentDay, 2, 4, 0);
        }

        public void ApplyBackStrainSafe()
        {
            ApplyInjurySafe("buffBackStrain", ApplyBackStrain, Triggers.BackStrain);
        }

        public void ApplyDeepCuts(string source = "generic")
        {
            // Применяем базовый бафф травмы (до лечения)
            _buffManager.AddBuff("buffDeepCuts", -2);
            _dialogueManager.AddTopic(ConversationTopics.DeepCuts, 7);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 2 + 3 + 2 = 7 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffDeepCuts", currentDay, 2, 3, 2);
        }

        public void ApplyDeepCutsSafe(string source = "generic")
        {
            string trigger = source == "combat" 
                ? Triggers.DeepCutsCombat 
                : Triggers.DeepCutsFarming;
            ApplyInjurySafe("buffDeepCuts", () => ApplyDeepCuts(source), trigger);
        }

        public void ApplyBurnWounds()
        {
            _buffManager.AddBuff("buffBurnWounds", -2);
            _dialogueManager.AddTopic(ConversationTopics.BurnWounds, 8);
            Game1.playSound("fireball");

            // Инициализируем состояние дебаффа (2 фазы: 3 + 5 = 8 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBurnWounds", currentDay, 3, 5, 0);

        }

        public void ApplyBurnWoundsSafe()
        {
            ApplyInjurySafe("buffBurnWounds", ApplyBurnWounds, Triggers.BurnWounds);
        }

        public void ApplyInfectedWound()
        {
            _buffManager.AddBuff("buffInfectedWound", -2);
            _dialogueManager.AddTopic(ConversationTopics.InfectedWound, 6);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (2 фазы: 2 + 4 = 6 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffInfectedWound", currentDay, 2, 4, 0);

        }

        public void ApplyInfectedWoundSafe()
        {
            ApplyInjurySafe("buffInfectedWound", ApplyInfectedWound, Triggers.InfectedWound);
        }

        // === ТЯЖЁЛЫЕ ТРАВМЫ (3 фазы) ===

        public void ApplyTornMuscles()
        {
            _buffManager.AddBuff("buffTornMuscles", -2);
            _dialogueManager.AddTopic(ConversationTopics.TornMuscles, 11);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageSevere, 11);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 3 + 5 + 3 = 11 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffTornMuscles", currentDay, 3, 5, 3);
        }

        public void ApplyTornMusclesSafe()
        {
            ApplyInjurySafe("buffTornMuscles", ApplyTornMuscles, Triggers.TornMuscles);
        }

        public void ApplyConcussion()
        {
            _buffManager.AddBuff("buffConcussion", -2);
            _dialogueManager.AddTopic(ConversationTopics.Concussion, 9);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageSevere, 9);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 2 + 4 + 3 = 9 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffConcussion", currentDay, 2, 4, 3);

            if (_config.ForceHospitalization)
            {
                var harvey = HarveyHelper.FindHarvey(Game1.currentLocation);
                _hospitalizationManager.StartForcedHospitalization("buffConcussion", harvey);
            }
        }

        public void ApplyConcussionSafe()
        {
            ApplyInjurySafe("buffConcussion", ApplyConcussion, Triggers.Concussion);
        }

        public void ApplyFracturedBone()
        {
            _buffManager.AddBuff("buffFracturedBone", -2);
            _dialogueManager.AddTopic(ConversationTopics.FracturedBone, 18);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageCritical, 18);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 4 + 10 + 4 = 18 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffFracturedBone", currentDay, 4, 10, 4);

        }

        public void ApplyFracturedBoneSafe()
        {
            ApplyInjurySafe("buffFracturedBone", ApplyFracturedBone, Triggers.FracturedBone);
        }

        public void ApplyShrapnelWounds()
        {
            _buffManager.AddBuff("buffShrapnelWounds", -2);
            _dialogueManager.AddTopic(ConversationTopics.ShrapnelWounds, 11);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageCritical, 11);
            _dialogueManager.AddTopic(ConversationTopics.PostOperativeCare, 7);
            Game1.playSound("stoneCrack");

            // Инициализируем состояние дебаффа (3 фазы: 3 + 5 + 3 = 11 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffShrapnelWounds", currentDay, 3, 5, 3);

        }

        public void ApplyShrapnelWoundsSafe()
        {
            ApplyInjurySafe("buffShrapnelWounds", ApplyShrapnelWounds, Triggers.ShrapnelWounds);
        }

        // === СПЕЦИАЛЬНЫЕ ТРАВМЫ ===

        public void ApplySurgicalWound()
        {
            _buffManager.AddBuff("buffSurgicalWound", -2);
            _dialogueManager.AddTopic(ConversationTopics.SurgicalWound, 7);
            _dialogueManager.AddTopic(ConversationTopics.PostOperativeCare, 7);
            Game1.playSound("debuffHit");
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffSurgicalWound", currentDay, 7, 0, 0);
        }

        public void ApplySurgicalWoundSafe()
        {
            ApplyInjurySafe("buffSurgicalWound", ApplySurgicalWound, Triggers.SurgicalWound);
        }

        /// <summary>
        /// Применить простуду (2 фазы: острая + восстановление)
        /// </summary>
        public void ApplyCold()
        {
            _monitor.Log("🤧 Применяем простуду (Cold)", LogLevel.Info);
            
            // Применяем бафф простуды
            _buffManager.AddBuff(InjuryBuffs.Cold, -2);
            _dialogueManager.AddTopic(ConversationTopics.Cold, 4);
            Game1.playSound("debuffHit");
            
            // Инициализируем состояние дебаффа (2 фазы: 2 + 2 = 4 дня)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState(InjuryBuffs.Cold, currentDay, 2, 2, 0);
            
            Game1.addHUDMessage(new HUDMessage("Простуда! Температура, слабость...", HUDMessage.error_type));
        }

        public void ApplyColdSafe()
        {
            ApplyInjurySafe(InjuryBuffs.Cold, ApplyCold, Triggers.Cold);
        }
    }
}

