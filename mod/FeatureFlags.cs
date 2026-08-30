namespace TrackpadCameraControl
{
    /// <summary>
    /// Product-surface gates. Default off for ship; builders consult these before exposing gated UI.
    /// </summary>
    public static class FeatureFlags
    {
        public const bool EnableCadGestureStyle = false;
        public const bool EnableContactsCapture = false;
        public const bool EnableAssistChrome = false;
    }
}
