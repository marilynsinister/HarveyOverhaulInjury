# Map Passport: Farm

## 1. Metadata

- **LocationName:** Farm
- **Map asset:** `Maps/Farm` (runtime; layout зависит от save)
- **Map file:** **Needs map export / not available in repository** — отдельный TMX бессмысленен как «единственный источник правды»
- **Source:** variable / vanilla + custom layouts (+ modded farm maps через CP/SMAPI)
- **Status:** **partial-by-design**
- **Main risk:** different farm layouts and player-placed objects
- **Safe assumption:** NPC visitors appear near farmhouse door (visitor spawn area)

**Used by events (HarveyOverhaul CP):**

- `eventHarveyFirstVisit`, `eventHarveySecondVisit` — `eventsCare.json`
- `eventHarveyFirstWalk`, `acceptWalk` (fork), `eventHarveyCheckHealthFarmer`, `eventHarveyCheckFarmerOutsideAfter22`, `eventHarveyMorningCheckup`, `eventHarveyStormComfortFarm` — `events.json`

**Связанные документы:** [`map-passports.md`](../map-passports.md) · [`cp-event-authoring-rules.md`](../../EventPatterns/cp-event-authoring-rules.md) §12.4 · [`events-map-audit-plan.md`](../events-map-audit-plan.md)

---

## 2. Почему Farm нельзя анализировать как обычную карту

Farm в Stardew Valley — **не одна фиксированная карта**, а семейство runtime-карт с общим именем локации `Farm`.

### Разные базовые layouts

У игрока может быть:

| Layout | Особенность для событий |
|--------|-------------------------|
| **Standard** | Классический двор перед FarmHouse; baseline audit `(64,15–18)` |
| **Riverland** | Вода, мосты, смещённые зоны проходимости |
| **Forest** | Деревья и лесной декор ближе к дому |
| **Hill-top** | Перепады высот, другие координаты farmhouse |
| **Wilderness** | Пещера, река, иной рельеф |
| **Four Corners** | Четыре «угла», другая геометрия двора |
| **Beach** | Песок, вода у краёв |
| **Meadowlands** | Луга, иной фон |
| **Modded / custom** | SVE Grandpa's Farm, IF2R, CP farm replacements — warps, размер, объекты |

### Player-placed и runtime-изменения

- Игрок ставит **здания** (курицник, сарай, силос, хлев, купальня, обелиски).
- На земле — **заборы, факелы, декор, деревья, кусты, кропы**.
- **Shipping bin**, **pet bowl**, **mailbox**, **greenhouse** (после unlock) меняют проходимость и кадр.
- **Pond** (если построен) и **farm cave** — фиксированные точки на одной ферме, отсутствуют на другой.

### Следствие для CP-событий

Координаты, безопасные на **Standard Farm**, на другом layout могут оказаться:

- водой или `Passable=F`;
- внутри Buildings / player building;
- за забором или в кустах;
- вне viewport или за краем walkable area.

**Поэтому нельзя полагаться на конкретный объект карты** (пруд, дерево, shipping bin, теплица, дорожка), **кроме зоны farmhouse / visitor spawn** — единственной области, которую игра и мод трактуют как «дом + посетители у двери».

> TMX Farm в репозитории **отсутствует намеренно**: один экспорт не покрывает все layouts. Паспорт описывает **принципы постановки**, а не таблицу тайлов.

---

## 3. Safe Farm staging principle

Главное правило HarveyOverhaul для универсальных Farm-событий:

### Visitor zone у farmhouse

1. **Все универсальные Farm-события** должны начинаться и происходить **около farmhouse door / visitor spawn area**.
2. **Harvey** — как **посетитель у двери** (южнее или сбоку от порога, не на warp-тайле двери).
3. **Farmer** — **рядом с дверью** или «только что вышел из дома» (соседний проходимый тайл).
4. **Movement** — **короткий: 1–3 тайла максимум**; после каждого шага — `faceDirection`.
5. **Не вести** персонажей через двор, к лесу, к пруду или к краю карты.
6. **Не ставить** сцену у объектов, которые **меняются между saves**:
   - pond, greenhouse, shipping bin, pet bowl, cave entrance;
   - mailbox как опорная точка камеры;
   - crops, scarecrows, paths, player buildings;
   - произвольные «красивые» деревья или декор.

### Куда переносить сцены вместо Farm

| Нужный тон | Лучшая локация |
|------------|----------------|
| Прогулка, романтика, apothecary | **Forest**, **Town**, **Mountain**, **Beach** |
| Медицинский осмотр, лечение | **Hospital** (или короткий диалог у **doorway**, затем fade → Hospital) |
| Storm comfort с укрытием | У **porch/door** на Farm **или** `changeLocation` → Hospital / Saloon / Forest |
| Длинный маршрут NPC | **Не Farm** |

### Baseline Standard (только справочно, не универсальный стандарт)

Текущие проверенные CP-события используют **Standard Farm** зону:

| Роль | Пример coords (Standard) | Направление |
|------|--------------------------|-------------|
| viewport | `(64, 15)` | на вход farmhouse |
| farmer | `(64, 16)` | `faceDirection 2` (south) |
| Harvey | `(64, 17)` или `(64, 18)` | `faceDirection 0` (north), visitor tile |

Эти числа **не гарантированы** на Riverland / Beach / modded farm — их можно использовать только как **референс уже проверенного Standard layout**, не как шаблон для новых событий без теста.

---

## 4. Safe staging zones

### `farm_house_door_visitor_area`

**Назначение:**

- первый контакт (`eventHarveyFirstVisit`);
- повторный визит (`eventHarveySecondVisit`);
- предложение прогулки (`eventHarveyFirstWalk` — только intro у двери);
- ночная проверка (`eventHarveyCheckFarmerOutsideAfter22`);
- утренний осмотр у порога (`eventHarveyMorningCheckup` — outdoor staging);
- короткая сцена заботы (`eventHarveyCheckHealthFarmer` — до fade в Hospital);
- warning после обморока в городе;
- storm comfort intro (`eventHarveyStormComfortFarm` — до fork в Hospital).

| Поле | Значение |
|------|----------|
| **Coordinates/range** | **needs confirmation per layout** — использовать visitor spawn near farmhouse door; на Standard: ~(64,15–18) |
| **Harvey** | visitor tile **южнее/рядом** с дверью; смотрит на farmer (`faceDirection 0` или боком); **не на тайле двери** |
| **Farmer** | adjacent tile у порога; часто `(door_y + 1)`; `faceDirection 2` если Harvey с юга |
| **Recommended movement** | Harvey `move 0 -1 0` (1 шаг к farmer) или без move; **не advancedMove** |
| **Recommended camera** | viewport на **farmhouse entrance** (`continue X Y` ≈ door row); центр кадра — дверь + оба NPC |
| **Risk** | **low**, если сцена **не выходит** за 2–3 тайла от порога |

### `farm_house_threshold`

**Назначение:**

- farmer «выходит из дома» (визуально — уже стоит на outdoor tile у двери);
- Harvey ждёт у двери как гость;
- короткий диалог без обхода двора.

| Поле | Значение |
|------|----------|
| **Coordinates/range** | 1–2 тайла непосредственно **перед** FarmHouse door (outdoor Back, без Buildings) |
| **Harvey** | не перекрывает дверь — игрок должен иметь визуальный «проход» в дом |
| **Farmer** | на пороге/клетке южнее двери |
| **Movement** | 0–1 шаг; optional `playSound doorClose` без warp |
| **Camera** | тот же viewport, что visitor area |
| **Risk** | **low–medium** — проверить, что tile не occupied player building |

### `farm_do_not_use_variable_area`

**Назначение:** явная **запретная зона** для универсальных событий — всё **дальше ~3 тайлов** от farmhouse door.

| Категория | Почему risky |
|-----------|--------------|
| Центр/край фермы | layout-specific проходимость |
| Pond / river tiles | water на части layouts |
| Greenhouse / buildings | position varies |
| Shipping bin / mailbox | classic trap для `(64,x)`-style coords на non-Standard |
| Crops / paths / decor | player-placed |
| Forest warp side / bus stop side | далеко от door, NPC path conflicts |

**Правило:** любая сцена здесь — только после **ручной проверки на каждом целевом layout** (Standard + минимум 1 альтернативный + modded, если поддерживаете).

---

## 5. Rules for Farm events

Обязательные правила для **новых и пересмотренных** CP-событий на `Data/Events/Farm`:

1. **Не использовать длинные маршруты** на Farm (`advancedMove` через двор, `move` > 3 тайлов суммарно).
2. **Не использовать координаты в дальних частях** фермы для универсальных событий.
3. **Не привязывать сцену к объектам** фермы, кроме **farmhouse door / porch / visitor tiles**.
4. **Не ставить `temporaryAnimatedSprite`** на землю фермы без проверки layout (blanket, мед. сумка, и т.д.).
5. **Не использовать picnic blanket** на Farm как универсальную сцену без multi-layout QA.
6. **Прогулка / романтика** — переносить в **Forest**, **Town**, **Mountain**, **Beach** (`acceptWalk` → `changeLocation Forest` — правильный паттерн).
7. **Медицинский осмотр** — короткий диалог у **doorway**, затем **`globalFade` + `changeLocation Hospital`** (`eventHarveyCheckHealthFarmer`, storm fork).
8. **Storm comfort на Farm** — только у **door/porch**; не уводить к «случайному дереву» на дворе; fork в Hospital допустим.
9. **Не setup NPC на warp-тайле** FarmHouse door (игрок не должен «стоять в двери»).
10. **Не блокировать** shipping bin / pet bowl / mailbox — они могут быть в кадре, но не как anchor координат.
11. **Viewport обязателен** (`continue X Y` или `viewport`) — привязан к **door area**, не к центру всей карты Farm.
12. **После `changeLocation`** — полный re-setup NPC на новой карте.
13. **Modded farms:** всё за пределами visitor area считать **risky until proven**.

---

## 6. Movement guidance

### Good

| Actor | Паттерн | Команды | Notes |
|-------|---------|---------|-------|
| Harvey | Подойти к farmer на 1–2 тайла | `move Harvey 0 -1 0` | как в `eventHarveyCheckFarmerOutsideAfter22`, storm comfort |
| Harvey | Статично ждать у двери | setup / `continue` only | first/second visit |
| Farmer | 1 шаг в сторону / назад | `move farmer 0 1 0` | освободить кадр двери |
| Оба | Только `faceDirection` | после speak / emote | обязательно для visitor staging |
| Harvey | Не перекрывать дверь | Harvey на `(door_x, door_y+2)` | visitor tile южнее порога |

### Bad

| Паттерн | Почему |
|---------|--------|
| Harvey идёт **через весь двор** | player buildings / fences |
| Farmer идёт к **произвольной точке** на ферме | layout-specific |
| Сцена у **shipping bin / pond / tree / crops** | объект может отсутствовать или быть blocked |
| NPC на tile, где игрок мог поставить **building** | runtime collision |
| **`advancedMove`** на Farm | почти всегда избыточен и опасен |
| Длинный path к **Forest warp** без `changeLocation` | используйте fork → Forest event |

---

## 7. Events using Farm

| Event ID | File | Status | Notes |
|----------|------|--------|-------|
| `eventHarveyFirstVisit` | `eventsCare.json` | **manually-verified-do-not-touch** | Visitor у двери; Standard `(64,15–18)`; onboarding после BusStop |
| `eventHarveySecondVisit` | `eventsCare.json` | **manually-verified-do-not-touch** | Та же door zone; витаминный чай; short dialogue |
| `eventHarveyFirstWalk` | `events.json` | **manually-verified-do-not-touch** | Intro у двери; fork `acceptWalk` → Forest — **не** вести прогулку по Farm |
| `acceptWalk` | `events.json` (fork) | **manually-verified-do-not-touch** | Fork-подсобытие; старт с Farm door → **`changeLocation Forest`** |
| `eventHarveyCheckHealthFarmer` | `events.json` | **manually-verified-do-not-touch** | Осмотр у двери → **`changeLocation Hospital`** |
| `eventHarveyCheckFarmerOutsideAfter22` | `events.json` | **manually-verified-do-not-touch** | Ночной visitor; Harvey `move 0 -1`; topic chain после Town pass-out |
| `eventHarveyMorningCheckup` | `events.json` | **manually-verified-do-not-touch** | Dating; утро у порога `(64,17)` Harvey; emote + pause; короткая сцена |
| `eventHarveyStormComfortFarm` | `events.json` | **manually-verified-do-not-touch** | Storm у door; короткий move; fork → **Hospital** (не у деревьев двора) |

**Не в активном CP (справочно):** `MyMod_HarveyUrgentFarmVisit` в `events_for_mode_new_formatted.json` — **не подключён** к `content.json`.

**Исключены из map-audit правок:** все события выше помечены в [`events-map-audit-plan.md`](../events-map-audit-plan.md) как «уже проверено вручную».

---

## 8. Quick Farm rules

1. **Farm-сцены держать у farmhouse door** — visitor area, не центр карты.
2. **Не использовать дальние координаты Farm** без отдельной multi-layout проверки.
3. **Visitor positioning у двери** — базовая безопасная постановка Harvey.
4. **Farmer — соседний тайл**, не warp door tile.
5. **Movement ≤ 3 тайла** суммарно; предпочитать static + `faceDirection`.
6. **Не anchor на shipping bin, pond, greenhouse, crops, mailbox.**
7. **Прогулка / романтика** — `changeLocation` в Forest/Town/Beach/Mountain.
8. **Медицина / storm fork** — doorway на Farm → fade → Hospital.
9. **Custom / modded farm maps:** всё за visitor area = **risky**.
10. **Не выдумывать coords** из TMX — его нет; Standard `(64,15–18)` только как verified reference.
11. **Viewport** центрировать на **farmhouse entrance**, не на всю Farm.
12. **После changeLocation** — re-warp всех actors.
13. **Новые универсальные события** на Farm — только по принципу §3, не по «красивой точке» на дворе.
14. **Picnic blanket / ground sprites** на Farm — только с layout QA.
15. **Пересмотр legacy coords** — допустим только с тестом Standard + alt layout; иначе **do not touch** verified events.

---

**Метод:** принципиальный паспорт вариативной карты; без TMX.  
**Baseline coords:** из script preview активных CP (`64 15` / `farmer 64 16` / `Harvey 64 17–18`) — Standard Farm, manually verified.  
**Не учтено:** конкретные mod farm CP, runtime building collision, SVE IF2R warp side effects.
