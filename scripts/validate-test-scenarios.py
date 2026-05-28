#!/usr/bin/env python3
"""
Валидатор JSON-сценариев Harvey Overhaul Injury.
Без внешних зависимостей — только stdlib.
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

# Корень репозитория (scripts/..)
REPO_ROOT = Path(__file__).resolve().parent.parent
SCENARIOS_DIR = REPO_ROOT / "docs" / "testing" / "scenarios"

SUITE_ID_PATTERN = re.compile(r"^[a-z0-9][a-z0-9-]*$")
TEST_ID_PATTERN = re.compile(r"^HOI-[A-Z0-9-]+$")
PRIORITIES = frozenset({"P0", "P1", "P2", "P3"})
STEP_TYPES = frozenset({"smapi_command", "stardewmcp", "manual", "observe", "wait", "note"})
CLEANUP_TYPES = frozenset({"smapi_command", "stardewmcp", "manual", "note"})

SUITE_REQUIRED = ("suiteId", "suiteName", "description", "requires", "tests")
REQUIRES_REQUIRED = ("stardewMcp", "smapiConsole", "manualClick", "debugCommands")
TEST_REQUIRED = ("id", "title", "priority", "steps", "expected", "cleanup")
INDEX_SUITE_REQUIRED = ("suiteId", "file", "title", "testCount")


class Reporter:
    def __init__(self) -> None:
        self.errors: list[str] = []
        self.warnings: list[str] = []

    def error(self, msg: str) -> None:
        self.errors.append(msg)

    def warn(self, msg: str) -> None:
        self.warnings.append(msg)

    @property
    def ok(self) -> bool:
        return len(self.errors) == 0


def load_json(path: Path, r: Reporter) -> object | None:
    try:
        text = path.read_text(encoding="utf-8")
    except OSError as e:
        r.error(f"{path}: не удалось прочитать файл — {e}")
        return None
    try:
        return json.loads(text)
    except json.JSONDecodeError as e:
        r.error(f"{path}: невалидный JSON — {e}")
        return None


def is_smoke_test(test: dict, suite_id: str) -> bool:
    if suite_id.startswith("00-smoke"):
        return True
    mechanics = test.get("mechanics")
    if isinstance(mechanics, list) and "smoke" in mechanics:
        return True
    return False


def cleanup_has_injury_reset(cleanup: list) -> bool:
    for step in cleanup:
        if not isinstance(step, dict):
            continue
        if step.get("type") == "smapi_command" and step.get("command") == "injury_reset":
            return True
    return False


def validate_step(step: dict, ctx: str, r: Reporter, allowed_types: frozenset[str]) -> None:
    if not isinstance(step, dict):
        r.error(f"{ctx}: шаг должен быть объектом")
        return
    stype = step.get("type")
    if stype not in allowed_types:
        r.error(f"{ctx}: неизвестный type={stype!r}, ожидается один из {sorted(allowed_types)}")
        return
    desc = step.get("description")
    if not isinstance(desc, str) or not desc.strip():
        r.error(f"{ctx}: отсутствует непустой description")
    if stype == "smapi_command" and not step.get("command"):
        r.error(f"{ctx}: smapi_command требует command")
    if stype == "stardewmcp" and not step.get("tool"):
        r.error(f"{ctx}: stardewmcp требует tool")


def validate_expected(expected: object, ctx: str, r: Reporter) -> None:
    if not isinstance(expected, dict):
        r.error(f"{ctx}: expected должен быть объектом")
        return
    allowed_keys = {
        "buffsPresent",
        "buffsAbsent",
        "topicsPresent",
        "topicsAbsent",
        "state",
        "logsContain",
        "logsNotContain",
        "hudContains",
        "location",
        "notes",
    }
    for key in expected:
        if key not in allowed_keys:
            r.error(f"{ctx}: неизвестное поле expected.{key}")


def validate_test(test: object, suite_path: Path, suite_id: str, r: Reporter) -> str | None:
    """Возвращает test id при успехе, иначе None."""
    ctx_base = f"{suite_path.name}"
    if not isinstance(test, dict):
        r.error(f"{ctx_base}: элемент tests[] должен быть объектом")
        return None

    tid = test.get("id")
    ctx = f"{ctx_base} → test {tid!r}"

    for field in TEST_REQUIRED:
        if field not in test:
            r.error(f"{ctx}: отсутствует обязательное поле '{field}'")

    if not isinstance(tid, str) or not TEST_ID_PATTERN.match(tid):
        r.error(f"{ctx_base}: id должен соответствовать ^HOI-[A-Z0-9-]+$ (получено {tid!r})")
        return None

    title = test.get("title")
    if not isinstance(title, str) or not title.strip():
        r.error(f"{ctx}: title должен быть непустой строкой")

    priority = test.get("priority")
    if priority not in PRIORITIES:
        r.error(f"{ctx}: priority должен быть P0–P3 (получено {priority!r})")

    steps = test.get("steps")
    if not isinstance(steps, list) or len(steps) == 0:
        r.error(f"{ctx}: steps должен быть непустым массивом")
    else:
        for i, step in enumerate(steps):
            validate_step(step, f"{ctx} → steps[{i}]", r, STEP_TYPES)

    expected = test.get("expected")
    validate_expected(expected, f"{ctx} → expected", r)

    cleanup = test.get("cleanup")
    if not isinstance(cleanup, list) or len(cleanup) == 0:
        r.error(f"{ctx}: cleanup должен быть непустым массивом")
    else:
        for i, step in enumerate(cleanup):
            validate_step(step, f"{ctx} → cleanup[{i}]", r, CLEANUP_TYPES)
        if not is_smoke_test(test, suite_id) and not cleanup_has_injury_reset(cleanup):
            r.error(
                f"{ctx}: cleanup должен содержать smapi_command injury_reset "
                "(исключение: smoke-тесты)"
            )

    return tid


def validate_suite(data: object, path: Path, r: Reporter) -> list[str]:
    """Возвращает список test id из файла."""
    ids: list[str] = []
    if not isinstance(data, dict):
        r.error(f"{path.name}: корень должен быть объектом (test suite)")
        return ids

    for field in SUITE_REQUIRED:
        if field not in data:
            r.error(f"{path.name}: отсутствует поле suite '{field}'")

    suite_id = data.get("suiteId")
    if not isinstance(suite_id, str) or not SUITE_ID_PATTERN.match(suite_id):
        r.error(f"{path.name}: suiteId должен быть kebab-case (получено {suite_id!r})")
    elif path.stem != suite_id:
        r.error(f"{path.name}: suiteId={suite_id!r} не совпадает с именем файла {path.stem}")

    requires = data.get("requires")
    if isinstance(requires, dict):
        for field in REQUIRES_REQUIRED:
            if field not in requires:
                r.error(f"{path.name}: requires.{field} обязателен")
        dc = requires.get("debugCommands")
        if not isinstance(dc, list):
            r.error(f"{path.name}: requires.debugCommands должен быть массивом")
    else:
        r.error(f"{path.name}: requires должен быть объектом")

    tests = data.get("tests")
    if not isinstance(tests, list) or len(tests) == 0:
        r.error(f"{path.name}: tests должен быть непустым массивом")
        return ids

    sid = suite_id if isinstance(suite_id, str) else path.stem
    for test in tests:
        tid = validate_test(test, path, sid, r)
        if tid:
            ids.append(tid)

    return ids


def validate_index(r: Reporter) -> dict[str, Path]:
    """Проверяет index.json, возвращает map file → path."""
    index_path = SCENARIOS_DIR / "index.json"
    data = load_json(index_path, r)
    referenced: dict[str, Path] = {}

    if not isinstance(data, dict):
        if data is not None:
            r.error("index.json: корень должен быть объектом")
        return referenced

    suites = data.get("suites")
    if not isinstance(suites, list) or len(suites) == 0:
        r.error("index.json: suites должен быть непустым массивом")
        return referenced

    seen_suite_ids: set[str] = set()
    for i, entry in enumerate(suites):
        ctx = f"index.json → suites[{i}]"
        if not isinstance(entry, dict):
            r.error(f"{ctx}: запись должна быть объектом")
            continue
        for field in INDEX_SUITE_REQUIRED:
            if field not in entry:
                r.error(f"{ctx}: отсутствует поле '{field}'")

        suite_id = entry.get("suiteId")
        file_name = entry.get("file")
        test_count = entry.get("testCount")

        if isinstance(suite_id, str):
            if suite_id in seen_suite_ids:
                r.error(f"{ctx}: дублирующийся suiteId={suite_id!r}")
            seen_suite_ids.add(suite_id)

        if not isinstance(file_name, str) or not file_name.endswith(".json"):
            r.error(f"{ctx}: file должен быть именем .json файла")
            continue

        suite_path = SCENARIOS_DIR / file_name
        if not suite_path.is_file():
            r.error(f"{ctx}: файл не найден — {file_name}")
        else:
            referenced[file_name] = suite_path

        if isinstance(test_count, int) and suite_path.is_file():
            suite_data = load_json(suite_path, r)
            if isinstance(suite_data, dict):
                tests = suite_data.get("tests")
                actual = len(tests) if isinstance(tests, list) else 0
                if actual != test_count:
                    r.error(
                        f"{ctx}: testCount={test_count}, фактически тестов в {file_name}: {actual}"
                    )

    return referenced


def _configure_stdout() -> None:
    """Корректный UTF-8 в консоли Windows (cp1251 ломает кириллицу в отчёте)."""
    if hasattr(sys.stdout, "reconfigure"):
        try:
            sys.stdout.reconfigure(encoding="utf-8")
        except (OSError, ValueError):
            pass


def main() -> int:
    _configure_stdout()
    r = Reporter()

    if not SCENARIOS_DIR.is_dir():
        r.error(f"Каталог сценариев не найден: {SCENARIOS_DIR}")
        _print_report(r)
        return 1

    # Все .json должны парситься
    all_json = sorted(SCENARIOS_DIR.glob("*.json"))
    if not all_json:
        r.error(f"Нет JSON-файлов в {SCENARIOS_DIR}")

    schema_path = SCENARIOS_DIR / "schema.harvey-injury-test.schema.json"
    for path in all_json:
        load_json(path, r)

    # index + suite-файлы
    indexed = validate_index(r)

    all_test_ids: dict[str, list[str]] = {}
    suite_files = sorted(
        p for p in all_json if p.name not in ("index.json", "schema.harvey-injury-test.schema.json")
    )

    for path in suite_files:
        data = load_json(path, r)
        if data is None:
            continue
        ids = validate_suite(data, path, r)
        for tid in ids:
            all_test_ids.setdefault(tid, []).append(path.name)

    # Suite-файлы должны быть в index (кроме index/schema)
    indexed_names = set(indexed.keys())
    for path in suite_files:
        if path.name not in indexed_names:
            r.warn(f"{path.name}: файл suite не указан в index.json")

    for file_name in indexed_names:
        if file_name not in {p.name for p in suite_files}:
            r.error(f"index.json ссылается на {file_name}, но файл отсутствует или не suite")

    # Уникальность test id
    for tid, files in sorted(all_test_ids.items()):
        if len(files) > 1:
            r.error(f"Дублирующийся test id {tid} в файлах: {', '.join(files)}")

    _print_report(r, len(suite_files), len(all_test_ids))

    return 0 if r.ok else 1


def _print_report(r: Reporter, suite_count: int = 0, test_count: int = 0) -> None:
    print("=" * 60)
    print("Harvey Overhaul Injury — валидация JSON-сценариев")
    print(f"Каталог: {SCENARIOS_DIR}")
    print("=" * 60)

    if suite_count:
        print(f"\nПроверено suite-файлов: {suite_count}")
        print(f"Уникальных test id: {test_count}")

    if r.warnings:
        print(f"\nПредупреждения ({len(r.warnings)}):")
        for w in r.warnings:
            print(f"  [WARN] {w}")

    if r.errors:
        print(f"\nОшибки ({len(r.errors)}):")
        for e in r.errors:
            print(f"  [FAIL] {e}")
        print("\nИтог: FAIL")
    else:
        print("\nИтог: OK — все проверки пройдены")


if __name__ == "__main__":
    sys.exit(main())
