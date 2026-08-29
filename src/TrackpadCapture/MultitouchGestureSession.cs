using System;

namespace TrackpadCapture
{
    /// <summary>
    /// Multitouch contact samples → GestureFrame primitives (centroid, pinch, rotate).
    /// Does not decide camera ops — that stays in the mod.
    /// </summary>
    public sealed class MultitouchGestureSession
    {
        private bool _active;
        private float _lastCentroidX;
        private float _lastCentroidY;
        private float _lastDistance = -1f;
        private float _lastAngle;
        private bool _haveCentroid;
        private bool _havePair;

        public bool TryUpdate(
            int fingerCount,
            bool haveCentroid,
            float centroidX,
            float centroidY,
            bool havePair,
            float distance,
            float angleRadians,
            uint modifiers,
            out GestureFrame frame
        )
        {
            frame = default;

            if (fingerCount < 2)
            {
                if (_active)
                {
                    frame = GestureFrame.Create(
                        fingerCount,
                        GesturePhase.Ended,
                        0f,
                        0f,
                        0f,
                        0f,
                        modifiers
                    );
                    Reset();
                    return true;
                }

                return false;
            }

            if (!_active)
            {
                _active = true;
                StoreSample(haveCentroid, centroidX, centroidY, havePair, distance, angleRadians);
                frame = GestureFrame.Create(
                    fingerCount,
                    GesturePhase.Began,
                    0f,
                    0f,
                    0f,
                    0f,
                    modifiers
                );
                return true;
            }

            float dx = 0f;
            float dy = 0f;
            if (haveCentroid && _haveCentroid)
            {
                dx = centroidX - _lastCentroidX;
                dy = centroidY - _lastCentroidY;
            }

            float pinch = 0f;
            if (havePair && _havePair && _lastDistance > 1e-6f)
            {
                pinch = (distance - _lastDistance) / _lastDistance;
            }

            float rotate = 0f;
            if (havePair && _havePair)
            {
                // Wire rotateDelta in degrees (CameraApplicator adds to yaw degrees).
                rotate = NormalizeAngle(angleRadians - _lastAngle) * (180f / (float)Math.PI);
            }

            StoreSample(haveCentroid, centroidX, centroidY, havePair, distance, angleRadians);
            frame = GestureFrame.Create(
                fingerCount,
                GesturePhase.Changed,
                dx,
                dy,
                pinch,
                rotate,
                modifiers
            );
            return true;
        }

        public void Reset()
        {
            _active = false;
            _lastDistance = -1f;
            _lastAngle = 0f;
            _haveCentroid = false;
            _havePair = false;
        }

        private void StoreSample(
            bool haveCentroid,
            float centroidX,
            float centroidY,
            bool havePair,
            float distance,
            float angleRadians
        )
        {
            if (haveCentroid)
            {
                _lastCentroidX = centroidX;
                _lastCentroidY = centroidY;
                _haveCentroid = true;
            }

            if (havePair)
            {
                _lastDistance = distance;
                _lastAngle = angleRadians;
                _havePair = true;
            }
        }

        private static float NormalizeAngle(float radians)
        {
            const float pi = (float)Math.PI;
            const float twoPi = (float)(Math.PI * 2.0);
            while (radians > pi)
            {
                radians -= twoPi;
            }

            while (radians < -pi)
            {
                radians += twoPi;
            }

            return radians;
        }
    }
}
