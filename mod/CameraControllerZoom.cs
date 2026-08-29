using System;
using System.Reflection;

namespace TrackpadCameraControl
{
    /// <summary>Production <see cref="ICameraZoom"/> via reflection on CameraController.</summary>
    public sealed class CameraControllerZoom : ICameraZoom
    {
        private static FieldInfo _targetSizeField;
        private static FieldInfo _currentSizeField;
        private static PropertyInfo _instanceProp;
        private static bool _resolved;
        private static bool _available;

        public bool IsAvailable
        {
            get
            {
                EnsureCameraFields();
                return _available;
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

        private static bool TryGetController(out object cam)
        {
            cam = null;
            EnsureCameraFields();
            if (!_available)
            {
                return false;
            }

            try
            {
                cam = _instanceProp.GetValue(null, null);
                return cam != null;
            }
            catch
            {
                return false;
            }
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
                Type camType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    camType = asm.GetType("CameraController");
                    if (camType != null)
                    {
                        break;
                    }
                }

                if (camType == null)
                {
                    return;
                }

                _instanceProp =
                    camType.GetProperty(
                        "instance",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                    )
                    ?? camType.GetProperty(
                        "Instance",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                    );

                _targetSizeField = camType.GetField(
                    "m_targetSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                _currentSizeField = camType.GetField(
                    "m_currentSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                _available = _instanceProp != null && _targetSizeField != null;
            }
            catch
            {
                _available = false;
            }
        }
    }
}
