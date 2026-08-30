using TrackpadCameraControl;
using Xunit;
using System;
using System.IO;

namespace TrackpadCameraControl.Tests
{
    public class ModOptionsTests
    {
        [Fact]
        public void CaptureBackendToIndex_Apple_IsZero()
        {
            Assert.Equal(0, ModOptions.CaptureBackendToIndex(CaptureBackend.AppleGestures));
        }

        [Fact]
        public void CaptureBackendToIndex_Contacts_IsOne()
        {
            Assert.Equal(1, ModOptions.CaptureBackendToIndex(CaptureBackend.Contacts));
        }

        [Fact]
        public void ApplyCaptureBackendIndex_SelectsContacts()
        {
            var settings = new ModSettings { CaptureBackend = CaptureBackend.AppleGestures };
            ModOptions.ApplyCaptureBackendIndex(settings, 1);
            Assert.Equal(CaptureBackend.Contacts, settings.CaptureBackend);
        }

        [Fact]
        public void ApplyCaptureBackendIndex_SelectsApple()
        {
            var settings = new ModSettings { CaptureBackend = CaptureBackend.Contacts };
            ModOptions.ApplyCaptureBackendIndex(settings, 0);
            Assert.Equal(CaptureBackend.AppleGestures, settings.CaptureBackend);
        }

        [Fact]
        public void ClampSensitivity_RoundsPositiveValues()
        {
            Assert.Equal(1.23f, ModOptions.ClampSensitivity(1.234f));
            Assert.Equal(999f, ModOptions.ClampSensitivity(999f));
        }

        [Fact]
        public void ApplyPanSensitivityX_StoresRoundedPositive()
        {
            var settings = new ModSettings { PanSensitivityX = 0.50f };
            ModOptions.ApplyPanSensitivityX(settings, 999f);
            Assert.Equal(999f, settings.PanSensitivityX);
        }

        [Fact]
        public void EnsureSettings_ReusesExisting()
        {
            Mod.ClearSettingsForTests();
            try
            {
                string dir = Path.Combine(
                    Path.GetTempPath(),
                    "tcc-ensure-" + Guid.NewGuid().ToString("N")
                );
                Directory.CreateDirectory(dir);
                ModOptions.Store = new ModSettingsStore(Path.Combine(dir, "settings.xml"));
                ModSettings first = Mod.EnsureSettings();
                first.PanSensitivityX = 2.25f;
                ModSettings second = Mod.EnsureSettings();
                Assert.Same(first, second);
                Assert.Equal(2.25f, second.PanSensitivityX);
            }
            finally
            {
                Mod.ClearSettingsForTests();
            }
        }
    }
}
