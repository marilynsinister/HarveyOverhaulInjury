# Аудит топиков и писем HarveyOverhaul — сводка

Дата: 2026-05-24 (актуализация)  
Источники: `audit-topics-csharp.md`, `audit-mail-csharp.md`, `audit-topics-cp-existence.md`, `audit-mail-cp-existence.md`, `audit-dynamic-id-risks.md`, `audit-dead-content.md`, `audit-relationship-tone.md`, `audit-medical-texts.md`.

---

## Масштаб (актуально)

| Категория | Topics | Mail |
|-----------|--------|------|
| C# вызывает, CP отсутствует (HIGH) | **0** dialogue gaps | **0** |
| CP есть, вызова нет (мёртвый контент) | **64** | **79** |
| Тон / медицина (контент есть) | ~15 generic After | 1 (sleep gate) |
| Event-only topics (OK без dialogue) | 4 | — |

---

## 1. Критические ошибки — статус

### ✅ Закрыто (2026-05-23)

- 11× `topicTreatment*` — CP keys
- `topicSurgicalWound`, `topicSurgicalWoundCured`, `topicColdCured`, Cold phases
- `topicHealthDamageSevere`, `topicTooCold`
- 4 mail: `DirtyWoundInfection`, `WetBandageInfection`, `TreatmentUrgentReminder`, `TreatmentFinalWarning`
- Neglect mail: C# → `HarveyMod_NeglectWarning`
- Phase alias block1 (Cast/Observation/Treatment/Surgery/Rehab)
- Treat copy-paste P0 (Concussion, Infected, Fractured, Burn, Hurt)

### ✅ Закрыто C# (2026-05-24)

- `topicDiagnosisComplete` — `TryAddDiagnosisCompleteTopic`
- `topicRescueOperation` — `RescueOperationLauncher`
- Storm comfort — `StormComfortLauncher` (`buffStressThunder` / `topicHarveyStormStress`)
- Pass-out cutscenes — `eventHarveyEmergencyCare` / `eventHarveyExhaustion`
- Minor mine rescue — `TryTriggerMinorMineRescue`
- Repeatable injuries — cooldown вместо permanent AppliedTriggers
- Completion handler — Cold + Surgical cured

### ⚠️ Открыто (не HIGH)

| ID | Проблема | Приоритет |
|----|----------|-----------|
| `MailIds.WetCare` / `WetStitchesCare` | CP есть, C# не send | MEDIUM |
| `topicHarveyMinorMineRescue` | C# add, нет dialogue | MEDIUM |
| Health damage topics не снимаются | C# cleanup | MEDIUM |
| `mailHarveySleepControl` | C# без hearts gate | MEDIUM |
| Memory topics (16) | Задел | LOW |
| Stress module off | 27+22 ID | LOW |
| Narrative mail (~38) | Задел | LOW |

---

## 2. Чеклист синхронизации (актуальный)

### ID C# ↔ CP

- [x] Каждый gameplay `AddTopic` / `GetPhaseTopicId` / `*Cured` — exact CP dialogue key
- [x] Каждый `addMailForTomorrow` — exact CP mail key
- [x] 11 `topicTreatment*` в CP
- [x] Surgical/Cold/HealthDamage/TooCold keys
- [x] Neglect mail unified ID
- [x] Phase aliases block1
- [x] Все send через `MailIds`
- [ ] `topicHarveyMinorMineRescue` dialogue
- [ ] WetCare mail wired

### Тон / медицина

- [x] P0 Treat copy-paste (CP 2026-05-23)
- [x] P0 phase topics переписаны (block1)
- [x] Hearts gates injury/cure base (CP 2026-05-23)
- [ ] Treat After3–7 generic (не P0 травмы) — LOW
- [ ] C# sleep mail hearts gate — MEDIUM

---

## 3. Рекомендуемый порядок (оставшееся)

1. **MEDIUM:** Wire `HarveyMod_WetCare` / `WetStitchesCare` или удалить константы
2. **MEDIUM:** Dialogue `topicHarveyMinorMineRescue`
3. **MEDIUM:** RemoveTopic для health damage / post-op при recovery
4. **MEDIUM:** C# gate `mailHarveySleepControl` по отношениям
5. **LOW:** Stress Include decision; memory topics; narrative mail cleanup

---

## Связанные документы

| Документ | Фокус |
|----------|-------|
| [audit-dynamic-id-risks.md](./audit-dynamic-id-risks.md) | Шаблоны Replace, GetPhaseTopicId |
| [audit-topics-cp-existence.md](./audit-topics-cp-existence.md) | C# topic → CP dialogue |
| [audit-mail-cp-existence.md](./audit-mail-cp-existence.md) | C# mail → CP Mail |
| [audit-dead-content.md](./audit-dead-content.md) | CP → вызовы (обратный) |
| [audit-relationship-tone.md](./audit-relationship-tone.md) | Hearts / Dating |
| [audit-medical-texts.md](./audit-medical-texts.md) | Медицина по травмам |
| [events-audit.md](./events-audit.md) | CP events + C# launchers |
