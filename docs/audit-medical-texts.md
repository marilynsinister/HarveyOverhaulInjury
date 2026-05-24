# Аудит медицинского соответствия текстов (topics / mail / Treat)

Дата: **2026-05-24** (актуализация статуса)  
Источники: `dialoguesHarveyInjury.json`, `dialoguesHarveyCure.json`, `dialoguesHarvey.json`, `mailInjury.json`, `mailCure.json`, C# `TopicIds.GetPhaseTopicId`, [audit-topics-cp-existence.md](./audit-topics-cp-existence.md).

> **Статус синхронизации ID:** P0 medical fixes (2026-05-23) применены в CP. Все C# gameplay topic/mail ID имеют CP keys. Детальные таблицы ниже сохранены как reference; актуальный чеклист — в разделе «Статус приоритетов».

## Фазы в игре

| Метка в документе | CP / C# |
|-------------------|---------|
| **нелеченная** | `topic{Injury}` при активном `buff*` |
| **acute** | фаза 1 (`*PhaseAcute` или alias: `PhaseSurgery`, `PhaseCast`, `PhaseObservation`, `PhaseTreatment`) |
| **healing** | фаза 2 |
| **recovery** | фаза 3 (alias: `PhaseRehab`, `PhaseLimited`) |
| **cured** | `topic{Injury}Cured` после финального осмотра |
| **complication** | `topicHarvey_*`, `Proximity_*`, осложняющие buff |

C# всегда генерирует ID вида `topic{Injury}PhaseAcute|Healing|Recovery`. В CP block1 часть ключей **не совпадает** (`PhaseCast`, `PhaseObservation`, `PhaseTreatment`, `PhaseRehab`) — при несовпадении игрок не получает фазовую реплику (см. раздел «ID-рассинхрон»).

---

## Системные выводы

### 1. Шаблон `Treat_{Injury}_After*` — копипаст между травмами

14 пар Before/After × 12 травм ≈ 168 строк, но **After1–After7 почти идентичны** для всех травм. Отсюда:

| Ошибка | Где | Пример |
|--------|-----|--------|
| «Повязка свежая» при сотрясении | `Treat_Concussion_After1` | У сотрясения нет повязки на голову |
| «Повязка» / «синяк» при ушибе | `Treat_Hurt_After1–2` | `topicHurt` = царапина, не синяк |
| «Закреплю сустав» при царапине | `Treat_Hurt_Before4` | Нет показаний к иммобилизации сустава |
| «Травмировать голову» + вставание | `Treat_Concussion_After2–4` | Нужны: темнота, экраны, без резких нагрузок, не «опирайся и иди» |
| «Травмировать кость» без запрета шахты | `Treat_FracturedBone_After*` | Нет «шахта/тяжёлая работа запрещены» |
| Одинаковый «день отдыха» | все `Treat_*_After7` | Для инфекции нужен курс AB, для перелома — immobilization |

**Before**-блоки чуть разнообразнее, но After — медицински невалидный шаблон.

### 2. Фазовые `topic*Phase*` vs `PhaseTransition_*`

| Источник | Медицинское качество |
|----------|---------------------|
| `PhaseTransition_*` в `dialoguesHarveyInjury.json` | **Образцовое:** конкретная стадия (гипс, швы, костная мозоль, невrologия, AB-курс) |
| `topic*PhaseAcute/Healing/Recovery` в `dialoguesHarveyCure.json` | **Слабое:** «Боже мой!», «под контролем», без дифференциации acute → recovery |
| Письма `HarveyMod_*_Phase2/3` в `mailCure.json` | **Хорошее:** режим, запреты, реабилитация |

**Recovery** в phase-topics звучит так же жёстко, как acute — нарушает требование «мягче, но осторожно».

### 3. Cured-топики

Базовый block1 в целом **OK медицински** (осмотр, рекомендации, без паники). Проблемы:

- «Организм очень хрупкий» / «я буду следить» на **каждой** cured — тональная тревога после выздоровления
- ~~Нет `topicColdCured`, `topicSurgicalWoundCured`~~ — **✅ добавлены 2026-05-23** (см. «Статус приоритетов»)
- ~~`topicSurgicalWoundHealed`~~ — legacy key удалён; C# использует `topicSurgicalWoundCured`

### 4. Осложнения и почта

| Запись | Мед. качество |
|--------|---------------|
| `HarveyMod_WetCare`, `HarveyMod_WetStitchesCare` | **Хорошо:** риск инфекции, чек-лист, не мочить |
| `HarveyMod_InfectionAlert`, `HarveyMod_Infection_Phase2` | **Хорошо:** срочность, AB, не прерывать курс |
| `topicHarvey_WetBandage` (диалог) | **Частично:** смена повязки есть, **риск инфекции не назван явно** |
| `HarveyMod_NeglectWarning` | **Слабо:** нет связи с осложнением раны/срывом AB |

### 5. ID-рассинхрон (фаза ≠ текст)

C# ждёт `PhaseHealing`, CP block1 отдаёт:

- `topicFracturedBonePhaseCast` → phase 2
- `topicConcussionPhaseObservation` → phase 2
- `topicInfectedWoundPhaseTreatment` → phase 2
- `topicShrapnelWoundsPhaseSurgery` → phase 1 (вместо Acute)
- `topicTornMusclesPhaseRehab` → phase 3
- ~~`topicColdPhaseAcute/Healing/Recovery` — отсутствуют~~ — **✅ добавлены 2026-05-23** (alias block1)

---

## Сводка по травмам

| Травма | Нелеченная | Acute/healing | Treat | Cured | Главная мед. проблема |
|--------|------------|---------------|-------|-------|------------------------|
| Hurt | ✓ | — (нефазовая) | ✗ шаблон | ✓ | After = синяк/сустав вместо царапины |
| BadlyHurt | ✓ | — | ✓ частично | ✓ | OK по ране; After5 «ни куда без меня» — не мед., а контроль |
| SprainedAnkle | ✓ | ✓ частично | ✓ | ✓ | Phase/recovery OK; Treat After — копипаст |
| BruisedRibs | ✓ | ✓ | ✓ | ✓ | Дыхание/повязка OK |
| BackStrain | ✓ | ✓ частично | ✓ | ✓ | «Массаж» в topic при остром спазме — спорно |
| DeepCuts | ✓ | ✓ | ✓ Before | ✓ | Швы/чистота OK; phase acute OK |
| BurnWounds | ✓ | ✓ | ✗ | ✓ | **Нет: не мочить, не вскрывать пузыри, риск инфекции** |
| InfectedWound | ✓✓ | ✓ | ✓ частично | ✓ | Topic/mail сильные; Treat After без акцента на AB |
| TornMuscles | ✓ | ✓ | ✓ | ✓ | Иммобилизация OK |
| Concussion | ✓✓ | ✓ частично | ✗✗ | ✓ | **Treat After1 «повязка»**; recovery слишком жёсткий |
| FracturedBone | ✓ | ✓ ID | ✓ After1 | ✓ | Гипс OK; **нет запрета шахты** в topic/Treat |
| ShrapnelWounds | ✓ | ✓ ID | ✓ | ✓ | Операция/осколки OK |
| SurgicalWound | **✅ topic** | ✓ | ✓ | **✅ Cured** | Base + cured добавлены 2026-05-23 |
| Cold | ✓ | **✅ Phase*** | — | **✅ Cured** | Фазы + cured добавлены 2026-05-23 |

---

## Детальные таблицы

### buffHurt / topicHurt

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicHurt` | нелеченная | «Поцарапалась? Покажи, где болит. Обработаю.» | **да** | — | «Не спорь» в других ключах — здесь OK | — |
| `Treat_Hurt_Before1` | лечение (до) | «Обработаю, наложу повязку…» | **да** | — | контроль | — |
| `Treat_Hurt_Before4` | лечение (до) | «Закреплю сустав…» | **нет** | Царапина не требует фиксации сустава | шаблон | «Проверю, что сустав не задет. Повязка + антисептик.» |
| `Treat_Hurt_After1` | лечение (после) | «Синяк держится…» | **нет** | Hurt ≠ синяк | шаблон | «Кожа краснеет вокруг царапины — нормально. Не мочи повязку.» |
| `Treat_Hurt_After2–7` | лечение (после) | Отдых, «не отпущу», синяк | **частично** | Смешение ушиб/царапина | гиперопека | Сократить до 2–3 After с акцентом: чистота, покой, смена повязки |
| `topicHurtCured` | cured | «Рана зажила… осторожнее в саду/лесу» | **да** | — | «хрупкая» в hearts 6–10 block | «Повязку снял, заживление хорошее. Перчатки при работе.» |

### buffBadlyHurt / topicBadlyHurt

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicBadlyHurt` | нелеченная | «Серьёзные травмы. Немедленно лечение.» | **да** | — | OK | — |
| `Treat_BadlyHurt_Before1–4` | лечение | Осмотр, повязка, кровотечение | **да** | Чистота, покой | — | — |
| `Treat_BadlyHurt_After4` | лечение | «Чистота и покой — главные задачи» | **да** | — | — | — |
| `Treat_BadlyHurt_After5` | лечение | «Никуда без меня» | **частично** | Медицински — постель/наблюдение, не эскорт | контроль | «Сегодня только дом и клиника. При усилении боли — сразу ко мне.» |
| `topicBadlyHurtCured` | cured | «Травма зажила… не жди слабость» | **да** | — | «хрупкий организм» | «Осмотр завершён. Лёгкие нагрузки постепенно.» |

### buffSprainedAnkle / topicSprainedAnkle

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicSprainedAnkle` | нелеченная | «Растяжение… костыли, лёд, не наступать» | **да** | RICE OK | «донесу» — тон | — |
| `topicSprainedAnklePhaseAcute` | acute | «Отёк! Эластичная повязка, никаких прогулок» | **да** | — | «Опять!» | «Отёк выражен. Бинт + лёд. Нагрузку на стопу убираем.» |
| `topicSprainedAnklePhaseRecovery` | recovery | «Связки восстанавливаются… контроль» | **частично** | Нет постепенной нагрузки | жёстко для recovery | «Можешь осторожно наступать. Бандаж ещё несколько дней.» |
| `PhaseTransition_SprainedAnkle_2` | healing | «Отёк спадает… нагружать осторожно» | **да** | Образец | нейтрально | Использовать как модель для `topic*Phase*` |
| `Treat_SprainedAnkle_Before1` | лечение | «Фиксация, холод, не шагать» | **да** | — | — | — |
| `Treat_SprainedAnkle_After*` | лечение | Копипаст After | **частично** | After1 OK (лёд); After2–7 шаблон | — | Оставить 2–3 уникальных After про лодыжку |
| `topicSprainedAnkleCured` | cured | «Растяжение прошло… не бегай по лесу» | **да** | — | — | — |
| `HarveyMod_SprainedAnkle_Phase2` (mail) | healing | «Отёк спал… бандаж, упражнения» | **да** | — | — | — |

### buffBruisedRibs / topicBruisedRibs

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicBruisedRibs` | нелеченная | «Ушиб рёбер… переломов нет, повязка, покой» | **да** | Дыхание, фиксация | «хрупкая» | — |
| `topicBruisedRibsPhaseAcute` | acute | «Никаких глубоких вдохов! Повязка» | **да** | — | паника «Боже мой» | «Дыши мелко. Обхват грудной клетки снимет боль при кашле.» |
| `topicBruisedRibsPhaseHealing` | healing | «Рёбра заживают… дыхание каждый день» | **частично** | OK | recovery-tone missing | Смягчить: «Вдох глубже, чем вчера — хороший знак.» |
| `PhaseTransition_BruisedRibs_2` | healing | «Дыхание ровнее… покой» | **да** | — | — | — |
| `Treat_BruisedRibs_*` | лечение | Повязка, дыхание, обезболивание | **да** | — | After — шаблон | — |
| `topicBruisedRibsCured` | cured | «Рёбра срослись… не поднимай тяжёлое» | **да** | «Срослись» для ушиба — **неточность** | — | «Ушиб зажил. Тяжести и резкие повороты ещё неделю без.» |
| `HarveyMod_BruisedRibs_Phase2` (mail) | healing | «Гематома рассасывается… щадящий режим» | **да** | — | — | — |

### buffBackStrain / topicBackStrain

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicBackStrain` | нелеченная | «Спазм… массаж и восстановление» | **частично** | При остром спазме — покой/тепло **до** глубокого массажа | — | «Спазм. Сначала покой и тепло; лёгкая растяжка — когда отпустит.» |
| `topicBackStrainPhaseAcute` | acute | Копия ribs acute | **частично** | «Повязка на спину» — не главное при strain | — | «Не наклоняйся. Обезболивающее + тепло на поясницу.» |
| `PhaseTransition_BackStrain_2` | healing | «Спазм снят… не перенапрягайся» | **да** | — | — | — |
| `Treat_BackStrain_*` | лечение | Повязка, мазь | **частично** | Дублирует ribs/ankle | — | Уникализировать под спину |
| `topicBackStrainCured` | cured | «Растяжение прошло… перерывы» | **да** | — | — | — |

### buffDeepCuts / topicDeepCuts

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicDeepCuts` | нелеченная | «Глубокие порезы… швы, смена повязок» | **да** | Швы, артерии | — | — |
| `topicDeepCutsPhaseAcute` | acute | «Накладываю швы! Никаких движений» | **да** | — | паника | «Нужны швы. Не мочи рану. Перевязка ежедневно.» |
| `topicDeepCutsPhaseHealing` | healing | «Швы заживают… контроль» | **да** | — | жёстко | «Края сходятся. Следи за покраснением — признак инфекции.» |
| `topicDeepCutsPhaseRecovery` | recovery | «Швы сняты, кожа восстанавливается» | **да** | — | «никаких рисков» — чуть жёстко | «Швы снял. Кожа ещё тонкая — перчатки обязательны.» |
| `PhaseTransition_DeepCuts_2/3` | healing/recovery | Швы, рубец | **да** | Образец | — | — |
| `Treat_DeepCuts_Before1–7` | лечение | Осмотр, швы | **да** | Before OK | — | — |
| `Treat_DeepCuts_After*` | лечение | Шаблон + «острые предметы» | **частично** | After2 OK | — | After: чистота, не мочить, признаки инфекции |
| `topicDeepCutsCured` | cured | «Порезы зажили… острые предметы» | **да** | — | — | — |
| `HarveyMod_DeepCuts_Phase2` (mail) | healing | «Швы чистые… мочить нельзя» | **да** | — | — | — |

### buffBurnWounds / topicBurnWounds

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicBurnWounds` | нелеченная | «Ожоги… мазь, этапы заживления» | **частично** | **Нет: не мочить, не вскрывать пузыри, стерильная повязка, риск инфекции** | «хрупкая» | «Ожог. Охладила уже? Сейчас стерильная повязка и мазь. Не мочи и не трогай пузыри — риск инфекции.» |
| `topicBurnWoundsPhaseAcute` | acute | «Обрабатываю раны!» | **частично** | Нет запрета воды/трения | паника | «Сначала прохладная вода 10 мин (если не делала). Потом повязка без давления.» |
| `topicBurnWoundsPhaseHealing` | healing | «Ожоги заживают…» | **частично** | Нет солнца/жары (есть в mail) | — | «Новая кожа нежная. Солнце и печь — табу. Мазь три раза в день.» |
| `PhaseTransition_BurnWounds_2` | healing | «Новая кожа… мази продолжаем» | **да** | — | — | — |
| `Treat_BurnWounds_Before/After` | лечение | «Обработаю ожоги»; After «не трогай повязки» | **частично** | After2 OK; **нет охлаждения/инфекции** | — | Before: «Проверю глубину. Пузыри не вскрываю.» |
| `topicBurnWoundsCured` | cured | «Ожоги прошли… осторожнее с огнём» | **да** | — | — | — |
| `HarveyMod_Burns_Phase2` (mail) | healing | «Регенерация… солнце запрещено» | **да** | — | — | — |

### buffInfectedWound / topicInfectedWound

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicInfectedWound` | нелеченная | «Воспаление… 39°, антибиотики каждые 6 ч» | **да** | Срочность, AB, температура | — | — |
| `buffAntibioticsTreatment` | healing | «AB действуют… курс до конца» | **да** | — | — | — |
| `topicInfectedWoundPhaseAcute` | acute | «AB немедленно… лично слежу» | **да** | — | «Боже мой» | «Температура и покраснение — начинаем AB. Курс без перерыва.» |
| `topicInfectedWoundPhaseTreatment` | healing (CP name) | «Инфекция отступает… контроль» | **частично** | **C# ждёт `PhaseHealing`** — ключ не совпадёт | — | Переименовать + добавить «продолжай AB, даже если лучше» |
| `Treat_InfectedWound_After5` | лечение | «Температура или боль — зови» | **да** | — | — | — |
| `Treat_InfectedWound_After1–4,6–7` | лечение | Шаблон «повязка/вставай» | **нет** | After3 «инфекция отступает» рано на 1-м визите | — | After: «Первый укол AB. Завтра — контроль температуры.» |
| `topicInfectedWoundCured` | cured | «Инфекция ушла, AB сработали» | **да** | — | «сразу приходи» — OK профилактика | — |
| `HarveyMod_InfectionAlert` (mail) | complication | «Температура — немедленно. AB вложены» | **да** | — | — | — |
| `HarveyMod_Infection_Phase2` (mail) | healing | «Температура спала… курс до конца» | **да** | — | — | — |
| `PhaseTransition_InfectedWound_2` | healing | «Инфекция отступает… AB до конца» | **да** | — | — | — |

### buffTornMuscles / topicTornMuscles

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicTornMuscles` | нелеченная | «Надрыв… фиксация, покой» | **да** | Immobilization | — | — |
| `topicTornMusclesPhaseAcute/Healing` | acute/healing | Иммобилизация, контроль | **да** | — | жёстко | — |
| `PhaseTransition_TornMuscles_3` | recovery | «Лёгкая растяжка… без фанатизма» | **да** | — | — | — |
| `Treat_TornMuscles_*` | лечение | Фиксация, покой | **да** | Before OK | After — шаблон | — |
| `topicTornMusclesCured` | cured | «Мышцы восстановились… разминка» | **да** | — | — | — |

### buffConcussion / topicConcussion

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicConcussion` | нелеченная | «Зрачки… полный покой, без света/звуков, рефлексы» | **да** | Не «отдохни и всё» | — | — |
| `topicConcussionPhaseAcute` | acute | «В постель! Не вставать!» | **да** | — | «безрассудство» | «Сотрясение. Темная комната, минимум экранов. Проверю реакцию зрачков каждые несколько часов.» |
| `topicConcussionPhaseObservation` | healing (CP) | «Домашний режим… проверка каждый час» | **да** | **C#: `PhaseHealing`** | жёстко | Alias + смягчить: «Симптомы слабее. Движение по дому — да, шахта и бег — нет.» |
| `topicConcussionPhaseRecovery` | recovery | «Финальная стадия… никаких рисков» | **частично** | Нет гradуальной нагрузки | **слишком тревожно для recovery** | «Головокружение реже. Лёгкие дела — можно; удары, алкоголь, шахта — ещё нет.» |
| `PhaseTransition_Concussion_2/3` | healing/recovery | Головная боль, невrologия | **да** | Образец | — | — |
| `Treat_Concussion_Before1–7` | лечение | Зрачки, покой | **да** | Before OK | — | — |
| `Treat_Concussion_After1` | лечение | **«Повязка свежая»** | **нет** | **Критическая ошибка копипаста** | — | «Острый период. Сегодня — только лежать, пить воду, без экранов.» |
| `Treat_Concussion_After2–4` | лечение | «Вставай, опирайся на меня» | **нет** | При сотрясении — минимум вертикализации в acute | — | «Если встаёшь — медленно, при головокружении сразу ложись.» |
| `topicConcussionCured` | cured | «Сотрясение прошло… не работай на высоте» | **да** | — | — | — |
| `HarveyMod_Concussion_Phase2/3` (mail) | healing/recovery | Экраны, без бега/шахт | **да** | — | — | — |

### buffFracturedBone / topicFracturedBone

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicFracturedBone` | нелеченная | «Перелом… гипс, покой недели» | **да** | Immobilization | — | Добавить: «Шахта и тяжёлая работа запрещены до снятия гипса.» |
| `topicFracturedBonePhaseAcute` | acute | «Шина, рентген, полный контроль» | **да** | — | — | — |
| `topicFracturedBonePhaseCast` | healing (CP) | «Гипс… не снимать» | **да** | **C#: `PhaseHealing`** | — | Alias `PhaseHealing` = тот же текст |
| `topicFracturedBonePhaseRecovery` | recovery | «Кость почти срослась… каждый шаг» | **частично** | Реабилитация не названа | жёстко | «Гипс скоро снимем. Потом — лёгкие упражнения по моей схеме.» |
| `PhaseTransition_FracturedBone_2/3` | healing/recovery | Костная мозоль, снятие гипса | **да** | — | — | — |
| `Treat_FracturedBone_After1` | лечение | «Гипс наложен… покой» | **да** | — | — | — |
| `Treat_FracturedBone_After5–7` | лечение | Шаблон без шахты | **частично** | **Нет запрета шахты/нагрузки** | — | «Гипс на N недель. Шахта, прыжки, тяжёлые мешки — нет.» |
| `topicFracturedBoneCured` | cured | «Кость срослась, гипс снимаем… без тяжёлой работы» | **да** | — | — | — |
| `mailHarveyMineForbidden` | complication | «Шахта запрещена при серьёзных ранах» | **да** | — | — | Связать явно с переломом в topic |
| `HarveyMod_FracturedBone_Phase2/3` (mail) | healing/recovery | Гипс, реабилитация | **да** | — | — | — |

### buffShrapnelWounds / topicShrapnelWounds

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicShrapnelWounds` | нелеченная | «Осколки… операция, удаление» | **да** | — | — | — |
| `topicShrapnelWoundsPhaseSurgery` | acute (CP name) | «В операционную!» | **да** | **C#: `PhaseAcute`** | — | Duplicate key `topicShrapnelWoundsPhaseAcute` |
| `topicShrapnelWoundsPhaseHealing` | healing | «Осколки удалены… швы» | **да** | Инфекция — в mail | — | — |
| `Treat_ShrapnelWounds_After6` | лечение | «Проверю на признаки инфекции» | **да** | — | — | — |
| `topicShrapnelWoundsCured` | cured | «Осколочные раны зажили… шрамы» | **да** | — | — | — |
| `HarveyMod_Shrapnel_Phase2/3` (mail) | healing/recovery | Перевязки, швы, мази | **да** | — | — | — |

### buffSurgicalWound / topicSurgicalWound

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicSurgicalWound` | нелеченная | **ОТСУТСТВУЕТ** | **нет** | Нет базовой реплики при buff | — | «Послеоперационный шов. Покажи линию швов — проверю на покраснение и отёк.» |
| `topicSurgicalWoundPhaseAcute–Recovery` | фазы | Осмотр швов, заживление | **да** | — | — | — |
| `Treat_SurgicalWound_*` | лечение | Осмотр стежков, смена повязки | **да** | — | After — шаблон | — |
| `topicSurgicalWoundHealed` | cured (не Cured) | «Рана зажила… постепенно в жизнь» | **частично** | C# ждёт `topicSurgicalWoundCured` | — | Добавить `topicSurgicalWoundCured` с тем же смыслом |
| `topicPostOperativeCare` | сопут. | «Смена повязок, рекомендации» | **да** | — | — | — |

### buffCold / topicCold

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicCold` | нелеченная (acute) | «Простуда… 38° (dating), лёгкие, жаропонижающее» | **да** | — | pet names в dating | — |
| `topicColdPhaseAcute/Healing/Recovery` | фазы | **ОТСУТСТВУЮТ** | **нет** | C# ставит фазовые topic | — | Acute: «Температура, покой, тёплое питьё»; Healing: «Температура норма, остаточный кашель» |
| `topicColdPhase1Ready` | → recovery | «Температура спала… остаточный кашель» | **да** | — | romance | — |
| `topicColdRecoveryReady` | recovery ready | «Полностью выздоровела» | **да** | — | — | — |
| `PhaseTransition_Cold_2` | healing | «Температура норма, кашель нормален» | **да** | — | — | — |
| `topicColdCured` | cured | **ОТСУТСТВУЕТ** | **нет** | — | — | «Лёгкие чистые. Можно в поле, но не переохлаждайся.» |

---

## Осложнения (WetBandage / DirtyWound / WetStitches / Neglect / AllergicRash / PainFlare)

| ID | Фаза | Текущий текст (сокр.) | Соответствует | Мед. проблема | Тональная проблема | Предлагаемая замена |
|----|------|----------------------|---------------|---------------|-------------------|---------------------|
| `topicHarvey_WetBandage` | complication | «Промокла… сухая повязка, не спорь» | **частично** | **Не назван риск инфекции** | OK | «Мокрая повязка = бактерии. Смени на сухую стерильную. При жаре или покраснении — сразу ко мне.» |
| `topicHarvey_DirtyWound` | complication | «Шахты с открытой раной… промоем» | **да** | Грязь → инфекция | OK | — |
| `topicHarvey_WetStitches` | complication | «Со швами в бассейне… обработаю заново» | **да** | Разрыв швов, инфекция | OK | — |
| `topicHarvey_Neglect` | complication | «Игнорируешь лечение… отвезу в клинику» | **частично** | Нет последствий (срыв AB, гной) | личное | «Пропуск перевязок ведёт к инфекции. Сегодня — осмотр в клинике.» |
| `topicHarvey_AllergicRash` | complication | «Пыльца при швах… антигистамин» | **да** | Контекст швов OK | — | — |
| `Proximity_PainFlare` | complication | «Боль вернулась? Обезболивающее» | **частично** | PainFlare для перелома/осколков (C#) — **нет контекста кости/шторма** | — | «При переломе/старой ране боль на погоду — норма. Обезболивающее; если не поможет — осмотр.» |
| `HarveyMod_WetCare` (mail) | complication | Чек-лист, покраснение → клиника | **да** | — | — | Синхронизировать с dialog `WetBandage` |
| `HarveyMod_WetStitchesCare` (mail) | complication | Критично, не трём, температура → срочно | **да** | — | — | — |
| `HarveyMod_NeglectWarning` (mail) | complication | «Игнорируешь лечение… жду в клинике» | **частично** | Нет мед. последствий | OK | «Пропуск процедур повышает риск осложнений. Сегодня — перевязка в клинике.» |

---

## Cured-топики (сводная)

| ID | Соответствует состоянию | Мед. проблема | Тональная проблема | Предлагаемая правка |
|----|-------------------------|---------------|-------------------|---------------------|
| `topicHurtCured` | да | — | «хрупкая» в 6–10 block | Финальный осмотр без тревоги |
| `topicBadlyHurtCured` | да | — | «хрупкий организм» | — |
| `topicSprainedAnkleCured` | да | — | — | — |
| `topicBruisedRibsCured` | частично | «срослись» для ушиба | — | «Ушиб зажил» |
| `topicBackStrainCured` | да | — | — | — |
| `topicDeepCutsCured` | да | — | — | — |
| `topicBurnWoundsCured` | да | — | — | — |
| `topicTornMusclesCured` | да | — | — | — |
| `topicConcussionCured` | да | — | — | — |
| `topicFracturedBoneCured` | да | — | «буду следить» | «Рентген чистый. Нагрузки — постепенно.» |
| `topicShrapnelWoundsCured` | да | — | — | — |
| `topicInfectedWoundCured` | да | — | лёгкая тревога OK | — |
| `topicColdCured` | **нет ключа** | — | — | Добавить (см. Cold) |
| `topicSurgicalWoundCured` | **нет ключа** | — | — | = `SurgicalWoundHealed` или alias |

---

## PhaseTransition_* (эталон — сохранить)

Ключи в `dialoguesHarveyInjury.json` (`PhaseTransition_SprainedAnkle_2`, `PhaseTransition_Concussion_2`, …) **медицински точны** и корректно смягчают тон по фазам. Рекомендация: **переписать `topic*Phase*` в cure по их образцу**, а не наоборот.

Пример для `topicBurnWoundsPhaseHealing` (замена):

> «Воспаление спало, идёт регенерация кожи. Мазь — утро, день, вечер. Солнце и горячая печь запрещены. Новая кожа хрупкая — не трогай и не мочи.»

Пример для `topicConcussionPhaseRecovery` (замена):

> «Головокружение почти не беспокоит. Разрешаю лёгкие дела по дому. Без бега, шахт и алкоголя ещё несколько дней. При головной боли — сразу ко мне.»

---

## Приоритеты правок

| P | Задача |
|---|--------|
| **P0** | Убрать копипаст `Treat_*_After*`: отдельные тексты для Concussion (без «повязки»), Infected (AB), Fractured (гипс + шахта), Burn (не мочить) |
| **P0** | Добавить `topicSurgicalWound`, `topicColdCured`, `topicSurgicalWoundCured`; фазовые `topicColdPhase*` |
| **P0** | Alias фазовых ключей: `PhaseCast`→`PhaseHealing`, `PhaseObservation`→`PhaseHealing`, `PhaseTreatment`→`PhaseHealing`, `PhaseSurgery`→`PhaseAcute`, `PhaseRehab`→`PhaseRecovery` |
| **P1** | Переписать `topic*PhaseRecovery` мягче; acute — меньше «Боже мой», больше процедур |
| **P1** | `topicBurnWounds` + Treat: явно «не мочить, не вскрывать пузыри, риск инфекции» |
| **P1** | `topicHarvey_WetBandage`: явный риск инфекции (как в mail) |
| **P2** | `topicBruisedRibsCured`: «срослись» → «зажили» |
| **P2** | `topicBackStrain`: отложить «массаж» до healing-фазы |
| **P2** | `Proximity_PainFlare`: контекст перелома/метeo |

---

## Исправления CP (2026-05-23, медицинский аудит)

Правки **только в CP JSON** (C# не менялся). Скрипты: `tmpMap/fix_medical_texts.py`, `tmpMap/fix_medical_texts_round2.py`.

### Выполнено (P0 / P1)

| Область | Файл | Что изменено |
|---------|------|--------------|
| `Treat_Hurt_*` | `dialoguesHarveyCure.json` | Before4 без фиксации сустава; After1–2 про царапину, не синяк |
| `Treat_Concussion_After1–4` | cure | Убрана «повязка»; покой, экраны, минимум вертикализации |
| `Treat_InfectedWound_After1–4,6–7` | cure | AB-курс, температура, перевязки — вместо копипаста сотрясения |
| `Treat_FracturedBone_After1,5–7` | cure | Запрет шахты/нагрузки, контроль гипса |
| `Treat_BurnWounds_Before1`, `After2` | cure | Охлаждение, не мочить, не вскрывать пузыри |
| `Treat_BadlyHurt_After5` | cure | Дом/клиника вместо «ни куда без меня» |
| `Treat_*_After1` (лодыжка, спина, порезы, осколки) | cure | Уникальные тексты вместо «Повязка свежая…» |
| `topicBurnWounds`, `topicBackStrain`, `topicFracturedBone` | `dialoguesHarveyInjury.json` | Мед. протокол: ожог/спазм/перелом + запрет шахты |
| `topic*Phase*` (block1) | cure | Acute/healing/recovery переписаны по образцу PhaseTransition: процедуры, без «Боже мой» |
| `topicInfectedWoundPhaseTreatment` | cure | Восстановлен alias-ключ (дубль PhaseHealing исправлен) |
| `topic*Cured` (block1) | cure | `topicBruisedRibsCured`: «зажил» вместо «срослись»; `topicHurtCured`, `topicFracturedBoneCured` — финальный осмотр без паники |
| `Proximity_PainFlare` | cure | Контекст перелома/метeo |
| `topicHarvey_WetBandage`, `topicHarvey_Neglect` | `dialoguesHarvey.json` | Явный риск инфекции / срыв AB |
| `mailHarvey_Neglect`, `HarveyMod_NeglectWarning` | `mailInjury.json` | Последствия пропуска перевязок и AB |
| `Treat_Hurt_After` | `dialoguesHarvey.json` | Царапина, не синяк |
| Dating/Married `topicBackStrain`, `topicBurnWounds`, `topicFracturedBone` | injury | Мед. содержание сохранено при романтическом тоне |

### Частично / осталось (2026-05-24)

| Область | Статус |
|---------|--------|
| `Treat_*_After3–7` (не P0-травмы) | **LOW** — generic follow-up; P0 After1–2 уникализированы |
| `topicSurgicalWound`, `topicColdCured`, Cold phases | **✅ done** (2026-05-23) |
| Alias фаз block1 | **✅ done** — duplicate keys PhaseHealing/Acute/Recovery |
| Cured в Hearts 6–10 / Dating / Married | Pet names **by design**; ribs «срослись»→«зажил» в block1 |
| `topicHarveyMinorMineRescue` | **нет dialogue** — C# launcher 2026-05-24 |
| Events / quests «Боже мой» | Экстренные сцены — вне scope |

### Статус приоритетов (актуально)

| P | Задача | Статус |
|---|--------|--------|
| **P0** | Treat After copy-paste (Concussion, Infected, Fractured, Burn, Hurt) | **✅ done** |
| **P0** | Base topics Burn/Back/Fracture + Cold/Surgical keys | **✅ done** |
| **P0** | Phase alias block1 | **✅ done** |
| **P1** | Phase recovery tone; WetBandage/Neglect | **✅ done** |
| **P2** | Ribs cured, BackStrain, PainFlare | **✅ done** |
| **LOW** | Generic Treat After3–7 для второстепенных травм | **открыто** |

---

## Методология

- Проверены все `topic*` травм в `dialoguesHarveyInjury.json`, все `Treat_*`, `topic*Phase*`, `topic*Cured` в `dialoguesHarveyCure.json`, осложнения в `dialoguesHarvey.json`, почта `mailInjury.json` + `mailCure.json`.
- `Treat_*` с одинаковым After-шаблоном оценены как группа + injury-specific exceptions.
- Сверка фаз с C# `GetPhaseTopicId` и `GetPhaseBuffId` (`InjuryManager.cs`).
- Код не изменялся.
