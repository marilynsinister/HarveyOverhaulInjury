# Шахта (MineForbidden) и обмороки (PassOut) — чеклист QA

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Механики C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) · CP: [02-cp-content-inventory.md](02-cp-content-inventory.md)  
> Setup: [06-debug-setup-commands.md](06-debug-setup-commands.md) · Assert: [05-debug-dump-commands.md](05-debug-dump-commands.md)  
> MCP: [stardew-mcp.md](stardew-mcp.md) · [injury-mcp.md](injury-mcp.md)

**Область:** запрет шахты (`HarveyMod_MineForbidden`, Severe warning), пайплайн pass-out / mine rescue / town collapse / exhaustion.  
**Не цель:** dirty wound exposure, полный прогон MainInjury — см. [09-complication-tests.md](09-complication-tests.md), [main-injury-testcases.md](main-injury-testcases.md).

Отмечайте `- [ ]` → `- [x]` по мере проверки.

---

## Журнал прогона

| Поле | Значение |
|------|----------|
| Тестер | |
| Слот сохранения | |
| Версия C# мода | |
| Версия CP | |
| `SendLetters` в config | |
| `ForceHospitalization` в config | |
| `MineForbiddenDurationDays` в config | |
| Harvey: dating/married (для PassOut TC) | |
| Дата | |

| ID | Сценарий | Статус | Заметки |
|----|----------|:------:|---------|
| HOI-MINE-001 | Severe + вход в шахту → mail + MineForbidden | [ ] | |
| HOI-MINE-002 | Light injury — мягкий warning, без запрета | [ ] | |
| HOI-MINE-003 | Повторный вход — warning не чаще раза в день | [ ] | |
| HOI-MINE-004 | MineForbidden истекает по `MineForbiddenDurationDays` | [ ] | |
| HOI-MINE-005 | MineForbidden + CP interception event | [ ] | |
| HOI-MINE-010 | buffDeepCuts + Mine — мягкий HUD, без выноса | [ ] | |
| HOI-MINE-011 | buffDeepCuts + DirtyWound + смена этажа | [ ] | |
| HOI-MINE-012 | buffBadlyHurt — строгий путь (письмо + запрет) | [ ] | |
| HOI-MINE-013 | MineForbidden — HUD со сроком при входе | [ ] | |
| HOI-PASSOUT-001 | HP 0–10 + dating → buffBadlyHurt | [ ] | |
| HOI-PASSOUT-002 | Mine death rescue pipeline | [ ] | |
| HOI-PASSOUT-003 | topicMineInjuryRescue → forced hospital | [ ] | |
| HOI-PASSOUT-004 | Exhaustion (stamina ≤ −15) | [ ] | |
| HOI-PASSOUT-005 | Late Town collapse | [ ] | |

---

## Предусловия (все TC)

- [ ] Игра через SMAPI, загружен тестовый сейв (`Context.IsWorldReady`)
- [ ] C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`
- [ ] `injury_validate_buffs` → `result=OK` (см. [07-smoke-save-tests.md](07-smoke-save-tests.md))
- [ ] Перед **каждым** изолированным TC: **`injury_reset`** (SMAPI / Injury MCP)
- [ ] **`NeedsMineRescueEvent=False`** после reset (иначе Severe warning в шахте подавлен)

### Config (записать в журнал)

| Параметр | Влияние на TC |
|----------|----------------|
| `SendLetters: true` | HOI-MINE-001, HOI-PASSOUT-005 — письма на утро |
| `MineForbiddenDurationDays` (default **2**) | HOI-MINE-004 — срок дебаффа |
| `ForceHospitalization: true` | HOI-PASSOUT-003 — вход в Hospital с `topicMineInjuryRescue` |
| `HospitalLocationName` (обычно `Hospital`) | HOI-PASSOUT-003 |

### Сейв для PassOut (HOI-PASSOUT-001…005)

**StardewMCP не выставляет флаг dating.** Нужен сейв, где `Harvey` — **dating или married** (`friendship.IsDating()` / `IsMarried()`).

Проверка:

| Кто | Команда / tool |
|-----|----------------|
| **StardewMCP** | `get_npc_info` → `Harvey` — статус отношений |
| **SMAPI** | при FAIL — завести отдельный «dating save» или вручную дать букет / свадьбу |

### Общая подготовка мира (StardewMCP)

| Tool | Аргументы | Зачем |
|------|-----------|-------|
| `pause_time` | `true` | Стабильный прогон без сдвига часов |
| `set_time` | `10am` | Дневной вход в шахту / клинику |
| `set_npc_relationship` | `Harvey`, `8` | Сердечки (не заменяет dating) |
| `warp_to_mine_floor` | `10` | Локация `Mine` / `UndergroundMine` |
| `teleport_player` | `Hospital` | Forced hosp., cutscenes |
| `teleport_player` | `Town`, опц. `x`: `37`, `y`: `59` | Late collapse (см. CP) |
| `set_health` | `0` … `10` | Pass-out / mine death |
| `set_health` | `99999` | Сброс HP между подшагами |
| `advance_day` | — | Сон, DayStarted, mail, rescue |
| `get_game_time` | — | Assert день / время |
| `get_player_info` | — | HP, stamina, локация |

### Assert после мутаций (SMAPI / Injury MCP)

```
injury_state_dump
injury_buff_dump
injury_topic_dump
injury_phase_list
```

### Полезные лог-префиксы

| Префикс | Когда |
|---------|-------|
| `⚠️ [Шахта]` / `[Шахта]` | Severe warning, mail, MineForbidden |
| `[MineForbidden]` | Запрет, interception, warp |
| `[MineRescue]` | Death / rescue / topic |
| `[PassOutEvent]` | Hospital pass-out cutscenes |
| `[PassOut]` | Resume после save/load |
| `🏥 Обнаружена телепортация после обморока` | OnPlayerWarped pass-out branch |

### Разделение инструментов (правило чата)

| Действие | Инструмент |
|----------|------------|
| Телепорт, время, дата, HP, погода, этаж шахты, сердечки | **StardewMCP** (`user-stardew`) |
| `injury_*`, QA setup/dump, `injury_debug_mine_rescue`, `injury_mine_forbidden_clear` | **SMAPI console** или **Injury MCP** (`user-harvey-injury`) |
| `debug ebi <eventId>` | **SMAPI / игровая debug-консоль** (не StardewMCP, не Injury MCP) |
| Клик Harvey, просмотр cutscene, почтовый ящик UI | **вручную**, если не указано иное |

---

# Правила поведения шахты (сводка)

| Уровень | Травмы / условие | При входе в MineShaft / Volcano | MineWarningDay | Письмо / запрет | Вынос / cutscene |
|---------|------------------|--------------------------------|-----------------|-----------------|------------------|
| **Мягкий** | Любая не-Severe (`buffDeepCuts`, `buffHurt`, LimitedActivity, осложнения без Severe main) | HUD 1×/день: *«Будь осторожна…»* или LimitedActivity-текст | **не ставится** | **нет** | **нет**; DirtyWound по экспозиции (`DirtyInMines`) |
| **Severe** | `InjurySets.Severe` как MainInjury | 1-й вход: строгий HUD + `MineWarningDay=today`; 2-й вход в тот же день: warp | **да** | На след. день: `mailHarveyMineForbidden` + `HarveyMod_MineForbidden` | C# warp; **не** CP event до запрета |
| **Запрет** | `HarveyMod_MineForbidden` | HUD *«Запрет… Осталось: X дн.»* + warp; cutscene `eventHarveyMineInterception` 1×/день (C# или CP) | — | уже было | **да** |

**CP `triggerHarveyMineWarning`:** только `HarveyMod_MineForbidden` + локация `Mine` + `!HarveyMineIntercept` + `!topicMineRescuePending`. **Не** срабатывает на `buffDeepCuts` без запрета.

**Не путать:** `eventHarveyMinorMineRescue` — отдельный pass-out/rescue pipeline; **не** вызывается автоматически при обычном входе в шахту с лёгкой травмой.

---

# MineForbidden

Цепочка Severe (C# `PlayerEventHandler.TryHandleSevereMineEntry` + `GameEventHandler`):

```text
Вход в Mine с MainInjury ∈ Severe
  → HUD error (первый раз за день) + MineWarningDay=today
  → DayEnding: mailHarveyMineForbidden на завтра (если SendLetters)
  → DayStarted (след. день): HarveyMod_MineForbidden + MineForbiddenAppliedDay=today
  → Повторный вход в тот же день с Forbidden: eventHarveyMineInterception (1×/день) или HUD+warp
  → DayStarted (appliedDay + MineForbiddenDurationDays): снятие дебаффа
```

**Severe ID:** `buffBadlyHurt`, `buffFracturedBone`, `buffConcussion`, `buffInfectedWound`, `buffBurnWounds`, `buffShrapnelWounds`, `buffSurgicalWound` — см. `InjurySets.Severe`.

---

## HOI-MINE-001 — Severe injury + вход в шахту

### ID

HOI-MINE-001

### Цель

Первый вход в шахту с **Severe** main (`buffBadlyHurt`) даёт строгое предупреждение; после сна — письмо `mailHarveyMineForbidden` и дебафф `HarveyMod_MineForbidden`.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `pause_time` | `true` |
| `set_time` | `10am` |

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffBadlyHurt
injury_state_dump
injury_phase_list
```

Ожидание до шахты: `MainInjuryId=buffBadlyHurt`, `NeedsMineRescueEvent=False`.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` (или `teleport_player` `Mine`) |
| 2 | **вручную / HUD** | Дождаться HUD: *«У тебя серьёзные раны — ты не должна идти в шахту!»* (`HUDMessage.error_type`) |
| 3 | **SMAPI** | `injury_state_dump` → `LastMineSevereWarningDay=<today>`, `MineWarningDay=<today>` |
| 4 | **StardewMCP** | `advance_day` (сон / следующий день) |
| 5 | **SMAPI** | `injury_buff_dump` → `HarveyMod_MineForbidden` |
| 6 | **SMAPI** | `injury_state_dump` → `MineForbiddenAppliedDay=<today>`, `MineWarningDay=-1` (сброшен при apply) |
| 7 | **вручную** | Утром проверить почту: базовый id **`mailHarveyMineForbidden`** (tier suffix по сердечкам) |

### Ожидаемый результат

- Первый вход: Severe HUD + `MineWarningDay` / `LastMineSevereWarningDay` = текущий день
- После `advance_day`: buff **`HarveyMod_MineForbidden`**, письмо **`mailHarveyMineForbidden`**
- SMAPI log: `[Шахта] Наложен дебафф «Харви запретил шахту»`, письмо запланировано на вечер предыдущего дня

### Debug HUD (F10) / log

- F10: активная травма `buffBadlyHurt` + служебный MineForbidden после дня 2
- Log: `⚠️ [Шахта] Вход с серьёзными ранами`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Warning при первом входе; после сна — buff + mail | Нет warning; нет mail при `SendLetters=true`; нет `HarveyMod_MineForbidden` |
| `MineWarningDay` выставлен до сна | `buffHurt` или `buffDeepCuts` вызывают ту же цепочку (ложное Severe) |

### Статус

- [ ] Сценарий пройден

---

## HOI-MINE-002 — Light injury + вход в шахту

### ID

HOI-MINE-002

### Цель

`buffHurt` или **`buffDeepCuts`** (не ∈ `InjurySets.Severe`) даёт **мягкий** HUD; **не** выставляются `MineWarningDay`, письмо и `HarveyMod_MineForbidden`. Расширенный прогон DeepCuts: **HOI-MINE-010** / **HOI-MINE-011**.

### Подготовка (StardewMCP)

`pause_time` `true`, `set_time` `10am`

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffHurt
injury_state_dump
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` |
| 2 | **вручную / HUD** | Мягкий HUD: *«Будь осторожна в шахте — твои раны могут загрязниться.»* (`health_type`, не error Severe) |
| 3 | **SMAPI** | `injury_state_dump` → `MineWarningDay=-1`, `LastMineSevereWarningDay=-1` |
| 4 | **StardewMCP** | `advance_day` |
| 5 | **SMAPI** | `injury_buff_dump` — **нет** `HarveyMod_MineForbidden` |
| 6 | **SMAPI** | `injury_topic_dump` / почта — **нет** `mailHarveyMineForbidden` |

### Ожидаемый результат

- `MainInjuryId=buffHurt` сохраняется
- **MineForbidden** не появляется

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Мягкий текст; state mine fields = −1 / empty | Severe-текст, `MineWarningDay≥0`, mail или `HarveyMod_MineForbidden` |

### Статус

- [ ] Сценарий пройден

---

## HOI-MINE-003 — Повторный вход в шахту (не спамить warning)

### ID

HOI-MINE-003

### Цель

Предупреждение **не дублируется** при повторном входе **в тот же игровой день** (light и Severe — разные ветки).

### Подготовка (StardewMCP)

`pause_time` `true`, `set_time` `10am`

### Команды SMAPI / Injury MCP

**Ветка A — light (`buffHurt`):**

```
injury_reset
injury_debuff_add buffHurt
```

**Ветка B — Severe (`buffBadlyHurt`):**

```
injury_reset
injury_debuff_add buffBadlyHurt
```

### Шаги — ветка A (light)

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` → зафиксировать HUD (1-й раз) |
| 2 | **StardewMCP** | `teleport_player` `Farm` (выйти из шахты) |
| 3 | **StardewMCP** | снова `warp_to_mine_floor` `10` |
| 4 | **вручную** | **Нет** второго мягкого HUD в тот же день |
| 5 | **StardewMCP** | `advance_day` |
| 6 | **StardewMCP** | снова `warp_to_mine_floor` `10` → мягкий HUD **снова** (новый день) |

### Шаги — ветка B (Severe)

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` → Severe HUD (1-й раз), `MineWarningDay=today` |
| 2 | **StardewMCP** | выйти на `Farm`, снова войти в шахту |
| 3 | **вручную** | **Нет** повторного Severe-текста «серьёзные раны»; вместо этого принудительный выход / *«Сегодня шахта закончена»* |
| 4 | **SMAPI** | `injury_state_dump` → `LastMineSevereForcedExitDay=today` |

### Ожидаемый результат

- Light: `_lastMineSoftHudDay` — один HUD за день
- Severe: один warning HUD за день; повтор — warp/exit, не спам `MineWarningDay`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| 2-й вход в тот же день без дубля того же warning-текста | Два одинаковых Severe-warning HUD подряд в один день |

### Статус

- [ ] Сценарий пройден (отметить ветку A / B в журнале)

---

## HOI-MINE-004 — MineForbidden истекает через `MineForbiddenDurationDays`

### ID

HOI-MINE-004

### Цель

Дебафф `HarveyMod_MineForbidden` снимается на `DayStarted`, когда `today >= MineForbiddenAppliedDay + MineForbiddenDurationDays` (config, default **2**).

### Подготовка (StardewMCP)

`pause_time` `true`, `set_time` `10am`

### Команды SMAPI / Injury MCP

Пройти цепочку HOI-MINE-001 **или** вручную:

```
injury_reset
injury_debuff_add buffBadlyHurt
```

Затем StardewMCP: `warp_to_mine_floor` `10` → Severe warning → `advance_day` → убедиться в `HarveyMod_MineForbidden`.

Записать из `injury_state_dump`:

- `MineForbiddenAppliedDay=D0`
- `MineForbiddenDurationDays=N` из config

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI** | `injury_buff_dump` — есть `HarveyMod_MineForbidden` |
| 2 | **StardewMCP** | `get_game_time` — запомнить текущий день `T` |
| 3 | **StardewMCP** | `advance_day` повторять, пока `T >= D0 + N` (обычно **ещё 1–2** сна после наложения при N=2) |
| 4 | **SMAPI** | после каждого утра: `injury_buff_dump`, `injury_state_dump` |
| 5 | **StardewMCP** | `warp_to_mine_floor` `10` с **Severe** main — **нет** HUD «Харви запретил шахту» (только обычный Severe-warning, если main ещё Severe) |

Опционально ускорить отладку:

| # | Кто | Действие |
|---|-----|----------|
| — | **SMAPI** | `injury_mine_forbidden_clear` — только для **сброса** зависшего состояния, **не** PASS для TC |

### Ожидаемый результат

- На утро `today >= D0 + N`: buff снят, `MineForbiddenAppliedDay=-1`
- Log: `[Шахта]` снятие запрета / `ExpireMineForbiddenIfDue`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Forbidden исчезает ровно по формуле дней | Buff висит бесконечно; снят на день раньше/позже без смены config |

### Статус

- [ ] Сценарий пройден

---

## HOI-MINE-005 — MineForbidden + CP event (без чёрного экрана)

### ID

HOI-MINE-005

### Цель

При активном `HarveyMod_MineForbidden` первый вход в шахту за день запускает **`eventHarveyMineInterception`** (или fallback HUD+warp) **без** чёрного экрана и **без** `NullReferenceException` в SMAPI log.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `pause_time` | `true` |
| `set_time` | `10am` |
| `warp_to_mine_floor` | `10` — **перед** `debug ebi`, игрок в `Mine` `(17,7)` по паспорту CP |

### Команды SMAPI / Injury MCP

Активировать Forbidden (как HOI-MINE-001):

```
injury_reset
injury_debuff_add buffBadlyHurt
```

StardewMCP: warning-день → `advance_day` → assert `HarveyMod_MineForbidden`.

Сброс interception-дня (если мешает повторный прогон):

```
injury_state_dump
```

(поле `LastMineForbiddenInterceptionDay` должно быть ≠ today перед тестом события)

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` — **первый** вход с Forbidden (C# path) |
| 2 | **вручную** | Дождаться cutscene **или** fallback HUD+warp; esc — пропуск диалога |
| 3 | **SMAPI log** | Нет `NullReferenceException`; есть `[MineForbidden] Запущено событие 'eventHarveyMineInterception'` **или** fallback warn |
| 4 | **StardewMCP** | `teleport_player` `Farm` |
| 5 | **StardewMCP** | снова `warp_to_mine_floor` `10` |
| 6 | **SMAPI / debug** | **`debug ebi eventHarveyMineInterception`** — принудительный CP-прогон (локация **Mine**) |
| 7 | **вручную** | Сцена до конца / skip; игрок не застрял на чёрном экране |
| 8 | **SMAPI** | `injury_mine_forbidden_clear` — только cleanup после теста |

### Ожидаемый результат

- Событие отыгрывает или fallback без зависания
- `LastMineForbiddenInterceptionDay=today` после первого входа
- Повторный вход в тот же день — HUD+warp без повторного полного event (1×/день)

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Нет black screen > ~3 с; нет NRE в log | Зависание fade; NRE в `PassOut`/`MineForbidden`/CP script |

### Статус

- [ ] Сценарий пройден

---

## HOI-MINE-010 — buffDeepCuts + Mine (мягкий путь)

### ID

HOI-MINE-010

### Цель

`buffDeepCuts` (не Severe): мягкий HUD, можно спускаться на другие этажи; **нет** `MineWarningDay`, письма, `HarveyMod_MineForbidden`, `eventHarveyMineInterception`.

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` → мягкий HUD |
| 2 | **StardewMCP** | `warp_to_mine_floor` `15` (другой этаж) |
| 3 | **вручную** | Игрок **остаётся** в шахте; **нет** cutscene interception / warp «наверх» |
| 4 | **SMAPI** | `injury_state_dump` → `MineWarningDay=-1`, `MineForbiddenAppliedDay=-1` |
| 5 | **SMAPI** | `injury_buff_dump` — **нет** `HarveyMod_MineForbidden` |

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Только мягкий HUD 1×/день; свободное перемещение по этажам | Severe HUD, MineForbidden, CP interception, принудительный warp |

### Статус

- [ ] Сценарий пройден

---

## HOI-MINE-011 — buffDeepCuts + DirtyWound + смена этажа

### ID

HOI-MINE-011

### Цель

При `buffDeepCuts` + осложнении `HarveyMod_DirtyWound` переход между этажами **не** выкидывает из шахты (в отличие от Severe / MineForbidden).

### Команды SMAPI / Injury MCP

```
injury_reset
injury_debuff_add buffDeepCuts
injury_complication_add HarveyMod_DirtyWound
```

(или дождаться DirtyWound от экспозиции в шахте — см. [09-complication-tests.md](09-complication-tests.md))

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` |
| 2 | **StardewMCP** | `warp_to_mine_floor` `25` |
| 3 | **вручную** | Нет warp на Farm / Hospital / Mine entrance cutscene |
| 4 | **SMAPI** | `MineWarningDay=-1`, нет `HarveyMod_MineForbidden` |

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Остаётся в MineShaft после смены этажа | `eventHarveyMineInterception` или Severe warp |

### Статус

- [ ] Сценарий пройден

---

## HOI-MINE-012 — buffBadlyHurt: строгое предупреждение → письмо + запрет

### ID

HOI-MINE-012

### Цель

Явная регрессия Severe-пути: `buffBadlyHurt` → строгий HUD → сон → `mailHarveyMineForbidden` + `HarveyMod_MineForbidden` на `MineForbiddenDurationDays`.

Дублирует HOI-MINE-001, но фиксирует правило №2 из сводки.

### Команды / шаги

См. **HOI-MINE-001** (те же команды и assert).

### Дополнительный assert

| Поле | Ожидание |
|------|----------|
| `MineWarningDay` | = день строгого HUD, затем −1 после apply |
| Утренний HUD | *«Харви запретил шахту на N дн. Осталось: N дн.»* |

### Статус

- [ ] Сценарий пройден (можно отметить вместе с HOI-MINE-001)

---

## HOI-MINE-013 — HarveyMod_MineForbidden: блок + текст срока

### ID

HOI-MINE-013

### Цель

При активном запрете вход в шахту показывает **оставшиеся дни** и блокирует (HUD + warp или cutscene 1×/день).

### Подготовка

Цепочка как HOI-MINE-001 → `advance_day` → `HarveyMod_MineForbidden` активен.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` |
| 2 | **вручную** | HUD: *«Запрет Харви на шахту ещё действует. Осталось: X дн.»* (см. также HOI-MINE-005 / CP interception) |
| 3 | **SMAPI** | F10 compact: `daysLeft` совпадает с `MineForbiddenDurationDays` − прошедшие дни |
| 4 | **вручную** | Игрок выведен из подземелья (warp) |

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Текст с **X** днями; блок входа | Только общий текст без срока; вход без блока |

### Статус

- [ ] Сценарий пройден

---

# PassOut

Пайплайны (C# `PassOutHandler`):

| Триггер | Условие | Итог (dating) |
|---------|---------|----------------|
| Критический HP | `LastPassedOutHealth` 0–10, не шахта | `buffBadlyHurt` + `eventHarveyEmergencyCare` |
| Шахтная смерть | `health≤0` в Mine | `NeedsMineRescueEvent`, `buffBadlyHurt`, rescue event утром |
| Истощение | `stamina≤−15` | `buffFarmerExhausted`, `topicFarmerExhausted`, `eventHarveyExhaustion` |
| Поздно в Town | `time≥2600`, Town | `buffSleepy`, `topicPassedOutInTown`, `mailHarveySleepControl` |

---

## HOI-PASSOUT-001 — HP 0–10 + Harvey dating/married

### ID

HOI-PASSOUT-001

### Цель

Критический pass-out **вне шахты** при отношениях с Харви → `buffBadlyHurt` (через `ApplyBadlyHurtSafe`).

### Предусловие сейва

- **StardewMCP:** `get_npc_info` `Harvey` → dating или married  
- Иначе TC **SKIP** (не FAIL мода)

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `Town` (не `Mine`) |
| `set_time` | `8pm` |
| `set_health` | `0` (или `1`–`10` перед обмороком — ветка DayEnding фиксирует `LastPassedOutHealth`) |

### Команды SMAPI / Injury MCP

```
injury_reset
injury_state_dump
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `set_health` `0` в `Town` |
| 2 | **StardewMCP** | `advance_day` (сон при 0 HP / pass-out) |
| 3 | **SMAPI** | `injury_state_dump` → `WasPassedOut` мог сброситься после warp; смотреть итог buff |
| 4 | **SMAPI** | `injury_buff_dump` → **`buffBadlyHurt`** |
| 5 | **SMAPI** | `injury_phase_list` → `MainInjuryId=buffBadlyHurt` |
| 6 | **вручную / log** | При успешном queue: cutscene **`eventHarveyEmergencyCare`** в Hospital; иначе fallback critical |

Альтернатива (мгновенная фиксация HP в шахте **не** подходит — см. HOI-PASSOUT-002):

- Боевой урон до 0 в Town: **вручную** (StardewMCP `spawn_monster` + без invincible) — если `set_health 0` + `advance_day` не ставит `WasPassedOut`.

### Ожидаемый результат

- `buffBadlyHurt` + `topicBadlyHurt`
- Log: `⚠️ Критический pass-out вне шахты` / `[PassOutEvent]`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| `buffBadlyHurt` при dating + pass-out вне Mine | Нет badly hurt при dating; срабатывает только без отношений |

### Статус

- [ ] Сценарий пройден

---

## HOI-PASSOUT-002 — Mine death rescue

### ID

HOI-PASSOUT-002

### Цель

Боевая «смерть» в шахте выставляет **`NeedsMineRescueEvent`**; на следующее утро — mine rescue event (`eventHarveyMineRescue` / `eventHarveyMineRescueDating`).

### Предусловие сейва

Harvey **dating/married** (см. HOI-PASSOUT-001).

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `pause_time` | `true` |
| `set_time` | `2pm` |
| `set_npc_relationship` | `Harvey`, `8` (сердечки; не заменяет dating) |

### Команды SMAPI / Injury MCP

```
injury_reset
injury_state_dump
```

### Шаги — путь gameplay (предпочтительный)

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `warp_to_mine_floor` `10` |
| 2 | **StardewMCP** | `set_health` `0` |
| 3 | **SMAPI** | Сразу `injury_state_dump` → `NeedsMineRescueEvent=True`, `PassedOutInMineYesterday=True`, `LastPassedOutHealth=0` |
| 4 | **SMAPI log** | `[MineRescue] Зафиксирована боевая смерть в шахте` |
| 5 | **StardewMCP** | `advance_day` |
| 6 | **SMAPI** | `injury_state_dump` → `PendingMineRescueEventId` = `eventHarveyMineRescueDating` или `eventHarveyMineRescue` |
| 7 | **вручную** | Утро: warp в Mine, cutscene rescue; esc по необходимости |
| 8 | **SMAPI** | После события: `topicMineInjuryRescue` в `injury_topic_dump`; `NeedsMineRescueEvent=False` |

### Шаги — путь debug (если шаг 3 не сработал)

| # | Кто | Действие |
|---|-----|----------|
| 1 | **SMAPI** | `injury_debug_mine_rescue` |
| 2 | **StardewMCP** | `advance_day` |
| 3 | **SMAPI** | assert как в шагах 6–8 выше |

### Ожидаемый результат

- `buffBadlyHurt` от `ApplyBadlyHurtFromMinePassOut`
- Утро: rescue event; topic **`topicMineInjuryRescue`**
- **Не** должно срабатывать Severe mine warning поверх rescue (`NeedsMineRescueEvent` блокирует warning)

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| `NeedsMineRescueEvent` True после death; event/topic после сна | Флаг False при dating+0 HP в Mine; зависание fade rescue |

### Статус

- [ ] Сценарий пройден (путь: gameplay / debug)

---

## HOI-PASSOUT-003 — Mine rescue topic → forced hospitalization

### ID

HOI-PASSOUT-003

### Цель

`topicMineInjuryRescue` + **Severe** main → при входе в **`Hospital`** принудительная госпитализация (`ForceHospitalization`, reason `mine_rescue`).

### Подготовка (StardewMCP)

`set_time` `10am`

### Команды SMAPI / Injury MCP

```
injury_reset
injury_topic_add topicMineInjuryRescue
injury_debuff_add buffBadlyHurt
injury_state_dump
injury_topic_dump
```

Требуется **`ForceHospitalization: true`** в config.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `teleport_player` `Hospital` |
| 2 | **SMAPI** | `injury_hospital_status` → `IsHospitalized=True`, `HospitalizedInjuryId=buffBadlyHurt`, reason **mine_rescue** |
| 3 | **SMAPI** | `injury_topic_dump` → **`topicMineInjuryRescue` удалён** |
| 4 | **SMAPI log** | `⚠️ Игрок в госпитале с ранами после шахты → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ` |

Альтернатива (proximity, 2 шага): Harvey рядом в Hospital — сначала warning emote, повтор — hosp. (см. `PlayerEventHandler` proximity). Для TC достаточно входа в локацию.

### Ожидаемый результат

- Forced hospital lock; topic снят
- `buffBadlyHurt` остаётся main (или phase по состоянию лечения)

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| `IsHospitalized=True` при topic+Severe+Hospital | Нет госпитализации при `ForceHospitalization=true` |
| | `buffHurt` (не Severe) + topic → hosp. (ложное) |

### Статус

- [ ] Сценарий пройден

---

## HOI-PASSOUT-004 — Exhaustion (stamina ≤ −15)

### ID

HOI-PASSOUT-004

### Цель

Обморок от истощения → `buffFarmerExhausted`, `topicFarmerExhausted`, hospital event **`eventHarveyExhaustion`** (dating).

### Ограничение автоматизации

**StardewMCP не имеет `set_stamina`.** Полная симуляция — **gameplay** или частично вручную.

### Предусловие сейва

Harvey dating/married.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `Farm` |
| `set_time` | `6am` |
| `get_player_info` | записать стартовую `stamina` |

### Команды SMAPI / Injury MCP

```
injury_reset
```

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **вручную / gameplay** | Истощить farmer до **`stamina ≤ −15`** (инструменты, бег, без еды; **не** StardewMCP) |
| 2 | **StardewMCP** | `get_player_info` → подтвердить `stamina ≤ −15` |
| 3 | **StardewMCP** | `advance_day` (pass-out при сне / DayEnding `TrackPassOut`) |
| 4 | **SMAPI** | `injury_state_dump` → `WasExhausted=True` (до clear) |
| 5 | **SMAPI** | `injury_buff_dump` → **`buffFarmerExhausted`** |
| 6 | **SMAPI** | `injury_topic_dump` → **`topicFarmerExhausted`** |
| 7 | **вручную / log** | Warp в Hospital → **`eventHarveyExhaustion`** или fallback exhaustion |

Опциональная проверка без истощения:

| # | Кто | Действие |
|---|-----|----------|
| — | **SMAPI** | `injury_topic_add topicFarmerExhausted` + `injury_debuff_add` — **не** заменяет полный pass-out pipeline; только sanity buff/topic |

### Ожидаемый результат

- Exhaustion flags + buff + topic
- Log: `Обнаружен обморок от истощения` / `💤 Pass-out от истощения`

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Buff+topic после реального stamina≤−15 и сна | Нет exhaustion при явном −15 stamina и dating |

### Статус

- [ ] Сценарий пройден · [ ] SKIP (нет ручного истощения)

---

## HOI-PASSOUT-005 — Late Town collapse

### ID

HOI-PASSOUT-005

### Цель

Поздний обморок в **Town** → `buffSleepy`, `topicPassedOutInTown`, письмо **`mailHarveySleepControl`**.

### Подготовка (StardewMCP)

| Tool | Аргументы |
|------|-----------|
| `teleport_player` | `Town`, опц. `37`, `59` |
| `set_time` | `2600` или `2:00am` (≥ 2600 в `Game1.timeOfDay`) |
| `pause_time` | `false` — время должно тикать к pass-out |

### Команды SMAPI / Injury MCP

```
injury_reset
```

`SendLetters: true` для mail.

### Шаги

| # | Кто | Действие |
|---|-----|----------|
| 1 | **StardewMCP** | `teleport_player` `Town` + `set_time` `2600` |
| 2 | **вручную** | Дождаться ванильного pass-out / collapse (или BETAS trigger **`eventHarveyLateNightCollapse`** из CP — отдельно от C# branch) |
| 3 | **StardewMCP** | `advance_day` если pass-out завершился сном |
| 4 | **SMAPI** | `injury_buff_dump` → **`buffSleepy`** |
| 5 | **SMAPI** | `injury_topic_dump` → **`topicPassedOutInTown`** |
| 6 | **вручную** | Почта: **`mailHarveySleepControl`** (+ tier) |
| 7 | **SMAPI log** | `🌙 Триггер: Обморок в городе из-за позднего времени` |

C# ветка **не** требует dating для sleepy/topic/mail (в отличие от HOI-PASSOUT-001).

### Ожидаемый результат

- `buffSleepy`, `topicPassedOutInTown` (2 дня)
- `mailHarveySleepControl` на завтра при `SendLetters`
- HUD: *«Ты упала без сил посреди города...»*

### Критерий PASS/FAIL

| PASS | FAIL |
|------|------|
| Три артефакта (buff, topic, mail) | Нет topic при pass-out в Town после 02:00 |

### Статус

- [ ] Сценарий пройден

---

## Сводка: StardewMCP vs SMAPI по TC

| ID | StardewMCP | SMAPI / Injury MCP | Вручную |
|----|------------|-------------------|---------|
| HOI-MINE-001 | warp mine, advance_day | reset, debuff_add, dumps | HUD, почта |
| HOI-MINE-002 | warp mine, advance_day | reset, debuff_add, dumps | HUD |
| HOI-MINE-003 | warp × N, advance_day | reset, debuff_add | HUD count |
| HOI-MINE-004 | advance_day × N, warp mine | dumps, (clear=fail only) | — |
| HOI-MINE-005 | warp mine, Farm | reset, debuff, advance, **debug ebi**, clear | cutscene |
| HOI-PASSOUT-001 | Town, health, advance_day | reset, dumps | emergency event |
| HOI-PASSOUT-002 | warp mine, health, advance_day | reset, debug_mine_rescue, dumps | rescue cutscene |
| HOI-PASSOUT-003 | teleport Hospital | topic_add, debuff_add, hospital_status | — |
| HOI-PASSOUT-004 | get_player_info, advance_day | reset | **stamina ≤ −15 gameplay** |
| HOI-PASSOUT-005 | Town, set_time late, advance_day | reset, dumps | pass-out/mail UI |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC, запись PASS/FAIL.
2. **Этот файл** — [10-mine-passout-tests.md](10-mine-passout-tests.md) — незакрытые HOI-MINE-* / HOI-PASSOUT-* из журнала.
3. **Следующая тема: госпитализация** — создать или дополнить чеклист (`11-hospitalization-tests.md`): `injury_hospital_status`, `ForceHospitalization`, discharge, `buffHarveyIntensiveCare`, night crisis; опора на [06-debug-setup-commands.md](06-debug-setup-commands.md) и [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) (поля `IsHospitalized`, `HospitalMinStayMinutes`).
4. **Proximity** — [docs/audit-proximity-reactions.md](../audit-proximity-reactions.md), `injury_proximity_test`, CP `harvey_proximity_injury.json`; не смешивать с mine warning.
5. **CP-события** — [02-cp-content-inventory.md](02-cp-content-inventory.md), [docs/events-inventory/01-cp-events-catalog.md](../events-inventory/01-cp-events-catalog.md), [EVENTS_TEST_CHECKLIST.md](EVENTS_TEST_CHECKLIST.md); `debug ebi` + warp по location.
6. [05-debug-dump-commands.md](05-debug-dump-commands.md) — assert после госпитализации / pass-out.
7. **Блокеры:** dating только из сейва; `set_stamina` нет в StardewMCP (HOI-PASSOUT-004); cutscene PASS только вручную; save/load resume — [07-smoke-save-tests.md](07-smoke-save-tests.md).
