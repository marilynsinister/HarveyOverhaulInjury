# -*- coding: utf-8 -*-
import pathlib

files = [
    pathlib.Path(
        r"D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code\questsCure.json"
    ),
    pathlib.Path(
        r"D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code\dialogues\harvey_dating.json"
    ),
    pathlib.Path(
        r"D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code\dialoguesHarveyCareTrust.json"
    ),
]

repls = [
    (
        "Теперь ты под моей защитой, и я не позволю ничему причинить тебе вред.",
        "Теперь ты под моей защитой — я не отпущу тебя к нагрузке, пока не убедимся, что всё в порядке.",
    ),
    (
        "Но не бойся — я здесь, и я не позволю тебе умереть.",
        "Но не бойся — я здесь, и я доведу лечение до конца.",
    ),
    (
        "и не позволю тебе снять повязку раньше времени.",
        "и прослежу, чтобы повязка оставалась до срока.",
    ),
    (
        "Теперь ты под моей защитой, и я не позволю ничему навредить тебе.",
        "Теперь ты под моей защитой — я буду рядом, пока риск не спадёт.",
    ),
    (
        "Я не позволю тебе снова довести себя до обморока на холоде. Потому что ты мне дорога.",
        "Мне страшно снова видеть тебя на грани обморока на холоде. Ты мне слишком дорога.",
    ),
    (
        "Потому что я не позволю тебе снова довести себя до обморока на солнце.",
        "Мне страшно снова видеть тебя без сознания на солнце.",
    ),
    (
        "Дома тоже действует протокол — я не позволю тебе снова игнорировать боль ради дел.",
        "Дома тоже действует протокол — я не отпущу тебя к нагрузке, пока боль не станет управляемой.",
    ),
]

for p in files:
    t = p.read_text(encoding="utf-8")
    orig = t
    for a, b in repls:
        t = t.replace(a, b)
    if t != orig:
        p.write_text(t, encoding="utf-8")
        print(f"updated {p.name}")
    else:
        print(f"no change {p.name}")

for p in files:
    c = p.read_text(encoding="utf-8").count("не позволю")
    print(f"{p.name}: не позволю = {c}")
