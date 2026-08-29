namespace TrackpadCameraControl
{
    /// <summary>Poll gesture source and apply camera ops. Safe to call every simulation frame.</summary>
    public sealed class GesturePipeline
    {
        private readonly ModSettings _settings;
        private IGestureSource _source;
        private int _reconnectCooldown;

        public GesturePipeline(ModSettings settings, IGestureSource source)
        {
            _settings = settings ?? new ModSettings();
            _source = source ?? new InProcessGestureSource();
        }

        public IGestureSource Source => _source;

        public bool IsConnected => _source != null && _source.IsConnected;

        public void SetSource(IGestureSource source)
        {
            if (_source != null)
            {
                _source.Disconnect();
            }

            _source = source ?? new InProcessGestureSource();
        }

        public void Tick()
        {
            if (_settings == null || !_settings.BridgeEnabled)
            {
                return;
            }

            if (!_source.IsConnected)
            {
                if (_reconnectCooldown > 0)
                {
                    _reconnectCooldown--;
                    return;
                }

                _source.Connect();
                if (!_source.IsConnected)
                {
                    _reconnectCooldown = 60; // ~1s at 60fps
                    return;
                }
            }

            int safety = 32;
            while (safety-- > 0 && _source.TryDequeue(out GestureFrame frame))
            {
                CameraOp op = GestureBindingResolver.Resolve(frame, _settings);
                if (op == CameraOp.None)
                {
                    continue;
                }

                CameraApplicator.Apply(
                    op,
                    frame.centroidDeltaX,
                    frame.centroidDeltaY,
                    frame.pinchScaleDelta,
                    frame.rotateDelta,
                    _settings
                );
            }
        }

        public void Shutdown()
        {
            if (_source != null)
            {
                _source.Disconnect();
            }
        }
    }
}
