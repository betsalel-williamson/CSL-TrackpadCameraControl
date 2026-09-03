using System;
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
        /// <summary>
        /// Gesture style identity (Maps+ only on v1 ship). Orthogonal to
        /// <see cref="ActiveFeelPresetName"/> (feel presets).
        /// </summary>
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

        /// <summary>
        /// Per-op trackpad gesture bindings for Debug labels (seeded from Maps+).
        /// Policy resolve reads <see cref="StyleTable"/> (L1).
        /// </summary>
        public TrackpadGesture ZoomGesture { get; set; } = TrackpadGesture.Pinch;

        public GestureModifierKey ZoomGestureModifier { get; set; } = GestureModifierKey.None;

        public TrackpadGesture PanGesture { get; set; } = TrackpadGesture.TwoFingerDrag;

        public GestureModifierKey PanGestureModifier { get; set; } = GestureModifierKey.None;

        public TrackpadGesture RotateGesture { get; set; } = TrackpadGesture.TwoFingerRotate;

        public GestureModifierKey RotateGestureModifier { get; set; } = GestureModifierKey.None;

        public TrackpadGesture OrbitGesture { get; set; } = TrackpadGesture.TwoFingerDrag;

        public GestureModifierKey OrbitGestureModifier { get; set; } = GestureModifierKey.Option;

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

        /// <summary>Centroid |delta| activation threshold (pan / orbit drag).</summary>
        public float MotionDeadband { get; set; } = 0.001f;

        /// <summary>Pinch scale-delta activation threshold (zoom).</summary>
        public float PinchDeadband { get; set; } = 0.001f;

        /// <summary>Twist rotate-delta activation threshold (Rotate op).</summary>
        public float RotateDeadband { get; set; } = 0.001f;

        public bool RequireGameFocus { get; set; } = true;
        public bool IgnoreOverUi { get; set; } = true;
        public bool DebugOverlay { get; set; }

        /// <summary>Debug panel Copy: include OS / device enumeration in clipboard report.</summary>
        public bool IncludeSystemInfoInCopy { get; set; } = true;

        /// <summary>User closed Debug panel via title-bar X; reopen chip shown when Assist on.</summary>
        public bool DebugPanelDismissed { get; set; }

        /// <summary>Persisted Debug panel position (UI relative coords).</summary>
        public float DebugPanelPosX { get; set; } = 40f;

        public float DebugPanelPosY { get; set; } = 60f;

        /// <summary>
        /// Active feel identity for the feel dropdown (Slow / Default / Fast / New Preset / named).
        /// Sensitivity and deadbands only — never gesture-style bindings.
        /// </summary>
        public string ActiveFeelPresetName { get; set; } = FeelProfiles.NameDefault;

        /// <summary>
        /// Seeds Maps+ style bindings into the live style table and per-op display fields.
        /// </summary>
        public void ApplyGesturePreset(GesturePreset preset)
        {
            _ = preset;
            GesturePreset = GesturePreset.MapsPlus;
            StyleTable = MapsPlusSeed.CreateTable();
            TrackpadGestureCatalog.ApplyMapsPlusDefaults(this);
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
            StyleTable = other.StyleTable ?? MapsPlusSeed.CreateTable();
            AssistUiEnabled = other.AssistUiEnabled;
            PanEnabled = other.PanEnabled;
            ZoomEnabled = other.ZoomEnabled;
            RotateEnabled = other.RotateEnabled;
            OrbitEnabled = other.OrbitEnabled;
            ZoomGesture = other.ZoomGesture;
            ZoomGestureModifier = other.ZoomGestureModifier;
            PanGesture = other.PanGesture;
            PanGestureModifier = other.PanGestureModifier;
            RotateGesture = other.RotateGesture;
            RotateGestureModifier = other.RotateGestureModifier;
            OrbitGesture = other.OrbitGesture;
            OrbitGestureModifier = other.OrbitGestureModifier;

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
