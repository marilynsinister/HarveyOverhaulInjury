# Map Passport: SkullCave

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`SkullCave`** — единственное имя локации в CP; отдельного **`SkullCaveEntrance`** в игре **нет** |
| **Related names** | **`Desert`** — внешний вход (пещера в пустыне); **`MineShaft`** — процедурные этажи **после** `SkullDoor`; **не путать** с **`Mine`** (другая шахта, другие CP-события) |
| **Map asset** | `Maps/SkullCave` |
| **Map file (audit)** | `tmpMap/vanilla/maps/SkullCave.tmx` |
| **Source** | **Vanilla** `Content/Maps/SkullCave.xnb`; **SVE** только **EditMap** (патч Warp / spawn с Desert) — **Load карты нет** |
| **Size** | 16×10 tiles, 16×16 px |
| **Passable (TMX)** | **~30** tiles — одна из самых тесных карт мода |
| **Status** | **high-risk / procedural unless exact stable map confirmed** — входная комната **фиксирована** на vanilla TMX, но карта **опаснее Mine**: крошечная площадь, `SkullDoor` в процедур, warp-выход, баги триггеров, SVE-патчи spawn |
| **Properties** | `AmbientLight=140 140 60`; `LocationContext=Desert` |

**Used by events:**

| Event ID | File | SkullCave act? |
|----------|------|----------------|
| `eventHarveySkullCavePrevention` | eventsCare.json | yes (stay in SkullCave) |
| `HarveySkullPromise` | eventsCare.json | fork (dialogue only, same `Entries`) |

**Triggers (не отдельные карты, но влияют на staging):**

| Trigger ID | File | Loc condition | Plays |
|------------|------|---------------|-------|
| `triggerLocationReactionSkullCaveExit` | triggersCare.json | `SkullCave` | `eventHarveySkullCavePrevention` |
| `triggerHarveySkullCaveWarning` | triggersCare.json | **`Mine` OR `SkullCave`** (битое OR) | `eventHarveySkullCavePrevention` — **Critical bug** при входе в **Mine** |

**Not on SkullCave map:** все rescue/interception/storm для **`Mine`** — см. [`Mine.md`](Mine.md). C# `PassOutHandler` учитывает `SkullCave` в mine-related pass-out, но **отдельного CP-rescue на SkullCave нет**.

**Связанные документы:** [`mine-events-map-risk-audit.md`](../mine-events-map-risk-audit.md), [`events-coordinate-audit.md`](../events-coordinate-audit.md), [`events-map-fix-backlog.md`](../events-map-fix-backlog.md), [`Desert.md`](Desert.md) (вход `(44,57)`), [`Hospital.md`](Hospital.md) (aftermath), [`Mine.md`](Mine.md) (аналог interception).

---

## 2. Почему SkullCave high-risk

SkullCave **рискованнее обычной `Mine`**, хотя обе связаны с подземельем: у Mine вход **широкий и предсказуемый** (77×20, якорь `(17,7)` + C# pre-warp), у SkullCave — **крошечная комната** и **сразу за дверью процедур**.

### Две разные «пещеры черепа»

| Concept | Stable layout? | CP fixed coords? |
|---------|----------------|------------------|
| **`SkullCave` (входная комната)** | **Partial** — vanilla 16×10, **~30** passable tiles | **Only if** player/event **confirmed on this map**; current prevention `(5,5)/(7,7)` **OK on repo TMX** |
| **`MineShaft` после `SkullDoor`** | **No** — процедурные этажи, лестницы, ямы, руда | **No** — любой `(x,y)` может быть стена/дыра/спавн |
| **Desert у входа в пещеру** | **Yes** (SVE Desert TMX) | **Yes** для prevention **до** warp — предпочтительнее новых сцен |

### Вариативность и опасная логика карты

- **Процедурные уровни:** `SkullDoor (3,3)` мгновенно уводит в **случайно генерируемые** этажи (`MineShaft`) — layout, ямы и узкие проходы **меняются каждый спуск**.
- **Опасные объекты:** лава-тайлы, камни, `Stone` breakables, монстры высокого tier — на глубине **combat + hazard**, не staging.
- **Лестницы / дыры:** hole tiles и ladder down — farmer/NPC могут **упасть на другой этаж** или застрять; event movement **не контролирует** post-event state игрока на процедурном этаже.
- **Случайная генерация:** даже «стабильный» вход **не гарантирует**, где окажется игрок **до/после** события, если триггер сработал при выходе из глубины или с wrong-location bug.
- **Фиксированные координаты ненадёжны** без подтверждения:
  - игрок на **процедурном этаже**, а событие в **`Data/Events/SkullCave`**;
  - SVE меняет **spawn с Desert** `(7,8)` vs vanilla exit `(7,9)` — **needs in-game verify**;
  - runtime `.xnb` / EditMap может отличаться от `tmpMap/vanilla/maps/SkullCave.tmx`.
- **Длинные movement-сцены опасны:** карта **16×10**, узкие «горловины» `(2,5)`, `(13,5)`, `(7,8–9)`; Harvey **не может** надёжно «перехватить» farmer длинным маршрутом — легко упёреться в Front или warp.
- **Инфраструктура триггеров:** `triggerHarveySkullCaveWarning` с **`Mine SkullCave`** (OR) вызывает SkullCave-событие **не на SkullCave** — **Broken / empty scene** ([`events-map-fix-backlog.md`](../events-map-fix-backlog.md)).

### Безопаснее для стабильных сцен

1. **Prevention / warning** — **Desert** у пещеры **`(44,55–58)`** **или** **короткая статика** в входной комнате **`(5,5)/(7,7)`** без движения к `(7,9)`.
2. **Rescue / aftermath** — **`globalFade` + `changeLocation Hospital`** ([`Hospital.md`](Hospital.md)); **не** разворачивать rescue внутри SkullCave или на процедурном этаже.
3. **Новые coords** — только после **`debug export current`** на **вашем** save + SVE; иначе **`Coordinates require exported map`**.

---

## 3. Stable areas

| Area | Stable? | Recommended use | Notes |
|------|---------|-----------------|----------------|
| **Desert cave entrance** `(44,55–58)` | **Yes** (SVE TMX) | **Preferred** для новых prevention «у входа в пустыне» | Warp **`(44,57)→SkullCave (7,8)`**; колонка `x=44` passable **`y=55–58`**; **`(43,57)`/`(45,57)` blocked** — узкий коридор |
| **SkullCave entrance room** `(2,4)–(13,9)` | **Partial** | **Existing** prevention only — farmer `(5,5)`, Harvey `(7,7)` | **~30** passable tiles; TMX verified; **re-verify** после SVE EditMap |
| **SkullCave core dialogue strip** `y=5`, `x=4–12` | **Partial** | Короткий dialogue / `quickQuestion` | Front на `(2,5)`/`(13,5)` — не вести двойной `advancedMove` |
| **SkullCave exit warp** `(7,9)` | **Yes (fixed tile)** | **Transition only** — fade/warp, **не setup NPC** | Warp → **Desert `(8,6)`** (vanilla TMX); Front overlay on tile |
| **`SkullDoor` zone** `(3,3)` | **Yes (blocked)** | **Never** — descent to procedural | Buildings + `SkullDoor` Action |
| **Random Skull Cave levels (`MineShaft`)** | **No** | **Avoid** | Procedural — holes, ladders, monsters, ore |
| **Deep combat / ladder chains** | **No** | **Avoid** | Player state unpredictable after event |
| **Hospital aftermath** | **Yes** (other map) | Rescue, medical follow-up | See [`Hospital.md`](Hospital.md) |

---

## 4. Doors, warps, exits

Подтверждено на **`tmpMap/vanilla/maps/SkullCave.tmx`** + cross-ref **`tmpMap/sve/maps/Locations/Desert.tmx`**. SVE EditMap spawn — **needs in-game check**.

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **7** | **9** | **Warp** | **Desert `(8, 6)`** | **`(7,8)`** pass + Front; **`(6,9)`/`(8,9)`** — **needs check** | **Exit** из SkullCave; **не ставить** Harvey/farmer на `(7,9)` |
| **3** | **3** | **Action `SkullDoor`** | → **MineShaft** (procedural skull levels) | none — **Buildings blocked** | **Descent — never setup**; object at pixel (48,48) |
| **12** | **4** | **Action `none`** | — | none | Buildings blocked — decor |

**External entry to SkullCave:**

| From | Warp / spawn | Notes |
|------|--------------|-------|
| **Desert** | **`(44, 57)` → SkullCave `(7, 8)`** | SVE Desert TMX; spawn **1 tile north** of exit warp `(7,9)` — **needs verify** in-game |
| **SkullCave exit** | **`(7, 9)` → Desert `(8, 6)`** | Vanilla map property `Warp` |

**Procedural level warps / holes / ladders after SkullDoor:** **`needs exact map export / exact level needed`** — not documented here.

---

## 5. Safe staging principles

1. **Prevention-сцена лучше у входа** — **Desert `(44,55–58)`** или **короткая статика** в **входной комнате**; **не** внутри случайного `MineShaft` этажа.
2. **Harvey может физически «перехватить» farmer** в центре комнаты (`move 0 -2`, `-1 0` от `(7,7)` — audit OK), но **не должен блокировать** warp tile **`(7,9)`** или **`SkullDoor (3,3)`** после события.
3. **Rescue aftermath — в Hospital** — если когда-либо добавится skull-rescue, паттерн: **fade → Hospital**, не extended SkullCave crawl (как major mine rescue → Hospital в [`Mine.md`](Mine.md)).
4. **Если сцена всё же внутри SkullCave** — movement **минимальный** (≤4 tiles); только verified path `(7,7)→(7,5)→(6,5)`; **no** route to `(7,9)`, `(3,3)`, `(2,5)`/`(13,5)` bottlenecks.
5. **Не использовать fixed coords** вне **`(2,4)–(13,9)`** без **exact map export** на runtime.
6. **Триггер должен совпадать с локацией патча** — событие в **`Data/Events/SkullCave`** только при **`Current SkullCave`**; для **Mine** — **`eventHarveyMineInterception`** ([`Mine.md`](Mine.md)).
7. **Нет C# pre-warp** для skull prevention (в отличие от `BeginMineRescueWarp`) — setup **`(5,5)/(7,7)`** **обязан** сбрасывать позицию; нельзя полагаться на «где стоял игрок в пустыне».
8. **`topicMineRescuePending`** — блокирует skull triggers на время mine rescue warp; учитывать при тестах.
9. **Камера:** `viewport 7 5` или `(5,5)` — карта крошечная, но Front/AlwaysFront на юге сжимают кадр; **needs in-game check**.
10. **Fork `HarveySkullPromise`** — только dialogue после `quickQuestion`; **без** movement — безопасен на тех же coords.

---

## 6. Safe staging zones

Zones ниже подтверждены на **repo TMX** — **re-verify** после SVE EditMap и `debug export current`.

### `skullcave_entrance_warning`

| Field | Value |
|-------|-------|
| **Range** | **`x=4–12`, `y=4–7`** (core); audit anchors **`(5,5)`** farmer, **`(7,7)`** Harvey |
| **Farmer anchor** | **`(5, 5)`** — `eventHarveySkullCavePrevention` setup |
| **Harvey anchor** | **`(7, 7)`** → optional **`move 0 -2 3`** → **`(7, 5)`** → **`move -1 0 3`** → **`(6, 5)`** |
| **Viewport** | **`(7, 5)`** or **`(5, 5)`** |
| **Use** | prevention, warning dialogue, `quickQuestion` |
| **Risk** | Тесно; не расширять cast; не добавлять второго NPC |

### `skullcave_exit_transition`

| Field | Value |
|-------|-------|
| **Tile** | **`(7, 9)`** — warp → Desert `(8,6)` |
| **Use** | **Player exit only** — `warp` farmer after dialogue **или** штатный walk-out; **не** Harvey `move` сюда |
| **Adjacent** | **`(7,8)`** — passable + Front; last safe tile before warp |
| **Notes** | Блокировка `(7,9)` NPC после event → игрок **не выйдет** в Desert |

### `desert_cave_door_area`

| Field | Value |
|-------|-------|
| **Range** | **Desert `x=44`, `y=55–58`** (SVE TMX passable column) |
| **Warp** | **`(44, 57)` → SkullCave `(7, 8)`** |
| **Use** | **Preferred staging** для **новых** prevention «Harvey у входа в пустыне» **до** warp |
| **Farmer/Harvey (suggested, not in CP yet)** | farmer **`(44,56)`**; Harvey **`(44,58)`** or **`(43,56)`** — **needs in-game check** ( `(43,57)` blocked ) |
| **Viewport** | **`(44, 57)`** |
| **Notes** | Narrative «не лезь в Skull Cave» **без** риска tiny interior / wrong-location trigger |

**If runtime export missing:** write **`Coordinates require exported map`** for any tile outside zones above.

---

## 7. Risk zones

| Zone | Risk | Why |
|------|------|-----|
| **`MineShaft` / skull procedural floors** | **Critical** | Random layout — walls, holes, ladders, enemies |
| **`SkullDoor (3, 3)`** | **Critical** | Descent into procedural; Buildings blocked |
| **Exit warp `(7, 9)`** | **Critical** | Instant Desert transition — NPC block = softlock risk |
| **Holes / ladders (deep levels)** | **Critical** | Uncontrolled floor change — **needs exact level export** |
| **Random rocks / breakables** | **High** | Spawn varies; path closure mid-event |
| **Walls / map edge `x≤1`, `x≥14`, `y≤3`** | **High** | ~30 tiles total — easy out-of-bounds viewport |
| **Monster spawn areas (deep)** | **High** | Combat during cutscene — N/A on entrance map in vanilla |
| **Narrow paths `(2,5)`, `(13,5)`** | **High** | Front overlay; ≤1 neighbor — NPC stuck |
| **South choke `(7,8–9)`** | **High** | Front + warp; `(7,9)` narrow + edge |
| **Dark / visual clutter** | **Medium** | `AmbientLight 140 140 60`, lava walls — readability |
| **`triggerHarveySkullCaveWarning` + Mine** | **Critical (logic)** | Wrong map for `Data/Events/SkullCave` — not a tile issue |
| **Object `(12, 4)` blocked** | **Medium** | Buildings `none` — decor collision |

---

## 8. Events using SkullCave

| Event ID | Trigger source | Fixed coords? | Risk | Recommendation |
|----------|----------------|---------------|------|----------------|
| **`eventHarveySkullCavePrevention`** | **`triggerLocationReactionSkullCaveExit`** — `LocationChanged`, loc=**SkullCave**, Dating/Engaged/Married, no `topicMineRescuePending` | **Yes** `(5,5)` farmer, `(7,7)` Harvey; Harvey **`move 0 -2`**, **`-1 0`** | **Low–Medium** (coords) / **Medium** (trigger timing on exit) | **Keep** coords on confirmed entrance TMX; **short static** scene; **fix** warning trigger separately (Critical) |
| **`eventHarveySkullCavePrevention`** | **`triggerHarveySkullCaveWarning`** — `LocationChanged`, loc=**`Mine` OR `SkullCave`**, injury buffs, Dating/Married | Same coords — event in **`SkullCave`** patch | **Critical** (trigger) | **Doc/backlog only:** condition → **`SkullCave` only**; Mine → **`eventHarveyMineInterception`** |
| **`HarveySkullPromise`** | Fork inside `eventHarveySkullCavePrevention` (`quickQuestion` branch) | Inherits parent setup — **no extra moves** | **Low** | **Keep** dialogue-only fork |

### Related (not SkullCave map)

| Event ID | Map | Note |
|----------|-----|------|
| **`eventHarveyMineInterception`** | **Mine** | Correct counterpart for **Mine** entry — [`Mine.md`](Mine.md) |
| **Major/minor mine rescue** | **Mine → Hospital** | C# may flag pass-out in SkullCave name, but cutscene plays on **Mine `(17,7)`** — narrativ gap if player died in skull depths |

### C# constraints (reference only — do not edit in doc tasks)

```csharp
// PassOutHandler — SkullCave treated as mine-related for pass-out tracking
name.Contains("SkullCave", StringComparison.OrdinalIgnoreCase)
// IsPlayerInMineOrVolcano — includes SkullCave name + MineShaft class
// No BeginSkullCaveRescueWarp — unlike Mine (17,7)
```

---

## 9. Recommended alternatives

For **risky** или **new** skull-related scenes:

| Instead of | Prefer |
|------------|--------|
| Long Harvey chase inside SkullCave | **Desert** `desert_cave_door_area` — static dialogue before warp |
| Prevention on procedural floor | **Impossible** — warp player to entrance first (C#) **or** stage on Desert |
| Rescue aftermath in cave | **`globalFade` + `changeLocation Hospital`** — bed/seated pattern [`Hospital.md`](Hospital.md) |
| Fixed coords on unexported level | **`Coordinates require exported map`** — or don't ship |
| `triggerHarveySkullCaveWarning` firing in **Mine** | **Split triggers** — Mine interception vs SkullCave prevention ([`events-map-fix-backlog.md`](../events-map-fix-backlog.md)) |
| Extended `move` / `advancedMove` | **`quickQuestion` + faceDirection** — current prevention pattern |
| Blocking `(7,9)` with NPC | End event with **`faceDirection`** / fade — let player walk out |
| New coords in tiny room | **Move scene to Desert** or **Hospital** — SkullCave interior only if TMX re-verified |

---

## 10. Quick SkullCave rules

1. **Status: high-risk** — stable **only** for confirmed **entrance room** TMX; procedural depths **never** for fixed coords.
2. **`SkullCave` ≠ `MineShaft`** — CP patches **`Data/Events/SkullCave`** = **16×10 entrance** only.
3. **No `SkullCaveEntrance` location name** — use **`SkullCave`** in CP/triggers.
4. **~30 passable tiles** — smallest staging map in mod; prefer **short static** scenes.
5. **Audit anchors:** farmer **`(5, 5)`**, Harvey **`(7, 7)`** — **OK** on repo vanilla TMX.
6. **Max verified Harvey path:** **`(7,7)→(7,5)→(6,5)`** — do not extend toward south warp.
7. **Forbidden: setup NPC on `(7, 9)`** — Desert exit warp.
8. **Forbidden: setup on `SkullDoor (3, 3)`** — procedural descent.
9. **Forbidden: long movement** through `(2,5)`, `(13,5)`, `(7,8–9)` bottlenecks.
10. **Forbidden: fixed coords on random skull levels** without per-level export.
11. **New prevention → prefer Desert `(44,55–58)`** over interior — [`Desert.md`](Desert.md).
12. **Rescue aftermath → Hospital** — not SkullCave extended scenes.
13. **Use `globalFade` / `changeLocation`** for location shifts — not walk through warp during event.
14. **Trigger location must match event patch** — **`SkullCave` only** for skull prevention (**Critical** fix for `triggerHarveySkullCaveWarning`).
15. **Mine entry with injury → `eventHarveyMineInterception`**, not skull event — [`Mine.md`](Mine.md).
16. **SVE may change Desert↔Skull spawn `(7,8)` vs `(7,9)`** — verify in-game after EditMap.
17. **No C# pre-warp** for prevention — rely on event **setup** lines.
18. **Fork `HarveySkullPromise`** — dialogue only; safe on parent coords.
19. **Runtime TMX export required** before any new coord outside §6 zones.
20. **Do not edit events** in documentation tasks — passport for review/planning only.

---

**Метод:** `tmpMap/vanilla/maps/SkullCave.tmx` + `tmpMap/sve/maps/Locations/Desert.tmx` + [`mine-events-map-risk-audit.md`](../mine-events-map-risk-audit.md) + [`events-coordinate-audit.md`](../events-coordinate-audit.md).  
**Не учтено:** per-level `MineShaft` skull layouts, live monster state, unexported SVE EditMap diffs, exact runtime spawn tile `(7,8)` vs `(7,9)`.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
