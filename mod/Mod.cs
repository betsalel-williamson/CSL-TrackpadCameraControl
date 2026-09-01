using System;
using System.IO;
#if HAS_CITIES
using CitiesHarmony.API;
using ColossalFramework.UI;
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

        /// <summary>
        /// Debug panel footer line (Built UTC + asm). Null when neither stamp is available.
        /// </summary>
        internal static string GetBuildInfoFooterDisplay()
        {
            string built = GetAssemblyBuildTimestampUtcDisplay();
            string asm = GetAssemblyIdentityDisplay();
            if (string.IsNullOrEmpty(built) && string.IsNullOrEmpty(asm))
            {
                return null;
            }

            string line = "Built (UTC): " + (built ?? "?");
            if (!string.IsNullOrEmpty(asm))
            {
                line += "  ·  asm " + asm;
            }

            return line;
        }

        public static ModRuntime Runtime { get; private set; }

        /// <summary>Arm capture connect after city load or mod enable while a city is active.</summary>
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

        /// <summary>Shim for call sites expecting <c>Mod.Pipeline</c>.</summary>
        public static GesturePipeline Pipeline => Runtime?.Pipeline;

        /// <summary>Shim for call sites expecting <c>Mod.InjectSource</c>.</summary>
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

        internal static ModSettings EnsureSettings()
        {
            return Settings;
        }

        private static ModSettings _settingsCache;

        private static ModSettings EnsureSettingsInternal()
        {
            if (_settingsCache == null)
            {
                if (ModOptions.Store == null)
                {
                    ModOptions.Store = new ModSettingsStore(ModSettingsStore.DefaultPath());
                }

                _settingsCache = ModOptions.Store.LoadOrFactory();
            }

            return _settingsCache;
        }

        public void OnEnabled()
        {
            try
            {
                EnsureSettingsInternal();
                ModSettings settings = _settingsCache;
                GestureCaptureLog.Line(
                    "mod enabled backend=" + CaptureBackendFlags.Resolve(settings)
                );
                IGestureSource source;
                if (IsE2eInjectEnabled())
                {
                    source = new InjectGestureSource();
                }
                else
                {
                    source = CreateCaptureSource(settings);
                }

                Runtime = new ModRuntime(settings, source);
            }
            catch
            {
                // Fail soft: gestures may be unavailable; suppress stays on while the mod is enabled.
                EnsureSettingsInternal();
                Runtime = new ModRuntime(_settingsCache, new InProcessGestureSource());
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

            // Auto-reload (Paradox Automate) runs OnDisabled → Destroy then OnEnabled while the
            // city stays loaded. OnLevelLoaded does not fire again — recreate Debug UI here.
            // EnsureCreated fails soft when UIView is unavailable (main menu / early boot).
            try
            {
                TuningPanelHost.EnsureCreated();
                TuningPanelHost.ApplyVisibility();
                if (UIView.GetAView() != null)
                {
                    ArmCaptureOnLevelLoaded();
                }
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
                TuningPanelHost.Destroy();
                VanillaCameraKeyLabelsWatch.Unhook();
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
                ModOptions.FlushStore(true);
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
            ModOptions.Store = null;
            InputGates.Context = null;
            GestureCaptureLog.Close();
            GestureCaptureLog.PathResolver = null;
            ModOptions.ResetSettingsChangedHandlers();
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
