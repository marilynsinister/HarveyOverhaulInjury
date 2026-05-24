# Map Passport: Hospital

## Metadata

| Field | Value |
|-------|--------|
| **LocationName** | Hospital |
| **Map asset** | `Maps/Hospital` |
| **Map file** | `tmpMap/sve/maps/Locations/Hospital.tmx` (fallback: `tmpMap/vanilla/maps/Hospital.tmx`) |
| **Source** | SVE Load (`Hospital.tbin`); база vanilla `townInterior` + `townInterior_2` |
| **Size** | 24×20 tiles, 16×16 px |
| **Status** | **ready** (TMX в repo; runtime SVE Load может слегка отличаться — **needs in-game check** после патчей) |
| **Layers** | Back, Back2, Buildings, Front, Front2, Front3, Paths, AlwaysFront |
| **Map properties** | `Doors`, `Warp`, `DayTiles`, `NightTiles` (анимация кроватей на Front y=1) |

**Ключевая роль для мода:** осмотры, госпитализация, emergency care, first treatment, treatment plan/review, recovery, stay in hospital, mine-rescue aftermath, storm comfort fork, night crisis, story arc E2/E5/E6, dating medical check.

**Связанные документы:**

- [`tmpMap/Hospital_event_placement_analysis.md`](../../tmpMap/Hospital_event_placement_analysis.md) — палаты A/B, C# `HospitalBedX/Y=20,5`
- [`events-coordinate-audit.md`](../events-coordinate-audit.md) — постановка по событиям
- [`cp-event-authoring-rules.md`](../../EventPatterns/cp-event-authoring-rules.md) §2.3, §7, §12.3

**Used by events (CP):**

| Event ID | File | Audit / manual |
|----------|------|----------------|
| `HarveyMod_FirstTreatment` | events.json | needs-review (farmer 5,9 Buildings) |
| `HarveyOverhaulStory.E2_InsistentExam` | events.json | needs-review |
| `HarveyOverhaulStory.E5_StormBeside` | events.json | warning |
| `HarveyOverhaulStory.E6_SayItOutLoud` | events.json | warning |
| `eventHarveyMedicalCheck_Dating` | events.json | needs-review |
| `HarveyMod_NightCrisis_Dating` / `_PreDating` | events.json | warning |
| `HarveyMod_BirthdayHospital_Dating` / `_Friend` | events.json | warning |
| `eventHarveyMineRescue` / `_Dating` | eventsMineRescue.json | checked-ok (20,5) |
| `eventHarveyMinorMineRescue` | eventsMineRescue.json | checked-ok (14,6) |
| `eventRescueOperation` | events.json | partial (3,15 phone; 20,5 fin) |
| `eventHarveyMedicalCheck` | events.json | manually-verified-do-not-touch |
| `eventHarveyEmergencyCare` | events.json | manually-verified-do-not-touch |
| `eventHarveyExhaustion` | events.json | manually-verified-do-not-touch |
| `eventHarveyTraumaExam` | events.json | manually-verified-do-not-touch |
| `eventHarveyTreatmentCollapse` | events.json | manually-verified-do-not-touch |
| `eventStayInHospital` | events.json | manually-verified-do-not-touch |
| `HarveyMod_TreatmentPlanMeeting` | events.json | manually-verified-do-not-touch |
| `HarveyMod_TreatmentReview` | events.json | manually-verified-do-not-touch |
| `HarveyMod_RecoveryComplete` | events.json | manually-verified-do-not-touch |
| `HarveyMod_NightCrisis` | events.json | manually-verified-do-not-touch |
| `HarveyMod_BirthdayHospital` | events.json | manually-verified-do-not-touch |

**Town ↔ Hospital:** вход с улицы — Town `LockedDoorWarp (36,55)` → Hospital `(10,19)`; выход Hospital warp `(10,20)` → Town `(36,56)`.

---

## Confirmed safe coordinates

Координаты проверены по **TMX** (`tmpMap/sve/maps/Locations/Hospital.tmx`).  
`pass=True` = Back проходим, Buildings=0. **Front** может быть — NPC часто ставится, но `move` ненадёжен.

| Purpose | Harvey X/Y | Farmer X/Y | Dir Harvey | Dir farmer | Notes |
|---------|------------|------------|------------|------------|-------|
| **Major bed — Harvey bedside** | **19, 5** | **20, 5** ‡ | **1** (east) | **2** (south) | ‡ farmer: Buildings; **only** `ignoreCollisions` + `positionOffset 32 -52` + lying `animate 4 5`. Harvey: Front OK |
| **Major bed — Harvey south approach** | **20, 6** | 20, 5 ‡ | **0** (north) | 2 | Подход с юга; не `move` на (20,5) |
| **Minor bed — exam seated** | **15, 6** | **14, 6** | **3** (west) | 0–2 | `eventHarveyMinorMineRescue`; open floor |
| **Minor alt farmer** | 15, 6 | **13, 6** | 3 | **1** (east) | side angle |
| **West exam — Harvey** | **4, 5** | **4, 6** or **6, 10** | **1–2** | 0/2 | FirstTreatment / кушетка; `(4,5)` pass, `(3,5)` Front |
| **West exam — farmer couch** | 4, 5 | **6, 10** | 1 | **0** (north) | E2 exam; `(6,10)` pass + Front |
| **E2 Harvey start** | **1, 5** | (enters from door) | **2** | — | Front overlay — **needs in-game check** |
| **Corridor / NightCrisis** | **15, 8** | **14, 8** or **16, 8** | **1** | **3** | `(15,8)` pass; dating night crisis |
| **Reception — Harvey** | **10, 14** | **10, 19** | **2** (south) | **0** (north) | E5/E6/Birthday; `(10,19)` near exit |
| **Reception — Harvey desk** | **10, 18** | **10, 19** | **2** | 0 | E6 setup; Harvey Front — needs check |
| **Reception move cluster** | **9, 15**–**10, 16** | **10, 15**–**10, 16** | varies | varies | E5/E6 short moves — TMX pass |
| **Phone / rescue op** | **3, 15** | off-screen | **0** | — | `(3,15)` pass + Front; Lewis `(1000,1000)` |
| **Phone alt (needs check)** | 3, 15 | — | — | — | не warp Town `(3,15)` — это телефонная зона, не exit |
| **Treatment plan talk** | **4, 5** | **5, 5** / seated **5, 5** | **1** | **3** | `HarveyMod_TreatmentPlanMeeting` viewport `(5,7)` — **needs in-game check** |
| **Harvey private office door** | warp from **9–10, 5** | — | — | — | Door Harvey → HarveyRoom; **не setup** |
| **Exit to Town** | — | **10, 19** | — | **0** | `(10,19)` pass; warp dest `(10,20)` OOB south |
| **HarveyRoom warp** | — | — | — | — | `(9,1)`, `(10,1)` pass + Front → HarveyRoom `(6,12)` |

**C# / config (major bed):** `ModConfig.HospitalBedX/Y = 20, 5`; `HospitalizationManager.WarpToHospitalBed()`.

---

## Beds and medical positioning

### Палата B — major bed (mod default)

```
        x: 18  19  20  21  22
y=3        #   #   #   #   #   Buildings (шкафы/стена)
y=4        .   #   #   #   #   изголовье Buildings
y=5        f   f   B   B   #   B = bed 1100/1101; farmer ‡(20,5)
y=6        f   f   .   f   f   Harvey approach (20,6) или (19,5)
y=7        #   #   .   #   #
```

| Role | X | Y | TMX | Pattern |
|------|---|---|-----|---------|
| Farmer lying | 20 | 5 | Buildings | `ignoreCollisions` → `warp` → `positionOffset 32 -52` → `animate true true … 4 5` → `faceDirection 2` |
| Harvey at bedside | 19 | 5 | pass, Front | static; `faceDirection 1` |
| Maru (if used) | 18–21 | 5–7 | avoid doors/bed | **needs check** per script |

### Палата A — minor / seated exam

```
        x: 12  13  14  15  16
y=5        #   .   #   #   .   #
y=6        #   .   .   .   .   #   ← core (13–16, 6)
y=7        f   f   .   .   .   #
```

| Role | X | Y | Pattern |
|------|---|---|---------|
| Farmer seated/weak | 14 | 6 | `animate 4 5` или `showFrame farmer 107` + `faceDirection 2` — без offset |
| Harvey exam | 15 | 6 | `faceDirection 3` |

### Нижний зал (station ward) — y=14–18

Кровати Buildings **843/844/875** около `(11–15, 15–17)`. **Не путать** с major `(20,5)`.  
Текущие CP mine-rescue / `HospitalBedX/Y` **сюда не ведут**. Для новых сцен — **needs map export / in-game check**.

### Медицинские «роли» на карте

| Сцена | Лучшая зона | Harvey | Farmer | Команды (подтверждённые CP) |
|-------|-------------|--------|--------|----------------------------|
| **Слушает лёгкие / стетоскоп** | West `(4,5)–(6,10)` | `(5,5)` | `(5,4)` seated | `faceDirection farmer 2` → `showFrame farmer 107`; Harvey `(5,5)` face **0**; `animate Harvey … 22 20 21 20` |
| **Читает карту / бумаги** | Reception `(10,14)–(10,16)` or NightCrisis `(15,8)` | `(15,8)` / `(10,14)` | nearby | narrative + `speak`; TreatmentPlan `(5,7)` viewport — needs check |
| **Приносит чай / лекарство** | Bedside `(19,5)` or `(15,6)` | у кровати | lying/seated | `addItem` + `pause`; без long move |
| **Farmer слабая / дрожит** | `(10,19)` E5; `(14,6)` collapse | рядом | `(10,19)` / `(14,6)` | `startJittering`; `animate farmer … 5 4` (fear); `emote 12` |
| **Осмотр у кушетки** | `(4,6)–(6,10)` | `(4,5)` | `(4,6)` / `(6,10)` | short `move`; **не** `(5,9)` door |
| **Приватный разговор** | `(15,8)` / `(9,16)` | `(15,8)` | `(14,8)` | evening `ambientLight 100 90 75` (E6) |
| **Emergency carry-in** | fade → `(14,6)` or `(20,5)` | warp after fade | warp + animate | `changeLocation Hospital` + re-setup |

---

## Doors and warps

### Входы / выходы

| X | Y | Type | Destination / Action | Safe nearby tiles | Notes |
|---:|---:|------|----------------------|-------------------|-------|
| **—** | **—** | **Town entrance** | Town `(36,55)` → **Hospital `(10,19)`** | `(10,19)`, `(9,19)`, `(11,19)` | Внешняя дверь на Town; farmer enter E5/E6 |
| **10** | **20** | **Warp** | **Town `(36, 56)`** | `(10,19)` south | Exit; **не setup** на y=20 (OOB в TMX) |
| **9** | **1** | Warp | HarveyRoom `(6, 12)` | `(9,2)`, `(10,2)` | pass + Front |
| **10** | **1** | Warp | HarveyRoom `(6, 12)` | same | Harvey office upstairs |
| **5** | **9** | Door (Action) | open=1 closed=120 | **none** (Buildings) | West corridor; **не setup**; `doAction` only |
| **5** | **13** | Door | open=1 closed=120 | none | Internal |
| **10** | **13** | Door | open=1 closed=120 | none | Dating medical check `doAction` — risky |
| **9** | **5** | Door Harvey | open=1 closed=120 | none | Office |
| **10** | **5** | Door Harvey | open=1 closed=120 | none | Office |

### Safe tiles у входа с Town

| Tile | pass | Use |
|------|------|-----|
| `(10,19)` | yes | farmer spawn entering (E5, E6, Birthday) |
| `(10,18)` | yes, Front | Harvey behind farmer — needs visual check |
| `(9,19)`, `(11,19)` | yes | flank positions |
| `(10,20)` | OOB | warp property only — **never setup** |

---

## Furniture blockers

| Object | Coords (approx) | Layer | Blocks move? | Blocks view? | Event note |
|--------|-----------------|-------|--------------|--------------|------------|
| **Major bed** | (20,5), (21,5) | Buildings | **yes** | Front bedding | farmer lying pattern only |
| **Bed headboard** | (19,4)–(22,4) | Buildings | yes | yes | |
| **Minor ward beds** | (11–15, 15–17) | Buildings | yes | yes | not mod default bed |
| **HospitalShop counter** | (5–7, 16) | Buildings | yes | partial | reception; don't block path south |
| **Doors** | (5,9), (5,13), (10,13), (9–10,5) | Buildings | yes | — | Action Door |
| **Exam messages** | (1,4), (3,4), (7,9), (16–19, 2–3) | Buildings | yes | partial | decor / exam props |
| **West room walls** | x=0–2, y=4–11 | Buildings | yes | — | |
| **Corridor narrowing** | (14–16, 9) | Front density | partial | Front | narrow for 2 NPC |
| **Lamps / nightstands** | on Front at beds | Front | usually no | **yes** | `(19,5)`, `(18,5)` — don't treat as walk target |
| **AlwaysFront decor** | DayTiles beds (14,1), (21,1) | AlwaysFront | no | yes | night/day swap |
| **Paths NPC routes** | y=6, x=13–16 (2185) | Paths | no | — | Maru/vanilla NPC paths |

---

## Safe staging zones

### `hospital_entrance_dialogue`

| Field | Value |
|-------|-------|
| **Range** | `(8,18)–(12,19)` |
| **Harvey** | `(10,18)` or `(10,14)` face **2** |
| **Farmer** | `(10,19)` face **0** |
| **Viewport** | `(10, 19)` |
| **Scenes** | E5 storm entry, E6, Birthday welcome, generic «вошли в клинику» |
| **Risks** | `(10,20)` warp south; Harvey `(10,18)` Front overlay |

### `hospital_counter_dialogue`

| Field | Value |
|-------|-------|
| **Range** | `(1,14)–(14,19)` open floor; counter **(5–7,16)** blocked |
| **Harvey** | `(10,14)`–`(10,16)` or `(3,15)` phone |
| **Farmer** | `(10,15)`–`(10,19)` or `(8,16)` |
| **Viewport** | `(8, 16)` / `(10, 16)` |
| **Scenes** | reception, phone rescue (`eventRescueOperation` start), paperwork |
| **Risks** | counter tiles; `(1,15)` narrow; `(3,15)` Front |

### `hospital_bed_left_side`

| Field | Value |
|-------|-------|
| **Range** | `(18,5)–(19,6)` |
| **Harvey** | **`(19, 5)`** face **1** |
| **Farmer** | on bed `(20,5)` ‡ or standing `(18,5)` Front |
| **Viewport** | `(19, 5)` close-up or `(14, 6)` wide |
| **Scenes** | major rescue dialogue at bedside |
| **Risks** | `(18,5)` Front; don't block `(20,6)` approach |

### `hospital_bed_right_side`

| Field | Value |
|-------|-------|
| **Range** | `(20,6)–(21,6)` — **(21,5) blocked** |
| **Harvey** | `(20, 6)` face **0** |
| **Farmer** | bed `(20,5)` ‡ |
| **Viewport** | `(20, 5)` — tight; prefer `(14,6)` |
| **Scenes** | alternate angle bedside — **needs in-game check** |
| **Risks** | `(21,5)` bed tile |

### `hospital_bed_farmer_lie`

| Field | Value |
|-------|-------|
| **Range** | tile **`(20, 5)`** only (major) or `(14,6)` animate lie (minor) |
| **Harvey** | `(19,5)` or `(15,6)` |
| **Farmer** | **`(20,5)`** + `ignoreCollisions` + **`positionOffset 32 -52`** + **`animate … 4 5`** + **face 2** |
| **Viewport** | **`(14, 6)`** wide (verified mine rescue) |
| **Scenes** | mine rescue, hospitalization, exhaustion aftermath |
| **Risks** | without ignoreCollisions = **Broken**; reset offset before move |

### `hospital_exam_area`

| Field | Value |
|-------|-------|
| **Range** | West **`(3,5)–(8,11)`**; core **`(4,5)–(6,10)`** |
| **Harvey** | **`(4, 5)`** face **1**; exam **`(6, 10)`** with showFrame |
| **Farmer** | **`(4, 6)`**, **`(6, 10)`**, seated **`(5, 5)`** |
| **Viewport** | `(5, 8)` / `(6, 10)` |
| **Scenes** | FirstTreatment, E2 exam, rib check animate |
| **Risks** | **`(5,9)` DOOR — never farmer setup**; `(5,4)` blocked north |

### `hospital_private_talk`

| Field | Value |
|-------|-------|
| **Range** | **`(13,7)–(17,9)`** corridor or **`(9,15)–(11,16)`** reception quiet |
| **Harvey** | **`(15, 8)`** face **1** (NightCrisis verified) |
| **Farmer** | **`(14, 8)`** or **`(16, 8)`** face **3** |
| **Viewport** | `(15, 8)`; E6: `ambientLight 100 90 75` |
| **Scenes** | NightCrisis, E6 evening talk, treatment review |
| **Risks** | low; avoid `(14,9)` Front cluster |

### `hospital_exit_walkout`

| Field | Value |
|-------|-------|
| **Range** | `(9,18)–(11,19)` facing south exit |
| **Harvey** | `(10,14)` escort or off-screen |
| **Farmer** | **`(10, 19)`** face **0** or **2** |
| **Viewport** | `(10, 19)` then fade |
| **Scenes** | end hospital stay; E5 near exit `(10,19)` OK per audit |
| **Risks** | **`(10,20)` warp** — use fade before walk south |

---

## Risk zones

| Coords | Risk | Why | Avoid |
|--------|------|-----|-------|
| **(20,5), (21,5)** | bed Buildings | lying tile only with ignoreCollisions | setup walk NPC |
| **(5,9), (10,13), (5,13), (9–10,5)** | Action Door | Buildings | setup, doAction without intent |
| **(10,20)** | warp OOB | → Town | any setup |
| **(9–10,1)** | warp | → HarveyRoom | block with NPC |
| **(5–7,16)** | counter | HospitalShop | path through counter |
| **(19,4)–(22,4)** | headboard | Buildings | walk |
| **(11–15,15–17)** | lower beds | wrong ward vs 20,5 | mod bed confusion |
| **(1,5), (3,5), (18,5), (19,5)** | Front overlay | pass but visual/move quirks | long advancedMove |
| **(14–16,9)** | narrow corridor | Front density | two NPC + move |
| **(19,14), (22,14), (1,15)** | narrow (TMX) | ≤1 neighbor | advancedMove pairs |
| **(6,0)–(23,0)** | map edge | north void | walk north |
| **(3,15)** | phone + Front | rescue staging | don't confuse with warp |
| **Farmer (5,9)** in FirstTreatment | **Broken** in audit | door tile | new scenes: use `(4,6)` |

---

## Movement templates

Команды — **шаблоны**; координаты только из таблицы «Confirmed safe».

| Name | Commands | When to use | Notes |
|------|----------|-------------|-------|
| **Major bed staging** | `ignoreCollisions farmer/` → `warp farmer 20 5` → `faceDirection farmer 2` → `positionOffset farmer 32 -52` → `animate farmer true true 10000 4 5` | Mine rescue fin, hospitalization | Reset offset before later move |
| **Harvey bedside static** | `warp Harvey 19 5` → `faceDirection Harvey 1` | Любая major bed сцена | No move onto (20,5) |
| **Minor exam staging** | `warp farmer 14 6` → `warp Harvey 15 6` → `faceDirection Harvey 3` → `viewport 14 6` | Minor rescue, weak farmer seated | No offset |
| **West exam approach** | `warp Harvey 4 5` → `warp farmer 4 6` → `move Harvey 1 0 0` (optional) | FirstTreatment | **Not** via (5,9) |
| **Farmer to couch** | `move farmer 0 -4 3` from south door path — **legacy FirstTreatment** | needs review | Prefer warp `(6,10)` |
| **E2 exam** | farmer moves to final tile → `faceDirection 2` → `showFrame farmer 107` | Insistent exam | final tile по §Confirmed safe |
| **Reception short** | `warp farmer 10 19` → `move farmer 0 -4 3` → `(10,15)` | E5 storm | `(10,19)` safe |
| **E5/E6 cluster** | `move Harvey 0 -2/−3 0` between `(10,18)–(10,15)`; `move farmer 0 -3/−4` | Storm / evening talk | keep ≤4 tiles |
| **NightCrisis** | `setup Harvey 15 8` → `move Harvey -2 0 3` (audit: (13,8) **needs check** blocked?) | Dating crisis | verify (13,8) in-game |
| **Phone rescue** | `changeLocation Hospital` → `warp Harvey 3 15` → `viewport 8 15` | eventRescueOperation | Lewis off-screen |
| **Exit to Town** | `globalFade` → `viewport -1000 -1000` → `end` / warp Town via game | Leave hospital | don't walk onto (10,20) |
| **From Town entry** | fade in → `warp farmer 10 19` → `faceDirection 0` | Post changeLocation from Town | match Town (36,55) door |

---

## Animation templates

| Name | Commands | Required positioning | Notes |
|------|----------|----------------------|-------|
| **Farmer lying (major bed)** | `ignoreCollisions farmer/` → `warp farmer 20 5` → `positionOffset farmer 32 -52` → `animate farmer true true <ms> 4 5` → `faceDirection 2` | Harvey `(19,5)` | **Confirmed** mine rescue |
| **Farmer lying (minor)** | `warp farmer 14 6` → `positionOffset farmer 0 10` → `animate farmer false true 1000 4 5` | Harvey `(15,6)` | Some collapse events — offset **needs in-game check** |
| **Farmer seated exam** | `move` → `stopAnimation` → `faceDirection farmer 2` → `showFrame farmer 107` | `(5,4)` стул / `(6,10)` couch | face **2** (лицом к камере) |
| **Harvey stethoscope** | `animate Harvey false true 1000 22 20 21 20` (+ variants) | Harvey `(4,5)`–`(4,6)` | FirstTreatment / rib exam |
| **Harvey listen / exam frame** | `showFrame Harvey 107` (E2 pattern) | `(6,10)` area | **needs check** exact frame in runtime |
| **Farmer weak / tremble** | `startJittering` / `animate farmer false true 3000 5 4` | `(10,19)` or `(14,6)` | E5 jitter; fear animate |
| **Harvey rush** | `speed Harvey 5` → short `move` 1–3 tiles | `(20,6)`→`(19,5)` | late-night `(36,56)`→… in Town first |
| **End lying state** | `stopAnimation farmer` → `positionOffset farmer 0 0` → `animate false` | before dialogue continue or exit | **Anti-pattern** if skipped |
| **Ambient** | `ambientLight 180 180 180` (day) / `100 90 75` (E6 eve) / `Hospital_Ambient` music | zone-specific | E5/E6 mood |
| **Fade handoff** | `globalFade` → `viewport -1000 -1000` → `changeLocation` / `fade false` | multi-location | Always re-warp actors after |

---

## Anti-patterns (Hospital)

| Anti-pattern | Why | Instead |
|--------------|-----|---------|
| Setup on **lamp / nightstand Front** `(18–19,5)` | pass but wrong anchor; sprite overlap | Harvey `(19,5)` floor; farmer bed pattern |
| **Block exit** `(10,19–20)` | warp to Town; player stuck | farmer `(10,19)` max; fade before south |
| **Walk Harvey through counter** `(5–7,16)` | Buildings blocked | south corridor `(10,15–19)` |
| **Large positionOffset** as walk substitute | drift, broken collision | short `move` on pass tiles; offset only for bed |
| **Long move** through `(14,9)` / west choke | Front furniture | warp to zone; ≤4 tile moves |
| **Farmer lying animate** without reset mid-scene | frozen on bed while standing dialogue | `stopAnimation` + `positionOffset 0 0` |
| **Setup on `(5,9)`** «exam couch» | Door Buildings — audit **Broken** | `(4,6)` / `(6,10)` |
| **Move onto `(20,5)`** without ignoreCollisions | Buildings bed | full lying pattern |
| **Confuse lower ward** `(11–15,15–17)` with major bed | wrong visuals / C# bed | always `(20,5)` for mod hospitalization |
| **`doAction (10,13)`** without intent | internal door Buildings | narrative-only; needs in-game check |
| **Harvey `(1,5)`** long routes | Front + edge | warp `(4,5)` for west exam |
| **E6-style farmer from (0,0)** | audit Broken path | explicit warp `(10,19)` first |

---

## Event checklist for Hospital

Использовать перед правкой **любого** CP-события на `Data/Events/Hospital`:

- [ ] Target = `Hospital`; все coords после `changeLocation Hospital` re-setup.
- [ ] Farmer **не** на `(5,9)`, `(10,13)`, `(9–10,5)` — door Buildings.
- [ ] Major bed `(20,5)`: **`ignoreCollisions` + `positionOffset 32 -52` + lying animate**; Harvey **`(19,5)`**, not `(20,5)`.
- [ ] Minor/seated: **`(14,6)` / `(15,6)`** — open floor, no offset unless script requires.
- [ ] West exam: farmer **`(4,6)` or `(6,10)`**, Harvey **`(4,5)`** — not door `(5,9)`.
- [ ] Reception: farmer **`(10,19)`** enter; moves ≤4 tiles to `(10,15–16)`.
- [ ] Exit: **`(10,19)`** OK; **не** `(10,20)` warp tile.
- [ ] **`positionOffset` сброшен** (`0 0`) перед `move` после lying.
- [ ] **`stopAnimation` / end animate** если сцена продолжается standing.
- [ ] Viewport set: **`(14,6)`** bed wide; **`(10,19)`** entrance; **`(15,8)`** private; **`(6,10)`** exam.
- [ ] Нет **long advancedMove** через `(14,9)` furniture choke.
- [ ] Нет setup на **counter `(5–7,16)`** или **headboard (19,4)+**.
- [ ] **`doAction` на door** только если narrativ открытия; tile Buildings — expect **needs in-game check**.
- [ ] Phone scene: Harvey **`(3,15)`**, viewport **`(8,15)`**; Lewis **`(1000,1000)`**.
- [ ] Maru/temp NPC: zone **x=17–21, y=5–7**, not doors.
- [ ] DayTiles/NightTiles: кровати Front `(14,1)`, `(21,1)` — verify evening scenes.
- [ ] After Town entry: farmer **`(10,19)`** aligns with Town door **`(36,55)`**.
- [ ] Manually verified events (**MedicalCheck, EmergencyCare, …**) — **do not touch** without explicit request.
- [ ] New coords marked **`needs check`** until in-game + TMX pass confirmed.
- [ ] SVE runtime Load: spot-check after mod update.

---

## Quick Hospital rules

1. **Major bed = `(20,5)` farmer + `(19,5)` Harvey** — единый стандарт мода и C#.
2. **Minor bed = `(14,6)` + `(15,6)`** — seated/weak без major offset.
3. **`(5,9)` — дверь, не кушетка** (FirstTreatment legacy — Broken in audit).
4. **`ignoreCollisions` + `positionOffset 32 -52` + `animate 4 5`** — единственный путь на major bed.
5. **Viewport `(14,6)`** — лучший wide shot палаты B.
6. **Не путать** палату B `(20,5)` с нижним залом `(11–15, 15–17)`.
7. **Стойка `(5–7,16)`** — не ставить NPC; оставить проход с юга.
8. **Entrance `(10,19)`**; exit через fade, не walk на `(10,20)`.
9. **Harvey office `(9–10,5)`** — door only; warp HarveyRoom `(9–10,1)`.
10. **West exam `(4,5)–(6,10)`** — стетоскоп; farmer сидит: `faceDirection 2` + `showFrame farmer 107`.
11. **NightCrisis / private `(15,8)`** — pass OK in TMX.
12. **E5/E6 `(10,18–19)`** — Front on Harvey — needs visual check.
13. **Short moves only** (≤4 tiles) in corridors; no advancedMove through furniture.
14. **Reset offset** before movement after lying.
15. **Re-warp all actors** after every `changeLocation`.
16. **Town door `(36,55)` ↔ Hospital `(10,19)`** — keep narrative consistency.
17. **Manually verified hospital events** — do not touch.
18. **New scenes:** copy templates from §Movement/Animation, not invent coords.
19. **Lamps/nightstands** — Front at `(18–19,5)`; aesthetic only.
20. **Runtime SVE** — final check in-game for Front/DayTiles.

---

## Сидение farmer (showFrame)

### Правило (in-game)

**Сидение:** `showFrame farmer 107` при **`faceDirection farmer 2`** (лицом к камере).

### Предупреждение

`showFrame` меняет кадр, но **не перемещает** actor. Перед `showFrame farmer 107` farmer уже должна стоять на **final sitting tile**.

### Порядок посадки (west exam, стул `(5,4)`)

1. farmer доходит до стула явными `move` (из `(6,6)`: `move -1 0 3/` → **`move 0 -2 0/`** → `(5,4)`);
2. `stopAnimation farmer`;
3. **`faceDirection farmer 2`**;
4. **`showFrame farmer 107`**;
5. `pause 300–500`.

Harvey exam: из `(4,6)` → **`move 1 0 0/`** → **`move 0 -1 0/`** → `(5,5)`; **`faceDirection Harvey 0`** → `stopAnimation` → `animate … 22 20 21 20`.

### Антипаттерн

```text
advancedMove farmer false -1 0 0 -1/
showFrame farmer 107/
```

Без `stopAnimation` и **`faceDirection farmer 2`** — farmer может зависнуть в ходьбе.

См. [`cp-event-authoring-rules.md`](../cp-event-authoring-rules.md).

---

**Метод:** TMX `tmpMap/sve/maps/Locations/Hospital.tmx` + [`Hospital_event_placement_analysis.md`](../../tmpMap/Hospital_event_placement_analysis.md).  
**Не учтено:** runtime `.tbin`, engine Front collision, exact sprite overlap.  
**Генератор:** `tmpMap/generate_split_map_passports.py` **не перезаписывает** этот файл (hand-maintained).
