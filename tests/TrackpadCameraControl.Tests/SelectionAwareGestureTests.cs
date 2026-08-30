using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public sealed class FakeSelectionContext : ISelectionContext
    {
        public bool HasSelection { get; set; }
        public float WorldX { get; set; }
        public float WorldY { get; set; }
        public float WorldZ { get; set; }
        public float AppliedYawDegrees { get; set; }
        public int RotateCalls { get; set; }

        public bool TryGetSelectedWorldPosition(out float x, out float y, out float z)
        {
            x = WorldX;
            y = WorldY;
            z = WorldZ;
            return HasSelection;
        }

        public bool TryApplyObjectYawDelta(float deltaDegrees)
        {
            if (!HasSelection)
            {
                return false;
            }

            AppliedYawDegrees += deltaDegrees;
            RotateCalls++;
            return true;
        }
    }

    public class SelectionAwareGestureTests
    {
        [Fact]
        public void Yaw_WithSelection_RotatesObject_NotCamera()
        {
            var cam = new FakeCameraController { AngleX = 40f };
            var selection = new FakeSelectionContext
            {
                HasSelection = true,
                WorldX = 10f,
                WorldY = 0f,
                WorldZ = 20f,
            };
            var settings = new ModSettings { YawRotateGain = 2f };

            CameraApplicator.Apply(
                CameraOp.Yaw,
                0,
                0,
                0,
                0.5f,
                settings,
                cam,
                CameraApplicator.InputModality.Drag,
                selection
            );

            Assert.Equal(40f, cam.AngleX, 3);
            Assert.Equal(1f, selection.AppliedYawDegrees, 3);
            Assert.Equal(1, selection.RotateCalls);
        }

        [Fact]
        public void Yaw_WithoutSelection_YawsCamera()
        {
            var cam = new FakeCameraController { AngleX = 0f };
            var selection = new FakeSelectionContext { HasSelection = false };
            var settings = new ModSettings { YawRotateGain = 2f };

            CameraApplicator.Apply(
                CameraOp.Yaw,
                0,
                0,
                0,
                0.5f,
                settings,
                cam,
                CameraApplicator.InputModality.Drag,
                selection
            );

            Assert.Equal(1f, cam.AngleX, 3);
            Assert.Equal(0, selection.RotateCalls);
        }

        [Fact]
        public void Orbit_WithSelection_PivotsTargetThenOrbits()
        {
            var cam = new FakeCameraController
            {
                TargetX = 0f,
                TargetY = 0f,
                TargetZ = 0f,
                AngleX = 10f,
                AngleY = 30f,
            };
            var selection = new FakeSelectionContext
            {
                HasSelection = true,
                WorldX = 100f,
                WorldY = 5f,
                WorldZ = -50f,
            };
            var settings = new ModSettings
            {
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
                OrbitPitchMin = 0f,
                OrbitPitchMax = 90f,
            };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                5f,
                -2f,
                0,
                0,
                settings,
                cam,
                CameraApplicator.InputModality.Drag,
                selection
            );
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(100f, cam.TargetX, 3);
            Assert.Equal(5f, cam.TargetY, 3);
            Assert.Equal(-50f, cam.TargetZ, 3);
            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(28f, cam.AngleY, 3);
        }

        [Fact]
        public void Orbit_WithSelection_ZeroDelta_StillRefreshesTarget()
        {
            var cam = new FakeCameraController
            {
                TargetX = 3f,
                TargetY = 1f,
                TargetZ = 4f,
                AngleX = 10f,
                AngleY = 30f,
            };
            var selection = new FakeSelectionContext
            {
                HasSelection = true,
                WorldX = 100f,
                WorldY = 5f,
                WorldZ = -50f,
            };
            var settings = new ModSettings
            {
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
                OrbitPitchMin = 0f,
                OrbitPitchMax = 90f,
            };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                0f,
                0f,
                0,
                0,
                settings,
                cam,
                CameraApplicator.InputModality.Drag,
                selection
            );

            Assert.Equal(100f, cam.TargetX, 3);
            Assert.Equal(5f, cam.TargetY, 3);
            Assert.Equal(-50f, cam.TargetZ, 3);
            Assert.Equal(10f, cam.AngleX, 3);
            Assert.Equal(30f, cam.AngleY, 3);
        }

        [Fact]
        public void Orbit_WithoutSelection_LeavesTarget_ChangesAngles()
        {
            var cam = new FakeCameraController
            {
                TargetX = 3f,
                TargetY = 1f,
                TargetZ = 4f,
                AngleX = 10f,
                AngleY = 30f,
            };
            var selection = new FakeSelectionContext { HasSelection = false };
            var settings = new ModSettings
            {
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
                OrbitPitchMin = 0f,
                OrbitPitchMax = 90f,
            };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                5f,
                -2f,
                0,
                0,
                settings,
                cam,
                CameraApplicator.InputModality.Drag,
                selection
            );
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(3f, cam.TargetX, 3);
            Assert.Equal(1f, cam.TargetY, 3);
            Assert.Equal(4f, cam.TargetZ, 3);
            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(28f, cam.AngleY, 3);
        }

        [Fact]
        public void Pipeline_WithSelection_InjectedRotate_DoesNotYawCamera()
        {
            var settings = new ModSettings
            {
                YawEnabled = true,
                YawRotateGain = 1f,
                RotateEpsilon = 0.001f,
            };
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController { AngleX = 12f };
            var selection = new FakeSelectionContext { HasSelection = true };
            var pipeline = new GesturePipeline(settings, inject, cam, selection);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    rotateDelta = 3f,
                }
            );

            pipeline.Tick();

            Assert.Equal(12f, cam.AngleX, 3);
            Assert.Equal(3f, selection.AppliedYawDegrees, 3);
        }

        [Fact]
        public void Pipeline_WithoutSelection_InjectedRotate_YawsCamera()
        {
            var settings = new ModSettings
            {
                YawEnabled = true,
                YawRotateGain = 1f,
                RotateEpsilon = 0.001f,
            };
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController { AngleX = 12f };
            var selection = new FakeSelectionContext { HasSelection = false };
            var pipeline = new GesturePipeline(settings, inject, cam, selection);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    rotateDelta = 3f,
                }
            );

            pipeline.Tick();

            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(0, selection.RotateCalls);
        }

        [Fact]
        public void CitiesSelectionContext_WithoutCities_FailsSoft()
        {
            var ctx = new CitiesSelectionContext();
            Assert.False(ctx.TryGetSelectedWorldPosition(out _, out _, out _));
            Assert.False(ctx.TryApplyObjectYawDelta(5f));
        }
    }
}
