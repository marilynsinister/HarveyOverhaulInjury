#!/usr/bin/env python3
"""Find dialogue strings where $action is not preceded by # (raw text scan)."""
import re
from pathlib import Path

ROOTS = [
    Path(r"D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]"),
    Path(r"c:\Users\Admin\HarveyStressMeter"),
    Path(r"c:\Users\Admin\HarveyOverhaulInjury"),
]

BAD_PATTERN = re.compile(r"(?<!#)\$action")
ENTRY_PATTERN = re.compile(r'"([^"]+)":\s*"((?:\\.|[^"\\])*)"')


def scan_file(fp: str):
    try:
        with open(fp, encoding="utf-8") as f:
            text = f.read()
    except OSError:
        return [], 0, 0

    total = text.count("$action")
    hash_total = text.count("#$action")
    issues = []

    for key, value in ENTRY_PATTERN.findall(text):
        if "$action" not in value:
            continue
        for m in BAD_PATTERN.finditer(value):
            start = max(0, m.start() - 50)
            end = min(len(value), m.end() + 50)
            issues.append((key, "..." + value[start:end] + "..."))

    return issues, total, hash_total


def main():
    all_issues = []
    total_action = 0
    total_hash_action = 0

    for root in ROOTS:
        if not root.exists():
            continue
        for fp in root.rglob("*.json"):
            issues, total, hash_total = scan_file(str(fp))
            total_action += total
            total_hash_action += hash_total
            for key, snippet in issues:
                all_issues.append((str(fp), key, snippet))

    print(f"Total $action occurrences: {total_action}")
    print(f"Total #$action occurrences: {total_hash_action}")
    print(f"Dialogue issues (no # before $action): {len(all_issues)}")
    for fp, key, snippet in all_issues:
        print("---")
        print(fp)
        print(key)
        print(snippet)


if __name__ == "__main__":
    main()
