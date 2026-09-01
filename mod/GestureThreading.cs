#if HAS_CITIES
using ICities;

namespace TrackpadCameraControl
{
    public class GestureThreading : ThreadingExtensionBase
    {
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            _ = realTimeDelta;
            _ = simulationTimeDelta;
            try
            {
                Mod.Runtime?.Pipeline?.Tick();
            }
            catch
            {
                // Fail soft every frame.
            }
        }
    }
}
#endif
