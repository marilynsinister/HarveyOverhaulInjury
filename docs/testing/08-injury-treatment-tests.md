# Простые и фазовые травмы — лечение (чеклист QA)

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Механики C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md)  
> Assert-команды: [05-debug-dump-commands.md](05-debug-dump-commands.md) · Setup: [06-debug-setup-commands.md](06-debug-setup-commands.md)  
> Smoke / save-load: [07-smoke-save-tests.md](07-smoke-save-tests.md)  
> MCP: [stardew-mcp.md](stardew-mcp.md) · [injury-mcp.md](injury-mcp.md)

**Область:** полный цикл лечения **простых** (`buffHurt`, `buffBadlyHurt`, `buffSurgicalWound`) и **фазовых** (все 11 ID из `KnownTraumas`) через клик Харви + QA dump/assert.  
**Не цель:** осложнения, MainInjury priority, госпитализация в глубину — см. [main-injury-testcases.md](main-injury-testcases.md) и следующий чат (осложнения).

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
| HOI-SIMPLE-001 | buffHurt — simple treatment | [ ] | |
| HOI-SIMPLE-002 | buffBadlyHurt — simple treatment | [ ] | |
| HOI-SIMPLE-003 | buffSurgicalWound — simple treatment | [ ] | |
| HOI-PHASE-001 | buffSprainedAnkle | [ ] | |
| HOI-PHASE-002 | buffBruisedRibs | [ ] | |
| HOI-PHASE-003 | buffBackStrain | [ ] | |
| HOI-PHASE-004 | buffDeepCuts | [ ] | |
| HOI-PHASE-005 | buffBurnWounds | [ ] | |
| HOI-PHASE-006 | buffInfectedWound | [ ] | |
| HOI-PHASE-007 | buffTornMuscles | [ ] | |
| HOI-PHASE-008 | buffConcussion | [ ] | |
| HOI-PHASE-009 | buffFracturedBone | [ ] | |
| HOI-PHASE-010 | buffShrapnelWounds | [ ] | |
| HOI-PHASE-011 | buffCold | [ ] | |
| HOI-PHASE-REG-001 | ReadyForRecovery не на последней фазе | [ ] | |
| HOI-PHASE-REG-002 | phase_ready у нефазовой травмы | [ ] | |
| HOI-PHASE-REG-003 | Повторный клик после выздоровления | [ ] | |
| HOI-PHASE-REG-004 | Потерянный phase buff при DebuffState | [ ] | |
| HOI-PHASE-REG-005 | DebuffState есть, buff отсутствует | [ ] | |

---

## Предусловия (все TC)

- [ ] Игра через SMAPI, загружен тестовый сейв (`Context.IsWorldReady`)
- [ ] C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`
- [ ] `injury_validate_buffs` → `result=OK` (см. [07-smoke-save-tests.md](07-smoke-save-tests.md) HOI-CMD-003)
- [ ] Injury MCP `user-harvey-injury` (опционально) · StardewMCP `user-stardew`
- [ ] Перед **каждым** изолированным TC: `injury_reset`

### Общая подготовка мира (StardewMCP)

| Tool | Аргументы | Зачем |
|------|-----------|-------|
| `teleport_player` | `location`: `Hospital`, опц. `x`: `20`, `y`: `5` | Харви в клинике |
| `set_time` | `10am` | Рабочие часы клиники |
| `set_npc_relationship` | `Harvey`, `3` (750 pts) | First treatment / диалоги |
| `pause_time` | `true` | Опционально — стабильный прогон |
| `get_npc_location` | `Harvey` | Убедиться, что NPC в той же локации |

**Клик по Харви:** StardewMCP **не** умеет action click. Варианты:

| Способ | Когда |
|--------|-------|
| **Вручную** — подойти к Harvey, ЛКМ | E2E, cutscene, диалог |
| `injury_harvey_click` | Механика без диалога (StartTreatment / AdvancePhase / CompleteRecovery) |

Для TC с пометкой «клик» — сначала teleport + `get_npc_location`; если Harvey недоступен — warp в его текущую локацию.

---

## Справочник: простые vs фазовые

| Тип | BuffId | Cure после StartTreatment | Срок лечения (дней) | `TotalPhases` |
|-----|--------|---------------------------|---------------------|---------------|
| простая | `buffHurt` | `buffHarveyTreatment` | 2 | 0 |
| простая | `buffBadlyHurt` | `buffHarveyIntensiveCare` | 4 | 0 |
| простая | `buffSurgicalWound` | `buffPostSurgicalCare` | 7 | 0 |
| фазовая | см. HOI-PHASE-001…011 | phase buffs → `buffHarveyCare` | сумма фаз | 2 или 3 |

**Завершение простого лечения:** `DayStarted` → `CheckSimpleTreatmentCompletion` (снятие cure buff, `MainInjuryId` очищен, `topic*Cured`).  
**Завершение фазового лечения:** `injury_phase_recovery` + клик → `buffHarveyCare`, main снята.

---

# Простые травмы

Общий пайплайн (все HOI-SIMPLE-*):

```
injury_reset
injury_debuff_add <buffId>
injury_state_dump
→ teleport к Harvey (StardewMCP)
→ клик по Harvey (вручную или injury_harvey_click)
injury_state_dump
injury_buff_dump
→ advance_day × N до завершения срока
injury_state_dump
injury_buff_dump
injury_topic_dump
```

**Ускорение (опционально):** `injury_test_age_injury <buffId> <days>` вместо многократного `advance_day` — см. [06-debug-setup-commands.md](06-debug-setup-commands.md). Основной путь TC — `advance_day`.

---

## HOI-SIMPLE-001 — buffHurt

### ID

HOI-SIMPLE-001

### Цель

Проверить simple treatment: base trauma → cure `buffHarveyTreatment` → авто-завершение через 2 игровых дня.

### Подготовка (StardewMCP)

Общая подготовка (Hospital, 10am, Harvey 3♥).

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffHurt
injury_state_dump
```

После клика по Harvey:

```
injury_state_dump
injury_buff_dump
injury_phase_list
```

Дождаться завершения (2 дня после StartTreatment):

```
# StardewMCP: advance_day  (×2 от PhaseStartDay, или до срабатывания CheckSimpleTreatmentCompletion)
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffHurt`.
2. **Assert до лечения:** `injury_state_dump` — `MainInjuryId=buffHurt`, `TreatmentStarted=False`, `TotalPhases=0`, `CurrentPhase=0`.
3. StardewMCP: `teleport_player` Hospital → **клик по Harvey** (или `injury_harvey_click`).
4. **Assert после StartTreatment:** `TreatmentStarted=True`; `injury_buff_dump` — `buff=buffHarveyTreatment tags=mod,cure` (base `buffHurt` снят).
5. StardewMCP: `advance_day` до истечения 2 дней лечения (`PhaseStartDay + Phase1Duration`).
6. **Assert завершение:** cure buff снят; `MainInjuryId=(none)`; `ActiveDebuffs.count=0`; `topic=topicHurtCured` в `injury_topic_dump`.

### Ожидаемый результат

| Этап | state dump | buff dump |
|------|------------|-----------|
| После debuff_add | `MainInjuryId=buffHurt`, `TreatmentStarted=False` | `buff=buffHurt tags=mod,trauma` |
| После клика | `TreatmentStarted=True`, `Phase1Duration=2` | `buff=buffHarveyTreatment tags=mod,cure` |
| После срока | `MainInjuryId` пуст; DebuffState удалён | нет trauma/cure mod buffs |

### Log markers

| Маркер | Когда |
|--------|-------|
| `Нефазовое лечение начато: buffHarveyTreatment, срок=2 дней` | StartTreatment |
| `Нефазовое лечение завершено: buffHurt` | DayStarted |
| `[MainInjury] Основная травма завершена: buffHurt` | CompleteMainInjury |

### Pass criteria

- **PASS:** cure после клика; через 2 `advance_day` main и cure сняты; `topicHurtCured` owned.
- **FAIL:** base `buffHurt` остаётся после клика; cure не снимается; `MainInjuryId` не очищен.

### Статус

- [ ] Сценарий пройден

---

## HOI-SIMPLE-002 — buffBadlyHurt

### ID

HOI-SIMPLE-002

### Цель

Simple treatment для тяжёлой травмы: `buffHarveyIntensiveCare`, срок 4 дня; доп. topic `topicHealthDamageCritical`.

### Подготовка (StardewMCP)

Общая подготовка. Если клик вызывает госпитализацию — зафиксировать в журнале; для чистого assert лечения допустим `injury_harvey_click` или `injury_hospital_discharge` после admission (см. [06-debug-setup-commands.md](06-debug-setup-commands.md)).

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffBadlyHurt
injury_state_dump
injury_topic_dump
```

После клика и после завершения — как HOI-SIMPLE-001 (`injury_state_dump`, `injury_buff_dump`, `advance_day` ×4).

### Шаги

1. `injury_reset` → `injury_debuff_add buffBadlyHurt`.
2. Assert: `MainInjuryId=buffBadlyHurt`, `TreatmentStarted=False`; `injury_topic_dump` — `topicBadlyHurt`, `topicHealthDamageCritical`.
3. Teleport к Harvey → **клик** (или `injury_harvey_click`).
4. Assert: `TreatmentStarted=True`; `buff=buffHarveyIntensiveCare tags=mod,cure`.
5. `advance_day` ×4 (или до `CheckSimpleTreatmentCompletion`).
6. Assert: cure снят; `MainInjuryId` пуст; `topicBadlyHurtCured` (или аналог из `TopicIds.GetCuredTopic`).

### Ожидаемый результат

- Cure buff: `buffHarveyIntensiveCare`, `Phase1Duration=4`.
- После срока: нет `buffBadlyHurt`, нет intensive care; main cleared.
- `topicHealthDamageCritical` снят вместе с завершением main (или истёк — зафиксировать фактическое поведение).

### Pass criteria

- **PASS:** intensive care после клика; auto-complete через 4 дня; main очищен.
- **FAIL:** лечение не стартовало; cure остаётся бесконечно; main не снят.

### Статус

- [ ] Сценарий пройден

---

## HOI-SIMPLE-003 — buffSurgicalWound

### ID

HOI-SIMPLE-003

### Цель

Simple treatment послеоперационной раны: `buffPostSurgicalCare`, 7 дней; topic `topicPostOperativeCare`.

### Подготовка (StardewMCP)

Общая подготовка.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffSurgicalWound
injury_state_dump
injury_topic_dump
```

После клика → `advance_day` ×7 → dump.

### Шаги

1. `injury_reset` → `injury_debuff_add buffSurgicalWound`.
2. Assert до лечения: `MainInjuryId=buffSurgicalWound`, `TreatmentStarted=False`; topic `topicPostOperativeCare`.
3. Teleport к Harvey → **клик**.
4. Assert: `buff=buffPostSurgicalCare tags=mod,cure`; `Phase1Duration=7`.
5. `advance_day` до завершения (7 дней).
6. Assert: cure и main сняты; cured topic (`topicSurgicalWoundCured` / CP bridge).

### Ожидаемый результат

| Этап | buff dump |
|------|-----------|
| После debuff_add | `buffSurgicalWound tags=mod,trauma` |
| После клика | `buffPostSurgicalCare tags=mod,cure` |
| После 7 дней | mod trauma/cure отсутствуют |

### Pass criteria

- **PASS:** post-surgical cure 7 дней; auto-complete; main cleared.
- **FAIL:** `buffSurgicalWound` не заменён на cure; срок не отсчитывается.

### Статус

- [ ] Сценарий пройден

---

# Фазовые травмы

## HOI-PHASE-TEMPLATE — шаблон фазового лечения

### ID

HOI-PHASE-TEMPLATE

### Цель

Эталонный цикл: StartTreatment → смена фаз → recovery → `buffHarveyCare`. Конкретные ID и phase buffs — в HOI-PHASE-001…011.

### Параметры кейса

| Поле | Плейсхолдер | Пример |
|------|-------------|--------|
| `<buffId>` | ID травмы | `buffDeepCuts` |
| `<topicId>` | базовый topic | `topicDeepCuts` |
| `<totalPhases>` | 2 или 3 | `3` |
| `<P1>/<P2>/<P3>` | длительности фаз (дней) | `2/3/2` |
| `<phase1Buff>` … | ID phase buffs | см. таблицу ниже |

### Подготовка (StardewMCP)

Общая подготовка (Hospital, 10am, Harvey 3♥).

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add <buffId>
injury_state_dump
injury_buff_dump
injury_phase_list
```

**A — до первого клика (assert baseline):**

- `TreatmentStarted=False`, `CurrentPhase=0`, `TotalPhases=<totalPhases>`
- `ReadyForNextPhase=False`, `ReadyForRecovery=False`
- `injury_buff_dump`: `buff=<buffId> tags=mod,trauma`

**B — StartTreatment (клик по Harvey):**

```
# вручную: клик Harvey  ИЛИ  injury_harvey_click
injury_state_dump
injury_buff_dump
injury_phase_list
```

Ожидается:

- `TreatmentStarted=True`, `CurrentPhase=1`
- base `<buffId>` снят; `buff=<phase1Buff> tags=mod,phase`
- `MainInjuryId=<buffId>` **не меняется**
- `topic=<topicId>` снят или заменён на `topicTreatment<Injury>`

**C — переход фаза 1 → 2:**

```
injury_phase_ready <buffId> 1
# клик Harvey
injury_state_dump
injury_buff_dump
injury_phase_list
```

Ожидается: `CurrentPhase=2`, phase buff = `<phase2Buff>`; `ReadyForNextPhase=False`.

**D — если `<totalPhases>=3`, фаза 2 → 3:**

```
injury_phase_ready <buffId> 1
# клик Harvey
injury_state_dump
injury_buff_dump
```

Ожидается: `CurrentPhase=3`, phase buff = `<phase3Buff>`.

**E — финальное выздоровление:**

```
injury_phase_recovery <buffId> 1
# клик Harvey
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

Ожидается:

- `MainInjuryId=(none)`; DebuffState для `<buffId>` удалён
- phase buffs сняты; `buff=buffHarveyCare tags=mod,cure`
- `Active main injury valid: no`
- В логе: `[MainInjury] Основная травма завершена: <buffId>` или `Механическое выздоровление применено`

### Шаги (кратко)

1. `injury_reset` → `injury_debuff_add <buffId>`.
2. Assert baseline (TreatmentStarted=false, phase 0).
3. Teleport + **клик Harvey** → phase 1.
4. `injury_phase_ready 1` + **клик** → phase 2 (и phase 3, если 3 фазы).
5. `injury_phase_recovery 1` + **клик** → полное выздоровление + `buffHarveyCare`.
6. На каждом шаге: `injury_state_dump` + `injury_buff_dump`.

### Pass criteria (шаблон)

- **PASS:** все флаги и buff tags совпали с таблицей кейса; main stable до recovery; после recovery — только `buffHarveyCare`.
- **FAIL:** смена фазы без `ReadyForNextPhase`; base buff не снят после StartTreatment; recovery без `ReadyForRecovery`; `MainInjuryId` потерян до recovery.

### Статус

- [ ] Шаблон понятен / прогнан на одном эталонном ID

---

## Таблица фазовых кейсов

| ID | buffId | фазы (дней) | P1 buff | P2 buff | P3 buff |
|----|--------|-------------|---------|---------|---------|
| HOI-PHASE-001 | `buffSprainedAnkle` | 2: 3+4 | `HarveyMod_SprainedAnkle_Acute` | `HarveyMod_SprainedAnkle_Recovery` | — |
| HOI-PHASE-002 | `buffBruisedRibs` | 2: 4+5 | `HarveyMod_BruisedRibs_Acute` | `HarveyMod_BruisedRibs_Healing` | — |
| HOI-PHASE-003 | `buffBackStrain` | 2: 2+4 | `HarveyMod_BackStrain_Acute` | `HarveyMod_BackStrain_Recovery` | — |
| HOI-PHASE-004 | `buffDeepCuts` | 3: 2+3+2 | `HarveyMod_DeepCuts_Acute` | `HarveyMod_DeepCuts_Healing` | `HarveyMod_DeepCuts_Recovery` |
| HOI-PHASE-005 | `buffBurnWounds` | 2: 3+5 | `HarveyMod_BurnWounds_Acute` | `HarveyMod_BurnWounds_Healing` | — |
| HOI-PHASE-006 | `buffInfectedWound` | 2: 3+11 | `HarveyMod_InfectedWound_Acute` | `HarveyMod_InfectedWound_Treatment` | — |
| HOI-PHASE-007 | `buffTornMuscles` | 3: 3+5+3 | `HarveyMod_TornMuscles_Acute` | `HarveyMod_TornMuscles_Healing` | `HarveyMod_TornMuscles_Rehab` |
| HOI-PHASE-008 | `buffConcussion` | 3: 2+4+3 | `HarveyMod_Concussion_Acute` | `HarveyMod_Concussion_Rest` | `HarveyMod_Concussion_Limited` |
| HOI-PHASE-009 | `buffFracturedBone` | 3: 4+10+4 | `HarveyMod_FracturedBone_Acute` | `HarveyMod_FracturedBone_Cast` | `HarveyMod_FracturedBone_Recovery` |
| HOI-PHASE-010 | `buffShrapnelWounds` | 3: 3+5+3 | `HarveyMod_Shrapnel_Surgery` | `HarveyMod_Shrapnel_Healing` | `HarveyMod_Shrapnel_Recovery` |
| HOI-PHASE-011 | `buffCold` | 2: 2+2 | `HarveyMod_Cold_Acute` | `HarveyMod_Cold_Recovery` | — |

Каждый HOI-PHASE-00N — **копия шаблона** с подстановкой строк из таблицы. Доп. topics для assert (опционально): `injury_topic_dump` после StartTreatment — `topicTreatment*`, `topic*Phase*`.

---

## HOI-PHASE-001 — buffSprainedAnkle

### ID

HOI-PHASE-001

### Цель

Фазовое лечение 2 фазы (3+4 дн.): acute → recovery → `buffHarveyCare`.

### Команды / шаги

Следовать [HOI-PHASE-TEMPLATE](#hoi-phase-template--шаблон-фазового-лечения) с:

- `<buffId>` = `buffSprainedAnkle`
- `<totalPhases>` = `2`
- phase buffs: `HarveyMod_SprainedAnkle_Acute` → `HarveyMod_SprainedAnkle_Recovery`

### Pass criteria

- **PASS:** как шаблон; 1× `injury_phase_ready` + клик между фазами; recovery → `buffHarveyCare`.
- **FAIL:** любое расхождение phase buff / CurrentPhase.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-002 — buffBruisedRibs

### ID

HOI-PHASE-002

### Цель

2 фазы (4+5 дн.): `HarveyMod_BruisedRibs_Acute` → `HarveyMod_BruisedRibs_Healing`.

### Команды / шаги

Шаблон HOI-PHASE-TEMPLATE, `<buffId>` = `buffBruisedRibs`, 2 фазы.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-003 — buffBackStrain

### ID

HOI-PHASE-003

### Цель

2 фазы (2+4 дн.): `HarveyMod_BackStrain_Acute` → `HarveyMod_BackStrain_Recovery`.

### Команды / шаги

Шаблон, `<buffId>` = `buffBackStrain`.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-004 — buffDeepCuts

### ID

HOI-PHASE-004

### Цель

3 фазы (2+3+2 дн.) — эталон для save/load в [07-smoke-save-tests.md](07-smoke-save-tests.md) HOI-SAVE-001.

### Команды / шаги

Шаблон, `<buffId>` = `buffDeepCuts`, 3 фазы, 2× `injury_phase_ready` + клик.

### Pass criteria

- **PASS:** Acute → Healing → Recovery → `buffHarveyCare`; `MainInjuryId=buffDeepCuts` до recovery.
- **FAIL:** пропуск фазы; orphan base `buffDeepCuts` вместе с phase buff.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-005 — buffBurnWounds

### ID

HOI-PHASE-005

### Цель

2 фазы (3+5 дн.): `HarveyMod_BurnWounds_Acute` → `HarveyMod_BurnWounds_Healing`.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-006 — buffInfectedWound

### ID

HOI-PHASE-006

### Цель

2 фазы (3+11 дн.): `HarveyMod_InfectedWound_Acute` → `HarveyMod_InfectedWound_Treatment`; Critical main.

### Примечание

Длинная фаза 2 — для assert достаточно `injury_phase_ready` / `injury_phase_recovery`, без реального ожидания 11 дней.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-007 — buffTornMuscles

### ID

HOI-PHASE-007

### Цель

3 фазы (3+5+3 дн.): Acute → Healing → Rehab → `buffHarveyCare`.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-008 — buffConcussion

### ID

HOI-PHASE-008

### Цель

3 фазы (2+4+3 дн.): Acute → Rest → Limited.

### Примечание

При `ForceHospitalization` config — возможен hospital lock после debuff_add; для TC лечения использовать `injury_harvey_click` или discharge. Зафиксировать в журнале.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-009 — buffFracturedBone

### ID

HOI-PHASE-009

### Цель

3 фазы (4+10+4 дн.): Acute → Cast → Recovery; severe main + `topicHealthDamageCritical`.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-010 — buffShrapnelWounds

### ID

HOI-PHASE-010

### Цель

3 фазы (3+5+3 дн.): Surgery → Healing → Recovery; `topicPostOperativeCare`.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-011 — buffCold

### ID

HOI-PHASE-011

### Цель

2 фазы (2+2 дн.): `HarveyMod_Cold_Acute` → `HarveyMod_Cold_Recovery` → `buffHarveyCare`; cured bridge `topicColdCured` (CP).

### Статус

- [ ] Сценарий пройден

---

# Регрессии фаз

Негативные и edge-case сценарии для `ReadyForNextPhase` / `ReadyForRecovery` и рассинхрона buff ↔ DebuffState.

---

## HOI-PHASE-REG-001 — ReadyForRecovery на не последней фазе

### ID

HOI-PHASE-REG-001

### Цель

`ReadyForRecovery=true` при `CurrentPhase < TotalPhases` **не** должно завершать лечение при клике Harvey.

### Подготовка (StardewMCP)

Общая подготовка.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click
injury_phase_recovery buffDeepCuts 1
injury_state_dump
injury_buff_dump
# клик Harvey (НЕ должно быть CompleteRecovery)
injury_state_dump
injury_buff_dump
injury_phase_list
```

### Шаги

1. StartTreatment → `CurrentPhase=1`, `TotalPhases=3`.
2. **Без** прохождения фаз 2–3: `injury_phase_recovery buffDeepCuts 1`.
3. Assert до клика: `ReadyForRecovery=True`, `CurrentPhase=1`.
4. **Клик Harvey** (или `injury_harvey_click`).
5. Assert после клика: лечение **не** завершено; `MainInjuryId=buffDeepCuts`; phase buff всё ещё активен; `buffHarveyCare` **отсутствует**.

### Pass criteria

- **PASS:** recovery заблокирован; state/buff без изменений (или только диалог без механики).
- **FAIL:** `MainInjuryId` очищен; появился `buffHarveyCare` на фазе 1/2.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-REG-002 — phase_ready у нефазовой травмы

### ID

HOI-PHASE-REG-002

### Цель

`injury_phase_ready` на simple injury (`TotalPhases=0`) не ломает лечение; флаг сбрасывается санитарной очисткой.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffHurt
injury_harvey_click
injury_phase_ready buffHurt 1
injury_state_dump
# StardewMCP: advance_day
injury_state_dump
injury_phase_list
```

### Шаги

1. Simple treatment started (`buffHarveyTreatment`, `TotalPhases=0`).
2. `injury_phase_ready buffHurt 1` — команда может установить флаг (simple **разрешён** для recovery; для ready — см. [03-existing-debug-commands.md](03-existing-debug-commands.md)).
3. `advance_day` — `CheckSimpleTreatmentCompletion` / санитарный сброс `ReadyForNextPhase` у non-phased.
4. Assert: simple лечение завершается по сроку 2 дня; нет попытки «смены фазы»; нет phase buff.

### Pass criteria

- **PASS:** после 2 дней cure снят, main cleared; нет phase buff; SMAPI без exception.
- **FAIL:** появился phase buff; `CurrentPhase>0` у `buffHurt`; лечение зависло.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-REG-003 — Повторный клик после выздоровления

### ID

HOI-PHASE-REG-003

### Цель

После полного recovery повторный клик Harvey не перезапускает лечение и не дублирует `buffHarveyCare`.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffBackStrain
injury_harvey_click
injury_phase_ready buffBackStrain 1
injury_harvey_click
injury_phase_recovery buffBackStrain 1
injury_harvey_click
injury_buff_dump
injury_state_dump
# второй клик Harvey
injury_harvey_click
injury_buff_dump
injury_state_dump
injury_phase_list
```

### Шаги

1. Пройти полный цикл 2 фаз → recovery (как HOI-PHASE-003).
2. Зафиксировать: один `buffHarveyCare`, `MainInjuryId=(none)`.
3. Повторный `injury_harvey_click` или **ручной клик**.
4. Assert: не появился второй trauma/cure; `ActiveDebuffs.count=0` для main; count `buffHarveyCare` не удвоился (или duration refresh — зафиксировать).

### Pass criteria

- **PASS:** нет нового `buffBackStrain`; нет повторного StartTreatment; не более одного care buff (или документированный refresh).
- **FAIL:** main/truma buff снова на игроке; двойной `buffHarveyCare` с разными источниками.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-REG-004 — Потерянный phase buff при существующем DebuffState

### ID

HOI-PHASE-REG-004

### Цель

При активном лечении (DebuffState `TreatmentStarted=True`, `CurrentPhase=N`) отсутствие phase buff на игроке — регресс restore / valid main.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffFracturedBone
injury_harvey_click
injury_state_dump
injury_buff_dump
# симуляция потери buff: вручную снять debuff через debug / или save-load без snapshot
# StardewMCP: advance_day
injury_state_dump
injury_buff_dump
injury_phase_list
```

### Шаги

1. StartTreatment → phase 1 buff на игроке.
2. **Симулировать потерю:** save/load без buff (B2 из HOI-SAVE-001) **или** дождаться `DayStarted` `[BuffRestore]` после `advance_day`.
3. Assert: либо phase buff **восстановлен** из `SavedActiveBuffs`, либо `injury_phase_list` → `valid: no` / предупреждение в логе.
4. `MainInjuryId` должен оставаться `buffFracturedBone` до явного cure (не silent clear).

### Pass criteria

- **PASS:** restore phase buff после сна **или** явный `valid: no` + документированное поведение; main id сохранён в state.
- **FAIL:** silent потеря лечения; `MainInjuryId` cleared без recovery; двойной base+phase buff.

### Статус

- [ ] Сценарий пройден

---

## HOI-PHASE-REG-005 — DebuffState есть, buff отсутствует

### ID

HOI-PHASE-REG-005

### Цель

Рассинхрон: запись в `ActiveDebuffs`, но ни trauma, ни phase buff не applied — detect через dump.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_state_dump
# снять buff вручную (debug remove) ИЛИ injury_main_set после partial wipe — только если воспроизводимо
injury_buff_dump
injury_validate_buffs
injury_phase_list
```

### Шаги

1. `injury_debuff_add buffDeepCuts` → state и buff согласованы.
2. Удалить buff с игрока **без** `injury_reset` (debug / ванильный способ — зафиксировать).
3. `injury_buff_dump`: нет `buffDeepCuts`; `injury_state_dump`: `ActiveDebuffs.buffDeepCuts` **есть**.
4. `injury_phase_list`: `Active main injury valid: no` (ожидаемо).
5. Опционально: `advance_day` — проверить, восстанавливает ли `[BuffRestore]` trauma buff или требует cleanup.

### Pass criteria

- **PASS:** `valid: no` при orphan state; restore или явный log `[BuffRestore]`; нет crash.
- **FAIL:** `valid: yes` при отсутствии buff; неконсистентный клик Harvey (StartTreatment повторно без ошибки).

### Статус

- [ ] Сценарий пройден

---

## Быстрый прогон (минимум)

| Приоритет | ID | Зачем |
|-----------|-----|-------|
| P0 | HOI-SIMPLE-001 | baseline simple |
| P0 | HOI-PHASE-004 | 3 фазы, эталон main-injury |
| P0 | HOI-PHASE-REG-001 | gate ReadyForRecovery |
| P1 | HOI-SIMPLE-003 | longest simple (7 d) |
| P1 | HOI-PHASE-009 | longest phased |
| P2 | HOI-PHASE-001…011 | полная матрица |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC, правило «ручной клик».
2. **Этот файл** — [08-injury-treatment-tests.md](08-injury-treatment-tests.md) — незакрытые HOI-SIMPLE / HOI-PHASE / REG в журнале прогона.
3. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — таблица **осложнений** (`DirtyWound`, `WetBandage`, `Neglect`, infection escalation).
4. [main-injury-testcases.md](main-injury-testcases.md) — сценарии **4–5** (осложнения + эскалация), **8** (severe/hospital); не дублировать simple/phased treatment из чата 08.
5. [06-debug-setup-commands.md](06-debug-setup-commands.md) — `injury_complication_add`, `injury_test_age_complication` для infection rolls без gameplay.
6. **Следующий артеfact:** `docs/testing/09-complication-tests.md` (или расширение main-injury-testcases) — осложнения, WetBandage eligibility, DirtyWound → Infected, Neglect.
7. [05-debug-dump-commands.md](05-debug-dump-commands.md) — assert после complication setup (`ActiveComplications`, `injury_buff_dump` tags=complication).
