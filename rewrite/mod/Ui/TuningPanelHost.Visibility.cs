namespace TrackpadCameraControl.Rewrite
{
    internal static partial class TuningPanelHost
    {
        internal static bool ShouldShowRoot(bool assistEnabled, bool dismissed)
        {
            return assistEnabled && !dismissed;
        }

        internal static bool ShouldShowReopen(bool assistEnabled, bool dismissed)
        {
            return assistEnabled && dismissed;
        }

        public static void ClearUserDismiss()
        {
            ModSettings s = Mod.EnsureSettings();
            if (s == null)
            {
                return;
            }

            s.DebugPanelDismissed = false;
            ModOptions.NotifyChanged();
        }
    }
}
