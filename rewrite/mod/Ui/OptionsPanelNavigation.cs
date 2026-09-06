using System;
using System.Reflection;
#if HAS_CITIES
using ColossalFramework.UI;
#endif

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Open Cities Options focused on this mod's settings category when possible.</summary>
    internal static class OptionsPanelNavigation
    {
#if HAS_CITIES
        /// <summary>
        /// Show Options and select the Trackpad Camera Control mod page (<see cref="Mod.OptionsTitle"/>).
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

            // Game API is SelectMod(string), not SetCategory(string) — SetCategory takes Category enum.
            string title = Mod.OptionsTitle;
            if (!TrySelectMod(panel, title))
            {
                ModLog.Info("options SelectMod failed for: " + title);
            }

            return true;
        }
#endif

        /// <summary>
        /// Invoke game <c>OptionsMainPanel.SelectMod(string)</c> when present (fail-soft).
        /// Kept reflection-based so unit tests can exercise it without Cities assemblies.
        /// </summary>
        internal static bool TrySelectMod(object panel, string modName)
        {
            if (panel == null || string.IsNullOrEmpty(modName))
            {
                return false;
            }

            try
            {
                MethodInfo method = panel
                    .GetType()
                    .GetMethod(
                        "SelectMod",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(string) },
                        null
                    );
                if (method == null)
                {
                    return false;
                }

                method.Invoke(panel, new object[] { modName });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
