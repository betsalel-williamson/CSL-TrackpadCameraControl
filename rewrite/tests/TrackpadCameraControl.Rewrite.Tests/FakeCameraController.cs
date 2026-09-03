using System;
using TrackpadCameraControl.Rewrite;

namespace TrackpadCameraControl.Rewrite.Tests
{
    public delegate void PanClampAction(ref float x, ref float z);

    /// <summary>
    /// Camera double for tier A fixtures. Orbit velocity is queued only —
    /// never integrated inside <see cref="AddAngleVelocity"/> (L10 / harnesses).
    /// </summary>
    public sealed class FakeCameraController : ICameraController
    {
        public float Size { get; set; } = 100f;
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float TargetZ { get; set; }
        public float AngleX { get; set; }
        public float AngleY { get; set; }

        public float MinX { get; set; } = float.NaN;
        public float MaxX { get; set; } = float.NaN;
        public float MinZ { get; set; } = float.NaN;
        public float MaxZ { get; set; } = float.NaN;

        public PanClampAction ClampPanCustom { get; set; }

        public void ClampPanTarget(ref float x, ref float z)
        {
            if (ClampPanCustom != null)
            {
                ClampPanCustom(ref x, ref z);
                return;
            }

            if (float.IsNaN(MinX) || float.IsNaN(MaxX) || float.IsNaN(MinZ) || float.IsNaN(MaxZ))
            {
                return;
            }

            float minX = MinX;
            float maxX = MaxX;
            float minZ = MinZ;
            float maxZ = MaxZ;
            if (minX > maxX)
            {
                float swap = minX;
                minX = maxX;
                maxX = swap;
            }

            if (minZ > maxZ)
            {
                float swap = minZ;
                minZ = maxZ;
                maxZ = swap;
            }

            if (x < minX)
            {
                x = minX;
            }
            else if (x > maxX)
            {
                x = maxX;
            }

            if (z < minZ)
            {
                z = minZ;
            }
            else if (z > maxZ)
            {
                z = maxZ;
            }
        }

        public float AngleVelocityX { get; set; }

        public float AngleVelocityY { get; set; }

        public float PendingYaw { get; private set; }

        public float PendingPitch { get; private set; }

        public int AddAngleVelocityCallCount { get; private set; }

        /// <summary>
        /// Queue only — must not change AngleX/Y (same contract as production).
        /// </summary>
        public void AddAngleVelocity(float yawDelta, float pitchDelta)
        {
            AddAngleVelocityCallCount++;
            PendingYaw += yawDelta;
            PendingPitch += pitchDelta;
        }

        public void FlushPendingAngleVelocity(float deltaTimeSeconds)
        {
            float dt = deltaTimeSeconds;
            if (dt < 0.001f)
            {
                dt = 0.001f;
            }

            AngleVelocityX += PendingYaw / dt;
            AngleVelocityY += PendingPitch / dt;
            PendingYaw = 0f;
            PendingPitch = 0f;
        }

        public void ClearPendingAngleVelocity()
        {
            PendingYaw = 0f;
            PendingPitch = 0f;
        }

        public void ClearAngleVelocity(bool yaw, bool pitch)
        {
            if (yaw)
            {
                AngleVelocityX = 0f;
                PendingYaw = 0f;
            }

            if (pitch)
            {
                AngleVelocityY = 0f;
                PendingPitch = 0f;
            }
        }

        /// <summary>
        /// Mirror vanilla UpdateTargetPosition orbit order: damp → flush → integrate.
        /// </summary>
        public static void SimulateVanillaOrbitFrame(
            FakeCameraController cam,
            float inertia,
            float deltaTimeSeconds
        )
        {
            float dt = deltaTimeSeconds;
            if (dt < 0.001f)
            {
                dt = 0.001f;
            }

            float damp = (float)Math.Pow(inertia, dt);
            cam.AngleVelocityX *= damp;
            cam.AngleVelocityY *= damp;
            cam.FlushPendingAngleVelocity(dt);
            cam.AngleX += cam.AngleVelocityX * dt;
            float nextPitch = cam.AngleY + cam.AngleVelocityY * dt;
            if (nextPitch < 0f)
            {
                nextPitch = 0f;
            }
            else if (nextPitch > 90f)
            {
                nextPitch = 90f;
            }

            cam.AngleY = nextPitch;
        }
    }
}
