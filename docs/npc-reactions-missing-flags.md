# NPC reactions — флаги отношений с Харви

Проверка для overlay-реакций `HarveyOverhaul_NPCReaction_HarveyRel_*` (Dating / Engaged / Married).

---

## Доступно в Content Patcher (используется)

| Условие CP | TriggerActions | When в патче |
|------------|----------------|--------------|
| Dating | `PLAYER_NPC_RELATIONSHIP Current Harvey Dating` | `"Relationship:Harvey": "Dating"` |
| Engaged | `PLAYER_NPC_RELATIONSHIP Current Harvey Engaged` | `"Relationship:Harvey": "Engaged"` |
| Married | `PLAYER_NPC_RELATIONSHIP Current Harvey Married` | `"Relationship:Harvey": "Married"` |

Источник: ванильное `FriendshipStatus` через SMAPI/CP; уже используется в `dialogues/harvey_dating.json`, `harvey_married.json`, `dialoguesHarveyRecoveryPlan.json` и др.

**Вывод:** отдельные invented-флаги для Dating / Engaged / Married **не нужны**.

---

## Нет отдельного gate (TODO на будущее)

| Задача | Проблема | TODO |
|--------|----------|------|
| Реакция «только что стали Dating» vs «Dating давно» | Нет conversation topic «relationship milestone» без события | При желании — topic из C#/события после bouquet |
| Divorced / снова Friendly | CP видит `Divorced`; отдельные реплики не добавлены | Не включать без явного дизайна |
| Hearts 0–2 + Married | Теоретически невозможно в ваниле | — |
| Реакция без контекста травмы/stress | Только `Relationship:Harvey` без mod state | Намеренно не делаем — иначе NPC комментируют пару без повода |

---

## Связанные mod-gates (контекст заботы)

HarveyRel-триггеры срабатывают только если **одновременно** активен хотя бы один gate из injury/stress/recovery (тот же набор, что в `triggersNpcReactionsHarveyRel.json`).

Без активной травмы, stress buff или recovery topic bridge `HarveyOverhaul_NPCReaction_HarveyRel_*` **не ставится** — NPC не говорят о паре «из ниоткуда».

---

## Файлы реализации

- `assets/Code/npc_reactions_harvey_relationship.json` — диалоги
- `assets/Code/triggersNpcReactionsHarveyRel.json` — bridge topics
- `assets/Code/triggersNpcReactionsInjuryStress.json` — базовые bridge отключены при Dating/Engaged/Married с Харви
