using System.Collections.Generic;
using System.Xml.Serialization;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Schema 1–2 XML shape (Sensitivity / ButtonScale / Deadzone / LowPass / Invert).
    /// Used only to deserialize old settings.xml before migrating to <see cref="ModSettings"/>.
    /// </summary>
    public class LegacyModSettings
    {
        public GesturePreset GesturePreset { get; set; } = GesturePreset.MapsPlus;
        public GestureResolveMode GestureResolveMode { get; set; } = GestureResolveMode.Concurrent;

        public bool AssistUiEnabled { get; set; } = true;
        public bool PanEnabled { get; set; } = true;
        public bool ZoomEnabled { get; set; } = true;
        public bool YawEnabled { get; set; } = true;
        public bool OrbitEnabled { get; set; } = true;
        public OrbitTrigger OrbitTrigger { get; set; } = OrbitTrigger.ModifierPlusTwoFinger;

        public float PanSensitivityX { get; set; } = 0.005f;
        public float PanSensitivityY { get; set; } = 0.005f;
        public float OrbitYawSensitivity { get; set; } = 0.10f;
        public float OrbitPitchSensitivity { get; set; } = 0.10f;
        public float ZoomSensitivity { get; set; } = 1.00f;
        public float YawRotateSensitivity { get; set; } = 2.00f;

        public float OrbitPitchMin { get; set; } = 0f;
        public float OrbitPitchMax { get; set; } = 90f;

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

        public float MotionDeadzone { get; set; } = 0.1f;
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

        public string ActiveFeelPresetName { get; set; } = FeelProfiles.NameDefault;

        public ModSettings ToModSettings()
        {
            return new ModSettings
            {
                GesturePreset = GesturePreset,
                GestureResolveMode = GestureResolveMode,
                AssistUiEnabled = AssistUiEnabled,
                PanEnabled = PanEnabled,
                ZoomEnabled = ZoomEnabled,
                RotateEnabled = YawEnabled,
                OrbitEnabled = OrbitEnabled,
                PanGainX = PanSensitivityX,
                PanGainY = PanSensitivityY,
                OrbitYawGain = OrbitYawSensitivity,
                OrbitPitchGain = OrbitPitchSensitivity,
                ZoomGain = ZoomSensitivity,
                RotateGain = YawRotateSensitivity,
                PanStepX = PanButtonScaleX,
                PanStepY = PanButtonScaleY,
                OrbitYawStep = OrbitYawButtonScale,
                OrbitPitchStep = OrbitPitchButtonScale,
                ZoomStep = ZoomButtonScale,
                RotateStep = YawRotateButtonScale,
                SignInvertPanX = InvertPanX,
                SignInvertPanY = InvertPanY,
                SignInvertOrbitYaw = InvertOrbitYaw,
                SignInvertOrbitPitch = InvertOrbitPitch,
                SignInvertZoom = InvertZoom,
                SignInvertRotate = InvertYawRotate,
                MotionDeadband = MotionDeadzone,
                PinchDeadband = PinchEpsilon,
                RotateDeadband = RotateEpsilon,
                RequireGameFocus = RequireGameFocus,
                IgnoreOverUi = IgnoreOverUi,
                DebugOverlay = DebugOverlay,
                ActiveFeelPresetName = ActiveFeelPresetName,
                StyleTable = MapsPlusSeed.CreateTable(),
            };
        }
    }

    [XmlRoot("TrackpadCameraControlSettings")]
    public class LegacySettingsEnvelope
    {
        public int SchemaVersion { get; set; }

        public LegacyModSettings Current { get; set; }

        [XmlArray("UserPresets")]
        [XmlArrayItem("Preset")]
        public List<LegacyNamedPreset> UserPresets { get; set; }
    }

    public class LegacyNamedPreset
    {
        public string Name { get; set; }

        public LegacyModSettings Settings { get; set; }
    }
}
