# План проверки карт событий

Отчёт составлен по CP-файлам мода `HarveyOverhaul [CP]` без правок кода и `content.json`.

**Источники Events/\*:** `assets/Code/events.json`, `assets/Code/eventsCare.json`, `assets/Code/eventsMineRescue.json`.  
Файл `events_for_mode_new_formatted.json` в `content.json` не подключён.

**Аудит координат (техническая постановка):** [`events-coordinate-audit.md`](events-coordinate-audit.md).  
**Риски Mine/SkullCave/Volcano:** [`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md).  
**Storm comfort (визуал укрытий):** [`storm-comfort-map-audit.md`](storm-comfort-map-audit.md).  
**Story arc E1–E9:** [`story-arc-map-audit.md`](story-arc-map-audit.md).  
**Backlog исправлений:** [`events-map-fix-backlog.md`](events-map-fix-backlog.md).

**Формат ID story-арки в CP:** `HarveyOverhaulStory.E{N}_…` (точка, не подчёркивание).

**«Визуально на карте»** — нужно открыть локацию в игре/редакторе и проверить тайлы, коллизии, камеру и постановку NPC у указанных координат.

---

## Исключены из проверки

| Event ID | Локация | Причина |
|---|---|---|
| eventHarveyFirstVisit | Farm | Уже проверено вручную |
| eventHarveySecondVisit | Farm | Уже проверено вручную |
| eventHarveyFirstWalk | Farm | Уже проверено вручную |
| acceptWalk | Farm (fork → Forest) | Уже проверено вручную |
| eventHarveyCheckHealthFarmer | Farm → Hospital | Уже проверено вручную |
| eventHarveyCheckFarmerOutsideAfter22 | Farm | Уже проверено вручную |
| eventHarveyMorningCheckup | Farm | Уже проверено вручную |
| eventHarveyStormComfortFarm | Farm → Hospital (fork) | Уже проверено вручную |
| eventHarveyMedicalCheck | Hospital | Уже проверено вручную |
| eventHarveyEmergencyCare | Hospital | Уже проверено вручную |
| eventHarveyExhaustion | Hospital | Уже проверено вручную |
| eventHarveyTraumaExam | Hospital | Уже проверено вручную |
| eventHarveyTreatmentCollapse | Hospital | Уже проверено вручную |
| eventStayInHospital | Hospital | Уже проверено вручную |
| HarveyMod_TreatmentPlanMeeting | Hospital | Уже проверено вручную |
| HarveyMod_TreatmentReview | Hospital | Уже проверено вручную |
| HarveyMod_RecoveryComplete | Hospital | Уже проверено вручную |
| 58 | SeedShop | Уже проверено вручную |
| eventHarveyMountainDate | Mountain | Уже проверено вручную |
| eventHarveyLateNightCollapse | Town → Hospital | Уже проверено вручную |
| 528013 | Town → Custom_* (шар) | Уже проверено вручную |
| eventHarveyRoomCheckup | HarveyRoom | Уже проверено вручную |
| eventHarveyRoomCheckup2 | HarveyRoom | Уже проверено вручную |
| eventHarveyFirstDate | Forest | Уже проверено вручную |
| eventHarveyPropose | Beach | Уже проверено вручную |
| HarveyMod_NightCrisis | Hospital | Уже проверено вручную (базовый ID) |
| HarveyMod_BirthdayHospital | Hospital | Уже проверено вручную (базовый ID) |

---

## Требуют проверки

### 1. C# → CP (шахты, спасение, перехват)

| Event ID | Локация | Файл | Asset/key | Команды движения/координат | Приоритет | Комментарий |
|---|---|---|---|---|---|---|
| eventHarveyMineRescue | Mine → Hospital | eventsMineRescue.json | Data/Events/Mine | warp, move, viewport, positionOffset, faceDirection, animate, changeLocation, globalFade, ignoreCollisions | **High** | Старт 17 7; Harvey warp 17 10; перенос в Hospital 20 5 + offset; анимация лёжа. **Визуально:** Mine (вход/лестница), палата Hospital. |
| eventHarveyMineRescueDating | Mine → Hospital | eventsMineRescue.json | Data/Events/Mine | те же + dating-текст | **High** | Почти зеркало `eventHarveyMineRescue`. Проверить оба варианта на одной карте Mine. |
| eventHarveyMinorMineRescue | Mine → Hospital | eventsMineRescue.json | Data/Events/Mine | warp, move, changeLocation, viewport, animate, globalFade | **High** | Оба NPC уже на 17 7/17 10; короткий fade в Hospital 14 6. |
| HarveyMod_FirstTreatment | Hospital | events.json | Data/Events/Hospital | move, faceDirection, showFrame, animate (кратко) | **Medium** | Кушетка ~4 6 / farmer 5 9. Мало варпов, но осмотр у координат клиники. **Визуально:** Hospital. |
| eventRescueOperation | Hospital → Woods → Forest → Hospital | events.json | Data/Events/Woods | warp, move, speed, advancedMove, positionOffset, animate, changeLocation, temporaryAnimatedSprite, showFrame, globalFade | **High** | Мульти-локация: телефон 3 15, лес 27 18, поиск 40 20, машина Forest 66 16, палата 20 5. **Визуально:** Woods, Forest (дорога/пикап), Hospital. |
| eventHarveyMineInterception | Mine | eventsCare.json | Data/Events/Mine | move, faceDirection, animate, advancedMove (нет), exit move 0 3 | **High** | EarthMine 17 7; Harvey с 17 10; уход вниз по шахте. Запускается из C# (`triggersCare.json`). |
| eventHarveySkullCavePrevention | SkullCave | eventsCare.json | Data/Events/SkullCave | move, faceDirection, quickQuestion, fork | **Medium** | continue 5 5; Harvey 7 7; fork `HarveySkullPromise` (только dialogue). **Визуально:** вход Skull Cave. |

**Fork-подсобытия (не отдельные ID, но проверять при тесте SkullCave):** `HarveySkullPromise`.

---

### 2. Storm comfort (кроме Farm)

| Event ID | Локация | Файл | Asset/key | Команды движения/координат | Приоритет | Комментарий |
|---|---|---|---|---|---|---|
| eventHarveyStormComfortForest | Forest | events.json | Data/Events/Forest | warp Harvey 35 13, move -11 0, faceDirection, animate farmer | **Medium** | Farmer 23 13; Harvey появляется с востока. Без смены локации. **Визуально:** Forest, участок 23–35 по X. |
| eventHarveyStormComfortMountain | Custom_AdventurerSummit → Mountain | events.json | Data/Events/Custom_AdventurerSummit | warp, speed, advancedMove, stopAdvancedMoves, changeLocation, animate | **High** | Старт 41 27; Harvey warp 32 42 + длинный advancedMove; затем Mountain 79 1 и ещё advancedMove. **Визуально:** обе карты, перила/склон. |
| eventHarveyStormComfortTown | Town → Saloon | events.json | Data/Events/Town | warp Harvey 36 56, advancedMove, changeLocation, warp в Saloon 14 23, advancedMove | **High** | Farmer 39 73; погоня по Town; укрытие в Saloon с Gus. **Визуально:** Town (юг), интерьер Saloon. |
| eventHarveyStormComfortDesert | Desert | events.json | Data/Events/Desert | warp Harvey 17 26, move, faceDirection, animate | **Medium** | Farmer 15 23; короткий подход Harvey. **Визуально:** Desert, автобусная зона. |
| eventHarveyStormComfortMine | Mine → Town | events.json | Data/Events/Mine | warp Harvey 18 13, move, changeLocation Town, warp 72 22 | **High** | Farmer 15 5; подъём из шахты; финал на скамейке Town. **Визуально:** Mine (лестница), Town 72 22. |

---

### 3. HarveyOverhaulStory

| Event ID | Локация | Файл | Asset/key | Команды движения/координат | Приоритет | Комментарий |
|---|---|---|---|---|---|---|
| HarveyOverhaulStory.E1_SlipperyPath | BusStop | events.json | Data/Events/BusStop | move, proceedPosition, faceDirection, animate, quickQuestion (ветки move), globalFade | **High** | Viewport 52 24; farmer 20 23, Harvey 26 22; сходятся к центру. **Визуально:** дорога BusStop, мокрые тайлы/обочина. |
| HarveyOverhaulStory.E2_InsistentExam | Hospital | events.json | Data/Events/Hospital | doAction, move, `faceDirection 2` + showFrame 107, animate, proceedPosition, quickQuestion | **High** | 6 10; вход farmer, кушетка, осмотр. **Визуально:** приёмная + кушетка Hospital. |
| HarveyOverhaulStory.E2B_QuietAgreement | Town | events.json | Data/Events/Town | move, showFrame, speed, quickQuestion (ветки move), fork-логика в question | **Medium** | 28 67; 2 варианта погоды (Sunny/Wind), один ID. **Визуально:** площадь Town, лавка/дерево. |
| HarveyOverhaulStory.E3_ForestApothecary | Forest | events.json | Data/Events/Forest | move, proceedPosition, faceDirection, quickQuestion | **Medium** | 50 13; прогулка -3 тайла. **Визуально:** лесная тропа, место сбора трав. |
| HarveyOverhaulStory.E3B_WingPatient | Forest | events.json | Data/Events/Forest | move, animate Harvey 40 41, quickQuestion | **Medium** | 48 14; сцена с «птицей» у тропы. **Визуально:** Forest, трава у 48 14. |
| HarveyOverhaulStory.E4_PierBreath | Beach | events.json | Data/Events/Beach | move, faceDirection, quickQuestion, exit move 0 -10 | **Medium** | 39 23; farmer идёт к пирсу. **Визуально:** пирс Beach, вода, край карты. |
| HarveyOverhaulStory.E4B_TooQuiet | Mountain | events.json | Data/Events/Mountain | move farmer +2, proceedPosition, faceDirection, quickQuestion | **Medium** | 44 21 — перила; 2 погодных триггера. **Визуально:** Mountain, вид на долину. |
| HarveyOverhaulStory.E5_StormBeside | Hospital | events.json | Data/Events/Hospital | move, proceedPosition, showFrame 55, startJittering, quickQuestion | **High** | 10 19; вход внутрь клиники во время грозы. **Визуально:** Hospital, коридор/рабочая зона. |
| HarveyOverhaulStory.E6_SayItOutLoud | Hospital | events.json | Data/Events/Hospital | move, proceedPosition, quickQuestion (ветки move farmer), faceDirection | **High** | 10 16; разговор у стола; ветки шаг ближе/назад. **Визуально:** Hospital. |
| HarveyOverhaulStory.E7_TownSip_Sunny | Town | events.json | Data/Events/Town | move, addTemporaryActor Penny, removeTemporarySprites, quickQuestion (ветки move) | **High** | 26 22; NPC Penny проходит через сцену. **Визуально:** Town, лавка, траектория Penny 24 22 → east. |
| HarveyOverhaulStory.E8_QuietShelf | ArchaeologyHouse | events.json | Data/Events/ArchaeologyHouse | move Gunther, warp Harvey 3 15, advancedMove Harvey, warp Gunther, quickQuestion (move Harvey) | **High** | 18 9; полки, Gunther уходит/возвращается. **Визуально:** музей, верхние полки, путь Harvey от входа. |
| HarveyOverhaulStory.E9_LightInWindow | Town (у клиники) | events.json | Data/Events/Town | quickQuestion (ветки warp Harvey, move farmer), ambientLight | **Medium** | 35 88; вечер у окна Hospital. **Визуально:** фасад клиники, ступени, освещение. |

---

### 4. Прочие relationship / onboarding

| Event ID | Локация | Файл | Asset/key | Команды движения/координат | Приоритет | Комментарий |
|---|---|---|---|---|---|---|
| eventHarveyFirstMeeting | BusStop | events.json + eventsCare.json | Data/Events/BusStop | move, faceDirection, question, fork (`declineFood`, `refuseCheckup`) | **Medium** | Дубликат в двух файлах (одинаковое содержимое). 19 23 / Harvey 27 23. **Визуально:** BusStop. |
| eventHarveyCheckup | ⚠ BusStop (target) | eventsCare.json | Data/Events/BusStop | move, showFrame 117, question, fork `irregularEating`, end position 10 17 | **High** | Координаты 5 9 / 1 5 похожи на **Hospital**, но патч идёт в **BusStop** — вероятная ошибка target. **Сначала проверить reachability и карту.** |
| eventHarveyMedicalCheck_Dating | Hospital | events.json | Data/Events/Hospital | warp farmer 10 19, move, advancedMove, viewport move, positionOffset, animate, quickQuestion | **High** | Отдельный ID от проверенного `eventHarveyMedicalCheck`. Сложный маршрут к койке 20 5. **Визуально:** Hospital (dating-вариант). |
| HarveyMod_NightCrisis_Dating | Hospital | events.json | Data/Events/Hospital | move farmer, faceDirection, move Harvey | **Medium** | 15 8; отдельно от проверенного `HarveyMod_NightCrisis`. **Визуально:** вечерняя клиника, кресло ~15 8. |
| HarveyMod_NightCrisis_PreDating | Hospital | events.json | Data/Events/Hospital | те же команды | **Medium** | Те же координаты, другой текст/тон. |
| HarveyMod_BirthdayHospital_Dating | Hospital | events.json | Data/Events/Hospital | move farmer 0 -4, globalFade | **Low** | 10 15; минимум движения. **Визуально:** приёмная Hospital. |
| HarveyMod_BirthdayHospital_Friend | Hospital | events.json | Data/Events/Hospital | move farmer 0 -4, globalFade | **Low** | Аналогично dating-варианту. |

**Fork-подсобытия FirstMeeting:** `declineFood`, `refuseCheckup`.  
**Fork Checkup:** `irregularEating`.

---

## Сводка по приоритетам

| Приоритет | Кол-во событий | Критерий |
|---|---:|---|
| **High** | 18 | warp / changeLocation / advancedMove / temporary actor / мульти-локация / сложная постановка |
| **Medium** | 14 | move + координаты, без тяжёлых варпов |
| **Low** | 2 | почти dialogue + один move |

---

## Уникальные карты для разбора

Список локations, которые нужно открыть, сверить с координатами событий и проверить коллизии, камеру и проходимость:

1. **Mine** — `eventHarveyMineRescue`, `eventHarveyMineRescueDating`, `eventHarveyMinorMineRescue`, `eventHarveyMineInterception`, `eventHarveyStormComfortMine` (старт)
2. **Hospital** — `HarveyMod_FirstTreatment`, `HarveyMod_NightCrisis_*`, `HarveyMod_BirthdayHospital_*`, `HarveyOverhaulStory.E2/E5/E6`, `eventHarveyMedicalCheck_Dating`, финалы mine-rescue и `eventRescueOperation`
3. **SkullCave** — `eventHarveySkullCavePrevention`
4. **Woods** — `eventRescueOperation` (сцена под кустами)
5. **Forest** — `eventHarveyStormComfortForest`, `HarveyOverhaulStory.E3/E3B`, финал `eventRescueOperation` (пикап)
6. **Custom_AdventurerSummit** — `eventHarveyStormComfortMountain` (акт 1)
7. **Mountain** — `eventHarveyStormComfortMountain` (акт 2), `HarveyOverhaulStory.E4B_TooQuiet`
8. **Town** — storm comfort (старт Town, финал Mine), `HarveyOverhaulStory.E2B/E7/E9`, `eventHarveyStormComfortMine` (финал)
9. **Saloon** — `eventHarveyStormComfortTown` (второй акт)
10. **Desert** — `eventHarveyStormComfortDesert`
11. **BusStop** — `eventHarveyFirstMeeting`, `HarveyOverhaulStory.E1_SlipperyPath`, ⚠ `eventHarveyCheckup` (если target не исправлен)
12. **Beach** — `HarveyOverhaulStory.E4_PierBreath`
13. **ArchaeologyHouse** — `HarveyOverhaulStory.E8_QuietShelf`

**Дополнительно при полном прогоне `eventRescueOperation`:** проверить спрайт `temporaryAnimatedSprite Maps\spring_town` на Forest 67 12 (машина/пикап).

---

## Рекомендуемый порядок проверки

1. **Mine + SkullCave** — C#-триггеры, коллизии лестниц, палата после fade.
2. **eventRescueOperation** — самая длинная цепочка локаций.
3. **Storm comfort (5 локаций)** — особенно Mountain (summit → mountain) и Town → Saloon.
4. **HarveyOverhaulStory E1–E9** — по порядку сюжета; E8 (музей) и E7 (Penny) — отдельное внимание актёрам.
5. **Onboarding** — `eventHarveyFirstMeeting`, затем **`eventHarveyCheckup`** (сначала подтвердить target BusStop vs Hospital).
6. **Dating-варианты** — `eventHarveyMedicalCheck_Dating`, `HarveyMod_NightCrisis_Dating/PreDating`, `HarveyMod_BirthdayHospital_*`.

---

## Заметки для тестировщика

- Story-события **E2B** и **E4B** имеют по два погодных триггера (Sunny/Wind) с **одним Event ID** — достаточно одной успешной проверки координат на вариант погоды.
- `eventHarveyMineRescue` и `eventHarveyMineRescueDating` взаимоисключающие; `eventHarveyMinorMineRescue` — отдельная ветка для dating/married.
- `HarveyMod_NightCrisis`, `HarveyMod_BirthdayHospital` (без суффикса) в списке проверенных; в этом отчёте остаются только **_Dating**, **_Friend**, **_PreDating** и отдельный **`eventHarveyMedicalCheck_Dating`**.
- При обнаружении рассинхрона target/координат (`eventHarveyCheckup`) — зафиксировать в баг-лист, но **не править** до отдельного задания.
