# Аудит событий Харви — HarveyOverhaul [CP]

Дата: актуальное состояние CP после правок topic gates, pacing, Story chain, heart-gates.

**Источники:** `content.json` → `events.json`, `eventsCare.json`, `eventsMineRescue.json`, `triggersCare.json`; C# `PassOutHandler`, `DialogueManager`, `InjuryManager`.

**Актуализация:** 2026-05-23 (gates, split, Story chain, C# topics).

Friendship `N` = очки (1♥ = 250).

---

## Сводная таблица

| Event ID | Название сцены | Location | Time | Weather | Hearts/Friendship | Dating/Married? | Предыдущие события | Injury/topic/mail | Слишком рано? | Недостижимо? | Комментарий | Рекомендуемая правка |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `eventHarveyFirstMeeting` | Первая встреча на BusStop | `BusStop` | 0600 2600 | — | — | Нет | !seen: eventHarveyFirstMeeting | topicFirstMeeting | Нет | Нет | Дубль в events.json, eventsCare.json; eventsCare грузится после events.json | Оставить один источник в eventsCare.json |
| `eventHarveyCheckup` | Согласованный осмотр в клинике | `BusStop`* | 1400 1600 | — | — | Нет | seen: eventHarveyFirstMeeting | topicAgreedCheckup, DAYS_PLAYED 2 | Нет | ⚠️ Target mismatch | ⚠️ Ключ BusStop, скрипт — координаты Hospital | `Target: Hospital` |
| `eventHarveyFirstVisit` | Первый визит на ферму | `Farm` | 600 1200 | — | — | Нет | !seen: eventHarveyFirstVisit | topicFirstMeeting, DAYS_PLAYED 3 | Нет | Нет | Зависит от BusStop meeting; без Town fallback meeting цепочка мёртва | — |
| `eventHarveySecondVisit` | Второй визит (чай) | `Farm` | 600 1200 | — | — | Нет | seen: eventHarveyFirstVisit; !seen: eventHarveySecondVisit | topicHarveyFirstVisitAgree, topicHarveyFirstVisitNeutral, topicHarveyFirstVisitRefused, DAYS_PLAYED 7 | Нет | Нет | — | — |
| `eventHarveyFirstWalk` | Прогулка в лес | `Farm` | 600 1200 | Sunny | — | Нет | seen: eventHarveySecondVisit; !seen: eventHarveyFirstWalk | topicHarveySecondVisitAgree, topicHarveySecondVisitNeutral, topicHarveySecondVisitRefused, DAYS_PLAYED 11 | Нет | Нет | — | — |
| `HarveyOverhaulStory.E1_SlipperyPath` | E1 — Скользкая дорожка | `BusStop` | 700 1400 | Wind | 500 (2♥) | Нет | !seen: HarveyOverhaulStory.E1_SlipperyPath | HarveyMod_CD_Global | Нет | Нет | — | — |
| `HarveyOverhaulStory.E2_InsistentExam` | E2 — Настойчивый осмотр | `Hospital` | 0900 1700 | — | 750 (3♥) | Нет | seen: HarveyOverhaulStory.E1_SlipperyPath; !seen: HarveyOverhaulStory.E2_InsistentExam | HarveyMod_CD_Global HarveyMod_CD_E1 | Нет | Нет | — | — |
| `HarveyOverhaulStory.E3_ForestApothecary` | E3 — Лесная аптека | `Forest` | 1200 1800 | Sunny | 1000 (4♥) | Нет | seen: HarveyOverhaulStory.E2_InsistentExam; !seen: HarveyOverhaulStory.E3_ForestApothecary | HarveyMod_CD_Global, HarveyMod_CD_E2 | Нет | Нет | — | — |
| `HarveyOverhaulStory.E4_PierBreath` | E4 — Дыхание на пирсе | `Beach` | 1800 2600 | Sunny | 1250 (5♥) | Нет | seen: HarveyOverhaulStory.E3_ForestApothecary; !seen: HarveyOverhaulStory.E4_PierBreath | topicHarveyPierBreath | Нет | Нет | — | — |
| `HarveyOverhaulStory.E5_StormBeside` | E5 — Рядом в грозу | `Hospital` | 1400 2000 | Storm | 1500 (6♥) | Нет | seen: HarveyOverhaulStory.E4_PierBreath; !seen: HarveyOverhaulStory.E5_StormBeside | HarveyMod_CD_Global, HarveyMod_CD_E4 | Нет | Нет | — | — |
| `HarveyOverhaulStory.E6_SayItOutLoud` | E6 — Скажи вслух | `Hospital` | 1900 2330 | — | 1750 (7♥) | Нет | seen: HarveyOverhaulStory.E5_StormBeside; !seen: HarveyOverhaulStory.E6_SayItOutLoud | HarveyMod_CD_Global, HarveyMod_CD_E5 | Нет | Нет | — | — |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | E7 — Глоток воды в городе | `Town` | 1200 1500 | Sunny | 2000 (8♥) | Нет | seen: HarveyOverhaulStory.E6_SayItOutLoud; !seen: HarveyOverhaulStory.E7_TownSip_Sunny | HarveyMod_CD_Global | Нет | Нет | ⚠️ Тон: ревниво-опекунский тон при gate 8♥ | — |
| `HarveyOverhaulStory.E8_QuietShelf` | E8 — Тихая полка | `ArchaeologyHouse` | 1000 1600 | — | 2000 (8♥) | Нет | seen: HarveyOverhaulStory.E7_TownSip_Sunny; !seen: HarveyOverhaulStory.E8_QuietShelf | HarveyMod_CD_Global, HarveyMod_CD_E7 | Нет | Нет | — | — |
| `HarveyMod_FirstTreatment` | Первое лечение (клиника) | `Hospital` | 900 2100 | — | 750 (3♥) | Нет | — | topicHarveyNeedsFirstTreatment (C#), !seen | Нет | Нет | Тон med после правок | — |
| `HarveyMod_NightCrisis_Dating` | Ночной кризис (dating) | `Hospital` | 2200 2600 | — | 1500 (6♥) | Dating/Married | seen FirstTreatment | topicNightCrisisComplete | Нет | Нет | Split ✅ | — |
| `HarveyMod_NightCrisis_PreDating` | Ночной кризис (pre-dating) | `Hospital` | 2200 2600 | — | 1500 (6♥) | !Dating | seen FirstTreatment | topicNightCrisisComplete | Нет | Нет | Проф. тон без объятий | — |
| `HarveyMod_BirthdayHospital_Dating` | День рождения (dating) | `Hospital` | — | — | 2000 (8♥) | Dating/Married | — | topicBirthdayHospitalComplete, summer 9 | Нет | Нет | Split ✅ | — |
| `HarveyMod_BirthdayHospital_Friend` | День рождения (friend) | `Hospital` | — | — | 2000 (8♥) | !Dating | — | topicBirthdayHospitalComplete, summer 9 | Нет | Нет | Split ✅ | — |
| `eventHarveyMedicalCheck` | Медосмотр (pre-dating) | `Hospital` | 1400 1800 | Sunny | 1500 (6♥) | !Dating | — | mailHarveyMedicalCheckReminder | Нет | Нет | Split pre-dating | — |
| `eventHarveyMedicalCheck_Dating` | Медосмотр (dating) | `Hospital` | 1400 1800 | Sunny | 1500 (6♥) | Dating/Married | — | mailHarveyMedicalCheckReminder | Нет | Нет | Split ✅ | — |
| `HarveyMod_TreatmentPlanMeeting` | План лечения | `Hospital` | 900 1700 | — | 500 (2♥) | Нет | — | topicDiagnosisComplete | Нет | **Да** (orphan topic) | Cure-ветка | Добавить topic producer |
| `eventHarveyTraumaExam` | Осмотр после травмы | `Hospital` | 0800 1800 | — | 2000 (8♥) | Нет | — | — | Нет | Нет | 8♥ без seen/topic — только friendship gate | — |
| `eventHarveyEmergencyCare` | Экстренная помощь | `Hospital` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp | Подключить trigger/C# bridge или добавить gate topic/buff |
| `eventHarveyExhaustion` | Истощение / капельница | `Hospital` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp | Подключить trigger/C# bridge или добавить gate topic/buff |
| `eventHarveyTreatmentCollapse` | Коллапс на лечении | `Hospital` | — | — | — | Нет | — | — | Нет | ⚠️ Нет preconditions — только C#? | Нет CP preconditions — запуск через C#/триггер или ручной warp; Orphan без явного триггера в CP | Подключить trigger/C# bridge или добавить gate topic/buff |
| `eventStayInHospital` | Принудительная госпитализация | `Hospital` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp | Подключить trigger/C# bridge или добавить gate topic/buff |
| `eventHarveyMineRescue` | Спасение в шахте (severe) | `Mine` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp; C# PassOutHandler startEvent; preconditions в runtime | — |
| `eventHarveyMinorMineRescue` | Лёгкое спасение в шахте | `Mine` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp | — |
| `eventHarveyMineRescueDating` | Спасение в шахте (dating) | `Mine` | — | — | — | Dating или Married | !seen: eventHarveyMineRescueDating, eventHarveyMineRescue, eventHarveyMinorMineRescue | — | Нет | Нет | Dating/Married вариант rescue | — |
| `eventHarveyStormComfortFarm` | Утешение в грозу — ферма | `Farm` | 2000 2600 | Storm | 750 (3♥) | Нет | — | buffStressThunder | ⚠️ 3♥ + Random — может до Story/E6 | ⚠️ Random — может никогда не выпасть | Нет eventsSeen; повтор при снятии buff/topic | Добавить HarveyMod_CD_* или !seen |
| `eventHarveyStormComfortTown` | Утешение в грозу — город | `Town` | — | Storm | 750 (3♥) | Нет | — | buffStressThunder | ⚠️ 3♥ + Random — может до Story/E6 | ⚠️ Random — может никогда не выпасть | Нет eventsSeen; повтор при снятии buff/topic | Добавить HarveyMod_CD_* или !seen |
| `eventHarveyStormComfortForest` | Утешение в грозу — лес | `Forest` | — | Storm | 750 (3♥) | Нет | — | buffStressThunder | ⚠️ 3♥ + Random — может до Story/E6 | ⚠️ Random — может никогда не выпасть | Нет eventsSeen; повтор при снятии buff/topic | Добавить HarveyMod_CD_* или !seen |
| `eventHarveyStormComfortMountain` | Утешение в грозу — горы | `Custom_AdventurerSummit` | — | Storm | 750 (3♥) | Нет | — | buffStressThunder | ⚠️ 3♥ + Random — может до Story/E6 | ⚠️ Random — может никогда не выпасть | Нет eventsSeen; повтор при снятии buff/topic | Добавить HarveyMod_CD_* или !seen |
| `eventHarveyStormComfortDesert` | Утешение в грозу — пустыня | `Desert` | — | Storm | 750 (3♥) | Нет | — | buffStressThunder | ⚠️ 3♥ + Random — может до Story/E6 | ⚠️ Random — может никогда не выпасть | Нет eventsSeen; повтор при снятии buff/topic | Добавить HarveyMod_CD_* или !seen |
| `eventHarveyStormComfortMine` | Утешение в грозу — шахта | `Mine` | — | Storm | 750 (3♥) | Нет | — | buffStressThunder | ⚠️ 3♥ + Random — может до Story/E6 | ⚠️ Random — может никогда не выпасть | Нет eventsSeen; повтор при снятии buff/topic | Добавить HarveyMod_CD_* или !seen |
| `eventHarveyCheckFarmerOutsideAfter22` | Проверка после 22:00 | `Farm` | 2200 0200 | — | — | Dating или Married | — | topicPassedOutInTown | Нет | Нет | — | — |
| `eventHarveyMorningCheckup` | Утренний осмотр на ферме | `Farm` | 0600 0800 | — | — | Dating | — | topicHarveyMandatoryCheckup | Нет | Нет | ⚠️ Тон: «солнышко», завтрак в постель при gate Dating | — |
| `eventHarveyCheckHealthFarmer` | Проверка после смерти игрока | `Farm` | 600 1200 | — | — | Dating | seen: PlayerKilled | — | Нет | Нет | ⚠️ Тон: очень опекунски при gate Dating | — |
| `eventHarveyRoomCheckup` | Осмотр в комнате клиники | `HarveyRoom` | — | — | 1500 (6♥) | Нет | — | — | Нет | Нет | — | — |
| `eventHarveyRoomCheckup2` | Случайный осмотр в HarveyRoom | `HarveyRoom` | — | — | — | Dating | — | — | Нет | ⚠️ Требует Spiderbuttons.BETAS + Random 0.2 | — | — |
| `eventHarveyFirstDate` | Первое свидание | `Forest` | 1800 2600 | Sunny | 2000 (8♥) | Dating | — | — | Нет | Нет | 8♥ Dating; параллельно Story E7/E8 без связи | — |
| `eventHarveyMountainDate` | Свидание в горах | `Mountain` | 900 1200 | Sunny | 2250 (9♥) | Dating | — | — | Нет | Нет | — | — |
| `eventHarveyPropose` | Предложение | `Beach` | 1800 2600 | Sunny | 2500 (10♥) | Dating | — | — | Нет | Нет | 10♥ + Dating; не требует seen first date | — |
| `eventHarveyLateNightCollapse` | Обморок поздно ночью (Town) | `Town` | 2400 2600 | — | — | Нет | — | — | Нет | Нет | — | — |
| `eventHarveySkullCavePrevention` | Пещера черепа — предупреждение | `SkullCave` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp | — |
| `eventHarveyMineInterception` | Перехват у шахты | `Mine` | — | — | — | Нет | — | — | Нет | Нет | Нет CP preconditions — запуск через C#/триггер или ручной warp | — |

---

## Особое внимание (детальный разбор)

### Ранняя care-цепочка (Farm / BusStop)

| Event | Статус | Ключевые риски |
|---|---|---|
| `eventHarveyFirstMeeting` | ✅ gate через `!seen` + `!topicFirstMeeting` | Только BusStop; дубль JSON; без Town fallback |
| `eventHarveyCheckup` | ⚠️ **частично** | Ключ BusStop, скрипт Hospital (5,9); topic + seen meeting + day 2 | Target mismatch | Перенести на Hospital |
| `eventHarveyFirstVisit` | ✅ pacing day 3 + outcome topics | Ждёт `topicFirstMeeting` (7d) |
| `eventHarveySecondVisit` | ✅ day 7 + seen FirstVisit + !outcome topics | — |
| `eventHarveyFirstWalk` | ✅ day 11 + Sunny + seen SecondVisit + !outcome topics | — |

### HarveyOverhaul Story E1–E8

Линейная дуга после правок: **E(n) требует seen E(n−1) + heart-gate + !seen self + CD topics**.

| Event | Hearts | Prev event | Out of order? |
|---|---|---|---|
| E1 | 2♥, Wind | — | Может не стартовать без ветра |
| E2 | 3♥ | E1 | Нет |
| E3 | 4♥, Thu–Sat, Sunny | E2 | Нет |
| E4 | 5♥, Sunny, вечер | E3 | **Было** skip E3→E2; исправлено |
| E5 | 6♥, Storm | E4 | Нет |
| E6 | 7♥ | E5 | Нет |
| E7 | 8♥, Sunny | E6 | **Было** only E1; исправлено |
| E8 | 8♥, Sat | E7 | **Было** only E1; исправлено |

### Cure / hospital (HarveyMod + injury)

| Event | Hearts | Gate | Проблема |
|---|---|---|---|
| `HarveyMod_FirstTreatment` | 3♥ | `topicHarveyNeedsFirstTreatment` (C#) | Тон med после правок |
| `HarveyMod_NightCrisis_Dating` | 6♥ | Dating/Married + seen FirstTreatment | Split ✅ |
| `HarveyMod_NightCrisis_PreDating` | 6♥ | !Dating + seen FirstTreatment | Split ✅ |
| `HarveyMod_BirthdayHospital_Dating` / `_Friend` | 8♥ | summer 9, Hospital | Split ✅ |
| `eventHarveyMedicalCheck` | 6♥ | mail received | OK если trigger в triggersCare |
| `eventHarveyTraumaExam` | 8♥ | time only | Нет topic/injury gate в CP |
| `eventHarveyEmergencyCare` | — | **нет preconditions** | Orphan в CP |
| `eventHarveyExhaustion` | — | **нет preconditions** | Orphan в CP |
| `eventHarveyTreatmentCollapse` | — | **нет preconditions** | Orphan в CP |
| `eventStayInHospital` | — | **нет preconditions** | C#/warp script |

### Mine rescue

| Event | Gate | Комментарий |
|---|---|---|
| `eventHarveyMineRescueDating` | Dating/Married + C# | Основной cutscene при шахтной смерти |
| `eventHarveyMineRescue` | CP fallback | Legacy; C# без Dating **не запускает** rescue |
| `eventHarveyMinorMineRescue` | C# + !Severe | **Недостижим** — всегда `buffBadlyHurt` |

### Storm comfort (6 локаций)

Общий шаблон: **3♥ + buffStressThunder + storm + Random** — нет `eventsSeen`, может опередить Story E5–E6, повторяется при новом buff.

### Dating / proposal / room

| Event | Gate | Out of order / тон |
|---|---|---|
| `eventHarveyFirstDate` | 8♥ Dating | Не требует E6/E7; OK для dating |
| `eventHarveyMountainDate` | 9♥ Dating | — |
| `eventHarveyPropose` | 10♥ Dating | Не требует seen FirstDate |
| `eventHarveyRoomCheckup` | 6♥ | Pre-dating; клинический тон |
| `eventHarveyRoomCheckup2` | Dating + BETAS + Random 0.2 | Часто **недостижимо** |
| `eventHarveyMorningCheckup` | Dating + topic | Партнёрский тон — OK при Dating |
| `eventHarveyCheckFarmerOutsideAfter22` | Dating/Married + topic | OK |

---

## Приоритетные риски (кратко)

### Ранняя care-цепочка (Farm / BusStop)
- `eventHarveyFirstMeeting` — только BusStop; Town fallback не добавлен (см. `02-first-meeting-reachability.md`).
- `eventHarveyCheckup` — ключ на **BusStop**, скрипт клиники → часто **недостижимо**.
- `eventHarveyFirstVisit` → `SecondVisit` → `FirstWalk` — pacing через DAYS_PLAYED + outcome topics ✅.

### HarveyOverhaul Story E1–E8
- Линейная цепочка `seen E(n-1)` + heart-gates ✅ (после правок).
- E1 требует **Wind** — может откладывать старт дуги.

### Параллельные ветки без связи со Story
- `HarveyMod_FirstTreatment` / `NightCrisis` — 3–6♥, не требуют E1–E8.
- Storm comfort (6 локаций) — 3♥ + Random, **нет** seen Story.
- Dating-сцены (`FirstDate`, `Propose`, room checkups) — не привязаны к E6/E7.

### Injury / hospital
- `eventHarveyEmergencyCare`, `eventHarveyExhaustion`, `eventHarveyTreatmentCollapse`, `eventStayInHospital` — **без** CP preconditions (orphan).
- Mine rescue — C# **Dating/Married only**; `topicMineRescuePending` в triggersCare.

### Тон vs hearts
- Storm comfort — **тексты med (вар. B)**, но **`buffStressThunder`** всё ещё блокирует запуск.
- `dialoguesHarveyStress/Cure/Pregnant` — отдельный проход (pet names без gate).

---
## Дубликаты event ID
- **`eventHarveyFirstMeeting`:** events.json @ BusStop; eventsCare.json @ BusStop

---
## Связанные документы
- [01-early-farm-visit-chain.md](01-early-farm-visit-chain.md)
- [02-first-meeting-reachability.md](02-first-meeting-reachability.md)
- [../events-inventory/07-reachability-table.md](../events-inventory/07-reachability-table.md)
