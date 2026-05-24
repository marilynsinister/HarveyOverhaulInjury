# Map Passports Index

## Назначение

Паспорта карт используются при проверке и правке **CP-событий** Harvey Overhaul: координаты farmer/Harvey/NPC, проходимость, двери и warp, мебель, safe staging zones, camera/viewport и рискованные зоны.

Каждая локация — отдельный hand-maintained файл в [`maps/`](maps/). Базовый парсинг TMX: `tmpMap/generate_map_passports.py` / `generate_split_map_passports.py` (генератор **не перезаписывает** детальные паспорта).

**Не учтено в TMX:** runtime-патчи SVE (`Load`/`EditMap`), `positionOffset`, коллизии NPC с Front-тайлами, сезонные sheet swap.

---

## Карты

| Location | Passport | Source | Status | Used by events | Main risk |
|----------|----------|--------|--------|----------------|-----------|
| **Hospital** | [`maps/Hospital.md`](maps/Hospital.md) | SVE Load + vanilla TMX | **ready (priority)** | HarveyMod_FirstTreatment, HarveyOverhaulStory.E2/E5/E6, mine-rescue finale, night crisis, … (+21) | major bed `(20,5)` Buildings — только offset+lying; door/warp tiles; west exam `(5,9)` |
| **Farm** | [`maps/Farm.md`](maps/Farm.md) | variable layouts (no single TMX) | **variable-layout-sensitive** | eventHarveyFirstVisit, eventHarveySecondVisit, eventHarveyFirstWalk, eventHarveyMorningCheckup, eventHarveyStormComfortFarm, … (+8) | **layouts + player buildings** — safe staging **only** near farmhouse visitor/door `(64,15–18)` Standard |
| **Town** | [`maps/Town.md`](maps/Town.md) | external SVE/custom (CC / Town_Joja) | **partial** | eventHarveyStormComfortTown, HarveyOverhaulStory.E2B/E7/E9, eventHarveyStormComfortMine (finale), … (+6) | public NPC traffic; CC vs Joja layout; storm chase length |
| **Forest** | [`maps/Forest.md`](maps/Forest.md) | external SVE/custom + EditMap | **ready** | eventHarveyStormComfortForest, HarveyOverhaulStory.E3/E3B, eventHarveyFirstDate, eventRescueOperation (pickup), … (+6) | 120×120 — long move; storm tree canopy **needs visual check** |
| **Woods** | [`maps/Woods.md`](maps/Woods.md) | external SVE/custom (Woods2) | **ready** | eventRescueOperation | narrow paths; rescue `(27,18)` / `(40,20)`; DeepWoods runtime |
| **Mountain** | [`maps/Mountain.md`](maps/Mountain.md) | external SVE/custom + EditMap | **ready** | eventHarveyStormComfortMountain (act 2), HarveyOverhaulStory.E4B, eventHarveyMountainDate | **LoadMap Mine `(103,16)`**; north Summit warps; cliff/rails `(44,21)` |
| **Mine** | [`maps/Mine.md`](maps/Mine.md) | external SVE/custom (entrance TMX) | **procedural-high-risk** | eventHarveyMineRescue, eventHarveyMineInterception, eventHarveyStormComfortMine, … (+5) | **entrance only** — `MineShaft` procedural; south edge `y≥11`; prefer fade→Hospital |
| **SkullCave** | [`maps/SkullCave.md`](maps/SkullCave.md) | external vanilla (16×10 entrance) | **procedural-high-risk** | eventHarveySkullCavePrevention (+ fork HarveySkullPromise) | tiny room; **`SkullDoor (3,3)`** → procedural depths; exit warp `(7,9)`; trigger OR-bug |
| **Desert** | [`maps/Desert.md`](maps/Desert.md) | external SVE/custom | **partial** | eventHarveyStormComfortDesert | Harvey warp **`(17,26)` Buildings/bus**; open sand — weak shelter visual |
| **BusStop** | [`maps/BusStop.md`](maps/BusStop.md) | external SVE/custom + EditMap | **partial** | eventHarveyFirstMeeting, HarveyOverhaulStory.E1, eventHarveyCheckup | **Checkup target BusStop / coords Hospital** — Critical; IF2R warp patches |
| **Beach** | [`maps/Beach.md`](maps/Beach.md) | external SVE/custom + EditMap | **partial** | HarveyOverhaulStory.E4_PierBreath, eventHarveyPropose | **water/ocean `#` tiles**; E4 overlap `(39,13)`; Propose **do-not-touch** |
| **HarveyRoom** | [`maps/HarveyRoom.md`](maps/HarveyRoom.md) | external vanilla interior | **needs export** | eventHarveyRoomCheckup, eventHarveyRoomCheckup2 | **tiny room** — furniture overlap, door `(6,12)`, short move only; TMX **not in repo** |
| **Custom_AdventurerSummit** | [`maps/Custom_AdventurerSummit.md`](maps/Custom_AdventurerSummit.md) | external SVE/custom (`AdventurerSummit.tmx`) | **partial** | eventHarveyStormComfortMountain (act 1 only) | **SVE-only**; storm `advancedMove` through **Broken rocks**; south warps → Mountain |

**Всего паспортов:** **13** (`docs/CheckEvent/maps/*.md`).

### Связанные локации без отдельного паспорта

| Location | Используется | Где смотреть |
|----------|--------------|--------------|
| **Saloon** | `eventHarveyStormComfortTown` (act 2) | TMX: `tmpMap/vanilla/maps/Saloon.tmx`; staging **`(14,23)`**; warp `(14,25)→Town` |
| **ArchaeologyHouse** | `HarveyOverhaulStory.E8_QuietShelf` | TMX: `tmpMap/sve/maps/Locations/ArchaeologyHouse.tmx`; farmer **`(18,9)`**; Harvey warp **`(3,15)`** |
| **Town_Joja** | альтернатива Town при Joja route | `tmpMap/sve/maps/Locations/Town_Joja.tmx`; см. [`Town.md`](maps/Town.md) |
| **MineShaft / skull depths** | vanilla procedural | **no fixed coords** — см. [`Mine.md`](maps/Mine.md), [`SkullCave.md`](maps/SkullCave.md) |

---

## Как использовать паспорт карты

1. Открыть паспорт нужной карты из таблицы выше.
2. Проверить **Metadata** — source, status, список событий.
3. Проверить стартовые координаты farmer / Harvey / NPC и **safe staging zones**.
4. Проверить **movement path** — короткие сегменты, проходимость каждого тайла.
5. Проверить **doors / warps** — не setup на warp/door.
6. Проверить **viewport / camera** — clamp, края карты, визуальный смысл (outdoor/scenic).
7. Только потом править событие в `assets/Code/events*.json`.

Чеклист ревью: [`cp-event-review-checklist.md`](cp-event-review-checklist.md).  
Правила авторинга: [`../EventPatterns/cp-event-authoring-rules.md`](../EventPatterns/cp-event-authoring-rules.md).

---

## Статусы

| Status | Значение |
|--------|----------|
| **ready** | TMX в repo (или стабильная зона подтверждена); ключевые event coords проверены |
| **ready (priority)** | **Hospital** — разобран подробно; первый паспорт для правок clinic/bed/warp |
| **partial** | TMX есть, но runtime/SVE/EditMap может отличаться **или** coords/audit неполные |
| **needs export** | Нет TMX в repo — координаты только из audit/manual; нужен `debug export current` |
| **external vanilla** | Vanilla map/xnb; SVE патчит минимально (SkullCave entrance) |
| **external SVE/custom** | SVE Load/EditMap или custom локация без vanilla аналога |
| **variable-layout-sensitive** | **Farm** — layout и постройки игрока меняют карту; universal coords запрещены вне visitor zone |
| **procedural-high-risk** | **Mine / SkullCave** — fixed coords только на подтверждённом entrance; depths procedural |
| **manually-covered** | События на карте в основном **manually-verified-do-not-touch** (см. паспорт §Events) |

---

## Приоритет файлов TMX

SVE TMX (если Load заменяет vanilla) → vanilla TMX. **SkullCave** — vanilla entrance. **Woods** → `Woods2.tmx`. **Custom_AdventurerSummit** → `NewLocations/AdventurerSummit.tmx` (SVE only). **Farm** — per-save export, не один repo TMX.

---

## Universal location risk notes

### Farm

Любые универсальные события на Farm должны держаться у **farmhouse door / visitor spawn area** (baseline Standard **`~64,15–18`**). Не использовать дальние координаты и объекты фермы без отдельной проверки, потому что **layouts и player buildings** могут отличаться. Riverland / Forest / Beach / modded farm maps — отдельный export. См. [`Farm.md`](maps/Farm.md).

### Mine / SkullCave

Фиксированные координаты **high-risk**. Предпочтительно использовать **entrance**, эвакуацию на **Mountain / Desert**, или **Hospital aftermath** (`globalFade` + `changeLocation Hospital`). **Не** ставить сцены на процедурных этажах `MineShaft` / глубинах Skull Cave. См. [`Mine.md`](maps/Mine.md), [`SkullCave.md`](maps/SkullCave.md).

### Small interiors

**HarveyRoom** и подобные маленькие карты требуют **короткого movement** (1–2 тайла), строгой проверки **мебели / overlap**, и **не блокировать дверь/warp**. Без TMX — **needs export** перед новыми coords. См. [`HarveyRoom.md`](maps/HarveyRoom.md), [`Hospital.md`](maps/Hospital.md) (interior rules).

### Outdoor scenic maps

**Forest, Mountain, Beach, Desert** (и **Custom_AdventurerSummit**) требуют проверки **визуального смысла**: укрытие от грозы, **вода**, мосты, **края обрыва**, деревья, декор, viewport. Координата может быть проходимой на TMX, но narrativно неверной (открытое поле vs навес). См. [`storm-comfort-map-audit.md`](storm-comfort-map-audit.md).
