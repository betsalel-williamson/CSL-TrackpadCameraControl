using System;
using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Poll gesture source and apply camera ops. Safe to call every simulation frame.</summary>
    public sealed class GesturePipeline
    {
        private readonly ModSettings _settings;
        private readonly ICameraController _camera;
        private readonly ISelectionContext _selection;
        private readonly GestureSession _session = new GestureSession();
        private IGestureSource _source;
        private int _reconnectCooldown;

        public GesturePipeline(ModSettings settings, IGestureSource source)
            : this(settings, source, new CitiesCameraAdapter(), CitiesSelectionContext.Instance) { }

        public GesturePipeline(
            ModSettings settings,
            IGestureSource source,
            ICameraController camera
        )
            : this(settings, source, camera, CitiesSelectionContext.Instance) { }

        public GesturePipeline(
            ModSettings settings,
            IGestureSource source,
            ICameraController camera,
            ISelectionContext selection
        )
        {
            _settings = settings ?? new ModSettings();
            _source = source ?? CreateDefaultCaptureSource();
            _camera = camera ?? new CitiesCameraAdapter();
            _selection = selection ?? CitiesSelectionContext.Instance;
        }

        public IGestureSource Source => _source;

        public ICameraController Camera => _camera;

        public ISelectionContext Selection => _selection;

        public bool IsConnected => _source != null && _source.IsConnected;

        public void SetSource(IGestureSource source)
        {
            if (_source != null)
            {
                _source.Disconnect();
            }

            _source = source ?? CreateDefaultCaptureSource();
        }

        public void Tick()
        {
            if (_settings == null)
            {
                return;
            }

            EnsureInjectSourceIfArmed();
            if (!(_source is InjectGestureSource))
            {
                EnsureCaptureSource();
            }

            if (_source is InjectGestureSource inject)
            {
                E2eInjectFileProtocol.Poll(inject, _camera);
            }

            InputGates.SyncFrameState();
            if (InputGates.ShouldBlockAllCameraInput())
            {
                InputGates.DisarmTransientCameraState(_camera);
                _session.Reset();
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
                    _reconnectCooldown = 60;
                    return;
                }
            }

            bool skipApply = InputGates.ShouldSkipModCamera(_settings);

            int safety = 32;
            bool applied = false;
            while (safety-- > 0 && _source.TryDequeue(out GestureFrame frame))
            {
                frame = GameModifierKeys.Enrich(frame);

                CameraOp ops = _session.Process(frame, _settings);
                if (ops == CameraOp.None)
                {
                    continue;
                }

                if (skipApply)
                {
                    continue;
                }

                FeelMath.Apply(
                    ops,
                    frame.centroidDeltaX,
                    frame.centroidDeltaY,
                    frame.pinchScaleDelta,
                    frame.rotateDelta,
                    _settings,
                    _camera,
                    _selection
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

        public void ArmCapture()
        {
            if (_source == null)
            {
                return;
            }

            try
            {
                _source.Disconnect();
            }
            catch
            {
                // fail soft
            }

            _reconnectCooldown = 0;
            try
            {
                _source.Connect();
            }
            catch
            {
                // fail soft
            }

            ModLog.Info("gestures armed");
        }

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
                if (Mod.Runtime != null)
                {
                    Mod.Runtime.Inject = inject;
                }
            }
            catch
            {
                // fail soft
            }
        }

        private void EnsureCaptureSource()
        {
            if (_source is AppleGestureSource)
            {
                return;
            }

            try
            {
                IGestureSource next = CreateDefaultCaptureSource();
                next.Connect();
                SetSource(next);
            }
            catch
            {
                SetSource(CreateDefaultCaptureSource());
            }
        }

        internal static IGestureSource CreateDefaultCaptureSource()
        {
            var apple = new AppleGestureSource();
            apple.ShouldCapture = InputGates.ShouldCaptureGestures;
            apple.PreciseScrollChanged = precise =>
            {
                VanillaCameraSuppress.PreciseTrackpadScroll = precise;
            };
            return apple;
        }
    }
}
