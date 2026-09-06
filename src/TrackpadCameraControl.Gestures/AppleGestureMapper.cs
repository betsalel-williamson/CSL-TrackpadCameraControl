namespace TrackpadCameraControl.Gestures
{
    /// <summary>AppKit NSEvent fields → GestureFrame. No camera ops. Honest finger counts.</summary>
    public static class AppleGestureMapper
    {
        public const ulong EventTypeRotate = 18;
        public const ulong EventTypeBeginGesture = 19;
        public const ulong EventTypeEndGesture = 20;
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

        /// <summary>
        /// AppKit scroll / magnify / rotate are two-finger trackpad gestures on macOS.
        /// EndGesture reports zero fingers (lift). Callers may override when OS exposes a count.
        /// </summary>
        public const int AppKitActiveFingerCount = 2;

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
                fingerCount: -1,
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
            return TryMap(
                eventType,
                nsPhase,
                modifierFlags,
                scrollingDeltaX,
                scrollingDeltaY,
                magnification,
                rotationDegrees,
                hasPreciseScrollingDeltas,
                fingerCount: -1,
                out frame
            );
        }

        /// <param name="fingerCount">
        /// Honest contact count from the OS when available; pass negative to use AppKit defaults
        /// (2 for active scroll/magnify/rotate, 0 for EndGesture).
        /// </param>
        public static bool TryMap(
            ulong eventType,
            ulong nsPhase,
            ulong modifierFlags,
            double scrollingDeltaX,
            double scrollingDeltaY,
            double magnification,
            float rotationDegrees,
            bool hasPreciseScrollingDeltas,
            int fingerCount,
            out GestureFrame frame
        )
        {
            frame = default(GestureFrame);
            if (eventType == EventTypeSwipe || eventType == EventTypeBeginGesture)
            {
                return false;
            }

            if (eventType == EventTypeEndGesture)
            {
                int endFingers = fingerCount >= 0 ? fingerCount : 0;
                frame = new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    timestampNs = 0,
                    fingerCount = endFingers,
                    phase = (int)GesturePhase.Ended,
                    centroidDeltaX = 0f,
                    centroidDeltaY = 0f,
                    pinchScaleDelta = 0f,
                    rotateDelta = 0f,
                    modifiers = MapModifiers(modifierFlags),
                };
                return true;
            }

            float dx = 0f;
            float dy = 0f;
            float pinch = 0f;
            float rot = 0f;

            if (eventType == EventTypeScrollWheel)
            {
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

            int fingers = fingerCount >= 0 ? fingerCount : AppKitActiveFingerCount;
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
