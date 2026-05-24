# Финальная валидация: topics & mail (после исправлений)

Дата: 2026-05-23  
Область: C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`  
Метод: read-only — `dotnet build`, скрипт `tmpMap/final_validation_topics_mail.py`, grep C#/CP, сверка с `audit-*` и `audit-dead-content.md`. **Код и JSON не менялись.**

---

## Проверено

### 1. C# topic ID → CP dialogue

**Охват:** все `ConversationTopics`, `KnownTraumas`/`KnownComplications`, динамические ID через `TopicIds` (11 `topicTreatment*`, 33 `topic*Phase*`, 14 `topic*Cured`).

| Результат | Детали |
|-----------|--------|
| **97 / 100** topic keys найдены в подключённых CP-файлах (`dialoguesHarvey*.json`) | Скрипт `final_validation_topics_mail.py` |
| **3 ID без dialogue key** | См. таблицу ниже |

| Topic ID | C# использование | CP dialogue | Оценка |
|----------|------------------|-------------|--------|
| `topicHarveyNeedsFirstTreatment` | `DialogueManager.AddTopic` → триггер события | **нет ключа** | **OK:** реплики в CP-событии `HarveyMod_FirstTreatment` (`events.json`), не в dialogue map |
| `topicFirstTreatmentComplete` | CP-событие ставит после прохождения; C# проверяет `HasTopic` | **нет ключа** | **OK:** финал в cutscene, отдельный dialogue не нужен |
| `topicMineRescuePending` | `PassOutHandler` — блокирует CP mine triggers | **нет ключа** | **OK:** gate-only topic, не для разговора с Харви |

**Динамические ID (`TopicIds`):** формулы `GetInjuryTopic`, `GetTreatmentTopic`, `GetCuredTopic`, `GetPhaseTopicId`, `GetComplicationTopic` раскрываются в ключи, которые **есть в CP** (в т.ч. `topicColdPhaseAcute/Healing/Recovery`, `topicSurgicalWoundCured`, все `topicTreatment*` в block1 `dialoguesHarveyCure.json`).

**Dev-команда:** `injury_audit_content` (C#) дублирует эту проверку в SMAPI-логе.

---

### 2. C# mail ID → CP mail

**Отправляет C# (`addMailForTomorrow`, `SendLetters == true`):**

| Mail ID | CP файл | Статус |
|---------|---------|--------|
| `mailHarveySleepControl` | mailInjury.json | ✓ |
| `mailHarveyMineForbidden` | mailInjury.json | ✓ |
| `HarveyMod_DirtyWoundInfection` | mailInjury.json | ✓ |
| `HarveyMod_WetBandageInfection` | mailInjury.json | ✓ |
| `HarveyMod_TreatmentUrgentReminder` | mailInjury.json | ✓ |
| `HarveyMod_TreatmentFinalWarning` | mailInjury.json | ✓ |
| `HarveyMod_NeglectWarning` | mailInjury.json | ✓ |

**Итог:** **7 / 7** — missing mail **0**.

**Примечание:** в CP также есть `mailHarvey_Neglect` (другой ID, C# **не шлёт**). Актуальный send — `MailIds.NeglectWarning` → `HarveyMod_NeglectWarning`.

**Константы `MailIds` без send (задел):** `WetCare`, `WetStitchesCare`, `InfectionAlert` — entries в CP (`mailCure.json` / `mailInjury.json`), wiring в C# отсутствует.

---

### 3. Dynamic topic IDs → CP keys

| Формула C# | Пример | CP |
|------------|--------|-----|
| `buffX` → `topicX` | `topicConcussion` | dialoguesHarveyInjury.json ✓ |
| `topicTreatment{X}` | `topicTreatmentCold` | dialoguesHarveyCure.json block1 ✓ |
| `topic{X}PhaseAcute/Healing/Recovery` | `topicFracturedBonePhaseHealing` | dialoguesHarveyCure.json ✓ |
| `topic{X}Cured` | `topicSurgicalWoundCured` | injury + cure ✓ |
| `HarveyMod_*` → `topicHarvey_*` | `topicHarvey_WetBandage` | dialoguesHarvey.json ✓ |

Legacy CP aliases (`PhaseCast`, `Phase1Ready` и т.д.) **удалены** в рамках dead-content cleanup (2026-05-23).

---

### 4. Прямые строки mail/topic в C# vs константы

| Место | Строка | Константа есть? | Риск |
|-------|--------|-----------------|------|
| `TimeEventHandler.cs:154` | `"topicHarvey_NightRound"` | **нет** в `ConversationTopics` | **LOW:** CP key есть; стоит добавить константу позже |
| `PlayerEventHandler.cs:1077` | `"situationReaction_Drunk"` | **нет** (CP trigger topic) | **LOW:** by design для triggersCare |
| `ModEntry.KnownTraumas` | `"topicHurt"` и др. | дублируют `ConversationTopics.*` | **LOW:** drift risk, не runtime bug |
| `InteractionHandler` cured list | `"buffHurt"` + `TopicIds.GetCuredTopic` | OK | — |
| Все `AddTopic` / `addMailForTomorrow` в менеджерах | — | через `ConversationTopics` / `MailIds` / `TopicIds` | ✓ |

**Нарушений «send/add с raw string вместо константы» не найдено**, кроме двух trigger-topic строк выше.

---

### 5. CP-topics без вызова (должны ли срабатывать?)

После `audit-dead-content` cleanup (2026-05-23):

| Категория | Статус |
|-----------|--------|
| Legacy `*Phase*Ready`, `topicForestRescue` When-блок | **удалены** |
| `topicHarveyModerateCare` / `topicHarveyIntensiveCare` | **подключены** в `triggersCare.json` (`AddConversationTopic` + mail) |
| `topicHarveyGentleCare` | активен через `triggersCare` ✓ |
| Relationship narrative (`topicBoyfriendWorries`, `topicHealthCheckup`, …) | **задел** — намеренно без AddTopic |
| Memory `topicHarvey_*_memory_*` (16) | **задел** — нужен memory-триггер или C# |
| Stress block (`dialoguesHarveyStress.json` не в Include) | **задел** — ~27 topics |
| NPC-only (`dialoguesNpc.json`) | **задел** |

**Активный pipeline injury/cure:** «мёртвых» topic keys, которые C# ставит, но CP не содержит, **не осталось**.

---

### 6. Романтический тон на низких отношениях

| Область | Статус |
|---------|--------|
| `dialoguesHarveyInjury.json` block1 (0♥ fallback) | **исправлено:** нейтральный врачебный «вы», без pet names (`audit-relationship-tone`, правки 2026-05-23) |
| `dialoguesHarveyInjury.json` Hospital_* | **есть** `When: Hearts 0,1` / `3,4,5` / Dating / Married |
| `dialoguesHarveyCure.json` block1 (cured / Treat / phase без When) | **частично OK:** phase/treatment переписаны профессионально; **остаток:** часть `topic*Cured` (напр. `topicBadlyHurtCured`) — «хрупкий организм», «прислушивалась» на **0♥** |
| `mailHarveySleepControl` | C# шлёт **без** проверки dating/married; текст смягчён, но всё ещё личный тон |
| Dating / Married blocks | **OK** по тону для уровня |

---

### 7. Медицинское соответствие текстов

| Область | Статус |
|---------|--------|
| Injury base topics | **OK** после medical audit (burn → не мочить, fracture → шахта запрещена, concussion → покой) |
| `PhaseTransition_*` (injury) | **эталон** — стадии конкретны |
| `topic*Phase*` block1 (cure) | **OK** — протокол вместо паники |
| `Treat_*` P0-травмы | **OK** — уникализированы After1–2 |
| `Treat_*_After3–7` (не P0) | **LOW:** шаблонный follow-up |
| `Proximity_PainFlare` | **частично:** нет контекста перелома/погоды |
| `topicBruisedRibsCured` (Hearts block) | «срослись» → в base исправлено на «ушиб зажил»; проверить hearts-блоки |
| Complication mail (`WetCare`, `NeglectWarning`) | тексты **OK**; `WetCare` не wired в C# |

---

### 8. JSON валидность

| Файлы | Strict JSON | Content Patcher |
|-------|-------------|-----------------|
| `dialoguesHarvey*.json`, `mail*.json` | **FAIL** после strip `//` — trailing commas, inline comments (JSONC) | **OK** — CP 2.x принимает JSONC |
| `events.json`, `eventsCare.json` | **FAIL** — control characters в event strings (норма для SDV) | **OK** в игре |
| `eventsMineRescue.json` | parse fail в strict validator | работает через CP |
| `content.json` | JSONC с комментариями | OK |

**Вывод:** strict JSON-парсер не применим; для CP это **ожидаемо**. Регрессий после dead-content cleanup (висячие запятые в `mailCure.json`) **не обнаружено**.

---

### 9. C# компиляция

```
dotnet build HarveyOverhaulInjury.csproj → 0 errors, 9 warnings
```

| Тип | Файлы | Заметка |
|-----|-------|---------|
| Obsolete API | `StateManager`, `TreatmentManager` | `ActivePhases`, `WasTreatmentDiscussed` |
| Nullable | `PlayerEventHandler.cs:73` | CS8604 |

Missing using/namespace/константы — **нет**. `ContentAuditRunner`, `TopicIds`, `MailIds` — на месте.

---

## Исправлено (накоплено в серии правок до этой валидации)

### C# (2026-05-23)
- `TopicIds`, расширенный `ConversationTopics`, синхронизация `MailIds`
- Dev-команда `injury_audit_content`

### CP JSON (2026-05-23)
- Добавлены missing HIGH-priority dialogue keys (`topicTreatment*`, phase, cured, complications)
- Medical / relationship tone pass (`Treat_*`, injury base, phase block1)
- Missing mail entries для C# send chain
- Dead-content cleanup: legacy phase-ready topics/mail, `topicForestRescue` When-блок
- `topicSurgicalWoundHealed` → `topicSurgicalWoundCured`
- `triggersCare`: `AddConversationTopic` для moderate/intensive care

### Документация
- `audit-topics-cp-existence.md`, `audit-mail-cp-existence.md`, `audit-dead-content.md`, medical/relationship audits

---

## Остались риски

| Приоритет | Риск | Где |
|-----------|------|-----|
| **MEDIUM** | `MailIds.WetCare` / `WetStitchesCare` / `InfectionAlert` — CP есть, C# не шлёт | ComplicationManager |
| **MEDIUM** | Дубль neglect ID: C# → `HarveyMod_NeglectWarning`, CP также хранит `mailHarvey_Neglect` | mailInjury.json |
| **MEDIUM** | Cure block1 `topic*Cured` — личный/опекунский тон на 0♥ | dialoguesHarveyCure.json |
| **MEDIUM** | `mailHarveySleepControl` без gate по отношениям | PassOutHandler + mailInjury.json |
| **LOW** | Raw string `"topicHarvey_NightRound"` без константы | TimeEventHandler.cs |
| **LOW** | `topicHealthDamageCritical/Severe` не снимаются при recovery | C# InteractionHandler |
| **LOW** | Generic `Treat_*_After3–7`, `Proximity_PainFlare` без контекста травмы | CP cure |
| **LOW** | ~100+ CP topics/mail как **задел** (stress, memory, narrative, injury alerts) | см. audit-dead-content |
| **INFO** | `dialoguesHarveyStress.json` не в Include | content.json |

---

## Что проверить в игре вручную

1. **`injury_audit_content`** в SMAPI — missing mail=0, missing dialogue=0 (или только 3 gate-topics).
2. **Первая травма 0–2♥:** диалог `topicHurt` — нейтральный тон, без pet names.
3. **Фазовое лечение:** клик по Харви → `topicTreatment*` → смена фазы → `topic*PhaseHealing` (не silence).
4. **Recovery:** финальный осмотр → `topic*Cured` (в т.ч. `topicSurgicalWoundCured`, `topicColdCured`).
5. **Complication chain:** dirty/wet wound → infection mail на следующий день.
6. **Neglect chain:** просрочка фазы → urgent mail → final warning → `HarveyMod_NeglectWarning` + `topicHarvey_Neglect`.
7. **Mine rescue:** обморок в шахте → `topicMineRescuePending` блокирует CP warning → rescue event → `topicMineInjuryRescue`.
8. **First treatment:** первая treatable травма → `topicHarveyNeedsFirstTreatment` → событие `HarveyMod_FirstTreatment`.
9. **Care triggers (dating+):** gentle → moderate mail **+ topic** → intensive mail **+ topic**.
10. **Pass-out Town:** `topicPassedOutInTown` + `mailHarveySleepControl` на следующий день.

---

## Список тестовых сценариев

| # | Сценарий | Команды / действия | Ожидание |
|---|----------|-------------------|----------|
| T1 | Audit smoke | `injury_audit_content` | missing mail=0; dialogue gaps только gate-topics |
| T2 | Лёгкая травма, 0♥ | `injury_debuff_add buffHurt` → поговорить с Харви | `topicHurt`, нейтральный текст |
| T3 | Sprained ankle phased | `injury_debuff_add buffSprainedAnkle` → лечение → фазы | `topicTreatmentSprainedAnkle`, `PhaseAcute/Healing/Recovery` |
| T4 | Surgical cured ID | `injury_debuff_add buffSurgicalWound` → recovery | `topicSurgicalWoundCured` (не `Healed`) |
| T5 | Cold phased | cold debuff → 3 phase topics | `topicColdPhaseAcute/Healing/Recovery`, `topicColdCured` |
| T6 | Dirty wound mail | dirty complication + roll infection | `HarveyMod_DirtyWoundInfection` утром |
| T7 | Wet bandage mail | wet bandage + roll | `HarveyMod_WetBandageInfection` |
| T8 | Phase neglect | `injury_phase_*` debug → пропуск дней | Urgent → Final → NeglectWarning mail |
| T9 | Mine forbidden | severe injury + mine entry + sleep | `mailHarveyMineForbidden`, buff `HarveyMod_MineForbidden` |
| T10 | Mine rescue | pass out in mine (debug) | pending topic → event → `topicMineInjuryRescue` |
| T11 | First treatment event | first Harvey-treatable injury, 750+ friendship | `HarveyMod_FirstTreatment` fires |
| T12 | Care chain | dating Harvey, несколько DayStarted | gentle topic → moderate/intensive mail+topics |
| T13 | Pass out town | pass out after 26:00 in town | `topicPassedOutInTown`, sleep mail |
| T14 | Complication topics | wet/dirty/neglect buffs | `topicHarvey_WetBandage` etc. in dialogue |
| T15 | Night round | night visit trigger (if enabled) | `topicHarvey_NightRound` dialogue from cure |
| T16 | Too cold | cold exposure topic | `topicTooCold` (TryAdd in PlayerEventHandler) |
| T17 | Health damage topics | badly hurt / fracture apply | `topicHealthDamageCritical` dialogue |
| T18 | Treatment completed | complete phased treatment | `topicTreatmentCompleted` |
| T19 | Overprotective mode | emergency supervision trigger conditions | `topicOverprotectiveMode` |
| T20 | Full reset | `injury_full_reset` (if exists) → re-apply injury | topics/mail не дублируются, audit clean |

---

## Связанные документы

- [audit-topics-cp-existence.md](./audit-topics-cp-existence.md)
- [audit-mail-cp-existence.md](./audit-mail-cp-existence.md)
- [audit-dead-content.md](./audit-dead-content.md)
- [audit-relationship-tone.md](./audit-relationship-tone.md)
- [audit-medical-texts.md](./audit-medical-texts.md)
- [id-naming-standard.md](./id-naming-standard.md)
