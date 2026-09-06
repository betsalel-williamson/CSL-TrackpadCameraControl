using TrackpadCameraControl.Gestures;
using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>
    /// Integration: inject source (OS stand-in) + camera fake (game) — one fake per layer.
    /// </summary>
    public class PipelineIntegrationTests
    {
        [Fact]
        public void Tick_InjectPan_MovesCameraWithoutOsOrUnityFakes()
        {
            ModSettings settings = ModSettings.CreateFactoryDefaults();
            settings.PanGainX = 1f;
            settings.PanGainY = 1f;
            settings.SignInvertPanX = false;
            settings.SignInvertPanY = false;
            settings.MotionDeadband = 0.001f;
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
            };
            var pipeline = new GesturePipeline(settings, inject, cam);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.2f,
                    centroidDeltaY = 0f,
                }
            );
            pipeline.Tick();

            Assert.Equal(0.2f, cam.TargetX, 3);
        }

        [Fact]
        public void FakeOsGestureSource_IsOsStandInOnly()
        {
            var os = new FakeOsGestureSource();
            os.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    pinchScaleDelta = 0.1f,
                }
            );
            Assert.True(os.TryDequeue(out GestureFrame frame));
            Assert.Equal(0.1f, frame.pinchScaleDelta);
        }
    }

    /// <summary>OS-layer stand-in only — does not implement camera or Cities ports.</summary>
    public sealed class FakeOsGestureSource : IGestureSource
    {
        private readonly InjectGestureSource _inner = new InjectGestureSource();

        public bool IsConnected => _inner.IsConnected;

        public void Connect()
        {
            _inner.Connect();
        }

        public void Disconnect()
        {
            _inner.Disconnect();
        }

        public bool TryDequeue(out GestureFrame frame)
        {
            return _inner.TryDequeue(out frame);
        }

        public void Enqueue(GestureFrame frame)
        {
            _inner.Enqueue(frame);
        }
    }
}
