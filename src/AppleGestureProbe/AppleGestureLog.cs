using System.Globalization;
using System.Text;

namespace AppleGestureProbe
{
    /// <summary>Formats AppKit gesture probe lines (stderr). No camera binding.</summary>
    public static class AppleGestureLog
    {
        /// <summary>Wire-format line prefix shared by probe output and tests.</summary>
        public const string LinePrefix = "apple src=";

        public static string Format(
            string source,
            string type,
            string phase,
            string mods,
            string momentum,
            double? scrollingDeltaX,
            double? scrollingDeltaY,
            double? deltaX,
            double? deltaY,
            bool? precise,
            double? magnification,
            double? rotation
        )
        {
            var sb = new StringBuilder();
            sb.Append(LinePrefix)
                .Append(source)
                .Append(" type=")
                .Append(type)
                .Append(" phase=")
                .Append(phase)
                .Append(" mods=")
                .Append(mods);

            if (momentum != null)
            {
                sb.Append(" momentum=").Append(momentum);
            }

            AppendDouble(sb, "sdx", scrollingDeltaX, "0.0000");
            AppendDouble(sb, "sdy", scrollingDeltaY, "0.0000");
            AppendDouble(sb, "dx", deltaX, "0.0000");
            AppendDouble(sb, "dy", deltaY, "0.0000");

            if (precise != null)
            {
                sb.Append(" precise=").Append(precise.Value ? "1" : "0");
            }

            AppendDouble(sb, "mag", magnification, "0.00000");
            AppendDouble(sb, "rot", rotation, "0.0000");

            return sb.ToString();
        }

        private static void AppendDouble(
            StringBuilder sb,
            string name,
            double? value,
            string format
        )
        {
            if (value == null)
            {
                return;
            }

            sb.Append(' ')
                .Append(name)
                .Append('=')
                .Append(value.Value.ToString(format, CultureInfo.InvariantCulture));
        }
    }
}
