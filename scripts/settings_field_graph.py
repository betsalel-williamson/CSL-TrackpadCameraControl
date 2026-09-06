#!/usr/bin/env -S python3 -u
"""Settings field → tick consumer graph (rewrite Phase 3).

Parses public auto-properties on ModSettings and fails when a live field is never
*read* outside the Settings / UI persist layer (greenfield L1 / L12).

Exclusions (documented):
  chrome          Options/Debug panel chrome — not camera math
  seed_identity   GesturePreset style identity (Options switcher); tick reads StyleTable
  xml_alias       [XmlElement(...)] deserialize-only aliases (ShouldSerialize* = false)
  xml_ignore      [XmlIgnore] runtime-only (still must have a reader if live)
  schema_non_field
                  Schema forbids these on the live blob; if still present they FAIL
                  unless listed in ALLOW_SCHEMA_NON_FIELD (warn-only escape hatch)

Heuristic: a read is `.FieldName` not followed by `=`. Writes alone do not count.
Scan roots: rewrite/mod/{Policy,Apply,Host} (and any other non-Feel/Ui).
"""

from __future__ import annotations

import argparse
import os
import re
import sys
from collections import defaultdict
from pathlib import Path

REPO_HINTS = ("rewrite", "package.json", "TrackpadCameraControl.sln")

CHROME = frozenset(
    {
        "AssistUiEnabled",
        "ActiveFeelPresetName",
        "IncludeSystemInfoInCopy",
        "DebugPanelDismissed",
        "DebugPanelPosX",
        "DebugPanelPosY",
        "DebugOverlay",
    }
)

# Style identity for Options / ApplyGesturePreset; resolve consumes StyleTable (L1).
SEED_IDENTITY = frozenset({"GesturePreset"})

# Schema non-fields that must not appear as live ceremony without a tick reader.
# If still on ModSettings with no outside reader → FAIL (correct gate signal).
SCHEMA_NON_FIELD = frozenset(
    {
        "BridgeEnabled",  # out-of-process bridge not on ship path
    }
)

EXCLUDE_DIR_NAMES = frozenset({"Settings", "Feel", "Ui", "obj", "bin"})


def find_repo_root(start: Path) -> Path:
    cur = start.resolve()
    for _ in range(8):
        if (cur / "package.json").exists() and (cur / "rewrite").is_dir():
            return cur
        if cur.parent == cur:
            break
        cur = cur.parent
    return start.resolve()


def parse_mod_settings_fields(source: str) -> list[dict]:
    """Return dicts: name, xml_ignore, xml_alias."""
    lines = source.splitlines()
    fields: list[dict] = []
    i = 0
    while i < len(lines):
        attrs: list[str] = []
        while i < len(lines) and lines[i].strip().startswith("["):
            buf = lines[i].strip()
            i += 1
            while i < len(lines) and buf.count("[") > buf.count("]"):
                buf += lines[i].strip()
                i += 1
            attrs.append(buf)
        if i >= len(lines):
            break
        m = re.match(
            r"\s*public\s+([\w.<>,\[\]]+)\s+(\w+)\s*\{\s*get;\s*set;",
            lines[i],
        )
        if m:
            fields.append(
                {
                    "name": m.group(2),
                    "type": m.group(1),
                    "xml_ignore": any("XmlIgnore" in a for a in attrs),
                    "xml_alias": any("XmlElement" in a for a in attrs),
                    "attrs": attrs,
                }
            )
        i += 1
    return fields


def collect_reads_writes(
    mod_root: Path, field_names: list[str]
) -> tuple[dict[str, set[str]], dict[str, set[str]]]:
    reads: dict[str, set[str]] = defaultdict(set)
    writes: dict[str, set[str]] = defaultdict(set)
    for path in mod_root.rglob("*.cs"):
        rel_parts = path.relative_to(mod_root).parts
        if not rel_parts:
            continue
        if rel_parts[0] in EXCLUDE_DIR_NAMES or "obj" in rel_parts or "bin" in rel_parts:
            continue
        text = path.read_text(encoding="utf-8", errors="replace")
        rel = str(path.relative_to(mod_root)).replace("\\", "/")
        for name in field_names:
            for m in re.finditer(rf"\.{re.escape(name)}\b", text):
                after = text[m.end() : m.end() + 12]
                if re.match(r"\s*=(?!=)", after):
                    writes[name].add(rel)
                else:
                    reads[name].add(rel)
    return reads, writes


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--repo-root",
        type=Path,
        default=None,
        help="Repository root (default: discover from this script)",
    )
    parser.add_argument(
        "--allow-schema-non-field",
        action="store_true",
        help="Warn on SCHEMA_NON_FIELD misses instead of failing (escape hatch)",
    )
    parser.add_argument("-q", "--quiet", action="store_true")
    args = parser.parse_args()

    script_dir = Path(__file__).resolve().parent
    repo = args.repo_root or find_repo_root(script_dir)
    settings_path = repo / "rewrite" / "mod" / "Feel" / "ModSettings.cs"
    if not settings_path.is_file():
        settings_path = repo / "rewrite" / "mod" / "Settings" / "ModSettings.cs"
    mod_root = repo / "rewrite" / "mod"

    if not settings_path.is_file():
        print(f"error: ModSettings not found at {settings_path}", file=sys.stderr)
        return 2

    fields = parse_mod_settings_fields(
        settings_path.read_text(encoding="utf-8", errors="replace")
    )
    names = [f["name"] for f in fields]
    reads, writes = collect_reads_writes(mod_root, names)

    failures: list[str] = []
    warnings: list[str] = []
    ok_count = 0
    skipped = 0

    def out(msg: str = "") -> None:
        print(msg, flush=True)

    def err(msg: str) -> None:
        print(msg, file=sys.stderr, flush=True)

    if not args.quiet:
        out(f"settings-field-graph: scanning {mod_root}")
        out(
            f"  fields={len(fields)} chrome={len(CHROME)} "
            f"seed_identity={len(SEED_IDENTITY)} schema_non_field={len(SCHEMA_NON_FIELD)}"
        )

    for f in fields:
        name = f["name"]
        if f["xml_alias"]:
            skipped += 1
            if not args.quiet:
                out(f"  SKIP alias     {name}")
            continue
        if name in CHROME or name in SEED_IDENTITY:
            skipped += 1
            kind = "chrome" if name in CHROME else "seed_identity"
            if not args.quiet:
                out(f"  SKIP {kind:12} {name}")
            continue

        readers = sorted(reads.get(name, ()))
        writers = sorted(writes.get(name, ()))

        if readers:
            ok_count += 1
            if not args.quiet:
                out(f"  OK   {name} <- {', '.join(readers)}")
            continue

        msg = f"{name}: no tick-path reader outside Settings/Ui"
        if writers:
            msg += f" (writes only: {', '.join(writers)})"
        if f["xml_ignore"]:
            msg += " [XmlIgnore]"

        if name in SCHEMA_NON_FIELD:
            msg += " [schema non-field — should be removed from live blob]"
            if args.allow_schema_non_field:
                warnings.append(msg)
                if not args.quiet:
                    out(f"  WARN {msg}")
                continue

        failures.append(msg)
        if not args.quiet:
            out(f"  FAIL {msg}")

    if warnings and not args.quiet:
        out(f"warnings: {len(warnings)}")
    if failures:
        err(
            f"settings-field-graph: FAIL ({len(failures)} field(s) without consumer; "
            f"ok={ok_count} skipped={skipped})"
        )
        for m in failures:
            err(f"  - {m}")
        return 1

    out(
        f"settings-field-graph: PASS (ok={ok_count} skipped={skipped} warnings={len(warnings)})"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
