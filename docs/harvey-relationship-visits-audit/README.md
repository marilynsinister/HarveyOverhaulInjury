# Harvey Relationship Visits Audit — HarveyOverhaul [CP]

Аудит и правки **логики обращений Харви** в зависимости от уровня отношений: условия запуска событий, conversation topics, gates по friendship/dating/marriage, тон и последовательность визитов.

| Файл | Содержание | Статус |
|---|---|---|
| [01-early-farm-visit-chain.md](01-early-farm-visit-chain.md) | Ранняя фермерская цепочка: FirstVisit → SecondVisit → FirstWalk, topic gates | ✅ CP исправлено |
| [02-first-meeting-reachability.md](02-first-meeting-reachability.md) | Первая встреча: BusStop-only, fallback Town / C# | ✅ слой A в CP |
| [harvey-events-audit.md](harvey-events-audit.md) | **Полный аудит** всех CP-событий Харви (~47 event ID) | 📋 актуализирован 2026-05-23 |
| [cp-preconditions-audit.md](cp-preconditions-audit.md) | **Техаудит preconditions:** ручная сводка CRITICAL/HIGH | 📋 актуализирован 2026-05-23 |
| [romantic-tone-audit.md](romantic-tone-audit.md) | Pre-dating романтика → вариант B | ✅ правки CP |
| [physical-contact-audit.md](physical-contact-audit.md) | Телесный контакт в событиях | ✅ правки CP |
| [controlling-lines-replacements.md](controlling-lines-replacements.md) | Контролирующие реплики | ✅ 116+ замен |
| [harvey-events-fix-report.md](../harvey-events-fix-report.md) | **Сводный отчёт** всех правок CP/C# | ✅ финал 2026-05-23 |
| [cp-preconditions-audit-appendix.md](cp-preconditions-audit-appendix.md) | Полный автоген по 8 категориям (перезаписывается скриптом) | 🤖 auto |
| *(далее)* | Обращения при средних/высоких hearts, dating, marriage | 🔲 в работе |

**Связанные материалы:** [harvey-relationship-tone-guide.md](../harvey-relationship-tone-guide.md), [events-inventory/10-relationship-narrative-audit.md](../events-inventory/10-relationship-narrative-audit.md), [events-audit.md](../events-audit.md)

**CP sources:** `HarveyOverhaul [CP]/assets/Code/eventsCare.json`, `events.json`, `eventsMineRescue.json`, `triggersCare.json`

**Принцип правок (2026-05-23):** gates/split по Dating; смягчение тона pre-dating (вариант B); C# topics для FirstTreatment и mine rescue.
