#if HAS_CITIES
using System;
using HarmonyLib;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Two Harmony patches: scroll suppress + orbit flush. No capture/UI logic.</summary>
    public static class Patcher
    {
        public const string HarmonyId = "com.betsalel.trackpadcameracontrol.rewrite";

        private static bool patched;
        private static bool harmonyMissingLogged;

        public static void PatchAll()
        {
            if (patched)
            {
                return;
            }

            try
            {
                Harmony harmony = new Harmony(HarmonyId);
                // Only the two patch classes in this assembly (scroll suppress + orbit flush).
                harmony.PatchAll(typeof(Patcher).Assembly);
                patched = true;
            }
            catch (Exception ex)
            {
                LogHarmonyMissingOnce("patch failed: " + ex.Message);
            }
        }

        public static void UnpatchAll()
        {
            if (!patched)
            {
                return;
            }

            try
            {
                Harmony harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
            }
            catch
            {
                // ignore
            }

            patched = false;
        }

        public static void LogHarmonyMissingOnce()
        {
            LogHarmonyMissingOnce(null);
        }

        public static void LogHarmonyMissingOnce(string detail)
        {
            if (harmonyMissingLogged)
            {
                return;
            }

            harmonyMissingLogged = true;
            string line =
                "TrackpadCameraControl.Rewrite: Cities Harmony is not ready. Two-finger pan may still fight vanilla scroll-zoom. Subscribe to Cities Harmony and re-enable this mod.";
            if (!string.IsNullOrEmpty(detail))
            {
                line = line + " (" + detail + ")";
            }

            try
            {
                Debug.LogWarning(line);
            }
            catch
            {
                // ignore
            }
        }
    }

    [HarmonyPatch(typeof(CameraController), "HandleScrollWheelEvent")]
    internal static class HandleScrollWheelEventPatch
    {
        public static bool Prefix()
        {
            return InputGates.ShouldRunVanillaScrollWheel();
        }
    }

    [HarmonyPatch(typeof(CameraController), "HandleMouseEvents")]
    internal static class HandleMouseEventsPatch
    {
        public static bool Prefix()
        {
            return InputGates.ShouldRunVanillaMouseEvents();
        }

        public static void Postfix()
        {
            if (!InputGates.ShouldFlushPendingOrbit())
            {
                return;
            }

            try
            {
                ICameraController camera = Mod.Runtime?.Pipeline?.Camera;
                if (camera == null)
                {
                    return;
                }

                camera.FlushPendingAngleVelocity(Time.deltaTime);
            }
            catch
            {
                // Fail soft every frame.
            }
        }
    }
}
#endif
