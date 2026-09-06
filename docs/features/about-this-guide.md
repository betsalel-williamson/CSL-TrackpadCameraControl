# About this guide

**Feature** contracts for the primary ship tree (`mod/` + gesture library under `src/`).

This guide describes the architecture so Maps+ UI and gesture dynamics stay at [UI parity](../glossary/ui-parity.md) with the v1 player surface while internals stay greenfield-simple (lessons L1–L13). Do not implement that parity by copying the historical prototype under `bootstrap/`.

**Read order:** [Under the hood](./under-the-hood.md) (stack layers, import matrix, units) → [Feel catalog](./feel-catalog.md) (player surface inventory) → [greenfield redesign lessons](./greenfield-redesign-lessons.md) (L1–L13) → [parity with shipping](./parity-with-shipping.md). Tick contract: [system architecture](./system-architecture.md). Style-table resolve: [ADR 0004](./adr/0004-style-table-driven-resolve.md). Source independence: [ADR 0005](./adr/0005-ux-parity-not-source-parity.md). Library vs mod: [ADR 0006](./adr/0006-gesture-library-vs-mod-surface.md).
