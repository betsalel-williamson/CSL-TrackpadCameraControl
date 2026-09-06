namespace TrackpadCameraControl
{
    /// <summary>
    /// Product-surface gates as <b>compile-time</b> symbols (see mod csproj).
    /// Symbols: <c>ENABLE_CAD_GESTURE_STYLE</c>, <c>ENABLE_CONTACTS_CAPTURE</c>,
    /// <c>ENABLE_ASSIST_CHROME</c>. Off by default for ship so gated code is not compiled in.
    /// Enable with MSBuild: <c>-p:EnableCadGestureStyle=true</c> (etc.).
    /// Const mirrors exist for tests and docs; call sites must use <c>#if</c>, not runtime if.
    /// </summary>
    public static class FeatureFlags
    {
#if ENABLE_CAD_GESTURE_STYLE
        public const bool EnableCadGestureStyle = true;
#else
        public const bool EnableCadGestureStyle = false;
#endif

#if ENABLE_CONTACTS_CAPTURE
        public const bool EnableContactsCapture = true;
#else
        public const bool EnableContactsCapture = false;
#endif

#if ENABLE_ASSIST_CHROME
        public const bool EnableAssistChrome = true;
#else
        public const bool EnableAssistChrome = false;
#endif
    }
}
