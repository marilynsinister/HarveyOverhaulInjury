import re
from pathlib import Path
from collections import defaultdict

CP = Path(r"D:/Games/Steam/steamapps/common/Stardew Valley/Mods/HarveyOverhaul/HarveyOverhaul [CP]")

KEY_RE = re.compile(
    r'"((?:topic\w+Cured|Recovery_Complete_\w+|PhaseTransition_\w+|Treat_\w+_(?:Before|After)\d+|topicTreatment\w+|topic\w+Phase(?:Acute|Healing|Recovery)))"\s*:'
)

PHASED = {
    "DeepCuts": [2, 3],
    "FracturedBone": [2, 3],
    "Concussion": [2, 3],
    "TornMuscles": [2, 3],
    "ShrapnelWounds": [2, 3],
    "BurnWounds": [2],
    "InfectedWound": [2],
    "BackStrain": [2],
    "BruisedRibs": [2],
    "SprainedAnkle": [2],
    "Cold": [2],
}

TARGET_FILES = [
    "assets/Code/dialoguesHarveyCure.json",
    "assets/Code/dialoguesHarveyInjury.json",
    "assets/Code/dialoguesHarvey.json",
    "assets/Code/dialoguesHarveyMedicalCare.json",
    "assets/Code/dialoguesHarveyCare.json",
]


def extract_patches(text: str):
    """Split by Change blocks and extract When + keys."""
    patches = []
    for m in re.finditer(
        r'"Target"\s*:\s*"Characters/Dialogue/Harvey"[\s\S]*?"Entries"\s*:\s*\{',
        text,
    ):
        start = m.start()
        # find When before Entries
        chunk = text[max(0, start - 800) : m.end()]
        when_m = re.search(r'"When"\s*:\s*(\{[\s\S]*?\})\s*,', chunk)
        when = when_m.group(1) if when_m else "{}"
        prio_m = re.search(r'"Priority"\s*:\s*"([^"]+)"', chunk)
        prio = prio_m.group(1) if prio_m else ""

        # extract keys until closing of Entries - naive brace match
        pos = m.end()
        depth = 1
        while pos < len(text) and depth > 0:
            if text[pos] == "{":
                depth += 1
            elif text[pos] == "}":
                depth -= 1
            pos += 1
        entries_body = text[m.end() : pos - 1]
        keys = KEY_RE.findall(entries_body)
        patches.append((when, prio, keys))
    return patches


def main():
    global_keys: dict[str, list] = defaultdict(list)
    dupes_in_patch = []

    for rel in TARGET_FILES:
        path = CP / rel
        if not path.exists():
            continue
        text = path.read_text(encoding="utf-8")
        for when, prio, keys in extract_patches(text):
            seen = set()
            for k in keys:
                if k in seen:
                    dupes_in_patch.append((k, rel, when))
                seen.add(k)
                global_keys[k].append((rel, when, prio))

    print("=== DUPLICATES WITHIN SAME PATCH ===")
    for item in dupes_in_patch:
        print(f"  {item[0]} @ {item[1]} When={item[2][:60]}")
    print(f"Total: {len(dupes_in_patch)}")

    print("\n=== CROSS-FILE SAME When ===")
    conflicts = []
    for key, locs in sorted(global_keys.items()):
        by_when: dict[str, set] = defaultdict(set)
        for rel, when, _prio in locs:
            by_when[when].add(rel)
        for when, files in by_when.items():
            if len(files) > 1:
                conflicts.append((key, when, sorted(files)))
    for key, when, files in conflicts:
        print(f"  {key}: {files} When={when[:70]}")
    print(f"Total conflicts: {len(conflicts)}")

    print("\n=== PhaseTransition by When (Injury) ===")
    pt_by_when: dict[str, set] = defaultdict(set)
    for key, locs in global_keys.items():
        if not key.startswith("PhaseTransition_"):
            continue
        for rel, when, _prio in locs:
            if "Injury" in rel:
                pt_by_when[when].add(key)
    for when, keys in sorted(pt_by_when.items(), key=lambda x: -len(x[1])):
        hearts = re.search(r'"Hearts:Harvey"\s*:\s*"([^"]+)"', when)
        label = hearts.group(1) if hearts else when[:40]
        print(f"  [{len(keys)}] hearts={label}")

    def keys_for_hearts(hearts_val: str) -> set[str]:
        result = set()
        for when, keys in pt_by_when.items():
            if f'"Hearts:Harvey": "{hearts_val}"' in when.replace(" ", ""):
                result |= keys
            elif hearts_val in when:
                result |= keys
        return result

    h02 = keys_for_hearts("0,1,2")
    h35 = keys_for_hearts("3,4,5")
    h610 = keys_for_hearts("6,7,8,9,10")
    print(f"\n0-2: {len(h02)}, 3-5: {len(h35)}, 6-10: {len(h610)}")
    if h02 - h35:
        print("Missing in 3-5 vs 0-2:")
        for k in sorted(h02 - h35):
            print(f"  - {k}")
    if h02 - h610:
        print(f"Missing in 6-10 vs 0-2 ({len(h02 - h610)}):")
        for k in sorted(h02 - h610):
            print(f"  - {k}")

    print("\n=== MISSING PhaseTransition (global) ===")
    all_keys = set(global_keys.keys())
    missing = []
    for inj, phases in PHASED.items():
        for ph in phases:
            base = f"PhaseTransition_{inj}_{ph}"
            matches = [k for k in all_keys if k == base or k.startswith(base + "_")]
            if not matches:
                missing.append(base)
                print(f"  MISSING: {base}")
            else:
                print(f"  OK {base}: {matches}")

    print("\n=== Recovery_Complete by When (Cure) ===")
    rc_by_when: dict[str, set] = defaultdict(set)
    for key, locs in global_keys.items():
        if not key.startswith("Recovery_Complete_"):
            continue
        for _rel, when, _prio in locs:
            rc_by_when[when].add(key)
    for when, keys in sorted(rc_by_when.items(), key=lambda x: -len(x[1])):
        print(f"  [{len(keys)}] {when}")

    print("\n=== Dual final path ===")
    for key in sorted(global_keys):
        m = re.match(r"Recovery_Complete_(\w+)_", key)
        if not m:
            continue
        inj = m.group(1)
        cured = f"topic{inj}Cured"
        if cured in global_keys:
            print(f"  {inj}: Recovery_Complete + {cured}")

    print("\n=== Key counts by category ===")
    cats = defaultdict(int)
    for key in global_keys:
        if key.startswith("PhaseTransition_"):
            cats["PhaseTransition"] += 1
        elif key.startswith("Recovery_Complete_"):
            cats["Recovery_Complete"] += 1
        elif key.endswith("Cured"):
            cats["topicCured"] += 1
        elif key.startswith("topicTreatment"):
            cats["topicTreatment"] += 1
        elif "Phase" in key and key.startswith("topic"):
            cats["topicPhase"] += 1
        elif "_Before" in key:
            cats["Treat_Before"] += 1
        elif "_After" in key:
            cats["Treat_After"] += 1
    for c, n in sorted(cats.items()):
        print(f"  {c}: {n}")


if __name__ == "__main__":
    main()
