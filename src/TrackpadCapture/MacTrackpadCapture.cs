using System;
using System.Runtime.InteropServices;

namespace TrackpadCapture
{
    /// <summary>
    /// macOS MultitouchSupport → GestureFrames (centroid, pinch, rotate, modifiers).
    /// Keep a CFRunLoop pumping while started so contact callbacks fire.
    /// </summary>
    public sealed class MacTrackpadCapture : IDisposable
    {
        private readonly MultitouchGestureSession _session = new MultitouchGestureSession();
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
                _session.Reset();
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

            SampleTouches(
                touches,
                numTouches,
                out bool haveCentroid,
                out float cx,
                out float cy,
                out bool havePair,
                out float distance,
                out float angle
            );

            uint modifiers = MacModifierKeys.ReadModifiers();

            GestureFrame outFrame;
            bool emit;
            lock (_gate)
            {
                emit = _session.TryUpdate(
                    numTouches,
                    haveCentroid,
                    cx,
                    cy,
                    havePair,
                    distance,
                    angle,
                    modifiers,
                    out outFrame
                );
            }

            if (emit)
            {
                Console.Error.WriteLine(
                    "TrackpadBridge: gesture fingers="
                        + outFrame.fingerCount
                        + " phase="
                        + outFrame.phase
                        + " dC=("
                        + outFrame.centroidDeltaX.ToString("0.####")
                        + ","
                        + outFrame.centroidDeltaY.ToString("0.####")
                        + ") pinch="
                        + outFrame.pinchScaleDelta.ToString("0.####")
                        + " rot="
                        + outFrame.rotateDelta.ToString("0.####")
                        + " mods="
                        + outFrame.modifiers
                );
                sink(outFrame);
            }
        }

        private static void SampleTouches(
            IntPtr touches,
            int numTouches,
            out bool haveCentroid,
            out float centroidX,
            out float centroidY,
            out bool havePair,
            out float distance,
            out float angle
        )
        {
            haveCentroid = false;
            centroidX = 0f;
            centroidY = 0f;
            havePair = false;
            distance = 0f;
            angle = 0f;

            if (numTouches < 1 || touches == IntPtr.Zero)
            {
                return;
            }

            int n = numTouches > 5 ? 5 : numTouches;
            int stride = Marshal.SizeOf(typeof(MultitouchNative.MTTouch));
            float sumX = 0f;
            float sumY = 0f;
            MultitouchNative.MTTouch first = default;
            MultitouchNative.MTTouch second = default;

            for (int i = 0; i < n; i++)
            {
                var t = (MultitouchNative.MTTouch)
                    Marshal.PtrToStructure(
                        new IntPtr(touches.ToInt64() + (long)i * stride),
                        typeof(MultitouchNative.MTTouch)
                    );
                sumX += t.normalized.pos.x;
                sumY += t.normalized.pos.y;
                if (i == 0)
                {
                    first = t;
                }
                else if (i == 1)
                {
                    second = t;
                }
            }

            haveCentroid = true;
            centroidX = sumX / n;
            centroidY = sumY / n;

            if (n >= 2)
            {
                float dx = second.normalized.pos.x - first.normalized.pos.x;
                float dy = second.normalized.pos.y - first.normalized.pos.y;
                distance = (float)Math.Sqrt(dx * dx + dy * dy);
                angle = (float)Math.Atan2(dy, dx);
                havePair = true;
            }
        }
    }
}
