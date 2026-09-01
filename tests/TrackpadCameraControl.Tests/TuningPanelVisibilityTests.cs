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
        // OPTIONS "Show debug panel" off ⇒ no reopen chip even if user previously dismissed (X).
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
        public void ClearUserDismiss_ClearsPersistedFlag()
        {
            Mod.SetSettingsForTests(new ModSettings { DebugPanelDismissed = true });
            try
            {
                TuningPanelHost.ClearUserDismiss();
                Assert.False(Mod.Settings.DebugPanelDismissed);
            }
            finally
            {
                Mod.ClearSettingsForTests();
            }
        }
    }
}
