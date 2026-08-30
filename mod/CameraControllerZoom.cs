using System;
using System.Reflection;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl
{
    /// <summary>
    /// Production <see cref="ICameraController"/> for CS1. CameraController is a MonoBehaviour with
    /// no static instance — resolve via FindObjectOfType, then read/write target size/position/angle.
    /// </summary>
    public sealed class CameraControllerZoom : ICameraController
    {
        private static FieldInfo _targetSizeField;
        private static FieldInfo _currentSizeField;
        private static FieldInfo _targetPositionField;
        private static FieldInfo _currentPositionField;
        private static FieldInfo _targetAngleField;
        private static FieldInfo _currentAngleField;
        private static FieldInfo _angleVelocityField;
#if !HAS_CITIES
        private static MethodInfo _findObjectOfType;
#endif
        private static Type _camType;
        private static bool _resolved;
        private static bool _available;
        private static bool _loggedMissing;
        private static bool _loggedOk;

        private object _cachedController;
        private int _missStreak;
        private float _pendingYaw;
        private float _pendingPitch;

        public bool IsAvailable
        {
            get
            {
                EnsureCameraFields();
                return _available && TryGetController(out _);
            }
        }

        public float Size
        {
            get
            {
                if (!TryGetController(out object cam))
                {
                    return float.NaN;
                }

                return (float)_targetSizeField.GetValue(cam);
            }
            set
            {
                if (!TryGetController(out object cam))
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

        /// <summary>
        /// Clamp pan to the current unlocked game area (grows with purchases; not a fixed square).
        /// </summary>
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
                // Fail soft: leave proposed pan unclamped.
            }
#endif
        }

        /// <summary>
        /// Queue yaw/pitch for middle-mouse-style orbit. Does not write angles or
        /// <c>m_angleVelocity</c> here — vanilla damps velocity first in UpdateTargetPosition.
        /// Flush from HandleMouseEvents Harmony postfix (after damp, before integrate).
        /// </summary>
        public void AddAngleVelocity(float yawDelta, float pitchDelta)
        {
            if (yawDelta == 0f && pitchDelta == 0f)
            {
                return;
            }

            _pendingYaw += yawDelta;
            _pendingPitch += pitchDelta;
        }

        /// <summary>
        /// Write queued deltas into <c>m_angleVelocity</c> as pending / max(dt, 0.001)
        /// so velocity * dt ≈ pending this frame.
        /// </summary>
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

            if (!TryGetController(out object cam) || _angleVelocityField == null || (!yaw && !pitch))
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
            // Pan uses ClampPanTarget (joint XZ via GameAreaManager.ClampPoint) before TargetX/Z
            // writes so X and Z are not clamped against a stale other axis.
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

            object vec = _targetAngleField.GetValue(cam);
            vec = SetVectorComponent(vec, _targetAngleField.FieldType, index, value);
            _targetAngleField.SetValue(cam, vec);
            if (_currentAngleField != null)
            {
                _currentAngleField.SetValue(cam, vec);
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

            // Structs: mutate boxed copy then return for SetValue.
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
                LogMissingOnce("CameraController type/fields not resolved");
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

            cam = FindController();
            if (cam == null)
            {
                _missStreak++;
                if (_missStreak == 1 || (_missStreak % 300) == 0)
                {
                    WriteDiag(
                        "CameraController not in scene yet (load a city / wait for gameplay)"
                    );
                }

                return false;
            }

            _cachedController = cam;
            _missStreak = 0;
            if (!_loggedOk)
            {
                _loggedOk = true;
                WriteDiag("CameraController resolved; gesture camera armed");
            }

            return true;
        }

        private static object FindController()
        {
#if HAS_CITIES
            return UnityEngine.Object.FindObjectOfType<CameraController>();
#else
            if (_findObjectOfType == null || _camType == null)
            {
                return null;
            }

            try
            {
                return _findObjectOfType.Invoke(null, new object[] { _camType });
            }
            catch
            {
                return null;
            }
#endif
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
#else
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    _camType = asm.GetType("CameraController");
                    if (_camType != null)
                    {
                        break;
                    }
                }

                if (_camType == null)
                {
                    return;
                }

                Type objectType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    objectType = asm.GetType("UnityEngine.Object");
                    if (objectType != null)
                    {
                        break;
                    }
                }

                if (objectType != null)
                {
                    _findObjectOfType = objectType.GetMethod(
                        "FindObjectOfType",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[] { typeof(Type) },
                        null
                    );
                }
#endif

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
            }
            catch
            {
                _available = false;
            }
        }

        private static void LogMissingOnce(string detail)
        {
            if (_loggedMissing)
            {
                return;
            }

            _loggedMissing = true;
            WriteDiag(detail);
        }

        private static void WriteDiag(string message)
        {
            try
            {
                string line = "TrackpadCameraControl: " + message;
#if HAS_CITIES
                Debug.Log(line);
#endif
                string tmp = Environment.GetEnvironmentVariable("TMPDIR");
                if (string.IsNullOrEmpty(tmp))
                {
                    tmp = System.IO.Path.GetTempPath();
                }

                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(tmp, "trackpad-camera-control-mod.log"),
                    line + Environment.NewLine
                );
            }
            catch
            {
                // ignore
            }
        }
    }
}
