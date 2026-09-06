using System;
using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    [Flags]
    public enum CameraOp
    {
        None = 0,
        Pan = 1 << 0,
        Zoom = 1 << 1,

        /// <summary>Product Rotate op (two-finger twist). Not Orbit yaw/pitch.</summary>
        Rotate = 1 << 2,

        /// <summary>Orbit drag — applies yaw + pitch around the pivot.</summary>
        Orbit = 1 << 3,
    }

    /// <summary>
    /// Policy resolve: match Capture primitives against the live style binding table only.
    /// </summary>
    public static class StyleBindingResolver
    {
        public static CameraOp ResolveCandidates(
            GestureFrame frame,
            ModSettings settings,
            bool orbitLatched
        )
        {
            if (settings == null)
            {
                return CameraOp.None;
            }

            StyleBindingTable table = settings.StyleTable;
            if (table == null)
            {
                return CameraOp.None;
            }

            CameraOp ops = CameraOp.None;
            for (int i = 0; i < table.Count; i++)
            {
                StyleBindingRow row = table[i];
                if (!IsOpEnabled(row.Op, settings))
                {
                    continue;
                }

                if (!RowMatches(row, frame, settings, orbitLatched))
                {
                    continue;
                }

                ops |= row.Op;
            }

            return ops;
        }

        public static CameraOp ExclusiveZoomVersusRotate(
            CameraOp ops,
            GestureFrame frame,
            ModSettings settings
        )
        {
            bool zoom = (ops & CameraOp.Zoom) != 0;
            bool rotate = (ops & CameraOp.Rotate) != 0;
            if (!zoom || !rotate || settings == null)
            {
                return ops;
            }

            float pinchDeadband = settings.PinchDeadband > 1e-8f ? settings.PinchDeadband : 0.001f;
            float rotateDeadband =
                settings.RotateDeadband > 1e-8f ? settings.RotateDeadband : 0.001f;
            float zoomScore = Abs(frame.pinchScaleDelta) / pinchDeadband;
            float rotateScore = Abs(frame.rotateDelta) / rotateDeadband;

            if (zoomScore >= rotateScore)
            {
                return ops & ~CameraOp.Rotate;
            }

            return ops & ~CameraOp.Zoom;
        }

        public static CameraOp ExclusiveOrbitVersusRotate(
            CameraOp ops,
            GestureFrame frame,
            ModSettings settings
        )
        {
            if ((ops & CameraOp.Orbit) == 0 || (ops & CameraOp.Rotate) == 0)
            {
                return ops;
            }

            if (IsTwistDominant(frame, settings))
            {
                return ops & ~CameraOp.Orbit;
            }

            return ops & ~CameraOp.Rotate;
        }

        public static bool IsTwistDominant(GestureFrame frame, ModSettings settings)
        {
            if (settings == null)
            {
                return Abs(frame.rotateDelta) > 1e-6f
                    && Abs(frame.rotateDelta) >= Abs(frame.centroidDeltaX)
                    && Abs(frame.rotateDelta) >= Abs(frame.centroidDeltaY);
            }

            float rotateDeadband =
                settings.RotateDeadband > 1e-8f ? settings.RotateDeadband : 0.001f;
            float dead = settings.MotionDeadband > 1e-8f ? settings.MotionDeadband : 0.001f;
            float rotateScore = Abs(frame.rotateDelta) / rotateDeadband;
            float motionScore = Max(Abs(frame.centroidDeltaX), Abs(frame.centroidDeltaY)) / dead;
            return rotateScore > 0f && rotateScore >= motionScore;
        }

        public static bool IsOrbitChordActive(GestureFrame frame, ModSettings settings)
        {
            if (settings == null || !settings.OrbitEnabled)
            {
                return false;
            }

            StyleBindingTable table = settings.StyleTable;
            if (table == null)
            {
                return false;
            }

            for (int i = 0; i < table.Count; i++)
            {
                StyleBindingRow row = table[i];
                if (row.Op != CameraOp.Orbit)
                {
                    continue;
                }

                if (RowMatches(row, frame, settings, orbitLatched: false))
                {
                    return true;
                }
            }

            return false;
        }

        public static CameraOp PickPrimary(CameraOp candidates)
        {
            if ((candidates & CameraOp.Orbit) != 0)
            {
                return CameraOp.Orbit;
            }

            if ((candidates & CameraOp.Zoom) != 0)
            {
                return CameraOp.Zoom;
            }

            if ((candidates & CameraOp.Rotate) != 0)
            {
                return CameraOp.Rotate;
            }

            if ((candidates & CameraOp.Pan) != 0)
            {
                return CameraOp.Pan;
            }

            return CameraOp.None;
        }

        private static bool RowMatches(
            StyleBindingRow row,
            GestureFrame frame,
            ModSettings settings,
            bool orbitLatched
        )
        {
            bool skipFingerAndMod = orbitLatched && row.Op == CameraOp.Orbit;
            if (!skipFingerAndMod)
            {
                if (!row.MatchesFingers(frame.fingerCount))
                {
                    return false;
                }

                if (!row.MatchesModifiers(frame.modifiers))
                {
                    return false;
                }
            }

            switch (row.Primitive)
            {
                case StylePrimitive.Pinch:
                    return Abs(frame.pinchScaleDelta) > settings.PinchDeadband;

                case StylePrimitive.Rotate:
                    return Abs(frame.rotateDelta) > settings.RotateDeadband;

                case StylePrimitive.CentroidMotion:
                {
                    bool motion =
                        Abs(frame.centroidDeltaX) > settings.MotionDeadband
                        || Abs(frame.centroidDeltaY) > settings.MotionDeadband;
                    if (row.Op == CameraOp.Orbit)
                    {
                        return motion || orbitLatched;
                    }

                    return motion;
                }

                default:
                    return false;
            }
        }

        private static bool IsOpEnabled(CameraOp op, ModSettings settings)
        {
            switch (op)
            {
                case CameraOp.Pan:
                    return settings.PanEnabled;
                case CameraOp.Zoom:
                    return settings.ZoomEnabled;
                case CameraOp.Rotate:
                    return settings.RotateEnabled;
                case CameraOp.Orbit:
                    return settings.OrbitEnabled;
                default:
                    return false;
            }
        }

        private static float Max(float a, float b)
        {
            return a >= b ? a : b;
        }

        private static float Abs(float v)
        {
            return v < 0f ? -v : v;
        }
    }
}
