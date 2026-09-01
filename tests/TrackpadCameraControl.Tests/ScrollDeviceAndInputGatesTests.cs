using System;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    [Collection(VanillaCameraSuppressCollection.Name)]
    public sealed class ScrollDeviceAndInputGatesTests : IDisposable
    {
        public ScrollDeviceAndInputGatesTests()
        {
            ModTestState.Reset();
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
        }

        public void Dispose()
        {
            ModTestState.Reset();
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            VanillaCameraSuppress.MenuOrOverUi = false;
        }

        [Fact]
        public void Mapper_PreciseScroll_MapsToPanDeltas()
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
                    hasPreciseScrollingDeltas: true,
                    out GestureFrame frame
                )
            );
            Assert.Equal(20.0f, frame.centroidDeltaX);
            Assert.Equal(-10.0f, frame.centroidDeltaY);
        }

        [Fact]
        public void Mapper_NonPreciseScroll_DoesNotEmitPanFrame()
        {
            Assert.False(
                AppleGestureMapper.TryMap(
                    AppleGestureMapper.EventTypeScrollWheel,
                    AppleGestureMapper.PhaseChanged,
                    0,
                    20.0,
                    -10.0,
                    0,
                    0f,
                    hasPreciseScrollingDeltas: false,
                    out _
                )
            );
        }

        [Fact]
        public void Mapper_Magnify_IgnoresPreciseFlag()
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
                    hasPreciseScrollingDeltas: false,
                    out GestureFrame frame
                )
            );
            Assert.Equal(0.05f, frame.pinchScaleDelta);
        }

        [Fact]
        public void InputGates_MenuOpen_SkipsModCamera()
        {
            var ui = new FakeGameUiContext { MenuOrOptionsOpen = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                Assert.True(InputGates.ShouldSkipModCamera(new ModSettings()));
            }
        }

        [Fact]
        public void InputGates_IgnoreOverUi_AndPointerOverUi_Skips()
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
        public void InputGates_IgnoreOverUiOff_PointerOverUi_DoesNotSkip()
        {
            var ui = new FakeGameUiContext { PointerOverUi = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                Assert.False(
                    InputGates.ShouldSkipModCamera(new ModSettings { IgnoreOverUi = false })
                );
            }
        }

        [Fact]
        public void InputGates_RequireGameFocus_Unfocused_Skips()
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
        public void InputGates_RequireGameFocusOff_Unfocused_DoesNotSkip()
        {
            var ui = new FakeGameUiContext { GameFocused = false };
            using (new InputGatesContextScope(ui))
            {
                Assert.False(
                    InputGates.ShouldSkipModCamera(new ModSettings { RequireGameFocus = false })
                );
            }
        }

        [Fact]
        public void InputGates_WorldFocused_DoesNotSkip()
        {
            using (new InputGatesContextScope(new FakeGameUiContext()))
            {
                Assert.False(InputGates.ShouldSkipModCamera(new ModSettings()));
            }
        }

        [Fact]
        public void Pipeline_WhenGateSkips_DoesNotApplyCamera()
        {
            var ui = new FakeGameUiContext { MenuOrOptionsOpen = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                RunPipelinePanAssert(staysStill: true);
            }
        }

        [Fact]
        public void Pipeline_WhenGateAllows_AppliesCamera()
        {
            using (new InputGatesContextScope(new FakeGameUiContext()))
            {
                RunPipelinePanAssert(staysStill: false);
            }
        }

        [Fact]
        public void Pipeline_Tick_UpdatesSuppressMenuOrOverUiFromGates()
        {
            var ui = new FakeGameUiContext { MenuOrOptionsOpen = true, GameFocused = true };
            using (new InputGatesContextScope(ui))
            {
                var inject = new InjectGestureSource();
                var pipeline = new GesturePipeline(
                    new ModSettings(),
                    inject,
                    new FakeCameraController()
                );
                pipeline.Tick();

                Assert.True(VanillaCameraSuppress.MenuOrOverUi);
            }
        }

        private static void RunPipelinePanAssert(bool staysStill)
        {
            var settings = new ModSettings
            {
                PanEnabled = true,
                MotionDeadband = 0.001f,
                PanGainX = 1f,
                PanGainY = 1f,
            };
            var inject = new InjectGestureSource();
            var cam = new FakeCameraController
            {
                Size = 1f,
                TargetX = 0f,
                TargetZ = 0f,
            };
            var pipeline = new GesturePipeline(settings, inject, cam);

            inject.Enqueue(
                new GestureFrame
                {
                    magic = GestureFrame.Magic,
                    version = GestureFrame.Version,
                    fingerCount = 2,
                    phase = (int)GesturePhase.Changed,
                    centroidDeltaX = 0.2f,
                }
            );

            pipeline.Tick();

            if (staysStill)
            {
                Assert.Equal(0f, cam.TargetX);
                Assert.Equal(0f, cam.TargetZ);
            }
            else
            {
                Assert.True(cam.TargetX != 0f || cam.TargetZ != 0f);
            }
        }
    }
}
