"""Fix literal \\n tokens that were inserted outside JSON string values."""
from __future__ import annotations

import json
import pathlib
import re

ROOT = pathlib.Path(
    r"D:/Games/Steam/steamapps/common/Stardew Valley/Mods/HarveyOverhaul/HarveyOverhaul [CP]/assets/Code"
)


def fix_structural_newlines(text: str) -> str:
    text = re.sub(r",\\n\"", ",\n\"", text)
    text = re.sub(r":\\n\"", ":\n\"", text)
    return text


def strip_json_comments(text: str) -> str:
    """Remove // and /* */ comments for validation only."""
    out: list[str] = []
    i = 0
    in_string = False
    escape = False
    while i < len(text):
        ch = text[i]
        if in_string:
            out.append(ch)
            if escape:
                escape = False
            elif ch == "\\":
                escape = True
            elif ch == '"':
                in_string = False
            i += 1
            continue

        if ch == '"':
            in_string = True
            out.append(ch)
            i += 1
            continue

        if ch == "/" and i + 1 < len(text):
            nxt = text[i + 1]
            if nxt == "/":
                i += 2
                while i < len(text) and text[i] not in "\r\n":
                    i += 1
                continue
            if nxt == "*":
                i += 2
                while i + 1 < len(text) and not (text[i] == "*" and text[i + 1] == "/"):
                    i += 1
                i = min(i + 2, len(text))
                continue

        out.append(ch)
        i += 1
    return "".join(out)


def main() -> None:
    for path in sorted(ROOT.glob("buffs*.json")):
        raw = path.read_text(encoding="utf-8")
        fixed = fix_structural_newlines(raw)
        if fixed == raw:
            continue

        try:
            json.loads(strip_json_comments(fixed))
        except json.JSONDecodeError as exc:
            print(f"FAIL {path.name}: {exc}")
            continue

        path.write_text(fixed, encoding="utf-8")
        print(f"fixed: {path.name}")


def validate_all() -> int:
    bad = 0
    for path in sorted(ROOT.glob("buffs*.json")):
        raw = path.read_text(encoding="utf-8")
        try:
            json.loads(strip_json_comments(raw))
            print(f"OK: {path.name}")
        except json.JSONDecodeError as exc:
            print(f"BAD: {path.name}: {exc}")
            bad += 1
    return bad


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1 and sys.argv[1] == "--validate":
        raise SystemExit(validate_all())
    main()
