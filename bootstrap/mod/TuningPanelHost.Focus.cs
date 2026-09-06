#if HAS_CITIES
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    internal static partial class TuningPanelHost
    {
        private const byte TitleBarRgb = 0;
        private const byte PanelBodyRgb = 255;
        private const byte PanelBodyAlphaActive = 255;
        private const byte PanelBodyAlphaInactive = 160;

        private static bool _panelFocused = true;

        /// <summary>Per-frame hover visual update (called from gesture thread).</summary>
        public static void ProcessPanelFocusVisual()
        {
            if (_root == null || !_root.isVisible)
            {
                return;
            }

            bool hovered = IsMouseOverPanel();
            if (hovered == _panelFocused)
            {
                return;
            }

            _panelFocused = hovered;
            ApplyPanelFocusVisual();
        }

        private static void OnPanelMouseDown()
        {
            if (GameUiContext.Default.IsMenuOrOptionsOpen())
            {
                return;
            }

            _root.BringToFront();
        }

        private static void ResetPanelFocusVisual()
        {
            _panelFocused = true;
        }

        private static bool IsMouseOverPanel()
        {
            UIComponent hovered = UIInput.hoveredComponent;
            if (IsPanelOrChild(hovered))
            {
                return true;
            }

            return ContainsMouseRecursive(_root);
        }

        private static bool ContainsMouseRecursive(UIComponent component)
        {
            if (component == null || !component.isVisible)
            {
                return false;
            }

            if (component.containsMouse)
            {
                return true;
            }

            int count = component.components.Count;
            for (int i = 0; i < count; i++)
            {
                if (ContainsMouseRecursive(component.components[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPanelOrChild(UIComponent component)
        {
            if (component == null || _root == null)
            {
                return false;
            }

            UIComponent walk = component;
            while (walk != null)
            {
                if (walk == _root)
                {
                    return true;
                }

                walk = walk.parent;
            }

            return false;
        }

        private static void ApplyPanelFocusVisual()
        {
            if (_root == null)
            {
                return;
            }

            byte alpha = _panelFocused ? PanelBodyAlphaActive : PanelBodyAlphaInactive;
            _root.color = new Color32(PanelBodyRgb, PanelBodyRgb, PanelBodyRgb, alpha);
            if (_titleBar != null)
            {
                _titleBar.color = new Color32(TitleBarRgb, TitleBarRgb, TitleBarRgb, alpha);
            }
        }
    }
}
#endif
