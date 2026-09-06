#if HAS_CITIES
using ColossalFramework.UI;

namespace TrackpadCameraControl.Rewrite
{
    internal static partial class TuningPanelHost
    {
        /// <summary>Keep Debug panel below Options or other modal UI after create, rebuild, or visibility changes.</summary>
        public static void ApplyPanelStackOrder()
        {
            if (_root == null)
            {
                return;
            }

            if (!GameUiContext.Default.IsMenuOrOptionsOpen())
            {
                return;
            }

            _root.SendToBack();
            if (_reopen != null && _reopen.isVisible)
            {
                _reopen.SendToBack();
            }
        }
    }
}
#endif
