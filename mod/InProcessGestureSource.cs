namespace TrackpadCameraControl
{
    /// <summary>Deploy-path placeholder — in-process Multitouch later.</summary>
    public sealed class InProcessGestureSource : IGestureSource
    {
        public bool IsConnected => false;

        public void Connect()
        {
            // Not implemented in MVP.
        }

        public void Disconnect() { }

        public bool TryDequeue(out GestureFrame frame)
        {
            frame = default;
            return false;
        }
    }
}
