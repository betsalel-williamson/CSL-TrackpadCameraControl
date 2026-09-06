using System;
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
            Assert.StartsWith("TrackpadCameraControl:", text);
            Assert.Contains("Built (UTC):", text);
            Assert.DoesNotContain("Mod:", text);
            Assert.DoesNotContain("--- System ---", text);
            Assert.DoesNotContain("--- Input devices ---", text);
            Assert.DoesNotContain("--- Assemblies ---", text);
        }

        [Fact]
        public void Format_WithSystemInfo_IncludesCommonSystemSections()
        {
            string text = QaClipboardReport.Format(true);
            Assert.False(string.IsNullOrEmpty(text));
            Assert.Contains("--- System ---", text);
            Assert.Contains("OS:", text);
            Assert.DoesNotContain("CPU:", text);
            Assert.DoesNotContain("Memory:", text);
            Assert.Contains("--- Input devices ---", text);
        }

        [SkipOnMacOsFact]
        public void Format_WithSystemInfo_OffMac_ShowsEnumerationUnavailable()
        {
            string text = QaClipboardReport.Format(true);
            Assert.Contains("(macOS input enumeration unavailable on this host)", text);
        }

        [Fact]
        public void Format_WithSystemInfo_IncludesAssembliesSection()
        {
            string text = QaClipboardReport.Format(true);
            Assert.Contains("--- Assemblies ---", text);
            Assert.StartsWith("TrackpadCameraControl:", text);
            // This mod is only on the header line, not repeated under Assemblies.
            int first = text.IndexOf("TrackpadCameraControl:", StringComparison.Ordinal);
            int second = text.IndexOf(
                "TrackpadCameraControl:",
                first + 1,
                StringComparison.Ordinal
            );
            Assert.Equal(-1, second);
            Assert.Contains("0Harmony:", text);
            Assert.Contains("CitiesHarmony.API:", text);
        }

        [Fact]
        public void Format_WithSystemInfo_OmitsUninformativeUnityAssemblyStamps()
        {
            string text = QaClipboardReport.Format(true);
            Assert.DoesNotContain("UnityEngine:", text);
            Assert.DoesNotContain("Assembly-CSharp:", text);
        }

        [Fact]
        public void ShouldEmitVersionLine_RejectsZeroButKeepsMissing()
        {
            Assert.False(QaAssemblyVersions.ShouldEmitVersionLine(null));
            Assert.False(QaAssemblyVersions.ShouldEmitVersionLine(""));
            Assert.True(QaAssemblyVersions.ShouldEmitVersionLine("missing"));
            Assert.False(QaAssemblyVersions.ShouldEmitVersionLine("0.0.0.0"));
            Assert.True(QaAssemblyVersions.ShouldEmitVersionLine("2.0.1.0"));
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
