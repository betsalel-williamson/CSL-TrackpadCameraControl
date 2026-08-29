namespace TrackpadCameraControl
{
    public static class CameraApplicator
    {
        private static readonly CameraControllerZoom DefaultCamera = new CameraControllerZoom();

        public static void Apply(
            CameraOp op,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings
        )
        {
            Apply(op, dx, dy, pinchDelta, rotateDelta, settings, DefaultCamera);
        }

        public static void Apply(
            CameraOp op,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings,
            ICameraZoom camera
        )
        {
            _ = dx;
            _ = dy;
            _ = rotateDelta;

            if (op != CameraOp.Zoom || settings == null || camera == null)
            {
                return;
            }

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
    }
}
