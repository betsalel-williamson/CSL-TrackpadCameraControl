using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    public class VanillaCameraKeyLabelsTests
    {
        [Fact]
        public void JoinBindingLabels_DedupesAndSeparates()
        {
            string joined = VanillaCameraKeyLabels.JoinBindingLabels(new[] { "W", "S", "W", "A" });
            Assert.Equal("W · S · A", joined);
        }

        [Fact]
        public void FormatVanillaActionLine_UsesKeymappingPrefix()
        {
            Assert.Equal(
                "Keymapping(s): Middle Mouse · W",
                VanillaCameraKeyLabels.FormatVanillaActionLine("Middle Mouse · W")
            );
            Assert.Equal(
                "Keymapping(s): none",
                VanillaCameraKeyLabels.FormatVanillaActionLine(null)
            );
            Assert.Equal("Keymapping(s): none", VanillaCameraKeyLabels.FormatVanillaActionLine(""));
        }

        [Fact]
        public void FormatGestureLine_UsesGesturePrefix()
        {
            Assert.Equal(
                "Gesture(s): Pinch",
                VanillaCameraKeyLabels.FormatGestureLine(TrackpadGestureCatalog.MapsPlusZoom)
            );
            Assert.Equal(
                "Gesture(s): Option (⌥)+two-finger drag",
                VanillaCameraKeyLabels.FormatGestureLine(TrackpadGestureCatalog.MapsPlusOrbit)
            );
            Assert.Equal(
                "Gesture(s): none",
                VanillaCameraKeyLabels.FormatGestureLine(TrackpadGestureBinding.None)
            );
        }

        [Fact]
        public void FormatGestureLineForOp_FactoryRotate_IsTwoFingerRotate()
        {
            ModSettings settings = ModSettings.CreateFactoryDefaults();
            Assert.Equal(
                "Gesture(s): Two-finger rotate",
                VanillaCameraKeyLabels.FormatGestureLineForOp(settings, CameraOp.Rotate)
            );
        }

        [Fact]
        public void OpDescriptions_AreTwoLines_GestureThenKeymapping()
        {
            string zoom = VanillaCameraKeyLabels.OpDescriptionZoom;
            string[] lines = zoom.Split('\n');
            Assert.Equal(2, lines.Length);
            Assert.StartsWith("Gesture(s):", lines[0]);
            Assert.StartsWith("Keymapping(s):", lines[1]);
            Assert.Equal("Gesture(s): Pinch", lines[0]);
            // Without Cities session, keymapping resolves to none.
            Assert.Equal("Keymapping(s): none", lines[1]);
        }

        [Fact]
        public void OptionsHost_OpDescriptions_DelegateToLiveLabels()
        {
            Assert.Equal(VanillaCameraKeyLabels.OpDescriptionOrbit, OptionsHost.OpDescriptionOrbit);
            Assert.Contains(
                "Gesture(s): Option (⌥)+two-finger drag",
                OptionsHost.OpDescriptionOrbit
            );
        }
    }
}
