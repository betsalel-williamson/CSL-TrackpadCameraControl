using System;
using System.Collections.Generic;
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

        /// <summary>Product Sensitivity slider floor as a fraction of factory default.</summary>
        public const float SensitivitySliderMinFactor = 0.1f;

        /// <summary>Product Sensitivity slider ceiling as a fraction of factory default.</summary>
        public const float SensitivitySliderMaxFactor = 2f;

        /// <summary>Product Sensitivity slider step as a fraction of factory default (~10%).</summary>
        public const float SensitivitySliderStepFactor = 0.1f;

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
        public const string OpHeadingPan = "Pan — Slide the camera laterally · two-finger drag";

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
        /// Sensitivity numeric policy: round to four decimals (supports pan 0.005 after
        /// folding the old 0.01 scroll unit into defaults); no upper cap.
        /// Non-positive values become 0 (Apply*Sensitivity ignores ≤ 0 separately).
        /// </summary>
        public static float ClampGain(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return RoundGain(value);
        }

        /// <summary>Options Sensitivity slider minimum for a factory default (0.1×).</summary>
        public static float SensitivitySliderMin(float factoryDefault)
        {
            return RoundGain(factoryDefault * SensitivitySliderMinFactor);
        }

        /// <summary>Options Sensitivity slider maximum for a factory default (2×).</summary>
        public static float SensitivitySliderMax(float factoryDefault)
        {
            return RoundGain(factoryDefault * SensitivitySliderMaxFactor);
        }

        /// <summary>Options Sensitivity slider step for a factory default (~10%).</summary>
        public static float SensitivitySliderStep(float factoryDefault)
        {
            float step = RoundGain(factoryDefault * SensitivitySliderStepFactor);
            return step > 0f ? step : 0.0001f;
        }

        /// <summary>
        /// Clamp a Sensitivity edit to the product Options slider range for that factory default.
        /// Non-positive / out-of-range values snap into [0.1×, 2×] factory.
        /// </summary>
        public static float ClampGainToFactoryRange(float value, float factoryDefault)
        {
            float min = SensitivitySliderMin(factoryDefault);
            float max = SensitivitySliderMax(factoryDefault);
            if (min < 0.0001f)
            {
                min = 0.0001f;
            }

            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return RoundGain(value);
        }

        public static float Round2(float value)
        {
            return (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Four-decimal round for Sensitivity after scroll-unit fold into defaults.</summary>
        public static float RoundGain(float value)
        {
            return (float)Math.Round(value, 4, MidpointRounding.AwayFromZero);
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
                float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
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

        private static void ApplyPositiveGain(
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

            assign(settings, RoundGain(value));
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

        public static void ApplyPanGainX(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.PanGainX = v);
        }

        public static void ApplyPanGainY(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.PanGainY = v);
        }

        public static void ApplyOrbitYawGain(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.OrbitYawGain = v);
        }

        public static void ApplyOrbitPitchGain(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.OrbitPitchGain = v);
        }

        /// <summary>
        /// Schema-retained for old presets. Live orbit clamp is vanilla 0…90 in
        /// <see cref="CameraApplicator"/>; negative values are ignored.
        /// </summary>
        public static void ApplyOrbitPitchMin(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            if (value < 0f)
            {
                return;
            }

            settings.OrbitPitchMin = Round2(value);
            AfterFeelFieldChanged(settings);
        }

        /// <summary>
        /// Schema-retained for old presets. Live orbit clamp is vanilla 0…90 in
        /// <see cref="CameraApplicator"/>; non-positive values are ignored.
        /// </summary>
        public static void ApplyOrbitPitchMax(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            if (value <= 0f)
            {
                return;
            }

            settings.OrbitPitchMax = Round2(value);
            AfterFeelFieldChanged(settings);
        }

        public static void ApplyZoomGain(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.ZoomGain = v);
        }

        public static void ApplyYawRotateGain(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.YawRotateGain = v);
        }

        public static void ApplyPanStepX(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanStepX = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyPanStepY(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanStepY = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyOrbitYawStep(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitYawStep = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyOrbitPitchStep(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitPitchStep = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyZoomStep(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomStep = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyYawRotateStep(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawRotateStep = ClampScale(value);
            NotifyChanged(settings);
        }

        public static void ApplyPanFilterAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanFilterAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyZoomFilterAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.ZoomFilterAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyYawFilterAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.YawFilterAlpha = ClampAlpha(value);
            NotifyChanged(settings);
        }

        public static void ApplyOrbitFilterAlpha(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.OrbitFilterAlpha = ClampAlpha(value);
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

        /// <summary>Last entry in the feel-preset dropdown — promotes current feel to a named slot.</summary>
        public const string FeelPresetSaveAsLabel = "Save as…";

        /// <summary>
        /// Dropdown items: Slow, Default, Fast, named user presets, New Preset (when present/active), Save as….
        /// Shared by Options (C3) and the Debug panel (C4).
        /// </summary>
        public static string[] GetFeelPresetDropdownItems(ModSettings settings)
        {
            var items = new List<string>(8);
            items.Add(FeelProfiles.NameSlow);
            items.Add(FeelProfiles.NameDefault);
            items.Add(FeelProfiles.NameFast);

            bool hasNewPresetSlot = false;
            string[] named = ListNamedFeelPresetNames();
            if (named != null)
            {
                for (int i = 0; i < named.Length; i++)
                {
                    string name = named[i];
                    if (string.IsNullOrEmpty(name) || FeelProfiles.IsBuiltInName(name))
                    {
                        continue;
                    }

                    if (string.Equals(name, FeelProfiles.NameNewPreset, StringComparison.Ordinal))
                    {
                        hasNewPresetSlot = true;
                        continue;
                    }

                    items.Add(name);
                }
            }

            bool newPresetActive =
                settings != null
                && string.Equals(
                    settings.ActiveFeelPresetName,
                    FeelProfiles.NameNewPreset,
                    StringComparison.Ordinal
                );
            if (hasNewPresetSlot || newPresetActive)
            {
                items.Add(FeelProfiles.NameNewPreset);
            }

            items.Add(FeelPresetSaveAsLabel);
            return items.ToArray();
        }

        /// <summary>Index of <paramref name="activeName"/> in dropdown items; defaults to Default.</summary>
        public static int IndexOfFeelPresetDropdownItem(string[] items, string activeName)
        {
            int defaultIndex = 1;
            if (items == null || items.Length == 0)
            {
                return 0;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (string.Equals(items[i], FeelProfiles.NameDefault, StringComparison.Ordinal))
                {
                    defaultIndex = i;
                }
            }

            if (string.IsNullOrEmpty(activeName))
            {
                return defaultIndex;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (string.Equals(items[i], FeelPresetSaveAsLabel, StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(items[i], activeName, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return defaultIndex;
        }

        /// <summary>
        /// Apply a feel-preset dropdown selection (not Save as…). Built-ins use ApplyFeel*; others load by name.
        /// </summary>
        public static void ApplyFeelPresetDropdownChoice(ModSettings settings, string label)
        {
            if (settings == null || string.IsNullOrEmpty(label))
            {
                return;
            }

            if (string.Equals(label, FeelPresetSaveAsLabel, StringComparison.Ordinal))
            {
                return;
            }

            if (string.Equals(label, FeelProfiles.NameSlow, StringComparison.Ordinal))
            {
                ApplyFeelSlow(settings);
                return;
            }

            if (string.Equals(label, FeelProfiles.NameDefault, StringComparison.Ordinal))
            {
                ApplyFeelDefault(settings);
                return;
            }

            if (string.Equals(label, FeelProfiles.NameFast, StringComparison.Ordinal))
            {
                ApplyFeelFast(settings);
                return;
            }

            LoadNamedFeelPreset(settings, label);
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
