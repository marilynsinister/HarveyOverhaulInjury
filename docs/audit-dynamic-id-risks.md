# Аудит рисков динамических ID (C# ↔ CP)

Дата: 2026-05-24 (актуализация)  
Область: `HarveyOverhaulInjury` (C#) ↔ `HarveyOverhaul [CP]`.

## Как читать

- **CP** = ключ в `Characters/Dialogue/Harvey` или `Data/Mail`.
- **C# only** = ID в коде, CP-ключа нет.
- **Dead CP** = ключ в CP, C# не ставит topic.
- Риск: **HIGH** = игрок не видит текст; **MEDIUM** = частично; **LOW** = косметика.

---

## Сводка по шаблонам (актуально)

| Шаблон | Статус | Остаток |
|--------|--------|---------|
| `TopicIds.GetInjuryTopic` | ✅ 14/14 в CP | — |
| `TopicIds.GetTreatmentTopic` (11) | ✅ все в CP | — |
| `TopicIds.GetCuredTopic` | ✅ incl. Cold, Surgical | — |
| `TopicIds.GetPhaseTopicId` | ✅ alias keys в block1 | LOW: фазы 2–3 не AddTopic |
| `PhaseTransition_*` | ✅ OK | — |
| `MailIds.*` send | ✅ 7/7 в CP | MEDIUM: WetCare не wired |
| Event launcher topics | ✅ wired в C# | MEDIUM: minor rescue dialogue |

---

## Исправлено (2026-05-23 — 2026-05-24)

| Риск | Решение |
|------|---------|
| Neglect mail ID | C# → `HarveyMod_NeglectWarning` |
| 4 infection/reminder mail | CP entries + `MailIds` |
| `TopicIds` centralization | `Constants.cs` |
| `topicColdCured`, `topicSurgicalWoundCured` | CP keys + completion list |
| 11× `topicTreatment*` | CP block1 |
| Phase alias block1 | Duplicate keys PhaseHealing/Acute/Recovery |
| `topicHealthDamageSevere`, `topicTooCold`, `topicSurgicalWound` | CP injury |
| Legacy `topicStressRecoveryComplete` checks | Удалены из C# |
| AppliedTriggers one-shot all injuries | **Story one-shot** + **injury cooldown** |
| `topicDiagnosisComplete` orphan | C# `TryAddDiagnosisCompleteTopic` |
| `topicRescueOperation` orphan | `RescueOperationLauncher` |
| Storm `buffStressThunder` | `StormComfortLauncher` |
| Pass-out emergency events | `QueueHospitalEvent` в PassOutHandler |
| Minor mine rescue | `TryTriggerMinorMineRescue` |

---

## Остаётся открытым

| # | Проблема | Сторона | Риск |
|---|----------|---------|------|
| 1 | `MailIds.WetCare` / `WetStitchesCare` — send не wired | C# | MEDIUM |
| 2 | `topicHealthDamageCritical/Severe`, `PostOperativeCare` — не Remove при recovery | C# | MEDIUM |
| 3 | `topicHarvey_PainFlare` — buff/topic не в gameplay (только debug) | C# | MEDIUM |
| 4 | `topicHarveyMinorMineRescue` — нет CP dialogue key | CP | MEDIUM |
| 5 | Фазы 2–3 topic keys — C# не AddTopic (PhaseTransition only) | C# design | LOW |
| 6 | `mailHarvey_Neglect` legacy дубль в CP | CP cleanup | LOW |
| 7 | Memory topics (16) — не wired | CP/C# | LOW (задел) |
| 8 | Stress module (`dialoguesHarveyStress.json` off) | CP | LOW (задел) |

---

## Матрица HIGH-priority — статус

| # | Проблема | Статус |
|---|----------|--------|
| 1 | 11× `topicTreatment*` | ✅ CP |
| 2 | `topicSurgicalWound` base | ✅ CP |
| 3 | `topicSurgicalWoundCured` | ✅ CP (Healed удалён) |
| 4 | `topicColdCured` + Cold phases | ✅ CP |
| 5 | `topicHealthDamageSevere` | ✅ CP |
| 6 | Neglect mail ID | ✅ C# + CP |
| 7 | 4 infection/reminder mail | ✅ CP |
| 8 | WetCare MailIds | ⚠️ CP есть, C# не send |
| 9 | Phase alias block1 | ✅ CP |

---

## Рекомендуемая архитектура (без изменений)

1. **`TopicIds` / `ConversationTopics`** — единственное место Replace. ✅
2. **CP:** exact key для каждого C# ID. ✅ (кроме event-only)
3. **Mail:** все send через `MailIds`. ✅
4. **Генератор:** `tmpMap/final_validation_topics_mail.py` + SMAPI `injury_audit_content`.

**C# обновлён 2026-05-23; CP sync 2026-05-23; launchers 2026-05-24.**
