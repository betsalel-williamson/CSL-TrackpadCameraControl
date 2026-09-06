using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class GestureCaptureLogTests
    {
        [Fact]
        public void Line_WritesToConfiguredPath()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "trackpad-capture-log-test-" + System.Guid.NewGuid().ToString("N") + ".log"
            );
            using (new GestureCaptureLogScope(path))
            {
                GestureCaptureLog.Line("hello-capture");
                GestureCaptureLog.Close();
                string text = File.ReadAllText(path);
                Assert.Contains("hello-capture", text);
                Assert.Contains(GestureCaptureLog.OpenedLinePrefix, text);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
