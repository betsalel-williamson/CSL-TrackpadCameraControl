using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Always-disconnected source for unsupported platforms (non-macOS / missing AppKit).
    /// Lives in the mod assembly so Cities auto-reload does not depend on Gestures.dll version.
    /// </summary>
    public sealed class NoopGestureSource : IGestureSource
    {
        public bool IsConnected => false;

        public void Connect() { }

        public void Disconnect() { }

        public bool TryDequeue(out GestureFrame frame)
        {
            frame = default(GestureFrame);
            return false;
        }
    }
}
