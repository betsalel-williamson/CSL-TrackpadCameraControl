using System;
using System.Collections.Generic;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Builds op-heading copy with live keymapping labels from Cities Options bindings.
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

        /// <summary>
        /// Third op-heading line. Prefer <c>Keymapping(s): {bindings}</c>;
        /// when unbound, still name the concept (<c>Keymapping(s): none</c>).
        /// </summary>
        internal static string FormatVanillaActionLine(string bindings)
        {
            if (string.IsNullOrEmpty(bindings))
            {
                return "Keymapping(s): none";
            }

            return "Keymapping(s): " + bindings;
        }

        private static string BuildDescription(
            string meaning,
            string mapsPlus,
            string keymappingBindings
        )
        {
            return meaning + "\n" + mapsPlus + "\n" + FormatVanillaActionLine(keymappingBindings);
        }

        private static string BuildHeading(
            string title,
            string meaning,
            string mapsPlus,
            string keymappingBindings
        )
        {
            return title + "\n" + BuildDescription(meaning, mapsPlus, keymappingBindings);
        }

        public static string OpDescriptionZoom =>
            BuildDescription(
                "Change camera distance / size",
                "Pinch",
                ResolveZoomVanillaBindings()
            );

        public static string OpHeadingZoom => "Zoom\n" + OpDescriptionZoom;

        public static string OpDescriptionPan =>
            BuildDescription(
                "Slide the camera laterally",
                "Two-finger drag",
                ResolvePanVanillaBindings()
            );

        public static string OpHeadingPan => "Pan\n" + OpDescriptionPan;

        public static string OpDescriptionRotate =>
            BuildDescription(
                "Yaw the camera or rotate a place/relocate ghost",
                "Two-finger rotate",
                ResolveRotateVanillaBindings()
            );

        public static string OpHeadingRotate => "Rotate\n" + OpDescriptionRotate;

        public static string OpDescriptionOrbit =>
            BuildDescription(
                "Pitch + yaw around the pivot",
                "Option (⌥)+two-finger drag",
                ResolveOrbitVanillaBindings()
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
