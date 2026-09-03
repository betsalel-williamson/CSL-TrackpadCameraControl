# Gesture library

The **Unity-reusable** middle stack layer: shared primitive/frame contract, backend interface, and OS→frame mappers. Lives under `rewrite/src`. Any Unity title could consume it without bringing Cities: Skylines Options, Maps+ seeds, or Harmony.

It does **not** decide pan/zoom/orbit, own feel UI, or reference `ICities` / Colossal. Native OS backends plug in here only. Contrast with the [mod surface](./mod-surface.md) and the [capture plane](./capture-plane.md) (tick role inside the CSL mod). Full stack: features guide _Under the hood_ and ADR 0006.
