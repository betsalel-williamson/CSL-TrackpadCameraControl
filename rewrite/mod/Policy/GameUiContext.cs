namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Production game UI / focus probes (Colossal UI + Unity when available).</summary>
    public sealed class GameUiContext : IGameUiContext
    {
        public static readonly GameUiContext Default = new GameUiContext();

        public bool IsMenuOrOptionsOpen()
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

        public bool IsPointerOverUi()
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

        public bool IsGameFocused()
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
