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
        /// City pan bounds on XZ. <see cref="float.NaN"/> means unavailable — skip clamp.
        /// </summary>
        float MinX { get; }

        float MaxX { get; }
        float MinZ { get; }
        float MaxZ { get; }
    }
}
