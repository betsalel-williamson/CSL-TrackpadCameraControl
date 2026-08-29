using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public sealed class FakeCameraZoom : ICameraZoom
    {
        public float Size { get; set; } = 100f;
    }

    public class GestureBindingResolverTests
    {
        [Fact]
        public void Resolve_PinchAboveEpsilon_ReturnsZoom()
        {
            var settings = new ModSettings { ZoomEnabled = true, PinchEpsilon = 0.001f };
            var frame = new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
                fingerCount = 2,
                phase = (int)GesturePhase.Changed,
                pinchScaleDelta = 0.05f,
            };

            Assert.Equal(CameraOp.Zoom, GestureBindingResolver.Resolve(frame, settings));
        }

        [Fact]
        public void Resolve_PinchBelowEpsilon_ReturnsNone()
        {
            var settings = new ModSettings { ZoomEnabled = true, PinchEpsilon = 0.01f };
            var frame = new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
                fingerCount = 2,
                phase = (int)GesturePhase.Changed,
                pinchScaleDelta = 0.001f,
            };

            Assert.Equal(CameraOp.None, GestureBindingResolver.Resolve(frame, settings));
        }

        [Fact]
        public void Resolve_ZoomDisabled_ReturnsNone()
        {
            var settings = new ModSettings { ZoomEnabled = false, PinchEpsilon = 0.001f };
            var frame = new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
                fingerCount = 2,
                phase = (int)GesturePhase.Changed,
                pinchScaleDelta = 0.05f,
            };

            Assert.Equal(CameraOp.None, GestureBindingResolver.Resolve(frame, settings));
        }
    }

    public class GestureFrameTests
    {
        [Fact]
        public void Size_Is48Bytes()
        {
            Assert.Equal(48, GestureFrame.Size);
            Assert.Equal(48, System.Runtime.InteropServices.Marshal.SizeOf<GestureFrame>());
        }

        [Fact]
        public void IsValid_RequiresMagicAndVersion()
        {
            var good = new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
            };
            Assert.True(good.IsValid);

            var bad = new GestureFrame { magic = 0, version = GestureFrame.Version };
            Assert.False(bad.IsValid);
        }
    }

    public class CameraApplicatorTests
    {
        [Fact]
        public void Apply_PositivePinch_DecreasesSize()
        {
            var cam = new FakeCameraZoom { Size = 100f };
            var settings = new ModSettings { ZoomSensitivity = 1f, InvertZoom = false };

            CameraApplicator.Apply(CameraOp.Zoom, 0, 0, 0.1f, 0, settings, cam);

            Assert.True(cam.Size < 100f);
            Assert.Equal(90f, cam.Size, 3);
        }

        [Fact]
        public void Apply_ClampsMinimumSize()
        {
            var cam = new FakeCameraZoom { Size = 11f };
            var settings = new ModSettings { ZoomSensitivity = 1f };

            CameraApplicator.Apply(CameraOp.Zoom, 0, 0, 0.9f, 0, settings, cam);

            Assert.Equal(10f, cam.Size);
        }
    }

    public class HeadlessPipelineE2eTests
    {
        [Fact]
        public void InjectedPinch_ZoomsFakeCamera()
        {
            var settings = new ModSettings
            {
                BridgeEnabled = true,
                ZoomEnabled = true,
                ZoomSensitivity = 1f,
                PinchEpsilon = 0.001f,
            };
            var inject = new InjectGestureSource();
            var cam = new FakeCameraZoom { Size = 200f };
            var pipeline = new GesturePipeline(settings, inject, cam);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    pinchScaleDelta = 0.1f,
                }
            );

            pipeline.Tick();

            Assert.Equal(180f, cam.Size, 3);
        }
    }
}
