using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class ModRuntimeTests
    {
        [Fact]
        public void Shutdown_ClearsHarmonyBuffersAndDeactivates()
        {
            var runtime = new ModRuntime(new ModSettings(), new InProcessGestureSource());
            VanillaCameraSuppress.PreciseTrackpadScroll = true;
            VanillaCameraSuppress.MenuOrOverUi = true;

            runtime.Shutdown();

            Assert.False(runtime.IsActive);
            Assert.False(VanillaCameraSuppress.PreciseTrackpadScroll);
            Assert.False(VanillaCameraSuppress.MenuOrOverUi);
        }

        [Fact]
        public void Pipeline_UsesSingleCameraInstance()
        {
            var runtime = new ModRuntime(new ModSettings(), new InProcessGestureSource());
            Assert.Same(runtime.Pipeline.Camera, runtime.Pipeline.Camera);
        }

        [Fact]
        public void IsModActive_WhenRuntimeMissing_ReturnsFalse()
        {
            var mod = new Mod();
            mod.OnDisabled();
            Assert.False(ModRuntime.IsModActive());
        }

        [Fact]
        public void IsModActive_WhenModEnabled_ReturnsTrue()
        {
            using (new ModTestHarness())
            {
                Assert.True(ModRuntime.IsModActive());
            }
        }
    }
}
