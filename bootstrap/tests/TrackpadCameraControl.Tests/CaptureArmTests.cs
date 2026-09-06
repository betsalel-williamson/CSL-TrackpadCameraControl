using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public sealed class CaptureArmTests
    {
        [Fact]
        public void ModRuntime_Constructor_DoesNotConnectSource()
        {
            var source = new RecordingGestureSource();
            _ = new ModRuntime(new ModSettings(), source);
            Assert.Equal(0, source.ConnectCount);
            Assert.Equal(0, source.DisconnectCount);
        }

        [Fact]
        public void GesturePipeline_ArmCapture_DisconnectsThenConnects()
        {
            var source = new RecordingGestureSource();
            var pipeline = new GesturePipeline(new ModSettings(), source);

            pipeline.ArmCapture();

            Assert.Equal(1, source.DisconnectCount);
            Assert.Equal(1, source.ConnectCount);
            Assert.True(source.IsConnected);
        }

        [Fact]
        public void ArmCaptureOnLevelLoaded_WhenRuntimePresent_ReArmsPipeline()
        {
            var source = new RecordingGestureSource();
            var mod = new Mod();
            mod.OnEnabled();
            try
            {
                Mod.Runtime.Pipeline.SetSource(source);
                Mod.ArmCaptureOnLevelLoaded();

                Assert.Equal(1, source.DisconnectCount);
                Assert.Equal(1, source.ConnectCount);
            }
            finally
            {
                mod.OnDisabled();
            }
        }
    }
}
