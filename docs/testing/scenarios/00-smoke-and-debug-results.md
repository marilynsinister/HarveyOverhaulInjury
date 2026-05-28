# Результаты: suite 00-smoke-and-debug

> Базовые правила: [00-ai-testing-rules.md](../00-ai-testing-rules.md)  
> Сценарий: [00-smoke-and-debug.json](00-smoke-and-debug.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 |
| Сейв | test (Spring 6 Y1) |
| StardewMCP | OK (24842) |
| Injury MCP | OK (24843) |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-SMOKE-001 | Игра и мод загружены | **PASS** | StardewMCP: игрок Mine, Harvey 8♥; `injury_debuff_list` OK. SMAPI log — не проверялся (нет MCP). |
| HOI-SMOKE-002 | injury_reset очищает состояние | **PASS** | После add: MainInjury=buffDeepCuts + DirtyWound; после reset: (none). |
| HOI-CMD-001 | injury_debuff_list | **PASS** | buffHurt, buffDeepCuts, buffFracturedBone, HarveyMod_DirtyWound, HarveyMod_WetBandage в выводе. |
| HOI-CMD-002 | injury_debuff_add buffHurt | **PASS** | MainInjury=buffHurt, valid=YES, topicHurt owned (days=2). |
| HOI-CMD-003 | injury_debuff_add buffDeepCuts | **PASS** | фаза 0/3, TreatmentStarted=no. |
| HOI-CMD-004 | injury_phase_ready | **PASS** | [→след.фаза] на buffDeepCuts. |
| HOI-CMD-005 | injury_phase_recovery | **PASS** | Цепочка harvey_click→advance×2→recovery; фаза 3/3 [→выздоровление]. |
| HOI-CMD-006 | unknown buff id | **PASS*** | MainInjury=(none), crash нет. *Warning в SMAPI log не проверялся. |

## Что читать следующему чату

- [01-simple-injuries.json](01-simple-injuries.json) — suite 01 (3 TC, simple treatment + advance_day)
- [00-ai-testing-rules.md](../00-ai-testing-rules.md)
- [injury-mcp.md](../injury-mcp.md) · [stardew-mcp.md](../stardew-mcp.md)
- Ручная проверка SMAPI log для HOI-SMOKE-001 и HOI-CMD-006 (optional)
- После suite 01 → [02-phased-injuries.json](02-phased-injuries.json)
