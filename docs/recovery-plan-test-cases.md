# Тест-кейсы: План восстановления Харви (Recovery Plan)

Документ для ручной и полуавтоматической проверки фичи **«План восстановления Харви»** после выписки из госпитализации.

Связанные материалы:
- UI / архитектура: [`recovery-plan-stardewui-research.md`](recovery-plan-stardewui-research.md)
- Госпитализация (предусловие выписки): [`testing/scenarios/06-hospitalization.json`](testing/scenarios/06-hospitalization.json)
- Injury MCP: [`testing/injury-mcp.md`](testing/injury-mcp.md)
- StardewMCP: [`testing/stardew-mcp.md`](testing/stardew-mcp.md)

---

## Название механики

**План восстановления** — после выписки из госпитализации Харви назначает 3-дневный щадящий режим. Игрок должен избегать шахты, истощения, обмороков и критического HP/энергии. Каждый **спокойный** игровой день (оценка в **конце дня**, перед сном) засчитывается в прогресс `CompletedDays / RequiredDays`. При срыве день не засчитывается. После 3 зачтённых дней — состояние «поговорить с Харви» и CP-реплики по conversation topic.

---

## Результаты статической проверки (код + сборка)

| # | Проверка | Статус | Комментарий |
|---|----------|--------|-------------|
| 1 | Сборка C# | **PASS** | `dotnet build` — 0 ошибок |
| 2 | JSON CP | **PASS** | `dialoguesHarveyRecoveryPlan.json` — strict JSON OK |
| 3 | Старый сейв без `ActiveRecoveryPlan` | **PASS*** | `StateManager.EnsureRecoveryPlanState()` выходит при `null`; *нужен ручной load старого сейва* |
| 4 | Старт после выписки | **PASS*** | `HospitalizationManager.Discharge()` → `StartHospitalDischargePlan()` |
| 5 | Нет повторного старта | **PASS** | `HasActivePlan()` блокирует перезапись |
| 6 | Hotkey UI | **PASS*** | `RecoveryPlanKey = "H"` (`ModConfig`); *нужен in-game тест* |
| 7 | UI без плана | **PASS** | HUD «Сейчас нет активного…», без `CreateMenuFromAsset` |
| 8 | Прогресс в UI | **PASS*** | `ProgressText = CompletedDays / RequiredDays`; зачёт только после сна |
| 9 | Шахта срывает день | **PASS*** | `CheckViolationOnLocationEntry` при warp в Mine |
| 10 | Сорванный день не засчитывается | **PASS** | `OnDayEnding`: `TodayFailed` → без `CompletedDays++` |
| 11 | Спокойный день засчитывается | **PASS** | `!TodayFailed` → `CompletedDays++` |
| 12 | «Поговорить с Харви» | **PASS*** | `CompletionTalkPending`, `RequiresHarveyTalk` после 3-го зачёта |
| 13 | CP-реплики по topic | **PASS*** | C# ставит topics; CP ключи совпадают |
| 14 | HUD не спамится | **PASS** | Нарушение: HUD только при `firstFailureToday`; день: `LastEvaluatedDay` |
| 15 | Save/load | **PASS*** | `InjuryState.ActiveRecoveryPlan` в save data |

---

## Известные проблемы (не критичные — не блокируют релиз)

| ID | Severity | Описание |
|----|----------|----------|
| RP-GAP-001 | Low | `CompletionTalkPending` / `RequiresHarveyTalk` **не сбрасываются** после разговора с Харви — UI остаётся в режиме «поговори с Харви», пока не вызвать `recovery_plan_clear` или не очистить state вручную. |
| RP-GAP-002 | Low | Topics `HarveyMod_RecoveryPlan*` **не попадают** в блок `ACTIVE TOPICS` в `injury_debug_dump` — фильтр `IsModConversationTopic` не включает префикс `HarveyMod_RecoveryPlan`. Проверять через `recovery_plan_status` или `Game1.player.activeDialogueEvents` вручную. |
| RP-GAP-003 | Info | В compact debug HUD (F10) **нет блока Recovery Plan** — только консоль `recovery_plan_status`. |
| RP-GAP-004 | Info | Зачёт дня срабатывает в **`DayEnding`** (сон), а не в полночь. В UI прогресс обновляется после пробуждения. |

Критических крашей и ошибок сборки **не обнаружено**.

---

## Предусловия

| Условие | Зачем |
|---------|-------|
| **HarveyOverhaulInjury** (собранный DLL) | C# логика |
| **HarveyOverhaul [CP]** | Реплики `dialoguesHarveyRecoveryPlan.json` |
| **focustense.StardewUI** ≥ 0.6.1 | Окно плана (hotkey **H**) |
| Загруженный сейв | Все команды требуют `Context.IsWorldReady` |
| Для выписки «по-настоящему» | Госпитализация: `ForceHospitalization=true`, Severe-травма, `injury_hospital_discharge` |

### Поля save-state (`InjuryState.ActiveRecoveryPlan`)

| Поле | Назначение |
|------|------------|
| `IsActive` | План в процессе |
| `PlanId` | `RecoveryPlan_HospitalDischarge` |
| `Reason` | `hospital` |
| `RequiredDays` / `CompletedDays` | Цель / зачтённые дни (обычно 3) |
| `TodayFailed` | Срыв текущего дня |
| `TodayViolationReasons` | Причины нарушений за сегодня |
| `CompletionTalkPending` | План завершён, ждёт разговора |
| `RequiresHarveyTalk` | Флаг «нужен разговор» |
| `LastEvaluatedDay` | Защита от двойного зачёта за день |

### Conversation topics (C# → CP)

| Topic ID | Когда ставится | Срок |
|----------|----------------|------|
| `HarveyMod_RecoveryPlanStarted` | Старт плана | 3 д |
| `HarveyMod_RecoveryPlanViolated` | Первое нарушение дня | 2 д |
| `HarveyMod_RecoveryPlanCompleted` | 3 зачтённых дня | 3 д |

### Причины нарушений (`RegisterViolation`)

| reason | Триггер |
|--------|---------|
| `mine` | Вход в Mine / UndergroundMine / MineShaft |
| `skull_cave` | Skull Cave |
| `low_health` | HP ≤ 10 (throttle ~2 с) |
| `low_stamina` | Stamina ≤ 15% max (throttle ~1 с) |
| `passout` | `WasPassedOut` перед сном |

---

## Команды для подготовки

### SMAPI-консоль (основные)

```text
recovery_plan_start [injuryId]     # QA-старт (как после выписки)
recovery_plan_fail <reason>        # Симуляция нарушения
recovery_plan_status               # Снимок состояния
recovery_plan_clear                # Полный сброс плана

injury_reset                       # Чистый лист мода
injury_hospital_status             # Статус госпитализации
injury_hospital_discharge          # Выписка → должен стартовать план
injury_debug_dump                  # Полный отчёт (F10 full через MCP)
```

### Injury MCP (`user-harvey-injury`)

```text
injury_reset
injury_debuff_add { buff_id: "buffBadlyHurt" }
injury_hospital_discharge          # только если IsHospitalized=true
injury_debug_dump
```

### StardewMCP (`user-stardew`)

```text
teleport_player { location: "Mine", x: 17, y: 7 }   # срыв через шахту
teleport_player { location: "Town", x: 43, y: 68 } # к клинике / Харви
set_health { amount: 5 }                             # low_health (осторожно)
get_player_info                                      # локация, HP, день
```

### Hotkey UI

- По умолчанию: **H** (`ModConfig.RecoveryPlanKey`)
- Требует StardewUI; без мода — HUD «Сейчас нет активного плана…»

---

## Чеклист тест-кейсов

### TC-RP-00 — Сборка и контент (smoke)

- [ ] **Действие:** `dotnet build HarveyOverhaulInjury.csproj`
- [ ] **Ожидание:** 0 errors; DLL деплоится в `Mods/HarveyOverhaul/HarveyOverhaulInjury`
- [ ] **UI:** —
- [ ] **SMAPI log:** —
- [ ] **Команды:** локальная сборка

- [ ] **Действие:** проверить CP `assets/Code/dialoguesHarveyRecoveryPlan.json` + `content.json` Include
- [ ] **Ожидание:** JSON парсится; 3 topic × 5 tier (0–2 / 3–5 / 6–10 / Dating / Married)
- [ ] **UI:** —
- [ ] **SMAPI log:** CP без ошибок при загрузке мода
- [ ] **Команды:** —

---

### TC-RP-01 — Старый сейв без RecoveryPlan не падает

- [ ] **Действие:** загрузить сейв, созданный **до** появления `ActiveRecoveryPlan`
- [ ] **Ожидание:** игра и SMAPI без exception; день начинается нормально
- [ ] **UI:** **H** → HUD «Сейчас нет активного плана восстановления»
- [ ] **SMAPI log:** нет `[RecoveryPlan]` error; `[RecoveryPlan] Новый день` **не** появляется
- [ ] **Команды:** `recovery_plan_status` → `(none)`

---

### TC-RP-02 — План стартует после выписки

- [ ] **Подготовка:** `injury_reset` → Severe (`buffBadlyHurt`) → госпитализация → `injury_hospital_discharge`
- [ ] **Действие:** выписка через `HospitalizationManager.Discharge()` (QA или игровой flow)
- [ ] **Ожидание:**
  - HUD: «Харви назначил план восстановления на 3 дня.»
  - `ActiveRecoveryPlan.IsActive = true`
  - topic `HarveyMod_RecoveryPlanStarted` (3 д)
- [ ] **UI:** **H** → «Прогресс: 0 / 3», «Сегодня режим соблюдается»
- [ ] **SMAPI log:**
  ```text
  [RecoveryPlan] Старт RecoveryPlan_HospitalDischarge: injury=..., required=3d
  [RecoveryPlan] План восстановления после выписки: injury=...
  ```
- [ ] **Команды:** `injury_hospital_status` → `IsHospitalized=false`; `recovery_plan_status`

**Быстрый QA-путь (без полной госпитализации):**

- [ ] `recovery_plan_start buffBadlyHurt` — эквивалент старта после выписки

---

### TC-RP-03 — План не стартует повторно поверх активного

- [ ] **Подготовка:** активный план (`recovery_plan_start`)
- [ ] **Действие:** повторно `recovery_plan_start` или вторая `injury_hospital_discharge`
- [ ] **Ожидание:**
  - `[RecoveryPlan] План не запущен (уже активен или ошибка).`
  - `CompletedDays`, `StartDay` **не** сбрасываются
  - Второй HUD старта **не** появляется
- [ ] **UI:** прогресс без изменений
- [ ] **SMAPI log:** `[RecoveryPlan] Пропуск старта: уже активен план ...` (Debug)
- [ ] **Команды:** `recovery_plan_start` × 2

---

### TC-RP-04 — UI открывается по hotkey

- [ ] **Подготовка:** активный план; StardewUI установлен; игрок свободен (не в меню/event)
- [ ] **Действие:** нажать **H**
- [ ] **Ожидание:** открывается StardewUI-окно «План восстановления Харви»
- [ ] **UI:** заголовок, тип «После выписки», прогресс, правила, статус дня
- [ ] **SMAPI log:** `[RecoveryPlanUI] Окно плана восстановления открыто.` (Debug)
- [ ] **Команды:** `recovery_plan_start` → **H**

---

### TC-RP-05 — UI без плана не крашится

- [ ] **Подготовка:** `recovery_plan_clear` или сейв без плана
- [ ] **Действие:** нажать **H**
- [ ] **Ожидание:** **нет** exception; меню StardewUI **не** открывается
- [ ] **UI:** HUD «Сейчас нет активного плана восстановления.»
- [ ] **SMAPI log:** без stack trace
- [ ] **Команды:** `recovery_plan_clear` → **H**

**Дополнительно (StardewUI отсутствует):**

- [ ] Отключить StardewUI → **H** → тот же HUD, Warn `[RecoveryPlanUI] StardewUI API недоступен`

---

### TC-RP-06 — UI прогресс 0/3 → 1/3 → 2/3 → 3/3

> Зачёт дня — в **конце дня** (`OnDayEnding`), после сна прогресс виден в UI.

| Шаг | Действие | UI (`ProgressText`) | `recovery_plan_status` |
|-----|----------|---------------------|-------------------------|
| A | Сразу после старта, до сна | `0 / 3` | `completed=0/3` |
| B | 1-й спокойный день → сон | `1 / 3` | `completed=1/3` |
| C | 2-й спокойный день → сон | `2 / 3` | `completed=2/3` |
| D | 3-й спокойный день → сон | `3 / 3` + статус «План завершён — поговори с Харви» | `active=false`, `completionTalkPending=true` |

- [ ] **Ожидание на каждом шаге:** HUD «План восстановления: день зачтён.» (кроме шага A)
- [ ] **SMAPI log:** `[RecoveryPlan] День зачтён: N/3`
- [ ] **Команды:** `recovery_plan_start` → 3× (спокойный день + сон); между днями **не** заходить в шахту

---

### TC-RP-07 — Вход в шахту срывает день

- [ ] **Подготовка:** активный план, новый игровой день
- [ ] **Действие:** `teleport_player Mine 17 7` или лифт на Mountain → Mine
- [ ] **Ожидание:**
  - HUD: «План восстановления: режим сорван.»
  - `TodayFailed = true`
  - `TodayViolationReasons` содержит `mine`
  - topic `HarveyMod_RecoveryPlanViolated`
- [ ] **UI:** «Сегодня режим сорван»; строка нарушений `mine`
- [ ] **SMAPI log:**
  ```text
  [RecoveryPlan] Нарушение (mine, serious=True): today=1, total=...
  ```
- [ ] **Команды:** `recovery_plan_start` → warp Mine

**Skull Cave:**

- [ ] warp в Skull Cave → reason `skull_cave`

---

### TC-RP-08 — Сорванный день не засчитывается

- [ ] **Подготовка:** активный план; днём срыв (шахта или `recovery_plan_fail mine`)
- [ ] **Действие:** лечь спать
- [ ] **Ожидание:**
  - `CompletedDays` **не** увеличился
  - HUD зачёта **нет**
  - На следующий день `TodayFailed` сброшен (`OnDayStarted`)
- [ ] **UI:** после сна прогресс прежний (напр. `0/3` или `1/3`)
- [ ] **SMAPI log:** `[RecoveryPlan] День не зачтён (нарушения: mine)`
- [ ] **Команды:** `recovery_plan_fail mine` → сон → `recovery_plan_status`

---

### TC-RP-09 — Спокойный день засчитывается

- [ ] **Подготовка:** активный план; **без** нарушений за день
- [ ] **Действие:** сон
- [ ] **Ожидание:** `CompletedDays++`, `TodayCompleted = true`, HUD «день зачтён»
- [ ] **UI:** прогресс +1
- [ ] **SMAPI log:** `[RecoveryPlan] День зачтён: N/3`
- [ ] **Команды:** `recovery_plan_start` → сон (без шахты)

---

### TC-RP-10 — После 3 зачтённых дней — «поговорить с Харви»

- [ ] **Подготовка:** 3 спокойных дня подряд
- [ ] **Действие:** сон после 3-го зачёта
- [ ] **Ожидание:**
  - HUD: «План восстановления завершён. Поговори с Харви.»
  - `IsActive = false`
  - `CompletionTalkPending = true`, `RequiresHarveyTalk = true`
  - topic `HarveyMod_RecoveryPlanCompleted`
- [ ] **UI:** «План завершён — поговори с Харви»; прогресс `3 / 3`
- [ ] **SMAPI log:**
  ```text
  [RecoveryPlan] План завершён — ожидается разговор с Харви
  [RecoveryPlan] Итог: completed=3, violations=...
  ```
- [ ] **Команды:** `recovery_plan_status`

---

### TC-RP-11 — Реплики Харви по conversation topic

Проверить **отдельно** для tier (0–2 ❤ / 3–5 / dating / married):

| Topic | Когда | Ожидание при клике по Harvey |
|-------|-------|------------------------------|
| `HarveyMod_RecoveryPlanStarted` | После старта | Реплика про 3 дня щадящего режима |
| `HarveyMod_RecoveryPlanViolated` | После 1-го нарушения дня | Реплика «осмотр, потом лекция» |
| `HarveyMod_RecoveryPlanCompleted` | После завершения | Реплика о снятии режима |

- [ ] **Действие:** телепорт к Harvey (`Town` / `Hospital`), поговорить
- [ ] **UI:** —
- [ ] **SMAPI log:** topic в `activeDialogueEvents` (проверка через `recovery_plan_status` + ручной dialogue)
- [ ] **Команды:**
  ```text
  recovery_plan_start          # → Started
  recovery_plan_fail mine      # → Violated
  # ... 3 спокойных дня ...    # → Completed
  ```

**StardewMCP:** `teleport_player` к Harvey; hearts — через friendship tools при необходимости.

---

### TC-RP-12 — HUD-сообщения не спамятся

**Нарушения:**

- [ ] **Действие:** 2+ нарушения **одной** причины за день (`recovery_plan_fail mine` × 2)
- [ ] **Ожидание:** HUD «режим сорван» **один раз**; второй вызов — только log
- [ ] **SMAPI log:** второй вызов без нового HUD

**Разные причины в один день:**

- [ ] `recovery_plan_fail mine` → `recovery_plan_fail low_stamina`
- [ ] **Ожидание:** HUD срыва **один** (при первом); обе причины в `TodayViolationReasons`

**Зачёт дня:**

- [ ] **Действие:** двойной вызов `OnDayEnding` (теоретически) / перезагрузка после сна
- [ ] **Ожидание:** `[RecoveryPlan] День уже оценён — пропуск` при повторной оценке того же дня

---

### TC-RP-13 — План сохраняется после save/load

- [ ] **Подготовка:** активный план с прогрессом (напр. `1/3`, `TodayFailed=true`)
- [ ] **Действие:** сохранить → выйти в меню → загрузить сейв
- [ ] **Ожидание:** все поля `ActiveRecoveryPlan` восстановлены; active topics на месте
- [ ] **UI:** **H** показывает тот же прогресс и статус
- [ ] **SMAPI log:** после load — `[RecoveryPlan] Новый день` только при смене дня, без ошибок
- [ ] **Команды:** `recovery_plan_status` до и после reload

---

## Дополнительные edge-case сценарии

### TC-RP-E1 — Нарушение через low HP / stamina

- [ ] `set_health 5` (при активном плане) → `low_health` в reasons
- [ ] Истощение stamina ≤ 15% → `low_stamina` (подождать ~1 с in-game)

### TC-RP-E2 — Обморок (passout)

- [ ] Симуляция pass-out с `WasPassedOut=true` перед сном → reason `passout`
- [ ] Сложный сценарий; при QA проще: `recovery_plan_fail passout`

### TC-RP-E3 — UI после завершения (CompletionTalkPending)

- [ ] После TC-RP-10 нажать **H**
- [ ] **Ожидание:** окно **открывается** (`HasPlanToDisplay`: active **или** completion pending)
- [ ] Hint: «Поговори с Харви, чтобы снять режим.»

### TC-RP-E4 — `recovery_plan_clear`

- [ ] После clear: `recovery_plan_status` → `(none)`; **H** → HUD «нет активного плана»

---

## Матрица: пункты задачи → тест-кейсы

| # из ТЗ | Тест-кейс |
|---------|-----------|
| 1 | TC-RP-00 |
| 2 | TC-RP-00 |
| 3 | TC-RP-01 |
| 4 | TC-RP-02 |
| 5 | TC-RP-03 |
| 6 | TC-RP-04 |
| 7 | TC-RP-05 |
| 8 | TC-RP-06 |
| 9 | TC-RP-07 |
| 10 | TC-RP-08 |
| 11 | TC-RP-09 |
| 12 | TC-RP-10 |
| 13 | TC-RP-11 |
| 14 | TC-RP-12 |
| 15 | TC-RP-13 |

---

## Быстрый smoke-прогон (≈15 мин)

```text
1. injury_reset
2. recovery_plan_start buffBadlyHurt
3. recovery_plan_status          # active=true, 0/3
4. [H]                           # UI 0/3
5. recovery_plan_fail mine       # HUD срыв
6. [сон]                         # не зачтён
7. recovery_plan_status          # still 0/3
8. [H]                           # «режим сорван» → новый день → «соблюдается»
9. [3× спокойный день + сон]
10. recovery_plan_status         # completionTalkPending=true
11. [H]                          # 3/3, поговори с Харви
12. Поговорить с Harvey          # Completed topic
13. save → load → recovery_plan_status  # state на месте
14. recovery_plan_clear
```

---

## Журнал прогона

| TC ID | Дата | Результат | Заметки |
|-------|------|-----------|---------|
| TC-RP-00 | | [ ] PASS / FAIL | |
| TC-RP-01 | | [ ] | |
| … | | | |
