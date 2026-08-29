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

        public void OnEnabled()
        {
            VanillaCameraSuppress.Enabled = true;
            try
            {
                Settings = new ModSettings();
                IGestureSource source;
                if (IsE2eInjectEnabled())
                {
                    InjectSource = new InjectGestureSource();
                    source = InjectSource;
                }
                else
                {
                    InjectSource = null;
                    source = Settings.BridgeEnabled
                        ? (IGestureSource)new IpcGestureSource()
                        : new InProcessGestureSource();
                }

                Pipeline = new GesturePipeline(Settings, source);
                source.Connect();
            }
            catch
            {
                // Fail soft: gestures may be unavailable; suppress stays on while the mod is enabled.
                Settings = new ModSettings();
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
    }
}
