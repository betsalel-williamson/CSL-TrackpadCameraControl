using System;
using TrackpadCameraControl.Gestures;
using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    public class NoopGestureSourceTests
    {
        [Fact]
        public void Noop_NeverConnectsOrDequeues()
        {
            var source = new NoopGestureSource();
            Assert.False(source.IsConnected);
            source.Connect();
            Assert.False(source.IsConnected);
            Assert.False(source.TryDequeue(out _));
            source.Disconnect();
            Assert.False(source.IsConnected);
        }

        [Fact]
        public void CreateFailSoftCaptureSource_IsNoop()
        {
            IGestureSource source = GesturePipeline.CreateFailSoftCaptureSource();
            Assert.IsType<NoopGestureSource>(source);
            Assert.False(source.IsConnected);
        }

        [Fact]
        public void IsAppKitAvailable_OnMac_DoesNotRequireAppKitBinaryFile()
        {
            // Regression: File.Exists(…/AppKit) is false under dyld shared cache.
            if (!GesturePipeline.IsAppKitAvailable())
            {
                return;
            }

            Assert.True(
                System.IO.Directory.Exists("/System/Library/Frameworks/AppKit.framework")
                    || Environment.OSVersion.Platform == PlatformID.MacOSX
            );
            IGestureSource source = GesturePipeline.CreateDefaultCaptureSource();
            Assert.IsType<AppleGestureSource>(source);
        }

        [Fact]
        public void CreateDefaultCaptureSource_UsesNoopWhenAppKitMissing()
        {
            if (GesturePipeline.IsAppKitAvailable())
            {
                IGestureSource source = GesturePipeline.CreateDefaultCaptureSource();
                Assert.IsType<AppleGestureSource>(source);
                return;
            }

            IGestureSource noop = GesturePipeline.CreateDefaultCaptureSource();
            Assert.IsType<NoopGestureSource>(noop);
        }
    }
}
