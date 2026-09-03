using System;
using System.Collections.Generic;

namespace TrackpadCameraControl.Rewrite
{
    public enum FeelControlKind
    {
        Dropdown,
        Button,
        Toggle,
        Slider,
        Numeric,
    }

    public sealed class FeelCatalogField
    {
        public string Section { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public FeelControlKind Kind { get; set; }

        public FeelCatalogField(string section, string id, string label, FeelControlKind kind)
        {
            Section = section;
            Id = id;
            Label = label;
            Kind = kind;
        }
    }

    /// <summary>Ordered inventory of player-facing feel controls (Options + Debug share this).</summary>
    public static class FeelCatalog
    {
        private static readonly FeelCatalogField[] Fields =
        {
            new FeelCatalogField("General", "feelPreset", "Feel preset", FeelControlKind.Dropdown),
            new FeelCatalogField("General", "saveAs", "Save as…", FeelControlKind.Button),
            new FeelCatalogField("General", "deletePreset", "Delete", FeelControlKind.Button),
            new FeelCatalogField(
                "General",
                "resetFactory",
                "Reset to factory",
                FeelControlKind.Button
            ),
            new FeelCatalogField("General", "sensitivity", "Sensitivity", FeelControlKind.Slider),
            new FeelCatalogField(
                "General",
                "showDebugPanel",
                "Show debug panel",
                FeelControlKind.Toggle
            ),
            new FeelCatalogField("Zoom", "zoomSensitivity", "Sensitivity", FeelControlKind.Slider),
            new FeelCatalogField("Pan", "panSensitivity", "Sensitivity", FeelControlKind.Slider),
            new FeelCatalogField(
                "Rotate",
                "rotateSensitivity",
                "Sensitivity",
                FeelControlKind.Slider
            ),
            new FeelCatalogField(
                "Orbit",
                "orbitSensitivity",
                "Sensitivity",
                FeelControlKind.Slider
            ),
        };

        public static IList<FeelCatalogField> AllFields()
        {
            return Fields;
        }

        public static string[] SectionOrder()
        {
            return new[] { "General", "Zoom", "Pan", "Rotate", "Orbit" };
        }
    }
}
