# NPC Reactions — Source Audit

Аудит существующих ID (флаги, ConversationTopic, события, buff/debuff, `$action`, ключи диалогов, условия) для реакций NPC на состояние фермера и отношения с Харви.

**Репозитории:**
| Репо | Роль |
|------|------|
| `HarveyOverhaulInjury` (C#) | Травмы, recovery plan, pass-out, mine rescue, storm comfort launcher |
| `HarveyStressMeter` (C#) | Stress meter, лечение стресса, darkness therapy, Gotoro rescue |
| `HarveyOverhaul [CP]` | Диалоги, события, почта, триггеры Content Patcher |

**Дата аудита:** 2026-06-12  
**Важно:** vanilla `HasFlag` / `SetFlag` **нигде не используются** в этих трёх репозиториях. Состояние передаётся через ConversationTopic, mail, buffs, `eventsSeen`, relationship.

---

## Оглавление

1. [CP-safe условия](#1-cp-safe-условия)
2. [Injury — травмы и осложнения](#2-injury--травмы-и-осложнения)
3. [Clinic / appointment — лечение у Харви](#3-clinic--appointment--лечение-у-харви)
4. [Recovery Plan — режим восстановления](#4-recovery-plan--режим-восстановления)
5. [Rescue / collapse — обмороки и спасение](#5-rescue--collapse--обмороки-и-спасение)
6. [Stress — страхи и стресс](#6-stress--страхи-и-стресс)
7. [Harvey relationship — Dating / Engaged / Married](#7-harvey-relationship--dating--engaged--married)
8. [$action-команды](#8-action-команды)
9. [Отсутствующие, но полезные ID](#9-отсутствующие-но-полезные-id)
10. [Статус активации контента](#10-статус-активации-контента)

---

## 1. CP-safe условия

### ✅ Безопасно для Content Patcher / GameStateQuery

| Тип | CP `When` / GameStateQuery | Примеры |
|-----|---------------------------|---------|
| ConversationTopic | `HasConversationTopic` / `PLAYER_HAS_CONVERSATION_TOPIC Current <id>` | `topicDeepCuts`, `HarveyMod_RecoveryPlanStarted` |
| Buff | `HasBuff` / `PLAYER_HAS_BUFF Current <id>` | `buffFracturedBone`, `buffStressThunder` |
| Mail | `PLAYER_HAS_MAIL Current <id> Received` | `mailHarveySleepControl`, `mailHarveyStressTreatmentThunder` |
| Seen event | `PLAYER_HAS_SEEN_EVENT Current <eventId>` | `HarveyMod_FirstTreatment`, `eventHarveyMineRescue` |
| Relationship | `Relationship:Harvey` / `PLAYER_NPC_RELATIONSHIP Current Harvey Dating` | `Dating`, `Engaged`, `Married` |
| Friendship | `Hearts:Harvey` / `Friendship Harvey 750` | tier 0–10 |
| Quest | vanilla `HasQuest` | `HarveyMod_ThunderRecovery` |
| Weather / Time / Season | vanilla CP tokens | `storm`, `2200 0200` |

**Пример (CP events):**
```
eventHarveyStormComfortFarm/.../GameStateQuery ANY "PLAYER_HAS_BUFF Current buffStressThunder" "PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyStormStress"
```
Файл: `HarveyOverhaul [CP]/assets/Code/events.json`

**Пример (CP dialogue When):**
```json
"When": { "Relationship:Harvey": "Dating, Engaged, Married" }
```
Файл: `HarveyOverhaul [CP]/assets/Code/dialoguesHarveyTreatmentNeededRomantic.json:7-9`

### ⚠️ Осторожно

| ID / паттерн | Почему |
|--------------|--------|
| `topicStressTreatment*Started` | Legacy; C# только cleanup, **не** старт лечения (`HarveyStressMeter/Constants/TreatmentTopics.cs:26-27`) |
| `topicStressGotoroForestRescuePending` | Краткоживущий anti-dupe маркер C# |
| `topicMineRescuePending` | Блокирует CP, пока C# готовит cutscene |
| `HarveyMod_CD_*` | Cooldown-bridge; короткий TTL |
| Mod buffs без регистрации в данных | `buffEmergencySupervision` — нужна регистрация buff в CP/C# |

### ❌ Не CP-safe (только C# save / runtime)

| Поле / состояние | Репо | Файл |
|------------------|------|------|
| `InjuryState.*` (MainInjuryId, RecoveryPlan*, Needs*Event, CareTrust score) | Injury | `Core/Models/InjuryState.cs` |
| `DebuffState.*` (CurrentPhase, TreatmentStarted) | Injury | `Core/Models/DebuffState.cs` |
| `RecoveryPlanState.*` (IsActive, TodayFailed, ExtensionsUsed) | Injury | `Core/Models/RecoveryPlanState.cs` |
| `stress-data-v1` save (StressLoad, WarTraumaFlag, Darkness.FearLevel, SocialAnxietyTherapy.Phase) | StressMeter | `Helpers/SaveDataHelper.cs` |
| `TreatmentFlags`, `ShownCareTrustDialogueKeys` | StressMeter | `Models/TreatmentFlags.cs` |
| `_pendingMineRescueEventId` (in-memory) | Injury | между warp и startEvent |

**Мост для NPC-реакций:** C# выставляет **topics / buffs / mail** — их и использовать в CP, не читать save напрямую.

---

## 2. Injury — травмы и осложнения

### 2.1 Основные debuff ID (травмы)

Канон: `HarveyOverhaulInjury/Core/Constants.cs` (`InjurySets`, `ModEntry.KnownTraumas`).

| Buff ID | Topic (базовый) | CP dialogue key | Пример строки |
|---------|-----------------|-----------------|---------------|
| `buffHurt` | `topicHurt` | `topicHurt` | `dialoguesHarveyInjury.json` |
| `buffBadlyHurt` | `topicBadlyHurt` | `topicBadlyHurt` | то же |
| `buffSprainedAnkle` | `topicSprainedAnkle` | `topicSprainedAnkle` | то же |
| `buffBruisedRibs` | `topicBruisedRibs` | `topicBruisedRibs` | то же |
| `buffBackStrain` | `topicBackStrain` | `topicBackStrain` | то же |
| `buffDeepCuts` | `topicDeepCuts` | `topicDeepCuts` | то же |
| `buffBurnWounds` | `topicBurnWounds` | `topicBurnWounds` | то же |
| `buffInfectedWound` | `topicInfectedWound` | `topicInfectedWound` | то же |
| `buffTornMuscles` | `topicTornMuscles` | `topicTornMuscles` | то же |
| `buffConcussion` | `topicConcussion` | `topicConcussion` | то же |
| `buffFracturedBone` | `topicFracturedBone` | `topicFracturedBone` | то же |
| `buffShrapnelWounds` | `topicShrapnelWounds` | `topicShrapnelWounds` | то же |
| `buffSurgicalWound` | `topicSurgicalWound` | `topicSurgicalWound` | то же |
| `buffCold` | `topicCold` | `topicCold` | то же |

**Фазовые topics (C# генерирует):** `topic{Injury}Phase{Acute|Healing|Recovery}`  
Пример: `topicDeepCutsPhaseAcute` — `Core/Constants.cs:362-369` (`TopicIds.GetPhaseTopicId`).

**Фазовые buffs (примеры):** `HarveyMod_DeepCuts_Acute`, `HarveyMod_FracturedBone_Cast`, `HarveyMod_Concussion_Rest` — `Managers/InjuryManager.cs`.

### 2.2 Осложнения

| Buff ID | Topic | Treatment-needed key |
|---------|-------|---------------------|
| `HarveyMod_WetBandage` | `topicHarvey_WetBandage` | `HarveyMod_TreatmentNeeded_WetBandage` |
| `HarveyMod_DirtyWound` | `topicHarvey_DirtyWound` | `HarveyMod_TreatmentNeeded_DirtyWound` |
| `HarveyMod_WetStitches` | `topicHarvey_WetStitches` | `HarveyMod_TreatmentNeeded_WetStitches` |
| `HarveyMod_AllergicRash` | `topicHarvey_AllergicRash` | `HarveyMod_TreatmentNeeded_AllergicRash` |
| `HarveyMod_PainFlare` | `topicHarvey_PainFlare` | `HarveyMod_TreatmentNeeded_PainFlare` |
| `HarveyMod_Neglect` | `topicHarvey_Neglect` | `HarveyMod_TreatmentNeeded_Neglect` |

Константы: `Core/Constants.cs:128-177`.

### 2.3 Situational / ограничения

| Buff ID | Topic | Назначение | Источник |
|---------|-------|------------|----------|
| `HarveyMod_MineForbidden` | — | Жёсткий запрет шахты | `InjuryBuffs.MineForbidden` |
| `HarveyMod_MineRestricted` | — | Мягкое ограничение | `InjuryBuffs.MineRestricted` |
| `HarveyMod_Hospitalized` | — | Госпитализация | `StatusBuffs.Hospitalized` |
| `HarveyMod_DoctorVisitNeeded` | — | Напоминание о визите | `ReminderBuffs.DoctorVisitNeeded` |
| `buffFarmerExhausted` | `topicFarmerExhausted` | Истощение stamina | `PassOutHandler.cs` |
| `buffSleepy` | `topicPassedOutInTown` | Поздний обморок в Town | `PassOutHandler.cs` |
| `buffTooCold` | `topicTooCold` | Холод (Injury, не stress) | `PlayerEventHandler.cs` |
| `buffEmergencySupervision` | — | Gate для story E9 | CP `events.json` |
| `buffStrictSupervision` | — | Neglect care (CP-only ref) | docs |

### 2.4 Cured / health damage topics

| Topic | Источник |
|-------|----------|
| `topic{Cold,SurgicalWound,...}Cured` | `TopicIds.GetCuredTopic` |
| `topicHealthDamageCritical` | `Constants.cs:196` |
| `topicHealthDamageSevere` | `Constants.cs:197` |
| `topicPostOperativeCare` | `Constants.cs:198` |
| `topicOverprotectiveMode` | CP-only; C# **не создаёт** (`Helpers/MineForbiddenHelper.cs:34`) |

### 2.5 Trigger IDs (AppliedTriggers cooldown)

Шаблон: `{{ModId}}_trigger{Name}` — `Constants.cs:805-829`.

`triggerHurt`, `triggerBadlyHurt`, `triggerSprainedAnkle`, `triggerBruisedRibs`, `triggerBackStrain`, `triggerDeepCutsCombat`, `triggerDeepCutsFarming`, `triggerBurnWounds`, `triggerInfectedWound`, `triggerTornMuscles`, `triggerConcussion`, `triggerFracturedBone`, `triggerShrapnelWounds`, `triggerSurgicalWound`, `triggerExplosionInjury`, `triggerCold`.

### 2.6 Programmatic dialogue keys (Injury C#)

Префиксы из `Managers/DialogueManager.cs`, `docs/id-naming-standard.md`:

| Паттерн | Пример | Назначение |
|---------|--------|------------|
| `TreatmentStart_{Injury}_*` | `TreatmentStart_DeepCuts_01` | Старт лечения |
| `PhaseTransition_{Injury}_{n}` | `PhaseTransition_Concussion_3` | Смена фазы |
| `RecoveryComplete_{Injury}_*` | `RecoveryComplete_FracturedBone_01` | Выздоровление |
| `ComplicationTreatment_{Name}_*` | `ComplicationTreatment_WetBandage_01` | Осложнения |
| `Treat_{Injury}_Before/After{n}` | `Treat_DeepCuts_Before1` | Лечебный сеанс |
| `Proximity_*` | `Proximity_Injury_Untreated_High` | Proximity bubbles |
| `HarveyCareTrust_{Context}_{Trust}_{Relationship}_XX` | `HarveyCareTrust_TreatmentStart_High_Dating_01` | CareTrust |

**Proximity data (CP Load, не Dialogue):**
- `Data/HarveyOverhaul/HarveyProximityInjuryDialogue` ← `dialogues/harvey_proximity_injury.json`
- `Data/HarveyOverhaul/HarveyProximityPregnancyDialogue` ← `dialogues/harvey_proximity_pregnancy.json`

---

## 3. Clinic / appointment — лечение у Харви

### 3.1 Cure / treatment buffs

| Buff ID | Константа | Назначение |
|---------|-----------|------------|
| `buffHarveyTreatment` | `CureBuffs.Treatment` | Простое лечение |
| `buffHarveyIntensiveCare` | `CureBuffs.IntensiveCare` | Тяжёлое |
| `HarveyMod_BadlyHurt_OutpatientCare` | `CureBuffs.BadlyHurtOutpatientCare` | Амбулаторно |
| `buffHarveyProtection` | `CureBuffs.Protection` | Защита |
| `buffHarveyRecovery` | `CureBuffs.Recovery` | Recovery buff |
| `buffTeracitin` | `CureBuffs.Teracitin` | Медикамент |
| `buffAntibioticsTreatment` | `CureBuffs.Antibiotics` | Антибиотики |
| `buffForcedSedation` | `CureBuffs.ForcedSedation` | Седация |
| `buffPostSurgicalCare` | `CureBuffs.PostSurgical` | После операции |
| `buffHarveyCare` | `CureBuffs.Care` | Забота после лечения |
| `buffHarveyRehab` | `CureBuffs.Rehab` | Реабилитация |

Файл: `Core/Constants.cs:22-42`.

### 3.2 Treatment-needed topics (клик по Харви → `$action`)

Шаблон: `HarveyMod_TreatmentNeeded_{InjuryName}` — `TopicIds.GetTreatmentNeededTopic`.

**14 травм + 6 осложнений** — см. `dialoguesHarveyTreatmentNeeded.json`, `dialoguesHarveyTreatmentNeededClose.json` (Hearts 8–10 Friendly), `dialoguesHarveyTreatmentNeededRomantic.json` (Dating/Engaged/Married).

Пример:
```json
"HarveyMod_TreatmentNeeded_DeepCuts": "...#$action HarveyOverhaulInjury_StartTreatment buffDeepCuts"
```
Файл: `dialoguesHarveyTreatmentNeededRomantic.json:16`

### 3.3 Prescription topics

| Topic | Prescription ID |
|-------|-----------------|
| `topicHarvey_Prescription_Rest` | `HarveyMod_Prescription_Rest` |
| `topicHarvey_Prescription_NoMine` | `HarveyMod_Prescription_NoMine` |
| `topicHarvey_Prescription_KeepDry` | `HarveyMod_Prescription_KeepDry` |
| `topicHarvey_Prescription_LightWork` | `HarveyMod_Prescription_LightWork` |
| `topicHarvey_Prescription_Checkup` | `HarveyMod_Prescription_Checkup` |
| `topicHarvey_PrescriptionViolation` | — |
| `topicHarvey_PrescriptionFollowed` | — |

### 3.4 Compliance / CareTrust topics

| Topic | Назначение |
|-------|------------|
| `topicHarvey_ComplianceHigh/Neutral/Low` | Соблюдение лечения |
| `topicHarvey_StrictMedicalMode` | Усиленный контроль |
| `topicHarvey_TrustedPatient` | Стабильное соблюдение |
| `HarveyMod_CareTrust_Low/Medium/High` | Скрытое мед. доверие для CP |

### 3.5 Checkup / treatment plan topics

| Topic | Назначение |
|-------|------------|
| `topicHarvey_CheckupDue` | Контрольный осмотр |
| `topicHarvey_RecoveryCheckupDue` | Recovery checkup |
| `topicHarvey_CheckupPhase{1-3}` | Фазовый осмотр |
| `topicHarvey_CheckupDue_{Injury}` | По травме |
| `topicHarvey_RecoveryCheckupDue_{Injury}` | Recovery по травме |
| `topicHarvey_TreatmentPlanGiven` | План выдан |
| `topicHarvey_TreatmentPlan_{Injury}` | План по травме |
| `topicTreatment{Injury}` | Курс лечения (шаблон) |

### 3.6 Treatment pipeline topics

| Topic | Event / действие |
|-------|------------------|
| `topicHarveyNeedsFirstTreatment` | → `HarveyMod_FirstTreatment` |
| `topicFirstTreatmentComplete` | после FirstTreatment |
| `topicDiagnosisComplete` | → `HarveyMod_TreatmentPlanMeeting` |
| `topicTreatmentCompleted` | завершение курса |
| `topicHarveyMandatoryCheckup` | → `eventHarveyMorningCheckup` |
| `topicAgreedCheckup` / `topicRefusedCheckup` | care chain |
| `topicHarvey_EscalatedCare` | `dialoguesHarveyCure.json` |
| `topicHarvey_NightRound` | ночной обход |
| `topicHarvey_NightRoundSevereFirst` | → `eventHarveyNightRoundSevereFirst` |
| `topicHarvey_NightRoundFollowup` | после severe night round |
| `topicHarvey_ForcedHospitalization` | принудительная госпитализация |
| `topicHarvey_Rehab` / `_RehabStrict` / `_RehabCompleted` | реабилитация |
| `topicHarvey_SelfCare` / `_CleanBandage` / `_WarmTea` / `_SelfCarePraise` | самопомощь |

### 3.7 Clinic events (CP)

| Event ID | Файл | Условия (примеры) |
|----------|------|-------------------|
| `HarveyMod_FirstTreatment` | `events.json` | `topicHarveyNeedsFirstTreatment` |
| `HarveyMod_TreatmentPlanMeeting` | `events.json` | `topicDiagnosisComplete` |
| `eventHarveyMedicalCheck` | `events.json` | `!Dating Married` + mail reminder |
| `eventHarveyMedicalCheck_Dating` | `events.json` | `Dating Married` + mail |
| `eventHarveyTraumaExam` | `events.json` | — |
| `eventHarveyTreatmentCollapse` | `events.json` | collapse в Hospital |
| `eventStayInHospital` | `events.json` | госпитализация |
| `eventHarveyMorningCheckup` | `events.json` | `topicHarveyMandatoryCheckup` + Dating |
| `HarveyMod_NightCrisis_Dating` | `events.json` | Dating/Married + FirstTreatment seen |
| `HarveyMod_NightCrisis_PreDating` | `events.json` | `!Dating Married` |
| `HarveyMod_BirthdayHospital_Dating` | `events.json` | Dating/Married, summer 9 |
| `HarveyMod_BirthdayHospital_Friend` | `events.json` | `!Dating Married` |
| `eventHarveyAfterHospitalDischargeHome` | `eventsAfterHospitalDischarge.json` | `topicHarvey_AfterHospitalDischargeHome` |
| `eventHarveyNightRoundSevereFirst` | `eventsNightRoundSevere.json` | severe + Dating |
| `eventHarveyCareMovementAnimationTest` | `events.json` | debug |

C# events (launcher): `eventHarveyEmergencyCare`, `eventHarveyExhaustion` — `Core/Constants.cs:454-479`.

### 3.8 Clinic mail (выборка)

**C# отправляет:** `mailHarveySleepControl`, `mailHarveyMineForbidden`, `HarveyMod_DirtyWoundInfection`, `HarveyMod_WetBandageInfection`, `HarveyMod_TreatmentUrgentReminder`, `HarveyMod_TreatmentFinalWarning`, `HarveyMod_NeglectWarning` — `Core/MailIds`, `docs/audit-mail-csharp.md`.

**Tiered (суффиксы `_LowHearts` / `_MidHearts` / `_Dating` / `_Married`):**  
`mailHarveyTreatmentPlan_Minor/Severe`, `mailHarveyPrescriptionViolation`, `mailHarveyCheckupReminder`, `mailHarveyRehabReminder`, `mailHarveyRehabCompleted`, `mailHarveyNoMineViolation`, `mailHarveyKeepDryViolation`, `mailHarveyRestViolation`.

**CP-only (C# не шлёт):** `mailHarveyMedicalCheckReminder`, `mailHarveyIntensiveCare`, `mailHarveyModerateCare`, `HarveyMod_CheckupOverdue_*`, и др. — `mail.json`, `mailCure.json`, `mailInjury.json`.

---

## 4. Recovery Plan — режим восстановления

### 4.1 Lifecycle topics (C# → CP dialogue keys)

| Topic | CP dialogue | C# константа |
|-------|-------------|--------------|
| `HarveyMod_RecoveryPlanStarted` | `dialoguesHarveyRecoveryPlan.json:11` | `RecoveryPlanStarted` |
| `HarveyMod_RecoveryPlanViolated` | `:12` (fallback) | `RecoveryPlanViolated` |
| `HarveyMod_RecoveryPlanCompleted` | `:13` | `RecoveryPlanCompleted` |
| `HarveyMod_RecoveryPlanExtended` | `:27` | `RecoveryPlanExtended` |
| `HarveyMod_RecoveryPlanMaxExtensionsReached` | `:28` | `RecoveryPlanMaxExtensionsReached` |
| `HarveyMod_RecoveryPlanStrictFollowUpRequired` | `:29` | `RecoveryPlanStrictFollowUpRequired` |
| `HarveyMod_RecoveryPlanSoftTone` | `:23` | `RecoveryPlanSoftTone` |
| `HarveyMod_RecoveryPlanPerfect` | `:21` | `RecoveryPlanPerfect` |

### 4.2 Violation by type

| Topic | Violation type (C#) | Reason ID (C# internal) |
|-------|---------------------|-------------------------|
| `HarveyMod_RecoveryPlanViolated_Mine` | `Mine` | `entered_mine`, `entered_volcano` |
| `HarveyMod_RecoveryPlanViolated_LowStamina` | `LowStamina` | `stamina_too_low`, `heavy_work` |
| `HarveyMod_RecoveryPlanViolated_LowHealth` | `LowHealth` | `health_too_low` |
| `HarveyMod_RecoveryPlanViolated_LateNight` | `LateNight` | `too_late` |
| `HarveyMod_RecoveryPlanViolated_Rain` | `Rain` | `rain_bandage` |

**Нет отдельного CP topic для:** `IgnoredCheckup` (`missed_harvey_checkup`), `PassedOut` (`passed_out`) — fallback на `HarveyMod_RecoveryPlanViolated` + severity topics.

### 4.3 Violation by severity

| Topic | C# |
|-------|-----|
| `HarveyMod_RecoveryPlanViolated_Mild` | `RecoveryPlanViolatedMild` |
| `HarveyMod_RecoveryPlanViolated_Medium` | `RecoveryPlanViolatedMedium` |
| `HarveyMod_RecoveryPlanViolated_Severe` | `RecoveryPlanViolatedSevere` |
| `topicHarveyRecoveryViolationMild/Medium/Severe` | `RecoveryViolationTopics.cs` |

### 4.4 Completion variants

| Topic | Outcome |
|-------|---------|
| `HarveyMod_RecoveryPlanCompleted_Perfect` | идеально |
| `HarveyMod_RecoveryPlanCompleted_WithWarnings` | с предупреждениями |
| `HarveyMod_RecoveryPlanCompleted_Normal` | нормально |

### 4.5 Internal reason codes (не topics — только C# save)

`Core/RecoveryPlanReasonIds.cs`:  
`entered_mine`, `entered_volcano`, `stamina_too_low`, `health_too_low`, `too_late`, `heavy_work`, `rain_bandage`, `missed_harvey_checkup`, `passed_out`, `unknown`.

**CP-safe:** только если C# выставил соответствующий **topic** (см. таблицы выше).

---

## 5. Rescue / collapse — обмороки и спасение

### 5.1 Pass-out / exhaustion

| Buff / Topic | Event | Mail | Источник |
|--------------|-------|------|----------|
| `buffFarmerExhausted` + `topicFarmerExhausted` | `eventHarveyExhaustion` | — | Injury C# |
| `buffSleepy` + `topicPassedOutInTown` | — | `mailHarveySleepControl` (+ tier) | Injury C# + `triggersInjuryMail.json` |
| `topicHarvey_MorningAfterExhaustion` | `eventHarveyMorningAfterExhaustion` | — | `eventsMorningAfterExhaustion.json` |
| `topicHarvey_ExhaustionFollowup` | — | — | после morning event |
| `topicHarveyExhaustion` | — | — | CP `harvey_topics_medical.json` |
| — | `eventHarveyLateNightCollapse` | — | CP Town 24–26 |
| — | `eventHarveyEmergencyCare` | — | critical pass-out |
| — | `eventHarveyTreatmentCollapse` | — | Hospital collapse |

**Trigger (BETAS, не vanilla CP):**  
`{{ModId}}_triggerSleepControl_{Neutral,Friend,Dating,Married}` — `Spiderbuttons.BETAS_PassedOut` → `topicPassedOutInTown` — `triggersInjuryMail.json`.

### 5.2 Mine rescue

| Topic | Event | Примечание |
|-------|-------|------------|
| `topicMineRescuePending` | блокирует CP | C# готовит cutscene |
| `topicMineInjuryRescue` | — | после rescue |
| `topicHarveyMinorMineRescue` | `eventHarveyMinorMineRescue` | minor rescue |
| `HarveyMineIntercept` | `eventHarveyMineInterception` | cooldown 3 д |
| — | `eventHarveyMineRescue` | legacy Mine scene |
| — | `eventHarveyMineRescueDating` | legacy dating |
| — | `eventHarveyMineRescueMorning` | утро, non-dating |
| — | `eventHarveyMineRescueMorningDating` | утро, dating |

Файлы: `eventsMineRescue.json`, `eventsCare.json` (interception).  
Mail: `mailHarveyAfterMineRescue`, `mailHarveyMineForbidden`, `mailHarveyMineWarning`.

### 5.3 External / woods rescue

| Topic | Event |
|-------|-------|
| `topicHarvey_ExternalRescueConcern` | `eventHarveyAfterExternalRescueHome` |
| `topicHarvey_AfterExternalRescueCooldown` | cooldown |
| `topicRescueOperation` | `eventRescueOperation` (Woods) |
| `topicRescueComplete` | после rescue op |
| `HarveyMod_CD_RescueOperation` | cooldown 14 д |

Engaged/Married variants: `eventHarveyAfterExternalRescueHomeEngaged`, `_HomeMarried` — `eventsExternalRescue.json`.

### 5.4 Hospital discharge

| Topic | Event |
|-------|-------|
| `topicHarvey_AfterHospitalDischargeHome` | `eventHarveyAfterHospitalDischargeHome` |
| `HarveyMod_HospitalDischarged` | — |
| `topicHarvey_HospitalDischargeFollowup` | follow-up 3 д |

### 5.5 Gotoro forest rescue (StressMeter)

| Topic | Event |
|-------|-------|
| `topicStressGotoroFlashbackActive` | gate для rescue |
| `topicStressGotoroForestRescuePending` | anti-dupe |
| `topicStressGotoroForestRescueDone` | CP only (`eventsGotoroForestRescue.json`); C# **не создаёт** |
| `topicStressTrust_RescueMidTrust/HighTrust/Dating/Married` | post-rescue follow-up |

Events: `HarveyStress_GotoroForestRescue_{MidTrust,HighTrust,Dating,Married}` — `Constants/FlashbackRescueEventIds.cs`.

---

## 6. Stress — страхи и стресс

### 6.1 Активные stress debuffs (HarveyStressMeter C# pipeline)

| Страх | Buff | Gate topic | Quest | Mail |
|-------|------|------------|-------|------|
| Гроза | `buffStressThunder` | `topicStressThunder` | `HarveyMod_ThunderRecovery` | `mailHarveyStressTreatmentThunder` |
| Темнота | `buffStressDarkness` + `buffDarknessLevel1/2/3` | `topicStressDarkness`, `topicStressDarknessLevel2/3` | `HarveyMod_DarknessStep1/2/3`, `HarveyMod_DarknessProphylaxis` | `mailHarveyStressTreatmentDarkness`, `mailHarveyDarknessWorry` |
| Социальная тревожность | `buffStressSocial` | `topicStressSocial` | `HarveyMod_SocialRecovery` | `mailHarveyStressTreatmentSocial` |
| Переутомление | `buffStressOverwork` | `topicStressOverwork` | `HarveyMod_OverworkRecovery` | `mailHarveyStressTreatmentOverwork` |
| Усталость | `buffStressTired` | `topicStressTired` | `HarveyMod_TiredRecovery` | `mailHarveyStressTreatmentTired` |
| Одиночество | `buffStressLonely` | `topicStressLonely` | `HarveyMod_LonelyRecovery` | `mailHarveyStressTreatmentLonely` |
| Голод | `buffStressHunger` | `topicStressHunger` | `HarveyMod_HungerRecovery` | `mailHarveyStressTreatmentHunger` |
| Холод | `buffStressTooCold` | `topicStressTooCold` | `HarveyMod_TooColdRecovery` | `mailHarveyStressTreatmentTooCold` |
| Недосып | `buffStressNoSleep` | `topicStressNoSleep` | `HarveyMod_NoSleepRecovery` | `mailHarveyStressTreatmentNoSleep` |

Канон buffs: `HarveyStressMeter/Constants/BuffIds.cs`.  
Implemented pipeline: `TreatmentTopics.ImplementedBuffIds` — 9 buffs (Social, Tired, Overwork, NoSleep, Lonely, Hunger, TooCold, Thunder, Darkness).

### 6.2 Treatment followup / review topics (C# ставит после consent)

Шаблон: `topicStressTreatment{BuffName}Followup`, `topicStressTreatment{BuffName}ReadyForReview`, `topicStressTreatment{BuffName}Cured`.

**Активны в CP:** `dialoguesHarveyCureStress.json` (включён в `content.json:146-148`).

**Обнулены CP-патчем** (`stressTopicsUnimplementedDisabled.json`): базовые `topicStress*` и legacy `*Started/*Followup` для implemented buffs — C# использует programmatic dialogues, не CP topic keys для gate.

### 6.3 Darkness therapy chain (C# topics)

| Topic | Файл |
|-------|------|
| `topicDarknessTherapyStart` | `GameLogicHandler.cs:538` |
| `topicDarknessStep1Complete` / `Step2Complete` | `DarknessService.cs` |
| `topicDarknessLanternReceived` | `DarknessService.cs` |
| `topicDarknessFullyCured` | `DarknessService.cs` |
| `topicDarknessStep1ReadyForHarvey` | `DarknessLegacyHelper.cs` |
| `topicStressDarknessSerious` / `Phobia` | `DarknessService.cs` |
| `topicHarveyDarknessWorryLetterSent` | cooldown письма |

### 6.4 Storm comfort (Injury launcher + CP events)

| ID | Роль |
|----|------|
| `buffStressThunder` OR `topicHarveyStormStress` | gate |
| `HarveyMod_CD_StormComfort` | cooldown после comfort |
| `topicStressThunder` | legacy fallback |
| `eventHarveyStormComfort{Farm,Forest,Town,Mine,Mountain,Desert}` | cutscenes |

Константы: `StormComfortIds` — `Core/Constants.cs:293-305`.

### 6.5 Support buffs / service topics

| Buff / Topic | Назначение |
|--------------|------------|
| `buffStressImmunity` | cooldown после storm comfort |
| `HarveyStress.CareAura` | aura near Harvey |
| `buffCalmingAtHospitalWithHarvey` | thunder at clinic |
| `buffHarveyLantern` / `buffDimLight` / `buffDarknessOvercome` | darkness therapy |
| `buffRestingAtHome` / `buffOverworkBreak` | treatment buffs |
| `topicOverworkBreakActive` / `Interrupted` | overwork breaks |
| `topicSpeakToSomebody` / `topicEatSomething` | служебные gate |
| `topicStressTreatmentStarted` | служебный маркер C# |

### 6.6 Stress episodes (programmatic keys)

Episodes: `PhysicalExhaustion`, `Burnout`, `AnxietySpike`, `GotoroFlashback`, `SocialShutdown` — `Constants/StressEpisodes.cs`.

Dialogue keys (`stress_flow_dialogues.json`):  
`episode_{Episode}_start`, `episode_{Episode}_review`, `ambient_{Cause}`, `trust_early_professional`, `trust_trusted_doctor`, `trust_safe_person`, `trust_dating_grounding`, `trust_married_anchor`, `trust_rescue_{Tier}`, `social_anxiety_review`.

### 6.7 Disabled / unimplemented stress (есть в JSON, не активны)

**Buffs в `BuffIds.cs`, но без C# pipeline:**  
`buffStressCriticism`, `BadDream`, `Panic`, `SleepDeprivation`, `AnxietyWave`, `MentalFatigue`, `ShadowParanoia`, `FreezeResponse`, `Isolation`, `Breakdown`, `Collapse`, `Numbness`, `Despair`.

**Topics обнулены в CP:** `stressTopicsUnimplementedDisabled.json` — `topicStressCriticism`, `topicStressBadDream`, …, `topicStressCollapse`, и все `topicStressTreatment*Started/Followup/Cured` для implemented buffs.

**`dialoguesHarveyStress.json` закомментирован** в `content.json:207-211` — стартовые stress-диалоги с `$action HarveyStress_StartTreatment` **не загружаются**.

**JSON-only buffs (нет в `BuffIds.cs`):** `buffSleepDeprivation`, `buffStressCritical` — `assets/stress_dialogues.json`.

### 6.8 NPC stress reactions (другие NPC)

`dialoguesNpc.json`: `topicStressHunger`, `topicAcceptHospital` — реакции не-Harvey NPC.

---

## 7. Harvey relationship — Dating / Engaged / Married

### 7.1 CP relationship tokens

| Token | Использование |
|-------|---------------|
| `Relationship:Harvey: Dating` | диалоги, events |
| `Relationship:Harvey: Engaged` | `dialoguesHarveyTreatmentNeededRomantic.json`, `eventsExternalRescue.json` |
| `Relationship:Harvey: Married` | married split files, pregnancy triggers |
| `GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating` | events |
| `GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married` | combined gate |
| `GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Engaged` | external rescue engaged |
| `GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married` | pre-dating content |

**C# tier (не CP):** `HarveyHelper.GetHarveyRelationshipTier()` → `LowHearts/MidHearts/HighHearts/Dating/Married` — `Helpers/HarveyHelper.cs`.

### 7.2 Split events by relationship

| Pre-dating | Dating / Married |
|------------|------------------|
| `eventHarveyMedicalCheck` | `eventHarveyMedicalCheck_Dating` |
| `HarveyMod_NightCrisis_PreDating` | `HarveyMod_NightCrisis_Dating` |
| `HarveyMod_BirthdayHospital_Friend` | `HarveyMod_BirthdayHospital_Dating` |
| `eventHarveyMineRescueMorning` | `eventHarveyMineRescueMorningDating` |
| `eventHarveyAfterExternalRescueHome` | `_HomeEngaged`, `_HomeMarried` |
| `HarveyOverhaulStory.E10_HarveyWasWrong` | `E10_HarveyWasWrong_Dating` |
| `HarveyOverhaulStory.E12_HarveyIsTired` | `E12_HarveyIsTired_Dating` |
| `HarveyOverhaulStory.E15_FuturePlan` | `E15_FuturePlan_Married` |

**Engaged:** явно только в `eventHarveyAfterExternalRescueHomeEngaged` и romantic treatment dialogues; большинство gates — binary Dating/Married vs rest.

### 7.3 Care / trust / story topics (CP-only, не Injury/Stress state)

**Visits / walks:** `topicHarveyFirstVisitAgree/Refused/Neutral`, `topicHarveySecondVisitAgree/Refused/Neutral`, `topicHarveyAcceptFirstWalk`, `topicHarveyDeclineFirstWalk`, `topicHarveyWalkGood/Neutral/Bad`.

**Trust arc:** `topicHarveyTrust_{Water,Breakfast,Rest,DoctorDecides,FullExam,SmallExam,JustSit,LeftClinic,TouchOk,BreathHard,NeedsSpace,PublicCare}`, `topicHarveyTrustFinal`, `topicHarveyCareAgreement`.

**Storm story:** `topicHarveyStorm_{Clinic,Home,Note,Escort}`.

**Door / bad day:** `topicHarveyDoorSignal_{Close,Boundary,Note}`, `topicHarveyBadDay_{Silent,Water,NoQuestions}`.

**Dating:** `topicHarveyDate_{Kiss,NotYet,Hug,SitLonger}`, `topicHarveyFuturePlan`, `_CheckIn`, `_StillLearning`, `_TeaSignal`, `topicHarveyNotOnlyPatient`.

**Mines:** `topicHarveyMines_{ReturnTime,Supplies,Note,CallMe}`, `topicHarveyHelp_{Asks,Spotter,Independent}`.

**Cooldown bridge:** `HarveyMod_CD_Global`, `HarveyMod_CD_E1`…`E15`, `HarveyMod_CD_E2B`, `E3B`, `E4B`, `E7`, `E9`, `E11`, `E13`, `HarveyMod_CD_RomE1`, `HarveyMod_CD_StormComfort`.

**Misc:** `topicFirstMeeting`, `topicHarveyTraumaReveal`, `topicHarveyWasCaredFor`, `topicHarveyProximityReaction/Strict/Praise`, `topicNightCrisisComplete`, `topicBirthdayHospitalComplete`.

Файлы: `events.json`, `eventsCare.json`, `harvey_topics_medical.json`, `harvey_dating.json`, `harvey_married.json`.

### 7.4 Tiered mail / dialogue by relationship

Суффиксы `_LowHearts`, `_MidHearts`, `_Dating`, `_Married` на: treatment plan, prescription violation, checkup, rehab mails — `mailHarveyMedicalTiered.json`, `mailCure.json`.

Recovery plan dialogues split by `Hearts:Harvey` and `Relationship:Harvey Dating/Married` — `dialoguesHarveyRecoveryPlan.json`.

---

## 8. $action-команды

### HarveyOverhaulInjury

| Action | Аргумент | Handler | Пример |
|--------|----------|---------|--------|
| `HarveyOverhaulInjury_StartTreatment` | `buff{Injury}` | `TreatmentStartHandler` | `$action HarveyOverhaulInjury_StartTreatment buffDeepCuts` |
| `HarveyOverhaulInjury_TreatComplication` | `HarveyMod_{Complication}` | `TreatmentStartHandler` | `$action HarveyOverhaulInjury_TreatComplication HarveyMod_WetBandage` |

Константы: `Core/Constants.cs:308-314`.  
**TODO (не реализовано):** `HarveyOverhaulInjury_AdvancePhase`, `HarveyOverhaulInjury_CompleteRecovery`.

Файлы CP: `dialoguesHarveyTreatmentNeeded*.json`.

### HarveyStressMeter

| Action | Аргумент | Назначение |
|--------|----------|------------|
| `HarveyStress_StartTreatment` | `buffStress*` | consent → start treatment |
| `HarveyStress_CompleteReview` | `buffStress*` | finish review |
| `HarveyStress_SocialAnxiety_Start` | — | social therapy start |
| `HarveyStress_SocialAnxiety_StartReview` | — | social follow-up |
| `HarveyStress_SocialAnxiety_Complete` | — | social quest complete |

Константы: `HarveyStressMeter/Constants/HarveyStressActions.cs`.  
Handler: `Services/HarveyStressActionHandler.cs`.

**Активные CP refs:** `dialoguesHarveyCureStress.json` (`CompleteReview`, `SocialAnxiety_Complete`).  
**Отключены:** `dialoguesHarveyStress.json` (`StartTreatment`).

---

## 9. Отсутствующие, но полезные ID

Для NPC-реакций **нет** CP-safe маркеров — только internal C# state. Предложения ниже **не существуют** в коде на момент аудита.

### 9.1 Injury / Recovery Plan

| Желаемый маркер | Зачем | Что есть сейчас |
|-----------------|-------|-----------------|
| `topicHarvey_RecoveryPlanActive` | NPC видит «режим идёт» без нарушения | Только `HarveyMod_RecoveryPlanStarted` (3 д) или C# `RecoveryPlanState.IsActive` |
| `HarveyMod_RecoveryPlanViolated_IgnoredCheckup` | пропуск осмотра | C# type `IgnoredCheckup`, fallback topic |
| `HarveyMod_RecoveryPlanViolated_PassedOut` | обморок во время режима | C# type `PassedOut`, fallback topic |
| `HarveyMod_RecoveryPlanViolated_Volcano` | вход в вулкан | reason `entered_volcano` → topic `_Mine` |
| `topicHarvey_MainInjury_{Injury}` | стабильный gate по главной травме | `MainInjuryId` только в save |
| `topicHarvey_InTreatment` | идёт активное лечение (фаза > 0) | `DebuffState.TreatmentStarted` — save only |
| `topicHarvey_Hospitalized` | в госпитали | buff `HarveyMod_Hospitalized` ✅ (есть) |
| `topicHarvey_PhaseReadyForCheckup` | пора на осмотр | `ReadyForNextPhase` — save only |
| `HarveyMod_CareTrust_*` bridge по numeric score | granular trust | только Low/Medium/High topics |

### 9.2 Stress

| Желаемый маркер | Зачем | Что есть сейчас |
|-----------------|-------|-----------------|
| `topicStress_ActiveTreatment` | любой stress-курс идёт | `HasActiveTreatment` — save only |
| `topicStress_AwaitingReview` | ждёт review у Харви | `AwaitingHarveyReview` — save only |
| `topicStressLoad_{Calm,Mild,High,Critical}` | общий уровень stress meter | `StressLoadState.CurrentStressLoad` — save only |
| `topicStress_WarTrauma` | хроническая war trauma | `WarTraumaFlag` — save only |
| `topicStress_SocialTherapyPhase_*` | фаза social anxiety timer | `SocialAnxietyTherapyState.Phase` — save only |
| Базовые `topicStress*` для CP NPC | реакции villagers на stress | **обнулены** `stressTopicsUnimplementedDisabled.json` |
| `topicStressGotoroForestRescueDone` | после Gotoro rescue | упомянут в test plan, **C# не создаёт** |

### 9.3 Rescue / collapse

| Желаемый маркер | Зачем | Что есть сейчас |
|-----------------|-------|-----------------|
| `topicHarvey_PassedOutYesterday` | generic «вчера обморок» | `PassedOutInTownYesterday` — save only |
| `topicHarvey_MineRescuePending` | alias | `topicMineRescuePending` ✅ |
| `topicHarvey_ExhaustedNotInjured` | истощение без травмы | partial: `topicFarmerExhausted` |

### 9.4 Relationship

| Желаемый маркер | Зачем | Что есть сейчас |
|-----------------|-------|-----------------|
| Explicit `Engaged` split dialogues | больше engaged-контента | mostly binary Dating/Married |
| `topicHarvey_MedicalPartner` | married medical tone | partial via married files |
| Vanilla-style `flag` для story forks | CP простые gates | только topics + `eventsSeen` |

---

## 10. Статус активации контента

| Файл / система | Статус | Примечание |
|----------------|--------|------------|
| `dialoguesHarveyInjury.json` | ✅ включён | `content.json:156-158` |
| `dialoguesHarveyRecoveryPlan.json` | ✅ включён | `content.json:186-188` |
| `dialoguesHarveyTreatmentNeeded*.json` | ✅ включён | Late+1/+2 |
| `dialoguesHarveyCureStress.json` | ✅ включён | followup/review stress |
| `dialoguesHarveyStress.json` | ❌ закомментирован | start treatment CP dialogues |
| `stressTopicsUnimplementedDisabled.json` | ✅ включён | nullifies stress topic keys |
| `triggersInjuryMail.json` | ✅ включён | BETAS pass-out |
| `triggersStress.json` | ❌ закомментирован | если есть в content.json |
| Storm comfort events | ✅ активны | Injury C# launcher + CP |
| Stress 9-buff pipeline | ✅ HarveyStressMeter | programmatic + cure stress dialogues |
| Stress unimplemented buffs | ❌ disabled | JSON exists, C# cleanup only |
| `HarveyMod_RecoveryComplete` event | ❌ закомментирован | `events.json` |

---

## Быстрый справочник: что использовать для NPC-реакций

| Сценарий | Рекомендуемые CP-safe gates |
|----------|----------------------------|
| Игрок ранен | `PLAYER_HAS_BUFF Current buff*` OR `HasConversationTopic topic*` |
| Нужно лечение | `HasConversationTopic HarveyMod_TreatmentNeeded_*` |
| Режим восстановления | `HasConversationTopic HarveyMod_RecoveryPlanStarted` / `*Violated*` / `*Completed*` |
| Нарушил режим (тип) | `HasConversationTopic HarveyMod_RecoveryPlanViolated_Mine` etc. |
| Обморок / истощение | `topicPassedOutInTown`, `topicFarmerExhausted`, `topicHarvey_MorningAfterExhaustion` |
| Mine rescue | `topicMineInjuryRescue`, `topicMineRescuePending`, `mailHarveyAfterMineRescue` |
| Stress (активный) | `PLAYER_HAS_BUFF Current buffStress*`; followup: `topicStressTreatment*Followup` |
| Гроза | `buffStressThunder` OR `topicHarveyStormStress`; NOT `HarveyMod_CD_StormComfort` |
| Dating Harvey | `Relationship:Harvey: Dating` OR `PLAYER_NPC_RELATIONSHIP … Dating` |
| Engaged | `Relationship:Harvey: Engaged` (редко) |
| Married | `Relationship:Harvey: Married` |
| Pre-dating medical | `!PLAYER_NPC_RELATIONSHIP … Dating Married` + Hearts tiers |

---

*Аудит основан только на файлах трёх репозиториев. ID не выдуманы; если маркер отсутствует в таблицах выше — его нет в проекте.*
