namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Debug host: floating panel chrome over the same FeelCatalog.</summary>
    public static class DebugHost
    {
        public static bool IsCreated { get; private set; }

        public static string Title => Mod.DebugPanelTitle;

        public static void EnsureCreated()
        {
            IsCreated = true;
        }

        public static void ApplyVisibility()
        {
            // Visibility driven by AssistUiEnabled / DebugPanelDismissed when Cities UI is present.
        }

        public static void Destroy()
        {
            IsCreated = false;
        }
    }
}
