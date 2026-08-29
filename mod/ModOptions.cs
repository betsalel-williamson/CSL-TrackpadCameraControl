namespace TrackpadCameraControl
{
    /// <summary>
    /// Options bindings for capture backend and sensitivities. Cities UIHelper
    /// calls these; tests cover them without ICities.
    /// </summary>
    public static class ModOptions
    {
        public const float SensitivityMin = 0.05f;
        public const float SensitivityMax = 8f;
        public const float SensitivityStep = 0.05f;

        public static readonly string[] CaptureBackendLabels = new string[]
        {
            "AppKit (current)",
            "Contacts (legacy)",
        };

        public static int CaptureBackendToIndex(CaptureBackend backend)
        {
            return backend == CaptureBackend.Contacts ? 1 : 0;
        }

        public static CaptureBackend IndexToCaptureBackend(int index)
        {
            return index == 1 ? CaptureBackend.Contacts : CaptureBackend.AppleGestures;
        }

        public static void ApplyCaptureBackendIndex(ModSettings settings, int index)
        {
            if (settings == null)
            {
                return;
            }

            settings.CaptureBackend = IndexToCaptureBackend(index);
        }

        public static float ClampSensitivity(float value)
        {
            if (value < SensitivityMin)
            {
                return SensitivityMin;
            }

            if (value > SensitivityMax)
            {
                return SensitivityMax;
            }

            return value;
        }

        public static void ApplyPanSensitivityX(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanSensitivityX = ClampSensitivity(value);
        }

        public static void ApplyPanSensitivityY(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanSensitivityY = ClampSensitivity(value);
        }

        public static void ApplyOrbitYawSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitYawSensitivity = ClampSensitivity(value);
        }

        public static void ApplyOrbitPitchSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchSensitivity = ClampSensitivity(value);
        }

        public static void ApplyZoomSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomSensitivity = ClampSensitivity(value);
        }

        public static void ApplyYawRotateSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawRotateSensitivity = ClampSensitivity(value);
        }
    }
}
