namespace TrackpadCameraControl
{
    /// <summary>
    /// Lifecycle bag for mod-enabled state: settings ref, gesture pipeline, inject source, active flag.
    /// Created in <see cref="Mod.OnEnabled"/>, cleared in <see cref="Mod.OnDisabled"/>.
    /// </summary>
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
            var camera = new CameraControllerZoom();
            Pipeline = new GesturePipeline(Settings, source, camera);
            Inject = source as InjectGestureSource;
        }

        /// <summary>True while the mod is enabled in Content Manager.</summary>
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
