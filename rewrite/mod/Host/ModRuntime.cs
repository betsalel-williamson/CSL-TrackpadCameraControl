using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    public sealed class ModRuntime
    {
        public ModSettings Settings { get; }
        public GesturePipeline Pipeline { get; }
        public InjectGestureSource Inject { get; internal set; }
        public bool IsActive { get; private set; }

        public ModRuntime(ModSettings settings, IGestureSource source)
        {
            Settings = settings ?? new ModSettings();
            IsActive = true;
            var camera = new CitiesCameraAdapter();
            Pipeline = new GesturePipeline(Settings, source, camera);
            Inject = source as InjectGestureSource;
        }

        public static bool IsModActive()
        {
            return Mod.Runtime != null && Mod.Runtime.IsActive;
        }

        public void Shutdown()
        {
            IsActive = false;
            try
            {
                Pipeline?.Shutdown();
            }
            catch
            {
                // fail soft
            }

            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
        }
    }
}
