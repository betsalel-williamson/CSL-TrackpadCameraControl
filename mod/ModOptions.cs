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
            "Maps+ — map-app pan/pinch/yaw; Option (⌥)+two-finger orbit",
            "CAD — same pan/pinch/yaw; three-finger orbit",
        };

        public static readonly string MapsPlusDescription =
            "Two-finger pan, pinch zoom, two-finger rotate yaw, Option (⌥)+two-finger orbit. Lower conflict with OS three-finger gestures.";

        public static readonly string CadDescription =
            "Same pan/pinch/yaw as Maps+. Three-finger drag orbits (may fight OS Mission Control / Spaces).";

        /// <summary>Pan section heading: meaning + Maps+ activation.</summary>
        public const string OpHeadingPan =
            "Pan — Slide the camera laterally · two-finger drag";

        /// <summary>Zoom section heading: meaning + Maps+ activation.</summary>
        public const string OpHeadingZoom =
            "Zoom — Change camera distance / size · pinch (mouse wheel: vanilla zoom)";

        /// <summary>Rotate section heading: meaning + Maps+ activation.</summary>
        public const string OpHeadingRotate =
            "Rotate — Yaw around the vertical axis · two-finger rotate";

        /// <summary>Orbit section heading: meaning + Maps+ activation.</summary>
        public const string OpHeadingOrbit =
            "Orbit — Pitch + yaw around the pivot · Option (⌥)+two-finger drag";

        public static ModSettingsStore Store { get; set; }

        /// <summary>
        /// Raised after settings apply + force flush so Options / Debug panel can refresh.
        /// </summary>
        public static event Action SettingsChanged;

        /// <summary>Test helper: drop all SettingsChanged subscribers.</summary>
        internal static void ClearSettingsChangedForTests()
        {
            SettingsChanged = null;
        }

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
            NotifyChanged(settings);
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
            NotifyChanged(settings);
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

            return Round2(value);
        }

        /// <summary>
        /// Sensitivity numeric policy: round to two decimals; no upper cap.
        /// Non-positive values become 0 (Apply*Sensitivity ignores ≤ 0 separately).
        /// </summary>
        public static float ClampSensitivity(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return Round2(value);
        }

        public static float Round2(float value)
        {
            return (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
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
            return Round2(value).ToString("0.00", CultureInfo.InvariantCulture);
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

        private static void ApplyPositiveSensitivity(
            ModSettings settings,
            float value,
            Action<ModSettings, float> assign
        )
        {
            if (settings == null || assign == null)
            {
                return;
            }

            if (value <= 0f)
            {
                return;
            }

            assign(settings, Round2(value));
            AfterFeelFieldChanged(settings);
        }

        /// <summary>
        /// Feel-field apply path: dirty → New Preset autosave, then notify/sync.
        /// </summary>
        public static void AfterFeelFieldChanged(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            FeelProfiles.EnsureDirtyNewPreset(settings, Store);
            NotifyChanged(settings);
        }

        public static void ApplyPanSensitivityX(ModSettings settings, float value)
        {
            ApplyPositiveSensitivity(settings, value, (s, v) => s.PanSensitivityX = v);
        }

        public static void ApplyPanSensitivityY(ModSettings settings, float value)
        {
            ApplyPositiveSensitivity(settings, value, (s, v) => s.PanSensitivityY = v);
        }

        public static void ApplyOrbitYawSensitivity(ModSettings settings, float value)
        {
            ApplyPositiveSensitivity(settings, value, (s, v) => s.OrbitYawSensitivity = v);
        }

        public static void ApplyOrbitPitchSensitivity(ModSettings settings, float value)
        {
            ApplyPositiveSensitivity(settings, value, (s, v) => s.OrbitPitchSensitivity = v);
        }

        public static void ApplyOrbitPitchMin(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchMin = Round2(value);
            AfterFeelFieldChanged(settings);
        }

        public static void ApplyOrbitPitchMax(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchMax = Round2(value);
            AfterFeelFieldChanged(settings);
        }

        public static void ApplyZoomSensitivity(ModSettings settings, float value)
        {
            ApplyPositiveSensitivity(settings, value, (s, v) => s.ZoomSensitivity = v);
        }

        public static void ApplyYawRotateSensitivity(ModSettings settings, float value)
        {
            ApplyPositiveSensitivity(settings, value, (s, v) => s.YawRotateSensitivity = v);
        }

        public static void ApplyPanButtonScaleX(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanButtonScaleX = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyPanButtonScaleY(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanButtonScaleY = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyOrbitYawButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitYawButtonScale = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyOrbitPitchButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchButtonScale = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyZoomButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomButtonScale = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyYawRotateButtonScale(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawRotateButtonScale = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyPanLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanLowPassAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyZoomLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomLowPassAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyYawLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawLowPassAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyOrbitLowPassAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitLowPassAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyBool(ModSettings settings, Action<ModSettings> mutate)
        {
            if (settings == null || mutate == null)
            {
                return;
            }

            mutate(settings);
            NotifyChanged(settings);
        }

        /// <summary>
        /// Apply a feel-surface bool (enables / reverse). Dirties to New Preset.
        /// </summary>
        public static void ApplyFeelBool(ModSettings settings, Action<ModSettings> mutate)
        {
            if (settings == null || mutate == null)
            {
                return;
            }

            mutate(settings);
            AfterFeelFieldChanged(settings);
        }

        public static void ResetToFactory(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.CopyFrom(ModSettings.CreateFactoryDefaults());
            settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
            if (Store != null)
            {
                Store.SaveNow(settings);
            }

            RaiseSettingsChanged();
        }

        public static void ApplyFeelDefault(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            FeelProfiles.ApplyDefault(settings);
            settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
            NotifyChanged(settings);
        }

        public static void ApplyFeelSlow(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            FeelProfiles.ApplySlow(settings);
            settings.ActiveFeelPresetName = FeelProfiles.NameSlow;
            NotifyChanged(settings);
        }

        public static void ApplyFeelFast(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            FeelProfiles.ApplyFast(settings);
            settings.ActiveFeelPresetName = FeelProfiles.NameFast;
            NotifyChanged(settings);
        }

        /// <summary>
        /// Save as… — promotes current feel to a named preset. Cannot overwrite Slow/Default/Fast.
        /// Removes the New Preset scratch slot when promoting to another name.
        /// </summary>
        public static bool SaveNamedFeelPreset(ModSettings settings, string name)
        {
            if (settings == null || string.IsNullOrEmpty(name) || Store == null)
            {
                return false;
            }

            if (FeelProfiles.IsBuiltInName(name))
            {
                return false;
            }

            if (!Store.SaveUserPreset(name, settings, settings))
            {
                return false;
            }

            settings.ActiveFeelPresetName = name;
            if (!string.Equals(name, FeelProfiles.NameNewPreset, StringComparison.Ordinal))
            {
                Store.RemoveUserPreset(FeelProfiles.NameNewPreset);
            }

            Store.SaveNow(settings);
            RaiseSettingsChanged();
            return true;
        }

        /// <summary>Load a named feel preset into live settings.</summary>
        public static bool LoadNamedFeelPreset(ModSettings settings, string name)
        {
            if (settings == null || string.IsNullOrEmpty(name) || Store == null)
            {
                return false;
            }

            ModSettings snap;
            if (!Store.TryGetUserPreset(name, out snap))
            {
                return false;
            }

            FeelProfiles.CopyFeelFields(settings, snap);
            settings.ActiveFeelPresetName = name;
            NotifyChanged(settings);
            return true;
        }

        public static string[] ListNamedFeelPresetNames()
        {
            if (Store == null)
            {
                return new string[0];
            }

            return Store.ListUserPresetNames();
        }

        public static void NotifyChanged()
        {
            NotifyChanged(Mod.Settings);
        }

        /// <summary>Force-flush the given settings blob and raise <see cref="SettingsChanged"/>.</summary>
        public static void NotifyChanged(ModSettings settings)
        {
            if (Store != null && settings != null)
            {
                Store.MarkDirty();
                Store.FlushIfNeeded(settings, true);
            }

            RaiseSettingsChanged();
        }

        private static void RaiseSettingsChanged()
        {
            Action handler = SettingsChanged;
            if (handler != null)
            {
                handler();
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
