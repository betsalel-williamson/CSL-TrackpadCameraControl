using System;

namespace TrackpadCameraControl.Rewrite
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

        public static void EnsureDirtyNewPreset(ModSettings settings, SettingsStore store)
        {
            if (settings == null)
            {
                return;
            }

            settings.ActiveFeelPresetName = NameNewPreset;
            if (store != null)
            {
                store.UpsertUserPresetInMemory(NameNewPreset, settings);
            }
        }

        public static void ApplyDefault(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            CopyFeelFields(settings, ModSettings.CreateFactoryDefaults());
        }

        public static void ApplySlow(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, SlowMultiplier);
        }

        public static void ApplyFast(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, FastMultiplier);
        }

        public static void CopyFeelFields(ModSettings dest, ModSettings source)
        {
            if (dest == null || source == null)
            {
                return;
            }

            dest.PanEnabled = source.PanEnabled;
            dest.ZoomEnabled = source.ZoomEnabled;
            dest.RotateEnabled = source.RotateEnabled;
            dest.OrbitEnabled = source.OrbitEnabled;

            dest.SignInvertPanX = source.SignInvertPanX;
            dest.SignInvertPanY = source.SignInvertPanY;
            dest.SignInvertOrbitYaw = source.SignInvertOrbitYaw;
            dest.SignInvertOrbitPitch = source.SignInvertOrbitPitch;
            dest.SignInvertZoom = source.SignInvertZoom;
            dest.SignInvertRotate = source.SignInvertRotate;

            dest.PanGainX = source.PanGainX;
            dest.PanGainY = source.PanGainY;
            dest.ZoomGain = source.ZoomGain;
            dest.RotateGain = source.RotateGain;
            dest.OrbitYawGain = source.OrbitYawGain;
            dest.OrbitPitchGain = source.OrbitPitchGain;

            dest.MotionDeadband = source.MotionDeadband;
            dest.PinchDeadband = source.PinchDeadband;
            dest.RotateDeadband = source.RotateDeadband;
        }

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

            settings.PanGainX = FeelMath.RoundGain(factory.PanGainX * multiplier);
            settings.PanGainY = FeelMath.RoundGain(factory.PanGainY * multiplier);
            settings.ZoomGain = FeelMath.RoundGain(factory.ZoomGain * multiplier);
            settings.RotateGain = FeelMath.RoundGain(factory.RotateGain * multiplier);
            settings.OrbitYawGain = FeelMath.RoundGain(factory.OrbitYawGain * multiplier);
            settings.OrbitPitchGain = FeelMath.RoundGain(factory.OrbitPitchGain * multiplier);

            settings.SignInvertPanX = factory.SignInvertPanX;
            settings.SignInvertPanY = factory.SignInvertPanY;
            settings.SignInvertOrbitYaw = factory.SignInvertOrbitYaw;
            settings.SignInvertOrbitPitch = factory.SignInvertOrbitPitch;
            settings.SignInvertZoom = factory.SignInvertZoom;
            settings.SignInvertRotate = factory.SignInvertRotate;
        }
    }
}
