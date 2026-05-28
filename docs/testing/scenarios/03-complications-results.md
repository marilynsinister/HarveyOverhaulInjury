# Результаты: suite 03-complications

> Сценарий: [03-complications.json](03-complications.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 4, после фиксов WetStitches/MineForbidden reset) |
| Сейв | test (Spring 15 Y1) |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-COMP-001 | WetBandage manual + treat | **PASS** | (прогон 1) harvey_click TreatComplications → Complications=(none) |
| HOI-COMP-002 | WetBandage от дождя | **PASS** | `injury_rain_wet_simulate force=True` → HarveyMod_WetBandage + ActiveComplications |
| HOI-COMP-003 | WetBandage без лечения | **PASS** | (прогон 1) TreatmentStarted=no → нет WetBandage |
| HOI-COMP-004 | DirtyWound в шахте | **PASS** | (прогон 1) mine_dirty_simulate 60m force |
| HOI-COMP-005 | DirtyWound negative | **PASS** | (прогон 1) |
| HOI-COMP-006 | DirtyWound→Infected | **PASS** | (прогон 1) |
| HOI-COMP-007 | WetBandage→Infected | **PASS** | (прогон 1) |
| HOI-COMP-008 | WetStitches pool | **PASS** | `harvey_click` → `BathHouse_Pool` + `injury_location_logic` → Complications=HarveyMod_WetStitches (фикс HasWetStitchesExposure) |
| HOI-COMP-009 | Neglect | **PASS** | test_age 5d + run_daily_checks → Complications=HarveyMod_Neglect, topicHarvey_Neglect |
| HOI-COMP-010 | PainFlare storm | **PASS*** | *QA-путь:* `injury_complication_add HarveyMod_PainFlare` при main=buffFracturedBone; автоматический storm roll не прогонялся |
| HOI-COMP-011 | AllergicRash | **PASS*** | *QA-путь:* `injury_complication_add HarveyMod_AllergicRash` (автотриггер в C# отсутствует по дизайну сценария) |

## Исправления, подтверждённые прогоном

- `TryApplyNeglectComplication` — Neglect в `ActiveComplications`, не только на игроке
- `injury_rain_wet_simulate` — COMP-002 без UpdateTick
- `RunQaDailyChecks` — расширенный отчёт (`Neglect=`, `MineForbidden=`)

## MCP-цепочка COMP-008

```
injury_reset → injury_debuff_add buffSurgicalWound → injury_harvey_click
→ teleport BathHouse_Pool → injury_location_logic
→ injury_phase_list   # Complications=HarveyMod_WetStitches
```

## Что читать следующему чату

- [04-mine-forbidden-results.md](04-mine-forbidden-results.md)
- [06-hospitalization-results.md](06-hospitalization-results.md)
