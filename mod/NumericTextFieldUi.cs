#if HAS_CITIES
using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    internal static class NumericTextFieldUi
    {
        /// <summary>
        /// Shared scope for Options (and any caller that omits an explicit panel scope).
        /// </summary>
        private static readonly object DefaultTabScope = new object();

        private static readonly List<UIComponent> TabStops = new List<UIComponent>();
        private static readonly List<object> TabScopes = new List<object>();

        /// <summary>
        /// Colossal UITextField has no separate numeric widget; numericalOnly + allowFloats
        /// filter key entry but do not normalize paste or every invalid sequence — we still
        /// sanitize on eventTextChanged.
        /// </summary>
        public static void ConfigureFloatField(UITextField field)
        {
            if (field == null)
            {
                return;
            }

            field.numericalOnly = true;
            field.allowFloats = true;
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

        /// <summary>
        /// Colossal submits on Return only. Keypad Enter and Tab must unfocus (or advance)
        /// so <c>submitOnFocusLost</c> / <c>eventTextSubmitted</c> can confirm the value.
        /// Tab advances on key <em>up</em> only (key down is consumed so builtin nav does not
        /// also move). After the last tab stop in the same scope, focus wraps to the first.
        /// </summary>
        /// <param name="includeInTabOrder">
        /// When false, Enter still confirms but Tab does not visit this field (e.g. Feel name).
        /// </param>
        /// <param name="tabScope">
        /// Group for Tab cycling (Debug panel root, etc.). Null uses the shared Options scope.
        /// </param>
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
            if (includeInTabOrder)
            {
                RegisterTabStop(field, tabScope);
            }

            field.eventKeyDown += OnConfirmKeyDown;
            field.eventKeyUp += OnConfirmKeyUp;
        }

        /// <summary>
        /// Include a non-text focusable (e.g. checkbox) in the same Tab cycle as wired fields.
        /// </summary>
        public static void WireTabStop(UIComponent component, object tabScope = null)
        {
            if (component == null)
            {
                return;
            }

            component.canFocus = true;
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
                // Consume on down so Colossal/builtin does not advance; move on key up only.
                p.Use();
            }
        }

        private static void OnConfirmKeyUp(UIComponent component, UIKeyEventParameter p)
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

            if (IsTabKey(p))
            {
                p.Use();
                FocusNext(field);
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
            }
        }

        private static void OnTabKeyUp(UIComponent component, UIKeyEventParameter p)
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

        private static bool IsConfirmKey(UIKeyEventParameter p)
        {
            return p.keycode == KeyCode.Return
                || p.keycode == KeyCode.KeypadEnter
                || p.character == '\r'
                || p.character == '\n';
        }

        private static bool IsTabKey(UIKeyEventParameter p)
        {
            return p.keycode == KeyCode.Tab || p.character == '\t';
        }

        /// <summary>
        /// Move focus to the next tab stop in the same scope (by tabIndex, then registration).
        /// Wraps to the first when on the last.
        /// </summary>
        private static void FocusNext(UIComponent from)
        {
            if (from == null)
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
