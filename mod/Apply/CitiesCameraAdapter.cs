using System;
using System.Reflection;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Thin Cities camera adapter: queue-only orbit; reflect CameraController fields when present.
    /// </summary>
    public class CitiesCameraAdapter : ICameraController
    {
        private static FieldInfo _targetSizeField;
        private static FieldInfo _currentSizeField;
        private static FieldInfo _targetPositionField;
        private static FieldInfo _currentPositionField;
        private static FieldInfo _targetAngleField;
        private static FieldInfo _currentAngleField;
        private static FieldInfo _angleVelocityField;
        private static Type _camType;
        private static bool _resolved;
        private static bool _available;

        private object _cachedController;
        private float _pendingYaw;
        private float _pendingPitch;

        public float Size
        {
            get
            {
                if (!TryGetController(out object cam) || _targetSizeField == null)
                {
                    return float.NaN;
                }

                return (float)_targetSizeField.GetValue(cam);
            }
            set
            {
                if (!TryGetController(out object cam) || _targetSizeField == null)
                {
                    return;
                }

                _targetSizeField.SetValue(cam, value);
                if (_currentSizeField != null)
                {
                    _currentSizeField.SetValue(cam, value);
                }
            }
        }

        public float TargetX
        {
            get { return GetPositionComponent(0); }
            set { SetPositionComponent(0, value); }
        }

        public float TargetY
        {
            get { return GetPositionComponent(1); }
            set { SetPositionComponent(1, value); }
        }

        public float TargetZ
        {
            get { return GetPositionComponent(2); }
            set { SetPositionComponent(2, value); }
        }

        public float AngleX
        {
            get { return GetAngleComponent(0); }
            set { SetAngleComponent(0, value); }
        }

        public float AngleY
        {
            get { return GetAngleComponent(1); }
            set { SetAngleComponent(1, value); }
        }

        public void ClampPanTarget(ref float x, ref float z)
        {
#if HAS_CITIES
            try
            {
                GameAreaManager areas = GameAreaManager.instance;
                if (areas == null)
                {
                    return;
                }

                float y = TargetY;
                if (float.IsNaN(y))
                {
                    y = 0f;
                }

                Vector3 position = new Vector3(x, y, z);
                areas.ClampPoint(ref position);
                x = position.x;
                z = position.z;
            }
            catch
            {
                // fail soft
            }
#endif
        }

        public void AddAngleVelocity(float yawDelta, float pitchDelta)
        {
            if (yawDelta == 0f && pitchDelta == 0f)
            {
                return;
            }

            _pendingYaw += yawDelta;
            _pendingPitch += pitchDelta;
        }

        public void FlushPendingAngleVelocity(float deltaTimeSeconds)
        {
            if (
                (_pendingYaw == 0f && _pendingPitch == 0f)
                || !TryGetController(out object cam)
                || _angleVelocityField == null
            )
            {
                _pendingYaw = 0f;
                _pendingPitch = 0f;
                return;
            }

            float dt = deltaTimeSeconds;
            if (dt < 0.001f)
            {
                dt = 0.001f;
            }

            float yawDelta = _pendingYaw / dt;
            float pitchDelta = _pendingPitch / dt;
            _pendingYaw = 0f;
            _pendingPitch = 0f;

            object vec = _angleVelocityField.GetValue(cam);
            if (vec == null)
            {
                return;
            }

            Type vectorType = _angleVelocityField.FieldType;
            float x = GetVectorComponent(vec, 0);
            float y = GetVectorComponent(vec, 1);
            if (float.IsNaN(x))
            {
                x = 0f;
            }

            if (float.IsNaN(y))
            {
                y = 0f;
            }

            object next = SetVectorComponent(vec, vectorType, 0, x + yawDelta);
            next = SetVectorComponent(next, vectorType, 1, y + pitchDelta);
            _angleVelocityField.SetValue(cam, next);
        }

        public void ClearPendingAngleVelocity()
        {
            _pendingYaw = 0f;
            _pendingPitch = 0f;
        }

        public void ClearAngleVelocity(bool yaw, bool pitch)
        {
            if (yaw)
            {
                _pendingYaw = 0f;
            }

            if (pitch)
            {
                _pendingPitch = 0f;
            }

            if (
                !TryGetController(out object cam)
                || _angleVelocityField == null
                || (!yaw && !pitch)
            )
            {
                return;
            }

            object vec = _angleVelocityField.GetValue(cam);
            if (vec == null)
            {
                return;
            }

            Type vectorType = _angleVelocityField.FieldType;
            float x = GetVectorComponent(vec, 0);
            float y = GetVectorComponent(vec, 1);
            if (float.IsNaN(x))
            {
                x = 0f;
            }

            if (float.IsNaN(y))
            {
                y = 0f;
            }

            if (yaw)
            {
                x = 0f;
            }

            if (pitch)
            {
                y = 0f;
            }

            object next = SetVectorComponent(vec, vectorType, 0, x);
            next = SetVectorComponent(next, vectorType, 1, y);
            _angleVelocityField.SetValue(cam, next);
        }

        private float GetPositionComponent(int index)
        {
            if (!TryGetController(out object cam) || _targetPositionField == null)
            {
                return float.NaN;
            }

            return GetVectorComponent(_targetPositionField.GetValue(cam), index);
        }

        private void SetPositionComponent(int index, float value)
        {
            if (!TryGetController(out object cam) || _targetPositionField == null)
            {
                return;
            }

            object vec = _targetPositionField.GetValue(cam);
            vec = SetVectorComponent(vec, _targetPositionField.FieldType, index, value);
            _targetPositionField.SetValue(cam, vec);
            if (_currentPositionField != null)
            {
                _currentPositionField.SetValue(cam, vec);
            }
        }

        private float GetAngleComponent(int index)
        {
            if (!TryGetController(out object cam) || _targetAngleField == null)
            {
                return float.NaN;
            }

            return GetVectorComponent(_targetAngleField.GetValue(cam), index);
        }

        private void SetAngleComponent(int index, float value)
        {
            if (!TryGetController(out object cam) || _targetAngleField == null)
            {
                return;
            }

            Type vectorType = _targetAngleField.FieldType;
            object target = _targetAngleField.GetValue(cam);
            target = SetVectorComponent(target, vectorType, index, value);
            _targetAngleField.SetValue(cam, target);

            if (_currentAngleField != null)
            {
                object current = _currentAngleField.GetValue(cam);
                current = SetVectorComponent(current, vectorType, index, value);
                _currentAngleField.SetValue(cam, current);
            }
        }

        private static float GetVectorComponent(object vector, int index)
        {
            if (vector == null)
            {
                return float.NaN;
            }

            string name =
                index == 0 ? "x"
                : index == 1 ? "y"
                : "z";
            FieldInfo f = vector.GetType().GetField(name);
            if (f == null)
            {
                return float.NaN;
            }

            return (float)f.GetValue(vector);
        }

        private static object SetVectorComponent(
            object vector,
            Type vectorType,
            int index,
            float value
        )
        {
            if (vector == null)
            {
                vector = Activator.CreateInstance(vectorType);
            }

            string name =
                index == 0 ? "x"
                : index == 1 ? "y"
                : "z";
            FieldInfo f = vectorType.GetField(name);
            if (f == null)
            {
                return vector;
            }

            object boxed = vector;
            f.SetValue(boxed, value);
            return boxed;
        }

        private bool TryGetController(out object cam)
        {
            cam = null;
            EnsureCameraFields();
            if (!_available)
            {
                return false;
            }

            if (_cachedController != null)
            {
#if HAS_CITIES
                var mb = _cachedController as MonoBehaviour;
                if (mb != null && mb)
                {
                    cam = _cachedController;
                    return true;
                }

                _cachedController = null;
#else
                cam = _cachedController;
                return true;
#endif
            }

#if HAS_CITIES
            cam = UnityEngine.Object.FindObjectOfType<CameraController>();
#else
            cam = null;
#endif
            if (cam == null)
            {
                return false;
            }

            _cachedController = cam;
            return true;
        }

        private static void EnsureCameraFields()
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            try
            {
#if HAS_CITIES
                _camType = typeof(CameraController);
                BindingFlags bf =
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                _targetSizeField = _camType.GetField("m_targetSize", bf);
                _currentSizeField = _camType.GetField("m_currentSize", bf);
                _targetPositionField = _camType.GetField("m_targetPosition", bf);
                _currentPositionField = _camType.GetField("m_currentPosition", bf);
                _targetAngleField = _camType.GetField("m_targetAngle", bf);
                _currentAngleField = _camType.GetField("m_currentAngle", bf);
                _angleVelocityField = _camType.GetField("m_angleVelocity", bf);
                _available = _targetSizeField != null;
#else
                _available = false;
#endif
            }
            catch
            {
                _available = false;
            }
        }
    }
}
