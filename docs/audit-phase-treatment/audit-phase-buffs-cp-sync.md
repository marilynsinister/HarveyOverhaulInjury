# Аудит соответствия фазовых баффов C# ↔ Content Patcher

**Дата:** 2026-05-24  
**Тип:** read-only аудит (код не менялся)

## Источники

| Слой | Путь |
|------|------|
| C# | `Managers/TreatmentManager.cs`, `Managers/InjuryManager.cs`, `ModEntry.cs` |
| CP manifest | `HarveyOverhaul [CP]/content.json` (Include → `buffsCure.json`, `buffsInjury.json`, `buffsCureStress.json`, `buffsStress.json`) |
| Data/Buffs | `assets/Code/buffsCure.json`, `assets/Code/buffsInjury.json` |
| Спецификация | `docs/MOD_SPEC.md` §3.2 |

CP-пак установлен локально:  
`D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\`  
(в репозитории SMAPI-мода JSON-файлов CP нет — проверка выполнена по установленному CP).

---

## Сводка

| Проверка | Результат |
|----------|-----------|
| Все ID из `GetPhaseBuffId` есть в Data/Buffs (CP) | **11/11 травм — OK** |
| Лишних phase-бaffов в CP, не используемых C# | **нет** |
| Ложная 3-я фаза в игровой логике (2-фазные травмы) | **нет** (`TotalPhases=2` блокирует advance) |
| Коллизия ID `buff*` vs `HarveyMod_*` | **нет** (разные пространства имён) |
| `RemoveAllPhaseBuffs` покрывает все реальные фазовые ID | **OK** (11/11) |
| `injury_phase_advance` использует те же ID | **OK** (`AdvanceInjuryToNextPhase` → `GetPhaseBuffId`) |
| `injury_phase_cure` удаляет все фазовые баффы | **⚠️ нет** (только текущую фазу — см. ниже) |

---

## Таблица соответствия

Колонка **«есть ли ID в Data/Buffs»** — для каждой фазы и базового `buff*`: все перечисленные ID найдены в CP (`buffsCure.json` или `buffsInjury.json`), подключённых через `content.json`.

| injuryId | TotalPhases | phase 1 buff | phase 2 buff | phase 3 buff | есть ли ID в Data/Buffs | найденная проблема | рекомендация |
|----------|-------------|--------------|--------------|--------------|-------------------------|-------------------|--------------|
| `buffConcussion` | 3 | `HarveyMod_Concussion_Acute` | `HarveyMod_Concussion_Rest` | `HarveyMod_Concussion_Limited` | base ✓; ф1 ✓ (`buffsCure`); ф2 ✓; ф3 ✓ | CP-триггеры (`triggersCare.json`) проверяют `buffConcussion`, не фазовые ID — после старта лечения interception/mine-guard не видит травму | Документировать или добавить в CP-условия фазовые ID (или topic/state bridge) |
| `buffFracturedBone` | 3 | `HarveyMod_FracturedBone_Acute` | `HarveyMod_FracturedBone_Cast` | `HarveyMod_FracturedBone_Recovery` | все ✓ (`buffsCure` + base `buffsInjury`) | То же — CP `PLAYER_HAS_BUFF buffFracturedBone` не срабатывает на фазах | См. выше |
| `buffTornMuscles` | 3 | `HarveyMod_TornMuscles_Acute` | `HarveyMod_TornMuscles_Healing` | `HarveyMod_TornMuscles_Rehab` | все ✓ | CP-триггеры по base ID | См. выше |
| `buffShrapnelWounds` | 3 | `HarveyMod_Shrapnel_Surgery` | `HarveyMod_Shrapnel_Healing` | `HarveyMod_Shrapnel_Recovery` | все ✓ | CP-триггеры по base ID | См. выше |
| `buffDeepCuts` | 3 | `HarveyMod_DeepCuts_Acute` | `HarveyMod_DeepCuts_Healing` | `HarveyMod_DeepCuts_Recovery` | все ✓ | CP-триггеры по base ID; известный gap mine interception (см. `docs/events-inventory/07-reachability-details.md`) | Расширить CP-условия или использовать conversation topic |
| `buffSprainedAnkle` | 2 | `HarveyMod_SprainedAnkle_Acute` | `HarveyMod_SprainedAnkle_Recovery` | *(alias ф2)* | base ✓; ф1 ✓; ф2 ✓; ф3 n/a | `GetPhaseBuffId(3)` дублирует ф2 — **намеренно**; `TotalPhases=2` не даёт перейти на ф3 | Оставить alias для `RemoveAllPhaseBuffs`; при рефакторинге можно не запрашивать phase 3 при `TotalPhases==2` |
| `buffBruisedRibs` | 2 | `HarveyMod_BruisedRibs_Acute` | `HarveyMod_BruisedRibs_Healing` | *(alias ф2)* | все ✓ | Alias ф3 = ф2; логика 2 фаз корректна | — |
| `buffBurnWounds` | 2 | `HarveyMod_BurnWounds_Acute` | `HarveyMod_BurnWounds_Healing` | *(alias ф2)* | все ✓ | Alias ф3; CP-триггеры по base ID | См. concussion |
| `buffInfectedWound` | 2 | `HarveyMod_InfectedWound_Acute` | `HarveyMod_InfectedWound_Treatment` | *(alias ф2)* | все ✓ | Alias ф3; naming «Treatment» vs topic `PhaseHealing` — только косметика ID | — |
| `buffBackStrain` | 2 | `HarveyMod_BackStrain_Acute` | `HarveyMod_BackStrain_Recovery` | *(alias ф2)* | все ✓ | Alias ф3 | — |
| `buffCold` | 2 | `HarveyMod_Cold_Acute` | `HarveyMod_Cold_Recovery` | *(alias ф2)* | base ✓; **ф1/ф2 в `buffsInjury.json`** (остальные фазовые — в `buffsCure.json`) | Разнесение по файлам не ломает загрузку; `buffCold` отсутствует в CP-триггерах Severe (ожидаемо) | При добавлении новых cold-бaffов держать их в том же Include-файле или документировать split |

---

## Детали по пунктам задачи

### 1. Фазовые травмы (`TreatmentManager.PhasedInjuries`)

11 ID: `buffConcussion`, `buffFracturedBone`, `buffTornMuscles`, `buffSprainedAnkle`, `buffBruisedRibs`, `buffDeepCuts`, `buffBurnWounds`, `buffInfectedWound`, `buffBackStrain`, `buffShrapnelWounds`, `buffCold`.

Совпадает с `docs/MOD_SPEC.md` §3.2.

### 2–3. Маппинг C# и наличие в CP

Все **28 уникальных** фазовых ID из `InjuryManager.GetPhaseBuffId` найдены в Data/Buffs:

- **25 записей** — `buffsCure.json`
- **2 записи** (`HarveyMod_Cold_Acute`, `HarveyMod_Cold_Recovery`) — `buffsInjury.json`
- **11 базовых** `buff*` — `buffsInjury.json`

Orphan phase-бaffов в CP без C#-маппинга: **0**.  
C# ID без CP-определения: **0**.

### 4. Двухфазные травмы и «ложная» 3-я фаза

`TotalPhases` вычисляется в `StateManager.CreateDebuffState`:

```csharp
TotalPhases = phase3Duration > 0 ? 3 : (phase2Duration > 0 ? 2 : 0)
```

Для 6 двухфазных травм `Phase3Duration = 0` → `TotalPhases = 2`.

- `CheckInjuryPhases` на фазе 2 выставляет `ReadyForRecovery`, не `ReadyForNextPhase`.
- `AdvancePhase` не инкрементирует за `TotalPhases`.
- `GetPhaseBuffId(injury, 3)` для 2-фазных — **alias** той же строки, что фаза 2 (комментарии «2 фазы» в коде).

**Вывод:** ложного перехода на 3-ю фазу в штатной логике нет. Alias phase 3 используется только как защитный дубликат в `RemoveAllPhaseBuffs`.

### 5. Конфликт базовых `buff*` и фазовых `HarveyMod_*`

| Аспект | Статус |
|--------|--------|
| Коллизия ID | Нет — разные ключи в Data/Buffs |
| Одновременное наложение | `StartPhasedTreatment` снимает base перед phase 1 |
| CP preconditions | Многие проверяют **только** `buff*` (до лечения корректно; **после старта лечения** — gap) |
| `persistentBuffs.json` | Содержит base `buff*`, **не** содержит `HarveyMod_*_Acute/...`; файл **не подключён** в `content.json` — фактически неактивен; восстановление фазовых баффов делает C# через `SavedActiveBuffs` |

### 6. `RemoveAllPhaseBuffs`

Для каждой травмы удаляет: `GetPhaseBuffId(1..3)` + `injuryId`.

Проверка покрытия: множество **реально используемых** баффов (фазы 1..`TotalPhases` + base) ⊆ множество удаляемых — **11/11 OK**.

Для 2-фазных phase 3 ID дублирует phase 2 — безопасный no-op при повторном remove.

Игровой путь выздоровления (`InteractionHandler.CompleteRecovery`) вызывает `RemoveAllPhaseBuffs` — **корректно**.

### 7. Debug-команды и те же ID

| Команда | Путь | ID | Замечание |
|---------|------|-----|-----------|
| `injury_phase_advance <buffId>` | `TreatmentManager.AdvanceInjuryToNextPhase` | `GetPhaseBuffId(oldPhase)` / `GetPhaseBuffId(newPhase)` | ✓ те же ID; guard `CurrentPhase >= TotalPhases` |
| `injury_phase_cure <buffId>` | `TreatmentManager.CompleteInjuryRecovery` | `GetPhaseBuffId(CurrentPhase)` только | ⚠️ удаляет **один** фазовый бафф, **не** base, **не** остальные фазы; не вызывает `RemoveAllPhaseBuffs` |
| `injury_phase_list/ready/recovery` | `StateManager` | работают с `buffId` (base), не с `HarveyMod_*` | ✓ ожидаемо для оператора |

**Рекомендация для debug:** `injury_phase_cure` должен зеркалить `CompleteRecovery` (`RemoveAllPhaseBuffs`) или явно документировать расхождение в [`docs/testing/FOR_TEST.md`](../testing/FOR_TEST.md).

---

## Распределение по CP-файлам (Data/Buffs)

| Файл | Include в content.json | Содержимое для фаз |
|------|------------------------|-------------------|
| `buffsCure.json` | ✓ | Все фазовые баффы, кроме Cold |
| `buffsInjury.json` | ✓ | Base `buff*` + `HarveyMod_Cold_Acute/Recovery` |
| `buffsCureStress.json` | ✓ | Phase-бaffов нет |
| `buffsStress.json` | ✓ | Phase-бaffов нет |

---

## Согласованность с MOD_SPEC

Длительности фаз и `TotalPhases` в C# (`InjuryManager.Apply*`) совпадают с таблицей §3.2 MOD_SPEC для всех 11 травм.

Примеры маппинга из MOD_SPEC подтверждены в CP:

- `buffDeepCuts` → Acute / Healing / Recovery ✓
- `buffFracturedBone` → Acute / Cast / Recovery ✓
- `buffCold` → Acute / Recovery ✓

---

## Тестовые сценарии (ручная проверка в игре)

1. **`injury_debuff_add buffDeepCuts`** → клик Харви → SMAPI log: add `HarveyMod_DeepCuts_Acute`, remove `buffDeepCuts`.
2. **`injury_phase_advance buffDeepCuts`** ×2 → в HUD/баффах по очереди Healing, Recovery; каждый ID существует (нет silent fail `BuffExists`).
3. **`injury_phase_cure buffDeepCuts`** на фазе 2 после advance с ф1 — проверить, не остался ли `HarveyMod_DeepCuts_Acute` (известный gap debug-пути).
4. **`buffCold`**: после лечения только `HarveyMod_Cold_Acute` → `HarveyMod_Cold_Recovery` (оба из `buffsInjury.json`).
5. **2-фазная:** `buffSprainedAnkle` — `injury_phase_list` показывает `2/2` на последней фазе; `injury_phase_advance` отклоняется.
6. **CP trigger:** с `buffDeepCuts` (до лечения) зайти в Mine с dating — trigger срабатывает; после старта лечения (только `HarveyMod_DeepCuts_Acute`) — проверить, что CP interception **не** видит травму (documented gap).

---

## Связанные документы

- `docs/audit-phase-treatment/audit-phase-transitions.md` — аудит переходов фаз
- `docs/events-inventory/11-id-sync-audit.md` — полный ID-sync (включая phase buffs)
- `docs/MOD_SPEC.md` §3.2, §5.3
- [`docs/testing/FOR_TEST.md`](../testing/FOR_TEST.md) — debug-команды

## Инструмент проверки

Скрипт перекрёстной сверки: `tmpMap/audit_phase_buffs_cp.py` (можно перезапустить после изменений CP/C#).
