using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public delegate void PanClampAction(ref float x, ref float z);

    public sealed class FakeCameraController : ICameraController
    {
        public float Size { get; set; } = 100f;
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float TargetZ { get; set; }
        public float AngleX { get; set; }
        public float AngleY { get; set; }

        /// <summary>Optional AABB for tests. NaN = no AABB clamp.</summary>
        public float MinX { get; set; } = float.NaN;
        public float MaxX { get; set; } = float.NaN;
        public float MinZ { get; set; } = float.NaN;
        public float MaxZ { get; set; } = float.NaN;

        /// <summary>Optional custom clamp (e.g. L-shape). Invoked instead of AABB when set.</summary>
        public PanClampAction ClampPanCustom { get; set; }

        public void ClampPanTarget(ref float x, ref float z)
        {
            if (ClampPanCustom != null)
            {
                ClampPanCustom(ref x, ref z);
                return;
            }

            if (
                float.IsNaN(MinX)
                || float.IsNaN(MaxX)
                || float.IsNaN(MinZ)
                || float.IsNaN(MaxZ)
            )
            {
                return;
            }

            float minX = MinX;
            float maxX = MaxX;
            float minZ = MinZ;
            float maxZ = MaxZ;
            if (minX > maxX)
            {
                float swap = minX;
                minX = maxX;
                maxX = swap;
            }

            if (minZ > maxZ)
            {
                float swap = minZ;
                minZ = maxZ;
                maxZ = swap;
            }

            if (x < minX)
            {
                x = minX;
            }
            else if (x > maxX)
            {
                x = maxX;
            }

            if (z < minZ)
            {
                z = minZ;
            }
            else if (z > maxZ)
            {
                z = maxZ;
            }
        }
    }

    public class ModSettingsPresetTests
    {
        [Fact]
        public void ApplyPreset_MapsPlus_SeedsModifierOrbit()
        {
            var settings = new ModSettings { OrbitTrigger = OrbitTrigger.ThreeFinger };
            settings.ApplyPreset(GesturePreset.MapsPlus);
            Assert.Equal(GesturePreset.MapsPlus, settings.GesturePreset);
            Assert.Equal(OrbitTrigger.ModifierPlusTwoFinger, settings.OrbitTrigger);
        }

        [Fact]
        public void ApplyPreset_CAD_SeedsThreeFingerOrbit()
        {
            var settings = new ModSettings();
            settings.ApplyPreset(GesturePreset.CAD);
            Assert.Equal(GesturePreset.CAD, settings.GesturePreset);
            Assert.Equal(OrbitTrigger.ThreeFinger, settings.OrbitTrigger);
        }
    }

    public class GestureBindingResolverTests
    {
        [Fact]
        public void ResolveCandidates_PinchAboveEpsilon_IncludesZoom()
        {
            var settings = new ModSettings { ZoomEnabled = true, PinchEpsilon = 0.001f };
            var frame = Frame(pinch: 0.05f);

            Assert.Equal(
                CameraOp.Zoom,
                GestureBindingResolver.ResolveCandidates(frame, settings, false) & CameraOp.Zoom
            );
        }

        [Fact]
        public void ResolveCandidates_PinchBelowEpsilon_NoZoom()
        {
            var settings = new ModSettings { ZoomEnabled = true, PinchEpsilon = 0.01f };
            var frame = Frame(pinch: 0.001f);

            Assert.Equal(
                CameraOp.None,
                GestureBindingResolver.ResolveCandidates(frame, settings, false)
            );
        }

        [Fact]
        public void ResolveCandidates_ZoomDisabled_NoZoom()
        {
            var settings = new ModSettings { ZoomEnabled = false, PinchEpsilon = 0.001f };
            var frame = Frame(pinch: 0.05f);

            Assert.Equal(
                CameraOp.None,
                GestureBindingResolver.ResolveCandidates(frame, settings, false) & CameraOp.Zoom
            );
        }

        [Fact]
        public void ResolveCandidates_Concurrent_PinchAndDrag_IncludesZoomAndPan()
        {
            var settings = new ModSettings { PinchEpsilon = 0.001f, MotionDeadzone = 0.001f };
            var frame = Frame(fingers: 2, pinch: 0.05f, dx: 0.02f, dy: 0f);

            CameraOp ops = GestureBindingResolver.ResolveCandidates(frame, settings, false);
            ops = GestureBindingResolver.ExclusiveZoomVersusYaw(ops, frame, settings);
            Assert.True((ops & CameraOp.Zoom) != 0);
            Assert.True((ops & CameraOp.Pan) != 0);
        }

        [Fact]
        public void ExclusiveZoomVersusYaw_KeepsDominantPinch()
        {
            var settings = new ModSettings { PinchEpsilon = 0.001f, RotateEpsilon = 0.001f };
            var frame = Frame(pinch: 0.05f, rotate: 0.01f);
            CameraOp ops = CameraOp.Zoom | CameraOp.Yaw;
            Assert.Equal(
                CameraOp.Zoom,
                GestureBindingResolver.ExclusiveZoomVersusYaw(ops, frame, settings)
            );
        }

        [Fact]
        public void ExclusiveZoomVersusYaw_KeepsDominantRotate()
        {
            var settings = new ModSettings { PinchEpsilon = 0.001f, RotateEpsilon = 0.001f };
            var frame = Frame(pinch: 0.002f, rotate: 0.5f);
            CameraOp ops = CameraOp.Zoom | CameraOp.Yaw;
            Assert.Equal(
                CameraOp.Yaw,
                GestureBindingResolver.ExclusiveZoomVersusYaw(ops, frame, settings)
            );
        }

        [Fact]
        public void OptionTwoFinger_EngagesOrbitNotPan()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadzone = 0.001f,
            };
            var session = new GestureSession();
            CameraOp ops = session.Process(
                Frame(dx: 0.02f, modifiers: (uint)GestureModifiers.Option),
                settings
            );
            Assert.True(session.OrbitLatched);
            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);
        }

        [Fact]
        public void PickPrimary_PrefersOrbitOverZoom()
        {
            Assert.Equal(
                CameraOp.Orbit,
                GestureBindingResolver.PickPrimary(CameraOp.Zoom | CameraOp.Orbit | CameraOp.Pan)
            );
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

    public class GestureSessionTests
    {
        [Fact]
        public void Concurrent_PinchAndDrag_ReturnsBoth()
        {
            var settings = new ModSettings
            {
                GestureResolveMode = GestureResolveMode.Concurrent,
                PinchEpsilon = 0.001f,
                MotionDeadzone = 0.001f,
            };
            var session = new GestureSession();
            var frame = new GestureFrame
            {
                magic = GestureFrame.Magic,
                version = GestureFrame.Version,
                fingerCount = 2,
                phase = (int)GesturePhase.Changed,
                pinchScaleDelta = 0.05f,
                centroidDeltaX = 0.02f,
            };

            CameraOp ops = session.Process(frame, settings);
            Assert.True((ops & CameraOp.Zoom) != 0);
            Assert.True((ops & CameraOp.Pan) != 0);
        }

        [Fact]
        public void OrbitLatch_ContinuesAfterModifierReleased()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadzone = 0.001f,
            };
            var session = new GestureSession();

            session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.02f,
                    modifiers = (uint)GestureModifiers.Option,
                },
                settings
            );
            Assert.True(session.OrbitLatched);

            CameraOp ops = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.03f,
                    modifiers = 0,
                },
                settings
            );

            Assert.True(session.OrbitLatched);
            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);
            Assert.Equal(CameraOp.None, ops & CameraOp.Zoom);
        }

        [Fact]
        public void OrbitLatch_AllowsYaw_SuppressesZoom()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadzone = 0.001f,
                PinchEpsilon = 0.001f,
                RotateEpsilon = 0.001f,
            };
            var session = new GestureSession();

            session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.02f,
                    modifiers = (uint)GestureModifiers.Option,
                },
                settings
            );

            CameraOp ops = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.02f,
                    pinchScaleDelta = 0.1f,
                    rotateDelta = 0.05f,
                    modifiers = (uint)GestureModifiers.Option,
                },
                settings
            );

            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.True((ops & CameraOp.Yaw) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Zoom);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);
        }

        [Fact]
        public void OrbitLatch_ClearsOnEnded()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadzone = 0.001f,
            };
            var session = new GestureSession();

            session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.02f,
                    modifiers = (uint)GestureModifiers.Option,
                },
                settings
            );
            Assert.True(session.OrbitLatched);

            session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Ended,
                },
                settings
            );
            Assert.False(session.OrbitLatched);
        }

        [Fact]
        public void PrimaryOnly_ReturnsSingleOp()
        {
            var settings = new ModSettings
            {
                GestureResolveMode = GestureResolveMode.PrimaryOnly,
                PinchEpsilon = 0.001f,
                MotionDeadzone = 0.001f,
            };
            var session = new GestureSession();
            CameraOp ops = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    pinchScaleDelta = 0.05f,
                    centroidDeltaX = 0.02f,
                },
                settings
            );

            Assert.Equal(CameraOp.Zoom, ops);
        }

        [Fact]
        public void SessionLock_LocksFirstPrimaryUntilEnd()
        {
            var settings = new ModSettings
            {
                GestureResolveMode = GestureResolveMode.SessionLock,
                PinchEpsilon = 0.001f,
                MotionDeadzone = 0.001f,
            };
            var session = new GestureSession();

            CameraOp first = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Began,
                    pinchScaleDelta = 0.05f,
                    centroidDeltaX = 0.02f,
                },
                settings
            );
            Assert.Equal(CameraOp.Zoom, first);

            CameraOp later = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    pinchScaleDelta = 0f,
                    centroidDeltaX = 0.05f,
                },
                settings
            );
            Assert.Equal(CameraOp.None, later);
        }

        [Fact]
        public void CAD_ThreeFinger_EngagesOrbit()
        {
            var settings = new ModSettings();
            settings.ApplyPreset(GesturePreset.CAD);
            settings.MotionDeadzone = 0.001f;
            var session = new GestureSession();

            CameraOp ops = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 3,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.02f,
                },
                settings
            );

            Assert.True(session.OrbitLatched);
            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);
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
            var cam = new FakeCameraController { Size = 100f };
            var settings = new ModSettings { ZoomSensitivity = 1f, InvertZoom = false };

            CameraApplicator.Apply(CameraOp.Zoom, 0, 0, 0.1f, 0, settings, cam);

            Assert.True(cam.Size < 100f);
            Assert.Equal(90f, cam.Size, 3);
        }

        [Fact]
        public void Apply_ClampsMinimumSize()
        {
            var cam = new FakeCameraController { Size = 11f };
            var settings = new ModSettings { ZoomSensitivity = 1f };

            CameraApplicator.Apply(CameraOp.Zoom, 0, 0, 0.9f, 0, settings, cam);

            Assert.Equal(10f, cam.Size);
        }

        [Fact]
        public void Apply_Pan_MovesTargetOnXZ()
        {
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
            };
            var settings = new ModSettings { PanSensitivityX = 1f, PanSensitivityY = 1f };

            CameraApplicator.Apply(CameraOp.Pan, 0.1f, 0f, 0, 0, settings, cam);

            Assert.True(cam.TargetX != 0f || cam.TargetZ != 0f);
        }

        [Fact]
        public void Apply_Orbit_ChangesAngles()
        {
            var cam = new FakeCameraController { AngleX = 10f, AngleY = 20f };
            var settings = new ModSettings { OrbitYawSensitivity = 1f, OrbitPitchSensitivity = 1f };

            CameraApplicator.Apply(CameraOp.Orbit, 5f, -2f, 0, 0, settings, cam);

            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(18f, cam.AngleY, 3);
        }

        [Fact]
        public void Apply_YawRotate_ChangesAngleX()
        {
            var cam = new FakeCameraController { AngleX = 0f };
            var settings = new ModSettings { YawRotateSensitivity = 2f };

            CameraApplicator.Apply(CameraOp.Yaw, 0, 0, 0, 0.5f, settings, cam);

            Assert.Equal(1f, cam.AngleX, 3);
        }

        [Fact]
        public void Apply_ConcurrentZoomAndPan_BothApply()
        {
            var cam = new FakeCameraController
            {
                Size = 100f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
            };
            var settings = new ModSettings
            {
                ZoomSensitivity = 1f,
                PanSensitivityX = 1f,
                PanSensitivityY = 1f,
            };

            CameraApplicator.Apply(CameraOp.Zoom | CameraOp.Pan, 0.1f, 0f, 0.1f, 0, settings, cam);

            Assert.Equal(90f, cam.Size, 3);
            Assert.True(cam.TargetX != 0f || cam.TargetZ != 0f);
        }
    }

    public class HeadlessPipelineE2eTests
    {
        [Fact]
        public void InjectedPinch_ZoomsFakeCamera()
        {
            var settings = new ModSettings
            {
                ZoomEnabled = true,
                ZoomSensitivity = 1f,
                PinchEpsilon = 0.001f,
            };
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController { Size = 200f };
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

        [Fact]
        public void InjectedTwoFingerDrag_PansFakeCamera()
        {
            var settings = new ModSettings
            {
                PanEnabled = true,
                MotionDeadzone = 0.001f,
                PanSensitivityX = 1f,
                PanSensitivityY = 1f,
            };
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
                }
            );

            pipeline.Tick();

            Assert.True(cam.TargetX != 0f || cam.TargetZ != 0f);
        }
    }
}
