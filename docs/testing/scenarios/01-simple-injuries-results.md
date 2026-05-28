# Результаты: suite 01-simple-injuries

> Сценарий: [01-simple-injuries.json](01-simple-injuries.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 |
| Метод лечения | `injury_harvey_click` (без ручного клика) |
| Ускорение срока | `injury_test_age_injury` + `injury_run_daily_checks` |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-SIMPLE-001 | buffHurt | **PASS** | StartTreatment → buffHarveyTreatment; age 2d → MainInjury=(none) |
| HOI-SIMPLE-002 | buffBadlyHurt | **PASS** | buffHarveyIntensiveCare; age 4d → cure |
| HOI-SIMPLE-003 | buffSurgicalWound | **PASS** | buffPostSurgicalCare; age 7d → cure |

## Что читать следующему чату

- [02-phased-injuries.json](02-phased-injuries.json) — 11 TC, полный цикл фаз
- [00-smoke-and-debug-results.md](00-smoke-and-debug-results.md) — suite 00 (8/8 PASS)
- StardewMCP teleport/Hospital не использовался — `injury_harvey_click` достаточен для simple treatment
