# Аудит тона Харви по уровням отношений (topics / mail / events)

Дата: **2026-05-25** (актуализация: таблица стадий)  
Источники: [audit-topics-cp-existence.md](./audit-topics-cp-existence.md), [audit-mail-cp-existence.md](./audit-mail-cp-existence.md), CP-файлы `dialoguesHarvey*.json`, `mail*.json`, `events*.json`.

> Детальные таблицы проблем (2026-05-23) сохранены как reference. Актуальный статус исправлений — в разделе «Статус приоритетов (2026-05-24)».

**250 friendship points = 1 сердце.**

---

## Таблица стадий (канон)

| Стадия | ♥ / gate | Тон Харви | Обращение | Допустимо | Запрещено до следующей стадии |
|--------|----------|-----------|-----------|-----------|-------------------------------|
| **0 — врач / знакомый** | 0–2♥, `< 750` | Профессионально, осторожно | **«Вы»** | Осмотр, протокол, мягкое приглашение в клинику | Pet names, «хрупкая», «не отпущу», романтика, **«ты»** |
| **1 — доверие** | 3–5♥, `750–1249` | Мягкая забота, личное беспокойство | **«Ты»** | Настойчивый врач, договорённости, follow-up | **«Вы»**, романтические обращения, «люблю», «моя» |
| **2 — близкий друг** | 6–8♥, `1500–1999` | Тревога сильнее, тепло без romance | **«Ты»** (без ласковых имён) | «Я рядом», «беспокоюсь», контроль **с** opt-out | **«Вы»**, **«люблю»**, **«моя/мой»**, pet names, поцелуи как romance |
| **2b — почти партнёр** | 9–10♥ до букета, `2000+` | Как 6–8, но глубже trust | **«Ты»** | Сильная опека, «ты важна» | **«Вы»**, romance-маркеры только после букета |
| **3 — Dating** | `Relationship: Dating` | Романтическая забота | **«Ты»** + ласковые | «Солнышко», «дорогая», объятия, поцелуи (с согласия) | Лишение выбора без escape hatch |
| **4 — Married** | `Relationship: Married` | Максимальная нежность + гиперопека | «Ты» + pet names | «Котёнок», «девочка моя», домашняя близость | Контроль **без** выхода (запреты без «остановите меня» / opt-out) |

### CP / events — технический маппинг

| Стадия | CP `When` / `Friendship` |
|--------|---------------------------|
| 0–2♥ | без `When`, `< 750`, injury `Hearts: 0–2`, story **E1** (`500`) |
| 3–5♥ | `750+`, `Hearts: 3–5`, story **E2–E4** |
| 6–8♥ | `1500+`, `Hearts: 6–8`, events E4B–E7 |
| 9–10♥ (pre-dating) | `2000+`, `Hearts: 8–10` **без** pet names |
| Dating | `Relationship: Dating` |
| Married | `Relationship: Married`, `MarriageDialogueHarvey` |

### Story-arc (`HarveyOverhaulStory.*`) — дополнение к стадиям

Trust-линия E4–E13 **до букета** сознательно держит **«Вы»** (согласие, границы, `topicHarveyTrust_*`) — это не отменяет стадию 6–8, а уточняет *жанр* сцены. Переход на **«ты»** в story — в split-версиях (`E10_Dating`, `E12_Dating`, `E15_Married`) и в `HarveyOverhaulRomance.*`. Реплики фермерши в `message` / `quickQuestion` («ты остановилась…») — POV игрока, не обращение Харви.

**Запреты до Dating (все каналы):** «люблю», «моя/мой/моё», «солнышко», «малышка», «котёнок», «дорогая» (как pet name), сцены-свидания, поцелуи как romance.

---

## Шкала уровней (legacy, для старых таблиц аудита)

| Уровень | SDV | ≈ стадия |
|---------|-----|----------|
| 0 | 0♥ | 0 |
| 1 | 1–2♥ | 0 |
| 2 | 3–4♥ | 1 |
| 3 | 5–6♥ | 1–2 |
| 4 | 7–8♥ | 2 |
| 5 | 9–10♥ | 2b |
| 6 | Dating | 3 |
| 7 | Married | 4 |

---

## Системные выводы (архитектура CP)

### 1. «Базовый блок без When» — главная проблема

Три крупных файла патчат `Characters/Dialogue/Harvey` **без условий по сердцам**:

| Файл | Что попадает на 0–3♥ |
|------|----------------------|
| `dialoguesHarvey.json` (блок 1) | `Introduction`, осложнения, `Hospital_Mon`…`Sun` (ультимативный тон), `Resort_*`, `FlowerDance_Accept`, event-memory, `dating_Harvey` / `married_Harvey` (vanilla keys) |
| `dialoguesHarveyInjury.json` (блок 1) | Все `topic*` травм, `PhaseTransition_*` |
| `dialoguesHarveyCure.json` (блок 1) | `topic*Cured`, `Treat_*`, фазовые `topic*Phase*`, `Proximity_*` |

**Итог:** при первой травме на 0–2♥ игрок получает тот же гиперопекающий текст, что и на 8–10♥. Лестница отношений **не работает** для injury/cure pipeline.

### 2. Перекрытие блоков по hearts — частично есть, но поздно

`dialoguesHarvey.json` имеет градацию:
- 4–7♥ — мягче, без ласковых имён
- 8–10♥ — «малышка», «котёнок», «не отпущу»
- Dating / Married — «солнышко», «девочка моя»

Но **injury/cure topics не участвуют** в этой градации (только отдельные `Hospital_*` при `HasConversationTopic: topicHurt`).

### 3. Почта — без gates

Все C#-письма (`mailInjury.json`) без `When` по hearts/relationship. `mailHarveySleepControl` звучит опекунски при **любом** уровне (C# шлёт без проверки dating).

### 4. Речь фермерши

**Хорошо:** сюжетные events (`HarveyOverhaulStory.*`, `HarveyMod_FirstTreatment`) используют `quickQuestion` + `message` / `emote farmer` — фермер почти молчит или выбирает коротко.

**Плохо:** в `dialoguesHarvey.json` base есть длинные `message` от фермерши в event-memory (`eventSeen_*`), но это реплики **после** vanilla-событий, не injury-topics.

---

## Лестница тона (ожидание vs факт)

| Стадия | Ожидаемый тон Харви | Факт для injury/mail (2026-05-25) |
|--------|---------------------|-----------------------------------|
| 0–2♥ | Врач, «Вы», без pet names | **✅** fallback injury/cure после P0-fix |
| 3–5♥ | Доверие, мягкая забота, без romance | **✅** blocks `Hearts: 3–5` |
| 6–8♥ | Близкий друг, тревога, без «люблю/моя» | **⚠️** `Hearts: 8–10` в dialoguesHarvey — проверить pet names |
| 9–10♥ pre-dating | Как 6–8, сильнее опека | **⚠️** vanilla events иногда romance без gate |
| Dating | «Солнышко», объятия, «ты» | **✅** Dating-блоки |
| Married | Нежность + гиперопека **с** opt-out | **✅** E15 «остановите меня»; контроль в topics — отдельный баланс |

---

## Таблица проблем (приоритетные записи)

Формат: **Подходит** = да / частично / нет.

### A. Injury topics — base block (`dialoguesHarveyInjury.json`, без When)

| ID | Файл | Текущий уровень условий | Фактический тон | Подходит | Что не так | Нужный уровень | Правка условия | Правка текста |
|----|------|-------------------------|-----------------|----------|------------|----------------|----------------|---------------|
| `topicHurt` … `topicCold` (все базовые) | dialoguesHarveyInjury.json | 0 (все ♥) | Настойчивый врач + «хрупкая», «не спорь», «не оставлю одну» | **нет** | Контроль и телесные оценки до знакомства; нет профессиональной дистанции на 0–2♥ | 0–1: клиника; 2–4: текущий minus pet/control; 5+: усиление | Разбить на `Hearts 0–2` / `3–5` / `6–10` / Dating / Married (как в injury Hospital_* ) | 0–2: «Покажите рану, обработаю в клинике»; убрать «хрупкая» до 4♥ |
| `topicPassedOutInTown` | dialoguesHarveyInjury.json | 0 | «Одна никуда не пойдёшь», запрет выхода | **нет** | Ограничение свободы без доверия; C# письмо `mailHarveySleepControl` усиливает | 4♥+ или Dating | `Hearts: 4,5,6,7` минимум для restrictiva; 0–3: «Приходите в клинику завтра» | Смягчить: рекомендация, не запрет |
| `topicOverprotectiveMode` | dialoguesHarveyInjury.json | 0 | «Не могу отпустить», «никаких шахт без меня» | **нет** | Романтико-опекунский, не врачебный | 6♥+ или после severe injury event | `Hearts: 6,7,8,9,10` или `HasSeenEvent` mine rescue | — |
| `topicSpeakToHarvey` | dialoguesHarveyInjury.json | 0 | «Я не только врач, но и друг» | **нет** | Дружба заявлена до 2–3♥ | 3♥+ | `Hearts: 3,4,5…` | 0–2: только профессиональная поддержка |
| `PhaseTransition_*` | dialoguesHarveyInjury.json | 0 | Клинический, нейтральный | **частично** | OK по тону; проблема только в отсутствии gates (не критично) | любой при активной травме | — | — |

### B. Injury topics — Dating / Married blocks

| ID | Файл | Текущий уровень | Фактический тон | Подходит | Что не так | Нужный уровень | Правка условия | Правка текста |
|----|------|-----------------|-----------------|----------|------------|----------------|----------------|---------------|
| `topicCold` (Dating) | dialoguesHarveyInjury.json | Dating | «Солнышко», «малышка», «рядом со мной» | **да** | — | Dating | — | — |
| `topicHurt` (Married) | dialoguesHarveyInjury.json | Married | «Котёнок», «хрупкая» | **да** | — | Married | — | — |
| `Hospital_Mon`…`Sun` (0–5♥, topicHurt) | dialoguesHarveyInjury.json | 0–5 + topic | «Я всегда рядом», «хрупкая» | **частично** | На 0–2♥ «всегда рядом» рано; на 3–5 — acceptable | 2♥+ для «рядом» | Поднять порог `Hearts: 2,3,4,5` или split 0–1 | 0–1: «Заходите на перевязку» |
| `Hospital_Mon`…`Sun` (6–10♥) | dialoguesHarveyInjury.json | 6–10 + topic | «Не позволю навредить», контроль | **частично** | Допустимо 6–8♥; на 9–10 без dating — borderline romance | 6–8: OK; 9–10: чуть мягче без «не позволю» | — | 9–10: «Пожалуйста, берегите себя» вместо приказа |

### C. Cure / Treat (`dialoguesHarveyCure.json`)

| ID | Файл | Текущий уровень | Фактический тон | Подходит | Что не так | Нужный уровень | Правка условия | Правка текста |
|----|------|-----------------|-----------------|----------|------------|----------------|----------------|---------------|
| `Treat_*_Before/After` (все) | dialoguesHarveyCure.json | 0 | «Не отпущу без контроля», «слишком хрупкая», приказы | **нет** | Лечебный UI на 0♥ звучит как опекун-партнёр | 0–2: clinical; 3–5: firm; 6+: current | Три блока по hearts (как cured) | Base: «Сейчас осмотрю и перевяжу» без «не отпущу» |
| `topic*Cured` | dialoguesHarveyCure.json | 0 | %сцены%, «хрупкая», «буду следить» | **частично** | Тон 6–8♥ при 0♥ | 3♥+ для personal follow-up | `Hearts 0–2` короткий clinical; `3+` текущий | — |
| `topic*Cured` | dialoguesHarveyCure.json | Hearts 6–10 | Усиленный контроль, «не позволю» | **частично** | OK для 7–8♥; harsh для 6♥ | 7♥+ | `Hearts: 7,8,9,10` | — |
| `topicHarvey_NightRound` | dialoguesHarveyCure.json | 0 / Dating / Married | Ночной осмотр, забота | **частично** | Base OK medically; Dating/Married OK | C# inline + topic — base neutral OK | — | Base: без «не волнуйся, малышка» (если есть в dating-only) |

### D. Осложнения и trust (`dialoguesHarvey.json` base)

| ID | Файл | Текущий уровень | Фактический тон | Подходит | Что не так | Нужный уровень | Правка условия | Правка текста |
|----|------|-----------------|-----------------|----------|------------|----------------|----------------|---------------|
| `Introduction` | dialoguesHarvey.json | 0 | «Вы бледная и истощённая», осмотр сразу | **частично** | Медосмотр незнакомцу — OK; «истощённая» — assumptions | 0 | — | «Если нужна помощь — клиника открыта» |
| `topicHarvey_WetBandage` и др. | dialoguesHarvey.json | 0 | Строгий врач, без pet names | **да** | Профессионально-резко — OK на любом ♥ | любой | — | — |
| `topicHarvey_ForcedHospitalization` | dialoguesHarvey.json | 0 | «Не отпущу, пока не станет лучше» | **нет** | Intimate custody language без relationship | 4♥+ или Dating | `Hearts: 4+` или event-gated | «Вы под наблюдением до стабилизации» |
| `topicHarveyTrust_*` | dialoguesHarvey.json | Event-gated topics | «Вы» + договорённости | **да** | Хорошая модель: формально на раннем trust-arc | 3–6♥ events | — | — |
| `Hospital_Mon`…`Sun` (base, без topic) | dialoguesHarvey.json | 0 | «Госпитализирую», «запрещаю», ultimatum | **нет** | Клинический террор на 0♥; дублирует мягкие `Mon`–`Sun` ниже в том же файле | 6♥+ или active severe buff | `Hearts: 6,7,8,9,10` + `HasBuff` injury | Убрать из base; оставить в high-heart block |
| `Mon`…`Sun` (base, formal) | dialoguesHarvey.json | 0 | «Вы», профилактика | **да** | Образцовый ранний тон | 0–3♥ | — | — |
| `Resort_*`, `FlowerDance_Accept` | dialoguesHarvey.json | 0 | Контроль, «под моей заботой» | **нет** | Слишком лично для 0–4♥ | 6♥+ | `Hearts: 6,7,8,9,10` | — |
| `timeReaction_*` | dialoguesHarvey.json | 8–10 / 4–7 / Dating | Градация есть | **частично** | 8♥ block с «малышка» — рано (уровень 4–5 по шкале) | Pet names: Dating+ | Перенести pet names из `Hearts 8–10` в Dating block | 8–10: тёпло без «котёнок» |
| `AcceptBouquet` | dialoguesHarvey.json | Vanilla gate | «Под моей защитой», контроль | **частично** | Accept OK; tone very possessive — OK at dating start | Dating | — | Смягчить «не спорь с требованиями» |
| `RejectBouquet_AlreadyAccepted` | dialoguesHarvey.json | Married/Dating | «Девочка моя», «не отпущу» | **да** | — | Dating+ | — | — |

### E. Care (`dialoguesHarveyCare.json`)

| ID | Файл | Текущий уровень | Фактический тон | Подходит | Что не так | Нужный уровень | Правка условия | Правка текста |
|----|------|-----------------|-----------------|----------|------------|----------------|----------------|---------------|
| `topicHarveyGentleCare` | dialoguesHarveyCare.json | 0 (trigger CP) | «Молодой человек» | **нет** | Romance без Dating gate | Dating | `Relationship: Dating` | «Как ваш врач, рекомендую отдых» |
| `topicFirstMeeting` | dialoguesHarveyCare.json | 0 | Смущённый врач | **да** | OK early tone | 0–2♥ | — | — |
| `buffStrictSupervision` | dialoguesHarveyCare.json | 0 | «Каждое действие контролируется» | **нет** | Только для high neglect / 6♥+ | 6♥+ | `Hearts: 6+` | — |

### F. Почта (C# → CP)

| ID | Файл | Текущий уровень | Фактический тон | Подходит | Что не так | Нужный уровень | Правка условия | Правка текста |
|----|------|-----------------|-----------------|----------|------------|----------------|----------------|---------------|
| `mailHarveySleepControl` | mailInjury.json | 0 (C# без ♥ check) | Curfew 22:00, «худая, бледная», успокоительное | **нет** | Intimate + controlling; C# не требует dating | 4♥+ или Dating | C#: `IsDatingOrMarried` или split mail ID | Neutral: «Рекомендую режим сна»; dating-variant: current |
| `mailHarveyMineForbidden` | mailInjury.json | 0 | Врачебный запрет шахты | **да** | OK — medical authority | любой при severe | — | — |
| `HarveyMod_NeglectWarning` | mailInjury.json | 0 | «Фермер», строго, без pet names | **да** | OK | 3♥+ (phase treatment) | — | Fix ID mismatch (audit-mail) |
| `mailHarveyNoteGirlfriend` / `mailHarveyNoteWife` | mailCare.json | CP triggers | Romance | **да** | Gated by CP event/mail trigger | Dating / Married | — | — |
| `mailHarveyStep1` vs `mailHarveyStep1Dating` | mailCare.json | Split variants | Neutral vs controlling | **частично** | Хорошая **модель** split; но base Step1 всё ещё harsh on 0♥ | 0–3 / Dating | — | — |

### G. События (выборка)

Проверка по **таблице стадий**: «Вы» только E1; с E2 (750+) — «ты».

| ID | Файл | Friendship | Стадия | Подходит | Замечание |
|----|------|------------|--------|----------|-----------|
| `HarveyOverhaulStory.E1` | events.json | 500 (2♥) | 0 | **да** | «Вы», осмотр — единственное story-событие на «Вы» | 
| `HarveyOverhaulStory.E4_PierBreath` | events.json | 1250 (5♥) | 1 | **да** | «ты», trust-touch |
| `HarveyOverhaulStory.E6_SayItOutLoud` | events.json | 1750 (7♥) | 2 | **да** | «ты», договор — без «люблю» |
| `HarveyOverhaulStory.E7_DoorSignal` | events.json | 2000 (8♥) | 2 | **да** | «ты», opt-out |
| `HarveyOverhaulStory.E8_BadDayNoReason` | events.json | 2250 (9♥) | 2b | **да** | «ты» в ветках |
| `HarveyOverhaulStory.E10` pre / `_Dating` | events.json | 2750 | 2b / 3 | **да** | «ты»; split — romance, не обращение |
| `HarveyOverhaulStory.E12` pre / `_Dating` | events.json | 3250 | 2b / 3 | **да** | «ты»; split — romance |
| `HarveyOverhaulRomance.E1` | events.json | 3500 + Dating | 3 | **да** | «ты», поцелуй с согласия |
| `HarveyOverhaulStory.E14` | events.json | 4000 + Dating | 3 | **да** | «ты»; «любить тебя» — OK по смыслу |
| `HarveyOverhaulStory.E15` / `_Married` | events.json | 4500 | 3 / 4 | **да** | «ты» + opt-out; Married — pet names |
| `eventHarveyTraumaExam` | events.json | 2000 (8♥) | 2 | **частично** | «ты» без romance — OK; проверить маркеры |
| `eventHarveyFirstWalk` | events.json | day 11 | 0 | **нет** | «ты», романтичный fork до 2♥ — см. romantic-tone-audit |

*(Полная таблица 2026-05-23 — см. git history раздела G.)*

---

## Образцовая градация (что уже работает)

1. **`dialoguesHarvey.json` blocks:** `Hearts 4–7` → мягче; `8–10` → теплее; `Dating` → pet names; `Married` → `MarriageDialogueHarvey`.
2. **`dialoguesHarveyInjury.json`:** `Hospital_*` при `topicHurt` split 0–5 / 6–10 / Dating / Married.
3. **`dialoguesHarveyCure.json`:** `topic*Cured` и фазовые topics имеют Hearts 6–10 / Dating / Married (но **не** base Treat_*).
4. **Events:** `HarveyMod_NightCrisis_PreDating` vs `_Dating`; `eventHarveyMedicalCheck` vs `_Dating`; story arc E1→E9 с rising Friendship.
5. **`topicHarveyTrust_*`:** формальное «вы», без pet names — хороший trust-arc.
6. **`mailHarveyStep1` vs `mailHarveyStep1Dating`:** два тона в одной линии.

---

## Рекомендуемая модель исправления (без смены характера)

### Минимальный план (высокий ROI)

1. **Скопировать схему hearts из `topic*Cured`** на:
   - все `topic*` в `dialoguesHarveyInjury.json` block 1;
   - все `Treat_*` в `dialoguesHarveyCure.json` block 1.
2. **Убрать из base `dialoguesHarvey.json`:**
   - `Hospital_Mon`…`Sun` (strict) → только `Hearts 6+` + severe injury;
   - `Resort_*`, possessive `FlowerDance_Accept` → `Hearts 6+` или Dating.
3. **Почта:** split `mailHarveySleepControl` → neutral (0–3♥) / firm (4–7♥) / intimate (Dating+); C# gate.
4. **Pet names rule:** `солнышко / малышка / котёнок / дорогая / девочка моя` — **только** `Relationship: Dating|Married`. На 6–10♥ friendship — близкий друг **без** pet names и **без** «люблю/моя».
5. **Married rule:** гиперопека допустима, но с opt-out («остановите меня», «уйду на шаг», `quickQuestion`).
6. **Care:** `topicHarveyGentleCare` → `When: Dating`.

### Шаблон текстов по уровням (injury example)

| Стадия | `topicHurt` (пример) |
|--------|----------------------|
| 0–2♥ | «Покажите, где болит. Обработаю в клинике.» |
| 3–5♥ | «Порез серьёзнее, чем кажется. Давай перевяжу — и без тяжёлой работы сегодня.» |
| 6–8♥ | «Ты снова без перчаток? …Перевяжу. Пожалуйста, береги руки.» |
| 9–10♥ pre-dating | «Я не хочу снова видеть эту рану. Проверю завтра.» — **без** «хрупкая» как pet-substitute |
| Dating | (текущий dating-block: «солнышко», «рядом») |
| Married | (married-block + opt-out в сценах контроля) |

---

## Речь фермерши — сводка

| Категория | Паттерн | Оценка |
|-----------|---------|--------|
| Injury topic dialogues | Только Харви говорит | **OK** |
| Treat / cured %сцены% | Нarrator, farmer молчит | **OK** |
| Story events (E1–E9) | `quickQuestion`, `message` 1 строка, `emote` | **OK** |
| `eventSeen_*` memories | Длинные farmer thoughts | **средне** — допустимо post-vanilla |
| `HarveyMod_TreatmentPlanMeeting` | Farmer выбирает из 3 options | **OK** |

**Правило для новых записей:** farmer — `emote`, `message` ≤1 предложение, или `$q`/`quickQuestion`; не `speak farmer` абзацами.

---

## Приоритеты

| P | Задача |
|---|--------|
| P0 | Hearts-gates для injury/cure base blocks (Treat + topic*) |
| P0 | Убрать strict `Hospital_Mon`–`Sun` и `Resort_*` из 0♥ base dialoguesHarvey |
| P1 | Pet names только Dating+ |
| P1 | `mailHarveySleepControl` + C# relationship gate |
| P2 | `dialoguesHarveyCare` romance topics → Dating only |
| P2 | Поднять Friendship на `HarveyMod_TreatmentPlanMeeting` (500→750) |

---

## Методология

- Проверены все `When: Hearts/Relationship` в `dialoguesHarvey*.json`.
- Mail: отсутствие `When` = уровень 0 для всех получателей.
- Events: Friendship ÷ 250 = сердца; `PLAYER_NPC_RELATIONSHIP Dating/Married` учтён.
- Триггерные topic (`topicHarveyTrust_*`) оценены по контексту event, не по CP When.
- Код не изменялся.

---

## Исправления CP (2026-05-25, обращение story-arc)

**Канон:** «Вы» только **0–2♥** (story **E1**, `500 FP`). С **E2** (`750+`) — **«ты»** во всех `speak Harvey`, включая pre-dating.

| ID | Файл | Изменение |
|----|------|-----------|
| `HarveyOverhaulStory.E2`–`E15` | `events.json` | Массовая конвертация «Вы» → «ты» в `speak Harvey` при `Friendship >= 750` |
| `HarveyOverhaulStory.E1` | `events.json` | **Без изменений** — «Вы» сохранён (2♥) |
| `HarveyOverhaulStory.E11`, `E13` | `events.json` | Ручная дочистка в `quickQuestion` / narrative |

## Исправления CP (2026-05-23)

### Исправлено условием (`When` / Friendship)

| ID / область | Файл | Было | Стало |
|--------------|------|------|-------|
| `Hospital_Mon`…`Sun` (strict) | `dialoguesHarvey.json` | Base без `When` (0♥) | Удалены из base; перенесены в блок `Hearts: 6,7,8,9,10` |
| `Spring_5`, `Summer_12`, `Fall_20`, `Winter_3` | `dialoguesHarvey.json` | Base 0♥ | Только `Hearts: 6+` |
| `Saloon`, `Saloon2`, `Hospital`, `Hospital2`, `Desert*`, `HarveyRoom*`, `ArchaeologyHouse*`, `Beach*` | `dialoguesHarvey.json` | Base 0♥ | Только `Hearts: 6+` |
| `Resort_*`, `FlowerDance_Accept` | `dialoguesHarvey.json` | Base 0♥ | Удалены из base; остаются в блоках `Hearts: 4–7`, `8–10`, `Dating` |
| `topicHarvey_ForcedHospitalization` (intimate) | `dialoguesHarvey.json` | Base 0♥ | Клинический текст в base; «не отпущу» — только `Hearts: 6+` |
| `Hospital_*` при `topicHurt` | `dialoguesHarveyInjury.json` | `Hearts: 0–5` | Split: `0–1` (клиника), `2–5` (мягче), `6–10` / Dating / Married без изменений |
| Injury `topic*` | `dialoguesHarveyInjury.json` | Один base-блок | Добавлены блоки `Hearts: 3–5` и `Hearts: 6–10` поверх клинического fallback |
| `buffStrictSupervision` | `dialoguesHarveyCare.json` | Base 0♥ | Только `Hearts: 6+` |
| `topicHarveyGentleCare` (romance) | `dialoguesHarveyCare.json` | Base 0♥ | Base — врач; «молодой человек» — только `Relationship: Dating` |
| `HarveyMod_TreatmentPlanMeeting` | `events.json` | `Friendship Harvey 500` (2♥) | `Friendship Harvey 750` (3♥) |
| `eventRescueOperation` | `events.json` | `Friendship Harvey 600` (2♥) | `Friendship Harvey 1000` (4♥) |

### Исправлено текстом (без нового gate / смягчение fallback)

| ID / область | Файл | Изменение |
|--------------|------|-----------|
| `Introduction` | `dialoguesHarvey.json` | Убраны assumptions «бледная и истощённая»; нейтральное приглашение в клинику |
| `topicHarvey_ForcedHospitalization` (base) | `dialoguesHarvey.json` | «Вы под медицинским наблюдением до стабилизации» |
| `summer_9`, `winter_30`, `spring_14` (base) | `dialoguesHarvey.json` | Праздничные реплики без «под моей защитой» |
| Injury `topic*` block1 | `dialoguesHarveyInjury.json` | Клинический fallback (0–2♥): «вы», протокол, без «хрупкая» / «не отпущу» |
| `Treat_*` block1 | `dialoguesHarveyCure.json` | Смягчены «не отпущу», «не позволю», «под защитой», «хрупкая» → клинический тон |
| `mailHarveySleepControl` | `mailInjury.json` | Нейтральные рекомендации по сну вместо curfew + intimate описания (C# gate не менялся) |

### Оставлено осознанно

| ID / область | Причина |
|--------------|---------|
| Dating / Married блоки (`dialoguesHarvey*.json`) | Pet names и романтика — по дизайну при `Relationship: Dating/Married` |
| Блок `Hearts: 8–10` + `Relationship: Dating` в `dialoguesHarvey.json` (комментарий «РОМАНТИКА») | Уже gated `Relationship: Dating`; pet names допустимы |
| `topicHarveyTrust_*` | Event-gated trust-arc; формальное «вы» — образцовая модель |
| `mailHarveyMineForbidden`, `HarveyMod_NeglectWarning` | Медицинский/строгий тон без intimacy — OK на любом ♥ |
| `RejectBouquet_AlreadyAccepted` | Vanilla gate Dating+; «девочка моя» — OK |
| `eventSeen_*` memory dialogues | Post-vanilla memories; не injury-pipeline |
| `mailHarveySleepControl` split по ♥ | Требует C# gate или второй mail ID — **не сделано** (только смягчение текста) |
| `mailHarveyOverprotectiveNotice` | Строгий медицинский тон при severe injury — OK |
| PhaseTransition_* | Клинически нейтральны — gates не нужны |
| Hearts 6–10 injury block | Сохранён настойчивый тон без pet names (дружба высокого уровня) |

### Статус приоритетов (2026-05-24)

| P | Задача | Статус |
|---|--------|--------|
| P0 | Hearts-gates injury/cure base | **✅ done** — injury split 0–1/2–5/6–10; Treat block1 смягчён |
| P0 | Strict Hospital/Resort из 0♥ base | **✅ done** |
| P1 | Pet names только Dating+ | **✅ done** (base); Dating/Married blocks без изменений |
| P1 | `mailHarveySleepControl` C# gate | **⚠️ открыто** — CP текст смягчён; C# gate не добавлен |
| P2 | `topicHarveyGentleCare` → Dating | **✅ done** |
| P2 | `HarveyMod_TreatmentPlanMeeting` 750 | **✅ done** |
| NEW | C# launchers без tone gate (night visit, storm) | **⚠️ открыто** — см. [events-audit.md](./events-audit.md) |

---

## Исправления CP (2026-05-23, медицинские тексты)

См. [audit-medical-texts.md](./audit-medical-texts.md) — раздел «Исправления CP (2026-05-23, медицинский аудит)».

Кратко по тону в рамках мед. правок:

| ID / область | Файл | Изменение |
|--------------|------|-----------|
| `topicHarvey_WetBandage` | `dialoguesHarvey.json` | Риск инфекции явно; без intimate «промокла до нитки» |
| `topicHarvey_Neglect` | `dialoguesHarvey.json` | Мед. последствия (AB, инфекция), не «ты мне небезразлична» |
| `mailHarvey_Neglect`, `HarveyMod_NeglectWarning` | `mailInjury.json` | Пропуск перевязок → осложнения |
| `topic*Phase*` block1 | `dialoguesHarveyCure.json` | Профессиональный протокол вместо паники «Боже мой» на 0♥ |
| `Treat_*` block1 | cure | Клиническая точность по типу травмы (см. medical audit) |
| `Treat_Hurt_After` | `dialoguesHarvey.json` | Царапина вместо «синяк / хрупкая» |
| Injury base + Dating/Married | `dialoguesHarveyInjury.json` | BackStrain: покой до массажа; Burn: не мочить; Fracture: шахта запрещена |

### Оставлено осознанно (мед. аудит)

| ID / область | Причина |
|--------------|---------|
| Dating/Married cured-блоки с pet names | Relationship-gated; мед. «срослись» исправлено только для ribs |
| `Treat_*_After3–7` (не P0-травмы) | Generic follow-up; P0-уникализация выполнена для ключевых After1–2 и критичных травм |
| Events / quests «Боже мой» | Экстренные сцены — допустимая эмоция; вне scope topic/mail |
