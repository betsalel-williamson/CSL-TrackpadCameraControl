#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl
{
    /// <summary>
    /// Merge keyboard modifiers from the game process. Out-of-process capture cannot
    /// see Option/Shift while Cities has focus (mods stay 0 on the wire). In-process
    /// Contacts still benefit when HID reads miss a key.
    /// </summary>
    public static class GameModifierKeys
    {
        public static GestureFrame Enrich(GestureFrame frame)
        {
#if HAS_CITIES
            try
            {
                uint mods = frame.modifiers;
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    mods |= (uint)GestureModifiers.Option;
                }

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    mods |= (uint)GestureModifiers.Shift;
                }

                if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))
                {
                    mods |= (uint)GestureModifiers.Command;
                }

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    mods |= (uint)GestureModifiers.Control;
                }

                frame.modifiers = mods;
            }
            catch
            {
                // fail soft
            }
#endif
            return frame;
        }
    }
}
