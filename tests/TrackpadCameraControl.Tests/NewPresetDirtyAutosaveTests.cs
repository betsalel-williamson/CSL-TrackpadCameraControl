using System;
using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class ActiveFeelPresetNameDefaultsTests
    {
        [Fact]
        public void FactoryDefaults_ActiveFeelPresetName_IsDefault()
        {
            ModSettings settings = ModSettings.CreateFactoryDefaults();
            Assert.Equal(FeelProfiles.NameDefault, settings.ActiveFeelPresetName);
        }

        [Fact]
        public void FeelProfileNameConstants_MatchProductLabels()
        {
            Assert.Equal("Slow", FeelProfiles.NameSlow);
            Assert.Equal("Default", FeelProfiles.NameDefault);
            Assert.Equal("Fast", FeelProfiles.NameFast);
            Assert.Equal("New Preset", FeelProfiles.NameNewPreset);
        }

        [Fact]
        public void IsBuiltInName_RecognizesSlowDefaultFast_Only()
        {
            Assert.True(FeelProfiles.IsBuiltInName(FeelProfiles.NameSlow));
            Assert.True(FeelProfiles.IsBuiltInName(FeelProfiles.NameDefault));
            Assert.True(FeelProfiles.IsBuiltInName(FeelProfiles.NameFast));
            Assert.False(FeelProfiles.IsBuiltInName(FeelProfiles.NameNewPreset));
            Assert.False(FeelProfiles.IsBuiltInName("MyFeel"));
            Assert.False(FeelProfiles.IsBuiltInName(null));
            Assert.False(FeelProfiles.IsBuiltInName(""));
        }
    }

    [Collection(ModOptionsStoreCollection.Name)]
    public class NewPresetDirtyAutosaveTests : IDisposable
    {
        private readonly string _dir;
        private readonly string _path;

        public NewPresetDirtyAutosaveTests()
        {
            Mod.ClearSettingsForTests();
            _dir = Path.Combine(
                Path.GetTempPath(),
                "tcc-new-preset-" + Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(_dir);
            _path = Path.Combine(_dir, "settings.xml");
        }

        public void Dispose()
        {
            Mod.ClearSettingsForTests();
            ModOptions.ClearSettingsChangedForTests();
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

        private ModSettingsStore OpenStoreWithLive(out ModSettings live)
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            live = ModSettings.CreateFactoryDefaults();
            Mod.SetSettingsForTests(live);
            store.SaveNow(live);
            return store;
        }

        [Fact]
        public void ApplyFeelSlow_SetsActiveName_DoesNotWriteNewPresetSlot()
        {
            OpenStoreWithLive(out ModSettings live);
            live.PanSensitivityX = 9f;

            ModOptions.ApplyFeelSlow(live);

            Assert.Equal(FeelProfiles.NameSlow, live.ActiveFeelPresetName);
            Assert.Equal(0.0037f, live.PanSensitivityX);
            ModSettings scratch;
            Assert.False(ModOptions.Store.TryGetUserPreset(FeelProfiles.NameNewPreset, out scratch));
        }

        [Fact]
        public void ApplyFeelDefault_AndFast_SetActiveNames()
        {
            OpenStoreWithLive(out ModSettings live);

            ModOptions.ApplyFeelDefault(live);
            Assert.Equal(FeelProfiles.NameDefault, live.ActiveFeelPresetName);

            ModOptions.ApplyFeelFast(live);
            Assert.Equal(FeelProfiles.NameFast, live.ActiveFeelPresetName);
            Assert.Equal(ModOptions.RoundSensitivity(0.005f * 1.25f), live.PanSensitivityX);
        }

        [Fact]
        public void ApplyPanSensitivityX_WhileOnDefault_SwitchesToNewPresetAndAutosaves()
        {
            OpenStoreWithLive(out ModSettings live);
            Assert.Equal(FeelProfiles.NameDefault, live.ActiveFeelPresetName);

            ModOptions.ApplyPanSensitivityX(live, 0.75f);

            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);
            Assert.Equal(0.75f, live.PanSensitivityX);

            ModSettings scratch;
            Assert.True(
                ModOptions.Store.TryGetUserPreset(FeelProfiles.NameNewPreset, out scratch)
            );
            Assert.Equal(0.75f, scratch.PanSensitivityX);
        }

        [Fact]
        public void FurtherEditsOnNewPreset_AutosaveIntoSameSlot()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyPanSensitivityX(live, 0.75f);
            ModOptions.ApplyZoomSensitivity(live, 1.50f);

            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);
            ModSettings scratch;
            Assert.True(
                ModOptions.Store.TryGetUserPreset(FeelProfiles.NameNewPreset, out scratch)
            );
            Assert.Equal(0.75f, scratch.PanSensitivityX);
            Assert.Equal(1.50f, scratch.ZoomSensitivity);
        }

        [Fact]
        public void DirtyFromBuiltIn_DoesNotOverwriteBuiltInApplyContract()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyFeelSlow(live);
            ModOptions.ApplyPanSensitivityX(live, 0.99f);

            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);

            var other = ModSettings.CreateFactoryDefaults();
            FeelProfiles.ApplySlow(other);
            Assert.Equal(0.0037f, other.PanSensitivityX);
            Assert.NotEqual(0.99f, other.PanSensitivityX);
        }

        [Fact]
        public void SaveNamedFeelPreset_RejectsBuiltInNames()
        {
            OpenStoreWithLive(out ModSettings live);
            live.PanSensitivityX = 0.80f;

            Assert.False(ModOptions.SaveNamedFeelPreset(live, FeelProfiles.NameSlow));
            Assert.False(ModOptions.SaveNamedFeelPreset(live, FeelProfiles.NameDefault));
            Assert.False(ModOptions.SaveNamedFeelPreset(live, FeelProfiles.NameFast));
            Assert.False(ModOptions.SaveNamedFeelPreset(live, ""));
            Assert.False(ModOptions.SaveNamedFeelPreset(live, null));

            string[] names = ModOptions.ListNamedFeelPresetNames();
            Assert.DoesNotContain(FeelProfiles.NameSlow, names);
            Assert.DoesNotContain(FeelProfiles.NameDefault, names);
            Assert.DoesNotContain(FeelProfiles.NameFast, names);
        }

        [Fact]
        public void SaveNamedFeelPreset_PromotesNewPreset_SelectsNamed()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyPanSensitivityX(live, 0.80f);
            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);

            Assert.True(ModOptions.SaveNamedFeelPreset(live, "MyFeel"));

            Assert.Equal("MyFeel", live.ActiveFeelPresetName);
            ModSettings named;
            Assert.True(ModOptions.Store.TryGetUserPreset("MyFeel", out named));
            Assert.Equal(0.80f, named.PanSensitivityX);

            ModSettings scratch;
            Assert.False(
                ModOptions.Store.TryGetUserPreset(FeelProfiles.NameNewPreset, out scratch)
            );
        }

        [Fact]
        public void AfterSaveAs_FurtherEdit_DirtiesToNewPresetAgain()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyPanSensitivityX(live, 0.80f);
            Assert.True(ModOptions.SaveNamedFeelPreset(live, "MyFeel"));

            ModOptions.ApplyPanSensitivityX(live, 0.90f);

            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);
            ModSettings named;
            Assert.True(ModOptions.Store.TryGetUserPreset("MyFeel", out named));
            Assert.Equal(0.80f, named.PanSensitivityX);

            ModSettings scratch;
            Assert.True(
                ModOptions.Store.TryGetUserPreset(FeelProfiles.NameNewPreset, out scratch)
            );
            Assert.Equal(0.90f, scratch.PanSensitivityX);
        }

        [Fact]
        public void LoadNamedFeelPreset_SetsActiveName()
        {
            OpenStoreWithLive(out ModSettings live);
            live.PanSensitivityX = 1.11f;
            Assert.True(ModOptions.SaveNamedFeelPreset(live, "KeepMe"));

            ModOptions.ApplyFeelDefault(live);
            Assert.Equal(FeelProfiles.NameDefault, live.ActiveFeelPresetName);
            Assert.Equal(0.005f, live.PanSensitivityX);

            Assert.True(ModOptions.LoadNamedFeelPreset(live, "KeepMe"));
            Assert.Equal("KeepMe", live.ActiveFeelPresetName);
            Assert.Equal(1.11f, live.PanSensitivityX);
        }

        [Fact]
        public void ApplyOrbitPitchMin_WhileOnNamed_DirtiesToNewPreset()
        {
            OpenStoreWithLive(out ModSettings live);
            live.PanSensitivityX = 1.00f;
            Assert.True(ModOptions.SaveNamedFeelPreset(live, "Pitchy"));
            ModOptions.LoadNamedFeelPreset(live, "Pitchy");

            ModOptions.ApplyOrbitPitchMin(live, 12f);

            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);
            Assert.Equal(12f, live.OrbitPitchMin);
        }

        [Fact]
        public void NotifyChanged_ForceFlushesAndRaisesSettingsChanged()
        {
            OpenStoreWithLive(out ModSettings live);
            int raised = 0;
            ModOptions.SettingsChanged += () => raised++;

            live.PanSensitivityY = 0.42f;
            ModOptions.NotifyChanged(live);

            Assert.Equal(1, raised);

            var reloaded = new ModSettingsStore(_path).LoadOrFactory();
            Assert.Equal(0.42f, reloaded.PanSensitivityY);
        }

        [Fact]
        public void ResetToFactory_SetsActiveDefault()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyPanSensitivityX(live, 0.80f);
            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);

            ModOptions.ResetToFactory(live);

            Assert.Equal(FeelProfiles.NameDefault, live.ActiveFeelPresetName);
            Assert.Equal(0.005f, live.PanSensitivityX);
        }

        [Fact]
        public void GetFeelPresetDropdownItems_IncludesBuiltInsAndSaveAs()
        {
            OpenStoreWithLive(out ModSettings live);
            string[] items = ModOptions.GetFeelPresetDropdownItems(live);

            Assert.Equal(FeelProfiles.NameSlow, items[0]);
            Assert.Equal(FeelProfiles.NameDefault, items[1]);
            Assert.Equal(FeelProfiles.NameFast, items[2]);
            Assert.Equal(ModOptions.FeelPresetSaveAsLabel, items[items.Length - 1]);
            Assert.DoesNotContain(FeelProfiles.NameNewPreset, items);
            Assert.Equal(
                1,
                ModOptions.IndexOfFeelPresetDropdownItem(items, FeelProfiles.NameDefault)
            );
        }

        [Fact]
        public void GetFeelPresetDropdownItems_IncludesNewPresetWhenActive()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyPanSensitivityX(live, 0.80f);
            Assert.Equal(FeelProfiles.NameNewPreset, live.ActiveFeelPresetName);

            string[] items = ModOptions.GetFeelPresetDropdownItems(live);

            Assert.Contains(FeelProfiles.NameNewPreset, items);
            Assert.Equal(ModOptions.FeelPresetSaveAsLabel, items[items.Length - 1]);
            Assert.Equal(
                Array.IndexOf(items, FeelProfiles.NameNewPreset),
                ModOptions.IndexOfFeelPresetDropdownItem(items, FeelProfiles.NameNewPreset)
            );
        }

        [Fact]
        public void ApplyFeelPresetDropdownChoice_LoadsBuiltIn()
        {
            OpenStoreWithLive(out ModSettings live);
            ModOptions.ApplyFeelPresetDropdownChoice(live, FeelProfiles.NameFast);
            Assert.Equal(FeelProfiles.NameFast, live.ActiveFeelPresetName);
            Assert.Equal(ModOptions.RoundSensitivity(0.005f * 1.25f), live.PanSensitivityX);
        }
    }
}
