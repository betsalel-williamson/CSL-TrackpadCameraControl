using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TrackpadCameraControl
{
    /// <summary>Append-only capture log for inspecting in-process gesture frames.</summary>
    public static class GestureCaptureLog
    {
        public const string EnvVar = "TRACKPAD_CAPTURE_LOG";
        public const string DefaultFileName = "trackpad-camera-control.log";

        private static readonly object Gate = new object();
        private static StreamWriter _writer;
        private static bool _failed;
        internal static string PathOverride;

        public static string ResolvePath()
        {
            if (!string.IsNullOrEmpty(PathOverride))
            {
                return PathOverride;
            }

            string env = Environment.GetEnvironmentVariable(EnvVar);
            if (!string.IsNullOrEmpty(env))
            {
                return env;
            }

            string tmp = Environment.GetEnvironmentVariable("TMPDIR");
            if (string.IsNullOrEmpty(tmp))
            {
                tmp = Path.GetTempPath();
            }

            return Path.Combine(tmp, DefaultFileName);
        }

        public static void Line(string message)
        {
            if (message == null)
            {
                return;
            }

            lock (Gate)
            {
                try
                {
                    EnsureWriterUnlocked();
                    if (_writer == null)
                    {
                        return;
                    }

                    _writer.WriteLine(message);
                    _writer.Flush();
                }
                catch
                {
                    _failed = true;
                    CloseUnlocked();
                }
            }
        }

        public static void Frame(string backend, GestureFrame frame)
        {
            Line(
                (backend ?? "capture")
                    + " fingers="
                    + frame.fingerCount
                    + " phase="
                    + frame.phase
                    + " dC=("
                    + frame.centroidDeltaX.ToString("0.####", CultureInfo.InvariantCulture)
                    + ","
                    + frame.centroidDeltaY.ToString("0.####", CultureInfo.InvariantCulture)
                    + ") pinch="
                    + frame.pinchScaleDelta.ToString("0.####", CultureInfo.InvariantCulture)
                    + " rot="
                    + frame.rotateDelta.ToString("0.####", CultureInfo.InvariantCulture)
                    + " mods="
                    + frame.modifiers
            );
        }

        internal static void ResetForTests()
        {
            lock (Gate)
            {
                CloseUnlocked();
                _failed = false;
            }
        }

        private static void EnsureWriterUnlocked()
        {
            if (_writer != null || _failed)
            {
                return;
            }

            string path = ResolvePath();
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _writer = new StreamWriter(
                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                new UTF8Encoding(false)
            );
            _writer.WriteLine("capture log opened path=" + path);
            _writer.Flush();
        }

        private static void CloseUnlocked()
        {
            if (_writer == null)
            {
                return;
            }

            try
            {
                _writer.Dispose();
            }
            catch
            {
                // Fail soft.
            }

            _writer = null;
        }
    }
}
