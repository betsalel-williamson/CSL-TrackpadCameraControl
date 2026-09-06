using System;
using TrackpadCameraControl.Rewrite;

namespace TrackpadCameraControl.Rewrite.Tests
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

    /// <summary>Enables / disables rewrite <see cref="Mod"/> for InputGates policies that need IsModActive.</summary>
    internal sealed class ModTestHarness : IDisposable
    {
        private readonly Mod _mod;
        private readonly string _dir;

        public ModTestHarness(ModSettings seedSettings = null)
        {
            _dir = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "tcc-rewrite-harness-" + Guid.NewGuid().ToString("N")
            );
            System.IO.Directory.CreateDirectory(_dir);
            var store = new ModSettingsStore(System.IO.Path.Combine(_dir, "settings.xml"));
            ModOptions.Store = store;
            if (seedSettings != null)
            {
                store.SaveNow(seedSettings);
            }
            else
            {
                store.LoadOrFactory();
            }

            _mod = new Mod();
            _mod.OnEnabled();
        }

        public void Dispose()
        {
            _mod.OnDisabled();
            ModOptions.Store = null;
            try
            {
                if (System.IO.Directory.Exists(_dir))
                {
                    System.IO.Directory.Delete(_dir, true);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    internal static class ModTestState
    {
        public static void Reset()
        {
            new Mod().OnDisabled();
            ModOptions.Store = null;
        }
    }
}
