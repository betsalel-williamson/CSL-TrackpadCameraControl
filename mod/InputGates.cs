using System;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Single policy for mod apply, Harmony vanilla patches, and AppKit capture gates.
    /// <see cref="VanillaCameraSuppress"/> holds per-frame flags only.
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

        /// <summary>Sync suppress flags once per pipeline tick before apply/Harmony reads them.</summary>
        public static void SyncFrameState()
        {
            VanillaCameraSuppress.MenuOrOverUi = IsMenuOrOverUi();
        }

        /// <summary>
        /// Mod on + unfocused: block mod apply, vanilla Harmony handlers, capture, and orbit flush.
        /// </summary>
        public static bool ShouldBlockAllCameraInput()
        {
            return VanillaCameraSuppress.Enabled && !IsGameFocused();
        }

        /// <summary>
        /// Mod on + focused + world (not menu/popup): trackpad apply and orbit flush allowed.
        /// </summary>
        public static bool IsModWorldPathActive()
        {
            return VanillaCameraSuppress.Enabled && IsGameFocused() && !IsMenuOrOverUi();
        }

        /// <summary>Clear sticky suppress/orbit state after focus loss.</summary>
        public static void DisarmTransientCameraState(ICameraController camera)
        {
            VanillaCameraSuppress.PreciseTrackpadScroll = false;
            camera?.ClearPendingAngleVelocity();
        }

        public static bool ShouldSkipModCamera(ModSettings settings)
        {
            if (ShouldBlockAllCameraInput())
            {
                return true;
            }

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

        /// <summary>Harmony scroll prefix: false = block vanilla scroll handler.</summary>
        public static bool ShouldRunVanillaScrollWheel()
        {
            if (ShouldBlockAllCameraInput())
            {
                return false;
            }

            return !ShouldSuppressVanillaScrollWheel(
                VanillaCameraSuppress.PreciseTrackpadScroll,
                VanillaCameraSuppress.MenuOrOverUi
            );
        }

        public static bool ShouldSuppressVanillaScrollWheel(bool preciseTrackpad, bool menuOrOverUi)
        {
            return VanillaCameraSuppress.Enabled && preciseTrackpad && !menuOrOverUi;
        }

        /// <summary>Harmony mouse prefix: false = block edge pan / mouse rotate.</summary>
        public static bool ShouldRunVanillaMouseEvents(bool rotateBindingHeld)
        {
            if (ShouldBlockAllCameraInput())
            {
                return false;
            }

            return !ShouldSuppressVanillaMouseRotate(rotateBindingHeld);
        }

        public static bool ShouldSuppressVanillaMouseRotate(bool rotateBindingHeld)
        {
            return VanillaCameraSuppress.Enabled && rotateBindingHeld;
        }

        /// <summary>Harmony orbit postfix: flush queued trackpad orbit only on world path.</summary>
        public static bool ShouldFlushPendingOrbit()
        {
            return IsModWorldPathActive();
        }

        /// <summary>AppKit capture: enqueue only when mod on and focused.</summary>
        public static bool ShouldCaptureGestures()
        {
            return VanillaCameraSuppress.Enabled && IsGameFocused();
        }

        public static bool IsMenuOrOptionsOpen()
        {
            if (MenuOpenOverride != null)
            {
                return MenuOpenOverride();
            }

            return DetectMenuOrOptionsOpen();
        }

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
