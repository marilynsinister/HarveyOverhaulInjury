# Smoke, reset, save/load — чеклист QA

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Механики C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) · CP: [02-cp-content-inventory.md](02-cp-content-inventory.md)  
> Assert-команды: [05-debug-dump-commands.md](05-debug-dump-commands.md) · Setup: [06-debug-setup-commands.md](06-debug-setup-commands.md)  
> MCP: [stardew-mcp.md](stardew-mcp.md) · [injury-mcp.md](injury-mcp.md)

**Область:** запуск мода, связь StardewMCP, базовые `injury_*`, персистентность `InjuryState` и buff-restore после сна / save-load.  
**Не цель:** фазовые сценарии MainInjury, осложнения в глубину — см. `main-injury-testcases.md` и следующий чат (простые/фазовые травмы).

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
| HOI-SMOKE-001 | Запуск игры | [ ] | |
| HOI-SMOKE-002 | StardewMCP связь | [ ] | |
| HOI-CMD-001 | injury_reset | [ ] | |
| HOI-CMD-002 | injury_debuff_list | [ ] | |
| HOI-CMD-003 | injury_validate_buffs | [ ] | |
| HOI-SAVE-001 | Сохранение активной травмы (лечение + phase buff) | [ ] | |
| HOI-SAVE-002 | Сохранение осложнения | [ ] | |
| HOI-SAVE-003 | reset после save/load | [ ] | |

### Предусловия (все TC кроме SMOKE-001)

- [ ] Игра запущена через SMAPI, загружен тестовый сейв (`Context.IsWorldReady`)
- [ ] C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]` в папке Mods
- [ ] Опционально: Injury MCP `EnableInjuryMcp: true` → `[InjuryMCP] listening on http://localhost:24843`
- [ ] StardewMCP на `http://localhost:24842` (для SMOKE-002 и подготовки SAVE)

---

## HOI-SMOKE-001 — Запуск игры

### ID

HOI-SMOKE-001

### Цель

Убедиться, что при старте SMAPI нет фатальных ошибок мода, C# и CP-пак подключены, `Data/Buffs` доступны для `BuffManager`.

### Подготовка (StardewMCP)

Не требуется (проверка до или сразу после загрузки title → сейв).

### Команды SMAPI

Нет (только чтение лога при запуске). После загрузки сейва — опционально:

```
injury_validate_buffs
```

### Шаги

1. Закрыть игру (если была открыта).
2. Запустить Stardew Valley через **SMAPI**.
3. В окне/файле SMAPI-лога найти секцию загрузки модов.
4. Загрузить любой сейв (новый или тестовый).
5. В логе найти строки мода `Harvey Overhaul Injury` / `HarveyOverhaulInjury`.
6. Убедиться, что **Content Patcher** загрузил пак `HarveyOverhaul [CP]` без ошибок патча `Data/Buffs`.
7. После `SaveLoaded` — опционально `injury_validate_buffs` в консоли SMAPI.

### Ожидаемый результат

- В списке SMAPI **нет** строки `Error` / `Failed` для `HarveyOverhaulInjury`.
- Мод отображается как **loaded** (зелёный / без предупреждений о missing dependency).
- CP: пак `HarveyOverhaul [CP]` в логе Content Patcher без failed patches на `Data/Buffs`, `Data/Events`, …
- После загрузки сейва в логе C# мода: `[Buffs] Загружено N записей из Data/Buffs.` (N > 0).
- `injury_validate_buffs` (если вызван): `result=OK` (см. HOI-CMD-003).

### Log markers

| Маркер | Где | Значение |
|--------|-----|----------|
| SMAPI `loaded […] HarveyOverhaulInjury` | SMAPI console | C# мод встал |
| `[InjuryMCP] listening on http://localhost:24843` | SMAPI | MCP включён (опционально) |
| Content Patcher `HarveyOverhaul [CP]` | SMAPI / CP log | CP pack OK |
| `[Buffs] Загружено` | SMAPI | Data/Buffs прочитан |
| `[Buffs] Ошибка загрузки Data/Buffs` | SMAPI | **FAIL** — CP или порядок загрузки |
| `Состояние загружено из сохранения` | SMAPI | SaveLoaded прошёл |

### Pass criteria

- **PASS:** нет SMAPI Error по Injury/CP; `[Buffs] Загружено` без Error; при `injury_validate_buffs` — `result=OK`.
- **FAIL:** любой Error при старте мода/CP; отсутствует `[Buffs] Загружено`; `injury_validate_buffs` → `result=MISSING`.

### Статус

- [ ] Сценарий пройден

---

## HOI-SMOKE-002 — StardewMCP связь

### ID

HOI-SMOKE-002

### Цель

Проверить, что StardewMCP отвечает на базовые read-only tools и мир игры доступен агенту (порт 24842).

### Подготовка (StardewMCP)

Загруженный сейв, игра не на паузе в меню title.

| # | Tool | Аргументы (пример) |
|---|------|-------------------|
| 1 | `get_player_info` | — |
| 2 | `get_game_time` | — |
| 3 | `get_weather` | — |
| 4 | `get_npc_info` | `npc_name`: `Harvey` |
| 5 | `get_npc_location` | `npc_name`: `Harvey` |

*(Схемы: `mcps/user-stardew/tools/*.json`.)*

### Команды SMAPI

Не требуются.

### Шаги

1. Убедиться, что игра запущена с подключённым StardewMCP.
2. Вызвать пять tools по порядку (Cursor: `CallMcpTool` → `user-stardew`).
3. Проверить, что каждый ответ — JSON/текст без HTTP/connection error.
4. Сверить: локация игрока — валидное имя; время — сезон/день/часы; Harvey — существует в ответе NPC.

### Ожидаемый результат

| Tool | Минимум в ответе |
|------|------------------|
| `get_player_info` | `location`, HP/energy, координаты или tile |
| `get_game_time` | season, day, year, time of day |
| `get_weather` | сегодняшняя погода (например `Sunny`) |
| `get_npc_info` | Harvey: friendship / hearts / relationship |
| `get_npc_location` | Harvey: `location` (или явное «не найден» только если NPC недоступен по сюжету) |

### Log markers

StardewMCP не пишет в SMAPI-лог мода. Ошибки — только в ответе MCP (`connection refused`, `timeout`).

### Pass criteria

- **PASS:** все 5 вызовов успешны; `get_player_info` и `get_game_time` содержат осмысленные поля текущего сейва.
- **FAIL:** любой tool не отвечает или connection error (игра не запущена / неверный порт).

### Статус

- [ ] Сценарий пройден

---

## HOI-CMD-001 — injury_reset

### ID

HOI-CMD-001

### Цель

Проверить полный сброс мода: после активной main + осложнения `injury_reset` очищает state, баффы и owned topics.

### Подготовка (StardewMCP)

| Tool | Назначение |
|------|------------|
| `get_player_info` | Baseline локации (опционально) |
| `teleport_player` | `Hospital` — если следующий TC требует клинику |

Для этого TC достаточно загруженного сейва.

### Команды SMAPI

```
injury_reset
injury_debuff_add buffDeepCuts
injury_complication_add HarveyMod_DirtyWound
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_reset
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

*(Injury MCP: те же имена tools на `user-harvey-injury`.)*

### Шаги

1. `injury_reset` — чистый старт.
2. `injury_debuff_add buffDeepCuts` — main травма.
3. `injury_complication_add HarveyMod_DirtyWound` — осложнение (main ∈ `DirtyInMines`; при SKIP см. [06-debug-setup-commands.md](06-debug-setup-commands.md) — сначала main, затем complication).
4. **До reset:** три dump + `injury_phase_list` — зафиксировать `MainInjuryId`, complications, applied buffs, owned topics.
5. `injury_reset`.
6. **После reset:** те же dump + `injury_phase_list`.
7. F10 (опционально): нет активной main, нет complications.

### Ожидаемый результат

**До reset:**

- `injury_state_dump`: `MainInjuryId=buffDeepCuts`, `ActiveDebuffs.count≥1`, `ActiveComplications.count≥1` (DirtyWound).
- `injury_buff_dump`: `buffDeepCuts` tags=`mod,trauma`; `HarveyMod_DirtyWound` tags=`mod,complication`.
- `injury_topic_dump`: owned topics для травмы/осложнения (например `topicDeepCuts`, `topicHarvey_DirtyWound`).
- `injury_phase_list`: `MainInjuryId: buffDeepCuts`, complications перечислены.

**После reset:**

- `MainInjuryId=(none)` / пусто; `ActiveDebuffs.count=0`; `ActiveComplications.count=0`.
- `injury_buff_dump`: `count=0` или только vanilla; нет `buffDeepCuts`, нет `HarveyMod_DirtyWound`.
- `injury_topic_dump`: owned count=0 (секция `owned` пуста).
- `injury_phase_list`: `MainInjuryId: (none)`, `valid: no` или нет активных травм.

### Log markers

| Маркер | Когда |
|--------|-------|
| `=== ПОЛНЫЙ СБРОС ВЫПОЛНЕН ===` | После каждого `injury_reset` |
| `Удалено N топиков InjuryCare` | reset |
| `[QA] injury_state_dump` | dump до/после |
| `Нельзя сбросить данные до загрузки сохранения` | **FAIL** — сейв не загружен |

### Pass criteria

- **PASS:** после второго `injury_reset` все три dump и `injury_phase_list` согласованы с «пустым» модом; до reset — main + complication присутствуют.
- **FAIL:** после reset остались mod trauma/complication buffs, owned topics или `MainInjuryId≠(none)`.

### Статус

- [ ] Сценарий пройден

---

## HOI-CMD-002 — injury_debuff_list

### ID

HOI-CMD-002

### Цель

Read-only список ID травм и осложнений совпадает с `KnownTraumas` / `KnownComplications` и пригоден для `injury_debuff_add`.

### Подготовка (StardewMCP)

Не требуется.

### Команды SMAPI

```
injury_debuff_list
```

### Шаги

1. Вызвать `injury_debuff_list` в SMAPI-консоли (или Injury MCP `injury_debuff_list`).
2. Найти в выводе обязательные ID: `buffDeepCuts`, `buffFracturedBone`, `buffHurt`, `HarveyMod_DirtyWound`, `HarveyMod_WetBandage`.
3. Для одной фазовой травмы проверить строки фаз (P1/P2/P3 durations) — например `buffDeepCuts`.
4. Убедиться, что команда **не меняет** состояние: `injury_state_dump` до и после идентичен (при пустом сейве после `injury_reset`).

### Ожидаемый результат

- Секция травм содержит все ID из [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) (таблица «Все травмы»).
- Секция осложнений содержит `HarveyMod_*` из таблицы «Все осложнения».
- Нет исключения в SMAPI; state не изменился.

### Log markers

Вывод только в консоль (без обязательного `[QA]`). Ошибок SMAPI быть не должно.

### Pass criteria

- **PASS:** список непустой; перечисленные ID найдены; `injury_state_dump` не изменился от одного вызова list.
- **FAIL:** пустой список, SMAPI exception, или отсутствует ключевой ID из реестра C#.

### Статус

- [ ] Сценарий пройден

---

## HOI-CMD-003 — injury_validate_buffs

### ID

HOI-CMD-003

### Цель

Pre-release gate: все C# buff ID (trauma, complication, phase, cure) существуют в `Data/Buffs` после патчей CP.

### Подготовка (StardewMCP)

Загруженный сейв (нужен доступ к `GameContent.Load` для `Data/Buffs`).

### Команды SMAPI

```
injury_validate_buffs
```

### Шаги

1. После HOI-SMOKE-001 (сейв загружен) вызвать `injury_validate_buffs`.
2. Прочитать итоговую строку `result=OK` или `result=MISSING`.
3. При `MISSING` — записать `ids=…` в журнал прогона (регресс CP↔C#).

### Ожидаемый результат

```
[QA] injury_validate_buffs: OK
result=OK checked=N
```

`N` — число проверенных ID (может расти с контентом; критично `result=OK`).

### Log markers

| Маркер | Значение |
|--------|----------|
| `[QA] injury_validate_buffs: OK` | PASS |
| `result=MISSING missing_count=` | FAIL |
| `ids=buffTooCold` (пример) | какой ID чинить в CP |

### Pass criteria

- **PASS:** `result=OK`, `missing_count=0` (или отсутствует строка MISSING).
- **FAIL:** любой `MISSING` id; exception при вызове до загрузки сейва.

### Статус

- [ ] Сценарий пройден

---

## HOI-SAVE-001 — Сохранение активной травмы (лечение + phase buff)

### ID

HOI-SAVE-001

### Цель

Проверить персистентность фазового лечения: после начала лечения `buffDeepCuts` активен phase buff; после сна / save-load state и восстановление баффов согласованы.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `Hospital` (или клиника, где доступен Harvey) |
| `set_time` | `10am` |
| `set_npc_relationship` | `Harvey`, `3` (если клик/диалог требует сердечки) |
| `pause_time` | `true` — опционально, стабильный прогон |

### Команды SMAPI

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click
injury_state_dump
injury_buff_dump
injury_phase_list
```

После сна или save-load:

```
injury_state_dump
injury_buff_dump
injury_phase_list
```

### Шаги

**A — подготовка лечения**

1. `injury_reset`.
2. `injury_debuff_add buffDeepCuts`.
3. `injury_harvey_click` — StartTreatment (снятие base buff, phase 1).
4. Зафиксировать dump: `TreatmentStarted=True`, `CurrentPhase=1`, phase buff `HarveyMod_DeepCuts_Acute`.

**B — персистентность (выберите один или оба пути)**

| Путь | Действие | Что тестирует |
|------|----------|----------------|
| **B1** | StardewMCP `advance_day` | `DayEnding` snapshot → `DayStarted` `[BuffRestore]` |
| **B2** | **Вручную:** сохранить слот → title → загрузить слот | `InjuryState` в файле сейва + `SaveLoaded` |

5. После B1/B2 дождаться утра (`DayStarted` отработал).
6. Повторить dump + `injury_phase_list`.

### Ожидаемый результат

**Сразу после лечения (шаг 4):**

- `injury_buff_dump`: `buff=HarveyMod_DeepCuts_Acute tags=mod,phase` (base `buffDeepCuts` снят).
- `injury_state_dump`: `MainInjuryId=buffDeepCuts`, `ActiveDebuffs.buffDeepCuts.TreatmentStarted=True`, `CurrentPhase=1`.
- `SavedActiveBuffs` содержит phase id (после конца дня — в snapshot).

**После B1 (`advance_day`) или B2 (save/load):**

- `MainInjuryId=buffDeepCuts`, `Active main injury valid: yes`.
- На игроке снова **phase buff** `HarveyMod_DeepCuts_Acute` (или восстановлен по snapshot без дубля base+phase).
- `DebuffState`: фаза 1, лечение начато; нет «потери» main.
- Нет двойного наложения conflicting trauma buffs.

### Log markers

| Маркер | Когда |
|--------|-------|
| `Снапшот баффов` | `OnDayEnding` — перед сном |
| `[BuffRestore] restored` | `DayStarted` — успех restore |
| `[BuffRestore] skip invalid` | stale buff не восстановлен (**ожидаемо** для невалидных, не для phase при валидном лечении) |
| `Состояние загружено из сохранения` | B2 save/load |
| `Состояние сохранено` | autosave при выходе |

### Pass criteria

- **PASS:** после сна/load `MainInjuryId=buffDeepCuts`, `TreatmentStarted=True`, phase buff на игроке, `injury_phase_list` valid=yes.
- **FAIL:** main пустой; лечение сброшено в `CurrentPhase=0` без причины; только base buff без phase после лечения; дублирующие trauma buffs.

### Статус

- [ ] Сценарий пройден

---

## HOI-SAVE-002 — Сохранение осложнения

### ID

HOI-SAVE-002

### Цель

Проверить, что `ActiveComplications` и buff осложнения переживают сон / save-load вместе с main.

### Подготовка (StardewMCP)

| Tool | Назначение |
|------|------------|
| `advance_day` | Следующий день после snapshot |
| `get_game_time` | Зафиксировать день до/после |

### Команды SMAPI

```
injury_reset
injury_debuff_add buffDeepCuts
injury_complication_add HarveyMod_DirtyWound
injury_state_dump
injury_buff_dump
```

После `advance_day` или save/load:

```
injury_state_dump
injury_buff_dump
injury_phase_list
```

### Шаги

1. `injury_reset`.
2. `injury_debuff_add buffDeepCuts` — main для eligibility DirtyWound.
3. `injury_complication_add HarveyMod_DirtyWound` — при `SKIP` проверить лог и main (см. [06-debug-setup-commands.md](06-debug-setup-commands.md)).
4. Dump: `ActiveComplications.count≥1`, ключ `HarveyMod_DirtyWound`; buff dump — `tags=mod,complication`.
5. **B1:** `advance_day` **или** **B2 (вручную):** save → reload.
6. Повторить dump + `injury_phase_list`.

### Ожидаемый результат

- `MainInjuryId=buffDeepCuts` сохранён.
- `ActiveComplications` содержит `HarveyMod_DirtyWound` (дата старта может совпадать с `InjuryStartDay` осложнения в dump).
- Buff `HarveyMod_DirtyWound` на игроке после restore (если был в `SavedActiveBuffs` и прошёл `ShouldRestoreComplicationBuff`).
- Topic `topicHarvey_DirtyWound` в `injury_topic_dump` (owned), если не истёк.

### Log markers

| Маркер | Значение |
|--------|----------|
| `[QA] injury_complication_add complication=HarveyMod_DirtyWound ok=yes` | Успешное наложение |
| `[QA] injury_complication_add SKIP` | **FAIL** подготовки — исправить main/eligibility |
| `[Complication] MainInjury=buffDeepCuts, complication=HarveyMod_DirtyWound` | Применение (если есть в логе) |
| `[BuffRestore] restored` | complication восстановлен с snapshot |

### Pass criteria

- **PASS:** после B1/B2 `ActiveComplications.count≥1` с DirtyWound; main не сброшен; buff или state согласованы (buff на игроке или явный restore в логе).
- **FAIL:** complication исчез из state без `injury_reset`; main потерян; SKIP на шаге 3 не устранён.

### Статус

- [ ] Сценарий пройден

---

## HOI-SAVE-003 — reset после save/load

### ID

HOI-SAVE-003

### Цель

Убедиться, что `injury_reset` работает **после** перезагрузки сейва с активной травмой (не только в той же сессии).

### Подготовка (StardewMCP)

Как HOI-SAVE-001/002: подготовить сейв с активной травмой, затем save/load.

| Tool | Когда |
|------|-------|
| `advance_day` | Альтернатива полному reload для накопления snapshot (опционально) |

### Команды SMAPI

```
injury_reset
injury_debuff_add buffFracturedBone
injury_state_dump
```

**Вручную:** сохранить → выйти в title → загрузить слот.

```
injury_state_dump
injury_phase_list
injury_reset
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Шаги

1. `injury_reset` → `injury_debuff_add buffFracturedBone` → dump (main установлена).
2. **Вручную:** сохранить игру, выйти в title screen, загрузить тот же слот.
3. Без дополнительных команд: `injury_state_dump` — `MainInjuryId=buffFracturedBone` (персистентность).
4. `injury_reset`.
5. Dump + `injury_phase_list` — полная очистка как в HOI-CMD-001.

### Ожидаемый результат

**После load (шаг 3):**

- State восстановлен из файла сейва: main = `buffFracturedBone`.

**После `injury_reset` (шаг 5):**

- Как HOI-CMD-001 после reset: пустой mod state, нет owned topics, нет trauma buffs.

### Log markers

| Маркер | Когда |
|--------|-------|
| `Состояние загружено из сохранения` | load |
| `=== ПОЛНЫЙ СБРОС ВЫПОЛНЕН ===` | reset после load |
| `[PassOut] … resume` | Информационно; не FAIL для этого TC |

### Pass criteria

- **PASS:** main сохранилась после load; после `injury_reset` — полный сброс (идентично HOI-CMD-001).
- **FAIL:** reset после load не очищает state/buffs/topics; main не восстановилась после load.

### Статус

- [ ] Сценарий пройден

---

## Быстрый smoke (минимум)

Если времени мало: **SMOKE-001 → SMOKE-002 → CMD-003 → CMD-001 → SAVE-003** (B2 save/load для SAVE-003 обязателен).

| ID | Критичный результат |
|----|---------------------|
| HOI-SMOKE-001 | Нет Error, `[Buffs] Загружено` |
| HOI-SMOKE-002 | 5 StardewMCP tools OK |
| HOI-CMD-003 | `result=OK` |
| HOI-CMD-001 | reset очищает main+complication |
| HOI-SAVE-003 | reset после reload сейва |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI и формат TC.
2. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — **простые травмы** (`buffHurt`, `buffBadlyHurt`, `buffSurgicalWound`): simple treatment, cure buffs, сроки 2/4/7 дней.
3. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — **фазовые травмы**: `injury_phase_*`, `injury_harvey_click`, phase buff IDs, `CheckInjuryPhases`.
4. [main-injury-testcases.md](main-injury-testcases.md) — сценарии **6** (фазовое лечение), **7** (выздоровление); не дублировать save/load (**9**) — уже в этом файле.
5. [05-debug-dump-commands.md](05-debug-dump-commands.md) + [06-debug-setup-commands.md](06-debug-setup-commands.md) — assert/setup для TC простых и фазовых травм.
6. **Следующий артеfact:** `docs/testing/08-simple-injury-testcases.md` и/или `docs/testing/09-phased-injury-testcases.md` (создать по аналогии с этим чеклистом).
7. [injury-mcp.md](injury-mcp.md) — автопрогон `injury_harvey_click`, `injury_phase_advance` для фазовых TC.
