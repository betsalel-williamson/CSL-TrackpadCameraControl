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
        public void GetBuildInfoFooterDisplay_RespectsDevBuildIdentity()
        {
            string footer = Mod.GetBuildInfoFooterDisplay();
            if (BuildInfo.ShowDevBuildIdentity)
            {
                Assert.False(string.IsNullOrEmpty(footer));
                Assert.StartsWith("Built (UTC):", footer);
                Assert.DoesNotContain("\n", footer);
                Assert.DoesNotContain("asm ", footer);
            }
            else
            {
                Assert.Null(footer);
            }
        }

        [Fact]
        public void GetBuildInfoPanelDisplay_RespectsDevBuildIdentity()
        {
            string line = Mod.GetBuildInfoPanelDisplay();
            if (BuildInfo.ShowDevBuildIdentity)
            {
                Assert.False(string.IsNullOrEmpty(line));
                Assert.StartsWith("Built (local):", line);
                Assert.DoesNotContain("asm ", line);
            }
            else
            {
                Assert.Null(line);
            }
        }

        [Fact]
        public void OptionsTitle_MatchesShippingProductName()
        {
            string title = Mod.OptionsTitle;
            Assert.StartsWith("Trackpad Camera Control (macOS)", title);
            Assert.DoesNotContain("Rewrite", title);
        }

        [Fact]
        public void Description_DoesNotAdvertiseRewriteInContentManager()
        {
            Mod mod = new Mod();
            Assert.DoesNotContain("Rewrite", mod.Description);
        }

        [Fact]
        public void DebugPanelTitle_UsesProductSemverByDefault()
        {
            string title = Mod.DebugPanelTitle;
            Assert.StartsWith("Trackpad Camera Control (macOS) ", title);
            Assert.DoesNotContain("Rewrite", title);
            string token = BuildInfo.ShowDevBuildIdentity
                ? Mod.GetAssemblyIdentityDisplay()
                : Mod.GetProductVersionDisplay();
            Assert.False(string.IsNullOrEmpty(token));
            Assert.EndsWith(token, title);
            if (!BuildInfo.ShowDevBuildIdentity)
            {
                Assert.Equal(Mod.GetProductVersionDisplay(), token);
                string asm = Mod.GetAssemblyIdentityDisplay();
                if (!string.IsNullOrEmpty(asm) && asm != token)
                {
                    Assert.False(
                        title.EndsWith(asm, StringComparison.Ordinal),
                        "Debug title should not use assembly build/revision by default"
                    );
                }
            }
        }
    }
}
