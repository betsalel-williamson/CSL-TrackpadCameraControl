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
        /// Prevents a second Tab advance in the same frame (keycode + char events, or focus
        /// handoff delivering the same press to the newly focused control).
        /// </summary>
        private static int _tabAdvanceFrame = -1;

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
        /// Tab advances once on key <em>down</em> (not up — up would retarget the newly focused
        /// field and skip a stop). After the last tab stop in the same scope, focus wraps.
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
            // Debug creates fields with AddUIComponent — default builtinKeyNavigation is false,
            // which blocks Colossal digit insert. Options template fields already have it true;
            // set explicitly so both paths type. We only Use() Tab/Enter below.
            field.builtinKeyNavigation = true;
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

            // Swallow leftover Tab up so it cannot retarget the newly focused control.
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
            // KeyCode only — matching character '\t' as well can fire a second event per press.
            return p.keycode == KeyCode.Tab;
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
