# Тестирование Harvey Overhaul

Документация для **ручной проверки мода в игре** и чеклистов перед релизом.

## С чего начать

| Документ | Назначение |
|----------|------------|
| [`FOR_TEST.md`](FOR_TEST.md) | Справочник: SMAPI-команды, травмы, pass-out, mail, debug-HUD (F10) |
| [`EVENTS_TEST_CHECKLIST.md`](EVENTS_TEST_CHECKLIST.md) | Чеклист всех CP-событий + сценарии S01–S18 с галочками |
| [`manual-test-scenarios-topics-mail.md`](manual-test-scenarios-topics-mail.md) | Детальные сценарии topics/mail/тон Харви (1–14) |
| [`main-injury-testcases.md`](main-injury-testcases.md) | MainInjury + Complications: сценарии 1–10, реестр событий |
| [`final-validation-topics-mail.md`](final-validation-topics-mail.md) | Автоматическая сверка topic/mail ID (read-only аудит) |

## Быстрый старт

1. Загрузить C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`.
2. `injury_reset` перед изолированным сценарием.
3. F10 — debug-HUD; `injury_audit_content` — сверка контента в SMAPI-логе.

## Связанная документация (вне этой папки)

- [`../events-inventory/`](../events-inventory/) — каталог событий, цепочки, reachability
- [`../CheckEvent/`](../CheckEvent/) — чеклист правки одного CP-события, паспорта карт
- [`../flow-click-harvey.md`](../flow-click-harvey.md) — логика клика по Харви
- [`../audit-phase-treatment/`](../audit-phase-treatment/) — аудит фаз лечения
