using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class ScrollUnitMigrationTests
    {
        [Fact]
        public void MigrateScrollUnitIntoSensitivity_FoldsFormerMapperScale()
        {
            var settings = new ModSettings
            {
                PanSensitivityX = 0.50f,
                PanSensitivityY = 0.50f,
                OrbitYawSensitivity = 10.00f,
                OrbitPitchSensitivity = 10.00f,
                MotionDeadzone = 0.001f,
            };

            ModSettingsStore.MigrateScrollUnitIntoSensitivity(settings);

            Assert.Equal(0.005f, settings.PanSensitivityX, 4);
            Assert.Equal(0.005f, settings.PanSensitivityY, 4);
            Assert.Equal(0.10f, settings.OrbitYawSensitivity, 4);
            Assert.Equal(0.10f, settings.OrbitPitchSensitivity, 4);
            Assert.Equal(0.1f, settings.MotionDeadzone, 4);
        }

        [Fact]
        public void FactoryDefaults_MatchFoldedScrollUnit()
        {
            ModSettings factory = ModSettings.CreateFactoryDefaults();
            Assert.Equal(0.005f, factory.PanSensitivityX);
            Assert.Equal(0.005f, factory.PanSensitivityY);
            Assert.Equal(0.10f, factory.OrbitYawSensitivity);
            Assert.Equal(0.10f, factory.OrbitPitchSensitivity);
            Assert.Equal(0.1f, factory.MotionDeadzone);
            Assert.Equal(2, ModSettingsStore.CurrentSchemaVersion);
        }
    }
}
