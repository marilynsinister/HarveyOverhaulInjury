# Контролирующие реплики — замены

Всего затронуто записей: **116+** (ручные доработки после прогона, **2026-05-23**)
Полный список ключей диалогов лечения: см. ниже.

**Статус:** правки **применены** в CP (`events.json`, `eventsCare.json`, `eventsMineRescue.json`, `dialoguesHarvey.json` и др.).

## Изменённые события (без Dating/Married)

| Сцена | Файл | Суть правок |
|-------|------|-------------|
| `eventHarveyTreatmentCollapse` | events.json | «не обсуждается» → просьба; «без разрешения» → «покажись мне»; «слежу» → «буду проверять» |
| `eventStayInHospital` | events.json | «приказ врача / остаёшься» → «настаиваю»; «не позволю рисковать» → «не хочу, чтобы снова рисковала» |
| `HarveyOverhaulStory.E2_InsistentExam` | events.json | убраны «защитник», «не обсуждается», «отвечаю за здоровье»; режим как рекомендация врача |
| `HarveyMod_FirstTreatment` | events.json | шахты «сначала покажись»; «не обсуждается» → просьба |
| `HarveyMod_NightCrisis` | events.json | «не смей протестовать» → «не спорь, если согласна»; мягче удержание на ночь |
| `HarveyOverhaulStory.E6_SayItOutLoud` | events.json | «не отпущу в темноте» → «могу остаться, если захочешь» |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | events.json | «не позволю упасть» → «не хочу, чтобы упала» |
| `eventHarveyLateNightCollapse` | events.json | «не обсуждается» → просьба |
| `HarveyOverhaulStory.E4_PierBreath` | events.json | «безопасность не обсуждается» → «отнесись серьёзно» |
| `eventHarveyStormComfortDesert` | events.json | «не отпущу ни на шаг» → «провожу, пока не будешь в безопасности» |
| `eventRescueOperation` | events.json | «останешься у меня» → «предложу остаться под присмотром» |
| `eventHarveyMineInterception` | eventsCare.json | «принимаю решения за тебя» → «слово за тобой»; мягче отправка домой |
| `MyMod_HarveyStressTiredCheck` | events_for_mode_new_formatted.json | «не отпущу домой» → «останься здесь» |

## Dating/Married — убрано отнятие согласия

| Сцена | Правки |
|-------|--------|
| `eventHarveyPropose` | «защитник» → «берег/опора»; «решение за нас обоих» → «решение только твоё» |
| `eventHarveyMineRescueDating` | «не обсуждается» → просьба; «Не спорь» → «доверься мне» |
| `eventHarveyCheckHealthFarmer` (Dating) | «не отпущу ни на секунду» → «рядом на каждом шагу» |
| `mailHarveyAfterMineRescue` | «Это не просьба» → «Это важно» |

## Примеры diff (события)

**Коллапс / госпитализация**
- Было: «Это не обсуждается» / «Не вставай без моего разрешения»
- Стало: «Я очень прошу отнестись к этому серьёзно» / «Пожалуйста, не вставай — сначала покажись мне»

**E2 — настойчивый осмотр**
- Было: «ваш защитник» / «режим — не обсуждается»
- Стало: «рядом и помогу, если вы позволите» / «режим — рекомендация врача»

**Ночной кризис**
- Было: «И не смей протестовать! Я не отпущу тебя домой…»
- Стало: «Пожалуйста, не спорь сейчас… Останься здесь, если согласна»

---

## Изменённые ключи диалогов (выборка)

### `dialoguesHarvey.json::AcceptGift_(O)348`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarvey.json::ArchaeologyHouse`
- Я не позволю тебе страдать → Я сделаю всё, чтобы тебе не страдать

### `dialoguesHarvey.json::Beach8`
- это не обсуждается → я очень прошу отнестись к этому серьёзно

### `dialoguesHarvey.json::Desert`
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarvey.json::GreenRain`
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarvey.json::Hospital_Tue`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarvey.json::Mon4`
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarvey.json::Outdoor_4`
- это не обсуждается → я очень прошу отнестись к этому серьёзно

### `dialoguesHarvey.json::RejectItem_category_alcohol`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarvey.json::Saloon4`
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarvey.json::Treat_Hurt_Before`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarvey.json::eventSeen_eventHarveyFirstWalk_twoweek`
- Я не позволю тебе страдать → Я сделаю всё, чтобы тебе не страдать

### `dialoguesHarvey.json::summer_Thu`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarvey.json::summer_Wed`
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarvey.json::winter_8`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarvey.json::winter_Thu`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_AlcoholPoisoning_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_AlcoholPoisoning_After2`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_AlcoholPoisoning_After3`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_AlcoholPoisoning_After4`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_AlcoholPoisoning_After5`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_AlcoholPoisoning_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_BackStrain_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_BackStrain_After3`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_BackStrain_After4`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_BackStrain_After5`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_BackStrain_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_BackStrain_Before6`
- Я не позволю тебе игнорировать мои указания → Пожалуйста, не игнорируй мои рекомендации

### `dialoguesHarveyCure.json::Treat_BadlyHurt_After2`
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_BadlyHurt_After3`
- Не вздумай сопротивляться лечению → Пожалуйста, не сопротивляйся лечению

### `dialoguesHarveyCure.json::Treat_BadlyHurt_After4`
- Я не позволю никому тебя тревожить → Я не дам никому тебя тревожить

### `dialoguesHarveyCure.json::Treat_BadlyHurt_After5`
- Я отвечаю за твоё состояние → Как врач, я обязан следить за твоим состоянием
- Я слежу за каждым признаком → Я буду внимательно следить за каждым признаком

### `dialoguesHarveyCure.json::Treat_BadlyHurt_Before1`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_BadlyHurt_Before7`
- Я не позволю тебе игнорировать мои указания → Пожалуйста, не игнорируй мои рекомендации

### `dialoguesHarveyCure.json::Treat_BruisedRibs_After2`
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_BruisedRibs_After3`
- Не вздумай сопротивляться лечению → Пожалуйста, не сопротивляйся лечению

### `dialoguesHarveyCure.json::Treat_BruisedRibs_After5`
- Я отвечаю за твоё состояние → Как врач, я обязан следить за твоим состоянием
- Я слежу за каждым признаком → Я буду внимательно следить за каждым признаком

### `dialoguesHarveyCure.json::Treat_BruisedRibs_Before6`
- Я не позволю тебе игнорировать мои указания → Пожалуйста, не игнорируй мои рекомендации

### `dialoguesHarveyCure.json::Treat_BurnWounds_After5`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_Concussion_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_Concussion_After2`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_Concussion_After3`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_Concussion_After4`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_Concussion_After5`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_Concussion_After6`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_Concussion_Before6`
- Я не позволю тебе страдать → Я сделаю всё, чтобы тебе не страдать

### `dialoguesHarveyCure.json::Treat_DeepCuts_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_DeepCuts_After3`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_DeepCuts_After4`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_DeepCuts_After5`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_DeepCuts_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_DeepCuts_Before4`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_FracturedBone_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_FracturedBone_After2`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_FracturedBone_After3`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_FracturedBone_After4`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_FracturedBone_After5`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_FracturedBone_After6`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_Hurt_After3`
- Я не позволю тебе снова себя травмировать → Я не хочу, чтобы ты снова себя травмировала

### `dialoguesHarveyCure.json::Treat_Hurt_After6`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_Hurt_Before1`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_InfectedWound_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_InfectedWound_After2`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_InfectedWound_After3`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_InfectedWound_After4`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_InfectedWound_After5`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_InfectedWound_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_ShrapnelWounds_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_ShrapnelWounds_After2`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_ShrapnelWounds_After3`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_ShrapnelWounds_After4`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_ShrapnelWounds_After5`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_ShrapnelWounds_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_SprainedAnkle_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_SprainedAnkle_After3`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_SprainedAnkle_After4`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_SprainedAnkle_After5`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_SprainedAnkle_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_SprainedAnkle_Before1`
- иначе я тебя просто не отпущу → пока я не убедюсь, что ты в безопасности

### `dialoguesHarveyCure.json::Treat_SurgicalWound_After1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_SurgicalWound_After2`
- Не смей! → Пожалуйста, не рискуй

### `dialoguesHarveyCure.json::Treat_SurgicalWound_After3`
- Я не позволю тебе навредить себе → Я не хочу, чтобы ты снова навредила себе

### `dialoguesHarveyCure.json::Treat_SurgicalWound_After4`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::Treat_SurgicalWound_After5`
- Я не отпущу тебя, пока → Я останусь рядом, пока
- Я внимательно слежу за → Я буду внимательно следить за

### `dialoguesHarveyCure.json::Treat_SurgicalWound_After6`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treat_SurgicalWound_Before3`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::Treat_TornMuscles_After3`
- Я не позволю тебе страдать → Я сделаю всё, чтобы тебе не страдать

### `dialoguesHarveyCure.json::Treatment_Phase_Encouragement`
- Я не отпущу тебя, пока → Я останусь рядом, пока

### `dialoguesHarveyCure.json::Treatment_Phase_Warning1`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::buffAntibioticsTreatment`
- Я не позволю тебе рисковать → Я не хочу, чтобы ты снова рисковала

### `dialoguesHarveyCure.json::topicBackStrainPhaseRecovery`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyCure.json::topicShrapnelWoundsPhaseHealing`
- без моего разрешения → сначала покажись мне

### `dialoguesHarveyPregnant.json::topicAfterCheckup`
- это не обсуждается → я очень прошу отнестись к этому серьёзно
- без моего разрешения → сначала скажи мне

### `dialoguesHarveyPregnant.json::topicHarveyIntensiveCare`
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно

### `dialoguesHarveyStress.json::topicStressDarknessLevel3`
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно

### `events.json::HarveyMod_FirstTreatment`
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно
- без моего разрешения → сначала покажись мне
- Я не позволю ничему плохому случиться с тобой → Я сделаю всё, чтобы с тобой ничего плохого не случилось

### `events.json::HarveyMod_NightCrisis`
- А теперь - никуда не уходишь. Остаёшься здесь до утра → А теперь, пожалуйста, останься здесь до утра — тебе нужен покой
- Я не отпущу тебя домой в таком состоянии → Я прошу остаться здесь в таком состоянии

### `events.json::HarveyOverhaulStory.E2_InsistentExam`
- режим — не обсуждается → режим — это рекомендация врача, прошу отнестись серьёзно
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно
- Я отвечаю за ваше здоровье → Как врач, я обязан предупредить вас о рисках
- ваш защитник → рядом и помогу, если вы позволите
- Я не отпущу вас, пока → Я провожу вас, пока
- Я слежу за вами → Я буду проверять, как вы справляетесь, если вы не против
- Я не позволю вам снова довести себя → Я не хочу, чтобы вы снова довели себя

### `events.json::HarveyOverhaulStory.E4_PierBreath`
- Безопасность не обсуждается → Пожалуйста, отнесись к безопасности серьёзно
- без моих напоминаний → самостоятельно

### `events.json::HarveyOverhaulStory.E6_SayItOutLoud`
- Я не отпущу тебя одну в темноте → Если станет страшно — зови меня. Я могу остаться рядом, если захочешь

### `events.json::HarveyOverhaulStory.E7_TownSip_Sunny`
- Я не позволю тебе упасть → Я не хочу, чтобы ты упала

### `events.json::eventHarveyLateNightCollapse`
- это не обсуждается → я очень прошу отнестись к этому серьёзно

### `events.json::eventHarveyStormComfortDesert`
- Я не отпущу тебя ни на шаг → Я провожу тебя и не отойду, пока ты не будешь в безопасности

### `events.json::eventHarveyTreatmentCollapse`
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно
- без моего разрешения → сначала покажись мне
- Я слежу за твоим состоянием → Я буду проверять твоё состояние

### `events.json::eventRescueOperation`
- ты останешься у меня → я предложу тебе остаться под моим присмотром

### `events.json::eventStayInHospital`
- Сегодня ты остаёшься здесь. Это приказ врача → Сегодня лучше останься здесь ещё немного — как врач, я настаиваю

### `eventsCare.json::eventHarveyMineInterception`
- я принимаю решения за тебя → я предложу план лечения, но окончательное слово — за тобой
- Я не позволю тебе причинить себе вред → Я не хочу, чтобы ты снова навредила себе
- Ты идёшь домой. Прямо сейчас → Давай вернёмся домой — тебе нужен покой

### `eventsMineRescue.json::eventHarveyMineRescueDating`
- Это не обсуждается → Я очень прошу отнестись к этому серьёзно

### `eventsMineRescue.json::mailHarveyAfterMineRescue`
- Это не просьба. Твоё здоровье → Это важно. Твоё здоровье
