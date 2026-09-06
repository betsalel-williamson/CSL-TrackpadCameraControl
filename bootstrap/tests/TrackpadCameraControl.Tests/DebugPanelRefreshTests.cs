using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class DebugPanelRefreshTests
    {
        [Fact]
        public void StringArraysEqual_MatchesOrdinalContent()
        {
            Assert.True(
                DebugPanelRefresh.StringArraysEqual(
                    new[] { "Slow", "Default" },
                    new[] { "Slow", "Default" }
                )
            );
            Assert.False(
                DebugPanelRefresh.StringArraysEqual(
                    new[] { "Slow", "Default" },
                    new[] { "Slow", "Fast" }
                )
            );
            Assert.False(
                DebugPanelRefresh.StringArraysEqual(new[] { "Slow" }, new[] { "Slow", "Default" })
            );
        }

        [Fact]
        public void CanRefreshHeadingInPlace_WhenNonEmptyLineCountMatches()
        {
            string heading =
                "Zoom\nChange camera distance / size\nGesture(s): Pinch\nKeymapping(s): none";
            Assert.True(DebugPanelRefresh.CanRefreshHeadingInPlace(heading, 4));
            Assert.False(DebugPanelRefresh.CanRefreshHeadingInPlace(heading, 3));
            Assert.Equal(4, DebugPanelRefresh.NonEmptyHeadingLines(heading).Length);
            Assert.Equal(3, DebugPanelRefresh.NonEmptyHeadingLines("A\n\nB\nC").Length);
        }
    }
}
