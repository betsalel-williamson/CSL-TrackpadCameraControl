using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class VanillaCameraKeyLabelsTests
    {
        [Fact]
        public void JoinBindingLabels_DedupesAndSeparates()
        {
            string joined = VanillaCameraKeyLabels.JoinBindingLabels(new[] { "W", "S", "W", "A" });
            Assert.Equal("W · S · A", joined);
        }

        [Fact]
        public void FormatVanillaActionLine_IncludesBindingsWhenPresent()
        {
            Assert.Equal(
                "Middle Mouse · W: vanilla orbit",
                VanillaCameraKeyLabels.FormatVanillaActionLine("Middle Mouse · W", "vanilla orbit")
            );
            Assert.Equal(
                "vanilla zoom",
                VanillaCameraKeyLabels.FormatVanillaActionLine(null, "vanilla zoom")
            );
        }
    }
}
