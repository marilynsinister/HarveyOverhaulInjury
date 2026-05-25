# Чеклист: модель одной основной травмы (MainInjury + Complications)

Дата: 2026-05-25  
Мод: **HarveyOverhaulInjury** (C#)  
Цель: проверить, что в сохранении всегда не более одной **основной** травмы (`MainInjuryId`), осложнения живут отдельно, эскалация инфекции и госпитализация согласованы с main.

---

## Подготовка

| Что | Как |
|-----|-----|
| SMAPI | Консоль открыта |
| Чистый старт сценария | `injury_reset` перед каждым блоком (или новый слот) |
| Список состояния | `injury_phase_list` |
| Debug HUD | F10: compact → full |
| Полный дамп | `injury_debug_dump` |
| Ремонт main (только debug) | `injury_main_set <buffId>`, `injury_main_clear` |
| Принудительная замена main | `injury_debuff_add --force <buffId>` |

### Что смотреть в `injury_phase_list`

- `MainInjuryId: ...`
- `Active main injury valid: yes/no`
- `Complications: ...`

### Что смотреть в debug HUD (F10)

- `Main injury` / `valid`
- `Main injury phase`, `treatment started`
- `ReadyForNextPhase / ReadyForRecovery`
- `Complications`
- `SavedActiveBuffs count`

### Полезные лог-префиксы (SMAPI)

- `[MainInjury]` — установка, замена, блокировка, миграция
- `[Complication] MainInjury=..., complication=...` — осложнения и эскалация

---

## 1. Базовое наложение основной травмы

**Шаги:** `injury_reset` → `injury_debuff_add buffFracturedBone`

**Ожидается:**

- [ ] `MainInjuryId = buffFracturedBone` (`injury_phase_list`)
- [ ] `Active main injury valid: yes`
- [ ] В `ActiveDebuffs` есть `DebuffState` для `buffFracturedBone`
- [ ] До начала лечения активен **базовый** бафф `buffFracturedBone` (не фазовый)
- [ ] `injury_phase_list` показывает строку `buffFracturedBone` с фазой/флагами
- [ ] Debug HUD: `Main injury: buffFracturedBone`

---

## 2. Блокировка второй основной травмы

**Подготовка:** сценарий 1 выполнен, активен `buffFracturedBone`.

**Шаги:** `injury_debuff_add buffDeepCuts` (без `--force`)

**Ожидается:**

- [ ] `buffDeepCuts` **не** становится второй основной травмой
- [ ] `MainInjuryId` остаётся `buffFracturedBone`
- [ ] В SMAPI-логе есть блокировка, например: `[MainInjury] Новая травма заблокирована, уже есть основная: buffFracturedBone, попытка: buffDeepCuts`
- [ ] Команда сообщает об отказе или не меняет main (без `--force` новая main не применяется)

**Дополнительно (опционально):** `injury_debuff_add --force buffDeepCuts` → main заменена на `buffDeepCuts`, старый перелом снят.

---

## 3. Upgrade лёгкой травмы в тяжёлую

**Подготовка:** `injury_reset` → `injury_debuff_add buffHurt`

**Шаги:** `injury_debuff_add buffBadlyHurt`

**Ожидается:**

- [ ] `buffHurt` заменён на `buffBadlyHurt` (старый бафф/состояние сняты)
- [ ] `MainInjuryId = buffBadlyHurt`
- [ ] В логе: `[MainInjury] Основная травма заменена: buffHurt -> buffBadlyHurt`
- [ ] Одновременно **не** активны `buffHurt` и `buffBadlyHurt` как две основные

---

## 4. Осложнение DirtyWound при открытой ране

**Подготовка:** `injury_reset` → `injury_debuff_add buffDeepCuts`

**Шаги:** зайти в шахту с открытой раной и дождаться срабатывания грязной раны  
(или ускорить через exposure / `injury_mine_dirty_debug`, нарушение `NoMine` при severe main — по ситуации)

**Ожидается:**

- [ ] `MainInjuryId` остаётся `buffDeepCuts`
- [ ] Появляется осложнение `HarveyMod_DirtyWound` (бафф + topic)
- [ ] `ActiveComplications` содержит ключ `HarveyMod_DirtyWound`
- [ ] В логе: `[Complication] MainInjury=buffDeepCuts, complication=HarveyMod_DirtyWound`
- [ ] `buffDeepCuts` и `DirtyWound` сосуществуют: main + complication, не две main

---

## 5. Эскалация DirtyWound → InfectedWound

**Подготовка:** сценарий 4, активны `buffDeepCuts` + `HarveyMod_DirtyWound`

**Шаги:** прожить дни без лечения осложнения / дождаться daily-check инфекции  
(день 1: 15%, день 2: 40%, день 3+: 100% roll; можно ускорить сном)

**Ожидается:**

- [ ] `buffDeepCuts` заменён на `buffInfectedWound` (не две основные одновременно)
- [ ] `MainInjuryId = buffInfectedWound`
- [ ] `HarveyMod_DirtyWound` удалён (бафф, `ActiveComplications`, topic)
- [ ] HUD: «Рана инфицирована! Срочно к врачу!»
- [ ] В логе эскалации: `[MainInjury] Основная травма заменена: buffDeepCuts -> buffInfectedWound`
- [ ] При fallback (если замена не удалась): **нет** второй main, есть `PainFlare` + срочный topic/mail

---

## 6. Фазовое лечение у Харви

**Подготовка:** `injury_reset` → `injury_debuff_add buffFracturedBone` (или `buffInfectedWound`)

**Шаги:**

1. Поговорить с Харви → начать лечение
2. Дождаться готовности фазы / `injury_phase_advance <buffId>` (debug)
3. Снова кликнуть по Харви для перехода фазы

**Ожидается:**

- [ ] `MainInjuryId` **не меняется** при смене фазы (остаётся базовый `buffId`)
- [ ] Базовый бафф снимается, активен **фазовый** бафф (`HarveyMod_*_Acute` / `_Treatment` и т.д.)
- [ ] `_injuryManager.GetActiveInjury()` / `injury_phase_list` / debug HUD показывают **базовый** id (`buffFracturedBone`), не phase buff id
- [ ] `Active main injury valid: yes` на протяжении лечения

---

## 7. Полное выздоровление

**Подготовка:** активная фазовая травма **или** добавить осложнение отдельно:

```
injury_debuff_add buffDeepCuts
injury_debuff_add HarveyMod_PainFlare
```

**Шаги:** пройти лечение до конца **или** `injury_phase_cure buffDeepCuts`

**Ожидается:**

- [ ] `DebuffState` для вылеченной травмы удалён
- [ ] Базовый и фазовые баффы этой травмы сняты
- [ ] `MainInjuryId` очищен (`(none)` в `injury_phase_list`)
- [ ] В логе: `[MainInjury] Основная травма завершена: ...`
- [ ] Осложнения (`HarveyMod_PainFlare` и др.), если были, **не** удалены случайно при cure main
- [ ] После cure main осложнения всё ещё в `Complications:` / `ActiveComplications`

---

## 8. Госпитализация и severe по MainInjury

### 8a. Severe определяется по main

**Подготовка:** `injury_reset` → `injury_debuff_add buffFracturedBone` + topic mine rescue (или `injury_debug_mine_rescue`)

**Шаги:** зайти в госпиталь / proximity к Харви с `topicMineInjuryRescue` при `ForceHospitalization: true`

**Ожидается:**

- [ ] Принудительная госпитализация срабатывает (severe main = `buffFracturedBone`)
- [ ] Debug HUD: `Main injury serious: yes` (или эквивалент в full dump)
- [ ] Решение **не** опирается на «любой severe buff на игроке» в обход main

### 8b. PainFlare не запускает госпитализацию

**Подготовка:** `injury_reset` → `injury_debuff_add buffHurt` → `injury_debuff_add HarveyMod_PainFlare`

**Шаги:** проверить `IsMainInjurySerious` через debug HUD; попытаться вызвать mine-rescue hospitalization без severe main

**Ожидается:**

- [ ] `MainInjuryId = buffHurt` (лёгкая травма)
- [ ] `Main injury serious: no`
- [ ] `HarveyMod_PainFlare` есть в complications, но **не** делает состояние severe
- [ ] Принудительная госпитализация **не** стартует только из-за PainFlare

---

## 9. Сохранение и загрузка

**Подготовка:** активная main + осложнение, например:

```
injury_reset
injury_debuff_add buffDeepCuts
injury_debuff_add HarveyMod_DirtyWound
```

**Шаги:** сохранить → выйти в title → загрузить → дождаться `DayStarted` (утро)

**Ожидается:**

- [ ] После save/load `MainInjuryId` сохранён (`buffDeepCuts`)
- [ ] Утром баффы восстановлены (`DayStarted` buff restore)
- [ ] `GetActiveInjury()` / `injury_phase_list` находят ту же основную травму
- [ ] `Active main injury valid: yes`
- [ ] Осложнения сохранены в `ActiveComplications`

---

## 10. Миграция старого сейва

**Симуляция:** сохранение, где `MainInjuryId` пустой, но в `ActiveDebuffs` несколько травм  
(старый сейв до поля main **или** debug: `injury_main_clear` при нескольких active debuffs)

**Шаги:** загрузить сохранение / выполнить `injury_main_clear` → перезагрузить или вызвать код, который читает main (`injury_phase_list`, клик по Харви)

**Ожидается:**

- [ ] При пустом `MainInjuryId` и непустом `ActiveDebuffs` выбирается **самая приоритетная** травма (`MainInjuryPriorityOrder`)
- [ ] В логе миграции: `[MainInjury] Миграция: основная травма = ...`
- [ ] `injury_phase_list`: `MainInjuryId` заполнен, `valid: yes`
- [ ] Осложнения из `KnownComplicationBuffIds` **не** становятся main

---

## Быстрая регрессия (минимум)

Если времени мало, пройти по порядку сценарии **1 → 2 → 3 → 5 → 7 → 8b**.

| # | Команда / действие | Критичный результат |
|---|-------------------|---------------------|
| 1 | `injury_debuff_add buffFracturedBone` | main установлена |
| 2 | `injury_debuff_add buffDeepCuts` | main не дублируется |
| 3 | `buffHurt` → `buffBadlyHurt` | upgrade main |
| 5 | DirtyWound → sleep | infection replaces main |
| 7 | `injury_phase_cure` | main cleared, complications stay |
| 8b | main=hurt + PainFlare | no forced hospital |

---

## Связанные документы

- [manual-test-scenarios-topics-mail.md](testing/manual-test-scenarios-topics-mail.md) — лечение, topics, mail
- [FOR_TEST.md](testing/FOR_TEST.md) — общий справочник debug-команд
- [mines-forbidden-injuries.md](mines-forbidden-injuries.md) — шахта, severe, госпитализация
