# Recovery Plan — чеклист QA

План восстановления — **не бафф**, а save-state `InjuryState.RecoveryPlan` + окно StardewUI (клавиша **H**).

## Подготовка

- [ ] SMAPI + мод собран и загружен
- [ ] StardewMCP / Injury MCP при необходимости
- [ ] Команды: `injury_plan_show`, `injury_plan_refresh`, `injury_plan_violate`, `injury_plan_clear`

---

## 1. Лёгкая травма (`buffHurt`)

- [ ] **Действие:** `injury_reset` → `injury_debuff_add buffHurt` → клик по Харви → StartTreatment
- [ ] **SMAPI:** `injury_plan_show` → `IsActive=true`, есть tasks
- [ ] **Ожидание:** нет нового баффа `RecoveryPlan` / `HarveyMod_RecoveryPlan` в `injury_buff_dump`
- [ ] **UI (H):** план виден, пустое состояние не показывается

---

## 2. Фазовая травма (`buffDeepCuts`)

- [ ] **Действие:** `injury_debuff_add buffDeepCuts` → лечение у Харви
- [ ] **SMAPI:** `injury_plan_show` → `day 1/3` (или длительность фазы 1), `phase 1/3`
- [ ] **Действие:** `injury_phase_advance` / дождаться ReadyForNextPhase
- [ ] **Ожидание:** `status=NeedsHarveyTalk`, задача `VisitHarveyIfReady`
- [ ] **UI:** «Фаза: Острая фаза», «день X из Y»

---

## 3. Шахта (severe)

- [ ] **Действие:** main = `buffBadlyHurt` или severe + `warp_to_mine`
- [ ] **SMAPI:** `injury_plan_show` → violation `EnteredMinesDuringRecovery`
- [ ] **Ожидание:** `status=Urgent` (severe + шахта)
- [ ] **UI:** задача «Не ходить в шахту…» помечена ✗
- [ ] **HUD:** одна строка про нарушение (не спам)

---

## 4. Stamina

- [ ] **Действие:** опустить stamina &lt; 15% (MCP health/stamina или в игре)
- [ ] **SMAPI:** violation `LowStaminaDuringRecovery`
- [ ] **Ожидание:** без повторного HUD каждые 10 сек

---

## 5. HP

- [ ] **Действие:** HP &lt; 35% → warning violation
- [ ] **Действие:** HP &lt; 15% → `status=Urgent`
- [ ] **UI:** задача «Вернуться домой…»

---

## 6. Поздний сон

- [ ] **Действие:** активен `SleepBeforeMidnight`, лечь после 00:00
- [ ] **SMAPI:** после сна / `injury_plan_show` → `LateSleepDuringRecovery`
- [ ] **Напоминание:** в 23:00 одна строка от Харви (TimeEventHandler)

---

## 7. Осложнение

- [ ] **Действие:** `injury_debuff_add HarveyMod_WetBandage` (или complication MCP)
- [ ] **Ожидание:** задача `TreatComplications`, `NeedsHarveyTalk`
- [ ] **UI:** строка «Есть осложнение: …»

---

## 8. Выздоровление

- [ ] **Действие:** завершить лечение (CompleteRecovery у Харви)
- [ ] **SMAPI:** `injury_plan_show` → inactive / `(none)`
- [ ] **Ожидание:** violations очищены, HUD «План восстановления завершён»

---

## 9. StardewUI отсутствует

- [ ] **Действие:** отключить `focustense.StardewUI`
- [ ] **Ожидание:** мод не падает; **H** → HUD «нет активного плана» или fallback
- [ ] **SMAPI:** `injury_plan_show` работает

---

## 10. UX

- [ ] Активных баффов не прибавилось из-за плана
- [ ] Правила понятны из окна (≤6 задач)
- [ ] HUD не спамит (1×/день на тип)
- [ ] F10 compact: блок `RecoveryPlan: active … Concern=…`
- [ ] Харви не «ругает» каждые 10 сек (только log / 1 HUD)

---

## Быстрые команды SMAPI

```
injury_reset
injury_debuff_add buffDeepCuts
injury_plan_show
injury_plan_refresh
injury_plan_violate mines
injury_plan_clear
recovery_plan_status
recovery_violate mine|stamina|health|night|rain
recovery_complete perfect|warnings
recovery_debug
```

---

# Recovery Plan — тесты разных нарушений

- [ ] Старт плана восстановления показывает старую/новую Started-реплику.
- [ ] Вход в шахту с активным планом даёт Mine-нарушение.
- [ ] Mine выбирается главным нарушением, если в тот же день была low stamina.
- [ ] LowStamina сначала даёт warning, если порог предупреждения.
- [ ] LowStamina ломает день, если игрок продолжает до критического порога.
- [ ] LowHealth сразу ломает день и даёт строгую медицинскую реплику.
- [ ] LateNight даёт ночную реплику, не похожую на шахту/HP.
- [ ] Rain даёт реплику про мокрую повязку/риск простуды/сухую одежду.
- [ ] Perfect completion показывается только если не было warning/violation.
- [ ] WithWarnings completion показывается, если были предупреждения, но план завершён.
- [ ] Старый fallback `HarveyMod_RecoveryPlanViolated` работает, если конкретный ключ не найден.
- [ ] Диалоги не спамятся при частых тиках/варпах.
- [ ] Debug HUD показывает LastViolationType, severity и TodayViolationReasons.

### Подсказки для ручной проверки

| Сценарий | Команда / действие | Topic / ожидание |
|----------|-------------------|------------------|
| Mine | `recovery_violate mine` → клик Харви | `HarveyMod_RecoveryPlanViolated_Mine` |
| Stamina warning | опустить stamina до 25%, подождать HUD | HUD без topic |
| Stamina violation | `recovery_violate stamina` | `HarveyMod_RecoveryPlanViolated_LowStamina` |
| Health | `recovery_violate health` | `HarveyMod_RecoveryPlanViolated_LowHealth` |
| Late night | `recovery_violate night` или сон после 00:00 | `HarveyMod_RecoveryPlanViolated_LateNight` |
| Rain | `recovery_violate rain` (mild) / дождь 2+ мин | warning / `…_Rain` |
| Perfect end | `recovery_complete perfect` | `HarveyMod_RecoveryPlanCompleted_Perfect` |
| Warnings end | `recovery_complete warnings` | `HarveyMod_RecoveryPlanCompleted_WithWarnings` |
| Приоритет | `recovery_violate stamina` → `recovery_violate mine` | topic остаётся Mine |
| Debug | `recovery_debug` или F10 | `TodayViolationTypes`, `CompletionTopic=…` |
