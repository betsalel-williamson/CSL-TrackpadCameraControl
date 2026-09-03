# Developer guide (rewrite)

Contributor contracts for the `rewrite/` target tree — layout, state, settings, flags, harnesses, quality gates, install, and parity QA.

North star: greenfield redesign lessons (L1–L13) in the features guide. [UI parity](../glossary/ui-parity.md) is player-facing; [ADR 0005](../features/adr/0005-ux-parity-not-source-parity.md) forbids cloning shipping sources.

- [Developer guide (rewrite)](#table-of-contents)
  - [About this guide](./about-this-guide.md)
  - [Repository layout](./repository-layout.md)
  - [State ownership](./state-ownership.md)
  - [Settings schema](./settings-schema.md)
  - [Feature flags](./feature-flags.md)
  - [Logging](./logging.md)
  - [Release process](./release-process.md)
  - [Harnesses and testing](./harnesses-and-testing.md)
  - [Static analysis and quality](./static-analysis-and-quality.md)
  - [Local MVP install](./local-mvp-install.md)
  - [QA checklist](./qa-checklist.md)

- [In-game parity checklist (tier C)](./in-game-parity-checklist.md)

## Clone experiment (closed)

The clone-and-strip pass is **closed**. Do not extend quarantined `rewrite/mod` sources as the v1 path. Findings remain as context only.

- [About the review guide](./review/about-this-guide.md)
- [Organized product feedback](./review/v1-product-feedback.md)
- [v1 audit and cleanup plan](./review/v1-audit-plan.md) (closed)

### Specialist audit shards (R1)

- [Architecture audit](./review/architecture-audit.md)
- [Capture layer audit](./review/capture-audit.md)
- [Settings / schema audit](./review/settings-audit.md)
- [UI / product surface](./review/ui-audit.md)
- [Release / versioning](./review/release-audit.md)
- [Tests / static analysis](./review/tests-sa-audit.md)
