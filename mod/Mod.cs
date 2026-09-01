using System;
using System.IO;
#if HAS_CITIES
using CitiesHarmony.API;
using ICities;
#endif

namespace TrackpadCameraControl
{
    public class Mod
#if HAS_CITIES
        : IUserMod
#endif
    {
        public const string E2eInjectEnvVar = "TRACKPAD_E2E_INJECT";
        public const string E2eInjectFlagFileName = "e2e-inject.flag";

        /// <summary>Options tab / Content Manager title: mod name + assembly version.</summary>
        public string Name => OptionsTitle;

        public string Description =>
            "macOS trackpad camera — pan, pinch zoom, orbit. No middle mouse. Windows/Linux not supported yet.";

        /// <summary>
        /// Mod display title including temporary macOS tag and product semver
        /// (e.g. for Options group header / Content Manager).
        /// </summary>
        public static string OptionsTitle
        {
            get
            {
                string version = GetProductVersionDisplay();
                if (string.IsNullOrEmpty(version))
                {
                    return "Trackpad Camera Control (macOS)";
                }

                return "Trackpad Camera Control (macOS) " + version;
            }
        }

        /// <summary>
        /// Product semver from package.json (BuildInfo / InformationalVersion).
        /// Not the assembly Major.Minor.* identity Cities uses for auto-reload.
        /// </summary>
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

        /// <summary>Legacy alias for tests / callers expecting OptionsTitle version source.</summary>
        internal static string GetAssemblyVersionDisplay()
        {
            return GetProductVersionDisplay();
        }

        /// <summary>UTC compile time stamped at MSBuild (Debug panel dev confirmation).</summary>
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

        /// <summary>
        /// Full assembly identity (Major.Minor.Build.Revision). Changes each compile so
        /// Cities auto-reloads; Debug panel shows this beside Built (UTC).
        /// </summary>
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

        public static ModSettings Settings { get; private set; }
        public static GesturePipeline Pipeline { get; private set; }
        public static InjectGestureSource InjectSource { get; internal set; }

        internal static ModSettings EnsureSettings()
        {
            if (Settings == null)
            {
                if (ModOptions.Store == null)
                {
                    ModOptions.Store = new ModSettingsStore(ModSettingsStore.DefaultPath());
                }

                Settings = ModOptions.Store.LoadOrFactory();
            }

            return Settings;
        }

        /// <summary>Test helper: inject settings without touching the disk store.</summary>
        internal static void SetSettingsForTests(ModSettings settings)
        {
            Settings = settings;
        }

        internal static void ClearSettingsForTests()
        {
            Settings = null;
            ModOptions.Store = null;
        }

        public void OnEnabled()
        {
            VanillaCameraSuppress.Enabled = true;
            try
            {
                EnsureSettings();
                GestureCaptureLog.Line(
                    "mod enabled backend=" + CaptureBackendFlags.Resolve(Settings)
                );
                IGestureSource source;
                if (IsE2eInjectEnabled())
                {
                    InjectSource = new InjectGestureSource();
                    source = InjectSource;
                }
                else
                {
                    InjectSource = null;
                    source = CreateCaptureSource(Settings);
                }

                Pipeline = new GesturePipeline(Settings, source);
                source.Connect();
            }
            catch
            {
                // Fail soft: gestures may be unavailable; suppress stays on while the mod is enabled.
                EnsureSettings();
                InjectSource = null;
                Pipeline = new GesturePipeline(Settings, new InProcessGestureSource());
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
                TuningPanelHost.Destroy();
            }
            catch
            {
                // ignore
            }
#endif
            VanillaCameraSuppress.Enabled = false;
            try
            {
                ModOptions.FlushStore(true);
            }
            catch
            {
                // ignore
            }

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
            InjectSource = null;
            ModOptions.Store = null;
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

                string asmDir = Path.GetDirectoryName(typeof(GestureFrame).Assembly.Location);
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

            ModSettings s = EnsureSettings();
            OptionsSettingsUi.Build(helper, s);
        }
#endif

        /// <summary>
        /// Product surface uses <see cref="CaptureBackendFlags.Resolve"/>: without
        /// <c>ENABLE_CONTACTS_CAPTURE</c>, AppleKit wins unless maintainer env
        /// <c>TRACKPAD_CAPTURE_BACKEND=contacts</c> overrides.
        /// </summary>
        internal static IGestureSource CreateCaptureSource(ModSettings settings)
        {
            if (CaptureBackendFlags.Resolve(settings) == CaptureBackend.AppleGestures)
            {
                return new AppleGestureSource();
            }

            if (settings != null && settings.BridgeEnabled)
            {
                return new IpcGestureSource();
            }

            return new InProcessGestureSource();
        }
    }
}
