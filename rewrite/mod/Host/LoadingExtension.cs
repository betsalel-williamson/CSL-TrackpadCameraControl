#if HAS_CITIES
using ICities;

namespace TrackpadCameraControl.Rewrite
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                if (
                    mode == LoadMode.NewGame
                    || mode == LoadMode.LoadGame
                    || mode == LoadMode.NewGameFromScenario
                    || mode == LoadMode.LoadScenario
                    || mode == LoadMode.NewAsset
                    || mode == LoadMode.LoadAsset
                )
                {
                    Mod.ArmCaptureOnLevelLoaded();
                    DebugHost.EnsureCreated();
                    DebugHost.ApplyVisibility();
                }
            }
            catch
            {
                // fail soft
            }
        }

        public override void OnLevelUnloading()
        {
            try
            {
                FeelEditor.FlushStore(true);
                DebugHost.Destroy();
            }
            catch
            {
                // fail soft
            }
        }
    }
}
#endif
