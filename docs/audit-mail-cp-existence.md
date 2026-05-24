# Аудит наличия CP-почты для C# mail ID

Дата: 2026-05-24 (актуализация)  
Источник C#: [audit-mail-csharp.md](./audit-mail-csharp.md)  
CP: `HarveyOverhaul [CP]` → `Data/Mail` (Include в `content.json`).

**Подключённые mail-файлы:** `mail.json`, `mailCare.json`, `mailCure.json`, `mailInjury.json`, `mailStress.json`.

---

## Сводка

| Статус | Кол-во |
|--------|--------|
| ✓ Exact match для всех C# send | **7 / 7** |
| ✗ Отсутствует в CP | **0** |
| ✗ ID mismatch | **0** (C# → `HarveyMod_NeglectWarning`) |
| `MailIds` без send, но в CP | **3** |

**Итог:** все письма, которые C# отправляет, **имеют exact entry в CP**.

---

## Основная таблица — C# send

| Mail ID (C#) | CP | Файл | Контекст C# | Проблема |
|--------------|-----|------|-------------|----------|
| `mailHarveySleepControl` | ✓ | mailInjury.json | PassOutHandler, Town ≥26:00 | **MEDIUM:** нет gate по hearts в C# |
| `mailHarveyMineForbidden` | ✓ | mailInjury.json | Severe + шахта, вечер | — |
| `HarveyMod_DirtyWoundInfection` | ✓ | mailInjury.json | Dirty wound roll | — |
| `HarveyMod_WetBandageInfection` | ✓ | mailInjury.json | Wet bandage roll | — |
| `HarveyMod_TreatmentUrgentReminder` | ✓ | mailInjury.json | Phase neglect +3 | — |
| `HarveyMod_TreatmentFinalWarning` | ✓ | mailInjury.json | Phase neglect −1 | — |
| `HarveyMod_NeglectWarning` | ✓ | mailInjury.json | Phase neglect overdue | **LOW:** дубль `mailHarvey_Neglect` в CP (C# не шлёт) |

---

## Неиспользуемые константы vs CP

| C# `MailIds` | Send? | CP ключ | Заметка |
|--------------|-------|---------|---------|
| `WetCare` | Нет | `HarveyMod_WetCare` | Подключить ComplicationManager или удалить константу |
| `WetStitchesCare` | Нет | `HarveyMod_WetStitchesCare` | То же |
| `InfectionAlert` | Нет | `HarveyMod_InfectionAlert` | Generic; C# шлёт dirty/wet-specific |

---

## Дубли в CP (C# не шлёт)

| CP ключ | Заметка |
|---------|---------|
| `mailHarvey_Neglect` | Legacy ID; C# шлёт `HarveyMod_NeglectWarning` |
| `HarveyMod_InfectionAlert` | Generic infection; C# шлёт `DirtyWoundInfection` / `WetBandageInfection` |
| `mailHarveyMineWarning`, `HarveyMod_MineWarning` | CP/triggers only |

---

## Топ исправлений (статус)

1. ~~4 missing mail entries~~ — **✅ done** (2026-05-23)
2. ~~Neglect ID mismatch~~ — **✅ done** (C# → `HarveyMod_NeglectWarning`)
3. **MEDIUM:** Wire `WetCare` / `WetStitchesCare` или удалить константы — **открыто**
4. **MEDIUM:** `mailHarveySleepControl` — C# gate по отношениям — **открыто**
5. **LOW:** Удалить legacy `mailHarvey_Neglect` из CP — **опционально**
