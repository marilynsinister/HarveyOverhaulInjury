# Форматирование CP-событий (HarveyOverhaul)

Стиль оформления event script в Content Patcher-файлах мода **HarveyOverhaul [CP]**.

**Источник CP:** `HarveyOverhaul [CP]/assets/Code/`  
**Подключённые файлы:** `events.json`, `eventsCare.json`, `eventsMineRescue.json` (см. `content.json`)

**Связанные документы:** [cp-events-format-inventory](events-format/cp-events-format-inventory.md) · [04-fork-subevents](events-inventory/04-fork-subevents.md)

---

## Основные правила

### Event script — строка, не массив

Значение события в `Entries` — **одна JSON-строка** с literal переносами между командами. Не превращать script в JSON-массив команд и не использовать `\\n` для разделения команд.

```json
"eventHarveyFirstVisit/Time 600 1200/...": "
    continue/
    64 15/
    farmer 64 16 2 Harvey 64 18 0/
    speak Harvey \"...\"/
    end
"
```

### Команды — по одной на строку

Каждая команда event script (до `end`, `end dialogue`, `end position` и т.д.) — отдельная физическая строка внутри multiline string.

**Перенос строки ставится после `/`**, на границе команд. Не разрывать команду посередине.

### Ключ события и preconditions — одна строка

JSON-ключ остаётся **целиком на одной строке**: ID события и все preconditions (`/Time`, `/Weather`, `/GameStateQuery`, `/Friendship` и т.д.) не переносятся.

```json
"eventHarveyFirstWalk/Time 600 1200/Weather Sunny/GameStateQuery DAYS_PLAYED 11/...": "
    continue/
    ...
"
```

Допустимо, что **первая команда script** (`continue/`, `none/`, `Hospital_Ambient/` и т.п.) стоит на той же строке, что и открывающая кавычка значения; coords и остальные команды — ниже.

### speak / message — одна команда

Реплики оформляются как **одна строка на команду**. Текст диалога не переносится на новую физическую строку внутри кавычек.

Игровые переносы внутри реплики (`#$b#`, `\n\n` в SDV-тексте) остаются **внутри одной команды** — это не разделение команд event script.

### Сложные команды — не дробить внутри

Если команда имеет внутреннюю структуру, её **оставляют целиком на одной строке** (можно вынести как отдельную строку script, но не разрывать внутри):

| Конструкция | Правило |
|---|---|
| `quickQuestion ...#opt1#opt2(break)ветка1\\speak...(break)ветка2/` | Вся команда — одна строка |
| `question forkN "..."` | Одна строка |
| `fork subEventId/` | Отдельная строка (простая команда) |
| Ветки с `(break)` внутри QQ | Не дробить |

**Подход:**

- команды **до** `quickQuestion` — построчно;
- сам `quickQuestion` — **одна строка**;
- команды **после** `quickQuestion` — построчно.

То же для цепочек `question fork` + `fork`: ветки-подсобытия (`declineFood`, `acceptWalk` и т.д.) форматируются построчно, monolithic QQ внутри них — нет.

### Что не менять при форматировании

- тексты реплик и варианты ответов;
- friendship, action, conversation topics;
- event IDs и preconditions;
- порядок команд;
- экранирование `\"` в диалогах.

### Как форматировать (и как не форматировать)

- **Вручную**, с семантическим diff по смыслу команд.
- **Не использовать** Python-скрипты, jq, sed/awk, regex-массовые замены и автоформаттеры JSON для такого reformat.
- **Не добавлять** JSONC-комментарии (`//`, `/* */`) — в CP-файлах они не являются частью целевого стиля (legacy-комментарии в репозитории не трогать и не множить).

### Валидация

Обычный JSON-валидатор может **ругаться** на literal переносы внутри строк event script — для Content Patcher это **ожидаемо и допустимо**.

Проверять нужно:

1. **Совместимость с Content Patcher** — event string, `/` между командами, fork/QQ синтаксис SDV.
2. **Семантический diff** — те же команды, тексты, preconditions, порядок; изменилась только раскладка по строкам.

Строгий JSON RFC — **не единственный** критерий корректности.

---

## Пример: до / после

### До (склеенные команды)

```json
"eventHarveyStormComfortFarm/GameStateQuery PLAYER_HAS_BUFF Current buffStressThunder/Weather storm/Time 2000 2600/Friendship Harvey 750/Random 0.6": "none/64 15/
        farmer 64 16 2 Harvey 64 18 0/
        skippable/
        speak Harvey \"Я почувствовал, что тебе страшно.$s\"/
        quickQuestion #Согласиться#Отказаться(break)speak Harvey \"Хорошо.$0\"\\friendship Harvey 50(break)speak Harvey \"Понимаю.$s\"/
        pause 1000/
        end"
```

### После (целевой стиль)

```json
"eventHarveyStormComfortFarm/GameStateQuery PLAYER_HAS_BUFF Current buffStressThunder/Weather storm/Time 2000 2600/Friendship Harvey 750/Random 0.6": "
        none/
        64 15/
        farmer 64 16 2 Harvey 64 18 0/
        skippable/
        speak Harvey \"Я почувствовал, что тебе страшно.$s\"/
        quickQuestion #Согласиться#Отказаться(break)speak Harvey \"Хорошо.$0\"\\friendship Harvey 50(break)speak Harvey \"Понимаю.$s\"/
        pause 1000/
        end"
```

Изменилось только разбиение простых команд (`none/`, coords). Строка `quickQuestion` с `(break)` и `\\speak` **осталась одной строкой**.

---

## Краткий чеклист перед коммитом

- [ ] Script — multiline **string**, не array
- [ ] Простые команды — по одной на строку, перенос после `/`
- [ ] Ключ + preconditions — одна JSON-строка, без изменений
- [ ] `quickQuestion` / fork / `(break)` — не разорваны внутри
- [ ] Тексты, friendship, topics, IDs — без изменений
- [ ] Без новых JSONC-комментариев
- [ ] Без `\\n` вместо literal newlines между командами
- [ ] Проверка: CP-совместимость + semantic diff, не только strict JSON
