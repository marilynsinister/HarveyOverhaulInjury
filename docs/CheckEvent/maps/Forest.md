# Map Passport: Forest

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName** | Forest |
| **Map asset** | `Maps/Forest` |
| **Map file** | `tmpMap/sve/maps/Locations/Forest.tmx` (fallback: `tmpMap/vanilla/maps/Forest.tmx`) |
| **Source** | SVE Load + CP EditMap; база vanilla outdoors + SVE canopy/shadow tilesets |
| **Size** | **120×120** tiles, 16×16 px |
| **Status** | **ready** (TMX в repo; runtime SVE/IF2R патчи — **needs in-game check**) |
| **Layers** | Back, Back2, Back3, Back5, Buildings, Buildings2, Buildings3, Paths, Front, Front2, AlwaysFront, AlwaysFront2, AlwaysFront3 |
| **Outdoors** | `Outdoors=T`; seasonal objects; `BrookSounds` (река/ручьи); SVE `zGrandpasFarm_*CanopyShadow` |

**Роль для мода:** прогулки, первое свидание, story arc E3/E3B (аптекарь / птица), storm comfort, финал rescue operation (пикап машины), fork `acceptWalk` после Farm.

**Used by events:**

| Event ID | File | Notes |
|----------|------|-------|
| `eventHarveyStormComfortForest` | events.json | storm, `(23,13)` |
| `HarveyOverhaulStory.E3_ForestApothecary` | events.json | sunny, `(50,13)` |
| `HarveyOverhaulStory.E3B_WingPatient` | events.json | sunny, `(48,14)` |
| `eventHarveyFirstDate` | events.json | dating picnic, `(65,38–46)` |
| `eventRescueOperation` | events.json | Forest act: `(66,16)` + sprite `(67,12)` |
| `acceptWalk` | events.json (fork) | Farm door → **`changeLocation Forest`** |

**Связанные документы:** [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md), [`story-arc-map-audit.md`](../story-arc-map-audit.md) §E3/E3B, [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`cp-event-authoring-rules.md`](../../EventPatterns/cp-event-authoring-rules.md) §3, §9.

**Passability (TMX):** ~9234 проходимых тайлов; ~4803 Buildings; ~469 Front-only (passable, move risky).

---

## 2. Important areas

### Вход с Farm (север)

- **Warp:** Forest `(67–72, -1)` → Farm `(41, 64)` (и обратно с Farm south edge).
- **Подход с юга карты:** широкая **север–юг тропа** `x≈64–75`, `y≈10–25` — проходима по TMX; `(67,10)` Buildings (не setup), `(68–72,0)` — край warp.
- **Визуал:** лесная аллея от фермы; SVE canopy shadows на `(75,10–11)` Front — **needs in-game check**.
- **CP:** типичный вход игрока с Farm; `acceptWalk` делает `changeLocation` сюда (точные coords fork — **needs check**, events.json не в repo audit).

### Путь к Marnie / Ranch (северо-восток)

- **AnimalShop:** `LockedDoorWarp (90,15)` → AnimalShop `(13,19)`.
- **MarnieShed:** `(87,7)` → Custom_MarnieShed.
- **Зона ranch:** `x≈79–101`, `y≈6–20` — здания, заборы, Buildings; counter `(90,15)` **blocked**.
- **Проходимая тропа** к ranch с запада: `y≈13–19`, `x≈76–89` (обход counter с юга).
- **CP:** не используется Harvey-сценами; не вести NPC через `(90,15)`.

### Мосты и переправы

Река/ручьи идут **горизонтальными полосами** через карту. BrookSounds (TMX property) — ориентиры воды:

| Brook tile | Passable? | Adjacent path (bridge candidates) |
|------------|-----------|----------------------------------|
| `(54,99)`, `(64,101)` | blocked | узкие берега — юг карты |
| `(46,85)`, `(60,58)`, `(118,45)` | **pass** (мост/брод) | `(45–47,85)`, `(59–61,58)`, `(117–118,45)` |
| `(29,76)`, `(43,65)`, `(79,53)`, `(88,45)` | blocked | мосты на `y≈43–48`, `y≈57–62`, `y≈84–86` (см. §4) |

**Подтверждённые мостовые ряды (passable «островки» на ряду воды):** `y=45` (x≈77,83,93–95,118), `y=58–62` (много x), `y=85` (x≈26–46). Точный визуал моста — **needs in-game check**.

### Река / озеро

- **Не одно озеро:** сеть ручьёв + горизонтальные «водные» ряды (Buildings/Water tiles на tileset).
- **Центральный пояс воды:** `y≈57–62` (разделяет север и центр).
- **Южный пояс:** `y≈84–87`.
- **Восточный брод к Town:** `(118,45)` pass + BrookSounds — стык с Town-side river.
- **Опасность:** тайлы воды = Buildings blocked; `move` через воду **Broken**.

### Лесные тропинки (подтверждённые CP-коридоры)

| Тропа | Range | Использование |
|-------|-------|---------------|
| **Storm lane** | `y=13`, `x=23–35` | `eventHarveyStormComfortForest` — чистый горизонт |
| **E3 herb path** | `y=13`, `x=47–51` | E3 apothecary |
| **E3B grass path** | `y=14`, `x=46–49` | wing patient |
| **Farm trunk road** | `x=64–75`, `y=10–25` | вход с Farm, rescue pickup approach |
| **Rescue road** | `(66,16)` area, `y=12–17`, `x=64–74` | машина / Lewis |

Front на storm lane: `(10–11,13)`, `(42–44,13)` — pass OK, move может «тереться» о кусты.

### Secluded clearing / quiet area

| Zone | Center (TMX) | Size hint | Visual |
|------|--------------|-----------|--------|
| **North clearing** | `(36, 30)` | open region ~321 tiles | поляна севернее центра; 5×5 pass OK, Front=0 |
| **Central picnic** | `(65, 45)` | open ~253 tiles `(41–76,52–68)` | **FirstDate** — плед, закат; pass OK |
| **E3 story cluster** | `(48, 14)` / `(50, 13)` | тропа + трава | herbs / bird scene |
| **South-west grove** | `(31, 74)` | region ~209 tiles | более густой декор — Front в 5×5 — **needs check** |
| **Stump cluster** | `(79, 11–14)` | Buildings пни | «лесной декор», не walk target |

### Зоны под деревьями (storm comfort)

| Zone | Coords | Shelter visual | CP |
|------|--------|----------------|-----|
| **Verified storm** | `(23,13)` | тропа под кронами Back (canopy зависит от season) | `eventHarveyStormComfortForest` |
| **Stump / log decor** | `(79,11)–(82,14)` | пни Buildings + Front — «под деревьями» | альтернатива — **needs in-game check** |
| **Wizard house wall** | `(9,21)–(11,23)` | стена здания слева = укрытие от дождя | narrativ OK; door `(9,20)` blocked |
| **Open picnic (65,45)** | центр поляны | **мало** навеса — для storm **не подходит** | FirstDate only (sunny) |

### Выходы (Town / Farm / Woods / Beach)

| Direction | Forest warp | Destination | Safe nearby |
|-----------|-------------|-------------|-------------|
| **Farm** | `(67–72, -1)` | Farm `(41, 64)` | `(68–72, 0–2)` pass |
| **Woods** | `(-1, 6–7)` | Woods `(81, 29)` | `(0, 6–7)` pass + Front |
| **Town** | `(120, 24–27)` | Town `(0, 90)` | `(115–118, 24–27)` pass |
| **Beach** | — | **нет прямого warp** | через Town `(57–60,116)` → Beach |

LeahHouse `(104,32)`, WizardHouse `(9,20)`, AndyHouse `(62,66)` — interior doors, не CP Harvey.

---

## 3. Doors, warps, exits

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **67–72** | **-1** | **Warp** | **Farm `(41, 64)`** | `(68–72, 0–2)` | главный вход с фермы |
| **-1** | **6–7** | **Warp** | **Woods `(81, 29)`** | `(0, 6–7)`, `(1, 6–7)` | запад; Front на `(0,6)` |
| **120** | **24–27** | **Warp** | **Town `(0, 90)`** | `(115–118, 24–27)` | восток; `(119,25)` OOB edge |
| 90 | 15 | LockedDoorWarp | AnimalShop `(13, 19)` | `(89,16)`, `(88,17)` south path | Marnie shop — Buildings |
| 87 | 7 | LockedDoorWarp | Custom_MarnieShed `(6, 9)` | `(86,8)` area | shed |
| 9 | 20 | LockedDoorWarp | WizardHouse `(9, 34)` | `(5–6, 21–23)` pass | wizard |
| 104 | 32 | LockedDoorWarp | LeahHouse `(7, 9)` | south approach **needs check** | Leah |
| 62 | 66 | LockedDoorWarp | Custom_AndyHouse `(12, 22)` | **needs check** | SVE Andy |

**Не setup** на warp tiles (`y=-1`, `x=120`, `x=-1`).

---

## 4. Natural blockers

| Object | Coords / range | Blocks movement? | Blocks visibility? | Notes |
|--------|----------------|------------------|--------------------|-------|
| **Trees (Buildings trunks)** | по всей карте, особенно `x<20`, `y<30`, ranch `x>85` | **yes** | partial | не `move` / `advancedMove` «сквозь» |
| **Bushes / small Front** | storm `(10–11,13)`, `(42–44,13)`; farm `(75,10–11)` | no / partial | **yes** | pass OK; NPC может «прятаться» |
| **Water / river rows** | `y≈43–48`, `57–62`, `84–87`; BrookSounds tiles | **yes** | — | только мостовые x (§2) |
| **Bridges** | см. `y=45,58,85` passable strips | no on bridge tile | partial | длинный move по мосту — Avoid |
| **Fences / ranch Buildings** | `(79–101, 6–20)` | **yes** | yes | Marnie block |
| **Stumps Forest.29** | `(79–83, 10–14)` | **yes** | Front | storm alt visual — не walk |
| **Wizard / Leah walls** | `(2–4, 20–22)`, `(104,32)` area | **yes** | yes | shelter narrative |
| **Map edges** | `y=0` north strip, `x=0` west, `x=119` east | partial | — | warp / void |
| **AlwaysFront foliage** | Day/Night tiles `(104,19)`, `(117,19)`; canopy SVE | usually no | **yes** | не ставить «лицом» в куст |
| **Narrow paths** | `(46,3)`, `(79,10)`, `(27,23)`, `(48,32)`, `(95,41)`, `(66,10)` | pass but **≤1 neighbor** | — | один NPC OK; двойной advancedMove — Avoid |
| **SVE canopy shadow** | UnderCanopyShadow layers | no | darkens tile | visual only; **needs in-game check** |

---

## 5. Safe staging zones

### `forest_path_dialogue`

| Field | Value |
|-------|-------|
| **Range** | **`(64, 12)–(75, 25)`** farm trunk; **`(20, 12)–(55, 15)`** central paths; **`(47, 12)–(52, 15)`** E3 |
| **Harvey** | `(51, 13)` face **3**; или `(35, 13)` → `move -11 0` → `(24, 13)` face **3** |
| **Farmer** | `(50, 13)` face **1**; storm `(23, 13)` face **1→2** |
| **FaceDirection** | встреча лицом: Harvey **3**, farmer **1** (горизонталь); или оба **2** на тропе N–S |
| **Camera** | `(50, 13)` E3; `(23, 13)` storm; `(69, 18)` farm path — **needs in-game check** |
| **Movement** | короткий `move ±2–4` по одной оси; `proceedPosition` после |
| **Visual meaning** | «прогулка по лесной тропе», Harvey подходит сбоку |
| **Risks** | Front `(10–11,13)`; длинный horizontal move без viewport на 120×120 |

### `forest_tree_shelter_storm`

| Field | Value |
|-------|-------|
| **Range** | **verified:** `(21, 12)–(26, 14)` around `(23,13)`; **alt:** `(76, 9)–(85, 15)` stump ring |
| **Harvey** | **warp `(35, 13)`** → **`move -11 0 3`** → **`(24, 13)`** face **3** |
| **Farmer** | **`(23, 13)`** face **1** then **2**; `animate farmer false true 2000 5 4` |
| **Camera** | **`viewport 23 13 true`** (recommended backlog); default = **needs in-game check** |
| **Movement** | farmer static; Harvey один горизонтальный проход |
| **Visual meaning** | «укрылась под деревьями» + ирония «деревья притягивают молнии» |
| **Risks** | canopy не привязан к object tile; `(35,13)` spawn может быть «открытая аллея» — **needs in-game check** |

### `forest_bridge_edge`

| Field | Value |
|-------|-------|
| **Range** | **`(116, 44)–(118, 46)`** east brook; **`(44, 57)–(54, 62)`** central bridges — **needs in-game check** |
| **Harvey** | `(117, 45)` or `(118, 44)` — pass TMX; face **0/2** к воде |
| **Farmer** | `(116, 45)` / `(115, 45)` — pass; 1 tile от brook |
| **FaceDirection** | к воде **2** или друг к другу **1/3** |
| **Camera** | `(117, 45)` tight; river in frame — **needs in-game check** |
| **Movement** | **≤2 tiles**; не `advancedMove` вдоль `y=58` |
| **Visual meaning** | тревога, «осторожно у воды», мягкий разговор на переправе |
| **Risks** | slip into water tile; Front on `(114–115,45)`; двое на узком мосту `(118,45)` |

### `forest_clearing_private_talk`

| Field | Value |
|-------|-------|
| **Range** | **`(63, 36)–(67, 48)`** FirstDate; **`(45, 12)–(52, 16)`** E3/E3B; **`(33, 27)–(39, 33)`** north clearing |
| **Harvey** | **`(65, 46)`** face **1** (FirstDate); **`(49, 14)`** face **3** (E3B); **`(51, 13)`** (E3) |
| **Farmer** | **`(65, 38)`** face **2** (FirstDate); **`(48, 14)`** / **`(50, 13)`** |
| **FaceDirection** | FirstDate: farmer **2** south, Harvey **1** east; E3: side-by-side **1/3** |
| **Camera** | **`viewport 65 45 true`** (FirstDate verified script); **`(48, 14)`** E3B |
| **Movement** | FirstDate: `move farmer 0 7 2` to `(65,45)`; E3: `move -3 0`; E3B: `move -2 0` |
| **Visual meaning** | доверие, пикник, сбор трав, помощь птице — **без** толпы NPC |
| **Risks** | `(57,59)` center has Front in 5×5 — prefer `(36,30)` or `(65,45)`; temporaryAnimatedSprite `(66,44)` — не блокировать actors |

---

## 6. Storm comfort suitability

Оценка для **`eventHarveyStormComfortForest`** и новых storm-сцен.

### Хорошо (укрытие / лесной смысл)

| Zone | Coords | Почему |
|------|--------|--------|
| **Current CP (verified)** | **`(23, 13)`** | центральная лесная тропа; pass OK; narrativ «под деревьями»; Harvey approach с востока |
| **Stump cluster (alt)** | `(79, 11–14)` | явный лесной декор (пни); кольцо pass `(76–77, 10–15)` | 
| **Wizard house lee** | `(9, 21–23)` | стена = физическое укрытие; узко |
| **North path under trees** | `(20–30, 10–15)` | та же «аллея», что storm — **needs in-game canopy check** |

### Слабо / нелепо «посреди поля»

| Zone | Coords | Почему |
|------|--------|--------|
| **Central picnic meadow** | **`(65, 38–46)`** | большая **открытая** поляна; FirstDate = sunset, не storm |
| **Ranch approach** | `(90, 15)` area | забор, здания, не «лес» |
| **East open** | `(106, 15)` region | мало деревьев на Back — **needs in-game check** |
| **Bridge mid-river** | `y=58`, open water both sides | гроза + вода = двойной stress, не «укрытие» |
| **Farm warp strip** | `(68, 0–5)` | слишком близко к выходу / open north edge |

### Рекомендации (документация only)

1. **Оставить `(23,13)`** как canon — TMX + audit OK ([`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md)).
2. Опционально **`viewport 23 13 true`** — backlog visual polish.
3. Альтернатива только после **in-game screenshot**: stump `(79,12)` или Wizard `(10,22)`.
4. **Не** переносить storm на FirstDate clearing `(65,45)` — визуально wrong.

---

## 7. Risk zones

| Coords / range | Risk | Why | Avoid |
|----------------|------|-----|-------|
| **Water rows** | `y=43–48`, `57–62`, `84–87` | Buildings / Water | setup, move into river |
| **Bridges** | narrow x on water rows | 1-tile wide | 2 NPC + long move |
| **`(66, 10)`** | narrow + Buildings | rescue north | setup |
| **`(90, 15)`** | Marnie counter | Buildings + Front | path / dialogue |
| **`(79–83, 10–14)`** | stumps | Buildings | walk target |
| **Front clusters** | `(10–11,13)`, `(42–44,13)`, `(0,6)` | visibility / clip | dense dialogue framing |
| **AlwaysFront** | `(104,19)`, `(117,19)` | foliage overlay | actor on tile — **needs check** |
| **Map edges** | `x=0`, `x=119`, `y=0` | warp / void | end move |
| **`(67, 12)`** | truck sprite `(67,12)` | overlap rescue | offset sprite carefully |
| **120×120 scale** | anywhere | no viewport = wrong biome on screen | always set viewport for new scenes |
| **SVE IF2R** | Farm/Forest warps | runtime patch | re-test after farm type change |

---

## 8. Movement guidance

### Good

- **Короткие движения по тропе:** `move Harvey -11 0 3` on `y=13` (storm); `move -3 0` / `-2 0` (E3/E3B).
- **Harvey подходит и останавливается на дистанции:** warp east → move west → stop 1 tile from farmer `(24,13)` vs `(23,13)`.
- **Farmer отступ на 1 тайл:** E3B fork «отойти» — optional `move farmer 0 2 2` (backlog polish).
- **`proceedPosition`** после парных move.
- **`changeLocation Forest`** → full re-setup farmer + Harvey + viewport.
- **FirstDate:** single-axis `move farmer 0 7 2` on open `(65,38)` → `(65,45)`.

### Avoid

- **Движение через деревья/кусты** (Buildings blocked) — use path corridors §2.
- **Длинные маршруты по мостам** — water both sides, narrow.
- **Движение вдоль воды** без TMX pass check on every tile.
- **Setup под плотным AlwaysFront / Front** — `(0,6)`, `(114,45)`.
- **`advancedMove` >4–5 tiles** без пошагового audit on 120×120.
- **Диагональные long moves** через `(57,59)` center — Front density.
- **Rescue sprite `(67,12)`** + Harvey `(65,16)` — verify no overlap / z-order.

---

## 9. Events using Forest

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| `eventHarveyStormComfortForest` | events.json | **checked-ok** | farmer `(23,13)`; Harvey warp `(35,13)` → move `-11 0` → `(24,13)`; rain; no viewport |
| `HarveyOverhaulStory.E3_ForestApothecary` | events.json | **checked-ok** | `(50,13)` / `(51,13)`; move `-3 0`; sunny Thu–Sat |
| `HarveyOverhaulStory.E3B_WingPatient` | events.json | **checked-ok** | `(48,14)` / `(49,14)`; animate Harvey kneel; move `-2 0` |
| `eventHarveyFirstDate` | events.json | **manually-verified-do-not-touch** | `viewport (65,45)`; farmer `(65,38)` Harvey `(65,46)`; sprite `(66,44)`; picnic |
| `acceptWalk` | events.json (fork) | **manually-verified-do-not-touch** | Farm `eventHarveyFirstWalk` fork → **`changeLocation Forest`**; Forest coords **needs check** |
| `eventRescueOperation` | events.json | **needs-review** | Forest: farmer `(66,16)` Harvey `(65,16)` Lewis `(73,17)`; sprite `(67,12)`; multi-loc |

**Не в content / dead:** `MyMod_HarveyStormComfortForest` in `events_for_mode_new_formatted.json` — не загружается.

---

## 10. Quick Forest rules

1. **120×120** — всегда задавай **`viewport`** для новых сцен.
2. **Storm canon:** farmer **`(23,13)`**, Harvey **`(35,13)`** → **`move -11 0 3`**.
3. **E3:** **`(50,13)` / `(51,13)`**; **E3B:** **`(48,14)` / `(49,14)`**.
4. **FirstDate:** **`(65,38–46)`**, viewport **`(65,45)`** — **manually verified, do not touch**.
5. **Rescue pickup:** **`(66,16)`**, truck sprite **`(67,12)`** — pass TMX OK.
6. **Farm вход:** warp **`(67–72, -1)`**; тропа **`x=64–75`**, не setup на **`(67,10)`**.
7. **Town выход:** east **`(120, 24–27)`**; подход **`(115–118, 24–27)`**.
8. **Woods:** west **`(-1, 6–7)`**; **`(0,6)`** Front.
9. **Beach:** **нет** прямого warp — только через Town.
10. **Вода** — только мостовые тайлы §2; иначе **Broken**.
11. **Marnie `(90,15)`** — не блокировать counter.
12. **Короткие move** (≤4–11 по одной оси на verified paths).
13. **Storm:** не использовать open picnic **`(65,45)`**.
14. **Front/AlwaysFront** — pass OK, но NPC может скрыться; проверять кадр.
15. **Stumps `(79–83,10–14)`** — декор, не walk.
16. **`changeLocation`** → re-warp всех actors.
17. **`acceptWalk` / FirstDate** — **do not touch** без явного запроса.
18. **SVE canopy / IF2R** — финальный in-game скрин после патчей.
19. **Brook / bridge scenes** — 1 NPC или static farmer; **`needs in-game check`**.
20. **Новые coords** — TMX pass + visual meaning, иначе помечай **`needs check`**.

---

**Метод:** TMX `tmpMap/sve/maps/Locations/Forest.tmx` + [`events-coordinate-audit.md`](../events-coordinate-audit.md) + [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md).  
**Не учтено:** runtime `.tbin`, engine Front collision vs NPC hitbox, seasonal tree swap.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
