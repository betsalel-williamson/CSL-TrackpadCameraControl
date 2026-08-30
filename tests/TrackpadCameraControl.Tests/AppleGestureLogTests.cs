using AppleGestureProbe;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class AppleGestureLogTests
    {
        [Fact]
        public void ScrollLine_OmitsMagnifyAndIncludesPreciseDeltas()
        {
            string line = AppleGestureLog.Format(
                "local",
                "scroll",
                "changed",
                "opt",
                "changed",
                1.25,
                -4.0,
                0.0,
                -1.0,
                true,
                null,
                null
            );

            Assert.StartsWith(AppleGestureLog.LinePrefix + "local", line);
            Assert.Contains("type=scroll", line);
            Assert.Contains("phase=changed", line);
            Assert.Contains("mods=opt", line);
            Assert.Contains("momentum=changed", line);
            Assert.Contains("sdx=", line);
            Assert.Contains("sdy=", line);
            Assert.Contains("precise=1", line);
            Assert.DoesNotContain(" mag=", line);
            Assert.DoesNotContain(" rot=", line);
        }

        [Fact]
        public void MagnifyLine_IncludesMagOnly()
        {
            string line = AppleGestureLog.Format(
                "local",
                "magnify",
                "changed",
                "-",
                null,
                null,
                null,
                null,
                null,
                null,
                0.01234,
                null
            );

            Assert.StartsWith(AppleGestureLog.LinePrefix + "local", line);
            Assert.Contains("type=magnify", line);
            Assert.Contains("mods=-", line);
            Assert.Contains("mag=", line);
            Assert.DoesNotContain("sdx=", line);
            Assert.DoesNotContain("precise=", line);
            Assert.DoesNotContain(" rot=", line);
        }
    }
}
