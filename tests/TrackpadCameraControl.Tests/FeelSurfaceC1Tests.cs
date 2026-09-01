using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class FeatureFlagsTests
    {
        [Fact]
        public void AllProductFlags_DefaultOff()
        {
            Assert.False(FeatureFlags.EnableCadGestureStyle);
            Assert.False(FeatureFlags.EnableContactsCapture);
            Assert.False(FeatureFlags.EnableAssistChrome);
        }
    }

    public class FactoryFeelDefaultsTests
    {
        [Fact]
        public void CreateFactoryDefaults_MatchesPlaytestFeel()
        {
            // Single place that locks absolute factory feel numbers as product contract.
            ModSettings s = ModSettings.CreateFactoryDefaults();

            Assert.True(s.SignInvertPanX);
            Assert.False(s.SignInvertPanY);
            Assert.Equal(0.005f, s.PanGainX);
            Assert.Equal(0.005f, s.PanGainY);
            Assert.Equal(1.00f, s.ZoomGain);
            Assert.Equal(2.00f, s.YawRotateGain);
            Assert.Equal(1.00f, s.OrbitYawGain);
            Assert.Equal(1.00f, s.OrbitPitchGain);
            Assert.Equal(0f, s.OrbitPitchMin);
            Assert.Equal(90f, s.OrbitPitchMax);
            Assert.Equal(0.001f, s.MotionDeadband);
            Assert.False(s.AssistUiEnabled);
        }

        [Fact]
        public void CopyFrom_IncludesOrbitPitchLimits()
        {
            var source = new ModSettings { OrbitPitchMin = -45f, OrbitPitchMax = 60f };
            var dest = new ModSettings();
            dest.CopyFrom(source);
            Assert.Equal(-45f, dest.OrbitPitchMin);
            Assert.Equal(60f, dest.OrbitPitchMax);
        }
    }

    public class SensitivityNumericPolicyTests
    {
        [Fact]
        public void FormatFloat_RoundsToTwoDecimals()
        {
            Assert.Equal("1.25", ModOptions.FormatFloat(1.254f));
            Assert.Equal("0.50", ModOptions.FormatFloat(0.5f));
            Assert.Equal("10.00", ModOptions.FormatFloat(10f));
            // Two-decimal FormatFloat cannot represent factory pan 0.005; FormatGain can.
            float pan = FeelExpectation.Factory().PanGainX;
            Assert.Equal("0.005", ModOptions.FormatGain(pan));
            Assert.NotEqual(ModOptions.FormatGain(pan), ModOptions.FormatFloat(pan));
        }

        [Fact]
        public void RoundGain_RoundsToThreeDecimals()
        {
            Assert.Equal(0.004f, ModOptions.RoundGain(0.00375f));
            Assert.Equal(0.006f, ModOptions.RoundGain(0.00625f));
        }

        [Fact]
        public void FormatGain_RoundsToThreeDecimals()
        {
            Assert.Equal("0.005", ModOptions.FormatGain(0.005f));
            Assert.Equal("0.004", ModOptions.FormatGain(0.00375f));
        }

        [Fact]
        public void Round2_RoundsHalfAwayFromZero()
        {
            Assert.Equal(1.23f, ModOptions.Round2(1.234f));
            Assert.Equal(1.24f, ModOptions.Round2(1.235f));
        }

        [Fact]
        public void ApplyPanGainX_RoundsToThreeDecimals()
        {
            float seed = FeelExpectation.Factory().PanGainX;
            var settings = new ModSettings { PanGainX = seed };
            ModOptions.ApplyPanGainX(settings, 0.001234f);
            Assert.Equal(0.001f, settings.PanGainX);
        }

        [Fact]
        public void ApplyPanGainX_RejectsZeroAndNegative()
        {
            float seed = FeelExpectation.Factory().PanGainX;
            var settings = new ModSettings { PanGainX = seed };
            ModOptions.ApplyPanGainX(settings, 0f);
            Assert.Equal(seed, settings.PanGainX);
            ModOptions.ApplyPanGainX(settings, -1f);
            Assert.Equal(seed, settings.PanGainX);
        }

        [Fact]
        public void ApplyPanGainX_AllowsValuesAboveOldScaleMax()
        {
            var settings = new ModSettings { PanGainX = 0.50f };
            ModOptions.ApplyPanGainX(settings, 999f);
            Assert.Equal(999.00f, settings.PanGainX);
        }

        [Fact]
        public void ApplyZoomGain_RejectsNonPositive()
        {
            var settings = new ModSettings { ZoomGain = 1.00f };
            ModOptions.ApplyZoomGain(settings, 0f);
            Assert.Equal(1.00f, settings.ZoomGain);
        }
    }

    public class OrbitPitchClampTests
    {
        [Fact]
        public void ApplyOrbit_Button_ClampsPitchToVanillaMax()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 70f };
            var settings = new ModSettings { OrbitYawGain = 1f, OrbitPitchGain = 1f };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                0f,
                30f,
                0f,
                0f,
                settings,
                cam,
                CameraApplicator.InputModality.Button
            );

            Assert.Equal(90f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_Button_ClampsPitchToVanillaMin()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 20f };
            var settings = new ModSettings { OrbitYawGain = 1f, OrbitPitchGain = 1f };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                0f,
                -20f,
                0f,
                0f,
                settings,
                cam,
                CameraApplicator.InputModality.Button
            );

            Assert.Equal(0f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_Button_IgnoresSettingsPitchLimits()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 10f };
            var settings = new ModSettings
            {
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
                OrbitPitchMin = 40f,
                OrbitPitchMax = 50f,
            };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                0f,
                100f,
                0f,
                0f,
                settings,
                cam,
                CameraApplicator.InputModality.Button
            );

            Assert.Equal(90f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_Button_FloorsAtZeroEvenIfSettingsNegative()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 5f };
            var settings = new ModSettings
            {
                OrbitYawGain = 1f,
                OrbitPitchGain = 1f,
                OrbitPitchMin = -80f,
                OrbitPitchMax = 90f,
            };

            CameraApplicator.Apply(
                CameraOp.Orbit,
                0f,
                -100f,
                0f,
                0f,
                settings,
                cam,
                CameraApplicator.InputModality.Button
            );

            Assert.Equal(0f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_Drag_QueuesVelocity_AndStopsAtZeroFloor()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 0f };
            var settings = new ModSettings { OrbitYawGain = 1f, OrbitPitchGain = 1f };

            CameraApplicator.Apply(CameraOp.Orbit, 5f, -3f, 0f, 0f, settings, cam);
            Assert.Equal(0f, cam.AngleX, 3);
            Assert.Equal(0f, cam.AngleY, 3);

            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(5f, cam.AngleX, 3);
            // Pitch already at 0; downward delta is zeroed before queue (free-cam guard).
            Assert.Equal(0f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_Drag_QueuesVelocity_WithinRange()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 40f };
            var settings = new ModSettings { OrbitYawGain = 1f, OrbitPitchGain = 1f };

            CameraApplicator.Apply(CameraOp.Orbit, 5f, -3f, 0f, 0f, settings, cam);
            FakeCameraController.SimulateVanillaOrbitFrame(
                cam,
                inertia: 1f,
                deltaTimeSeconds: 1f / 60f
            );

            Assert.Equal(5f, cam.AngleX, 3);
            Assert.Equal(37f, cam.AngleY, 3);
        }
    }

    public class PanCityBoundsClampTests
    {
        [Fact]
        public void ApplyPan_ClampsTargetInsideFakeBounds()
        {
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
                MinX = -10f,
                MaxX = 10f,
                MinZ = -10f,
                MaxZ = 10f,
            };
            var settings = new ModSettings
            {
                PanGainX = 1f,
                PanGainY = 1f,
                SignInvertPanX = false,
                SignInvertPanY = false,
            };

            CameraApplicator.Apply(CameraOp.Pan, 100f, 0f, 0f, 0f, settings, cam);

            Assert.Equal(10f, cam.TargetX, 3);
            Assert.Equal(0f, cam.TargetZ, 3);
        }

        [Fact]
        public void ApplyPan_ClampsTargetToMinBounds()
        {
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
                MinX = -10f,
                MaxX = 10f,
                MinZ = -5f,
                MaxZ = 5f,
            };
            var settings = new ModSettings
            {
                PanGainX = 1f,
                PanGainY = 1f,
                SignInvertPanX = false,
                SignInvertPanY = false,
            };

            CameraApplicator.Apply(CameraOp.Pan, -100f, -100f, 0f, 0f, settings, cam);

            Assert.Equal(-10f, cam.TargetX, 3);
            Assert.Equal(-5f, cam.TargetZ, 3);
        }

        [Fact]
        public void ApplyPan_SkipsClampWhenBoundsUnavailable()
        {
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
            };
            var settings = new ModSettings
            {
                PanGainX = 1f,
                PanGainY = 1f,
                SignInvertPanX = false,
                SignInvertPanY = false,
            };

            CameraApplicator.Apply(CameraOp.Pan, 50f, 0f, 0f, 0f, settings, cam);

            Assert.Equal(50f, cam.TargetX, 3);
        }

        [Fact]
        public void ApplyPan_UsesCustomClampInsteadOfAabb()
        {
            // L-shape: allow x in [-10,10] only when z >= 0; otherwise pull z to 0.
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
                MinX = -100f,
                MaxX = 100f,
                MinZ = -100f,
                MaxZ = 100f,
                ClampPanCustom = (ref float x, ref float z) =>
                {
                    if (z < 0f)
                    {
                        z = 0f;
                    }

                    if (x < -10f)
                    {
                        x = -10f;
                    }
                    else if (x > 10f)
                    {
                        x = 10f;
                    }
                },
            };
            var settings = new ModSettings
            {
                PanGainX = 1f,
                PanGainY = 1f,
                SignInvertPanX = false,
                SignInvertPanY = false,
            };

            CameraApplicator.Apply(CameraOp.Pan, 50f, -50f, 0f, 0f, settings, cam);

            Assert.Equal(10f, cam.TargetX, 3);
            Assert.Equal(0f, cam.TargetZ, 3);
        }
    }
}
