#if HAS_CITIES
using System;
using ColossalFramework.UI;

namespace TrackpadCameraControl
{
    internal static partial class TuningPanelHost
    {
        private static int _nextTabIndex;

        private static void ResetTabOrder()
        {
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
            component.builtinKeyNavigation = true;
        }

        private static void WireTextFieldSubmit(UITextField field, Action submit)
        {
            if (field == null || submit == null)
            {
                return;
            }

            AssignTabOrder(field);
            field.submitOnFocusLost = true;
            field.eventTextSubmitted += (c, text) => submit();
            // Keypad Enter: unfocus on key down. Tab: advance on key up only (wraps).
            // Colossal only submits on Return by default.
            NumericTextFieldUi.WireConfirmKeys(field);
        }

        private static void WireFloatTextFieldSubmit(UITextField field, Action submit)
        {
            NumericTextFieldUi.ConfigureFloatField(field);
            WireTextFieldSubmit(field, submit);
        }

        private static void SubmitFloatField(
            UITextField field,
            ModSettings s,
            Func<float> get,
            Action<ModSettings, float> apply
        )
        {
            if (!ModOptions.TryApplyFloat(s, field.text, apply))
            {
                field.text = FormatFieldValue(get());
                return;
            }

            field.text = FormatFieldValue(get());
        }
    }
}
#endif
