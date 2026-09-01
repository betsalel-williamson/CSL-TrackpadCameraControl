namespace TrackpadCameraControl
{
    public static class VanillaCameraSuppress
    {
        public static bool Enabled { get; set; }

        /// <summary>
        /// Set from AppKit scroll events: true when hasPreciseScrollingDeltas (trackpad).
        /// Cleared on non-precise (mouse wheel) scroll.
        /// </summary>
        public static bool PreciseTrackpadScroll { get; set; }

        /// <summary>
        /// Set each pipeline tick from InputGates: menu/Options open or pointer over UI.
        /// When true, do not suppress vanilla scroll (UI needs it).
        /// </summary>
        public static bool MenuOrOverUi { get; set; }

        /// <summary>Harmony: run vanilla scroll handler (false = block input).</summary>
        public static bool ShouldRunVanillaScrollWheel()
        {
            if (InputGates.ShouldBlockCameraInput())
            {
                return false;
            }

            return !ShouldSkipScrollWheel(PreciseTrackpadScroll, MenuOrOverUi);
        }

        public static bool ShouldSkipScrollWheel()
        {
            return ShouldSkipScrollWheel(PreciseTrackpadScroll, MenuOrOverUi);
        }

        /// <summary>
        /// Suppress vanilla scroll-zoom only for precise trackpad while the world path is active.
        /// Unfocused blocking is handled by <see cref="InputGates.ShouldBlockCameraInput"/>.
        /// </summary>
        public static bool ShouldSkipScrollWheel(bool preciseTrackpad, bool menuOrOverUi)
        {
            return Enabled && preciseTrackpad && !menuOrOverUi;
        }

        /// <summary>Harmony: run vanilla mouse camera handler (false = block edge pan / rotate).</summary>
        public static bool ShouldRunVanillaMouseEvents(bool rotateBindingHeld)
        {
            if (InputGates.ShouldBlockCameraInput())
            {
                return false;
            }

            return !ShouldSkipMouseHandler(rotateBindingHeld);
        }

        public static bool ShouldSkipMouseHandler(bool rotateBindingHeld)
        {
            return Enabled && rotateBindingHeld;
        }

        /// <summary>Harmony orbit flush runs only while the mod world camera path is armed.</summary>
        public static bool ShouldFlushPendingOrbit()
        {
            return InputGates.IsModCameraArmed();
        }
    }
}
