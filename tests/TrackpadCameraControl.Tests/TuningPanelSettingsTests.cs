using System;
using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class CameraApplicatorButtonTests
    {
        [Fact]
        public void ApplyButton_Pan_UsesButtonStepNotDragScale()
        {
            var camera = new FakeCameraController
            {
                Size = 100f,
                TargetX = 0f,
                TargetZ = 0f,
            };
            var settings = new ModSettings
            {
                SignInvertPanX = false,
                SignInvertPanY = false,
                PanGainX = 10f,
                PanGainY = 10f,
                PanStepX = 0.05f,
                PanStepY = 0.05f,
            };

            CameraApplicator.ApplyButton(CameraOp.Pan, 1f, 0f, 0f, 0f, settings, camera);

            // Button step 0.05 * size 100 = 5 world units; drag scale must not apply.
            Assert.Equal(5f, camera.TargetX, 3);
        }

        [Fact]
        public void ApplyButton_Pan_RespectsInvert()
        {
            var camera = new FakeCameraController
            {
                Size = 100f,
                TargetX = 0f,
                TargetZ = 0f,
            };
            var settings = new ModSettings { PanStepX = 0.05f, SignInvertPanX = true };

            CameraApplicator.ApplyButton(CameraOp.Pan, 1f, 0f, 0f, 0f, settings, camera);

            Assert.Equal(-5f, camera.TargetX, 3);
        }

        [Fact]
        public void ApplyDrag_Pan_UsesDragScale()
        {
            var camera = new FakeCameraController
            {
                Size = 100f,
                TargetX = 0f,
                TargetZ = 0f,
            };
            var settings = new ModSettings
            {
                SignInvertPanX = false,
                SignInvertPanY = false,
                PanGainX = 2f,
                PanGainY = 1f,
            };

            CameraApplicator.Apply(CameraOp.Pan, 0.05f, 0f, 0f, 0f, settings, camera);

            Assert.Equal(10f, camera.TargetX, 3);
        }

        [Fact]
        public void ApplyButton_Zoom_UsesButtonStep()
        {
            var camera = new FakeCameraController { Size = 100f };
            var settings = new ModSettings { ZoomGain = 10f, ZoomStep = 0.1f };

            CameraApplicator.ApplyButton(CameraOp.Zoom, 0f, 0f, 1f, 0f, settings, camera);

            // size * (1 - 0.1) = 90
            Assert.Equal(90f, camera.Size, 3);
        }
    }

    public class DragLowPassTests
    {
        [Fact]
        public void Filter_Disabled_IsIdentity()
        {
            var lp = new DragLowPass();
            var settings = new ModSettings { PanFilterEnabled = false };
            float dx = 0.5f;
            float dy = 0.25f;
            float pinch = 0f;
            float rotate = 0f;

            lp.Filter(CameraOp.Pan, settings, ref dx, ref dy, ref pinch, ref rotate);

            Assert.Equal(0.5f, dx);
            Assert.Equal(0.25f, dy);
        }

        [Fact]
        public void Filter_ContactsFlagOff_IgnoresPanFilterEnabled()
        {
            // EnableContactsCapture is const false for ship; LP must pass through raw.
            Assert.False(FeatureFlags.EnableContactsCapture);

            var lp = new DragLowPass();
            var settings = new ModSettings { PanFilterEnabled = true, PanFilterAlpha = 0.5f };
            float dx = 1f;
            float dy = 0f;
            float pinch = 0f;
            float rotate = 0f;

            lp.Filter(CameraOp.Pan, settings, ref dx, ref dy, ref pinch, ref rotate);
            Assert.Equal(1f, dx);

            dx = 0f;
            lp.Filter(CameraOp.Pan, settings, ref dx, ref dy, ref pinch, ref rotate);
            Assert.Equal(0f, dx);
        }

        [Fact]
        public void Reset_ClearsState()
        {
            var lp = new DragLowPass();
            var settings = new ModSettings { PanFilterEnabled = true, PanFilterAlpha = 0.5f };
            float dx = 1f;
            float dy = 0f;
            float pinch = 0f;
            float rotate = 0f;
            lp.Filter(CameraOp.Pan, settings, ref dx, ref dy, ref pinch, ref rotate);
            lp.Reset();
            dx = 0f;
            lp.Filter(CameraOp.Pan, settings, ref dx, ref dy, ref pinch, ref rotate);
            Assert.Equal(0f, dx);
        }
    }

    [Collection(ModOptionsStoreCollection.Name)]
    public class ModSettingsStoreTests : IDisposable
    {
        private readonly string _dir;
        private readonly string _path;

        public ModSettingsStoreTests()
        {
            _dir = Path.Combine(
                Path.GetTempPath(),
                "tcc-settings-tests-" + Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(_dir);
            _path = Path.Combine(_dir, "settings.xml");
        }

        public void Dispose()
        {
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
        public void RoundTrip_Current_PreservesScales()
        {
            var store = new ModSettingsStore(_path);
            var settings = new ModSettings
            {
                PanGainX = 2.5f,
                ZoomStep = 0.2f,
                PanFilterEnabled = true,
                PanFilterAlpha = 0.4f,
            };
            store.SaveNow(settings);

            ModSettings loaded = store.LoadOrFactory();
            Assert.Equal(2.5f, loaded.PanGainX);
            Assert.Equal(0.2f, loaded.ZoomStep);
            Assert.True(loaded.PanFilterEnabled);
            Assert.Equal(0.4f, loaded.PanFilterAlpha);
        }

        [Fact]
        public void MissingFile_YieldsFactoryAndPersists()
        {
            var store = new ModSettingsStore(_path);
            ModSettings loaded = store.LoadOrFactory();
            FeelExpectation.AssertMatchesFactoryFeel(loaded);
            Assert.False(loaded.AssistUiEnabled);
            Assert.True(File.Exists(_path));
        }

        [Fact]
        public void RoundTrip_PreservesAssistUiEnabledTrue()
        {
            var store = new ModSettingsStore(_path);
            store.SaveNow(new ModSettings { AssistUiEnabled = true, PanGainX = 1.25f });
            ModSettings loaded = store.LoadOrFactory();
            Assert.True(loaded.AssistUiEnabled);
            Assert.Equal(1.25f, loaded.PanGainX);
        }

        [Fact]
        public void RoundTrip_PreservesDebugQolFields()
        {
            var store = new ModSettingsStore(_path);
            store.SaveNow(
                new ModSettings { IncludeSystemInfoInCopy = false, DebugPanelDismissed = true }
            );

            ModSettings loaded = store.LoadOrFactory();
            Assert.False(loaded.IncludeSystemInfoInCopy);
            Assert.True(loaded.DebugPanelDismissed);
        }

        [Fact]
        public void CorruptFile_YieldsFactory()
        {
            File.WriteAllText(_path, "not-xml{{{");
            var store = new ModSettingsStore(_path);
            ModSettings loaded = store.LoadOrFactory();
            FeelExpectation.AssertMatchesFactoryFeel(loaded);
            Assert.Equal(GesturePreset.MapsPlus, loaded.GesturePreset);
        }

        [Fact]
        public void ResetToFactory_WritesDefaults()
        {
            var store = new ModSettingsStore(_path);
            ModOptions.Store = store;
            try
            {
                var settings = new ModSettings { PanGainX = 9f };
                store.SaveNow(settings);
                ModOptions.ResetToFactory(settings);
                FeelExpectation.AssertMatchesFactoryFeel(settings);
                ModSettings loaded = store.LoadOrFactory();
                FeelExpectation.AssertMatchesFactoryFeel(loaded);
            }
            finally
            {
                ModOptions.Store = null;
            }
        }

        [Fact]
        public void Envelope_UserPresetsRoundTripEmpty()
        {
            var store = new ModSettingsStore(_path);
            store.SaveNow(new ModSettings { PanGainX = 1.25f });
            string xml = File.ReadAllText(_path);
            Assert.Contains("UserPresets", xml);
            Assert.Contains("SchemaVersion", xml);
        }
    }

    public class ModOptionsParseTests
    {
        [Fact]
        public void TryParseFloat_Valid_Stores()
        {
            Assert.True(ModOptions.TryParseFloat("1.25", out float v));
            Assert.Equal(1.25f, v);
        }

        [Fact]
        public void TryParseFloat_Garbage_Fails()
        {
            Assert.False(ModOptions.TryParseFloat("nope", out _));
        }

        [Fact]
        public void TryApplyFloat_StoresRoundedPositive()
        {
            var settings = new ModSettings { PanGainX = 0.50f };
            Assert.True(ModOptions.TryApplyFloat(settings, "999", ModOptions.ApplyPanGainX));
            Assert.Equal(999f, settings.PanGainX);
        }

        [Fact]
        public void ApplyGesturePresetIndex_CAD_SeedsOrbit()
        {
            var settings = new ModSettings();
            ModOptions.ApplyGesturePresetIndex(settings, 1);
            Assert.Equal(GesturePreset.CAD, settings.GesturePreset);
            Assert.Equal(OrbitTrigger.ThreeFinger, settings.OrbitTrigger);
        }

        [Fact]
        public void ClampGain_RoundsPositive_NoUpperCap()
        {
            Assert.Equal(0f, ModOptions.ClampGain(-1f));
            Assert.Equal(999f, ModOptions.ClampGain(999f));
            Assert.Equal(1.5f, ModOptions.ClampGain(1.5f));
        }
    }
}
