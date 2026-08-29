using System;
using TrackpadCapture;
using Xunit;
using CaptureFrame = TrackpadCapture.GestureFrame;
using CaptureMods = TrackpadCapture.GestureModifiers;
using CapturePhase = TrackpadCapture.GesturePhase;

namespace TrackpadCameraControl.Tests
{
    public class MultitouchGestureSessionTests
    {
        [Fact]
        public void TwoFingerDrag_EmitsCentroidDelta()
        {
            var session = new MultitouchGestureSession();
            Assert.True(
                session.TryUpdate(2, true, 0.4f, 0.5f, true, 0.2f, 0f, 0u, out CaptureFrame began)
            );
            Assert.Equal((int)CapturePhase.Began, began.phase);

            Assert.True(
                session.TryUpdate(2, true, 0.45f, 0.5f, true, 0.2f, 0f, 0u, out CaptureFrame moved)
            );
            Assert.Equal((int)CapturePhase.Changed, moved.phase);
            Assert.InRange(moved.centroidDeltaX, 0.049f, 0.051f);
            Assert.Equal(0f, moved.centroidDeltaY);
            Assert.Equal(0f, moved.pinchScaleDelta);
        }

        [Fact]
        public void PinchOut_EmitsPositiveScaleDelta()
        {
            var session = new MultitouchGestureSession();
            session.TryUpdate(2, true, 0.5f, 0.5f, true, 0.2f, 0f, 0u, out _);
            Assert.True(
                session.TryUpdate(2, true, 0.5f, 0.5f, true, 0.3f, 0f, 0u, out CaptureFrame pinch)
            );
            Assert.InRange(pinch.pinchScaleDelta, 0.49f, 0.51f);
        }

        [Fact]
        public void Rotate_EmitsDegrees()
        {
            var session = new MultitouchGestureSession();
            session.TryUpdate(2, true, 0.5f, 0.5f, true, 0.2f, 0f, 0u, out _);
            float quarter = (float)(Math.PI / 2.0);
            Assert.True(
                session.TryUpdate(
                    2,
                    true,
                    0.5f,
                    0.5f,
                    true,
                    0.2f,
                    quarter,
                    0u,
                    out CaptureFrame rot
                )
            );
            Assert.InRange(rot.rotateDelta, 89f, 91f);
        }

        [Fact]
        public void LiftFingers_EmitsEnded()
        {
            var session = new MultitouchGestureSession();
            session.TryUpdate(2, true, 0.5f, 0.5f, true, 0.2f, 0f, 0u, out _);
            Assert.True(
                session.TryUpdate(0, false, 0f, 0f, false, 0f, 0f, 0u, out CaptureFrame ended)
            );
            Assert.Equal((int)CapturePhase.Ended, ended.phase);
        }

        [Fact]
        public void OptionModifier_Propagates()
        {
            var session = new MultitouchGestureSession();
            uint option = (uint)CaptureMods.Option;
            Assert.True(
                session.TryUpdate(2, true, 0.5f, 0.5f, true, 0.2f, 0f, option, out CaptureFrame f)
            );
            Assert.Equal(option, f.modifiers);
        }
    }
}
