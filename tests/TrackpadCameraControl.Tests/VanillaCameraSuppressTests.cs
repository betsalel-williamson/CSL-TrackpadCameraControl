using System;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class VanillaCameraSuppressTests : IDisposable
    {
        public VanillaCameraSuppressTests()
        {
            ResetSuppressStatics();
        }

        public void Dispose()
        {
            ResetSuppressStatics();
        }

        private static void ResetSuppressStatics()
        {
            VanillaCameraSuppress.Enabled = false;
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
            InputGates.ResetTestHooks();
        }

        [Fact]
        public void ShouldSkipScrollWheel_WhenDisabled_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = false;
            Assert.False(
                VanillaCameraSuppress.ShouldSkipScrollWheel(
                    preciseTrackpad: true,
                    menuOrOverUi: false
                )
            );
        }

        [Fact]
        public void ShouldSkipScrollWheel_WhenEnabledPreciseWorld_ReturnsTrue()
        {
            VanillaCameraSuppress.Enabled = true;
            Assert.True(
                VanillaCameraSuppress.ShouldSkipScrollWheel(
                    preciseTrackpad: true,
                    menuOrOverUi: false
                )
            );
        }

        [Fact]
        public void ShouldSkipScrollWheel_WhenEnabledWheel_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = true;
            Assert.False(
                VanillaCameraSuppress.ShouldSkipScrollWheel(
                    preciseTrackpad: false,
                    menuOrOverUi: false
                )
            );
        }

        [Fact]
        public void ShouldSkipScrollWheel_WhenEnabledOverUi_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = true;
            Assert.False(
                VanillaCameraSuppress.ShouldSkipScrollWheel(
                    preciseTrackpad: true,
                    menuOrOverUi: true
                )
            );
        }

        [Fact]
        public void ShouldRunVanillaScrollWheel_WhenUnfocused_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = true;
            InputGates.GameFocusedOverride = () => false;
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            Assert.False(VanillaCameraSuppress.ShouldRunVanillaScrollWheel());
        }

        [Fact]
        public void ShouldRunVanillaMouseEvents_WhenUnfocused_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = true;
            InputGates.GameFocusedOverride = () => false;
            Assert.False(VanillaCameraSuppress.ShouldRunVanillaMouseEvents(true));
            Assert.False(VanillaCameraSuppress.ShouldRunVanillaMouseEvents(false));
        }

        [Fact]
        public void ShouldFlushPendingOrbit_WhenUnfocused_ReturnsFalse()
        {
            VanillaCameraSuppress.Enabled = true;
            InputGates.GameFocusedOverride = () => false;
            Assert.False(VanillaCameraSuppress.ShouldFlushPendingOrbit());
        }

        [Fact]
        public void ShouldSkipScrollWheel_Parameterless_UsesSettableState()
        {
            VanillaCameraSuppress.Enabled = true;
            VanillaCameraSuppress.PreciseTrackpadScroll = true;
            VanillaCameraSuppress.MenuOrOverUi = false;
            Assert.True(VanillaCameraSuppress.ShouldSkipScrollWheel());

            VanillaCameraSuppress.MenuOrOverUi = true;
            Assert.False(VanillaCameraSuppress.ShouldSkipScrollWheel());

            VanillaCameraSuppress.MenuOrOverUi = false;
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            Assert.False(VanillaCameraSuppress.ShouldSkipScrollWheel());
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
