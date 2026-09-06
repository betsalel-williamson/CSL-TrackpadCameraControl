using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Versioned XML settings envelope (schema v1). Injectable path for tests.
    /// </summary>
    public sealed class ModSettingsStore
    {
        public const int CurrentSchemaVersion = 1;

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
                Path.Combine(Path.Combine(root, "Colossal Order"), "Cities_Skylines"),
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

                int fileSchema = PeekSchemaVersion(_filePath);
                if (
                    fileSchema != CurrentSchemaVersion
                    || !TryLoadCurrent(out ModSettings current, out List<NamedPreset> presets)
                )
                {
                    _userPresets = new List<NamedPreset>();
                    _presetsHydrated = true;
                    ModSettings recovered = ModSettings.CreateFactoryDefaults();
                    SaveNow(recovered);
                    return recovered;
                }

                _userPresets = presets;
                _presetsHydrated = true;
                current.StyleTable = MapsPlusSeed.CreateTable();
                current.ApplyGesturePreset(GesturePreset.MapsPlus);
                return current;
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
                if (preset != null && string.Equals(preset.Name, name, StringComparison.Ordinal))
                {
                    _userPresets.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        internal static int PeekSchemaVersion(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return 0;
            }

            try
            {
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    while (reader.Read())
                    {
                        if (
                            reader.NodeType == XmlNodeType.Element
                            && reader.Name == "SchemaVersion"
                        )
                        {
                            return reader.ReadElementContentAsInt();
                        }
                    }
                }
            }
            catch
            {
                // fall through
            }

            return 0;
        }

        private bool TryLoadCurrent(out ModSettings current, out List<NamedPreset> presets)
        {
            current = null;
            presets = new List<NamedPreset>();
            try
            {
                Envelope envelope;
                using (var reader = new StreamReader(_filePath))
                {
                    var serializer = new XmlSerializer(typeof(Envelope));
                    envelope = serializer.Deserialize(reader) as Envelope;
                }

                if (envelope == null || envelope.Current == null)
                {
                    return false;
                }

                current = envelope.Current;
                if (envelope.UserPresets != null)
                {
                    presets = new List<NamedPreset>(envelope.UserPresets);
                }

                return true;
            }
            catch
            {
                return false;
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
                if (
                    PeekSchemaVersion(_filePath) == CurrentSchemaVersion
                    && TryLoadCurrent(out ModSettings unused, out List<NamedPreset> presets)
                )
                {
                    _userPresets = presets;
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

                if (
                    PeekSchemaVersion(_filePath) == CurrentSchemaVersion
                    && TryLoadCurrent(out ModSettings current, out List<NamedPreset> unusedPresets)
                    && current != null
                )
                {
                    return current;
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
