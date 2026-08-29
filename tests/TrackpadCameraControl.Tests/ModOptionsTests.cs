using TrackpadCameraControl;
using Xunit;

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
        public void ClampSensitivity_PinsToRange()
        {
            Assert.Equal(ModOptions.SensitivityMin, ModOptions.ClampSensitivity(-1f));
            Assert.Equal(ModOptions.SensitivityMax, ModOptions.ClampSensitivity(99f));
            Assert.Equal(1.5f, ModOptions.ClampSensitivity(1.5f));
        }

        [Fact]
        public void ApplyPanSensitivityX_ClampsAndStores()
        {
            var settings = new ModSettings();
            ModOptions.ApplyPanSensitivityX(settings, 99f);
            Assert.Equal(ModOptions.SensitivityMax, settings.PanSensitivityX);
        }

        [Fact]
        public void EnsureSettings_ReusesExisting()
        {
            Mod.ClearSettingsForTests();
            try
            {
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
