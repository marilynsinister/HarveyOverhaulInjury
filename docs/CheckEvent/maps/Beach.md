# Map Passport: Beach

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`Beach`** |
| **Related names** | **`Town`** — северный вход `(37–40, 0)` ↔ Town `(57–60, 116)`; **`ElliottHouse`** — door `(49,10)`; **`FishShop`** — `(30,33)`; **`Custom_BlueMoonVineyard`** — west warp `(0,4–7)` |
| **Map asset** | `Maps/Beach` |
| **Map file (audit)** | `tmpMap/sve/maps/Locations/Beach.tmx` |
| **Source** | **SVE Load** + CP **EditMap** |
| **Size** | 104×50 tiles, 16×16 px |
| **Passable (TMX)** | **~4136** tiles; **Passable+Front ~38** — много открытого песка и воды по востоку |
| **Status** | **partial** — pier/E4/Propose coords **OK** on repo TMX; E4 finale **sprite overlap** (backlog); **Propose manually verified do-not-touch** |
| **Properties** | `Outdoors=T`; `Fish`, `Debris`, seasonal objects; `DayTiles`/`NightTiles` on pier lamps `(44,31)`, `(90,35)` |

**Used by events:**

| Event ID | File | Beach act? |
|----------|------|------------|
| `HarveyOverhaulStory.E4_PierBreath` | events.json | yes — pier breath ritual |
| `eventHarveyPropose` | events.json | yes — proposal / water scene |

**Not on Beach:** E4B (`Mountain`), FirstDate (`Forest`), storm comfort — other maps.

**Связанные документы:** [`story-arc-map-audit.md`](../story-arc-map-audit.md), [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`events-map-fix-backlog.md`](../events-map-fix-backlog.md), [`Town.md`](Town.md) (south beach exit).

---

## 2. Important areas

| Area | Approx range / anchor | Role | Staging notes |
|------|----------------------|------|---------------|
| **Beach entrance (from Town)** | **`x=37–40`, `y=0–3`** — north edge | Игрок приходит с **`Town (58,115)`** | Passable sand path; **не** block north warps |
| **Pier (main CP anchor)** | **`x=39–40`, `y=7–23`** — vertical boardwalk | E4, Propose, breath-at-water | Narrow plank **`x=39–40`**; water **east** `x≥43`; blocked **west** `x≤35` at `y=23` |
| **Shoreline / water edge** | Pier tip **`(39,13)`**; south pier end **`(39,23)`** | «Смотреть на море», дыхание, proposal water | Column **`x=39`** passable **`y=13–23`**; **`y=25+`** south = **blocked** (pier base/cliff) |
| **Elliott cabin area** | **`x=42–51`, `y=8–12`** | NPC houses, decor | **`(49,10)`** Elliott door **blocked**; avoid staging |
| **Tide pools / bridge (east)** | **`BrokenBeachBridge (58,13)`**; east **`x=55–65`, `y=10–16`** | SVE bridge / tide area | **`(58,13)` blocked** — не block path; narrow planks |
| **Quiet sand (west)** | **`x=20–35`, `y=15–20`** | Open sand, private talk | Wide passable; **no water in frame** unless viewport east |
| **Fish Shop / Willy** | **`(30,33)`** counter | Shop warp | **Buildings blocked** |
| **Romantic / proposal route** | Pier descent **`(40,7)` → `(40,23)`** zigzag (Propose script) | Long choreographed walk to water | **All passable** on TMX — **manually verified** |
| **East open sand** | **`x=62–92`, `y=2–40`** | Large beach field | Good for **static** dialogue; far from pier events |

---

## 3. Doors, warps, exits

Подтверждено на **`tmpMap/sve/maps/Locations/Beach.tmx`**.

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **37–40** | **-1** | **Warp** (map property) | **Town `(58, 115)`** | **`(37,0)`–`(40,2)`** pass | **North entrance** from Town — **не setup on `y=0` warps** |
| **0** | **4–7** | **Action Warp** | **Custom_BlueMoonVineyard `(55, 62)`** | **`(1,5)`** — **needs check** | West edge — SVE vineyard |
| **49** | **10** | **LockedDoorWarp** | **ElliottHouse `(3, 9)`** | **`(48,11)`**, **`(50,11)`** — **needs check** | Elliott cabin — **blocked** |
| **30** | **33** | **LockedDoorWarp** | **FishShop `(5, 9)`** | none on tile | Willy shop — **blocked** |

**External entry:**

| From | Spawn / warp | Notes |
|------|--------------|-------|
| **Town south** | **`(38, 0)`** typical (Town `(57–60, 116)`) | см. [`Town.md`](Town.md) |
| **Blue Moon Vineyard** | West edge `(0,4–7)` | Optional mod route |

**Water / ocean:** not a warp — **non-walkable** tiles east of pier and south **`y≥25`** at pier base; **`move` into `#`** = stuck or edge.

---

## 4. Obstacles and visual blockers

| Type | Where | Blocks movement? | Event impact |
|------|-------|------------------|--------------|
| **Вода (ocean)** | East **`x≥43`** at pier level; slosh tiles **`x=74–89`, `y=11–12`** | **Yes** (non-passable Back) | **Never setup** — farmer/Harvey **on sand/pier only** |
| **Пирс (planks)** | **`x=39–40`, `y=7–23`** | Passable | Main CP spine; **`y=25–26`** pier base **blocked** |
| **Мост / tide pool** | **`BrokenBeachBridge (58,13)`** | **Yes** | Quest object — don't end event blocking repair path |
| **Камни / debris** | Scattered east/south; `Debris` map property | Mixed | Visual clutter on open sand |
| **Декор / Messages** | Bathhouse messages **`(29–34, 2–4)`**; Elliott **`(42–44, 9–10)`** | Many **Buildings blocked** | Don't setup on sign tiles |
| **Здания** | Elliott **`(49,10)`**, Fish Shop **`(30,33)`**, bathhouse north | **Yes** | Use **approach tiles** only |
| **Front / pier rails** | **`y=23–24`** edges; **`y=26`** Front south | Partial | **`faceDirection 2`** = toward water on pier |
| **Края карты** | North **`y=0`**, south **`y=49`**, east **`x=103`** | Void / clamp | E4/Propose **`move 0 -10`** stops at **`y=13`** — **not** map top |

---

## 5. Safe staging zones

Zones подтверждены на **repo TMX** — re-verify после SVE Load / `debug export current`.

### `beach_entrance_dialogue`

| Field | Value |
|-------|-------|
| **Range** | **`x=35–42`, `y=1–5`** — sand path south of Town warp |
| **Farmer anchor** | **`(39, 2)`** or **`(38, 3)`** |
| **Harvey anchor** | **`(41, 2)`** — short greeting, **4+ tiles** apart |
| **Directions** | Farmer **2** (south onto beach); Harvey **3** or **2** |
| **Camera** | **`viewport 39 3`** — path + ocean glimpse east |
| **Movement** | **None** or **`move ±1,0`** — short meet after Town exit |
| **Visual meaning** | «Только сошли с тропы с города» — casual encounter |
| **Risks** | **Don't block `(37–40, 0)`** Town warps |

### `beach_pier_breath_scene`

| Field | Value |
|-------|-------|
| **Range** | Pier **`x=39–40`, `y=13–23`** — E4 canon |
| **Farmer anchor** | Setup **`(40, 17)`** → walk → **`(39, 23)`** → breath **`(39, 13)`** |
| **Harvey anchor** | Setup **`(39, 23)`** → breath **`(39, 13)`** — **overlap issue** (both same tile) |
| **Directions** | At water: **`faceDirection 2`** (south toward ocean) for both; E4 uses **farmer 3 / Harvey 1** when meeting at `(39,23)` |
| **Camera** | Setup line **`39 23`** (E4); **`ambientLight 110 110 140`** evening |
| **Movement** | Farmer: **`0 6`**, **`-1 0`**, then **`0 -10`**; Harvey: **`0 -10`** — column **`x=39`** **all passable** `y=13–23` |
| **Visual meaning** | End of pier, open water south/east — **ideal** breath/trust scene |
| **Risks** | **Both on `(39,13)`** — sprite overlap ([`story-arc-map-audit.md`](../story-arc-map-audit.md) P1); backlog: Harvey **`(40,13)`**; **`move 0 -10`** — don't extend past **`y=13`** without checking |

### `beach_shore_private_talk`

| Field | Value |
|-------|-------|
| **Range** | **`x=22–34`, `y=16–20`** — west sand, away from pier traffic |
| **Farmer anchor** | **`(28, 18)`** |
| **Harvey anchor** | **`(30, 18)`** or **`(26, 18)`** — **2–4 tiles** apart |
| **Directions** | Both **2** (toward ocean) if south of actors; or face each other **1/3** |
| **Camera** | **`viewport 28 18`** — sand + distant water, no pier clutter |
| **Movement** | **Static** preferred — romantic/trust dialogue |
| **Visual meaning** | Quiet evening on sand — **not** on planks; lower visual intensity than pier |
| **Risks** | No water tiles; avoid **`y≥21`** west rocks **`x=21`** blocked |

### `beach_proposal_safe_area`

| Field | Value |
|-------|-------|
| **Range** | Full pier route **`(40,7)` → `(40,23)`** + water **`(39–41, 23)`** — **Propose canon** |
| **Farmer anchor** | Setup **`(40, 7)`** face **2**; Harvey **`(39, 23)`** face **2** |
| **Prop** | **`temporaryAnimatedSprite`** at **`(41, 16)`** — blanket/setup on pier |
| **Directions** | **`faceDirection 2`** at water; **`viewport move 0 3`** during walk |
| **Camera** | Setup **`40 16`**; pan with **`viewport move 0 3 3500`** |
| **Movement** | Farmer: **`0 8`**, **`-1 0`**, **`0 4`**, **`1 0`**, **`0 4`** → **`(40,23)`** area — **all passable** TMX |
| **Visual meaning** | Evening proposal at water — sand, gentle surf, « safest spot » narrativ |
| **Risks** | Long pier path — **don't shorten** without re-verify; **`manually-verified-do-not-touch`** per audit plan |
| **Status** | **✅ manually-verified-do-not-touch** — do not change coords in doc-only tasks |

---

## 6. Beach-specific rules

1. **Не ставить персонажей на воду** — only pier planks and passable sand (`#` tiles = ocean/cliff).
2. **Движение по пирсу проверять тщательно** — spine **`x=39–40`**; west **`x≤35`** blocked at **`y=23`** approach.
3. **Не использовать края пирса для длинного movement** — south base **`y=25–26`** blocked; max north **`y=13`** for water-facing breath (E4).
4. **Романтические сцены — статичнее** — prefer **`faceDirection` + dialogue** over long pier walks (except Propose canon).
5. **Смотрят на море → `faceDirection 2`** (south) when standing on pier facing ocean.
6. **Camera must show water/pier** when narrativ requires — E4 **`39 23`**, Propose **`40 16`** + viewport pan.
7. **Не block Town entrance `(37–40, 0)`** or Elliott/FishShop doors.
8. **BrokenBeachBridge `(58,13)`** — don't place NPCs on bridge tile.
9. **E4 finale overlap** — documented backlog only; passport notes **`(39,13)`×2** visual issue.
10. **Propose event** — **do-not-touch** coords unless explicit user task.
11. **Evening lighting** — E4 `ambientLight`; Propose `night_market` music — atmosphere matches.
12. **quickQuestion on E4** — fork moves **after** choice; no movement during QQ (good pattern).
13. **New coords outside pier column** → TMX pass + **`debug export current`**.

---

## 7. Risk zones

| Zone | Risk | Why |
|------|------|-----|
| **Water tiles (east/south ocean)** | **Critical** | Non-passable — NPC stuck / wrong narrativ |
| **Pier south base `(39,25–26)`** | **Critical** | Blocked — end of walkable pier |
| **Pier west choke `(35–36,23)`** | **High** | `#` blocks — approach only from north |
| **Town warps north `(37–40, 0)`** | **Critical** | Transition to Town |
| **E4/Propose overlap `(39,13)`** | **Visual** | Both actors same tile — sprite stack |
| **BrokenBeachBridge `(58,13)`** | **High** | Blocked quest bridge |
| **Elliott door `(49,10)`** | **High** | LockedDoorWarp |
| **Fish Shop `(30,33)`** | **High** | Buildings |
| **Blue Moon warp `(0,4–7)`** | **Medium** | West edge transition |
| **East tide / slosh `(74–89,11–12)`** | **Medium** | Water adjacency |
| **Map edges `y=0`, `y=49`, `x=103`** | **Medium** | Viewport / walk-off |
| **Bathhouse message tiles north** | **Medium** | Buildings blocked |
| **Narrow passages (audit)** | **Medium** | `(68,0)`, `(78,0)`, `(49,3)`, etc. |

---

## 8. Events using Beach

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| **`HarveyOverhaulStory.E4_PierBreath`** | events.json | **checked-ok** / **Warning (finale overlap)** | Evening sunny, 5♥, after E3B. Farmer **`(40,17)`**, Harvey **`(39,23)`** — **OK**. Moves to **`(39,13)`** both — **overlap**. `ocean` sound, `ambientLight`, quickQuestion trust forks. Backlog: Harvey **`(40,13)`** — [`events-map-fix-backlog.md`](../events-map-fix-backlog.md). |
| **`eventHarveyPropose`** | events.json | **manually-verified-do-not-touch** | Dating, 10♥, sunny evening, not winter. Farmer **`(40,7)`**, Harvey **`(39,23)`**; blanket sprite **`(41,16)`**; long pier walk to water — **OK** TMX. Proposal at shore — **do not change coords** without explicit task. See [`events-map-audit-plan.md`](../events-map-audit-plan.md). |

**Fork sub-events (E4):** trust branches via `quickQuestion` — movement in fork «сжать руку» only.

**No other CP events** on `Data/Events/Beach` per inventory.

---

## 9. Quick Beach rules

1. **Status partial** — pier coords stable; E4 overlap documented; Propose **do-not-touch**.
2. **Main pier spine:** **`x=39–40`, `y=7–23`** — E4 + Propose.
3. **Water breath point:** **`(39,13)`** — face **2** toward ocean.
4. **E4 setup:** farmer **`(40,17)`**, Harvey **`(39,23)`** — **~6 tiles** start distance.
5. **Propose setup:** farmer **`(40,7)`**, Harvey **`(39,23)`** — **manually verified**.
6. **Forbidden: setup on ocean `#` tiles** east of pier.
7. **Forbidden: block Town warps `(37–40, 0)`**.
8. **Forbidden: long movement south of `(39,24)`** — pier ends **`y=25+`**.
9. **Romantic static scenes** → `beach_shore_private_talk` west sand **`(28,18)`**.
10. **Entrance meet** → `beach_entrance_dialogue` **`(39,2)`**.
11. **Camera:** show water for pier scenes — **`viewport 39 23`** or Propose pan.
12. **faceDirection 2** when gazing at sea from pier.
13. **BrokenBeachBridge `(58,13)`** — avoid.
14. **E4 overlap at `(39,13)`** — known; fix only via backlog, not passport edits.
15. **Propose** — **do-not-touch** (audit plan confirmed).
16. **Town entry** — south **`Town (57–60,116)`** ↔ **`Beach (38,0)`**.
17. **Elliott/FishShop doors** — blocked; staging on approach sand only.
18. **Movement on pier:** verify column before **`move 0 ±N`**.
19. **New coords** → runtime export — **`Coordinates require exported map`**.
20. **Do not edit events** in documentation tasks — passport for review/planning only.

---

**Метод:** `tmpMap/sve/maps/Locations/Beach.tmx` + [`story-arc-map-audit.md`](../story-arc-map-audit.md) + [`events-coordinate-audit.md`](../events-coordinate-audit.md) + cross-ref [`Town.md`](Town.md).  
**Не учтено:** runtime `.tbin` diffs, exact water animation tiles at dusk, tide state.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
