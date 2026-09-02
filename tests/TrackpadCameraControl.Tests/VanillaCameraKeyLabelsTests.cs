using Xunit;

namespace TrackpadCameraControl.Tests
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
        public void FormatGestureLineForOp_BothOrbit_JoinsMapsPlusAndCad()
        {
            var settings = ModSettings.CreateFactoryDefaults();
            settings.OrbitTrigger = OrbitTrigger.Both;
            string line = VanillaCameraKeyLabels.FormatGestureLineForOp(settings, CameraOp.Orbit);
            Assert.Equal("Gesture(s): Option (⌥)+two-finger drag · Three-finger drag", line);
        }
    }

    public class TrackpadGestureCatalogTests
    {
        [Fact]
        public void FactoryDefaults_AreMapsPlusBindings()
        {
            ModSettings s = ModSettings.CreateFactoryDefaults();
            Assert.Equal(TrackpadGesture.Pinch, s.ZoomGesture);
            Assert.Equal(GestureModifierKey.None, s.ZoomGestureModifier);
            Assert.Equal(TrackpadGesture.TwoFingerDrag, s.PanGesture);
            Assert.Equal(TrackpadGesture.TwoFingerRotate, s.RotateGesture);
            Assert.Equal(TrackpadGesture.TwoFingerDrag, s.OrbitGesture);
            Assert.Equal(GestureModifierKey.Option, s.OrbitGestureModifier);
            Assert.Equal(OrbitTrigger.ModifierPlusTwoFinger, s.OrbitTrigger);
        }

        [Fact]
        public void ApplyFeelDefault_DoesNotRewriteGestureStyleBindings()
        {
            ModSettings s = ModSettings.CreateFactoryDefaults();
            s.ApplyGesturePreset(GesturePreset.CAD);
            ModOptions.ApplyFeelDefault(s);
            Assert.Equal(GesturePreset.CAD, s.GesturePreset);
            Assert.Equal(TrackpadGesture.ThreeFingerDrag, s.OrbitGesture);
            Assert.Equal(GestureModifierKey.None, s.OrbitGestureModifier);
            Assert.Equal(OrbitTrigger.ThreeFinger, s.OrbitTrigger);
            Assert.Equal(FeelProfiles.NameDefault, s.ActiveFeelPresetName);
        }

        [Fact]
        public void ApplyPreset_Cad_UpdatesOrbitPairAndTrigger()
        {
            ModSettings s = ModSettings.CreateFactoryDefaults();
            s.ApplyGesturePreset(GesturePreset.CAD);
            Assert.Equal(GesturePreset.CAD, s.GesturePreset);
            Assert.Equal(TrackpadGesture.ThreeFingerDrag, s.OrbitGesture);
            Assert.Equal(GestureModifierKey.None, s.OrbitGestureModifier);
            Assert.Equal(OrbitTrigger.ThreeFinger, s.OrbitTrigger);
            Assert.Equal(TrackpadGesture.Pinch, s.ZoomGesture);
        }

        [Fact]
        public void ApplyPreset_MapsPlus_RestoresOptionOrbit()
        {
            ModSettings s = ModSettings.CreateFactoryDefaults();
            s.ApplyGesturePreset(GesturePreset.CAD);
            s.ApplyGesturePreset(GesturePreset.MapsPlus);
            Assert.Equal(TrackpadGesture.TwoFingerDrag, s.OrbitGesture);
            Assert.Equal(GestureModifierKey.Option, s.OrbitGestureModifier);
            Assert.Equal(OrbitTrigger.ModifierPlusTwoFinger, s.OrbitTrigger);
        }

        [Fact]
        public void CopyFrom_CopiesGestureBindings()
        {
            var source = ModSettings.CreateFactoryDefaults();
            source.ApplyGesturePreset(GesturePreset.CAD);
            var dest = ModSettings.CreateFactoryDefaults();
            dest.CopyFrom(source);
            Assert.Equal(TrackpadGesture.ThreeFingerDrag, dest.OrbitGesture);
            Assert.Equal(GestureModifierKey.None, dest.OrbitGestureModifier);
        }
    }
}
