using System;
using System.Reflection;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Best-effort CS1 selection context. Headless: no object yaw. Cities: placement/relocate only.
    /// Relocate ghost twist writes m_angle plus render angles (m_mouseAngle / m_cachedAngle).
    /// </summary>
    public sealed class CitiesSelectionContext : ISelectionContext
    {
        public static readonly CitiesSelectionContext Instance = new CitiesSelectionContext();

#if HAS_CITIES
        private static FieldInfo _buildingMouseAngleField;
        private static FieldInfo _buildingCachedAngleField;
        private static FieldInfo _buildingAngleChangedField;
        private static bool _buildingAngleFieldsResolved;
#endif

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
                    ApplyBuildingToolAngleDelta(buildingTool, deltaDegrees);
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
        private static void ApplyBuildingToolAngleDelta(
            BuildingTool buildingTool,
            float deltaDegrees
        )
        {
            EnsureBuildingAngleFields();
            float deltaRad = BuildingToolAngleDelta.DegreesToRadians(deltaDegrees);
            if (_buildingMouseAngleField != null)
            {
                object raw = _buildingMouseAngleField.GetValue(buildingTool);
                if (raw is float mouseAngle)
                {
                    _buildingMouseAngleField.SetValue(buildingTool, mouseAngle + deltaRad);
                }
            }

            if (_buildingCachedAngleField != null)
            {
                object raw = _buildingCachedAngleField.GetValue(buildingTool);
                if (raw is float cachedAngle)
                {
                    _buildingCachedAngleField.SetValue(buildingTool, cachedAngle + deltaRad);
                }
            }

            if (_buildingAngleChangedField != null)
            {
                _buildingAngleChangedField.SetValue(buildingTool, true);
            }
        }

        private static void EnsureBuildingAngleFields()
        {
            if (_buildingAngleFieldsResolved)
            {
                return;
            }

            _buildingAngleFieldsResolved = true;
            BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = typeof(BuildingTool);
            _buildingMouseAngleField = type.GetField("m_mouseAngle", flags);
            _buildingCachedAngleField = type.GetField("m_cachedAngle", flags);
            _buildingAngleChangedField = type.GetField("m_angleChanged", flags);
        }

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
