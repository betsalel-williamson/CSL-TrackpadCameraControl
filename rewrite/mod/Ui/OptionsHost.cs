using System;
using System.Collections.Generic;
using System.Globalization;
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
        public FeelHostKind Host { get; set; }
    }

    /// <summary>Options host: catalogs FeelCatalog into Colossal Options groups via FeelEditor.</summary>
    public static class OptionsHost
    {
        public static IList<FeelCatalogField> Fields => FeelCatalog.AllFields();

        /// <summary>Maps+ gesture blurbs for Options op groups (static ship copy).</summary>
        public static string OpDescriptionZoom =>
            "Gesture(s): pinch\nKeymapping(s): mouse wheel zoom";

        public static string OpDescriptionPan =>
            "Gesture(s): two-finger drag\nKeymapping(s): edge pan / WASD";

        public static string OpDescriptionRotate =>
            "Gesture(s): two-finger twist\nKeymapping(s): rotate left/right";

        public static string OpDescriptionOrbit =>
            "Gesture(s): Option (⌥) + two-finger drag\nKeymapping(s): middle-mouse orbit";

        /// <summary>Pure mapping for unit tests — no Colossal session required.</summary>
        public static FeelControlKind MapKind(FeelControlKind catalogKind)
        {
            return FeelHostMapping.MapKind(catalogKind);
        }

        /// <summary>Options-visible descriptors in section order.</summary>
        public static IList<FeelControlDescriptor> BuildDescriptors()
        {
            return BuildDescriptors(FeelHostKind.Options);
        }

        /// <summary>Host-filtered descriptors in section order.</summary>
        public static IList<FeelControlDescriptor> BuildDescriptors(FeelHostKind host)
        {
            var list = new List<FeelControlDescriptor>();
            foreach (string section in FeelCatalog.SectionOrder())
            {
                foreach (FeelCatalogField field in FeelCatalog.AllFields())
                {
                    if (field.Section != section || !FeelCatalog.IsVisibleOn(field, host))
                    {
                        continue;
                    }

                    list.Add(
                        new FeelControlDescriptor
                        {
                            Section = field.Section,
                            Id = field.Id,
                            Label = field.Label,
                            Kind = FeelCatalog.KindOn(field, host),
                            Host = host,
                        }
                    );
                }
            }

            return list;
        }

        /// <summary>Suggested Save as… name: overwrite active named, else next New Preset N.</summary>
        public static string SuggestFeelSaveAsName(FeelEditor editor)
        {
            if (editor == null || editor.Settings == null)
            {
                return FeelProfiles.NameNewPreset + " 1";
            }

            if (IsNamedUserFeelPreset(editor.Settings))
            {
                return editor.Settings.ActiveFeelPresetName;
            }

            return NextNumberedNewPresetName(editor);
        }

        public static bool IsFeelDirtyNewPreset(ModSettings settings)
        {
            return settings != null
                && string.Equals(
                    settings.ActiveFeelPresetName,
                    FeelProfiles.NameNewPreset,
                    StringComparison.Ordinal
                );
        }

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

        public static string NextNumberedNewPresetName(FeelEditor editor)
        {
            string prefix = FeelProfiles.NameNewPreset + " ";
            int max = 0;
            string[] named =
                editor != null && editor.Store != null
                    ? editor.Store.ListUserPresetNames()
                    : new string[0];
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
                    if (
                        int.TryParse(
                            rest,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out int n
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

#if HAS_CITIES
        public static void Build(UIHelperBase helper, FeelEditor editor)
        {
            FeelHostBinder.BindOptionsCatalog(helper, editor);
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
