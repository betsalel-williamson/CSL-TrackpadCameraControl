using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class ModRuntimeTests
    {
        public ModRuntimeTests()
        {
            ModRuntime.ClearForTests();
        }

        [Fact]
        public void Shutdown_ClearsHarmonyBuffersAndDeactivates()
        {
            var runtime = ModRuntime.CreateForTests(active: true);
            Mod.SetRuntimeForTests(runtime);
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
            var runtime = ModRuntime.CreateForTests();
            Assert.Same(runtime.Pipeline.Camera, runtime.Pipeline.Camera);
        }

        [Fact]
        public void IsModActive_WhenRuntimeMissing_ReturnsFalse()
        {
            ModRuntime.ClearForTests();
            Assert.False(ModRuntime.IsModActive());
        }

        [Fact]
        public void IsModActive_WhenRuntimeActive_ReturnsTrue()
        {
            ModRuntime.SetModActiveForTests(true);
            Assert.True(ModRuntime.IsModActive());
            ModRuntime.ClearForTests();
        }
    }
}
