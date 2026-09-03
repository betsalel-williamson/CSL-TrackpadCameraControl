using System;
using System.Globalization;
using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
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
        public void GetBuildInfoFooterDisplay_IsUtcTimestampOnly_WhenDevIdentity()
        {
            if (!BuildInfo.ShowDevBuildIdentity)
            {
                Assert.Null(Mod.GetBuildInfoFooterDisplay());
                return;
            }

            string footer = Mod.GetBuildInfoFooterDisplay();
            Assert.False(string.IsNullOrEmpty(footer));
            Assert.StartsWith("Built (UTC):", footer);
            Assert.DoesNotContain("\n", footer);
            Assert.DoesNotContain("asm ", footer);
        }

        [Fact]
        public void GetBuildInfoPanelDisplay_UsesLocalLabel_WhenDevIdentity()
        {
            if (!BuildInfo.ShowDevBuildIdentity)
            {
                Assert.Null(Mod.GetBuildInfoPanelDisplay());
                return;
            }

            string line = Mod.GetBuildInfoPanelDisplay();
            Assert.False(string.IsNullOrEmpty(line));
            Assert.StartsWith("Built (local):", line);
            Assert.DoesNotContain("asm ", line);
        }

        [Fact]
        public void DebugPanelTitle_IncludesVersionToken()
        {
            string title = Mod.DebugPanelTitle;
            Assert.StartsWith("Trackpad Camera Control Rewrite (macOS) ", title);
            string token = BuildInfo.ShowDevBuildIdentity
                ? Mod.GetAssemblyIdentityDisplay()
                : Mod.GetProductVersionDisplay();
            Assert.False(string.IsNullOrEmpty(token));
            Assert.EndsWith(token, title);
        }
    }
}
