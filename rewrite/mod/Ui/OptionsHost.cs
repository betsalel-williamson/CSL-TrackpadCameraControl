using System.Collections.Generic;
#if HAS_CITIES
using ICities;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Options host: catalogs FeelCatalog into Colossal Options groups.</summary>
    public static class OptionsHost
    {
        public static IList<FeelCatalogField> Fields => FeelCatalog.AllFields();

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
                foreach (FeelCatalogField field in FeelCatalog.AllFields())
                {
                    if (field.Section != section)
                    {
                        continue;
                    }

                    // Thin wiring: labels only until feel editor binds are expanded.
                    group.AddCheckbox(
                        field.Label,
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
        }
#endif
    }
}
