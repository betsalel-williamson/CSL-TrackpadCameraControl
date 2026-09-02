using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class OptionsPanelNavigationTests
    {
        [Fact]
        public void TrySetCategory_NullPanel_ReturnsFalse()
        {
            Assert.False(OptionsPanelNavigation.TrySetCategory(null, "Trackpad"));
        }

        [Fact]
        public void TrySetCategory_EmptyCategory_ReturnsFalse()
        {
            Assert.False(OptionsPanelNavigation.TrySetCategory(new FakeOptionsPanel(), ""));
        }

        [Fact]
        public void TrySetCategory_InvokesSetCategoryWhenPresent()
        {
            var panel = new FakeOptionsPanel();
            Assert.True(
                OptionsPanelNavigation.TrySetCategory(
                    panel,
                    "Trackpad Camera Control (macOS) 0.2.0"
                )
            );
            Assert.Equal("Trackpad Camera Control (macOS) 0.2.0", panel.LastCategory);
        }

        [Fact]
        public void TrySetCategory_MissingMethod_ReturnsFalse()
        {
            Assert.False(OptionsPanelNavigation.TrySetCategory(new object(), "Trackpad"));
        }

        private sealed class FakeOptionsPanel
        {
            public string LastCategory { get; private set; }

            public void SetCategory(string category)
            {
                LastCategory = category;
            }
        }
    }
}
