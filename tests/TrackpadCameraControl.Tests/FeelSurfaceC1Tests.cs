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
            Assert.Equal(0.50f, s.PanSensitivityX);
            Assert.Equal(0.50f, s.PanSensitivityY);
            Assert.Equal(1.00f, s.ZoomSensitivity);
            Assert.Equal(2.00f, s.YawRotateSensitivity);
            Assert.Equal(10.00f, s.OrbitYawSensitivity);
            Assert.Equal(10.00f, s.OrbitPitchSensitivity);
            Assert.Equal(-80f, s.OrbitPitchMin);
            Assert.Equal(80f, s.OrbitPitchMax);
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
        public void ApplyPanSensitivityX_RoundsToTwoDecimals()
        {
            var settings = new ModSettings { PanSensitivityX = 0.50f };
            ModOptions.ApplyPanSensitivityX(settings, 1.234f);
            Assert.Equal(1.23f, settings.PanSensitivityX);
        }

        [Fact]
        public void ApplyPanSensitivityX_RejectsZeroAndNegative()
        {
            var settings = new ModSettings { PanSensitivityX = 0.50f };
            ModOptions.ApplyPanSensitivityX(settings, 0f);
            Assert.Equal(0.50f, settings.PanSensitivityX);
            ModOptions.ApplyPanSensitivityX(settings, -1f);
            Assert.Equal(0.50f, settings.PanSensitivityX);
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
                OrbitPitchMin = -80f,
                OrbitPitchMax = 80f,
            };

            CameraApplicator.Apply(CameraOp.Orbit, 0f, 20f, 0f, 0f, settings, cam);

            Assert.Equal(80f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_ClampsPitchToMin()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = -70f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
                OrbitPitchMin = -80f,
                OrbitPitchMax = 80f,
            };

            CameraApplicator.Apply(CameraOp.Orbit, 0f, -20f, 0f, 0f, settings, cam);

            Assert.Equal(-80f, cam.AngleY, 3);
        }

        [Fact]
        public void ApplyOrbit_SwapsMinMaxWhenInverted()
        {
            var cam = new FakeCameraController { AngleX = 0f, AngleY = 0f };
            var settings = new ModSettings
            {
                OrbitYawSensitivity = 1f,
                OrbitPitchSensitivity = 1f,
                OrbitPitchMin = 80f,
                OrbitPitchMax = -80f,
            };

            CameraApplicator.Apply(CameraOp.Orbit, 0f, 100f, 0f, 0f, settings, cam);

            Assert.Equal(80f, cam.AngleY, 3);
        }
    }
}
