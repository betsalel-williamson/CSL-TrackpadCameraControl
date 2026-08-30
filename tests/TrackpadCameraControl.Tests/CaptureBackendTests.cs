using System;
using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class CaptureBackendFlagsTests
    {
        [Fact]
        public void Resolve_DefaultSettings_IsAppleGestures()
        {
            Assert.Equal(
                CaptureBackend.AppleGestures,
                CaptureBackendFlags.Resolve(new ModSettings(), null)
            );
        }

        [Fact]
        public void Resolve_EnvApple_OverridesSettings()
        {
            var settings = new ModSettings { CaptureBackend = CaptureBackend.Contacts };
            Assert.Equal(
                CaptureBackend.AppleGestures,
                CaptureBackendFlags.Resolve(settings, "apple")
            );
        }

        [Fact]
        public void Resolve_EnvContacts_OverridesAppleSettings()
        {
            var settings = new ModSettings { CaptureBackend = CaptureBackend.AppleGestures };
            Assert.Equal(
                CaptureBackend.Contacts,
                CaptureBackendFlags.Resolve(settings, "contacts")
            );
        }

        [Fact]
        public void Resolve_ContactsFlagOff_ForcesAppleEvenIfSettingsContacts()
        {
            Assert.False(FeatureFlags.EnableContactsCapture);
            var settings = new ModSettings { CaptureBackend = CaptureBackend.Contacts };
            Assert.Equal(
                CaptureBackend.AppleGestures,
                CaptureBackendFlags.Resolve(settings, null)
            );
        }

        [Theory]
        [InlineData("apple")]
        [InlineData("Apple")]
        [InlineData("applegestures")]
        [InlineData("appkit")]
        public void TryParse_AppleAliases(string value)
        {
            Assert.True(CaptureBackendFlags.TryParse(value, out CaptureBackend backend));
            Assert.Equal(CaptureBackend.AppleGestures, backend);
        }
    }

    public class AppleGestureMapperTests
    {
        [Fact]
        public void Scroll_MapsToTwoFingerCentroid()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    20.0,
                    -10.0,
                    0,
                    0f,
                    out GestureFrame frame
                )
            );
            Assert.Equal(2, frame.fingerCount);
            Assert.Equal((int)GesturePhase.Changed, frame.phase);
            Assert.Equal(20.0f, frame.centroidDeltaX);
            Assert.Equal(-10.0f, frame.centroidDeltaY);
            Assert.Equal(0f, frame.pinchScaleDelta);
        }

        [Fact]
        public void Scroll_Option_SetsModifierBit()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    AppleGestureMapper.FlagMaskAlternate,
                    1.0,
                    0,
                    0,
                    0f,
                    out GestureFrame frame
                )
            );
            Assert.Equal((uint)GestureModifiers.Option, frame.modifiers);
        }

        [Fact]
        public void Magnify_MapsToPinchScale()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeMagnify,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    0,
                    0,
                    0.05,
                    0f,
                    out GestureFrame frame
                )
            );
            Assert.Equal(2, frame.fingerCount);
            Assert.Equal(0.05f, frame.pinchScaleDelta);
        }

        [Fact]
        public void Rotate_MapsDegreesToRotateDelta()
        {
            Assert.True(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeRotate,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    0,
                    0,
                    0,
                    12.5f,
                    out GestureFrame frame
                )
            );
            Assert.Equal(12.5f, frame.rotateDelta);
        }

        [Fact]
        public void Swipe_IsIgnored()
        {
            Assert.False(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeSwipe,
                    AppleGestureMapper.PhaseEnded,
                    0,
                    0,
                    0,
                    0,
                    0f,
                    out _
                )
            );
        }
    }

    public class InProcessCaptureTests
    {
        [Fact]
        public void DefaultSettings_BridgeDisabled()
        {
            Assert.False(new ModSettings().BridgeEnabled);
        }

        [Fact]
        public void CreateCaptureSource_Default_UsesAppleGesturesBackend()
        {
            Assert.Equal(
                CaptureBackend.AppleGestures,
                CaptureBackendFlags.Resolve(new ModSettings(), null)
            );
            Assert.NotNull(Mod.CreateCaptureSource(new ModSettings()));
        }

        [Fact]
        public void CreateCaptureSource_ContactsSettings_WhenFlagOff_StillResolvesApple()
        {
            Assert.False(FeatureFlags.EnableContactsCapture);
            var settings = new ModSettings
            {
                CaptureBackend = CaptureBackend.Contacts,
                BridgeEnabled = true,
            };
            Assert.Equal(CaptureBackend.AppleGestures, CaptureBackendFlags.Resolve(settings, null));
            Assert.NotNull(Mod.CreateCaptureSource(settings));
        }
    }

    public class GestureCaptureLogTests
    {
        [Fact]
        public void Line_WritesToConfiguredPath()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "trackpad-capture-log-test-" + Guid.NewGuid().ToString("N") + ".log"
            );
            GestureCaptureLog.PathOverride = path;
            GestureCaptureLog.ResetForTests();
            try
            {
                GestureCaptureLog.Line("hello-capture");
                GestureCaptureLog.ResetForTests();
                string text = File.ReadAllText(path);
                Assert.Contains("hello-capture", text);
                Assert.Contains(GestureCaptureLog.OpenedLinePrefix, text);
            }
            finally
            {
                GestureCaptureLog.ResetForTests();
                GestureCaptureLog.PathOverride = null;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
