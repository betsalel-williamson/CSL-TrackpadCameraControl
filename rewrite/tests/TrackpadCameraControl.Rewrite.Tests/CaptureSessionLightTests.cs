using TrackpadCameraControl.Gestures;
using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>
    /// Light tier B — mapper/session fill of frame primitives (not AppKit hardware).
    /// Proves honest finger defaults and Option modifier propagation into GestureFrame fields.
    /// </summary>
    public class CaptureSessionLightTests
    {
        [Fact]
        public void Mapper_PreciseScroll_EmitsTwoFingerCentroidDelta()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    20.0,
                    -10.0,
                    0,
                    0f,
                    hasPreciseScrollingDeltas: true,
                    out GestureFrame frame
                )
            );
            Assert.Equal(AppleGestureMapper.AppKitActiveFingerCount, frame.fingerCount);
            Assert.Equal(20.0f, frame.centroidDeltaX);
            Assert.Equal(-10.0f, frame.centroidDeltaY);
            Assert.Equal(0f, frame.pinchScaleDelta);
            Assert.Equal(0f, frame.rotateDelta);
        }

        [Fact]
        public void Mapper_Magnify_EmitsPinchWithTwoFingers()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeMagnify,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    0,
                    0,
                    0.05,
                    0f,
                    hasPreciseScrollingDeltas: false,
                    out GestureFrame frame
                )
            );
            Assert.Equal(2, frame.fingerCount);
            Assert.Equal(0.05f, frame.pinchScaleDelta);
        }

        [Fact]
        public void Mapper_Rotate_EmitsRotateDeltaWithTwoFingers()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeRotate,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    0,
                    0,
                    0,
                    12.5f,
                    hasPreciseScrollingDeltas: true,
                    out GestureFrame frame
                )
            );
            Assert.Equal(2, frame.fingerCount);
            Assert.Equal(12.5f, frame.rotateDelta);
        }

        [Fact]
        public void Mapper_OptionModifier_PropagatesOntoFrame()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    AppleGestureMapper.FlagMaskAlternate,
                    1.0,
                    0,
                    0,
                    0f,
                    hasPreciseScrollingDeltas: true,
                    out GestureFrame frame
                )
            );
            Assert.Equal((uint)GestureModifiers.Option, frame.modifiers);
        }

        [Fact]
        public void Mapper_HonestFingerOverride_ThreeFingersPreserved()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    1.0,
                    0,
                    0,
                    0f,
                    hasPreciseScrollingDeltas: true,
                    fingerCount: 3,
                    out GestureFrame frame
                )
            );
            Assert.Equal(3, frame.fingerCount);
        }

        [Fact]
        public void Mapper_EndGesture_EmitsZeroFingers()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeEndGesture,
                    AppleGestureMapper.PhaseEnded,
                    0,
                    0,
                    0,
                    0,
                    0f,
                    out GestureFrame frame
                )
            );
            Assert.Equal(0, frame.fingerCount);
            Assert.Equal((int)GesturePhase.Ended, frame.phase);
        }

        [Fact]
        public void MapperFrame_ThroughSession_OptionScrollResolvesOrbit()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    AppleGestureMapper.FlagMaskAlternate,
                    0.05,
                    0,
                    0,
                    0f,
                    hasPreciseScrollingDeltas: true,
                    out GestureFrame frame
                )
            );

            var settings = new ModSettings();
            settings.ApplyGesturePreset(GesturePreset.MapsPlus);
            settings.MotionDeadband = 0.001f;
            var session = new GestureSession();

            CameraOp ops = session.Process(frame, settings);
            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);
        }
    }
}
