# Чеклист: продвинутые механики InjuryCare

Ручная проверка новых систем: **предписания**, **доверие к лечению**, **контрольные осмотры**, **реабилитация**, **медицинский план**, **самопомощь**, **proximity-реакции** и **регрессия** базового лечения.

**Моды:** C# `HarveyOverhaulInjury` + Content Patcher `HarveyOverhaul [CP]`  
**UniqueID C#:** `marilynsinister.HarveyOverhaul.Injury`

---

## Общая подготовка

| Параметр | Значение |
|----------|----------|
| Консоль SMAPI | `\` (по умолчанию) |
| Debug-HUD | **F10** — травмы, предписания, compliance, rehab, proximity |
| Полный дамп | `injury_debug_dump` |
| Сброс перед сценарием | `injury_reset` |
| Письма | В `config.json`: `SendLetters: true` |
| Proximity | Дистанция ≤ `ProximityTiles` (по умолчанию **3**), антиспам: **1× за локацию** + кулдаун **120** игр. мин |

**Обязательные команды (используются в сценариях ниже):**

```text
injury_reset
injury_debuff_add <id>
injury_phase_ready <id> 1
injury_phase_recovery <id> 1
injury_prescription_list
injury_prescription_add <id> <injuryId> [days]
injury_compliance_set <number>
injury_checkup_due <buffId>
injury_rehab_start <buffId> [days]
injury_rehab_status
injury_selfcare_bandage
injury_selfcare_tea
```

**Дополнительно полезные:**

```text
injury_phase_list
injury_phase_advance <buffId>
injury_phase_cure <buffId>
injury_prescription_clear
injury_rehab_clear
injury_proximity_test <situation> [tone]
injury_debug_mine_rescue
injury_mine_forbidden_clear
injury_medical_snapshot
```

**Тоны proximity (`HarveyHelper.GetRelationshipToneWithHarvey`):** 0–3 ♥ → `Low`; 4–7 ♥ → `Mid`; 8+ ♥ → `High`; dating/married → `Romantic` (в CP для married иногда мапится на `High`).

---

## 1. Предписания Харви

- [ ] **1.1 Автоназначение предписаний после начала лечения**

  **Цель:** убедиться, что после клика «лечение» назначаются правильные предписания, topic и CP-баффы.

  **Подготовка:** чистое состояние; Харви в клинике или рядом на карте.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  ```

  **Действия:**
  1. Кликнуть по Харви → начать лечение.
  2. Выполнить `injury_prescription_list`.
  3. Открыть F10 → блок предписаний.
  4. Проверить иконки баффов у игрока (`HarveyMod_Prescription_*`).

  **Ожидаемый результат:**
  - Для `buffDeepCuts`: `KeepDry`, `NoMine`, `Checkup`.
  - Topics: `topicHarvey_Prescription_KeepDry`, `topicHarvey_Prescription_NoMine`, `topicHarvey_Prescription_Checkup`.
  - Баффы с мягкими эффектами / информационные (CP `buffsMedicalCare.json`).
  - В логе: `Назначены предписания для buffDeepCuts: ...`

  **Логи SMAPI:** `Назначены предписания для`, `Предписание HarveyMod_Prescription_*`, `[TreatmentPlan]`

---

- [ ] **1.2 Ручное добавление предписания (консоль)**

  **Цель:** проверить `injury_prescription_add` и отображение в списке.

  **Подготовка:** активная травма с начатым лечением.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffFracturedBone
  injury_prescription_add HarveyMod_Prescription_Rest buffFracturedBone 5
  injury_prescription_add HarveyMod_Prescription_NoMine buffFracturedBone 7
  injury_prescription_list
  ```

  **Действия:**
  1. Кликнуть Харви → начать лечение (если ещё не начато).
  2. Добавить предписания командами выше.
  3. Поговорить с Харви → реплика по `topicHarvey_Prescription_Rest` / `NoMine` (тон по сердечкам).

  **Ожидаемый результат:**
  - `injury_prescription_list` показывает оба предписания и `TreatmentComplianceScore`.
  - Бафф `HarveyMod_Prescription_Rest` / `NoMine` на игроке.
  - Диалог из CP `dialoguesHarveyMedicalCare.json`.

  **Логи SMAPI:** `Предписание HarveyMod_Prescription_* (N дн.) ← buffFracturedBone`

---

- [ ] **1.3 Предписание Rest — ранний сон (self-care path)**

  **Цель:** проверить предписание «отдых» и связь с самопомощью.

  **Подготовка:** активное предписание Rest.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffConcussion
  injury_prescription_add HarveyMod_Prescription_Rest buffConcussion 3
  ```

  **Действия:**
  1. Начать лечение у Харви.
  2. Лечь спать **до 22:00** (или дождаться конца дня с `timeOfDay < 2200`).
  3. На следующий день проверить compliance и topics.

  **Ожидаемый результат:**
  - При раннем сне: `[SelfCare] RestCare применён`, compliance +1.
  - Topic `topicHarvey_SelfCarePraise` (если сработал RestCare).
  - Нарушение Rest при позднем сне — см. раздел 2.

  **Логи SMAPI:** `[SelfCare] RestCare применён`, `Compliance * → * (+1, selfcare_rest)`

---

- [ ] **1.4 Предписание LightWork**

  **Цель:** предписание «лёгкая работа» и мягкий дебафф stamina.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffBackStrain
  injury_prescription_add HarveyMod_Prescription_LightWork buffBackStrain 3
  injury_prescription_list
  ```

  **Действия:**
  1. Начать лечение.
  2. Проверить бафф `HarveyMod_Prescription_LightWork` (описание в CP, −3 MaxStamina).
  3. Поговорить с Харви → `topicHarvey_Prescription_LightWork`.

  **Ожидаемый результат:** информационный/мягкий бафф; topic-диалог без грубости; нарушение при heavy work — раздел 2.

  **Логи SMAPI:** `Предписание HarveyMod_Prescription_LightWork`

---

- [ ] **1.5 Предписание Checkup (информационное)**

  **Цель:** бафф-напоминание о контрольном осмотре.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_prescription_add HarveyMod_Prescription_Checkup buffDeepCuts 7
  ```

  **Действия:**
  1. Начать лечение.
  2. Проверить бафф `HarveyMod_Prescription_Checkup` и topic `topicHarvey_Prescription_Checkup`.
  3. Связать с разделом 4 (`injury_checkup_due`).

  **Ожидаемый результат:** информационный бафф без жёстких штрафов; topic напоминает о визите в клинику.

  **Логи SMAPI:** `Предписание HarveyMod_Prescription_Checkup`

---

- [ ] **1.6 Снятие истёкших предписаний**

  **Цель:** предписания с коротким сроком исчезают, compliance за «следование» начисляется.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffHurt
  injury_prescription_add HarveyMod_Prescription_LightWork buffHurt 1
  ```

  **Действия:**
  1. Не нарушать предписание в день назначения.
  2. Переспать на следующий день (после истечения срока).
  3. `injury_prescription_list`.

  **Ожидаемый результат:** предписание снято; при полном соблюдении вчера — topic `topicHarvey_PrescriptionFollowed`; compliance может вырасти (`RewardComplianceDaily`).

  **Логи SMAPI:** `Сняты истёкшие предписания`, `Compliance * → * (+*, prescription_followed)`

---

## 2. Нарушения предписаний

- [ ] **2.1 NoMine — вход в шахту**

  **Цель:** нарушение предписания «без шахт», compliance −1, topic violation.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_prescription_add HarveyMod_Prescription_NoMine buffDeepCuts 7
  injury_compliance_set 0
  ```

  **Действия:**
  1. Войти в `MineShaft` / `VolcanoDungeon` (или локацию шахты).
  2. Проверить HUD-предупреждение.
  3. Поговорить с Харви → `topicHarvey_PrescriptionViolation`.
  4. `injury_prescription_list` — `ViolationCount`, `IsViolated`.

  **Ожидаемый результат:**
  - HUD: «Харви просил тебя не ходить в шахту...»
  - Compliance −1 (`prescription_violation:mine`).
  - Topic `topicHarvey_PrescriptionViolation` (2 дня).
  - Повторное нарушение **в тот же день** не дублируется.

  **Логи SMAPI:** `[Prescription] NoMine violation #N`, `Нарушение предписания HarveyMod_Prescription_NoMine: mine`, `Compliance * → * (-1, prescription_violation:mine)`

---

- [ ] **2.2 KeepDry — дождь под открытым небом**

  **Цель:** нарушение «держать сухим» при длительном дожде.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffBurnWounds
  injury_prescription_add HarveyMod_Prescription_KeepDry buffBurnWounds 5
  injury_rain_debug 150 150
  ```

  **Действия:**
  1. В дождь стоять на улице ≥ **120 сек** (или использовать `injury_rain_debug`).
  2. Дождаться срабатывания violation.

  **Ожидаемый результат:** `[Prescription] KeepDry violation`, topic violation, compliance −1.

  **Логи SMAPI:** `Нарушение предписания HarveyMod_Prescription_KeepDry: rain`

---

- [ ] **2.3 LightWork — тяжёлый инструмент при низкой энергии**

  **Цель:** нарушение при работе топором/киркой при stamina ≤ 25%.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffTornMuscles
  injury_prescription_add HarveyMod_Prescription_LightWork buffTornMuscles 3
  ```

  **Действия:**
  1. Снизить stamina игрока (< 25% max).
  2. Использовать Pickaxe/Hoe/Axe ≥ ~90 сек непрерывно.

  **Ожидаемый результат:** violation `heavy_work`, compliance −1, HUD-предупреждение.

  **Логи SMAPI:** `Нарушение предписания HarveyMod_Prescription_LightWork: heavy_work`

---

- [ ] **2.4 Rest — поздний сон**

  **Цель:** нарушение Rest при засыпании после 22:00.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffConcussion
  injury_prescription_add HarveyMod_Prescription_Rest buffConcussion 3
  ```

  **Действия:**
  1. Дождаться `timeOfDay >= 2200` (или сменить время через мод-читы / дождаться в игре).
  2. Лечь спать.

  **Ожидаемый результат:** violation `late_sleep` на `DayEnding`; compliance −1.

  **Логи SMAPI:** `Нарушение предписания HarveyMod_Prescription_Rest: late_sleep`

---

- [ ] **2.5 Третье нарушение → NeglectStrikes**

  **Цель:** эскалация при повторных нарушениях одного предписания.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffFracturedBone
  injury_prescription_add HarveyMod_Prescription_NoMine buffFracturedBone 7
  ```

  **Действия:**
  1. Три **разных игровых дня**: каждый день войти в шахту (или эмулировать через прямой вызов, если есть debug).
  2. После 3-го нарушения проверить `NeglectStrikes` в F10 / `injury_debug_dump`.

  **Ожидаемый результат:** на 3-м нарушении `NeglectStrikes++`; лог warn.

  **Логи SMAPI:** `Предписание HarveyMod_Prescription_NoMine: 3-е нарушение → NeglectStrikes=N`

---

- [ ] **2.6 Proximity при нарушении предписания**

  **Цель:** CP-реплика `Proximity_Prescription_Violated_*` после violation.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_prescription_add HarveyMod_Prescription_NoMine buffDeepCuts 7
  injury_proximity_test prescription_violated Mid
  ```

  **Действия:**
  1. Нарушить NoMine (войти в шахту).
  2. Выйти, найти Харви в той же локации (≤ 3 клетки), **новая локация** после violation.
  3. Дождаться proximity-облачка.

  **Ожидаемый результат:** prefix `Proximity_Prescription_Violated_{Low|Mid|High}`; topic `topicHarvey_ProximityStrict`; лечение **не** начинается автоматически.

  **Логи SMAPI:** `[Proximity] Показ облачка`, `LastReactionReason` содержит prescription context (F10 / `injury_debug_dump`)

---

## 3. Доверие к лечению

- [ ] **3.1 Базовый compliance и topics**

  **Цель:** `injury_compliance_set` выставляет уровень и соответствующий topic.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set 6
  injury_prescription_list
  ```

  **Действия:**
  1. Начать лечение (клик Харви).
  2. Поговорить с Харви → `topicHarvey_ComplianceHigh`.

  **Ожидаемый результат:** score = 6, level High; topic High на 2 дня; CP-диалог «Спасибо, что выполняешь рекомендации...».

  **Логи SMAPI:** `TreatmentComplianceScore = 6 (level: ...)`

---

- [ ] **3.2 Low compliance**

  **Цель:** низкий уровень доверия и строгий, но не грубый тон.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set -6
  ```

  **Действия:**
  1. Начать лечение.
  2. Поговорить с Харви → `topicHarvey_ComplianceLow`.
  3. Дождаться HUD «Харви всё чаще напоминает...» (раз в день при активном лечении).

  **Ожидаемый результат:** topic Low; proximity `Proximity_Compliance_Low_*` при встрече (раздел 8).

  **Логи SMAPI:** `TreatmentComplianceScore = -6`, `TryShowLowComplianceReminder` (HUD)

---

- [ ] **3.3 Neutral compliance**

  **Команды:**
  ```text
  injury_reset
  injury_compliance_set 0
  ```

  **Действия:** поговорить с Харви при активном лечении.

  **Ожидаемый результат:** topic `topicHarvey_ComplianceNeutral`.

  **Логи SMAPI:** `TreatmentComplianceScore = 0 (level: Neutral)`

---

- [ ] **3.4 Trusted patient (высокий compliance + бонус)**

  **Цель:** при High compliance и выздоровлении — `buffHarveyCare` + `topicHarvey_TrustedPatient`.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffHurt
  injury_compliance_set 7
  injury_phase_recovery buffHurt 1
  ```

  **Действия:**
  1. Начать лечение `buffHurt`.
  2. Дождаться готовности к recovery / `injury_phase_recovery buffHurt 1`.
  3. Клик Харви → выздоровление.
  4. Вызвать `ApplyHighComplianceRecoveryBonuses` (автоматически при recovery) или проверить после cure.

  **Ожидаемый результат:** `buffHarveyCare`; topic `topicHarvey_TrustedPatient`.

  **Логи SMAPI:** `High compliance recovery: buffHarveyCare + topicHarvey_TrustedPatient`

---

- [ ] **3.5 Compliance меняется от нарушений и осмотров**

  **Цель:** интеграция compliance с другими системами.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set 2
  injury_prescription_add HarveyMod_Prescription_NoMine buffDeepCuts 7
  injury_checkup_due buffDeepCuts
  ```

  **Действия:**
  1. Нарушить NoMine (−1).
  2. Пройти осмотр вовремя (+1 через `CompleteCheckup`).
  3. `injury_prescription_list` после каждого шага.

  **Ожидаемый результат:** score меняется с reason в логе; clamp [−10, +10].

  **Логи SMAPI:** `Compliance * → * (±N, prescription_violation:...)`, `Compliance * → * (+1, checkup_on_time:...)`

---

## 4. Контрольные осмотры

- [ ] **4.1 Готовность к смене фазы (phase checkup)**

  **Цель:** `injury_checkup_due` / естественный `ReadyForNextPhase` → topics и блокировка advance без осмотра.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_checkup_due buffDeepCuts
  ```

  **Действия:**
  1. **Сначала** начать лечение (клик Харви) — иначе debug-команда не сработает.
  2. `injury_checkup_due buffDeepCuts`.
  3. Проверить F10: `→след.фаза`.
  4. Поговорить с Харви → `topicHarvey_CheckupDue`, `topicHarvey_CheckupDue_DeepCuts`.
  5. Кликнуть для смены фазы.

  **Ожидаемый результат:**
  - Topics checkup на 3 дня.
  - Бафф `HarveyMod_Prescription_Checkup` (если назначен).
  - После осмотра: `[Checkup] Completed`, topics сняты, compliance ± по срокам.

  **Логи SMAPI:** `[Checkup] debug: buffDeepCuts → ReadyForNextPhase`, `[Checkup] Phase due buffDeepCuts → phase N`, `[Checkup] Completed for buffDeepCuts`

---

- [ ] **4.2 Финальный recovery checkup**

  **Цель:** осмотр перед полным выздоровлением.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffConcussion
  injury_phase_ready buffConcussion 1
  injury_phase_advance buffConcussion
  injury_phase_ready buffConcussion 1
  injury_phase_advance buffConcussion
  injury_checkup_due buffConcussion
  ```

  **Действия:**
  1. Начать лечение; довести до последней фазы (или debug).
  2. На последней фазе: `injury_checkup_due` → `ReadyForRecovery`.
  3. Поговорить с Харви → `topicHarvey_RecoveryCheckupDue`.

  **Ожидаемый результат:** recovery topics; после клика — cure + rehab (если травма в списке rehab).

  **Логи SMAPI:** `[Checkup] Recovery due buffConcussion`, `[Checkup] debug: buffConcussion → ReadyForRecovery`

---

- [ ] **4.3 Просрочка осмотра — напоминания и письмо**

  **Цель:** HUD на 2-й день, письмо `HarveyMod_CheckupOverdue` на 4-й, штраф на 5-й.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_checkup_due buffDeepCuts
  ```

  **Действия:**
  1. Начать лечение + выставить checkup due.
  2. **Не** кликать Харви; переспать 2, 4, 5 дней (или сдвинуть дату через save editor / debug time).
  3. Проверить почту и compliance.

  **Ожидаемый результат:**
  - День +2: HUD «Харви ждёт тебя на контрольный осмотр...»
  - День +4: письмо `HarveyMod_CheckupOverdue` (если `SendLetters: true`)
  - День +5: compliance −1, `NeglectStrikes++`

  **Логи SMAPI:** `[Checkup] Soft reminder day 2`, `[Checkup] Overdue letter scheduled`, `[Checkup] Overdue penalty`

---

- [ ] **4.4 Своевременный осмотр — бонус compliance**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set 0
  injury_checkup_due buffDeepCuts
  ```

  **Действия:** в тот же или следующий день кликнуть Харви для смены фазы.

  **Ожидаемый результат:** compliance +1 (`checkup_on_time`).

  **Логи SMAPI:** `Compliance * → * (+1, checkup_on_time:buffDeepCuts)`, `[Checkup] Completed`

---

## 5. Реабилитация

- [ ] **5.1 Автостарт после выздоровления**

  **Цель:** после cure тяжёлой травмы стартует rehab.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffConcussion
  injury_phase_ready buffConcussion 1
  injury_rehab_status
  ```

  **Действия:**
  1. Пройти полное лечение до cure (фазы + recovery).
  2. `injury_rehab_status`.

  **Ожидаемый результат:**
  - `buffHarveyRehab` на игроке.
  - Topic `topicHarvey_Rehab`.
  - HUD «Харви назначил восстановительный режим...»
  - F10: блок REHAB.

  **Логи SMAPI:** `[Rehab] Старт: buffConcussion, N дн.`

---

- [ ] **5.2 Принудительный старт (консоль)**

  **Команды:**
  ```text
  injury_reset
  injury_rehab_start buffFracturedBone 5
  injury_rehab_status
  ```

  **Действия:** проверить бафф, topic, статус.

  **Ожидаемый результат:** 5 дней rehab; `left=5d`; мягкий debuff speed/stamina (CP).

  **Логи SMAPI:** `[Rehab] Старт: buffFracturedBone, 5 дн.`, `Реабилитация запущена для buffFracturedBone`

---

- [ ] **5.3 Нарушение rehab — шахта**

  **Команды:**
  ```text
  injury_reset
  injury_rehab_start buffConcussion 3
  injury_compliance_set 3
  ```

  **Действия:** войти в шахту.

  **Ожидаемый результат:** topic `topicHarvey_RehabStrict`; compliance −1; HUD ошибки; proximity `Proximity_Rehab_Violated_*`.

  **Логи SMAPI:** `[Rehab] Нарушение #1 (mine)`, `Compliance * → * (-1, rehab_mine)`

---

- [ ] **5.4 Завершение rehab без нарушений**

  **Команды:**
  ```text
  injury_reset
  injury_rehab_start buffBadlyHurt 2
  ```

  **Действия:** переспать 2+ дня без нарушений; `injury_rehab_status`.

  **Ожидаемый результат:** rehab снята; topic `topicHarvey_RehabCompleted`; compliance +1 (`rehab_perfect`); опционально `buffHarveyCare`.

  **Логи SMAPI:** `[Rehab] Завершена реабилитация после buffBadlyHurt (violations=False)`

---

- [ ] **5.5 Rehab strict при повторных нарушениях**

  **Команды:**
  ```text
  injury_reset
  injury_rehab_start buffShrapnelWounds 5
  ```

  **Действия:** в разные дни — шахта, heavy work при low stamina, поздний сон.

  **Ожидаемый результат:** `RehabViolationCount` растёт; topic Strict; продление режима (если реализовано в менеджере).

  **Логи SMAPI:** `[Rehab] Нарушение #N (heavy_work|late_sleep|mine)`

---

## 6. Медицинский план / письма

- [ ] **6.1 План лечения после начала лечения**

  **Цель:** topics + письмо на следующий день.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffConcussion
  ```

  **Действия:**
  1. Клик Харви → лечение.
  2. HUD «Харви составил план лечения...»
  3. Переспать → проверить почту `mailHarveyTreatmentPlan_Concussion`.
  4. Поговорить с Харви → `topicHarvey_TreatmentPlanGiven`.

  **Ожидаемый результат:** письмо с рекомендациями; topic Given + `topicHarvey_TreatmentPlan_Concussion` (если есть в CP).

  **Логи SMAPI:** `[TreatmentPlan] Письмо mailHarveyTreatmentPlan_Concussion запланировано на завтра`, `[TreatmentPlan] topics: topicHarvey_TreatmentPlanGiven, ...`

---

- [ ] **6.2 План для minor-травмы**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffHurt
  ```

  **Действия:** лечение → сон → почта.

  **Ожидаемый результат:** `mailHarveyTreatmentPlan_Minor`.

  **Логи SMAPI:** `[TreatmentPlan] Письмо mailHarveyTreatmentPlan_Minor ...`

---

- [ ] **6.3 План для severe / infection / fracture / burn / cold**

  **Команды (по одному сценарию):**
  ```text
  injury_reset
  injury_debuff_add buffBadlyHurt
  ```
  ```text
  injury_reset
  injury_debuff_add buffInfectedWound
  ```
  ```text
  injury_reset
  injury_debuff_add buffFracturedBone
  ```
  ```text
  injury_reset
  injury_debuff_add buffBurnWounds
  ```
  ```text
  injury_reset
  injury_debuff_add HarveyMod_Cold_Acute
  ```

  **Ожидаемый результат:** соответствующие `mailHarveyTreatmentPlan_*` из CP.

  **Логи SMAPI:** `[TreatmentPlan] Письмо mailHarveyTreatmentPlan_*`

---

- [ ] **6.4 CP-only письма (заготовки для будущего C#)**

  **Цель:** убедиться, что тексты есть в CP (отправка из C# может быть ещё не подключена).

  **Проверка в CP / почтовом ящике через debug:**
  - `mailHarvey_CheckupReminder`
  - `mailHarvey_RehabReminder`
  - `mailHarvey_PrescriptionViolation`

  **Примечание:** сейчас C# шлёт `HarveyMod_CheckupOverdue` при просрочке осмотра; три новых ID — CP-контент на будущее.

  **Логи SMAPI:** `[Checkup] Overdue letter scheduled` → `HarveyMod_CheckupOverdue`

---

## 7. Самопомощь

- [ ] **7.1 Clean bandage (дома)**

  **Цель:** смена повязки дома, защита от инфекции, compliance при визите к Харви.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_debuff_add HarveyMod_WetBandage
  injury_selfcare_bandage
  ```

  **Действия:**
  1. Начать лечение (для контекста).
  2. `injury_selfcare_bandage` (force, без проверки локации).
  3. Кликнуть Харви → compliance +1, topic `topicHarvey_SelfCare` / praise.

  **Ожидаемый результат:**
  - Бафф `HarveyMod_CleanBandage`.
  - 50% шанс снять `HarveyMod_WetBandage`.
  - `PendingSelfCareBandageCompliance` → +1 при визите.
  - Topic `topicHarvey_SelfCarePraise` / `topicHarvey_CleanBandage`.

  **Логи SMAPI:** `[SelfCare] CleanBandage применён`, `[SelfCare] CleanBandage: WetBandage снята`, `[SelfCare] +1 compliance за домашнюю повязку`

---

- [ ] **7.2 Warm tea (простуда)**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add HarveyMod_Cold_Acute
  injury_selfcare_tea
  ```

  **Действия:** проверить бафф и HUD.

  **Ожидаемый результат:** `HarveyMod_WarmTea` (+defense/immunity/stamina в CP); topic `topicHarvey_WarmTea` при разговоре; HUD «Тёплый чай согрел...».

  **Логи SMAPI:** `[SelfCare] WarmTea применён`

---

- [ ] **7.3 SelfCare без условий — отказ**

  **Команды:**
  ```text
  injury_reset
  injury_selfcare_bandage
  ```

  **Ожидаемый результат:** `[SelfCare] CleanBandage не применён (нет условий)` — нет открытой раны / wet bandage / infected wound.

  **Логи SMAPI:** `[SelfCare] CleanBandage не применён` или `CleanBandage: нет условий`

---

- [ ] **7.4 Защита от инфекции после bandage**

  **Цель:** self-care снижает шанс WetBandage → Infection.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_debuff_add HarveyMod_WetBandage
  injury_selfcare_bandage
  ```

  **Действия:** дождаться `DayStarted` / проверки `ComplicationManager` несколько дней.

  **Ожидаемый результат:** пониженный шанс инфекции при активной self-care protection; лог защиты.

  **Логи SMAPI:** `[SelfCare] CleanBandage: защита от инфекции на завтра`, `[SelfCare] Защита * использована`

---

## 8. Proximity-реакции

- [ ] **8.1 High compliance — praise**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set 8
  injury_proximity_test compliance_high Mid
  ```

  **Действия:** начать лечение; подойти к Харви (новая локация).

  **Ожидаемый результат:** `Proximity_Compliance_High_*`; topic `topicHarvey_ProximityPraise`.

  **Логи SMAPI:** `[Proximity] Показ облачка`, prefix в `LastReactionReason`

---

- [ ] **8.2 Low compliance — strict**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set -7
  injury_proximity_test compliance_low Mid
  ```

  **Ожидаемый результат:** `Proximity_Compliance_Low_*`; topic `topicHarvey_ProximityStrict`.

  **Логи SMAPI:** `[Proximity] Показ облачка`

---

- [ ] **8.3 Severe / Light injury proximity**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffBadlyHurt
  injury_proximity_test severe High
  ```
  ```text
  injury_reset
  injury_debuff_add buffHurt
  injury_proximity_test light Low
  ```

  **Ожидаемый результат:** `Proximity_Injury_Severe_*` / `Proximity_Injury_Light_*` из CP.

  **Логи SMAPI:** `[Proximity] Показ облачка`

---

- [ ] **8.4 Phase ready / Recovery ready**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_checkup_due buffDeepCuts
  injury_proximity_test readyphase Mid
  ```

  **Ожидаемый результат:** `Proximity_Phase_ReadyNextPhase_*` или `Proximity_Recovery_ReadyRecovery_*`.

  **Логи SMAPI:** `[Proximity] Показ облачка`

---

- [ ] **8.5 Антиспам proximity**

  **Цель:** не более 1 облачка за локацию + кулдаун 120 игр. мин.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffHurt
  ```

  **Действия:**
  1. Подойти к Харви → первое облачко.
  2. Отойти и снова подойти **в той же локации** → второго нет.
  3. Сменить локацию и вернуться **раньше 120 мин** → пропуск по кулдауну.
  4. Подождать 2+ игровых часа или проверить F10: `Cooldown elapsed`.

  **Ожидаемый результат:** второе облачко блокируется.

  **Логи SMAPI:** `[Proximity] Пропуск: уже показано в этой локации`, `[Proximity] Пропуск: кулдаун N/120 игр. мин`

---

## 9. Регрессия старых механик

- [ ] **9.1 Обычное лечение buffHurt**

  **Цель:** нефазовая травма: treatment → recovery → cured.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffHurt
  injury_phase_recovery buffHurt 1
  ```

  **Действия:** клик Харви (лечение) → дождаться recovery / `injury_phase_recovery` → клик (cure).

  **Ожидаемый результат:** `buffHarveyTreatment` → cured topic; предписания LightWork (если назначены); medical plan Minor.

  **Логи SMAPI:** `[TreatmentPlan]`, `Назначены предписания`, стандартный treatment pipeline (`injury_medical_snapshot`)

---

- [ ] **9.2 Фазовое лечение buffDeepCuts**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_phase_ready buffDeepCuts 1
  ```

  **Действия:** лечение → осмотр → advance фаз 1→2→3 → recovery → cure.

  **Ожидаемый результат:** фазовые баффы `HarveyMod_DeepCuts_*`; PhaseTransition-диалоги; checkup между фазами.

  **Логи SMAPI:** `[Checkup] Phase due`, phase advance logs, `[Checkup] Completed`

---

- [ ] **9.3 Смена фазы**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_phase_ready buffDeepCuts 1
  injury_checkup_due buffDeepCuts
  ```

  **Действия:** клик Харви после ready + checkup due.

  **Ожидаемый результат:** `CurrentPhase` +1; новый phase buff; старый снят.

  **Логи SMAPI:** `injury_phase_list` / F10 показывает phase 2/3

---

- [ ] **9.4 Выздоровление (cured pipeline)**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffHurt
  injury_phase_recovery buffHurt 1
  injury_phase_cure buffHurt
  ```

  **Действия:** альтернативно — клик при `ReadyForRecovery`.

  **Ожидаемый результат:** травма снята; `topicHurtCured` / cured topic; опционально rehab для тяжёлых.

  **Логи SMAPI:** cure/remove buff logs; `[Rehab] Старт` только для eligible injuries

---

- [ ] **9.5 WetBandage → Infection**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_debuff_add HarveyMod_WetBandage
  ```

  **Действия:** начать лечение; несколько дней без смены повязки / без self-care; дождаться `ComplicationManager` roll.

  **Ожидаемый результат:** переход к инфекции или письмо `HarveyMod_WetBandageInfection`; topic `topicHarvey_WetBandage`.

  **Логи SMAPI:** complication/infection logs в `ComplicationManager`; mail scheduled

---

- [ ] **9.6 DirtyWound в шахте**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_mine_dirty_debug
  ```

  **Действия:** начать лечение; провести 30+ игровых минут в Mine / нарушить NoMine 2+ раза.

  **Ожидаемый результат:** `HarveyMod_DirtyWound`; topic `topicHarvey_DirtyWound`; proximity `Proximity_Complication_DirtyWound_*`.

  **Логи SMAPI:** dirty wound apply log; `[Prescription] NoMine violation #2+`

---

- [ ] **9.7 MineForbidden (Severe chain)**

  **Цель:** регрессия запрета шахты не сломана новыми предписаниями.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffBadlyHurt
  injury_mine_forbidden_clear
  ```

  **Действия:** войти в шахту → предупреждение; сон → письмо `mailHarveyMineForbidden` → дебафф `HarveyMod_MineForbidden`.

  **Ожидаемый результат:** перехват входа; отдельно от `HarveyMod_Prescription_NoMine`.

  **Логи SMAPI:** mine forbidden logs; см. [`mine-forbidden-test-cases.md`](mine-forbidden-test-cases.md)

---

- [ ] **9.8 Mine rescue event**

  **Команды:**
  ```text
  injury_reset
  injury_debug_mine_rescue
  ```

  **Действия:** переспать (`DayStarted`) → CP-событие rescue; topic `topicMineInjuryRescue`.

  **Ожидаемый результат:** событие из CP; при proximity + Severe — **приоритет** forced hosp path над обычным облачком.

  **Логи SMAPI:** `⚠️ Харви: proximity-предупреждение перед госпитализацией`, `ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ`

---

- [ ] **9.9 Forced hospitalization**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffBadlyHurt
  injury_debug_mine_rescue
  ```

  **Действия:**
  1. Rescue + topic Severe.
  2. Дважды подойти к Харви в proximity (предупреждение → госпитализация).

  **Ожидаемый результат:** warp в Hospital; `HospitalizationManager.StartForcedHospitalizationWithExplanation`; лечение продолжается в клинике.

  **Логи SMAPI:** `⚠️ Харви обнаружил раны после обморока в шахте → ПРИНУДИТЕЛЬНАЯ ГОСПИТАЛИЗАЦИЯ`

---

- [ ] **9.10 Отсутствие спама proximity-реакций**

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_compliance_set -8
  injury_prescription_add HarveyMod_Prescription_NoMine buffDeepCuts 7
  ```

  **Действия:** нарушить предписание + low compliance + подойти к Харви несколько раз за минуту.

  **Ожидаемый результат:** одно облачко; приоритет контекста (prescription > compliance > injury); без DialogueBox.

  **Логи SMAPI:** один `[Proximity] Показ облачка`; далее `[Proximity] Пропуск: ...`

---

- [ ] **9.11 Сохранение / загрузка на следующий день**

  **Цель:** state предписаний, compliance, rehab, checkup tracking переживает save/load.

  **Команды:**
  ```text
  injury_reset
  injury_debuff_add buffDeepCuts
  injury_prescription_add HarveyMod_Prescription_KeepDry buffDeepCuts 5
  injury_compliance_set 4
  injury_checkup_due buffDeepCuts
  injury_rehab_start buffConcussion 3
  injury_prescription_list
  injury_rehab_status
  ```

  **Действия:**
  1. Записать F10 / `injury_debug_dump`.
  2. Сохраниться и выйти **или** переспать.
  3. Загрузить save / новый день.
  4. Снова `injury_prescription_list`, `injury_rehab_status`, F10.

  **Ожидаемый результат:** все поля `InjuryState` (prescriptions, compliance, rehab, checkup counters, proximity minute) восстановлены; баффы на месте.

  **Логи SMAPI:** при load — `TreatmentComplianceScore clamped` (только если legacy); без полного сброса state

---

## Журнал прогона

| Дата | Тестер | Версия мода | Сценарии (номера) | Pass/Fail | Заметки |
|------|--------|-------------|-------------------|-----------|---------|
| | | | | | |

---

## Связанные документы

- [`testing/FOR_TEST.md`](testing/FOR_TEST.md) — общие команды и жизненный цикл травмы
- [`proximity-reactions-test.md`](proximity-reactions-test.md) — детали proximity и fallback-цепочки
- [`mine-forbidden-test-cases.md`](mine-forbidden-test-cases.md) — запрет шахты
- [`testing/manual-test-scenarios-topics-mail.md`](testing/manual-test-scenarios-topics-mail.md) — topics и почта
