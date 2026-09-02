using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public sealed class GameFocusActivationTests
    {
        [Fact]
        public void TryActivate_WithoutCitiesHost_ReturnsFalse()
        {
            Assert.False(GameFocusActivation.TryActivate());
        }

        [Fact]
        public void ArmCursorHideFollowUp_SetsRemainingFrames()
        {
            GameFocusActivation.ArmCursorHideFollowUp(3);
            Assert.Equal(3, GameFocusActivation.CursorHideFramesRemaining);
            GameFocusActivation.ArmCursorHideFollowUp(0);
            Assert.Equal(0, GameFocusActivation.CursorHideFramesRemaining);
        }

        [Fact]
        public void TickCursorHideFollowUp_WithoutCities_ClearsRemaining()
        {
            GameFocusActivation.ArmCursorHideFollowUp(2);
            GameFocusActivation.TickCursorHideFollowUp(null);
            Assert.Equal(0, GameFocusActivation.CursorHideFramesRemaining);
        }
    }
}
