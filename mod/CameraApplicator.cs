using System;

namespace TrackpadCameraControl
{
    public static class CameraApplicator
    {
        private static readonly CameraControllerZoom DefaultCamera = new CameraControllerZoom();
        private const float Deg2Rad = (float)(Math.PI / 180.0);

        public static void Apply(
            CameraOp ops,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings
        )
        {
            Apply(ops, dx, dy, pinchDelta, rotateDelta, settings, DefaultCamera);
        }

        public static void Apply(
            CameraOp ops,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings,
            ICameraController camera
        )
        {
            if (ops == CameraOp.None || settings == null || camera == null)
            {
                return;
            }

            if ((ops & CameraOp.Zoom) != 0)
            {
                ApplyZoom(pinchDelta, settings, camera);
            }

            if ((ops & CameraOp.Pan) != 0)
            {
                ApplyPan(dx, dy, settings, camera);
            }

            if ((ops & CameraOp.Orbit) != 0)
            {
                ApplyOrbit(dx, dy, settings, camera);
            }

            if ((ops & CameraOp.Yaw) != 0)
            {
                ApplyYawRotate(rotateDelta, settings, camera);
            }
        }

        private static void ApplyZoom(
            float pinchDelta,
            ModSettings settings,
            ICameraController camera
        )
        {
            float size = camera.Size;
            if (float.IsNaN(size))
            {
                return;
            }

            float delta = pinchDelta * settings.ZoomSensitivity;
            if (settings.InvertZoom)
            {
                delta = -delta;
            }

            // Pinch out (positive scale delta) → zoom in (smaller size).
            float next = size * (1f - delta);
            if (next < 10f)
            {
                next = 10f;
            }

            if (next > 5000f)
            {
                next = 5000f;
            }

            camera.Size = next;
        }

        private static void ApplyPan(
            float dx,
            float dy,
            ModSettings settings,
            ICameraController camera
        )
        {
            float x = camera.TargetX;
            float z = camera.TargetZ;
            if (float.IsNaN(x) || float.IsNaN(z))
            {
                return;
            }

            float mx = dx * settings.PanSensitivityX;
            float my = dy * settings.PanSensitivityY;
            if (settings.InvertPanX)
            {
                mx = -mx;
            }

            if (settings.InvertPanY)
            {
                my = -my;
            }

            float size = camera.Size;
            if (!float.IsNaN(size))
            {
                mx *= size;
                my *= size;
            }

            float yaw = camera.AngleX;
            if (float.IsNaN(yaw))
            {
                yaw = 0f;
            }

            float rad = yaw * Deg2Rad;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            // Camera-relative XZ: right * mx + forward * my
            camera.TargetX = x + cos * mx + sin * my;
            camera.TargetZ = z + -sin * mx + cos * my;
        }

        private static void ApplyOrbit(
            float dx,
            float dy,
            ModSettings settings,
            ICameraController camera
        )
        {
            float yaw = camera.AngleX;
            float pitch = camera.AngleY;
            if (float.IsNaN(yaw) || float.IsNaN(pitch))
            {
                return;
            }

            float dyaw = dx * settings.OrbitYawSensitivity;
            float dpitch = dy * settings.OrbitPitchSensitivity;
            if (settings.InvertOrbitYaw)
            {
                dyaw = -dyaw;
            }

            if (settings.InvertOrbitPitch)
            {
                dpitch = -dpitch;
            }

            camera.AngleX = yaw + dyaw;
            camera.AngleY = pitch + dpitch;
        }

        private static void ApplyYawRotate(
            float rotateDelta,
            ModSettings settings,
            ICameraController camera
        )
        {
            float yaw = camera.AngleX;
            if (float.IsNaN(yaw))
            {
                return;
            }

            float delta = rotateDelta * settings.YawRotateSensitivity;
            if (settings.InvertYawRotate)
            {
                delta = -delta;
            }

            camera.AngleX = yaw + delta;
        }
    }
}
