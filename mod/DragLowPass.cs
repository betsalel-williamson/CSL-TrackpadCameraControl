using System;

namespace TrackpadCameraControl
{
    /// <summary>EMA helper for per-op drag low-pass. Buttons skip this path.</summary>
    public sealed class DragLowPass
    {
        private float _panX;
        private float _panY;
        private float _zoom;
        private float _yaw;
        private float _orbitX;
        private float _orbitY;
        private bool _panInit;
        private bool _zoomInit;
        private bool _yawInit;
        private bool _orbitInit;

        public void Reset()
        {
            _panInit = false;
            _zoomInit = false;
            _yawInit = false;
            _orbitInit = false;
            _panX = 0f;
            _panY = 0f;
            _zoom = 0f;
            _yaw = 0f;
            _orbitX = 0f;
            _orbitY = 0f;
        }

        public void Filter(
            CameraOp ops,
            ModSettings settings,
            ref float dx,
            ref float dy,
            ref float pinchDelta,
            ref float rotateDelta
        )
        {
            // LP rides EnableContactsCapture: settings toggles only apply when Contacts is on.
            if (!FeatureFlags.EnableContactsCapture || settings == null)
            {
                return;
            }

            if ((ops & CameraOp.Pan) != 0)
            {
                if (settings.PanLowPassEnabled)
                {
                    Filter2(settings.PanLowPassAlpha, ref _panInit, ref _panX, ref _panY, ref dx, ref dy);
                }
                else
                {
                    _panInit = false;
                }
            }

            if ((ops & CameraOp.Orbit) != 0)
            {
                if (settings.OrbitLowPassEnabled)
                {
                    Filter2(
                        settings.OrbitLowPassAlpha,
                        ref _orbitInit,
                        ref _orbitX,
                        ref _orbitY,
                        ref dx,
                        ref dy
                    );
                }
                else
                {
                    _orbitInit = false;
                }
            }

            if ((ops & CameraOp.Zoom) != 0)
            {
                if (settings.ZoomLowPassEnabled)
                {
                    Filter1(settings.ZoomLowPassAlpha, ref _zoomInit, ref _zoom, ref pinchDelta);
                }
                else
                {
                    _zoomInit = false;
                }
            }

            if ((ops & CameraOp.Yaw) != 0)
            {
                if (settings.YawLowPassEnabled)
                {
                    Filter1(settings.YawLowPassAlpha, ref _yawInit, ref _yaw, ref rotateDelta);
                }
                else
                {
                    _yawInit = false;
                }
            }
        }

        private static void Filter1(float alpha, ref bool init, ref float state, ref float sample)
        {
            float a = ClampAlpha(alpha);
            if (!init)
            {
                state = sample;
                init = true;
                return;
            }

            state = state + a * (sample - state);
            sample = state;
        }

        private static void Filter2(
            float alpha,
            ref bool init,
            ref float stateX,
            ref float stateY,
            ref float sampleX,
            ref float sampleY
        )
        {
            float a = ClampAlpha(alpha);
            if (!init)
            {
                stateX = sampleX;
                stateY = sampleY;
                init = true;
                return;
            }

            stateX = stateX + a * (sampleX - stateX);
            stateY = stateY + a * (sampleY - stateY);
            sampleX = stateX;
            sampleY = stateY;
        }

        private static float ClampAlpha(float alpha)
        {
            if (alpha < 0f)
            {
                return 0f;
            }

            if (alpha > 1f)
            {
                return 1f;
            }

            return alpha;
        }
    }
}
