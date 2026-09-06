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

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            _ = realTimeDelta;
            _ = simulationTimeDelta;
            try
            {
                DebugHost.ProcessPanelFocusVisual();
            }
            catch
            {
                // fail soft
            }
        }
    }
}
#endif
