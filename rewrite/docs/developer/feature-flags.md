# Feature flags

Maintainer contract for unfinished product surfaces. v1 ship DLL compiles **AppKit capture + Maps+ only** — IPC, Contacts, and CAD modules were removed from the rewrite tree (not compile-gated stubs).

## Flags (rewrite v1)

| Property / doc name  | Compile symbol         | When off (ship)                               | When compiled on                                     |
| -------------------- | ---------------------- | --------------------------------------------- | ---------------------------------------------------- |
| `EnableAssistChrome` | `ENABLE_ASSIST_CHROME` | No Assist pads/buttons; no button-step fields | Future — chrome nudges through the shared apply path |

CAD gesture style and Contacts capture are **v2 / docs-only** — see [release process](./release-process.md) and [organized product feedback](./review/v1-product-feedback.md).

## Rules

- Call sites use `#if ENABLE_ASSIST_CHROME` only when Assist is compiled on.
- Off modules leave **no** stub UI, no-op filters, or dead tick subscribers.
- Default ship and rewrite local install leave `EnableAssistChrome` **false**.

## How to enable (local experiments)

```bash
dotnet build rewrite/mod/TrackpadCameraControl.Rewrite.csproj -p:EnableAssistChrome=true
```

Assist chrome is not a ship QA path until a dedicated validation pass.
