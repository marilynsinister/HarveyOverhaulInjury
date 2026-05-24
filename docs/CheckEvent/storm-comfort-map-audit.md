# Аудит storm comfort: укрытия от грозы

Проверка **визуального смысла** постановки (не только проходимости) для CP-событий `eventHarveyStormComfort*` на outdoor-картах.  
**Farm исключён** (проверен вручную). **События не изменялись.**

**Источники:** CP `events.json`; TMX-паспорта [`map-passports.md`](map-passports.md); координатный аудит [`events-coordinate-audit.md`](events-coordinate-audit.md); шахтный риск [`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md).

**Метод:** сопоставление setup/warp/move/viewport с объектами TMX (двери, warp, Buildings, Front), плюс narrativ fit — «игрок ищет укрытие, Харви ведёт в безопасное место».

---

## Общие условия random-event

Все пять событий используют один шаблон preconditions:

| Условие | Значение |
|---------|----------|
| Погода | `Weather storm` |
| Дружба | `Friendship Harvey 750` |
| Random | Forest **0.55**, Mountain **0.4**, Town **0.3**, Desert **0.3**, Mine **0.8** |
| Cooldown | `!HarveyMod_CD_StormComfort` |
| Триггер страха | `buffStressThunder` **или** `topicHarveyStormStress` |
| Фестиваль | `!FestivalDay` |

**Локация entry:** событие срабатывает при **входе** в локацию, если условия выполнены. Setup-строка **перезаписывает** позицию farmer (не зависит от warp-точки входа).

| Event ID | Patch target | Соответствие «где играешь → где event» |
|----------|--------------|----------------------------------------|
| `eventHarveyStormComfortForest` | `Data/Events/Forest` | OK |
| `eventHarveyStormComfortMountain` | `Data/Events/Custom_AdventurerSummit` (+ act 2 `Mountain`) | OK — игрок на Summit, не vanilla Mountain |
| `eventHarveyStormComfortTown` | `Data/Events/Town` (+ act 2 `Saloon`) | OK |
| `eventHarveyStormComfortDesert` | `Data/Events/Desert` | OK |
| `eventHarveyStormComfortMine` | `Data/Events/Mine` (+ finale `Town`) | OK |

---

## Сводная оценка

| Event | Визуальный смысл укрытия | Проходимость | Итог |
|-------|--------------------------|--------------|------|
| Forest | **Хорошо** (лес + ирония про молнии) | OK | Оставить, усилить привязку к дереву опционально |
| Mountain | **Отлично** (опасная высота → склон) | Warning (advancedMove) | Смысл сильный; проверить path in-game |
| Town | **Хорошо** (улица → Saloon) | Warning | Слабый старт, сильный финал в интерьере |
| Desert | **Слабо / противоречие** | Broken (Harvey warp) | Нарратив «нет укрытий» OK, постановка плохая |
| Mine | **Отлично** (шахтный навес → скамейка) | OK | Сильная двухактная логика |

---

## eventHarveyStormComfortForest / Forest

### Текущая постановка

- **farmer:** setup `(23, 13)`, facing 1 → 2; animate crouch/fear; без move
- **Harvey:** off-screen → `warp (35, 13)`; `move -11 0` → `(24, 13)`; facing 3
- **camera:** default (viewport не задаётся; `rain/` music)
- **movement:** один горизонтальный проход Harvey с востока; без смены локации; `globalFade` в конце

**Условие entry:** Forest + storm + random 0.55 — **логично** (лес как «естественное укрытие»).

### Оценка

- **Проходимость:** OK — `(23,13)`, `(35,13)`, путь по y=13 проходим ([`events-coordinate-audit.md`](events-coordinate-audit.md)).
- **Визуальный смысл укрытия:** **Хорошо, с намеренной иронией.**  
  - Зона x≈23–35, y=13 — **центральная лесная тропа** (outdoors grass/dirt), не открытое поле Town/Desert.  
  - Реплики: «деревья притягивают молнии» vs «искала защиту под деревьями» — **смысл сцены совпадает** с локацией Forest.  
  - На TMX **нет** Buildings/Front на `(23,13)` — персонаж на тропе, не в стене/воде.  
  - **Слабость:** на точных тайлах нет привязки к object «дерево» (passport: объекты рядом — пусто); визуально canopy зависит от **seasonal tree tiles** на Back — нужен in-game скрин.
- **Риски:**
  - Камера без `viewport` — на большой Forest (120×120) может показать «пустую» тропу без явного кронового навеса.
  - `(35,13)` spawn Harvey — восточный край сцены; если там меньше деревьев на Back, подход выглядит как «бег по аллее».

**Объекты рядом (использовать в кадре):**

| Тип | Где на карте | Расстояние от сцены |
|-----|--------------|---------------------|
| Деревья (Back tile) | вся зона y=10–15 | на тайле / рядом |
| Forest.29 (пень/заготовка) | `(79–83, 10–14)` | далеко — не в кадре |
| LeahHouse door | `(104, 32)` | далеко |
| Marnie ranch | `(90, 15)` | ~70 тайлов восток |

### Лучшие альтернативы

| Zone | Coordinates/range | Почему лучше | Риски |
|------|-------------------|--------------|-------|
| Кластер Forest.29 | `(79, 11)`–`(82, 14)` | Явные **пни/лесной декор** на Buildings; ощущение «под деревьями» | Дальше от центра карты; Harvey spawn с `(88, 13)` |
| E3-аптекарская зона | `(48, 14)` / `(50, 13)` | Уже story-зона мода; можно «укрытие у крыльца» | Конфликт визуала с другими E3-сценами |
| У Wizard House | `(9, 21)`–`(11, 23)` | **Стена дома** слева = реальное укрытие от дождя | Узко; door `(9, 20)` |

---

## eventHarveyStormComfortMountain / Custom_AdventurerSummit → Mountain

### Текущая постановка

- **farmer:** setup `(41, 28)` (script: `41 28` в ключе, facing 1); animate fear; `advancedMove` к `(32, 28)`
- **Harvey:** `warp (32, 42)` → `advancedMove 0 -14 8 0` → `(40, 28)` area; затем увод вниз
- **camera:** act 1 — default; act 2 после fade — `viewport (76, 15)` на Mountain
- **movement:** длинные `advancedMove` на Summit; fade → `changeLocation Mountain`; warp farmer `(79, 1)`, Harvey `(79, 0)`; ещё `advancedMove` к `(84, 7)` / `(83, 8)`

**Условие entry:** `Custom_AdventurerSummit` + storm — **логично** (игрок на SVE-площадке у шахты/гильдии, не vanilla Mountain).

### Оценка

- **Проходимость:** Warning — эвристика advancedMove через `(34–37, 33–38)` = Broken tiles; финалы `(84,7)`, `(83,8)` — Warning ([`events-coordinate-audit.md`](events-coordinate-audit.md)).
- **Визуальный смысл укрытия:** **Отлично по narrativ, средне по финальной точке.**  
  - **Act 1 `(41, 28)`:** юго-восток Summit — зона **возвышенности** (`AdventurerSummit.2–5` messages на `(49–55, 27–31)` — скальный декор). Игрок на **опасной высоте** во время грозы — **идеально** для реплик про молнию.  
  - **Harvey `(32, 42)`:** южный **warp-path к Mountain** `(31–33, 43)` — логичное «появление с тропы снизу».  
  - **Act 2 `(79, 1)` Mountain:** SVE **warp-стык Summit↔Mountain**; `viewport (76, 15)` смотрит на **склон/озеро вниз** — «укрытие у склона», реплика «гром мягче» **оправдана**.  
  - **Слабость:** финальные тайлы `(84, 7)` / `(83, 8)` — **открытый склон**, не навес; персонажи стоят на траве у края, не под крышей (но после эвакуации с высоты это приемлемо).
- **Риски:**
  - `(79, 0)` Harvey — **верхний край** карты, warp-tiles на y=-1; визуально «у обрыва».
  - advancedMove может застрять / идти через скалы — in-game обязателен.
  - `OriginalMinesEntrance` меняет связку карт — проверить на save с SVE.

**Объекты рядом:**

| Тип | Координаты | Роль в сцене |
|-----|------------|--------------|
| Warp Mine | Summit `(19, 14)` | «шахта рядом» — контекст |
| Adventure Guild door | `(32, 21)` | **реальное укрытие** (не используется) |
| Скальный декор | `(49–55, 27–31)` | визуал высоты act 1 |
| Озеро/низина | Mountain viewport `(76, 15)` | контраст «спустились вниз» |
| Перила E4B | Mountain `(44, 21)` | альтернатива для «склон» |

### Лучшие альтернативы

| Zone | Coordinates/range | Почему лучше | Риски |
|------|-------------------|--------------|-------|
| У Guild door | Summit `(32, 21)`–`(33, 22)` | **Крыша гильдии** = укрытие; Harvey из `(32, 42)` идёт на север | Нужен короткий move, не advancedMove через скалы |
| У Mine warp Summit | `(18, 14)`–`(20, 16)` | Навес **входа шахты** (как Mine storm); единая visual language | Близко к warp-тайлу `(19, 14)` |
| Mountain Maru bridge | `(44, 21)` | **Перила/мост** — укрытие от ветра, уже E4B-зона | Другой narrativ (не «с высоты») |

---

## eventHarveyStormComfortTown / Town → Saloon

### Текущая постановка

- **farmer:** setup `(39, 73)`, facing 2; animate fear; `advancedMove 6 0 0 -2` → `(45, 71)` area
- **Harvey:** `warp (36, 56)` — **дверь Hospital**; `advancedMove 0 1 1 0 0 17` (бег к югу); затем `7 0 0 -2` к farmer
- **camera:** act 1 — default; act 2 Saloon — `viewport (14, 23)`
- **movement:** погоня по югу Town → fade → Saloon; farmer/Harvey `(14, 23)` / `(13, 23)`; move к `(18, 21)` / `(13, 21)`; Gus `(13, 18)`

**Условие entry:** Town + storm + random 0.3 — **логично**.

### Оценка

- **Проходимость:** Warning — advancedMove эвристика; warp Hospital `(36, 56)` Front overlay; Saloon `(13, 23)` Front ([`events-coordinate-audit.md`](events-coordinate-audit.md)).
- **Визуальный смысл укрытия:** **Хорошо в целом; слабый act 1, сильный act 2.**  
  - **`(39, 73)` farmer:** **южная Town**, проходимая площадь; рядом warp на **Shearwater Bridge** `(119, 72–76)` — открытое пространство у **моста**, не под навесом. Визуально: «стою на улице под дождём» — **OK для страха**, **слабо как укрытие**.  
  - **Harvey `(36, 56)`:** spawn у **Hospital exam door** `(36, 55)` — **отличный** narrativ («врач выбегает из клиники»).  
  - **Saloon act 2:** интерьер, реплика «**крыша крепкая**» — **лучший payoff** среди outdoor-веток; бар `(13, 18–23)`, Gus — атмосфера **общественного убежища**.  
  - Реплика «к салуну к Эмили» — визуально ведёт к `(45, 70)` door Saloon (не используется как warp-цель, но **смысл верный**).
- **Риски:**
  - Farmer `(39, 73)` — **не у стены/навеса**; открытый юг.
  - advancedMove Harvey «0 17» на юг — 17 тайлов через карту; камера может **терять** пару.
  - Saloon TMX — **vanilla**; SVE `.tbin` может чуть сдвинуть бар.
  - CC vs `Town_Joja` — другая планировка юга.

**Объекты рядом:**

| Тип | Координаты | Роль |
|-----|------------|------|
| Hospital door | `(36, 55–56)` | spawn Harvey — **вход/укрытие** |
| Saloon door | `(45, 70)` | цель «укрытие» narrativ |
| Bridge warp | `(119, 72–76)` | открытое пространство act 1 |
| Trailer / Josh area | `(57–72, 63–68)` | жилые фасады — возможная «стена» |

### Лучшие альтернативы

| Zone | Coordinates/range | Почему лучше | Риски |
|------|-------------------|--------------|-------|
| У Hospital | `(36, 58)`–`(38, 60)` | Farmer у **фасада клиники** — сразу «безопасное место»; Harvey из `(36, 56)` | Короче chase; меньше драмы |
| У Saloon door | `(44, 71)`–`(46, 72)` | Старт **под навесом** салoon; короткий вход | Может сработать до advancedMove |
| E2B-лавка | `(28, 67)` | «**Тень у лавки/дерева**» — уже проверенная story-зона | Другой story-контекст |

---

## eventHarveyStormComfortDesert / Desert

### Текущая постановка

- **farmer:** setup `(15, 23)`, facing 2; animate fear
- **Harvey:** `warp (17, 26)` — **тайл автобуса**; `move 0 -2`, `-2 0` → `(15, 24)`
- **camera:** default
- **movement:** короткий подход Harvey; без смены локации; fade

**Условие entry:** Desert + storm — **логично** (редкий random 0.3).

### Оценка

- **Проходимость:** **Broken** — `(17, 26)` Buildings + `DesertBus` object ([`events-coordinate-audit.md`](events-coordinate-audit.md), [`map-passports.md`](map-passports.md)).
- **Визуальный смысл укрытия:** **Слабо / намеренное противоречие.**  
  - **`(15, 23)` farmer:** **открытый песок** у северной части карты, bus route — **нет навеса**, **нет стены**. Реплика Harvey «**нет укрытий**» — **narrativ OK**, визуал **подтверждает проблему**.  
  - **Harvey `(17, 26)`:** spawn **на/внутри спрайта автобуса** (Buildings) — **ломает** и проходимость, и кадр (NPC в металле кузова).  
  - **Нет** перехода в SandyHouse `(20, 14)` — упущенный **реальный shelter**.  
  - Камера default — пустыня 60×156, `(15, 23)` — **мало атмосферных объектов** в кадре (только sand + bus).
- **Риски:**
  - Harvey в стене автобуса — **High** visual/technical.
  - Игрок на открытом sand без props — **скучный кадр**.
  - `NPCWarp` bus `(18, 26)` — риск случайного warp во время сцены.

**Объекты рядом:**

| Тип | Координаты | Роль |
|-----|------------|------|
| DesertBus | `(18, 27)` Back action | **автобус** — частичный навес, если spawn **рядом** |
| SandyHouse door | `(20, 14)` | **лучшее укрытие** на карте |
| Skull Cave warp | `(44, 57)` | далеко |

### Лучшие альтернативы

| Zone | Coordinates/range | Почему лучше | Риски |
|------|-------------------|--------------|-------|
| У автобуса (сбоку) | farmer `(16, 24)`; Harvey warp `(18, 24)` | **Рядом с кузовом**, не на Buildings; narrativ «у автобуса» | `(18, 27)` action tile |
| У Sandy Shop | `(20, 15)`–`(22, 16)` | **Крыша магазина** — Harvey «нашёл, веду к укрытию» | Нужен move/fade в дом или у фасада |
| Под скалой (south) | `(5, 47)`–`(14, 53)` | Узкая **open area #2** у скального декора (passport) | Далеко от bus; другой контекст |

---

## eventHarveyStormComfortMine / Mine → Town

### Текущая постановка

- **farmer:** setup `(15, 5)`, facing 2; animate fear
- **Harvey:** `warp (18, 13)` → `move 0 -8` → `(18, 5)` → `-2 0` → `(16, 5)`; затем `0 2` → `(16, 7)`
- **camera:** act 1 — default; act 2 Town — `viewport (72, 22)`
- **movement:** act 1 короткий move на Mine; fade → Town `(72, 22)` / `(73, 22)` — реплика «**сядь на скамейку**»

**Условие entry:** Mine + storm + random **0.8** (самый частый outdoor storm) — **логично** (шахта = «спрятаться под землёй»).

### Оценка

- **Проходимость:** OK на Mine; Town finale OK ([`events-coordinate-audit.md`](events-coordinate-audit.md)).
- **Визуальный смысл укрытия:** **Отлично (act 1), хорошо с оговоркой (act 2).**  
  - **`(15, 5)` farmer:** **деревянная платформа входа шахты** под зданием лифта `(17, 3)` — **частичный навес**, narrativ «**спрятаться под землёй от грома**» — **сильнейший** visual match среди outdoor-сцен.  
  - **Harvey `(18, 13)`:** подъём с **южного выхода** / warp `(18, 14)→Summit` — «вытащил наверх из опасного подземелья» — **логично**.  
  - **Move на y=5–7:** коридор x=15–18 — **настил у лифта**, не обрыв (см. [`Mine_event_placement_analysis.md`](../../tmpMap/Mine_event_placement_analysis.md)).  
  - **Act 2 `(72, 22)` Town:** реплика про **скамейку**; TMX passport **не фиксирует** object bench на `(72, 22)` — тайл проходим, Landscape back — **needs in-game**: возможно трава/площадь **без** скамейки → **visual mismatch** текста.  
  - Harvey «клиника — убежище» — thematic OK на любом outdoor finale.
- **Риски:**
  - Harvey path `(18, 13)→(18, 5)` — у **южного края** и warp Summit; TMX OK, но Front на y≥11 — path не заходит туда.
  - **Скамейка** на `(72, 22)` не подтверждена TMX — **Medium** narrativ risk.
  - Mine random 0.8 — часто; постановка должна быть безупречной.

**Объекты рядom:**

| Тип | Координаты | Роль |
|-----|------------|------|
| MineElevator building | `(17, 3)` | **навес/козырёк** над farmer |
| Warp Summit | `(18, 14)` | exit/path Harvey |
| Wooden platform | x=15–21, y=4–10 | безопасный настил |
| Town playground area | `(72, 16)`–`(75, 20)` vanilla | **скамейка** вероятно **рядом**, не на `(72, 22)` |

### Лучшие альтернативы

| Zone | Coordinates/range | Почему лучше | Риски |
|------|-------------------|--------------|-------|
| Mine якорь rescue | `(17, 7)` / Harvey `(17, 10)` | Единый **visual language** с mine rescue; тот же навес | Короче сцена |
| Town у Clinic | `(35, 88)`–`(36, 90)` | E9-зона у **клиники** — совпадает с репликой «иди в клинику» | Без скамейки |
| Town playground bench | `(72, 18)`–`(74, 19)` | Ближе к **вероятной скамейке** vanilla playground | Нужен runtime export |

---

## Камера (сводка)

| Event | viewport | Атмосферная зона? |
|-------|----------|-------------------|
| Forest | нет | Зависит от follow-player; может быть «пустая тропа» |
| Mountain act 1 | нет | Риск — большой Summit |
| Mountain act 2 | `(76, 15)` | **Да** — склон, озеро, контраст после высоты |
| Town act 1 | нет | Юг Town + chase — может терять центр |
| Town act 2 Saloon | `(14, 23)` | **Да** — бар, интерьер |
| Desert | нет | Пустыня + sand — слабая атмосфера |
| Mine act 1 | нет | Маленькая карта 77×20 — **обычно OK** |
| Mine act 2 Town | `(72, 22)` | OK если есть playground; иначе generic grass |

**Рекомендация (для будущих правок, не здесь):** act 1 Forest/Desert/Mine — `viewport` на пару `(farmer)` или `(17, 7)` Mine.

---

## In-game чеклист (визуал)

1. **Forest `(23,13)`** — видны ли кроны/тени деревьев на farmer?
2. **Summit `(41,28)`** — читается ли «опасная высота»?
3. **Mountain `(79,1)` после fade** — склон/озеро в viewport `(76,15)`?
4. **Town `(39,73)`** — открытое небо vs Saloon interior payoff?
5. **Desert `(17,26)`** — Harvey **не** внутри спрайта bus?
6. **Mine `(15,5)`** — навес лифта над головой?
7. **Town `(72,22)`** — есть ли **скамейка** в кадре при реплике?

---

## Связанные документы

- [`events-coordinate-audit.md`](events-coordinate-audit.md) — проходимость и move
- [`mine-events-map-risk-audit.md`](mine-events-map-risk-audit.md) — Mine storm + warp Summit
- [`map-passports.md`](map-passports.md) — объекты и зоны
- [`events-map-audit-plan.md`](events-map-audit-plan.md) — общий план (Farm excluded)
