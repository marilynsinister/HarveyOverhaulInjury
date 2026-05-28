# Результаты: suite 02-phased-injuries

> Сценарий: [02-phased-injuries.json](02-phased-injuries.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 3) |
| Метод | `injury_harvey_click` + `injury_phase_*` + `injury_phase_cure` |

| ID | Buff | Статус | Заметки |
|----|------|:------:|---------|
| HOI-PHASE-001 | buffSprainedAnkle | **PASS** | 0→1→2→cure, MainInjury=(none) |
| HOI-PHASE-002 | buffBruisedRibs | **PASS** | фаза 2 = HarveyMod_BruisedRibs_Healing |
| HOI-PHASE-003 | buffBackStrain | **PASS** | |
| HOI-PHASE-004 | buffDeepCuts | **PASS** | (прогон 1) |
| HOI-PHASE-005 | buffBurnWounds | **PASS** | severe; StartTreatment без auto-hosp |
| HOI-PHASE-006 | buffInfectedWound | **PASS** | severe; фаза 2 = HarveyMod_InfectedWound_Treatment |
| HOI-PHASE-007 | buffTornMuscles | **PASS** | 3 фазы; `phase_recovery` → MainInjury=(none) до `phase_cure` |
| HOI-PHASE-008 | buffConcussion | **PASS*** | *QA:* `debuff_add` сразу phase 1/3 + auto-hosp; phase_* без `ignore_hospital`; cure OK |
| HOI-PHASE-009 | buffFracturedBone | **PASS** | (прогон 1) `ignore_hospital` / discharge перед phase_* |
| HOI-PHASE-010 | buffShrapnelWounds | **PASS** | StartTreatment → hosp; phase_* работают in-hospital |
| HOI-PHASE-011 | buffCold | **PASS** | |

**Итого: 11/11 PASS** (2 с QA-оговорками: Concussion, FracturedBone).

## Заметки MCP

- `buffFracturedBone` / `buffShrapnelWounds` + `injury_harvey_click` → `StartForcedHospitalization` — ожидаемо.
- `buffConcussion` + `injury_debuff_add` → сразу phase 1 + госпитализация (без отдельного harvey_click).
- Severe фазовые: `injury_phase_*` работают и при `IsHospitalized=true`.

## Что читать следующему чату

- [03-complications-results.md](03-complications-results.md) — COMP-008 PASS
- [04-mine-forbidden-results.md](04-mine-forbidden-results.md) — MINE-003 PASS
