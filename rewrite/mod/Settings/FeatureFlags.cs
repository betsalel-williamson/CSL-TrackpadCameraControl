namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Product-surface gates as <b>compile-time</b> symbols (see mod csproj).
    /// Symbol: <c>ENABLE_ASSIST_CHROME</c>. Off by default for ship so gated code is not compiled in.
    /// Enable with MSBuild: <c>-p:EnableAssistChrome=true</c>.
    /// Const mirrors exist for tests and docs; call sites must use <c>#if</c>, not runtime if.
    /// </summary>
    public static class FeatureFlags
    {
#if ENABLE_ASSIST_CHROME
        public const bool EnableAssistChrome = true;
#else
        public const bool EnableAssistChrome = false;
#endif
    }
}
