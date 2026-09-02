using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class OptionsPanelNavigationTests
    {
        [Fact]
        public void TrySelectMod_NullPanel_ReturnsFalse()
        {
            Assert.False(OptionsPanelNavigation.TrySelectMod(null, "Trackpad"));
        }

        [Fact]
        public void TrySelectMod_EmptyName_ReturnsFalse()
        {
            Assert.False(OptionsPanelNavigation.TrySelectMod(new FakeOptionsPanel(), ""));
        }

        [Fact]
        public void TrySelectMod_InvokesSelectModWhenPresent()
        {
            var panel = new FakeOptionsPanel();
            Assert.True(
                OptionsPanelNavigation.TrySelectMod(panel, "Trackpad Camera Control (macOS) 0.2.0")
            );
            Assert.Equal("Trackpad Camera Control (macOS) 0.2.0", panel.LastModName);
        }

        [Fact]
        public void TrySelectMod_MissingMethod_ReturnsFalse()
        {
            Assert.False(OptionsPanelNavigation.TrySelectMod(new object(), "Trackpad"));
        }

        private sealed class FakeOptionsPanel
        {
            public string LastModName { get; private set; }

            public void SelectMod(string modName)
            {
                LastModName = modName;
            }
        }
    }
}
