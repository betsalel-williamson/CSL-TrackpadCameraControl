using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Versioned XML settings envelope (schema v1). Injectable path for tests.</summary>
    public class SettingsStore
    {
        public const int CurrentSchemaVersion = 1;

        private readonly string _filePath;
        private DateTime _lastWriteUtc = DateTime.MinValue;
        private bool _dirty;
        private List<NamedPreset> _userPresets = new List<NamedPreset>();
        private bool _presetsHydrated;

        public SettingsStore(string filePath)
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

        public bool SaveUserPreset(string name, ModSettings feelSnapshot, ModSettings current)
        {
            if (!UpsertUserPresetInMemory(name, feelSnapshot))
            {
                return false;
            }

            ModSettings blob = current ?? feelSnapshot;
            SaveEnvelope(blob);
            return true;
        }

        /// <summary>
        /// Upsert a named feel snapshot in memory only — does not write the envelope.
        /// Used by dirty→New Preset so ApplyGain owns the single coalesced flush.
        /// </summary>
        public bool UpsertUserPresetInMemory(string name, ModSettings feelSnapshot)
        {
            if (string.IsNullOrEmpty(name) || feelSnapshot == null)
            {
                return false;
            }

            EnsurePresetsLoaded();
            ModSettings snap = FeelProfiles.SnapshotFeel(feelSnapshot);
            bool found = false;
            for (int i = 0; i < _userPresets.Count; i++)
            {
                if (
                    _userPresets[i] != null
                    && string.Equals(_userPresets[i].Name, name, StringComparison.Ordinal)
                )
                {
                    _userPresets[i].Feel = snap;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _userPresets.Add(new NamedPreset { Name = name, Feel = snap });
            }

            return true;
        }

        public bool TryLoadUserPreset(string name, ModSettings dest)
        {
            if (string.IsNullOrEmpty(name) || dest == null)
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
                    && preset.Feel != null
                )
                {
                    FeelProfiles.CopyFeelFields(dest, preset.Feel);
                    dest.ActiveFeelPresetName = name;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteUserPreset(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            EnsurePresetsLoaded();
            for (int i = _userPresets.Count - 1; i >= 0; i--)
            {
                if (
                    _userPresets[i] != null
                    && string.Equals(_userPresets[i].Name, name, StringComparison.Ordinal)
                )
                {
                    _userPresets.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void MarkDirtyAndMaybeFlush(ModSettings settings)
        {
            _dirty = true;
            if ((DateTime.UtcNow - _lastWriteUtc).TotalMilliseconds < 400)
            {
                return;
            }

            SaveNow(settings);
        }

        public bool HasPendingDirty => _dirty;

        public void SaveNow(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            SaveEnvelope(settings);
            _dirty = false;
            _lastWriteUtc = DateTime.UtcNow;
        }

        private void SaveEnvelope(ModSettings settings)
        {
            EnsureDirectory();
            var envelope = new SettingsEnvelope
            {
                SchemaVersion = CurrentSchemaVersion,
                Settings = settings,
                UserPresets = _userPresets.ToArray(),
            };

            var serializer = new XmlSerializer(typeof(SettingsEnvelope));
            using (var writer = new StreamWriter(_filePath))
            {
                serializer.Serialize(writer, envelope);
            }
        }

        private void EnsureDirectory()
        {
            string dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private void EnsurePresetsLoaded()
        {
            if (_presetsHydrated)
            {
                return;
            }

            if (File.Exists(_filePath) && TryLoadCurrent(out _, out List<NamedPreset> presets))
            {
                _userPresets = presets;
            }
            else
            {
                _userPresets = new List<NamedPreset>();
            }

            _presetsHydrated = true;
        }

        private static int PeekSchemaVersion(string path)
        {
            try
            {
                using (var reader = XmlReader.Create(path))
                {
                    while (reader.Read())
                    {
                        if (
                            reader.NodeType == XmlNodeType.Element
                            && reader.Name == "SchemaVersion"
                        )
                        {
                            string text = reader.ReadElementContentAsString();
                            if (int.TryParse(text, out int v))
                            {
                                return v;
                            }

                            return -1;
                        }
                    }
                }
            }
            catch
            {
                return -1;
            }

            return -1;
        }

        private bool TryLoadCurrent(out ModSettings settings, out List<NamedPreset> presets)
        {
            settings = null;
            presets = new List<NamedPreset>();
            try
            {
                var serializer = new XmlSerializer(typeof(SettingsEnvelope));
                using (var reader = new StreamReader(_filePath))
                {
                    var envelope = serializer.Deserialize(reader) as SettingsEnvelope;
                    if (envelope == null || envelope.Settings == null)
                    {
                        return false;
                    }

                    settings = envelope.Settings;
                    if (envelope.UserPresets != null)
                    {
                        for (int i = 0; i < envelope.UserPresets.Length; i++)
                        {
                            if (envelope.UserPresets[i] != null)
                            {
                                presets.Add(envelope.UserPresets[i]);
                            }
                        }
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public class NamedPreset
        {
            public string Name { get; set; }
            public ModSettings Feel { get; set; }
        }

        public class SettingsEnvelope
        {
            public int SchemaVersion { get; set; }
            public ModSettings Settings { get; set; }
            public NamedPreset[] UserPresets { get; set; }
        }
    }
}
