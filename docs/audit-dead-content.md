# Обратный аудит: мёртвый CP-контент (topics & mail)

Дата: **2026-05-24** (актуализация; таблицы пересчитаны скриптом `tmpMap/gen_audit_dead_content.py`)  
Направление: **CP → вызовы** (противоположность audit-topics-cp-existence / audit-mail-cp-existence).

## История обработки

| Дата | Действие |
|------|----------|
| 2026-05-23 | CP JSON cleanup: ~36 legacy topic keys, 38 mail keys удалены; 3 topic подключены |
| 2026-05-23 | C# sync: `TopicIds`, `MailIds`, completion list, CP dialogue/mail gaps |
| 2026-05-24 | Пересчёт: legacy `*Phase*Ready` удалены → **0** в группе A; dead topics **64**, dead mail **79** |

## Метод

1. Собраны ключи `topic*` и `mail*` / `HarveyMod_*` из CP dialogue и mail JSON.
2. Искали вызовы в:
   - C#: `AddTopic`, `RemoveTopic`, `HasTopic`, `TryAdd`, `addMailForTomorrow`, `KnownTraumas`
   - CP: `addConversationTopic`, `AddConversationTopic`, `addMail`/`AddMail`, `PLAYER_HAS_CONVERSATION_TOPIC`, `PLAYER_HAS_MAIL`, `#$t topic`
   - Файлы: `events.json`, `eventsCare.json`, `eventsMineRescue.json`, `triggersCare.json` (+ закомментированные triggers*)
3. Динамические C# ID (`topicTreatment*`, `topic*Phase*`, `topic*Cured`) считаются **вызываемыми**, даже если exact key отсутствует в grep CP.

## Сводка

| | Topics | Mail |
|--|--------|------|
| Всего ключей в CP | 247 | 104 |
| С найденным вызовом | 183 | 25 |
| **Без вызова (мёртвые кандидаты)** | **64** | **79** |

**Главные причины мёртвого контента:**

1. **Legacy phased cure** — ~40 `topic*Phase*Ready` и phase-mail из старой CP-системы; C# использует `GetPhaseTopicId` → `PhaseAcute/Healing/Recovery`.
2. **`dialoguesHarveyStress.json` закомментирован** в `content.json` — весь stress-topic блок не грузится.
3. **Закомментированы triggers** (`triggersCure`, `triggersInjury`, `triggersStress`) — care/recovery/stress mail не отправляются.
4. **Memory topics** (`*_memory_oneday/oneweek`) — SDV memory keys; C#/CP не ставят их при снятии complication topic.
5. **Narrative mail в `mail.json`** — ~30 писем без триггеров (задел на будущее).

---

## Приоритет 1 — критичные несовпадения (актуально)

| ID | Проблема | Статус |
|----|----------|--------|
| `topicSurgicalWoundHealed` | C# → `topicSurgicalWoundCured` | **✅ done** — legacy Healed удалён |
| `HarveyMod_WetCare`, `HarveyMod_WetStitchesCare` | `MailIds` есть, send не wired | **⚠️ MEDIUM** — задел в CP |
| `mailHarvey_Neglect` vs `HarveyMod_NeglectWarning` | Дубли ID | **✅ C#** шлёт `NeglectWarning`; `mailHarvey_Neglect` — dead CP |
| Memory topics (16 keys) | Memory не ставятся | **kept intentionally** |
| `topicHarveyMinorMineRescue` | C# add, dialogue нет | **⚠️ MEDIUM** — добавить CP key |

---

## B. Stress (dialoguesHarveyStress.json — не в Include) (27)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `topicDarknessLanternReceived` | topic | dialoguesHarveyStress.json | — | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicDarknessStep1Complete` | topic | dialoguesHarveyStress.json | — | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicDarknessStep2Complete` | topic | dialoguesHarveyStress.json | — | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressAnxietyWave` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressBadDream` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressBreakdown` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressCollapse` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressCritical` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressCriticism` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDarkness` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDarknessLevel2` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDarknessLevel3` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDespair` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressFreezeResponse` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressHunger` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressIsolation` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressLonely` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressMentalFatigue` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressNoSleep` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressNumbness` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressOverwork` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressPanic` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressShadowParanoia` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressSleepDeprivation` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressSocial` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressTired` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | C# stress system / triggersStress (отключён) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressTooCold` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |

## C. Memory topics (после снятия complication) (16)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `topicHarvey_AllergicRash_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_AllergicRash | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_AllergicRash_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_AllergicRash | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_DirtyWound_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_DirtyWound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_DirtyWound_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_DirtyWound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_EscalatedCare_memory_oneday` | topic | dialoguesHarvey.json | SDV memory после topicHarvey_EscalatedCare | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_EscalatedCare_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_EscalatedCare | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_ForcedHospitalization_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_ForcedHospitalization | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_ForcedHospitalization_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_ForcedHospitalization | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_Neglect_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_Neglect | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_Neglect_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_Neglect | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_NightRound_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_NightRound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_NightRound_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_NightRound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetBandage_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetBandage | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetBandage_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetBandage | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetStitches_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetStitches | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetStitches_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetStitches | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |

## D. NPC-only topics (13)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `topicEmotionalSupport` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicFatigue` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHarveyLove` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHarveySupport` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHope` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicMedicalCare` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicMentalExhaustion` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicMentalHealth` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicPanicAttacks` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicPersonalStruggle` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicSleepIssues` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicStressRecovery` | topic | dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicTrauma` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |

## E. Cure narrative / relationship (8)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `topicBoyfriendWorries` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicHarvey_EscalatedCare` | topic | dialoguesHarveyCure.json | — | **нет** | задел (нет AddTopic) | подключить в triggersCare или удалить |
| `topicHealthCheckup` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicHusbandlyProtection` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicPreventiveCare` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicProtectiveBoyfriend` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicStartTreatment` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicWifelyWorries` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |

## Mail: 1. Narrative mail (mail.json) (33)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `HarveyMod_AdvancedTreatmentUnlocked` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_AlcoholWarning` | mail | mail.json, mailCure.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_AnniversaryReflection` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ComfortLetter` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_CriticalCareUnlocked` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_DangerWarning` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_DiagnosisComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_DoctorWorries` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_EmergencyHospitalization` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_EscalationNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ExtendedTreatmentNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_FamilySupport` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_FatigueTreatmentComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_FuturePlans` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_HealthyHolidays` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_LoveConfession` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_MedicalRecognition` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_MonthlyCheckup` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_MovingInNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_PanicAttackComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_PerfectPatientAward` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ProfessionalSuccess` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ProtectionOffer` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_PsychologicalSupport` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_RecoveryReliefLetter` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_RelapseNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_SleepTherapyComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_TraumaAnxietyNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_TreatmentPlanReady` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_TreatmentSeriesComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ViolationWarning` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_WeatherWarning` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_WinterHealthTips` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |

## Mail: 2. Care recovery chain (mailCare.json) (14)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `mailHarveyPostTrauma` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecovery1` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecovery2` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecovery3` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecoveryFinal` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecoveryFinalDating` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecoveryFinal_Friendship` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRestRequired` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep1` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep1Dating` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep2` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep2Dating` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep3` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep3Dating` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |

## Mail: 3. Injury alert mail (mailInjury.json) (10)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `HarveyMod_AnkleInjuryAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_BackStrainAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_BurnWoundsAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_ConcussionAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_DeepCutsAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_FractureAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_RibInjuryAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_ShrapnelAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_TornMusclesAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `mailHarvey_Neglect` | mail | mailInjury.json | — | **нет** | мёртвый / не подключён | удалить или подключить вызов |

## Mail: 4. Phased cure mail (mailCure.json) (1)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `HarveyMod_AlcoholWarning` | mail | mail.json, mailCure.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |

## Mail: 5. Stress mail (mailStress.json) (22)

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `mailHarveyStressTreatmentAnxietyWave` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentBadDream` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentBreakdown` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentCollapse` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentCritical` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentCriticism` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentDarkness` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentDespair` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentFreezeResponse` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentHunger` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentIsolation` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentLonely` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentMentalFatigue` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentNoSleep` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentNumbness` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentOverwork` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentPanic` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentShadowParanoia` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentSocial` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentThunder` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentTired` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentTooCold` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |

---

## Полная таблица: topics без вызова

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `topicBoyfriendWorries` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicDarknessLanternReceived` | topic | dialoguesHarveyStress.json | — | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicDarknessStep1Complete` | topic | dialoguesHarveyStress.json | — | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicDarknessStep2Complete` | topic | dialoguesHarveyStress.json | — | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicEmotionalSupport` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicFatigue` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHarveyLove` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHarveySupport` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHarvey_AllergicRash_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_AllergicRash | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_AllergicRash_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_AllergicRash | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_DirtyWound_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_DirtyWound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_DirtyWound_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_DirtyWound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_EscalatedCare` | topic | dialoguesHarveyCure.json | — | **нет** | задел (нет AddTopic) | подключить в triggersCare или удалить |
| `topicHarvey_EscalatedCare_memory_oneday` | topic | dialoguesHarvey.json | SDV memory после topicHarvey_EscalatedCare | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_EscalatedCare_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_EscalatedCare | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_ForcedHospitalization_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_ForcedHospitalization | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_ForcedHospitalization_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_ForcedHospitalization | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_Neglect_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_Neglect | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_Neglect_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_Neglect | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_NightRound_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_NightRound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_NightRound_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_NightRound | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetBandage_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetBandage | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetBandage_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetBandage | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetStitches_memory_oneday` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetStitches | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHarvey_WetStitches_memory_oneweek` | topic | dialoguesHarvey.json | Memory-реплика после снятия topicHarvey_WetStitches | **нет** | задел (memory после снятия topic) | подключить memory-триггер при RemoveTopic или удалить |
| `topicHealthCheckup` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicHope` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicHusbandlyProtection` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicMedicalCare` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicMentalExhaustion` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicMentalHealth` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicPanicAttacks` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicPersonalStruggle` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicPreventiveCare` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicProtectiveBoyfriend` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicSleepIssues` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicStartTreatment` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |
| `topicStressAnxietyWave` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressBadDream` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressBreakdown` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressCollapse` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressCritical` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressCriticism` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDarkness` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDarknessLevel2` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDarknessLevel3` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressDespair` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressFreezeResponse` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressHunger` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressIsolation` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressLonely` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressMentalFatigue` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressNoSleep` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressNumbness` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressOverwork` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressPanic` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressRecovery` | topic | dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicStressShadowParanoia` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressSleepDeprivation` | topic | dialoguesHarveyStress.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressSocial` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressTired` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | C# stress system / triggersStress (отключён) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicStressTooCold` | topic | dialoguesHarveyStress.json, dialoguesNpc.json | Stress-триггеры (triggersStress.json закомментирован) | **нет** | мёртвый (файл не в Include) | подключить dialoguesHarveyStress.json или удалить |
| `topicTrauma` | topic | dialoguesNpc.json | NPC reaction topic (не Harvey C#) | **нет** | NPC-only topic (Harvey не ставит) | NPC-триггер или удалить dialogue key |
| `topicWifelyWorries` | topic | dialoguesHarveyCure.json | — | **нет** | задел (relationship cure narrative) | подключить триггер/событие или удалить |

---

## Полная таблица: mail без вызова

| ID | Тип | Где определён | Где должен вызываться | Найден вызов | Мёртвый/задел | Что сделать |
|----|-----|---------------|----------------------|--------------|---------------|-------------|
| `HarveyMod_AdvancedTreatmentUnlocked` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_AlcoholWarning` | mail | mail.json, mailCure.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_AnkleInjuryAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_AnniversaryReflection` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_BackStrainAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_BurnWoundsAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_ComfortLetter` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ConcussionAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_CriticalCareUnlocked` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_DangerWarning` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_DeepCutsAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_DiagnosisComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_DoctorWorries` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_EmergencyHospitalization` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_EscalationNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ExtendedTreatmentNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_FamilySupport` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_FatigueTreatmentComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_FractureAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_FuturePlans` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_HealthyHolidays` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_LoveConfession` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_MedicalRecognition` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_MonthlyCheckup` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_MovingInNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_PanicAttackComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_PerfectPatientAward` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ProfessionalSuccess` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ProtectionOffer` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_PsychologicalSupport` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_RecoveryReliefLetter` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_RelapseNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_RibInjuryAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_ShrapnelAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_SleepTherapyComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_TornMusclesAlert` | mail | mailInjury.json | C# InjuryManager при Apply* injury | **нет** | задел (injury alert mail) | подключить C# OnInjuryApplied или удалить |
| `HarveyMod_TraumaAnxietyNotice` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_TreatmentPlanReady` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_TreatmentSeriesComplete` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_ViolationWarning` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_WeatherWarning` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `HarveyMod_WinterHealthTips` | mail | mail.json | — | **нет** | задел (narrative mail без триггера) | подключить триггер/событие или удалить |
| `mailHarveyPostTrauma` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecovery1` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecovery2` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecovery3` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecoveryFinal` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecoveryFinalDating` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRecoveryFinal_Friendship` | mail | mailCare.json | Care recovery chain после травмы | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyRestRequired` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep1` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep1Dating` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep2` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep2Dating` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep3` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStep3Dating` | mail | mailCare.json | — | **нет** | задел (care recovery chain) | подключить triggersCare chain или удалить |
| `mailHarveyStressTreatmentAnxietyWave` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentBadDream` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentBreakdown` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentCollapse` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentCritical` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentCriticism` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentDarkness` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentDespair` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentFreezeResponse` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentHunger` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentIsolation` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentLonely` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentMentalFatigue` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentNoSleep` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentNumbness` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentOverwork` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentPanic` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentShadowParanoia` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentSocial` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentThunder` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentTired` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarveyStressTreatmentTooCold` | mail | mailStress.json | — | **нет** | задел (stress-триггеры закомментированы) | подключить triggersStress или удалить |
| `mailHarvey_Neglect` | mail | mailInjury.json | — | **нет** | мёртвый / не подключён | удалить или подключить вызов |

---

## Активные topics (для справки, вызов найден)

Всего 183. Примеры:

- `topicAcceptHospital` ← CP events/triggers
- `topicAfterCheckup` ← CP events/triggers; CP eventsCare.json
- `topicAgreedCheckup` ← CP PLAYER_HAS_CONVERSATION_TOPIC; CP events/triggers; CP eventsCare.json
- `topicAlcoholPoisoningPhaseAcute` ← C# GetPhaseTopicId
- `topicAlcoholPoisoningPhaseHealing` ← C# GetPhaseTopicId
- `topicAlcoholPoisoningPhaseRecovery` ← C# GetPhaseTopicId
- `topicBackStrain` ← C# AddTopic
- `topicBackStrainCured` ← C# dynamic topic*Cured
- `topicBackStrainPhaseAcute` ← C# GetPhaseTopicId
- `topicBackStrainPhaseHealing` ← C# GetPhaseTopicId
- `topicBackStrainPhaseRecovery` ← C# GetPhaseTopicId
- `topicBadlyHurt` ← C# AddTopic
- `topicBadlyHurtCured` ← C# dynamic topic*Cured
- `topicBadlyHurtPhaseAcute` ← C# GetPhaseTopicId
- `topicBadlyHurtPhaseHealing` ← C# GetPhaseTopicId
- `topicBadlyHurtPhaseRecovery` ← C# GetPhaseTopicId
- `topicBruisedRibs` ← C# AddTopic
- `topicBruisedRibsCured` ← C# dynamic topic*Cured
- `topicBruisedRibsPhaseAcute` ← C# GetPhaseTopicId
- `topicBruisedRibsPhaseHealing` ← C# GetPhaseTopicId
- `topicBruisedRibsPhaseRecovery` ← C# GetPhaseTopicId
- `topicBurnWounds` ← C# AddTopic
- `topicBurnWoundsCured` ← C# dynamic topic*Cured
- `topicBurnWoundsPhaseAcute` ← C# GetPhaseTopicId
- `topicBurnWoundsPhaseHealing` ← C# GetPhaseTopicId

*(… и ещё 158)*

## Активные mail (для справки)

- `HarveyMod_DirtyWoundInfection` ← C# addMailForTomorrow
- `HarveyMod_EmergencyNightWarning` ← CP events/triggers
- `HarveyMod_EmergencySupervision` ← CP events/triggers
- `HarveyMod_InfectionAlert` ← C# addMailForTomorrow
- `HarveyMod_LateNightWarning` ← CP events/triggers
- `HarveyMod_MineWarning` ← CP events/triggers
- `HarveyMod_NeglectWarning` ← C# addMailForTomorrow
- `HarveyMod_TreatmentFinalWarning` ← C# addMailForTomorrow
- `HarveyMod_TreatmentUrgentReminder` ← C# addMailForTomorrow
- `HarveyMod_WetBandageInfection` ← C# addMailForTomorrow
- `HarveyMod_WetCare` ← C# addMailForTomorrow
- `HarveyMod_WetStitchesCare` ← C# addMailForTomorrow
- `mailHarveyCaveWarning` ← CP events/triggers
- `mailHarveyIntensiveCare` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyMedicalCheckReminder` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyMineForbidden` ← C# addMailForTomorrow
- `mailHarveyMineWarning` ← CP events/triggers
- `mailHarveyModerateCare` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyNote1` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyNote2` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyNote3` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyNote4` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyNoteGirlfriend` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveyNoteWife` ← CP PLAYER_HAS_MAIL; CP events/triggers
- `mailHarveySleepControl` ← C# addMailForTomorrow

## Закомментированная инфраструктура (content.json)

| Include | Статус | Влияние |
|---------|--------|---------|
| `dialoguesHarveyStress.json` | **закомментирован** | ~30 stress topics мёртвы |
| `triggersCure.json` | закомментирован | phase mail, cure triggers |
| `triggersInjury.json` | закомментирован | injury alert mail |
| `triggersStress.json` | закомментирован | stress mail chain |
| `triggersQuestsStress.json` | закомментирован | quest stress |

## Рекомендуемый порядок чистки

1. **Удалить или архивировать** legacy `*Phase*Ready` topics + phase mail (группы A + mailCure phase).
2. **Решить по stress**: включить Include + triggers **или** удалить `dialoguesHarveyStress.json` + `mailStress.json`.
3. **Подключить** 3–5 high-value narrative mail (LateNight, MineWarning уже в triggersCare — OK).
4. **Memory topics**: либо CP TriggerActions при RemoveTopic, либо удалить 14 keys.
5. **Синхронизировать** `topicSurgicalWoundHealed` / `HarveyMod_WetCare` с C#.
