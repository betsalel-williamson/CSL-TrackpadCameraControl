using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Preset dirty model and sensitivity writes. One dirty bit; coalesced autosave via store.
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

        public void ClearDirty()
        {
            _dirty = false;
        }
    }
}
