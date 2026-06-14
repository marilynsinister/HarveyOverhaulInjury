# NPC-реакции: баланс частоты, совместимость, нагрузка

Обзор всех CP-блоков реакций жителей (HarveyOverhaul `[CP]`, март 2026).  
Источники: `assets/Code/npc_reactions_*.json`, `assets/Code/triggersNpcReactions*.json`, [`npc-reactions-source-audit.md`](npc-reactions-source-audit.md), [`npc-reactions-matrix.md`](npc-reactions-matrix.md).

---

## Краткий вывод

| Аспект | Оценка | Комментарий |
|--------|--------|-------------|
| Ежедневный спам | ⚠️ **Риск** | 48 из 52 триггеров без `RANDOM`/cooldown → bridge-topic ставится **каждое утро**, пока жив mod-gate |
| Нагрузка CPU | 🟡 Приемлемо | Только `DayStarted`, без `OnTimeChanged`; 52 проверки с длинным `GameStateQuery ANY` — раз в день |
| Конфликт слоёв | ⚠️ **Риск** | Generic + HomeCare + Young + Maru/Marlon могут дать **до 8–12 bridge-topics** за одно утро |
| Харви / ваниль | 🟢 В целом OK | Отдельные ключи, `Late + 3…6`; Harvey не использует bridge NPC |
| Врачебная тайна | 🟢 OK | Нет «Харви рассказал диагноз»; RumorSoft — только наблюдения |
| Длина / тон | 🟢 OK | ≤3 блока; Young/Marlon/Shane различаются; Generic/HomeCare однообразнее |
| Дети | 🟢 OK | `RANDOM 0.12` + `Kids_Cooldown` 4 дня — этalon для редкости |

**Главная проблема:** система **живая**, но при длинной травме или режиме восстановления игрок слышит mod-реплику **почти у каждого NPC каждый день**, а не «изредка напоминание долины».

---

## Инвентарь блоков

| Блок | Файлы | Bridge-триггеров | Диалог-патчей | Priority | TTL bridge | RANDOM / CD |
|------|--------|------------------|---------------|----------|------------|-------------|
| **Generic A–G** | `injury_stress` | 9 | 30 | Late + 3 | 2 дня | ❌ |
| **HarveyRel** | `harvey_relationship` | 3 (+ mine) | 27 | Late + 4 | 2 дня | ❌ (только при романе с Harvey) |
| **RumorSoft** | `rumor_soft` | 1 | 13 | Late + 5 | 1 день | ✅ `RANDOM 0.18`, `!RumorSoft` |
| **HomeCare** | `home_care` | 6 | 42 | Late + 5 | 2 дня | ❌ |
| **Maru** | `maru` | 11 | 11 | Late + 6 | 2 дня | ❌ |
| **Marlon** | `marlon` | 9 | 9 | Late + 6 | 2 дня | ❌ |
| **Young** | `young` | 10 | 110 | Late + 6 | 2 дня | ❌ |
| **Kids** | `kids` | 3 | 6 | Late + 6 | 1 день | ✅ `RANDOM 0.12`, `Kids_Cooldown` 4d |

**Итого:** 52 `TriggerActions` (все `DayStarted`), **248** NPC-реплик, **0** `OnTimeChanged`.

---

## Проверка по 10 пунктам

### 1. Не показываются ли реакции каждый день бесконечно?

**Частично нарушено.**

- Пока активен `topicHurt`, `HarveyMod_RecoveryPlanStarted`, `buffStressTired` и т.п., триггер на **каждом** `DayStarted` снова вызывает `AddConversationTopic … 2`.
- Bridge-topic с TTL 2 дня **не ограничивает частоту**, если gate держится неделями: игрок получает mod-приветствие при каждом разговоре с NPC, у которого есть патч на активный bridge-key.
- **Исключения (хорошо):** RumorSoft (~18% утра при gate), Kids (~12% + пауза 4 дня).

### 2. Ограничения через ConversationTopic / SeenEvent?

| Механизм | Где есть | Где нет |
|----------|----------|---------|
| Bridge-topic TTL | Везде (1–2 дня) | Не равно «редко» |
| `!PLAYER_HAS_CONVERSATION_TOPIC` (anti-retrigger) | RumorSoft, Kids | Generic, HomeCare, Young, Maru, Marlon, HarveyRel |
| `RANDOM` | RumorSoft, Kids | Остальные |
| `SeenEvent` one-shot | — | Нигде в NPC-реакциях |
| `!Relationship:Harvey` | Generic, HomeCare, Maru, Marlon | **Young** (кроме Penny/Leah/Elliott в `When`) |
| Hearts gate | Большинство блоков | Marlon — без hearts в патчах |

### 3. Тяжёлые условия, частый пересчёт?

- **52× `DayStarted`** с `GameStateQuery ANY` на 5–40 подусловий — **один раз в день**, для SMAPI/CP это нормально.
- **Не** на каждый тик времени — хорошо.
- Узкое место — не CPU, а **логическая**: много триггеров с **пересекающимися** ANY-списками (один gate → несколько bridge).

### 4. OnTimeChanged там, где хватит диалога?

**Нет.** Во всех `triggersNpcReactions*.json` только `DayStarted`. ✅

### 5. Замена ванильных реплик?

**Да, временно — по дизайну conversation topic.**

- Пока активен bridge-topic, игра предпочитает entry с **ключом = topic id** вместо seasonal/heart greeting.
- При **длинном** лечении (5–14 дней) игрок почти не слышит обычных приветствий Robin/Evelyn/… — только «отдыхай / режим / устала».
- Отдельные ключи mod ≠ перезапись ванильных строк — **но поведение для игрока = замена**, пока gate активен.

### 6. Конфликт с диалогами Харви?

| Ситуация | Риск |
|----------|------|
| Harvey dialogue keys | 🟢 Другие topic id — не пересекаются |
| Приоритет | Harvey mod-диалоги на своих `topicHarvey_*` — OK |
| Роман с Harvey | Generic/HomeCare **выключены**; HarveyRel + Young (8 NPC) + Maru/Marlon rel-ветки **включены** — возможны **2 bridge-topic** утром (HarveyRel + Young_*) |
| «Просил передать» (HarveyRel) | Уместно для overlay; не дублирует `$action` лечения |

**Рекомендация:** при Dating/Married с Harvey для NPC без HarveyRel — либо Young тоже исключать, либо общий CD (см. ниже).

### 7. NPC говорят то, чего не могут знать?

| Реплика | Оценка |
|---------|--------|
| «Слышала, ты не в форме» / походка | 🟢 Наблюдение |
| «На режиме» / «Харви составил план» | 🟡 Допустимо как «в городе знают, что фермер на лечении»; не называть диагноз |
| «Тебя вытащили из шахты» (Marlon, Young) | 🟢 Публичное событие после rescue |
| RumorSoft «Харви вышел из клиники быстрее» | 🟢 Наблюдение, не тайна |
| Maru «доктор сосредоточеннее» | 🟢 Коллега в клинике |
| HarveyRel «Харви просил передать» | 🟢 Явно от доктора, не сплетня |
| Young Exhaustion + `topicPassedOutInTown` | 🟡 NPC не видели обморок — лучше gate только `topicFarmerExhausted` / `buffStressTired` |

**Нарушений врачебной тайны в текстах не найдено** (нет диагнозов, «Харви рассказал про…»).

### 8. Врачебная тайна

- RumorSoft: явный запрет в комментарии CP — соблюдён.
- Generic/HomeCare/Young: общие формулировки («не в форме», «режим») — **OK**.
- Marlon «приказ врача» — публичный запрет шахты, не карта пациента — **OK**.

### 9. Длина реплик

- Автопроверка: **0** реплик >280 символов или >3 блоков `$b#`.
- Соответствует матрице (2–3 блока). ✅

### 10. Одинаковый тон

| Блок | Различимость |
|------|--------------|
| **Young** | ✅ Сильная (Abigail/Shane/Clint/…) |
| **Marlon / Maru / Shane** | ✅ |
| **HarveyRel** | ✅ Уважение к паре |
| **Generic + HomeCare** | ⚠️ Много «отдыхай / молодец / суп» — **близкие** друг к другу |
| **HomeCare vs Young** (Robin, Evelyn, Gus…) | ⚠️ Дублируют категории при разных bridge-keys |

---

## Сценарий «тяжёлое утро» (стресс-тест)

Игрок **без романа с Harvey**: `topicSprainedAnkle` + `HarveyMod_RecoveryPlanStarted` + `buffStressTired` + `buffStressThunder`.

**Может сработать за одно `DayStarted`:**

| Bridge topic | Блок |
|--------------|------|
| `InjuryRecent` | Generic |
| `RecoveryCompliant` | Generic |
| `Overwork` | Generic (buffStressTired в ANY) |
| `StormFear` | Generic |
| `HomeCare_Tired` | HomeCare |
| `HomeCare_RecoveryCompliant` | HomeCare |
| `HomeCare_Storm` | HomeCare |
| `Young_Injury` | Young |
| `Young_RecoveryPlan` | Young |
| `Young_Exhaustion` | Young |
| `Young_Thunder` | Young |

→ **до 11 bridge-topics**. Разговор с Robin: в словаре 3–4 ключа; игра показывает **одну** topic-реплику — но при обходе города **каждый** знакомый NPC с патчем говорит про травму/усталость.

---

## Потенциально навязчивые реакции

| Приоритет | Bridge / блок | Почему навязчиво |
|-----------|---------------|------------------|
| 🔴 Высокий | **Young_**\* (10 триггеров) | 11 NPC × каждый день gate; перекрывает Generic/HomeCare по смыслу |
| 🔴 Высокий | **Generic RecoveryCompliant + HomeCare RecoveryCompliant** | Дубль; Evelyn/Marnie/Robin — двойной «молодец» |
| 🔴 Высокий | **Generic Overwork** (`buffStressTired` в ANY) | Пересекается с HomeCare_Tired и Young_Exhaustion |
| 🟠 Средний | **Generic InjuryRecent** | Каждый день всей травмы; 5 NPC |
| 🟠 Средний | **Marlon RecoveryActive + MineBanRespected** | Оба на `RecoveryPlanStarted` — два Marlon-topic за утро |
| 🟠 Средний | **Young_RecoveryPlan** incl. `RecoveryPlanCompleted_*` | Похвала может повторяться, пока жив completed-topic |
| 🟠 Средний | **HarveyRel** (при романе) | Каждый день gate + 9 NPC — overlay без RANDOM |
| 🟡 Низкий | **Maru_**\* (кроме MineRescue) | 1 NPC, коротко, Late+6 |
| 🟢 OK | **RumorSoft** | RANDOM + 1 день |
| 🟢 OK | **Kids_**\* | RANDOM + 4d CD |

---

## Что лучше сделать реже

1. **Generic A–G** — свернуть или добавить `RANDOM 0.2` + `HarveyMod_CD_NPCReaction 3` на весь блок; оставить только NPC **без** покрытия в HomeCare/Young/Maru.
2. **HomeCare** — `RANDOM 0.25` **или** один bridge «HomeCare_Day» вместо 6 параллельных.
3. **Young** — `RANDOM 0.3` **или** CD 2 дня на категорию (`Young_CD_Injury`, …); исключить `RecoveryPlanCompleted_*` из триггера RecoveryPlan (только `Started` / `SoftTone`).
4. **Generic/HOME Overwork vs Tired** — развести gates: Overwork **без** `buffStressTired`.
5. **HarveyRel** — `RANDOM 0.35` или CD 3 дня; не чаще RumorSoft по ощущениям.
6. **Marlon** — объединить `RecoveryActive` + `MineBanRespected` в один trigger; mutual exclusion между SevereInjury и MineRescue.

---

## Что можно оставить (баланс OK или легко чинится текстом)

| Блок | Рекомендация |
|------|--------------|
| **Kids** | Оставить как этalon частоты |
| **RumorSoft** | Оставить; при спаме снизить до `RANDOM 0.12` |
| **Maru** | Оставить; 1 NPC, профессиональный тон, не дублирует толпу |
| **Marlon** | Оставить после слияния дублирующих триггеров; важен для шахты |
| **HarveyRel** | Оставить для романса; добавить CD |
| **Young (контент)** | Оставить тексты; **урезать частоту**, не удалять |
| **HomeCare (контент)** | Оставить для Jodi/Caroline/George (нет в Young) |

---

## Какие условия усилить (CP, без C#)

### A. Общий cooldown (рекомендуется первым)

```text
AddConversationTopic HarveyMod_CD_NPCReaction 2   // или 3
```

На **каждый** trigger (кроме Kids/RumorSoft):

```text
!PLAYER_HAS_CONVERSATION_TOPIC Current HarveyMod_CD_NPCReaction
```

В `Actions` после bridge — добавить CD topic.

### B. Mutual exclusion утренних bridge

Один «главный» bridge на день, например приоритет:

`MineRescue` > `RecoveryViolated` > `InjurySevere` > `Injury` > `Stress` > `RecoveryCompliant`

Остальные триггеры того же утра: `!PLAYER_HAS_CONVERSATION_TOPIC Current HarveyOverhaul_NPCReaction_Primary`.

*Требует* небольшой refactor triggers (цепочка или один trigger с несколькими Actions — в CP ограничено).

### C. Минимальные правки по блокам

| Блок | Усиление |
|------|----------|
| Generic | `!HarveyMod_CD_NPCReaction`; убрать `buffStressTired` из Overwork |
| HomeCare | + `RANDOM 0.25`; `!CD` |
| Young | + `!HarveyMod_CD_NPCReaction`; `!Relationship Harvey` **для всех** 11 или CD при романе; убрать Completed из RecoveryPlan trigger |
| Young Exhaustion | убрать `topicPassedOutInTown` |
| Marlon | один bridge на RecoveryPlanStarted |
| Maru MineRescue | при Harvey Dating — только `Maru_Dating`, не `Maru_MineRescue` (+ `!Maru_Dating topic` на neutral) |
| HarveyRel | + `RANDOM 0.3` или CD 3d |

### D. Приоритеты (если два bridge-key у одного NPC)

Сейчас: Young/Maru/Marlon **Late + 6** > HomeCare/Rumor **+5** > HarveyRel **+4** > Generic **+3**.

При нескольких активных topic-keys порядок выбора в игре **не гарантирован CP Priority** (Priority влияет на merge патчей, не на pick topic). **CD + один bridge** надёжнее, чем повышать Priority.

---

## Желательные флаги в C# (точнее и реже)

| Topic / buff | Мод | Зачем |
|--------------|-----|--------|
| `HarveyMod_NPCReactionCooldown` | Injury или Stress | C# ставит 2–3 дня после **любой** NPC-bridge реакции; CP триггеры проверяют `!topic` |
| `HarveyMod_InjuryEpisodeId` + `…_NpcReacted` | Injury | One-shot «эпизод травмы» — NPC реагируют 1–2 раза за эпизод, не каждый день buff |
| `HarveyMod_RecoveryPlanActive` | Injury | Отделить от `Completed_*`; CP Young/HomeCare только на Active |
| `HarveyMod_RecoveryPlanCompletedRecent` | Injury | TTL 1–2 дня для похвалы, не весь Completed-topic |
| `HarveyMod_PublicMineRescue` | Injury | Публичное знание rescue vs «просто рана» — для Marlon/Young Mine |
| `HarveyMod_StressVisibleToTown` | StressMeter | Соц./гроза/усталость: NPC не читают buff, только «мягкий» публичный флаг |
| `HarveyMod_SocialRecovery_Completed` | StressMeter | Penny-сцена «после квеста», не на каждый followup |
| `HarveyMod_IgnoredCheckup` | Injury | Maru «не пропускай осмотр» без угадывания CheckupDue |
| `HarveyMod_PlayerCarryingHeavy` | Injury | Robin «не тащи» — только когда реально несёт big craftable |
| `HarveyMod_PrescriptionActive_LightWork` | Injury | Уже есть buff — использовать **вместо** широкого RecoveryPlanStarted в Robin-сценах |

---

## OnTimeChanged — не нужен

Все реакции — **passive dialogue** при разговоре. Нет причин переводить на `OnTimeChanged`. Ambient **сценки** (отдельный дизайн `eventHarveyAmbient_*`) — через вход на локацию + `!SEEN_EVENT`, не time tick.

---

## Конфликт слоёв: кто кого заменяет

```text
[ Kids / RumorSoft ]     ← редкие, OK
        ↓
[ Maru / Marlon / Young ] ← Late+6, много NPC
        ↓
[ HomeCare / HarveyRel ]  ← Late+4…5
        ↓
[ Generic A–G ]           ← Late+3, широкие gates
        ↓
[ Vanilla seasonal/heart ]
```

**Целевая архитектура (рекомендация):**

1. **Generic** — только уникальные NPC (Demetrius, Wizard, Pam…) **без** Young/HomeCare.
2. **HomeCare** — Jodi, Caroline, George, Evelyn, … (семья/соседи).
3. **Young** — 11 молодых (единственный слой для них).
4. **Maru / Marlon** — специализация.
5. **Общий CD** на все слои.

---

## Чеклист перед релизом патча баланса

- [ ] Зарегистрировать `HarveyMod_CD_NPCReaction` в `Data/ConversationTopics`
- [ ] Добавить CD в 48 «ежедневных» triggers
- [ ] Убрать пересечение Tired/Overwork/Exhaustion gates
- [ ] Young: убрать `PassedOutInTown`, `RecoveryPlanCompleted_*` из частых triggers
- [ ] Marlon: слить RecoveryActive + MineBanRespected
- [ ] Документировать в `npc-reactions-cp-todos.md` C# flags
- [ ] Плейтест: 7 дней `buffSprainedAnkle` + режим — считать mod vs vanilla greetings в Town

---

## Связанные документы

- [`npc-reactions-cp-todos.md`](npc-reactions-cp-todos.md) — TODO gates
- [`npc-reactions-matrix.md`](npc-reactions-matrix.md) — контент-дизайн
- [`npc-reactions-source-audit.md`](npc-reactions-source-audit.md) — реальные buff/topic

---

*Документ: аудит баланса, без изменений CP. Следующий шаг — патч triggers (CD + RANDOM) по приоритетам таблицы «навязчивые».*
