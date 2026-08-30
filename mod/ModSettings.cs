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

        public bool AssistUiEnabled { get; set; } = true;
        public bool PanEnabled { get; set; } = true;
        public bool ZoomEnabled { get; set; } = true;
        public bool YawEnabled { get; set; } = true;
        public bool OrbitEnabled { get; set; } = true;
        public OrbitTrigger OrbitTrigger { get; set; } = OrbitTrigger.ModifierPlusTwoFinger;

        public float PanSensitivityX { get; set; } = 0.50f;
        public float PanSensitivityY { get; set; } = 0.50f;
        public float OrbitYawSensitivity { get; set; } = 10.00f;
        public float OrbitPitchSensitivity { get; set; } = 10.00f;
        public float ZoomSensitivity { get; set; } = 1.00f;
        public float YawRotateSensitivity { get; set; } = 2.00f;

        public float OrbitPitchMin { get; set; } = -80f;
        public float OrbitPitchMax { get; set; } = 80f;

        public float PanButtonScaleX { get; set; } = 0.05f;
        public float PanButtonScaleY { get; set; } = 0.05f;
        public float OrbitYawButtonScale { get; set; } = 2f;
        public float OrbitPitchButtonScale { get; set; } = 2f;
        public float ZoomButtonScale { get; set; } = 0.05f;
        public float YawRotateButtonScale { get; set; } = 2f;

        public bool InvertPanX { get; set; } = true;
        public bool InvertPanY { get; set; }
        public bool InvertOrbitYaw { get; set; }
        public bool InvertOrbitPitch { get; set; }
        public bool InvertZoom { get; set; }
        public bool InvertYawRotate { get; set; }

        public float MotionDeadzone { get; set; } = 0.001f;
        public float PinchEpsilon { get; set; } = 0.001f;
        public float RotateEpsilon { get; set; } = 0.001f;
        public float FingerCountHysteresis { get; set; } = 0.05f;

        public bool PanLowPassEnabled { get; set; }
        public float PanLowPassAlpha { get; set; } = 0.3f;
        public bool ZoomLowPassEnabled { get; set; }
        public float ZoomLowPassAlpha { get; set; } = 0.3f;
        public bool YawLowPassEnabled { get; set; }
        public float YawLowPassAlpha { get; set; } = 0.3f;
        public bool OrbitLowPassEnabled { get; set; }
        public float OrbitLowPassAlpha { get; set; } = 0.3f;

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
        /// Does not wipe custom scales or low-pass settings.
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

        /// <summary>Copy all feel and binding fields from another settings instance.</summary>
        public void CopyFrom(ModSettings other)
        {
            if (other == null)
            {
                return;
            }

            GesturePreset = other.GesturePreset;
            GestureResolveMode = other.GestureResolveMode;
            AssistUiEnabled = other.AssistUiEnabled;
            PanEnabled = other.PanEnabled;
            ZoomEnabled = other.ZoomEnabled;
            YawEnabled = other.YawEnabled;
            OrbitEnabled = other.OrbitEnabled;
            OrbitTrigger = other.OrbitTrigger;

            PanSensitivityX = other.PanSensitivityX;
            PanSensitivityY = other.PanSensitivityY;
            OrbitYawSensitivity = other.OrbitYawSensitivity;
            OrbitPitchSensitivity = other.OrbitPitchSensitivity;
            ZoomSensitivity = other.ZoomSensitivity;
            YawRotateSensitivity = other.YawRotateSensitivity;

            OrbitPitchMin = other.OrbitPitchMin;
            OrbitPitchMax = other.OrbitPitchMax;

            PanButtonScaleX = other.PanButtonScaleX;
            PanButtonScaleY = other.PanButtonScaleY;
            OrbitYawButtonScale = other.OrbitYawButtonScale;
            OrbitPitchButtonScale = other.OrbitPitchButtonScale;
            ZoomButtonScale = other.ZoomButtonScale;
            YawRotateButtonScale = other.YawRotateButtonScale;

            InvertPanX = other.InvertPanX;
            InvertPanY = other.InvertPanY;
            InvertOrbitYaw = other.InvertOrbitYaw;
            InvertOrbitPitch = other.InvertOrbitPitch;
            InvertZoom = other.InvertZoom;
            InvertYawRotate = other.InvertYawRotate;

            MotionDeadzone = other.MotionDeadzone;
            PinchEpsilon = other.PinchEpsilon;
            RotateEpsilon = other.RotateEpsilon;
            FingerCountHysteresis = other.FingerCountHysteresis;

            PanLowPassEnabled = other.PanLowPassEnabled;
            PanLowPassAlpha = other.PanLowPassAlpha;
            ZoomLowPassEnabled = other.ZoomLowPassEnabled;
            ZoomLowPassAlpha = other.ZoomLowPassAlpha;
            YawLowPassEnabled = other.YawLowPassEnabled;
            YawLowPassAlpha = other.YawLowPassAlpha;
            OrbitLowPassEnabled = other.OrbitLowPassEnabled;
            OrbitLowPassAlpha = other.OrbitLowPassAlpha;

            RequireGameFocus = other.RequireGameFocus;
            IgnoreOverUi = other.IgnoreOverUi;
            BridgeEnabled = other.BridgeEnabled;
            DebugOverlay = other.DebugOverlay;
            CaptureBackend = other.CaptureBackend;
        }

        public static ModSettings CreateFactoryDefaults()
        {
            return new ModSettings();
        }
    }
}
