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

        public float MinX
        {
            get
            {
                return TryGetCityBounds(out float minX, out _, out _, out _) ? minX : float.NaN;
            }
        }

        public float MaxX
        {
            get
            {
                return TryGetCityBounds(out _, out float maxX, out _, out _) ? maxX : float.NaN;
            }
        }

        public float MinZ
        {
            get
            {
                return TryGetCityBounds(out _, out _, out float minZ, out _) ? minZ : float.NaN;
            }
        }

        public float MaxZ
        {
            get
            {
                return TryGetCityBounds(out _, out _, out _, out float maxZ) ? maxZ : float.NaN;
            }
        }

        /// <summary>
        /// CS1 playable/game area on XZ is ±8640 (9×9 tiles). Prefer GameAreaManager when present.
        /// </summary>
        private const float GameAreaHalfExtent = 8640f;

        private static bool TryGetCityBounds(
            out float minX,
            out float maxX,
            out float minZ,
            out float maxZ
        )
        {
            minX = float.NaN;
            maxX = float.NaN;
            minZ = float.NaN;
            maxZ = float.NaN;
#if HAS_CITIES
            try
            {
                // Prefer unlocked-area clamp via GameAreaManager when the singleton is alive;
                // axis bounds still come from the fixed game-area half-extent so applicator
                // can clamp without Unity-only APIs in tests.
                if (GameAreaManager.instance == null)
                {
                    return false;
                }

                minX = -GameAreaHalfExtent;
                maxX = GameAreaHalfExtent;
                minZ = -GameAreaHalfExtent;
                maxZ = GameAreaHalfExtent;
                return true;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
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
#if HAS_CITIES
            if (index == 0 || index == 2)
            {
                vec = ClampVectorToGameArea(vec);
            }
#endif
            _targetPositionField.SetValue(cam, vec);
            if (_currentPositionField != null)
            {
                _currentPositionField.SetValue(cam, vec);
            }
        }

#if HAS_CITIES
        private static object ClampVectorToGameArea(object vec)
        {
            try
            {
                GameAreaManager areas = GameAreaManager.instance;
                if (areas == null || vec == null || !(vec is Vector3))
                {
                    return vec;
                }

                Vector3 position = (Vector3)vec;
                areas.ClampPoint(ref position);
                return position;
            }
            catch
            {
                return vec;
            }
        }
#endif

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
