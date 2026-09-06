using System;
using System.Globalization;
using System.IO;
using System.Text;
using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>File protocol for local in-game e2e when inject mode is on.</summary>
    public static class E2eInjectFileProtocol
    {
        public const string RequestFileName = "e2e-inject-request";
        public const string ResultFileName = "e2e-inject-result";

        public static string ModDirectory()
        {
            try
            {
                return Path.GetDirectoryName(typeof(FeelMath).Assembly.Location);
            }
            catch
            {
                return null;
            }
        }

        public static void Poll(InjectGestureSource inject, ICameraController camera)
        {
            if (inject == null)
            {
                return;
            }

            string dir = ModDirectory();
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            string path = Path.Combine(dir, RequestFileName);
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                string text = File.ReadAllText(path, Encoding.UTF8).Trim();
                File.Delete(path);
                if (
                    !float.TryParse(
                        text,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float pinch
                    )
                )
                {
                    return;
                }

                inject.Enqueue(
                    new GestureFrame
                    {
                        magic = GestureFrame.Magic,
                        version = GestureFrame.Version,
                        fingerCount = 2,
                        phase = (int)GesturePhase.Changed,
                        pinchScaleDelta = pinch,
                    }
                );
            }
            catch
            {
                // fail soft
            }
        }

        public static void WriteResult(ICameraController camera)
        {
            if (camera == null)
            {
                return;
            }

            string dir = ModDirectory();
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            try
            {
                File.WriteAllText(
                    Path.Combine(dir, ResultFileName),
                    camera.Size.ToString("R", CultureInfo.InvariantCulture),
                    Encoding.UTF8
                );
            }
            catch
            {
                // fail soft
            }
        }
    }
}
