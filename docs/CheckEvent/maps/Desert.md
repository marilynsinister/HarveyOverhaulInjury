# Map Passport: Desert

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`Desert`** |
| **Related names** | **`BusStop`** — автобус туда/обратно; **`SkullCave`** — пещера черепа (warp `(44,57)`); **`SandyHouse`** — Oasis Shop (interior warp с `(20,14)`); **не путать** с `DesertFestival` / другими desert-контекстами |
| **Map asset** | `Maps/Desert` |
| **Map file (audit)** | `tmpMap/sve/maps/Locations/Desert.tmx` |
| **Source** | **SVE Load** (`Desert.tbin` replaces vanilla); layout **fixed** 60×156 |
| **Size** | 60×156 tiles, 16×16 px |
| **Passable (TMX)** | **~6878** tiles — очень открытая карта |
| **Status** | **partial** — TMX в repo стабилен для ключевых зон; **storm Harvey `(17,26)` Broken**; runtime `.tbin` может отличаться — re-verify export |
| **Properties** | `LocationContext=Desert`; `Outdoors=T`; `SeasonOverride=summer`; `Light=35 42 4 20 14 4`; `NPCWarp=18 26 BusStop 22 10` |

**Used by events:**

| Event ID | File | Desert act? |
|----------|------|-------------|
| `eventHarveyStormComfortDesert` | events.json | yes (stay in Desert) |

**Related (не `Data/Events/Desert`, но staging на Desert):**

| Event ID | File | Связь |
|----------|------|-------|
| `eventHarveySkullCavePrevention` | eventsCare.json | **Сейчас** играет в **SkullCave** `(5,5)/(7,7)`; **рекомендуемая** альтернатива prevention — **Desert** у `(44,55–58)` — см. [`SkullCave.md`](SkullCave.md) |

**Not on Desert map:** mine rescue, skull prevention (current CP), BusStop events — см. [`BusStop.md`](BusStop.md), [`SkullCave.md`](SkullCave.md).

**Связанные документы:** [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md), [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`events-map-fix-backlog.md`](../events-map-fix-backlog.md), [`SkullCave.md`](SkullCave.md), [`BusStop.md`](BusStop.md).

---

## 2. Important areas

| Area | Approx range / anchor | Role | Staging notes |
|------|----------------------|------|---------------|
| **Bus arrival area** | **`x=15–21`, `y=23–27`**; bus sprite **`(17–19, 26–27)`**; `DesertBus` action **`(18,27)`** | Игрок приезжает из **BusStop**; Harvey «встречает у автобуса» | Открытый песок + кузов bus как **частичное** укрытие; **`(17,26)`/`(18,26)` Buildings — не passable** |
| **SkullCave entrance** | **`x=44`, `y=55–58`**; warp **`(44,57)`** | Вход в пещеру черепа | Узкая колонка; **`(43,57)`/`(45,57)` blocked** — Harvey **сбоку**, не на warp |
| **Oasis / Sandy Shop** | Door **`(20,14)`**; porch **`(20,15)–(22,16)`** | Магазин Sandy, **лучшее укрытие** на карте | Door blocked; фасад `(20,15)` Front — staging **у крыльца**, не на door tile |
| **Open sand areas** | Большая часть карты, esp. **north** `y=20–40`, **central** `y=50–90` | Heat / exhaustion / «нет укрытий» narrativ | OK для **короткого** dialogue; **плохо** для comfort без props — пустой кадр |
| **Palm / tree / decor shelter candidates** | Bus north row **`(18,23–24)`** Front от кузова; shop roof **`(20,15–16)`**; **rock pocket** **`(5,47)–(14,53)`** (open area #2) | Визуальное укрытие от жары/грозы | Shop > bus flank > south rocks > голый песок |
| **Paths and exits** | **BusStop** via `NPCWarp` / bus; **SkullCave** `(44,57)`; **SandyHouse** `(20,14)`; **Sand Dragon** `(51,89)` far south | Navigation anchors | Не блокировать warp/door после event |
| **Sand Dragon / deep south** | **`(51–52, 89)`** blocked | Quest landmark | **Avoid** staging |
| **Legendary sword** | **`(30, 142)`** Back action | Easter egg | Low traffic — **avoid** for events |

---

## 3. Doors, warps, exits

Подтверждено на **`tmpMap/sve/maps/Locations/Desert.tmx`**. Cross-ref **BusStop** — spawn при приезде может быть **`(18,27)`** (`BusStop` `NPCWarp`).

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **44** | **57** | **Warp** | **SkullCave `(7, 8)`** | **`(44,56)`**, **`(44,58)`** pass; **`(44,55)`** pass+Front | **Skull Cave entrance** — **never setup NPC on `(44,57)`** |
| **18** | **26** | **NPCWarp** (map property) | **BusStop `(22, 10)`** | **`(17,24)`**, **`(16,24)`**, **`(15,23)`** | Tile **Buildings blocked** — warp point, не staging |
| **18** | **27** | **TouchAction `DesertBus`** | Bus interaction / arrival tile | **`(17,25)`**, **`(16,24)`**, **`(18,24)`** (Front) | Типичный **spawn** с автобуса — **needs in-game verify** |
| **20** | **14** | **Action `LockedDoorWarp`** | **SandyHouse `(4, 9)`** | **`(20,15)`**, **`(21,15)`**, **`(22,16)`** | Shop door — **blocked**; shelter fade target |
| **51–52** | **89** | **Action `SandDragon`** | Quest | none nearby | Buildings blocked |
| **7** | **107** | **Action `Message`** | Sign | **`(8,108)`** — **needs check** | Oasis edge marker |
| **30** | **142** | **TouchAction `legendarySword`** | Item | **`(29,141)`** — **needs check** | Far south |

**External entry:**

| From | How | Typical farmer tile |
|------|-----|---------------------|
| **BusStop** | Bus ride → Desert | **`(18, 27)`** or near **`(18, 26)`** — **needs in-game check** (BusStop passport: `22 8 Desert 18 27`) |
| **SkullCave** | Exit warp | **Desert `(8, 6)`** per SkullCave vanilla — SVE may differ; **needs verify** |

---

## 4. Obstacles and visual blockers

| Type | Where | Blocks movement? | Event impact |
|------|-------|------------------|--------------|
| **Пальмы / деревья пустыни** | Scattered; **Front** у bus **`(18–21, 23–24)`** | Partial (Front overlay) | Укрытие **визуально**; не ставить farmer **внутри** Front без проверки кадра |
| **Кактусы / desert decor** | Open sand, esp. south/east | Usually passable Back | **Visual clutter** — не center scene на декор без viewport |
| **Камни / скалы** | **South-west pocket** `(5,47)–(14,53)`; edges `x≤3`, `y≥150` | Mixed — walls `#` in TMX | **Rock shelter** candidate; narrow entries |
| **Стены / скальные барьеры** | Map perimeter; skull approach **`x=40–42`**, **`x=45–48`** at `y=55–60` | **Yes** | Skull entrance — **одна** проходимая колонка `x=44` |
| **Вода / оasis edge** | South **`y≈104–110`**, **`x≈4–10`** | Partial — message tile **`(7,107)`** blocked | **Heat/exhaustion** narrativ; **не** staging в воде |
| **Декор Buildings** | **Bus body `(17–19, 26–27)`** | **Yes** — `(17,26)`, `(18,26)` **not passable** | Harvey **`warp (17,26)` = Broken** (inside bus sprite) |
| **Sandy Shop building** | **`(18–22, 10–17)`** | Walls blocked; porch partial | Best **roof shelter** on map |
| **Края карты** | `x=0`, `x=59`, `y=0`, `y=155` | Void / clamp | **Viewport** drift on 60×156 — use **`viewport`** for dialogue |
| **Sand Dragon statues** | `(51,89)` | Blocked | Far — ignore for CP staging |

---

## 5. Safe staging zones

Zones подтверждены на **repo TMX** — **re-verify** после SVE Load / `debug export current`.

### `desert_bus_arrival_safe`

| Field | Value |
|-------|-------|
| **Range** | **`x=15–21`, `y=23–25`** (open sand north of bus); extend check-in to **`y=26`** only on **passable** tiles |
| **Farmer anchor** | **`(18, 27)`** or **`(17, 25)`** — typical post-bus stand; **`(15, 23)`** — current storm setup (open sand) |
| **Harvey anchor** | **`(16, 24)`** or **`(19, 25)`** — passable, **not** on Buildings; face farmer |
| **Directions** | Farmer **2** (south) toward bus/Harvey; Harvey **0** (north) or **3** (west) toward farmer |
| **Camera** | **`viewport 18 25`** or **`17 24`** — bus body partially in frame |
| **Movement** | Max **`move ±2`** — open sand, no chase across map |
| **Use** | Arrival / check-in; Harvey worried if farmer returns weak from skull run |
| **Risks** | **`(17,26)`/`(18,26)`** blocked; **`NPCWarp (18,26)`** — don't block return to bus; **`DesertBus (18,27)`** — farmer needs access to board |

### `desert_skullcave_entrance_warning`

| Field | Value |
|-------|-------|
| **Range** | **`x=44`, `y=55–58`** (narrow column); approach from **`(42,56)`** — **blocked** — use **`(44,55)`** or **`(44,58)`** only |
| **Farmer anchor** | **`(44, 56)`** or **`(44, 58)`** — facing cave **`(44,57)`** |
| **Harvey anchor** | **`(44, 55)`** (north, Front) or offset **not on warp**: stand **`(44,58)`** south of farmer — **only one tile wide** at warp level |
| **Directions** | Farmer toward **`(44,57)`** (SkullCave warp); Harvey **between farmer and warp** but **not on `(44,57)`** |
| **Camera** | **`viewport 44 57`** — cave mouth + both actors |
| **Movement** | **Static preferred** — column too narrow for Harvey `advancedMove`; max 1 tile |
| **Use** | **Recommended** SkullCave prevention **before** warp ([`SkullCave.md`](SkullCave.md)); Harvey blocks/interrupts farmer |
| **Risks** | **`(44,57)` Critical** — warp tile; **`(43,57)`/`(45,57)` blocked** — no flank path; blocking warp = softlock |

### `desert_storm_or_heat_shelter`

| Field | Value |
|-------|-------|
| **Range (options)** | **A)** Bus flank **`(15–17, 23–24)`** — current storm farmer zone; **B)** Shop porch **`(20,15)–(22,16)`** — real roof; **C)** Rock pocket **`(9,50)–(14,51)`** — south shelter |
| **Farmer anchor (storm current)** | **`(15, 23)`** — **OK** passable; **open sand** — narrativ «нет укрытий» **OK**, visual **weak** |
| **Harvey anchor (storm current)** | **`(17, 26)`** — **Broken** (Buildings); **recommended:** **`(16, 24)`** or **`(18, 24)`** (Front at 18,24) |
| **Harvey anchor (shelter upgrade)** | **`(21, 16)`** at shop porch — «веду под крышу»; or **`(18, 24)`** «у автобуса» |
| **Directions** | Storm: farmer **2**, Harvey approaches **`move 0 -2`**, **`-2 0`** → **`(15,24)`** (path **OK** if spawn fixed) |
| **Camera** | **`viewport 16 24`** (storm bus) or **`21 15`** (shop shelter) |
| **Movement** | Current: Harvey **`(17,26)→(17,24)→(15,24)`** — path passable **after** valid spawn; **fix warp first** |
| **Use** | `eventHarveyStormComfortDesert`; future heat weakness / exhaustion scenes |
| **Risks** | Open sand **no shelter visual**; Harvey in bus **Broken**; shop door **`(20,14)`** — use porch only unless `changeLocation SandyHouse` |

### `desert_open_dialogue_safe`

| Field | Value |
|-------|-------|
| **Range** | **`x=15–21`, `y=24–26`** (bus plateau); alt **`x=27–32`, `y=4–8`** (small open patch north-east) |
| **Farmer anchor** | **`(17, 25)`** |
| **Harvey anchor** | **`(19, 25)`** or **`(15, 25)`** — 2–4 tiles apart, no movement |
| **Directions** | Face each other **`faceDirection`** only |
| **Camera** | **`viewport 17 25`** |
| **Movement** | **None** — static dialogue |
| **Use** | Short lines, check-in, promise — **not** comfort/shelter scenes |
| **Risks** | Empty sand frame — add viewport or pick zone with bus/rock in background |

---

## 6. Desert-specific staging rules

1. **Не ставить comfort/shelter-сцену** в середине **голого песка** без bus/shop/rock в кадре — иначе narrativ про укрытие **ломается** визually ([`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md)).
2. **SkullCave prevention** — лучше **у входа Desert `(44,55–58)`**, **не на warp `(44,57)`**; текущий CP в SkullCave interior — Desert = **recommended** для новых/переносов.
3. **Harvey у SkullCave** — **сбоку/перед** farmer в узкой колонке, **оставляя warp проходимым** для игрока.
4. **Harvey у автобуса** — **рядом с кузовом** `(16–19, 24–25)`, **never** `warp` на **`(17,26)`/`(18,26)`** (Buildings).
5. **`eventHarveyStormComfortDesert`** — narrativ «**нет укрытий**» **согласован** с open sand **`(15,23)`**, но Harvey spawn **должен** быть **сбоку от bus**, не в металле.
6. **Storm comfort с настоящим укрытием** — рассмотреть **shop porch** или **`globalFade` → SandyHouse** (backlog — не править события здесь).
7. **Movement короткий** — карта **огромная** (60×156); длинный chase = пустой кадр + NPC pathing на песке.
8. **Обязательно `viewport`** для dialogue — иначе camera follow теряет actors на open map.
9. **Heat / exhaustion** — prefer **partial shade** (bus, shop, rocks); debuff narrativ без visual shelter = **weak**.
10. **После event** — не блокировать **`(18,27)`** DesertBus, **`(18,26)`** NPCWarp, **`(44,57)`** SkullCave warp.
11. **SeasonOverride=summer`** — lighting fixed; storm overlay (`rain/`) still applies for storm comfort entry.
12. **Новые coords** → TMX pass + runtime export; иначе **`Coordinates require exported map`**.

---

## 7. Risk zones

| Zone | Risk | Why |
|------|------|-----|
| **SkullCave warp `(44, 57)`** | **Critical** | Instant transition — NPC block = softlock |
| **Skull approach `(43,57)` / `(45,57)`** | **High** | Blocked — no flank; only `x=44` column |
| **Bus Buildings `(17–19, 26–27)`** | **Critical** | Not passable — current Harvey storm warp **Broken** |
| **`NPCWarp (18, 26)`** | **High** | Return to BusStop — don't end event blocking tile |
| **`DesertBus (18, 27)`** | **High** | TouchAction — farmer must reach bus |
| **SandyHouse door `(20, 14)`** | **High** | LockedDoorWarp — blocked |
| **Water / oasis `(7, 107)` message** | **Medium** | Blocked sign; wet edge — odd for dialogue setup |
| **Sand Dragon `(51, 89)`** | **Medium** | Blocked quest objects |
| **Cactus / palm Front clusters** | **Medium** | NPC stuck / bad visibility |
| **Narrow passages** (audit) | **Medium** | `(42,4)`, `(44,4)`, `(46,20)`, `(5,21)`, `(32,21)`, `(18,27)`, `(44,55)`, `(45,59)` — ≤1 neighbor |
| **Map edges** | **Medium** | 60×156 — viewport walk-off |
| **Open sand (no props)** | **Visual** | Empty scene — especially storm/comfort |
| **SkullCave prevention wrong map** | **Critical (logic)** | Trigger bug fires SkullCave event from **Mine** — not Desert, but affects «go to skull» flow |

---

## 8. Events using Desert

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| **`eventHarveyStormComfortDesert`** | events.json | **needs-review** / coords **Broken** (Harvey) | Entry: Desert + `buffStressThunder` + storm + friendship ≥750 + Random 0.3. Farmer **`(15,23)` OK**; Harvey **`warp (17,26)` Broken** — Buildings/bus. Moves **`0 -2`, `-2 0` → (15,24)** path OK **if** spawn fixed. Narrativ «нет укрытий» vs visual. Backlog: **`(18,24)`** or **`(16,24)`** Harvey. See [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md), [`events-map-fix-backlog.md`](../events-map-fix-backlog.md). |
| **`eventHarveySkullCavePrevention`** | eventsCare.json | **N/A on Desert** (plays **SkullCave**) | **Recommended alternative staging:** `desert_skullcave_entrance_warning` **`(44,55–58)`** before warp — not implemented in CP. Triggers: [`SkullCave.md`](SkullCave.md). |

**Fork / sub-events on Desert:** none.

**C# / triggers referencing Desert:** storm comfort = **CP location entry only** (no C# warp). Skull prevention = SpaceCore triggers on **SkullCave** location, not Desert.

---

## 9. Quick Desert rules

1. **Status partial** — TMX stable; **storm Harvey spawn Broken** until coords fixed.
2. **Only CP event on map:** `eventHarveyStormComfortDesert` — farmer **`(15,23)`**, Harvey **not `(17,26)`**.
3. **Bus arrival zone:** **`x=15–21`, `y=23–27`** — `desert_bus_arrival_safe`.
4. **Skull entrance:** warp **`(44,57)`** — staging **`(44,55–58)`**, **never on warp tile**.
5. **Best shelter:** Sandy Shop porch **`(20,15)–(22,16)`** > bus flank > south rocks **`(9,50)–(14,51)`**.
6. **Forbidden:** Harvey warp on **`(17,26)`/`(18,26)`** — inside bus Buildings.
7. **Forbidden:** block **`DesertBus (18,27)`**, **`NPCWarp (18,26)`**, **`SkullCave (44,57)`**.
8. **Comfort/storm scenes** need **visual shelter** OR accept «empty desert» narrativ — don't claim shade on bare sand.
9. **Movement ≤4 tiles** — open map punishes long routes.
10. **Use `viewport`** — mandatory on 60×156.
11. **SkullCave prevention (future/alternate)** → Desert entrance, not procedural depths — [`SkullCave.md`](SkullCave.md).
12. **Static dialogue** → `desert_open_dialogue_safe` **`(17,25)`**.
13. **Shop door `(20,14)`** — blocked; porch OK, interior needs fade/warp.
14. **BusStop link** — arrival **`(18,27)`** typical — verify with BusStop passport.
15. **Heat/exhaustion scenes** — shade props required for believable comfort.
16. **Narrow tiles** — no double NPC **`advancedMove`** through `(44,55)` skull approach.
17. **Sand Dragon / far south** — not for Harvey events.
18. **New coords** → runtime export first — **`Coordinates require exported map`**.
19. **Do not edit events** in documentation tasks — passport for review/planning only.
20. **Cross-test:** storm Desert + injured skull run + bus return — three different Desert zones.

---

**Метод:** `tmpMap/sve/maps/Locations/Desert.tmx` + [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md) + [`events-coordinate-audit.md`](../events-coordinate-audit.md) + cross-ref [`SkullCave.md`](SkullCave.md), [`BusStop.md`](BusStop.md).  
**Не учтено:** runtime `.tbin` diffs, exact bus spawn tile, SkullCave return spawn `(8,6)` vs SVE, palm object positions not in object list.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
