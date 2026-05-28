"""Re-wrap buff Description fields (~32 chars/line, JSON \\n escapes)."""
from __future__ import annotations

import pathlib
import re

MAX_LINE = 32
ROOT = pathlib.Path(
    r"D:/Games/Steam/steamapps/common/Stardew Valley/Mods/HarveyOverhaul/HarveyOverhaul [CP]/assets/Code"
)
PAT = re.compile(r'"Description": "((?:[^"\\]|\\.)*)"')


def wrap_text(text: str) -> str:
    text = re.sub(r"\s+", " ", text.strip())
    if len(text) <= MAX_LINE:
        return text

    sentences = re.split(r"(?<=[.!?])\s+", text)
    chunks: list[str] = []
    for sent in sentences:
        sent = sent.strip()
        if not sent:
            continue
        if len(sent) <= MAX_LINE:
            chunks.append(sent)
            continue
        for part in re.split(r"(?<=[,;—])\s+", sent):
            part = part.strip()
            if not part:
                continue
            if len(part) <= MAX_LINE:
                chunks.append(part)
            else:
                chunks.extend(_wrap_words(part))

    lines: list[str] = []
    for c in chunks:
        if lines and len(lines[-1]) + 1 + len(c) <= MAX_LINE:
            lines[-1] = f"{lines[-1]} {c}"
        elif len(c) <= MAX_LINE:
            lines.append(c)
        else:
            lines.extend(_wrap_words(c))

    # Do not end a line on ":" or "—" alone with continuation.
    i = 0
    while i < len(lines) - 1:
        if lines[i].rstrip().endswith((":", "—")) and len(lines[i]) + 1 + len(lines[i + 1]) <= MAX_LINE + 6:
            lines[i] = f"{lines[i]} {lines[i + 1]}"
            del lines[i + 1]
        else:
            i += 1

    return "\\n".join(lines)


def _wrap_words(text: str) -> list[str]:
    words = text.split()
    lines: list[str] = []
    cur = ""
    for w in words:
        candidate = w if not cur else f"{cur} {w}"
        if len(candidate) <= MAX_LINE:
            cur = candidate
        else:
            if cur:
                lines.append(cur)
            cur = w
    if cur:
        lines.append(cur)
    return lines


def main() -> None:
    changed = 0
    for path in sorted(ROOT.glob("buffs*.json")):
        raw = path.read_text(encoding="utf-8")

        def repl(m: re.Match[str]) -> str:
            nonlocal changed
            body = m.group(1).replace("\\n", " ").replace("\\\\", "\\")
            wrapped = wrap_text(body)
            if wrapped != m.group(1):
                changed += 1
            return f'"Description": "{wrapped}"'

        new = PAT.sub(repl, raw)
        if new != raw:
            path.write_text(new, encoding="utf-8")
            print(path.name)
    print(f"updated: {changed}")


if __name__ == "__main__":
    main()
