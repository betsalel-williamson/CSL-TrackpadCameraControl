using System;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Selects how trackpad primitives are captured. Both interpreters run in-process.
    /// Contacts is MultitouchSupport; AppleGestures is AppKit (no Accessibility).
    /// </summary>
    public enum CaptureBackend
    {
        Contacts = 0,
        AppleGestures = 1,
    }

    public static class CaptureBackendFlags
    {
        public const string EnvVar = "TRACKPAD_CAPTURE_BACKEND";

        public static bool TryParse(string value, out CaptureBackend backend)
        {
            backend = CaptureBackend.Contacts;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            string v = value.Trim().ToLowerInvariant();
            if (v == "apple" || v == "applegestures" || v == "appkit")
            {
                backend = CaptureBackend.AppleGestures;
                return true;
            }

            if (v == "contacts" || v == "multitouch" || v == "old")
            {
                backend = CaptureBackend.Contacts;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Env wins when set so a launch flag can switch backends without Options UI.
        /// When <see cref="FeatureFlags.EnableContactsCapture"/> is off, product play forces
        /// AppleGestures (ignore stored Contacts). Maintainer env
        /// <c>TRACKPAD_CAPTURE_BACKEND=contacts</c> may still override for local debugging.
        /// </summary>
        public static CaptureBackend Resolve(ModSettings settings, string envValue)
        {
            // Maintainer override: TRACKPAD_CAPTURE_BACKEND may force Contacts even when the
            // product flag is off; normal play with flag off always uses AppleKit.
            if (TryParse(envValue, out CaptureBackend fromEnv))
            {
                return fromEnv;
            }

            CaptureBackend fromSettings =
                settings != null ? settings.CaptureBackend : CaptureBackend.AppleGestures;

            // Helper call keeps CS0162 quiet while EnableContactsCapture is a const false.
            return IsContactsCaptureEnabled() ? fromSettings : CaptureBackend.AppleGestures;
        }

        private static bool IsContactsCaptureEnabled()
        {
            return FeatureFlags.EnableContactsCapture;
        }

        public static CaptureBackend Resolve(ModSettings settings)
        {
            return Resolve(settings, Environment.GetEnvironmentVariable(EnvVar));
        }
    }
}
