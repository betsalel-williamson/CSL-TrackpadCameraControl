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

        /// <summary>Prefix of the first line written when a capture log file is opened.</summary>
        public const string OpenedLinePrefix = "capture log opened";

        private static readonly object Gate = new object();
        private static StreamWriter _writer;
        private static bool _failed;
        private static Func<string> _pathResolver;

        /// <summary>Optional path resolver (defaults to env / temp file).</summary>
        public static Func<string> PathResolver
        {
            get { return _pathResolver; }
            set { _pathResolver = value; }
        }

        public static string ResolvePath()
        {
            if (_pathResolver != null)
            {
                return _pathResolver();
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

        /// <summary>Close the log writer (mod disable / test teardown).</summary>
        public static void Close()
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
            _writer.WriteLine(OpenedLinePrefix + " path=" + path);
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
