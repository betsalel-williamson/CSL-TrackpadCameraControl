using System.Collections.Generic;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class CinematicCameraInputTests
    {
        private static readonly int[] SampleKeys = { 1, 2, 999 };

        [Fact]
        public void ShouldAbort_ModifierOnlyInput_DoesNotAbort()
        {
            var down = new HashSet<int> { 310 };

            Assert.False(
                CinematicCameraInput.ShouldAbortNonInteractivePlayback(
                    true,
                    false,
                    down.Contains,
                    SampleKeys
                )
            );
        }

        [Fact]
        public void ShouldAbort_NonModifierKey_Aborts()
        {
            var down = new HashSet<int> { 999 };

            Assert.True(
                CinematicCameraInput.ShouldAbortNonInteractivePlayback(
                    true,
                    false,
                    down.Contains,
                    SampleKeys
                )
            );
        }

        [Fact]
        public void ShouldAbort_ShortcutPressed_DoesNotAbort()
        {
            var down = new HashSet<int> { 999 };

            Assert.False(
                CinematicCameraInput.ShouldAbortNonInteractivePlayback(
                    true,
                    true,
                    down.Contains,
                    SampleKeys
                )
            );
        }

        [Fact]
        public void ShouldAbort_NoKeys_DoesNotAbort()
        {
            Assert.False(
                CinematicCameraInput.ShouldAbortNonInteractivePlayback(
                    false,
                    false,
                    _ => false,
                    SampleKeys
                )
            );
        }

        [Fact]
        public void IsModifierKey_RecognizesCommand()
        {
            Assert.True(CinematicCameraInput.IsModifierKey(310));
            Assert.False(CinematicCameraInput.IsModifierKey(999));
        }
    }
}
