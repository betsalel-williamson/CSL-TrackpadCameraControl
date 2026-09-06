using TrackpadCameraControl.Rewrite;
using Xunit;

namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>
    /// Selection-port coverage: FeelMath rotate prefers object yaw when selection accepts it.
    /// </summary>
    public class SelectionPortGoldenTests
    {
        [Fact]
        public void MapsPlus_Rotate_WithSelection_AppliesObjectYaw_NotCamera()
        {
            var settings = new ModSettings { RotateGain = 2f };
            var cam = new FakeCameraController { AngleX = 40f };
            var selection = new FakeSelectionContext { HasSelection = true };

            FeelMath.Apply(
                CameraOp.Rotate,
                0f,
                0f,
                0f,
                0.5f,
                settings,
                cam,
                FeelMath.InputModality.Drag,
                selection
            );

            Assert.Equal(40f, cam.AngleX, 3);
            Assert.Equal(1f, selection.AppliedYawDegrees, 3);
            Assert.Equal(1, selection.RotateCalls);
        }

        [Fact]
        public void MapsPlus_Rotate_WithoutSelection_YawsCamera()
        {
            var settings = new ModSettings { RotateGain = 2f };
            var cam = new FakeCameraController { AngleX = 10f };
            var selection = new FakeSelectionContext { HasSelection = false };

            FeelMath.Apply(
                CameraOp.Rotate,
                0f,
                0f,
                0f,
                0.5f,
                settings,
                cam,
                FeelMath.InputModality.Drag,
                selection
            );

            Assert.Equal(11f, cam.AngleX, 3);
            Assert.Equal(0, selection.RotateCalls);
        }
    }
}
