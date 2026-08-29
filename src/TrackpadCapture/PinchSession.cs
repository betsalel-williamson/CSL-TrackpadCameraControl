using System;

namespace TrackpadCapture
{
    /// <summary>Two-finger pinch state machine matching the retired C TrackpadBridge.</summary>
    public sealed class PinchSession
    {
        private bool _pinchActive;
        private float _lastDistance = -1f;

        public bool TryUpdate(int fingerCount, float distance, out GestureFrame frame)
        {
            frame = default;

            if (fingerCount < 2)
            {
                if (_pinchActive)
                {
                    frame = GestureFrame.Create(fingerCount, GesturePhase.Ended, 0f);
                    _pinchActive = false;
                    _lastDistance = -1f;
                    return true;
                }

                return false;
            }

            if (!_pinchActive || _lastDistance < 0f)
            {
                frame = GestureFrame.Create(fingerCount, GesturePhase.Began, 0f);
                _pinchActive = true;
                _lastDistance = distance;
                return true;
            }

            float delta = 0f;
            if (_lastDistance > 1e-6f)
            {
                delta = (distance - _lastDistance) / _lastDistance;
            }

            _lastDistance = distance;
            frame = GestureFrame.Create(fingerCount, GesturePhase.Changed, delta);
            return true;
        }

        public void Reset()
        {
            _pinchActive = false;
            _lastDistance = -1f;
        }
    }
}
