# Стандарт именования ID: topics & mail (HarveyOverhaul Injury)

Дата: 2026-05-23  
Основа: [fix-plan-topics-mail.md](./fix-plan-topics-mail.md), аудиты C#/CP 2026-05-23.

**Статус:** норматив для новых ID и для миграции рассинхронов. Код и JSON **не изменялись**.

**Принцип:** C# — **источник правды** для динамически генерируемых ID (Replace, GetPhaseTopicId, cured). CP **обязан** иметь exact key в `Characters/Dialogue/Harvey` или `Data/Mail`. Legacy-ID не переименовывать, если уже работают в save/triggers — только duplicate alias или правка «мёртвой» стороны.

---

## Обзор пространств имён

| Пространство | Префикс | Кто создаёт | Пример |
|--------------|---------|-------------|--------|
| Buff травмы | `buff` | C# `InjuryManager` | `buffDeepCuts` |
| Buff осложнения | `HarveyMod_` | C# / CP | `HarveyMod_WetBandage` |
| Topic травмы | `topic` + `{InjuryName}` | C# | `topicDeepCuts` |
| Topic осложнения | `topicHarvey_` | C# | `topicHarvey_WetBandage` |
| Topic фаза | `topic` + `{InjuryName}Phase` + `{Stage}` | C# `GetPhaseTopicId` | `topicDeepCutsPhaseHealing` |
| Topic cured | `topic` + `{InjuryName}Cured` | C# | `topicDeepCutsCured` |
| Topic лечение (курс) | `topicTreatment` + `{InjuryName}` | C# | `topicTreatmentDeepCuts` |
| Dialogue лечение | `Treat_` | CP (+ pick C#) | `Treat_DeepCuts_Before1` |
| Dialogue смена фазы | `PhaseTransition_` | CP (+ pick C#) | `PhaseTransition_DeepCuts_2` |
| Dialogue proximity | `Proximity_` | CP (+ pick C#) | `Proximity_WetBandage` |
| Mail системный (новый) | `HarveyMod_` | C# / CP | `HarveyMod_DirtyWoundInfection` |
| Mail legacy (заморожен) | `mailHarvey` + optional `_` | C# / CP triggers | `mailHarveySleepControl` |

`{InjuryName}` = идентификатор buff **без** префикса `buff` (PascalCase, как в коде: `DeepCuts`, `FracturedBone`, `Cold`).

---

## 1. Базовые травмы

### Стандарт

```
buff{InjuryName}  →  topic{InjuryName}
```

**Формула C#:** `injuryId.Replace("buff", "topic")`  
**Пример:** `buffDeepCuts` → `topicDeepCuts`

### Почему так

- Уже реализовано в `TreatmentManager`, `InteractionHandler`, `InjuryManager`.
- Один механический Replace — меньше опечаток, проще diff-скрипт.
- Префикс `topic` — convention SDV для `activeDialogueEvents`.
- Имя после `topic` совпадает с `{InjuryName}` из buff (не `topicBuffDeepCuts`).

### Полный список canonical пар (C# → topic)

| Buff | Topic |
|------|-------|
| `buffHurt` | `topicHurt` |
| `buffBadlyHurt` | `topicBadlyHurt` |
| `buffSprainedAnkle` | `topicSprainedAnkle` |
| `buffBruisedRibs` | `topicBruisedRibs` |
| `buffBackStrain` | `topicBackStrain` |
| `buffDeepCuts` | `topicDeepCuts` |
| `buffBurnWounds` | `topicBurnWounds` |
| `buffInfectedWound` | `topicInfectedWound` |
| `buffTornMuscles` | `topicTornMuscles` |
| `buffConcussion` | `topicConcussion` |
| `buffFracturedBone` | `topicFracturedBone` |
| `buffShrapnelWounds` | `topicShrapnelWounds` |
| `buffSurgicalWound` | `topicSurgicalWound` |
| `buffCold` | `topicCold` |

### Сопутствующие topic (не buff→topic Replace)

| Topic | Когда ставится |
|-------|----------------|
| `topicHealthDamageCritical` | badly hurt, fracture, shrapnel |
| `topicHealthDamageSevere` | torn muscles, concussion |
| `topicPostOperativeCare` | shrapnel, surgical wound |

### Что привести к стандарту

| Текущий ID | Проблема | Исправить | Где |
|------------|----------|-----------|-----|
| *(отсутствует)* `topicSurgicalWound` | C# add, CP нет | Добавить ключ | **CP** |
| *(отсутствует)* `topicHealthDamageSevere` | C# строка, CP нет | Добавить ключ | **CP** |
| `topicHealthDamage` | CP legacy, C# не ставит | Удалить orphan | **CP** |
| `topicSurgicalWoundHealed` | Не базовый topic; путают с cured | См. §3 | **CP** |
| `topicStressTooCold` | Stress-модуль; C# ставит `topicTooCold` | Не merge без Include stress; добавить `topicTooCold` | **CP** |
| `topicHarvey_MineDeathRescue` | Legacy constant, не используется | Удалить константу | **C#** |
| `topicHealthDamageSevere` | Прямая строка в C# | Вынести в `ConversationTopics` | **C#** |

---

## 2. Фазы лечения

### Стандарт

```
buff{InjuryName}, phase 1  →  topic{InjuryName}PhaseAcute
buff{InjuryName}, phase 2  →  topic{InjuryName}PhaseHealing
buff{InjuryName}, phase 3  →  topic{InjuryName}PhaseRecovery
```

**Формула C#:** `GetPhaseTopicId(injuryId, phase)` → `$"topic{injuryName}Phase{stageName}"`  
где `stageName` ∈ `{Acute, Healing, Recovery}` (фазы 1/2/3).

**Пример:** `buffDeepCuts` phase 2 → `topicDeepCutsPhaseHealing`

### Почему так

- Три **фиксированных** английских суффикса — не медицинские синонимы (`Cast`, `Surgery`, `Rehab`).
- C# уже генерирует только эти имена; CP block1 с alternate names — источник silence.
- `PhaseTransition_{Injury}_{2|3}` — **отдельный** dialogue-префикс при смене фазы (не topic).
- Legacy `topic*Phase1Ready`, `*Phase2Ready`, `*RecoveryReady` — **не** стандарт; удалить после миграции.

### Phased injuries (C# `PhasedInjuries`)

Concussion, FracturedBone, TornMuscles, SprainedAnkle, BruisedRibs, DeepCuts, BurnWounds, InfectedWound, BackStrain, ShrapnelWounds, Cold — для каждой три canonical phase topic.

### Что привести к стандарту

| Текущий CP ID (legacy) | Canonical ID | Где исправлять |
|------------------------|--------------|----------------|
| `topicFracturedBonePhaseCast` | `topicFracturedBonePhaseHealing` | **CP** — duplicate key (block1) |
| `topicConcussionPhaseObservation` | `topicConcussionPhaseHealing` | **CP** — duplicate |
| `topicInfectedWoundPhaseTreatment` | `topicInfectedWoundPhaseHealing` | **CP** — duplicate |
| `topicShrapnelWoundsPhaseSurgery` | `topicShrapnelWoundsPhaseAcute` | **CP** — duplicate |
| `topicTornMusclesPhaseRehab` | `topicTornMusclesPhaseRecovery` | **CP** — duplicate |
| `topicColdPhase1Ready` | `topicColdPhaseAcute` / `Recovery` | **CP** — заменить содержимым canonical keys |
| `topicColdRecoveryReady` | `topicColdPhaseRecovery` или cured-flow | **CP** — удалить после canonical |
| 32× `*Phase*Ready` | `*PhaseAcute/Healing/Recovery` | **CP** — удалить legacy |
| *(отсутствуют)* `topicColdPhaseAcute/Healing/Recovery` | — | **CP** — добавить |
| `topicHurtPhase*`, `topicBadlyHurtPhase*` | Hurt не phased в C# | **CP** — удалить dead keys |
| `topicSurgicalWoundPhase*` | Surgical не phased в C# | **CP** — удалить или product decision |

**C# менять не нужно** — `GetPhaseTopicId` уже соответствует стандарту. Исключение: если позже включат AddTopic на фазы 2–3 (сейчас только PhaseAcute при старте лечения) — имена уже правильные.

### Связанные dialogue-префиксы (не topics)

| Префикс | Пример | Назначение |
|---------|--------|------------|
| `PhaseTransition_{Injury}_{n}` | `PhaseTransition_DeepCuts_2` | Реплика при advance; `n` = **новая** фаза (2 или 3) |
| `Treat_{Injury}_Before{n}` | `Treat_DeepCuts_Before1` | Осмотр до процедуры |
| `Treat_{Injury}_After{n}` | `Treat_DeepCuts_After1` | После процедуры |

Для `buffCold`: canonical Treat — `Treat_Cold_Before*` / `After*`; CP-aliases на `Treat_Cold_Acute` / `Recovery` (**CP** duplicate, без смены C#).

---

## 3. Выздоровление (cured)

### Стандарт (выбранный вариант)

```
buff{InjuryName}  →  topic{InjuryName}Cured
```

**Формула C#:** `$"topic{injuryId.Replace("buff", "")}Cured"`  
**Примеры:** `buffDeepCuts` → `topicDeepCutsCured`; `buffCold` → `topicColdCured`; `buffSurgicalWound` → `topicSurgicalWoundCured`

### Почему `Cured`, а не `Healed` / `Recovered` / `RecoveryReady`

| Вариант | Вердикт |
|---------|---------|
| **`topic*Cured`** | ✓ C# уже использует в `CompleteRecovery`, `CheckSimpleTreatmentCompletion`, `PickRandomDialogueByPrefix(topicId)` |
| `topic*Healed` | ✗ CP-only `topicSurgicalWoundHealed` — единственный outlier, ломает динамический шаблон |
| `topic*Recovered` | ✗ Нигде не генерируется C# |
| `topic*RecoveryReady` | ✗ Legacy CP cure; C# не add |

Суффикс **`Cured`** = conversation topic после финального осмотра (7 д), перехватывается `CheckAndHandleCompletionTopic`.

### Что привести к стандарту

| Текущий ID | Проблема | Исправить | Где |
|------------|----------|-----------|-----|
| `topicSurgicalWoundHealed` | CP ≠ C# `topicSurgicalWoundCured` | Duplicate или rename → `topicSurgicalWoundCured` | **CP** (rename рискует CP events — предпочтительно **duplicate**) |
| *(отсутствует)* `topicColdCured` | C# add, CP нет | Добавить ключ | **CP** |
| `topicColdRecoveryReady` | Legacy, не C# | Удалить после `topicColdCured` | **CP** |
| `topicColdCured` | Нет в `CheckAndHandleCompletionTopic` | Добавить в completion list | **C#** |
| `topicSurgicalWoundCured` | Нет в completion list | Добавить | **C#** |
| `topicStressRecoveryComplete`, `topicTraumaHealingComplete` | Check-only, never add | Удалить check | **C#** |

### Topic лечения на весь курс (отдельно от cured)

```
buff{InjuryName}  →  topicTreatment{InjuryName}
```

**Формула C#:** `injuryId.Replace("buff", "topicTreatment")`  
**Пример:** `buffDeepCuts` → `topicTreatmentDeepCuts`

**Статус:** C# add в `StartPhasedTreatment`; CP keys отсутствуют.  
**Решение:** добавить 11 ключей в CP **или** убрать AddTopic в C# (product). ID при любом выборе — **`topicTreatment{InjuryName}`**, не менять.

Legacy `topicTreatmentCompleted` — отдельный debug/legacy path; не путать с `topic*Cured`.

---

## 4. Осложнения

### Стандарт

```
HarveyMod_{ComplicationName}  →  topicHarvey_{ComplicationName}
```

**Формула C# (remove при лечении):** `compId.Replace("HarveyMod_", "topicHarvey_")`  
**Примеры:**

| Buff | Topic |
|------|-------|
| `HarveyMod_WetBandage` | `topicHarvey_WetBandage` |
| `HarveyMod_DirtyWound` | `topicHarvey_DirtyWound` |
| `HarveyMod_WetStitches` | `topicHarvey_WetStitches` |
| `HarveyMod_Neglect` | `topicHarvey_Neglect` |
| `HarveyMod_AllergicRash` | `topicHarvey_AllergicRash` |
| `HarveyMod_PainFlare` | `topicHarvey_PainFlare` |

### Почему так

- Buff осложнений уже в пространстве `HarveyMod_*` (`InjuryBuffs`).
- Topic осложнений — `topicHarvey_*` (подчёркивание после Harvey) — отличает от `topicDeepCuts` и от `topicHarveyNeedsFirstTreatment` (camelCase «Harvey» без `_` для event-триггеров).
- `ConversationTopics` в C# уже хранит canonical topic IDs; buff и topic **разные** пространства — это норма SDV.

### Proximity-диалоги (не topic)

```
HarveyMod_{Name}  →  Proximity_{Name}   (без topicHarvey_ / HarveyMod_)
```

**Пример:** `Proximity_WetBandage`, `Proximity_DirtyWound1`…`3`  
Используются в `TreatmentManager.BuildCombinedDialogue` при лечении у Харви. Topic и Proximity **дополняют** друг друга; для симметрии с другими осложнениями topic key `topicHarvey_PainFlare` желателен (**CP** duplicate из `Proximity_PainFlare`).

### Что привести к стандарту

| Текущий ID | Проблема | Исправить | Где |
|------------|----------|-----------|-----|
| *(отсутствует)* `topicHarvey_PainFlare` | Только `Proximity_PainFlare`; C# не add topic в gameplay | Duplicate topic key | **CP**; опционально AddTopic в **C#** |
| `topicHarvey_AllergicRash` | C# add только debug | Wire gameplay или оставить CP-only | **C#** (если нужен gameplay) |
| Memory `topicHarvey_*_memory_*` | Orphan SDV memory keys | Wire или удалить | **CP** / **C#** (низкий приоритет) |

---

## 5. Topics: шахта, обморок, окружение, события

### Стандарт (не buff→topic Replace)

Группа **ситуационных** topic — **явные строки** в `ConversationTopics` или литералы; шаблон:

| Категория | Паттерн | Пример |
|-----------|---------|--------|
| Обморок / усталость | `topic` + `{Situation}` | `topicFarmerExhausted`, `topicPassedOutInTown` |
| Шахта / rescue | `topic` + `{MineEvent}` | `topicMineInjuryRescue`, `topicMineRescuePending` |
| First treatment (CP event) | `topic` + `{EventHook}` | `topicHarveyNeedsFirstTreatment`, `topicFirstTreatmentComplete` |
| Окружение | `topic` + `{Condition}` | `topicTooCold` |
| Situation (не topic-префикс логики) | `situation` + `{Reaction}` | `situationReaction_Drunk` |
| Ночной обход | `topicHarvey_` + `{Event}` | `topicHarvey_NightRound` |

### Почему отдельно от травм

- Нет buff `buffPassedOutInTown` → Replace не применим.
- `topicMineInjuryRescue` vs legacy `topicHarvey_MineDeathRescue` — canonical **первый** (используется C#).
- `topicHarveyNeedsFirstTreatment` — camelCase `Harvey` **без** `_` — триггер CP-события, не осложнение.
- `topicTooCold` ≠ `topicCold` (переохлаждение buff `buffTooCold` vs болезнь `buffCold`).

### Что привести к стандарту

| Текущий ID | Проблема | Исправить | Где |
|------------|----------|-----------|-----|
| `topicHarvey_MineDeathRescue` | Legacy constant | Удалить | **C#** |
| `topicMineInjuryRescue` | Event add, нет dialogue | Добавить dialogue key | **CP** |
| `topicTooCold` | C# add, CP нет | Добавить ключ | **CP** |
| `topicStressTooCold` | Stress file не подключён | Не использовать как alias для injury | **CP** (не трогать stress без решения по модулю) |
| `topicForestRescue` | When без AddTopic | AddTopic в event или удалить When | **CP** |
| `topicMineInjuryRescue` (string duplicate) | `PlayerEventHandler` строка | Использовать `ConversationTopics.MineInjuryRescue` | **C#** |

---

## 6. Mail ID

### Стандарт для **новых** писем (C# Injury + системные оповещения)

```
HarveyMod_{Purpose}
```

**PascalCase** после первого `_`; описательное имя; без пробелов.

**Примеры (canonical, C# уже шлёт или будет слать):**

| Mail ID | Назначение |
|---------|------------|
| `HarveyMod_DirtyWoundInfection` | Dirty wound → инфекция |
| `HarveyMod_WetBandageInfection` | Wet bandage → инфекция |
| `HarveyMod_TreatmentUrgentReminder` | Просрочка фазы +3 д |
| `HarveyMod_TreatmentFinalWarning` | Последний день grace |
| `HarveyMod_NeglectWarning` | Neglect после grace |
| `HarveyMod_WetCare` | Инструкция при wet bandage |
| `HarveyMod_WetStitchesCare` | Инструкция при wet stitches |
| `HarveyMod_InfectionAlert` | Generic infection (шаблон; не дублировать с Dirty/Wet specific) |

### Legacy **замороженные** ID (не переименовывать)

Уже в production/save/triggers — **оставить exact key**:

| Mail ID | Статус | Действие |
|---------|--------|----------|
| `mailHarveySleepControl` | C# send ✓, CP ✓ | **Freeze** — только добавить `MailIds.SleepControl` в C# |
| `mailHarveyMineForbidden` | C# send ✓, CP ✓ | **Freeze** — эталон |
| `mailHarveyNote1`…`Note4`, `mailHarveyNoteGirlfriend`, `mailHarveyNoteWife` | CP triggers ✓ | **Freeze** |
| `mailHarveyIntensiveCare`, `mailHarveyModerateCare` | CP triggers ✓ | **Freeze** |
| `mailHarveyStep1` / `Step1Dating` … | CP care chain (triggers off) | **Freeze** до включения triggers |
| `mailHarveyMineWarning`, `mailHarveyCaveWarning` | CP triggers only | **Freeze** — не путать с `mailHarveyMineForbidden` |

### Почему два префикса, но один стиль для новых

| Префикс | История | Политика |
|---------|---------|----------|
| `mailHarvey*` / `mailHarvey_*` | Ранний CP care/injury, triggers, романтические цепочки | **Не ломать** — saves, `PLAYER_HAS_MAIL`, events |
| `HarveyMod_*` | Основной bulk CP (`mail.json`, `mailInjury.json`, `mailCure.json`) | **Новые** C#-письма и исправление рассинхронов |

Единый префикс `HarveyMod_*` для всего потребовал бы массового rename ~50+ ключей и ломки triggers — не оправдано.

**Правило для авторов:** новый mail от **HarveyOverhaul.InjuryCare C#** → только `HarveyMod_*`. Новый mail только от **CP triggers/events** без C# → допустим `mailHarvey*` если цепочка care/romance уже на этом префиксе.

### Что привести к стандарту

| Текущий ID | Canonical | Исправить | Где |
|------------|-----------|-----------|-----|
| C# `mailHarvey_Neglect` | `HarveyMod_NeglectWarning` | Изменить `MailIds.Neglect` | **C#** (CP уже имеет текст) |
| C# `mailHarvey_WetCare` | `HarveyMod_WetCare` | Исправить `MailIds.WetCare` | **C#** |
| C# `mailHarvey_WetStitchesCare` | `HarveyMod_WetStitchesCare` | Исправить `MailIds.WetStitchesCare` | **C#** |
| C# `mailHarvey_Infection` | `HarveyMod_InfectionAlert` | Исправить или удалить мёртвую константу | **C#** |
| *(отсутствуют в CP)* 4× `HarveyMod_*Infection/Reminder/Warning` | — | Добавить entries | **CP** |
| `mailHarveySleepControl` | *(freeze)* | Вынести в `MailIds`; не rename | **C#** only constant |
| `mailHarveyMineForbidden` | *(freeze)* | OK | — |
| `HarveyMod_NeglectWarning` vs duplicate `mailHarvey_Neglect` | Один ID | Предпочтительно **C# → NeglectWarning**, не duplicate | **C#** |
| Phase mail `HarveyMod_*_Phase2/3` | Legacy CP cure | Подключить triggers **или** archive; не rename | **CP** / docs |
| Narrative `HarveyMod_*` в `mail.json` (~38) | Задел | Archive; новые не добавлять без trigger | **docs** |

### MailIds в C#

Все `addMailForTomorrow(...)` — **только** через `MailIds.*`; значения = **exact CP key** по таблице выше.

---

## 7. Сводная таблица формул (C#)

| Сущность | Формула | Пример |
|----------|---------|--------|
| Buff травмы | `buff{InjuryName}` | `buffDeepCuts` |
| Topic травмы | `injuryId.Replace("buff","topic")` | `topicDeepCuts` |
| Topic фаза | `$"topic{injuryName}Phase{Acute\|Healing\|Recovery}"` | `topicDeepCutsPhaseHealing` |
| Topic cured | `$"topic{injuryName}Cured"` | `topicDeepCutsCured` |
| Topic курс лечения | `injuryId.Replace("buff","topicTreatment")` | `topicTreatmentDeepCuts` |
| Topic осложнение | `compId.Replace("HarveyMod_","topicHarvey_")` | `topicHarvey_WetBandage` |
| PhaseTransition dialogue | `$"PhaseTransition_{injuryName}_{phase}"` | `PhaseTransition_DeepCuts_2` |
| Treat dialogue | `$"Treat_{injuryName}_Before\|After{n}"` | `Treat_DeepCuts_After1` |

---

## 8. Миграция: приоритет и «где править»

| Приоритет | Действие | Сторона |
|-----------|----------|---------|
| P0 | CP entries для 4 blank mail + 11 `topicTreatment*` (или C# remove AddTopic) | CP / C# |
| P0 | CP keys: `topicSurgicalWound`, `topicColdPhase*`, `topicColdCured`, `topicHealthDamageSevere`, `topicTooCold` | CP |
| P0 | C# `MailIds.Neglect` → `HarveyMod_NeglectWarning` | C# |
| P0 | CP duplicate phase aliases (Cast→Healing и т.д.) | CP |
| P0 | CP `topicSurgicalWoundCured` duplicate; C# completion list | CP + C# |
| P1 | C# `MailIds` WetCare, WetStitchesCare, Infection; literals → constants | C# |
| P1 | CP удалить `*Phase*Ready`, `topicSurgicalWoundHealed` после canonical | CP |
| P2 | Freeze documentation для `mailHarvey*` care chain | docs |
| P2 | Stress `topicStress*` — отдельный модуль, не смешивать с injury | docs |

**Общее правило:** если C# **динамически** генерирует ID — править **CP** (добавить/alias key). Если C# **константа** не совпадает с CP — править **C#** (дешевле, чем rename CP triggers). Если оба frozen в saves — **duplicate** в CP, deprecate старое.

---

## 9. Анти-пatterns (запрещено для новых ID)

| Нельзя | Почему | Вместо |
|--------|--------|--------|
| `topic*Healed`, `topic*Recovered` | C# cured = `*Cured` | `topic*Cured` |
| `topic*PhaseCast/Surgery/Rehab/Observation/Treatment` | C# = Acute/Healing/Recovery | Canonical phase + CP alias при необходимости |
| `topic*Phase1Ready`, `*RecoveryReady` | Legacy CP, C# не add | Canonical phase topics |
| `topicBuffDeepCuts` | Replace даёт `topicDeepCuts` | `topicDeepCuts` |
| `mailHarvey_*` для новых C# system mail | Рассинхрон с bulk CP | `HarveyMod_*` |
| Rename frozen `mailHarveySleepControl` | Saves, triggers | Freeze + optional split variant |
| `topicStressTooCold` для `buffTooCold` | Другой модуль/ID | `topicTooCold` |
| Mix `HarveyMod_Neglect` buff и `mailHarvey_Neglect` mail | Разные сущности OK; mail ID must match CP | Mail: `HarveyMod_NeglectWarning` |

---

## 10. Проверка соответствия

```text
# Topics (SDV console)
debug ebi Harvey topicDeepCuts
debug ebi Harvey topicDeepCutsPhaseHealing
debug ebi Harvey topicDeepCutsCured
debug ebi Harvey topicHarvey_WetBandage

# Mail
debug mail HarveyMod_DirtyWoundInfection
debug mail HarveyMod_NeglectWarning
debug mail mailHarveySleepControl
debug mail mailHarveyMineForbidden
```

**Automated diff (рекомендуется):** grep CP JSON для каждого ID из `KnownTraumas`, `PhasedInjuries`, `ConversationTopics`, `MailIds`, формул §7.

---

## Связанные документы

| Документ | Содержание |
|----------|------------|
| [fix-plan-topics-mail.md](./fix-plan-topics-mail.md) | Задачи миграции по группам A–D |
| [audit-dynamic-id-risks.md](./audit-dynamic-id-risks.md) | Риски Replace / GetPhaseTopicId |
| [audit-topics-csharp.md](./audit-topics-csharp.md) | Полный список C# topic operations |
| [audit-mail-csharp.md](./audit-mail-csharp.md) | Полный список C# mail send |

---

*Документ составлен по fix-plan-topics-mail.md. Код и JSON не изменялись.*
