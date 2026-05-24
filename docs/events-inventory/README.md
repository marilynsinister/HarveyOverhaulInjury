# Events Inventory — HarveyOverhaul / InjuryCare

Черновая инвентаризация всех CP-событий и C#-мостов мода InjuryCare + content pack HarveyOverhaul [CP].

| Файл | Содержание |
|---|---|
| [00-summary-table.md](00-summary-table.md) | Сводная таблица |
| [01-cp-events-catalog.md](01-cp-events-catalog.md) | Полный каталог CP с script preview |
| [02-csharp-bridges.md](02-csharp-bridges.md) | C# startEvent, topics, mail (авто) |
| [03-gaps-and-risks.md](03-gaps-and-risks.md) | Разрывы, дубликаты, недостижимые события |
| [04-fork-subevents.md](04-fork-subevents.md) | Fork-подсобытия (declineFood, refuseCheckup, …) |
| [05-csharp-inventory.md](05-csharp-inventory.md) | Детальный разбор C# startEvent, topics, mail |
| [06-locations-index.md](06-locations-index.md) | Индекс по локациям Data/Events/* |
| [07-reachability-table.md](07-reachability-table.md) | **Достижимость:** сводная таблица |
| [07-reachability-details.md](07-reachability-details.md) | **Достижимость:** детальный разбор (5 пунктов) |
| [08-events-as-book.md](08-events-as-book.md) | **Книга:** читабельное содержание всех сцен |
| [09-timing-audit.md](09-timing-audit.md) | **Тайминги:** аудит C#-обработчиков и цепочек |
| [10-relationship-narrative-audit.md](10-relationship-narrative-audit.md) | **Сюжет:** аудит тона и relationship gates |
| [11-id-sync-audit.md](11-id-sync-audit.md) | **ID sync:** buff/topic/mail/event/trigger C# ↔ CP |
| [12-cp-event-launch-safety.md](12-cp-event-launch-safety.md) | **Безопасность:** C# `startEvent` / mine rescue |
| [13-one-shot-audit.md](13-one-shot-audit.md) | **Одноразовость:** eventsSeen, AppliedTriggers, topics |
| [14-scenario-chains.md](14-scenario-chains.md) | **Сценарные цепочки:** 8 major flows step-by-step |
| [events-audit.md](../events-audit.md) | **Сводный аудит** CP + C# |
| [harvey-events-fix-report.md](../harvey-events-fix-report.md) | Отчёт о правках 2026-05-23 |

**Аудит обращений по уровню отношений (с правками CP):** [harvey-relationship-visits-audit](../harvey-relationship-visits-audit/) — gates, topics, визиты Харви, тон, контакт.

**Ручное тестирование в игре:** [docs/testing](../testing/) — чеклисты, SMAPI-команды, сценарии topics/mail и CP-событий.

**Статус:** актуализация **2026-05-24**. **46** уникальных event ID в активном CP (49 записей с дублями ключей). C# launchers wired: hospital pass-out (`eventHarveyEmergencyCare` / `eventHarveyExhaustion`), minor mine rescue, storm comfort buff gate, `topicRescueOperation`, `topicDiagnosisComplete`.

**Автоген:** `00`–`04`, `06`, `08`, `11`, `13` — `python tmpMap/parse_events_inventory.py`, `generate_event_book.py`, `sync_id_audit.py`, `one_shot_audit.py`. Ручные секции — `python tmpMap/update_events_inventory_manual.py`.

**Отчёт о правках:** [harvey-events-fix-report.md](../harvey-events-fix-report.md) · **Сводный аудит:** [events-audit.md](../events-audit.md)

**CP sources (content.json):** `events.json`, `eventsCare.json`, `eventsMineRescue.json`  
**Не подключено:** `events_for_mode_new_formatted.json`
