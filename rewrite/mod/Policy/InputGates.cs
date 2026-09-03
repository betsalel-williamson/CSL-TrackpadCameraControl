namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Single policy for mod apply and Harmony vanilla patches.</summary>
    public static class InputGates
    {
        private static IGameUiContext _context;

        public static IGameUiContext Context
        {
            get { return _context ?? GameUiContext.Default; }
            set { _context = value; }
        }

        internal static IGameUiContext ContextOrNull
        {
            get { return _context; }
        }

        public static void SyncFrameState()
        {
            VanillaCameraSuppress.MenuOrOverUi = IsMenuOrOverUi();
        }

        public static bool ShouldBlockAllCameraInput()
        {
            return ModRuntime.IsModActive() && !IsGameFocused();
        }

        public static bool IsModWorldPathActive()
        {
            return ModRuntime.IsModActive() && IsGameFocused() && !IsMenuOrOverUi();
        }

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

        /// <summary>
        /// Vanilla mouse path: only block when the game is unfocused and the mod is active.
        /// Orbit flush still runs from the mouse-events Postfix when world path is active.
        /// </summary>
        public static bool ShouldRunVanillaMouseEvents()
        {
            return !ShouldBlockAllCameraInput();
        }

        public static bool ShouldFlushPendingOrbit()
        {
            return IsModWorldPathActive();
        }

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
