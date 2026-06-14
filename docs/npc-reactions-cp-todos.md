# NPC reactions CP — TODO (отсутствующие gates)

Список состояний из [`npc-reactions-source-audit.md`](npc-reactions-source-audit.md), для которых **нет отдельного CP-safe topic/buff** и реакции пока не добавлены или покрыты только через fallback.

---

## Recovery plan — нарушения без отдельного topic

| Состояние (C#) | Fallback topic | TODO |
|----------------|----------------|------|
| `IgnoredCheckup` (`missed_harvey_checkup`) | `HarveyMod_RecoveryPlanViolated` | Добавить `HarveyMod_RecoveryPlanViolated_IgnoredCheckup` в C# + CP, если нужна отдельная реплика NPC про пропуск осмотра |
| `PassedOut` (`passed_out`) | `HarveyMod_RecoveryPlanViolated` | Добавить `HarveyMod_RecoveryPlanViolated_PassedOut` для точного gate «обморок во время режима» |
| Вход в вулкан (`entered_volcano`) | `HarveyMod_RecoveryPlanViolated_Mine` | Отдельный topic не нужен, если `_Mine` достаточно; при необходимости — `HarveyMod_RecoveryPlanViolated_Volcano` |

**Текущее покрытие CP:** категория C использует bridge `HarveyOverhaul_NPCReaction_RecoveryViolated` / `_RecoveryViolatedMine` по существующим topics из аудита.

---

## Stress — отсутствующие bridge-topics для NPC

| Состояние | Проблема | TODO |
|-----------|----------|------|
| `topicStressSocial` (legacy) | Обнулён для Харви в `stressTopicsUnimplementedDisabled.json`; C# может не ставить topic для NPC-gate | Реакции F используют `buffStressSocial`, `HarveyMod_SocialRecovery`, `topicStressTreatmentSocialFollowup` |
| `topicOverworkBreakInterrupted` | Есть в StressMeter, не использован в bridge | При необходимости отдельный trigger для «сорванного перерыва» |
| `HarveyMod_CD_StormComfort` | Короткий TTL, **не gate** | Намеренно не используется (см. аудит §7) |

---

## Injury — дифференциация лёгкое / тяжёлое

| Задача | TODO |
|--------|------|
| Разные реплики Abigail для `buffHurt` vs `buffFracturedBone` | **Сделано:** `HarveyOverhaul_NPCReaction_Young_Injury` / `_InjurySevere` в `npc_reactions_young.json` |
| Marlon на лёгкой фермерской травме | Сейчас Marlon реагирует на `InjuryRecent` с шахтным текстом — при желании ограничить gate только `topicMineInjuryRescue` + severe buffs |

---

## Dating / Engaged / Married overlay

| Задача | TODO |
|--------|------|
| Отдельные реплики при `Relationship:Harvey` | Частично заложено в текст (Marnie D, Evelyn B); полный набор state 12 из matrix — отдельный проход с `When: Relationship:Harvey` |

---

## Техническая заметка: префикс ключей

Stardew показывает диалог NPC только если **ключ entry = активный ConversationTopic**. Поэтому:

- **Ключи диалогов:** `HarveyOverhaul_NPCReaction_*` (bridge topics)
- **Реальные gates** (травма, режим, stress buffs) → `TriggerActions` на `DayStarted` в `triggersNpcReactionsInjuryStress.json`
- Ванильные и существующие ключи в `dialoguesNpc.json` **не перезаписываются** (другие topic ID + Priority `Late + 3`)

Долгосрочно: C# может выставлять bridge topics напрямую и убрать CP-триггеры.

---

## RumorSoft (ambient, редко)

| Файл | Gate | Частота |
|------|------|---------|
| `triggersNpcReactionsRumorSoft.json` | `RANDOM 0.18` + mod care ANY + `!FestivalDay` | ~1 раз в 5–6 дней при активном состоянии |
| `npc_reactions_rumor_soft.json` | `HarveyOverhaul_NPCReaction_RumorSoft` (1 день) | 13 NPC, по 2 варианта (`$c`) |

Без OnTimeChanged; без диагнозов и «Харви рассказал».

---

## Maru (клиника)

| Файл | Ключи |
|------|-------|
| `npc_reactions_maru.json` | `HarveyOverhaul_NPCReaction_Maru_*` (11 состояний) |
| `triggersNpcReactionsMaru.json` | Отдельные bridge topics; при Dating/Engaged/Married — Maru_* вместо общих + HarveyRel для Maru |

Старые реплики Maru в `npc_reactions_injury_stress.json` и `npc_reactions_harvey_relationship.json` удалены.

---

## Marlon (шахты и режим)

| Файл | Ключи | Нагрузка |
|------|-------|----------|
| `npc_reactions_marlon.json` | `HarveyOverhaul_NPCReaction_Marlon_*` (9 состояний) | — |
| `triggersNpcReactionsMarlon.json` | Только `DayStarted`, без `OnTimeChanged` | ≤1 bridge topic/день при активном gate |

Gates: `topicMineInjuryRescue`, тяжёлые `topic*`, `HarveyMod_RecoveryPlanStarted`, `HarveyMod_RecoveryPlanViolated_Mine`, `HasBuff HarveyMod_MineForbidden` / `MineRestricted`, `Relationship:Harvey`.

Старые реплики Marlon в `npc_reactions_injury_stress.json`, `npc_reactions_harvey_relationship.json` и mine-триггеры HarveyRel удалены.

---

## HomeCare (домашние NPC)

| Файл | Bridge topics | NPC |
|------|---------------|-----|
| `npc_reactions_home_care.json` | `HarveyOverhaul_NPCReaction_HomeCare_*` | Evelyn, George, Marnie, Robin, Jodi, Caroline, Gus |
| `triggersNpcReactionsHomeCare.json` | 6 категорий, только `DayStarted` | Без романа с Harvey (есть HarveyRel) |

Категории: усталость, после лечения, режим соблюдён/нарушен, гроза, соц. тревога/одиночество. Без диагнозов.

---

## Young (молодые NPC)

| Файл | Bridge topics | NPC |
|------|---------------|-----|
| `npc_reactions_young.json` | `HarveyOverhaul_NPCReaction_Young_*` (10 bridge) | Abigail, Sam, Sebastian, Haley, Alex, Penny, Leah, Elliott, Emily, Shane, Clint |
| `triggersNpcReactionsYoung.json` | Только `DayStarted` | Gates по аудиту; без исключения романа с Harvey |

Категории: Injury (лёгкая/тяжёлая), RecoveryPlan, RecoveryViolated, MineRescue, Exhaustion, Thunder, Darkness, Social, Overwork.

Penny / Leah / Elliott при `Relationship:Harvey` Dating/Engaged/Married — Young отключён (`When`), чтобы не конфликтовать с `HarveyRel_*`.

Старые реплики этих NPC в `npc_reactions_injury_stress.json` удалены (Abigail InjuryMine, Penny, Shane, Leah, Elliott, Emily, Haley, Clint).

---

## Kids (Jas, Vincent)

| Файл | Bridge topics | Частота |
|------|---------------|---------|
| `npc_reactions_kids.json` | `HarveyOverhaul_NPCReaction_Kids_Rest`, `_Kind`, `_Storm` | 6 реплик (3×2 NPC) |
| `triggersNpcReactionsKids.json` | `RANDOM 0.12`, `!FestivalDay`, `Kids_Cooldown` 4 дня | Только мягкие gates; без тяжёлых травм, шахты, обмороков |
