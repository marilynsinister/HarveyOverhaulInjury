# Недостающие debug-команды для AI-тестирования

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Существующие команды: [03-existing-debug-commands.md](03-existing-debug-commands.md)  
> Механики C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) · CP: [02-cp-content-inventory.md](02-cp-content-inventory.md)

**Область:** только **новые** SMAPI/Injury MCP команды, которых ещё нет в `ModEntry.cs`.  
**Не меняет код** — спецификация для следующего чата реализации.

**Принцип:** одна команда — одно действие. Без «суперкоманд», объединяющих reset + debuff + phase.

**Связь с существующим:** `injury_debug_dump` ≈ F10 full HUD (человекочитаемый отчёт). Команды ниже — **структурированные** дампы и **точечные** мутации, которых в `injury_debug_dump` / `injury_phase_list` недостаточно для автоматических assert.

---

## Dump-команды (read-only)

### injury_state_dump

#### usage

```
injury_state_dump
```

Без аргументов. Требует загруженное сохранение.

#### Зачем нужна

Машиночитаемый полный снимок `InjuryState` (save key `injury_state`): MainInjury, `ActiveDebuffs`, осложнения, шахта, госпитализация, pass-out, neglect, rain, prescriptions/rehab — без обрезки F10 и без смешения с buff/topic.

#### Что должна менять

**Ничего** (read-only). Опционально: `StateManager.Save()` не вызывать.

#### Что не должна менять

Buffs, topics, `eventsSeen`, mail queue, игровое время, локацию.

#### Ожидаемый log prefix

```
[QA] injury_state_dump
```

Каждое поле — отдельная строка или стабильный key=value блок (для парсинга MCP-ответом).

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| main-injury **9** Save/load | assert state до/после reload без ручного F10 |
| **4b** DirtyWound при фазовом лечении | `TreatmentStarted`, phase без base buff |
| **5** Infection escalation | `ActiveComplications`, `NeglectStrikesByInjury`, `LastInfectionEscalationDay` |
| **11** NeglectStrikesByInjury | per-injury strikes, не перенос между main |
| Mine rescue / pass-out (F10 поля) | `NeedsMineRescueEvent`, `PendingHospitalPassOutEventId` |
| Hospitalization 8a | `IsHospitalized`, `HospitalizedInjuryId`, discharge timers |

---

### injury_buff_dump

#### usage

```
injury_buff_dump
```

#### Зачем нужна

Список **всех** баффов на игроке: vanilla + mod, с пометкой `mod`, `trauma`, `phase`, `cure`, `complication`, `prescription`, `orphan` (buff есть, DebuffState нет).

#### Что должна менять

**Ничего.**

#### Что не должна менять

State, topics, MainInjury.

#### Ожидаемый log prefix

```
[QA] injury_buff_dump
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **1** Базовое наложение | base `buffFracturedBone` до лечения |
| **4b** Фазовое лечение | phase buff активен, base снят |
| **6** Фазовое лечение | смена phase1→phase2 buff на игроке |
| **7** Cure | нет main buff, есть `buffHarveyCare` |
| **5b/5c** WetBandage | только `buffInfectedWound` vs + `HarveyMod_WetBandage` |
| Buff restore (DayStarted) | `SavedActiveBuffs` vs фактические applied buffs |
| `injury_cleanup_invalid_complications` | orphan/stale buff detection |

---

### injury_topic_dump

#### usage

```
injury_topic_dump
```

Опционально в реализации (не обязательно в v1): `injury_topic_dump [filter]` — `topic`, `HarveyMod`, `situation`, `all`.

#### Зачем нужна

Все ключи `Game1.player.activeDialogueEvents`, с **днями до истечения**; отдельная секция `topic*` и `HarveyMod*` / owned (`ModTopicRegistry`).

#### Что должна менять

**Ничего.**

#### Что не должна менять

Buffs, InjuryState (кроме чтения).

#### Ожидаемый log prefix

```
[QA] injury_topic_dump
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **3** Upgrade hurt→badly | `topicHurt` снят, `topicBadlyHurt` + `topicHealthDamageCritical` |
| **5** Infection | `topicHarvey_DirtyWound` снят, `topicInfectedWound` есть |
| **5b** WetBandage neg | нет `topicHarvey_WetBandage` |
| FirstTreatment / Diagnosis bridge | `topicHarveyNeedsFirstTreatment`, `topicDiagnosisComplete` |
| Mine rescue | `topicMineRescuePending`, `topicMineInjuryRescue` |
| `injury_foreign_topic_add` | foreign topic не в owned-секции |
| CP cured flow vs `injury_phase_cure` | `topic*Cured` vs `topicTreatmentCompleted` |

---

### injury_validate_buffs

#### usage

```
injury_validate_buffs
```

#### Зачем нужна

Сверка ID из C# (`KnownTraumas`, `KnownComplications`, `GetPhaseBuffId` 1–3, `CureByInjury`, prescription/self-care IDs) с `Data/Buffs` (как `injury_audit_content`, но **только buffs**, exit code / summary для CI).

#### Что должна менять

**Ничего.**

#### Что не должна менять

Игровое состояние.

#### Ожидаемый log prefix

```
[QA] injury_validate_buffs
```

Итог: `[QA] injury_validate_buffs: OK` или `MISSING n: buffId1, buffId2`.

#### Тесты, которые разблокирует

| Область | Зачем |
|---------|-------|
| Регресс CP↔C# | 🔴 `buffTooCold` в C#, нет в CP ([02-cp-content-inventory](02-cp-content-inventory.md)) |
| Phase buff mapping | все `HarveyMod_*_Acute` из `GetPhaseBuffId` |
| Pre-release gate | автоматический FAIL до ручного прогона TC |
| **6** Фазовое лечение | phase buff существует в Data перед `injury_phase_advance` |

---

## Topic-команды

### injury_topic_add

#### usage

```
injury_topic_add <topicId> [days]
```

- `days` — default по типу topic (как в `DialogueManager.AddTopic` / константа травмы), или **7** для произвольного owned topic.
- Только **owned** topics (`ModTopicRegistry`) — иначе warn и отказ (для foreign — `injury_foreign_topic_add`).

#### Зачем нужна

Поставить мост CP без полного gameplay (FirstTreatment, Diagnosis, mine rescue pending, cured stub).

#### Что должна менять

`activeDialogueEvents[topicId]` (+ days). **Не** добавляет buff/state, если topic не связан с автоматическим триггером.

#### Что не должна менять

`MainInjuryId`, `ActiveDebuffs`, `ActiveComplications` (если не вызван отдельный debuff_add).

#### Ожидаемый log prefix

```
[QA] injury_topic_add topicId=... days=...
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| `HarveyMod_FirstTreatment` precondition | `topicHarveyNeedsFirstTreatment` |
| Treatment plan meeting | `topicDiagnosisComplete` |
| Mine rescue interception | `topicMineRescuePending` без pass-out |
| Checkup overdue dialogue | `topicHarvey_CheckupDue` |
| Topic conflict / reset | owned topic survives `injury_reset` cleanup test |

---

### injury_topic_remove

#### usage

```
injury_topic_remove <topicId>
```

#### Зачем нужна

Снять один topic для негативных TC (нет диалога, нет CP gate) без полного `injury_reset`.

#### Что должна менять

Удаляет ключ из `activeDialogueEvents` (через `DialogueManager` / registry-safe remove).

#### Что не должна менять

Buffs, InjuryState, другие topics.

#### Ожидаемый log prefix

```
[QA] injury_topic_remove topicId=... removed=yes|no
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **5** после infection | убрать stale `topicHarvey_WetBandage` вручную в негативе |
| CP event `!HasConversationTopic` | симуляция «игрок уже видел цепочку» |
| Neglect / compliance | сброс `topicHarvey_Neglect` между подшагами |
| Orphan `HarveyMineIntercept` | удаление non-`topic*` ключа ([02-cp](02-cp-content-inventory.md)) |

---

## Complication-команды

### injury_complication_add

#### usage

```
injury_complication_add <complicationBuffId> [ageDays]
```

- `complicationBuffId` — из `KnownComplications` (`HarveyMod_DirtyWound`, …).
- `ageDays` — optional: сдвинуть `ActiveComplications[id]` на `today - ageDays` (для infection roll **1→15%, 2→40%, 3+→100%**).

#### Зачем нужна

Добавить осложнение через **`ComplicationManager.TryApplyComplication`** (или эквивалент с теми же side effects), а не «голый» `injury_debuff_add` (который ставит `TreatmentStarted=true` на complication DebuffState и обходит eligibility).

#### Что должна менять

При успехе: mod buff, `ActiveComplications`, `DebuffState` complication, complication topic, лог `[Complication] MainInjury=...`.

При `ageDays`: только день в `ActiveComplications` (не ломая остальной state).

#### Что не должна менять

`MainInjuryId` (если не эскалация). Не запускать infection roll автоматически (только подготовка возраста).

#### Ожидаемый log prefix

```
[QA] injury_complication_add
```

При отказе eligibility: `[QA] injury_complication_add SKIP: <reason>` (зеркало `[DirtyWound] skip` / `[WetBandage] skip`).

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **4** DirtyWound | без 60+ мин в шахте |
| **4c** DirtyWound neg (fracture) | SKIP main not in DirtyInMines |
| **5** Dirty→Infected | `ageDays=2` + `advance_day` |
| **5b/5c** WetBandage | после `injury_test_setup deep_cuts_treated` |
| **8b** PainFlare ≠ severe | `HarveyMod_PainFlare` без смены main |
| WetStitches / Neglect | без дней neglect strikes |

---

### injury_complication_remove

#### usage

```
injury_complication_remove <complicationBuffId>
```

#### Зачем нужна

Снять осложнение через **`ComplicationManager.RemoveComplication`** (buff + state + topic + `ActiveComplications`), как после `TreatAllComplications` для одного ID.

#### Что должна менять

Удаляет complication buff, topic, `ActiveComplications`, complication `DebuffState`.

#### Что не должна менять

Main injury, другие complications, hospital/mine flags.

#### Ожидаемый log prefix

```
[QA] injury_complication_remove complication=... ok=yes|no
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| Treat complication path | состояние «до лечения» / «после» без `injury_phase_cure` |
| **5** already infected + stale comp | очистка перед assert |
| `injury_cleanup_invalid_complications` | точечный remove vs полный cleanup |
| Infection neg | убрать DirtyWound, проверить что roll не срабатывает |

---

## Time-travel QA (сдвиг дней в state)

### injury_test_age_injury

#### usage

```
injury_test_age_injury <buffId> <daysBack>
```

Сдвигает `DebuffState.InjuryStartDay` и `PhaseStartDay` на `today - daysBack` для указанной травмы в `ActiveDebuffs`.

#### Зачем нужна

Эмуляция «травма N дней назад» без `advance_day` × N (быстрее для phase neglect, simple treatment completion, checkup overdue).

#### Что должна менять

Только day-поля в `DebuffState` для `buffId` + `Save()`.

#### Что не должна менять

Buff duration (минуты), topics expiry (если не синхронизировать отдельно — **документировать**), `ReadyForNextPhase` flags.

#### Ожидаемый log prefix

```
[QA] injury_test_age_injury buffId=... injuryStart=... phaseStart=...
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **6** Фазовая смена | фаза «истекла» после `DayStarted CheckInjuryPhases` |
| Neglect phase | `CheckPhaseNeglect` без многодневного сна |
| Simple hurt 2d / badly 4d | `CheckSimpleTreatmentCompletion` |
| Checkup overdue | `MissedCheckupDays` pipeline |
| Infection untreated | neglected phase end warning |

---

### injury_test_age_complication

#### usage

```
injury_test_age_complication <complicationBuffId> <daysBack>
```

Сдвигает `ActiveComplications[compId]` на `today - daysBack`.

#### Зачем нужна

Ускорение infection escalation (**5**): день 1/2/3+ без сна.

#### Что должна менять

Только `ActiveComplications[compId]` (+ `Save()`).

#### Что не должна менять

Main injury, buff minutes, topics (days остаются — при необходимости отдельно `injury_topic_add`).

#### Ожидаемый log prefix

```
[QA] injury_test_age_complication comp=... startDay=...
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **5** Dirty→Infected | 15% / 40% / 100% rolls |
| WetBandage infection | wet comp age + `advance_day` |
| DirtyWound untreated roll | `TryApplyDirtyWoundFromUntreated` |

---

## Hospital-команды

### injury_hospital_status

#### usage

```
injury_hospital_status
```

#### Зачем нужна

Read-only снимок госпитализации: `IsHospitalized`, `HospitalizedInjuryId`, `HospitalizationReason`, admission/discharge timers, `PendingForcedHospitalizationWarning`, `DaysWithSevere`.

#### Что должна менять

**Ничего.**

#### Что не должна менять

Gameplay lock (не снимать hold).

#### Ожидаемый log prefix

```
[QA] injury_hospital_status
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **8a** Severe / hospital | lock, min stay |
| Concussion `ForceHospitalization` | immediate hosp |
| Pass-out hospital queue | `PendingHospitalPassOutEventId` |
| Discharge timing | `HospitalDischargeReadyShown`, minutes |

---

### injury_hospital_discharge

#### usage

```
injury_hospital_discharge
```

#### Зачем нужна

Принудительно завершить hospital hold (как успешный discharge): снять lock, warp optional **не** делать — только state + buff cleanup по правилам `HospitalizationManager`.

#### Что должна менять

`IsHospitalized=false`, снятие intensive/hospital buffs по injury, outpatient при badly hurt, сброс pending forced warning (по политике discharge).

#### Что не должна менять

Main injury identity (если не выписка = cure), mine rescue flags, unrelated topics.

#### Ожидаемый log prefix

```
[QA] injury_hospital_discharge injury=... ok=yes|no
```

#### Тесты, которые разблокирует

| TC / сценарий | Зачем |
|---------------|-------|
| **8a** после severe | выйти из Hospital без ожидания min stay |
| Badly hurt pipeline | intensive → outpatient |
| Blocked interaction | проверка FSM после discharge |
| Repeated TC | не перезагружать сейв |

---

## Сценарии injury_test_setup

Команда **одна**; сценарий — **один** аргумент. Каждый сценарий = `injury_reset` + минимальные шаги для одного TC (без объединения нескольких механик).

### injury_test_setup

#### usage

```
injury_test_setup <scenarioId>
```

#### Зачем нужна

Стандартизированная подготовка: один вызов вместо 5–10 команд в чеклисте; снижает ошибки AI-агента.

#### Что должна менять

Зависит от сценария (см. таблицу). Всегда начинает с полного `injury_reset`, затем только шаги этого сценария.

#### Что не должна менять

`eventsSeen`, foreign topics, vanilla state (кроме явно указанного в сценарии).

#### Ожидаемый log prefix

```
[QA] injury_test_setup scenario=<id>
[QA] injury_test_setup done: MainInjury=... complications=...
```

#### Тесты, которые разблокирует

Все перечисленные сценарии — см. колонку «TC» в таблице.

---

### Таблица сценариев

| scenarioId | Подготовка (после reset) | TC / назначение |
|------------|---------------------------|-----------------|
| `simple_hurt` | `injury_debuff_add buffHurt` | **1**, **3**, simple treatment |
| `simple_badly_hurt` | `injury_debuff_add buffBadlyHurt` (+ critical topic) | **8a** severe, badly hurt hosp |
| `deep_cuts` | `injury_debuff_add buffDeepCuts` | **4**, **4b** neg/pos, MainInjury |
| `deep_cuts_treated` | `deep_cuts` + `injury_harvey_click` (StartTreatment) | **4b**, **5c**, WetBandage pos |
| `fractured_bone` | `injury_debuff_add buffFracturedBone` | **1**, **2**, **4c** neg dirty |
| `concussion` | `injury_debuff_add buffConcussion` | Severe, forced hosp config |
| `infected_wound` | `injury_debuff_add buffInfectedWound` | **5b** wet neg |
| `surgical_wound` | `injury_debuff_add buffSurgicalWound` | WetStitches, post-op |
| `wet_bandage` | `deep_cuts_treated` + `injury_complication_add HarveyMod_WetBandage` | **5c**, rain + StardewMCP |
| `dirty_wound` | `deep_cuts` + `injury_complication_add HarveyMod_DirtyWound` | **5**, infection |
| `wet_stitches` | `surgical_wound` + `injury_complication_add HarveyMod_WetStitches` | KeepDry violation |
| `neglect` | фазовая травма + `injury_test_age_injury` + neglect comp / strikes | **11**, neglect mail |
| `mine_forbidden_pending` | severe main + `MineWarningDay=today` (state only) | Mine forbidden mail next day |
| `mine_forbidden_active` | `HarveyMod_MineForbidden` buff + `MineForbiddenAppliedDay=today` | Interception, debuff day |
| `mine_rescue_pending` | то же, что `injury_debug_mine_rescue` | Mine rescue DayStarted |
| `forced_hospitalization_ready` | serious main + `PendingForcedHospitalizationWarning=true` | Forced hosp warning |
| `cold` | `injury_debuff_add buffCold` | Rain/cold, phased cold |
| `pain_flare` | любая main + `injury_complication_add HarveyMod_PainFlare` | **8b** не severe |

**Не дублировать в setup:** полный save/load, CP cutscene play, StardewMCP warp — остаются в TC вручную.

---

## Сводка: отличие от существующих команд

| Новая команда | Ближайший аналог сегодня | Почему недостаточно |
|---------------|-------------------------|---------------------|
| `injury_state_dump` | `injury_debug_dump` | HUD-текст, нет стабильного полного state для MCP parse |
| `injury_buff_dump` | `injury_medical_snapshot` | snapshot только treatable DebuffState, не все applied buffs |
| `injury_topic_dump` | `injury_audit_content` | audit = CP keys exist, не активные topics игрока |
| `injury_topic_add/remove` | `injury_foreign_topic_add` | foreign only; нет owned bridge topics |
| `injury_complication_add/remove` | `injury_debuff_add` | другой DebuffState path, нет eligibility/age |
| `injury_test_age_*` | `advance_day` × N | медленно, меняет весь мир |
| `injury_test_setup` | цепочка в md | ошибки порядка, разный baseline между чатами |
| `injury_hospital_*` | StardewMCP teleport | teleport ≠ state hold |
| `injury_validate_buffs` | `injury_audit_content` | audit = mail+dialogue, не buff registry |

---

## Приоритет реализации

### P0 — нужно обязательно

| Команда | Причина |
|---------|---------|
| `injury_state_dump` | Базовый assert для всех MainInjury TC через MCP |
| `injury_buff_dump` | Единственный надёжный способ видеть phase/cure/orphan buffs |
| `injury_topic_dump` | Topics — половина CP bridges; `injury_phase_list` их не показывает |
| `injury_validate_buffs` | Регресс CP↔C# до любого прогона (buffTooCold, phase IDs) |

### P1 — полезно

| Команда | Причина |
|---------|---------|
| `injury_complication_add` | **4**, **5** без шахты/дождя |
| `injury_complication_remove` | Treat path, stale cleanup |
| `injury_test_age_injury` | Фазы, neglect, simple cure без multi-day |
| `injury_test_age_complication` | Infection escalation |
| `injury_topic_add` | CP event preconditions |
| `injury_topic_remove` | Негативные TC, orphan keys |
| `injury_test_setup` | Стандартный baseline (минимум: `deep_cuts`, `dirty_wound`, `deep_cuts_treated`, `simple_hurt`) |
| `injury_hospital_status` | **8a**, concussion hosp |

### P2 — можно позже

| Команда | Причина |
|---------|---------|
| `injury_hospital_discharge` | Реже нужен, можно ждать min stay вручную |
| `injury_test_setup` (все 18 сценариев) | Расширять по мере прогона TC |
| `injury_topic_dump [filter]` | Удобство, не блокер |
| Расширение `injury_validate_buffs` → stress buffs | Вне InjuryCare scope |

---

## Injury MCP

Каждая новая команда должна получить зеркало в `Testing/InjuryMcpServer.cs` + строка в [`injury-mcp.md`](injury-mcp.md).

**Ответ MCP после мутации:** для setup/add/remove/age/discharge — append `injury_phase_list` или краткий `[QA]` summary (как сейчас для `injury_debuff_add`).

**Ответ MCP для dump/validate:** полный текст отчёта в return body (не только SMAPI log).

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, ограничения, формат TC.
2. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — поля `InjuryState`, ID, пробелы QA.
3. [02-cp-content-inventory.md](02-cp-content-inventory.md) — CP buff/event/mail ↔ C#.
4. [03-existing-debug-commands.md](03-existing-debug-commands.md) — поведение существующих `injury_*` (не дублировать).
5. **Этот файл** — [04-missing-debug-commands.md](04-missing-debug-commands.md) — спецификация недостающих команд.
6. **Реализация:** сначала **P0 dump-команды** (`injury_state_dump`, `injury_buff_dump`, `injury_topic_dump`, `injury_validate_buffs`) в `ModEntry.cs` + Injury MCP + [`FOR_TEST.md`](FOR_TEST.md).
7. [main-injury-testcases.md](main-injury-testcases.md) — обновить подготовку TC на новые команды после реализации.
8. [injury-mcp.md](injury-mcp.md) — схемы tools для Cursor `CallMcpTool`.
