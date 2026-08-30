using System;
using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class FeelProfilesApplyTests
    {
        [Fact]
        public void ApplySlow_UsesFactoryTimesSlowMultiplier_Rounded()
        {
            var settings = new ModSettings
            {
                PanGainX = 9f,
                PanGainY = 9f,
                ZoomGain = 9f,
                YawRotateGain = 9f,
                OrbitYawGain = 9f,
                OrbitPitchGain = 9f,
                SignInvertPanX = false,
                SignInvertPanY = true,
                OrbitPitchMin = -10f,
                OrbitPitchMax = 10f,
            };

            FeelProfiles.ApplySlow(settings);

            FeelExpectation.AssertMatchesScaledFactory(settings, FeelProfiles.SlowMultiplier);
        }

        [Fact]
        public void ApplyFast_UsesFactoryTimesFastMultiplier_Rounded()
        {
            var settings = new ModSettings
            {
                PanGainX = 0.01f,
                SignInvertPanX = false,
                OrbitPitchMin = 0f,
                OrbitPitchMax = 1f,
            };

            FeelProfiles.ApplyFast(settings);

            FeelExpectation.AssertMatchesScaledFactory(settings, FeelProfiles.FastMultiplier);
        }

        [Fact]
        public void ApplyDefault_RestoresFactoryFeelFromDirtySettings()
        {
            var settings = new ModSettings
            {
                PanGainX = 9f,
                SignInvertPanX = false,
                SignInvertPanY = true,
                OrbitPitchMin = -1f,
                OrbitPitchMax = 1f,
                PanEnabled = false,
            };

            FeelProfiles.ApplyDefault(settings);

            FeelExpectation.AssertMatchesFactoryFeel(settings);
            Assert.True(settings.PanEnabled);
        }
    }

    [Collection(ModOptionsStoreCollection.Name)]
    public class FeelProfilesNamedPresetTests : IDisposable
    {
        private readonly string _dir;
        private readonly string _path;

        public FeelProfilesNamedPresetTests()
        {
            ModOptions.Store = null;
            _dir = Path.Combine(
                Path.GetTempPath(),
                "tcc-feel-presets-" + Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(_dir);
            _path = Path.Combine(_dir, "settings.xml");
        }

        public void Dispose()
        {
            ModOptions.Store = null;
            try
            {
                if (Directory.Exists(_dir))
                {
                    Directory.Delete(_dir, true);
                }
            }
            catch
            {
                // ignore
            }
        }

        [Fact]
        public void SaveAndLoadNamedFeelPreset_RoundTripsFeelFields()
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            var live = ModSettings.CreateFactoryDefaults();
            store.SaveNow(live);

            live.PanGainX = 1.25f;
            live.PanGainY = 0.75f;
            live.ZoomGain = 2.00f;
            live.YawRotateGain = 3.00f;
            live.OrbitYawGain = 4.00f;
            live.OrbitPitchGain = 5.00f;
            live.SignInvertPanX = false;
            live.SignInvertPanY = true;
            live.OrbitPitchMin = -40f;
            live.OrbitPitchMax = 40f;
            live.PanEnabled = false;

            Assert.True(ModOptions.SaveNamedFeelPreset(live, "MyFeel"));

            var restored = ModSettings.CreateFactoryDefaults();
            Assert.True(ModOptions.LoadNamedFeelPreset(restored, "MyFeel"));

            Assert.Equal(1.25f, restored.PanGainX);
            Assert.Equal(0.75f, restored.PanGainY);
            Assert.Equal(2.00f, restored.ZoomGain);
            Assert.Equal(3.00f, restored.YawRotateGain);
            Assert.Equal(4.00f, restored.OrbitYawGain);
            Assert.Equal(5.00f, restored.OrbitPitchGain);
            Assert.False(restored.SignInvertPanX);
            Assert.True(restored.SignInvertPanY);
            Assert.Equal(-40f, restored.OrbitPitchMin);
            Assert.Equal(40f, restored.OrbitPitchMax);
            Assert.False(restored.PanEnabled);
        }

        [Fact]
        public void NamedFeelPresets_SurviveCurrentSettingsSave()
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            var live = ModSettings.CreateFactoryDefaults();
            store.SaveNow(live);

            live.PanGainX = 1.11f;
            Assert.True(ModOptions.SaveNamedFeelPreset(live, "KeepMe"));

            live.PanGainX = 2.22f;
            store.SaveNow(live);

            var other = ModSettings.CreateFactoryDefaults();
            Assert.True(ModOptions.LoadNamedFeelPreset(other, "KeepMe"));
            Assert.Equal(1.11f, other.PanGainX);
        }

        [Fact]
        public void ListNamedFeelPresetNames_ReturnsSavedNames()
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            var live = ModSettings.CreateFactoryDefaults();
            store.SaveNow(live);

            ModOptions.SaveNamedFeelPreset(live, "Alpha");
            ModOptions.SaveNamedFeelPreset(live, "Beta");

            string[] names = ModOptions.ListNamedFeelPresetNames();
            Assert.Contains("Alpha", names);
            Assert.Contains("Beta", names);
        }

        [Fact]
        public void ApplyFeelSlow_ViaModOptions_MatchesSlowContract()
        {
            var settings = new ModSettings { PanGainX = 9f, SignInvertPanX = false };
            ModOptions.ApplyFeelSlow(settings);
            FeelExpectation.AssertMatchesScaledFactory(settings, FeelProfiles.SlowMultiplier);
        }

        [Fact]
        public void ResetToFactory_AlignsWithApplyDefaultFeel()
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            var settings = new ModSettings
            {
                PanGainX = 9f,
                SignInvertPanX = false,
                OrbitPitchMin = -5f,
            };
            store.SaveNow(settings);

            ModOptions.ResetToFactory(settings);

            FeelExpectation.AssertMatchesFactoryFeel(settings);
        }
    }
}
