using System.Collections.Concurrent;

namespace TrackpadCameraControl
{
    /// <summary>Thread-safe inject queue for headless / in-game harnesses.</summary>
    public sealed class InjectGestureSource : IGestureSource
    {
        private readonly ConcurrentQueue<GestureFrame> _queue = new ConcurrentQueue<GestureFrame>();
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
            _queue.Enqueue(frame);
        }

        public bool TryDequeue(out GestureFrame frame)
        {
            if (!_enabled)
            {
                frame = default;
                return false;
            }

            return _queue.TryDequeue(out frame);
        }
    }
}
