using System;
using System.Collections.Generic;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Builds op-heading copy with live gesture bindings (settings) and keymapping labels.
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
        /// Keymapping line. Prefer <c>Keymapping(s): {bindings}</c>;
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

        /// <summary>
        /// Gesture line from a settings binding pair.
        /// </summary>
        internal static string FormatGestureLine(TrackpadGestureBinding binding)
        {
            string label = TrackpadGestureCatalog.ToDisplayLabel(binding);
            if (string.IsNullOrEmpty(label))
            {
                return "Gesture(s): none";
            }

            return "Gesture(s): " + label;
        }

        /// <summary>
        /// Gesture line for an op; when orbit trigger is Both, joins Maps+ and CAD orbit labels.
        /// </summary>
        internal static string FormatGestureLineForOp(ModSettings settings, CameraOp op)
        {
            if (
                op == CameraOp.Orbit
                && settings != null
                && settings.OrbitTrigger == OrbitTrigger.Both
            )
            {
                string a = TrackpadGestureCatalog.ToDisplayLabel(
                    TrackpadGestureCatalog.MapsPlusOrbit
                );
                string b = TrackpadGestureCatalog.ToDisplayLabel(TrackpadGestureCatalog.CadOrbit);
                string joined = JoinBindingLabels(new[] { a, b });
                if (string.IsNullOrEmpty(joined))
                {
                    return "Gesture(s): none";
                }

                return "Gesture(s): " + joined;
            }

            return FormatGestureLine(TrackpadGestureCatalog.GetBinding(settings, op));
        }

        private static string BuildDescription(
            string meaning,
            string gestureLine,
            string keymappingBindings
        )
        {
            return meaning
                + "\n"
                + gestureLine
                + "\n"
                + FormatVanillaActionLine(keymappingBindings);
        }

        private static ModSettings LabelSettings()
        {
            try
            {
                return Mod.EnsureSettings() ?? ModSettings.CreateFactoryDefaults();
            }
            catch
            {
                return ModSettings.CreateFactoryDefaults();
            }
        }

        public static string OpDescriptionZoom =>
            BuildDescription(
                "Change camera distance / size",
                FormatGestureLineForOp(LabelSettings(), CameraOp.Zoom),
                ResolveZoomVanillaBindings()
            );

        public static string OpHeadingZoom => "Zoom\n" + OpDescriptionZoom;

        public static string OpDescriptionPan =>
            BuildDescription(
                "Slide the camera laterally",
                FormatGestureLineForOp(LabelSettings(), CameraOp.Pan),
                ResolvePanVanillaBindings()
            );

        public static string OpHeadingPan => "Pan\n" + OpDescriptionPan;

        public static string OpDescriptionRotate =>
            BuildDescription(
                "Rotate the camera or a place/relocate ghost",
                FormatGestureLineForOp(LabelSettings(), CameraOp.Rotate),
                ResolveRotateVanillaBindings()
            );

        public static string OpHeadingRotate => "Rotate\n" + OpDescriptionRotate;

        public static string OpDescriptionOrbit =>
            BuildDescription(
                "Pitch + yaw around the pivot",
                FormatGestureLineForOp(LabelSettings(), CameraOp.Orbit),
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
