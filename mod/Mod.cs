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

        public string Name => "Trackpad Camera Control";
        public string Description =>
            "Trackpad multitouch camera — pan, orbit, zoom. Vanilla scroll-zoom suppressed while enabled.";

        public static ModSettings Settings { get; private set; }
        public static GesturePipeline Pipeline { get; private set; }
        public static InjectGestureSource InjectSource { get; internal set; }

        internal static ModSettings EnsureSettings()
        {
            if (Settings == null)
            {
                Settings = new ModSettings();
            }

            return Settings;
        }

        internal static void ClearSettingsForTests()
        {
            Settings = null;
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
#endif
            VanillaCameraSuppress.Enabled = false;
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
            helper.AddGroup("Capture");
            helper.AddDropdown(
                "Interpreter",
                ModOptions.CaptureBackendLabels,
                ModOptions.CaptureBackendToIndex(s.CaptureBackend),
                sel => ModOptions.ApplyCaptureBackendIndex(s, sel)
            );

            helper.AddGroup("Sensitivity");
            helper.AddSlider(
                "Pan X",
                ModOptions.SensitivityMin,
                ModOptions.SensitivityMax,
                ModOptions.SensitivityStep,
                s.PanSensitivityX,
                v => ModOptions.ApplyPanSensitivityX(s, v)
            );
            helper.AddSlider(
                "Pan Y",
                ModOptions.SensitivityMin,
                ModOptions.SensitivityMax,
                ModOptions.SensitivityStep,
                s.PanSensitivityY,
                v => ModOptions.ApplyPanSensitivityY(s, v)
            );
            helper.AddSlider(
                "Orbit yaw",
                ModOptions.SensitivityMin,
                ModOptions.SensitivityMax,
                ModOptions.SensitivityStep,
                s.OrbitYawSensitivity,
                v => ModOptions.ApplyOrbitYawSensitivity(s, v)
            );
            helper.AddSlider(
                "Orbit pitch",
                ModOptions.SensitivityMin,
                ModOptions.SensitivityMax,
                ModOptions.SensitivityStep,
                s.OrbitPitchSensitivity,
                v => ModOptions.ApplyOrbitPitchSensitivity(s, v)
            );
            helper.AddSlider(
                "Zoom",
                ModOptions.SensitivityMin,
                ModOptions.SensitivityMax,
                ModOptions.SensitivityStep,
                s.ZoomSensitivity,
                v => ModOptions.ApplyZoomSensitivity(s, v)
            );
            helper.AddSlider(
                "Yaw rotate",
                ModOptions.SensitivityMin,
                ModOptions.SensitivityMax,
                ModOptions.SensitivityStep,
                s.YawRotateSensitivity,
                v => ModOptions.ApplyYawRotateSensitivity(s, v)
            );
        }
#endif

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
