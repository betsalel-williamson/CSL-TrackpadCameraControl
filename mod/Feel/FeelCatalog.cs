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

        /// <summary>Toolkit mapping result for <see cref="Toggle"/> (Colossal AddCheckbox).</summary>
        Checkbox,
    }

    public enum FeelHostKind
    {
        Options,
        Debug,
    }

    public sealed class FeelCatalogField
    {
        public string Section { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public FeelControlKind OptionsKind { get; set; }
        public FeelControlKind DebugKind { get; set; }
        public bool OptionsVisible { get; set; }
        public bool DebugVisible { get; set; }

        /// <summary>Legacy single-kind accessor for tests that map toolkit kinds.</summary>
        public FeelControlKind Kind => OptionsVisible ? OptionsKind : DebugKind;

        public FeelCatalogField(
            string section,
            string id,
            string label,
            FeelControlKind optionsKind,
            FeelControlKind debugKind,
            bool optionsVisible,
            bool debugVisible
        )
        {
            Section = section;
            Id = id;
            Label = label;
            OptionsKind = optionsKind;
            DebugKind = debugKind;
            OptionsVisible = optionsVisible;
            DebugVisible = debugVisible;
        }
    }

    /// <summary>Ordered inventory of player-facing feel controls (Options + Debug share this).</summary>
    public static class FeelCatalog
    {
        private static readonly FeelCatalogField[] Fields =
        {
            new FeelCatalogField(
                "General",
                "showDebugPanel",
                "Show debug panel",
                FeelControlKind.Toggle,
                FeelControlKind.Toggle,
                optionsVisible: true,
                debugVisible: false
            ),
            new FeelCatalogField(
                "General",
                "feelPreset",
                "Feel preset",
                FeelControlKind.Dropdown,
                FeelControlKind.Dropdown,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "General",
                "saveAs",
                "Save as…",
                FeelControlKind.Button,
                FeelControlKind.Button,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "General",
                "deletePreset",
                "Delete",
                FeelControlKind.Button,
                FeelControlKind.Button,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "General",
                "reset",
                "Reset",
                FeelControlKind.Button,
                FeelControlKind.Button,
                optionsVisible: false,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Zoom",
                "zoomSensitivity",
                "Sensitivity",
                FeelControlKind.Slider,
                FeelControlKind.Numeric,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Zoom",
                "zoomDeadband",
                "Deadband",
                FeelControlKind.Numeric,
                FeelControlKind.Numeric,
                optionsVisible: false,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Pan",
                "panSensitivityX",
                "Sensitivity X",
                FeelControlKind.Slider,
                FeelControlKind.Numeric,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Pan",
                "panSensitivityY",
                "Sensitivity Y",
                FeelControlKind.Slider,
                FeelControlKind.Numeric,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Pan",
                "panDeadband",
                "Deadband",
                FeelControlKind.Numeric,
                FeelControlKind.Numeric,
                optionsVisible: false,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Rotate",
                "rotateSensitivity",
                "Sensitivity",
                FeelControlKind.Slider,
                FeelControlKind.Numeric,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Rotate",
                "rotateDeadband",
                "Deadband",
                FeelControlKind.Numeric,
                FeelControlKind.Numeric,
                optionsVisible: false,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Orbit",
                "orbitYawSensitivity",
                "Sensitivity yaw",
                FeelControlKind.Slider,
                FeelControlKind.Numeric,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Orbit",
                "orbitPitchSensitivity",
                "Sensitivity pitch",
                FeelControlKind.Slider,
                FeelControlKind.Numeric,
                optionsVisible: true,
                debugVisible: true
            ),
            new FeelCatalogField(
                "Orbit",
                "orbitDeadband",
                "Deadband",
                FeelControlKind.Numeric,
                FeelControlKind.Numeric,
                optionsVisible: false,
                debugVisible: true
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

        public static bool IsVisibleOn(FeelCatalogField field, FeelHostKind host)
        {
            if (field == null)
            {
                return false;
            }

            return host == FeelHostKind.Options ? field.OptionsVisible : field.DebugVisible;
        }

        public static FeelControlKind KindOn(FeelCatalogField field, FeelHostKind host)
        {
            if (field == null)
            {
                return FeelControlKind.Button;
            }

            return host == FeelHostKind.Options ? field.OptionsKind : field.DebugKind;
        }
    }
}
