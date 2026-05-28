# Harvey Overhaul Injury — индекс тестирования

> **Главная точка входа** для нового Cursor-чата по QA мода.  
> Базовые правила: [00-ai-testing-rules.md](00-ai-testing-rules.md)  
> MCP: [injury-mcp.md](injury-mcp.md) · [stardew-mcp.md](stardew-mcp.md)

**Моды:** C# `HarveyOverhaulInjury` + CP `HarveyOverhaul [CP]`  
**Перед изолированным TC:** `injury_reset` → подготовка StardewMCP → команды мода → assert (`injury_phase_list`, dump-команды) → F10 / SMAPI log при UI/событиях.

---

## Карта документов

| # | Файл | Содержание |
|---|------|------------|
| 00 | [00-ai-testing-rules.md](00-ai-testing-rules.md) | Цикл MCP/SMAPI, ограничения, формат TC |
| 01 | [01-csharp-mechanics-inventory.md](01-csharp-mechanics-inventory.md) | C# ID, state, пайплайны, пробелы QA |
| 02 | [02-cp-content-inventory.md](02-cp-content-inventory.md) | CP buff/event/mail ↔ C# |
| 03 | [03-existing-debug-commands.md](03-existing-debug-commands.md) | Поведение core `injury_*` |
| 04 | [04-missing-debug-commands.md](04-missing-debug-commands.md) | Спецификация будущих команд |
| 05 | [05-debug-dump-commands.md](05-debug-dump-commands.md) | P0 read-only: state/buff/topic/validate |
| 06 | [06-debug-setup-commands.md](06-debug-setup-commands.md) | P0 setup: complication/topic/hospital/age |
| 07 | [07-smoke-save-tests.md](07-smoke-save-tests.md) | Smoke, команды, save/load |
| 08 | [08-injury-treatment-tests.md](08-injury-treatment-tests.md) | Простые + фазовые травмы, регрессии фаз |
| 09 | [09-complication-tests.md](09-complication-tests.md) | Осложнения, эскалация, neglect |
| 10 | [10-mine-passout-tests.md](10-mine-passout-tests.md) | Шахта, MineForbidden, pass-out |
| 11 | [11-hospital-proximity-events-tests.md](11-hospital-proximity-events-tests.md) | Госпитализация, proximity, CP smoke |
| — | [main-injury-testcases.md](main-injury-testcases.md) | MainInjury + complications (сценарии 1–11) |
| — | [FOR_TEST.md](FOR_TEST.md) | Справочник команд и HUD |

---

## Порядок запуска тестов

Рекомендуемая последовательность прогона (от инфраструктуры к E2E). Каждый блок — отдельный чат или сессия; внутри блока — `injury_reset` перед каждым изолированным TC.

| Этап | Что прогонять | Файл(ы) | Зачем |
|------|---------------|---------|-------|
| **1. Smoke** | Запуск мода, StardewMCP | [07](07-smoke-save-tests.md) `HOI-SMOKE-*` | Игра и MCP живы |
| **2. Validate buffs** | `injury_validate_buffs` | [07](07-smoke-save-tests.md) `HOI-CMD-003` | Регресс CP↔C# до TC |
| **3. Commands** | reset, debuff_list, reset+complication | [07](07-smoke-save-tests.md) `HOI-CMD-*` | Базовые `injury_*` |
| **4. Simple injuries** | hurt, badly hurt, surgical | [08](08-injury-treatment-tests.md) `HOI-SIMPLE-*` | Simple treatment pipeline |
| **5. Phased injuries** | 11 фазовых + REG | [08](08-injury-treatment-tests.md) `HOI-PHASE-*`, `HOI-PHASE-REG-*` | Фазы, Ready*, recovery |
| **6. Complications** | Wet/Dirty/Infection/Neglect/… | [09](09-complication-tests.md) `HOI-COMP-*` | Осложнения отдельно от main |
| **7. MainInjury** | Приоритет, upgrade, эскалация | [main-injury-testcases.md](main-injury-testcases.md) `HOI-MI-*` | Одна main + complications |
| **8. Mine** | Severe warning, Forbidden, interception | [10](10-mine-passout-tests.md) `HOI-MINE-*` | Шахта без pass-out |
| **9. Passout** | HP, rescue, exhaustion, Town | [10](10-mine-passout-tests.md) `HOI-PASSOUT-*` | PassOutHandler |
| **10. Hospitalization** | Forced hosp, bed, discharge | [11](11-hospital-proximity-events-tests.md) `HOI-HOSP-*` | HospitalizationManager |
| **11. Proximity** | Облачко, антиспам, не лечит | [11](11-hospital-proximity-events-tests.md) `HOI-PROX-*` | HarveyReactionManager |
| **12. CP events** | `debug ebi` smoke | [11](11-hospital-proximity-events-tests.md) `HOI-CP-*` | Cutscenes, black screen |
| **13. Save/load** | Phase + complication + reset после reload | [07](07-smoke-save-tests.md) `HOI-SAVE-*` | Персистентность |
| **14. Regressions** | Фазовые gates, orphan buff, hosp+event | [08](08-injury-treatment-tests.md) REG, [11](11-hospital-proximity-events-tests.md) `HOI-HOSP-007` | Edge cases |

**Справочники (по необходимости, не прогон):** [01](01-csharp-mechanics-inventory.md), [02](02-cp-content-inventory.md), [03](03-existing-debug-commands.md), [05](05-debug-dump-commands.md), [06](06-debug-setup-commands.md).

---

## Таблица всех test IDs

Легенда колонок:

- **Ручной клик** — клик по Harvey, cutscene, почта UI, save/load title (если MCP не покрывает).
- **StardewMCP** — хотя бы один tool из `user-stardew` для подготовки или assert.
- **Debug command** — хотя бы одна SMAPI/Injury MCP `injury_*` (кроме чистого `debug ebi`).

### Smoke и команды

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-SMOKE-001 | [07](07-smoke-save-tests.md) | Запуск SMAPI + CP + Data/Buffs | P0 | нет | нет | опц. `injury_validate_buffs` | [ ] |
| HOI-SMOKE-002 | [07](07-smoke-save-tests.md) | Связь StardewMCP (5 tools) | P0 | нет | да | нет | [ ] |
| HOI-CMD-001 | [07](07-smoke-save-tests.md) | `injury_reset` полный сброс | P0 | нет | опц. | да | [ ] |
| HOI-CMD-002 | [07](07-smoke-save-tests.md) | `injury_debuff_list` read-only | P1 | нет | нет | да | [ ] |
| HOI-CMD-003 | [07](07-smoke-save-tests.md) | `injury_validate_buffs` gate | P0 | нет | нет | да | [ ] |

### Save / load

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-SAVE-001 | [07](07-smoke-save-tests.md) | Phase buff + TreatmentStarted после сна/load | P0 | да (B2 save) | да | да | [ ] |
| HOI-SAVE-002 | [07](07-smoke-save-tests.md) | `ActiveComplications` после сна/load | P1 | да (B2) | да | да | [ ] |
| HOI-SAVE-003 | [07](07-smoke-save-tests.md) | `injury_reset` после reload сейва | P0 | да | опц. | да | [ ] |

### Простые травмы

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-SIMPLE-001 | [08](08-injury-treatment-tests.md) | `buffHurt` → cure 2 дня | P0 | да / `injury_harvey_click` | да | да | [ ] |
| HOI-SIMPLE-002 | [08](08-injury-treatment-tests.md) | `buffBadlyHurt` → intensive 4 дня | P1 | да | да | да | [ ] |
| HOI-SIMPLE-003 | [08](08-injury-treatment-tests.md) | `buffSurgicalWound` → post-surgical 7 дн. | P2 | да | да | да | [ ] |

### Фазовые травмы

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-PHASE-001 | [08](08-injury-treatment-tests.md) | `buffSprainedAnkle` 2 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-002 | [08](08-injury-treatment-tests.md) | `buffBruisedRibs` 2 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-003 | [08](08-injury-treatment-tests.md) | `buffBackStrain` 2 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-004 | [08](08-injury-treatment-tests.md) | `buffDeepCuts` 3 фазы (эталон) | P0 | да | да | да | [ ] |
| HOI-PHASE-005 | [08](08-injury-treatment-tests.md) | `buffBurnWounds` 2 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-006 | [08](08-injury-treatment-tests.md) | `buffInfectedWound` 2 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-007 | [08](08-injury-treatment-tests.md) | `buffTornMuscles` 3 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-008 | [08](08-injury-treatment-tests.md) | `buffConcussion` 3 фазы (+ hosp config) | P1 | да | да | да | [ ] |
| HOI-PHASE-009 | [08](08-injury-treatment-tests.md) | `buffFracturedBone` 3 фазы | P1 | да | да | да | [ ] |
| HOI-PHASE-010 | [08](08-injury-treatment-tests.md) | `buffShrapnelWounds` 3 фазы | P2 | да | да | да | [ ] |
| HOI-PHASE-011 | [08](08-injury-treatment-tests.md) | `buffCold` 2 фазы | P2 | да | да | да | [ ] |

### Регрессии фаз

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-PHASE-REG-001 | [08](08-injury-treatment-tests.md) | Recovery на не последней фазе блокируется | P0 | да | да | да | [ ] |
| HOI-PHASE-REG-002 | [08](08-injury-treatment-tests.md) | `phase_ready` на simple не ломает cure | P1 | нет | да | да | [ ] |
| HOI-PHASE-REG-003 | [08](08-injury-treatment-tests.md) | Повторный клик после recovery | P1 | да | да | да | [ ] |
| HOI-PHASE-REG-004 | [08](08-injury-treatment-tests.md) | Потеря phase buff → BuffRestore | P1 | да (load) | да | да | [ ] |
| HOI-PHASE-REG-005 | [08](08-injury-treatment-tests.md) | DebuffState без buff → valid:no | P1 | опц. | нет | да | [ ] |

### MainInjury (модель одной основной травмы)

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-MI-001 | [main-injury](main-injury-testcases.md) §1 | Базовое наложение main | P0 | нет | опц. | да | [ ] |
| HOI-MI-002 | [main-injury](main-injury-testcases.md) §2 | Блокировка второй main | P0 | нет | нет | да | [ ] |
| HOI-MI-003 | [main-injury](main-injury-testcases.md) §3 | Upgrade hurt→badly hurt | P1 | нет | нет | да | [ ] |
| HOI-MI-003b | [main-injury](main-injury-testcases.md) §3b | Upgrade заблокирован в лечении | P1 | да | да | да | [ ] |
| HOI-MI-004 | [main-injury](main-injury-testcases.md) §4 | DirtyWound осложнение | P1 | опц. | да (mine) | да | [ ] |
| HOI-MI-004b | [main-injury](main-injury-testcases.md) §4b | DirtyWound при фазовом лечении | P1 | да | да | да | [ ] |
| HOI-MI-004c | [main-injury](main-injury-testcases.md) §4c | DirtyWound neg (не DirtyInMines) | P1 | да (mine) | да | да | [ ] |
| HOI-MI-005 | [main-injury](main-injury-testcases.md) §5 | DirtyWound→InfectedWound | P0 | нет | да | да | [ ] |
| HOI-MI-005b | [main-injury](main-injury-testcases.md) §5b | WetBandage neg (без лечения) | P0 | да (rain) | да | да | [ ] |
| HOI-MI-005c | [main-injury](main-injury-testcases.md) §5c | WetBandage pos (treated infected) | P1 | да | да | да | [ ] |
| HOI-MI-005d | [main-injury](main-injury-testcases.md) §5d | WetBandage neg (fracture) | P1 | да (rain) | да | да | [ ] |
| HOI-MI-006 | [main-injury](main-injury-testcases.md) §6 | Фазовое лечение у Harvey | P0 | да | да | да | [ ] |
| HOI-MI-007 | [main-injury](main-injury-testcases.md) §7 | Полное выздоровление | P1 | да | да | да | [ ] |
| HOI-MI-008a | [main-injury](main-injury-testcases.md) §8a | Severe по MainInjury | P0 | опц. | да | да | [ ] |
| HOI-MI-008b | [main-injury](main-injury-testcases.md) §8b | PainFlare ≠ severe | P1 | опц. | да (storm) | да | [ ] |
| HOI-MI-009 | [main-injury](main-injury-testcases.md) §9 | Save/load main+state | P0 | да | да | да | [ ] |
| HOI-MI-010 | [main-injury](main-injury-testcases.md) §10 | Миграция старого сейва | P2 | да | нет | да | [ ] |
| HOI-MI-011 | [main-injury](main-injury-testcases.md) §11 | NeglectStrikes per-injury | P0 | нет | да | да | [ ] |

### Осложнения (детальный чеклист)

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-COMP-001 | [09](09-complication-tests.md) | WetBandage QA add + TreatComplications | P0 | да | да | да | [ ] |
| HOI-COMP-002 | [09](09-complication-tests.md) | WetBandage от дождя (gameplay) | P1 | да (ожидание) | да | да | [ ] |
| HOI-COMP-003 | [09](09-complication-tests.md) | WetBandage без StartTreatment | P0 | да (rain) | да | да | [ ] |
| HOI-COMP-004 | [09](09-complication-tests.md) | DirtyWound в шахте | P1 | да (mine) | да | да | [ ] |
| HOI-COMP-005 | [09](09-complication-tests.md) | DirtyWound neg (не DirtyInMines) | P2 | да (mine) | да | да | [ ] |
| HOI-COMP-006 | [09](09-complication-tests.md) | DirtyWound→InfectedWound | P0 | нет | да | да | [ ] |
| HOI-COMP-007 | [09](09-complication-tests.md) | WetBandage→InfectedWound | P1 | нет | да | да | [ ] |
| HOI-COMP-008 | [09](09-complication-tests.md) | WetStitches (pool + surgical) | P2 | да | да | да | [ ] |
| HOI-COMP-009 | [09](09-complication-tests.md) | Neglect strikes + treat | P1 | да | да | да | [ ] |
| HOI-COMP-010 | [09](09-complication-tests.md) | NeglectStrikes per-injury | P0 | нет | да | да | [ ] |
| HOI-COMP-011 | [09](09-complication-tests.md) | PainFlare от грозы | P1 | опц. | да | да | [ ] |
| HOI-COMP-012 | [09](09-complication-tests.md) | AllergicRash (нет C# autotrigger) | P2 | да | да | да | [ ] |

### Шахта (MineForbidden)

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-MINE-001 | [10](10-mine-passout-tests.md) | Severe + mine → mail + Forbidden | P0 | да (почта, HUD) | да | да | [ ] |
| HOI-MINE-002 | [10](10-mine-passout-tests.md) | Light injury — мягкий warning | P1 | да (HUD) | да | да | [ ] |
| HOI-MINE-003 | [10](10-mine-passout-tests.md) | Повторный вход — не спамить | P1 | да (HUD) | да | да | [ ] |
| HOI-MINE-004 | [10](10-mine-passout-tests.md) | Forbidden истекает по дням | P1 | нет | да | да | [ ] |
| HOI-MINE-005 | [10](10-mine-passout-tests.md) | CP interception, no black screen | P0 | да (cutscene) | да | да + `debug ebi` | [ ] |

### Обмороки (PassOut)

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-PASSOUT-001 | [10](10-mine-passout-tests.md) | Critical HP → badly hurt | P1 | да (event) | да | да | [ ] |
| HOI-PASSOUT-002 | [10](10-mine-passout-tests.md) | Mine death rescue pipeline | P0 | да (cutscene) | да | да | [ ] |
| HOI-PASSOUT-003 | [10](10-mine-passout-tests.md) | topicMineInjuryRescue → forced hosp | P0 | нет | да | да | [ ] |
| HOI-PASSOUT-004 | [10](10-mine-passout-tests.md) | Exhaustion stamina≤−15 | P2 | да (gameplay) | частично | да | [ ] |
| HOI-PASSOUT-005 | [10](10-mine-passout-tests.md) | Late Town collapse | P1 | да (pass-out, mail) | да | да | [ ] |

### Госпитализация

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-HOSP-001 | [11](11-hospital-proximity-events-tests.md) | Forced hospitalization старт | P0 | да (диалог) | да | да | [ ] |
| HOI-HOSP-002 | [11](11-hospital-proximity-events-tests.md) | Warp на койку BedX/Y | P0 | нет | да | да | [ ] |
| HOI-HOSP-003 | [11](11-hospital-proximity-events-tests.md) | Выход заблокирован до min stay | P1 | да (HUD) | да | да | [ ] |
| HOI-HOSP-004 | [11](11-hospital-proximity-events-tests.md) | Discharge после срока | P1 | да | да | да | [ ] |
| HOI-HOSP-005 | [11](11-hospital-proximity-events-tests.md) | Hospital activities interval | P2 | да (диалоги) | да | да | [ ] |
| HOI-HOSP-006 | [11](11-hospital-proximity-events-tests.md) | `injury_hospital_discharge` | P1 | нет | да | да | [ ] |
| HOI-HOSP-007 | [11](11-hospital-proximity-events-tests.md) | Regression: event + exit | P1 | да (cutscene) | да | да + `debug ebi` | [ ] |

### Proximity

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-PROX-001 | [11](11-hospital-proximity-events-tests.md) | Облачко при травме рядом | P1 | да (подход) | да | да | [ ] |
| HOI-PROX-002 | [11](11-hospital-proximity-events-tests.md) | Proximity не StartTreatment | P0 | да | да | да | [ ] |
| HOI-PROX-003 | [11](11-hospital-proximity-events-tests.md) | Антиспам 1×/локация + 120 мин | P1 | да | да | да | [ ] |
| HOI-PROX-004 | [11](11-hospital-proximity-events-tests.md) | Смена локации сброс per-location | P2 | да | да | да | [ ] |

### CP-события (smoke)

| ID | Файл | Механика | P | Ручной | StardewMCP | Debug | Статус |
|----|------|----------|---|--------|------------|-------|--------|
| HOI-CP-001 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyMineRescueDating` | P0 | да | да | `debug ebi` | [ ] |
| HOI-CP-002 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyMineRescue` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-003 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyMinorMineRescue` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-004 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyMineInterception` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-005 | [11](11-hospital-proximity-events-tests.md) | `eventHarveySkullCavePrevention` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-006 | [11](11-hospital-proximity-events-tests.md) | `eventStayInHospital` (orphan) | P2 | да | да | `debug ebi` | [ ] |
| HOI-CP-007 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyLateNightCollapse` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-008 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyCheckHealthFarmer` | P2 | да | да | `debug ebi` | [ ] |
| HOI-CP-009 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyEmergencyCare` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-010 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyExhaustion` | P1 | да | да | `debug ebi` | [ ] |
| HOI-CP-011 | [11](11-hospital-proximity-events-tests.md) | `eventHarveyTreatmentCollapse` (orphan) | P2 | да | да | `debug ebi` | [ ] |

**Итого executable TC:** 88 (+ шаблон `HOI-PHASE-TEMPLATE` в [08](08-injury-treatment-tests.md) — не отдельный прогон).

---

## Быстрый P0-набор (~30 минут)

Минимальный набор, который ловит самые опасные классы багов: инфраструктура, CP↔C#, одна main, gates лечения, осложнения, шахта, госпитализация, персистентность, CP cutscene.

| # | ID | Команды / действия | PASS если |
|---|-----|-------------------|-----------|
| 1 | HOI-CMD-003 | `injury_validate_buffs` | `result=OK` |
| 2 | HOI-CMD-001 | reset → add main+comp → reset | Пустой state после 2-го reset |
| 3 | HOI-PHASE-004 | reset → `buffDeepCuts` → click → ready×2 → recovery → click | 3 фазы + `buffHarveyCare`; main cleared |
| 4 | HOI-COMP-003 | reset → `buffDeepCuts` **без** click → rain 60m | Нет WetBandage |
| 5 | HOI-COMP-006 | reset → deep cuts → `complication_add DirtyWound age=3` → `advance_day` | `MainInjuryId=buffInfectedWound`; Dirty снят |
| 6 | HOI-MINE-001 | reset → `buffBadlyHurt` → warp mine → advance_day | `HarveyMod_MineForbidden` + mail |
| 7 | HOI-PASSOUT-002 | reset → mine 0 HP **или** `injury_debug_mine_rescue` → advance_day | `NeedsMineRescueEvent` / rescue event утром |
| 8 | HOI-HOSP-001 | reset → `topic_add topicMineInjuryRescue` → `buffBadlyHurt` → warp Hospital | `IsHospitalized=True`; topic снят |
| 9 | HOI-SAVE-001 | reset → deep cuts → click → `advance_day` **или** save/load | Phase buff + `TreatmentStarted` сохранены |
| 10 | HOI-MINE-005 | Forbidden active → warp Mine → `debug ebi eventHarveyMineInterception` | Нет black screen / NRE |

**Опционально в те же 30 мин:** HOI-PHASE-REG-001 (recovery gate), HOI-CP-001 (dating mine rescue smoke).

**Цикл одного TC:**

```
injury_reset                    → user-harvey-injury
StardewMCP (warp/time/weather)  → user-stardew
injury_debuff_add / setup / phase_* 
injury_state_dump + injury_phase_list
[ручной шаг если в таблице]
записать [x] в журнал файла + таблицу выше
```

---

## Что делать при падении теста

1. **Не менять код** во время прогона — только зафиксировать FAIL; фикс — отдельный минимальный diff под конкретный TC.
2. Сохранить артефакты **до** `injury_reset` следующего TC.
3. Обновить статус в журнале исходного чеклиста (`- [x]` / заметка FAIL) и строку в таблице выше.
4. В конце чата — блок «Что читать следующему чату» (см. [00-ai-testing-rules.md](00-ai-testing-rules.md) §6).

### Шаблон bug report

```markdown
## Bug: <краткое название>

**Test ID:** HOI-XXXX-NNN
**Файл TC:** docs/testing/NN-....md

### Шаги
1. injury_reset
2. …
3. …

### Ожидалось
- MainInjuryId=…
- …

### Фактически
- MainInjuryId=…
- …

### SMAPI log
```
[вставить релевантные строки: [MainInjury], [Complication], [MineForbidden], [PassOut], [QA], Error]
```

### Debug HUD (F10)
- Main injury: …
- valid: …
- Complications: …
- LastClickDebug: …

### InjuryState dump
```
[вывод injury_state_dump или injury_debug_dump]
```

### Active buffs
```
[вывод injury_buff_dump]
```

### Screenshot
- (приложить: HUD, cutscene, чёрный экран, диалог)

### Подозреваемый файл/метод
- `Path/Class.Method` — почему
```

---

## Что нельзя автоматизировать полностью

| Область | Почему | Обход в QA |
|---------|--------|------------|
| **Клик по Harvey** | StardewMCP не умеет action-click на NPC | `injury_harvey_click` для механики FSM; E2E — вручную |
| **Текст диалога / тон** | Нет assert на локализацию и tier | Визуально; `injury_proximity_test` только для proximity lines |
| **Cutscene после CP event** | `debug ebi` ≠ полный gameplay trigger | Smoke: нет NRE/black screen; полный pipeline — pass-out/mine TC |
| **Чёрный экран после CP event** | Визуальный баг fade/viewport | HOI-MINE-005, HOI-CP-*; esc + SMAPI log |
| **Маршруты NPC в event** | Pathing ваниль/CP script | Визуально при `debug ebi` |
| **Почта в UI** | Нет MCP read mailbox | Утром вручную; assert topic/mail id в log при `SendLetters` |
| **Save/load через title** | Injury MCP не триггерит меню | HOI-SAVE-003, HOI-MI-009 — вручную |
| **Dating/married Harvey** | `set_npc_relationship` ≠ dating flag | Отдельный dating-сейв; `get_npc_info` |
| **Истощение stamina** | Нет `set_stamina` в StardewMCP | HOI-PASSOUT-004 — gameplay или SKIP |
| **60+ мин exposure** (дождь, шахта) | Долго в realtime | `injury_rain_debug`, `injury_complication_add`, `injury_test_age_*` |
| **eventsSeen / one-shot CP** | `injury_reset` не сбрасывает | Новый сейв или документировать повторный `ebi` |

---

## Следующие улучшения (backlog QA)

| Улучшение | Зачем | Статус |
|-----------|-------|--------|
| Больше QA-команд | Закрыть пробелы из [04-missing-debug-commands.md](04-missing-debug-commands.md) | Частично: [05](05-debug-dump-commands.md), [06](06-debug-setup-commands.md) |
| `injury_test_setup <scenarioId>` | Один вызов baseline (`deep_cuts_treated`, `dirty_wound`, …) | Спецификация в [04](04-missing-debug-commands.md), не реализовано |
| Валидатор CP event ids | Сверка `EventIds` C# ↔ `Data/Events` CP | Вручную через [02](02-cp-content-inventory.md); отдельной команды нет |
| Проверка hospital bed walkability | HOI-HOSP-002 автоматом: bed tile + `get_walkable_tiles` | Сейчас StardewMCP вручную в TC |
| Запуск daily checks вручную | `CheckTreatmentCompletion`, infection roll, neglect без `advance_day` | Нет `injury_run_daily_checks`; только `advance_day` |
| `injury_force_dirty_wound` / `injury_pass_out_sim` | Gameplay без ожидания | В [04](04-missing-debug-commands.md) P1/P2 |
| Зеркало всех QA tools в Injury MCP | Cursor `CallMcpTool` | Сверить с [injury-mcp.md](injury-mcp.md) |
| `TESTING_INDEX.md` в [README.md](README.md) | Ссылка «начни здесь» | Обновить README при следующем редактировании |

---

## Быстрые ссылки на assert-команды

| Задача | Команда |
|--------|---------|
| MainInjury + phases | `injury_phase_list` |
| Полный state | `injury_state_dump` |
| Buffs на игроке | `injury_buff_dump` |
| Topics | `injury_topic_dump` |
| CP↔C# buff registry | `injury_validate_buffs` |
| Осложнение с eligibility | `injury_complication_add` / `remove` |
| Возраст comp/injury | `injury_test_age_complication` / `injury_test_age_injury` |
| Госпитализация | `injury_hospital_status` / `injury_hospital_discharge` |
| Симуляция клика Harvey | `injury_harvey_click` |
| Mine rescue flags | `injury_debug_mine_rescue` |
| Mine dirty read-only | `injury_mine_dirty_debug` |

---

## Резюме

**Этот индекс — главный файл для нового Cursor-чата. Начинай отсюда.**

1. Прочитай [00-ai-testing-rules.md](00-ai-testing-rules.md).
2. Прогони **P0-набор (~30 мин)** из раздела выше или выбери блок по **порядку запуска**.
3. Открой детальный чеклист по ID из **таблицы test IDs**.
4. Фиксируй PASS/FAIL в журнале файла и в колонке **Статус** этой таблицы.
5. При FAIL — шаблон bug report; не держи результат только в чате.
