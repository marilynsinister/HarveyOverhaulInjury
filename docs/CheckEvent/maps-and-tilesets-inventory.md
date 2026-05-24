# Инвентаризация карт и tileset’ов

Отчёт для проверки координат событий из [`events-map-audit-plan.md`](events-map-audit-plan.md).  
Технические паспорта карт: [`map-passports.md`](map-passports.md).  
Справочник tileset’ов для сцен: [`tileset-reuse-guide.md`](tileset-reuse-guide.md).  
Аудит координат событий: [`events-coordinate-audit.md`](events-coordinate-audit.md).  
Риски шахтных/пещерных событий: [`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md).  
Storm comfort (укрытия от грозы): [`storm-comfort-map-audit.md`](storm-comfort-map-audit.md).  
Story arc E1–E9: [`story-arc-map-audit.md`](story-arc-map-audit.md).  
Backlog исправлений: [`events-map-fix-backlog.md`](events-map-fix-backlog.md).

**Метод:** поиск `.tmx`/`.tbin` в репозитории и модах; сверка с `Load`/`EditMap` в SVE CP; частичный разбор tileset из TMX.  
**Не менялось:** события, `content.json`.

---

## Контекст окружения

| Источник | Карты в репозитории | Патчи карт |
|---|---|---|
| **HarveyOverhaulInjury** | `tmpMap/Mine.tmx`, `tmpMap/Hospital.tmx` (экспорт для анализа) | нет |
| **HarveyOverhaul [CP]** | нет `.tmx`/`.tbin` | нет `Load`/`EditMap` карт (только спрайты персонажей) |
| **Vanilla** | `Content/Maps/*.xnb` (в игре) | — |
| **Stardew Valley Expanded (SVE)** | `[CP] SVE/assets/Maps/**`, `assets/maps/locations/**` | **заменяет** многие vanilla-карты через `Load`; сотни `EditMap` |

**Важно для тестов:** если в профиле установлен **SVE**, координаты событий нужно проверять на **runtime-карте после всех CP-патчей SVE**, а не на чистом vanilla `.xnb`.  
Документы `tmpMap/MAP_ANALYSIS_VERIFICATION.md`, `Mine_event_placement_analysis.md`, `Hospital_event_placement_analysis.md` уже опираются на экспортированные TMX.

**Не входят в scope audit-плана:** `MineShaft` / процедурные этажи шахты — все события используют локацию **`Mine`** (вход у гор), не подземные уровни.

---

## Карты

| LocationName | Map asset / file | Source | В репозитории проекта | Нужно экспортировать? | Используется событиями (из audit) |
|---|---|---|---|---|---|
| **Mine** | `Maps/Mine` | **SVE** `Assets/maps/locations/Mine.tbin` (Load заменяет vanilla) | `tmpMap/Mine.tmx` (77×20) | Да, если нет актуального runtime-экспорта под ваш save/SVE-конфиг | `eventHarveyMineRescue`, `eventHarveyMineRescueDating`, `eventHarveyMinorMineRescue`, `eventHarveyMineInterception`, `eventHarveyStormComfortMine` (акт 1) |
| **Hospital** | `Maps/Hospital` | **SVE** `Assets/Maps/Locations/Hospital.tbin` | `tmpMap/Hospital.tmx` (24×20) | Да (runtime после SVE Load) | `HarveyMod_FirstTreatment`, `HarveyMod_NightCrisis_*`, `HarveyMod_BirthdayHospital_*`, `HarveyOverhaulStory.E2/E5/E6`, `eventHarveyMedicalCheck_Dating`, финалы mine-rescue, `eventRescueOperation` (финал) |
| **SkullCave** | `Maps/SkullCave` | **Vanilla** `Content/Maps/SkullCave.xnb`; SVE только **EditMap** (смена Warp) | нет | Да (`debug export current` на входе Skull Cave) | `eventHarveySkullCavePrevention` |
| **Woods** | `Maps/Woods` | **SVE** `Assets/maps/locations/Woods2.tmx` (Load) | нет | Да | `eventRescueOperation` (сцена 27 18 / 40 20) |
| **Forest** | `Maps/Forest` | **SVE** `Assets/maps/locations/Forest.tmx` + патчи | нет | Да | `eventHarveyStormComfortForest`, `HarveyOverhaulStory.E3/E3B`, `eventRescueOperation` (пикап 66 16) |
| **Custom_AdventurerSummit** | `Maps/Custom_AdventurerSummit` | **SVE only** `assets/Maps/NewLocations/AdventurerSummit.tmx` (65×43) + EditMap | нет (есть в папке SVE) | Да (из игры или напрямую TMX SVE) | `eventHarveyStormComfortMountain` (акт 1: 41 27, Harvey 32 42) |
| **Mountain** | `Maps/Mountain` | **SVE** `Assets/Maps/Locations/Mountain.tmx` (135×41) + патчи | нет | Да | `eventHarveyStormComfortMountain` (акт 2: 79 1), `HarveyOverhaulStory.E4B_TooQuiet` (44 21) |
| **Town** | `Maps/Town` | **SVE** `Assets/Maps/Locations/Town.tmx` или `Town_Joja.tmx` + патчи | нет | Да | `eventHarveyStormComfortTown` (39 73), `HarveyOverhaulStory.E2B/E7/E9`, `eventHarveyStormComfortMine` (72 22) |
| **Saloon** | `Maps/Saloon` | **SVE** `Assets/maps/locations/Saloon.tbin` | нет | Да | `eventHarveyStormComfortTown` (акт 2: 14 23) |
| **Desert** | `Maps/Desert` | **SVE** `Assets/maps/locations/Desert.tbin` | нет | Да | `eventHarveyStormComfortDesert` (15 23) |
| **BusStop** | `Maps/BusStop` | **SVE** `Assets/maps/locations/BusStop.tmx` + патчи | нет | Да | `eventHarveyFirstMeeting`, `HarveyOverhaulStory.E1_SlipperyPath`; ⚠ `eventHarveyCheckup` (target BusStop, coords как Hospital) |
| **Beach** | `Maps/Beach` | **SVE** `Assets/maps/locations/Beach.tmx` + патчи | нет | Да | `HarveyOverhaulStory.E4_PierBreath` (39 23) |
| **ArchaeologyHouse** | `Maps/ArchaeologyHouse` | **SVE** `Assets/maps/locations/ArchaeologyHouse.tbin` | нет | Да | `HarveyOverhaulStory.E8_QuietShelf` (18 9) |

### Дополнительно (не локация, но нужно для координат)

| Asset | Тип | Source | Нужно для |
|---|---|---|---|
| `LooseSprites/Cursors` (region 0,1810 — spring_town truck) | temporaryAnimatedSprite | **Vanilla** | `eventRescueOperation` — спрайт машины на Forest **67 12** |
| `Maps/spring_town` (tilesheet reference в команде события) | sprite sheet ref | Vanilla seasonal | тот же кадр пикапа в rescue |

---

## Tileset’ы

Легенда: **External TSX?** — в TMX tileset встроен (`<image source=…>` внутри map), отдельного `.tsx` нет. Пути `source` — логические имена tilesheet’ов игры/SVE (резолвятся в `Content/Maps/*.xnb` или PNG SVE).

### Карты с TMX в проекте / доступные для разбора

| Map | Tileset name | Image source | External TSX? | Назначение / заметки |
|---|---|---|---|---|
| **Mine** (`tmpMap/Mine.tmx`) | paths | `paths` | нет | NPC paths, дорожки |
| | untitled tile sheet | `Mines\mine` | нет | Vanilla mine entrance tiles (стены, пол, лифт) |
| **Hospital** (`tmpMap/Hospital.tmx`) | 1 | `townInterior` | нет | Интерьер клиники (пол, стены, мебель) |
| | p | `paths` | нет | Paths |
| | v15_TownInterior_2 | `townInterior_2` | нет | Расширенный interior sheet (кушетки, двери) |
| **Custom_AdventurerSummit** (SVE TMX) | outdoors | `spring_outdoorsTileSheet` | нет | Базовый outdoor (сезонный в runtime) |
| | Paths | `paths` | нет | |
| | zspring_town | `spring_town.png` | нет | SVE town overlay |
| | untitled tile sheet2 | `spring_outdoorsTileSheet2` | нет | |
| | zspring_z_extras | `spring_z_extras.png` | нет | SVE extras |
| | zspring_beach | `spring_beach.png` | нет | |
| | v16_Waterfalls | `spring_Waterfalls` | нет | Водопады / склон |
| | spring_Shadows | `spring_Shadows.png` | нет | Тени |
| | zspring_SVE_Tilesheet2 | `spring_SVE_Tilesheet2.png` | нет | **Кастом SVE** — уникальные тайлы локации |

### SVE replacement maps (external к репозиторию, tilesets из TMX SVE)

| Map | Основные tileset names | Image sources (типовой набор) | External TSX? | Заметки |
|---|---|---|---|---|
| **Mountain** | outdoors, untitled tile sheet2, Paths, v16_Shadows, v16_Waterfalls, zspring_z_extras, winter_outdoorsTileSheet, zspring_SVE_Tilesheet2, zspring_town | `spring_outdoorsTileSheet`, `spring_outdoorsTileSheet2.png`, `paths`, `spring_Shadows`, `spring_Waterfalls`, `spring_z_extras.png`, `winter_outdoorsTileSheet.png`, `spring_SVE_Tilesheet2.png`, `spring_town.png` | нет | E4B **44 21** — перила/обрыв; storm comfort warp **79 1** — SVE warp с AdventurerSummit |
| **Forest** | Paths, zspring_town, zspring_beach, zspring_z_extras, outdoors, z_SVEbuildingShadow, zGrandpasFarm_* shadows, z_outdoors2, … (+4) | vanilla outdoors + **SVE shadow/canopy** sheets | нет | E3 **50 13**, E3B **48 14**, storm **23 13**, rescue **66 16** |
| **BusStop** | outdoors, Paths, spring_town, v16_outdoorsTileSheet2, zspring_SVE_Tilesheet2 | `spring_outdoorsTileSheet`, `paths`, `spring_town`, `spring_outdoorsTileSheet2`, `spring_SVE_Tilesheet2.png` | нет | First meeting **19 23**, E1 **52 24** viewport |
| **Woods** | untitled tile sheet, Paths, zspring_z_extras, zspring_town, zspring_RedBaneberry_Tilesheet, v16_Waterfalls, v16_Outdoors2, zspring_island_tilesheet_1, v16_Shadows | outdoors + **SVE RedBaneberry**, waterfalls | нет | Rescue **27 18**, **40 20** |
| **Beach** | Paths, untitled tile sheet, v16_Shadows, zspring_town | `paths.png`, `spring_beach.png`, `spring_Shadows`, `spring_town.png` | нет | E4 pier **39 23** |
| **Town** | (в `Town.tmx` / `Town_Joja.tmx`) | `spring_town`, outdoors, paths, SVE town sheets | нет | Большая карта; E2B **28 67**, E7 **26 22**, E9 **35 88**, storm **39 73**, mine-exit **72 22** |
| **Saloon** | (в `Saloon.tbin`) | `townInterior` / saloon interior (vanilla family) | n/a (binary) | Storm comfort интерьер **14 23** |
| **Desert** | (в `Desert.tbin`) | desert tilesheet, paths | n/a (binary) | Storm **15 23** |
| **ArchaeologyHouse** | (в `ArchaeologyHouse.tbin`) | museum interior (`townInterior` family) | n/a (binary) | E8 полки **18 9** |
| **Hospital** | (в `Hospital.tbin`) | `townInterior`, `townInterior_2`, paths | n/a (binary) | Совпадает с экспортом `tmpMap/Hospital.tmx` |
| **Mine** | (в `Mine.tbin`) | `Mines\mine`, paths | n/a (binary) | Совпадает с экспортом `tmpMap/Mine.tmx` |
| **SkullCave** | vanilla mine/skull sheets | `Mines\mine`, lava variants | n/a | Экспорт из игры; SVE меняет только Warp properties |

### Vanilla-only tileset families (если тест **без SVE**)

| Семейство | Типичные image source | Карты |
|---|---|---|
| Seasonal outdoors | `{season}_outdoorsTileSheet`, `{season}_outdoorsTileSheet2` | BusStop, Forest, Mountain, Woods, Desert (частично) |
| Town | `{season}_town` | Town, части Forest/Beach |
| Beach | `{season}_beach` | Beach |
| Interiors | `townInterior`, `townInterior_2` | Hospital, Saloon, ArchaeologyHouse, HarveyRoom |
| Mines | `Mines\mine`, `lava_tiles` | Mine, SkullCave |
| Paths | `paths` | почти все outdoor/interior |
| Loose sprites | `LooseSprites/Cursors`, `Maps/spring_town` | temporaryAnimatedSprite (rescue) |

---

## Что нужно получить дополнительно

Файлы, которых **нет в HarveyOverhaulInjury/HarveyOverhaul [CP]**, но они нужны для полной проверки координат:

### Обязательный минимум (13 локаций audit)

1. **Runtime export** всех 13 локаций из **вашего тестового save** (SVE + прочие моды как в [`docs/testing/FOR_TEST.md`](../testing/FOR_TEST.md)):
   - `Mine`, `Hospital`, `SkullCave`, `Woods`, `Forest`, `Custom_AdventurerSummit`, `Mountain`, `Town`, `Saloon`, `Desert`, `BusStop`, `Beach`, `ArchaeologyHouse`
   - Способ: SMAPI `debug export current` standing in location, или Content Patcher / xnbcli unpack `Content/Maps/<Name>.xnb`, или копия SVE TMX/tbin + сверка в игре.

2. **SVE base maps** (если не экспортируете из игры):
   - `Stardew Valley Expanded/[CP] SVE/assets/Maps/NewLocations/AdventurerSummit.tmx`
   - `.../assets/Maps/Locations/Mountain.tmx`
   - `.../assets/maps/locations/Mine.tbin`
   - `.../assets/Maps/Locations/Hospital.tbin`
   - `.../assets/maps/locations/Forest.tmx`, `Woods2.tmx`, `BusStop.tmx`, `Beach.tmx`
   - `.../assets/maps/locations/Saloon.tbin`, `Desert.tbin`, `ArchaeologyHouse.tbin`
   - `.../assets/Maps/Locations/Town.tmx` **или** `Town_Joja.tmx` (зависит от CC/Joja)

3. **SkullCave.xnb** — vanilla `Content/Maps/SkullCave.xnb` (SVE не делает Load, только warp patches).

### Условные патчи (проверять, если активны в save)

| Условие | Карта | Файл патча (SVE) | Риск для координат |
|---|---|---|---|
| `OriginalMinesEntrance` | Custom_AdventurerSummit, Mountain | `OriginalMineLocation_*.tbin`, смена Warp | Меняет вход шахты / warp **79 1** |
| Event 1000077 (railroad) | Custom_AdventurerSummit | `AdventurerSummit_Railroad_Shortcut.tmx` | Лестница/мост слева |
| CC vs Joja (5553210) | Town, Mountain, Forest | `Town_Joja.tmx`, `Mountain_Joja.tmx`, … | Другая планировка Town |
| `HasMod: maxvollmer.deepwoodsmod` | Woods | `DeepWoodsCompatibility.tmx` | Расширение Woods |
| `HasMod: flashshifter.GrampletonFields` | Beach | `GrampletonFields_Beach.tbin` | Край пляжа |
| IF2R / Grandpa's Farm | BusStop, Forest | `BusStop_IF2R_Warps.tbin`, `IF2R_warp_fix*.tbin` | Варпы и края карты |
| Fall leaves / MistEffects | многие outdoor | `*_Leaves.tbin`, `AdventurerSummit_Mist.tbin` | Обычно не двигают коллизии |

### Sprites / tilesheets (не карты, но для постановки)

- `Content/LooseSprites/Cursors.xnb` — кадр машины для `eventRescueOperation`
- Seasonal `Content/Maps/spring_town.xnb` (и аналоги) — привязка temporaryAnimatedSprite

### Рекомендуемое обновление в репозитории

- Обновить `tmpMap/Mine.tmx` и `tmpMap/Hospital.tmx` экспортом из **актуального runtime** (после SVE Load), если save/config изменился.
- Добавить экспорты для **Woods, Forest, Town, Mountain, Custom_AdventurerSummit, BusStop** — там больше всего `move`/`advancedMove`/`warp`.

---

## Как экспортировать (шпаргалка)

```text
# В игре с SMAPI, стоя на нужной локации:
debug export current

# Или unpacked vanilla:
Content/Maps/<LocationName>.xnb  →  xnbcli / Tiled

# SVE карты уже лежат как TMX/tbin в папке мода (см. таблицу выше).
# Для .tbin: PyTK / SDV map tools, или export current в игре (предпочтительно).
```

**Asset key в Data:** `Maps/<LocationName>` (совпадает с `LocationName` в событиях и `changeLocation`).

---

## Сводка

| Категория | Кол-во |
|---|---:|
| Уникальных локаций в audit | 13 |
| TMX в репозитории проекта | 2 (Mine, Hospital) |
| Локаций только в SVE (нет vanilla .xnb) | 1 (`Custom_AdventurerSummit`) |
| Локаций с SVE `Load` (замена vanilla) | ≥10 |
| CP-патчей карт в HarveyOverhaul [CP] | 0 |

**Вывод:** Harvey Overhaul **не добавляет** своих карт; проверка координат = vanilla + **SVE (обязательно при установленном SVE)** + optional mod patches + 2 локальных экспорта в `tmpMap/` как стартовая база для Mine/Hospital.
