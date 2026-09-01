using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class QaClipboardReportTests
    {
        [Fact]
        public void Format_BuildOnly_ExcludesSystemSections()
        {
            string text = QaClipboardReport.Format(false);
            Assert.False(string.IsNullOrEmpty(text));
            Assert.StartsWith("Built (UTC):", text);
            Assert.Contains("Mod:", text);
            Assert.DoesNotContain("--- System ---", text);
            Assert.DoesNotContain("--- Input devices ---", text);
            Assert.DoesNotContain("--- Assemblies ---", text);
        }

        [Fact]
        public void Format_WithSystemInfo_IncludesSystemSections()
        {
            string text = QaClipboardReport.Format(true);
            Assert.False(string.IsNullOrEmpty(text));
            Assert.Contains("--- System ---", text);
            Assert.Contains("OS:", text);
            Assert.Contains("Model:", text);
            Assert.DoesNotContain("CPU:", text);
            Assert.DoesNotContain("Memory:", text);
            Assert.Contains("--- Input devices ---", text);
            Assert.DoesNotContain("(unable to enumerate input devices)", text);
        }

        [Fact]
        public void Format_WithSystemInfo_IncludesAssembliesSection()
        {
            string text = QaClipboardReport.Format(true);
            Assert.Contains("--- Assemblies ---", text);
            Assert.Contains("TrackpadCameraControl:", text);
            Assert.Contains("UnityEngine:", text);
            Assert.Contains("0Harmony:", text);
            Assert.Contains("CitiesHarmony.API:", text);
        }

        [Fact]
        public void FormatAssemblyVersion_ReturnsMissingForUnknown()
        {
            Assert.Equal(
                "missing",
                QaAssemblyVersions.FormatAssemblyVersion("DefinitelyNotLoaded.Assembly.XYZ")
            );
        }

        [Fact]
        public void FormatModelId_UsesUsbHexWhenVendorPresent()
        {
            Assert.Equal("046D:C24E", MacQaSystemInfo.FormatModelId(0x046D, 0xC24E));
            Assert.Equal("pid 035A", MacQaSystemInfo.FormatModelId(0, 858));
            Assert.Null(MacQaSystemInfo.FormatModelId(0, 0));
        }

        [Fact]
        public void FormatDeviceDisplay_AppendsModelTransportWithoutSerial()
        {
            string display = MacQaSystemInfo.FormatDeviceDisplay(
                "Logitech G500s Laser Gaming Mouse",
                0x046D,
                0xC24E,
                0x8401,
                "USB",
                false
            );
            Assert.Equal("Logitech G500s Laser Gaming Mouse (046D:C24E · rev 8401 · USB)", display);
            Assert.DoesNotContain("Serial", display);
        }

        [Fact]
        public void FormatDeviceLine_ShowsQuantityForDuplicateModels()
        {
            Assert.Equal(
                "Magic Trackpad (05AC:030E · Bluetooth)",
                MacQaSystemInfo.FormatDeviceLine("Magic Trackpad (05AC:030E · Bluetooth)", 1)
            );
            Assert.Equal(
                "Magic Trackpad (05AC:030E · Bluetooth) ×2",
                MacQaSystemInfo.FormatDeviceLine("Magic Trackpad (05AC:030E · Bluetooth)", 2)
            );
        }
    }
}
