#if HAS_CITIES
using ColossalFramework.UI;

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
                if (!string.Equals(sanitized, text, System.StringComparison.Ordinal))
                {
                    tf.text = sanitized;
                }
            };
        }
    }
}
#endif
