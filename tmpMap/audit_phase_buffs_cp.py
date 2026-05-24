#!/usr/bin/env python3
"""Cross-check C# phase buff IDs vs CP Data/Buffs."""
import json
import re
from pathlib import Path

CS = Path(r"c:/Users/Admin/HarveyOverhaulInjury")
CP_DIR = Path(
    r"D:/Games/Steam/steamapps/common/Stardew Valley/Mods/HarveyOverhaul/HarveyOverhaul [CP]/assets/Code"
)

text = (CS / "Managers/InjuryManager.cs").read_text(encoding="utf-8")
tm = (CS / "Managers/TreatmentManager.cs").read_text(encoding="utf-8")
phased = re.findall(r'"(buff[A-Za-z]+)"', tm.split("PhasedInjuries")[1].split("};")[0])

injuries = {}
cur = None
for line in text.splitlines():
    m = re.search(r'\["(buff[A-Za-z]+)"\]', line)
    if m:
        cur = m.group(1)
        injuries[cur] = {}
    m2 = re.search(r'\[(\d+)\]\s*=\s*"([^"]+)"', line)
    if m2 and cur:
        injuries[cur][int(m2.group(1))] = m2.group(2)

durations = {
    "buffSprainedAnkle": (7, 7, 0),
    "buffBruisedRibs": (10, 11, 0),
    "buffBackStrain": (5, 7, 0),
    "buffDeepCuts": (3, 7, 4),
    "buffBurnWounds": (7, 14, 0),
    "buffInfectedWound": (3, 11, 0),
    "buffTornMuscles": (7, 14, 7),
    "buffConcussion": (3, 11, 7),
    "buffFracturedBone": (7, 35, 14),
    "buffShrapnelWounds": (5, 10, 7),
    "buffCold": (3, 4, 0),
}


def total_phases(p1, p2, p3):
    return 3 if p3 > 0 else (2 if p2 > 0 else 0)


def strip_json_comments(raw: str) -> str:
    out = []
    for line in raw.splitlines():
        if line.strip().startswith("//"):
            continue
        if "//" in line:
            in_str = esc = False
            cut = len(line)
            for i, ch in enumerate(line):
                if esc:
                    esc = False
                    continue
                if ch == "\\":
                    esc = True
                    continue
                if ch == '"':
                    in_str = not in_str
                    continue
                if not in_str and line[i : i + 2] == "//":
                    cut = i
                    break
            line = line[:cut]
        out.append(line)
    raw = "\n".join(out)
    return re.sub(r",(\s*[}\]])", r"\1", raw)


cp_buffs = {}
for fn in ["buffsCure.json", "buffsInjury.json", "buffsCureStress.json", "buffsStress.json"]:
    p = CP_DIR / fn
    if not p.exists():
        continue
    data = json.loads(strip_json_comments(p.read_text(encoding="utf-8")))
    entries = data.get("Changes", [{}])[0].get("Entries", {})
    for k, v in entries.items():
        cp_buffs[k] = fn

print("=== TABLE ===")
for inj in phased:
    p1, p2, p3 = durations[inj]
    tp = total_phases(p1, p2, p3)
    phases = injuries.get(inj, {})
    p1b, p2b, p3b = phases.get(1, "?"), phases.get(2, "?"), phases.get(3, "?")
    cp1 = "n/a" if tp < 1 else ("YES" if p1b in cp_buffs else "NO")
    cp2 = "n/a" if tp < 2 else ("YES" if p2b in cp_buffs else "NO")
    cp3 = "n/a" if tp < 3 else ("YES" if p3b in cp_buffs else "NO")
    base = "YES" if inj in cp_buffs else "NO"
    alias = tp == 2 and p2b == p3b
    print(
        f"{inj}|{tp}|{p1b}|{p2b}|{p3b}|base:{base} p1:{cp1} p2:{cp2} p3:{cp3}|alias3:{alias}|base_file:{cp_buffs.get(inj,'?')}"
    )

csharp_phase = {v for m in injuries.values() for v in m.values()}
cp_phase = {
    b
    for b in cp_buffs
    if b.startswith("HarveyMod_")
    and any(
        x in b
        for x in [
            "Acute",
            "Healing",
            "Recovery",
            "Cast",
            "Rest",
            "Limited",
            "Surgery",
            "Rehab",
            "Treatment",
        ]
    )
}
print("\nCP phase buffs not in C#:", sorted(cp_phase - csharp_phase))
print("C# phase buffs not in CP:", sorted(csharp_phase - set(cp_buffs.keys())))

print("\nRemoveAllPhaseBuffs unique IDs per injury:")
for inj in phased:
    tp = total_phases(*durations[inj])
    phases = injuries[inj]
    remove_set = {phases[1], phases[2], phases[3], inj}
    used = {phases[i] for i in range(1, tp + 1)} | {inj}
    print(inj, "OK" if used <= remove_set else "GAP", "used=", sorted(used), "remove=", sorted(remove_set))
