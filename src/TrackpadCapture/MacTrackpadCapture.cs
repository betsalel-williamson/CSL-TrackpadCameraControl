using System;
using System.Runtime.InteropServices;

namespace TrackpadCapture
{
    /// <summary>
    /// macOS MultitouchSupport → pinch GestureFrames. Keep a CFRunLoop pumping while started
    /// so contact callbacks fire (same requirement as the retired C bridge).
    /// </summary>
    public sealed class MacTrackpadCapture : IDisposable
    {
        private readonly PinchSession _pinch = new PinchSession();
        private readonly object _gate = new object();
        private readonly bool _debug;
        private MultitouchNative.MTContactCallback _callback;
        private IntPtr _devices;
        private Action<GestureFrame> _onFrame;
        private bool _started;

        public MacTrackpadCapture(bool debug = false)
        {
            _debug = debug;
        }

        public bool IsStarted
        {
            get
            {
                lock (_gate)
                {
                    return _started;
                }
            }
        }

        /// <summary>Load MultitouchSupport, register callbacks, start all devices.</summary>
        public bool TryStart(Action<GestureFrame> onFrame, out string error)
        {
            if (onFrame == null)
            {
                throw new ArgumentNullException(nameof(onFrame));
            }

            error = null;
            lock (_gate)
            {
                if (_started)
                {
                    error = "already started";
                    return false;
                }

                if (!MultitouchNative.TryLoad(out error))
                {
                    return false;
                }

                _onFrame = onFrame;
                // Keep delegate rooted for native lifetime.
                _callback = OnContactFrame;

                _devices = MultitouchNative.DeviceCreateList();
                if (_devices == IntPtr.Zero)
                {
                    error = "MTDeviceCreateList returned null";
                    _callback = null;
                    _onFrame = null;
                    return false;
                }

                long count = MultitouchNative.CFArrayGetCount(_devices);
                if (count < 1)
                {
                    error = "no multitouch devices";
                    MultitouchNative.CFRelease(_devices);
                    _devices = IntPtr.Zero;
                    _callback = null;
                    _onFrame = null;
                    return false;
                }

                for (long i = 0; i < count; i++)
                {
                    IntPtr dev = MultitouchNative.CFArrayGetValueAtIndex(_devices, i);
                    MultitouchNative.RegisterContactFrameCallback(dev, _callback);
                    MultitouchNative.DeviceStart(dev, 0);
                }

                _started = true;
                Console.Error.WriteLine(
                    "TrackpadCapture: started " + count + " multitouch device(s)"
                );
                return true;
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (!_started)
                {
                    return;
                }

                _started = false;
                _pinch.Reset();
                _onFrame = null;
                _callback = null;

                if (_devices != IntPtr.Zero)
                {
                    long count = MultitouchNative.CFArrayGetCount(_devices);
                    for (long i = 0; i < count; i++)
                    {
                        IntPtr dev = MultitouchNative.CFArrayGetValueAtIndex(_devices, i);
                        try
                        {
                            MultitouchNative.DeviceStop(dev);
                        }
                        catch
                        {
                            // Fail soft on teardown.
                        }
                    }

                    MultitouchNative.CFRelease(_devices);
                    _devices = IntPtr.Zero;
                }
            }
        }

        private void OnContactFrame(
            IntPtr device,
            IntPtr touches,
            int numTouches,
            double timestamp,
            int frame
        )
        {
            Action<GestureFrame> sink;
            lock (_gate)
            {
                if (!_started)
                {
                    return;
                }

                sink = _onFrame;
            }

            if (sink == null)
            {
                return;
            }

            if (_debug)
            {
                Console.Error.WriteLine("TrackpadBridge: contacts=" + numTouches);
            }

            float distance = 0f;
            if (numTouches >= 2 && touches != IntPtr.Zero)
            {
                var a = Marshal.PtrToStructure<MultitouchNative.MTTouch>(touches);
                var b = Marshal.PtrToStructure<MultitouchNative.MTTouch>(
                    IntPtr.Add(touches, Marshal.SizeOf(typeof(MultitouchNative.MTTouch)))
                );
                float dx = a.normalized.pos.x - b.normalized.pos.x;
                float dy = a.normalized.pos.y - b.normalized.pos.y;
                distance = (float)Math.Sqrt(dx * dx + dy * dy);
            }

            GestureFrame outFrame;
            bool emit;
            lock (_gate)
            {
                emit = _pinch.TryUpdate(numTouches, distance, out outFrame);
            }

            if (emit)
            {
                // Always log pinch emits so a silent terminal is a real signal (no Multitouch).
                Console.Error.WriteLine(
                    "TrackpadBridge: pinch phase="
                        + outFrame.phase
                        + " delta="
                        + outFrame.pinchScaleDelta.ToString("0.####")
                        + " fingers="
                        + outFrame.fingerCount
                );
                sink(outFrame);
            }
        }
    }
}
