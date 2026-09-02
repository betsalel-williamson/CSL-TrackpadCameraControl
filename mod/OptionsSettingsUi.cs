#if HAS_CITIES
using System;
using System.Collections.Generic;
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
    /// - Sensitivity uses <see cref="UIHelperBase.AddSlider"/> on a fixed [0, 1] UI domain;
    ///   <see cref="ModOptions.GainToSensitivityUi"/> / <see cref="ModOptions.SensitivityUiToGain"/>
    ///   map piecewise to 0.1× / 1× / 2× factory (UI 0.5 = Default / Debug field value).
    /// - Feel presets use a dropdown; Save as… is a button (enabled when dirty) that opens a name dialog.
    /// - Options controls bind to live <see cref="ModSettings"/> at build time only; Apply*
    ///   raises <see cref="ModOptions.SettingsChanged"/> for Debug rebuild (C2). Reopen Options
    ///   to refresh sliders after Debug edits — in-place rebuild is not practical under UIHelperBase.
    /// Gated controls use compile-time ENABLE_* symbols (see FeatureFlags / csproj).
    /// </summary>
    internal static class OptionsSettingsUi
    {
        private static readonly List<Action> SensitivitySliderRefreshes = new List<Action>(8);
        private static Action _saveAsEnabledSync;

        public static void Build(UIHelperBase helper, ModSettings s)
        {
            if (helper == null || s == null)
            {
                return;
            }

            SensitivitySliderRefreshes.Clear();

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
                () => s.ZoomGain,
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
                () => s.PanGainX,
                factory.PanGainX,
                ModOptions.ApplyPanGainX,
                "Sensitivity Y",
                () => s.PanGainY,
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
                () => s.RotateGain,
                factory.RotateGain,
                ModOptions.ApplyRotateGain,
                "Button step",
                s.RotateStep,
                ModOptions.ApplyRotateStep,
                s.RotateFilterEnabled,
                v => ModOptions.ApplyBool(s, x => x.RotateFilterEnabled = v),
                s.RotateFilterAlpha,
                ModOptions.ApplyRotateFilterAlpha
            );

            BuildOpGroup(
                helper,
                "Orbit",
                ModOptions.OpDescriptionOrbit,
                "Sensitivity yaw",
                () => s.OrbitYawGain,
                factory.OrbitYawGain,
                ModOptions.ApplyOrbitYawGain,
                "Sensitivity pitch",
                () => s.OrbitPitchGain,
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
            ModOptions.SettingsChanged += RefreshOpDescriptions;
            ModOptions.SettingsChanged += RefreshAllSensitivitySliders;
            root.eventVisibilityChanged += OnOptionsRootVisibilityChanged;
            RefreshAllSensitivitySliders();
        }

        private static void OnOptionsRootVisibilityChanged(UIComponent component, bool visible)
        {
            if (visible)
            {
                RefreshAllSensitivitySliders();
            }
        }

        private static void RefreshAllSensitivitySliders()
        {
            for (int i = 0; i < SensitivitySliderRefreshes.Count; i++)
            {
                Action refresh = SensitivitySliderRefreshes[i];
                if (refresh != null)
                {
                    refresh();
                }
            }
        }

        private static void DetachOpDescriptionRefresher()
        {
            if (_opDescriptionRoot != null)
            {
                _opDescriptionRoot.eventVisibilityChanged -= OnOptionsRootVisibilityChanged;
            }

            VanillaCameraKeyLabelsWatch.LabelsChanged -= RefreshOpDescriptions;
            ModOptions.SettingsChanged -= RefreshOpDescriptions;
            ModOptions.SettingsChanged -= RefreshAllSensitivitySliders;
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

                    ModOptions.ApplyFeelPresetDropdownChoice(s, presetLabels[sel]);
                }
            );

            object saveAsCreated = group.AddButton(
                "Save as…",
                () =>
                {
                    FeelSaveAsDialog.Show(s, null);
                }
            );
            UIButton saveAsButton = saveAsCreated as UIButton;
            if (saveAsButton != null)
            {
                if (_saveAsEnabledSync != null)
                {
                    ModOptions.SettingsChanged -= _saveAsEnabledSync;
                }

                _saveAsEnabledSync = () =>
                {
                    if (saveAsButton != null)
                    {
                        saveAsButton.isEnabled = ModOptions.IsFeelDirtyNewPreset(Mod.Settings ?? s);
                    }
                };
                saveAsButton.isEnabled = ModOptions.IsFeelDirtyNewPreset(s);
                ModOptions.SettingsChanged += _saveAsEnabledSync;
            }
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
            Func<float> getSensitivity,
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
                getSensitivity,
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
            Func<float> getSensitivityA,
            float factoryA,
            Action<ModSettings, float> onSensitivityA,
            string sensitivityBLabel,
            Func<float> getSensitivityB,
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
                getSensitivityA,
                factoryA,
                v => onSensitivityA(s, v)
            );
            AddSensitivityControl(
                group,
                sensitivityBLabel,
                getSensitivityB,
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
        /// Sensitivity via AddSlider on a fixed [0, 1] UI domain. Gain uses
        /// <see cref="ModOptions.GainToSensitivityUi"/> / <see cref="ModOptions.SensitivityUiToGain"/>
        /// (0.1× / 1× / 2× factory; UI 0.5 = Default). Re-applies on show/size and manually
        /// places the thumb — Colossal often leaves it at min when value is set before layout.
        /// </summary>
        private static void AddSensitivityControl(
            UIHelperBase helper,
            string label,
            Func<float> getValue,
            float factoryDefault,
            OnValueChanged onChanged
        )
        {
            if (getValue == null)
            {
                return;
            }

            float uiValue = ModOptions.GainToSensitivityUi(getValue(), factoryDefault);

            bool suppressCallback = true;
            object created = helper.AddSlider(
                label,
                ModOptions.SensitivityUiMin,
                ModOptions.SensitivityUiMax,
                ModOptions.SensitivityUiStep,
                uiValue,
                v =>
                {
                    if (suppressCallback)
                    {
                        return;
                    }

                    onChanged(ModOptions.SensitivityUiToGain(v, factoryDefault));
                }
            );

            UISlider slider = ResolveSlider(created);
            if (slider != null)
            {
                Action refresh = () =>
                {
                    bool previousSuppress = suppressCallback;
                    suppressCallback = true;
                    try
                    {
                        float ui = ModOptions.GainToSensitivityUi(getValue(), factoryDefault);
                        ForceSliderUi(slider, ui);
                    }
                    finally
                    {
                        suppressCallback = previousSuppress;
                    }
                };

                slider.eventSizeChanged += (c, size) =>
                {
                    if (size.x > 1f)
                    {
                        refresh();
                    }
                };
                slider.eventVisibilityChanged += (c, visible) =>
                {
                    if (visible)
                    {
                        refresh();
                    }
                };
                SensitivitySliderRefreshes.Add(refresh);
                refresh();
            }

            suppressCallback = false;
        }

        private static UISlider ResolveSlider(object created)
        {
            UISlider slider = created as UISlider;
            if (slider != null)
            {
                return slider;
            }

            UIComponent component = created as UIComponent;
            if (component == null)
            {
                return null;
            }

            slider = component.Find<UISlider>("Slider");
            if (slider != null)
            {
                return slider;
            }

            return component.GetComponentInChildren<UISlider>();
        }

        /// <summary>
        /// Set [0, 1] domain + value, then place the thumb explicitly. Colossal's
        /// UpdateValueIndicators often leaves the thumb at min when the track width was 0
        /// at first set, and may not correct it later.
        /// </summary>
        private static void ForceSliderUi(UISlider slider, float ui)
        {
            if (slider == null)
            {
                return;
            }

            if (ui < ModOptions.SensitivityUiMin)
            {
                ui = ModOptions.SensitivityUiMin;
            }

            if (ui > ModOptions.SensitivityUiMax)
            {
                ui = ModOptions.SensitivityUiMax;
            }

            slider.minValue = ModOptions.SensitivityUiMin;
            slider.maxValue = ModOptions.SensitivityUiMax;
            slider.stepSize = ModOptions.SensitivityUiStep;

            float bump =
                ui < ModOptions.SensitivityUiFactory
                    ? ModOptions.SensitivityUiMax
                    : ModOptions.SensitivityUiMin;
            slider.value = bump;
            slider.value = ui;
            PlaceThumb(slider, ui);
        }

        private static void PlaceThumb(UISlider slider, float ui)
        {
            if (slider == null)
            {
                return;
            }

            UIComponent thumb = slider.thumbObject;
            if (thumb == null)
            {
                return;
            }

            float track = slider.width;
            if (track < 8f && slider.parent != null)
            {
                track = Mathf.Max(track, slider.parent.width);
            }

            if (track < 8f)
            {
                return;
            }

            ui = Mathf.Clamp01(ui);
            Vector3 p = thumb.relativePosition;
            float thumbW = Mathf.Max(1f, thumb.width);
            p.x = ui * Mathf.Max(0f, track - thumbW);
            thumb.relativePosition = p;
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
                ModOptions.FormatGain(value),
                _ => { },
                onSubmit
            );
            UITextField field = created as UITextField;
            if (field != null)
            {
                field.submitOnFocusLost = true;
                field.selectOnFocus = true;
                NumericTextFieldUi.ConfigureFloatField(field);
                NumericTextFieldUi.WireConfirmKeys(field);
            }
        }
    }
}
#endif
