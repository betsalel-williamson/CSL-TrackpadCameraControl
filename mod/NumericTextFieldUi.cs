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
        /// Fields that opt into confirm-key / Tab cycling. Used so Tab wraps among
        /// value fields even when Colossal left every <c>tabIndex</c> at the default.
        /// </summary>
        private static readonly List<UITextField> ConfirmFields = new List<UITextField>();

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
        /// also move). After the last wired field, focus wraps to the first.
        /// </summary>
        public static void WireConfirmKeys(UITextField field)
        {
            if (field == null)
            {
                return;
            }

            field.submitOnFocusLost = true;
            field.canFocus = true;
            RegisterConfirmField(field);
            field.eventKeyDown += OnConfirmKeyDown;
            field.eventKeyUp += OnConfirmKeyUp;
        }

        private static void RegisterConfirmField(UITextField field)
        {
            for (int i = 0; i < ConfirmFields.Count; i++)
            {
                if (ReferenceEquals(ConfirmFields[i], field))
                {
                    return;
                }
            }

            ConfirmFields.Add(field);
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
        /// Move focus to the next wired confirm field in the same view (by tabIndex, then
        /// registration order). Wraps to the first when on the last.
        /// </summary>
        private static void FocusNext(UIComponent from)
        {
            if (from == null)
            {
                return;
            }

            PruneConfirmFields();

            UIView view = from.GetUIView();
            List<UITextField> peers = new List<UITextField>();
            for (int i = 0; i < ConfirmFields.Count; i++)
            {
                UITextField c = ConfirmFields[i];
                if (c == null || !c.isVisible || !c.isEnabled || !c.canFocus)
                {
                    continue;
                }

                if (view != null && c.GetUIView() != view)
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

            peers.Sort(CompareConfirmFields);

            int current = -1;
            for (int i = 0; i < peers.Count; i++)
            {
                if (ReferenceEquals(peers[i], from))
                {
                    current = i;
                    break;
                }
            }

            UITextField next = current < 0 ? peers[0] : peers[(current + 1) % peers.Count];
            if (ReferenceEquals(next, from))
            {
                from.Unfocus();
                return;
            }

            next.Focus();
        }

        private static int CompareConfirmFields(UITextField a, UITextField b)
        {
            int tab = a.tabIndex.CompareTo(b.tabIndex);
            if (tab != 0)
            {
                return tab;
            }

            return ConfirmFields.IndexOf(a).CompareTo(ConfirmFields.IndexOf(b));
        }

        private static void PruneConfirmFields()
        {
            for (int i = ConfirmFields.Count - 1; i >= 0; i--)
            {
                if (ConfirmFields[i] == null)
                {
                    ConfirmFields.RemoveAt(i);
                }
            }
        }
    }
}
#endif
