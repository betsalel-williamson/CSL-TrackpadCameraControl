using System.IO;
using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class EngineeringNamesMigrationTests
    {
        [Fact]
        public void LoadOrFactory_MigratesSchema2SensitivityXmlToGain()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "tcc-eng-" + Path.GetRandomFileName() + ".xml"
            );
            try
            {
                File.WriteAllText(
                    path,
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<TrackpadCameraControlSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SchemaVersion>2</SchemaVersion>
  <Current>
    <PanSensitivityX>0.0075</PanSensitivityX>
    <PanSensitivityY>0.004</PanSensitivityY>
    <OrbitYawSensitivity>0.12</OrbitYawSensitivity>
    <OrbitPitchSensitivity>0.11</OrbitPitchSensitivity>
    <ZoomSensitivity>1.5</ZoomSensitivity>
    <YawRotateSensitivity>2.5</YawRotateSensitivity>
    <PanButtonScaleX>0.07</PanButtonScaleX>
    <MotionDeadzone>0.2</MotionDeadzone>
    <InvertPanX>false</InvertPanX>
    <InvertPanY>true</InvertPanY>
    <PanLowPassEnabled>true</PanLowPassEnabled>
    <PanLowPassAlpha>0.4</PanLowPassAlpha>
    <ActiveFeelPresetName>Default</ActiveFeelPresetName>
  </Current>
  <UserPresets>
    <Preset>
      <Name>MyFeel</Name>
      <Settings>
        <PanSensitivityX>0.009</PanSensitivityX>
        <InvertPanX>true</InvertPanX>
      </Settings>
    </Preset>
  </UserPresets>
</TrackpadCameraControlSettings>"
                );

                var store = new ModSettingsStore(path);
                ModSettings loaded = store.LoadOrFactory();

                Assert.Equal(0.0075f, loaded.PanGainX);
                Assert.Equal(0.004f, loaded.PanGainY);
                Assert.Equal(0.12f, loaded.OrbitYawGain);
                Assert.Equal(0.11f, loaded.OrbitPitchGain);
                Assert.Equal(1.5f, loaded.ZoomGain);
                Assert.Equal(2.5f, loaded.YawRotateGain);
                Assert.Equal(0.07f, loaded.PanStepX);
                Assert.Equal(0.2f, loaded.MotionDeadband);
                Assert.False(loaded.SignInvertPanX);
                Assert.True(loaded.SignInvertPanY);
                Assert.True(loaded.PanFilterEnabled);
                Assert.Equal(0.4f, loaded.PanFilterAlpha);

                Assert.True(store.TryGetUserPreset("MyFeel", out ModSettings preset));
                Assert.Equal(0.009f, preset.PanGainX);
                Assert.True(preset.SignInvertPanX);

                string rewritten = File.ReadAllText(path);
                Assert.Contains(
                    "<SchemaVersion>" + ModSettingsStore.CurrentSchemaVersion + "</SchemaVersion>",
                    rewritten
                );
                Assert.Contains("<PanGainX>", rewritten);
                Assert.DoesNotContain("<PanSensitivityX>", rewritten);
                Assert.Contains("<MotionDeadband>", rewritten);
                Assert.DoesNotContain("<MotionDeadzone>", rewritten);
                Assert.Contains("<SignInvertPanY>", rewritten);
                Assert.Contains("<PanFilterEnabled>", rewritten);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void LoadOrFactory_LegacyWithoutAssistUiEnabled_DefaultsTrueForMigration()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "tcc-legacy-assist-" + Path.GetRandomFileName() + ".xml"
            );
            try
            {
                File.WriteAllText(
                    path,
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<TrackpadCameraControlSettings>
  <SchemaVersion>2</SchemaVersion>
  <Current>
    <PanSensitivityX>0.005</PanSensitivityX>
    <ActiveFeelPresetName>Default</ActiveFeelPresetName>
  </Current>
</TrackpadCameraControlSettings>"
                );

                var store = new ModSettingsStore(path);
                ModSettings loaded = store.LoadOrFactory();
                Assert.True(loaded.AssistUiEnabled);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void LoadOrFactory_MigratesSchema1ScrollUnitThenEngineeringNames()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "tcc-s1-" + Path.GetRandomFileName() + ".xml"
            );
            try
            {
                File.WriteAllText(
                    path,
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<TrackpadCameraControlSettings>
  <SchemaVersion>1</SchemaVersion>
  <Current>
    <PanSensitivityX>0.50</PanSensitivityX>
    <PanSensitivityY>0.50</PanSensitivityY>
    <OrbitYawSensitivity>100.00</OrbitYawSensitivity>
    <OrbitPitchSensitivity>100.00</OrbitPitchSensitivity>
    <MotionDeadzone>0.00001</MotionDeadzone>
  </Current>
</TrackpadCameraControlSettings>"
                );

                var store = new ModSettingsStore(path);
                ModSettings loaded = store.LoadOrFactory();
                ModSettings factory = ModSettings.CreateFactoryDefaults();

                Assert.Equal(factory.PanGainX, loaded.PanGainX, 4);
                Assert.Equal(factory.PanGainY, loaded.PanGainY, 4);
                Assert.Equal(factory.OrbitYawGain, loaded.OrbitYawGain, 4);
                Assert.Equal(factory.OrbitPitchGain, loaded.OrbitPitchGain, 4);
                Assert.Equal(factory.MotionDeadband, loaded.MotionDeadband, 4);
                Assert.Contains(
                    "<SchemaVersion>" + ModSettingsStore.CurrentSchemaVersion + "</SchemaVersion>",
                    File.ReadAllText(path)
                );
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void LoadOrFactory_MigratesSchema5PinchEpsilonXmlToDeadbandNames()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "tcc-deadband-" + Path.GetRandomFileName() + ".xml"
            );
            try
            {
                File.WriteAllText(
                    path,
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<TrackpadCameraControlSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SchemaVersion>5</SchemaVersion>
  <Current>
    <PanGainX>0.005</PanGainX>
    <PinchEpsilon>0.042</PinchEpsilon>
    <RotateEpsilon>0.017</RotateEpsilon>
    <MotionDeadband>0.003</MotionDeadband>
    <ActiveFeelPresetName>Default</ActiveFeelPresetName>
  </Current>
</TrackpadCameraControlSettings>"
                );

                var store = new ModSettingsStore(path);
                ModSettings loaded = store.LoadOrFactory();

                Assert.Equal(0.042f, loaded.PinchDeadband);
                Assert.Equal(0.017f, loaded.YawDeadband);
                Assert.Equal(0.003f, loaded.MotionDeadband);

                string rewritten = File.ReadAllText(path);
                Assert.Contains(
                    "<SchemaVersion>" + ModSettingsStore.CurrentSchemaVersion + "</SchemaVersion>",
                    rewritten
                );
                Assert.Contains("<PinchDeadband>0.042</PinchDeadband>", rewritten);
                Assert.Contains("<YawDeadband>0.017</YawDeadband>", rewritten);
                Assert.DoesNotContain("<PinchEpsilon>", rewritten);
                Assert.DoesNotContain("<RotateEpsilon>", rewritten);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
