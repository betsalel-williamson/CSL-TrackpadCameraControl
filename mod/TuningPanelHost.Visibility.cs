namespace TrackpadCameraControl
{
    internal static partial class TuningPanelHost
    {
        private static bool _dismissedByUser;

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
            _dismissedByUser = false;
        }

        internal static bool IsUserDismissedForTests()
        {
            return _dismissedByUser;
        }

        internal static void SetUserDismissedForTests(bool dismissed)
        {
            _dismissedByUser = dismissed;
        }
    }
}
