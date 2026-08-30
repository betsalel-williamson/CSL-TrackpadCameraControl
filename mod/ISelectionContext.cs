namespace TrackpadCameraControl
{
    /// <summary>
    /// Seam for "is a placeable / city object selected?" used by selection-aware
    /// yaw (object rotate) and Option-orbit (pivot on selection). Production:
    /// <see cref="CitiesSelectionContext"/>; tests use a fake.
    /// </summary>
    public interface ISelectionContext
    {
        /// <summary>
        /// World position of the current selection (orbit pivot). Returns false when
        /// nothing is selected or APIs are unavailable.
        /// </summary>
        bool TryGetSelectedWorldPosition(out float x, out float y, out float z);

        /// <summary>
        /// Apply a yaw delta in degrees to the selected / placeable object.
        /// Returns false when nothing rotatable is selected (caller should yaw the camera).
        /// </summary>
        bool TryApplyObjectYawDelta(float deltaDegrees);
    }
}
