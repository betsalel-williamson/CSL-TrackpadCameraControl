using System;
using System.IO;
using TrackpadCameraControl;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Clears mod runtime and options store between tests.
    /// </summary>
    internal static class ModTestState
    {
        public static void Reset()
        {
            new Mod().OnDisabled();
            ModOptions.Store = null;
        }
    }

    /// <summary>
    /// Seeds settings through a real <see cref="ModSettingsStore"/> on disk.
    /// </summary>
    internal sealed class ModSettingsTestContext : IDisposable
    {
        private readonly string _dir;

        private ModSettingsTestContext(string dir)
        {
            _dir = dir;
        }

        public static ModSettingsTestContext With(ModSettings settings)
        {
            string dir = Path.Combine(
                Path.GetTempPath(),
                "tcc-settings-ctx-" + Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(dir);
            var store = new ModSettingsStore(Path.Combine(dir, "settings.xml"));
            ModOptions.Store = store;
            store.SaveNow(settings ?? ModSettings.CreateFactoryDefaults());
            Mod.EnsureSettings();
            return new ModSettingsTestContext(dir);
        }

        public void Dispose()
        {
            ModTestState.Reset();
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
