using System;
using System.Runtime.InteropServices;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    internal static class PlatformTestFacts
    {
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    /// <summary>Runs only on macOS; skipped elsewhere (IOKit / hw.model probes).</summary>
    internal sealed class MacOsFactAttribute : FactAttribute
    {
        public MacOsFactAttribute()
        {
            if (!PlatformTestFacts.IsMacOS)
            {
                Skip = "macOS only";
            }
        }
    }

    /// <summary>Runs only off macOS; skipped on Darwin (non-Mac fallback paths).</summary>
    internal sealed class SkipOnMacOsFactAttribute : FactAttribute
    {
        public SkipOnMacOsFactAttribute()
        {
            if (PlatformTestFacts.IsMacOS)
            {
                Skip = "Not applicable on macOS";
            }
        }
    }
}
