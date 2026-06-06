# StardewUI: исследование для фичи «План восстановления Харви»

> Дата: 2026-06-06  
> Репозиторий: `HarveyOverhaulInjury`  
> Статус: только исследование, без реализации.

---

## 1. Версия / ветка StardewUI

| Источник | Значение |
|----------|----------|
| Установленный мод `Mods/StardewUI/manifest.json` | **0.6.3-unofficial-mushymato.1**, UniqueID `focustense.StardewUI`, UpdateKeys → `GitHub:Mushymato/StardewUI` |
| `HarveyOverhaulStress/manifest.json` | зависимость `focustense.StardewUI`, `MinimumVersion: 0.6.1`, **`IsRequired: false`** |
| `HarveyOverhaulInjury/manifest.json` | **зависимости StardewUI нет** |
| Документация focustense | Framework API, версии до 1.0 — dev-ветки с возможными breaking changes |

**Вывод:** в игре стоит **неофициальный форк Mushymato** (ветка `dev-unoffical` на GitHub). Для Injury-мода ориентир — **Framework API `focustense.StardewUI` ≥ 0.6.1**, с учётом фактически установленной **0.6.3-unofficial-mushymato.1**.

Дополнительно: установленный StardewUI требует **SMAPI 4.5.2+** и **игру 1.6.15+** (`manifest.json` StardewUI). У Injury сейчас `MinimumApiVersion: 4.1.0` — при подключении UI это нужно согласовать.

---

## 2. Как подключается StardewUI в этом проекте

### HarveyOverhaulInjury (целевой репозиторий)

**StardewUI не подключён.**

Подтверждение по файлам:

- `HarveyOverhaulInjury.csproj` — только `Pathoschild.Stardew.ModBuildConfig`, **нет** `Reference` на `StardewUI.dll` и **нет** `SMAPIDependency`.
- `manifest.json` — нет `focustense.StardewUI` в `Dependencies`.
- Поиск по `.cs` — **0** вхождений `StardewUI`, `IViewEngine`, `RegisterViews`, `StarML`, `Handbook`.

### Соседний мод HarveyOverhaulStress (не эталон)

В папке `Mods/HarveyOverhaul/HarveyOverhaulStress/` есть задел под UI, но **исходников C# в репозитории Injury нет** — только скомпилированный `HarveyStressMeter.dll` и ассеты.

По строкам DLL и ассетам видно, что Stress пытался использовать Framework API:

- классы: `HarveyStressMeter.UI.HandbookManager`, `HandbookViewModel`, `HandbookRow`, `HandbookTab`
- вызовы API: `RegisterViews`, `RegisterSprites`, `PreloadAssets`, `CreateMenuFromAsset`, `CreateMenuControllerFromAsset`, `EnableHotReloading` / `EnableHotReloadingWithSourceSync`
- разметка: `assets/views/Handbook.sml`
- хоткей в `Config.json`: `"OpenHandbook": "LeftShift + H, RightShift + H"`

**Пользовательское указание:** Handbook в Stress **отключён / не работает** — не использовать как эталон.

### Канонический способ подключения (документация focustense)

Рекомендуемый путь для нового UI в Injury:

**A. Зависимость в `manifest.json` (обязательно, не optional):**

```json
{
  "UniqueID": "focustense.StardewUI",
  "MinimumVersion": "0.6.1",
  "IsRequired": true
}
```

**B. Ссылка на API** — один из двух вариантов из [Core Library docs](https://focustense.github.io/StardewUI/library/):

1. **ModBuildConfig** — `Reference` на `$(GameModsPath)\StardewUI\StardewUI.dll` (`Private: false`).
2. **ModManifestBuilder** — `SMAPIDependency Include="focustense.StardewUI" Reference="true"`.

**C. Runtime в `GameLaunched`:**

```csharp
viewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
viewEngine.RegisterViews("Mods/<UniqueID>/Views", "assets/views");
viewEngine.RegisterSprites("Mods/<UniqueID>/Sprites", "assets/sprites");
// опционально: viewEngine.PreloadAssets();
// dev: viewEngine.EnableHotReloading();
```

Пример — [Quick Start на focustense.github.io/StardewUI](https://focustense.github.io/StardewUI/).

**D. Namespace API:** `StardewUI.Framework.IViewEngine` (подтверждено `StardewUI.xml` в установленном моде).

**Термин `AssetPipeline`:** в `StardewUI.xml` и документации focustense **не найден**. Используются `AssetRegistry` / методы `RegisterViews` / `RegisterSprites` / `RegisterCustomData`.

---

## 3. Есть ли уже UI-слой / Handbook / окна

| Компонент | Injury (C#) | Injury (CP) | Stress |
|-----------|---------------|-------------|--------|
| StardewUI-окно | **нет** | **нет** | `Handbook.sml` + DLL (не эталон) |
| План лечения | `TreatmentPlanManager` → HUD + topics + mail | `mailHarveyTreatmentPlan_*`, диалоги `topicHarvey_TreatmentPlanGiven` | — |
| QA UI | `QaGameUiCommands` — vanilla `DialogueBox` / `activeClickableMenu` | — | — |
| Debug HUD | `ModEntry` F10 — свой `SpriteBatch`-оверлей, **не StardewUI** | — | Stress HUD (отдельный сервис) |

**Текущий «план лечения» в Injury — не UI:**

- `TreatmentPlanManager.SendTreatmentPlanForInjury()` — topics `topicHarvey_TreatmentPlanGiven`, injury-specific topic, HUD-сообщение, tiered-mail на следующий день.
- Данные плана живут в CP (`mailHarveyMedicalTiered.json`, `mailInjury.json`), не в отдельном view model.

**Handbook (Stress):** StarML-файл с `*repeat="{ActiveStates}"` / `{AllStates}`, спрайты `@Mods/StardewUI/Sprites/*`. Функционал признан нерабочим — для Injury не копировать.

---

## 4. Минимальный способ открыть окно StardewUI

По документации и `IViewEngine` в `StardewUI.xml`:

```csharp
// 1. Получить API (GameLaunched)
IViewEngine viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
if (viewEngine == null) return; // StardewUI не установлен

// 2. Зарегистрировать ассеты (один раз при старте)
viewEngine.RegisterViews("Mods/marilynsinister.HarveyOverhaul.Injury/Views", "assets/views");
viewEngine.RegisterSprites("Mods/marilynsinister.HarveyOverhaul.Injury/Sprites", "assets/sprites");

// 3. Собрать view model (с INotifyPropertyChanged — см. §7)
var context = recoveryPlanViewModel.Build();

// 4. Создать меню и показать
if (Context.IsPlayerFree)
    Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(
        "Mods/marilynsinister.HarveyOverhaul.Injury/Views/RecoveryPlan",
        context);
```

Ключевые факты API:

- `CreateMenuFromAsset` **не показывает** меню сам — нужен `Game1.activeClickableMenu` (или `IMenuController.Menu`).
- Альтернатива с настройкой поведения: `CreateMenuControllerFromAsset` → `controller.Menu`.
- Для HUD без полноэкранного меню: `CreateDrawableFromAsset` + `IViewDrawable.Draw(SpriteBatch, Vector2)`.

Минимальный StarML (по примеру Quick Start):

```xml
<frame layout="900px 620px"
       background={@Mods/StardewUI/Sprites/MenuBackground}
       border={@Mods/StardewUI/Sprites/MenuBorder}
       border-thickness="36,36,40,36"
       padding="24">
  <label font="dialogue" text={Title} />
  <label font="small" text={Summary} />
</frame>
```

Файл: `assets/views/RecoveryPlan.sml` → asset name `Mods/.../Views/RecoveryPlan`.

---

## 5. Формат разметки

| Формат | Использование в проекте | Рекомендация |
|--------|-------------------------|--------------|
| **StarML (`.sml`)** | Stress: `Handbook.sml` | **Да** — основной формат Framework |
| **Sprite JSON** | Stress: `assets/sprites/*.json` + PNG | Да, через `RegisterSprites` |
| **cs-only UI** | Injury: F10 debug overlay | Не для «Плана восстановления» |
| **Content Patcher для StarML** | не используется | Опционально; Framework достаточно SMAPI asset pipeline |

Синтаксис StarML (подтверждено Handbook.sml + docs):

- layout: `layout="900px 620px"`, `orientation="vertical"`
- привязки: `text={Title}`, `*repeat={Items}`, `click=|Method()|`, редиректы `^` / `~Type`
- внешние спрайты: `{@Mods/StardewUI/Sprites/MenuBackground}`
- свои спрайты: `sprite={Icon}` после `RegisterSprites`

---

## 6. Классы и методы, реально существующие в проекте

### StardewUI / UI (в Injury)

**Отсутствуют.** Нет `IViewEngine`, view model, `.sml`, UI-менеджера.

### Доменные классы Injury — источник данных для будущего Recovery Plan UI

| Класс | Файл | Что даст UI |
|-------|------|-------------|
| `TreatmentPlanManager` | `Managers/TreatmentPlanManager.cs` | факт выдачи плана, mail id, topics |
| `TreatmentPlanTopics` | `Core/Constants.cs` | `topicHarvey_TreatmentPlanGiven`, `topicHarvey_TreatmentPlan_{Injury}` |
| `TreatmentManager` | `Managers/TreatmentManager.cs` | старт лечения, фазы, recovery |
| `DebuffState` | `Core/Models/DebuffState.cs` | `CurrentPhase`, `TotalPhases`, `ReadyForNextPhase`, `ReadyForRecovery`, длительности фаз |
| `InjuryState` | `Core/Models/InjuryState.cs` | `MainInjuryId`, `ActivePrescriptions`, `TreatmentComplianceScore`, rehab-поля |
| `PrescriptionManager` | `Managers/PrescriptionManager.cs` | правила предписаний по травме |
| `PrescriptionState` | `Core/Models/PrescriptionState.cs` | срок, нарушения |
| `PrescriptionIds` | `Core/Constants.cs` | Rest, NoMine, KeepDry, LightWork, Checkup |
| `ComplianceManager` | `Managers/ComplianceManager.cs` | уровень соблюдения режима |
| `CheckupManager` | `Managers/CheckupManager.cs` | контрольные осмотры |
| `RehabManager` | `Managers/RehabManager.cs` | пост-выздоровительный режим |
| `SelfCareManager` | `Managers/SelfCareManager.cs` | самопомощь |
| `DialogueManager.TryAddDiagnosisCompleteTopic` | `Managers/DialogueManager.cs` | topic для CP-события плана (отдельный нарративный слой) |

### Stress DLL (только справочно, не эталон)

Из `HarveyStressMeter.dll`:

- `HarveyStressMeter.UI.HandbookManager` — `BuildViewModel`, `GetAllStates`, `StateInfo`
- `HandbookViewModel` — свойства `ActiveStates`, `AllStates` (совпадают с Handbook.sml)
- Поля row-модели: `Title`, `Effects`, `Causes`, `CureSummary`, `StatusText`, `StatusColor`, `TreatmentStageText`, `IconSprite`

### StardewUI API (установленный мод, `StardewUI.xml`)

Публичный контракт `IViewEngine`:

- `RegisterViews(assetPrefix, modDirectory)`
- `RegisterSprites(assetPrefix, modDirectory)`
- `RegisterCustomData(assetPrefix, modDirectory)`
- `CreateMenuFromAsset(assetName, context)`
- `CreateMenuControllerFromAsset(assetName, context)`
- `CreateDrawableFromAsset(assetName)`
- `EnableHotReloading(sourceDirectory?)`
- `PreloadAssets()` — **только один раз** после регистрации
- `PreloadModels(types)` / `PreloadModels(maxDepth, types)` (overload mushymato)

`IMenuController` — `Menu`, `Close()`, `CanClose`, звуки, `HideHUD`, gutters.

`IViewDrawable` — `Context`, `MaxSize`, `Draw(SpriteBatch, Vector2)`.

Extension: `ViewEngineExtensions.EnableHotReloadingWithSourceSync` (есть в установленной версии).

---

## 7. Ошибки, которые нельзя допускать

По документации focustense + антипаттерны Stress:

1. **`IsRequired: false` для StardewUI** — Stress так делает; API может быть `null`, UI молча не откроется.
2. **Копировать Handbook.sml / HandbookManager из Stress** — нерабочий эталон.
3. **View model без `INotifyPropertyChanged`** — UI не обновится после изменений ([Binding Context](https://focustense.github.io/StardewUI/framework/binding-context/)).
4. **`*repeat` на `List<T>` с мутацией без `ObservableCollection`** — новые элементы не появятся.
5. **`PreloadAssets()` вызвать дважды** — риск ошибок/крашей (`StardewUI.xml`).
6. **Прямая ссылка на DLL без manifest dependency** — неправильный порядок загрузки модов.
7. **Submodule/shared project Core + Framework DLL одновременно** — конфликт типов (`Edges -> Edges`), docs явно запрещают.
8. **`CreateMenuFromAsset` без `Game1.activeClickableMenu`** — меню создано, но не видно.
9. **Открытие UI без `Context.IsPlayerFree`** — конфликт с событиями/диалогами.
10. **Сложные nested binding без проверки** — Stress использует `{IconSprite.Texture}` / `{IconSprite.SourceRect}`; при ошибке модели окно пустое или падает.
11. **Игнорировать версию SMAPI/игры StardewUI** — установленный мод требует новее, чем заявлен Injury.
12. **`CreateMenuFromMarkup` в продакшене** — docs: только для тестов, нет кэша и патчей.

---

## 8. Минимальный план реализации «Плана восстановления Харви» через StardewUI

> Ориентир: **документация focustense**, данные — существующие менеджеры Injury. Stress Handbook не копировать.

### Шаг 0. Инфраструктура (до UI)

1. Добавить в `manifest.json` обязательную зависимость `focustense.StardewUI` ≥ 0.6.1.
2. Добавить API-reference в `.csproj` (ModBuildConfig → `StardewUI.dll`).
3. Поднять `MinimumApiVersion` / проверить совместимость с SMAPI 4.5.2+.
4. Создать `UI/RecoveryPlanMenuController.cs` (или аналог) — единая точка StardewUI.

### Шаг 1. Регистрация ассетов

```
assets/views/RecoveryPlan.sml
assets/sprites/recoveryPlan.json (+ PNG при необходимости)
```

В `GameLaunched`:

```csharp
_viewEngine.RegisterViews("Mods/marilynsinister.HarveyOverhaul.Injury/Views", "assets/views");
_viewEngine.RegisterSprites("Mods/marilynsinister.HarveyOverhaul.Injury/Sprites", "assets/sprites");
_viewEngine.PreloadModels(typeof(RecoveryPlanViewModel), typeof(RecoveryPlanSectionRow));
_viewEngine.PreloadAssets();
```

### Шаг 2. View model (минимальный MVP)

`RecoveryPlanViewModel` (partial + `[Notify]` / INPC):

| Свойство | Источник в Injury |
|----------|-------------------|
| `Title` | «План восстановления» + имя травмы |
| `InjuryName` | `MainInjuryId` / `DebuffState.BuffId` |
| `PhaseText` | `DebuffState.CurrentPhase` / `TotalPhases` |
| `DaysInPhase` / `DaysRemaining` | `PhaseStartDay`, `GetCurrentPhaseDuration()` |
| `ReadyStatus` | `ReadyForNextPhase` / `ReadyForRecovery` |
| `ComplianceText` | `ComplianceManager.GetComplianceLevel()` |
| `Prescriptions` | `ObservableCollection<PrescriptionRow>` из `InjuryState.ActivePrescriptions` |
| `Recommendations` | текст из CP/mail-шаблона или краткий summary из `TreatmentPlanManager.ResolveMailId` |
| `HasActivePlan` | topic `TreatmentPlanTopics.Given` или `TreatmentStarted` |

Методы: `Refresh()` — перечитать `StateManager.State`; опционально `void Update(TimeSpan)` для таймеров.

### Шаг 3. StarML (MVP-экран)

Один скроллируемый `frame` (как в Quick Start, не копировать сетку Handbook):

- шапка `banner` с `{Title}`
- блок «Текущая травма / фаза»
- `*repeat={Prescriptions}` — строка предписания + остаток дней
- блок «Рекомендации Харви» — `{Recommendations}`
- кнопка закрытия через стандартное поведение меню / `IMenuController`

Использовать `@Mods/StardewUI/Sprites/*` для рамок (как в официальном примере).

### Шаг 4. Точки открытия UI

Минимальные триггеры (без перегруза):

1. **После `TreatmentPlanManager.SendTreatmentPlanForInjury`** — опционально «Открыть план» (второй HUD или автопоказ).
2. **Хоткей** — по аналогии со Stress `OpenHandbook`, но в `ModConfig` Injury (новое поле).
3. **Клик по письму / topic `topicHarvey_TreatmentPlanGiven`** — если добавить action в CP или C#-обработчик.

Показ:

```csharp
if (_viewEngine == null || !Context.IsPlayerFree) return;
Game1.activeClickableMenu = _viewEngine.CreateMenuFromAsset(AssetId, _viewModel);
```

### Шаг 5. Связь с существующей логикой (не дублировать)

- **Не заменять** mail/topics — UI дополняет `TreatmentPlanManager`, не ломает CP-цепочку.
- Тексты рекомендаций: первый MVP — строки из C# (короткий summary по `injuryId`); позже — i18n/CP.
- Обновление экрана: вызывать `Refresh()` при `DayStarted`, смене фазы (`TreatmentManager`), нарушении предписания (`PrescriptionManager`).

### Шаг 6. Тестирование

1. SMAPI log: успешный `GetApi<IViewEngine>`.
2. `injury_debuff_add` → лечение у Харви → открыть план → проверить фазу/предписания.
3. Смена дня / `injury_phase_advance` → UI обновляет фазу (INPC).
4. Без StardewUI в списке модов — graceful fallback (лог + mail/HUD как сейчас).

### Шаг 7. Что сознательно отложить

- Вкладки, справочник всех травм (как Stress Handbook).
- `CreateDrawableFromAsset` HUD-виджет на экране.
- CP-only UI без C# view model (docs: модель обязательна).

---

## Приложение: карта файлов по StardewUI

| Путь | Роль |
|------|------|
| `Mods/StardewUI/manifest.json` | версия установленного Framework |
| `Mods/StardewUI/StardewUI.xml` | XML-doc API (`IViewEngine`, `IMenuController`, `IViewDrawable`) |
| `HarveyOverhaulInjury/manifest.json` | **нет** StardewUI |
| `HarveyOverhaulInjury/HarveyOverhaulInjury.csproj` | **нет** reference |
| `HarveyOverhaulStress/assets/views/Handbook.sml` | StarML (не эталон) |
| `HarveyOverhaulStress/HarveyStressMeter.dll` | скомпилированный Handbook (не эталон) |
| https://focustense.github.io/StardewUI/ | Quick Start, концепции |
| https://focustense.github.io/StardewUI/framework/ | Framework vs Core |
| https://focustense.github.io/StardewUI/library/ | подключение DLL / SMAPIDependency |

---

## Итог в одном абзаце

**HarveyOverhaulInjury сегодня не использует StardewUI.** План лечения реализован через `TreatmentPlanManager` (HUD, topics, tiered mail) и CP-контент. Для фичи «План восстановления Харви» нужно по документации focustense добавить обязательную зависимость `focustense.StardewUI`, зарегистрировать `.sml`-view, собрать `RecoveryPlanViewModel` из `StateManager`/`DebuffState`/`PrescriptionManager`, и открывать окно через `CreateMenuFromAsset` → `Game1.activeClickableMenu`. Реализацию Handbook в HarveyOverhaulStress не повторять.
