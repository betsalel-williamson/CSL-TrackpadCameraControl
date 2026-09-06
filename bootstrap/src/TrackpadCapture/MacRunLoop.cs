using System;

namespace TrackpadCapture
{
    /// <summary>Pumps the default CoreFoundation run loop (needed for Multitouch callbacks).</summary>
    public static class MacRunLoop
    {
        private static readonly object Gate = new object();
        private static IntPtr s_defaultMode;

        public static void RunInDefaultMode(double seconds, bool returnAfterSourceHandled)
        {
            IntPtr mode = GetDefaultMode();
            if (mode == IntPtr.Zero)
            {
                return;
            }

            MultitouchNative.CFRunLoopRunInMode(mode, seconds, returnAfterSourceHandled);
        }

        private static IntPtr GetDefaultMode()
        {
            lock (Gate)
            {
                if (s_defaultMode == IntPtr.Zero)
                {
                    s_defaultMode = MultitouchNative.CreateDefaultRunLoopMode();
                }

                return s_defaultMode;
            }
        }
    }
}
