using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Expected Slow/Fast/Default feel values derived from factory + <see cref="FeelProfiles"/> multipliers.
    /// </summary>
    internal static class FeelExpectation
    {
        public static ModSettings Factory()
        {
            return ModSettings.CreateFactoryDefaults();
        }

        public static float ScaledGain(float factoryGain, float multiplier)
        {
            return ModOptions.RoundGain(factoryGain * multiplier);
        }

        public static void AssertMatchesScaledFactory(ModSettings actual, float multiplier)
        {
            ModSettings factory = Factory();
            Assert.Equal(ScaledGain(factory.PanGainX, multiplier), actual.PanGainX);
            Assert.Equal(ScaledGain(factory.PanGainY, multiplier), actual.PanGainY);
            Assert.Equal(ScaledGain(factory.ZoomGain, multiplier), actual.ZoomGain);
            Assert.Equal(ScaledGain(factory.RotateGain, multiplier), actual.RotateGain);
            Assert.Equal(ScaledGain(factory.OrbitYawGain, multiplier), actual.OrbitYawGain);
            Assert.Equal(ScaledGain(factory.OrbitPitchGain, multiplier), actual.OrbitPitchGain);
            Assert.Equal(factory.SignInvertPanX, actual.SignInvertPanX);
            Assert.Equal(factory.SignInvertPanY, actual.SignInvertPanY);
            Assert.Equal(factory.OrbitPitchMin, actual.OrbitPitchMin);
            Assert.Equal(factory.OrbitPitchMax, actual.OrbitPitchMax);
        }

        public static void AssertMatchesFactoryFeel(ModSettings actual)
        {
            ModSettings factory = Factory();
            Assert.Equal(factory.PanGainX, actual.PanGainX);
            Assert.Equal(factory.PanGainY, actual.PanGainY);
            Assert.Equal(factory.ZoomGain, actual.ZoomGain);
            Assert.Equal(factory.RotateGain, actual.RotateGain);
            Assert.Equal(factory.OrbitYawGain, actual.OrbitYawGain);
            Assert.Equal(factory.OrbitPitchGain, actual.OrbitPitchGain);
            Assert.Equal(factory.SignInvertPanX, actual.SignInvertPanX);
            Assert.Equal(factory.SignInvertPanY, actual.SignInvertPanY);
            Assert.Equal(factory.OrbitPitchMin, actual.OrbitPitchMin);
            Assert.Equal(factory.OrbitPitchMax, actual.OrbitPitchMax);
        }
    }
}
