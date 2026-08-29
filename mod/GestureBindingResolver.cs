namespace TrackpadCameraControl
{
    public enum CameraOp
    {
        None,
        Pan,
        Zoom,
        Yaw,
        Orbit,
    }

    public static class GestureBindingResolver
    {
        public static CameraOp Resolve(GestureFrame frame, ModSettings settings)
        {
            if (settings == null || !settings.ZoomEnabled)
            {
                return CameraOp.None;
            }

            float pinch = frame.pinchScaleDelta;
            if (pinch < 0f)
            {
                pinch = -pinch;
            }

            if (pinch <= settings.PinchEpsilon)
            {
                return CameraOp.None;
            }

            // MVP: any significant pinch maps to zoom (Maps+ seed).
            if (frame.fingerCount >= 2 || frame.phase == (int)GesturePhase.Changed)
            {
                return CameraOp.Zoom;
            }

            return CameraOp.None;
        }
    }
}
