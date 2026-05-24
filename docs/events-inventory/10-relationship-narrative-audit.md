# Сюжетный аудит отношений с Харви

Аудит тона сцен, relationship gates в CP и C#. **Актуализация:** 2026-05-24 (gates без изменений в этом проходе). Предыдущие правки 2026-05-23 — split NightCrisis/Birthday/MedicalCheck, care chain, variant B тона.

Источники: CP (`events.json`, `eventsCare.json`, `eventsMineRescue.json`, `triggersCare.json`), C# (`InteractionHandler`, `TimeEventHandler`, `HospitalizationManager`, `PassOutHandler`).

**Применённые правки:** [harvey-events-fix-report.md](../harvey-events-fix-report.md), [harvey-relationship-visits-audit/](../harvey-relationship-visits-audit/README.md)

Шкала сердечек: 250 pts = 1 heart (vanilla).

---

## Методология

Для каждой сцены оценены:

- **Текущий gate** — что реально требует CP/C# (hearts, `PLAYER_NPC_RELATIONSHIP`, topic, отсутствие gate).
- **Рекомендуемый gate** — к какой стадии логично привязать по тексту.
- **Тон** — профессиональный / тёплый врач / опекунский / романтический / супружеский / травма-интимный.
- **Проблема** — диссонанс gate ↔ текст, роль врача, повторяемость.
- **Исправление** — gate, fork, отдельные ветки dating/married, или оставить медицинским.

### Стадии отношений

| Стадия | Обычно в коде | Ожидаемый тон Харви |
|---|---|---|
| **1. До дружбы / доктор** | `!PLAYER_HAS_MET`, 0–1 heart, «Вы» | Профессиональная дистанция, сочувствие без pet names |
| **2. Дружба, не dating** | Hearts 2–7, без `Dating` | Забота, границы, «пациент / сосед», без ночёвок и ревности |
| **3. Dating** | `PLAYER_NPC_RELATIONSHIP Dating` | Романтика, нежность, ревность уместна в мягкой форме |
| **4. Married** | `Married` (часто в одном gate с Dating) | Супружеская опека, «мы», быт, меньше формальностей |
| **5. После брака** | Отдельных веток почти **нет** | CP/C# не различают engaged/married в тексте |

**Важно:** в CP `Dating` и `Married` часто в одном `GameStateQuery`. Отдельные married-диалоги почти не используются — это системная проблема.

---

## Сводка по стадиям (рекомендуемое размещение)

### Стадия 1 — до дружбы / обычный доктор

Медицински и нейтрально: `HarveyOverhaulStory.E1`, `eventHarveyFirstMeeting` (с оговоркой), `eventHarveyCheckup`, care-визиты на ферму (ранняя фаза).

### Стадия 2 — дружба без dating

Story E2–E4, E7–E8, `HarveyMod_FirstTreatment`, `HarveyMod_TreatmentPlanMeeting`, `eventHarveyMedicalCheck`, `eventRescueOperation` (с понижением интимности), storm comfort **только мед-ветка**, `eventHarveyLateNightCollapse` (проф. режим).

### Стадия 3 — dating

Романтика: `eventHarveyFirstDate`, `eventHarveyMountainDate`, `eventHarveyPropose`, mine rescue dating, night checks, `HarveyMod_NightCrisis_Dating`, storm comfort (med tone), `eventHarveyRoomCheckup2`, mine/skull interception.

### Стадия 4 — married

Те же события, что dating, но **нужны married-варианты текста** (обращение, быт, «жена»). Сейчас: gate есть, веток нет.

### Стадия 5 — после брака

Явных post-marriage arc в CP **нет**. `eventHarveyPropose` → vanilla свадьба; дальше только общие married gates.

### Медицина без романа (InjuryCare)

Должны работать при dating **и** без него (с разным текстом): `eventHarveyMineRescue` (legacy), minor rescue, emergency/exhaustion/collapse, C# treatment click, phase transitions, dirty/wet complications.

---

## Таблица аудита

| Event ID / Scene | Текущий relationship gate | Рекомендуемый gate | Тон Харви | Проблема | Исправление |
|---|---|---|---|---|---|
| `eventHarveyFirstMeeting` | `!PLAYER_HAS_MET` | Стадия 1 | Тёплый врач + «первая официальная встреча» | Пиджак, еда, «хрупкая» — **очень** опекунски для незнакомца; намёк «ты напоминаешь…» | Оставить стадию 1, но смягчить pet names; fork: формальное «Вы» vs мягче после согласия на еду |
| `eventHarveyCheckup` | Topic `topicAgreedCheckup` | Стадия 1–2 | Медосмотр | OK для доктора | Без романтики; gate hearts ≥1 опционально |
| `eventHarveyFirstVisit` | Topic + day≥3 | Стадия 1–2 | Сосед-врач | OK | «Пациенты» в ветке agree — OK |
| `eventHarveySecondVisit` | Day≥7 + seen first visit + outcome topics | Стадия 2 | Дружелюбный врач | ✅ Gate исправлен | — |
| `eventHarveyFirstWalk` | Sunny, seen second visit, outcome topics | Стадия 2 | Прогулка | ✅ **Достижимо**; тон смягчён (вар. B) | Опционально dating gate |
| `HarveyOverhaulStory.E1_SlipperyPath` | Wind, !seen E1 | Стадия 1 | «Вы», городской врач | OK — этalon stage 1 | — |
| `HarveyOverhaulStory.E2_InsistentExam` | Seen E1, **2 hearts** | Стадия 2 | Настойчивый осмотр | OK | — |
| `HarveyOverhaulStory.E3_ForestApothecary` | **4 hearts**, seen E2 | Стадия 2–3 | Травник + забота | На 4 hearts уже «лесная фея» — рановато для non-dating | Gate **6 hearts** или dating; иначе убрать поэтику |
| `HarveyOverhaulStory.E4_PierBreath` | Seen **E3**, evening, 5♥ | Стадия 2 | Дыхание, grounding | ✅ Тон смягчён (вар. B) | — |
| `HarveyOverhaulStory.E5_StormBeside` | **6♥**, storm, seen E4 | Pre-dating story | Эмоциональная поддержка | ✅ Тон med (вар. B) | — |
| `HarveyOverhaulStory.E6_SayItOutLoud` | **7♥**, seen E5 | Pre-dating story | Теплица / клиника | ✅ Тон med (вар. B) | — |
| `HarveyOverhaulStory.E7_TownSip_Sunny` | **8♥**, seen **E6** | Стадия 2 | Опека | ✅ Цепочка + тон | — |
| `HarveyOverhaulStory.E8_QuietShelf` | **8♥**, Sat, seen **E7** | Стадия 2 | Тихая поддержка | ✅ Цепочка исправлена | — |
| `HarveyMod_FirstTreatment` | **3♥** + `topicHarveyNeedsFirstTreatment` (C#) | Стадия 2 (med) | Мед-осмотр | ✅ Тон/контакт смягчены; C# topic | — |
| `HarveyMod_NightCrisis_Dating` | **Dating/Married** | Dating | Объятия | Split ✅ | Married text variant |
| `HarveyMod_NightCrisis_PreDating` | **6♥**, !Dating | Стадия 2 | Ночной кризис med | Split ✅ | — |
| `HarveyMod_BirthdayHospital_Dating` | **Dating/Married** | Dating | Праздник | Split ✅ | Married text |
| `HarveyMod_BirthdayHospital_Friend` | **8♥**, !Dating | Стадия 2 | Праздник в клинике | Split ✅ | — |
| `HarveyMod_TreatmentPlanMeeting` | Topic + **2 hearts** | Стадия 2 | План лечения | OK медицински | — |
| `eventHarveyMedicalCheck` | **6 hearts** + mail | Стадия 2–3 | Напоминание осмотра | OK | — |
| `eventHarveyTraumaExam` | **8 hearts** | **Dating** (травма) | Раскрытие шрамов | Глубоко личное при только hearts | Gate Dating + opt-out; врач остаётся профессиональным |
| `eventHarveyEmergencyCare` | **Нет gate** | Любая + severe injury | Крик → осмотр | Недостижимо; тон OK для экстренки | Launcher + gate по hearts (мягче текст если <4) |
| `eventHarveyExhaustion` | **Нет gate** | Любая + exhausted | Госпital care | Недостижимо | C# PlayEvent; med tone always OK |
| `eventHarveyTreatmentCollapse` | **Нет gate** | Severe / collapse | Клиника | Недостижимо | C# или trigger |
| `eventStayInHospital` | **Нет gate** | Injury | Палата | Заменено C# hosp | Связать с `HospitalizationManager` или CP |
| `eventHarveyMineRescueDating` | C#: dating; CP: Dating/Married | **Dating/Married** | «Держись», «я рядом», panic love | OK для dating; C# bypass CP | Married fork («жена»); legacy для non-dating |
| `eventHarveyMineRescue` | C# dating only; CP **нет** rel gate | **Med: любой** / **Rom: dating** | «Потерял тебя», reanimation | Legacy тон **romantic** но C# требует dating | Non-dating: холоднее med script; dating: текущий |
| `eventHarveyMinorMineRescue` | C# dating | **Med без dating** (лёгкая травма) | Строгий врач | C# требует dating для **любого** rescue | Minor без dating + нейтральный текст |
| `eventHarveyMineInterception` | SpaceCore: **Dating/Married** | Dating/Married | «Моя пациентка», «моё слово закон» | OK для dating; **слишком** для friends | Gate OK; married — «мы договорились» |
| `eventHarveySkullCavePrevention` | SpaceCore: **Dating/Engaged/Married** | Dating+ | Паника, «без меня» | OK | Engaged/Married text variants |
| `eventHarveyCheckHealthFarmer` | **Dating** | Dating | Осмотр после PlayerKilled | OK | — |
| `eventHarveyCheckFarmerOutsideAfter22` | Topic + **Dating/Married** | Dating/Married | Ночной контроль | OK | Married: «я жду дома» |
| `eventHarveyMorningCheckup` | Topic + **Dating** | Dating | Утренний осмотр | OK | Married variant |
| `eventHarveyLateNightCollapse` | **Только время 24–26** | Стадия 2 med / Dating личное | «Приду за 60 секунд», капельница | **Нет relationship gate** — интимная опека для strangers | Gate hearts ≥4 med; «60 сек» только Dating+ |
| `eventHarveyStormComfortFarm` | **3♥**, storm, random | Pre-dating med | Утешение в **клинике** (не HarveyRoom) | ✅ Тон med (вар. B); ❌ buff gate | C# buff или убрать gate |
| `eventHarveyStormComfortForest` | **3♥**, storm, random | Pre-dating med | Укрытие | ✅ Тон med (вар. B); ❌ buff | C# buff |
| `eventHarveyStormComfortTown` | **3♥**, random | Pre-dating med | Saloon | ✅ Тон med; ❌ buff | C# buff |
| `eventHarveyStormComfortMine` | **3♥**, random | Pre-dating med | Эвакуация | ✅ Тон med; ❌ buff | C# buff |
| `eventHarveyStormComfortMountain` | **3♥**, random | Pre-dating med | Горы | ✅ Тон med; ❌ buff | C# buff |
| `eventHarveyStormComfortDesert` | **3♥**, random | Pre-dating med | Пустыня | ✅ Тон med; ❌ buff | C# buff |
| `eventRescueOperation` | **2.4♥** + topic | Trauma med | Осмотр | ✅ Контакт с согласием; topic orphan | C# topic |
| `eventHarveyFirstDate` | **Dating**, 8♥ | Dating | Свидание | OK | — |
| `eventHarveyMountainDate` | **Dating**, 9♥ | Dating | Горы | OK | — |
| `eventHarveyPropose` | **Dating**, 10♥ | Dating | Предложение | ✅ «Решение только твоё» | — |
| `eventHarveyRoomCheckup` | **6♥**, no dating | Стадия 2 med | Осмотр | ✅ Контакт смягчён | — |
| `eventHarveyRoomCheckup2` | **Dating** + BETAS | Dating | Сюрприз-визит | OK | Married variant |
| **C#: Night visit** (`TimeEventHandler`) | Severe buff, home, **no rel gate** | **Dating/Married** | «Я рядом», стук, +10 friendship | **Ночь в спальне** без romance gate | Gate `IsDatingOrMarriedToHarvey()` |
| **C#: Forced hospitalization** | `ForceHospitalization` + topic + Severe | Dating/Married (C# already) | «Не обсуждается», mine_rescue | OK по gate C#; **снимает topic** | Married text; не удалять topic до конца сцены |
| **C#: Treatment click** (`InteractionHandler`) | **Нет rel gate** | Med **всегда** | PhaseTransition / Treat | OK — чистая медицина | Оставить без romance; dating adds optional `$l` lines in JSON |
| **C#: Proximity discovery** | **Нет rel gate** | Hearts ≥2 | Эмоция при травме | OK для доктора | — |
| **C#: PassOut town** (`topicPassedOutInTown`) | Dating not required | Стадия 2+ | Sleep mail | Topic → CP events с dating gate | OK chain if CP gated |
| `mailHarveyMedicalCheckReminder` | triggersCare: **Dating/Married** | Dating/Married | Письмо | OK | — |
| `mailHarveySleepControl` | После town pass-out | Стадия 2+ | Контроль сна | OK | — |
| `mailHarveyMineForbidden` | После mine warning | Injury (any rel) | Запрет шахты | OK — med authority | — |

---

## Повторяющиеся системные проблемы

### 1. Hearts без Dating = романтика

**Было:** storm comfort, FirstTreatment, NightCrisis, E4–E6, RescueOperation, RoomCheckup.

**2026-05-23:** тексты смягчены (вар. B) + split NightCrisis/Birthday/MedicalCheck; storm **launcher** (`buffStressThunder`) всё ещё открыт; `dialoguesHarveyStress/Cure` — отдельный проход.

### 2. Dating gate в CP, но не в C#

**Затронуто:** night visit (нет gate). Mine rescue — **исправлено:** C# только Dating/Married.

### 3. Married = Dating в тексте

**Затронуто:** почти все `Dating Married` gates.

**Исправление:** `$query PLAYER_NPC_RELATIONSHIP Married` → alternate lines (быт, «жена», дом).

### 4. Слишком холодно при dating/married

Редко. Скорее **обратное**. Исключение: `eventHarveyMineRescue` legacy — OK emotionally for dating; для married нужен усиленный тон.

### 5. Роль врача

**2026-05-23:** controlling-lines audit (116+ замен) — «не обсуждается» → просьба; interception/mine rescue смягчены. Forced hosp — по-прежнему авторитарный тон (InjuryCare design).

### 6. Блокировать до dating?

**Да, блокировать (или сильно смягчить):** storm comfort romantic forks, E5–E6, room checkup, night visit C#, mountain/first date (уже gated), mine interception.

**Нет, оставить медицинским без романа:** E1–E2, checkup chain, emergency/exhaustion, minor rescue, treatment click, medical check, late collapse (med branch).

### 7. One-shot vs repeat

Story E1–E8 — one-shot по design (OK).

Storm comfort — **repeatable** при random + buff → романтика может **повторяться** на 3♥ — gate Dating снижает absurdity.

Mine rescue — one-shot `eventsSeen` (см. timing audit) — OK для кат-сцены, нужен med fallback для repeat.

---

## Приоритет правок (сюжет)

| Приоритет | Сцены | Статус |
|---|---|---|
| **P0** | `buffStressThunder` / storm comfort ×6 | ❌ Открыто — launcher |
| **P0** | 4 mail IDs осложнений | ❌ Открыто |
| **P1** | C# night visit gate | ❌ Открыто |
| **P1** | `eventRescueOperation` topic | ❌ Orphan topic |
| **P2** | Married alternate lines | ⚠️ Частично |
| ~~**P3**~~ | ~~`eventHarveyFirstWalk` reachability~~ | ✅ 2026-05-23 |
| ~~**P0**~~ | ~~Storm romantic tone / HarveyRoom~~ | ✅ Текст med (вар. B) |
| ~~**P2**~~ | ~~FirstTreatment / NightCrisis fork~~ | ✅ Split + C# topic |
| ~~**P3**~~ | ~~`eventHarveyPropose` consent~~ | ✅ «решение только твоё» |

---

## Связанные документы

- [07-reachability-table.md](07-reachability-table.md) — техническая достижимость
- [09-timing-audit.md](09-timing-audit.md) — порядок DayStarted / rescue / hosp
- [08-events-as-book.md](08-events-as-book.md) — тексты сцен для правки тона

**Статус:** актуализирован после правок CP/C# (2026-05-23). Открытые пункты: storm buff, mail, night visit gate, Married-тексты.
