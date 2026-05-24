# Достижимость CP-событий HarveyOverhaul / InjuryCare

**Актуализация 2026-05-24.** **Актуализация 2026-05-24.** Анализ для **активной** связки: CP `content.json` (`events.json`, `eventsCare.json`, `eventsMineRescue.json`) + SMAPI-мод **HarveyOverhaulInjury** (C#).

Учтено: `triggersInjury.json` и `triggersStress.json` в CP **закомментированы**; `events_for_mode_new_formatted.json` **не подключён**.

Легенда «Достижимо?»: **Да** / **Частично** (узкое окно или зависимости) / **Нет** / **Мёртвый контент** (не в content.json).

| Event ID | Достижимо? | Почему | Блокирующее условие | Как исправить |
|---|---|---|---|---|
| **Mine rescue (InjuryCare)** |||||
| `eventHarveyMineRescue` | Частично | C# **только Dating/Married**; fallback CP legacy если dating-entry отсутствует | Без dating cutscene не запускается (by design) | Med-only rescue для non-dating — опционально |
| `eventHarveyMineRescueDating` | Да | C# severe + dating/married; warp Mine → `startEvent`; `topicMineRescuePending` блокирует interception | Первая rescue-сцена; повтор → topic | — |
| `eventHarveyMinorMineRescue` | **Частично** | C# `TryTriggerMinorMineRescue` при входе в Mine с injury **без** Severe; **не** боевой death-rescue | Severe combat death → major rescue only; 1×/день cooldown | Документировать отличие minor vs severe path |
| `eventHarveyMineInterception` | Частично | `triggersCare` → SpaceCore PlayEvent при входе в Mine | Dating/married + SpaceCore + **базовый** injury-buff из списка CP (не фазовый `HarveyMod_*`) | В Condition добавить фазовые ID **или** `PLAYER_HAS_CONVERSATION_TOPIC topic*` |
| `eventHarveySkullCavePrevention` | Частично | SpaceCore при входе в SkullCave (`triggerLocationReactionSkullCaveExit`); `triggerHarveySkullCaveWarning` **сломан** (`Mine SkullCave` одновременно) | SpaceCore + dating/married; для warning-триггера — невыполнимая локация | Исправить Condition на `SkullCave` **или** разделить на два триггера |
| **Pass-out / care chain** |||||
| `eventHarveyFirstMeeting` | Да | BusStop, `!PLAYER_HAS_MET Harvey`, 06:00–26:00; скрипт из `eventsCare.json` (перекрывает дубль в `events.json`) | Первый визит на BusStop до знакомства | — |
| `eventHarveyCheckup` | Да | BusStop 14:00–16:00 + `topicAgreedCheckup` из fork `eventHarveyFirstMeeting` | Нужно согласиться на осмотр в first meeting | — |
| `eventHarveyFirstVisit` | Да | Farm 10:00–12:00, day ≥3, `topicFirstMeeting`, !seen first visit | `topicFirstMeeting` (7 д) из first meeting | — |
| `eventHarveySecondVisit` | Да | Farm, day ≥7, seen first visit, !outcome topics first visit (7d), !seen | Outcome-topic pacing | — |
| `eventHarveyFirstWalk` | **Да** | Farm sunny, day ≥11, seen second visit, !outcome topics second visit, !seen | **Исправлено 2026-05-23** | — |
| `eventHarveyCheckFarmerOutsideAfter22` | Частично | Farm 22:00–02:00 + `topicPassedOutInTown` + dating/married | Topic (2 д) ставит **C#** PassOutHandler при обмороке в Town после 26:00; нужен вход на Farm ночью | Синхронизировать длительность topic с цепочкой |
| `eventHarveyMorningCheckup` | Частично | Farm 06:00–08:00 + `topicHarveyMandatoryCheckup` + **только Dating** | After22 ставит topic (1 д) для Dating **и** Married, но event **исключает** Married | Добавить `Married` в precondition **или** отдельная married-версия |
| `eventHarveyCheckHealthFarmer` | Частично | Farm 10:00–12:00 + `eventsSeen PlayerKilled` + Dating | `PlayerKilled` — ванильный флаг обморока; C# mine rescue **не** заменяет его; нужен dating | Документировать / добавить bridge из InjuryCare pass-out |
| `eventHarveyLateNightCollapse` | Частично | Town 24:00–26:00, без других gates | BETAS-триггер с PlayEvent **отключён**; только random entry в Town в полночь | Включить `triggersInjury` **или** C# `startEvent` при town pass-out |
| `eventHarveyEmergencyCare` | **Частично** | C# `QueueHospitalEvent` при critical pass-out (health≤10) **вне шахты** + dating/married; fallback если seen | Dating gate; one-shot cutscene (`eventsSeen`) | — |
| `eventHarveyExhaustion` | **Частично** | C# `QueueHospitalEvent` при `WasExhausted` pass-out **вне шахты**; в шахте — только fallback topic | Exhaustion в Mine пропускает cutscene | — |
| `eventHarveyTreatmentCollapse` | **Нет** | Нет preconditions, нет switchEvent, нет trigger | Orphan script | Привязать к stress collapse topic **или** удалить |
| `eventStayInHospital` | **Нет** | Orphan; C# `HospitalizationManager` блокирует выход диалогом, не CP-event | Нет caller | `switchEvent` при попытке выхода **или** удалить |
| **Hospital / stress mod** |||||
| `eventHarveyMedicalCheck` | Да | Hospital, sunny, 14:00–18:00, hearts 1500, mail `mailHarveyMedicalCheckReminder` | Mail шлёт `triggersCare` при dating/married (на завтра каждый DayStarted — спам-риск) | Ограничить trigger mail одноразовым флагом |
| `eventHarveyTraumaExam` | Да | Hospital 08:00–18:00, hearts 2000 | — | — |
| `HarveyMod_FirstTreatment` | Да | Hospital 09:00–21:00, 750♥, **`topicHarveyNeedsFirstTreatment`** (C#), !seen | — | — |
| `HarveyMod_NightCrisis_Dating` | Да | Hospital night, 1500♥, Dating/Married, seen FirstTreatment, !seen Pre/Dating/legacy | — | — |
| `HarveyMod_NightCrisis_PreDating` | Да | Hospital night, 1500♥, !Dating, seen FirstTreatment | Split 2026-05-23 | — |
| `HarveyMod_BirthdayHospital_Dating` | Частично | Hospital, summer 9, 2000♥, Dating/Married, !seen Friend/legacy | Игрок в Hospital в этот день | — |
| `HarveyMod_BirthdayHospital_Friend` | Частично | Hospital, summer 9, 2000♥, !Dating | Игрок в Hospital | — |
| `HarveyMod_TreatmentPlanMeeting` | **Частично** | Hospital + `topicDiagnosisComplete` — C# `TryAddDiagnosisCompleteTopic` при старте eligible лечения | Нужен активный treatment eligible injury | — |
| **Story arc E1–E8** |||||
| `HarveyOverhaulStory.E1_SlipperyPath` | Да | BusStop, wind, 07:00–14:00, !seen E1, !CD global | — | — |
| `HarveyOverhaulStory.E2_InsistentExam` | Да | Hospital, seen E1, hearts ≥2, !CD | — | — |
| `HarveyOverhaulStory.E3_ForestApothecary` | Частично | Forest Thu–Sat, sunny, seen E2, !CD | Нужен визит в лес в окне | — |
| `HarveyOverhaulStory.E4_PierBreath` | Да | Beach evening, sunny, seen **E3**, !topic pier | Линейная цепочка после E3 | — |
| `HarveyOverhaulStory.E5_StormBeside` | Частично | Hospital, **storm**, seen E4, hearts 1500, !CD | Нужна гроза + Hospital днём | — |
| `HarveyOverhaulStory.E6_SayItOutLoud` | Частично | Hospital evening, seen E5, hearts 1750 | CD topics E5/global | — |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | Да | Town sunny midday, seen **E6**, hearts 2000, !CD | **Исправлено:** было seen E1 | — |
| `HarveyOverhaulStory.E8_QuietShelf` | Частично | ArchaeologyHouse Sat 10–16, seen **E7**, !CD E7 | **Исправлено:** было seen E1 | Precondition E7 вместо E1 |
| **Romance / dates** |||||
| `eventHarveyFirstDate` | Да | Forest, dating, hearts 2000, sunny evening, не winter | — | — |
| `eventHarveyMountainDate` | Да | Mountain morning, dating, hearts 2250, sunny | — | — |
| `eventHarveyPropose` | Да | Beach, dating, hearts 2500, sunny evening, не winter | — | — |
| `eventHarveyRoomCheckup` | Да | HarveyRoom, hearts 1500 | — | — |
| `eventHarveyRoomCheckup2` | Частично | HarveyRoom, dating, BETAS NPC location, Random 0.2 | **Требует мод BETAS** | Fallback без BETAS или vanilla location check |
| **Storm comfort (stress)** |||||
| `eventHarveyStormComfortFarm` | **Частично** | C# `StormComfortLauncher` (daily roll) → `buffStressThunder` или `topicHarveyStormStress`; затем vanilla entry + Random | Dating не требуется; hearts ≥750; 1 roll/день | Random + location window |
| `eventHarveyStormComfortForest` | **Частично** | То же (C# buff gate) + Forest + Random 0.55 | Random + storm day | — |
| `eventHarveyStormComfortTown` | **Частично** | C# buff gate + Town + Random 0.3 | Random | — |
| `eventHarveyStormComfortMine` | **Частично** | C# buff gate + Mine + Random 0.8 | Random | — |
| `eventHarveyStormComfortMountain` | **Частично** | C# buff gate + SVE summit + Random 0.4 | SVE + Random | — |
| `eventHarveyStormComfortDesert` | **Частично** | C# buff gate + Desert + Random 0.3 | Random | — |
| **Other** |||||
| `eventRescueOperation` | **Частично** | C# `RescueOperationLauncher` ставит `topicRescueOperation` после E5 / storm comfort; Woods + storm + hearts 600 | Нужен storm + topic window + !seen | — |
| `MyMod_HarveyStormComfortForest` | Мёртвый | Файл не в content.json | Не загружается | Include в content.json **или** удалить |
| `MyMod_HarveyStressTiredCheck` | Мёртвый | Не в content.json | — | То же |
| `MyMod_HarveyUrgentFarmVisit` | Мёртвый | Не в content.json | — | То же |

## Сводка

| Категория | Да | Частично | Нет / мёртвый |
|---|---:|---:|---:|
| InjuryCare / mine | 1 | 4 | 0 |
| Care / pass-out chain | 3 | 6 | 2 |
| Hospital mod | 3 | 2 | 0 |
| Story E1–E8 | 3 | 5 | 0 |
| Romance | 4 | 1 | 0 |
| Storm comfort | 0 | 6 | 0 (+3 мёртвых файла) |
| Прочее | 0 | 1 | 0 (+3 мёртвых) |

**Оставшиеся конфликты InjuryCare:** phase-buff vs CP trigger buff list (Mine/Skull interception); `topicMineInjuryRescue` снимается при forced hosp; morning checkup Dating-only vs Married after22; orphan `eventStayInHospital` / `eventHarveyTreatmentCollapse`.
