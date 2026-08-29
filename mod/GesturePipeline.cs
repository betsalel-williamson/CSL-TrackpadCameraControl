namespace TrackpadCameraControl
{
    /// <summary>Poll gesture source and apply camera ops. Safe to call every simulation frame.</summary>
    public sealed class GesturePipeline
    {
        private readonly ModSettings _settings;
        private readonly ICameraController _camera;
        private readonly GestureSession _session = new GestureSession();
        private IGestureSource _source;
        private int _reconnectCooldown;

        public GesturePipeline(ModSettings settings, IGestureSource source)
            : this(settings, source, new CameraControllerZoom()) { }

        public GesturePipeline(
            ModSettings settings,
            IGestureSource source,
            ICameraController camera
        )
        {
            _settings = settings ?? new ModSettings();
            _source = source ?? new InProcessGestureSource();
            _camera = camera ?? new CameraControllerZoom();
        }

        public IGestureSource Source => _source;

        public ICameraController Camera => _camera;

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

            EnsureInjectSourceIfArmed();

            if (_source is InjectGestureSource inject)
            {
                E2eInjectFileProtocol.Poll(inject, _camera);
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
            bool applied = false;
            while (safety-- > 0 && _source.TryDequeue(out GestureFrame frame))
            {
                CameraOp ops = _session.Process(frame, _settings);
                if (ops == CameraOp.None)
                {
                    continue;
                }

                CameraApplicator.Apply(
                    ops,
                    frame.centroidDeltaX,
                    frame.centroidDeltaY,
                    frame.pinchScaleDelta,
                    frame.rotateDelta,
                    _settings,
                    _camera
                );
                applied = true;
            }

            if (applied && _source is InjectGestureSource)
            {
                E2eInjectFileProtocol.WriteResult(_camera);
            }
        }

        public void Shutdown()
        {
            if (_source != null)
            {
                _source.Disconnect();
            }
        }

        /// <summary>
        /// Hot-swap to inject when the e2e flag appears while the game is already running
        /// (smoke script arms flags after the mod may already be enabled).
        /// </summary>
        private void EnsureInjectSourceIfArmed()
        {
            if (_source is InjectGestureSource)
            {
                return;
            }

            if (!Mod.IsE2eInjectEnabled())
            {
                return;
            }

            try
            {
                var inject = new InjectGestureSource();
                inject.Connect();
                SetSource(inject);
                Mod.InjectSource = inject;
            }
            catch
            {
                // fail soft
            }
        }
    }
}
