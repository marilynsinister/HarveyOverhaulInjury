using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>Результат проверки согласованности основной травмы с баффами игрока.</summary>
    public sealed class MainInjuryValidation
    {
        public bool Valid { get; init; }
        public string? Reason { get; init; }
        public string? MainInjuryId { get; init; }
        public bool BaseBuffActive { get; init; }
        public string? CureBuffId { get; init; }
        public bool CureBuffActive { get; init; }
        public string? PhaseBuffId { get; init; }
        public bool PhaseBuffActive { get; init; }
        public bool TreatmentStarted { get; init; }
    }

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
        private ComplicationManager? _complicationManager;
        private int _lastInjuryGameTime = -999;
        private int _lastMainInjuryBlockedFeedbackHour = -1;

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

        public void SetComplicationManager(ComplicationManager complicationManager)
        {
            _complicationManager = complicationManager;
        }

        /// <summary>
        /// Числовой приоритет основной травмы (выше = серьёнее). Шаг между соседними рангами — 20.
        /// </summary>
        public int GetInjuryPriorityPublic(string injuryId)
        {
            for (int i = 0; i < InjurySets.MainInjuryPriorityOrder.Length; i++)
            {
                if (string.Equals(InjurySets.MainInjuryPriorityOrder[i], injuryId, StringComparison.OrdinalIgnoreCase))
                    return (InjurySets.MainInjuryPriorityOrder.Length - i) * InjuryPriorityStep;
            }

            return 0;
        }

        /// <summary>
        /// Попытаться применить основную травму с учётом правила «только одна основная».
        /// </summary>
        public bool TryApplyMainInjury(
            string newInjuryId,
            Action applyAction,
            bool allowUpgrade = true,
            bool forceReplace = false,
            bool suppressBlockedFeedback = false)
        {
            string? currentMain = GetCurrentMainInjuryId();

            if (string.IsNullOrEmpty(currentMain))
            {
                applyAction();
                _stateManager.SetMainInjury(newInjuryId);
                return true;
            }

            if (string.Equals(currentMain, newInjuryId, StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log($"[MainInjury] Травма уже активна: {newInjuryId}", LogLevel.Debug);
                return false;
            }

            if (forceReplace || (allowUpgrade && CanUpgradeMainInjury(currentMain, newInjuryId)))
            {
                string oldInjuryId = currentMain;
                RemoveAllPhaseBuffs(oldInjuryId);
                RemoveMainInjuryTopics(oldInjuryId);
                _stateManager.RemoveDebuffState(oldInjuryId);

                applyAction();
                _stateManager.SetMainInjury(newInjuryId, force: forceReplace);

                if (string.Equals(newInjuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase))
                    _complicationManager?.ClearWoundRelatedComplicationsAfterInfection();

                _monitor.Log(
                    $"[MainInjury] Основная травма заменена: {oldInjuryId} -> {newInjuryId}",
                    LogLevel.Info);
                return true;
            }

            _monitor.Log(
                $"[MainInjury] Новая травма заблокирована, уже есть основная: {currentMain}, попытка: {newInjuryId}",
                LogLevel.Debug);
            if (!suppressBlockedFeedback)
                HandleMainInjuryBlocked(currentMain, newInjuryId);
            return false;
        }

        private void HandleMainInjuryBlocked(string currentMain, string newInjuryId)
        {
            int currentPriority = GetInjuryPriorityPublic(currentMain);
            int newPriority = GetInjuryPriorityPublic(newInjuryId);
            bool isHeavierAttempt = newPriority > currentPriority;

            if (isHeavierAttempt && !ShouldSkipPainFlareForMineDirtyWound(currentMain))
                TryApplyPainFlareInsteadOfInjury(newInjuryId);

            TryShowMainInjuryBlockedHud(newInjuryId, isHeavierAttempt);
        }

        /// <summary>
        /// В шахте при DirtyInMines-травме осложнение — DirtyWound, не PainFlare.
        /// </summary>
        private static bool ShouldSkipPainFlareForMineDirtyWound(string currentMainInjuryId)
        {
            if (!InjurySets.DirtyInMines.Contains(currentMainInjuryId))
                return false;

            return IsInsideMineOrVolcano(Game1.player?.currentLocation);
        }

        private static bool IsInsideMineOrVolcano(GameLocation? location)
        {
            if (location == null)
                return false;

            return location is MineShaft or VolcanoDungeon
                || string.Equals(location.NameOrUniqueName, "Mine", StringComparison.OrdinalIgnoreCase)
                || string.Equals(location.NameOrUniqueName, "UndergroundMine", StringComparison.OrdinalIgnoreCase)
                || string.Equals(location.NameOrUniqueName, "VolcanoDungeon", StringComparison.OrdinalIgnoreCase);
        }

        private void TryApplyPainFlareInsteadOfInjury(string attemptedInjuryId)
        {
            string? currentMain = GetActiveInjury();
            if (!InjurySets.IsPainFlareEligibleMain(currentMain))
            {
                _monitor.Log(
                    $"[MainInjury] PainFlare пропущен: main={currentMain ?? "none"} не pain-sensitive (попытка: {attemptedInjuryId})",
                    LogLevel.Debug);
                return;
            }

            if (_complicationManager?.TryApplyPainFlareFromBlockedInjury(attemptedInjuryId) == true)
            {
                _monitor.Log(
                    $"[MainInjury] Вместо новой травмы добавлено обострение боли (попытка: {attemptedInjuryId})",
                    LogLevel.Info);
            }
        }

        private void TryShowMainInjuryBlockedHud(string newInjuryId, bool isHeavierAttempt)
        {
            int currentHour = Game1.timeOfDay / 100;
            if (_lastMainInjuryBlockedFeedbackHour == currentHour)
                return;

            _lastMainInjuryBlockedFeedbackHour = currentHour;

            bool isSevereCase = isHeavierAttempt
                || InjurySets.Severe.Contains(newInjuryId)
                || InjurySets.Critical.Contains(newInjuryId);

            string message = isSevereCase
                ? "Твоё состояние ухудшилось. Лучше показаться Харви."
                : "Старая травма дала о себе знать...";

            Game1.addHUDMessage(new HUDMessage(
                message,
                isSevereCase ? HUDMessage.error_type : HUDMessage.health_type));
        }

        private const int InjuryPriorityStep = 20;

        private bool CanUpgradeMainInjury(string currentInjuryId, string newInjuryId)
        {
            if (!InjurySets.IsMainInjuryUpgradePair(currentInjuryId, newInjuryId))
                return false;

            DebuffState? currentState = _stateManager.GetDebuffState(currentInjuryId);
            if (currentState?.TreatmentStarted == true)
            {
                _monitor.Log(
                    $"[MainInjury] Upgrade blocked: current main injury {currentInjuryId} is already in treatment",
                    LogLevel.Info);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Основная травма из ActiveDebuffs по приоритету (без осложнений).
        /// </summary>
        public string? ResolveMainInjuryFromActiveDebuffs()
        {
            var candidates = _stateManager.State.ActiveDebuffs.Keys
                .Where(id => IsBaseMainInjuryId(id))
                .ToList();

            return InjurySets.SelectMainInjuryByPriority(candidates);
        }

        /// <summary>
        /// Текущий MainInjuryId: синхронизирует сохранённое значение с ActiveDebuffs.
        /// </summary>
        public string? GetCurrentMainInjuryId()
        {
            if (_stateManager.QaSuppressMainInjuryAutoSync)
                return _stateManager.GetMainInjuryId();

            string? resolved = ResolveMainInjuryFromActiveDebuffs();
            if (resolved == null)
                return _stateManager.GetMainInjuryId();

            if (!_stateManager.IsMainInjury(resolved))
                _stateManager.SetMainInjury(resolved, force: true);

            return resolved;
        }

        /// <summary>
        /// QA: ensure player buffs match ActiveDebuffs / ActiveComplications before daily checks or infection tests.
        /// </summary>
        public void SyncActiveBuffsFromStateForQa()
        {
            EnsureActiveTreatmentBuffs();
        }

        /// <summary>
        /// Восстановить ожидаемый лечебный/фазовый бафф для одной записи ActiveDebuffs.
        /// </summary>
        public int EnsureTreatmentBuffForInjury(string buffId)
        {
            if (!_stateManager.State.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
                return 0;

            if (InjurySets.KnownComplicationBuffIds.Contains(buffId))
                return 0;

            int restored = 0;

            if (!debuffState.TreatmentStarted)
            {
                if (!_buffManager.HasBuff(buffId))
                {
                    _buffManager.AddBuff(buffId, -2);
                    if (_buffManager.HasBuff(buffId))
                        restored++;
                    else
                        LogBuffSyncFailure(buffId, buffId);
                }

                return restored;
            }

            if (TreatmentManager.IsSimpleTreatmentInjury(buffId))
            {
                string? cureBuffId = GetExpectedCureBuffId(buffId);
                if (cureBuffId != null && !_buffManager.HasBuff(cureBuffId))
                {
                    _buffManager.AddBuff(cureBuffId, -2);
                    if (_buffManager.HasBuff(cureBuffId))
                        restored++;
                    else
                        LogBuffSyncFailure(buffId, cureBuffId);
                }

                if (_buffManager.HasBuff(buffId))
                    _buffManager.RemoveBuff(buffId);

                return restored;
            }

            if (!debuffState.IsPhasedInjury || debuffState.CurrentPhase <= 0)
                return restored;

            string expectedPhaseBuffId = GetPhaseBuffId(buffId, debuffState.CurrentPhase);
            if (!string.IsNullOrEmpty(expectedPhaseBuffId) && !_buffManager.HasBuff(expectedPhaseBuffId))
            {
                _buffManager.AddBuff(expectedPhaseBuffId, -2);
                if (_buffManager.HasBuff(expectedPhaseBuffId))
                {
                    restored++;
                    _monitor.Log(
                        $"[BuffSync] Восстановлен фазовый бафф {expectedPhaseBuffId} ({buffId}, фаза {debuffState.CurrentPhase})",
                        LogLevel.Info);
                }
                else
                    LogBuffSyncFailure(buffId, expectedPhaseBuffId);
            }

            if (_buffManager.HasBuff(buffId))
                _buffManager.RemoveBuff(buffId);

            for (int phase = 1; phase <= 3; phase++)
            {
                if (phase == debuffState.CurrentPhase)
                    continue;

                string stalePhaseBuffId = GetPhaseBuffId(buffId, phase);
                if (!string.IsNullOrEmpty(stalePhaseBuffId) && _buffManager.HasBuff(stalePhaseBuffId))
                    _buffManager.RemoveBuff(stalePhaseBuffId);
            }

            return restored;
        }

        /// <summary>
        /// Есть ли на игроке бафф, соответствующий текущему этапу лечения травмы.
        /// </summary>
        public bool HasExpectedTreatmentBuff(string buffId)
        {
            if (!_stateManager.State.ActiveDebuffs.TryGetValue(buffId, out var debuffState))
                return false;

            if (!debuffState.TreatmentStarted)
                return _buffManager.HasBuff(buffId);

            if (TreatmentManager.IsSimpleTreatmentInjury(buffId))
                return IsCureBuffActive(buffId);

            if (!debuffState.IsPhasedInjury || debuffState.CurrentPhase <= 0)
                return false;

            string expectedPhaseBuffId = GetPhaseBuffId(buffId, debuffState.CurrentPhase);
            return !string.IsNullOrEmpty(expectedPhaseBuffId) && _buffManager.HasBuff(expectedPhaseBuffId);
        }

        private void LogBuffSyncFailure(string injuryId, string buffId)
        {
            string exists = _buffManager.BuffExists(buffId) ? "exists in Data/Buffs" : "MISSING in Data/Buffs";
            _monitor.Log(
                $"[BuffSync] Не удалось наложить {buffId} для {injuryId} ({exists})",
                LogLevel.Error);
        }

        /// <summary>
        /// Восстановить ожидаемые лечебные/фазовые баффы по DebuffState (после сна, reload, сбоя DialogueBox).
        /// </summary>
        public int EnsureActiveTreatmentBuffs()
        {
            int restored = 0;

            foreach (var (buffId, _) in _stateManager.State.ActiveDebuffs)
                restored += EnsureTreatmentBuffForInjury(buffId);

            foreach (string compId in _stateManager.State.ActiveComplications.Keys.ToList())
            {
                if (string.Equals(compId, InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase)
                    && !InjurySets.IsPainFlareEligibleMain(_stateManager.GetMainInjuryId()))
                {
                    _complicationManager?.RemoveComplicationForQa(compId);
                    continue;
                }

                if (!_buffManager.HasBuff(compId))
                {
                    _buffManager.AddBuff(compId, -2);
                    restored++;
                }
            }

            return restored;
        }

        public string? GetExpectedCureBuffId(string injuryId)
        {
            if (TreatmentManager.CureByInjury.TryGetValue(injuryId, out string? cureBuff))
                return cureBuff;

            return null;
        }

        public bool IsCureBuffActive(string injuryId)
        {
            string? cureBuffId = GetExpectedCureBuffId(injuryId);
            if (cureBuffId == null)
                return false;

            if (_buffManager.HasBuff(cureBuffId))
                return true;

            return string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase)
                && _buffManager.HasBuff(CureBuffs.BadlyHurtOutpatientCare);
        }

        /// <summary>
        /// Проверить согласованность основной травмы с DebuffState и активными баффами.
        /// </summary>
        public MainInjuryValidation ValidateMainInjury(string? mainInjuryId)
        {
            if (string.IsNullOrEmpty(mainInjuryId))
                return new MainInjuryValidation { Valid = false, Reason = "no main injury id" };

            if (InjurySets.KnownComplicationBuffIds.Contains(mainInjuryId))
            {
                return new MainInjuryValidation
                {
                    Valid = false,
                    Reason = "main id is a complication",
                    MainInjuryId = mainInjuryId,
                };
            }

            DebuffState? debuffState = _stateManager.GetDebuffState(mainInjuryId);
            if (debuffState == null)
            {
                return new MainInjuryValidation
                {
                    Valid = false,
                    Reason = "no DebuffState in ActiveDebuffs",
                    MainInjuryId = mainInjuryId,
                };
            }

            bool baseBuffActive = _buffManager.HasBuff(mainInjuryId);
            string? cureBuffId = GetExpectedCureBuffId(mainInjuryId);
            bool cureBuffActive = IsCureBuffActive(mainInjuryId);
            string? phaseBuffId = debuffState.TreatmentStarted && debuffState.IsPhasedInjury && debuffState.CurrentPhase > 0
                ? GetPhaseBuffId(mainInjuryId, debuffState.CurrentPhase)
                : null;
            bool phaseBuffActive = !string.IsNullOrEmpty(phaseBuffId) && _buffManager.HasBuff(phaseBuffId);

            if (!debuffState.TreatmentStarted)
            {
                if (!baseBuffActive)
                {
                    return new MainInjuryValidation
                    {
                        Valid = false,
                        Reason = "TreatmentStarted=false but base buff missing",
                        MainInjuryId = mainInjuryId,
                        BaseBuffActive = baseBuffActive,
                        CureBuffId = cureBuffId,
                        CureBuffActive = cureBuffActive,
                        PhaseBuffId = phaseBuffId,
                        PhaseBuffActive = phaseBuffActive,
                        TreatmentStarted = false,
                    };
                }

                return BuildValidMainInjuryValidation(
                    mainInjuryId, debuffState, baseBuffActive, cureBuffId, cureBuffActive, phaseBuffId, phaseBuffActive);
            }

            if (TreatmentManager.IsSimpleTreatmentInjury(mainInjuryId))
            {
                if (!cureBuffActive)
                {
                    return new MainInjuryValidation
                    {
                        Valid = false,
                        Reason = $"TreatmentStarted=true but cure buff {cureBuffId} missing",
                        MainInjuryId = mainInjuryId,
                        BaseBuffActive = baseBuffActive,
                        CureBuffId = cureBuffId,
                        CureBuffActive = false,
                        PhaseBuffId = phaseBuffId,
                        PhaseBuffActive = phaseBuffActive,
                        TreatmentStarted = true,
                    };
                }

                return BuildValidMainInjuryValidation(
                    mainInjuryId, debuffState, baseBuffActive, cureBuffId, cureBuffActive, phaseBuffId, phaseBuffActive);
            }

            if (debuffState.IsPhasedInjury)
            {
                if (debuffState.CurrentPhase <= 0)
                {
                    return new MainInjuryValidation
                    {
                        Valid = false,
                        Reason = "TreatmentStarted=true but CurrentPhase=0 for phased injury",
                        MainInjuryId = mainInjuryId,
                        BaseBuffActive = baseBuffActive,
                        CureBuffId = cureBuffId,
                        CureBuffActive = cureBuffActive,
                        PhaseBuffId = phaseBuffId,
                        PhaseBuffActive = phaseBuffActive,
                        TreatmentStarted = true,
                    };
                }

                if (!phaseBuffActive)
                {
                    return new MainInjuryValidation
                    {
                        Valid = false,
                        Reason = $"TreatmentStarted=true but phase buff {phaseBuffId} missing",
                        MainInjuryId = mainInjuryId,
                        BaseBuffActive = baseBuffActive,
                        CureBuffId = cureBuffId,
                        CureBuffActive = cureBuffActive,
                        PhaseBuffId = phaseBuffId,
                        PhaseBuffActive = false,
                        TreatmentStarted = true,
                    };
                }

                return BuildValidMainInjuryValidation(
                    mainInjuryId, debuffState, baseBuffActive, cureBuffId, cureBuffActive, phaseBuffId, phaseBuffActive);
            }

            return BuildValidMainInjuryValidation(
                mainInjuryId, debuffState, baseBuffActive, cureBuffId, cureBuffActive, phaseBuffId, phaseBuffActive);
        }

        public MainInjuryValidation GetMainInjuryDebugInfo()
        {
            string? mainId = GetCurrentMainInjuryId() ?? ResolveMainInjuryFromActiveDebuffs();
            return ValidateMainInjury(mainId);
        }

        private static MainInjuryValidation BuildValidMainInjuryValidation(
            string mainInjuryId,
            DebuffState debuffState,
            bool baseBuffActive,
            string? cureBuffId,
            bool cureBuffActive,
            string? phaseBuffId,
            bool phaseBuffActive) =>
            new()
            {
                Valid = true,
                MainInjuryId = mainInjuryId,
                BaseBuffActive = baseBuffActive,
                CureBuffId = cureBuffId,
                CureBuffActive = cureBuffActive,
                PhaseBuffId = phaseBuffId,
                PhaseBuffActive = phaseBuffActive,
                TreatmentStarted = debuffState.TreatmentStarted,
            };

        private void RemoveMainInjuryTopics(string injuryId)
        {
            _dialogueManager.RemoveTopic(TopicIds.GetInjuryTopic(injuryId));
            _dialogueManager.RemoveTopic(TopicIds.GetTreatmentTopic(injuryId));
            for (int phase = 1; phase <= 3; phase++)
                _dialogueManager.RemoveTopic(GetPhaseTopicId(injuryId, phase));

            switch (injuryId)
            {
                case "buffBadlyHurt":
                case "buffFracturedBone":
                case "buffShrapnelWounds":
                    _dialogueManager.RemoveTopic(ConversationTopics.HealthDamageCritical);
                    break;
                case "buffTornMuscles":
                case "buffConcussion":
                    _dialogueManager.RemoveTopic(ConversationTopics.HealthDamageSevere);
                    break;
            }

            if (string.Equals(injuryId, "buffShrapnelWounds", StringComparison.OrdinalIgnoreCase)
                || string.Equals(injuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
            {
                _dialogueManager.RemoveTopic(ConversationTopics.PostOperativeCare);
            }
        }

        /// <summary>
        /// Получить активную основную травму (базовый buffId, не фазовый бафф).
        /// </summary>
        public string? GetActiveInjury()
        {
            string? mainInjuryId = ResolveMainInjuryFromActiveDebuffs();
            if (mainInjuryId == null)
            {
                string? storedMain = _stateManager.GetMainInjuryId();
                if (!string.IsNullOrEmpty(storedMain))
                    _stateManager.ClearMainInjury(storedMain);
                return null;
            }

            if (!IsBaseMainInjuryId(mainInjuryId))
            {
                _stateManager.ClearMainInjury(mainInjuryId);
                return null;
            }

            if (!_stateManager.IsMainInjury(mainInjuryId))
                _stateManager.SetMainInjury(mainInjuryId, force: true);

            LogActiveMainInjury(mainInjuryId);
            return mainInjuryId;
        }

        /// <summary>
        /// Получить активную травму или её фазу по приоритету (для госпитализации и мед. пайплайна).
        /// </summary>
        public string? GetActiveInjuryOrPhaseByPriority() => GetActiveInjury();

        private static bool IsBaseMainInjuryId(string injuryId) =>
            InjurySets.HarveyTreatable.Contains(injuryId)
            && !InjurySets.KnownComplicationBuffIds.Contains(injuryId);

        private void LogActiveMainInjury(string injuryId)
        {
            _monitor.Log($"[MainInjury] Активная основная травма: {injuryId}", LogLevel.Trace);
        }

        /// <summary>
        /// MainInjury ∈ Severe ∪ Critical (без учёта осложнений вроде PainFlare).
        /// </summary>
        public static bool IsSeriousMainInjuryId(string? injuryId)
        {
            if (string.IsNullOrEmpty(injuryId))
                return false;

            return InjurySets.Severe.Contains(injuryId)
                || InjurySets.Critical.Contains(injuryId);
        }

        /// <summary>
        /// Серьёзная основная травма: MainInjury ∈ Severe ∪ Critical и активна (база или фаза).
        /// Осложнения сами по себе не считаются серьёзной травмой.
        /// </summary>
        public bool IsMainInjurySerious()
        {
            string? mainInjuryId = GetActiveInjury();
            if (string.IsNullOrEmpty(mainInjuryId))
                return false;

            if (!IsSeriousMainInjuryId(mainInjuryId))
                return false;

            return HasInjuryOrPhase(mainInjuryId);
        }

        public bool HasDirtyWoundComplication() =>
            _buffManager.HasBuff(InjuryBuffs.DirtyWound)
            || _stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound);

        /// <summary>
        /// Серьёзная основная + грязная рана — усиленный риск (шахта, госпитализация).
        /// </summary>
        public bool HasSeriousMainInjuryWithDirtyWound() =>
            IsMainInjurySerious() && HasDirtyWoundComplication();

        /// <summary>
        /// Есть серьёзная основная травма (MainInjury, не «любой severe buff»).
        /// </summary>
        public bool HasAnySevereInjuryOrPhase() => IsMainInjurySerious();

        /// <summary>
        /// Проверить наличие травмы или её фазы (DebuffState + ожидаемый бафф для текущего этапа лечения).
        /// </summary>
        public bool HasInjuryOrPhase(string injuryId) => ValidateMainInjury(injuryId).Valid;

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
                MainInjury = GetActiveInjury()
            };

            // Проверяем осложнения
            CheckAndAddComplication(result, InjuryBuffs.DirtyWound, "DirtyWound");
            CheckAndAddComplication(result, InjuryBuffs.WetBandage, "WetBandage");
            CheckAndAddComplication(result, InjuryBuffs.WetStitches, "WetStitches");
            CheckAndAddComplication(result, InjuryBuffs.AllergicRash, "AllergicRash");
            CheckAndAddComplication(result, InjuryBuffs.PainFlare, "PainFlare");

            return result;
        }

        private void CheckAndAddComplication(InjuryCollection collection, string buffId, string name)
        {
            if (!_stateManager.State.ActiveComplications.ContainsKey(buffId))
                return;

            if (string.Equals(buffId, InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase)
                && !InjurySets.IsPainFlareEligibleMain(collection.MainInjury ?? GetActiveInjury()))
                return;

            if (!_buffManager.HasBuff(buffId) && !_stateManager.HasDebuffState(buffId))
                return;

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

        /// <summary>
        /// Снять все лечебные баффы травмы (фазовые + простые cure-бaffы).
        /// Вызывается только после финального осмотра у Харви.
        /// </summary>
        public void RemoveAllTreatmentBuffs(string injuryId)
        {
            RemoveAllPhaseBuffs(injuryId);

            if (!TreatmentManager.IsSimpleTreatmentInjury(injuryId))
                return;

            string? cureBuffId = GetExpectedCureBuffId(injuryId);
            if (cureBuffId != null)
                _buffManager.RemoveBuff(cureBuffId);

            if (string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                _buffManager.RemoveBuff(CureBuffs.BadlyHurtOutpatientCare);
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
                if (!TryApplyMainInjury(injuryId, applyFunc))
                    return;

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

        private void ApplyHurtCore()
        {
            _buffManager.AddBuff("buffHurt", -2);
            _dialogueManager.AddTopic(ConversationTopics.Hurt, 2);
            Game1.playSound("debuffHit");
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffHurt", currentDay, 2, 0, 0);
        }

        public void ApplyHurt()
        {
            TryApplyMainInjury("buffHurt", ApplyHurtCore);
        }

        public void ApplyHurtSafe()
        {
            ApplyInjurySafe("buffHurt", ApplyHurtCore, Triggers.Hurt);
        }

        private void ApplyBadlyHurtCore()
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

        public void ApplyBadlyHurt()
        {
            TryApplyMainInjury("buffBadlyHurt", ApplyBadlyHurtCore);
        }

        public void ApplyBadlyHurtSafe()
        {
            ApplyInjurySafe("buffBadlyHurt", ApplyBadlyHurtCore, Triggers.BadlyHurt);
        }

        public void ApplyBadlyHurtFromMinePassOut()
        {
            _monitor.Log("[MineRescue] Принудительно применяем buffBadlyHurt после смерти в шахте", LogLevel.Warn);

            string? currentMain = GetCurrentMainInjuryId();
            if (!string.IsNullOrEmpty(currentMain))
            {
                if (string.Equals(currentMain, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
                {
                    _monitor.Log("[MineRescue] buffBadlyHurt уже активен после смерти в шахте", LogLevel.Debug);
                    return;
                }

                if (GetInjuryPriorityPublic(currentMain) > GetInjuryPriorityPublic("buffBadlyHurt"))
                {
                    _monitor.Log(
                        $"[MineRescue] buffBadlyHurt не применён: текущая травма {currentMain} тяжелее",
                        LogLevel.Debug);
                    return;
                }
            }

            // Смерть в шахте эскалирует даже buffHurt на лечении (forceReplace обходит TreatmentStarted).
            bool needsForceReplace = !string.IsNullOrEmpty(currentMain);
            if (!TryApplyMainInjury(
                    "buffBadlyHurt",
                    ApplyBadlyHurtCore,
                    allowUpgrade: true,
                    forceReplace: needsForceReplace,
                    suppressBlockedFeedback: true))
            {
                _monitor.Log("[MineRescue] Не удалось применить buffBadlyHurt после смерти в шахте", LogLevel.Warn);
                return;
            }

            _dialogueManager.TryAddHarveyNeedsFirstTreatmentTopic("buffBadlyHurt");
        }

        // === СРЕДНИЕ ТРАВМЫ ===

        private void ApplySprainedAnkleCore()
        {
            _buffManager.AddBuff("buffSprainedAnkle", -2);
            _dialogueManager.AddTopic(ConversationTopics.SprainedAnkle, 7);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (2 фазы: 3 + 4 = 7 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffSprainedAnkle", currentDay, 3, 4, 0);
        }

        public void ApplySprainedAnkle()
        {
            TryApplyMainInjury("buffSprainedAnkle", ApplySprainedAnkleCore);
        }

        public void ApplySprainedAnkleSafe()
        {
            ApplyInjurySafe("buffSprainedAnkle", ApplySprainedAnkleCore, Triggers.SprainedAnkle);
        }

        private void ApplyBruisedRibsCore()
        {
            _buffManager.AddBuff("buffBruisedRibs", -2);
            _dialogueManager.AddTopic(ConversationTopics.BruisedRibs, 9);
            Game1.playSound("debuffHit");
            
            // Инициализируем состояние дебаффа (2 фазы: 4 + 5 = 9 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBruisedRibs", currentDay, 4, 5, 0);
        }

        public void ApplyBruisedRibs()
        {
            TryApplyMainInjury("buffBruisedRibs", ApplyBruisedRibsCore);
        }

        public void ApplyBruisedRibsSafe()
        {
            ApplyInjurySafe("buffBruisedRibs", ApplyBruisedRibsCore, Triggers.BruisedRibs);
        }

        private void ApplyBackStrainCore()
        {
            _buffManager.AddBuff("buffBackStrain", -2);
            _dialogueManager.AddTopic(ConversationTopics.BackStrain, 6);
            Game1.playSound("debuffHit");
            
            // Инициализируем состояние дебаффа (2 фазы: 2 + 4 = 6 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBackStrain", currentDay, 2, 4, 0);
        }

        public void ApplyBackStrain()
        {
            TryApplyMainInjury("buffBackStrain", ApplyBackStrainCore);
        }

        public void ApplyBackStrainSafe()
        {
            ApplyInjurySafe("buffBackStrain", ApplyBackStrainCore, Triggers.BackStrain);
        }

        private void ApplyDeepCutsCore()
        {
            // Применяем базовый бафф травмы (до лечения)
            _buffManager.AddBuff("buffDeepCuts", -2);
            _dialogueManager.AddTopic(ConversationTopics.DeepCuts, 7);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 2 + 3 + 2 = 7 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffDeepCuts", currentDay, 2, 3, 2);
        }

        public void ApplyDeepCuts(string source = "generic")
        {
            TryApplyMainInjury("buffDeepCuts", ApplyDeepCutsCore);
        }

        public void ApplyDeepCutsSafe(string source = "generic")
        {
            string trigger = source == "combat" 
                ? Triggers.DeepCutsCombat 
                : Triggers.DeepCutsFarming;
            ApplyInjurySafe("buffDeepCuts", ApplyDeepCutsCore, trigger);
        }

        private void ApplyBurnWoundsCore()
        {
            _buffManager.AddBuff("buffBurnWounds", -2);
            _dialogueManager.AddTopic(ConversationTopics.BurnWounds, 8);
            Game1.playSound("fireball");

            // Инициализируем состояние дебаффа (2 фазы: 3 + 5 = 8 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffBurnWounds", currentDay, 3, 5, 0);
        }

        public void ApplyBurnWounds()
        {
            TryApplyMainInjury("buffBurnWounds", ApplyBurnWoundsCore);
        }

        public void ApplyBurnWoundsSafe()
        {
            ApplyInjurySafe("buffBurnWounds", ApplyBurnWoundsCore, Triggers.BurnWounds);
        }

        private void ApplyInfectedWoundCore()
        {
            _buffManager.AddBuff("buffInfectedWound", -2);
            _dialogueManager.AddTopic(ConversationTopics.InfectedWound, 14);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (2 фазы: 3 + 11 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffInfectedWound", currentDay, 3, 11, 0);
        }

        public void ApplyInfectedWound()
        {
            TryApplyMainInjury("buffInfectedWound", ApplyInfectedWoundCore);
        }

        public void ApplyInfectedWoundSafe()
        {
            ApplyInjurySafe("buffInfectedWound", ApplyInfectedWoundCore, Triggers.InfectedWound);
        }

        /// <summary>
        /// Эскалация осложнения (DirtyWound/WetBandage) в основную травму buffInfectedWound.
        /// Заменяет InfectionSensitive MainInjury через TryApplyMainInjury, без cooldown ApplyInjurySafe.
        /// </summary>
        public bool TryEscalateComplicationToInfectedWound(string sourceComplicationId)
        {
            string? currentMain = _stateManager.GetMainInjuryId();
            if (string.IsNullOrEmpty(currentMain))
                currentMain = GetActiveInjury();

            if (string.IsNullOrEmpty(currentMain))
            {
                _monitor.Log(
                    $"[MainInjury] Эскалация инфекции отменена: нет основной травмы (source={sourceComplicationId})",
                    LogLevel.Warn);
                return false;
            }

            if (!InjurySets.InfectionSensitive.Contains(currentMain))
            {
                _monitor.Log(
                    $"[MainInjury] Эскалация инфекции отменена: {currentMain} не InfectionSensitive (source={sourceComplicationId})",
                    LogLevel.Debug);
                return false;
            }

            if (string.Equals(currentMain, "buffInfectedWound", StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log(
                    $"[MainInjury] Эскалация инфекции: уже buffInfectedWound (source={sourceComplicationId})",
                    LogLevel.Info);
                return true;
            }

            if (string.IsNullOrEmpty(_stateManager.GetMainInjuryId()))
                _stateManager.SetMainInjury(currentMain);

            bool upgraded = TryApplyMainInjury(
                "buffInfectedWound",
                ApplyInfectedWoundCore,
                allowUpgrade: false,
                forceReplace: true,
                suppressBlockedFeedback: true);

            if (upgraded)
                _dialogueManager.TryAddHarveyNeedsFirstTreatmentTopic("buffInfectedWound");

            return upgraded;
        }

        // === ТЯЖЁЛЫЕ ТРАВМЫ (3 фазы) ===

        private void ApplyTornMusclesCore()
        {
            _buffManager.AddBuff("buffTornMuscles", -2);
            _dialogueManager.AddTopic(ConversationTopics.TornMuscles, 11);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageSevere, 11);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 3 + 5 + 3 = 11 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffTornMuscles", currentDay, 3, 5, 3);
        }

        public void ApplyTornMuscles()
        {
            TryApplyMainInjury("buffTornMuscles", ApplyTornMusclesCore);
        }

        public void ApplyTornMusclesSafe()
        {
            ApplyInjurySafe("buffTornMuscles", ApplyTornMusclesCore, Triggers.TornMuscles);
        }

        private void ApplyConcussionCore()
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
                _hospitalizationManager.StartForcedHospitalization(
                    "buffConcussion",
                    HarveyHelper.GetHarvey());
            }
        }

        public void ApplyConcussion()
        {
            TryApplyMainInjury("buffConcussion", ApplyConcussionCore);
        }

        public void ApplyConcussionSafe()
        {
            ApplyInjurySafe("buffConcussion", ApplyConcussionCore, Triggers.Concussion);
        }

        private void ApplyFracturedBoneCore()
        {
            _buffManager.AddBuff("buffFracturedBone", -2);
            _dialogueManager.AddTopic(ConversationTopics.FracturedBone, 18);
            _dialogueManager.AddTopic(ConversationTopics.HealthDamageCritical, 18);
            Game1.playSound("debuffHit");

            // Инициализируем состояние дебаффа (3 фазы: 4 + 10 + 4 = 18 дней)
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffFracturedBone", currentDay, 4, 10, 4);
        }

        public void ApplyFracturedBone()
        {
            TryApplyMainInjury("buffFracturedBone", ApplyFracturedBoneCore);
        }

        public void ApplyFracturedBoneSafe()
        {
            ApplyInjurySafe("buffFracturedBone", ApplyFracturedBoneCore, Triggers.FracturedBone);
        }

        private void ApplyShrapnelWoundsCore()
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

        public void ApplyShrapnelWounds()
        {
            TryApplyMainInjury("buffShrapnelWounds", ApplyShrapnelWoundsCore);
        }

        public void ApplyShrapnelWoundsSafe()
        {
            ApplyInjurySafe("buffShrapnelWounds", ApplyShrapnelWoundsCore, Triggers.ShrapnelWounds);
        }

        // === СПЕЦИАЛЬНЫЕ ТРАВМЫ ===

        private void ApplySurgicalWoundCore()
        {
            _buffManager.AddBuff("buffSurgicalWound", -2);
            _dialogueManager.AddTopic(ConversationTopics.SurgicalWound, 7);
            _dialogueManager.AddTopic(ConversationTopics.PostOperativeCare, 7);
            Game1.playSound("debuffHit");
            int currentDay = (int)Game1.stats.DaysPlayed;
            _stateManager.CreateDebuffState("buffSurgicalWound", currentDay, 7, 0, 0);
        }

        public void ApplySurgicalWound()
        {
            TryApplyMainInjury("buffSurgicalWound", ApplySurgicalWoundCore);
        }

        public void ApplySurgicalWoundSafe()
        {
            ApplyInjurySafe("buffSurgicalWound", ApplySurgicalWoundCore, Triggers.SurgicalWound);
        }

        private void ApplyColdCore()
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

        /// <summary>
        /// Применить простуду (2 фазы: острая + восстановление)
        /// </summary>
        public void ApplyCold()
        {
            TryApplyMainInjury(InjuryBuffs.Cold, ApplyColdCore);
        }

        public void ApplyColdSafe()
        {
            ApplyInjurySafe(InjuryBuffs.Cold, ApplyColdCore, Triggers.Cold);
        }
    }
}

