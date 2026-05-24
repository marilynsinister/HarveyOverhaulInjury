# C# InjuryCare: запуск и мосты к CP

**Автоген 2026-05-24**

## startEvent / eventsSeen / Load Data/Events

| Механизм | Где | Детали |
|---|---|---|
| `TriggerEventByName` / `TryStartLocationEvent` | PassOutHandler.cs | Mine rescue; hospital pass-out; minor mine rescue |
| `QueueHospitalEvent` | PassOutHandler.cs | `eventHarveyEmergencyCare`, `eventHarveyExhaustion` → Hospital |
| `TryTriggerMinorMineRescue` | PassOutHandler.cs ← PlayerEventHandler | `eventHarveyMinorMineRescue` при injury без Severe |
| `TriggerMineRescueEvents()` | GameEventHandler.OnDayStarted → PassOutHandler | Severe mine combat death + dating |
| `StormComfortLauncher` | TimeEventHandler | Daily roll → `buffStressThunder` / `topicHarveyStormStress` |
| `RescueOperationLauncher` | PlayerEventHandler | `topicRescueOperation` после E5 / storm comfort |

## Topics, которые C# выставляет как мост к CP-событиям

| Topic | Где выставляется | CP-события, ожидающие topic |
|---|---|---|
| `HarveyMod_CD_RescueOperation` | EventHandlers/RescueOperationLauncher.cs | — |
| `HarveyMod_CD_StormComfort` | EventHandlers/StormComfortLauncher.cs | — |
| `situationReaction_Drunk` | EventHandlers\PlayerEventHandler.cs | — |
| `topicAgreedCheckup` | — | eventHarveyCheckup (Hospital) |
| `topicDiagnosisComplete` | Managers/DialogueManager.cs (TryAddDiagnosisCompleteTopic) | HarveyMod_TreatmentPlanMeeting (Hospital) |
| `topicFirstMeeting` | — | eventHarveyFirstMeeting (BusStop), eventHarveyFirstVisit (Farm), eventHarveyFirstMeeting (BusStop) |
| `topicHarveyExhaustion` | EventHandlers/PassOutHandler.cs (fallback) | — |
| `topicHarveyMandatoryCheckup` | — | eventHarveyMorningCheckup (Farm) |
| `topicHarveyMinorMineRescue` | EventHandlers/PassOutHandler.cs | — |
| `topicHarveyStormStress` | EventHandlers/StormComfortLauncher.cs | — |
| `topicHarvey_NightRound` | EventHandlers\TimeEventHandler.cs | — |
| `topicPassedOutInTown` | — | eventHarveyCheckFarmerOutsideAfter22 (Farm) |
| `topicRescueOperation` | EventHandlers/RescueOperationLauncher.cs | eventRescueOperation (Woods) |

## Mail из C#
| Mail | Где | CP-события |
|---|---|---|
| `DirtyWoundInfection` | Managers\ComplicationManager.cs | — |
| `MineForbidden` | EventHandlers\GameEventHandler.cs | — |
| `NeglectWarning` | Managers\ComplicationManager.cs | — |
| `SleepControl` | EventHandlers\PassOutHandler.cs | — |
| `TreatmentFinalWarning` | Managers\ComplicationManager.cs | — |
| `TreatmentUrgentReminder` | Managers\ComplicationManager.cs | — |
| `WetBandageInfection` | Managers\ComplicationManager.cs | — |
| `mailHarveyMineForbidden` | — | — |
| `mailHarveySleepControl` | — | — |