using System;
using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>
    /// InputGates with <see cref="FakeGameUiContext"/> — menu / over-UI / focus skip.
    /// Does not claim Harmony order or AppKit capture timing (tier C).
    /// </summary>
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class InputGatesTests : IDisposable
    {
        private ModTestHarness _harness;

        public InputGatesTests()
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
            ModTestState.Reset();
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
        }

        [Fact]
        public void ShouldSkipModCamera_WhenMenuOpen_ReturnsTrue()
        {
            var ui = new FakeGameUiContext { MenuOrOptionsOpen = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                Assert.True(InputGates.ShouldSkipModCamera(new ModSettings()));
            }
        }

        [Fact]
        public void ShouldSkipModCamera_WhenIgnoreOverUiAndPointerOverUi_ReturnsTrue()
        {
            var ui = new FakeGameUiContext { PointerOverUi = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                Assert.True(
                    InputGates.ShouldSkipModCamera(new ModSettings { IgnoreOverUi = true })
                );
            }
        }

        [Fact]
        public void ShouldSkipModCamera_WhenUnfocusedAndRequireGameFocus_ReturnsTrue()
        {
            var ui = new FakeGameUiContext { GameFocused = false };
            using (new InputGatesContextScope(ui))
            {
                Assert.True(
                    InputGates.ShouldSkipModCamera(new ModSettings { RequireGameFocus = true })
                );
            }
        }

        [Fact]
        public void ShouldSkipModCamera_WhenWorldFocused_ReturnsFalse()
        {
            var ui = new FakeGameUiContext { GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                Assert.False(InputGates.ShouldSkipModCamera(new ModSettings()));
            }
        }

        [Fact]
        public void Pipeline_WhenOverUi_DoesNotApplyPan()
        {
            var settings = new ModSettings();
            settings.ApplyPreset(GesturePreset.MapsPlus);
            settings.IgnoreOverUi = true;
            settings.PanGainX = 1f;
            settings.PanGainY = 1f;
            settings.MotionDeadband = 0.001f;

            var inject = new InjectGestureSource();
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
                AngleX = 0f,
            };
            var pipeline = new GesturePipeline(settings, inject, cam);

            var ui = new FakeGameUiContext { PointerOverUi = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                inject.Enqueue(
                    new GestureFrame
                    {
                        magic = GestureFrame.Magic,
                        version = GestureFrame.Version,
                        fingerCount = 2,
                        phase = (int)GesturePhase.Changed,
                        centroidDeltaX = 0.1f,
                    }
                );
                pipeline.Tick();
            }

            Assert.Equal(0f, cam.TargetX, 3);
            Assert.Equal(0f, cam.TargetZ, 3);
        }

        [Fact]
        public void ShouldBlockAllCameraInput_WhenUnfocusedAndModOn_ReturnsTrue()
        {
            _harness = new ModTestHarness();
            var ui = new FakeGameUiContext { GameFocused = false };
            using (new InputGatesContextScope(ui))
            {
                Assert.True(InputGates.ShouldBlockAllCameraInput());
            }
        }

        [Fact]
        public void ShouldBlockAllCameraInput_WhenModOff_ReturnsFalse()
        {
            var ui = new FakeGameUiContext { GameFocused = false };
            using (new InputGatesContextScope(ui))
            {
                Assert.False(InputGates.ShouldBlockAllCameraInput());
            }
        }
    }

    [CollectionDefinition(VanillaCameraSuppressCollection.Name, DisableParallelization = true)]
    public class VanillaCameraSuppressCollection
    {
        public const string Name = "VanillaCameraSuppress.Rewrite";
    }
}
