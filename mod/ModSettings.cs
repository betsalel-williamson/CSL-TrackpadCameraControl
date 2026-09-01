using System;
using System.Xml.Serialization;

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

    /// <summary>
    /// Live settings model and schema ≥3 XML shape. Persist field names use control-systems
    /// language (gain, step, deadband, filter, sign invert). Player Options still say Sensitivity.
    /// </summary>
    public class ModSettings
    {
        public GesturePreset GesturePreset { get; set; } = GesturePreset.MapsPlus;
        public GestureResolveMode GestureResolveMode { get; set; } = GestureResolveMode.Concurrent;

        public bool AssistUiEnabled { get; set; } = false;
        public bool PanEnabled { get; set; } = true;
        public bool ZoomEnabled { get; set; } = true;
        public bool YawEnabled { get; set; } = true;
        public bool OrbitEnabled { get; set; } = true;
        public OrbitTrigger OrbitTrigger { get; set; } = OrbitTrigger.ModifierPlusTwoFinger;

        public float PanGainX { get; set; } = 0.005f;
        public float PanGainY { get; set; } = 0.005f;

        public float OrbitYawGain { get; set; } = 1.00f;
        public float OrbitPitchGain { get; set; } = 1.00f;
        public float ZoomGain { get; set; } = 1.00f;
        public float YawRotateGain { get; set; } = 2.00f;

        /// <summary>Schema-retained; orbit clamp uses vanilla 0…90 (see CameraApplicator).</summary>
        public float OrbitPitchMin { get; set; } = 0f;

        /// <summary>Schema-retained; orbit clamp uses vanilla 0…90 (see CameraApplicator).</summary>
        public float OrbitPitchMax { get; set; } = 90f;

        public float PanStepX { get; set; } = 0.05f;
        public float PanStepY { get; set; } = 0.05f;
        public float OrbitYawStep { get; set; } = 2f;
        public float OrbitPitchStep { get; set; } = 2f;
        public float ZoomStep { get; set; } = 0.05f;
        public float YawRotateStep { get; set; } = 2f;

        public bool SignInvertPanX { get; set; } = true;
        public bool SignInvertPanY { get; set; }
        public bool SignInvertOrbitYaw { get; set; }
        public bool SignInvertOrbitPitch { get; set; }
        public bool SignInvertZoom { get; set; }
        public bool SignInvertYawRotate { get; set; }

        /// <summary>Centroid |delta| activation threshold (pan / orbit drag); not low-pass filter alpha.</summary>
        public float MotionDeadband { get; set; } = 0.001f;

        /// <summary>Pinch scale-delta activation threshold (zoom); not low-pass filter alpha.</summary>
        public float PinchDeadband { get; set; } = 0.001f;

        /// <summary>Twist rotate-delta activation threshold (yaw); not low-pass filter alpha.</summary>
        public float YawDeadband { get; set; } = 0.001f;

        /// <summary>Schema 3–5 XML: former <c>PinchEpsilon</c> element (deserialize only).</summary>
        [XmlElement("PinchEpsilon")]
        public float PinchEpsilonXml
        {
            set => PinchDeadband = value;
            get => 0f;
        }

        public bool ShouldSerializePinchEpsilonXml() => false;

        /// <summary>Schema 3–5 XML: former <c>RotateEpsilon</c> element (deserialize only).</summary>
        [XmlElement("RotateEpsilon")]
        public float RotateEpsilonXml
        {
            set => YawDeadband = value;
            get => 0f;
        }

        public bool ShouldSerializeRotateEpsilonXml() => false;

        public float FingerCountHysteresis { get; set; } = 0.05f;

        public bool PanFilterEnabled { get; set; }
        public float PanFilterAlpha { get; set; } = 0.3f;
        public bool ZoomFilterEnabled { get; set; }
        public float ZoomFilterAlpha { get; set; } = 0.3f;
        public bool YawFilterEnabled { get; set; }
        public float YawFilterAlpha { get; set; } = 0.3f;
        public bool OrbitFilterEnabled { get; set; }
        public float OrbitFilterAlpha { get; set; } = 0.3f;

        public bool RequireGameFocus { get; set; } = true;
        public bool IgnoreOverUi { get; set; } = true;
        public bool BridgeEnabled { get; set; }
        public bool DebugOverlay { get; set; }

        /// <summary>Debug panel Copy: include OS / device enumeration in clipboard report.</summary>
        public bool IncludeSystemInfoInCopy { get; set; } = true;

        /// <summary>User closed Debug panel via title-bar X; reopen chip shown when Assist on.</summary>
        public bool DebugPanelDismissed { get; set; }

        /// <summary>Persisted Debug panel position (UI relative coords).</summary>
        public float DebugPanelPosX { get; set; } = 40f;

        public float DebugPanelPosY { get; set; } = 60f;

        /// <summary>
        /// AppleGestures = in-process AppKit (default). Contacts = in-process MultitouchSupport.
        /// Overridden by TRACKPAD_CAPTURE_BACKEND when that env var is set.
        /// </summary>
        public CaptureBackend CaptureBackend { get; set; } = CaptureBackend.AppleGestures;

        /// <summary>
        /// Active feel identity for the preset dropdown (Slow / Default / Fast / New Preset / named).
        /// </summary>
        public string ActiveFeelPresetName { get; set; } = FeelProfiles.NameDefault;

        /// <summary>
        /// Seeds orbit trigger (and related defaults) from Maps+ or CAD. Custom is a no-op.
        /// Does not wipe custom scales or filter settings.
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

            PanGainX = other.PanGainX;
            PanGainY = other.PanGainY;
            OrbitYawGain = other.OrbitYawGain;
            OrbitPitchGain = other.OrbitPitchGain;
            ZoomGain = other.ZoomGain;
            YawRotateGain = other.YawRotateGain;

            OrbitPitchMin = other.OrbitPitchMin;
            OrbitPitchMax = other.OrbitPitchMax;

            PanStepX = other.PanStepX;
            PanStepY = other.PanStepY;
            OrbitYawStep = other.OrbitYawStep;
            OrbitPitchStep = other.OrbitPitchStep;
            ZoomStep = other.ZoomStep;
            YawRotateStep = other.YawRotateStep;

            SignInvertPanX = other.SignInvertPanX;
            SignInvertPanY = other.SignInvertPanY;
            SignInvertOrbitYaw = other.SignInvertOrbitYaw;
            SignInvertOrbitPitch = other.SignInvertOrbitPitch;
            SignInvertZoom = other.SignInvertZoom;
            SignInvertYawRotate = other.SignInvertYawRotate;

            MotionDeadband = other.MotionDeadband;
            PinchDeadband = other.PinchDeadband;
            YawDeadband = other.YawDeadband;
            FingerCountHysteresis = other.FingerCountHysteresis;

            PanFilterEnabled = other.PanFilterEnabled;
            PanFilterAlpha = other.PanFilterAlpha;
            ZoomFilterEnabled = other.ZoomFilterEnabled;
            ZoomFilterAlpha = other.ZoomFilterAlpha;
            YawFilterEnabled = other.YawFilterEnabled;
            YawFilterAlpha = other.YawFilterAlpha;
            OrbitFilterEnabled = other.OrbitFilterEnabled;
            OrbitFilterAlpha = other.OrbitFilterAlpha;

            RequireGameFocus = other.RequireGameFocus;
            IgnoreOverUi = other.IgnoreOverUi;
            BridgeEnabled = other.BridgeEnabled;
            DebugOverlay = other.DebugOverlay;
            CaptureBackend = other.CaptureBackend;
            ActiveFeelPresetName = other.ActiveFeelPresetName;
            IncludeSystemInfoInCopy = other.IncludeSystemInfoInCopy;
            DebugPanelDismissed = other.DebugPanelDismissed;
            DebugPanelPosX = other.DebugPanelPosX;
            DebugPanelPosY = other.DebugPanelPosY;
        }

        public static ModSettings CreateFactoryDefaults()
        {
            return new ModSettings();
        }
    }
}
