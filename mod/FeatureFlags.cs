namespace TrackpadCameraControl
{
    /// <summary>
    /// Product-surface gates. Default off for ship; builders consult these before exposing gated UI.
    /// Use static readonly (not const) so gated UI/code is not CS0162-unreachable while flags are off.
    /// </summary>
    public static class FeatureFlags
    {
        public static readonly bool EnableCadGestureStyle = false;
        public static readonly bool EnableContactsCapture = false;
        public static readonly bool EnableAssistChrome = false;
    }
}
