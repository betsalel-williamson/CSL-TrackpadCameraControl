namespace TrackpadCameraControl
{
    public interface IGestureSource
    {
        bool IsConnected { get; }

        void Connect();

        void Disconnect();

        /// <summary>Try to read the next frame. Returns false when none available.</summary>
        bool TryDequeue(out GestureFrame frame);
    }
}
