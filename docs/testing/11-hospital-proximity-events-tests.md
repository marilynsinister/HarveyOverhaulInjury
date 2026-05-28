# Госпитализация, proximity и CP-события — чеклист QA

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Механики C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) · CP: [02-cp-content-inventory.md](02-cp-content-inventory.md)  
> Setup: [06-debug-setup-commands.md](06-debug-setup-commands.md) · Assert: [05-debug-dump-commands.md](05-debug-dump-commands.md)  
> Связанные: [10-mine-passout-tests.md](10-mine-passout-tests.md) (HOI-PASSOUT-003) · [EVENTS_TEST_CHECKLIST.md](EVENTS_TEST_CHECKLIST.md)  
> MCP: [stardew-mcp.md](stardew-mcp.md) · [injury-mcp.md](injury-mcp.md)

**Область:** принудительная госпитализация (`HospitalizationManager`), hospital hold / discharge, proximity-реакции (`HarveyReactionManager`), smoke CP cutscenes InjuryCare.  
**Не цель:** полный story arc, stress-only события, рефакторинг CP/C# «по ходу теста».

Отмечайте `- [ ]` → `- [x]` по мере проверки.

---

## Журнал прогона

| Поле | Значение |
|------|----------|
| Тестер | |
| Слот сохранения | |
| Версия C# мода | |
| Версия CP | |
| `ForceHospitalization` в config | |
| `MinHospitalStayMinutes` в config | |
| `HospitalActivityIntervalMinutes` в config | |
| `HospitalBedX` / `HospitalBedY` в config | |
| Harvey: dating/married (для proximity / mine rescue CP) | |
| Дата | |

| ID | Сценарий | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-HOSP-001 | Forced hospitalization стартует | [ ] | |
| HOI-HOSP-002 | Warp на койку (HospitalBedX/Y) | [ ] | |
| HOI-HOSP-003 | Выход заблокирован до MinHospitalStayMinutes | [ ] | |
| HOI-HOSP-004 | После срока можно выйти | [ ] | |
| HOI-HOSP-005 | Hospital activities (интервал) | [ ] | |
| HOI-HOSP-006 | Emergency discharge (`injury_hospital_discharge`) | [ ] | |
| HOI-HOSP-007 | Regression: чужое событие при выходе | [ ] | |
| HOI-PROX-001 | Реакция на травму рядом с Харви | [ ] | |
| HOI-PROX-002 | Proximity не начинает лечение | [ ] | |
| HOI-PROX-003 | Реакция не спамится | [ ] | |
| HOI-PROX-004 | Смена локации сбрасывает флаг | [ ] | |
| HOI-CP-001 | `eventHarveyMineRescueDating` | [ ] | |
| HOI-CP-002 | `eventHarveyMineRescue` | [ ] | |
| HOI-CP-003 | `eventHarveyMinorMineRescue` | [ ] | |
| HOI-CP-004 | `eventHarveyMineInterception` | [ ] | |
| HOI-CP-005 | `eventHarveySkullCavePrevention` | [ ] | |
| HOI-CP-006 | `eventStayInHospital` | [ ] | |
| HOI-CP-007 | `eventHarveyLateNightCollapse` | [ ] | |
| HOI-CP-008 | `eventHarveyCheckHealthFarmer` | [ ] | |
| HOI-CP-009 | `eventHarveyEmergencyCare` | [ ] | |
| HOI-CP-010 | `eventHarveyExhaustion` | [ ] | |
| HOI-CP-011 | `eventHarveyTreatmentCollapse` | [ ] | |

---

## Предусловия (все TC)

- [ ] Игра через SMAPI, загружен тестовый сейв (`Context.IsWorldReady`)
- [ ] C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`
- [ ] `injury_validate_buffs` → `result=OK` (см. [07-smoke-save-tests.md](07-smoke-save-tests.md))
- [ ] Перед **каждым** изолированным TC: **`injury_reset`** (SMAPI / Injury MCP)
- [ ] Записать в журнал значения из `config.json` мода (см. таблицу ниже)

### Config (влияет на HOI-HOSP-*)

| Параметр | Default | Влияние |
|----------|---------|---------|
| `ForceHospitalization` | `true` | HOI-HOSP-001 — без него hold не стартует |
| `MinHospitalStayMinutes` | `120` | HOI-HOSP-003/004 — fallback срок, если injury-specific меньше |
| `HospitalActivityIntervalMinutes` | `40` | HOI-HOSP-005 — для проверки «каждые 20 мин» **временно** поставить `20` в config |
| `MaxHospitalActivitiesPerStay` | `3` | HOI-HOSP-005 — лимит активностей за stay |
| `HospitalLocationName` | `Hospital` | все hosp TC |
| `HospitalBedX` / `HospitalBedY` | `20` / `4` | HOI-HOSP-002 — сверять с config, не с устаревшими docs |
| `ProximityTiles` | `3` | HOI-PROX-* — радиус обнаружения |

**Severe ID** (`InjurySets.Severe`): `buffBadlyHurt`, `buffFracturedBone`, `buffConcussion`, `buffInfectedWound`, `buffBurnWounds`, `buffShrapnelWounds`, `buffSurgicalWound`.

### Общая подготовка мира (StardewMCP)

| Tool | Аргументы | Зачем |
|------|-----------|-------|
| `pause_time` | `true` / `false` | Стабильный setup / прогон игровых минут |
| `set_time` | `10am`, `12:00pm`, … | Admission, discharge, late collapse |
| `teleport_player` | `Hospital` | Forced hosp., CP events |
| `teleport_player` | `Town`, опц. `37`, `59` | Late collapse |
| `warp_to_mine_floor` | `10` | Mine rescue CP |
| `teleport_player` | `SkullCave`, опц. `5`, `5` | Skull prevention |
| `teleport_player` | `Farm` | Check health farmer |
| `set_npc_relationship` | `Harvey`, `8` | Сердечки (не заменяет dating) |
| `get_player_info` | — | Локация, tile после warp |
| `get_surroundings` | — | NPC/объекты у койки |
| `get_walkable_tiles` | `Hospital` | Walkable вокруг `(HospitalBedX, HospitalBedY)` |
| `get_npc_info` | `Harvey` | Dating/married для CP |

### Assert (SMAPI / Injury MCP)

```
injury_hospital_status
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Полезные log-префиксы

| Префикс | Когда |
|---------|-------|
| `🏥 Начинаем принудительную госпитализацию` | Start forced hosp |
| `⚠️ Игрок в госпитале с ранами после шахты` | HandleHospitalLogic |
| `🏥 Попытка покинуть больницу заблокирована` | HandleWarpAttempt |
| `✅ Минимальный срок госпитализации прошёл` | NotifyDischargeReadyIfNeeded |
| `🏥 Активность #N:` | HospitalActivityManager |
| `[Proximity]` | Облачко, кулдаун, сброс локации |
| `[Warped] Пропуск location logic: активно событие` | Regression HOI-HOSP-007 |
| `[PassOutEvent]` | Hospital pass-out cutscenes (CP-009/010) |

### Разделение инструментов

| Действие | Инструмент |
|----------|------------|
| Телепорт, время, окружение, walkable tiles | **StardewMCP** (`user-stardew`) |
| `injury_*`, hospital status/discharge | **SMAPI** / **Injury MCP** (`user-harvey-injury`) |
| `debug ebi <eventId>` | **SMAPI debug console** (не StardewMCP) |
| Cutscene skip, HUD/F10, клик Harvey | **вручную** |

---

# Госпитализация

Цепочка forced hosp (C# `PlayerEventHandler.HandleHospitalLogic` / proximity mine-rescue):

```text
topicMineInjuryRescue + MainInjury ∈ Severe + ForceHospitalization
  → вход в Hospital ИЛИ proximity к Harvey (2-й заход)
  → HospitalizationManager.StartForcedHospitalizationWithExplanation(reason=mine_rescue)
  → IsHospitalized=true, WarpToHospitalBed, topicMineInjuryRescue снят
  → HandleWarpAttempt блокирует выход из Hospital до CanDischarge()
  → TimeChanged: NotifyDischargeReadyIfNeeded + HospitalActivityManager
  → выход после срока: Discharge() (+ intensive→outpatient для buffBadlyHurt)
```

**Min stay по injury** (`CalculateHospitalStayMinutes`): badly hurt 90 мин (mine_rescue ≥120), burn/shrapnel/surgical 120, concussion/infected/fracture 180; иначе `MinHospitalStayMinutes`.

---

## HOI-HOSP-001 — Forced hospitalization стартует

### ID

HOI-HOSP-001

### Цель

**Severe** main + `topicMineInjuryRescue` + вход в **`Hospital`** при `ForceHospitalization=true` → hospital hold (`IsHospitalized=true`).

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `pause_time` | `true` |
| `set_time` | `10am` |

### Команды SMAPI / Injury MCP

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
injury_phase_list
injury_topic_dump
```

Ожидание до входа: `MainInjuryId=buffBadlyHurt`, topic **`topicMineInjuryRescue`** активен.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `teleport_player` `Hospital` |
| 2 | **SMAPI** | `injury_hospital_status` → `IsHospitalized=True`, `HospitalizedInjuryId=buffBadlyHurt`, `HospitalizationReason=mine_rescue` |
| 3 | **SMAPI** | `injury_topic_dump` → **`topicMineInjuryRescue` удалён** |
| 4 | **SMAPI** | `injury_buff_dump` → **`buffHarveyIntensiveCare`** (pre-hospital treatment) или phase buff badly hurt |
| 5 | **SMAPI log** | `⚠️ Игрок в госпитале с ранами после шахты → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ` |
| 6 | **вручную** | Диалог объяснения Харви (mine rescue text); звук `debuffHit` |

### Ожидаемый результат

- Hospital hold активен; topic rescue снят
- `HospitalMinStayMinutes≥120` для `mine_rescue` + `buffBadlyHurt`
- `topicHarvey_ForcedHospitalization` может быть добавлен (2 дня)

### Debug HUD (F10) / log

- F10: `IsHospitalized`, injury id, admission time
- Log: `🏥 Начинаем принудительную госпитализацию: buffBadlyHurt, причина: mine_rescue`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| `IsHospitalized=True` сразу после warp Hospital | Hold не стартует при `ForceHospitalization=true` + severe + topic |
| Topic rescue снят | `buffHurt` (не Severe) + topic → hosp (ложное) |
| | Повторный вход в Hospital re-triggers hosp без reset |

### Статус

- [ ] Сценарий пройден

---

## HOI-HOSP-002 — Warp на койку (HospitalBedX/Y)

### ID

HOI-HOSP-002

### Цель

После forced hosp игрок оказывается на **`(HospitalBedX, HospitalBedY)`** из config; тайл walkable, рядом нет soft-lock.

### Подготовка (StardewMCP)

`pause_time` `true`, `set_time` `10am`

### Команды SMAPI / Injury MCP

Пройти setup HOI-HOSP-001 **или**:

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
```

Записать из config / журнала: `HospitalBedX`, `HospitalBedY` (default **20**, **4**).

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `teleport_player` `Hospital` → forced hosp |
| 2 | **StardewMCP** | `get_player_info` → `location=Hospital`, tile **(X,Y) = config bed** |
| 3 | **StardewMCP** | `get_walkable_tiles` `Hospital` — bed tile **в списке walkable** |
| 4 | **StardewMCP** | `get_surroundings` — Harvey/NPC/объекты в радиусе; нет застревания в стене |
| 5 | **SMAPI** | `injury_hospital_status` → `IsHospitalized=True` |
| 6 | **вручную** | Игрок может двигаться **внутри** Hospital (не выходя наружу) |

### Ожидаемый результат

- Координаты совпадают с `ModConfig.HospitalBedX/Y` ±0
- Walkable tile; farmer `faceDirection` вверх (0) после warp
- Нет NRE / black screen при warp

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Tile = config bed; walkable | Warp на `(0,0)`, в стену, или другую палату без обновления config |
| | `get_player_info` location ≠ `Hospital` после hosp |

### Статус

- [ ] Сценарий пройден

---

## HOI-HOSP-003 — Выход заблокирован до MinHospitalStayMinutes

### ID

HOI-HOSP-003

### Цель

Пока **`CanDischarge=False`**, попытка выйти из **`Hospital`** блокируется: HUD error, отложенный возврат на койку, **без** снятия `IsHospitalized`.

### Подготовка (StardewMCP)

`pause_time` `false` (для корректного elapsed — или держать время до попытки выхода)

### Команды SMAPI / Injury MCP

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
```

StardewMCP: `teleport_player` `Hospital` → hosp active.

```
injury_hospital_status
```

Записать `HospitalAdmissionMinutes`, `HospitalMinStayMinutes`, `CanDischarge=False`.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI** | Убедиться `CanDischarge=False` (сразу после admission) |
| 2 | **StardewMCP** | `teleport_player` `Town` (или выход через дверь **вручную**) |
| 3 | **вручную / HUD** | HUD: *«Тебе пока нельзя покидать больницу.»* (`error_type`) |
| 4 | **SMAPI log** | `🏥 Попытка покинуть больницу заблокирована — отложенный возврат` |
| 5 | **Подождать ~1 с** | `UpdateHospitalizationLock` (каждые 15 тиков) → warp обратно на койку |
| 6 | **StardewMCP** | `get_player_info` → снова `Hospital`, tile bed |
| 7 | **SMAPI** | `injury_hospital_status` → **`IsHospitalized=True`** (hold не снят) |
| 8 | **вручную** | Реплика Харви «Назад в палату» **после** возврата (если Harvey в локации) |

### Ожидаемый результат

- Игрок не остаётся в Town; state hold сохранён
- `HospitalDischargeReadyShown=False`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Блок + return to bed; `IsHospitalized` true | Свободный выход до min stay |
| | Hold снят без `Discharge()` |

### Статус

- [ ] Сценарий пройден

---

## HOI-HOSP-004 — После срока можно выйти

### ID

HOI-HOSP-004

### Цель

После **`CanDischarge=True`** выход из Hospital вызывает **`Discharge()`**, снимает hold, разрешает warp; для `buffBadlyHurt` — intensive → outpatient.

### Подготовка (StardewMCP)

`pause_time` `false`

### Команды SMAPI / Injury MCP

Setup как HOI-HOSP-001:

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
```

StardewMCP: `teleport_player` `Hospital`.

```
injury_hospital_status
```

Запомнить `HospitalMinStayMinutes` (ожидание **≥120** для mine_rescue + badly hurt).

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | Сдвинуть время: `set_time` на **admission + MinStay** (напр. с `10:00am` → `12:00pm` при stay=120) |
| 2 | **SMAPI** | `injury_hospital_status` → `CanDischarge=True`; при первом tick — `HospitalDischargeReadyShown=True` |
| 3 | **вручную / HUD** | HUD: *«Харви разрешил выписку…»* |
| 4 | **StardewMCP** | `teleport_player` `Town` (выход из Hospital) |
| 5 | **SMAPI** | `injury_hospital_status` → **`IsHospitalized=False`** |
| 6 | **SMAPI** | `injury_buff_dump` → нет `buffHarveyIntensiveCare`; есть **`HarveyMod_BadlyHurt_OutpatientCare`** |
| 7 | **SMAPI log** | `✅ Игрок покидает больницу после окончания срока` / `🏥 Выписка пациента` |
| 8 | **вручную** | Реплика Харви на выписку (если Harvey в Hospital при exit) |

### Ожидаемый результат

- Hold снят; игрок в целевой локации (Town)
- Main injury `buffBadlyHurt` сохранён

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Discharge после min stay; outpatient buff | Выход заблокирован после `CanDischarge=True` |
| | `IsHospitalized` висит после легального выхода |

### Статус

- [ ] Сценарий пройден

---

## HOI-HOSP-005 — Hospital activities (интервал игровых минут)

### ID

HOI-HOSP-005

### Цель

Во время hold **`HospitalActivityManager`** запускает случайные активности с интервалом **`HospitalActivityIntervalMinutes`** (для этого TC — **20** мин в config; default в коде **40**).

**Пул активностей:** `checkVitals`, `bringWater`, `adjustPillow`, `readChart`, `conversation`, `holdHand`, `checkBandage`, `bringMedicine`, `comfort`, `checkTemperature`.

### Подготовка

- [ ] В `config.json`: `"HospitalActivityIntervalMinutes": 20` (записать в журнал)
- [ ] `MaxHospitalActivitiesPerStay` ≥ 2 для наблюдения нескольких активностей

StardewMCP: `pause_time` `false`, `set_time` `10:00am`

### Команды SMAPI / Injury MCP

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
```

StardewMCP: `teleport_player` `Hospital`.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI log** | Очистить/отметить позицию лога |
| 2 | **StardewMCP** | `set_time` `10:20am` ( +20 игр. мин от admission) |
| 3 | **SMAPI log** | Строка `🏥 Активность #1: <id>` где `<id>` ∈ пулу выше |
| 4 | **StardewMCP** | `set_time` `10:40am` |
| 5 | **SMAPI log** | `🏥 Активность #2: …` (если `< MaxHospitalActivitiesPerStay`) |
| 6 | **вручную** | Диалоговое окно Харви (не облачко proximity); звук `healSound` |
| 7 | **Проверка эффектов** | `bringWater` → +15 stamina; `bringMedicine` → +10 HP; `holdHand` → dating-only bonus HP |

**Не должно срабатывать:** во время `Game1.eventUp`, меню, `HasPendingReturnToHospital`, после `MaxHospitalActivitiesPerStay`.

### Ожидаемый результат

- ≥1 активность после первого интервала; id из фиксированного списка
- Нет NRE; игрок не теряет control навсегда

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Log activity #N с valid id; dialog closes | Нет активностей за 2× interval при свободном игроке |
| | Активности во время cutscene / blocked exit pending |

### Статус

- [ ] Сценарий пройден · [ ] SKIP (config interval не меняли)

---

## HOI-HOSP-006 — Emergency discharge (debug)

### ID

HOI-HOSP-006

### Цель

**`injury_hospital_discharge`** принудительно снимает hold **без** ожидания min stay и **без** warp (только state + buff cleanup).

### Подготовка (StardewMCP)

`pause_time` `true`, `set_time` `10am`

### Команды SMAPI / Injury MCP

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
```

StardewMCP: `teleport_player` `Hospital` → hosp.

```
injury_hospital_status
```

→ `IsHospitalized=True`, `CanDischarge=False`.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI** | `injury_hospital_discharge` |
| 2 | **SMAPI log** | `[QA] injury_hospital_discharge injury=buffBadlyHurt ok=yes` |
| 3 | **SMAPI** | `injury_hospital_status` → **`IsHospitalized=False`** |
| 4 | **SMAPI** | `injury_buff_dump` → **`HarveyMod_BadlyHurt_OutpatientCare`**, нет intensive |
| 5 | **StardewMCP** | `get_player_info` → игрок **остался** на текущем tile (warp **не** выполнялся) |
| 6 | **StardewMCP** | `teleport_player` `Town` — выход **не** блокируется |
| 7 | **SMAPI** | `injury_hospital_discharge` повторно → `ok=no reason=not hospitalized` |

### Ожидаемый результат

- QA discharge = gameplay `Discharge()` по buff policy
- Main injury не cure'ится

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| ok=yes; hold снят; outpatient для badly hurt | ok=yes при `IsHospitalized=False` |
| | Intensive care остаётся после discharge |

### Статус

- [ ] Сценарий пройден

---

## HOI-HOSP-007 — Regression: чужое событие при выходе из больницы

### ID

HOI-HOSP-007

### Цель

Если во время hospital hold играет **другое CP-событие**, реплика Харви (**discharge / «назад в палату»**) **не вставляется** в чужой cutscene; после события **hold и warp-lock сохраняются**.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `pause_time` | `true` |
| `set_time` | `10am` |
| `teleport_player` | `Hospital` |

### Команды SMAPI / Injury MCP

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
```

StardewMCP: `teleport_player` `Hospital` → hosp.

**Ветка A — blocked exit во время события:**

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI** | `injury_hospital_status` → `CanDischarge=False` |
| 2 | **SMAPI debug** | `debug ebi eventStayInHospital` (короткая сцена в Hospital) |
| 3 | **во время события** | **StardewMCP** `teleport_player` `Town` — попытка выхода |
| 4 | **SMAPI log** | `[Warped] Пропуск location logic: активно событие` **или** блок без `drawDialogue` поверх event |
| 5 | **вручную** | **Нет** наложения dialogue box Харви поверх cutscene; esc/skip — событие до конца |
| 6 | **после события** | `injury_hospital_status` → **`IsHospitalized=True`** |
| 7 | **StardewMCP** | При необходимости подождать tick → игрок возвращён на койку (`get_player_info`) |
| 8 | **SMAPI log** | Нет `NullReferenceException` в `[Hospital]` / `DialogueManager` |

**Ветка B — discharge-ready + событие:**

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `set_time` +min stay → `CanDischarge=True` |
| 2 | **SMAPI debug** | `debug ebi eventStayInHospital` **до** выхода |
| 3 | **после end event** | выход `Town` → нормальный `Discharge()` **без** corrupted event state |
| 4 | **SMAPI** | `IsHospitalized=False` только после легального exit |

### Ожидаемый результат

- `UpdateHospitalizationLock` / `HandleWarpAttempt` уважают `Game1.eventUp`
- Hold не теряется silently; pending return отрабатывает после события

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Нет Harvey dialogue overlay на чужом event; hold сохранён | Dialogue box Харви ломает cutscene |
| | `IsHospitalized=False` mid-event без discharge |
| | NRE; игрок soft-lock после event+exit |

### Статус

- [ ] Сценарий пройден (ветка A / B)

---

# Proximity

Механика (C# `PlayerEventHandler.CheckHarveyProximity`):

```text
Harvey в той же локации, dist ≤ ProximityTiles
  → mine-rescue path: warning emote → forced hosp (не обычное облачко)
  → иначе: ShowEmoteWithText (без DialogueBox, без StartTreatment)
Антиспам: 1× за локацию (_proximityReactionShown) + кулдаун 120 игр. мин
Смена локации: сброс _proximityReactionShown (кулдаун LastProximityReactionMinute сохраняется)
```

**Sanity CP-линий:** `injury_proximity_test <situation> [tone]` — только текст, **без** state (не заменяет HOI-PROX-001).

---

## HOI-PROX-001 — Реакция на травму рядом с Харви

### ID

HOI-PROX-001

### Цель

При активной травме и Harvey в радиу **`ProximityTiles`** показывается **облачко** (emote + text above head), topic proximity при необходимости.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `pause_time` | `true` |
| `set_time` | `10am` |
| `teleport_player` | `Hospital` |
| `set_npc_relationship` | `Harvey`, `6` |

Harvey должен быть **в той же локации** (рабочие часы клиники). Проверка: `get_surroundings` → `Harvey`.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_state_dump
```

**Не** добавлять `topicMineInjuryRescue` (иначе forced hosp path).

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | Подойти к Harvey в пределах 3 клеток (**вручную** или teleport рядом) |
| 2 | **Подождать ≤1 с** | `CheckHarveyProximity` (UpdateTicked /60) |
| 3 | **SMAPI log** | `[Proximity] Показ облачка: локация=Hospital, дистанция=…` |
| 4 | **SMAPI log** | `[Proximity] Облачко: context=Untreated|LightInjury|…` |
| 5 | **вручную** | Текст над головой Harvey; **нет** полноэкранного `drawDialogue` |
| 6 | **SMAPI** | `injury_state_dump` → `LastProximityReactionMinute≥0`, `LastProximityReactionReason` заполнен |
| 7 | **SMAPI** | `injury_topic_dump` → опц. `topicHarvey_ProximityReaction` / `topicHarvey_ProximityStrict` |

### Ожидаемый результат

- Облачко видно; движение игрока **не** блокируется
- CP-текст из `HarveyProximityInjuryDialogue` (prefix `Proximity_Injury_*`)

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Emote + text; state LastProximity* обновлён | Нет реакции при Harvey рядом + active debuff |
| | Сразу forced hosp без mine-rescue topic |

### Статус

- [ ] Сценарий пройден · [ ] SKIP (Harvey не в Hospital)

---

## HOI-PROX-002 — Proximity не начинает лечение автоматически

### ID

HOI-PROX-002

### Цель

Proximity-обнаружение **не** вызывает `StartTreatment` / `ApplyTreatmentForInjury` — только emote; лечение по-прежнему только **клик Harvey**.

### Подготовка (StardewMCP)

Как HOI-PROX-001.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffHurt
injury_phase_list
```

До proximity: `TreatmentStarted=False` (или отсутствует debuff state treatment flags).

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | Подойти к Harvey → дождаться proximity облачка |
| 2 | **SMAPI** | `injury_phase_list` → **`TreatmentStarted=False`** |
| 3 | **SMAPI** | `injury_buff_dump` → **нет** `buffHarveyTreatment` / phase buff |
| 4 | **SMAPI** | `injury_topic_dump` → **есть** `topicHurt`; **нет** `topicTreatmentHurt` |
| 5 | **SMAPI** | `injury_hospital_status` → `IsHospitalized=False` |
| 6 | **Опционально** | `injury_harvey_click` → **только после** явного simulate click лечение стартует |

### Ожидаемый результат

- Proximity = UX hint only
- `LastClickDebug` (F10) пуст до клика

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| После proximity — untreated state | `TreatmentStarted=True` или cure buff без клика |
| | Forced hosp от `buffHurt` без mine topic |

### Статус

- [ ] Сценарий пройден

---

## HOI-PROX-003 — Реакция не спамится

### ID

HOI-PROX-003

### Цель

В **одной локации** — не более **одного** proximity-облачка; глобальный кулдаун **120 игровых минут** между обычными реакциями.

### Подготовка (StardewMCP)

`pause_time` `false` для ветки кулдауна; `Hospital`, `10am`, Harvey на месте.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
```

### Шаги — per-location

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | Подойти к Harvey → 1-е облачко |
| 2 | **SMAPI log** | `[Proximity] Показ облачка` (1 раз) |
| 3 | **StardewMCP** | Отойти и снова подойти **в Hospital** |
| 4 | **SMAPI log** | `[Proximity] Пропуск: уже показано в этой локации` |
| 5 | **вручную** | **Нет** второго облачка |

### Шаги — cooldown (опц.)

| # | Кто | Действие |
|---|-----|----------|
| 1 | После шага 1 — **StardewMCP** `teleport_player` `Farm` (сброс per-location) |
| 2 | Harvey **не** рядом на Farm — только смена локации |
| 3 | **StardewMCP** | `set_time` +**30** min (< 120) → вернуться `Hospital` |
| 4 | **SMAPI log** | `[Proximity] Пропуск: кулдаун …/120` |
| 5 | **StardewMCP** | `set_time` +**120** min total → снова Hospital + Harvey |
| 6 | **SMAPI log** | Новое облачко разрешено |

### Ожидаемый результат

- Per-location flag и 120-min cooldown работают независимо

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| 2-й подход в той же локации без spam | Два облачка подряд в Hospital |
| | Кулдаун <120 мин игнорируется |

### Статус

- [ ] Сценарий пройден

---

## HOI-PROX-004 — Смена локации сбрасывает флаг реакции

### ID

HOI-PROX-004

### Цель

При **warp в другую локацию** сбрасывается `_proximityReactionShown` (per-location), но **`LastProximityReactionMinute`** сохраняется для глобального кулдауна.

### Подготовка (StardewMCP)

`Hospital`, `10am`

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **Hospital** | Proximity облачко #1 |
| 2 | **SMAPI** | `injury_state_dump` → записать `LastProximityReactionMinute=M0` |
| 3 | **StardewMCP** | `teleport_player` `Farm` |
| 4 | **SMAPI log** | `[Proximity] Локация «Farm»: сброс per-location, кулдаун с M0` |
| 5 | **StardewMCP** | `teleport_player` `Hospital` (если Harvey доступен — dating save / время) |
| 6 | **Если elapsed ≥120 мин** | Новое облачко возможно |
| 7 | **Если elapsed <120 мин** | `[Proximity] Пропуск: кулдаун` — per-location сброшен, но cooldown **нет** |

### Ожидаемый результат

- Log подтверждает reset per-location без обнуления `LastProximityReactionMinute`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Log «сброс per-location» на смене локации | После Farm→Hospital мгновенный 2-й bubble при cooldown <120 |
| | `LastProximityReactionMinute` сброшен в -1 без новой реакции |

### Статус

- [ ] Сценарий пройден

---

# CP-события (InjuryCare smoke)

**Общий протокол для HOI-CP-001…011:**

| # | Проверка | Как |
|---|----------|-----|
| 1 | **Запуск** | SMAPI `debug ebi <eventId>` после warp на location из таблицы |
| 2 | **Чёрный экран** | Fade ≤ ~3 с; esc/skip восстанавливает картинку |
| 3 | **NRE** | SMAPI log без `NullReferenceException` / необработанных exception |
| 4 | **Управление** | После `end` — игрок может ходить, меню работает |
| 5 | **Локация** | `get_player_info` — логичная карта (часто `Hospital` после rescue/collapse) |
| 6 | **Topics/buffs** | `injury_topic_dump` / `injury_buff_dump` — см. колонку «после события» |

**Перед каждым CP TC:** `injury_reset` + warp. Dating/married — для dating-вариантов (сейв, не только hearts).

### Сводная таблица CP TC

| ID | Event ID | Warp (StardewMCP) | InjuryCare | После события (topics/buffs) |
|----|----------|-------------------|------------|------------------------------|
| HOI-CP-001 | `eventHarveyMineRescueDating` | `warp_to_mine_floor` `10` | ✅ PassOutHandler | `topicMineInjuryRescue`; badly hurt; warp Hospital |
| HOI-CP-002 | `eventHarveyMineRescue` | Mine `(17,7)` | ✅ | как 001 без dating tone |
| HOI-CP-003 | `eventHarveyMinorMineRescue` | Mine | ✅ minor | `topicHarveyMinorMineRescue`; короче; Hospital |
| HOI-CP-004 | `eventHarveyMineInterception` | Mine `(17,7)` | ⚠️ CP trigger | `HarveyMineIntercept` (не `topic*`); exit move |
| HOI-CP-005 | `eventHarveySkullCavePrevention` | SkullCave `(5,5)` | ⚠️ CP trigger | warning topic; только SkullCave patch |
| HOI-CP-006 | `eventStayInHospital` | `Hospital` | ❌ orphan | короткая сцена `(9,16)`; state C# не меняет |
| HOI-CP-007 | `eventHarveyLateNightCollapse` | Town `(37,59)`, time ≥25:00 | ⚠️ CP trigger | `buffSleepy`, collapse; warp Hospital |
| HOI-CP-008 | `eventHarveyCheckHealthFarmer` | `Farm`, 6am–12pm | ❌ vanilla gate | mid-scene warp Hospital; dating |
| HOI-CP-009 | `eventHarveyEmergencyCare` | Hospital (опц.) | ✅ C# queue | critical pass-out path; `buffBadlyHurt` |
| HOI-CP-010 | `eventHarveyExhaustion` | Hospital (опц.) | ✅ C# queue | `buffFarmerExhausted`, `buffHarveyDropper` |
| HOI-CP-011 | `eventHarveyTreatmentCollapse` | Hospital | ❌ orphan | Teracitin text; fade end |

---

## HOI-CP-001 — eventHarveyMineRescueDating

### ID

HOI-CP-001

### Цель

Smoke: dating-вариант full mine rescue — cutscene без зависания; Hospital warp; bridge topic.

### Предусловие сейва

Harvey **dating/married** (`get_npc_info`).

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `warp_to_mine_floor` | `10` |
| `set_time` | `8am` |

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyMineRescueDating` |
| 2 | **вручную** | Просмотр / skip cutscene до `end` |
| 3 | **StardewMCP** | `get_player_info` → **`Hospital`**, валидный tile |
| 4 | **SMAPI** | `injury_topic_dump` → **`topicMineInjuryRescue`** (если C# pipeline уже наложил badly hurt) **или** topic добавлен событием |
| 5 | **SMAPI log** | Нет NRE; нет бесконечного fade |

**Примечание:** чистый `ebi` без pass-out state может не воспроизвести все C# flags — для полного pipeline см. [HOI-PASSOUT-002](10-mine-passout-tests.md).

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Event completes; player free; logical location | Black screen; NRE; soft-lock |
| | Viewport/warp на `(17,7)` Mine ломает сцену |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-002 — eventHarveyMineRescue

### ID

HOI-CP-002

### Цель

Smoke: neutral/full mine rescue (non-dating tone).

### Подготовка / шаги

Как HOI-CP-001, но:

- Сейв **без** dating (или hearts-only)
- `debug ebi eventHarveyMineRescue`

### Ожидаемый результат

- Аналогично 001; тон neutral; Hospital warp
- `topicMineInjuryRescue` / mail `mailHarveyAfterMineRescue` по script CP

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-003 — eventHarveyMinorMineRescue

### ID

HOI-CP-003

### Цель

Smoke: minor rescue — короче, dating, Hospital warp без full rescue severity.

### Предусловие

Dating/married.

### Подготовка (StardewMCP)

`warp_to_mine_floor` `10`

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyMinorMineRescue` |
| 2 | **вручную** | Cutscene complete |
| 3 | **SMAPI** | `injury_topic_dump` → **`topicHarveyMinorMineRescue`** |
| 4 | **StardewMCP** | `get_player_info` → Hospital |

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Короткая сцена OK; topic minor | Full rescue duplicate; NRE |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-004 — eventHarveyMineInterception

### ID

HOI-CP-004

### Цель

Smoke: interception при MineForbidden / warning — exit move, без black screen.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `warp_to_mine_floor` | `10` — игрок в **Mine** `(17,7)` |
| `set_time` | `10am` |

Опционально: активировать `HarveyMod_MineForbidden` (см. [HOI-MINE-005](10-mine-passout-tests.md)).

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffBadlyHurt
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyMineInterception` |
| 2 | **вручную** | Сцена: Harvey блокирует вход; move exit |
| 3 | **StardewMCP** | `get_player_info` → валидная локация (не void) |
| 4 | **SMAPI** | `injury_topic_dump` → ключ **`HarveyMineIntercept`** (legacy без `topic` prefix) |
| 5 | **SMAPI log** | Нет NRE |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-005 — eventHarveySkullCavePrevention

### ID

HOI-CP-005

### Цель

Smoke: Skull Cave warning event — только на карте **SkullCave** (известный trigger bug с Mine OR).

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `SkullCave`, `x`: `5`, `y`: `5` |
| `set_time` | `10am` |

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffFracturedBone
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveySkullCavePrevention` |
| 2 | **вручную** | Cutscene complete |
| 3 | **StardewMCP** | `get_player_info` → SkullCave или логичный warp после script |
| 4 | **SMAPI log** | Нет NRE / black screen |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-006 — eventStayInHospital

### ID

HOI-CP-006

### Цель

Smoke: orphan stay scene — **не** wired к C# hosp; проверка script integrity.

### Подготовка (StardewMCP)

`teleport_player` `Hospital`, `10am`

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventStayInHospital` |
| 2 | **вручную** | Короткая сцена у `(9,16)` |
| 3 | **SMAPI** | `injury_hospital_status` → **`IsHospitalized=False`** (orphan — C# hold **не** стартует) |
| 4 | **StardewMCP** | Player free после end |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-007 — eventHarveyLateNightCollapse

### ID

HOI-CP-007

### Цель

Smoke: late Town collapse (CP trigger path) — anim + Hospital warp.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `Town`, `37`, `59` |
| `set_time` | `2500` или `2600` |
| `pause_time` | `false` |

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyLateNightCollapse` |
| 2 | **вручную** | Collapse anim; dating/married tone variants |
| 3 | **StardewMCP** | После end → **`Hospital`** (или script location) |
| 4 | **SMAPI log** | Нет NRE |

**Cross-check C#:** natural Town pass-out → [HOI-PASSOUT-005](10-mine-passout-tests.md) (`buffSleepy`, `topicPassedOutInTown`).

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-008 — eventHarveyCheckHealthFarmer

### ID

HOI-CP-008

### Цель

Smoke: Farm morning check — mid-scene Hospital warp; vanilla `PlayerKilled` + dating gates.

### Предусловие

Dating Harvey; желательно `eventsSeen` содержит `PlayerKilled` (ванilla).

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `Farm` |
| `set_time` | `8am` |

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyCheckHealthFarmer` |
| 2 | **вручную** | Cutscene; warp Hospital mid-scene |
| 3 | **StardewMCP** | `get_player_info` после end |
| 4 | **SMAPI log** | Нет NRE / stuck fade |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-009 — eventHarveyEmergencyCare

### ID

HOI-CP-009

### Цель

Smoke: C# hospital pass-out **critical** event (`PassOutHandler.QueueHospitalEvent`).

### Подготовка (StardewMCP)

`teleport_player` `Hospital` (optional — script may `changeLocation`)

### Команды SMAPI / Injury MCP

```
injury_reset
```

Опционально gameplay setup: [HOI-PASSOUT-001](10-mine-passout-tests.md) → pending queue.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyEmergencyCare` |
| 2 | **вручную** | Cutscene complete |
| 3 | **SMAPI** | После gameplay path: `buffBadlyHurt`, `PendingHospitalPassOutEventId` cleared |
| 4 | **StardewMCP** | Logical Hospital location |
| 5 | **SMAPI log** | `[PassOutEvent]` без exception |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-010 — eventHarveyExhaustion

### ID

HOI-CP-010

### Цель

Smoke: exhaustion hospital event — long scene; `buffHarveyDropper` / exhaustion topic при gameplay path.

### Подготовка / команды

Как HOI-CP-009.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyExhaustion` |
| 2 | **вручную** | Длинная сцена до end |
| 3 | **SMAPI** | Gameplay path: `topicFarmerExhausted`, `buffFarmerExhausted` ([HOI-PASSOUT-004](10-mine-passout-tests.md)) |
| 4 | **SMAPI log** | Нет NRE |

### Статус

- [ ] Сценарий пройден

---

## HOI-CP-011 — eventHarveyTreatmentCollapse

### ID

HOI-CP-011

### Цель

Smoke: **orphan** treatment collapse — Teracitin dialogue; fade end; не ломает save.

### Подготовка (StardewMCP)

`teleport_player` `Hospital`

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI debug** | `debug ebi eventHarveyTreatmentCollapse` |
| 2 | **вручную** | Текст Teracitin; fade out/in |
| 3 | **SMAPI** | `injury_hospital_status` → not hospitalized (orphan) |
| 4 | **StardewMCP** | Player control restored |

### Статус

- [ ] Сценарий пройден

---

## Сводка: StardewMCP vs SMAPI по TC

| ID | StardewMCP | SMAPI / Injury MCP | Вручную |
|----|------------|-------------------|---------|
| HOI-HOSP-001 | teleport Hospital | reset, topic_add, debuff_add, hospital_status | hosp dialog |
| HOI-HOSP-002 | get_player_info, walkable, surroundings | setup hosp | — |
| HOI-HOSP-003 | teleport Town (exit attempt) | hospital_status | HUD, return dialog |
| HOI-HOSP-004 | set_time + exit | hospital_status, buff_dump | discharge dialog |
| HOI-HOSP-005 | set_time × N | setup hosp | activity dialogs |
| HOI-HOSP-006 | get_player_info | hospital_discharge, dumps | — |
| HOI-HOSP-007 | teleport, set_time | setup, debug ebi, hospital_status | cutscene, exit |
| HOI-PROX-001…004 | Hospital/Farm warp | debuff_add, state_dump | walk near Harvey |
| HOI-CP-001…011 | warp по таблице | reset, dumps | **debug ebi**, cutscene |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC, запись PASS/FAIL.
2. **Этот файл** — [11-hospital-proximity-events-tests.md](11-hospital-proximity-events-tests.md) — незакрытые HOI-HOSP-* / HOI-PROX-* / HOI-CP-* из журнала.
3. **Следующий артеfact: `TESTING_INDEX.md`** — финальный индекс всей папки `docs/testing/`: ссылки на чеклисты 00–11, порядок прогона, матрица «TC → команды → MCP», статус блокеров (dating save, `set_stamina`, cutscene-only PASS).
4. [10-mine-passout-tests.md](10-mine-passout-tests.md) — перекрёстные HOI-PASSOUT-* для mine rescue / forced hosp pipeline.
5. [05-debug-dump-commands.md](05-debug-dump-commands.md) · [06-debug-setup-commands.md](06-debug-setup-commands.md) — assert/setup для hospital и topics.
6. [02-cp-content-inventory.md](02-cp-content-inventory.md) · [docs/events-inventory/01-cp-events-catalog.md](../events-inventory/01-cp-events-catalog.md) — детали CP scripts beyond smoke.
7. **Блокеры:** Harvey placement для PROX; `HospitalActivityIntervalMinutes` default 40 vs TC 20; orphan events (006, 011); save/load resume hospital queue — [07-smoke-save-tests.md](07-smoke-save-tests.md).
