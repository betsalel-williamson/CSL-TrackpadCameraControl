namespace TrackpadCameraControl.Rewrite.Tests
{
    /// <summary>Selection port only — one subsystem fake for FeelMath rotate path.</summary>
    internal sealed class FakeSelectionContext : ISelectionContext
    {
        public bool HasSelection { get; set; }

        public float AppliedYawDegrees { get; private set; }

        public int RotateCalls { get; private set; }

        public bool TryGetSelectedWorldPosition(out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 0f;
            return HasSelection;
        }

        public bool TryApplyObjectYawDelta(float deltaDegrees)
        {
            if (!HasSelection)
            {
                return false;
            }

            AppliedYawDegrees += deltaDegrees;
            RotateCalls++;
            return true;
        }
    }
}
