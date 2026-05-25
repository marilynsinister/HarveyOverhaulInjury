# Medical pipeline — ручная проверка (SMAPI)

**Дата:** 2026-05-25  
**Версия:** после refactor `InteractionHandler` (dialogue → pending → apply) + защита конфликтов  
**Мод:** `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`

---

## Ограничение среды проверки

| Что проверено | Как |
|---------------|-----|
| Сборка C# | `dotnet build` — **OK** |
| CP-ключи диалогов | `tmpMap/audit_pipeline_keys.py` — **OK** (Treat/PhaseTransition/Recovery_Complete) |
| Логика приоритетов D/E | Разбор `TryResolveMedicalAction` — **OK** |
| Клик по Harvey в игре | **Не выполнялся агентом** (нет доступа к игровому UI) |

Для in-game шагов ниже — процедура, ожидаемые значения `injury_medical_snapshot` / F10 и строки SMAPI-лога.  
Новые команды для проверки: `injury_medical_snapshot`, `injury_foreign_topic_add`.

**Предусловия:** сохранение загружено, Harvey в клинике (Hospital), будний день, игрок свободен.

---

## Общий чеклист после каждого клика

1. **До клика:** `injury_medical_snapshot`  
   - `Decision now:` должен показывать `SELECT <Type>:<target>`  
   - `Standard dialogue:` → `BLOCKED: InjuryCare medical action (...)`
2. **Клик по Harvey** (ActionButton)
3. **Во время диалога:** vanilla/topic-диалог CP **не** должен перехватить разговор
4. **После закрытия DialogueBox:** снова `injury_medical_snapshot` + `injury_phase_list`
5. **F10:** `Last click`, `Pending`, `Decision now`, `Standard dialogue`
6. **SMAPI log:** `[MedicalAction] queued ...` → `[MedicalAction] dialogue shown ...` → `[MedicalAction] applied type=...`

---

## Сценарий A — старт лечения сотрясения

### Команды

```
injury_reset
injury_debuff_add buffConcussion
injury_medical_snapshot
```

→ **Клик по Harvey** → закрыть DialogueBox →:

```
injury_medical_snapshot
injury_phase_list
```

### Ожидание до клика

| Проверка | Ожидание |
|----------|----------|
| `Decision now` | `SELECT StartTreatment:buffConcussion` |
| `Standard dialogue` | `BLOCKED: ...` |
| DebuffState | `TreatmentStarted=false`, `CurrentPhase=0` |
| Баффы | `buffConcussion` ACTIVE, фазовых нет |

### Ожидание при клике (до закрытия диалога)

| Проверка | Ожидание |
|----------|----------|
| Vanilla-диалог | **Не открывается** (suppress) |
| `topicConcussion` CP | **Не перехватывает** (InjuryCare pipeline) |
| Реакция | `TreatWithReaction` → эмоция + текст (concussion = critical) |
| Текст | Префикс `Treat_Concussion_Before_*` (CP) |
| Механика | **Нет** смены баффов/state до закрытия |

### Ожидание после закрытия DialogueBox

| Проверка | Ожидание |
|----------|----------|
| `buffConcussion` | снят |
| `HarveyMod_Concussion_Acute` | ACTIVE |
| DebuffState | `TreatmentStarted=true`, `CurrentPhase=1` |
| SMAPI | `[MedicalAction] applied type=StartTreatment injury=buffConcussion` |

### Статус

| Критерий | Код / CP | In-game |
|----------|----------|---------|
| Pipeline StartTreatment | ✅ `ApplyPendingStartTreatment` → `StartPhasedTreatment` | ⏳ |
| CP Treat_Concussion_Before | ✅ audit | ⏳ |
| Suppress vanilla | ✅ `OnButtonPressed` | ⏳ |

---

## Сценарий B — переход фазы 2→3 (сотрясение)

### Подготовка

```
injury_reset
injury_debuff_add buffConcussion
```

→ **Клик 1:** старт лечения (сценарий A) → фаза 1.

```
injury_phase_advance buffConcussion
```

(теперь `CurrentPhase=2`, бафф `HarveyMod_Concussion_Rest`)

```
injury_phase_ready buffConcussion 1
injury_foreign_topic_add topic_joja_Certified 5
injury_medical_snapshot
```

→ **Клик 2 по Harvey** → закрыть DialogueBox →:

```
injury_medical_snapshot
injury_phase_list
```

### Ожидание до клика 2

| Проверка | Ожидание |
|----------|----------|
| `Decision now` | `SELECT AdvancePhase:buffConcussion` |
| `CurrentPhase` | `2/3` |
| `ReadyForNextPhase` | `true` |
| Foreign topic | `topic_joja_Certified` есть, но **не запускается** |

### Ожидание при клике 2

| Проверка | Ожидание |
|----------|----------|
| Текст | `PhaseTransition_Concussion_3_*` (осмотр после домашнего режима) |
| `topic_joja_Certified` | **Не срабатывает** |
| Механика | **После** закрытия диалога |

### Ожидание после закрытия

| Проверка | Ожидание |
|----------|----------|
| `HarveyMod_Concussion_Rest` | снят |
| `HarveyMod_Concussion_Limited` | ACTIVE |
| DebuffState | `CurrentPhase=3`, `ReadyForNextPhase=false` |
| SMAPI | `[MedicalAction] applied type=AdvancePhase` |

### Статус

| Критерий | Код / CP | In-game |
|----------|----------|---------|
| AdvancePhase после диалога | ✅ `ApplyPendingAdvancePhase` | ⏳ |
| PhaseTransition_Concussion_3 | ✅ CP (3 варианта) | ⏳ |
| Foreign topic не мешает | ✅ suppress + gate | ⏳ |

---

## Сценарий C — выздоровление

### Подготовка

(продолжение B или быстрый путь:)

```
injury_reset
injury_debuff_add buffConcussion
```

→ Клик: старт → `injury_phase_advance` ×2 (фаза 3) **или** довести до phase 3 вручную.

```
injury_phase_recovery buffConcussion 1
injury_medical_snapshot
```

→ **Клик по Harvey** → закрыть DialogueBox →:

```
injury_medical_snapshot
injury_phase_list
```

### Ожидание до клика

| Проверка | Ожидание |
|----------|----------|
| `Decision now` | `SELECT CompleteRecovery:buffConcussion` |
| `CurrentPhase` | `3/3`, `ReadyForRecovery=true` |

### Ожидание при клике

| Проверка | Ожидание |
|----------|----------|
| Текст | `Recovery_Complete_Concussion_*` |
| `topic*Cured` | **Не создаётся** (нет второго финального разговора) |

### Ожидание после закрытия

| Проверка | Ожидание |
|----------|----------|
| Фазовые баффы | все сняты |
| DebuffState `buffConcussion` | **удалён** |
| `buffHarveyCare` | ACTIVE |
| Ready flags | отсутствуют (state удалён) |
| SMAPI | `[MedicalAction] applied type=CompleteRecovery` |

### Статус

| Критерий | Код / CP | In-game |
|----------|----------|---------|
| Recovery без topic*Cured | ✅ `ApplyMechanicalPhasedRecovery` | ⏳ |
| Recovery_Complete CP | ✅ 3 варианта | ⏳ |

---

## Сценарий D — несколько травм

### Команды

```
injury_reset
injury_debuff_add buffConcussion
injury_debuff_add buffDeepCuts
```

→ **Клик:** старт лечения concussion (A).

```
injury_phase_ready buffConcussion 1
injury_medical_snapshot
```

### Ожидание `Decision now` (без клика)

```
SELECT AdvancePhase:buffConcussion
```

**Не** `StartTreatment:buffDeepCuts` — нелеченная DeepCuts (приоритет C) ниже готовой фазы (B).

| Травма | TreatmentStarted | Ready |
|--------|------------------|-------|
| buffConcussion | true | ReadyForNextPhase |
| buffDeepCuts | false | — |

### Статус

| Критерий | Результат |
|----------|-----------|
| Приоритет B > C | ✅ подтверждено кодом `TryResolveMedicalAction` |
| Concussion priority 100 > DeepCuts 65 | ✅ при равном типе действия |
| In-game | ⏳ |

---

## Сценарий E — осложнение + готовая фаза

### Команды

```
injury_reset
injury_debuff_add buffConcussion
```

→ Клик: старт лечения.

```
injury_phase_ready buffConcussion 1
injury_debuff_add HarveyMod_WetBandage
injury_medical_snapshot
```

### Ожидание клика 1

```
SELECT AdvancePhase:buffConcussion
```

**Не** `TreatComplications:HarveyMod_WetBandage`.

→ После клика 1 и закрытия диалога:

```
injury_medical_snapshot
```

Ожидание:

```
SELECT TreatComplications:HarveyMod_WetBandage
```

### Статус

| Критерий | Результат |
|----------|-----------|
| B (AdvancePhase) > D (TreatComplications) | ✅ код |
| Осложнение по DebuffState, не по чужому баффу | ✅ `KnownComplicationBuffIds` |
| In-game | ⏳ |

---

## Сводная таблица

| Сценарий | Decision (ожид.) | CP ключи | Код pipeline | In-game |
|----------|------------------|----------|--------------|---------|
| A StartTreatment | `StartTreatment:buffConcussion` | Treat_Before ✅ | ✅ | ⏳ |
| B AdvancePhase 2→3 | `AdvancePhase:buffConcussion` | PhaseTransition_3 ✅ | ✅ | ⏳ |
| C CompleteRecovery | `CompleteRecovery:buffConcussion` | Recovery_Complete ✅ | ✅ | ⏳ |
| D Multi-injury | Advance > Start | — | ✅ | ⏳ |
| E Phase + complication | Phase → Complications | — | ✅ | ⏳ |

**Легенда:** ✅ — проверено статически / audit; ⏳ — требует прохождения в игре одним кликом по Harvey.

---

## Как завершить in-game проверку (5–10 мин)

1. Перезапустить игру (подтянуть свежий DLL после `dotnet build`).
2. Пройти A→C на одном save или с `injury_reset` между сценариями.
3. После каждого клика сверить `injury_medical_snapshot` с таблицами выше.
4. В SMAPI log искать `[MedicalAction]` — порядок `queued` → `dialogue shown` → `applied`.
5. Отметить в этом файле колонку In-game: ✅/❌ + заметки.

### Типичные строки SMAPI (успех A)

```
[MedicalAction] queued type=StartTreatment injury=buffConcussion
[MedicalAction] dialogue shown key/prefix=Treat_Concussion_Before
😊 Харви отреагировал эмоцией ...
[MedicalAction] applied type=StartTreatment injury=buffConcussion complications=0
✅ Применена Фаза 1: HarveyMod_Concussion_Acute
```

---

## Связанные файлы

- `EventHandlers/InteractionHandler.cs` — pipeline
- `Managers/TreatmentManager.cs` — `StartPhasedTreatment`, `AdvanceInjuryToNextPhase`, `ApplyMechanicalPhasedRecovery`
- `docs/testing/FOR_TEST.md` — общий справочник команд
- `tmpMap/audit_pipeline_keys.py` — аудит CP-ключей
