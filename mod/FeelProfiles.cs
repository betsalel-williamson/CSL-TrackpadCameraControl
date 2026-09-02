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
        /// Every feel-field edit sets active identity to New Preset and upserts the scratch slot
        /// in <paramref name="store"/> (when non-null), including when already on New Preset.
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

        /// <summary>Factory Default sensitivities × 0.75 (RoundGain); reverse + pitch = factory.</summary>
        public static void ApplySlow(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, SlowMultiplier);
        }

        /// <summary>Factory Default sensitivities × 1.25 (RoundGain); reverse + pitch = factory.</summary>
        public static void ApplyFast(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, FastMultiplier);
        }

        /// <summary>
        /// Copy product-surface feel fields: enables, reverse, sensitivities, pitch limits, deadbands.
        /// Does <b>not</b> copy <see cref="ModSettings.GesturePreset"/>, orbit trigger, or per-op
        /// trackpad gesture bindings — those belong to gesture style, not feel.
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

            dest.SignInvertPanX = source.SignInvertPanX;
            dest.SignInvertPanY = source.SignInvertPanY;
            dest.SignInvertOrbitYaw = source.SignInvertOrbitYaw;
            dest.SignInvertOrbitPitch = source.SignInvertOrbitPitch;
            dest.SignInvertZoom = source.SignInvertZoom;
            dest.SignInvertYawRotate = source.SignInvertYawRotate;

            dest.PanGainX = source.PanGainX;
            dest.PanGainY = source.PanGainY;
            dest.ZoomGain = source.ZoomGain;
            dest.YawRotateGain = source.YawRotateGain;
            dest.OrbitYawGain = source.OrbitYawGain;
            dest.OrbitPitchGain = source.OrbitPitchGain;

            dest.OrbitPitchMin = source.OrbitPitchMin;
            dest.OrbitPitchMax = source.OrbitPitchMax;

            dest.MotionDeadband = source.MotionDeadband;
            dest.PinchDeadband = source.PinchDeadband;
            dest.YawDeadband = source.YawDeadband;
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

            settings.PanGainX = ModOptions.RoundGain(factory.PanGainX * multiplier);
            settings.PanGainY = ModOptions.RoundGain(factory.PanGainY * multiplier);
            settings.ZoomGain = ModOptions.RoundGain(factory.ZoomGain * multiplier);
            settings.YawRotateGain = ModOptions.RoundGain(factory.YawRotateGain * multiplier);
            settings.OrbitYawGain = ModOptions.RoundGain(factory.OrbitYawGain * multiplier);
            settings.OrbitPitchGain = ModOptions.RoundGain(factory.OrbitPitchGain * multiplier);

            settings.SignInvertPanX = factory.SignInvertPanX;
            settings.SignInvertPanY = factory.SignInvertPanY;
            settings.SignInvertOrbitYaw = factory.SignInvertOrbitYaw;
            settings.SignInvertOrbitPitch = factory.SignInvertOrbitPitch;
            settings.SignInvertZoom = factory.SignInvertZoom;
            settings.SignInvertYawRotate = factory.SignInvertYawRotate;

            settings.OrbitPitchMin = factory.OrbitPitchMin;
            settings.OrbitPitchMax = factory.OrbitPitchMax;
        }
    }
}
