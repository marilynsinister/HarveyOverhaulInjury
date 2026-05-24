# Ручные тест-сценарии: topics, mail, тон Харви

Дата: 2026-05-23  
Мод: **HarveyOverhaulInjury** (C#) + **HarveyOverhaul [CP]**  
Цель: проверить цепочки «травма → разговор → лечение → cured», письма C# и тон Харви по отношениям.

**Код и JSON в этом документе не менялись** — только инструкции для тестера.

---

## Общая подготовка

| Что | Как |
|-----|-----|
| Сборка | Актуальные версии C# + CP в `Mods/` |
| SMAPI | Консоль открыта (`\` или настроенная клавиша) |
| Чистое сохранение | Для первого прохода — новый слот или `injury_reset` перед сценарием |
| Письма | В `config.json` мода: `SendLetters: true` (иначе mail-сценарии не проверить) |
| Харви рядом | Клиника (`Hospital`) или локация, где он доступен для клика |
| Аудит контента | `injury_audit_content` — сверка topic/mail ID с CP в SMAPI-логе |

### Debug-команды (SMAPI)

| Команда | Назначение |
|---------|------------|
| `injury_debuff_add <id>` | Наложить травму/осложнение из списка мода |
| `injury_debuff_list` | Список ID |
| `injury_reset` | Полный сброс состояния мода |
| `injury_phase_list` | Активные травмы, фаза, флаги готовности |
| `injury_phase_ready <buffId> [1\|0]` | Флаг «можно сменить фазу» (эмуляция ожидания) |
| `injury_phase_recovery <buffId> [1\|0]` | Флаг «можно завершить лечение» (последняя фаза) |
| `injury_phase_advance <buffId>` | Принудительная смена фазы (обход клика) |
| `injury_phase_cure <buffId>` | Полное выздоровление (обход клика) |
| `injury_debug_mine_rescue` | Выставить флаги спасения из шахты + `buffBadlyHurt` |

### Как проверять topic и mail

- **Conversation topic:** Social tab → вкладка «Conversation Topics» у NPC (или мод-оверлей, если есть). Альтернатива: SMAPI-лог после действия.
- **Mail:** Почтовый ящик на ферме утром (`mailReceived` в save / визуально письмо Харви).
- **Тон:** сверять реплику с блоком CP по условию `Hearts:Harvey` / `Relationship:Harvey` в `dialoguesHarveyInjury.json`, `dialoguesHarveyCure.json`, `dialoguesHarvey.json`.
- **Ошибка контента без CP:** `injury_audit_content` → `MISSING in CP dialogue` / `MISSING in CP mail`.

### Важно про `injury_debuff_add`

Команда создаёт **базовый topic травмы** и `DebuffState`, но **не дублирует все side-topics** из «натуральных» триггеров. Например, `ApplyBadlyHurt()` в игре добавляет ещё `topicHealthDamageCritical`, а debug-команда — **нет**. Для полной цепочки side-topics используйте игровой триггер или учитывайте расхождение в «что считать ошибкой».

---

## 1. Лёгкая травма — `buffHurt`

### Подготовка
- Сохранение без активных травм мода (`injury_reset`).
- 0–8♥ с Харви (любой тон; для сценария 12 — отдельно 0–1♥).
- Харви доступен для разговора.

### Команда / действие
1. `injury_debuff_add buffHurt`
2. Поговорить с Харви (клик) → начать лечение.
3. `injury_phase_list` — убедиться: `TreatmentStarted`, не фазовая, срок 3 дня.
4. Дождаться 3 игровых дня **или** ускорить через сон + проверку `DayStarted` (утром HUD «Лечение завершено! Обратись к Харви…»).
5. Снова поговорить с Харви при активном `topicHurtCured`.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| После наложения | `topicHurt` (3 д) |
| Первая treatable-травма (если FirstTreatment не seen) | `topicHarveyNeedsFirstTreatment` (7 д) — триггер события, **не dialogue key** |
| После клика «лечение» | `topicHurt` **снят**; лечебный бафф `buffHarveyTreatment` |
| После истечения срока лечения | `topicHurtCured` (7 д) |
| После финального разговора | `topicHurtCured` **снят** |

### Ожидаемый mail
- **Нет** (C# не шлёт письмо для `buffHurt`).

### Ожидаемый тон Харви
| ♥ / отношения | Injury (`topicHurt`) | Cured (`topicHurtCured`) |
|---------------|----------------------|---------------------------|
| 0–1♥ | «Покажите…» / «Вы» — block1 + `Hearts: 0,1` Hospital | «Заживление хорошее…» — нейтральный med (block1 cure) |
| 3–5♥ | «Порез? Покажи…» — `Hearts: 3,4,5` | Тот же block1 или hearts-блок cure |
| 6–10♥ | «Ты поцарапалась?» — `Hearts: 6–10` | Личнее, но без pet names |
| Dating / Married | «Солнышко…» — `Relationship: Dating/Married` | «…моя хорошая / солнышко…» — dating/married cure-блок |

### Что считать ошибкой
- После `injury_debuff_add` нет `buffHurt` или `topicHurt`.
- Клик по Харви не снимает `topicHurt` и не даёт `buffHarveyTreatment`.
- Через 3+ дня лечения нет HUD и нет `topicHurtCured`.
- Финальный разговор: vanilla/fallback вместо `topicHurtCured` из CP.
- На 0–1♥ — pet names («солнышко», «малышка») в injury или cured.
- `topicHurt` и `topicHurtCured` висят одновременно после финала.

---

## 2. Тяжёлая травма — `buffBadlyHurt`

### Подготовка
- `injury_reset`.
- Для side-topic `topicHealthDamageCritical` — **предпочтительно** натуральный триггер (HP ≤ 10 + обморок при dating/married) **или** принять, что debug даёт только `topicBadlyHurt`.

### Команда / действие
1. `injury_debuff_add buffBadlyHurt` *(или обморок с HP ≤ 10 при dating/married)*.
2. Поговорить с Харви → лечение.
3. Ждать **8 дней** лечения (`Phase1Duration = 8`) или ускорять сон.
4. При `topicBadlyHurtCured` — финальный разговор.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| Наложение (игра) | `topicBadlyHurt` (8 д) + `topicHealthDamageCritical` (8 д) |
| Наложение (debug) | `topicBadlyHurt` (8 д) |
| После лечения | `buffHarveyIntensiveCare`, `topicBadlyHurt` снят |
| Завершение | `topicBadlyHurtCured` (7 д) |

### Ожидаемый mail
- **Нет** от C# для базовой тяжёлой травмы.

### Ожидаемый тон Харви
- 0–1♥: «Травма серьёзная… не откладывайте» (block1 / Hospital).
- 3–5♥: «Травма серьёзнее… не спорь».
- Dating/Married: «Малышка, серьёзные травмы…» — тревога + опека.
- Cured: med → с 6♥/dating — личная благодарность и предупреждение беречь себя.

### Что считать ошибкой
- Нет `buffBadlyHurt` / `topicBadlyHurt`.
- Лечение не даёт `buffHarveyIntensiveCare`.
- Нет `topicBadlyHurtCured` после 8 дней.
- `topicHealthDamageCritical` остаётся навсегда после cured *(известный риск C# — не удаляется в `RemoveInjuryRelatedTopics`)* — зафиксировать как баг, если висит > 7 д после финала.
- Pet names на 0–1♥.

---

## 3. Фазовая травма — `buffDeepCuts`

### Подготовка
- `injury_reset`.
- Понимание фаз: **3 + 7 + 4 = 14 дней** (Acute → Healing → Recovery).

### Команда / действие
1. `injury_debuff_add buffDeepCuts`
2. Клик Харви → лечение (фаза 1).
3. `injury_phase_list` → `фаза 1/3`, `в лечении`.
4. Для каждой смены фазы:
   - `injury_phase_ready buffDeepCuts 1` → клик Харви **или** `injury_phase_advance buffDeepCuts`
   - Проверить диалог `PhaseTransition_DeepCuts_2` / `_3`
5. На фазе 3/3: `injury_phase_recovery buffDeepCuts 1` → клик → `CompleteRecovery`.
6. Альтернатива финала: `injury_phase_cure buffDeepCuts`.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| До лечения | `topicDeepCuts` (14 д) |
| После старта лечения | `topicDeepCuts` снят; `topicTreatmentDeepCuts`; `topicDeepCutsPhaseAcute` (7 д) |
| Фаза 2 | `topicDeepCutsPhaseHealing` |
| Фаза 3 | `topicDeepCutsPhaseRecovery` |
| Выздоровление | `topicDeepCutsCured` (7 д); treatment/phase topics сняты |
| Бонус | `buffHarveyCare` на ~2 дня |

### Ожидаемый mail
- **Нет** при штатном прохождении.
- При **просрочке фазы** (не кликать Харви после `ReadyForNextPhase` > grace): `HarveyMod_TreatmentUrgentReminder` → `HarveyMod_TreatmentFinalWarning` → `HarveyMod_NeglectWarning` (отдельный негативный тест).

### Ожидаемый тон Харви
- Injury: «Глубокие порезы…» (med → личный по ♥).
- PhaseTransition: «Швы держатся…» / «Рубец формируется…» (block1, нейтральный).
- Cured: развёрнутый осмотр + советы по острым предметам; dating/married — «малышка / хрупкая».

### Что считать ошибкой
- После лечения остаётся базовый `buffDeepCuts` вместо phase-бaffa.
- Нет `topicTreatmentDeepCuts` или phase-topics.
- `injury_phase_ready` + клик не меняет фазу / не показывает PhaseTransition.
- `topicDeepCutsCured` не появляется после recovery.
- Phase-topics висят после cured.

---

## 4. Перелом — `buffFracturedBone`

### Подготовка
- `injury_reset`.
- Фазы: **7 + 35 + 14 = 56 дней** (долгий сценарий — использовать debug-флаги фаз).

### Команда / действие
1. `injury_debuff_add buffFracturedBone`
2. Клик Харви → лечение.
3. Цикл фаз 1→2→3 через `injury_phase_ready` / `injury_phase_recovery` + клики.
4. Финал: `topicFracturedBoneCured`.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| Наложение (игра) | `topicFracturedBone` + `topicHealthDamageCritical` |
| Наложение (debug) | `topicFracturedBone` (56 д) |
| Лечение | `topicTreatmentFracturedBone`, `topicFracturedBonePhaseAcute/Healing/Recovery` |
| Финал | `topicFracturedBoneCured` |

### Ожидаемый mail
- **Нет** (штатно). В CP есть `HarveyMod_FractureAlert` — **C# не шлёт**; получение = отдельный CP-trigger, не ошибка этого сценария.

### Ожидаемый тон Харви
- «Признаки перелома… гипс… шахта запрещена» (med).
- Dating: «Малышка, вижу перелом…» + фиксация, гипс, опека.
- PhaseTransition_2/3: терпение / снятие гипса.

### Что считать ошибкой
- Нет Severe-поведения в шахте при активном переломе *(см. сценарий 11)*.
- Фазовая машина не стартует (остался `buffFracturedBone` без phase buff).
- Нет cured-topic после recovery.
- `ForceHospitalization: true` + dating — неожиданная госпитализация **до** клика (если конфиг включён — ожидаемо; иначе — баг).

---

## 5. Сотрясение — `buffConcussion`

### Подготовка
- `injury_reset`.
- Фазы: **3 + 11 + 7 = 21 день**.

### Команда / действие
1. `injury_debuff_add buffConcussion`
2. Лечение → фазы → recovery → cured.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| Наложение (игра) | `topicConcussion` + `topicHealthDamageSevere` |
| Наложение (debug) | `topicConcussion` (21 д) |
| Лечение | `topicTreatmentConcussion`, phase topics Acute/Healing/Recovery |
| Финал | `topicConcussionCured` |

### Ожидаемый mail
- **Нет** (штатно).

### Ожидаемый тон Харви
- Med: «Подозрение на сотрясение… зрачки… покой».
- 6–10♥ / dating: бледность, проверка зрачков, «не волнуйся».
- Cured: неврологическая стабильность, постепенное возвращение к нагрузкам.

### Что считать ошибкой
- Нет `topicConcussion` / phase pipeline.
- Side-topic `topicHealthDamageSevere` не снимается после cured *(риск, как у Critical)*.
- Pet names на 0–1♥ в injury.

---

## 6. Инфекция — `buffInfectedWound`

### Подготовка
- `injury_reset`.
- Фазы: **3 + 11 = 14 дней** (2 фазы).

### Команда / действие
1. `injury_debuff_add buffInfectedWound`
2. Лечение → `injury_phase_ready` на фазе 1 → клик → фаза 2.
3. `injury_phase_recovery buffInfectedWound 1` → клик → cured.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| Наложение | `topicInfectedWound` (14 д) |
| Лечение | `topicTreatmentInfectedWound`, `topicInfectedWoundPhaseAcute`, `topicInfectedWoundPhaseHealing` |
| Финал | `topicInfectedWoundCured` |

### Ожидаемый mail
- **Нет** при прямом наложении.
- При переходе **из** dirty/wet (сценарии 7–8): `HarveyMod_DirtyWoundInfection` / `HarveyMod_WetBandageInfection`.

### Ожидаемый тон Харви
- Med: «Признаки воспаления… 39°… антибиотики».
- Dating: «Солнышко… каждые 6 часов… не дам инфекции победить».
- PhaseTransition_InfectedWound_2: «Инфекция отступает…»

### Что считать ошибкой
- 2-фазная травма обрабатывается как simple cure.
- Нет `topicInfectedWoundCured`.
- Письмо об инфекции без предшествующего dirty/wet.

---

## 7. Мокрая повязка — `HarveyMod_WetBandage`

### Подготовка
- Активное лечение с повязкой: `buffHarveyTreatment` или `buffHarveyIntensiveCare` (например, после лечения `buffHurt` / `buffBadlyHurt`).
- **Или** быстрый путь: `injury_debuff_add HarveyMod_WetBandage` (только topic + осложнение).

### Команда / действие

**Натуральный путь:**
1. Начать лечение (`buffHarveyTreatment`).
2. Стоять под дождём на outdoor-локации (не в здании) несколько минут.
3. HUD «Повязка промокла!»

**Debug-путь:**
1. `injury_debuff_add HarveyMod_WetBandage`
2. Поговорить с Харви (обработка осложнения при клике, если в списке complications).

**Эскалация (отдельная проверка):**
- Не лечить 1–3+ дня → `CheckWetBandageComplication` → инфекция.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| Промокание | `topicHarvey_WetBandage` (4 д) |
| После инфекции | `topicHarvey_WetBandage` снят → `topicInfectedWound` + фазовое лечение |

### Ожидаемый mail
- При инфекции: **`HarveyMod_WetBandageInfection`** (утро после roll).
- До инфекции: **нет**.

### Ожидаемый тон Харви
- `topicHarvey_WetBandage` (dialoguesHarvey.json block1): «Повязка промокла… смени на сухую…» — med, без pet names на 0♥.
- Memory keys `topicHarvey_WetBandage_memory_*` — только при соответствующих CP-триггерах.

### Что считать ошибкой
- Промокание без активной повязки (`buffHarveyTreatment` / IntensiveCare).
- Нет topic после HUD.
- Инфекция без письма при `SendLetters: true`.
- `HarveyMod_WetCare` в mail — **C# не шлёт** (задел); получение не ожидается.

---

## 8. Грязная рана — `HarveyMod_DirtyWound`

### Подготовка
- Активная травма, подходящая для mine-dirty (`buffDeepCuts` и др. из `InjurySets.DirtyInMines`) **или** debug.

### Команда / действие

**Натуральный путь:**
1. `injury_debuff_add buffDeepCuts` (или другая open-wound).
2. Войти в Mine / Volcano, провести время (exposure + roll).
3. HUD «Рана загрязнилась!»

**Debug-путь:**
1. `injury_debuff_add HarveyMod_DirtyWound`

**Эскалация:**
- День 1: 15% / день 2: 40% / день 3+: 100% roll → `ApplyInfectedWoundSafe`.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| Загрязнение | `topicHarvey_DirtyWound` (4 д) |
| Инфекция | `topicHarvey_DirtyWound` снят → `topicInfectedWound` |

### Ожидаемый mail
- При инфекции: **`HarveyMod_DirtyWoundInfection`** (+ антибиотики вложением).
- До инфекции: **нет**.

### Ожидаемый тон Харви
- «Шахты? С открытой раной?…» — строгий med (block1).
- Dating-блоки memory — личнее, если CP-триггер сработал.

### Что считать ошибкой
- Dirty без активной травмы / без входа в шахту (для натурального пути).
- Нет topic / buff `HarveyMod_DirtyWound`.
- Инфекция без mail.
- Одновременно dirty + wet без логики лечения.

---

## 9. Обморок в городе

### Подготовка
- `injury_reset`.
- Локация **Town** (или подлокация с `Town` в имени).
- **Нет** dating/married **или** dating — оба варианта стоит проверить (тон письма одинаковый).
- Нет активного `topicPassedOutInTown` / `buffSleepy`.

### Команда / действие
1. Оставаться в Town до **2:00** (`timeOfDay >= 2600`) без сна → обморок.
2. Утром после warp: проверить buff/topic/mail.
3. Поговорить с Харви.

*(Debug-эмуляция полного pass-out pipeline в C# отсутствует — только натуральный обморок.)*

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| После обморока | `topicPassedOutInTown` (2 д), `buffSleepy` |

### Ожидаемый mail
- **`mailHarveySleepControl`** — на **следующее утро** после обморока (`SendLetters: true`).
- Шлётся **без** проверки dating/married.

### Ожидаемый тон Харви
| ♥ | Topic |
|----|-------|
| 0–1♥ | «Обморок на улице… приходите в клинику» (block1) |
| 3–5♥ | «Ты потеряла сознание… завтра осмотр» |
| Dating/Married | «Девочка моя… не выходишь одна» |

Письмо: med «Вы снова пренебрегли сном…» (формальное «Вы»).

### Что считать ошибкой
- Обморок в Town без `topicPassedOutInTown`.
- Нет письма при `SendLetters: true`.
- Обморок в Mine вместо Town → другая ветка (сценарий 10).
- Pet names в topic на 0–1♥.

---

## 10. Спасение из шахты

### Подготовка
- **Dating или married** с Харви (обязательно для rescue).
- `injury_reset`.
- Событие mine rescue **не** просмотрено (`!seen` для выбранного eventId).

### Команда / действие

**Натуральный путь:**
1. Dating/married.
2. Умереть в Mine (health → 0, не exhaustion / не late night).
3. Сон → утро: warp в Mine, cutscene Харви.

**Debug-путь:**
1. `injury_debug_mine_rescue`
2. Сон → утро → вход/warp в Mine.

### Ожидаемый topic
| Этап | Topic |
|------|-------|
| После смерти | `buffBadlyHurt`, `topicBadlyHurt`, `topicHarveyNeedsFirstTreatment` (если first) |
| Gate | `topicMineRescuePending` (блокирует CP mine triggers; **не dialogue**) |
| После события | `topicMineInjuryRescue` (событие добавляет само) |
| Pending | снят |

### Ожидаемый mail
- **Нет** прямого C#-mail на rescue.
- На **следующий день** после rescue-warp в Mine с Severe возможен **ложный** mine warning → `mailHarveyMineForbidden` *(известный timing-риск)*.

### Ожидаемый tон Харви
- В cutscene — по версии события (dating vs friendship).
- `topicMineInjuryRescue`: med «Пришлось вытащить…» → dating «Малышка… леденело сердце».

### Что считать ошибкой
- Dating/married, смерть в Mine — **нет** rescue (нет warp / нет события).
- `buffBadlyHurt` не наложен.
- Нет `topicMineInjuryRescue` после события.
- Rescue без отношений — **не ошибка** (by design).
- Cutscene не проигрывается, флаги `NeedsMineRescueEvent` зависли.

---

## 11. Запрет шахты и письмо

### Подготовка
- `injury_reset`.
- **Severe**-травма: `injury_debuff_add buffBadlyHurt` (или FracturedBone, Concussion, InfectedWound, BurnWounds, ShrapnelWounds, SurgicalWound).
- `SendLetters: true`.
- **Не** активен `NeedsMineRescueEvent` (иначе warning подавлен).

### Команда / действие
1. Наложить Severe-травму.
2. **В тот же день** войти в **Mine** или **VolcanoDungeon**.
3. HUD: «У тебя серьёзные раны — ты не должна идти в шахту!»
4. **Лечь спать** (DayEnding).
5. **Утро:** проверить почту и buff.
6. Попробовать снова войти в шахту (блок `HarveyMod_MineForbidden`).

**Контроль:** `buffDeepCuts` (не Severe) → только мягкий HUD, **без** `MineWarningDay` и **без** mail.

### Ожидаемый topic
- Topics **не используются** в цепочке запрета (только buffs).
- При наличии `topicMineInjuryRescue` + `ForceHospitalization` — отдельная госпитализация (не этот сценарий).

### Ожидаемый mail
- **`mailHarveyMineForbidden`** — утро **после** дня с warning (вечером `addMailForTomorrow`).
- Текст: запрет шахты/вулкана, med authority.

### Ожидаемый buff
- Утро после warning-day: **`HarveyMod_MineForbidden`** (~2 дня по `MineForbiddenDurationDays`).

### Ожидаемый тон Харви
- HUD: строгое «serьёзные раны».
- Письмо: «Как твой врач… вход запрещён» — допустимо на любом ♥ (med authority).

### Что считать ошибкой
- Severe + вход в Mine — нет HUD / `MineWarningDay` не выставлен.
- После сна — нет письма при `SendLetters: true`.
- Нет `HarveyMod_MineForbidden` на следующее утро.
- Лёгкая травма (`buffHurt`, `buffDeepCuts`) → письмо/запрет *(ложное срабатывание)*.
- Двойное письмо за один инцидент.

---

## 12. Низкие сердца с Харви (0–1♥)

### Подготовка
- Новый или отдельный слот: **0–1♥** (≤ 250 friendship), **не** dating/married.
- `injury_reset`.
- Рекомендуется пройти **не** `HarveyMod_FirstTreatment` (или принять смешанный тон события).

### Команда / действие
Проверить **набор репрезентативных topics** (можно через debug):

1. `injury_debuff_add buffHurt` → разговор → cured pipeline.
2. `injury_debuff_add buffBadlyHurt` → разговор.
3. Обморок в Town (сценарий 9) → topic + mail.
4. `injury_debuff_add HarveyMod_WetBandage` → разговор.
5. При 0♥ + `HasConversationTopic: topicHurt` — проверить `Hospital_Mon`…`Sun` (отдельный When-блок).

### Ожидаемый topic
- Те же ID, что в сценариях 1–9; важен **тон**, не другой ID.

### Ожидаемый mail
- `mailHarveySleepControl`, `mailHarveyMineForbidden` — **медицинский** тон, «Вы» допустимо.
- Системные `HarveyMod_*` — «Фермер, это Харви» (нейтрально).

### Ожидаемый тон Харви
| Источник | Ожидание на 0–1♥ |
|----------|------------------|
| `dialoguesHarveyInjury.json` block1 | «Вы», «Покажите», без «солнышко/малышка/котёнок» |
| `Hearts: 0,1` + `topicHurt` | Hospital_* — клинический протокол |
| `dialoguesHarveyCure.json` block1 | Med cured, без личной опеки |
| Dating-only keys | **Не должны** срабатывать |

### Что считать ошибкой
- Pet names, «солнышко», «девочка моя», «@» на 0–1♥ в injury/cured/Hospital.
- Cured-тексты уровня dating («моя хорошая», «хрупкая девушка») на 0♥ *(известный риск block1 cure — фиксировать)*.
- `Relationship: Dating/Married` блок перекрывает 0♥.

---

## 13. Dating с Харви

### Подготовка
- Подарить букет → **Dating**.
- `injury_reset`.

### Команда / действие
1. `injury_debuff_add buffHurt` → topic + cured.
2. `injury_debuff_add buffFracturedBone` или `buffConcussion` → heavy tone.
3. `injury_debuff_add buffDeepCuts` → phase + cured.
4. Обморок Town / mine rescue (если не пройден) — опционально.

### Ожидаемый topic
- Стандартные ID; CP выбирает блок `Relationship: Harvey → Dating`.

### Ожидаемый mail
- Med-письма (`mailHarveySleepControl`, `HarveyMod_*`) остаются сдержанными — **OK**.
- CP-only alert-письма (`HarveyMod_FractureAlert` и т.д.) — не от C#.

### Ожидаемый тон Харви
- Injury: «Солнышко», «малышка», «котёнок», тревога + нежность.
- Cured: личная забота, «хрупкая», «я рядом».
- `topicPassedOutInTown`: «Девочка моя… не выходишь одна».
- PhaseTransition на dating: те же neutral PhaseTransition (block1) — **ожидаемо**.

### Что считать ошибкой
- Dating, но срабатывает только block1 med без личных имён *(When Dating не применился)*.
- Formal «Вы» в dating-блоке injury (не в mail).
- Married-тексты при статусе Dating.

---

## 14. Married с Харви

### Подготовка
- Женаты на Харви.
- `injury_reset`.
- Проверить домашний контекст (Harvey spouse) + клинику.

### Команда / действие
1. `injury_debuff_add buffHurt` → лечение → cured.
2. `injury_debuff_add buffBadlyHurt` → intensive care tone.
3. `injury_debuff_add buffInfectedWound` → antibiotics tone.
4. Complication: wet/dirty → разговор.
5. Mine forbidden с Severe — med письмо OK.

### Ожидаемый topic
- Те же ID; приоритет **`Relationship: Married`** над hearts-блоками.

### Ожидаемый mail
- Как в dating: med authority в системных письмах — норма.
- `MarriageDialogueHarvey` — вне scope injury, unless buff/topic hooks.

### Ожидаемый тон Харви
- Injury/cured: как dating + чуть более domestic («моя хорошая», «ладно?», совместный быт).
- `topicMineInjuryRescue`: эмоциональная опека.
- Hospital_* при married + injury — не должны отменять spouse-тон без причины.

### Что считать ошибкой
- Married, но только 0♥ med block1 без spouse intimacy.
- Dating-блок вместо married (если оба When — married должен перекрывать по Priority/Late).
- Отсутствие cured-реплики для married при наличии dating-варианта.

---

## Сводная таблица mail (C# → CP)

| Mail ID | Сценарий |
|---------|----------|
| `mailHarveySleepControl` | 9 — обморок в Town |
| `mailHarveyMineForbidden` | 11 — Severe + вход в шахту + сон |
| `HarveyMod_WetBandageInfection` | 7 — эскалация wet |
| `HarveyMod_DirtyWoundInfection` | 8 — эскалация dirty |
| `HarveyMod_TreatmentUrgentReminder` | 3–6 — просрочка фазы (+3 дня после фазы) |
| `HarveyMod_TreatmentFinalWarning` | 3–6 — просрочка (+6 дней) |
| `HarveyMod_NeglectWarning` | 3–6 — просрочка (≥7 дней grace) |

---

## Чек-лист перед релизом (кратко)

- [ ] Сценарии 1–6: debuff → topic → treat → cured topic → финальный диалог.
- [ ] 7–8: complication topic + mail при инфекции.
- [ ] 9: Town pass-out + `mailHarveySleepControl`.
- [ ] 10: mine rescue (dating) + `topicMineInjuryRescue`.
- [ ] 11: Severe mine → mail + `HarveyMod_MineForbidden`.
- [ ] 12–14: тон 0♥ / dating / married на `topicHurt`, cured, complications.
- [ ] `injury_audit_content` — 0 missing mail, gate-only topics OK.

---

## Связанные документы

- [final-validation-topics-mail.md](./final-validation-topics-mail.md) — автоматическая сверка ID
- [EVENTS_TEST_CHECKLIST.md](./EVENTS_TEST_CHECKLIST.md) — чеклист CP-событий
- [FOR_TEST.md](./FOR_TEST.md) — SMAPI-команды и справочник
- [README.md](./README.md) — индекс тестовой документации
- [../audit-relationship-tone.md](../audit-relationship-tone.md) — матрица тона по ♥
- [../mines-forbidden-injuries.md](../mines-forbidden-injuries.md) — Severe + запрет шахты
- [../events-inventory/14-scenario-chains.md](../events-inventory/14-scenario-chains.md) — цепочки C#→CP
