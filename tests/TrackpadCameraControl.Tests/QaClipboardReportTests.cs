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
        }

        [Fact]
        public void Format_WithSystemInfo_IncludesSystemSections()
        {
            string text = QaClipboardReport.Format(true);
            Assert.False(string.IsNullOrEmpty(text));
            Assert.Contains("--- System ---", text);
            Assert.Contains("OS:", text);
            Assert.Contains("--- Input devices ---", text);
            Assert.DoesNotContain("(unable to enumerate input devices)", text);
        }
    }
}
