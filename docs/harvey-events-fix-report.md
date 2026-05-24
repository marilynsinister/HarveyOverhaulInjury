# Финальный отчёт: правки событий Харви

**Дата:** 2026-05-23  
**Мод:** Harvey Overhaul (CP) + HarveyOverhaulInjury (C#)  
**Источники:** `content.json` → `events.json`, `eventsCare.json`, `eventsMineRescue.json`, `triggersCare.json`; C# `PassOutHandler`, `DialogueManager`, `InjuryManager`

Отчёт для сценариста и программиста: что изменилось, зачем, и как проверить в игре.

---

## 1. Что было исправлено по условиям событий

### Разделение версий по отношениям (Dating / Married vs pre-dating)

| Событие | Было | Стало |
|---------|------|-------|
| `HarveyMod_NightCrisis` | Одна версия с романтическим тоном | **`HarveyMod_NightCrisis_Dating`** (Dating/Married) + **`HarveyMod_NightCrisis_PreDating`** (`!Dating !Married`); взаимоисключающие `!seen` |
| `HarveyMod_BirthdayHospital` | Одна романтическая сцена | **`_Dating`** / **`_Friend`**; summer 9, Hospital, 8♥; cross-`!seen` + `topicBirthdayHospitalComplete` |
| `eventHarveyMedicalCheck` | Один текст для всех | **`eventHarveyMedicalCheck`** (pre-dating) + **`eventHarveyMedicalCheck_Dating`**; письмо-напоминание только для Dating/Married |
| `eventHarveyMineRescue*` | Legacy без gate | **`eventHarveyMineRescueDating`** с `PLAYER_NPC_RELATIONSHIP Dating Married` + cross-`!seen` по трём rescue-событиям |

### Упрощение и надёжность триггеров

| Событие | Изменение |
|---------|-----------|
| **`HarveyMod_FirstTreatment`** | Условие CP: `topicHarveyNeedsFirstTreatment` + `!seen` + `!topicFirstTreatmentComplete` (вместо длинного OR по injury-topics). Topic ставит **C#** при первой лечебной травме |
| **Шахтное спасение (C#)** | Rescue запускается **только при Dating/Married**; выбор: severe → `eventHarveyMineRescueDating`, иначе → `eventHarveyMinorMineRescue`. Повтор → только `topicMineInjuryRescue`, без повтора кат-сцены |
| **Конфликт rescue + interception** | Новый topic **`topicMineRescuePending`** (C#): блокирует `eventHarveyMineInterception` и SkullCave-warning на время warp в шахту |
| **`eventHarveyMineRescue` (legacy)** | Добавлены `!seen` guards; используется только как fallback, если Dating-сцена отсутствует в CP |
| **`eventHarveyMinorMineRescue`** | Добавлены `!seen` guards (см. §6 — путь всё ещё редкий) |

### Storm comfort (pre-dating по задумке)

Все **`eventHarveyStormComfort*`** (Farm, Forest, Town, Mine, Mountain, Desert):

- **Условия без изменений:** `buffStressThunder` + `Weather storm` + 750♥ + Random; **без** Dating gate — сцены доступны до романтики.
- **Текст и контакт** переписаны (§4), чтобы не звучать как свидание.

### Story E1–E8 (условия цепочки)

Цепочка не ломалась; усилены **cooldown-topics** `HarveyMod_CD_Global` и `HarveyMod_CD_E1`…`E8`, чтобы события не наслаивались:

| Шаг | ID | ♥ | Предыдущее | Доп. условия |
|-----|-----|---|------------|--------------|
| E1 | `HarveyOverhaulStory.E1_SlipperyPath` | 500 | — | Wind, 700–1400 |
| E2 | `E2_InsistentExam` | 750 | E1 | `!HarveyMod_CD_E1` |
| E3 | `E3_ForestApothecary` | 1000 | E2 | Thu–Sat, Sunny, `!HarveyMod_CD_E2` |
| E4 | `E4_PierBreath` | 1250 | E3 | Sunny, 1800–2600 |
| E5 | `E5_StormBeside` | 1500 | E4 | Storm, `!HarveyMod_CD_E4` |
| E6 | `E6_SayItOutLoud` | 1750 | E5 | 1900–2330, `!HarveyMod_CD_E5` |
| E7 | `E7_TownSip_Sunny` | 2000 | E6 | Sunny, 1200–1500 |
| E8 | `E8_QuietShelf` | 2000 | E7 | Sat, `!HarveyMod_CD_E7` |

---

## 2. Что было исправлено по последовательности Story E1–E8

**Задача:** ранняя дуга (E1–E8) остаётся **pre-dating** — доверие и лечение, без скачка в романтику.

| Сцена | Суть правки |
|-------|-------------|
| **E2** | Убраны «ваш защитник», «режим не обсуждается» → рекомендации врача, выбор за игроком |
| **E4** | «Безопасность не обсуждается» → «отнесись серьёзно» |
| **E5** | «моё желание», `$l` → «моя работа», «приходи, когда понадобится» |
| **E6** | Fork «защищать» / «свет в окне для тебя» → «рядом как врач», «клиника открыта» |
| **E7** | «не позволю упасть» → «не хочу, чтобы упала» |

**Cooldown-topics** после каждого E-события не дают следующему сработать в тот же день «случайно» поверх другого. Пропуск E возможен только если не выполнены погода/день недели/♥ — это штатно для Stardew.

---

## 3. Какие события теперь требуют Dating / Married

Эти сцены **намеренно** романтичнее или ближе телесно. Без букета / супружества они **не должны** проигрываться.

| Событие | Gate |
|---------|------|
| `eventHarveyCheckHealthFarmer` | Dating |
| `eventHarveyMorningCheckup` | Dating |
| `eventHarveyCheckFarmerOutsideAfter22` | Dating или Married |
| `eventHarveyMedicalCheck_Dating` | Dating или Married |
| `eventHarveyFirstDate` | Dating |
| `eventHarveyMountainDate` | Dating |
| `eventHarveyPropose` | Dating (10♥) |
| `eventHarveyRoomCheckup2` | Dating |
| `HarveyMod_NightCrisis_Dating` | Dating или Married |
| `HarveyMod_BirthdayHospital_Dating` | Dating или Married |
| `eventHarveyMineRescueDating` | Dating или Married (C# + CP) |
| `eventHarveyMineInterception` | Dating/Married (триггер `triggersCare.json`) |
| `eventHarveySkullCavePrevention` | Dating/Married (триггер) |

**Pre-dating пары** (явные альтернативы):

- `HarveyMod_NightCrisis_PreDating`
- `HarveyMod_BirthdayHospital_Friend`
- `eventHarveyMedicalCheck`
- `eventHarveyMineRescue` (только fallback / debug)

---

## 4. Какие реплики были смягчены

### Принцип замен (pre-dating / кризис)

| Было (романтика / контроль) | Стало (врач / границы) |
|----------------------------|-------------------------|
| «Иди ко мне» | «Сядь рядом, **если** тебе так спокойнее» |
| «Я укрою тебя собой» / объятие anim 54/55/101 | «Помогу найти укрытие» / «Дыши. Я рядом» (без навязанного hug) |
| «Я не отпущу» / «не могу потерять» | «Останусь рядом, пока гроза не стихнет» / «как пациентка» |
| «солнышко», «моё желание», `$l` без dating | `$0`, клинический тон |
| «Только мы» | «Никого лишних» / «буду дежурить» |
| «Дай мне руку» при «не трогай» | «Возьми, **если готов**. Если нет — подожду» |

### По группам сцен

**Storm comfort (7 локаций + Farm ночью):**  
Farm → переждать в **клинике**, не в `HarveyRoom`; Mountain/Desert/Mine/Town/Forest — убраны объятия, «смысл жизни», «найду тебя» → «на связи», «моя работа».

**`eventRescueOperation` (trauma, гроза):**  
Убраны поцелуй, навязанные объятия; fork — контакт **только если игрок не отстраняется**; «Только я» → «буду дежурить».

**Первое лечение и визиты:**  
`HarveyMod_FirstTreatment` — «нежно берёт за руку» → «протягивает руку, **ждёт ответа**»; «не обсуждается» → «очень прошу отнестись серьёзно».

**Контролирующие формулировки (116+ записей):**  
См. `docs/harvey-relationship-visits-audit/controlling-lines-replacements.md` — «не обсуждается», «приказ врача», «слежу» → просьба, рекомендация, «буду проверять».

**Dating-only (сохранили близость, убрали отнятие выбора):**  
`eventHarveyPropose` — финал: «решение **только твоё**»; `eventHarveyMineRescueDating` — «доверься мне» вместо «не спорь».

**Шахтное спасение legacy + письмо:**  
`eventHarveyMineRescue` / `mailHarveyAfterMineRescue` — «вернись ко мне», «потерял тебя» → «стабилизируем пульс», «доставил в клинику».

---

## 5. Какие topics были исправлены

| Topic | Кто ставит | Кто снимает | Зачем |
|-------|------------|-------------|-------|
| **`topicHarveyNeedsFirstTreatment`** | C# при первой лечебной травме | После `HarveyMod_FirstTreatment` / начала лечения | Надёжный триггер FirstTreatment вместо OR по buff |
| **`topicFirstTreatmentComplete`** | Событие FirstTreatment | — | One-shot guard повтора FirstTreatment |
| **`topicNightCrisisComplete`** | NightCrisis Pre/Dating | — | One-shot между версиями |
| **`topicBirthdayHospitalComplete`** | Birthday Friend/Dating | — | One-shot между версиями |
| **`topicMineInjuryRescue`** | CP rescue-события / C# fallback | Госпитализация (`PlayerEventHandler`) | Диалоги + forced hosp |
| **`topicMineRescuePending`** | C# перед warp в Mine | `ClearMineRescueState` | Блок interception во время rescue |
| **`HarveyMod_CD_Global`, `HarveyMod_CD_E1`…`E8`** | Story E1–E8 | Истекают по дням | Cooldown цепочки E1–E8 |
| **`topicRescueOperation` → `topicRescueComplete`** | CP script `eventRescueOperation` | Topic **нигде не создаётся** (C# не ставит) | Orphan — событие недостижимо |

**Исправление логики:**  
`IsMineRescueEventAlreadySeen` больше **не** считает активный `topicMineInjuryRescue` за «уже seen» — только `eventsSeen`, чтобы повторная гибель в шахте не пропускала кат-сцену ошибочно.

---

## 6. Потенциально недостижимые события — найдено и статус

| Событие | Проблема | Статус |
|---------|----------|--------|
| **`eventHarveyMinorMineRescue`** | C# после смерти в шахте всегда накладывает `buffBadlyHurt` → всегда severe → Dating/legacy path | **Не исправлено** (нужно отдельное решение: лёгкая смерть без severe) |
| **Mine rescue без Dating** | C# `TriggerMineRescueEvents` выходит, если нет Dating/Married | **По дизайну** — pre-dating игрок **не** получает cutscene rescue (только ванильное восстановление) |
| **`events_for_mode_new_formatted.json`** (MyMod storm/stress) | **Не подключён** в `content.json` | Правки внесены «на будущее»; в текущей сборке **не активен** |
| **Legacy `eventHarveyMineRescue`** | Достижим только если Dating-сцена отсутствует в CP | Fallback; текст смягчён |
| **Письмо после rescue (Dating)** | `mailHarveyAfterMineRescue` шлёт только legacy-событие | Dating-ветка письма **не** добавлялась — возможное улучшение |

---

## 7. Что осталось проверить вручную в игре

1. **Диалоги stress/cure/pregnant** (`dialoguesHarveyStress.json` и др.) — pet names («солнышко», «люблю») на топиках **без** Dating gate в ключах; отдельный проход.
2. **Ранние визиты** (`eventsCare.json`) — много `$l`, но без явной романтики; автору решить, нужен ли split Friend/Dating.
3. **`eventHarveyMinorMineRescue`** — воспроизвести только через debug / искусственное отключение `buffBadlyHurt`.
4. **Mine rescue без Dating** — убедиться, что поведение соответствует задумке (нет cutscene, нет «сломанного» soft-lock).
5. **Story E3** — только Thu–Fri–Sat; **E8** — только Sat: проверить календарь при тесте.
6. **Storm comfort** — Random; может не выпасть за один сезон — тест с debug / принудительным `buffStressThunder`.
7. **Reload во время mine rescue** — resume через `PendingMineRescueEventId`.
8. **Совместимость с другими модами** на `Data/Events/*` и SpaceCore triggers.

---

## 8. Чеклист тестирования

Использовать **новый сейв** (или отдельный слот). Отмечать: ✅ / ❌ / N/A.

### Ранняя игра (дни 1–12)

- [ ] **День 1–3:** первая встреча (`eventHarveyFirstMeeting`) — тон «вы», пиджак, без романтики  
- [ ] **Первый осмотр** в клинике (eventsCare) — согласие / отказ, topic после визита  
- [ ] **Первый визит на ферму** — профессиональная забота  
- [ ] **Второй визит** — чай / отказ, topics `topicHarveySecondVisit*`  
- [ ] **День 11, 750♥+:** прогулка `eventHarveyFirstWalk` — без романтичного fork, без «мы пара»  

### Story E1–E8 (по порядку, следить за ♥ и днями недели)

- [ ] **E1** SlipperyPath (ветер, 500♥)  
- [ ] **E2** InsistentExam после E1 (750♥)  
- [ ] **E3** ForestApothecary после E2 (1000♥, Thu–Sat, солнце)  
- [ ] **E4** PierBreath после E3 (1250♥, вечер)  
- [ ] **E5** StormBeside после E4 (1500♥, гроза) — pre-dating тон  
- [ ] **E6** SayItOutLoud после E5 (1750♥)  
- [ ] **E7** TownSip после E6 (2000♥, день, солнце)  
- [ ] **E8** QuietShelf после E7 (2000♥, суббота)  

### Лечение и кризисы (pre-dating)

- [ ] **Первая травма** → topic `topicHarveyNeedsFirstTreatment` → **`HarveyMod_FirstTreatment`** (750♥, клиника)  
- [ ] После FirstTreatment — topic снят, повтор FirstTreatment не идёт  
- [ ] **`HarveyMod_NightCrisis_PreDating`** (6♥+, ночь, после FirstTreatment, **без** букета) — без объятий, профессиональный тон  
- [ ] **`HarveyMod_BirthdayHospital_Friend`** (summer 9, 8♥, **без** dating) — без «только мы» / «самый важный день»  

### Dating-ветки

- [ ] Букет → **`HarveyMod_NightCrisis_Dating`** (не дублирует PreDating)  
- [ ] **`HarveyMod_BirthdayHospital_Dating`** (summer 9) — романтичнее Friend-версии  
- [ ] **`eventHarveyMedicalCheck_Dating`** после письма (только Dating/Married)  
- [ ] **`eventHarveyFirstDate`**, **`eventHarveyMountainDate`**, **`eventHarveyPropose`** — только Dating; propose: «решение только твоё»  

### Шахта

- [ ] **Mine rescue без Dating:** смерть в шахте → **нет** cutscene Харви (C# skip) — осознанно  
- [ ] **Mine rescue Dating:** смерть → утро warp Mine → **`eventHarveyMineRescueDating`** → Hospital → `topicMineInjuryRescue`  
- [ ] **Повтор** rescue → только topic, без повтора кат-сцены  
- [ ] **Нет** `eventHarveyMineInterception` одновременно с rescue-warp (`topicMineRescuePending`)  

### Гроза (storm comfort)

- [ ] Активен `buffStressThunder` + гроза + 750♥  
- [ ] **Farm** (ночь 20:00–26:00) — клиника, не HarveyRoom  
- [ ] **Forest / Town / Mine / Mountain / Desert** — нет навязанных объятий; тон тревога + укрытие  
- [ ] **`eventRescueOperation`** (отдельно: topic `topicRescueOperation`, гроза) — consent на контакт  

### Прочее

- [ ] **`eventHarveyPropose`** — 10♥, Dating, без «решение за нас обоих»  
- [ ] F10 / SMAPI log: `[MineRescue]`, topics без «залипания»  
- [ ] Сохранение / загрузка mid-rescue — resume события  

---

## Изменённые файлы (справка)

| Файл | Тип правок |
|------|------------|
| `HarveyOverhaul [CP]/assets/Code/events.json` | Условия split, Story, storm, rescue, propose, FirstTreatment |
| `HarveyOverhaul [CP]/assets/Code/eventsCare.json` | Контакт, interception, exhaustion |
| `HarveyOverhaul [CP]/assets/Code/eventsMineRescue.json` | Dating gate, legacy/minor guards, тон |
| `HarveyOverhaul [CP]/assets/Code/triggersCare.json` | `topicMineRescuePending` в условиях |
| `HarveyOverhaul [CP]/assets/Code/events_for_mode_new_formatted.json` | MyMod (не в content.json) |
| `HarveyOverhaulInjury/Core/Constants.cs` | Topics, event IDs |
| `HarveyOverhaulInjury/EventHandlers/PassOutHandler.cs` | Mine rescue logic, pending topic |
| `HarveyOverhaulInjury/Managers/DialogueManager.cs` | `topicHarveyNeedsFirstTreatment` |
| `HarveyOverhaulInjury/Managers/InjuryManager.cs` | First treatment topic on injury |
| `docs/harvey-relationship-visits-audit/*.md` | Детальные аудиты |

---

## Связанные аудиты

- `docs/harvey-relationship-visits-audit/romantic-tone-audit.md` — романтика без Dating gate  
- `docs/harvey-relationship-visits-audit/physical-contact-audit.md` — телесный контакт  
- `docs/harvey-relationship-visits-audit/controlling-lines-replacements.md` — контролирующие реплики  

*Конец отчёта.*
