# Тест proximity-реакций (InjuryCare)

Чеклист для ручной проверки proximity-облачка после рефакторинга (CP `harvey_proximity_injury.json`, `ProximityReactionManager`, fallback-цепочка).

**Метод проверки на 2026-05-25:** статический аудит C# + CP-ассета. In-game прогон **не выполнялся** — колонка «Результат» отражает ожидание по коду; после ручного теста замените `⏳` на `✅` / `❌`.

---

## Общие условия

| Параметр | Значение |
|----------|----------|
| Дистанция | ≤ `ProximityTiles` (по умолчанию **3** клетки), проверка ~1×/сек |
| Антиспам | 1× за локацию + кулдаун **120** игровых минут между облачками |
| Сброс per-location | при **смене локации** (`OnWarped`) флаг «уже показано здесь» сбрасывается |
| Proximity ≠ лечение | только `ShowEmoteWithText` / `showTextAboveHead`, **без** `DialogueBox`, **без** `TreatWithReaction` |
| Лечение | только **клик** по Харви → `InteractionHandler` → `TreatWithReaction` / medical pipeline |
| Быстрая проверка CP | `injury_proximity_test <situation> [tone]` — без баффов и state |
| Подготовка | перед сценарием желательно `injury_reset`; настроить сердечки с Харви через save / редактор дружбы |

**Тоны (`HarveyHelper.GetRelationshipToneWithHarvey`):**

| Сердечки | Dating/married | Тон |
|----------|----------------|-----|
| 0–3 | нет | `Low` |
| 4–7 | нет | `Mid` |
| 8+ | нет | `High` (не Romantic) |
| любые | да | `Romantic` |

**Приоритет выбора prefix (`ProximityReactionManager.ResolveProximityContext`):**

1. Осложнение (DirtyWound → WetStitches → WetBandage → …)
2. `ReadyForRecovery`
3. `ReadyForNextPhase`
4. `InTreatment`
5. `Untreated`

---

## Сценарий 1 — Low, лёгкая травма

**Сценарий:** 0–3 сердца с Харви, необработанная `buffHurt`.

**Команда:**
```
injury_reset
injury_debuff_add buffHurt
```

**Условия:**
- Сердечки с Харви: **0–3**, не dating
- Харви в той же локации, дистанция ≤ 3 клеток
- Локация ещё не показывала proximity в этом заходе (или сменить локацию)

**Ожидаемый prefix:** `Proximity_Injury_Untreated_Low`

**Примеры CP-строк:** «Вам стоит зайти в клинику.» / «Это нужно осмотреть.» / «Пожалуйста, не тяните.»

**Ожидаемое поведение:**
- Короткое нейтральное облачко (~3 с), эмоция `FindInjury`
- Обращение на «вы» (Low)
- Лечение **не** начинается
- Клик по Харви → `StartTreatment` / диалог лечения

**Быстрая проверка CP:**
```
injury_proximity_test untreated Low
```

**Результат проверки:** ⏳ Статика OK — prefix/emote/разделение proximity vs click подтверждены кодом. In-game не прогонялся.

---

## Сценарий 2 — Mid, глубокие порезы

**Сценарий:** 4–7 сердец, необработанная `buffDeepCuts`.

**Команда:**
```
injury_reset
injury_debuff_add buffDeepCuts
```

**Условия:**
- Сердечки: **4–7**, не dating
- Нет флагов `ReadyForNextPhase` / `ReadyForRecovery`
- Нет активных осложнений (иначе перехватят prefix)

**Ожидаемый prefix:** `Proximity_Injury_Untreated_Mid`

**Примеры CP-строк:** «Ты ранена? Зайди ко мне.» / «Я вижу, тебе больно.»

**Ожидаемое поведение:**
- Теплее Low, на «ты», **без** романтических ласкательных («милая», «любимая»)
- Эмоция `FindInjury` (серьёзная, но не critical)
- Лечение только по клику

**Быстрая проверка CP:**
```
injury_proximity_test untreated Mid
```

**Результат проверки:** ⏳ Статика OK — Mid-тексты в CP без романтики; `buffDeepCuts` ∈ `IsSeriousInjury` → `FindInjury`.

---

## Сценарий 3 — High без dating

**Сценарий:** 8+ сердец, **не** dating, `buffBruisedRibs`.

**Команда:**
```
injury_reset
injury_debuff_add buffBruisedRibs
```

**Условия:**
- Сердечки: **≥ 8**, `HarveyHelper.IsDatingOrMarriedToHarvey()` = **false**
- Только необработанная травма

**Ожидаемый prefix:** `Proximity_Injury_Untreated_High`

**Примеры CP-строк:** «Не геройствуй, пожалуйста.» / «Я волнуюсь. Покажись мне.»

**Ожидаемое поведение:**
- Забота и настойчивость, **без** «милая» / «любимая» / «малышка»
- Тон **High**, не Romantic (8+ без dating)
- Эмоция `FindInjury` (`buffBruisedRibs` не в critical/serious-списках proximity)

**Быстрая проверка CP:**
```
injury_proximity_test untreated High
```

**Результат проверки:** ⏳ Статика OK — `GetRelationshipToneWithHarvey` возвращает High при 8+ без dating; CP High Untreated без ласкательных.

---

## Сценарий 4 — Romantic, перелом

**Сценарий:** dating или married, `buffFracturedBone`.

**Команда:**
```
injury_reset
injury_debuff_add buffFracturedBone
```

**Условия:**
- Dating или married с Харви
- `TreatmentStarted` = false (ещё не кликали)

**Ожидаемый prefix:** `Proximity_Injury_Untreated_Romantic`

**Примеры CP-строк:** «Ко мне. Я серьёзно.» / «Не скрывай от меня боль.» / «Я вижу. И я волнуюсь.»

**Ожидаемое поведение:**
- Romantic-тон допустим (близость, тревога)
- Эмоция **`CriticalInjury`** (`buffFracturedBone` ∈ critical) — тревожнее, чем у лёгкой травмы
- Proximity **не** запускает лечение; клик → полноценное лечение

**Быстрая проверка CP:**
```
injury_proximity_test untreated Romantic
```

**Результат проверки:** ⏳ Статика OK — Romantic tone + Critical emote по коду; CP Romantic Untreated без шаблонного «ты здорова».

---

## Сценарий 5 — готовность к следующей фазе

**Сценарий:** лечение начато, выставлен `ReadyForNextPhase`.

**Команды (порядок важен):**
```
injury_reset
injury_debuff_add buffDeepCuts
```
1. **Кликнуть** Харви → начать лечение (`TreatmentStarted = true`)
2. ```
   injury_phase_ready buffDeepCuts 1
   ```
3. Сменить локацию и вернуться (сброс `_proximityReactionShown`) **или** зайти в новую комнату с Харви
4. Подойти к Харви

**Условия:**
- `ActiveDebuffs["buffDeepCuts"].ReadyForNextPhase = true`
- Нет активных осложнений с более высоким приоритетом
- Если proximity уже показывали в этой локации до `phase_ready` — нужен warp

**Ожидаемый prefix:** `Proximity_Phase_ReadyNextPhase_{tone}`

**Примеры CP-строк:** «Пора на контрольный осмотр.» / «Нужно проверить заживление.» / «Давай проверим заживление.»

**Ожидаемое поведение:**
- Реплика про **контрольный осмотр**, не «переходим к фазе 2»
- Эмоция `Thinking`
- Смена фазы — только по **клику** (`AdvancePhase`)

**Быстрая проверка CP:**
```
injury_proximity_test readyphase Mid
```

**Результат проверки:** ⏳ Статика OK — `ReadyForNextPhase` проверяется **раньше** `InTreatment`; CP Phase без wording «фаза N». ⚠️ Если подойти **до** клика по Харви, будет `Untreated`, не phase — это ожидаемо.

---

## Сценарий 6 — готовность к выздоровлению

**Сценарий:** финальный осмотр перед cured.

**Команды:**
```
injury_reset
injury_debuff_add buffDeepCuts
```
(опционально: клик → лечение, для реалистичного state)
```
injury_phase_recovery buffDeepCuts 1
```
Смена локации → подойти к Харви.

**Условия:**
- `ReadyForRecovery = true` на `buffDeepCuts`
- Нет осложнений

**Ожидаемый prefix:** `Proximity_Recovery_ReadyRecovery_{tone}`

**Примеры CP-строк:** «Нужен финальный осмотр.» / «Давай проверим результат.» / «Выздоровление нужно подтвердить.»

**Ожидаемое поведение:**
- Про **финальный осмотр**, **не** «ты здорова» / «полностью выздоровела»
- Эмоция `Thinking`
- Полное выздоровление — только по клику (`CompleteRecovery`)

**Быстрая проверка CP:**
```
injury_proximity_test recovery High
```

**Результат проверки:** ⏳ Статика OK — `ReadyForRecovery` имеет наивысший приоритет среди phase-флагов; CP Recovery без «ты здорова». ⚠️ При Romantic в CP есть «милая» в одной строке — это допустимо для dating; сценарий запрещает только преждевременное «здорова».

---

## Сценарий 7 — WetBandage

**Сценарий:** только осложнение «мокрая повязка».

**Команда:**
```
injury_reset
injury_debuff_add HarveyMod_WetBandage
```

**Условия:**
- Бафф `HarveyMod_WetBandage` активен (`InjuryBuffs.WetBandage`)
- Нет более приоритетного осложнения (DirtyWound, WetStitches)

**Ожидаемый prefix:** `Proximity_Complication_WetBandage_{tone}`

**Примеры CP-строк:** «Повязка промокла?» / «Её нужно заменить.» / «Так нельзя ходить.»

**Ожидаемое поведение:**
- Текст про **мокрую повязку**
- Эмоция `WorriedAboutPatient`
- Осложнение перехватывает prefix даже при наличии основной травмы
- Лечение осложнения — по клику

**Быстрая проверка CP:**
```
injury_proximity_test wetbandage Mid
```

**Результат проверки:** ⏳ Статика OK — `injury_debuff_add HarveyMod_WetBandage` совпадает с `InjuryBuffs.WetBandage`; CP-ключи WetBandage на всех тонах.

---

## Сценарий 8 — DirtyWound

**Сценарий:** загрязнённая рана.

**Команда:**
```
injury_reset
injury_debuff_add HarveyMod_DirtyWound
```

**Условия:**
- Бафф `HarveyMod_DirtyWound` активен
- DirtyWound — **наивысший** приоритет среди осложнений

**Ожидаемый prefix:** `Proximity_Complication_DirtyWound_{tone}`

**Примеры CP-строк:** «Рана загрязнена?» / «Нужно промыть немедленно.» / «Не трогай её руками.»

**Ожидаемое поведение:**
- Текст про **загрязнённую рану**
- Эмоция `DirtyWound` (`!`)
- Proximity не лечит

**Быстрая проверка CP:**
```
injury_proximity_test dirtywound Low
```

**Результат проверки:** ⏳ Статика OK — ID и MapComplicationKey → DirtyWound; CP совпадает.

---

## Сценарий 9 — Mine rescue + Severe (приоритет госпитализации)

**Сценарий:** `topicMineInjuryRescue` + Severe-травма → принудительная госпитализация **вместо** обычного proximity-облачка.

**Команды:**
```
injury_reset
injury_debuff_add buffBadlyHurt
injury_foreign_topic_add topicMineInjuryRescue 2
```

**Условия:**
- `ModConfig.ForceHospitalization = true` (по умолчанию)
- Активен conversation topic `topicMineInjuryRescue`
- Severe-бафф (`buffBadlyHurt` ∈ `InjurySets.Severe`)
- Харви рядом, игрок **не** уже госпитализирован

**Ожидаемый prefix:** **нет** proximity-prefix — ветка `canForcedHosp` в `CheckHarveyProximity`

**Ожидаемое поведение:**
- `StartForcedHospitalizationWithExplanation(..., "mine_rescue")`
- Топик `topicMineInjuryRescue` снимается
- **Не** вызывается `ShowProximityDiscovery` / CP proximity-строки
- `_proximityReactionShown = true` (повтор не спамит)

**Альтернатива (отложенный rescue):** `injury_debug_mine_rescue` — срабатывает на следующий `DayStarted`, не для мгновенного proximity-теста.

**Результат проверки:** ⏳ Статика OK — в `CheckHarveyProximity` блок `canForcedHosp` идёт **до** `ShowProximityDiscovery` и делает `return`. In-game: проверить отсутствие короткого CP-облачка и наличие hospitalization flow.

---

## Сценарий 10 — нет травм

**Сценарий:** здоровый игрок подходит к Харви.

**Команда:**
```
injury_reset
```

**Условия:**
- `ActiveDebuffs` пуст
- `ActiveComplications` пуст
- Нет `topicMineInjuryRescue` + Severe (иначе сценарий 9)
- Нет других injury-топиков без state (только топики не триггерят proximity сами по себе)

**Ожидаемый prefix:** —

**Ожидаемое поведение:**
- **Никакого** proximity-облачка от InjuryCare
- Ранний выход: `!hasAnyInState` в `CheckHarveyProximity`
- Обычный ванильный/CP-диалог по клику — не затронут

**Результат проверки:** ⏳ Статика OK — при пустом state `CheckHarveyProximity` возвращается до сбора injuries.

---

## Fallback-цепочка (регрессия)

Если exact-key отсутствует в CP, `BuildPrefixCandidates` + `PickRandomProximityLineByPrefixes`:

**Пример:** `Proximity_Complication_WetBandage_Romantic` (если ключа нет)
1. `…_WetBandage_Romantic`
2. `…_WetBandage_High`
3. `…_Generic_Romantic` / `…_Generic_High`
4. `…_Multiple_Romantic` / `…_Multiple_High` *(alias под CP)*
5. `Proximity_Injury_Untreated_{tone}`
6. `"Покажись мне в клинике."`

**Warn в SMAPI:** только если **ни один** prefix не дал строк; не каждый тик.

**Проверка:**
```
injury_proximity_test wetbandage Romantic
```
(при полном CP все ключи WetBandage_Romantic есть — должен матчиться сразу на шаге 1)

**Результат проверки:** ⏳ Статика OK — логика в `ProximityReactionManager.BuildPrefixCandidates` и `DialogueManager.PickRandomProximityLineByPrefixes`.

---

## Сводная таблица

| # | Тон | Prefix (primary) | Proximity лечит? | Статика |
|---|-----|------------------|------------------|---------|
| 1 | Low | `Proximity_Injury_Untreated_Low` | Нет | ⏳ OK |
| 2 | Mid | `Proximity_Injury_Untreated_Mid` | Нет | ⏳ OK |
| 3 | High | `Proximity_Injury_Untreated_High` | Нет | ⏳ OK |
| 4 | Romantic | `Proximity_Injury_Untreated_Romantic` | Нет | ⏳ OK |
| 5 | * | `Proximity_Phase_ReadyNextPhase_*` | Нет | ⏳ OK |
| 6 | * | `Proximity_Recovery_ReadyRecovery_*` | Нет | ⏳ OK |
| 7 | * | `Proximity_Complication_WetBandage_*` | Нет | ⏳ OK |
| 8 | * | `Proximity_Complication_DirtyWound_*` | Нет | ⏳ OK |
| 9 | — | *(forced hosp, не proximity)* | Госпитализация | ⏳ OK |
| 10 | — | *(нет)* | — | ⏳ OK |

---

## Логи для отладки

При успешном proximity в SMAPI (Debug):
```
[Proximity] Показ облачка: локация=..., дистанция=...
[Proximity] Облачко: emote=..., prefixes=[...], text='...'
```

При mine rescue (Warn):
```
⚠️ Харви обнаружил раны после обморока в шахте → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ
```

---

## Чеклист после in-game прогона

- [ ] 1 — Low Untreated
- [ ] 2 — Mid Untreated
- [ ] 3 — High без dating
- [ ] 4 — Romantic + critical emote
- [ ] 5 — ReadyNextPhase (после клика)
- [ ] 6 — ReadyRecovery
- [ ] 7 — WetBandage
- [ ] 8 — DirtyWound
- [ ] 9 — Mine rescue > proximity
- [ ] 10 — нет травм → тишина

После прогона обновите «Результат проверки» в каждом сценарии и отметьте чеклист.
