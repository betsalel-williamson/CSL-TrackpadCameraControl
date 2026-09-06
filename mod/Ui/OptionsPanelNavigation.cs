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

            string title = Mod.OptionsTitle;
            TrySelectMod(panel, title);
            return true;
        }
#endif

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
