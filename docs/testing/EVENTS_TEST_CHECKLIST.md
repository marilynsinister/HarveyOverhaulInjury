# Чеклист ручного тестирования событий Harvey Overhaul

**Дата документа:** 2026-05-24  
**Моды:** C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`  
**Файлы событий:** `events.json`, `eventsCare.json`, `eventsMineRescue.json`

> Отмечайте `- [ ]` → `- [x]` по мере проверки.  
> Для травм, topics и mail см. также [`FOR_TEST.md`](FOR_TEST.md) и [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md).

---

## Журнал прогона

| Поле | Значение |
|------|----------|
| Тестер | |
| Слот сохранения | |
| Версия C# мода | |
| Версия CP | |
| Дата начала | |
| Дата окончания | |

**Сводка (заполнять вручную):**

| Категория | Всего | Проверено | Баги |
|-----------|------:|----------:|-----:|
| Шахта / экстренные | 8 | | |
| Лечение / госпиталь | 11 | | |
| Onboarding / ферма | 7 | | |
| Pass-out / ночь | 4 | | |
| Storm comfort | 6 | | |
| Story E1–E9 | 12 | | |
| Romance | 5 | | |
| Прочее | 3 | | |
| **Итого активных** | **56** | | |

---

## Как пользоваться

### Подготовка

1. Установить актуальные C# + CP в `Mods/`.
2. В `config.json` мода: `SendLetters: true`.
3. Открыть консоль SMAPI (`\`).
4. Перед каждым изолированным сценарием: `injury_reset`.
5. В игре **F10** — debug-HUD (травмы, topics, `LastClickDebug`).
6. Для аудита ID: `injury_audit_content` → смотреть SMAPI-лог.

### Что проверять у каждого события

| Критерий | Как |
|----------|-----|
| **Запуск** | Cutscene стартует при ожидаемых условиях |
| **Карта** | Нет застреваний, чёрного экрана, NPC в стене |
| **Движение** | `move` / `advancedMove` доходят до цели |
| **Анимации** | `animate` / `showFrame` / `stopAnimation` корректны |
| **Диалог** | Реплики, emote, портреты соответствуют сцене |
| **Topics / mail** | Нужные topics/mail появились и снялись |
| **Финал** | `end`, warp, `changeLocation` — игрок в валидном месте |
| **Повтор** | One-shot: второй раз не дублируется (если задумано) |

### Полезные команды

```
injury_reset
injury_debuff_add <buffId>
injury_debuff_list
injury_phase_list
injury_phase_ready <buffId> 1
injury_phase_advance <buffId>
injury_debug_mine_rescue
injury_night_visit_reset
injury_audit_content
```

### Как форсировать событие (если нет естественного триггера)

| Способ | Когда |
|--------|-------|
| Игровые условия + вход в локацию | Большинство CP entry-событий |
| SMAPI `debug event <EventID>` | Быстрая проверка скрипта (может пропустить preconditions) |
| C# debug-команды | Mine rescue, debuff, phase |
| Отдельный тест-слот | Story arc E1→E9, romance chain |

---

## Сценарии тестирования (пошагово)

Отмечайте сценарий целиком, когда все шаги пройдены.

### S01 — Первая травма и First Treatment

- [ ] **S01** First treatment chain

```
injury_reset
injury_debuff_add buffHurt
```

1. Friendship ≥ 750, Hospital 9:00–21:00.
2. Ожидание: `topicHarveyNeedsFirstTreatment` → событие `HarveyMod_FirstTreatment`.
3. Проверить: `topicFirstTreatmentComplete`, cutscene без ошибок карты.

---

### S02 — План лечения (серьёзная травма)

- [ ] **S02** Treatment plan meeting

```
injury_reset
injury_debuff_add buffConcussion
```

1. Клик по Харви → начало лечения.
2. Ожидание: `topicDiagnosisComplete`.
3. Hospital 9:00–17:00 → `HarveyMod_TreatmentPlanMeeting`.
4. Проверить fork agree/refusal topics.

---

### S03 — Фазовая травма (полный цикл)

- [ ] **S03** Phased injury DeepCuts

```
injury_reset
injury_debuff_add buffDeepCuts
```

1. Клик Харви → лечение, `topicTreatmentDeepCuts`.
2. `injury_phase_ready buffDeepCuts 1` → клик → смена фазы.
3. Повтор до recovery → `topicDeepCutsCured` → финальный диалог.

---

### S04 — Major mine rescue (dating)

- [ ] **S04** Mine rescue dating

```
injury_reset
injury_debug_mine_rescue
```

1. Лечь спать → утро.
2. Warp в Mine → `eventHarveyMineRescueDating`.
3. Проверить: Hospital, `topicMineInjuryRescue`, `buffBadlyHurt`, госпитализация.

---

### S05 — Minor mine rescue

- [ ] **S05** Minor mine rescue

```
injury_reset
injury_debuff_add buffBackStrain
```

1. Dating с Харви.
2. Mine с HP ≤ 35% или stamina ≤ 15%.
3. Ожидание: `eventHarveyMinorMineRescue` (не major rescue).

---

### S06 — Pass-out в Town → утренний осмотр

- [ ] **S06** Town pass-out chain

1. Dating, остаться в Town до 2:00+.
2. Утро: `topicPassedOutInTown`, `mailHarveySleepControl`.
3. Farm 22:00–02:00 → `eventHarveyCheckFarmerOutsideAfter22`.
4. Следующее утро 6:00–8:00 → `eventHarveyMorningCheckup`.

---

### S07 — Critical pass-out (не шахта)

- [ ] **S07** Emergency care

1. Dating, HP ≤ 10, обморок **вне** Mine.
2. Ожидание: `eventHarveyEmergencyCare` → Hospital.

---

### S08 — Истощение (exhaustion)

- [ ] **S08** Exhaustion hospital

1. Dating, stamina ≤ -15, обморок вне шахты.
2. Ожидание: `eventHarveyExhaustion`, `topicHarveyExhaustion`.

---

### S09 — Dirty wound → infection

- [ ] **S09** Dirty wound escalation

```
injury_reset
injury_debuff_add buffDeepCuts
```

1. Шахта 60+ мин (или `injury_mine_dirty_debug`).
2. Ожидание: `HarveyMod_DirtyWound` → mail `HarveyMod_DirtyWoundInfection` → `buffInfectedWound`.

---

### S10 — Wet bandage

- [ ] **S10** Wet bandage

```
injury_reset
injury_debuff_add buffHurt
```

1. Клик Харви (лечение) → дождь / `injury_debuff_add HarveyMod_WetBandage`.
2. Эскалация → mail `HarveyMod_WetBandageInfection`.

---

### S11 — Severe → запрет шахты

- [ ] **S11** Mine forbidden

```
injury_debuff_add buffBadlyHurt
```

1. Войти в Mine/Volcano в тот же день.
2. Сон → утро: `mailHarveyMineForbidden`, `HarveyMod_MineForbidden`.

---

### S12 — Storm comfort (C# roll)

- [ ] **S12** Storm comfort

1. Friendship ≥ 750, гроза 12:00–22:00.
2. Ждать daily roll C# (35%) → `buffStressThunder` или `topicHarveyStormStress`.
3. Войти в Farm/Forest/Town/Mine/Desert/Mountain → одно из `eventHarveyStormComfort*`.

---

### S13 — Story arc E1→E9 (полная цепочка)

- [ ] **S13** Story arc полностью

| Шаг | Event ID | Локация | Ключевые условия |
|-----|----------|---------|------------------|
| 1 | `HarveyOverhaulStory.E1_SlipperyPath` | BusStop | Wind, 7:00–14:00, 2♥ |
| 2 | `HarveyOverhaulStory.E2_InsistentExam` | Hospital | Seen E1, 3♥ |
| 3 | `HarveyOverhaulStory.E2B_QuietAgreement` | Town | Seen E2, sunny/wind |
| 4 | `HarveyOverhaulStory.E3_ForestApothecary` | Forest | Thu–Sat, sunny, seen E2B |
| 5 | `HarveyOverhaulStory.E3B_WingPatient` | Forest | Seen E3, sunny |
| 6 | `HarveyOverhaulStory.E4_PierBreath` | Beach | Seen E3B, вечер, sunny |
| 7 | `HarveyOverhaulStory.E4B_TooQuiet` | Mountain | Seen E4, вечер |
| 8 | `HarveyOverhaulStory.E5_StormBeside` | Hospital | **Storm**, seen E4B, 6♥ |
| 9 | `HarveyOverhaulStory.E6_SayItOutLoud` | Hospital | Seen E5, вечер |
| 10 | `HarveyOverhaulStory.E7_TownSip_Sunny` | Town | Seen E6, sunny midday |
| 11 | `HarveyOverhaulStory.E8_QuietShelf` | ArchaeologyHouse | **Sat** 10–16, seen E7 |
| 12 | `HarveyOverhaulStory.E9_LightInWindow` | Town | Seen E8, вечер |

После E5: проверить `topicRescueOperation` → `eventRescueOperation` (Woods, storm).

---

### S14 — Onboarding (новое сохранение)

- [ ] **S14** Care onboarding chain

| # | Event ID | Когда |
|---|----------|-------|
| 1 | `eventHarveyFirstMeeting` | Первый визит BusStop |
| 2 | `eventHarveyFirstVisit` | Farm, day ≥ 3 |
| 3 | `eventHarveySecondVisit` | Farm, day ≥ 7 |
| 4 | `eventHarveyFirstWalk` | Farm, day ≥ 11, sunny |

---

### S15 — Romance milestones

- [ ] **S15** Romance chain (dating)

| Event ID | Локация | Hearts | Прочее |
|----------|---------|--------|--------|
| `eventHarveyFirstDate` | Forest | 8♥ | Sunny evening, не winter |
| `eventHarveyMountainDate` | Mountain | 9♥ | Sunny morning |
| `eventHarveyPropose` | Beach | 10♥ | Sunny evening |

---

### S16 — Night crisis (после First Treatment)

- [ ] **S16** Night crisis

1. Пройти `HarveyMod_FirstTreatment`.
2. Hospital 22:00–26:00, 6♥+.
3. Dating → `HarveyMod_NightCrisis_Dating`; иначе → `HarveyMod_NightCrisis_PreDating`.

---

### S17 — Mine / Skull interception (SpaceCore)

- [ ] **S17** Mine interception

```
injury_debuff_add buffDeepCuts
```

1. Dating, SpaceCore загружен.
2. Вход в Mine → `eventHarveyMineInterception`.
3. Skull Cave exit → `eventHarveySkullCavePrevention`.

---

### S18 — Аудит контента

- [ ] **S18** Content audit

```
injury_audit_content
```

Ожидание: 0 `MISSING in CP` для mail/topics (gate-only topics — OK).

---

## Чеклист событий (по одному)

**Легенда колонок:**  
☐ Cutscene · ☐ Карта · ☐ Topics · ☐ Повтор OK · Заметки

---

### A. Шахта и экстренные (C# + CP)

| ☐ | Event ID | Локация | Как запустить | Cutscene | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:--------:|:-----:|:------:|:------:|---------|
| [ ] | `eventHarveyMineRescueDating` | Mine | S04 / смерть в шахте, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMineRescue` | Mine | Legacy fallback (!dating) | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMinorMineRescue` | Mine | S05 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMineInterception` | Mine | S17, вход с травмой | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveySkullCavePrevention` | SkullCave | S17, выход | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyEmergencyCare` | Hospital | S07 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyExhaustion` | Hospital | S08 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventRescueOperation` | Woods | После E5 / storm, topic + storm | [ ] | [ ] | [ ] | [ ] | |

---

### B. Лечение и госпиталь

| ☐ | Event ID | Локация | Как запустить | Cutscene | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:--------:|:-----:|:------:|:------:|---------|
| [ ] | `HarveyMod_FirstTreatment` | Hospital | S01 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_TreatmentPlanMeeting` | Hospital | S02 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_NightCrisis_Dating` | Hospital | S16, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_NightCrisis_PreDating` | Hospital | S16, !dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_BirthdayHospital_Dating` | Hospital | 9 summer, dating, Hospital | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_BirthdayHospital_Friend` | Hospital | 9 summer, !dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMedicalCheck` | Hospital | Mail reminder, !dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMedicalCheck_Dating` | Hospital | Mail reminder, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyTraumaExam` | Hospital | 8♥, Hospital днём | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyCheckup` | Hospital | `topicAgreedCheckup` (fork first meeting) | [ ] | [ ] | [ ] | [ ] | ⚠️ C# topic не ставит |
| [ ] | `eventHarveyTreatmentCollapse` | Hospital | Orphan — только manual debug | [ ] | [ ] | [ ] | [ ] | ❌ нет launcher |
| [ ] | `eventStayInHospital` | Hospital | Orphan — только manual debug | [ ] | [ ] | [ ] | [ ] | ❌ нет launcher |

---

### C. Onboarding и визиты на ферму

| ☐ | Event ID | Локация | Как запустить | Cutscene | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:--------:|:-----:|:------:|:------:|---------|
| [ ] | `eventHarveyFirstMeeting` | BusStop | S14, новый слот | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyFirstVisit` | Farm | S14, day ≥ 3 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveySecondVisit` | Farm | S14, day ≥ 7 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyFirstWalk` | Farm | S14, day ≥ 11, sunny | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyCheckHealthFarmer` | Farm | После vanilla `PlayerKilled`, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyCheckFarmerOutsideAfter22` | Farm | S06 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMorningCheckup` | Farm | S06, 6:00–8:00, dating | [ ] | [ ] | [ ] | [ ] | ⚠️ married? |

---

### D. Pass-out и поздняя ночь

| ☐ | Event ID | Локация | Как запустить | Cutscene | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:--------:|:-----:|:------:|:------:|---------|
| [ ] | `eventHarveyLateNightCollapse` | Town | Town 24:00–26:00 | [ ] | [ ] | [ ] | [ ] | |

---

### E. Storm comfort (6 локаций)

После C# roll (`buffStressThunder` / `topicHarveyStormStress`) — войти в локацию в день грозы.

| ☐ | Event ID | Локация | Cutscene | Карта | Topics | Заметки |
|:-:|----------|---------|:--------:|:-----:|:------:|---------|
| [ ] | `eventHarveyStormComfortFarm` | Farm | [ ] | [ ] | [ ] | weight 0.6 |
| [ ] | `eventHarveyStormComfortForest` | Forest | [ ] | [ ] | [ ] | weight 0.55 |
| [ ] | `eventHarveyStormComfortTown` | Town | [ ] | [ ] | [ ] | weight 0.3 |
| [ ] | `eventHarveyStormComfortMine` | Mine | [ ] | [ ] | [ ] | weight 0.8 |
| [ ] | `eventHarveyStormComfortMountain` | Mountain / SVE Summit | [ ] | [ ] | [ ] | weight 0.4 |
| [ ] | `eventHarveyStormComfortDesert` | Desert | [ ] | [ ] | [ ] | weight 0.3 |

- [ ] **S12** Storm comfort (весь блок E)

---

### F. Story arc E1–E9

| ☐ | Event ID | Локация | Cutscene | Карта | Topics / mail | Заметки |
|:-:|----------|---------|:--------:|:-----:|---------------|---------|
| [ ] | `HarveyOverhaulStory.E1_SlipperyPath` | BusStop | [ ] | [ ] | [ ] | Wind, 2♥ |
| [ ] | `HarveyOverhaulStory.E2_InsistentExam` | Hospital | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E2B_QuietAgreement` | Town | [ ] | [ ] | [ ] | Sunny/wind |
| [ ] | `HarveyOverhaulStory.E3_ForestApothecary` | Forest | [ ] | [ ] | [ ] | Thu–Sat |
| [ ] | `HarveyOverhaulStory.E3B_WingPatient` | Forest | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E4_PierBreath` | Beach | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E4B_TooQuiet` | Mountain | [ ] | [ ] | [ ] | Sunny/wind |
| [ ] | `HarveyOverhaulStory.E5_StormBeside` | Hospital | [ ] | [ ] | [ ] | **Storm** → rescue topic |
| [ ] | `HarveyOverhaulStory.E6_SayItOutLoud` | Hospital | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E7_TownSip_Sunny` | Town | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E8_QuietShelf` | ArchaeologyHouse | [ ] | [ ] | [ ] | **Saturday** |
| [ ] | `HarveyOverhaulStory.E9_LightInWindow` | Town | [ ] | [ ] | [ ] | Финал арки |

- [ ] **S13** Story arc (вся цепочка без пропусков)

---

### G. Romance и комната Харви

| ☐ | Event ID | Локация | Cutscene | Карта | Заметки |
|:-:|----------|---------|:--------:|:-----:|---------|
| [ ] | `eventHarveyFirstDate` | Forest | [ ] | [ ] | Dating, 8♥ |
| [ ] | `eventHarveyMountainDate` | Mountain | [ ] | [ ] | Dating, 9♥ |
| [ ] | `eventHarveyPropose` | Beach | [ ] | [ ] | Dating, 10♥ |
| [ ] | `eventHarveyRoomCheckup` | HarveyRoom | [ ] | [ ] | 6♥ |
| [ ] | `eventHarveyRoomCheckup2` | HarveyRoom | [ ] | [ ] | Dating + BETAS |

- [ ] **S15** Romance (вся цепочка)

---

### H. Debug / не подключено

| ☐ | Event ID | Статус | Заметки |
|:-:|----------|--------|---------|
| [ ] | `eventHarveyCareMovementAnimationTest` | Debug-only | Hospital, manual |
| [ ] | `MyMod_HarveyUrgentFarmVisit` | 💀 не в content.json | |
| [ ] | `MyMod_HarveyStormComfortForest` | 💀 не в content.json | |
| [ ] | `MyMod_HarveyStressTiredCheck` | 💀 не в content.json | |

---

## Чеклист травм и лечения (C#, не cutscene)

Сверка с [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md).

| ☐ | Buff / сценарий | Topic → cured | Mail | Тон 0♥ / dating |
|:-:|-----------------|---------------|------|-----------------|
| [ ] | `buffHurt` | [ ] | [ ] | [ ] |
| [ ] | `buffBadlyHurt` | [ ] | [ ] | [ ] |
| [ ] | `buffDeepCuts` (фазы) | [ ] | [ ] | [ ] |
| [ ] | `buffConcussion` | [ ] | [ ] | [ ] |
| [ ] | `buffFracturedBone` | [ ] | [ ] | [ ] |
| [ ] | `buffBurnWounds` | [ ] | [ ] | [ ] |
| [ ] | `buffInfectedWound` | [ ] | [ ] | [ ] |
| [ ] | `buffCold` | [ ] | [ ] | [ ] |
| [ ] | `HarveyMod_DirtyWound` | [ ] | [ ] | [ ] |
| [ ] | `HarveyMod_WetBandage` | [ ] | [ ] | [ ] |
| [ ] | `HarveyMod_Neglect` | [ ] | [ ] | [ ] |
| [ ] | Ночной визит (Severe) | [ ] | — | [ ] |

---

## Чеклист перед релизом

- [ ] S01–S03: debuff → treat → cured
- [ ] S04–S05: mine rescue major + minor
- [ ] S06–S08: pass-out цепочки
- [ ] S09–S10: dirty / wet → infection mail
- [ ] S11: Severe mine forbidden
- [ ] S12: storm comfort (хотя бы 2 локации)
- [ ] S13: story E1→E9 (или spot-check ключевых E5, E8, E9)
- [ ] S14: onboarding на чистом слоте
- [ ] S15: romance (если релевантно)
- [ ] S16: night crisis обе ветки
- [ ] S17: mine/skull interception
- [ ] S18: `injury_audit_content` без missing
- [ ] F10 HUD без ошибок на всех сценариях
- [ ] SMAPI-лог: нет exception при `startEvent`

---

## Журнал багов

| # | Event ID | Описание | Severity | Статус |
|---|----------|----------|----------|--------|
| 1 | | | | |
| 2 | | | | |
| 3 | | | | |

---

## Связанная документация

| Документ | Содержание |
|----------|------------|
| [`FOR_TEST.md`](FOR_TEST.md) | Команды, травмы, pass-out, mail |
| [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md) | Детальные сценарии topics/mail/тон |
| [`../events-inventory/14-scenario-chains.md`](../events-inventory/14-scenario-chains.md) | Диаграммы C# → CP |
| [`../events-inventory/00-summary-table.md`](../events-inventory/00-summary-table.md) | Сводка всех Event ID |
| [`../events-inventory/07-reachability-table.md`](../events-inventory/07-reachability-table.md) | Достижимость и риски |
| [`../CheckEvent/cp-event-review-checklist.md`](../CheckEvent/cp-event-review-checklist.md) | Чеклист правки одного события |
