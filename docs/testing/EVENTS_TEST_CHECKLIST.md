# Чеклист ручного тестирования событий Harvey Overhaul

**Дата документа:** 2026-05-25  
**Моды:** C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`  
**Файлы событий:** `events.json`, `eventsCare.json`, `eventsMineRescue.json`

> Отмечайте `- [ ]` → `- [x]` по мере проверки.  
> Для травм, topics и mail см. также [`FOR_TEST.md`](FOR_TEST.md) и [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md).

**Changelog 2026-05-25:** story-арка расширена до **E15** (+ `HarveyOverhaulRomance.E1`); после E6 — **E7_DoorSignal** и развилка E8 (trust vs main); правки обращения «Вы»→«ты» с E2 (750 FP); частичные fix координат E7/E8 (см. map-audit).

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
| Лечение / госпиталь (Injury) | 10 | | |
| Onboarding / ферма | 7 | | |
| Pass-out / ночь | 4 | | |
| Storm comfort | 6 | | |
| Story arc E1–E15 | 24 | | |
| Romance | 6 | | |
| Прочее / чужие модули | 4 | | |
| **Итого активных (Injury CP)** | **69** | | |

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
| **Тон** | E1: «Вы» (2♥); E2+: «ты» в `speak Harvey`; dating-split — см. [`audit-relationship-tone.md`](../audit-relationship-tone.md) |
| **Mail** | Новые: `mailHarveyHomeSafetyProtocol`, `mailHarveyMinesAgreement`, `mailHarveyFuturePlanNote` |

### Полезные команды

```
injury_reset
injury_debuff_add <buffId>
injury_debuff_list
injury_phase_list
injury_phase_ready <buffId> 1
injury_phase_recovery <buffId> 1
injury_phase_advance <buffId>
injury_phase_cure <buffId>
injury_foreign_topic_add <topicId> [days]
injury_debug_mine_rescue
injury_night_visit_reset
injury_audit_content
```

**Телепорт в шахту (SMAPI, не мод):**

```
warp Mine 17 7
```

Точка `(17, 7)` — та же, что использует C# для mine rescue. Другие локации:

```
warp SkullCave 5 5
warp VolcanoDungeon 1 1
```

После `injury_debug_mine_rescue` утром warp обычно **автоматический**; команда нужна для S05, S11, S17, dirty wound, storm comfort Mine.

### Как форсировать событие (если нет естественного триггера)

| Способ | Когда |
|--------|-------|
| Игровые условия + вход в локацию | Большинство CP entry-событий |
| SMAPI `warp Mine 17 7` | Шахта, interception, dirty, storm comfort Mine |
| SMAPI `debug event <EventID>` | Быстрая проверка скрипта (может пропустить preconditions) |
| C# debug-команды | Mine rescue, debuff, phase |
| Отдельный тест-слот | Story arc E1→E15, romance chain |

### Завершение лечения (команды → клик по Харви)

Чтобы Харви **сказал, что лечение завершено**, и снялись лечебные баффы — нужен **клик после debug-команды** (кроме `injury_phase_cure`, см. ниже). F10 → `LastClickDebug`.

**Фазовая травма** (`buffDeepCuts`, `buffConcussion`, …) — финал через `CompleteRecovery`:

```
injury_reset
injury_debuff_add buffDeepCuts
```

1. **Клик по Харви** → начало лечения (`topicTreatmentDeepCuts`, phase-buff).
2. Для **каждой** смены фазы (смотреть `injury_phase_list`):

```
injury_phase_ready buffDeepCuts 1
```

→ **клик по Харви** → диалог смены фазы (или `injury_phase_advance buffDeepCuts` — только механика, **без** реплики).

3. На **последней** фазе:

```
injury_phase_recovery buffDeepCuts 1
```

→ **клик по Харви** → реплика «выздоровление завершено»; сняты phase-buff и state; **`buffHarveyCare`**.

> Фазовым травмам **`topic*Cured` не ставится** — финал только через шаг 3.

**Нефазовая травма** (`buffHurt`, `buffBadlyHurt`) — финал через `topic*Cured`:

```
injury_reset
injury_debuff_add buffHurt
```

1. **Клик по Харви** → `buffHarveyTreatment` (или `buffHarveyIntensiveCare`).
2. Дождаться `topicHurtCured` (3 игровых дня / сон) **или** для теста:

```
injury_foreign_topic_add topicHurtCured 7
```

3. **Клик по Харви** → финальный осмотр; снят cure-buff, **`buffHarveyCare`**.

**Без диалога Харви** (быстрый сброс механики):

```
injury_phase_cure buffDeepCuts
```

→ HUD «Лечение завершено», `topicTreatmentCompleted`; **реплики Харви нет**, cure/phase-buff снимаются сразу.

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

### S02 — Серьёзная фазовая травма (начало лечения)

- [ ] **S02** Phased treatment start (Concussion)

```
injury_reset
injury_debuff_add buffConcussion
```

1. **Клик по Харви** → фазовое лечение: `topicTreatmentConcussion`, `topicConcussionPhaseAcute`.
2. Проверить F10: `TreatmentStarted`, phase-buff, `topicHealthDamageSevere`.
3. Завершение цикла — см. блок **«Завершение лечения»** выше (S03).

> ⚠️ C# при старте фазового лечения может поставить `topicDiagnosisComplete` — это **ошибочный мост** к CP-событию **`HarveyMod_TreatmentPlanMeeting`** (контент **Stress**, не Injury). В Injury-чеклисте это событие **не тестируем**; см. секцию H.

---

### S03 — Фазовая травма (полный цикл до выздоровления)

- [ ] **S03** Phased injury DeepCuts — полный цикл

```
injury_reset
injury_debuff_add buffDeepCuts
```

1. Клик Харви → лечение, `topicTreatmentDeepCuts`.
2. `injury_phase_ready buffDeepCuts 1` → клик → смена фазы (×2 для DeepCuts).
3. `injury_phase_recovery buffDeepCuts 1` → клик → реплика о завершении; нет injury-buff, есть `buffHarveyCare`.

---

### S04 — Major mine rescue (dating)

- [ ] **S04** Mine rescue dating

```
injury_reset
injury_debug_mine_rescue
```

1. Лечь спать → утро (C# сам сделает `warp Mine 17 7` и cutscene).
2. Или вручную после debug: `warp Mine 17 7`.
3. Проверить: Hospital, `topicMineInjuryRescue`, `buffBadlyHurt`, госпитализация.

---

### S05 — Minor mine rescue

- [ ] **S05** Minor mine rescue

```
injury_reset
injury_debuff_add buffBackStrain
```

1. Dating с Харви.
2. `warp Mine 17 7` при HP ≤ 35% или stamina ≤ 15% (или после debuff).
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

### S13 — Story arc: основная ветка E1→E15

- [ ] **S13** Story arc (main path)

Общая цепочка E1–E6, затем **E7_DoorSignal** (обязательный мост). На **E8** ветки **взаимоисключающие** (`HarveyMod_CD_E8`): либо main (ниже), либо trust-fork (S19).

| Шаг | Event ID | Локация | Ключевые условия |
|-----|----------|---------|------------------|
| 1 | `HarveyOverhaulStory.E1_SlipperyPath` | BusStop | Wind, 7:00–14:00, 2♥ — **«Вы»** |
| 2 | `HarveyOverhaulStory.E2_InsistentExam` | Hospital | Seen E1, 3♥ — **«ты»** |
| 3 | `HarveyOverhaulStory.E2B_QuietAgreement` | Town | Seen E2, 3♥ |
| 4 | `HarveyOverhaulStory.E3_ForestApothecary` | Forest | Thu–Sat, 4♥, seen E2B |
| 5 | `HarveyOverhaulStory.E3B_WingPatient` | Forest | Seen E3, 4♥ |
| 6 | `HarveyOverhaulStory.E4_PierBreath` | Beach | Seen E3B, вечер, 5♥ |
| 7 | `HarveyOverhaulStory.E4B_TooQuiet` | Mountain | Seen E4, вечер, 6♥ |
| 8 | `HarveyOverhaulStory.E5_StormBeside` | Hospital | **Storm**, seen E4B, 6♥ |
| 9 | `HarveyOverhaulStory.E6_SayItOutLoud` | Hospital | Seen E5, вечер, 7♥ |
| 10 | `HarveyOverhaulStory.E7_DoorSignal` | Farm | Seen E6, вечер, 8♥ |
| 11 | `HarveyOverhaulStory.E8_BadDayNoReason` | Forest | Seen E7_DoorSignal, 9♥ — **не** проходить S19 |
| 12 | `HarveyOverhaulStory.E9_CameByHerself` | Hospital | Seen E8_BadDay, 10♥; нет активных injury/stress topics |
| 13 | `HarveyOverhaulStory.E10_HarveyWasWrong` | Town | Seen E9_CameByHerself, 11♥, **!dating** |
| 13b | `HarveyOverhaulStory.E10_HarveyWasWrong_Dating` | Town | То же + **dating/married** |
| 14 | `HarveyOverhaulStory.E11_HomeSafetyProtocol` | FarmHouse | After E10*, 12♥ → mail `mailHarveyHomeSafetyProtocol` |
| 15 | `HarveyOverhaulStory.E12_HarveyIsTired` | Hospital | After E11, 13♥, **!dating** |
| 15b | `HarveyOverhaulStory.E12_HarveyIsTired_Dating` | Hospital | After E11, 13♥, **dating** → `topicHarveyWasCaredFor` |
| 16 | `HarveyOverhaulRomance.E1_NotAnExamDate` | Beach | **Dating**, 14♥, after E12* — согласие на поцелуй/объятие/паузу |
| 17 | `HarveyOverhaulStory.E13_MinesAgreement` | BusStop | Dating/married **или** seen mine rescue; 12♥; утро/вечер → mail `mailHarveyMinesAgreement` |
| 18 | `HarveyOverhaulStory.E14_NotOnlyPatient` | Forest | **Dating**, 16♥, after Rom E1 + E13 → `topicHarveyNotOnlyPatient` |
| 19 | `HarveyOverhaulStory.E15_FuturePlan` | FarmHouse | **Dating !married**, 18♥, after E14 → mail `mailHarveyFuturePlanNote` |
| 19b | `HarveyOverhaulStory.E15_FuturePlan_Married` | FarmHouse | **Married**, 18♥, after E14 |

После E5: C# ставит `topicRescueOperation` (≥ **8♥** / 2000 FP) → `eventRescueOperation` (Woods, storm).

---

### S19 — Story arc: trust-fork (альтернатива E8–E9 main)

- [ ] **S19** Trust fork E7→E9 (не совмещать с E8_BadDay)

1. Пройти E1–E6 и **E7_DoorSignal**.
2. **Не** запускать `E8_BadDayNoReason`.
3. Дождаться снятия `HarveyMod_CD_E7` (7 д.) → sunny midday Town → `E7_TownSip_Sunny`.
4. **Sat** 10:00–16:00 → `E8_QuietShelf` → topics help fork.
5. Вечер Town → `E9_LightInWindow` → `topicHarveyTrustFinal`, mail `HarveyOverhaul_E9_LightNote`.
6. Проверить: main-ветка E10+ **недоступна** (нет `E9_CameByHerself`).

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
| `HarveyOverhaulRomance.E1_NotAnExamDate` | Beach | 14♥ | После E12*; согласие на поцелуй/объятие |
| `eventHarveyFirstDate` | Forest | 8♥ | Sunny evening, не winter |
| `eventHarveyMountainDate` | Mountain | 9♥ | Sunny morning |
| `eventHarveyPropose` | Beach | 10♥ | Sunny evening |

> `HarveyOverhaulRomance.E1` — часть story-арки (S13); vanilla dates — отдельная цепочка 8–10♥.

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
| [ ] | `eventRescueOperation` | Woods | После E5, topic + storm, C# ≥8♥ | [ ] | [ ] | [ ] | [ ] | |

---

### B. Лечение и госпиталь

| ☐ | Event ID | Локация | Как запустить | Cutscene | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:--------:|:-----:|:------:|:------:|---------|
| [ ] | `HarveyMod_FirstTreatment` | Hospital | S01 | [ ] | [ ] | [ ] | [ ] | |
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

### F. Story arc E1–E15 (+ trust-fork)

**Main path** (E8_BadDay → E9_CameByHerself → E10+). **Trust fork:** S19 (E7_TownSip → E8_QuietShelf → E9_LightInWindow).

| ☐ | Event ID | Локация | Cutscene | Карта | Topics / mail | Заметки |
|:-:|----------|---------|:--------:|:-----:|---------------|---------|
| [ ] | `HarveyOverhaulStory.E1_SlipperyPath` | BusStop | [ ] | [ ] | [ ] | Wind, 2♥, «Вы» |
| [ ] | `HarveyOverhaulStory.E2_InsistentExam` | Hospital | [ ] | [ ] | [ ] | 3♥; ⚠️ coords (audit P0) |
| [ ] | `HarveyOverhaulStory.E2B_QuietAgreement` | Town | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E3_ForestApothecary` | Forest | [ ] | [ ] | [ ] | Thu–Sat |
| [ ] | `HarveyOverhaulStory.E3B_WingPatient` | Forest | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E4_PierBreath` | Beach | [ ] | [ ] | [ ] | ⚠️ overlap (39,13) |
| [ ] | `HarveyOverhaulStory.E4B_TooQuiet` | Mountain | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E5_StormBeside` | Hospital | [ ] | [ ] | [ ] | **Storm** → rescue topic |
| [ ] | `HarveyOverhaulStory.E6_SayItOutLoud` | Hospital | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E7_DoorSignal` | Farm | [ ] | [ ] | [ ] | **NEW** вечер у порога |
| [ ] | `HarveyOverhaulStory.E8_BadDayNoReason` | Forest | [ ] | [ ] | [ ] | **Main** E8; mutex с QuietShelf |
| [ ] | `HarveyOverhaulStory.E9_CameByHerself` | Hospital | [ ] | [ ] | [ ] | **Main** E9; trust exam forks |
| [ ] | `HarveyOverhaulStory.E10_HarveyWasWrong` | Town | [ ] | [ ] | [ ] | !dating |
| [ ] | `HarveyOverhaulStory.E10_HarveyWasWrong_Dating` | Town | [ ] | [ ] | [ ] | dating split |
| [ ] | `HarveyOverhaulStory.E11_HomeSafetyProtocol` | FarmHouse | [ ] | [ ] | [ ] | mail safety kit |
| [ ] | `HarveyOverhaulStory.E12_HarveyIsTired` | Hospital | [ ] | [ ] | [ ] | !dating |
| [ ] | `HarveyOverhaulStory.E12_HarveyIsTired_Dating` | Hospital | [ ] | [ ] | [ ] | dating split |
| [ ] | `HarveyOverhaulStory.E13_MinesAgreement` | BusStop | [ ] | [ ] | [ ] | mine rescue OR dating |
| [ ] | `HarveyOverhaulStory.E14_NotOnlyPatient` | Forest | [ ] | [ ] | [ ] | dating only |
| [ ] | `HarveyOverhaulStory.E15_FuturePlan` | FarmHouse | [ ] | [ ] | [ ] | dating !married |
| [ ] | `HarveyOverhaulStory.E15_FuturePlan_Married` | FarmHouse | [ ] | [ ] | [ ] | married split |
| [ ] | `HarveyOverhaulStory.E7_TownSip_Sunny` | Town | [ ] | [ ] | [ ] | **Trust fork**; coords fix (29,22) |
| [ ] | `HarveyOverhaulStory.E8_QuietShelf` | ArchaeologyHouse | [ ] | [ ] | [ ] | **Trust fork**; Sat; Gunther (11,9) |
| [ ] | `HarveyOverhaulStory.E9_LightInWindow` | Town | [ ] | [ ] | [ ] | **Trust fork**; trust final |

- [ ] **S13** Story main path (E1→E15 без пропусков)
- [ ] **S19** Trust fork (отдельный слот)

---

### G. Romance и комната Харви

| ☐ | Event ID | Локация | Cutscene | Карта | Заметки |
|:-:|----------|---------|:--------:|:-----:|---------|
| [ ] | `HarveyOverhaulRomance.E1_NotAnExamDate` | Beach | [ ] | [ ] | Story romance; dating, 14♥ |
| [ ] | `eventHarveyFirstDate` | Forest | [ ] | [ ] | Dating, 8♥ |
| [ ] | `eventHarveyMountainDate` | Mountain | [ ] | [ ] | Dating, 9♥ |
| [ ] | `eventHarveyPropose` | Beach | [ ] | [ ] | Dating, 10♥ |
| [ ] | `eventHarveyRoomCheckup` | HarveyRoom | [ ] | [ ] | 6♥ |
| [ ] | `eventHarveyRoomCheckup2` | HarveyRoom | [ ] | [ ] | Dating + BETAS |

- [ ] **S15** Romance (vanilla dates + Rom E1 в S13)

---

### H. Debug / чужие модули / не подключено

| ☐ | Event ID | Статус | Заметки |
|:-:|----------|--------|---------|
| [ ] | `HarveyMod_TreatmentPlanMeeting` | **HarveyOverhaulStress** | CP в `events.json`, но script — **стресс/тревожность** (`topicTreatmentAgreement`…). C# Injury ошибочно ставит `topicDiagnosisComplete` → не тестировать в Injury-слоте |
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

- [ ] S01–S03: debuff → treat → **клик-финал** (`phase_recovery` или `topic*Cured`)
- [ ] S04–S05: mine rescue major + minor
- [ ] S06–S08: pass-out цепочки
- [ ] S09–S10: dirty / wet → infection mail
- [ ] S11: Severe mine forbidden
- [ ] S12: storm comfort (хотя бы 2 локации)
- [ ] S13: story main E1→E15 (или spot-check E7_DoorSignal, E10, E14, E15)
- [ ] S19: trust-fork E7→E9 (отдельный слот, mutex с E8_BadDay)
- [ ] S14: onboarding на чистом слоте
- [ ] S15: romance (vanilla 8–10♥ + `HarveyOverhaulRomance.E1` в dating-слоте)
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
| [`../CheckEvent/story-arc-map-audit.md`](../CheckEvent/story-arc-map-audit.md) | Story arc E1–E15, coords, changelog |
| [`../audit-relationship-tone.md`](../audit-relationship-tone.md) | Тон «Вы»/«ты», dating-split |
| [`../CheckEvent/cp-event-review-checklist.md`](../CheckEvent/cp-event-review-checklist.md) | Чеклист правки одного события |
