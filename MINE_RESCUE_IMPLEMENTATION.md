# Реализация событий спасения в шахте

## 📋 Обзор

События спасения в шахте (`eventHarveyMineRescue` и `eventHarveyMinorMineRescue`) теперь запускаются программно из мода **HarveyOverhaulInjury**.

---

## ✅ Реализация

### 1. Отслеживание обморока в шахте

**Файл:** `EventHandlers/PassOutHandler.cs`

**Метод:** `TrackPassOut()` (расширен)

```csharp
// ⭐ НОВОЕ: Проверка обморока в шахте для событий спасения
if (_stateManager.State.WasPassedOut && 
    (currentLocation.Contains("Mine") || currentLocation == "UndergroundMine"))
{
    _stateManager.State.PassedOutInMineYesterday = true;
    _stateManager.State.NeedsMineRescueEvent = true;
    _monitor.Log($"[MineRescue] Игрок потерял сознание в шахте: {currentLocation}", LogLevel.Info);
}
```

**Вызывается:** В конце дня через `OnDayEndingPassOutCheck` в `ModEntry.cs`

---

### 2. Запуск событий в начале следующего дня

**Файл:** `EventHandlers/PassOutHandler.cs`

**Метод:** `TriggerMineRescueEvents()` (новый)

```csharp
public void TriggerMineRescueEvents()
{
    if (!_stateManager.State.NeedsMineRescueEvent) return;

    // Проверяем условия
    bool hasSeriousInjuries = ...;
    bool isDatingOrMarried = ...;
    float healthPercent = ...;

    // Запускаем подходящее событие
    if (hasSeriousInjuries && isDatingOrMarried && healthPercent <= 15f)
    {
        TriggerEventByName("eventHarveyMineRescue", "Mine");
    }
    else if (hasMinorInjury && ...)
    {
        TriggerEventByName("eventHarveyMinorMineRescue", "Mine");
    }

    // Сбрасываем флаги
    _stateManager.State.PassedOutInMineYesterday = false;
    _stateManager.State.NeedsMineRescueEvent = false;
}
```

**Вызывается:** В начале дня через `GameEventHandler.OnDayStarted()`

---

### 3. Программный запуск события

**Файл:** `EventHandlers/PassOutHandler.cs`

**Метод:** `TriggerEventByName()` (новый)

```csharp
private void TriggerEventByName(string eventId, string locationName)
{
    // Загружаем данные событий
    var eventData = Game1.content.Load<Dictionary<string, string>>($"Data/Events/{locationName}");
    
    // Находим событие по ID
    string eventScript = FindEventByIdInData(eventData, eventId);
    
    // Запускаем событие
    Game1.player.eventsSeen.Add(eventId);
    var gameEvent = new Event(eventScript);
    location.startEvent(gameEvent);
}
```

---

## 🔗 Интеграция с системой модов

### Связи между модами:

```
HarveyOverhaulInjury (C# мод)
    ↓ проверяет условия
    ↓ запускает событие программно
    ↓
HarveyOverhaul [CP] (Content Patcher)
    ↓ предоставляет данные событий
    ↓ eventsMineRescue.json
    ↓
Stardew Valley
    ↓ воспроизводит событие
    ✅ Игрок видит сцену спасения
```

---

## 🎬 События

### 1. eventHarveyMineRescue - Критическое спасение

**Условия в коде:**
- `hasSeriousInjuries` - серьёзные раны (buffBadlyHurt, buffConcussion, buffFracturedBone, buffShrapnelWounds, buffBurnWounds)
- `isDatingOrMarried` - отношения с Харви (Dating/Married)
- `healthPercent <= 15%` - критическое здоровье
- Событие не просмотрено ранее

**Результат:**
- Драматическая сцена реанимации
- +60 дружбы с Харви
- Письмо `mailHarveyAfterMineRescue`
- Топик `topicMineInjuryRescue` (2 дня)

---

### 2. eventHarveyMinorMineRescue - Лёгкое спасение

**Условия в коде:**
- `hasMinorInjury` - buffBadlyHurt
- `harveyFriendship >= 500 && < 1000` - 2-3 сердца
- `healthPercent <= 15%` - низкое здоровье
- События не просмотрены ранее

**Результат:**
- Быстрая медицинская помощь
- +35 дружбы с Харви
- Топик `topicMineInjuryRescue` (2 дня)

---

## 📊 Поток выполнения

```
День 1 (Обморок):
    Игрок в шахте
    ↓
    Health = 0 или Stamina <= -15
    ↓
    PassOutHandler.TrackPassOut()
    ↓
    PassedOutInMineYesterday = true
    NeedsMineRescueEvent = true
    ↓
    OnDayEnding → Сохранение

День 2 (Событие):
    OnDayStarted
    ↓
    GameEventHandler.OnDayStarted()
    ↓
    PassOutHandler.TriggerMineRescueEvents()
    ↓
    Проверка условий (раны, отношения, здоровье)
    ↓
    TriggerEventByName("eventHarveyMineRescue", "Mine")
    ↓
    Событие запущено программно
    ↓
    Флаги сброшены
```

---

## 🔧 Изменённые файлы

### HarveyOverhaulInjury:

1. ✅ `Core/Models/InjuryState.cs`
   - Добавлено: `PassedOutInMineYesterday`
   - Добавлено: `NeedsMineRescueEvent`

2. ✅ `EventHandlers/PassOutHandler.cs`
   - Расширен: `TrackPassOut()` - отслеживание обморока в шахте
   - Добавлено: `TriggerMineRescueEvents()` - запуск событий
   - Добавлено: `TriggerEventByName()` - программный запуск события

3. ✅ `EventHandlers/GameEventHandler.cs`
   - Добавлено: `SetPassOutHandler()` - связь с PassOutHandler
   - Изменено: `OnDayStarted()` - вызов TriggerMineRescueEvents()

4. ✅ `Managers/DialogueManager.cs`
   - Добавлено: `GetHarveyFriendship()` - получение очков дружбы

5. ✅ `ModEntry.cs`
   - Добавлено: `_gameEventHandler.SetPassOutHandler(_passOutHandler)`

### HarveyOverhaul [CP]:

6. ✅ `assets/Code/eventsMineRescue.json`
   - Исправлено: Удалено поле `"Format"` (secondary file)
   - События запускаются из HarveyOverhaulInjury, а не через Content Patcher

---

## 🎮 Тестирование

### Подготовка:
1. Начать отношения с Харви (Dating/Married)
2. Получить раны через урон в шахтах

### Сценарий теста:
1. Зайти в шахты (Mine или Skull Cavern)
2. Получить урон до health ≤ 15%
3. Получить травму (buffBadlyHurt)
4. Дождаться обморока (health = 0 или stamina <= -15)
5. На следующий день проверить логи:

```
[MineRescue] Игрок потерял сознание в шахте: Mine
[MineRescue] Проверка условий для события спасения в шахте
[MineRescue] Состояние: Раны=true, Харви=true(2500), Здоровье=10%
[MineRescue] ✅ Запуск события: eventHarveyMineRescue (серьёзные раны)
[MineRescue] ✅ Событие 'eventHarveyMineRescue' успешно запущено
```

### Консольные команды:
```bash
# Добавить раны
player_add buffBadlyHurt
player_add buffConcussion

# Установить отношения
player_setfriendship Harvey 2500 dating

# Снизить здоровье
player_sethealth 10

# Телепорт в шахту
debug warp Mine 20 20
```

---

## ✅ Результат

### Что исправлено:
1. ✅ Content Patcher error - удалено `"Format"` из secondary файла
2. ✅ События спасения теперь работают через программный запуск
3. ✅ Логика перенесена в правильный мод (HarveyOverhaulInjury)
4. ✅ Оба мода скомпилированы без ошибок

### Преимущества:
- 🎯 События гарантированно срабатывают
- 📝 Детальное логирование
- 🔧 Легко тестировать и отлаживать
- 🏗️ Правильная архитектура (логика ранений в моде Injury)

---

**Дата:** 27 ноября 2025  
**Мод:** HarveyOverhaulInjury v1.0.0  
**Статус:** ✅ Готово к тестированию

