namespace TrackpadCameraControl
{
    /// <summary>
    /// Per-contact-session state: orbit latch and SessionLock ownership.
    /// </summary>
    public sealed class GestureSession
    {
        public bool OrbitLatched { get; private set; }

        public CameraOp LockedOp { get; private set; }

        public void Reset()
        {
            OrbitLatched = false;
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
                && GestureBindingResolver.IsOrbitTriggerActive(frame, settings)
            )
            {
                OrbitLatched = true;
            }

            CameraOp candidates = GestureBindingResolver.ResolveCandidates(
                frame,
                settings,
                OrbitLatched
            );

            if (OrbitLatched)
            {
                // Orbit owns the latched session. Yaw was previously allowed too, but Option-drag
                // often emits twist noise → Orbit yaw + Yaw in one frame = jumping AngleX.
                candidates &= CameraOp.Orbit;
            }

            // After latch masking: pinch vs twist stay exclusive when both remain.
            candidates = GestureBindingResolver.ExclusiveZoomVersusYaw(candidates, frame, settings);
            candidates = GestureBindingResolver.ExclusiveOrbitVersusYaw(candidates);

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
                return GestureBindingResolver.PickPrimary(candidates);
            }

            // SessionLock
            if (LockedOp != CameraOp.None)
            {
                return candidates & LockedOp;
            }

            if (frame.phase == (int)GesturePhase.Began || candidates != CameraOp.None)
            {
                LockedOp = GestureBindingResolver.PickPrimary(candidates);
                return LockedOp;
            }

            return CameraOp.None;
        }
    }
}
