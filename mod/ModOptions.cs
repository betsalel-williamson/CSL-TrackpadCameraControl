using System;
using System.Globalization;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Shared Options / in-game panel bindings. Cities UI calls these; tests cover them without ICities.
    /// </summary>
    public static class ModOptions
    {
        public const float ScaleMin = 0f;
        public const float ScaleMax = 100f;

        /// <summary>Legacy alias for tests and older call sites.</summary>
        public const float SensitivityMin = ScaleMin;

        /// <summary>Legacy alias for tests and older call sites.</summary>
        public const float SensitivityMax = ScaleMax;

        public const float SensitivityStep = 0.05f;

        public const float AlphaMin = 0f;
        public const float AlphaMax = 1f;

        public static readonly string[] CaptureBackendLabels = new string[]
        {
            "AppKit (current)",
            "Contacts (legacy)",
        };

        public static readonly string[] GesturePresetLabels = new string[]
        {
            "Maps+ — map-app pan/pinch/yaw; modifier+two-finger orbit",
            "CAD — same pan/pinch/yaw; three-finger orbit",
        };

        public static readonly string MapsPlusDescription =
            "Two-finger pan, pinch zoom, two-finger rotate yaw, modifier+two-finger orbit. Lower conflict with OS three-finger gestures.";

        public static readonly string CadDescription =
            "Same pan/pinch/yaw as Maps+. Three-finger drag orbits (may fight OS Mission Control / Spaces).";

        public static ModSettingsStore Store { get; set; }

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
            NotifyChanged();
        }

        public static int GesturePresetToIndex(GesturePreset preset)
        {
            return preset == GesturePreset.CAD ? 1 : 0;
        }

        public static GesturePreset IndexToGesturePreset(int index)
        {
            return index == 1 ? GesturePreset.CAD : GesturePreset.MapsPlus;
        }

        public static void ApplyGesturePresetIndex(ModSettings settings, int index)
        {
            if (settings == null)
            {
                return;
            }

            settings.ApplyPreset(IndexToGesturePreset(index));
            NotifyChanged();
        }

        public static string PresetDescription(GesturePreset preset)
        {
            return preset == GesturePreset.CAD ? CadDescription : MapsPlusDescription;
        }

        public static float ClampScale(float value)
        {
            if (value < ScaleMin)
            {
                return ScaleMin;
            }

            if (value > ScaleMax)
            {
                return ScaleMax;
            }

            return value;
        }

        public static float ClampSensitivity(float value)
        {
            return ClampScale(value);
        }

        public static float ClampAlpha(float value)
        {
            if (value < AlphaMin)
            {
                return AlphaMin;
            }

            if (value > AlphaMax)
            {
                return AlphaMax;
            }

            return value;
        }

        public static bool TryParseFloat(string text, out float value)
        {
            value = 0f;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            string trimmed = text.Trim();
            if (
                float.TryParse(
                    trimmed,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value
                )
            )
            {
                return true;
            }

            // Accept locale decimal separators (e.g. "1,5").
            return float.TryParse(
                trimmed,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out value
            );
        }

        public static string FormatFloat(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public static bool TryApplyFloat(
            ModSettings settings,
            string text,
            Action<ModSettings, float> apply
        )
        {
            if (settings == null || apply == null)
            {
                return false;
            }

            float parsed;
            if (!TryParseFloat(text, out parsed))
            {
                return false;
            }

            apply(settings, parsed);
            return true;
        }

        public static void ApplyPanSensitivityX(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanSensitivityX = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyPanSensitivityY(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanSensitivityY = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyOrbitYawSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitYawSensitivity = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyOrbitPitchSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchSensitivity = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyZoomSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomSensitivity = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyYawRotateSensitivity(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawRotateSensitivity = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyPanButtonScaleX(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanButtonScaleX = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyPanButtonScaleY(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanButtonScaleY = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyOrbitYawButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitYawButtonScale = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyOrbitPitchButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchButtonScale = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyZoomButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomButtonScale = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyYawRotateButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawRotateButtonScale = ClampScale(value);
            NotifyChanged();
        }

        public static void ApplyPanLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanLowPassAlpha = ClampAlpha(value);
            NotifyChanged();
        }

        public static void ApplyZoomLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomLowPassAlpha = ClampAlpha(value);
            NotifyChanged();
        }

        public static void ApplyYawLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawLowPassAlpha = ClampAlpha(value);
            NotifyChanged();
        }

        public static void ApplyOrbitLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitLowPassAlpha = ClampAlpha(value);
            NotifyChanged();
        }

        public static void ApplyBool(ModSettings settings, Action<ModSettings> mutate)
        {
            if (settings == null || mutate == null)
            {
                return;
            }

            mutate(settings);
            NotifyChanged();
        }

        public static void ResetToFactory(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.CopyFrom(ModSettings.CreateFactoryDefaults());
            if (Store != null)
            {
                Store.SaveNow(settings);
            }
            else
            {
                NotifyChanged();
            }
        }

        public static void NotifyChanged()
        {
            if (Store != null)
            {
                Store.MarkDirty();
                if (Mod.Settings != null)
                {
                    Store.FlushIfNeeded(Mod.Settings, false);
                }
            }
        }

        public static void FlushStore(bool force)
        {
            if (Store != null && Mod.Settings != null)
            {
                Store.FlushIfNeeded(Mod.Settings, force);
            }
        }
    }
}
