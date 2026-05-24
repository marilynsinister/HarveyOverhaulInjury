# Технический аудит CP preconditions — события Харви

Автоматический разбор `events.json`, `eventsCare.json`, `eventsMineRescue.json` + cross-ref с `triggersCare.json`, `dialoguesHarvey*.json`, C# InjuryCare.

**Автоген appendix 2026-05-23** — CP после split gates / Story chain / C# topics.

Проверено событий: **47** | Уникальных event ID: **46**

---

## Сводка по категориям

| # | Категория | Найдено |
|---|---|---|
| 1 | Несуществующие conversation topics (проверка без add в CP/C#) | **1** |
| 2 | Topics проверяются, но не добавляются в CP/C# | **0** |
| 3 | Topics добавляются, но не используются в preconditions/triggers | **9** |
| 4 | PLAYER_HAS_SEEN_EVENT на несуществующий ID | **0** |
| 5 | seen + !seen одного ID в одном ключе | **0** |
| 6 | Противоречие локация/погода/сцена | **1** |
| 7 | Dating-текст без Dating/Married gate | **11** |
| 8 | Медкризис без injury/topic gate | **6** |
| + | Непредсказуемость (Random, no preconditions, BETAS) | **13** |

---

## 1. Несуществующие conversation topics

- **`topicHarveyNeedsFirstTreatment`** — требуют: `HarveyMod_FirstTreatment`
  - Пример условия: `Friendship Harvey 750/Time 900 2100/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuer`

---

## 2. Topics проверяются, но нигде не добавляются (CP/C#)

*Проблем не найдено.*

---

## 3. Topics добавляются, но не используются в gates

- **`topicTreatmentAgreement`** — добавляет `HarveyMod_TreatmentPlanMeeting` (реакции в dialogue OK)
- **`HarveyMod_CD_E3`** — добавляет `HarveyOverhaulStory.E3_ForestApothecary` (реакции в dialogue OK)
- **`topicHarveyTraumaReveal`** — добавляет `eventHarveyTraumaExam` (реакции в dialogue OK)
- **`HarveyMod_CD_E8`** — добавляет `HarveyOverhaulStory.E8_QuietShelf` (реакции в dialogue OK)
- **`topicHarveyExhaustion`** — добавляет `eventHarveyExhaustion` (реакции в dialogue OK)
- **`topicIntensiveTreatment`** — добавляет `HarveyMod_TreatmentPlanMeeting` (реакции в dialogue OK)
- **`HarveyMod_CD_E6`** — добавляет `HarveyOverhaulStory.E6_SayItOutLoud` (реакции в dialogue OK)
- **`topicMineInjuryRescue`** — добавляет `eventHarveyMineRescue, eventHarveyMineRescueDating, eventHarveyMinorMineRescue` (реакции в dialogue OK)
- **`topicTreatmentRefusal`** — добавляет `HarveyMod_TreatmentPlanMeeting` (реакции в dialogue OK)

---

## 4. PLAYER_HAS_SEEN_EVENT / HasSeenEvent — ID не найден

*Проблем не найдено.*

---

## 5. Конфликт seen и !seen в одном ключе

*Проблем не найдено.*

---

## 6. Локация / погода vs сцена

- **`eventHarveyCheckup`** — Зарегистрировано на `BusStop`, скрипт — координаты клиники
  - `eventHarveyCheckup/Time 1400 1600/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicAgreedCheckup/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarv`

---

## 7. Романтичный тон без Dating/Married

- **`HarveyOverhaulStory.E7_TownSip_Sunny`** @ `Town` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveyFirstMeeting`** @ `BusStop` — Романтичный/партнёрский тон без Dating/Married gate
- **`HarveyOverhaulStory.E3_ForestApothecary`** @ `Forest` — Романтичный/партнёрский тон без Dating/Married gate
- **`HarveyOverhaulStory.E4_PierBreath`** @ `Beach` — Романтичный/партнёрский тон без Dating/Married gate
- **`HarveyOverhaulStory.E8_QuietShelf`** @ `ArchaeologyHouse` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveyFirstVisit`** @ `Farm` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveySecondVisit`** @ `Farm` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveyExhaustion`** @ `Hospital` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveyFirstMeeting`** @ `BusStop` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveyCheckup`** @ `BusStop` — Романтичный/партнёрский тон без Dating/Married gate
- **`eventHarveySkullCavePrevention`** @ `SkullCave` — Романтичный/партнёрский тон без Dating/Married gate

---

## 8. Медицинский кризис без injury/topic gate

- **`eventHarveyTraumaExam`** @ `Hospital` — TraumaExam: только Friendship 2000, нет injury topic
- **`eventHarveyTreatmentCollapse`** @ `Hospital` — Медицинский/кризисный script без topic/buff/mail/seen gate в CP key
- **`eventStayInHospital`** @ `Hospital` — Медицинский/кризисный script без topic/buff/mail/seen gate в CP key
- **`eventHarveyEmergencyCare`** @ `Hospital` — Медицинский/кризисный script без topic/buff/mail/seen gate в CP key
- **`eventHarveyExhaustion`** @ `Hospital` — Медицинский/кризисный script без topic/buff/mail/seen gate в CP key
- **`eventHarveySkullCavePrevention`** @ `SkullCave` — Медицинский/кризисный script без topic/buff/mail/seen gate в CP key

---

## 9. Непредсказуемость (Random / orphan / BETAS)

- **`eventHarveyStormComfortFarm`** @ `Farm` — Random 0.6 (~60%)
- **`eventHarveyTreatmentCollapse`** @ `Hospital` — Нет CP preconditions — только C#/ручной запуск
- **`eventStayInHospital`** @ `Hospital` — Нет CP preconditions — только C#/ручной запуск
- **`eventHarveyStormComfortMountain`** @ `Custom_AdventurerSummit` — Random 0.4 (~40%)
- **`eventHarveyStormComfortTown`** @ `Town` — Random 0.3 (~30%)
- **`eventHarveyRoomCheckup2`** @ `HarveyRoom` — Требует мод BETAS
- **`eventHarveyStormComfortForest`** @ `Forest` — Random 0.55 (~55%)
- **`eventHarveyStormComfortDesert`** @ `Desert` — Random 0.3 (~30%)
- **`eventHarveyStormComfortMine`** @ `Mine` — Random 0.8 (~80%)
- **`eventHarveyEmergencyCare`** @ `Hospital` — Нет CP preconditions — только C#/ручной запуск
- **`eventHarveyExhaustion`** @ `Hospital` — Нет CP preconditions — только C#/ручной запуск
- **`eventHarveySkullCavePrevention`** @ `SkullCave` — Нет CP preconditions — только C#/ручной запуск
- **`eventHarveyMineInterception`** @ `Mine` — Нет CP preconditions — только C#/ручной запуск

---

## Минимальные рекомендуемые правки

| Приоритет | Event | Правка |
|---|---|---|
| CRITICAL | `eventHarveyCheckup` | Перенести ключ с `Data/Events/BusStop` на `Data/Events/Hospital` (тот же script). |
| HIGH | `eventHarveyFirstMeeting` | Удалить дубль из events.json; добавить Town fallback (см. 02-first-meeting-reachability.md). |
| HIGH | `eventHarveyTreatmentCollapse` | Подключить triggersCare / C# bridge с topic или buff gate; или document as C#-only. |
| HIGH | `eventHarveyEmergencyCare` | Подключить triggersCare / C# bridge с topic или buff gate; или document as C#-only. |
| HIGH | `eventHarveyExhaustion` | Подключить triggersCare / C# bridge с topic или buff gate; или document as C#-only. |
| MED | `eventHarveyStormComfortFarm` | Добавить `!PLAYER_HAS_SEEN_EVENT` + HarveyMod_CD_* cooldown. |
| MED | `eventHarveyStormComfortMountain` | Добавить `!PLAYER_HAS_SEEN_EVENT` + HarveyMod_CD_* cooldown. |
| MED | `eventHarveyStormComfortTown` | Добавить `!PLAYER_HAS_SEEN_EVENT` + HarveyMod_CD_* cooldown. |
| MED | `eventHarveyStormComfortForest` | Добавить `!PLAYER_HAS_SEEN_EVENT` + HarveyMod_CD_* cooldown. |
| MED | `eventHarveyStormComfortDesert` | Добавить `!PLAYER_HAS_SEEN_EVENT` + HarveyMod_CD_* cooldown. |
| MED | `eventHarveyStormComfortMine` | Добавить `!PLAYER_HAS_SEEN_EVENT` + HarveyMod_CD_* cooldown. |
| MED | `HarveyMod_FirstTreatment, HarveyOverhaulStory.E6+` | Сверить heart-gates с тоном; FirstTreatment поднять до 4–5♥ или смягчить текст. |

---

## Примечания

- Topics с ключами только в `dialoguesHarvey*.json` считаются **валидными** (реакции на активный topic).
- Orphan-события без preconditions могут быть **намеренно** C#-only (`PassOutHandler`, hospitalization).
- `HarveyMod_CD_*` topics — cooldown Story; отсутствие в gate других веток — отдельный design debt.
- См. также [harvey-events-audit.md](harvey-events-audit.md), [01-early-farm-visit-chain.md](01-early-farm-visit-chain.md).
