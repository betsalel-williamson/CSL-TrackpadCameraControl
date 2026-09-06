#if HAS_CITIES
using ICities;

namespace TrackpadCameraControl
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
                    GameFocusActivation.TryActivate();
                    Mod.ArmCaptureOnLevelLoaded();
                    TuningPanelHost.EnsureCreated();
                    TuningPanelHost.ApplyVisibility();
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
                ModOptions.FlushStore(true);
                TuningPanelHost.Destroy();
            }
            catch
            {
                // fail soft
            }
        }
    }
}
#endif
