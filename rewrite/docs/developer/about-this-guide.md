# About this guide

**Audience:** Contributors and agents implementing the `rewrite/` clean-architecture tree.

**Lookup:** Start with [repository layout](./repository-layout.md) (deploy `TrackpadCameraControl.Rewrite`). Settings and flags: [settings schema](./settings-schema.md), [feature flags](./feature-flags.md). Prove work with [harnesses and testing](./harnesses-and-testing.md), [static analysis and quality](./static-analysis-and-quality.md), then [local MVP install](./local-mvp-install.md) `--rewrite` and the [QA checklist](./qa-checklist.md) parity matrix.

Target feature contracts live under `rewrite/docs/features/` (greenfield redesign lessons L1–L13, [ADR 0005](../features/adr/0005-ux-parity-not-source-parity.md)). Shipping as-built docs remain under repo-root `docs/` until cutover — do not mix MDCP links across those roots. `rewrite/mod` C# from the clone experiment is quarantined until a UX-contract rebuild is approved.
