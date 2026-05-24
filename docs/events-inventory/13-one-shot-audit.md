# Аудит одноразовости: triggers, events, topics

Разделение механик HarveyOverhaul InjuryCare + CP по категориям повторяемости.

## Категории

| # | Категория | Ожидание |
|---|---|---|
| 1 | **One-shot story event** | Один раз за сейв (`eventsSeen`, mail received) |
| 2 | **Repeatable injury trigger** | Повтор после выздоровления / cooldown |
| 3 | **Repeatable care scene** | Может повторяться, но не чаще N дней |
| 4 | **Daily/temporary reaction** | Раз в день / ночь / короткий topic |

## Сводка

- Строк в таблице: **94**
- CRITICAL: **0** | HIGH: **9**

### Механизмы контроля одноразовости

| Механизм | Где | Назначение |
|---|---|---|
| `Game1.player.eventsSeen` | CP preconditions + C# mine rescue | Story / cutscene one-shot |
| `InjuryState.AppliedTriggers` | C# InjuryManager (story only: SurgicalWound, ExplosionInjury) | Story one-shot gate |
| `InjuryCooldownUntilDay` | C# StateManager | Repeatable injury cooldown per buff |
| `LastNightRoundRollDay` / `LastNightRoundDay` | TimeEventHandler | 1 roll / 1 visit per night |
| `activeDialogueEvents` / AddTopic(days) | C# + CP | Temporary topics |
| `!PLAYER_HAS_MAIL Received` | triggersCare | One-shot mail chains |
| `HarveyMod_CD_*` topics | CP events E1–E8 | Cooldown between story beats |

## Критичные разрывы (CRITICAL)

- *(нет)*

## HIGH риски (выборка)

- **event:eventHarveyCheckFarmerOutsideAfter22** — C# bridge topics или снять seen/topic gate
- **event:eventHarveyCheckup** — C# bridge topics или снять seen/topic gate
- **event:eventHarveyMineInterception** — Cooldown mail или !PLAYER_HAS_MAIL today
- **event:eventHarveyMorningCheckup** — C# bridge topics или снять seen/topic gate
- **event:eventHarveySecondVisit** — C# bridge topics или снять seen/topic gate
- **event:eventHarveySkullCavePrevention** — Cooldown mail или !PLAYER_HAS_MAIL today
- **event:eventHarveyTreatmentCollapse** — switchEvent или удалить orphan
- **trigger:triggerHarveyMineWarning** — Fix buff check; mail cooldown
- **trigger:triggerHarveySkullCaveWarning** — Fix buff check; mail cooldown

## Политика повторяемости травм (2026-05-24)

`AppliedTriggers` — только **story one-shot** (`SurgicalWound`, `ExplosionInjury`).
Остальные травмы — **cooldown** через `InjuryCooldownUntilDay` / `RepeatableInjuryCooldownDays`.

Оставшийся риск: drunk `situationReaction` всё ещё использует `AppliedTriggers` one-shot per save.

## Таблица

| Trigger/Event | Сейчас | Должно быть | Риск | Рекомендация |
|---|---|---|---|---|
| `trigger:triggerHarveyMineWarning` | PlayEvent + mail on LocationChanged; base buff IDs | 4. Daily/temporary | HIGH — buffDeepCuts vs phase buff mismatch | Fix buff check; mail cooldown |
| `trigger:triggerHarveySkullCaveWarning` | PlayEvent + mail on LocationChanged; base buff IDs | 4. Daily/temporary | HIGH — buffDeepCuts vs phase buff mismatch | Fix buff check; mail cooldown |
| `event:eventHarveyMineInterception` | CP Trigger LocationChanged + mail; нет seen | 4. Daily/temporary | HIGH — каждый вход в Mine/SkullCave с injury buff | Cooldown mail или !PLAYER_HAS_MAIL today |
| `event:eventHarveySkullCavePrevention` | CP Trigger LocationChanged + mail; нет seen | 4. Daily/temporary | HIGH — каждый вход в Mine/SkullCave с injury buff | Cooldown mail или !PLAYER_HAS_MAIL today |
| `event:eventHarveyTreatmentCollapse` | Script-only — нет trigger | 3. Repeatable care | HIGH — недостижимо / dead | switchEvent или удалить orphan |
| `event:eventHarveyCheckFarmerOutsideAfter22` | Topic chain (CP); orphan topics без C# | 3. Repeatable care | HIGH — цепочка не стартует / one-shot topic | C# bridge topics или снять seen/topic gate |
| `event:eventHarveyCheckup` | Topic chain (CP); orphan topics без C# | 3. Repeatable care | HIGH — цепочка не стартует / one-shot topic | C# bridge topics или снять seen/topic gate |
| `event:eventHarveyMorningCheckup` | Topic chain (CP); orphan topics без C# | 3. Repeatable care | HIGH — цепочка не стартует / one-shot topic | C# bridge topics или снять seen/topic gate |
| `event:eventHarveySecondVisit` | Topic chain (CP); orphan topics без C# | 3. Repeatable care | HIGH — цепочка не стартует / one-shot topic | C# bridge topics или снять seen/topic gate |
| `C# HarveyMod_MineForbidden buff` | MineForbiddenAppliedDay + duration days | 4. Daily/temporary | LOW | OK |
| `C# Mine entry warning HUD` | _lastMineWarningDay 1×/day | 4. Daily/temporary | LOW | OK |
| `C# Neglect strikes` | NeglectStrikes counter; topicNeglect 7d | 3. Repeatable care | LOW | OK |
| `C# Night visit (TimeEventHandler)` | LastNightRoundRollDay + LastNightRoundDay; 35% roll/night | 4. Daily/temporary | LOW | OK |
| `C# buffFarmerExhausted / buffSleepy` | Gate: !hasBuff && !topic | 4. Daily/temporary | LOW | OK after topic expires |
| `C# topic*Cured / topicTreatmentCompleted` | 7d; auto GameEventHandler + InteractionHandler | 3. Repeatable care | LOW | OK — completion dialogue one per topic instance |
| `C# topic*Phase* (InjuryManager)` | 7d per phase; removed on phase advance | 2. Repeatable injury | LOW | OK |
| `C# topicHarvey_NightRound` | 2d via activeDialogueEvents; LastNightRoundRollDay 1×/night | 4. Daily/temporary | LOW | OK |
| `C# triggerBackStrain → buffBackStrain` | LastInjuryAppliedDayByTrigger cooldown (RepeatableInjuryCooldownDays) | 2. Repeatable injury | LOW | OK — cooldown config |
| `C# triggerBadlyHurt → buffBadlyHurt` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerBruisedRibs → buffBruisedRibs` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerBurnWounds → buffBurnWounds` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerCold → buffCold` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerConcussion → buffConcussion` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerDeepCutsCombat → buffDeepCuts` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerDeepCutsFarming → buffDeepCuts` | LastInjuryAppliedDayByTrigger cooldown (RepeatableInjuryCooldownDays) | 2. Repeatable injury | LOW | OK — cooldown config |
| `C# triggerExplosionInjury → buffShrapnelWounds?` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerFracturedBone → buffFracturedBone` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerHurt → buffHurt` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerInfectedWound → buffInfectedWound` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerSprainedAnkle → buffSprainedAnkle` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | LOW | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerTornMuscles → buffTornMuscles` | LastInjuryAppliedDayByTrigger cooldown (RepeatableInjuryCooldownDays) | 2. Repeatable injury | LOW | OK — cooldown config |
| `CP HarveyMod_CD_* topics` | 2–7d cooldown between story events E1–E8 | 1. One-shot story | LOW | OK — soft gate between story beats |
| `event:HarveyMod_FirstTreatment` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyMod_TreatmentPlanMeeting` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E1_SlipperyPath` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E2B_QuietAgreement` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E2_InsistentExam` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E3B_WingPatient` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E3_ForestApothecary` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E4B_TooQuiet` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E4_PierBreath` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E5_StormBeside` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E6_SayItOutLoud` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E7_TownSip_Sunny` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E8_QuietShelf` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:HarveyOverhaulStory.E9_LightInWindow` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:eventHarveyFirstMeeting` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:eventHarveyFirstVisit` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:eventHarveyFirstWalk` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `event:eventRescueOperation` | eventsSeen (CP preconditions) | 1. One-shot story | LOW | OK |
| `trigger:triggerEmergencySupervision` | Topic 7d после комбинации reactions | 3. Repeatable care | LOW | OK |
| `trigger:triggerHarveyNoteNote1` | !PLAYER_HAS_MAIL Received — one-shot mail chain | 1. One-shot story | LOW | OK для note chain |
| `trigger:triggerHarveyNoteNote2` | !PLAYER_HAS_MAIL Received — one-shot mail chain | 1. One-shot story | LOW | OK для note chain |
| `trigger:triggerHarveyNoteNote3` | !PLAYER_HAS_MAIL Received — one-shot mail chain | 1. One-shot story | LOW | OK для note chain |
| `trigger:triggerHarveyNoteNote4` | !PLAYER_HAS_MAIL Received — one-shot mail chain | 1. One-shot story | LOW | OK для note chain |
| `trigger:triggerHarveyNoteWife` | !PLAYER_HAS_MAIL Received — one-shot mail chain | 1. One-shot story | LOW | OK для note chain |
| `C# eventsSeen: mine rescue (3 events)` | onEventFinished → eventsSeen; IsMineRescueEventAlreadySeen → topic only | 1. One-shot story | LOW (после fix) | OK |
| `C# Complication topics (Wet/Dirty/Neglect)` | 4–7d topics; buff until treated | 2. Repeatable injury | LOW — repeatable via re-injury | OK |
| `trigger:triggerHarveyGentleCare` | Mail/topic chain + RANDOM; topic 7d | 3. Repeatable care | LOW — sequential one-shot mail | OK |
| `trigger:triggerHarveyIntensiveCare` | Mail/topic chain + RANDOM; topic 7d | 3. Repeatable care | LOW — sequential one-shot mail | OK |
| `trigger:triggerHarveyModerateCare` | Mail/topic chain + RANDOM; topic 7d | 3. Repeatable care | LOW — sequential one-shot mail | OK |
| `CP buffStressThunder + storm comfort` | StormComfortLauncher daily roll → buff or topicHarveyStormStress | 4. Daily/temporary | LOW — wired 2026-05-24 | OK |
| `event:eventHarveyEmergencyCare` | C# QueueHospitalEvent → warp Hospital → startEvent; eventsSeen one-shot | 3. Repeatable care | LOW — wired 2026-05-24 | OK |
| `event:eventHarveyExhaustion` | C# QueueHospitalEvent → warp Hospital → startEvent; eventsSeen one-shot | 3. Repeatable care | LOW — wired 2026-05-24 | OK |
| `C# topicFarmerExhausted / topicPassedOutInTown` | 3d / 2d; gate hasBuff + HasConversationTopic | 4. Daily/temporary | LOW — повтор после expiry | OK |
| `event:HarveyMod_BirthdayHospital_Dating` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:HarveyMod_BirthdayHospital_Friend` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:HarveyMod_NightCrisis_Dating` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:HarveyMod_NightCrisis_PreDating` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:eventHarveyCheckHealthFarmer` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:eventHarveyLateNightCollapse` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:eventHarveyMedicalCheck` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:eventHarveyMedicalCheck_Dating` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:eventHarveyRoomCheckup` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `event:eventHarveyRoomCheckup2` | eventsSeen если vanilla trigger | 1. One-shot story | MED | Проверить preconditions |
| `trigger:triggerHarveyMedicalCheckReminder` | !PLAYER_HAS_MAIL Received — one-shot mail chain | 3. Repeatable care | MED | OK для note chain |
| `trigger:triggerLocationReactionMine` | Topic 1–2 дня; Trigger=LocationChanged; нет seen | 4. Daily/temporary | MED — LocationChanged может часто | Раз в день gate (topic или Last*Day в C#) |
| `trigger:triggerLocationReactionMineExit` | Topic 1–2 дня; Trigger=LocationChanged; нет seen | 4. Daily/temporary | MED — LocationChanged может часто | Раз в день gate (topic или Last*Day в C#) |
| `trigger:triggerLocationReactionSkullCaveExit` | Topic 1–2 дня; Trigger=LocationChanged; нет seen | 4. Daily/temporary | MED — LocationChanged может часто | Раз в день gate (topic или Last*Day в C#) |
| `trigger:triggerTimeReactionEarly` | Topic 1–2 дня; Trigger=LocationChanged; нет seen | 4. Daily/temporary | MED — LocationChanged может часто | Раз в день gate (topic или Last*Day в C#) |
| `trigger:triggerTimeReactionLate` | Topic 1–2 дня; Trigger=LocationChanged; нет seen | 4. Daily/temporary | MED — LocationChanged может часто | Раз в день gate (topic или Last*Day в C#) |
| `trigger:triggerTimeReactionVeryLate` | Topic 1–2 дня; Trigger=LocationChanged; нет seen | 4. Daily/temporary | MED — LocationChanged может часто | Раз в день gate (topic или Last*Day в C#) |
| `C# situationReaction_Drunk + AppliedTriggers` | AppliedTriggers one-shot per save | 4. Daily/temporary | MED — drunk reaction once ever | Repeatable: clear trigger on topic expiry |
| `C# triggerShrapnelWounds → buffShrapnelWounds` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | MED — story one-shot | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# triggerSurgicalWound → buffSurgicalWound` | InjuryCooldownUntilDay per buff (AppliedTriggers only for story: SurgicalWound, ExplosionInjury) | 2. Repeatable injury | MED — story one-shot | OK — repeatable via cooldown (2026-05-24 policy) |
| `C# topicMineInjuryRescue` | Temporary 2d; removed on hospital warp | 3. Repeatable care | MED — topic снимается до клика по Харви | Не RemoveTopic до interaction |
| `event:eventHarveyMineRescue` | C# eventsSeen on finish + topic fallback; repeat → topic only | 1. One-shot story | MED — кат-сцена one-shot, topic повторяется | OK по дизайну; не блокировать повтор rescue topic |
| `event:eventHarveyMineRescueDating` | C# eventsSeen on finish + topic fallback; repeat → topic only | 1. One-shot story | MED — кат-сцена one-shot, topic повторяется | OK по дизайну; не блокировать повтор rescue topic |
| `event:eventHarveyMinorMineRescue` | C# eventsSeen on finish + topic fallback; repeat → topic only | 1. One-shot story | MED — кат-сцена one-shot, topic повторяется | OK по дизайну; не блокировать повтор rescue topic |
| `event:eventHarveyFirstDate` | eventsSeen / topics (chain) | 1. One-shot story | MED — нет seen guard на ключе | Добавить !PLAYER_HAS_SEEN_EVENT на ключ |
| `event:eventHarveyMountainDate` | eventsSeen / topics (chain) | 1. One-shot story | MED — нет seen guard на ключе | Добавить !PLAYER_HAS_SEEN_EVENT на ключ |
| `event:eventHarveyPropose` | eventsSeen / topics (chain) | 1. One-shot story | MED — нет seen guard на ключе | Добавить !PLAYER_HAS_SEEN_EVENT на ключ |
| `event:eventHarveyTraumaExam` | eventsSeen / topics (chain) | 1. One-shot story | MED — нет seen guard на ключе | Добавить !PLAYER_HAS_SEEN_EVENT на ключ |
| `C# ApplyBadlyHurtFromMinePassOut` | Прямой ApplyBadlyHurt без AppliedTriggers gate | 2. Repeatable injury | MED — обходит one-shot для badly hurt в шахте | OK для mine death; документировать исключение |

**Статус:** черновик аудита одноразовости.
