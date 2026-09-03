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

    /// <summary>Options host: catalogs FeelCatalog into Colossal Options groups.</summary>
    public static class OptionsHost
    {
        public static IList<FeelCatalogField> Fields => FeelCatalog.AllFields();

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
        public static void Build(UIHelperBase helper, ModSettings settings)
        {
            if (helper == null || settings == null)
            {
                return;
            }

            foreach (string section in FeelCatalog.SectionOrder())
            {
                UIHelperBase group = helper.AddGroup(section);
                foreach (FeelControlDescriptor d in BuildDescriptors())
                {
                    if (d.Section != section)
                    {
                        continue;
                    }

                    group.AddCheckbox(
                        d.Label,
                        false,
                        _ =>
                        {
                            FeelEditor.NotifyChanged();
                        }
                    );
                }
            }
        }
#else
        public static void Build(object helper, ModSettings settings)
        {
            _ = helper;
            _ = settings;
            _ = BuildDescriptors();
        }
#endif
    }
}
