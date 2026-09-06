# About this guide

Target **feature** contracts for the `rewrite/` clean-architecture tree. Shipping as-built docs live under repo-root `docs/features/`.

This guide describes the architecture **to implement** so Maps+ UI and gesture dynamics stay at [UI parity](../glossary/ui-parity.md) with the shipping mod while internals stay greenfield-simple (lessons L1–L13). Do not implement that parity by copying shipping C#.

**Read order:** [Under the hood](./under-the-hood.md) (stack layers, import matrix, units) → [Feel catalog](./feel-catalog.md) (player surface inventory) → [greenfield redesign lessons](./greenfield-redesign-lessons.md) (L1–L13) → [parity with shipping](./parity-with-shipping.md). Tick contract: [system architecture](./system-architecture.md). Style-table resolve: [ADR 0004](./adr/0004-style-table-driven-resolve.md). Source independence: [ADR 0005](./adr/0005-ux-parity-not-source-parity.md). Library vs mod: [ADR 0006](./adr/0006-gesture-library-vs-mod-surface.md).
