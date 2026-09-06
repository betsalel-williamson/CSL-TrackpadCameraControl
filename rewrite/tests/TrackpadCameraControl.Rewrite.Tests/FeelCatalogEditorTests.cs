using System;
using System.IO;
using System.Runtime.InteropServices;
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
            Assert.Equal("showDebugPanel", fields[0].Id);
            Assert.Equal("Show debug panel", fields[0].Label);
            Assert.True(fields[0].OptionsVisible);
            Assert.False(fields[0].DebugVisible);
            Assert.Equal("feelPreset", fields[1].Id);
            Assert.Equal(FeelControlKind.Dropdown, fields[1].OptionsKind);
            Assert.Equal("zoomSensitivity", fields[5].Id);
            Assert.Equal("Zoom", fields[5].Section);
            Assert.Equal(FeelControlKind.Slider, fields[5].OptionsKind);
            Assert.Equal(FeelControlKind.Numeric, fields[5].DebugKind);
        }

        [Fact]
        public void OptionsAndDebug_ShareCatalogButFilterVisibility()
        {
            var options = OptionsHost.BuildDescriptors(FeelHostKind.Options);
            var debug = OptionsHost.BuildDescriptors(FeelHostKind.Debug);
            Assert.Contains(options, d => d.Id == "showDebugPanel");
            Assert.DoesNotContain(debug, d => d.Id == "showDebugPanel");
            Assert.Contains(debug, d => d.Id == "reset");
            Assert.DoesNotContain(options, d => d.Id == "reset");
            Assert.Contains(debug, d => d.Id == "zoomDeadband");
            Assert.DoesNotContain(options, d => d.Id == "zoomDeadband");
            Assert.Contains(options, d => d.Id == "panSensitivityX");
            Assert.Contains(debug, d => d.Id == "panSensitivityX");
            Assert.Equal("General", options[0].Section);
            Assert.Equal("Orbit", options[options.Count - 1].Section);
            Assert.Equal("General", debug[0].Section);
            Assert.Equal("Orbit", debug[debug.Count - 1].Section);
        }

        [Fact]
        public void OptionsDescriptors_UseSliders_DebugUsesNumerics()
        {
            FeelControlDescriptor zoomOptions = FindDesc(
                OptionsHost.BuildDescriptors(FeelHostKind.Options),
                "zoomSensitivity"
            );
            FeelControlDescriptor zoomDebug = FindDesc(
                OptionsHost.BuildDescriptors(FeelHostKind.Debug),
                "zoomSensitivity"
            );
            Assert.Equal(FeelControlKind.Slider, zoomOptions.Kind);
            Assert.Equal(FeelControlKind.Numeric, zoomDebug.Kind);
        }

        [Fact]
        public void Catalog_HasNoMasterSensitivityOrOptionsReset()
        {
            foreach (FeelCatalogField field in FeelCatalog.AllFields())
            {
                Assert.NotEqual("sensitivity", field.Id);
                Assert.NotEqual("resetFactory", field.Id);
            }
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

        [Fact]
        public void HostMapping_SliderDropdownButton_NeverMapToCheckbox()
        {
            Assert.NotEqual(FeelControlKind.Checkbox, OptionsHost.MapKind(FeelControlKind.Slider));
            Assert.NotEqual(
                FeelControlKind.Checkbox,
                OptionsHost.MapKind(FeelControlKind.Dropdown)
            );
            Assert.NotEqual(FeelControlKind.Checkbox, OptionsHost.MapKind(FeelControlKind.Button));
            Assert.NotEqual(FeelControlKind.Checkbox, OptionsHost.MapKind(FeelControlKind.Numeric));

            Assert.Equal(FeelControlKind.Checkbox, OptionsHost.MapKind(FeelControlKind.Toggle));
            Assert.Equal(FeelControlKind.Slider, DebugHost.MapKind(FeelControlKind.Slider));
            Assert.Equal(FeelControlKind.Dropdown, DebugHost.MapKind(FeelControlKind.Dropdown));
            Assert.Equal(
                FeelControlKind.Button,
                FeelHostMapping.ExpectedToolkit(FeelControlKind.Button)
            );

            FeelHostMapping.AssertKindMapsTo(FeelControlKind.Toggle, FeelControlKind.Checkbox);
            FeelHostMapping.AssertKindMapsTo(FeelControlKind.Slider, FeelControlKind.Slider);
        }

        [Fact]
        public void Catalog_EveryField_MapsToMatchingToolkitKind()
        {
            foreach (FeelCatalogField field in FeelCatalog.AllFields())
            {
                FeelControlKind optionsToolkit = FeelHostMapping.MapKind(field.OptionsKind);
                FeelControlKind debugToolkit = FeelHostMapping.MapKind(field.DebugKind);
                if (field.OptionsKind == FeelControlKind.Toggle)
                {
                    Assert.Equal(FeelControlKind.Checkbox, optionsToolkit);
                }
                else
                {
                    Assert.Equal(field.OptionsKind, optionsToolkit);
                }

                if (field.DebugKind == FeelControlKind.Toggle)
                {
                    Assert.Equal(FeelControlKind.Checkbox, debugToolkit);
                }
                else
                {
                    Assert.Equal(field.DebugKind, debugToolkit);
                }
            }
        }

        [Fact]
        public void EnsureDirtyNewPreset_DoesNotRewriteEnvelopeFile()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                Assert.True(File.Exists(path));
                DateTime before = File.GetLastWriteTimeUtc(path);

                FeelProfiles.EnsureDirtyNewPreset(settings, store);

                Assert.Equal(FeelProfiles.NameNewPreset, settings.ActiveFeelPresetName);
                Assert.Equal(before, File.GetLastWriteTimeUtc(path));
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
        public void Editor_ApplyGain_LeavesPendingDirtyOrClearsWithSingleSaveNow()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
                var editor = new FeelEditor(settings, store);

                editor.ApplyGain((s, v) => s.ZoomGain = v, 1.42f);

                Assert.True(editor.IsDirty);
                Assert.Equal(FeelProfiles.NameNewPreset, settings.ActiveFeelPresetName);
                Assert.True(store.HasPendingDirty);
                store.SaveNow(settings);
                Assert.False(store.HasPendingDirty);
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
        public void DebugHost_ApplyVisibility_ReadsAssistUiEnabled()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                var editor = new FeelEditor(settings, store);
                DebugHost.EnsureCreated(editor);
                Assert.True(DebugHost.IsCreated);

                editor.SetShowDebugPanel(true);
                DebugHost.ApplyVisibility();
                Assert.True(DebugHost.IsVisible);

                settings.DebugPanelDismissed = true;
                DebugHost.ApplyVisibility();
                Assert.False(DebugHost.IsVisible);

                editor.SetShowDebugPanel(true);
                DebugHost.ApplyVisibility();
                Assert.True(DebugHost.IsVisible);

                DebugHost.Destroy();
                Assert.False(DebugHost.IsCreated);
            }
            finally
            {
                DebugHost.Destroy();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void DebugHost_BuildPanelModel_MatchesCatalogInventoryAndValues()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                settings.ActiveFeelPresetName = FeelProfiles.NameFast;
                settings.ZoomGain = 1.25f;
                settings.AssistUiEnabled = true;
                var editor = new FeelEditor(settings, store);

                var model = DebugHost.BuildPanelModel(editor);
                var descriptors = DebugHost.BuildDescriptors();

                Assert.Equal(descriptors.Count, model.Count);
                for (int i = 0; i < descriptors.Count; i++)
                {
                    Assert.Equal(descriptors[i].Id, model[i].Id);
                    Assert.Equal(descriptors[i].Section, model[i].Section);
                    Assert.Equal(
                        FeelHostMapping.MapKind(descriptors[i].Kind),
                        model[i].ToolkitKind
                    );
                }

                FeelPanelEntry preset = FindEntry(model, "feelPreset");
                Assert.Equal("dropdown", preset.ValueKind);
                Assert.Equal(FeelProfiles.NameFast, preset.TextValue);

                FeelPanelEntry zoom = FindEntry(model, "zoomSensitivity");
                Assert.Equal("numeric", zoom.ValueKind);
                Assert.NotNull(zoom.NumericValue);

                Assert.Throws<InvalidOperationException>(() => FindEntry(model, "showDebugPanel"));
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
        public void Catalog_Ids_DoNotExposeAssistButtonSteps()
        {
            foreach (FeelCatalogField field in FeelCatalog.AllFields())
            {
                Assert.DoesNotContain("Step", field.Id, StringComparison.OrdinalIgnoreCase);
            }

            foreach (FeelControlDescriptor d in OptionsHost.BuildDescriptors())
            {
                Assert.DoesNotContain("Step", d.Id, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void DebugHost_ShouldShowRootAndReopen_FollowAssistDismissRules()
        {
            Assert.True(DebugHost.ShouldShowRoot(true, false));
            Assert.False(DebugHost.ShouldShowRoot(true, true));
            Assert.False(DebugHost.ShouldShowRoot(false, false));
            Assert.True(DebugHost.ShouldShowReopen(true, true));
            Assert.False(DebugHost.ShouldShowReopen(true, false));
        }

        [Fact]
        public void SuggestFeelSaveAsName_UsesNextNumberedNewPreset()
        {
            string path = Path.Combine(Path.GetTempPath(), "tcc-feel-" + Path.GetRandomFileName());
            try
            {
                var store = new SettingsStore(path);
                ModSettings settings = store.LoadOrFactory();
                var editor = new FeelEditor(settings, store);
                settings.ActiveFeelPresetName = FeelProfiles.NameNewPreset;
                Assert.Equal("New Preset 1", OptionsHost.SuggestFeelSaveAsName(editor));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static FeelControlDescriptor FindDesc(
            System.Collections.Generic.IList<FeelControlDescriptor> list,
            string id
        )
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    return list[i];
                }
            }

            throw new InvalidOperationException("Missing descriptor: " + id);
        }

        private static FeelPanelEntry FindEntry(
            System.Collections.Generic.IList<FeelPanelEntry> model,
            string id
        )
        {
            for (int i = 0; i < model.Count; i++)
            {
                if (model[i].Id == id)
                {
                    return model[i];
                }
            }

            throw new InvalidOperationException("Missing panel entry: " + id);
        }
    }

    internal static class PlatformTestFacts
    {
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    internal sealed class MacOsFactAttribute : FactAttribute
    {
        public MacOsFactAttribute()
        {
            if (!PlatformTestFacts.IsMacOS)
            {
                Skip = "macOS only";
            }
        }
    }

    internal sealed class SkipOnMacOsFactAttribute : FactAttribute
    {
        public SkipOnMacOsFactAttribute()
        {
            if (PlatformTestFacts.IsMacOS)
            {
                Skip = "Not applicable on macOS";
            }
        }
    }
}
