# Осложнения — чеклист QA

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Механики C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) · CP: [02-cp-content-inventory.md](02-cp-content-inventory.md)  
> Assert: [05-debug-dump-commands.md](05-debug-dump-commands.md) · Setup: [06-debug-setup-commands.md](06-debug-setup-commands.md)  
> Лечение (базовый пайплайн): [08-injury-treatment-tests.md](08-injury-treatment-tests.md)  
> MainInjury (сценарии 4–5, 11 — перекрёстные ссылки): [main-injury-testcases.md](main-injury-testcases.md)  
> MCP: [stardew-mcp.md](stardew-mcp.md) · [injury-mcp.md](injury-mcp.md)

**Область:** осложнения InjuryCare (`HarveyMod_*`), eligibility, эскалация в `buffInfectedWound`, лечение через `TreatComplications`, Neglect, PainFlare, AllergicRash.  
**Не цель:** полный прогон MainInjury priority, госпитализация, save/load — см. соседние чеклисты.

Отмечайте `- [ ]` → `- [x]` по мере проверки.

---

## Журнал прогона

| Поле | Значение |
|------|----------|
| Тестер | |
| Слот сохранения | |
| Версия C# мода | |
| Версия CP | |
| Дата | |

| ID | Сценарий | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-COMP-001 | WetBandage — ручное наложение и лечение | [ ] | |
| HOI-COMP-002 | WetBandage от дождя (gameplay) | [ ] | |
| HOI-COMP-003 | WetBandage НЕ без лечения | [ ] | |
| HOI-COMP-004 | DirtyWound в шахте | [ ] | |
| HOI-COMP-005 | DirtyWound — неподходящие main | [ ] | |
| HOI-COMP-006 | DirtyWound → InfectedWound | [ ] | |
| HOI-COMP-007 | WetBandage → InfectedWound | [ ] | |
| HOI-COMP-008 | WetStitches (бассейн) | [ ] | |
| HOI-COMP-009 | Neglect (заброшенность) | [ ] | |
| HOI-COMP-010 | Neglect per-injury regression | [ ] | |
| HOI-COMP-011 | PainFlare от грозы | [ ] | |
| HOI-COMP-012 | AllergicRash (buff/topic) | [ ] | |

---

## Предусловия (все TC)

- [ ] Игра через SMAPI, загружен тестовый сейв (`Context.IsWorldReady`)
- [ ] C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`
- [ ] `injury_validate_buffs` → `result=OK` (см. [07-smoke-save-tests.md](07-smoke-save-tests.md))
- [ ] `SendLetters: true` в config — если проверяете mail эскалации
- [ ] Injury MCP `user-harvey-injury` (опционально) · StardewMCP `user-stardew`
- [ ] Перед **каждым** изолированным TC: `injury_reset`

### Whitelist осложнений (C# `InjurySets`)

| Набор | BuffId main | Осложение |
|-------|-------------|-----------|
| `DirtyInMines` | `buffDeepCuts`, `buffBurnWounds`, `buffShrapnelWounds` | `HarveyMod_DirtyWound` |
| `WetBandageSensitive` | `buffDeepCuts`, `buffBurnWounds`, `buffShrapnelWounds`, `buffInfectedWound`, `buffSurgicalWound` | `HarveyMod_WetBandage` |
| Surgical/shrapnel context | `buffSurgicalWound`, `buffShrapnelWounds` (+ phase) | `HarveyMod_WetStitches` |
| `StormPainSensitive` | `buffFracturedBone`, `buffShrapnelWounds`, `buffTornMuscles`, `buffBruisedRibs` | `HarveyMod_PainFlare` (гроза 30%/час) |

**WetBandage от воды/дождя:** только при `TreatmentStarted=true` и main ∈ `WetBandageSensitive`.  
**`buffHurt` / `buffFracturedBone`:** WetBandage **не** применим (нет в whitelist).

### Общая подготовка мира (StardewMCP)

| Tool | Аргументы | Зачем |
|------|-----------|-------|
| `teleport_player` | `Hospital`, опц. `x`: `20`, `y`: `5` | Клик по Харви |
| `set_time` | `10am` | Рабочие часы клиники |
| `set_npc_relationship` | `Harvey`, `3` | Диалоги лечения |
| `set_weather` | `rain` / `storm` | WetBandage / PainFlare |
| `teleport_player` | `Farm` / outdoor | Exposure под дождём |
| `warp_to_mine_floor` | `10` (или `Mine` через `teleport_player`) | DirtyWound exposure |
| `teleport_player` | `BathHouse_Pool` | WetStitches |
| `set_date` | сезон `spring` | AllergicRash (CP/контекст) |
| `advance_day` | — | Daily infection / neglect |
| `pause_time` | `false` | Для отсчёта игровых минут exposure |

**Клик по Харви:** StardewMCP не умеет action click. Варианты: **вручную** (E2E) или `injury_harvey_click` (механика без cutscene, в т.ч. `TreatComplications`).

### Assert после каждой мутации

```
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

Ожидаемые tags в `injury_buff_dump`: `mod,complication` для `HarveyMod_*`.

### Полезные лог-префиксы

| Префикс | Когда |
|---------|-------|
| `[Complication] MainInjury=..., complication=...` | Наложение / эскалация |
| `[WetBandage] allowed/skip: ...` | Eligibility дождя/воды |
| `[DirtyWound] allowed/skip: ...` | Eligibility шахты |
| `[Complication] Cleared wound-related complications after infection` | Эскалация → infected |
| `[Complication] Infection escalation finalized` | Финал daily roll |
| `[Neglect] ...` | Strikes / сброс / штраф |
| `[Prescription] KeepDry pool violation` | Бассейн + KeepDry |
| `[QA] injury_complication_add SKIP: ...` | QA eligibility отказ |

---

## HOI-COMP-001 — WetBandage вручную (QA add + TreatComplications)

### ID

HOI-COMP-001

### Цель

Проверить ручное наложение `HarveyMod_WetBandage` через QA-команду после начала лечения и снятие осложнения кликом по Харви (`TreatComplications`).

### Подготовка (StardewMCP)

Общая подготовка (Hospital, 10am, Harvey 3♥).

> **Main для WetBandage:** используйте `buffDeepCuts` (или другой ID из `WetBandageSensitive`).  
> `buffHurt` ∉ whitelist → `injury_complication_add HarveyMod_WetBandage` даст **SKIP** (опциональный негатив в конце TC).

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_state_dump
```

После StartTreatment:

```
# вручную: клик Harvey  ИЛИ  injury_harvey_click
injury_state_dump
injury_buff_dump
injury_phase_list
```

Наложение осложнения:

```
injury_complication_add HarveyMod_WetBandage
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

Лечение осложнения:

```
# вручную: клик Harvey  ИЛИ  injury_harvey_click
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffDeepCuts`.
2. Assert до лечения: `TreatmentStarted=False`; `ActiveComplications.count=0`.
3. Teleport к Harvey → **клик** (StartTreatment, phase 1 или cure-path).
4. Assert: `TreatmentStarted=True`; main остаётся `buffDeepCuts`.
5. `injury_complication_add HarveyMod_WetBandage` — ожидается `ok=yes`, не SKIP.
6. Assert после add:
   - `injury_buff_dump`: `buff=HarveyMod_WetBandage tags=mod,complication`
   - `injury_state_dump`: `ActiveComplications.HarveyMod_WetBandage` (день старта)
   - `injury_topic_dump`: `topic=topicHarvey_WetBandage owned`
   - `MainInjuryId=buffDeepCuts` (не меняется)
7. **Клик Harvey** → `TreatComplications`.
8. Assert после treat:
   - нет `HarveyMod_WetBandage` в buff/state/topics
   - `MainInjuryId=buffDeepCuts`; лечение main **не** прервано (phase buff / `TreatmentStarted=True`)

### Опционально — негатив на `buffHurt`

```
injury_reset
injury_debuff_add buffHurt
injury_harvey_click
injury_complication_add HarveyMod_WetBandage
```

Ожидается: `[QA] injury_complication_add SKIP: main not WetBandageSensitive`.

### Debug HUD (F10)

- `Complications: HarveyMod_WetBandage` между шагами 5–7
- `Main injury: buffDeepCuts`, `treatment started: yes`

### SMAPI log

- `[QA] injury_complication_add complication=HarveyMod_WetBandage ok=yes`
- `[Complication] MainInjury=buffDeepCuts, complication=HarveyMod_WetBandage`
- `[MedicalAction] applied type=TreatComplications complications=HarveyMod_WetBandage` (или список без wet после treat)

### Критерий прохождения

- **PASS:** после add — buff + `ActiveComplications` + topic; после клика — осложнение снято, main и лечение сохранены.
- **FAIL:** SKIP на add при treated `buffDeepCuts`; осложнение остаётся после TreatComplications; main потерян.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-002 — WetBandage от дождя (gameplay)

### ID

HOI-COMP-002

### Цель

Проверить автоматическое наложение `HarveyMod_WetBandage` при активном лечении main из `WetBandageSensitive` + exposure под дождём (без QA add).

### Подготовка (StardewMCP)

```
teleport_player Farm
set_time 10am
set_weather rain
pause_time false
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click
injury_state_dump
injury_rain_debug
```

После exposure:

```
injury_rain_debug
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffDeepCuts`.
2. **Клик Harvey** → `TreatmentStarted=True` (обязательное предусловие).
3. StardewMCP: `set_weather rain` → `teleport_player` на outdoor (`Farm`, `Town`, …).
4. **Ждать 60+ игровых минут** под дождём на улице (1 real sec ≈ 1 sec rain counter) **или** ускорить через `injury_rain_debug [secToday] [continuous]` + gameplay tick (зафиксировать способ в журнале).
5. Assert:
   - `HarveyMod_WetBandage` в buff + `ActiveComplications`
   - `topicHarvey_WetBandage` owned
   - `MainInjuryId=buffDeepCuts`
6. `injury_rain_debug` — счётчики `TimeUnderRainTicks` / continuous rain выросли.

### Ожидаемый результат

| Поле | Значение |
|------|----------|
| Log | `[WetBandage] allowed: main=buffDeepCuts, treatmentStarted=True` → `[Complication] ... HarveyMod_WetBandage` |
| HUD | «Повязка промокла!» (или эквивалент) |
| Main | не меняется |

### Debug HUD (F10)

- `Complications: HarveyMod_WetBandage`
- `treatment started: yes`

### Критерий прохождения

- **PASS:** WetBandage появился **только после** StartTreatment + 60m rain outdoors.
- **FAIL:** нет осложнения при treated deep cuts + достаточном exposure; WetBandage до клика Harvey.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-003 — WetBandage НЕ появляется без лечения

### ID

HOI-COMP-003

### Цель

Негатив: дождь **не** добавляет WetBandage, пока `TreatmentStarted=false` (открытая рана / main без повязки).

### Подготовка (StardewMCP)

```
set_weather rain
teleport_player Farm
pause_time false
```

### Команды SMAPI / Injury MCP

**Вариант A — `buffDeepCuts` (WetBandageSensitive, но без лечения):**

```
injury_reset
injury_debuff_add buffDeepCuts
injury_phase_list
# НЕ кликать Harvey
```

**Вариант B — `buffHurt` (не sensitive):**

```
injury_reset
injury_debuff_add buffHurt
injury_phase_list
```

После дождя (оба варианта):

```
injury_rain_debug
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

При stale buff из старых прогонов:

```
injury_cleanup_invalid_complications
```

### Шаги

1. `injury_reset` → `injury_debuff_add` (**без** `injury_harvey_click`).
2. Assert: `TreatmentStarted=False`; `ActiveComplications.count=0`.
3. StardewMCP: дождь + outdoor 60+ игровых минут (или `injury_rain_debug` + ожидание).
4. Assert:
   - `HarveyMod_WetBandage` **отсутствует**
   - `topicHarvey_WetBandage` **отсутствует**
   - Log: `[WetBandage] skip: treatment not started` и/или `no active bandage/treatment`

### Ожидаемый результат

- Main (`buffDeepCuts` / `buffHurt`) на месте; complications пусты.
- Перекрёстная ссылка: [main-injury-testcases.md](main-injury-testcases.md) сценарий **5b**.

### Критерий прохождения

- **PASS:** ни buff, ни topic, ни `ActiveComplications` для WetBandage после rain без лечения.
- **FAIL:** WetBandage появился до StartTreatment (кроме stale — тогда cleanup и повтор).

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-004 — DirtyWound в шахте

### ID

HOI-COMP-004

### Цель

Проверить eligibility и roll `HarveyMod_DirtyWound` в шахте при main ∈ `DirtyInMines` (`buffDeepCuts`), включая HUD warning и debug exposure.

### Подготовка (StardewMCP)

```
set_time 10am
warp_to_mine_floor 10
# или: teleport_player location=Mine x=17 y=7
pause_time false
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_mine_dirty_debug
injury_state_dump
injury_phase_list
```

После времени в шахте:

```
injury_mine_dirty_debug
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffDeepCuts` (открытая рана, `TreatmentStarted=False`).
2. `injury_mine_dirty_debug` — assert `hasDirtyInjury=true`, `hasDirtyWound=false` (до roll).
3. StardewMCP: `warp_to_mine_floor 10` (или Mine).
4. **60+ игровых минут** в шахте (или дождаться roll exposure — зафиксировать время в журнале).
5. Assert при успешном roll:
   - `HarveyMod_DirtyWound` buff + `ActiveComplications`
   - `topicHarvey_DirtyWound`
   - `MainInjuryId=buffDeepCuts`
   - Log: `[DirtyWound] allowed: main=buffDeepCuts, reason=open or treated wound surface` → `[Complication] ... HarveyMod_DirtyWound`
6. Опционально: HUD warning о грязи / риске (зафиксировать текст).

### Debug HUD (F10)

- `Complications: HarveyMod_DirtyWound` после roll
- Счётчики mine dirty в F10 / `injury_mine_dirty_debug`

### Критерий прохождения

- **PASS:** `hasDirtyInjury=true` в debug; DirtyWound может появиться после exposure; main не дублируется.
- **FAIL:** `hasDirtyInjury=false` для `buffDeepCuts`; DirtyWound без exposure; main заменена на complication.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-005 — DirtyWound НЕ для неподходящих травм

### ID

HOI-COMP-005

### Цель

Негатив: mine exposure **не** даёт DirtyWound, если main ∉ `DirtyInMines`.

### Подготовка (StardewMCP)

```
warp_to_mine_floor 10
pause_time false
```

### Команды SMAPI / Injury MCP

Для **каждого** buffId отдельный подпрогон (`injury_reset` между ними):

```
injury_reset
injury_debuff_add <buffId>
injury_mine_dirty_debug
# 60+ мин в шахте
injury_mine_dirty_debug
injury_phase_list
injury_buff_dump
```

| Под-TC | buffId | Ожидание |
|--------|--------|----------|
| 005a | `buffHurt` | DirtyWound **нет** |
| 005b | `buffSprainedAnkle` | DirtyWound **нет** |
| 005c | `buffBackStrain` | DirtyWound **нет** |

Дополнительно QA SKIP:

```
injury_complication_add HarveyMod_DirtyWound
```

→ `[QA] injury_complication_add SKIP: main not in DirtyInMines` для каждого ID выше.

### Шаги

1. Для каждого buffId: reset → debuff_add → mine 60+ min.
2. Assert: `hasDirtyInjury=false` в `injury_mine_dirty_debug`.
3. Log: `[DirtyWound] skip: main not in DirtyInMines, main=<buffId>`.
4. `HarveyMod_DirtyWound` отсутствует в buff/state/topics.

### Критерий прохождения

- **PASS:** все три main — без DirtyWound от шахты; QA add SKIP.
- **FAIL:** DirtyWound на fracture/sprain/back strain от mine roll.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-006 — DirtyWound → InfectedWound (эскалация)

### ID

HOI-COMP-006

### Цель

Проверить daily infection escalation: `HarveyMod_DirtyWound` (день 3+) → замена main на `buffInfectedWound`, очистка wound-complications.

### Подготовка (StardewMCP)

```
set_time 6am
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_complication_add HarveyMod_DirtyWound
injury_test_age_complication HarveyMod_DirtyWound 3
injury_state_dump
injury_buff_dump
```

Daily check (один из путей):

```
# Путь A — gameplay
# StardewMCP: advance_day
```

```
# Путь B — ускорение (рекомендуется для CI)
# advance_day один раз при ageDays=3 → roll 100%
```

После утра:

```
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
injury_debug_dump
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffDeepCuts`.
2. `injury_complication_add HarveyMod_DirtyWound` → assert complication active.
3. `injury_test_age_complication HarveyMod_DirtyWound 3` (возраст ≥3 дней → **100%** roll).
4. StardewMCP: `advance_day` (DayStarted → `CheckTreatmentCompletion` / infection roll).
5. Assert после эскалации:
   - `MainInjuryId=buffInfectedWound`
   - `buffDeepCuts` **снят** (buff, DebuffState, topics deep cuts)
   - `HarveyMod_DirtyWound` **снят**
   - `ActiveComplications` без wound-related (`DirtyWound`, `WetBandage`, `Neglect`, `WetStitches`)
   - `topicInfectedWound` есть; `topicHarvey_DirtyWound` **нет**
   - Mail `HarveyMod_DirtyWoundInfection` (если letters on)
   - Log: `[Complication] Infection escalation finalized (... source=HarveyMod_DirtyWound ...)`
   - `NeglectStrikesByInjury` сброшен

### Шкала roll (справочно)

| Дней осложнения | Шанс |
|-----------------|------|
| 1 | 15% |
| 2 | 40% |
| 3+ | 100% |

### Критерий прохождения

- **PASS:** main заменена на infected; wound complications cleared; mail/topic по таблице.
- **FAIL:** DirtyWound остаётся на day 3+ без roll; две main одновременно.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-007 — WetBandage → InfectedWound (эскалация)

### ID

HOI-COMP-007

### Цель

Проверить эскалацию `HarveyMod_WetBandage` (день 3+) → `buffInfectedWound` по шкале `CalculateWetBandageInfectionChance`.

### Подготовка (StardewMCP)

Общая подготовка; `set_time 6am`.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click
injury_complication_add HarveyMod_WetBandage
injury_test_age_complication HarveyMod_WetBandage 3
injury_state_dump
```

```
# StardewMCP: advance_day
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Шаги

1. Treated `buffDeepCuts` (`TreatmentStarted=True`).
2. `injury_complication_add HarveyMod_WetBandage`.
3. `injury_test_age_complication HarveyMod_WetBandage 3`.
4. `advance_day` → daily infection check.
5. Assert (аналогично HOI-COMP-006, source=WetBandage):
   - `MainInjuryId=buffInfectedWound`
   - `HarveyMod_WetBandage` cleared
   - Mail `HarveyMod_WetBandageInfection`
   - Log: `source=HarveyMod_WetBandage`, `alreadyInfected=false`
   - `TreatmentStarted=false` для новой main infected

### Критерий прохождения

- **PASS:** эскалация с treated deep cuts + wet bandage day 3+.
- **FAIL:** WetBandage остаётся; main не infected; wound complications не cleared.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-008 — WetStitches (бассейн + surgical)

### ID

HOI-COMP-008

### Цель

Проверить `HarveyMod_WetStitches` при входе в `BathHouse_Pool` с активной `buffSurgicalWound` и последующее лечение у Харви.

### Подготовка (StardewMCP)

```
teleport_player BathHouse_Pool
set_time 10am
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffSurgicalWound
injury_state_dump
injury_buff_dump
```

После входа в pool (LocationChanged):

```
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

Лечение:

```
teleport_player Hospital
injury_harvey_click
# при наличии complication — второй клик TreatComplications
#   ИЛИ один клик если FSM объединяет
injury_state_dump
injury_buff_dump
injury_topic_dump
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffSurgicalWound`.
2. Assert: `MainInjuryId=buffSurgicalWound`; `TreatmentStarted=False` (до клика).
3. StardewMCP: `teleport_player BathHouse_Pool` — триггер `HandleSpaLogic`.
4. Assert WetStitches:
   - `buff=HarveyMod_WetStitches tags=mod,complication`
   - `topicHarvey_WetStitches`
   - HUD «Швы намокли!» (или эквивалент)
   - Log: `Швы намокли при купании` / `[Prescription] KeepDry pool violation` (если активен KeepDry)
5. Teleport Hospital → **клик Harvey**:
   - StartTreatment (`buffPostSurgicalCare`) при первом визите
   - TreatComplications снимает WetStitches (повторный клик или тот же, зафиксировать FSM)
6. Assert: WetStitches снят; main лечение продолжается.

### QA shortcut (опционально)

```
injury_complication_add HarveyMod_WetStitches
```

после `buffSurgicalWound` — eligibility OK; gameplay pool — отдельная проверка шага 3–4.

### Критерий прохождения

- **PASS:** WetStitches от pool/spa с surgical main; снимается TreatComplications; **не** эскалирует в infected (в отличие от Dirty/WetBandage).
- **FAIL:** нет WetStitches после pool; surgical main потеряна.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-009 — Neglect (заброшенность лечения)

### ID

HOI-COMP-009

### Цель

Проверить появление `HarveyMod_Neglect` при просрочке лечения фазовой травмы / пропуске визита к Харви (`CheckPhaseNeglect` + `CheckNeglect` на DayEnding).

### Подготовка (StardewMCP)

```
set_time 6am
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click
injury_test_age_injury buffDeepCuts 5
injury_state_dump
injury_phase_list
```

Ускорение neglect (один или оба пути):

```
# Путь A — phase neglect (фаза истекла, нет advance)
injury_phase_ready buffDeepCuts 0
# StardewMCP: advance_day × N

# Путь B — untreated main (без StartTreatment)
injury_reset
injury_debuff_add buffDeepCuts
injury_test_age_injury buffDeepCuts 4
# StardewMCP: advance_day × 3  (NeglectDaysThreshold default=3)
```

После накопления strikes:

```
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_debug_dump
injury_phase_list
```

### Шаги

1. Фазовая травма в лечении **или** нелеченная main — выбрать вариант A/B.
2. «Состарить» травму: `injury_test_age_injury` + `advance_day` без клика Harvey / без `injury_phase_advance`.
3. Assert в `injury_debug_dump`: `NeglectStrikesByInjury: buffDeepCuts=N` (N растёт на DayEnding).
4. При `N >= NeglectDaysThreshold` (config, default **3**):
   - `HarveyMod_Neglect` buff + `ActiveComplications`
   - `topicHarvey_Neglect`
   - Mail `HarveyMod_NeglectWarning` (phase neglect path)
   - Log: `Применение штрафа за заброшенность` / `[Complication] ... NeglectWarning`
5. **Клик Harvey** → TreatComplications снимает Neglect (зафиксировать).

### Debug HUD (F10)

- `Complications: HarveyMod_Neglect`
- Neglect strikes в full dump (F10 / `injury_debug_dump`)

### Критерий прохождения

- **PASS:** Neglect после просрочки; strikes видны в state; treat снимает complication.
- **FAIL:** Neglect на day 1; strikes без DayEnding; neglect без mail/topic.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-010 — Neglect per-injury regression

### ID

HOI-COMP-010

### Цель

Проверить, что `NeglectStrikesByInjury` **per buffId**: strikes одной травмы не переносятся на другую main.

### Подготовка (StardewMCP)

`set_time 6am`.

### Команды SMAPI / Injury MCP

**Часть A — смена main:**

```
injury_reset
injury_debuff_add buffHurt
# advance_day × 3 без лечения → strikes buffHurt
injury_debug_dump
injury_debuff_add --force buffDeepCuts
injury_debug_dump
injury_state_dump
```

**Часть B — StartTreatment сброс:**

```
injury_reset
injury_debuff_add buffDeepCuts
# накопить strikes (advance_day × 3, без лечения)
injury_debug_dump
injury_harvey_click
injury_debug_dump
```

**Часть C — infection сброс (перекрёстно с COMP-006):**

После эскалации Dirty→Infected — `NeglectStrikesByInjury` для старой main cleared.

### Шаги

1. **A:** Накопить strikes для `buffHurt` → `--force buffDeepCuts` → assert strikes для `buffHurt` **удалены**, для `buffDeepCuts` = **0**.
2. Log: `[Neglect] Сброс счётчика при смене MainInjuryId: buffHurt -> buffDeepCuts`.
3. **B:** Strikes на `buffDeepCuts` → StartTreatment → strikes для `buffDeepCuts` **сброшены**.
4. Log: `[Neglect] Сброс NeglectStrikesByInjury для buffDeepCuts` (или эквивалент).
5. **C:** После infection escalation — глобальный/per-injury reset (см. COMP-006 assert).

### Ожидаемый результат

| Событие | NeglectStrikesByInjury |
|---------|------------------------|
| `--force` другая main | старая запись удалена, новая main = 0 |
| StartTreatment | сброс для текущей main |
| Infection escalation | сброс (не карать в день эскалации) |

### Критерий прохождения

- **PASS:** strikes не «переезжают» между buffId; сбросы по таблице.
- **FAIL:** `buffDeepCuts` наследует N от `buffHurt`; strikes растут после StartTreatment.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-011 — PainFlare от грозы

### ID

HOI-COMP-011

### Цель

Проверить `HarveyMod_PainFlare` при грозе снаружи для main ∈ `StormPainSensitive` (`buffFracturedBone` / `buffShrapnelWounds`).

### Подготовка (StardewMCP)

```
set_weather storm
teleport_player Farm
set_time 1200
pause_time false
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffFracturedBone
injury_state_dump
injury_phase_list
```

После exposure (или QA shortcut):

```
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

**QA shortcut** (если roll 30% не воспроизводится):

```
injury_complication_add HarveyMod_PainFlare
```

→ eligibility: main ∈ storm/overwork sensitive; не severe hospital path.

### Шаги

1. `injury_reset` → `injury_debuff_add buffFracturedBone` (повторить с `buffShrapnelWounds` — опционально).
2. StardewMCP: `set_weather storm` + outdoor + `Game1.isLightning` активен.
3. Ждать tick `PlayerEventHandler` (гроза снаружи) — roll **30%/игровой час**.
4. Assert при успехе:
   - `HarveyMod_PainFlare` buff + `ActiveComplications`
   - `topicHarvey_PainFlare`
   - `MainInjuryId` **не меняется** (остаётся fracture/shrapnel)
   - F10: `Main injury serious` по **main**, не по PainFlare
   - Log: `[Complication] MainInjury=buffFracturedBone, complication=HarveyMod_PainFlare`
5. **Клик Harvey** → TreatComplications снимает PainFlare; main сохраняется.

### Негатив (перекрёстно)

`buffHurt` + storm → PainFlare **не** от storm (main ∉ `StormPainSensitive`). QA add → SKIP.

### Критерий прохождения

- **PASS:** PainFlare от storm на fracture/shrapnel; не тригgerит severe/hospital; treat снимает.
- **FAIL:** PainFlare меняет MainInjuryId; forced hospital только из-за PainFlare.

### Статус

- [ ] Сценарий пройден

---

## HOI-COMP-012 — AllergicRash (buff / topic)

### ID

HOI-COMP-012

### Цель

Проверить осложнение `HarveyMod_AllergicRash` и topic `topicHarvey_AllergicRash` — наложение, assert, лечение.  
**Примечание:** в проанализированном C# **нет autotrigger** от весны (`SpringRashChance` в config не подключён) — gameplay spring roll помечен как **manual / CP future**; основной путь — QA add.

### Подготовка (StardewMCP)

```
set_date spring 15
teleport_player Hospital
set_time 10am
```

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_complication_add HarveyMod_AllergicRash
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

Лечение:

```
injury_harvey_click
# TreatComplications если complication активен параллельно с StartTreatment — зафиксировать порядок FSM
injury_state_dump
injury_buff_dump
injury_topic_dump
```

### Шаги

1. `injury_reset` → любая active main (например `buffDeepCuts`).
2. StardewMCP: `set_date` → **spring** (для контекста CP dialogue / proximity).
3. `injury_complication_add HarveyMod_AllergicRash` → `ok=yes` (нет main whitelist; только duplicate check).
4. Assert:
   - `buff=HarveyMod_AllergicRash tags=mod,complication`
   - `topicHarvey_AllergicRash owned`
   - `ActiveComplications.HarveyMod_AllergicRash`
   - `MainInjuryId` unchanged
5. **Клик Harvey** → TreatComplications → rash снят.
6. **Gameplay spring (опционально, может FAIL по design):** прогулка spring outdoor / CP triggers — если rash **не** появился автоматически, зафиксировать **EXPECTED** (нет C# trigger) в журнале, не считать FAIL.

### CP / dialogue (опционально)

- Proximity key `Proximity_Complication_AllergicRash` при Harvey рядом
- `injury_proximity_test AllergicRash` — только текст, без state

### Критерий прохождения

- **PASS:** QA add + buff/topic/state; TreatComplications снимает; infection escalation **не** трогает AllergicRash (не wound-related).
- **FAIL:** duplicate add без ошибки; rash cleared при infection escalation (не должно, если unrelated).

### Статус

- [ ] Сценарий пройден

---

## Быстрый прогон (минимум)

| Приоритет | ID | Зачем |
|-----------|-----|-------|
| P0 | HOI-COMP-001 | QA add + TreatComplications baseline |
| P0 | HOI-COMP-003 | WetBandage gate (treatment required) |
| P0 | HOI-COMP-006 | Dirty → infected escalation |
| P0 | HOI-COMP-010 | NeglectStrikes regression |
| P1 | HOI-COMP-002 | Rain gameplay |
| P1 | HOI-COMP-004 | Mine DirtyWound |
| P1 | HOI-COMP-007 | Wet → infected |
| P1 | HOI-COMP-011 | Storm PainFlare |
| P2 | HOI-COMP-005, 008, 009, 012 | Негативы, spa, neglect paths, rash |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC, правило «ручной клик».
2. **Этот файл** — [09-complication-tests.md](09-complication-tests.md) — незакрытые HOI-COMP в журнале прогона.
3. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — поля state: шахта (`MineWarningDay`, `MineForbiddenAppliedDay`, dirty exposure), pass-out, `NeedsMineRescueEvent`.
4. [02-cp-content-inventory.md](02-cp-content-inventory.md) — CP events mine rescue / interception / skull prevention; mail `mailHarveyMineForbidden`.
5. [main-injury-testcases.md](main-injury-testcases.md) — блок **A** (mine rescue events), severe mine entry — не дублировать с HOI-COMP, использовать как E2E дополнение.
6. **Следующий артеfact:** `docs/testing/10-mine-tests.md` (или аналог) — **шахта**, exposure rolls, `HarveyMod_MineForbidden`, warning/interception events, mine rescue pipeline.
7. **После шахты:** `docs/testing/11-pass-out-tests.md` (или аналог) — **обмороки**: Town 2:00, emergency care, exhaustion, hospital queue, `PassOutHandler` flags.
8. [06-debug-setup-commands.md](06-debug-setup-commands.md) · [05-debug-dump-commands.md](05-debug-dump-commands.md) — assert для mine/forbidden (`injury_mine_dirty_debug`, `injury_mine_forbidden_clear`, `injury_debug_mine_rescue`).
9. [stardew-mcp.md](stardew-mcp.md) — `warp_to_mine_floor`, `set_health`, `teleport_player` для pass-out сценариев.
