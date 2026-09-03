using System;
using System.Globalization;
using System.Text;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Classical float text rules for mod numeric fields (optional leading minus, digits,
    /// at most one decimal separator). Shared by options UI and debug tuning panel.
    /// </summary>
    internal static class NumericFieldInput
    {
        /// <summary>
        /// Filters live UITextField text to an in-progress float token (may end with "." or ",").
        /// </summary>
        public static string SanitizePartialFloatText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            bool hasLeadingMinus = false;
            bool hasDecimalSep = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '0' && c <= '9')
                {
                    sb.Append(c);
                    continue;
                }

                if (c == '-' && sb.Length == 0 && !hasLeadingMinus)
                {
                    hasLeadingMinus = true;
                    sb.Append(c);
                    continue;
                }

                if ((c == '.' || c == ',') && !hasDecimalSep)
                {
                    hasDecimalSep = true;
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// True when text is a complete float literal (not an in-progress partial like "-" or "1.").
        /// </summary>
        public static bool IsCompleteFloatText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            string trimmed = text.Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            int start = 0;
            if (trimmed[0] == '-')
            {
                if (trimmed.Length == 1)
                {
                    return false;
                }

                start = 1;
            }

            int sepIndex = -1;
            char sep = '\0';
            for (int i = start; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (c >= '0' && c <= '9')
                {
                    continue;
                }

                if ((c == '.' || c == ',') && sepIndex < 0)
                {
                    sepIndex = i;
                    sep = c;
                    continue;
                }

                return false;
            }

            if (sepIndex < 0)
            {
                return trimmed.Length > start;
            }

            // "." or "," alone after optional minus is incomplete.
            if (sepIndex == start && sepIndex == trimmed.Length - 1)
            {
                return false;
            }

            if (sepIndex == trimmed.Length - 1)
            {
                return false;
            }

            return true;
        }

        public static bool TryParseFloatText(string text, out float value)
        {
            value = 0f;
            if (!IsCompleteFloatText(text))
            {
                return false;
            }

            string trimmed = text.Trim();
            if (
                float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            )
            {
                return true;
            }

            // Accept locale decimal separators (e.g. "1,5").
            return float.TryParse(
                trimmed,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out value
            );
        }
    }
}
