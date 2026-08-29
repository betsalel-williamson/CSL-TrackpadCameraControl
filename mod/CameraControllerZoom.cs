using System;
using System.Reflection;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl
{
    /// <summary>
    /// Production <see cref="ICameraZoom"/> for CS1. CameraController is a MonoBehaviour with
    /// no static instance — resolve via FindObjectOfType, then read/write m_targetSize.
    /// </summary>
    public sealed class CameraControllerZoom : ICameraZoom
    {
        private static FieldInfo _targetSizeField;
        private static FieldInfo _currentSizeField;
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
                WriteDiag("CameraController resolved; pinch zoom armed");
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

                _targetSizeField = _camType.GetField(
                    "m_targetSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                _currentSizeField = _camType.GetField(
                    "m_currentSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

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
