using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class SelectionGesturePriorityTests
    {
        [Fact]
        public void Relocate_BeatsPlacementGhost()
        {
            Assert.Equal(
                SelectionGestureKind.RelocateInstance,
                SelectionGesturePriority.Resolve(
                    placementToolArmed: true,
                    relocateBuildingId: 42,
                    hasValidSelectedInstance: true
                )
            );
        }

        [Fact]
        public void Relocate_BeatsSelectedInstance_EvenWhenBothPresent()
        {
            // Ensures twist during relocate is classified as RelocateInstance (ghost angle),
            // not SelectedInstance (which would spin the old-cell buffer building).
            Assert.Equal(
                SelectionGestureKind.RelocateInstance,
                SelectionGesturePriority.Resolve(
                    placementToolArmed: true,
                    relocateBuildingId: 7,
                    hasValidSelectedInstance: true
                )
            );
        }

        [Fact]
        public void SelectedInstance_BeatsPlacementGhost_WhenNotRelocating()
        {
            Assert.Equal(
                SelectionGestureKind.SelectedInstance,
                SelectionGesturePriority.Resolve(
                    placementToolArmed: true,
                    relocateBuildingId: 0,
                    hasValidSelectedInstance: true
                )
            );
        }

        [Fact]
        public void PlacementGhost_WhenArmedWithoutSelection()
        {
            Assert.Equal(
                SelectionGestureKind.PlacementGhost,
                SelectionGesturePriority.Resolve(
                    placementToolArmed: true,
                    relocateBuildingId: 0,
                    hasValidSelectedInstance: false
                )
            );
        }

        [Fact]
        public void SelectedInstance_WithoutPlacementTool()
        {
            Assert.Equal(
                SelectionGestureKind.SelectedInstance,
                SelectionGesturePriority.Resolve(
                    placementToolArmed: false,
                    relocateBuildingId: 0,
                    hasValidSelectedInstance: true
                )
            );
        }

        [Fact]
        public void None_WhenNothingArmed()
        {
            Assert.Equal(
                SelectionGestureKind.None,
                SelectionGesturePriority.Resolve(
                    placementToolArmed: false,
                    relocateBuildingId: 0,
                    hasValidSelectedInstance: false
                )
            );
        }

        [Theory]
        [InlineData(SelectionGestureKind.RelocateInstance, true)]
        [InlineData(SelectionGestureKind.PlacementGhost, true)]
        [InlineData(SelectionGestureKind.SelectedInstance, false)]
        [InlineData(SelectionGestureKind.None, false)]
        public void AllowsObjectYaw_OnlyGhostModes(SelectionGestureKind kind, bool expected)
        {
            Assert.Equal(expected, SelectionGesturePriority.AllowsObjectYaw(kind));
        }
    }
}
