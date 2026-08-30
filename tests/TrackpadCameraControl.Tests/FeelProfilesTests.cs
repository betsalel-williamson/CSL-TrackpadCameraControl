using System;
using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class FeelProfilesApplyTests
    {
        [Fact]
        public void ApplySlow_UsesFactorySensitivitiesTimes075_Rounded()
        {
            var settings = new ModSettings
            {
                PanSensitivityX = 9f,
                PanSensitivityY = 9f,
                ZoomSensitivity = 9f,
                YawRotateSensitivity = 9f,
                OrbitYawSensitivity = 9f,
                OrbitPitchSensitivity = 9f,
                InvertPanX = false,
                InvertPanY = true,
                OrbitPitchMin = -10f,
                OrbitPitchMax = 10f,
            };

            FeelProfiles.ApplySlow(settings);

            Assert.Equal(0.38f, settings.PanSensitivityX);
            Assert.Equal(0.38f, settings.PanSensitivityY);
            Assert.Equal(0.75f, settings.ZoomSensitivity);
            Assert.Equal(1.50f, settings.YawRotateSensitivity);
            Assert.Equal(7.50f, settings.OrbitYawSensitivity);
            Assert.Equal(7.50f, settings.OrbitPitchSensitivity);
            Assert.True(settings.InvertPanX);
            Assert.False(settings.InvertPanY);
            Assert.Equal(-80f, settings.OrbitPitchMin);
            Assert.Equal(80f, settings.OrbitPitchMax);
        }

        [Fact]
        public void ApplyFast_UsesFactorySensitivitiesTimes125_Rounded()
        {
            var settings = new ModSettings
            {
                PanSensitivityX = 0.01f,
                InvertPanX = false,
                OrbitPitchMin = 0f,
                OrbitPitchMax = 1f,
            };

            FeelProfiles.ApplyFast(settings);

            Assert.Equal(0.63f, settings.PanSensitivityX);
            Assert.Equal(0.63f, settings.PanSensitivityY);
            Assert.Equal(1.25f, settings.ZoomSensitivity);
            Assert.Equal(2.50f, settings.YawRotateSensitivity);
            Assert.Equal(12.50f, settings.OrbitYawSensitivity);
            Assert.Equal(12.50f, settings.OrbitPitchSensitivity);
            Assert.True(settings.InvertPanX);
            Assert.False(settings.InvertPanY);
            Assert.Equal(-80f, settings.OrbitPitchMin);
            Assert.Equal(80f, settings.OrbitPitchMax);
        }

        [Fact]
        public void ApplyDefault_RestoresFactoryFeelFromDirtySettings()
        {
            var settings = new ModSettings
            {
                PanSensitivityX = 9f,
                InvertPanX = false,
                InvertPanY = true,
                OrbitPitchMin = -1f,
                OrbitPitchMax = 1f,
                PanEnabled = false,
            };

            FeelProfiles.ApplyDefault(settings);

            Assert.Equal(0.50f, settings.PanSensitivityX);
            Assert.Equal(0.50f, settings.PanSensitivityY);
            Assert.Equal(1.00f, settings.ZoomSensitivity);
            Assert.Equal(2.00f, settings.YawRotateSensitivity);
            Assert.Equal(10.00f, settings.OrbitYawSensitivity);
            Assert.Equal(10.00f, settings.OrbitPitchSensitivity);
            Assert.True(settings.InvertPanX);
            Assert.False(settings.InvertPanY);
            Assert.Equal(-80f, settings.OrbitPitchMin);
            Assert.Equal(80f, settings.OrbitPitchMax);
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

            live.PanSensitivityX = 1.25f;
            live.PanSensitivityY = 0.75f;
            live.ZoomSensitivity = 2.00f;
            live.YawRotateSensitivity = 3.00f;
            live.OrbitYawSensitivity = 4.00f;
            live.OrbitPitchSensitivity = 5.00f;
            live.InvertPanX = false;
            live.InvertPanY = true;
            live.OrbitPitchMin = -40f;
            live.OrbitPitchMax = 40f;
            live.PanEnabled = false;

            Assert.True(ModOptions.SaveNamedFeelPreset(live, "MyFeel"));

            var restored = ModSettings.CreateFactoryDefaults();
            Assert.True(ModOptions.LoadNamedFeelPreset(restored, "MyFeel"));

            Assert.Equal(1.25f, restored.PanSensitivityX);
            Assert.Equal(0.75f, restored.PanSensitivityY);
            Assert.Equal(2.00f, restored.ZoomSensitivity);
            Assert.Equal(3.00f, restored.YawRotateSensitivity);
            Assert.Equal(4.00f, restored.OrbitYawSensitivity);
            Assert.Equal(5.00f, restored.OrbitPitchSensitivity);
            Assert.False(restored.InvertPanX);
            Assert.True(restored.InvertPanY);
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

            live.PanSensitivityX = 1.11f;
            Assert.True(ModOptions.SaveNamedFeelPreset(live, "KeepMe"));

            live.PanSensitivityX = 2.22f;
            store.SaveNow(live);

            var other = ModSettings.CreateFactoryDefaults();
            Assert.True(ModOptions.LoadNamedFeelPreset(other, "KeepMe"));
            Assert.Equal(1.11f, other.PanSensitivityX);
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
            var settings = new ModSettings { PanSensitivityX = 9f, InvertPanX = false };
            ModOptions.ApplyFeelSlow(settings);
            Assert.Equal(0.38f, settings.PanSensitivityX);
            Assert.True(settings.InvertPanX);
        }

        [Fact]
        public void ResetToFactory_AlignsWithApplyDefaultFeel()
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            var settings = new ModSettings
            {
                PanSensitivityX = 9f,
                InvertPanX = false,
                OrbitPitchMin = -5f,
            };
            store.SaveNow(settings);

            ModOptions.ResetToFactory(settings);

            Assert.Equal(0.50f, settings.PanSensitivityX);
            Assert.True(settings.InvertPanX);
            Assert.Equal(-80f, settings.OrbitPitchMin);
            Assert.Equal(80f, settings.OrbitPitchMax);
        }
    }
}
