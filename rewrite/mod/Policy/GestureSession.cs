using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Per-contact-session state: orbit latch, rotate ownership, and SessionLock.
    /// </summary>
    public sealed class GestureSession
    {
        public bool OrbitLatched { get; private set; }

        public bool RotateOwned { get; private set; }

        public CameraOp LockedOp { get; private set; }

        public void Reset()
        {
            OrbitLatched = false;
            RotateOwned = false;
            LockedOp = CameraOp.None;
        }

        public CameraOp Process(GestureFrame frame, ModSettings settings)
        {
            if (settings == null)
            {
                return CameraOp.None;
            }

            if (
                frame.fingerCount <= 0
                || frame.phase == (int)GesturePhase.Ended
                || frame.phase == (int)GesturePhase.Cancelled
            )
            {
                Reset();
                return CameraOp.None;
            }

            if (
                !OrbitLatched
                && settings.OrbitEnabled
                && StyleBindingResolver.IsOrbitChordActive(frame, settings)
            )
            {
                OrbitLatched = true;
                RotateOwned = false;
            }

            if (
                !OrbitLatched
                && settings.RotateEnabled
                && Abs(frame.rotateDelta) > settings.RotateDeadband
            )
            {
                RotateOwned = true;
            }

            CameraOp candidates = StyleBindingResolver.ResolveCandidates(
                frame,
                settings,
                OrbitLatched
            );

            if (OrbitLatched)
            {
                candidates &= CameraOp.Orbit;
            }
            else if (RotateOwned)
            {
                candidates &= ~(CameraOp.Pan | CameraOp.Orbit);
            }

            candidates = StyleBindingResolver.ExclusiveZoomVersusRotate(
                candidates,
                frame,
                settings
            );
            candidates = StyleBindingResolver.ExclusiveOrbitVersusRotate(
                candidates,
                frame,
                settings
            );

            return ApplyResolveMode(frame, settings, candidates);
        }

        private CameraOp ApplyResolveMode(
            GestureFrame frame,
            ModSettings settings,
            CameraOp candidates
        )
        {
            GestureResolveMode mode = settings.GestureResolveMode;

            if (mode == GestureResolveMode.Concurrent)
            {
                return candidates;
            }

            if (mode == GestureResolveMode.PrimaryOnly)
            {
                return StyleBindingResolver.PickPrimary(candidates);
            }

            if (LockedOp != CameraOp.None)
            {
                return candidates & LockedOp;
            }

            if (frame.phase == (int)GesturePhase.Began || candidates != CameraOp.None)
            {
                LockedOp = StyleBindingResolver.PickPrimary(candidates);
                return LockedOp;
            }

            return CameraOp.None;
        }

        private static float Abs(float v)
        {
            return v < 0f ? -v : v;
        }
    }
}
