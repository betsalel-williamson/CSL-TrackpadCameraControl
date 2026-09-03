# Low-pass

Optional EMA smoothing on continuous (trackpad) deltas before [Sensitivity](./sensitivity.md) / gain is applied. Per-op enable + alpha. Persist fields use `*FilterEnabled` / `*FilterAlpha` (schema ≤2: `*LowPass*`). Options may still say **Low-pass**.

**v1:** AppKit / Maps+ ships **without** low-pass UI or filtering. Low-pass was designed for a Contacts interpreter that remains **unfinished** (`EnableContactsCapture`). Treat LP as **future** until Contacts is validated.
