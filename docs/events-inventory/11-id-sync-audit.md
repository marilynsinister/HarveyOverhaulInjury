# Синхронизация ID: C# InjuryCare ↔ Content Patcher

Аудит соответствия идентификаторов между SMAPI-модом и content pack. **Автоген 2026-05-24**.

- C# файлов: `32`
- CP JSON (`assets/Code/`): `30`
- Уникальных ID в таблице: **810**
- Проблемных строк: **276** (из них ❌: **0**)

## Проверки

| # | Правило | Результат |
|---|---|---|
| 1 | Phase buff IDs из `GetPhaseBuffId` → `Data/Buffs` | 0 отсутствуют — **все на месте** |
| 2 | Phase topic формат `topic{Injury}Phase{Acute|Healing|Recovery}` | C#: 33; ключи в dialogues: 33/33; альт. `PhaseTransition_*`: 16 |
| 3 | C# mail (`addMailForTomorrow`) → CP Mail | ❌ см. таблицу проблем |
| 4 | CP preconditions / When / event script | ⚠️ см. «CP не создаёт C#» |
| 5 | Completion topics `topic*Cured`, `topicTreatmentCompleted` | без диалога: 0 |
| 6 | Trigger IDs `{ModId}_trigger*` | C#: 16; CP TriggerActions: 18 |

## Phase buff: отсутствуют в CP

- *(все phase buff из InjuryManager найдены в buffsInjury/buffsCure)*

## Phase topics: C# формат без ключа в dialogues

- *(все 33 phase topic имеют ключи в dialoguesHarveyCure/Injury)*

## Две схемы phase-диалогов в CP

- **C# / Cure:** `topicDeepCutsPhaseAcute`, `topicDeepCutsPhaseHealing`, … — создаёт `GetPhaseTopicId`
- **Injury (legacy):** `PhaseTransition_DeepCuts_2`, `PhaseTransition_DeepCuts_3` — **C# не создаёт**; вероятно мёртвые ключи или старая схема

- Ключей `topic*Phase*` в dialogues: **45**
- Ключей `PhaseTransition_*`: **16**

## Критичные разрывы (❌)

- *(нет строк со статусом ❌)*

## Таблица проблем

| ID | Тип | Где создаётся | Где используется | Есть в CP? | Есть в C#? | Статус |
|---|---|---|---|---|---|---|
| `HarveyMod_BirthdayHospital` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_BirthdayHospital_Dating` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_BirthdayHospital_Friend` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E1` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E2` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E2B` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E3` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E3B` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E4` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E4B` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E5` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E6` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E7` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E8` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E9` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_Global` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_EarlyMorningCare` | buff | — | CP: triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_EmergencyNightWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_EmergencySupervision` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_LateNightWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_MineWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_NightCrisis` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_NightCrisis_Dating` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_NightCrisis_PreDating` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E1_SlipperyPath` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E2B_QuietAgreement` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E2_InsistentExam` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E3B_WingPatient` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E3_ForestApothecary` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E4B_TooQuiet` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E4_PierBreath` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E6_SayItOutLoud` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E8_QuietShelf` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E9_LightInWindow` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffEmergencySupervision` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffHarveyDropper` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff), eventsCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffStressImmunity` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff), events.json (ref:buff), events_for_mode_new_formatted.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffStressNoSleep` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffStressTired` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyCheckFarmerOutsideAfter22` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyCheckHealthFarmer` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyCheckup` | event | — | CP: eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstDate` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstMeeting` | event | — | CP: events.json (ref:event), eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstVisit` | event | — | CP: eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstWalk` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyLateNightCollapse` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMedicalCheck` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMedicalCheck_Dating` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMineInterception` | event | — | CP: eventsCare.json (ref:event), triggersCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMorningCheckup` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMountainDate` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyPropose` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyRoomCheckup` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyRoomCheckup2` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveySecondVisit` | event | — | CP: events.json (ref:event), eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveySkullCavePrevention` | event | — | CP: eventsCare.json (ref:event), triggersCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortDesert` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortFarm` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortForest` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortMine` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortMountain` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortTown` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyTraumaExam` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyTreatmentCollapse` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyAfterMineRescue` | mail | — | CP: eventsMineRescue.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyCaveWarning` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyIntensiveCare` | mail | — | CP: Data/Mail, mail.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyMedicalCheckReminder` | mail | — | CP: Data/Mail, events.json (ref:mail), mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyMineWarning` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyModerateCare` | mail | — | CP: Data/Mail, mail.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote1` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote2` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote3` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote4` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNoteWife` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyPostTrauma` | mail | — | CP: Data/Mail, eventsCare.json (ref:mail), mailCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicAcceptHospital` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicAfterCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicAgreedCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicAlcoholPoisoningPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicAlcoholPoisoningPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicAlcoholPoisoningPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBackStrainCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBirthdayHospitalComplete` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicBoyfriendWorries` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBruisedRibsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBurnWoundsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicConcernForHealth` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicConcussionCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessFullyCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessLanternReceived` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessStep1Complete` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessStep2Complete` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessTherapyStart` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDeepCutsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicEmotionalSupport` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicFatigue` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicFirstMeeting` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, dialoguesHarveyCare.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicFracturedBoneCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarveyAcceptFirstWalk` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyCareAgreement` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyDeclineFirstWalk` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyExhaustion` | topic | — | CP: dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisit` | topic | — | CP: eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisitAgree` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisitNeutral` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisitRefused` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyGentleCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyHelp_Asks` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyHelp_Independent` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyHelp_Spotter` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyIntensiveCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyLove` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarveyMandatoryCheckup` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyModerateCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyPierBreath` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisit` | topic | — | CP: eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisitAgree` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisitNeutral` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisitRefused` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Clinic` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Escort` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Home` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Note` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySupport` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarveyTooQuiet` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTraumaReveal` | topic | — | CP: dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrustFinal` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_Breakfast` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_BreathHard` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_DoctorDecides` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_NeedsSpace` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_PublicCare` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_Rest` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_TouchOk` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_Water` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWalkBad` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWalkGood` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, dialoguesNpc.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWalkNeutral` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWingPatient` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarvey_AllergicRash_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_AllergicRash_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_DirtyWound_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_DirtyWound_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_EscalatedCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_EscalatedCare_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_EscalatedCare_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_ForcedHospitalization_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_ForcedHospitalization_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_Neglect_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_Neglect_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_NightRound_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_NightRound_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetBandage_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetBandage_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetStitches_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetStitches_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHealthCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHope` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHusbandlyProtection` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicInfectedWoundCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicIntensiveTreatment` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicMedicalCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicMentalExhaustion` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicMentalHealth` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicNightCrisisComplete` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicOverprotectiveMode` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicPanicAttacks` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicPersonalStruggle` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicPreventiveCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicProtectiveBoyfriend` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicRefusedCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicRescueComplete` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicShrapnelWoundsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSleepIssues` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSprainedAnkleCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStartTreatment` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressAnxietyWave` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressBadDream` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressBreakdown` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, events_for_mode_new_formatted.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicStressCollapse` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, events_for_mode_new_formatted.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicStressCritical` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressCriticism` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDarkness` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDarknessLevel2` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDarknessLevel3` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDespair` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressFreezeResponse` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressHunger` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressIsolation` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressLonely` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressMentalFatigue` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressNoSleep` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressNumbness` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressOverwork` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressPanic` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressShadowParanoia` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressSleepDeprivation` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressSocial` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTired` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json, events_for_mode_new_formatted.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicStressTooCold` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentAnxietyWaveCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentAnxietyWaveStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBadDreamCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBadDreamStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBreakdownCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBreakdownStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCollapseCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCollapseStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCriticismCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCriticismStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDarknessCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDarknessStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDespairCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDespairStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentFreezeResponseCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentFreezeResponseStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentHungerCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentHungerStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentIsolationCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentIsolationStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentLonelyCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentLonelyStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentMentalFatigueCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentMentalFatigueStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNoSleepCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNoSleepStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNumbnessCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNumbnessStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentOverworkCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentOverworkStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentPanicCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentPanicStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentShadowParanoiaCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentShadowParanoiaStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSleepDeprivationCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSleepDeprivationStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSocialCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSocialStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentThunderCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentThunderStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTiredCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTiredStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTooColdCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTooColdStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSurgicalWoundPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSurgicalWoundPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSurgicalWoundPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTornMusclesCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTrauma` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentAgreement` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicTreatmentBackStrain` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentBruisedRibs` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentBurnWounds` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentCold` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentConcussion` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentDeepCuts` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentFracturedBone` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentInfectedWound` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentProgress` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentRefusal` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicTreatmentShrapnelWounds` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentSprainedAnkle` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentTornMuscles` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicWifelyWorries` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |

## Полная таблица

| ID | Тип | Где создаётся | Где используется | Есть в CP? | Есть в C#? | Статус |
|---|---|---|---|---|---|---|
| `HarveyMod_AdvancedTreatmentUnlocked` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_AlcoholWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), mailCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_AllergicRash` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_Aloe` | buff | — | CP: quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_AnkleInjuryAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_AnkleTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_AnniversaryReflection` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_AntiAnxietyEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_AntiAnxietyMed` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_AntibioticsTherapy` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_AnxietyTincture` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_AnxietyTinctureEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_AnxietyWaveRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_BackStrainAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_BackStrain_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_BackStrain_Recovery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_BackTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_BadDreamRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_BirthdayHospital` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_BirthdayHospital_Dating` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_BirthdayHospital_Friend` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_BracesClint` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_BreakdownRecovery` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_BreakdownRecoveryComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_BreakdownRecoveryProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_BreakdownRecoveryStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_BreathingDifficulty` | buff | — | CP: Data/Buffs, buffsInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_BreathingGuide` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_BruisedRibs_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_BruisedRibs_Healing` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_BurnTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_BurnWoundsAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_BurnWounds_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_BurnWounds_Healing` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_CD_E1` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E2` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E2B` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E3` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E3B` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E4` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E4B` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E5` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E6` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E7` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E8` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_E9` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_Global` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_CD_RescueOperation` | HarveyMod | — | C#: Core\Constants.cs | нет | да | OK |
| `HarveyMod_CD_StormComfort` | HarveyMod | — | C#: Core\Constants.cs; CP: events.json (ref:buff) | да | да | OK |
| `HarveyMod_CalmingEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_CalmingHerbs` | buff | — | CP: Data/Buffs, items.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_CalmingSoup` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_CalmingTea` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_CalmingTeaEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_CatatonicRecovery` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_CatatonicState` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_Cold_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_Cold_Recovery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_CollapseRehabilitation` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_CollapseRehabilitationComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_CollapseRehabilitationProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_CollapseRehabilitationStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_ComfortAmulet` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_ComfortLetter` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_CompleteBreakdown` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_ConcussionAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_ConcussionTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_Concussion_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_Concussion_Limited` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_Concussion_Rest` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_CookingHelpEvelyn` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_CriticalCareUnlocked` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_CriticalInjuryTreatment` | buff | — | CP: Data/Buffs, questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_CriticalRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_CriticismRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_DangerWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_DarknessRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_DarknessStep1` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_DarknessStep2` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_DarknessStep3` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_DarknessTherapy` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_DeepCutsAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_DeepCutsTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_DeepCuts_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_DeepCuts_Healing` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_DeepCuts_Recovery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_DeliveryGus` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_DespairIntervention` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_DespairInterventionComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_DespairInterventionProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_DespairInterventionStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_DirtyWound` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_DirtyWoundInfection` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_DoctorWorries` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_EarlyMorningCare` | buff | — | CP: triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_EmergencyHospitalization` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmergencyNightWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_EmergencyRelief` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmergencyResponse` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmergencySedative` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmergencySedativeEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmergencySupervision` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_EmotionalNumbnessTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmotionalStimulant` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmotionalStimulantEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_EmotionalThawing` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_EncouragementGeneral` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_EnergyRestorer` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_EscalationNotice` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_ExtendedTreatmentNotice` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_FamilySupport` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_FamilySupportJodi` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_FatigueTreatment` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_FatigueTreatmentComplete` | buff | — | CP: Data/Buffs, Data/Mail, mail.json (ref:buff), quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_FatigueTreatmentProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_FatigueTreatmentStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_FirstTreatment` | HarveyMod | — | C#: Core\Constants.cs, EventHandlers\PlayerEventHandler.cs, Managers\DialogueManager.cs; CP: events.json (ref:buff) | да | да | OK |
| `HarveyMod_FlowerBouquet` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_FractureAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_FractureTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_FracturedBone_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_FracturedBone_Cast` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_FracturedBone_Recovery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_FreezeResponseRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_FuturePlans` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_GardenCareCaroline` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_GentleCareStart` | buff | — | CP: Data/Buffs, quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_GroundingStone` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_HarveyTreatment` | buff | — | CP: Data/Buffs, questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_HealingBroth` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_HealthyHolidays` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_HealthyMeal` | buff | — | CP: Data/Buffs, items.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_HerbalHelpCaroline` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_HospitalizeRobin` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_HungerRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_ImpairedMobility` | buff | — | CP: Data/Buffs, buffsInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_InfectedWound_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_InfectedWound_Treatment` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_InfectionAlert` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_InfectionTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_InjureCareEvelyn` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_InjuryCommentRobin` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_InjuryWorryMaru` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_IntensiveCare` | buff | — | CP: Data/Buffs, quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_IsolationTherapy` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_LateNightWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_LonelyRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_LoveConfession` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_MaintenanceCheckup` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_MayorConcernLewis` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_MedicalRecognition` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_MedicalToolsClint` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_MedicinesPierre` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_MemoryItem` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_MentalFatigueRecovery` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_MentalFatigueRecoveryComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_MentalFatigueRecoveryProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_MentalFatigueRecoveryStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_MentalFatigueRecovery_Light` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_MentalFatigueRecovery_Moderate` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_MineForbidden` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_MineWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_ModerateCare` | buff | — | CP: Data/Buffs, quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_MonthlyCheckup` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_MotherlyCareJodi` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_MovingInNotice` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_MuscleAtrophy` | buff | — | CP: Data/Buffs, buffsInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_MuscleTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_Neglect` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_NeglectWarning` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_NightCrisis` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_NightCrisis_Dating` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_NightCrisis_PreDating` | buff | — | CP: events.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyMod_NoSleepRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_NumbnessRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_NutritionBoost` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_NutritionGus` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_NutritionPlan` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_OfficialSupportLewis` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_OvercareMaru` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_OverworkRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_PainFlare` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_PanicAttackComplete` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_PanicAttackTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_PanicAttackTreatmentComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_PanicAttackTreatmentProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_PanicAttackTreatmentShop` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_PanicAttackTreatmentStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_PanicRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_PerfectPatientAward` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_PhysicalRehabilitation` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_PreventiveCare` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_ProfessionalSuccess` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_ProgressCelebration` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_ProtectionOffer` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_PsychForm` | buff | — | CP: Data/Buffs, items.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_PsychologicalEvaluation` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_PsychologicalRecovery` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_PsychologicalSupport` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_PsychoticBreak` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_PsychoticEpisode` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_RecoveryDrink` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_RecoveryEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_RecoveryMeds` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_RecoveryMonitoring` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_RecoveryReliefLetter` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_RecoveryRobin` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_RelapseNotice` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_RelapseWarning` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_ResearchDemetrius` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_RibInjuryAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_RibsTreatment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_ScientificHelpDemetrius` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_SelfCareJournal` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_Sepsis` | buff | — | CP: Data/Buffs, buffsInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_SevereDissociation` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_ShadowParanoiaRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_ShrapnelAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_ShrapnelSurgery` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsCure.json (ref:buff) | да | нет | OK |
| `HarveyMod_Shrapnel_Healing` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_Shrapnel_Recovery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_Shrapnel_Surgery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_SleepDeprivationRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_SleepTherapy` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_SleepTherapyComplete` | buff | — | CP: Data/Buffs, Data/Mail, mail.json (ref:buff), quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_SleepTherapyProgress` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_SleepTherapyStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_SleepTracker` | buff | — | CP: Data/Buffs, items.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_SocialRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_SocialSupport` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_SpecialOrderPierre` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_SprainedAnkle_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_SprainedAnkle_Recovery` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_Stabilizer` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_StabilizerEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_StormProtection` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_StormProtectionComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_StormProtectionStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressDiagnosisBasic` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff), quests.json (ref:buff), questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressDiagnosisComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressDiagnosisStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressReliefAmpoule` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressReliefEffect` | buff | — | CP: items.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressReliefPill` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_StressReliefSalve` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_StrictSupervisionQuest` | buff | — | CP: Data/Buffs, quests.json (ref:buff) | да | нет | OK |
| `HarveyMod_SuicidalIdeation` | buff | — | CP: Data/Buffs, quests.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_SurgeryPrideMaru` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_TherapyJournal` | buff | — | CP: Data/Buffs, items.json (ref:buff) | да | нет | OK |
| `HarveyMod_ThunderRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_TiredRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_TooColdRecovery` | buff | — | CP: Data/Buffs, questsStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_TornMusclesAlert` | buff | — | CP: Data/Mail, mailInjury.json (ref:buff) | да | нет | OK |
| `HarveyMod_TornMuscles_Acute` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_TornMuscles_Healing` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_TornMuscles_Rehab` | HarveyMod/phase_buff | InjuryManager.GetPhaseBuffId | C#: Managers/InjuryManager.cs, Managers/TreatmentManager.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `HarveyMod_TraumaAnxietyNotice` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_TraumaAssessment` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_TraumaAssessmentComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_TraumaAssessmentStart` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_TraumaWork` | buff | — | CP: Data/Buffs, quests.json (ref:buff), questsStress.json (ref:buff), quests_balanced.json (ref:buff) | да | нет | OK |
| `HarveyMod_TreatmentComplete` | buff | — | CP: Data/Buffs, quest_dialogues.json (ref:buff) | да | нет | OK |
| `HarveyMod_TreatmentFinalWarning` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_TreatmentPlanMeeting` | HarveyMod | — | C#: Core\Constants.cs, Managers\DialogueManager.cs; CP: events.json (ref:buff) | да | да | OK |
| `HarveyMod_TreatmentPlanReady` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_TreatmentSeriesComplete` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_TreatmentUrgentReminder` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_Trigger_RestCheck` | buff | — | CP: buffsCureStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_Trigger_ThunderCalmingCheck` | buff | — | CP: buffsCureStress.json (ref:buff) | да | нет | OK |
| `HarveyMod_ViolationWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_WeatherWarning` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyMod_WeddingWishEvelyn` | buff | — | CP: Data/Buffs, dialoguesNpc.json (ref:buff) | да | нет | OK |
| `HarveyMod_WetBandage` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_WetBandageInfection` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_WetCare` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailCure.json (ref:buff) | да | да | OK |
| `HarveyMod_WetStitches` | HarveyMod/HarveyMod/buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `HarveyMod_WetStitchesCare` | HarveyMod/mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailCure.json (ref:buff) | да | да | OK |
| `HarveyMod_WinterHealthTips` | buff | — | CP: Data/Mail, mail.json (ref:buff) | да | нет | OK |
| `HarveyOverhaulStory.E1_SlipperyPath` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E2B_QuietAgreement` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E2_InsistentExam` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E3B_WingPatient` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E3_ForestApothecary` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E4B_TooQuiet` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E4_PierBreath` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E5_StormBeside` | event | — | C#: Core\Constants.cs; CP: events.json (ref:event) | да | да | OK |
| `HarveyOverhaulStory.E6_SayItOutLoud` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E8_QuietShelf` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `HarveyOverhaulStory.E9_LightInWindow` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `PhaseTransition_BackStrain_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_BruisedRibs_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_BurnWounds_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_Cold_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_Concussion_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_Concussion_3` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_DeepCuts_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_DeepCuts_3` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_FracturedBone_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_FracturedBone_3` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_InfectedWound_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_ShrapnelWounds_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_ShrapnelWounds_3` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_SprainedAnkle_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_TornMuscles_2` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `PhaseTransition_TornMuscles_3` | phase_transition (CP-only key) | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | нет | OK |
| `buffAlcoholPoisoning` | buff | EventHandlers\PlayerEventHandler.cs | C#: EventHandlers\PlayerEventHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffAntibioticsTreatment` | buff | — | C#: Core/Constants.cs, Core\Constants.cs, EventHandlers\PlayerEventHandler.cs; CP: Data/Buffs, buffsCure.json (ref:buff), dialoguesHarveyCure.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffBackStrain` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffBadlyHurt` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\GameEventHandler.cs, EventHandlers\InteractionHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffBruisedRibs` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffBurnWounds` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, EventHandlers\TimeEventHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffCalmTeaEffect` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff) | да | нет | OK |
| `buffCalmingAtHospitalWithHarvey` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffCold` | HarveyMod/buff/buff | — | C#: Core/Constants.cs, Core\Constants.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff) | да | да | OK |
| `buffConcussion` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, EventHandlers\TimeEventHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffConstantSupervision` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff) | да | нет | OK |
| `buffDarknessLevel1` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffDarknessLevel2` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffDarknessLevel3` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffDarknessOvercome` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffDeepCuts` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, Core\Models\DebuffState.cs, EventHandlers\InteractionHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffDimLight` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffEmergencySupervision` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffFarmerExhausted` | buff | EventHandlers\PassOutHandler.cs | C#: EventHandlers\PassOutHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffForcedSedation` | buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `buffFracturedBone` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, EventHandlers\TimeEventHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffHarveyCare` | buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `buffHarveyDropper` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff), eventsCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffHarveyHealing` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffHarveyIntensiveCare` | buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsCure.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffHarveyLantern` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffHarveyProtection` | buff | — | C#: Core/Constants.cs, Core\Constants.cs, EventHandlers\PlayerEventHandler.cs; CP: Data/Buffs, buffsCure.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffHarveyRecovery` | buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsCure.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffHarveyTreatment` | buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsCure.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffHarveyTreatmentAnxietyWave` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentBadDream` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentBreakdown` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentCollapse` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentCritical` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentCriticism` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentDarkness` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentDespair` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentFreezeResponse` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentHunger` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentIsolation` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentLonely` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentMentalFatigue` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentNoSleep` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentNumbness` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentOverwork` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentPanic` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentShadowParanoia` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentSocial` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentThunder` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentTired` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHarveyTreatmentTooCold` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffHurt` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\GameEventHandler.cs, EventHandlers\InteractionHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffInfectedWound` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, EventHandlers\TimeEventHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffLightAndSafe` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffManager` | buff | EventHandlers\StormComfortLauncher.cs | C#: EventHandlers\GameEventHandler.cs, EventHandlers\InteractionHandler.cs, EventHandlers\PassOutHandler.cs | нет | да | OK |
| `buffOverworkBreak` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffPainFlare` | buff | — | CP: Data/Buffs, buffsInjury.json (ref:buff) | да | нет | OK |
| `buffPostSurgicalCare` | buff | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `buffRestingAtHome` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffShrapnelWounds` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, EventHandlers\TimeEventHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffSleepDeprivation` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffSleepy` | buff | EventHandlers\PassOutHandler.cs | C#: EventHandlers\PassOutHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff) | да | да | OK |
| `buffSprainedAnkle` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffStressAnxietyWave` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressBadDream` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressBreakdown` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressBurnout` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressCollapse` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressCritical` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressCriticalExhaustion` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressCriticism` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressDarkness` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressDespair` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressDespairCollapse` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressExhaustion` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressFreezeResponse` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressHunger` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressImmunity` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff), events.json (ref:buff), events_for_mode_new_formatted.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffStressIsolation` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressLonely` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressMentalBreakdown` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressMentalFatigue` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressNightTerror` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressNoSleep` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffStressNumbness` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressOverwork` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressPanic` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressPanicBreakdown` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressPanicCollapse` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressParanoidCollapse` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressPhysicalDepletion` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressRecovery` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffStressRecoveryAdvanced` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffStressRecoveryGlow` | buff | — | CP: Data/Buffs, buffsCureStress.json (ref:buff) | да | нет | OK |
| `buffStressShadowParanoia` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressSocial` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff) | да | нет | OK |
| `buffStressSocialAnxiety` | buff | — | CP: Data/Buffs, persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStressThunder` | buff | EventHandlers\StormComfortLauncher.cs | C#: Core\Constants.cs, EventHandlers\StormComfortLauncher.cs; CP: Data/Buffs, buffsStress.json (ref:buff), events.json (ref:buff), events_for_mode_new_formatted.json (ref:buff) | да | да | OK |
| `buffStressTired` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `buffStressTooCold` | buff | — | CP: Data/Buffs, buffsStress.json (ref:buff), persistentBuffs.json (ref:buff) | да | нет | OK |
| `buffStrictSupervision` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff), dialoguesHarveyCare.json (ref:buff), dialoguesHarveyPregnant.json (ref:buff) | да | нет | OK |
| `buffSurgicalWound` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\GameEventHandler.cs, EventHandlers\InteractionHandler.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffTeracitin` | buff | — | C#: Core/Constants.cs, Core\Constants.cs, EventHandlers\PlayerEventHandler.cs; CP: Data/Buffs, buffsCure.json (ref:buff) | да | да | OK |
| `buffTooCold` | buff | EventHandlers\PlayerEventHandler.cs | C#: EventHandlers\PlayerEventHandler.cs | нет | да | OK |
| `buffTornMuscles` | buff | Managers\InjuryManager.cs | C#: Core\Constants.cs, EventHandlers\InteractionHandler.cs, Managers\InjuryManager.cs; CP: Data/Buffs, buffsInjury.json (ref:buff), persistentBuffs.json (ref:buff), triggersCare.json (ref:buff) | да | да | OK |
| `buffTraumaHealing` | buff | — | CP: Data/Buffs, buffsCure.json (ref:buff) | да | нет | OK |
| `buffWarmth` | buff | — | C#: EventHandlers\PlayerEventHandler.cs | нет | да | OK |
| `buffsCure` | buff | — | C#: Managers\BuffManager.cs | нет | да | OK |
| `buffsInjury` | buff | — | C#: Managers\BuffManager.cs | нет | да | OK |
| `buffsToRemove` | buff | — | C#: Managers\InjuryManager.cs | нет | да | OK |
| `eventHarveyCheckFarmerOutsideAfter22` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyCheckHealthFarmer` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyCheckup` | event | — | CP: eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyEmergencyCare` | event | — | C#: Core\Constants.cs, EventHandlers\PassOutHandler.cs; CP: eventsCare.json (ref:event) | да | да | OK |
| `eventHarveyExhaustion` | event | — | C#: Core\Constants.cs, EventHandlers\PassOutHandler.cs; CP: eventsCare.json (ref:event) | да | да | OK |
| `eventHarveyFirstDate` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstMeeting` | event | — | CP: events.json (ref:event), eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstVisit` | event | — | CP: eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyFirstWalk` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyLateNightCollapse` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMedicalCheck` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMedicalCheck_Dating` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMedicalCheck_memory_oneday` | event | — | CP: Data/Events, dialoguesHarvey.json (ref:event) | да | нет | OK |
| `eventHarveyMineInterception` | event | — | CP: eventsCare.json (ref:event), triggersCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMineRescue` | event | — | C#: Core\Constants.cs, EventHandlers\PassOutHandler.cs; CP: eventsMineRescue.json (ref:event) | да | да | OK |
| `eventHarveyMineRescueDating` | event | — | C#: Core\Constants.cs, EventHandlers\PassOutHandler.cs; CP: eventsMineRescue.json (ref:event) | да | да | OK |
| `eventHarveyMinorMineRescue` | event | — | C#: Core\Constants.cs, Core\Models\InjuryState.cs, EventHandlers\PassOutHandler.cs; CP: eventsMineRescue.json (ref:event) | да | да | OK |
| `eventHarveyMorningCheckup` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyMountainDate` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyPropose` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyRoomCheckup` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyRoomCheckup2` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveySecondVisit` | event | — | CP: events.json (ref:event), eventsCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveySkullCavePrevention` | event | — | CP: eventsCare.json (ref:event), triggersCare.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfort` | event | — | C#: Core\Constants.cs | нет | да | OK |
| `eventHarveyStormComfortDesert` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortFarm` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortForest` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortMine` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortMountain` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyStormComfortTown` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyTraumaExam` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `eventHarveyTreatmentCollapse` | event | — | CP: events.json (ref:event) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyAfterMineRescue` | mail | — | CP: eventsMineRescue.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyCaveWarning` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyIntensiveCare` | mail | — | CP: Data/Mail, mail.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyMedicalCheckReminder` | mail | — | CP: Data/Mail, events.json (ref:mail), mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyMineForbidden` | mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:mail) | да | да | OK |
| `mailHarveyMineWarning` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyModerateCare` | mail | — | CP: Data/Mail, mail.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote1` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote2` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote3` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNote4` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyNoteGirlfriend` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyNoteWife` | mail | — | CP: Data/Mail, mailCare.json (ref:mail), triggersCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyPostTrauma` | mail | — | CP: Data/Mail, eventsCare.json (ref:mail), mailCare.json (ref:mail) | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `mailHarveyRecovery1` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyRecovery2` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyRecovery3` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyRecoveryFinal` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyRecoveryFinalDating` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyRecoveryFinal_Friendship` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyRestRequired` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveySleepControl` | mail | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Data/Mail, mailInjury.json (ref:mail) | да | да | OK |
| `mailHarveyStep1` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyStep1Dating` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyStep2` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyStep2Dating` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyStep3` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyStep3Dating` | mail | — | CP: Data/Mail, mailCare.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentAnxietyWave` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentBadDream` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentBreakdown` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentCollapse` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentCritical` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentCriticism` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentDarkness` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentDespair` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentFreezeResponse` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentHunger` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentIsolation` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentLonely` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentMentalFatigue` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentNoSleep` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentNumbness` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentOverwork` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentPanic` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentShadowParanoia` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentSocial` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentThunder` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentTired` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarveyStressTreatmentTooCold` | mail | — | CP: Data/Mail, mailStress.json (ref:mail) | да | нет | OK |
| `mailHarvey_Neglect` | mail | — | CP: Data/Mail, mailInjury.json (ref:mail) | да | нет | OK |
| `mailInjury` | mail | — | C#: Core\Constants.cs | нет | да | OK |
| `mailKeys` | mail | — | C#: Helpers\ContentAuditRunner.cs | нет | да | OK |
| `topicAcceptHospital` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicAfterCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicAgreedCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicAlcoholPoisoningPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicAlcoholPoisoningPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicAlcoholPoisoningPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBackStrain` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicBackStrainCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBackStrainPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBackStrainPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBackStrainPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBadlyHurt` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicBadlyHurtCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBadlyHurtPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBirthdayHospitalComplete` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicBoyfriendWorries` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBruisedRibs` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicBruisedRibsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBruisedRibsPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBruisedRibsPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBruisedRibsPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBurnWounds` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicBurnWoundsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicBurnWoundsPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBurnWoundsPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicBurnWoundsPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicCold` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicColdCured` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicColdPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicColdPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicColdPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicConcernForHealth` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicConcussion` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicConcussionCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicConcussionPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicConcussionPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicConcussionPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicDarknessFullyCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessLanternReceived` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessStep1Complete` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessStep2Complete` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDarknessTherapyStart` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDeepCuts` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicDeepCutsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicDeepCutsPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicDeepCutsPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicDeepCutsPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicDiagnosisComplete` | topic | Managers\DialogueManager.cs | C#: Core/Constants.cs, Core\Constants.cs, Managers\DialogueManager.cs; CP: events.json | да | да | OK |
| `topicEmotionalSupport` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicFarmerExhausted` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicFatigue` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicFirstMeeting` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, dialoguesHarveyCare.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicFirstTreatmentComplete` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, Managers\DialogueManager.cs; CP: events.json | да | да | OK |
| `topicFracturedBone` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicFracturedBoneCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicFracturedBonePhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicFracturedBonePhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicFracturedBonePhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicHarveyAcceptFirstWalk` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyCareAgreement` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyDeclineFirstWalk` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyExhaustion` | topic | — | CP: dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisit` | topic | — | CP: eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisitAgree` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisitNeutral` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyFirstVisitRefused` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyGentleCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyHelp_Asks` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyHelp_Independent` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyHelp_Spotter` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyIntensiveCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyLove` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarveyMandatoryCheckup` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyMinorMineRescue` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: eventsMineRescue.json | да | да | OK |
| `topicHarveyModerateCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyNeedsFirstTreatment` | topic | Managers\DialogueManager.cs | C#: Core/Constants.cs, Core\Constants.cs, Managers\DialogueManager.cs; CP: events.json | да | да | OK |
| `topicHarveyPierBreath` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisit` | topic | — | CP: eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisitAgree` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisitNeutral` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySecondVisitRefused` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json, eventsCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStormStress` | topic | EventHandlers\StormComfortLauncher.cs | C#: Core\Constants.cs, EventHandlers\StormComfortLauncher.cs; CP: events.json | да | да | OK |
| `topicHarveyStorm_Clinic` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Escort` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Home` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyStorm_Note` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveySupport` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarveyTooQuiet` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTraumaReveal` | topic | — | CP: dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrustFinal` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_Breakfast` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_BreathHard` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_DoctorDecides` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_NeedsSpace` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_PublicCare` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_Rest` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_TouchOk` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyTrust_Water` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWalkBad` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWalkGood` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, dialoguesNpc.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWalkNeutral` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarveyWingPatient` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicHarvey_` | topic | — | C#: Core\Constants.cs | нет | да | OK |
| `topicHarvey_AllergicRash` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_AllergicRash_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_AllergicRash_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_DirtyWound` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_DirtyWound_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_DirtyWound_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_EscalatedCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_EscalatedCare_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_EscalatedCare_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_ForcedHospitalization` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_ForcedHospitalization_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_ForcedHospitalization_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_Neglect` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_Neglect_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_Neglect_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_NightRound` | topic | EventHandlers\TimeEventHandler.cs | C#: EventHandlers\TimeEventHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicHarvey_NightRound_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_NightRound_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_PainFlare` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_WetBandage` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_WetBandage_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetBandage_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetStitches` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | да | OK |
| `topicHarvey_WetStitches_memory_oneday` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHarvey_WetStitches_memory_oneweek` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarvey.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHealthCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHealthDamageCritical` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicHealthDamageSevere` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicHope` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurt` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicHurtCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHurtPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicHusbandlyProtection` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicInfectedWound` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicInfectedWoundCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicInfectedWoundPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicInfectedWoundPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicInfectedWoundPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicIntensiveTreatment` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicMedicalCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicMentalExhaustion` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicMentalHealth` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicMineInjuryRescue` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, EventHandlers\PassOutHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json, eventsMineRescue.json | да | да | OK |
| `topicMineRescuePending` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: triggersCare.json | да | да | OK |
| `topicNightCrisisComplete` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicOverprotectiveMode` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json, triggersCare.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicPanicAttacks` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicPassedOutInTown` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json, events.json | да | да | OK |
| `topicPersonalStruggle` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicPostOperativeCare` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicPreventiveCare` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicProtectiveBoyfriend` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicRefusedCheckup` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCare.json, dialoguesHarveyPregnant.json, events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicRescueComplete` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicRescueOperation` | topic | EventHandlers\RescueOperationLauncher.cs | C#: Core\Constants.cs, EventHandlers\RescueOperationLauncher.cs; CP: events.json | да | да | OK |
| `topicShrapnelWounds` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicShrapnelWoundsCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicShrapnelWoundsPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicShrapnelWoundsPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicShrapnelWoundsPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicSleepIssues` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSprainedAnkle` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicSprainedAnkleCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSprainedAnklePhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicSprainedAnklePhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicSprainedAnklePhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicStartTreatment` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressAnxietyWave` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressBadDream` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressBreakdown` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, events_for_mode_new_formatted.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicStressCollapse` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, events_for_mode_new_formatted.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicStressCritical` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressCriticism` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDarkness` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDarknessLevel2` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDarknessLevel3` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressDespair` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressFreezeResponse` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressHunger` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressIsolation` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressLonely` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressMentalFatigue` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressNoSleep` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressNumbness` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressOverwork` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressPanic` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressShadowParanoia` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressSleepDeprivation` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressSocial` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressThunder` | topic | — | C#: Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json, events.json | да | да | OK |
| `topicStressTired` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json, events_for_mode_new_formatted.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicStressTooCold` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyStress.json, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentAnxietyWaveCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentAnxietyWaveStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBadDreamCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBadDreamStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBreakdownCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentBreakdownStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCollapseCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCollapseStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCriticismCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentCriticismStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDarknessCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDarknessStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDespairCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentDespairStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentFreezeResponseCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentFreezeResponseStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentHungerCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentHungerStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentIsolationCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentIsolationStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentLonelyCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentLonelyStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentMentalFatigueCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentMentalFatigueStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNoSleepCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNoSleepStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNumbnessCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentNumbnessStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentOverworkCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentOverworkStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentPanicCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentPanicStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentShadowParanoiaCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentShadowParanoiaStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSleepDeprivationCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSleepDeprivationStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSocialCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentSocialStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentThunderCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentThunderStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTiredCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTiredStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTooColdCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicStressTreatmentTooColdStarted` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCureStress.json, dialoguesHarveyStress.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSurgicalWound` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicSurgicalWoundCured` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json, dialoguesHarveyInjury.json | да | да | OK |
| `topicSurgicalWoundPhaseAcute` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSurgicalWoundPhaseHealing` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicSurgicalWoundPhaseRecovery` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTooCold` | topic | — | C#: Core/Constants.cs, Core\Constants.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicTornMuscles` | topic | — | C#: Core/Constants.cs, Core\Constants.cs, ModEntry.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyInjury.json | да | да | OK |
| `topicTornMusclesCured` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTornMusclesPhaseAcute` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicTornMusclesPhaseHealing` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicTornMusclesPhaseRecovery` | phase_topic | InjuryManager.GetPhaseTopicId → InteractionHandler | C#: EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicTrauma` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatment` | topic | — | C#: Core\Constants.cs | нет | да | OK |
| `topicTreatmentAgreement` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicTreatmentBackStrain` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentBruisedRibs` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentBurnWounds` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentCold` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentCompleted` | completion_topic/topic | TreatmentManager.AddTopic | C#: Core/Constants.cs, Core\Constants.cs, EventHandlers/InteractionHandler.cs; CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | да | OK |
| `topicTreatmentConcussion` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentDeepCuts` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentFracturedBone` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentInfectedWound` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentProgress` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesNpc.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentRefusal` | topic | — | CP: events.json | да | нет | ⚠️ CP preconditions — C# не создаёт |
| `topicTreatmentShrapnelWounds` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentSprainedAnkle` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicTreatmentTornMuscles` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `topicWifelyWorries` | topic | — | CP: Characters/Dialogue/Harvey, dialoguesHarveyCure.json | да | нет | ⚠️ CP dialogue key — C# не создаёт topic |
| `triggerBackStrain` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerBadlyHurt` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerBruisedRibs` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerBurnWounds` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerCold` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerConcussion` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerDeepCutsCombat` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerDeepCutsFarming` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerEmergencySupervision` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerExplosionInjury` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerFracturedBone` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerHarveyGentleCare` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyIntensiveCare` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyMedicalCheckReminder` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyMineWarning` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyModerateCare` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyNoteNote1` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyNoteNote2` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyNoteNote3` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyNoteNote4` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveyNoteWife` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHarveySkullCaveWarning` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerHurt` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerInfectedWound` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerLocationReactionMine` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerLocationReactionMineExit` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerLocationReactionSkullCaveExit` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerShrapnelWounds` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerSituationReactionDrunk` | trigger | EventHandlers\PlayerEventHandler.cs | C#: EventHandlers\PlayerEventHandler.cs | нет | да | OK |
| `triggerSprainedAnkle` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerSurgicalWound` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |
| `triggerTimeReactionEarly` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerTimeReactionLate` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerTimeReactionVeryLate` | trigger | — | CP: Data/TriggerActions | да | нет | OK |
| `triggerTornMuscles` | trigger | Constants.Triggers → StateManager.AppliedTriggers | C#: Core/Constants.cs, Core\Constants.cs, Managers/InjuryManager.cs | нет | да | OK |

**Статус:** автоген 2026-05-24.
