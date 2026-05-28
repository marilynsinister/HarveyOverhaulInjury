# Результаты: suite 04-mine-forbidden

> Сценарий: [04-mine-forbidden.json](04-mine-forbidden.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 4) |
| Обход warp | `injury_mine_warning_simulate` + `injury_run_daily_checks` |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-MINE-001 | Severe — вход в шахту | **PASS*** | *QA:* warningWasYesterday=false → MineWarningDay=10, LastMineSevereWarningDay=10. Канонический warp по-прежнему не триггерит HandleMinesLogic |
| HOI-MINE-002 | Light injury — без запрета | **PASS** | (прогон 1) buffHurt + mine warp |
| HOI-MINE-003 | Warning не спамится | **PASS** | `location_logic` ×2: MineWarningDay=15 без increment; reset снимает orphan MineForbidden |
| HOI-MINE-004 | MineForbidden на след. день | **PASS** | warningWasYesterday=true + daily_checks → MineForbidden=True, buff HarveyMod_MineForbidden |
| HOI-MINE-005 | MineForbidden expiry | **PASS** | appliedDay=10, advance_day×2 (→ day 12) + daily_checks → MineForbidden=False |
| HOI-MINE-006 | CP event interception | **PARTIAL*** | *MCP:* forbidden + `location_logic` → LastMineForbiddenInterceptionDay=15, IsPlayerFree=True; CP cutscene — `debug ebi` |

## MCP-цепочка (MINE-001 → 004 → 005)

```
injury_reset → injury_debuff_add buffBadlyHurt
→ injury_mine_warning_simulate [warning_was_yesterday=true]
→ injury_run_daily_checks   # apply forbidden
→ stardew advance_day × N + injury_run_daily_checks   # expiry
```

## Подтверждено прогоном 4

- `injury_mine_warning_simulate` ×2 в один день → второй вызов `SKIP: warning already set today`
- `injury_reset` после `MineForbidden=True` → buff_dump без `HarveyMod_MineForbidden`

## Что читать следующему чату

- [05-passout-results.md](05-passout-results.md)
- HOI-MINE-006 CP cutscene — `debug ebi eventHarveyMineInterception`
