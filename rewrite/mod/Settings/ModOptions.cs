using System;
using System.Collections.Generic;
using System.Globalization;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Shared Options / in-game panel bindings. Cities UI calls these; tests cover them without ICities.
    /// </summary>
    public static class ModOptions
    {
        public const float ScaleMin = 0f;
        public const float ScaleMax = 100f;

        /// <summary>Legacy alias for tests and older call sites.</summary>
        /// <summary>Legacy alias for tests and older call sites.</summary>
        public const float SensitivityStep = 0.05f;

        /// <summary>Product Sensitivity slider floor as a fraction of factory default (0.1×).</summary>
        public const float SensitivitySliderMinFactor = 0.1f;

        /// <summary>Product Sensitivity slider ceiling as a fraction of factory default (2×).</summary>
        public const float SensitivitySliderMaxFactor = 2f;

        /// <summary>Product Sensitivity slider step as a fraction of factory default (~10%).</summary>
        public const float SensitivitySliderStepFactor = 0.1f;

        /// <summary>
        /// Options UI slider domain floor. Colossal thumbs/stepping stay simple on [0, 1];
        /// convert to/from gain with <see cref="GainToSensitivityUi"/> / <see cref="SensitivityUiToGain"/>.
        /// </summary>
        public const float SensitivityUiMin = 0f;

        /// <summary>Options UI slider domain ceiling (factory Default maps to mid = 0.5).</summary>
        public const float SensitivityUiMax = 1f;

        /// <summary>UI position where gain equals the factory default.</summary>
        public const float SensitivityUiFactory = 0.5f;

        /// <summary>
        /// Options UI slider step in [0, 1]. Ten notches from mid→max ≈ +10% of factory per notch
        /// on the high side (1×→2×); low side is piecewise 1×→0.1×.
        /// </summary>
        public const float SensitivityUiStep = 0.05f;

        public static readonly string[] GesturePresetLabels = new string[]
        {
            "Maps+ — map-app pan/pinch/yaw; Option (⌥)+two-finger orbit",
            "CAD — same pan/pinch/yaw; three-finger orbit",
        };

        public static readonly string MapsPlusDescription =
            "Two-finger pan, pinch zoom, two-finger rotate yaw, Option (⌥)+two-finger orbit. Lower conflict with OS three-finger gestures.";

        public static readonly string CadDescription =
            "Same pan/pinch/yaw as Maps+. Three-finger drag orbits (may fight OS Mission Control / Spaces).";

        /// <summary>Pan section body copy for Options (group title is separate).</summary>
        public static string OpDescriptionPan => VanillaCameraKeyLabels.OpDescriptionPan;

        /// <summary>Zoom section body copy for Options (group title is separate).</summary>
        public static string OpDescriptionZoom => VanillaCameraKeyLabels.OpDescriptionZoom;

        /// <summary>Rotate section body copy for Options (group title is separate).</summary>
        public static string OpDescriptionRotate => VanillaCameraKeyLabels.OpDescriptionRotate;

        /// <summary>Orbit section body copy for Options (group title is separate).</summary>
        public static string OpDescriptionOrbit => VanillaCameraKeyLabels.OpDescriptionOrbit;

        /// <summary>Pan section heading for Debug panel (includes title line).</summary>
        public static string OpHeadingPan => VanillaCameraKeyLabels.OpHeadingPan;

        /// <summary>Zoom section heading for Debug panel (includes title line).</summary>
        public static string OpHeadingZoom => VanillaCameraKeyLabels.OpHeadingZoom;

        /// <summary>Rotate section heading for Debug panel (includes title line).</summary>
        public static string OpHeadingRotate => VanillaCameraKeyLabels.OpHeadingRotate;

        /// <summary>Orbit section heading for Debug panel (includes title line).</summary>
        public static string OpHeadingOrbit => VanillaCameraKeyLabels.OpHeadingOrbit;

        public static ModSettingsStore Store { get; set; }

        /// <summary>
        /// Raised after settings apply + force flush so Options / Debug panel can refresh.
        /// </summary>
        public static event Action SettingsChanged;

        /// <summary>Drop all <see cref="SettingsChanged"/> handlers (mod disable).</summary>
        internal static void ResetSettingsChangedHandlers()
        {
            SettingsChanged = null;
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

            settings.ApplyGesturePreset(IndexToGesturePreset(index));
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

            return RoundGain(value);
        }

        /// <summary>
        /// Sensitivity numeric policy: round to three decimals (supports pan 0.005 after
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

        /// <summary>Options Sensitivity slider step for a factory default (~10% of factory, gain units).</summary>
        public static float SensitivitySliderStep(float factoryDefault)
        {
            float step = RoundGain(factoryDefault * SensitivitySliderStepFactor);
            return step > 0f ? step : 0.0001f;
        }

        /// <summary>
        /// Gain → Options UI [0, 1]. Piecewise linear in multiplier space so factory maps to
        /// <see cref="SensitivityUiFactory"/> (0.5): UI 0 → 0.1×, UI 0.5 → 1×, UI 1 → 2×.
        /// A single linear map over [0.1×, 2×] would put mid at 1.05×, not Default.
        /// </summary>
        public static float GainToSensitivityUi(float gain, float factoryDefault)
        {
            float factory = RoundGain(factoryDefault);
            if (factory <= 0f)
            {
                return SensitivityUiMin;
            }

            float min = SensitivitySliderMin(factory);
            float max = SensitivitySliderMax(factory);
            float g = RoundGain(gain);

            if (g <= min)
            {
                return SensitivityUiMin;
            }

            if (g >= max)
            {
                return SensitivityUiMax;
            }

            if (g <= factory)
            {
                float loSpan = factory - min;
                if (loSpan < 0.0001f)
                {
                    return SensitivityUiFactory;
                }

                return SensitivityUiFactory * ((g - min) / loSpan);
            }

            float hiSpan = max - factory;
            if (hiSpan < 0.0001f)
            {
                return SensitivityUiFactory;
            }

            return SensitivityUiFactory
                + (SensitivityUiMax - SensitivityUiFactory) * ((g - factory) / hiSpan);
        }

        /// <summary>
        /// Options UI [0, 1] → gain. Inverse of <see cref="GainToSensitivityUi"/> (0.1× / 1× / 2×).
        /// </summary>
        public static float SensitivityUiToGain(float ui, float factoryDefault)
        {
            if (ui < SensitivityUiMin)
            {
                ui = SensitivityUiMin;
            }

            if (ui > SensitivityUiMax)
            {
                ui = SensitivityUiMax;
            }

            float factory = RoundGain(factoryDefault);
            float min = SensitivitySliderMin(factory);
            float max = SensitivitySliderMax(factory);

            float gain;
            if (ui <= SensitivityUiFactory)
            {
                float t = SensitivityUiFactory > 0f ? ui / SensitivityUiFactory : 0f;
                gain = min + t * (factory - min);
            }
            else
            {
                float t = (ui - SensitivityUiFactory) / (SensitivityUiMax - SensitivityUiFactory);
                gain = factory + t * (max - factory);
            }

            return ClampGainToFactoryRange(gain, factory);
        }

        /// <summary>
        /// Clamp a Sensitivity edit to the product Options slider range for that factory default.
        /// Out-of-range values snap into [0.1×, 2×] factory; gains round to three decimals.
        /// </summary>
        public static float ClampGainToFactoryRange(float value, float factoryDefault)
        {
            float min = SensitivitySliderMin(factoryDefault);
            float max = SensitivitySliderMax(factoryDefault);

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

        /// <summary>Three-decimal round for all product numeric apply/display (gain, step, pitch, deadband).</summary>
        public static float RoundGain(float value)
        {
            return (float)Math.Round(value, 3, MidpointRounding.AwayFromZero);
        }

        public static bool TryParseFloat(string text, out float value)
        {
            return NumericFieldInput.TryParseFloatText(text, out value);
        }

        public static string FormatGain(float value)
        {
            return RoundGain(value).ToString("0.000", CultureInfo.InvariantCulture);
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

        public static void ApplyZoomGain(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.ZoomGain = v);
        }

        public static void ApplyRotateGain(ModSettings settings, float value)
        {
            ApplyPositiveGain(settings, value, (s, v) => s.RotateGain = v);
        }

        public static void ApplyMotionDeadband(ModSettings settings, float value)
        {
            ApplyNonNegativeThreshold(settings, value, (s, v) => s.MotionDeadband = v);
        }

        public static void ApplyPinchDeadband(ModSettings settings, float value)
        {
            ApplyNonNegativeThreshold(settings, value, (s, v) => s.PinchDeadband = v);
        }

        public static void ApplyRotateDeadband(ModSettings settings, float value)
        {
            ApplyNonNegativeThreshold(settings, value, (s, v) => s.RotateDeadband = v);
        }

        private static void ApplyNonNegativeThreshold(
            ModSettings settings,
            float value,
            Action<ModSettings, float> assign
        )
        {
            if (settings == null || assign == null || value < 0f)
            {
                return;
            }

            assign(settings, RoundGain(value));
            AfterFeelFieldChanged(settings);
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

        public static void ApplyRotateStep(ModSettings settings, float value)
        {
            if (settings == null)
            {
                return;
            }

            settings.RotateStep = ClampScale(value);
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

            float panelX = settings.DebugPanelPosX;
            float panelY = settings.DebugPanelPosY;
            settings.CopyFrom(ModSettings.CreateFactoryDefaults());
            settings.DebugPanelPosX = panelX;
            settings.DebugPanelPosY = panelY;
            settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
            if (Store != null)
            {
                Store.SaveNow(settings);
            }

            RaiseSettingsChanged();
        }

        /// <summary>Persist Debug panel position without rebuilding the UI.</summary>
        public static void ApplyPanelPosition(ModSettings settings, float x, float y)
        {
            if (settings == null)
            {
                return;
            }

            settings.DebugPanelPosX = x;
            settings.DebugPanelPosY = y;
            if (Store != null)
            {
                Store.MarkDirty();
                Store.FlushIfNeeded(settings, true);
            }
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

            if (string.Equals(name, FeelProfiles.NameNewPreset, StringComparison.Ordinal))
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

        /// <summary>
        /// True when the active feel identity is a named user preset — not Slow/Default/Fast
        /// and not the New Preset scratch slot.
        /// </summary>
        public static bool IsNamedUserFeelPreset(ModSettings settings)
        {
            return settings != null
                && !string.IsNullOrEmpty(settings.ActiveFeelPresetName)
                && !FeelProfiles.IsBuiltInName(settings.ActiveFeelPresetName)
                && !string.Equals(
                    settings.ActiveFeelPresetName,
                    FeelProfiles.NameNewPreset,
                    StringComparison.Ordinal
                );
        }

        /// <summary>
        /// Delete the active named user feel preset, persist, and apply Default.
        /// Built-ins and New Preset cannot be deleted.
        /// </summary>
        public static bool DeleteNamedFeelPreset(ModSettings settings)
        {
            if (settings == null || Store == null || !IsNamedUserFeelPreset(settings))
            {
                return false;
            }

            Store.RemoveUserPreset(settings.ActiveFeelPresetName);
            FeelProfiles.ApplyDefault(settings);
            settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
            Store.SaveNow(settings);
            RaiseSettingsChanged();
            return true;
        }

        /// <summary>
        /// Suggested Save as… name: overwrite the active named preset, otherwise the next
        /// unused <c>New Preset N</c> (N starts at 1).
        /// </summary>
        public static string SuggestFeelSaveAsName(ModSettings settings)
        {
            if (IsNamedUserFeelPreset(settings))
            {
                return settings.ActiveFeelPresetName;
            }

            return NextNumberedNewPresetName();
        }

        /// <summary>Next unused name in the series New Preset 1, New Preset 2, …</summary>
        public static string NextNumberedNewPresetName()
        {
            string prefix = FeelProfiles.NameNewPreset + " ";
            int max = 0;
            string[] named = ListNamedFeelPresetNames();
            if (named != null)
            {
                for (int i = 0; i < named.Length; i++)
                {
                    string name = named[i];
                    if (
                        string.IsNullOrEmpty(name)
                        || !name.StartsWith(prefix, StringComparison.Ordinal)
                    )
                    {
                        continue;
                    }

                    string rest = name.Substring(prefix.Length);
                    int n;
                    if (
                        int.TryParse(
                            rest,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out n
                        )
                        && n > max
                    )
                    {
                        max = n;
                    }
                }
            }

            return prefix + (max + 1).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>True when the active feel identity is the dirty New Preset scratch slot.</summary>
        public static bool IsFeelDirtyNewPreset(ModSettings settings)
        {
            return settings != null
                && string.Equals(
                    settings.ActiveFeelPresetName,
                    FeelProfiles.NameNewPreset,
                    StringComparison.Ordinal
                );
        }

        /// <summary>Last entry label retained for tests / migration; no longer in the dropdown.</summary>
        public const string FeelPresetSaveAsLabel = "Save as…";

        /// <summary>
        /// Dropdown items: Slow, Default, Fast, named user presets, New Preset (when present/active).
        /// Shared by Options and the Debug panel. Save as… is a separate button + dialog.
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

            bool newPresetActive = IsFeelDirtyNewPreset(settings);
            if (hasNewPresetSlot || newPresetActive)
            {
                items.Add(FeelProfiles.NameNewPreset);
            }

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
