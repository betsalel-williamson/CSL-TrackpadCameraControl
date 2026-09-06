namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Per-frame Harmony buffers — not preferences. Policy lives in <see cref="InputGates"/>.
    /// </summary>
    public static class VanillaCameraSuppress
    {
        public static bool PreciseTrackpadScroll { get; set; }

        public static bool MenuOrOverUi { get; set; }
    }
}
