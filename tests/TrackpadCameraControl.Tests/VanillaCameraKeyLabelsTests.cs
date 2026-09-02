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
        public void FormatVanillaActionLine_UsesKeymappingPrefix()
        {
            Assert.Equal(
                "Keymapping(s): Middle Mouse · W",
                VanillaCameraKeyLabels.FormatVanillaActionLine("Middle Mouse · W")
            );
            Assert.Equal(
                "Keymapping(s): none",
                VanillaCameraKeyLabels.FormatVanillaActionLine(null)
            );
            Assert.Equal("Keymapping(s): none", VanillaCameraKeyLabels.FormatVanillaActionLine(""));
        }
    }
}
