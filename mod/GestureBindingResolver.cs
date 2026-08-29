using System;

namespace TrackpadCameraControl
{
    [Flags]
    public enum CameraOp
    {
        None = 0,
        Pan = 1 << 0,
        Zoom = 1 << 1,
        Yaw = 1 << 2,
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

            if (settings.ZoomEnabled && Abs(frame.pinchScaleDelta) > settings.PinchEpsilon)
            {
                ops |= CameraOp.Zoom;
            }

            if (settings.YawEnabled && Abs(frame.rotateDelta) > settings.RotateEpsilon)
            {
                ops |= CameraOp.Yaw;
            }

            bool motion =
                Abs(frame.centroidDeltaX) > settings.MotionDeadzone
                || Abs(frame.centroidDeltaY) > settings.MotionDeadzone;

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
        /// When both zoom and yaw qualify, keep the stronger signal so they never apply together.
        /// </summary>
        public static CameraOp ExclusiveZoomVersusYaw(
            CameraOp ops,
            GestureFrame frame,
            ModSettings settings
        )
        {
            bool zoom = (ops & CameraOp.Zoom) != 0;
            bool yaw = (ops & CameraOp.Yaw) != 0;
            if (!zoom || !yaw || settings == null)
            {
                return ops;
            }

            float pinchEps = settings.PinchEpsilon > 1e-8f ? settings.PinchEpsilon : 0.001f;
            float rotEps = settings.RotateEpsilon > 1e-8f ? settings.RotateEpsilon : 0.001f;
            float zoomScore = Abs(frame.pinchScaleDelta) / pinchEps;
            float yawScore = Abs(frame.rotateDelta) / rotEps;

            if (zoomScore >= yawScore)
            {
                return ops & ~CameraOp.Yaw;
            }

            return ops & ~CameraOp.Zoom;
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
                Abs(frame.centroidDeltaX) > settings.MotionDeadzone
                || Abs(frame.centroidDeltaY) > settings.MotionDeadzone;
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

        /// <summary>PrimaryOnly priority: Orbit > Zoom > Yaw > Pan.</summary>
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

            if ((candidates & CameraOp.Yaw) != 0)
            {
                return CameraOp.Yaw;
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
