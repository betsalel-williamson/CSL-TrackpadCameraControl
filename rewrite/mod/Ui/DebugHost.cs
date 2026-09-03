using System;
using System.Collections.Generic;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Debug host: floating panel chrome over the same FeelCatalog + shared FeelEditor.
    /// Catalog binding is shared with Options via <see cref="FeelHostBinder"/>.
    /// </summary>
    public static partial class DebugHost
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

        /// <summary>
        /// Ordered descriptor + live value projections — proves Debug is a FeelCatalog skin without Colossal.
        /// </summary>
        public static IList<FeelPanelEntry> BuildPanelModel(FeelEditor editor = null)
        {
            FeelEditor target = editor ?? _editor;
            return FeelHostBinder.BuildPanelModel(target);
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
#if HAS_CITIES
            EnsurePanelBuilt();
#endif
            ApplyVisibility();
        }

        public static void ApplyVisibility()
        {
            ModSettings settings = _editor != null ? _editor.Settings : null;
            if (settings == null)
            {
                IsVisible = false;
#if HAS_CITIES
                ApplyPanelVisibility();
#endif
                return;
            }

            IsVisible = ShouldShowRoot(settings.AssistUiEnabled, settings.DebugPanelDismissed);
#if HAS_CITIES
            ApplyPanelVisibility();
#endif
        }

        /// <summary>Shared visibility rule with shipping TuningPanelHost.</summary>
        public static bool ShouldShowRoot(bool assistEnabled, bool dismissed)
        {
            return assistEnabled && !dismissed;
        }

        /// <summary>Reopen chip when assist is on but user dismissed the panel.</summary>
        public static bool ShouldShowReopen(bool assistEnabled, bool dismissed)
        {
            return assistEnabled && dismissed;
        }

        public static void Destroy()
        {
            if (_settingsHooked)
            {
                FeelEditor.SettingsChanged -= OnSettingsChanged;
                _settingsHooked = false;
            }

#if HAS_CITIES
            DestroyPanel();
#endif
            IsCreated = false;
            IsVisible = false;
            _editor = null;
        }

        private static void OnSettingsChanged()
        {
#if HAS_CITIES
            OnPanelSettingsChanged();
#else
            ApplyVisibility();
#endif
        }
    }
}
