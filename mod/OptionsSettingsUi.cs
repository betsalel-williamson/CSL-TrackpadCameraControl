#if HAS_CITIES
using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Builds the mirrored Options page (number fields, not sliders).
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

            BuildFeelPresetRow(helper, s);

            helper.AddGroup("Assist UI");
            helper.AddCheckbox(
                "Show in-game Assist / tuning panel",
                s.AssistUiEnabled,
                v =>
                    ModOptions.ApplyBool(
                        s,
                        x =>
                        {
                            x.AssistUiEnabled = v;
                            TuningPanelHost.ApplyVisibility();
                        }
                    )
            );

#if ENABLE_CAD_GESTURE_STYLE
            helper.AddGroup("Gesture style");
            helper.AddDropdown(
                "Style",
                ModOptions.GesturePresetLabels,
                ModOptions.GesturePresetToIndex(s.GesturePreset),
                sel => ModOptions.ApplyGesturePresetIndex(s, sel)
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            helper.AddGroup("Capture");
            helper.AddDropdown(
                "Interpreter",
                ModOptions.CaptureBackendLabels,
                ModOptions.CaptureBackendToIndex(s.CaptureBackend),
                sel => ModOptions.ApplyCaptureBackendIndex(s, sel)
            );
#endif

            // Per-op groups mirror Assist panel columns (best-effort under ColossalUI helper).
            BuildOpGroup(
                helper,
                ModOptions.OpHeadingPan,
                s.PanEnabled,
                v => ModOptions.ApplyBool(s, x => x.PanEnabled = v),
                s.InvertPanX,
                v => ModOptions.ApplyBool(s, x => x.InvertPanX = v),
                "Reverse X",
                s.InvertPanY,
                v => ModOptions.ApplyBool(s, x => x.InvertPanY = v),
                "Reverse Y",
                "Sensitivity X",
                s.PanSensitivityX,
                ModOptions.ApplyPanSensitivityX,
                "Sensitivity Y",
                s.PanSensitivityY,
                ModOptions.ApplyPanSensitivityY,
                "Button step X",
                s.PanButtonScaleX,
                ModOptions.ApplyPanButtonScaleX,
                "Button step Y",
                s.PanButtonScaleY,
                ModOptions.ApplyPanButtonScaleY,
                s.PanLowPassEnabled,
                v => ModOptions.ApplyBool(s, x => x.PanLowPassEnabled = v),
                s.PanLowPassAlpha,
                ModOptions.ApplyPanLowPassAlpha
            );

            BuildOpGroup1Axis(
                helper,
                ModOptions.OpHeadingZoom,
                s.ZoomEnabled,
                v => ModOptions.ApplyBool(s, x => x.ZoomEnabled = v),
                s.InvertZoom,
                v => ModOptions.ApplyBool(s, x => x.InvertZoom = v),
                "Reverse",
                "Sensitivity",
                s.ZoomSensitivity,
                ModOptions.ApplyZoomSensitivity,
                "Button step",
                s.ZoomButtonScale,
                ModOptions.ApplyZoomButtonScale,
                s.ZoomLowPassEnabled,
                v => ModOptions.ApplyBool(s, x => x.ZoomLowPassEnabled = v),
                s.ZoomLowPassAlpha,
                ModOptions.ApplyZoomLowPassAlpha
            );

            BuildOpGroup1Axis(
                helper,
                ModOptions.OpHeadingRotate,
                s.YawEnabled,
                v => ModOptions.ApplyBool(s, x => x.YawEnabled = v),
                s.InvertYawRotate,
                v => ModOptions.ApplyBool(s, x => x.InvertYawRotate = v),
                "Reverse",
                "Sensitivity",
                s.YawRotateSensitivity,
                ModOptions.ApplyYawRotateSensitivity,
                "Button step",
                s.YawRotateButtonScale,
                ModOptions.ApplyYawRotateButtonScale,
                s.YawLowPassEnabled,
                v => ModOptions.ApplyBool(s, x => x.YawLowPassEnabled = v),
                s.YawLowPassAlpha,
                ModOptions.ApplyYawLowPassAlpha
            );

            BuildOpGroup(
                helper,
                ModOptions.OpHeadingOrbit,
                s.OrbitEnabled,
                v => ModOptions.ApplyBool(s, x => x.OrbitEnabled = v),
                s.InvertOrbitYaw,
                v => ModOptions.ApplyBool(s, x => x.InvertOrbitYaw = v),
                "Reverse yaw",
                s.InvertOrbitPitch,
                v => ModOptions.ApplyBool(s, x => x.InvertOrbitPitch = v),
                "Reverse pitch",
                "Sensitivity yaw",
                s.OrbitYawSensitivity,
                ModOptions.ApplyOrbitYawSensitivity,
                "Sensitivity pitch",
                s.OrbitPitchSensitivity,
                ModOptions.ApplyOrbitPitchSensitivity,
                "Button step yaw",
                s.OrbitYawButtonScale,
                ModOptions.ApplyOrbitYawButtonScale,
                "Button step pitch",
                s.OrbitPitchButtonScale,
                ModOptions.ApplyOrbitPitchButtonScale,
                s.OrbitLowPassEnabled,
                v => ModOptions.ApplyBool(s, x => x.OrbitLowPassEnabled = v),
                s.OrbitLowPassAlpha,
                ModOptions.ApplyOrbitLowPassAlpha
            );

            AddFloatField(
                helper,
                "Pitch min",
                s.OrbitPitchMin,
                text => ModOptions.TryApplyFloat(s, text, ModOptions.ApplyOrbitPitchMin)
            );
            AddFloatField(
                helper,
                "Pitch max",
                s.OrbitPitchMax,
                text => ModOptions.TryApplyFloat(s, text, ModOptions.ApplyOrbitPitchMax)
            );
        }

        private static void BuildFeelPresetRow(UIHelperBase helper, ModSettings s)
        {
            helper.AddGroup("Feel presets");
            helper.AddButton("Slow", () => ModOptions.ApplyFeelSlow(s));
            helper.AddButton("Default", () => ModOptions.ApplyFeelDefault(s));
            helper.AddButton("Fast", () => ModOptions.ApplyFeelFast(s));
            helper.AddButton(
                "Reset to factory",
                () =>
                {
                    ModOptions.ResetToFactory(s);
                }
            );

            // Simple name field for Save as… / Load (no fancy dialogs under ColossalUI).
            string[] nameBox = new string[] { "" };
            helper.AddTextfield(
                "Preset name",
                "",
                text =>
                {
                    nameBox[0] = text ?? "";
                },
                text =>
                {
                    nameBox[0] = text ?? "";
                }
            );
            helper.AddButton(
                "Save as…",
                () =>
                {
                    ModOptions.SaveNamedFeelPreset(s, nameBox[0]);
                }
            );
            helper.AddButton(
                "Load",
                () =>
                {
                    ModOptions.LoadNamedFeelPreset(s, nameBox[0]);
                }
            );
        }

        private static void BuildOpGroup1Axis(
            UIHelperBase helper,
            string title,
            bool enabled,
            OnCheckChanged onEnabled,
            bool invert,
            OnCheckChanged onInvert,
            string invertLabel,
            string sensitivityLabel,
            float sensitivityValue,
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
            helper.AddGroup(title);
            helper.AddCheckbox("Enable", enabled, onEnabled);
            helper.AddCheckbox(invertLabel, invert, onInvert);
            AddFloatField(helper, sensitivityLabel, sensitivityValue, text =>
            {
                if (!ModOptions.TryApplyFloat(s, text, onSensitivity))
                {
                    // leave prior value
                }
            });
#if ENABLE_ASSIST_CHROME
            AddFloatField(helper, buttonLabel, buttonValue, text =>
            {
                ModOptions.TryApplyFloat(s, text, onButton);
            });
#endif

#if ENABLE_CONTACTS_CAPTURE
            helper.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(helper, "Low-pass alpha", lpAlpha, text =>
            {
                ModOptions.TryApplyFloat(s, text, onLpAlpha);
            });
#endif
        }

        private static void BuildOpGroup(
            UIHelperBase helper,
            string title,
            bool enabled,
            OnCheckChanged onEnabled,
            bool invertA,
            OnCheckChanged onInvertA,
            string invertALabel,
            bool invertB,
            OnCheckChanged onInvertB,
            string invertBLabel,
            string sensitivityALabel,
            float sensitivityA,
            Action<ModSettings, float> onSensitivityA,
            string sensitivityBLabel,
            float sensitivityB,
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
            helper.AddGroup(title);
            helper.AddCheckbox("Enable", enabled, onEnabled);
            helper.AddCheckbox(invertALabel, invertA, onInvertA);
            helper.AddCheckbox(invertBLabel, invertB, onInvertB);
            AddFloatField(
                helper,
                sensitivityALabel,
                sensitivityA,
                text => ModOptions.TryApplyFloat(s, text, onSensitivityA)
            );
            AddFloatField(
                helper,
                sensitivityBLabel,
                sensitivityB,
                text => ModOptions.TryApplyFloat(s, text, onSensitivityB)
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
