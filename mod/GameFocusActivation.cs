using System;
using System.Runtime.InteropServices;

namespace TrackpadCameraControl
{
    /// <summary>
    /// AppKit activation after city load so the game window owns focus/cursor.
    /// Unity <c>Cursor.visible = false</c> alone does not always hide the macOS hardware
    /// cursor on cold boot — also call <c>NSCursor.hide</c> and re-assert for a few seconds.
    /// Does not cache focus — <see cref="InputGates"/> re-queries each frame.
    /// If dual-cursor remains after this, treat as CS1/Unity Mac quirk (alt-tab workaround).
    /// </summary>
    public static class GameFocusActivation
    {
        private const string LibObjC = "/usr/lib/libobjc.A.dylib";
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const string AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";
        private const string ApplicationServicesPath =
            "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

        /// <summary>Frames to re-assert cursor hide after level load (~3s at 60fps).</summary>
        public const int CursorHideFollowUpFrames = 180;

        private static int _cursorHideFramesRemaining;
        private static int _nsCursorHideDepth;
        private static bool _loggedKeyWindowMiss;

        public static bool TryActivate()
        {
#if HAS_CITIES
            try
            {
                if (dlopen(AppKitPath, 2) == IntPtr.Zero)
                {
                    GestureCaptureLog.Line("focus activate: AppKit dlopen failed");
                    return false;
                }

                dlopen(ApplicationServicesPath, 2);

                IntPtr app = objc_msgSend(
                    objc_getClass("NSApplication"),
                    sel_registerName("sharedApplication")
                );
                if (app == IntPtr.Zero)
                {
                    GestureCaptureLog.Line("focus activate: NSApplication missing");
                    return false;
                }

                objc_msgSend_void_bool(app, sel_registerName("activateIgnoringOtherApps:"), true);
                bool keyWindow = TryMakeKeyAndOrderFront(app);

                HideHardwareAndUnityCursor();
                ArmCursorHideFollowUp(CursorHideFollowUpFrames);

                bool unityVisible = UnityEngine.Cursor.visible;
                bool cgVisible = TryCgCursorIsVisible(out bool cgOk);
                GestureCaptureLog.Line(
                    "focus activated on level load keyWindow="
                        + (keyWindow ? "1" : "0")
                        + " unityCursorVisible="
                        + (unityVisible ? "1" : "0")
                        + " cgCursorVisible="
                        + (cgOk ? (cgVisible ? "1" : "0") : "?")
                        + " nsHideDepth="
                        + _nsCursorHideDepth
                );
                return true;
            }
            catch (Exception ex)
            {
                GestureCaptureLog.Line("focus activate failed: " + ex.GetType().Name);
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
        /// Re-hide hardware + Unity cursor for a few seconds after load while focused and
        /// Options/menu closed. Logs if Unity flips <c>Cursor.visible</c> back on.
        /// </summary>
        public static void TickCursorHideFollowUp(IGameUiContext ui)
        {
#if HAS_CITIES
            if (_cursorHideFramesRemaining <= 0)
            {
                return;
            }

            int remaining = _cursorHideFramesRemaining;
            _cursorHideFramesRemaining--;
            if (ui == null)
            {
                return;
            }

            if (!ui.IsGameFocused() || ui.IsMenuOrOptionsOpen())
            {
                // Let Options/menu keep a usable cursor; do not NSCursor.unhide here (balance
                // stays until follow-up ends or process exits — fail-soft).
                return;
            }

            bool before = UnityEngine.Cursor.visible;
            HideHardwareAndUnityCursor();
            bool after = UnityEngine.Cursor.visible;

            // Log first frame, every ~1s, and last frame — evidence for QA.
            bool sample =
                remaining == CursorHideFollowUpFrames
                || remaining == 120
                || remaining == 60
                || remaining == 1;
            if (sample || before)
            {
                bool cgVisible = TryCgCursorIsVisible(out bool cgOk);
                GestureCaptureLog.Line(
                    "focus cursor follow-up rem="
                        + remaining
                        + " focused=1 menu=0 unityWasVisible="
                        + (before ? "1" : "0")
                        + " unityNow="
                        + (after ? "1" : "0")
                        + " cg="
                        + (cgOk ? (cgVisible ? "1" : "0") : "?")
                );
            }
#else
            _ = ui;
            _cursorHideFramesRemaining = 0;
#endif
        }

#if HAS_CITIES
        private static void HideHardwareAndUnityCursor()
        {
            UnityEngine.Cursor.visible = false;

            // NSCursor.hide is reference-counted — only call when the OS cursor is still visible
            // (or we have never hidden) so we do not stack dozens of hides during follow-up.
            bool cgVisible = TryCgCursorIsVisible(out bool cgOk);
            if ((cgOk && cgVisible) || _nsCursorHideDepth == 0)
            {
                TryNsCursorHide();
            }
        }

        private static void TryNsCursorHide()
        {
            try
            {
                IntPtr nsCursor = objc_getClass("NSCursor");
                if (nsCursor == IntPtr.Zero)
                {
                    return;
                }

                // Class method; hide/unhide are reference-counted by AppKit.
                objc_msgSend(nsCursor, sel_registerName("hide"));
                _nsCursorHideDepth++;
            }
            catch
            {
                // Fail soft.
            }
        }

        private static bool TryMakeKeyAndOrderFront(IntPtr app)
        {
            try
            {
                IntPtr window = objc_msgSend(app, sel_registerName("mainWindow"));
                string which = "main";
                if (window == IntPtr.Zero)
                {
                    window = objc_msgSend(app, sel_registerName("keyWindow"));
                    which = "key";
                }

                if (window == IntPtr.Zero)
                {
                    if (!_loggedKeyWindowMiss)
                    {
                        _loggedKeyWindowMiss = true;
                        GestureCaptureLog.Line("focus activate: no main/key NSWindow yet");
                    }

                    return false;
                }

                objc_msgSend_void_ptr(
                    window,
                    sel_registerName("makeKeyAndOrderFront:"),
                    IntPtr.Zero
                );
                GestureCaptureLog.Line("focus activate: makeKeyAndOrderFront via " + which);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryCgCursorIsVisible(out bool ok)
        {
            ok = false;
            try
            {
                bool visible = CGCursorIsVisible();
                ok = true;
                return visible;
            }
            catch
            {
                return false;
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

        [DllImport(ApplicationServicesPath)]
        private static extern bool CGCursorIsVisible();
#endif
    }
}
