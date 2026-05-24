# Индекс по локациям Data/Events/*

Подключённые CP-файлы. Fork-подсобытия не включены.

**Автоген из CP** (2026-05-24).


| Location | Event IDs (count) |
|---|---|
| **ArchaeologyHouse** | `HarveyOverhaulStory.E8_QuietShelf` (1) |
| **Beach** | `HarveyOverhaulStory.E4_PierBreath`, `eventHarveyPropose` (2) |
| **BusStop** | `HarveyOverhaulStory.E1_SlipperyPath`, `eventHarveyFirstMeeting` (2) |
| **Farm** | `eventHarveyCheckFarmerOutsideAfter22`, `eventHarveyCheckHealthFarmer`, `eventHarveyFirstVisit`, `eventHarveyFirstWalk`, `eventHarveyMorningCheckup`, `eventHarveySecondVisit` (6) |
| **Forest** | `HarveyOverhaulStory.E3B_WingPatient`, `HarveyOverhaulStory.E3_ForestApothecary`, `eventHarveyFirstDate` (3) |
| **HarveyRoom** | `eventHarveyRoomCheckup`, `eventHarveyRoomCheckup2` (2) |
| **Hospital** | `HarveyMod_BirthdayHospital_Dating`, `HarveyMod_BirthdayHospital_Friend`, `HarveyMod_FirstTreatment`, `HarveyMod_NightCrisis_Dating`, `HarveyMod_NightCrisis_PreDating`, `HarveyMod_TreatmentPlanMeeting`, `HarveyOverhaulStory.E2_InsistentExam`, `HarveyOverhaulStory.E5_StormBeside`, `HarveyOverhaulStory.E6_SayItOutLoud`, `eventHarveyCareMovementAnimationTest`, `eventHarveyCheckup`, `eventHarveyEmergencyCare`, `eventHarveyExhaustion`, `eventHarveyMedicalCheck`, `eventHarveyMedicalCheck_Dating`, `eventHarveyTraumaExam`, `eventHarveyTreatmentCollapse`, `eventStayInHospital` (18) |
| **Mine** | `eventHarveyMineInterception`, `eventHarveyMineRescue`, `eventHarveyMineRescueDating`, `eventHarveyMinorMineRescue` (4) |
| **Mountain** | `HarveyOverhaulStory.E4B_TooQuiet`, `eventHarveyMountainDate` (2) |
| **SkullCave** | `eventHarveySkullCavePrevention` (1) |
| **Town** | `HarveyOverhaulStory.E2B_QuietAgreement`, `HarveyOverhaulStory.E7_TownSip_Sunny`, `HarveyOverhaulStory.E9_LightInWindow`, `eventHarveyLateNightCollapse` (4) |
| **Woods** | `eventRescueOperation` (1) |

**Итого:** 46 уникальных custom event ID в 12 локациях.

## Файлы-источники

| Файл | Локации |
|---|---|
| `events.json` | Farm, Hospital, SeedShop, Woods, Mountain, Custom_AdventurerSummit, Town, BusStop, HarveyRoom, Forest, Beach, ArchaeologyHouse, Desert, Mine |
| `eventsCare.json` | Farm, Hospital, BusStop, SkullCave, Mine |
| `eventsMineRescue.json` | Mine (+ `Data/Mail` mailHarveyAfterMineRescue) |

## Не подключено (content.json)

`events_for_mode_new_formatted.json` — Forest, Hospital, Farm (MyMod_* IDs).