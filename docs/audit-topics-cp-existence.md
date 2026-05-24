# Аудит наличия CP-диалогов для C# conversation topics

Дата: 2026-05-24 (актуализация)  
Источник C#: [audit-topics-csharp.md](./audit-topics-csharp.md)  
CP: `HarveyOverhaul [CP]` → `Characters/Dialogue/Harvey`.

**Подключённые dialogue-файлы:** `dialoguesHarvey.json`, `dialoguesHarveyCare.json`, `dialoguesHarveyCure.json`, `dialoguesHarveyCureStress.json`, `dialoguesHarveyInjury.json`, `dialoguesHarveyPregnant.json`.  
**Не подключён:** `dialoguesHarveyStress.json`.

---

## Сводка

| Категория | C# ID | CP exact match | HIGH gaps |
|-----------|-------|----------------|-----------|
| Базовые травмы | 14 | **14** | 0 |
| Сопутствующие | 3 | **3** | 0 |
| `topicTreatment*` | 11 | **11** | 0 |
| Фазовые `topic*Phase*` | 33 | **33** | 0 (alias keys в block1) |
| `topic*Cured` | 14 | **14** | 0 |
| Осложнения | 6 | **6** | 0 |
| Обморок / env | 5 | **5** | 0 |
| Event-only (dialogue не нужен) | 4 | — | 0 |
| Dialogue gaps (MEDIUM) | 1 | 0 | `topicHarveyMinorMineRescue` |

**Итог:** все HIGH-priority topic ID, для которых C# ожидает реплику Харви при разговоре, **имеют CP keys** (добавлены 2026-05-23 + medical/tone правки).

---

## CP keys без dialogue (by design)

| Topic ID | Кто ставит | Почему OK |
|----------|------------|-----------|
| `topicHarveyNeedsFirstTreatment` | C# | Триггер CP event, не dialogue |
| `topicFirstTreatmentComplete` | CP event | C# check-only |
| `topicDiagnosisComplete` | C# | Триггер `HarveyMod_TreatmentPlanMeeting` |
| `topicRescueOperation` | C# launcher | Триггер `eventRescueOperation` |
| `topicMineRescuePending` | C# | Блокирующий (1 д), реплика не нужна |
| `topicHarveyStormStress` | C# launcher | Gate storm events; реплика в cutscene |

---

## Открытые задачи (MEDIUM / LOW)

| ID | Проблема | Действие |
|----|----------|----------|
| `topicHarveyMinorMineRescue` | C# add после minor rescue; **нет dialogue key** | Добавить в `dialoguesHarveyInjury.json` |
| `topicSurgicalWoundHealed` | Legacy key удалён из CP | C# использует `topicSurgicalWoundCured` ✓ |
| `Recovery_Complete_*` | C# filter, ключей нет | Добавить или убрать filter |
| `topicHarvey_ForcedHospitalization` | C# inline hosp, не AddTopic | OK как optional dialogue |

---

## Топ исправлений (статус)

1. ~~11× `topicTreatment*`~~ — **✅ done**
2. ~~Cold phase + cured~~ — **✅ done**
3. ~~Surgical base + cured~~ — **✅ done**
4. ~~HealthDamageSevere, TooCold, MineInjuryRescue, PainFlare~~ — **✅ done**
5. ~~Phase alias block1~~ — **✅ done**
6. **MEDIUM:** `topicHarveyMinorMineRescue` dialogue — **открыто**

---

## Методология

- Скрипт: `tmpMap/final_validation_topics_mail.py`
- Exact match ключей в Include-файлах
- Event-only topics: precondition в `events.json`, dialogue key не обязателен
- Последняя валидация: 2026-05-24 — **3 missing** = event-only topics без dialogue (ожидаемо)
