# Чеклист: модель одной основной травмы (MainInjury + Complications)

**Дата:** 2026-05-25 (обновлено после правок WetBandage / DirtyWound / Neglect / infection escalation)  
**Мод:** C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`  
**Цель:** проверить, что в сохранении всегда не более одной **основной** травмы (`MainInjuryId`), осложнения живут отдельно, эскалация инфекции и госпитализация согласованы с main.

> **Whitelist (C# `InjurySets`):**  
> - `DirtyInMines` → DirtyWound в шахте: `buffDeepCuts`, `buffBurnWounds`, `buffShrapnelWounds` (проверка через `MainInjuryId` + `HasInjuryOrPhase`, не только base-buff)  
> - `WetBandageSensitive` → WetBandage от воды **только после** `TreatmentStarted` и реальной повязки: `buffDeepCuts`, `buffBurnWounds`, `buffShrapnelWounds`, `buffInfectedWound`, `buffSurgicalWound` (**не** `buffFracturedBone` — там WetCast, не WetBandage)  
> - `NeglectStrikesByInjury` — счётчик заброшенности **по buffId**, не глобальный; сброс при смене main, `StartTreatment`, `CompleteRecovery`, эскалации в инфекцию

> Отмечайте `- [ ]` → `- [x]` по мере проверки.

---

## Журнал прогона

| Поле | Значение |
|------|----------|
| Тестер | |
| Слот сохранения | |
| Версия C# мода | |
| Дата | |

**Сводка MainInjury-сценариев:**

| # | Сценарий | Статус | Заметки |
|---|----------|:------:|---------|
| 1 | Базовое наложение main | [ ] | |
| 2 | Блокировка второй main | [ ] | |
| 3 | Upgrade лёгкой → тяжёлой | [ ] | |
| 3b | Upgrade заблокирован в лечении | [ ] | |
| 4 | DirtyWound (осложнение) | [ ] | |
| 4b | DirtyWound в шахте при фазовом лечении | [ ] | |
| 4c | DirtyWound: main не из DirtyInMines (негатив) | [ ] | |
| 5 | DirtyWound → InfectedWound | [ ] | |
| 5b | WetBandage: untreated infected (негатив) | [ ] | |
| 5c | WetBandage: после лечения infected (позитив) | [ ] | |
| 5d | WetBandage: перелом в лечении (негатив) | [ ] | |
| 6 | Фазовое лечение у Харви | [ ] | |
| 7 | Полное выздоровление | [ ] | |
| 8a | Severe по MainInjury | [ ] | |
| 8b | PainFlare ≠ severe | [ ] | |
| 9 | Save / load | [ ] | |
| 10 | Миграция старого сейва | [ ] | |
| 11 | NeglectStrikesByInjury (не переносится между main) | [ ] | |

---

## Подготовка

### Окружение

- [ ] SMAPI загружен, консоль открыта (`\`)
- [ ] C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]` установлены
- [ ] В `config.json`: `SendLetters: true` (если проверяете mail)
- [ ] Перед **каждым** изолированным сценарием — чистый старт (см. ниже)

### Команды MainInjury и диагностика

| Команда | Назначение |
|---------|------------|
| `injury_reset` | Полный сброс: баффы, state, topics `topic*` / `situation*` |
| `injury_phase_list` | `MainInjuryId`, valid, фазы, осложнения |
| `injury_debuff_list` | Все ID травм и осложнений + фазы |
| `injury_debuff_add <id> [минуты]` | Наложить травму/осложнение (учитывает MainInjury) |
| `injury_debuff_add --force <id> [минуты]` | Принудительная замена текущей main |
| `injury_main_set <buffId>` | **[DEBUG]** Установить `MainInjuryId` (нужен `DebuffState`) |
| `injury_main_clear` | **[DEBUG]** Очистить `MainInjuryId` без удаления баффов |
| `injury_debug_dump` | Полный дамп state в SMAPI-лог |
| `injury_cleanup_invalid_complications` | Удалить stale/невалидные осложнения (WetBandage без лечения, main ∉ WetBandageSensitive, orphan DebuffState/SavedActiveBuffs) |
| `injury_rain_debug` | Счётчики дождя / промокания повязки |
| `injury_phase_ready <buffId> [1\|0]` | «Фаза истекла, можно сменить» |
| `injury_phase_recovery <buffId> [1\|0]` | «Можно завершить лечение» |
| `injury_phase_advance <buffId>` | Принудительная смена фазы (без диалога) |
| `injury_phase_cure <buffId>` | Полное выздоровление без клика по Харви |
| `injury_mine_dirty_debug` | Read-only: риск грязной раны в шахте |
| `injury_debug_mine_rescue` | Флаги mine rescue → сработает на `DayStarted` |
| `injury_foreign_topic_add <topicId> [days]` | Поставить topic для теста |

**Debug HUD:** **F10** (compact → full) — `Main injury`, `valid`, фазы, `Complications`, `SavedActiveBuffs count`, `Main injury serious`.

**SMAPI warp (не мод):**

```
warp Mine 17 7
warp Hospital 20 5
```

### Что смотреть в `injury_phase_list`

- [ ] Строка `MainInjuryId: ...` (или `(none)`)
- [ ] `Active main injury valid: yes/no`
- [ ] `Complications: ...`
- [ ] Строки активных травм с фазой / `TreatmentStarted`
- [ ] В `injury_debug_dump`: `NeglectStrikesByInjury: buffId=N` (или `(none)`)

### Что смотреть в debug HUD (F10)

- [ ] `Main injury` / `valid`
- [ ] `Main injury phase`, `treatment started`
- [ ] `ReadyForNextPhase` / `ReadyForRecovery`
- [ ] `Complications`
- [ ] `SavedActiveBuffs count`
- [ ] `Main injury serious: yes/no` (для сценариев 8a/8b)

### Полезные лог-префиксы (SMAPI)

| Префикс | Когда смотреть |
|---------|----------------|
| `[MainInjury]` | Установка, замена, блокировка, миграция, завершение |
| `[Complication] MainInjury=..., complication=...` | Осложнения и эскалация |
| `[Complication] Cleared wound-related complications after infection: ...` | Очистка WetBandage/DirtyWound/WetStitches/Neglect после инфекции |
| `[Complication] Infection escalation finalized (...)` | Финализация эскалации (в т.ч. если main уже `buffInfectedWound`) |
| `[WetBandage] skip: <reason>, main=..., treatmentStarted=...` | Дождь/вода: WetBandage не применён (`treatment not started`, `main not WetBandageSensitive`, `no active bandage/treatment`, …) |
| `[WetBandage] allowed: main=..., treatmentStarted=...` | WetBandage разрешён (есть лечение + whitelist + повязка) |
| `[DirtyWound] allowed/skip: ..., main=...` | Eligibility грязной раны в шахте (`DirtyInMines` + `HasInjuryOrPhase`) |
| `[Neglect] Сброс счётчика при смене MainInjuryId: ...` | NeglectStrikesByInjury не переносится между main |
| `[Neglect] Сброс NeglectStrikesByInjury для ...` | StartTreatment / CompleteRecovery / infection |
| `[BuffRestore] skip invalid complication buff: ...` | DayStarted: snapshot не восстановил stale buff |
| `[ComplicationCleanup] removing ...` | `injury_cleanup_invalid_complications` |
| `[MineRescue]` | Major/minor rescue, warp |
| `[PassOutEvent]` | Emergency / exhaustion |

---

## 1. Базовое наложение основной травмы

- [ ] **Сценарий 1 пройден**

### Подготовка

```
injury_reset
```

- [ ] `injury_phase_list`: `MainInjuryId: (none)`
- [ ] F10: нет активных травм

### Шаги

```
injury_debuff_add buffFracturedBone
injury_phase_list
```

- [ ] Команда выполнилась без ошибки в SMAPI

### Ожидается

- [ ] `MainInjuryId = buffFracturedBone` (`injury_phase_list`)
- [ ] `Active main injury valid: yes`
- [ ] В `ActiveDebuffs` есть `DebuffState` для `buffFracturedBone`
- [ ] До начала лечения активен **базовый** бафф `buffFracturedBone` (не фазовый `HarveyMod_*`)
- [ ] `injury_phase_list` показывает строку `buffFracturedBone` с фазой/флагами
- [ ] Debug HUD (F10): `Main injury: buffFracturedBone`, `valid: yes`
- [ ] SMAPI-лог: `[MainInjury] Установлена основная травма: buffFracturedBone` (или эквивалент apply)

### Дополнительная проверка (опционально)

```
injury_debug_dump
```

- [ ] В дампе блок MainInjury согласован с `injury_phase_list`

---

## 2. Блокировка второй основной травмы

- [ ] **Сценарий 2 пройден**

### Подготовка

Сценарий 1 выполнен, активен `buffFracturedBone`:

```
injury_phase_list
```

- [ ] `MainInjuryId: buffFracturedBone`
- [ ] `Active main injury valid: yes`

### Шаги

```
injury_debuff_add buffDeepCuts
injury_phase_list
```

- [ ] Команда **без** `--force`

### Ожидается

- [ ] `buffDeepCuts` **не** становится второй основной травмой
- [ ] `MainInjuryId` остаётся `buffFracturedBone`
- [ ] В SMAPI-логе блокировка, например: `[MainInjury] Новая травма заблокирована, уже есть основная: buffFracturedBone, попытка: buffDeepCuts`
- [ ] Консоль сообщает об отказе или main не меняется
- [ ] `injury_debuff_list`: `buffDeepCuts` отсутствует (или не добавлен как main)

### Дополнительно (опционально): `--force`

```
injury_debuff_add --force buffDeepCuts
injury_phase_list
```

- [ ] `MainInjuryId = buffDeepCuts`
- [ ] Старый перелом снят (нет `buffFracturedBone` как активной main)
- [ ] В логе: замена main `buffFracturedBone -> buffDeepCuts`
- [ ] Если был `NeglectStrikesByInjury` для перелома — сброшен при смене main

---

## 3. Upgrade лёгкой травмы в тяжёлую

- [ ] **Сценарий 3 пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffHurt
injury_phase_list
```

- [ ] `MainInjuryId: buffHurt`
- [ ] `Active main injury valid: yes`

### Шаги

```
injury_debuff_add buffBadlyHurt
injury_phase_list
injury_debuff_list
```

### Ожидается

- [ ] `buffHurt` заменён на `buffBadlyHurt` (старый бафф/состояние сняты)
- [ ] `MainInjuryId = buffBadlyHurt`
- [ ] В логе: `[MainInjury] Основная травма заменена: buffHurt -> buffBadlyHurt`
- [ ] Одновременно **не** активны `buffHurt` и `buffBadlyHurt` как две основные
- [ ] `buffBadlyHurt` **не** в `ActiveComplications`
- [ ] F10: `Main injury: buffBadlyHurt`
- [ ] Topics: `topicBadlyHurt`, `topicHealthDamageCritical` (нет `topicHurt`)

### 3b. Upgrade заблокирован после начала лечения

- [ ] **Сценарий 3b пройден**

#### Подготовка

```
injury_reset
injury_debuff_add buffHurt
# Начать лечение: клик по Харви (появится buffHarveyTreatment, TreatmentStarted=true)
injury_phase_list
```

- [ ] `MainInjuryId: buffHurt`, `TreatmentStarted: true`

#### Шаги

```
injury_debuff_add buffBadlyHurt
injury_phase_list
```

#### Ожидается

- [ ] `MainInjuryId` остаётся `buffHurt`
- [ ] В логе: `[MainInjury] Upgrade заблокирован: buffHurt уже в лечении, попытка buffBadlyHurt`
- [ ] `buffBadlyHurt` не становится main без `--force`

---

## 4. Осложнение DirtyWound при открытой ране

- [ ] **Сценарий 4 пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffDeepCuts
injury_phase_list
```

- [ ] `MainInjuryId: buffDeepCuts`
- [ ] Травма **не** в фазе лечения (открытая рана; base-buff `buffDeepCuts` на игроке)
- [ ] `injury_mine_dirty_debug`: `hasDirtyInjury=true` (не только `HasBuff(buffDeepCuts)` — eligibility через main + phase)

### Шаги (вариант A — шахта)

```
injury_mine_dirty_debug
warp Mine 17 7
```

- [ ] Провести в шахте 60+ игровых минут **или** дождаться roll exposure
- [ ] В логе (периодически): `[DirtyWound] allowed: main=buffDeepCuts, reason=open or treated wound surface`

### Шаги (вариант B — debug exposure)

```
injury_mine_dirty_debug
```

- [ ] `hasDirtyInjury=true`, `hasDirtyWound=false` до roll
- [ ] Смотреть счётчики exposure в F10 / дампе

### Шаги (вариант C — severe main + NoMine violation)

При severe main и нарушении запрета шахты — по ситуации из [`../mines-forbidden-injuries.md`](../mines-forbidden-injuries.md).

### Ожидается

- [ ] `MainInjuryId` остаётся `buffDeepCuts`
- [ ] Появляется осложнение `HarveyMod_DirtyWound` (бафф + topic)
- [ ] `ActiveComplications` / F10 `Complications` содержат `HarveyMod_DirtyWound`
- [ ] В логе: `[Complication] MainInjury=buffDeepCuts, complication=HarveyMod_DirtyWound`
- [ ] `buffDeepCuts` и `DirtyWound` сосуществуют: main + complication, **не** две main
- [ ] `injury_phase_list`: `Complications: HarveyMod_DirtyWound` (или список с ним)

---

## 4b. DirtyWound в шахте при фазовом лечении (base-buff снят)

- [ ] **Сценарий 4b пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffDeepCuts
# Клик по Харви → StartTreatment (фаза 1, base buffDeepCuts снят)
injury_phase_list
```

- [ ] `MainInjuryId: buffDeepCuts`, `TreatmentStarted: true`
- [ ] На игроке **фазовый** бафф (`HarveyMod_DeepCuts_Acute` и т.п.), **нет** `buffDeepCuts`
- [ ] `HasInjuryOrPhase(buffDeepCuts)` = true (regression: раньше шахта могла не срабатывать)

### Шаги

```
injury_mine_dirty_debug
warp Mine 17 7
# 60+ игровых минут в шахте
injury_phase_list
```

### Ожидается

- [ ] `[DirtyWound] allowed: main=buffDeepCuts, ...` (не `skip: no base buff or phase for main`)
- [ ] DirtyWound **может** появиться (roll), main остаётся `buffDeepCuts`
- [ ] `injury_mine_dirty_debug`: `hasDirtyInjury=true` при активной фазе

---

## 4c. DirtyWound: main не из DirtyInMines (негатив)

- [ ] **Сценарий 4c пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffFracturedBone
injury_phase_list
```

### Шаги

```
injury_mine_dirty_debug
warp Mine 17 7
# 60+ игровых минут
injury_phase_list
```

### Ожидается

- [ ] `hasDirtyInjury=false` в `injury_mine_dirty_debug`
- [ ] В логе: `[DirtyWound] skip: main not in DirtyInMines, main=buffFracturedBone`
- [ ] `HarveyMod_DirtyWound` **не** появляется от mine exposure (PainFlare от другой травмы — отдельная механика)

---

## 5. Эскалация DirtyWound → InfectedWound

- [ ] **Сценарий 5 пройден**

### Подготовка

Сценарий 4 выполнен:

```
injury_phase_list
```

- [ ] `MainInjuryId: buffDeepCuts`
- [ ] `HarveyMod_DirtyWound` в complications
- [ ] Topic `topicHarvey_DirtyWound` (Social / F10)

### Шаги

Прожить дни **без** лечения осложнения / дождаться daily-check инфекции:

- День 1: 15% roll
- День 2: 40%
- День 3+: 100%

```
# Ускорение: лечь спать несколько ночей подряд
# После каждого утра:
injury_phase_list
```

- [ ] При необходимости: `injury_debug_dump` после 3-го дня

После successful infection roll (утро, когда main уже заменена):

```
injury_phase_list
injury_debug_dump
```

Опционально — проверка, что дождь не «оживляет» WetBandage до лечения:

```
# Сухая погода → дождь (или дождливый день)
# Стоять на улице 60+ игровых минут под дождём
injury_phase_list
injury_debug_dump
```

### Ожидается (сразу после successful infection roll)

- [ ] `MainInjuryId = buffInfectedWound`
- [ ] `buffDeepCuts` **отсутствует** (бафф, `ActiveDebuffs`, topics deep cuts)
- [ ] `HarveyMod_DirtyWound` **отсутствует** (бафф, `ActiveComplications`, `ActiveDebuffs`, topic `topicHarvey_DirtyWound`)
- [ ] `HarveyMod_WetBandage` **отсутствует**, если лечение `buffInfectedWound` ещё **не** начато
- [ ] `ActiveComplications`: **none** или нет wound-related complications (`DirtyWound`, `WetBandage`, `Neglect`, `WetStitches`)
- [ ] Active mod buffs (F10 / `injury_debug_dump`): только `buffInfectedWound`, **без** `HarveyMod_WetBandage` / `HarveyMod_Neglect`
- [ ] `TreatmentStarted = false` для `buffInfectedWound`
- [ ] Topic `topicInfectedWound` есть; `topicHarvey_DirtyWound` / `topicHarvey_WetBandage` / `topicHarvey_Neglect` **отсутствуют**
- [ ] В логе (при очистке): `[Complication] Cleared wound-related complications after infection: HarveyMod_DirtyWound, ...` (если были wound-complications)
- [ ] В логе: `[Complication] Infection escalation finalized (day ..., source=HarveyMod_DirtyWound, alreadyInfected=false)`
- [ ] `NeglectStrikesByInjury` сброшен (в дампе `(none)` или без записи для старой main)
- [ ] HUD: «Рана инфицирована! Срочно к врачу!» (или эквивалент)
- [ ] Mail `HarveyMod_DirtyWoundInfection` (если `SendLetters: true`)
- [ ] В логе: `[MainInjury] Основная травма заменена: buffDeepCuts -> buffInfectedWound`
- [ ] При fallback (если замена не удалась): **нет** второй main, есть `PainFlare` + срочный topic/mail

### Ожидается (main уже buffInfectedWound + новое wound-complication)

Если у игрока уже `buffInfectedWound`, а daily-check эскалирует **оставшийся** DirtyWound/WetBandage:

- [ ] `MainInjuryId` остаётся `buffInfectedWound`
- [ ] `FinalizeInfectionEscalation` всё равно вызывается → wound-complications очищены из баффов, `ActiveComplications`, `ActiveDebuffs`, `SavedActiveBuffs`, topics
- [ ] В логе: `alreadyInfected=true`, `[Complication] MainInjury=buffInfectedWound, complication=... cleared (already infected)`

### Ожидается (лечение и дождь после эскалации)

- [ ] **Клик по Харви** начинает лечение `buffInfectedWound` (`TreatmentStarted: true`, фаза 1)
- [ ] **До** `TreatmentStarted=true`: стоять под дождём **не** добавляет `HarveyMod_WetBandage`
- [ ] **Только после** `TreatmentStarted=true` и при активной повязке/фазе WetBandage **может** появляться от дождя (см. сценарии **5b** / **5c**)
- [ ] После сна (`DayStarted`): stale `HarveyMod_WetBandage` **не** восстанавливается из `SavedActiveBuffs`, если лечение не начато (лог `[BuffRestore] skip invalid complication buff: HarveyMod_WetBandage, ...`)

---

## 5b. WetBandage при untreated buffInfectedWound (негатив)

- [ ] **Сценарий 5b пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffInfectedWound
injury_phase_list
```

- [ ] `MainInjuryId: buffInfectedWound`
- [ ] `TreatmentStarted: false`
- [ ] `Complications: (none)`

### Шаги

1. Дождливый день (или дождь через консоль/погоду)
2. Стоять **на улице** под дождём **60+ игровых минут** (1 сек real ≈ 1 сек счётчика дождя)
3. При необходимости:

```
injury_rain_debug
injury_phase_list
injury_debug_dump
```

4. Если в сейве остался stale WetBandage от старых прогонов:

```
injury_cleanup_invalid_complications
injury_phase_list
```

### Ожидается

- [ ] `HarveyMod_WetBandage` **не появляется** — нет лечения/повязки у Харви
- [ ] `ActiveComplications: (none)` (или без wound-related)
- [ ] Active mod buffs: только `buffInfectedWound`
- [ ] Topic `topicHarvey_WetBandage` **отсутствует**
- [ ] В логе (периодически): `[WetBandage] skip: treatment not started, main=buffInfectedWound, treatmentStarted=False`  
  (или `no active bandage/treatment` — оба варианта OK до StartTreatment)

---

## 5c. WetBandage после начала лечения buffInfectedWound (позитив)

- [ ] **Сценарий 5c пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffInfectedWound
injury_phase_list
```

- [ ] Hospital 9:00–21:00, Харви на месте
- [ ] `TreatmentStarted: false`

### Шаги

1. **Клик по Харви** → начать лечение

```
injury_phase_list
injury_medical_snapshot
```

- [ ] `TreatmentStarted: true` для `buffInfectedWound`
- [ ] Активен cure-бaff и/или фазовый бафф (`HarveyMod_InfectedWound_Acute` и т.п.)

2. Дождливый день → стоять на улице под дождём **60+ игровых минут**

```
injury_phase_list
injury_debug_dump
```

### Ожидается

- [ ] **До** шага 1 (клик Харви): WetBandage от дождя **не** появляется (как в **5b**)
- [ ] **После** `TreatmentStarted=true`: WetBandage **может** появиться от дождя (HUD «Повязка промокла!», `HarveyMod_WetBandage` в complications) — `buffInfectedWound` ∈ `WetBandageSensitive`
- [ ] `MainInjuryId` остаётся `buffInfectedWound`
- [ ] В логе при успехе: `[WetBandage] allowed: main=buffInfectedWound, treatmentStarted=True` → `[Complication] MainInjury=buffInfectedWound, complication=HarveyMod_WetBandage`

---

## 5d. WetBandage: перелом в лечении (негатив, WetCast)

- [ ] **Сценарий 5d пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffFracturedBone
# Клик по Харви → StartTreatment (фаза гипса / HarveyMod_FracturedBone_*)
injury_phase_list
```

- [ ] `MainInjuryId: buffFracturedBone`, `TreatmentStarted: true`
- [ ] Активен фазовый бафф перелома (не «повязка» из `WetBandageSensitive`)

### Шаги

1. Дождливый день → стоять на улице под дождём **60+ игровых минут**

```
injury_rain_debug
injury_phase_list
injury_debug_dump
```

### Ожидается

- [ ] `HarveyMod_WetBandage` **не появляется** — `buffFracturedBone` **не** в `WetBandageSensitive`
- [ ] В логе: `[WetBandage] skip: main not WetBandageSensitive, main=buffFracturedBone, treatmentStarted=True`
- [ ] Stale WetBandage из старого сейва удаляется через `injury_cleanup_invalid_complications` (reason: not WetBandageSensitive)

---

## 6. Фазовое лечение у Харви

- [ ] **Сценарий 6 пройден**

### Подготовка (вариант: перелом)

```
injury_reset
injury_debuff_add buffFracturedBone
injury_phase_list
```

Альтернатива: `buffInfectedWound` — та же логика main id.

- [ ] Hospital 9:00–21:00, Харви на месте

### Шаги

1. **Клик по Харви** → начать лечение

```
injury_phase_list
```

- [ ] `TreatmentStarted: true` для `buffFracturedBone`
- [ ] F10 `LastClickDebug` — ветка начала лечения

2. Дождаться готовности фазы **или**:

```
injury_phase_ready buffFracturedBone 1
```

3. **Клик по Харви** → переход фазы (диалог)

   Альтернатива без диалога:

```
injury_phase_advance buffFracturedBone
injury_phase_list
```

4. Повторить шаги 2–3 для каждой фазы (смотреть `injury_phase_list`)

### Ожидается

- [ ] `MainInjuryId` **не меняется** при смене фазы (остаётся `buffFracturedBone`)
- [ ] Базовый бафф снят, активен **фазовый** (`HarveyMod_*_Acute` / `_Treatment` и т.д.)
- [ ] `GetActiveInjury()` / `injury_phase_list` / F10 показывают **базовый** id (`buffFracturedBone`), не phase buff id
- [ ] `Active main injury valid: yes` на протяжении всего лечения
- [ ] Topics лечения: `topicTreatmentFracturedBone`, фазовые `topic*Phase*`
- [ ] `NeglectStrikesByInjury` для `buffFracturedBone` сброшен при StartTreatment (в дампе `(none)` или без записи для этой main)

---

## 7. Полное выздоровление

- [ ] **Сценарий 7 пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffDeepCuts
injury_debuff_add HarveyMod_PainFlare
injury_phase_list
```

- [ ] Main: `buffDeepCuts`
- [ ] Complication: `HarveyMod_PainFlare`

### Шаги (вариант A — быстрый cure)

```
injury_phase_cure buffDeepCuts
injury_phase_list
injury_debuff_list
```

### Шаги (вариант B — полный цикл с Харви)

См. [`FOR_TEST.md`](FOR_TEST.md) — «Завершение лечения»:

```
injury_phase_recovery buffDeepCuts 1
```

→ **клик по Харви** → финал recovery

### Ожидается

- [ ] `DebuffState` для вылеченной травмы удалён
- [ ] Базовый и фазовые баффы `buffDeepCuts` сняты
- [ ] `MainInjuryId` очищен (`(none)` в `injury_phase_list`)
- [ ] В логе: `[MainInjury] Основная травма завершена: buffDeepCuts` (или `CompleteMainInjury`)
- [ ] `NeglectStrikesByInjury` для `buffDeepCuts` сброшен (`[Neglect] Сброс NeglectStrikesByInjury для buffDeepCuts`)
- [ ] `HarveyMod_PainFlare` **не** удалён случайно при cure main
- [ ] F10 / `injury_phase_list`: `Complications: HarveyMod_PainFlare` (или в списке осложнений)
- [ ] `Active main injury valid: no` (или n/a при пустом main)

---

## 8. Госпитализация и severe по MainInjury

### 8a. Severe определяется по main

- [ ] **Сценарий 8a пройден**

#### Подготовка

```
injury_reset
injury_debuff_add buffFracturedBone
injury_foreign_topic_add topicMineInjuryRescue 7
injury_phase_list
```

Альтернатива topic:

```
injury_debug_mine_rescue
# → сон → утро (если тестируете полный rescue pipeline)
```

- [ ] `MainInjuryId: buffFracturedBone` (severe)
- [ ] Dating/married с Харви (для proximity hospitalization)

#### Шаги

```
warp Hospital 20 5
# Подойти к Харви при topicMineInjuryRescue и ForceHospitalization
injury_debug_dump
```

#### Ожидается

- [ ] Принудительная госпитализация срабатывает (severe main = `buffFracturedBone`)
- [ ] F10 / `injury_debug_dump`: `Main injury serious: yes`
- [ ] Решение **не** опирается на «любой severe buff на игроке» в обход main
- [ ] `IsMainInjurySerious()` = true только из-за main, не complication

---

### 8b. PainFlare не запускает госпитализацию

- [ ] **Сценарий 8b пройден**

#### Подготовка

```
injury_reset
injury_debuff_add buffHurt
injury_debuff_add HarveyMod_PainFlare
injury_phase_list
injury_debug_dump
```

- [ ] `MainInjuryId: buffHurt` (лёгкая)
- [ ] `HarveyMod_PainFlare` в complications

#### Шаги

- [ ] F10: `Main injury serious: no`
- [ ] Попытка mine-rescue / forced hospital **без** severe main:

```
injury_foreign_topic_add topicMineInjuryRescue 7
warp Hospital 20 5
```

#### Ожидается

- [ ] `MainInjuryId = buffHurt`
- [ ] `Main injury serious: no`
- [ ] `HarveyMod_PainFlare` в complications, но **не** делает состояние severe
- [ ] Принудительная госпитализация **не** стартует только из-за PainFlare

---

## 9. Сохранение и загрузка

- [ ] **Сценарий 9 пройден**

### Подготовка

```
injury_reset
injury_debuff_add buffDeepCuts
injury_debuff_add HarveyMod_DirtyWound
injury_phase_list
injury_debug_dump
```

- [ ] Записать `MainInjuryId`, complications, фазу

### Шаги

1. Сохранить игру (слот)
2. Выйти в title screen
3. Загрузить слот
4. Дождаться `DayStarted` (утро)

```
injury_phase_list
injury_debug_dump
```

### Ожидается

- [ ] После save/load `MainInjuryId` сохранён (`buffDeepCuts`)
- [ ] Утром баффы восстановлены (`DayStarted` buff restore)
- [ ] `GetActiveInjury()` / `injury_phase_list` — та же основная травма
- [ ] `Active main injury valid: yes`
- [ ] Осложнения сохранены в `ActiveComplications` / F10
- [ ] Нет дублирования main или «потери» main при restore
- [ ] Stale `HarveyMod_WetBandage` в `SavedActiveBuffs` **не** восстанавливается, если main ∉ `WetBandageSensitive` или лечение не начато (`[BuffRestore] skip invalid complication buff` / cleanup)

---

## 10. Миграция старого сейва

- [ ] **Сценарий 10 пройден**

### Симуляция

Сохранение, где `MainInjuryId` пустой, но в `ActiveDebuffs` несколько травм:

**Вариант A — старый сейв до поля main**

**Вариант B — debug:**

```
injury_reset
injury_debuff_add buffHurt
injury_debuff_add buffDeepCuts
injury_main_clear
injury_phase_list
```

- [ ] `MainInjuryId: (none)`
- [ ] В `ActiveDebuffs` несколько записей (или несколько баффов на игроке)

### Шаги

```
injury_phase_list
# или перезагрузить save / кликнуть по Харви — триггер MigrateMainInjuryId
injury_debug_dump
```

### Ожидается

- [ ] При пустом `MainInjuryId` и непустом `ActiveDebuffs` выбирается **самая приоритетная** травма (`MainInjuryPriorityOrder`)
- [ ] В логе: `[MainInjury] Миграция: основная травма = ...`
- [ ] `injury_phase_list`: `MainInjuryId` заполнен, `valid: yes`
- [ ] Осложнения из `KnownComplicationBuffIds` **не** становятся main
- [ ] Приоритет: тяжёлая травма (например `buffDeepCuts`) побеждает `buffHurt`
- [ ] При смене main в логе: `[Neglect] Сброс счётчика при смене MainInjuryId: buffHurt -> buffDeepCuts` (если были strikes у старой main)

---

## 11. NeglectStrikesByInjury не переносится между main

- [ ] **Сценарий 11 пройден**

### Подготовка A — смена main

```
injury_reset
injury_debuff_add buffHurt
# Несколько дней без лечения / или debug increment через prescription neglect (3+ нарушения NoMine)
injury_debug_dump
```

- [ ] В дампе: `NeglectStrikesByInjury: buffHurt=N` (N > 0)

### Шаги A

```
injury_debuff_add --force buffDeepCuts
injury_debug_dump
```

### Ожидается A

- [ ] `NeglectStrikesByInjury` для `buffHurt` **сброшен** (запись удалена)
- [ ] Счётчик для `buffDeepCuts` = 0 (не наследует N от `buffHurt`)
- [ ] Лог: `[Neglect] Сброс счётчика при смене MainInjuryId: buffHurt -> buffDeepCuts`

### Подготовка B — StartTreatment

```
injury_reset
injury_debuff_add buffDeepCuts
# Накопить strikes (untreated neglect / checkup overdue — по ситуации)
injury_debug_dump
```

### Шаги B

Клик по Харви → StartTreatment → `injury_debug_dump`

### Ожидается B

- [ ] `NeglectStrikesByInjury` для `buffDeepCuts` сброшен после StartTreatment

---

## Быстрая регрессия (минимум)

Если времени мало — пройти **1 → 2 → 3 → 4b → 5 → 5b → 5d → 7 → 8b → 11** по порядку.

- [ ] **Быстрая регрессия пройдена**

| # | Команды | Критичный результат | ✓ |
|---|---------|---------------------|:-:|
| 1 | `injury_reset` → `injury_debuff_add buffFracturedBone` | main установлена | [ ] |
| 2 | `injury_debuff_add buffDeepCuts` (без `--force`) | main не дублируется | [ ] |
| 3 | `injury_reset` → `buffHurt` → `buffBadlyHurt` | upgrade main | [ ] |
| 4b | `buffDeepCuts` + лечение + Mine 60m | DirtyWound eligible без base-buff | [ ] |
| 5 | DirtyWound + сон 3 дня | infection replaces main, wound-complications cleared | [ ] |
| 5b | `buffInfectedWound` + дождь без лечения | WetBandage не появляется | [ ] |
| 5d | `buffFracturedBone` + лечение + дождь | WetBandage skip: not WetBandageSensitive | [ ] |
| 7 | `injury_phase_cure buffDeepCuts` + PainFlare | main cleared, complication stays | [ ] |
| 8b | main=hurt + PainFlare | no forced hospital | [ ] |
| 11 | `--force` другая main | NeglectStrikes не переносится | [ ] |

---

## Полный реестр событий CP (чеклист)

**Источники:** `events.json`, `eventsCare.json`, `eventsMineRescue.json` (активный CP).  
**69 ID** в Injury-чеклисте; детальные сценарии запуска — в [`EVENTS_TEST_CHECKLIST.md`](EVENTS_TEST_CHECKLIST.md).

**Легенда:** ☐ Cutscene · ☐ Карта · ☐ Topics/mail · ☐ Повтор OK

---

### A. Шахта и экстренные (C# + CP)

| ☐ | Event ID | Локация | Как запустить | Cut | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:---:|:-----:|:------:|:------:|---------|
| [ ] | `eventHarveyMineRescueDating` | Mine | `injury_debug_mine_rescue` → сон; dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMineRescue` | Mine | Legacy fallback (!dating) | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMinorMineRescue` | Mine | `buffBackStrain` + Mine, HP≤35% / stamina≤15% | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMineInterception` | Mine | Вход с травмой, SpaceCore | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveySkullCavePrevention` | SkullCave | Выход из Skull, SpaceCore | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyEmergencyCare` | Hospital | Pass-out HP≤10 вне шахты, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyExhaustion` | Hospital | Pass-out stamina≤-15 вне шахты | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventRescueOperation` | Woods | После E5 / `topicRescueOperation` + storm | [ ] | [ ] | [ ] | [ ] | |

- [ ] **Блок A проверен**

---

### B. Лечение и госпиталь

| ☐ | Event ID | Локация | Как запустить | Cut | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:---:|:-----:|:------:|:------:|---------|
| [ ] | `HarveyMod_FirstTreatment` | Hospital | `buffHurt` + `topicHarveyNeedsFirstTreatment` | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_NightCrisis_Dating` | Hospital | После FirstTreatment, 22–26, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_NightCrisis_PreDating` | Hospital | После FirstTreatment, !dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_BirthdayHospital_Dating` | Hospital | 9 summer, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `HarveyMod_BirthdayHospital_Friend` | Hospital | 9 summer, !dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMedicalCheck` | Hospital | Mail reminder, !dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMedicalCheck_Dating` | Hospital | Mail reminder, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyTraumaExam` | Hospital | 8♥, Hospital днём | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyCheckup` | Hospital | `topicAgreedCheckup` | [ ] | [ ] | [ ] | [ ] | ⚠️ C# topic не ставит |
| [ ] | `eventHarveyTreatmentCollapse` | Hospital | Manual `debug event` | [ ] | [ ] | [ ] | [ ] | ❌ orphan |
| [ ] | `eventStayInHospital` | Hospital | Manual `debug event` | [ ] | [ ] | [ ] | [ ] | ❌ orphan |

- [ ] **Блок B проверен**

---

### C. Onboarding и визиты на ферму

| ☐ | Event ID | Локация | Как запустить | Cut | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:---:|:-----:|:------:|:------:|---------|
| [ ] | `eventHarveyFirstMeeting` | BusStop | Новый слот, BusStop | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyFirstVisit` | Farm | day ≥ 3 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveySecondVisit` | Farm | day ≥ 7 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyFirstWalk` | Farm | day ≥ 11, sunny | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyCheckHealthFarmer` | Farm | После `PlayerKilled`, dating | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyCheckFarmerOutsideAfter22` | Farm | `topicPassedOutInTown`, 22–02 | [ ] | [ ] | [ ] | [ ] | |
| [ ] | `eventHarveyMorningCheckup` | Farm | 6–8, `topicHarveyMandatoryCheckup` | [ ] | [ ] | [ ] | [ ] | |

- [ ] **Блок C проверен**

---

### D. Pass-out и поздняя ночь

| ☐ | Event ID | Локация | Как запустить | Cut | Карта | Topics | Повтор | Заметки |
|:-:|----------|---------|---------------|:---:|:-----:|:------:|:------:|---------|
| [ ] | `eventHarveyLateNightCollapse` | Town | Town 24:00–26:00 | [ ] | [ ] | [ ] | [ ] | |

- [ ] **Блок D проверен**

---

### E. Storm comfort (6 локаций)

После C# roll (гроза 12–22, 750 FP, 35%) — войти в локацию.

| ☐ | Event ID | Локация | Cut | Карта | Topics | Заметки |
|:-:|----------|---------|:---:|:-----:|:------:|---------|
| [ ] | `eventHarveyStormComfortFarm` | Farm | [ ] | [ ] | [ ] | weight 0.6 |
| [ ] | `eventHarveyStormComfortForest` | Forest | [ ] | [ ] | [ ] | weight 0.55 |
| [ ] | `eventHarveyStormComfortTown` | Town | [ ] | [ ] | [ ] | weight 0.3 |
| [ ] | `eventHarveyStormComfortMine` | Mine | [ ] | [ ] | [ ] | weight 0.8 |
| [ ] | `eventHarveyStormComfortMountain` | Mountain / SVE Summit | [ ] | [ ] | [ ] | weight 0.4 |
| [ ] | `eventHarveyStormComfortDesert` | Desert | [ ] | [ ] | [ ] | weight 0.3 |

- [ ] **Блок E проверен**

---

### F. Story arc E1–E15 (+ trust-fork)

**Main path:** E7_DoorSignal → E8_BadDay → E9_CameByHerself → E10+.  
**Trust fork:** E7_TownSip → E8_QuietShelf → E9_LightInWindow (отдельный слот).

| ☐ | Event ID | Локация | Cut | Карта | Topics/mail | Заметки |
|:-:|----------|---------|:---:|:-----:|-------------|---------|
| [ ] | `HarveyOverhaulStory.E1_SlipperyPath` | BusStop | [ ] | [ ] | [ ] | Wind, 2♥, «Вы» |
| [ ] | `HarveyOverhaulStory.E2_InsistentExam` | Hospital | [ ] | [ ] | [ ] | 3♥, «ты» |
| [ ] | `HarveyOverhaulStory.E2B_QuietAgreement` | Town | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E3_ForestApothecary` | Forest | [ ] | [ ] | [ ] | Thu–Sat |
| [ ] | `HarveyOverhaulStory.E3B_WingPatient` | Forest | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E4_PierBreath` | Beach | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E4B_TooQuiet` | Mountain | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E5_StormBeside` | Hospital | [ ] | [ ] | [ ] | Storm → rescue topic |
| [ ] | `HarveyOverhaulStory.E6_SayItOutLoud` | Hospital | [ ] | [ ] | [ ] | |
| [ ] | `HarveyOverhaulStory.E7_DoorSignal` | Farm | [ ] | [ ] | [ ] | Main bridge |
| [ ] | `HarveyOverhaulStory.E8_BadDayNoReason` | Forest | [ ] | [ ] | [ ] | Main E8 |
| [ ] | `HarveyOverhaulStory.E9_CameByHerself` | Hospital | [ ] | [ ] | [ ] | Main E9 |
| [ ] | `HarveyOverhaulStory.E10_HarveyWasWrong` | Town | [ ] | [ ] | [ ] | !dating |
| [ ] | `HarveyOverhaulStory.E10_HarveyWasWrong_Dating` | Town | [ ] | [ ] | [ ] | dating |
| [ ] | `HarveyOverhaulStory.E11_HomeSafetyProtocol` | FarmHouse | [ ] | [ ] | [ ] | mail safety |
| [ ] | `HarveyOverhaulStory.E12_HarveyIsTired` | Hospital | [ ] | [ ] | [ ] | !dating |
| [ ] | `HarveyOverhaulStory.E12_HarveyIsTired_Dating` | Hospital | [ ] | [ ] | [ ] | dating |
| [ ] | `HarveyOverhaulStory.E13_MinesAgreement` | BusStop | [ ] | [ ] | [ ] | mine rescue OR dating |
| [ ] | `HarveyOverhaulStory.E14_NotOnlyPatient` | Forest | [ ] | [ ] | [ ] | dating |
| [ ] | `HarveyOverhaulStory.E15_FuturePlan` | FarmHouse | [ ] | [ ] | [ ] | dating !married |
| [ ] | `HarveyOverhaulStory.E15_FuturePlan_Married` | FarmHouse | [ ] | [ ] | [ ] | married |
| [ ] | `HarveyOverhaulStory.E7_TownSip_Sunny` | Town | [ ] | [ ] | [ ] | **Trust fork** |
| [ ] | `HarveyOverhaulStory.E8_QuietShelf` | ArchaeologyHouse | [ ] | [ ] | [ ] | **Trust fork**, Sat |
| [ ] | `HarveyOverhaulStory.E9_LightInWindow` | Town | [ ] | [ ] | [ ] | **Trust fork** |

- [ ] **Блок F (main path) проверен**
- [ ] **Блок F (trust fork) проверен** (отдельный слот)

---

### G. Romance и комната Харви

| ☐ | Event ID | Локация | Cut | Карта | Заметки |
|:-:|----------|---------|:---:|:-----:|---------|
| [ ] | `HarveyOverhaulRomance.E1_NotAnExamDate` | Beach | [ ] | [ ] | Story, 14♥, dating |
| [ ] | `eventHarveyFirstDate` | Forest | [ ] | [ ] | 8♥, sunny evening |
| [ ] | `eventHarveyMountainDate` | Mountain | [ ] | [ ] | 9♥, morning |
| [ ] | `eventHarveyPropose` | Beach | [ ] | [ ] | 10♥ |
| [ ] | `eventHarveyRoomCheckup` | HarveyRoom | [ ] | [ ] | 6♥ |
| [ ] | `eventHarveyRoomCheckup2` | HarveyRoom | [ ] | [ ] | dating + BETAS |

- [ ] **Блок G проверен**

---

### H. Debug / чужие модули / не подключено

| ☐ | Event ID | Статус | Cut | Заметки |
|:-:|----------|--------|:---:|---------|
| [ ] | `HarveyMod_TreatmentPlanMeeting` | HarveyOverhaulStress | [ ] | Не тестировать в Injury-слоте |
| [ ] | `eventHarveyCareMovementAnimationTest` | debug-only | [ ] | manual Hospital |
| [ ] | `MyMod_HarveyUrgentFarmVisit` | 💀 не в content.json | [ ] | |
| [ ] | `MyMod_HarveyStormComfortForest` | 💀 не в content.json | [ ] | |
| [ ] | `MyMod_HarveyStressTiredCheck` | 💀 не в content.json | [ ] | |

- [ ] **Блок H просмотрен**

---

### Сводка по событиям

| Категория | Всего | Проверено | Баги |
|-----------|------:|----------:|-----:|
| A. Шахта / экстренные | 8 | | |
| B. Лечение / госпиталь | 11 | | |
| C. Onboarding / ферма | 7 | | |
| D. Pass-out | 1 | | |
| E. Storm comfort | 6 | | |
| F. Story arc | 24 | | |
| G. Romance | 6 | | |
| H. Debug / orphan | 5 | | |
| **Итого** | **68** | | |

---

## Чеклист перед релизом (MainInjury)

- [ ] Сценарии 1–3: apply / block / upgrade main
- [ ] Сценарии 4–4c: DirtyWound (open wound, phased treatment, negative for non-DirtyInMines)
- [ ] Сценарии 5–5d: infection escalation + WetBandage whitelist / cleanup
- [ ] Сценарий 6: phase treatment, main id stable
- [ ] Сценарий 7: cure main, complications preserved, Neglect reset
- [ ] Сценарии 8a–8b: severe только по main
- [ ] Сценарий 9: save/load persistence
- [ ] Сценарий 10: migration / priority order
- [ ] Сценарий 11: NeglectStrikesByInjury per main
- [ ] Быстрая регрессия 1→2→3→4b→5→5b→5d→7→8b→11
- [ ] F10 / `injury_debug_dump` без рассинхрона main vs buffs
- [ ] SMAPI: нет exception при apply/cure/migrate

---

## Журнал багов

| # | Сценарий / Event | Описание | Severity | Статус |
|---|------------------|----------|----------|--------|
| 1 | | | | |
| 2 | | | | |

---

## Связанные документы

| Документ | Содержание |
|----------|------------|
| [`FOR_TEST.md`](FOR_TEST.md) | Все debug-команды, травмы, pass-out |
| [`EVENTS_TEST_CHECKLIST.md`](EVENTS_TEST_CHECKLIST.md) | Пошаговые сценарии S01–S19 для событий |
| [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md) | Topics, mail, тон Харви |
| [`../mines-forbidden-injuries.md`](../mines-forbidden-injuries.md) | Шахта, severe, госпитализация |
| [`../events-inventory/00-summary-table.md`](../events-inventory/00-summary-table.md) | Автосводка всех Event ID |
