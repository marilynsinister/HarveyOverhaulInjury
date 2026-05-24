# Map Passport: Town

## 1. Metadata

- **LocationName:** Town
- **Map asset:** `Maps/Town` (runtime: `Town.tmx` или `Town_Joja.tmx` при Joja route)
- **Map file:** `tmpMap/sve/maps/Locations/Town.tmx` (130×116, tile 16×16)
- **Source:** SVE Load (Community Center route) + CP EditMap; альтернатива **`Town_Joja.tmx`** при Joja; база — vanilla outdoor + SVE town sheets
- **Status:** **partial** (TMX в repo; runtime/SVE/CC vs Joja может отличаться; NPC traffic не моделируется TMX)
- **Used by events:**
  - `eventHarveyStormComfortTown` — `events.json` (Town → Saloon)
  - `HarveyOverhaulStory.E2B_QuietAgreement` — `events.json`
  - `HarveyOverhaulStory.E7_TownSip_Sunny` — `events.json`
  - `HarveyOverhaulStory.E9_LightInWindow` — `events.json` (фасад клиники)
  - `eventHarveyStormComfortMine` — `events.json` (финал act 2 на Town)
  - `eventHarveyLateNightCollapse` — `events.json` (Town → Hospital)

**Связанные локации (не Town, но в цепочках):** Saloon (storm act 2), Hospital (collapse / E9 narrative / storm fork).

**Метод:** парсинг TMX + audit [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md), [`story-arc-map-audit.md`](../story-arc-map-audit.md).  
**Не учтено:** runtime NPC schedules, `positionOffset`, Front collision в движке, SVE `.tbin` после Load.

---

## 2. Important areas

Town — большая outdoor-карта (130×116). Для HarveyOverhaul критичны **клиника**, **юг**, **центр** и **мостовая зона**.

| Area | Approx range (TMX) | Роль в моде | Harvey CP relevance |
|------|-------------------|-------------|---------------------|
| **Clinic entrance (Hospital facade, north side)** | `(36,55)` door tile; подход `(36,56)–(38,57)` | LockedDoorWarp → Hospital `(10,19)`; Harvey «выбегает из клиники» | storm comfort warp, late-night collapse Harvey spawn, Hospital exit dest `(36,56)` |
| **Clinic south facade / steps (E9)** | `(33,88)–(40,91)` | Вечер у **южного** фасада; свет в окне, крыльцо | `HarveyOverhaulStory.E9_LightInWindow` `(35,88)` |
| **Clinic path (Pierre ↔ Hospital)** | `(34,57)–(42,62)` open | Путь farmer к клинике; late-night collapse staging `(37,59)` | escort, collapse, medical concern |
| **Town square (central south)** | `(24,64)–(34,70)` | Открытая площадь, лавки | `HarveyOverhaulStory.E2B_QuietAgreement` `(28,67)` |
| **North-center plaza (E7 bench zone)** | `(22,20)–(34,26)` | Лавка/фонтанная зона; **NPC traffic** (Penny path) | `HarveyOverhaulStory.E7_TownSip_Sunny` `(26,22)` |
| **Path from BusStop side (west edge)** | `x=0`, `y≈54–55` warp → BusStop | Вход с запада; onboarding chain | косвенно (FirstMeeting на BusStop) |
| **South Town / Saloon approach** | `(39,70)–(48,76)` | Открытый юг; Saloon door `(45,70)` | storm comfort farmer `(39,73)`, chase к Saloon |
| **Saloon area** | door `(45,70)` → interior `(14,24)`; подход `(44,71)–(46,72)` | Укрытие от грозы (interior act 2) | storm comfort narrative «к салoon» |
| **Shearwater Bridge / east edge** | warp `(119,72)–(119,76)` → Custom_ShearwaterBridge | Мост, узкий выход; вода/обрыв восток | рядом со storm `(39,73)` — **visual**, не setup |
| **Path to Beach (south edge)** | `(57,116)–(60,116)` warp → Beach | Выход на пляж | не используется CP; край карты |
| **Path to Mountain (north edge)** | `(79,81)–(83,81)` warp → Mountain | Северный выход | не используется CP |
| **Path to Forest (west-north)** | `x=-1`, `y≈89–91` → Forest | Лесной выход | не используется CP |
| **Mayor manor area** | `(58,85)–(59,85)` doors | ManorHouse; декор, fence | фон для юго-западных сцен |
| **Seed Shop / Pierre block** | `(43,56)–(44,56)` counter; `(37,47)–(47,52)` | Здание Pierre; узкие проходы севернее | late-night `(37,59)` — **перед** Pierre/Hospital path |
| **Graveyard / river edge (northwest)** | vanilla ~`(30,95)–(45,105)` (ориентир) | Кладбище, река — **не в Harvey CP coords** | помечено как **unused**; не ставить новые сцены без export |
| **Late night collapse area** | viewport `(37,59)`; farmer `(37,59)` collapse | Ночной обморок на **дороге к клинике** | `eventHarveyLateNightCollapse` — manually verified |
| **Playground / bench (mine storm finale)** | `(72,18)–(74,22)` vanilla orient.; event `(72,22)` | «Сядь на скамейку» — **скамейка не подтверждена TMX на (72,22)** | `eventHarveyStormComfortMine` finale |

**CC vs Joja:** при Joja route активна **`Town_Joja.tmx`** — другая застройка юга и центра. Любые coords audit проверять **на обеих** картах перед правкой.

---

## 3. Doors, warps, exits

### Критичные двери (LockedDoorWarp / Action)

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **36** | **55** | **Clinic door (Action)** | **Hospital `(10, 19)`** | `(36,56)` passable, Front; `(37,57)`, `(38,57)` | **Главная дверь клиники.** Buildings на `(36,55)`. Не setup farmer на `(36,55)`. Harvey warp `(36,56)` — narrativ OK, Front overlay |
| **45** | **70** | Saloon door | Saloon `(14, 24)` | `(44,71)` blocked; южнее `(45,72)+` passable | Storm comfort **цель** «укрытие»; `changeLocation Saloon` |
| 43 | 56 | Seed Shop | SeedShop `(6, 29)` | `(42,57)–(48,58)` open path | Pierre; рядом clinic path |
| 44 | 56 | Seed Shop | SeedShop `(6, 29)` | same | counter Buildings |
| 45 | 70 | Saloon | Saloon `(14, 24)` | см. выше | hours 1200–2400 |
| 58 | 85 | Manor | ManorHouse `(4, 11)` | `(57,86)–(60,87)` | Lewis manor |
| 59 | 85 | Manor | ManorHouse `(5, 11)` | same | |
| 101 | 89 | Museum | ArchaeologyHouse `(3, 14)` | `(100,90)` area | E8 — другая локация |
| 94 | 81 | Blacksmith | Blacksmith `(5, 19)` | south approach | |
| 95–96 | 50 | JojaMart | JojaMart | plaza north | Joja layout |

**Hospital ↔ Town (обратный warp из Hospital TMX):** выход Hospital `(10,20)` → **Town `(36, 56)`** — совпадает с clinic spawn Harvey.

### Map warps (границы и переходы)

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| -1 | 54–55 | Warp | BusStop `(42, 23–24)` | `(0,54)–(2,55)` edge | западный вход |
| -1 | 89–91 | Warp | Forest `(118, 25)` | north-west edge | |
| 79–83 | -1 | Warp | Mountain `(15, 40)` | north edge | |
| 57–60 | 116 | Warp | Beach `(38, 0)` | south edge | |
| 92 | 45 | Warp | MovieTheater `(13, 4)` | ⚠ tight | CC; narrow `(92,45)` |
| **119** | **72–74** | Warp | **Custom_ShearwaterBridge** | `(118,72)–(118,74)` passable | **мост**; `(119,75–76)` blocked |
| 52–53 | 19 | Action | WarpCommunityCenter | CC interior | CC route |
| 98 | 0 | Back Warp | Mountain `(85, 40)` | north | LoadMap/Warp property |

Координаты `x=-1`, `y=-1`, `y=116` — **вне карты** в TMX; в игре это edge warps, не setup NPC.

---

## 4. Action / TouchAction tiles

Ключевые тайлы для CP (полный список ~240+ — см. TMX). Приоритет — **клиника**, **Saloon**, **warp/CC**.

| X | Y | Layer | Property | Meaning | Event risk |
|---:|---:|-------|----------|---------|------------|
| **36** | **55** | Buildings | `LockedDoorWarp 10 19 Hospital 900 1500` | **Clinic entrance** | **high** — door; не setup; `doAction`/fade → Hospital |
| **36** | **56** | Back/Front | (passable, Front) | Подход к двери | **medium** — Harvey warp/storm/late-night; Front overlay |
| **45** | **70** | Buildings | `LockedDoorWarp 14 24 Saloon 1200 2400` | Saloon door | **high** — storm destination |
| 52 | 19 | Buildings | `WarpCommunityCenter` | CC warp | high |
| 53 | 19 | Buildings | `WarpCommunityCenter` | CC warp | high |
| 119 | 72–76 | map Warp | → ShearwaterBridge | east bridge | **high** — edge; рядом storm south |
| 43–44 | 56 | Buildings | SeedShop doors | Pierre shop | medium — block clinic path north |
| 98 | 0 | Back | `Warp Mountain 85 40` | north warp | high |

---

## 5. Furniture / visual blockers / obstacles

| Category | Approx locations | Blocks movement? | Blocks visibility? | Notes |
|----------|------------------|------------------|--------------------|-------|
| **Clinic building walls** | `(34,53)–(42,57)` north facade | yes (Buildings) | yes | Door only `(36,55)` |
| **Clinic south walls/steps** | `(33,88)–(40,89)` | partial `#` on `(33–34,88–89)` | Front/AlwaysFront | E9 farmer `(35,88)` on open tiles |
| **Pierre/Seed Shop block** | `(43,56)–(47,56)` | yes | yes | Севернее collapse path |
| **Saloon building** | `(45,68)–(48,72)` | yes south face | yes | Door `(45,70)` |
| **Town square trees/benches** | `(26,20)–(28,24)` | `(27,22)` **Buildings** | Front | **E7 Harvey broken** on `(27,22)` |
| **Fences / lamp posts** | scattered, esp. `(25,25)`, manor | partial | AlwaysFront | E7 Penny path `(32,24)→(38,24)` |
| **Bridges** | east `(119,72–76)` | warp tiles | — | Shearwater; water east |
| **Water / river** | northwest graveyard, east coast | Passable=F / void | — | не использовать без export |
| **Mailboxes / garbage** | `Garbage Pierre (46,51)`, Saloon `(47,70)` | yes | decor | не anchor coords |
| **NPC traffic lanes** | Paths layer + schedules | dynamic | — | Pierre, Penny, villagers 10:00–17:00 |
| **Narrow passages** | `(92,45)` theater, `(32,45)`, `(76,51)`, south `(45,46)` | 1-wide | — | avoid dual NPC |
| **AlwaysFront trees** | central north, clinic south edges | no collision | **yes** — sprite cover | E9, storm chase |

---

## 6. Safe staging zones

### `town_clinic_door_area`

**Для:** Harvey встречает farmer у клиники; late-night medical concern; transition to Hospital; короткий диалог перед входом; storm Harvey spawn.

| Field | Value |
|-------|-------|
| **Coordinates/range** | Door `(36,55)`; staging **`(36,56)–(38,58)`** (TMX passable south of door) |
| **Harvey recommended** | **`(36,56)`** face **2** (south) или **1** (east) к farmer — storm, late-night warp |
| **Farmer recommended** | **`(37,57)`–`(38,58)`** face **0/3** к Harvey; **не** `(36,55)` |
| **faceDirection** | Harvey → farmer; farmer → Harvey/clinic |
| **viewport** | **`(37, 56)`–`(38, 57)`**; Hospital exit dest `(36,56)` |
| **Risks** | `(36,55)` Buildings; Front на `(36,56)`; после 17:00 — villagers near Pierre |

### `town_clinic_path`

**Для:** Harvey подходит к farmer; farmer идёт к клинике; late-night collapse `(37,59)`.

| Field | Value |
|-------|-------|
| **Coordinates/range** | **`(34,57)–(42,62)`** open (TMX); collapse **`(37,59)`** |
| **Harvey recommended** | spawn `(36,56)` → **`move 0 3 1`** (late-night) max **3 tiles** к farmer |
| **Farmer recommended** | **`(37,59)`** collapse + `positionOffset 0 -16`; или **`(38,58)`** standing |
| **faceDirection** | farmer **2** lying/weak; Harvey **1** approaching |
| **viewport** | **`(37, 59)`** (late-night verified) |
| **Risks** | длинный path к `(39,73)`; Pierre counter north; night OK (fewer NPC) |

### `town_square_open_area`

**Для:** нейтральная встреча; небольшое движение; E2B.

| Field | Value |
|-------|-------|
| **Coordinates/range** | **`(24,64)–(34,70)`**; event ref **`(28,67)`** |
| **Harvey recommended** | **`(32,67)`** (audit OK) face **3** west к farmer |
| **Farmer recommended** | **`(28,67)`** face **1** |
| **faceDirection** | face each other across 4 tiles — OK |
| **viewport** | **`(28, 67)`** |
| **Risks** | low TMX; **NPC traffic** day; Wind/Sunny variants same coords |

### `town_late_night_safe_spot`

**Для:** поздний коллапс; тревожный диалог; сцены после 24:00; medical escort.

| Field | Value |
|-------|-------|
| **Coordinates/range** | Primary **`(37,59)`**; clinic path **`(35,57)–(40,62)`** |
| **Harvey recommended** | **`warp (36,56)`** → short **`move 0 3 1`** — **manually verified** |
| **Farmer recommended** | **`(37,59)`** + offset; animate collapse |
| **faceDirection** | Harvey **1** approach; farmer **2** |
| **viewport** | **`(37, 59)`** |
| **Risks** | low traffic 24:00–26:00; then **`changeLocation Hospital`** — re-setup |

### Дополнительные audit-зоны (кратко)

| Zone ID | Coords | Use | Risk |
|---------|--------|-----|------|
| `town_storm_south` | farmer `(39,73)`; chase → `(45,71)` | storm act 1 | open sky; bridge warp east; **long advancedMove** |
| `town_saloon_porch` | `(44,71)–(46,72)` | storm narrative shelter | Buildings adjacent |
| `town_e9_clinic_window` | farmer `(35,88)`; Harvey fork `(36,89)`/`(34,88)` | E9 evening | **overlap risk** same tile |
| `town_e7_bench` | `(26,22)` farmer; Penny `(32,24)` | E7 | Harvey **`(27,22)` Buildings — broken** |
| `town_playground_finale` | `(72,22)`/`(73,22)` | mine storm finale | bench **not confirmed** on tile |

---

## 7. Risk zones

| Coords/range | Risk | Why dangerous | Avoid in event commands |
|--------------|------|---------------|---------------------------|
| `(36,55)` | door/warp | Clinic LockedDoorWarp | setup farmer/Harvey |
| `(45,70)` | door | Saloon door Buildings | setup NPC |
| `(119,72)–(119,76)` | warp / bridge | → ShearwaterBridge; `(119,75–76)` blocked | setup, long move ending on warp |
| `(52,19)–(53,19)` | warp | Community Center | setup |
| `(92,45)` | warp + narrow | Movie Theater | advancedMove |
| `(57,116)–(60,116)` | map edge | → Beach | walk-out |
| `x=-1`, north `y=-1` | map edge | BusStop/Forest/Mountain | setup |
| `(27,22)` | furniture | **Buildings** — E7 Harvey | setup Harvey |
| `(24,22)` | furniture | Buildings (Penny start area fence) | Penny path only |
| `(33,88)–(34,89)` | wall/steps | Buildings south clinic | farmer path — use `(35,88)` |
| `(39,73)` area + east | NPC + bridge traffic | open + warp column | long chase without viewport |
| `(72,22)` | visual mismatch | text «скамейка» без bench object | verify in-game before new dialogue |
| Central north `(22,20)–(34,26)` | NPC traffic | Penny, villagers, festival | E7 temp actor path |
| Passable + AlwaysFront | visual | trees, lamps, signs | sprite overlap |
| **Town_Joja variant** | layout | different south/shop blocks | same numeric coords may fail |

---

## 8. Movement guidance

### Good paths

| Actor | From | To | Commands | Notes |
|-------|------|-----|----------|-------|
| Harvey | `(36,56)` | `(37,59)` | `move 0 3 1` | late-night — **3 tiles max**, verified |
| Harvey | `(36,56)` | `(39,73)` | ⚠ `advancedMove` short segments + **viewport** | storm — backlog: сократить chase |
| Harvey | `(32,67)` | `(28,67)` | `move -4 0 3` | E2B-style short approach |
| Farmer | `(35,88)` | `(35,87)` | `move 0 -1 0` | E9 fork — 1 tile |
| Harvey | `(36,56)` | clinic door | static + `faceDirection` | escort narrative без длинного path |
| Farmer | `(39,73)` | `(45,71)` | `advancedMove 6 0 0 -2` | storm — к Saloon area; then **fade → Saloon** |
| Both | `(72,22)` | `(73,22)` | static / 1 step | mine finale — keep static |

### Avoid paths

| From | To | Why avoid |
|------|-----|-----------|
| `(36,56)` | `(39,73)` | **17-tile advancedMove** без viewport — камера теряет NPC ([`events-map-fix-backlog.md`](../events-map-fix-backlog.md)) |
| Any | `(119,72+)` | bridge warp column |
| `(26,22)` | `(38,24)` | через **Buildings** `(27,22)` и NPC traffic |
| South | North cross-town | полная карта 130×116 — только с viewport chain |
| `(39,73)` | `(45,70)` | через Buildings Saloon face — использовать door warp/fade |
| Clinic | Museum/Blacksmith | unrelated long routes |
| Any setup | `(36,55)`, `(45,70)` | door tiles |

**Правило:** Town movement для care-сцен — **≤3 тайла** или **`globalFade` + warp** к целевой зоне (Saloon, Hospital).

---

## 9. Recommended scene types

| Scene type | Recommended zone | viewport | Notes |
|------------|------------------|----------|-------|
| **Медицинская тревога у клиники** | `town_clinic_door_area` + `town_clinic_path` | `(37,56)`–`(38,57)` | Harvey from `(36,56)`; fade → Hospital for treatment |
| **Поздняя встреча / collapse** | `town_late_night_safe_spot` `(37,59)` | `(37,59)` | verified; animate farmer; strict tone |
| **Мягкий escort to clinic** | `town_clinic_path` | follow farmer | short Harvey move; speak → fade Hospital |
| **Короткий диалог на площади** | `town_square_open_area` `(28,67)` | `(28,67)` | E2B; avoid `(27,22)` |
| **Вечер у окна клиники (E9)** | `town_e9_clinic_window` `(35,88)` | **`(35,88)`** recommended | `ambientLight 80 70 55`; Harvey warp after QQ, not same tile as farmer |
| **Storm comfort (outdoor act 1)** | `town_storm_south` `(39,73)` | `(39,73)` then chase OR fade | prefer **Saloon fade** over long chase |
| **Storm / weakness bench** | `(72,22)` | `(72,22)` | confirm bench in-game; mine finale |
| **Интимные / dating сцены** | **осторожно** | clinic south / square | Town = **public**; NPC traffic day; prefer Hospital/HarveyRoom for intimacy |
| **Не использовать Town для** | длинные romantic walks, picnic, apothecary | — | → Forest, Beach, Mountain |

---

## 10. Events using Town

| Event ID | File | Status | Coordinates checked? | Notes |
|----------|------|--------|----------------------|-------|
| `eventHarveyLateNightCollapse` | `events.json` | **manually-verified-do-not-touch** | yes | `(37,59)` collapse; Harvey `(36,56)` → move; → Hospital |
| `HarveyOverhaulStory.E2B_QuietAgreement` | `events.json` | checked-ok | yes | `(28,67)` / Harvey `(32,67)` |
| `eventHarveyStormComfortTown` | `events.json` | needs-review | partial | farmer `(39,73)`; Harvey `(36,56)`; long advancedMove; act 2 Saloon |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | `events.json` | needs-review | partial | `(26,22)` OK; Harvey **`(27,22)` Buildings broken**; Penny `(32,24)` |
| `HarveyOverhaulStory.E9_LightInWindow` | `events.json` | needs-review | partial | `(35,88)` OK; Harvey hide → fork warp; overlap risk |
| `eventHarveyStormComfortMine` | `events.json` | needs-review | partial | finale **`(72,22)`/`(73,22)`**; bench narrativ unconfirmed |

**Связанный vanilla (не Harvey CP body):** `528013` balloon @ Town — manually verified, вне scope правок.

**Saloon act 2** (same event chain): см. `tmpMap/vanilla/maps/Saloon.tmx`, staging `(14,23)` — не Town, но обязательный re-warp после `changeLocation`.

---

## 11. Quick Town rules

1. **Клиника:** door **`(36,55)`** — never setup; Harvey **`(36,56)`**, farmer south **`(37,57)+`**.
2. **Hospital exit** lands **`(36,56)`** — согласовать с clinic door area.
3. **Movement ≤3 tiles** для care; иначе **`globalFade` + warp** (Saloon, Hospital).
4. **Viewport обязателен** на большой карте — привязка к scene zone, не default follow.
5. **Storm chase `(36,56)→(39,73)`** — risky; prefer fade to Saloon ([`storm-comfort-map-audit.md`](../storm-comfort-map-audit.md)).
6. **Saloon door `(45,70)`** — narrativ shelter; interior `(14,23)` after changeLocation.
7. **E9 `(35,88)`** — south facade; Harvey fork **`(34,88)`** or **`(36,89)`**, not same tile as farmer.
8. **E7:** never Harvey **`(27,22)`** — use **`(32,67)`** or **`(28,67)`** adjacent open tile.
9. **Late night `(37,59)`** — manually verified; do not move without full re-test.
10. **Bridge warps `(119,72–76)`** — не конечная точка movement/storm south.
11. **Не длинные advancedMove** через весь Town без viewport segments.
12. **NPC traffic:** центр `(26,22)`, Pierre `(43,56)` — day hours; temp actors with short east-west path only.
13. **CC vs Town_Joja** — test both before coord changes.
14. **Интимность** — публичный Town; medical intimacy → Hospital.
15. **`(72,22)` bench text** — verify playground props in-game before new lines.
16. **Front/AlwaysFront** — passable but ugly overlap; check camera.
17. **Penny E7 path** `(32,24)→(38,24)` — don't block with Harvey static on path.
18. **New universal Town events** — default to **`town_clinic_door_area`** or **`town_square_open_area`**, not random coords.
19. **Water/river/graveyard** — no Harvey coords without map export.
20. **After `changeLocation`** — full actor re-setup (Saloon, Hospital).

---

**См. также:** [`map-passports.md`](../map-passports.md) · [`cp-event-authoring-rules.md`](../../EventPatterns/cp-event-authoring-rules.md) §12.5 · Saloon TMX `tmpMap/vanilla/maps/Saloon.tmx`
