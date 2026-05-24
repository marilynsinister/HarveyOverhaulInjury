# Детальный разбор достижимости (по каждому событию)

Формат для каждого ID: **условия → источники → геймплей → конфликты → исправление**.

---

## InjuryCare: mine rescue

### `eventHarveyMineRescueDating`

1. **Условия:** dating/married Harvey; C# выбирает ID при Severe + наличии entry в `Data/Events/Mine`; игрок warp Mine (17,7); CP-preconditions на ключе (seen guards) **обходятся** через `startEvent`.
2. **Источники:** C# `PassOutHandler.TriggerMineRescueEvents` → `NeedsMineRescueEvent` после health≤0 в Mine; `ApplyBadlyHurtFromMinePassOut` → `buffBadlyHurt`; relationship — игрок.
3. **Геймплей:** да — боевая смерть в шахте с Харви dating/married.
4. **Конфликты:** C# пишет `eventsSeen` до конца cutscene; повтор rescue → topic без event (`IsMineRescueEventAlreadySeen`).
5. **Исправление:** перенести `eventsSeen.Add` на `end` event **или** не добавлять вручную (vanilla сама).

### `eventHarveyMineRescue`

1. **Условия:** fallback severe, если нет dating-entry; те же C# gates + Severe.
2. **Источники:** C# `ResolveSevereMineRescueEventId()` если dating-event отсутствует в content.
3. **Геймплей:** почти никогда при текущем CP (dating-entry есть) — legacy.
4. **Конфликты:** dating и legacy mutual exclusion в CP preconditions; C# не использует CP preconditions.
5. **Исправление:** оставить как fallback или удалить дублирующий скрипт.

### `eventHarveyMinorMineRescue`

1. **Условия:** C# `TryTriggerMinorMineRescue` — injury buff active, **нет** Severe, dating не требуется; warp Mine или start in-place.
2. **Источники:** `PlayerEventHandler` при входе в Mine; `LastMinorMineRescueDay` cooldown; fallback `topicHarveyMinorMineRescue`.
3. **Геймплей:** **да** при лёгкой травме + вход в шахту; **нет** при боевой смерти (severe path).
4. **Конфликты:** отделён от severe rescue; seen → skip cutscene.
5. **Исправление:** ✅ **2026-05-24** — wired в C#.

### `eventHarveyMineInterception`

1. **Условия:** SpaceCore PlayEvent; `triggersCare`: dating/married + `PLAYER_LOCATION_NAME Mine` + один из **базовых** `buffSurgicalWound`…`buffInfectedWound`.
2. **Источники:** topic `HarveyMineIntercept` — script; mail `mailHarveyMineWarning` — trigger; buffs — **C# InjuryManager** (до фаз лечения).
3. **Геймплей:** да, если зайти в Mine с активной травмой и SpaceCore; **нет**, если только фазовый бафф лечения.
4. **Конфликты:** CP trigger проверяет `buffDeepCuts`, C# после начала лечения заменяет на `HarveyMod_DeepCuts_Acute` → trigger не видит травму.
5. **Исправление:** расширить Condition `PLAYER_HAS_BUFF` фазовыми ID **или** `HasInjuryOrPhase` через custom GSQ.

### `eventHarveySkullCavePrevention`

1. **Условия:** SpaceCore; вход в SkullCave + dating (`triggerLocationReactionSkullCaveExit`); альтернативный warning-trigger **битый**.
2. **Источники:** triggersCare; fork `HarveySkullPromise` в том же Entries.
3. **Геймплей:** частично — при выходе/входе SkullCave с отношениями; warning при Mine+SkullCave одновременно невозможен.
4. **Конфликты:** `PLAYER_LOCATION_NAME Current Mine SkullCave` — логическая ошибка в Condition.
5. **Исправление:** `SkullCave` OR `Mine` в двух триггерах.

---

## Care chain (ранняя игра)

### `eventHarveyFirstMeeting`

1. **Условия:** BusStop; 06:00–26:00; `!PLAYER_HAS_MET Harvey`.
2. **Источники:** vanilla met flag; topics `topicFirstMeeting`, `topicConcernForHealth`, опционально `topicAgreedCheckup` — **script**.
3. **Геймплей:** да — первый заход на BusStop.
4. **Конфликты:** дубль key в `events.json` и `eventsCare.json` (побеждает care).
5. **Исправление:** удалить дубль из `events.json`.

### `eventHarveyCheckup`

1. **Условия:** BusStop 14:00–16:00; `topicAgreedCheckup`.
2. **Источники:** topic из fork first meeting (CP only).
3. **Геймплей:** да, если согласилась на осмотр.
4. **Конфликты:** topic снимается в конце checkup — OK.
5. **Исправление:** —

### `eventHarveyFirstVisit`

1. **Условия:** Farm 10:00–12:00; day≥3; `topicFirstMeeting`; !seen first visit.
2. **Источники:** `topicFirstMeeting` — first meeting script.
3. **Геймплей:** да при активном topic к day 3+.
4. **Конфликts:** topic 7 д — OK.
5. **Исправление:** —

### `eventHarveySecondVisit`

1. **Условия:** Farm 10:00–12:00; **day≥7**; seen first visit; **`!topicHarveyFirstVisitAgree/Neutral/Refused`**; !seen second visit.
2. **Источники:** outcome-topics (7 д) из first visit script.
3. **Геймплей:** да после истечения outcome-topic первого визита.
4. **Конфликты:** обёрточный `topicHarveyFirstVisit` (2 д) больше не в gate.
5. **Исправление:** ✅ **2026-05-23** — [01-early-farm-visit-chain.md](../harvey-relationship-visits-audit/01-early-farm-visit-chain.md).

### `eventHarveyFirstWalk`

1. **Условия:** Farm sunny 10:00–12:00; **day≥11**; seen second visit; **`!topicHarveySecondVisitAgree/Neutral/Refused`**; !seen first walk.
2. **Источники:** outcome-topics (7 д) из second visit script.
3. **Геймплей:** **да** после second visit + outcome window.
4. **Конфликты:** снято — старый `!topicHarveySecondVisit` удалён.
5. **Исправление:** ✅ **2026-05-23**.

---

## Pass-out / InjuryCare vs отключённые triggers

### `eventHarveyCheckFarmerOutsideAfter22`

1. **Условия:** Farm 22:00–02:00; `topicPassedOutInTown`; dating/married.
2. **Источники:** **C#** PassOutHandler: `WasUpTooLate` + location Town + buff sleepy + topic 2 д.
3. **Геймплей:** да — обморок в городе после 26:00, затем ночной заход на Farm.
4. **Конфликты:** BETAS trigger `eventHarveyLateNightCollapse` в Town **отключён** — параллельная ветка не работает.
5. **Исправление:** выбрать одну ветку Town vs Farm или связать topics.

### `eventHarveyMorningCheckup`

1. **Условия:** Farm 06:00–08:00; `topicHarveyMandatoryCheckup`; **Dating only** (не Married!).
2. **Источники:** topic 1 д — script after22 (dating **и** married).
3. **Геймплей:** только для dating после after22; **married игроки заблокированы**.
4. **Конфликts:** **relationship mismatch** after22 vs morning.
5. **Исправление:** `Dating Married` в precondition morning event.

### `eventHarveyCheckHealthFarmer`

1. **Условия:** Farm 10:00–12:00; `eventsSeen PlayerKilled`; Dating.
2. **Источники:** `PlayerKilled` — vanilla pass-out flag; не ставится C# mine rescue.
3. **Геймплей:** после первого vanilla exhaustion pass-out + dating.
4. **Конфликты:** InjuryCare pass-out может не дать PlayerKilled; mine death — другой pipeline.
5. **Исправление:** добавить `topicBadlyHurt` OR PlayerKilled в precondition.

### `eventHarveyLateNightCollapse`

1. **Условия:** Town 24:00–26:00 только (нет topic/mail).
2. **Источники:** random location entry; BETAS trigger (отключён) дублировал с PlayEvent.
3. **Геймплей:** редко — быть в Town в полночь с анимацией collapse.
4. **Конфликты:** C# town pass-out не запускает event; только topic.
5. **Исправление:** C# `startEvent` при `topicPassedOutInTown` **или** включить trigger.

### `eventHarveyEmergencyCare` / `eventHarveyExhaustion`

1. **Условия:** script-only в CP; C# `QueueHospitalEvent` warp Hospital → `startEvent`.
2. **Источники:** `PassOutHandler.OnPlayerWarped` — critical health ≤10 (не mine) или `WasExhausted` (не mine).
3. **Геймплей:** **да** при dating/married pass-out вне шахты; one-shot (`eventsSeen`) + fallback topic.
4. **Конфликты:** exhaustion **в шахте** — cutscene пропущен (mine pipeline).
5. **Исправление:** ✅ **2026-05-24** — `QueueHospitalEvent` wired.

### `eventHarveyTreatmentCollapse` / `eventStayInHospital`

1. **Условия:** none — ожидают `switchEvent` или manual start.
2. **Источники:** **нет** в активных файлах; C# HospitalizationManager — dialogue block.
3. **Геймплей:** **нет**.
4. **Конфликts:** дублирование intent с C# hosp hold.
5. **Исправление:** wire `eventStayInHospital` в `HandleWarpAttempt` **или** удалить orphan scripts.

---

## Hospital events

### `eventHarveyMedicalCheck`

1. **Условия:** Hospital; sunny; 14:00–18:00; hearts 1500; mail reminder Received.
2. **Источники:** mail — `triggersCare` DayStarted (dating/married); event script — items/dialogue.
3. **Геймплей:** да при полученном письме и визите в Hospital днём.
4. **Конфликты:** trigger mail без `!Received` — риск спама писем.
5. **Исправление:** `!PLAYER_HAS_MAIL Received` в trigger Condition.

### `eventHarveyTraumaExam` / `HarveyMod_FirstTreatment` / `HarveyMod_NightCrisis_*` / `HarveyMod_BirthdayHospital_*`

1. **Условия:** hearts + time (+ seen FirstTreatment для NightCrisis; Dating split; season/day для Birthday).
2. **Источники:** friendship; `topicHarveyNeedsFirstTreatment` (C#) для FirstTreatment.
3. **Геймплей:** да при прокачке отношений и визитах в Hospital.
4. **Конфликты:** FirstTreatment требует C# topic; Birthday требует `LocationName Hospital` в summer 9.
5. **Исправление:** split NightCrisis/Birthday ✅ 2026-05-23.

### `HarveyMod_TreatmentPlanMeeting`

1. **Условия:** Hospital 09:00–17:00; `topicDiagnosisComplete`; hearts 500.
2. **Источники:** C# `TryAddDiagnosisCompleteTopic` при старте eligible treatment (`TreatmentManager`).
3. **Геймплей:** **да** после начала лечения phase-eligible травмы + визит в Hospital.
4. **Конфликты:** topic снимается после meeting script.
5. **Исправление:** ✅ **2026-05-24**.

---

## Story arc `HarveyOverhaulStory.E1`–`E8`

Общая механика CD: `HarveyMod_CD_Global` + `HarveyMod_CD_E#` блокируют повтор ~2–7 д; **линейная** цепочка `seen E(n−1)`.

| ID | Ключевые gates | Геймплей | Главный риск |
|---|---|---|---|
| E1 | Wind, BusStop, 2♥ (500) | Early BusStop | Нужен ветер |
| E2 | seen E1, 3♥ (750), Hospital | Hospital visit | — |
| E3 | seen E2, 4♥ (1000), Forest Thu–Sat sunny | Forest | День недели |
| E4 | seen **E3**, 5♥ (1250), Beach evening | Beach | — |
| E5 | seen E4, 6♥ (1500), **storm**, Hospital | Storm + Hospital | Погода RNG |
| E6 | seen E5, 7♥ (1750), Hospital evening | Hospital | CD timing |
| E7 | seen **E6**, 8♥ (2000), Town sunny | Town midday | ✅ было seen E1 — исправлено |
| E8 | seen **E7**, 8♥ (2000), Sat Archaeology | Museum Sat | ✅ было seen E1 — исправлено |

---

## Romance / dates

### `eventHarveyFirstDate` / `eventHarveyMountainDate` / `eventHarveyPropose`

1. **Условия:** dating; hearts 2000/2250/2500; sunny; time windows; seasons кроме winter (date/propose).
2. **Источники:** friendship; relationship status игрока.
3. **Геймплей:** стандартная романтическая прогрессия.
4. **Конфликты:** нет с InjuryCare.
5. **Исправление:** —

### `eventHarveyRoomCheckup` / `eventHarveyRoomCheckup2`

1. **Условия:** HarveyRoom; hearts 1500 / dating + BETAS location Harvey + Random 0.2.
2. **Источники:** friendship; BETAS mod для #2.
3. **Геймплей:** #1 да; #2 только с BETAS (~20%).
4. **Конфликты:** hard dep BETAS.
5. **Исправление:** vanilla `GameStateQuery` Harvey in HarveyRoom.

---

## Storm comfort (`eventHarveyStormComfort*` ×6)

1. **Условия:** storm + `buffStressThunder` (или legacy topic) + hearts 750 + Random + location/time.
2. **Источники:** C# `StormComfortLauncher.TryDailyStormComfortRoll` (TimeEventHandler) — 1 roll/день, `buffStressThunder` или `topicHarveyStormStress`.
3. **Геймплей:** **частично** — нужен успешный daily roll + storm + вход в локацию + Random.
4. **Конфликты:** Random может не сработать; cooldown `HarveyMod_CD_StormComfort` после event.
5. **Исправление:** ✅ **2026-05-24** — C# buff gate wired.

---

## Прочее

### `eventRescueOperation`

1. **Условия:** Woods; storm; hearts 600; `topicRescueOperation`.
2. **Источники:** C# `RescueOperationLauncher` после E5_StormBeside или storm comfort event.
3. **Геймплей:** **частично** — нужен topic + storm + Woods + !seen.
4. **Конфликты:** parallel trauma arc, не E1–E8; cooldown `HarveyMod_CD_RescueOperation`.
5. **Исправление:** ✅ **2026-05-24**.

### `MyMod_*` (3 events)

1. **Не загружаются** — вне `content.json`.
2. **Исправление:** Include **или** delete file.

---

## InjuryCare: cross-cutting конфликты

| Конфликт | Стороны | Эффект |
|---|---|---|
| C# pass-out vs disabled BETAS triggers | PassOutHandler vs triggersInjury | Emergency/exhaustion **wired**; late collapse Town — только random entry |
| Severe vs minor mine path | PassOutHandler + PlayerEventHandler | Minor rescue **wired** для non-Severe injury |
| Phase buff vs CP trigger buff list | TreatmentManager vs triggersCare | Mine/Skull interception не срабатывает mid-treatment |
| `eventsSeen` pre-add | PassOutHandler line 403 | CP seen-guards для dating rescue некonsistent |
| `topicMineInjuryRescue` removed | PlayerEventHandler forced hosp | Topic живёт <1 дня если сразу hosp |
| Morning checkup Dating-only | after22 vs morning preconditions | Married блокированы |
| First walk vs second visit topic | eventsCare preconditions | First walk мёртв |
