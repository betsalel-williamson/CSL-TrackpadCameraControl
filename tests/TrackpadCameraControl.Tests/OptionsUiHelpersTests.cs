using System.IO;
using System.Text.Json;
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

            Assert.Equal(0.1f, ModOptions.SensitivitySliderMin(1f));
            Assert.Equal(2f, ModOptions.SensitivitySliderMax(1f));
            Assert.Equal(0.1f, ModOptions.SensitivitySliderStep(1f));

            Assert.Equal(1.00f, ModOptions.SensitivitySliderMin(10.00f));
            Assert.Equal(20.00f, ModOptions.SensitivitySliderMax(10.00f));
            Assert.Equal(1.00f, ModOptions.SensitivitySliderStep(10.00f));
        }

        [Fact]
        public void SensitivityUi_Conversion_FactoryIsMidAndRoundTrips()
        {
            Assert.Equal(0f, ModOptions.SensitivityUiMin);
            Assert.Equal(1f, ModOptions.SensitivityUiMax);
            Assert.Equal(0.5f, ModOptions.SensitivityUiFactory);
            Assert.Equal(0.05f, ModOptions.SensitivityUiStep);

            float factory = 2.00f;
            // Anchors: 0.1× / 1× / 2× (piecewise — not a single linear span).
            Assert.Equal(0.5f, ModOptions.GainToSensitivityUi(factory, factory), 3);
            Assert.Equal(0f, ModOptions.GainToSensitivityUi(0.2f, factory), 3);
            Assert.Equal(1f, ModOptions.GainToSensitivityUi(4f, factory), 3);

            Assert.Equal(2.00f, ModOptions.SensitivityUiToGain(0.5f, factory), 3);
            Assert.Equal(0.2f, ModOptions.SensitivityUiToGain(0f, factory), 3);
            Assert.Equal(4.00f, ModOptions.SensitivityUiToGain(1f, factory), 3);

            // One notch above mid ≈ +10% of factory on the high side.
            float up = ModOptions.SensitivityUiToGain(
                ModOptions.SensitivityUiFactory + ModOptions.SensitivityUiStep,
                factory
            );
            Assert.Equal(RoundGainLocal(factory * 1.1f), up, 3);

            // Round-trip UI notches.
            for (float ui = 0f; ui <= 1f + 0.001f; ui += ModOptions.SensitivityUiStep)
            {
                float gain = ModOptions.SensitivityUiToGain(ui, factory);
                Assert.Equal(ui, ModOptions.GainToSensitivityUi(gain, factory), 2);
            }

            // Pan-scale factory (0.005) still mid-tracks to the Debug field value.
            Assert.Equal(0.5f, ModOptions.GainToSensitivityUi(0.005f, 0.005f), 3);
            Assert.Equal(0.005f, ModOptions.SensitivityUiToGain(0.5f, 0.005f), 3);
            Assert.Equal(0.001f, ModOptions.SensitivityUiToGain(0f, 0.005f), 3);
            Assert.Equal(0.010f, ModOptions.SensitivityUiToGain(1f, 0.005f), 3);
        }

        private static float RoundGainLocal(float value)
        {
            return ModOptions.RoundGain(value);
        }

        [Fact]
        public void ClampGainToFactoryRange_ClampsAndRounds()
        {
            Assert.Equal(0.05f, ModOptions.ClampGainToFactoryRange(0f, 0.50f));
            Assert.Equal(0.05f, ModOptions.ClampGainToFactoryRange(0.01f, 0.50f));
            Assert.Equal(1.00f, ModOptions.ClampGainToFactoryRange(9f, 0.50f));
            Assert.Equal(0.551f, ModOptions.ClampGainToFactoryRange(0.551f, 0.50f));
        }

        [Fact]
        public void ApplyOrbitPitchMin_AllowsZero_IgnoresNegative()
        {
            var s = new ModSettings { OrbitPitchMin = 5f };
            ModOptions.ApplyOrbitPitchMin(s, 0f);
            Assert.Equal(0f, s.OrbitPitchMin);
            ModOptions.ApplyOrbitPitchMin(s, -3f);
            Assert.Equal(0f, s.OrbitPitchMin);
            ModOptions.ApplyOrbitPitchMin(s, 12.345f);
            Assert.Equal(12.345f, s.OrbitPitchMin);
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
            Assert.False(string.IsNullOrEmpty(title));

            string packageVersion = ReadPackageJsonVersion();
            Assert.False(string.IsNullOrEmpty(packageVersion));

            // Behavior: Content Manager / Options title surfaces the package version
            // (MSBuild syncs AssemblyVersion from package.json).
            string[] parts = packageVersion.Split('.');
            Assert.True(parts.Length >= 2, "package.json version should be at least major.minor");
            Assert.Contains(parts[0] + "." + parts[1], title);

            if (parts.Length >= 3)
            {
                Assert.Contains(parts[0] + "." + parts[1] + "." + parts[2], title);
            }
        }

        private static string ReadPackageJsonVersion()
        {
            string root = NativeResourceLeakAnalyzer.FindRepoRoot();
            string path = Path.Combine(root, "package.json");
            using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path)))
            {
                return doc.RootElement.GetProperty("version").GetString();
            }
        }
    }
}
