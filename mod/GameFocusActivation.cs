using System;
using System.Runtime.InteropServices;

namespace TrackpadCameraControl
{
    /// <summary>
    /// AppKit activation after city load so the game window owns focus/cursor.
    /// One-shot activate plus a short fail-soft cursor re-hide (vanilla may show the OS
    /// cursor again during load). Does not cache focus — <see cref="InputGates"/> re-queries.
    /// Residual dual-cursor after this is treated as a CS1/Unity Mac quirk (alt-tab workaround).
    /// </summary>
    public static class GameFocusActivation
    {
        private const string LibObjC = "/usr/lib/libobjc.A.dylib";
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const string AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";

        /// <summary>Frames to re-assert <c>Cursor.visible = false</c> after level load.</summary>
        public const int CursorHideFollowUpFrames = 45;

        private static int _cursorHideFramesRemaining;

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
                TryMakeKeyAndOrderFront(app);

                UnityEngine.Cursor.visible = false;
                ArmCursorHideFollowUp(CursorHideFollowUpFrames);
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

        /// <summary>Queue delayed cursor re-hide frames (also used from tests).</summary>
        public static void ArmCursorHideFollowUp(int frames)
        {
            if (frames < 0)
            {
                frames = 0;
            }

            _cursorHideFramesRemaining = frames;
        }

        /// <summary>Remaining follow-up frames (tests).</summary>
        public static int CursorHideFramesRemaining => _cursorHideFramesRemaining;

        /// <summary>
        /// Re-hide the OS/Unity cursor for a few frames after load while the game is focused
        /// and Options/menu is closed — avoids fighting UI that needs a visible cursor.
        /// </summary>
        public static void TickCursorHideFollowUp(IGameUiContext ui)
        {
#if HAS_CITIES
            if (_cursorHideFramesRemaining <= 0)
            {
                return;
            }

            _cursorHideFramesRemaining--;
            if (ui == null)
            {
                return;
            }

            if (!ui.IsGameFocused() || ui.IsMenuOrOptionsOpen())
            {
                return;
            }

            UnityEngine.Cursor.visible = false;
#else
            _ = ui;
            _cursorHideFramesRemaining = 0;
#endif
        }

#if HAS_CITIES
        private static void TryMakeKeyAndOrderFront(IntPtr app)
        {
            try
            {
                IntPtr window = objc_msgSend(app, sel_registerName("mainWindow"));
                if (window == IntPtr.Zero)
                {
                    window = objc_msgSend(app, sel_registerName("keyWindow"));
                }

                if (window == IntPtr.Zero)
                {
                    return;
                }

                objc_msgSend_void_ptr(
                    window,
                    sel_registerName("makeKeyAndOrderFront:"),
                    IntPtr.Zero
                );
            }
            catch
            {
                // Optional hardening — activation without key-window still helps.
            }
        }

        [DllImport(LibObjC)]
        private static extern IntPtr objc_getClass(string name);

        [DllImport(LibObjC)]
        private static extern IntPtr sel_registerName(string name);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr recv, IntPtr sel);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_bool(IntPtr recv, IntPtr sel, bool arg);

        [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_ptr(IntPtr recv, IntPtr sel, IntPtr arg);

        [DllImport(LibSystem)]
        private static extern IntPtr dlopen(string path, int mode);
#endif
    }
}
