#!/usr/bin/env -S python3 -u
"""Layer-import lint for rewrite stack boundaries (ADR 0006 / under-the-hood).

Fails when:
  - rewrite/src references ICities, Colossal, HarmonyLib, or CSL Feel/Maps+ product types
  - rewrite/mod contains AppKit / Multitouch P/Invoke (DllImport of AppKit paths)
  - a pure Policy / Apply / Feel file gains UnityEngine / ICities / Harmony / AppKit usings
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

LIBRARY_FORBIDDEN = re.compile(
    r"\b(ICities|ColossalFramework|HarmonyLib|CitiesHarmony|FeelCatalog|FeelEditor|"
    r"FeelProfiles|MapsPlusSeed|StyleBindingResolver|CameraApplicator|FeelMath|"
    r"ModSettings|OptionsHost|DebugHost)\b"
)

MOD_APPKIT = re.compile(
    r'DllImport\s*\(\s*["\'].*(?:AppKit|Multitouch|libobjc)',
    re.IGNORECASE,
)

PURE_DIR_FORBIDDEN_USING = re.compile(
    r"^\s*using\s+(UnityEngine|ICities|HarmonyLib|CitiesHarmony|ColossalFramework)\b",
    re.MULTILINE,
)

PURE_DIR_APPKIT = re.compile(r"\b(DllImport|libobjc|/System/Library/Frameworks/AppKit)\b")


def find_repo_root(start: Path) -> Path:
    cur = start.resolve()
    for _ in range(8):
        if (cur / "package.json").exists() and (cur / "rewrite").is_dir():
            return cur
        if cur.parent == cur:
            break
        cur = cur.parent
    return start.resolve()


def iter_cs(root: Path):
    if not root.is_dir():
        return
    for path in root.rglob("*.cs"):
        parts = path.parts
        if "obj" in parts or "bin" in parts:
            continue
        yield path


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", type=Path, default=None)
    parser.add_argument("-q", "--quiet", action="store_true")
    args = parser.parse_args()

    repo = args.repo_root or find_repo_root(Path(__file__).resolve().parent)
    failures: list[str] = []

    lib_root = repo / "rewrite" / "src"
    for path in iter_cs(lib_root):
        text = path.read_text(encoding="utf-8", errors="replace")
        for m in LIBRARY_FORBIDDEN.finditer(text):
            failures.append(f"{path.relative_to(repo)}: library must not reference {m.group(1)}")

    mod_root = repo / "rewrite" / "mod"
    for path in iter_cs(mod_root):
        text = path.read_text(encoding="utf-8", errors="replace")
        rel = path.relative_to(repo)
        if MOD_APPKIT.search(text) or (
            "DllImport" in text and ("AppKit" in text or "libobjc" in text)
        ):
            failures.append(f"{rel}: mod must not contain AppKit/Multitouch P/Invoke")

        top = path.relative_to(mod_root).parts[0] if path.relative_to(mod_root).parts else ""
        if top in ("Policy", "Apply", "Feel"):
            # Cities adapters are allowed under Apply/ when named *Adapter, Cities*, or Game*
            name = path.name
            is_adapter = "Adapter" in name or name.startswith("Cities") or name.startswith("Game")
            if top == "Apply" and is_adapter:
                continue
            if PURE_DIR_FORBIDDEN_USING.search(text) or PURE_DIR_APPKIT.search(text):
                # Allow HAS_CITIES blocks in Game* / Cities* already skipped; flag pure files
                if "using UnityEngine" in text or "using ICities" in text or "using HarmonyLib" in text:
                    failures.append(
                        f"{rel}: pure {top} file must not import Unity/ICities/Harmony/AppKit"
                    )
                elif PURE_DIR_APPKIT.search(text) and "DllImport" in text:
                    failures.append(f"{rel}: pure {top} file must not use AppKit P/Invoke")

    if not args.quiet:
        print(f"layer-import-lint: scanned {lib_root.relative_to(repo)} and {mod_root.relative_to(repo)}")

    if failures:
        print(f"layer-import-lint: FAIL ({len(failures)})", file=sys.stderr)
        for f in failures:
            print(f"  - {f}", file=sys.stderr)
        return 1

    print("layer-import-lint: PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())
