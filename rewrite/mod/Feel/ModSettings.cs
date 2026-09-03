using System.Xml.Serialization;

namespace TrackpadCameraControl.Rewrite
{
    public enum GesturePreset
    {
        MapsPlus,
    }

    public enum GestureResolveMode
    {
        Concurrent,
        SessionLock,
        PrimaryOnly,
    }

    /// <summary>
    /// Live settings model (schema v1 XML). Persist field names use control-systems
    /// language (gain, step, deadband, sign invert). Player Options still say Sensitivity.
    /// </summary>
    public class ModSettings
    {
        public GesturePreset GesturePreset { get; set; } = GesturePreset.MapsPlus;

        public GestureResolveMode GestureResolveMode { get; set; } = GestureResolveMode.Concurrent;

        /// <summary>
        /// Live style binding table consumed by Policy resolve (Maps+ seed by default).
        /// Not serialized — reseeded from MapsPlusSeed on load.
        /// </summary>
        [XmlIgnore]
        public StyleBindingTable StyleTable { get; set; } = MapsPlusSeed.CreateTable();

        public bool AssistUiEnabled { get; set; } = false;
        public bool PanEnabled { get; set; } = true;
        public bool ZoomEnabled { get; set; } = true;
        public bool RotateEnabled { get; set; } = true;
        public bool OrbitEnabled { get; set; } = true;

        public float PanGainX { get; set; } = 0.005f;
        public float PanGainY { get; set; } = 0.005f;

        public float OrbitYawGain { get; set; } = 1.00f;
        public float OrbitPitchGain { get; set; } = 1.00f;
        public float ZoomGain { get; set; } = 1.00f;
        public float RotateGain { get; set; } = 2.00f;

        public float PanStepX { get; set; } = 0.05f;
        public float PanStepY { get; set; } = 0.05f;
        public float OrbitYawStep { get; set; } = 2f;
        public float OrbitPitchStep { get; set; } = 2f;
        public float ZoomStep { get; set; } = 0.05f;
        public float RotateStep { get; set; } = 2f;

        public bool SignInvertPanX { get; set; } = true;
        public bool SignInvertPanY { get; set; }
        public bool SignInvertOrbitYaw { get; set; }
        public bool SignInvertOrbitPitch { get; set; }
        public bool SignInvertZoom { get; set; }
        public bool SignInvertRotate { get; set; }

        public float MotionDeadband { get; set; } = 0.001f;
        public float PinchDeadband { get; set; } = 0.001f;
        public float RotateDeadband { get; set; } = 0.001f;

        public bool RequireGameFocus { get; set; } = true;
        public bool IgnoreOverUi { get; set; } = true;
        public bool DebugOverlay { get; set; }

        public bool IncludeSystemInfoInCopy { get; set; } = true;
        public bool DebugPanelDismissed { get; set; }
        public float DebugPanelPosX { get; set; } = 40f;
        public float DebugPanelPosY { get; set; } = 60f;

        public string ActiveFeelPresetName { get; set; } = FeelProfiles.NameDefault;

        public void ApplyGesturePreset(GesturePreset preset)
        {
            _ = preset;
            GesturePreset = GesturePreset.MapsPlus;
            StyleTable = MapsPlusSeed.CreateTable();
        }

        public void CopyFrom(ModSettings other)
        {
            if (other == null)
            {
                return;
            }

            GesturePreset = other.GesturePreset;
            GestureResolveMode = other.GestureResolveMode;
            StyleTable = other.StyleTable ?? MapsPlusSeed.CreateTable();
            AssistUiEnabled = other.AssistUiEnabled;
            PanEnabled = other.PanEnabled;
            ZoomEnabled = other.ZoomEnabled;
            RotateEnabled = other.RotateEnabled;
            OrbitEnabled = other.OrbitEnabled;

            PanGainX = other.PanGainX;
            PanGainY = other.PanGainY;
            OrbitYawGain = other.OrbitYawGain;
            OrbitPitchGain = other.OrbitPitchGain;
            ZoomGain = other.ZoomGain;
            RotateGain = other.RotateGain;

            PanStepX = other.PanStepX;
            PanStepY = other.PanStepY;
            OrbitYawStep = other.OrbitYawStep;
            OrbitPitchStep = other.OrbitPitchStep;
            ZoomStep = other.ZoomStep;
            RotateStep = other.RotateStep;

            SignInvertPanX = other.SignInvertPanX;
            SignInvertPanY = other.SignInvertPanY;
            SignInvertOrbitYaw = other.SignInvertOrbitYaw;
            SignInvertOrbitPitch = other.SignInvertOrbitPitch;
            SignInvertZoom = other.SignInvertZoom;
            SignInvertRotate = other.SignInvertRotate;

            MotionDeadband = other.MotionDeadband;
            PinchDeadband = other.PinchDeadband;
            RotateDeadband = other.RotateDeadband;

            RequireGameFocus = other.RequireGameFocus;
            IgnoreOverUi = other.IgnoreOverUi;
            DebugOverlay = other.DebugOverlay;
            ActiveFeelPresetName = other.ActiveFeelPresetName;
            IncludeSystemInfoInCopy = other.IncludeSystemInfoInCopy;
            DebugPanelDismissed = other.DebugPanelDismissed;
            DebugPanelPosX = other.DebugPanelPosX;
            DebugPanelPosY = other.DebugPanelPosY;
        }

        public static ModSettings CreateFactoryDefaults()
        {
            var s = new ModSettings();
            s.StyleTable = MapsPlusSeed.CreateTable();
            return s;
        }
    }
}
