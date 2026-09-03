using System;
using System.Reflection;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Best-effort CS1 selection context.
    /// Object yaw: relocate / new-placement ghosts only.
    /// Click-selected instances do not steal yaw. Option-orbit never re-homes Target.
    /// Hover is not used. All paths fail soft.
    /// </summary>
    public sealed class CitiesSelectionContext : ISelectionContext
    {
        public static readonly CitiesSelectionContext Instance = new CitiesSelectionContext();

#if HAS_CITIES
        private static FieldInfo _mousePositionField;
        private static bool _mousePositionResolved;
        private static FieldInfo _selectedInstanceField;
        private static bool _selectedInstanceResolved;
        private static FieldInfo _buildingMouseAngleField;
        private static FieldInfo _buildingCachedAngleField;
        private static FieldInfo _buildingAngleChangedField;
        private static bool _buildingAngleFieldsResolved;
        private const float Deg2Rad = (float)(Math.PI / 180.0);
#endif

        public bool TryGetSelectedWorldPosition(out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 0f;
            // Option-orbit never re-homes Target (see SelectionGesturePriority.AllowsOrbitPivot).
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
                bool placementArmed = IsPlacementTool(TryGetCurrentTool());
                int relocateId = TryGetRelocateBuildingId();
                bool hasSelected =
                    TryGetInstanceSelection(out InstanceID selectedId)
                    && IsInstanceValid(selectedId);

                SelectionGestureKind kind = SelectionGesturePriority.Resolve(
                    placementArmed,
                    relocateId,
                    hasSelected
                );

                if (!SelectionGesturePriority.AllowsObjectYaw(kind))
                {
                    return false;
                }

                // Relocate and new-placement ghost: tool m_angle only.
                return TryRotatePlacementTool(deltaDegrees);
            }
            catch
            {
                // fail soft
            }
#endif
            return false;
        }

#if HAS_CITIES
        private static int TryGetRelocateBuildingId()
        {
            ToolBase tool = TryGetCurrentTool();
            // Prefab may be unset briefly; m_relocate alone means relocate mode.
            if (tool is BuildingTool buildingTool)
            {
                return buildingTool.m_relocate;
            }

            return 0;
        }

        private static bool IsInstanceValid(InstanceID id)
        {
            if (id.IsEmpty)
            {
                return false;
            }

            try
            {
                InstanceManager manager = InstanceManager.instance;
                if (manager != null && !InstanceManager.IsValid(id))
                {
                    return false;
                }
            }
            catch
            {
                // If IsValid is unavailable, fall through to buffer reads.
            }

            return true;
        }

        private static bool TryGetToolPlacementPosition(out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 0f;
            ToolBase tool = TryGetCurrentTool();
            if (!IsPlacementTool(tool))
            {
                return false;
            }

            if (!TryReadMousePosition(tool, out Vector3 mouse))
            {
                return false;
            }

            x = mouse.x;
            y = mouse.y;
            z = mouse.z;
            return true;
        }

        private static bool TryRotatePlacementTool(float deltaDegrees)
        {
            ToolBase tool = TryGetCurrentTool();
            if (
                tool is BuildingTool buildingTool
                && (buildingTool.m_prefab != null || buildingTool.m_relocate != 0)
            )
            {
                // New place: SimulationStep copies m_angle → m_mouseAngle when not in
                // relocate-ish tool modes. Relocate render uses m_cachedAngle ← m_mouseAngle
                // and often ignores m_angle, so update all three for a visible ghost twist.
                buildingTool.m_angle = NormalizeDegrees(buildingTool.m_angle + deltaDegrees);
                ApplyBuildingToolAngleDelta(buildingTool, deltaDegrees);
                return true;
            }

            if (tool is PropTool propTool && propTool.m_prefab != null)
            {
                propTool.m_angle = NormalizeDegrees(propTool.m_angle + deltaDegrees);
                return true;
            }

            return false;
        }

        private static void ApplyBuildingToolAngleDelta(
            BuildingTool buildingTool,
            float deltaDegrees
        )
        {
            EnsureBuildingAngleFields();
            float deltaRad = deltaDegrees * Deg2Rad;
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

        private static bool IsPlacementTool(ToolBase tool)
        {
            if (tool is BuildingTool buildingTool)
            {
                return buildingTool.m_prefab != null || buildingTool.m_relocate != 0;
            }

            if (tool is PropTool propTool)
            {
                return propTool.m_prefab != null;
            }

            return false;
        }

        private static bool TryGetInstanceSelection(out InstanceID id)
        {
            id = default(InstanceID);
            if (
                TryReadInstanceIdField(
                    ref _selectedInstanceResolved,
                    ref _selectedInstanceField,
                    "m_selectedInstance",
                    out id
                ) && !id.IsEmpty
            )
            {
                return true;
            }

            return false;
        }

        private static bool TryReadInstanceIdField(
            ref bool resolved,
            ref FieldInfo cached,
            string fieldName,
            out InstanceID id
        )
        {
            id = default(InstanceID);
            if (!resolved)
            {
                resolved = true;
                cached = FindInstanceIdField(fieldName);
            }

            if (cached == null)
            {
                return false;
            }

            object host = ResolveFieldHost(cached);
            if (host == null && !cached.IsStatic)
            {
                return false;
            }

            object value = cached.GetValue(host);
            if (value is InstanceID instanceId)
            {
                id = instanceId;
                return true;
            }

            return false;
        }

        private static FieldInfo FindInstanceIdField(string fieldName)
        {
            Type[] hosts =
            {
                typeof(ToolBase),
                typeof(DefaultTool),
                typeof(ToolController),
                typeof(InstanceManager),
            };
            BindingFlags flags =
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic;
            foreach (Type host in hosts)
            {
                FieldInfo field = host.GetField(fieldName, flags);
                if (field != null && field.FieldType == typeof(InstanceID))
                {
                    return field;
                }
            }

            // Last resort: scan loaded Assembly-CSharp types once per field name.
            try
            {
                Assembly asm = typeof(ToolBase).Assembly;
                foreach (Type type in asm.GetTypes())
                {
                    FieldInfo field = type.GetField(fieldName, flags);
                    if (field != null && field.FieldType == typeof(InstanceID))
                    {
                        return field;
                    }
                }
            }
            catch
            {
                // fail soft
            }

            return null;
        }

        private static object ResolveFieldHost(FieldInfo field)
        {
            if (field.IsStatic)
            {
                return null;
            }

            Type declaring = field.DeclaringType;
            if (declaring == null)
            {
                return null;
            }

            if (typeof(ToolBase).IsAssignableFrom(declaring))
            {
                ToolBase tool = TryGetCurrentTool();
                if (tool != null && declaring.IsInstanceOfType(tool))
                {
                    return tool;
                }

                return null;
            }

            if (declaring == typeof(ToolController))
            {
                return ToolsModifierControl.toolController;
            }

            if (declaring == typeof(InstanceManager))
            {
                return InstanceManager.instance;
            }

            // Singleton-style: look for public static `instance`
            PropertyInfo instanceProp = declaring.GetProperty(
                "instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
            );
            if (instanceProp != null)
            {
                return instanceProp.GetValue(null, null);
            }

            FieldInfo instanceField = declaring.GetField(
                "instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
            );
            if (instanceField != null)
            {
                return instanceField.GetValue(null);
            }

            return null;
        }

        private static bool TryGetInstancePosition(
            InstanceID id,
            out float x,
            out float y,
            out float z
        )
        {
            x = 0f;
            y = 0f;
            z = 0f;
            if (id.Building != 0)
            {
                BuildingManager buildings = BuildingManager.instance;
                if (buildings == null)
                {
                    return false;
                }

                Vector3 p = buildings.m_buildings.m_buffer[id.Building].m_position;
                x = p.x;
                y = p.y;
                z = p.z;
                return true;
            }

            if (id.Prop != 0)
            {
                PropManager props = PropManager.instance;
                if (props == null)
                {
                    return false;
                }

                PropInstance prop = props.m_props.m_buffer[id.Prop];
                if (!TryReadPropPosition(prop, out Vector3 p))
                {
                    return false;
                }

                x = p.x;
                y = p.y;
                z = p.z;
                return true;
            }

            return false;
        }

        private static bool TryReadPropPosition(PropInstance prop, out Vector3 position)
        {
            position = Vector3.zero;
            BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo posProp = typeof(PropInstance).GetProperty("Position", flags);
            if (posProp != null)
            {
                object value = posProp.GetValue(prop, null);
                if (value is Vector3 v)
                {
                    position = v;
                    return true;
                }
            }

            FieldInfo posField = typeof(PropInstance).GetField("m_position", flags);
            if (posField != null)
            {
                object value = posField.GetValue(prop);
                if (value is Vector3 v)
                {
                    position = v;
                    return true;
                }
            }

            return false;
        }

        private static ToolBase TryGetCurrentTool()
        {
            ToolController controller = ToolsModifierControl.toolController;
            return controller != null ? controller.CurrentTool : null;
        }

        private static bool TryReadMousePosition(ToolBase tool, out Vector3 position)
        {
            position = Vector3.zero;
            if (!_mousePositionResolved)
            {
                _mousePositionResolved = true;
                _mousePositionField = typeof(ToolBase).GetField(
                    "m_mousePosition",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            if (_mousePositionField == null || tool == null)
            {
                return false;
            }

            object value = _mousePositionField.GetValue(tool);
            if (value is Vector3 v)
            {
                position = v;
                return true;
            }

            return false;
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
