using System;
using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(ModOptionsStoreCollection.Name)]
    public class ModOptionsTests
    {
        [Fact]
        public void CaptureBackendIndex_RoundTripsAppleAndContacts()
        {
            Assert.Equal(
                CaptureBackend.AppleGestures,
                ModOptions.IndexToCaptureBackend(
                    ModOptions.CaptureBackendToIndex(CaptureBackend.AppleGestures)
                )
            );
            Assert.Equal(
                CaptureBackend.Contacts,
                ModOptions.IndexToCaptureBackend(
                    ModOptions.CaptureBackendToIndex(CaptureBackend.Contacts)
                )
            );
        }

        [Fact]
        public void ApplyCaptureBackendIndex_SelectsContacts()
        {
            var settings = new ModSettings { CaptureBackend = CaptureBackend.AppleGestures };
            ModOptions.ApplyCaptureBackendIndex(
                settings,
                ModOptions.CaptureBackendToIndex(CaptureBackend.Contacts)
            );
            Assert.Equal(CaptureBackend.Contacts, settings.CaptureBackend);
        }

        [Fact]
        public void ApplyCaptureBackendIndex_SelectsApple()
        {
            var settings = new ModSettings { CaptureBackend = CaptureBackend.Contacts };
            ModOptions.ApplyCaptureBackendIndex(
                settings,
                ModOptions.CaptureBackendToIndex(CaptureBackend.AppleGestures)
            );
            Assert.Equal(CaptureBackend.AppleGestures, settings.CaptureBackend);
        }

        [Fact]
        public void ClampGain_RoundsPositiveValues()
        {
            Assert.Equal(1.234f, ModOptions.ClampGain(1.234f));
            Assert.Equal(999f, ModOptions.ClampGain(999f));
        }

        [Fact]
        public void ApplyPanGainX_StoresRoundedPositive()
        {
            var settings = new ModSettings { PanGainX = 0.50f };
            ModOptions.ApplyPanGainX(settings, 999f);
            Assert.Equal(999f, settings.PanGainX);
        }

        [Fact]
        public void EnsureSettings_ReusesExisting()
        {
            ModTestState.Reset();
            try
            {
                string dir = Path.Combine(
                    Path.GetTempPath(),
                    "tcc-ensure-" + Guid.NewGuid().ToString("N")
                );
                Directory.CreateDirectory(dir);
                ModOptions.Store = new ModSettingsStore(Path.Combine(dir, "settings.xml"));
                ModSettings first = Mod.EnsureSettings();
                first.PanGainX = 2.25f;
                ModSettings second = Mod.EnsureSettings();
                Assert.Same(first, second);
                Assert.Equal(2.25f, second.PanGainX);
            }
            finally
            {
                ModTestState.Reset();
            }
        }
    }
}
