#if HAS_CITIES
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    internal static class NumericTextFieldUi
    {
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
        /// </summary>
        public static void WireConfirmKeys(UITextField field)
        {
            if (field == null)
            {
                return;
            }

            field.submitOnFocusLost = true;
            field.eventKeyDown += OnConfirmKey;
            field.eventKeyUp += OnConfirmKey;
        }

        private static void OnConfirmKey(UIComponent component, UIKeyEventParameter p)
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

        /// <summary>Move focus to the next higher <see cref="UIComponent.tabIndex"/> (wraps).</summary>
        private static void FocusNext(UIComponent from)
        {
            if (from == null)
            {
                return;
            }

            UIView view = from.GetUIView();
            if (view == null)
            {
                from.Unfocus();
                return;
            }

            UIComponent[] components = view.GetComponentsInChildren<UIComponent>();
            if (components == null || components.Length == 0)
            {
                from.Unfocus();
                return;
            }

            int current = from.tabIndex;
            UIComponent best = null;
            int bestIndex = int.MaxValue;
            UIComponent wrap = null;
            int wrapIndex = int.MaxValue;

            for (int i = 0; i < components.Length; i++)
            {
                UIComponent c = components[i];
                if (c == null || c == from || !c.isVisible || !c.isEnabled || !c.canFocus)
                {
                    continue;
                }

                if (c.tabIndex < 0)
                {
                    continue;
                }

                if (c.tabIndex > current && c.tabIndex < bestIndex)
                {
                    best = c;
                    bestIndex = c.tabIndex;
                }

                if (c.tabIndex < wrapIndex)
                {
                    wrap = c;
                    wrapIndex = c.tabIndex;
                }
            }

            UIComponent next = best != null ? best : wrap;
            if (next != null)
            {
                next.Focus();
            }
            else
            {
                from.Unfocus();
            }
        }
    }
}
#endif
