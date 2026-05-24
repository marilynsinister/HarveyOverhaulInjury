# Аудит conversation topic ID в C# (HarveyOverhaul.InjuryCare)

Дата: 2026-05-24 (актуализация)  
Область: все `.cs` файлы проекта.  
`PLAYER_HAS_CONVERSATION_TOPIC` в C# **не найден** — условия событий в CP.

---

## Сводка

| Категория | Кол-во |
|-----------|--------|
| Базовые травмы | 14 |
| Сопутствующие | 3 |
| `topicTreatment*` | 11 |
| Фазовые `topic*Phase*` | 33 |
| `topic*Cured` | 14 |
| Осложнения `topicHarvey_*` | 6 |
| Обморок / шахта / FirstTreatment | 7 (+ minor rescue) |
| Event-only topics (без dialogue key) | 4 |
| Launchers (storm / rescue) | 3 |

---

## Ключевые изменения с 2026-05-23

| Область | Было | Стало |
|---------|------|-------|
| Динамические topic ID | Inline `Replace` | Класс `TopicIds` + `ConversationTopics` |
| Completion list | Без Cold/Surgical | `topicColdCured`, `topicSurgicalWoundCured` в `CheckAndHandleCompletionTopic` |
| Neglect mail | `mailHarvey_Neglect` | `MailIds.NeglectWarning` |
| AppliedTriggers | Все one-shot | **Story one-shot** (`SurgicalWound`, `ExplosionInjury`) + **injury cooldown** (`InjuryCooldownUntilDay`) для repeatable |
| `topicDiagnosisComplete` | Не ставился | `DialogueManager.TryAddDiagnosisCompleteTopic` при старте лечения eligible травм |
| `topicRescueOperation` | Orphan | `RescueOperationLauncher` после E5 storm |
| Storm comfort | `buffStressThunder` не ставился | `StormComfortLauncher` — buff или fallback `topicHarveyStormStress` |
| Minor mine rescue | Недостижим (всегда Severe) | `PassOutHandler.TryTriggerMinorMineRescue` — опасное состояние без Severe |
| Pass-out cutscenes | Только buff/topic | `QueueHospitalEvent` → `eventHarveyEmergencyCare` / `eventHarveyExhaustion` |
| Legacy checks | `topicStressRecoveryComplete` | **Удалены** |

---

## Event-only topics (dialogue key не требуется)

| Topic ID | Кто ставит | CP использование |
|----------|------------|------------------|
| `topicHarveyNeedsFirstTreatment` | C# `DialogueManager` | Precondition `HarveyMod_FirstTreatment` |
| `topicFirstTreatmentComplete` | CP event script | C# check-only |
| `topicDiagnosisComplete` | C# `TryAddDiagnosisCompleteTopic` | Precondition `HarveyMod_TreatmentPlanMeeting` |
| `topicRescueOperation` | C# `RescueOperationLauncher` | Precondition `eventRescueOperation` |
| `topicMineRescuePending` | C# `PassOutHandler` | Блокирует CP interception (1 д) |
| `topicHarveyStormStress` | C# `StormComfortLauncher` | Fallback gate storm comfort events |
| `topicHarveyMinorMineRescue` | C# после `eventHarveyMinorMineRescue` | **Нет dialogue key** — MEDIUM |

---

## Активные риски для CP-синхронизации

1. **`topicHealthDamageCritical/Severe`, `topicPostOperativeCare`** — add при травме, **нет remove** при recovery.
2. **`topicShrapnelWounds`** — topic duration 42 д ≠ сумма фаз 22 д.
3. **Фазовые topics 2–3** — C# не AddTopic при смене фазы (только PhaseTransition dialogue).
4. **`topicHarvey_AllergicRash`, `topicHarvey_PainFlare`** — только debug add; PainFlare buff не wired в gameplay.
5. **Ночной визит** — без dating gate.

---

## Префиксы диалогов (не conversation topics)

| Префикс | Где | CP |
|---------|-----|-----|
| `PhaseTransition_*` | `InteractionHandler.AdvanceToNextPhase` | ✓ injury JSON |
| `Treat_*` | `DialogueManager.PickTreatmentDialogue` | ✓ cure JSON |
| `Proximity_*` | `TreatmentManager.BuildCombinedDialogue` | ✓ cure JSON |

---

## Индекс файлов

| Файл | Роль |
|------|------|
| `Core/Constants.cs` | `ConversationTopics`, `TopicIds`, launchers |
| `Managers/DialogueManager.cs` | add/remove; FirstTreatment; DiagnosisComplete |
| `Managers/InjuryManager.cs` | базовые/health/post-op topics |
| `Managers/TreatmentManager.cs` | `topicTreatment*` |
| `EventHandlers/InteractionHandler.cs` | cured, phase1, completion |
| `EventHandlers/PassOutHandler.cs` | pass-out, mine rescue, hospital events |
| `EventHandlers/StormComfortLauncher.cs` | storm buff/topic |
| `EventHandlers/RescueOperationLauncher.cs` | rescue operation topic |
| `Managers/StateManager.cs` | cooldown vs story triggers |
