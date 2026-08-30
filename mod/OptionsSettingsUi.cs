#if HAS_CITIES
using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Builds the Options page: General → Zoom → Pan → Rotate → Orbit.
    /// ColossalUI / UIHelperBase limits (best-effort):
    /// - No real horizontal rule or indent — <see cref="UIHelperBase.AddSpace"/> then
    ///   <see cref="UIHelperBase.AddGroup"/> approximates “HR → section title → rows”.
    /// - Sensitivity uses <see cref="UIHelperBase.AddSlider"/> (0.1×–2× factory, step ≈ 10%).
    /// - Feel presets use a dropdown; Save as… is the last entry plus a name text field
    ///   (dropdown cannot collect a new name alone).
    /// - Options controls bind to live <see cref="ModSettings"/> at build time; Apply*
    ///   already raises <see cref="ModOptions.SettingsChanged"/> (C2). Full control rebuild
    ///   on that event is not practical under UIHelperBase.
    /// Gated controls use compile-time ENABLE_* symbols (see FeatureFlags / csproj).
    /// </summary>
    internal static class OptionsSettingsUi
    {
        public static void Build(UIHelperBase helper, ModSettings s)
        {
            if (helper == null || s == null)
            {
                return;
            }

            ModSettings factory = ModSettings.CreateFactoryDefaults();

            // Title group: mod name + version (also on IUserMod.Name for the Options tab).
            helper.AddGroup(Mod.OptionsTitle);

            BuildGeneralSection(helper, s);

#if ENABLE_CAD_GESTURE_STYLE
            SectionBreak(helper, "Gesture style");
            helper.AddDropdown(
                "Style",
                ModOptions.GesturePresetLabels,
                ModOptions.GesturePresetToIndex(s.GesturePreset),
                sel => ModOptions.ApplyGesturePresetIndex(s, sel)
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            SectionBreak(helper, "Capture");
            helper.AddDropdown(
                "Interpreter",
                ModOptions.CaptureBackendLabels,
                ModOptions.CaptureBackendToIndex(s.CaptureBackend),
                sel => ModOptions.ApplyCaptureBackendIndex(s, sel)
            );
#endif

            // Product order: General → Zoom → Pan → Rotate → Orbit.
            BuildOpGroup1Axis(
                helper,
                ModOptions.OpHeadingZoom,
                "Sensitivity",
                s.ZoomGain,
                factory.ZoomGain,
                ModOptions.ApplyZoomGain,
                "Button step",
                s.ZoomStep,
                ModOptions.ApplyZoomStep,
                s.ZoomFilterEnabled,
                v => ModOptions.ApplyBool(s, x => x.ZoomFilterEnabled = v),
                s.ZoomFilterAlpha,
                ModOptions.ApplyZoomFilterAlpha
            );

            BuildOpGroup(
                helper,
                ModOptions.OpHeadingPan,
                "Sensitivity X",
                s.PanGainX,
                factory.PanGainX,
                ModOptions.ApplyPanGainX,
                "Sensitivity Y",
                s.PanGainY,
                factory.PanGainY,
                ModOptions.ApplyPanGainY,
                "Button step X",
                s.PanStepX,
                ModOptions.ApplyPanStepX,
                "Button step Y",
                s.PanStepY,
                ModOptions.ApplyPanStepY,
                s.PanFilterEnabled,
                v => ModOptions.ApplyBool(s, x => x.PanFilterEnabled = v),
                s.PanFilterAlpha,
                ModOptions.ApplyPanFilterAlpha
            );

            BuildOpGroup1Axis(
                helper,
                ModOptions.OpHeadingRotate,
                "Sensitivity",
                s.YawRotateGain,
                factory.YawRotateGain,
                ModOptions.ApplyYawRotateGain,
                "Button step",
                s.YawRotateStep,
                ModOptions.ApplyYawRotateStep,
                s.YawFilterEnabled,
                v => ModOptions.ApplyBool(s, x => x.YawFilterEnabled = v),
                s.YawFilterAlpha,
                ModOptions.ApplyYawFilterAlpha
            );

            BuildOpGroup(
                helper,
                ModOptions.OpHeadingOrbit,
                "Sensitivity yaw",
                s.OrbitYawGain,
                factory.OrbitYawGain,
                ModOptions.ApplyOrbitYawGain,
                "Sensitivity pitch",
                s.OrbitPitchGain,
                factory.OrbitPitchGain,
                ModOptions.ApplyOrbitPitchGain,
                "Button step yaw",
                s.OrbitYawStep,
                ModOptions.ApplyOrbitYawStep,
                "Button step pitch",
                s.OrbitPitchStep,
                ModOptions.ApplyOrbitPitchStep,
                s.OrbitFilterEnabled,
                v => ModOptions.ApplyBool(s, x => x.OrbitFilterEnabled = v),
                s.OrbitFilterAlpha,
                ModOptions.ApplyOrbitFilterAlpha
            );
        }

        private static void BuildGeneralSection(UIHelperBase helper, ModSettings s)
        {
            SectionBreak(helper, "General");

            // Schema field remains AssistUiEnabled; product label is Debug.
            helper.AddCheckbox(
                "Show debug panel",
                s.AssistUiEnabled,
                v =>
                    ModOptions.ApplyBool(
                        s,
                        x =>
                        {
                            x.AssistUiEnabled = v;
                            if (v)
                            {
                                TuningPanelHost.ClearUserDismiss();
                            }

                            TuningPanelHost.ApplyVisibility();
                        }
                    )
            );

            string[] presetLabels = ModOptions.GetFeelPresetDropdownItems(s);
            string[] saveAsName = new string[] { "" };

            helper.AddDropdown(
                "Feel preset",
                presetLabels,
                ModOptions.IndexOfFeelPresetDropdownItem(presetLabels, s.ActiveFeelPresetName),
                sel =>
                {
                    if (sel < 0 || sel >= presetLabels.Length)
                    {
                        return;
                    }

                    string label = presetLabels[sel];
                    if (
                        string.Equals(
                            label,
                            ModOptions.FeelPresetSaveAsLabel,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        ModOptions.SaveNamedFeelPreset(s, saveAsName[0]);
                        return;
                    }

                    ModOptions.ApplyFeelPresetDropdownChoice(s, label);
                }
            );

            // Name field for Save as… (dropdown last entry cannot collect a new name alone).
            helper.AddTextfield(
                "Preset name",
                "",
                text =>
                {
                    saveAsName[0] = text ?? "";
                },
                text =>
                {
                    saveAsName[0] = text ?? "";
                }
            );
            helper.AddButton(
                "Save as…",
                () =>
                {
                    ModOptions.SaveNamedFeelPreset(s, saveAsName[0]);
                }
            );
        }

        /// <summary>
        /// Best-effort section rhythm: spacing (stand-in for HR) then AddGroup title.
        /// UIHelperBase has no indent / rule APIs.
        /// </summary>
        private static void SectionBreak(UIHelperBase helper, string title)
        {
            helper.AddSpace(12);
            helper.AddGroup(title);
        }

        private static void BuildOpGroup1Axis(
            UIHelperBase helper,
            string title,
            string sensitivityLabel,
            float sensitivityValue,
            float factorySensitivity,
            Action<ModSettings, float> onSensitivity,
            string buttonLabel,
            float buttonValue,
            Action<ModSettings, float> onButton,
            bool lpEnabled,
            OnCheckChanged onLp,
            float lpAlpha,
            Action<ModSettings, float> onLpAlpha
        )
        {
            ModSettings s = Mod.EnsureSettings();
            SectionBreak(helper, title);
            AddSensitivityControl(
                helper,
                sensitivityLabel,
                sensitivityValue,
                factorySensitivity,
                v => onSensitivity(s, v)
            );
#if ENABLE_ASSIST_CHROME
            AddFloatField(
                helper,
                buttonLabel,
                buttonValue,
                text =>
                {
                    ModOptions.TryApplyFloat(s, text, onButton);
                }
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            helper.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(
                helper,
                "Low-pass alpha",
                lpAlpha,
                text =>
                {
                    ModOptions.TryApplyFloat(s, text, onLpAlpha);
                }
            );
#endif
        }

        private static void BuildOpGroup(
            UIHelperBase helper,
            string title,
            string sensitivityALabel,
            float sensitivityA,
            float factoryA,
            Action<ModSettings, float> onSensitivityA,
            string sensitivityBLabel,
            float sensitivityB,
            float factoryB,
            Action<ModSettings, float> onSensitivityB,
            string buttonALabel,
            float buttonA,
            Action<ModSettings, float> onButtonA,
            string buttonBLabel,
            float buttonB,
            Action<ModSettings, float> onButtonB,
            bool lpEnabled,
            OnCheckChanged onLp,
            float lpAlpha,
            Action<ModSettings, float> onLpAlpha
        )
        {
            ModSettings s = Mod.EnsureSettings();
            SectionBreak(helper, title);
            AddSensitivityControl(
                helper,
                sensitivityALabel,
                sensitivityA,
                factoryA,
                v => onSensitivityA(s, v)
            );
            AddSensitivityControl(
                helper,
                sensitivityBLabel,
                sensitivityB,
                factoryB,
                v => onSensitivityB(s, v)
            );
#if ENABLE_ASSIST_CHROME
            AddFloatField(
                helper,
                buttonALabel,
                buttonA,
                text => ModOptions.TryApplyFloat(s, text, onButtonA)
            );
            AddFloatField(
                helper,
                buttonBLabel,
                buttonB,
                text => ModOptions.TryApplyFloat(s, text, onButtonB)
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            helper.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(
                helper,
                "Low-pass alpha",
                lpAlpha,
                text => ModOptions.TryApplyFloat(s, text, onLpAlpha)
            );
#endif
        }

        /// <summary>
        /// Sensitivity via AddSlider when available; value clamped to 0.1×–2× factory default.
        /// </summary>
        private static void AddSensitivityControl(
            UIHelperBase helper,
            string label,
            float value,
            float factoryDefault,
            OnValueChanged onChanged
        )
        {
            float min = ModOptions.SensitivitySliderMin(factoryDefault);
            float max = ModOptions.SensitivitySliderMax(factoryDefault);
            float step = ModOptions.SensitivitySliderStep(factoryDefault);
            float clamped = ModOptions.ClampGainToFactoryRange(value, factoryDefault);

            helper.AddSlider(
                label,
                min,
                max,
                step,
                clamped,
                v =>
                {
                    float next = ModOptions.ClampGainToFactoryRange(v, factoryDefault);
                    onChanged(next);
                }
            );
        }

        private static void AddFloatField(
            UIHelperBase helper,
            string label,
            float value,
            OnTextSubmitted onSubmit
        )
        {
            object created = helper.AddTextfield(
                label,
                ModOptions.FormatFloat(value),
                _ => { },
                onSubmit
            );
            UITextField field = created as UITextField;
            if (field != null)
            {
                field.submitOnFocusLost = true;
                field.selectOnFocus = true;
                field.allowFloats = true;
                field.numericalOnly = false;
            }
        }
    }
}
#endif
