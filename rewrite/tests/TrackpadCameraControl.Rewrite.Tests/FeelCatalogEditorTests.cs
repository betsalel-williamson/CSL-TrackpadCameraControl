using System.IO;
using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>
    /// Unit tests for feel catalog, editor, and store — no Unity/Cities refs (mod pure layer).
    /// </summary>
    public class FeelCatalogEditorTests
    {
        [Fact]
        public void Catalog_SectionOrder_MatchesContract()
        {
            string[] sections = FeelCatalog.SectionOrder();
            Assert.Equal(new[] { "General", "Zoom", "Pan", "Rotate", "Orbit" }, sections);
        }

        [Fact]
        public void Catalog_FirstFields_MatchFeelCatalogShard()
        {
            var fields = FeelCatalog.AllFields();
            Assert.Equal("feelPreset", fields[0].Id);
            Assert.Equal("Feel preset", fields[0].Label);
            Assert.Equal(FeelControlKind.Dropdown, fields[0].Kind);
            Assert.Equal("showDebugPanel", fields[5].Id);
            Assert.Equal("zoomSensitivity", fields[6].Id);
            Assert.Equal("Zoom", fields[6].Section);
        }

        [Fact]
        public void OptionsAndDebug_ShareSameDescriptorInventory()
        {
            var options = OptionsHost.BuildDescriptors();
            var debug = DebugHost.BuildDescriptors();
            Assert.Equal(options.Count, debug.Count);
            for (int i = 0; i < options.Count; i++)
            {
                Assert.Equal(options[i].Id, debug[i].Id);
                Assert.Equal(options[i].Section, debug[i].Section);
                Assert.Equal(options[i].Label, debug[i].Label);
            }

            Assert.Equal("General", options[0].Section);
            Assert.Equal("Orbit", options[options.Count - 1].Section);
        }

        [Fact]
        public void Editor_EditWhileDefault_DirtiesToNewPreset()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
                var editor = new FeelEditor(settings, store);

                editor.ApplyGain((s, v) => s.ZoomGain = v, 1.5f);

                Assert.True(editor.IsDirty);
                Assert.Equal(FeelProfiles.NameNewPreset, settings.ActiveFeelPresetName);
                Assert.Equal(1.5f, settings.ZoomGain, 3);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void Editor_SaveAs_ThenLoad_RestoresNamedFeel()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                var editor = new FeelEditor(settings, store);
                editor.ApplyGain((s, v) => s.ZoomGain = v, 1.75f);
                Assert.True(editor.SaveAs("MyFeel"));
                Assert.Equal("MyFeel", settings.ActiveFeelPresetName);
                Assert.False(editor.IsDirty);

                editor.LoadPreset(FeelProfiles.NameDefault);
                Assert.Equal(FeelProfiles.NameDefault, settings.ActiveFeelPresetName);
                Assert.NotEqual(1.75f, settings.ZoomGain);

                Assert.True(editor.LoadPreset("MyFeel"));
                Assert.Equal(1.75f, settings.ZoomGain, 3);
                Assert.Equal("MyFeel", settings.ActiveFeelPresetName);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void Editor_DeleteNamed_AppliesDefault()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                var editor = new FeelEditor(settings, store);
                editor.ApplyGain((s, v) => s.ZoomGain = v, 1.6f);
                Assert.True(editor.SaveAs("TempFeel"));
                Assert.True(editor.DeleteNamedPreset("TempFeel"));
                Assert.Equal(FeelProfiles.NameDefault, settings.ActiveFeelPresetName);
                Assert.False(editor.DeleteNamedPreset(FeelProfiles.NameDefault));
                Assert.False(editor.DeleteNamedPreset(FeelProfiles.NameNewPreset));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void Editor_ResetToFactory_ClearsDirty()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                var editor = new FeelEditor(settings, store);
                editor.ApplyGain((s, v) => s.ZoomGain = v, 1.9f);
                editor.ResetToFactory();
                Assert.Equal(FeelProfiles.NameDefault, settings.ActiveFeelPresetName);
                Assert.False(editor.IsDirty);
                Assert.Equal(ModSettings.CreateFactoryDefaults().ZoomGain, settings.ZoomGain, 3);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void Store_OneDirty_OneFlush_ClearsPending()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                // Coalesce window may skip flush right after LoadOrFactory SaveNow —
                // force one write path and assert dirty clears.
                store.MarkDirtyAndMaybeFlush(settings);
                if (store.HasPendingDirty)
                {
                    store.SaveNow(settings);
                }

                Assert.False(store.HasPendingDirty);
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
