namespace TrackpadCameraControl
{
    /// <summary>
    /// Seam for selection-aware object yaw. Production: <see cref="CitiesSelectionContext"/>; tests use a fake.
    /// Option-orbit does not re-home Target — <see cref="TryGetSelectedWorldPosition"/> is retained for the seam only.
    /// </summary>
    public interface ISelectionContext
    {
        /// <summary>
        /// Reserved seam hook. Option-orbit never re-homes Target; always returns false.
        /// </summary>
        bool TryGetSelectedWorldPosition(out float x, out float y, out float z);

        /// <summary>
        /// Apply a yaw delta in degrees to the selected / placeable object.
        /// Returns false when nothing rotatable is selected (caller should yaw the camera).
        /// </summary>
        bool TryApplyObjectYawDelta(float deltaDegrees);
    }
}
