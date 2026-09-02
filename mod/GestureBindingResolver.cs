using System;

namespace TrackpadCameraControl
{
    [Flags]
    public enum CameraOp
    {
        None = 0,
        Pan = 1 << 0,
        Zoom = 1 << 1,

        /// <summary>Product <b>Rotate</b> op (two-finger twist). Not Orbit yaw/pitch.</summary>
        Rotate = 1 << 2,

        /// <summary>Orbit drag — applies yaw + pitch around the pivot.</summary>
        Orbit = 1 << 3,
    }

    public static class GestureBindingResolver
    {
        /// <summary>
        /// Candidate ops from a single frame (ignores resolve mode and orbit latch filtering).
        /// </summary>
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

            CameraOp ops = CameraOp.None;

            if (settings.ZoomEnabled && Abs(frame.pinchScaleDelta) > settings.PinchDeadband)
            {
                ops |= CameraOp.Zoom;
            }

            if (settings.YawEnabled && Abs(frame.rotateDelta) > settings.YawDeadband)
            {
                ops |= CameraOp.Rotate;
            }

            bool motion =
                Abs(frame.centroidDeltaX) > settings.MotionDeadband
                || Abs(frame.centroidDeltaY) > settings.MotionDeadband;

            bool orbitTrigger = orbitLatched || IsOrbitTriggerActive(frame, settings);
            if (settings.OrbitEnabled && orbitTrigger && (motion || orbitLatched))
            {
                ops |= CameraOp.Orbit;
            }

            if (
                settings.PanEnabled
                && motion
                && !orbitTrigger
                && frame.fingerCount >= 2
                && frame.fingerCount < 3
            )
            {
                ops |= CameraOp.Pan;
            }

            return ops;
        }

        /// <summary>
        /// When both zoom and rotate qualify, keep the stronger signal so they never apply together.
        /// </summary>
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
            float rotateDeadband = settings.YawDeadband > 1e-8f ? settings.YawDeadband : 0.001f;
            float zoomScore = Abs(frame.pinchScaleDelta) / pinchDeadband;
            float rotateScore = Abs(frame.rotateDelta) / rotateDeadband;

            if (zoomScore >= rotateScore)
            {
                return ops & ~CameraOp.Rotate;
            }

            return ops & ~CameraOp.Zoom;
        }

        /// <summary>
        /// Orbit centroid drag already writes camera yaw (<c>AngleX</c>). Concurrent twist can
        /// double-write — but if twist dominates centroid motion, prefer <see cref="CameraOp.Rotate"/>
        /// and drop Orbit so rotate does not steal scroll <c>dy</c> into pitch.
        /// </summary>
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

        /// <summary>Backward-compatible overload: prefer orbit (drop rotate) when both present.</summary>
        public static CameraOp ExclusiveOrbitVersusRotate(CameraOp ops)
        {
            return ExclusiveOrbitVersusRotate(ops, default(GestureFrame), null);
        }

        /// <summary>
        /// True when two-finger twist outweighs centroid travel (rotate intent vs pan/orbit drag).
        /// </summary>
        public static bool IsTwistDominant(GestureFrame frame, ModSettings settings)
        {
            if (settings == null)
            {
                return Abs(frame.rotateDelta) > 1e-6f
                    && Abs(frame.rotateDelta) >= Abs(frame.centroidDeltaX)
                    && Abs(frame.rotateDelta) >= Abs(frame.centroidDeltaY);
            }

            float rotateDeadband = settings.YawDeadband > 1e-8f ? settings.YawDeadband : 0.001f;
            float dead = settings.MotionDeadband > 1e-8f ? settings.MotionDeadband : 0.001f;
            float rotateScore = Abs(frame.rotateDelta) / rotateDeadband;
            float motionScore = Max(Abs(frame.centroidDeltaX), Abs(frame.centroidDeltaY)) / dead;
            return rotateScore > 0f && rotateScore >= motionScore;
        }

        private static float Max(float a, float b)
        {
            return a >= b ? a : b;
        }

        public static bool IsOrbitTriggerActive(GestureFrame frame, ModSettings settings)
        {
            if (settings == null || !settings.OrbitEnabled)
            {
                return false;
            }

            OrbitTrigger trigger = settings.OrbitTrigger;
            if (trigger == OrbitTrigger.Off)
            {
                return false;
            }

            bool motion =
                Abs(frame.centroidDeltaX) > settings.MotionDeadband
                || Abs(frame.centroidDeltaY) > settings.MotionDeadband;
            if (!motion && frame.phase != (int)GesturePhase.Began)
            {
                // Allow engage on Began with modifier even before first delta samples.
            }

            bool modifierTwo =
                frame.fingerCount >= 2
                && frame.fingerCount < 3
                && (frame.modifiers & (uint)GestureModifiers.Option) != 0;

            bool threeFinger = frame.fingerCount >= 3;

            if (trigger == OrbitTrigger.ModifierPlusTwoFinger)
            {
                return modifierTwo;
            }

            if (trigger == OrbitTrigger.ThreeFinger)
            {
                return threeFinger;
            }

            if (trigger == OrbitTrigger.Both)
            {
                return modifierTwo || threeFinger;
            }

            return false;
        }

        /// <summary>PrimaryOnly priority: Orbit > Zoom > Rotate > Pan.</summary>
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

        private static float Abs(float v)
        {
            return v < 0f ? -v : v;
        }
    }
}
