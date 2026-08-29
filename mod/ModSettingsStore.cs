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
        public const int CurrentSchemaVersion = 1;

        private readonly string _filePath;
        private DateTime _lastWriteUtc = DateTime.MinValue;
        private bool _dirty;

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
                    ModSettings recovered = ModSettings.CreateFactoryDefaults();
                    SaveNow(recovered);
                    return recovered;
                }

                return envelope.Current;
            }
            catch
            {
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

            string dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var envelope = new Envelope
            {
                SchemaVersion = CurrentSchemaVersion,
                Current = settings,
                UserPresets = new List<NamedPreset>(),
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
