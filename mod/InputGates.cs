using System;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Gates that skip mod camera apply (menus, pointer-over-UI, unfocused).
    /// Production probes Colossal UI / Unity focus when HAS_CITIES; tests override via hooks.
    /// </summary>
    public static class InputGates
    {
        public static Func<bool> MenuOpenOverride { get; set; }
        public static Func<bool> PointerOverUiOverride { get; set; }
        public static Func<bool> GameFocusedOverride { get; set; }

        public static void ResetTestHooks()
        {
            MenuOpenOverride = null;
            PointerOverUiOverride = null;
            GameFocusedOverride = null;
        }

        /// <summary>
        /// When the mod is on but the game is unfocused, block all camera input (mod and vanilla)
        /// until focus returns.
        /// </summary>
        public static bool ShouldBlockCameraInput()
        {
            return VanillaCameraSuppress.Enabled && !IsGameFocused();
        }

        /// <summary>
        /// Mod may apply trackpad camera ops and run selective vanilla suppress while focused on the world.
        /// </summary>
        public static bool IsModCameraArmed()
        {
            if (!VanillaCameraSuppress.Enabled)
            {
                return false;
            }

            if (!IsGameFocused())
            {
                return false;
            }

            if (IsMenuOrOverUi())
            {
                return false;
            }

            return true;
        }

        /// <summary>Drop sticky mod camera state when the world path is not armed.</summary>
        public static void DisarmTransientCameraState(ICameraController camera)
        {
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            if (camera != null)
            {
                camera.ClearPendingAngleVelocity();
            }
        }

        public static bool ShouldSkipModCamera(ModSettings settings)
        {
            if (settings == null)
            {
                settings = new ModSettings();
            }

            if (IsMenuOrOptionsOpen())
            {
                return true;
            }

            if (settings.IgnoreOverUi && IsPointerOverUi())
            {
                return true;
            }

            if (settings.RequireGameFocus && !IsGameFocused())
            {
                return true;
            }

            return false;
        }

        /// <summary>True when Options or another modal menu is open.</summary>
        public static bool IsMenuOrOptionsOpen()
        {
            if (MenuOpenOverride != null)
            {
                return MenuOpenOverride();
            }

            return DetectMenuOrOptionsOpen();
        }

        /// <summary>True when the pointer is over Colossal UI / an active popup.</summary>
        public static bool IsPointerOverUi()
        {
            if (PointerOverUiOverride != null)
            {
                return PointerOverUiOverride();
            }

            return DetectPointerOverUi();
        }

        public static bool IsGameFocused()
        {
            if (GameFocusedOverride != null)
            {
                return GameFocusedOverride();
            }

            return DetectGameFocused();
        }

        /// <summary>Menu open or pointer over UI — vanilla scroll should remain available.</summary>
        public static bool IsMenuOrOverUi()
        {
            return IsMenuOrOptionsOpen() || IsPointerOverUi();
        }

        private static bool DetectMenuOrOptionsOpen()
        {
#if HAS_CITIES
            try
            {
                if (ColossalFramework.UI.UIView.HasModalInput())
                {
                    return true;
                }

                OptionsMainPanel options =
                    ColossalFramework.UI.UIView.library != null
                        ? ColossalFramework.UI.UIView.library.Get<OptionsMainPanel>("OptionsPanel")
                        : null;
                if (options != null && options.component != null && options.component.isVisible)
                {
                    return true;
                }
            }
            catch
            {
                // fail soft: treat as not open
            }
#endif
            return false;
        }

        private static bool DetectPointerOverUi()
        {
#if HAS_CITIES
            try
            {
                return ColossalFramework.UI.UIView.IsInsideUI();
            }
            catch
            {
                // fail soft
            }
#endif
            return false;
        }

        private static bool DetectGameFocused()
        {
#if HAS_CITIES
            try
            {
                return UnityEngine.Application.isFocused;
            }
            catch
            {
                return true;
            }
#else
            return true;
#endif
        }
    }
}
