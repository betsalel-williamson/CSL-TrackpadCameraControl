using System;
using System.Collections.Generic;
#if HAS_CITIES
using ICities;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Control descriptor produced from <see cref="FeelCatalog"/> for Options/Debug hosts.
    /// Hosts map descriptors to toolkit widgets — they do not own the field list.
    /// </summary>
    public sealed class FeelControlDescriptor
    {
        public string Section { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public FeelControlKind Kind { get; set; }
    }

    /// <summary>Options host: catalogs FeelCatalog into Colossal Options groups via FeelEditor.</summary>
    public static class OptionsHost
    {
        public static IList<FeelCatalogField> Fields => FeelCatalog.AllFields();

        /// <summary>Pure mapping for unit tests — no Colossal session required.</summary>
        public static FeelControlKind MapKind(FeelControlKind catalogKind)
        {
            return FeelHostMapping.MapKind(catalogKind);
        }

        /// <summary>Pure mapping for unit tests — no Colossal session required.</summary>
        public static IList<FeelControlDescriptor> BuildDescriptors()
        {
            var list = new List<FeelControlDescriptor>();
            foreach (string section in FeelCatalog.SectionOrder())
            {
                foreach (FeelCatalogField field in FeelCatalog.AllFields())
                {
                    if (field.Section != section)
                    {
                        continue;
                    }

                    list.Add(
                        new FeelControlDescriptor
                        {
                            Section = field.Section,
                            Id = field.Id,
                            Label = field.Label,
                            Kind = field.Kind,
                        }
                    );
                }
            }

            return list;
        }

#if HAS_CITIES
        public static void Build(UIHelperBase helper, FeelEditor editor)
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
                foreach (FeelControlDescriptor d in BuildDescriptors())
                {
                    if (d.Section != section)
                    {
                        continue;
                    }

                    BindField(group, d, editor, settings, factory);
                }
            }
        }

        private static void BindField(
            UIHelperBase group,
            FeelControlDescriptor d,
            FeelEditor editor,
            ModSettings settings,
            ModSettings factory
        )
        {
            FeelControlKind toolkit = MapKind(d.Kind);
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
                    // General sensitivity: informational / representative ZoomGain bind only.
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
                    // Unknown ids: still honor toolkit kind so we never checkbox-for-all.
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
#else
        public static void Build(object helper, FeelEditor editor)
        {
            _ = helper;
            _ = editor;
            _ = BuildDescriptors();
        }

        public static void Build(object helper, ModSettings settings)
        {
            _ = helper;
            _ = settings;
            _ = BuildDescriptors();
        }
#endif
    }
}
