using System;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class InputGatesCameraPolicyTests : IDisposable
    {
        public InputGatesCameraPolicyTests()
        {
            ResetState();
        }

        public void Dispose()
        {
            ResetState();
        }

        private static void ResetState()
        {
            ModRuntime.ClearForTests();
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
            InputGates.ResetTestHooks();
        }

        [Fact]
        public void ShouldSuppressVanillaScrollWheel_WhenDisabled_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(false);
            Assert.False(
                InputGates.ShouldSuppressVanillaScrollWheel(
                    preciseTrackpad: true,
                    menuOrOverUi: false
                )
            );
        }

        [Fact]
        public void ShouldSuppressVanillaScrollWheel_WhenEnabledPreciseWorld_ReturnsTrue()
        {
            ModRuntime.SetModActiveForTests(true);
            Assert.True(
                InputGates.ShouldSuppressVanillaScrollWheel(
                    preciseTrackpad: true,
                    menuOrOverUi: false
                )
            );
        }

        [Fact]
        public void ShouldSuppressVanillaScrollWheel_WhenEnabledWheel_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(true);
            Assert.False(
                InputGates.ShouldSuppressVanillaScrollWheel(
                    preciseTrackpad: false,
                    menuOrOverUi: false
                )
            );
        }

        [Fact]
        public void ShouldSuppressVanillaScrollWheel_WhenEnabledOverUi_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(true);
            Assert.False(
                InputGates.ShouldSuppressVanillaScrollWheel(
                    preciseTrackpad: true,
                    menuOrOverUi: true
                )
            );
        }

        [Fact]
        public void ShouldRunVanillaScrollWheel_WhenUnfocused_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(true);
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldRunVanillaScrollWheel());
        }

        [Fact]
        public void ShouldRunVanillaMouseEvents_WhenUnfocused_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(true);
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldRunVanillaMouseEvents(true));
            Assert.False(InputGates.ShouldRunVanillaMouseEvents(false));
        }

        [Fact]
        public void ShouldFlushPendingOrbit_WhenUnfocused_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(true);
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldFlushPendingOrbit());
        }

        [Fact]
        public void ShouldBlockAllCameraInput_WhenUnfocusedAndModOn_ReturnsTrue()
        {
            ModRuntime.SetModActiveForTests(true);
            InputGates.GameFocusedOverride = () => false;
            Assert.True(InputGates.ShouldBlockAllCameraInput());
        }

        [Fact]
        public void ShouldBlockAllCameraInput_WhenModOff_ReturnsFalse()
        {
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldBlockAllCameraInput());
        }

        [Fact]
        public void ShouldRunVanillaScrollWheel_UsesFrameFlags()
        {
            ModRuntime.SetModActiveForTests(true);
            InputGates.GameFocusedOverride = () => true;
            VanillaCameraSuppress.PreciseTrackpadScroll = true;
            VanillaCameraSuppress.MenuOrOverUi = false;
            Assert.False(InputGates.ShouldRunVanillaScrollWheel());

            VanillaCameraSuppress.MenuOrOverUi = true;
            Assert.True(InputGates.ShouldRunVanillaScrollWheel());

            VanillaCameraSuppress.MenuOrOverUi = false;
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            Assert.True(InputGates.ShouldRunVanillaScrollWheel());
        }

        [Fact]
        public void ShouldSuppressVanillaMouseRotate_WhenEnabledAndRotateHeld_ReturnsTrue()
        {
            ModRuntime.SetModActiveForTests(true);
            Assert.True(InputGates.ShouldSuppressVanillaMouseRotate(true));
        }

        [Fact]
        public void ShouldSuppressVanillaMouseRotate_WhenEnabledAndRotateNotHeld_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(true);
            Assert.False(InputGates.ShouldSuppressVanillaMouseRotate(false));
        }

        [Fact]
        public void ShouldSuppressVanillaMouseRotate_WhenDisabledAndRotateHeld_ReturnsFalse()
        {
            ModRuntime.SetModActiveForTests(false);
            Assert.False(InputGates.ShouldSuppressVanillaMouseRotate(true));
        }
    }
}
