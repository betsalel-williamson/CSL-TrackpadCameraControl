using System;
using System.Runtime.InteropServices;

namespace TrackpadCapture
{
    /// <summary>macOS modifier key state for GestureModifiers (Maps+ Option = orbit).</summary>
    internal static class MacModifierKeys
    {
        private const int CombinedSessionState = 0; // kCGEventSourceStateCombinedSessionState
        private const int HidSystemState = 1; // kCGEventSourceStateHIDSystemState

        // CGEventFlags
        private const ulong FlagMaskShift = 0x00020000;
        private const ulong FlagMaskControl = 0x00040000;
        private const ulong FlagMaskAlternate = 0x00080000; // Option
        private const ulong FlagMaskCommand = 0x00100000;

        // Carbon GetCurrentKeyModifiers bits (Events.h)
        private const uint CarbonCmdKey = 1u << 8;
        private const uint CarbonShiftKey = 1u << 9;
        private const uint CarbonOptionKey = 1u << 11;
        private const uint CarbonControlKey = 1u << 12;

        public static uint ReadModifiers()
        {
            try
            {
                uint mods = 0;
                mods |= FromFlags(CGEventSourceFlagsState(HidSystemState));
                mods |= FromFlags(CGEventSourceFlagsState(CombinedSessionState));
                mods |= FromCarbon(GetCurrentKeyModifiers());
                return mods;
            }
            catch
            {
                return 0;
            }
        }

        private static uint FromFlags(ulong flags)
        {
            uint mods = 0;
            if ((flags & FlagMaskAlternate) != 0)
            {
                mods |= (uint)GestureModifiers.Option;
            }

            if ((flags & FlagMaskShift) != 0)
            {
                mods |= (uint)GestureModifiers.Shift;
            }

            if ((flags & FlagMaskCommand) != 0)
            {
                mods |= (uint)GestureModifiers.Command;
            }

            if ((flags & FlagMaskControl) != 0)
            {
                mods |= (uint)GestureModifiers.Control;
            }

            return mods;
        }

        private static uint FromCarbon(uint carbon)
        {
            uint mods = 0;
            if ((carbon & CarbonOptionKey) != 0)
            {
                mods |= (uint)GestureModifiers.Option;
            }

            if ((carbon & CarbonShiftKey) != 0)
            {
                mods |= (uint)GestureModifiers.Shift;
            }

            if ((carbon & CarbonCmdKey) != 0)
            {
                mods |= (uint)GestureModifiers.Command;
            }

            if ((carbon & CarbonControlKey) != 0)
            {
                mods |= (uint)GestureModifiers.Control;
            }

            return mods;
        }

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern ulong CGEventSourceFlagsState(int stateID);

        [DllImport(
            "/System/Library/Frameworks/Carbon.framework/Frameworks/HIToolbox.framework/HIToolbox"
        )]
        private static extern uint GetCurrentKeyModifiers();
    }
}
