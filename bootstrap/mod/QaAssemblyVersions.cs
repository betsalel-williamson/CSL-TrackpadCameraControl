using System;
using System.Reflection;
using System.Text;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl
{
    /// <summary>
    /// Dependency-critical managed assembly versions for QA clipboard parity checks.
    /// </summary>
    internal static class QaAssemblyVersions
    {
        internal static void AppendSection(StringBuilder sb, bool includeThisMod = true)
        {
            sb.AppendLine();
            sb.AppendLine("--- Assemblies ---");
            string unity = FormatUnityRuntimeDisplay();
            if (!string.IsNullOrEmpty(unity))
            {
                sb.AppendLine("Unity: " + unity);
            }

            AppendNamed(sb, "ICities");
            AppendNamed(sb, "CitiesHarmony.API");
            AppendNamed(sb, "0Harmony");
            if (includeThisMod)
            {
                AppendNamed(sb, "TrackpadCameraControl");
            }
        }

        private static void AppendNamed(StringBuilder sb, string simpleName)
        {
            string version = FormatAssemblyVersion(simpleName);
            if (!ShouldEmitVersionLine(version))
            {
                return;
            }

            sb.AppendLine(simpleName + ": " + version);
        }

        /// <summary>Omit Unity default <c>0.0.0.0</c> stamps; keep <c>missing</c> for parity checks.</summary>
        internal static bool ShouldEmitVersionLine(string version)
        {
            return !string.IsNullOrEmpty(version)
                && !string.Equals(version, "0.0.0.0", StringComparison.Ordinal);
        }

        /// <summary>Loaded assembly version, or <c>missing</c>.</summary>
        internal static string FormatAssemblyVersion(string simpleName)
        {
            if (string.IsNullOrEmpty(simpleName))
            {
                return "missing";
            }

            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    AssemblyName name = assemblies[i].GetName();
                    if (
                        name != null
                        && string.Equals(name.Name, simpleName, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        Version v = name.Version;
                        return v != null ? v.ToString() : "missing";
                    }
                }
            }
            catch
            {
                // fail soft
            }

            return "missing";
        }

        internal static string FormatUnityRuntimeDisplay()
        {
#if HAS_CITIES
            try
            {
                string version = Application.unityVersion;
                return string.IsNullOrEmpty(version) ? null : version;
            }
            catch
            {
                return null;
            }
#else
            return null;
#endif
        }
    }
}
