namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Best-effort CS1 selection context. Headless: no object yaw. Cities: placement/relocate only.
    /// </summary>
    public sealed class CitiesSelectionContext : ISelectionContext
    {
        public static readonly CitiesSelectionContext Instance = new CitiesSelectionContext();

        public bool TryGetSelectedWorldPosition(out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 0f;
            return false;
        }

        public bool TryApplyObjectYawDelta(float deltaDegrees)
        {
            if (float.IsNaN(deltaDegrees) || deltaDegrees == 0f)
            {
                return false;
            }

#if HAS_CITIES
            try
            {
                ToolBase tool =
                    ToolsModifierControl.toolController != null
                        ? ToolsModifierControl.toolController.CurrentTool
                        : null;
                if (
                    tool is BuildingTool buildingTool
                    && (buildingTool.m_prefab != null || buildingTool.m_relocate != 0)
                )
                {
                    buildingTool.m_angle = NormalizeDegrees(buildingTool.m_angle + deltaDegrees);
                    return true;
                }

                if (tool is PropTool propTool && propTool.m_prefab != null)
                {
                    propTool.m_angle = NormalizeDegrees(propTool.m_angle + deltaDegrees);
                    return true;
                }
            }
            catch
            {
                // fail soft
            }
#endif
            return false;
        }

#if HAS_CITIES
        private static float NormalizeDegrees(float degrees)
        {
            degrees %= 360f;
            if (degrees < 0f)
            {
                degrees += 360f;
            }

            return degrees;
        }
#endif
    }
}
