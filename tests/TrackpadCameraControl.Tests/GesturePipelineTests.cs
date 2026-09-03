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

            if (float.IsNaN(MinX) || float.IsNaN(MaxX) || float.IsNaN(MinZ) || float.IsNaN(MaxZ))
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

        public float AngleVelocityX { get; set; }

        public float AngleVelocityY { get; set; }

        public float PendingYaw { get; private set; }

        public float PendingPitch { get; private set; }

        public int AddAngleVelocityCallCount { get; private set; }

        /// <summary>
        /// Queue only — must not change AngleX/Y (same contract as production).
        /// </summary>
        public void AddAngleVelocity(float yawDelta, float pitchDelta)
        {
            AddAngleVelocityCallCount++;
            PendingYaw += yawDelta;
            PendingPitch += pitchDelta;
        }

        public void FlushPendingAngleVelocity(float deltaTimeSeconds)
        {
            float dt = deltaTimeSeconds;
            if (dt < 0.001f)
            {
                dt = 0.001f;
            }

            AngleVelocityX += PendingYaw / dt;
            AngleVelocityY += PendingPitch / dt;
            PendingYaw = 0f;
            PendingPitch = 0f;
        }

        public void ClearPendingAngleVelocity()
        {
            PendingYaw = 0f;
            PendingPitch = 0f;
        }

        public void ClearAngleVelocity(bool yaw, bool pitch)
        {
            if (yaw)
            {
                AngleVelocityX = 0f;
                PendingYaw = 0f;
            }

            if (pitch)
            {
                AngleVelocityY = 0f;
                PendingPitch = 0f;
            }
        }

        /// <summary>
        /// Mirror vanilla UpdateTargetPosition orbit order: damp → flush (HandleMouseEvents) → integrate.
        /// </summary>
        public static void SimulateVanillaOrbitFrame(
            FakeCameraController cam,
            float inertia,
            float deltaTimeSeconds
        )
        {
            float dt = deltaTimeSeconds;
            if (dt < 0.001f)
            {
                dt = 0.001f;
            }

            float damp = (float)System.Math.Pow(inertia, dt);
            cam.AngleVelocityX *= damp;
            cam.AngleVelocityY *= damp;
            cam.FlushPendingAngleVelocity(dt);
            cam.AngleX += cam.AngleVelocityX * dt;
            float nextPitch = cam.AngleY + cam.AngleVelocityY * dt;
            if (nextPitch < 0f)
            {
                nextPitch = 0f;
            }
            else if (nextPitch > 90f)
            {
                nextPitch = 90f;
            }

            cam.AngleY = nextPitch;
        }
    }

    public class YawDoesNotPitchTests
    {
        [Fact]
        public void ApplyRotation_HardHandoff_ClearsOrbitYawAndPitchVelocity()
        {
            var cam = new FakeCameraController
            {
                AngleX = 10f,
                AngleY = 12f,
                AngleVelocityX = 8f,
                AngleVelocityY = 5f,
                Size = 40f,
            };
            cam.AddAngleVelocity(1f, -2f);
            var settings = new ModSettings { RotateGain = 1f };
            int addsBefore = cam.AddAngleVelocityCallCount;

            CameraApplicator.Apply(CameraOp.Rotate, 0f, 0f, 0f, 3f, settings, cam);

            Assert.Equal(13f, cam.AngleX, 3);
            Assert.Equal(12f, cam.AngleY, 3);
            Assert.Equal(0f, cam.AngleVelocityX, 3);
            Assert.Equal(0f, cam.AngleVelocityY, 3);
            Assert.Equal(0f, cam.PendingYaw, 3);
            Assert.Equal(0f, cam.PendingPitch, 3);
            Assert.Equal(addsBefore, cam.AddAngleVelocityCallCount);
        }

        [Fact]
        public void ApplyRotation_WithOrbitOpSameCall_DoesNotAddAngleVelocity()
        {
            var cam = new FakeCameraController
            {
                AngleX = 0f,
                AngleY = 15f,
                Size = 30f,
            };
            var settings = new ModSettings
            {
                RotateGain = 1f,
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
            };

            CameraApplicator.Apply(
                CameraOp.Rotate | CameraOp.Orbit,
                5f,
                -3f,
                0f,
                2f,
                settings,
                cam
            );

            Assert.Equal(2f, cam.AngleX, 3);
            Assert.Equal(15f, cam.AngleY, 3);
            Assert.Equal(0, cam.AddAngleVelocityCallCount);
            Assert.Equal(0f, cam.PendingPitch, 3);
        }
    }

    public class RotationHardHandoffTests
    {
        [Fact]
        public void AfterOrbitCoast_RotationFreezesPitchAndStopsVelocity()
        {
            var cam = new FakeCameraController
            {
                AngleX = 10f,
                AngleY = 8f,
                Size = 25f,
            };
            var settings = new ModSettings
            {
                RotateGain = 1f,
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
            };

            CameraApplicator.Apply(CameraOp.Orbit, 4f, -2f, 0f, 0f, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );
            float pitchAfterOrbit = cam.AngleY;
            float yawAfterOrbit = cam.AngleX;
            Assert.True(cam.AngleVelocityY != 0f || pitchAfterOrbit != 8f);

            // Simulate leftover coast still in velocity when rotation starts.
            cam.AngleVelocityX = 3f;
            cam.AngleVelocityY = -4f;
            int addsAfterOrbit = cam.AddAngleVelocityCallCount;

            CameraApplicator.Apply(CameraOp.Rotate, 0f, 0.5f, 0f, 2f, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(addsAfterOrbit, cam.AddAngleVelocityCallCount);
            Assert.Equal(pitchAfterOrbit, cam.AngleY, 3);
            Assert.Equal(0f, cam.AngleVelocityX, 3);
            Assert.Equal(0f, cam.AngleVelocityY, 3);
            Assert.Equal(yawAfterOrbit + 2f, cam.AngleX, 3);
        }

        [Fact]
        public void RotateOwned_DropsCompanionScrollOrbitAndPan()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadband = 0.001f,
                RotateDeadband = 0.001f,
            };
            var session = new GestureSession();

            CameraOp rot = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    rotateDelta = 2f,
                },
                settings
            );
            Assert.Equal(CameraOp.Rotate, rot);
            Assert.True(session.RotateOwned);
            Assert.False(session.OrbitLatched);

            CameraOp scroll = session.Process(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaY = 0.5f,
                },
                settings
            );
            Assert.Equal(CameraOp.None, scroll & CameraOp.Pan);
            Assert.Equal(CameraOp.None, scroll & CameraOp.Orbit);
        }

        [Fact]
        public void OptionOrbitLatch_StillIgnoresRotation()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadband = 0.001f,
                RotateDeadband = 0.001f,
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
                    rotateDelta = 3f,
                    modifiers = (uint)GestureModifiers.Option,
                },
                settings
            );
            Assert.True((ops & CameraOp.Orbit) != 0);
            Assert.Equal(CameraOp.None, ops & CameraOp.Rotate);
            Assert.False(session.RotateOwned);
        }

        [Fact]
        public void Pipeline_OrbitThenRotation_NoFurtherAddAngleVelocity_PitchFrozen()
        {
            var settings = new ModSettings
            {
                RotateGain = 1f,
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
                MotionDeadband = 0.001f,
                RotateDeadband = 0.001f,
            };
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController
            {
                AngleX = 10f,
                AngleY = 10f,
                Size = 20f,
            };
            var pipeline = new GesturePipeline(settings, inject, cam);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaY = -0.3f,
                    modifiers = (uint)GestureModifiers.Option,
                }
            );
            pipeline.Tick();
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );
            float pitchAfter = cam.AngleY;
            int addsAfterOrbit = cam.AddAngleVelocityCallCount;
            Assert.True(addsAfterOrbit > 0);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Ended,
                }
            );
            pipeline.Tick();

            cam.AngleVelocityX = 2f;
            cam.AngleVelocityY = -3f;

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    rotateDelta = 4f,
                }
            );
            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaY = 0.4f,
                }
            );
            pipeline.Tick();
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(addsAfterOrbit, cam.AddAngleVelocityCallCount);
            Assert.Equal(pitchAfter, cam.AngleY, 3);
            Assert.Equal(14f, cam.AngleX, 3);
            Assert.Equal(0f, cam.AngleVelocityY, 3);
        }
    }

    /// <summary>
    /// Documents the production policy in CameraControllerZoom.SetAngleComponent:
    /// writing one angle axis must not replace the other axis on m_currentAngle
    /// (full-vector copy caused pitch pops on two-finger rotation vs Q/E).
    /// </summary>
    public class AngleAxisWritePolicyTests
    {
        [Fact]
        public void WritingYawAxis_LeavesPitchComponentsUnchanged()
        {
            // Stand-in for Vector2 target/current: [x=yaw, y=pitch]
            float[] target = { 10f, 40f };
            float[] current = { 10f, 25f }; // pitch still lerping toward 40

            ApplyAxisOnly(target, current, index: 0, value: 13f);

            Assert.Equal(13f, target[0], 3);
            Assert.Equal(40f, target[1], 3);
            Assert.Equal(13f, current[0], 3);
            Assert.Equal(25f, current[1], 3); // must not snap to target pitch
        }

        private static void ApplyAxisOnly(float[] target, float[] current, int index, float value)
        {
            target[index] = value;
            current[index] = value;
        }
    }

    public class OrbitVelocityQueueFlushTests
    {
        [Fact]
        public void AddAngleVelocity_DoesNotChangeAngles_UntilFlush()
        {
            var cam = new FakeCameraController { AngleX = 10f, AngleY = 20f };

            cam.AddAngleVelocity(5f, -2f);

            Assert.Equal(10f, cam.AngleX, 3);
            Assert.Equal(20f, cam.AngleY, 3);
            Assert.Equal(5f, cam.PendingYaw, 3);
            Assert.Equal(-2f, cam.PendingPitch, 3);
        }

        [Fact]
        public void SimulateWithoutFlush_LeavesAnglesUnchanged()
        {
            var cam = new FakeCameraController { AngleX = 10f, AngleY = 20f };
            cam.AddAngleVelocity(5f, -2f);

            // Damp + integrate only — no HandleMouseEvents flush.
            float dt = 1f / 60f;
            float damp = (float)System.Math.Pow(1f, dt);
            cam.AngleVelocityX *= damp;
            cam.AngleVelocityY *= damp;
            cam.AngleX += cam.AngleVelocityX * dt;
            cam.AngleY += cam.AngleVelocityY * dt;

            Assert.Equal(10f, cam.AngleX, 3);
            Assert.Equal(20f, cam.AngleY, 3);
            Assert.Equal(5f, cam.PendingYaw, 3);
        }

        [Fact]
        public void SimulateVanillaOrbitFrame_AppliesPendingAfterDamp()
        {
            var cam = new FakeCameraController { AngleX = 10f, AngleY = 20f };
            cam.AddAngleVelocity(5f, -2f);

            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(18f, cam.AngleY, 3);
            Assert.Equal(0f, cam.PendingYaw, 3);
            Assert.Equal(0f, cam.PendingPitch, 3);
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
        public void ResolveCandidates_PinchAboveDeadband_IncludesZoom()
        {
            var settings = new ModSettings { ZoomEnabled = true, PinchDeadband = 0.001f };
            var frame = Frame(pinch: 0.05f);

            Assert.Equal(
                CameraOp.Zoom,
                GestureBindingResolver.ResolveCandidates(frame, settings, false) & CameraOp.Zoom
            );
        }

        [Fact]
        public void ResolveCandidates_PinchBelowDeadband_NoZoom()
        {
            var settings = new ModSettings { ZoomEnabled = true, PinchDeadband = 0.01f };
            var frame = Frame(pinch: 0.001f);

            Assert.Equal(
                CameraOp.None,
                GestureBindingResolver.ResolveCandidates(frame, settings, false)
            );
        }

        [Fact]
        public void ResolveCandidates_ZoomDisabled_NoZoom()
        {
            var settings = new ModSettings { ZoomEnabled = false, PinchDeadband = 0.001f };
            var frame = Frame(pinch: 0.05f);

            Assert.Equal(
                CameraOp.None,
                GestureBindingResolver.ResolveCandidates(frame, settings, false) & CameraOp.Zoom
            );
        }

        [Fact]
        public void ResolveCandidates_Concurrent_PinchAndDrag_IncludesZoomAndPan()
        {
            var settings = new ModSettings { PinchDeadband = 0.001f, MotionDeadband = 0.001f };
            var frame = Frame(fingers: 2, pinch: 0.05f, dx: 0.02f, dy: 0f);

            CameraOp ops = GestureBindingResolver.ResolveCandidates(frame, settings, false);
            ops = GestureBindingResolver.ExclusiveZoomVersusRotate(ops, frame, settings);
            Assert.True((ops & CameraOp.Zoom) != 0);
            Assert.True((ops & CameraOp.Pan) != 0);
        }

        [Fact]
        public void ExclusiveZoomVersusRotate_KeepsDominantPinch()
        {
            var settings = new ModSettings { PinchDeadband = 0.001f, RotateDeadband = 0.001f };
            var frame = Frame(pinch: 0.05f, rotate: 0.01f);
            CameraOp ops = CameraOp.Zoom | CameraOp.Rotate;
            Assert.Equal(
                CameraOp.Zoom,
                GestureBindingResolver.ExclusiveZoomVersusRotate(ops, frame, settings)
            );
        }

        [Fact]
        public void ExclusiveZoomVersusRotate_KeepsDominantRotate()
        {
            var settings = new ModSettings { PinchDeadband = 0.001f, RotateDeadband = 0.001f };
            var frame = Frame(pinch: 0.002f, rotate: 0.5f);
            CameraOp ops = CameraOp.Zoom | CameraOp.Rotate;
            Assert.Equal(
                CameraOp.Rotate,
                GestureBindingResolver.ExclusiveZoomVersusRotate(ops, frame, settings)
            );
        }

        [Fact]
        public void ExclusiveOrbitVersusRotate_DropsRotateWhenOrbitDominant()
        {
            var settings = new ModSettings { RotateDeadband = 0.001f, MotionDeadband = 0.1f };
            var frame = Frame(dx: 5f, dy: 5f, rotate: 0.001f);
            Assert.Equal(
                CameraOp.Orbit,
                GestureBindingResolver.ExclusiveOrbitVersusRotate(
                    CameraOp.Orbit | CameraOp.Rotate,
                    frame,
                    settings
                )
            );
        }

        [Fact]
        public void ExclusiveOrbitVersusRotate_KeepsRotateWhenTwistDominant()
        {
            var settings = new ModSettings { RotateDeadband = 0.001f, MotionDeadband = 0.1f };
            var frame = Frame(dx: 0.01f, dy: 0.01f, rotate: 2f);
            Assert.Equal(
                CameraOp.Rotate,
                GestureBindingResolver.ExclusiveOrbitVersusRotate(
                    CameraOp.Orbit | CameraOp.Rotate,
                    frame,
                    settings
                )
            );
        }

        [Fact]
        public void ExclusiveOrbitVersusRotate_LegacyOverload_DropsRotate()
        {
            Assert.Equal(
                CameraOp.Orbit,
                GestureBindingResolver.ExclusiveOrbitVersusRotate(CameraOp.Orbit | CameraOp.Rotate)
            );
            Assert.Equal(
                CameraOp.Rotate,
                GestureBindingResolver.ExclusiveOrbitVersusRotate(CameraOp.Rotate)
            );
        }

        [Fact]
        public void OptionTwoFinger_EngagesOrbitNotPan()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadband = 0.001f,
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
                PinchDeadband = 0.001f,
                MotionDeadband = 0.001f,
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
                MotionDeadband = 0.001f,
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
        public void OrbitLatch_SuppressesYawAndZoom()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadband = 0.001f,
                PinchDeadband = 0.001f,
                RotateDeadband = 0.001f,
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
            Assert.Equal(CameraOp.None, ops & CameraOp.Rotate);
            Assert.Equal(CameraOp.None, ops & CameraOp.Zoom);
            Assert.Equal(CameraOp.None, ops & CameraOp.Pan);
        }

        [Fact]
        public void OrbitLatch_ClearsOnEnded()
        {
            var settings = new ModSettings
            {
                OrbitTrigger = OrbitTrigger.ModifierPlusTwoFinger,
                MotionDeadband = 0.001f,
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
                PinchDeadband = 0.001f,
                MotionDeadband = 0.001f,
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
                PinchDeadband = 0.001f,
                MotionDeadband = 0.001f,
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
            settings.MotionDeadband = 0.001f;
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
            var settings = new ModSettings { ZoomGain = 1f, SignInvertZoom = false };

            CameraApplicator.Apply(CameraOp.Zoom, 0, 0, 0.1f, 0, settings, cam);

            Assert.True(cam.Size < 100f);
            Assert.Equal(90f, cam.Size, 3);
        }

        [Fact]
        public void Apply_ClampsMinimumSize()
        {
            var cam = new FakeCameraController { Size = 11f };
            var settings = new ModSettings { ZoomGain = 1f };

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
            var settings = new ModSettings { PanGainX = 1f, PanGainY = 1f };

            CameraApplicator.Apply(CameraOp.Pan, 0.1f, 0f, 0, 0, settings, cam);

            Assert.True(cam.TargetX != 0f || cam.TargetZ != 0f);
        }

        [Fact]
        public void Apply_Orbit_ChangesAngles()
        {
            var cam = new FakeCameraController { AngleX = 10f, AngleY = 20f };
            var settings = new ModSettings { OrbitYawGain = 1f, OrbitPitchGain = 1f };

            CameraApplicator.Apply(CameraOp.Orbit, 5f, -2f, 0, 0, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(15f, cam.AngleX, 3);
            Assert.Equal(18f, cam.AngleY, 3);
        }

        [Fact]
        public void Apply_Rotate_ChangesAngleX()
        {
            var cam = new FakeCameraController { AngleX = 0f };
            var settings = new ModSettings { RotateGain = 2f };

            CameraApplicator.Apply(CameraOp.Rotate, 0, 0, 0, 0.5f, settings, cam);

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
                ZoomGain = 1f,
                PanGainX = 1f,
                PanGainY = 1f,
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
                ZoomGain = 1f,
                PinchDeadband = 0.001f,
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
                MotionDeadband = 0.001f,
                PanGainX = 1f,
                PanGainY = 1f,
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
