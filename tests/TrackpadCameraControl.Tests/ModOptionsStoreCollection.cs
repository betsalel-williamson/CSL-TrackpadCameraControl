using Xunit;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Serializes tests that mutate static <see cref="TrackpadCameraControl.ModOptions.Store"/>
    /// (and related Mod.Settings) so parallel xUnit runs do not race.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ModOptionsStoreCollection
    {
        public const string Name = "ModOptionsStore";
    }
}
