# Аудит телесного контакта в событиях Харви

Дата: 2026-05-23  
Файлы CP: `assets/Code/events.json`, `eventsCare.json`, `eventsMineRescue.json`, `events_for_mode_new_formatted.json`

## Правило

| Стадия | Контакт |
|--------|---------|
| Pre-dating | Медицински необходимый / лёгкий / с явным согласием |
| Dating/Married | Допустима большая близость |
| Кризис (pre-dating) | Помощь без романтизации |

## Сводная таблица

| Сцена | Файл | Gate | Контакт | Статус |
|-------|------|------|---------|--------|
| `eventHarveyFirstMeeting` | eventsCare | — | пиджак на плечи, протягивает еду | ✅ OK |
| `eventHarveyFirstWalk` | events.json | pre | *протягивает руку* | ✅ OK |
| `eventHarveyCheckHealthFarmer` | events.json | Dating | запястье, под руку, койка | ✅ Dating |
| `eventHarveyStormComfort*` (5 лок.) | events.json | 3♥ | словесная поддержка (анимация объятий закомментирована) | ✅ OK |
| `HarveyOverhaulStory.E5` | events.json | 6♥ story | марля в ладонь (grounding) | ✅ OK |
| `HarveyOverhaulStory.E6` | events.json | 7♥ story | без касания; «не приближаясь» | ✅ OK |
| `HarveyMod_FirstTreatment` | events.json | 3♥ + injury | ~~нежно берёт за руку~~ | 🔧 исправлено |
| `HarveyMod_NightCrisis_PreDating` | events.json | 6♥ | без объятий | ✅ OK |
| `HarveyMod_NightCrisis_Dating` | events.json | Dating | обнимает, усаживает | ✅ Dating |
| `eventHarveyMedicalCheck` / `_Dating` | events.json | split | осмотр, без лишней близости | ✅ OK |
| `eventHarveyRoomCheckup` | events.json | 6♥ | ~~ловит за руку~~ | 🔧 исправлено |
| `eventHarveyRoomCheckup2` | events.json | Dating | перехват у окна (безопасность) | ✅ Dating |
| `eventHarveyExhaustion` | eventsCare | кризис | ~~гладит по волосам~~ | 🔧 исправлено |
| `eventHarveyEmergencyCare` | eventsCare | кризис | подхватывает на руки (message) | ✅ мед. |
| `eventHarveyMineInterception` | eventsCare | лечение | ~~берёт за руки / крепко держит~~ | 🔧 исправлено |
| `eventRescueOperation` | events.json | 2♥+ topic | ~~объятие, гладит, целует в макушку~~ | 🔧 исправлено |
| `eventHarveyMineRescue` | eventsMineRescue | кризис | поднимает на руки | ✅ мед. |
| `eventHarveyMineRescueDating` | eventsMineRescue | Dating | несёт, клиника | ✅ Dating |
| `eventHarveyFirstDate` | events.json | Dating | ладонь | ✅ Dating |
| `eventHarveyMountainDate` | events.json | Dating | анимация близости | ✅ Dating |
| `eventHarveyPropose` | events.json | Dating | одеяло, рука, объятие в воде | ✅ Dating |
| `HarveyMod_BirthdayHospital_*` | events.json | split | без телесного | ✅ OK |

## Применённые замены (diff)

### `HarveyMod_FirstTreatment` — events.json

```diff
- *нежно берёт за руку* Ты слишком важна для меня.
+ *протягивает руку, но ждёт твоего ответа* Ты слишком важна для меня — как пациентка.
```

### `eventHarveyRoomCheckup` — events.json

```diff
- message "Ты пытаешься сбежать, но Харви ловит тебя за руку."
+ message "Ты пытаешься сбежать, но Харви останавливает тебя за локоть — мягко, но настойчиво."
```

### `eventHarveyExhaustion` — eventsCare.json

```diff
- *гладит по волосам* Дай мне позаботиться о тебе.$l
+ *отходит на шаг, чтобы не давить* Дай мне позаботиться о тебе.$0
```

### `eventHarveyMineInterception` — eventsCare.json

```diff
- *берёт тебя за руки* ... (диагностика)
+ *осторожно берёт за запястья — проверяет пульс* ...

- message "Харви ведёт тебя прочь от шахт, крепко держа за руку."
+ message "Харви ведёт тебя прочь от шахт, поддерживая за локоть."
```

### `eventRescueOperation` — events.json

```diff
- *обнимает очень осторожно* ... *гладит по волосам* ... Навсегда.$l
+ *медленно протягивает руки — ждёт, пока ты сама не ответишь* ... *кладёт ладонь на плечо — ненадолго* ...$0

- *целует в макушку* ...
+ *опускает лоб к твоему — коротко, почти не касаясь* ...

- *крепко обнимает* / *держит за руки*
+ *обнимает — только потому что ты не отстраняешься* / *сжимает ладонь — если ты сама не отпрятала руку*
```

## Не трогали (намеренно)

- **Dating/Married** сцены: propose, first date, mountain date, night crisis dating, room checkup2.
- **Медицинский кризис**: pulse/wrist, carry from mine, hospital bed — без романтических `$l` там, где правили.
- **Закомментированные** блоки в events.json.
- **dialoguesHarveyPregnant.json** / stress dialogues — вне scope «событий».

## Рекомендации на будущее

1. `eventRescueOperation` при желании можно разделить на `_Friend` / `_Dating` (как NightCrisis).
2. Storm comfort: добавить `!Dating` ветку с более сухим тоном «останься в клинике» vs romantic sleepover fork.
3. `HarveyOverhaulStory.E6` fork «безопасность»: смягчить слова «защищать» (не контакт, но тон).
