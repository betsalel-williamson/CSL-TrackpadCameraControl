using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Pure relocate render-angle delta for BuildingTool ghost twist.
    /// Place copies m_angle → m_mouseAngle in SimulationStep; relocate render uses
    /// m_mouseAngle / m_cachedAngle and often ignores m_angle.
    /// </summary>
    public static class BuildingToolAngleDelta
    {
        public const float Deg2Rad = (float)(Math.PI / 180.0);

        /// <summary>
        /// Apply degrees delta to mouse/cached radian angles and mark angleChanged.
        /// </summary>
        public static void ApplyRenderAngles(
            float deltaDegrees,
            ref float mouseAngleRadians,
            ref float cachedAngleRadians,
            out bool angleChanged
        )
        {
            float deltaRad = deltaDegrees * Deg2Rad;
            mouseAngleRadians += deltaRad;
            cachedAngleRadians += deltaRad;
            angleChanged = true;
        }

        /// <summary>Degrees → radians conversion used by the Cities adapter.</summary>
        public static float DegreesToRadians(float deltaDegrees)
        {
            return deltaDegrees * Deg2Rad;
        }
    }
}
