using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TrackpadCameraControl
{
    /// <summary>
    /// File protocol for local in-game e2e when inject mode is on.
    /// Request: <c>e2e-inject-request</c> (UTF-8 text: pinchScaleDelta float).
    /// Result: <c>e2e-inject-result</c> (UTF-8 text: camera size after apply).
    /// </summary>
    public static class E2eInjectFileProtocol
    {
        public const string RequestFileName = "e2e-inject-request";
        public const string ResultFileName = "e2e-inject-result";

        public static string ModDirectory()
        {
            try
            {
                // Use a type that does not implement ICities interfaces (Mod does when HAS_CITIES).
                return Path.GetDirectoryName(typeof(GestureFrame).Assembly.Location);
            }
            catch
            {
                return null;
            }
        }

        public static void Poll(InjectGestureSource inject, ICameraZoom camera)
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

            string requestPath = Path.Combine(dir, RequestFileName);
            if (!File.Exists(requestPath))
            {
                return;
            }

            try
            {
                string text = File.ReadAllText(requestPath, Encoding.UTF8).Trim();
                File.Delete(requestPath);
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

        public static void WriteResult(ICameraZoom camera)
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
                float size = camera.Size;
                if (float.IsNaN(size))
                {
                    return;
                }

                string path = Path.Combine(dir, ResultFileName);
                File.WriteAllText(path, size.ToString(CultureInfo.InvariantCulture), Encoding.UTF8);
            }
            catch
            {
                // fail soft
            }
        }
    }
}
