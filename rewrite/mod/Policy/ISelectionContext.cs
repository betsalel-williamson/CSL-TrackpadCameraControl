namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Seam for selection-aware object yaw. Tests use a fake; production uses Cities adapter.</summary>
    public interface ISelectionContext
    {
        bool TryGetSelectedWorldPosition(out float x, out float y, out float z);

        bool TryApplyObjectYawDelta(float deltaDegrees);
    }
}
