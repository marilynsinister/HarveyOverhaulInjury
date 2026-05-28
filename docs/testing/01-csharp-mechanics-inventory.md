# Инвентаризация C# механик Harvey Overhaul Injury

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)

**Область:** только C# (`HarveyOverhaulInjury`), файлы из задания чата 01.  
**Не включено:** CP-события, JSON баффов/диалогов, менеджеры вне списка (PrescriptionManager, CheckupManager, …) — только как перекрёстные ссылки, где C#-константы используются ими.

**Источник правды по ID:** `Core/Constants.cs`, `ModEntry.KnownTraumas` / `KnownComplications`.

---

## Все травмы

| BuffId | TopicId | тип | фазы (дней) | cure buff или phase buffs | где создаётся | где лечится | что проверять в тестах |
|--------|---------|-----|-------------|---------------------------|---------------|-------------|------------------------|
| `buffHurt` | `topicHurt` | простая | — (2 дн. simple) | `buffHarveyTreatment` | `PlayerEventHandler.ProcessDamageBasedInjuries` (5+ dmg, 35%); `injury_debuff_add` | Клик Harvey → `InteractionHandler` → `TreatmentManager.StartSimpleTreatment`; авто-завершение `GameEventHandler.CheckSimpleTreatmentCompletion` | MainInjury, cure buff после лечения, `ReadyForRecovery` N/A; topic `topicHurtCured` после срока |
| `buffBadlyHurt` | `topicBadlyHurt` | простая | — (4 дн.) | `buffHarveyIntensiveCare` → после выписки `HarveyMod_BadlyHurt_OutpatientCare` | HP≤10 `ProcessDamageBasedInjuries`; pass-out critical `PassOutHandler`; mine death `PassOutHandler`/`ApplyBadlyHurtFromMinePassOut`; `injury_debuff_add` | Лечение у Harvey; госпитализация `HospitalizationManager`; discharge заменяет intensive→outpatient | + `topicHealthDamageCritical`; Severe; mine rescue → badly hurt; upgrade только из `buffHurt` |
| `buffSprainedAnkle` | `topicSprainedAnkle` | фазовая (2) | 3 + 4 | P1 `HarveyMod_SprainedAnkle_Acute`, P2 `HarveyMod_SprainedAnkle_Recovery` | Только `InjuryManager.ApplySprainedAnkleSafe` (trigger CP); `injury_debuff_add` | StartTreatment → phase buffs; AdvancePhase / CompleteRecovery через клик Harvey | `injury_phase_*`; topics `topicSprainedAnklePhaseAcute/Healing` |
| `buffBruisedRibs` | `topicBruisedRibs` | фазовая (2) | 4 + 5 | P1 `HarveyMod_BruisedRibs_Acute`, P2 `HarveyMod_BruisedRibs_Healing` | Combat 15+ dmg 25% `ProcessDamageBasedInjuries`; `injury_debuff_add` | То же фазовое лечение | Neglect grace 2 дн.; overwork → PainFlare |
| `buffBackStrain` | `topicBackStrain` | фазовая (2) | 2 + 4 | P1 `HarveyMod_BackStrain_Acute`, P2 `HarveyMod_BackStrain_Recovery` | Farming tool-use roll `PlayerEventHandler`; `injury_debuff_add` | Фазовое лечение | Overwork-sensitive; farming counters `injury_farming_counters` |
| `buffDeepCuts` | `topicDeepCuts` | фазовая (3) | 2 + 3 + 2 | `HarveyMod_DeepCuts_Acute/Healing/Recovery` | Combat 10+ dmg 30%; farming deep cuts; `injury_debuff_add` | Фазовое лечение + checkup между фазами (CheckupManager, вне списка) | DirtyInMines, WetBandageSensitive, InfectionSensitive; MainInjury priority |
| `buffBurnWounds` | `topicBurnWounds` | фазовая (2) | 3 + 5 | `HarveyMod_BurnWounds_Acute/Healing` | Explosion 40% `CheckExplosionInjuries`; `injury_debuff_add` | Фазовое лечение | Severe; dirty mine exposure |
| `buffInfectedWound` | `topicInfectedWound` | фазовая (2) | 3 + 11 | `HarveyMod_InfectedWound_Acute/Treatment` | Эскалация DirtyWound/WetBandage `ComplicationManager`; `ApplyInfectedWoundSafe` (CP trigger); `injury_debuff_add` | Фазовое лечение; urgent untreated warnings | Эскалация снимает wound complications; Critical |
| `buffTornMuscles` | `topicTornMuscles` | фазовая (3) | 3 + 5 + 3 | `HarveyMod_TornMuscles_Acute/Healing/Rehab` | Farming tool-use; `injury_debuff_add` | Фазовое лечение | + `topicHealthDamageSevere`; overwork/storm |
| `buffConcussion` | `topicConcussion` | фазовая (3) | 2 + 4 + 3 | `HarveyMod_Concussion_Acute/Rest/Limited` | Combat 20+ dmg 25%; `ForceHospitalization` config → immediate hosp.; `injury_debuff_add` | Фазовое лечение; forced hosp. | Critical; TreatmentPlanEligible; night visit |
| `buffFracturedBone` | `topicFracturedBone` | фазовая (3) | 4 + 10 + 4 | `HarveyMod_FracturedBone_Acute/Cast/Recovery` | Combat 30+ dmg 10%; `injury_debuff_add` | Фазовое лечение | + `topicHealthDamageCritical`; storm PainFlare |
| `buffShrapnelWounds` | `topicShrapnelWounds` | фазовая (3) | 3 + 5 + 3 | `HarveyMod_Shrapnel_Surgery/Healing/Recovery` | Explosion 60% shrapnel; `injury_debuff_add` | Фазовое лечение | + Critical + `topicPostOperativeCare` |
| `buffSurgicalWound` | `topicSurgicalWound` | простая | — (7 дн.) | `buffPostSurgicalCare` | Story one-shot `Triggers.SurgicalWound` (CP); `injury_debuff_add` | Simple treatment | + `topicPostOperativeCare`; WetStitches risk |
| `buffCold` | `topicCold` | фазовая (2) | 2 + 2 | P1 `HarveyMod_Cold_Acute`, P2 `HarveyMod_Cold_Recovery` | Дождь 5–20+ мин `PlayerEventHandler.CheckColdRisk`; `injury_debuff_add` | Фазовое лечение; self-care tea (SelfCareManager) | Rain debug `injury_rain_debug`; cured topic `topicColdCured` |

**Примечания**

- **Простые** (без `injury_phase_advance`): `buffHurt`, `buffBadlyHurt`, `buffSurgicalWound` — см. `TreatmentManager.IsSimpleTreatmentInjury` / `CureByInjury`.
- **Фазовые** — см. `TreatmentManager.PhasedInjuries`; смена фазы только при `ReadyForNextPhase` + клик Harvey (`AdvanceInjuryToNextPhase`).
- **MainInjury:** одна основная; приоритет `InjurySets.MainInjuryPriorityOrder`; upgrade allowlist только `buffHurt`→`buffBadlyHurt` (до начала лечения).
- **Доп. cure-баффы** (не main injury): `buffHarveyCare`, `buffHarveyRehab`, `buffHarveyProtection`, `buffTeracitin`, `buffAntibioticsTreatment`, `buffForcedSedation` — назначаются в CP/расширенном пайплайне; в проанализированных файлах явно используются Care/Rehab/Outpatient.

---

## Все осложнения

| BuffId | TopicId | как появляется | как лечится | может ли перейти в инфекцию | что проверять |
|--------|---------|----------------|-------------|------------------------------|---------------|
| `HarveyMod_DirtyWound` | `topicHarvey_DirtyWound` | Шахта: roll `ComplicationManager.TryApplyDirtyWoundFromMine` (`PlayerEventHandler` exposure); нелеченная InfectionSensitive +1 день 25% `TryApplyDirtyWoundFromUntreated`; NoMine violation bonus; `injury_debuff_add` | Клик Harvey → `TreatAllComplications` (`TreatmentManager`) | **Да** — дни 1→15%, 2→40%, 3+→100%; mail `HarveyMod_DirtyWoundInfection` | `injury_mine_dirty_debug`; MainInjury ∈ DirtyInMines; эскалация → `buffInfectedWound`, clears wound comps |
| `HarveyMod_WetBandage` | `topicHarvey_WetBandage` | Дождь/вода при активной повязке `PlayerEventHandler` → `TryApplyWetBandageFromWater`; KeepDry violation rolls; `injury_debuff_add` | `TreatAllComplications` | **Да** — дни 1–4+ шкала `CalculateWetBandageInfectionChance`; mail `HarveyMod_WetBandageInfection`; CleanBandage protection | Только при `TreatmentStarted` + WetBandageSensitive; invalid → `injury_cleanup_invalid_complications` |
| `HarveyMod_WetStitches` | `topicHarvey_WetStitches` | KeepDry violation + surgical/shrapnel context `PlayerEventHandler`; wet exposure при `buffSurgicalWound` | `TreatAllComplications` | **Нет** (не в infection escalation path; cleared on infection) | Prescription KeepDry; отдельно от WetBandage |
| `HarveyMod_Neglect` | `topicHarvey_Neglect` | Просрочка фазы лечения `ComplicationManager.CheckPhaseNeglect`; DayEnding neglect strikes `GameEventHandler.CheckNeglect` (≥ `NeglectDaysThreshold`) | `TreatAllComplications` | **Нет** | Mail `HarveyMod_NeglectWarning`; strikes в `NeglectStrikesByInjury` |
| `HarveyMod_PainFlare` | `topicHarvey_PainFlare` | Гроза 30% `TryApplyStormPainFlareIfEligible`; перегрузка 40%; blocked heavier injury `InjuryManager.TryApplyPainFlareInsteadOfInjury`; infection fallback | `TreatAllComplications`; night visit 50% снятие | **Нет** | Не блокирует MainInjury; storm + overwork |
| `HarveyMod_AllergicRash` | `topicHarvey_AllergicRash` | **В проанализированных файлах автotrigger отсутствует** — только `injury_debuff_add`; константа `SpringRashChance` в config не подключена в этих файлах | `TreatAllComplications` | **Нет** | CP/другой код?; proximity dialogue есть |

**Служебные debuff (не осложнения DebuffState):**

| BuffId | Назначение |
|--------|------------|
| `HarveyMod_MineForbidden` | На день после Severe warning в шахте; снимается через `MineForbiddenDurationDays` |
| `buffTooCold` | 5 мин холод на улице — не HarveyTreatable, отдельный debuff |

---

## Все conversation topics из C#

Колонка «дней» — значение при `AddTopic`. Удаление: `injury_reset` / `ModTopicRegistry` для owned topics.

### Травмы (базовые)

| topic id | кто добавляет | кто удаляет | дней | тестовый риск |
|----------|---------------|-------------|------|---------------|
| `topicHurt` | `InjuryManager`, `ModEntry` debug | `TreatmentManager` (start), `InjuryManager` (replace), reset | 2 | CP dialogue Treat_Hurt_* |
| `topicBadlyHurt` | `InjuryManager`, debug | reset, replace | 4 | + Critical HUD |
| `topicSprainedAnkle` … `topicCold` | `InjuryManager`, debug | `TreatmentManager` start (injury topic), reset | 4–18 | Phase topics после лечения |
| `topicTreatment{Injury}` | `TreatmentManager.StartPhasedTreatment` | recovery/cure, reset | total duration | Мост CP treatment plan |
| `topic{Injury}Phase{Acute\|Healing\|Recovery}` | `TreatmentManager` | phase advance, recovery, reset | phase duration | Checkup topics |
| `topic{Injury}Cured` | `GameEventHandler.CheckSimpleTreatmentCompletion` | reset / expire | 7 | Финальный осмотр CP |
| `topicColdCured`, `topicSurgicalWoundCured` | CP / recovery pipeline (константы) | reset | — | Cured cutscenes |
| `topicTreatmentCompleted` | `TreatmentManager.CompleteInjuryRecovery` (debug cure) | reset | 7 | Debug-only path |

### Осложнения и situational

| topic id | кто добавляет | кто удаляет | дней | тестовый риск |
|----------|---------------|-------------|------|---------------|
| `topicHarvey_WetBandage` … `topicHarvey_PainFlare` | `ComplicationManager`, debug | `RemoveComplication`, `TreatAllComplications`, reset | 2–7 | Proximity_Complication_* |
| `topicHarvey_Neglect` | `ComplicationManager`, `GameEventHandler` | treat, reset | 7 | Neglect mail |
| `topicHealthDamageCritical` | badly hurt, fracture, shrapnel, infection fallback | `InjuryManager` remove on replace | 3–18 | CP severe lines |
| `topicHealthDamageSevere` | torn, concussion | remove on replace | 9–11 | CP |
| `topicPostOperativeCare` | shrapnel, surgical | remove on replace | 7 | CP surgery arc |
| `topicTooCold` | `PlayerEventHandler` (direct dict, не DialogueManager) | expire | 2 | Не в ModTopicRegistry? |
| `topicPassedOutInTown` | `PassOutHandler` | expire | 2 | Town 2:00 |
| `topicFarmerExhausted` | `PassOutHandler` fallback | expire | 3 | Exhaustion event |
| `topicMineInjuryRescue` | `PassOutHandler` | `InteractionHandler`, `PlayerEventHandler` | 2 | Mine rescue CP |
| `topicMineRescuePending` | `PassOutHandler` | after event / fallback | 1 | Blocks CP interception |
| `topicHarveyMinorMineRescue` | `PassOutHandler` | expire | 2 | Minor rescue |
| `topicHarvey_ForcedHospitalization` | `HospitalizationManager` | expire | 2 | Forced hosp. |
| `topicHarvey_NightRound` | `TimeEventHandler` | expire | 2 | Night visit; не в ConversationTopics class |

### Мосты CP / лечение (константы; setter часто вне списка файлов)

| topic id | кто добавляет (C#) | кто удаляет | дней | тестовый риск |
|----------|-------------------|-------------|------|---------------|
| `topicHarveyNeedsFirstTreatment` | `DialogueManager.TryAdd*` via `InjuryManager` | FirstTreatment seen, start treatment | 7 | `HarveyMod_FirstTreatment` |
| `topicFirstTreatmentComplete` | CP event | — | — | One-shot bridge |
| `topicDiagnosisComplete` | `DialogueManager.TryAddDiagnosisCompleteTopic` | — | 3 | `HarveyMod_TreatmentPlanMeeting` |
| `topicHarvey_Prescription_*` | PrescriptionManager | expire/violation | varies | Prescription mail tier |
| `topicHarvey_CheckupDue*` | CheckupManager | CompleteCheckup | — | Checkup overdue |
| `topicHarvey_Compliance*` | ComplianceManager | rotate | — | Tone, not Friendship |
| `topicHarvey_TreatmentPlan*` | TreatmentPlanManager | expire | — | Tiered plan mail |
| `topicHarvey_Rehab*` | RehabManager | clear | — | Post-severe rehab |
| `topicHarvey_SelfCare*` | SelfCareManager | — | — | Home care |
| `topicHarvey_Proximity*` | HarveyReactionManager via `PlayerEventHandler` | expire | — | CP proximity asset |
| `topicHarveyStormStress`, `HarveyMod_CD_*` | StormComfortLauncher, RescueOperationLauncher | — | — | Parallel story arcs |

**Полный owned-set:** `ModTopicRegistry.GetAllOwnedTopicIds()` — используется `injury_reset`.

---

## Все mail ids из C#

| mail id | условие отправки | что проверять |
|---------|------------------|---------------|
| `mailHarveySleepControl` | Pass-out Town late `PassOutHandler`; `SendLetters` | Legacy CP key |
| `mailHarveyMineForbidden` | DayEnding: `MineWarningDay == today` tiered `GameEventHandler` | После Severe mine warning; tier suffix |
| `HarveyMod_WetCare` | **Не вызывается** в проанализированных C# файлах | CP-only? audit |
| `HarveyMod_WetStitchesCare` | **Не вызывается** в analyzed files | audit |
| `HarveyMod_InfectionAlert` | **Не вызывается** в analyzed files | audit |
| `HarveyMod_NeglectWarning` | Phase neglect `ComplicationManager` | Dedupe via SendLetters |
| `HarveyMod_DirtyWoundInfection` | DirtyWound → infection escalation | MainInjury → infected |
| `HarveyMod_WetBandageInfection` | WetBandage → infection | Self-care multiplier |
| `HarveyMod_TreatmentUrgentReminder` | Untreated / phase end neglect warning | Once per daily call |
| `HarveyMod_TreatmentFinalWarning` | Untreated infected / day before neglect | Critical path |
| `HarveyMod_CheckupOverdue` | CheckupManager missed 4+ days | Tiered |
| `mailHarveyTreatmentPlan_Minor` / `_Severe` | TreatmentPlanManager on start treatment | Tier `_LowHearts`…`_Married` |
| `mailHarveyPrescriptionViolation` | PrescriptionManager violation | Friendship не падает |
| `mailHarveyCheckupReminder` | CheckupManager | Tiered |
| `mailHarveyRehabReminder` / `RehabCompleted` | RehabManager | Post fracture/concussion etc. |
| `mailHarveyNoMineViolation` / `KeepDryViolation` / `RestViolation` | Prescription violations | `injury_test_prescription_violation` |

**Tiered mail:** `HarveyMailHelper.TryScheduleTieredMail` + `SentMedicalMailDays` dedupe.

---

## Все состояния InjuryState

Save key: `injury_state` (`StateManager`).

### Травмы

| Поле | Назначение |
|------|------------|
| `MainInjuryId` | Единственная основная травма |
| `ActiveDebuffs` | `Dictionary<buffId, DebuffState>` — фазы, TreatmentStarted, Ready* |
| `AppliedTriggers` | One-shot story triggers |
| `InjuryCooldownUntilDay` | Cooldown repeatable injuries |
| `LastInjuryAppliedDayByTrigger` | Legacy → миграция |
| `TreatmentComplianceScore` | −10…10, не Friendship |
| `ActivePrescriptions` | Prescription state by id |
| `ActiveRehabInjuryId`, `RehabStartDay`, `RehabDurationDays`, `RehabViolated`, `RehabViolationCount`, `LastRehabViolationDay` | Rehab arc |
| `LastCheckupComplianceDay`, `LastPrescriptionReminderDay`, `LastLowComplianceHudDay` | Compliance UX |

### Осложнения

| Поле | Назначение |
|------|------------|
| `ActiveComplications` | comp buffId → day started |
| `LastInfectionEscalationDay` | Skip neglect same day |
| `NeglectStrikes`, `NeglectStrikesByInjury` | Neglect counters |
| `WetBandageMailDay`, `WetStitchesMailDay` | Mail dedupe (legacy) |
| `SelfCareProtections`, `PendingSelfCareBandageCompliance`, `LastSelfCare*Day` | Self-care |
| `SentMedicalMailDays` | Mail dedupe keys |

### Шахта

| Поле | Назначение |
|------|------------|
| `MineWarningDay` | Severe warning → next day MineForbidden |
| `LastMineSevereWarningDay`, `LastMineSevereForcedExitDay` | Severe re-entry |
| `MineForbiddenAppliedDay`, `LastMineForbiddenInterceptionDay` | Forbidden debuff lifecycle |
| `MineDirtyExposureMinutesToday`, `LastMineDirtyExposureDay`, `LastMineDirtyWoundRollMinute`, `MineDirtyRiskBoostUntilMinute` | Dirty wound rolls |
| `PassedOutInMineYesterday`, `NeedsMineRescueEvent`, `PendingMineRescueEventId`, `PendingMinorMineRescueEventId`, `LastMinorMineRescueDay` | Rescue pipeline |

### Обмороки

| Поле | Назначение |
|------|------------|
| `WasPassedOut`, `WasExhausted`, `WasUpTooLate` | Pass-out classification |
| `LastPassedOutHealth`, `LastPassedOutLocation` | Branching events |
| `PassedOutInTownYesterday` | Town pass-out |
| `PendingHospitalPassOutEventId`, `PendingHospitalPassOutFallbackKind` | Hospital cutscene queue |

### Госпитализация

| Поле | Назначение |
|------|------------|
| `IsHospitalized`, `HospitalizedInjuryId`, `HospitalizationReason` | Active stay |
| `HospitalAdmissionDay/Time/Minutes`, `HospitalMinStayMinutes`, `HospitalDischargeReadyShown` | Discharge timing |
| `PendingForcedHospitalizationWarning`, `PendingForcedHospitalizationWarningDay` | Pre-forced warning |
| `DaysWithSevere` | Severe duration tracking |

### save/load

| Поле | Назначение |
|------|------------|
| `SavedActiveBuffs` | Snapshot end-of-day → restore DayStarted `GameEventHandler.RestoreBuffsFromSnapshot` |
| `TopicMemory` | Legacy |
| `TreatmentConversations`, `ActivePhases` | Obsolete → migration in `StateManager` |

### Дождь / простуда

| Поле | Назначение |
|------|------------|
| `TimeUnderRainTicks`, `LastRainCheckTime` | Wet bandage continuous rain |
| `TotalTimeUnderRainToday`, `LastRainDay` | Cold risk daily accumulator |

### neglect

| Поле | Назначение |
|------|------------|
| `NeglectStrikesByInjury` | Per-injury day-end neglect |
| DebuffState: `MissedCheckupDays`, `CheckupReminderSent`, `CheckupLateLetterSent`, `CheckupOverduePenaltyApplied` | Checkup neglect |

### Прочее (proximity, storm, night)

| Поле | Назначение |
|------|------------|
| `LastNightRoundDay`, `LastNightRoundRollDay` | Night visit |
| `LastStormComfortRollDay`, `LastStormComfortEventDay` | Storm comfort |
| `LastProximityCheckDay`, `LastSupportDay`, `LastProximityReactionMinute`, `LastStrictReactionDay`, `LastProximityReactionReason` | Proximity |
| `LastHealth` | Damage-based injury detection |

### DebuffState (внутри ActiveDebuffs)

| Поле | Назначение |
|------|------------|
| `BuffId`, `InjuryStartDay` | Identity |
| `TreatmentStarted`, `HarveyConversationHappened` | Treatment pipeline |
| `TotalPhases`, `CurrentPhase`, `PhaseStartDay`, `Phase1/2/3Duration` | Phases |
| `ReadyForNextPhase`, `ReadyForRecovery`, `ReadySinceDay` | Gates for Harvey click |
| Checkup* flags | Overdue checkup |

---

## Все console commands

| команда | аргументы | что делает | чего не хватает для тестов |
|---------|----------|------------|----------------------------|
| `injury_reset` | — | Full reset buffs, topics (owned), state | Не тригgerит CP events |
| `injury_debuff_list` | — | Список trauma/complication IDs | — |
| `injury_debuff_add` | `[--force] <id> [мин]` | Main/complication + DebuffState + topic | Не симулирует combat/farming rolls |
| `injury_main_clear` | — | Clear MainInjuryId only | Repair-only |
| `injury_main_set` | `<buffId>` | Set MainInjuryId | Repair-only |
| `injury_phase_list` | — | Phases, Ready*, MainInjuryId | — |
| `injury_phase_ready` | `<buffId> [1\|0]` | ReadyForNextPhase | N/A for simple injuries |
| `injury_phase_recovery` | `<buffId> [1\|0]` | ReadyForRecovery | — |
| `injury_phase_advance` | `<buffId>` | Force next phase | Skips Harvey dialog |
| `injury_phase_cure` | `<buffId>` | Full recovery | Skips cured CP topic flow |
| `injury_rain_debug` | `[secToday] [continuous]` | Rain/cold counters | — |
| `injury_mine_dirty_debug` | — | Read mine dirty state | No force dirty wound |
| `injury_mine_forbidden_clear` | — | Clear mine forbidden | — |
| `injury_debug_mine_rescue` | — | Set mine rescue flags for next day | Needs manual warp/event |
| `injury_cooldowns` | — | Show injury cooldowns | No set cooldown |
| `injury_farming_counters` | — | Tool use counters | No reset/set |
| `injury_night_visit_reset` | — | Reset night visit flags | — |
| `injury_audit_content` | — | Mail/topic vs CP data | Read-only |
| `injury_debug_dump` | — | Full state log | — |
| `injury_cleanup_invalid_complications` | — | Remove stale complications | — |
| `injury_medical_snapshot` | — | Medical pipeline snapshot | — |
| `injury_harvey_click` | `[dry]` | Simulate Harvey click | No cutscene |
| `injury_foreign_topic_add` | `<topic> [days]` | Foreign topic conflict test | — |
| `injury_proximity_test` | `<situation> [tone]` | CP proximity line only | No state |
| `injury_prescription_list` | — | Active prescriptions | — |
| `injury_prescription_add` | `<id> <injury> [days]` | Add prescription | — |
| `injury_prescription_clear` | — | Clear prescriptions + compliance | — |
| `injury_compliance_set` | `<n>` | Set TreatmentComplianceScore | — |
| `injury_test_prescription_violation` | `[NoMine\|KeepDry]` | Test violation | — |
| `injury_checkup_due` | `<buffId>` | Force checkup due state | — |
| `injury_rehab_start` | `<buffId> [days]` | Start rehab | — |
| `injury_rehab_status` | — | Rehab status | — |
| `injury_rehab_clear` | — | Clear rehab | — |
| `injury_selfcare_bandage` | — | Force bandage self-care | — |
| `injury_selfcare_tea` | — | Force tea | — |
| `injury_selfcare_rest` | — | Force rest | — |

**Пробелы для автотестов:** нет `injury_force_dirty_wound`, `injury_force_infection_roll`, `injury_hospitalize`, `injury_pass_out_sim`, save/load trigger, sprained ankle / surgical wound gameplay trigger без CP, AllergicRash trigger, tiered mail preview.

---

## Ключевые пайплайны (для TC)

```text
Травма → MainInjury + DebuffState + topic
  → (опц.) topicHarveyNeedsFirstTreatment
  → клик Harvey: StartTreatment → cure или phase1 buff
  → DayStarted: CheckInjuryPhases → ReadyForNextPhase / ReadyForRecovery
  → клик: AdvancePhase / CompleteRecovery
  → buffHarveyCare + topicCured / TreatmentCompleted

Осложнение → ActiveComplications + buff + topic
  → TreatComplications ИЛИ daily CheckTreatmentCompletion → infection
```

**Обработчики событий (анalyzed files):**

| Файл | Роль |
|------|------|
| `PlayerEventHandler` | Damage, farming, rain, mine, prescriptions, proximity |
| `GameEventHandler` | Day start/end, phases, neglect, buff restore, mine forbidden |
| `TimeEventHandler` | Night visit, hospital discharge tick, storm comfort hook |
| `PassOutHandler` | Pass-out, mine/hospital events, mail sleep control |
| `InteractionHandler` | Harvey click medical FSM |
| `ModEntry` | Console, MCP mirror, debug debuff add |

**Helpers:** `GameUtils.Roll/Today`, `HarveyHelper` relationship tier для mail/proximity.

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC.
2. **Этот файл** — [`01-csharp-mechanics-inventory.md`](01-csharp-mechanics-inventory.md) — справочник C# ID и пробелов QA.
3. **Аудит CP-контента** — следующий шаг: [`docs/events-inventory/01-cp-events-catalog.md`](../events-inventory/01-cp-events-catalog.md) (или создать `02-cp-content-audit.md`), сопоставить:
   - event preconditions ↔ C# topics (`topicHarveyNeedsFirstTreatment`, `topicDiagnosisComplete`, mine rescue IDs);
   - `Data/Buffs` phase buff IDs ↔ `InjuryManager.GetPhaseBuffId`;
   - `Data/Mail` ↔ `MailIds` (в т.ч. неиспользуемые `HarveyMod_WetCare`, `InfectionAlert`);
   - Harvey dialogue keys `Treat_*`, `Proximity_*` ↔ таблицы topics выше.
4. [main-injury-testcases.md](main-injury-testcases.md) — прогон TC по MainInjury + complications с опорой на таблицы здесь.
5. [injury-mcp.md](injury-mcp.md) + [stardew-mcp.md](stardew-mcp.md) — автоматизация подготовки.
6. **Блокеры для C#-only тестов:** sprained ankle / surgical / allergic без CP; save/load buff restore; cutscene PASS только вручную.
7. **Рекомендуемый артеfact чата 02:** `docs/testing/02-cp-content-audit.md` — матрица CP event/mail/dialog ↔ C# constants.
