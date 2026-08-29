using System;

namespace TrackpadCameraControl
{
    public enum GesturePreset
    {
        MapsPlus,
        CAD,
        Custom,
    }

    public enum OrbitTrigger
    {
        ModifierPlusTwoFinger,
        ThreeFinger,
        Both,
        Off,
    }

    public enum GestureResolveMode
    {
        Concurrent,
        SessionLock,
        PrimaryOnly,
    }

    public class ModSettings
    {
        public GesturePreset GesturePreset { get; set; } = GesturePreset.MapsPlus;
        public GestureResolveMode GestureResolveMode { get; set; } = GestureResolveMode.Concurrent;

        public bool PanEnabled { get; set; } = true;
        public bool ZoomEnabled { get; set; } = true;
        public bool YawEnabled { get; set; } = true;
        public bool OrbitEnabled { get; set; } = true;
        public OrbitTrigger OrbitTrigger { get; set; } = OrbitTrigger.ModifierPlusTwoFinger;

        public float PanSensitivityX { get; set; } = 1f;
        public float PanSensitivityY { get; set; } = 1f;
        public float OrbitYawSensitivity { get; set; } = 1f;
        public float OrbitPitchSensitivity { get; set; } = 1f;
        public float ZoomSensitivity { get; set; } = 1f;
        public float YawRotateSensitivity { get; set; } = 1f;

        public bool InvertPanX { get; set; }
        public bool InvertPanY { get; set; }
        public bool InvertOrbitYaw { get; set; }
        public bool InvertOrbitPitch { get; set; }
        public bool InvertZoom { get; set; }
        public bool InvertYawRotate { get; set; }

        public float MotionDeadzone { get; set; } = 0.001f;
        public float PinchEpsilon { get; set; } = 0.001f;
        public float RotateEpsilon { get; set; } = 0.001f;
        public float FingerCountHysteresis { get; set; } = 0.05f;
        public float Smoothing { get; set; } // 0 = off

        public bool RequireGameFocus { get; set; } = true;
        public bool IgnoreOverUi { get; set; } = true;
        public bool BridgeEnabled { get; set; }
        public bool DebugOverlay { get; set; }

        /// <summary>
        /// AppleGestures = in-process AppKit (default). Contacts = in-process MultitouchSupport.
        /// Overridden by TRACKPAD_CAPTURE_BACKEND when that env var is set.
        /// </summary>
        public CaptureBackend CaptureBackend { get; set; } = CaptureBackend.AppleGestures;

        /// <summary>
        /// Seeds orbit trigger (and related defaults) from Maps+ or CAD. Custom is a no-op.
        /// </summary>
        public void ApplyPreset(GesturePreset preset)
        {
            if (preset == GesturePreset.Custom)
            {
                return;
            }

            GesturePreset = preset;
            if (preset == GesturePreset.MapsPlus)
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger;
            }
            else if (preset == GesturePreset.CAD)
            {
                OrbitTrigger = OrbitTrigger.ThreeFinger;
            }
        }
    }
}
