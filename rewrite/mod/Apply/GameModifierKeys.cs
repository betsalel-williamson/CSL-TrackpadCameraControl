using TrackpadCameraControl.Gestures;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Merge Unity keyboard modifier state into capture frames (Cities adapter under Apply/).
    /// In-process AppKit already supplies NSEvent modifierFlags; this OR-merge covers
    /// held-key edge cases when event flags lag game focus.
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
