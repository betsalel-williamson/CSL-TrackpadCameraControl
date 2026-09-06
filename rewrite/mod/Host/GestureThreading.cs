#if HAS_CITIES
using ICities;

namespace TrackpadCameraControl.Rewrite
{
    public class GestureThreading : ThreadingExtensionBase
    {
        public override void OnAfterSimulationTick()
        {
            try
            {
                Mod.Runtime?.Pipeline?.Tick();
            }
            catch
            {
                // fail soft
            }
        }
    }
}
#endif
