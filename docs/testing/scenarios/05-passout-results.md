# Результаты: suite 05-passout

> Сценарий: [05-passout.json](05-passout.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 5) |
| Сейв | test (Spring Day 18 Y1) |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-PASSOUT-001 | HP 0–10 → buffBadlyHurt | **SKIP** | нет `injury_pass_out_sim`; StardewMCP `set_health` не триггерит PassOutHandler без gameplay ticks |
| HOI-PASSOUT-002 | Mine rescue pipeline | **PASS** | `injury_debug_mine_rescue` → NeedsMineRescueEvent=True → advance_day + daily_checks → NeedsMineRescueEvent=False, buffBadlyHurt |
| HOI-PASSOUT-003 | topic → forced hosp | **PASS*** | topicMineInjuryRescue + buffBadlyHurt + Hospital; **`injury_game_ui_end_event`** (если FirstTreatment) → **`injury_location_logic`** → IsHospitalized=true, reason=mine_rescue, topicMineInjuryRescue снят |
| HOI-PASSOUT-004 | Exhaustion pass-out | **SKIP** | нет StardewMCP stamina / `injury_pass_out_sim kind=exhaustion` |
| HOI-PASSOUT-005 | Late town collapse | **SKIP** | не прогонялся; нужен live 2:00 AM |

## MCP-цепочка PASSOUT-003 (FirstTreatment blocker)

```
injury_reset → injury_topic_add topicMineInjuryRescue
→ injury_debuff_add buffBadlyHurt
→ teleport Hospital (10am)
→ injury_game_ui_end_event   # если HarveyMod_FirstTreatment
→ injury_location_logic
→ injury_hospital_status     # reason=mine_rescue
```

## Что читать следующему чату

- [06-hospitalization-results.md](06-hospitalization-results.md)
- [07-proximity-results.md](07-proximity-results.md)
- HOI-PASSOUT-001/004 — нужен `injury_pass_out_sim`
