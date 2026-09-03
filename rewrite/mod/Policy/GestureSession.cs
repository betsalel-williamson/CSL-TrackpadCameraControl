namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Per-contact-session state: orbit latch, rotate ownership, and SessionLock.
    /// Resolve candidates come from the style binding table only (ADR 0004).
    /// </summary>
    public sealed class GestureSession
    {
        public bool OrbitLatched { get; private set; }

        /// <summary>
        /// True after a two-finger rotation starts this contact while orbit is not latched.
        /// Companion ScrollWheel must not pan/orbit for the rest of the contact.
        /// </summary>
        public bool RotateOwned { get; private set; }

        public CameraOp LockedOp { get; private set; }

        public void Reset()
        {
            OrbitLatched = false;
            RotateOwned = false;
            LockedOp = CameraOp.None;
        }

        /// <summary>
        /// Update latch / lock from the frame and return the op set to apply.
        /// </summary>
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

            // Rotation ownership only when Option-orbit is not owning the contact
            // (Option held → rotate ignored is expected).
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
                // Orbit owns the latched session. Twist noise must not jump AngleX via rotation.
                candidates &= CameraOp.Orbit;
            }
            else if (RotateOwned)
            {
                // Rotate-owned: companion ScrollWheel must not refill orbit pending / pan.
                candidates &= ~(CameraOp.Pan | CameraOp.Orbit);
            }

            // After latch masking: pinch vs twist stay exclusive when both remain.
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

            // SessionLock
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
