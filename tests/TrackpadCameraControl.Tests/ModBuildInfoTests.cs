using System;
using System.Globalization;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class ModBuildInfoTests
    {
        [Fact]
        public void GetProductVersionDisplay_MatchesPackageSemverShape()
        {
            string product = Mod.GetProductVersionDisplay();
            Assert.False(string.IsNullOrEmpty(product));
            string[] parts = product.Split('.');
            Assert.True(parts.Length >= 2, "product version should be at least major.minor");
            Assert.True(int.TryParse(parts[0], out _), "major");
            Assert.True(int.TryParse(parts[1], out _), "minor");
        }

        [Fact]
        public void GetAssemblyBuildTimestampUtcDisplay_IsPresentAndParses()
        {
            string built = Mod.GetAssemblyBuildTimestampUtcDisplay();
            Assert.False(string.IsNullOrEmpty(built));
            Assert.True(
                DateTime.TryParse(
                    built,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out DateTime parsed
                ),
                "BuildTimestampUtc should parse: " + built
            );
            Assert.Equal(DateTimeKind.Utc, parsed.Kind);
        }

        [Fact]
        public void GetAssemblyIdentityDisplay_HasFourComponents()
        {
            string asm = Mod.GetAssemblyIdentityDisplay();
            Assert.False(string.IsNullOrEmpty(asm));
            string[] parts = asm.Split('.');
            Assert.Equal(4, parts.Length);
        }

        [Fact]
        public void GetBuildInfoFooterDisplay_MatchesFooterShape()
        {
            string footer = Mod.GetBuildInfoFooterDisplay();
            Assert.False(string.IsNullOrEmpty(footer));
            Assert.StartsWith("Built (UTC):", footer);
            Assert.Contains("asm ", footer);
        }
    }
}
