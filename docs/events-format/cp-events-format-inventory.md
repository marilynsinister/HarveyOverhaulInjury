# CP Events — инвентарь форматирования

Сводка по Content Patcher-файлам с event scripts в моде **HarveyOverhaul [CP]**.  
Источник CP: `D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code\`

**Дата аудита:** 2026-05-23  
**Связанные документы:** [events-inventory](../events-inventory/README.md) · [04-fork-subevents](../events-inventory/04-fork-subevents.md)

---

## Подключение через content.json

| Файл | Статус |
|---|---|
| `events.json` | ✅ Include |
| `eventsCare.json` | ✅ Include |
| `eventsMineRescue.json` | ✅ Include |
| `events_for_mode_new_formatted.json` | ❌ не подключён (черновик array-формата) |

В `events.json` есть **закомментированный** блок `Data/Events/FarmHouse` (`HarveyMod_MovingIn`, `HarveyMod_Nightmares`).

---

## 1. `events.json`

**Targets (14):**  
`Farm`, `Hospital`, `SeedShop`, `Woods`, `Mountain`, `Custom_AdventurerSummit`, `Town`, `BusStop`, `HarveyRoom`, `Forest`, `Beach`, `ArchaeologyHouse`, `Desert`, `Mine`

### Ключи по локациям

| Target | Event ID (до `/`) |
|---|---|
| **Farm** | `eventHarveyFirstWalk`, `acceptWalk`, `eventHarveyCheckHealthFarmer`, `eventHarveyCheckFarmerOutsideAfter22`, `eventHarveyMorningCheckup`, `eventHarveyStormComfortFarm` |
| **Hospital** | `HarveyOverhaulStory.E5_StormBeside`, `E6_SayItOutLoud`, `eventHarveyMedicalCheck_Dating`, `eventHarveyMedicalCheck`, `eventHarveyTraumaExam`, `eventHarveyTreatmentCollapse`, `eventStayInHospital`, `HarveyMod_TreatmentPlanMeeting`, `HarveyOverhaulStory.E2_InsistentExam`, `HarveyMod_FirstTreatment`, `HarveyMod_NightCrisis_Dating`, `HarveyMod_NightCrisis_PreDating`, `HarveyMod_BirthdayHospital_Dating`, `HarveyMod_BirthdayHospital_Friend` |
| **SeedShop** | `58` (vanilla 6-heart edit) |
| **Woods** | `eventRescueOperation` |
| **Mountain** | `eventHarveyMountainDate` |
| **Custom_AdventurerSummit** | `eventHarveyStormComfortMountain` |
| **Town** | `HarveyOverhaulStory.E7_TownSip_Sunny`, `eventHarveyLateNightCollapse`, `528013` (vanilla balloon edit), `eventHarveyStormComfortTown` |
| **BusStop** | `eventHarveyFirstMeeting`, `declineFood`, `refuseCheckup`, `HarveyOverhaulStory.E1_SlipperyPath` |
| **HarveyRoom** | `eventHarveyRoomCheckup`, `eventHarveyRoomCheckup2` |
| **Forest** | `HarveyOverhaulStory.E3_ForestApothecary`, `eventHarveyFirstDate`, `eventHarveyStormComfortForest` |
| **Beach** | `HarveyOverhaulStory.E4_PierBreath`, `eventHarveyPropose` |
| **ArchaeologyHouse** | `HarveyOverhaulStory.E8_QuietShelf` |
| **Desert** | `eventHarveyStormComfortDesert` |
| **Mine** | `eventHarveyStormComfortMine` |

**Формат:** многострочные JSON-строки, команды с `/` на отдельных строках.  
**Исключение:** блоки `quickQuestion` часто встроены **одной физической строкой** с `(break)` и `\\speak` / `\\message`.

---

## 2. `eventsCare.json`

**Targets (5):** `Farm`, `Hospital`, `BusStop`, `SkullCave`, `Mine`

| Target | Event ID |
|---|---|
| **Farm** | `eventHarveyFirstVisit`, `eventHarveySecondVisit` |
| **Hospital** | `eventHarveyEmergencyCare`, `eventHarveyExhaustion` |
| **BusStop** | `eventHarveyFirstMeeting`, `declineFood`, `refuseCheckup`, `eventHarveyCheckup`, `irregularEating` |
| **SkullCave** | `eventHarveySkullCavePrevention`, `HarveySkullPromise` |
| **Mine** | `eventHarveyMineInterception` |

**Дубликаты с `events.json` (BusStop):** `eventHarveyFirstMeeting`, `declineFood`, `refuseCheckup`.

---

## 3. `eventsMineRescue.json`

**Target:** `Data/Events/Mine`

| Event ID |
|---|
| `eventHarveyMineRescue` |
| `eventHarveyMinorMineRescue` |
| `eventHarveyMineRescueDating` |

*(В том же файле: `Data/Mail` → `mailHarveyAfterMineRescue` — не event.)*

**Формат:** весь script каждого события — **одна физическая строка JSON**, команды через `/`.

---

## 4. `events_for_mode_new_formatted.json` (черновик)

**Targets:** `Forest`, `Hospital`, `Farm`

| Target | Event ID |
|---|---|
| **Forest** | `MyMod_HarveyStormComfortForest` |
| **Hospital** | `MyMod_HarveyStressTiredCheck` |
| **Farm** | `MyMod_HarveyUrgentFarmVisit` |

**Формат:** массив строк (`"Entries": { "key": [ "cmd/", "(break)", ... ] }`).  
`quickQuestion` и `(break)` уже разнесены — эталон «нового» формата.

---

## Скрипты одной длинной строкой

### A. Весь event script — одна строка JSON

| Файл | Ключ | ~длина |
|---|---|---|
| `eventsMineRescue.json` | `eventHarveyMineRescue` | ~2380 |
| `eventsMineRescue.json` | `eventHarveyMineRescueDating` | ~2346 |
| `eventsMineRescue.json` | `eventHarveyMinorMineRescue` | ~1007 |

### B. Многострочное событие, но inline-блок на одной строке

**`quickQuestion` + `(break)` + `\\speak` / `\\message` в одной строке** — `events.json`:

| Ключ | ~длина строки |
|---|---|
| `acceptWalk` | ~1172 |
| `eventHarveyStormComfortFarm` | ~1130 |
| `eventHarveyMedicalCheck_Dating` | ~1202 |
| `eventHarveyMedicalCheck` | ~1210 |
| `HarveyMod_TreatmentPlanMeeting` | ~1068 |
| `eventHarveyStormComfortForest` | ~1031 |
| `eventHarveyStormComfortMine` | ~996 |
| `eventHarveyStormComfortDesert` | ~932 |
| `eventHarveyStormComfortTown` | ~928 |
| `eventHarveyStormComfortMountain` | ~883 |
| `eventRescueOperation` | ~600 |
| `HarveyOverhaulStory.E6_SayItOutLoud` | ~509 |
| `HarveyOverhaulStory.E4_PierBreath` | ~411 |
| `58` (SeedShop) | ~386 |
| `eventHarveyMorningCheckup` | ~329 |

**`eventsCare.json`:**

| Ключ | ~длина |
|---|---|
| `eventHarveyFirstVisit` | ~956 |
| `eventHarveySecondVisit` | ~789 |

**Короткие inline `quickQuestion` (<250 симв., но однострочные):**

- `eventHarveyCheckFarmerOutsideAfter22`
- `HarveyOverhaulStory.E5_StormBeside`

---

## Риски ручного форматирования

`choose` в активных CP-файлах **не найден**.

### Критичный — `quickQuestion` + `(break)` + много inline-команд

| Ключ | Файл | Причина |
|---|---|---|
| `acceptWalk` | events.json | 3 ветки, 6× speak + 3× message, вложенные кавычки |
| `eventHarveyMedicalCheck` / `_Dating` | events.json | ~13 speak в одной строке, длинные `(break)`-ветки |
| `HarveyMod_TreatmentPlanMeeting` | events.json | 3 ветки лечения |
| `eventHarveyStormComfort*` (6 локаций) | events.json | один паттерн inline QQ |
| `eventRescueOperation` | events.json | QQ + speak + action в одной строке |
| `eventHarveyFirstVisit`, `eventHarveySecondVisit` | eventsCare.json | QQ + 6 speak + 3 message |
| `HarveyOverhaulStory.E4_PierBreath` | events.json | QQ + `\\mail` в конце строки |
| `HarveyOverhaulStory.E5_StormBeside`, `E6_SayItOutLoud` | events.json | QQ с кавычками в prompt |

### Высокий — `question fork` + sub-event

| Основное событие | Sub-event | Файл |
|---|---|---|
| `eventHarveyFirstWalk` | `acceptWalk` (с QQ!) | events.json |
| `eventHarveyFirstMeeting` | `declineFood`, `refuseCheckup` | events.json + eventsCare.json |
| `eventHarveyCheckup` | `irregularEating` | eventsCare.json |
| `eventHarveySkullCavePrevention` | `HarveySkullPromise` | eventsCare.json |
| `HarveyOverhaulStory.E1_SlipperyPath` | fork (закоммент. `leaveHospital`) | events.json |

### Высокий — monolithic single-line

| Ключ | Файл |
|---|---|
| `eventHarveyMineRescue` | eventsMineRescue.json — 12+ speak |
| `eventHarveyMineRescueDating` | eventsMineRescue.json — 11+ speak |
| `eventHarveyMinorMineRescue` | eventsMineRescue.json — 5 speak |

### Средний — много диалогов, но уже с `/`-разбивкой

`eventHarveyCheckHealthFarmer`, `eventHarveyEmergencyCare`, `eventHarveyExhaustion`, `HarveyOverhaulStory.E2`–`E8`, `HarveyMod_NightCrisis_*`, `eventHarveyPropose`, `528013` — опаснее фрагменты с `$#`, `\n\n`, закомментированными `--animate` (напр. `eventHarveyStormComfortFarm`).

### Низкий — array-формат (черновик)

`MyMod_HarveyStormComfortForest`, `MyMod_HarveyStressTiredCheck` — `(break)` отдельными элементами; `MyMod_HarveyUrgentFarmVisit` — без QQ.

---

## Три формата в проекте

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Multiline string + "/" per line (events.json, care)      │
│    quickQuestion часто inline одной строкой                  │
├─────────────────────────────────────────────────────────────┤
│ 2. Single-line "/" script (eventsMineRescue.json)           │
├─────────────────────────────────────────────────────────────┤
│ 3. JSON array of strings (events_for_mode_new_formatted)    │
│    quickQuestion / (break) — отдельные элементы             │
└─────────────────────────────────────────────────────────────┘
```

---

## Приоритеты перед ручным форматированием

1. **`eventsMineRescue.json`** — три monolithic-строки (самый простой diff по объёму строк, но много escaped quotes).
2. **Inline `quickQuestion` в `events.json`** — storm-comfort серия, medical check, treatment plan, acceptWalk.
3. **`eventsCare.json`** — first/second visit (дублируют паттерн QQ из main events).
4. **Fork-цепочки** — не трогать ключи sub-event без проверки `fork` / `question forkN` в родителе.
5. **Черновик `events_for_mode_new_formatted.json`** — использовать как reference для target-формата, не как активный патч.

---

## Заметки

- Несколько ключей в одном `Entries` связаны через `end","nextKey":` или `end dialogue ...","nextKey":` — при разбиении строк не разрывать JSON-склейку между событиями.
- Sub-event ключи без preconditions (`acceptWalk`, `declineFood`, `refuseCheckup`, `HarveySkullPromise`, `irregularEating`) живут в том же `Entries`, что и родитель.
- Перед массовым reformat проверять дубликаты BusStop между `events.json` и `eventsCare.json`.
