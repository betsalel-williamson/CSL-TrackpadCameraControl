# QA Clipboard Report Trim Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Trim Debug Copy system info to OS + Model, keep input models, add dependency-critical assembly versions.

**Architecture:** `QaClipboardReport.Format` stays the orchestrator. `MacQaSystemInfo` drops CPU/Memory. New `QaAssemblyVersions` (or methods on `QaClipboardReport`) appends a whitelist of loaded managed assemblies + Unity runtime version under `HAS_CITIES`.

**Tech Stack:** C# net35 mod, xUnit tests (EnableCitiesRefs=false), existing Debug Copy checkbox.

**Spec:** `docs/superpowers/specs/2026-09-01-qa-clipboard-report-design.md`

## Global Constraints

- No CPU/Memory in clipboard
- No Apple framework versions/paths/Multitouch
- No serial numbers; duplicates as ×N
- Assemblies whitelist only: Unity, UnityEngine, Assembly-CSharp, ICities, CitiesHarmony.API, 0Harmony, TrackpadCameraControl
- Fail soft → `missing`, never throw
- Checkbox off → footer + Mod only

---

### Task 1: Trim System section (drop CPU/Memory)

**Files:**

- Modify: `mod/MacQaSystemInfo.cs` (`AppendHardware`)
- Test: `tests/TrackpadCameraControl.Tests/QaClipboardReportTests.cs`

**Interfaces:**

- Consumes: existing `MacQaSystemInfo.AppendSection`
- Produces: System block with only `OS:` and `Model:` lines

- [ ] **Step 1: Extend failing assertion**

In `Format_WithSystemInfo_IncludesSystemSections`, add:

```csharp
Assert.DoesNotContain("CPU:", text);
Assert.DoesNotContain("Memory:", text);
Assert.Contains("Model:", text);
```

- [ ] **Step 2: Run test — expect FAIL if CPU/Memory still present**

Run: `dotnet test tests/TrackpadCameraControl.Tests/TrackpadCameraControl.Tests.csproj --filter FullyQualifiedName~QaClipboard --nologo`

- [ ] **Step 3: Change `AppendHardware` to only write Model**

Remove CPU and Memory writes (both `#if HAS_CITIES` and sysctl fallbacks for machine/CPU). Keep Model via `SystemInfo.deviceModel` or `hw.model`.

- [ ] **Step 4: Re-run tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "fix: drop CPU and Memory from QA clipboard system block"
```

---

### Task 2: Add Assemblies section

**Files:**

- Create: `mod/QaAssemblyVersions.cs`
- Modify: `mod/QaClipboardReport.cs`
- Test: `tests/TrackpadCameraControl.Tests/QaClipboardReportTests.cs`

**Interfaces:**

- Produces: `internal static void AppendSection(StringBuilder sb)`
- Produces: `internal static string FormatAssemblyVersion(string simpleName)` → version string or `"missing"`
- Produces: `internal static string FormatUnityRuntimeDisplay()` → unity version or null outside Cities

- [ ] **Step 1: Write failing tests**

```csharp
[Fact]
public void Format_WithSystemInfo_IncludesAssembliesSection()
{
    string text = QaClipboardReport.Format(true);
    Assert.Contains("--- Assemblies ---", text);
    Assert.Contains("TrackpadCameraControl:", text);
    Assert.Contains("UnityEngine:", text);
}

[Fact]
public void FormatAssemblyVersion_ReturnsMissingForUnknown()
{
    Assert.Equal("missing", QaAssemblyVersions.FormatAssemblyVersion("DefinitelyNotLoaded.Assembly.XYZ"));
}
```

- [ ] **Step 2: Run — FAIL (type/section missing)**

- [ ] **Step 3: Implement `QaAssemblyVersions`**

```csharp
internal static class QaAssemblyVersions
{
    internal static void AppendSection(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("--- Assemblies ---");
        string unity = FormatUnityRuntimeDisplay();
        if (!string.IsNullOrEmpty(unity))
            sb.AppendLine("Unity: " + unity);
        AppendNamed(sb, "UnityEngine");
        AppendNamed(sb, "Assembly-CSharp");
        AppendNamed(sb, "ICities");
        AppendNamed(sb, "CitiesHarmony.API");
        AppendNamed(sb, "0Harmony");
        AppendNamed(sb, "TrackpadCameraControl");
    }

    internal static string FormatAssemblyVersion(string simpleName)
    {
        try
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(a.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase))
                {
                    Version v = a.GetName().Version;
                    return v != null ? v.ToString() : "missing";
                }
            }
        }
        catch { }
        return "missing";
    }

    internal static string FormatUnityRuntimeDisplay()
    {
#if HAS_CITIES
        try { return UnityEngine.Application.unityVersion; } catch { return null; }
#else
        return null;
#endif
    }
}
```

Wire from `QaClipboardReport.Format` after `MacQaSystemInfo.AppendSection(sb)`.

- [ ] **Step 4: Tests PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat: add assemblies section to QA clipboard report"
```

---

### Task 3: Docs + changeset

**Files:**

- Modify: `docs/features/debug-ui-camera-chrome.md`
- Modify: `docs/developer/qa-checklist.md`
- Modify: `docs/developer/mod-reload-during-development.md` (if it mentions CPU)
- Modify: `.changeset/debug-build-info-copy.md`

- [ ] **Step 1: Update docs to OS/Model + assemblies (not CPU/RAM)**

- [ ] **Step 2: `npm run docs:check`**

- [ ] **Step 3: Commit**

```bash
git commit -m "docs: align QA copy docs with OS model and assemblies"
```

---

## Spec coverage

| Spec item                      | Task                     |
| ------------------------------ | ------------------------ |
| Drop CPU/Memory                | 1                        |
| Assemblies whitelist           | 2                        |
| Unity Application.unityVersion | 2                        |
| Fail soft missing              | 2                        |
| Docs update                    | 3                        |
| Input ×N / no serials          | already done — no change |
