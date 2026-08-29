using System;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public sealed class VanillaCameraSuppressTests : IDisposable
    {
        public VanillaCameraSuppressTests()
        {
            VanillaCameraSuppress.Enabled = false;
        }

        public void Dispose()
        {
            VanillaCameraSuppress.Enabled = false;
        }

        [Fact]
        public void ShouldSkipScrollWheel_WhenDisabled_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = false;
            Assert.False(VanillaCameraSuppress.ShouldSkipScrollWheel());
        }

        [Fact]
        public void ShouldSkipScrollWheel_WhenEnabled_ReturnsTrue()
        {
            VanillaCameraSuppress.Enabled = true;
            Assert.True(VanillaCameraSuppress.ShouldSkipScrollWheel());
        }

        [Fact]
        public void ShouldSkipMouseHandler_WhenEnabledAndRotateHeld_ReturnsTrue()
        {
            VanillaCameraSuppress.Enabled = true;
            Assert.True(VanillaCameraSuppress.ShouldSkipMouseHandler(true));
        }

        [Fact]
        public void ShouldSkipMouseHandler_WhenEnabledAndRotateNotHeld_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = true;
            Assert.False(VanillaCameraSuppress.ShouldSkipMouseHandler(false));
        }

        [Fact]
        public void ShouldSkipMouseHandler_WhenDisabledAndRotateHeld_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = false;
            Assert.False(VanillaCameraSuppress.ShouldSkipMouseHandler(true));
        }
    }
}
