# Аудит существующих debug-команд (ModEntry.cs)

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Справочник механик: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md)

**Область:** только команды, зарегистрированные в `ModEntry.cs` (SMAPI console + зеркало Injury MCP).  
**Источник правды:** `ModEntry.cs` — `FullReset`, `CmdDebuff*`, `CmdPhase*`, `BuildPhaseListReport`, `ApplyDebugTraumaEffects`.

**Не включено:** остальные `injury_*` (`injury_harvey_click`, `injury_debug_dump`, …) — отдельный чат.

---

## Общая схема вызовов

```text
injury_reset          → BuffManager.RemoveBuff* + ModTopicRegistry cleanup + StateManager.Clear()
injury_debuff_add     → InjuryManager.TryApplyMainInjury / ApplyDebugTraumaEffects / complication path
injury_phase_ready    → StateManager.SetReadyForNextPhase
injury_phase_recovery → StateManager.SetReadyForRecovery
injury_phase_advance  → TreatmentManager.AdvanceInjuryToNextPhase
injury_phase_cure     → TreatmentManager.CompleteInjuryRecovery
injury_phase_list     → read-only (InjuryManager.GetMainInjuryDebugInfo + StateManager)
injury_debuff_list    → read-only (KnownTraumas / KnownComplications)
```

**Injury MCP:** те же handler'ы; после мутаций возвращает `BuildPhaseListReport()` (см. `ExecuteMcpTool`).

---

## injury_reset

### usage

```
injury_reset
```

Без аргументов. Требует загруженное сохранение (`Context.IsWorldReady`).

### BuffManager

- Удаляет базовые баффы всех записей из `ActiveDebuffs` (травмы + осложнения в словаре).
- Для фазовых травм (`TotalPhases > 0`) — дополнительно снимает phase-buff 1–3 через `InjuryManager.GetPhaseBuffId`.
- Удаляет баффы всех `ActiveComplications`.
- Снимает фиксированный список cure/self-care/prescription buffs (`buffHarveyTreatment`, `buffHarveyIntensiveCare`, `buffHarveyCare`, `buffHarveyRehab`, prescription IDs и т.д.).

**Не снимает явно:** `HarveyMod_MineForbidden`, `buffTooCold` и прочие «служебные» debuff, если они не попали в перечисленные словари/списки.

### StateManager

- `_prescriptionManager.ClearAllPrescriptions()`, `_rehabManager.ClearRehab()`.
- `_stateManager.Clear()` — новый пустой `InjuryState` + `Save()`.

### DialogueManager

- Напрямую чистит `Game1.player.activeDialogueEvents`: удаляются только ID из `ModTopicRegistry.GetAllOwnedTopicIds()`.
- Чужие топики (другие моды, `injury_foreign_topic_add`) **не** трогаются.

### Edge cases — обработаны

- Вызов до загрузки сейва → warn, без изменений.
- Фазовые баффы снимаются даже если `CurrentPhase` рассинхронизирован с UI.
- Owned topics не затрагивают foreign keys.

### Edge cases — не обработаны

- Orphan vanilla buffs вне списков reset (MineForbidden, TooCold).
- `eventsSeen` / mail queue / hospital cutscene queue не сбрасываются.
- CP one-shot состояние (`AppliedTriggers` обнуляется через `Clear`, но `eventsSeen` остаётся).

### Достаточно для AI-тестирования?

**Да** как стандартный «чистый лист» перед изолированным TC. Для сценариев save/load, mine rescue, hospitalization — нужны доп. команды или ручная подготовка.

### StardewMCP после команды

| Проверка | Tool |
|----------|------|
| Игрок жив, базовая локация | `get_player_info` |
| HP/энергия при необходимости | `set_health`, `get_player_info` |
| Harvey hearts (если TC про диалог/тон) | `set_npc_relationship Harvey N` |
| Время/погода для следующего шага | `get_game_time`, `set_time`, `set_weather` |

Подтверждение состояния мода — через `injury_phase_list` / Injury MCP (не StardewMCP): `MainInjuryId: (none)`, пустые complications.

---

## injury_debuff_list

### usage

```
injury_debuff_list
```

Read-only. Выводит `KnownTraumas` (buffId, topicId, длительности фаз P1/P2/P3) и `KnownComplications`.

### BuffManager / StateManager / DialogueManager

**Ничего не меняет.**

### Edge cases — обработаны

- Работает без загруженного сейва (список статический).

### Edge cases — не обработаны

- Не показывает cure-buff IDs, phase-buff IDs, prescription IDs.
- Не помечает, какие травмы simple vs phased (кроме нулевых фаз в строке).
- ID вне `KnownTraumas`/`KnownComplications` упомянуты текстом, но не перечислены.

### Достаточно для AI-тестирования?

**Частично.** Хватит выбрать ID для `injury_debuff_add`; для полной карты buff/topic — нужен `01-csharp-mechanics-inventory.md` или `injury_audit_content`.

### StardewMCP после команды

Не требуется. Опционально: нет.

---

## injury_debuff_add

### usage

```
injury_debuff_add [--force] <id> [минуты]
```

- `--force` — замена текущей main через `TryApplyMainInjury(..., forceReplace: true)`.
- `минуты` — длительность баффа; по умолчанию `-2` (весь игровой день). Передаётся в `BuffManager.AddBuff`.
- Три ветки: **KnownTrauma** → `CmdDebuffAddTrauma`; **KnownComplication** → прямой путь; **unknown** → только `AddBuff`.

### BuffManager

| Ветка | Действие |
|-------|----------|
| Trauma (новая/замена main) | `AddBuff(trauma.BuffId)` |
| Trauma (та же main) | `AddBuff` повторно |
| Complication | `AddBuff(comp.BuffId)` |
| Unknown | `AddBuff(id)` (+ warn если нет в Data/Buffs) |

При `--force`/upgrade старые phase-buff снимает `InjuryManager.TryApplyMainInjury` (не сама команда).

### StateManager

| Ветка | Действие |
|-------|----------|
| Trauma | `CreateDebuffState(buffId, today, P1, P2, P3)`; `SetMainInjury` через `TryApplyMainInjury` |
| Complication | `ActiveComplications[compId] = today`, `CreateComplicationState` |
| Unknown | **без** DebuffState / MainInjury |

`TryApplyMainInjury` при замене: `RemoveDebuffState(old)`, `SetMainInjury(new)`; при `buffInfectedWound` — `ClearWoundRelatedComplicationsAfterInfection`.

### DialogueManager

| Ветка | Действие |
|-------|----------|
| Trauma | `AddTopic(trauma.TopicId, topicDays)`; для `buffBadlyHurt` — `AddTopic(topicHealthDamageCritical)`; `TryAddHarveyNeedsFirstTreatmentTopic` |
| Complication | `AddTopic(comp.TopicId, 4)` |
| Unknown | **ничего** |

**Не добавляет** side-topics натуральных триггеров: `topicHealthDamageSevere` (concussion, torn), `topicPostOperativeCare` (shrapnel, surgical), `topicHealthDamageCritical` (fracture, shrapnel, infection) — кроме badly hurt.

### Edge cases — обработаны

- Нет сейва → warn.
- Вторая main без `--force` → отказ + лог (MainInjury сохраняется).
- Upgrade `buffHurt` → `buffBadlyHurt` без `--force`, если лечение не начато.
- Upgrade заблокирован, если `TreatmentStarted == true`.
- Повтор того же buffId при уже main → `ApplyDebugTraumaEffects` (refresh buff/topic), DebuffState **не** перезаписывается.
- `--force` заменяет main, чистит старые topics/phase buffs/state.
- Неизвестный ID — warn + попытка AddBuff.
- Complication получает `TreatmentStarted=true` в DebuffState (для restore pipeline).

### Edge cases — не обработаны

- Нет симуляции combat/farming/rain/mine rolls.
- Нет cooldown / `AppliedTriggers` / `SetInjuryCooldown`.
- Нет звуков (`debuffHit`) и HUD feedback кроме achievement HUD.
- Side-topics и mail tier отличаются от игрового триггера.
- Unknown buff без DebuffState → `injury_phase_*` бесполезны, MainInjury не ставится.
- Повторное добавление complication не проверяет дубликат в `ActiveComplications` (перезапись дня).
- `CleanupInvalidComplications` **не** вызывается автоматически после add.

### Достаточно для AI-тестирования?

**Да** для MainInjury, complications, фазового state baseline. **Нет** для полной CP-цепочки (FirstTreatment, severe mail, госпитализация) без доп. команд / ручных шагов.

### StardewMCP после команды

| Сценарий | Tool |
|----------|------|
| MainInjury + диалог Harvey | `teleport_player` к Harvey, `set_npc_relationship Harvey 3`, `set_time 10am` |
| DirtyWound / mine | `warp_to_mine_floor N`, `get_player_info` (локация) |
| WetBandage | `set_weather rain`, `teleport_player` outdoor |
| Severe / hospital | `teleport_player Hospital`, hearts Harvey |
| Complication без main | обычно не нужен StardewMCP — проверка через `injury_phase_list` |

Всегда: `injury_phase_list` → `MainInjuryId`, `valid`, `Complications`.

---

## injury_phase_list

### usage

```
injury_phase_list
```

Read-only (SMAPI log / MCP return text).

### BuffManager

**Не меняет.** Читает через `InjuryManager.GetMainInjuryDebugInfo()` → `BaseBuffActive`, `CureBuffActive`, `PhaseBuffActive`.

### StateManager

**Не меняет.** Читает `State`, `GetAllActiveDebuffStates()`, `GetMainInjuryDebugInfo`.

### DialogueManager

**Не меняет.** Топики не перечисляет (только косвенно через treatment flags).

### Edge cases — обработаны

- Нет сейва → `"Error: load a save first."` (MCP) / ранний return в console path через тот же builder.
- Invalid MainInjury → `valid: no` + `Reason`.
- Пустой ActiveDebuffs → явное сообщение.
- Осложнения — список ключей `ActiveComplications`.

### Edge cases — не обработаны

- Не показывает conversation topics, mail, hospital flags, mine/rescue, neglect counters.
- `MainInjuryId from state` дублирует `MainInjuryId` (legacy строка).
- Не различает complication DebuffState vs trauma в блоке «активные травмы» (все из `ActiveDebuffs`).
- Orphan buff без DebuffState не виден.

### Достаточно для AI-тестирования?

**Да** как primary assertion после мутаций (MainInjury, phases, Ready*, complications). Для глубокого state — `injury_debug_dump`.

### StardewMCP после команды

Обычно не нужен. Если `valid: no` из-за рассинхрона buff/state — StardewMCP не поможет; нужны repair-команды (`injury_main_set`, `injury_cleanup_invalid_complications`).

---

## injury_phase_ready

### usage

```
injury_phase_ready <buffId> [1|0]
```

- `[1|0]` — default `1` (включить ReadyForNextPhase).
- Только **фазовые** травмы (`TotalPhases > 0`, не simple).

### BuffManager

**Не меняет.**

### StateManager

- `SetReadyForNextPhase(id, ready)` → `DebuffState.ReadyForNextPhase`, `UpdatePhaseReadyTracking`, `Save()`.
- При `ready=false` — `CheckupManager.ClearCheckupTracking`.

### DialogueManager

**Не меняет.**

### Edge cases — обработаны

- Нет сейва / нет buffId в `ActiveDebuffs` → warn.
- Simple injuries (`buffHurt`, `buffBadlyHurt`, `buffSurgicalWound`) → отказ с подсказкой использовать `injury_phase_recovery` / `injury_phase_cure`.
- `TotalPhases <= 0` при `ready=1` → отказ.
- Уже последняя фаза при `ready=1` → отказ, подсказка `injury_phase_recovery`.
- HUD message при успехе.

### Edge cases — не обработаны

- Не проверяет `TreatmentStarted` / `CurrentPhase > 0` (можно выставить ready до начала лечения — бесполезно для advance без `injury_harvey_click`).
- Не проверяет, что buffId == MainInjuryId.
- Complication buffIds в ActiveDebuffs теоретически могут пройти проверку `TotalPhases <= 0` и получить ready только через recovery path, не через ready — OK.

### Достаточно для AI-тестирования?

**Да** для эмуляции «фаза истекла» перед кликом Harvey или `injury_phase_advance`. Не заменяет `StartTreatment`.

### StardewMCP после команды

| Шаг | Tool |
|-----|------|
| Клик Harvey для advance (ручной TC) | `teleport_player` + `set_npc_relationship Harvey N` + `get_npc_location Harvey` |
| Проверка без клика | только `injury_phase_list` → флаг `[→след.фаза]` |

---

## injury_phase_recovery

### usage

```
injury_phase_recovery <buffId> [1|0]
```

- Default `ready=1` → `ReadyForRecovery`.
- Работает для **simple и phased** (в т.ч. последняя фаза).

### BuffManager

**Не меняет.**

### StateManager

- `SetReadyForRecovery(id, ready)` → `DebuffState.ReadyForRecovery`, tracking, `Save()`.

### DialogueManager

**Не меняет.**

### Edge cases — обработаны

- Нет сейва / unknown buffId → warn.
- Simple injuries **разрешены** (в отличие от `phase_ready`).
- Сброс ready (`0`) поддерживается.
- HUD message.

### Edge cases — не обработаны

- Не проверяет, что травма в лечении или на последней фазе (можно выставить recovery с `CurrentPhase=0`).
- Не проверяет MainInjuryId.
- Не эмулирует auto-set из `GameEventHandler.CheckInjuryPhases` (день + duration).

### Достаточно для AI-тестирования?

**Да** для финального осмотра (simple + phased). Пара с `injury_harvey_click` или ручным кликом Harvey.

### StardewMCP после команды

Аналогично `injury_phase_ready`: teleport к Harvey для клика; assert через `injury_phase_list` (`[→выздоровление]`).

---

## injury_phase_advance

### usage

```
injury_phase_advance <buffId>
```

Принудительная смена фазы **без диалога Harvey**. Только phased injuries.

### BuffManager

Через `TreatmentManager.AdvanceInjuryToNextPhase`:

- `RemoveBuff(oldPhaseBuffId)`
- `AddBuff(newPhaseBuffId, -2)`

### StateManager

- `AdvancePhase(injuryId, today)` — инкремент `CurrentPhase`, сброс ready-tracking.
- `SetReadyForNextPhase(false)` если флаг остался.

Также `CheckupManager.CompleteCheckup` (side effect на checkup state).

### DialogueManager

- `RemoveTopic(oldPhaseTopicId)`
- `AddTopic(newPhaseTopicId, topicDays)`

### Edge cases — обработаны

- Simple injury → отказ.
- Нет DebuffState / последняя фаза / не фазовая → отказ в `CmdPhaseAdvance`.
- `TreatmentManager` дополнительно требует `ReadyForNextPhase == true` и `CurrentPhase > 0` — иначе warn, **без смены фазы**.
- Invalid phase increment → warn «AdvancePhase не изменил фазу».

### Edge cases — не обработаны

- **Cmd не выставляет `ReadyForNextPhase` автоматически** — нужен предварительный `injury_phase_ready 1`.
- **Не стартует лечение** — нужен `injury_harvey_click` / клик Harvey (`CurrentPhase` должен быть ≥ 1).
- Пропускает диалог Harvey, CP phase transition lines, `eventsSeen`.
- Известный баг рассинхрона phase topic keys (см. `docs/audit-phase-treatment/`) — advance может дать orphan/wrong topic.
- Не проверяет MainInjury vs secondary debuff state.

### Достаточно для AI-тестирования?

**Частично.** Подходит для чистой механики buff/state/topic между фазами. Для E2E «как в игре» — `injury_phase_ready` + клик Harvey; advance — shortcut.

### StardewMCP после команды

| Проверка | Tool |
|----------|------|
| Phase buff на игроке (косвенно) | `get_player_info` — ограниченно; надёжнее `injury_phase_list` (`PhaseBuff active`) |
| Следующий клик Harvey | teleport + hearts |
| Checkup TC | `set_date` / `advance_day` если тестируется overdue checkup |

---

## injury_phase_cure

### usage

```
injury_phase_cure <buffId>
```

Полное выздоровление без клика Harvey.

### BuffManager

Через `TreatmentManager.CompleteInjuryRecovery` → `ApplyMechanicalPhasedRecovery`:

- `RemoveAllPhaseBuffs(injuryId)` — base injury + phase buffs 1–3.
- `AddBuff(buffHarveyCare, 28800000 ms)`.

**Не снимает** cure-buff simple treatment (`buffHarveyTreatment`, `buffHarveyIntensiveCare`, `buffPostSurgicalCare`), если они уже были наложены до cure.

### StateManager

- `CompleteMainInjury(injuryId)` — clear MainInjuryId.
- `RemoveDebuffState(injuryId)`.
- `NotifyInjuryRecovered` → residual cooldown 2 дня для repeatable.

### DialogueManager

- Удаляет injury, treatment, phase topics (1–3).
- **Добавляет** `topicTreatmentCompleted` (7 дн.) — debug path, **не** `topic{Injury}Cured`.
- `ApplyHighComplianceRecoveryBonuses` через ComplianceManager (topics compliance).

### Edge cases — обработаны

- Нет DebuffState → warn, без изменений.
- Работает для simple и phased.
- HUD «полное выздоровление».

### Edge cases — не обработаны

- Orphan cure-buff после simple treatment mid-flight.
- Не лечит осложнения (DirtyWound остаёт в `ActiveComplications`).
- Не снимает hospitalization / mine / pass-out flags в state (если были до cure main).
- CP cured cutscene / `topicDeepCutsCured` не ставится — только `topicTreatmentCompleted`.
- Complications DebuffState не удаляются.

### Достаточно для AI-тестирования?

**Да** для «main исчезла, MainInjury пуст» и cleanup между TC. **Нет** для проверки CP cured flow и полного medical pipeline.

### StardewMCP после команды

| Проверка | Tool |
|----------|------|
| Игрок свободен для новой травмы | `get_player_info` |
| Следующий debuff_add | обычно `injury_reset` или сразу add |
| Harvey Care buff | только in-game / `injury_phase_list` (Care не в list) — F10 или dump |

Assert: `injury_phase_list` → `MainInjuryId: (none)`, нет строки buffId; complications unchanged.

---

## Сводная таблица: тестовая потребность vs команды

| Тестовая потребность | Покрыта текущей командой | Какая команда / пробел |
|----------------------|:------------------------:|-------------------------|
| Чистый старт перед TC | да | `injury_reset` |
| Список ID травм/осложнений | да | `injury_debuff_list` |
| Наложить main injury | да | `injury_debuff_add <id>` |
| Замена main / force | да | `injury_debuff_add --force` |
| Upgrade hurt→badly hurt | да | `injury_debuff_add buffBadlyHurt` (без force) |
| Блокировка второй main | да | `injury_debuff_add` (без force) + assert `injury_phase_list` |
| Наложить complication | да | `injury_debuff_add HarveyMod_*` |
| Assert MainInjuryId, valid, phases | да | `injury_phase_list` |
| Эмуляция «фаза истекла» | да | `injury_phase_ready` |
| Эмуляция «можно выписать» | да | `injury_phase_recovery` |
| Смена фазы (механика) | частично | `injury_phase_ready` + `injury_phase_advance` (+ start treatment) |
| Смена фазы (диалог CP) | нет | `injury_harvey_click` / ручной клик Harvey |
| StartTreatment / cure buff | нет | `injury_harvey_click` |
| Полное выздоровление main | да | `injury_phase_cure` |
| Cured topic / CP cutscene | нет | игровой CompleteRecovery; debug ≠ `topic*Cured` |
| Side-topics (Severe, Critical, PostOp) | нет | только частично при `debuff_add`; нужна `injury_debuff_add_full` или game trigger |
| DirtyWound из шахты | нет | `injury_mine_dirty_debug` + gameplay; нет `injury_force_dirty_wound` |
| Infection escalation | нет | daily pipeline / `injury_debuff_add buffInfectedWound --force` |
| WetBandage от дождя | частично | `injury_debuff_add` + StardewMCP rain; нет wet exposure sim |
| Neglect strikes | нет | `advance_day` + time; нет `injury_neglect_set` |
| Hospitalization / Severe warp | нет | StardewMCP teleport + ручной/event |
| Mine rescue / pass-out | нет | `injury_debug_mine_rescue`, ручной sleep |
| Save/load buff restore | нет | ручной save/load |
| Prescription / checkup / rehab | нет | `injury_prescription_*`, `injury_checkup_due`, `injury_rehab_*` |
| Repair invalid MainInjury | нет | `injury_main_set`, `injury_main_clear`, `injury_cleanup_invalid_complications` |
| Полный dump state/topics | нет | `injury_debug_dump`, `injury_audit_content` |
| Симуляция клика Harvey (FSM) | нет | `injury_harvey_click` |
| Cooldown injuries | нет | `injury_cooldowns` (read); нет set |
| FirstTreatment / Diagnosis CP | частично | `TryAddHarveyNeedsFirstTreatmentTopic` при debuff_add; event — вручную |

---

## Рекомендуемые цепочки для AI-прогона

### MainInjury baseline

```
injury_reset
injury_debuff_add buffFracturedBone
injury_phase_list    → MainInjuryId, valid=yes
```

### Блокировка + force

```
injury_reset
injury_debuff_add buffFracturedBone
injury_debuff_add buffDeepCuts              → отказ
injury_debuff_add --force buffDeepCuts      → main=buffDeepCuts
```

### Фазовое лечение (механика)

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click                         → StartTreatment (не в scope этого файла)
injury_phase_ready buffDeepCuts 1
injury_phase_advance buffDeepCuts           → фаза 2
injury_phase_list
```

### Simple recovery

```
injury_reset
injury_debuff_add buffHurt
injury_harvey_click
injury_phase_recovery buffHurt 1
injury_harvey_click                         → CompleteRecovery
```

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC, ограничения.
2. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — полный справочник ID, state fields, **пробелы QA** (§ «Все console commands»).
3. **Этот файл** — [03-existing-debug-commands.md](03-existing-debug-commands.md) — поведение 8 core-команд из `ModEntry.cs`.
4. **Следующий артеfact:** `docs/testing/04-missing-debug-commands.md` — приоритизированный список **недостающих** debug-команд для закрытия строк таблицы «Покрыта: нет» (force dirty wound, hospitalize, pass-out sim, debuff_add_full side-topics, cooldown set, save/load helper, …).
5. [main-injury-testcases.md](main-injury-testcases.md) — прогон TC с опорой на таблицу покрытия выше.
6. [injury-mcp.md](injury-mcp.md) + [stardew-mcp.md](stardew-mcp.md) — автоматизация; команды вне scope (harvey_click, debug_dump) уже зеркалированы частично в MCP.
7. **Блокеры без новых команд:** CP cutscenes, `eventsSeen`, save/load, sprained ankle / surgical gameplay trigger — помечать «вручную» в TC.
