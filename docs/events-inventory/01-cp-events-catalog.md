# Content Patcher: каталог Data/Events/*


## Data/Events/ArchaeologyHouse

### `HarveyOverhaulStory.E8_QuietShelf/DayOfWeek Sat/Time 1000 1600/!FestivalDay/Friendship Harvey 2000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E7_TownSip_Sunny/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E8_QuietShelf/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E7`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E8_QuietShelf`
- **Preconditions:** `DayOfWeek Sat/Time 1000 1600/!FestivalDay/Friendship Harvey 2000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E7_TownSip_Sunny/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E8_QuietShelf/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E7`
- **Commands:** AddConversationTopic, addConversationTopic, friendship
- **Bridge (mail/topic):** Current, HarveyMod_CD_E8, HarveyMod_CD_Global, topicHarveyHelp_Asks, topicHarveyHelp_Independent, topicHarveyHelp_Spotter
- **Script preview:** `         none/         16 9/         farmer 16 9 2 Gunther 11 9 0 Harvey -1000 -1000 0/         skippable/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E8 2/         pause 200/         faceDirection Gunther 1/         pause 400/         move Gunther 6 0 1/         speak Gunther "Здравствуйте, @. Поможете с карточками каталога?$h#$b#Разложите их по видам — и если достанете коробку с верхней полки, будет проще. Только тихо, пож...`


## Data/Events/Beach

### `HarveyOverhaulStory.E4_PierBreath/Weather Sunny/Time 1800 2600/!FestivalDay/Friendship Harvey 1250/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3B_WingPatient/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E4_PierBreath`
- **Preconditions:** `Weather Sunny/Time 1800 2600/!FestivalDay/Friendship Harvey 1250/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3B_WingPatient/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`
- **Commands:** AddConversationTopic, addConversationTopic
- **Bridge (mail/topic):** Current, HarveyMod_CD_E4, HarveyMod_CD_Global, topicHarveyPierBreath, topicHarveyTrust_BreathHard, topicHarveyTrust_NeedsSpace, topicHarveyTrust_TouchOk
- **Script preview:** `         ocean/         39 23/         farmer 40 17 2 Harvey 39 23 1/         skippable/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E4 3/         ambientLight 110 110 140/         move farmer 0 6 2/         faceDirection Harvey 1/         faceDirection farmer 3/         pause 400/         playSound ocean/         emote farmer 40/         speak Harvey "На воде дыхание честнее. Его труднее обмануть.$0"/         speak Harvey "...`

### `eventHarveyPropose/Weather Sunny/Friendship Harvey 2500/Time 1800 2600/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating/GameStateQuery SEASON Spring Summer Fall`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyPropose`
- **Preconditions:** `Weather Sunny/Friendship Harvey 2500/Time 1800 2600/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating/GameStateQuery SEASON Spring Summer Fall`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `night_market/         40 16/         farmer 40 7 2 Harvey 39 23 2/         temporaryAnimatedSprite LooseSprites\\Cursors 0 1810 87 58 999999 1 999999 41 16 false false 28 0 1 0 0 0 hold_last_frame/         skippable/         move farmer 0 8 2/         move farmer -1 0 2/         move farmer 0 4 2/         move farmer 1 0 2/         move farmer 0 4 2 true/         viewport move 0 3 3500/         speak Harvey "Пришел пораньше... *стоит у воды в расстегнутой рубашке* Чтобы проверить - нет ли медуз ...`


## Data/Events/BusStop

### `HarveyOverhaulStory.E1_SlipperyPath/Time 700 1400/Weather Wind/!FestivalDay/Friendship Harvey 500/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E1_SlipperyPath/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E1_SlipperyPath`
- **Preconditions:** `Time 700 1400/Weather Wind/!FestivalDay/Friendship Harvey 500/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E1_SlipperyPath/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E1, HarveyMod_CD_Global
- **Script preview:** `          none/          52 24/          farmer 20 23 1 Harvey 26 22 3/          setSkipActions AddMail Current HarveyOverhaul_E1_Note tomorrow/          skippable/          pause 400/          playSound sandyStep/          move farmer 3 0 1 true/          move Harvey -3 0 3 true/          proceedPosition farmer/          proceedPosition Harvey/          message "Порыв ветра бросает мокрые листья под ноги."/          faceDirection farmer 2/          animate farmer false true 2500 5 4/          p...`

### `eventHarveyFirstMeeting/Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyFirstMeeting`
- **Preconditions:** `Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting`
- **Commands:** —
- **Bridge (mail/topic):** topicConcernForHealth, topicFirstMeeting
- **Script preview:** `         none/         20 23/          farmer 20 23 1 Harvey 27 23 3/         skippable/         move farmer 3 0 1 true/         move Harvey -3 0 3 true/         emote Harvey 32/         emote farmer 32/         pause 500/         addConversationTopic topicFirstMeeting 7/         speak Harvey "Здравствуйте! Вы... это вы купили старую ферму?$0#$b# *улыбается, но взгляд становится обеспокоенным* Добро пожаловать в долину...$h"/         emote Harvey 8/         pause 400/         speak Harvey "*нере...`

### `eventHarveyFirstMeeting/Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveyFirstMeeting`
- **Preconditions:** `Time 0600 2600/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting`
- **Commands:** —
- **Bridge (mail/topic):** topicConcernForHealth, topicFirstMeeting
- **Script preview:** `                     continue/                     20 23/                      farmer 20 23 1 Harvey 27 23 3/                     skippable/                     move farmer 3 0 1 true/                     move Harvey -3 0 3 true/                     emote Harvey 32/                     emote farmer 32/                     pause 500/                     addConversationTopic topicFirstMeeting 7/                     speak Harvey "Здравствуйте! Вы... это вы купили старую ферму?$0#$b# *улыбается, но ...`


## Data/Events/Farm

### `eventHarveyCheckFarmerOutsideAfter22/Time 2200 0200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicPassedOutInTown/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating Married`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyCheckFarmerOutsideAfter22`
- **Preconditions:** `Time 2200 0200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicPassedOutInTown/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating Married`
- **Commands:** —
- **Bridge (mail/topic):** topicHarveyMandatoryCheckup
- **Script preview:** `         continue/         64 15/         farmer 64 16 2 Harvey 64 18 0/         pause 600/         skippable/         playSound owl/         pause 500/         speak Harvey "@?!$a"/         emote Harvey 12/         pause 400/         playSound grassyStep/         move Harvey 0 -1 0/         pause 300/         speak Harvey "Что ты делаешь на улице в это время?$a#$b#Сейчас же марш в дом!$a"/         pause 500/         emote farmer 12/         pause 600/         quickQuestion Я просто...#Хотела пр...`

### `eventHarveyCheckHealthFarmer/Time 600 1200/GameStateQuery PLAYER_HAS_SEEN_EVENT Current PlayerKilled/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyCheckHealthFarmer`
- **Preconditions:** `Time 600 1200/GameStateQuery PLAYER_HAS_SEEN_EVENT Current PlayerKilled/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `continue/         64 15/         farmer 64 16 2 Harvey 64 18 0/         skippable/         pause 600/         speak Harvey "Ты выглядишь ужасно!$8#$b#*быстро подходит* Давай проверим...$u"/         pause 500/         startJittering/         emote farmer 24/         pause 300/         animate Harvey false false 2000 19/         pause 2000/         speak Harvey "*берёт за запястье* Пульс 110, зрачки расширены... *ощупывает лоб* И температура повышена!$u"/         emote Harvey 28/         pause 400...`

### `eventHarveyFirstVisit/Time 600 1200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstVisit/GameStateQuery DAYS_PLAYED 3`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveyFirstVisit`
- **Preconditions:** `Time 600 1200/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstMeeting/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstVisit/GameStateQuery DAYS_PLAYED 3`
- **Commands:** action
- **Bridge (mail/topic):** topicHarveyFirstVisit, topicHarveyFirstVisitAgree, topicHarveyFirstVisitNeutral, topicHarveyFirstVisitRefused
- **Script preview:** `     continue/     64 15/     farmer 64 16 2 Harvey 64 18 0/     skippable/     pause 600/     speak Harvey "Привет! Как дела? Просто хотел проверить, как ты себя чувствуешь.$h"/     message "Я... я в порядке, спасибо..."/     speak Harvey "Ты выглядишь немного уставшей. Долгая дорога?$s"/     message "Да, немного..."/     speak Harvey "Понимаю. Если тебе понадобится помощь - я всегда рядом.$h#$b#Но только если ты сама захочешь.$l"/     quickQuestion ...#Спасибо за заботу#Я справлюсь сама#Мне ну...`

### `eventHarveyFirstWalk/Time 600 1200/Weather Sunny/GameStateQuery DAYS_PLAYED 11/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitAgree/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitNeutral/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitRefused/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveySecondVisit/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstWalk`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyFirstWalk`
- **Preconditions:** `Time 600 1200/Weather Sunny/GameStateQuery DAYS_PLAYED 11/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitAgree/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitNeutral/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveySecondVisitRefused/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveySecondVisit/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstWalk`
- **Commands:** —
- **Bridge (mail/topic):** topicHarveyDeclineFirstWalk
- **Script preview:** `         continue/         64 15/         farmer 64 16 2 Harvey 64 18 0/         skippable/         pause 600/         speak Harvey "Привет. Я заметил, что ты часто бываешь в лесу.$h#$b#Сегодня хорошая погода для прогулки.$0"/         message "Да... я люблю этот лес."/         speak Harvey "Тогда пойдём вместе — до заката. Свежий воздух полезен.$h"/         question fork0 "#Согласиться#Отказаться"/         fork acceptWalk/         speak Harvey "Понимаю. Но если передумаешь — я свободен до заката...`

### `eventHarveyMorningCheckup/Time 0600 0800/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyMandatoryCheckup/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyMorningCheckup`
- **Preconditions:** `Time 0600 0800/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyMandatoryCheckup/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `         continue/         64 15/         farmer 64 16 2 Harvey 64 17 0/         pause 1000/         skippable/         speak Harvey "@? Проснись, солнышко...$l"/         emote Harvey 20/         pause 800/         speak Harvey "Я принёс завтрак в постель.$h#$b#И витаминный чай для восстановления.$l"/         addItem 201 1/         addItem 614 1/         pause 600/         emote farmer 20/         pause 500/         speak Harvey "Давай проверим твоё состояние...$u#$b#Пульс нормализовался, но орг...`

### `eventHarveySecondVisit/Time 600 1200/GameStateQuery DAYS_PLAYED 7/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitAgree/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitNeutral/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitRefused/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstVisit/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveySecondVisit`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveySecondVisit`
- **Preconditions:** `Time 600 1200/GameStateQuery DAYS_PLAYED 7/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitAgree/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitNeutral/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyFirstVisitRefused/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstVisit/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveySecondVisit`
- **Commands:** action
- **Bridge (mail/topic):** topicHarveySecondVisit, topicHarveySecondVisitAgree, topicHarveySecondVisitNeutral, topicHarveySecondVisitRefused
- **Script preview:** `     continue/     64 15/     farmer 64 16 2 Harvey 64 18 0/     skippable/     pause 600/     speak Harvey "Привет! Я принёс тебе витаминный чай. Он поможет восстановить силы.$h"/     addItem 614 1/     message "Спасибо, но я не болею..."/     speak Harvey "Конечно, ты здорова!$h#$b#Но витамины никогда не помешают, особенно в новом месте.$l"/     message "Харви осторожно кладёт пакетик чая ей в карман."/     speak Harvey "Попробуй, если захочешь. Никаких обязательств.$h"/     quickQuestion ...#...`


## Data/Events/Forest

### `HarveyOverhaulStory.E3B_WingPatient/Time 1200 1800/Weather Sunny/!FestivalDay/Friendship Harvey 1000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3_ForestApothecary/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3B_WingPatient/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E3`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E3B_WingPatient`
- **Preconditions:** `Time 1200 1800/Weather Sunny/!FestivalDay/Friendship Harvey 1000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3_ForestApothecary/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3B_WingPatient/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E3`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E3B, HarveyMod_CD_Global, topicHarveyWingPatient
- **Script preview:** `none/           48 14/           farmer 48 14 1 Harvey 49 14 3/           skippable/           setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E3B 3#AddConversationTopic Current topicHarveyWingPatient 5/           pause 400/           move farmer -2 0 3 true/           move Harvey -2 0 3 true/           proceedPosition farmer/           proceedPosition Harvey/           pause 300/           message "В траве у тропы что-то тихо шевелится...`

### `HarveyOverhaulStory.E3_ForestApothecary/DayOfWeek Thu Fri Sat/Time 1200 1800/!FestivalDay/Weather Sunny/Friendship Harvey 1000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3_ForestApothecary/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E3_ForestApothecary`
- **Preconditions:** `DayOfWeek Thu Fri Sat/Time 1200 1800/!FestivalDay/Weather Sunny/Friendship Harvey 1000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E3_ForestApothecary/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E3, HarveyMod_CD_Global
- **Script preview:** `none/           50 13/           farmer 50 13 1 Harvey 51 13 3/           skippable/           setSkipActions AddMail Current HarveyOverhaul_E3_Reminder tomorrow/           pause 400/           speak Harvey "Вы обещали показать лес. Я буду тихим — обещаю.$h"/           faceDirection Harvey 1/           emote farmer 32/           move farmer -3 0 3 true/           move Harvey -3 0 3 true/           proceedPosition farmer/           proceedPosition Harvey/           pause 300/           message "Т...`

### `eventHarveyFirstDate/Weather Sunny/Friendship Harvey 2000/Time 1800 2600/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating/GameStateQuery SEASON Spring Summer Fall`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyFirstDate`
- **Preconditions:** `Weather Sunny/Friendship Harvey 2000/Time 1800 2600/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating/GameStateQuery SEASON Spring Summer Fall`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `kindadumbautumn/         65 45/         farmer 65 38 2 Harvey 65 46 1/         temporaryAnimatedSprite LooseSprites\\Cursors 0 1810 87 58 999999 1 999999 66 44 false false 5 0 1 0 0 0 hold_last_frame/         skippable/         move farmer 0 7 2/         pause 3000/         speak Harvey "Ты пришла! *нервно поправляет галстук* Я... э-э... подготовил всё для твоего комфорта.$l"/         emote Harvey 32/         faceDirection Harvey 1/         animate Harvey false true 1000 40 41/         speak Har...`


## Data/Events/HarveyRoom

### `eventHarveyRoomCheckup/Friendship Harvey 1500`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyRoomCheckup`
- **Preconditions:** `Friendship Harvey 1500`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `continue/         8 9/         farmer 1000 1000 0 Harvey 9 9 3/         playSound doorClose/         skippable/         warp farmer 6 12/         move farmer 0 -3 1 true/         move Harvey -1 0 3 true/         move farmer 1 0 1 true/         pause 200/         speak Harvey "Плановый медицинский осмотр!$u#$b#@, я заметил, что ты чихала. Это плановый осмотр.$0"/         pause 200/         message "Ты пытаешься сбежать, но Харви останавливает тебя за локоть — мягко, но настойчиво."/         speed...`

### `eventHarveyRoomCheckup2/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating/GameStateQuery Spiderbuttons.BETAS_NPC_LOCATION Harvey HarveyRoom/GameStateQuery RANDOM 0.2`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyRoomCheckup2`
- **Preconditions:** `GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating/GameStateQuery Spiderbuttons.BETAS_NPC_LOCATION Harvey HarveyRoom/GameStateQuery RANDOM 0.2`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `continue/         8 9/         farmer 1000 1000 0 Harvey 9 9 3/         playSound doorClose/         skippable/         warp farmer 6 12/         move farmer 0 -3 1 true/         move Harvey -1 0 3 true/         move farmer 1 0 1 true/         pause 200/         speak Harvey "Плановый осмотр, @! *достаёт стетоскоп* Я заметил ты чихала утром - это тревожно.$a"/         pause 200/         emote Harvey 12/         pause 200/         message "Ты пятишься к двери..."/         faceDirection farmer 1/ ...`


## Data/Events/Hospital

### `HarveyMod_BirthdayHospital_Dating/Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Friend/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicBirthdayHospitalComplete`

- **Файл:** `events.json`
- **Event ID:** `HarveyMod_BirthdayHospital_Dating`
- **Preconditions:** `Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Friend/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicBirthdayHospitalComplete`
- **Commands:** —
- **Bridge (mail/topic):** topicBirthdayHospitalComplete
- **Script preview:** `spring_day_ambient/         10 15/         Harvey 9 14 1 farmer 10 19 0/         skippable/         viewport 10 15 true/         pause 500/         playSound doorOpen/         move farmer 0 -4 0 true/         pause 300/         emote Harvey 20/         speak Harvey "С днём рождения, @! *широко улыбается*$0#$h#Я знаю, ты не хотела праздновать...$l"/         pause 400/         speak Harvey "Но я не мог позволить этому дню пройти незамеченным.$l#$b#*достаёт небольшую коробочку* Это специально для т...`

### `HarveyMod_BirthdayHospital_Friend/Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Friend/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicBirthdayHospitalComplete`

- **Файл:** `events.json`
- **Event ID:** `HarveyMod_BirthdayHospital_Friend`
- **Preconditions:** `Friendship Harvey 2000/LocationName Hospital/Season summer/Day 9/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Friend/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_BirthdayHospital/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicBirthdayHospitalComplete`
- **Commands:** —
- **Bridge (mail/topic):** topicBirthdayHospitalComplete
- **Script preview:** `spring_day_ambient/         10 15/         Harvey 9 14 1 farmer 10 19 0/         skippable/         viewport 10 15 true/         pause 500/         playSound doorOpen/         move farmer 0 -4 0 true/         pause 300/         emote Harvey 20/         speak Harvey "С днём рождения, @.$0#$b#Я подумал, день в больнице не должен быть совсем серым.$h"/         pause 400/         speak Harvey "*достаёт небольшую коробочку* Это маленький подарок. Не как врачебное назначение, обещаю.$h"/         pause...`

### `HarveyMod_FirstTreatment/Friendship Harvey 750/Time 900 2100/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstTreatmentComplete/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyNeedsFirstTreatment`

- **Файл:** `events.json`
- **Event ID:** `HarveyMod_FirstTreatment`
- **Preconditions:** `Friendship Harvey 750/Time 900 2100/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicFirstTreatmentComplete/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarveyNeedsFirstTreatment`
- **Commands:** —
- **Bridge (mail/topic):** topicFirstTreatmentComplete
- **Script preview:** `         Hospital_Ambient/         4 6/         farmer 4 6 0 Harvey 4 5 2/         skippable/         pause 800/         move farmer 1 0 0/         pause 500/         faceDirection Harvey 1/         pause 500/         emote Harvey 28/         pause 500/         speak Harvey "Наконец-то... Я так волновался за тебя.$0#$b#Садись сюда. Нужно провести полное обследование.$u"/,         pause 500/         move farmer 0 -2 2/         showFrame farmer 107/         pause 500/         move Harvey 1 0 0/   ...`

### `HarveyMod_NightCrisis_Dating/Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_PreDating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicNightCrisisComplete`

- **Файл:** `events.json`
- **Event ID:** `HarveyMod_NightCrisis_Dating`
- **Preconditions:** `Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_PreDating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicNightCrisisComplete`
- **Commands:** —
- **Bridge (mail/topic):** topicNightCrisisComplete
- **Script preview:** `         nightTime/         14 8/         Harvey 14 8 3 farmer 10 7 1/         skippable/         pause 800/         playSound doorClose/         move farmer 6 0 1/         faceDirection Harvey 0/         pause 100/         emote Harvey 16/         move farmer 0 1 3/         pause 100/         faceDirection Harvey 1/         pause 400/         speak Harvey "Что?! Ты здесь в такое время?!$8#$b#*подбегает* С тобой всё в порядке? Говоря со мной.$8"/         pause 300/         move Harvey 0 0 1/    ...`

### `HarveyMod_NightCrisis_PreDating/Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_PreDating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicNightCrisisComplete`

- **Файл:** `events.json`
- **Event ID:** `HarveyMod_NightCrisis_PreDating`
- **Preconditions:** `Friendship Harvey 1500/Time 2200 2600/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyMod_FirstTreatment/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_PreDating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis_Dating/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_NightCrisis/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicNightCrisisComplete`
- **Commands:** —
- **Bridge (mail/topic):** topicNightCrisisComplete
- **Script preview:** `         nightTime/         15 8/         Harvey 15 8 3 farmer 10 7 1/         skippable/         pause 800/         playSound doorClose/         move farmer 6 0 1/         faceDirection Harvey 0/         pause 100/         emote Harvey 16/         move farmer 0 1 3/         pause 100/         faceDirection Harvey 1/         pause 400/         speak Harvey "Что?! Ты здесь в такое время?!$8#$b#*быстро подходит* С тобой всё в порядке? Что случилось?$8"/         pause 300/         move Harvey 0 0 1...`

### `HarveyMod_TreatmentPlanMeeting/Time 900 1700/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicDiagnosisComplete/Friendship Harvey 750/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_TreatmentPlanMeeting`

- **Файл:** `events.json`
- **Event ID:** `HarveyMod_TreatmentPlanMeeting`
- **Preconditions:** `Time 900 1700/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicDiagnosisComplete/Friendship Harvey 750/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyMod_TreatmentPlanMeeting`
- **Commands:** AddConversationTopic, addConversationTopic, friendship, action
- **Bridge (mail/topic):** topicDiagnosisComplete, topicIntensiveTreatment, topicTreatmentAgreement, topicTreatmentRefusal
- **Script preview:** `         Hospital_Ambient/         5 7/         farmer 5 5 3 Harvey 4 5 1/         skippable/         pause 800/         speak Harvey "Наконец-то у меня есть полная картина твоего состояния.$u#$b#Садись, обсудим план лечения.$0"/         move farmer 0 -1 2/         showFrame farmer 107/         move Harvey 1 0 0/         pause 500/         speak Harvey "У тебя комплексное расстройство: хронический стресс с элементами тревожности.$a#$b#Лечение будет поэтапным.$0"/         speak Harvey "Этап 1: Ст...`

### `HarveyOverhaulStory.E2_InsistentExam/Time 0900 1700/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E1_SlipperyPath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global HarveyMod_CD_E1`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E2_InsistentExam`
- **Preconditions:** `Time 0900 1700/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E1_SlipperyPath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global HarveyMod_CD_E1`
- **Commands:** AddConversationTopic, addConversationTopic
- **Bridge (mail/topic):** Current, topicHarveyTrust_Breakfast, topicHarveyTrust_DoctorDecides, topicHarveyTrust_Rest, topicHarveyTrust_Water
- **Script preview:** `          Hospital_Ambient/          6 10/          Harvey 2 5 2 farmer 5 10 0/          setSkipActions AddMail Current HarveyOverhaul_E2_PierInvite tomorrow/          skippable/          playSound doorOpen/          pause 400/          emote farmer 12/          pause 100/          move farmer 0 -5 3/          pause 100/          faceDirection Harvey 1/          pause 100/          speak Harvey "Садитесь. Пульс, дыхание, осмотр — по порядку.$a"/          pause 100/          proceedPosition farme...`

### `HarveyOverhaulStory.E5_StormBeside/Weather Storm/Time 1400 2000/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet, !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E5_StormBeside, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4B`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E5_StormBeside`
- **Preconditions:** `Weather Storm/Time 1400 2000/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet, !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E5_StormBeside, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4B`
- **Commands:** AddConversationTopic, addConversationTopic, friendship
- **Bridge (mail/topic):** Current, HarveyMod_CD_E5, HarveyMod_CD_Global, topicHarveyStorm_Clinic, topicHarveyStorm_Escort, topicHarveyStorm_Home, topicHarveyStorm_Note
- **Script preview:** `         none/         10 19/         farmer 10 19 0 Harvey 9 19 1/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E5 5/         skippable/         ambientLight 80 80 110/         playSound thunder/         emote farmer 28/         startJittering/         speak Harvey "Ко мне. Сейчас — внутрь.$8"/         playSound doorClose/         pause 200/         move Harvey 0 -4 0 true/         pause 200/         move farmer 0 -4 3 true/...`

### `HarveyOverhaulStory.E6_SayItOutLoud/Time 1900 2330/!FestivalDay/Friendship Harvey 1750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E5_StormBeside, !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E6_SayItOutLoud, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E5`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E6_SayItOutLoud`
- **Preconditions:** `Time 1900 2330/!FestivalDay/Friendship Harvey 1750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E5_StormBeside, !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E6_SayItOutLoud, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global, !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E5`
- **Commands:** friendship
- **Bridge (mail/topic):** Current, HarveyMod_CD_E6, HarveyMod_CD_Global, topicHarveyCareAgreement
- **Script preview:** `         Hospital_Ambient/         10 16/         Harvey 9 19 1 farmer 10 19 0/         skippable/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 4#AddConversationTopic Current HarveyMod_CD_E6 7#AddConversationTopic Current topicHarveyCareAgreement 7/         ambientLight 100 90 75/         pause 400/         move Harvey 0 -3 0 true/         move farmer 0 -3 3 true/         proceedPosition Harvey/         proceedPosition farmer/         pause 500/         playSound shwip...`

### `eventHarveyCareMovementAnimationTest/Time 900 1700/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyCareMovementAnimationTest`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyCareMovementAnimationTest`
- **Preconditions:** `Time 900 1700/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyCareMovementAnimationTest`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `         Hospital_Ambient/         5 8/         farmer 5 6 3 Harvey 4 5 1/         skippable/         viewport 5 8 true/         pause 600/         faceDirection Harvey 2/         emote Harvey 16/         pause 500/         speak Harvey "Подождите... вы дрожите.$s"/         message "Я просто немного устала..."/         speak Harvey "\\"Немного\\" — опасное слово. Обычно после него мои пациенты пытаются упасть в обморок.$a"/         move Harvey 0 1 2/         faceDirection Harvey 1/         pause...`

### `eventHarveyCheckup/Time 1400 1600/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicAgreedCheckup/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery DAYS_PLAYED 2`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveyCheckup`
- **Preconditions:** `Time 1400 1600/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicAgreedCheckup/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyFirstMeeting/GameStateQuery DAYS_PLAYED 2`
- **Commands:** —
- **Bridge (mail/topic):** topicAfterCheckup, topicAgreedCheckup
- **Script preview:** `                     none/                     4 6/                     farmer 4 6 1 Harvey 4 5 3/                     skippable/                     speak Harvey "*поднимает голову от бумаг* А, вы пришли! *улыбается* Проходите, располагайтесь.$h#$b#*указывает на кушетку* Садитесь, пожалуйста. Это займет совсем немного времени.$l"/                     pause 1000/                     move farmer 1 0 0/                     pause 200/                     move farmer 0 -2 2/                     paus...`

### `eventHarveyEmergencyCare`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveyEmergencyCare`
- **Preconditions:** `none`
- **Commands:** —
- **Bridge (mail/topic):** mailHarveyPostTrauma
- **Script preview:** `     none/     -1000 -1000/     farmer 1000 1000 0 Harvey 1000 1000 0/     message "Всё плывёт перед глазами..."/     message "Ты чувствуешь, как кто-то подхватывает тебя на руки..."/     message "Ты приходишь в себя в клинике. Харви в белом халате с пятнами крови."/     changeLocation Hospital/     pause 200/     warp farmer 14 6/     positionOffset farmer 0 10/     faceDirection farmer 2/     animate farmer false true 1000 4 5/     pause 200/     warp Harvey 15 6/     faceDirection Harvey 3/  ...`

### `eventHarveyExhaustion`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveyExhaustion`
- **Preconditions:** `none`
- **Commands:** —
- **Bridge (mail/topic):** topicHarveyExhaustion
- **Script preview:** `     none/     -1000 -1000/     farmer 1000 1000 1 Harvey 1000 1000 1/     ambientLight 0 0 0/     pause 1000/     message "Твоё дыхание становится прерывистым, руки дрожат, а перед глазами пляшут чёрные точки..."/     message "Последнее, что ты видишь - как Харви резко бросает медицинские карты и бежит к тебе..."/     message "Ты приходишь в себя в клинике."/     changeLocation Hospital/     ignoreCollisions farmer/     warp farmer 20 5/     faceDirection farmer 2/     animate farmer true true ...`

### `eventHarveyMedicalCheck/Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MAIL Current mailHarveyMedicalCheckReminder Received/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyMedicalCheck`
- **Preconditions:** `Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MAIL Current mailHarveyMedicalCheckReminder Received/GameStateQuery !PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `         Hospital_Ambient/         10 15/         Harvey 10 14 2/         playSound doorOpen/         warp farmer 10 19 true/         faceDirection farmer 0/         skippable/         pause 500/         emote farmer 12/         speak Harvey "Привет. Рад, что ты ${пришёл^пришла}$.$h"/         pause 300/         move Harvey 0 2 2 true/         move farmer 0 -2 0 true/         pause 200/         speak Harvey "Ты дрожишь. Давай сядем."/         pause 200/         faceDirection Harvey 0/         pau...`

### `eventHarveyMedicalCheck_Dating/Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MAIL Current mailHarveyMedicalCheckReminder Received/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyMedicalCheck_Dating`
- **Preconditions:** `Friendship Harvey 1500/Time 1400 1800/Weather sunny/GameStateQuery PLAYER_HAS_MAIL Current mailHarveyMedicalCheckReminder Received/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `         Hospital_Ambient/         10 15/         Harvey 10 14 2/         playSound doorOpen/         warp farmer 10 19 true/         faceDirection farmer 0/         skippable/         pause 500/         emote farmer 12/         speak Harvey "Привет, @. Рад тебя видеть.$l"/         pause 300/         move Harvey 0 2 2 true/         move farmer 0 -2 0 true/         pause 200/         speak Harvey "Ты дрожишь. Давай сядем."/         pause 200/         faceDirection Harvey 0/         pause 200/    ...`

### `eventHarveyTraumaExam/Time 0800 1800/Friendship Harvey 2000`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyTraumaExam`
- **Preconditions:** `Time 0800 1800/Friendship Harvey 2000`
- **Commands:** —
- **Bridge (mail/topic):** topicHarveyTraumaReveal
- **Script preview:** `none/         5 6/         farmer 5 6 3 Harvey 4 6 1/         animate Harvey false true 1000 22 20 21 20/         skippable/         speak Harvey "Одну минуту... Ты дышишь неровно. Мне нужно проверить твои рёбра.$u"/         pause 500/         emote farmer 28/         message "Нет, всё в порядке..."/         pause 300/         animate Harvey false true 1000 22 20 21 20 22/         pause 300/         speak Harvey " Это приказ, а не просьба. *берёт стетоскоп*$a"/         playSound backpackIN/     ...`

### `eventHarveyTreatmentCollapse`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyTreatmentCollapse`
- **Preconditions:** `none`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `none/         -1000 -1000/         farmer 1000 1000 0 Harvey 1000 1000 0/         skippable/         message "Голова кружится... Всё плывёт... Я не могу..."/         changeLocation Hospital/         pause 200/         warp farmer 14 6/         positionOffset farmer 0 10/         faceDirection farmer 2/         animate farmer false true 1000 4 5/         pause 200/         warp Harvey 15 6/         faceDirection Harvey 3/         viewport 14 6/         message Harvey "Ну наконец-то ты проснулась....`

### `eventStayInHospital`

- **Файл:** `events.json`
- **Event ID:** `eventStayInHospital`
- **Preconditions:** `none`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `none/         9 16/         farmer 9 16 1 Harvey 10 16 3/         skippable/         speak Harvey "Куда ты собралась?$a#$b#Я не говорил, что ты можешь вставать.$a"/         message "Мне нужно домой… Я уже лучше себя чувствую."/         speak Harvey "Ты ещё не готова. Я не хочу, чтобы ты снова рисковала здоровьем.$a#$b#Ложись обратно. Сегодня лучше останься здесь ещё немного — как врач, я настаиваю.$a"/         globalFade/         viewport -1000 -1000/         end...`


## Data/Events/Mine

### `eventHarveyMineInterception`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveyMineInterception`
- **Preconditions:** `none`
- **Commands:** —
- **Bridge (mail/topic):** HarveyMineIntercept
- **Script preview:** `     EarthMine/     17 7/     farmer 17 7 0 Harvey 17 10 0/     skippable/     pause 800/     speak Harvey "Стой.$a#$b#Стой прямо сейчас.$a"/     pause 600/     move Harvey 0 -2 0/     faceDirection farmer 2/     emote Harvey 12/     speak Harvey "Я же говорил тебе - никаких шахт во время лечения!$a#$b#Ты думаешь, это игра?$a"/     pause 400/     emote farmer 8/     message "Я... я просто хотела..."/     speak Harvey "Хотела что? Усугубить стресс? Получить еще раны?$a#$b#*осматривает тебя* Посмо...`

### `eventHarveyMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue`

- **Файл:** `eventsMineRescue.json`
- **Event ID:** `eventHarveyMineRescue`
- **Preconditions:** `GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue`
- **Commands:** —
- **Bridge (mail/topic):** mailHarveyAfterMineRescue, topicMineInjuryRescue
- **Script preview:** `none/     -1000 -1000/     farmer 17 7 2 Harvey 1000 1000 0/     pause 1000/     ambientLight 40 40 40/     fade true/     message "Ты теряешь сознание..."/     pause 2000/     playSound thudStep/     message "Быстрые шаги эхом отзываются в шахте..."/     pause 800/     warp Harvey 17 10/     viewport 17 7 true/     pause 500/     fade false/     pause 300/     emote Harvey 16/     speak Harvey "@?! НЕТ!$8"/     pause 400/     move Harvey 0 -2 0 true/     pause 200/     animate Harvey false true...`

### `eventHarveyMineRescueDating/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue`

- **Файл:** `eventsMineRescue.json`
- **Event ID:** `eventHarveyMineRescueDating`
- **Preconditions:** `GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue, !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue`
- **Commands:** —
- **Bridge (mail/topic):** topicMineInjuryRescue
- **Script preview:** `none/     -1000 -1000/     farmer 17 7 2 Harvey 1000 1000 0/     pause 900/     ambientLight 40 40 40/     fade true/     message "Темно… тяжело…"/     pause 800/     playSound thudStep/     message "Шаги эхом отдаются в шахте…"/     pause 400/     warp Harvey 17 10/     viewport 17 7 true/     pause 400/     fade false/     pause 200/     move Harvey 0 -2 0 true/     pause 200/     emote Harvey 16/     speak Harvey "@… Нет. Нет.$8"/     pause 400/     speak Harvey "Дыши. Смотри на меня.$8"/    ...`

### `eventHarveyMinorMineRescue/GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating`

- **Файл:** `eventsMineRescue.json`
- **Event ID:** `eventHarveyMinorMineRescue`
- **Preconditions:** `GameStateQuery PLAYER_NPC_RELATIONSHIP Current Harvey Dating Married/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMinorMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueDating`
- **Commands:** —
- **Bridge (mail/topic):** topicHarveyMinorMineRescue
- **Script preview:** `none/     -1000 -1000/     farmer 17 7 2 Harvey 17 10 0/     pause 500/     message "Голова кружится, дыхание сбивается... Ещё шаг — и ноги точно подведут."/     pause 400/     speak Harvey "@! Стой!$8"/     pause 200/     move Harvey 0 -2 0 true/     emote Harvey 16/     speak Harvey "Ты на грани. В таком состоянии я не дам тебе идти дальше.$a#$b#Сейчас в клинику — покой и осмотр.$u"/     pause 800/     globalFade/     changeLocation Hospital/     warp farmer 14 6/     warp Harvey 15 6/     vie...`


## Data/Events/Mountain

### `HarveyOverhaulStory.E4B_TooQuiet/Time 1800 2200/Weather Sunny/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E4B_TooQuiet`
- **Preconditions:** `Time 1800 2200/Weather Sunny/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E4B, HarveyMod_CD_Global, topicHarveyTooQuiet
- **Script preview:** `         none/         44 21/         farmer 42 21 1 Harvey 45 21 3/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 3#AddConversationTopic Current HarveyMod_CD_E4B 5#AddConversationTopic Current topicHarveyTooQuiet 5/         skippable/         ambientLight 90 85 110/         pause 400/         move farmer 2 0 1 true/         proceedPosition farmer/         faceDirection Harvey 3/         faceDirection farmer 1/         pause 300/         message "Харви стоит у перил и с...`

### `HarveyOverhaulStory.E4B_TooQuiet/Time 1800 2200/Weather Wind/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E4B_TooQuiet`
- **Preconditions:** `Time 1800 2200/Weather Wind/!FestivalDay/Friendship Harvey 1500/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4_PierBreath/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E4B_TooQuiet/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E4`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E4B, HarveyMod_CD_Global, topicHarveyTooQuiet
- **Script preview:** `         none/         44 21/         farmer 42 21 1 Harvey 45 21 3/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 3#AddConversationTopic Current HarveyMod_CD_E4B 5#AddConversationTopic Current topicHarveyTooQuiet 5/         skippable/         ambientLight 90 85 110/         pause 400/         move farmer 2 0 1 true/         proceedPosition farmer/         faceDirection Harvey 3/         faceDirection farmer 1/         pause 300/         message "Харви стоит у перил и с...`

### `eventHarveyMountainDate/Weather sunny/Time 900 1200/Friendship Harvey 2250/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyMountainDate`
- **Preconditions:** `Weather sunny/Time 900 1200/Friendship Harvey 2250/GameStateQuery PLAYER_NPC_RELATIONSHIP  Current Harvey Dating`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `         spring_day_ambient/         45 22/         farmer 41 19 3 Harvey 46 22 3/         move farmer 0 3 1/         move farmer 4 0 1/         skippable/         pause 500/         speak Harvey "$h @... я подумал, что тебе понравится это место. Здесь так тихо... и спокойно."/         pause 500/         message "Здесь действительно красиво. Ты прав."/         pause 500/         speak Harvey "$4 Я часто мечтал привести тебя сюда. Не как врач... а как человек, который хочет быть рядом."/         ...`


## Data/Events/SkullCave

### `eventHarveySkullCavePrevention`

- **Файл:** `eventsCare.json`
- **Event ID:** `eventHarveySkullCavePrevention`
- **Preconditions:** `none`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `     continue/     5 5/     farmer 5 5 0 Harvey 7 7 0/     skippable/     viewport 6 5 true/     pause 1500/     move Harvey 0 -2 3/     proceedPosition Harvey/     pause 100/     faceDirection farmer 1/     pause 100/     speak Harvey "@! Немедленно выйди отсюда!$a#$b#Пещера черепа — не место для игр! Здесь можно умереть!$a"/     pause 1800/     emote Harvey 12/     speak Harvey "Ты понимаешь, что тут происходит?$a#$b#Монстры, которые могут убить за секунду! Яд, проклятия, смертельные ловушки!?...`


## Data/Events/Town

### `HarveyOverhaulStory.E2B_QuietAgreement/Time 1000 1600/Weather Sunny/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E2B_QuietAgreement`
- **Preconditions:** `Time 1000 1600/Weather Sunny/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E2B, HarveyMod_CD_Global, topicHarveyTrust_PublicCare
- **Script preview:** `          none/          28 67/          farmer 28 67 2 Harvey 32 67 1/          setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E2B 3#AddConversationTopic Current topicHarveyTrust_PublicCare 5/          skippable/          pause 400/          emote farmer 56/          showFrame farmer 107/          message "Ты останавливаешься у лавки, прислонившись к стволу дерева."/          emote Harvey 16/          speed Harvey 4/          move Har...`

### `HarveyOverhaulStory.E2B_QuietAgreement/Time 1000 1600/Weather Wind/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E2B_QuietAgreement`
- **Preconditions:** `Time 1000 1600/Weather Wind/!FestivalDay/Friendship Harvey 750/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2_InsistentExam/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E2B_QuietAgreement/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E2`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E2B, HarveyMod_CD_Global, topicHarveyTrust_PublicCare
- **Script preview:** `          none/          28 67/          farmer 28 67 2 Harvey 32 67 1/          setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E2B 3#AddConversationTopic Current topicHarveyTrust_PublicCare 5/          skippable/          pause 400/          emote farmer 56/          showFrame farmer 107/          message "Ты останавливаешься у лавки, прислонившись к стволу дерева."/          emote Harvey 16/          speed Harvey 4/          move Har...`

### `HarveyOverhaulStory.E7_TownSip_Sunny/Weather Sunny/Time 1200 1500/!FestivalDay/Friendship Harvey 2000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E6_SayItOutLoud/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E7_TownSip_Sunny/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E7_TownSip_Sunny`
- **Preconditions:** `Weather Sunny/Time 1200 1500/!FestivalDay/Friendship Harvey 2000/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E6_SayItOutLoud/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E7_TownSip_Sunny/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global`
- **Commands:** —
- **Bridge (mail/topic):** Current, HarveyMod_CD_E7, HarveyMod_CD_Global
- **Script preview:** `          none/          26 22/          farmer 26 22 1 Harvey 29 22 3/          skippable/          setSkipActions AddConversationTopic Current HarveyMod_CD_Global 2#AddConversationTopic Current HarveyMod_CD_E7 2/          pause 300/          emote farmer 56/          message "Солнце греет камни на площади. На секунду шум города становится слишком густым."/          emote Harvey 16/          speak Harvey "Вы остановились.$u"/          speak Harvey "Нет, не отвечайте. Я вижу.$a"/          pause ...`

### `HarveyOverhaulStory.E9_LightInWindow/Time 2000 2330/!FestivalDay/Friendship Harvey 2250/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E8_QuietShelf/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E9_LightInWindow/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E8`

- **Файл:** `events.json`
- **Event ID:** `HarveyOverhaulStory.E9_LightInWindow`
- **Preconditions:** `Time 2000 2330/!FestivalDay/Friendship Harvey 2250/GameStateQuery PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E8_QuietShelf/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulStory.E9_LightInWindow/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_Global/GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_E8`
- **Commands:** warp, friendship
- **Bridge (mail/topic):** Current, HarveyMod_CD_E9, HarveyMod_CD_Global, topicHarveyTrustFinal
- **Script preview:** `         none/         35 88/         farmer 35 88 0 Harvey -1000 -1000 0/         skippable/         setSkipActions AddConversationTopic Current HarveyMod_CD_Global 4#AddConversationTopic Current HarveyMod_CD_E9 7#AddConversationTopic Current topicHarveyTrustFinal 10#AddMail Current HarveyOverhaul_E9_LightNote tomorrow/         ambientLight 80 70 55/         viewport 35 88 true/         pause 400/         message "В окне клиники всё ещё горит свет."/         message "На подоконнике стоит кружка...`

### `eventHarveyLateNightCollapse/Time 2400 2600`

- **Файл:** `events.json`
- **Event ID:** `eventHarveyLateNightCollapse`
- **Preconditions:** `Time 2400 2600`
- **Commands:** —
- **Bridge (mail/topic):** —
- **Script preview:** `continue/         37 59/         farmer 37 59 3 Harvey 1000 1000 0/         positionOffset farmer 0 -16/         faceDirection farmer 2/         animate farmer false true 3000 5 4/         skippable/         pause 1000/         playSound doorClose/         pause 400/         warp Harvey 36 56/         faceDirection Harvey 2/         pause 400/         emote Harvey 16/         pause 400/         speed Harvey 5/         move Harvey 0 3 1/         pause 400/         animate Harvey false true 1000 4...`


## Data/Events/Woods

### `eventRescueOperation/Weather Storm/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicRescueOperation/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventRescueOperation`

- **Файл:** `events.json`
- **Event ID:** `eventRescueOperation`
- **Preconditions:** `Weather Storm/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicRescueOperation/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventRescueOperation`
- **Commands:** —
- **Bridge (mail/topic):** topicRescueComplete, topicRescueOperation
- **Script preview:** `         thunder_small/         -1000 -1000/         farmer 1000 1000 0 Harvey 1000 1000 0 Lewis 1000 1000 0/         skippable/         pause 500/         changeLocation Hospital/         warp Harvey 3 15/         warp Lewis 1000 1000/         faceDirection Harvey 0/         viewport 8 15/         playSound telephone_dialtone/         emote Harvey 16/         speak Harvey "Алло? Льюис?$0"/         pause 600/         speak Lewis "Харви! Я сбил @! Она выбежала на дорогу во время грозы!$8"/       ...`
