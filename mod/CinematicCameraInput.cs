using System;
using System.Collections.Generic;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Vanilla <c>CinematicCameraController</c> aborts non-interactive playback on
    /// <c>Input.anyKey</c>, which includes ⌘ alone — so ⌘-Tab exits before the switch.
    /// </summary>
    public static class CinematicCameraInput
    {
        private static readonly HashSet<int> ModifierKeyCodes = BuildModifierKeyCodes();

#if HAS_CITIES
        private static readonly int[] UnityNonModifierKeyCodes = BuildUnityNonModifierKeyCodes();
#endif

        public static bool IsModifierKey(int keyCode)
        {
            return ModifierKeyCodes.Contains(keyCode);
        }

        /// <summary>
        /// When enabled, playback aborts only if a non-modifier key is down (Escape still aborts interactive mode separately).
        /// </summary>
        public static bool ShouldAbortNonInteractivePlayback(
            bool anyKey,
            bool shortcutPressed,
            Func<int, bool> isKeyDown,
            int[] nonModifierKeyCodes
        )
        {
            if (!anyKey || shortcutPressed || isKeyDown == null || nonModifierKeyCodes == null)
            {
                return false;
            }

            return HasNonModifierKeyDown(isKeyDown, nonModifierKeyCodes);
        }

        public static bool HasNonModifierKeyDown(
            Func<int, bool> isKeyDown,
            int[] nonModifierKeyCodes
        )
        {
            if (isKeyDown == null || nonModifierKeyCodes == null)
            {
                return false;
            }

            for (int i = 0; i < nonModifierKeyCodes.Length; i++)
            {
                if (isKeyDown(nonModifierKeyCodes[i]))
                {
                    return true;
                }
            }

            return false;
        }

#if HAS_CITIES
        public static bool ShouldAbortUnityPlayback(bool anyKey, bool shortcutPressed)
        {
            return ShouldAbortNonInteractivePlayback(
                anyKey,
                shortcutPressed,
                key => UnityEngine.Input.GetKey((UnityEngine.KeyCode)key),
                UnityNonModifierKeyCodes
            );
        }
#endif

        private static HashSet<int> BuildModifierKeyCodes()
        {
            var keys = new HashSet<int>();
#if HAS_CITIES
            AddModifierKeys(keys, typeof(UnityEngine.KeyCode));
#else
            // Unity KeyCode ints (stable on CS1 Mono) for unit tests without UnityEngine.
            int[] modifiers = { 306, 305, 304, 303, 308, 307, 310, 309 };
            for (int i = 0; i < modifiers.Length; i++)
            {
                keys.Add(modifiers[i]);
            }
#endif
            return keys;
        }

#if HAS_CITIES
        private static int[] BuildUnityNonModifierKeyCodes()
        {
            var keys = new List<int>();
            Array values = Enum.GetValues(typeof(UnityEngine.KeyCode));
            for (int i = 0; i < values.Length; i++)
            {
                int code = (int)values.GetValue(i);
                if (code == (int)UnityEngine.KeyCode.None || IsModifierKey(code))
                {
                    continue;
                }

                keys.Add(code);
            }

            return keys.ToArray();
        }

        private static void AddModifierKeys(HashSet<int> keys, Type keyCodeType)
        {
            AddKey(keys, keyCodeType, "LeftControl");
            AddKey(keys, keyCodeType, "RightControl");
            AddKey(keys, keyCodeType, "LeftShift");
            AddKey(keys, keyCodeType, "RightShift");
            AddKey(keys, keyCodeType, "LeftAlt");
            AddKey(keys, keyCodeType, "RightAlt");
            AddKey(keys, keyCodeType, "LeftCommand");
            AddKey(keys, keyCodeType, "RightCommand");
            AddKey(keys, keyCodeType, "LeftApple");
            AddKey(keys, keyCodeType, "RightApple");
        }

        private static void AddKey(HashSet<int> keys, Type keyCodeType, string name)
        {
            try
            {
                keys.Add((int)Enum.Parse(keyCodeType, name, false));
            }
            catch
            {
                // ignore missing names on other Unity versions
            }
        }
#endif
    }
}
