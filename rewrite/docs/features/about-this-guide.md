# About this guide

Target **feature** contracts for the `rewrite/` clean-architecture tree. Shipping as-built docs live under repo-root `docs/features/`.

This guide describes the architecture **to implement** so Maps+ UI and gesture dynamics stay at [UI parity](../glossary/ui-parity.md) with the shipping mod while internals stay greenfield-simple (lessons L1–L13). Do not implement that parity by copying shipping C#.

Start with [greenfield redesign lessons](./greenfield-redesign-lessons.md) (L1–L13), then [parity with shipping](./parity-with-shipping.md) for the player-facing definition of done. Planes and tick live in [system architecture](./system-architecture.md); style-table resolve is [ADR 0004](./adr/0004-style-table-driven-resolve.md); source independence is [ADR 0005](./adr/0005-ux-parity-not-source-parity.md).
