# Map Passport: Mountain

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName** | Mountain |
| **Map asset** | `Maps/Mountain` |
| **Map file** | `tmpMap/sve/maps/Locations/Mountain.tmx` (fallback: `tmpMap/vanilla/maps/Mountain.tmx`) |
| **Source** | SVE Load + CP EditMap; vanilla outdoors + SVE tilesets (waterfalls, town props) |
| **Size** | **135×41** tiles (широкая горизонтальная карта), 16×16 px |
| **Status** | **ready** (TMX в repo; `OriginalMinesEntrance`, CC/Joja, runtime `.tbin` — **needs in-game check**) |
| **Layers** | Back, Back2, Back6, Buildings, Buildings2, Buildings3, Paths, Front, AlwaysFront, AlwaysFront2 |
| **Outdoors** | `Outdoors=T`; `BrookSounds`; `Fish`/`Frog`; seasonal debris/trees |

**Роль для мода:** шахта (LoadMap Mine), озеро, мост/перила, свидание, story E4B, storm comfort act 2 (после AdventurerSummit), связка с Mine через `(103,16)`.

**Used by events (Mountain location):**

| Event ID | File | Notes |
|----------|------|-------|
| `eventHarveyStormComfortMountain` | events.json | **act 2 only:** warp `(79,1)` / `(79,0)`; `viewport (76,15)`; act 1 = Custom_AdventurerSummit |
| `HarveyOverhaulStory.E4B_TooQuiet` | events.json | перила `(44,21)` |
| `eventHarveyMountainDate` | events.json | dating picnic `(41,19)` → `(45,22)` |

**Связанные (не на Mountain, но контекст шахты):**

| Event ID | Location | Notes |
|----------|----------|-------|
| `eventHarveyMineInterception` | **Mine** `(17,7)` / `(17,10)` | C# trigger при входе в **Mine**, не Mountain — паттерн «у входа в шахту» см. §6 |

**Связанные документы:** [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md), [`story-arc-map-audit.md`](../story-arc-map-audit.md) §E4B, [`Mine.md`](Mine.md) (LoadMap `(103,17)` ↔ `(103,16)`), [`Custom_AdventurerSummit.md`](Custom_AdventurerSummit.md) (storm act 1).

**Passability (TMX):** ~2565 passable; ~2637 Buildings; ~630 Front-only.

---

## 2. Important areas

### Mine entrance (критично)

- **LoadMap Mine:** Back action **`(103, 16)`** → Mine (runtime; Mine exit **`LoadMap Mountain 103 17`**).
- **Узкий «воротной» карман:** `x≈99–108`, `y≈15–20` — много Buildings; **единственный проходимый тайл входа** — **`(103,16)`** (+ **`(103,17)`** exit tile).
- **Подход с юга/запада:** открытая полоса **`(103–108, 18–20)`**, **`(104–105, 18)`** — лучшие flank tiles (не на LoadMap).
- **Визуал:** навес/скала над входом; Message tiles `Mountain.2–4` на `(27–31, 5–7)` — **другой** угол карты (west quarry messages), не путать с `(103,16)`.

### Adventurer Guild path (SVE)

- **Не на vanilla Mountain:** гильдия на **`Custom_AdventurerSummit`**.
- **Связь:** Mountain north warp **`(78–80, -1)`** ↔ Summit **`(32, 41)`** / south **`(31–33, 43)`**.
- Storm comfort act 1 на Summit; act 2 fade → Mountain **`(79,1)`**.

### Lake edge (озеро)

- **Западное озеро:** `x≈70–85`, `y≈8–14` — широкая passable полоса вдоль воды; Messages `Mountain.12` `(74,10)` blocked.
- **Восточное озеро / мостовая зона:** region center **`(116, 23)`**, `x≈110–125`, `y≈20–25` — длинный passable мост/набережная.
- **BrookSounds:** **`(48, 38)`**, **`(45, 9)`** — **needs in-game check** (audio markers).

### Bridges

| Bridge / rails | Coords | CP use |
|----------------|--------|--------|
| **Maru bridge / перила (west)** | **`(44, 21)`** | **E4B** — farmer +2 X к перилам |
| **East lake walkway** | `y≈20–25`, `x≈110–117` | atmospheric — **needs in-game check** |
| **South town bridges** | warp rows `y=41` | exit only |

### Robin / Carpenter (Science House)

- **Doors:** `(8, 20)` Maru/Robin; `(12, 25)` Science House back door.
- **Зона:** `x≈6–20`, `y≈18–26` — **узко** (Robin area often 1–2 tiles wide); не для dual NPC traffic.
- **Garbage Robin** `(9, 21)` — Buildings.

### Path to Town

| Exit | Forest warp on Mountain | Town dest |
|------|-------------------------|-----------|
| South-west | **`(14–16, 41)`** | **`(81, 0)`** |
| South-center | **`(57, 41)`** | **`(90, 1)`** |
| South-east | **`(85, 41)`** | **`(98, 1)`** |

Town north: `(79–83, -1)` → Mountain `(15, 40)` area — см. [`Town.md`](Town.md).

### Path to Railroad

- **North:** warp **`(9–10, -1)`** → Railroad **`(29, 59)`**.
- Не используется Harvey CP; не блокировать при сценах у северного края.

### Quiet mountain clearing

| Zone | Center | Use |
|------|--------|-----|
| **Lake date path** | **`(45, 22)`** | **MountainDate** — тихая тропа у воды |
| **West main** | **`(43, 19)`** | open region ~1745 tiles |
| **Mine approach south** | **`(105, 19)`** | open strip before entrance choke |
| **Summit landing** | **`(79, 1–3)`** | storm act 2 spawn |

### Storm shelter candidates

| Zone | Coords | Shelter visual | CP |
|------|--------|----------------|-----|
| **Summit warp slope** | **`(79,1)`**, viewport **`(76,15)`** | склон над озером, «гром мягче» | storm act 2 |
| **E4B railing** | **`(44,21)`** | перила/ветер | E4B; alt storm **needs check** |
| **Mine entrance overhang** | **`(104–105, 18–19)`** | rock/roof at shaft — **needs in-game check** | new «не в шахту» scenes |
| **West quarry messages** | `(27–31, 5–7)` | скала | **needs check** |
| **Open lake path `(45,22)`** | center | **слабо** — открытая тропа | sunny date only, not storm |

---

## 3. Doors, warps, exits

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **103** | **16** | **LoadMap (Back)** | **Mine** `(67,16)` area | **`(104,18)`**, **`(105,18)`**, **`(103,18)`**, **`(103,17)`** | **ШАХТА.** Не setup farmer/Harvey **на `(103,16)`** |
| **103** | **17** | **LoadMap (Mine exit)** | **Mountain** (return) | **`(103,18)`**, **`(104,18)`** | exit tile from Mine |
| **78–80** | **-1** | **Warp** | **Custom_AdventurerSummit `(32, 41)`** | **`(78–80, 0–3)`** pass | SVE summit; storm act 2 link |
| **9–10** | **-1** | **Warp** | **Railroad `(29, 59)`** | **`(9–10, 0–2)`** — **needs check** | north |
| **-1** | **12–14** | **Warp** | **Backwoods `(49, 14)`** | **`(0, 12–14)`** | west |
| **14–16** | **41** | **Warp** | **Town `(81, 0)`** | **`(14–16, 38–40)`** south approach | south-west |
| **57** | **41** | **Warp** | **Town `(90, 1)`** | **`(55–59, 38–40)`** | south; Front on `(57,40)` |
| **85** | **41** | **Warp** | **Town `(98, 1)`** | **`(83–84, 38–40)`** | south-east |
| **29** | **6** | **Warp** | **Tent `(2, 5)`** | none (blocked) | Linus tent — **не setup** |
| **8** | **20** | **LockedDoorWarp** | ScienceHouse `(3, 8)` | **`(7, 21–26)`** narrow | Robin/Maru |
| **12** | **25** | **LockedDoorWarp** | ScienceHouse `(6, 24)` | south path `y=26` | back door |

**OriginalMinesEntrance (SVE config):** может переназначить связку Mine/Summit — warp **`(79,1)`** и LoadMap **`(103,16)`** перепроверять in-game.

---

## 4. Obstacles and visual blockers

| Object / range | Blocks movement? | Blocks visibility? | Notes |
|----------------|------------------|--------------------|-------|
| **Mine entrance rock frame** | **yes** `(100–102,15–17)`, `(104–108,15–17)` | partial | только `(103,16–17)` pass |
| **Lake water (east/west)** | **yes** south of walkable strip | — | не setup на water tile |
| **Maru bridge rails** | partial south `(44,21)+` | — | E4B: `(42–45,21)` pass OK |
| **East lake bridge** | no on `x=110–117,y=20–25` | partial | atmospheric scenes |
| **Boulders / debris** | map property `Boulders 20 22` | — | **needs in-game check** |
| **Trees (Trees_asdf property)** | Buildings trunks scattered | Front canopy | не long move через grove |
| **Quarry cliff messages** | `(27–31,5–7)` Buildings | yes | west decor |
| **Robin house walls** | `(8–12,20–25)` | yes | narrow |
| **Front bushes** | `(103,15)` pass+Front; `(101–102,18)` Front | **yes** | mine approach visibility |
| **AlwaysFront** | mine choke, town warp `(57,40)` | **yes** | NPC clip |
| **Map edges** | `y=0` north `(78–80)`; `y=41` south warps | warp void | storm spawn `(79,0–1)` at edge |
| **NPC Paths** | Paths layer crossroads | — | Maru to mine route — **traffic** near `(103,18)` daytime |

---

## 5. Safe staging zones

### `mountain_mine_entrance_warning`

| Field | Value |
|-------|-------|
| **Range** | **`(99, 17)–(108, 20)`** approach; core flank **`(104–105, 18–19)`** |
| **Harvey tile** | **`(104, 18)`** or **`(105, 18)`** face **3** (west) — **between** farmer and mine; **NOT `(103,16)`** |
| **Farmer tile** | **`(106, 18)`** or **`(105, 19)`** face **1** (east toward entrance) — **NOT `(103,16–17)`** |
| **Directions** | Harvey **3**, farmer **1** or **2**; Harvey can face **0** if farmer south |
| **Viewport** | **`viewport 105 18 true`** or **`(103, 18)`** — frame entrance + both actors |
| **Movement** | **≤3 tiles** on `(104–107, 18–20)` strip; **no** `advancedMove` through `(99,15)` narrow |
| **Risks** | blocking **`(103,16)`** LoadMap; Front on `(102,18)`; NPC Maru path — **needs in-game check** |

**Purpose:** предупреждение перед шахтой; narrativ «запрет / забота»; шаблон для новых сцен ( **`eventHarveyMineInterception`** выполняется **внутри Mine**, но визуал «у входа» — здесь).

### `mountain_lake_path_dialogue`

| Field | Value |
|-------|-------|
| **Range** | **`(38, 18)–(48, 23)`** west lake path; verified date corridor **`y=19–22`, `x=41–46`** |
| **Harvey tile** | **`(46, 22)`** face **3** (**MountainDate** canon) |
| **Farmer tile** | start **`(41, 19)`** face **3** → **`move 0 3 1`** → **`(41, 22)`** → **`move 4 0 1`** → **`(45, 22)`** |
| **Directions** | оба face **1** (east) или Harvey **3** / farmer **1** at rendezvous |
| **Viewport** | **`viewport 45 22 true`** (**MountainDate** verified) |
| **Movement** | **two single-axis moves** (§3.1 authoring rules); no diagonal long path |
| **Risks** | open lake left — OK sunny; **`y=8–14`** lake edge has Front pockets `(72–74)` |

**Purpose:** спокойный диалог, dating trust; **MountainDate = manually verified — do not touch**.

### `mountain_bridge_area`

| Field | Value |
|-------|-------|
| **Range** | **E4B west:** **`(42, 21)–(46, 21)`**; east lake **`(110–117, 21–23)`** — **needs in-game check** |
| **Harvey tile** | **`(45, 21)`** face **3** (E4B setup) |
| **Farmer tile** | **`(42, 21)`** → **`move 2 0 1`** → **`(44, 21)`** at rails |
| **Directions** | Harvey **3** (view to valley); farmer **1** toward Harvey |
| **Viewport** | **`viewport 44 21 true`** (E4B); optional `ambientLight 90 85 110` |
| **Movement** | **1 move** farmer +2 X; static QQ (E4B pattern) |
| **Risks** | south of `(44,21)` slope blocked `#`; **не** ставить на `#` tiles south row; storm on bridge = **looks dangerous** — avoid |

**Purpose:** атмосфера, тишина у обрыва (E4B); осторожно с water/cliff framing.

### `mountain_storm_shelter`

| Field | Value |
|-------|-------|
| **Range** | **Act 2 canon:** north **`(76–84, 1–8)`** + viewport **`(76, 15)`**; alt **`(44,21)`** |
| **Harvey tile** | warp **`(79, 0)`** (storm canon); static finales **`(83,8)`** — **needs in-game** |
| **Farmer tile** | warp **`(79, 1)`** after `changeLocation Mountain` |
| **Directions** | face **2** (south to slope/lake) typical |
| **Viewport** | **`viewport 76 15 true`** after fade — **verified storm script** |
| **Movement** | current script uses **`advancedMove`** to **`(84,7)`** / **`(83,8)`** — audit **Warning** `(81,3)` Broken; prefer **short move** on `y=4–8,x=79–84` (pass TMX) for new scenes |
| **Risks** | **`(79,0–1)`** on **Summit warp edge**; `(81,3)` blocked; open slope not true «roof» |

**Purpose:** storm comfort act 2 — «спустились со склона»; см. [`Custom_AdventurerSummit.md`](Custom_AdventurerSummit.md) act 1.

---

## 6. Mine entrance special rules

1. **Never block `(103,16)`** — Back **LoadMap Mine**; farmer/Harvey/NPC on tile **blocks player mine entry** after event.
2. **Never setup on `(103,17)`** — Mine return exit tile (paired with Mine `(67,17)` object).
3. **Harvey «между farmer и входом»:** Harvey **`(104–105, 18)`**, farmer **`(106, 18)`** or **`(105, 19)`** — visually gates shaft; Harvey **not** on LoadMap tile.
4. **Farmer «перед входом»:** south strip **`(104–107, 18–20)`** facing **north (0)** or **east (1)**; **one tile away** from `(103,16)`.
5. **Short moves only** in choke **`x=99–108,y=15–20`** — narrow `(103,15)`, `(101,18)` Front; no `advancedMove`.
6. **Mine interception pattern** lives on **Mine map** `(17,7)/(17,10)` — if staging «перехват у горы», use **Mountain flank** coords above, then **`changeLocation Mine`** + Mine setup (see [`Mine.md`](Mine.md)).
7. **`eventHarveyMineInterception`:** trigger on **entering Mine**, not Mountain — do not duplicate warp on `(103,16)` in same frame as player mine use.
8. **Viewport** mandatory — entrance detail small on 135×41 map.
9. **OriginalMinesEntrance** SVE — re-test LoadMap coords after config change.
10. **Daytime NPC traffic:** Maru path near mine — **`needs in-game check`** for overlap.

---

## 7. Storm comfort suitability

Оценка для **`eventHarveyStormComfortMountain`** act 2 (Mountain) и новых storm-сцен.

### Лучшие зоны

| Rank | Zone | Why |
|------|------|-----|
| **1** | **`viewport (76,15)`** + actors **`(79,1)`/`(79,0)`** | Canon act 2; склон над озером; narrativ «ушли с высоты» |
| **2** | **`(44,21)`** E4B rails | Wind break, railing — **different mood** (quiet vs height) |
| **3** | **`(104,18)`** mine overhang | Rock shelter at shaft — **needs in-game screenshot** |
| **4** | West quarry **`(27–31,5–7)`** | Cliff cover — far from storm script path |

### Слабые / avoid

| Zone | Why |
|------|-----|
| **`(45,22)`** open lake path | Sunny date; **mid-storm = exposed** |
| **East bridge `x=110+`** | Long open walkway over water — **danger vibe** |
| **`(81,3)`** area | **Blocked** — current advancedMove hits Broken |
| **South town warp rows `y=41`** | Transition tiles, not shelter |
| **Center `(43,19)`** open | No rock/tree — **поле посреди тропы** |

**Act 1** storm on **Custom_AdventurerSummit** — см. отдельный паспорт; Mountain act 2 begins **`changeLocation Mountain`** + warp **`(79,1)`**.

---

## 8. Risk zones

| Coords / range | Risk | Why | Avoid |
|----------------|------|-----|-------|
| **`(103, 16–17)`** | **LoadMap / exit** | mine transition | setup, block |
| **`(103, 15)`** | narrow + Front | choke | dual NPC |
| **`(81, 3)`** | **blocked** | storm advancedMove | path through |
| **`(79, 0–1)`** | summit warp edge | north void + warp | long moves north |
| **`(78–80, -1)`** | warp | → AdventurerSummit | setup |
| **`(57, 40)`**, **`(85, 40)`** | town warp | south edge | end scene |
| **Lake water tiles** | blocked / water | east & west lakes | walk into water |
| **`(44,21)` south `#`** | cliff | fall visual | step south |
| **Robin `(8–12,20–25)`** | narrow | 1-tile paths | two NPC |
| **East `(92–102, 0–15)`** | narrow passages | audit list | advancedMove pairs |
| **Front clusters** | `(101–102,18)`, `(57,40)` | visibility | farmer dialogue |
| **NPC Paths** | near mine `(103,18)` | Maru schedule | event at 9–15h |
| **CC `Mountain_Joja`** | layout diff | Town south link | verify both maps |

---

## 9. Events using Mountain

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| **`eventHarveyStormComfortMountain`** | events.json | **needs-review** | **Act 2 Mountain:** farmer **`(79,1)`**, Harvey **`(79,0)`**, **`viewport (76,15)`**; advancedMove → **`(84,7)`/`(83,8)`** Warning. **Act 1 = Summit** (not this file). |
| **`HarveyOverhaulStory.E4B_TooQuiet`** | events.json | **checked-ok** | farmer **`(42,21)`** → **`(44,21)`**; Harvey **`(45,21)`**; перила; QQ static |
| **`eventHarveyMountainDate`** | events.json | **manually-verified-do-not-touch** | **`viewport (45,22)`**; farmer **`(41,19)`** Harvey **`(46,22)`**; moves **`0 3 1`**, **`4 0 1`** |

**Related (Mine, not Mountain row):** `eventHarveyMineInterception` — **Mine** only; C# `triggerHarveyMineWarning`.

---

## 10. Quick Mountain rules

1. **135×41** — always **`viewport`** for new outdoor scenes.
2. **Mine LoadMap = `(103,16)`** — **never** setup actors on it or `(103,17)`.
3. **Flank staging:** farmer **`(105–106, 18)`**, Harvey **`(104–105, 18)`** — gate entrance visually.
4. **Short moves** (≤4 tiles) in mine choke `x=99–108,y=15–20`.
5. **E4B canon:** **`(42,21)`/`(45,21)`**, farmer **`move 2 0 1`** → **`(44,21)`** — **checked OK**.
6. **MountainDate:** **`viewport 45 22`**, **`(41,19)`→`(45,22)`** — **manually verified, do not touch**.
7. **Storm act 2:** warp **`(79,1)`/`(79,0)`**, **`viewport 76 15`** after fade.
8. **Avoid `advancedMove`** through **`(81,3)`** and Summit rocks (act 1).
9. **Maru bridge / перила `(44,21)`** — not for storm unless intentional cliff mood.
10. **Lake dialogue:** west path **`x=41–46,y=19–22`** — pass OK, sunny preferred.
11. **East lake bridge `x=110+`** — atmospheric only; **needs in-game check**.
12. **Summit link:** north **`(78–80,-1)`** — don't block warp.
13. **Town south:** **`y=41`** warps — don't setup.
14. **Robin area** — too narrow for rescue-style multi-NPC.
15. **Mine interception** = **Mine map** `(17,7)` — use Mountain only for «approach» staging.
16. **OriginalMinesEntrance / CC Joja** — retest coords.
17. **Front at mine `(102,18)`** — check farmer visibility.
18. **NPC traffic** near mine entrance — daytime overlap **needs check**.
19. **Water tiles** — never setup; bridges OK if pass TMX.
20. **New mine-warning scenes:** Harvey **between** farmer and **`(103,16)`**, not **on** warp.

---

**Метод:** TMX `tmpMap/sve/maps/Locations/Mountain.tmx` + [`events-coordinate-audit.md`](../events-coordinate-audit.md).  
**Не учтено:** runtime `.tbin`, `OriginalMinesEntrance`, NPC schedule paths, CC/Joja Mountain layout.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
