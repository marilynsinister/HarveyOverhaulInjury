# Аудит писем, которые отправляет C# (HarveyOverhaul.InjuryCare)

Дата: 2026-05-24 (актуализация)  
Область: все `.cs` файлы проекта (без `obj/`).

Метод отправки: `Game1.addMailForTomorrow(...)` — письмо на **следующее утро**.  
Глобальный выключатель: `ModConfig.SendLetters` (по умолчанию `true`).

**Не найдено:** `mailReceived`, другие API отправки почты.

---

## Сводка

| Категория | Кол-во |
|-----------|--------|
| Реально отправляемые mail ID | **7** |
| Все через `MailIds.*` | **7** (100%) |
| Строковые литералы напрямую | **0** |
| Константы `MailIds` без send | **3** (`WetCare`, `WetStitchesCare`, `InfectionAlert`) |
| Legacy save-поля без send | **2** (`WetBandageMailDay`, `WetStitchesMailDay`) |

---

## Таблица: все вызовы `addMailForTomorrow`

| Mail ID | Где | Условие | Повтор | CP entry | Комментарий |
|---------|-----|---------|--------|----------|-------------|
| `mailHarveySleepControl` | `PassOutHandler` | Обморок в Town после 26:00 + dating/married + `SendLetters` | Может повториться | ✓ `mailInjury.json` | `MailIds.SleepControl` |
| `mailHarveyMineForbidden` | `GameEventHandler.OnDayEnding` | `MineWarningDay == today` после severe + шахта | 1× на инцидент | ✓ | `MailIds.MineForbidden` |
| `HarveyMod_DirtyWoundInfection` | `ComplicationManager` | Roll dirty wound → infection | При новом dirty | ✓ | `MailIds.DirtyWoundInfection` |
| `HarveyMod_WetBandageInfection` | `ComplicationManager` | Roll wet bandage → infection | При новой wet | ✓ | `MailIds.WetBandageInfection` |
| `HarveyMod_TreatmentUrgentReminder` | `ComplicationManager.CheckPhaseNeglect` | `days == phaseDuration + 3` | Новая фаза/травма | ✓ | `MailIds.TreatmentUrgentReminder` |
| `HarveyMod_TreatmentFinalWarning` | `ComplicationManager.CheckPhaseNeglect` | `days == totalAllowed - 1` | Новая фаза/травма | ✓ | `MailIds.TreatmentFinalWarning` |
| `HarveyMod_NeglectWarning` | `ComplicationManager.CheckPhaseNeglect` | `days >= totalAllowed` | Пока просрочка | ✓ | `MailIds.NeglectWarning` (не `mailHarvey_Neglect`) |

---

## Константы `MailIds` — полный список

| Константа | Значение | Send? | CP | Комментарий |
|-----------|----------|-------|-----|-------------|
| `SleepControl` | `mailHarveySleepControl` | **Да** | ✓ | — |
| `MineForbidden` | `mailHarveyMineForbidden` | **Да** | ✓ | — |
| `DirtyWoundInfection` | `HarveyMod_DirtyWoundInfection` | **Да** | ✓ | Добавлен в CP 2026-05-23 |
| `WetBandageInfection` | `HarveyMod_WetBandageInfection` | **Да** | ✓ | Добавлен в CP 2026-05-23 |
| `TreatmentUrgentReminder` | `HarveyMod_TreatmentUrgentReminder` | **Да** | ✓ | Добавлен в CP 2026-05-23 |
| `TreatmentFinalWarning` | `HarveyMod_TreatmentFinalWarning` | **Да** | ✓ | Добавлен в CP 2026-05-23 |
| `NeglectWarning` | `HarveyMod_NeglectWarning` | **Да** | ✓ | C# unified 2026-05-23; legacy `mailHarvey_Neglect` в CP — дубль |
| `WetCare` | `HarveyMod_WetCare` | **Нет** | ✓ `mailCure.json` | Задел: send не wired |
| `WetStitchesCare` | `HarveyMod_WetStitchesCare` | **Нет** | ✓ | Задел |
| `InfectionAlert` | `HarveyMod_InfectionAlert` | **Нет** | ✓ | Generic alert; C# шлёт dirty/wet-specific |

---

## Цепочки (кратко)

```
Шахта severe → MineWarningDay → mailHarveyMineForbidden → HarveyMod_MineForbidden
Обморок Town ≥26:00 → mailHarveySleepControl
Dirty/Wet roll → HarveyMod_*Infection
Phase neglect → Urgent → Final → HarveyMod_NeglectWarning + buff neglect
```

---

## Риски (актуальные)

1. **Нет проверки `mailReceived`** — neglect/infection могут повторяться при новых инцидентах.
2. **`MailIds.WetCare` / `WetStitchesCare` / `InfectionAlert`** — не используются в send.
3. **`mailHarveySleepControl`** — C# без gate по hearts (текст смягчён в CP 2026-05-23).
4. **Два пути neglect** — `GameEventHandler.CheckNeglect` без письма; `ComplicationManager` с письмом.

---

## Индекс файлов

| Файл | Роль |
|------|------|
| `Core/Constants.cs` | `MailIds.*` |
| `EventHandlers/PassOutHandler.cs` | `SleepControl` |
| `EventHandlers/GameEventHandler.cs` | `MineForbidden` |
| `Managers/ComplicationManager.cs` | infection + phase neglect mails |

**Статус синхронизации C# ↔ CP mail:** ✅ все 7 отправляемых ID имеют exact entry в CP.
