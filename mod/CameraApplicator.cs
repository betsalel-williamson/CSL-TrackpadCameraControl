using System;
using System.Reflection;

namespace TrackpadCameraControl
{
    public static class CameraApplicator
    {
        private static FieldInfo _targetSizeField;
        private static FieldInfo _currentSizeField;
        private static PropertyInfo _instanceProp;
        private static bool _resolved;
        private static bool _available;

        public static void Apply(
            CameraOp op,
            float dx,
            float dy,
            float pinchDelta,
            float rotateDelta,
            ModSettings settings
        )
        {
            _ = dx;
            _ = dy;
            _ = rotateDelta;

            if (op != CameraOp.Zoom || settings == null)
            {
                return;
            }

            EnsureCameraFields();
            if (!_available)
            {
                return;
            }

            object cam = _instanceProp.GetValue(null, null);
            if (cam == null)
            {
                return;
            }

            float delta = pinchDelta * settings.ZoomSensitivity;
            if (settings.InvertZoom)
            {
                delta = -delta;
            }

            // Pinch out (positive scale delta) → zoom in (smaller size).
            float size = (float)_targetSizeField.GetValue(cam);
            float next = size * (1f - delta);
            if (next < 10f)
            {
                next = 10f;
            }

            if (next > 5000f)
            {
                next = 5000f;
            }

            _targetSizeField.SetValue(cam, next);
            if (_currentSizeField != null)
            {
                _currentSizeField.SetValue(cam, next);
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
