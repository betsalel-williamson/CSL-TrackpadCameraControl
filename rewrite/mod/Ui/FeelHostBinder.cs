using System;
using System.Collections.Generic;
#if HAS_CITIES
using ICities;
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
            var list = new List<FeelPanelEntry>();
            if (editor == null || editor.Settings == null)
            {
                return list;
            }

            ModSettings settings = editor.Settings;
            ModSettings factory = ModSettings.CreateFactoryDefaults();
            foreach (FeelControlDescriptor d in OptionsHost.BuildDescriptors())
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
                    case "resetFactory":
                        entry.ValueKind = "button";
                        break;
                    case "showDebugPanel":
                        entry.ValueKind = "toggle";
                        entry.BoolValue = settings.AssistUiEnabled;
                        break;
                    case "sensitivity":
                        entry.ValueKind = "slider";
                        entry.NumericValue = FeelMath.GainToSensitivityUi(
                            settings.ZoomGain,
                            factory.ZoomGain
                        );
                        break;
                    case "zoomSensitivity":
                        entry.ValueKind = "slider";
                        entry.NumericValue = FeelMath.GainToSensitivityUi(
                            settings.ZoomGain,
                            factory.ZoomGain
                        );
                        break;
                    case "panSensitivity":
                        entry.ValueKind = "slider";
                        entry.NumericValue = FeelMath.GainToSensitivityUi(
                            settings.PanGainX,
                            factory.PanGainX
                        );
                        break;
                    case "rotateSensitivity":
                        entry.ValueKind = "slider";
                        entry.NumericValue = FeelMath.GainToSensitivityUi(
                            settings.RotateGain,
                            factory.RotateGain
                        );
                        break;
                    case "orbitSensitivity":
                        entry.ValueKind = "slider";
                        entry.NumericValue = FeelMath.GainToSensitivityUi(
                            settings.OrbitYawGain,
                            factory.OrbitYawGain
                        );
                        break;
                    default:
                        if (toolkit == FeelControlKind.Slider)
                        {
                            entry.ValueKind = "slider";
                            entry.NumericValue = FeelMath.GainToSensitivityUi(
                                settings.ZoomGain,
                                factory.ZoomGain
                            );
                        }
                        else if (toolkit == FeelControlKind.Button)
                        {
                            entry.ValueKind = "button";
                        }
                        else if (toolkit == FeelControlKind.Dropdown)
                        {
                            entry.ValueKind = "dropdown";
                            entry.TextValue = settings.ActiveFeelPresetName;
                        }
                        else if (toolkit == FeelControlKind.Checkbox)
                        {
                            entry.ValueKind = "toggle";
                            entry.BoolValue = false;
                        }

                        break;
                }

                list.Add(entry);
            }

            return list;
        }

#if HAS_CITIES
        public static void BindCatalog(UIHelperBase helper, FeelEditor editor)
        {
            if (helper == null || editor == null || editor.Settings == null)
            {
                return;
            }

            ModSettings settings = editor.Settings;
            ModSettings factory = ModSettings.CreateFactoryDefaults();

            foreach (string section in FeelCatalog.SectionOrder())
            {
                UIHelperBase group = helper.AddGroup(section);
                foreach (FeelControlDescriptor d in OptionsHost.BuildDescriptors())
                {
                    if (d.Section != section)
                    {
                        continue;
                    }

                    BindField(group, d, editor, settings, factory);
                }
            }
        }

        public static void BindField(
            UIHelperBase group,
            FeelControlDescriptor d,
            FeelEditor editor,
            ModSettings settings,
            ModSettings factory
        )
        {
            FeelControlKind toolkit = FeelHostMapping.MapKind(d.Kind);
            switch (d.Id)
            {
                case "feelPreset":
                    BindFeelPresetDropdown(group, d, editor, settings);
                    break;
                case "saveAs":
                    group.AddButton(
                        "Save as MyFeel",
                        () =>
                        {
                            editor.SaveAs("MyFeel");
                        }
                    );
                    break;
                case "deletePreset":
                    group.AddButton(
                        d.Label,
                        () =>
                        {
                            string name = settings.ActiveFeelPresetName;
                            if (
                                !string.IsNullOrEmpty(name)
                                && !FeelProfiles.IsBuiltInName(name)
                                && !string.Equals(
                                    name,
                                    FeelProfiles.NameNewPreset,
                                    StringComparison.Ordinal
                                )
                            )
                            {
                                editor.DeleteNamedPreset(name);
                            }
                        }
                    );
                    break;
                case "resetFactory":
                    group.AddButton(
                        d.Label,
                        () =>
                        {
                            editor.ResetToFactory();
                        }
                    );
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
                case "sensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label + " (zoom)",
                        () => settings.ZoomGain,
                        factory.ZoomGain,
                        gain => editor.ApplyGain((s, v) => s.ZoomGain = v, gain)
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
                case "panSensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.PanGainX,
                        factory.PanGainX,
                        gain =>
                            editor.ApplyGain(
                                (s, v) =>
                                {
                                    s.PanGainX = v;
                                    s.PanGainY = v;
                                },
                                gain
                            )
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
                case "orbitSensitivity":
                    BindSensitivitySlider(
                        group,
                        d.Label,
                        () => settings.OrbitYawGain,
                        factory.OrbitYawGain,
                        gain =>
                            editor.ApplyGain(
                                (s, v) =>
                                {
                                    s.OrbitYawGain = v;
                                    s.OrbitPitchGain = v;
                                },
                                gain
                            )
                    );
                    break;
                default:
                    if (toolkit == FeelControlKind.Slider)
                    {
                        BindSensitivitySlider(
                            group,
                            d.Label,
                            () => settings.ZoomGain,
                            factory.ZoomGain,
                            gain => editor.ApplyGain((s, v) => s.ZoomGain = v, gain)
                        );
                    }
                    else if (toolkit == FeelControlKind.Button)
                    {
                        group.AddButton(d.Label, () => FeelEditor.NotifyChanged());
                    }
                    else if (toolkit == FeelControlKind.Dropdown)
                    {
                        group.AddDropdown(d.Label, new[] { FeelProfiles.NameDefault }, 0, _ => { });
                    }
                    else if (toolkit == FeelControlKind.Checkbox)
                    {
                        group.AddCheckbox(d.Label, false, _ => FeelEditor.NotifyChanged());
                    }

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
            var items = new List<string>
            {
                FeelProfiles.NameSlow,
                FeelProfiles.NameDefault,
                FeelProfiles.NameFast,
                FeelProfiles.NameNewPreset,
            };
            if (editor.Store != null)
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

            string[] labels = items.ToArray();
            int selected = 0;
            string active = settings.ActiveFeelPresetName;
            for (int i = 0; i < labels.Length; i++)
            {
                if (string.Equals(labels[i], active, StringComparison.Ordinal))
                {
                    selected = i;
                    break;
                }
            }

            group.AddDropdown(
                d.Label,
                labels,
                selected,
                sel =>
                {
                    if (sel < 0 || sel >= labels.Length)
                    {
                        return;
                    }

                    editor.LoadPreset(labels[sel]);
                }
            );
        }

        private static void BindSensitivitySlider(
            UIHelperBase group,
            string label,
            Func<float> getGain,
            float factoryDefault,
            Action<float> onGain
        )
        {
            float ui = FeelMath.GainToSensitivityUi(getGain(), factoryDefault);
            group.AddSlider(
                label,
                FeelMath.SensitivityUiMin,
                FeelMath.SensitivityUiMax,
                FeelMath.SensitivityUiStep,
                ui,
                v =>
                {
                    onGain(FeelMath.SensitivityUiToGain(v, factoryDefault));
                }
            );
        }
#endif
    }
}
