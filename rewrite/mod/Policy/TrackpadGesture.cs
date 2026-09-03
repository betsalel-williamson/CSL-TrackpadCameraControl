using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    public enum TrackpadGesture
    {
        None = 0,
        TwoFingerDrag = 1,
        Pinch = 2,
        TwoFingerRotate = 3,
        ThreeFingerDrag = 4,
        FourFingerDrag = 5,
        TwoFingerSwipe = 6,
        ThreeFingerSwipe = 7,
        TwoFingerTap = 8,
        ThreeFingerTap = 9,
    }

    public enum GestureModifierKey
    {
        None = 0,
        Option = 1,
        Shift = 2,
        Command = 3,
        Control = 4,
    }

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

        public static TrackpadGestureBinding GetBinding(ModSettings settings, CameraOp op)
        {
            if (settings == null || settings.StyleTable == null)
            {
                return TrackpadGestureBinding.None;
            }

            StyleBindingTable table = settings.StyleTable;
            for (int i = 0; i < table.Count; i++)
            {
                StyleBindingRow row = table[i];
                if (row.Op != op)
                {
                    continue;
                }

                return RowToDisplayBinding(row);
            }

            return TrackpadGestureBinding.None;
        }

        private static TrackpadGestureBinding RowToDisplayBinding(StyleBindingRow row)
        {
            switch (row.Primitive)
            {
                case StylePrimitive.Pinch:
                    return MapsPlusZoom;
                case StylePrimitive.Rotate:
                    return MapsPlusRotate;
                case StylePrimitive.CentroidMotion:
                    if ((row.RequiredModifiers & GestureModifiers.Option) != 0)
                    {
                        return MapsPlusOrbit;
                    }

                    return MapsPlusPan;
                default:
                    return TrackpadGestureBinding.None;
            }
        }
    }
}
