# Разрывы: C# ↔ CP

## События упомянуты в C#, но отсутствуют в подключённых CP-файлах

- `eventHarveyStormComfort` — refs: Core\Constants.cs

## CP-события без C# и без SpaceCore trigger (только vanilla preconditions)

- `HarveyMod_BirthdayHospital_Dating` @ Hospital — pre: `Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery `
- `HarveyMod_BirthdayHospital_Friend` @ Hospital — pre: `Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery `
- `HarveyMod_FirstTreatment` @ Hospital — pre: `Friendship Harvey 750/Time 900 2100/GameStateQuery !PLAYER_HAS_SEEN_EVENT Curren`
- `HarveyMod_NightCrisis_Dating` @ Hospital — pre: `Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Curre`
- `HarveyMod_NightCrisis_PreDating` @ Hospital — pre: `Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Curre`
- `HarveyMod_TreatmentPlanMeeting` @ Hospital — pre: `Time 900 1700/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicDiagnosi`
- `HarveyOverhaulStory.E1_SlipperyPath` @ BusStop — pre: `Time 700 1400/Weather Wind/!FestivalDay/Friendship Harvey 500/GameStateQuery !PL`
- `HarveyOverhaulStory.E2B_QuietAgreement` @ Town — pre: `Time 1000 1600/Weather Sunny/!FestivalDay/Friendship Harvey 750/GameStateQuery P`
- `HarveyOverhaulStory.E2B_QuietAgreement` @ Town — pre: `Time 1000 1600/Weather Wind/!FestivalDay/Friendship Harvey 750/GameStateQuery PL`
- `HarveyOverhaulStory.E2_InsistentExam` @ Hospital — pre: `Time 0900 1700/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN`
- `HarveyOverhaulStory.E3B_WingPatient` @ Forest — pre: `Time 1200 1800/Weather Sunny/!FestivalDay/Friendship Harvey 1000/GameStateQuery `
- `HarveyOverhaulStory.E3_ForestApothecary` @ Forest — pre: `DayOfWeek Thu Fri Sat/Time 1200 1800/!FestivalDay/Weather Sunny/Friendship Harve`
- `HarveyOverhaulStory.E4B_TooQuiet` @ Mountain — pre: `Time 1800 2200/Weather Sunny/!FestivalDay/Friendship Harvey 1500/GameStateQuery `
- `HarveyOverhaulStory.E4B_TooQuiet` @ Mountain — pre: `Time 1800 2200/Weather Wind/!FestivalDay/Friendship Harvey 1500/GameStateQuery P`
- `HarveyOverhaulStory.E4_PierBreath` @ Beach — pre: `Weather Sunny/Time 1800 2600/!FestivalDay/Friendship Harvey 1250/GameStateQuery `
- `HarveyOverhaulStory.E5_StormBeside` @ Hospital — pre: `Weather Storm/Time 1400 2000/!FestivalDay/Friendship Harvey 1500/GameStateQuery `
- `HarveyOverhaulStory.E6_SayItOutLoud` @ Hospital — pre: `Time 1900 2330/!FestivalDay/Friendship Harvey 1750/GameStateQuery PLAYER_HAS_SEE`
- `HarveyOverhaulStory.E7_TownSip_Sunny` @ Town — pre: `Weather Sunny/Time 1200 1500/!FestivalDay/Friendship Harvey 2000/GameStateQuery `
- `HarveyOverhaulStory.E8_QuietShelf` @ ArchaeologyHouse — pre: `DayOfWeek Sat/Time 1000 1600/!FestivalDay/Friendship Harvey 2000/GameStateQuery `
- `HarveyOverhaulStory.E9_LightInWindow` @ Town — pre: `Time 2000 2330/!FestivalDay/Friendship Harvey 2250/GameStateQuery PLAYER_HAS_SEE`
- `eventHarveyCareMovementAnimationTest` @ Hospital — pre: `Time 900 1700/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyCareMovem`
- `eventHarveyCheckFarmerOutsideAfter22` @ Farm — pre: `Time 2200 0200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicPassedO`
- `eventHarveyCheckHealthFarmer` @ Farm — pre: `Time 600 1200/GameStateQuery PLAYER_HAS_SEEN_EVENT Current PlayerKilled/GameStat`
- `eventHarveyCheckup` @ Hospital — pre: `Time 1400 1600/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicAgreedC`
- `eventHarveyFirstDate` @ Forest — pre: `Weather Sunny/Friendship Harvey 2000/Time 1800 2600/GameStateQuery PLAYER_NPC_RE`
- `eventHarveyFirstMeeting` @ BusStop — pre: `Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMee`
- `eventHarveyFirstMeeting` @ BusStop — pre: `Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMee`
- `eventHarveyFirstVisit` @ Farm — pre: `Time 600 1200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMee`
- `eventHarveyFirstWalk` @ Farm — pre: `Time 600 1200/Weather Sunny/GameStateQuery DAYS_PLAYED 11/GameStateQuery !PLAYER`
- `eventHarveyLateNightCollapse` @ Town — pre: `Time 2400 2600`
- `eventHarveyMedicalCheck` @ Hospital — pre: `Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MA`
- `eventHarveyMedicalCheck_Dating` @ Hospital — pre: `Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MA`
- `eventHarveyMorningCheckup` @ Farm — pre: `Time 0600 0800/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyM`
- `eventHarveyMountainDate` @ Mountain — pre: `Weather sunny/Time 900 1200/Friendship Harvey 2250/GameStateQuery PLAYER_NPC_REL`
- `eventHarveyPropose` @ Beach — pre: `Weather Sunny/Friendship Harvey 2500/Time 1800 2600/GameStateQuery PLAYER_NPC_RE`
- `eventHarveyRoomCheckup` @ HarveyRoom — pre: `Friendship Harvey 1500`
- `eventHarveyRoomCheckup2` @ HarveyRoom — pre: `GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating/GameStateQuery Spi`
- `eventHarveySecondVisit` @ Farm — pre: `Time 600 1200/GameStateQuery DAYS_PLAYED 7/GameStateQuery !PLAYER_HAS_CONVERSATI`
- `eventHarveyTraumaExam` @ Hospital — pre: `Time 0800 1800/Friendship Harvey 2000`
- `eventHarveyTreatmentCollapse` @ Hospital — pre: ``
- `eventStayInHospital` @ Hospital — pre: ``

## CP-события с риском недостижимости

- `HarveyMod_BirthdayHospital_Dating` @ Hospital: C# не выставляет topic: topicBirthdayHospitalComplete
- `HarveyMod_BirthdayHospital_Friend` @ Hospital: C# не выставляет topic: topicBirthdayHospitalComplete
- `HarveyMod_FirstTreatment` @ Hospital: C# не выставляет topic: topicFirstTreatmentComplete, topicHarveyNeedsFirstTreatment
- `HarveyMod_NightCrisis_Dating` @ Hospital: C# не выставляет topic: topicNightCrisisComplete
- `HarveyMod_NightCrisis_PreDating` @ Hospital: C# не выставляет topic: topicNightCrisisComplete
- `HarveyOverhaulStory.E1_SlipperyPath` @ BusStop: C# не выставляет topic: HarveyMod_CD_Global
- `HarveyOverhaulStory.E2B_QuietAgreement` @ Town: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E2
- `HarveyOverhaulStory.E2_InsistentExam` @ Hospital: C# не выставляет topic: HarveyMod_CD_Global
- `HarveyOverhaulStory.E3B_WingPatient` @ Forest: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E3
- `HarveyOverhaulStory.E3_ForestApothecary` @ Forest: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E2
- `HarveyOverhaulStory.E4B_TooQuiet` @ Mountain: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E4
- `HarveyOverhaulStory.E4_PierBreath` @ Beach: C# не выставляет topic: HarveyMod_CD_Global
- `HarveyOverhaulStory.E5_StormBeside` @ Hospital: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E4B
- `HarveyOverhaulStory.E6_SayItOutLoud` @ Hospital: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E5
- `HarveyOverhaulStory.E7_TownSip_Sunny` @ Town: C# не выставляет topic: HarveyMod_CD_Global
- `HarveyOverhaulStory.E8_QuietShelf` @ ArchaeologyHouse: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E7
- `HarveyOverhaulStory.E9_LightInWindow` @ Town: C# не выставляет topic: HarveyMod_CD_Global, HarveyMod_CD_E8
- `eventHarveyCheckFarmerOutsideAfter22` @ Farm: C# не выставляет topic: topicPassedOutInTown
- `eventHarveyCheckup` @ Hospital: C# не выставляет topic: topicAgreedCheckup
- `eventHarveyFirstMeeting` @ BusStop: duplicate across events.json, eventsCare.json; дубль events.json + eventsCare.json + BusStop; C# не выставляет topic: topicFirstMeeting
- `eventHarveyFirstVisit` @ Farm: C# не выставляет topic: topicFirstMeeting
- `eventHarveyFirstWalk` @ Farm: C# не выставляет topic: topicHarveySecondVisitAgree, topicHarveySecondVisitNeutral, topicHarveySecondVisitRefused
- `eventHarveyMineRescue` @ Mine: C# добавляет eventsSeen до vanilla — возможен double-mark
- `eventHarveyMineRescueDating` @ Mine: C# добавляет eventsSeen до vanilla — возможен double-mark
- `eventHarveyMorningCheckup` @ Farm: C# не выставляет topic: topicHarveyMandatoryCheckup
- `eventHarveyRoomCheckup2` @ HarveyRoom: требует BETAS mod
- `eventHarveySecondVisit` @ Farm: C# не выставляет topic: topicHarveyFirstVisitAgree, topicHarveyFirstVisitNeutral, topicHarveyFirstVisitRefused
- `eventHarveyTreatmentCollapse` @ Hospital: нет C# startEvent — только CP/trigger или недостижимо
- `eventStayInHospital` @ Hospital: нет C# startEvent — только CP/trigger или недостижимо

## Файлы вне content.json

- `MyMod_HarveyStormComfortForest` @ Forest in `events_for_mode_new_formatted.json` — **не загружается**
- `MyMod_HarveyStressTiredCheck` @ Hospital in `events_for_mode_new_formatted.json` — **не загружается**
- `MyMod_HarveyUrgentFarmVisit` @ Farm in `events_for_mode_new_formatted.json` — **не загружается**

## Дубликаты event ID

- `HarveyOverhaulStory.E2B_QuietAgreement`: Town (events.json), Town (events.json)
- `HarveyOverhaulStory.E4B_TooQuiet`: Mountain (events.json), Mountain (events.json)
- `eventHarveyFirstMeeting`: BusStop (events.json), BusStop (eventsCare.json)