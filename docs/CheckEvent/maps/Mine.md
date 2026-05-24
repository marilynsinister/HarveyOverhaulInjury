# Map Passport: Mine

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`Mine`** — статичная карта **входа в шахту** (`Maps/Mine`) |
| **Related names** | **`MineShaft`** — процедурные подземные уровни (этажи); **`Mountain` LoadMap `(103,16)`** — внешний вход с горы; **не путать** |
| **Map asset** | `Maps/Mine` |
| **Map file (audit)** | `tmpMap/sve/maps/Locations/Mine.tmx` (also `tmpMap/Mine.tmx`) |
| **Source** | **SVE Load** (`Mine.tbin` replaces vanilla); entrance layout **fixed** 77×20; **MineShaft = procedural** |
| **Size** | 77×20 tiles, 16×16 px |
| **Status** | **risky** — coords **stable only on confirmed `Mine` entrance TMX**; **never** on `MineShaft` / unexported runtime |
| **Properties** | `AmbientLight=80 80 40`; **`ViewportFollowPlayer=true`**; dark mine lighting |

**Used by events:**

| Event ID | File | Mine act? |
|----------|------|-----------|
| `eventHarveyMineRescue` | eventsMineRescue.json | yes → Hospital |
| `eventHarveyMineRescueDating` | eventsMineRescue.json | yes → Hospital |
| `eventHarveyMinorMineRescue` | eventsMineRescue.json | yes → Hospital |
| `eventHarveyMineInterception` | eventsCare.json | yes (stay in Mine) |
| `eventHarveyStormComfortMine` | events.json | yes (act 1) → Town act 2 |

**Not on Mine map:** `eventHarveySkullCavePrevention` → **SkullCave** (trigger bug if fired from Mine — см. [`mine-events-map-risk-audit.md`](../mine-events-map-risk-audit.md)).

**Связанные документы:** [`tmpMap/Mine_event_placement_analysis.md`](../../tmpMap/Mine_event_placement_analysis.md), [`mine-events-map-risk-audit.md`](../mine-events-map-risk-audit.md), [`Hospital.md`](Hospital.md) (aftermath bed), [`Mountain.md`](Mountain.md) (LoadMap `(103,16)`).

---

## 2. Почему Mine рискованная

### Две разные «шахты»

| Concept | Stable layout? | CP fixed coords? |
|---------|----------------|------------------|
| **`Mine` (entrance)** | **Yes** — одна карта 77×20 | **Yes** — `(17,7)`, `(17,10)`, etc. **if TMX matches runtime** |
| **`MineShaft` (levels)** | **No** — процедурная генерация этажей | **No** — любой `(x,y)` может быть стена/руда/лестница/вода |
| **Combat rooms underground** | **No** | **Never** stage rescue/dialogue here |

### Почему фиксированные координаты ломаются

- Игрок **умирает на этаже 80** `MineShaft`, а cutscene играет на **входе** `Mine` — это **намеренный** C# warp, не «там где упал».
- **`OriginalMinesEntrance` (SVE)** меняет связку Mountain/Summit ↔ Mine; warp **`(18,14)`** может вести на **Custom_AdventurerSummit**, не vanilla Mountain.
- **Runtime `.tbin`** может отличаться от `tmpMap/Mine.tmx` в repo — coords **не safe** без `debug export current` под ваш save.
- **Rescue с long movement** через `(23,9)` спуск, вагонетку, `y≥11` — **Broken** / NPC stuck.
- **Без C# pre-warp** major rescue на wrong map или wrong tile — event **Broken**.

### Безопаснее для стабильных сцен

1. **Короткая сцена** только на **подтверждённом входе** `Mine` (коридор `x=17`, `y=6–10`).
2. **`globalFade` + `changeLocation Hospital`** для aftermath (major/minor rescue).
3. **«Перехват перед шахтой»** для narrativ «не лезь» — предпочтительно **Mountain** flank `(104–105,18)` **или** короткий **Mine** `(17,7)` без спуска.
4. **Не** ставить новые сцены на **MineShaft** без экспорта **конкретного** этажа.

---

## 3. Stable vs unstable areas

| Area | Stable? | Use for events? | Notes |
|------|---------|-----------------|-------|
| **Mine entrance platform** | **Yes** (TMX confirmed) | **Yes** — all current CP | Core **`x=15–21`, `y=6–10`**; anchor **`(17,7)`** |
| **North platform / elevator roof** | Partial | Setup only **`y=4–5`** | **`(17,3)`** MineElevator **blocked**; storm farmer **`(15,5)`** OK |
| **Vertical corridor `x=17`** | **Yes** | Harvey `move 0 -2` | **`y=6–10`** verified |
| **South edge / cliff** | Partial | **Avoid** `move` **`y≥11`** | Front/void; **`(18,13–14)`** storm/warp |
| **Shaft descent `(23,9)`** | **Yes (blocked)** | **Never** | Action `Mine` → underground |
| **Mine level portal `(67,9)`** | **Yes (blocked)** | **Never** | `Mine 77377` |
| **Minecart `(11–12,10)`** | **Yes (blocked)** | **Never** | Buildings |
| **East/right mine field** | **No** for events | **Avoid** | Opens to procedural shaft |
| **`MineShaft` random levels** | **No** | **No** | Procedural — **Coordinates require exported map** |
| **Elevator / ladder tiles** | **No** for staging | **No** | Interactive; block movement |
| **Combat rooms (underground)** | **No** | **No** | Monsters, ore, water |
| **Mountain `(103,16)` LoadMap** | Separate map | Pre-mine warning | See [`Mountain.md`](Mountain.md) — not `Mine` location |

---

## 4. Doors, warps, exits

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **18** | **14** | **Warp** | **Custom_AdventurerSummit `(19, 16)`** | **`(18, 13)`** pass; **`(17, 13)`** — **needs check** | SVE default; **не setup** on warp; storm Harvey **`(18,13)`** |
| **67** | **17** | **LoadMap (Back)** | **Mountain `(103, 17)`** | **`(66, 17)`** — **needs check**; east edge | alternate exit; Front on tile |
| **23** | **9** | **Action `Mine`** | → **MineShaft** (procedural) | none | **descent — never setup** |
| **17** | **3** | **MineElevator** | elevator UI | none | Buildings blocked |
| **11–12** | **10** | **MinecartTransport** | cart ride | none | blocked |
| **67** | **9** | **Mine 77377** | specific level | none | blocked |

**MineShaft warps / ladder tiles / elevator destinations:** **`needs map export / exact level needed`** — not documented here.

**External entry to this map:**

| From | How |
|------|-----|
| **Mountain** | LoadMap **`(103, 16)`** → spawn area east side of Mine map — **needs in-game check** exact farmer tile |
| **Custom_AdventurerSummit** | Warp **`(19, 14)`** ↔ Mine **`(18, 14)`** |
| **C# rescue** | `Game1.warpFarmer("Mine", 17, 7, 2)` — **PassOutHandler.BeginMineRescueWarp** |

---

## 5. Safe staging principles

1. **Never** stage rescue on a **random MineShaft level** without an **exported TMX** of that exact floor.
2. **No long routes** — max verified: **`move 0 -2`** (rescue), interception adds **`±1,0`** and **`0 3`** staying **`y≤10`**.
3. **Rescue pattern (canon):**
   - C# **`warpFarmer Mine 17 7`** (major) **or** setup **`(17,7)/(17,10)`** (minor/interception/storm setup)
   - **Short** Mine dialogue / animate on entrance
   - **`globalFade` + `changeLocation Hospital`** (major/minor) **or** Town (storm)
4. **Aftermath in Hospital** — major bed **`(20,5)`** + lying pattern; minor **`(14,6)/(15,6)`** — see [`Hospital.md`](Hospital.md).
5. **«Перехват перед шахтой»** — prefer **Mountain** `(104–105,18)` for «у двери с горы»; **inside Mine** use **`(17,7)/(17,10)`** only.
6. **Harvey must not** path through **`(23,9)`**, carts, **`y≥11`**, or into **MineShaft**.
7. **Do not block** warp **`(18,14)`** or descent **`(23,9)`** with NPCs after event.
8. **Viewport:** set **`viewport 17 7 true`** despite `ViewportFollowPlayer` — backlog for storm/rescue readability.
9. **C#-triggered events:** assume farmer position **reset** by warp/setup — do not rely on pre-warp MineShaft coords.
10. **Any new coords** → TMX pass on **your** runtime export first.

---

## 6. Potential safe zones

Zones below confirmed on **`tmpMap/sve/maps/Locations/Mine.tmx`** — **re-verify after SVE Load / OriginalMinesEntrance**.

### `mine_entrance_safe_area`

| Field | Value |
|-------|-------|
| **Range** | **`x=15–21`, `y=6–10`** (core); extend setup **`y=4–5`** for storm north platform |
| **Farmer anchor** | **`(17, 7)`** face **2** (south) — C# + all rescue scripts |
| **Harvey anchor** | **`(17, 10)`** → optional **`move 0 -2 0`** → **`(17, 8)`** |
| **Alt tiles** | farmer **`(16,7)`/`(18,7)`**; Harvey **`(16,10)`/`(18,10)`** — TMX pass |
| **Viewport** | **`(17, 7)`** |
| **Use** | rescue, interception, C# warp target, optional new entrance dialogue |

### `mine_rescue_short_scene_area`

| Field | Value |
|-------|-------|
| **Range** | Same as **`mine_entrance_safe_area`** — **no separate deep zone** |
| **Farmer** | **`(17, 7)`** — weak/collapsed narrative (animate fear / pass out) |
| **Harvey** | **`(17, 10)`** → **`(17, 8)`** one **`move 0 -2`** |
| **Movement max** | Interception: Harvey **`(16,8)`→`(16,7)`**; farmer exit **`move 0 3`** → **`(17,10)`** — all **`y≤10`** OK per audit |
| **Then** | **`globalFade`** → **`changeLocation Hospital`** — **do not** continue in Mine |
| **Risk** | Without C# warp, farmer may spawn from **`(23,9)`** side if entered from shaft — setup line **overrides** |

### `mine_exit_to_hospital_transition`

| Field | Value |
|-------|-------|
| **Coordinates on Mine** | **No fixed tile** — transition is **`globalFade` / `changeLocation`** |
| **Hospital landing (major)** | farmer **`(20,5)`** + **`ignoreCollisions` + `positionOffset 32 -52` + animate lying** |
| **Hospital landing (minor)** | **`(14,6)` / `(15,6)`** seated |
| **Storm variant** | Mine act ends → **`changeLocation Town`** **`(72,22)`/`(73,22)`** — not Hospital |
| **Notes** | Palata coords = **Hospital passport**; Mine passport ends at fade |

**If TMX export missing:** write **`Coordinates require exported map`** for any proposed tile outside **`(15–21,6–10)`**.

---

## 7. Risk zones

| Zone | Risk | Why |
|------|------|-----|
| **`MineShaft` interiors** | **Critical** | Procedural — walls, ore, water, enemies |
| **`(23, 9)`** descent | **Critical** | Action into procedural levels |
| **`(67, 9)`** level portal | **Critical** | Warp to fixed level ID — not entrance staging |
| **`(17, 3)`** elevator | **High** | MineElevator Buildings |
| **`(11–12, 10)`** minecart | **High** | Buildings + cart transport |
| **`y ≥ 11`** (esp. **`y≥12`**) | **High** | Front cliff, void; farmer storm **`move 0 2`** to **`(15,7)`** OK (y=7) |
| **`(18, 14)`** warp | **High** | Summit transition — storm spawn adjacent **`(18,13)`** |
| **`x ≤ 13`, `x ≥ 22`** (y=6–10) | **Medium** | void / shaft / cart approach |
| **East field `x>50`** | **Medium** | LoadMap Mountain exit, different geometry |
| **Darkness + AmbientLight** | **Visual** | Low readability — use viewport / pause |
| **Monster spawn (underground)** | **N/A on entrance map** | Entrance map has **no** combat spawns in vanilla — **needs in-game check** mod patches |
| **NPC Paths** | **Low** | Sparse on entrance; **`ViewportFollowPlayer`** |

---

## 8. Events using Mine

| Event ID | Trigger source | Current location | Fixed coords? | Risk | Recommendation |
|----------|----------------|------------------|---------------|------|----------------|
| **`eventHarveyMineRescue`** | C# `PassOutHandler` → `NeedsMineRescueEvent` → **`BeginMineRescueWarp`** → `warpFarmer(17,7)` → `startEvent` | Mine → Hospital | **Yes** `(17,7)`, `(17,10)`, Hosp `(20,5)` | **Medium** | **Keep** entrance + C# warp; **keep** Hospital aftermath; verify lying in-game |
| **`eventHarveyMineRescueDating`** | Same C# chain (dating branch) | Mine → Hospital | **Yes** — mirror major | **Medium** | Same as major |
| **`eventHarveyMinorMineRescue`** | C# `TryTriggerMinorMineRescue` (Mine/Volcano low HP); warp **`Mine 17 7`** if not in Mine | Mine → Hospital | **Yes** `(17,7)`, `(17,10)`, Hosp `(14,6)` | **Medium** | **Keep** short Mine + Hospital; Volcano trigger = narrativ mismatch |
| **`eventHarveyMineInterception`** | SpaceCore **`triggerHarveyMineWarning`** (`LocationChanged`, loc=**Mine**) | Mine only | **Yes** `(17,7)`, `(17,10)`; exit **`move 0 3`** | **Low–Medium** | **Keep**; don't extend movement past **`y=10`** |
| **`eventHarveyStormComfortMine`** | CP **location entry** (storm + friendship + random); **no** C# warp | Mine → Town | **Yes** `(15,5)`, Harvey `(18,13)`, moves on **`x=18`**; Town `(72,22)` | **Medium** | **Keep** entrance; optional shorten Harvey spawn to **`(17,10)`** + **`move 0 -2`** (backlog); **not** on warp **`(18,14)`** |

### C# constraints (major/minor rescue)

```csharp
// PassOutHandler.BeginMineRescueWarp — обязателен для major rescue
Game1.warpFarmer(..., "Mine", 17, 7, 2);
// PendingMinorMineRescueEventId + warp Mine 17 7 если игрок не в Mine
```

**Without this warp:** event may run on **MineShaft** or wrong entrance tile → **Broken**.

**Volcano gap:** death in **`VolcanoDungeon`** may **not** set `NeedsMineRescueEvent` — C# issue, not coords ([`mine-events-map-risk-audit.md`](../mine-events-map-risk-audit.md)).

---

## 9. Recommendations for Mine events

### `eventHarveyMineRescue` / `eventHarveyMineRescueDating`

| Strategy | Detail |
|----------|--------|
| **Keep** | C# **`BeginMineRescueWarp(17,7)`** + Mine **`(17,7)/(17,10)`** + **`move 0 -2`** |
| **Keep** | **`globalFade` → Hospital** + bed lying pattern |
| **Reduce movement** | Already minimal — **do not add** shaft exploration |
| **Avoid** | Any coord outside **`y=6–10`** corridor without new TMX export |
| **Before changes** | Runtime TMX export + in-game lying frame at Hospital |

### `eventHarveyMinorMineRescue`

| Strategy | Detail |
|----------|--------|
| **Keep** | Short entrance scene → quick **Hospital `(14,6)`** |
| **Consider (C#)** | Volcano trigger → skip Mine act, fade straight Hospital — narrativ |
| **Avoid** | Extra moves before fade |

### `eventHarveyMineInterception`

| Strategy | Detail |
|----------|--------|
| **Keep** | Setup resets positions; moves stay **`y≤10`** |
| **Do not touch** without request — **checked OK** on TMX |
| **Avoid** | `advancedMove` into east shaft or south cliff |

### `eventHarveyStormComfortMine`

| Strategy | Detail |
|----------|--------|
| **Keep** | Farmer **`(15,5)`** under elevator canopy narrativ |
| **Optional polish** | Harvey **`(17,10)`** + **`move 0 -2`** instead of **`(18,13)`** + long north move (avoid warp adjacency) |
| **Keep** | **`changeLocation Town`** act 2 — bench scene |
| **Avoid** | Extending Mine act; staging on **`(18,14)`** |

### New Mine content (general)

| Rule | Detail |
|------|--------|
| **Require exported map** | Before any new fixed coord |
| **Never** | MineShaft level coords from memory |
| **Prefer** | Mountain warning → fade → Hospital |
| **Max movement** | ≤4 tiles on **`x=17`, `y=6–10`** |

---

## 10. Quick Mine rules

1. **`Mine` ≠ `MineShaft`** — CP coords only on **entrance map**.
2. **Status risky** — TMX in repo ≠ runtime until **export verified**.
3. **Anchor farmer `(17, 7)`** — C# + all rescue/interception scripts.
4. **Anchor Harvey `(17, 10)`** → **`move 0 -2`** → **`(17, 8)`**.
5. **Forbidden: `move` NPC to `y ≥ 11`** (cliff / Front).
6. **Forbidden: setup on `(23,9)`, `(11–12,10)`, `(17,3)`, `(67,9)`**.
7. **Forbidden: long rescue routes** through east shaft or carts.
8. **Major rescue requires C# `BeginMineRescueWarp`** — never CP-only on random tile.
9. **After rescue: `globalFade` → Hospital** — not extended Mine crawl.
10. **Hospital major bed `(20,5)`** — `ignoreCollisions` + offset + lying (Hospital passport).
11. **Minor rescue → Hospital `(14,6)/(15,6)`** — no bed offset.
12. **Interception: stay in entrance** — exit `move 0 3` south along **`x=17`** only.
13. **Storm: farmer `(15,5)`**; Harvey path on **`x=18`** OK TMX; don't block **`(18,14)`** warp.
14. **Viewport `17 7`** recommended despite `ViewportFollowPlayer`.
15. **Warp `(18,14)`** — Summit link; **OriginalMinesEntrance** may change pairing.
16. **Mountain `(103,16)`** — external door; warning scenes prefer Mountain flank (Mountain passport).
17. **Volcano death** — may skip major rescue (C# gap) — document only.
18. **New coords outside core zone** → **`Coordinates require exported map`**.
19. **Manually verified:** none marked do-not-touch for Mine-only; **interception/minor/storm base OK** per audit — still verify in-game.
20. **Do not edit events** in doc tasks — passport for review/planning only.

---

**Метод:** `tmpMap/sve/maps/Locations/Mine.tmx` + [`Mine_event_placement_analysis.md`](../../tmpMap/Mine_event_placement_analysis.md) + [`mine-events-map-risk-audit.md`](../mine-events-map-risk-audit.md) + `PassOutHandler.cs` `BeginMineRescueWarp`.  
**Не учтено:** per-level `MineShaft` layouts, live monster state, unexported runtime diffs.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
