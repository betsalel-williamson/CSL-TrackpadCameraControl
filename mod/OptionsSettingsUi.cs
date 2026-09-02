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
    /// - Sections use nested <see cref="UIHelperBase.AddGroup"/> (short title + native glow);
    ///   long <see cref="ModOptions.OpDescription*"/> strings are small labels inside group Content.
    /// - Sensitivity uses <see cref="UIHelperBase.AddSlider"/> (0.1×–2× factory, step ≈ 10%).
    /// - Feel presets use a dropdown; Save as… is the last entry plus a name text field
    ///   (dropdown cannot collect a new name alone).
    /// - Options controls bind to live <see cref="ModSettings"/> at build time only; Apply*
    ///   raises <see cref="ModOptions.SettingsChanged"/> for Debug rebuild (C2). Reopen Options
    ///   to refresh sliders after Debug edits — in-place rebuild is not practical under UIHelperBase.
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

            BuildGeneralSection(helper, s);

#if ENABLE_CAD_GESTURE_STYLE
            UIHelperBase gestureGroup = SectionBreak(helper, "Gesture style");
            gestureGroup.AddDropdown(
                "Style",
                ModOptions.GesturePresetLabels,
                ModOptions.GesturePresetToIndex(s.GesturePreset),
                sel => ModOptions.ApplyGesturePresetIndex(s, sel)
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            UIHelperBase captureGroup = SectionBreak(helper, "Capture");
            captureGroup.AddDropdown(
                "Interpreter",
                ModOptions.CaptureBackendLabels,
                ModOptions.CaptureBackendToIndex(s.CaptureBackend),
                sel => ModOptions.ApplyCaptureBackendIndex(s, sel)
            );
#endif

            // Product order: General → Zoom → Pan → Rotate → Orbit.
            BuildOpGroup1Axis(
                helper,
                "Zoom",
                ModOptions.OpDescriptionZoom,
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
                "Pan",
                ModOptions.OpDescriptionPan,
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
                "Rotate",
                ModOptions.OpDescriptionRotate,
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
                "Orbit",
                ModOptions.OpDescriptionOrbit,
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

            AttachOpDescriptionRefresher(helper);
        }

        private static UIComponent _opDescriptionRoot;

        private static void AttachOpDescriptionRefresher(UIHelperBase helper)
        {
            UIHelper ui = helper as UIHelper;
            UIComponent root = ui != null ? ui.self as UIComponent : null;
            if (root == null)
            {
                return;
            }

            DetachOpDescriptionRefresher();
            _opDescriptionRoot = root;
            VanillaCameraKeyLabelsWatch.LabelsChanged += RefreshOpDescriptions;
        }

        private static void DetachOpDescriptionRefresher()
        {
            VanillaCameraKeyLabelsWatch.LabelsChanged -= RefreshOpDescriptions;
            _opDescriptionRoot = null;
        }

        private static void RefreshOpDescriptions()
        {
            if (_opDescriptionRoot == null)
            {
                return;
            }

            try
            {
                if (_opDescriptionRoot.parent == null)
                {
                    DetachOpDescriptionRefresher();
                    return;
                }
            }
            catch
            {
                DetachOpDescriptionRefresher();
                return;
            }

            SetOpDescriptionLabel(
                _opDescriptionRoot,
                "OpHeadingZoom",
                ModOptions.OpDescriptionZoom
            );
            SetOpDescriptionLabel(_opDescriptionRoot, "OpHeadingPan", ModOptions.OpDescriptionPan);
            SetOpDescriptionLabel(
                _opDescriptionRoot,
                "OpHeadingRotate",
                ModOptions.OpDescriptionRotate
            );
            SetOpDescriptionLabel(
                _opDescriptionRoot,
                "OpHeadingOrbit",
                ModOptions.OpDescriptionOrbit
            );
        }

        private static void SetOpDescriptionLabel(UIComponent root, string name, string text)
        {
            UILabel label = FindRecursive(root, name) as UILabel;
            if (label == null || label.text == text)
            {
                return;
            }

            label.text = text;
            label.PerformLayout();
        }

        private static UIComponent FindRecursive(UIComponent parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == name)
            {
                return parent;
            }

            foreach (UIComponent child in parent.components)
            {
                UIComponent hit = FindRecursive(child, name);
                if (hit != null)
                {
                    return hit;
                }
            }

            return null;
        }

        private static void BuildGeneralSection(UIHelperBase helper, ModSettings s)
        {
            UIHelperBase group = SectionBreak(helper, "General");

            // Schema field remains AssistUiEnabled; product label is Debug.
            group.AddCheckbox(
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
                                // Create if missing (e.g. after auto-reload Destroy with checkbox already on).
                                TuningPanelHost.EnsureCreated();
                            }

                            TuningPanelHost.ApplyVisibility();
                        }
                    )
            );

            string[] presetLabels = ModOptions.GetFeelPresetDropdownItems(s);
            string[] saveAsName = new string[] { "" };

            group.AddDropdown(
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
            object presetNameCreated = group.AddTextfield(
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
            UITextField presetNameField = presetNameCreated as UITextField;
            if (presetNameField != null)
            {
                // Match Debug feel name: start-aligned for LTR (Colossal has no RTL Start).
                presetNameField.horizontalAlignment = UIHorizontalAlignment.Left;
            }
            group.AddButton(
                "Save as…",
                () =>
                {
                    ModOptions.SaveNamedFeelPreset(s, saveAsName[0]);
                }
            );
        }

        /// <summary>
        /// Opens a nested Options group with a short title (native glow underline).
        /// Callers must add controls on the returned helper, not the parent.
        /// </summary>
        private static UIHelperBase SectionBreak(UIHelperBase helper, string shortTitle)
        {
            return helper.AddGroup(shortTitle);
        }

        /// <summary>
        /// Places the long OpHeading description as a small label inside group Content
        /// (not as the AddGroup title).
        /// </summary>
        private static void AddGroupDescription(UIHelperBase group, string opId, string text)
        {
            if (group == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            UIHelper ui = group as UIHelper;
            if (ui == null)
            {
                return;
            }

            // UIHelper.self is typed as object in the ICities/Colossal API.
            UIComponent root = ui.self as UIComponent;
            if (root == null)
            {
                return;
            }

            UILabel label = root.AddUIComponent<UILabel>();
            label.name = "OpHeading" + opId;
            label.textScale = 0.85f;
            label.autoSize = true;
            label.text = text;
            label.PerformLayout();
        }

        private static void BuildOpGroup1Axis(
            UIHelperBase helper,
            string shortTitle,
            string description,
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
            UIHelperBase group = SectionBreak(helper, shortTitle);
            AddGroupDescription(group, shortTitle, description);
            AddSensitivityControl(
                group,
                sensitivityLabel,
                sensitivityValue,
                factorySensitivity,
                v => onSensitivity(s, v)
            );
#if ENABLE_ASSIST_CHROME
            AddFloatField(
                group,
                buttonLabel,
                buttonValue,
                text =>
                {
                    ModOptions.TryApplyFloat(s, text, onButton);
                }
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            group.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(
                group,
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
            string shortTitle,
            string description,
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
            UIHelperBase group = SectionBreak(helper, shortTitle);
            AddGroupDescription(group, shortTitle, description);
            AddSensitivityControl(
                group,
                sensitivityALabel,
                sensitivityA,
                factoryA,
                v => onSensitivityA(s, v)
            );
            AddSensitivityControl(
                group,
                sensitivityBLabel,
                sensitivityB,
                factoryB,
                v => onSensitivityB(s, v)
            );
#if ENABLE_ASSIST_CHROME
            AddFloatField(
                group,
                buttonALabel,
                buttonA,
                text => ModOptions.TryApplyFloat(s, text, onButtonA)
            );
            AddFloatField(
                group,
                buttonBLabel,
                buttonB,
                text => ModOptions.TryApplyFloat(s, text, onButtonB)
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            group.AddCheckbox("Low-pass", lpEnabled, onLp);
            AddFloatField(
                group,
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
                NumericTextFieldUi.ConfigureFloatField(field);
            }
        }
    }
}
#endif
