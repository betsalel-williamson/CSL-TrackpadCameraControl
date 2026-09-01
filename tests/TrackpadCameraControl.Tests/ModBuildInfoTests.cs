using System;
using System.Globalization;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class ModBuildInfoTests
    {
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
    }
}
