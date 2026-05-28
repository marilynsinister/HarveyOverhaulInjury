# Результаты: suite 07-proximity

> Сценарий: [07-proximity.json](07-proximity.json)

| Поле | Значение |
|------|----------|
| Дата | 2026-05-28 (прогон 5) |
| Сейв | test (Spring Day 18 Y1) |

| ID | Название | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-PROX-001 | Реакция рядом с Harvey | **SKIP** | нужен UpdateTick ~10с; `injury_proximity_test` не в Injury MCP (только SMAPI console) |
| HOI-PROX-002 | Proximity не лечит | **PASS** | buffDeepCuts + teleport Hospital → TreatmentStarted=no, CurrentPhase=0, buffDeepCuts active |
| HOI-PROX-003 | Cooldown в локации | **SKIP** | нужен live tick + SMAPI log `[Proximity]` |
| HOI-PROX-004 | Сброс при смене локации | **SKIP** | нужен live tick; warp Farm→Hospital без MCP assert на bubble |

## Что читать следующему чату

- Добавить `injury_proximity_test` в Injury MCP (опционально)
- [08-cp-events.json](08-cp-events.json) — ручной `debug ebi`
