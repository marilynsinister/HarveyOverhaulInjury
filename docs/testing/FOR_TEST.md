# Справочник для отладки Harvey Overhaul Injury

Полный чеклист для проверки мода в игре: события, травмы, топики, жизненный цикл и консольные команды SMAPI.

**Моды:** C# `HarveyOverhaulInjury` + Content Patcher `HarveyOverhaul [CP]`  
**UniqueID C#:** `marilynsinister.HarveyOverhaul.Injury`

---

## Быстрый старт

1. Убедитесь, что загружены **C# мод** и **Content Patcher**.
2. Откройте консоль SMAPI (`\` по умолчанию).
3. Перед каждым тестом: `injury_reset` — полный сброс состояния мода.
4. В игре нажмите **F10** — debug-HUD с активными травмами, флагами, топиками.
5. В `config.json` мода: `SendLetters: true` — иначе письма не проверить.

---

## Консольные команды SMAPI

| Команда | Назначение |
|---------|------------|
| `injury_reset` | Полный сброс: баффы, state, топики `topic*` / `situation*` |
| `injury_debuff_list` | Список всех ID травм и осложнений + фазы |
| `injury_debuff_add <id> [минуты]` | Наложить травму/осложнение (+ DebuffState + topic). Минуты по умолчанию `-2` (весь день) |
| `injury_phase_list` | Активные травмы: фаза, `TreatmentStarted`, флаги готовности |
| `injury_phase_ready <buffId> [1\|0]` | Эмуляция «фаза истекла, можно сменить» |
| `injury_phase_recovery <buffId> [1\|0]` | Эмуляция «можно завершить лечение» |
| `injury_phase_advance <buffId>` | Принудительная смена фазы |
| `injury_phase_cure <buffId>` | Полное выздоровление без клика |
| `injury_rain_debug [secToday] [secContinuous]` | Счётчики дождя (простуда, мокрая повязка) |
| `injury_mine_dirty_debug` | Состояние риска грязной раны в шахте (read-only) |
| `injury_debug_mine_rescue` | Флаги mine rescue → сработает на следующий `DayStarted` |
| `injury_cooldowns` | Cooldown повторяемых травм |
| `injury_farming_counters` | Счётчики использования инструментов |
| `injury_night_visit_reset` | Сброс флагов ночного визита Харви |
| `injury_audit_content` | Аудит Mail/topics vs CP (только в SMAPI-лог) |

**Примеры:**

```
injury_reset
injury_debuff_add buffDeepCuts
injury_phase_ready buffDeepCuts 1
injury_debug_mine_rescue
```

**Важно:** `injury_debuff_add` не добавляет все side-topics (например, `topicHealthDamageCritical` при `buffBadlyHurt`). Для полной цепочки используйте игровой триггер или учитывайте расхождение.

---

## Debug-HUD (F10)

Показывает:

- активные травмы (фаза, лечится/нет, флаги `→след.фаза` / `→выздоровление`);
- осложнения;
- mine dirty exposure, rain counters;
- флаги pass-out / mine rescue;
- cooldowns;
- все mod topics;
- `LastClickDebug` — последняя ветка обработки клика по Харви.

---

## Жизненный цикл травмы

```
[Триггер] → buff + topic + DebuffState
    ↓ клик по Харви (InteractionHandler)
[Начало лечения] → TreatmentStarted = true
    • Нефазовые (Hurt, BadlyHurt, Surgical): замена на cure-buff, авто-complete через N дней → topic*Cured
    • Фазовые: buff → Phase1 buff, topicTreatment*, topic*PhaseAcute
    ↓ каждый DayStarted
[Фаза elapsed] → ReadyForNextPhase / ReadyForRecovery
    ↓ клик по Харви
[Advance / CompleteRecovery] → topic*Cured + buffHarveyCare
    ↓ клик при topic*Cured
[Финальный диалог] → снятие cure-buff, care-buff
```

**Мосты к CP-событиям при первой/серьёзной травме:**

- `topicHarveyNeedsFirstTreatment` (7 д) → `HarveyMod_FirstTreatment`
- `topicDiagnosisComplete` → `HarveyMod_TreatmentPlanMeeting`

**Cooldown:** повторная та же травма — через 7 дней (`RepeatableInjuryCooldownDays`) + 30 игровых минут между apply. После выздоровления — +2 дня residual cooldown.

---

## Травмы: как получить, фазы, топики, назначение

| Buff ID | Как получить | Фазы (дни) | Cure-buff | Топики при наложении | Назначение |
|---------|--------------|------------|-----------|----------------------|------------|
| `buffHurt` | Урон ≥5, 35% | 3 (нефаз.) | `buffHarveyTreatment` | `topicHurt` (3д) | Лёгкая царапина |
| `buffBadlyHurt` | HP ≤10 после урона; pass-out HP≤10; смерть в шахте | 8 (нефаз.) | `buffHarveyIntensiveCare` | `topicBadlyHurt`, `topicHealthDamageCritical` | Тяжёлая травма |
| `buffSprainedAnkle` | **Нет автотриггера** — только debug | 7+7 | фазовое | `topicSprainedAnkle` (14д) | Растянутая лодыжка |
| `buffBruisedRibs` | Урон ≥15, 25% | 10+11 | фазовое | `topicBruisedRibs` (21д) | Ушибленные рёбра |
| `buffBackStrain` | Farming: 30+ uses мотыги/лейки при stamina≤30, 15% | 5+7 | фазовое | `topicBackStrain` (12д) | Боль в спине |
| `buffDeepCuts` | Combat: урон ≥10, 30%; Farming: 25+ uses косы/топора при stamina≤15, 15% | 3+7+4 | фазовое | `topicDeepCuts` (14д) | Глубокие порезы |
| `buffBurnWounds` | Взрыв рядом: 50% × 40% burn | 7+14 | фазовое | `topicBurnWounds` (21д) | Ожоги; госпитализация (dating) |
| `buffInfectedWound` | Эскалация Dirty/Wet; debug | 3+11 | фазовое | `topicInfectedWound` (14д) | Инфекция; госпитализация |
| `buffTornMuscles` | Farming: 20+ uses топора/кирки при stamina≤20, 12% | 7+14+7 | фазовое | `topicTornMuscles`, `topicHealthDamageSevere` (28д) | Разрыв мышц |
| `buffConcussion` | Урон ≥20, 25% | 3+11+7 | фазовое | `topicConcussion`, `topicHealthDamageSevere` (21д) | Сотрясение; госпитализация |
| `buffFracturedBone` | Урон ≥30, 10% | 7+35+14 | фазовое | `topicFracturedBone`, `topicHealthDamageCritical` (56д) | Перелом; госпитализация |
| `buffShrapnelWounds` | Взрыв: 50% × 60% shrapnel | 5+10+7 | фазовое | `topicShrapnelWounds`, `topicHealthDamageCritical`, `topicPostOperativeCare` | Осколки; госпитализация |
| `buffSurgicalWound` | **Story one-shot** — только debug/ручной | 14 (нефаз.) | `buffPostSurgicalCare` | `topicSurgicalWound`, `topicPostOperativeCare` | После операции |
| `buffCold` | Дождь снаружи: 5/10/15/20+ мин → 5/20/50/80% | 3+4 | фазовое | `topicCold` (7д) | Простуда |

**Приоритет combat-урона** (одна травма за hit, от тяжёлой к лёгкой): BadlyHurt (HP≤10) → Fracture → Concussion → BruisedRibs → DeepCuts → Hurt.

**Наборы для проверок:**

- **Severe:** BadlyHurt, Shrapnel, Fracture, Concussion, Surgical, Infected, Burn — mine warning, night visit, госпитализация.
- **DirtyInMines:** DeepCuts, Burn, Shrapnel — риск `HarveyMod_DirtyWound`.
- **Critical:** Concussion, Fracture, BadlyHurt, Infected.

**Топики при лечении (динамические):**

- `topicTreatment{buffName}` — начало лечения
- `topic{Injury}PhaseAcute/Healing/Recovery` — фазы
- `topic{Injury}Cured` / `topicColdCured` — выздоровление
- `topicTreatmentCompleted` — финал recovery

---

## Осложнения

| Buff ID | Как получить | Эскалация | Topic | Mail при эскалации |
|---------|--------------|-----------|-------|-------------------|
| `HarveyMod_DirtyWound` | Шахта с open-wound травмой (exposure roll) | День 1: 15%, 2: 40%, 3+: 100% → `buffInfectedWound` | `topicHarvey_DirtyWound` (4д) | `HarveyMod_DirtyWoundInfection` |
| `HarveyMod_WetBandage` | Дождь при `buffHarveyTreatment`/`IntensiveCare`; спа/баня с повязкой | День 1: 15%, 2: 35%, 3+: 65% → infection | `topicHarvey_WetBandage` (4д) | `HarveyMod_WetBandageInfection` |
| `HarveyMod_WetStitches` | Спа/баня при `buffSurgicalWound` | — | `topicHarvey_WetStitches` (4д) | — |
| `HarveyMod_Neglect` | 3+ дня без лечения; просрочка фазы +7 grace | HUD + письмо | `topicHarvey_Neglect` | `HarveyMod_NeglectWarning` |
| `HarveyMod_PainFlare` | **Нет gameplay-триггера** — только debug | Night visit снимает 50% | `topicHarvey_PainFlare` | — |
| `HarveyMod_AllergicRash` | **Нет gameplay-триггера** | — | `topicHarvey_AllergicRash` | — |
| `HarveyMod_MineForbidden` | Утро после Severe + вход в Mine/Volcano | 2 дня (config) | — | `mailHarveyMineForbidden` |

**Просрочка фазы лечения:** `HarveyMod_TreatmentUrgentReminder` → `HarveyMod_TreatmentFinalWarning` → Neglect.

---

## События: C# → CP

### Полный реестр событий (52 ID в активном CP)

Источники: `events.json`, `eventsCare.json`, `eventsMineRescue.json` (подключены через `content.json`).  
Файл `events_for_mode_new_formatted.json` **не подключён** — 3 legacy `MyMod_*` внизу таблицы.

| # | Event ID | Локация | Запуск | Статус |
|---|----------|---------|--------|--------|
| **Шахта / экстренные (C#)** |||||
| 1 | `eventHarveyMineRescueDating` | Mine | C# `PassOutHandler` → warp утром | ✅ основной rescue (dating/married) |
| 2 | `eventHarveyMineRescue` | Mine | C# fallback (legacy) | ⚠️ без dating |
| 3 | `eventHarveyMinorMineRescue` | Mine | C# `TryTriggerMinorMineRescue` | ✅ опасное состояние без Severe |
| 4 | `eventHarveyMineInterception` | Mine | SpaceCore `triggersCare.json` | ⚠️ при входе с травмой |
| 5 | `eventHarveySkullCavePrevention` | SkullCave | SpaceCore `triggersCare.json` | ⚠️ при выходе |
| 6 | `eventHarveyEmergencyCare` | Hospital | C# `PassOutHandler` (HP≤10 вне шахты) | ✅ |
| 7 | `eventHarveyExhaustion` | Hospital | C# `PassOutHandler` (stamina≤-15) | ✅ |
| 8 | `eventRescueOperation` | Woods | C# topic gate + vanilla entry | ✅ после E5 / storm |
| **Лечение / госпиталь** |||||
| 9 | `HarveyMod_FirstTreatment` | Hospital | CP entry + `topicHarveyNeedsFirstTreatment` (C#) | ✅ |
| 10 | `HarveyMod_TreatmentPlanMeeting` | Hospital | CP entry + `topicDiagnosisComplete` (C#) | ✅ |
| 11 | `HarveyMod_NightCrisis_Dating` | Hospital | CP entry (dating/married) | ✅ |
| 12 | `HarveyMod_NightCrisis_PreDating` | Hospital | CP entry (!dating) | ✅ |
| 13 | `HarveyMod_BirthdayHospital_Dating` | Hospital | CP entry (9 summer, dating) | ⚠️ |
| 14 | `HarveyMod_BirthdayHospital_Friend` | Hospital | CP entry (9 summer, !dating) | ⚠️ |
| 15 | `eventHarveyMedicalCheck` | Hospital | CP entry + mail | ✅ !dating |
| 16 | `eventHarveyMedicalCheck_Dating` | Hospital | CP entry + mail | ✅ dating |
| 17 | `eventHarveyTraumaExam` | Hospital | CP entry (8♥) | ✅ |
| 18 | `eventHarveyCheckup` | Hospital | CP entry + `topicAgreedCheckup` | ⚠️ C# topic не ставит |
| 19 | `eventHarveyTreatmentCollapse` | Hospital | — | ❌ orphan, нет launcher |
| 20 | `eventStayInHospital` | Hospital | — | ❌ orphan, заменено C# hosp |
| **Onboarding / визиты на ферму** |||||
| 21 | `eventHarveyFirstMeeting` | BusStop | CP entry | ✅ (дубль в events + eventsCare) |
| 22 | `eventHarveyFirstVisit` | Farm | CP entry | ✅ |
| 23 | `eventHarveySecondVisit` | Farm | CP entry | ✅ |
| 24 | `eventHarveyFirstWalk` | Farm | CP entry | ✅ |
| 25 | `eventHarveyMorningCheckup` | Farm | CP entry + `topicHarveyMandatoryCheckup` | ✅ |
| 26 | `eventHarveyCheckFarmerOutsideAfter22` | Farm | CP entry + `topicPassedOutInTown` (C#) | ✅ |
| 27 | `eventHarveyCheckHealthFarmer` | Farm | CP entry (после `PlayerKilled`) | ✅ |
| **Pass-out / поздняя ночь** |||||
| 28 | `eventHarveyLateNightCollapse` | Town | CP entry (24:00–26:00) | ⚠️ |
| **Storm comfort (C# roll → CP cutscene)** |||||
| 29 | `eventHarveyStormComfortFarm` | Farm | C# `StormComfortLauncher` + CP Random | ✅ |
| 30 | `eventHarveyStormComfortForest` | Forest | то же | ✅ |
| 31 | `eventHarveyStormComfortTown` | Town | то же | ✅ |
| 32 | `eventHarveyStormComfortMine` | Mine | то же | ✅ |
| 33 | `eventHarveyStormComfortMountain` | Custom_AdventurerSummit / Mountain | то же (+ SVE act 2) | ✅ |
| 34 | `eventHarveyStormComfortDesert` | Desert | то же | ✅ |
| **Сюжетная арка HarveyOverhaulStory (E1–E9)** |||||
| 35 | `HarveyOverhaulStory.E1_SlipperyPath` | BusStop | CP entry (ветер, 2♥) | ✅ |
| 36 | `HarveyOverhaulStory.E2_InsistentExam` | Hospital | CP entry (3♥, seen E1) | ✅ |
| 37 | `HarveyOverhaulStory.E2B_QuietAgreement` | Town | CP entry (seen E2) | ✅ |
| 38 | `HarveyOverhaulStory.E3_ForestApothecary` | Forest | CP entry (Thu–Sat, 4♥) | ✅ |
| 39 | `HarveyOverhaulStory.E3B_WingPatient` | Forest | CP entry (seen E3) | ✅ |
| 40 | `HarveyOverhaulStory.E4_PierBreath` | Beach | CP entry (seen E3B) | ✅ |
| 41 | `HarveyOverhaulStory.E4B_TooQuiet` | Mountain | CP entry (seen E4) | ✅ |
| 42 | `HarveyOverhaulStory.E5_StormBeside` | Hospital | CP entry (шторм, 6♥) | ✅ → `topicRescueOperation` |
| 43 | `HarveyOverhaulStory.E6_SayItOutLoud` | Hospital | CP entry (7♥, seen E5) | ✅ |
| 44 | `HarveyOverhaulStory.E7_TownSip_Sunny` | Town | CP entry (8♥, sunny) | ✅ |
| 45 | `HarveyOverhaulStory.E8_QuietShelf` | ArchaeologyHouse | CP entry (Sat, 8♥) | ✅ |
| 46 | `HarveyOverhaulStory.E9_LightInWindow` | Town | CP entry (seen E8) | ✅ |
| **Romance milestones** |||||
| 47 | `eventHarveyFirstDate` | Forest | CP entry (dating, 8♥) | ✅ |
| 48 | `eventHarveyMountainDate` | Mountain | CP entry (dating, 9♥) | ✅ |
| 49 | `eventHarveyPropose` | Beach | CP entry (dating, 10♥) | ✅ |
| 50 | `eventHarveyRoomCheckup` | HarveyRoom | CP entry (6♥) | ✅ |
| 51 | `eventHarveyRoomCheckup2` | HarveyRoom | CP entry (dating + BETAS) | ⚠️ |
| **Debug / тест** |||||
| 52 | `eventHarveyCareMovementAnimationTest` | Hospital | CP entry (manual) | debug-only |
| **Не подключены (`events_for_mode_new_formatted.json`)** |||||
| — | `MyMod_HarveyUrgentFarmVisit` | Farm | — | 💀 не в content.json |
| — | `MyMod_HarveyStormComfortForest` | Forest | — | 💀 не в content.json |
| — | `MyMod_HarveyStressTiredCheck` | Hospital | — | 💀 не в content.json |

**Цепочка story-арки:** E1 → E2 → E2B → E3 → E3B → E4 → E4B → **E5** → E6 → E7 → E8 → E9.

---

### Запускаются из C# (`startEvent` / warp)

| Event ID | Условия | Топики | Назначение |
|----------|---------|--------|------------|
| `eventHarveyMineRescue` / `eventHarveyMineRescueDating` | Смерть в Mine (HP=0), dating/married, `!seen` | `topicMineInjuryRescue`, `topicMineRescuePending` (gate) | Major rescue → Hospital |
| `eventHarveyMinorMineRescue` | Mine/Volcano, dating, есть травма (не Severe), HP≤35% или stamina≤15%, `!seen`, не сегодня | `topicHarveyMinorMineRescue` | Опасное состояние без боевой смерти |
| `eventHarveyEmergencyCare` | Pass-out вне шахты, dating, HP было ≤10 | — | Критический обморок → Hospital |
| `eventHarveyExhaustion` | Pass-out от истощения (stamina≤-15), вне шахты, нет `topicFarmerExhausted` | `topicHarveyExhaustion` (CP) | Истощение → Hospital |

**Mine rescue — как проверить:**

```
injury_debug_mine_rescue
```

→ лечь спать → утро → warp в Mine → cutscene.

**Minor mine rescue:**

```
injury_debuff_add buffBackStrain
```

→ dating с Харви → Mine с low HP/stamina.

### C# gate → topic → CP precondition

| Event ID | Локация | Preconditions | Topic-мост от C# |
|----------|---------|---------------|------------------|
| `HarveyMod_FirstTreatment` | Hospital | 9:00–21:00, Friendship≥750, `!seen`, `!topicFirstTreatmentComplete`, **`topicHarveyNeedsFirstTreatment`** | C# при первой treatable травме |
| `HarveyMod_TreatmentPlanMeeting` | Hospital | 9:00–17:00, **`topicDiagnosisComplete`**, Friendship≥750 | C# после начала серьёзного лечения |
| `eventHarveyCheckFarmerOutsideAfter22` | Farm | 22:00–02:00, **`topicPassedOutInTown`**, dating | Pass-out в Town после 2:00 |
| `eventHarveyMorningCheckup` | Farm | 6:00–8:00, **`topicHarveyMandatoryCheckup`** | CP ставит topic в предыдущем событии |
| `eventRescueOperation` | Woods | Storm, **`topicRescueOperation`**, `!seen` | После E5 или storm comfort |
| `eventHarveyMineInterception` | Mine | SpaceCore trigger при входе с травмой | CP `triggersCare.json` |
| `eventHarveySkullCavePrevention` | SkullCave | SpaceCore trigger при выходе | CP `triggersCare.json` |

### Storm comfort (C# roll → CP cutscenes)

**Условия roll** (`StormComfortLauncher`): гроза (`isLightning`), 12:00–22:00, Friendship≥750, не festival, нет cooldown, 35% шанс/день.

После успеха: `buffStressThunder` или fallback `topicHarveyStormStress`.

| Event ID | Локация | Random weight |
|----------|---------|---------------|
| `eventHarveyStormComfortFarm` | Farm | 0.6 |
| `eventHarveyStormComfortForest` | Forest | 0.55 |
| `eventHarveyStormComfortMountain` | Custom_AdventurerSummit | 0.4 |
| `eventHarveyStormComfortTown` | Town | 0.3 |
| `eventHarveyStormComfortDesert` | Desert | 0.3 |
| `eventHarveyStormComfortMine` | Mine | 0.8 |

**Проверка:** dating, Friendship≥750, дождь+гроза 12–22, ждать roll или несколько дней. Debug-команды storm не эмулируют — нужен реальный `TimeChanged` + storm.

### Сюжетная арка HarveyOverhaulStory (только CP)

Цепочка: `E1_SlipperyPath` → `E2_InsistentExam` → `E2B_QuietAgreement` → `E3_ForestApothecary` → `E3B_WingPatient` → `E4_PierBreath` → `E4B_TooQuiet` → **`E5_StormBeside`** → `E6_SayItOutLoud` → `E7_TownSip_Sunny` → `E8_QuietShelf` → `E9_LightInWindow`.

**E5** — триггер для `topicRescueOperation` из C#.

### Прочие CP-события (relationship / onboarding)

| Event ID | Условия (кратко) | Назначение |
|----------|------------------|------------|
| `eventHarveyFirstMeeting` | BusStop, `!topicFirstMeeting` | Первое знакомство |
| `eventHarveyFirstVisit` / `eventHarveySecondVisit` | Farm care chain | Визиты на ферму |
| `eventHarveyFirstWalk` | Farm, после second visit | Первая прогулка |
| `eventHarveyCheckHealthFarmer` | Farm, после vanilla `PlayerKilled`, dating | Проверка после смерти |
| `eventHarveyCheckup` | BusStop, **`topicAgreedCheckup`** | **C# topic не ставит** |
| `eventHarveyMedicalCheck` / `_Dating` | Hospital, письмо reminder | Медосмотр |
| `eventHarveyTraumaExam` | Hospital, Friendship≥2000 | Травма-экзамен |
| `HarveyMod_NightCrisis_Dating` / `_PreDating` | после FirstTreatment | Ночной кризис |
| `HarveyMod_BirthdayHospital_*` | 9 summer | День рождения |
| `eventHarveyFirstDate`, `eventHarveyMountainDate`, `eventHarveyPropose` | romance milestones | Свидания |
| `eventHarveyLateNightCollapse` | Town 24:00–26:00 | Поздний коллапс |
| `eventHarveyRoomCheckup` / `eventHarveyRoomCheckup2` | HarveyRoom | Осмотр в комнате |

**Без C# bridge (только CP/script):** `eventHarveyTreatmentCollapse`, `eventStayInHospital`.

**Не подключены:** `MyMod_*` из `events_for_mode_new_formatted.json`.

---

## Pass-out pipeline (обмороки)

| Сценарий | Условие | Результат |
|----------|---------|-----------|
| Боевая смерть в Mine | HP=0, dating/married | `NeedsMineRescueEvent`, `buffBadlyHurt`, mine rescue утром |
| Критический pass-out вне шахты | HP было ≤10, dating | `eventHarveyEmergencyCare` |
| Истощение | stamina≤-15, вне шахты, dating | `eventHarveyExhaustion` |
| Поздно в Town | time≥2:00, локация Town | `topicPassedOutInTown`, `buffSleepy`, `mailHarveySleepControl` |
| Обморок в Town → утро | `topicPassedOutInTown` | `eventHarveyCheckFarmerOutsideAfter22` (22–02, Farm) → `topicHarveyMandatoryCheckup` → `eventHarveyMorningCheckup` |

---

## Ночной визит Харви

- **22:00–26:00**, FarmHouse, dating/married, **Severe**-травма.
- 35% шанс за ночь (`NightVisitChance`).
- Topic: `topicHarvey_NightRound` (2д).
- Снимает `HarveyMod_PainFlare` с 50% шансом.

**Сброс для повторного теста:** `injury_night_visit_reset`

---

## Запрет шахты (Severe)

1. `injury_debuff_add buffBadlyHurt` (или другая Severe).
2. Войти в **Mine** или **VolcanoDungeon** в тот же день.
3. HUD: «У тебя серьёзные раны…»
4. Лечь спать.
5. Утро: `mailHarveyMineForbidden` + `HarveyMod_MineForbidden` (~2 дня).

**Контроль:** `buffDeepCuts` (не Severe) → только мягкий HUD, без письма.

---

## Готовые сценарии тестирования

### 1. First treatment chain

```
injury_reset
injury_debuff_add buffHurt
```

→ Hospital 9:00–21:00, Friendship≥750 → `HarveyMod_FirstTreatment`

### 2. Treatment plan

```
injury_debuff_add buffConcussion
```

→ клик Харви (лечение) → `topicDiagnosisComplete` → Hospital → `HarveyMod_TreatmentPlanMeeting`

### 3. Фазовая травма

```
injury_debuff_add buffDeepCuts
```

→ клик Харви → `injury_phase_ready buffDeepCuts 1` → клик → `injury_phase_advance buffDeepCuts` → … → cured

### 4. Dirty → infected

```
injury_debuff_add buffDeepCuts
```

→ шахта 60+ мин / `injury_mine_dirty_debug`

### 5. Wet bandage

```
injury_debuff_add buffHurt
```

→ клик Харви (лечение) → стоять под дождём / `injury_debuff_add HarveyMod_WetBandage`

### 6. Mine major rescue

```
injury_debug_mine_rescue
```

→ сон → утро

### 7. Town pass-out

→ оставаться в Town до 2:00 → утро проверить `topicPassedOutInTown` + `mailHarveySleepControl`

### 8. Аудит контента

```
injury_audit_content
```

→ смотреть SMAPI-лог на `MISSING in CP`.

---

## Как проверять в игре

| Что | Как |
|-----|-----|
| Conversation topics | Social tab → Conversation Topics у NPC; или F10 HUD / SMAPI-лог |
| Buffs | Buffs panel в игре; F10 HUD |
| Письма | Почтовый ящик утром; `SendLetters: true` |
| События | `eventsSeen` в save; cutscene при входе в локацию |
| Клик по Харви | F10 → `LastClickDebug` |
| SMAPI-лог | Фильтры: `[MineRescue]`, `[PassOutEvent]`, `[StormComfort]`, `[FarmingInjury]`, `[ColdExposure]`, `[WetBandage]` |

---

## Сводная таблица mail (C# → CP)

| Mail ID | Сценарий |
|---------|----------|
| `mailHarveySleepControl` | Обморок в Town |
| `mailHarveyMineForbidden` | Severe + вход в шахту + сон |
| `HarveyMod_WetBandageInfection` | Эскалация wet |
| `HarveyMod_DirtyWoundInfection` | Эскалация dirty |
| `HarveyMod_TreatmentUrgentReminder` | Просрочка фазы (+3 дня) |
| `HarveyMod_TreatmentFinalWarning` | Просрочка (+6 дней) |
| `HarveyMod_NeglectWarning` | Просрочка (≥7 дней grace) |

---

## Известные пробелы (не баг теста, а ограничения)

1. **`buffSprainedAnkle`** — метод есть, автотриггера нет → только `injury_debuff_add`.
2. **`HarveyMod_PainFlare` / `HarveyMod_AllergicRash`** — только debug.
3. **`eventHarveyCheckup`** ждёт `topicAgreedCheckup` — C# не ставит.
4. **`eventHarveyTreatmentCollapse` / `eventStayInHospital`** — нет C# launcher.
5. **`SpringRashChance`** в config — не используется в коде.
6. **`injury_debuff_add`** не дублирует все side-topics из натуральных триггеров.

---

## Связанная документация в репозитории

- [`EVENTS_TEST_CHECKLIST.md`](EVENTS_TEST_CHECKLIST.md) — чеклист всех CP-событий + сценарии S01–S18
- [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md) — детальные сценарии 1–14
- [`README.md`](README.md) — индекс тестовой документации
- [`../events-inventory/00-summary-table.md`](../events-inventory/00-summary-table.md) — все 52 CP event ID (активный content pack)
- [`../events-inventory/14-scenario-chains.md`](../events-inventory/14-scenario-chains.md) — пошаговые цепочки
- [`../flow-click-harvey.md`](../flow-click-harvey.md) — логика клика по Харви

---

## Чек-лист перед релизом (кратко)

- [ ] Сценарии 1–6: debuff → topic → treat → cured topic → финальный диалог
- [ ] 7–8: complication topic + mail при инфекции
- [ ] 9: Town pass-out + `mailHarveySleepControl`
- [ ] 10: mine rescue (dating) + `topicMineInjuryRescue`
- [ ] 11: Severe mine → mail + `HarveyMod_MineForbidden`
- [ ] 12–14: тон 0♥ / dating / married на `topicHurt`, cured, complications
- [ ] `injury_audit_content` — 0 missing mail, gate-only topics OK
