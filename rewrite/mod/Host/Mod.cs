using System;
using System.Globalization;
using System.IO;
using TrackpadCameraControl.Gestures;
#if HAS_CITIES
using CitiesHarmony.API;
using ICities;
#endif

namespace TrackpadCameraControl.Rewrite
{
    public class Mod
#if HAS_CITIES
        : IUserMod
#endif
    {
        public const string E2eInjectEnvVar = "TRACKPAD_E2E_INJECT";
        public const string E2eInjectFlagFileName = "e2e-inject.flag";

        public string Name => OptionsTitle;

        public string Description =>
            "Rewrite — macOS trackpad camera: pan, pinch zoom, orbit. Windows/Linux not supported yet.";

        public static string OptionsTitle
        {
            get
            {
                string version = GetProductVersionDisplay();
                if (string.IsNullOrEmpty(version))
                {
                    return "Trackpad Camera Control Rewrite (macOS)";
                }

                return "Trackpad Camera Control Rewrite (macOS) " + version;
            }
        }

        public static string DebugPanelTitle
        {
            get
            {
                string version = BuildInfo.ShowDevBuildIdentity
                    ? GetAssemblyIdentityDisplay()
                    : GetProductVersionDisplay();
                if (string.IsNullOrEmpty(version))
                {
                    return "Trackpad Camera Control Rewrite (macOS)";
                }

                return "Trackpad Camera Control Rewrite (macOS) " + version;
            }
        }

        internal static string GetProductVersionDisplay()
        {
            try
            {
                const string product = BuildInfo.ProductVersion;
                if (!string.IsNullOrEmpty(product))
                {
                    return product;
                }
            }
            catch
            {
                // fail soft
            }

            return null;
        }

        internal static string GetAssemblyBuildTimestampUtcDisplay()
        {
            try
            {
                const string built = BuildInfo.BuildTimestampUtc;
                if (!string.IsNullOrEmpty(built))
                {
                    return built;
                }
            }
            catch
            {
                // fail soft
            }

            return null;
        }

        internal static string GetAssemblyIdentityDisplay()
        {
            try
            {
                Version v = typeof(Mod).Assembly.GetName().Version;
                if (v == null)
                {
                    return null;
                }

                return v.ToString();
            }
            catch
            {
                return null;
            }
        }

        internal static string GetBuildInfoFooterDisplay()
        {
            if (!BuildInfo.ShowDevBuildIdentity)
            {
                return null;
            }

            string built = GetAssemblyBuildTimestampUtcDisplay();
            if (string.IsNullOrEmpty(built))
            {
                return null;
            }

            return "Built (UTC): " + built;
        }

        internal static string GetBuildInfoPanelDisplay()
        {
            if (!BuildInfo.ShowDevBuildIdentity)
            {
                return null;
            }

            string builtUtc = GetAssemblyBuildTimestampUtcDisplay();
            if (string.IsNullOrEmpty(builtUtc))
            {
                return null;
            }

            if (
                !DateTime.TryParse(
                    builtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out DateTime utc
                )
            )
            {
                return "Built (local): " + builtUtc;
            }

            DateTime local = utc.ToLocalTime();
            return "Built (local): "
                + local.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static ModRuntime Runtime { get; private set; }

        public static void ArmCaptureOnLevelLoaded()
        {
            try
            {
                Runtime?.Pipeline?.ArmCapture();
            }
            catch
            {
                // fail soft
            }
        }

        public static GesturePipeline Pipeline => Runtime?.Pipeline;

        public static InjectGestureSource InjectSource
        {
            get => Runtime?.Inject;
            internal set
            {
                if (Runtime != null)
                {
                    Runtime.Inject = value;
                }
            }
        }

        public static ModSettings Settings => Runtime?.Settings ?? EnsureSettingsInternal();

        private static ModSettings _settingsCache;

        private static ModSettings EnsureSettingsInternal()
        {
            if (_settingsCache == null)
            {
                if (FeelEditor.ActiveStore == null)
                {
                    FeelEditor.ActiveStore = new SettingsStore(SettingsStore.DefaultPath());
                }

                _settingsCache = FeelEditor.ActiveStore.LoadOrFactory();
            }

            return _settingsCache;
        }

        public void OnEnabled()
        {
            try
            {
                EnsureSettingsInternal();
                ModSettings settings = _settingsCache;
                ModLog.Info("mod enabled capture=AppKit");
                IGestureSource source;
                if (IsE2eInjectEnabled())
                {
                    source = new InjectGestureSource();
                }
                else
                {
                    source = GesturePipeline.CreateDefaultCaptureSource();
                }

                Runtime = new ModRuntime(settings, source);
            }
            catch
            {
                EnsureSettingsInternal();
                Runtime = new ModRuntime(
                    _settingsCache,
                    GesturePipeline.CreateDefaultCaptureSource()
                );
            }

#if HAS_CITIES
            try
            {
                HarmonyHelper.DoOnHarmonyReady(Patcher.PatchAll);
                if (!HarmonyHelper.IsHarmonyInstalled)
                {
                    Patcher.LogHarmonyMissingOnce();
                }
            }
            catch
            {
                Patcher.LogHarmonyMissingOnce();
            }

            try
            {
                DebugHost.EnsureCreated();
                DebugHost.ApplyVisibility();
            }
            catch
            {
                // fail soft
            }
#endif
        }

        public void OnDisabled()
        {
#if HAS_CITIES
            try
            {
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Patcher.UnpatchAll();
                }
            }
            catch
            {
                // ignore
            }

            try
            {
                DebugHost.Destroy();
            }
            catch
            {
                // ignore
            }
#endif
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
            try
            {
                FeelEditor.FlushStore(true);
            }
            catch
            {
                // ignore
            }

            try
            {
                Runtime?.Shutdown();
            }
            catch
            {
                // ignore
            }

            Runtime = null;
            _settingsCache = null;
            FeelEditor.ActiveStore = null;
            InputGates.Context = null;
            ModLog.ClearTestSink();
            FeelEditor.ResetSettingsChangedHandlers();
        }

        public static bool IsE2eInjectEnabled()
        {
            try
            {
                string env = Environment.GetEnvironmentVariable(E2eInjectEnvVar);
                if (
                    !string.IsNullOrEmpty(env)
                    && (
                        env == "1"
                        || env.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || env.Equals("yes", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    return true;
                }

                string tmp = Environment.GetEnvironmentVariable("TMPDIR");
                if (string.IsNullOrEmpty(tmp))
                {
                    tmp = Path.GetTempPath();
                }

                if (File.Exists(Path.Combine(tmp, E2eInjectFlagFileName)))
                {
                    return true;
                }

                string asmDir = Path.GetDirectoryName(typeof(FeelMath).Assembly.Location);
                if (
                    !string.IsNullOrEmpty(asmDir)
                    && File.Exists(Path.Combine(asmDir, E2eInjectFlagFileName))
                )
                {
                    return true;
                }
            }
            catch
            {
                // fail soft
            }

            return false;
        }

#if HAS_CITIES
        public void OnSettingsUI(UIHelperBase helper)
        {
            if (helper == null)
            {
                return;
            }

            OptionsHost.Build(helper, EnsureSettingsInternal());
        }
#endif
    }
}
