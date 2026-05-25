# Тест-кейсы: запрет шахты (HarveyMod_MineForbidden)

Документ для ручной проверки цепочки **Severe-травма → предупреждение → письмо → дебафф → перехват → снятие запрета**.

Подробное описание механики: [`mines-forbidden-injuries.md`](mines-forbidden-injuries.md).

---

## Название механики

**Запрет шахты после тяжёлой травмы** — после входа в подземелье с Severe-травмой Харви предупреждает игрока; на следующий день приходит письмо и накладывается дебафф `HarveyMod_MineForbidden`. Пока дебафф активен, вход в шахту блокируется (катсцена или HUD + warp), вулкан обрабатывается отдельно.

---

## Предусловия

| Условие | Зачем | По умолчанию |
|--------|-------|--------------|
| **Severe-дебафф** | Запускает цепочку предупреждения | Один из 7 ID ниже |
| **`SendLetters = true`** | Письмо `mailHarveyMineForbidden` планируется в конце дня | В конфиге мода |
| **`ForceHospitalization`** | **Не** нужен для запрета шахты; только для госпитализации с `topicMineInjuryRescue` | По желанию |
| **Dating/Married с Харви** | **Не** требуется для C#-логики `HarveyMod_MineForbidden` | CP-триггер `triggerHarveyMineWarning` требует Dating/Married, но при активном дебаффе событие запускается из **C#** (`PlayerEventHandler`) |
| **HarveyOverhaul [CP]** | Катсцена `eventHarveyMineInterception` в `Data/Events/Mine` | Должен быть установлен |
| **Нет `NeedsMineRescueEvent`** | Иначе предупреждение в шахте подавляется до rescue-cutscene | Проверить через `injury_debug_dump` |

### Severe-дебаффы (InjurySets.Severe)

| ID |
|----|
| `buffBadlyHurt` |
| `buffConcussion` |
| `buffFracturedBone` |
| `buffInfectedWound` |
| `buffBurnWounds` |
| `buffShrapnelWounds` |
| `buffSurgicalWound` |

### Поля save-state (InjuryState)

| Поле | Назначение |
|------|------------|
| `MineWarningDay` | День строгого предупреждения; триггер письма на завтра |
| `LastMineSevereWarningDay` | Первое предупреждение Severe за день (переживает reload) |
| `LastMineSevereForcedExitDay` | Принудительный выход при повторном входе до `MineForbidden` |
| `MineForbiddenAppliedDay` | День наложения `HarveyMod_MineForbidden` |
| `LastMineForbiddenInterceptionDay` | День последнего показа `eventHarveyMineInterception` |

Проверка: консоль SMAPI → `injury_debug_dump`.

---

## Команды для подготовки

```text
injury_debuff_add buffBadlyHurt
injury_debuff_add buffConcussion
injury_debuff_add buffFracturedBone
injury_debuff_list
injury_debuff_remove HarveyMod_MineForbidden
injury_debug_dump
```

Вход в локации (SMAPI):

```text
warp Mine 17 7
warp VolcanoDungeon 1 1
```

После warp на Ginger Island для вулкана:

```text
warp IslandNorth 31 17
```

Конфиг (при необходимости): `MineForbiddenDurationDays` по умолчанию **2** игровых дня.

---

## Сценарии

### A. Первый вход в шахту с Severe-травмой (до MineForbidden)

**Подготовка**

1. Убедиться, что **нет** `HarveyMod_MineForbidden` (`injury_debuff_list`).
2. `injury_debuff_add buffBadlyHurt` (или другой Severe).
3. `SendLetters = true` в конфиге.
4. Новый игровой день (или сброс `LastMineSevereWarningDay` через новый день).

**Действия**

1. Войти в шахту: лифт на Mountain **или** `warp Mine 17 7`.
2. Оставаться в шахте или выйти — оба варианта допустимы.

**Ожидаемый результат**

- HUD: «Харви: У тебя серьёзные раны — ты не должна идти в шахту! Возможны осложнения.»
- `MineWarningDay = today`, `LastMineSevereWarningDay = today`.
- **Нет** `HarveyMod_MineForbidden`.
- **Нет** `eventHarveyMineInterception`.
- Игрок **может** остаться в шахте после первого предупреждения.
- Загрязнение раны (DirtyWound) на первом входе **может** сработать — это отдельная механика.

**SMAPI-лог**

```text
⚠️ [Шахта] Вход с серьёзными ранами — предупреждение Харви, письмо и дебафф завтра
```

В конце того же дня (после сна):

```text
[Шахта] Письмо о запрете шахты запланировано на завтра (день предупреждения: …)
```

---

### A2. Повторный вход в тот же день (Severe, до MineForbidden)

Промежуточный шаг между A и B — проверка «мягкого наказания» без дебаффа.

**Подготовка**

- Выполнен сценарий **A** в тот же игровой день.
- Игрок вышел из шахты.

**Действия**

1. Снова войти в шахту / `warp Mine 17 7`.

**Ожидаемый результат**

- HUD: «Харви уже предупреждал тебя. Сегодня шахта закончена.»
- Звук `cancel`.
- Warp на **Mountain** (53, 8).
- **Нет** катсцены.
- `LastMineSevereForcedExitDay = today`.
- `MineWarningDay` по-прежнему = today (письмо всё равно придёт завтра).

**SMAPI-лог**

```text
⚠️ [Шахта] Повторный вход с Severe после предупреждения — принудительный выход
```

При третьей и дальнейших попытках в тот же день:

```text
⚠️ [Шахта] Повторная попытка входа с Severe после принудительного выхода
```

HUD короче: «Сегодня шахта закончена.»

---

### B. Следующий день после предупреждения

**Подготовка**

- Завершён сценарий **A** (или A2): вчера был `MineWarningDay`.
- Лечь спать.

**Действия**

1. Проснуться утром.
2. Проверить почту и баффы.

**Ожидаемый результат**

- В почте: **`mailHarveyMineForbidden`** (если `SendLetters = true`).
- Активен дебафф **`HarveyMod_MineForbidden`**.
- `MineForbiddenAppliedDay = today`.
- `MineWarningDay` сброшен в **-1**.

**SMAPI-лог**

```text
[Шахта] Наложен дебафф «Харви запретил шахту» на 2 дн. (день …)
```

---

### C. Первый вход в шахту под HarveyMod_MineForbidden

**Подготовка**

- Активен `HarveyMod_MineForbidden` (сценарий **B** или несколько дней назад).
- `LastMineForbiddenInterceptionDay != today`.
- HarveyOverhaul [CP] загружен.

**Действия**

1. `warp Mine 17 7` или войти через лифт.

**Ожидаемый результат**

- Запускается **`eventHarveyMineInterception`** (катсцена у входа в шахту).
- После события игрок на **Mountain** (53, 8), если всё ещё был в Mine/MineShaft.
- `LastMineForbiddenInterceptionDay = today`.
- Топик **`HarveyMineIntercept`** на 3 дня (из CP-события).

**SMAPI-лог**

```text
[MineForbidden] Игрок вошёл в шахту при активном запрете Харви
[MineForbidden] Запущено событие 'eventHarveyMineInterception'
[MineForbidden] Событие завершено, игрок всё ещё в подземелье — warp наружу (CP без changeLocation)
```

Если событие не стартовало (нет CP / ошибка):

```text
[MineForbidden] Событие не запустилось — fallback HUD + warp
```

---

### D. Повторный вход в шахту в тот же день (под MineForbidden)

**Подготовка**

- Сценарий **C** уже выполнен **сегодня**.
- `HarveyMod_MineForbidden` всё ещё активен.

**Действия**

1. Снова войти в шахту.

**Ожидаемый результат**

- **Нет** повторной катсцены.
- HUD с остатком дней, например:
  - «Харви запретил шахту до окончания лечения. Осталось: 2 дн.»
  - или «… Остался 1 день.»
- Звук `cancel`.
- Warp на **Mountain** (53, 8).

**SMAPI-лог**

```text
[MineForbidden] Игрок вошёл в шахту при активном запрете Харви
```

(Без строки «Запущено событие …».)

---

### E. Истечение запрета

**Подготовка**

- `HarveyMod_MineForbidden` был наложен `MineForbiddenDurationDays` дней назад (по умолчанию **2**).
- Пример: наложен в день 10 → снимается с начала дня **12** (`today >= appliedDay + durationDays`).

**Действия**

1. Проснуться в день истечения.
2. Войти в шахту без новой Severe-травмы (или с лёгкой травмой).

**Ожидаемый результат**

- Дебафф **`HarveyMod_MineForbidden` снят**.
- `MineForbiddenAppliedDay = -1`.
- Вход в шахту снова **разрешён** (если нет нового Severe / нового цикла предупреждения).

**SMAPI-лог**

```text
[Шахта] Снят дебафф «Харви запретил шахту» (истёк срок: 2 дн.)
```

---

### F. Вулкан под HarveyMod_MineForbidden

**Подготовка**

- Активен `HarveyMod_MineForbidden`.
- Доступ к Ginger Island.

**Действия**

1. `warp VolcanoDungeon 1 1` (или войти через вулкан на острове).

**Ожидаемый результат**

- **Нет** `eventHarveyMineInterception` (это шахтная сцена).
- HUD про **опасные подземелья** с остатком дней.
- Звук **`debuffHit`**.
- Warp на **IslandNorth** (31, 17) — тайл у входа в вулкан; при неточной позиции проверить визуально на тестовом сейве.

**SMAPI-лог**

```text
[MineForbidden] Игрок вошёл в вулкан при активном запрете Харви
```

---

## Чек-лист быстрой регрессии

| # | Сценарий | HUD / событие | Warp | State |
|---|----------|---------------|------|-------|
| 1 | A — первый Severe | Предупреждение | Нет | `MineWarningDay = today` |
| 2 | A2 — второй Severe | «Уже предупреждал…» | Mountain | `LastMineSevereForcedExitDay = today` |
| 3 | B — утро | Письмо + дебафф | — | `MineForbiddenAppliedDay = today` |
| 4 | C — первый MineForbidden | `eventHarveyMineInterception` | Mountain | `LastMineForbiddenInterceptionDay = today` |
| 5 | D — повтор в тот же день | HUD + остаток дней | Mountain | без новой катсцены |
| 6 | E — истечение | — | — | дебафф снят |
| 7 | F — вулкан | HUD подземелья | IslandNorth | без шахтного события |

---

## Типичные ложные срабатывания

| Симптом | Возможная причина |
|---------|-------------------|
| Предупреждение не показалось | `NeedsMineRescueEvent = true` (ожидается rescue утром) |
| Письмо не пришло | `SendLetters = false` или не было `MineWarningDay` вчера |
| Катсцена не запустилась | CP не загружен / нет `Data/Events/Mine` / ошибка в логе `[MineForbidden]` |
| Двойная катсцена | Проверить, не срабатывает ли параллельно CP `triggerHarveyMineWarning` (SpaceCore) при Dating + injury buff **без** `MineForbidden` |
| После reload снова катсцена в тот же день | `LastMineForbiddenInterceptionDay` не сохранился → `injury_debug_dump` |

---

## Связанные файлы кода

| Файл | Роль |
|------|------|
| `EventHandlers/PlayerEventHandler.cs` | Предупреждение Severe, forced exit, `MineForbidden`, warp |
| `EventHandlers/GameEventHandler.cs` | Письмо в `DayEnding`, наложение/снятие дебаффа в `DayStarted` |
| `Core/Models/InjuryState.cs` | Поля save-state |
| `Core/ModConfig.cs` | `MineForbiddenDurationDays`, `SendLetters` |
| `HarveyOverhaul [CP]/assets/Code/eventsCare.json` | `eventHarveyMineInterception` |
