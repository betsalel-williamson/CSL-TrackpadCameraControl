using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class ScrollUnitMigrationTests
    {
        [Fact]
        public void MigrateScrollUnitIntoGain_FoldsFormerMapperScale()
        {
            ModSettings factory = ModSettings.CreateFactoryDefaults();
            var settings = new ModSettings
            {
                PanGainX = factory.PanGainX / ModSettingsStore.V1ScrollUnit,
                PanGainY = factory.PanGainY / ModSettingsStore.V1ScrollUnit,
                OrbitYawGain = factory.OrbitYawGain / ModSettingsStore.V1ScrollUnit,
                OrbitPitchGain = factory.OrbitPitchGain / ModSettingsStore.V1ScrollUnit,
                MotionDeadband = factory.MotionDeadband * ModSettingsStore.V1ScrollUnit,
            };

            ModSettingsStore.MigrateScrollUnitIntoGain(settings);

            Assert.Equal(factory.PanGainX, settings.PanGainX, 4);
            Assert.Equal(factory.PanGainY, settings.PanGainY, 4);
            Assert.Equal(factory.OrbitYawGain, settings.OrbitYawGain, 4);
            Assert.Equal(factory.OrbitPitchGain, settings.OrbitPitchGain, 4);
            Assert.Equal(factory.MotionDeadband, settings.MotionDeadband, 4);
        }

        [Fact]
        public void FactoryDefaults_MatchFoldedScrollUnit()
        {
            ModSettings factory = ModSettings.CreateFactoryDefaults();
            var preFold = new ModSettings
            {
                PanGainX = factory.PanGainX / ModSettingsStore.V1ScrollUnit,
                PanGainY = factory.PanGainY / ModSettingsStore.V1ScrollUnit,
                OrbitYawGain = factory.OrbitYawGain / ModSettingsStore.V1ScrollUnit,
                OrbitPitchGain = factory.OrbitPitchGain / ModSettingsStore.V1ScrollUnit,
                MotionDeadband = factory.MotionDeadband * ModSettingsStore.V1ScrollUnit,
            };
            ModSettingsStore.MigrateScrollUnitIntoGain(preFold);

            Assert.Equal(preFold.PanGainX, factory.PanGainX, 4);
            Assert.Equal(preFold.PanGainY, factory.PanGainY, 4);
            Assert.Equal(preFold.OrbitYawGain, factory.OrbitYawGain, 4);
            Assert.Equal(preFold.OrbitPitchGain, factory.OrbitPitchGain, 4);
            Assert.Equal(preFold.MotionDeadband, factory.MotionDeadband, 4);
            Assert.Equal(7, ModSettingsStore.CurrentSchemaVersion);
        }
    }
}
