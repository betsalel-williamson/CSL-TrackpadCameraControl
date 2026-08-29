using System;
using System.Runtime.InteropServices;

namespace AppleGestureProbe
{
    /// <summary>
    /// C# AppKit gesture probe: NSWindow + NSApplication event pump.
    /// Logs scroll / magnify / rotate / swipe. No Accessibility. Not a camera backend.
    /// </summary>
    internal static class Program
    {
        private static volatile bool s_running = true;
        private static IntPtr s_window;

        private static int Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.Error.WriteLine("AppleGestureProbe: macOS only.");
                return 1;
            }

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                s_running = false;
            };

            if (!AppKitNative.TryLoad(out string error))
            {
                Console.Error.WriteLine("AppleGestureProbe: " + error);
                return 1;
            }

            IntPtr app = AppKitNative.Msg(
                AppKitNative.GetClass("NSApplication"),
                AppKitNative.Sel("sharedApplication")
            );
            if (app == IntPtr.Zero)
            {
                Console.Error.WriteLine(
                    "AppleGestureProbe: NSApplication.sharedApplication failed"
                );
                return 1;
            }

            AppKitNative.MsgVoid(
                app,
                AppKitNative.Sel("setActivationPolicy:"),
                AppKitNative.ActivationPolicyRegular
            );

            s_window = AppKitNative.Msg(AppKitNative.GetClass("NSWindow"), AppKitNative.Sel("new"));
            nuint style =
                AppKitNative.WindowStyleTitled
                | AppKitNative.WindowStyleClosable
                | AppKitNative.WindowStyleMiniaturizable
                | AppKitNative.WindowStyleResizable;
            AppKitNative.MsgVoid(s_window, AppKitNative.Sel("setStyleMask:"), style);
            AppKitNative.SetContentSize(s_window, 640, 400);

            IntPtr title = AppKitNative.CreateCfString("Apple Gesture Probe");
            AppKitNative.MsgVoid(s_window, AppKitNative.Sel("setTitle:"), title);
            AppKitNative.CFRelease(title);
            AppKitNative.MsgVoid(s_window, AppKitNative.Sel("center"));
            AppKitNative.MsgVoid(s_window, AppKitNative.Sel("makeKeyAndOrderFront:"), IntPtr.Zero);

            AppKitNative.MsgVoid(app, AppKitNative.Sel("activateIgnoringOtherApps:"), true);
            AppKitNative.MsgVoid(app, AppKitNative.Sel("finishLaunching"));

            IntPtr mode = AppKitNative.CreateCfString("kCFRunLoopDefaultMode");
            IntPtr nsDate = AppKitNative.GetClass("NSDate");
            IntPtr selDate = AppKitNative.Sel("dateWithTimeIntervalSinceNow:");
            IntPtr selSend = AppKitNative.Sel("sendEvent:");
            IntPtr selWindows = AppKitNative.Sel("windows");
            IntPtr selCount = AppKitNative.Sel("count");

            Console.Error.WriteLine(
                "apple src=probe type=ready tap=0 — C# NSApplication pump, window-local, no Accessibility. Gesture on the probe window."
            );

            while (s_running)
            {
                IntPtr until = AppKitNative.Msg(nsDate, selDate, 0.25);
                IntPtr ev = AppKitNative.NextEvent(
                    app,
                    AppKitNative.EventMaskAny,
                    until,
                    mode,
                    true
                );
                if (ev != IntPtr.Zero)
                {
                    LogEvent(ev);
                    AppKitNative.MsgVoid(app, selSend, ev);
                }

                IntPtr windows = AppKitNative.Msg(app, selWindows);
                if (windows != IntPtr.Zero && AppKitNative.ULong(windows, selCount) == 0)
                {
                    break;
                }
            }

            AppKitNative.CFRelease(mode);
            Console.Error.WriteLine("AppleGestureProbe: stopped");
            return 0;
        }

        private static void LogEvent(IntPtr ev)
        {
            ulong type = AppKitNative.ULong(ev, AppKitNative.Sel("type"));
            string typeName = TypeName(type);
            if (typeName == null)
            {
                return;
            }

            nuint phase = (nuint)AppKitNative.ULong(ev, AppKitNative.Sel("phase"));
            ulong modsRaw = AppKitNative.ULong(ev, AppKitNative.Sel("modifierFlags"));

            string momentum = null;
            double? sdx = null;
            double? sdy = null;
            double? dx = null;
            double? dy = null;
            bool? precise = null;
            double? mag = null;
            double? rot = null;

            if (type == AppKitNative.EventTypeScrollWheel)
            {
                momentum = PhaseName(
                    (nuint)AppKitNative.ULong(ev, AppKitNative.Sel("momentumPhase"))
                );
                sdx = AppKitNative.Double(ev, AppKitNative.Sel("scrollingDeltaX"));
                sdy = AppKitNative.Double(ev, AppKitNative.Sel("scrollingDeltaY"));
                dx = AppKitNative.Double(ev, AppKitNative.Sel("deltaX"));
                dy = AppKitNative.Double(ev, AppKitNative.Sel("deltaY"));
                precise = AppKitNative.Bool(ev, AppKitNative.Sel("hasPreciseScrollingDeltas"));
            }
            else if (type == AppKitNative.EventTypeMagnify)
            {
                mag = AppKitNative.Double(ev, AppKitNative.Sel("magnification"));
            }
            else if (type == AppKitNative.EventTypeRotate)
            {
                rot = AppKitNative.Float(ev, AppKitNative.Sel("rotation"));
            }
            else if (type == AppKitNative.EventTypeSwipe)
            {
                dx = AppKitNative.Double(ev, AppKitNative.Sel("deltaX"));
                dy = AppKitNative.Double(ev, AppKitNative.Sel("deltaY"));
            }

            string line = AppleGestureLog.Format(
                "local",
                typeName,
                PhaseName(phase),
                ModifierNames(modsRaw),
                momentum,
                sdx,
                sdy,
                dx,
                dy,
                precise,
                mag,
                rot
            );
            Console.Error.WriteLine(line);
        }

        private static string TypeName(ulong type)
        {
            if (type == AppKitNative.EventTypeScrollWheel)
            {
                return "scroll";
            }

            if (type == AppKitNative.EventTypeMagnify)
            {
                return "magnify";
            }

            if (type == AppKitNative.EventTypeRotate)
            {
                return "rotate";
            }

            if (type == AppKitNative.EventTypeSwipe)
            {
                return "swipe";
            }

            if (type == AppKitNative.EventTypeBeginGesture)
            {
                return "begin";
            }

            if (type == AppKitNative.EventTypeEndGesture)
            {
                return "end";
            }

            if (type == AppKitNative.EventTypeGesture)
            {
                return "gesture";
            }

            if (type == AppKitNative.EventTypeSmartMagnify)
            {
                return "smart";
            }

            return null;
        }

        private static string PhaseName(nuint phase)
        {
            if (phase == 0)
            {
                return "none";
            }

            string name = "";
            void Add(nuint bit, string label)
            {
                if ((phase & bit) == 0)
                {
                    return;
                }

                if (name.Length > 0)
                {
                    name += "+";
                }

                name += label;
            }

            Add(AppKitNative.PhaseBegan, "began");
            Add(AppKitNative.PhaseChanged, "changed");
            Add(AppKitNative.PhaseEnded, "ended");
            Add(AppKitNative.PhaseCancelled, "cancelled");
            Add(AppKitNative.Stationary, "stationary");
            Add(AppKitNative.MayBegin, "mayBegin");
            return name.Length == 0 ? "other" : name;
        }

        private static string ModifierNames(ulong flags)
        {
            string names = "";
            void Add(ulong mask, string label)
            {
                if ((flags & mask) == 0)
                {
                    return;
                }

                if (names.Length > 0)
                {
                    names += ",";
                }

                names += label;
            }

            Add(AppKitNative.FlagMaskAlternate, "opt");
            Add(AppKitNative.FlagMaskShift, "shift");
            Add(AppKitNative.FlagMaskCommand, "cmd");
            Add(AppKitNative.FlagMaskControl, "ctrl");
            return names.Length == 0 ? "-" : names;
        }
    }
}
