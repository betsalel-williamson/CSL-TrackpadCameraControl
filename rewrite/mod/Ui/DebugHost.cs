using System.Collections.Generic;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Debug host: floating panel chrome over the same FeelCatalog.</summary>
    public static class DebugHost
    {
        public static bool IsCreated { get; private set; }

        public static string Title => Mod.DebugPanelTitle;

        /// <summary>Same catalog descriptors as Options — one inventory, two skins.</summary>
        public static IList<FeelControlDescriptor> BuildDescriptors()
        {
            return OptionsHost.BuildDescriptors();
        }

        public static void EnsureCreated()
        {
            IsCreated = true;
        }

        public static void ApplyVisibility()
        {
            // Visibility driven by AssistUiEnabled / DebugPanelDismissed when Cities UI is present.
        }

        public static void Destroy()
        {
            IsCreated = false;
        }
    }
}
