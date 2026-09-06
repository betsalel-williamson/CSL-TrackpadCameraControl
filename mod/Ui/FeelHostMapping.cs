using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Maps FeelCatalog control kinds to Colossal toolkit widgets used by Options/Debug hosts.
    /// Hosts must call <see cref="MapKind"/> — never treat every field as Checkbox.
    /// </summary>
    public static class FeelHostMapping
    {
        /// <summary>Catalog kind → toolkit control kind (Toggle → Checkbox).</summary>
        public static FeelControlKind MapKind(FeelControlKind catalogKind)
        {
            switch (catalogKind)
            {
                case FeelControlKind.Dropdown:
                    return FeelControlKind.Dropdown;
                case FeelControlKind.Button:
                    return FeelControlKind.Button;
                case FeelControlKind.Toggle:
                    return FeelControlKind.Checkbox;
                case FeelControlKind.Slider:
                    return FeelControlKind.Slider;
                case FeelControlKind.Numeric:
                    return FeelControlKind.Numeric;
                case FeelControlKind.Checkbox:
                    return FeelControlKind.Checkbox;
                default:
                    return catalogKind;
            }
        }

        /// <summary>Alias used by some call sites / docs.</summary>
        public static FeelControlKind ExpectedToolkit(FeelControlKind catalogKind)
        {
            return MapKind(catalogKind);
        }

        public static void AssertKindMapsTo(FeelControlKind catalogKind, FeelControlKind expected)
        {
            FeelControlKind actual = MapKind(catalogKind);
            if (actual != expected)
            {
                throw new InvalidOperationException(
                    "FeelHostMapping: "
                        + catalogKind
                        + " maps to "
                        + actual
                        + ", expected "
                        + expected
                );
            }
        }
    }
}
