using System;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Selects how trackpad primitives are captured. Contacts is MultitouchSupport
    /// via the bridge; AppleGestures is in-process AppKit (no Accessibility).
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
        /// </summary>
        public static CaptureBackend Resolve(ModSettings settings, string envValue)
        {
            if (TryParse(envValue, out CaptureBackend fromEnv))
            {
                return fromEnv;
            }

            if (settings == null)
            {
                return CaptureBackend.Contacts;
            }

            return settings.CaptureBackend;
        }

        public static CaptureBackend Resolve(ModSettings settings)
        {
            return Resolve(settings, Environment.GetEnvironmentVariable(EnvVar));
        }
    }
}
