# Low-pass

Optional EMA smoothing on continuous (trackpad) deltas before [Sensitivity](./sensitivity.md) / gain is applied. Per-op enable + alpha. Persist fields use `*FilterEnabled` / `*FilterAlpha` (schema ≤2: `*LowPass*`). Options may still say **Low-pass**.

On the product surface, low-pass appears only when `EnableContactsCapture` is on (Contacts interpreter). AppleKit / Maps+ ship without low-pass UI or filtering.
