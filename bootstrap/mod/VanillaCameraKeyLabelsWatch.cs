#if HAS_CITIES
using System;
using ColossalFramework.Globalization;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Notifies UI when vanilla camera keymapping or locale labels change.
    /// Driven by Harmony hooks on key save paths — not per-frame polling.
    /// </summary>
    internal static class VanillaCameraKeyLabelsWatch
    {
        public static event Action LabelsChanged;

        private static string _fingerprint;
        private static bool _localeHooked;

        public static void EnsureHooked()
        {
            if (_localeHooked)
            {
                return;
            }

            _localeHooked = true;
            _fingerprint = VanillaCameraKeyLabels.ComputeFingerprint();
            LocaleManager.eventLocaleChanged += OnLocaleChanged;
        }

        public static void Unhook()
        {
            if (!_localeHooked)
            {
                return;
            }

            _localeHooked = false;
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;
            _fingerprint = null;
        }

        /// <summary>Called from Harmony when keymapping bindings or locale labels change.</summary>
        public static void NotifyLabelsChangedFromGame()
        {
            EnsureHooked();
            string fp = VanillaCameraKeyLabels.ComputeFingerprint();
            if (_fingerprint != null && fp == _fingerprint)
            {
                return;
            }

            _fingerprint = fp;
            RaiseChanged();
        }

        private static void OnLocaleChanged()
        {
            NotifyLabelsChangedFromGame();
        }

        private static void RaiseChanged()
        {
            Action handler = LabelsChanged;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
#endif
