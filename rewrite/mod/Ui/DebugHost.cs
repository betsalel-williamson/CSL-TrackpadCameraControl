using System;
using System.Collections.Generic;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Debug host: floating panel chrome over the same FeelCatalog + shared FeelEditor.
    /// Under HAS_CITIES a minimal floating panel can be layered later; visibility and
    /// descriptor inventory are wired for tests and Options parity today.
    /// </summary>
    public static class DebugHost
    {
        private static FeelEditor _editor;
        private static bool _settingsHooked;

        public static bool IsCreated { get; private set; }

        /// <summary>True when AssistUiEnabled && !DebugPanelDismissed.</summary>
        public static bool IsVisible { get; private set; }

        public static string Title => Mod.DebugPanelTitle;

        public static FeelEditor Editor => _editor;

        /// <summary>Same catalog descriptors as Options — one inventory, two skins.</summary>
        public static IList<FeelControlDescriptor> BuildDescriptors()
        {
            return OptionsHost.BuildDescriptors();
        }

        /// <summary>Unit-testable toolkit mapping shared with OptionsHost.</summary>
        public static FeelControlKind MapKind(FeelControlKind catalogKind)
        {
            return FeelHostMapping.MapKind(catalogKind);
        }

        public static void EnsureCreated(FeelEditor editor = null)
        {
            if (editor != null)
            {
                _editor = editor;
            }
            else if (_editor == null)
            {
                ModSettings settings = Mod.Settings;
                SettingsStore store =
                    FeelEditor.ActiveStore ?? new SettingsStore(SettingsStore.DefaultPath());
                FeelEditor.ActiveStore = store;
                _editor = new FeelEditor(settings, store);
            }

            if (!_settingsHooked)
            {
                FeelEditor.SettingsChanged += OnSettingsChanged;
                _settingsHooked = true;
            }

            IsCreated = true;
            ApplyVisibility();
        }

        public static void ApplyVisibility()
        {
            ModSettings settings = _editor != null ? _editor.Settings : null;
            if (settings == null)
            {
                IsVisible = false;
                return;
            }

            IsVisible = settings.AssistUiEnabled && !settings.DebugPanelDismissed;
        }

        public static void Destroy()
        {
            if (_settingsHooked)
            {
                FeelEditor.SettingsChanged -= OnSettingsChanged;
                _settingsHooked = false;
            }

            IsCreated = false;
            IsVisible = false;
            _editor = null;
        }

        private static void OnSettingsChanged()
        {
            ApplyVisibility();
        }
    }
}
