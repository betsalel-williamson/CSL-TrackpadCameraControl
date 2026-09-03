using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Versioned XML settings envelope. Injectable path for tests.
    /// </summary>
    public sealed class ModSettingsStore
    {
        /// <summary>
        /// Schema 9 renames Rotate feel fields (YawDeadband/YawRotate* → RotateDeadband/RotateGain/…).
        /// Schema 8 renames Rotate op gesture bindings (YawGesture* → RotateGesture*); yaw/pitch stay Orbit axes.
        /// Schema 7 persists per-op trackpad gesture bindings (Zoom/Pan/Rotate/Orbit gesture + modifier).
        /// Schema 6 renames pinch/rotate activation thresholds to PinchDeadband / YawDeadband (was PinchEpsilon / RotateEpsilon).
        /// Schema 5 adds persisted Debug panel position (DebugPanelPosX/Y).
        /// Schema 4 adds Debug QoL prefs (IncludeSystemInfoInCopy, DebugPanelDismissed).
        /// Schema 3 persists control-systems field names (gain, step, deadband, filter, sign invert).
        /// Schema 2 used Sensitivity / ButtonScale / Deadzone / LowPass / Invert element names.
        /// Schema 1 also used pre-scaled AppKit scroll (migrate ×0.01 into pan/orbit gain).
        /// </summary>
        public const int CurrentSchemaVersion = 9;

        /// <summary>
        /// Former AppleGestureMapper.ScrollToCentroid scale. Schema 1 used pre-scaled scroll
        /// and larger gains; schema 2+ folds this into pan/orbit gain on load.
        /// </summary>
        internal const float V1ScrollUnit = 0.01f;

        /// <summary>First schema version that writes engineering XML element names.</summary>
        private const int EngineeringNamesSchemaVersion = 3;

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
                ModSettings current;
                List<NamedPreset> presets;
                if (fileSchema < EngineeringNamesSchemaVersion)
                {
                    if (!TryLoadLegacy(out current, out presets))
                    {
                        _userPresets = new List<NamedPreset>();
                        _presetsHydrated = true;
                        ModSettings recovered = ModSettings.CreateFactoryDefaults();
                        SaveNow(recovered);
                        return recovered;
                    }

                    if (fileSchema < 2)
                    {
                        MigrateScrollUnitIntoGain(current);
                        for (int i = 0; i < presets.Count; i++)
                        {
                            NamedPreset preset = presets[i];
                            if (preset != null && preset.Settings != null)
                            {
                                MigrateScrollUnitIntoGain(preset.Settings);
                            }
                        }
                    }
                }
                else if (!TryLoadCurrent(out current, out presets))
                {
                    _userPresets = new List<NamedPreset>();
                    _presetsHydrated = true;
                    ModSettings recovered = ModSettings.CreateFactoryDefaults();
                    SaveNow(recovered);
                    return recovered;
                }

                _userPresets = presets;
                _presetsHydrated = true;

                EnsureMapsPlusRotateBinding(current);

                if (fileSchema < CurrentSchemaVersion)
                {
                    try
                    {
                        SaveNow(current);
                    }
                    catch
                    {
                        // keep migrated in-memory even if rewrite fails
                    }
                }

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

        /// <summary>
        /// Schema 8 alias getters used to return <c>None</c>; a wiped Rotate binding on Maps+
        /// reloads as "Gesture(s): none". Re-seed Maps+ defaults when that happens.
        /// </summary>
        internal static void EnsureMapsPlusRotateBinding(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (
                settings.GesturePreset == GesturePreset.MapsPlus
                && settings.RotateGesture == TrackpadGesture.None
            )
            {
                TrackpadGestureCatalog.ApplyMapsPlusDefaults(settings);
                settings.StyleTable = MapsPlusSeed.CreateTable();
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

        /// <summary>
        /// Fold former mapper ScrollToCentroid (0.01) into gain / motion deadband so
        /// raw AppKit scroll deltas keep the same feel (schema 1 → 2).
        /// </summary>
        internal static void MigrateScrollUnitIntoGain(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.PanGainX *= V1ScrollUnit;
            settings.PanGainY *= V1ScrollUnit;
            settings.OrbitYawGain *= V1ScrollUnit;
            settings.OrbitPitchGain *= V1ScrollUnit;
            if (settings.MotionDeadband > 0f)
            {
                settings.MotionDeadband /= V1ScrollUnit;
            }
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

        private bool TryLoadLegacy(out ModSettings current, out List<NamedPreset> presets)
        {
            current = null;
            presets = new List<NamedPreset>();
            try
            {
                LegacySettingsEnvelope legacy;
                using (var reader = new StreamReader(_filePath))
                {
                    var serializer = new XmlSerializer(typeof(LegacySettingsEnvelope));
                    legacy = serializer.Deserialize(reader) as LegacySettingsEnvelope;
                }

                if (legacy == null || legacy.Current == null)
                {
                    return false;
                }

                current = legacy.Current.ToModSettings();
                if (legacy.UserPresets != null)
                {
                    for (int i = 0; i < legacy.UserPresets.Count; i++)
                    {
                        LegacyNamedPreset lp = legacy.UserPresets[i];
                        if (lp == null)
                        {
                            continue;
                        }

                        presets.Add(
                            new NamedPreset
                            {
                                Name = lp.Name,
                                Settings = lp.Settings != null ? lp.Settings.ToModSettings() : null,
                            }
                        );
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
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
                int fileSchema = PeekSchemaVersion(_filePath);
                ModSettings unused;
                List<NamedPreset> presets;
                bool ok =
                    fileSchema < EngineeringNamesSchemaVersion
                        ? TryLoadLegacy(out unused, out presets)
                        : TryLoadCurrent(out unused, out presets);
                if (ok)
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

                int fileSchema = PeekSchemaVersion(_filePath);
                ModSettings current;
                List<NamedPreset> unusedPresets;
                bool ok =
                    fileSchema < EngineeringNamesSchemaVersion
                        ? TryLoadLegacy(out current, out unusedPresets)
                        : TryLoadCurrent(out current, out unusedPresets);
                if (ok && current != null)
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
