using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Services;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление осложнениями и проверками лечения
    /// </summary>
    public class ComplicationManager
    {
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly InjuryManager _injuryManager;
        private readonly ComplianceManager _complianceManager;
        private readonly SelfCareManager _selfCareManager;
        private readonly MedicalLetterScheduler? _medicalLetterScheduler;

        private int _lastStormPainFlareGameHour = -1;
        private int _lastOverworkComplicationGameHour = -1;
        private string? _lastWetBandageSkipLogKey;
        private string? _lastDirtyWoundEligibilityLogKey;

        public ComplicationManager(
            IMonitor monitor,
            ModConfig config,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            InjuryManager injuryManager,
            ComplianceManager complianceManager,
            SelfCareManager selfCareManager,
            MedicalLetterScheduler? medicalLetterScheduler = null)
        {
            _monitor = monitor;
            _config = config;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _injuryManager = injuryManager;
            _complianceManager = complianceManager;
            _selfCareManager = selfCareManager;
            _medicalLetterScheduler = medicalLetterScheduler;
        }

        public string? GetActiveMainInjuryId() =>
            _stateManager.GetMainInjuryId() ?? _injuryManager.GetActiveInjury();

        public bool IsMainInjuryIn(HashSet<string> injurySet)
        {
            string? mainInjuryId = GetActiveMainInjuryId();
            return !string.IsNullOrEmpty(mainInjuryId) && injurySet.Contains(mainInjuryId);
        }

        public bool HasComplication(string complicationId) =>
            _buffManager.HasBuff(complicationId)
            || _stateManager.State.ActiveComplications.ContainsKey(complicationId);

        /// <summary>
        /// Есть реальная повязка/обработка раны: cure-бaff Харви или фазовое лечение main-травмы.
        /// </summary>
        public bool HasActiveBandageOrWoundDressing()
        {
            if (_buffManager.HasBuff(InjuryBuffs.WetBandage))
                return false;

            return HasAnyActiveBandagedInjuryInTreatment();
        }

        /// <summary>
        /// Есть ли хотя бы одна травма в активном лечении с реальной повязкой/обработкой раны.
        /// </summary>
        public bool HasAnyActiveBandagedInjuryInTreatment()
        {
            foreach (var (injuryId, debuffState) in _stateManager.State.ActiveDebuffs)
            {
                if (!InjurySets.HarveyTreatable.Contains(injuryId))
                    continue;

                if (InjurySets.KnownComplicationBuffIds.Contains(injuryId))
                    continue;

                if (!debuffState.TreatmentStarted)
                    continue;

                if (HasBandageForInjury(injuryId, debuffState))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Есть реальная повязка/швы: лечебный бафф Харви или фазовая main-травма в активном лечении.
        /// </summary>
        public bool HasActiveTreatmentBandage() => HasActiveBandageOrWoundDressing();

        /// <summary>
        /// Мокрая повязка — только при реальном лечении/повязке, не при открытой нелеченной ране.
        /// </summary>
        public bool CanReceiveWetBandageFromWater()
        {
            if (!HasAnyActiveBandagedInjuryInTreatment())
            {
                LogWetBandageSkip("none", false, "no active bandaged injury");
                return false;
            }

            string? mainInjuryId = GetActiveMainInjuryId();
            DebuffState? mainDebuff = !string.IsNullOrEmpty(mainInjuryId)
                ? _stateManager.GetDebuffState(mainInjuryId)
                : null;
            bool treatmentStarted = mainDebuff?.TreatmentStarted ?? false;

            if (string.IsNullOrEmpty(mainInjuryId))
            {
                LogWetBandageSkip("none", treatmentStarted, "no main injury");
                return false;
            }

            if (mainDebuff == null || !treatmentStarted)
            {
                LogWetBandageSkip(mainInjuryId, treatmentStarted, "treatment not started");
                return false;
            }

            if (!InjurySets.WetBandageSensitive.Contains(mainInjuryId))
            {
                LogWetBandageSkip(mainInjuryId, treatmentStarted, "main not WetBandageSensitive");
                return false;
            }

            // Хирургическая рана после лечения — WetStitches, не WetBandage
            if (string.Equals(mainInjuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase))
            {
                LogWetBandageSkip(mainInjuryId, treatmentStarted, "surgical wound uses WetStitches");
                return false;
            }

            if (!HasBandageForInjury(mainInjuryId, mainDebuff))
            {
                LogWetBandageSkip(mainInjuryId, treatmentStarted, "no active bandage/treatment");
                return false;
            }

            _lastWetBandageSkipLogKey = null;
            _monitor.Log(
                $"[WetBandage] allowed: main={mainInjuryId}, treatmentStarted={treatmentStarted}",
                LogLevel.Debug);
            return true;
        }

        /// <summary>Снять мокрую повязку, если больше нет травм с активной повязкой в лечении.</summary>
        public void CleanupWetBandageIfNoBandagedInjuries()
        {
            if (HasAnyActiveBandagedInjuryInTreatment())
                return;

            bool hadBuff = _buffManager.HasBuff(InjuryBuffs.WetBandage);
            bool hadComplication = _stateManager.State.ActiveComplications.Remove(InjuryBuffs.WetBandage);
            string wetTopic = TopicIds.GetComplicationTopic(InjuryBuffs.WetBandage);
            bool hadTopic = _dialogueManager.HasTopic(wetTopic);

            if (hadBuff)
            {
                _buffManager.RemoveBuff(InjuryBuffs.WetBandage);
                _monitor.Log($"[RecoveryCleanup] Removed lingering buff {InjuryBuffs.WetBandage}", LogLevel.Info);
            }

            if (hadTopic)
            {
                _dialogueManager.RemoveTopic(wetTopic);
                _monitor.Log($"[RecoveryCleanup] Removed lingering topic {wetTopic}", LogLevel.Info);
            }

            if (hadBuff || hadComplication || hadTopic)
                _stateManager.Save();
        }

        private bool HasBandageOrTreatmentForMainInjury()
        {
            string? mainInjuryId = GetActiveMainInjuryId();
            if (string.IsNullOrEmpty(mainInjuryId))
                return false;

            DebuffState? mainDebuff = _stateManager.GetDebuffState(mainInjuryId);
            if (mainDebuff == null || !mainDebuff.TreatmentStarted)
                return false;

            return HasBandageForInjury(mainInjuryId, mainDebuff);
        }

        private bool HasBandageForInjury(string injuryId, DebuffState debuffState)
        {
            if (!debuffState.TreatmentStarted)
                return false;

            if (TreatmentManager.IsSimpleTreatmentInjury(injuryId))
                return HasCureBandageBuffForInjury(injuryId);

            if (!debuffState.IsPhasedInjury || debuffState.CurrentPhase <= 0)
                return false;

            string phaseBuff = _injuryManager.GetPhaseBuffId(injuryId, debuffState.CurrentPhase);
            return !string.IsNullOrEmpty(phaseBuff) && _buffManager.HasBuff(phaseBuff);
        }

        private bool HasCureBandageBuffForInjury(string injuryId)
        {
            if (string.Equals(injuryId, "buffBadlyHurt", StringComparison.OrdinalIgnoreCase))
            {
                return _buffManager.HasBuff(CureBuffs.IntensiveCare)
                    || _buffManager.HasBuff(CureBuffs.BadlyHurtOutpatientCare);
            }

            if (SimpleInjuryCures.Map.TryGetValue(injuryId, out string? cureBuffId))
                return _buffManager.HasBuff(cureBuffId);

            return false;
        }

        /// <summary>
        /// WetBandage валиден только при активном лечении main-травмы и реальной повязке/обработке.
        /// </summary>
        public bool IsWetBandageComplicationValid()
        {
            if (!_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.WetBandage))
                return false;

            string? mainInjuryId = GetActiveMainInjuryId();
            if (string.IsNullOrEmpty(mainInjuryId)
                || !InjurySets.WetBandageSensitive.Contains(mainInjuryId))
            {
                return false;
            }

            return HasBandageOrTreatmentForMainInjury();
        }

        /// <summary>
        /// PainFlare валиден только при main ∈ StormPainSensitive ∪ OverworkSensitive.
        /// </summary>
        public bool IsPainFlareComplicationValid()
        {
            if (!_stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.PainFlare))
                return false;

            return InjurySets.IsPainFlareEligibleMain(GetActiveMainInjuryId());
        }

        /// <summary>
        /// Активные осложнения, которые можно лечить у Харви (ActiveComplications + валидность).
        /// </summary>
        public IReadOnlyList<string> GetActiveTreatableComplicationIds()
        {
            var active = new HashSet<string>(
                _stateManager.State.ActiveComplications.Keys,
                StringComparer.OrdinalIgnoreCase);

            var ordered = new List<string>();
            foreach (string compId in InjurySets.ComplicationPriorityOrder)
            {
                if (active.Contains(compId) && IsTreatableComplication(compId))
                    ordered.Add(compId);
            }

            foreach (string compId in active.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                if (!ordered.Contains(compId, StringComparer.OrdinalIgnoreCase) && IsTreatableComplication(compId))
                    ordered.Add(compId);
            }

            return ordered;
        }

        private bool IsTreatableComplication(string compId)
        {
            if (!_stateManager.State.ActiveComplications.ContainsKey(compId))
                return false;

            if (string.Equals(compId, InjuryBuffs.WetBandage, StringComparison.OrdinalIgnoreCase))
                return IsWetBandageComplicationValid();

            if (string.Equals(compId, InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase))
                return IsPainFlareComplicationValid();

            return _buffManager.HasBuff(compId) || _stateManager.HasDebuffState(compId);
        }

        /// <summary>
        /// Обострение боли вместо более тяжёлой main-травмы (простуда + удар в шахте и т.п. — без PainFlare).
        /// </summary>
        public bool TryApplyPainFlareFromBlockedInjury(string attemptedInjuryId)
        {
            if (!InjurySets.IsPainFlareEligibleMain(GetActiveMainInjuryId()))
                return false;

            return TryApplyComplication(
                InjuryBuffs.PainFlare,
                requiredMainInjurySet: null,
                topicDays: 2,
                new HUDMessage("Удар или нагрузка обострили боль от травмы.", HUDMessage.health_type));
        }

        /// <summary>
        /// Скрытая травма: повышенный риск осложнения при длительном сокрытии от Харви.
        /// </summary>
        public void TryRollHiddenInjuryComplicationRisk(string injuryId)
        {
            DebuffState? ds = _stateManager.GetDebuffState(injuryId);
            if (ds == null || !ds.HiddenFromHarvey)
                return;

            if (HasComplication(InjuryBuffs.DirtyWound) || HasComplication(InjuryBuffs.Neglect))
                return;

            double chance = 0.08 + Math.Min(0.2, ds.HiddenDays * 0.03) + ds.SuspicionLevel * 0.02;
            if (Game1.random.NextDouble() >= chance)
                return;

            if (InjurySets.BandageSensitive.Contains(injuryId)
                || string.Equals(injuryId, "buffDeepCuts", StringComparison.OrdinalIgnoreCase))
            {
                TryApplyComplication(
                    InjuryBuffs.DirtyWound,
                    requiredMainInjurySet: null,
                    topicDays: 2,
                    new HUDMessage("Скрытая рана ухудшилась без осмотра.", HUDMessage.error_type));
                _monitor.Log($"[InjuryVisibility] DirtyWound roll from hidden {injuryId}", LogLevel.Warn);
                return;
            }

            TryApplyNeglectComplication(
                injuryId,
                new HUDMessage("Травма без наблюдения врача даёт о себе знать.", HUDMessage.error_type));
            _monitor.Log($"[InjuryVisibility] Neglect roll from hidden {injuryId}", LogLevel.Warn);
        }

        /// <summary>
        /// Debug/repair: удалить невалидные и stale осложнения из текущего сейва.
        /// </summary>
        public void CleanupInvalidComplications()
        {
            var removed = new List<string>();

            foreach (string compId in CollectTrackedComplicationIds())
            {
                if (!ShouldRemoveComplication(compId, out string reason))
                    continue;

                LogComplicationCleanupRemoval(compId, reason);
                RemoveComplication(compId);
                removed.Add(compId);
            }

            if (removed.Count == 0)
            {
                _monitor.Log("[ComplicationCleanup] nothing to remove", LogLevel.Info);
                return;
            }

            _monitor.Log(
                $"[ComplicationCleanup] done, removed {removed.Count}: {string.Join(", ", removed)}",
                LogLevel.Info);
            _stateManager.Save();
        }

        /// <summary>QA: добавить осложнение через TryApplyComplication с проверкой eligibility.</summary>
        public bool TryApplyComplicationForQa(string complicationId, int? ageDays, out string skipReason)
        {
            skipReason = string.Empty;
            if (!InjurySets.KnownComplicationBuffIds.Contains(complicationId))
            {
                skipReason = "unknown complication id";
                return false;
            }

            if (HasComplication(complicationId))
            {
                skipReason = "already active";
                return false;
            }

            if (!CheckQaComplicationEligibility(complicationId, out skipReason))
                return false;

            var (requiredSet, topicDays) = GetQaComplicationParams(complicationId);
            if (!TryApplyComplication(complicationId, requiredSet, topicDays))
            {
                skipReason = "apply failed";
                return false;
            }

            if (ageDays.HasValue)
            {
                int today = GameUtils.Today();
                int startDay = today - Math.Max(0, ageDays.Value);
                _stateManager.State.ActiveComplications[complicationId] = startDay;
                if (_stateManager.State.ActiveDebuffs.TryGetValue(complicationId, out DebuffState? ds))
                {
                    ds.InjuryStartDay = startDay;
                    ds.PhaseStartDay = startDay;
                }
            }

            _stateManager.Save();
            return true;
        }

        /// <summary>QA: снять одно осложнение (buff + ActiveComplications + DebuffState + topic).</summary>
        public bool RemoveComplicationForQa(string complicationId)
        {
            bool hadBuff = _buffManager.HasBuff(complicationId);
            bool hadComplication = _stateManager.State.ActiveComplications.ContainsKey(complicationId);
            bool hadDebuffState = _stateManager.HasDebuffState(complicationId);
            bool hadTopic = _dialogueManager.HasTopic(GetComplicationTopic(complicationId));

            if (!hadBuff && !hadComplication && !hadDebuffState && !hadTopic)
                return false;

            RemoveComplication(complicationId);
            _stateManager.Save();
            return true;
        }

        private bool CheckQaComplicationEligibility(string complicationId, out string skipReason)
        {
            skipReason = string.Empty;
            string? mainInjuryId = GetActiveMainInjuryId();
            DebuffState? mainDebuff = !string.IsNullOrEmpty(mainInjuryId)
                ? _stateManager.GetDebuffState(mainInjuryId)
                : null;
            bool treatmentStarted = mainDebuff?.TreatmentStarted ?? false;

            switch (complicationId)
            {
                case InjuryBuffs.DirtyWound:
                    if (string.IsNullOrEmpty(mainInjuryId))
                    {
                        skipReason = "no main injury";
                        return false;
                    }
                    if (!InjurySets.DirtyInMines.Contains(mainInjuryId))
                    {
                        skipReason = "main not in DirtyInMines";
                        return false;
                    }
                    if (!_injuryManager.HasInjuryOrPhase(mainInjuryId))
                    {
                        skipReason = "no base buff or phase for main";
                        return false;
                    }
                    return true;

                case InjuryBuffs.WetBandage:
                    if (string.IsNullOrEmpty(mainInjuryId))
                    {
                        skipReason = "no main injury";
                        return false;
                    }
                    if (mainDebuff == null || !treatmentStarted)
                    {
                        skipReason = "treatment not started";
                        return false;
                    }
                    if (!InjurySets.WetBandageSensitive.Contains(mainInjuryId))
                    {
                        skipReason = "main not WetBandageSensitive";
                        return false;
                    }
                    if (!HasActiveBandageOrWoundDressing())
                    {
                        skipReason = "no active bandage/treatment";
                        return false;
                    }
                    return true;

                case InjuryBuffs.WetStitches:
                    if (string.IsNullOrEmpty(mainInjuryId))
                    {
                        skipReason = "no main injury";
                        return false;
                    }
                    if (!string.Equals(mainInjuryId, "buffSurgicalWound", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(mainInjuryId, "buffShrapnelWounds", StringComparison.OrdinalIgnoreCase))
                    {
                        skipReason = "main not surgical/shrapnel";
                        return false;
                    }
                    return true;

                case InjuryBuffs.PainFlare:
                    if (!IsMainInjuryIn(InjurySets.StormPainSensitive)
                        && !IsMainInjuryIn(InjurySets.OverworkSensitive))
                    {
                        skipReason = "main not pain-sensitive";
                        return false;
                    }
                    return true;

                default:
                    return true;
            }
        }

        private (HashSet<string>? requiredSet, int topicDays) GetQaComplicationParams(string complicationId)
        {
            return complicationId switch
            {
                InjuryBuffs.DirtyWound => (InjurySets.DirtyInMines, 4),
                InjuryBuffs.WetBandage => (InjurySets.WetBandageSensitive, 4),
                InjuryBuffs.WetStitches => (QaSurgicalShrapnelSet, 4),
                InjuryBuffs.PainFlare => (
                    IsMainInjuryIn(InjurySets.StormPainSensitive)
                        ? InjurySets.StormPainSensitive
                        : InjurySets.OverworkSensitive,
                    2),
                InjuryBuffs.Neglect => (null, 7),
                InjuryBuffs.AllergicRash => (null, 4),
                _ => (null, 4),
            };
        }

        private static readonly HashSet<string> QaSurgicalShrapnelSet = new(StringComparer.OrdinalIgnoreCase)
        {
            "buffSurgicalWound",
            "buffShrapnelWounds",
        };

        private HashSet<string> CollectTrackedComplicationIds()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string id in _stateManager.State.ActiveComplications.Keys)
                ids.Add(id);

            foreach (string id in _stateManager.State.ActiveDebuffs.Keys)
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(id))
                    ids.Add(id);
            }

            foreach (string id in _stateManager.State.SavedActiveBuffs)
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(id))
                    ids.Add(id);
            }

            foreach (string id in _buffManager.GetActiveModBuffs())
            {
                if (InjurySets.KnownComplicationBuffIds.Contains(id))
                    ids.Add(id);
            }

            return ids;
        }

        private bool ShouldRemoveComplication(string compId, out string reason)
        {
            reason = string.Empty;
            bool inActiveComplications = _stateManager.State.ActiveComplications.ContainsKey(compId);
            bool hasDebuffState = _stateManager.HasDebuffState(compId);
            bool hasBuff = _buffManager.HasBuff(compId);
            bool inSaved = _stateManager.State.SavedActiveBuffs.Any(id =>
                string.Equals(id, compId, StringComparison.OrdinalIgnoreCase));
            string topicId = GetComplicationTopic(compId);
            bool hasTopic = _dialogueManager.HasTopic(topicId);

            if (!inActiveComplications && !hasDebuffState && !hasBuff && !inSaved && !hasTopic)
                return false;

            if (string.Equals(compId, InjuryBuffs.WetBandage, StringComparison.OrdinalIgnoreCase))
            {
                if (inActiveComplications && IsWetBandageComplicationValid())
                    return false;

                reason = inActiveComplications
                    ? $"invalid for main={GetActiveMainInjuryId() ?? "none"} (no bandage/treatment or not WetBandageSensitive)"
                    : "stale (not in ActiveComplications)";
                return true;
            }

            if (string.Equals(compId, InjuryBuffs.PainFlare, StringComparison.OrdinalIgnoreCase))
            {
                if (inActiveComplications && IsPainFlareComplicationValid())
                    return false;

                reason = inActiveComplications
                    ? $"invalid for main={GetActiveMainInjuryId() ?? "none"} (not pain-sensitive)"
                    : "stale (not in ActiveComplications)";
                return true;
            }

            if (!inActiveComplications)
            {
                reason = "stale (not in ActiveComplications)";
                return true;
            }

            return false;
        }

        private void LogComplicationCleanupRemoval(string compId, string reason)
        {
            string topicId = GetComplicationTopic(compId);
            _monitor.Log(
                $"[ComplicationCleanup] removing {compId}: reason={reason}, " +
                $"buff={_buffManager.HasBuff(compId)}, " +
                $"ActiveComplications={_stateManager.State.ActiveComplications.ContainsKey(compId)}, " +
                $"DebuffState={_stateManager.HasDebuffState(compId)}, " +
                $"SavedActiveBuffs={_stateManager.State.SavedActiveBuffs.Any(id => string.Equals(id, compId, StringComparison.OrdinalIgnoreCase))}, " +
                $"topic={topicId}={_dialogueManager.HasTopic(topicId)}",
                LogLevel.Info);
        }

        private void LogWetBandageSkip(string mainInjuryId, bool treatmentStarted, string reason)
        {
            string key = $"{mainInjuryId}|{treatmentStarted}|{reason}";
            if (_lastWetBandageSkipLogKey == key)
                return;

            _lastWetBandageSkipLogKey = key;
            string message = reason == "no active bandaged injury"
                ? "[Rain] WetBandage skipped: no active bandaged injury."
                : $"[WetBandage] skip: {reason}, main={mainInjuryId}, treatmentStarted={treatmentStarted}";
            _monitor.Log(message, LogLevel.Debug);
        }

        private void LogDirtyWoundEligibility(string? mainInjuryId, bool allowed, string reason)
        {
            string main = mainInjuryId ?? "none";
            string key = $"{main}|{allowed}|{reason}";
            if (_lastDirtyWoundEligibilityLogKey == key)
                return;

            _lastDirtyWoundEligibilityLogKey = key;
            _monitor.Log(
                allowed
                    ? $"[DirtyWound] allowed: main={main}, reason={reason}"
                    : $"[DirtyWound] skip: {reason}, main={main}",
                LogLevel.Debug);
        }

        public bool CanReceiveMineDirtyWound()
        {
            string? mainInjuryId = GetActiveMainInjuryId();
            if (string.IsNullOrEmpty(mainInjuryId))
            {
                LogDirtyWoundEligibility(mainInjuryId, false, "no main injury");
                return false;
            }

            if (!InjurySets.DirtyInMines.Contains(mainInjuryId))
            {
                LogDirtyWoundEligibility(mainInjuryId, false, "main not in DirtyInMines");
                return false;
            }

            if (!_injuryManager.HasInjuryOrPhase(mainInjuryId))
            {
                LogDirtyWoundEligibility(mainInjuryId, false, "no base buff or phase for main");
                return false;
            }

            LogDirtyWoundEligibility(mainInjuryId, true, "open or treated wound surface");
            return true;
        }

        public bool TryApplyComplication(
            string complicationId,
            HashSet<string>? requiredMainInjurySet,
            int topicDays,
            HUDMessage? hudMessage = null)
        {
            if (requiredMainInjurySet != null && !IsMainInjuryIn(requiredMainInjurySet))
                return false;

            if (HasComplication(complicationId))
                return false;

            string mainInjuryId = GetActiveMainInjuryId() ?? "none";
            int today = GameUtils.Today();

            _buffManager.AddBuff(complicationId, -2);
            _stateManager.State.ActiveComplications[complicationId] = today;
            _stateManager.CreateComplicationState(complicationId, today);
            _dialogueManager.EnsureComplicationDialogueTopics(complicationId, topicDays);

            if (hudMessage != null)
                Game1.addHUDMessage(hudMessage);

            _monitor.Log(
                $"[Complication] MainInjury={mainInjuryId}, complication={complicationId}",
                LogLevel.Info);
            return true;
        }

        public bool TryApplyDirtyWoundFromMine(double chance, string reason)
        {
            if (!CanReceiveMineDirtyWound())
                return false;

            if (HasComplication(InjuryBuffs.DirtyWound))
                return false;

            if (!GameUtils.Roll(chance))
            {
                _monitor.Log($"[Шахта] Грязная рана не сработала: chance={chance:P0}, {reason}", LogLevel.Debug);
                return false;
            }

            bool applied = TryApplyComplication(
                InjuryBuffs.DirtyWound,
                InjurySets.DirtyInMines,
                topicDays: 4,
                new HUDMessage("Рана загрязнилась! Риск инфекции!", HUDMessage.error_type));

            if (applied)
            {
                _complianceManager.AddCompliance(-1, "dirty_wound");
                _monitor.Log($"[Шахта] Рана загрязнилась: chance={chance:P0}, {reason}", LogLevel.Warn);
            }

            return applied;
        }

        public bool TryApplyWetBandageFromWater(
            int topicDays,
            HUDMessage hudMessage,
            string logContext)
        {
            if (!CanReceiveWetBandageFromWater())
                return false;

            bool applied = TryApplyComplication(
                InjuryBuffs.WetBandage,
                InjurySets.WetBandageSensitive,
                topicDays,
                hudMessage);

            if (applied)
                _monitor.Log(logContext, LogLevel.Info);

            return applied;
        }

        public void TryApplyStormPainFlareIfEligible(bool isOutsideInStorm)
        {
            if (!isOutsideInStorm || !Game1.isLightning)
                return;

            if (!IsMainInjuryIn(InjurySets.StormPainSensitive))
                return;

            int gameHour = Game1.timeOfDay / 100;
            if (_lastStormPainFlareGameHour == gameHour)
                return;

            if (!GameUtils.Roll(0.30))
                return;

            if (TryApplyComplication(
                    InjuryBuffs.PainFlare,
                    InjurySets.StormPainSensitive,
                    topicDays: 2,
                    new HUDMessage("Гроза усилила боль от травмы...", HUDMessage.health_type)))
            {
                _lastStormPainFlareGameHour = gameHour;
            }
        }

        public bool TryRollOverworkComplication(float stamina, int toolUses, float staminaThreshold, int toolUsesThreshold)
        {
            if (!IsMainInjuryIn(InjurySets.OverworkSensitive))
                return false;

            if (stamina > staminaThreshold && toolUses < toolUsesThreshold)
                return false;

            int gameHour = Game1.timeOfDay / 100;
            if (_lastOverworkComplicationGameHour == gameHour)
                return false;

            string? mainInjuryId = GetActiveMainInjuryId();

            if (!HasComplication(InjuryBuffs.PainFlare) && GameUtils.Roll(0.40))
            {
                if (TryApplyComplication(
                        InjuryBuffs.PainFlare,
                        InjurySets.OverworkSensitive,
                        topicDays: 2,
                        new HUDMessage("Перегрузка обострила боль от травмы.", HUDMessage.health_type)))
                {
                    _lastOverworkComplicationGameHour = gameHour;
                    return true;
                }
            }

            _monitor.Log(
                $"[Complication] MainInjury={mainInjuryId ?? "none"}, complication=NeglectWarning",
                LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage(
                "Харви предупреждает: не перегружай травмированное тело.",
                HUDMessage.health_type));
            _lastOverworkComplicationGameHour = gameHour;
            return true;
        }

        private static string GetComplicationTopic(string complicationId) =>
            complicationId switch
            {
                InjuryBuffs.WetBandage => ConversationTopics.WetBandage,
                InjuryBuffs.DirtyWound => ConversationTopics.DirtyWound,
                InjuryBuffs.WetStitches => ConversationTopics.WetStitches,
                InjuryBuffs.Neglect => ConversationTopics.Neglect,
                InjuryBuffs.AllergicRash => ConversationTopics.AllergicRash,
                InjuryBuffs.PainFlare => ConversationTopics.PainFlare,
                _ => TopicIds.GetComplicationTopic(complicationId),
            };

        private bool IsInfectionEscalationAllowed()
        {
            string? mainInjuryId = GetActiveMainInjuryId();
            if (string.IsNullOrEmpty(mainInjuryId))
                return false;

            if (string.Equals(mainInjuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase))
                return true;

            return InjurySets.InfectionSensitive.Contains(mainInjuryId);
        }

        private bool IsInfectedWoundActive()
        {
            if (string.Equals(GetActiveMainInjuryId(), "buffInfectedWound", StringComparison.OrdinalIgnoreCase))
                return true;

            return _injuryManager.HasInjuryOrPhase("buffInfectedWound");
        }

        private const string InfectionEscalationHudMessage = "Рана инфицирована! Срочно к врачу!";

        private void RemoveComplication(string complicationId)
        {
            _buffManager.RemoveBuff(complicationId);
            _stateManager.State.ActiveComplications.Remove(complicationId);
            if (_stateManager.State.ActiveDebuffs.ContainsKey(complicationId))
                _stateManager.RemoveDebuffState(complicationId);
            _dialogueManager.RemoveTopic(GetComplicationTopic(complicationId));
            _stateManager.State.SavedActiveBuffs.RemoveAll(id =>
                string.Equals(id, complicationId, StringComparison.OrdinalIgnoreCase));
            _medicalLetterScheduler?.CancelLettersForState(complicationId);
        }

        private void QueueComplicationMail(string mailId, string reason, string stateId)
        {
            _medicalLetterScheduler?.TryQueueTieredMail(
                mailId,
                reason,
                stateId,
                critical: false);
        }

        /// <summary>
        /// Снять осложнения раны/повязки после эскалации в buffInfectedWound.
        /// Не трогает unrelated осложнения (PainFlare, AllergicRash и т.п.).
        /// </summary>
        public void ClearWoundRelatedComplicationsAfterInfection()
        {
            var cleared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var woundIds = new HashSet<string>(
                InjurySets.WoundComplicationsClearedOnInfectionEscalation,
                StringComparer.OrdinalIgnoreCase);

            foreach (string complicationId in InjurySets.WoundComplicationsClearedOnInfectionEscalation)
            {
                if (TryClearWoundComplication(complicationId))
                    cleared.Add(complicationId);
            }

            foreach (string complicationId in _stateManager.State.ActiveComplications.Keys.ToList())
            {
                if (!woundIds.Contains(complicationId) || cleared.Contains(complicationId))
                    continue;

                if (TryClearWoundComplication(complicationId))
                    cleared.Add(complicationId);
            }

            _stateManager.ResetNeglectStrikes();
            _stateManager.State.SavedActiveBuffs.RemoveAll(id => woundIds.Contains(id));

            if (cleared.Count > 0)
            {
                _monitor.Log(
                    $"[Complication] Cleared wound-related complications after infection: {string.Join(", ", cleared)}",
                    LogLevel.Info);
            }
        }

        private bool TryClearWoundComplication(string complicationId)
        {
            bool hadBuff = _buffManager.HasBuff(complicationId);
            bool hadComplication = _stateManager.State.ActiveComplications.ContainsKey(complicationId);
            bool hadDebuffState = _stateManager.HasDebuffState(complicationId);
            bool hadTopic = _dialogueManager.HasTopic(GetComplicationTopic(complicationId));

            if (!hadBuff && !hadComplication && !hadDebuffState && !hadTopic)
                return false;

            RemoveComplication(complicationId);
            return true;
        }

        private void FinalizeInfectionEscalation(string sourceComplicationId, bool wasAlreadyInfected)
        {
            int today = GameUtils.Today();
            _stateManager.State.LastInfectionEscalationDay = today;

            ClearWoundRelatedComplicationsAfterInfection();

            _stateManager.State.SavedActiveBuffs = _buffManager.GetActiveModBuffs();
            _stateManager.Save();

            _monitor.Log(
                $"[Complication] Infection escalation finalized (day {today}, source={sourceComplicationId}, alreadyInfected={wasAlreadyInfected})",
                LogLevel.Info);
        }

        private bool TryEscalateComplicationToInfection(
            string sourceComplicationId,
            int days,
            string mailId,
            string complianceReason)
        {
            if (!IsInfectionEscalationAllowed())
            {
                _monitor.Log(
                    $"[Complication] MainInjury={GetActiveMainInjuryId() ?? "none"}, escalation=infection blocked (not InfectionSensitive)",
                    LogLevel.Debug);
                return false;
            }

            string mainInjuryId = GetActiveMainInjuryId() ?? "none";
            bool wasAlreadyInfected = string.Equals(
                mainInjuryId,
                "buffInfectedWound",
                StringComparison.OrdinalIgnoreCase);
            _monitor.Log(
                $"[Complication] MainInjury={mainInjuryId}, escalation attempt: {sourceComplicationId}→buffInfectedWound (day {days})",
                LogLevel.Warn);

            bool escalated = _injuryManager.TryEscalateComplicationToInfectedWound(sourceComplicationId);

            if (escalated || IsInfectedWoundActive())
            {
                FinalizeInfectionEscalation(sourceComplicationId, wasAlreadyInfected);

                if (string.Equals(sourceComplicationId, InjuryBuffs.DirtyWound, StringComparison.OrdinalIgnoreCase)
                    && !wasAlreadyInfected
                    && escalated)
                {
                    _monitor.Log(
                        $"[Complication] DirtyWound escalated: {mainInjuryId} -> buffInfectedWound",
                        LogLevel.Warn);
                }

                _monitor.Log(
                    wasAlreadyInfected
                        ? $"[Complication] MainInjury=buffInfectedWound, complication={sourceComplicationId} cleared (already infected)"
                        : $"[Complication] MainInjury=buffInfectedWound, complication={sourceComplicationId}→buffInfectedWound (replaced {mainInjuryId})",
                    LogLevel.Warn);

                if (!wasAlreadyInfected && escalated)
                {
                    Game1.addHUDMessage(new HUDMessage(InfectionEscalationHudMessage, HUDMessage.error_type));
                    string infectionReason = string.Equals(sourceComplicationId, InjuryBuffs.DirtyWound, StringComparison.OrdinalIgnoreCase)
                        ? MedicalLetterReasons.InfectionDirty
                        : MedicalLetterReasons.InfectionWet;
                    QueueComplicationMail(mailId, infectionReason, sourceComplicationId);
                    _complianceManager.AddCompliance(-2, complianceReason);
                    return true;
                }

                return false;
            }

            RemoveComplication(sourceComplicationId);

            ApplyInfectionEscalationFallback(mainInjuryId, sourceComplicationId);

            if (IsInfectedWoundActive())
                FinalizeInfectionEscalation(sourceComplicationId, wasAlreadyInfected);

            return false;
        }

        private void ApplyInfectionEscalationFallback(string mainInjuryId, string sourceComplicationId)
        {
            _monitor.Log(
                $"[Complication] MainInjury={mainInjuryId}, escalation=infection failed, complication={sourceComplicationId}, fallback=PainFlare",
                LogLevel.Warn);

            if (!HasComplication(InjuryBuffs.PainFlare))
            {
                TryApplyComplication(
                    InjuryBuffs.PainFlare,
                    requiredMainInjurySet: null,
                    topicDays: 2,
                    hudMessage: null);
            }

            _dialogueManager.AddTopic(ConversationTopics.HealthDamageCritical, 3);
            QueueComplicationMail(
                MailIds.TreatmentUrgentReminder,
                MedicalLetterReasons.TreatmentUrgent,
                GetActiveMainInjuryId() ?? "");

            Game1.addHUDMessage(new HUDMessage(InfectionEscalationHudMessage, HUDMessage.error_type));
            _complianceManager.AddCompliance(-2, "infection_escalation_fallback");
        }

        /// <summary>
        /// Проверить завершение/осложнения лечения (вызывается каждый день).
        /// Возвращает true, если осложнение эскалировало в buffInfectedWound в этом вызове.
        /// </summary>
        public bool CheckTreatmentCompletion()
        {
            int today = GameUtils.Today();
            bool infectionEscalated = false;

            // Проверка осложнений от грязной раны
            infectionEscalated |= CheckDirtyWoundComplication(today);

            // Проверка осложнений от мокрой повязки
            infectionEscalated |= CheckWetBandageComplication(today);

            // Проверка фазовых травм на небрежность
            CheckPhaseNeglect(today);

            // Травмы без начатого лечения
            CheckUntreatedInjuries(today);

            return infectionEscalated;
        }

        /// <summary>
        /// Проверить осложнение: Грязная рана → Инфекция
        /// </summary>
        private bool CheckDirtyWoundComplication(int today)
        {
            if (!_stateManager.State.ActiveComplications.TryGetValue(InjuryBuffs.DirtyWound, out int startDay))
                return false;

            int days = today - startDay;
            double infectionChance = days switch
            {
                0 => 0.0,
                1 => 0.15,
                2 => 0.40,
                _ => 1.0
            };

            if (!GameUtils.Roll(infectionChance)) return false;

            return TryEscalateComplicationToInfection(
                InjuryBuffs.DirtyWound,
                days,
                MailIds.DirtyWoundInfection,
                "infection_dirty_wound");
        }

        /// <summary>
        /// Проверить осложнение: Мокрая повязка → Инфекция
        /// </summary>
        private bool CheckWetBandageComplication(int today)
        {
            if (!_stateManager.State.ActiveComplications.TryGetValue(InjuryBuffs.WetBandage, out int startDay))
                return false;

            int days = today - startDay;
            double infectionChance = CalculateWetBandageInfectionChance(days);
            infectionChance *= _selfCareManager.GetWetBandageInfectionChanceMultiplier();

            if (infectionChance <= 0)
            {
                _monitor.Log($"[WetBandage] Инфекция не проверяется: days={days}, startDay={startDay}, today={today}", LogLevel.Debug);
                return false;
            }

            _monitor.Log(
                $"[WetBandage] Проверка инфекции: startDay={startDay}, today={today}, days={days}, chance={infectionChance:P0}",
                LogLevel.Debug);

            if (_selfCareManager.HasSelfCareProtection(SelfCareProtectionTypes.CleanBandage))
                _selfCareManager.ConsumeSelfCareProtection(SelfCareProtectionTypes.CleanBandage);

            if (!GameUtils.Roll(infectionChance)) return false;

            return TryEscalateComplicationToInfection(
                InjuryBuffs.WetBandage,
                days,
                MailIds.WetBandageInfection,
                "infection_wet_bandage");
        }

        // Баланс мокрой повязки: days = today - startDay
        private static double CalculateWetBandageInfectionChance(int days)
        {
            return days switch
            {
                <= 0 => 0.0,
                1 => 0.10,
                2 => 0.35,
                3 => 0.60,
                _ => 0.80
            };
        }

        /// <summary>
        /// Дней без лечения, после которых Харви требует осмотр (TreatmentStarted == false).
        /// </summary>
        private int GetUntreatedThresholdDays(string injuryId)
        {
            return injuryId switch
            {
                "buffHurt" or InjuryBuffs.Cold or "buffBackStrain" => 3,
                "buffSprainedAnkle" or "buffBruisedRibs" => 2,
                "buffDeepCuts" or "buffBurnWounds" or "buffShrapnelWounds" => 1,
                "buffBadlyHurt" or "buffConcussion" or "buffFracturedBone"
                    or "buffInfectedWound" or "buffSurgicalWound" or "buffTornMuscles" => 1,
                _ => 2
            };
        }

        private static bool IsComplicationEntry(string injuryId, DebuffState debuffState)
        {
            return debuffState.TotalPhases == 0
                && InjurySets.KnownComplicationBuffIds.Contains(injuryId);
        }

        private static bool IsInfectionSensitiveInjury(string injuryId) =>
            InjurySets.InfectionSensitive.Contains(injuryId);

        private static string GetUntreatedWarningKey(string injuryId, int injuryStartDay)
            => $"untreatedWarning_{injuryId}_{injuryStartDay}";

        /// <summary>
        /// Проверить травмы, по которым ещё не начато лечение (TreatmentStarted == false).
        /// </summary>
        private void CheckUntreatedInjuries(int today)
        {
            bool sentUrgentReminder = false;
            bool sentStrongWarning = false;

            foreach (var kv in _stateManager.State.ActiveDebuffs.ToList())
            {
                string injuryId = kv.Key;
                var debuffState = kv.Value;

                if (IsComplicationEntry(injuryId, debuffState))
                    continue;

                if (debuffState.TreatmentStarted)
                    continue;

                int daysUntreated = today - debuffState.InjuryStartDay;
                if (daysUntreated < 0)
                    continue;

                int threshold = GetUntreatedThresholdDays(injuryId);

                if (IsInfectionSensitiveInjury(injuryId)
                    && string.Equals(injuryId, GetActiveMainInjuryId(), StringComparison.OrdinalIgnoreCase)
                    && daysUntreated >= threshold + 1)
                {
                    TryApplyDirtyWoundFromUntreated(injuryId, daysUntreated);
                }

                if (daysUntreated < threshold)
                    continue;

                string warningKey = GetUntreatedWarningKey(injuryId, debuffState.InjuryStartDay);
                if (_stateManager.WasApplied(warningKey))
                    continue;

                _stateManager.MarkApplied(warningKey);

                bool isInfectedWound = string.Equals(
                    injuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase);

                if (isInfectedWound)
                {
                    _monitor.Log(
                        $"🚨 Нелеченная инфекция: {injuryId}, дней без осмотра {daysUntreated}",
                        LogLevel.Warn);

                    Game1.addHUDMessage(new HUDMessage(
                        "Харви: инфекция не терпит отлагательств — нужен осмотр!",
                        HUDMessage.error_type));

                    if (!sentStrongWarning)
                    {
                        sentStrongWarning = true;
                        QueueComplicationMail(
                            MailIds.TreatmentFinalWarning,
                            MedicalLetterReasons.TreatmentFinal,
                            injuryId);
                    }
                }
                else
                {
                    _monitor.Log(
                        $"⚠️ Нелеченная травма: {injuryId}, дней без осмотра {daysUntreated} (порог {threshold})",
                        LogLevel.Warn);

                    Game1.addHUDMessage(new HUDMessage(
                        "Харви: эту травму нельзя оставлять без осмотра.",
                        HUDMessage.health_type));

                    if (!sentUrgentReminder)
                    {
                        sentUrgentReminder = true;
                        QueueComplicationMail(
                            MailIds.TreatmentUrgentReminder,
                            MedicalLetterReasons.UntreatedInjury,
                            injuryId);
                    }
                }
            }
        }

        /// <summary>
        /// Риск загрязнения открытой раны при затянувшемся отказе от осмотра.
        /// </summary>
        private void TryApplyDirtyWoundFromUntreated(string injuryId, int daysUntreated)
        {
            if (!IsInfectionSensitiveInjury(injuryId))
                return;

            if (!GameUtils.Roll(0.25))
            {
                _monitor.Log(
                    $"[Untreated] Грязная рана не сработала: {injuryId}, дней {daysUntreated}, chance=25%",
                    LogLevel.Debug);
                return;
            }

            if (TryApplyComplication(
                    InjuryBuffs.DirtyWound,
                    InjurySets.InfectionSensitive,
                    topicDays: 4,
                    new HUDMessage("Рана загрязнилась! Срочно к Харви!", HUDMessage.error_type)))
            {
                _monitor.Log(
                    $"ОСЛОЖНЕНИЕ: {injuryId} без осмотра → грязная рана (день {daysUntreated})",
                    LogLevel.Warn);
            }
        }

        /// <summary>
        /// Небрежность через стандартный путь осложнения (buff + ActiveComplications + topic).
        /// </summary>
        public bool TryApplyNeglectComplication(string? injuryIdForStrikes = null, HUDMessage? hudMessage = null)
        {
            bool alreadyActive = HasComplication(InjuryBuffs.Neglect);

            if (!_dialogueManager.HasTopic(ConversationTopics.Neglect))
                _dialogueManager.AddTopic(ConversationTopics.Neglect, 7);

            if (alreadyActive)
                return false;

            bool applied = TryApplyComplication(
                InjuryBuffs.Neglect,
                requiredMainInjurySet: null,
                topicDays: 7,
                hudMessage ?? new HUDMessage(
                    "Травма ухудшилась из-за отсутствия лечения!",
                    HUDMessage.error_type));

            if (applied && !string.IsNullOrEmpty(injuryIdForStrikes))
            {
                int strikes = _stateManager.IncrementNeglectStrikes(injuryIdForStrikes);
                _monitor.Log(
                    $"📊 [Neglect] strikes ({injuryIdForStrikes}): {strikes}",
                    LogLevel.Warn);
            }

            return applied;
        }

        /// <summary>
        /// Дней отсрочки после окончания текущей фазы, прежде чем наступит небрежность.
        /// </summary>
        private int GetNeglectGraceDays(string injuryId)
        {
            return injuryId switch
            {
                "buffHurt" or InjuryBuffs.Cold or "buffBackStrain" => 3,
                "buffSprainedAnkle" or "buffBruisedRibs" or "buffDeepCuts" or "buffBurnWounds" => 2,
                "buffTornMuscles" or "buffShrapnelWounds" or "buffFracturedBone" or "buffSurgicalWound" => 1,
                "buffConcussion" or "buffInfectedWound" or "buffBadlyHurt" => 1,
                _ => 2
            };
        }

        /// <summary>
        /// Проверить фазовые травмы на небрежность.
        /// Каждый тип письма/уведомления отправляется не более одного раза за вызов,
        /// даже если несколько травм одновременно достигли порога.
        /// </summary>
        private void CheckPhaseNeglect(int today)
        {
            // Флаги: отправлено ли уже данное письмо в текущем вызове
            bool sentUrgentReminder = false;
            bool sentFinalWarning   = false;
            bool sentNeglect        = false;

            foreach (var kv in _stateManager.State.ActiveDebuffs.ToList())
            {
                string injuryId = kv.Key;
                var debuffState = kv.Value;

                if (IsComplicationEntry(injuryId, debuffState)
                    || _stateManager.State.ActiveComplications.ContainsKey(injuryId)
                    || InjurySets.KnownComplicationBuffIds.Contains(injuryId))
                    continue;

                if (injuryId.StartsWith("HarveyMod_", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Небрежность — только фазовые main-травмы в активной фазе
                if (debuffState.TotalPhases == 0 || !debuffState.IsPhasedInjury || !debuffState.IsInTreatment)
                    continue;

                string? mainInjuryId = GetActiveMainInjuryId();
                if (!string.IsNullOrEmpty(mainInjuryId)
                    && !string.Equals(injuryId, mainInjuryId, StringComparison.OrdinalIgnoreCase))
                    continue;

                int daysSincePhaseStart = today - debuffState.PhaseStartDay;
                int currentPhaseDuration = debuffState.GetCurrentPhaseDuration();
                if (currentPhaseDuration <= 0)
                    continue;

                int coldBonus = _selfCareManager.GetColdNeglectGraceBonus(injuryId);
                int gracePeriod = GetNeglectGraceDays(injuryId) + coldBonus;
                if (coldBonus > 0)
                    _selfCareManager.ConsumeSelfCareProtection(SelfCareProtectionTypes.WarmTea);

                int neglectDay = currentPhaseDuration + gracePeriod;

                // Первое предупреждение: фаза завершена, нужен осмотр для перехода
                if (daysSincePhaseStart == currentPhaseDuration)
                {
                    _monitor.Log(
                        $"⚠️ Предупреждение: {injuryId} — фаза завершена, осмотр нужен (grace {gracePeriod} д)",
                        LogLevel.Warn);

                    if (!sentUrgentReminder)
                    {
                        sentUrgentReminder = true;
                        QueueComplicationMail(
                            MailIds.TreatmentUrgentReminder,
                            MedicalLetterReasons.TreatmentUrgent,
                            injuryId);
                        Game1.addHUDMessage(new HUDMessage("Харви настаивает на осмотре!", HUDMessage.health_type));
                    }
                }
                // Финальное предупреждение: за 1 день до небрежности (только если grace > 1)
                else if (gracePeriod > 1 && daysSincePhaseStart == neglectDay - 1)
                {
                    _monitor.Log(
                        $"🚨 ФИНАЛЬНОЕ предупреждение: {injuryId} — небрежность завтра (день {neglectDay})",
                        LogLevel.Error);

                    if (!sentFinalWarning)
                    {
                        sentFinalWarning = true;
                        QueueComplicationMail(
                            MailIds.TreatmentFinalWarning,
                            MedicalLetterReasons.TreatmentFinal,
                            injuryId);
                        Game1.addHUDMessage(new HUDMessage("СРОЧНО! Необходим осмотр!", HUDMessage.error_type));
                    }
                }
                // Небрежность: grace исчерпан
                else if (daysSincePhaseStart >= neglectDay)
                {
                    _monitor.Log(
                        $"🚫 ОСЛОЖНЕНИЕ: {injuryId} → небрежность (день {daysSincePhaseStart}, порог {neglectDay})",
                        LogLevel.Error);

                    TryApplyNeglectComplication(
                        injuryId,
                        sentNeglect
                            ? null
                            : new HUDMessage(
                                "Травма ухудшилась из-за отсутствия лечения!",
                                HUDMessage.error_type));

                    if (!sentNeglect)
                    {
                        sentNeglect = true;
                        QueueComplicationMail(
                            MailIds.NeglectWarning,
                            MedicalLetterReasons.NeglectWarning,
                            injuryId);
                    }
                }
            }
        }
    }
}

