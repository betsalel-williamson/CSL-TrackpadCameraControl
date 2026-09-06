#if HAS_CITIES
using System;
using System.Collections.Generic;
using ColossalFramework.UI;

namespace TrackpadCameraControl.Rewrite
{
    public static partial class DebugHost
    {
        private sealed class FloatFieldBinding
        {
            public UITextField Field;
            public Func<float> Get;
        }

        private sealed class CheckBinding
        {
            public UICheckBox Box;
            public Func<bool> Get;
        }

        private static readonly List<FloatFieldBinding> _floatBindings =
            new List<FloatFieldBinding>(24);
        private static readonly List<CheckBinding> _checkBindings = new List<CheckBinding>(4);
        private static int _nextTabIndex;

        private static void ClearRefreshBindings()
        {
            _floatBindings.Clear();
            _checkBindings.Clear();
            _nextTabIndex = 0;
        }

        private static void AssignTabOrder(UIComponent component)
        {
            if (component == null)
            {
                return;
            }

            component.canFocus = true;
            component.tabIndex = _nextTabIndex++;
        }

        private static void RegisterFloatField(UITextField field, Func<float> get)
        {
            if (field == null || get == null)
            {
                return;
            }

            _floatBindings.Add(new FloatFieldBinding { Field = field, Get = get });
        }

        private static void RegisterCheck(UICheckBox box, Func<bool> get)
        {
            if (box == null || get == null)
            {
                return;
            }

            _checkBindings.Add(new CheckBinding { Box = box, Get = get });
        }

        /// <summary>
        /// Update live controls from settings without Destroy/EnsureCreated.
        /// </summary>
        private static bool TryRefreshInPlace()
        {
            if (_root == null || _editor == null || _editor.Settings == null)
            {
                return false;
            }

            ModSettings s = _editor.Settings;
            _handlingSettingsChanged = true;
            try
            {
                RefreshFeelDropdown(s);
                RefreshFloatFields();
                RefreshChecks();
                ApplyPanelVisibility();
                return true;
            }
            finally
            {
                _handlingSettingsChanged = false;
            }
        }

        private static void RefreshFeelDropdown(ModSettings s)
        {
            if (_feelDropdown == null)
            {
                return;
            }

            string[] items = FeelHostBinder.BuildFeelPresetItems(_editor);
            if (!StringArraysEqual(_feelDropdownItems, items))
            {
                _feelDropdownItems = items;
                _feelDropdown.items = items;
            }

            int index = FeelHostBinder.IndexOfFeelPreset(
                _feelDropdownItems,
                s.ActiveFeelPresetName
            );
            if (_feelDropdown.selectedIndex != index)
            {
                _feelDropdown.selectedIndex = index;
            }

            RefreshFeelPresetButtons();
        }

        private static void RefreshFloatFields()
        {
            for (int i = 0; i < _floatBindings.Count; i++)
            {
                FloatFieldBinding binding = _floatBindings[i];
                if (binding == null || binding.Field == null || binding.Get == null)
                {
                    continue;
                }

                string next = FormatFieldValue(binding.Get());
                if (!string.Equals(binding.Field.text, next, StringComparison.Ordinal))
                {
                    binding.Field.text = next;
                }
            }
        }

        private static void RefreshChecks()
        {
            for (int i = 0; i < _checkBindings.Count; i++)
            {
                CheckBinding binding = _checkBindings[i];
                if (binding == null || binding.Box == null || binding.Get == null)
                {
                    continue;
                }

                bool next = binding.Get();
                if (binding.Box.isChecked != next)
                {
                    binding.Box.isChecked = next;
                }
            }
        }

        private static bool StringArraysEqual(string[] a, string[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static void WireFloatTextFieldSubmit(UITextField field, Action submit)
        {
            NumericTextFieldUi.ConfigureFloatField(field);
            AssignTabOrder(field);
            field.submitOnFocusLost = true;
            field.eventTextSubmitted += (c, text) =>
            {
                if (submit != null)
                {
                    submit();
                }
            };
            NumericTextFieldUi.WireConfirmKeys(field, includeInTabOrder: true, tabScope: _root);
        }

        private static void SubmitFloatField(
            UITextField field,
            Func<float> get,
            Action<float> apply
        )
        {
            if (field == null || get == null || apply == null)
            {
                return;
            }

            if (_handlingSettingsChanged)
            {
                return;
            }

            float parsed;
            if (!NumericFieldInput.TryParseFloatText(field.text, out parsed))
            {
                field.text = FormatFieldValue(get());
                return;
            }

            apply(parsed);
            field.text = FormatFieldValue(get());
        }
    }
}
#endif
