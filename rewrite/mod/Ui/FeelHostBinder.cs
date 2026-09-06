using System;
using System.Collections.Generic;
#if HAS_CITIES
using ColossalFramework.UI;
using ICities;
using UnityEngine;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Shared catalog → toolkit binding for Options and Debug hosts.
    /// Hosts supply chrome; this owns FeelEditor write paths per field id.
    /// </summary>
    public static class FeelHostBinder
    {
        public static IList<FeelPanelEntry> BuildPanelModel(FeelEditor editor)
        {
            return BuildPanelModel(editor, FeelHostKind.Debug);
        }

        public static IList<FeelPanelEntry> BuildPanelModel(FeelEditor editor, FeelHostKind host)
        {
            var list = new List<FeelPanelEntry>();
            if (editor == null || editor.Settings == null)
            {
                return list;
            }

            ModSettings settings = editor.Settings;
            ModSettings factory = ModSettings.CreateFactoryDefaults();
            foreach (FeelControlDescriptor d in OptionsHost.BuildDescriptors(host))
            {
                FeelControlKind toolkit = FeelHostMapping.MapKind(d.Kind);
                var entry = new FeelPanelEntry
                {
                    Section = d.Section,
                    Id = d.Id,
                    Label = d.Label,
                    CatalogKind = d.Kind,
                    ToolkitKind = toolkit,
                };

                switch (d.Id)
                {
                    case "feelPreset":
                        entry.ValueKind = "dropdown";
                        entry.TextValue = settings.ActiveFeelPresetName;
                        break;
                    case "saveAs":
                    case "deletePreset":
                    case "reset":
                        entry.ValueKind = "button";
                        break;
                    case "showDebugPanel":
                        entry.ValueKind = "toggle";
                        entry.BoolValue = settings.AssistUiEnabled;
                        break;
                    case "zoomSensitivity":
                        entry.ValueKind = toolkit == FeelControlKind.Numeric ? "numeric" : "slider";
                        entry.NumericValue =
                            toolkit == FeelControlKind.Numeric
                                ? settings.ZoomGain
                                : FeelMath.GainToSensitivityUi(settings.ZoomGain, factory.ZoomGain);
                        break;
                    case "panSensitivityX":
                        entry.ValueKind = toolkit == FeelControlKind.Numeric ? "numeric" : "slider";
                        entry.NumericValue =
                            toolkit == FeelControlKind.Numeric
                                ? settings.PanGainX
                                : FeelMath.GainToSensitivityUi(settings.PanGainX, factory.PanGainX);
                        break;
                    case "panSensitivityY":
                        entry.ValueKind = toolkit == FeelControlKind.Numeric ? "numeric" : "slider";
                        entry.NumericValue =
                            toolkit == FeelControlKind.Numeric
                                ? settings.PanGainY
                                : FeelMath.GainToSensitivityUi(settings.PanGainY, factory.PanGainY);
                        break;
                    case "rotateSensitivity":
                        entry.ValueKind = toolkit == FeelControlKind.Numeric ? "numeric" : "slider";
                        entry.NumericValue =
                            toolkit == FeelControlKind.Numeric
                                ? settings.RotateGain
                                : FeelMath.GainToSensitivityUi(
                                    settings.RotateGain,
                                    factory.RotateGain
                                );
                        break;
                    case "orbitYawSensitivity":
                        entry.ValueKind = toolkit == FeelControlKind.Numeric ? "numeric" : "slider";
                        entry.NumericValue =
                            toolkit == FeelControlKind.Numeric
                                ? settings.OrbitYawGain
                                : FeelMath.GainToSensitivityUi(
                                    settings.OrbitYawGain,
                                    factory.OrbitYawGain
                                );
                        break;
                    case "orbitPitchSensitivity":
                        entry.ValueKind = toolkit == FeelControlKind.Numeric ? "numeric" : "slider";
                        entry.NumericValue =
                            toolkit == FeelControlKind.Numeric
                                ? settings.OrbitPitchGain
                                : FeelMath.GainToSensitivityUi(
                                    settings.OrbitPitchGain,
                                    factory.OrbitPitchGain
                                );
                        break;
                    case "zoomDeadband":
                        entry.ValueKind = "numeric";
                        entry.NumericValue = settings.PinchDeadband;
                        break;
                    case "panDeadband":
                    case "orbitDeadband":
                        entry.ValueKind = "numeric";
                        entry.NumericValue = settings.MotionDeadband;
                        break;
                    case "rotateDeadband":
                        entry.ValueKind = "numeric";
                        entry.NumericValue = settings.RotateDeadband;
                        break;
                    default:
                        entry.ValueKind =
                            toolkit == FeelControlKind.Button ? "button"
                            : toolkit == FeelControlKind.Dropdown ? "dropdown"
                            : toolkit == FeelControlKind.Checkbox ? "toggle"
                            : "numeric";
                        break;
                }

                list.Add(entry);
            }

            return list;
        }

#if HAS_CITIES
        private static readonly List<Action> SensitivitySliderRefreshes = new List<Action>(8);
        private static FeelEditor _boundEditor;
        private static UIComponent _optionsRoot;
        private static UIDropDown _feelPresetDropdown;
        private static string[] _feelPresetDropdownItems;
        private static UIButton _saveAsButton;
        private static UIButton _deleteButton;
        private static bool _feelPresetDropdownSyncing;
        private static bool _settingsHooked;

        public static void BindOptionsCatalog(UIHelperBase helper, FeelEditor editor)
        {
            if (helper == null || editor == null || editor.Settings == null)
            {
                return;
            }

            DetachOptionsSync();
            SensitivitySliderRefreshes.Clear();
            _boundEditor = editor;
            _feelPresetDropdown = null;
            _feelPresetDropdownItems = null;
            _saveAsButton = null;
            _deleteButton = null;

            ModSettings settings = editor.Settings;
            ModSettings factory = ModSettings.CreateFactoryDefaults();

            foreach (string section in FeelCatalog.SectionOrder())
            {
                UIHelperBase group = helper.AddGroup(section);
                if (section == "Zoom")
                {
                    AddGroupDescription(group, "Zoom", OptionsHost.OpDescriptionZoom);
                }
                else if (section == "Pan")
                {
                    AddGroupDescription(group, "Pan", OptionsHost.OpDescriptionPan);
                }
                else if (section == "Rotate")
                {
                    AddGroupDescription(group, "Rotate", OptionsHost.OpDescriptionRotate);
                }
                else if (section == "Orbit")
                {
                    AddGroupDescription(group, "Orbit", OptionsHost.OpDescriptionOrbit);
                }

                foreach (
                    FeelControlDescriptor d in OptionsHost.BuildDescriptors(FeelHostKind.Options)
                )
                {
                    if (d.Section != section)
                    {
                        continue;
                    }

                    BindOptionsField(group, d, editor, settings, factory);
                }
            }

            AttachOptionsSync(helper);
            RefreshAllSensitivitySliders();
            RefreshFeelPresetControls();
        }

        private static void AttachOptionsSync(UIHelperBase helper)
        {
            UIHelper ui = helper as UIHelper;
            _optionsRoot = ui != null ? ui.self as UIComponent : null;
            if (!_settingsHooked)
            {
                FeelEditor.SettingsChanged += OnOptionsSettingsChanged;
                _settingsHooked = true;
            }

            if (_optionsRoot != null)
            {
                _optionsRoot.eventVisibilityChanged += OnOptionsRootVisibilityChanged;
            }
        }

        private static void DetachOptionsSync()
        {
            if (_optionsRoot != null)
            {
                _optionsRoot.eventVisibilityChanged -= OnOptionsRootVisibilityChanged;
                _optionsRoot = null;
            }
        }

        private static void OnOptionsSettingsChanged()
        {
            RefreshAllSensitivitySliders();
            RefreshFeelPresetControls();
        }

        private static void OnOptionsRootVisibilityChanged(UIComponent component, bool visible)
        {
            if (visible)
            {
                RefreshAllSensitivitySliders();
                RefreshFeelPresetControls();
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

        private static void RefreshFeelPresetControls()
        {
            FeelEditor editor = _boundEditor;
            if (editor == null || editor.Settings == null)
            {
                return;
            }

            ModSettings live = editor.Settings;
            if (_feelPresetDropdown != null)
            {
                string[] items = BuildFeelPresetItems(editor);
                _feelPresetDropdownItems = items;
                _feelPresetDropdownSyncing = true;
                try
                {
                    _feelPresetDropdown.items = items;
                    _feelPresetDropdown.selectedIndex = IndexOfFeelPreset(
                        items,
                        live.ActiveFeelPresetName
                    );
                }
                finally
                {
                    _feelPresetDropdownSyncing = false;
                }
            }

            if (_saveAsButton != null)
            {
                _saveAsButton.isEnabled = OptionsHost.IsFeelDirtyNewPreset(live);
            }

            if (_deleteButton != null)
            {
                _deleteButton.isEnabled = OptionsHost.IsNamedUserFeelPreset(live);
            }
        }

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

            UIComponent root = ui.self as UIComponent;
            if (root == null)
            {
                return;
            }

            UILabel label = root.AddUIComponent<UILabel>();
            label.name = "OpHeading" + opId;
            label.textScale = 0.85f;
            label.autoSize = true;
            label.wordWrap = true;
            label.width = Mathf.Max(200f, root.width - 20f);
            label.text = text;
            label.PerformLayout();
        }

        /// <summary>Legacy entry used by older Debug path; Options skin only.</summary>
        public static void BindCatalog(UIHelperBase helper, FeelEditor editor)
        {
            BindOptionsCatalog(helper, editor);
        }

        public static void BindOptionsField(
            UIHelperBase group,
            FeelControlDescriptor d,
            FeelEditor editor,
            ModSettings settings,
            ModSettings factory
        )
        {
            switch (d.Id)
            {
                case "feelPreset":
                    BindFeelPresetDropdown(group, d, editor, settings);
                    break;
                case "saveAs":
                    {
                        object created = group.AddButton(
                            d.Label,
                            () =>
                            {
                                FeelSaveAsDialog.Show(editor, RefreshFeelPresetControls);
                            }
                        );
                        _saveAsButton = created as UIButton;
                        if (_saveAsButton != null)
                        {
                            _saveAsButton.isEnabled = OptionsHost.IsFeelDirtyNewPreset(settings);
                        }
                    }

                    break;
                case "deletePreset":
                    {
                        object created = group.AddButton(
                            d.Label,
                            () =>
                            {
                                if (OptionsHost.IsNamedUserFeelPreset(editor.Settings))
                                {
                                    editor.DeleteNamedPreset(editor.Settings.ActiveFeelPresetName);
                                }
                            }
                        );
                        _deleteButton = created as UIButton;
                        if (_deleteButton != null)
                        {
                            _deleteButton.isEnabled = OptionsHost.IsNamedUserFeelPreset(settings);
                        }
                    }

                    break;
                case "showDebugPanel":
                    group.AddCheckbox(
                        d.Label,
                        settings.AssistUiEnabled,
                        v =>
                        {
                            editor.SetShowDebugPanel(v);
                        }
                    );
                    break;
                case "zoomSensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.ZoomGain,
                        factory.ZoomGain,
                        gain => editor.ApplyGain((s, v) => s.ZoomGain = v, gain)
                    );
                    break;
                case "panSensitivityX":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.PanGainX,
                        factory.PanGainX,
                        gain => editor.ApplyGain((s, v) => s.PanGainX = v, gain)
                    );
                    break;
                case "panSensitivityY":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.PanGainY,
                        factory.PanGainY,
                        gain => editor.ApplyGain((s, v) => s.PanGainY = v, gain)
                    );
                    break;
                case "rotateSensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.RotateGain,
                        factory.RotateGain,
                        gain => editor.ApplyGain((s, v) => s.RotateGain = v, gain)
                    );
                    break;
                case "orbitYawSensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.OrbitYawGain,
                        factory.OrbitYawGain,
                        gain => editor.ApplyGain((s, v) => s.OrbitYawGain = v, gain)
                    );
                    break;
                case "orbitPitchSensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.OrbitPitchGain,
                        factory.OrbitPitchGain,
                        gain => editor.ApplyGain((s, v) => s.OrbitPitchGain = v, gain)
                    );
                    break;
            }
        }

        private static void BindFeelPresetDropdown(
            UIHelperBase group,
            FeelControlDescriptor d,
            FeelEditor editor,
            ModSettings settings
        )
        {
            _feelPresetDropdownItems = BuildFeelPresetItems(editor);
            int selected = IndexOfFeelPreset(
                _feelPresetDropdownItems,
                settings.ActiveFeelPresetName
            );
            object created = group.AddDropdown(
                d.Label,
                _feelPresetDropdownItems,
                selected,
                sel =>
                {
                    if (_feelPresetDropdownSyncing)
                    {
                        return;
                    }

                    if (
                        _feelPresetDropdownItems == null
                        || sel < 0
                        || sel >= _feelPresetDropdownItems.Length
                    )
                    {
                        return;
                    }

                    editor.LoadPreset(_feelPresetDropdownItems[sel]);
                }
            );
            _feelPresetDropdown = created as UIDropDown;
        }

        internal static string[] BuildFeelPresetItems(FeelEditor editor)
        {
            var items = new List<string>
            {
                FeelProfiles.NameSlow,
                FeelProfiles.NameDefault,
                FeelProfiles.NameFast,
                FeelProfiles.NameNewPreset,
            };
            if (editor != null && editor.Store != null)
            {
                string[] named = editor.Store.ListUserPresetNames();
                for (int i = 0; i < named.Length; i++)
                {
                    if (
                        !string.IsNullOrEmpty(named[i])
                        && !string.Equals(
                            named[i],
                            FeelProfiles.NameNewPreset,
                            StringComparison.Ordinal
                        )
                        && !FeelProfiles.IsBuiltInName(named[i])
                        && !items.Contains(named[i])
                    )
                    {
                        items.Add(named[i]);
                    }
                }
            }

            return items.ToArray();
        }

        internal static int IndexOfFeelPreset(string[] labels, string active)
        {
            if (labels == null)
            {
                return 0;
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (string.Equals(labels[i], active, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return 0;
        }

        private static void BindSensitivitySlider(
            UIHelperBase group,
            string label,
            Func<float> getGain,
            float factoryDefault,
            Action<float> onGain
        )
        {
            float uiValue = FeelMath.GainToSensitivityUi(getGain(), factoryDefault);
            bool suppressCallback = true;
            object created = group.AddSlider(
                label,
                FeelMath.SensitivityUiMin,
                FeelMath.SensitivityUiMax,
                FeelMath.SensitivityUiStep,
                uiValue,
                v =>
                {
                    if (suppressCallback)
                    {
                        return;
                    }

                    onGain(FeelMath.SensitivityUiToGain(v, factoryDefault));
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
                        float ui = FeelMath.GainToSensitivityUi(getGain(), factoryDefault);
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
        /// at first set.
        /// </summary>
        private static void ForceSliderUi(UISlider slider, float ui)
        {
            if (slider == null)
            {
                return;
            }

            if (ui < FeelMath.SensitivityUiMin)
            {
                ui = FeelMath.SensitivityUiMin;
            }

            if (ui > FeelMath.SensitivityUiMax)
            {
                ui = FeelMath.SensitivityUiMax;
            }

            slider.minValue = FeelMath.SensitivityUiMin;
            slider.maxValue = FeelMath.SensitivityUiMax;
            slider.stepSize = FeelMath.SensitivityUiStep;

            float bump =
                ui < FeelMath.SensitivityUiFactory
                    ? FeelMath.SensitivityUiMax
                    : FeelMath.SensitivityUiMin;
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
#endif
    }
}
