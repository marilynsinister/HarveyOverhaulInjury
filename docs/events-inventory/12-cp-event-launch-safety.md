# Безопасность запуска CP-событий из C#

Аудит C#-путей `location.startEvent(...)`: severe/minor **mine rescue**, **hospital pass-out** (`QueueHospitalEvent`), resume после save.

**Статус:** P0/P1 исправления внесены в `PassOutHandler.cs`, `InjuryState.cs`, `PlayerEventHandler.cs`, `ModEntry.cs` (см. конец документа).

Источник: `EventHandlers/PassOutHandler.cs`, CP: `assets/Code/eventsMineRescue.json`.

---

## Обзор цепочки mine rescue

```
OnUpdateTicked / TrackPassOut
  → NeedsMineRescueEvent = true, Save()

DayStarted (+100 ms)
  → RestoreBuffsFromSnapshot()
  → TriggerMineRescueEvents()
      → warp Mine, _pendingMineRescueEventId = eventId  (in-memory)

OnPlayerWarped (PlayerEventHandler → PassOutHandler)
  → TriggerEventByName(eventId, "Mine")
      → startEvent + eventsSeen.Add
  → clear NeedsMineRescueEvent / PassedOutInMineYesterday
```



---

## Обзор: hospital pass-out cutscenes (2026-05-24)

```
OnPlayerWarped (после сна)
  → critical health ≤10 (не mine) OR WasExhausted (не mine)
  → QueueHospitalEvent(eventHarveyEmergencyCare | eventHarveyExhaustion)
      → warp Hospital, PendingHospitalPassOutEventId
  → OnPlayerWarped Hospital → TryStartLocationEvent
  → onEventFinished → eventsSeen (one-shot)
```

**Minor mine rescue:** `PlayerEventHandler` (Mine entry) → `TryTriggerMinorMineRescue` — отдельный pipeline, не severe DayStarted rescue.

## Checklist по вопросам

| # | Вопрос | Вердикт | Комментарий |
|---|---|---|---|
| 1 | Поиск по `Data/Events/<Location>` | **В целом OK** | `Game1.content.Load<Dictionary<string,string>>("Data/Events/Mine")` + `EventKeyMatches` |
| 2 | Безопасность `StartsWith(eventId)` | **OK при текущей схеме** | Используется `eventId + "/"`, не голый prefix |
| 3 | `Game1.player.eventsSeen` | **Риск** | Ручной `Add(eventId)`; проверка в `IsMineRescueEventAlreadySeen` — только exact match |
| 4 | `eventsSeen` до успешного event | **Да, проблема** | Добавляется сразу после `startEvent`, не после `/end` |
| 5 | Исключение из `startEvent` | **OK** | Inner catch → `false`, `eventsSeen` не трогается → `RunMineRescueFallback` |
| 6 | Пустой / битый script | **Частично OK** | `null`/empty отсеивается; whitespace и runtime-fail mid-event — нет |
| 7 | Mine / Hospital не найдены | **Mine — fallback есть; Hospital — нет проверки** | `RunMineRescueFallback` warp без validate |
| 8 | Fallback topic без кинематики | **OK** | `AddTopic(topicMineInjuryRescue)` + warp Hospital |
| 9 | Ранний сброс rescue-флагов | **Да, проблема** | Флаги сбрасываются при **старте** event, не при завершении; `WasPassedOut` не сбрасывается в rescue-ветке |

---

## 1. Поиск события в `Data/Events/Mine`

```366:392:EventHandlers/PassOutHandler.cs
        private bool TriggerEventByName(string eventId, string locationName)
        {
            ...
                var eventData = Game1.content.Load<...>($"Data/Events/{locationName}");
                ...
                foreach (var kvp in eventData)
                {
                    if (EventKeyMatches(kvp.Key, eventId))
                    {
                        eventScript = kvp.Value;
                        break;
                    }
                }
```

**Что работает:**

- CP-ключи вида `eventHarveyMineRescueDating/GameStateQuery ...` находятся через `StartsWith(eventId + "/")`.
- `MineRescueEventExists` использует ту же логику — согласованный выбор dating vs legacy.

**Риски (низкие):**

- При нескольких ключах с одним `eventId/` (разные preconditions) берётся **первый** из `foreach` — порядок не гарантирован как «лучший» match. Сейчас в CP по одному ключу на ID.
- `ResolveSevereMineRescueEventId` проверяет наличие dating-сцены, но **не** проверяет, что GameStateQuery precondition выполнится при C#-старте (C# обходит CP When — стартует script напрямую). Это **намеренно** (dating уже проверен через `IsDatingOrMarriedToHarvey()`).

---

## 2. `EventKeyMatches` и похожие ID

```360:364:EventHandlers/PassOutHandler.cs
        private static bool EventKeyMatches(string key, string eventId)
        {
            return key.Equals(eventId, StringComparison.OrdinalIgnoreCase)
                || key.StartsWith(eventId + "/", StringComparison.OrdinalIgnoreCase);
        }
```

| Запрос | Ключ в CP | Match? |
|---|---|---|
| `eventHarveyMineRescue` | `eventHarveyMineRescue` | да (Equals) |
| `eventHarveyMineRescue` | `eventHarveyMineRescueDating/...` | **нет** (нет `/` после prefix) |
| `eventHarveyMineRescueDating` | `eventHarveyMineRescueDating/GameStateQuery...` | да (StartsWith) |
| `eventHarveyMinorMineRescue` | `eventHarveyMineRescue` | нет |

**Вывод:** голый `StartsWith(eventId)` был бы опасен (`eventHarveyMineRescue` ⊂ `eventHarveyMineRescueDating`). Текущая реализация с **`eventId + "/"`** — корректна для существующих ID.

**Точечное усиление (опционально):** сравнивать только stem: `key.Split('/')[0].Equals(eventId, ...)`.

---

## 3–4. `eventsSeen`: когда и как

```344:354:EventHandlers/PassOutHandler.cs
        private static bool IsMineRescueEventAlreadySeen(string eventId)
        {
            if (Game1.player.eventsSeen.Contains(eventId))
                return true;
            if (eventId == "eventHarveyMineRescueDating"
                && Game1.player.eventsSeen.Contains("eventHarveyMineRescue"))
                return true;
            return false;
        }
```

```400:405:EventHandlers/PassOutHandler.cs
                try
                {
                    location.startEvent(new StardewValley.Event(eventScript));
                    Game1.player.eventsSeen.Add(eventId);
                    ...
                    return true;
```

| Сценарий | Поведение | Проблема? |
|---|---|---|
| `startEvent` throws | `eventsSeen` **не** добавляется → fallback | OK |
| `startEvent` OK, crash на середине cutscene | `eventsSeen` уже есть, topic может **не** добавиться (script ещё не дошёл до `AddConversationTopic`) | **Да** |
| Повторная смерть в шахте | `IsMineRescueEventAlreadySeen` → только topic, без кат-сцены | OK по дизайну |
| CP `PLAYER_HAS_SEEN_EVENT` в ключе dating | C# стартует script в обход precondition; seen контролирует C# | OK, но дублирует логику CP |

**Важно:** vanilla/CP при нормальном trigger обычно помечает seen **после** event. Здесь — **в момент старта**, что расходится с семантикой `eventsSeen`.

---

## 5. Исключение из `startEvent`

Двойной try/catch: подготовка (Load, lookup) и запуск. При ошибке парсинга/старта:

- возвращается `false`;
- `OnPlayerWarped` вызывает `RunMineRescueFallback` (topic + Hospital);
- `eventsSeen` не портится.

**OK.**

---

## 6. Пустой / битый `eventScript`

| К case | Обработка |
|---|---|
| Ключ не найден | log + `false` → fallback |
| `null` / `""` | `string.IsNullOrEmpty` → `false` → fallback |
| Только пробелы | пройдёт проверку → возможен throw в `Event` ctor → catch → fallback |
| Синтаксически OK, логическая ошибка mid-script | `eventsSeen` уже добавлен, fallback **не** сработает |

**Точечное усиление:** `string.IsNullOrWhiteSpace(eventScript)`.

---

## 7. Локации Mine / Hospital

**Mine (TriggerMineRescueEvents):**

```272:278:EventHandlers/PassOutHandler.cs
            var mineLocation = Game1.getLocationFromName("Mine");
            if (mineLocation == null)
            {
                RunMineRescueFallback(eventId);
                return;
            }
```

**OK** — есть fallback.

**Mine (TriggerEventByName):** повторная проверка `getLocationFromName` — OK.

**Hospital (RunMineRescueFallback):**

```302:303:EventHandlers/PassOutHandler.cs
            string hospital = _config.HospitalLocationName;
            Game1.warpFarmer(hospital, ...);
```

- Нет проверки, что локация существует (кастомный `HospitalLocationName` в config).
- Нет try/catch вокруг warp.
- При fail игрок остаётся в Mine **после** сброса `NeedsMineRescueEvent` — rescue «сгорел».

**Точечное усиление:** validate `Game1.getLocationFromName(hospital) != null`; при fail — только topic + HUD, флаги не сбрасывать (или оставить `NeedsMineRescueEvent` для retry).

---

## 8. Fallback: topic без кинематики

```289:304:EventHandlers/PassOutHandler.cs
        private void RunMineRescueFallback(string eventId)
        {
            _dialogueManager.AddTopic(ConversationTopics.MineInjuryRescue, 2);
            _pendingMineRescueEventId = null;
            _stateManager.State.NeedsMineRescueEvent = false;
            _stateManager.State.PassedOutInMineYesterday = false;
            _stateManager.Save();
            ...
            Game1.warpFarmer(hospital, ...);
        }
```

**OK по смыслу:** игрок получает `topicMineInjuryRescue`, дальше работает `InteractionHandler` / госпитализация.

**Замечание:** fallback не добавляет `eventsSeen` — повторная смерть снова попытается кат-сцену (если флаг rescue снова выставится). Это может быть желаемым или нет.

---

## 9. Ранний сброс `NeedsMineRescueEvent` / `PassedOutInMineYesterday`

### 9a. Успешный `startEvent` (главная проблема)

```55:61:EventHandlers/PassOutHandler.cs
                bool started = TriggerEventByName(...);
                if (started)
                {
                    _pendingMineRescueEventId = null;
                    _stateManager.State.NeedsMineRescueEvent = false;
                    _stateManager.State.PassedOutInMineYesterday = false;
                    _stateManager.Save();
                }
```

Флаги сбрасываются **сразу после вызова `startEvent`**, пока cutscene ещё идёт.

| Последствие | |
|---|---|
| Reload mid-event | retry rescue **невозможен** (флаги false, `eventsSeen` true) |
| Topic не получен | нет автоматического повтора |
| `WasPassedOut` остаётся `true` | при следующем warp в тот же день срабатывает блок 77–97 (`ApplyBadlyHurtSafe` + лишний HUD) |

### 9b. «Уже seen» — сброс без topic-проверки

```262:268:EventHandlers/PassOutHandler.cs
            if (IsMineRescueEventAlreadySeen(eventId))
            {
                _dialogueManager.AddTopic("topicMineInjuryRescue", 2);
                _stateManager.State.NeedsMineRescueEvent = false;
                ...
                return;
            }
```

**OK** — topic добавляется явно.

### 9c. Нет dating — сброс без fallback

```245:250:EventHandlers/PassOutHandler.cs
            if (!_dialogueManager.IsDatingOrMarriedToHarvey())
            {
                _stateManager.State.NeedsMineRescueEvent = false;
                _stateManager.State.PassedOutInMineYesterday = false;
                return;
            }
```

Rescue «потреблён», но ни event, ни topic — если флаги были выставлены при dating, а утром отношений нет (edge case).

### 9d. `_pendingMineRescueEventId` не в save

Между `warp Mine` и `OnPlayerWarped`: crash/reload → pending потерян, `NeedsMineRescueEvent` ещё true → **повтор только на следующий `DayStarted`**, не сразу после reload.

---

## Дополнительный риск: порядок `Warped`

`ModEntry`: `PlayerEventHandler.OnWarped` **раньше** `PassOutHandler.OnPlayerWarped`.

При rescue-warp в Mine с severe-травмами:

1. `HandleMineEntryWarning()` — HUD «не ходи в шахту» **перед** rescue-cutscene.
2. Затем `TriggerEventByName`.

Ложное предупреждение на **штатном** rescue-warp (см. `09-timing-audit.md`).

---

## Предлагаемые точечные исправления

Без переписывания архитектуры — только `PassOutHandler` (+ минимально `InjuryState` / `PlayerEventHandler`).

### P0 — must fix

**1. Отложить `eventsSeen` и сброс флагов до конца event**

```csharp
var evt = new Event(eventScript);
string capturedId = eventId;
evt.onEventFinished += (_, __) =>
{
    if (!Game1.player.eventsSeen.Contains(capturedId))
        Game1.player.eventsSeen.Add(capturedId);
    _stateManager.State.NeedsMineRescueEvent = false;
    _stateManager.State.PassedOutInMineYesterday = false;
    _stateManager.State.WasPassedOut = false;
    _stateManager.State.WasExhausted = false;
    _stateManager.State.WasUpTooLate = false;
    _pendingMineRescueEventId = null;
    _stateManager.Save();
};
location.startEvent(evt);
return true; // не сбрасывать флаги в OnPlayerWarped при started
```

В `OnPlayerWarped` при `started == true` — **не** чистить флаги (это делает callback). При `false` — fallback как сейчас.

**2. Persist pending event id**

В `InjuryState`:

```csharp
public string PendingMineRescueEventId { get; set; } = "";
```

- Записывать при warp, очищать в `onEventFinished` / fallback.
- В `OnPlayerWarped`: если `NeedsMineRescueEvent && player already in Mine && pending set` — retry `TriggerEventByName` (reload recovery).

### P1 — should fix

**3. Hospital validate в fallback**

```csharp
if (Game1.getLocationFromName(hospital) == null)
{
    _monitor.Log("[MineRescue] Hospital not found — topic only, keeping NeedsMineRescueEvent for retry", LogLevel.Error);
    _dialogueManager.AddTopic(...);
    // НЕ сбрасывать NeedsMineRescueEvent
    return;
}
```

**4. Suppress mine warning на rescue-warp**

В `PlayerEventHandler.HandleMineEntryWarning()` в начале:

```csharp
if (_stateManager.State.NeedsMineRescueEvent)
    return;
```

(или проверять непустой `PendingMineRescueEventId` после P0.)

**5. Сброс `WasPassedOut` в rescue-ветке**

Даже до P0 — в блоке `if (started)` добавить сброс pass-out флагов (или перенести в callback из P0).

### P2 — nice to have

**6. `IsNullOrWhiteSpace` для script**

**7. Проверка `Game1.currentLocation == location` перед `startEvent`**

**8. `IsMineRescueEventAlreadySeen` — дополнить проверкой topic**

```csharp
if (Helpers.GameUtils.HasConversationTopic("topicMineInjuryRescue"))
    return true; // уже обработано без повторной кат-сцены
```

**9. Safety net: если event finished, а topic нет — добавить topic в callback**

```csharp
if (!HasConversationTopic("topicMineInjuryRescue"))
    _dialogueManager.AddTopic(...);
```

---

## Сводка приоритетов

| Приоритет | Проблема | Fix |
|---|---|---|
| P0 | `eventsSeen` до `/end` | `onEventFinished` |
| P0 | Флаги rescue сброшены при старте, не при fin | тот же callback |
| P0 | `_pendingMineRescueEventId` не в save | поле в `InjuryState` |
| P1 | Hospital warp без validate | check + не сбрасывать flag |
| P1 | Mine warning на rescue-warp | skip if `NeedsMineRescueEvent` |
| P1 | `WasPassedOut` после rescue | clear в callback / rescue block |
| P2 | whitespace script, currentLocation check | мелкие guards |

---

## Связанные документы

- [09-timing-audit.md](09-timing-audit.md) — порядок DayStarted/Warped, race mine warning
- [11-id-sync-audit.md](11-id-sync-audit.md) — `topicMineInjuryRescue`, CP events/mail

**Статус:** исправления P0/P1 применены (2025-05).

### Внесённые изменения

| Fix | Файл |
|---|---|
| `onEventFinished` → `eventsSeen` + `ClearMineRescueState` | `PassOutHandler.cs` |
| `PendingMineRescueEventId` в save | `InjuryState.cs` |
| `ResumePendingMineRescueIfNeeded` на SaveLoaded | `ModEntry.cs` |
| Skip mine warning при `NeedsMineRescueEvent` | `PlayerEventHandler.cs` |
| Hospital validate в fallback | `PassOutHandler.cs` |
| `EnsureMineRescueTopic` safety net | `PassOutHandler.cs` |
| `IsNullOrWhiteSpace`, проверка currentLocation | `PassOutHandler.cs` |
