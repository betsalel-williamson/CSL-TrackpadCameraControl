using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Preset dirty model and sensitivity writes. One dirty bit; coalesced autosave via store.
    /// Options and Debug hosts share this editor — they do not own preset state.
    /// </summary>
    public sealed class FeelEditor
    {
        private readonly ModSettings _settings;
        private readonly SettingsStore _store;
        private bool _dirty;

        public FeelEditor(ModSettings settings, SettingsStore store)
        {
            _settings = settings ?? new ModSettings();
            _store = store;
        }

        public ModSettings Settings => _settings;

        public SettingsStore Store => _store;

        public bool IsDirty => _dirty;

        public static SettingsStore ActiveStore { get; set; }

        public static event Action SettingsChanged;

        public static void NotifyChanged()
        {
            Action handlers = SettingsChanged;
            if (handlers != null)
            {
                handlers();
            }
        }

        public static void ResetSettingsChangedHandlers()
        {
            SettingsChanged = null;
        }

        public static void FlushStore(bool force)
        {
            SettingsStore store = ActiveStore;
            if (store == null)
            {
                return;
            }

            ModSettings live = Mod.Runtime != null ? Mod.Runtime.Settings : null;
            if (live == null)
            {
                return;
            }

            if (force)
            {
                store.SaveNow(live);
            }
            else
            {
                store.MarkDirtyAndMaybeFlush(live);
            }
        }

        /// <summary>Dropdown select: load Slow / Default / Fast / named / New Preset.</summary>
        public bool LoadPreset(string name)
        {
            if (string.IsNullOrEmpty(name) || _settings == null)
            {
                return false;
            }

            if (string.Equals(name, FeelProfiles.NameSlow, StringComparison.Ordinal))
            {
                FeelProfiles.ApplySlow(_settings);
                _settings.ActiveFeelPresetName = FeelProfiles.NameSlow;
                _dirty = false;
                NotifyChanged();
                return true;
            }

            if (string.Equals(name, FeelProfiles.NameDefault, StringComparison.Ordinal))
            {
                FeelProfiles.ApplyDefault(_settings);
                _settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
                _dirty = false;
                NotifyChanged();
                return true;
            }

            if (string.Equals(name, FeelProfiles.NameFast, StringComparison.Ordinal))
            {
                FeelProfiles.ApplyFast(_settings);
                _settings.ActiveFeelPresetName = FeelProfiles.NameFast;
                _dirty = false;
                NotifyChanged();
                return true;
            }

            if (_store != null && _store.TryLoadUserPreset(name, _settings))
            {
                _dirty = false;
                NotifyChanged();
                return true;
            }

            return false;
        }

        /// <summary>Save as… — enabled conceptually on New Preset; persists named feel snapshot.</summary>
        public bool SaveAs(string name)
        {
            if (string.IsNullOrEmpty(name) || FeelProfiles.IsBuiltInName(name) || _store == null)
            {
                return false;
            }

            if (!_store.SaveUserPreset(name, _settings, _settings))
            {
                return false;
            }

            _settings.ActiveFeelPresetName = name;
            _dirty = false;
            if (_store != null)
            {
                _store.SaveNow(_settings);
            }

            NotifyChanged();
            return true;
        }

        /// <summary>Delete named user preset only; applies Default and persists.</summary>
        public bool DeleteNamedPreset(string name)
        {
            if (string.IsNullOrEmpty(name) || FeelProfiles.IsBuiltInName(name) || _store == null)
            {
                return false;
            }

            if (string.Equals(name, FeelProfiles.NameNewPreset, StringComparison.Ordinal))
            {
                return false;
            }

            if (!_store.DeleteUserPreset(name))
            {
                return false;
            }

            FeelProfiles.ApplyDefault(_settings);
            _settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
            _dirty = false;
            _store.SaveNow(_settings);
            NotifyChanged();
            return true;
        }

        public void ResetToFactory()
        {
            FeelProfiles.ApplyDefault(_settings);
            _settings.ActiveFeelPresetName = FeelProfiles.NameDefault;
            _dirty = false;
            if (_store != null)
            {
                _store.SaveNow(_settings);
            }

            NotifyChanged();
        }

        public void MarkDirty()
        {
            _dirty = true;
            FeelProfiles.EnsureDirtyNewPreset(_settings, _store);
        }

        public void ApplyGain(Action<ModSettings, float> assign, float value)
        {
            if (assign == null || _settings == null)
            {
                return;
            }

            assign(_settings, FeelMath.RoundGain(value));
            MarkDirty();
            if (_store != null)
            {
                _store.MarkDirtyAndMaybeFlush(_settings);
            }

            NotifyChanged();
        }

        /// <summary>Options "Show debug panel" → AssistUiEnabled chrome bit.</summary>
        public void SetShowDebugPanel(bool enabled)
        {
            if (_settings == null)
            {
                return;
            }

            _settings.AssistUiEnabled = enabled;
            if (enabled)
            {
                _settings.DebugPanelDismissed = false;
            }

            MarkDirty();
            if (_store != null)
            {
                _store.MarkDirtyAndMaybeFlush(_settings);
            }

            NotifyChanged();
        }

        /// <summary>Title-bar close on floating Debug panel.</summary>
        public void DismissDebugPanel()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.DebugPanelDismissed = true;
            MarkDirty();
            if (_store != null)
            {
                _store.MarkDirtyAndMaybeFlush(_settings);
            }

            NotifyChanged();
        }

        /// <summary>Persist floating panel position after title-bar drag.</summary>
        public void SaveDebugPanelPosition(float x, float y)
        {
            if (_settings == null)
            {
                return;
            }

            _settings.DebugPanelPosX = x;
            _settings.DebugPanelPosY = y;
            if (_store != null)
            {
                _store.MarkDirtyAndMaybeFlush(_settings);
            }

            NotifyChanged();
        }

        public void ClearDirty()
        {
            _dirty = false;
        }
    }
}
