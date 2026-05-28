# Правила AI-тестирования Harvey Overhaul Injury

**Базовый документ.** Каждый тестовый файл в `docs/testing/` начинается со ссылки на этот файл.

**Моды:** C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`  
**Справочники:** [`stardew-mcp.md`](stardew-mcp.md) · [`FOR_TEST.md`](FOR_TEST.md) · [`injury-mcp.md`](injury-mcp.md)

---

## 1. Цель тестирования

Максимально проверить **HarveyOverhaulInjury (C#)** через AI-агента в Cursor при запущенной игре.

**Области покрытия:**

| Область | Что проверяем |
|---------|----------------|
| C# state | `MainInjuryId`, `DebuffState`, флаги, save/load |
| Баффы | наложение, приоритет, upgrade, cure-buff |
| Топики | `topic*`, `situation*`, мосты к CP |
| CP-события | cutscene, preconditions, `eventsSeen` |
| Госпитализация | Severe, Hospital warp, night crisis |
| Осложнения | DirtyWound, WetBandage, Neglect, Infected |
| Шахта | dirty exposure, mine rescue, mine forbidden |
| Обмороки | pass-out, Town 2:00, emergency care, exhaustion |

**Не цель:** менять StardewMCP, ваниль или делать рефакторинг «по ходу теста».

---

## 2. Ограничения

| Правило | Деталь |
|---------|--------|
| **StardewMCP** | Не менять код/конфиг StardewMCP. Только вызывать tools как внешний клиент (`user-stardew`, порт 24842). |
| **Ванильная игра** | Не патчить Stardew Valley / SMAPI core. |
| **Debug-команды** | Новые QA-ручки — **только** в `HarveyOverhaulInjury` (SMAPI console + при необходимости Injury MCP). |
| **Рефакторинг** | Во время прогона тестов — без крупных перестроек; фиксы — минимальный diff под конкретный FAIL. |
| **Content Patcher** | CP правим только если тест доказал баг в событии/тексте; иначе — сценарий в md. |

**Injury MCP** (`user-harvey-injury`, порт 24843) — зеркало SMAPI `injury_*` для автоматизации; те же ограничения: сервер в C# моде, не в StardewMCP.

---

## 3. Как использовать StardewMCP

**Сервер:** `user-stardew` · **URL:** `http://localhost:24842`  
**Перед вызовом:** читать схему tool в `mcps/user-stardew/tools/*.json`.

### Игрок

- `get_player_info` — локация, тайл, HP, энергия, деньги
- `get_player_inventory` / `add_item_to_inventory` / `remove_item_from_inventory`
- `teleport_player` — warp (`location`, опц. `x`, `y`)
- `set_health` — текущее HP (1–max; `99999` = полное)
- `toggle_invincible`, `set_speed`, `equip_item`, `add_money`, …

### Время и мир

- `get_game_time` — сезон, день, год, время суток
- `set_time` — `8am`, `2:30pm`, `1800`
- `set_date` — день / сезон / год
- `advance_day` — следующий день
- `pause_time` — пауза игровых часов
- `get_weather` / `set_weather` — `sunny`, `rain`, `thunderstorm`, …
- `warp_to_mine_floor` — этаж шахты (121+ = Skull Cavern)

### NPC и окружение

- `get_npc_info` / `get_npc_location` / `get_all_npc_locations`
- `set_npc_relationship` — сердечки (`Harvey`, `3` = 750 pts)
- `get_surroundings` — NPC, предметы, постройки рядом
- `get_location_names` / `get_location_warps` / `get_walkable_tiles`

### Бой / шахта (при необходимости)

- `spawn_monster` / `kill_all_monsters`

**Полный список:** [`stardew-mcp.md`](stardew-mcp.md).

**Типичная подготовка сценария:**

```
StardewMCP: teleport_player Hospital
StardewMCP: set_time 10am
StardewMCP: set_npc_relationship Harvey 3
StardewMCP: warp_to_mine_floor 10
```

Команды мода (`injury_*`) — **не** через StardewMCP.

---

## 4. Как использовать SMAPI console

Консоль SMAPI (клавиша `` ` `` / `\` по умолчанию). Команды мода — префикс `injury_`.

| Команда | Назначение |
|---------|------------|
| `injury_reset` | Полный сброс мода перед изолированным сценарием |
| `injury_debuff_list` | Список ID травм и осложнений |
| `injury_debuff_add <id> [минуты]` | Наложить buff (+ state + topic) |
| `injury_phase_list` | Активные травмы: фаза, флаги, **MainInjuryId** |
| `injury_phase_ready <buffId> [1\|0]` | «Можно сменить фазу» |
| `injury_phase_recovery <buffId> [1\|0]` | «Можно завершить лечение» |
| `injury_phase_advance <buffId>` | Принудительная смена фазы |
| `injury_phase_cure <buffId>` | Полное выздоровление без клика |

**Доп. QA (см. [`FOR_TEST.md`](FOR_TEST.md)):**

- `injury_rain_debug`, `injury_mine_dirty_debug`, `injury_debug_mine_rescue`
- `injury_cooldowns`, `injury_night_visit_reset`, `injury_audit_content`
- будущие `injury_*` — документировать здесь и в FOR_TEST

**Через Cursor:** те же команды через **Injury MCP** (`user-harvey-injury`) — см. [`injury-mcp.md`](injury-mcp.md).

**Цикл:**

```
injury_reset
injury_debuff_add buffDeepCuts
injury_phase_list
```

---

## 5. Формат каждого тест-кейса

Каждый сценарий — отдельный блок или файл. Шаблон:

```markdown
## TC-XXX: Краткое название

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)

### ID
TC-XXX

### Цель
Одно предложение: что доказываем.

### Подготовка (StardewMCP)
- teleport_player …
- set_time …
- set_npc_relationship Harvey N
- (опц.) warp_to_mine_floor / set_weather

### Команды SMAPI
```
injury_reset
injury_debuff_add …
```

### Шаги в игре
1. …
2. Клик по Harvey / войти в локацию / сон — только если MCP не покрывает.

### Ожидаемый результат
- MainInjuryId / buff / topic / mail / event ID

### Debug HUD (F10)
- строки: активные травмы, complications, LastClickDebug, …

### SMAPI log
- теги: `[MineRescue]`, `[PassOutEvent]`, `[FarmingInjury]`, …

### Критерий прохождения
Чёткое PASS/FAIL (что должно совпасть 1:1).

### Статус
- [ ] Сценарий пройден
```

**Правило:** если шаг нельзя автоматизировать (save/load, cutscene skip) — явно пометить «вручную» и не считать FAIL из-за отсутствия MCP.

---

## 6. Правило сохранения контекста между чатами

Каждый чат Cursor **обязан:**

1. **Читать** уже созданные файлы в `docs/testing/` (сначала этот файл, затем целевой чеклист).
2. **Не держать** полный результат только в ответе чата — итог в **отдельном `.md`** (обновить чеклист или создать `NN-<topic>-results.md`).
3. **Фиксировать** PASS/FAIL: галочка `[x]`, дата, краткая причина FAIL.
4. **В конце чата** — блок «Что читать следующему чату» (3–7 пунктов: файлы, незакрытые TC, блокеры).

**Шаблон финала чата:**

```markdown
## Что читать следующему чату
- docs/testing/00-ai-testing-rules.md
- docs/testing/<активный-чеклист>.md — TC-003 FAIL: …
- docs/testing/stardew-mcp.md — рецепт mine rescue
- Блокер: нужен save/load вручную для TC-009
```

---

## Быстрый цикл AI-прогона

```
1. injury_reset          (SMAPI / Injury MCP)
2. StardewMCP            подготовка мира
3. injury_debuff_add / injury_phase_* 
4. Проверка ответа MCP или injury_phase_list
5. F10 + SMAPI log       если сценарий требует UI/событие
6. Записать результат в md + галочка
```

**Ручные шаги** (только когда в TC указано): клик Harvey, просмотр cutscene, перезагрузка сейва.

---

## Связанные документы

| Файл | Когда |
|------|-------|
| [`README.md`](README.md) | Индекс всей тестовой папки |
| [`FOR_TEST.md`](FOR_TEST.md) | Справочник травм, событий, HUD |
| [`main-injury-testcases.md`](main-injury-testcases.md) | MainInjury + complications |
| [`stardew-mcp.md`](stardew-mcp.md) | Все tools StardewMCP |
| [`injury-mcp.md`](injury-mcp.md) | Автовызов injury_* из Cursor |
