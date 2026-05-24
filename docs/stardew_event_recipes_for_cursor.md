# Stardew Valley — рецепты для написания событий Content Patcher

Источник: `Stardew Valley - Event Modding Resource.xlsx`.

Документ подготовлен для Cursor: его можно положить в проект рядом с `content.json` / `assets/Code/events*.json` и использовать как рабочую инструкцию при генерации, аудите и правке событий.

> Важно: этот `.md` не является заменой тестирования в игре. Таблица содержит справочные ID, ванильные примеры и подсказки, но каждое событие всё равно нужно проверять через SMAPI log и запуск в нужной локации.

---

## 0. Что есть в XLSX

| Лист | Содержание | Где в этом `.md` |
| --- | --- | --- |
| Sound Bank IDs | 284 звуковых ID (имя + описание + где используется) для `playSound`. | §10 |
| Music Bank IDs | 138 музыкальных/ambient ID для первого токена события или `playMusic`. | §11 |
| Item IDs | 587 ID предметов для `addObject`, `addItem`, `removeItem`, `itemAboveHead`, подарков и реквизита. | §12 |
| Event IDs | 687 примеров Event ID из ванили/модов, полезно для избежания конфликтов. | §14 |
| Farmer Frame IDs | 119 кадров фермера для `showFrame farmer ...`; визуальные кадры + предупреждения. | §8 |
| Emote IDs | 16 фиксированных ID (кратные 4); семантика и статистика — в этом документе. | §7 |
| NPC Portrait IDs | Визуальная таблица портретов NPC. Правила `$h/$s/$u/$l/$a/$0..$9` — в этом документе. | §5 |
| NPC Sprite IDs | Визуальная таблица NPC-кадров; для `showFrame Harvey ...`, `animate Harvey ...`. | §9 |
| Symbols | 6 спецсимволов для `textAboveHead`. | §13 |
| Raw dump | 222 разобранных ванильных примера событий — донор паттернов: эмоций, анимаций, специфичных команд. | §4, §7.2, §9.2, §25, §26 |

> Прим. Лист Emote IDs / NPC Sprite IDs / NPC Portrait IDs состоит **только** из картинок-спрайтов без текстовых описаний — все семантические значения в этом `.md` собраны из исходного кода Stardew Valley (`Character.cs`, `Event.cs`) и из частоты использования в Raw dump.

---

## 1. Базовый рецепт CP-события

### 1.1. Минимальная структура `EditData`

```json
{
  "Action": "EditData",
  "Target": "Data/Events/Hospital",
  "Entries": {
    "eventHarveyExample/Time 900 1700/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyExample": "event script here"
  }
}
```

### 1.2. Правила для Cursor

При создании или правке событий Cursor обязан:

1. Не использовать JSONC-комментарии внутри `content.json`.
2. Не менять существующие ID событий, топиков и писем без отдельной задачи.
3. Не удалять guards:
   - `!PLAYER_HAS_SEEN_EVENT`;
   - `PLAYER_HAS_SEEN_EVENT`;
   - `PLAYER_HAS_CONVERSATION_TOPIC`;
   - `!PLAYER_HAS_CONVERSATION_TOPIC`;
   - `Time`;
   - `Weather`;
   - `Friendship` / `PLAYER_HEARTS`.
4. Не смешивать несколько смысловых сцен в один огромный event entry.
5. В конце события всегда явно завершать сценарий:
   - `end`;
   - или `end dialogue Harvey "..."`, если нужен follow-up диалог.
6. Для событий InjuryCare не дублировать C#-логику баффов. CP-событие должно показывать сцену, а баффы/фазы/состояние должны оставаться в C#.

---

## 2. Формула события

Любое событие лучше собирать по такой схеме:

```txt
music_or_ambient/
start positions/
skippable/
viewport/
pause/
первый визуальный акцент/
основной диалог/
движение или жест/
выбор игрока, если нужен/
последствия выбора/
topic/mail/friendship/
end
```

Пример каркаса:

```txt
kindadumbautumn/
-1000 -1000/
farmer 20 12 2 Harvey 22 12 3/
skippable/
viewport 21 12 true/
pause 500/
speak Harvey "Садись, пожалуйста. Я хочу проверить повязку.$u"/
move farmer 0 1 0/
faceDirection farmer 0/
showFrame farmer true 117/
pause 500/
speak Harvey "Вот так. Спокойно. Я рядом.$l"/
friendship Harvey 20/
addConversationTopic topicHarveyAfterBandageCheck 3/
end
```

---

## 3. Preconditions: как выбирать условия

### 3.1. Частые условия

```txt
Time 900 1700
Weather Sunny
Weather Rain
Weather Storm
DayOfWeek Mon Tue Wed Thu Fri
Friendship Harvey 1000
GameStateQuery PLAYER_HEARTS Current Harvey 4
GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventId
GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventId
GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicId
GameStateQuery !PLAYER_HAS_CONVERSATION_TOPIC Current topicId
```

### 3.2. Рецепты условий

#### Одноразовая романтическая сцена

```txt
eventHarveyCare1/Time 900 1700/Friendship Harvey 1000/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyCare1
```

#### Сцена после C#-topic

```txt
eventHarveyAfterMineRescue/Time 900 1700/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicMineInjuryRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyAfterMineRescue
```

#### Сцена продолжения цепочки

```txt
eventHarveyStory2/Time 900 1700/GameStateQuery PLAYER_HAS_SEEN_EVENT Current eventHarveyStory1/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyStory2
```

#### Сцена на погоду

```txt
eventHarveyStormComfort/Weather Storm/Time 1400 2200/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyStormComfort
```

---

## 4. Команды событий: практическая шпаргалка

Ниже — самые частые команды из `Raw dump`, отсортированные по встречаемости.

| Команда | Сколько раз в raw dump | Пример из vanilla dump |
| --- | --- | --- |
| pause | 3316 | pause 300 |
| faceDirection | 1361 | faceDirection farmer 1 |
| speak | 1251 | speak Marnie \"Oh! You brought the cave carrot! Thank you so much, Mr. @.$h^Oh! You brought the cave carrot! Thank you so much, Ms. @.$h\" |
| move | 908 | move farmer 0 -1 0 |
| playSound | 562 | playSound woodyStep |
| positionOffset | 546 | positionOffset Jas -2 1 |
| showFrame | 542 | showFrame 35 |
| emote | 332 | emote Marnie 16 |
| animate | 261 | animate Shane false true 500 32 33 |
| viewport | 258 | viewport 22 8 true |
| warp | 219 | warp Shane 25 7 |
| jump | 218 | jump Shane |
| stopAnimation | 190 | stopAnimation Shane |
| end | 175 | end dialogue Marnie \"Thanks again for helping me out.$h#$e#...and my goats say 'thanks' too.\" |
| playMusic | 143 | playMusic none |
| farmer | 127 | farmer 13 19 0 Marnie 13 14 2 |
| specificTemporarySprite | 122 | specificTemporarySprite shanePassedOut |
| message | 104 | message \"Could she have dropped it nearby?\" |
| globalFade | 102 | globalFade |
| shake | 101 | shake Shane 2000 |
| skippable | 97 | skippable |
| speed | 95 | speed Marnie 4 |
| textAboveHead | 93 | textAboveHead Marnie \"Oh dear...\" |
| fork | 56 | fork 958699 mysteryBook |
| -1000 | 50 | -1000 -1000 |
| question | 43 | question fork1 \"#I believe in you!#Wow, you're really arrogant.\" |
| updateMinigame | 35 | updateMinigame 1 |
| null | 30 | null |
| switchEvent | 27 | switchEvent elliottPianoJoin |
| glow | 26 | glow 0 0 255 true |
| doAction | 24 | doAction 21 13 |
| advancedMove | 22 | advancedMove chicken2 false 0 -4 -2 0 |
| fade | 22 | fade |
| continue | 18 | continue |
| 64 | 17 | 64 15 |

### 4.1. Команды, которые чаще всего нужны для Харви

```txt
speak Harvey "Текст.$h"
message "Нарративный текст без говорящего."
emote Harvey 20
textAboveHead Harvey "..."
faceDirection Harvey 2
move Harvey 0 1 1
advancedMove Harvey false 0 -1 1 0
showFrame Harvey 16
animate Harvey false true 250 16 17 18 19
stopAnimation Harvey
playSound doorClose
playMusic kindadumbautumn
pause 500
friendship Harvey 20
addConversationTopic topicId 3
mail mailId
end
```

---

## 5. Диалоги и портреты

### 5.1. Портреты в тексте (короткие алиасы)

В таблице `NPC Portrait IDs` указано: если закончить реплику `$<число>` или `$<буква>`, будет показан соответствующий портрет NPC.

| Код | Английское имя | Семантика | Когда уместно для Харви |
| --- | --- | --- | --- |
| `$neutral` / `$0` | neutral | Спокойный взгляд по умолчанию | Нарратив, спокойная инструкция врача |
| `$h` / `$happy` | happy | Тёплая улыбка | После успешного осмотра/выздоровления |
| `$s` / `$sad` | sad | Грусть, лёгкая тревога | Видит ухудшение, переживает за фермершу |
| `$u` / `$unique` | unique | Сосредоточенный, "врачебный" взгляд | Осмотр, постановка диагноза, серьёзный тон |
| `$l` / `$love` | love | Влюблённый/смущённый | Романтические сцены, признания, забота |
| `$a` / `$angry` | angry | Раздражение, строгость | Запрет шахт, выговор за безрассудство |
| `$1` … `$9` | numeric | Конкретный кадр портрета по индексу | Когда нужен точный нестандартный портрет |

> Прим. Stardew 1.6 поддерживает длинные имена (`$neutral`, `$happy`, `$sad`, `$angry`, `$love`, `$unique`). Если в файле уже используются короткие — придерживаться короткой формы для единообразия.

Пример:

```txt
speak Harvey "Не спорь со мной из вежливости. Ты еле стоишь.$a"
speak Harvey "Я рядом. Дыши ровно.$l"
speak Harvey "Хорошо... уже лучше.$h"
speak Harvey "Пульс ровный. Хорошо.$0"
```

### 5.2. Управляющие коды внутри реплик

| Код | Что делает | Пример |
| --- | --- | --- |
| `#$b#` | Новый dialog box (новая страница) в той же реплике | `"Сядь.$s#$b#Дыши ровно.$u"` |
| `$e` | Принудительно завершить ветку диалога | `"Хватит, $e"` |
| `$k` | Завершить диалог с короткой паузой (для "выхода") | `"Я пойду.$k"` |
| `$b` (один) | Иногда используется как break, чаще именно `#$b#` | — |
| `^` | Гендерный вариант: текст до `^` для мужского фермера, после — для женской | `"good boy^good girl"` |
| `@` | Имя игрока | `"Спасибо, @."` |
| `%farm` | Название фермы | `"Прогуляйся по %farm."` |
| `%time` | Текущее игровое время | `"Уже %time."` |
| `%adj` | Случайное прилагательное | — |
| `%noun` | Случайное существительное | — |
| `%pet` | Имя питомца | — |

### 5.3. Расширенные конструкции `$q / $r / $y / $c / $d / $v / $p`

Эти коды работают преимущественно в `Data/Characters/Dialogue/<NPC>.json` и аналогичных таблицах, но часть из них появляется и внутри `speak` в событиях.

| Код | Синтаксис | Назначение |
| --- | --- | --- |
| `$q <answer_key> <fallback_key>` | `"$q 123 fallback#Вопрос?#Ответ A#Ответ B"` | Вопрос с вариантами ответа. `answer_key` — куда писать выбор, `fallback_key` — ветка по умолчанию |
| `$r <answer_key> <friendship_change> <response_key>` | `"$r 123 25 yesAnswer#Согласиться"` | Один из ответов на `$q` (+25 дружбы при выборе) |
| `$y "вопрос_ответ1_следствие1_ответ2_следствие2"` | `"$y 'Хочешь чай?_Да_Спасибо_Нет_Ладно'"` | Быстрый "in-line" вопрос с короткими ответами (без отдельной записи в Dialogue) |
| `$c <вероятность> "текст_A" "текст_B"` | `"$c .5 'Привет!' 'Здравствуй.'"` | Случайный выбор реплики (вероятность от 0 до 1) |
| `$d <day_key> "текст_если_да" "текст_если_нет"` | `"$d monday 'Понедельник…' 'Не понедельник.'"` | Ветка по дню недели или флагу |
| `$v <eventID>` | `"$v 158022"` | Запустить указанное событие прямо из диалога |
| `$p <eventID> "текст_если_видел" "текст_если_нет"` | `"$p 4 'Помнишь?' 'Странно…'"` | Ветка по факту просмотра события |
| `$t <topicID>` | `"$t topicHarveyLate"` | Ветка по активной conversation topic |
| `$1`…`$9` | Без аргументов | Принудительный номер портрета (как в 5.1) |
| `*` | В начале строки | "Этой репликой можно перезаписать стандартную" (для Dialogue, не для событий) |

> Внутри `speak` в событиях самые полезные — `$q + fork`, см. раздел 6. Остальные коды чаще нужны в Dialogue-файлах персонажей.

### 5.4. Управляющие коды preconditions (для cтроки `Time/Weather/...`)

В коротком формате preconditions есть «свои» однобуквенные коды (как раз их видно в Raw dump):

| Код | Аналог в long form | Назначение |
| --- | --- | --- |
| `f <NPC> <points>` | `Friendship` | Минимум очков дружбы (250 = 1 сердце) |
| `t <start> <end>` | `Time` | Окно времени |
| `w sunny/rainy/snowy` | `Weather` | Погода |
| `z <season>` | `Season` | Сезон (spring/summer/fall/winter) |
| `u <day>` | `DayOfMonth` | Конкретный день месяца |
| `d Mon Tue …` | `DayOfWeek` | Дни недели |
| `n <event_or_letter>` | `MailReceived` | Игрок получил письмо или флаг |
| `e <event_id>` | `PLAYER_HAS_SEEN_EVENT` | Игрок уже видел событие |
| `!e <event_id>` | `!PLAYER_HAS_SEEN_EVENT` | Игрок ещё не видел событие |
| `p <NPC>` | `PLAYER_HAS_DATING_NPC` / в браке | NPC в группе романа |
| `H` | — | Игрок женат на ком-то |
| `y <year>` | `Year` | Минимальный год |

Эти короткие коды работают только в строке-ключе entry. В современных модах лучше использовать длинные `GameStateQuery ...`/`Friendship ...`/`Time ...` — они читаемее.

---

## 6. Выборы игрока

Для героини, которая в основном молчит, выборы лучше использовать как эмоциональные реакции, а не длинные реплики.

### 6.1. Рецепт `question + fork`

```txt
speak Harvey "Ты позволишь мне осмотреть рану?$s"/
question allowCheck "Кивнуть#Отвести взгляд#Сказать, что страшно"/
fork allowCheck_yes/
speak Harvey "Спасибо. Я буду осторожен.$l"/
friendship Harvey 20/
end
```

### 6.2. Рецепт молчаливых ответов

Варианты выбора:

```txt
Кивнуть
Молча протянуть руку
Отвести взгляд
Сжать край рукава
Сделать шаг ближе
Попросить не уходить
```

Для Харви это хорошо работает так:

```txt
speak Harvey "Не обязательно говорить вслух. Просто кивни, если больно.$s"
```

---

## 7. Эмоции (`emote`)

Из листа `Emote IDs`:

- ID эмоций идут кратно 4 — нельзя вызвать отдельный кадр.
- Обычно `emote` проигрывается полностью, и только после этого скрипт идёт дальше.
- Чтобы сделать эмоцию **асинхронной** (скрипт продолжается параллельно), добавить `true` четвёртым словом: `emote Harvey 12 true`.
- Эмоция «прилетает» над головой персонажа и автоматически исчезает.

### 7.1. Семантика всех 16 ID

Названия — из исходного `Character.cs` Stardew Valley. ID-кадр идёт парой: рисунок-эмоция всегда занимает 4 кадра спрайтшита эмоций.

| ID | Имя в коде | Картинка | Семантика | Когда уместно для Харви/фермерши |
| --- | --- | --- | --- | --- |
| 0 | `?` | вопросительный знак | Недоумение, «что происходит?» | Харви заметил что-то странное в осмотре; фермерша не понимает указание |
| 4 | `empty` | пустой фрейм | Сбрасывает текущую эмоцию, можно вставить в цепочку как «no-op» | Очистка перед следующей реакцией |
| 8 | `blush` | розовые щёки | Смущение, лёгкая нежность | Фермерша после ласкового жеста Харви; Харви, когда невольно сказал лишнее |
| 12 | `angry` | красная сжатая надбровь / стрелы | Раздражение, строгость | Харви запрещает шахты; фермерша огрызается на гиперопеку |
| 16 | `!` | восклицательный знак | Удивление, тревожный «ой» | Харви видит кровь/ухудшение; фермерша слышит резкий звук |
| 20 | `heart` | сердце | Любовь, благодарность | Романтические моменты, признания |
| 24 | `sleep` | «Z» | Сон / засыпание / усталость | Фермерша засыпает в больничной палате; Харви устал после смены |
| 28 | `sad` | слеза | Грусть, плач, эмоциональный спад | Тяжёлая сцена после травмы; фермерша плачет от усталости |
| 32 | `happy` | улыбка | Радость, облегчение | Хороший прогноз, удачное выздоровление |
| 36 | `x` | крестик / «нет» | Отказ, недоступно | Реакция фермерши на запрет; «нельзя» |
| 40 | `pause` | многоточие `...` | Задумчивость, неловкая пауза | Харви взвешивает решение; фермерша не знает, что сказать |
| 44 | `videogame` | контроллер | Геймерская / шутка | Скорее для Sebastian/Sam — у Харви почти не пригодится |
| 48 | `music` | нота | Музыка, лёгкое настроение | Сцена с радио, музыкальный момент в больнице |
| 52 | `jewel` | алмаз | Подарок / ценность | Сцена с дорогим подарком, особым предметом |
| 56 | `taunt` | злобный смайл | Дразнящее, насмешливое | Редко — скорее для негативных персонажей |
| 60 | `uneasy` | сине-серое лицо | Тревога, дурное предчувствие, страх | Сильный страх фермерши перед инъекцией; Харви видит критическое состояние |

### 7.2. Частота использования в vanilla (Raw dump)

Не все эмоции используются одинаково часто. Распределение из 332 эмоций в ванильных событиях:

| ID | Имя | В ванили | Заметки |
| --- | --- | --- | --- |
| 28 | sad | 78 | Самая частая — драматические сцены |
| 16 | exclamation | 59 | Удивление/тревога |
| 40 | pause | 50 | Многоточие как пауза в драме |
| 8 | blush | 49 | Очень частая в романтических ивентах |
| 12 | angry | 43 | Раздражение |
| 32 | happy | 33 | Лёгкое настроение |
| 20 | heart | 11 | Реже — приберегается для сильных моментов |
| 56 | taunt | 3 | Почти не используется |
| 24 | sleep | 2 | Только специальные сцены |
| 60 | uneasy | 2 | Редкий, но яркий акцент |
| 52 / 48 | jewel / music | по 1 | Почти не используется |
| 0 / 4 / 36 / 44 | question / empty / x / videogame | 0 в этом срезе | Применять осторожно — нет vanilla-паттернов |

> Для фермерши (`emote farmer N`) в vanilla чаще всего: `8` (21), `28` (20), `16` (15), `32` (14), `40` (10), `56` (3), `12` (2), `60`/`48` (по 1). Это хороший «бюджет» для молчаливой героини.

### 7.3. Базовый синтаксис

```txt
emote <actor> <id>          # синхронно, скрипт ждёт окончания
emote <actor> <id> true     # асинхронно, скрипт идёт дальше
emote farmer <id>           # эмоция над головой фермера
```

### 7.4. Рецепты

#### Одновременная парная реакция

```txt
emote Harvey 16 true/
emote farmer 60/
pause 700/
speak Harvey "Не двигайся. Я рядом.$s"
```

#### Сброс эмоции

```txt
emote Harvey 20/
pause 400/
emote Harvey 4
```

Полезно перед сменой настроения сцены, чтобы зритель не «застрял» на предыдущем символе.

#### Романтический микро-момент

```txt
speak Harvey "Ты слишком много на себя берёшь.$s"/
pause 300/
emote farmer 8/
pause 600/
speak Harvey "...но я знал это, когда мы начали встречаться.$l"
```

#### Реакция на запрет

```txt
speak Harvey "Сегодня — никаких шахт.$a"/
pause 300/
emote farmer 36/
pause 500/
speak Harvey "Знаю. Но альтернатив нет.$s"
```

#### Страх перед процедурой

```txt
emote farmer 60/
pause 400/
speak Harvey "Дыши. Я объясню каждый шаг до того, как сделаю.$u"
```

---

## 8. Farmer Frame IDs

### 8.1. Главное правило

В листе `Farmer Frame IDs` указано важное предупреждение: вызов кадра фермера меняет **только части спрайта**, а не весь спрайт целиком. Поэтому **перед** `showFrame farmer ...` нужно правильно поставить направление через `faceDirection`, иначе голова/волосы окажутся «задом наперёд», а руки — не в том месте.

```txt
faceDirection farmer 0/
showFrame farmer 117
```

`showFrame farmer N` и `showFrame N` — это **разные** команды:

- `showFrame farmer N` — учитывает текущее направление фермера и поворачивает кадр.
- `showFrame N` — рисует кадр «как есть» без поворота.

Если кадр выглядит криво, попробовать вторую форму, потом снова первую.

### 8.2. Карта диапазонов кадров фермера

Источник — `Game1.player` спрайтшит. Кадры идут блоками по направлению/состоянию. Это **ориентир**: модели причёсок/одежды могут сдвигать оттенки, всегда проверять визуально.

| Frame ID | Направление | Состояние | Использовать для |
| --- | --- | --- | --- |
| 0 | вниз | стояние (idle) | Базовая поза анфас (самая частая в ванили) |
| 1–3 | вниз | шаги вперёд (walk cycle) | Анимация движения, можно через `animate farmer false true 250 1 2 3` |
| 4 | вниз | альтернативное стояние (рука у груди) | «Скованная» поза, тревога |
| 5–7 | вниз | дополнительные walk-кадры | Бег / комбинированная анимация |
| 8 | право | стояние | Базовая поза вправо |
| 9–11 | право | walk cycle | Шаги вправо |
| 12–15 | право | alt-кадры (рука вперёд / поднята) | Жест приветствия |
| 16 | вверх | стояние | Базовая поза спиной |
| 17–19 | вверх | walk cycle | Уход вверх по сцене |
| 20–23 | вверх | alt-кадры | Жест-указание вверх |
| 24 | влево | стояние | Базовая поза влево |
| 25–27 | влево | walk cycle | Шаги влево |
| 28–31 | влево | alt-кадры | Жест влево |
| 32–37 | разные | tool actions (карманные предметы) | Сцены с инструментами (редко в романе) |
| 38–41 | разные | tool swing | **Кривые** в ванили — не использовать (см. 8.4) |
| 42–47 | разные | броски, fishing | **Кривые** в ванили — не использовать |
| 48–60 | разные | взаимодействие | Используется в кат-сценах, требует теста |
| 61–80 | разные | специальные жесты | Тесты обязательны |
| 90 | — | `itemAboveHead` (предмет над головой) | Сцена «нашёл предмет» — обычно ставится через `itemAboveHead`, а не вручную |
| 94 | — | passed out / без сознания | Сцена обморока, кризис, медицинская критическая ситуация |
| 101 | — | sleeping | Спит в кровати (используется в `Stardrop`/Shane 6h ивенте) |
| 102–105 | — | разные «лежащие» кадры | Лечение, отдых на кушетке |
| 117 | вниз | sitting (сидит) | Главный кадр сидения; парно с `faceDirection farmer 0` |
| 118–122 | — | пусто | Не использовать (нет спрайтов) |

> **Совет:** для большинства Harvey-сцен достаточно `0`, `8`, `16`, `24` (idle в 4-х направлениях), `94` (без сознания), `101` (спит), `117` (сидит) и пары `4`/`7` (взволнованная анфас-поза).

### 8.3. Самые ходовые кадры в vanilla

Из 542 вызовов `showFrame` в Raw dump для `farmer` самые частые:

| Frame | Сколько раз | Назначение |
| --- | --- | --- |
| 0 | 12 | Idle лицом к камере |
| 94 | 5 | Обморок / падение |
| 4, 6, 7 | по 2 | Тревожные анфас-кадры |
| 90 | 1 | itemAboveHead (предмет в руках) |

Это значит: даже сложные ванильные сцены опираются на горсть кадров — не нужно изобретать новые без причины.

### 8.4. Сидение фермера (рецепт)

Для сцен «Харви посадил фермершу» использовать:

```txt
faceDirection farmer 0/
showFrame farmer true 117
```

Безопасный рецепт:

```txt
warp farmer 20 12/
faceDirection farmer 0/
pause 200/
showFrame farmer true 117/
pause 500/
speak Harvey "Вот так. Сидишь спокойно, пожалуйста.$u"
```

### 8.5. Обморок / лежащая фермерша

```txt
faceDirection farmer 0/
showFrame farmer 94/
specificTemporarySprite shanePassedOut/  # опционально — добавит дополнительный визуал
pause 800/
emote Harvey 16/
speak Harvey "@! @!$a#$b#Очнись, я здесь.$s"
```

> `shanePassedOut` — это готовый visual effect из vanilla (Shane 7h ивент). Хорошо подходит для медицинских кризисов.

### 8.6. Кадры с предупреждениями

Эти кадры из таблицы помечены как проблемные или подозрительные. Полные комментарии автора xlsx-файла:

| Frame ID | Предупреждение |
| --- | --- |
| 10 | Actually missing, from what I can tell. |
| 38 | Note the arms. It looks even worse from all other angles. |
| 39 | Note the arms. It looks even worse from all other angles. |
| 40 | Note the arms. It looks even worse from all other angles. |
| 41 | Note the arms. It looks even worse from all other angles. |
| 43 | Yes, the arms are missing. I don't know why; must be part of another frame. |
| 45 | Clearly broken, possibly part of a multiframe. |
| 46 | Clearly broken, possibly part of a multiframe. |
| 47 | Clearly broken, possibly part of a multiframe. |
| 66 | Possibly should be facing down instead? |
| 75 | Possibly should be facing down instead? |
| 109 | Using this produces an odd looping sound for some reason. |
| 118 | 118 to 122 were all blank so I am assuming there are no more sprites. |

Правило для Cursor: не использовать эти кадры в новых событиях без отдельного теста в игре.

### 8.7. Анимация фермера через `animate`

`animate <actor> <flip> <loop> <interval_ms> <frame1> <frame2> ...`

- `flip` — `true/false`, зеркалить ли;
- `loop` — `true/false`, зациклить ли (для односекундной анимации обычно `false`);
- `interval_ms` — задержка между кадрами;
- дальше — список frame IDs.

Самые частые анимации фермера в vanilla:

```txt
animate farmer false true 250 94            # «затягивающийся» обморок
animate farmer false true 250 6             # лёгкое покачивание
animate farmer false true 250 7
animate farmer false true 250 101           # сон с дыханием
animate farmer false true 250 102 103       # шевелится во сне
animate farmer false true 250 104 105       # «беспокойный сон»
animate farmer false true 250 35            # tool swing
animate farmer false true 250 15            # жест вправо
animate farmer false true 250 30            # жест влево
```

Для остановки — `stopAnimation farmer`. После `stopAnimation` лучше явно поставить `faceDirection`, иначе спрайт может «застыть» на полу-кадре.

---

## 9. NPC Sprite IDs

Лист `NPC Sprite IDs` даёт визуальные кадры NPC и примеры:

```txt
showFrame Abigail 33
animate Abigail false true 250 16 17 18 19
```

Главное отличие от фермера: спрайт NPC меняется **целиком**, направление учитывается автоматически по самому frame ID (внутри спрайтшита расположены подряд кадры всех 4-х направлений).

### 9.1. Базовый «грамматический» набор NPC-кадров

Большинство NPC использует одну схему 4×N: первая «строка» кадров — вниз, дальше право/вверх/влево, дальше специальные анимации.

| Frame range | Что обычно |
| --- | --- |
| 0 | idle вниз |
| 1–3 | walk вниз |
| 4 | alt вниз (часто — «жест рукой») |
| 5 | idle вправо |
| 6–7 | walk вправо |
| 8 | alt вправо |
| 9 | idle вверх |
| 10–11 | walk вверх |
| 12 | alt вверх |
| 13 | idle влево |
| 14–15 | walk влево |
| 16+ | специальные кадры — танец, готовка, инструмент, чтение и т. д. |

Для Харви этот «обычный» 16-кадровый блок выглядит так:

| Frame | Что |
| --- | --- |
| 0–4 | вниз: idle / walk / alt |
| 5–8 | вправо |
| 9–12 | вверх |
| 13–16 | влево |
| 19 | редкий жест (использовался в vanilla `showFrame Harvey 19` 1 раз) |
| 20–23 | стетоскоп / осмотр (см. 9.4) |
| 24–28 | вариативные «врачебные» кадры (рука у подбородка и т. п.) |
| 29–33 | разговорные кадры с расширенной мимикой |

### 9.2. Что Харви реально делает в vanilla

Самые частые `showFrame Harvey N` в Raw dump:

| Frame | Сколько раз | Описание (визуально из ванили) |
| --- | --- | --- |
| 26 | 7 | Спокойный взгляд / разговор |
| 30 | 6 | Жест рукой, «объясняет» |
| 33 | 6 | Серьёзный, чуть наклон головы |
| 25, 27, 31 | по 4 | Промежуточные «discussing» кадры |
| 0 | 4 | Idle вниз |
| 20, 24, 29, 32, 12 | по 2 | Сцены осмотра |
| 8, 19, 21, 23, 28, 4 | по 1 | Редкие специальные кадры |

Раз vanilla обходится этими ~10 кадрами для всех heart events — для CP-сцен HarveyOverhaul можно безопасно опираться на тот же набор.

### 9.3. Анимации Харви в vanilla (Raw dump)

`animate Harvey ...` встречается 4 раза в ванили. Известный приём — «Харви возится с инструментом/папкой»:

```txt
animate Harvey false true 250 12 13 14 15
```

Эту анимацию можно интерпретировать как «листает карту/папку», «делает запись», «прокручивает что-то в руках». Подходит для сцены **заполнения медицинской карты**.

Для Харви безопасно использовать паттерн «короткий цикл из 2–4 соседних кадров с шагом 200–300 мс»:

```txt
animate Harvey false true 250 25 26 27 26   # кивает в такт разговору
animate Harvey false true 300 30 31         # объясняет, машет рукой
animate Harvey false true 200 20 21 22 21   # осматривает (рука к лицу пациента)
```

Не забыть `stopAnimation Harvey` и `faceDirection Harvey <dir>` после остановки.

### 9.4. Рецепт «Харви проверяет пульс»

```txt
faceDirection Harvey 0/
faceDirection farmer 2/
pause 300/
emote Harvey 16/
speak Harvey "Сейчас проверю пульс. Дыши ровно.$u"/
pause 300/
animate Harvey false true 800 22 20 21 20/
pause 1200/
stopAnimation Harvey/
speak Harvey "Пульс частый, но уже ровнее.$s"
```

### 9.5. Рецепт «Харви строгий врач»

```txt
faceDirection Harvey 2/
showFrame Harvey 33/
emote Harvey 12/
pause 500/
speak Harvey "Нет. В шахту сегодня ты не пойдёшь.$a#$b#Я видел такие симптомы слишком много раз, чтобы позволить тебе рисковать.$s"
```

### 9.6. Рецепт «Харви делает запись в карте»

```txt
faceDirection Harvey 0/
pause 200/
animate Harvey false true 250 12 13 14 15/
pause 1500/
stopAnimation Harvey/
faceDirection Harvey 2/
speak Harvey "Записал. Сегодня — постельный режим. Без обсуждения.$u"
```

### 9.7. Анимации других NPC (для перекрёстных сцен)

Если в сцене Харви оказывается ещё кто-то (Maru как медсестра, Penny с детьми и т. д.), полезно знать «фирменные» анимации NPC из vanilla:

| NPC | Анимация (`animate <NPC> false true <ms> <frames>`) | Что это |
| --- | --- | --- |
| Maru | `animate Maru false true 250 29 30` | «Колет инъекцию / работает с прибором» |
| Penny | `animate Penny false true 250 0 1 2 3` | Учительский жест |
| Penny | `animate Penny false true 250 24 25` | Чтение книги |
| Sebastian | `animate Sebastian false true 250 42 43 52 53` | Курит / стоит у окна |
| Abigail | `animate Abigail false true 250 49 34 35 48` | Игра на флейте |
| Sam | `animate Sam false true 250 20 21` | Бьёт по гитаре |
| Sam | `animate Sam false true 250 22 23` | Sk8 трюк |
| Leah | `animate Leah false true 250 30 31` | Лепит из глины |
| Pierre / Morris / Pam / Vincent / Linus | `animate <NPC> false true 250 20 21` | Универсальный «жест разговора» |
| Robin | `animate Robin false true 250 32 33` | Молотком стучит |
| Robin | `animate Robin false true 250 34 35` | Пилит доску |
| Elliott | `animate Elliott false true 250 36 37` | Пишет роман |
| Shane | `animate Shane false true 250 0 30 0 30 0 31 0 31` | «Шатается пьяный» |
| Bear | `animate Bear false true 250 16 17` | Поворот головы |
| Dog | `animate Dog false true 250 20 21 22 23` | Виляет хвостом |
| Cat | `animate Cat false true 250 16 17 18` | Умывается |

> Полный список таких паттернов лежит в Raw dump xlsx. Чтобы добавить новую анимацию NPC — найти в Raw dump строку `animate <NPC>` и скопировать рабочий набор кадров.

### 9.8. Распределение анимаций по NPC в vanilla

Кто чаще всего «играет» в кат-сценах (по Raw dump, всего 261 вызов `animate`):

```
farmer    69    Penny     18    Sebastian 15    Elliott   14
Emily     14    Abigail   14    Vincent   11    Sam       11
Leah       9    Pam        9    Alex       7    Bear       7
Shane      6    Robin      6    Morris     6    Pierre     5
Maru       5    Harvey     4    Linus      4
```

Харви проигрывает только 4 анимации — это значит, что для нового контента **новые анимационные циклы Харви — это ответственность мода**, и стоит закладывать запас тестового времени.

---

## 10. Звуки

### 10.1. Подборка для медицинских/романтических событий Харви

| Sound ID | Описание | Где уместно |
| --- | --- | --- |
| dialogueCharacter | Single beep | Advance dialogue box |
| doorClose | Door closing | Closing door |
| doorCreak | Creaking of the hinge of a fairly small box being opened | Opening doors, or the item sell box |
| thunder | Loud, thundery explosion, similar to the meteor strike sound | Ambient thunder sound |
| rain | Ambient, sound of rain outdoors, loops perfectly | Ambient rain sound |
| debuffHit | Video game style magic effect | Farmer pushed back by the barrier blocking mutant bug lair; using the Shrine of Memories |
| ow | Electric coughing sound | Taking damage |
| fallDown | A quick descending slide-whistle-style sound | Falling down a hole in the Skull Cavern |
| healSound | Retro style magic effect sound | Shadow Brute Shaman healing |
| achievement | Two happy "success" synth tones, echoing | Achievement sound |
| questcomplete | Five happy "success" xylophone notes, "you did a thing!" | Journal updated or quest completed |
| yoba | A simple chime | Shrine of appearances confirm change |
| secret1 | A fairly long mystical chime signalling a discovery | Find Pierre’s hidden stash in heart event |
| discoverMineral | A happy success chime | A geode had a good item in it |
| Hospital_Ambient | Ambient sound of an office with computers running | Inside the bathhouse, supposedly |
| breathin | Short, soft ascending fwoop | Dialogue box open |
| breathout | Short, soft descending bleep | Dialogue box close |
| wind | Very short, breathy wind noise | Complete all bundles in 1 package (unconfirmed) |
| explosion | Loud explosion | Bomb exploding |
| fireball | Typical video game fire ignite / fireball cast sound | Light fireplace, brazier, etc |
| stoneStep | Knocking on hard wood | Walking on stone |
| grassyStep | Shovelling sand | Walking on grass |
| woodyStep | Low knock, arcade-like footstep | Walking on wood |
| thudStep | A low bonk sound | Walking on wood/placing a bomb |
| shadowpeep | A breathy mythical whistle | Shadow Brute sound |
| clubloop | Low, ambient, kind of creepy hum with occasional slide whistles | Casino "music" |

### 10.2. Рецепты звука

#### Дверь / ночной визит

```txt
playSound doorClose/
pause 500/
speak Harvey "Это я. Не вставай, пожалуйста.$s"
```

#### Гроза / тревога

```txt
playSound thunder/
pause 600/
emote farmer 12/
speak Harvey "Ко мне. Сейчас — вместе.$a"
```

#### Завершение лечения

```txt
playSound achievement/
speak Harvey "Вот и всё. Лечение завершено.$h#$b#Но я всё равно буду присматривать за тобой ещё пару дней.$l"
```

#### Боль / резкое ухудшение

```txt
playSound ow/
shake farmer 800/
speak Harvey "Не двигайся. Я посмотрю.$a"
```

---

## 11. Музыка и ambience

### 11.1. Подборка для Харви-сцен

| Music/Ambient ID | Описание | Использование в таблице | Название трека |
| --- | --- | --- | --- |
| kindadumbautumn | Guitar and flute, pleasant autumn-y vibes | Harvey heart event | Grapefruit Sky (Dr. Harvey's Theme) |
| harveys_theme_jazz |  | Harvey's 14-Heart event | Grapefruit Sky (Pasta Primavera Mix) |
| sweet | Flute and pizzicato, then piano/bass, more "woods exploration" music | Wedding (start), Harvey 4 heart? | Buttercup Melody |
| musicboxsong | A nice and pleasant music box style tune | Stealing a kiss somewhere | Music Box Song |
| sadpiano | Melancholy piano piece | Abigail's 10 heart (Confession) | A Dark Corner Of The Past |
| desolate | Melancholy piano piece, Chrono Trigger style | Alex heart event, Abigail's 10 heart event | A Sad Story (Alex's Theme) |
| nightTime | Ambient, crickets, chirping |  | - |
| spring_night_ambient | Ambient, insects at night |  | - |
| spaceMusic | Bass and synth, crickets chirping, moody night ambience | Maru heart event | Starwatcher (Maru's Theme) |
| communityCenter | Ambient, quiet wind chimes and mystic sounds |  | - |
| babblingBrook | Ambient, babbling brook |  | - |
| ocean | Ambient, ocean waves | Beach exterior | - |
| cracklingFire | Ambient, crackling campfire |  | - |
| clubloop | Ambient sound loop (low hum, cavern, echoey) |  | - |
| Upper_Ambient | Cave ambience - slight wind, drips of water | Mines exploration | - |
| Frost_Ambient | Cave ambience - wind, water on ice | Mines exploration | - |
| Lava_Ambient | Cave ambience - deep thrum, bubbling of lava | Mines exploration | - |
| distantBanjo | Bass line, harmonica, banjos, optimistic town music |  | Distant Banjo |
| springtown | Pleasant guitar ballad | Generic spring music | Pelican Town |
| aerobics | Looping shitty aerobics music | Harvey's 6 heart (Aerobics) |  |

### 11.2. Рецепты музыки

#### Тёплая сцена Харви

```txt
kindadumbautumn/
farmer 20 12 2 Harvey 22 12 3/
skippable/
...
```

#### Тихая романтическая сцена

```txt
musicboxsong/
...
```

#### Тревожная сцена / кризис

```txt
sadpiano/
...
```

#### Ночь

```txt
nightTime/
...
```

#### Шахта / опасность

```txt
Upper_Ambient/
...
```

---

## 12. Предметы для реквизита

Подборка ID из `Item IDs`, полезная для сцен Харви:

| Item ID | Предмет | Описание |
| --- | --- | --- |
| 395 | Coffee | It smells delicious. This is sure to give you a boost. |
| 253 | Triple Shot Espresso | It's more potent than regular coffee! |
| 196 | Salad | A healthy garden salad. |
| 216 | Bread | A crusty baguette. |
| 18 | Daffodil | A traditional spring flower that makes a nice gift. |
| 20 | Leek | A tasty relative of the onion. |
| 22 | Dandelion | Not the prettiest flower, but the leaves make a good salad. |
| 16 | Wild Horseradish | A spicy root found in the spring. |
| 591 | Tulip | The most popular spring flower. Has a very faint sweet smell. |
| 815 | Tea Leaves | The young leaves of the tea plant. Can be brewed into the popular, energizing beverage. |
| 614 | Green Tea | A pleasant, energizing beverage made from lightly processed tea leaves. |
| 167 | Joja Cola | The flagship product of Joja corporation. |
| 773 | Life Elixir | Restores health to full. |
| 349 | Energy Tonic | Restores a lot of energy. |
| 351 | Muscle Remedy | When you've pushed your body too hard, drink this to remove 'Exhaustion'. |
| 403 | Field Snack | A quick snack to fuel the hungry forager. |
| 651 | Poppyseed Muffin | It has a soothing effect. |
| 78 | Cave Carrot | A starchy snack found in caves. It helps miners work longer. |
| 404 | Common Mushroom | Slightly nutty, with good texture. |
| 406 | Wild Plum | Tart and juicy with a pungent aroma. |
| 396 | Spice Berry | It fills the air with a pungent aroma. |
| 410 | Blackberry | An early-fall treat. |
| 408 | Hazelnut | That's one big hazelnut! |
| 412 | Winter Root | A starchy tuber. |
| 416 | Snow Yam | This little yam was hiding beneath the snow. |
| 414 | Crystal Fruit | A delicate fruit that pops up from the snow. |
| 418 | Crocus | A flower that can bloom in the winter. |

### 12.1. Рецепты с предметами

#### Харви даёт воду/лекарство через нарратив

Лучше всего для медицинской сцены:

```txt
message "Харви протягивает тебе стакан воды."/
speak Harvey "Медленно. Маленькими глотками.$u"
```

#### Предмет над головой

```txt
itemAboveHead farmer 395/
speak Harvey "Кофе — не завтрак, но сейчас хотя бы поможет тебе согреться.$s"
```

#### Удалить предмет по квестовой сцене

```txt
removeItem 78/
speak Harvey "Спасибо. Cave Carrot пригодится для питательной смеси.$h"
```

---

## 13. Символы в `textAboveHead`

Из листа `Symbols`:

| Символ | Описание |
| --- | --- |
| < | Less Than Sign |
| = | Equals Sign |
| $ | Dollar Sign |
| @ | At Sign |
| ` | Grave Accent |
| > | Greater Than Sign |

Пример:

```txt
textAboveHead Harvey "..."
textAboveHead farmer "<"
```

Осторожно: `$` в диалогах имеет специальное значение, поэтому не вставлять его в реплики без проверки.

---

## 14. Event ID и защита от конфликтов

Лист `Event IDs` содержит список ID из base game и модов. В самой таблице есть рекомендация для новых моддеров использовать 8- или 9-значные ID, где первые 4 цифры соответствуют Nexus mod id.

Для этого проекта лучше придерживаться одного из двух подходов:

### Вариант A — строковые ID

```txt
eventHarveyMineRescueDating
HarveyOverhaulStory.E1_SlipperyPath
HarveyMod_FirstTreatment
```

Плюсы: читаемо, удобно искать, меньше риск случайно пересечься с vanilla numeric IDs.

### Вариант B — числовые ID с префиксом

```txt
123400001
123400002
123400003
```

Плюсы: ближе к старой практике. Минус: хуже читается.

Правило для Cursor: новые события HarveyOverhaul создавать со строковыми ID с префиксом:

```txt
HarveyOverhaulInjury.EventName
HarveyMod.EventName
eventHarveyEventName
```

---

## 15. Рецепты для InjuryCare-событий

Эти рецепты заточены под проект, где C# управляет травмами, баффами, фазами и топиками, а CP показывает сцены.

### 15.1. Событие после C# topic

C# выставляет topic:

```csharp
_dialogueManager.AddTopic("topicMineInjuryRescue", 3);
```

CP читает topic:

```json
{
  "Action": "EditData",
  "Target": "Data/Events/Hospital",
  "Entries": {
    "eventHarveyMineRescueFollowup/Time 900 1700/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicMineInjuryRescue/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyMineRescueFollowup": "sadpiano/-1000 -1000/farmer 20 12 2 Harvey 22 12 3/skippable/viewport 21 12 true/pause 500/speak Harvey \"Ты напугала меня вчера.$s#$b#Я не буду ругать тебя сейчас. Сначала — осмотр.\"/emote Harvey 12/pause 500/speak Harvey \"Но потом мы серьёзно поговорим о шахте.$a\"/friendship Harvey 25/end"
  }
}
```

### 15.2. Сцена смены фазы лечения

Условие:

```txt
GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicDeepCutsPhaseHealing
```

Сцена:

```txt
kindadumbautumn/
farmer 20 12 2 Harvey 22 12 3/
skippable/
viewport 21 12 true/
speak Harvey "Повязка чистая. Хорошо.$h#$b#Рана больше не выглядит острой, но расслабляться рано.$u"/
pause 300/
speak Harvey "Теперь начинается стадия заживления. Я хочу, чтобы ты берегла руку.$s"/
addConversationTopic topicHarveyDeepCutsCare 3/
friendship Harvey 20/
end
```

Важно: не добавлять и не удалять баффы в CP. Это делает C#.

### 15.3. Ночная проверка

```txt
eventHarveyNightRoundFollowup/Time 600 900/GameStateQuery PLAYER_HAS_CONVERSATION_TOPIC Current topicHarvey_NightRound/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current eventHarveyNightRoundFollowup
```

Сцена:

```txt
nightTime/
farmer 10 7 2 Harvey 10 9 0/
skippable/
pause 500/
playSound doorClose/
speak Harvey "Доброе утро. Я знаю, что заходил поздно.$s#$b#И да, я всё ещё считаю, что поступил правильно.$u"/
speak Harvey "Сегодня — без шахт, без тяжёлых инструментов и без геройства.$a#$b#Сначала завтрак. Потом я проверю повязку.$l"/
friendship Harvey 15/
end
```

### 15.4. Запрет шахты

```txt
playSound debuffHit/
emote Harvey 12/
speak Harvey "Нет. В таком состоянии — никаких шахт.$a#$b#У тебя серьёзная травма, и я не позволю тебе усугубить её ради пары камней.$s"/
question mineBan "Промолчать#Возразить#Попросить проводить домой"/
```

Ветки:
- промолчать → мягче;
- возразить → Харви строже;
- попросить проводить → романтическое усиление.

### 15.5. Мокрая повязка

```txt
rain/
playSound rain/
speak Harvey "Повязка промокла?$s#$b#Покажи. Сейчас же.$a"/
message "Он осторожно разворачивает край бинта, стараясь не причинить боль."/
speak Harvey "Никаких мокрых бинтов. Это прямой путь к инфекции.$s"/
addConversationTopic topicHarvey_WetBandageAftercare 3/
end
```

### 15.6. Инфекция

```txt
debuffHit/
speak Harvey "Температура. Покраснение. Боль усилилась...$s#$b#Это инфекция.$a"/
pause 500/
speak Harvey "Слушай меня внимательно. Теперь лечение будет строгим. Без пропусков.$a"/
friendship Harvey 20/
end
```

---

## 16. Рецепты движения

### 16.1. Простое движение

```txt
move Harvey 0 1 1
```

Формат:

```txt
move <actor> <dx> <dy> <facing?>
```

### 16.2. Сложный маршрут

```txt
advancedMove Harvey false 0 -1 1 0 0 1
```

Использовать, когда NPC должен пройти несколько шагов подряд.

### 16.3. Безопасная постановка перед движением

Если персонаж не доходит на 1 тайл:

1. Проверить стартовую позицию.
2. Проверить коллизии карты.
3. Временно заменить длинный маршрут на `warp`.
4. Потом вернуть короткое движение.

```txt
warp Harvey 22 12/
pause 200/
advancedMove Harvey false 0 -1/
pause 300/
faceDirection Harvey 2
```

---

## 17. Рецепты камеры

### 17.1. Навести камеру на тайл

```txt
viewport 21 12 true
```

### 17.2. Плавное движение камеры

```txt
viewport move 1 0 3000
```

### 17.3. Вернуть камеру

```txt
viewport -2000 -2000
```

---

## 18. Рецепты затемнения и смены карты

### 18.1. Затемнение

```txt
globalFade/
pause 500/
```

### 18.2. Временная карта

Из raw dump часто используется:

```txt
changeToTemporaryMap MarnieBarn false
```

Рецепт:

```txt
globalFade/
viewport -2000 -2000/
changeToTemporaryMap SomeMap false/
warp farmer 10 10/
pause 500/
```

Использовать только если нужна отдельная постановочная сцена, иначе лучше ставить событие на существующей карте.

---

## 19. Чеклист перед правкой события

Перед изменением Cursor должен ответить себе:

- Какой файл меняется?
- Какой `eventId`?
- На какой карте событие живёт?
- Какие preconditions уже есть?
- Есть ли `!PLAYER_HAS_SEEN_EVENT`?
- Есть ли topic, который событие требует?
- Кто выставляет этот topic: CP или C#?
- Есть ли в событии `end`?
- Есть ли команды, которые меняют состояние: `mail`, `addConversationTopic`, `friendship`, `removeItem`, `addQuest`?
- Нужно ли событие повторять или оно одноразовое?
- Нужно ли после события удалить/заменить topic?
- Не ломает ли сцена молчаливый характер фермерши?

---

## 20. Чеклист тестирования в игре

Для каждого события:

1. Запустить SMAPI.
2. Включить сохранение, где доступны нужные условия.
3. Проверить, что `content.json` загрузился без ошибок.
4. Проверить, что Target `Data/Events/<Location>` существует.
5. Проверить, что условия реально выполнимы.
6. Войти в нужную локацию в нужное время.
7. Проверить:
   - персонажи стоят на правильных тайлах;
   - камера не улетает;
   - нет застревания на `move`;
   - `question/fork` работает;
   - финальный `end` отдаёт управление игроку;
   - topic/mail/friendship применяются;
   - событие не повторяется бесконечно.

---

## 21. Типичные ошибки

### Ошибка: событие не срабатывает

Проверить:
- неправильная локация `Data/Events/...`;
- конфликт ID;
- уже просмотрено событие;
- topic не активен;
- time window слишком узкий;
- погода не совпадает;
- условие `Friendship` / `PLAYER_HEARTS` недостижимо.

### Ошибка: персонаж не доходит до точки

Проверить:
- коллизия;
- NPC/объект на пути;
- неверный стартовый `warp`;
- слишком длинный `advancedMove`.

Решение:
```txt
warp actor X Y/
pause 200/
move actor dx dy facing
```

### Ошибка: фермер выглядит сломанно после `showFrame`

Проверить:
- был ли `faceDirection farmer ...`;
- нужен ли `showFrame farmer ID` или `showFrame ID`;
- не используется ли проблемный кадр из списка выше.

### Ошибка: диалог Харви не с тем портретом

Проверить `$h/$s/$u/$l/$a` в конце реплики.

### Ошибка: JSON невалиден

Проверить:
- кавычки внутри script экранированы `\"`;
- нет комментариев;
- нет висячих запятых;
- переносы строк сделаны валидно для JSON.

---

## 22. Как Cursor должен работать с этим документом

### 22.1. Когда только аудит, без правок

```txt
Задача: только аудит. Ничего не менять в коде и JSON.

Изучи событие <eventId> в файле <path>. Проверь:
1. Выполнимы ли preconditions.
2. Не конфликтует ли eventId.
3. Есть ли защита от повторов.
4. Логична ли постановка персонажей.
5. Нет ли рискованных showFrame/animate.
6. Есть ли end.
7. Не дублирует ли событие C#-логику травм/баффов.
8. Соответствует ли тон Харви: заботливый, внимательный, врачебно-настойчивый, иногда гиперопекающий, но не жестокий.

Выведи результат в Markdown: проблема → риск → как исправить. Код не править.
```

### 22.2. Когда можно править конкретное событие

```txt
Задача: точечная правка. Менять только событие <eventId> в файле <path>. Остальные события не трогать.

Используй рецепты из docs/event-recipes.md.
Нужно:
- сохранить eventId;
- сохранить все существующие guards, если они не названы как проблема;
- не менять ID топиков/писем;
- не трогать C#;
- не добавлять JSONC-комментарии;
- оставить JSON валидным.

После правки кратко перечисли, какие команды изменены.
```

### 22.3. Когда нужно создать новое событие

```txt
Задача: создать новое CP-событие для HarveyOverhaul.

Сначала предложи:
1. eventId;
2. Target `Data/Events/<Location>`;
3. preconditions;
4. какие topics нужны;
5. какие topics событие добавляет;
6. нужна ли связь с C# InjuryCare.

Потом создай событие в нужном JSON-файле.
Тон Харви: заботливый врач, уверенный, настойчивый, физически и эмоционально надёжный. Не делать его карикатурно грубым.
Фермерша в основном молчит; ответы — через `question`, `message`, жесты и короткие реакции.
```

### 22.4. Когда нужно форматировать событие

```txt
Задача: отформатировать event script для читаемости.

Не использовать Python-скрипты.
Не менять смысл, порядок команд, ID, условия, тексты.
Разбить команды по строкам так, чтобы JSON оставался валидным.
Если формат проекта требует одну строку — не переносить. Сначала проверить существующий стиль файла.
```

---

## 23. Мини-библиотека сцен Харви

### 23.1. Мягкая забота

```txt
speak Harvey "Я не собираюсь давить.$s#$b#Но я останусь рядом, пока дыхание не станет ровным.$l"
```

### 23.2. Строгий врач

```txt
speak Harvey "Нет. Это не упрямство с моей стороны — это медицинское решение.$a#$b#Ты не пойдёшь туда с такой травмой.$s"
```

### 23.3. Гиперопека без жестокости

```txt
speak Harvey "Я знаю, ты привыкла справляться сама.$s#$b#Но сейчас ты под моим наблюдением. И да, я буду занудным.$u#$b#Потому что ты мне важна.$l"
```

### 23.4. Молчаливая фермерша

```txt
message "Ты молча протягиваешь руку. Харви замечает, как дрожат пальцы."
speak Harvey "Спасибо. Этого достаточно.$l"
```

### 23.5. После шахты

```txt
speak Harvey "Я видел, в каком состоянии тебя принесли.$s#$b#Не проси меня сделать вид, что всё нормально.$a"
```

### 23.6. После выздоровления

```txt
speak Harvey "Рана закрылась хорошо.$h#$b#Но восстановление — это не приглашение снова испытывать судьбу.$u"
```

---

## 24. Быстрый старт для нового события

Скопировать и адаптировать:

```json
{
  "Action": "EditData",
  "Target": "Data/Events/Hospital",
  "Entries": {
    "HarveyOverhaulInjury.Example/Time 900 1700/Friendship Harvey 1000/GameStateQuery !PLAYER_HAS_SEEN_EVENT Current HarveyOverhaulInjury.Example": "kindadumbautumn/-1000 -1000/farmer 20 12 2 Harvey 22 12 3/skippable/viewport 21 12 true/pause 500/speak Harvey \"Садись, пожалуйста. Я хочу убедиться, что тебе лучше.$u\"/pause 300/faceDirection farmer 0/showFrame farmer true 117/speak Harvey \"Вот так. Не геройствуй хотя бы пять минут.$h#$b#Я рядом.\"/friendship Harvey 20/addConversationTopic topicHarveyExampleAftercare 3/end"
  }
}
```

---

## 25. Спецэффекты сцены (`specificTemporarySprite`, `glow`, `screenFlash`, `shake`, jitter, fade)

### 25.1. `specificTemporarySprite`

Готовые «эффектные» спрайты из vanilla Stardew, которые можно вызвать одной командой. Их код жёстко прописан в `Event.cs`, поэтому набор фиксирован, но он удобный.

Список ID из Raw dump, отсортированный по частоте:

| ID | Где использовался | Назначение в Harvey-сценах |
| --- | --- | --- |
| `heart` (7) | Многие романтические сцены | Парящие сердечки над парой |
| `shakeTent` / `stopShakeTent` (по 7) | Палатка трясётся / останавливается | Сцена сборов / тревожной ночи (мало применимо для больницы) |
| `curtainOpen` / `curtainClose` (6/5) | Шторы открываются/закрываются | Открыть «занавес» в начале драматической сцены |
| `balloonBirds` (4) | Птицы вокруг шара | Финальная сцена Harvey-balloon ивента |
| `removeSprite` (3) | Удалить активный спецспрайт | Очистка перед сменой эффекта |
| `grandpaSpirit` / `grandpaNight` (по 2) | Дух дедушки | — (специфично для Grandpa-ивентов) |
| `joshSteak` (2) | Алекс жарит стейк | — |
| `shanePassedOut` | Силуэт Шейна без сознания | **Идеален для сцены обморока фермерши** |
| `waterShane` / `waterShaneDone` | Стакан воды у бессознательного NPC | **Точный аналог «Харви подаёт воду»** |
| `umbrella` | Раскрытый зонт | Сцена дождя у дома Харви |
| `morrisFlying` | Моррис улетает | — |
| `springOnion`, `springOnionDemo`, `springOnionPeel`, `springOnionRemove` | Луковые сцены Leah 5h | — |
| `EmilyBoomBox`, `EmilyBoomBoxStart`, `EmilyBoomBoxStop` | Бумбокс | Музыкальная сцена |
| `EmilySleeping` | NPC спит | **Подходит для сцены «фермерша спит в палате»** |
| `maruBeaker` | Колба | Лабораторная сцена с Мару |
| `leahLaptop` | Ноутбук | Деловая сцена |
| `pennyFieldTrip` | Дети с Пенни | Сцена с детьми |
| `shaneHospital`, `shaneCliffs` | Кризисные сцены Shane 6h/7h | **`shaneHospital` визуально перекликается с Harvey-палатой** |
| `EmilySign`, `EmilySongBackLights`, `jasGift`, `jasGiftOpen`, `elliottBoat`, `beachStuff`, `willyCrabExperiment`, `leahTree`, `leahPicnic`, `haleyRoomDark`, `JoshMom`, `joshFootball` | По 1 разу | Специфичны для своих сцен; смотреть Raw dump перед использованием |

#### Рецепты с `specificTemporarySprite`

```txt
specificTemporarySprite heart/
pause 1500/
specificTemporarySprite removeSprite
```

```txt
faceDirection farmer 0/
showFrame farmer 94/
specificTemporarySprite shanePassedOut/
pause 800/
emote Harvey 16/
speak Harvey "@!$a#$b#Очнись.$s"/
specificTemporarySprite waterShane/
pause 1200/
specificTemporarySprite waterShaneDone/
speak Harvey "Маленькими глотками. Тихо.$u"
```

### 25.2. `glow` — окрашивание сцены

Команда `glow R G B <holdAfterEvent>` накладывает цветной оверлей на сцену.

Из Raw dump чаще всего используются:

| RGB | hold | Сколько раз | Эффект |
| --- | --- | --- | --- |
| `0 0 255 false` | 4 | Синее свечение (магия, холод, ночь) |
| `255 0 255 false` | 4 | Розово-фиолетовое (магия Wizard) |
| `0 255 255 false` | 3 | Бирюзовое (вода, лёд, призрак) |
| `255 255 0 false` | 2 | Жёлтое (тёплый свет, лампа) |
| `0 255 0 false` | 2 | Зелёное (тревога, отрава, инфекция) |
| `255 0 0 false` | 2 | Красное (опасность, инфекция, лихорадка) |
| `0 0 255 true` | 1 | Синее с удержанием после события |
| `80 70 255 true` | 1 | Тёмно-фиолетовый (мистика) |

Рецепт «лихорадка / инфекция»:

```txt
glow 255 0 0 false/
pause 1000/
speak Harvey "Температура. Это инфекция.$s"/
```

Рецепт «ночь в палате»:

```txt
glow 80 70 255 true/
nightTime/
pause 1500/
speak Harvey "Тише. Не вставай.$u"
```

### 25.3. `screenFlash` и `fade` / `globalFade`

| Команда | Что делает |
| --- | --- |
| `screenFlash 0.5` | Короткая вспышка с указанной прозрачностью (0..1) |
| `fade` | Постепенно затемнить экран (НЕ блокирует) |
| `fade unfade` | Развернуть затемнение обратно |
| `globalFade` | Полное затемнение и пауза скрипта до полного fade-in |
| `globalFade <speed>` | Затемнение с заданной скоростью |
| `globalFadeToClear` | Из чёрного в нормальный экран |

Пример удара / резкой боли:

```txt
playSound ow/
screenFlash 0.7/
shake farmer 800/
pause 300/
speak Harvey "Не двигайся.$a"
```

### 25.4. `shake` и jitter

- `shake <actor> <duration_ms>` — встряхивает спрайт актёра (отыгрывает страх, удар, всхлип).
- `startJittering` / `stopJittering` — фермер «трясётся» (Harvey 7h-ивент аналог).
- В Raw dump их 15 пар (`startJittering` ↔ `stopJittering`), всегда вместе.

```txt
startJittering/
pause 500/
speak Harvey "Ты дрожишь. Я тебя укрою.$s"/
pause 1500/
stopJittering
```

### 25.5. `cutscene`, `screenFlash`, `proceedPosition`

| Команда | Назначение |
| --- | --- |
| `cutscene <name>` | Запустить предзаписанную ванильную кат-сцену по имени (`marriageDance`, `eggHuntWinner`, `marcelloBalloon` и т. д.) |
| `proceedPosition <actor>` | Принудительно «довести» актёра до точки, если `advancedMove` застрял |
| `addObject <x> <y> <itemId>` | Положить предмет на карту во время сцены |
| `removeObject <x> <y>` | Убрать предмет с карты |
| `removeTile <x> <y> <layer>` | Скрыть конкретный тайл (`Front`, `Buildings`, `AlwaysFront`) |
| `temporaryAnimatedSprite ...` | Полностью кастомный временный спрайт (тяжёлый синтаксис, см. Wiki) |

---

## 26. Расширенные варианты `end`

После `end` можно поставить «модификатор», который меняет то, как сценарий передаёт управление игроку. Из Raw dump:

| Форма | Сколько в ванили | Что делает |
| --- | --- | --- |
| `end` | по умолчанию (175) | Просто закрывает событие, оставляя игрока на текущей карте/тайле |
| `end dialogue <NPC> "текст"` | 45 | После события NPC «продолжает» с обычным диалогом |
| `end warpOut` | 19 | Перенести игрока «наружу» (на улицу из дома/локации) |
| `end position <x> <y>` | 6 | Поставить игрока в конкретную точку текущей карты |
| `end dialogueWarpOut <NPC> "текст"` | 3 | Диалог + telep наружу |
| `end bed` | 2 | Уложить игрока в кровать (как окончание дня) |
| `end Maru1` / `end Position` | 2 | Кастомные хендлеры — игнорировать без явной необходимости |
| `end invisibleWarpOut` | 1 | Невидимый warp-out без анимации |
| `end beginGame` | 1 | Старт новой игры (только для Intro) |

Полезные рецепты:

```txt
# Тихо вернуть фермершу в палату:
end position 12 14

# Закончить с тёплым диалогом Харви:
end dialogue Harvey "Береги себя. Я зайду вечером.$l"

# После события сразу уложить в постель (закончить день):
end bed
```

---

## 27. Расширенные варианты `viewport`

| Форма | Что делает |
| --- | --- |
| `viewport <x> <y> true` | Центрировать камеру на тайле `<x,y>` и зафиксировать (`true` = clamp by default) |
| `viewport <x> <y> clamp true` | То же, но с «прижиманием» к границе карты — частый паттерн в больших локациях |
| `viewport -1000 -1000` | «Спрятать» камеру за край мира (классический способ начать чёрный экран) |
| `viewport -2000 -2000` | То же — глубже за пределы; разница чисто эстетическая |
| `viewport -100 -100` | Маленький off-screen — хорошо для коротких затемнений |
| `viewport move <dx> <dy> <duration_ms>` | Плавно сместить камеру на `<dx>×<dy>` тайлов за `<duration_ms>` мс |
| `viewport <x> <y>` без `true` | Установить позицию, но **не** блокировать камеру — игрок может её сдвинуть |

Типичные значения из Raw dump (для понимания «масштаба»):
`viewport move 0 2 800` (медленный сдвиг вниз) … `viewport move 1 0 5500` (длинное горизонтальное проезд).

Рецепт «приехать на тайл, потом плавно отъехать обратно»:

```txt
viewport 22 12 true/
pause 800/
speak Harvey "Я вижу повязку отсюда. Подожди.$s"/
pause 500/
viewport move 0 -2 1500/
pause 1500/
viewport -2000 -2000
```

---

## 28. Временные акторы, broadcast и переменные

### 28.1. `addTemporaryActor`

Добавляет NPC, которого нет на карте по умолчанию (нужно для сцен с приглашёнными гостями, призраками, временными помощниками).

Синтаксис:

```txt
addTemporaryActor <name> <sprite_w> <sprite_h> <x> <y> <facing> [breather] [type]
```

- `<name>` — имя спрайта в `Characters/<name>`;
- `<sprite_w> <sprite_h>` — размеры одного кадра (обычно `16 32` для NPC, `16 16` для животных);
- `<x> <y>` — начальная клетка;
- `<facing>` — направление (0 — вниз, 1 — вправо, 2 — вверх, 3 — влево);
- `breather` — `true` если у NPC «дышащая» анимация (стандарт).

Пример (взято из Grandpa-ивента):

```txt
addTemporaryActor Grandpa 1 1 -100 -100 2 true/
specificTemporarySprite grandpaSpirit/
viewport -1000 -1000 true/
pause 10000/
speak Grandpa "Привет, дитя.$s"
```

Для Harvey-сцен это полезно, если в палату приходит **временный персонаж**: например, специалист из «другого госпиталя», старый друг, или родственник фермерши.

### 28.2. `broadcastEvent`

`broadcastEvent` — запустить кат-сцену для всех игроков в multiplayer (без него событие видит только хост). Использовать осторожно: для сценариев `Hospital` это редко нужно, но если событие сюжетное — стоит добавить.

### 28.3. `switchEvent <eventId>`

Прерывает текущий скрипт и сразу запускает другой `eventId` (без обычной проверки preconditions). Полезно для длинных «склейк»: разбили на 2-3 entry, между ними `switchEvent`.

### 28.4. `resetVariable` / `setVariable`

Управление сценарной переменной (для сложных fork-цепочек, когда выбор первого вопроса влияет на ветку через несколько секунд):

```txt
resetVariable/
question forkA "Согласиться#Отказаться"/
fork forkA_yes/
setVariable agreed/
...
```

### 28.5. `pause`, `speed`, `jump`

- `pause <ms>` — стандартная задержка (3316 раз в Raw dump — самая частая команда).
- `speed <actor> <multiplier>` — изменить скорость передвижения NPC (по умолчанию 4, можно поставить 6–8 для драматического «убегания»).
- `jump <actor> [intensity]` — прыжок (по умолчанию короткий; можно дать число до 16+ для «испуга»).

---

## 29. Напоминание для Cursor

Главная задача событий HarveyOverhaul — не показать максимум команд, а создать понятную сцену:

- зачем Харви пришёл;
- что он заметил;
- почему он тревожится;
- как он помогает;
- как фермерша реагирует без длинных речей;
- что меняется после сцены: topic, дружба, письмо, следующее событие.

Если команда не усиливает сцену — её лучше не добавлять.
