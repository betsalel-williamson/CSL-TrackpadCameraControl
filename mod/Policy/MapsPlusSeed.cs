using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Maps+ factory seed rows: two-finger pan, pinch zoom, two-finger rotate,
    /// Option+two-finger orbit.
    /// </summary>
    public static class MapsPlusSeed
    {
        public static StyleBindingTable CreateTable()
        {
            return new StyleBindingTable(
                new[]
                {
                    new StyleBindingRow(
                        CameraOp.Zoom,
                        StylePrimitive.Pinch,
                        2,
                        2,
                        GestureModifiers.None,
                        GestureModifiers.None
                    ),
                    new StyleBindingRow(
                        CameraOp.Rotate,
                        StylePrimitive.Rotate,
                        2,
                        2,
                        GestureModifiers.None,
                        GestureModifiers.None
                    ),
                    new StyleBindingRow(
                        CameraOp.Orbit,
                        StylePrimitive.CentroidMotion,
                        2,
                        2,
                        GestureModifiers.Option,
                        GestureModifiers.None
                    ),
                    new StyleBindingRow(
                        CameraOp.Pan,
                        StylePrimitive.CentroidMotion,
                        2,
                        2,
                        GestureModifiers.None,
                        GestureModifiers.Option
                    ),
                }
            );
        }
    }
}
