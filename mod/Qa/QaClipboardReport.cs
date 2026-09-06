using System;
using System.Text;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Debug panel copy buffer for build identity and optional QA system context.
    /// </summary>
    internal static class QaClipboardReport
    {
        internal static string Format(bool includeSystemInfo)
        {
            StringBuilder sb = new StringBuilder();

            string asm = Mod.GetAssemblyIdentityDisplay();
            if (!string.IsNullOrEmpty(asm))
            {
                sb.AppendLine("TrackpadCameraControl.Rewrite: " + asm);
            }

            string built = Mod.GetBuildInfoFooterDisplay();
            if (!string.IsNullOrEmpty(built))
            {
                sb.AppendLine(built);
            }

            if (includeSystemInfo)
            {
                MacQaSystemInfo.AppendSection(sb);
                // Skip this mod — already on the first line.
                QaAssemblyVersions.AppendSection(sb, includeThisMod: false);
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
