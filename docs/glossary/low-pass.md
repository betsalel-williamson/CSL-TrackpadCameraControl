# Low-pass

Optional per-op exponential smoothing (EMA) on **drag** deltas after gesture resolve and before the camera applicator. Player-facing: enable plus **alpha** (0–1). Default off.

Buttons skip low-pass. When enabled: first sample seeds the filter; later samples use `smoothed += alpha * (raw - smoothed)`. Filter state resets on touch-up (Ended / Cancelled / zero fingers).
