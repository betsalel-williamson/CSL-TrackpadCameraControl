#if HAS_CITIES
using ICities;
#endif

namespace TrackpadCameraControl
{
    public class Mod
#if HAS_CITIES
        : IUserMod
#endif
    {
        public string Name => "Trackpad Camera Control";
        public string Description =>
            "Trackpad multitouch camera — pinch zoom MVP. Hot-configurable Options later.";

        public static ModSettings Settings { get; private set; }
        public static GesturePipeline Pipeline { get; private set; }

        public void OnEnabled()
        {
            try
            {
                Settings = new ModSettings();
                IGestureSource source = Settings.BridgeEnabled
                    ? (IGestureSource)new IpcGestureSource()
                    : new InProcessGestureSource();
                Pipeline = new GesturePipeline(Settings, source);
                if (Settings.BridgeEnabled)
                {
                    source.Connect(); // fail soft if bridge not running
                }
            }
            catch
            {
                // Fail soft: leave vanilla input alone.
                Settings = new ModSettings();
                Pipeline = new GesturePipeline(Settings, new InProcessGestureSource());
            }
        }

        public void OnDisabled()
        {
            try
            {
                Pipeline?.Shutdown();
            }
            catch
            {
                // ignore
            }

            Pipeline = null;
            Settings = null;
        }
    }
}
