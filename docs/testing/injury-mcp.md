# Injury MCP — команды HarveyOverhaulInjury для Cursor

Отдельный MCP-сервер **внутри C# мода** — зеркало консольных команд `injury_*` для автоматического тестирования (MainInjury, осложнения, фазы).

- **Порт:** `24843` (StardewMCP — `24842`)
- **Cursor config:** `~/.cursor/mcp.json` → сервер `harvey-injury`
- **CallMcpTool server id:** `user-harvey-injury` (после Reload MCP)
- **Включение:** `EnableInjuryMcp: true` в `config.json` мода (по умолчанию включено)

> Перезапустите игру после обновления DLL. В SMAPI-логе должно быть: `[InjuryMCP] listening on http://localhost:24843`

---

## Инструменты (25+)

| Tool | Назначение |
|------|------------|
| `injury_reset` | Полный сброс мода |
| `injury_debuff_list` | Список ID травм и осложнений |
| `injury_debuff_add` | Наложить buff (`buff_id`, `force?`, `minutes?`) |
| `injury_phase_list` | **MainInjuryId**, фазы, complications (основная проверка) |
| `injury_debug_dump` | Полный debug-отчёт (F10 full) |
| `injury_main_clear` | Очистить MainInjuryId |
| `injury_main_set` | Установить MainInjuryId (`buff_id`) |
| `injury_phase_ready` | Флаг готовности к смене фазы |
| `injury_phase_recovery` | Флаг готовности к выздоровлению |
| `injury_phase_advance` | Принудительная смена фазы |
| `injury_phase_cure` | Полное выздоровление |
| `injury_harvey_click` | Мед. действие как клик по Харви. `ignore_hospital`, `discharge_if_hospitalized`, `dry_run` |
| `injury_run_daily_checks` | Утренние проверки сразу (infection roll после `advance_day`) |
| `injury_mine_dirty_debug` | Риск грязной раны в шахте (read-only) |
| `injury_mine_dirty_simulate` | Симуляция exposure в шахте: `minutes`, `force_roll` |
| `injury_main_migrate` | Миграция MainInjuryId после `injury_main_clear` |
| `injury_neglect_set` | `NeglectStrikesByInjury` для теста сценария 11 |
| `injury_game_ui_status` | Статус event/dialogue/menu |
| `injury_game_ui_advance` | Шаг cutscene/диалога (`steps`) |
| `injury_game_ui_end_event` | Принудительно завершить event |
| `injury_game_ui_close_menu` | Закрыть dialogue/menu |
| `injury_rain_debug` | Счётчики дождя / wet bandage |
| `injury_debug_mine_rescue` | Флаги mine rescue |
| `injury_cleanup_invalid_complications` | Очистка stale осложнений |
| `injury_foreign_topic_add` | Чужой topic для тестов |

После **мутаций** (`debuff_add`, `reset`, `phase_*`) ответ = актуальный `injury_phase_list`.

---

## MainInjury testcases — что автоматизируется

| # | Сценарий | Injury MCP | StardewMCP | Вручную |
|---|----------|:----------:|:----------:|---------|
| 1 | Базовое наложение main | ✅ | — | — |
| 2 | Блокировка второй main | ✅ | — | — |
| 3 | Upgrade лёгкой → тяжёлой | ✅ | — | — |
| 3b | Upgrade заблокирован в лечении | ✅ + `injury_harvey_click` | — | — |
| 4–4c | DirtyWound | ✅ | warp mine, pause | время в шахте |
| 4b | DirtyWound при фазовом лечении | ✅ + `injury_harvey_click` | warp mine | время в шахте |
| 5 | Dirty → Infected | ✅ | `injury_test_age_complication` + `injury_run_daily_checks` | — |
| 5b–5d | WetBandage | ✅ + `injury_harvey_click` (5d) | rain, location | — |
| 6 | Фазовое лечение | ✅ `injury_harvey_click` / phase_* | Hospital | — |
| 7 | Выздоровление | ✅ | — | клик / phase_cure |
| 8a–8b | Severe | ✅ | mine, NPC | dating |
| 9 | Save/load | частично | — | **перезагрузка сейва** |
| 10 | Миграция | ❌ | — | старый сейв |
| 11 | NeglectStrikes | ✅ debug_dump | advance_day | — |

---

## Пример: сценарий 1 (Cursor)

```
1. user-harvey-injury → injury_reset
2. user-harvey-injury → injury_debuff_add { buff_id: "buffFracturedBone" }
3. Проверить ответ: MainInjuryId: buffFracturedBone, valid: yes
```

## Пример: сценарий 2

```
1. injury_debuff_add buffFracturedBone  (из сценария 1)
2. injury_debuff_add { buff_id: "buffDeepCuts" }  // без force
3. Ожидание: MainInjuryId остаётся buffFracturedBone
4. injury_debuff_add { buff_id: "buffDeepCuts", force: true }
5. Ожидание: MainInjuryId = buffDeepCuts
```

## Пример: сценарий 4 (dirty wound)

```
user-stardew: warp_to_mine_floor { floor: 10 }
user-stardew: pause_time { ... }
user-harvey-injury: injury_reset
user-harvey-injury: injury_debuff_add { buff_id: "buffDeepCuts" }
user-harvey-injury: injury_mine_dirty_debug
```

---

## Конфиг мода

```json
{
  "EnableInjuryMcp": true,
  "InjuryMcpPort": 24843
}
```

`EnableInjuryMcp: false` — сервер не стартует (только SMAPI-консоль).

---

## Связанные документы

- [`main-injury-testcases.md`](main-injury-testcases.md) — полный чеклист
- [`stardew-mcp.md`](stardew-mcp.md) — StardewMCP (мир, время, NPC)
- [`FOR_TEST.md`](FOR_TEST.md) — SMAPI-команды и события
