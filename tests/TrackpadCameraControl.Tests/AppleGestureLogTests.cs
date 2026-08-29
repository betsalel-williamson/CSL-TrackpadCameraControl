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
            Assert.Equal(
                "apple src=local type=scroll phase=changed mods=opt momentum=changed sdx=1.2500 sdy=-4.0000 dx=0.0000 dy=-1.0000 precise=1",
                line
            );
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
            Assert.Equal("apple src=local type=magnify phase=changed mods=- mag=0.01234", line);
        }
    }
}
