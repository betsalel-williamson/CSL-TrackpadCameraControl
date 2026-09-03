#if HAS_CITIES
using System;
using System.Reflection;
using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
{
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

            try
            {
                string tmp = Environment.GetEnvironmentVariable("TMPDIR");
                if (string.IsNullOrEmpty(tmp))
                {
                    tmp = System.IO.Path.GetTempPath();
                }

                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(tmp, "trackpad-camera-control-rewrite-mod.log"),
                    line + Environment.NewLine
                );
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
        private static FieldInfo rotateKeyField;

        public static bool Prefix(CameraController __instance)
        {
            return InputGates.ShouldRunVanillaMouseEvents(IsCameraMouseRotateHeld(__instance));
        }

        /// <summary>
        /// After vanilla damp (and optional mouse seed), flush trackpad orbit pending into
        /// m_angleVelocity before UpdateTargetPosition integrates. Runs even when Prefix skips
        /// the original (rotate binding held).
        /// </summary>
        public static void Postfix()
        {
            if (!InputGates.ShouldFlushPendingOrbit())
            {
                return;
            }

            try
            {
                ICameraController camera =
                    Mod.Runtime?.Pipeline != null ? Mod.Runtime.Pipeline.Camera : null;
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

        private static bool IsCameraMouseRotateHeld(CameraController instance)
        {
            try
            {
                if (rotateKeyField == null)
                {
                    rotateKeyField = typeof(CameraController).GetField(
                        "m_cameraMouseRotate",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (rotateKeyField == null)
                {
                    return false;
                }

                SavedInputKey key = rotateKeyField.GetValue(instance) as SavedInputKey;
                return key != null && key.IsPressed();
            }
            catch
            {
                return false;
            }
        }
    }

    [HarmonyPatch]
    internal static class OptionsKeymappingPanelRefreshPatch
    {
        private static MethodBase TargetMethod()
        {
            Type panelType = AccessTools.TypeByName("OptionsKeymappingPanel");
            return panelType == null ? null : AccessTools.Method(panelType, "RefreshKeyMapping");
        }

        public static void Postfix()
        {
            VanillaCameraKeyLabelsWatch.NotifyLabelsChangedFromGame();
        }
    }

    [HarmonyPatch]
    internal static class OptionsKeymappingPanelResetPatch
    {
        private static MethodBase TargetMethod()
        {
            Type panelType = AccessTools.TypeByName("OptionsKeymappingPanel");
            return panelType == null ? null : AccessTools.Method(panelType, "ResetKeyMapping");
        }

        public static void Postfix()
        {
            VanillaCameraKeyLabelsWatch.NotifyLabelsChangedFromGame();
        }
    }

    [HarmonyPatch]
    internal static class OptionsKeymappingPanelClearPatch
    {
        private static MethodBase TargetMethod()
        {
            Type panelType = AccessTools.TypeByName("OptionsKeymappingPanel");
            return panelType == null ? null : AccessTools.Method(panelType, "OnClearKeyMapping");
        }

        public static void Postfix()
        {
            VanillaCameraKeyLabelsWatch.NotifyLabelsChangedFromGame();
        }
    }

    [HarmonyPatch(typeof(SavedInputKey), "value", MethodType.Setter)]
    internal static class SavedInputKeyValueSetterPatch
    {
        public static void Postfix(SavedInputKey __instance)
        {
            if (!VanillaCameraKeyLabels.IsWatchedCameraKey(__instance))
            {
                return;
            }

            VanillaCameraKeyLabelsWatch.NotifyLabelsChangedFromGame();
        }
    }
}
#endif
