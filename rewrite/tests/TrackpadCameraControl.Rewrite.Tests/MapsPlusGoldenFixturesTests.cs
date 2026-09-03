using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>
    /// Tier A — golden Maps+ fixtures: hand-built frames through style-table resolve,
    /// session, and apply against a queue-only camera fake (harnesses L10).
    /// Asserts camera outcomes / op resolution — not latch fields alone.
    /// </summary>
    public class MapsPlusGoldenFixturesTests
    {
        [Fact]
        public void MapsPlus_TwoFingerPan_MovesTargetOnXZ()
        {
            ModSettings settings = MapsPlusSettings();
            settings.PanGainX = 1f;
            settings.PanGainY = 1f;
            settings.SignInvertPanX = false;
            settings.SignInvertPanY = false;
            var session = new GestureSession();
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
            };

            CameraOp ops = session.Process(Frame(dx: 0.1f), settings);
            Assert.Equal(CameraOp.Pan, ops);

            CameraApplicator.Apply(ops, 0.1f, 0f, 0f, 0f, settings, cam);

            Assert.True(cam.TargetX != 0f || cam.TargetZ != 0f);
            Assert.Equal(0.1f, cam.TargetX, 3);
            Assert.Equal(0f, cam.TargetZ, 3);
        }

        [Fact]
        public void MapsPlus_PinchZoom_DecreasesSize()
        {
            ModSettings settings = MapsPlusSettings();
            settings.ZoomGain = 1f;
            var session = new GestureSession();
            var cam = new FakeCameraController { Size = 100f };

            CameraOp ops = session.Process(Frame(pinch: 0.1f), settings);
            Assert.Equal(CameraOp.Zoom, ops);

            CameraApplicator.Apply(ops, 0f, 0f, 0.1f, 0f, settings, cam);

            Assert.Equal(90f, cam.Size, 3);
        }

        [Fact]
        public void MapsPlus_TwoFingerRotate_ChangesYaw()
        {
            ModSettings settings = MapsPlusSettings();
            settings.RotateGain = 2f;
            var session = new GestureSession();
            var cam = new FakeCameraController { AngleX = 0f };

            CameraOp ops = session.Process(Frame(rotate: 0.5f), settings);
            Assert.Equal(CameraOp.Rotate, ops);

            CameraApplicator.Apply(ops, 0f, 0f, 0f, 0.5f, settings, cam);

            Assert.Equal(1f, cam.AngleX, 3);
        }

        [Fact]
        public void MapsPlus_OptionTwoFingerOrbit_QueuesVelocityThenFlushesToAngles()
        {
            ModSettings settings = MapsPlusSettings();
            settings.OrbitYawGain = 1f;
            settings.OrbitPitchGain = 1f;
            var session = new GestureSession();
            var cam = new FakeCameraController { AngleX = 10f, AngleY = 20f };

            CameraOp ops = session.Process(
                Frame(dx: 5f, dy: -2f, modifiers: (uint)GestureModifiers.Option),
                settings
            );
            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);

            float yawBefore = cam.AngleX;
            float pitchBefore = cam.AngleY;
            CameraApplicator.Apply(ops, 5f, -2f, 0f, 0f, settings, cam);

            // Queue-only: angles unchanged until vanilla damp → flush → integrate.
            Assert.Equal(yawBefore, cam.AngleX, 3);
            Assert.Equal(pitchBefore, cam.AngleY, 3);
            Assert.True(cam.AddAngleVelocityCallCount > 0);
            Assert.Equal(5f, cam.PendingYaw, 3);
            Assert.Equal(-2f, cam.PendingPitch, 3);

            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(18f, cam.AngleY, 3);
        }

        [Fact]
        public void MapsPlus_OrbitLatch_ContinuesAfterOptionReleased_StillOrbitsNotPans()
        {
            ModSettings settings = MapsPlusSettings();
            settings.OrbitYawGain = 1f;
            settings.OrbitPitchGain = 1f;
            var session = new GestureSession();
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 30f };

            CameraOp first = session.Process(
                Frame(dx: 0.02f, modifiers: (uint)GestureModifiers.Option),
                settings
            );
            Assert.True((first & CameraOp.Orbit) != 0);

            CameraOp second = session.Process(Frame(dx: 3f, modifiers: 0), settings);
            Assert.True((second & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, second & CameraOp.Pan);
            Assert.Equal(CameraOp.None, second & CameraOp.Zoom);

            CameraApplicator.Apply(second, 3f, 0f, 0f, 0f, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(3f, cam.AngleX, 3);
            Assert.Equal(30f, cam.AngleY, 3);
        }

        [Fact]
        public void MapsPlus_OrbitThenRotate_HardHandoff_FreezesPitchAndStopsOrbitVelocity()
        {
            ModSettings settings = MapsPlusSettings();
            settings.OrbitYawGain = 1f;
            settings.OrbitPitchGain = 1f;
            settings.RotateGain = 1f;
            var session = new GestureSession();
            var cam = new FakeCameraController
            {
                AngleX = 0f,
                AngleY = 8f,
                Size = 40f,
            };

            CameraOp orbit = session.Process(
                Frame(dx: 4f, dy: -2f, modifiers: (uint)GestureModifiers.Option),
                settings
            );
            Assert.True((orbit & CameraOp.Orbit) != 0);
            CameraApplicator.Apply(orbit, 4f, -2f, 0f, 0f, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            float pitchAfterOrbit = cam.AngleY;
            float yawAfterOrbit = cam.AngleX;
            int addsAfterOrbit = cam.AddAngleVelocityCallCount;
            Assert.True(addsAfterOrbit > 0);

            // End contact so rotate can own the next contact (orbit latch clears).
            session.Process(Frame(phase: GesturePhase.Ended), settings);

            CameraOp rotate = session.Process(Frame(rotate: 2f), settings);
            Assert.Equal(CameraOp.Rotate, rotate);
            CameraApplicator.Apply(rotate, 0f, 0f, 0f, 2f, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(addsAfterOrbit, cam.AddAngleVelocityCallCount);
            Assert.Equal(0f, cam.AngleVelocityX, 3);
            Assert.Equal(0f, cam.AngleVelocityY, 3);
            Assert.Equal(pitchAfterOrbit, cam.AngleY, 3);
            Assert.Equal(yawAfterOrbit + 2f, cam.AngleX, 3);
        }

        [Fact]
        public void FeelGainScaling_FastZoom_AmplifiesSizeDeltaVsFactory()
        {
            ModSettings factory = MapsPlusSettings();
            factory.ZoomGain = ModSettings.CreateFactoryDefaults().ZoomGain;

            ModSettings fast = MapsPlusSettings();
            FeelProfiles.ApplyFast(fast);
            Assert.True(fast.ZoomGain > factory.ZoomGain);

            var camFactory = new FakeCameraController { Size = 200f };
            var camFast = new FakeCameraController { Size = 200f };
            const float pinch = 0.1f;

            CameraApplicator.Apply(CameraOp.Zoom, 0f, 0f, pinch, 0f, factory, camFactory);
            CameraApplicator.Apply(CameraOp.Zoom, 0f, 0f, pinch, 0f, fast, camFast);

            Assert.True(camFast.Size < camFactory.Size);
            float expectedFactory = 200f * (1f - pinch * factory.ZoomGain);
            float expectedFast = 200f * (1f - pinch * fast.ZoomGain);
            Assert.Equal(expectedFactory, camFactory.Size, 3);
            Assert.Equal(expectedFast, camFast.Size, 3);
        }

        [Fact]
        public void ResolveCandidates_MapsPlus_SeedsMatchChords()
        {
            ModSettings settings = MapsPlusSettings();

            Assert.Equal(
                CameraOp.Pan,
                StyleBindingResolver.ResolveCandidates(Frame(dx: 0.02f), settings, false)
            );
            Assert.Equal(
                CameraOp.Zoom,
                StyleBindingResolver.ResolveCandidates(Frame(pinch: 0.05f), settings, false)
            );
            Assert.Equal(
                CameraOp.Rotate,
                StyleBindingResolver.ResolveCandidates(Frame(rotate: 0.05f), settings, false)
            );
            Assert.Equal(
                CameraOp.Orbit,
                StyleBindingResolver.ResolveCandidates(
                    Frame(dx: 0.02f, modifiers: (uint)GestureModifiers.Option),
                    settings,
                    false
                )
            );
        }

        [Fact]
        public void InjectedPipeline_MapsPlusPinch_ZoomsFakeCamera()
        {
            ModSettings settings = MapsPlusSettings();
            settings.ZoomGain = 1f;
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController { Size = 200f };
            var pipeline = new GesturePipeline(settings, inject, cam);

            inject.Enqueue(Frame(pinch: 0.1f));
            pipeline.Tick();

            Assert.Equal(180f, cam.Size, 3);
        }

        private static ModSettings MapsPlusSettings()
        {
            var settings = new ModSettings();
            settings.ApplyGesturePreset(GesturePreset.MapsPlus);
            settings.PinchDeadband = 0.001f;
            settings.MotionDeadband = 0.001f;
            settings.RotateDeadband = 0.001f;
            settings.GestureResolveMode = GestureResolveMode.Concurrent;
            return settings;
        }

        private static GestureFrame Frame(
            int fingers = 2,
            float pinch = 0f,
            float dx = 0f,
            float dy = 0f,
            float rotate = 0f,
            uint modifiers = 0,
            GesturePhase phase = GesturePhase.Changed
        )
        {
            return new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
                fingerCount = fingers,
                phase = (int)phase,
                pinchScaleDelta = pinch,
                centroidDeltaX = dx,
                centroidDeltaY = dy,
                rotateDelta = rotate,
                modifiers = modifiers,
            };
        }
    }
}
