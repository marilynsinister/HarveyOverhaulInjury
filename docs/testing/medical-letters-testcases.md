# Medical Letters — тест-кейсы

Конфиг (`HarveyOverhaulInjury/config.json`):

```json
"MedicalLetters": "CriticalOnly",
"SendStoryLetters": true,
"SendRomanticCareLetters": true
```

Логи: `[MedicalLetters]` в SMAPI. QA: `injury_mail_dump`.

## 1. Недосып закрыт в тот же день

1. `injury_reset` → pass-out Town late (или QA-триггер `PassedOutInTown` + `buffSleepy`).
2. Поговорить с Харви / снять topic + buff в тот же день.
3. Сон → утро.
4. **Ожидание:** `mailHarveySleepControl` **не** в почте; в логе `not sent (stale)` или pending пуст.

## 2. DirtyWound вылечена через $action

1. `injury_debuff_add HarveyMod_DirtyWound` + main injury.
2. День ending → pending `InfectionDirty` / routine (только если `MedicalLetters=All`).
3. `$action HarveyOverhaulInjury_TreatComplication HarveyMod_DirtyWound` или dialogue treat.
4. Сон.
5. **Ожидание:** письмо про грязную рану **не** приходит.

## 3. Hidden deepCuts, HarveyAware=false

1. `buffDeepCuts`, `HiddenFromHarvey=true`, `HarveyAware=false`.
2. Попытка queue через Complication/All mode (debug).
3. **Ожидание:** лог `blocked hidden injury mail`; письмо с точным диагнозом не уходит.

## 4. MineForbidden активен

1. Severe injury + mine warning → `MineWarningDay=today`.
2. `MedicalLetters=CriticalOnly`.
3. Сон.
4. **Ожидание:** `mailHarveyMineForbidden` может прийти, пока запрет активен.

## 5. MedicalLetters=Off

1. `MedicalLetters`: `Off` в config.
2. Любой триггер (neglect, treatment plan, sleep).
3. **Ожидание:** нет medical pending; story/romantic CP-триггеры не затронуты C#.

## 6. SendStoryLetters

1. `SendStoryLetters=true`, CP story mail (events) — без изменений C#.
2. Medical routine при `CriticalOnly` — не шлётся из C#.

## StressMeter

- Routine stress treatment start mails: только `MedicalLetters=All`.
- `CompleteTreatment` → romantic `GenericDone` только если `SendRomanticCareLetters=true`.
- `mailHarveyDarknessWorry`: `All` + валидация активного Darkness buff.
