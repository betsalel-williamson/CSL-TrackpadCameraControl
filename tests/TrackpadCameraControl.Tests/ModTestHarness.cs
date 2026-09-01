using System;
using System.IO;
using TrackpadCameraControl;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Enables the mod through the real OnEnabled/OnDisabled lifecycle for policy tests.
    /// </summary>
    internal sealed class ModTestHarness : IDisposable
    {
        private readonly Mod _mod;
        private readonly string _dir;

        public ModTestHarness(ModSettings seedSettings = null)
        {
            _dir = Path.Combine(
                Path.GetTempPath(),
                "tcc-mod-harness-" + Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(_dir);
            var store = new ModSettingsStore(Path.Combine(_dir, "settings.xml"));
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
                if (Directory.Exists(_dir))
                {
                    Directory.Delete(_dir, true);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
