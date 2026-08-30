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

        /// <summary>Restore factory Default feel fields onto <paramref name="settings"/>.</summary>
        public static void ApplyDefault(ModSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            CopyFeelFields(settings, ModSettings.CreateFactoryDefaults());
        }

        /// <summary>Factory Default sensitivities × 0.75 (Round2); reverse + pitch = factory.</summary>
        public static void ApplySlow(ModSettings settings)
        {
            ApplyScaledFromFactory(settings, SlowMultiplier);
        }

        /// <summary>Factory Default sensitivities × 1.25 (Round2); reverse + pitch = factory.</summary>
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

            settings.PanSensitivityX = ModOptions.Round2(factory.PanSensitivityX * multiplier);
            settings.PanSensitivityY = ModOptions.Round2(factory.PanSensitivityY * multiplier);
            settings.ZoomSensitivity = ModOptions.Round2(factory.ZoomSensitivity * multiplier);
            settings.YawRotateSensitivity = ModOptions.Round2(
                factory.YawRotateSensitivity * multiplier
            );
            settings.OrbitYawSensitivity = ModOptions.Round2(
                factory.OrbitYawSensitivity * multiplier
            );
            settings.OrbitPitchSensitivity = ModOptions.Round2(
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
