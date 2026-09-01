namespace TrackpadCameraControl
{
    /// <summary>
    /// Runtime flags for Harmony scroll/mouse patches. Policy lives in <see cref="InputGates"/>.
    /// </summary>
    public static class VanillaCameraSuppress
    {
        public static bool Enabled { get; set; }

        /// <summary>
        /// Set from AppKit scroll events: true when hasPreciseScrollingDeltas (trackpad).
        /// </summary>
        public static bool PreciseTrackpadScroll { get; set; }

        /// <summary>
        /// Set each pipeline tick from <see cref="InputGates.SyncFrameState"/>.
        /// </summary>
        public static bool MenuOrOverUi { get; set; }
    }
}
