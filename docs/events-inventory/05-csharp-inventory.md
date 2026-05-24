# C# InjuryCare: запуск событий и мосты к CP

Мод **HarveyOverhaulInjury** (C#) напрямую запускает **mine rescue**, **hospital pass-out cutscenes** и **minor mine rescue**. Остальные CP-события — vanilla entry (локация + preconditions) или **SpaceCore PlayEvent** из `triggersCare.json`. Storm comfort и rescue operation используют C# **topic/buff gates**, CP играет cutscene при входе в локацию.

**Актуализация:** 2026-05-24

---

## 1. Прямой запуск событий (`startEvent`)

| Метод | Файл | Когда | Event IDs | Location |
|---|---|---|---|---|
| `TriggerEventByName()` | `PassOutHandler.cs` | После warp в Mine (`OnPlayerWarped`) | `eventHarveyMineRescue`, `eventHarveyMineRescueDating` | Mine |
| `TryStartLocationEvent()` | `PassOutHandler.cs` | Minor rescue / hospital resume | `eventHarveyMinorMineRescue`, `eventHarveyEmergencyCare`, `eventHarveyExhaustion` | Mine / Hospital |
| `QueueHospitalEvent()` | `PassOutHandler.cs` | Critical pass-out / exhaustion вне шахты | `eventHarveyEmergencyCare`, `eventHarveyExhaustion` | Hospital (warp → startEvent) |
| `TryTriggerMinorMineRescue()` | `PassOutHandler.cs` ← `PlayerEventHandler` (вход в Mine) | Injury без Severe | `eventHarveyMinorMineRescue` | Mine |
| `TriggerMineRescueEvents()` | `PassOutHandler.cs` ← `GameEventHandler.OnDayStarted` | Утро после боевой смерти в шахте | severe dating rescue | телепорт → Mine |

**Цепочка mine rescue (severe):**

1. `OnUpdateTicked` / `TrackPassOut` — `NeedsMineRescueEvent`, `PassedOutInMineYesterday`, `ApplyBadlyHurtFromMinePassOut()`
2. `OnDayStarted` → `TriggerMineRescueEvents()` — `eventHarveyMineRescueDating` (dating/married)
3. Warp Mine (17,7) → `OnPlayerWarped` → `Load Data/Events/Mine` → `startEvent`
4. Fallback: `RunMineRescueFallback()` — topic + warp Hospital

**Цепочка hospital pass-out:**

1. `OnPlayerWarped` после сна — critical health ≤10 **вне шахты** или exhaustion (`WasExhausted`)
2. `QueueHospitalEvent` → warp Hospital → `TryStartLocationEvent` в `OnPlayerWarped`
3. Если `eventsSeen` — только fallback (topic/HUD); pending resume через `PendingHospitalPassOutEventId`

**Minor mine rescue:**

- `PlayerEventHandler` при входе в Mine с injury buff, **без** Severe → `TryTriggerMinorMineRescue()`
- Cooldown: `LastMinorMineRescueDay`; seen → skip cutscene
- Fallback topic: `topicHarveyMinorMineRescue`

**`eventsSeen`:** добавляется в `onEventFinished` (mine rescue fix 2025-05). Риск рассинхрона с CP `!PLAYER_HAS_SEEN_EVENT` снижен.

**`Load Data/Events/...`:** `Data/Events/Mine`, `Data/Events/Hospital` (pass-out cutscenes).

---

## 2. Topics как мост C# → CP

| Topic | Кто выставляет (C#) | CP-события, ожидающие topic | Примечание |
|---|---|---|---|
| `topicPassedOutInTown` | `PassOutHandler` | `eventHarveyCheckFarmerOutsideAfter22` | ✅ |
| `topicFarmerExhausted` | `PassOutHandler` fallback | — (cutscene через `QueueHospitalEvent`) | ✅ exhaustion wired |
| `topicMineInjuryRescue` | mine rescue event / fallback | forced hosp | ✅ |
| `topicHarveyMinorMineRescue` | minor rescue fallback | dialogues | ✅ |
| `topicDiagnosisComplete` | `DialogueManager.TryAddDiagnosisCompleteTopic` | `HarveyMod_TreatmentPlanMeeting` | ✅ wired 2026-05-24 |
| `topicRescueOperation` | `RescueOperationLauncher` после E5 / storm comfort | `eventRescueOperation` | ✅ wired 2026-05-24 |
| `topicHarveyStormStress` / `buffStressThunder` | `StormComfortLauncher` (daily roll) | `eventHarveyStormComfort*` | ✅ buff gate wired |
| `HarveyMod_CD_StormComfort` | после storm comfort event | cooldown | ✅ |
| `HarveyMod_CD_RescueOperation` | после rescue event | cooldown | ✅ |
| Injury topics `topicHurt`, … | `InjuryManager` | dialogues, triggersCare | dialogue bridge |

**Topics только из CP (C# не выставляет):**

- `topicFirstMeeting`, `topicAgreedCheckup` — BusStop care chain
- `topicHarveySecondVisit`, `topicHarveyFirstVisit` — Farm visits
- `HarveyMod_CD_*` (story E1–E8) — script bridge в CP events

---

## 3. Mail как мост C# → CP

| Mail | C# | CP consumer |
|---|---|---|
| `mailHarveySleepControl` | `PassOutHandler` | dialogues |
| `mailHarveyMineForbidden` | `GameEventHandler.OnDayEnding` | debuff (C#) |
| `HarveyMod_*` neglect/infection | `ComplicationManager` | mail entries + dialogues |
| `mailHarveyMedicalCheckReminder` | triggersCare (CP) | `eventHarveyMedicalCheck` |
| `mailHarveyAfterMineRescue` | CP script | post-rescue |

---

## 4. SpaceCore PlayEvent (CP triggers, не C#)

Из `assets/Code/triggersCare.json`:

| Trigger ID | Event | Условие |
|---|---|---|
| `triggerHarveySkullCaveWarning` | `eventHarveySkullCavePrevention` | dating + injury buffs (Condition битый: Mine+SkullCave) |
| `triggerHarveyMineWarning` | `eventHarveyMineInterception` | dating + base injury buffs + Mine |
| `triggerLocationReactionSkullCaveExit` | `eventHarveySkullCavePrevention` | SkullCave exit + dating |

---

## 5. C# launchers vs orphan CP scripts

| CP event | C# launcher / альтернатива |
|---|---|
| `eventHarveyEmergencyCare` | ✅ `QueueHospitalEvent` (critical pass-out) |
| `eventHarveyExhaustion` | ✅ `QueueHospitalEvent` (exhaustion pass-out) |
| `eventHarveyMinorMineRescue` | ✅ `TryTriggerMinorMineRescue` |
| `eventHarveyStormComfort*` | ✅ `StormComfortLauncher` → buff/topic → vanilla entry |
| `eventRescueOperation` | ✅ `RescueOperationLauncher` → topic → vanilla entry |
| `eventStayInHospital` | `HospitalizationManager` — dialogue block (orphan script) |
| `eventHarveyTreatmentCollapse` | orphan — нет caller |

---

## 6. Сводка: event ID в C#

```
eventHarveyMineRescue          PassOutHandler.cs (legacy fallback)
eventHarveyMineRescueDating    PassOutHandler.cs
eventHarveyMinorMineRescue     PassOutHandler.cs, PlayerEventHandler.cs
eventHarveyEmergencyCare       PassOutHandler.cs (QueueHospitalEvent)
eventHarveyExhaustion          PassOutHandler.cs (QueueHospitalEvent)
eventRescueOperation           RescueOperationLauncher.cs (topic only; CP plays event)
eventHarveyStormComfort*       StormComfortLauncher.cs (IsStormComfortEventId)
```

---

## 7. State flags (не events, но связаны)

| Flag / state | Файл | Назначение |
|---|---|---|
| `NeedsMineRescueEvent` | PassOutHandler | очередь severe mine rescue |
| `PendingHospitalPassOutEventId` | PassOutHandler | resume hospital cutscene |
| `PendingMinorMineRescueEventId` | PassOutHandler | resume minor rescue |
| `LastStormComfortRollDay` / `LastStormComfortEventDay` | StormComfortLauncher | 1 roll / 1 event per day |
| `LastMinorMineRescueDay` | PassOutHandler | minor rescue cooldown |
| `MineWarningDay` | GameEventHandler | mail forbidden chain |
