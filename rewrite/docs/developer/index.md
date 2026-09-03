# Developer guide (rewrite)

Contributor contracts for the `rewrite/` target tree — layout, state, settings, flags, harnesses, quality gates, install, and parity QA.

North star: greenfield redesign lessons (L1–L13) in the features guide. Start with features _Under the hood_. Glossary _UI parity_ is player-facing; ADR 0005 forbids cloning shipping sources; ADR 0006 separates gesture library from mod surface.

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

## System architecture review

Procedure: project skill `system-architecture-review`. Latest pass:

- [About the review guide](./review/about-this-guide.md)
- [Under-the-hood synthesis (Conditional)](./review/uth-synthesis.md)

## Clone experiment (closed — historical)

- [Organized product feedback](./review/v1-product-feedback.md)
- [v1 audit and cleanup plan](./review/v1-audit-plan.md) (closed)

### Specialist audit shards (clone-era — historical)

- [Architecture audit](./review/architecture-audit.md)
- [Capture layer audit](./review/capture-audit.md)
- [Settings / schema audit](./review/settings-audit.md)
- [UI / product surface](./review/ui-audit.md)
- [Release / versioning](./review/release-audit.md)
- [Tests / static analysis](./review/tests-sa-audit.md)
