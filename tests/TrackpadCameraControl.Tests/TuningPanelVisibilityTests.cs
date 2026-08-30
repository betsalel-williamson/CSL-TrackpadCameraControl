using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class TuningPanelVisibilityTests
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public void ShouldShowRoot_ReflectsAssistAndDismiss(
            bool assistEnabled,
            bool dismissed,
            bool expected
        )
        {
            Assert.Equal(expected, TuningPanelHost.ShouldShowRoot(assistEnabled, dismissed));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void ShouldShowReopen_ReflectsAssistAndDismiss(
            bool assistEnabled,
            bool dismissed,
            bool expected
        )
        {
            Assert.Equal(expected, TuningPanelHost.ShouldShowReopen(assistEnabled, dismissed));
        }

        [Fact]
        public void ClearUserDismiss_ClearsSessionFlag()
        {
            TuningPanelHost.SetUserDismissedForTests(true);
            try
            {
                TuningPanelHost.ClearUserDismiss();
                Assert.False(TuningPanelHost.IsUserDismissedForTests());
            }
            finally
            {
                TuningPanelHost.SetUserDismissedForTests(false);
            }
        }
    }
}
