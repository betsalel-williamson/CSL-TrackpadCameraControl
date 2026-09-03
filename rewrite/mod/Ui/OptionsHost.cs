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
            FeelHostBinder.BindCatalog(helper, editor);
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
