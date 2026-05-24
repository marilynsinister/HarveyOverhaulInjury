# Технический аудит CP preconditions — события Харви

Cross-ref: `events.json`, `eventsCare.json`, `eventsMineRescue.json`, `triggersCare.json`, `dialoguesHarvey*.json`, C# InjuryCare.

**Актуализация:** 2026-05-23 (после правок gates/split/C# topics).

Проверено событий: **47** | Уникальных event ID: **47** (без дубля FirstMeeting)

> **О структуре файла:** ниже — **ручная сводка** (CRITICAL/HIGH, то что скрипт не ловит или даёт шум).  
> Полный автоматический дамп по 8 категориям — в [cp-preconditions-audit-appendix.md](cp-preconditions-audit-appendix.md) (перегенерация: `python tmpMap/audit_cp_preconditions.py`).

---

## Executive summary (ручная верификация)

### Исправлено 2026-05-23

| ID | Было | Стало |
|---|---|---|
| **`eventHarveySecondVisit` / `FirstWalk`** | конфликт `!topic*` vs script | outcome-topics `*Agree/Neutral/Refused` + `!seen` + day 7/11 |
| **`HarveyMod_NightCrisis`** | одна романт. версия | `_Dating` / `_PreDating` |
| **`HarveyMod_BirthdayHospital`** | одна версия | `_Dating` / `_Friend` |
| **`eventHarveyMedicalCheck`** | один текст | pre-dating + `_Dating` |
| **`HarveyMod_FirstTreatment`** | OR по injury-topics | `topicHarveyNeedsFirstTreatment` (C#) |
| **Story E4–E8** | E4 после E2; E7/E8 после E1 | линейная цепочка E(n−1) + CD topics |
| **Mine rescue (C#)** | legacy для всех | Dating/Married only; `topicMineRescuePending` |

### CRITICAL — недостижимо / сломан gate (открыто)

| ID | Условие | Проблема |
|---|---|---|
| **`buffStressThunder`** | `PLAYER_HAS_BUFF` в storm comfort ×6 | Buff в `buffsStress.json`, но **не ставится**: `triggersStress.json` отключён в `content.json`, C# не ставит. → storm comfort **мертвы**. |
| **`topicDiagnosisComplete`** | gate `HarveyMod_TreatmentPlanMeeting` | Topic **только remove** в script, **add нигде нет** в CP/C#/triggers. |
| **`eventHarveyCheckup`** | `Data/Events/BusStop` + coords Hospital | Осмотр проверяется на BusStop, script — клиника. |

### HIGH

- Orphan CP keys: `eventHarveyEmergencyCare`, `Exhaustion`, `TreatmentCollapse`, `eventStayInHospital`
- `eventHarveyRoomCheckup2` — BETAS + Random 0.2
- Дубль `eventHarveyFirstMeeting` (`events.json` + `eventsCare.json`)

### Авто-скрипт: известные ограничения

| Категория в Appendix | Почему «0 проблем» может врать |
|---|---|
| §1–2 topics не существуют | **Не проверяет buffs** (`buffStressThunder`); `topicDiagnosisComplete` может быть в dialogue keys без add |
| §7 романтика (27 шт.) | Много **false positive** из `$l` portrait token — см. отфильтрованный список ниже |
| §3 topics unused (12 шт.) | Часто **OK** — post-scene dialogue / `HarveyMod_CD_*` |

### Тон vs gate (отфильтовано, без `$l` false positives)

| Event | Gate | Замечание |
|---|---|---|
| `HarveyMod_FirstTreatment` | 3♥ + C# topic | Тон med после правок | ✅ |
| `HarveyMod_NightCrisis_*` | Split Dating / PreDating | — | ✅ |
| `HarveyMod_TreatmentPlanMeeting` | 2♥ + **мертвый topic** | + «солнышко» в script | ❌ orphan topic |
| `eventHarveyFirstMeeting` | !seen | Опекунски для незнакомца (тон) |
| `HarveyOverhaulStory.E6/E7` | 7–8♥ + Story chain | **OK** после правок |

### Минимальные правки (приоритет)

| Приор. | Объект | Правка |
|---|---|---|
| CRITICAL | `eventHarveyCheckup` | `Target: Hospital` |
| CRITICAL | `buffStressThunder` | CP trigger или C# при storm |
| CRITICAL | `topicDiagnosisComplete` | trigger/quest add topic |
| HIGH | orphan hospital events | triggersCare / C# bridge |
| MED | storm comfort ×6 | `!seen` + CD topic |

Полные списки по каждому event ID — в [cp-preconditions-audit-appendix.md](cp-preconditions-audit-appendix.md).
