#if HAS_CITIES
using System.Reflection;
using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace TrackpadCameraControl
{
    [HarmonyPatch(typeof(CinematicCameraController), "Update")]
    internal static class CinematicCameraPlaybackPatch
    {
        private static FieldInfo interactiveField;
        private static FieldInfo shortcutField;

        /// <summary>
        /// Replace vanilla abort-on-any-key when <see cref="ModSettings.CinematicBackgroundPlayback"/> is on.
        /// </summary>
        public static bool Prefix(CinematicCameraController __instance)
        {
            ModSettings settings = Mod.Settings;
            if (settings == null || !settings.CinematicBackgroundPlayback)
            {
                return true;
            }

            EnsureFields();
            if (interactiveField == null || shortcutField == null)
            {
                return true;
            }

            bool interactive = (bool)interactiveField.GetValue(__instance);
            if (interactive && Input.GetKeyDown(KeyCode.Escape))
            {
                AbortAndDisable(__instance);
                return false;
            }

            if (!interactive)
            {
                var shortcut = shortcutField.GetValue(__instance) as SavedInputKey;
                bool shortcutPressed = shortcut != null && shortcut.IsPressed();
                if (CinematicCameraInput.ShouldAbortUnityPlayback(Input.anyKey, shortcutPressed))
                {
                    AbortAndDisable(__instance);
                }
            }

            return false;
        }

        private static void AbortAndDisable(CinematicCameraController instance)
        {
            instance.AbortScript();
            instance.enabled = false;
        }

        private static void EnsureFields()
        {
            if (interactiveField != null && shortcutField != null)
            {
                return;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            interactiveField =
                interactiveField
                ?? typeof(CinematicCameraController).GetField("m_Interactive", flags);
            shortcutField =
                shortcutField
                ?? typeof(CinematicCameraController).GetField(
                    "m_ShortcutInGameShortcutCinematicCameraMode",
                    flags
                );
        }
    }
}
#endif
