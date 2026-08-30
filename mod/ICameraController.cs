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
        /// Add yaw/pitch into the camera angle velocity (same path as middle mouse button
        /// drag). Vanilla LateUpdate applies inertia and lerps current toward target.
        /// </summary>
        void AddAngleVelocity(float yawDelta, float pitchDelta);
    }
}
