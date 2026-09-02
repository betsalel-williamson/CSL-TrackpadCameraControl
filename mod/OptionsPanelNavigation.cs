using System;
using System.Reflection;
#if HAS_CITIES
using ColossalFramework.UI;
#endif

namespace TrackpadCameraControl
{
    /// <summary>Open Cities Options focused on this mod's settings category when possible.</summary>
    internal static class OptionsPanelNavigation
    {
#if HAS_CITIES
        /// <summary>
        /// Show Options and select the Trackpad Camera Control category (<see cref="Mod.OptionsTitle"/>).
        /// Returns false if the modal could not be shown.
        /// </summary>
        public static bool OpenModOptions()
        {
            if (UIView.library == null)
            {
                return false;
            }

            OptionsMainPanel panel = UIView.library.ShowModal<OptionsMainPanel>("OptionsPanel");
            if (panel == null)
            {
                panel = UIView.library.Get<OptionsMainPanel>("OptionsPanel");
            }

            if (panel == null)
            {
                return false;
            }

            TrySetCategory(panel, Mod.OptionsTitle);
            return true;
        }
#endif

        /// <summary>
        /// Invoke game <c>OptionsMainPanel.SetCategory(string)</c> when present (fail-soft).
        /// Kept reflection-based so unit tests can exercise it without Cities assemblies.
        /// </summary>
        internal static bool TrySetCategory(object panel, string category)
        {
            if (panel == null || string.IsNullOrEmpty(category))
            {
                return false;
            }

            try
            {
                MethodInfo method = panel
                    .GetType()
                    .GetMethod(
                        "SetCategory",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(string) },
                        null
                    );
                if (method == null)
                {
                    return false;
                }

                method.Invoke(panel, new object[] { category });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
