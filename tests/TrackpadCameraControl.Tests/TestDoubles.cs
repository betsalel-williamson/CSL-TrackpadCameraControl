using System;
using TrackpadCameraControl;

namespace TrackpadCameraControl.Tests
{
    internal sealed class FakeGameUiContext : IGameUiContext
    {
        public bool MenuOrOptionsOpen { get; set; }

        public bool PointerOverUi { get; set; }

        public bool GameFocused { get; set; } = true;

        public bool IsMenuOrOptionsOpen()
        {
            return MenuOrOptionsOpen;
        }

        public bool IsPointerOverUi()
        {
            return PointerOverUi;
        }

        public bool IsGameFocused()
        {
            return GameFocused;
        }
    }

    internal sealed class InputGatesContextScope : IDisposable
    {
        private readonly IGameUiContext _previous;

        public InputGatesContextScope(IGameUiContext context)
        {
            _previous = InputGates.ContextOrNull;
            InputGates.Context = context;
        }

        public void Dispose()
        {
            InputGates.Context = _previous;
        }
    }

    internal sealed class GestureCaptureLogScope : IDisposable
    {
        private readonly Func<string> _previousResolver;
        private readonly string _path;

        public GestureCaptureLogScope(string path)
        {
            _path = path;
            _previousResolver = GestureCaptureLog.PathResolver;
            GestureCaptureLog.PathResolver = () => _path;
            GestureCaptureLog.Close();
        }

        public void Dispose()
        {
            GestureCaptureLog.Close();
            GestureCaptureLog.PathResolver = _previousResolver;
        }
    }
}
