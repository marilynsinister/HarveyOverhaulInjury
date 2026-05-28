# JSON-сценарии тестирования Harvey Overhaul Injury

Машиночитаемые инструкции для AI-тестировщика в Cursor. Формат **не исполняется** как код — агент читает JSON и выполняет шаги через MCP и консоль SMAPI.

**Схема:** [`schema.harvey-injury-test.schema.json`](schema.harvey-injury-test.schema.json)  
**Индекс наборов:** [`index.json`](index.json)  
**Базовые правила:** [`../00-ai-testing-rules.md`](../00-ai-testing-rules.md)

---

## Зачем нужны JSON-сценарии

Markdown-чеклисты (`main-injury-testcases.md`, `07-smoke-save-tests.md`, …) удобны людям, но плохо структурированы для простых AI-моделей:

- один тест смешан с соседними сценариями;
- шаги и ожидания описаны прозой, а не полями;
- нет явного `cleanup` после каждого кейса.

JSON-сценарии решают это:

| Свойство | Эффект |
|----------|--------|
| Один файл = группа тестов | Логическое покрытие (smoke, MainInjury, complications) |
| Один `tests[]` элемент = одна механика | Агент выполняет **один** тест за раз |
| Явные `steps` и `expected` | Меньше догадок, проще PASS/FAIL |
| `cleanup` после теста | Следующий тест не наследует мусорное состояние |

JSON **не заменяет** markdown-справочники — он ссылается на них через `mechanics`, `failureHints` и документированные команды.

---

## Как AI должен выполнять сценарий

### Перед прогоном

1. Прочитать [`../00-ai-testing-rules.md`](../00-ai-testing-rules.md).
2. Убедиться, что игра запущена с SMAPI и модом HarveyOverhaulInjury.
3. Проверить `requires` в suite: StardewMCP (`user-stardew`, порт 24842), Injury MCP / SMAPI (`user-harvey-injury`, порт 24843).
4. **Перед вызовом MCP** — прочитать схему tool в `mcps/user-stardew/tools/*.json` или [`../injury-mcp.md`](../injury-mcp.md).

### Цикл одного теста

```
1. Прочитать tests[i]: id, preconditions, steps, expected, passCriteria
2. Выполнить steps по порядку
3. Собрать фактическое состояние (injury_phase_list, get_player_info, debug_dump, лог)
4. Сверить с expected и passCriteria → PASS или FAIL
5. Выполнить cleanup (обязательно, даже при FAIL)
6. Записать результат в markdown-чеклист (дата, причина FAIL)
```

**Правило:** не запускать два теста из одного suite параллельно. Один тест — один изолированный прогон.

### Приоритет инструментов для команд мода

| Способ | Когда |
|--------|-------|
| Injury MCP (`user-harvey-injury`) | Предпочтительно из Cursor — ответ содержит актуальный state |
| SMAPI console | Если MCP недоступен или шаг явно помечен как console-only |
| StardewMCP | Только мир, время, NPC, телепорт — **не** команды `injury_*` |

---

## Типы шагов (`steps[].type`)

| type | Назначение | Обязательные поля |
|------|------------|-------------------|
| `smapi_command` | Команда мода `injury_*` | `command`, `description`; опц. `args` |
| `stardewmcp` | Tool StardewMCP | `tool`, `description`; опц. `args` |
| `manual` | Действие игрока вручную | `description` (что именно сделать) |
| `observe` | Только чтение состояния | `description`; часто `tool` + `args` или `command` |
| `wait` | Пауза / ожидание условия | `description`; опц. `args` (`seconds`, `condition`) |
| `note` | Контекст для агента, без действия | `description` |

### Примеры

**SMAPI / Injury MCP:**

```json
{
  "type": "smapi_command",
  "command": "injury_debuff_add",
  "args": { "buff_id": "buffFracturedBone" },
  "description": "Наложить main-травму buffFracturedBone"
}
```

Эквивалент в Injury MCP: `CallMcpTool` → server `user-harvey-injury`, tool `injury_debuff_add`, arguments из `args`.

**StardewMCP:**

```json
{
  "type": "stardewmcp",
  "tool": "teleport_player",
  "args": { "location": "Hospital", "x": 19, "y": 5 },
  "description": "Телепорт в больницу для сценария лечения"
}
```

**Observe:**

```json
{
  "type": "observe",
  "tool": "injury_phase_list",
  "description": "Снять MainInjuryId и список complications"
}
```

**Manual:**

```json
{
  "type": "manual",
  "description": "Кликнуть по Harvey на тайле (3♥+, днём в клинике) для первого лечения"
}
```

---

## Поле `expected`

Описывает **целевое состояние после steps**, не промежуточное. AI сверяет факт с полями; если поле пустое или отсутствует — не проверять.

| Поле | Проверка |
|------|----------|
| `buffsPresent` / `buffsAbsent` | Активные баффы игрока |
| `topicsPresent` / `topicsAbsent` | Conversation topics |
| `state` | Поля мода: `MainInjuryId`, `Phase`, флаги (`valid`, `Complications`, …) — из `injury_phase_list` или `injury_debug_dump` |
| `logsContain` / `logsNotContain` | SMAPI log (подстроки) |
| `hudContains` | Debug HUD F10 или dump |
| `location` | `get_player_info.location` |
| `notes` | Текстовые ожидания, которые агент проверяет вручную |

**Пример:**

```json
"expected": {
  "buffsPresent": ["buffFracturedBone"],
  "state": {
    "MainInjuryId": "buffFracturedBone",
    "valid": true
  },
  "buffsAbsent": ["buffDeepCuts"]
}
```

`passCriteria` дублирует критичные условия человекочитаемыми фразами — при конфликте приоритет у **более строгого** условия в `passCriteria`.

---

## Зачем cleanup после каждого теста

Тесты модифицируют shared state сейва:

- `MainInjuryId`, `DebuffState`, complications;
- topics, cooldowns, флаги mine rescue / hospitalization;
- баффы и phase-buffs на игроке.

Без cleanup следующий тест может:

- получить ложный PASS (осталась нужная травма);
- получить ложный FAIL (блокировка второй main);
- замаскировать регрессию.

**Минимальный cleanup:** `injury_reset`. Для suite с миром — дополнительно телепорт на фarm, снятие дождя и т.д.

Cleanup выполняется **всегда** — после PASS и после FAIL.

---

## Как добавлять новые сценарии

1. Создать файл `docs/testing/scenarios/<suite-id>.json` по схеме.
2. Валидировать JSON (без комментариев, без trailing comma).
3. Добавить запись в [`index.json`](index.json):

```json
{
  "suiteId": "main-injury-smoke",
  "file": "main-injury-smoke.json",
  "title": "MainInjury — smoke",
  "testCount": 3
}
```

4. Один тест = одна механика (`mechanics`: один главный тег).
5. ID теста: `HOI-<AREA>-<NNN>` (например `HOI-MAIN-001`).
6. Связать с markdown-источником в `description` suite или в `failureHints`.

**Чеклист нового теста:**

- [ ] `preconditions` явно перечисляют зависимости (hearts Harvey, локация, …)
- [ ] Первый шаг или cleanup содержит сброс, если тест не продолжение цепочки
- [ ] `expected` и `passCriteria` согласованы
- [ ] `requires.manualClick` suite = true, если есть `type: manual`
- [ ] `failureHints` указывают файлы C# / CP при типичном FAIL

---

## Если StardewMCP не умеет выполнить шаг

StardewMCP **не меняем**. Алгоритм для агента:

1. **Команды мода** (`injury_*`) — только Injury MCP или SMAPI console, не StardewMCP.
2. **Нет нужного tool** — пометить шаг как `manual` или `note`, выполнить вручную по `description`.
3. **Частичная автоматизация** — выполнить доступные шаги, зафиксировать BLOCKED в отчёте, не считать FAIL из-за отсутствия MCP.
4. **Нужна новая QA-ручка** — документировать в [`../04-missing-debug-commands.md`](../04-missing-debug-commands.md), не расширять StardewMCP.
5. **Проверка состояния** — `injury_phase_list`, `injury_debug_dump`, `get_player_info` вместо несуществующего tool.

Пример замены: вместо «симулировать 2 часа в шахте» — `injury_mine_dirty_simulate` (Injury MCP) + `note` о том, что realtime-ожидание не используется.

---

## Если нужен ручной клик по Harvey

Некоторые механики (диалог, cutscene, discharge) не полностью автоматизируются.

1. Пометить шаг `"type": "manual"` с точной инструкцией (локация, время, hearts, что нажать).
2. В suite установить `"requires": { "manualClick": true, ... }`.
3. **Альтернатива:** `injury_harvey_click` (Injury MCP) — когда сценарий про мед. действие без UI cutscene; указать в `args`: `ignore_hospital`, `dry_run` и т.д.
4. После manual-шага — `observe` через `injury_phase_list` или `injury_game_ui_status`.
5. В отчёте явно разделить: **AUTO PASS/FAIL** (MCP) vs **MANUAL VERIFY** (игрок подтвердил).

Не считать FAIL, если тест помечен manual и игрок не выполнил шаг — статус **SKIPPED** или **BLOCKED**.

---

## Связанные документы

| Файл | Назначение |
|------|------------|
| [`../00-ai-testing-rules.md`](../00-ai-testing-rules.md) | Общие правила AI-тестов |
| [`../injury-mcp.md`](../injury-mcp.md) | Injury MCP tools |
| [`../stardew-mcp.md`](../stardew-mcp.md) | StardewMCP tools |
| [`../main-injury-testcases.md`](../main-injury-testcases.md) | Источник сценариев MainInjury |
| [`../TESTING_INDEX.md`](../TESTING_INDEX.md) | Индекс всей тестовой документации |
