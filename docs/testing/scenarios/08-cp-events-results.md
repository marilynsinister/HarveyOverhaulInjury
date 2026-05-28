# Результаты: suite 08-cp-events

> Сценарий: [08-cp-events.json](08-cp-events.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-CP-001…011 | CP cutscenes smoke | **SKIP** | требуют SMAPI `debug ebi <eventId>` + ручной прогон cutscene; StardewMCP не запускает CP events |

## Автоматизация

- Подготовка через MCP: `injury_reset`, `injury_debug_mine_rescue`, `warp_to_mine_floor`, `injury_game_ui_status` / `injury_game_ui_end_event`
- Assert cutscene: **только вручную** (нет MCP `debug ebi`)

## Что читать следующему чату

- [09-save-load-results.md](09-save-load-results.md)
- Ручной smoke: `debug ebi eventHarveyMineRescueDating` после `injury_debug_mine_rescue`
