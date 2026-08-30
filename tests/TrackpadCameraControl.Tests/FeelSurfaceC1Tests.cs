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
            ModSettings s = ModSettings.CreateFactoryDefaults();

            Assert.True(s.InvertPanX);
            Assert.False(s.InvertPanY);
            Assert.Equal(0.005f, s.PanSensitivityX);
            Assert.Equal(0.005f, s.PanSensitivityY);
            Assert.Equal(1.00f, s.ZoomSensitivity);
            Assert.Equal(2.00f, s.YawRotateSensitivity);
            Assert.Equal(0.10f, s.OrbitYawSensitivity);
            Assert.Equal(0.10f, s.OrbitPitchSensitivity);
            Assert.Equal(7f, s.OrbitPitchMin);
            Assert.Equal(90f, s.OrbitPitchMax);
            Assert.Equal(0.1f, s.MotionDeadzone);
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
        }

        [Fact]
        public void Round2_RoundsHalfAwayFromZero()
        {
            Assert.Equal(1.23f, ModOptions.Round2(1.234f));
            Assert.Equal(1.24f, ModOptions.Round2(1.235f));
        }

        [Fact]
        public void ApplyPanSensitivityX_RoundsToFourDecimals()
        {
            var settings = new ModSettings { PanSensitivityX = 0.005f };
            ModOptions.ApplyPanSensitivityX(settings, 0.001234f);
            Assert.Equal(0.0012f, settings.PanSensitivityX);
        }

        [Fact]
        public void ApplyPanSensitivityX_RejectsZeroAndNegative()
        {
            var settings = new ModSettings { PanSensitivityX = 0.005f };
            ModOptions.ApplyPanSensitivityX(settings, 0f);
            Assert.Equal(0.005f, settings.PanSensitivityX);
            ModOptions.ApplyPanSensitivityX(settings, -1f);
            Assert.Equal(0.005f, settings.PanSensitivityX);
        }

        [Fact]
        public void ApplyPanSensitivityX_AllowsValuesAboveOldScaleMax()
        {
            var settings = new ModSettings { PanSensitivityX = 0.50f };
            ModOptions.ApplyPanSensitivityX(settings, 999f);
            Assert.Equal(999.00f, settings.PanSensitivityX);
        }

        [Fact]
        public void ApplyZoomSensitivity_RejectsNonPositive()
        {
            var settings = new ModSettings { ZoomSensitivity = 1.00f };
            ModOptions.ApplyZoomSensitivity(settings, 0f);
            Assert.Equal(1.00f, settings.ZoomSensitivity);
        }
    }

    public class OrbitPitchClampTests
    {
        [Fact]
        public void ApplyOrbit_ClampsPitchToSettingsRange()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 70f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
                OrbitPitchMin = 7f,
                OrbitPitchMax = 90f,
            };

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
        public void ApplyOrbit_ClampsPitchToMin()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 20f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
                OrbitPitchMin = 7f,
                OrbitPitchMax = 90f,
            };

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

            Assert.Equal(7f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_SwapsMinMaxWhenInverted()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 10f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
                OrbitPitchMin = 90f,
                OrbitPitchMax = 7f,
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
        public void ApplyOrbit_EnforcesPitchAboveZeroEvenIfSettingsNegative()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 5f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
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

            Assert.True(cam.AngleY > 0f);
        }

        [Fact]
        public void ApplyOrbit_Drag_UsesAngleVelocity_NotHardClampPastMax()
        {
            // Drag feeds middle mouse button-style velocity (fake integrates immediately).
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 40f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
                OrbitPitchMin = 7f,
                OrbitPitchMax = 90f,
            };

            CameraApplicator.Apply(CameraOp.Orbit, 5f, -3f, 0f, 0f, settings, cam);

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
                PanSensitivityX = 1f,
                PanSensitivityY = 1f,
                InvertPanX = false,
                InvertPanY = false,
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
                PanSensitivityX = 1f,
                PanSensitivityY = 1f,
                InvertPanX = false,
                InvertPanY = false,
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
                PanSensitivityX = 1f,
                PanSensitivityY = 1f,
                InvertPanX = false,
                InvertPanY = false,
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
                PanSensitivityX = 1f,
                PanSensitivityY = 1f,
                InvertPanX = false,
                InvertPanY = false,
            };

            CameraApplicator.Apply(CameraOp.Pan, 50f, -50f, 0f, 0f, settings, cam);

            Assert.Equal(10f, cam.TargetX, 3);
            Assert.Equal(0f, cam.TargetZ, 3);
        }
    }
}
