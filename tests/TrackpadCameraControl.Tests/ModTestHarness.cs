using TrackpadCameraControl;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Enables the mod through the real OnEnabled/OnDisabled lifecycle for policy tests.
    /// </summary>
    internal sealed class ModTestHarness : System.IDisposable
    {
        private readonly Mod _mod;

        public ModTestHarness(ModSettings settings = null)
        {
            Mod.SetSettingsForTests(settings ?? new ModSettings());
            _mod = new Mod();
            _mod.OnEnabled();
        }

        public void Dispose()
        {
            _mod.OnDisabled();
            Mod.ClearSettingsForTests();
        }
    }
}
