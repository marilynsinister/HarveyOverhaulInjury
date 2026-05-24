# Аудит координат CP-событий (техническая постановка)

Анализ **непроверенных** событий из [`events-map-audit-plan.md`](events-map-audit-plan.md).
Сверка с [`map-passports.md`](map-passports.md) и TMX в `tmpMap/`. **События не изменялись.**

**Проверено событий:** 31 / 31
**Статусы:** OK=11, Warning=11, Broken=9, Needs map export=0

## Сводная таблица

| Event ID | Target | Приоритет | Статус |
|----------|--------|-----------|--------|
| eventHarveyMineRescue | Mine | High | **Warning** |
| eventHarveyMineRescueDating | Mine | High | **Warning** |
| eventHarveyMinorMineRescue | Mine | High | **OK** |
| HarveyMod_FirstTreatment | Hospital | Medium | **Broken** |
| eventRescueOperation | Woods | High | **Broken** |
| eventHarveyMineInterception | Mine | High | **OK** |
| eventHarveySkullCavePrevention | SkullCave | Medium | **OK** |
| eventHarveyStormComfortForest | Forest | Medium | **OK** |
| eventHarveyStormComfortMountain | Custom_AdventurerSummit | High | **Warning** |
| eventHarveyStormComfortTown | Town | High | **Warning** |
| eventHarveyStormComfortDesert | Desert | Medium | **Broken** |
| eventHarveyStormComfortMine | Mine | High | **OK** |
| HarveyOverhaulStory.E1_SlipperyPath | BusStop | High | **OK** |
| HarveyOverhaulStory.E2_InsistentExam | Hospital | High | **Broken** |
| HarveyOverhaulStory.E2B_QuietAgreement | Town | Medium | **OK** |
| HarveyOverhaulStory.E3_ForestApothecary | Forest | Medium | **OK** |
| HarveyOverhaulStory.E3B_WingPatient | Forest | Medium | **OK** |
| HarveyOverhaulStory.E4_PierBreath | Beach | Medium | **OK** |
| HarveyOverhaulStory.E4B_TooQuiet | Mountain | Medium | **OK** |
| HarveyOverhaulStory.E5_StormBeside | Hospital | High | **Warning** |
| HarveyOverhaulStory.E6_SayItOutLoud | Hospital | High | **Warning** |
| HarveyOverhaulStory.E7_TownSip_Sunny | Town | High | **Broken** |
| HarveyOverhaulStory.E8_QuietShelf | ArchaeologyHouse | High | **Broken** |
| HarveyOverhaulStory.E9_LightInWindow | Town | Medium | **Broken** |
| eventHarveyFirstMeeting | BusStop | Medium | **Warning** |
| eventHarveyCheckup | BusStop | High | **Broken** |
| eventHarveyMedicalCheck_Dating | Hospital | High | **Broken** |
| HarveyMod_NightCrisis_Dating | Hospital | Medium | **Warning** |
| HarveyMod_NightCrisis_PreDating | Hospital | Medium | **Warning** |
| HarveyMod_BirthdayHospital_Dating | Hospital | Low | **Warning** |
| HarveyMod_BirthdayHospital_Friend | Hospital | Low | **Warning** |

---

## HarveyMod_FirstTreatment

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** Medium
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 5 | 9 | Hospital | Buildings, Back непроходим | Broken |
| setup | Harvey | 4 | 5 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (5,9) | move farmer 0 -4 3 | (5,5) | прямой | OK |
| farmer | (5,5) | move farmer 0 -1 2 | (5,4) | (5,4)=Broken | Warning |
| Harvey | (4,5) | move Harvey 1 0 0 | (5,5) | прямой | OK |
| Harvey | (5,5) | move Harvey -2 0 0 | (3,5) | прямой | OK |
| Harvey | (3,5) | move Harvey 2 0 0 | (5,5) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (5,9) | Hospital | Action Door (5,9); Buildings на тайле | Broken |
| (4,5) | Hospital | Action Message "Hospital.3" (3,4) | OK |

### Проблемы

- **[High]** setup farmer (5,9) на Hospital: Buildings, Back непроходим
- **[Medium]** Путь farmer move farmer 0 -1 2: (5,4)=Broken

### Рекомендации

- Старт farmer (5,9): заменить на (4,6) или (6,10) — проходимые тайлы у кушетки (map-passports)

---

## HarveyOverhaulStory.E2_InsistentExam

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** High
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 1 | 5 | Hospital | Front, Back проходим | Warning |
| doAction | — | 5 | 9 | Hospital | Buildings, Back непроходим | Broken |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (0,0) | move farmer 0 -5 3 | (0,-5) | (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken; (0,-4)=Broken; (0,-5)=Broken | Warning |
| farmer | (0,-5) | move farmer 0 -1 2 | (0,-6) | (0,-6)=Broken | Warning |
| Harvey | (1,5) | move Harvey 4 0 0 | (5,5) | прямой | OK |
| Harvey | (5,5) | move Harvey -1 0 1 | (4,5) | прямой | OK |
| Harvey | (4,5) | move Harvey -1 0 0 | (3,5) | прямой | OK |
| Harvey | (3,5) | move Harvey 1 0 0 | (4,5) | прямой | OK |
| Harvey | (4,5) | move Harvey -1 0 1 | (3,5) | прямой | OK |
| farmer | (0,-6) | move farmer 0 1 3 | (0,-5) | (0,-5)=Broken | Warning |
| farmer | (0,-5) | move farmer 0 -2 0 true | (0,-7) | (0,-6)=Broken; (0,-7)=Broken | Warning |
| Harvey | (3,5) | move Harvey 0 -2 0 true | (3,3) | (3,4)=Broken; (3,3)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (1,5) | Hospital | Action Message "Hospital.2" (1,4); Front/AlwaysFront на тайле | Warning |
| (5,9) | Hospital | Action Door (5,9); Buildings на тайле | Broken |

### Проблемы

- **[Medium]** setup Harvey (1,5) на Hospital: Front, Back проходим
- **[High]** doAction — (5,9) на Hospital: Buildings, Back непроходим
- **[Medium]** Путь farmer move farmer 0 -5 3: (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken; (0,-4)=Broken; (0,-5)=Broken
- **[Medium]** Путь farmer move farmer 0 -1 2: (0,-6)=Broken
- **[Medium]** Путь farmer move farmer 0 1 3: (0,-5)=Broken
- **[Medium]** Путь farmer move farmer 0 -2 0 true: (0,-6)=Broken; (0,-7)=Broken
- **[Medium]** Путь Harvey move Harvey 0 -2 0 true: (3,4)=Broken; (3,3)=Broken

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## HarveyOverhaulStory.E7_TownSip_Sunny

- **Локация:** Town
- **Файл:** `events.json` → `Town`
- **Приоритет:** High
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 26 | 22 | Town | Back проходим | OK |
| setup | Harvey | 27 | 22 | Town | Buildings, Back непроходим | Broken |
| addTemporaryActor | Penny | 32 | 24 | Town | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Penny | (32,24) | move Penny 2 0 1 true | (34,24) | прямой | OK |
| Penny | (34,24) | move Penny 4 0 1 true | (38,24) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (26,22) | Town | — | OK |
| (27,22) | Town | Buildings на тайле | Broken |
| (32,24) | Town | — | OK |

### Проблемы

- **[High]** setup Harvey (27,22) на Town: Buildings, Back непроходим

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## HarveyOverhaulStory.E8_QuietShelf

- **Локация:** ArchaeologyHouse
- **Файл:** `events.json` → `ArchaeologyHouse`
- **Приоритет:** High
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 18 | 9 | ArchaeologyHouse | Front, Back проходим | Warning |
| setup | Gunther | 11 | 9 | ArchaeologyHouse | Back проходим | OK |
| setup | Harvey | -1000 | -1000 | ArchaeologyHouse | вне карты (50×20) | Broken |
| warp | Harvey | 3 | 15 | ArchaeologyHouse | Front, Warp, Back проходим | Warning |
| warp | Gunther | 6 | 5 | ArchaeologyHouse | Buildings, Front, Back непроходим | Broken |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Gunther | (11,9) | move Gunther 6 0 1 | (17,9) | прямой | OK |
| Gunther | (17,9) | move Gunther -3 0 0 | (14,9) | прямой | OK |
| Gunther | (14,9) | move Gunther 0 -4 3 | (14,5) | прямой | OK |
| Gunther | (14,5) | move Gunther -2 0 0 | (12,5) | прямой | OK |
| Harvey | (3,15) | advancedMove Harvey false 0 -2 4 0 0 1 5 0 0 -2... | (5,18) | (4,16)=Broken; (4,17)=Broken; (5,18)=Broken; advancedMove — проверить в игре | Warning |
| Gunther | (6,5) | move Gunther 2 0 2 | (8,5) | (7,5)=Broken | Warning |
| Gunther | (8,5) | move Gunther 0 4 0 | (8,9) | прямой | OK |
| Gunther | (8,9) | move Gunther 8 0 1 | (16,9) | (9,9)=Broken; (10,9)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (18,9) | ArchaeologyHouse | Action Message "ArchaeologyHouse.3" (17,8); Action Message "ArchaeologyHouse.4" (18,8); Action Message "ArchaeologyHouse.5" (19,8); Front/AlwaysFront на тайле | Warning |
| (11,9) | ArchaeologyHouse | Action Notes 15 (11,8); Action Notes 16 (12,8) | OK |
| (-1000,-1000) | ArchaeologyHouse | координата вне карты | Broken |
| (3,15) | ArchaeologyHouse | Warp→Town (3,15); Front/AlwaysFront на тайле | Warning |
| (6,5) | ArchaeologyHouse | Warp→Custom_GunthersRoom (5,5); Buildings на тайле; Front/AlwaysFront на тайле | Broken |

### Проблемы

- **[Medium]** setup farmer (18,9) на ArchaeologyHouse: Front, Back проходим
- **[High]** setup Harvey (-1000,-1000) на ArchaeologyHouse: вне карты (50×20)
- **[Medium]** warp Harvey (3,15) на ArchaeologyHouse: Front, Warp, Back проходим
- **[Medium]** Путь Harvey advancedMove Harvey false 0 -2 4 0 0 1 5 0 0 -2 3 0 0 -3 2 0: (4,16)=Broken; (4,17)=Broken; (5,18)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove Harvey: финальная точка (5,18) — проверить траекторию в игре
- **[High]** warp Gunther (6,5) на ArchaeologyHouse: Buildings, Front, Back непроходим
- **[Medium]** Путь Gunther move Gunther 2 0 2: (7,5)=Broken
- **[Medium]** Путь Gunther move Gunther 8 0 1: (9,9)=Broken; (10,9)=Broken

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## HarveyOverhaulStory.E9_LightInWindow

- **Локация:** Town
- **Файл:** `events.json` → `Town`
- **Приоритет:** Medium
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 35 | 88 | Town | Back проходим | OK |
| setup | Harvey | -1000 | -1000 | Town | вне карты (130×116) | Broken |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| — | — | — | — | — | — |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (35,88) | Town | — | OK |
| (-1000,-1000) | Town | координата вне карты | Broken |

### Проблемы

- **[High]** setup Harvey (-1000,-1000) на Town: вне карты (130×116)

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## eventHarveyCheckup

- **Локация:** BusStop ⚠
- **Файл:** `eventsCare.json` → `BusStop`
- **Приоритет:** High
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 2 | 5 | BusStop | Buildings, Back непроходим | Broken |
| setup | Harvey | 1 | 5 | BusStop | Buildings, Front, Back непроходим | Broken |
| end position | farmer | 10 | 17 | BusStop | Buildings, Back непроходим | Broken |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (2,5) | move farmer 3 0 0 | (5,5) | (3,5)=Broken; (4,5)=Broken; (5,5)=Broken | Warning |
| farmer | (5,5) | move farmer 0 -1 2 | (5,4) | (5,4)=Broken | Warning |
| Harvey | (1,5) | move Harvey 3 0 0 | (4,5) | (2,5)=Broken; (3,5)=Broken; (4,5)=Broken | Warning |
| Harvey | (4,5) | move Harvey 1 0 0 | (5,5) | (5,5)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (2,5) | BusStop | Buildings на тайле | Broken |
| (1,5) | BusStop | Buildings на тайле; Front/AlwaysFront на тайле | Broken |
| (10,17) | BusStop | Buildings на тайле | Broken |

### Проблемы

- **[High]** setup farmer (2,5) на BusStop: Buildings, Back непроходим
- **[High]** setup Harvey (1,5) на BusStop: Buildings, Front, Back непроходим
- **[Medium]** Путь farmer move farmer 3 0 0: (3,5)=Broken; (4,5)=Broken; (5,5)=Broken
- **[Medium]** Путь farmer move farmer 0 -1 2: (5,4)=Broken
- **[Medium]** Путь Harvey move Harvey 3 0 0: (2,5)=Broken; (3,5)=Broken; (4,5)=Broken
- **[Medium]** Путь Harvey move Harvey 1 0 0: (5,5)=Broken
- **[High]** end position farmer (10,17) на BusStop: Buildings, Back непроходим
- **[High]** Target Data/Events/BusStop, но координаты и viewport (5,9) как Hospital — рассинхрон локации

### Рекомендации

- Либо перенести патч в Data/Events/Hospital, либо заменить все координаты на BusStop (напр. старт farmer ~20,23 / Harvey ~26,22 по E1)

---

## eventHarveyMedicalCheck_Dating

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** High
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 10 | 14 | Hospital | Back проходим | OK |
| warp | farmer | 10 | 19 | Hospital | Back проходим | OK |
| doAction | — | 10 | 13 | Hospital | Buildings, Back непроходим | Broken |
| warp | farmer | 20 | 5 | Hospital | Buildings, Back непроходим | Broken |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (10,14) | move Harvey 0 2 2 true | (10,16) | прямой | OK |
| farmer | (10,19) | move farmer 0 -2 0 true | (10,17) | прямой | OK |
| Harvey | (10,16) | move Harvey 0 -2 0 | (10,14) | прямой | OK |
| farmer | (10,17) | move farmer 0 -2 0 | (10,15) | прямой | OK |
| Harvey | (10,14) | advancedMove Harvey false 0 -7 6 0 0 1 5 0 | (17,12) | (12,13)=Broken; (13,13)=Broken; (14,13)=Broken; (15,13)=Broken; (16,12)=Broken; (17,12)=Broken; advancedMove — проверить в игре | Warning |
| farmer | (10,15) | advancedMove farmer false 0 -8 6 0 0 1 4 0 0 -2... | (16,9) | (12,13)=Broken; (13,12)=Broken; (15,10)=Broken; advancedMove — проверить в игре | Warning |
| Harvey | (17,12) | advancedMove Harvey false -1 0 0 -2 -1 0 | (14,11) | (16,12)=Broken; advancedMove — проверить в игре | Warning |
| Harvey | (14,11) | advancedMove Harvey false 0 -1 | (14,10) | advancedMove — проверить в игре | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (10,14) | Hospital | Action Door (10,13) | OK |
| (10,19) | Hospital | Warp→Town (10,20) | OK |
| (10,13) | Hospital | Action Door (10,13); Buildings на тайле | Broken |
| (20,5) | Hospital | Buildings на тайле | Broken |

### Проблемы

- **[High]** doAction — (10,13) на Hospital: Buildings, Back непроходим
- **[Medium]** Путь Harvey advancedMove Harvey false 0 -7 6 0 0 1 5 0: (12,13)=Broken; (13,13)=Broken; (14,13)=Broken; (15,13)=Broken; (16,12)=Broken; (17,12)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove Harvey: финальная точка (17,12) — проверить траекторию в игре
- **[Medium]** Путь farmer advancedMove farmer false 0 -8 6 0 0 1 4 0 0 -2 -1 0 0 -1: (12,13)=Broken; (13,12)=Broken; (15,10)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove farmer: финальная точка (16,9) — проверить траекторию в игре
- **[High]** warp farmer (20,5) на Hospital: Buildings, Back непроходим
- **[Medium]** Путь Harvey advancedMove Harvey false -1 0 0 -2 -1 0: (16,12)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove Harvey: финальная точка (14,11) — проверить траекторию в игре
- **[Medium]** advancedMove Harvey: финальная точка (14,10) — проверить траекторию в игре

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## eventHarveyStormComfortDesert

- **Локация:** Desert
- **Файл:** `events.json` → `Desert`
- **Приоритет:** Medium
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 15 | 23 | Desert | Back проходим | OK |
| warp | Harvey | 17 | 26 | Desert | Buildings, Back непроходим | Broken |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (17,26) | move Harvey 0 -2 3 | (17,24) | прямой | OK |
| Harvey | (17,24) | move Harvey -2 0 3 | (15,24) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (15,23) | Desert | — | OK |
| (17,26) | Desert | Action DesertBus (18,27); Buildings на тайле | Broken |

### Проблемы

- **[High]** warp Harvey (17,26) на Desert: Buildings, Back непроходим

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## eventRescueOperation

- **Локация:** Hospital → Woods → Forest → Hospital
- **Файл:** `events.json` → `Woods`
- **Приоритет:** High
- **Статус:** **Broken**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| warp | Harvey | 3 | 15 | Hospital | Front, Back проходим | Warning |
| warp | Lewis | 1000 | 1000 | Hospital | off-screen staging (1000,1000) | OK |
| warp | farmer | 27 | 18 | Woods | Front, Back проходим | Warning |
| warp | Harvey | 40 | 20 | Woods | Back проходим | OK |
| warp | Lewis | 38 | 20 | Woods | Back проходим | OK |
| warp | farmer | 66 | 16 | Forest | Back проходим | OK |
| warp | Harvey | 65 | 16 | Forest | Back проходим | OK |
| warp | Lewis | 73 | 17 | Forest | Back проходим | OK |
| warp | farmer | 20 | 5 | Hospital | Buildings, Back непроходим | Broken |
| warp | Harvey | 19 | 5 | Hospital | Front, Back проходим | Warning |
| warp | Lewis | 20 | 6 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (40,20) | move Harvey -7 0 3 | (33,20) | прямой | OK |
| Lewis | (38,20) | move Lewis -6 0 3 | (32,20) | прямой | OK |
| Harvey | (33,20) | move Harvey -4 0 3 | (29,20) | прямой | OK |
| Harvey | (29,20) | move Harvey -2 0 3 | (27,20) | прямой | OK |
| Harvey | (27,20) | move Harvey 0 -1 0 | (27,19) | прямой | OK |
| Lewis | (32,20) | move Lewis -4 0 3 | (28,20) | прямой | OK |
| Lewis | (28,20) | move Lewis 0 -1 0 | (28,19) | прямой | OK |
| Lewis | (28,19) | move Lewis 0 2 2 | (28,21) | прямой | OK |
| Lewis | (28,21) | move Lewis 2 0 3 | (30,21) | прямой | OK |
| Lewis | (20,6) | advancedMove Lewis false 0 2 -4 0 0 -1 -6 0 0 12 | (15,14) | (19,7)=Broken; (18,10)=Broken; (17,11)=Broken; (16,12)=Broken; (16,13)=Broken; (15,14)=Broken; advancedMove — проверить в игре | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (3,15) | Hospital | Front/AlwaysFront на тайле | Warning |
| (1000,1000) | Hospital | координата вне карты | OK |
| (27,18) | Woods | Front/AlwaysFront на тайле | Warning |
| (40,20) | Woods | — | OK |
| (38,20) | Woods | — | OK |
| (66,16) | Forest | — | OK |
| (65,16) | Forest | — | OK |
| (73,17) | Forest | — | OK |
| (20,5) | Hospital | Buildings на тайле | Broken |
| (19,5) | Hospital | Front/AlwaysFront на тайле | Warning |
| (20,6) | Hospital | — | OK |

### Проблемы

- **[Medium]** warp Harvey (3,15) на Hospital: Front, Back проходим
- **[Medium]** warp farmer (27,18) на Woods: Front, Back проходим
- **[High]** warp farmer (20,5) на Hospital: Buildings, Back непроходим
- **[Medium]** warp Harvey (19,5) на Hospital: Front, Back проходим
- **[Medium]** Путь Lewis advancedMove Lewis false 0 2 -4 0 0 -1 -6 0 0 12: (19,7)=Broken; (18,10)=Broken; (17,11)=Broken; (16,12)=Broken; (16,13)=Broken; (15,14)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove Lewis: финальная точка (15,14) — проверить траекторию в игре
- **[Low]** warp farmer (20,5) + positionOffset + ignoreCollisions — палата, Buildings ожидаем; Warning в TMX, OK с animate лёжа

### Рекомендации

- Исправить координаты/target перед in-game тестом (см. проблемы выше).

---

## HarveyMod_BirthdayHospital_Dating

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** Low
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 10 | 14 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (0,0) | move farmer 0 -4 0 | (0,-4) | (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken; (0,-4)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (10,14) | Hospital | Action Door (10,13) | OK |

### Проблемы

- **[Medium]** Путь farmer move farmer 0 -4 0: (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken; (0,-4)=Broken

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## HarveyMod_BirthdayHospital_Friend

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** Low
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 10 | 14 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (0,0) | move farmer 0 -4 0 | (0,-4) | (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken; (0,-4)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (10,14) | Hospital | Action Door (10,13) | OK |

### Проблемы

- **[Medium]** Путь farmer move farmer 0 -4 0: (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken; (0,-4)=Broken

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## HarveyMod_NightCrisis_Dating

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** Medium
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 15 | 8 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (0,0) | move farmer 6 0 1 | (6,0) | (1,0)=Broken; (2,0)=Broken; (3,0)=Broken; (4,0)=Broken; (5,0)=Broken | Warning |
| farmer | (6,0) | move farmer 0 1 3 | (6,1) | прямой | OK |
| Harvey | (15,8) | move Harvey 0 0 1 | (15,8) | нет смещения | OK |
| Harvey | (15,8) | move Harvey -2 0 3 | (13,8) | (13,8)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (15,8) | Hospital | — | OK |

### Проблемы

- **[Medium]** Путь farmer move farmer 6 0 1: (1,0)=Broken; (2,0)=Broken; (3,0)=Broken; (4,0)=Broken; (5,0)=Broken
- **[Medium]** Путь Harvey move Harvey -2 0 3: (13,8)=Broken

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## HarveyMod_NightCrisis_PreDating

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** Medium
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 15 | 8 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (0,0) | move farmer 6 0 1 | (6,0) | (1,0)=Broken; (2,0)=Broken; (3,0)=Broken; (4,0)=Broken; (5,0)=Broken | Warning |
| farmer | (6,0) | move farmer 0 1 3 | (6,1) | прямой | OK |
| Harvey | (15,8) | move Harvey 0 0 1 | (15,8) | нет смещения | OK |
| Harvey | (15,8) | move Harvey -2 0 3 | (13,8) | (13,8)=Broken | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (15,8) | Hospital | — | OK |

### Проблемы

- **[Medium]** Путь farmer move farmer 6 0 1: (1,0)=Broken; (2,0)=Broken; (3,0)=Broken; (4,0)=Broken; (5,0)=Broken
- **[Medium]** Путь Harvey move Harvey -2 0 3: (13,8)=Broken

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## HarveyOverhaulStory.E5_StormBeside

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** High
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 10 | 19 | Hospital | Back проходим | OK |
| setup | Harvey | 10 | 18 | Hospital | Front, Back проходим | Warning |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (10,18) | move Harvey 0 -3 0 true | (10,15) | прямой | OK |
| farmer | (10,19) | move farmer 0 -4 3 true | (10,15) | прямой | OK |
| Harvey | (10,15) | move Harvey -1 0 1 true | (9,15) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (10,19) | Hospital | Warp→Town (10,20) | OK |
| (10,18) | Hospital | Front/AlwaysFront на тайле | Warning |

### Проблемы

- **[Medium]** setup Harvey (10,18) на Hospital: Front, Back проходим

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## HarveyOverhaulStory.E6_SayItOutLoud

- **Локация:** Hospital
- **Файл:** `events.json` → `Hospital`
- **Приоритет:** High
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | Harvey | 10 | 18 | Hospital | Front, Back проходим | Warning |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (10,18) | move Harvey 0 -2 0 true | (10,16) | прямой | OK |
| farmer | (0,0) | move farmer 0 -3 3 true | (0,-3) | (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken | Warning |
| Harvey | (10,16) | move Harvey -1 0 1 | (9,16) | прямой | OK |
| farmer | (0,-3) | move farmer 0 2 0 true | (0,-1) | (0,-2)=Broken; (0,-1)=Broken | Warning |
| Harvey | (9,16) | move Harvey 0 2 0 true | (9,18) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (10,18) | Hospital | Front/AlwaysFront на тайле | Warning |

### Проблемы

- **[Medium]** setup Harvey (10,18) на Hospital: Front, Back проходим
- **[Medium]** Путь farmer move farmer 0 -3 3 true: (0,-1)=Broken; (0,-2)=Broken; (0,-3)=Broken
- **[Medium]** Путь farmer move farmer 0 2 0 true: (0,-2)=Broken; (0,-1)=Broken

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## eventHarveyFirstMeeting

- **Локация:** BusStop
- **Файл:** `events.json` → `BusStop`
- **Приоритет:** Medium
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 19 | 23 | BusStop | Front, Back проходим | Warning |
| setup | Harvey | 27 | 23 | BusStop | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (19,23) | move farmer 3 0 1 true | (22,23) | прямой | OK |
| Harvey | (27,23) | move Harvey -3 0 3 true | (24,23) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (19,23) | BusStop | Front/AlwaysFront на тайле | Warning |
| (27,23) | BusStop | — | OK |

### Проблемы

- **[Medium]** setup farmer (19,23) на BusStop: Front, Back проходим

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## eventHarveyMineRescue

- **Локация:** Mine → Hospital
- **Файл:** `eventsMineRescue.json` → `Mine`
- **Приоритет:** High
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 17 | 7 | Mine | Back проходим | OK |
| warp | Harvey | 17 | 10 | Mine | Back проходим | OK |
| warp | farmer | 20 | 5 | Hospital | Buildings, Back непроходим | OK (ignoreCollisions) |
| warp | Harvey | 19 | 5 | Hospital | Front, Back проходим | Warning |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (17,10) | move Harvey 0 -2 0 true | (17,8) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (17,7) | Mine | — | OK |
| (17,10) | Mine | — | OK |
| (20,5) | Hospital | Buildings на тайле | OK (ignoreCollisions) |
| (19,5) | Hospital | Front/AlwaysFront на тайле | Warning |

### Проблемы

- **[Medium]** warp Harvey (19,5) на Hospital: Front, Back проходим
- **[Low]** warp farmer (20,5) + positionOffset + ignoreCollisions — палата, Buildings ожидаем; Warning в TMX, OK с animate лёжа

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## eventHarveyMineRescueDating

- **Локация:** Mine → Hospital
- **Файл:** `eventsMineRescue.json` → `Mine`
- **Приоритет:** High
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 17 | 7 | Mine | Back проходим | OK |
| warp | Harvey | 17 | 10 | Mine | Back проходим | OK |
| warp | farmer | 20 | 5 | Hospital | Buildings, Back непроходим | OK (ignoreCollisions) |
| warp | Harvey | 19 | 5 | Hospital | Front, Back проходим | Warning |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (17,10) | move Harvey 0 -2 0 true | (17,8) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (17,7) | Mine | — | OK |
| (17,10) | Mine | — | OK |
| (20,5) | Hospital | Buildings на тайле | OK (ignoreCollisions) |
| (19,5) | Hospital | Front/AlwaysFront на тайле | Warning |

### Проблемы

- **[Medium]** warp Harvey (19,5) на Hospital: Front, Back проходим
- **[Low]** warp farmer (20,5) + positionOffset + ignoreCollisions — палата, Buildings ожидаем; Warning в TMX, OK с animate лёжа

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## eventHarveyStormComfortMountain

- **Локация:** Custom_AdventurerSummit → Mountain
- **Файл:** `events.json` → `Custom_AdventurerSummit`
- **Приоритет:** High
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 41 | 28 | Custom_AdventurerSummit | Back проходим | OK |
| warp | Harvey | 32 | 42 | Custom_AdventurerSummit | Back проходим | OK |
| warp | farmer | 79 | 1 | Mountain | Back проходим | OK |
| warp | Harvey | 79 | 0 | Mountain | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (32,42) | advancedMove Harvey false 0 -14 8 0 | (40,28) | (34,38)=Broken; (35,37)=Broken; (35,36)=Broken; (36,35)=Broken; (37,34)=Broken; (37,33)=Broken; advancedMove — проверить в игре | Warning |
| Harvey | (40,28) | advancedMove Harvey false -8 0 0 14 | (32,28) | advancedMove — проверить в игре | Warning |
| farmer | (41,28) | advancedMove farmer false -9 0 0 14 | (32,28) | advancedMove — проверить в игре | Warning |
| farmer | (79,1) | advancedMove farmer false 0 7 -2 0 0 7 -1 0 | (84,7) | (81,3)=Broken; advancedMove — проверить в игре | Warning |
| Harvey | (79,0) | advancedMove Harvey false 0 8 -2 0 0 6 -1 0 0 1 | (83,8) | (81,3)=Broken; advancedMove — проверить в игре | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (41,28) | Custom_AdventurerSummit | — | OK |
| (32,42) | Custom_AdventurerSummit | Warp→Mountain (31,43); Warp→Mountain (32,43); Warp→Mountain (33,43) | OK |
| (79,1) | Mountain | — | OK |
| (79,0) | Mountain | Warp→Custom_AdventurerSummit (78,-1); Warp→Custom_AdventurerSummit (79,-1); Warp→Custom_AdventurerSummit (80,-1) | OK |

### Проблемы

- **[Medium]** Путь Harvey advancedMove Harvey false 0 -14 8 0: (34,38)=Broken; (35,37)=Broken; (35,36)=Broken; (36,35)=Broken; (37,34)=Broken; (37,33)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove Harvey: финальная точка (40,28) — проверить траекторию в игре
- **[Medium]** advancedMove Harvey: финальная точка (32,28) — проверить траекторию в игре
- **[Medium]** advancedMove farmer: финальная точка (32,28) — проверить траекторию в игре
- **[Medium]** Путь farmer advancedMove farmer false 0 7 -2 0 0 7 -1 0: (81,3)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove farmer: финальная точка (84,7) — проверить траекторию в игре
- **[Medium]** Путь Harvey advancedMove Harvey false 0 8 -2 0 0 6 -1 0 0 1: (81,3)=Broken; advancedMove — проверить в игре
- **[Medium]** advancedMove Harvey: финальная точка (83,8) — проверить траекторию в игре
- **[Medium]** Mountain warp (79,1): Back проходим — край карты, SVE warp с Summit

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## eventHarveyStormComfortTown

- **Локация:** Town → Saloon
- **Файл:** `events.json` → `Town`
- **Приоритет:** High
- **Статус:** **Warning**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 39 | 73 | Town | Back проходим | OK |
| warp | Harvey | 36 | 56 | Town | Front, Back проходим | Warning |
| warp | farmer | 14 | 23 | Saloon | Back проходим | OK |
| warp | Harvey | 13 | 23 | Saloon | Front, Back проходим | Warning |
| warp | Gus | 13 | 18 | Saloon | Front, Back проходим | Warning |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (36,56) | advancedMove Harvey false 0 1 1 0 0 17 | (36,57) | advancedMove — проверить в игре | Warning |
| Harvey | (36,57) | advancedMove Harvey false 0 2 0 1 | (36,59) | advancedMove — проверить в игре | Warning |
| farmer | (39,73) | advancedMove farmer false 6 0 0 -2 | (45,73) | advancedMove — проверить в игре | Warning |
| Harvey | (36,59) | advancedMove Harvey false 7 0 0 -2 | (43,59) | advancedMove — проверить в игре | Warning |
| farmer | (14,23) | advancedMove farmer false 0 -2 4 0 | (18,21) | advancedMove — проверить в игре | Warning |
| Harvey | (13,23) | advancedMove Harvey false 0 -2 3 0 | (13,21) | advancedMove — проверить в игре | Warning |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (39,73) | Town | — | OK |
| (36,56) | Town | Action LockedDoorWarp 10 19 Hospital 900 1500 (36,55); Front/AlwaysFront на тайле | Warning |
| (14,23) | Saloon | — | OK |
| (13,23) | Saloon | Front/AlwaysFront на тайле | Warning |
| (13,18) | Saloon | Action Saloon (14,19); Front/AlwaysFront на тайле | Warning |

### Проблемы

- **[Medium]** warp Harvey (36,56) на Town: Front, Back проходим
- **[Medium]** advancedMove Harvey: финальная точка (36,57) — проверить траекторию в игре
- **[Medium]** advancedMove Harvey: финальная точка (36,59) — проверить траекторию в игре
- **[Medium]** advancedMove farmer: финальная точка (45,73) — проверить траекторию в игре
- **[Medium]** advancedMove Harvey: финальная точка (43,59) — проверить траекторию в игре
- **[Medium]** warp Harvey (13,23) на Saloon: Front, Back проходим
- **[Medium]** warp Gus (13,18) на Saloon: Front, Back проходим
- **[Medium]** advancedMove farmer: финальная точка (18,21) — проверить траекторию в игре
- **[Medium]** advancedMove Harvey: финальная точка (13,21) — проверить траекторию в игре

### Рекомендации

- Провести in-game тест на runtime-карте после SVE Load.

---

## HarveyOverhaulStory.E1_SlipperyPath

- **Локация:** BusStop
- **Файл:** `events.json` → `BusStop`
- **Приоритет:** High
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 20 | 23 | BusStop | Back проходим | OK |
| setup | Harvey | 26 | 22 | BusStop | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (20,23) | move farmer 3 0 1 true | (23,23) | прямой | OK |
| Harvey | (26,22) | move Harvey -3 0 3 true | (23,22) | прямой | OK |
| Harvey | (23,22) | move Harvey -2 0 3 true | (21,22) | прямой | OK |
| farmer | (23,23) | move farmer -3 0 3 true | (20,23) | прямой | OK |
| Harvey | (21,22) | move Harvey -3 0 3 true | (18,22) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (20,23) | BusStop | — | OK |
| (26,22) | BusStop | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## HarveyOverhaulStory.E2B_QuietAgreement

- **Локация:** Town
- **Файл:** `events.json` → `Town`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 28 | 67 | Town | Back проходим | OK |
| setup | Harvey | 32 | 67 | Town | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (32,67) | move Harvey -2 0 3 true | (30,67) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (28,67) | Town | — | OK |
| (32,67) | Town | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## HarveyOverhaulStory.E3B_WingPatient

- **Локация:** Forest
- **Файл:** `events.json` → `Forest`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 48 | 14 | Forest | Back проходим | OK |
| setup | Harvey | 49 | 14 | Forest | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (48,14) | move farmer -2 0 3 true | (46,14) | прямой | OK |
| Harvey | (49,14) | move Harvey -2 0 3 true | (47,14) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (48,14) | Forest | — | OK |
| (49,14) | Forest | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## HarveyOverhaulStory.E3_ForestApothecary

- **Локация:** Forest
- **Файл:** `events.json` → `Forest`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 50 | 13 | Forest | Back проходим | OK |
| setup | Harvey | 51 | 13 | Forest | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (50,13) | move farmer -3 0 3 true | (47,13) | прямой | OK |
| Harvey | (51,13) | move Harvey -3 0 3 true | (48,13) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (50,13) | Forest | — | OK |
| (51,13) | Forest | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## HarveyOverhaulStory.E4B_TooQuiet

- **Локация:** Mountain
- **Файл:** `events.json` → `Mountain`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 42 | 21 | Mountain | Back проходим | OK |
| setup | Harvey | 45 | 21 | Mountain | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (42,21) | move farmer 2 0 1 true | (44,21) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (42,21) | Mountain | — | OK |
| (45,21) | Mountain | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## HarveyOverhaulStory.E4_PierBreath

- **Локация:** Beach
- **Файл:** `events.json` → `Beach`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 40 | 17 | Beach | Back проходим | OK |
| setup | Harvey | 39 | 23 | Beach | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| farmer | (40,17) | move farmer 0 6 2 | (40,23) | прямой | OK |
| farmer | (40,23) | move farmer -1 0 3 | (39,23) | прямой | OK |
| farmer | (39,23) | move farmer 0 -10 0 true | (39,13) | прямой | OK |
| Harvey | (39,23) | move Harvey 0 -10 0 true | (39,13) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (40,17) | Beach | — | OK |
| (39,23) | Beach | — | OK |

### Проблемы

- **[High]** Harvey и farmer на одном тайле в конце сценария

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## eventHarveyMineInterception

- **Локация:** Mine
- **Файл:** `eventsCare.json` → `Mine`
- **Приоритет:** High
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 17 | 7 | Mine | Back проходим | OK |
| setup | Harvey | 17 | 10 | Mine | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (17,10) | move Harvey 0 -2 0 | (17,8) | прямой | OK |
| Harvey | (17,8) | move Harvey -1 0 0 | (16,8) | прямой | OK |
| Harvey | (16,8) | move Harvey 0 -1 1 | (16,7) | прямой | OK |
| Harvey | (16,7) | move Harvey 0 3 2 true | (16,10) | прямой | OK |
| farmer | (17,7) | move farmer 0 3 2 true | (17,10) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (17,7) | Mine | — | OK |
| (17,10) | Mine | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## eventHarveyMinorMineRescue

- **Локация:** Mine → Hospital
- **Файл:** `eventsMineRescue.json` → `Mine`
- **Приоритет:** High
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 17 | 7 | Mine | Back проходим | OK |
| setup | Harvey | 17 | 10 | Mine | Back проходим | OK |
| warp | farmer | 14 | 6 | Hospital | Back проходим | OK |
| warp | Harvey | 15 | 6 | Hospital | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (17,10) | move Harvey 0 -2 0 true | (17,8) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (17,7) | Mine | — | OK |
| (17,10) | Mine | — | OK |
| (14,6) | Hospital | — | OK |
| (15,6) | Hospital | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## eventHarveySkullCavePrevention

- **Локация:** SkullCave
- **Файл:** `eventsCare.json` → `SkullCave`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 5 | 5 | SkullCave | Back проходим | OK |
| setup | Harvey | 7 | 7 | SkullCave | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (7,7) | move Harvey 0 -2 3 | (7,5) | прямой | OK |
| Harvey | (7,5) | move Harvey -1 0 3 | (6,5) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (5,5) | SkullCave | — | OK |
| (7,7) | SkullCave | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## eventHarveyStormComfortForest

- **Локация:** Forest
- **Файл:** `events.json` → `Forest`
- **Приоритет:** Medium
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 23 | 13 | Forest | Back проходим | OK |
| warp | Harvey | 35 | 13 | Forest | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (35,13) | move Harvey -11 0 3 | (24,13) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (23,13) | Forest | — | OK |
| (35,13) | Forest | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## eventHarveyStormComfortMine

- **Локация:** Mine → Town
- **Файл:** `events.json` → `Mine`
- **Приоритет:** High
- **Статус:** **OK**

### Использованные координаты

| Команда | Actor | X | Y | Loc | Проверка тайла | Результат |
|---------|-------|---|---|-----|----------------|-----------|
| setup | farmer | 15 | 5 | Mine | Back проходим | OK |
| warp | Harvey | 18 | 13 | Mine | Back проходим | OK |
| warp | farmer | 72 | 22 | Town | Back проходим | OK |
| warp | Harvey | 73 | 22 | Town | Back проходим | OK |

### Движение

| Actor | From | Move command | To | Проходимость пути | Результат |
|-------|------|--------------|-----|-------------------|-----------|
| Harvey | (18,13) | move Harvey 0 -8 3 | (18,5) | прямой | OK |
| Harvey | (18,5) | move Harvey -2 0 0 | (16,5) | прямой | OK |
| Harvey | (16,5) | move Harvey 0 2 2 | (16,7) | прямой | OK |
| farmer | (15,5) | move farmer 0 2 2 | (15,7) | прямой | OK |

### Объекты рядом

| Координата | Loc | Что рядом | Риск |
|------------|-----|-----------|------|
| (15,5) | Mine | — | OK |
| (18,13) | Mine | Warp→Custom_AdventurerSummit (18,14) | OK |
| (72,22) | Town | — | OK |
| (73,22) | Town | — | OK |

### Проблемы

- Нет проблем по TMX (runtime/SVE-патчи всё ещё проверить в игре).

### Рекомендации

- Координаты согласуются с TMX; финальная проверка — один прогон в игре с SVE.

---

## Методология

- Проходимость: Back≠0, Buildings=0, без Passable=F (TMX SVE/vanilla).
- `ignoreCollisions` учитывается для farmer в mine-rescue палате.
- `advancedMove` — эвристика конечной точки + **Warning** (нужен in-game).
- Saloon — vanilla TMX; SVE `.tbin` может отличаться.
- SkullCave/Mine — фиксированные координаты на входе, не процедурные этажи.
