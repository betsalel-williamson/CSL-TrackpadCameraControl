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
    }
}
