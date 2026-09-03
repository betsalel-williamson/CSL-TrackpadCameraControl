using System;
using System.Xml.Serialization;

namespace TrackpadCameraControl.Rewrite
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
        /// <summary>
        /// Gesture style (Maps+ / CAD / future OS layouts): which trackpad chords map to
        /// Zoom/Pan/Rotate/Orbit. Orthogonal to <see cref="ActiveFeelPresetName"/> (Slow/Default/Fast
        /// sensitivity). Changing feel never rewrites gesture bindings.
        /// </summary>
        public GesturePreset GesturePreset { get; set; } = GesturePreset.MapsPlus;

        public GestureResolveMode GestureResolveMode { get; set; } = GestureResolveMode.Concurrent;

        /// <summary>
        /// Live style binding table consumed by Policy resolve (Maps+ seed by default).
        /// Not serialized as free-form remaps on ship — reseeded from MapsPlusSeed on load.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public StyleBindingTable StyleTable { get; set; } = MapsPlusSeed.CreateTable();


        public bool AssistUiEnabled { get; set; } = false;
        public bool PanEnabled { get; set; } = true;
        public bool ZoomEnabled { get; set; } = true;
        public bool RotateEnabled { get; set; } = true;
        public bool OrbitEnabled { get; set; } = true;
        public OrbitTrigger OrbitTrigger { get; set; } = OrbitTrigger.ModifierPlusTwoFinger;

        /// <summary>
        /// Schema 7+: per-op trackpad gesture bindings (composable gesture + modifier).
        /// Owned by <see cref="GesturePreset"/> via <see cref="ApplyGesturePreset"/> — not by feel presets.
        /// Product op name is <b>Rotate</b> (schema 8: <c>RotateGesture*</c>); yaw/pitch axes belong to Orbit.
        /// </summary>
        public TrackpadGesture ZoomGesture { get; set; } = TrackpadGesture.Pinch;

        public GestureModifierKey ZoomGestureModifier { get; set; } = GestureModifierKey.None;

        public TrackpadGesture PanGesture { get; set; } = TrackpadGesture.TwoFingerDrag;

        public GestureModifierKey PanGestureModifier { get; set; } = GestureModifierKey.None;

        public TrackpadGesture RotateGesture { get; set; } = TrackpadGesture.TwoFingerRotate;

        public GestureModifierKey RotateGestureModifier { get; set; } = GestureModifierKey.None;

        /// <summary>Schema 7 XML: former <c>YawGesture</c> element (deserialize only).</summary>
        [XmlElement("YawGesture")]
        public TrackpadGesture YawGestureXml
        {
            set => RotateGesture = value;
            get => RotateGesture;
        }

        public bool ShouldSerializeYawGestureXml() => false;

        /// <summary>Schema 7 XML: former <c>YawGestureModifier</c> element (deserialize only).</summary>
        [XmlElement("YawGestureModifier")]
        public GestureModifierKey YawGestureModifierXml
        {
            set => RotateGestureModifier = value;
            get => RotateGestureModifier;
        }

        public bool ShouldSerializeYawGestureModifierXml() => false;

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

        /// <summary>Centroid |delta| activation threshold (pan / orbit drag); not low-pass filter alpha.</summary>
        public float MotionDeadband { get; set; } = 0.001f;

        /// <summary>Pinch scale-delta activation threshold (zoom); not low-pass filter alpha.</summary>
        public float PinchDeadband { get; set; } = 0.001f;

        /// <summary>Twist rotate-delta activation threshold (Rotate op); not low-pass filter alpha.</summary>
        public float RotateDeadband { get; set; } = 0.001f;

        /// <summary>Schema 3–5 XML: former <c>PinchEpsilon</c> element (deserialize only).</summary>
        [XmlElement("PinchEpsilon")]
        public float PinchEpsilonXml
        {
            set => PinchDeadband = value;
            get => PinchDeadband;
        }

        public bool ShouldSerializePinchEpsilonXml() => false;

        /// <summary>Schema 3–5 XML: former <c>RotateEpsilon</c> element (deserialize only).</summary>
        [XmlElement("RotateEpsilon")]
        public float RotateEpsilonXml
        {
            set => RotateDeadband = value;
            get => RotateDeadband;
        }

        public bool ShouldSerializeRotateEpsilonXml() => false;

        /// <summary>Schema 3–8 XML: former <c>YawDeadband</c> element (deserialize only).</summary>
        [XmlElement("YawDeadband")]
        public float YawDeadbandXml
        {
            set => RotateDeadband = value;
            get => RotateDeadband;
        }

        public bool ShouldSerializeYawDeadbandXml() => false;

        /// <summary>Schema 3–8 XML: former <c>YawEnabled</c> element (deserialize only).</summary>
        [XmlElement("YawEnabled")]
        public bool YawEnabledXml
        {
            set => RotateEnabled = value;
            get => RotateEnabled;
        }

        public bool ShouldSerializeYawEnabledXml() => false;

        /// <summary>Schema 3–8 XML: former <c>YawRotateGain</c> element (deserialize only).</summary>
        /// <remarks>
        /// Alias getters must return the live backing value. Mono's XmlSerializer (Cities) can
        /// self-assign alias properties during deserialize; <c>get =&gt; 0f</c> wiped RotateGain
        /// after a successful <c>&lt;RotateGain&gt;</c> load (empty Debug Rotate fields until Reset).
        /// </remarks>
        [XmlElement("YawRotateGain")]
        public float YawRotateGainXml
        {
            set => RotateGain = value;
            get => RotateGain;
        }

        public bool ShouldSerializeYawRotateGainXml() => false;

        /// <summary>Schema 3–8 XML: former <c>YawRotateStep</c> element (deserialize only).</summary>
        [XmlElement("YawRotateStep")]
        public float YawRotateStepXml
        {
            set => RotateStep = value;
            get => RotateStep;
        }

        public bool ShouldSerializeYawRotateStepXml() => false;

        /// <summary>Schema 3–8 XML: former <c>SignInvertYawRotate</c> element (deserialize only).</summary>
        [XmlElement("SignInvertYawRotate")]
        public bool SignInvertYawRotateXml
        {
            set => SignInvertRotate = value;
            get => SignInvertRotate;
        }

        public bool ShouldSerializeSignInvertYawRotateXml() => false;

        /// <summary>Schema 3–8 XML: former <c>YawFilterEnabled</c> element (deserialize only).</summary>
        [XmlElement("YawFilterEnabled")]
        public bool YawFilterEnabledXml
        {
            set => RotateFilterEnabled = value;
            get => RotateFilterEnabled;
        }

        public bool ShouldSerializeYawFilterEnabledXml() => false;

        /// <summary>Schema 3–8 XML: former <c>YawFilterAlpha</c> element (deserialize only).</summary>
        [XmlElement("YawFilterAlpha")]
        public float YawFilterAlphaXml
        {
            set => RotateFilterAlpha = value;
            get => RotateFilterAlpha;
        }

        public bool ShouldSerializeYawFilterAlphaXml() => false;

        public bool PanFilterEnabled { get; set; }
        public float PanFilterAlpha { get; set; } = 0.3f;
        public bool ZoomFilterEnabled { get; set; }
        public float ZoomFilterAlpha { get; set; } = 0.3f;
        public bool RotateFilterEnabled { get; set; }
        public float RotateFilterAlpha { get; set; } = 0.3f;
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
        /// Active feel identity for the feel dropdown (Slow / Default / Fast / New Preset / named).
        /// Sensitivity and deadbands only — never gesture-style bindings.
        /// </summary>
        public string ActiveFeelPresetName { get; set; } = FeelProfiles.NameDefault;

        /// <summary>
        /// Seeds per-op gesture bindings + orbit trigger from Maps+ or CAD gesture style.
        /// Does not change feel gains, deadbands, or <see cref="ActiveFeelPresetName"/>.
        /// Custom is a no-op.
        /// </summary>
        public void ApplyGesturePreset(GesturePreset preset)
        {
            if (preset == GesturePreset.Custom)
            {
                return;
            }

            GesturePreset = preset;
            if (preset == GesturePreset.MapsPlus)
            {
                StyleTable = MapsPlusSeed.CreateTable();
                TrackpadGestureCatalog.ApplyMapsPlusDefaults(this);
            }
#if ENABLE_CAD_GESTURE_STYLE
            else if (preset == GesturePreset.CAD)
            {
                StyleTable = CadSeed.CreateTable();
                TrackpadGestureCatalog.ApplyCadDefaults(this);
            }
#endif
        }

        /// <summary>Legacy alias for <see cref="ApplyGesturePreset"/>.</summary>
        public void ApplyPreset(GesturePreset preset)
        {
            ApplyGesturePreset(preset);
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
            OrbitTrigger = other.OrbitTrigger;

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
            PanFilterEnabled = other.PanFilterEnabled;
            PanFilterAlpha = other.PanFilterAlpha;
            ZoomFilterEnabled = other.ZoomFilterEnabled;
            ZoomFilterAlpha = other.ZoomFilterAlpha;
            RotateFilterEnabled = other.RotateFilterEnabled;
            RotateFilterAlpha = other.RotateFilterAlpha;
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
            var s = new ModSettings();
            s.StyleTable = MapsPlusSeed.CreateTable();
            return s;
        }
    }
}
