using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    public class BuildingToolAngleDeltaTests
    {
        [Fact]
        public void ApplyRenderAngles_AddsRadianDeltaAndSetsChanged()
        {
            float mouse = 1.0f;
            float cached = 2.0f;
            BuildingToolAngleDelta.ApplyRenderAngles(90f, ref mouse, ref cached, out bool changed);

            Assert.True(changed);
            Assert.Equal(1.0f + BuildingToolAngleDelta.Deg2Rad * 90f, mouse, 5);
            Assert.Equal(2.0f + BuildingToolAngleDelta.Deg2Rad * 90f, cached, 5);
        }

        [Fact]
        public void DegreesToRadians_MatchesPiOver180()
        {
            Assert.Equal(
                (float)(System.Math.PI / 2.0),
                BuildingToolAngleDelta.DegreesToRadians(90f),
                5
            );
            Assert.Equal(0f, BuildingToolAngleDelta.DegreesToRadians(0f), 5);
        }

        [Fact]
        public void ApplyRenderAngles_NegativeDelta_Subtracts()
        {
            float mouse = 0f;
            float cached = 0f;
            BuildingToolAngleDelta.ApplyRenderAngles(-45f, ref mouse, ref cached, out bool changed);
            Assert.True(changed);
            Assert.Equal(-BuildingToolAngleDelta.Deg2Rad * 45f, mouse, 5);
            Assert.Equal(-BuildingToolAngleDelta.Deg2Rad * 45f, cached, 5);
        }
    }
}
