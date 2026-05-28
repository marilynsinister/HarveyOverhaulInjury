# Результаты: suite 06-hospitalization

> Сценарий: [06-hospitalization.json](06-hospitalization.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 5) |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-HOSP-001 | Forced hospitalization start | **PASS** | topicMineInjuryRescue + buffBadlyHurt + Hospital + `location_logic` (после end_event при FirstTreatment) → reason=**mine_rescue** |
| HOI-HOSP-001b | buffConcussion debuff_add | **PASS** | auto-hosp на Farm |
| HOI-HOSP-002 | Warp HospitalBed | **PASS** | (прогон 1) |
| HOI-HOSP-discharge | injury_hospital_discharge | **PASS** | (прогон 1) |
| HOI-HOSP-003 | Exit lock до min stay | **PASS** | `injury_hospital_lock_enforce` |
| HOI-HOSP-004 | CanDischarge после срока | **PASS** | admission 10:00 + `set_time 12:10pm` → CanDischarge=True, HospitalDischargeReadyShown=True (прогон 5) |
| HOI-HOSP-005 | Hospital activities | **PARTIAL*** | `set_time` 10:40/11:20/12:10 при hold — TimeChanged срабатывает (видно по CanDischarge); log `🏥 Активность` через MCP не читается |
| HOI-HOSP-006 | injury_hospital_discharge | **PASS*** | buffConcussion → discharge ok=yes |
| HOI-HOSP-007 | Событие при выходе | **SKIP** | ручной `debug ebi eventStayInHospital` |

## MCP-обход HOSP-001 / PASSOUT-003

См. [05-passout-results.md](05-passout-results.md) — FirstTreatment при входе в Hospital с `topicHarveyNeedsFirstTreatment`.

## Что читать следующему чату

- [07-proximity-results.md](07-proximity-results.md)
- [08-cp-events-results.md](08-cp-events-results.md)
- HOI-HOSP-005 — QA `injury_hospital_activity_tick` для assert без SMAPI log
