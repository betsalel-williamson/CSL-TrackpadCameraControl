using System;
using System.Runtime.InteropServices;

namespace AppleGestureProbe
{
    /// <summary>AppKit / libobjc surface for a window-local NSEvent pump (no Accessibility).</summary>
    internal static class AppKitNative
    {
        private const string LibObjC = "/usr/lib/libobjc.A.dylib";
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const string AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";
        private const string FoundationPath =
            "/System/Library/Frameworks/Foundation.framework/Foundation";
        private const string CoreFoundationPath =
            "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        public const ulong EventTypeRotate = 18;
        public const ulong EventTypeBeginGesture = 19;
        public const ulong EventTypeEndGesture = 20;
        public const ulong EventTypeScrollWheel = 22;
        public const ulong EventTypeGesture = 29;
        public const ulong EventTypeMagnify = 30;
        public const ulong EventTypeSwipe = 31;
        public const ulong EventTypeSmartMagnify = 32;

        public const ulong EventMaskAny = ulong.MaxValue;

        public const nuint WindowStyleTitled = 1;
        public const nuint WindowStyleClosable = 2;
        public const nuint WindowStyleMiniaturizable = 4;
        public const nuint WindowStyleResizable = 8;

        public const nuint ActivationPolicyRegular = 0;

        public const ulong FlagMaskShift = 0x00020000;
        public const ulong FlagMaskControl = 0x00040000;
        public const ulong FlagMaskAlternate = 0x00080000;
        public const ulong FlagMaskCommand = 0x00100000;

        public const nuint PhaseBegan = 1 << 0;
        public const nuint Stationary = 1 << 1;
        public const nuint PhaseChanged = 1 << 2;
        public const nuint PhaseEnded = 1 << 3;
        public const nuint PhaseCancelled = 1 << 4;
        public const nuint MayBegin = 1 << 5;

        public static bool TryLoad(out string error)
        {
            error = null;
            if (dlopen(AppKitPath, 2) == IntPtr.Zero || dlopen(FoundationPath, 2) == IntPtr.Zero)
            {
                error = "dlopen AppKit/Foundation failed";
                return false;
            }

            return true;
        }

        public static IntPtr GetClass(string name) => objc_getClass(name);

        public static IntPtr Sel(string name) => sel_registerName(name);

        public static IntPtr Msg(IntPtr recv, IntPtr sel) => objc_msgSend(recv, sel);

        public static IntPtr Msg(IntPtr recv, IntPtr sel, IntPtr arg) =>
            objc_msgSend_IntPtr(recv, sel, arg);

        public static IntPtr Msg(IntPtr recv, IntPtr sel, double arg) =>
            objc_msgSend_double_retPtr(recv, sel, arg);

        public static void MsgVoid(IntPtr recv, IntPtr sel) => objc_msgSend_void(recv, sel);

        public static void MsgVoid(IntPtr recv, IntPtr sel, IntPtr arg) =>
            objc_msgSend_void_IntPtr(recv, sel, arg);

        public static void MsgVoid(IntPtr recv, IntPtr sel, nuint arg) =>
            objc_msgSend_void_nuint(recv, sel, arg);

        public static void MsgVoid(IntPtr recv, IntPtr sel, bool arg) =>
            objc_msgSend_void_bool(recv, sel, arg);

        public static void SetContentSize(IntPtr window, double width, double height) =>
            objc_msgSend_setContentSize(window, Sel("setContentSize:"), width, height);

        public static IntPtr NextEvent(
            IntPtr app,
            ulong mask,
            IntPtr untilDate,
            IntPtr mode,
            bool dequeue
        )
        {
            return objc_msgSend_nextEvent(
                app,
                Sel("nextEventMatchingMask:untilDate:inMode:dequeue:"),
                mask,
                untilDate,
                mode,
                dequeue
            );
        }

        public static ulong ULong(IntPtr recv, IntPtr sel) => objc_msgSend_ulong(recv, sel);

        public static double Double(IntPtr recv, IntPtr sel) => objc_msgSend_f64(recv, sel);

        public static float Float(IntPtr recv, IntPtr sel) => objc_msgSend_f32(recv, sel);

        public static bool Bool(IntPtr recv, IntPtr sel) => objc_msgSend_bool(recv, sel);

        public static IntPtr CreateCfString(string value)
        {
            return CFStringCreateWithCString(IntPtr.Zero, value, 0x08000100);
        }

        [DllImport(LibObjC)]
        private static extern IntPtr objc_getClass(string name);

        [DllImport(LibObjC)]
        private static extern IntPtr sel_registerName(string name);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr(IntPtr recv, IntPtr sel, IntPtr arg);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_double_retPtr(
            IntPtr recv,
            IntPtr sel,
            double arg
        );

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_IntPtr(IntPtr recv, IntPtr sel, IntPtr arg);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_nuint(IntPtr recv, IntPtr sel, nuint arg);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_bool(
            IntPtr recv,
            IntPtr sel,
            [MarshalAs(UnmanagedType.I1)] bool arg
        );

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_nextEvent(
            IntPtr recv,
            IntPtr sel,
            ulong mask,
            IntPtr untilDate,
            IntPtr mode,
            [MarshalAs(UnmanagedType.I1)] bool dequeue
        );

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern ulong objc_msgSend_ulong(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern double objc_msgSend_f64(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern float objc_msgSend_f32(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool objc_msgSend_bool(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_setContentSize(
            IntPtr recv,
            IntPtr sel,
            double width,
            double height
        );

        [DllImport(LibSystem)]
        private static extern IntPtr dlopen(string path, int mode);

        [DllImport(CoreFoundationPath)]
        private static extern IntPtr CFStringCreateWithCString(
            IntPtr alloc,
            string str,
            uint encoding
        );
    }
}
