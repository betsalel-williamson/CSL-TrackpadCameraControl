using System;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl.Rewrite
{
    public static class ModLog
    {
        public const string Prefix = "[TrackpadCameraControl] ";

        internal static Action<string> TestSink;

        public static void Info(string message)
        {
            if (message == null)
            {
                return;
            }

            Action<string> sink = TestSink;
            if (sink != null)
            {
                sink(Prefix + message);
                return;
            }

#if HAS_CITIES
            Debug.Log(Prefix + message);
#endif
        }

        public static void CaptureTrace(string message)
        {
            if (!IsCaptureTraceEnabled() || message == null)
            {
                return;
            }

            Info(message);
        }

        internal static void ClearTestSink()
        {
            TestSink = null;
        }

        private static bool? _captureTraceEnabled;

        private static bool IsCaptureTraceEnabled()
        {
            if (_captureTraceEnabled.HasValue)
            {
                return _captureTraceEnabled.Value;
            }

            bool enabled = false;
            try
            {
                string env = Environment.GetEnvironmentVariable("TRACKPAD_CAPTURE_TRACE");
                enabled =
                    env == "1"
                    || string.Equals(env, "true", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(env, "yes", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                enabled = false;
            }

            _captureTraceEnabled = enabled;
            return enabled;
        }
    }
}
