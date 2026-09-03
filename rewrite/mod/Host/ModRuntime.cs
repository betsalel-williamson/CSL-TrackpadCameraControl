using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    public sealed class ModRuntime
    {
        public ModSettings Settings { get; }
        public GesturePipeline Pipeline { get; }
        public FeelEditor Editor { get; }
        public bool IsActive { get; private set; }

        public ModRuntime(ModSettings settings, IGestureSource source, FeelEditor editor = null)
        {
            Settings = settings ?? new ModSettings();
            IsActive = true;
            var camera = new CitiesCameraAdapter();
            Pipeline = new GesturePipeline(Settings, source, camera);
            Editor =
                editor
                ?? new FeelEditor(
                    Settings,
                    FeelEditor.ActiveStore ?? new SettingsStore(SettingsStore.DefaultPath())
                );
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
