using Xunit;

namespace TrackpadCameraControl.Tests
{
    /// <summary>
    /// Serializes tests that mutate static <see cref="TrackpadCameraControl.VanillaCameraSuppress"/>
    /// (and related InputGates hooks) so parallel xUnit runs do not race.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class VanillaCameraSuppressCollection
    {
        public const string Name = "VanillaCameraSuppress";
    }
}
