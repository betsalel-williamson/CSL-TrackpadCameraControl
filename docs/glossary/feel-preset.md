# Feel preset

Player-facing name for a **feel profile**: Sensitivity values and related feel fields (schema may also hold reverse / enables). Built-ins **Slow**, **Default**, and **Fast** are immutable. The feel **dropdown** loads on select; dirty edits move to **New Preset**; **Save as…** / **Delete** / **Reset to factory** follow the hot settings contract.

A feel preset is **not** [gesture style](./gesture-style.md). Changing Slow ↔ Default ↔ Fast never rewrites the [style binding table](./style-binding-table.md). Orbit pitch is an [apply constant](./apply-constant.md) (**0°–90°**), not a feel field.
