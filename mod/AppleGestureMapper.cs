namespace TrackpadCameraControl
{
    /// <summary>AppKit NSEvent fields → GestureFrame. No camera ops.</summary>
    public static class AppleGestureMapper
    {
        public const ulong EventTypeRotate = 18;
        public const ulong EventTypeScrollWheel = 22;
        public const ulong EventTypeMagnify = 30;
        public const ulong EventTypeSwipe = 31;

        public const ulong PhaseBegan = 1u << 0;
        public const ulong PhaseChanged = 1u << 2;
        public const ulong PhaseEnded = 1u << 3;
        public const ulong PhaseCancelled = 1u << 4;

        public const ulong FlagMaskShift = 0x00020000;
        public const ulong FlagMaskControl = 0x00040000;
        public const ulong FlagMaskAlternate = 0x00080000;
        public const ulong FlagMaskCommand = 0x00100000;

        public static bool TryMap(
            ulong eventType,
            ulong nsPhase,
            ulong modifierFlags,
            double scrollingDeltaX,
            double scrollingDeltaY,
            double magnification,
            float rotationDegrees,
            out GestureFrame frame
        )
        {
            return TryMap(
                eventType,
                nsPhase,
                modifierFlags,
                scrollingDeltaX,
                scrollingDeltaY,
                magnification,
                rotationDegrees,
                hasPreciseScrollingDeltas: true,
                out frame
            );
        }

        public static bool TryMap(
            ulong eventType,
            ulong nsPhase,
            ulong modifierFlags,
            double scrollingDeltaX,
            double scrollingDeltaY,
            double magnification,
            float rotationDegrees,
            bool hasPreciseScrollingDeltas,
            out GestureFrame frame
        )
        {
            frame = default;
            if (eventType == EventTypeSwipe)
            {
                return false;
            }

            int fingers = 2;
            float dx = 0f;
            float dy = 0f;
            float pinch = 0f;
            float rot = 0f;

            if (eventType == EventTypeScrollWheel)
            {
                // Mouse wheel (non-precise) must not become pan; leave vanilla zoom alone.
                if (!hasPreciseScrollingDeltas)
                {
                    return false;
                }

                dx = (float)scrollingDeltaX;
                dy = (float)scrollingDeltaY;
            }
            else if (eventType == EventTypeMagnify)
            {
                pinch = (float)magnification;
            }
            else if (eventType == EventTypeRotate)
            {
                rot = rotationDegrees;
            }
            else
            {
                return false;
            }

            frame = new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
                timestampNs = 0,
                fingerCount = fingers,
                phase = (int)MapPhase(nsPhase),
                centroidDeltaX = dx,
                centroidDeltaY = dy,
                pinchScaleDelta = pinch,
                rotateDelta = rot,
                modifiers = MapModifiers(modifierFlags),
            };
            return true;
        }

        public static GesturePhase MapPhase(ulong nsPhase)
        {
            if ((nsPhase & PhaseCancelled) != 0)
            {
                return GesturePhase.Cancelled;
            }

            if ((nsPhase & PhaseEnded) != 0)
            {
                return GesturePhase.Ended;
            }

            if ((nsPhase & PhaseBegan) != 0)
            {
                return GesturePhase.Began;
            }

            return GesturePhase.Changed;
        }

        public static uint MapModifiers(ulong flags)
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
    }
}
