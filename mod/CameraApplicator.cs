using System;

namespace TrackpadCameraControl
{
    public static class CameraApplicator
    {
        private static readonly CameraControllerZoom DefaultCamera = new CameraControllerZoom();
        private const float Deg2Rad = (float)(Math.PI / 180.0);

        /// <summary>Pitch must stay &gt; 0 even if settings allow non-positive mins.</summary>
        private const float PitchEpsilon = 0.01f;

        public enum InputModality
        {
            Drag,
            Button,
        }

        public static void Apply(
            CameraOp ops,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings
        )
        {
            Apply(ops, dx, dy, pinchDelta, rotateDelta, settings, DefaultCamera, InputModality.Drag);
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
            Apply(
                ops,
                dx,
                dy,
                pinchDelta,
                rotateDelta,
                settings,
                camera,
                InputModality.Drag,
                null
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
                dx = dxSign * settings.PanButtonScaleX;
                dy = dySign * settings.PanButtonScaleY;
            }

            if ((ops & CameraOp.Orbit) != 0)
            {
                dx = dxSign * settings.OrbitYawButtonScale;
                dy = dySign * settings.OrbitPitchButtonScale;
            }

            if ((ops & CameraOp.Zoom) != 0)
            {
                pinch = pinchSign * settings.ZoomButtonScale;
            }

            if ((ops & CameraOp.Yaw) != 0)
            {
                rotate = rotateSign * settings.YawRotateButtonScale;
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
            InputModality modality
        )
        {
            Apply(ops, dx, dy, pinchDelta, rotateDelta, settings, camera, modality, null);
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
                ApplyOrbit(dx, dy, settings, camera, modality, selection);
            }

            if ((ops & CameraOp.Yaw) != 0)
            {
                ApplyYawRotate(rotateDelta, settings, camera, modality, selection);
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
                modality == InputModality.Button
                    ? pinchDelta
                    : pinchDelta * settings.ZoomSensitivity;
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

            float mx =
                modality == InputModality.Button ? dx : dx * settings.PanSensitivityX;
            float my =
                modality == InputModality.Button ? dy : dy * settings.PanSensitivityY;
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
            InputModality modality,
            ISelectionContext selection
        )
        {
            float yaw = camera.AngleX;
            float pitch = camera.AngleY;
            if (float.IsNaN(yaw) || float.IsNaN(pitch))
            {
                return;
            }

            // Always re-home look-at from selection (live instance / relocate / ghost).
            // Priority in CitiesSelectionContext prefers live buffer positions so this does not
            // flicker like hover; keeping Target in sync on zero-delta latch ticks avoids
            // orbiting a leftover pan target after select/relocate.
            if (
                selection != null
                && selection.TryGetSelectedWorldPosition(out float sx, out float sy, out float sz)
            )
            {
                if (!float.IsNaN(sx))
                {
                    camera.TargetX = sx;
                }

                if (!float.IsNaN(sy))
                {
                    camera.TargetY = sy;
                }

                if (!float.IsNaN(sz))
                {
                    camera.TargetZ = sz;
                }
            }

            float dyaw =
                modality == InputModality.Button ? dx : dx * settings.OrbitYawSensitivity;
            float dpitch =
                modality == InputModality.Button ? dy : dy * settings.OrbitPitchSensitivity;
            if (settings.InvertOrbitYaw)
            {
                dyaw = -dyaw;
            }

            if (settings.InvertOrbitPitch)
            {
                dpitch = -dpitch;
            }

            camera.AngleX = yaw + dyaw;

            float nextPitch = pitch + dpitch;
            float min = settings.OrbitPitchMin;
            float max = settings.OrbitPitchMax;
            if (min > max)
            {
                float swap = min;
                min = max;
                max = swap;
            }

            if (min < PitchEpsilon)
            {
                min = PitchEpsilon;
            }

            if (max < min)
            {
                max = min;
            }

            if (nextPitch < min)
            {
                nextPitch = min;
            }
            else if (nextPitch > max)
            {
                nextPitch = max;
            }

            camera.AngleY = nextPitch;
        }

        private static void ApplyYawRotate(
            float rotateDelta,
            ModSettings settings,
            ICameraController camera,
            InputModality modality,
            ISelectionContext selection
        )
        {
            float delta =
                modality == InputModality.Button
                    ? rotateDelta
                    : rotateDelta * settings.YawRotateSensitivity;
            if (settings.InvertYawRotate)
            {
                delta = -delta;
            }

            if (selection != null && selection.TryApplyObjectYawDelta(delta))
            {
                return;
            }

            float yaw = camera.AngleX;
            if (float.IsNaN(yaw))
            {
                return;
            }

            camera.AngleX = yaw + delta;
        }
    }
}
