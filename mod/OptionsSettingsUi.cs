#if HAS_CITIES
using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Builds the mirrored Options page (number fields, not sliders).
    /// </summary>
    internal static class OptionsSettingsUi
    {
        public static void Build(UIHelperBase helper, ModSettings s)
        {
            if (helper == null || s == null)
            {
                return;
            }

            helper.AddGroup("Gesture preset");
            helper.AddDropdown(
                "Preset",
                ModOptions.GesturePresetLabels,
                ModOptions.GesturePresetToIndex(s.GesturePreset),
                sel => ModOptions.ApplyGesturePresetIndex(s, sel)
            );
            helper.AddButton(
                "Reset to factory default",
                () =>
                {
                    ModOptions.ResetToFactory(s);
                }
            );

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

            helper.AddGroup("Capture");
            helper.AddDropdown(
                "Interpreter",
                ModOptions.CaptureBackendLabels,
                ModOptions.CaptureBackendToIndex(s.CaptureBackend),
                sel => ModOptions.ApplyCaptureBackendIndex(s, sel)
            );

            BuildOpGroup(
                helper,
                "Pan",
                s.PanEnabled,
                v => ModOptions.ApplyBool(s, x => x.PanEnabled = v),
                s.InvertPanX,
                v => ModOptions.ApplyBool(s, x => x.InvertPanX = v),
                "Reverse X",
                s.InvertPanY,
                v => ModOptions.ApplyBool(s, x => x.InvertPanY = v),
                "Reverse Y",
                "Drag scale X",
                s.PanSensitivityX,
                ModOptions.ApplyPanSensitivityX,
                "Drag scale Y",
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
                "Zoom",
                s.ZoomEnabled,
                v => ModOptions.ApplyBool(s, x => x.ZoomEnabled = v),
                s.InvertZoom,
                v => ModOptions.ApplyBool(s, x => x.InvertZoom = v),
                "Reverse",
                "Drag scale",
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
                "Rotate (yaw)",
                s.YawEnabled,
                v => ModOptions.ApplyBool(s, x => x.YawEnabled = v),
                s.InvertYawRotate,
                v => ModOptions.ApplyBool(s, x => x.InvertYawRotate = v),
                "Reverse",
                "Drag scale",
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
                "Orbit",
                s.OrbitEnabled,
                v => ModOptions.ApplyBool(s, x => x.OrbitEnabled = v),
                s.InvertOrbitYaw,
                v => ModOptions.ApplyBool(s, x => x.InvertOrbitYaw = v),
                "Reverse yaw",
                s.InvertOrbitPitch,
                v => ModOptions.ApplyBool(s, x => x.InvertOrbitPitch = v),
                "Reverse pitch",
                "Drag scale yaw",
                s.OrbitYawSensitivity,
                ModOptions.ApplyOrbitYawSensitivity,
                "Drag scale pitch",
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
        }

        private static void BuildOpGroup1Axis(
            UIHelperBase helper,
            string title,
            bool enabled,
            OnCheckChanged onEnabled,
            bool invert,
            OnCheckChanged onInvert,
            string invertLabel,
            string dragLabel,
            float dragValue,
            Action<ModSettings, float> onDrag,
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
            AddFloatField(helper, dragLabel, dragValue, text =>
            {
                if (!ModOptions.TryApplyFloat(s, text, onDrag))
                {
                    // leave prior value
                }
            });
            AddFloatField(helper, buttonLabel, buttonValue, text =>
            {
                ModOptions.TryApplyFloat(s, text, onButton);
            });
            helper.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(helper, "Low-pass alpha", lpAlpha, text =>
            {
                ModOptions.TryApplyFloat(s, text, onLpAlpha);
            });
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
            string dragALabel,
            float dragA,
            Action<ModSettings, float> onDragA,
            string dragBLabel,
            float dragB,
            Action<ModSettings, float> onDragB,
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
            AddFloatField(helper, dragALabel, dragA, text => ModOptions.TryApplyFloat(s, text, onDragA));
            AddFloatField(helper, dragBLabel, dragB, text => ModOptions.TryApplyFloat(s, text, onDragB));
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
            helper.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(
                helper,
                "Low-pass alpha",
                lpAlpha,
                text => ModOptions.TryApplyFloat(s, text, onLpAlpha)
            );
        }

        private static void AddFloatField(
            UIHelperBase helper,
            string label,
            float value,
            OnTextSubmitted onSubmit
        )
        {
            helper.AddTextfield(
                label,
                ModOptions.FormatFloat(value),
                _ => { },
                onSubmit
            );
        }
    }
}
#endif
