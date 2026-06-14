# -*- coding: utf-8 -*-
import re
import pathlib

CP = pathlib.Path(
    r"D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code"
)


def main():
    # 1. Recovery plan
    p = CP / "dialoguesHarveyRecoveryPlan.json"
    t = p.read_text(encoding="utf-8")
    t2 = t.replace("дорогая", "солнышко").replace("Дорогая", "Солнышко")
    if t2 != t:
        p.write_text(t2, encoding="utf-8")
        print("recovery plan: updated")

    # 2. harvey_married
    p = CP / "dialogues" / "harvey_married.json"
    t = p.read_text(encoding="utf-8")
    repls = [
        ("Ты — моя смелость, малышка.", "Ты — моя смелость, моя девочка."),
        ("Ты — мой дом, малышка.", "Ты — мой дом, любимая."),
        ("Дом был пустым до тебя, малышка.", "Дом был пустым до тебя, солнышко."),
        ("Ты — главная звезда, малышка.", "Ты — главная звезда, любимая."),
        ("Ты смелее, чем думаешь, малышка.", "Ты смелее, чем думаешь, моя девочка."),
        ("Не тороплю, малышка.", "Не тороплю, солнышко."),
        ("Спасибо, малышка.", "Спасибо, любимая."),
    ]
    for a, b in repls:
        t = t.replace(a, b)
    p.write_text(t, encoding="utf-8")
    print("harvey_married: updated")

    # 3. priority2
    p = CP / "dialogues" / "harvey_priority2.json"
    t = p.read_text(encoding="utf-8")
    t = t.replace("С праздником, малышка.", "С праздником, солнышко.")
    p.write_text(t, encoding="utf-8")

    # 4. events comment
    p = CP / "events.json"
    t = p.read_text(encoding="utf-8")
    t = t.replace(
        '// speak Harvey \\"Дорогая! Проснись!',
        '// speak Harvey \\"Солнышко! Проснись!',
    )
    p.write_text(t, encoding="utf-8")

    # 5. harvey_gifts married block
    p = CP / "dialogues" / "harvey_gifts.json"
    t = p.read_text(encoding="utf-8")
    idx = t.find('"Relationship:Harvey": "Married"')
    if idx >= 0:
        before, after = t[:idx], t[idx:]
        terms = ["солнышко", "любимая", "моя девочка", "солнышко", "любимая"]
        counter = [0]

        def repl_kot(m):
            term = terms[counter[0] % len(terms)]
            counter[0] += 1
            word = m.group(0)
            if word[0] == "К":
                if term == "моя девочка":
                    return "Моя девочка"
                return term[0].upper() + term[1:]
            return term

        after = re.sub(r"[Кк]отёнок", repl_kot, after)
        t = before + after
    p.write_text(t, encoding="utf-8")
    print("harvey_gifts: updated")

    # verify
    patterns = ["Дорогая", "дорогая", "котёнок", "Котёнок", "малышка", "хрупк"]
    for pat in patterns:
        files = []
        for f in CP.rglob("*.json"):
            try:
                if pat in f.read_text(encoding="utf-8", errors="ignore"):
                    files.append(str(f.relative_to(CP)))
            except OSError:
                pass
        print(f"{pat}: {len(files)} files")
        for fn in files[:15]:
            print(f"  - {fn}")


if __name__ == "__main__":
    main()
