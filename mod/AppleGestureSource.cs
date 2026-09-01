using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TrackpadCameraControl
{
    /// <summary>
    /// In-process AppKit local monitor → GestureFrame. Uses Cities' NSApplication
    /// (no extra window, no Accessibility). Fail soft if AppKit is missing.
    /// </summary>
    public sealed class AppleGestureSource : IGestureSource, IDisposable
    {
        private const string LibObjC = "/usr/lib/libobjc.A.dylib";
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const string AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";

        private const ulong GestureMask =
            (1UL << 18)
            | (1UL << 19)
            | (1UL << 20)
            | (1UL << 22)
            | (1UL << 29)
            | (1UL << 30)
            | (1UL << 31);

        private const int BlockIsGlobal = 1 << 28;

        private readonly object _gate = new object();
        private readonly Queue<GestureFrame> _queue = new Queue<GestureFrame>();
        private bool _connected;
        private IntPtr _monitor;
        private GCHandle _blockHandle;
        private GCHandle _descHandle;
        private BlockInvoke _invoke;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr BlockInvoke(IntPtr block, IntPtr nsEvent);

        [StructLayout(LayoutKind.Sequential)]
        private struct BlockDescriptor
        {
            public IntPtr reserved;
            public IntPtr size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BlockLiteral
        {
            public IntPtr isa;
            public int flags;
            public int reserved;
            public IntPtr invoke;
            public IntPtr descriptor;
        }

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
            lock (_gate)
            {
                DisconnectUnlocked();
                if (dlopen(AppKitPath, 2) == IntPtr.Zero)
                {
                    GestureCaptureLog.Line("apple AppKit missing");
                    return;
                }

                IntPtr app = objc_msgSend(
                    objc_getClass("NSApplication"),
                    sel_registerName("sharedApplication")
                );
                if (app == IntPtr.Zero)
                {
                    return;
                }

                _invoke = OnBlock;
                IntPtr invokePtr = Marshal.GetFunctionPointerForDelegate(_invoke);

                IntPtr isa = dlsym(InternalHandleMinusTwo(), "_NSConcreteGlobalBlock");
                if (isa == IntPtr.Zero)
                {
                    return;
                }

                var desc = new BlockDescriptor
                {
                    reserved = IntPtr.Zero,
                    size = (IntPtr)Marshal.SizeOf(typeof(BlockLiteral)),
                };
                _descHandle = GCHandle.Alloc(desc, GCHandleType.Pinned);

                var literal = new BlockLiteral
                {
                    isa = isa,
                    flags = BlockIsGlobal,
                    reserved = 0,
                    invoke = invokePtr,
                    descriptor = _descHandle.AddrOfPinnedObject(),
                };
                _blockHandle = GCHandle.Alloc(literal, GCHandleType.Pinned);

                _monitor = objc_msgSend_mask_block(
                    objc_getClass("NSEvent"),
                    sel_registerName("addLocalMonitorForEventsMatchingMask:handler:"),
                    GestureMask,
                    _blockHandle.AddrOfPinnedObject()
                );
                _connected = _monitor != IntPtr.Zero;
                if (!_connected)
                {
                    GestureCaptureLog.Line("apple monitor failed");
                    FreeHandlesUnlocked();
                }
                else
                {
                    GestureCaptureLog.Line("apple monitor started");
                }
            }
        }

        public void Disconnect()
        {
            lock (_gate)
            {
                DisconnectUnlocked();
            }
        }

        public bool TryDequeue(out GestureFrame frame)
        {
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

        private void DisconnectUnlocked()
        {
            if (_monitor != IntPtr.Zero)
            {
                objc_msgSend_void_ptr(
                    objc_getClass("NSEvent"),
                    sel_registerName("removeMonitor:"),
                    _monitor
                );
                _monitor = IntPtr.Zero;
            }

            if (_blockHandle.IsAllocated)
            {
                _blockHandle.Free();
            }

            if (_descHandle.IsAllocated)
            {
                _descHandle.Free();
            }

            _queue.Clear();
            _connected = false;
            _invoke = null;
        }

        private void FreeHandlesUnlocked()
        {
            if (_blockHandle.IsAllocated)
            {
                _blockHandle.Free();
            }

            if (_descHandle.IsAllocated)
            {
                _descHandle.Free();
            }
        }

        private IntPtr OnBlock(IntPtr block, IntPtr nsEvent)
        {
            if (nsEvent == IntPtr.Zero)
            {
                return nsEvent;
            }

            if (!InputGates.IsGameFocused())
            {
                return nsEvent;
            }

            ulong type = objc_msgSend_ulong(nsEvent, sel_registerName("type"));
            ulong phase = objc_msgSend_ulong(nsEvent, sel_registerName("phase"));
            ulong mods = objc_msgSend_ulong(nsEvent, sel_registerName("modifierFlags"));
            double sdx = 0;
            double sdy = 0;
            double mag = 0;
            float rot = 0f;
            bool precise = true;
            if (type == AppleGestureMapper.EventTypeScrollWheel)
            {
                sdx = objc_msgSend_f64(nsEvent, sel_registerName("scrollingDeltaX"));
                sdy = objc_msgSend_f64(nsEvent, sel_registerName("scrollingDeltaY"));
                precise = objc_msgSend_bool(nsEvent, sel_registerName("hasPreciseScrollingDeltas"));
                // Drive vanilla suppress: precise trackpad → may skip zoom; wheel → allow.
                VanillaCameraSuppress.PreciseTrackpadScroll = precise;
            }
            else if (type == AppleGestureMapper.EventTypeMagnify)
            {
                mag = objc_msgSend_f64(nsEvent, sel_registerName("magnification"));
            }
            else if (type == AppleGestureMapper.EventTypeRotate)
            {
                rot = objc_msgSend_f32(nsEvent, sel_registerName("rotation"));
            }

            if (
                AppleGestureMapper.TryMap(
                    type,
                    phase,
                    mods,
                    sdx,
                    sdy,
                    mag,
                    rot,
                    precise,
                    out GestureFrame frame
                )
            )
            {
                lock (_gate)
                {
                    if (_queue.Count < 64)
                    {
                        _queue.Enqueue(frame);
                    }
                }

                GestureCaptureLog.Frame("apple", frame);
            }

            return nsEvent;
        }

        private static IntPtr InternalHandleMinusTwo()
        {
            return (IntPtr)(-2);
        }

        [DllImport(LibObjC)]
        private static extern IntPtr objc_getClass(string name);

        [DllImport(LibObjC)]
        private static extern IntPtr sel_registerName(string name);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_mask_block(
            IntPtr recv,
            IntPtr sel,
            ulong mask,
            IntPtr block
        );

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_ptr(IntPtr recv, IntPtr sel, IntPtr arg);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern ulong objc_msgSend_ulong(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern double objc_msgSend_f64(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern float objc_msgSend_f32(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool objc_msgSend_bool(IntPtr recv, IntPtr sel);

        [DllImport(LibSystem)]
        private static extern IntPtr dlopen(string path, int mode);

        [DllImport(LibSystem)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);
    }
}
