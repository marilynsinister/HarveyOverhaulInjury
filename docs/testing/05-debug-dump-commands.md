# P0 dump-команды (QA read-only)

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Спецификация: [04-missing-debug-commands.md](04-missing-debug-commands.md)  
> Реализация: `Testing/QaDumpCommands.cs`, регистрация в `ModEntry.cs`, зеркало в `Testing/InjuryMcpServer.cs`

**Область:** только read-only дампы. Ничего не меняют в buffs, topics, state, мире.

**Префикс лога:** все команды пишут в SMAPI log с `[QA]`.

**Требование:** загруженное сохранение (`Context.IsWorldReady`).

---

## Общий цикл assert

```
injury_reset
injury_debuff_add buffFracturedBone
injury_state_dump      → MainInjuryId=buffFracturedBone
injury_buff_dump       → buff=buffFracturedBone tags=mod,trauma
injury_topic_dump      → topic=topicFracturedBone days=… owned
injury_validate_buffs  → OK или MISSING …
```

Через **Injury MCP** (`user-harvey-injury`): те же имена tools; тело ответа = полный текст отчёта.

---

## injury_state_dump

### usage

```
injury_state_dump
```

Машиночитаемый снимок `InjuryState` (save key `injury_state`): MainInjury, DebuffState, осложнения, шахта, госпитализация, pass-out, neglect, rain, prescriptions, rehab.

### Отличие от injury_debug_dump

| Команда | Формат | Назначение |
|---------|--------|------------|
| `injury_debug_dump` | человекочитаемый HUD (F10 full) | отладка в игре |
| `injury_state_dump` | стабильный `key=value` | MCP parse / assert TC |

### Expected log (после `injury_reset` + `injury_debuff_add buffDeepCuts`)

```
[QA] injury_state_dump
MainInjuryId=buffDeepCuts
TreatmentComplianceScore=0
DaysWithSevere=0
ActiveDebuffs.count=1
ActiveDebuffs.buffDeepCuts.BuffId=buffDeepCuts
ActiveDebuffs.buffDeepCuts.TreatmentStarted=False
ActiveDebuffs.buffDeepCuts.CurrentPhase=0
ActiveDebuffs.buffDeepCuts.TotalPhases=3
...
ActiveComplications.count=0
LastInfectionEscalationDay=0
...
IsHospitalized=False
NeedsMineRescueEvent=False
SavedActiveBuffs=(none)
```

### Assert-примеры

| Сценарий | Поле |
|----------|------|
| MainInjury baseline | `MainInjuryId=buffFracturedBone` |
| StartTreatment | `ActiveDebuffs.buffDeepCuts.TreatmentStarted=True`, `CurrentPhase=1` |
| Infection | `ActiveComplications.count=0` после эскалации; `MainInjuryId=buffInfectedWound` |
| Mine rescue | `NeedsMineRescueEvent=True`, `PendingMineRescueEventId=…` |
| Hospital | `IsHospitalized=True`, `HospitalizedInjuryId=…` |

---

## injury_buff_dump

### usage

```
injury_buff_dump
```

Все активные баффы на игроке (vanilla + mod) с тегами:

| Тег | Значение |
|-----|----------|
| `mod` | ID есть в `Data/Buffs` |
| `trauma` | `KnownTraumas` |
| `complication` | `KnownComplications` |
| `phase` | `InjuryManager.GetPhaseBuffId` (фазы 1–3) |
| `cure` | `CureBuffs` / `SimpleInjuryCures` |
| `prescription` | `PrescriptionIds` |
| `orphan` | buff активен, но нет `DebuffState` и не trauma/complication |
| `vanilla` | ни один mod-тег не подошёл |

### Expected log (после `injury_debuff_add buffFracturedBone`)

```
[QA] injury_buff_dump
count=1
SavedActiveBuffs=(none)
buff=buffFracturedBone tags=mod,trauma
```

### Expected log (после `injury_harvey_click` на фазовой травме)

```
[QA] injury_buff_dump
count=1
SavedActiveBuffs=(none)
buff=HarveyMod_DeepCuts_Acute tags=mod,phase
```

*(base `buffDeepCuts` снят, phase buff активен — типичный assert TC-4b/6.)*

### Expected log (после `injury_phase_cure`)

```
[QA] injury_buff_dump
count=1
buff=buffHarveyCare tags=mod,cure
```

---

## injury_topic_dump

### usage

```
injury_topic_dump
```

Все ключи `Game1.player.activeDialogueEvents` с **днями до истечения**. Секции: `all`, `topic*`, `HarveyMod*`, `owned` (`ModTopicRegistry`).

### Expected log (после `injury_debuff_add buffBadlyHurt`)

```
[QA] injury_topic_dump
count=2
--- all ---
topic=topicBadlyHurt days=4 owned
topic=topicHealthDamageCritical days=3 owned
--- topic count=2 ---
topic=topicBadlyHurt days=4
topic=topicHealthDamageCritical days=3
--- HarveyMod count=0 ---
--- owned count=2 ---
topic=topicBadlyHurt days=4
topic=topicHealthDamageCritical days=3
```

### Expected log (foreign topic)

После `injury_foreign_topic_add topic_joja_Certified 5`:

```
topic=topic_joja_Certified days=5 foreign
```

---

## injury_validate_buffs

### usage

```
injury_validate_buffs
```

Сверка C# ID с `Data/Buffs` (CP). Проверяются:

- все `KnownTraumas` (buffId);
- все `KnownComplications`;
- все поля `CureBuffs` + значения `SimpleInjuryCures`;
- phase buff IDs: `GetPhaseBuffId(trauma, 1..3)` для каждой травмы.

**Не меняет** игровое состояние. Не требует активной травмы — только загруженный сейв (для доступа к `Data/Buffs`).

### Expected log (все ID на месте)

```
[QA] injury_validate_buffs: OK
result=OK checked=62
```

*(число `checked` может расти при добавлении травм — важен `result=OK`.)*

### Expected log (регресс CP↔C#)

```
[QA] injury_validate_buffs: MISSING 1: buffTooCold
result=MISSING missing_count=1 ids=buffTooCold checked=62
```

### Когда вызывать

- **Pre-release gate** — до прогона TC;
- после правок `Data/Buffs` в CP или `GetPhaseBuffId` в C#;
- если `injury_debuff_add` / `injury_phase_advance` падает с «buff not found».

---

## Injury MCP

| Tool | Аргументы | Ответ |
|------|-----------|-------|
| `injury_state_dump` | — | полный state report |
| `injury_buff_dump` | — | полный buff report |
| `injury_topic_dump` | — | полный topic report |
| `injury_validate_buffs` | — | summary + body (`OK` / `MISSING`) |

Пример Cursor:

```
CallMcpTool user-harvey-injury injury_state_dump
CallMcpTool user-harvey-injury injury_validate_buffs
```

---

## Сравнение с соседними командами

| Команда | Read-only | Что даёт |
|---------|:---------:|----------|
| `injury_phase_list` | да | MainInjury, phases, complications (кратко) |
| `injury_debug_dump` | да | HUD-текст, смешанный |
| `injury_medical_snapshot` | да | pipeline / DebuffState treatable only |
| `injury_audit_content` | да | CP mail/dialogue keys exist |
| **`injury_state_dump`** | да | **полный InjuryState key=value** |
| **`injury_buff_dump`** | да | **все applied buffs + tags** |
| **`injury_topic_dump`** | да | **активные topics игрока** |
| **`injury_validate_buffs`** | да | **registry buff IDs vs Data/Buffs** |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI.
2. [04-missing-debug-commands.md](04-missing-debug-commands.md) — **P1** команды (complication_add, topic_add, test_setup, …).
3. [main-injury-testcases.md](main-injury-testcases.md) — обновить подготовку TC на новые dump assert.
4. [injury-mcp.md](injury-mcp.md) — добавить схемы 4 новых tools (если ещё не обновлено).
5. **Следующий чат:** P1 `injury_complication_add` / `injury_test_setup` (не dump).
