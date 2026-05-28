# P0 setup-команды (QA mutations)

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Спецификация: [04-missing-debug-commands.md](04-missing-debug-commands.md)  
> Реализация: `Testing/QaSetupCommands.cs`, `Managers/ComplicationManager.cs` (QA-методы), регистрация в `ModEntry.cs`, зеркало в `Testing/InjuryMcpServer.cs`

**Область:** точечные мутации topics, complications, age-travel, hospital. Не включает `injury_test_setup`.

**Префикс лога:** все команды пишут в SMAPI log с `[QA]`.

**Требование:** загруженное сохранение (`Context.IsWorldReady`).

**Age-команды:** не меняют `Game1.stats.DaysPlayed` — только day-поля в `InjuryState`.

---

## Общий цикл подготовки TC

```
injury_reset
injury_debuff_add buffDeepCuts
injury_harvey_click                    # StartTreatment для WetBandage
injury_complication_add HarveyMod_DirtyWound
injury_test_age_complication HarveyMod_DirtyWound 2
injury_state_dump                      # assert ActiveComplications
StardewMCP: advance_day                # infection roll
```

Через **Injury MCP** (`user-harvey-injury`): те же имена tools; после мутаций в ответе — `[QA] summary` + `injury_phase_list`.

---

## injury_topic_add

### usage

```
injury_topic_add <topicId> [days]
```

- Только **owned** topics (`ModTopicRegistry`) — иначе `SKIP` и отказ.
- `days` — default по типу topic (сумма фаз травмы из `KnownTraumas`, 4/7 для осложнений) или **7** для прочих owned.

### Что меняет

`activeDialogueEvents[topicId]` (+ days). Не добавляет buff/state.

### Expected log

```
[QA] injury_topic_add
[QA] injury_topic_add topicId=topicHarveyNeedsFirstTreatment days=7
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| `HarveyMod_FirstTreatment` | `topicHarveyNeedsFirstTreatment` без gameplay |
| Treatment plan meeting | `topicDiagnosisComplete` |
| Mine rescue interception | `topicMineRescuePending` |
| Checkup overdue dialogue | `topicHarvey_CheckupDue` |
| Topic conflict / reset | owned topic survives cleanup tests |

---

## injury_topic_remove

### usage

```
injury_topic_remove <topicId>
```

### Что меняет

Удаляет ключ из `activeDialogueEvents`. Buffs и InjuryState не трогает.

### Expected log

```
[QA] injury_topic_remove
[QA] injury_topic_remove topicId=topicHarvey_WetBandage removed=yes
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| **5** infection neg | убрать stale `topicHarvey_WetBandage` |
| CP `!HasConversationTopic` | симуляция «цепочка уже пройдена» |
| Neglect / compliance | сброс `topicHarvey_Neglect` между подшагами |
| Orphan keys | удаление non-`topic*` без `injury_reset` |

---

## injury_complication_add

### usage

```
injury_complication_add <complicationBuffId> [ageDays]
```

- ID из `KnownComplications` (`HarveyMod_DirtyWound`, …).
- `ageDays` — optional: `ActiveComplications[id] = today - ageDays` (+ синхрон DebuffState start day).

### Что меняет (при успехе)

Через `ComplicationManager.TryApplyComplicationForQa`:

- mod buff;
- `ActiveComplications`;
- `DebuffState` осложнения (`CreateComplicationState`);
- complication topic, если известен.

`MainInjuryId` не меняется. Infection roll не запускается автоматически.

### Eligibility / SKIP

| Осложнение | Условие | Пример SKIP |
|------------|---------|-------------|
| `HarveyMod_DirtyWound` | main ∈ DirtyInMines + buff/phase | `main not in DirtyInMines` (**4c** neg) |
| `HarveyMod_WetBandage` | treatment started + bandage + WetBandageSensitive | `treatment not started` |
| `HarveyMod_WetStitches` | main = surgical/shrapnel | `main not surgical/shrapnel` |
| `HarveyMod_PainFlare` | storm or overwork sensitive main | `main not pain-sensitive` |
| `HarveyMod_Neglect`, `HarveyMod_AllergicRash` | нет дубликата | `already active` |

### Expected log

```
[QA] injury_complication_add
[QA] injury_complication_add complication=HarveyMod_DirtyWound ok=yes ageDays=2
```

или

```
[QA] injury_complication_add SKIP: main not in DirtyInMines
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| **4** DirtyWound | без 60+ мин в шахте |
| **4c** DirtyWound neg | SKIP на fracture |
| **5** Dirty→Infected | `ageDays=2` + `advance_day` |
| **5b/5c** WetBandage | после treated deep cuts |
| **8b** PainFlare | без смены main на severe |
| WetStitches / Neglect | без дней neglect strikes |

---

## injury_complication_remove

### usage

```
injury_complication_remove <complicationBuffId>
```

### Что меняет

Через `ComplicationManager.RemoveComplicationForQa`: buff, `ActiveComplications`, `DebuffState`, topic.

### Expected log

```
[QA] injury_complication_remove
[QA] injury_complication_remove complication=HarveyMod_DirtyWound ok=yes
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| Treat complication path | состояние до/после без `injury_phase_cure` |
| **5** stale comp cleanup | перед assert |
| Infection neg | убрать DirtyWound, проверить что roll не срабатывает |
| `injury_cleanup_invalid_complications` | точечный remove vs полный cleanup |

---

## injury_test_age_injury

### usage

```
injury_test_age_injury <buffId> <daysBack>
```

Сдвигает `DebuffState.InjuryStartDay` и `PhaseStartDay` на `today - daysBack` для указанной травмы в `ActiveDebuffs`.

### Что не меняет

Buff duration (минуты), topic expiry, `ReadyForNextPhase` / `ReadyForRecovery`, `Game1.stats.DaysPlayed`.

### Expected log

```
[QA] injury_test_age_injury
[QA] injury_test_age_injury buffId=buffDeepCuts injuryStart=12 phaseStart=12 today=14
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| **6** Фазовая смена | фаза «истекла» после `DayStarted CheckInjuryPhases` |
| Neglect phase | `CheckPhaseNeglect` без многодневного сна |
| Simple hurt 2d / badly 4d | `CheckSimpleTreatmentCompletion` |
| Checkup overdue | `MissedCheckupDays` pipeline |
| Infection untreated | neglected phase end warning |

---

## injury_test_age_complication

### usage

```
injury_test_age_complication <complicationBuffId> <daysBack>
```

Сдвигает только `ActiveComplications[compId]` на `today - daysBack`.

### Что не меняет

Main injury, buff minutes, topics (days остаются — при необходимости отдельно `injury_topic_add`).

### Expected log

```
[QA] injury_test_age_complication
[QA] injury_test_age_complication comp=HarveyMod_DirtyWound startDay=12 today=14
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| **5** Dirty→Infected | 15% / 40% / 100% rolls (день 1/2/3+) |
| WetBandage infection | wet comp age + `advance_day` |
| DirtyWound untreated roll | `TryApplyDirtyWoundFromUntreated` |

---

## injury_hospital_status

### usage

```
injury_hospital_status
```

Read-only. Не снимает hospital lock.

### Expected log

```
[QA] injury_hospital_status
IsHospitalized=True
HospitalizedInjuryId=buffBadlyHurt
HospitalizationReason=severe
HospitalAdmissionDay=14
HospitalMinStayMinutes=120
HospitalDischargeReadyShown=False
PendingForcedHospitalizationWarning=False
DaysWithSevere=1
PendingHospitalPassOutEventId=(none)
CanDischarge=False
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| **8a** Severe / hospital | lock, min stay, timers |
| Concussion forced hosp | immediate admission state |
| Pass-out hospital queue | `PendingHospitalPassOutEventId` |
| Discharge timing | `HospitalDischargeReadyShown`, `CanDischarge` |

---

## injury_hospital_discharge

### usage

```
injury_hospital_discharge
```

Принудительно завершает hospital hold: `HospitalizationManager.Discharge()` — снятие lock, cleanup buffs по правилам discharge (intensive → outpatient для badly hurt). **Warp не делает.**

### Expected log

```
[QA] injury_hospital_discharge
[QA] injury_hospital_discharge injury=buffBadlyHurt ok=yes
```

Если не госпитализирован:

```
[QA] injury_hospital_discharge injury=(none) ok=no reason=not hospitalized
```

### Разблокирует тесты

| TC / сценарий | Зачем |
|---------------|-------|
| **8a** после severe | выйти из Hospital без ожидания min stay |
| Badly hurt pipeline | intensive → outpatient |
| Blocked interaction | FSM после discharge |
| Repeated TC | не перезагружать сейв |

---

## Injury MCP

| Tool | Аргументы | Ответ |
|------|-----------|-------|
| `injury_topic_add` | `topic_id`, опц. `days` | `[QA] summary` + phase list |
| `injury_topic_remove` | `topic_id` | `[QA] summary` + phase list |
| `injury_complication_add` | `complication_id`, опц. `age_days` | `[QA] summary` + phase list |
| `injury_complication_remove` | `complication_id` | `[QA] summary` + phase list |
| `injury_test_age_injury` | `buff_id`, `days_back` | `[QA] summary` + phase list |
| `injury_test_age_complication` | `complication_id`, `days_back` | `[QA] summary` + phase list |
| `injury_hospital_status` | — | полный hospital report |
| `injury_hospital_discharge` | — | `[QA] summary` + phase list |

Пример Cursor:

```
CallMcpTool user-harvey-injury injury_complication_add {"complication_id":"HarveyMod_DirtyWound","age_days":2}
CallMcpTool user-harvey-injury injury_hospital_status
```

---

## Сравнение с соседними командами

| Команда | Меняет state | Отличие |
|---------|:------------:|---------|
| `injury_debuff_add` (complication) | да | голый path, `TreatmentStarted=true`, без eligibility |
| `injury_foreign_topic_add` | topic only | только foreign, не owned |
| `injury_phase_cure` | full cure | не для одного complication |
| `injury_cleanup_invalid_complications` | remove stale | массовый cleanup, не точечный |
| StardewMCP `teleport_player` | location | не hospital hold state |
| **`injury_complication_add`** | complication | eligibility + ActiveComplications age |
| **`injury_test_age_*`** | day fields only | не `advance_day` × N |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI.
2. [05-debug-dump-commands.md](05-debug-dump-commands.md) — assert после setup (`injury_state_dump`, …).
3. [04-missing-debug-commands.md](04-missing-debug-commands.md) — **следующий:** `injury_test_setup` (не в этом чате).
4. [main-injury-testcases.md](main-injury-testcases.md) — обновить подготовку TC на новые setup-команды.
5. [injury-mcp.md](injury-mcp.md) — добавить схемы 8 новых tools (если ещё не обновлено).
