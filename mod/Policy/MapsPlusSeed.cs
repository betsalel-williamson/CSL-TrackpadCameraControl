using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Maps+ factory seed rows: two-finger pan, pinch zoom, two-finger rotate,
    /// Option+two-finger orbit.
    /// </summary>
    /// <remarks>
    /// Rows pass every <see cref="StyleBindingRow"/> constructor argument by name so the
    /// compiler checks each value against its parameter. The columns mean:
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
                        op: CameraOp.Zoom,
                        primitive: StylePrimitive.Pinch,
                        minFingerCount: 2,
                        maxFingerCount: 2,
                        requiredModifiers: GestureModifiers.None,
                        forbiddenModifiers: GestureModifiers.None
                    ),
                    // Two-finger twist rotates, regardless of modifiers.
                    new StyleBindingRow(
                        op: CameraOp.Rotate,
                        primitive: StylePrimitive.Rotate,
                        minFingerCount: 2,
                        maxFingerCount: 2,
                        requiredModifiers: GestureModifiers.None,
                        forbiddenModifiers: GestureModifiers.None
                    ),
                    // Option held + two-finger drag orbits.
                    new StyleBindingRow(
                        op: CameraOp.Orbit,
                        primitive: StylePrimitive.CentroidMotion,
                        minFingerCount: 2,
                        maxFingerCount: 2,
                        requiredModifiers: GestureModifiers.Option,
                        forbiddenModifiers: GestureModifiers.None
                    ),
                    // Plain two-finger drag pans; Option is forbidden so it does not
                    // overlap with the orbit row above.
                    new StyleBindingRow(
                        op: CameraOp.Pan,
                        primitive: StylePrimitive.CentroidMotion,
                        minFingerCount: 2,
                        maxFingerCount: 2,
                        requiredModifiers: GestureModifiers.None,
                        forbiddenModifiers: GestureModifiers.Option
                    ),
                }
            );
        }
    }
}
