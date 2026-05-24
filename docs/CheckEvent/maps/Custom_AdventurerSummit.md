# Map Passport: Custom_AdventurerSummit

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`Custom_AdventurerSummit`** — имя локации в CP/triggers; в SVE asset часто **`AdventurerSummit.tmx`** |
| **Related names** | **`Mountain`** — южный выход **`(31–33, 43)` → `(79, 1)`**; **`Mine`** — warp **`(19, 14) → (18, 13)`**; **`AdventureGuild`** — дверь **`(32, 21)`**; **`Railroad`** — west **`LoadMap (0, 14–15)`** (SVE shortcut) |
| **Map asset** | `Maps/Custom_AdventurerSummit` |
| **Map file (audit)** | `tmpMap/sve/maps/NewLocations/AdventurerSummit.tmx` |
| **Source** | **SVE custom** — **нет vanilla `.xnb`**; только Stardew Valley Expanded (`FlashShifter.StardewValleyExpandedCP`) |
| **Size** | **65×43** tiles, 16×16 px |
| **Passable (TMX)** | **~750** tiles; **Buildings blocked ~2033**; узкие тропы и скальные «острова» |
| **Status** | **partial** — TMX в repo (SVE snapshot); runtime **EditMap** (Railroad shortcut, Mist, leaves) может отличаться — **needs in-game export** после патчей |
| **Properties** | `Outdoors=T`; `BrookSounds`; `DayTiles`/`NightTiles`/`Light` (фонари у Mine/Guild); `AllowGrassSurviveInWinter=T` |

**Used by events:**

| Event ID | File | Summit act? |
|----------|------|-------------|
| `eventHarveyStormComfortMountain` | events.json | **yes — act 1 only** (act 2 = **`Mountain`**, см. [`Mountain.md`](Mountain.md)) |

**Not on Summit:** E4B, MountainDate, mine rescue — **`Mountain`** / **`Mine`**. Vanilla **Mountain** без SVE **не содержит** эту локацию.

**Связанные документы:** [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md), [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`events-map-fix-backlog.md`](../events-map-fix-backlog.md), [`Mountain.md`](Mountain.md), [`Mine.md`](Mine.md), [`maps-and-tilesets-inventory.md`](../maps-and-tilesets-inventory.md).

---

## 2. Required files

### Где лежит карта

| Location | Path | Notes |
|----------|------|-------|
| **Repo (audit snapshot)** | `tmpMap/sve/maps/NewLocations/AdventurerSummit.tmx` | Использован для паспорта; **не** runtime `.tbin` |
| **SVE mod source** | `[CP] Stardew Valley Expanded/assets/Maps/NewLocations/AdventurerSummit.tmx` | Канонический источник; Load как **`Custom_AdventurerSummit`** |
| **Vanilla game** | **отсутствует** | Без SVE локации **нет** |
| **Runtime export** | `debug export current` на Summit в save с SVE | Нужен если EditMap-патчи отличаются от repo TMX |

### Tileset’ы (из TMX)

| Tileset name | firstgid | Image source | Role |
|--------------|----------|--------------|------|
| **outdoors** | 1 | `spring_outdoorsTileSheet` | База трава/земля/склон (сезон меняется в runtime) |
| **Paths** | 1976 | `paths` | NPC paths, тропы |
| **zspring_town** | 2040 | `spring_town.png` | SVE town overlay |
| **untitled tile sheet2** | 4344 | `spring_outdoorsTileSheet2` | Outdoor detail |
| **zspring_z_extras** | 5464 | `spring_z_extras.png` | SVE extras |
| **zspring_beach** | 11864 | `spring_beach.png` | Декор |
| **v16_Waterfalls** | 12391 | `spring_Waterfalls` | Водопады / скальные потоки |
| **spring_Shadows** | 13291 | `spring_Shadows.png` | Тени canopy |
| **zspring_SVE_Tilesheet2** | 13766 | `spring_SVE_Tilesheet2.png` | **Уникальные SVE-тайлы Summit** — guild, скалы, мосты |

**Layers:** Back, Back2, Back3, Back6, Buildings, Buildings2, Buildings3, Paths, Front, AlwaysFront, AlwaysFront2.

### Если карты нет / устарела

1. Установить **SVE** и зайти на **Adventurer Summit** (через Mountain north или Mine `(18,14)`).
2. **`debug export current`** → сравнить с `AdventurerSummit.tmx`.
3. Экспортировать **seasonal** sheets (`spring_`/`summer_`/…) если правите визуал зимой.
4. Проверить **EditMap:** `AdventurerSummit_Railroad_Shortcut`, `AdventurerSummit_Mist`, `OriginalMinesEntrance` — могут менять warps/collision.

**Без SVE:** локация недоступна → CP-события на `Data/Events/Custom_AdventurerSummit` **не сработают**.

---

## 3. Important areas

| Area | Approx range / anchor | Role | Staging notes |
|------|----------------------|------|---------------|
| **Entrance / exit south (→ Mountain)** | **`x=31–33`, `y=42–43`** (warp row) | Спуск на **`Mountain (79, 1)`** | Harvey spawn **`(32, 42)`** — **1 tile north** of warp; **не block** `(31–33, 43)` |
| **Mine entrance (northwest hub)** | **`x=17–21`, `y=12–17`**; warp **`(19, 14)`** | Шахта, навес, «укрытие у входа» | Колонка **`x=19`** passable **`y=12–16`**; **не setup on `(19,14)`** warp |
| **Adventure Guild door** | **`(32, 21)`** Buildings | **Крыша гильдии** — лучшее «укрытие» на карте | Approach **`(33, 22)`–`(33, 24)`** pass; door tile **blocked** |
| **Cliff / scenic east (storm act 1)** | **`x=38–42`, `y=25–30`**; anchor **`(41, 28)`** | **Опасная высота** — narrativ грозы | Скальный декор **`(49–55, 27–31)`** — Buildings messages, **не walk** |
| **Summit viewpoint / rock messages** | **`x=49–55`, `y=27–31`** | Визуал «вершины», подписи `AdventurerSummit.1–5` | **Blocked** — только фон кадра, не staging |
| **Sheltered path (south approach)** | **`x=29–35`, `y=22–28`** | Тропа снизу к центру | Широкая полоса pass; Harvey intercept path |
| **West railroad shortcut** | **`x=0`, `y=14–15`** | `LoadMap Railroad 54 26` | **Edge** — transition, не dialogue |
| **Northeast rim** | **`x=58–64`, `y=24–31`** | Край карты, узкий выступ | **Risky** — мало passable tiles |
| **Central slope field** | **`x=13–58`, `y=7–42`** (major passable island) | Общее поле staging | **~750** tiles total; много `#` скал внутри |

---

## 4. Doors/warps/exits

Подтверждено property **`Warp`** на **`tmpMap/sve/maps/NewLocations/AdventurerSummit.tmx`**.

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **31** | **43** | **Warp** | **Mountain `(79, 1)`** | **`(31, 42)`**, **`(32, 42)`** — pass | Южный выход; **OOB row** `y=43` — transition only |
| **32** | **43** | **Warp** | **Mountain `(79, 1)`** | **`(32, 42)`**, **`(33, 42)`** | Storm Harvey spawn adjacent |
| **33** | **43** | **Warp** | **Mountain `(79, 1)`** | **`(33, 42)`** | Тройной warp — **не block** |
| **19** | **14** | **Warp** | **Mine `(18, 13)`** | **`(19, 13)`**, **`(20, 15)`**, **`(20, 16)`** | Mine hub; **не setup** on warp |
| **32** | **21** | **LockedDoorWarp** | **AdventureGuild `(6, 19)`** | **`(33, 22)`**, **`(32, 23)`** | **Buildings blocked** — door only |
| **0** | **14** | **LoadMap** | **Railroad `(54, 26)`** | **`(1, 14)`** — **needs check** | West edge SVE shortcut |
| **0** | **15** | **LoadMap** | **Railroad `(54, 26)`** | **`(1, 15)`** | Same |

**External entry:**

| From | Spawn / warp | Notes |
|------|--------------|-------|
| **Mountain north** | **`Mountain (78–80, -1)` → Summit `(32, 41)`** (SVE) | Обратная связь act 2 storm; см. [`Mountain.md`](Mountain.md) |
| **Mine south** | **`Mine (18, 14)` → Summit `(19, 14)`** | SVE `OriginalMinesEntrance` — **re-verify** warps |

---

## 5. Obstacles and visual blockers

| Type | Where | Blocks movement? | Event impact |
|------|-------|------------------|--------------|
| **Cliffs / rock walls** | Scattered `#` clusters; messages **`(26–28, 18–20)`**, **`(49–55, 27–31)`** | **Yes** (Buildings) | **Never setup**; scenic backdrop only |
| **Large rocks / boulders** | East scenic **`x≥43`, `y=25–31`** near `(41,28)` | **Yes** | Act 1 «высота» — rocks **in frame**, actors on **`x≤42`** pass strip |
| **Trees / bushes** | Back seasonal spawn; brook zones north | Mixed | Storm: prefer **path/grass**, не inside Front trunk |
| **Paths (Paths layer)** | NPC routes через центр | Usually pass | `advancedMove` должен следовать **walkable**, не Paths-only |
| **Summit edge / void** | **`y=43`**, **`x=0` west**, **`x=64` east** | Void / warp | **No long move** to edge — viewport clamp |
| **Guild building** | **`(32, 21)`** + surround **`y=19–21`, `x=29–35`** | **Yes** near door | Use **approach tiles** `(33,22+)` for shelter staging |
| **Mine shaft building** | **`x=17–21`, `y=12–14`** | Partial | Warp **`(19,14)`** blocked; pass south **`y=15+`** |
| **Decorative Messages** | `AdventurerSummit.1–7`, `Summit.1` | **Yes** | Sign tiles — don't dialogue on |
| **Front / AlwaysFront** | Rails, cliff lips, waterfall foam | Partial | NPC **visible** but path may look float — **needs in-game check** |
| **Waterfalls (v16_Waterfalls)** | North/center brook **`BrookSounds`** | Often **non-walk** adjacent | Don't `move` into water tiles |
| **Unknown custom tiles** | **`zspring_SVE_Tilesheet2`** unique IDs | **Verify per export** | New coords → **`Coordinates require exported map`** |

---

## 6. Safe staging zones

Zones подтверждены на **repo TMX** — re-verify после SVE EditMap и `debug export current`.

### `summit_view_dialogue`

| Field | Value |
|-------|-------|
| **Range** | **`x=38–42`, `y=26–30`** — open slope east of center |
| **Farmer anchor** | **`(41, 28)`** — **storm act 1 canon** (setup `farmer 41 28 1`) |
| **Harvey anchor** | **`(40, 28)`** or **`(39, 28)`** — **1–2 tiles** west, face farmer |
| **Directions** | Farmer **2** (south, fear crouch in storm); Harvey **1** or **0** approaching from west/south |
| **Viewport** | **`41 27`** — setup line in script (center on farmer + horizon) |
| **Movement** | **Static** preferred; max **`move ±1,0`** or **`0,±1`** — emotional / trust talk |
| **Visual meaning** | **Возвышенность** — скалы **`(49+)`** в кадре, открытое небо — «опасная высота при грозе» ([`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md)) |
| **Risks** | **Не step east** into rock Buildings; no **`viewport` missing** on large map — player loses scenic read; **не overlap** on same tile |

### `summit_storm_shelter`

| Field | Value |
|-------|-------|
| **Range** | **Option A — Guild:** **`x=31–34`, `y=22–24`**; **Option B — Mine mouth:** **`x=18–22`, `y=15–17`** |
| **Farmer anchor** | Guild: **`(33, 23)`**; Mine: **`(20, 16)`** |
| **Harvey anchor** | Guild: **`(33, 22)`** face **2** under eaves; Mine: **`(21, 16)`** near shaft overhang |
| **Directions** | Both **2** (south) under shelter or face each other **3/1** |
| **Viewport** | Guild: **`33 22`**; Mine: **`20 15`** |
| **Movement** | **Short** — **`move 0 -3`** max toward shelter; prefer **`globalFade`** if crossing rocks |
| **Visual meaning** | **Укрытие от грозы** — крыша гильдии (best) или навес шахты (единый visual language с Mine storm) |
| **Risks** | **Don't stand on `(32, 21)` door**; **don't block `(19, 14)`** warp; current script **doesn't use** guild — backlog alternative only |

### `summit_path_intercept`

| Field | Value |
|-------|-------|
| **Range** | **South path:** **`x=29–35`, `y=28–42`** — Harvey meets farmer moving downslope |
| **Farmer anchor** | Start **`(41, 28)`** → intended **`(32, 28)`** after `advancedMove -9 0` (storm canon) |
| **Harvey anchor** | **`warp (32, 42)`** → move north to **`(32, 28)`** / **`(40, 28)`** area |
| **Directions** | Harvey **0** (north) on spawn at `(32,42)`; then **1** or **0** toward farmer |
| **Viewport** | **`41 27`** act 1 open; consider **`37 28`** when both converge |
| **Movement** | **Canon (needs-review):** `advancedMove Harvey false 0 -14 8 0` — audit **Warning** (path through **`(34–37, 33–38)` Broken**). **Safer:** `move 0 -5` on column **`x=32`** (pass **`y=42→28`**) + `move 8 0` on **`y=28`** OR **`globalFade` + warp Harvey `(40,28)`** |
| **Visual meaning** | Harvey **снизу по тропе** (`32,42` near Mountain warps) перехватывает farmer на склоне |
| **Risks** | **Long diagonal `advancedMove` through rocks** — stuck; **`stopAdvancedMoves`** before `changeLocation`; block **`(31–33, 43)`** warps |

---

## 7. Summit-specific rules

1. **Не ставить персонажей на край обрыва**, если в кадре виден пустой void — особенно **`y=42`** у south warps и **`x=0`** west.
2. **Не использовать фиксированные координаты без карты** — SVE-only; без мода локация не существует.
3. **Storm comfort act 1** должен **читаться как «опасная высота → эвакуация вниз»** — viewport **`41 27`**, rocks east in frame.
4. **Storm shelter** — визуально лучше **Guild `(33,22)`** или **Mine `(20,16)`**, чем открытый **`(41,28)`** для *finale* (текущий скрипт финалит на Mountain — OK).
5. **Camera** — показывать **summit + склон/скалы**; после fade act 2 — **`Mountain viewport (76,15)`** (не Summit).
6. **Movement короткий** — **`≤5 tiles`** per segment on Summit; **`advancedMove` через центральные скалы** — backlog **Broken**.
7. **Не block south warps `(31–33, 43)`** — softlock выхода на Mountain.
8. **Не setup on `(19, 14)`** Mine warp — игрок не войдёт в шахту.
9. **`warp Harvey (32, 42)`** — **OK** TMX; adjacent to warps — intentional «с тропы снизу».
10. **Farmer `(41, 28)`** — **OK** pass; **не путать** с viewport line **`41 27`**.
11. **После `changeLocation Mountain`** — Summit coords **не действуют**; re-setup обязателен.
12. **`OriginalMinesEntrance`** — перепроверить **`Mine↔Summit`** warps на save.
13. **Front/AlwaysFront** на тропах — in-game check после движения.
14. **Новые summit-сцены** — только с **`debug export current`**; repo TMX = baseline, не gospel runtime.

---

## 8. Risk zones

| Zone | Coords/range | Risk | Why |
|------|--------------|------|-----|
| **South Mountain warps** | **`(31–33, 43)`** | **Critical** | Instant **`Mountain (79,1)`** — NPC block = softlock |
| **Harvey spawn row** | **`(32, 42)`** adjacent warps | **High** | One step south → transition |
| **Mine warp** | **`(19, 14)`** | **Critical** | Warp → **`Mine (18,13)`** |
| **Guild door** | **`(32, 21)`** | **High** | Buildings + LockedDoorWarp |
| **Scenic rock messages** | **`(49–55, 27–31)`** | **High** | Blocked — overlap if warp wrong |
| **advancedMove rock diagonal** | **`(34–37, 33–38)`** | **Critical** | Audit **Broken** — Harvey/farmer stuck |
| **Narrow passages** | **`(26,3)`, `(16,6)`, `(28,8)`, `(6,21)`, `(0,23)`, `(56,28)`, `(27,34)`, `(22,40)`** | **High** | ≤1 neighbor — dual NPC stuck |
| **Map edges** | **`x=0` west LoadMap**, **`x=64` east**, **`y=42` south rim** | **High** | Void / transition |
| **West Railroad `(0,14–15)`** | **Medium** | LoadMap — accidental exit |
| **Waterfall/brook tiles** | North **`BrookSounds`** zone | **Medium** | Non-walk adjacent |
| **Unknown post-EditMap tiles** | Runtime-only patches | **Medium** | Mist, leaves, shortcut — **needs export** |
| **Open height `(41,28)`** | **Narrative OK / shelter weak** | **Low–Med** | Correct for «lightning risk», wrong for «под крышей» |

---

## 9. Events using this map

| Event ID | File | Act | Status | Summit coords (audit) | Notes |
|----------|------|-----|--------|----------------------|-------|
| **`eventHarveyStormComfortMountain`** | events.json | **Act 1** | **needs-review** | **`viewport 41 27`**; farmer **`(41, 28)`** f**1**→**2**; Harvey **`warp (32, 42)`** f**0** | Storm + **`buffStressThunder`** + **3♥** + **Random 0.4**; pre-dating med tone |
| **`eventHarveyStormComfortMountain`** | events.json | **Act 2** | **needs-review** | **`changeLocation Mountain`** — **not Summit** | farmer **`(79,1)`**, Harvey **`(79,0)`**, **`viewport (76,15)`** — см. [`Mountain.md`](Mountain.md) |

**Script preview (act 1 opening, catalog):**

```
rain/
41 27/
farmer 41 28 1 Harvey 1000 1000 0/
playSound thunder/
emote farmer 16/
faceDirection farmer 2/
animate farmer false true 3000 5 4/
warp Harvey 32 42/
speak Harvey "ОПАСНО! Ты на возвышенности во время грозы!..."
advancedMove Harvey false 0 -14 8 0/
...
changeLocation Mountain/
```

**Problems (doc/backlog only — do not fix in doc task):**

- **`advancedMove Harvey 0 -14 8 0`** — path through **Broken** rocks ([`events-coordinate-audit.md`](../events-coordinate-audit.md) **Warning**).
- **No other CP events** on `Data/Events/Custom_AdventurerSummit` per inventory.
- **Reachability:** requires **SVE + storm + buff/topic + Random 0.4** — часто **не срабатывает** ([`07-reachability-table.md`](../../events-inventory/07-reachability-table.md)).

**Vanilla/SVE non-CP:** guild quests, railroad shortcut — out of scope.

---

## 10. Quick Custom_AdventurerSummit rules

1. **SVE-only custom** — без SVE локации и события **нет**.
2. **Repo TMX:** `tmpMap/sve/maps/NewLocations/AdventurerSummit.tmx` — **65×43**, **~750** passable.
3. **Only CP event act 1:** `eventHarveyStormComfortMountain` — act 2 on **Mountain**.
4. **Canon farmer:** **`(41, 28)`**; **viewport `41 27`**.
5. **Canon Harvey entry:** **`warp (32, 42)`** — south path, **not** on warp row.
6. **Forbidden: setup on `(31–33, 43)`** — Mountain transition.
7. **Forbidden: setup on `(19, 14)`** — Mine transition.
8. **Forbidden: long `advancedMove` through `(34–37, 33–38)`** — Broken rocks.
9. **Prefer `globalFade` + warp** over diagonal chase across summit.
10. **Storm narrativ:** height **`(41,28)`** OK act 1; shelter alt **`(33,22)`** guild / **`(20,16)`** mine.
11. **Guild door `(32,21)`** — approach only, **2+ tiles** from door.
12. **Scenic rocks `(49–55, 27–31)`** — backdrop, **not** actor tiles.
13. **South path intercept** — column **`x=32`, `y=28–42`** passable on TMX.
14. **`stopAdvancedMoves`** before `changeLocation Mountain`.
15. **Mountain link:** south warps → **`(79,1)`**; north return **`Mountain (78–80,-1)`**.
16. **Mine link:** **`(19,14) ↔ (18,13)`** — check `OriginalMinesEntrance`.
17. **Camera act 1:** set **`viewport 41 27`** if default loses summit read.
18. **Runtime EditMap** — re-export before new fixed coords.
19. **Status partial** — TMX in repo, runtime may differ.
20. **Do not edit events** in documentation-only tasks — passport for review/planning.

---

**Метод:** `tmpMap/sve/maps/NewLocations/AdventurerSummit.tmx` + [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md) + [`events-coordinate-audit.md`](../events-coordinate-audit.md) + walkability parse (`generate_map_passports.py`).  
**Не учтено:** runtime `.tbin` / EditMap (Railroad, Mist, leaves), seasonal sheet swaps, exact `advancedMove` trajectory in engine.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
