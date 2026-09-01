using System;
using System.Runtime.InteropServices;

namespace TrackpadCameraControl
{
    /// <summary>
    /// One-shot AppKit activation after city load so the game window receives focus.
    /// Does not cache focus — <see cref="InputGates"/> re-queries each frame.
    /// </summary>
    public static class GameFocusActivation
    {
        private const string LibObjC = "/usr/lib/libobjc.A.dylib";
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const string AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";

        public static bool TryActivate()
        {
#if HAS_CITIES
            try
            {
                if (dlopen(AppKitPath, 2) == IntPtr.Zero)
                {
                    return false;
                }

                IntPtr app = objc_msgSend(
                    objc_getClass("NSApplication"),
                    sel_registerName("sharedApplication")
                );
                if (app == IntPtr.Zero)
                {
                    return false;
                }

                objc_msgSend_void_bool(app, sel_registerName("activateIgnoringOtherApps:"), true);

                UnityEngine.Cursor.visible = false;
                GestureCaptureLog.Line("focus activated on level load");
                return true;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }

#if HAS_CITIES
        [DllImport(LibObjC)]
        private static extern IntPtr objc_getClass(string name);

        [DllImport(LibObjC)]
        private static extern IntPtr sel_registerName(string name);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_bool(IntPtr recv, IntPtr sel, bool arg);

        [DllImport(LibSystem)]
        private static extern IntPtr dlopen(string path, int mode);
#endif
    }
}
