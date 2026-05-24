# Технический бриф для написания event script (Content Patcher)

**Мод:** Harvey Overhaul: Injury & Care  
**Событие:** `eventHarveyMineRescue` (major — боевая смерть / обморок в шахте)  
**Тон:** тревожный → медицински собранный → в клинике строгий контроль → мягкий финал  
**Длительность:** ~60–90 с  
**Источники:** `Mine_event_placement_analysis.md`, `Hospital_event_placement_analysis.md`, `eventHarveyMineRescue_blocking_sheet.md`

> **Задача исполнителя:** написать строку события для CP (`EditData` → `Data/Events/Mine`).  
> **Не делать:** JSON-обёртку CP, правки C#, `content.json`, Harmony, кастомные карты/спрайты.

---

## 1. ID события

```
eventHarveyMineRescue
```

**Связанное (не путать):** `eventHarveyMinorMineRescue` — отдельное лёгкое спасение, другие координаты Hospital. Этот бриф только для **major**.

---

## 2. Target карты (Content Patcher)

| Поле | Значение |
|------|----------|
| **EditData Target** | `Data/Events/Mine` |
| **Entry key** | `eventHarveyMineRescue` |

Событие **запускается** на локации `Mine` (C#: `TriggerEventByName("eventHarveyMineRescue", "Mine")`).  
Внутри строки события используется **`changeLocation Hospital`** для второй части.

**Файл мода (ориентир):** `HarveyOverhaul [CP]/assets/Code/eventsMineRescue.json` — патч уже подключён через `content.json` Include; **не менять** структуру `content.json`, только содержимое entry при необходимости.

---

## 3. Карты внутри сцены

| Порядок | Location name | Роль |
|--------|---------------|------|
| 1 | `Mine` | Падение, появление Харви, осмотр, решение везти в клинику |
| 2 | `Hospital` | Игрок на кровати, Харви у постели, запрет ухода, topic |

**Зависимость:** локация `Hospital` из **Stardew Valley Expanded (CP)**. Без SVE карта/имя могут не совпасть.

---

## 4. Безопасные координаты — Mine

Карта **77×20**, вход в шахту (платформа y=6–10, ось **x=17**).

| Роль | X | Y | Facing | Примечание |
|------|---:|---:|--------|------------|
| Игрок (старт) | **17** | **7** | **2** (вниз) | Совпадает с C# warp перед событием |
| Харви (spawn) | **17** | **10** | **0** (вверх) | Предпочтительно: `Harvey 1000 1000` → `warp Harvey 17 10` |
| Харви (после подхода) | **17** | **8** | **0** | `move Harvey 0 -2 0` с (17,10) — путь проверен |
| Камера (основная) | **17** | **7** | — | `viewport 17 7 true` |
| Камера (опц.) | **17** | **8** | — | После подхода Харви |

**Запасные (только если move ломается):** игрок (16,7) или (18,7); Харви warp (16,10) или (18,10).

**Старт события (рекомендуемый префикс):** `farmer 17 7 2` / `Harvey 1000 1000 0` (как в текущем CP).

---

## 5. Безопасные координаты — Hospital

Карта **24×20**, SVE, **палата B** (правая кровать). Совпадает с `ModConfig`: `HospitalBedX=20`, `HospitalBedY=5`.

| Роль | X | Y | Facing | Примечание |
|------|---:|---:|--------|------------|
| Игрок (кровать) | **20** | **5** | **2** (лёжа); потом **3** при «пробуждении» | **Обязательно:** `ignoreCollisions` + `positionOffset 32 -52` + lying `animate` frames **4 5** |
| Харви (у постели) | **19** | **5** | **1** (вправо, к игроку) | `warp Harvey 19 5` + `faceDirection Harvey 1` |
| Камера (основная) | **14** | **6** | — | `viewport 14 6` — видны оба и палата |
| Камера (опц. крупно) | **19** | **5** | — | Финальные реплики |

**Подход Харви (опционально):** spawn **(20, 6)** → move только до **(19, 5)**, **не** на (20,5).

**Не использовать палату A:** (14,6) / (15,6) — только для `eventHarveyMinorMineRescue`.

---

## 6. Последовательность сцен (blocking)

### Mine (шаги 1–10)

1. Затемнение: `ambientLight 40 40 40`, `fade true`, игрок **(17,7)** неподвижен.  
2. `pause` ~800.  
3. `message` (от лица игрока) + `playSound thudStep`.  
4. `warp Harvey 17 10`, `fade false`, `viewport 17 7 true`.  
5. `move Harvey 0 -2 0` → **(17,8)**.  
6. `emote Harvey 16` + speak (испуг, `$8`).  
7. `animate Harvey` 22 20 21 + speak (пульс/осмотр, `$8`).  
8. `emote Harvey 12` + speak (везу в клинику, `$a`).  
9. Короткий speak (`$s`) + `pause` ~600.  
10. `globalFade`, `viewport -1000 -1000`, `pause` ~1400.

### Переход (шаг 11)

11. `changeLocation Hospital`

### Hospital (шаги 12–21)

12. `ignoreCollisions farmer` / warp **20 5** / `positionOffset 32 -52` / lying animate / `warp Harvey 19 5` / `faceDirection` / `viewport 14 6` / свет **180 180 180**.  
13. Speak: в клинике, не вставать (`$u`).  
14. Speak: раны, кровотечение, обезболивающее (`$u`); опц. `playSound dwop`.  
15. Speak + emote: наблюдение, уход запрещён (`$a`).  
16. `message`: открываешь глаза.  
17. Опц.: `stopAnimation farmer`, `faceDirection farmer 3`, `emote farmer 8`, speak.  
18. Speak: вчера шахта, лечение (`$s`).  
19. Speak + `emote 28`: буду рядом (`$l`).  
20. `action AddConversationTopic topicMineInjuryRescue 2`, `friendship Harvey` (+40…+60), опц. `mail mailHarveyAfterMineRescue`.  
21. `globalFade`, `end dialogue` (короткий хвост).

**Без Maru** в этой версии сцены (по постановке «Харви не отпускает»).

---

## 7. Топики / флаги после события

### Должно быть в event script

| ID | Команда | Срок | Назначение |
|----|---------|------|------------|
| `topicMineInjuryRescue` | `action AddConversationTopic topicMineInjuryRescue 2` | 2 дня | C#: госпитализация, диалоги, `HandleHospitalLogic` |
| `mailHarveyAfterMineRescue` | `mail` в конце (опц.) | — | Письмо на след. день; entry в `Data/Mail` того же JSON |
| Friendship | `friendship Harvey N` | — | N ≈ 40–60 (баланс мода) |

### Уже на стороне C# (не дублировать в событии)

- `NeedsMineRescueEvent`, `PassedOutInMineYesterday` — выставляются **до** события.  
- Сброс флагов rescue после успешного старта — `PassOutHandler.OnPlayerWarped`.  
- `ApplyBadlyHurtFromMinePassOut` — бафф до/вне cutscene.  
- Принудительная госпитализация при входе в Hospital — по **topic** + Severe + `ForceHospitalization`.  
- `WarpToHospitalBed()` → **(20, 5)** при hosp.  
- Удаление `topicMineInjuryRescue` при старте госпитализации — C#, не в событии.

### Не добавлять в событие

- Новые custom flags / mail без entry в `Data/Mail`.  
- `RemoveConversationTopic topicMineInjuryRescue` (удалит C# при hosp).  
- Второй NPC (Maru) без отдельного ТЗ.

---

## 8. Что нельзя делать

1. Менять **C#** (`PassOutHandler`, `ModConfig`, `HospitalizationManager`).  
2. Менять **`content.json`** (только тело события в `eventsMineRescue.json`).  
3. Использовать **кастомные карты**, новые спрайты, Harmony.  
4. **move** игрока в Mine; **move** Харви на y≥11 или через края карты.  
5. Ставить игрока на **(20,5)** без `ignoreCollisions` + `positionOffset` + lying animate.  
6. **move** Харви на тайл кровати **(20,5)** или **(21,5)**.  
7. Переносить major-сцену на **(14,6)** Hospital — рассинхрон с кроватью мода.  
8. Вызывать **Maru** / толпу NPC в тесной палате 24×20.  
9. Удалять или не добавлять **`topicMineInjuryRescue`** — сломается цепочка мода.  
10. Запускать событие с Target `Data/Events/Hospital` — C# ждёт старт на **Mine**.

---

## 9. Запрещённые координаты

### Mine

| X | Y | Причина |
|---:|---:|---------|
| 17 | 3 | MineElevator |
| 15–21 | 0–3 | Стена лифта |
| 23 | 9 | Спуск в шахту |
| 11–12 | 10 | Вагонетка |
| * | 11+ | Обрыв / Front, NPC stuck |
| 17 | 12–13 | Скала / void |
| ≤13 | 6–10 | Пустота запад |
| ≥22 | 9+ | Спуск / край |
| 67 | 18 | Варп, край карты |

### Hospital

| X | Y | Причина |
|---:|---:|---------|
| 20 | 5 | Кровать — только с ignoreCollisions + offset (не для walk) |
| 21 | 5 | Тайл кровати 1101 |
| 19 | 4 | Изголовье-блок |
| 20–21 | 4 | Рама кровати |
| 18–22 | 3 | Шкафы/стена |
| 9–10 | 5 | Door Harvey |
| 5 | 9, 13 | Двери коридора |
| 14 | 6 | Палата A (minor only) |
| 11–15 | 15–17 | Нижний зал, другие кровати |
| 5–7 | 16 | HospitalShop |

---

## 10. Ручная проверка в игре (чеклист)

После написания script — тест с **SVE**, отношения с Харви (dating/married), severe injury / mine death:

| # | Что проверить | Ожидание |
|---|---------------|----------|
| 1 | C# debug / смерть в шахте → утро → запуск события | Телепорт Mine **(17,7)**, событие стартует без ошибки в логе |
| 2 | Mine: игрок виден, не в стене | **(17,7)**, лицом вниз |
| 3 | Mine: Харви появляется и подходит | **(17,10)→(17,8)**, без застревания |
| 4 | Mine: камера | Оба в кадре, не пустой край |
| 5 | Переход `changeLocation Hospital` | Без вылета; загрузка SVE Hospital |
| 6 | Hospital: игрок на кровати | Спрайт на одеяле (offset); не в полу/стене |
| 7 | Hospital: Харви **(19,5)** | Смотрит на игрока, не в двери |
| 8 | Hospital: камера **(14,6)** | Видны кровать, Харви, стены палаты |
| 9 | После события: topic | `topicMineInjuryRescue` в соц. панели / логе |
| 10 | Вход в Hospital после события | Госпитализация / диалог (если Severe + config) |
| 11 | Позиция после hosp warp | Игрок **(20,5)** совпадает с событием |
| 12 | Повторный просмотр | C# не должен зациклить (флаги сброшены) |
| 13 | Длительность | ~60–90 с, без «зависания» на animate |

**Если offset кровати съехал:** подбирать только `positionOffset` (база **32 -52** из текущего CP), координаты тайлов не менять.

---

## Справка: SDV event syntax (напоминание)

- Facing: **0** up, **1** right, **2** down, **3** left.  
- `move NAME dx dy facing` — с (17,10) move `0 -2` = на север.  
- Harvey emotes: **16** shock, **12** concern, **28** relief.  
- Harvey medical animate: **22 20 21** (как в текущем CP).  
- Farmer lying: animate **4 5** + `positionOffset 32 -52`.  
- Dialogue tokens: `$8` fear, `$a` angry/strict, `$s` sad, `$u` serious, `$l` love soft, `$h` happy (избегать в Mine).

---

## Связанные файлы в репозитории

| Файл | Содержание |
|------|------------|
| `tmpMap/Mine_event_placement_analysis.md` | Коллизии Mine |
| `tmpMap/Hospital_event_placement_analysis.md` | Коллизии Hospital |
| `tmpMap/eventHarveyMineRescue_blocking_sheet.md` | Пошаговая раскадровка |
| `EventHandlers/PassOutHandler.cs` | Запуск события, warp 17,7 |
| `Core/ModConfig.cs` | HospitalBed 20,5 |
| `HarveyOverhaul [CP]/assets/Code/eventsMineRescue.json` | Текущий патч (референс) |
