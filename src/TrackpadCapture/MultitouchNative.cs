using System;
using System.Runtime.InteropServices;

namespace TrackpadCapture
{
    /// <summary>P/Invoke surface for private MultitouchSupport + CoreFoundation (macOS).</summary>
    internal static class MultitouchNative
    {
        private const string MultitouchPath =
            "/System/Library/PrivateFrameworks/MultitouchSupport.framework/MultitouchSupport";
        private const string CoreFoundationPath =
            "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        private const string LibSystem = "/usr/lib/libSystem.dylib";

        [StructLayout(LayoutKind.Sequential)]
        internal struct MTPoint
        {
            public float x;
            public float y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MTVector
        {
            public MTPoint pos;
            public MTPoint vel;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MTTouch
        {
            public int frame;
            public double timestamp;
            public int pathIndex;
            public int state;
            public int fingerID;
            public int handID;
            public MTVector normalized;
            public float size;
            public int zero1;
            public float angle;
            public float majorAxis;
            public float minorAxis;
            public MTVector absolute;
            public int zero2;
            public int zero3;
            public float zDensity;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void MTContactCallback(
            IntPtr device,
            IntPtr touches,
            int numTouches,
            double timestamp,
            int frame
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MTDeviceCreateListFn();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MTRegisterContactFrameCallbackFn(
            IntPtr device,
            MTContactCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MTDeviceStartFn(IntPtr device, int threaded);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MTDeviceStopFn(IntPtr device);

        private static IntPtr s_lib;
        private static MTDeviceCreateListFn s_createList;
        private static MTRegisterContactFrameCallbackFn s_registerCallback;
        private static MTDeviceStartFn s_deviceStart;
        private static MTDeviceStopFn s_deviceStop;

        public static bool TryLoad(out string error)
        {
            error = null;
            if (s_lib != IntPtr.Zero)
            {
                return true;
            }

            s_lib = dlopen(
                MultitouchPath,
                1 /* RTLD_LAZY */
            );
            if (s_lib == IntPtr.Zero)
            {
                IntPtr errPtr = dlerror();
                string detail = errPtr != IntPtr.Zero ? Marshal.PtrToStringAnsi(errPtr) : "unknown";
                error = "dlopen MultitouchSupport failed: " + detail;
                return false;
            }

            s_createList = LoadFn<MTDeviceCreateListFn>("MTDeviceCreateList");
            s_registerCallback = LoadFn<MTRegisterContactFrameCallbackFn>(
                "MTRegisterContactFrameCallback"
            );
            s_deviceStart = LoadFn<MTDeviceStartFn>("MTDeviceStart");
            s_deviceStop = LoadFn<MTDeviceStopFn>("MTDeviceStop");

            if (
                s_createList == null
                || s_registerCallback == null
                || s_deviceStart == null
                || s_deviceStop == null
            )
            {
                error = "missing MultitouchSupport symbols";
                return false;
            }

            return true;
        }

        public static IntPtr DeviceCreateList() => s_createList();

        public static void RegisterContactFrameCallback(IntPtr device, MTContactCallback callback)
        {
            s_registerCallback(device, callback);
        }

        public static void DeviceStart(IntPtr device, int threaded) =>
            s_deviceStart(device, threaded);

        public static void DeviceStop(IntPtr device) => s_deviceStop(device);

        private static T LoadFn<T>(string name)
            where T : class
        {
            IntPtr sym = dlsym(s_lib, name);
            if (sym == IntPtr.Zero)
            {
                return null;
            }

            return (T)(object)Marshal.GetDelegateForFunctionPointer(sym, typeof(T));
        }

        [DllImport(LibSystem, SetLastError = true)]
        private static extern IntPtr dlopen(string path, int mode);

        [DllImport(LibSystem)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(LibSystem)]
        private static extern IntPtr dlerror();

        [DllImport(CoreFoundationPath)]
        public static extern long CFArrayGetCount(IntPtr theArray);

        [DllImport(CoreFoundationPath)]
        public static extern IntPtr CFArrayGetValueAtIndex(IntPtr theArray, long idx);

        [DllImport(CoreFoundationPath)]
        public static extern void CFRelease(IntPtr cf);

        [DllImport(CoreFoundationPath)]
        public static extern IntPtr CFStringCreateWithCString(
            IntPtr alloc,
            string str,
            uint encoding
        );

        [DllImport(CoreFoundationPath)]
        public static extern int CFRunLoopRunInMode(
            IntPtr mode,
            double seconds,
            [MarshalAs(UnmanagedType.I1)] bool returnAfterSourceHandled
        );

        public const uint kCFStringEncodingUTF8 = 0x08000100;

        public static IntPtr CreateDefaultRunLoopMode()
        {
            return CFStringCreateWithCString(
                IntPtr.Zero,
                "kCFRunLoopDefaultMode",
                kCFStringEncodingUTF8
            );
        }
    }
}
