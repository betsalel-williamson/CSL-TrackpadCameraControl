namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Camera seam for apply (production or test fake): size, target, yaw/pitch.</summary>
    public interface ICameraController
    {
        float Size { get; set; }
        float TargetX { get; set; }
        float TargetY { get; set; }
        float TargetZ { get; set; }

        float AngleX { get; set; }
        float AngleY { get; set; }

        void ClampPanTarget(ref float x, ref float z);

        /// <summary>
        /// Queue yaw/pitch for middle-mouse-style orbit. Does not change AngleX/Y.
        /// </summary>
        void AddAngleVelocity(float yawDelta, float pitchDelta);

        void FlushPendingAngleVelocity(float deltaTimeSeconds);

        void ClearPendingAngleVelocity();

        void ClearAngleVelocity(bool yaw, bool pitch);
    }
}
