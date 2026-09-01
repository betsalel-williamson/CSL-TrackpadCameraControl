using System;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Single policy for mod apply, Harmony vanilla patches, and AppKit capture gates.
    /// <see cref="VanillaCameraSuppress"/> holds per-frame flags only.
    /// </summary>
    public static class InputGates
    {
        private static IGameUiContext _context;

        /// <summary>Game UI / focus probe seam. Null uses <see cref="GameUiContext.Default"/>.</summary>
        public static IGameUiContext Context
        {
            get { return _context ?? GameUiContext.Default; }
            set { _context = value; }
        }

        internal static IGameUiContext ContextOrNull
        {
            get { return _context; }
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
            return ModRuntime.IsModActive() && !IsGameFocused();
        }

        /// <summary>
        /// Mod on + focused + world (not menu/popup): trackpad apply and orbit flush allowed.
        /// </summary>
        public static bool IsModWorldPathActive()
        {
            return ModRuntime.IsModActive() && IsGameFocused() && !IsMenuOrOverUi();
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
            return ModRuntime.IsModActive() && preciseTrackpad && !menuOrOverUi;
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
            return ModRuntime.IsModActive() && rotateBindingHeld;
        }

        /// <summary>Harmony orbit postfix: flush queued trackpad orbit only on world path.</summary>
        public static bool ShouldFlushPendingOrbit()
        {
            return IsModWorldPathActive();
        }

        /// <summary>AppKit capture: enqueue only when mod on and focused.</summary>
        public static bool ShouldCaptureGestures()
        {
            return ModRuntime.IsModActive() && IsGameFocused();
        }

        public static bool IsMenuOrOptionsOpen()
        {
            return Context.IsMenuOrOptionsOpen();
        }

        public static bool IsPointerOverUi()
        {
            return Context.IsPointerOverUi();
        }

        public static bool IsGameFocused()
        {
            return Context.IsGameFocused();
        }

        public static bool IsMenuOrOverUi()
        {
            return IsMenuOrOptionsOpen() || IsPointerOverUi();
        }
    }
}
