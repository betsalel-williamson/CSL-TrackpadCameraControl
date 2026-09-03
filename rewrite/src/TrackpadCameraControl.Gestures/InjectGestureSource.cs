using System.Collections.Generic;

namespace TrackpadCameraControl.Gestures
{
    /// <summary>Thread-safe inject queue for headless / in-game harnesses.</summary>
    public sealed class InjectGestureSource : IGestureSource
    {
        private readonly object _gate = new object();
        private readonly Queue<GestureFrame> _queue = new Queue<GestureFrame>();
        private volatile bool _enabled = true;

        public bool IsConnected => _enabled;

        public void Connect()
        {
            _enabled = true;
        }

        public void Disconnect()
        {
            _enabled = false;
        }

        public void Enqueue(GestureFrame frame)
        {
            lock (_gate)
            {
                _queue.Enqueue(frame);
            }
        }

        public bool TryDequeue(out GestureFrame frame)
        {
            if (!_enabled)
            {
                frame = default(GestureFrame);
                return false;
            }

            lock (_gate)
            {
                if (_queue.Count == 0)
                {
                    frame = default(GestureFrame);
                    return false;
                }

                frame = _queue.Dequeue();
                return true;
            }
        }
    }
}
