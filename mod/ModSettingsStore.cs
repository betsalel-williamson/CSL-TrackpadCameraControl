using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Versioned XML settings envelope. Injectable path for tests.
    /// </summary>
    public sealed class ModSettingsStore
    {
        public const int CurrentSchemaVersion = 2;

        /// <summary>
        /// Schema 1 used scroll deltas pre-scaled by 0.01 in AppleGestureMapper and larger
        /// Sensitivity defaults. Schema 2 uses raw scroll deltas; Sensitivity × 0.01.
        /// </summary>
        private const float V1ScrollUnit = 0.01f;

        private readonly string _filePath;
        private DateTime _lastWriteUtc = DateTime.MinValue;
        private bool _dirty;
        private List<NamedPreset> _userPresets = new List<NamedPreset>();
        private bool _presetsHydrated;

        public ModSettingsStore(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("path required", "filePath");
            }

            _filePath = filePath;
        }

        public string FilePath => _filePath;

        public static string DefaultPath()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(root))
            {
                root = Path.GetTempPath();
            }

            return Path.Combine(
                Path.Combine(
                    Path.Combine(root, "Colossal Order"),
                    "Cities_Skylines"
                ),
                Path.Combine("TrackpadCameraControl", "settings.xml")
            );
        }

        public ModSettings LoadOrFactory()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _userPresets = new List<NamedPreset>();
                    _presetsHydrated = true;
                    ModSettings fresh = ModSettings.CreateFactoryDefaults();
                    SaveNow(fresh);
                    return fresh;
                }

                Envelope envelope;
                using (var reader = new StreamReader(_filePath))
                {
                    var serializer = new XmlSerializer(typeof(Envelope));
                    envelope = serializer.Deserialize(reader) as Envelope;
                }

                if (envelope == null || envelope.Current == null)
                {
                    _userPresets = new List<NamedPreset>();
                    _presetsHydrated = true;
                    ModSettings recovered = ModSettings.CreateFactoryDefaults();
                    SaveNow(recovered);
                    return recovered;
                }

                _userPresets =
                    envelope.UserPresets != null
                        ? new List<NamedPreset>(envelope.UserPresets)
                        : new List<NamedPreset>();
                _presetsHydrated = true;

                if (envelope.SchemaVersion < CurrentSchemaVersion)
                {
                    MigrateScrollUnitIntoSensitivity(envelope.Current);
                    for (int i = 0; i < _userPresets.Count; i++)
                    {
                        NamedPreset preset = _userPresets[i];
                        if (preset != null && preset.Settings != null)
                        {
                            MigrateScrollUnitIntoSensitivity(preset.Settings);
                        }
                    }

                    envelope.SchemaVersion = CurrentSchemaVersion;
                    try
                    {
                        SaveNow(envelope.Current);
                    }
                    catch
                    {
                        // keep migrated in-memory even if rewrite fails
                    }
                }

                return envelope.Current;
            }
            catch
            {
                _userPresets = new List<NamedPreset>();
                _presetsHydrated = true;
                ModSettings recovered = ModSettings.CreateFactoryDefaults();
                try
                {
                    SaveNow(recovered);
                }
                catch
                {
                    // ignore secondary write failure
                }

                return recovered;
            }
        }

        /// <summary>Names of named feel profiles in the persist envelope.</summary>
        public string[] ListUserPresetNames()
        {
            EnsurePresetsLoaded();
            var names = new List<string>();
            for (int i = 0; i < _userPresets.Count; i++)
            {
                NamedPreset preset = _userPresets[i];
                if (preset != null && !string.IsNullOrEmpty(preset.Name))
                {
                    names.Add(preset.Name);
                }
            }

            return names.ToArray();
        }

        /// <summary>
        /// Upsert a named feel snapshot into <c>userPresets</c> and rewrite the envelope
        /// (preserves current settings blob when the file already exists).
        /// </summary>
        public bool SaveUserPreset(string name, ModSettings feelSnapshot, ModSettings current)
        {
            if (string.IsNullOrEmpty(name) || feelSnapshot == null)
            {
                return false;
            }

            EnsurePresetsLoaded();

            ModSettings snap = FeelProfiles.SnapshotFeel(feelSnapshot);
            bool replaced = false;
            for (int i = 0; i < _userPresets.Count; i++)
            {
                NamedPreset existing = _userPresets[i];
                if (
                    existing != null
                    && string.Equals(existing.Name, name, StringComparison.Ordinal)
                )
                {
                    existing.Settings = snap;
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                _userPresets.Add(new NamedPreset { Name = name, Settings = snap });
            }

            ModSettings toWrite = current ?? LoadCurrentOrFactoryWithoutResettingPresets();
            SaveNow(toWrite);
            return true;
        }

        /// <summary>Load a named feel preset's settings snapshot (feel fields).</summary>
        public bool TryGetUserPreset(string name, out ModSettings feelSnapshot)
        {
            feelSnapshot = null;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            EnsurePresetsLoaded();
            for (int i = 0; i < _userPresets.Count; i++)
            {
                NamedPreset preset = _userPresets[i];
                if (
                    preset != null
                    && string.Equals(preset.Name, name, StringComparison.Ordinal)
                    && preset.Settings != null
                )
                {
                    feelSnapshot = FeelProfiles.SnapshotFeel(preset.Settings);
                    return true;
                }
            }

            return false;
        }

        /// <summary>Remove a named feel preset from the in-memory list (caller should SaveNow).</summary>
        public bool RemoveUserPreset(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            EnsurePresetsLoaded();
            for (int i = _userPresets.Count - 1; i >= 0; i--)
            {
                NamedPreset preset = _userPresets[i];
                if (
                    preset != null
                    && string.Equals(preset.Name, name, StringComparison.Ordinal)
                )
                {
                    _userPresets.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fold former mapper ScrollToCentroid (0.01) into Sensitivity / motion deadzone so
        /// raw AppKit scroll deltas keep the same feel.
        /// </summary>
        internal static void MigrateScrollUnitIntoSensitivity(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanSensitivityX *= V1ScrollUnit;
            settings.PanSensitivityY *= V1ScrollUnit;
            settings.OrbitYawSensitivity *= V1ScrollUnit;
            settings.OrbitPitchSensitivity *= V1ScrollUnit;
            if (settings.MotionDeadzone > 0f)
            {
                settings.MotionDeadzone /= V1ScrollUnit;
            }
        }

        private void EnsurePresetsLoaded()
        {
            if (_presetsHydrated)
            {
                return;
            }

            if (_userPresets == null)
            {
                _userPresets = new List<NamedPreset>();
            }

            _presetsHydrated = true;

            if (!File.Exists(_filePath))
            {
                return;
            }

            try
            {
                Envelope envelope;
                using (var reader = new StreamReader(_filePath))
                {
                    var serializer = new XmlSerializer(typeof(Envelope));
                    envelope = serializer.Deserialize(reader) as Envelope;
                }

                if (envelope != null && envelope.UserPresets != null)
                {
                    _userPresets = new List<NamedPreset>(envelope.UserPresets);
                }
            }
            catch
            {
                // keep empty list
            }
        }

        private ModSettings LoadCurrentOrFactoryWithoutResettingPresets()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return ModSettings.CreateFactoryDefaults();
                }

                Envelope envelope;
                using (var reader = new StreamReader(_filePath))
                {
                    var serializer = new XmlSerializer(typeof(Envelope));
                    envelope = serializer.Deserialize(reader) as Envelope;
                }

                if (envelope != null && envelope.Current != null)
                {
                    return envelope.Current;
                }
            }
            catch
            {
                // fall through
            }

            return ModSettings.CreateFactoryDefaults();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        /// <summary>Write if dirty and debounce elapsed (or force).</summary>
        public void FlushIfNeeded(ModSettings settings, bool force)
        {
            if (settings == null)
            {
                return;
            }

            if (!_dirty && !force)
            {
                return;
            }

            if (!force)
            {
                TimeSpan since = DateTime.UtcNow - _lastWriteUtc;
                if (since.TotalMilliseconds < 400)
                {
                    return;
                }
            }

            SaveNow(settings);
            _dirty = false;
        }

        public void SaveNow(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            EnsurePresetsLoaded();

            string dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (_userPresets == null)
            {
                _userPresets = new List<NamedPreset>();
            }

            var envelope = new Envelope
            {
                SchemaVersion = CurrentSchemaVersion,
                Current = settings,
                UserPresets = new List<NamedPreset>(_userPresets),
            };

            using (var writer = new StreamWriter(_filePath, false))
            {
                var serializer = new XmlSerializer(typeof(Envelope));
                serializer.Serialize(writer, envelope);
            }

            _lastWriteUtc = DateTime.UtcNow;
            _dirty = false;
        }

        [XmlRoot("TrackpadCameraControlSettings")]
        public class Envelope
        {
            public int SchemaVersion { get; set; }

            public ModSettings Current { get; set; }

            [XmlArray("UserPresets")]
            [XmlArrayItem("Preset")]
            public List<NamedPreset> UserPresets { get; set; }
        }

        public class NamedPreset
        {
            public string Name { get; set; }

            public ModSettings Settings { get; set; }
        }
    }
}
