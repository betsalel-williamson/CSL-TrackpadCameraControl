using System;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Built-in Slow / Default / Fast feel profiles and named feel field copy.
    /// Slow/Fast always multiply factory Default sensitivities — never dirty live values.
    /// </summary>
    public static class FeelProfiles
    {
        public const float SlowMultiplier = 0.75f;
        public const float FastMultiplier = 1.25f;

        public const string NameSlow = "Slow";
        public const string NameDefault = "Default";
        public const string NameFast = "Fast";
        public const string NameNewPreset = "New Preset";

        /// <summary>Built-in Slow / Default / Fast — never overwritten by Save as… or dirty autosave.</summary>
        public static bool IsBuiltInName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return string.Equals(name, NameSlow, StringComparison.Ordinal)
                || string.Equals(name, NameDefault, StringComparison.Ordinal)
                || string.Equals(name, NameFast, StringComparison.Ordinal);
        }

        /// <summary>
        /// Any feel-field edit while not already on New Preset switches identity to New Preset
        /// and upserts the scratch slot in <paramref name="store"/> (when non-null).
        /// Further edits while on New Preset keep autosaving into that slot.
        /// </summary>
        public static void EnsureDirtyNewPreset(ModSettings settings, ModSettingsStore store)
        {
            if (settings == null)
            {
                return;
            }

            settings.ActiveFeelPresetName = NameNewPreset;
            if (store != null)
            {
                store.SaveUserPreset(NameNewPreset, settings, settings);
            }
        }

        /// <summary>Restore factory Default feel fields onto <paramref name="settings"/>.</summary>
        public static void ApplyDefault(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            CopyFeelFields(settings, ModSettings.CreateFactoryDefaults());
        }

        /// <summary>Factory Default sensitivities × 0.75 (RoundSensitivity); reverse + pitch = factory.</summary>
        public static void ApplySlow(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, SlowMultiplier);
        }

        /// <summary>Factory Default sensitivities × 1.25 (RoundSensitivity); reverse + pitch = factory.</summary>
        public static void ApplyFast(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, FastMultiplier);
        }

        /// <summary>
        /// Copy product-surface feel fields: enables, reverse, sensitivities, pitch limits.
        /// </summary>
        public static void CopyFeelFields(ModSettings dest, ModSettings source)
        {
            if (dest == null || source == null)
            {
                return;
            }

            dest.PanEnabled = source.PanEnabled;
            dest.ZoomEnabled = source.ZoomEnabled;
            dest.YawEnabled = source.YawEnabled;
            dest.OrbitEnabled = source.OrbitEnabled;

            dest.InvertPanX = source.InvertPanX;
            dest.InvertPanY = source.InvertPanY;
            dest.InvertOrbitYaw = source.InvertOrbitYaw;
            dest.InvertOrbitPitch = source.InvertOrbitPitch;
            dest.InvertZoom = source.InvertZoom;
            dest.InvertYawRotate = source.InvertYawRotate;

            dest.PanSensitivityX = source.PanSensitivityX;
            dest.PanSensitivityY = source.PanSensitivityY;
            dest.ZoomSensitivity = source.ZoomSensitivity;
            dest.YawRotateSensitivity = source.YawRotateSensitivity;
            dest.OrbitYawSensitivity = source.OrbitYawSensitivity;
            dest.OrbitPitchSensitivity = source.OrbitPitchSensitivity;

            dest.OrbitPitchMin = source.OrbitPitchMin;
            dest.OrbitPitchMax = source.OrbitPitchMax;
        }

        /// <summary>Snapshot feel fields into a new ModSettings instance.</summary>
        public static ModSettings SnapshotFeel(ModSettings source)
        {
            var snap = new ModSettings();
            if (source != null)
            {
                CopyFeelFields(snap, source);
            }

            return snap;
        }

        private static void ApplyScaledFromFactory(ModSettings settings, float multiplier)
        {
            if (settings == null)
            {
                return;
            }

            ModSettings factory = ModSettings.CreateFactoryDefaults();

            settings.PanSensitivityX = ModOptions.RoundSensitivity(factory.PanSensitivityX * multiplier);
            settings.PanSensitivityY = ModOptions.RoundSensitivity(factory.PanSensitivityY * multiplier);
            settings.ZoomSensitivity = ModOptions.RoundSensitivity(factory.ZoomSensitivity * multiplier);
            settings.YawRotateSensitivity = ModOptions.RoundSensitivity(
                factory.YawRotateSensitivity * multiplier
            );
            settings.OrbitYawSensitivity = ModOptions.RoundSensitivity(
                factory.OrbitYawSensitivity * multiplier
            );
            settings.OrbitPitchSensitivity = ModOptions.RoundSensitivity(
                factory.OrbitPitchSensitivity * multiplier
            );

            settings.InvertPanX = factory.InvertPanX;
            settings.InvertPanY = factory.InvertPanY;
            settings.InvertOrbitYaw = factory.InvertOrbitYaw;
            settings.InvertOrbitPitch = factory.InvertOrbitPitch;
            settings.InvertZoom = factory.InvertZoom;
            settings.InvertYawRotate = factory.InvertYawRotate;

            settings.OrbitPitchMin = factory.OrbitPitchMin;
            settings.OrbitPitchMax = factory.OrbitPitchMax;
        }
    }
}
