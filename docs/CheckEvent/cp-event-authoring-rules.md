# CP Event Authoring Rules (CheckEvent)

Краткий справочник для аудита CP-событий HarveyOverhaul в контексте CheckEvent.

**Полная версия:** [`docs/EventPatterns/cp-event-authoring-rules.md`](../EventPatterns/cp-event-authoring-rules.md)  
**Паспорта карт:** [`map-passports.md`](map-passports.md)  
**Чеклист ревью:** [`cp-event-review-checklist.md`](cp-event-review-checklist.md)

---

## Animation / showFrame — сидение farmer

### Правило (подтверждено in-game)

**Сидение farmer:** `showFrame farmer 107` при **`faceDirection farmer 2`** (лицом к камере / вниз).

Кадр `107` без предшествующего `stopAnimation` + `faceDirection 2` может оставить farmer в walking/back frame — порядок команд обязателен.

### Предупреждение: showFrame не перемещает actor

`showFrame` меняет кадр спрайта, но **не перемещает** actor на другой тайл.  
Перед `showFrame farmer 107` farmer **уже должна стоять** на правильном **final sitting tile** (явные `move` / `warp`, не `positionOffset` как компенсация).

### Правильный порядок посадки

1. actor доходит до **final sitting tile** (сверить по [`maps/Hospital.md`](maps/Hospital.md));
2. `stopAnimation farmer`;
3. **`faceDirection farmer 2`**;
4. **`showFrame farmer 107`**;
5. `pause 300–500`.

### Антипаттерн

**Плохо:**

```text
advancedMove farmer false -1 0 0 -1/
showFrame farmer 107/
```

**Почему плохо:**

- маршрут может не довести до стула;
- нет `stopAnimation`;
- нет **`faceDirection farmer 2`** перед showFrame;
- farmer может зависнуть в walking/back frame.

### Лучше (стул, farmer лицом к камере)

```text
move farmer -1 0 3/
move farmer 0 -2 0/
stopAnimation farmer/
faceDirection farmer 2/
showFrame farmer 107/
pause 300/
move Harvey 1 0 0/
move Harvey 0 -1 0/
stopAnimation Harvey/
faceDirection Harvey 0/
pause 300/
```

Из `(6,6)`: farmer → **`(5,4)`**; Harvey из `(4,6)` → **`(5,5)`**. Harvey **`faceDirection 0`** (спиной к камере, лицом к farmer). См. [`maps/HarveyRoom.md`](maps/HarveyRoom.md), [`maps/Hospital.md`](maps/Hospital.md).

**Примечание:** `showFrame farmer true 117` встречается в других сценах (напр. `eventHarveyCheckup`) — не путать с паттерном стула **`107` + face `2`**.

### Harvey — медицинская анимация после посадки

```text
move Harvey 1 0 0/
move Harvey 0 -1 0/
stopAnimation Harvey/
faceDirection Harvey 0/
pause 300/
animate Harvey false true 1000 22 20 21 20/
pause 1200/
stopAnimation Harvey/
faceDirection Harvey 0/
emote Harvey 32/
pause 500/
```

**Не использовать** `positionOffset` как компенсацию того, что farmer не дошла до тайла.
