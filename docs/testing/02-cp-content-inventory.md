# Инвентаризация CP-контента Harvey Overhaul [CP]

> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> Сопоставление с C#: [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md)

**Область:** Content Patcher `HarveyOverhaul [CP]` — `content.json`, `assets/Code/*.json`, proximity asset.  
**Не включено:** правки JSON, полный каталог stress-бaffов (~40 шт. в `buffsStress.json` / `buffsCureStress.json`) — только перекрёстные ссылки.

**Структура CP:** патчи идут через `content.json` → `Include` / `Load`; отдельной папки `Data/` в репозитории нет — всё в `assets/Code/`.

| Include в content.json | Target |
|------------------------|--------|
| `buffsInjury.json`, `buffsCure.json`, `buffsMedicalCare.json`, … | `Data/Buffs` |
| `events.json`, `eventsCare.json`, `eventsMineRescue.json`, … | `Data/Events/*` |
| `dialoguesHarvey*.json`, `dialoguesNpc.json` | `Characters/Dialogue/Harvey` (+ NPC) |
| `mail*.json` | `Data/Mail` |
| `triggersCare.json`, `triggersInjuryMail.json` | `Data/TriggerActions` |
| `assets/Dialogue/harvey_proximity_injury.json` | `Load` → `Data/HarveyOverhaul/HarveyProximityInjuryDialogue` |

**TriggerActions (активные):** `triggersCare.json` (mine/skull interception, care mail chain), `triggersInjuryMail.json` (BETAS pass-out → `eventHarveyLateNightCollapse` + tiered sleep mail). Закомментированы в `content.json`: `triggersCure`, `triggersInjury`, `triggersStress`, …

---

## Buffs из CP (Injury / Medical)

Колонка «C#» — `KnownTraumas` / `KnownComplications` / `GetPhaseBuffId` / `CureByInjury` / `BuffManager` в `HarveyOverhaulInjury`.  
**Риск несовпадения:** 🔴 критично для теста · 🟡 косметика/legacy · 🟢 OK

### Базовые травмы и осложнения

| buff id | название | описание (кратко) | источник-файл | C# | риск |
|---------|----------|-------------------|---------------|-----|------|
| `buffHurt` | Рана | Вас ранили | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffBadlyHurt` | Сильная рана | Серьёзное ранение | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffSprainedAnkle` | Растянутая лодыжка | Хромота, фиксация | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffBruisedRibs` | Ушибленные рёбра | Боль при дыхании | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffBackStrain` | Растяжение спины | Боль в пояснице | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffDeepCuts` | Глубокие порезы | Кровоточащие раны | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffBurnWounds` | Ожоговые раны | Ожоги 2-й степени | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffTornMuscles` | Разорванные мышцы | Мышечная травма | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffConcussion` | Сотрясение мозга | Головокружение, тошнота | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffFracturedBone` | Перелом кости | Сломанная кость | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffShrapnelWounds` | Осколочные ранения | Множественные раны | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffInfectedWound` | Инфицированная рана | Воспаление, антибиотики | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffSurgicalWound` | Послеоперационная рана | Швы после осколков | `buffsInjury.json` | ✅ trauma | 🟢 |
| `buffCold` | Простуда | После дождя | `buffsInjury.json` | ✅ trauma | 🟢 |
| `HarveyMod_DirtyWound` | Грязная рана | Риск инфекции в шахте | `buffsInjury.json` | ✅ complication | 🟢 |
| `HarveyMod_WetBandage` | Мокрая повязка | Промокшая повязка | `buffsInjury.json` | ✅ complication | 🟢 |
| `HarveyMod_WetStitches` | Намокшие швы | Купание повредило шов | `buffsInjury.json` | ✅ complication | 🟢 |
| `HarveyMod_Neglect` | Небрежность в лечении | Игнор рекомендаций | `buffsInjury.json` | ✅ complication | 🟢 |
| `HarveyMod_PainFlare` | Обострение боли | Гроза, давление | `buffsInjury.json` | ✅ complication | 🟢 |
| `HarveyMod_AllergicRash` | Аллергическая реакция | Пыльца / швы | `buffsInjury.json` | ✅ complication | 🟡 C# без autotrigger |
| `HarveyMod_MineForbidden` | Харви запретил шахту | Debuff на день | `buffsInjury.json` | ✅ state | 🟢 |
| `buffPainFlare` | Острая боль | Legacy ID | `buffsInjury.json` | ❌ | 🟡 дубль `HarveyMod_PainFlare` |
| `buffFarmerExhausted` | Истощение | Переутомление | `buffsInjury.json` | ⚠️ pass-out | 🟡 не в KnownTraumas |
| `buffSleepy` | Нарушение режима | После late pass-out | `buffsInjury.json` | ⚠️ trigger | 🟢 |
| `buffAlcoholPoisoning` | Алкогольное отравление | CP-only interaction | `buffsInjury.json` | ❌ | 🟡 |
| `HarveyMod_ImpairedMobility` | Нарушение подвижности | Без лечения | `buffsInjury.json` | ❌ | 🟡 orphan escalation? |
| `HarveyMod_MuscleAtrophy` | Атрофия мышц | Без лечения | `buffsInjury.json` | ❌ | 🟡 |
| `HarveyMod_BreathingDifficulty` | Затруднённое дыхание | Ушиб рёбер | `buffsInjury.json` | ❌ | 🟡 |
| `HarveyMod_Sepsis` | Заражение крови | Критическое | `buffsInjury.json` | ❌ | 🔴 нет C# pipeline |
| `buffTooCold` | — | — | — | ✅ C# debuff | 🔴 **нет в CP** |

### Фазовые баффы лечения (phase buffs)

| buff id | название | источник | C# `GetPhaseBuffId` | риск |
|---------|----------|----------|---------------------|------|
| `HarveyMod_DeepCuts_Acute/Healing/Recovery` | Глубокие порезы ф1–3 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_FracturedBone_Acute/Cast/Recovery` | Перелом ф1–3 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_Concussion_Acute/Rest/Limited` | Сотрясение ф1–3 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_Shrapnel_Surgery/Healing/Recovery` | Осколки ф1–3 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_TornMuscles_Acute/Healing/Rehab` | Надрыв ф1–3 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_SprainedAnkle_Acute/Recovery` | Растяжение ф1–2 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_BruisedRibs_Acute/Healing` | Ушиб рёбер ф1–2 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_BurnWounds_Acute/Healing` | Ожоги ф1–2 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_InfectedWound_Acute/Treatment` | Инфекция ф1–2 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_BackStrain_Acute/Recovery` | Спина ф1–2 | `buffsCure.json` | ✅ | 🟢 |
| `HarveyMod_Cold_Acute/Recovery` | Простуда ф1–2 | `buffsInjury.json` + cure | ✅ | 🟢 |
| `HarveyMod_BadlyHurt_Acute/Healing/Recovery` | Тяжёлая рана ф1–3 | `buffsCure.json` | ✅ mapped | 🟡 C# doc: «простая» + intensive; phase buffs есть в CP |

### Cure / post-treatment баффы

| buff id | название | источник | C# | риск |
|---------|----------|----------|-----|------|
| `buffHarveyTreatment` | Лечение Харви | `buffsCure.json` | ✅ `CureByInjury` hurt | 🟢 |
| `buffHarveyIntensiveCare` | Интенсивная терапия | `buffsCure.json` | ✅ badly hurt | 🟢 |
| `HarveyMod_BadlyHurt_OutpatientCare` | Восстановление после тяжёлой | `buffsCure.json` | ✅ discharge | 🟢 |
| `buffPostSurgicalCare` | Послеоперационный уход | `buffsCure.json` | ✅ surgical | 🟢 |
| `buffHarveyCare` | Забота Харви | `buffsCure.json` | ✅ recovery | 🟢 |
| `buffHarveyRehab` | Реабилитация | `buffsMedicalCare.json` | ✅ RehabManager | 🟢 |
| `buffHarveyProtection` | Защита Харви | `buffsCure.json` | ⚠️ ref | 🟡 |
| `buffAntibioticsTreatment` | Курс антибиотиков | `buffsCure.json` | ⚠️ ref | 🟢 |
| `buffTeracitin` | Терацитин | `buffsCure.json` | ⚠️ ref | 🟡 event collapse text |
| `buffForcedSedation` | Принудительная седация | `buffsCure.json` | ⚠️ ref | 🟡 |
| `buffHarveyHealing/Recovery/Dropper` | Прочие cure | `buffsCure.json` | ❌ / CP events | 🟡 |
| `HarveyMod_Prescription_*` | Рецепты Rest/NoMine/… | `buffsMedicalCare.json` | ✅ PrescriptionManager | 🟢 |
| `HarveyMod_SelfCare/CleanBandage/WarmTea` | Self-care | `buffsMedicalCare.json` | ✅ SelfCareManager | 🟢 |

**Итого CP injury/medical buffs:** ~70 записей в `Data/Buffs` (+ ~40 stress-only в других файлах).

---

## Events из CP (InjuryCare)

**Debug:** SMAPI `debug ebi <eventId>` — принудительный запуск. Для location-bound событий нужен warp на карту из колонки location (или событие с `none/` + `changeLocation`).

**Связь с InjuryCare:** ✅ C# `PassOutHandler` / topics / triggers · ⚠️ только CP/trigger · ❌ orphan (нет preconditions и C# startEvent)

### Обязательные события (чеклист)

| event id | location | условия запуска | debug `ebi` | InjuryCare | риск зависания/warp/чёрный экран |
|----------|----------|-----------------|-------------|------------|-----------------------------------|
| `eventHarveyMineRescue` | **Mine** | `!seen` rescue variants; C#: pass-out death in mine → `BeginMineRescueWarp` | warp Mine → `debug ebi eventHarveyMineRescue` | ✅ PassOutHandler | 🔴 fade true/false; warp Hospital; coords `(17,7)` — проверять viewport |
| `eventHarveyMineRescueDating` | **Mine** | Dating/Married + `!seen` others; C# prefers if relationship | warp Mine → `debug ebi eventHarveyMineRescueDating` | ✅ PassOutHandler | 🔴 same; тон Dating; opt-out «стоп» |
| `eventHarveyMinorMineRescue` | **Mine** | Dating/Married + `!seen` full rescue; C#: mine pass-out non-severe | warp Mine → `debug ebi eventHarveyMinorMineRescue` | ✅ PassOutHandler | 🟡 короче; Hospital warp |
| `eventHarveyMineInterception` | **Mine** | SpaceCore `triggerHarveyMineWarning`: LocationChanged Mine + injury buff + Dating; **нет** `/Time` в ключе | warp Mine `(17,7)` → `debug ebi eventHarveyMineInterception` | ⚠️ CP trigger only | 🟡 move `0 3` exit; topic `HarveyMineIntercept`; phase buff IDs **не** в trigger list |
| `eventHarveySkullCavePrevention` | **SkullCave** | `triggerLocationReactionSkullCaveExit` / `triggerHarveySkullCaveWarning` (SkullCave + injury buff) | warp SkullCave `(5,5)` → `debug ebi eventHarveySkullCavePrevention` | ⚠️ CP trigger | 🔴 trigger bug: warning иногда с `Mine OR SkullCave` — event только в SkullCave patch |
| `eventStayInHospital` | **Hospital** | **Нет preconditions** (orphan) | warp Hospital → `debug ebi eventStayInHospital` | ❌ C# hosp без event | 🟢 короткая сцена `(9,16)`; недостижимо в gameplay |
| `eventHarveyLateNightCollapse` | **Town** | `Time 2400 2600` + BETAS pass-out Town (`triggersInjuryMail.json`) | warp Town `(37,59)`, time 25:00 → `debug ebi eventHarveyLateNightCollapse` | ⚠️ CP trigger (не C#) | 🔴 collapse anim + warp Hospital; married/dating tone |
| `eventHarveyCheckHealthFarmer` | **Farm** | `Time 600 1200` + `PlayerKilled` seen + Harvey Dating | warp Farm → `debug ebi eventHarveyCheckHealthFarmer` | ❌ vanilla gate only | 🔴 warp Hospital mid-scene; опекунский тон |
| `eventHarveyEmergencyCare` | **Hospital** (script) | C# `PassOutHandler.QueueHospitalEvent(EmergencyCare)` critical pass-out | `debug ebi eventHarveyEmergencyCare` (warp optional) | ✅ C# | 🟡 `changeLocation` from void coords |
| `eventHarveyExhaustion` | **Hospital** (script) | C# `QueueHospitalEvent(Exhaustion)` exhaustion pass-out | `debug ebi eventHarveyExhaustion` | ✅ C# | 🟡 long scene; adds `buffHarveyDropper`, topic |
| `eventHarveyTreatmentCollapse` | **Hospital** (script) | **Orphan** — нет trigger/C# | `debug ebi eventHarveyTreatmentCollapse` | ❌ | 🟡 Teracitin text; fade end |

**Файлы:** mine trio → `eventsMineRescue.json`; interception/skull/emergency/exhaustion → `eventsCare.json`; collapse/check health/treatment/stay → `events.json`.

### Прочие Injury-adjacent события

| event id | location | файл | C# / trigger | InjuryCare |
|----------|----------|------|--------------|------------|
| `HarveyMod_FirstTreatment` | Hospital | `events.json` | C# topic `topicHarveyNeedsFirstTreatment` | ✅ treatment pipeline |
| `HarveyMod_TreatmentPlanMeeting` | Hospital | `events.json` | topic `topicDiagnosisComplete` | ✅ |
| `HarveyMod_NightCrisis_*` | Hospital | `events.json` | CP gates | ⚠️ stress arc |
| `eventRescueOperation` | Woods | `events.json` | `RescueOperationLauncher` + topic | ⚠️ parallel arc |
| `eventHarveyCheckFarmerOutsideAfter22` | Farm | `events.json` | topic `topicPassedOutInTown` | ⚠️ town pass-out follow-up |

Полный каталог локаций: [`docs/events-inventory/01-cp-events-catalog.md`](../events-inventory/01-cp-events-catalog.md).

---

## Dialogue / topics (Injury)

Источники: `dialoguesHarveyInjury.json` (tiered by hearts), `dialoguesHarvey.json` (`Treat_*`, complications), `dialoguesHarveyMedicalCare.json`, `harvey_proximity_injury.json` (C# bubbles).

| topic / dialogue key | условие | файл | травма / механика | риск |
|----------------------|---------|------|-------------------|------|
| `topicHurt` … `topicCold` | `HasConversationTopic` + heart tier | `dialoguesHarveyInjury.json` | базовые traumas | 🟡 ty mix (Вы/ты) по tier |
| `topicBadlyHurt`, `topicHealthDamageCritical` | same | injury + harvey | severe | 🟢 |
| `topicDeepCuts`, `topicFracturedBone`, … | same | injury | phased traumas | 🟢 |
| `topicInfectedWound` | same | injury | infection | 🟢 |
| `topicMineInjuryRescue` | after mine rescue event | injury | mine pipeline | 🟢 |
| `topicHarveyMinorMineRescue` | C# PassOutHandler | injury (check harvey) | minor rescue | 🟢 |
| `topicPassedOutInTown`, `topicFarmerExhausted` | pass-out | injury | exhaustion | 🟢 |
| `topicHarvey_DirtyWound` … `topicHarvey_PainFlare` | complication topics | `dialoguesHarvey.json` | complications | 🟢 |
| `topicHarvey_WetBandage/WetStitches/Neglect` | + `_memory_*` | harvey | complications UX | 🟡 повтор memory lines |
| `topicHarveyNeedsFirstTreatment` | C# before first treat | harvey | bridge → `HarveyMod_FirstTreatment` | 🟢 |
| `topicDiagnosisComplete` | checkup bridge | medical | treatment plan | 🟢 |
| `topicHarvey_CheckupDue`, `topicHarvey_RecoveryCheckupDue` | CheckupManager | `dialoguesHarveyMedicalCare.json` | checkup | 🟢 |
| `topicHarvey_Prescription_*` | PrescriptionManager | medical | prescriptions | 🟢 |
| `topicHarvey_TreatmentPlanGiven` | TreatmentPlanManager | medical | plan mail | 🟢 |
| `topicHarvey_Rehab*` | RehabManager | medical | post-severe | 🟢 |
| `topicHarvey_ForcedHospitalization` | HospitalizationManager | harvey | forced hosp | 🟢 |
| `Treat_Hurt_Before/After` | dialogue keys | `dialoguesHarvey.json` | hurt treatment | 🟢 |
| `Proximity_Complication_*` | C# proximity | proximity json | complications | 🟡 many keys; tier by compliance |
| `Proximity_Injury_*` | C# proximity | proximity json | main injury | 🟡 |
| `HarveyMineIntercept` | mine interception event | eventsCare (topic add) | **не** `topic*` prefix | 🔴 ключ без `topic` — audit/reset? |

**Stress-only topics** (`topicStress*`, `dialoguesHarveyStress.json`) — в `content.json` stress dialogues **закомментированы**; файлы на диске есть.

---

## Mail

| mail id | когда должен прийти | кто отправляет в C# | текст в CP | риск missing |
|---------|---------------------|---------------------|------------|----------------|
| `mailHarveySleepControl` (+ `_Neutral/_Friend/_Dating/_Married`) | Pass-out Town late | `PassOutHandler` → base id; tier via trigger | ✅ `mailInjury.json` | 🟢 tier split |
| `mailHarveyMineForbidden` (+ tier `_LowHearts`…) | Day after mine warning | `GameEventHandler` + tier helper | ✅ injury + tiered | 🟢 |
| `HarveyMod_NeglectWarning` | Phase neglect | `ComplicationManager` | ✅ `mailInjury.json` | 🟢 |
| `HarveyMod_DirtyWoundInfection` | Dirty → infected | `ComplicationManager` | ✅ | 🟢 |
| `HarveyMod_WetBandageInfection` | Wet → infected | `ComplicationManager` | ✅ | 🟢 |
| `HarveyMod_TreatmentUrgentReminder` | Untreated / neglect warn | `ComplicationManager` | ✅ | 🟢 |
| `HarveyMod_TreatmentFinalWarning` | Critical neglect | `ComplicationManager` | ✅ | 🟢 |
| `HarveyMod_CheckupOverdue` (+ tier suffix) | Checkup 4+ days | `CheckupManager` + tier | ✅ injury + tiered | 🟢 |
| `mailHarveyTreatmentPlan_Minor/Severe` (+ tier) | Start treatment | `TreatmentPlanManager` | ✅ | 🟢 |
| `mailHarveyPrescriptionViolation` (+ tier) | Prescription violation | `PrescriptionManager` | ✅ tiered | 🟢 |
| `mailHarveyCheckupReminder` (+ tier) | Checkup due | `CheckupManager` | ✅ tiered | 🟢 |
| `mailHarveyRehabReminder/Completed` (+ tier) | Rehab arc | `RehabManager` | ✅ tiered | 🟢 |
| `mailHarveyNoMineViolation` (+ tier) | NoMine violation | `HarveyMailHelper` | ✅ tiered | 🟢 |
| `mailHarveyKeepDryViolation` (+ tier) | KeepDry violation | `HarveyMailHelper` | ✅ tiered | 🟢 |
| `mailHarveyRestViolation` (+ tier) | Rest violation | `HarveyMailHelper` | ✅ tiered | 🟢 |
| `HarveyMod_WetCare` | Wet bandage (?) | **не вызывается** в C# | ✅ `mailCure.json` | 🟡 CP-only path? |
| `HarveyMod_WetStitchesCare` | Wet stitches (?) | **не вызывается** | ✅ `mailCure.json` | 🟡 |
| `HarveyMod_InfectionAlert` | Infection warn | **не вызывается** | ✅ `mailInjury.json` | 🟡 CP/trigger only |
| `mailHarveyAfterMineRescue` | После full mine rescue | **CP event script** (`mail` cmd) | ✅ `eventsMineRescue.json` | 🟢 |
| `HarveyMod_*Alert`, `*_Phase2` mails | Phase transitions | CP/triggers (не C# MailIds) | ✅ `mailInjury.json`, `mailCure.json` | 🟡 дубли с tier plan |

**Всего mail keys в CP (injury+care+stress):** ~144 (включая tier suffixes).

---

## Несовпадения C# ↔ CP

### C# использует — CP не найден

| ID | тип | деталь |
|----|-----|--------|
| `buffTooCold` | buff | C# `PlayerEventHandler`; **нет** записи в `Data/Buffs` CP |
| — | event | C# **не** вызывает `eventStayInHospital`, `eventHarveyTreatmentCollapse`, `eventHarveyCheckHealthFarmer`, `eventHarveyLateNightCollapse` напрямую (только CP/trigger/vanilla gates) |

### CP содержит — C# не использует (injury-relevant)

| ID | тип | деталь |
|----|-----|--------|
| `buffPainFlare` | buff | Legacy; C# → `HarveyMod_PainFlare` |
| `HarveyMod_Sepsis`, `HarveyMod_ImpairedMobility`, `HarveyMod_MuscleAtrophy`, `HarveyMod_BreathingDifficulty` | buff | Escalation/debuff без C# manager |
| `buffAlcoholPoisoning` | buff | CP mail `HarveyMod_AlcoholWarning` |
| `HarveyMod_BadlyHurt_Acute/Healing/Recovery` | phase buff | В CP есть; C# doc трактует badly hurt как «простая» + intensive — **фактически** `GetPhaseBuffId` mapped |
| `eventHarveyTreatmentCollapse`, `eventStayInHospital` | event | Orphan scripts |
| `HarveyMod_WetCare`, `HarveyMod_WetStitchesCare`, `HarveyMod_InfectionAlert` | mail | Текст есть; C# `MailIds` не шлёт |
| `HarveyMineIntercept` | topic key | Добавляется событием **без** `topic` prefix |
| ~40 `buffStress*` / stress events | buff/event | Parallel stress mod; injury C# не трогает |

### Phase buff id

| Проверка | результат |
|----------|-----------|
| Все ID из `InjuryManager.GetPhaseBuffId` | ✅ найдены в `buffsCure.json` / `buffsInjury.json` (Cold) |
| `buffBadlyHurt` phases | ✅ в CP; ⚠️ документирование C# как «simple» расходится с mapping |

### Cure buff id

| C# `CureByInjury` | CP |
|-------------------|-----|
| `buffHarveyTreatment` | ✅ |
| `buffHarveyIntensiveCare` | ✅ |
| `buffPostSurgicalCare` | ✅ |
| Post-recovery `buffHarveyCare` | ✅ (не в CureByInjury; `CompleteInjuryRecovery`) |
| `HarveyMod_BadlyHurt_OutpatientCare` | ✅ (discharge path) |

### Event ID (`EventIds` constants)

| C# constant | CP event | статус |
|-------------|----------|--------|
| `HarveyMod_FirstTreatment` | ✅ `events.json` | OK |
| `HarveyMod_TreatmentPlanMeeting` | ✅ | OK |
| `eventHarveyEmergencyCare` | ✅ `eventsCare.json` | OK |
| `eventHarveyExhaustion` | ✅ | OK |
| `eventHarveyMinorMineRescue` | ✅ `eventsMineRescue.json` | OK |
| `eventHarveyMineRescueDating` | ✅ | OK |
| `eventHarveyMineRescue` | ✅ | OK |
| `eventRescueOperation` | ✅ `events.json` | OK |

**Не в `EventIds`, но в чеклисте:** mine interception, skull prevention, late collapse, check health, treatment collapse, stay in hospital — все **в CP**, запуск через trigger/vanilla/orphan.

---

## TriggerActions (кратко)

| Trigger id | Trigger | Injury-relevant action |
|------------|---------|------------------------|
| `triggerHarveyMineWarning` | LocationChanged → Mine | `PlayEvent eventHarveyMineInterception` |
| `triggerHarveySkullCaveWarning` | LocationChanged → SkullCave | `PlayEvent eventHarveySkullCavePrevention` |
| `triggerLocationReactionSkullCaveExit` | SkullCave + Dating | skull prevention event |
| `triggerSleepControl_*` | BETAS_PassedOut Town | mail tier + `eventHarveyLateNightCollapse` |
| `triggerEmergencySupervision` | DayStarted | buff `buffEmergencySupervision` |

---

## Что читать следующему чату

1. [00-ai-testing-rules.md](00-ai-testing-rules.md) — цикл MCP/SMAPI, формат TC.
2. [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) — C# ID, `injury_*` команды, пробелы QA.
3. **Этот файл** — [02-cp-content-inventory.md](02-cp-content-inventory.md) — матрица CP buff/event/mail/dialog ↔ C#.
4. **Следующий шаг: аудит debug-команд** — сверить [`FOR_TEST.md`](FOR_TEST.md) + `ModEntry` console/MCP с таблицами выше; добавить пробелы (`injury_force_dirty_wound`, `injury_pass_out_sim`, `injury_event_queue`, tiered mail preview).
5. [main-injury-testcases.md](main-injury-testcases.md) — прогон TC с опорой на оба инвентаря.
6. [injury-mcp.md](injury-mcp.md) + [stardew-mcp.md](stardew-mcp.md) — автоматизация; для событий — ручной `debug ebi` + warp по location из таблицы Events.
7. Блокеры cutscene-тестов: orphan events, SkullCave trigger bug, save/load между warp и `startEvent` (`PassOutHandler` resume flags).
