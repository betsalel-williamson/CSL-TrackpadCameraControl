using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Pure helpers for Debug panel in-place refresh (prefer editing controls over Destroy/recreate).
    /// </summary>
    public static class DebugPanelRefresh
    {
        public static bool StringArraysEqual(string[] a, string[] b)
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

        /// <summary>
        /// Non-empty newline-split lines — matches Debug panel op-heading label binding.
        /// </summary>
        public static string[] NonEmptyHeadingLines(string heading)
        {
            string[] raw = SplitHeadingLines(heading);
            if (raw.Length == 0)
            {
                return raw;
            }

            int count = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                if (!string.IsNullOrEmpty(raw[i]))
                {
                    count++;
                }
            }

            if (count == raw.Length)
            {
                return raw;
            }

            var lines = new string[count];
            int w = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                if (string.IsNullOrEmpty(raw[i]))
                {
                    continue;
                }

                lines[w++] = raw[i];
            }

            return lines;
        }

        /// <summary>
        /// Op headings can update in place when the non-empty line count matches the bound labels.
        /// </summary>
        public static bool CanRefreshHeadingInPlace(string heading, int boundLineCount)
        {
            if (boundLineCount <= 0)
            {
                return false;
            }

            return NonEmptyHeadingLines(heading).Length == boundLineCount;
        }

        public static string[] SplitHeadingLines(string heading)
        {
            if (string.IsNullOrEmpty(heading))
            {
                return new string[0];
            }

            return heading.Split('\n');
        }
    }
}
