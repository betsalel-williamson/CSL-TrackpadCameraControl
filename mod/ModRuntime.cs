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
        public bool IsActive { get; internal set; }

        public ModRuntime(ModSettings settings, IGestureSource source)
        {
            Settings = settings ?? new ModSettings();
            IsActive = true;
            var camera = new CameraControllerZoom();
            Pipeline = new GesturePipeline(Settings, source, camera);
            Inject = source as InjectGestureSource;
            Pipeline.Source.Connect();
        }

        private ModRuntime(
            ModSettings settings,
            GesturePipeline pipeline,
            InjectGestureSource inject,
            bool isActive
        )
        {
            Settings = settings;
            Pipeline = pipeline;
            Inject = inject;
            IsActive = isActive;
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

        /// <summary>Test helper: lightweight runtime without capture connect.</summary>
        internal static ModRuntime CreateForTests(bool active = true)
        {
            var settings = new ModSettings();
            var source = new InProcessGestureSource();
            var camera = new CameraControllerZoom();
            var pipeline = new GesturePipeline(settings, source, camera);
            return new ModRuntime(settings, pipeline, null, active);
        }

        /// <summary>Test helper: set mod-active policy without full OnEnabled.</summary>
        internal static void SetModActiveForTests(bool active)
        {
            if (active)
            {
                if (Mod.Runtime == null)
                {
                    Mod.SetRuntimeForTests(CreateForTests(true));
                }
                else
                {
                    Mod.Runtime.IsActive = true;
                }
            }
            else if (Mod.Runtime != null)
            {
                Mod.Runtime.Shutdown();
                Mod.ClearRuntimeForTests();
            }
        }

        /// <summary>Test helper: tear down runtime and Harmony buffer flags.</summary>
        internal static void ClearForTests()
        {
            if (Mod.Runtime != null)
            {
                Mod.Runtime.Shutdown();
            }

            Mod.ClearRuntimeForTests();
        }
    }
}
