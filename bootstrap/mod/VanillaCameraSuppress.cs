namespace TrackpadCameraControl
{
    /// <summary>
    /// Per-frame Harmony/AppKit buffers — not preferences. Policy lives in <see cref="InputGates"/>.
    /// Mod on/off is <see cref="ModRuntime.IsActive"/>; do not add other static state here.
    /// </summary>
    public static class VanillaCameraSuppress
    {
        /// <summary>
        /// Written from AppKit scroll events: true when hasPreciseScrollingDeltas (trackpad).
        /// </summary>
        public static bool PreciseTrackpadScroll { get; set; }

        /// <summary>
        /// Written each pipeline tick from <see cref="InputGates.SyncFrameState"/> only.
        /// </summary>
        public static bool MenuOrOverUi { get; set; }
    }
}
