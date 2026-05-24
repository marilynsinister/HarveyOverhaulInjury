# CP Event Review Checklist

Короткий чеклист для **анализа, правки или создания** одного CP-события HarveyOverhaul.
Копируй нужный блок в промпт Cursor вместе с Event ID и задачей.

**Полные правила:** [`docs/EventPatterns/cp-event-authoring-rules.md`](../EventPatterns/cp-event-authoring-rules.md)  
**Animation / showFrame (сидение):** [`cp-event-authoring-rules.md`](cp-event-authoring-rules.md)  
**Паспорта карт:** [`map-passports.md`](map-passports.md)  
**Список «не трогать»:** [`events-map-audit-plan.md`](events-map-audit-plan.md) → «Исключены из проверки»  
**Backlog известных проблем:** [`events-map-fix-backlog.md`](events-map-fix-backlog.md)  
**Примеры из проекта:** cp-event-authoring-rules.md §18

**Файлы событий:** `assets/Code/events.json`, `eventsCare.json`, `eventsMineRescue.json`

---

## Шаблон для промпта (скопировать целиком)

```text
Задача по CP-событию: <Event ID> — <что сделать: анализ / правка / новое>

Пройди чеклист docs/CheckEvent/cp-event-review-checklist.md.
Открыть cp-event-authoring-rules.md и map-passport локации <Location>.
Править только один Event ID. Условия и реплики не менять (если задача не про тексты).
В конце — отчёт: что было / что стало / как проверить in-game.
```

---

## Перед анализом

- [ ] Определён Event ID
- [ ] Найден файл события (`events.json` / `eventsCare.json` / `eventsMineRescue.json`)
- [ ] Определена локация (`Data/Events/<Location>` совпадает с координатами setup/warp)
- [ ] Проверено, не входит ли событие в список «не трогать» (`events-map-audit-plan.md`)
- [ ] Проверен backlog/аудит на известные проблемы этого Event ID
- [ ] Открыт map-passport нужной карты
- [ ] Открыт `cp-event-authoring-rules.md`

---

## Координаты

- [ ] Все стартовые координаты существуют на карте (X/Y в пределах width/height)
- [ ] Target `Data/Events/<Loc>` совпадает с локацией всех координат (не BusStop с Hospital-координатами)
- [ ] Farmer не стоит в стене / мебели / двери / warp
- [ ] Harvey не стоит в стене / мебели / двери / warp
- [ ] Temporary actors не стоят в опасных тайлах
- [ ] Койка `(20,5)` Hospital — только через `ignoreCollisions` + `positionOffset 32 -52` + lying animate
- [ ] Двери Hospital `(5,9)` / `(10,13)` — не setup-тайлы
- [ ] Mine: farmer `(17,7)`, Harvey `(17,10)`; не ходить на y ≥ 11
- [ ] Камера показывает активную зону (`viewport` на ключевую зону на больших картах)

---

## Movement

- [ ] Нет `move Actor X Y Direction`, где X и Y одновременно не нули
- [ ] `advancedMove`: каждый сегмент — одна ось; промежуточные тайлы проходимы
- [ ] Длинные маршруты разбиты (≥ 6 тайлов — риск) или заменены на `globalFade` + `warp`
- [ ] Путь проходимый (все промежуточные тайлы Back OK, Buildings пусто)
- [ ] После движения есть `faceDirection` (если дальше speak / emote / animate)
- [ ] Нет `speak` до завершения движения
- [ ] fork / move синхронизирован `pause` (300–600 ms после move в ветке)

---

## Dialogue / emote

- [ ] Actor существует перед `speak` (setup / `warp` / `addTemporaryActor`; после `changeLocation` — re-warp)
- [ ] Emote соответствует реплике и портрету (`$a` ≠ emote 32 Happy)
- [ ] После emote есть `pause` (300–700 ms)
- [ ] Портрет Харви соответствует эмоции и этапу arc (`$0`/`$h`/`$s`/`$u`/`$a`/`$l`/`$8`)
- [ ] `Heart` (20) — только dating/married или явная романтика
- [ ] Нет слишком интимного тона для раннего этапа отношений
- [ ] Нет алкоголя как «лечения» в medical/care-сценах

---

## Animation

- [ ] Animate соответствует направлению (`faceDirection` перед animate)
- [ ] Если сцена продолжается, есть `stopAnimation` / `showFrame` / сброс состояния
- [ ] `startJittering` парный с `stopJittering`
- [ ] `positionOffset` не используется как костыль плохой координаты (±60 px+ → фиксить тайл)
- [ ] Перед следующим `move` — `positionOffset Actor 0 0`, если offset был
- [ ] Поцелуй / лежание / сидение проверены по координатам (§7.5 authoring-rules)
- [ ] Для сидения farmer: `stopAnimation` → `faceDirection farmer 2` → `showFrame farmer 107`
- [ ] Перед showFrame actor дошёл до final tile
- [ ] Перед showFrame есть `stopAnimation`
- [ ] Перед showFrame есть `faceDirection`
- [ ] **Harvey exam:** после `move` — `stopAnimation` + `faceDirection` к farmer, затем `animate … 22 20 21 20`

---

## Branching

- [ ] Все ветки `quickQuestion` / `fork` заканчиваются валидно (`end` / `end dialogue` / `end position`)
- [ ] После каждой ветки персонажи стоят логично (тайл, `faceDirection`, нет overlap)
- [ ] Нет активного движения во время выбора (prompt — actors стоят)
- [ ] Нет забытых `end` / `globalFade`
- [ ] В ветке с `changeLocation` — re-setup всех NPC

---

## Завершение

- [ ] `viewport -1000 -1000` или осознанный финальный кадр
- [ ] `globalFade` используется корректно (перед `changeLocation` — `pause 1500+`)
- [ ] `ambientLight` возвращён к норме, если было сильное затемнение
- [ ] `changeLocation` безопасен: warp всех NPC, viewport заново, `fade false`
- [ ] JSON валиден (`quickQuestion` одной строкой, `\\` в ветках, `\"` в репликах)
- [ ] Изменён только нужный Event ID (один ID на задачу/коммит)
- [ ] Условия (Time, Weather, Friendship, Random) не тронуты — если задача не про них
- [ ] Составлен отчёт: **что было → что стало → источник (backlog/audit/passport) → как проверить in-game**

---

## Мини-отчёт (вставить в конец ответа Cursor)

```text
Event ID:
Файл:
Локация:
Статус: OK / Warning / Broken / needs in-game

Изменения:
- было: …
- стало: …
- источник: …

Проверка in-game:
1. …
2. debug eventbyid <id> (если применимо)
```
