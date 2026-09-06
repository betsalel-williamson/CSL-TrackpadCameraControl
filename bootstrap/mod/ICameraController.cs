namespace TrackpadCameraControl
{
    /// <summary>Camera seam for apply (production or test fake): size, target, yaw/pitch.</summary>
    public interface ICameraController
    {
        float Size { get; set; }
        float TargetX { get; set; }
        float TargetY { get; set; }
        float TargetZ { get; set; }

        /// <summary>Yaw degrees (CameraController m_targetAngle.x).</summary>
        float AngleX { get; set; }

        /// <summary>Pitch degrees (CameraController m_targetAngle.y).</summary>
        float AngleY { get; set; }

        /// <summary>
        /// Clamp a proposed pan target to the playable / unlocked city area on XZ.
        /// Production uses GameAreaManager.ClampPoint (grows with unlocks; non-rectangular).
        /// Test fakes may use an AABB or a custom shape.
        /// </summary>
        void ClampPanTarget(ref float x, ref float z);

        /// <summary>
        /// Queue yaw/pitch for middle-mouse-style orbit. Does <b>not</b> change AngleX/Y.
        /// Production flushes into <c>m_angleVelocity</c> from a HandleMouseEvents Harmony postfix
        /// (after vanilla inertia damp, before integrate).
        /// </summary>
        void AddAngleVelocity(float yawDelta, float pitchDelta);

        /// <summary>
        /// Flush queued orbit deltas into angle velocity as <c>pending / max(dt, 0.001)</c>
        /// so <c>velocity * dt ≈ pending</c> this frame. Call from HandleMouseEvents postfix.
        /// </summary>
        void FlushPendingAngleVelocity(float deltaTimeSeconds);

        /// <summary>Discard queued orbit deltas (e.g. when the game loses focus).</summary>
        void ClearPendingAngleVelocity();

        /// <summary>
        /// Zero orbit angle velocity so a prior Option-orbit coast cannot keep changing
        /// heading or pitch during a two-finger <b>rotation</b> (hard handoff).
        /// </summary>
        void ClearAngleVelocity(bool yaw, bool pitch);
    }
}
