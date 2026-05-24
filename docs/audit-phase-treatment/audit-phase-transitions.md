# Аудит фазового лечения травм

**Дата:** 2026-05-24  
**Тип:** read-only аудит (код не менялся)  
**Цель:** проверить цепочку Acute → Healing → Recovery → выздоровление и переходы между фазами.

## Область анализа

| Файл | Роль |
|------|------|
| `Core/Models/DebuffState.cs` | Модель состояния, флаги фаз |
| `Managers/StateManager.cs` | CRUD DebuffState, StartTreatment, AdvancePhase |
| `Managers/TreatmentManager.cs` | StartPhasedTreatment, AdvanceInjuryToNextPhase, CompleteInjuryRecovery |
| `Managers/InjuryManager.cs` | Создание DebuffState при травме, маппинг фазовых баффов |
| `EventHandlers/GameEventHandler.cs` | `CheckInjuryPhases` на DayStarted |
| `EventHandlers/InteractionHandler.cs` | Клик по Харви: старт, advance, recovery |
| `ModEntry.cs` | Debug-команды `injury_phase_*` |

## Схема штатного потока

```
[Триггер] → buff + DebuffState (phase=0, TreatmentStarted=false)
    ↓ клик по Харви
[Старт лечения] → remove base buff → add Phase1 buff → CurrentPhase=1
    ↓ каждый DayStarted
[CheckInjuryPhases] → ReadyForNextPhase / ReadyForRecovery
    ↓ клик по Харви
[Advance] или [CompleteRecovery] → remove buffs, topics, DebuffState → topic*Cured
```

---

## Критично

### 1. Блокировка готовой травмы более приоритетной «неготовой»

**Где:** `InteractionHandler.OnButtonPressed`, шаг 4.

```csharp
var inTreatment = modDebuffs
    .Where(d => d.TreatmentStarted)
    .OrderByDescending(d => GetInjuryPriority(d.BuffId))
    .FirstOrDefault();

if (inTreatment != null && CheckAndHandlePhaseTransition(harvey, inTreatment.BuffId, inTreatment))
```

Обрабатывается **только одна** травма — с наивысшим приоритетом среди `TreatmentStarted`. Если она ещё не готова к переходу (`ReadyForNextPhase` / `ReadyForRecovery` = false), `CheckAndHandlePhaseTransition` возвращает `false`, и обработка завершается стандартным диалогом игры.

**Последствие:** при двух одновременных фазовых травмах в лечении травма с меньшим приоритетом, уже готовая к смене фазы или выздоровлению, **не обработается**, пока не будет обслужена более приоритетная (или пока у неё не наступит готовность и игрок не кликнет снова после advance/recovery верхней).

**Пример:** `buffConcussion` в фазе 1 (не готова) + `buffDeepCuts` с `ReadyForRecovery` → клик по Харви не завершит DeepCuts.

---

### 2. Неверный ключ диалога `PhaseTransition_*` после смены фазы

**Где:** `InteractionHandler.AdvanceToNextPhase`.

Порядок вызовов:

1. `_treatmentManager.AdvanceInjuryToNextPhase(injuryId)` — инкрементирует `CurrentPhase`.
2. Ключ диалога: `PhaseTransition_{injuryName}_{debuffState.CurrentPhase + 1}`.

По стандарту (`docs/id-naming-standard.md`): `n` в `PhaseTransition_{Injury}_{n}` — **новая** фаза (2 или 3). После advance `CurrentPhase` уже равна новой фазе, поэтому ключ должен быть `PhaseTransition_{injury}_{CurrentPhase}`, а не `+ 1`.

| Переход | Ожидаемый ключ | Фактический ключ | Результат |
|---------|----------------|------------------|-----------|
| 1 → 2 | `PhaseTransition_DeepCuts_2` | `PhaseTransition_DeepCuts_3` | fallback-текст |
| 2 → 3 | `PhaseTransition_DeepCuts_3` | `PhaseTransition_DeepCuts_4` | fallback (ключа нет в CP) |

Для **2-фазных** травм (`buffCold`, `buffSprainedAnkle`, `buffBurnWounds` и др.) при единственном переходе 1→2 запрашивается `_3` вместо `_2` — кастомная реплика из `dialoguesHarveyInjury.json` не показывается.

---

## Средний приоритет

### 3. `AdvanceInjuryToNextPhase` не защищён от выхода за `TotalPhases`

**Где:** `TreatmentManager.AdvanceInjuryToNextPhase`.

- `DebuffState.AdvancePhase` инкрементирует фазу только при `CurrentPhase < TotalPhases`.
- Новый фазовый бафф накладывается **безусловно** по `newPhase = oldPhase + 1`.

При ручном `injury_phase_ready` на последней фазе или прямом вызове `AdvanceInjuryToNextPhase` возможны: лишний/fallback-бафф, рассинхрон `CurrentPhase` и активного баффа.

В штатном потоке `CheckInjuryPhases` на последней фазе выставляет только `ReadyForRecovery`, не `ReadyForNextPhase` — риск в основном через debug.

---

### 4. Фазовые topic 2/3 не добавляются при переходах

| Момент | Поведение |
|--------|-----------|
| Старт лечения | Добавляется только `topic{Injury}PhaseAcute` (фаза 1) — `InteractionHandler.StartTreatment` |
| Advance фазы | Topic не добавляются (логика в `GameEventHandler` закомментирована) |
| Выздоровление | Удаляются topic фаз 1–3 — `RemoveInjuryRelatedTopics` |

**Итог:** во время фаз 2 и 3 активен только `topic*PhaseAcute`. CP-диалоги по `topic*PhaseHealing` / `topic*PhaseRecovery` для пассивных реплик не активируются. Переходные реплики должны идти через `PhaseTransition_*` (см. баг №2).

---

### 5. Два разных пути полного выздоровления

| | Игровой клик (`CompleteRecovery`) | Debug `injury_phase_cure` (`CompleteInjuryRecovery`) |
|--|-------------------------------------|--------------------------------------------------------|
| Баффы | `RemoveAllPhaseBuffs` | только последний фазовый |
| Phase topics | удаляются 1–3 | **не удаляются** |
| Care buff | 2 дня (`2880000` мс) | 4 дня (`28800000` мс) |
| Топик | `topic*Cured` | `topicTreatmentCompleted` |

Игровой путь корректнее; debug-команда может оставить «висящие» phase topics.

---

### 6. Осложнения перехватывают клик раньше смены фазы

**Где:** `InteractionHandler`, шаг 3.

При `injuries.Complications.Count > 0` вызывается `StartTreatment(..., null)` **до** шага 4 (advance/recovery). Нужен отдельный клик для перехода фазы. Не deadlock, но UX-задержка при совмещении осложнения и готовой фазы.

---

### 7. `buffCold` без приоритета в `GetInjuryPriority`

`GetInjuryPriority("buffCold")` возвращает `0` — ниже всех травм. При двух травмах в лечении Cold обрабатывается последним и сильнее страдает от бага №1.

---

## Низкий приоритет

- **`ReadyForRecovery`** не сбрасывается явно при advance — на не-последних фазах не выставляется; при выздоровлении state удаляется целиком.
- **`ReadyForNextPhase`** сбрасывается в `DebuffState.AdvancePhase` — корректно для штатного advance.
- **Debug `injury_phase_advance`:** лог `ds.CurrentPhase → ds.CurrentPhase + 1` после advance показывает неверные числа (ссылка на тот же объект).
- **Рассинхрон buff/state после краша** без сна: `SavedActiveBuffs` обновляется только в `OnDayEnding`; теоретически на следующий день можно восстановить старый фазовый бафф из снапшота при уже обновлённом `DebuffState`.
- **`CompleteInjuryRecovery`** не вызывает `RemoveAllPhaseBuffs` — базовый `buff*` обычно уже снят при старте лечения.

---

## Что проверено — проблем не найдено

| Пункт цепочки | Статус |
|---------------|--------|
| **1. Создание DebuffState** | Каждый `Apply*()` вызывает `CreateDebuffState` с корректными `P1/P2/P3`; `TotalPhases` = 2 или 3; `CurrentPhase=0`, `TreatmentStarted=false`. Повторное создание не перезаписывает активное лечение. |
| **2. Первый клик по Харви** | `StartPhasedTreatment`: remove base buff → add Phase1 → `StartTreatment` (`CurrentPhase=1`). Топики: remove injury topic, add treatment + PhaseAcute. |
| **3. OnDayStarted → CheckInjuryPhases** | Пропуск `TotalPhases==0` (осложнения) и `!IsInTreatment`. `HasPhaseTimeElapsed` → `ReadyForNextPhase` или `ReadyForRecovery`. HUD-напоминания один раз. |
| **4. Следующий клик (advance)** | Remove old phase buff → `AdvancePhase` → add new phase buff. `ReadyForNextPhase` сбрасывается в `AdvancePhase`. |
| **5. Последняя фаза → recovery** | `IsLastPhase && ReadyForRecovery` проверяется **до** `ReadyForNextPhase` — корректный приоритет выздоровления для одной травмы. |
| **6. Cleanup после выздоровления (игровой путь)** | `RemoveAllPhaseBuffs`, `RemoveDebuffState`, injury/treatment/phase topics, `topic*Cured`, residual cooldown через `NotifyInjuryRecovered`. |
| **Advance за TotalPhases (штатно)** | `CheckInjuryPhases` не ставит `ReadyForNextPhase` на последней фазе. |
| **Синхрон state/buff в норме** | Advance атомарно меняет buff + `CurrentPhase`; restore из снапшота при обычном цикл день→сон→день согласован. |
| **Debug-команды `injury_phase_*`** | `list/ready/recovery/advance/cure` — рабочие обёртки; `advance` блокирует `CurrentPhase >= TotalPhases`. |

---

## Тестовые сценарии в игре

### Базовый happy path (3 фазы)

1. `injury_debuff_add buffDeepCuts` → `injury_phase_list` (phase 0/3).
2. Клик Харви → нет `buffDeepCuts`, есть `HarveyMod_DeepCuts_Acute`, `TreatmentStarted`, `topicDeepCutsPhaseAcute`.
3. `injury_phase_ready buffDeepCuts 1` → клик → бафф `Healing`, phase 2/3; в логе SMAPI проверить ключ `PhaseTransition_DeepCuts_*` (ожидаем `_2`, фактически `_3` — баг).
4. Повторить для фазы 3 → recovery HUD → `injury_phase_recovery buffDeepCuts 1` → клик → нет фазовых баффов, `topicDeepCutsCured`, пустой `injury_phase_list`.

### 2-фазная травма

5. `buffCold` или `buffSprainedAnkle`: полный цикл 1→2 → recovery. Проверить единственный переход и ключ `PhaseTransition_*_2`.

### Две травмы (критичный баг №1)

6. `buffConcussion` + `buffDeepCuts`, обе в лечении.
7. `injury_phase_recovery buffDeepCuts 1`, у Concussion флагов нет.
8. Клик Харви → **ожидаемый баг:** стандартный диалог, DeepCuts не cured.
9. `injury_phase_ready buffConcussion 1` → клик (advance Concussion) → снова клик → DeepCuts должен cured.

### Осложнение + готовая фаза

10. `buffDeepCuts` в лечении, `injury_debuff_add HarveyMod_DirtyWound`.
11. `injury_phase_ready buffDeepCuts 1` → первый клик лечит осложнение, второй — advance.

### Граница дней (без debug)

12. Старт лечения в день D, `Phase1Duration=3` → дождаться D+3, проверить HUD «готова к стадии заживления» и флаг в `injury_phase_list`.

### Debug vs gameplay recovery

13. `injury_phase_cure buffDeepCuts` → проверить orphan topics vs игровой `CompleteRecovery`.

### После выздоровления

14. `topicDeepCutsCured` → клик Харви → финальный диалог, снятие cured topic.

### Регрессия buff/state

15. Advance фазы днём → сон → утро: `injury_phase_list` и активный бафф совпадают с `CurrentPhase`.

---

## Резюме

Основная логика фаз **работает для одной фазовой травмы**: создание state, старт лечения, дневные флаги, advance/recovery и cleanup в игровом пути согласованы.

**Главные риски:**

1. Множественные травмы в лечении — готовая травма блокируется более приоритетной неготовой.
2. Off-by-one в ключе `PhaseTransition_*` — кастомные реплики переходов не показываются (fallback).
3. Phase topics 2/3 не создаются — пассивные фазовые диалоги CP не активируются после первой фазы.

## Связанные документы

- `docs/id-naming-standard.md` — формат `PhaseTransition_*` и `topic*Phase*`
- `docs/events-inventory/14-scenario-chains.md` — цепочка DeepCuts
- [`docs/testing/manual-test-scenarios-topics-mail.md`](../testing/manual-test-scenarios-topics-mail.md) — смежные ручные сценарии
- [`docs/testing/FOR_TEST.md`](../testing/FOR_TEST.md) — debug-команды и жизненный цикл травмы
