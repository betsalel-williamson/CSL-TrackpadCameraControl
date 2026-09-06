#!/usr/bin/env -S python3 -u
"""Native leak pairing scan for the rewrite tree (Phase 3).

Ports the approach of tests/TrackpadCameraControl.Tests/NativeResourceLeakAnalyzer.cs:
per-file acquire/release counts for GCHandle, CoreFoundation, Multitouch device, AppKit monitors.

Scan roots (rewrite + linked capture sources):
  rewrite/mod
  src/TrackpadCapture   (linked when EnableContactsCapture=true)
  src/TrackpadBridge
  src/AppleGestureProbe

Marker: a line containing `native-leak-ok:` skips that acquire (process-lifetime /
ownership transfer). Do not use it to silence a real leak.

This is pairing analysis, not a runtime leak detector (L10).
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

SCAN_ROOTS = (
    "rewrite/mod",
    "src/TrackpadCapture",
    "src/TrackpadBridge",
    "src/AppleGestureProbe",
)

GCH_FIELD_RE = re.compile(r"\bGCHandle\s+_?[A-Za-z]\w*\s*;")
FREE_RE = re.compile(r"\.Free\s*\(\s*\)")
CREATE_CF_STRING_DECL_RE = re.compile(r"IntPtr\s+CreateCfString\s*\(")


def find_repo_root(start: Path) -> Path:
    cur = start.resolve()
    for _ in range(8):
        if (cur / "package.json").exists() and (cur / "rewrite").is_dir():
            return cur
        if cur.parent == cur:
            break
        cur = cur.parent
    return start.resolve()


def strip_line_comment(raw: str) -> str:
    i = 0
    in_string = False
    while i < len(raw) - 1:
        c = raw[i]
        if c == '"' and (i == 0 or raw[i - 1] != "\\"):
            in_string = not in_string
        elif not in_string and c == "/" and raw[i + 1] == "/":
            return raw[:i]
        i += 1
    return raw


def is_cf_string_create(line: str) -> bool:
    if "CFStringCreateWithCString(" in line:
        return True
    if CREATE_CF_STRING_DECL_RE.search(line):
        return False
    if "CreateCfString(" in line:
        return True
    return False


def analyze_source(path: str, source: str) -> list[str]:
    findings: list[str] = []
    if source is None:
        return findings

    gch_alloc = gch_free = 0
    cf_create = cf_release = 0
    device_start = device_stop = 0
    add_monitor = remove_monitor = 0
    gchandle_field = False
    idisposable = False

    for raw in source.replace("\r\n", "\n").split("\n"):
        leak_ok = "native-leak-ok:" in raw
        line = strip_line_comment(raw)
        if "extern " in line or "delegate " in line:
            continue

        if GCH_FIELD_RE.search(line):
            gchandle_field = True
        if "IDisposable" in line:
            idisposable = True
        if "GCHandle.Alloc" in line and not leak_ok:
            gch_alloc += 1
        if FREE_RE.search(line):
            gch_free += 1
        if is_cf_string_create(line) and not leak_ok:
            cf_create += 1
        if "CFRelease(" in line:
            cf_release += 1
        if ".DeviceStart(" in line and not leak_ok:
            device_start += 1
        if ".DeviceStop(" in line:
            device_stop += 1
        if "addLocalMonitorForEventsMatchingMask" in line and not leak_ok:
            add_monitor += 1
        if "removeMonitor:" in line:
            remove_monitor += 1

    if gch_alloc > gch_free:
        findings.append(
            f"{path}: GCHandle.Alloc ({gch_alloc}) exceeds Free() ({gch_free})"
        )
    if gchandle_field and not idisposable:
        findings.append(f"{path}: GCHandle field requires IDisposable")
    if cf_create > cf_release:
        findings.append(
            f"{path}: CFStringCreateWithCString/CreateCfString ({cf_create}) "
            f"exceeds CFRelease ({cf_release})"
        )
    if device_start > device_stop:
        findings.append(
            f"{path}: DeviceStart ({device_start}) exceeds DeviceStop ({device_stop})"
        )
    if add_monitor > remove_monitor:
        findings.append(
            f"{path}: addLocalMonitor ({add_monitor}) exceeds removeMonitor ({remove_monitor})"
        )
    return findings


def analyze_tree(repo_root: Path) -> list[str]:
    findings: list[str] = []
    for relative in SCAN_ROOTS:
        directory = repo_root / relative
        if not directory.is_dir():
            continue
        for path in directory.rglob("*.cs"):
            parts = path.parts
            if "obj" in parts or "bin" in parts:
                continue
            rel = str(path.relative_to(repo_root)).replace("\\", "/")
            findings.extend(
                analyze_source(rel, path.read_text(encoding="utf-8", errors="replace"))
            )
    return findings


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", type=Path, default=None)
    parser.add_argument("-q", "--quiet", action="store_true")
    args = parser.parse_args()

    repo = args.repo_root or find_repo_root(Path(__file__).resolve().parent)
    if not args.quiet:
        print("native-leak-pairing: roots:")
        for r in SCAN_ROOTS:
            p = repo / r
            print(f"  {'OK' if p.is_dir() else 'missing':7} {r}")

    findings = analyze_tree(repo)
    if findings:
        print(f"native-leak-pairing: FAIL ({len(findings)} finding(s))", file=sys.stderr)
        for f in findings:
            print(f"  - {f}", file=sys.stderr)
        return 1

    print("native-leak-pairing: PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())
