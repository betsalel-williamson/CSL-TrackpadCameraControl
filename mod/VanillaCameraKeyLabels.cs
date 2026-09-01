using System;
using System.Collections.Generic;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Builds op-heading copy with live vanilla key labels from Cities keymappings.
    /// </summary>
    internal static partial class VanillaCameraKeyLabels
    {
        internal static string JoinBindingLabels(IEnumerable<string> labels)
        {
            if (labels == null)
            {
                return null;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parts = new List<string>();
            foreach (string label in labels)
            {
                if (string.IsNullOrEmpty(label) || !seen.Add(label))
                {
                    continue;
                }

                parts.Add(label);
            }

            return parts.Count == 0 ? null : string.Join(" · ", parts.ToArray());
        }

        internal static string FormatVanillaActionLine(string bindings, string action)
        {
            if (string.IsNullOrEmpty(bindings))
            {
                return action;
            }

            return bindings + ": " + action;
        }

        private static string BuildDescription(
            string meaning,
            string mapsPlus,
            string vanillaBindings,
            string vanillaAction
        )
        {
            return meaning
                + "\n"
                + mapsPlus
                + "\n"
                + FormatVanillaActionLine(vanillaBindings, vanillaAction);
        }

        private static string BuildHeading(
            string title,
            string meaning,
            string mapsPlus,
            string vanillaBindings,
            string vanillaAction
        )
        {
            return title
                + "\n"
                + BuildDescription(meaning, mapsPlus, vanillaBindings, vanillaAction);
        }

        public static string OpDescriptionZoom =>
            BuildDescription(
                "Change camera distance / size",
                "Pinch",
                ResolveZoomVanillaBindings(),
                "vanilla zoom"
            );

        public static string OpHeadingZoom => "Zoom\n" + OpDescriptionZoom;

        public static string OpDescriptionPan =>
            BuildDescription(
                "Slide the camera laterally",
                "Two-finger drag",
                ResolvePanVanillaBindings(),
                "vanilla"
            );

        public static string OpHeadingPan => "Pan\n" + OpDescriptionPan;

        public static string OpDescriptionRotate =>
            BuildDescription(
                "Yaw the camera or rotate a place/relocate ghost",
                "Two-finger rotate",
                ResolveRotateVanillaBindings(),
                "vanilla"
            );

        public static string OpHeadingRotate => "Rotate\n" + OpDescriptionRotate;

        public static string OpDescriptionOrbit =>
            BuildDescription(
                "Pitch + yaw around the pivot",
                "Option (⌥)+two-finger drag",
                ResolveOrbitVanillaBindings(),
                "vanilla orbit"
            );

        public static string OpHeadingOrbit => "Orbit\n" + OpDescriptionOrbit;

#if !HAS_CITIES
        private static string ResolveZoomVanillaBindings()
        {
            return null;
        }

        private static string ResolvePanVanillaBindings()
        {
            return null;
        }

        private static string ResolveRotateVanillaBindings()
        {
            return null;
        }

        private static string ResolveOrbitVanillaBindings()
        {
            return null;
        }
#endif
    }
}
