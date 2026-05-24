# CP Event Authoring Rules для HarveyOverhaul

Внутренний справочник для написания, аудита и исправления CP-событий мода
**HarveyOverhaul** в Stardew Valley. Перед созданием, проверкой или правкой
любого события Cursor **обязан** открыть этот документ, определить тип сцены
и пройтись по соответствующим чеклистам.

**Источники правил:**

- рабочие события проекта (`eventsCare.json`, `events.json`, `eventsMineRescue.json`);
- аудиты `docs/CheckEvent/*` (coordinate, story-arc, storm, mine, map-passports, tileset-reuse);
- backlog `docs/CheckEvent/events-map-fix-backlog.md`;
- стиль форматирования `docs/cp-event-formatting.md`;
- константы эмоций `Core/Emotes.cs`;
- синтаксис vanilla event commands / Content Patcher / SpaceCore.

**Что этот документ НЕ делает:**

- не меняет события и `content.json`;
- не подменяет map-паспорта — координаты всегда уточнять по
  [`map-passports.md`](../CheckEvent/map-passports.md);
- не отменяет ручную in-game проверку — он лишь сужает зону ошибок до правки.

---

## 1. Общие принципы

1. **Карта раньше координат.** Прежде чем вписать любое `X Y`, открыть
   паспорт локации в [`map-passports.md`](../CheckEvent/map-passports.md) и
   убедиться, что тайл существует, проходим и не зарезервирован
   warp/Buildings/Action. Если паспорт неполный — пометить в
   [`events-coordinate-audit.md`](../CheckEvent/events-coordinate-audit.md) как
   *needs visual check*, а не угадывать.
2. **Безопасная постановка раньше драматургии.** Сначала ставится
   технически рабочий каркас: `farmer`/`Harvey` на проходимых тайлах,
   корректные `faceDirection`, валидные `move`/`warp`. Только потом —
   реплики, emote, animate, музыка и свет.
3. **Условия события не трогаем.** `Time`, `Weather`, `Friendship`,
   `GameStateQuery`, `Random`, friendship/topic-флаги — не меняются вместе с
   правкой постановки. Условия — отдельная задача.
4. **Один Event ID на правку.** Никогда не править два события «заодно».
   Каждое событие — отдельный коммит/PR, иначе невозможно откатить.
5. **Никакого массового форматирования JSON.** `cp-event-formatting.md`
   запрещает sed/awk/jq/Python-скрипты и автоформаттеры на event scripts.
   Только ручной семантический diff.
6. **Никаких «координат на глаз».** Если нет TMX-паспорта тайла — событие
   **не** правится в этой точке; создаётся задача на экспорт карты.
7. **Любое движение проверяется по проходимости.** Все промежуточные тайлы
   маршрута должны быть проходимы (см. `events-coordinate-audit.md`).
   `move 0 -8` через 8 тайлов — это 8 проверок, не одна.
8. **Не править событие, если оно помечено OK во всех аудитах** без
   отдельной задачи. Исключение — выявленный новый блокер.
9. **Сохраняем тон HarveyOverhaul.** Харви — обеспокоенный, заботливый врач;
   ранние сцены — формально-вежливо, поздние/dating — мягко-настойчиво.
   Романтика не лезет в раннюю медицину (см. §6, §12.7).

---

## 2. Правила координат

### 2.1 Что проверять для каждой пары X Y

Перед использованием **любой** координаты `X Y` для actor/setup/warp/move/viewport:

- [ ] Тайл существует в пределах карты (X в `[0, width-1]`, Y в `[0, height-1]`).
  «Магическая» точка `-1000 -1000` — это **скрытие** actor вне карты, не
  обычная позиция.
- [ ] Слой Back проходим (`Passable=1` либо `NoSpawn=Tree/All` без блока).
- [ ] На слое Buildings **нет** тайла (Buildings объекты = непроходимо).
- [ ] На слое Front/AlwaysFront нет крупного overlay, перекрывающего спрайт
  персонажа (Warning, не всегда Broken).
- [ ] Нет Action/TouchAction/Warp на тайле, если только сцена явно не
  использует этот action.
- [ ] Нет мебели/стола/кровати/стеллажа на тайле (см. tileset reuse).
- [ ] Персонаж после warp/move виден игроку (если viewport не уведён).
- [ ] Персонаж не блокирует ключевой проход (дверь, узкий коридор).

### 2.2 Запрещено

- Ставить actor на **warp tile** (тайл-портал), если сцена не уводит actor
  немедленно. Пример: `(3,15)` ArchaeologyHouse — warp Town, не позиция.
- Ставить actor на **дверь** (Action Door / Buildings), кроме случая, когда
  сразу за дверью следует `move`/`warp` сквозь неё. Hospital `(5,9)`,
  `(10,13)` — двери, не setup-позиции.
- Ставить actor **внутрь спрайта мебели**: кровать, стойка, лампа, лавка,
  автобус, стеллаж. Hospital кровать `(20,5)` — Buildings; работает **только**
  с `ignoreCollisions` + `positionOffset` + lying frame (см. §3, §7, §12.3).
- Использовать координаты vanilla, не сверив SVE-патчи. SVE заменяет почти
  все outdoor-карты — см. [`maps-and-tilesets-inventory.md`](../CheckEvent/maps-and-tilesets-inventory.md).
- Использовать координаты `MineShaft`/`VolcanoDungeon` — это процедурные
  карты, в CP патчатся только `Mine`, `SkullCave`, `Custom_AdventurerSummit`.

### 2.3 Особые зоны Hospital (HarveyOverhaul-критично)

| Точка | Что это | Можно ли ставить actor |
|---|---|---|
| `(5,9)` | Action Door (вход с Town) | **Нет** (Buildings) |
| `(10,13)` | Action Door (внутренний) | **Нет** (Buildings) |
| `(10,19)` | Проходимая зона у входа | **Да** — типовой setup farmer |
| `(10,20)` | Warp Town | Только как exit, не setup |
| `(4,6)` | Проходимый тайл у кушетки | **Да** — setup farmer (checkup) |
| `(6,10)` | Проходимый тайл коридора | **Да** |
| `(20,5)` | Койка палаты (Buildings) | **Только** через `ignoreCollisions` + `positionOffset 32 -52` + lying animate |
| `(18–19,5)` | Соседние с кроватью тайлы | **Да** для Harvey сбоку |
| `(14,6)` | Прикушеточная зона / viewport | **Да**, типовой viewport |
| `(10,18)` | Front overlay | Warning — лучше `(9,19)` / `(11,19)` |
| `(15,8)` | Кресло / NightCrisis | Warning — verify in-game |

### 2.4 Особые зоны Mine

| Точка | Что это | Использование |
|---|---|---|
| `(17,7)` | Якорь rescue/interception (C# warp) | farmer setup для всех mine-событий |
| `(17,10)` | Стандартный spawn Harvey | Harvey setup |
| `(17,8)` | После `move Harvey 0 -2` | Промежуточная позиция |
| `(15,5)` | Платформа под лифтом, storm comfort | farmer (storm) |
| `(18,13)` | Южный край у warp Summit | **Опасно** — y≥11 Front/обрыв |
| `(18,14)` | Warp `Custom_AdventurerSummit` (SVE) | **Не setup**, только проход |
| `(17,3)` | Лифт | **Не ходить** |
| `(11–12,10)` | Вагонетка | **Не ходить** |
| `(23,9)` | Спуск | **Не ходить** |

Правило: на Mine **запрещены** `move`/`advancedMove` с y ≥ 11 для NPC.

### 2.5 Чеклист координаты `X Y`

```text
[ ] Тайл существует
[ ] Layer Back: проходим (Passable)
[ ] Layer Buildings: пусто
[ ] Layer Front/AlwaysFront: не перекрывает спрайт (или допустимо)
[ ] Action/TouchAction/Warp: отсутствует или используется намеренно
[ ] Мебель/декор: нет (или actor рядом, не на тайле)
[ ] Персонаж виден (в кадре viewport)
[ ] Персонаж не блокирует проход
[ ] Соседние тайлы для move также проходимы
```

---

## 3. Правила movement

### 3.1 Главное правило: `move` — одна ось

`move Actor X Y Direction` **не предназначена** для движения одновременно
по двум осям. Игра проигрывает диагональное движение криво (NPC выбирает
ось произвольно) или застревает.

**Запрещено:**

```text
move Harvey -2 -8 0
```

**Разрешённые формы:**

```text
move Harvey 0 -8 3/
move Harvey -2 0 0/
```

или одной командой через `advancedMove` (можно ставить waypoints):

```text
advancedMove Harvey false 0 -8 -2 0
```

### 3.2 `advancedMove`: формат и осторожность

Синтаксис: `advancedMove <NPC> <loop?> <dx1> <dy1> <dx2> <dy2> ...`.

- `loop?` = `true`/`false`. Для cutscene почти всегда `false`.
- Каждый сегмент — **только одна ось** не-нуля.
- Каждый сегмент: NPC проходит указанное число тайлов **в одну сторону**.
- Промежуточные тайлы должны быть проходимы — это **обязательное**
  требование, иначе NPC застрянет (см. backlog `eventHarveyMedicalCheck_Dating`).

**Плохо:** `advancedMove Harvey false 0 -14 8 0` через скалы Summit.
**Хорошо:** короткий `move` + `globalFade` + warp в нужную зону.

### 3.3 Длинные маршруты

- Длинные `move` (≥ 6 тайлов) — **признак риска**. Лучше:
  - разбить на отрезки с проверкой проходимости каждого;
  - либо `globalFade` + `warp` к нужной точке;
  - либо `advancedMove` с короткими сегментами через гарантированно
    проходимый коридор.
- На больших картах (Town, Forest, Mountain, Woods) длинные `advancedMove`
  без `viewport` теряют пару из кадра — добавлять `viewport` к точке встречи
  или сокращать сцену.

### 3.4 Movement и speak/animate

- **Не запускать `speak` до завершения движения.** SDV выполняет команды
  последовательно, но `move` ставит actor в режим «двигается» — `speak`
  может прерваться или начаться, когда actor ещё в пути.
- После `move` всегда проверять, **куда смотрит** actor. Direction
  параметра `move` — это направление **в момент финиша**. Если дальше идёт
  `speak`/`emote`/`animate`, обычно нужен явный `faceDirection`.
- В fork/quickQuestion при движении внутри ветки добавлять `pause` (хотя
  бы 300–600 мс) между `move` и `speak`.
- `move` не должен заставлять NPC проходить **через farmer**. NPC обходит
  игрока, но в узких коридорах застревает.
- `move` через мебель/двери без `doAction`/warp — не работает; пройти
  можно только через явный warp.

### 3.5 Альтернативы movement для интимных/тревожных сцен

В HarveyOverhaul часто нужен **визуальный сдвиг без полноценного move**:

- `positionOffset` — сдвиг спрайта на пиксели без смены tile (поцелуй,
  лежание, мелкая дрожь, шаг назад). См. §7.
- `animate` (`startJittering`/`stopJittering`) — дрожь на месте.
- `showFrame` — статичный кадр (сидит, скован, кашляет).
- `faceDirection` + `pause` — «остановился, посмотрел, ничего не сделал».

Для drama-сцены **«Харви замечает падение фермера»** короткие
`move Harvey 0 -2 0 true` (как в `eventHarveyMineRescue`) надёжнее, чем
длинный `advancedMove`.

### 3.6 Примеры

**Плохо:**

```text
move Harvey -2 -8 0/
```

**Хорошо (по одной оси):**

```text
move Harvey 0 -8 3/
move Harvey -2 0 0/
```

**Хорошо (waypoints через advancedMove):**

```text
advancedMove Harvey false 0 -8 -2 0
```

**Хорошо (длинный маршрут через fade):**

```text
move Harvey 0 -2 0/
pause 400/
globalFade/
warp Harvey 14 6/
faceDirection Harvey 3/
```

---

## 4. Правила faceDirection

`faceDirection Actor Direction` — где `Direction`:

| Код | Сторона |
|---|---|
| 0 | вверх (Север) |
| 1 | вправо (Восток) |
| 2 | вниз (Юг) |
| 3 | влево (Запад) |

### Правила

1. **После каждого `move`/`warp` явно ставить `faceDirection`,** если
   дальше идёт `speak`/`animate`/`emote` и нужное направление отличается
   от того, которым `move` оставил NPC.
2. **Перед `speak` направление должно быть логичным:** говорящий обычно
   смотрит на слушателя. Если Harvey справа от farmer (Harvey.x > farmer.x),
   то Harvey → 3, farmer → 1.
3. **В медицинских сценах** направление критичнее, чем в обычном диалоге:
   «слушает лёгкие / смотрит в глаза / проверяет пульс» работают только,
   если врач смотрит на пациента.
4. **После `animate`** часто нужен `stopAnimation` + `faceDirection` +
   `showFrame` (если кадр не сбрасывается).
5. Для лежащего farmer в палате `faceDirection farmer 2` — стандарт
   (см. `eventHarveyMineRescue`).
6. После пробуждения farmer часто меняется `faceDirection farmer 3`
   (повернулся к Харви) — устоявшийся паттерн.

---

## 5. Правила speak / message / dialogue

### 5.1 Когда что использовать

- `speak <NPC> "..."` — реплика **NPC**. NPC обязан существовать в локации
  события (либо setup, либо `addTemporaryActor`). После `speak Maru` без
  Maru в локации сцена ломается.
- `message "..."` — экранное окно, обычно для **мысли/реакции farmer** или
  нарратив-вставки. Farmer молчит — это часть стиля SDV; мысли farmer
  идут через `message`.
- `end dialogue Harvey "..."` — финальная реплика после `end`, отдаётся в
  обычный dialogue Harvey на остаток дня. Используется как мостик к
  топику/настроению дня.

### 5.2 Что нельзя

- Несколько `message` подряд (более 3) без действия между ними — игрок
  устаёт прокликивать. Лучше разбавлять `pause`/`emote`/`animate`.
- `speak <NPC>` для актёра, который **не на карте** в момент команды.
  Особенно при `changeLocation` — после смены локации Harvey может
  отсутствовать, нужен `warp Harvey` до первой реплики.
- `speak Harvey "..."` сразу после `move Harvey ...` без `pause` — речь
  начнётся, когда Harvey ещё идёт.

### 5.3 Портреты Harvey

Стандартные коды портретов (SDV portrait sheet, Harvey):

| Код | Портрет | Когда |
|---|---|---|
| `$0` | нейтральный | базовые реплики, инструкции |
| `$h` | счастливый | приветствие, успешное лечение, доброе слово |
| `$s` | грустный / обеспокоенный | беспокойство, тревога за farmer |
| `$u` | врач со стетоскопом / профессиональный | осмотр, медицинские инструкции |
| `$l` | влюблённый / тёплый | поздний arc, забота, dating, kiss-сцены |
| `$a` | серьёзный / строгий | приказ, запрет, «немедленно в клинику» |
| `$8` | испуганный / шок | находит farmer без сознания, паника врача |

**Прогрессия по этапам:**

- ранний onboarding (`eventHarveyFirstMeeting`, `eventHarveyCheckup`): в
  основном `$0`, `$h`, `$s`, `$u`; **минимум `$l`**;
- средний arc (Friendship 4–7 сердец): добавляются `$l` в мягких
  заботливых репликах, `$a` для запретов (`eventHarveyMineInterception`);
- dating/married (`eventHarveyMineRescueDating`, `_Dating`-варианты): `$l`
  активно используется, но в медицинских частях остаётся `$u`/`$a`.

### 5.4 Внутренние разрывы реплик

Внутри одной `speak` команды можно использовать **игровые разделители**:

- `#$b#` — разрыв на новый «пузырь» (новый кадр диалога, тот же speak).
  Стандартный паттерн HarveyOverhaul.
- `\n\n` — мягкий перенос внутри одного пузыря (для списков, советов).
- `@` — имя farmer (раскрывается в реальное имя).

`#$b#` не разрывает event-команду, остаётся внутри одной `speak`.

---

## 6. Правила emote

`emote <Actor> <ID>` — облачко над головой. ID — это **frame index** в
`emotes.png`, шаг 4. Источник: `Core/Emotes.cs`.

### 6.1 Таблица emote

| ID | Имя в `Emotes.cs` | Что | Когда уместно |
|---|---|---|---|
| 0 | Empty | пустое | редко |
| 8 | Question | `?` | непонимание farmer, удивление |
| 12 | Anger | `!!!` (красный) | строгий Харви, запрет, шок |
| 16 | Exclamation | `!` | обнаружил травму, резкое осознание |
| 20 | Heart | `♥` | **только** романтика (dating/married) |
| 24 | Sleep | `zzz` | усталость, перегрузка |
| 28 | Sad | слеза | грусть Харви, разочарование, сочувствие |
| 32 | Happy | улыбка | приветствие, успех, мягкая поддержка |
| 36 | Reject | `X` | отказ, «нет», запрет |
| 40 | Pause | `...` | размышление, заминка, неловкое молчание |
| 52 | Videogame | контроллер | редко, нерелевантно для HarveyOverhaul |
| 56 | Music | нота | редко |
| 60 | Blush | румянец | смущение, скромность farmer, мягкие моменты |

### 6.2 Семантические комплекты (`HarveyEmotes`)

Из `Core/Emotes.cs`:

- **Find injury / dirty wound:** `16` (`Exclamation`).
- **Serious / critical:** `12` (`Anger` — «!!!»).
- **Caring / recovery final:** `20` (`Heart`) — **только** Friendship ≥ 8
  сердец или dating.
- **Worry / disappointed / neglected:** `28` (`Sad`).
- **Treatment success / greeting / recovery:** `32` (`Happy`).
- **Stay in bed / refuse:** `36` (`Reject`).
- **Thinking / pause:** `40` (`Pause`).
- **Question / confused:** `8` (`Question`).

### 6.3 Правила использования

1. **Не ставить emote одновременно с резким `move`** без `pause`. Облачко
   успевает появиться поверх движения и обрезается. Стандарт: `emote` →
   `pause 300–700` → `speak`/`move`.
2. **Emote должен соответствовать портрету и реплике.** `emote Harvey 32`
   (улыбка) с репликой `"...$a"` (строгий портрет) — рассогласование.
3. **Heart (20) — только романтика.** В ранних медицинских сценах и
   сценах тревоги/страха `Heart` запрещён — он сломает тон.
4. **Для Harvey-врача чаще:** `16` (находит травму), `12` (строгий
   запрет), `28` (беспокоится), `32` (поддержка), `40` (думает).
5. **Для farmer-реакции чаще:** `8` (вопрос), `12` (страх/злость), `28`
   (грусть/усталость), `32` (благодарность), `60` (смущение от заботы).
6. **После emote — `pause 300–700`,** чтобы облачко успело прочитаться.
7. **Не дублировать подряд:** два `emote Harvey 28` без действия между
   ними не имеют смысла.

---

## 7. Правила animate / showFrame / positionOffset / stopAnimation

### 7.1 `animate`

Синтаксис: `animate <Actor> <flip> <loop> <frame_interval_ms> <frame1> <frame2> ...`

- `flip` (`true`/`false`) — горизонтальный flip спрайта.
- `loop` (`true`/`false`) — повторять или один цикл.
- frames — индексы кадров спрайта NPC.

**Правила:**

- Animate должен **соответствовать направлению** персонажа. Кадры
  ходьбы вниз/вверх/в сторону различаются; перед animate ставить
  `faceDirection`.
- Если `loop=true` (бесконечная анимация — дрожь, плач, лежание), сцена
  **обязана** иметь `stopAnimation <Actor>` где-то дальше, иначе actor
  останется заморожен после `end`.
- Если `loop=false`, animate отыграет один цикл и сбросится.
- Длительность зависит от `frame_interval * frames * loops`. Для коротких
  жестов (укол, бинт) хватает 100–800 мс; для лежания — `10000`+ с loop.

### 7.2 `showFrame`

`showFrame <Actor> <frameId>` или `showFrame <Actor> true <frameId>` —
заморозить один кадр. Полезно для «сидит», «согнулся», «осматривает».

- Известные кадры из проекта: `107` — farmer сидит/осматривает (checkup),
  `55` — Harvey накладывает повязку (E5), `117` — farmer сидит у кушетки.
- После `showFrame` смена `faceDirection` сбрасывает кадр — добавлять
  `faceDirection` **до** `showFrame`, а после `showFrame` действовать
  через animate/stopAnimation, а не faceDirection.

### 7.3 `positionOffset`

`positionOffset <Actor> <dx_px> <dy_px>` — сдвиг спрайта на пиксели
**без** смены tile. Один тайл = 64 пикселя.

**Подходит для:**

- лежания в больничной кровати: `positionOffset farmer 32 -52` после
  `warp farmer 20 5` (стандарт `eventHarveyMineRescue`);
- поцелуя (см. §7.5);
- мелкой дрожи/шатания (мелкие сдвиги ±4..±8 px);
- «полшага» назад/вперёд без полноценного move;
- мелкой коррекции, когда tile в целом верный.

**Запрещено:**

- компенсировать **неверную координату** огромным `positionOffset`
  (`60`+ px) — это маскирует баг, а не решает его. Координата должна
  быть верной сама по себе.
- использовать `positionOffset` на actor, который дальше будет ходить
  через `move` — offset «прилипает», и actor пойдёт со сдвигом.
  Перед следующим `move` обнулять: `positionOffset Actor 0 0`.

### 7.4 `stopAnimation` / `stopJittering`

- `stopAnimation <Actor>` — снять текущую loop-анимацию. Обязателен после
  любого `animate ... true true ...`, если сцена продолжается дальше.
- `startJittering` / `stopJittering` — отдельные команды для дрожи
  **farmer** (без аргумента — действует на farmer). Парные: каждой
  `startJittering` соответствует `stopJittering` (см. `eventHarveyFirstMeeting`).

### 7.5 Известные паттерны (НЕ копировать без проверки координат)

**Фермер лежит в больничной кровати:**

```text
ignoreCollisions farmer/
warp farmer 20 5/
faceDirection farmer 2/
positionOffset farmer 32 -52/
animate farmer true true 10000 4 5/
```

(или `positionOffset farmer 32 -32` для другой кровати — см.
`eventHarveyCheckHealthFarmer`).

**Фермер сидит на полу и тяжело дышит:**

```text
faceDirection farmer 2/
animate farmer false true 2000 5 4/
```

**Фермер дрожит на месте:**

```text
animate farmer true true 99999 5/
startJittering/
```

(не забыть `stopJittering` + `stopAnimation farmer`).

**Поцелуй, Harvey справа от farmer:**

```text
animate Harvey false false 4500 31/
animate farmer false false 4500 101/
positionOffset farmer 12 0/
positionOffset Harvey -12 0/
```

**Поцелуй, Harvey слева от farmer:**

```text
animate Harvey false false 4500 55/
animate farmer true false 4500 101/
positionOffset farmer -12 0/
positionOffset Harvey 12 0/
```

⚠ **Предупреждение:** все эти паттерны нельзя вставлять без:

- проверки тайлов под обоими actor (паспорт карты);
- проверки `faceDirection` обоих участников;
- проверки, что viewport показывает сцену;
- проверки, что после паттерна сцена корректно заканчивается
  (`stopAnimation`, `positionOffset Actor 0 0`, `globalFade` и т.п.).

---

## 8. Правила fork / quickQuestion / question / branching

### 8.1 Конструкции

- `quickQuestion ...#opt1#opt2#opt3(break)<ветка1>(break)<ветка2>(break)<ветка3>/` —
  inline-выбор. Все ветки **внутри одной команды**, не разрывать.
  Внутри ветки команды разделяются `\\` (двойной обратный слэш).
- `question <forkId> "#opt1#opt2"` + `fork <subEventId>/` — выбор через
  отдельные fork-подсобытия. `subEventId` — другой ключ в той же
  `Entries`-секции.
- `(break)` — разделитель веток внутри `quickQuestion`.

### 8.2 Правила выбора конструкции

| Случай | Конструкция |
|---|---|
| Короткие ветки (2–5 команд каждая) без смены локации | `quickQuestion` |
| Длинные ветки (диалог + смена локации/анимация) | `question fork` + `fork subEvent` |
| Нужно отдельно тестировать ветки | `fork` (можно вручную проигрывать subEvent) |
| Ветки уходят в разные локации | `fork` (subEvent проще читать) |

### 8.3 Тайминг и состояние

- Основной prompt `quickQuestion`/`question fork` **не должен** идти
  параллельно с `move`. Перед prompt actor должны **стоять**, иначе игрок
  видит выбор поверх ходьбы (см. story-arc audit: E2, E5, E6, E4B —
  все «нет move во время prompt» = OK).
- Если внутри fork-ветки есть `move`, ставить `pause` после move перед
  следующей репликой.
- В каждой ветке состояние сцены должно остаться валидным:
  - actors на проходимых тайлах;
  - активные `animate ... true true` корректно снимаются
    `stopAnimation`;
  - сцена заканчивается на `end` / `end dialogue` / `end position` /
    `globalFade ... end`.

### 8.4 Молчаливый farmer

Farmer в SDV в основном молчит — это часть стиля. В HarveyOverhaul:

- варианты ответа farmer оформляются короткими репликами без озвучки
  (текст `message` внутри ветки или просто реакция Harvey);
- если нужна «мысль farmer», использовать `message "..."`;
- эмоциональная реакция farmer — через `emote farmer`, не через `speak`.

### 8.5 Шаблон проверки fork

Для **каждой** ветки fork ответить на вопросы:

```text
[ ] Где Harvey на старте ветки?
[ ] Где farmer на старте ветки?
[ ] Кто куда смотрит (faceDirection)?
[ ] Активна ли анимация? Если да — есть ли stopAnimation?
[ ] Есть ли positionOffset, который нужно сбросить?
[ ] Куда ведут move/warp внутри ветки? Проходимы?
[ ] Какие topic/friendship/buff меняются?
[ ] Чем ветка заканчивается: end / end dialogue / end position / globalFade?
[ ] Viewport показывает результат ветки?
```

### 8.6 Антипаттерны fork

- `warp Harvey 35 88` в ветке, когда farmer тоже на `(35,88)` — overlap
  спрайтов (E9 backlog). Нужны соседние тайлы.
- `move` внутри ветки без `pause` перед `speak` — реплика начнётся
  раньше остановки.
- Ветка без `end` — событие зависнет.
- Длинная ветка с `changeLocation` без re-setup actors после смены.

---

## 9. Правила viewport / camera / globalFade / ambientLight

### 9.1 viewport

- `viewport X Y` — статичная камера на тайле.
- `viewport X Y true` — камера на тайле + следование за farmer (`true` =
  smooth pan / clamp).
- `viewport -1000 -1000` — стандартный «возврат камеры к игроку» в финале.

### 9.2 Правила

- Каждое событие должно иметь **осознанную** камеру: либо явный
  `viewport` на ключевую зону, либо follow-player (по умолчанию).
- На больших картах (Forest 120×120, Mountain 135×41, Town большой) без
  явного viewport сцена «уплывает» — задавать viewport в зону встречи.
- Перед `changeLocation` ставить `globalFade` + `pause 1500–2000` —
  иначе смена локации происходит «рывком».
- После смены локации viewport нужно поставить заново — старый
  привязан к старой карте.
- Не делать резкий `viewport move` во время важной реплики, если этого
  не требует drama (`viewport ... true` для slow-pan возможен).
- Если viewport ушёл с пары, после move ставить `viewport <X> <Y>` к
  новой позиции пары.

### 9.3 ambientLight

- `ambientLight R G B` — затемнение/окраска экрана.
- Типовые значения проекта:
  - `40 40 40` — кромешная тьма (обморок, потеря сознания);
  - `80 70 55` — вечер (E9);
  - `80 80 110` — гроза в клинике (E5);
  - `90 85 110` — вечер в горах (E4B);
  - `100 90 75` — вечер интерьера Hospital (E6);
  - `110 110 140` — синие сумерки у воды (E4);
  - `140 140 140` — приглушённый интерьер ночью;
  - `180 180 180` — обычный дневной интерьер (Hospital после rescue).
- После сильного затемнения (`40 40 40`) — обязательно сбрасывать
  обратно (`ambientLight 180 180 180` или штатный) до `end`.

### 9.4 Паттерны

**Стандартное окончание:**

```text
pause 1000/
globalFade/
viewport -1000 -1000/
end
```

**Смена локации:**

```text
pause 1000/
globalFade/
viewport -1000 -1000/
pause 1500/
changeLocation Hospital/
warp farmer 14 6/
warp Harvey 15 6/
faceDirection farmer 2/
faceDirection Harvey 3/
viewport 14 6/
pause 500/
fade false/
ambientLight 180 180 180/
```

**Затемнение для drama (потеря сознания):**

```text
ambientLight 40 40 40/
fade true/
message "Ты теряешь сознание..."/
pause 2000/
```

(после такого блока **обязательно** позже идёт `fade false` +
`ambientLight ...` к нормальному значению).

---

## 10. Правила warp / changeLocation / doAction

### 10.1 `warp`

`warp <Actor> X Y` — мгновенное перемещение actor на тайл `(X,Y)` в
**текущей** локации (или после `changeLocation`).

- Целевой тайл **должен** быть проходимым (или иметь `ignoreCollisions`).
- После warp обычно нужен `faceDirection`, иначе actor смотрит в сторону,
  заданную по умолчанию.
- Для лежания на кровати: `ignoreCollisions farmer/` **до** warp + warp +
  `positionOffset` + lying animate (см. §7.5).

### 10.2 `changeLocation`

`changeLocation <LocationName>` — событие переезжает в другую карту.

- После `changeLocation`:
  - все actors кроме farmer **могут отсутствовать** в новой локации;
  - `warp <Actor>` нужен заново для каждого NPC, который должен быть в сцене;
  - `viewport` сбрасывается — задавать заново;
  - `playMusic` сбрасывается на дефолт новой локации — переустанавливать
    при необходимости.
- `changeLocation` без проверки actor → `speak Harvey` сразу после =
  Harvey может быть не на карте.

### 10.3 `doAction`

`doAction X Y` — выполнить Action-тайл (как если бы farmer на него
нажал). Используется для открытия дверей, активации механизмов.

- Применять **только** на тайлах, где есть валидный Action в TMX.
- Hospital `doAction 5 9` (вход) / `doAction 10 13` — есть Action Door,
  но на Buildings → ломает движение через дверь (backlog: E2, Medical_Dating).
- Альтернатива: `playSound doorOpen` + `move`/`warp` через тайл за дверью
  (используется в исправлениях backlog).
- Не делать `doAction` на двери, через которую actor должен **войти** —
  это сложный edge case; чище через warp.

### 10.4 Паттерн открытия двери Hospital из Town

**Плохо** (Critical в backlog):

```text
doAction 5 9/
move farmer 0 -5 3/
```

**Хорошо:**

```text
playSound doorOpen/
warp farmer 10 19/
faceDirection farmer 0/
move farmer 0 -4 0/
```

---

## 11. Правила temporaryAnimatedSprite, addItem и предметов

### 11.1 `temporaryAnimatedSprite`

Сложная команда вставки спрайта на сцену. Минимальный формат:

```text
temporaryAnimatedSprite <texture> <sourceX> <sourceY> <w> <h> <duration_ms> <loops> <interval> <tileX> <tileY> <flicker> <flip> <sortPriority> <alpha> <fadeIn> <colorR> <colorG> <colorB> <mode>
```

**Правила:**

- Координаты `tileX tileY` — куда поставить спрайт. Должны быть
  логичными: плед на спине farmer, чай на столе, бутылка у лавки.
- Не ставить sprite поверх actor, если это не задумано (огромный плед
  поверх Harvey ломает кадр).
- Размер региона (`w h`) и масштаб подбирать по PNG источника
  (LooseSprites\\Cursors, Maps\\spring_town и т.п.).
- Sort priority определяет, под/над чем спрайт; для накрытия пледом
  обычно высокий sort.
- `hold_last_frame` в конце — спрайт останется после анимации.

### 11.2 Известные паттерны

**Плед (накрыть лежащего farmer):**

```text
temporaryAnimatedSprite LooseSprites\Cursors 0 1810 87 58 999999 1 999999 66 44 false false 5 0 1 0 0 0 hold_last_frame
```

⚠ Координаты `66 44` — пример; **под каждую карту/сцену пересчитывать**
относительно положения кровати/farmer.

**Машина пикапа (`eventRescueOperation`):**

```text
temporaryAnimatedSprite Maps\spring_town ... 67 12 ...
```

### 11.3 `addItem`

`addItem <id> <count>` — добавить предмет в инвентарь farmer.

Предметы-реквизиты HarveyOverhaul (vanilla items by ID):

| ID | Предмет | Семантика |
|---|---|---|
| `395` | Coffee | Утро, бодрость, забота |
| `614` | Tea | Стандартный «успокаивающий чай Харви» |
| `253` | Triple Shot Espresso | Кризис, бессонная ночь |
| `233` | Chocolate Cake | Дни рождения / награды |
| `279` | Ice Cream | Лёгкое угощение |
| `223` | Cookies | Печенье от Харви |
| `220` | Cake | Поздравления |
| `340` | Honey | Лечебное / противопростудное |
| `724` | Maple Syrup | Завтрак / восстановление |
| `245` | Energy Tonic | Восстановление энергии |
| `773` | Life Elixir | Серьёзное лечение (редко) |
| `456` | Oil of Garlic | Антимикробное (контекст травмы) |
| `201` | Pancakes | Завтрак в постель (morning checkup) |

Кастомные предметы мода (см. `items.json`):

- `HarveyMod_RecoveryMeds` — серьёзные препараты восстановления.
- `HarveyMod_PsychForm` — психологическая анкета.
- `HarveyMod_FlowerBouquet` — букет для эмоциональной терапии.
- `HarveyMod_MemoryItem` — важное воспоминание.

### 11.4 Запрещённые предметы в care/medical сценах

В сценах **заботы / лечения / тревоги** не использовать как «лечение»
или «успокоение» алкогольные предметы:

- Beer (`346`)
- Pale Ale (`303`)
- Mead (`459`)
- Wine (`348`)
- Spirits / любой алкоголь.

Харви — врач; алкоголь в его руках в медицинском контексте ломает
образ. Допустимо только в строго бытовых сценах (вечер в Saloon
вне medical-контекста).

### 11.5 Чеклист temporaryAnimatedSprite

```text
[ ] Texture указан корректно (Maps\\spring_town, LooseSprites\\Cursors)
[ ] sourceX/sourceY соответствуют реальному кадру PNG
[ ] tileX/tileY — проходимая или допустимая Buildings-зона
[ ] Спрайт не перекрывает actor нежелательно
[ ] sortPriority подобран (под/над actor)
[ ] duration / loops / interval согласованы (для hold — длинные значения)
[ ] hold_last_frame если спрайт должен остаться до end
[ ] После сцены спрайт убирается (естественно через fade или явно)
```

---

## 12. Правила для разных типов сцен

### 12.1 Простая диалоговая сцена

**Признаки:** farmer и Harvey стоят, обмениваются репликами, минимум
движения, без смены локации.

**Правила:**

- Минимум `move`/`warp`; если нужны — короткие (1–3 тайла).
- Harvey и farmer **смотрят друг на друга** (`faceDirection` после
  setup).
- Перед важной репликой: `emote` + `pause 400–800`.
- Финал: `globalFade` + `viewport -1000 -1000` + `end`.
- Подходит для: первый встречи, утренние визиты, прогулки.

**Шаблон:**

```text
continue/
<viewportX> <viewportY>/
farmer X1 Y1 D1 Harvey X2 Y2 D2/
skippable/
pause 600/
speak Harvey "..."$h/
emote farmer 32/
message "..."/
speak Harvey "..."$0/
pause 1000/
globalFade/
viewport -1000 -1000/
end
```

### 12.2 Сцена заботы / осмотра (общая)

**Признаки:** Harvey проверяет состояние farmer, может быть лёгкий
осмотр без полноценной медицины.

**Правила:**

- Harvey подходит **постепенно** (1–2 тайла), не «прыгает» в лицо.
- farmer не зажат у стены/двери; есть проход для жестов.
- Перед `animate Harvey` (осмотр) — `faceDirection Harvey` на farmer.
- Реплики Харви: мягкие, но уверенные (`$s` → `$u` → `$l` финал).
- Romance emote (`20` Heart) **нельзя** в раннем arc — только
  поддерживающие (`28`, `32`, `8`).
- В конце: `friendship Harvey +20..+50`, `addConversationTopic` для
  continuity.

### 12.3 Сцена лечения в Hospital

**Признаки:** event target `Data/Events/Hospital`, серьёзная медицина,
часто палата (койка `20 5`).

**Правила:**

- Перед setup проверить, что farmer **не** на двери (`5 9`, `10 13`)
  и **не** на лампе/тумбочке/стойке.
- Кушетка приёмной: setup `(4,6)` (рядом, не на тайле объекта).
- Палата (койка): паттерн `ignoreCollisions farmer/warp farmer 20 5/positionOffset farmer 32 -52/animate farmer true true 10000 4 5`.
- Harvey стоит **сбоку от койки** (`18,5`–`19,5`), не на ней.
- Перед «слушает лёгкие / проверяет пульс» — `faceDirection Harvey 3`
  (если Harvey справа от farmer) и `faceDirection farmer 2` (лежит на
  спине).
- После пробуждения farmer: `stopAnimation farmer` + `faceDirection
  farmer 3` (стандартный «повернулась к Харви»).
- Реплики Харви: `$u` для медицинских инструкций, `$s` для
  беспокойства, `$a` для строгости (запрет вставать), `$l` для
  заключительной мягкости.
- Если сцена включает Maru — обязательно `speak Maru "..."` после
  `warp Maru` или при условии, что Maru уже на карте (Hospital — Maru
  на работе, обычно в `(15,5)`).
- В финале — почти всегда есть `mail` (отправка письма) или
  `addConversationTopic`.

### 12.4 Сцена на Farm

**Признаки:** event target `Data/Events/Farm`, координаты Farm-зоны
(`(64,15–18)` для Standard Farm; другие layouts отличаются).

**Правила:**

- Учитывать, что farm layout **разный**: Standard, Riverland,
  Forest, Hill-top, Wilderness, Four Corners, Beach, Meadowlands. Без
  карты конкретного layout фиксированные координаты могут попасть в
  воду/обрыв.
- Безопаснее ставить сцену **около farmhouse / у входа** (`(64, 15–18)`
  — стандартная зона перед домом, обычно проходима на всех layouts).
- Избегать `warp` в зону, которая может быть водоёмом/обрывом на
  альтернативных layouts.
- Если событие требует ландшафтных объектов (дерево, мост), оно
  должно быть либо в FarmHouse-двери, либо быть проверено для всех
  layouts (что обычно невозможно — лучше перенести сцену).

### 12.5 Сцена в лесу / на улице

**Признаки:** outdoor локации (Forest, Mountain, Town, BusStop, Beach,
Desert, Woods, ArchaeologyHouse — частично).

**Правила:**

- Не ставить NPC в воду, кусты, пень, мост вне коллизий.
- Если сцена — укрытие от грозы, **визуально** должно быть оправдано:
  дерево, навес, фасад здания, скала. См. `storm-comfort-map-audit.md`.
- Проверять мосты и узкие проходы — там часто Front overlay.
- Камера: на больших картах **обязательно** `viewport <X> <Y>` к зоне
  встречи, иначе scene «уезжает».
- Если сцена в Forest — учитывать SVE-патчи (canopy shadows,
  RedBaneberry, Grandpa's Farm), которые могут сместить визуал.

### 12.6 Сцена в Mine / SkullCave / Volcano

**Правила (Mine):**

- Фиксированные координаты `(17,7)` farmer / `(17,10)` Harvey — **якорь**;
  совпадают с C# `BeginMineRescueWarp`. Использовать их во всех новых
  mine-событиях.
- Не вести NPC на y ≥ 11 (Front/обрыв).
- Не использовать `(11–12,10)` вагонетку, `(17,3)` лифт, `(23,9)` спуск.
- Mine — стабильная карта, но **SVE Load** + `OriginalMinesEntrance`
  меняют окружение → перед фиксацией координат проверять runtime export.
- Rescue / aftermath лучше переносить в Hospital (`fade` → `changeLocation
  Hospital`) — на Mine длинные сцены опасны.

**Правила (SkullCave):**

- Координаты `(5,5)` farmer / `(7,7)` Harvey — стабильны.
- Не ставить NPC на `(7,9)` — warp Desert.
- SkullCave — vanilla карта, SVE меняет только Warp.

**Правила (Volcano):**

- CP-событий в `Data/Events/Volcano*` **нет** и не должно быть без
  специального обсуждения.
- Volcano combat death может **не** триггерить major rescue (C# gap) —
  это известно, фиксится в C#, не в CP.

**Правила (MineShaft):**

- **Запрещено** патчить `Data/Events/MineShaft` или использовать
  координаты MineShaft. Карты процедурные.

### 12.7 Романтическая сцена

**Признаки:** dating/married preconditions, поцелуй, признание,
heart emote.

**Правила:**

- Дистанция зависит от arc:
  - ранний (3–8 сердец без dating): 2–4 тайла, без overlap;
  - средний (8+ сердец, не dating): 1–2 тайла, рядом;
  - dating/married: 1 тайл или поцелуй (offset).
- Поцелуй — только при проверенных координатах и face-direction (см. §7.5).
- `Heart` (20) — **только** в dating/married сценах или в сценах с
  явным романтическим контекстом (предложение, свадебная аллюзия).
- Не использовать `Heart` в сценах **тревоги / лечения / страха** без
  явного романтического перехода.
- Не делать резкий переход от медицинской тревоги к интимности в одном
  событии — это рушит характер Harvey-врача. Если переход нужен — через
  `pause` + смену portrait `$u → $l` + мягкую реплику.
- Романтика на улице/в публичном месте (Town площадь) — сдержаннее,
  чем в Hospital/HarveyRoom.

### 12.8 Сцена с паникой / слабостью farmer

**Признаки:** farmer без сознания / в страхе / в дрожи; Harvey спасает.

**Правила:**

- farmer может **пятиться** через `positionOffset farmer 0 4` или
  короткий `move farmer 0 1 2` назад. Не разворачивать спиной к Харви
  (это читается как «уходит»).
- Harvey **не наезжает** на farmer. Подход на 1–2 тайла, остановка,
  `faceDirection`, `pause`, реплика.
- Лучше показывать заботливое приближение через `pause`/`faceDirection`/
  `emote 28` (грусть, беспокойство), чем длинный движущийся `advancedMove`.
- При сильной тревоге — `startJittering` (farmer) + `animate farmer ...
  false true 2000 5 4` (тяжёлое дыхание) + `playSound thunder` / `owl` /
  `breathin` для атмосферы.
- Не запускать резкий длинный маршрут Harvey через всю карту в момент,
  когда farmer без сознания — лучше `fade` → `changeLocation Hospital`.
- Снимать `startJittering` через `stopJittering` до `end`, иначе farmer
  останется дрожать после сцены.

---

## 13. Антипаттерны (что НЕ делать)

Список плохих паттернов, выведенных из реальных backlog-проблем и
аудитов:

1. **`move Actor X Y D` с обоими ненулевыми X и Y.** Игра выберет ось
   произвольно или застрянет. Разбивать на одно-осевые или `advancedMove`.
2. **`speak <NPC>` для actor, которого нет на карте** (особенно после
   `changeLocation`). Нужно `warp <NPC>` или `addTemporaryActor` до speak.
3. **Координаты без проверки карты.** Использовать `(20,5)` Hospital без
   `ignoreCollisions` / `positionOffset` / lying — кадр сломан.
4. **Setup actor на warp tile.** Пример: `farmer 19 23` BusStop у warp;
   `warp Harvey 3 15` ArchaeologyHouse на warp Town.
5. **Setup actor внутри мебели / Buildings.** `Harvey 27 22` Town
   (лавка), `Harvey 17 26` Desert (автобус), `Gunther 6 5`
   ArchaeologyHouse — Critical блокеры.
6. **Длинный `advancedMove` через неизвестную карту** (например, через
   `(34–37, 33–38)` Summit). Заменять на `globalFade` + warp.
7. **fork без синхронизации:** `move` в одной ветке и сразу `speak` без
   `pause`. Речь начнётся в движении.
8. **`animate ... true true ...` без `stopAnimation`** — actor останется
   замороженным после `end`.
9. **`positionOffset` как замена правильной координате.** `positionOffset
   farmer 64 0` — это уже целый тайл, нужно фиксить координату, не offset.
10. **viewport не показывает сцену.** Длинный `advancedMove` на большой
    карте без `viewport` к точке встречи — игрок не видит actors.
11. **`changeLocation` без проверки actors.** После смены локации
    `speak Harvey` без `warp Harvey` — Harvey не на карте.
12. **Романтический `emote 20` (Heart) в раннем arc.** Особенно в
    тревожных/медицинских сценах. Ломает прогрессию Харви.
13. **Медицинское действие без `faceDirection`.** «Слушает лёгкие», а
    Харви смотрит в стену — несмешно и сломано.
14. **Mine-событие с фиксированными координатами** без проверки SVE
    Load / `OriginalMinesEntrance`. Особенно опасно для y ≥ 11.
15. **Несколько `message` подряд без пауз и действий.** Игрок устаёт
    прокликивать.
16. **`doAction` на двери Hospital `(5,9)` / `(10,13)`** — Buildings,
    Action работает странно. Заменять на `playSound doorOpen` + warp.
17. **Алкоголь как «лечение» / «успокоение»** в medical/care сценах —
    Beer/Wine/Mead/Pale Ale. Используем Tea (`614`), Coffee (`395`),
    Honey (`340`), кастомные `HarveyMod_RecoveryMeds`.
18. **Дублирование Event ID в нескольких файлах** (`eventHarveyFirstMeeting`
    в `events.json` и `eventsCare.json`). При правке — править оба или
    оставить один, не оба разные.
19. **`startJittering` без `stopJittering`** — farmer навсегда дрожит.
20. **Сильное затемнение (`ambientLight 40 40 40` / `fade true`) без
    возврата** до конца сцены — экран остаётся чёрным.
21. **fork-ветка без `end` / `end dialogue`** — событие зависнет.
22. **Setup farmer вне локации события.** `eventHarveyCheckup` имел
    target BusStop, а координаты Hospital — Critical блокер.
23. **`continue` vs `none` в начале** — путать. `continue/` — игрок
    появляется в текущей позиции (или в setup-позиции, заданной далее);
    `none/` — стандарт для events с явным setup всех actors.

---

## 14. Шаблоны безопасных событий

Мини-шаблоны для типовых сцен. **Все координаты в шаблонах — placeholder,
заменять под конкретную карту.**

### 14.1 Простая встреча

```text
continue/
<vpX> <vpY>/
farmer X1 Y1 2 Harvey X2 Y2 3/
skippable/
pause 600/
emote Harvey 32/
speak Harvey "Здравствуй. Как ты сегодня?$h"/
message "Нормально, спасибо..."/
speak Harvey "Я рад это слышать.$0"/
pause 800/
globalFade/
viewport -1000 -1000/
addConversationTopic topic<EventId> 5/
end
```

### 14.2 Осмотр в клинике (приёмная)

```text
continue/
4 6/
farmer 4 6 1 Harvey 4 5 3/
skippable/
pause 600/
speak Harvey "*улыбается* Проходи, садись.$h"/
move farmer 1 0 0/
pause 200/
move farmer 0 -2 2/
pause 100/
showFrame farmer true 117/
pause 1000/
faceDirection Harvey 2/
animate Harvey false true 800 22 20 21/
speak Harvey "Сейчас послушаю сердце.$u"/
pause 1500/
stopAnimation Harvey/
emote Harvey 32/
speak Harvey "Ритм в норме.$h"/
pause 800/
addConversationTopic topicAfterCheckup 5/
globalFade/
viewport -1000 -1000/
pause 500/
end position 10 17
```

### 14.3 Тревога на улице (Харви замечает farmer)

```text
continue/
<vpX> <vpY>/
farmer X1 Y1 2 Harvey 1000 1000 0/
skippable/
pause 500/
warp Harvey X2 Y2/
faceDirection Harvey 3/
viewport <vpX> <vpY>/
pause 400/
emote Harvey 16/
speak Harvey "@! Стой!$8"/
pause 300/
move Harvey 0 -2 0 true/
pause 200/
emote farmer 8/
speak Harvey "Ты на грани. Сейчас в клинику.$a#$b#Без возражений.$u"/
pause 800/
emote Harvey 28/
speak Harvey "Я рядом. Не бойся.$s"/
pause 600/
globalFade/
viewport -1000 -1000/
friendship Harvey 35/
end
```

### 14.4 Сцена с выбором (короткий fork)

```text
continue/
<vpX> <vpY>/
farmer X1 Y1 1 Harvey X2 Y2 3/
skippable/
pause 600/
speak Harvey "Хочешь чай или кофе?$h"/
quickQuestion ...#Чай#Кофе#Ничего(break)addItem 614 1\speak Harvey "Травяной чай, как ты любишь.$l"\friendship Harvey 20(break)addItem 395 1\speak Harvey "Свежесваренный.$h"\friendship Harvey 15(break)speak Harvey "Хорошо, без напитков.$s"\friendship Harvey 5/
pause 800/
globalFade/
viewport -1000 -1000/
end
```

### 14.5 Смена локации (Mine → Hospital)

```text
none/
-1000 -1000/
farmer 17 7 2 Harvey 1000 1000 0/
pause 800/
warp Harvey 17 10/
viewport 17 7 true/
pause 400/
fade false/
emote Harvey 16/
speak Harvey "@?! Что случилось?$8"/
move Harvey 0 -2 0 true/
animate Harvey false true 800 22 20 21/
speak Harvey "Пульс слабый. Я везу тебя в клинику.$a"/
pause 600/
globalFade/
viewport -1000 -1000/
pause 1500/
changeLocation Hospital/
ignoreCollisions farmer/
warp farmer 20 5/
positionOffset farmer 32 -52/
faceDirection farmer 2/
animate farmer true true 10000 4 5/
warp Harvey 18 5/
faceDirection Harvey 1/
viewport 14 6/
pause 500/
fade false/
ambientLight 180 180 180/
pause 800/
speak Harvey "Ты в безопасности.$u"/
pause 600/
stopAnimation farmer/
faceDirection farmer 3/
pause 300/
emote farmer 8/
speak Harvey "Не двигайся. Отдыхай.$l"/
pause 1000/
friendship Harvey 50/
mail mailAfter<EventId>/
globalFade/
viewport -1000 -1000/
end
```

---

## 15. Чеклист перед добавлением нового события

Перед коммитом нового события Cursor проходит весь список:

- [ ] Event ID уникален (нет дубликата в `events.json`, `eventsCare.json`,
      `eventsMineRescue.json`)
- [ ] Event ID следует [`id-naming-standard.md`](../id-naming-standard.md)
- [ ] Target (Data/Events/<Location>) **совпадает** с локацией координат
- [ ] Preconditions не конфликтуют с существующими событиями
      (особенно `PLAYER_HAS_SEEN_EVENT`, `Friendship`, `Time`, `Weather`)
- [ ] Карта проверена через [`map-passports.md`](../CheckEvent/map-passports.md)
- [ ] Координаты всех setup/warp/move/viewport безопасны (см. чеклист §2.5)
- [ ] Движение по одной оси или явный `advancedMove` (§3)
- [ ] После каждого `move`/`warp` есть `faceDirection`, если дальше speak
- [ ] Все actors существуют в локации (`warp` / `addTemporaryActor`)
- [ ] Камера показывает сцену (`viewport` к ключевой зоне)
- [ ] Нет активной loop-`animate` перед `end` без `stopAnimation`
- [ ] Все fork-ветки заканчиваются `end` и валидны (§8.5)
- [ ] `positionOffset` сброшен или ушёл через fade
- [ ] `startJittering` парный с `stopJittering`
- [ ] `ambientLight` возвращён к нормальному значению до `end`
- [ ] Финал имеет `globalFade` + `viewport -1000 -1000` + `end`/`end dialogue`/`end position`
- [ ] Тон Харви соответствует arc-этапу (см. §5.3)
- [ ] Emote не нарушает прогрессию (особенно `20` Heart) (§6)
- [ ] Предметы не из запрещённого списка (§11.4)
- [ ] JSON-валидность: `quickQuestion` не разорван, `\\` сохраняются,
      `\"` в репликах экранированы (см. `docs/cp-event-formatting.md`)
- [ ] Stylistic: команды по одной на строку, `quickQuestion`/preconditions
      одной строкой (§17 cp-event-formatting)
- [ ] Событие можно вручную запустить через `debug eventbyid <id>` или
      создать save с нужными preconditions для тестирования

---

## 16. Чеклист перед исправлением существующего события

Перед коммитом правки события Cursor проходит:

- [ ] Правится **только один** Event ID на правку
- [ ] **Условия** не меняются (Time, Weather, Friendship, GameStateQuery,
      Random, FestivalDay)
- [ ] **Тексты реплик** не меняются (если задача не «правка текстов»)
- [ ] **Тон Харви / portraits** не меняется (если задача не «тон»)
- [ ] Форматирование **минимальное**: правка только нужных строк
- [ ] Проблема подтверждена в [`events-map-fix-backlog.md`](../CheckEvent/events-map-fix-backlog.md)
      или в одном из аудитов (coordinate / story / storm / mine)
- [ ] Координата-замена подтверждена в [`map-passports.md`](../CheckEvent/map-passports.md)
- [ ] Если меняется movement — новые тайлы маршрута тоже проходимы
- [ ] Если меняется `changeLocation`/`warp` — actors после смены корректно
      позиционированы
- [ ] JSON-валидность: семантический diff показывает только изменённую
      строку (см. `docs/cp-event-formatting.md`)
- [ ] В отчёте/коммите указаны: **старые координаты**, **новые координаты**,
      **источник** (backlog, audit, map-passport)
- [ ] Не правятся события из «Исключены из проверки» в
      `events-map-audit-plan.md` без отдельной задачи
- [ ] Сцена была проиграна in-game (или назначена задача in-game проверки)

---

## 17. Как Cursor должен использовать этот документ

Перед **созданием** или **исправлением** любого CP-события Cursor:

1. **Открывает этот документ** (`docs/EventPatterns/cp-event-authoring-rules.md`).
2. **Определяет тип сцены** по §12 (диалог / осмотр / Hospital / Farm /
   улица / Mine / романтика / паника).
3. **Применяет соответствующий чеклист** из §12 + §13 (антипаттерны).
4. **Сверяет карту** через [`map-passports.md`](../CheckEvent/map-passports.md)
   и подтверждает координаты по §2.5.
5. **Проверяет backlog** [`events-map-fix-backlog.md`](../CheckEvent/events-map-fix-backlog.md)
   на предмет уже известных проблем этого события.
6. **Только потом** предлагает или вносит изменения.
7. Перед коммитом — проходит §15 (новое событие) или §16
   (исправление).

Если событие не подпадает ни под один шаблон §14 — Cursor **сообщает**
об этом пользователю и просит уточнения, а не импровизирует.

Если карта не покрыта map-паспортом — Cursor **не** угадывает
координаты; создаётся задача на TMX-экспорт.

---

## 18. Примеры из проекта

Подборка коротких фрагментов из событий, уже прошедших ручную сверку, и
из аудитов [`events-coordinate-audit.md`](../CheckEvent/events-coordinate-audit.md),
[`mine-events-map-risk-audit.md`](../CheckEvent/mine-events-map-risk-audit.md),
[`storm-comfort-map-audit.md`](../CheckEvent/storm-comfort-map-audit.md),
[`story-arc-map-audit.md`](../CheckEvent/story-arc-map-audit.md). Цель раздела —
показать, как правила §1–§13 проявляются в реальном коде HarveyOverhaul, и
зафиксировать узнаваемые анти-шаблоны как нельзя-делать-примеры.

Фрагменты сокращены до 1–3 команд; полные скрипты — в `eventsCare.json`,
`events.json`, `eventsMineRescue.json`. **Этот раздел ничего не исправляет**,
он только цитирует уже существующие постановки.

### 18.1 Хорошие паттерны

| Event ID | Локация | Фрагмент | Почему хорошо | Где можно переиспользовать |
|----------|---------|----------|---------------|----------------------------|
| `eventHarveyMineInterception` | Mine | `farmer 17 7 0 Harvey 17 10 0/` <br> `move Harvey 0 -2 0/` <br> `faceDirection farmer 2/` | Setup на якорной паре Mine `(17,7)`/`(17,10)` (§2.4); короткий move по одной оси (§3.1); явный `faceDirection farmer` перед эмоцией Харви (§4). | Любая новая mine-сцена; шаблон §14.3 (тревога на улице → fade в Hospital). |
| `eventHarveyMinorMineRescue` | Mine → Hospital | `globalFade/` <br> `changeLocation Hospital/` <br> `warp farmer 14 6/` <br> `warp Harvey 15 6/` <br> `viewport 14 6/` | Эталон смены локации (§9.4, §10.2): fade → changeLocation → pre-warp **обоих** actors → viewport на новую зону, ничего не оставлено «висеть». | Любая сцена «травма в шахте/лесу → клиника» (шаблон §14.5). |
| `eventHarveyMineRescue` | Mine → Hospital | `ignoreCollisions farmer/` <br> `warp farmer 20 5/` <br> `positionOffset farmer 32 -52/` <br> `animate farmer true true 10000 4 5/` | Единственный безопасный способ положить farmer на койку `(20,5)` (Buildings) — §2.3, §7.5: collision-bypass + пиксельный offset + lying loop. | Любая палатная сцена в `Data/Events/Hospital` (`HarveyMod_NightCrisis_*`, post-rescue cutscenes). |
| `eventHarveyMineRescue` | Mine (drama) | `ambientLight 40 40 40/` <br> `fade true/` <br> `message "Ты теряешь сознание..."/` <br> ... <br> `fade false/` | Эталон §9.4 для затемнения: чёрный экран + текст + гарантированный возврат `fade false` ниже по скрипту (анти-паттерн §13 п.20). | Все «потеря сознания / коллапс» сцены, в т.ч. `eventHarveyTreatmentCollapse`, `eventHarveyExhaustion`. |
| `eventHarveyMountainDate` | Mountain | `45 22/` <br> `farmer 41 19 3 Harvey 46 22 3/` <br> `move farmer 0 3 1/` <br> `move farmer 4 0 1/` | На большой карте Mountain задан явный `viewport (45,22)` к точке встречи (§9.2), движение разбито на **две одноосевые** `move` вместо одного диагонального (§3.1, §3.6). | Любые outdoor-сцены на Forest/Mountain/Town, где пара должна сойтись в одной точке. |
| `eventHarveyFirstWalk` | Farm | `speak Harvey "...$h"/` <br> `question fork0 "#Согласиться#Отказаться"/` <br> `fork acceptWalk/` | Длинная ветка «согласие на прогулку» вынесена в отдельное под-событие `acceptWalk` через `fork` (§8.2), а не вшита в `quickQuestion`. | Любая сцена с веткой, которая включает `changeLocation`, длинный диалог или movement по новой локации. |
| `HarveyMod_TreatmentPlanMeeting` | Hospital | `5 7/` <br> `farmer 5 5 3 Harvey 4 5 1/` <br> `showFrame farmer 107/` <br> `move Harvey 1 0 0/` | Setup в приёмной: farmer `(5,5)` / Harvey `(4,5)` — проходимые тайлы у кушетки (§2.3, §12.3), `showFrame 107` для «сидит на осмотре» (§7.2). Не задеты двери `(5,9)` / `(10,13)`. | Любая checkup-сцена в `Data/Events/Hospital` без палаты (приёмная, разговор у стола). |
| `eventHarveyStormComfortMine` | Mine → Town | `farmer 15 5 2 Harvey 1000 1000 0/` <br> `warp Harvey 18 13/` <br> `move Harvey 0 -8 3/` <br> `move Harvey -2 0 0/` | Setup на деревянной платформе у лифта (`(15,5)`) — визуально оправданное укрытие «спрятаться под землёй» (§9, §12.6). Движения Harvey — строго по одной оси, без y ≥ 11 (§2.4). | Любая storm-сцена, где нужен спуск/подъём по узкому коридору. |
| `eventHarveyMorningCheckup` | Farm (Dating) | `speak Harvey "@? Проснись, солнышко...$l"/` <br> `emote Harvey 20/` <br> `pause 800/` <br> `speak Harvey "Я принёс завтрак в постель.$h..."/` | Heart-emote (20) применён по правилам §6.2 / §6.3 п.3: dating + portrait `$l`, после emote — `pause 800` чтобы облачко прочиталось (§6.3 п.6). | Утренние dating-визиты, романтические checkup-сцены с мягким тоном. |
| `HarveyMod_BirthdayHospital_Dating` | Hospital | `playSound doorOpen/` <br> `move farmer 0 -4 0/` <br> `pause 300/` <br> `emote Harvey 20/` | Вход в клинику через `playSound doorOpen` + `move` (а не `doAction 5 9` / `10 13`) — §10.4 «хорошо». Heart (20) допустим: Friendship 2000 + Dating. | Любая сцена с входом farmer в Hospital из Town. |
| `eventHarveyStormComfortForest` | Forest | `warp Harvey 35 13/` <br> `faceDirection Harvey 3/` <br> `pause 500/` <br> `move Harvey -11 0 3/` | Warp на проходимый `(35,13)`, явный `faceDirection` **до** speed/move, длинный одноосевой `move -11` по чистому коридору y=13 (§3.1, §4). | Любой outdoor-сценарий с появлением Харви издалека и движением к farmer по прямой. |

### 18.2 Рискованные паттерны

| Event ID | Локация | Фрагмент | Почему риск | Как исправлять |
|----------|---------|----------|-------------|----------------|
| `eventHarveyCheckup` | BusStop ⚠ | `5 9/` <br> `farmer 2 5 1 Harvey 1 5 3/` | Target `Data/Events/BusStop`, а координаты и viewport `(5,9)` / `(1,5)` — это Hospital. На BusStop эти тайлы Buildings/Broken (§13 п.22). | Либо перенести событие в `Data/Events/Hospital`, либо переписать setup в зону BusStop (например, `(20,23)`/`(26,22)` как в `E1_SlipperyPath`). |
| `HarveyMod_FirstTreatment` | Hospital | `farmer 5 9 0 Harvey 4 5 2/` <br> `move farmer 0 -4 3/` | farmer setup на `(5,9)` — Action Door + Buildings (§2.3). Игрок появляется «в стене двери». | Setup farmer на `(5,10)` или `(6,10)` — проходимые тайлы у кушетки (§12.3), вход обыграть через `playSound doorOpen` (§10.4). |
| `HarveyOverhaulStory.E2_InsistentExam` | Hospital | `doAction 5 9/` <br> `playSound doorOpen/` <br> `move farmer 0 -5 3/` | `doAction (5,9)` на тайле двери — Buildings, поведение нестабильное (§10.3, §13 п.16); далее путь идёт через непроходимые `(0,-1)..(0,-5)` (`move 0 -5 3` от farmer вне карты). | Убрать `doAction`, оставить только `playSound doorOpen` + `warp farmer 10 19` + `move 0 -4 0` к кушетке (§10.4). |
| `eventHarveyMedicalCheck_Dating` | Hospital | `doAction 10 13/` <br> `advancedMove Harvey false 0 -7 6 0 0 1 5 0/` | `(10,13)` — внутренняя дверь Buildings (§2.3); `advancedMove` ведёт Harvey через `(12,13), (13,13), (14,13), (15,13), (16,12), (17,12)` — все Broken (§3.2, §3.3). | Заменить `doAction` на `playSound doorOpen`; разбить advancedMove на одноосевые `move` через проходимый коридор y=14 либо `globalFade` + `warp` к финальной точке. |
| `eventHarveyExhaustion` | Hospital | `ignoreCollisions farmer/` <br> `warp farmer 20 5/` <br> `faceDirection farmer 2/` <br> `animate farmer true true ...` | Палата `(20,5)`: `ignoreCollisions` есть, но **нет** `positionOffset 32 -52` — farmer лежит «в стандартной tile-точке», визуально «впадая в матрас». Также Harvey-сторона в этой сцене обычно ставится на `(19,5)` — Front overlay лампы тумбочки (Warning в §2.3). | Добавить `positionOffset farmer 32 -52` после warp (§7.5); Harvey ставить на `(18,5)` сбоку от койки, не на `(19,5)`. |
| `eventHarveyEmergencyCare` | Hospital | `warp farmer 14 6/` <br> `positionOffset farmer 0 10/` <br> `animate farmer false true 1000 4 5/` | `positionOffset 0 10` (10 px ≈ 1/6 тайла) используется, чтобы «опустить» спрайт farmer — это маскировка координаты, а не решение (§7.3, §13 п.9). | Либо использовать `showFrame farmer 117` («сидит у кушетки»), либо найти целочисленный тайл, на котором farmer выглядит сидящим без offset. |
| `eventHarveyTraumaExam` | Hospital | `farmer 5 6 3 Harvey 4 6 1/` <br> `animate Harvey false true 1000 22 20 21 20/` | `animate ... loop=true` для Harvey включается на первой же команде, **без** `stopAnimation Harvey` в видимой части скрипта. Анти-паттерн §13 п.8: Harvey останется «дёргать стетоскоп» после `end`. | После завершения осмотра — обязательно `stopAnimation Harvey` + `faceDirection Harvey 1` перед финальным `globalFade` (§7.1, §7.4). |
| `eventHarveyStormComfortDesert` | Desert | `warp Harvey 17 26/` <br> `move Harvey 0 -2 3/` <br> `move Harvey -2 0 3/` | `(17,26)` — Buildings + `DesertBus` action: Харви появляется **внутри спрайта автобуса** (§2.2, §13 п.5). Также narrativ «нет укрытий в пустыне» подтверждает плохой кадр. | Warp Harvey на `(18,24)` сбоку от автобуса (проходимо), либо сменить сцену на `changeLocation SandyHouse` (`(20,15)`) через fade — настоящее укрытие. |
| `eventHarveyStormComfortMountain` | Custom_AdventurerSummit | `advancedMove Harvey false 0 -14 8 0/` | 14 тайлов вверх + 8 на восток через `(34,38), (35,37), (36,35), (37,34), (37,33)` — все Broken (скалы). NPC застрянет (§3.3, §13 п.6). | Заменить весь сегмент на `globalFade` + `warp Harvey` к точке встречи `(40,28)` + `faceDirection` (как в §3.6 «длинный маршрут через fade»). |
| `eventHarveyStormComfortTown` | Town → Saloon | `advancedMove Harvey false 0 1 1 0 0 17/` | 17 тайлов на юг через большую карту Town без `viewport` — камера теряет Харви из кадра (§9.2, §13 п.10), плюс старт у Hospital `(36,56)` имеет Front overlay. | Разбить путь на 2 одноосевых сегмента с `viewport` к точке встречи, либо сразу `globalFade` + `warp Harvey 45 71` (у входа в Saloon) и продолжить сцену в интерьере. |
| `HarveyOverhaulStory.E8_QuietShelf` | ArchaeologyHouse | `warp Gunther 6 5/` <br> `advancedMove Harvey false 0 -2 4 0 0 1 5 0 0 -2 3 0 0 -3 2 0/` | `(6,5)` — Buildings + Warp→Gunther's Room (NPC в стене/в двери). advancedMove Harvey идёт через `(4,16), (4,17), (5,18)` — Broken (§13 п.5, §3.2). | Warp Gunther на `(11,9)` или `(8,9)` — проходимые тайлы зала; advancedMove Harvey разбить на короткие сегменты через коридор y=15 или использовать fade+warp. |
| `HarveyOverhaulStory.E9_LightInWindow` | Town | `farmer 35 88 0 Harvey -1000 -1000 0/` <br> ... <br> `warp Harvey 35 88/` | fork-warp Harvey приходит ровно на тайл farmer `(35,88)` — overlap спрайтов (§8.6). Сцена «у фасада клиники» визуально ломается. | Warp Harvey на соседний `(34,88)` или `(35,87)` с явным `faceDirection Harvey 1`/`2` и `viewport 35 88`. |

---

## Быстрая памятка

Самое важное на 20 строк (если читать только это):

1. **Карта раньше координат.** Map-passport — единственный источник
   истины для тайлов.
2. **`move Actor X Y D`: одно из X/Y = 0.** Иначе разбивать или
   `advancedMove`.
3. **После `move`/`warp` — `faceDirection`** перед speak/animate.
4. **`speak <NPC>` только если NPC на карте.** После `changeLocation` —
   сначала `warp`.
5. **Койка Hospital `(20,5)`** работает **только** через
   `ignoreCollisions` + `positionOffset 32 -52` + lying animate.
6. **Двери Hospital `(5,9)`/`(10,13)` — не setup-тайлы.** Заходить
   через `playSound doorOpen` + warp на `(10,19)`.
7. **Mine якорь — `(17,7)` farmer / `(17,10)` Harvey.** Не ходить
   на y ≥ 11.
8. **`Heart` (20) — только романтика.** Не в раннем medical/care.
9. **`animate ... true true ...` парный с `stopAnimation`.**
   `startJittering` парный с `stopJittering`.
10. **Большие карты — задавай `viewport` к точке встречи.**
11. **Перед `changeLocation` — `globalFade` + `pause 1500+`.**
12. **`positionOffset` не заменяет правильную координату.**
13. **fork-ветка должна заканчиваться `end`** и оставлять валидное
    состояние сцены.
14. **Алкоголь не используем как «лечение».** Tea (`614`), Honey (`340`),
    кастомные `HarveyMod_*` — да.
15. **Mine/SkullCave/Volcano: процедурные карты не патчим.**
16. **Не править несколько Event ID за раз.**
17. **Не менять условия и реплики при технических правках.**
18. **Форматирование — ручное.** `quickQuestion` остаётся одной строкой.
19. **Сильное затемнение → обязательно возврат до `end`.**
20. **Если сомнения — задача на in-game проверку, не «на глаз».**
21. **Target `Data/Events/<Loc>` обязан совпадать с локацией setup-координат.**
    Рассинхрон (`eventHarveyCheckup`: target BusStop, координаты Hospital) =
    Critical-блокер.

---

## Связанные документы

- [`docs/cp-event-formatting.md`](../cp-event-formatting.md) — JSON
  форматирование event scripts
- [`docs/CheckEvent/map-passports.md`](../CheckEvent/map-passports.md) — паспорта тайлов
- [`docs/CheckEvent/maps-and-tilesets-inventory.md`](../CheckEvent/maps-and-tilesets-inventory.md) — карты/SVE/tileset
- [`docs/CheckEvent/tileset-reuse-guide.md`](../CheckEvent/tileset-reuse-guide.md) — мебель/двери/декор
- [`docs/CheckEvent/events-coordinate-audit.md`](../CheckEvent/events-coordinate-audit.md) — координаты 31 событий
- [`docs/CheckEvent/events-map-fix-backlog.md`](../CheckEvent/events-map-fix-backlog.md) — backlog правок
- [`docs/CheckEvent/events-map-audit-plan.md`](../CheckEvent/events-map-audit-plan.md) — план аудита
- [`docs/CheckEvent/story-arc-map-audit.md`](../CheckEvent/story-arc-map-audit.md) — Story arc E1–E9
- [`docs/CheckEvent/storm-comfort-map-audit.md`](../CheckEvent/storm-comfort-map-audit.md) — storm comfort
- [`docs/CheckEvent/mine-events-map-risk-audit.md`](../CheckEvent/mine-events-map-risk-audit.md) — Mine/SkullCave/Volcano
- [`docs/id-naming-standard.md`](../id-naming-standard.md) — стандарт ID
- [`docs/harvey-relationship-tone-guide.md`](../harvey-relationship-tone-guide.md) — тон Харви по этапам
- `Core/Emotes.cs` — константы эмоций
