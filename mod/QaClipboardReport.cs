using System;
using System.Text;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Debug panel copy buffer for build identity and optional QA system context.
    /// </summary>
    internal static class QaClipboardReport
    {
        internal static string Format(bool includeSystemInfo)
        {
            StringBuilder sb = new StringBuilder();
            string footer = Mod.GetBuildInfoFooterDisplay();
            if (!string.IsNullOrEmpty(footer))
            {
                sb.AppendLine(footer);
            }

            string version = Mod.GetProductVersionDisplay();
            if (!string.IsNullOrEmpty(version))
            {
                sb.AppendLine("Mod: " + version);
            }

            if (includeSystemInfo)
            {
                MacQaSystemInfo.AppendSection(sb);
            }

            if (sb.Length == 0)
            {
                return string.Empty;
            }

            if (sb[sb.Length - 1] == '\n')
            {
                sb.Length -= 1;
            }

            return sb.ToString();
        }
    }
}
