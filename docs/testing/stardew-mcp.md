# StardewMCP — команды для тестирования в Cursor

MCP-сервер **StardewMCP** даёт агенту Cursor прямой доступ к запущенной игре Stardew Valley.

- **Конфиг Cursor:** `~/.cursor/mcp.json` → сервер `stardew`, URL `http://localhost:24842`
- **Идентификатор в CallMcpTool:** `user-stardew`
- **Схемы инструментов:** `mcps/user-stardew/tools/*.json` (читать перед вызовом)

> **Важно:** команды мода Harvey Overhaul (`injury_reset`, `injury_debuff_add`, …) по-прежнему вводятся в **консоль SMAPI**. StardewMCP дополняет их — подготовка мира, телепорт, время, погода, NPC.

---

## Player

| Tool | Назначение |
|------|------------|
| `get_player_info` | Локация, тайл, здоровье, энергия, деньги |
| `get_player_inventory` | Весь инвентарь со stack size |
| `add_item_to_inventory` | Добавить предмет по имени |
| `remove_item_from_inventory` | Убрать предмет по имени |
| `equip_item` | Надеть hat/boots/ring/shirt/pants/trinket; пустое имя — снять слот |
| `set_health` | Текущее HP (1–max; `99999` = полное лечение) |
| `add_money` | Добавить/снять золото (отрицательное значение) |
| `set_speed` | Доп. скорость передвижения |
| `set_skill_level` | Уровень farming/fishing/foraging/mining/combat (0–10) |
| `add_recipe` | Разблокировать рецепт крафта/готовки |
| `add_profession` | Добавить profession по numeric ID |
| `add_walnut` | Золотые орехи (Ginger Island) |
| `upgrade_house` | Уровень дома (0 = starter, 3 = cellar) |
| `toggle_invincible` | Иммунитет к урону on/off |
| `player_emote` | Эмо-бubble (heart, sad, happy, …) |
| `send_hud_message` | Уведомление в HUD |
| `show_speech_bubble` | Речь над NPC или монстром |
| `teleport_player` | Warp в локацию (`location`, опционально `x`, `y`) |

**Локации:** `get_location_names`. Перед координатами — `get_location_warps` (избегать exit-тайлов).

---

## World & Time

| Tool | Назначение |
|------|------------|
| `get_game_time` | Сезон, день, год, время суток |
| `set_time` | Время: `8am`, `2:30pm`, `1800` |
| `set_date` | День, сезон, год |
| `advance_day` | Перейти к началу следующего дня |
| `pause_time` | Поставить/снять паузу игровых часов |
| `get_weather` | Погода сегодня и прогноз |
| `set_weather` | `sunny`, `rain`, `thunderstorm`, `snow`, `windy` |
| `warp_to_mine_floor` | Этаж шахты (121+ = Skull Cavern) |
| `get_location_names` | Все валидные имена локаций |
| `get_location_warps` | Exit-тайлы локации и куда ведут |
| `get_walkable_tiles` | Сетка проходимости вокруг тайла |
| `get_surroundings` | NPC, предметы, машины, культуры, здания рядом |
| `clear_tile` | Убрать дерево/куст/траву/камень/сорняк с тайла |

---

## NPCs & Relationships

| Tool | Назначение |
|------|------------|
| `get_npc_info` | Дружба, расписание, ДР, статус отношений |
| `get_npc_location` | Где NPC сейчас |
| `get_all_npc_locations` | Локации всех жителей |
| `get_npc_gift_preferences` | Loved/liked подарки |
| `get_all_friendships` | Сводка дружбы со всеми |
| `get_upcoming_birthdays` | Дни рождения в ближайшие N дней |
| `get_spouse_info` | Информация о супруге |
| `set_npc_relationship` | Сердечки с NPC (`npc_name`, `hearts`; 1♥ = 250 pts) |

**Harvey:** 3♥ = 750 (First treatment), 8♥ = 2000 (Trauma exam), dating/married — через игровой прогресс или события.

---

## Farm

| Tool | Назначение |
|------|------------|
| `get_farm_info` | Культуры, постройки, животные, машины |
| `grow_crops` | Ускорить культуры в текущей локации на N дней |
| `befriend_animals` | Дружба с животными в локации |

---

## Community Center & Progression

| Tool | Назначение |
|------|------------|
| `get_bundle_status` | Статус всех bundle |
| `complete_community_center` | Завершить все комнаты CC |
| `set_mail_flag` | Добавить/убрать mail flag |
| `manage_quest` | add/complete/remove/clear quest по ID |

---

## Items & World State

| Tool | Назначение |
|------|------------|
| `find_item` | Поиск предмета в сундуках/мебели/на земле |
| `list_items` | Все сохранённые предметы в мире |
| `list_registry_items` | Реестр предметов по типу |

---

## Audio & Effects

| Tool | Назначение |
|------|------------|
| `play_music` | Фон (`spring1`, `rain`, `FlowerDance`; `none` = стоп) |
| `play_sound` | Звук по имени, опционально pitch |
| `play_effect` | Визуальный эффект на тайле: flash, glow, rainbow, sparkle, lightning |

---

## Fishing

| Tool | Назначение |
|------|------------|
| `get_catchable_fish` | Рыба сейчас (сезон, время, погода, уровень) |
| `get_fish_schedule` | Где и когда ловится конкретная рыба |

---

## Monsters

| Tool | Назначение |
|------|------------|
| `spawn_monster` | Спавн монстра (`monster`, `x`, `y`, `count` ≤ 20) |
| `kill_all_monsters` | Убрать всех монстров в текущей локации |

---

## Готовые рецепты (MCP + SMAPI)

### 1. First treatment chain

```
MCP: set_npc_relationship Harvey 3
MCP: teleport_player Hospital
MCP: set_time 10am
SMAPI: injury_reset
SMAPI: injury_debuff_add buffHurt
```

→ войти в Hospital, дождаться `HarveyMod_FirstTreatment`.

### 2. Mine major rescue

```
MCP: set_npc_relationship Harvey 8   # dating через игру или события
SMAPI: injury_debug_mine_rescue
MCP: advance_day
MCP: warp_to_mine_floor 5
```

### 3. Dirty wound в шахте

```
SMAPI: injury_reset
SMAPI: injury_debuff_add buffDeepCuts
MCP: warp_to_mine_floor 10
MCP: pause_time true
# подождать exposure / SMAPI: injury_mine_dirty_debug
```

### 4. Storm comfort roll

```
MCP: set_npc_relationship Harvey 3
MCP: set_weather thunderstorm
MCP: set_time 2pm
# dating + Friendship≥750; roll на DayStarted
```

### 5. Combat injury (natural trigger)

```
MCP: warp_to_mine_floor 20
MCP: toggle_invincible false
MCP: spawn_monster skeleton
# получить урон ≥10 для buffDeepCuts
```

### 6. Town pass-out (2:00)

```
MCP: teleport_player Town
MCP: set_time 2am
# дождаться pass-out pipeline
MCP: advance_day
MCP: teleport_player Farm
MCP: set_time 10pm
```

### 7. Night visit Harvey

```
MCP: set_npc_relationship Harvey 10  # married
SMAPI: injury_debuff_add buffBadlyHurt
MCP: set_time 10pm
MCP: teleport_player FarmHouse
```

---

## Пример вызова из Cursor

```json
{
  "server": "user-stardew",
  "toolName": "teleport_player",
  "arguments": {
    "location": "Hospital",
    "x": 10,
    "y": 15
  }
}
```

---

## Связанные документы

- [`FOR_TEST.md`](FOR_TEST.md) — SMAPI-команды мода, травмы, события
- [`main-injury-testcases.md`](main-injury-testcases.md) — чеклист MainInjury
- [`README.md`](README.md) — индекс тестовой документации
