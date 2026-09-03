using System;
using System.Collections.Generic;
using TrackpadCapture;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>In-process MultitouchSupport contacts → GestureFrame in the mod DLL.</summary>
    public sealed class InProcessGestureSource : IGestureSource, IDisposable
    {
        private const int MaxQueue = 64;

        private readonly object _gate = new object();
        private readonly Queue<GestureFrame> _queue = new Queue<GestureFrame>();
        private MacTrackpadCapture _capture;
        private bool _connected;

        public bool IsConnected
        {
            get
            {
                lock (_gate)
                {
                    return _connected;
                }
            }
        }

        public void Connect()
        {
            Disconnect();

            var capture = new MacTrackpadCapture(false, GestureCaptureLog.Line);
            lock (_gate)
            {
                _capture = capture;
                _connected = true;
            }

            if (!capture.TryStart(OnCaptureFrame, out string error))
            {
                GestureCaptureLog.Line("contacts start failed: " + error);
                Disconnect();
            }
        }

        public void Disconnect()
        {
            MacTrackpadCapture capture;
            lock (_gate)
            {
                capture = _capture;
                _capture = null;
                _connected = false;
                _queue.Clear();
            }

            if (capture != null)
            {
                capture.Dispose();
            }
        }

        public bool TryDequeue(out GestureFrame frame)
        {
            try
            {
                bool pump;
                lock (_gate)
                {
                    pump = _connected;
                }

                if (pump)
                {
                    MacRunLoop.RunInDefaultMode(0.0, true);
                }
            }
            catch
            {
                // Fail soft if CoreFoundation is unavailable.
            }

            lock (_gate)
            {
                if (_queue.Count == 0)
                {
                    frame = default;
                    return false;
                }

                frame = _queue.Dequeue();
                return true;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void OnCaptureFrame(TrackpadCapture.GestureFrame src)
        {
            GestureFrame frame = ToMod(src);
            lock (_gate)
            {
                if (!_connected)
                {
                    return;
                }

                if (_queue.Count < MaxQueue)
                {
                    _queue.Enqueue(frame);
                }
            }
        }

        private static GestureFrame ToMod(TrackpadCapture.GestureFrame src)
        {
            return new GestureFrame
            {
                magic = src.magic,
                version = src.version,
                flags = src.flags,
                timestampNs = src.timestampNs,
                fingerCount = src.fingerCount,
                phase = src.phase,
                centroidDeltaX = src.centroidDeltaX,
                centroidDeltaY = src.centroidDeltaY,
                pinchScaleDelta = src.pinchScaleDelta,
                rotateDelta = src.rotateDelta,
                modifiers = src.modifiers,
                reserved = src.reserved,
            };
        }
    }
}
