using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Maps+ factory seed rows: two-finger pan, pinch zoom, two-finger rotate,
    /// Option+two-finger orbit.
    /// </summary>
    /// <remarks>
    /// Each <see cref="StyleBindingRow"/> is built positionally, so the column order
    /// is a convention fixed by the <see cref="StyleBindingRow"/> constructor:
    /// <list type="number">
    ///   <item><c>op</c> — the <see cref="CameraOp"/> this row drives when it matches.</item>
    ///   <item><c>primitive</c> — the <see cref="StylePrimitive"/> gesture signal that
    ///   must exceed its deadband (Pinch → PinchDeadband, Rotate → RotateDeadband,
    ///   CentroidMotion → MotionDeadband).</item>
    ///   <item><c>minFingerCount</c> — fewest touching fingers allowed (inclusive).</item>
    ///   <item><c>maxFingerCount</c> — most touching fingers allowed (inclusive).</item>
    ///   <item><c>requiredModifiers</c> — <see cref="GestureModifiers"/> that must ALL be
    ///   held (<c>None</c> = no requirement).</item>
    ///   <item><c>forbiddenModifiers</c> — <see cref="GestureModifiers"/> of which NONE may
    ///   be held (<c>None</c> = nothing forbidden).</item>
    /// </list>
    /// Row order does not matter: <see cref="StyleBindingResolver.ResolveCandidates"/>
    /// ORs together every matching row's op, and
    /// <see cref="StyleBindingResolver.PickPrimary"/> plus the Exclusive* tie-breakers
    /// decide which op wins.
    /// </remarks>
    public static class MapsPlusSeed
    {
        public static StyleBindingTable CreateTable()
        {
            return new StyleBindingTable(
                new[]
                {
                    // Pinch with exactly two fingers zooms, regardless of modifiers.
                    new StyleBindingRow(
                        CameraOp.Zoom, // op
                        StylePrimitive.Pinch, // primitive
                        2, // minFingerCount
                        2, // maxFingerCount
                        GestureModifiers.None, // requiredModifiers
                        GestureModifiers.None // forbiddenModifiers
                    ),
                    // Two-finger twist rotates, regardless of modifiers.
                    new StyleBindingRow(
                        CameraOp.Rotate, // op
                        StylePrimitive.Rotate, // primitive
                        2, // minFingerCount
                        2, // maxFingerCount
                        GestureModifiers.None, // requiredModifiers
                        GestureModifiers.None // forbiddenModifiers
                    ),
                    // Option held + two-finger drag orbits.
                    new StyleBindingRow(
                        CameraOp.Orbit, // op
                        StylePrimitive.CentroidMotion, // primitive
                        2, // minFingerCount
                        2, // maxFingerCount
                        GestureModifiers.Option, // requiredModifiers
                        GestureModifiers.None // forbiddenModifiers
                    ),
                    // Plain two-finger drag pans; Option is forbidden so it does not
                    // overlap with the orbit row above.
                    new StyleBindingRow(
                        CameraOp.Pan, // op
                        StylePrimitive.CentroidMotion, // primitive
                        2, // minFingerCount
                        2, // maxFingerCount
                        GestureModifiers.None, // requiredModifiers
                        GestureModifiers.Option // forbiddenModifiers
                    ),
                }
            );
        }
    }
}
