# План исправлений: topics & mail (HarveyOverhaul Injury ↔ CP)

Дата: 2026-05-23  
Источники: [audit-topics-csharp.md](./audit-topics-csharp.md), [audit-mail-csharp.md](./audit-mail-csharp.md), [audit-topics-cp-existence.md](./audit-topics-cp-existence.md), [audit-mail-cp-existence.md](./audit-mail-cp-existence.md), [audit-relationship-tone.md](./audit-relationship-tone.md), [audit-medical-texts.md](./audit-medical-texts.md), [audit-dynamic-id-risks.md](./audit-dynamic-id-risks.md), [audit-dead-content.md](./audit-dead-content.md), [audit-topics-mail-final.md](./audit-topics-mail-final.md).

**Статус:** только план. Код и JSON **не изменялись**.

**Легенда приоритетов:** `critical` — blank mail / silence; `high` — заметный UX-сбой; `medium` — рассинхрон / maintainability; `low` — косметика / задел.

**Автоматизация:** «да» — скрипт/grep-шаблон без смысловой правки; «частично» — генерация каркаса, текст вручную; «нет» — нужен редактор / game test.

---

## A. Безопасные CP-исправления

Добавление отсутствующих топиков/писем, правка текста, правка условий `When` в Content Patcher **без изменения C#**.

### A1. Почта — отсутствующие entries (C# уже шлёт)

| ID | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|----|---------|------------------|-------------|--------------|-----------|------|
| `HarveyMod_DirtyWoundInfection` | `assets/Code/mailInjury.json` | C# `ComplicationManager.CheckDirtyWoundComplication` → blank mail | Добавить entry: грязная рана → инфекция, срочно в клинику (адаптировать `HarveyMod_InfectionAlert`, указать причину — шахта/грязь) | low | critical | частично |
| `HarveyMod_WetBandageInfection` | `assets/Code/mailInjury.json` | C# roll wet bandage → blank mail | Добавить entry: мокрая повязка → инфекция (отличить от dirty) | low | critical | частично |
| `HarveyMod_TreatmentUrgentReminder` | `assets/Code/mailInjury.json` | C# `CheckPhaseNeglect`, день `phaseDuration + 3` → blank mail | Добавить entry: «пора на осмотр, фаза затягивается» (не путать с neglect) | low | critical | частично |
| `HarveyMod_TreatmentFinalWarning` | `assets/Code/mailInjury.json` | C# `CheckPhaseNeglect`, день `totalAllowed - 1` → blank mail | Добавить entry: «завтра последний день grace, иначе neglect» | low | critical | частично |

### A2. Topics — отсутствующие dialogue keys (C# уже ставит)

| ID | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|----|---------|------------------|-------------|--------------|-----------|------|
| `topicSurgicalWound` | `dialoguesHarveyInjury.json` | `InjuryManager.ApplySurgicalWound` add, ключа нет (есть только `topicSurgicalWoundHealed`, phase, Treat) | Добавить базовую реплику при активном `buffSurgicalWound` | low | critical | частично |
| `topicHealthDamageSevere` | `dialoguesHarveyInjury.json` | C# add при torn muscles / concussion; есть `Critical`, нет `Severe` | Добавить ключ (можно по образцу `topicHealthDamageCritical`, мягче) | low | critical | частично |
| `topicTooCold` | `dialoguesHarveyInjury.json` | `PlayerEventHandler.CheckEnvironmentalConditions` TryAdd; CP-ключа нет | Добавить ключ (не `topicStressTooCold` из неподключённого stress-файла) | low | critical | частично |
| `topicColdPhaseAcute` | `dialoguesHarveyCure.json` | `GetPhaseTopicId` + `InteractionHandler.StartTreatment` | Добавить ключ (скопировать смысл из `Treat_Cold_Acute`) | low | critical | частично |
| `topicColdPhaseHealing` | `dialoguesHarveyCure.json` | Фаза 2 cold — ключ отсутствует | Добавить ключ фазы healing | low | critical | частично |
| `topicColdPhaseRecovery` | `dialoguesHarveyCure.json` | Фаза 3 cold — ключ отсутствует | Добавить ключ фазы recovery | low | critical | частично |
| `topicColdCured` | `dialoguesHarveyCure.json` | C# `CompleteRecovery` / `CheckSimpleTreatmentCompletion` | Добавить cured-реплику («лёгкие чистые…») | low | critical | частично |
| `topicTreatmentConcussion` | `dialoguesHarveyCure.json` | `TreatmentManager.StartPhasedTreatment` — topic на весь курс | Добавить короткую реплику «лечение идёт» или reuse phase acute | low | critical | частично |
| `topicTreatmentFracturedBone` | `dialoguesHarveyCure.json` | То же (11 шаблон `topicTreatment{Injury}`) | То же | low | critical | частично |
| `topicTreatmentTornMuscles` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentSprainedAnkle` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentBruisedRibs` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentDeepCuts` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentBurnWounds` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentInfectedWound` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentBackStrain` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentShrapnelWounds` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicTreatmentCold` | `dialoguesHarveyCure.json` | То же | То же | low | critical | частично |
| `topicMineInjuryRescue` | `dialoguesHarveyInjury.json` | Event add есть (`eventsMineRescue.json`), dialogue key нет | Короткая реплика после спасения (2 д topic) | low | medium | частично |
| `topicHarvey_PainFlare` | `dialoguesHarvey.json` | Есть только `Proximity_PainFlare` в cure; нет topic key как у других осложнений | Duplicate текста `Proximity_PainFlare` как topic key | low | medium | да |

### A3. Phase aliases — duplicate keys в block1 (C# ждёт `PhaseAcute|Healing|Recovery`)

| ID (C# ожидает) | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|-----------------|---------|------------------|-------------|--------------|-----------|------|
| `topicFracturedBonePhaseHealing` | `dialoguesHarveyCure.json` block1 | CP: `topicFracturedBonePhaseCast` | Duplicate key с тем же текстом | low | high | да |
| `topicConcussionPhaseHealing` | `dialoguesHarveyCure.json` block1 | CP: `topicConcussionPhaseObservation` | Duplicate key | low | high | да |
| `topicInfectedWoundPhaseHealing` | `dialoguesHarveyCure.json` block1 | CP: `topicInfectedWoundPhaseTreatment` | Duplicate key | low | high | да |
| `topicShrapnelWoundsPhaseAcute` | `dialoguesHarveyCure.json` block1 | CP: `topicShrapnelWoundsPhaseSurgery` | Duplicate key | low | high | да |
| `topicTornMusclesPhaseRecovery` | `dialoguesHarveyCure.json` block1 | CP: `topicTornMusclesPhaseRehab` | Duplicate key | low | high | да |
| `topicSprainedAnklePhaseHealing` | `dialoguesHarveyCure.json` block1 | Есть только в hearts 6–10+, нет в neutral block1 | Скопировать блок из hearts block в block1 | low | medium | частично |
| `topicBruisedRibsPhaseRecovery` | `dialoguesHarveyCure.json` block1 | Нет в neutral block1 | Добавить в block1 | low | medium | частично |
| `topicBackStrainPhaseHealing` | `dialoguesHarveyCure.json` block1 | Нет в neutral block1 | Добавить в block1 | low | medium | частично |
| `topicBurnWoundsPhaseRecovery` | `dialoguesHarveyCure.json` block1 | Нет в neutral block1 | Добавить в block1 | low | medium | частично |
| `topicInfectedWoundPhaseRecovery` | `dialoguesHarveyCure.json` block1 | Нет в neutral block1 | Добавить в block1 | low | medium | частично |
| `Treat_Cold_Before*` / `Treat_Cold_After*` | `dialoguesHarveyCure.json` | CP: `Treat_Cold_Acute` / `Recovery`; `PickTreatmentDialogue` ищет Before/After | Duplicate keys-aliases на существующий текст | low | high | да |

### A4. Переименование / duplicate CP-only (без C#)

| ID | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|----|---------|------------------|-------------|--------------|-----------|------|
| `topicSurgicalWoundCured` | `dialoguesHarveyInjury.json` | CP: `topicSurgicalWoundHealed` — другое имя | Переименовать **или** duplicate key `topicSurgicalWoundCured` = текст Healed | medium (если CP-only events ссылаются на Healed) | critical | да (duplicate) / нет (rename) |
| `HarveyMod_NeglectWarning` | `mailInjury.json` | C# шлёт `mailHarvey_Neglect` — см. раздел C | **Вариант CP-only:** duplicate entry `mailHarvey_Neglect` = текст `HarveyMod_NeglectWarning` | low | critical | да |

### A5. Медицинские правки текста (CP-only)

| ID | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|----|---------|------------------|-------------|--------------|-----------|------|
| `Treat_Concussion_After1` | `dialoguesHarveyCure.json` | «Повязка свежая» — копипаст | Покой, вода, без экранов | low | high | нет |
| `Treat_Concussion_After2`–`After4` | `dialoguesHarveyCure.json` | «Вставай, опирайся» — противопоказано | Минимум вертикализации | low | high | нет |
| `Treat_Hurt_Before4` | `dialoguesHarveyCure.json` | Фиксация сустава при царапине | «Проверю сустав, антисептик» | low | high | нет |
| `Treat_Hurt_After1`–`After2` | `dialoguesHarveyCure.json` | «Синяк» вместо царапины | Текст про царапину / не мочить | low | high | нет |
| `topicBurnWounds` | `dialoguesHarveyInjury.json` | Нет: не мочить, пузыри, инфекция | Дополнить базовую реплику | low | high | нет |
| `topicHarvey_WetBandage` | `dialoguesHarvey.json` | Нет явного риска инфекции | Добавить «мокрая = бактерии» (как в `HarveyMod_WetCare`) | low | high | нет |
| `topicFracturedBone` | `dialoguesHarveyInjury.json` | Нет запрета шахты | Добавить в topic | low | high | нет |
| `Treat_FracturedBone_After5`–`After7` | `dialoguesHarveyCure.json` | Шаблон без запрета нагрузки | Гипс, шахта/прыжки — нет | low | high | нет |
| `Treat_InfectedWound_After1`–`After4`, `After6`–`After7` | `dialoguesHarveyCure.json` | Шаблон «повязка/вставай» | AB-курс, контроль температуры | low | high | нет |
| `topicBackStrain` | `dialoguesHarveyInjury.json` | «Массаж» при остром спазме | Покой/тепло → потом растяжка | low | high | нет |
| `Proximity_PainFlare` | `dialoguesHarveyCure.json` | Нет контекста перелома/метeo | Добавить контекст погоды/кости | low | high | нет |
| `topicBruisedRibsCured` | `dialoguesHarveyCure.json` | «Рёбра срослись» для ушиба | «Ушиб зажил» | low | medium | нет |
| `topicConcussionPhaseRecovery` | `dialoguesHarveyCure.json` | Жёстко для recovery | Смягчить по образцу `PhaseTransition_Concussion_3` | low | high | нет |
| `topicBurnWoundsPhaseHealing` | `dialoguesHarveyCure.json` | Общий текст | Переписать по `PhaseTransition_BurnWounds_2` | low | high | нет |
| `Treat_*_After*` (остальной шаблон) | `dialoguesHarveyCure.json` | ~168 строк копипаста | Уникализировать по травме (sprained ankle, deep cuts, badly hurt After5…) | medium | medium | нет |

### A6. Тон и условия — CP-only (`When` / split blocks)

| ID / группа | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|-------------|---------|------------------|-------------|--------------|-----------|------|
| `topicHurt` … `topicCold` (block1 injury) | `dialoguesHarveyInjury.json` | Без `When` — гиперопека на 0♥ | Split: `Hearts 0–2` / `3–5` / `6–10` / Dating / Married (как cured-blocks) | medium | high | нет |
| `Treat_*` block1 | `dialoguesHarveyCure.json` | «Не отпущу» на 0♥ | Та же схема hearts | medium | high | нет |
| `topic*Cured` block1 | `dialoguesHarveyCure.json` | «Хрупкая» на 0♥ | `Hearts 0–2` — clinical cured | low | medium | нет |
| `Hospital_Mon`…`Sun` (strict) | `dialoguesHarvey.json` | Ultimatum на 0♥ | `When: Hearts 6+` + injury topic/buff | medium | high | частично |
| `Resort_*`, `FlowerDance_Accept` | `dialoguesHarvey.json` | Контроль без hearts gate | `When: Hearts 6+` | low | high | да |
| `topicPassedOutInTown` (block1) | `dialoguesHarveyInjury.json` | «Одна никуда не пойдёшь» на 0♥ | 0–3♥: рекомендация; restrictiva с 4♥+ | low | high | нет |
| `topicOverprotectiveMode` | `dialoguesHarveyInjury.json` | Романтика на 0♥ | `Hearts: 6+` | low | high | да |
| `topicSpeakToHarvey` | `dialoguesHarveyInjury.json` | «Я друг» до 2♥ | `Hearts: 3+` | low | high | да |
| `topicHarveyGentleCare` | `dialoguesHarveyCare.json` | Romance без Dating | `Relationship: Dating` | low | high | да |
| `buffStrictSupervision` | `dialoguesHarveyCare.json` | Контроль на 0♥ | `Hearts: 6+` | low | high | да |
| `topicHarvey_ForcedHospitalization` | `dialoguesHarvey.json` | Intimate custody на 0♥ | Clinical language; `Hearts: 4+` | low | high | нет |
| Pet names в `Hearts 8–10` | `dialoguesHarvey.json` | «Малышка/котёнок» до Dating | Pet names → только `Relationship: Dating|Married` | medium | high | частично |
| `mailHarveySleepControl` (neutral variant) | `mailInjury.json` | Intimate curfew без dating gate в C# | Neutral entry для 0–3♥ (C# gate — см. C) | low | high | нет |
| `HarveyMod_NeglectWarning` | `mailInjury.json` | Нет мед. последствий | Добавить «пропуск перевязок → инфекция» | low | low | нет |
| `Introduction` | `dialoguesHarvey.json` | «Истощённая» незнакомцу | «Клиника открыта» | low | low | нет |

### A7. Чистка мёртвого CP (безопасное удаление после этапа 1)

| ID / группа | Файл CP | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|-------------|---------|------------------|-------------|--------------|-----------|------|
| `*Phase*Ready` (32 ключа) | `dialoguesHarveyCure.json` | Legacy CP cure; C# использует `PhaseAcute/Healing/Recovery` | Удалить после добавления canonical keys | medium (если удалить раньше alias) | medium | да |
| `topicSurgicalWoundHealed` | `dialoguesHarveyInjury.json` | Заменён на `topicSurgicalWoundCured` | Удалить после duplicate/rename | low | medium | да |
| `mailHarveyOverprotectiveNotice` / `HarveyMod_OverprotectiveNotice` | `mailInjury.json`, `mail.json` | Дубли; C# не шлёт | Оставить один | low | low | да |
| `HarveyMod_AlcoholWarning` (2 файла) | `mail.json`, `mailCure.json` | Duplicate entry | Оставить один | low | low | да |
| `topicHealthDamage` | `dialoguesHarveyInjury.json` | Legacy; C# ставит Critical/Severe | Удалить orphan key | low | medium | да |
| `topicEatSomething`, `topicSpeakToSomebody` | `dialoguesHarveyInjury.json` | Legacy hooks без AddTopic | Удалить | low | low | да |

---

## B. Требуют правки C#

C# создаёт неправильный ID, плохой динамический шаблон, строка вместо константы, или логика topic/mail неверна — **исправление только в C#** (или удаление мёртвого C#-кода).

| ID | Файл / метод C# | Текущая проблема | Исправление | Риск поломки | Приоритет | Авто |
|----|-----------------|------------------|-------------|--------------|-----------|------|
| `topicColdCured` | `InteractionHandler.CheckAndHandleCompletionTopic` | C# add динамически, но **нет** в completion list — клик не перехватывает cured-диалог | Добавить `topicColdCured` в список | low | critical | да |
| `topicSurgicalWoundCured` | `InteractionHandler.CheckAndHandleCompletionTopic` | То же для surgical | Добавить в список | low | critical | да |
| `MailIds.Neglect` | `Core/Constants.cs` | Значение `mailHarvey_Neglect`, CP — `HarveyMod_NeglectWarning` | **Альтернатива CP-only:** duplicate в CP (см. A4). **C#-fix:** изменить константу → `HarveyMod_NeglectWarning` | low | critical | да |
| `MailIds.WetCare` | `Core/Constants.cs` | `mailHarvey_WetCare`, CP — `HarveyMod_WetCare`; send не wired | Исправить значение константы | low | high | да |
| `MailIds.WetStitchesCare` | `Core/Constants.cs` | `mailHarvey_WetStitchesCare`, CP — `HarveyMod_WetStitchesCare` | Исправить значение | low | high | да |
| `MailIds.Infection` | `Core/Constants.cs` | `mailHarvey_Infection`, CP — `HarveyMod_InfectionAlert`; не используется | Исправить значение **или** удалить мёртвую константу | low | high | да |
| `mailHarveySleepControl` | `PassOutHandler.OnPlayerWarped` | Строковый литерал, не в `MailIds` | Добавить `MailIds.SleepControl` | low | medium | да |
| `HarveyMod_DirtyWoundInfection` | `ComplicationManager.CheckDirtyWoundComplication` | Строковый литерал | `MailIds.DirtyWoundInfection` | low | medium | да |
| `HarveyMod_WetBandageInfection` | `ComplicationManager.CheckWetBandageComplication` | Строковый литерал | `MailIds.WetBandageInfection` | low | medium | да |
| `HarveyMod_TreatmentUrgentReminder` | `ComplicationManager.CheckPhaseNeglect` | Строковый литерал | `MailIds.TreatmentUrgentReminder` | low | medium | да |
| `HarveyMod_TreatmentFinalWarning` | `ComplicationManager.CheckPhaseNeglect` | Строковый литерал | `MailIds.TreatmentFinalWarning` | low | medium | да |
| `topicHealthDamageSevere` | `InjuryManager.ApplyTornMuscles`, `ApplyConcussion` | Прямая строка, нет `ConversationTopics` | Добавить константу, использовать её | low | medium | да |
| `topicMineInjuryRescue` | `PlayerEventHandler` | Дублирует `ConversationTopics.MineInjuryRescue` строкой | Использовать константу | low | low | да |
| `topicHealthDamageCritical` | `InteractionHandler.RemoveInjuryRelatedTopics` | Add при травме, **не remove** при recovery | Добавить remove в recovery-flow | medium | medium | нет |
| `topicHealthDamageSevere` | `InteractionHandler.RemoveInjuryRelatedTopics` | То же | Добавить remove | medium | medium | нет |
| `topicPostOperativeCare` | `InteractionHandler.RemoveInjuryRelatedTopics` | Не снимается при recovery | Добавить remove | medium | medium | нет |
| `topicShrapnelWounds` | `InjuryManager.ApplyShrapnelWounds` | Topic duration 42 д ≠ сумма фаз 22 д | Выровнять duration = P1+P2+P3 | medium | medium | нет |
| `HarveyMod_WetCare` | `ComplicationManager` | `MailIds.WetCare` есть, `addMailForTomorrow` не вызывается | Send при wet bandage (+ опционально `WetBandageMailDay`) | medium | medium | нет |
| `HarveyMod_WetStitchesCare` | `ComplicationManager` / `PlayerEventHandler` | Аналогично wet stitches | Send при купании со швами | medium | medium | нет |
| `topicHarvey_PainFlare` | `PlayerEventHandler` / storm logic | `PainFlareOnStorm` в Constants; buff/topic **не add** в геймплее (только debug) | AddTopic при storm + fracture/shrapnel | medium | medium | нет |
| `topicHarvey_AllergicRash` | `ModEntry.CmdDebuffAdd` only | Только debug add | Wire gameplay trigger **или** оставить CP-only (см. D) | medium | low | нет |
| `topicTreatmentCompleted` | `TreatmentManager.CompleteInjuryRecovery` | Legacy/debug AddTopic без CP | Удалить AddTopic **или** оставить (если CP добавят — см. C) | low | medium | да |
| `topicStressRecoveryComplete` | `InteractionHandler.CheckAndHandleCompletionTopic` | Check-only, C# никогда не add | Удалить check | low | low | да |
| `topicTraumaHealingComplete` | `InteractionHandler.CheckAndHandleCompletionTopic` | Check-only, C# никогда не add | Удалить check | low | low | да |
| `GetInjuryFromTopic` | `InteractionHandler` | Мёртвый код, нигде не вызывается | Удалить метод | low | low | да |
| `topicHarvey_MineDeathRescue` | `Core/Constants.cs` | Legacy; фактически `topicMineInjuryRescue` | Удалить неиспользуемую константу | low | low | да |
| `WetBandageMailDay` / `WetStitchesMailDay` | `InjuryState.cs` | Legacy save fields, не пишутся | Удалить **или** wire с send mail | low | low | да |
| `Recovery_Complete_*` | `DialogueManager.LoadDialoguesFromAsset` | Filter без ключей в CP | Убрать filter | low | low | да |
| Neglect mail repeat | `ComplicationManager.CheckPhaseNeglect` | Нет `mailReceived`; одно письмо много дней | Опционально one-shot / `mailReceived` check | medium | medium | нет |
| `topicTreatment*` (альтернатива) | `TreatmentManager.StartPhasedTreatment` | 11 topic без CP — альтернатива добавлению CP | **Убрать AddTopic** — полагаться на `Treat_*` / buff / phase topics | high (меняет UX если CP ждёт topic) | high | да |

---

## C. Требуют и C#, и CP

Нужно выбрать **единый стандарт ID** и синхронизировать обе стороны.

| ID | C# | CP | Текущая проблема | Исправление (рекомендация) | Риск поломки | Приоритет | Авто |
|----|-----|-----|------------------|----------------------------|--------------|-----------|------|
| `mailHarvey_Neglect` ↔ `HarveyMod_NeglectWarning` | `MailIds.Neglect` → send | `mailInjury.json` | ID mismatch → blank mail | **Стандарт:** `HarveyMod_NeglectWarning` в C# **или** rename CP key → `mailHarvey_Neglect` | low | critical | да (C#) / да (CP rename) |
| `topicSurgicalWoundCured` ↔ `topicSurgicalWoundHealed` | `CompleteRecovery` → `topic*Cured` | `dialoguesHarveyInjury.json` | Healed ≠ Cured | **Стандарт:** `topicSurgicalWoundCured` + CP duplicate/rename + C# completion list | medium | critical | частично |
| `topicColdCured` | add + completion list | `dialoguesHarveyCure.json` | Нет CP + нет completion | CP ключ + C# `CheckAndHandleCompletionTopic` | low | critical | частично |
| `topicTooCold` ↔ `topicStressTooCold` | `PlayerEventHandler` TryAdd | `dialoguesHarveyStress.json` (не в Include) | Разные ID, stress файл отключён | **Стандарт:** `topicTooCold` в injury JSON (не переименовывать C# в stress без Include) | medium | critical | нет |
| `Treat_Cold_*` naming | `DialogueManager.PickTreatmentDialogue` | `dialoguesHarveyCure.json` | `_Acute/_Recovery` vs `_Before/_After` | CP aliases **или** C# fallback на `_Acute` | low | high | частично |
| `mailHarveySleepControl` | `PassOutHandler` — без ♥ check | `mailInjury.json` | Intimate текст при любом ♥ | C# gate `IsDatingOrMarried` для intimate **+** CP neutral variant для friendship | medium | high | нет |
| `HarveyMod_WetCare` / `WetStitchesCare` | `MailIds` + send в ComplicationManager | `mailCure.json` | Константы рассинхрон + mail не шлётся | C# wire send + C# константы = CP keys | low | medium | частично |
| `HarveyMod_InfectionAlert` | не send; send `*DirtyWoundInfection`/`*WetBandageInfection` | `mailInjury.json` | Generic alert не привязан; новые ID без CP | CP: 2 новых entry (A1) **или** C# → `InfectionAlert` + CP split text | medium | critical | нет |
| `topicTreatment*` (11) | `StartPhasedTreatment` AddTopic | нет в CP | Topic без реплики | **Вариант 1:** CP keys (A2). **Вариант 2:** убрать AddTopic (B) | medium | critical | частично |
| `topicTreatmentCompleted` | `CompleteInjuryRecovery` | нет в CP | Legacy path | CP key **или** remove AddTopic в C# | low | medium | да |
| `topicForestRescue` | нет AddTopic | `dialoguesHarvey.json` When-gate | Orphan When — Hospital block never shows | CP event AddTopic **или** удалить When-block | medium | medium | нет |
| `topicHarveyModerateCare` / `topicHarveyIntensiveCare` | triggersCare шлёт mail only | `dialoguesHarveyCare.json` | Topic keys без AddTopic | CP AddTopic в chain **или** удалить keys | low | medium | нет |
| `topicStressRecoveryComplete` / `topicTraumaHealingComplete` | check-only | нет в CP; `topicStressRecovery` в NPC/stress | Мёртвая проверка | Удалить C# check **или** полная реализация + CP + Include stress | high (если реализовывать) | low | нет |
| Phase mail `HarveyMod_*_Phase2/3` | C# не шлёт phase mail | `mailCure.json` (26 legacy) | Legacy CP mail; triggersCure off | Подключить triggers **или** C# send on phase advance **или** удалить | medium | medium | нет |
| Injury alert mail (10× `HarveyMod_*Alert`) | `InjuryManager` не send | `mailInjury.json` | Задел без C# | C# send on Apply* **или** удалить CP entries | medium | low | нет |

---

## D. Только документация / оставить как есть

Задел на будущее, намеренно отключённый контент, или не ошибка.

| ID / группа | Где | Почему оставить | Действие | Приоритет |
|-------------|-----|-----------------|----------|-----------|
| `PhaseTransition_{Injury}_{2\|3}` | CP `dialoguesHarveyInjury.json` | Аудит OK; медицинский эталон | **Не менять**; использовать как образец для `topic*Phase*` | — |
| `topicMineRescuePending` | C# `PassOutHandler` | Блокирующий topic 1 д, диалог не ожидается | Документировать | low |
| `topicFirstTreatmentComplete` | CP event `HarveyMod_FirstTreatment` | C# только check для блокировки FirstTreatment | OK — CP-owned | low |
| `topicHarveyNeedsFirstTreatment` | C# + CP event | Триггер события, не dialogue key | Опциональный dialogue — не блокер | low |
| `topicHarvey_NightRound` | C# inline + CP topic | Дублирование intentional | OK | low |
| `situationReaction_Drunk` | C# + CP | Работает | OK | — |
| `mailHarveyMineForbidden` | C# + CP | Exact match ✓ | OK | — |
| `topicHarveyTrust_*` | CP event-gated | Хорошая модель тона | OK, не трогать | — |
| Narrative mail (~38) | `mail.json` | Задел без триггеров | Архив `_archive/` или roadmap; не удалять без решения | low |
| Care recovery mail (~14) | `mailCare.json` | `triggersCare.json` закомментирован | Продуктовое решение: enable triggers **или** archive | low |
| Stress module (27 topics + 22 mail) | `dialoguesHarveyStress.json`, `mailStress.json`, `content.json` | Include + triggers off | Решение: включить модуль **или** удалить из репо | low |
| Memory topics (16× `*_memory_*`) | `dialoguesHarvey.json` | SDV memory pattern; нет RemoveTopic trigger | Wire позже **или** удалить при чистке | low |
| NPC-only topics (13) | `dialoguesNpc.json` | Не Harvey Injury C# | Отдельный модуль / NPC triggers | low |
| Cure narrative topics (8) | `dialoguesHarveyCure.json` | `topicBoyfriendWorries`, `topicStartTreatment`… | Задел relationship arc | low |
| `topicHurtPhase*` / `topicBadlyHurtPhase*` | CP cure | Hurt/BadlyHurt не phased в C# | Удалить при чистке (A7) или расширить C# — product call | low |
| `topicAlcoholPoisoningPhase*` | CP cure | Buff/topic не в C# Injury | Задел или удалить | low |
| `topicHarvey_ForcedHospitalization` | CP dialogue | C# inline dialogue, не AddTopic | OK пока inline; CP key — fallback | low |
| `topicHarvey_MineDeathRescue` | C# Constants legacy | Не используется | Удалить константу (B) | low |
| `Recovery_Complete_*` | C# filter | Ключей нет | Убрать filter (B) или добавить CP — low priority | low |
| `HarveyMod_TreatmentPlanMeeting` Friendship 500 | CP `events.json` | 2♥ — рано для intensity | Поднять до 750 — баланс, не блокер | low |
| `eventRescueOperation` Friendship 600 | CP `events.json` | 2♥ — рано | Поднять до 1000 — баланс | low |
| `eventSeen_*` farmer thoughts | CP | Post-vanilla memory | Допустимо | low |
| Два пути neglect | C# | `GameEventHandler.CheckNeglect` — без mail; `ComplicationManager` — с mail | Документировать разную UX-логику | low |
| `mailHarveyMineWarning` / `HarveyMod_MineWarning` | CP triggers | C# шлёт только `mailHarveyMineForbidden` | Документировать active vs trigger-only IDs | low |
| `topicSurgicalWoundPhase*` | CP cure | SurgicalWound не phased в C# | Dead CP до product decision | low |
| `dialoguesHarveyStress.json` закомментирован | `content.json` | Stress — отдельная система | См. stress module decision | low |
| Diff-скрипт C# ↔ CP | docs / tooling | Maintainability | Добавить `tmpMap` скрипт grep `KnownTraumas` + `MailIds` vs CP | low |

---

## Сводка по группам

| Группа | Суть | Задач (approx) | Критических |
|--------|------|----------------|-------------|
| **A** | CP-only: keys, mail, text, When | ~80 | ~25 |
| **B** | C#: wiring, constants, remove, dead code | ~30 | ~4 |
| **C** | Согласование ID обе стороны | ~15 | ~8 |
| **D** | Задел / OK / решение позже | ~25 | 0 |

---

## Порядок исправлений

### 1. ID и существование

Сначала устранить **blank mail** и **silence при разговоре** — всё, что C# уже создаёт.

1. **Mail (critical):** `HarveyMod_DirtyWoundInfection`, `HarveyMod_WetBandageInfection`, `HarveyMod_TreatmentUrgentReminder`, `HarveyMod_TreatmentFinalWarning` → CP entries (A1).
2. **Mail ID sync:** `mailHarvey_Neglect` ↔ `HarveyMod_NeglectWarning` — выбрать один ID (C + CP или CP duplicate A4).
3. **Topics missing:** `topicSurgicalWound`, `topicHealthDamageSevere`, `topicTooCold`, `topicColdPhase*`, `topicColdCured` (A2).
4. **11× `topicTreatment*`:** CP keys (A2) **или** remove AddTopic (B) — одно product-решение.
5. **Cured rename:** `topicSurgicalWoundHealed` → `topicSurgicalWoundCured` (A4 + C).
6. **C# completion list:** `topicColdCured`, `topicSurgicalWoundCured` (B).
7. **Phase aliases block1:** duplicate `PhaseCast/Observation/Treatment/Surgery/Rehab` → canonical names (A3).
8. **Treat_Cold:** Before/After aliases или C# fallback (C).
9. **MailIds constants:** WetCare, WetStitchesCare, Infection, все send literals → `MailIds` (B).

**Smoke после этапа 1:** dirty/wet → infection mail; phase neglect chain (3 письма); cold/surgical full path; `debug ebi` / `debug mail` по финальным ID.

### 2. Условия

Логика срабатывания и `When` — без переписывания всех текстов.

1. **C# RemoveTopic:** `topicHealthDamageCritical`, `topicHealthDamageSevere`, `topicPostOperativeCare` при recovery (B).
2. **C# gates:** `mailHarveySleepControl` — dating check или split mail ID (C); wet care mail send (B+C).
3. **C# duration:** `topicShrapnelWounds` 42d → 22d (B).
4. **C# PainFlare:** AddTopic при storm или документировать Proximity-only (B).
5. **CP When hearts:** injury block1, Treat block1, strict Hospital/Resort (A6).
6. **Orphan gates:** `topicForestRescue`, care topics без AddTopic (C).
7. **Neglect:** optional `mailReceived` anti-spam (B).

### 3. Тексты

После того как ключи резолвятся — медицина и тон.

1. **Medical P0:** `Treat_Concussion_After*`, `Treat_Hurt_*`, burn/wet/fracture/infected (A5).
2. **Phase tone:** переписать `topic*PhaseRecovery` / acute панику по образцу `PhaseTransition_*` (A5).
3. **Tone P0:** hearts split injury + Treat (A6); pet names только Dating+ (A6).
4. **Mail text:** neutral `mailHarveySleepControl`, мед. последствия в neglect (A6).
5. **Treat After template:** уникализация по травмам (A5, batch PR).
6. **Cured / Introduction polish** (A6, low).

### 4. Мёртвый контент

Чистка репо после фиксации активного pipeline.

1. Удалить legacy `*Phase*Ready` (32) после canonical phase keys (A7).
2. Решение по **stress module**: Include + triggers **или** удалить файлы (D).
3. Решение по **triggersCure/Injury/Stress** в `content.json` (D/C).
4. Memory topics (16): wire **или** delete (D).
5. Narrative / care mail (~52): archive или roadmap (D).
6. NPC-only, alcohol poisoning phase, hurt/badly hurt phase dead keys (A7, D).
7. C# dead code: `GetInjuryFromTopic`, legacy constants, `CheckAndHandleCompletionTopic` orphans (B).

### 5. Финальная проверка

**ID sync checklist:**
- [ ] Каждый C# `AddTopic` / `TryAdd` / `GetPhaseTopicId` / `topic*Cured` → exact CP dialogue key
- [ ] Каждый C# `addMailForTomorrow` → exact CP mail key
- [ ] `MailIds.*` = фактические CP keys; send только через константы
- [ ] Phase aliases в block1 neutral для всех phased injuries
- [ ] Скрипт diff `KnownTraumas` + `PhasedInjuries` + `MailIds` vs grep CP JSON — 0 HIGH gaps

**Smoke scenarios:**
- [ ] 0♥ первая травма — нейтральный Harvey (после tone pass)
- [ ] Concussion phases 1→2→3 на neutral hearts — topic + PhaseTransition
- [ ] Dirty wound → infection — mail с текстом
- [ ] Phase neglect — urgent → final → neglect (3 письма)
- [ ] Cold: topic → phases → `topicColdCured` + completion dialogue
- [ ] Surgical: `topicSurgicalWound` → cure → `topicSurgicalWoundCured`
- [ ] `SendLetters = false` — no crash; `= true` — all mails deliver

**SMAPI debug (отладка мода):**
```text
injury_debuff_add buffConcussion
injury_phase_advance buffConcussion
injury_phase_recovery buffCold 1
debug ebi Harvey topicColdCured
debug mail HarveyMod_DirtyWoundInfection
injury_reset
```

---

## Связанные документы

| Документ | Когда смотреть |
|----------|----------------|
| [audit-dynamic-id-risks.md](./audit-dynamic-id-risks.md) | Шаблоны Replace, GetPhaseTopicId |
| [audit-topics-cp-existence.md](./audit-topics-cp-existence.md) | C# topic → CP dialogue |
| [audit-mail-cp-existence.md](./audit-mail-cp-existence.md) | C# mail → CP Mail |
| [audit-dead-content.md](./audit-dead-content.md) | CP → вызовы (обратный аудит) |
| [audit-relationship-tone.md](./audit-relationship-tone.md) | Hearts / Dating gates |
| [audit-medical-texts.md](./audit-medical-texts.md) | Медицина по травмам |
| [audit-topics-mail-final.md](./audit-topics-mail-final.md) | Итоговая сводка |

---

*Документ составлен по результатам аудита. Код и JSON не изменялись.*
