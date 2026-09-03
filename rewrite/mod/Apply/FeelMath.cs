using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Pure feel math: op + feel → camera deltas. No Unity / Cities types.
    /// </summary>
    public static class FeelMath
    {
        private const float Deg2Rad = (float)(Math.PI / 180.0);
        private const float VanillaPitchMin = 0f;
        private const float VanillaPitchMax = 90f;

        public enum InputModality
        {
            Drag,
            Button,
        }

        /// <summary>Three-decimal round for product numeric apply/display.</summary>
        public static float RoundGain(float value)
        {
            return (float)Math.Round(value, 3, MidpointRounding.AwayFromZero);
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
            Apply(ops, dx, dy, pinchDelta, rotateDelta, settings, camera, InputModality.Drag, null);
        }

        public static void ApplyButton(
            CameraOp ops,
            float dxSign,
            float dySign,
            float pinchSign,
            float rotateSign,
            ModSettings settings,
            ICameraController camera
        )
        {
            if (ops == CameraOp.None || settings == null || camera == null)
            {
                return;
            }

            float dx = 0f;
            float dy = 0f;
            float pinch = 0f;
            float rotate = 0f;

            if ((ops & CameraOp.Pan) != 0)
            {
                dx = dxSign * settings.PanStepX;
                dy = dySign * settings.PanStepY;
            }

            if ((ops & CameraOp.Orbit) != 0)
            {
                dx = dxSign * settings.OrbitYawStep;
                dy = dySign * settings.OrbitPitchStep;
            }

            if ((ops & CameraOp.Zoom) != 0)
            {
                pinch = pinchSign * settings.ZoomStep;
            }

            if ((ops & CameraOp.Rotate) != 0)
            {
                rotate = rotateSign * settings.RotateStep;
            }

            Apply(ops, dx, dy, pinch, rotate, settings, camera, InputModality.Button, null);
        }

        public static void Apply(
            CameraOp ops,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings,
            ICameraController camera,
            ISelectionContext selection
        )
        {
            Apply(
                ops,
                dx,
                dy,
                pinchDelta,
                rotateDelta,
                settings,
                camera,
                InputModality.Drag,
                selection
            );
        }

        public static void Apply(
            CameraOp ops,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings,
            ICameraController camera,
            InputModality modality,
            ISelectionContext selection
        )
        {
            if (ops == CameraOp.None || settings == null || camera == null)
            {
                return;
            }

            if ((ops & CameraOp.Rotate) != 0)
            {
                ops &= ~CameraOp.Orbit;
            }

            if ((ops & CameraOp.Zoom) != 0)
            {
                ApplyZoom(pinchDelta, settings, camera, modality);
            }

            if ((ops & CameraOp.Pan) != 0)
            {
                ApplyPan(dx, dy, settings, camera, modality);
            }

            if ((ops & CameraOp.Orbit) != 0)
            {
                ApplyOrbit(dx, dy, settings, camera, modality);
            }

            if ((ops & CameraOp.Rotate) != 0)
            {
                ApplyRotate(rotateDelta, settings, camera, modality, selection);
            }
        }

        private static void ApplyZoom(
            float pinchDelta,
            ModSettings settings,
            ICameraController camera,
            InputModality modality
        )
        {
            float size = camera.Size;
            if (float.IsNaN(size))
            {
                return;
            }

            float delta =
                modality == InputModality.Button ? pinchDelta : pinchDelta * settings.ZoomGain;
            if (settings.SignInvertZoom)
            {
                delta = -delta;
            }

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
            ICameraController camera,
            InputModality modality
        )
        {
            float x = camera.TargetX;
            float z = camera.TargetZ;
            if (float.IsNaN(x) || float.IsNaN(z))
            {
                return;
            }

            float mx = modality == InputModality.Button ? dx : dx * settings.PanGainX;
            float my = modality == InputModality.Button ? dy : dy * settings.PanGainY;
            if (settings.SignInvertPanX)
            {
                mx = -mx;
            }

            if (settings.SignInvertPanY)
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

            float nextX = x + cos * mx + sin * my;
            float nextZ = z + -sin * mx + cos * my;
            camera.ClampPanTarget(ref nextX, ref nextZ);
            camera.TargetX = nextX;
            camera.TargetZ = nextZ;
        }

        private static void ApplyOrbit(
            float dx,
            float dy,
            ModSettings settings,
            ICameraController camera,
            InputModality modality
        )
        {
            float yaw = camera.AngleX;
            float pitch = camera.AngleY;
            if (float.IsNaN(yaw) || float.IsNaN(pitch))
            {
                return;
            }

            float dyaw = modality == InputModality.Button ? dx : dx * settings.OrbitYawGain;
            float dpitch = modality == InputModality.Button ? dy : dy * settings.OrbitPitchGain;
            if (settings.SignInvertOrbitYaw)
            {
                dyaw = -dyaw;
            }

            if (settings.SignInvertOrbitPitch)
            {
                dpitch = -dpitch;
            }

            if (modality != InputModality.Button)
            {
                if (pitch <= VanillaPitchMin && dpitch < 0f)
                {
                    dpitch = 0f;
                }

                camera.AddAngleVelocity(dyaw, dpitch);
                return;
            }

            camera.AngleX = yaw + dyaw;

            float nextPitch = pitch + dpitch;
            if (nextPitch < VanillaPitchMin)
            {
                nextPitch = VanillaPitchMin;
            }
            else if (nextPitch > VanillaPitchMax)
            {
                nextPitch = VanillaPitchMax;
            }

            camera.AngleY = nextPitch;
        }

        private static void ApplyRotate(
            float rotateDelta,
            ModSettings settings,
            ICameraController camera,
            InputModality modality,
            ISelectionContext selection
        )
        {
            float delta =
                modality == InputModality.Button ? rotateDelta : rotateDelta * settings.RotateGain;
            if (settings.SignInvertRotate)
            {
                delta = -delta;
            }

            if (selection != null && selection.TryApplyObjectYawDelta(delta))
            {
                camera.ClearAngleVelocity(yaw: true, pitch: true);
                return;
            }

            float yaw = camera.AngleX;
            if (float.IsNaN(yaw))
            {
                return;
            }

            camera.ClearAngleVelocity(yaw: true, pitch: true);
            camera.AngleX = yaw + delta;
        }
    }

    /// <summary>Apply facade matching golden fixture call sites; delegates to <see cref="FeelMath"/>.</summary>
    public static class CameraApplicator
    {
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
            FeelMath.Apply(ops, dx, dy, pinchDelta, rotateDelta, settings, camera);
        }

        public static void Apply(
            CameraOp ops,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings,
            ICameraController camera,
            ISelectionContext selection
        )
        {
            FeelMath.Apply(ops, dx, dy, pinchDelta, rotateDelta, settings, camera, selection);
        }
    }
}
