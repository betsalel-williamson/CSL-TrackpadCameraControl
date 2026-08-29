namespace TrackpadCameraControl
{
    public static class VanillaCameraSuppress
    {
        public static bool Enabled { get; set; }

        public static bool ShouldSkipScrollWheel()
        {
            return Enabled;
        }

        public static bool ShouldSkipMouseHandler(bool rotateBindingHeld)
        {
            return Enabled && rotateBindingHeld;
        }
    }
}
