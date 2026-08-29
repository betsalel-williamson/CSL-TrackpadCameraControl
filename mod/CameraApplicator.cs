// Apply deltas to CameraController from settings (no feel literals).

namespace TrackpadCameraControl
{
    public static class CameraApplicator
    {
        public static void Apply(
            CameraOp op,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings
        )
        {
            // var cam = CameraController.instance;
            // Use settings.*Sensitivity / Invert* only — never magic numbers here.
            _ = op;
            _ = dx;
            _ = dy;
            _ = pinchDelta;
            _ = rotateDelta;
            _ = settings;
        }
    }
}
