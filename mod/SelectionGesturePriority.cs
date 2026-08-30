namespace TrackpadCameraControl
{
    /// <summary>
    /// Pure priority for selection-aware rotate / orbit pivot (no Unity).
    /// Relocate and live selection beat placement ghost / stale mouse.
    /// </summary>
    public enum SelectionGestureKind
    {
        None = 0,

        /// <summary>Relocate in progress: rotate / pivot the ghost preview, not the old-cell buffer.</summary>
        RelocateInstance = 1,

        /// <summary>Rotate / pivot an explicitly selected placed instance.</summary>
        SelectedInstance = 2,

        /// <summary>Ghost placement only: tool m_angle / m_mousePosition.</summary>
        PlacementGhost = 3,
    }

    public static class SelectionGesturePriority
    {
        /// <param name="placementToolArmed">BuildingTool/PropTool with prefab set.</param>
        /// <param name="relocateBuildingId">BuildingTool.m_relocate; 0 = not relocating.</param>
        /// <param name="hasValidSelectedInstance">InstanceManager selection is non-empty and valid.</param>
        public static SelectionGestureKind Resolve(
            bool placementToolArmed,
            int relocateBuildingId,
            bool hasValidSelectedInstance
        )
        {
            if (placementToolArmed && relocateBuildingId != 0)
            {
                return SelectionGestureKind.RelocateInstance;
            }

            if (hasValidSelectedInstance)
            {
                return SelectionGestureKind.SelectedInstance;
            }

            if (placementToolArmed)
            {
                return SelectionGestureKind.PlacementGhost;
            }

            return SelectionGestureKind.None;
        }

        /// <summary>
        /// Two-finger object yaw applies only while placing a new ghost or relocating.
        /// A click-selected placed instance must not steal camera yaw.
        /// </summary>
        public static bool AllowsObjectYaw(SelectionGestureKind kind)
        {
            return kind == SelectionGestureKind.RelocateInstance
                || kind == SelectionGestureKind.PlacementGhost;
        }
    }
}
