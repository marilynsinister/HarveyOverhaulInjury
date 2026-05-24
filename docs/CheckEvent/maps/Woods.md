# Map Passport: Woods

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName** | Woods |
| **Map asset** | `Maps/Woods` (runtime: **SVE `Woods2.tmx`** → LocationName `Woods`) |
| **Map file** | `tmpMap/sve/maps/Locations/Woods2.tmx` (fallback: `tmpMap/vanilla/maps/Woods.tmx`) |
| **Source** | SVE Load (`Woods2.tmx`); vanilla Secret Woods base + SVE RedBaneberry / waterfall tilesets |
| **Size** | **90×75** tiles, 16×16 px |
| **Status** | **ready** (TMX в repo; DeepWoods / runtime `.tbin` — **needs in-game check**) |
| **Layers** | Back, Back2, Back3, Buildings, Buildings2, Front, Front2, AlwaysFront, AlwaysFront2, Paths |
| **Map properties** | `AmbientLight=120 65 120` (темнее обычного леса); `BrookSounds`; `Stumps`; `Outdoors=T` |

**Роль для мода:** **напряжённые лесные сцены** — единственная CP-локация `eventRescueOperation` (trauma / storm / поиск farmer под кустами). Связка с E5 через `topicRescueOperation`.

**Used by events:**

| Event ID | File | Woods act |
|----------|------|-----------|
| `eventRescueOperation` | events.json | farmer `(27,18)`, Harvey `(40,20)`, Lewis `(38,20)`; moves → `(27,19)` |

*(Полное событие: Hospital phone → **Woods** → Forest pickup → Hospital bed — см. [`Hospital.md`](Hospital.md), [`Forest.md`](Forest.md).)*

**Связанные документы:** [`events-coordinate-audit.md`](../events-coordinate-audit.md) §eventRescueOperation, [`events-map-fix-backlog.md`](../events-map-fix-backlog.md), [`story-arc-map-audit.md`](../story-arc-map-audit.md) §E5↔Rescue.

**Passability (TMX):** ~1573 passable; ~5146 Buildings blocked; ~191 Front-only (passable, visibility/move risk).

---

## 2. Important areas

### Entrance / exit

| Area | Coords | Link | Notes |
|------|--------|------|-------|
| **Forest (east)** | warp **`(82, 29–30)`** | → Forest **`(0, 7)`** | главный вход с большого леса; подход **`(78–80, 29–30)`** pass; **`(81–82, 29)`** Front |
| **Custom_ForestWest (south)** | warp **`(47–49, 71)`** | → Custom_ForestWest **`(98, 1)`** | южный выход SVE; не CP Harvey |
| **Нет warp** | — | Farm / Town / Hospital | только через Forest или другие карты |

Forest ↔ Woods: Forest `(-1, 6–7)` → Woods **`(81, 29)`** (Forest side); Woods **`(82, 29)`** → Forest **`(0, 7)`**.

### Forest paths (подтверждённые CP)

| Path | Range | Use |
|------|-------|-----|
| **Rescue approach row** | **`y=20`, `x=27–40`** | Harvey/Lewis horizontal moves — **все tiles pass, Front=0** |
| **North-west grove** | `(3–47, 4–42)` | open region center **`(23, 19)`** — «укрытие» narrative |
| **Central trail** | `(8–82, 6–71)` | main passable blob; center **`(50, 42)`** |
| **East choke** | `x=78–82`, `y=27–28` | **blocked** — к warp только с `y=29–30` |

### Secluded areas

| Zone | Center | Character |
|------|--------|-----------|
| **Hideout cluster** | **`(40, 20)`** | Harvey/Lewis staging; открытая поляна у «густых кустов» narrative |
| **Farmer curl-up** | **`(27, 18)`** | «под кустом» — **Front на тайле**; кольцо Front `y=18–21` |
| **South pond / statues** | `y≈58–66`, `x≈8–29` | SecretWoods.1 messages — **глухой декор**, не rescue |
| **Open pocket (alt)** | **`(46, 46)`** | 5×5 all pass, Front=0 — **needs check** для новых сцен (далеко от canon) |

### Possible rescue area (canon)

```
        x: 25  26  27  28  29  ...  38  39  40
y=18    #   #   F*  .   f       f   f   f
y=19    .   .   .   .   .       .   .   f
y=20    .   .   .   .   .  ...  .   .   .
y=21    f   f   f   f   f       .   .   f

F* = farmer warp (27,18) — Front overlay
.  = pass, open floor
f  = pass + Front (кусты/листва)
#  = blocked
```

- **Search / weak farmer:** **`(27, 18)`** (canon) или alt **`(28, 18)`** (pass, **no Front on tile** — backlog).
- **Harvey finds:** approach **`(27, 19)`** after `move 0 -1`.
- **Team staging:** Harvey **`(40, 20)`**, Lewis **`(38, 20)`**.

### Water / pond / river

| Feature | Coords (TMX) | Passable? | Notes |
|---------|--------------|-----------|-------|
| **BrookSounds 1** | **`(48, 38)`** | pass + **Front** | ручей/водопад audio; не setup для lying farmer |
| **BrookSounds 2** | **`(45, 9)`** | pass + **Front** | северная зона |
| **South pond** | `y≈58–66`, west | mostly **blocked** (SecretWoods wall) | визуально вода/кусты — **needs in-game check** |
| **Water tileset props** | gid 209+ (Water=t) | varies | см. [`tileset-reuse-guide.md`](../tileset-reuse-guide.md) — visual check |

### Dense trees / AlwaysFront cover

- **Карта в целом** — Secret Woods: высокая плотность Buildings (стволы) + Front (кусты).
- **AmbientLight `120 65 120`** — приглушённый свет; storm-сцены **ещё темнее** (`rain`/`thunder`) — нужен **явный viewport**.
- **Front pockets:** `(27,18)` area, `(40,18–19)`, east warp strip, stump `(40,26)` Front.
- **Stumps (map property):** `(24,6)`, `(29,7)`, `(26,10)`, `(38,21)`, `(40,26)`, `(49,13)`, `(63,9)`, `(69,8)`, `(66,24)`, `(63,30)`, `(57,51)`, `(54,59)`, `(32,49)` — декор; **не walk targets**.

---

## 3. Doors, warps, exits

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **82** | **29** | **Warp** | **Forest `(0, 7)`** | **`(78–80, 29–30)`** pass | Front на `(81–82, 29)`; **не setup** на warp |
| **82** | **30** | **Warp** | **Forest `(0, 7)`** | **`(78–80, 30)`** | Front на `(78–82, 30)` |
| **47** | **71** | **Warp** | Custom_ForestWest `(98, 1)` | **`(46–48, 70)`** — **needs check** | south edge |
| **48** | **71** | **Warp** | Custom_ForestWest `(98, 1)` | same | |
| **49** | **71** | **Warp** | Custom_ForestWest `(98, 1)` | same | |

**Нет** LockedDoorWarp на Woods2 для Harvey CP. Interior transitions — только через Forest/Town chain в `eventRescueOperation`.

---

## 4. Natural blockers and visibility

| Object / range | Blocks movement? | Blocks visibility? | Notes |
|----------------|------------------|--------------------|-------|
| **Tree trunks (Buildings)** | **yes** | partial | большая часть карты; не `advancedMove` «сквозь лес» |
| **Front bushes / ferns** | no (pass) | **yes** | farmer `(27,18)` **inside Front** — персонаж может «пропасть» |
| **AlwaysFront2 overlay** | no | **yes** | stacking с Front — **needs in-game check** |
| **Stumps (Stumps property)** | mixed | partial | `(38,21)`, `(40,26)` near hideout; `(26,10)` north |
| **Water / brook tiles** | often blocked | — | Brook `(48,38)`, `(45,9)` pass+Front |
| **SecretWoods south wall** | **yes** | yes | `y≈58–66`, Messages SecretWoods.1 |
| **Narrow tiles (≤1 neighbor)** | pass | — | `(24,15)`, `(20,17)`, `(30–32,18–19)`, `(21,23)` near rescue |
| **East edge choke** | **yes** | — | `x=78–82`, `y=27–28` blocked |
| **SVE RedBaneberry decor** | varies | partial | tileset-specific — **needs in-game check** |
| **DeepWoods mod patch** | unknown | unknown | если установлен — **needs in-game check** |

**Visibility rule:** если `has_front(x,y)=true` на farmer tile → считать **high risk** для trauma/rescue readability.

---

## 5. Safe staging zones

### `woods_rescue_entry_area`

| Field | Value |
|-------|-------|
| **Range** | **`(25, 16)–(32, 22)`** around farmer spawn; approach corridor **`y=20`, `x=27–40`** |
| **Safe Harvey tile** | **`(40, 20)`** warp (off-screen team) — pass, no Front |
| **Safe farmer tile** | **`(28, 18)`** preferred visibility alt; canon **`(27, 18)`** — pass but **Front Warning** |
| **Viewport** | **`viewport 27 18 true`** or **`viewport 33 20 true`** (wider pair) — canon **needs check** (script may omit) |
| **Movement** | Harvey **`move -7 0 3`** → **`-4 0`** → **`-2 0`** on **`y=20`** (TMX **all OK**); then **`move 0 -1 0`** → **`(27, 19)`** |
| **Risks** | farmer Front at `(27,18)`; narrow `(30–32,18–19)`; не начинать длинный path с `(82,29)` через choke |

**Purpose:** вход в Woods-act после Hospital phone; farmer уже «под кустами»; Harvey/Lewis **appear at hideout**, не at Forest warp.

### `woods_rescue_focus_area`

| Field | Value |
|-------|-------|
| **Range** | **`(26, 17)–(29, 21)`** farmer; Harvey finale **`(27, 19)`**; Lewis **`(28, 19)–(30, 21)`** |
| **Safe Harvey tile** | **`(27, 19)`** face **2** (south to farmer); start moves from **`(40, 20)`** |
| **Safe farmer tile** | **`(27, 18)`** or **`(28, 18)`**; lying/crouch **`animate farmer … 5 4`** / fear — **не** на stump |
| **Viewport** | **`(27, 18)`** tight on farmer face/blood narrative; hold during Harvey approach |
| **Movement** | Harvey: **only** verified horizontal **`y=20`** chain + **1 tile north**; Lewis: **`(38,20)`** → **`(32,20)`** → **`(28,20)`** → **`(28,19)`** → retreat **`(28,21)`** → **`(30,21)`** — all pass TMX |
| **Risks** | farmer in Front bush; Harvey must **not** cut through `(24,15)` / `(20,17)` narrow; no `advancedMove` through trees |

**Purpose:** основная trauma-сцена — weak/collapsed farmer, Harvey осторожный подход, осмотр головы, триггер грома.

### `woods_private_dialogue_area`

| Field | Value |
|-------|-------|
| **Range** | **`(28, 19)–(32, 22)`** after Lewis backs off; Harvey **`(27, 19)`** / **`(28, 20)`** |
| **Safe Harvey tile** | **`(27, 19)`** or **`(28, 20)`** face **2/3** toward farmer |
| **Safe farmer tile** | **`(27, 18)`** / **`(28, 18)`** static (crouch animate) |
| **Viewport** | keep **`(27, 18)`** — двое в кадре; Lewis off at **`(30, 21)`** |
| **Movement** | **static** или 1-tile `faceDirection`; Lewis already moved away per script |
| **Risks** | Front ring `y=21`; storm darkness + AmbientLight — проверить читаемость портретов |

**Purpose:** тихий разговор («это гром», согласие на клинику) после паники; минимум движения.

---

## 6. Rescue operation guidance

### Читаемость сцены

1. **Камера важнее длины пути.** Canon использует короткие **`move`** по **`y=20`** (14 tiles horizontal, все pass) — **хороший паттерн**.
2. **Задай `viewport`** на farmer **`(27,18)`** до подхода Harvey; иначе на 90×75 игрок видит «случайный лес».
3. **AmbientLight + storm** — персонажи темнее; избегай дополнительных `ambientLight` без нужды.
4. **Трое в кадре:** Lewis **`(38,20)`** старт → отступ **`(30,21)`**; не держать всех на одном tile.

### Farmer placement

| Do | Don't |
|----|-------|
| **`(27,18)`** или **`(28,18)`** на **open pass** floor | Buildings stump **`(38,21)`** as farmer anchor |
| **`animate` crouch / fear** on pass tile | `positionOffset` deep into bush without test |
| **`(28,18)`** if Front overlap on `(27,18)` | lying **`animate 4 5`** under dense Front (invisible) |

### Harvey movement

| Do | Don't |
|----|-------|
| Warp **`(40, 20)`** → **`move -7 0 3`** → **`-4 0`** → **`-2 0`** → **`move 0 -1 0`** | `advancedMove` через северные `(24,15)` / `(20,17)` |
| **`speed Harvey 4–5`** on straight row | diagonal cut through `(30–32,18)` narrow |
| Stop **`(27,19)`** 1 tile south of farmer | long path from Forest warp **`(82,29)`** |

### Storm / night visibility

- Entry preconditions: **`Weather Storm`** + `topicRescueOperation`.
- **`thunder_small` / `rain`** in script — OK narrativ; **viewport обязателен**.
- South pond / deep woods **`y>55`** — слишком тёмно и узко для новых rescue-сцен.
- Prefer **north-west quadrant** `x<50`, `y<45` (verified rescue).

### Multi-location note (не править здесь)

Woods act → `changeLocation Forest` **`(66,16)`** → Hospital bed — Forest/Hospital coords in sibling passports. Overall event status **Broken** (Hospital `warp farmer 20 5` without bed pattern) — **document only**, не CP fix в этой задаче.

---

## 7. Risk zones

| Coords / range | Risk | Why | Avoid |
|----------------|------|-----|-------|
| **`(27, 18)`** | **Front overlay** | farmer visibility | alt **`(28,18)`** — backlog only |
| **`(30–32, 18–19)`** | narrow | ≤1 neighbor | two NPC + move |
| **`(24, 15)`, `(20, 17)`, `(21, 23)`** | narrow | choke | Harvey shortcuts |
| **`(78–82, 27–28)`** | blocked | east choke | approach from Forest warp |
| **`(81–82, 29–30)`** | warp + Front | transition | setup NPC |
| **`(47–49, 71)`** | warp south | edge | block exit |
| **Brook `(48,38)`, `(45,9)`** | water decor + Front | slip narrative | dialogue setup |
| **Stumps `(38,21)`, `(40,26)`** | Front / visual clutter | near hideout | farmer/Harvey anchor |
| **South `y=58–66`** | dense SecretWoods | blocked maze | rescue / dialogue |
| **Map edges** | `x<3`, `x>85`, `y<4`, `y>72` | void / warp | long moves |
| **Open `(46,46)`** | far from canon | continuity | new scenes only after check |
| **DeepWoods expansion** | mod patch | unknown layout | in-game if mod on |

---

## 8. Events using Woods

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| **`eventRescueOperation`** | events.json | **needs-review** (overall **Broken** — Hospital fin) | **Woods act OK-ish:** farmer **`(27,18)`** Warning Front; Harvey **`(40,20)`** OK; Lewis **`(38,20)`** OK; moves on **`y=20`** OK. Phone = Hospital. Pickup = Forest. Bed = Hospital — см. Hospital passport. |

**Preconditions (Woods entry):** `Weather Storm`, Friendship Harvey **600**, topic **`topicRescueOperation`** (C# after E5 — reachability см. events-inventory).

**Единственное CP-событие** на Woods по [`06-locations-index.md`](../../events-inventory/06-locations-index.md).

---

## 9. Quick Woods rules

1. **90×75 Secret Woods** — всегда **`viewport`** для trauma/rescue.
2. **Rescue canon:** farmer **`(27,18)`**; Harvey **`(40,20)`**; Lewis **`(38,20)`**.
3. **Alt farmer tile:** **`(28,18)`** — pass, no Front (backlog visual fix).
4. **Harvey approach:** only **`y=20` horizontal** moves → **`(27,19)`** — TMX verified.
5. **Не** `advancedMove` через деревья — только straight rows on open tiles.
6. **Farmer не на stump** — stumps list §2; `(38,21)`, `(40,26)` near scene.
7. **Front tile = visibility risk** — проверять in-game скрин.
8. **Forest exit** east **`(82,29–30)`** — не block; не setup on warp.
9. **Нет** Farm/Town warp — вход через Forest **`(0,7)`** side.
10. **Brook / pond** — не staging для collapsed farmer.
11. **South SecretWoods `y>55`** — не для rescue.
12. **AmbientLight** dark — storm + rain; держать камеру близко.
13. **Lewis retreat `(30,21)`** — private Harvey talk space.
14. **Narrow tiles** §7 — один NPC at a time.
15. **Multi-loc:** после Woods → Forest **`(66,16)`** — re-setup all actors.
16. **Hospital fin** broken in audit — bed pattern in Hospital.md — **do not fix here**.
17. **DeepWoods mod** — retest if installed.
18. **New coords** — TMX pass + **empty Front in 3×3** around farmer.
19. **Короткий подход + сильная камера** > длинный маршрут через лес.
20. **Manually verified events:** none on Woods yet — rescue is **needs-review**, not do-not-touch.

---

**Метод:** TMX `tmpMap/sve/maps/Locations/Woods2.tmx` + [`events-coordinate-audit.md`](../events-coordinate-audit.md).  
**Не учтено:** runtime `.tbin`, DeepWoodsCompatibility, engine Front vs sprite hitbox.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
