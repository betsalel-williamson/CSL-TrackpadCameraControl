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

        public static bool ShouldSkipScrollWheel()
        {
            return ShouldSkipScrollWheel(PreciseTrackpadScroll, MenuOrOverUi);
        }

        /// <summary>
        /// Suppress vanilla scroll-zoom only for precise trackpad while the world camera path is active.
        /// Mouse wheel and scroll over menus/UI are allowed.
        /// </summary>
        public static bool ShouldSkipScrollWheel(
            bool preciseTrackpad,
            bool menuOrOverUi,
            bool gameFocused = true
        )
        {
            return Enabled && preciseTrackpad && !menuOrOverUi;
        }

        public static bool ShouldSkipMouseHandler(bool rotateBindingHeld)
        {
            return Enabled && rotateBindingHeld;
        }
    }
}
