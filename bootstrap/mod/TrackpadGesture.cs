namespace TrackpadCameraControl
{
    /// <summary>
    /// Atomic trackpad contact patterns for assignment + labels.
    /// Player UX names (Pinch / Two-finger drag); AppKit Magnify/Scroll/Rotate, Windows PTP,
    /// and GTK pinch/swipe map here. Drag = continuous deltas; Swipe = discrete flick (catalog).
    /// </summary>
    public enum TrackpadGesture
    {
        None = 0,

        /// <summary>Continuous two-finger pan (AppKit scroll / scrollingDelta).</summary>
        TwoFingerDrag = 1,

        /// <summary>Pinch zoom (AppKit Magnify / GtkGestureZoom).</summary>
        Pinch = 2,

        /// <summary>Two-finger twist (AppKit Rotate / GtkGestureRotate).</summary>
        TwoFingerRotate = 3,

        /// <summary>Continuous three-finger drag (CAD orbit; Win 3-finger manipulations).</summary>
        ThreeFingerDrag = 4,

        /// <summary>Catalog stub — Win/GTK four-finger manipulations.</summary>
        FourFingerDrag = 5,

        /// <summary>Catalog stub — AppKit discrete swipe (not continuous drag).</summary>
        TwoFingerSwipe = 6,

        /// <summary>Catalog stub — discrete three-finger swipe.</summary>
        ThreeFingerSwipe = 7,

        /// <summary>Catalog stub — two-finger tap.</summary>
        TwoFingerTap = 8,

        /// <summary>Catalog stub — three-finger tap (Win Actions).</summary>
        ThreeFingerTap = 9,
    }

    /// <summary>
    /// Optional chord for a <see cref="TrackpadGestureBinding"/>. Aligns with
    /// <see cref="GestureModifiers"/> flags (Option = macOS ⌥ / Alt).
    /// </summary>
    public enum GestureModifierKey
    {
        None = 0,
        Option = 1,
        Shift = 2,
        Command = 3,
        Control = 4,
    }

    /// <summary>Composable activation: base gesture + optional modifier.</summary>
    public struct TrackpadGestureBinding
    {
        public TrackpadGesture Gesture;
        public GestureModifierKey Modifier;

        public TrackpadGestureBinding(TrackpadGesture gesture, GestureModifierKey modifier)
        {
            Gesture = gesture;
            Modifier = modifier;
        }

        public static TrackpadGestureBinding None =>
            new TrackpadGestureBinding(TrackpadGesture.None, GestureModifierKey.None);

        public bool IsNone => Gesture == TrackpadGesture.None;
    }

    /// <summary>Display labels and Maps+/CAD default seed tables for gesture bindings.</summary>
    public static class TrackpadGestureCatalog
    {
        public static string ToDisplayLabel(TrackpadGesture gesture)
        {
            switch (gesture)
            {
                case TrackpadGesture.TwoFingerDrag:
                    return "Two-finger drag";
                case TrackpadGesture.Pinch:
                    return "Pinch";
                case TrackpadGesture.TwoFingerRotate:
                    return "Two-finger rotate";
                case TrackpadGesture.ThreeFingerDrag:
                    return "Three-finger drag";
                case TrackpadGesture.FourFingerDrag:
                    return "Four-finger drag";
                case TrackpadGesture.TwoFingerSwipe:
                    return "Two-finger swipe";
                case TrackpadGesture.ThreeFingerSwipe:
                    return "Three-finger swipe";
                case TrackpadGesture.TwoFingerTap:
                    return "Two-finger tap";
                case TrackpadGesture.ThreeFingerTap:
                    return "Three-finger tap";
                default:
                    return null;
            }
        }

        public static string ToDisplayLabel(GestureModifierKey modifier)
        {
            switch (modifier)
            {
                case GestureModifierKey.Option:
                    return "Option (⌥)";
                case GestureModifierKey.Shift:
                    return "Shift";
                case GestureModifierKey.Command:
                    return "Command (⌘)";
                case GestureModifierKey.Control:
                    return "Control";
                default:
                    return null;
            }
        }

        public static string ToDisplayLabel(TrackpadGestureBinding binding)
        {
            if (binding.IsNone)
            {
                return null;
            }

            string gesture = ToDisplayLabel(binding.Gesture);
            if (string.IsNullOrEmpty(gesture))
            {
                return null;
            }

            if (binding.Modifier == GestureModifierKey.None)
            {
                return gesture;
            }

            string mod = ToDisplayLabel(binding.Modifier);
            if (string.IsNullOrEmpty(mod))
            {
                return gesture;
            }

            return mod + "+" + gesture.ToLowerInvariant();
        }

        public static TrackpadGestureBinding MapsPlusZoom =>
            new TrackpadGestureBinding(TrackpadGesture.Pinch, GestureModifierKey.None);

        public static TrackpadGestureBinding MapsPlusPan =>
            new TrackpadGestureBinding(TrackpadGesture.TwoFingerDrag, GestureModifierKey.None);

        public static TrackpadGestureBinding MapsPlusRotate =>
            new TrackpadGestureBinding(TrackpadGesture.TwoFingerRotate, GestureModifierKey.None);

        public static TrackpadGestureBinding MapsPlusOrbit =>
            new TrackpadGestureBinding(TrackpadGesture.TwoFingerDrag, GestureModifierKey.Option);

        public static TrackpadGestureBinding CadZoom => MapsPlusZoom;

        public static TrackpadGestureBinding CadPan => MapsPlusPan;

        public static TrackpadGestureBinding CadRotate => MapsPlusRotate;

        public static TrackpadGestureBinding CadOrbit =>
            new TrackpadGestureBinding(TrackpadGesture.ThreeFingerDrag, GestureModifierKey.None);

        public static void ApplyMapsPlusDefaults(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            SetBinding(settings, CameraOp.Zoom, MapsPlusZoom);
            SetBinding(settings, CameraOp.Pan, MapsPlusPan);
            SetBinding(settings, CameraOp.Rotate, MapsPlusRotate);
            SetBinding(settings, CameraOp.Orbit, MapsPlusOrbit);
            settings.OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger;
        }

        public static void ApplyCadDefaults(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            SetBinding(settings, CameraOp.Zoom, CadZoom);
            SetBinding(settings, CameraOp.Pan, CadPan);
            SetBinding(settings, CameraOp.Rotate, CadRotate);
            SetBinding(settings, CameraOp.Orbit, CadOrbit);
            settings.OrbitTrigger = OrbitTrigger.ThreeFinger;
        }

        public static TrackpadGestureBinding GetBinding(ModSettings settings, CameraOp op)
        {
            if (settings == null)
            {
                return TrackpadGestureBinding.None;
            }

            switch (op)
            {
                case CameraOp.Zoom:
                    return new TrackpadGestureBinding(
                        settings.ZoomGesture,
                        settings.ZoomGestureModifier
                    );
                case CameraOp.Pan:
                    return new TrackpadGestureBinding(
                        settings.PanGesture,
                        settings.PanGestureModifier
                    );
                case CameraOp.Rotate:
                    return new TrackpadGestureBinding(
                        settings.RotateGesture,
                        settings.RotateGestureModifier
                    );
                case CameraOp.Orbit:
                    return new TrackpadGestureBinding(
                        settings.OrbitGesture,
                        settings.OrbitGestureModifier
                    );
                default:
                    return TrackpadGestureBinding.None;
            }
        }

        public static void SetBinding(
            ModSettings settings,
            CameraOp op,
            TrackpadGestureBinding binding
        )
        {
            if (settings == null)
            {
                return;
            }

            switch (op)
            {
                case CameraOp.Zoom:
                    settings.ZoomGesture = binding.Gesture;
                    settings.ZoomGestureModifier = binding.Modifier;
                    break;
                case CameraOp.Pan:
                    settings.PanGesture = binding.Gesture;
                    settings.PanGestureModifier = binding.Modifier;
                    break;
                case CameraOp.Rotate:
                    settings.RotateGesture = binding.Gesture;
                    settings.RotateGestureModifier = binding.Modifier;
                    break;
                case CameraOp.Orbit:
                    settings.OrbitGesture = binding.Gesture;
                    settings.OrbitGestureModifier = binding.Modifier;
                    break;
            }
        }
    }
}
