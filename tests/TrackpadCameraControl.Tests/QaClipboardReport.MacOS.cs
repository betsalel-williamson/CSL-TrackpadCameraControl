using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    /// <summary>Darwin integration tests for QA clipboard system info (IOKit / hw.model).</summary>
    public class QaClipboardReportMacTests
    {
        [MacOsFact]
        public void Format_WithSystemInfo_IncludesModelAndEnumeratedDevices()
        {
            string text = QaClipboardReport.Format(true);
            Assert.Contains("Model:", text);
            Assert.DoesNotContain("(unable to enumerate input devices)", text);
            Assert.DoesNotContain("(macOS input enumeration unavailable on this host)", text);
        }
    }
}
