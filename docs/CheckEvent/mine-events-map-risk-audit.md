# Риски шахтных событий

Отдельный аудит событий **Mine / SkullCave / Volcano** с фокусом на стабильность фиксированных координат, цепочку запуска и расхождение «где умер игрок» vs «где играет cutscene».

**Источники:** CP `eventsMineRescue.json`, `eventsCare.json`, `events.json`, `triggersCare.json`; C# `PassOutHandler.cs`, `PlayerEventHandler.cs`; TMX `tmpMap/Mine.tmx` (SVE), `tmpMap/vanilla/maps/SkullCave.tmx`; [`map-passports.md`](map-passports.md), [`Mine_event_placement_analysis.md`](../../tmpMap/Mine_event_placement_analysis.md), [`events-coordinate-audit.md`](events-coordinate-audit.md).

**События и код не изменялись** — только анализ.

---

## Ключевой вывод

| Локация в игре | Используется в CP-событиях? | Риск координат |
|----------------|----------------------------|----------------|
| **`Mine`** (вход, 77×20, SVE Load) | **Да** — все rescue/interception/storm | **Средний** — карта фиксированная, но SVE/варианты входа меняют окружение |
| **`MineShaft`** (процедурные этажи) | **Нет** — события не патчатся сюда | **N/A** — C# переносит игрока на `Mine` перед cutscene |
| **`SkullCave`** (вход, 16×10, vanilla) | **Да** — prevention | **Низкий** по координатам; **высокий** по триггеру (см. ниже) |
| **`VolcanoDungeon`** | **Нет CP-событий** | **Высокий** на уровне C#-моста: death/minor-rescue частично не покрыты |

Процедурные этажи **не ломают** текущие скрипты напрямую: все cutscene используют **`Data/Events/Mine`** или **`Data/Events/SkullCave`** — отдельные статичные карты входа.

---

## Сводная таблица

| Event ID | Current location | Trigger source | Fixed coords? | Risk level | Recommendation |
|----------|------------------|----------------|---------------|------------|----------------|
| `eventHarveyMineRescue` | Mine → Hospital | C# `TriggerMineRescueEvents` (DayStarted) → `BeginMineRescueWarp` → `warpFarmer Mine 17 7` → `startEvent` в `OnPlayerWarped` | Да: (17,7), (17,10); Hospital (20,5)+(offset) | **Medium** | Оставить с **обязательным pre-warp** на (17,7); aftermath в Hospital — OK с `ignoreCollisions` |
| `eventHarveyMineRescueDating` | Mine → Hospital | То же, ветка dating/married | Да — зеркало major | **Medium** | То же |
| `eventHarveyMinorMineRescue` | Mine → Hospital | C# `TryTriggerMinorMineRescue` при входе в Mine/Volcano (низкое HP/stamina + травма); warp `Mine 17 7` если не в Mine | Да: (17,7), (17,10); Hospital (14,6) | **Medium** | Pre-warp обязателен; при триггере из **Volcano** — рассмотреть короткий fade сразу в Hospital (нарратив «вытащил с входа») |
| `eventHarveyMineInterception` | Mine (без смены локации) | SpaceCore `triggerHarveyMineWarning` (`LocationChanged`, `PLAYER_LOCATION_NAME Current Mine`) | Да: setup (17,7)/(17,10); exit `move 0 3` | **Low–Medium** | Оставить; setup сбрасывает позицию; не вести NPC на **y ≥ 11** (сейчас exit до y=10 — OK) |
| `eventHarveySkullCavePrevention` | SkullCave | SpaceCore: `triggerLocationReactionSkullCaveExit` (SkullCave); **также** `triggerHarveySkullCaveWarning` (Mine **или** SkullCave) | Да: (5,5), (7,7); move Harvey `0 -2`, `-1 0` | **High** (триггер) / **Low** (координаты на SkullCave) | **Исправить триггер:** SkullCave-событие только при `Current SkullCave`; координаты на SkullCave оставить |
| `eventHarveyStormComfortMine` | Mine → Town | CP **location entry** `Data/Events/Mine` (шторм + friendship + random); **не** C# warp | Да: farmer (15,5); Harvey warp (18,13); move; Town (72,22) | **Medium** | Pre-warp farmer на (15,5) при старте уже в setup; сократить вертикальный `move` у южного края/warp (18,14→Summit) или fade раньше |
| *(Volcano — нет event ID)* | — | C#: minor rescue / mine warning / dirty wound; **нет** `Data/Events/Volcano*` | — | **High** (мост) | Death HP=0 в Volcano **может не** выставить `NeedsMineRescueEvent`; minor rescue warp'ит на Mine — отдельная задача для C# |

---

## Цепочки запуска (как это работает сейчас)

### Major mine rescue (`eventHarveyMineRescue` / `Dating`)

```mermaid
sequenceDiagram
    participant MS as MineShaft/Volcano/Mine
    participant C# as PassOutHandler
    participant Map as Mine entrance
    participant CP as CP event script
    participant H as Hospital

    MS->>C#: HP<=0 (только loc.Contains "Mine")
    C#->>C#: NeedsMineRescueEvent=true
    Note over C#: Следующее утро DayStarted
    C#->>Map: warpFarmer(17,7)
    Map->>CP: startEvent (OnPlayerWarped)
    CP->>CP: farmer 17,7; Harvey warp 17,10; move -2
    CP->>H: globalFade + changeLocation Hospital
```

- Смерть фиксируется в **`MineShaft`**, **`Mine`**, локациях с `"Mine"` в имени — **не** в `VolcanoDungeon` (см. раздел High-risk).
- Cutscene **всегда** на карте **`Mine` (вход)**, не на этаже, где был бой — это **намеренная** стабилизация координат.

### Minor mine rescue

- Триггер: вход в **`Mine`** или **`VolcanoDungeon`** (`IsPlayerInMineOrVolcano`) + dating + травма + низкое HP/stamina.
- Если игрок не в `Mine` → `PendingMinorMineRescueEventId` + **`warpFarmer Mine 17 7`** → `startEvent` в `OnPlayerWarped`.
- Сцена короткая: без fade на шахте → сразу `changeLocation Hospital` (14,6).

### Interception / Skull prevention

- **Interception:** `LocationChanged` + локация **`Mine`** + injury buffs → `SpaceCore_PlayEvent eventHarveyMineInterception`.
- **Skull prevention:** два триггера; опасный — `triggerHarveySkullCaveWarning` с условием **`Mine SkullCave`** (OR) и событием в **`Data/Events/SkullCave`**.

### Storm comfort mine

- Обычный **CP entry event** на `Mine`: срабатывает при **входе** в локацию при шторме (preconditions в ключе).
- Setup **`farmer 15 5 2`** задаёт стартовую позицию независимо от warp-точки входа.
- Harvey: `warp 18 13` → длинный `move 0 -8` → короткий диалог → fade → **Town**.

---

## Стабильность координат на карте Mine (SVE TMX)

Проверенная зона ( [`Mine_event_placement_analysis.md`](../../tmpMap/Mine_event_placement_analysis.md) ):

| Координата | Использование | TMX | Комментарий |
|------------|---------------|-----|-------------|
| **(17, 7)** | C# warp + все rescue/interception setup | Back OK, Buildings=0 | **Якорная** точка; совпадает с C# и CP |
| **(17, 10)** | Harvey spawn / setup | Back OK | Коридор x=17, y=6–10 |
| **(17, 8)** | Harvey после `move 0 -2` | Back OK | Единственный move в rescue |
| **(15, 5)** | Storm comfort farmer | Back OK | Северная платформа, без Buildings |
| **(18, 13)** | Storm comfort Harvey warp | Back OK; рядом warp **(18,14)→Custom_AdventurerSummit** | Не ставить длинные move на **юг** (y≥11 — Front/обрыв) |
| **(20, 5)** Hospital | Palata после rescue | Buildings (кровать) | OK с `ignoreCollisions` + `positionOffset` + lying frame |

**Не зависит от уровня шахты:** координаты относятся только к локации `Mine`, не к `MineShaft` level N.

**Зависит от мод-патчей:**

- SVE **Load** `Mine.tbin` — базовая карта для audit.
- `OriginalMinesEntrance` — меняет связку Mountain/Summit ↔ Mine; warp **(18,14)** ведёт на **Custom_AdventurerSummit**, не vanilla Mountain ([`maps-and-tilesets-inventory.md`](maps-and-tilesets-inventory.md)).
- Runtime `debug export current` может отличаться от `tmpMap/Mine.tmx` в репозитории.

**Ограничения движения (жёсткие):**

- Не использовать **y ≥ 11** для `move` NPC (Front/обрыв).
- Не вести через **(23,9)** спуск, **(11–12,10)** вагонетку, **(17,3)** лифт.

---

## SkullCave (vanilla 16×10)

| Координата | TMX | Движение в событии |
|------------|-----|-------------------|
| (5, 5) farmer | Back OK | setup |
| (7, 7) Harvey | Back OK | setup |
| (7, 5) Harvey | Back OK | после `move 0 -2` |
| (6, 7) Harvey | Back OK | после `move -1 0` |

Карта **фиксированная**, уровней нет. SVE только **EditMap** warp (Desert). Координаты **стабильны**.

Риск: warp **(7,9)→Desert** — не ставить NPC на (7,9); текущий скрипт туда не ходит.

---

## Volcano — нет CP-событий, но влияет на mine pipeline

| Механика | Поведение | Риск |
|----------|-----------|------|
| `eventHarveyMinorMineRescue` | Может стартовать из Volcano → warp на **Mine (17,7)** | Medium — cutscene «у входа в шахту», хотя игрок был в вулкане |
| `NeedsMineRescueEvent` (major rescue) | `OnUpdateTicked`: `loc.Contains("Mine")` — **VolcanoDungeon не матчится** | **High** — боевая смерть в вулкане может **не** запустить rescue утром |
| `TrackPassOut` fallback | Только `Contains("Mine")` | **High** — тот же пробел |
| `HarveyMod_MineForbidden` / mail | Вход MineShaft **или** VolcanoDungeon | OK — не event |

**Отдельных событий `Data/Events/Volcano*` в CP нет.**

---

## High-risk: детальные блоки

## eventHarveySkullCavePrevention

### Почему риск

1. **Несовпадение локации события и триггера:** `triggerHarveySkullCaveWarning` (`triggersCare.json`) срабатывает при `PLAYER_LOCATION_NAME Current **Mine SkullCave**` (логическое OR), но вызывает `SpaceCore_PlayEvent **eventHarveySkullCavePrevention**`, которое лежит в **`Data/Events/SkullCave`**, а не `Mine`.
2. При входе в **`Mine`** с injury-buffs игрок **не на SkullCave** — SpaceCore может не найти/не запустить событие, телепортировать не туда или дать пустую сцену.
3. Дублирование: тот же event ID также вызывается корректным триггером `triggerLocationReactionSkullCaveExit` только для **SkullCave**.
4. **Координаты (5,5)/(7,7)** на SkullCave TMX — **OK**; проблема не в тайлах, а в **инфраструктуре запуска**.

### Безопасная альтернатива

- **Триггер:** разделить на два: `Mine` → `eventHarveyMineInterception` (уже есть отдельный `triggerHarveyMineWarning`); `SkullCave` → `eventHarveySkullCavePrevention`.
- **Убрать `Mine`** из условия `triggerHarveySkullCaveWarning` или заменить event ID на mine-версию в `Data/Events/Mine`.
- **Координаты SkullCave** оставить как есть — зона (2,4)–(13,9) достаточна.

### Что потом должен править Cursor

1. `assets/Code/triggersCare.json` — условие `triggerHarveySkullCaveWarning`: только `SkullCave`, не `Mine SkullCave`.
2. Проверить in-game: вход в Mine с `buffFracturedBone` → только **interception**, не skull event.
3. Не трогать координаты в `eventsCare.json`, пока триггер не исправлен.

---

## eventHarveyMineRescue / eventHarveyMineRescueDating

### Почему риск

1. **Medium, не Broken:** координаты **(17,7)/(17,10)** согласованы с C# `BeginMineRescueWarp(17,7)` и SVE TMX.
2. **Контекст смерти:** игрок мог погибнуть на **MineShaft level 80** — cutscene показывает **вход**; это narratively OK, но технически зависит от **утреннего warp** (без него — event на wrong map).
3. **Volcano gap:** смерть в `VolcanoDungeon` может **не** попасть в `NeedsMineRescueEvent` — rescue **не запустится** (C#, не CP).
4. **Hospital (20,5):** Buildings-тайл кровати — ожидаемо; работает с `ignoreCollisions` + `positionOffset` + animate lying ([`events-coordinate-audit.md`](events-coordinate-audit.md)).
5. **SVE / OriginalMinesEntrance:** фон и warp **(18,14)** другие; зона 17,7–17,10 в audit остаётся проходимой.

### Безопасная альтернатива

- **Оставить** схему: pre-warp **Mine (17,7)** → короткая сцена на входе → **fade → Hospital**.
- **Не переносить** major rescue целиком в Hospital — теряется «нашёл в шахте».
- **Опционально:** если death location = Volcano → skip Mine act, только Hospital (отдельная ветка C#).
- **Не использовать** координаты внутри `MineShaft` — карта процедурная.

### Что потом должен править Cursor

1. **C#** `PassOutHandler.OnUpdateTicked` / `TrackPassOut`: добавить `VolcanoDungeon` к детекции боевой смерти (отдельная задача).
2. Подтвердить `BeginMineRescueWarp` всегда вызывается до `startEvent` (уже так).
3. CP: не менять (17,7)/(17,10) без повторного TMX-export; Hospital bed — только если in-game lying frame ломается.

---

## eventHarveyMinorMineRescue

### Почему риск

1. Триггер из **`VolcanoDungeon`**: C# warp'ит на **Mine (17,7)** — координаты стабильны, но **локация cutscene** не совпадает с местом триггера.
2. Если игрок **уже в Mine** при триггере — warp не нужен, setup **(17,7)/(17,10)** всё равно задаёт позицию в скрипте.
3. Короткий script → быстрый `changeLocation Hospital` **(14,6)** — низкий риск pathfinding.
4. Конфликт с pending major rescue блокируется `CanTriggerMinorMineRescue` (`NeedsMineRescueEvent`, `topicMineRescuePending`).

### Безопасная альтернатива

- **Оставить** pre-warp на **(17,7)** для всех случаев вне Mine.
- **Альтернатива для Volcano:** `globalFade` → `changeLocation Hospital` без акта на Mine (короче, без риска южного края карты).
- Не использовать `MineShaft`.

### Что потом должен править Cursor

1. Опционально: в `TryTriggerMinorMineRescue` ветка `if (VolcanoDungeon)` → warp Hospital + урезанный event key (новый ID или fork).
2. CP: Hospital **(14,6)** уже проходим — менять только при in-game провале.

---

## eventHarveyStormComfortMine

### Почему риск

1. **CP location entry**, не C# warp: setup **(15,5)** задаёт farmer, но вход может быть с любого warp-тайла Mine — setup **перезаписывает** позицию (OK).
2. Harvey **warp (18,13)** — у **южного** края; сосед **(18,14)** = warp на **Custom_AdventurerSummit** (SVE).
3. **`move Harvey 0 -8`** из (18,13) → (18,5): проходит через y=12–6; по TMX x=18 проходим, но зона **y≥11** historically рискованна для Front — нужен **in-game** прогон с SVE.
4. Финал **Town (72,22)** — отдельная карта, OK по coordinate audit.
5. **Random 0.8** — не map risk, но событие редко воспроизводится для QA.

### Безопасная альтернатива

- **Вариант A (минимальный):** Harvey warp **(17,10)** + `move 0 -2` (как rescue) вместо (18,13)+8 тайлов.
- **Вариант B:** farmer **(17,7)**, Harvey **(17,10)**, один `move`, затем fade → Town (убрать walk у южного края).
- **Вариант C:** весь акт Mine = 2–3 реплики + `globalFade` без `move`.

### Что потом должен править Cursor

1. `events.json` → блок `eventHarveyStormComfortMine`: заменить **(18,13)** и длинный move на коридор **17,7–17,10**.
2. Прогон при **шторме + SVE + OriginalMinesEntrance** (если активен в save).
3. Town-финал **(72,22)/(73,22)** не трогать без экспорта Town TMX.

---

## C# bridge: Volcano + major rescue (не event, но блокирует события)

### Почему риск

- Major rescue events **никогда не стартуют**, если `NeedsMineRescueEvent` не выставлен.
- Детекция HP≤0 в `OnUpdateTicked` (стр. ~177): `loc.Contains("Mine")` — **`VolcanoDungeon` не подходит**.
- Документация мода (`docs/mines-forbidden-injuries.md`) уже отмечает этот разрыв для топика rescue.

### Безопасная альтернатива

- Расширить детекцию: `location is VolcanoDungeon` **или** `loc.Contains("Volcano")`.
- Утренний pipeline **без изменений:** `BeginMineRescueWarp` → Mine (17,7) → тот же CP event.

### Что потом должен править Cursor

1. `PassOutHandler.cs`: `OnUpdateTicked`, `TrackPassOut` — единый helper `IsCombatDeathUnderground(location)`.
2. Тест: HP=0 в Volcano → утро → warp Mine → `eventHarveyMineRescueDating`.
3. CP-события не менять.

---

## eventHarveyMineInterception

### Почему риск (Low–Medium)

- Координаты **(17,7)/(17,10)** — стабильны на Mine TMX.
- SpaceCore запускает при **LocationChanged → Mine**; setup **перезаписывает** позиции.
- Финальный **`move Harvey/farmer 0 3 2`** → y≈10 — внутри безопасного коридора.
- Конкуренция с `topicMineRescuePending` и `triggerHarveyMineWarning` mail — логическая, не координатная.

### Безопасная альтернатива

- **Оставить как есть** на Mine entrance.
- Не переносить на AdventurerSummit без новой карты и патча `Data/Events/Custom_AdventurerSummit`.

### Что потом должен править Cursor

- Только если in-game NPC застревает на exit move — сократить до `globalFade` без `move 0 3`.
- Координаты **(17,7)/(17,10)** не менять без нового TMX-export.

---

## Рекомендуемые стратегии (сводка)

| Стратегия | Когда применять |
|-----------|-----------------|
| **Оставить как есть + pre-warp (17,7)** | Major/minor rescue, interception — **default** |
| **Только стабильный Mine entrance** | Все текущие CP mine events — **уже так**; не патчить `MineShaft` |
| **Pre-warp на фиксированную точку** | C# **уже делает** для rescue; storm comfort полагается на setup в event |
| **Короткая сцена без walk** | Storm comfort (южный край); optional Volcano minor rescue |
| **Aftermath только Hospital** | Уже для rescue/minor; Hospital bed — с offset/ignoreCollisions |
| **Исправить триггер, не координаты** | SkullCave prevention при входе в **Mine** |
| **Исправить C#, не CP** | Volcano combat death → major rescue |

---

## Чеклист in-game (SVE save)

1. Death **MineShaft L40** → утро → warp **Mine (17,7)** → major rescue → Hospital bed.
2. Death **VolcanoDungeon** → утро → **ожидается ли rescue?** (сейчас — возможен **пропуск**).
3. Entry **Mine** injured → interception **(17,7)**; **не** skull event.
4. Entry **SkullCave** → skull prevention **(5,5)**.
5. Entry **Mine** injured (Skull trigger bug) → воспроизвести `triggerHarveySkullCaveWarning`.
6. **Storm + Mine entry** → storm comfort; Harvey path у **(18,13)**.
7. Save с **OriginalMinesEntrance** → повторить п.1 и п.6.

---

## Связанные документы

- [`events-coordinate-audit.md`](events-coordinate-audit.md) — постановка по тайлам
- [`events-map-audit-plan.md`](events-map-audit-plan.md) — общий план 31 события
- [`map-passports.md`](map-passports.md) — Mine / SkullCave
- [`../../tmpMap/Mine_event_placement_analysis.md`](../../tmpMap/Mine_event_placement_analysis.md) — детальная сетка входа
