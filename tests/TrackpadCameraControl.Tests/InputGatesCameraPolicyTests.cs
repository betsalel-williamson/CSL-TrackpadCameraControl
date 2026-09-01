using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class InputGatesCameraPolicyTests : System.IDisposable
    {
        private ModTestHarness _harness;

        public InputGatesCameraPolicyTests()
        {
            ResetState();
        }

        public void Dispose()
        {
            ResetState();
        }

        private void ResetState()
        {
            _harness?.Dispose();
            _harness = null;
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
            InputGates.ResetTestHooks();
        }

        private void EnableMod()
        {
            _harness?.Dispose();
            _harness = new ModTestHarness();
        }

        [Fact]
        public void ShouldSuppressVanillaScrollWheel_WhenDisabled_ReturnsFalse()
        {
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
            EnableMod();
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
            EnableMod();
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
            EnableMod();
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
            EnableMod();
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldRunVanillaScrollWheel());
        }

        [Fact]
        public void ShouldRunVanillaMouseEvents_WhenUnfocused_ReturnsFalse()
        {
            EnableMod();
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldRunVanillaMouseEvents(true));
            Assert.False(InputGates.ShouldRunVanillaMouseEvents(false));
        }

        [Fact]
        public void ShouldFlushPendingOrbit_WhenUnfocused_ReturnsFalse()
        {
            EnableMod();
            InputGates.GameFocusedOverride = () => false;
            Assert.False(InputGates.ShouldFlushPendingOrbit());
        }

        [Fact]
        public void ShouldBlockAllCameraInput_WhenUnfocusedAndModOn_ReturnsTrue()
        {
            EnableMod();
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
            EnableMod();
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
            EnableMod();
            Assert.True(InputGates.ShouldSuppressVanillaMouseRotate(true));
        }

        [Fact]
        public void ShouldSuppressVanillaMouseRotate_WhenEnabledAndRotateNotHeld_ReturnsFalse()
        {
            EnableMod();
            Assert.False(InputGates.ShouldSuppressVanillaMouseRotate(false));
        }

        [Fact]
        public void ShouldSuppressVanillaMouseRotate_WhenDisabledAndRotateHeld_ReturnsFalse()
        {
            Assert.False(InputGates.ShouldSuppressVanillaMouseRotate(true));
        }
    }
}
