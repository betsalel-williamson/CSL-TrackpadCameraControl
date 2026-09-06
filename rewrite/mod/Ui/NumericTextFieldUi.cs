#if HAS_CITIES
using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Colossal UITextField helpers for Debug numeric fields (left-aligned, sanitize, Tab/Enter).</summary>
    internal static class NumericTextFieldUi
    {
        private static readonly object DefaultTabScope = new object();
        private static readonly List<UIComponent> TabStops = new List<UIComponent>();
        private static readonly List<object> TabScopes = new List<object>();
        private static int _tabAdvanceFrame = -1;

        public static void ConfigureFloatField(UITextField field)
        {
            if (field == null)
            {
                return;
            }

            field.numericalOnly = true;
            field.allowFloats = true;
            field.horizontalAlignment = UIHorizontalAlignment.Left;
            field.verticalAlignment = UIVerticalAlignment.Middle;
            field.padding = new RectOffset(6, 6, 3, 3);
            field.textColor = Color.white;
            field.disabledTextColor = new Color32(128, 128, 128, 255);
            field.selectionBackgroundColor = new Color32(0, 105, 210, 255);
            field.cursorBlinkTime = 0.45f;
            field.cursorWidth = 1;
            field.eventTextChanged += (UIComponent component, string text) =>
            {
                UITextField tf = component as UITextField;
                if (tf == null)
                {
                    return;
                }

                string sanitized = NumericFieldInput.SanitizePartialFloatText(text);
                if (!string.Equals(sanitized, text, StringComparison.Ordinal))
                {
                    tf.text = sanitized;
                }
            };
        }

        public static void WireConfirmKeys(
            UITextField field,
            bool includeInTabOrder = true,
            object tabScope = null
        )
        {
            if (field == null)
            {
                return;
            }

            field.submitOnFocusLost = true;
            field.canFocus = true;
            field.builtinKeyNavigation = true;
            if (includeInTabOrder)
            {
                RegisterTabStop(field, tabScope);
            }

            field.eventKeyDown += OnConfirmKeyDown;
            field.eventKeyUp += OnConfirmKeyUp;
        }

        public static void WireTabStop(UIComponent component, object tabScope = null)
        {
            if (component == null)
            {
                return;
            }

            component.canFocus = true;
            component.builtinKeyNavigation = false;
            RegisterTabStop(component, tabScope);
            component.eventKeyDown += OnTabKeyDown;
            component.eventKeyUp += OnTabKeyUp;
        }

        private static void RegisterTabStop(UIComponent component, object tabScope)
        {
            for (int i = 0; i < TabStops.Count; i++)
            {
                if (ReferenceEquals(TabStops[i], component))
                {
                    TabScopes[i] = tabScope ?? DefaultTabScope;
                    return;
                }
            }

            TabStops.Add(component);
            TabScopes.Add(tabScope ?? DefaultTabScope);
        }

        private static void OnConfirmKeyDown(UIComponent component, UIKeyEventParameter p)
        {
            if (p == null || p.used)
            {
                return;
            }

            UITextField field = component as UITextField;
            if (field == null || !field.hasFocus)
            {
                return;
            }

            if (IsConfirmKey(p))
            {
                p.Use();
                field.Unfocus();
                return;
            }

            if (IsTabKey(p))
            {
                p.Use();
                FocusNext(field);
            }
        }

        private static void OnConfirmKeyUp(UIComponent component, UIKeyEventParameter p)
        {
            if (p == null || p.used)
            {
                return;
            }

            if (IsTabKey(p))
            {
                p.Use();
            }
        }

        private static void OnTabKeyDown(UIComponent component, UIKeyEventParameter p)
        {
            if (p == null || p.used || component == null || !component.hasFocus)
            {
                return;
            }

            if (IsTabKey(p))
            {
                p.Use();
                FocusNext(component);
            }
        }

        private static void OnTabKeyUp(UIComponent component, UIKeyEventParameter p)
        {
            if (p == null || p.used)
            {
                return;
            }

            if (IsTabKey(p))
            {
                p.Use();
            }
        }

        private static bool IsConfirmKey(UIKeyEventParameter p)
        {
            return p.keycode == KeyCode.Return
                || p.keycode == KeyCode.KeypadEnter
                || p.character == '\r'
                || p.character == '\n';
        }

        private static bool IsTabKey(UIKeyEventParameter p)
        {
            return p.keycode == KeyCode.Tab;
        }

        private static void FocusNext(UIComponent from)
        {
            if (from == null)
            {
                return;
            }

            int frame = Time.frameCount;
            if (frame == _tabAdvanceFrame)
            {
                return;
            }

            PruneTabStops();

            object scope = ScopeOf(from);
            List<UIComponent> peers = new List<UIComponent>();
            for (int i = 0; i < TabStops.Count; i++)
            {
                UIComponent c = TabStops[i];
                if (c == null || !c.isVisible || !c.isEnabled || !c.canFocus)
                {
                    continue;
                }

                if (!Equals(TabScopes[i], scope))
                {
                    continue;
                }

                peers.Add(c);
            }

            if (peers.Count == 0)
            {
                from.Unfocus();
                return;
            }

            peers.Sort(CompareTabStops);

            int current = -1;
            for (int i = 0; i < peers.Count; i++)
            {
                if (ReferenceEquals(peers[i], from))
                {
                    current = i;
                    break;
                }
            }

            UIComponent next = current < 0 ? peers[0] : peers[(current + 1) % peers.Count];
            if (ReferenceEquals(next, from))
            {
                from.Unfocus();
                return;
            }

            _tabAdvanceFrame = frame;
            next.Focus();
        }

        private static object ScopeOf(UIComponent component)
        {
            for (int i = 0; i < TabStops.Count; i++)
            {
                if (ReferenceEquals(TabStops[i], component))
                {
                    return TabScopes[i];
                }
            }

            return DefaultTabScope;
        }

        private static int CompareTabStops(UIComponent a, UIComponent b)
        {
            int tab = a.tabIndex.CompareTo(b.tabIndex);
            if (tab != 0)
            {
                return tab;
            }

            return TabStops.IndexOf(a).CompareTo(TabStops.IndexOf(b));
        }

        private static void PruneTabStops()
        {
            for (int i = TabStops.Count - 1; i >= 0; i--)
            {
                if (TabStops[i] == null)
                {
                    TabStops.RemoveAt(i);
                    TabScopes.RemoveAt(i);
                }
            }
        }
    }
}
#endif
