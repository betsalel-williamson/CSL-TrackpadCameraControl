#if ENABLE_CAD_GESTURE_STYLE
namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// CAD style seed (three-finger orbit). Compiled only when EnableCadGestureStyle is on.
    /// Requires Capture to emit honest three-finger counts.
    /// </summary>
    public static class CadSeed
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
                        3,
                        3,
                        GestureModifiers.None,
                        GestureModifiers.None
                    ),
                    new StyleBindingRow(
                        CameraOp.Pan,
                        StylePrimitive.CentroidMotion,
                        2,
                        2,
                        GestureModifiers.None,
                        GestureModifiers.None
                    ),
                }
            );
        }
    }
}
#endif
