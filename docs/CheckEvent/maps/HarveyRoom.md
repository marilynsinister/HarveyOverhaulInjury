# Map Passport: HarveyRoom

## 1. Metadata

| Field | Value |
|-------|--------|
| **LocationName (CP)** | **`HarveyRoom`** |
| **Related names** | **`Hospital`** — лестница/офис на 2-м этаже клиники; warp **`Hospital (9–10, 1)` → HarveyRoom `(6, 12)`**; **не путать** с палатой **`Hospital (20, 5)`** (major bed — другая карта) |
| **Map asset** | `Maps/HarveyRoom` |
| **Map file (audit)** | **Needs map export** — `HarveyRoom.tmx` / `HarveyRoom.xnb` **отсутствует** в репозитории |
| **Source** | **Vanilla interior** (`townInterior` / `townInterior_2`); SVE **не Load-ит** отдельную карту HarveyRoom (только Hospital warp) |
| **Size (approx)** | **~12×16** tiles, 16×16 px — типичный vanilla bedroom interior; **не подтверждено TMX** |
| **Passable (audit)** | **Мало** — одна «полоса» от двери к центру + узкие проходы между мебелью; точный подсчёт **needs map export** |
| **Status** | **partial-by-audit** — warp из Hospital TMX **подтверждён**; coords checkup-сцен из catalog/audit; мебель/обратный warp — **approximate / needs map export** |
| **Properties** | Indoor; без `Outdoors`; Harvey schedule: кровать (ночь), холодильник (утро), bookshelf (вечер) |

**Used by events:**

| Event ID | File | HarveyRoom act? |
|----------|------|-----------------|
| `eventHarveyRoomCheckup` | events.json | yes — домашний осмотр, 6♥ |
| `eventHarveyRoomCheckup2` | events.json | yes — dating + BETAS + Random 0.2 |

**Not on HarveyRoom:** все Hospital-палатные сцены, Town storm, Farm comfort — см. [`Hospital.md`](Hospital.md), [`Town.md`](Town.md).

**Связанные документы:** [`Hospital.md`](Hospital.md) (warp upstairs), [`events-map-audit-plan.md`](../events-map-audit-plan.md), [`physical-contact-audit.md`](../../harvey-relationship-visits-audit/physical-contact-audit.md), [`01-cp-events-catalog.md`](../../events-inventory/01-cp-events-catalog.md).

---

## 2. Important areas

| Area | Approx range / anchor | Role | Staging notes |
|------|----------------------|------|---------------|
| **Room entrance / door (south)** | **`(5–7, 11–12)`** — **spawn `(6, 12)` confirmed** | Вход с лестницы Hospital; «неожиданный визит» | **Не блокировать** warp tile; farmer warp **`6 12`** в обоих checkup |
| **Center dialogue strip** | **`(6–9, 8–10)`** — canon **`(7,9)` / `(8,9)`** | Основной осмотр, разговор лицом к лицу | **1 тайл** между farmer и Harvey — audit OK |
| **Bed area (northwest)** | **approx `(2–5, 3–6)`** | Сон Harvey, усталость, «тихая забота» | **Buildings** — **не setup**; lying только с полным палатным комплексом (**не используется** в текущих CP) |
| **Desk / radio / model planes (northeast)** | **approx `(8–11, 3–5)`** | Harvey читает/пишет; vanilla 8♥ (радио, модели) | Узко; **короткий move**; **не ставить** farmer на стол |
| **Chair / reading spot** | **approx `(7–9, 5–7)`** | Кресло у стола / у bookshelf | Front overlay — **needs in-game check** |
| **Bookshelf / fridge (east & west walls)** | **Shelf ~`(10–11, 6–9)`**; **Fridge ~`(2–3, 6–8)`** | Schedule anchors Harvey | **Blocked / Front** — декор, не staging |
| **Window (east wall, mid)** | **approx `(10–11, 6–9)`** | Checkup2: «бросается к окну» | **Narrative anchor only** — exact tile **needs map export** |
| **Private conversation spot** | **`(7, 9)`–`(8, 9)`** | 6♥ med tone — **1 tile apart** | Pre-dating: **дистанция**, без overlap |
| **Medical / comfort spot** | **`(7, 9)` / `(8, 9)`** + optional **`(6, 9)`** entrance approach | Осмотр, термометр, тонометр (текст) | **Без** `positionOffset` поцелуя на 6♥; dating Checkup2 — waist grab **text-only** in verified build |

---

## 3. Doors/warps

Подтверждено на **`tmpMap/sve/maps/Locations/Hospital.tmx`** (и vanilla Hospital): property `Warp` включает **`HarveyRoom 6 12`**.

| X | Y | Type | Destination | Safe nearby tiles | Notes |
|---:|---:|------|-------------|-------------------|-------|
| **6** | **12** | **Warp spawn** (from Hospital) | Entry from **`Hospital (9–10, 1)`** | **`(6, 11)`**, **`(6, 10)`**, **`(7, 11)`** — **needs TMX** | **Confirmed** landing tile в CP checkup: `warp farmer 6 12` |
| **7** | **12** | **Warp?** (typical pair) | **Likely** Hospital return — **needs map export** | **`(7, 11)`**, **`(6, 11)`** | Vanilla interiors often dual-tile door; **not verified in repo** |
| **—** | **—** | **Hospital upstairs** | **`Hospital (9, 1)`** and **`(10, 1)`** → **`HarveyRoom (6, 12)`** | **`Hospital (9, 2)`**, **`(10, 2)`** | External entry — см. [`Hospital.md`](Hospital.md) §Doors |
| **—** | **—** | **Return warp (approx)** | **`HarveyRoom → Hospital (9, 2)` or `(10, 2)`** | **`(6, 11)`** north of door | **Needs map export** — exact reverse warp string |

**External entry:**

| From | Spawn / warp | Notes |
|------|--------------|-------|
| **Hospital upstairs** | **`(6, 12)`** | `playSound doorClose` в checkup после warp |
| **Vanilla event entry** | CP patches **`Data/Events/HarveyRoom`** — триггер при входе в комнату | Setup скрывает farmer **`1000 1000`** до warp |

**Правило:** **никогда** не ставить Harvey/farmer на **`(6, 12)`** / **`(7, 12)`** в финале сцены — игрок не выйдет на лестницу.

---

## 4. Furniture and blockers

**Все координаты мебели — approximate (vanilla layout + schedule), пока нет `HarveyRoom.tmx` в repo.** После экспорта — заменить на TMX Buildings/Front.

| Object | Coords/range | Blocks movement | Blocks visibility | Notes |
|--------|--------------|-----------------|-------------------|-------|
| **Bed** | **approx `(2–4, 3–5)`** | **Yes** (Buildings) | Partial (Front headboard) | Северозапад; Harvey sleep schedule; **no** farmer warp on bed без `ignoreCollisions` + offset |
| **Desk / table (radio, papers)** | **approx `(9–11, 3–4)`** | **Yes** | Partial | Северо-восток; model planes (vanilla 8♥); **не** end dialogue on desk tile |
| **Chair / armchair** | **approx `(8–9, 5–6)`** | **Mixed** (often pass + Front) | Partial | У стола; посадка: **`faceDirection farmer 2`** → **`showFrame farmer 107`**; Harvey южнее — **`faceDirection Harvey 0`** |
| **Bookshelf** | **approx `(10–11, 6–8)`** | **Yes** | Yes (east wall) | Evening reading spot; **не** block window narrative path without verify |
| **Fridge / microwave** | **approx `(2–3, 6–8)`** | **Yes** | Partial | West wall; morning schedule |
| **Shelves / wall decor** | **approx `(0–1, 4–10)`**, **`(11, 4–10)`** | **Yes** (perimeter) | Partial | `townInterior` wall objects — сжимают проходимую зону |
| **Door (south)** | **`(5–7, 12)`** + Front mat **`y=11`** | **Warp** — **do not block** | — | **Confirmed** spawn **`(6, 12)`**; Checkup2 «блокирует дверь» — Harvey **рядом**, не on warp |
| **Window (east)** | **approx `(11, 6–9)`** | **Yes** (Buildings) | — | Checkup2 escape attempt — **text + move inferred**, coords **needs export** |
| **TV / small decor** | **approx `(4–6, 3–4)`** | **Mixed** | Low | Between bed and center — **needs TMX** |

**Проходимая «спина» комнаты (audit inference):** колонка **`x=6–8`**, **`y=8–11`** — подтверждена opening moves checkup (`6,12)→(6,9)→(7,9)` + Harvey `(9,9)→(8,9)`.

---

## 5. Safe staging zones

Zones **§5a–§5d** — для **новых** сцен и ревью. Текущие checkup-события **manually-verified-do-not-touch** — coords ниже **описывают canon**, не задачу на правку.

### `harveyroom_entrance_dialogue`

| Field | Value |
|-------|-------|
| **Range** | **`x=5–8`, `y=10–12`** — дверь + первые 2–3 тайла в комнату |
| **Farmer anchor** | **`(6, 12)`** on warp **или** **`(6, 11)`** после `move 0 -1` |
| **Harvey anchor** | **`(8, 11)`** or **`(9, 10)`** — встреча у порога, **2–3 тайла** дистанция |
| **faceDirection** | Farmer **0** (north into room) or **1** (east); Harvey **3** (west) or **2** (south) |
| **Movement** | **`move farmer 0 -1`** or **`0 -2` max** — как checkup **`0 -3`** только если весь столбец passable (**verified** для opening) |
| **viewport** | **`8 9`** (checkup canon) or **`6 11`** — дверь + Harvey в кадре |
| **Use** | Вход / неожиданный визит / «playSound doorClose» |
| **Risks** | **Block `(6,12)`**; длинный path от двери к кровати через мебель; overlap при **0** distance на **`y=12`** |

### `harveyroom_bedside_private_talk`

| Field | Value |
|-------|-------|
| **Range** | **`x=4–6`, `y=5–8`** — **approach only**, не on bed Buildings |
| **Farmer anchor** | **`(5, 7)`** or **`(6, 8)`** |
| **Harvey anchor** | **`(4, 7)`** or **`(5, 8)`** — у изголовья, **1–2 тайла** |
| **faceDirection** | Harvey **1** (east toward bed) or **2**; farmer **3** (west) — «тихий разговор» |
| **Movement** | **Max 2 tiles** from center strip — **`move ±1,0`** or **`0,-1`**; **no** diagonal `move` |
| **viewport** | **`5 7`** — bed in frame edge, actors on pass tiles |
| **Use** | Забота / усталость / pre-dating comfort (**не** lying без Hospital bed pattern) |
| **Risks** | **Bed Buildings `(2–4,3–5)`** — farmer inside mattress; **длинный march** от двери; романтика **слишком близко** на 6♥ |

### `harveyroom_desk_area`

| Field | Value |
|-------|-------|
| **Range** | **`x=7–10`, `y=4–6`** — подход к столу, не on desk |
| **Farmer anchor** | **`(7, 6)`** or **`(8, 7)`** |
| **Harvey anchor** | **`(9, 5)`** at desk **или** **`(8, 5)`** chair — Harvey **1 tile** from desk edge |
| **faceDirection** | Harvey **2** (south, over papers) or **3**; farmer **1** or **0** |
| **Movement** | **Static preferred**; max **`move 1,0`** ×1 |
| **viewport** | **`9 5`** — desk + radio in frame |
| **Use** | Harvey читает/пишет/ищет записи; «плановый осмотр» paperwork beat |
| **Risks** | **Desk `(9–11,3–4)` blocked**; farmer on chair Front; **`faceDirection farmer 2`** → **`showFrame 107`**; Harvey **`faceDirection 0`** с юга |

### `harveyroom_chair_exam`

| Field | Value |
|-------|-------|
| **Range** | **`x=7–9`, `y=5–7`** — стул у стола, farmer на стуле, Harvey напротив с юга |
| **Farmer anchor** | **`(8, 5)`** or **`(8, 6)`** — final sitting tile (**needs export**) |
| **Harvey anchor** | **1 tile south** of farmer — напр. farmer `(8,5)` → Harvey **`(8,6)`** |
| **faceDirection** | Farmer **`2`** (лицом вниз); Harvey **`0`** (спиной к камере, лицом к farmer) |
| **Посадка** | `move` → `stopAnimation farmer` → `faceDirection farmer 2` → `showFrame farmer 107` → `pause 300` |
| **Harvey exam** | `move Harvey …` → `stopAnimation` → `faceDirection Harvey 0` → `animate … 22 20 21 20` |
| **Use** | Осмотр / разговор у стула в кабинете |
| **Risks** | `showFrame` не перемещает actor — final tile до showFrame; обязателен **`faceDirection 2`** перед `107` |

### `harveyroom_close_dialogue_safe`

| Field | Value |
|-------|-------|
| **Range** | **`x=6–9`, `y=8–10`** — center strip |
| **Farmer anchor** | **`(7, 9)`** — **checkup canon** |
| **Harvey anchor** | **`(8, 9)`** — **checkup canon** (after `move Harvey -1 0 3` from **`(9,9)`**) |
| **faceDirection** | Farmer **1** (east); Harvey **3** (west) — **face each other**, **1 tile gap** |
| **Movement** | Opening only: farmer **`0 -3`**, **`1 0`**; Harvey **`-1 0`** — **≤3 tiles total** |
| **viewport** | **`8 9`** — setup line in both checkup events |
| **Use** | Romance / high trust **без overlap** — dating talk; **не** 6♥ med (там же coords OK с **локтем**, text-only) |
| **Risks** | **`positionOffset` kiss** → sprites in desk/bed; **same tile `(8,9)`×2** — forbidden; Checkup2 waist grab needs **offset reset** before next `move` |

**If runtime export missing:** any coord outside **`(6–9, 8–10)`** and door **`(6,12)`** → write **`Coordinates require exported map`**.

---

## 6. HarveyRoom-specific rules

1. **Movement короткий — 1–2 тайла** на шаг; opening checkup **`0 -3` + `1 0`** — **upper bound** для этой карты.
2. **Не делать длинные маршруты** (≥4 tiles) через комнату — застрянут в кровати/столе/углах.
3. **Не ставить персонажей в мебель** — bed, desk, shelf = Buildings; проверять после **`warp`**.
4. **Не перекрывать дверь** — **`(6,12)`** / **`(7,12)`** и **`y=11`** подход — Harvey «блокирует дверь» = **adjacent tile**, не warp.
5. **Близость осознанная:** **1 tile apart** (`(7,9)`/`(8,9)`) — OK для 6♥ med + dating dialogue; **same tile / kiss offset** — **только dating/married** + reset offset.
6. **Ранние / медицинские сцены — дистанция:** 6♥ checkup: **локоть** (message), не рука; **без** Heart emote в тревоге.
7. **После поцелуя / `positionOffset`** — **`positionOffset Actor 0 0`** перед следующим `move`; проверить, что спрайты **не в кровати/столе**.
8. **Статичная постановка > advancedMove** — комната меньше SkullCave по площади, но **больше мебели**.
9. **`viewport 8 9`** — default; не pan за **`x<0`**, **`y<0`**, **`x>11`**, **`y>15`** (approx bounds).
10. **Indoor private** — интимнее Town/Hospital reception; но **не** заменяет палатный **`(20,5)`** pattern.
11. **Checkup2 window/door chase** — если правите (не в doc-only): каждый `move` **≤2**, без route через **`(2–4,3–5)`** bed.
12. **BETAS gate** on Checkup2 — reachability отдельно от coords; passport не меняет preconditions.
13. **Harvey schedule** — evening bookshelf / morning fridge: **не conflicting** с event staging если event по entry warp.
14. **Не использовать `positionOffset` как замену тайла** — в тесной комнате ±12 px может = overlap с Front.

---

## 7. Risk zones

| Zone | Coords/range | Risk | Why | Avoid |
|------|--------------|------|-----|-------|
| **Door / warp tile** | **`(6, 12)`**, **`(7, 12)?`** | **Critical** | Hospital transition; NPC block = softlock | Setup / end position / long pause |
| **Bed tiles** | **approx `(2–4, 3–5)`** | **Critical** | Buildings — NPC inside bed | `warp` без `ignoreCollisions` |
| **Desk / table** | **approx `(9–11, 3–4)`** | **High** | Blocks move + wrong «осмотр at desk» read | Standing dialogue coords |
| **Chair Front overlay** | **approx `(8–9, 5–6)`** | **Medium** | Visual float / stuck | **needs TMX** |
| **Bookshelf / east wall** | **approx `(10–11, 4–9)`** | **High** | Narrow; window narrative | Long eastward `move` |
| **Fridge west** | **approx `(2–3, 6–8)`** | **Medium** | Blocks west path | Detour through bed corner |
| **Narrow gaps** | **`x=5–6` between bed & center**; **`x=9–10` desk–shelf** | **High** | 1-tile choke | Double NPC `move` same tick |
| **Wall / furniture corners** | Perimeter **`x≤1`**, **`x≥10`**, **`y≤2`** | **High** | Out of walkable island | Viewport clamp only — not walk |
| **Harvey / farmer overlap** | **`(8, 9)`×2** or offset without reset | **Visual / logic** | Sprites stack; next `move` breaks | 1 tile min on med; reset offset |
| **Center strip overload** | **`(7–8, 9)`** + props | **Medium** | All CP dialogue here — crowded with 3rd actor | Extra NPCs |
| **Unexported coords** | Any new `(x,y)` | **High** | No TMX in repo | **`Coordinates require exported map`** |

---

## 8. Events using HarveyRoom

| Event ID | File | Status | Opening coords (audit) | Notes |
|----------|------|--------|------------------------|-------|
| **`eventHarveyRoomCheckup`** | events.json | **manually-verified-do-not-touch** | **`viewport 8 9`**; Harvey **`(9, 9)`** f**3** → **`(8, 9)`**; farmer warp **`(6, 12)`** → **`(6, 9)`** → **`(7, 9)`** | **6♥** (`Friendship 1500`); pre-dating med; локоть (text); **не менять** без explicit task |
| **`eventHarveyRoomCheckup2`** | events.json | **manually-verified-do-not-touch** | Same opening as Checkup1 | **Dating** + **Spiderbuttons.BETAS** + **Random 0.2** — часто **недостижимо**; door block + window + waist (dating OK per physical-contact audit); full move coords **needs export** |

**Script preview (shared opening, catalog):**

```
8 9/
farmer 1000 1000 0 Harvey 9 9 3/
playSound doorClose/
warp farmer 6 12/
move farmer 0 -3 1 true/
move Harvey -1 0 3 true/
move farmer 1 0 1 true/
```

**Fork / branches:** Checkup1 — escape elbow (message, minimal move after); Checkup2 — retreat to door, block door, window intercept — **movement body not in repo preview** → §7 window/door zones **approx**.

**No other CP events** on `Data/Events/HarveyRoom` per [`06-locations-index.md`](../../events-inventory/06-locations-index.md).

**Vanilla reference (not CP):** Harvey **8♥** (`Enter Clinic`) — radio/window/model planes in same map; coords **not in HarveyOverhaul audit** — use only for layout hints, not CP edits.

---

## 9. Quick HarveyRoom rules

1. **Status: partial-by-audit** — export `HarveyRoom.tmx` before new fixed coords.
2. **Only 2 CP events** — оба **manually-verified-do-not-touch**.
3. **Confirmed spawn:** **`warp farmer 6 12`** from Hospital **`(9–10, 1)`**.
4. **Confirmed dialogue pair:** farmer **`(7, 9)`**, Harvey **`(8, 9)`**, **`viewport 8 9`**.
5. **Max opening movement:** farmer **`0 -3` + `1 0`**; Harvey **`-1 0`** — **do not extend** without TMX.
6. **Forbidden: NPC on `(6, 12)`** at scene end.
7. **Forbidden: long routes** across room (bed → window → door) без per-tile verify.
8. **Forbidden: farmer on bed/desk** without Hospital-style collision bypass.
9. **Forbidden: same-tile overlap** on med 6♥ — **1 tile gap** minimum.
10. **6♥ med:** elbow message OK; **no** romantic hand-hold / Heart emote.
11. **Dating Checkup2:** closer contact **text-only** in verified build — still **reset `positionOffset`** if ever added.
12. **Prefer static staging** over `advancedMove`.
13. **Door block narrative** = Harvey on **`(6, 11)`/`(7, 11)`**, not on warp.
14. **Window beat** = east wall **approx `(11, 6–9)`** — **needs export** before new moves.
15. **Private room ≠ Hospital bed `(20,5)`** — different map, different rules.
16. **Camera default `(8, 9)`** — small map; avoid viewport outside ~12×16.
17. **faceDirection** after `speak` — med exam faces each other (**1/3**).
18. **BETAS + Random 0.2** — reachability issue, not coord issue.
19. **New zones** → use §5 IDs; outside → **`Coordinates require exported map`**.
20. **Do not edit events** in documentation-only tasks — passport for review/planning.

---

**Метод:** `Hospital.tmx` warp property + [`01-cp-events-catalog.md`](../../events-inventory/01-cp-events-catalog.md) script preview + [`events-map-audit-plan.md`](../events-map-audit-plan.md) manual verify + vanilla layout **approx** from schedule/wiki.  
**Не учтено:** runtime `HarveyRoom.xnb`, exact Buildings/Front per tile, reverse warp string, Checkup2 mid-script coords, `positionOffset` if added later.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
