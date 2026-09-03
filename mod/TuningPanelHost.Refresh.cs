#if HAS_CITIES
using System;
using System.Collections.Generic;
using ColossalFramework.UI;

namespace TrackpadCameraControl
{
    internal static partial class TuningPanelHost
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

        private sealed class OpHeadingBinding
        {
            public UILabel[] Lines;
            public Func<string> GetHeading;
        }

        private static readonly List<FloatFieldBinding> _floatBindings =
            new List<FloatFieldBinding>(24);
        private static readonly List<CheckBinding> _checkBindings = new List<CheckBinding>(8);
        private static readonly List<OpHeadingBinding> _opHeadingBindings =
            new List<OpHeadingBinding>(4);

        private static void ClearRefreshBindings()
        {
            _floatBindings.Clear();
            _checkBindings.Clear();
            _opHeadingBindings.Clear();
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

        private static void RegisterOpHeading(UILabel[] lines, Func<string> getHeading)
        {
            if (lines == null || lines.Length == 0 || getHeading == null)
            {
                return;
            }

            _opHeadingBindings.Add(new OpHeadingBinding { Lines = lines, GetHeading = getHeading });
        }

        /// <summary>
        /// Update live controls from settings without Destroy/EnsureCreated.
        /// Returns false when structure changed enough that a full rebuild is required.
        /// </summary>
        private static bool TryRefreshInPlace()
        {
            if (_root == null)
            {
                return false;
            }

            ModSettings s = Mod.EnsureSettings();
            if (s == null)
            {
                return false;
            }

            _handlingSettingsChanged = true;
            try
            {
                if (!RefreshFeelDropdown(s))
                {
                    return false;
                }

                if (!RefreshOpHeadings())
                {
                    return false;
                }

                RefreshFloatFields();
                RefreshChecks();
#if ENABLE_CAD_GESTURE_STYLE
                UpdatePresetDesc(ModOptions.PresetDescription(s.GesturePreset));
#endif
                ApplyVisibility();
                return true;
            }
            finally
            {
                _handlingSettingsChanged = false;
            }
        }

        private static bool RefreshFeelDropdown(ModSettings s)
        {
            if (_feelDropdown == null)
            {
                return true;
            }

            string[] items = ModOptions.GetFeelPresetDropdownItems(s);
            if (!DebugPanelRefresh.StringArraysEqual(_feelDropdownItems, items))
            {
                _feelDropdownItems = items;
                _feelDropdown.items = items;
            }

            int index = ModOptions.IndexOfFeelPresetDropdownItem(
                _feelDropdownItems,
                s.ActiveFeelPresetName
            );
            if (_feelDropdown.selectedIndex != index)
            {
                _feelDropdown.selectedIndex = index;
            }

            return true;
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

        private static bool RefreshOpHeadings()
        {
            for (int i = 0; i < _opHeadingBindings.Count; i++)
            {
                OpHeadingBinding binding = _opHeadingBindings[i];
                if (binding == null || binding.Lines == null || binding.GetHeading == null)
                {
                    continue;
                }

                string heading = binding.GetHeading();
                if (!DebugPanelRefresh.CanRefreshHeadingInPlace(heading, binding.Lines.Length))
                {
                    return false;
                }

                string[] lines = DebugPanelRefresh.NonEmptyHeadingLines(heading);
                for (int line = 0; line < lines.Length; line++)
                {
                    UILabel label = binding.Lines[line];
                    if (label == null)
                    {
                        return false;
                    }

                    if (string.Equals(label.text, lines[line], StringComparison.Ordinal))
                    {
                        continue;
                    }

                    label.text = lines[line];
                    label.PerformLayout();
                }
            }

            return true;
        }
    }
}
#endif
