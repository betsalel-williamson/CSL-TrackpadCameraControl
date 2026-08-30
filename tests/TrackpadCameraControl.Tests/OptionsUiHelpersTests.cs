using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(ModOptionsStoreCollection.Name)]
    public class OptionsUiHelpersTests
    {
        [Fact]
        public void SensitivitySliderRange_UsesFactoryFractions()
        {
            Assert.Equal(0.05f, ModOptions.SensitivitySliderMin(0.50f));
            Assert.Equal(1.00f, ModOptions.SensitivitySliderMax(0.50f));
            Assert.Equal(0.05f, ModOptions.SensitivitySliderStep(0.50f));

            Assert.Equal(1.00f, ModOptions.SensitivitySliderMin(10.00f));
            Assert.Equal(20.00f, ModOptions.SensitivitySliderMax(10.00f));
            Assert.Equal(1.00f, ModOptions.SensitivitySliderStep(10.00f));
        }

        [Fact]
        public void ClampSensitivityToFactoryRange_ClampsAndRounds()
        {
            Assert.Equal(0.05f, ModOptions.ClampSensitivityToFactoryRange(0.01f, 0.50f));
            Assert.Equal(1.00f, ModOptions.ClampSensitivityToFactoryRange(9f, 0.50f));
            Assert.Equal(0.551f, ModOptions.ClampSensitivityToFactoryRange(0.551f, 0.50f));
        }

        [Fact]
        public void ApplyOrbitPitchMin_IgnoresNonPositive()
        {
            var s = new ModSettings { OrbitPitchMin = 7f };
            ModOptions.ApplyOrbitPitchMin(s, 0f);
            Assert.Equal(7f, s.OrbitPitchMin);
            ModOptions.ApplyOrbitPitchMin(s, -3f);
            Assert.Equal(7f, s.OrbitPitchMin);
            ModOptions.ApplyOrbitPitchMin(s, 12.345f);
            Assert.Equal(12.35f, s.OrbitPitchMin);
        }

        [Fact]
        public void ApplyOrbitPitchMax_IgnoresNonPositive()
        {
            var s = new ModSettings { OrbitPitchMax = 90f };
            ModOptions.ApplyOrbitPitchMax(s, 0f);
            Assert.Equal(90f, s.OrbitPitchMax);
            ModOptions.ApplyOrbitPitchMax(s, -1f);
            Assert.Equal(90f, s.OrbitPitchMax);
        }

        [Fact]
        public void OptionsTitle_IncludesAssemblyVersion()
        {
            string title = Mod.OptionsTitle;
            Assert.StartsWith("Trackpad Camera Control", title);
            Assert.Contains(".", title);
        }
    }
}
