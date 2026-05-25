# Аудит романтического тона (без gate Dating/Married)

Дата: 2026-05-23 (критерии стадий — см. [audit-relationship-tone.md](../audit-relationship-tone.md) § «Таблица стадий»)  
Файлы: `content.json` → `assets/Code/events*.json`, `events_for_mode_new_formatted.json`  
Критерии нарушения **до Dating**: «люблю», «моя/мой», pet names, «только мы», поцелуи/объятия как romance, сцена-свидание. На стадии **6–8♥** допустима тревога и «ты», но **не** romance-маркеры. **Обращение:** «Вы» только 0–2♥; story E2+ (`750+`) — «ты» (см. канон в `audit-relationship-tone.md`).

## Уже с gate Dating/Married — не трогаем

| Событие | Gate |
|---------|------|
| `eventHarveyMorningCheckup` | Dating |
| `eventHarveyCheckHealthFarmer` | Dating |
| `eventHarveyCheckFarmerOutsideAfter22` | Dating Married |
| `eventHarveyMedicalCheck_Dating` | Dating Married |
| `eventHarveyFirstDate` | Dating |
| `eventHarveyMountainDate` | Dating |
| `eventHarveyPropose` | Dating |
| `eventHarveyRoomCheckup2` | Dating |
| `HarveyMod_NightCrisis_Dating` | Dating Married |
| `HarveyMod_BirthdayHospital_Dating` | Dating Married |
| `eventHarveyMineRescueDating` | Dating Married |

## Найденные нарушения → решение

| Событие | ♥ / триггер | Маркеры | Вариант | Обоснование |
|---------|-------------|---------|---------|-------------|
| `eventHarveyFirstWalk` | day 11 | `$l`, рука, романтичный fork | **B** | Ранняя story-прогулка; достаточно смягчить до врачебно-дружеской |
| `eventHarveyStormComfortFarm` | 750, гроза | «не могу позволить», ночёвка, `$l` | **B** | Медицинский comfort; intimate sleepover остаётся, тон — профессиональный |
| `eventHarveyStormComfortMountain` | 750 | «не могу потерять», «смысл жизни», объятие | **B** | Спасение при грозе, не свидание |
| `eventHarveyStormComfortTown` | 750 | «моя радость», `$l` | **B** | Публичное укрытие у Гаса |
| `eventHarveyStormComfortForest` | 750 | «будь моим деревом», «лучшее укрытие — я» | **B** | Метафора романтики → опора/врач |
| `eventHarveyStormComfortDesert` | 750 | «сердце — компас», «никогда не оставлю» | **B** | Экстренный поиск, не dating |
| `eventHarveyStormComfortMine` | 750 | «убежище», «смысл жизни», «люблю»-тон | **B** | Эвакуация из шахты |
| `HarveyOverhaulStory.E5_StormBeside` | 1500 | «моё желание», `$l` | **B** | Story E1–E8: не поднимать Dating |
| `HarveyOverhaulStory.E6_SayItOutLoud` | 1750 | «защищать», «свет в окне для тебя» | **B** | Терапевтическая сцена в теплице |
| `HarveyMod_FirstTreatment` | 750 | `$l` на страхе/доверии | **B** | Первый осмотр — клинический тон |
| `HarveyMod_TreatmentPlanMeeting` | 500 | `$l` на плане | **B** | Лёгкий `$l` → `$0` |
| `eventRescueOperation` | 600 | «только мы», «Только я» | **B** | Травма/осмотр; уже смягчён контакт в physical-audit |
| `MyMod_HarveyStormComfortForest` | 750 | дубль forest storm | **B** | То же, что основной forest |
| `MyMod_HarveyStressTiredCheck` | 250 | «солнышко» | **B** | Гospital stress — профессиональный тон |
| `MyMod_HarveyUrgentFarmVisit` | 1000 | «чтобы тебя любили», hug anim | **B** | Breakdown visit — медицинская срочность |
| `eventHarveyFirstMeeting` | day 1 | `$l` на пиджаке | **B** | Погранично; `$l` → `$0` |

## Не трогали (погранично / уже OK)

| Событие | Причина |
|---------|---------|
| `HarveyMod_NightCrisis_PreDating` | Уже переписан в профессиональный тон |
| `HarveyMod_BirthdayHospital_Friend` | Friend-версия без «только мы» / «самый важный день» |
| `eventHarveyMedicalCheck` (pre-dating) | Уже professional fork |
| `HarveyOverhaulStory.E4/E7/E8` | Нет явных романтических маркеров |
| `eventsCare.json` (ранние визиты) | Много `$l`, но без pet names / love / date framing — оставлены |
| `dialoguesHarveyStress/Cure/Pregnant.json` | Топики ставятся C#/событиями; отдельный проход при необходимости |
| Закомментированные блоки в `events.json` | Не активны |

## Вариант A не применялся

Ни одна pre-dating сцена не является чистым свиданием/признанием — все привязаны к травме, грозе, лечению или story-дуге E1–E8. Split на `_Dating`/`_PreDating` (как NightCrisis) не требовался.

## Применённые правки (2026-05-23)

- `assets/Code/events.json` — 16 событий, вариант **B**
- `assets/Code/events_for_mode_new_formatted.json` — 3 MyMod-события, вариант **B**

### Ключевые замены

| Событие | Было | Стало |
|---------|------|-------|
| `eventHarveyStormComfortFarm` | «Оставайся у меня», `HarveyRoom`, «моя теперь» | Клиника, кушетка, проверка фермы как врач |
| `eventHarveyStormComfortMountain` | «не могу потерять», «смысл жизни» | «как пациентка», «моя работа» |
| `eventRescueOperation` | «Только мы», «Только я.$l» | «никого лишних», «буду дежурить» |
| `HarveyOverhaulStory.E5/E6` | «моё желание», «защищать», «свет для тебя» | «моя работа», «клиника открыта» |
| `MyMod_HarveyUrgentFarmVisit` | «чтобы тебя любили», hug anim | «кто-то рядом», без объятия |

### Отложено

`dialoguesHarveyStress.json`, `dialoguesHarveyCure.json`, `dialoguesHarveyPregnant.json` — pet names / «люблю» без gate в ключах; отдельный проход.
