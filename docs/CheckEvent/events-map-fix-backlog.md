# Backlog исправлений координат событий

Подготовлено по аудитам CheckEvent (май 2026). **CP и C# не изменялись** — только backlog для будущих правок.

**Исключены из backlog:** события, помеченные OK во всех отчётах без критики в story/storm/mine-аудитах — `eventHarveyMinorMineRescue`, `eventHarveyMineInterception`, `eventHarveyStormComfortForest`, `eventHarveyStormComfortMine` (базовая постановка), `HarveyOverhaulStory.E1`, `E2B`, `E3`, `E3B`, `E4B`, `E5` (без обязательных правок), а также все события из раздела «Исключены из проверки» в [`events-map-audit-plan.md`](events-map-audit-plan.md).

**Путь CP:** `D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code\`

---

## Critical

| Event ID | File | Location | Problem | Proposed fix | Why safe |
|----------|------|----------|---------|--------------|----------|
| `eventHarveyCheckup` | `eventsCare.json` | **BusStop** (ошибка) | Target `BusStop`, координаты Hospital `(2,5)/(1,5)/(5,9)` | Перенести патч в `Data/Events/Hospital` **или** полностью переписать coords под BusStop | Hospital-коords на BusStop = NPC в стенах; passport BusStop `(5,9)` Broken |
| `triggerHarveySkullCaveWarning` | `triggersCare.json` | Mine **или** SkullCave | `PLAYER_LOCATION_NAME Mine SkullCave` → `PlayEvent eventHarveySkullCavePrevention` (событие в **SkullCave**) | Убрать `Mine` из condition; оставить только `SkullCave` | На входе в Mine событие SkullCave не найдёт/сломает сцену ([`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md)) |
| `HarveyOverhaulStory.E2_InsistentExam` | `events.json` | Hospital | `doAction 5 9` на Buildings (дверь) | Убрать `doAction 5 9`; вход через `playSound doorOpen` + `move farmer` с `(10,19)` или warp `(10,19)` | Passport: `(5,9)` Buildings; проходимый вход `(10,19)` |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | `events.json` | Town | `Harvey 27 22` на Buildings | `Harvey 29 22` или `30 22` (4 тайла от farmer, как E2B) | Passport `(27,22)` Buildings; `(26,22)` farmer OK |
| `HarveyOverhaulStory.E8_QuietShelf` | `events.json` | ArchaeologyHouse | `warp Gunther 6 5` на Buildings | `warp Gunther 11 5` или `14 5` (проходимая зона за стеллажом) | Passport `(6,5)` Buildings + warp GuntherRoom |

---

## High

| Event ID | File | Location | Problem | Proposed fix | Why safe |
|----------|------|----------|---------|--------------|----------|
| `HarveyMod_FirstTreatment` | `events.json` | Hospital | `farmer 5 9` на Buildings (дверь) | `farmer 4 6` или `6 10` | Event ref в map-passports; `(5,9)` Buildings |
| `eventHarveyMedicalCheck_Dating` | `events.json` | Hospital | `doAction 10 13` на Buildings | Заменить на `move farmer 0 -2` + `faceDirection` без doAction на двери | `(10,13)` = Door Buildings |
| `eventHarveyMedicalCheck_Dating` | `events.json` | Hospital | `advancedMove` через `(12–17,10–13)` Broken | Упростить: короткие `move` по коридору `(10,15)→(15,8)` или fade к `(16,9)` | TMX path через мебель |
| `eventHarveyMedicalCheck_Dating` | `events.json` | Hospital | `warp farmer 20 5` без `ignoreCollisions` | Добавить `ignoreCollisions farmer/` + `positionOffset` + lying frame (как mine rescue) | Палата `(20,5)` Buildings by design с offset |
| `eventRescueOperation` | `events.json` | Hospital (финал) | `warp farmer 20 5` без палатного комплекса | `ignoreCollisions` + `positionOffset farmer 32 -52` + animate lying | Аналог `eventHarveyMineRescue` |
| `eventRescueOperation` | `events.json` | Hospital | `advancedMove Lewis` через `(15–19,7–14)` Broken | Заменить на 2–3 `move Lewis` по `(20,6)→(17,8)→(15,10)` или fade | TMX Hospital коридор |
| `HarveyOverhaulStory.E4_PierBreath` | `events.json` | Beach | Оба на `(39,13)` после `move 0 -10` | farmer `(39,13)`, Harvey `(40,13)` face 3/1 | Соседние тайлы, без overlap |
| `HarveyOverhaulStory.E8_QuietShelf` | `events.json` | ArchaeologyHouse | `advancedMove Harvey` → `(5,18)` через Broken | `warp Harvey 16 9` **или** `move 0 -2 4 0 0 1` без diagonals через `(4,16)` | Open area музея `(2,4)–(13,9)` |
| `HarveyOverhaulStory.E9_LightInWindow` | `events.json` | Town | Fork «пройти мимо»: `warp Harvey 35 88` = farmer tile | `warp Harvey 34 88` face 1 | Overlap на одном тайле |
| `eventHarveyStormComfortDesert` | `events.json` | Desert | `warp Harvey 17 26` на Buildings (bus) | `warp Harvey 18 24` (рядом с DesertBus) | Passport `(17,26)` Broken; `(15,23)` farmer OK |
| `eventHarveyStormComfortMountain` | `events.json` | Custom_AdventurerSummit | `advancedMove Harvey 0 -14 8 0` через скалы | Короткий path: warp `(41,28)` + `move 0 -3` + fade → Mountain | Summit passport рискованные тайлы `(34–37,33–38)` |
| `eventHarveyStormComfortTown` | `events.json` | Town | `advancedMove Harvey 0 1 1 0 0 17` — 17 тайлов, камера теряет пару | Сократить chase: Harvey warp `(39,73)` + `move 6 0` + fade → Saloon | Старт farmer `(39,73)` открытый юг |

---

## Medium

| Event ID | File | Location | Problem | Proposed fix | Why safe |
|----------|------|----------|---------|--------------|----------|
| `HarveyOverhaulStory.E2_InsistentExam` | `events.json` | Hospital | Exit `move Harvey/farmer 0 -2` → `(3,3)` Broken | Exit через `(4,6)` → `(10,19)` без `(3,3)` | Passport проходимость |
| `HarveyOverhaulStory.E2_InsistentExam` | `events.json` | Hospital | `Harvey 1 5` Front overlay | `Harvey 2 5` или `(3,6)` | Front на `(1,5)` Warning |
| `HarveyOverhaulStory.E8_QuietShelf` | `events.json` | ArchaeologyHouse | `warp Harvey 3 15` на warp Town | `warp Harvey 5 16` | `(3,15)` Warp→Town в passport |
| `HarveyOverhaulStory.E8_QuietShelf` | `events.json` | ArchaeologyHouse | `farmer 18 9` Front у витрины | `farmer 16 9` | Messages `(17–19,8)` — меньше overlay |
| `HarveyOverhaulStory.E9_LightInWindow` | `events.json` | Town | Нет `viewport` — фасад клиники может не попасть в кадр | `viewport 35 88 true` после setup | Passport E9 `(35,88)` |
| `HarveyOverhaulStory.E9_LightInWindow` | `events.json` | Town | Fork «окно»: `warp Harvey 36 89` | `warp Harvey 37 88` у крыльца | Южный фасад Hospital |
| `HarveyOverhaulStory.E5_StormBeside` | `events.json` | Hospital | `Harvey 10 18` Front | `Harvey 9 19` или `11 19` | `(10,18)` Front Warning |
| `HarveyOverhaulStory.E6_SayItOutLoud` | `events.json` | Hospital | Setup farmer только в ключе `10 16`, не в строке setup | Явно `farmer 10 19 0` в setup-строке | Стабильность при entry |
| `eventHarveyFirstMeeting` | `events.json` | BusStop | `farmer 19 23` Front | `farmer 20 23` (как E1) | Passport Front Warning |
| `HarveyMod_NightCrisis_Dating` | `events.json` | Hospital | Координаты `(15,8)` Front | In-game verify; при необходимости `(14,8)` | Warning в coordinate-audit |
| `HarveyMod_NightCrisis_PreDating` | `events.json` | Hospital | Аналогично Dating | In-game verify | Warning |
| `eventHarveyStormComfortMountain` | `events.json` | Mountain act 2 | `advancedMove` → `(84,7)`/`(83,8)`; `(81,3)` Broken | Финал `(77,15)` viewport + статичная сцена без long advancedMove | Край карты `(79,1)` warp |
| `eventHarveyMineRescue` / Dating | `eventsMineRescue.json` | Mine | Warning на Hospital `(19,5)` Front | In-game verify lying scene; без смены coords если OK | ignoreCollisions уже в скрипте |
| `eventRescueOperation` | `events.json` | Woods | `warp farmer 27 18` Front | `warp farmer 28 18` | Passport Front Warning |

---

## Low

| Event ID | File | Location | Problem | Proposed fix | Why safe |
|----------|------|----------|---------|--------------|----------|
| `eventHarveyStormComfortMine` | `events.json` | Mine | Harvey `warp 18 13` + long `move 0 -8` у warp Summit | Harvey `warp 17 10` + `move 0 -2` (как mine rescue) | [`storm-comfort-map-audit.md`](storm-comfort-map-audit.md) optional |
| `eventHarveyStormComfortForest` | `events.json` | Forest | Нет viewport; слабый canopy | `viewport 23 13 true` | Visual polish |
| `HarveyMod_BirthdayHospital_Dating/Friend` | `events.json` | Hospital | Front на setup tiles | In-game only | Low priority |
| `HarveyOverhaulStory.E3B_WingPatient` | `events.json` | Forest | Fork «отойти» — только message | Опционально `move farmer 0 2 2` в fork | Visual polish |
| `eventHarveySkullCavePrevention` | `eventsCare.json` | SkullCave | Координаты OK | **Не менять** coords; только trigger (Critical) | coords `(5,5)/(7,7)` passport OK |

---

## Инфраструктура (не CP-координаты, но блокирует события)

| ID | File | Problem | Proposed fix | Priority |
|----|------|---------|--------------|----------|
| Volcano combat death | `PassOutHandler.cs` | `NeedsMineRescueEvent` не ставится в `VolcanoDungeon` | Добавить `VolcanoDungeon` в детекцию HP≤0 | **High** |
| `eventHarveySkullCavePrevention` trigger | `triggersCare.json` | см. Critical table | см. Critical | **Critical** |

---

## Детальные блоки по событиям

---

## eventHarveyCheckup

### Проблема

Патч в `Data/Events/BusStop`, но все координаты и viewport `(5,9)` — из **Hospital**. На BusStop `(2,5)`, `(1,5)`, `(10,17)` — Buildings. Сцена технически не может корректно проиграться.

### Где править

- **Файл:** `assets/Code/eventsCare.json`
- **Target:** `Data/Events/BusStop` → **`Data/Events/Hospital`**
- **Ключ:** `eventHarveyCheckup`
- **Фрагменты:** setup `farmer 2 5` / `Harvey 1 5`; viewport `5 9`; `end position 10 17`

### Предложение

**Вариант A (рекомендуется):** перенести entry в `Data/Events/Hospital`; заменить setup на passport-safe:
- `farmer 4 6 0` или `10 19 0`
- `Harvey 4 5 2` или `10 18 2`
- viewport `10 15` или `14 6`

**Вариант B:** оставить BusStop и заменить все coords на BusStop E1-зону: farmer `(20,23)`, Harvey `(26,22)`.

### Почему это безопаснее

Passport BusStop: `(5,9)` и `(2,5)` — Buildings. Hospital `(4,6)`, `(10,19)` — проходимы ([`map-passports.md`](map-passports.md)).

### Что проверить в игре

- Триггер checkup (injury + relationship)
- NPC не в стене; осмотр у кушетки/входа
- `end position` — farmer на проходимом тайле

---

## HarveyMod_FirstTreatment

### Проблема

`setup farmer 5 9` — тайл двери Hospital (Buildings). `move farmer 0 -1 2` → `(5,4)` тоже рискованно.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Hospital`
- **Ключ:** `HarveyMod_FirstTreatment`
- **Фрагмент:** `farmer 5 9 2` в setup-строке; `move farmer 0 -1 2`

### Предложение

- Заменить setup: `farmer 4 6 2` (кушетка) или `6 10 2`
- Harvey оставить `4 5` или сдвинуть на `5 5`
- Убрать/заменить `move farmer 0 -1 2` на `move farmer 0 1 2` к кушетке

### Почему это безопаснее

Event ref `(4,6)` и `(6,10)` в map-passports — Back OK, Buildings=0.

### Что проверить в игре

- Первое лечение после injury
- Farmer у кушетки, не в дверном проёме
- Harvey подходит без застревания

---

## HarveyOverhaulStory.E2_InsistentExam

### Проблема

1. `doAction 5 9` — action на Buildings.  
2. Exit `move 0 -2 true` через `(3,3)`.  
3. (Minor) Harvey `(1,5)` Front.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Hospital`
- **Ключ:** `HarveyOverhaulStory.E2_InsistentExam`
- **Фрагменты:** `doAction 5 9/`; setup `Harvey 1 5 2 farmer 5 10 0`; финал `move Harvey 0 -2 0 true` / `move farmer 0 -2 0 true`

### Предложение

1. **Удалить** `doAction 5 9/`; добавить `playSound doorOpen/` + farmer уже внутри через setup `(5,10)` или warp `(10,19)` + `move 0 -4 3`.
2. Exit: `move farmer 0 2 3` → `(5,12)` → `move 0 3 2` → `(10,19)` (без `(3,3)`).
3. Harvey setup: `Harvey 2 5 2`.
4. Посадка на кушетке: заменить `showFrame farmer 107` → `stopAnimation` + `faceDirection` + `showFrame farmer true 117` после `move` на final tile (см. [`cp-event-authoring-rules.md`](cp-event-authoring-rules.md)).

### Почему это безопаснее

Скрипт уже использует `farmer 5 10` — логика осмотра у кушetки; `(5,9)` doAction — единственный Critical blocker.

### Что проверить в игре

- После E1, день, Hospital entry
- Осмотр на кушетке: `faceDirection farmer 2` + `showFrame farmer 107`; оба quickQuestion без движения
- Выход к двери `(10,20)` без застревания

---

## HarveyOverhaulStory.E7_TownSip_Sunny

### Проблема

`Harvey 27 22` — Buildings. Реплики про лавку/бутылку при Harvey в декоре.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Town`
- **Ключ:** `HarveyOverhaulStory.E7_TownSip_Sunny`
- **Фрагмент:** `farmer 26 22 1 Harvey 27 22 3/`

### Предложение

- `farmer 26 22 1 Harvey 29 22 3` (4 тайла, как E2B `28/32`)
- Penny temp `(32,24)` — без изменений

### Почему это безопаснее

Passport `(27,22)` Buildings; `(26,22)` и `(29,22)` Back OK ([`story-arc-map-audit.md`](story-arc-map-audit.md)).

### Что проверить в игре

- Sunny 12:00–15:00, после E6
- Harvey стоит на площади, не в лавке
- Penny проходит мимо без блокировки

---

## HarveyOverhaulStory.E8_QuietShelf

### Проблема

1. `warp Gunther 6 5` — Buildings.  
2. Harvey `advancedMove` через полки.  
3. `warp Harvey 3 15` — warp-тайл Town.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/ArchaeologyHouse`
- **Ключ:** `HarveyOverhaulStory.E8_QuietShelf`
- **Фрагменты:** `warp Gunther 6 5/`; `warp Harvey 3 15/`; `advancedMove Harvey false 0 -2 4 0...`

### Предложение

1. `warp Gunther 11 5` (return) вместо `(6,5)`.
2. `warp Harvey 16 9` вместо `(3,15)` + убрать длинный advancedMove **или** заменить на:
   ```
   move Harvey -11 0 3 true
   ```
   от `(16,9)` к farmer.
3. farmer `(16,9)` вместо `(18,9)` при необходимости.

### Почему это безопаснее

Open area `(2,4)–(13,9)` в passport ArchaeologyHouse; `(6,5)` явно Broken.

### Что проверить в игре

- Суббота 10–16, после E7
- Gunther уходит/возвращается не из стены
- Harvey подходит к полке; quickQuestion без застревания

---

## HarveyOverhaulStory.E9_LightInWindow

### Проблема

1. Fork overlap Harvey/farmer на `(35,88)`.  
2. Нет viewport — фасад клиники может не читаться.  
3. `Harvey -1000 -1000` в setup — **не баг** (скрытие до QQ); **не менять**.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Town`
- **Ключ:** `HarveyOverhaulStory.E9_LightInWindow`
- **Фрагменты:** fork `warp Harvey 35 88`; добавить после setup `viewport 35 88 true`

### Предложение

- Fork «пройти мимо»: `warp Harvey 34 88` + `faceDirection Harvey 1`
- Fork «окно/стучать»: `warp Harvey 37 88`
- После `ambientLight`: `viewport 35 88 true`

### Почему это безопаснее

`(35,88)` — passport Event ref E9, Back OK; соседние тайлы без Buildings.

### Что проверить в игре

- 20:00–23:30, после E8
- Видно окно клиники со светом
- Нет двойного спрайта на одном тайле
- Все 4 ветки первого quickQuestion

---

## HarveyOverhaulStory.E4_PierBreath

### Проблема

После `move farmer/Harvey 0 -10 true` оба на `(39,13)` — overlap спрайтов.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Beach`
- **Ключ:** `HarveyOverhaulStory.E4_PierBreath`
- **Фрагмент:** `move farmer 0 -10 0 true` / `move Harvey 0 -10 0 true` (оба к `(39,13)`)

### Предложение

- Оставить farmer `(39,13)`
- Изменить Harvey move на `(40,13)`:
  - либо отдельный `move Harvey -1 0 3` после farmer
  - либо `move Harvey 0 -10 0` только до `(40,13)` (если старт `(40,23)` — скорректировать второй шаг)

### Почему это безопаснее

Beach `(39,13)` и `(40,13)` оба Back OK; face 3/1 — диалог у воды.

### Что проверить в игре

- После E3B, вечер, солнце
- Дыхательная сцена: два силуэта рядом, не в одной точке
- quickQuestion «сжать руку» — move `-1,0` всё ещё логичен

---

## eventHarveyMedicalCheck_Dating

### Проблема

1. `doAction 10 13` на двери.  
2. Длинные `advancedMove` через мебель.  
3. `warp farmer 20 5` без палатного комплекса.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Hospital`
- **Ключ:** `eventHarveyMedicalCheck_Dating`
- **Фрагменты:** `doAction 10 13/`; блоки `advancedMove Harvey` / `advancedMove farmer`; `warp farmer 20 5`

### Предложение

1. Убрать `doAction 10 13`; farmer уже `warp 10 19` — достаточно `move 0 -2`.
2. Заменить advancedMove на:
   ```
   move Harvey 0 -2 0
   move farmer 0 -3 0
   globalFade
   warp farmer 16 9
   warp Harvey 17 9
   ```
   (осмотр) **или** fade → палата:
   ```
   ignoreCollisions farmer
   warp farmer 20 5
   positionOffset farmer 32 -52
   animate farmer true true 10000 4 5
   ```
3. Сократить viewport move если остаётся.

### Почему это безопаснее

Проверенный `eventHarveyMedicalCheck` (manual OK) — сверить coords с dating-веткой. Passport `(16,9)`, `(10,19)` проходимы.

### Что проверить в игре

- Dating, mail reminder, sunny 14–18
- Нет прохода сквозь койки
- Сцена на койке с lying frame

---

## eventRescueOperation

### Проблема

1. Финал `warp farmer 20 5` — Buildings без ignoreCollisions.  
2. Lewis `advancedMove` через коридор с Broken tiles.  
3. (Minor) Front на Woods `(27,18)`.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Woods` (мульти-локация внутри скрипта)
- **Ключ:** `eventRescueOperation`
- **Фрагменты:** блок Hospital finale; `advancedMove Lewis false 0 2 -4 0...`

### Предложение

1. Перед `warp farmer 20 5`:
   ```
   ignoreCollisions farmer/
   positionOffset farmer 32 -52/
   animate farmer true true 10000 4 5/
   ```
2. Lewis: заменить advancedMove на `move Lewis 0 2 0` → `move Lewis -4 0 0` → `move Lewis 0 -1 0` (проверить по TMX) **или** `globalFade` + warp Lewis `(17,8)`.
3. Woods: `warp farmer 28 18` вместо `(27,18)`.

### Почему это безопаснее

Тот же паттерн палаты, что `eventHarveyMineRescue` ([`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md)). Forest/Woods warps `(66,16)` OK.

### Что проверить в игре

- `topicRescueOperation` + storm + Woods entry (после E5)
- Поиск в лесу, машина Forest, палата
- Lewis не идёт сквозь стены

---

## eventHarveyStormComfortDesert

### Проблема

`warp Harvey 17 26` — Buildings (кузов автобуса).

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Desert`
- **Ключ:** `eventHarveyStormComfortDesert`
- **Фрагмент:** `warp Harvey 17 26/`

### Предложение

- `warp Harvey 18 24` + `faceDirection Harvey 2`
- `move Harvey 0 -1 3` → `(18,23)` рядом с farmer `(15,23)`

### Почему это безопаснее

Desert passport: `(15,23)` OK; `(17,26)` Broken; bus action `(18,27)`.

### Что проверить в игре

- Storm + Desert entry + stress thunder
- Harvey рядом с bus, не внутри спрайта
- Диалог «нет укрытий» на фоне пустыни

---

## eventHarveyStormComfortMountain

### Проблема

Act 1: длинный `advancedMove` с Summit через скалы. Act 2: advancedMove на Mountain с Broken `(81,3)`.

### Где править

- **Файл:** `assets/Code/events.json`
- **Targets:** `Custom_AdventurerSummit`, затем `Mountain` (в одном ключе)
- **Ключ:** `eventHarveyStormComfortMountain`
- **Фрагменты:** `advancedMove Harvey false 0 -14 8 0/`; act 2 `advancedMove farmer/Harvey`

### Предложение

**Act 1:** после реплики «немедленно вниз» — `globalFade` без diagonal path; или Harvey `move 0 -5 3` + fade (farmer `(41,28)` уже OK).

**Act 2:** после warp `(79,1)` — `viewport 76 15 true`; **убрать** advancedMove; статичная сцена + speak у склона.

### Почему это безопаснее

Summit `(41,28)` и Mountain viewport `(76,15)` — narrativ сильные ([`storm-comfort-map-audit.md`](storm-comfort-map-audit.md)); path — слабое звено.

### Что проверить в игре

- Storm + Summit entry
- Fade на Mountain без застревания
- Видны склон/озеро в act 2

---

## eventHarveyStormComfortTown

### Проблема

Harvey warp `(36,56)` OK, но `advancedMove 0 1 1 0 0 17` — 17 тайлов юг; farmer `(39,73)` открытое место; камера теряет action.

### Где править

- **Файл:** `assets/Code/events.json`
- **Target:** `Data/Events/Town` (+ Saloon в скрипте)
- **Ключ:** `eventHarveyStormComfortTown`
- **Фрагмент:** `warp Harvey 36 56/` … `advancedMove Harvey false 0 1 1 0 0 17/`

### Предложение

- Сократить: после `warp Harvey 36 56` → `speed Harvey 6` → `advancedMove Harvey false 0 17` **без** лишних leg (или warp Harvey `(39,72)`)
- Альтернатива: `globalFade` сразу после первой реплики → `changeLocation Saloon` (как финал уже делает)

### Почему это безопаснее

Saloon act 2 `(14,23)` — сильное укрытие; act 1 chase — слабое звено ([`storm-comfort-map-audit.md`](storm-comfort-map-audit.md)).

### Что проверить в игре

- Storm + Town south + stress
- Harvey выбегает из Hospital
- Saloon interior с Gus виден

---

## triggerHarveySkullCaveWarning (triggersCare.json)

### Проблема

При входе в **Mine** с injury-buffs запускается **SkullCave**-событие — wrong location.

### Где править

- **Файл:** `assets/Code/triggersCare.json`
- **Id:** `{{ModId}}_triggerHarveySkullCaveWarning`
- **Condition:** `PLAYER_LOCATION_NAME Current Mine SkullCave` → **`PLAYER_LOCATION_NAME Current SkullCave`**

### Предложение

Только SkullCave. Для Mine уже есть `triggerHarveyMineWarning` → `eventHarveyMineInterception`.

### Почему это безопаснее

[`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md): SpaceCore PlayEvent требует совпадения локации патча.

### Что проверить в игре

- Injury + enter **Mine** → interception, **не** skull event
- Injury + enter **SkullCave** → prevention `(5,5)/(7,7)`

---

## Сводка: порядок работ (рекомендуемый)

1. **Critical:** `eventHarveyCheckup` target · E2 doAction · E7 Harvey · E8 Gunther warp · skull trigger  
2. **High:** FirstTreatment · MedicalCheck_Dating · RescueOperation finale · E4/E8/E9 · Storm Desert/Mountain/Town  
3. **Medium:** E2 exit · E5/E6 polish · NightCrisis verify · Storm Mountain act2  
4. **Low:** Storm Mine/Forest viewport · Birthday verify  

---

## Связанные документы

- [`events-coordinate-audit.md`](events-coordinate-audit.md)
- [`story-arc-map-audit.md`](story-arc-map-audit.md)
- [`storm-comfort-map-audit.md`](storm-comfort-map-audit.md)
- [`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md)
- [`map-passports.md`](map-passports.md)
- [`events-map-audit-plan.md`](events-map-audit-plan.md)
