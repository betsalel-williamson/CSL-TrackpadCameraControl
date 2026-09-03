#if HAS_CITIES
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ColossalFramework;
using ColossalFramework.Globalization;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
{
    internal static partial class VanillaCameraKeyLabels
    {
        private const string KeyNameCategory = "KEYNAME";

        private static readonly string[] WatchedCameraKeyFields =
        {
            "m_cameraZoomCloser",
            "m_cameraZoomAway",
            "m_cameraMoveForward",
            "m_cameraMoveBackward",
            "m_cameraMoveLeft",
            "m_cameraMoveRight",
            "m_cameraRotateLeft",
            "m_cameraRotateRight",
            "m_cameraMouseRotate",
        };

        private static readonly Dictionary<string, FieldInfo> CameraKeyFields =
            new Dictionary<string, FieldInfo>();

        private static CameraController _cachedCamera;

        internal static bool IsWatchedCameraKey(SavedInputKey key)
        {
            if (key == null)
            {
                return false;
            }

            CameraController camera = GetCameraController();
            if (camera == null)
            {
                return false;
            }

            for (int i = 0; i < WatchedCameraKeyFields.Length; i++)
            {
                FieldInfo field = ResolveField(WatchedCameraKeyFields[i]);
                if (field == null)
                {
                    continue;
                }

                if (ReferenceEquals(field.GetValue(camera) as SavedInputKey, key))
                {
                    return true;
                }
            }

            return false;
        }

        internal static string ComputeFingerprint()
        {
            var sb = new StringBuilder(128);
            AppendLocaleFingerprint(sb, "OPTIONS", "mouseWheelZoom");
            AppendLocaleFingerprint(sb, "OPTIONS", "edgeScrolling");
            for (int i = 0; i < WatchedCameraKeyFields.Length; i++)
            {
                AppendKeyFingerprint(sb, ReadCameraKey(WatchedCameraKeyFields[i]));
            }

            return sb.ToString();
        }

        private static void AppendLocaleFingerprint(StringBuilder sb, string category, string key)
        {
            sb.Append(category);
            sb.Append('.');
            sb.Append(key);
            sb.Append('=');
            sb.Append(TryLocale(category, key));
            sb.Append('|');
        }

        private static void AppendKeyFingerprint(StringBuilder sb, SavedInputKey key)
        {
            if (key == null)
            {
                sb.Append('|');
                return;
            }

            try
            {
                sb.Append(key.value);
                sb.Append(':');
                sb.Append(key.Key);
                sb.Append('|');
            }
            catch
            {
                sb.Append('|');
            }
        }

        private static string ResolveZoomVanillaBindings()
        {
            var labels = new List<string>();
            AddLocaleLabel(labels, "OPTIONS", "mouseWheelZoom");
            AddBindingLabel(labels, "m_cameraZoomCloser");
            AddBindingLabel(labels, "m_cameraZoomAway");
            return JoinBindingLabels(labels);
        }

        private static string ResolvePanVanillaBindings()
        {
            var labels = new List<string>();
            AddLocaleLabel(labels, "OPTIONS", "edgeScrolling");
            AddBindingLabel(labels, "m_cameraMoveForward");
            AddBindingLabel(labels, "m_cameraMoveBackward");
            AddBindingLabel(labels, "m_cameraMoveLeft");
            AddBindingLabel(labels, "m_cameraMoveRight");
            return JoinBindingLabels(labels);
        }

        private static string ResolveRotateVanillaBindings()
        {
            var labels = new List<string>();
            AddBindingLabel(labels, "m_cameraRotateLeft");
            AddBindingLabel(labels, "m_cameraRotateRight");
            return JoinBindingLabels(labels);
        }

        private static string ResolveOrbitVanillaBindings()
        {
            return FormatBindingLabel(ReadCameraKey("m_cameraMouseRotate"));
        }

        private static void AddBindingLabel(List<string> labels, string fieldName)
        {
            string label = FormatBindingLabel(ReadCameraKey(fieldName));
            if (!string.IsNullOrEmpty(label))
            {
                labels.Add(label);
            }
        }

        private static void AddLocaleLabel(List<string> labels, string category, string key)
        {
            string label = TryLocale(category, key);
            if (!string.IsNullOrEmpty(label))
            {
                labels.Add(label);
            }
        }

        private static string TryLocale(string category, string key)
        {
            try
            {
                if (Locale.Exists(category, key))
                {
                    return Locale.Get(category, key);
                }
            }
            catch
            {
                // fail soft
            }

            return null;
        }

        private static string FormatBindingLabel(SavedInputKey key)
        {
            if (!IsBound(key))
            {
                return null;
            }

            try
            {
                string label = key.ToLocalizedString(KeyNameCategory);
                return string.IsNullOrEmpty(label) ? null : label;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsBound(SavedInputKey key)
        {
            if (key == null)
            {
                return false;
            }

            try
            {
                if (key.value == SavedInputKey.Empty)
                {
                    return false;
                }

                return key.Key != KeyCode.None;
            }
            catch
            {
                return false;
            }
        }

        private static SavedInputKey ReadCameraKey(string fieldName)
        {
            try
            {
                CameraController camera = GetCameraController();
                if (camera == null)
                {
                    return null;
                }

                FieldInfo field = ResolveField(fieldName);
                if (field == null)
                {
                    return null;
                }

                return field.GetValue(camera) as SavedInputKey;
            }
            catch
            {
                return null;
            }
        }

        private static CameraController GetCameraController()
        {
            try
            {
                if (_cachedCamera == null)
                {
                    _cachedCamera = UnityEngine.Object.FindObjectOfType<CameraController>();
                }
                else if (!_cachedCamera)
                {
                    _cachedCamera = UnityEngine.Object.FindObjectOfType<CameraController>();
                }
            }
            catch
            {
                _cachedCamera = null;
            }

            return _cachedCamera;
        }

        private static FieldInfo ResolveField(string fieldName)
        {
            FieldInfo cached;
            if (CameraKeyFields.TryGetValue(fieldName, out cached))
            {
                return cached;
            }

            try
            {
                cached = typeof(CameraController).GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }
            catch
            {
                cached = null;
            }

            CameraKeyFields[fieldName] = cached;
            return cached;
        }
    }
}
#endif
