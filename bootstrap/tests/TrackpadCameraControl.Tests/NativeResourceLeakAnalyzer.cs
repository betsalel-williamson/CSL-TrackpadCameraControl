using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Source pairing scan for unmanaged resources. Complements runtime tests;
    /// does not prove every control-flow path releases.
    /// </summary>
    internal static class NativeResourceLeakAnalyzer
    {
        private static readonly string[] ScanRoots =
        {
            "mod",
            "src/TrackpadCapture",
            "src/TrackpadBridge",
            "src/AppleGestureProbe",
        };

        public static List<string> AnalyzeTree(string repoRoot)
        {
            var findings = new List<string>();
            foreach (string relative in ScanRoots)
            {
                string dir = Path.Combine(repoRoot, relative);
                if (!Directory.Exists(dir))
                {
                    continue;
                }

                foreach (
                    string file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
                )
                {
                    if (
                        file.Contains(
                            Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar
                        )
                        || file.Contains(
                            Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar
                        )
                    )
                    {
                        continue;
                    }

                    string rel = file.Substring(repoRoot.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Replace('\\', '/');
                    findings.AddRange(AnalyzeSource(rel, File.ReadAllText(file)));
                }
            }

            return findings;
        }

        public static List<string> AnalyzeSource(string path, string source)
        {
            var findings = new List<string>();
            if (source == null)
            {
                return findings;
            }

            int gchAlloc = 0;
            int gchFree = 0;
            int cfCreate = 0;
            int cfRelease = 0;
            int deviceStart = 0;
            int deviceStop = 0;
            int addMonitor = 0;
            int removeMonitor = 0;
            bool gchandleField = false;
            bool idisposable = false;

            string[] lines = source.Replace("\r\n", "\n").Split('\n');
            foreach (string raw in lines)
            {
                bool leakOk = raw.IndexOf("native-leak-ok:", StringComparison.Ordinal) >= 0;
                string line = StripLineComment(raw);
                if (line.IndexOf("extern ", StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                if (line.IndexOf("delegate ", StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                if (Regex.IsMatch(line, @"\bGCHandle\s+_?[A-Za-z]\w*\s*;"))
                {
                    gchandleField = true;
                }

                if (line.IndexOf("IDisposable", StringComparison.Ordinal) >= 0)
                {
                    idisposable = true;
                }

                if (line.IndexOf("GCHandle.Alloc", StringComparison.Ordinal) >= 0 && !leakOk)
                {
                    gchAlloc++;
                }

                if (Regex.IsMatch(line, @"\.Free\s*\(\s*\)"))
                {
                    gchFree++;
                }

                if (IsCfStringCreate(line) && !leakOk)
                {
                    cfCreate++;
                }

                if (line.IndexOf("CFRelease(", StringComparison.Ordinal) >= 0)
                {
                    cfRelease++;
                }

                if (line.IndexOf(".DeviceStart(", StringComparison.Ordinal) >= 0 && !leakOk)
                {
                    deviceStart++;
                }

                if (line.IndexOf(".DeviceStop(", StringComparison.Ordinal) >= 0)
                {
                    deviceStop++;
                }

                if (
                    line.IndexOf("addLocalMonitorForEventsMatchingMask", StringComparison.Ordinal)
                        >= 0
                    && !leakOk
                )
                {
                    addMonitor++;
                }

                if (line.IndexOf("removeMonitor:", StringComparison.Ordinal) >= 0)
                {
                    removeMonitor++;
                }
            }

            if (gchAlloc > gchFree)
            {
                findings.Add(
                    path + ": GCHandle.Alloc (" + gchAlloc + ") exceeds Free() (" + gchFree + ")"
                );
            }

            if (gchandleField && !idisposable)
            {
                findings.Add(path + ": GCHandle field requires IDisposable");
            }

            if (cfCreate > cfRelease)
            {
                findings.Add(
                    path
                        + ": CFStringCreateWithCString/CreateCfString ("
                        + cfCreate
                        + ") exceeds CFRelease ("
                        + cfRelease
                        + ")"
                );
            }

            if (deviceStart > deviceStop)
            {
                findings.Add(
                    path
                        + ": DeviceStart ("
                        + deviceStart
                        + ") exceeds DeviceStop ("
                        + deviceStop
                        + ")"
                );
            }

            if (addMonitor > removeMonitor)
            {
                findings.Add(
                    path
                        + ": addLocalMonitor ("
                        + addMonitor
                        + ") exceeds removeMonitor ("
                        + removeMonitor
                        + ")"
                );
            }

            return findings;
        }

        public static string FindRepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "TrackpadCameraControl.sln")))
                {
                    return dir;
                }

                dir = Path.GetDirectoryName(dir);
            }

            throw new InvalidOperationException(
                "TrackpadCameraControl.sln not found from test host"
            );
        }

        private static bool IsCfStringCreate(string line)
        {
            if (line.IndexOf("CFStringCreateWithCString(", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (Regex.IsMatch(line, @"IntPtr\s+CreateCfString\s*\("))
            {
                return false;
            }

            if (line.IndexOf("CreateCfString(", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }

        private static string StripLineComment(string raw)
        {
            int i = 0;
            bool inString = false;
            while (i < raw.Length - 1)
            {
                char c = raw[i];
                if (c == '"' && (i == 0 || raw[i - 1] != '\\'))
                {
                    inString = !inString;
                }
                else if (!inString && c == '/' && raw[i + 1] == '/')
                {
                    return raw.Substring(0, i);
                }

                i++;
            }

            return raw;
        }
    }
}
