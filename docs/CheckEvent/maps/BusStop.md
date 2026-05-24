# Map Passport: BusStop

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`BusStop`** |
| **Related names** | **`Farm`** — западный выход `(9,22–25)` → `(79,17)`; **`Town`** — восточный `(44,22–25)` → `(0,54)`; **`Desert`** — автобус Pam `NPCWarp (22,8)` → `(18,27)`; **`Backwoods`** — север `(11,6–9)` |
| **Map asset** | `Maps/BusStop` |
| **Map file (audit)** | `tmpMap/sve/maps/Locations/BusStop.tmx` |
| **Source** | **SVE Load** + CP **EditMap**; IF2R / Grandpa's Farm могут патчить warps |
| **Size** | 65×30 tiles, 16×16 px |
| **Passable (TMX)** | **~744** tiles — компактная outdoor-карта |
| **Status** | **partial** — road/E1 coords **OK** on TMX; **FirstMeeting** Front warning; **Checkup** target/coords **Critical mismatch**; IF2R warp patches **needs in-game verify** |
| **Properties** | `Outdoors=T`; `NPCWarp=22 8 Desert 18 27`; **`ViewportClamp=10 0 35 30`** — камера ограничена центром карты |

**Used by events:**

| Event ID | File | BusStop act? |
|----------|------|--------------|
| `eventHarveyFirstMeeting` | events.json (+ duplicate eventsCare.json) | yes |
| `HarveyOverhaulStory.E1_SlipperyPath` | events.json | yes |
| `eventHarveyCheckup` | eventsCare.json | **target BusStop** — coords **Hospital** (**Broken**) |

**Onboarding chain (cross-map):** FirstMeeting (BusStop) → optional Checkup → `eventHarveyFirstVisit` (Farm) — см. [`Farm.md`](Farm.md).

**Связанные документы:** [`story-arc-map-audit.md`](../story-arc-map-audit.md), [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`events-map-fix-backlog.md`](../events-map-fix-backlog.md), [`Desert.md`](Desert.md), [`Town.md`](Town.md), [`Farm.md`](Farm.md).

---

## 2. Important areas

| Area | Approx range / anchor | Role | Staging notes |
|------|----------------------|------|---------------|
| **Bus arrival / shelter (north)** | **`x=17–25`, `y=8–14`**; `NPCWarp` **`(22,8)`** → Desert | Pam bus stop, укрытие/навес севернее дороги | **`(22,8)` blocked** — departure tile, не setup; проход **`(22,9–10)`** OK |
| **Road / path (main)** | **`y=22–24`**, esp. **`x=15–40`** | Горизонтальная дорога — **ядро** всех BusStop-сцен | Open dirt path; Front полоса **`x=18–19`** на `y=23–24` |
| **Path to Farm (west)** | **`x=9`, `y=22–25`** warps → **Farm `(79,17)`** | Игрок уходит на ферму после встречи | Warp tiles **Front** — staging **`x=12–16`**, не на `x=9` |
| **Path to Town (east)** | **`x=44`, `y=22–25`** warps → **Town `(0,54)`** | Путь в город | Warp **Front** — staging **`x=38–42`** перед переходом |
| **Bus object / sign** | Buildings **`(21,21)`** Message `BusStop.1`; blocked **`(17,11)`** | Декор остановки | **Не** ставить NPC на message tile |
| **Safe waiting area** | **`(20,23)–(27,23)`** road center; alt **`(23,22–23)`** meet point | First meeting, E1 convergence | **~6–8 tiles** distance — narrativ «встреча на дороге» |
| **Edge transitions** | West Farm **`x=9`**; East Town **`x=44`**; North Backwoods **`x=11, y=6–9`**; Desert bus **`(22,8)`** | Map boundaries | **Never** block warp column after event |

---

## 3. Warps / exits

Подтверждено на **`tmpMap/sve/maps/Locations/BusStop.tmx`**. IF2R / Grandpa's Farm — **needs in-game verify**.

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **9** | **22–25** | **Warp** | **Farm `(79, 17)`** | **`(14,23)`**, **`(15,23)`** pass | **West / farm** — **Front on warp** — не setup |
| **44** | **22–25** | **Warp** | **Town `(0, 54)`** | **`(38,23)`**, **`(40,23)`** pass | **East / town** — **Front on warp** |
| **11** | **6–9** | **Warp** | **Backwoods `(49, 30)`** | **`(12,10)`** — **needs check** | North trail — не для onboarding |
| **22** | **8** | **NPCWarp** (map property) | **Desert `(18, 27)`** | **`(22,9)`**, **`(22,10)`** pass | Bus to desert — tile **blocked** |
| **65** / **-1** | various | **Warp** (edge) | Town / Farm | off-map entries | Engine edge warps — ignore for staging |

**Bus-related (non-warp):**

| X | Y | Type | Notes |
|---:|---:|------|-------|
| **21** | **21** | **Message `BusStop.1`** | Buildings blocked — sign/decoration |
| **17** | **11** | **Action `None`** | Buildings blocked |

**Cross-map references:**

| From | To BusStop | Typical tile |
|------|------------|--------------|
| **Town** | West edge | **`(0,54)`** area → BusStop east — см. [`Town.md`](Town.md) |
| **Farm** | East path | **`(79,17)`** → BusStop west warps |
| **Desert** | Pam return | **`(18,27)`** → **`NPCWarp (22,8)`** — см. [`Desert.md`](Desert.md) |

---

## 4. Obstacles and blockers

| Type | Where | Blocks movement? | Event impact |
|------|-------|------------------|--------------|
| **Авобус / shelter (north)** | Buildings **`y=8–11`**, `x≈17–25`; warp **`(22,8)`** | **Yes** on shelter tiles | Harvey **не** path через северный навес; bus departure **не** block |
| **Дорожные объекты / Front** | Road edges **`(18–19,23–24)`** Front overlay | Passable + Front | Farmer **`(19,23)`** — **Front Warning**; prefer **`(20,23)`** |
| **Заборы / Buildings** | Perimeter, bus sign **`(21,21)`**, **`(17,11)`** | **Yes** | Avoid setup |
| **Деревья / AlwaysFront** | North/backwoods approach, east **`x=48+`** | Partial | E1 **viewport `(52,24)`** — east edge **Front-heavy** |
| **Края карты** | `x=0`, `x=64`, `y=0`, `y=29`; **`ViewportClamp 10,0,35,30`** | Clamp / void | Long **`move`** к краю — camera clip; E1 exit moves west OK on `y=22` |
| **Transition boundaries** | **`x=9`** Farm, **`x=44`** Town columns | Warp + Front | **Critical** — не end event on warp |
| **Hospital coords on BusStop** | **`(2,5)`, `(1,5)`, `(10,17)`, `(5,9)`** | **All blocked** | **`eventHarveyCheckup`** — **Broken** |

---

## 5. Safe staging zones

Zones подтверждены на **repo TMX** — re-verify после SVE / IF2R warp patches.

### `busstop_arrival_meeting`

| Field | Value |
|-------|-------|
| **Range** | **`x=18–28`, `y=22–24`** — центральная дорога |
| **Farmer anchor** | **`(20, 23)`** recommended (**no Front**); current FirstMeeting **`(19, 23)`** — **Front Warning** |
| **Harvey anchor** | **`(27, 23)`** — FirstMeeting setup; **~7 tiles** west of farmer — **good early distance** |
| **Meet point (after move)** | Farmer **`(22, 23)`**; Harvey **`(24, 23)`** — **2 tiles apart**, same road |
| **Directions** | Farmer **1** (east); Harvey **3** (west) — face each other on approach |
| **Camera** | Default or **`viewport 23 23`** — within **ViewportClamp** |
| **Movement** | FirstMeeting: **`move farmer 3 0`**, **`move Harvey -3 0`** — path **OK** on `y=23` |
| **Use** | **`eventHarveyFirstMeeting`** — первое знакомство; farmer «только приехала»; Harvey замечает состояние |
| **Risks** | **`(19,23)` Front**; интимные emote на незнакомце — tone audit ([`10-relationship-narrative-audit.md`](../../events-inventory/10-relationship-narrative-audit.md)); duplicate event in two JSON files |

### `busstop_path_dialogue`

| Field | Value |
|-------|-------|
| **Range** | **`x=18–26`, `y=22–24`** |
| **Farmer anchor** | **`(20, 23)`** — E1 canon |
| **Harvey anchor** | **`(26, 22)`** — E1 canon; **~6 tiles** offset (diagonal distance) |
| **Convergence** | Both → **`(23, 23)`** / **`(23, 22)`** — «полшага» narrativ |
| **Directions** | Farmer **1**; Harvey **3**; after meet **`faceDirection`** |
| **Camera** | E1: setup line **`52 24`** (viewport anchor east — clamp may pull); default OK on compact map |
| **Movement** | E1: **`±3,0`** approach; fork help **`±1,0`**; exit **`±3,0`** along road — all **OK** TMX |
| **Use** | **`HarveyOverhaulStory.E1_SlipperyPath`**; короткий диалог на мокрой дороге |
| **Risks** | **`proceedPosition`** required — don't skip; wind/slippery narrativ needs road tiles not shelter |

### `busstop_town_transition`

| Field | Value |
|-------|-------|
| **Range** | **`x=36–42`, `y=22–24`** — approach to Town warps **`x=44`** |
| **Farmer anchor** | **`(40, 23)`** facing **1** (east) |
| **Harvey anchor** | **`(38, 23)`** or **`(36, 23)`** — behind/side, not blocking |
| **Directions** | Farmer **1** (toward Town); Harvey **1** or **2** |
| **Camera** | **`viewport 40 23`** |
| **Movement** | Max **`move 2 0`** toward east — **stop before `x=44`** warp column |
| **Use** | Сцена «идём в город» / «направление к клинике» **before** warp (future or fade) |
| **Risks** | **`(44,22–25)` Critical** — Town warp + Front; blocking = player can't enter Town |

### `busstop_farm_transition`

| Field | Value |
|-------|-------|
| **Range** | **`x=12–18`, `y=22–24`** — approach to Farm warps **`x=9`** |
| **Farmer anchor** | **`(16, 23)`** or **`(14, 23)`** facing **3** (west) |
| **Harvey anchor** | **`(18, 23)`** — sees farmer off toward farm (**Front** at `(18,23)` — use **`(17,23)`** if overlap) |
| **Directions** | Farmer **3** (west toward farm path); Harvey **3** or **2** |
| **Camera** | **`viewport 15 23`** |
| **Movement** | **`move farmer -2 0`** max — **stop at `x≥12`**, not on `x=9` warps |
| **Use** | After FirstMeeting — farmer heads to farm; Harvey doesn't follow (or short wave) |
| **Risks** | **`(9,22–25)` Critical** — Farm warp; west edge **`x=0`** map boundary |

---

## 6. BusStop-specific rules

1. **Не ставить персонажей на transition tile** — Farm **`(9,22–25)`**, Town **`(44,22–25)`**, Backwoods **`(11,6–9)`**, Desert **`(22,8)`**.
2. **Не блокировать путь Farm/Town** после event — NPC на warp column = softlock.
3. **Harvey подходит коротко** — max **`±3,0`** on road (canon FirstMeeting/E1); **не** path через north bus shelter Buildings.
4. **Первая встреча — дистанция** — старт **~6–8 tiles** (`(20,23)`↔`(27,23)`); converge to **2 tiles**, not same tile.
5. **Ранняя забота** — пиджак/еда/мягкий диалог; **избегать** intimate emotes для незнакомца (tone audit); fork **`declineFood`**, **`refuseCheckup`** — dialogue only.
6. **Movement минимальный** — дорога читается лучше статики + один **`move`**; E1 exit along **`y=22`** OK.
7. **Farmer prefer `(20,23)` over `(19,23)`** — avoids Front overlay (backlog for FirstMeeting).
8. **ViewportClamp `10,0,35,30`** — camera **не** следует на far east `(52,24)` полностью; E1 viewport line = anchor, verify in-game.
9. **Target must match coords** — **`eventHarveyCheckup`**: BusStop target + Hospital coords = **Critical** ([`events-map-fix-backlog.md`](../events-map-fix-backlog.md)).
10. **Duplicate `eventHarveyFirstMeeting`** in events.json + eventsCare.json — same script; verify which entry wins at runtime.
11. **Visual «встреча на дороге»** — actors on **`y=22–24`**, not north shelter; bus in background optional.
12. **Новые coords** → TMX pass + IF2R export if using Grandpa's Farm.

---

## 7. Risk zones

| Zone | Risk | Why |
|------|------|-----|
| **Farm warps `(9, 22–25)`** | **Critical** | Instant Farm transition — Front on tiles |
| **Town warps `(44, 22–25)`** | **Critical** | Instant Town transition — Front on tiles |
| **Desert NPCWarp `(22, 8)`** | **High** | Blocked bus departure — don't setup |
| **Backwoods warps `(11, 6–9)`** | **High** | North exit — narrow Front |
| **Bus sign `(21, 21)`** | **High** | Buildings blocked |
| **Road edge Front `(18–19, 23–24)`** | **Medium** | Passable but visual overlap — farmer `(19,23)` |
| **East map edge `(48–64, 22–25)`** | **Medium** | Front wall — E1 viewport zone |
| **Map edges / ViewportClamp** | **Medium** | Camera clip; long moves off-center |
| **Hospital coords on BusStop** | **Critical** | Checkup `(2,5)/(1,5)/(10,17)` — all Buildings |
| **Narrow passages** (audit) | **Medium** | `(14,0)`, `(17,1)`, `(12,2)`, `(22,9)`, `(14,18)` — ≤1 neighbor |
| **IF2R warp patches** | **Medium** | Runtime may shift Farm/Town warp pairing |

---

## 8. Events using BusStop

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| **`HarveyOverhaulStory.E1_SlipperyPath`** | events.json | **checked-ok** / **manually verified pattern** | Viewport setup **`52 24`**; farmer **`(20,23)`**, Harvey **`(26,22)`** — **OK** TMX. Wind/morning road scene. Moves **`±3,0`**, fork **`±1,0`**, exit west — passable. See [`story-arc-map-audit.md`](../story-arc-map-audit.md) **E1 stable**. Re-verify IF2R warps in-game. |
| **`eventHarveyFirstMeeting`** | events.json (+ **duplicate** eventsCare.json) | **needs-review** / **partial** | Farmer **`(19,23)`** **Front Warning** — backlog **`(20,23)`**; Harvey **`(27,23)` OK**. Moves to **`(22,23)`/`(24,23)`** — OK. Onboarding: `topicFirstMeeting`, forks `declineFood`/`refuseCheckup`. **Not** marked do-not-touch — Front fix pending. |
| **`eventHarveyCheckup`** | eventsCare.json | **Broken** / **Critical target mismatch** | Target **`Data/Events/BusStop`**, coords **Hospital** `(2,5)/(1,5)`, viewport **`(5,9)`**, end **`(10,17)`** — **all blocked** on BusStop TMX. **Cannot play correctly** until moved to Hospital **or** coords rewritten to **`(20,23)/(26,22)`** zone. See [`events-map-fix-backlog.md`](../events-map-fix-backlog.md). |

**Fork sub-events (FirstMeeting, not separate map):** `declineFood`, `refuseCheckup` — dialogue branches, same BusStop coords.

**No other CP events** patch `Data/Events/BusStop` per inventory.

---

## 9. Quick BusStop rules

1. **Status partial** — E1/road **OK**; Checkup **Broken**; FirstMeeting Front warning.
2. **Main road staging:** **`y=22–24`**, **`x=15–40`** — all onboarding scenes.
3. **Canon anchors:** FirstMeeting **`(19–20,23)` / `(27,23)`**; E1 **`(20,23)` / `(26,22)`** — prefer **20** over 19.
4. **E1 viewport line `52 24`** — east anchor; respect **ViewportClamp**.
5. **Forbidden: setup on Farm warps `(9,22–25)`** or **Town `(44,22–25)`**.
6. **Forbidden: Harvey path through north bus shelter `(22,8)`** Buildings.
7. **Forbidden: Hospital coords `(2,5)`, `(1,5)`, `(5,9)`, `(10,17)` on BusStop** — Checkup bug.
8. **Meeting distance:** start **≥6 tiles**, converge to **2** — early relationship staging.
9. **Short movement only** — **`±3,0`** max on road; use **`proceedPosition`** (E1).
10. **Early care tone** — jacket/food/formal «Вы»; no intimate emotes for stranger.
11. **Farm transition** — stage **`(14–16,23)`**, face west; don't block **`x=9`**.
12. **Town transition** — stage **`(38–40,23)`**, face east; don't block **`x=44`**.
13. **Desert bus** — departure **`(22,8)`** blocked; access via **`(22,9–10)`**.
14. **Bus sign `(21,21)`** — blocked decor.
15. **Duplicate FirstMeeting** — two JSON files, same ID — verify runtime precedence.
16. **IF2R / Grandpa's Farm** — re-verify warps before new coords.
17. **Cross-map chain:** BusStop → Farm FirstVisit — [`Farm.md`](Farm.md).
18. **New coords** → runtime export — **`Coordinates require exported map`** if outside §5 zones.
19. **Do not edit events** in documentation tasks — passport for review/planning only.
20. **In-game checklist:** Harvey not in bus shelter; Farm/Town exits clear after scene; Checkup repro on BusStop (expect broken until target fix).

---

**Метод:** `tmpMap/sve/maps/Locations/BusStop.tmx` + [`events-coordinate-audit.md`](../events-coordinate-audit.md) + [`story-arc-map-audit.md`](../story-arc-map-audit.md) + cross-ref Town/Farm/Desert passports.  
**Не учтено:** IF2R/Grandpa's Farm warp `.tbin` patches, exact bus sprite bounds, runtime vs repo TMX diffs.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
