# Результаты: suite 09-save-load

> Сценарий: [09-save-load.json](09-save-load.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 4) |
| Путь | **B1** — `advance_day` + `injury_run_daily_checks` (без ручного save/load слота) |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-SAVE-001 | Фазовая травма в лечении | **PASS** | buffDeepCuts phase1 + advance_day + daily_checks → HarveyMod_DeepCuts_Acute restored; snapshot без orphan MineForbidden |
| HOI-SAVE-002 | Осложнение | **PASS** | DirtyWound + advance_day → Complications=HarveyMod_DirtyWound |
| HOI-SAVE-003 | SavedActiveBuffs restore | **PASS** | snapshot: phase buff + prescriptions, без orphan MineForbidden |
| HOI-SAVE-004 | AppliedTriggers | **SKIP** | нет `injury_trigger_set`; нужен manual save/load |
| HOI-SAVE-005 | injury_reset после load | **PASS*** | reset после DirtyWound+DeepCuts → MainInjury=(none); **B2** (save→load→reset) не делался |

## Исправления, подтверждённые прогоном 4

- `FullReset` снимает `HarveyMod_MineForbidden`
- `SanitizeOrphanMineForbiddenBuff` в DayStarted / snapshot — orphan не попадает в `SavedActiveBuffs`

## B2 (ручной save/load)

Не выполнялся. Для полного PASS по JSON: save slot → title → load slot и повтор `injury_phase_list`.
